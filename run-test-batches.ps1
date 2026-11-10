#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs NEsper regression tests in configurable batches
.DESCRIPTION
    Reads test-batches.json configuration and executes tests in batches with
    configurable timeouts, filters, and reporting.
.PARAMETER ConfigFile
    Path to the JSON configuration file (default: test-batches.json)
.PARAMETER BatchName
    Run only a specific batch by name
.PARAMETER Framework
    Target framework (default: from config or net9.0)
.PARAMETER Verbose
    Enable verbose test output
.PARAMETER IsolatedOnly
    Run only the isolated tests
.PARAMETER Summary
    Show configuration summary without running tests
.EXAMPLE
    .\run-test-batches.ps1
    Runs all enabled batches; TRX results saved under TestResults\<timestamp>\
.EXAMPLE
    .\run-test-batches.ps1 -BatchName "EPL-Database"
    Runs only the EPL-Database batch
.EXAMPLE
    .\run-test-batches.ps1 -Summary
    Shows configuration without running tests
#>

param(
    [string]$ConfigFile = "test-batches.json",
    [string]$BatchName = "",
    [string]$Framework = "",
    [switch]$Verbose,
    [switch]$IsolatedOnly,
    [switch]$Summary
)

# Color output functions
function Write-Header {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Green
}

function Write-Failure {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Message)
    Write-Host $Message -ForegroundColor White
}

# Load configuration
if (-not (Test-Path $ConfigFile)) {
    Write-Failure "Configuration file not found: $ConfigFile"
    exit 1
}

Write-Info "Loading configuration from: $ConfigFile"
$config = Get-Content $ConfigFile | ConvertFrom-Json

# Apply configuration defaults
$targetFramework = if ($Framework) { $Framework } else { $config.configuration.defaultFramework }
$runSettings = $config.configuration.runSettingsPath
$testProject = $config.configuration.testProjectPath
$stopOnFailure = $config.configuration.stopOnFirstFailure

# Summary mode
if ($Summary) {
    Write-Header "Test Batch Configuration Summary"
    Write-Info "Framework: $targetFramework"
    Write-Info "Run Settings: $runSettings"
    Write-Info "Test Project: $testProject"
    Write-Info "Stop on First Failure: $stopOnFailure"
    Write-Info ""

    Write-Info "Enabled Batches: $($config.batches | Where-Object { $_.enabled -eq $true } | Measure-Object | Select-Object -ExpandProperty Count)"
    Write-Info "Disabled Batches: $($config.batches | Where-Object { $_.enabled -eq $false } | Measure-Object | Select-Object -ExpandProperty Count)"
    Write-Info ""

    Write-Host "Batch Details:" -ForegroundColor Cyan
    foreach ($batch in $config.batches) {
        $status = if ($batch.enabled) { "ENABLED " } else { "DISABLED" }
        $color = if ($batch.enabled) { "Green" } else { "DarkGray" }
        Write-Host "  [$status] " -ForegroundColor $color -NoNewline
        Write-Host "$($batch.name) " -ForegroundColor White -NoNewline
        Write-Host "- $($batch.description)" -ForegroundColor Gray
    }

    Write-Info ""
    Write-Host "Known Failures: $($config.knownFailures.failures.Count)" -ForegroundColor Yellow
    foreach ($failure in $config.knownFailures.failures) {
        Write-Host "  - $($failure.test): $($failure.reason)" -ForegroundColor DarkYellow
    }

    exit 0
}

# Track results
$results = @{
    Total = 0
    Passed = 0
    Failed = 0
    Skipped = 0
    BatchResults = @()
}

# Create TestResults directory if it doesn't exist (use absolute path from script location)
$scriptDir = $PSScriptRoot
$testResultsDir = Join-Path $scriptDir "TestResults"
if (-not (Test-Path $testResultsDir)) {
    New-Item -ItemType Directory -Path $testResultsDir | Out-Null
}

$startTime = Get-Date
$runTimestamp = $startTime.ToString("yyyy-MM-dd_HH-mm-ss")
$timestamp = $startTime.ToString("yyyyMMdd_HHmmss")
$testResultsDir = Join-Path $PSScriptRoot "TestResults\$runTimestamp"
New-Item -ItemType Directory -Path $testResultsDir -Force | Out-Null
Write-Info "Test results directory: $testResultsDir"

