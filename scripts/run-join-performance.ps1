param(
    [string]$Framework = "net9.0",
    [string]$Verbosity = "minimal"
)

$tests = @(
    "FullyQualifiedName~WithKeyAndRangePerformance",
    "FullyQualifiedName~WithKeyBTreePerformance",
    "FullyQualifiedName~WithJoinPerformanceStreamB",
    "FullyQualifiedName~TestInfraNamedWIndowFAFQueryJoinPerformance",
    "FullyQualifiedName~TestEPLDatabaseJoinPerfNoCache",
    "FullyQualifiedName~TestEPLJoinUniqueIndex",
    "FullyQualifiedName~TestEPLDatabaseJoinPerformance",
    "FullyQualifiedName~WithRangePerformance",
    "FullyQualifiedName~With1Stream2HistInnerJoinPerformance"
)

$filter = "(" + ($tests -join "|") + ")"

$project = "tst/NEsper.Regression.Runner/NEsper.Regression.Runner.csproj"

$arguments = @(
    "test",
    $project,
    "--framework", $Framework,
    "--filter", $filter,
    "--logger", "trx;LogFileName=join-performance-$Framework.trx",
    "--results-directory", "TestResults",
    "--verbosity", $Verbosity
)

Write-Host "Running join-performance subset..."

dotnet @arguments
