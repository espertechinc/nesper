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
    Runs all enabled batches
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

$startTime = Get-Date

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

        $testArgs = @(
            "test",
            "--filter", $filter,
            "--framework", $targetFramework,
            "--settings", $runSettings,
            "--logger", "console;verbosity=normal"
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

        # Build test arguments
        $testArgs = @(
            "test",
            "--filter", $batch.filter,
            "--framework", $targetFramework,
            "--settings", "../../$runSettings",
            "--logger", "console;verbosity=normal"
        )

        if ($Verbose) {
            $testArgs[-1] = "console;verbosity=detailed"
        }

        # Run the tests
        Write-Info "Executing: dotnet $($testArgs -join ' ')"
        Write-Info ""

        $testOutput = & dotnet @testArgs 2>&1
        $exitCode = $LASTEXITCODE
        $batchEndTime = Get-Date
        $duration = ($batchEndTime - $batchStartTime).TotalSeconds

        # Parse results from output
        $testOutput | ForEach-Object { Write-Host $_ }

        $batchResult = @{
            Name = $batch.name
            ExitCode = $exitCode
            Duration = $duration
            Success = ($exitCode -eq 0)
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

    # Known failures reminder
    if ($config.knownFailures.failures.Count -gt 0) {
        Write-Info ""
        Write-Warning "Note: $($config.knownFailures.failures.Count) known failure(s) documented in configuration"
    }

    # Exit with appropriate code
    exit $(if ($results.Failed -gt 0) { 1 } else { 0 })

} finally {
    Pop-Location
}
