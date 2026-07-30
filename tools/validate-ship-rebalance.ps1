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
    FuelCellI = @(0.58, 5600, $null, $null)
    FuelCellII = @(0.60, 3600, $null, $null)
    FuelCellIII = @(0.62, 960, $null, $null)
    SolidCoreFissionReactorI = @(0.60, 80, 1, 80)
    SolidCoreFissionReactorII = @(0.625, 68, 3, 204)
    SolidCoreFissionReactorIII = @(0.65, 56, 10, 560)
    SolidCoreFissionReactorIV = @(0.675, 24, 30, 720)
    SolidCoreFissionReactorV = @(0.70, 16, 60, 960)
    SolidCoreFissionReactorVI = @(0.625, 12, 0.75, 9)
    SolidCoreFissionReactorVII = @(0.65, 10, 2, 20)
    SolidCoreFissionReactorVIII = @(0.675, 8, 4, 32)
    SolidCoreFissionReactorIX = @(0.70, 6, 6, 36)
    SolidCoreFissionReactorX = @(0.725, 4, 10, 40)
    MoltenSaltFissionReactorI = @(0.77, 4, 40, 160)
    MoltenSaltFissionReactorII = @(0.78, 3.6, 400, 1440)
    MoltenCoreFissionReactorI = @(0.70, 8, 4, 32)
    MoltenCoreFissionReactorII = @(0.73, 7, 17, 119)
    MoltenCoreFissionReactorIII = @(0.75, 6, 200, 1200)
}
if ($powerOverrides.Count -ne $expectedPower.Count) {
    throw "Power-plant override has $($powerOverrides.Count) rows instead of $($expectedPower.Count)."
}
foreach ($entry in $expectedPower.GetEnumerator()) {
    $row = @($powerOverrides | Where-Object dataName -eq $entry.Key)
    if ($row.Count -ne 1) {
        throw "Power-plant override must contain '$($entry.Key)' exactly once."
    }
    $expectedMaximumOutput = $entry.Value[2]
    if ($null -eq $expectedMaximumOutput) {
        Assert-Properties $row[0] @('dataName', 'efficiency', 'specificPower_tGW') $entry.Key
    }
    else {
        Assert-Properties $row[0] @(
            'dataName', 'maxOutput_GW', 'specificPower_tGW', 'efficiency') $entry.Key
        Assert-Near $row[0].maxOutput_GW $expectedMaximumOutput "$($entry.Key) maximum output"
        Assert-Near `
            ([double]$row[0].maxOutput_GW * [double]$row[0].specificPower_tGW) `
            $entry.Value[3] `
            "$($entry.Key) mass at maximum output"
    }
    Assert-Near $row[0].specificPower_tGW $entry.Value[1] "$($entry.Key) specific mass"
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
    '40mmAutocannon' = 0
    '6-inchCannon' = 2
    '8-inchCannon' = 2
}
if ($gunOverrides.Count -ne $expectedGunCrew.Count) {
    throw "Gun override has $($gunOverrides.Count) rows instead of $($expectedGunCrew.Count)."
}
foreach ($entry in $expectedGunCrew.GetEnumerator()) {
    $row = @($gunOverrides | Where-Object dataName -eq $entry.Key)
    if ($row.Count -ne 1) {
        throw "Gun override must contain '$($entry.Key)' exactly once."
    }
    Assert-Properties $row[0] @('dataName', 'crew') $entry.Key
    Assert-Near $row[0].crew $entry.Value "$($entry.Key) crew"
}

$laserOverrides = Read-JsonArray (Join-Path $modFiles 'TILaserWeaponTemplate.json')
if ($laserOverrides.Count -ne 1 -or
    $laserOverrides[0].dataName -ne 'PointDefenseLaserTurret') {
    throw 'Laser override must contain exactly the Point Defense Laser Turret.'
}
Assert-Properties $laserOverrides[0] @('dataName', 'crew') 'PointDefenseLaserTurret'
Assert-Near $laserOverrides[0].crew 0 'PointDefenseLaserTurret crew'

$magneticOverrides = Read-JsonArray (Join-Path $modFiles 'TIMagneticGunTemplate.json')
$expectedMagneticCrew = [ordered]@{
    LightRailgunBatteryMk1 = 2
    LightRailgunBatteryMk2 = 2
    LightRailgunBatteryMk3 = 2
    RailgunBatteryMk1 = 2
    RailgunBatteryMk2 = 2
    RailgunBatteryMk3 = 2
    LightRailCannonMk1 = 3
    LightRailCannonMk2 = 3
    LightRailCannonMk3 = 3
}
if ($magneticOverrides.Count -ne $expectedMagneticCrew.Count) {
    throw "Magnetic-gun override has $($magneticOverrides.Count) rows instead of $($expectedMagneticCrew.Count)."
}
foreach ($entry in $expectedMagneticCrew.GetEnumerator()) {
    $row = @($magneticOverrides | Where-Object dataName -eq $entry.Key)
    if ($row.Count -ne 1) {
        throw "Magnetic-gun override must contain '$($entry.Key)' exactly once."
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
$vanillaGuns = Read-JsonArray (Join-Path $VanillaTemplatesDir 'TIGunTemplate.json')
foreach ($id in $expectedGunCrew.Keys) {
    if (@($vanillaGuns | Where-Object dataName -eq $id).Count -ne 1) {
        throw "Installed game no longer contains gun '$id'."
    }
}
$vanillaLasers = Read-JsonArray (Join-Path $VanillaTemplatesDir 'TILaserWeaponTemplate.json')
if (@($vanillaLasers | Where-Object dataName -eq 'PointDefenseLaserTurret').Count -ne 1) {
    throw "Installed game no longer contains laser 'PointDefenseLaserTurret'."
}
$vanillaMagneticGuns = Read-JsonArray (Join-Path $VanillaTemplatesDir 'TIMagneticGunTemplate.json')
foreach ($id in $expectedMagneticCrew.Keys) {
    if (@($vanillaMagneticGuns | Where-Object dataName -eq $id).Count -ne 1) {
        throw "Installed game no longer contains magnetic gun '$id'."
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

Write-Host 'PASS: settled ship-rebalance overrides validated.'
