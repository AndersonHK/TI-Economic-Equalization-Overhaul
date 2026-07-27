[CmdletBinding()]
param(
    [string]$VanillaTemplatesDir = 'D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\Templates',
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIHabModuleTemplate.json'
}

$sourcePath = Join-Path $VanillaTemplatesDir 'TIHabModuleTemplate.json'
if (-not (Test-Path -LiteralPath $sourcePath)) {
    throw "Vanilla hab-module templates not found: $sourcePath"
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

$vanilla = Get-Content -LiteralPath $sourcePath -Raw | ConvertFrom-Json
$targets = @($vanilla | Where-Object {
    $_.tier -ge 1 -and
    $_.tier -le 3 -and
    -not $_.noBuild -and
    -not $_.destroyed -and
    -not $_.alienModule -and
    $_.dataName -notlike 'Alien*'
} | Sort-Object tier, dataName)

if ($targets.Count -ne 110) {
    throw "Expected 110 human-buildable T1-T3 modules, found $($targets.Count)."
}
if (@($targets | Where-Object tier -eq 1).Count -ne $crewValues.Count) {
    throw 'The explicit T1 crew map does not cover every targeted T1 module.'
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
$cleanMassIncrementTons = 5.0
$overrides = foreach ($template in $targets) {
    $vanillaMass = [double]$template.baseMass_tons
    $rebalancedMass = [Math]::Round(
        ($vanillaMass * 1.5) / $cleanMassIncrementTons,
        0,
        [MidpointRounding]::AwayFromZero) * $cleanMassIncrementTons
    $vanillaSum = 0.0
    $weights = [ordered]@{}
    foreach ($materialName in $materialNames) {
        $property = $template.weightedBuildMaterials.PSObject.Properties[$materialName]
        $value = if ($null -eq $property) { 0.0 } else { [double]$property.Value }
        $vanillaSum += $value
        $weights[$materialName] = [Math]::Round(
            $value * $vanillaMass / $rebalancedMass,
            9)
    }
    if ([Math]::Abs($vanillaSum - 1.0) -gt 0.0000001) {
        throw "$($template.dataName) has unexpected vanilla material sum $vanillaSum."
    }

    $entry = [ordered]@{
        dataName = $template.dataName
        baseMass_tons = $rebalancedMass
        weightedBuildMaterials = $weights
    }
    if ($template.tier -eq 1) {
        if (-not $crewValues.ContainsKey($template.dataName)) {
            throw "Missing reviewed crew value for $($template.dataName)."
        }
        $entry.crew = $crewValues[$template.dataName]
    }
    if ($template.dataName -eq 'HydroponicsBay') {
        $entry.specialRulesValue = 60
    }
    [pscustomobject]$entry
}

$json = $overrides | ConvertTo-Json -Depth 6
[IO.File]::WriteAllText(
    [IO.Path]::GetFullPath($OutputPath),
    $json + [Environment]::NewLine,
    [Text.UTF8Encoding]::new($false))
Write-Host "Wrote $($overrides.Count) reviewed hab-module overrides to $OutputPath"
