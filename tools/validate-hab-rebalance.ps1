[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$VanillaTemplatesDir,
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
}

$moduleSourcePath = Join-Path $VanillaTemplatesDir 'TIHabModuleTemplate.json'
$moduleOverridePath = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\TIHabModuleTemplate.json'
$habOverridePath = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\TIHabTemplate.json'
$globalOverridePath = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\TIGlobalConfig.json'

foreach ($requiredPath in @(
    $moduleSourcePath,
    $moduleOverridePath,
    $habOverridePath,
    $globalOverridePath
)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required hab-rebalance input is missing: $requiredPath"
    }
}

$crewValues = @{
    AdministrationNode = 4
    AntimatterTrap = 1
    AutomatedFissionPile = 0
    AutomatedMiningComplex = 0
    AutomatedOutpostCore = 0
    AutomatedPlatformCore = 0
    AutomatedSolarCollector = 0
    AutomatedSolarMirror = 0
    AutomatedSupplyDepot = 0
    BroadcastOutlet = 1
    ClimateLab = 1
    ConstructionModule = 2
    EnergyLab = 1
    FissionPile = 1
    FusionPile = 1
    HeavyFissionPile = 1
    HeavyFusionPile = 2
    HydroponicsBay = 1
    InformationScienceLab = 1
    LifeScienceLab = 1
    ListeningPost = 2
    MarinePlatoonBarracks = 30
    MaterialsLab = 1
    MilitaryScienceLab = 1
    OutpostCore = 2
    OutpostMiningComplex = 4
    ParticleCollider = 2
    PlatformCore = 2
    PointDefenseArray = 1
    Quarters = 1
    SocialScienceLab = 1
    SolarCollector = 0
    SolarMirror = 0
    SpaceDock = 4
    SpaceScienceLab = 1
    SupplyDepot = 0
    TouristBerth = 2
    XenologyLab = 1
}
$materialNames = @(
    'water',
    'volatiles',
    'metals',
    'nobleMetals',
    'fissiles',
    'antimatter',
    'exotics'
)

$vanillaJson = Get-Content -LiteralPath $moduleSourcePath -Raw | ConvertFrom-Json
$vanilla = @($vanillaJson | ForEach-Object { $_ })
$expected = @($vanilla | Where-Object {
    $_.tier -ge 1 -and
    $_.tier -le 3 -and
    -not $_.noBuild -and
    -not $_.destroyed -and
    -not $_.alienModule -and
    $_.dataName -notlike 'Alien*'
})
$overrideJson = Get-Content -LiteralPath $moduleOverridePath -Raw | ConvertFrom-Json
$overrides = @($overrideJson | ForEach-Object { $_ })

if ($expected.Count -ne 110 -or $overrides.Count -ne 110) {
    throw "Expected 110 vanilla targets and overrides; found $($expected.Count) and $($overrides.Count)."
}
$duplicates = $overrides | Group-Object dataName | Where-Object Count -ne 1
if ($duplicates) {
    throw "Duplicate hab-module overrides: $($duplicates.Name -join ', ')"
}

$vanillaByName = @{}
foreach ($template in $vanilla) {
    $vanillaByName[$template.dataName] = $template
}
$expectedNames = @($expected.dataName | Sort-Object)
$actualNames = @($overrides.dataName | Sort-Object)
if (($expectedNames -join ';') -ne ($actualNames -join ';')) {
    throw 'Hab-module overrides do not exactly match the human-buildable T1-T3 target set.'
}

$overrideByName = @{}
foreach ($override in $overrides) {
    $overrideByName[$override.dataName] = $override
    $source = $vanillaByName[$override.dataName]
    $expectedMass = [double]$source.baseMass_tons * 1.5
    if ([Math]::Abs([double]$override.baseMass_tons - $expectedMass) -gt 0.000001) {
        throw "$($override.dataName) mass is not exactly 150 percent of vanilla."
    }

    $sum = 0.0
    foreach ($materialName in $materialNames) {
        $sourceProperty = $source.weightedBuildMaterials.PSObject.Properties[$materialName]
        $sourceValue = if ($null -eq $sourceProperty) { 0.0 } else {
            [double]$sourceProperty.Value
        }
        $overrideProperty = $override.weightedBuildMaterials.PSObject.Properties[$materialName]
        if ($null -eq $overrideProperty) {
            throw "$($override.dataName) omits material '$materialName'."
        }
        $overrideValue = [double]$overrideProperty.Value
        if ([Math]::Abs($overrideValue - $sourceValue * 2.0 / 3.0) -gt 0.00000001) {
            throw "$($override.dataName) changes the vanilla '$materialName' material ratio."
        }
        $sum += $overrideValue
    }
    if ([Math]::Abs($sum - 2.0 / 3.0) -gt 0.00000001) {
        throw "$($override.dataName) material weights sum to $sum instead of two-thirds."
    }

    if ($source.tier -eq 1) {
        if (-not $crewValues.ContainsKey($override.dataName) -or
            [int]$override.crew -ne $crewValues[$override.dataName]) {
            throw "$($override.dataName) does not match the reviewed T1 crew map."
        }
    }
    elseif ($override.PSObject.Properties['crew']) {
        throw "$($override.dataName) unexpectedly overrides non-T1 crew."
    }
}

