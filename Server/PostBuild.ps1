param (
    [string]$Configuration
)

$scriptDir = $PSScriptRoot

# Determine mod output path based on build configuration
switch ($Configuration) {
    'Debug'   { $modOutput = Join-Path $scriptDir 'bin\Debug' }
    'Release' { $modOutput = Join-Path $scriptDir 'bin\Release' }
    default   {
        Write-Host "Unknown configuration: $Configuration"
        exit 1
    }
}

$sptPath = Join-Path (Get-Item $scriptDir).Parent.Parent.FullName "server-csharp\SPTarkov.Server\bin\$Configuration\net9.0"
$sptUserModsPath = Join-Path $sptPath 'user/mods'

# Ensure target directory exists
if (-not (Test-Path $sptUserModsPath)) {
    Write-Host "Target directory does not exist. Creating..."
    New-Item -ItemType Directory -Path $sptUserModsPath | Out-Null
}

# Copy contents of $modOutput to $sptUserModsPath
$source = Join-Path $modOutput '*'
Write-Host "Copying from: $source"
Write-Host "Copying to:   $sptUserModsPath"

try {
    Copy-Item -Path $source -Destination $sptUserModsPath -Recurse -Force -ErrorAction Stop
    Write-Host "Copy completed successfully."
} catch {
    Write-Host "Copy failed: $_"
}