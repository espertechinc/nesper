param(
    [string]$Framework = "net9.0",
    [string]$Verbosity = "minimal"
)

$tests = @(
    "FullyQualifiedName~TestInfraTableMTUngroupedAccessWithinRowFAFConsistency",
    "FullyQualifiedName~TestInfraTableMTGroupedAccessReadIntoTableWriteAggColConsistency",
    "FullyQualifiedName~TestMultithreadStmtNamedWindowUpdate",
    "FullyQualifiedName~TestMultithreadStmtNamedWindowMerge",
    "FullyQualifiedName~TestMultithreadStmtNamedWindowDelete",
    "FullyQualifiedName~TestMultithreadStmtDatabaseJoin",
    "FullyQualifiedName~TestMultithreadStmtListenerCreateStmt",
    "FullyQualifiedName~TestMultithreadStmtTwoPatterns",
    "FullyQualifiedName~TestMultithreadContextCountSimple",
    "FullyQualifiedName~TestMultithreadDeployAtomic"
)

if ($tests.Count -eq 0) {
    Write-Error "No tests configured"
    exit 1
}

$filter = "(" + ($tests -join "|") + ")"

Write-Host "Running multithread regression subset..."

$project = "tst/NEsper.Regression.Runner/NEsper.Regression.Runner.csproj"

$arguments = @(
    "test",
    $project,
    "--framework", $Framework,
    "--filter", $filter,
    "--logger", "trx;LogFileName=multithread-regressions-$Framework.trx",
    "--results-directory", "TestResults",
    "--verbosity", $Verbosity
)

dotnet @arguments
