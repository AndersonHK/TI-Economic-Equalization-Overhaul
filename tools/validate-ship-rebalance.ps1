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
    SolidCoreFissionReactorI = @(0.575, 160, 2, 320)
    SolidCoreFissionReactorII = @(0.60, 136, 3, 408)
    SolidCoreFissionReactorIII = @(0.625, 112, 10, 1120)
    SolidCoreFissionReactorIV = @(0.65, 48, 30, 1440)
    SolidCoreFissionReactorV = @(0.675, 32, 60, 1920)
    SolidCoreFissionReactorVI = @(0.60, 24, 0.75, 18)
    SolidCoreFissionReactorVII = @(0.625, 20, 2, 40)
    SolidCoreFissionReactorVIII = @(0.65, 16, 4, 64)
    SolidCoreFissionReactorIX = @(0.675, 12, 6, 72)
    SolidCoreFissionReactorX = @(0.70, 8, 10, 80)
    MoltenSaltFissionReactorI = @(0.725, 10, 40, 400)
    MoltenSaltFissionReactorII = @(0.75, 8, 400, 3200)
    MoltenCoreFissionReactorI = @(0.675, 16, 4, 64)
    MoltenCoreFissionReactorII = @(0.705, 14, 17, 238)
    MoltenCoreFissionReactorIII = @(0.725, 12, 200, 2400)
    VaporCoreFissionReactorI = @(0.87, 8, $null, $null)
    VaporCoreFissionReactorII = @(0.88, 6, $null, $null)
    VaporCoreFissionReactorIII = @(0.89, 5, $null, $null)
    GasCoreFissionReactorI = @(0.87, 20, $null, $null)
    GasCoreFissionReactorII = @(0.89, 16, $null, $null)
    GasCoreFissionReactorIII = @(0.91, 10, $null, $null)
    GasCoreFissionReactorIV = @(0.92, 7, $null, $null)
    GasCoreFissionReactorV = @(0.93, 6, $null, $null)
    GasCoreFissionReactorVI = @(0.94, 4, $null, $null)
    AlienHybridConfinementFusionReactor = @(0.995, 0.5, 5000, 2500)
    AlienAdvancedHybridConfinementFusionReactor = @(0.998, 0.175, 32000, 5600)
    AlienSuperAdvancedHybridConfinementFusionReactor = @(0.9995, 0.025, 107550, 2688.75)
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
$expectedFootprintHeatIds = @(
    'HeavyWaterHeatSink',
    'HeavyPotassiumHeatSink',
    'HeavySodiumHeatSink',
    'HeavyLithiumHeatSink',
    'HeavyMoltenSaltHeatSink',
    'HeavyExoticHeatSink'
)
if ($heatOverrides.Count -ne 7) {
    throw 'Heat-sink override must contain two crew overrides and six large-footprint overrides.'
}
foreach ($id in $expectedHeatIds) {
    $row = @($heatOverrides | Where-Object dataName -eq $id)
    if ($row.Count -ne 1) {
        throw "Heat-sink override must contain '$id' exactly once."
    }
    $expectedProperties = @('dataName', 'crew')
    if ($id -eq 'HeavyWaterHeatSink') {
        $expectedProperties += 'utilityFootprint'
    }
    Assert-Properties $row[0] $expectedProperties $id
    Assert-Near $row[0].crew 0 "$id crew"
}
foreach ($id in $expectedFootprintHeatIds) {
    $row = @($heatOverrides | Where-Object dataName -eq $id)
    if ($row.Count -ne 1 -or $row[0].utilityFootprint -ne 'TwoHorizontal') {
        throw "Large heat sink '$id' must have a TwoHorizontal footprint."
    }
    if ($id -ne 'HeavyWaterHeatSink') {
        Assert-Properties $row[0] @('dataName', 'utilityFootprint') $id
    }
}

