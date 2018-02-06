param(
	[string] $Version,
	[string] $BuildPath
)

# Add the compression library
Add-Type -Assembly "System.IO.Compression.FileSystem"

# Set the location where the build will land
$BuildRoot = "$BuildPath\NEsper-$Version"
$BuildRoot = [System.IO.Path]::GetFullPath("$BuildRoot")

# Extract the release from the version - ignoring build artifacts
$ReleaseVersion = [System.Text.RegularExpressions.Regex]::Replace(
	$Version, "(\d+)\.(\d+)\.(\d+)\..*", "$1.$2.$3");

# Set the location where the release will land
$ReleaseRoot = "$BuildPath\NEsper-$ReleaseVersion";
$ReleaseRoot = [System.IO.Path]::GetFullPath("$ReleaseRoot")

$ReleaseZipFile = "$BuildPath\NEsper-$ReleaseVersion.zip";

# Remove the zip file if it already exists
if (Test-Path "$ReleaseZipFile") {
	Remove-Item "$ReleaseZipFile"
}

# Move the build path to the release path if different
if ($BuildRoot -ne $ReleaseRoot) {
	[System.IO.Directory]::Delete($ReleaseRoot, $TRUE);
	[System.IO.Directory]::Move($BuildRoot, $ReleaseRoot);
}

# Compress the artifacts
[IO.Compression.ZipFile]::CreateFromDirectory(
	"$BuildPath\NEsper-$ReleaseVersion",
	"$ReleaseZipFile",
	[System.IO.Compression.CompressionLevel]::Optimal,
	$TRUE
)
