$solutionRoot = "d:\Src\Espertech\NEsper-8.9.x"
Get-ChildItem -Path $solutionRoot -Recurse -Filter *.csproj | ForEach-Object {
    $proj = $_.FullName
    Write-Host "Processing $proj"
    $outdated = dotnet list $proj package --outdated
    $lines = $outdated -split "`n"
    foreach ($line in $lines) {
        if ($line -match '^\s*(\S+)\s+([\d\.]+)\s+([\d\.]+)') {
            $pkg = $Matches[1]
            Write-Host "Updating $pkg in $proj"
            dotnet add $proj package $pkg
        }
    }
}
