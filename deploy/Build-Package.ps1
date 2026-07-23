<#
.SYNOPSIS
    Builds SPIE Ribbon for all Revit versions and assembles a self-contained install package.

.DESCRIPTION
    Produces dist\SpieRibbon-Package\ with this layout:

        host\<year>\        host DLL + Contracts.dll + deps
        modules\<year>\     every module DLL (+ their dependencies, e.g. ClosedXML)
        Deploy-SpieRibbon.ps1
        Install.bat
        README.txt

    then zips it to dist\SpieRibbon-<label>.zip. Hand the zip to a colleague; they extract it
    and double-click Install.bat. No admin rights required.

.PARAMETER SkipBuild
    Package the existing build output without rebuilding.
#>
param(
    [string]$Configuration = "Release",
    [string]$Label = "Beta-v0.1",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path $PSScriptRoot -Parent
$srcRoot  = Join-Path $repoRoot "src"
$sln      = Join-Path $repoRoot "SpieRibbon.sln"

$versionMap = @{ 2024 = "R24"; 2025 = "R25"; 2026 = "R26" }
$moduleProjects = @("SpieRibbon.Algemeen", "SpieRibbon.Civil", "SpieRibbon.EnI", "SpieRibbon.Hvac")

$packageDir = Join-Path $repoRoot "dist\SpieRibbon-Package"

# --- build ---
if (-not $SkipBuild) {
    foreach ($year in ($versionMap.Keys | Sort-Object)) {
        $cfg = "$Configuration $($versionMap[$year])"
        Write-Host "Building $cfg ..." -ForegroundColor Cyan
        & dotnet build $sln -c $cfg | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Build failed for configuration '$cfg'." }
    }
}

# --- stage ---
if (Test-Path $packageDir) { Remove-Item $packageDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $packageDir | Out-Null

foreach ($year in ($versionMap.Keys | Sort-Object)) {
    $suffix = $versionMap[$year]
    $cfgFolder = "$Configuration $suffix"

    # host
    $hostBin = Join-Path $srcRoot "SpieRibbon\bin\$cfgFolder"
    if (-not (Test-Path $hostBin)) { throw "Missing host build: $hostBin" }
    $hostDest = Join-Path $packageDir "host\$year"
    New-Item -ItemType Directory -Force -Path $hostDest | Out-Null
    Copy-Item -Path (Join-Path $hostBin '*') -Destination $hostDest -Recurse -Force

    # modules (merged into one folder per year)
    $modDest = Join-Path $packageDir "modules\$year"
    New-Item -ItemType Directory -Force -Path $modDest | Out-Null
    foreach ($proj in $moduleProjects) {
        $modBin = Join-Path $srcRoot "$proj\bin\$cfgFolder"
        if (-not (Test-Path $modBin)) { throw "Missing module build: $modBin" }
        Copy-Item -Path (Join-Path $modBin '*') -Destination $modDest -Recurse -Force
    }
}

# --- deploy script + launcher + readme ---
Copy-Item -Path (Join-Path $PSScriptRoot "Deploy-SpieRibbon.ps1") -Destination $packageDir -Force

$installBat = @'
@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Deploy-SpieRibbon.ps1"
echo.
pause
'@
Set-Content -Path (Join-Path $packageDir "Install.bat") -Value $installBat -Encoding ASCII

$readme = @'
SPIE Ribbon - install instructions
===================================

1. Copy this whole folder anywhere on your PC (Desktop, Documents, etc).

2. If Revit is open, close it first (Windows won't let the installer
   replace files Revit still has loaded).

3. Double-click "Install.bat". It installs the SPIE tab for whichever
   Revit versions (2024 / 2025 / 2026) are on this PC. No admin rights needed.

4. Start Revit. On the "SPIE" tab, click "SPIE Toolbox".
   Use the Settings button in the toolbox to enable the modules you want
   (SPIE Algemeen, Civil, E&I, HVAC). New modules start disabled.

To update later: get the new package folder, close Revit, run Install.bat again.
'@
Set-Content -Path (Join-Path $packageDir "README.txt") -Value $readme -Encoding ASCII

# --- zip ---
$zipPath = Join-Path $repoRoot "dist\SpieRibbon-$Label.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path $packageDir -DestinationPath $zipPath

Write-Host "`nPackage staged at: $packageDir" -ForegroundColor Green
Write-Host "Zip created at:    $zipPath" -ForegroundColor Green
