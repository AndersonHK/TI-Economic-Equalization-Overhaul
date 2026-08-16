[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$VanillaTemplatesDir,
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
$culture = [Globalization.CultureInfo]::InvariantCulture
$reportRoot = Join-Path $RepositoryRoot 'docs\ship-balance-research'
$csvPath = Join-Path $reportRoot 'tables\hull-variant-volume-and-slots.csv'
$jsonPath = Join-Path $reportRoot 'tables\hull-variant-volume-and-slots.json'
$reportPath = Join-Path $reportRoot 'hull-utility-slot-volume-report.md'
$imageRoot = Join-Path $reportRoot 'hull-variants'
$runtimeVolumePath = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\Config\hull-variant-main-volumes.csv'
$runtimeDriveScalePath = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\Config\hull-variant-drive-scales.csv'
$templatePath = Join-Path $VanillaTemplatesDir 'TIShipHullTemplate.json'
$overridePath = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\TIShipHullTemplate.json'
$streamingAssetsDir = Split-Path -Parent $VanillaTemplatesDir
$gameRoot = Split-Path -Parent (Split-Path -Parent $streamingAssetsDir)
$shipsBundlePath = Join-Path $streamingAssetsDir 'AssetBundles\ships'
$dlcBundlePath = Join-Path $gameRoot 'DLC_Content\DarkSkies\AssetBundles\ships_prm'

foreach ($requiredPath in @(
    $csvPath,
    $jsonPath,
    $reportPath,
    $runtimeVolumePath,
    $runtimeDriveScalePath,
    $templatePath,
    $overridePath,
    $shipsBundlePath,
    $dlcBundlePath,
    (Join-Path $imageRoot 'human-contact-sheet.png'),
    (Join-Path $imageRoot 'alien-contact-sheet.png'))) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Hull-variant report validation is missing '$requiredPath'."
    }
}

$templates = Get-Content -LiteralPath $templatePath -Raw | ConvertFrom-Json
$overrides = Get-Content -LiteralPath $overridePath -Raw | ConvertFrom-Json
$overrideByName = @{}
foreach ($override in $overrides) {
    $overrideByName[[string]$override.dataName] = $override
}
$csvRows = @(Import-Csv -LiteralPath $csvPath)
$evidence = Get-Content -LiteralPath $jsonPath -Raw | ConvertFrom-Json
$evidenceRows = @($evidence.rows)
$expectedAppearanceCount = ($templates | ForEach-Object { @($_.modelResource).Count } |
    Measure-Object -Sum).Sum

if ($templates.Count -ne 28 -or $expectedAppearanceCount -ne 64) {
    throw "Installed hull catalog is $($templates.Count) templates/$expectedAppearanceCount appearances; expected the documented 28/64 snapshot."
}
if ($csvRows.Count -ne $expectedAppearanceCount -or
    $evidenceRows.Count -ne $expectedAppearanceCount) {
    throw "Hull-variant artifacts have $($csvRows.Count) CSV/$($evidenceRows.Count) JSON rows; expected $expectedAppearanceCount."
}
if ([int]$evidence.metadata.template_count -ne $templates.Count -or
    [int]$evidence.metadata.appearance_count -ne $expectedAppearanceCount -or
    [int]$evidence.metadata.human_template_count -ne 13 -or
    [int]$evidence.metadata.alien_template_count -ne 15) {
    throw 'Hull-variant evidence metadata does not match the installed catalog.'
}

$expectedHashes = [ordered]@{
    hull_templates = (Get-FileHash -LiteralPath $templatePath -Algorithm SHA256).Hash
    ships_bundle = (Get-FileHash -LiteralPath $shipsBundlePath -Algorithm SHA256).Hash
    ships_prm_bundle = (Get-FileHash -LiteralPath $dlcBundlePath -Algorithm SHA256).Hash
    mod_hull_overrides = (Get-FileHash -LiteralPath $overridePath -Algorithm SHA256).Hash
}
foreach ($entry in $expectedHashes.GetEnumerator()) {
    if ([string]$evidence.metadata.source_sha256.($entry.Key) -ne $entry.Value) {
        throw "Hull-variant source hash '$($entry.Key)' is stale. Regenerate the report."
    }
}