if ([int]$overrideByName.HydroponicsBay.specialRulesValue -ne 60) {
    throw 'Hydroponics Bay capacity must be 60.'
}
if ($overrideByName.Farm.PSObject.Properties['specialRulesValue'] -or
    $overrideByName.AgricultureComplex.PSObject.Properties['specialRulesValue']) {
    throw 'T2 Farm and T3 Agriculture Complex capacity must remain vanilla.'
}

$globalJson = Get-Content -LiteralPath $globalOverridePath -Raw | ConvertFrom-Json
$global = @($globalJson | ForEach-Object { $_ })
if ($global.Count -ne 1 -or
    $global[0].dataName -ne 'globalConfig' -or
    [double]$global[0].crewWaterConsumptionTons_year -ne 3 -or
    [double]$global[0].crewVolatilesConsumptionTons_year -ne 3) {
    throw 'Global crew water and volatile consumption must both equal 3 tons per year.'
}

function Assert-Station {
    param(
        [object]$Station,
        [string]$Faction,
        [string[]]$SectorZero,
        [string[]]$SectorTwo,
        [double]$ExpectedMass,
        [int]$ExpectedCrew
    )

    if ($Station.sectors.Count -ne 5 -or
        $Station.sectors[0].faction -ne $Faction -or
        $Station.sectors[2].faction -ne $Faction -or
        ($Station.sectors[0].habModuleNames -join ';') -ne ($SectorZero -join ';') -or
        ($Station.sectors[2].habModuleNames -join ';') -ne ($SectorTwo -join ';')) {
        throw "$($Station.dataName) has an unexpected sector layout."
    }
    foreach ($sectorIndex in @(1, 3, 4)) {
        if (-not [string]::IsNullOrEmpty($Station.sectors[$sectorIndex].faction)) {
            throw "$($Station.dataName) unexpectedly activates sector $sectorIndex."
        }
    }

    $mass = 0.0
    $crew = 0
    foreach ($moduleName in @($SectorZero + $SectorTwo)) {
        if ([string]::IsNullOrEmpty($moduleName)) {
            continue
        }
        $mass += [double]$overrideByName[$moduleName].baseMass_tons
        $crew += [int]$overrideByName[$moduleName].crew
    }
    if ([Math]::Abs($mass - $ExpectedMass) -gt 0.000001 -or $crew -ne $ExpectedCrew) {
        throw "$($Station.dataName) totals $mass tons and $crew crew; expected $ExpectedMass and $ExpectedCrew."
    }
}

$habJson = Get-Content -LiteralPath $habOverridePath -Raw | ConvertFrom-Json
$habOverrides = @($habJson | ForEach-Object { $_ })
$habByName = @{}
foreach ($hab in $habOverrides) {
    $habByName[$hab.dataName] = $hab
}
$issSectorZero = @(
    'PlatformCore',
    'SolarCollector',
    'SpaceScienceLab',
    'Quarters',
    'SolarCollector'
)
$issSectorTwo = @('LifeScienceLab', 'MaterialsLab', 'Quarters', 'Quarters')
Assert-Station $habByName.InternationalSpaceStation 'CooperateCouncil' `
    $issSectorZero $issSectorTwo 427.5 8
Assert-Station $habByName.InternationalSpaceStationSkirmish 'ResistCouncil' `
    $issSectorZero $issSectorTwo 427.5 8
Assert-Station $habByName.Tiangong 'EscapeCouncil' `
    @('PlatformCore', 'SolarCollector', 'LifeScienceLab', '', '') `
    @('', '', '', '') 75 3

Write-Host 'PASS: 110 hab-module overrides, T1 crew, consumables, and starting stations validate.'
