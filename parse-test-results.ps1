#Requires -Version 5.1
<#
.SYNOPSIS
    Parses .trx test result files and reports individual test failures with retest commands.

.PARAMETER ResultsDir
    Path to a directory containing .trx files.
    If omitted, uses the most recently created TestResults/* subdirectory.

.PARAMETER ShowKnown
    Also display known/expected failures (hidden by default).

.EXAMPLE
    .\parse-test-results.ps1
    .\parse-test-results.ps1 -ResultsDir TestResults/2026-04-23_02-11-33
    .\parse-test-results.ps1 -ShowKnown
#>
param(
    [string]$ResultsDir,
    [switch]$ShowKnown
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Locate results directory ──────────────────────────────────────────────────

if (-not $ResultsDir) {
    $candidate = Get-ChildItem (Join-Path $PSScriptRoot 'TestResults') -Directory -ErrorAction SilentlyContinue |
        Sort-Object Name -Descending |
        Select-Object -First 1
    if (-not $candidate) {
        Write-Error "No TestResults/* directory found. Specify -ResultsDir explicitly."
        exit 1
    }
    $ResultsDir = $candidate.FullName
}

if (-not (Test-Path $ResultsDir)) {
    Write-Error "Results directory not found: $ResultsDir"
    exit 1
}

# ── Load test-batches.json ────────────────────────────────────────────────────

$configPath = Join-Path $PSScriptRoot 'test-batches.json'
if (-not (Test-Path $configPath)) {
    Write-Error "test-batches.json not found at $configPath"
    exit 1
}

$config      = Get-Content $configPath -Raw | ConvertFrom-Json
$testProject = $config.configuration.testProjectPath
$framework   = $config.configuration.defaultFramework
$runSettings = $config.configuration.runSettingsPath

# Build known-failure lookup: "ShortClass.Method" -> reason string
$knownMap = @{}
if ($config.knownFailures -and $config.knownFailures.failures) {
    foreach ($kf in $config.knownFailures.failures) {
        $knownMap[$kf.test] = if ($kf.reason) { $kf.reason } else { '' }
    }
}

# ── Parse TRX files ───────────────────────────────────────────────────────────

$allFailures = [System.Collections.Generic.List[PSCustomObject]]::new()

$trxFiles = @(Get-ChildItem $ResultsDir -Filter '*.trx' | Sort-Object Name)
if ($trxFiles.Count -eq 0) {
    Write-Host "No .trx files found in $ResultsDir" -ForegroundColor Yellow
    exit 0
}

foreach ($trxFile in $trxFiles) {
    [xml]$trx = Get-Content $trxFile.FullName -Raw

    # Index testId -> className from TestDefinitions
    $idToClass = @{}
    $unitTests = @($trx.TestRun.TestDefinitions.UnitTest)
    foreach ($ut in $unitTests) {
        if ($ut -and $ut.id) {
            $idToClass[$ut.id] = $ut.TestMethod.className
        }
    }

    $results = @($trx.TestRun.Results.UnitTestResult | Where-Object { $_ -and $_.outcome -eq 'Failed' })
    foreach ($result in $results) {
        $fullClass = $idToClass[$result.testId]
        $method    = $result.testName

        # Short class: last segment after '+' (nested), then last segment after '.'
        # Handles both  Outer+Nested  and  fully.qualified.ClassName  forms.
        $afterNested = ($fullClass -split '\+')[-1]
        $shortClass  = ($afterNested -split '\.')[-1]

        # Strip parameterization suffix like "(BASIC)" for lookup and filter
        $shortClassBase = $shortClass -replace '\([^)]*\)\s*$', ''

        $lookupKey = "$shortClassBase.$method"
        $isKnown   = $knownMap.ContainsKey($lookupKey)

        # Extract first line of error message
        $rawMsg = $result.Output.ErrorInfo.Message
        $firstLine = if ($rawMsg) {
            ($rawMsg -split "`r?`n")[0].Trim()
        } else { '' }

        # dotnet test --filter uses FullyQualifiedName~ (contains match).
        # ShortClassBase + Method is specific enough to target a single test fixture+method
        # while naturally including all parameterized variants ((BASIC), (ADVANCED), etc.).
        $filterExpr = "FullyQualifiedName~$shortClassBase.$method"
        $filterCmd  = "dotnet test $testProject --filter `"$filterExpr`" --framework $framework --settings $runSettings"

        $allFailures.Add([PSCustomObject]@{
            Batch          = $trxFile.BaseName
            FullClass      = $fullClass
            ShortClass     = $shortClass
            ShortClassBase = $shortClassBase
            Method         = $method
            LookupKey      = $lookupKey
            IsKnown        = $isKnown
            KnownReason    = if ($isKnown) { $knownMap[$lookupKey] } else { '' }
            FirstLine      = $firstLine
            FilterCmd      = $filterCmd
        })
    }
}

# ── Render output ─────────────────────────────────────────────────────────────

$runId      = Split-Path $ResultsDir -Leaf
$newFails   = @($allFailures | Where-Object { -not $_.IsKnown })
$knownFails = @($allFailures | Where-Object { $_.IsKnown })

Write-Host ''
Write-Host "Test Results: $runId" -ForegroundColor Cyan
Write-Host ('─' * 72) -ForegroundColor DarkGray

# ── New failures ──────────────────────────────────────────────────────────────

if ($newFails.Count -gt 0) {
    Write-Host ''
    Write-Host "NEW FAILURES  ($($newFails.Count))" -ForegroundColor Red

    foreach ($group in ($newFails | Group-Object Batch)) {
        Write-Host ''
        Write-Host "  [$($group.Name)]" -ForegroundColor Yellow
        foreach ($f in $group.Group) {
            Write-Host "    $($f.ShortClass).$($f.Method)" -ForegroundColor White
            if ($f.FirstLine) {
                Write-Host "    $($f.FirstLine)" -ForegroundColor DarkGray
            }
            Write-Host "    $($f.FilterCmd)" -ForegroundColor Cyan
            Write-Host ''
        }
    }
} else {
    Write-Host ''
    Write-Host '  No new failures.' -ForegroundColor Green
}

# ── Known failures (opt-in) ───────────────────────────────────────────────────

if ($ShowKnown) {
    if ($knownFails.Count -gt 0) {
        Write-Host ('─' * 72) -ForegroundColor DarkGray
        Write-Host ''
        Write-Host "KNOWN FAILURES  ($($knownFails.Count))  [expected, tracked]" -ForegroundColor DarkYellow
        foreach ($group in ($knownFails | Group-Object Batch)) {
            Write-Host ''
            Write-Host "  [$($group.Name)]" -ForegroundColor DarkGray
            foreach ($f in $group.Group) {
                Write-Host "    $($f.LookupKey)" -ForegroundColor DarkGray
                if ($f.KnownReason) {
                    Write-Host "      $($f.KnownReason)" -ForegroundColor DarkGray
                }
            }
        }
        Write-Host ''
    }
}

# ── Summary ───────────────────────────────────────────────────────────────────

Write-Host ('─' * 72) -ForegroundColor DarkGray
if ($newFails.Count -eq 0) {
    $label = if ($knownFails.Count -gt 0) { "$($knownFails.Count) known/expected" } else { 'none' }
    Write-Host "All failures are known/expected ($label). No action needed." -ForegroundColor Green
} else {
    Write-Host "$($newFails.Count) new failure(s)  |  $($knownFails.Count) known  |  $($allFailures.Count) total" -ForegroundColor Red
}
Write-Host ''
