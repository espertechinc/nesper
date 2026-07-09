#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Run NEsper benchmarks and collect all outputs in a timestamped directory.
.DESCRIPTION
    Runs one or more benchmark suites and saves every artifact -- BenchmarkDotNet
    Markdown/HTML/CSV results, Perforator console output -- under a single
    timestamped output directory together with a manifest.json capturing git state
    and machine info.  Suitable for CI, developer laptops, and cross-run comparison.

    Suites
    ------
    micro       BenchmarkDotNet microbenchmarks (locks, filter index, scheduling, ...)
    e2e         BenchmarkDotNet end-to-end throughput benchmarks (compile->deploy->send)
    perforator  Tight-loop profiling harness (~2 min, 6 EPL statements x 20 s each)
    all         All three suites

.PARAMETER Suite
    Comma-separated list of suites to run.  Use "all" for every suite.
    Default: "micro,e2e"

.PARAMETER Filter
    BenchmarkDotNet filter pattern applied to micro and e2e suites.
    Supports wildcards, e.g. "*Window*" or "*Filter*".
    Default: "*"  (runs all benchmarks non-interactively)

.PARAMETER Framework
    Target .NET runtime for the e2e suite (sets NESPER_BENCH_RUNTIMES, read by
    EndToEndJobsAttribute) and the BDN host process.  The micro suite defines its
    runtimes via [SimpleJob] attributes and always runs both.
    Accepted values: net8.0 | net9.0 | both
    Default: "both"

.PARAMETER GC
    GC mode for the e2e suite (sets NESPER_BENCH_GC, read by EndToEndJobsAttribute).
    Production deployments typically run Server GC; use "server" or "both" before
    publishing numbers externally.  The micro suite is unaffected.
    Accepted values: workstation | server | both
    Default: "workstation"  (matches the committed baseline in PERF-BENCHMARKS.md)

.PARAMETER OutputDir
    Override the output directory.
    Default: benchmark-results\<yyyy-MM-ddTHHmmss>

.PARAMETER Summary
    Show configuration without running any benchmarks.

.PARAMETER DryRun
    List available benchmarks (--list flat) for each BDN suite without measuring.

.EXAMPLE
    .\run-benchmarks.ps1
    Run all micro + e2e benchmarks on both runtimes; results in benchmark-results\<ts>\

.EXAMPLE
    .\run-benchmarks.ps1 -Suite micro -Filter "*Lock*"
    Run only lock-related microbenchmarks.

.EXAMPLE
    .\run-benchmarks.ps1 -Suite e2e -Framework net9.0 -Filter "*Window*"
    Run window benchmarks on net9.0 only.

.EXAMPLE
    .\run-benchmarks.ps1 -Suite all
    Run micro + e2e + perforator.

.EXAMPLE
    .\run-benchmarks.ps1 -Suite e2e -GC server
    Run end-to-end benchmarks under Server GC (production-representative numbers).

.EXAMPLE
    .\run-benchmarks.ps1 -Suite e2e -GC both -Filter "*Contended*"
    Compare Workstation vs Server GC side by side for the contention benchmark.

.EXAMPLE
    .\run-benchmarks.ps1 -DryRun
    List all available benchmark methods without measuring.

.EXAMPLE
    .\run-benchmarks.ps1 -Summary
    Show what would run, then exit.
#>

param(
    [string]$Suite     = "micro,e2e",
    [string]$Filter    = "*",
    [string]$Framework = "both",
    [string]$GC        = "workstation",
    [string]$OutputDir = "",
    [switch]$Summary,
    [switch]$DryRun
)

Set-StrictMode -Version Latest

# --- Colour helpers -----------------------------------------------------------

function Write-Header([string]$msg) {
    Write-Host ""
    Write-Host ("-" * 64) -ForegroundColor Cyan
    Write-Host "  $msg" -ForegroundColor Cyan
    Write-Host ("-" * 64) -ForegroundColor Cyan
    Write-Host ""
}

function Write-Ok([string]$msg)   { Write-Host "  [OK]   $msg" -ForegroundColor Green  }
function Write-Fail([string]$msg) { Write-Host "  [FAIL] $msg" -ForegroundColor Red    }
function Write-Info([string]$msg) { Write-Host "  $msg"        -ForegroundColor White  }
function Write-Warn([string]$msg) { Write-Host "  [WARN] $msg" -ForegroundColor Yellow }

# --- Validate -Suite ----------------------------------------------------------

$validSuites = @("micro", "e2e", "perforator")

$requestedSuites = if ($Suite -ieq "all") {
    $validSuites
} else {
    $Suite -split "," | ForEach-Object { $_.Trim().ToLower() }
}

foreach ($s in $requestedSuites) {
    if ($s -notin $validSuites) {
        Write-Host "ERROR: Unknown suite '$s'.  Valid values: micro, e2e, perforator, all" -ForegroundColor Red
        exit 1
    }
}

# --- Validate -Framework ------------------------------------------------------

