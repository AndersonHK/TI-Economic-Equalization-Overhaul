[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$TargetManagedDir,
    [Parameter(Mandatory = $true)]
    [string]$ModAssemblyPath,
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
$modFiles = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles'
$probePatchSource = Get-Content -LiteralPath (Join-Path $RepositoryRoot `
    'TIEconomyMod\Patches\ProbeSurveyPatches.cs') -Raw
if (-not $probePatchSource.Contains(
        '__instance.CanProspectFromShip(spaceBody)')) {
    throw 'Site-probe availability must retain the native colonization gate.'
}
if (-not $probePatchSource.Contains(
        'faction.CanProspectWithProbe(body, false)')) {
    throw 'Launch Probe visibility must use the native prospecting gate.'
}
if (-not $probePatchSource.Contains(
        '!ProbeSurveyRuntime.BodyHasProspectorEnRoute(')) {
    throw 'AI prospecting candidates must remain sequential while a drone is in flight.'
}
if (-not $probePatchSource.Contains(
        'typeof(LaunchAllProbeOperation)') -or
    -not $probePatchSource.Contains(
        'ProbeSurveyRuntime.EligibleSites(faction, body)')) {
    throw 'Bulk probe launches must enumerate eligible sites rather than bodies.'
}

function Assert-Close(
    [double]$Expected,
    [double]$Actual,
    [string]$Label,
    [double]$Tolerance = 0.000001) {
    if ([Math]::Abs($Expected - $Actual) -gt $Tolerance) {
        throw "$Label is $Actual; expected $Expected."
    }
}

function Load-AssemblyBytes([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required site-survey validation assembly is missing: $Path"
    }
    return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
}

$globalConfig = @(
    Get-Content -LiteralPath (Join-Path $modFiles 'TIGlobalConfig.json') -Raw |
        ConvertFrom-Json)[0]
Assert-Close 0.325 ([double]$globalConfig.probePayloadBaseline_tons) `
    'Probe payload baseline'
Assert-Close 0 ([double]$globalConfig.probePayloadPerHabSite_tons) `
    'Probe per-site payload coefficient'

[xml]$settings = Get-Content -LiteralPath (
    Join-Path $modFiles 'Settings.xml') -Raw
Assert-Close 2.2 `
    ([double]$settings.Settings.technology.researchCostMultiplier) `
    'Global-technology research multiplier'
Assert-Close 1.4 `
    ([double]$settings.Settings.technology.projectResearchCostMultiplier) `
    'Faction-project research multiplier'

$starts = Get-Content -LiteralPath (
    Join-Path $modFiles 'TIStartTimeTemplate.json') -Raw |
    ConvertFrom-Json
$modern = @($starts | Where-Object dataName -eq 'ModernDayStart')
$future = @($starts | Where-Object dataName -eq '2026Start')
if ($modern.Count -ne 1 -or $future.Count -ne 1) {
    throw 'Expected exactly one 2022 and one 2026 scenario override.'
}
if (($modern[0].startingTechs -join ';') -ne
    'Skywatch;WeAreNotAlone;OutpostHabs') {
    throw 'The 2022 active global technologies changed unexpectedly.'
}
if (($future[0].startingTechs -join ';') -ne
    'DeepSystemSkywatch;WeAreNotAlone;MissiontoMars') {
    throw 'The 2026 active slot must contain Mission to Mars.'
}
foreach ($scenario in @($modern[0], $future[0])) {
    if (@($scenario.projectsCompleted |
            Where-Object { $_ -eq 'Project_OutpostCore' }).Count -ne 1) {
        throw "$($scenario.dataName) must complete Outpost Core exactly once."
    }
}
if (@($future[0].globalTechsCompleted |
        Where-Object { $_ -eq 'MissiontotheMoon' }).Count -ne 1) {
    throw 'The 2026 start must complete Mission to the Moon exactly once.'
}

$templateDirectory = Join-Path (
    Split-Path -Parent $TargetManagedDir) 'StreamingAssets\Templates'
$installedSites = Get-Content -LiteralPath (
    Join-Path $templateDirectory 'TIHabSiteTemplate.json') -Raw |
    ConvertFrom-Json
$marsSites = @($installedSites | Where-Object parentBodyName -eq 'Mars')
if ($marsSites.Count -ne 25) {
    throw "Expected the installed Mars roster to contain 25 sites, found $($marsSites.Count)."
}
$technologies = Get-Content -LiteralPath (
    Join-Path $templateDirectory 'TITechTemplate.json') -Raw |
    ConvertFrom-Json
$mars = @($technologies | Where-Object dataName -eq 'MissiontoMars')
if ($mars.Count -ne 1) {
    throw 'Installed Mission to Mars technology was not found exactly once.'
}
$available2026 = @($future[0].globalTechsCompleted) +
    @($future[0].startingTechs)
$missingMarsPrerequisites = @($mars[0].prereqs | Where-Object {
    $_ -notin $available2026
})
if ($missingMarsPrerequisites.Count -gt 0) {
    throw "Mission to Mars is illegal in 2026; missing: $($missingMarsPrerequisites -join ', ')."
}

$allSites = Get-Content -LiteralPath (
    Join-Path $modFiles 'TIHabSiteTemplate.json') -Raw |
    ConvertFrom-Json
$sites = @($allSites | Where-Object parentBodyName -eq 'Luna')
if ($sites.Count -ne 35) {
    throw "Expected 35 lunar survey targets, found $($sites.Count)."
}

$G = 6.67430e-11
$earthMassKg = 5.972e24
$moonMassKg = 7.34767e22
$moonMeanRadiusKm = 1738.1 * (1 - 0.0012 / 3)
$moonMu = $G * $moonMassKg
$moonRotationSeconds = 2 * [Math]::PI * [Math]::Sqrt(
    [Math]::Pow(384399000.0, 3) /
    ($G * ($earthMassKg + $moonMassKg)))
$landingRadiusM = $moonMeanRadiusKm * 1000 + 200000
$orbitalSpeedMps = [Math]::Sqrt($moonMu / $landingRadiusM)
$gravityAt200 = $moonMu / [Math]::Pow($landingRadiusM, 2)
$surfaceGravity = $moonMu /
    [Math]::Pow($moonMeanRadiusKm * 1000, 2)
$averageGravity = ($gravityAt200 + $surfaceGravity) / 2
$verticalDeltaVMps = [Math]::Sqrt(400000 / $averageGravity) *
    $averageGravity
$circumferenceKm = 2 * [Math]::PI * $moonMeanRadiusKm
$boostCosts = @($sites | ForEach-Object {
    $rotationMps = [Math]::Cos(
        [double]$_.latitude * [Math]::PI / 180) *
        $circumferenceKm / $moonRotationSeconds * 1000
    $landingDeltaV = ($verticalDeltaVMps +
        $orbitalSpeedMps - $rotationMps) / 1000
    $normalizedDeltaV = 4.294550443 + $landingDeltaV
    0.325 * 0.1 * [Math]::Exp($normalizedDeltaV / 4.44)
})
$minimumBoost = ($boostCosts | Measure-Object -Minimum).Minimum
$maximumBoost = ($boostCosts | Measure-Object -Maximum).Maximum
$totalBoost = ($boostCosts | Measure-Object -Sum).Sum
Assert-Close 0.145215 $minimumBoost 'Lowest lunar site Boost' 0.000003
Assert-Close 0.145366 $maximumBoost 'Highest lunar site Boost' 0.000003
Assert-Close 5.084185 $totalBoost 'All-site lunar Boost' 0.00002

$harmonyAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($assemblyName in @(
    'FMODUnity.dll',
    'Unity.Burst.dll',
    'Unity.Collections.dll',
    'Unity.Jobs.dll',
    'Unity.Mathematics.dll',
    'Unity.Entities.dll',
    'UnityEngine.CoreModule.dll',
    'UnityEngine.dll',
    'UnityEngine.IMGUIModule.dll',
    'UnityEngine.PhysicsModule.dll',
    'UnityEngine.UI.dll',
    'UnityEngine.UIModule.dll',
    'Unity.TextMeshPro.dll',
    'Newtonsoft.Json.dll')) {
    $path = Join-Path $TargetManagedDir $assemblyName
    if (Test-Path -LiteralPath $path) {
        [void](Load-AssemblyBytes $path)
    }
}
[void](Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$gameAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
$modAssembly = Load-AssemblyBytes $ModAssemblyPath

$requiredGameTypes = @(
    'LaunchAllProbeOperation',
    'LaunchProbeOperation',
    'TIOperationTargeting_HabSite',
    'FoundBaseOperation',
    'PavonisInteractive.TerraInvicta.TIFactionState',
    'PavonisInteractive.TerraInvicta.TIHabSiteState')
foreach ($typeName in $requiredGameTypes) {
    if ($null -eq $gameAssembly.GetType($typeName, $false)) {
        throw "Required site-survey target type is missing: $typeName"
    }
}

$runtimeType = $modAssembly.GetType(
    'TIEconomyMod.ProbeSurveyRuntime', $true)
$payloadProperty = $runtimeType.GetProperty(
    'PayloadMass_tons',
    [Reflection.BindingFlags]::NonPublic -bor
    [Reflection.BindingFlags]::Static)
if ($null -eq $payloadProperty) {
    throw 'ProbeSurveyRuntime does not expose its payload-mass authority.'
}

$patchTypeNames = @(
    'TIEconomyMod.Patches.BulkProbeSiteTargetsPatch',
    'TIEconomyMod.Patches.ProbeManufacturingCostPatch',
    'TIEconomyMod.Patches.ProbeEarthCostPatch',
    'TIEconomyMod.Patches.ProbeManufacturingOptionsPatch',
    'TIEconomyMod.Patches.ProbeSiteTargetingMethodPatch',
    'TIEconomyMod.Patches.ProbeSiteTargetsPatch',
    'TIEconomyMod.Patches.ProbeSiteVisibilityPatch',
    'TIEconomyMod.Patches.ProbeSiteLaunchPatch',
    'TIEconomyMod.Patches.ProbeSiteCompletionPatch',
    'TIEconomyMod.Patches.SiteProspectedStatePatch',
    'TIEconomyMod.Patches.BodyProspectingCandidatePatch',
    'TIEconomyMod.Patches.BodyProbeAvailabilityPatch',
    'TIEconomyMod.Patches.BodyProspectingStatePatch',
    'TIEconomyMod.Patches.BodyProspectorEnRoutePatch',
    'TIEconomyMod.Patches.BodyProspectorArrivalPatch',
    'TIEconomyMod.Patches.ProspectorBodiesListPatch',
    'TIEconomyMod.Patches.SurveyedSiteFoundingAvailabilityPatch',
    'TIEconomyMod.Patches.SurveyedBaseTargetsPatch')
$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyId = 'ti.eeo.validate.probe-site-survey.' +
    [Guid]::NewGuid().ToString('N')
$harmony = [Activator]::CreateInstance($harmonyType, @($harmonyId))
try {
    foreach ($patchTypeName in $patchTypeNames) {
        $patchType = $modAssembly.GetType($patchTypeName, $true)
        try {
            [void]$harmony.CreateClassProcessor($patchType).Patch()
        }
        catch {
            throw "Failed applying '$patchTypeName': $($_.Exception.ToString())"
        }
    }
}
finally {
    $harmony.UnpatchAll($harmonyId)
}

Write-Host ('PASS: 0.325-tonne site surveys, 25-site Mars bulk launch, ' +
    'scenario state, lunar costs, and Harmony targets validate.')