$expectedPairs = @{}
foreach ($template in $templates) {
    $modelResources = @($template.modelResource)
    for ($appearanceIndex = 0; $appearanceIndex -lt $modelResources.Count; $appearanceIndex++) {
        $key = "$($template.dataName)|$appearanceIndex"
        $expectedPairs[$key] = [pscustomobject]@{
            Template = $template
            AppearanceIndex = $appearanceIndex
            ModelResource = [string]$modelResources[$appearanceIndex]
        }
    }
}

$runtimeDriveScales = @{}
foreach ($line in Get-Content -LiteralPath $runtimeDriveScalePath) {
    $trimmed = $line.Trim()
    if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed.StartsWith('#') -or
        $trimmed.StartsWith('dataName,')) {
        continue
    }
    $columns = @($trimmed.Split(','))
    if ($columns.Count -ne 3 -or $runtimeDriveScales.ContainsKey($columns[0])) {
        throw "Malformed or duplicate runtime drive-scale row '$trimmed'."
    }
    $runtimeDriveScales[$columns[0]] = [pscustomobject]@{
        DeLaval = @($columns[1].Split('|'))
        Magnetic = @($columns[2].Split('|'))
    }
}
$humanDriveTemplates = @($templates | Where-Object {
    -not [bool]$_.alien -and [string]$_.dataName -ne 'STOFighter'
})
if ($runtimeDriveScales.Count -ne 12 -or
    $runtimeDriveScales.Count -ne $humanDriveTemplates.Count) {
    throw "Runtime drive-scale catalog has $($runtimeDriveScales.Count) hulls; expected 12."
}
foreach ($template in $humanDriveTemplates) {
    $dataName = [string]$template.dataName
    if (-not $runtimeDriveScales.ContainsKey($dataName)) {
        throw "Runtime drive-scale catalog is missing '$dataName'."
    }
    $entry = $runtimeDriveScales[$dataName]
    $appearanceCount = @($template.modelResource).Count
    if ($entry.DeLaval.Count -ne $appearanceCount -or
        $entry.Magnetic.Count -ne $appearanceCount) {
        throw "Runtime drive-scale appearance count mismatch for '$dataName'."
    }
    foreach ($family in @('DeLaval', 'Magnetic')) {
        foreach ($valueText in @($entry.$family)) {
            if ([double]::Parse([string]$valueText, $culture) -le 0) {
                throw "Runtime drive-scale catalog has a non-positive $family value for '$dataName'."
            }
        }
    }
}
$gunshipScales = $runtimeDriveScales['Gunship']
if ([Math]::Abs([double]::Parse($gunshipScales.DeLaval[0], $culture) - 1.0) -gt 0.000001 -or
    [Math]::Abs([double]::Parse($gunshipScales.Magnetic[0], $culture) - 1.0) -gt 0.000001 -or
    [Math]::Abs([double]::Parse($gunshipScales.DeLaval[2], $culture) - 0.668230) -gt 0.000001 -or
    [Math]::Abs([double]::Parse($gunshipScales.Magnetic[2], $culture) - 0.397033) -gt 0.000001) {
    throw 'Runtime drive-scale catalog does not preserve the measured Gunship references.'
}

$seenPairs = @{}
foreach ($row in $csvRows) {
    $key = "$($row.dataName)|$($row.appearanceIndex)"
    if (-not $expectedPairs.ContainsKey($key)) {
        throw "Hull-variant CSV contains unknown pair '$key'."
    }
    if ($seenPairs.ContainsKey($key)) {
        throw "Hull-variant CSV duplicates '$key'."
    }
    $seenPairs[$key] = $true
    $expected = $expectedPairs[$key]
    $template = $expected.Template
    if ($row.modelResource -ne $expected.ModelResource) {
        throw "Hull-variant CSV model resource mismatch for '$key'."
    }
    $nose = [int]$template.noseHardpoints
    $hull = [int]$template.hullHardpoints
    $utility = [int]$template.internalModules
    if ([int]$row.noseHardpoints -ne $nose -or
        [int]$row.hullHardpoints -ne $hull -or
        [int]$row.utilitySlots -ne $utility -or
        [int]$row.weaponSlots -ne ($nose + $hull) -or
        [int]$row.countedSlots -ne ($nose + $hull + $utility)) {
        throw "Hull-variant CSV slot mismatch for '$key'."
    }
    foreach ($field in @(
        'mainHullX_m',
        'mainHullY_m',
        'mainHullLength_m',
        'mainHullEllipticalEnvelope_m3',
        'runtimeCylinder_m3')) {
        if ([double]::Parse([string]$row.$field, $culture) -le 0) {
            throw "Hull-variant CSV has non-positive '$field' for '$key'."
        }
    }
    if ([int]$row.includedMeshCount -le 0) {
        throw "Hull-variant CSV has no included meshes for '$key'."
    }
    $imagePath = Join-Path $imageRoot $row.imageFile
    if (-not (Test-Path -LiteralPath $imagePath -PathType Leaf) -or
        (Get-Item -LiteralPath $imagePath).Length -le 0) {
        throw "Hull-variant CSV is missing the image for '$key'."
    }
}
if ($seenPairs.Count -ne $expectedPairs.Count) {
    throw 'Hull-variant CSV does not cover every installed hull appearance.'
}

