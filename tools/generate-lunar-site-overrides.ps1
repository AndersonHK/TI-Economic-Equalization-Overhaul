[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = Split-Path -Parent $scriptDirectory
$sourceCsv = Join-Path $repositoryRoot `
    'docs\orbits-and-lunar-resources\lunar-site-yield-proposal.csv'
$modFiles = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles'

$siteMetadata = @(
    @{ Name = 'Mare Imbrium'; Latitude = 32.8; Longitude = -15.6; Fabricated = '15' },
    @{ Name = 'Peary Crater'; Latitude = 88.63; Longitude = 24.4; Fabricated = '26' },
    @{ Name = "D'Alembert Crater"; Latitude = 50.8; Longitude = 163.9; Fabricated = '16' },
    @{ Name = 'Copernicus Crater'; Latitude = 9.62; Longitude = -20.08; Fabricated = '23' },
    @{ Name = 'Mare Tranquillitatis'; Latitude = 8.5; Longitude = 31.4; Fabricated = '6' },
    @{ Name = 'Korolev Crater'; Latitude = -4.0; Longitude = -157.4; Fabricated = '2' },
    @{ Name = 'Tycho Crater'; Latitude = -43.31; Longitude = -11.36; Fabricated = '6' },
    @{ Name = 'Shackleton Crater'; Latitude = -89.9; Longitude = 0.0; Fabricated = '54' },
    @{ Name = 'Tsiolkovskiy Crater'; Latitude = -20.4; Longitude = 129.1; Fabricated = '69' },
    @{ Name = 'Plato Crater'; Latitude = 51.62; Longitude = -9.38; Fabricated = '0' },
    @{ Name = 'Humboldt Crater'; Latitude = -27.02; Longitude = 80.96; Fabricated = '0' },
    @{ Name = 'Clavius Crater'; Latitude = -58.62; Longitude = -14.73; Fabricated = '0' },
    @{ Name = 'Aristarchus Plateau'; Latitude = 25.7; Longitude = -47.4; Fabricated = '0' },
    @{ Name = 'Oceanus Procellarum'; Latitude = 5.0; Longitude = -75.0; Fabricated = '0' },
    @{ Name = 'Mare Serenitatis'; Latitude = 28.0; Longitude = 17.5; Fabricated = '0' },
    @{ Name = 'Mare Crisium'; Latitude = 17.0; Longitude = 59.1; Fabricated = '0' },
    @{ Name = 'Marius Hills'; Latitude = 18.0; Longitude = -61.0; Fabricated = '0' },
    @{ Name = 'South Pole-Aitken Basin'; Latitude = -53.0; Longitude = -169.0; Fabricated = '0' },
    @{ Name = "Schr$([char]0x00F6)dinger Basin"; Latitude = -75.0; Longitude = 132.4; Fabricated = '0' },
    @{ Name = 'Compton-Belkovich Volcanic Complex'; Latitude = 61.1; Longitude = 100.3; Fabricated = '0' },
    @{ Name = 'Gagarin Crater'; Latitude = -19.66; Longitude = 149.35; Fabricated = '0' },
    @{ Name = 'Orientale Basin'; Latitude = -19.4; Longitude = -92.8; Fabricated = '0' },
    @{ Name = 'Mare Moscoviense'; Latitude = 27.3; Longitude = 147.9; Fabricated = '0' },
    @{ Name = 'Gruithuisen Domes'; Latitude = 36.3; Longitude = -40.0; Fabricated = '0' },
    @{ Name = "Mons R$([char]0x00FC)mker"; Latitude = 40.8; Longitude = -58.1; Fabricated = '0' },
    @{ Name = 'Hadley-Apennine'; Latitude = 26.1; Longitude = 3.6; Fabricated = '0' },
    @{ Name = 'Taurus-Littrow'; Latitude = 20.2; Longitude = 30.8; Fabricated = '0' },
    @{ Name = 'Kepler Crater'; Latitude = 8.1; Longitude = -38.0; Fabricated = '0' },
    @{ Name = 'Rima Bode'; Latitude = 12.7; Longitude = -3.9; Fabricated = '0' },
    @{ Name = 'Reiner Gamma'; Latitude = 7.5; Longitude = -59.0; Fabricated = '0' }
)

$rows = @(Import-Csv -LiteralPath $sourceCsv)
if ($rows.Count -ne 30 -or $siteMetadata.Count -ne 30) {
    throw "Expected 30 approved yield rows and 30 site metadata rows."
}

$profiles = New-Object System.Collections.Generic.List[object]
$sites = New-Object System.Collections.Generic.List[object]
$siteLocalization = New-Object System.Collections.Generic.List[string]
$profileLocalization = New-Object System.Collections.Generic.List[string]
$resourceNames = @('water', 'volatiles', 'metals', 'nobles', 'fissiles')

for ($index = 0; $index -lt 30; $index++) {
    $row = $rows[$index]
    $metadata = $siteMetadata[$index]
    if ($row.site -ne $metadata.Name) {
        throw "Site order mismatch at row $($index + 1): '$($row.site)' versus '$($metadata.Name)'."
    }

    $siteNumber = $index + 1
    $siteId = "LunaSite$siteNumber"
    $profileId = 'EEOLunarSite{0:D2}Mine' -f $siteNumber
    $profile = [ordered]@{
        dataName = $profileId
        friendlyName = "$($metadata.Name) Mining Profile"
        modifyBySize = $false
    }
    foreach ($resource in $resourceNames) {
        $low = [double]$row.("${resource}_low")
        $high = [double]$row.("${resource}_high")
        if ($low -lt 0 -or $high -lt $low) {
            throw "Invalid $resource band for $($row.site): $low-$high."
        }
        if ($low -eq 0 -and $high -eq 0) {
            $profile["${resource}_mean"] = 0.0
            $profile["${resource}_width"] = 0.0
            $profile["${resource}_min"] = 0.0
            $profile["${resource}_jump"] = 0.0
        }
        else {
            $profile["${resource}_mean"] = ($low + $high) / 2.0
            $profile["${resource}_width"] = $high - $low
            $profile["${resource}_min"] = 0.0
            $profile["${resource}_jump"] = 0.0
        }
    }
    $profiles.Add([pscustomobject]$profile)

    $sites.Add([pscustomobject][ordered]@{
        friendlyName = $metadata.Name
        dataName = $siteId
        parentBodyName = 'Luna'
        X = $index % 6
        Y = [Math]::Floor($index / 6)
        latitude = [double]$metadata.Latitude
        longitude = [double]$metadata.Longitude
        miningProfileName = $profileId
        backgroundPath = 'habmodules/surface_09'
        fabricatedData = $metadata.Fabricated
        Density = 3.34
    })
    $siteLocalization.Add(
        "TIHabSiteTemplate.displayName.$siteId=$($metadata.Name)")
    $profileLocalization.Add(
        "TIMiningProfileTemplate.displayName.$profileId=Lunar")
    $profileLocalization.Add(
        "TIMiningProfileTemplate.description.$profileId=$($row.geological_role). Resource outputs are bounded to the documented site-specific geological range.")
}

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[IO.File]::WriteAllText(
    (Join-Path $modFiles 'TIMiningProfileTemplate.json'),
    (($profiles | ConvertTo-Json -Depth 8) + [Environment]::NewLine),
    $utf8NoBom)
[IO.File]::WriteAllText(
    (Join-Path $modFiles 'TIHabSiteTemplate.json'),
    (($sites | ConvertTo-Json -Depth 8) + [Environment]::NewLine),
    $utf8NoBom)
[IO.File]::WriteAllText(
    (Join-Path $modFiles 'TIHabSiteTemplate.en'),
    (($siteLocalization -join [Environment]::NewLine) + [Environment]::NewLine),
    $utf8NoBom)
[IO.File]::WriteAllText(
    (Join-Path $modFiles 'TIMiningProfileTemplate.en'),
    (($profileLocalization -join [Environment]::NewLine) + [Environment]::NewLine),
    $utf8NoBom)

Write-Host 'PASS: generated 30 lunar site and mining-profile overrides.'
