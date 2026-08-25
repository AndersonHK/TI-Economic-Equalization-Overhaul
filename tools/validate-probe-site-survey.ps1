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
$probeUiPatchSource = Get-Content -LiteralPath (Join-Path $RepositoryRoot `
    'TIEconomyMod\Patches\ProbeSurveyUiPatches.cs') -Raw
$probeNotificationSource = Get-Content -LiteralPath (Join-Path $RepositoryRoot `
    'TIEconomyMod\Core\ProbeSurveyNotifications.cs') -Raw
if (-not $probePatchSource.Contains(
        '__instance.CanProspectFromShip(spaceBody)')) {
    throw 'Site-probe availability must retain the native colonization gate.'
}
if (-not $probePatchSource.Contains(
        'faction.CanProspectWithProbe(body, false)')) {
    throw 'Launch Probe visibility must use the native prospecting gate.'
}
if (-not $probePatchSource.Contains(
        'typeof(LaunchAllProbeOperation)') -or
    -not $probePatchSource.Contains(
        'ProbeSurveyRuntime.EligibleSites(faction, body)')) {
    throw 'Bulk probe launches must enumerate eligible sites rather than bodies.'
}
if (-not $probeUiPatchSource.Contains(
        'TIOperationTargeting_HabSite') -and
    -not $probePatchSource.Contains(
        '__result = typeof(TIOperationTargeting_HabSite)')) {
    throw 'Individual probe launches must retain the hab-site targeting method.'
}
if (-not $probeUiPatchSource.Contains(
        'IntelSpaceBodyListItemController.OnClickProspectButton') -or
    -not $probeUiPatchSource.Contains(
        'OperationCanvasController.Singleton.OnOperationSelected')) {
    throw 'The Intel probe button must enter the site-target selection flow.'
}
if (-not $probeUiPatchSource.Contains(
        'ProbeSurveyRuntime.EligibleSites(') -or
    -not $probePatchSource.Contains(
        'ProbeSurveyRuntime.LegacyBodyProspectorEnRoute(')) {
    throw 'The body row must allow another eligible site while probes are in flight.'
}
if (-not $probeUiPatchSource.Contains(
        'ProbeSurveyRuntime.SiteProspected(faction, site)') -or
    -not $probeUiPatchSource.Contains(
        'ProbeSurveyRuntime.SurveyedSites(')) {
    throw 'Site markers, output, and Intel lists must share the per-site survey state.'
}
if (-not $probePatchSource.Contains(
        'ProbeSurveyRuntime.EarliestProspectorArrival(') -or
    -not $probePatchSource.Contains(
        '__result = ProbeSurveyRuntime.BodyHasProspectorEnRoute(')) {
    throw 'Body completion state must come from the shared pending-event authority.'
}
$probeRuntimeSource = Get-Content -LiteralPath (Join-Path $RepositoryRoot `
    'TIEconomyMod\Core\ProbeSurveyRuntime.cs') -Raw
foreach ($requiredEventGuard in @(
        'GameStateManager.GetAllGameStates<TITimeEvent>()',
        'pendingEvent.isComplete',
        'pendingEvent.archived',
        'pendingEvent.eventDataTemplateName',
        'ReferenceEquals(pendingEvent.eventObject2, target)')) {
    if (-not $probeRuntimeSource.Contains($requiredEventGuard)) {
        throw "Probe-in-flight state is missing event guard: $requiredEventGuard"
    }
}
if ($probeUiPatchSource.Contains('AssetCacheManager.')) {
    throw ('Survey UI patches must not statically dereference AssetCacheManager; ' +
        'Harmony applies them before Unity initializes its asset fields.')
}
if ($probeUiPatchSource.Contains(
        'nameof(IntelSpaceBodyListItemController.Refresh)') -or
    $probeUiPatchSource.Contains(
        'nameof(HabSiteController.GetEmptyHabSiteIcon)')) {
    throw ('Survey UI patches must not wrap methods that directly access ' +
        'AssetCacheManager during startup-time Harmony compilation.')
}
if (-not $probeUiPatchSource.Contains('"prospectedHabSiteIcon"') -or
    -not $probeUiPatchSource.Contains('ProspectedHabSiteIcon.GetValue(null)')) {
    throw 'Surveyed site markers must resolve the initialized icon lazily.'
}
if (-not $probeUiPatchSource.Contains(
        'SurveyedSiteIconRuntime.EnsureInstalled()') -or
    -not $probeUiPatchSource.Contains(
        '"GetEmptyHabSiteIcon"') -or
    -not $probeUiPatchSource.Contains(
        'new Harmony(Main.mod.Info.Id).Patch(')) {
    throw 'The shared empty-site icon hook must install only from the delayed runtime path.'
}
if (-not $probeUiPatchSource.Contains(
        'typeof(TIHabSiteState)') -or
    -not $probeUiPatchSource.Contains(
        'nameof(TIHabSiteState.ProductivityString)') -or
    -not $probeUiPatchSource.Contains(
        'typeof(BaseSiteListItemController)')) {
    throw 'Colonization and natural-body site output must consume per-site survey state.'
}
if (-not $probeNotificationSource.Contains(
        'site.ProductivityString(true)') -or
    $probeNotificationSource.Contains('foreach (TIHabSiteState')) {
    throw 'Probe completion must report exactly one surveyed site.'
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
    'PavonisInteractive.TerraInvicta.IntelSpaceBodyListItemController',
    'PavonisInteractive.TerraInvicta.IntelScreenController',
    'PavonisInteractive.TerraInvicta.IntelHabSiteListPane',
    'PavonisInteractive.TerraInvicta.HabSiteController',
    'PavonisInteractive.TerraInvicta.BaseSiteListItemController',
    'PavonisInteractive.TerraInvicta.TIFactionState',
    'PavonisInteractive.TerraInvicta.TIHabSiteState',
    'PavonisInteractive.TerraInvicta.TITimeEvent')
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

$stateMathType = $modAssembly.GetType(
    'TIEconomyMod.ProbeSurveyStateMath', $true)
$siteProspectedMethod = $stateMathType.GetMethod(
    'SiteProspected',
    [Reflection.BindingFlags]::NonPublic -bor
    [Reflection.BindingFlags]::Static)
$siteEnRouteMethod = $stateMathType.GetMethod(
    'SiteProspectorEnRoute',
    [Reflection.BindingFlags]::NonPublic -bor
    [Reflection.BindingFlags]::Static)
if ($null -eq $siteProspectedMethod -or $null -eq $siteEnRouteMethod) {
    throw 'ProbeSurveyStateMath does not expose its survey-state authority.'
}
$legacyLunaSite = $siteProspectedMethod.Invoke(
    $null, @([single]1.0, [single]0.0, [single]1.0))
$marsCompletedSite = $siteProspectedMethod.Invoke(
    $null, @([single]0.0, [single]1.0, [single]1.0))
$marsPendingSite = $siteProspectedMethod.Invoke(
    $null, @([single]0.0, [single]0.1, [single]1.0))
$marsProbeEnRoute = $siteEnRouteMethod.Invoke(
    $null, @(
        [single]0.0,
        [single]0.1,
        [single]0.1,
        [single]1.0))
if (-not $legacyLunaSite -or -not $marsCompletedSite -or
    $marsPendingSite -or -not $marsProbeEnRoute) {
    throw ('Legacy Luna and per-site Mars survey-state compatibility ' +
        'semantics changed unexpectedly.')
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
    'TIEconomyMod.Patches.SurveyedBaseTargetsPatch',
    'TIEconomyMod.Patches.SurveyedSiteProductivityPatch')

# UI patch methods reference Unity native calls which desktop PowerShell cannot
# JIT safely (Harmony reports an ECall SecurityException outside the Unity
# runtime). Resolve every class and target signature here; the compiled DLL and
# in-game PatchAll provide the remaining runtime coverage.
$uiPatchContracts = @(
    @('TIEconomyMod.Patches.IntelProbeSiteSelectionPatch',
      'PavonisInteractive.TerraInvicta.IntelSpaceBodyListItemController',
      'OnClickProspectButton'),
    @('TIEconomyMod.Patches.SurveyedSiteOutputPatch',
      'PavonisInteractive.TerraInvicta.HabSiteController',
      'BuildOutputString'),
    @('TIEconomyMod.Patches.SurveyedSiteMarkerPatch',
      'PavonisInteractive.TerraInvicta.HabSiteController',
      'SetMarkerData'),
    @('TIEconomyMod.Patches.SurveyedBodySiteListItemPatch',
      'PavonisInteractive.TerraInvicta.BaseSiteListItemController',
      'SetListItem'),
    @('TIEconomyMod.Patches.SurveyedIntelSiteModelsPatch',
      'PavonisInteractive.TerraInvicta.IntelScreenController',
      'SetHabSiteListModelData'),
    @('TIEconomyMod.Patches.SurveyedIntelSiteItemsPatch',
      'PavonisInteractive.TerraInvicta.IntelHabSiteListPane',
      'ItemsToDisplay'),
    @('TIEconomyMod.Patches.SurveyedIntelSiteTabActivePlayerPatch',
      'PavonisInteractive.TerraInvicta.IntelScreenController',
      'UpdateActivePlayerUIElements'),
    @('TIEconomyMod.Patches.SurveyedIntelSiteTabRefreshPatch',
      'PavonisInteractive.TerraInvicta.IntelScreenController',
      'RefreshAll'))
$allMethodFlags = [Reflection.BindingFlags]::Public -bor
    [Reflection.BindingFlags]::NonPublic -bor
    [Reflection.BindingFlags]::Instance -bor
    [Reflection.BindingFlags]::Static
foreach ($contract in $uiPatchContracts) {
    [void]$modAssembly.GetType($contract[0], $true)
    $targetType = $gameAssembly.GetType($contract[1], $true)
    if ($null -eq $targetType.GetMethod(
            $contract[2],
            $allMethodFlags)) {
        throw "Required UI survey target is missing: $($contract[1]).$($contract[2])"
    }
}
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

Write-Host ('PASS: per-site targeting/UI/notification paths, legacy Luna ' +
    'intel, per-site Mars intel, 0.325-tonne surveys, 25-site Mars bulk ' +
    'launch, scenario state, lunar costs, and Harmony targets validate.')