$runtimeVolumes = @{}
foreach ($line in Get-Content -LiteralPath $runtimeVolumePath) {
    $trimmed = $line.Trim()
    if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed.StartsWith('#') -or
        $trimmed.StartsWith('dataName,')) {
        continue
    }
    $separator = $trimmed.IndexOf(',')
    if ($separator -le 0) {
        throw "Malformed runtime hull-volume row '$trimmed'."
    }
    $dataName = $trimmed.Substring(0, $separator)
    if ($runtimeVolumes.ContainsKey($dataName)) {
        throw "Runtime hull-volume catalog duplicates '$dataName'."
    }
    $runtimeVolumes[$dataName] = @($trimmed.Substring($separator + 1).Split('|'))
}
if ($runtimeVolumes.Count -ne $templates.Count) {
    throw "Runtime hull-volume catalog has $($runtimeVolumes.Count) hulls; expected $($templates.Count)."
}
foreach ($template in $templates) {
    $dataName = [string]$template.dataName
    if (-not $runtimeVolumes.ContainsKey($dataName)) {
        throw "Runtime hull-volume catalog is missing '$dataName'."
    }
    $values = @($runtimeVolumes[$dataName])
    $sourceRows = @($csvRows | Where-Object { $_.dataName -eq $dataName } |
        Sort-Object { [int]$_.appearanceIndex })
    if ($values.Count -ne $sourceRows.Count) {
        throw "Runtime hull-volume appearance count mismatch for '$dataName'."
    }
    for ($appearanceIndex = 0; $appearanceIndex -lt $values.Count; $appearanceIndex++) {
        $runtimeValue = [double]::Parse([string]$values[$appearanceIndex], $culture)
        $sourceValue = [double]::Parse(
            [string]$sourceRows[$appearanceIndex].mainHullEllipticalEnvelope_m3,
            $culture)
        if ([Math]::Abs($runtimeValue - $sourceValue) -gt 0.000001) {
            throw "Runtime hull-volume value mismatch for '$dataName|$appearanceIndex'."
        }
    }
}

foreach ($row in $evidenceRows) {
    $key = "$($row.data_name)|$($row.appearance_index)"
    if (-not $expectedPairs.ContainsKey($key)) {
        throw "Hull-variant JSON contains unknown pair '$key'."
    }
    if (@($row.included_mesh_paths).Count -le 0) {
        throw "Hull-variant JSON has no included mesh paths for '$key'."
    }
    foreach ($meshPath in @($row.included_mesh_paths)) {
        if ([string]$meshPath -match '(?i)(^|/)drive|radiator|_rads|_rad_|engine|thruster|reactor') {
            throw "Hull-variant JSON includes machinery path '$meshPath' for '$key'."
        }
    }
}

$imageFiles = @(Get-ChildItem -LiteralPath $imageRoot -Filter '*.png' -File)
if ($imageFiles.Count -ne ($expectedAppearanceCount + 2)) {
    throw "Hull-variant image directory has $($imageFiles.Count) PNGs; expected $($expectedAppearanceCount + 2)."
}
$reportText = Get-Content -LiteralPath $reportPath -Raw
foreach ($requiredText in @(
    '**28 hull templates**',
    '**64 graphical appearances**',
    'Vmain-envelope = pi / 4 * X * Y * L',
    'STOFighter',
    'AlienMothership',
    'does not recommend or implement new counts yet')) {
    if (-not $reportText.Contains($requiredText)) {
        throw "Hull-variant report is missing required text '$requiredText'."
    }
}

Write-Host 'PASS: 28 hull templates, 64 graphical appearances, runtime volume and drive-scale catalogs, source hashes, slots, volumes, mesh exclusions, and 66 PNGs validated.'
