param(
    [string]$Framework = "net9.0",
    [string]$Verbosity = "minimal"
)

$tests = @(
    "FullyQualifiedName~WithInputParameterConversion",
    "FullyQualifiedName~WithOutputColumnConversion",
    "FullyQualifiedName~WithOutputRowConversion",
    "FullyQualifiedName~WithPropertyResolution",
    "FullyQualifiedName~WithVariables$",
    "FullyQualifiedName~WithVariablesPoll",
    "FullyQualifiedName~WithExpressionPoll",
    "FullyQualifiedName~WithPropertyResolution",
    "FullyQualifiedName~WithParameterInjectionCallback"
)

$filter = "(" + ($tests -join "|") + ")"

$project = "tst/NEsper.Regression.Runner/NEsper.Regression.Runner.csproj"

$arguments = @(
    "test",
    $project,
    "--framework", $Framework,
    "--filter", $filter,
    "--logger", "trx;LogFileName=conversion-adapter-$Framework.trx",
    "--results-directory", "TestResults",
    "--verbosity", $Verbosity
)

Write-Host "Running conversion/adapter subset..."

dotnet @arguments