$validFrameworks = @("net8.0", "net9.0", "both")
if ($Framework -notin $validFrameworks) {
    Write-Host "ERROR: Invalid -Framework '$Framework'.  Use: net8.0, net9.0, or both" -ForegroundColor Red
    exit 1
}

# --- Validate -GC ---------------------------------------------------------------

$validGcModes = @("workstation", "server", "both")
$GC = $GC.Trim().ToLower()
if ($GC -notin $validGcModes) {
    Write-Host "ERROR: Invalid -GC '$GC'.  Use: workstation, server, or both" -ForegroundColor Red
    exit 1
}
# Read by EndToEndJobsAttribute when the e2e suite composes its BDN job matrix.
$env:NESPER_BENCH_GC = $GC

# Host framework for "dotnet run --framework" (required when project multi-targets).
# When running both runtimes, net9.0 hosts the BDN process; BDN spawns child
# processes per job-defined runtime internally.
$hostFramework = if ($Framework -eq "both") { "net9.0" } else { $Framework }

# Runtime selection for the e2e suite is job-defined (EndToEndJobsAttribute) and
# controlled via this env var.  Do NOT pass BDN --runtimes: it does not filter
# job-defined runtimes, it ADDS an extra default-configured job that pollutes
# the results table.
$env:NESPER_BENCH_RUNTIMES = $Framework

# The micro suite defines its runtimes via [SimpleJob] attributes (always both).
if ($Framework -ne "both" -and "micro" -in $requestedSuites) {
    Write-Warn "-Framework '$Framework' applies to the e2e suite; micro always runs net8.0 + net9.0 (attribute-defined)."
}

# --- Project paths (relative to repo root) ------------------------------------

$MicroProject      = "tst\NEsper.Benchmarks.Micro\NEsper.Benchmarks.Micro.csproj"
$E2EProject        = "tst\NEsper.Benchmarks.EndToEnd\NEsper.Benchmarks.EndToEnd.csproj"
$PerforatorProject = "examples\NEsper.Benchmark\NEsper.Benchmark.Perforator\NEsper.Benchmark.Perforator.csproj"

# --- Verify we are at the repo root -------------------------------------------

foreach ($proj in @($MicroProject, $E2EProject, $PerforatorProject)) {
    if (-not (Test-Path $proj)) {
        Write-Host "ERROR: Project not found: $proj" -ForegroundColor Red
        Write-Host "       Run this script from the repository root." -ForegroundColor Red
        exit 1
    }
}

# --- Output directory ---------------------------------------------------------

$timestamp = Get-Date -Format "yyyy-MM-ddTHHmmss"
# Use Join-Path + Get-Location (PowerShell CWD) rather than GetFullPath, which
# resolves against the .NET process CWD and can differ after Set-Location calls.
$baseDir = if ($OutputDir) {
    Join-Path (Get-Location) $OutputDir
} else {
    Join-Path (Get-Location) "benchmark-results\$timestamp"
}

# --- Summary / DryRun modes ---------------------------------------------------

if ($Summary -or $DryRun) {
    $mode = if ($DryRun) { "DryRun" } else { "Summary" }
    Write-Header "NEsper Benchmark Runner -- $mode"
    Write-Info "Suites     : $($requestedSuites -join ', ')"
    Write-Info "Filter     : $Filter"
    Write-Info "Framework  : $Framework"
    Write-Info "GC (e2e)   : $GC"
    Write-Info "Output dir : $baseDir"
    Write-Host ""

    if ($DryRun) {
        foreach ($s in $requestedSuites) {
            if ($s -eq "perforator") {
                Write-Header "perforator -- fixed suite (no --list support)"
                Write-Info "Runs 6 EPL statements x 20 s each (~2 min total):"
                Write-Info "  TextWinLengthWithGroup"
                Write-Info "  TextWinLength"
                Write-Info "  TextAggregationWithTimeWindow"
                Write-Info "  TextAggregationWithNoWindow"
                Write-Info "  TextWithPropertyNames"
                Write-Info "  TextWithWildcard"
                continue
            }
            $proj = if ($s -eq "micro") { $MicroProject } else { $E2EProject }
            Write-Header "Available benchmarks: $s"
            & dotnet run --project $proj -c Release -- --list flat
        }
    }
    exit 0
}

# --- Metadata helpers ---------------------------------------------------------

function Get-GitMeta {
    $meta = @{ commit = ""; branch = ""; describe = "" }
    try { $meta.commit   = (& git rev-parse HEAD) -join ""; $meta.commit = $meta.commit.Trim() } catch {}
    try { $meta.branch   = (& git rev-parse --abbrev-ref HEAD) -join ""; $meta.branch = $meta.branch.Trim() } catch {}
    try { $meta.describe = (& git describe --tags --always --dirty) -join ""; $meta.describe = $meta.describe.Trim() } catch {}
    return $meta
}