# ---- Run metadata (run.json). Lets tools such as tools/test-dashboard tell what this run is and whether it is
# still alive: pid + processStart identify this PowerShell process, childPid the current "dotnet test".
$runMeta = [ordered]@{
    runId            = $runTimestamp
    status           = 'running'
    pid              = $PID
    processStart     = (Get-Process -Id $PID).StartTime.ToString('o')
    host             = "PowerShell $($PSVersionTable.PSVersion)"
    startedAt        = $startTime.ToString('o')
    finishedAt       = $null
    mode             = $(if ($IsolatedOnly) { 'isolated' } else { 'batches' })
    framework        = $targetFramework
    batchFilter      = $BatchName
    batches          = @()
    currentBatch     = $null
    currentLog       = $null
    childPid         = $null
    completedBatches = @()
    stoppedEarly     = $false
    exitCode         = $null
    env              = [ordered]@{}
    git              = $null
}
Get-ChildItem Env: | Where-Object { $_.Name -like 'NESPER_*' -or $_.Name -eq 'CONFIGFACTORY_CLASS' -or $_.Name -eq 'esper_logcode' } |
    ForEach-Object { $runMeta.env[$_.Name] = $_.Value }
try {
    $gitBranch = (& git -C $PSScriptRoot rev-parse --abbrev-ref HEAD 2>$null)
    $gitCommit = (& git -C $PSScriptRoot rev-parse --short HEAD 2>$null)
    $gitDirty  = [bool](& git -C $PSScriptRoot status --porcelain 2>$null)
    $runMeta.git = [ordered]@{ branch = "$gitBranch"; commit = "$gitCommit"; dirty = $gitDirty }
} catch { }

function Save-RunMeta {
    # Metadata must never fail a test run; retry because a reader may briefly hold the file.
    $target = Join-Path $testResultsDir 'run.json'
    $tmp = "$target.tmp"
    for ($attempt = 0; $attempt -lt 5; $attempt++) {
        try {
            [System.IO.File]::WriteAllText($tmp, ($runMeta | ConvertTo-Json -Depth 6))
            Move-Item -Path $tmp -Destination $target -Force
            return
        } catch { Start-Sleep -Milliseconds 100 }
    }
}
Save-RunMeta

# Run isolated tests if requested
if ($IsolatedOnly) {
    Write-Header "Running Isolated Tests"

    foreach ($test in $config.isolatedTests.tests) {
        if (-not $test.enabled) {
            Write-Warning "Skipping disabled isolated test: $($test.name)"
            continue
        }

        Write-Info "Running isolated test: $($test.name)"
        Write-Info "  Reason: $($test.reason)"

        $timeout = $test.timeout
        $filter = $test.filter

        $trxFile = Join-Path $testResultsDir "$($test.name).trx"
        $testArgs = @(
            "test",
            "--filter", $filter,
            "--framework", $targetFramework,
            "--settings", $runSettings,
            "--logger", "console;verbosity=normal",
            "--logger", "trx;LogFileName=$trxFile"
        )

        $testResult = & dotnet @testArgs
        $exitCode = $LASTEXITCODE

        if ($exitCode -eq 0) {
            Write-Success "  [PASS] $($test.name)"
            $results.Passed++
        } else {
            Write-Failure "  [FAIL] $($test.name)"
            $results.Failed++
        }
        $results.Total++
    }

    # Print summary
    Write-Header "Isolated Tests Summary"
    Write-Info "Total: $($results.Total)"
    Write-Success "Passed: $($results.Passed)"
    Write-Failure "Failed: $($results.Failed)"
    Write-Info ""
    Write-Info "TRX results: $testResultsDir"

    $runMeta.status = 'completed'
    $runMeta.finishedAt = (Get-Date).ToString('o')
    $runMeta.exitCode = $(if ($results.Failed -gt 0) { 1 } else { 0 })
    Save-RunMeta
    exit $(if ($results.Failed -gt 0) { 1 } else { 0 })
}

# Change to test project directory
Push-Location $testProject

