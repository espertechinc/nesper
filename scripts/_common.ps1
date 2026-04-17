#!/usr/bin/env pwsh
# scripts/_common.ps1
# Shared utilities for NEsper regression test analysis scripts.
# Dot-source this file: . "$PSScriptRoot/_common.ps1"

# ---------------------------------------------------------------------------
# Color output helpers
# ---------------------------------------------------------------------------

function Write-Header {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host " $Message" -ForegroundColor Cyan
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

function Write-Warn {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Message)
    Write-Host $Message -ForegroundColor White
}

function Write-Dim {
    param([string]$Message)
    Write-Host $Message -ForegroundColor DarkGray
}

# ---------------------------------------------------------------------------
# TRX parsing
# ---------------------------------------------------------------------------

# Returns an array of PSCustomObjects for every failed UnitTestResult in a TRX file.
# Properties: TestName, ErrorMessage, StackTrace
function Get-TrxFailures {
    param(
        [Parameter(Mandatory)][string]$TrxPath
    )

    if (-not (Test-Path $TrxPath)) {
        Write-Failure "TRX file not found: $TrxPath"
        return @()
    }

    [xml]$xml = Get-Content $TrxPath -Encoding UTF8 -Raw
    $nsm = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
    $nsm.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")

    $nodes = $xml.SelectNodes("//t:UnitTestResult[@outcome='Failed']", $nsm)

    $failures = @()
    foreach ($node in $nodes) {
        $msgNode   = $node.SelectSingleNode("t:Output/t:ErrorInfo/t:Message", $nsm)
        $traceNode = $node.SelectSingleNode("t:Output/t:ErrorInfo/t:StackTrace", $nsm)
        $failures += [PSCustomObject]@{
            TestName     = $node.GetAttribute("testName")
            ErrorMessage = if ($msgNode)   { $msgNode.InnerText }   else { "" }
            StackTrace   = if ($traceNode) { $traceNode.InnerText } else { "" }
        }
    }

    return $failures
}

# Returns counts of all outcomes from a TRX file: Passed, Failed, NotExecuted, Total
function Get-TrxSummary {
    param(
        [Parameter(Mandatory)][string]$TrxPath
    )

    if (-not (Test-Path $TrxPath)) {
        return $null
    }

    [xml]$xml = Get-Content $TrxPath -Encoding UTF8 -Raw
    $nsm = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
    $nsm.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")

    $counters = $xml.SelectSingleNode("//t:ResultSummary/t:Counters", $nsm)
    if ($counters) {
        return [PSCustomObject]@{
            Total       = [int]$counters.GetAttribute("total")
            Passed      = [int]$counters.GetAttribute("passed")
            Failed      = [int]$counters.GetAttribute("failed")
            NotExecuted = [int]$counters.GetAttribute("notExecuted")
        }
    }

    return $null
}

# ---------------------------------------------------------------------------
# Failure categorization
# ---------------------------------------------------------------------------

# Known failing tests from test-batches.json (XML XPath issue, tracked as XML-001)
$script:KnownXmlFailures = @(
    "TestEventXMLSchemaEventObservationDOM.WithCreateSchema",
    "TestEventXMLSchemaEventObservationDOM.WithPreconfig",
    "TestEventXMLSchemaEventObservationXPath.WithCreateSchema",
    "TestEventXMLSchemaEventObservationXPath.WithPreconfig"
)

# Categorizes a failure PSCustomObject (from Get-TrxFailures) into one of:
#   KnownXML    - tracked XML XPath failures (XML-001)
#   Database    - driver/connection failures (should be fixed by DB fix)
#   Performance - timing/threshold assertion failures
#   Logic       - all other assertion / regression failures
function Get-FailureCategory {
    param([PSCustomObject]$Failure)

    $name = $Failure.TestName
    $combined = "$($Failure.ErrorMessage) $($Failure.StackTrace)"

    # Known XML failures (exact match on suffix)
    foreach ($known in $script:KnownXmlFailures) {
        if ($name -like "*$known*") {
            return "KnownXML"
        }
    }

    # Database / driver failures
    if ($combined -match "MissingMethodException|No parameterless constructor|DbDriverPgSQL|NpgsqlException|could not connect|connection refused|ECONNREFUSED|DbDriver") {
        return "Database"
    }

    # Performance / timing threshold failures
    # Covers: "delta < N ms", "Expected: less than N", Multithread timing tests
    if ($combined -match "\bdelta\b|\belapsed\b|performance.*threshold|less than \d+.*ms|Expected.*less than" `
        -or $name -match "Multithreaded|Performance|Throughput") {
        return "Performance"
    }

    return "Logic"
}

# ---------------------------------------------------------------------------
# Runner helpers
# ---------------------------------------------------------------------------

$script:RootDir = Split-Path $PSScriptRoot -Parent
$script:TestProject = Join-Path $script:RootDir "tst/NEsper.Regression.Runner/NEsper.Regression.Runner.csproj"
$script:RunSettings = Join-Path $script:RootDir "NEsper.runsettings"
$script:ResultsDir  = Join-Path $script:RootDir "TestResults"

function Invoke-RegressionTests {
    param(
        [string]$Filter        = "",
        [string]$TrxName       = "regression",
        [string]$Framework     = "net9.0",
        [switch]$Verbose
    )

    if (-not (Test-Path $script:ResultsDir)) {
        New-Item -ItemType Directory -Path $script:ResultsDir | Out-Null
    }

    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $trxFile   = Join-Path $script:ResultsDir "$TrxName-$timestamp.trx"
    $logFile   = Join-Path $script:ResultsDir "$TrxName-$timestamp.log"

    $args = @(
        "test", $script:TestProject,
        "--framework", $Framework,
        "--settings", $script:RunSettings,
        "--logger", "trx;LogFileName=$trxFile",
        "--logger", "console;verbosity=$(if ($Verbose) { 'detailed' } else { 'normal' })"
    )

    if ($Filter) {
        $args += "--filter", $Filter
    }

    Write-Info "Command : dotnet $($args -join ' ')"
    Write-Info "TRX out : $trxFile"
    Write-Info ""

    $startTime = Get-Date
    & dotnet @args 2>&1 | Tee-Object -FilePath $logFile
    $exitCode = $LASTEXITCODE
    $duration = ((Get-Date) - $startTime).TotalSeconds

    Write-Info ""
    Write-Info "Duration: $([math]::Round($duration, 1))s  |  Exit code: $exitCode"

    return [PSCustomObject]@{
        TrxPath  = $trxFile
        LogPath  = $logFile
        ExitCode = $exitCode
        Duration = $duration
    }
}

# Print a formatted list of failures grouped by category
function Write-FailureList {
    param(
        [PSCustomObject[]]$Failures,
        [string[]]$Categories,     # which categories to include
        [int]$MaxMessage = 300     # truncate long messages
    )

    $shown = $Failures | Where-Object { $Categories -contains (Get-FailureCategory $_) }

    if ($shown.Count -eq 0) {
        Write-Success "  No failures in this category."
        return
    }

    foreach ($f in $shown) {
        $cat = Get-FailureCategory $f
        Write-Host "  [$cat] " -ForegroundColor Yellow -NoNewline
        Write-Host $f.TestName -ForegroundColor White
        $msg = $f.ErrorMessage -replace "`r?`n", " " | Select-Object -First 1
        if ($msg.Length -gt $MaxMessage) { $msg = $msg.Substring(0, $MaxMessage) + "..." }
        Write-Dim "         $msg"
    }
}