$gunOverrides = Read-JsonArray (Join-Path $modFiles 'TIGunTemplate.json')
$expectedGunCrew = [ordered]@{
    '10-inchCannon' = 3
    '30mmAutocannon' = 0
    '40mmAutocannon' = 0
    '6-inchCannon' = 2
    '8-inchCannon' = 2
}
$expectedGunPower = [ordered]@{
    '10-inchCannon' = @(2.2, 1.0)
    '30mmAutocannon' = @(0.085, 1.0)
    '40mmAutocannon' = @(8.7, 0.9)
    '6-inchCannon' = @(0.675, 1.0)
    '8-inchCannon' = @(1.40625, 1.0)
}
$expectedGunDiameters = [ordered]@{
    '10-inchCannon' = 254.0
    '30mmAutocannon' = 30.0
    '35mmAutocannon' = 35.0
    '40mmNoseAutocannon' = 40.0
    '40mmAutocannon' = 40.0
    '6-inchCannon' = 152.4
    '8-inchCannon' = 203.2
    '12-inchCannon' = 304.8
}
$expectedGunBalance = [ordered]@{
    '10-inchCannon' = @(180.0, $null, $null, 400.0)
    '30mmAutocannon' = @(1.75, 0.25, 1.0, $null)
    '35mmAutocannon' = @(2.6, 0.4, 1.75, 8.8)
    '40mmNoseAutocannon' = @(2.8, 0.4, 1.75, 8.8)
    '40mmAutocannon' = @(2.8, 0.5, 1.75, 8.8)
    '6-inchCannon' = @(40.0, $null, $null, 90.0)
    '8-inchCannon' = @(90.0, $null, $null, 200.0)
    '12-inchCannon' = @(320.0, $null, $null, 640.0)
}
if ($gunOverrides.Count -ne $expectedGunDiameters.Count) {
    throw "Gun override has $($gunOverrides.Count) rows instead of $($expectedGunDiameters.Count)."
}
foreach ($entry in $expectedGunDiameters.GetEnumerator()) {
    $row = @($gunOverrides | Where-Object dataName -eq $entry.Key)
    if ($row.Count -ne 1) {
        throw "Gun override must contain '$($entry.Key)' exactly once."
    }
    if ($expectedGunBalance.Contains($entry.Key)) {
        $hasCrewAndPower = $expectedGunCrew.Contains($entry.Key)
        if ($null -eq $expectedGunBalance[$entry.Key][1]) {
            $expectedProperties = if ($hasCrewAndPower) {
                @('dataName', 'crew', 'powerUse_MJ', 'efficiency',
                    'projectileDiameter_mm', 'ammoMass_kg', 'warheadMass_kg')
            }
            else {
                @('dataName', 'projectileDiameter_mm', 'ammoMass_kg',
                    'warheadMass_kg')
            }
        }
        elseif ($hasCrewAndPower) {
            $expectedProperties = if ($null -ne
                $expectedGunBalance[$entry.Key][3]) {
                @('dataName', 'crew', 'powerUse_MJ', 'efficiency',
                    'projectileDiameter_mm', 'ammoMass_kg',
                    'warheadMass_kg', 'intraSalvoCooldown_s', 'cooldown_s')
            }
            else {
                @('dataName', 'crew', 'powerUse_MJ', 'efficiency',
                    'projectileDiameter_mm', 'warheadMass_kg',
                    'intraSalvoCooldown_s', 'cooldown_s')
            }
        }
        else {
            $expectedProperties = @(
                'dataName', 'projectileDiameter_mm', 'ammoMass_kg',
                'warheadMass_kg', 'intraSalvoCooldown_s', 'cooldown_s')
        }
        Assert-Properties $row[0] $expectedProperties $entry.Key
        if ($hasCrewAndPower) {
            Assert-Near $row[0].crew $expectedGunCrew[$entry.Key] `
                "$($entry.Key) crew"
            Assert-Near $row[0].powerUse_MJ $expectedGunPower[$entry.Key][0] `
                "$($entry.Key) useful electrical work"
            Assert-Near $row[0].efficiency $expectedGunPower[$entry.Key][1] `
                "$($entry.Key) electrical efficiency"
        }
        Assert-Near $row[0].warheadMass_kg $expectedGunBalance[$entry.Key][0] `
            "$($entry.Key) damaging projectile mass"
        if ($null -ne $expectedGunBalance[$entry.Key][3]) {
            Assert-Near $row[0].ammoMass_kg $expectedGunBalance[$entry.Key][3] `
                "$($entry.Key) complete projectile mass"
            if ([double]$row[0].warheadMass_kg -gt
                [double]$row[0].ammoMass_kg) {
                throw "$($entry.Key) damaging mass exceeds complete ammunition mass."
            }
        }
        if ($null -ne $expectedGunBalance[$entry.Key][1]) {
            Assert-Near $row[0].intraSalvoCooldown_s $expectedGunBalance[$entry.Key][1] `
                "$($entry.Key) intra-salvo delay"
            Assert-Near $row[0].cooldown_s $expectedGunBalance[$entry.Key][2] `
                "$($entry.Key) cycle reload"
        }
    }
    Assert-Near $row[0].projectileDiameter_mm $entry.Value `
        "$($entry.Key) projectile diameter"
}

$laserOverrides = Read-JsonArray (Join-Path $modFiles 'TILaserWeaponTemplate.json')
if ($laserOverrides.Count -ne 1 -or
    $laserOverrides[0].dataName -ne 'PointDefenseLaserTurret') {
    throw 'Laser override must contain exactly the Point Defense Laser Turret.'
}
Assert-Properties $laserOverrides[0] @('dataName', 'crew') 'PointDefenseLaserTurret'
Assert-Near $laserOverrides[0].crew 0 'PointDefenseLaserTurret crew'

$magneticOverrides = Read-JsonArray (Join-Path $modFiles 'TIMagneticGunTemplate.json')
$expectedHumanRails = [ordered]@{
    LightRailgunBatteryMk1 = @(2, 14.0, 10.5, 8.0)
    LightRailgunBatteryMk2 = @(2, 14.0, 11.2, 6.0)
    LightRailgunBatteryMk3 = @(2, 14.0, 12.25, 4.0)
    RailgunBatteryMk1 = @(2, 30.0, 22.5, 12.0)
    RailgunBatteryMk2 = @(2, 30.0, 24.0, 9.0)
    RailgunBatteryMk3 = @(2, 30.0, 26.25, 6.0)
    LightRailCannonMk1 = @(3, 37.5, 28.125, 16.0)
    LightRailCannonMk2 = @(3, 37.5, 30.0, 12.0)
    LightRailCannonMk3 = @(3, 37.5, 32.8125, 8.0)
}

$expectedHumanCoils = [ordered]@{
    LightCoilgunBatteryMk1 = @(13, 10, 5.6, 600, 28, 4)
    LightCoilgunBatteryMk2 = @(13, 10, 6.8, 650, 18, 4)
    LightCoilgunBatteryMk3 = @(13, 10, 7.9, 750, 8, 4)
    CoilgunBatteryMk1 = @(25, 19, 6.8, 800, 28, 6)
    CoilgunBatteryMk2 = @(25, 20, 7.9, 850, 18, 6)
    CoilgunBatteryMk3 = @(25, 22, 9, 900, 8, 6)
    HeavyCoilgunBatteryMk1 = @(50, 38, 7.9, 1000, 28, 6)
    HeavyCoilgunBatteryMk2 = @(50, 40, 9, 1050, 18, 6)
    HeavyCoilgunBatteryMk3 = @(50, 44, 10.1, 1100, 8, 6)
    LightCoilCannonMk1 = @(31, 23, 6.8, 650, 34, 5)
    LightCoilCannonMk2 = @(31, 25, 7.9, 750, 22, 5)
    LightCoilCannonMk3 = @(31, 27, 10.1, 800, 10, 5)
    CoilCannonMk1 = @(63, 47, 7.9, 850, 34, 8)
    CoilCannonMk2 = @(63, 50, 9, 900, 22, 8)
    CoilCannonMk3 = @(63, 55, 11.3, 1000, 10, 8)
    HeavyCoilCannonMk1 = @(94, 71, 8.5, 1000, 34, 8)
    HeavyCoilCannonMk2 = @(94, 75, 9.6, 1050, 22, 8)
    HeavyCoilCannonMk3 = @(94, 82, 11.9, 1100, 10, 8)
    SpinalCoilerMk1 = @(125, 94, 9, 1100, 34, 8)
    SpinalCoilerMk2 = @(125, 100, 10.1, 1150, 22, 8)
    SpinalCoilerMk3 = @(125, 109, 12.4, 1250, 10, 8)
    HeavySiegeCoilerMk1 = @(938, 704, 4.3, 1000, 24, 22)
    HeavySiegeCoilerMk2 = @(938, 750, 4.8, 1050, 19, 15)
    HeavySiegeCoilerMk3 = @(938, 821, 5.9, 1100, 12, 11)
    SpinalSiegeCoilerMk1 = @(1250, 938, 4.5, 1100, 24, 22)
    SpinalSiegeCoilerMk2 = @(1250, 1000, 5.1, 1150, 19, 15)
    SpinalSiegeCoilerMk3 = @(1250, 1094, 6.3, 1250, 12, 11)
}
$expectedAlienMags = [ordered]@{
    AlienLightMagBattery = @(6.3, 650, 0.60, 19, 16, 9, 3)
    AlienMagBattery = @(7.5, 850, 0.60, 38, 32, 11, 4)
    AlienHeavyMagBattery = @(8.8, 1050, 0.60, 75, 64, 13, 5)
    AlienMiniLightMagCannon = @(7.5, 750, 0.60, 50, 43, 18, 3)
    AlienLightMagCannon = @(7.5, 750, 0.60, 50, 43, 18, 3)
    AlienMagCannon = @(8.8, 900, 0.60, 100, 85, 22, 4)
    AlienHeavyMagCannon = @(10.4, 1050, 0.60, 150, 128, 32, 6)
    AlienSpinalMagCannon = @(12.5, 1150, 0.60, 200, 170, 43, 8)
    AdvancedAlienLightMagBattery = @(8.4, 800, 0.75, 19, 17, 4, 3)
    AdvancedAlienMagBattery = @(9.8, 1000, 0.75, 38, 34, 5, 4)
    AdvancedAlienHeavyMagBattery = @(11.8, 1150, 0.75, 75, 68, 6, 5)
    AdvancedAlienLightMagCannon = @(10.8, 850, 0.75, 50, 45, 13, 3)
    AdvancedAlienMagCannon = @(12.5, 1050, 0.75, 100, 90, 16, 4)
    AdvancedAlienHeavyMagCannon = @(15.6, 1150, 0.75, 150, 135, 23, 6)
    AdvancedAlienSpinalMagCannon = @(18.8, 1300, 0.75, 200, 180, 31, 8)
    Gen3AlienLightMagBattery = @(10.3, 850, 0.85, 35, 30, 4, 3)
    Gen3AlienMagBattery = @(12.9, 1050, 0.85, 69, 60, 5, 3)
    Gen3AlienHeavyMagBattery = @(15.5, 1250, 0.85, 138, 120, 6, 4)
    Gen3AlienLightMagCannon = @(13.5, 1000, 0.85, 93, 80, 6, 3)
    Gen3AlienMagCannon = @(16.4, 1100, 0.85, 184, 160, 8, 3)
    Gen3AlienHeavyMagCannon = @(20.6, 1250, 0.85, 368, 320, 14, 4)
    Gen3AlienSpinalMagCannon = @(24.8, 1350, 0.85, 736, 640, 23, 5)
}
$expectedMagneticCount = $expectedHumanRails.Count + $expectedHumanCoils.Count + $expectedAlienMags.Count
if ($magneticOverrides.Count -ne $expectedMagneticCount) {
    throw "Magnetic-gun override has $($magneticOverrides.Count) rows instead of $expectedMagneticCount."
}
foreach ($entry in $expectedHumanRails.GetEnumerator()) {
    $row = @($magneticOverrides | Where-Object dataName -eq $entry.Key)
    if ($row.Count -ne 1) {
        throw "Magnetic-gun override must contain '$($entry.Key)' exactly once."
    }
    Assert-Properties $row[0] @(
        'dataName', 'crew', 'ammoMass_kg', 'warheadMass_kg', 'cooldown_s') $entry.Key
    Assert-Near $row[0].crew $entry.Value[0] "$($entry.Key) crew"
    Assert-Near $row[0].ammoMass_kg $entry.Value[1] "$($entry.Key) complete projectile mass"
    Assert-Near $row[0].warheadMass_kg $entry.Value[2] "$($entry.Key) damaging projectile mass"
    Assert-Near $row[0].cooldown_s $entry.Value[3] "$($entry.Key) cycle reload"
}
foreach ($entry in $expectedHumanCoils.GetEnumerator()) {
    $row = @($magneticOverrides | Where-Object dataName -eq $entry.Key)
    if ($row.Count -ne 1) {
        throw "Magnetic-gun override must contain '$($entry.Key)' exactly once."
    }
    Assert-Properties $row[0] @(
        'dataName', 'ammoMass_kg', 'warheadMass_kg', 'muzzleVelocity_kps',
        'targetingRange_km', 'cooldown_s', 'intraSalvoCooldown_s') $entry.Key
    Assert-Near $row[0].ammoMass_kg $entry.Value[0] "$($entry.Key) complete projectile mass"
    Assert-Near $row[0].warheadMass_kg $entry.Value[1] "$($entry.Key) damaging projectile mass"
    Assert-Near $row[0].muzzleVelocity_kps $entry.Value[2] "$($entry.Key) muzzle velocity"
    Assert-Near $row[0].targetingRange_km $entry.Value[3] "$($entry.Key) targeting range"
    Assert-Near $row[0].cooldown_s $entry.Value[4] "$($entry.Key) cycle reload"
    Assert-Near $row[0].intraSalvoCooldown_s $entry.Value[5] "$($entry.Key) intra-salvo reload"
    if ([double]$row[0].warheadMass_kg -gt [double]$row[0].ammoMass_kg) {
        throw "$($entry.Key) damaging mass exceeds complete projectile mass."
    }
}
foreach ($entry in $expectedAlienMags.GetEnumerator()) {
    $row = @($magneticOverrides | Where-Object dataName -eq $entry.Key)
    if ($row.Count -ne 1) {
        throw "Magnetic-gun override must contain '$($entry.Key)' exactly once."
    }
    Assert-Properties $row[0] @(
        'dataName', 'muzzleVelocity_kps', 'targetingRange_km', 'efficiency',
        'ammoMass_kg', 'warheadMass_kg', 'cooldown_s',
        'intraSalvoCooldown_s') $entry.Key
    Assert-Near $row[0].muzzleVelocity_kps $entry.Value[0] "$($entry.Key) muzzle velocity"
    Assert-Near $row[0].targetingRange_km $entry.Value[1] "$($entry.Key) targeting range"
    Assert-Near $row[0].efficiency $entry.Value[2] "$($entry.Key) efficiency"
    Assert-Near $row[0].ammoMass_kg $entry.Value[3] "$($entry.Key) complete projectile mass"
    Assert-Near $row[0].warheadMass_kg $entry.Value[4] "$($entry.Key) damaging projectile mass"
    Assert-Near $row[0].cooldown_s $entry.Value[5] "$($entry.Key) cycle reload"
    Assert-Near $row[0].intraSalvoCooldown_s $entry.Value[6] "$($entry.Key) intra-salvo reload"
    if ([double]$row[0].warheadMass_kg -gt [double]$row[0].ammoMass_kg) {
        throw "$($entry.Key) damaging mass exceeds complete projectile mass."
    }
}

$hullOverrides = Read-JsonArray (Join-Path $modFiles 'TIShipHullTemplate.json')
$expectedHulls = [ordered]@{
    Gunship = @(55, 15, 9719, 171, 3)
    Escort = @(62, 15, 10956, 338, 4)
    Corvette = @(65, 17, 14754, 385, 5)
    Frigate = @(100, 18, 25447, 576, 8)
    Monitor = @(100, 17, 22698, 679, 7)
    Destroyer = @(100, 23, 41548, 873, 9)
    Cruiser = @($null, $null, $null, 964, 12)
    Battlecruiser = @($null, $null, $null, 1170, 10)
    Lancer = @($null, $null, $null, 1958, 14)
    Battleship = @($null, $null, $null, 1558, 14)
    Dreadnought = @($null, $null, $null, 2346, 18)
    Titan = @($null, $null, $null, 3143, 19)
}
if ($hullOverrides.Count -ne $expectedHulls.Count) {
    throw "Hull override has $($hullOverrides.Count) rows instead of $($expectedHulls.Count)."
}
foreach ($entry in $expectedHulls.GetEnumerator()) {
    $row = @($hullOverrides | Where-Object dataName -eq $entry.Key)
    if ($row.Count -ne 1) {
        throw "Hull override must contain '$($entry.Key)' exactly once."
    }
    if ($null -eq $entry.Value[0]) {
        Assert-Properties $row[0] @('dataName', 'mass_tons', 'crew') $entry.Key
        Assert-Near $row[0].mass_tons $entry.Value[3] "$($entry.Key) mass_tons"
        Assert-Near $row[0].crew $entry.Value[4] "$($entry.Key) crew"
    }
    else {
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
    }
    $emptyMass = [double]$row[0].mass_tons + 3.0 * [double]$row[0].crew
    $expectedEmptyMass = switch ($entry.Key) {
        Gunship { 180 }
        Escort { 350 }
        Corvette { 400 }
        Frigate { 600 }
        Monitor { 700 }
        Destroyer { 900 }
        Cruiser { 1000 }
        Battlecruiser { 1200 }
        Lancer { 2000 }
        Battleship { 1600 }
        Dreadnought { 2400 }
        Titan { 3200 }
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
foreach ($id in $expectedGunDiameters.Keys) {
    if (@($vanillaGuns | Where-Object dataName -eq $id).Count -ne 1) {
        throw "Installed game no longer contains gun '$id'."
    }
}
foreach ($id in @('35mmAutocannon', '40mmNoseAutocannon')) {
    $vanilla = @($vanillaGuns | Where-Object dataName -eq $id)[0]
    $override = @($gunOverrides | Where-Object dataName -eq $id)[0]
    $vanillaCycle = [double]$vanilla.cooldown_s +
        ([double]$vanilla.salvo_shots - 1.0) *
        [double]$vanilla.intraSalvoCooldown_s
    $revisedCycle = [double]$override.cooldown_s +
        ([double]$vanilla.salvo_shots - 1.0) *
        [double]$override.intraSalvoCooldown_s
    Assert-Near $vanillaCycle 5.5 "$id vanilla cycle"
    Assert-Near $revisedCycle 2.95 "$id revised cycle"

    $vanillaSustained =
        0.5 * [double]$vanilla.warheadMass_kg *
        [Math]::Pow([double]$vanilla.muzzleVelocity_kps, 2) *
        [double]$vanilla.salvo_shots / $vanillaCycle
    $revisedSustained =
        0.5 * [double]$override.warheadMass_kg *
        [Math]::Pow([double]$vanilla.muzzleVelocity_kps, 2) *
        [double]$vanilla.salvo_shots / $revisedCycle
    $sustainedDelta = $revisedSustained / $vanillaSustained - 1.0
    if ($sustainedDelta -lt -0.14 -or $sustainedDelta -gt -0.11) {
        throw "$id sustained output changes by $sustainedDelta instead of approximately -15%."
    }
}
$vanillaLasers = Read-JsonArray (Join-Path $VanillaTemplatesDir 'TILaserWeaponTemplate.json')
if (@($vanillaLasers | Where-Object dataName -eq 'PointDefenseLaserTurret').Count -ne 1) {
    throw "Installed game no longer contains laser 'PointDefenseLaserTurret'."
}
$vanillaMagneticGuns = Read-JsonArray (Join-Path $VanillaTemplatesDir 'TIMagneticGunTemplate.json')
foreach ($id in @($expectedHumanRails.Keys) + @($expectedHumanCoils.Keys) + @($expectedAlienMags.Keys)) {
    if (@($vanillaMagneticGuns | Where-Object dataName -eq $id).Count -ne 1) {
        throw "Installed game no longer contains magnetic gun '$id'."
    }
}

function Get-EffectiveMagneticValue {
    param(
        [string]$DataName,
        [string]$Property
    )

    $override = @($magneticOverrides | Where-Object dataName -eq $DataName)[0]
    if ($override.PSObject.Properties.Name -contains $Property) {
        return [double]$override.$Property
    }
    $vanilla = @($vanillaMagneticGuns | Where-Object dataName -eq $DataName)[0]
    return [double]$vanilla.$Property
}

function Get-EffectiveMagneticSustainedDamage {
    param([string]$DataName)

    $vanilla = @($vanillaMagneticGuns | Where-Object dataName -eq $DataName)[0]
    $salvoShots = if ($null -ne $vanilla.salvo_shots) {
        [double]$vanilla.salvo_shots
    }
    else {
        1.0
    }
    $intraSalvoCooldown = Get-EffectiveMagneticValue `
        $DataName 'intraSalvoCooldown_s'
    $cycle = (Get-EffectiveMagneticValue $DataName 'cooldown_s') +
        ($salvoShots - 1.0) * $intraSalvoCooldown
    $damage = 0.5 *
        (Get-EffectiveMagneticValue $DataName 'warheadMass_kg') *
        [Math]::Pow(
            (Get-EffectiveMagneticValue $DataName 'muzzleVelocity_kps'), 2)
    return $damage * $salvoShots / $cycle
}

$coilTierComparisons = @(
    [pscustomobject]@{ Coil = 'LightCoilgunBatteryMk1'; Rail = 'LightRailgunBatteryMk2' }
    [pscustomobject]@{ Coil = 'LightCoilgunBatteryMk2'; Rail = 'LightRailgunBatteryMk3' }
    [pscustomobject]@{ Coil = 'CoilgunBatteryMk1'; Rail = 'RailgunBatteryMk2' }
    [pscustomobject]@{ Coil = 'CoilgunBatteryMk2'; Rail = 'RailgunBatteryMk3' }
    [pscustomobject]@{ Coil = 'HeavyCoilgunBatteryMk1'; Rail = 'HeavyRailgunBatteryMk2' }
    [pscustomobject]@{ Coil = 'HeavyCoilgunBatteryMk2'; Rail = 'HeavyRailgunBatteryMk3' }
    [pscustomobject]@{ Coil = 'LightCoilCannonMk1'; Rail = 'LightRailCannonMk2' }
    [pscustomobject]@{ Coil = 'LightCoilCannonMk2'; Rail = 'LightRailCannonMk3' }
    [pscustomobject]@{ Coil = 'CoilCannonMk1'; Rail = 'RailCannonMk2' }
    [pscustomobject]@{ Coil = 'CoilCannonMk2'; Rail = 'RailCannonMk3' }
    [pscustomobject]@{ Coil = 'HeavyCoilCannonMk1'; Rail = 'HeavyRailCannonMk2' }
    [pscustomobject]@{ Coil = 'HeavyCoilCannonMk2'; Rail = 'HeavyRailCannonMk3' }
    [pscustomobject]@{ Coil = 'SpinalCoilerMk1'; Rail = 'SpinalRailgunMk2' }
    [pscustomobject]@{ Coil = 'SpinalCoilerMk2'; Rail = 'SpinalRailgunMk3' }
    [pscustomobject]@{ Coil = 'HeavySiegeCoilerMk1'; Rail = 'HeavyRailCannonMk2' }
    [pscustomobject]@{ Coil = 'HeavySiegeCoilerMk2'; Rail = 'HeavyRailCannonMk3' }
    [pscustomobject]@{ Coil = 'SpinalSiegeCoilerMk1'; Rail = 'SpinalRailgunMk2' }
    [pscustomobject]@{ Coil = 'SpinalSiegeCoilerMk2'; Rail = 'SpinalRailgunMk3' }
)
foreach ($comparison in $coilTierComparisons) {
    $coilRange = Get-EffectiveMagneticValue $comparison.Coil 'targetingRange_km'
    $railRange = Get-EffectiveMagneticValue $comparison.Rail 'targetingRange_km'
    if ($coilRange -le $railRange) {
        throw "$($comparison.Coil) range $coilRange does not exceed $($comparison.Rail) range $railRange."
    }
    $coilSustained = Get-EffectiveMagneticSustainedDamage $comparison.Coil
    $railSustained = Get-EffectiveMagneticSustainedDamage $comparison.Rail
    if ($coilSustained -le $railSustained) {
        throw "$($comparison.Coil) sustained damage $coilSustained does not exceed $($comparison.Rail) sustained damage $railSustained."
    }
    $coilIntraSalvo = Get-EffectiveMagneticValue $comparison.Coil 'intraSalvoCooldown_s'
    $railInterSalvo = Get-EffectiveMagneticValue $comparison.Rail 'cooldown_s'
    if ($coilIntraSalvo -gt $railInterSalvo) {
        throw "$($comparison.Coil) intra-salvo reload $coilIntraSalvo exceeds $($comparison.Rail) inter-salvo reload $railInterSalvo."
    }
}

foreach ($tier in 1..3) {
    $lightBatteryDps = Get-EffectiveMagneticSustainedDamage `
        "LightCoilgunBatteryMk$tier"
    $coilBatteryDps = Get-EffectiveMagneticSustainedDamage `
        "CoilgunBatteryMk$tier"
    if ($coilBatteryDps -le 2.0 * $lightBatteryDps) {
        throw "Coilgun Battery Mk$tier sustained damage $coilBatteryDps does not exceed twice Light Coilgun Battery Mk$tier sustained damage $lightBatteryDps."
    }
}

$coilProgressionFamilies = @(
    'LightCoilgunBattery', 'CoilgunBattery', 'HeavyCoilgunBattery',
    'LightCoilCannon', 'CoilCannon', 'HeavyCoilCannon', 'SpinalCoiler')
foreach ($family in $coilProgressionFamilies) {
    $vanillaMk2 = @($vanillaMagneticGuns | Where-Object dataName -eq "${family}Mk2")[0]
    $vanillaMk3 = @($vanillaMagneticGuns | Where-Object dataName -eq "${family}Mk3")[0]
    $oldVelocityRatio = [double]$vanillaMk3.muzzleVelocity_kps /
        [double]$vanillaMk2.muzzleVelocity_kps
    $newVelocityRatio =
        (Get-EffectiveMagneticValue "${family}Mk3" 'muzzleVelocity_kps') /
        (Get-EffectiveMagneticValue "${family}Mk2" 'muzzleVelocity_kps')
    if ([Math]::Abs($newVelocityRatio / $oldVelocityRatio - 1.0) -gt 0.01) {
        throw "$family Mk2-to-Mk3 velocity ratio changed by more than 1%."
    }
    $oldIntraRatio = [double]$vanillaMk3.intraSalvoCooldown_s /
        [double]$vanillaMk2.intraSalvoCooldown_s
    $newIntraRatio =
        (Get-EffectiveMagneticValue "${family}Mk3" 'intraSalvoCooldown_s') /
        (Get-EffectiveMagneticValue "${family}Mk2" 'intraSalvoCooldown_s')
    Assert-Near $newIntraRatio $oldIntraRatio "$family Mk2-to-Mk3 intra-salvo ratio"
}

foreach ($id in $expectedHumanCoils.Keys) {
    $vanilla = @($vanillaMagneticGuns | Where-Object dataName -eq $id)[0]
    $expectedScaledRange = [Math]::Floor(
        ([double]$vanilla.targetingRange_km * 1.25) / 50.0) * 50.0
    $actualRange = Get-EffectiveMagneticValue $id 'targetingRange_km'
    Assert-Near $actualRange $expectedScaledRange "$id percentage-scaled targeting range"
}

$magneticProgressionCsvPath = Join-Path $RepositoryRoot `
    'docs\ship-balance-research\tables\magnetic-tier-progression-rework.csv'
$magneticProgressionRows = @(Import-Csv -LiteralPath $magneticProgressionCsvPath)
if ($magneticProgressionRows.Count -ne 49) {
    throw "Magnetic progression artifact has $($magneticProgressionRows.Count) rows instead of 49."
}
foreach ($row in $magneticProgressionRows) {
    $id = [string]$row.dataName
    $expectedInterSalvo = [double]$row.original_cooldown_s
    $isLightHumanCoil = $id -match `
        '^(LightCoilgunBattery|LightCoilCannon)Mk[123]$'
    $retainedIntraFraction = if ($isLightHumanCoil) { 0.40 } else { 0.60 }
    $expectedIntraSalvo = [Math]::Ceiling(
        [double]$row.original_intraSalvoCooldown_s * $retainedIntraFraction)
    $expectedWarheadMass = if ($id -eq 'LightCoilgunBatteryMk3') {
        10.0
    }
    else {
        [double]$row.original_warheadMass_kg
    }
    Assert-Near `
        ([double]$row.proposed_cooldown_s) $expectedInterSalvo `
        "$id artifact unchanged inter-salvo reload"
    Assert-Near `
        ([double]$row.proposed_intraSalvoCooldown_s) $expectedIntraSalvo `
        "$id artifact percentage-scaled intra-salvo reload"
    Assert-Near `
        (Get-EffectiveMagneticValue $id 'cooldown_s') $expectedInterSalvo `
        "$id runtime unchanged inter-salvo reload"
    Assert-Near `
        (Get-EffectiveMagneticValue $id 'intraSalvoCooldown_s') $expectedIntraSalvo `
        "$id runtime percentage-scaled intra-salvo reload"
    Assert-Near `
        (Get-EffectiveMagneticValue $id 'muzzleVelocity_kps') `
        ([double]$row.proposed_muzzleVelocity_kps) `
        "$id locked muzzle velocity"
    Assert-Near `
        (Get-EffectiveMagneticValue $id 'targetingRange_km') `
        ([double]$row.proposed_targetingRange_km) `
        "$id locked targeting range"
    Assert-Near `
        ([double]$row.proposed_warheadMass_kg) $expectedWarheadMass `
        "$id artifact proposed damaging mass"
    Assert-Near `
        (Get-EffectiveMagneticValue $id 'warheadMass_kg') $expectedWarheadMass `
        "$id runtime proposed damaging mass"
    if ($expectedIntraSalvo -gt $expectedInterSalvo) {
        throw "$id intra-salvo reload $expectedIntraSalvo exceeds its own inter-salvo reload $expectedInterSalvo."
    }
}

$alienDominanceComparisons = @(
    [pscustomobject]@{ Alien = 'AlienLightMagBattery'; Rail = 'LightRailgunBatteryMk1'; Coil = 'LightCoilgunBatteryMk1' }
    [pscustomobject]@{ Alien = 'AlienMagBattery'; Rail = 'RailgunBatteryMk1'; Coil = 'CoilgunBatteryMk1' }
    [pscustomobject]@{ Alien = 'AlienHeavyMagBattery'; Rail = 'HeavyRailgunBatteryMk1'; Coil = 'HeavyCoilgunBatteryMk1' }
    [pscustomobject]@{ Alien = 'AlienMiniLightMagCannon'; Rail = 'LightRailCannonMk1'; Coil = 'LightCoilCannonMk1' }
    [pscustomobject]@{ Alien = 'AlienLightMagCannon'; Rail = 'LightRailCannonMk1'; Coil = 'LightCoilCannonMk1' }
    [pscustomobject]@{ Alien = 'AlienMagCannon'; Rail = 'RailCannonMk1'; Coil = 'CoilCannonMk1' }
    [pscustomobject]@{ Alien = 'AlienHeavyMagCannon'; Rail = 'HeavyRailCannonMk1'; Coil = 'HeavyCoilCannonMk1' }
    [pscustomobject]@{ Alien = 'AlienSpinalMagCannon'; Rail = 'SpinalRailgunMk1'; Coil = 'SpinalCoilerMk1' }
    [pscustomobject]@{ Alien = 'AdvancedAlienLightMagBattery'; Rail = 'LightRailgunBatteryMk3'; Coil = 'LightCoilgunBatteryMk3' }
    [pscustomobject]@{ Alien = 'AdvancedAlienMagBattery'; Rail = 'RailgunBatteryMk3'; Coil = 'CoilgunBatteryMk3' }
    [pscustomobject]@{ Alien = 'AdvancedAlienHeavyMagBattery'; Rail = 'HeavyRailgunBatteryMk3'; Coil = 'HeavyCoilgunBatteryMk3' }
    [pscustomobject]@{ Alien = 'AdvancedAlienLightMagCannon'; Rail = 'LightRailCannonMk3'; Coil = 'LightCoilCannonMk3' }
    [pscustomobject]@{ Alien = 'AdvancedAlienMagCannon'; Rail = 'RailCannonMk3'; Coil = 'CoilCannonMk3' }
    [pscustomobject]@{ Alien = 'AdvancedAlienHeavyMagCannon'; Rail = 'HeavyRailCannonMk3'; Coil = 'HeavyCoilCannonMk3' }
    [pscustomobject]@{ Alien = 'AdvancedAlienSpinalMagCannon'; Rail = 'SpinalRailgunMk3'; Coil = 'SpinalCoilerMk3' }
)
foreach ($comparison in $alienDominanceComparisons) {
    foreach ($property in @(
        'ammoMass_kg', 'warheadMass_kg', 'muzzleVelocity_kps',
        'targetingRange_km', 'efficiency')) {
        $alienValue = Get-EffectiveMagneticValue $comparison.Alien $property
        $humanMaximum = [Math]::Max(
            (Get-EffectiveMagneticValue $comparison.Rail $property),
            (Get-EffectiveMagneticValue $comparison.Coil $property))
        if ($alienValue -le $humanMaximum) {
            throw "$($comparison.Alien) $property is $alienValue and does not strictly exceed the matching human maximum $humanMaximum."
        }
    }
}

$housekeepingIds = @($expectedHumanCoils.Keys) + @($expectedAlienMags.Keys)
foreach ($id in $housekeepingIds) {
    $vanilla = @($vanillaMagneticGuns | Where-Object dataName -eq $id)[0]
    $override = @($magneticOverrides | Where-Object dataName -eq $id)[0]
    $vanillaCycle = [double]$vanilla.cooldown_s +
        ([double]$vanilla.salvo_shots - 1.0) * [double]$vanilla.intraSalvoCooldown_s
    $proposedCycle = [double]$override.cooldown_s +
        ([double]$vanilla.salvo_shots - 1.0) *
        [double]$override.intraSalvoCooldown_s
    $cycleDelta = $proposedCycle / $vanillaCycle - 1.0
    if ($cycleDelta -lt -0.6000001 -or $cycleDelta -ge 0.0) {
        throw "$id total cycle changes by $cycleDelta instead of remaining faster than vanilla without exceeding a 60% reduction."
    }
    $ammoDelta = [double]$override.ammoMass_kg / [double]$vanilla.ammoMass_kg - 1.0
    $warheadDelta = [double]$override.warheadMass_kg / [double]$vanilla.warheadMass_kg - 1.0
    $warheadMassException = $id -eq 'LightCoilgunBatteryMk3' -and
        [double]$override.warheadMass_kg -eq 10.0 -and
        [double]$vanilla.warheadMass_kg -eq 8.75
    if ($ammoDelta -lt 0.20 -or $ammoDelta -gt 0.34 -or
        (-not $warheadMassException -and
            ($warheadDelta -lt 0.20 -or $warheadDelta -gt 0.34))) {
        throw "$id projectile masses do not round to approximately +25%."
    }
    foreach ($field in @(
        'ammoMass_kg', 'warheadMass_kg', 'targetingRange_km', 'cooldown_s')) {
        if ([double]$override.$field -ne [Math]::Round([double]$override.$field)) {
            throw "$id $field must be rounded to a whole unit."
        }
    }
    $velocityTenths = 10.0 * [double]$override.muzzleVelocity_kps
    if ([Math]::Abs($velocityTenths - [Math]::Round($velocityTenths)) -gt 0.0000001) {
        throw "$id muzzleVelocity_kps must be rounded to 0.1 km/s."
    }
}
$vanillaHulls = Read-JsonArray (Join-Path $VanillaTemplatesDir 'TIShipHullTemplate.json')
foreach ($id in $expectedHulls.Keys) {
    if (@($vanillaHulls | Where-Object dataName -eq $id).Count -ne 1) {
        throw "Installed game no longer contains hull '$id'."
    }
}

$driveOverrides = Read-JsonArray (Join-Path $modFiles 'TIDriveTemplate.json')
$vanillaDrives = Read-JsonArray (Join-Path $VanillaTemplatesDir 'TIDriveTemplate.json')
$driveFamilies = [ordered]@{
    AlienFusionLantern = @(1200000, 1200, 0.95, 5000)
    AlienFusionTorch = @(3800000, 2350, 0.97, 32000)
    AdvancedAlienFusionTorch = @(10500000, 3000, 0.98, 107550)
}
if ($driveOverrides.Count -ne 18) {
    throw "Alien drive override has $($driveOverrides.Count) rows instead of 18."
}
foreach ($family in $driveFamilies.GetEnumerator()) {
    for ($thrusters = 1; $thrusters -le 6; $thrusters++) {
        $id = "$($family.Key)x$thrusters"
        $row = @($driveOverrides | Where-Object dataName -eq $id)
        $vanilla = @($vanillaDrives | Where-Object dataName -eq $id)
        if ($row.Count -ne 1 -or $vanilla.Count -ne 1) {
            throw "Drive '$id' must exist exactly once in both override and installed data."
        }
        Assert-Properties $row[0] @(
            'dataName', 'thrust_N', 'EV_kps', 'efficiency',
            'thrustRating_GW', 'req power') $id
        $expectedThrust = [double]$family.Value[0] * $thrusters
        $expectedEv = [double]$family.Value[1]
        $expectedEfficiency = [double]$family.Value[2]
        $expectedJetPower = $expectedThrust * $expectedEv / 1000000 / 2
        $expectedRequiredPower = $expectedJetPower / $expectedEfficiency
        $jetPower = [double](([string]$row[0].thrustRating_GW).Replace(',', ''))
        $requiredPower = [double](([string]$row[0].'req power').Replace(',', ''))
        Assert-Near $row[0].thrust_N $expectedThrust "$id thrust"
        Assert-Near $row[0].EV_kps $expectedEv "$id exhaust velocity"
        Assert-Near $row[0].efficiency $expectedEfficiency "$id efficiency"
        Assert-Near $row[0].efficiency $vanilla[0].efficiency "$id unchanged efficiency"
        if ([Math]::Abs($jetPower - $expectedJetPower) -gt 0.0005 -or
            [Math]::Abs($requiredPower - $expectedRequiredPower) -gt 0.0005) {
            throw "$id power fields do not match thrust, EV, and efficiency."
        }
        if ($thrusters -eq 6 -and $requiredPower -ge [double]$family.Value[3]) {
            throw "$id requires $requiredPower GW, exceeding its matched reactor cap."
        }
    }
}

$reactorBayCsvPath = Join-Path $RepositoryRoot `
    'docs\ship-balance-research\tables\reactor-bay-variant-volumes.csv'
$reactorBayRows = @(Import-Csv -LiteralPath $reactorBayCsvPath)
$reactorBayHulls = @(
    'Gunship', 'Escort', 'Corvette', 'Frigate', 'Monitor', 'Destroyer',
    'Cruiser', 'Battlecruiser', 'Lancer', 'Battleship', 'Dreadnought', 'Titan'
)
$expectedBayVolumes = @(
    @(264.240616, 452.197326, 317.310118, 712.241612),
    @(264.240558, 452.197326, 317.310118, 712.241612),
    @(264.240616, 452.197235, 604.707011, 837.587811),
    @(332.341240, 675.443739, 1246.492028, 1233.527032),
    @(384.582064, 675.443717, 2617.607109, 2028.674504),
    @(384.582064, 675.443717, 2617.606700, 2028.674504),
    @(1989.241734, 1384.983819, 3930.637720, 3505.550347),
    @(1989.242548, 1384.983819, 3930.637720, 3505.550347),
    @(2365.773019, 2090.292333, 10223.879025, 8072.643840),
    @(5648.074162, 2090.291983, 5464.773080, 6945.700026),
    @(11476.330412, 2090.293033, 10223.879025, 10952.622272),
    @(15955.575747, 6290.836709, 16549.539439, 15840.889300)
)
if ($reactorBayRows.Count -ne 48) {
    throw "Reactor-bay measurement artifact has $($reactorBayRows.Count) rows instead of 48."
}
for ($hullIndex = 0; $hullIndex -lt $reactorBayHulls.Count; $hullIndex++) {
    for ($appearanceIndex = 0; $appearanceIndex -lt 4; $appearanceIndex++) {
        $hull = $reactorBayHulls[$hullIndex]
        $row = @($reactorBayRows | Where-Object {
            $_.hull -eq $hull -and
            [int]$_.appearanceIndex -eq $appearanceIndex
        })
        if ($row.Count -ne 1) {
            throw "Reactor-bay artifact must contain $hull appearance $appearanceIndex exactly once."
        }
        Assert-Near `
            ([double]$row[0].inscribedCylinder_m3) `
            ([double]$expectedBayVolumes[$hullIndex][$appearanceIndex]) `
            "$hull appearance $appearanceIndex reactor-bay volume"
        if ([double]$row[0].transverseX_m -le 0 -or
            [double]$row[0].transverseY_m -le 0 -or
            [double]$row[0].longitudinalLength_m -le 0 -or
            [string]::IsNullOrWhiteSpace($row[0].modelResource) -or
            [string]::IsNullOrWhiteSpace($row[0].meshName)) {
            throw "$hull appearance $appearanceIndex has incomplete source geometry."
        }
    }
}

$measurementToolPath = Join-Path $RepositoryRoot `
    'scripts\ship-balance\measure_ship_prefabs.py'
$measurementTool = Get-Content -LiteralPath $measurementToolPath -Raw
foreach ($requiredToken in @(
    'DLC_BUNDLE', '"radiator" in leaf', 'leaf.endswith("_rads")',
    'measure_reactor_bay_variants', 'reactor_bay_variant_measurements')) {
    if (-not $measurementTool.Contains($requiredToken)) {
        throw "Ship measurement tool is missing reactor-bay token '$requiredToken'."
    }
}

Write-Host 'PASS: settled ship-rebalance overrides and all 48 graphical reactor-bay measurements validated.'
