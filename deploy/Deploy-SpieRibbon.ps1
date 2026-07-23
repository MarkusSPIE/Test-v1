<#
.SYNOPSIS
    Installs SPIE Ribbon (host + modules) for the Revit versions on this machine.
    No admin rights required.

.DESCRIPTION
    Consumes a staged package layout produced by Build-Package.ps1:

        <SourceRoot>\host\<year>\      -> %AppData%\SpieRibbon\<year>
        <SourceRoot>\modules\<year>\   -> %AppData%\SpieRibbon\Modules\<year>

    and writes a per-user .addin manifest into %AppData%\Autodesk\Revit\Addins\<year>.
    All target locations are per-user, so no elevation is needed.

    By default SourceRoot is the folder this script lives in (the package root).

.EXAMPLE
    .\Deploy-SpieRibbon.ps1
#>
param(
    [string]$SourceRoot = $PSScriptRoot
)

$ErrorActionPreference = "Stop"

$AssemblyBaseName  = "SpieRibbon"
$FullClassName     = "SpieRibbon.Application"
$AddInId           = "8ab1c8bb-9ab9-441b-925c-6c0841b6ff30"
$VendorId          = "SPIE"
$VendorDescription = "SPIE"

# Plain Hashtable (not [ordered]: OrderedDictionary int-key lookups collide with its
# positional this[int] indexer and silently return nothing for keys like 2024).
$years = @(2024, 2025, 2026)

$installedAny = $false

foreach ($year in ($years | Sort-Object)) {
    $revitExe = "C:\Program Files\Autodesk\Revit $year\Revit.exe"
    if (-not (Test-Path $revitExe)) {
        Write-Host "Revit $year not installed on this machine - skipping." -ForegroundColor DarkGray
        continue
    }

    $hostSrc = Join-Path $SourceRoot "host\$year"
    if (-not (Test-Path $hostSrc)) {
        Write-Warning "Revit $year is installed, but no host build was found at '$hostSrc'."
        continue
    }

    # --- host ---
    $hostDest = Join-Path $env:AppData "SpieRibbon\$year"
    New-Item -ItemType Directory -Force -Path $hostDest | Out-Null
    try {
        # Clean first so stale files (e.g. a dependency dropped in a later version) don't linger.
        Remove-Item -Path (Join-Path $hostDest '*') -Recurse -Force -ErrorAction Stop
        Copy-Item -Path (Join-Path $hostSrc '*') -Destination $hostDest -Recurse -Force -ErrorAction Stop
    } catch {
        Write-Warning "Could not update Revit ${year}: $($_.Exception.Message)"
        Write-Warning "If Revit $year is currently running, close it and re-run this script."
        continue
    }

    # --- modules (optional) ---
    $modSrc = Join-Path $SourceRoot "modules\$year"
    if (Test-Path $modSrc) {
        $modDest = Join-Path $env:AppData "SpieRibbon\Modules\$year"
        New-Item -ItemType Directory -Force -Path $modDest | Out-Null
        try {
            Remove-Item -Path (Join-Path $modDest '*') -Recurse -Force -ErrorAction Stop
            Copy-Item -Path (Join-Path $modSrc '*') -Destination $modDest -Recurse -Force -ErrorAction Stop
        } catch {
            Write-Warning "Host updated, but modules for Revit $year could not be updated: $($_.Exception.Message)"
        }
    }

    # --- .addin manifest ---
    $addinFolder = Join-Path $env:AppData "Autodesk\Revit\Addins\$year"
    New-Item -ItemType Directory -Force -Path $addinFolder | Out-Null

    $assemblyPath = Join-Path $hostDest "$AssemblyBaseName.dll"
    $addinPath    = Join-Path $addinFolder "$AssemblyBaseName.addin"

    $manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>SPIE Ribbon</Name>
    <Assembly>$assemblyPath</Assembly>
    <AddInId>$AddInId</AddInId>
    <FullClassName>$FullClassName</FullClassName>
    <VendorId>$VendorId</VendorId>
    <VendorDescription>$VendorDescription</VendorDescription>
  </AddIn>
</RevitAddIns>
"@

    Set-Content -Path $addinPath -Value $manifest -Encoding UTF8
    Write-Host "Installed SPIE Ribbon for Revit $year -> $assemblyPath" -ForegroundColor Green
    $installedAny = $true
}

if (-not $installedAny) {
    Write-Warning "Nothing was installed. Either no supported Revit version is installed here, or no matching build was found under '$SourceRoot'."
} else {
    Write-Host "`nDone. Restart Revit for the changes to take effect." -ForegroundColor Cyan
}
