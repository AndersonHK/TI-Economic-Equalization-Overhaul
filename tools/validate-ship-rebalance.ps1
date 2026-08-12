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
    LightCoilgunBatteryMk1 = @(13, 10, 28)
    LightCoilgunBatteryMk2 = @(13, 10, 18)
    LightCoilgunBatteryMk3 = @(13, 11, 8)
    CoilgunBatteryMk1 = @(25, 19, 28)
    CoilgunBatteryMk2 = @(25, 20, 18)
    CoilgunBatteryMk3 = @(25, 22, 8)
    HeavyCoilgunBatteryMk1 = @(50, 38, 28)
    HeavyCoilgunBatteryMk2 = @(50, 40, 18)
    HeavyCoilgunBatteryMk3 = @(50, 44, 8)
    LightCoilCannonMk1 = @(31, 23, 34)
    LightCoilCannonMk2 = @(31, 25, 22)
    LightCoilCannonMk3 = @(31, 27, 10)
    CoilCannonMk1 = @(63, 47, 34)
    CoilCannonMk2 = @(63, 50, 22)
    CoilCannonMk3 = @(63, 55, 10)
    HeavyCoilCannonMk1 = @(94, 71, 34)
    HeavyCoilCannonMk2 = @(94, 75, 22)
    HeavyCoilCannonMk3 = @(94, 82, 10)
    SpinalCoilerMk1 = @(125, 94, 34)
    SpinalCoilerMk2 = @(125, 100, 22)
    SpinalCoilerMk3 = @(125, 109, 10)
    HeavySiegeCoilerMk1 = @(938, 704, 24)
    HeavySiegeCoilerMk2 = @(938, 750, 19)
    HeavySiegeCoilerMk3 = @(938, 821, 12)
    SpinalSiegeCoilerMk1 = @(1250, 938, 24)
    SpinalSiegeCoilerMk2 = @(1250, 1000, 19)
    SpinalSiegeCoilerMk3 = @(1250, 1094, 12)
}
$expectedAlienMags = [ordered]@{
    AlienLightMagBattery = @(5.0, 550, 0.60, 19, 16, 9)
    AlienMagBattery = @(6.0, 700, 0.60, 38, 32, 11)
    AlienHeavyMagBattery = @(7.0, 850, 0.60, 75, 64, 13)
    AlienMiniLightMagCannon = @(6.0, 600, 0.60, 50, 43, 18)
    AlienLightMagCannon = @(6.0, 600, 0.60, 50, 43, 18)
    AlienMagCannon = @(7.0, 750, 0.60, 100, 85, 22)
    AlienHeavyMagCannon = @(8.3, 850, 0.60, 150, 128, 32)
    AlienSpinalMagCannon = @(10.0, 950, 0.60, 200, 170, 43)
    AdvancedAlienLightMagBattery = @(6.7, 650, 0.75, 19, 17, 4)
    AdvancedAlienMagBattery = @(7.8, 800, 0.75, 38, 34, 5)
    AdvancedAlienHeavyMagBattery = @(9.4, 950, 0.75, 75, 68, 6)
    AdvancedAlienLightMagCannon = @(8.6, 700, 0.75, 50, 45, 13)
    AdvancedAlienMagCannon = @(10.0, 850, 0.75, 100, 90, 16)
    AdvancedAlienHeavyMagCannon = @(12.5, 950, 0.75, 150, 135, 23)
    AdvancedAlienSpinalMagCannon = @(15.0, 1050, 0.75, 200, 180, 31)
    Gen3AlienLightMagBattery = @(8.2, 700, 0.85, 35, 30, 4)
    Gen3AlienMagBattery = @(10.3, 850, 0.85, 69, 60, 5)
    Gen3AlienHeavyMagBattery = @(12.4, 1000, 0.85, 138, 120, 6)
    Gen3AlienLightMagCannon = @(10.8, 800, 0.85, 93, 80, 6)
    Gen3AlienMagCannon = @(13.1, 900, 0.85, 184, 160, 8)
    Gen3AlienHeavyMagCannon = @(16.5, 1000, 0.85, 368, 320, 14)
    Gen3AlienSpinalMagCannon = @(19.8, 1100, 0.85, 736, 640, 23)
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
        'dataName', 'ammoMass_kg', 'warheadMass_kg', 'cooldown_s') $entry.Key
    Assert-Near $row[0].ammoMass_kg $entry.Value[0] "$($entry.Key) complete projectile mass"
    Assert-Near $row[0].warheadMass_kg $entry.Value[1] "$($entry.Key) damaging projectile mass"
    Assert-Near $row[0].cooldown_s $entry.Value[2] "$($entry.Key) cycle reload"
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
        'ammoMass_kg', 'warheadMass_kg', 'cooldown_s') $entry.Key
    Assert-Near $row[0].muzzleVelocity_kps $entry.Value[0] "$($entry.Key) muzzle velocity"
    Assert-Near $row[0].targetingRange_km $entry.Value[1] "$($entry.Key) targeting range"
    Assert-Near $row[0].efficiency $entry.Value[2] "$($entry.Key) efficiency"
    Assert-Near $row[0].ammoMass_kg $entry.Value[3] "$($entry.Key) complete projectile mass"
    Assert-Near $row[0].warheadMass_kg $entry.Value[4] "$($entry.Key) damaging projectile mass"
    Assert-Near $row[0].cooldown_s $entry.Value[5] "$($entry.Key) cycle reload"
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
        ([double]$vanilla.salvo_shots - 1.0) * [double]$vanilla.intraSalvoCooldown_s
    $cycleDelta = $proposedCycle / $vanillaCycle - 1.0
    if ($cycleDelta -lt -0.22 -or $cycleDelta -gt -0.18) {
        throw "$id total cycle changes by $cycleDelta instead of approximately -20%."
    }
    $ammoDelta = [double]$override.ammoMass_kg / [double]$vanilla.ammoMass_kg - 1.0
    $warheadDelta = [double]$override.warheadMass_kg / [double]$vanilla.warheadMass_kg - 1.0
    if ($ammoDelta -lt 0.20 -or $ammoDelta -gt 0.34 -or
        $warheadDelta -lt 0.20 -or $warheadDelta -gt 0.34) {
        throw "$id projectile masses do not round to approximately +25%."
    }
    foreach ($field in @('ammoMass_kg', 'warheadMass_kg', 'cooldown_s')) {
        if ([double]$override.$field -ne [Math]::Round([double]$override.$field)) {
            throw "$id $field must be rounded to a whole unit."
        }
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

Write-Host 'PASS: settled ship-rebalance overrides validated.'
