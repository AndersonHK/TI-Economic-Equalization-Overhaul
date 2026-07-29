[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$VanillaTemplatesDir,
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
$modFiles = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles'

function Read-JsonArray {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Missing JSON file '$Path'."
    }
    return @(Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json)
}

function Assert-Properties {
    param(
        [object]$Value,
        [string[]]$Expected,
        [string]$Label
    )

    $actual = @($Value.PSObject.Properties.Name)
    if (($actual -join ';') -ne ($Expected -join ';')) {
        throw "$Label has fields '$($actual -join ', ')' instead of '$($Expected -join ', ')'."
    }
}

function Assert-Near {
    param(
        [double]$Actual,
        [double]$Expected,
        [string]$Label
    )

    if ([Math]::Abs($Actual - $Expected) -gt 0.0000001) {
        throw "$Label is $Actual instead of $Expected."
    }
}

$powerOverrides = Read-JsonArray (Join-Path $modFiles 'TIPowerPlantTemplate.json')
$expectedPower = [ordered]@{
    FuelCellI = @(0.63, 2800)
    FuelCellII = @(0.65, 1800)
    FuelCellIII = @(0.67, 480)
    SolidCoreFissionReactorI = @(0.70, $null)
    SolidCoreFissionReactorII = @(0.725, $null)
    SolidCoreFissionReactorIII = @(0.75, $null)
    SolidCoreFissionReactorIV = @(0.775, $null)
    SolidCoreFissionReactorV = @(0.80, $null)
    SolidCoreFissionReactorVI = @(0.725, $null)
    SolidCoreFissionReactorVII = @(0.75, $null)
    SolidCoreFissionReactorVIII = @(0.775, $null)
    SolidCoreFissionReactorIX = @(0.80, $null)
    SolidCoreFissionReactorX = @(0.825, $null)
}
if ($powerOverrides.Count -ne $expectedPower.Count) {
    throw "Power-plant override has $($powerOverrides.Count) rows instead of $($expectedPower.Count)."
}
foreach ($entry in $expectedPower.GetEnumerator()) {
    $row = @($powerOverrides | Where-Object dataName -eq $entry.Key)
    if ($row.Count -ne 1) {
        throw "Power-plant override must contain '$($entry.Key)' exactly once."
    }
    $expectedSpecificMass = $entry.Value[1]
    if ($null -eq $expectedSpecificMass) {
        Assert-Properties $row[0] @('dataName', 'efficiency') $entry.Key
    }
    else {
        Assert-Properties $row[0] @('dataName', 'efficiency', 'specificPower_tGW') $entry.Key
        Assert-Near $row[0].specificPower_tGW $expectedSpecificMass "$($entry.Key) specific mass"
    }
    Assert-Near $row[0].efficiency $entry.Value[0] "$($entry.Key) efficiency"
}

$heatOverrides = Read-JsonArray (Join-Path $modFiles 'TIHeatSinkTemplate.json')
$expectedHeatIds = @('WaterHeatSink', 'HeavyWaterHeatSink')
if ($heatOverrides.Count -ne $expectedHeatIds.Count) {
    throw 'Heat-sink override must contain exactly Water and Heavy Water.'
}
foreach ($id in $expectedHeatIds) {
    $row = @($heatOverrides | Where-Object dataName -eq $id)
    if ($row.Count -ne 1) {
        throw "Heat-sink override must contain '$id' exactly once."
    }
    Assert-Properties $row[0] @('dataName', 'crew') $id
    Assert-Near $row[0].crew 0 "$id crew"
}

$gunOverrides = Read-JsonArray (Join-Path $modFiles 'TIGunTemplate.json')
$expectedGunCrew = [ordered]@{
    '10-inchCannon' = 3
    '30mmAutocannon' = 0
}
if ($gunOverrides.Count -ne $expectedGunCrew.Count) {
    throw 'Gun override must contain exactly the 10-inch Cannon and 30mm Autocannon.'
}
foreach ($entry in $expectedGunCrew.GetEnumerator()) {
    $row = @($gunOverrides | Where-Object dataName -eq $entry.Key)
    if ($row.Count -ne 1) {
        throw "Gun override must contain '$($entry.Key)' exactly once."
    }
    Assert-Properties $row[0] @('dataName', 'crew') $entry.Key
    Assert-Near $row[0].crew $entry.Value "$($entry.Key) crew"
}

$hullOverrides = Read-JsonArray (Join-Path $modFiles 'TIShipHullTemplate.json')
$expectedHulls = [ordered]@{
    Gunship = @(55, 15, 9719, 171, 3)
    Escort = @(62, 15, 10956, 338, 4)
    Corvette = @(65, 17, 14754, 385, 5)
    Frigate = @(100, 18, 25447, 576, 8)
    Monitor = @(100, 17, 22698, 679, 7)
    Destroyer = @(100, 23, 41548, 873, 9)
}
if ($hullOverrides.Count -ne $expectedHulls.Count) {
    throw "Hull override has $($hullOverrides.Count) rows instead of $($expectedHulls.Count)."
}
foreach ($entry in $expectedHulls.GetEnumerator()) {
    $row = @($hullOverrides | Where-Object dataName -eq $entry.Key)
    if ($row.Count -ne 1) {
        throw "Hull override must contain '$($entry.Key)' exactly once."
    }
    Assert-Properties $row[0] @(
        'dataName', 'length_m', 'width_m', 'volume', 'mass_tons', 'crew') $entry.Key
    $fields = @('length_m', 'width_m', 'volume', 'mass_tons', 'crew')
    for ($i = 0; $i -lt $fields.Count; $i++) {
        Assert-Near $row[0].($fields[$i]) $entry.Value[$i] "$($entry.Key) $($fields[$i])"
    }
    $runtimeVolume = [Math]::PI *
        [Math]::Pow([double]$row[0].width_m / 2.0, 2) *
        [double]$row[0].length_m
    if ([Math]::Abs($runtimeVolume - [double]$row[0].volume) -gt 1.0) {
        throw "$($entry.Key) planning volume does not round to its runtime cylinder."
    }
    $emptyMass = [double]$row[0].mass_tons + 3.0 * [double]$row[0].crew
    $expectedEmptyMass = switch ($entry.Key) {
        Gunship { 180 }
        Escort { 350 }
        Corvette { 400 }
        Frigate { 600 }
        Monitor { 700 }
        Destroyer { 900 }
    }
    Assert-Near $emptyMass $expectedEmptyMass "$($entry.Key) empty mass"
}

$vanillaPower = Read-JsonArray (Join-Path $VanillaTemplatesDir 'TIPowerPlantTemplate.json')
foreach ($id in $expectedPower.Keys) {
    if (@($vanillaPower | Where-Object dataName -eq $id).Count -ne 1) {
        throw "Installed game no longer contains power plant '$id'."
    }
}
$vanillaHulls = Read-JsonArray (Join-Path $VanillaTemplatesDir 'TIShipHullTemplate.json')
foreach ($id in $expectedHulls.Keys) {
    if (@($vanillaHulls | Where-Object dataName -eq $id).Count -ne 1) {
        throw "Installed game no longer contains hull '$id'."
    }
}

$driveOverride = Join-Path $modFiles 'TIDriveTemplate.json'
if (Test-Path -LiteralPath $driveOverride) {
    throw 'The settled slice defers drive changes; TIDriveTemplate.json must not be packaged.'
}

Write-Host 'PASS: settled low-tech ship rebalance overrides validated.'