function Get-MachineMeta {
    $dotnetVer = ""; $arch = ""
    try { $dotnetVer = (& dotnet --version) -join ""; $dotnetVer = $dotnetVer.Trim() } catch {}
    try { $arch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString() } catch {}
    return @{
        machineName   = [System.Environment]::MachineName
        os            = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
        processors    = [System.Environment]::ProcessorCount
        dotnetVersion = $dotnetVer
        architecture  = $arch
    }
}

# --- BenchmarkDotNet runner ---------------------------------------------------

function Invoke-BdnSuite {
    param(
        [string]$Name,
        [string]$Project,
        [string]$ArtifactsDir
    )

    Write-Header "Suite: $Name"

    New-Item -ItemType Directory -Path $ArtifactsDir -Force | Out-Null

    # Arguments forwarded to BenchmarkDotNet (after the "--" separator)
    $bdnArgs = @("--filter", $Filter, "--artifacts", $ArtifactsDir)

    Write-Info "Command : dotnet run --project $Project --framework $hostFramework -c Release -- $($bdnArgs -join ' ')"
    Write-Host ""

    # Out-Host streams benchmark output to the console without letting it leak
    # into this function's return value (functions return all pipeline output).
    & dotnet run --project $Project --framework $hostFramework -c Release -- @bdnArgs | Out-Host

    $exit = $LASTEXITCODE
    Write-Host ""
    if ($exit -eq 0) { Write-Ok  "$Name finished (exit 0)" }
    else             { Write-Fail "$Name failed (exit $exit)" }
    return $exit
}

# --- Perforator runner --------------------------------------------------------

function Invoke-Perforator {
    param([string]$OutputFile)

    Write-Header "Suite: Perforator (~2 min)"
    Write-Info "Command : dotnet run --project $PerforatorProject -c Release"
    Write-Info "Output  : $OutputFile"
    Write-Host ""

    # Tee to file and console simultaneously; Out-Host keeps the function's
    # return value clean (functions return all pipeline output).
    & dotnet run --project $PerforatorProject -c Release | Tee-Object -FilePath $OutputFile | Out-Host

    $exit = $LASTEXITCODE
    Write-Host ""
    if ($exit -eq 0) { Write-Ok  "Perforator finished (exit 0)" }
    else             { Write-Fail "Perforator failed (exit $exit)" }
    return $exit
}

# --- Main run -----------------------------------------------------------------

Write-Header "NEsper Benchmark Runner"
Write-Info "Suites     : $($requestedSuites -join ', ')"
Write-Info "Filter     : $Filter"
Write-Info "Framework  : $Framework"
Write-Info "GC (e2e)   : $GC"
Write-Info "Output dir : $baseDir"
Write-Host ""

New-Item -ItemType Directory -Path $baseDir -Force | Out-Null

# Write manifest.json before running so partial runs are still identifiable
$manifest = [ordered]@{
    runTimestamp = (Get-Date -Format "o")
    suites       = $requestedSuites
    filter       = $Filter
    framework    = $Framework
    gcMode       = $GC
    git          = Get-GitMeta
    machine      = Get-MachineMeta
    outputDir    = $baseDir
}
$manifest | ConvertTo-Json -Depth 5 | Set-Content -Path "$baseDir\manifest.json" -Encoding utf8

Write-Ok "manifest.json written"
Write-Host ""

# Run each requested suite and collect exit codes
$results = @{}

foreach ($suite in $requestedSuites) {
    switch ($suite) {
        "micro" {
            $results["micro"] = Invoke-BdnSuite `
                -Name "Microbenchmarks" `
                -Project $MicroProject `
                -ArtifactsDir "$baseDir\micro"
        }
        "e2e" {
            $results["e2e"] = Invoke-BdnSuite `
                -Name "End-to-End Benchmarks" `
                -Project $E2EProject `
                -ArtifactsDir "$baseDir\e2e"
        }
        "perforator" {
            $results["perforator"] = Invoke-Perforator `
                -OutputFile "$baseDir\perforator.txt"
        }
    }
}

# --- Final summary ------------------------------------------------------------

Write-Header "Run Summary"
Write-Info "Output: $baseDir"
Write-Host ""

$allPassed = $true
foreach ($suite in $requestedSuites) {
    $exit = $results[$suite]
    if ($exit -eq 0) { Write-Ok  $suite }
    else             { Write-Fail "$suite (exit $exit)"; $allPassed = $false }
}

Write-Host ""
Write-Info "Output layout:"
Write-Info "  manifest.json          git commit, branch, machine info, run args"
foreach ($suite in $requestedSuites) {
    switch ($suite) {
        "micro"      { Write-Info "  micro\results\         BenchmarkDotNet .md / .html / .csv" }
        "e2e"        { Write-Info "  e2e\results\           BenchmarkDotNet .md / .html / .csv" }
        "perforator" { Write-Info "  perforator.txt         events/s per EPL statement" }
    }
}

Write-Host ""
if ($allPassed) {
    Write-Host "  All suites passed." -ForegroundColor Green
} else {
    Write-Host "  One or more suites failed - check output above." -ForegroundColor Red
    exit 1
}
