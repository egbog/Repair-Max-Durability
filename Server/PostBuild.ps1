$scriptDir = $PSScriptRoot
$modOutput = Join-Path $scriptDir 'bin\Debug'
$sptPath = Join-Path (Get-Item $scriptDir).Parent.FullName 'server-csharp\SPTarkov.Server\bin\Debug\net9.0'
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