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
$maintenanceProposalPath = Join-Path $RepositoryRoot `
    'docs\hab-economy\hab-module-maintenance-proposals.csv'

foreach ($requiredPath in @(
    $moduleSourcePath,
    $moduleOverridePath,
    $habOverridePath,
    $globalOverridePath,
    $maintenanceProposalPath
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
$maintenanceResources = [ordered]@{
    boost = 'boost'
    water = 'water'
    volatiles = 'volatiles'
    metals = 'metals'
    nobleMetals = 'noble_metals'
    fissiles = 'fissiles'
    antimatter = 'antimatter'
    exotics = 'exotics'
}
$cleanMassIncrementTons = 5.0

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
$proposalRows = @(Import-Csv -LiteralPath $maintenanceProposalPath)
$proposalByName = @{}
foreach ($proposal in $proposalRows) {
    if ($proposalByName.ContainsKey($proposal.data_name)) {
        throw "Duplicate maintenance proposal for $($proposal.data_name)."
    }
    $proposalByName[$proposal.data_name] = $proposal
}

if ($expected.Count -ne 110 -or $overrides.Count -ne 110 -or
    $proposalRows.Count -ne 110) {
    throw "Expected 110 vanilla targets, overrides, and maintenance proposals; found $($expected.Count), $($overrides.Count), and $($proposalRows.Count)."
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
$proposalNames = @($proposalRows.data_name | Sort-Object)
if (($expectedNames -join ';') -ne ($actualNames -join ';') -or
    ($expectedNames -join ';') -ne ($proposalNames -join ';')) {
    throw 'Hab-module overrides and maintenance proposals do not exactly match the human-buildable T1-T3 target set.'
}

$overrideByName = @{}
$maintenanceTotalByTier = @{ 1 = 0.0; 2 = 0.0; 3 = 0.0 }
foreach ($override in $overrides) {
    $overrideByName[$override.dataName] = $override
    $source = $vanillaByName[$override.dataName]
    $vanillaMass = [double]$source.baseMass_tons
    $expectedMass = [Math]::Round(
        ($vanillaMass * 1.5) / $cleanMassIncrementTons,
        0,
        [MidpointRounding]::AwayFromZero) * $cleanMassIncrementTons
    if ([Math]::Abs([double]$override.baseMass_tons - $expectedMass) -gt 0.000001) {
        throw "$($override.dataName) mass is not 150 percent of vanilla rounded to the nearest 5 tons."
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
        $expectedWeight = [Math]::Round(
            $sourceValue * $vanillaMass / $expectedMass,
            9)
        if ([Math]::Abs($overrideValue - $expectedWeight) -gt 0.00000001) {
            throw "$($override.dataName) changes the vanilla '$materialName' tonnage."
        }
        $sum += $overrideValue
    }
    $expectedSum = $vanillaMass / $expectedMass
    if ([Math]::Abs($sum - $expectedSum) -gt 0.00000001) {
        throw "$($override.dataName) material weights sum to $sum instead of $expectedSum."
    }

    if ($source.tier -eq 1) {
        $expectedCrew = $crewValues[$override.dataName]
        if ([double]$source.power -gt 0) {
            $expectedCrew *= 2
        }
        if (-not $crewValues.ContainsKey($override.dataName) -or
            [int]$override.crew -ne $expectedCrew) {
            throw "$($override.dataName) does not match the reviewed T1 crew map."
        }
    }
    elseif ([double]$source.power -gt 0) {
        if (-not $override.PSObject.Properties['crew'] -or
            [int]$override.crew -ne [int]$source.crew * 2) {
            throw "$($override.dataName) does not double generator crew."
        }
    }
    elseif ($override.PSObject.Properties['crew']) {
        throw "$($override.dataName) unexpectedly overrides non-generator T2/T3 crew."
    }

    if ([double]$source.power -gt 0) {
        if (-not $override.PSObject.Properties['power'] -or
            [double]$override.power -ne [double]$source.power * 2) {
            throw "$($override.dataName) does not double direct generator output."
        }
    }
    elseif ($override.PSObject.Properties['power']) {
        throw "$($override.dataName) unexpectedly overrides non-generator power."
    }

    $proposal = $proposalByName[$override.dataName]
    $vanillaMaintenance = $source.supportMaterials_month
    $overrideMaintenance = $override.supportMaterials_month
    $vanillaResourceTotal = 0.0
    $proposedResourceTotal = 0.0
    foreach ($resource in $maintenanceResources.GetEnumerator()) {
        $vanillaProperty = if ($null -eq $vanillaMaintenance) {
            $null
        }
        else {
            $vanillaMaintenance.PSObject.Properties[$resource.Key]
        }
        $vanillaValue = if ($null -eq $vanillaProperty) {
            0.0
        }
        else {
            [double]$vanillaProperty.Value
        }
        $proposedValue = [double]$proposal.($resource.Value)
        $overrideProperty = if ($null -eq $overrideMaintenance) {
            $null
        }
        else {
            $overrideMaintenance.PSObject.Properties[$resource.Key]
        }
        $vanillaResourceTotal += $vanillaValue
        $proposedResourceTotal += $proposedValue
        if ($vanillaValue -gt 0) {
            if ($proposedValue -le 0 -or
                $null -eq $overrideProperty -or
                [Math]::Abs([double]$overrideProperty.Value - $proposedValue) -gt 0.000001) {
                throw "$($override.dataName) does not preserve '$($resource.Key)' at its approved maintenance value."
            }
        }
        elseif ($proposedValue -ne 0 -or $null -ne $overrideProperty) {
            throw "$($override.dataName) adds maintenance resource '$($resource.Key)'."
        }
    }
    if ($vanillaResourceTotal -eq 0) {
        if ($null -ne $overrideMaintenance) {
            throw "$($override.dataName) adds a maintenance object to a zero-resource module."
        }
    }
    else {
        if ($null -eq $overrideMaintenance -or
            [double]$overrideMaintenance.money -ne [double]$vanillaMaintenance.money) {
            throw "$($override.dataName) does not preserve vanilla money maintenance."
        }
    }
    if ($proposedResourceTotal -gt $vanillaResourceTotal + 0.000001 -or
        [Math]::Abs(
            $proposedResourceTotal * 10 -
            [double]$proposal.proposed_tons_month) -gt 0.01) {
        throw "$($override.dataName) maintenance violates the approved cap or total."
    }
    $maintenanceTotalByTier[[int]$source.tier] += $proposedResourceTotal * 10
}

$expectedMaintenanceTotalByTier = @{ 1 = 178.585; 2 = 890.425; 3 = 3346.3 }
foreach ($tier in 1..3) {
    if ([Math]::Abs(
        $maintenanceTotalByTier[$tier] -
        $expectedMaintenanceTotalByTier[$tier]) -gt 0.01) {
        throw "T$tier maintenance totals $($maintenanceTotalByTier[$tier]) instead of $($expectedMaintenanceTotalByTier[$tier]) t/month."
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
        [string[]]$SectorFour,
        [double]$ExpectedMass,
        [int]$ExpectedCrew
    )

    if ($Station.sectors.Count -ne 5 -or
        $Station.sectors[0].faction -ne $Faction -or
        $Station.sectors[2].faction -ne $Faction -or
        $Station.sectors[4].faction -ne $Faction -or
        ($Station.sectors[0].habModuleNames -join ';') -ne ($SectorZero -join ';') -or
        ($Station.sectors[2].habModuleNames -join ';') -ne ($SectorTwo -join ';') -or
        ($Station.sectors[4].habModuleNames -join ';') -ne ($SectorFour -join ';')) {
        throw "$($Station.dataName) has an unexpected sector layout."
    }
    foreach ($sectorIndex in @(1, 3)) {
        if (-not [string]::IsNullOrEmpty($Station.sectors[$sectorIndex].faction)) {
            throw "$($Station.dataName) unexpectedly activates sector $sectorIndex."
        }
    }

    $mass = 0.0
    $crew = 0
    foreach ($moduleName in @($SectorZero + $SectorTwo + $SectorFour)) {
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
    'Quarters',
    'SolarCollector',
    'SpaceScienceLab',
    'SolarCollector'
)
# StationGridCell positions from StreamingAssets/AssetBundles/ui:
# internal 0: M1 north, M2 east, M3 south, M4 west
# internal 2: M0 outer/east junction, M1 south, M2 inward/west, M3 north
# internal 4: M0 outer/west junction, M1 north, M2 inward/east, M3 south
$issSectorTwo = @('LifeScienceLab', '', 'Quarters', '')
$issSectorFour = @('Quarters', '', 'MaterialsLab', '')
Assert-Station $habByName.InternationalSpaceStation 'CooperateCouncil' `
    $issSectorZero $issSectorTwo $issSectorFour 435 8
Assert-Station $habByName.InternationalSpaceStationSkirmish 'ResistCouncil' `
    $issSectorZero $issSectorTwo $issSectorFour 435 8
Assert-Station $habByName.Tiangong 'EscapeCouncil' `
    @('PlatformCore', 'SolarCollector', 'LifeScienceLab', '', '') `
    @('', '', '', '') @('', '', '', '') 80 3

Write-Host 'PASS: 110 hab-module overrides, T1-T3 maintenance, doubled generators, consumables, and starting stations validate.'