try {
    # Determine which batches to run
    if ($BatchName) {
        $batchesToRun = @($config.batches | Where-Object { $_.name -eq $BatchName })
        if ($batchesToRun.Count -eq 0) {
            Write-Failure "Batch not found: $BatchName"
            Write-Info "Available batches:"
            $config.batches | ForEach-Object { Write-Info "  - $($_.name)" }
            exit 1
        }
    } else {
        $batchesToRun = @($config.batches | Where-Object { $_.enabled -eq $true })
    }

    $runMeta.batches = @($batchesToRun | ForEach-Object { $_.name })
    Save-RunMeta

    Write-Header "NEsper Regression Test Batches"
    Write-Info "Running $($batchesToRun.Count) batch(es) on framework: $targetFramework"
    Write-Info ""

    # Run each batch
    $batchNumber = 1
    foreach ($batch in $batchesToRun) {
        Write-Header "Batch $batchNumber/$($batchesToRun.Count): $($batch.name)"
        Write-Info "Description: $($batch.description)"
        Write-Info "Filter: $($batch.filter)"
        Write-Info "Timeout: $($batch.timeout)ms"

        $batchStartTime = Get-Date

        # Create output filename for this batch
        $batchNameSafe = $batch.name -replace '[^a-zA-Z0-9-]', '_'
        $outputFile = Join-Path $testResultsDir "TestResult_${timestamp}_${batchNameSafe}.txt"
        $trxFile = Join-Path $testResultsDir "TestResult_${timestamp}_${batchNameSafe}.trx"

        # Build test arguments
        $consoleVerbosity = if ($Verbose) { "detailed" } else { "normal" }
        $trxFile = Join-Path $testResultsDir "$($batch.name).trx"
        $testArgs = @(
            "test",
            "--filter", $batch.filter,
            "--framework", $targetFramework,
            "--settings", "../../$runSettings",
            "--logger", "console;verbosity=$consoleVerbosity",
            "--logger", "trx;LogFileName=$trxFile"
        )

        # Run the tests
        Write-Info "Executing: dotnet $($testArgs -join ' ')"
        Write-Info "Output file: $outputFile"
        Write-Info ""

        # Output is echoed as it arrives and appended to a live log that tools can follow while the batch runs.
        # The live log is removed once the final output file below has been written.
        $liveFile = Join-Path $testResultsDir "$batchNameSafe.live.log"
        $runMeta.currentBatch = $batch.name
        $runMeta.currentLog = [System.IO.Path]::GetFileName($liveFile)
        $runMeta.childPid = $null
        Save-RunMeta

        $lines = New-Object System.Collections.Generic.List[string]
        $liveStream = New-Object System.IO.FileStream($liveFile, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write, [System.IO.FileShare]::ReadWrite)
        $live = New-Object System.IO.StreamWriter($liveStream)
        $live.AutoFlush = $true
        $childTries = 0
        try {
            & dotnet @testArgs 2>&1 | ForEach-Object {
                $line = "$_"
                if ($null -eq $runMeta.childPid -and $childTries -lt 20) {
                    $childTries++
                    try {
                        $child = Get-CimInstance Win32_Process -Filter "ParentProcessId=$PID AND Name='dotnet.exe'" -ErrorAction Stop | Select-Object -First 1
                        if ($child) { $runMeta.childPid = $child.ProcessId; Save-RunMeta }
                    } catch { $childTries = 20 }
                }
                $lines.Add($line)
                $live.WriteLine($line)
                Write-Host $line
            }
            $exitCode = $LASTEXITCODE
        } finally {
            $live.Dispose()
        }
        $batchEndTime = Get-Date
        $duration = ($batchEndTime - $batchStartTime).TotalSeconds

        # Write header to output file
        $fileHeader = @"
========================================
NEsper Test Batch Results
========================================
Batch: $($batch.name)
Description: $($batch.description)
Filter: $($batch.filter)
Framework: $targetFramework
Start Time: $($batchStartTime.ToString("yyyy-MM-dd HH:mm:ss"))
End Time: $($batchEndTime.ToString("yyyy-MM-dd HH:mm:ss"))
Duration: $('{0:N2}' -f $duration)s
Exit Code: $exitCode
========================================

"
"@
        Set-Content -Path $outputFile -Value $fileHeader
        Add-Content -Path $outputFile -Value (($lines -join "`r`n") + "`r`n")
        Remove-Item -Path $liveFile -ErrorAction SilentlyContinue

        $runMeta.currentBatch = $null
        $runMeta.currentLog = $null
        $runMeta.childPid = $null
        $runMeta.completedBatches += [ordered]@{ name = $batch.name; exitCode = $exitCode; seconds = [math]::Round($duration, 1) }
        Save-RunMeta

        $batchResult = @{
            Name = $batch.name
            ExitCode = $exitCode
            Duration = $duration
            Success = ($exitCode -eq 0)
            OutputFile = $outputFile
        }

        $results.BatchResults += $batchResult
        $results.Total++

        if ($exitCode -eq 0) {
            Write-Success "`n[PASS] $($batch.name) ($('{0:N2}' -f $duration)s)"
            $results.Passed++
        } else {
            Write-Failure "`n[FAIL] $($batch.name) ($('{0:N2}' -f $duration)s)"
            $results.Failed++

            if ($stopOnFailure) {
                Write-Warning "Stopping test execution due to failure (stopOnFirstFailure=true)"
                $runMeta.stoppedEarly = $true
                break
            }
        }

        $batchNumber++
    }

    # Print final summary
    $endTime = Get-Date
    $totalDuration = ($endTime - $startTime).TotalSeconds

    Write-Header "Test Execution Summary"
    Write-Info "Total Duration: $('{0:N2}' -f $totalDuration)s"
    Write-Info ""
    Write-Info "Batches:"
    Write-Info "  Total: $($results.Total)"
    Write-Success "  Passed: $($results.Passed)"
    Write-Failure "  Failed: $($results.Failed)"
    Write-Info ""

    # Detailed batch results
    Write-Host "Batch Results:" -ForegroundColor Cyan
    foreach ($result in $results.BatchResults) {
        $status = if ($result.Success) { "PASS" } else { "FAIL" }
        $color = if ($result.Success) { "Green" } else { "Red" }
        $duration = "{0:N2}s" -f $result.Duration
        Write-Host "  [$status] " -ForegroundColor $color -NoNewline
        Write-Host "$($result.Name) " -ForegroundColor White -NoNewline
        Write-Host "($duration)" -ForegroundColor Gray
    }

    # Write summary file
    $summaryFile = Join-Path $testResultsDir "TestSummary_${timestamp}.txt"
    $summaryContent = @"
========================================
NEsper Test Execution Summary
========================================
Execution Time: $($startTime.ToString("yyyy-MM-dd HH:mm:ss"))
Total Duration: $('{0:N2}' -f $totalDuration)s
Framework: $targetFramework

Batch Summary:
  Total: $($results.Total)
  Passed: $($results.Passed)
  Failed: $($results.Failed)

========================================
Detailed Batch Results:
========================================

"
"@
    foreach ($result in $results.BatchResults) {
        $status = if ($result.Success) { "PASS" } else { "FAIL" }
        $duration = "{0:N2}s" -f $result.Duration
        $summaryContent += "[$status] $($result.Name) ($duration)`n"
        $summaryContent += "  Output: $($result.OutputFile)`n`n"
    }
    
    if ($config.knownFailures.failures.Count -gt 0) {
        $summaryContent += "`n========================================`n"
        $summaryContent += "Known Failures ($($config.knownFailures.failures.Count)):`n"
        $summaryContent += "========================================`n`n"
        foreach ($failure in $config.knownFailures.failures) {
            $summaryContent += "  - $($failure.test): $($failure.reason)`n"
        }
    }
    
    Set-Content -Path $summaryFile -Value $summaryContent
    Write-Info ""
    Write-Success "Summary written to: $summaryFile"

    # Known failures reminder
    if ($config.knownFailures.failures.Count -gt 0) {
        Write-Info ""
        Write-Warning "Note: $($config.knownFailures.failures.Count) known failure(s) documented in configuration"
    }

    Write-Info ""
    Write-Info "TRX results: $testResultsDir"

    # Write summary.txt so the run is reviewable after the terminal closes
    $summaryLines = @(
        "NEsper Test Run Summary",
        "Run:       $runTimestamp",
        "Framework: $targetFramework",
        "Duration:  $('{0:N2}' -f $totalDuration)s",
        "Batches:   $($results.Total) total, $($results.Passed) passed, $($results.Failed) failed",
        "",
        "Batch Results:"
    )
    foreach ($result in $results.BatchResults) {
        $status = if ($result.Success) { "PASS" } else { "FAIL" }
        $summaryLines += "  [$status] $($result.Name) ($('{0:N2}' -f $result.Duration)s)"
    }
    $summaryLines | Set-Content (Join-Path $testResultsDir "summary.txt")

    # Record how the run ended, then exit with appropriate code
    $runMeta.status = 'completed'
    $runMeta.finishedAt = (Get-Date).ToString('o')
    $runMeta.exitCode = $(if ($results.Failed -gt 0) { 1 } else { 0 })
    Save-RunMeta
    exit $(if ($results.Failed -gt 0) { 1 } else { 0 })

} finally {
    Pop-Location
    if ($runMeta.status -eq 'running') {
        # Abnormal exit (Ctrl+C or an error): do not leave this run's test host behind, and record how it ended.
        if ($runMeta.childPid) {
            try { & taskkill /PID $runMeta.childPid /T /F 2>$null | Out-Null } catch { }
        }
        $runMeta.status = 'aborted'
        $runMeta.finishedAt = (Get-Date).ToString('o')
        Save-RunMeta
    }
}
