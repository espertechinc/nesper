param(
    [string]$Framework = "net9.0",
    [string]$Verbosity = "minimal"
)

$tests = @(
    "FullyQualifiedName~WithNoShare$",
    "FullyQualifiedName~WithDisableShare$",
    "FullyQualifiedName~WithDisableShareCreate$",
    "FullyQualifiedName~WithSubqNamedWindowIndexShare$",
    "FullyQualifiedName~WithNoShare$",
    "FullyQualifiedName~WithNoMetaLexAnalysis$",
    "FullyQualifiedName~WithNoMetaLexAnalysisGroup$",
    "FullyQualifiedName~WithOnTriggerNWInsertRemove$"
)

$filter = "(" + ($tests -join "|") + ")"

$project = "tst/NEsper.Regression.Runner/NEsper.Regression.Runner.csproj"

$arguments = @(
    "test",
    $project,
    "--framework", $Framework,
    "--filter", $filter,
    "--logger", "trx;LogFileName=namedwindow-sharing-$Framework.trx",
    "--results-directory", "TestResults",
    "--verbosity", $Verbosity
)

Write-Host "Running named window sharing subset..."

dotnet @arguments
