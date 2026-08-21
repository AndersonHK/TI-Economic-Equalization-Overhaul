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
        throw "Required hab-event validation assembly is missing: $Path"
    }
    return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
}

$templatePath = Join-Path (
    Split-Path -Parent $TargetManagedDir) `
    'StreamingAssets\Templates\TINarrativeEventTemplate.json'
$events = Get-Content -LiteralPath $templatePath -Raw | ConvertFrom-Json
$meteor = @($events | Where-Object dataName -eq 'event_MeteorStrike')
$accident = @($events | Where-Object dataName -eq 'event_HabAccident')
$debris = @($events | Where-Object dataName -eq 'event_OrbitalDebrisStrike')
$malfunction = @($events |
    Where-Object dataName -eq 'event_HabModuleMalfunction')
foreach ($selection in @($meteor, $accident, $debris, $malfunction)) {
    if ($selection.Count -ne 1) {
        throw 'An expected installed hab-loss event was not found exactly once.'
    }
}

Assert-Close 1 ([double]$meteor[0].baseWeight) 'Meteor Strike base weight'
Assert-Close 60 ([double]$meteor[0].global_cooldown_months) `
    'Meteor Strike global cooldown'
Assert-Close 2 ([double]$accident[0].baseWeight) 'Hab Accident base weight'
Assert-Close 12 ([double]$accident[0].global_cooldown_months) `
    'Hab Accident global cooldown'
Assert-Close 36 ([double]$accident[0].target_cooldown_months) `
    'Hab Accident target cooldown'

$supportFailureCondition = @($accident[0].targetConditions |
    Where-Object {
        $_.'$type' -eq 'TIFactionCondition_iHabSupportFailureLevel' -and
        $_.sign -eq 'GreaterThan' -and
        $_.strValue -eq '0'
    })
if ($supportFailureCondition.Count -ne 1) {
    throw 'Hab Accident no longer has its native positive support-failure gate.'
}
$debrisCondition = @($debris[0].targetConditions | Where-Object {
    $_.'$type' -eq 'TIOrbitCondition_iDestroyedAssetsInOrbit' -and
    $_.sign -eq 'GreaterThan' -and
    $_.strValue -eq '0'
})
if ($debrisCondition.Count -ne 1) {
    throw 'Orbital Debris Strike no longer requires debris in the target orbit.'
}
if (-not [bool]$malfunction[0].forceEvent) {
    throw 'Hab Module Malfunction is no longer a forced event.'
}

$harmonyAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($unityAssembly in Get-ChildItem `
    -LiteralPath $TargetManagedDir `
    -File `
    -Filter 'Unity*.dll') {
    [void](Load-AssemblyBytes $unityAssembly.FullName)
}
$fmodAssembly = Join-Path $TargetManagedDir 'FMODUnity.dll'
if (Test-Path -LiteralPath $fmodAssembly) {
    [void](Load-AssemblyBytes $fmodAssembly)
}
[void](Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
[void](Load-AssemblyBytes (Join-Path $TargetManagedDir 'Assembly-CSharp.dll'))
$modAssembly = Load-AssemblyBytes $ModAssemblyPath

$mathType = $modAssembly.GetType(
    'TIEconomyMod.HabEventExposureMath', $true)
$flags = [Reflection.BindingFlags]::NonPublic -bor
    [Reflection.BindingFlags]::Static
$exposureMethod = $mathType.GetMethod('ExposureMultiplier', $flags)
$adjustMethod = $mathType.GetMethod('AdjustSelectionWeight', $flags)
if ($null -eq $exposureMethod -or $null -eq $adjustMethod) {
    throw 'The orbital-hab exposure formula methods are missing.'
}
Assert-Close (1.0 / 30.0) `
    ([double]$exposureMethod.Invoke($null, @(1))) `
    'One-hab exposure multiplier'
Assert-Close 0.5 `
    ([double]$exposureMethod.Invoke($null, @(15))) `
    'Fifteen-hab exposure multiplier'
Assert-Close 1 `
    ([double]$exposureMethod.Invoke($null, @(30))) `
    'Thirty-hab exposure multiplier'
Assert-Close 1 `
    ([double]$exposureMethod.Invoke($null, @(31))) `
    'Above-threshold exposure multiplier'
Assert-Close 4 `
    ([double]$adjustMethod.Invoke(
        $null,
        @('event_OrbitalDebrisStrike', [single]4, 1))) `
    'Orbital Debris Strike exclusion'
Assert-Close 7 `
    ([double]$adjustMethod.Invoke(
        $null,
        @('event_HabModuleMalfunction', [single]7, 1))) `
    'Hab Module Malfunction exclusion'

$patchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.AmbientHabHazardWeightPatch', $true)
$targetMethodResolver = $patchType.GetMethod('TargetMethod', $flags)
if ($null -eq $targetMethodResolver) {
    throw 'Ambient-hab hazard patch does not expose its guarded target resolver.'
}
$resolvedTarget = $targetMethodResolver.Invoke($null, @())
if ($null -eq $resolvedTarget -or
    $resolvedTarget.Name -ne '<NarrativeEventsMonthlyUpdate>b__241_2') {
    throw "Ambient-hab hazard patch resolved the wrong selector: $($resolvedTarget.Name)"
}
$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyId = 'ti.eeo.validate.hab-event-exposure.' +
    [Guid]::NewGuid().ToString('N')
$harmony = [Activator]::CreateInstance($harmonyType, @($harmonyId))
try {
    try {
        $patchedMethods = @($harmony.CreateClassProcessor($patchType).Patch())
    }
    catch {
        throw "Failed applying ambient-hab hazard patch: $($_.Exception.ToString())"
    }
    if ($patchedMethods.Count -ne 1) {
        throw "Ambient-hab hazard patch emitted $($patchedMethods.Count) target methods instead of one."
    }
}
finally {
    $harmony.UnpatchAll($harmonyId)
}

Write-Host ('PASS: ambient Meteor Strike and Hab Accident weights scale ' +
    'linearly to 30 orbital habs; caused hazards remain native.')
