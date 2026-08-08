[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$TargetManagedDir,
    [Parameter(Mandatory = $true)]
    [string]$ModAssemblyPath
)

$ErrorActionPreference = 'Stop'

function Load-AssemblyBytes {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required weapon-cadence assembly is missing: $Path"
    }
    return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
}

$harmonyAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($assemblyName in @(
    'Newtonsoft.Json.dll',
    'FMODUnity.dll',
    'Unity.Burst.dll',
    'Unity.Collections.dll',
    'Unity.Jobs.dll',
    'Unity.Mathematics.dll',
    'Unity.Entities.dll',
    'UnityEngine.CoreModule.dll',
    'UnityEngine.dll',
    'UnityEngine.IMGUIModule.dll',
    'UnityEngine.ParticleSystemModule.dll',
    'UnityEngine.PhysicsModule.dll',
    'UnityEngine.UI.dll',
    'UnityEngine.UIModule.dll',
    'Unity.TextMeshPro.dll'
)) {
    $assemblyPath = Join-Path $TargetManagedDir $assemblyName
    if (Test-Path -LiteralPath $assemblyPath) {
        [void](Load-AssemblyBytes $assemblyPath)
    }
}
[void](Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$gameAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
$modAssembly = Load-AssemblyBytes $ModAssemblyPath

$patchProcessorType = $harmonyAssembly.GetType(
    'HarmonyLib.PatchProcessor', $true)
$instructionReader = @($patchProcessorType.GetMethods(
    [Reflection.BindingFlags]'Public,Static') | Where-Object {
        $_.Name -eq 'GetOriginalInstructions' -and
        $_.GetParameters().Count -eq 2 -and
        -not $_.GetParameters()[1].ParameterType.IsByRef
    })
if ($instructionReader.Count -ne 1) {
    throw "Expected one usable Harmony instruction reader, found $($instructionReader.Count)."
}

function Read-Instructions {
    param([Reflection.MethodBase]$Method)

    $arguments = [object[]]::new(2)
    $arguments[0] = $Method
    $arguments[1] = $null
    return $instructionReader[0].PSObject.BaseObject.Invoke(
        $null, $arguments)
}

$managerType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.SpaceCombatManager', $true)
$weaponInterface = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.Ship.IWeapon', $true)
$quarterSecond = $managerType.GetMethod(
    'CombatQuarterSecond',
    [Reflection.BindingFlags]'Public,NonPublic,Instance')
$fractionalSecond = $managerType.GetMethod(
    'CombatFractionalSecond',
    [Reflection.BindingFlags]'Public,NonPublic,Instance')
$acquireTarget = $weaponInterface.GetMethod('AcquireTarget')
$tryFire = $weaponInterface.GetMethod('TryFire')
if ($null -eq $quarterSecond -or
    $null -eq $fractionalSecond -or
    $null -eq $acquireTarget -or
    $null -eq $tryFire) {
    throw 'One or more TI 1.0.51 weapon-cadence targets are missing.'
}

$readerArguments = [object[]]::new(2)
$readerArguments[0] = $quarterSecond
$readerArguments[1] = $null
$originalEnumerable = $instructionReader[0].PSObject.BaseObject.Invoke(
    $null, $readerArguments)
$original = @($originalEnumerable)
$originalAcquireCalls = @($original | Where-Object {
    $_.operand -eq $acquireTarget
})
$originalFireCalls = @($original | Where-Object {
    $_.operand -eq $tryFire
})
if ($originalAcquireCalls.Count -ne 2 -or
    $originalFireCalls.Count -ne 2) {
    throw "Native cadence shape changed: AcquireTarget=$($originalAcquireCalls.Count), TryFire=$($originalFireCalls.Count)."
}

$runtimeType = $modAssembly.GetType(
    'TIEconomyMod.Patches.WeaponCadenceRuntime', $true)
$suppression = $runtimeType.GetMethod(
    'SuppressNativeAcquireTarget',
    [Reflection.BindingFlags]'Public,Static')
$run = $runtimeType.GetMethod(
    'Run', [Reflection.BindingFlags]'Public,Static')
$clear = $runtimeType.GetMethod(
    'Clear', [Reflection.BindingFlags]'Public,Static')
$checkAll = $runtimeType.GetMethod(
    'CheckAllWeapons', [Reflection.BindingFlags]'NonPublic,Static')
$tryWeapon = $runtimeType.GetMethod(
    'TryWeapon', [Reflection.BindingFlags]'NonPublic,Static')
if ($null -eq $suppression -or
    $null -eq $run -or
    $null -eq $clear -or
    $null -eq $checkAll -or
    $null -eq $tryWeapon) {
    throw 'Universal weapon-cadence runtime is incomplete.'
}

$suppressionPatchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.NativeWeaponCadenceSuppressionPatch', $true)
$transpiler = $suppressionPatchType.GetMethod(
    'Transpiler', [Reflection.BindingFlags]'Public,Static')
try {
    $arguments = [object[]]::new(1)
    $arguments[0] = $originalEnumerable
    $patched = @($transpiler.Invoke($null, $arguments))
}
catch {
    $failure = $_.Exception
    while ($failure.InnerException) {
        $failure = $failure.InnerException
    }
    throw $failure
}

$remainingAcquireCalls = @($patched | Where-Object {
    $_.operand -eq $acquireTarget
})
$suppressionCalls = @($patched | Where-Object {
    $_.operand -eq $suppression
})
if ($remainingAcquireCalls.Count -ne 0 -or
    $suppressionCalls.Count -ne 2 -or
    $patched.Count -ne $original.Count) {
    throw "Cadence suppression mismatch: native=$($remainingAcquireCalls.Count), replacements=$($suppressionCalls.Count), instructions=$($original.Count)->$($patched.Count)."
}

$runCalls = @(
    @(Read-Instructions $run) | Where-Object {
        $_.operand -is [Reflection.MethodInfo]
    } | ForEach-Object { $_.operand.Name })
foreach ($requiredCall in @(
    'AccumulateChecks',
    'OldestCheckOffset_s',
    'CheckAllWeapons'
)) {
    if ($runCalls -notcontains $requiredCall) {
        throw "Universal cadence driver no longer calls '$requiredCall'."
    }
}

$checkCalls = @(
    @(Read-Instructions $checkAll) | Where-Object {
        $_.operand -is [Reflection.MethodInfo]
    } | ForEach-Object { $_.operand.Name })
foreach ($requiredCall in @('IterateByClass', 'TryWeapon')) {
    if ($checkCalls -notcontains $requiredCall) {
        throw "Universal cadence scan no longer calls '$requiredCall'."
    }
}

$weaponCalls = @(
    @(Read-Instructions $tryWeapon) | Where-Object {
        $_.operand -is [Reflection.MethodInfo]
    } | ForEach-Object { $_.operand.Name })
foreach ($requiredCall in @(
    'AcquireTarget',
    'SelectWeaponVisualization',
    'TryFire'
)) {
    if ($weaponCalls -notcontains $requiredCall) {
        throw "Universal weapon check no longer calls '$requiredCall'."
    }
}

$mathType = $modAssembly.GetType(
    'TIEconomyMod.WeaponCadenceMath', $true)
$interval = $mathType.GetField(
    'CheckInterval_s',
    [Reflection.BindingFlags]'Public,Static').GetRawConstantValue()
if ([Math]::Abs([double]$interval - 0.05) -gt 0.000000001) {
    throw "Weapon cadence interval is $interval instead of 0.05 seconds."
}

$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyId = 'ti-eeo.weapon-cadence-validation.' +
    [Guid]::NewGuid().ToString('N')
$harmony = [Activator]::CreateInstance(
    $harmonyType, [object[]]@($harmonyId))
try {
    [void]$harmony.CreateClassProcessor($suppressionPatchType).Patch()
    $framePatchType = $modAssembly.GetType(
        'TIEconomyMod.Patches.FiftyMillisecondWeaponCadencePatch', $true)
    [void]$harmony.CreateClassProcessor($framePatchType).Patch()
}
catch {
    $failure = $_.Exception
    while ($failure.InnerException) {
        $failure = $failure.InnerException
    }
    throw $failure
}
finally {
    $harmony.UnpatchAll($harmonyId)
}

Write-Host 'PASS: all ship and hab weapons replace the native one-attempt-per-second partition with native acquisition and fire checks on a guarded 50 ms combat-time grid.'
