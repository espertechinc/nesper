param(
    [string]$Framework = "net9.0",
    [string]$Verbosity = "minimal"
)

$tests = @(
    "FullyQualifiedName~WithNoMetaLexAnalysis$",
    "FullyQualifiedName~WithNoMetaLexAnalysisGroup$",
    "FullyQualifiedName~WithInvalidPropertyEvent$",
    "FullyQualifiedName~WithInvalidPropertyHistorical$",
    "FullyQualifiedName~WithInvalid1Stream$",
    "FullyQualifiedName~WithInvalidSQL$",
    "FullyQualifiedName~WithInvalidBothHistorical$",
    "FullyQualifiedName~WithInvalidPropertyHistorical$",
    "FullyQualifiedName~WithInvalidPropertyEvent$"
)

$filter = "(" + ($tests -join "|") + ")"

$project = "tst/NEsper.Regression.Runner/NEsper.Regression.Runner.csproj"

$arguments = @(
    "test",
    $project,
    "--framework", $Framework,
    "--filter", $filter,
    "--logger", "trx;LogFileName=lex-validation-$Framework.trx",
    "--results-directory", "TestResults",
    "--verbosity", $Verbosity
)

Write-Host "Running lex/semantic validation subset..."

dotnet @arguments
