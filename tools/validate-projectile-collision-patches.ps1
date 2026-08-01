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
        throw "Required projectile-collision assembly is missing: $Path"
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

$ballisticType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.SpaceCombat.BallisticProjectileController',
    $true)
$target = $ballisticType.GetMethod(
    'UpdateController', [Reflection.BindingFlags]'Public,Instance')
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

$readerArguments = [object[]]::new(2)
$readerArguments[0] = $target
$readerArguments[1] = $null
$original = $instructionReader[0].PSObject.BaseObject.Invoke(
    $null, $readerArguments)
$patchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.ProjectileMovementSweepPatch', $true)
$transpiler = $patchType.GetMethod(
    'Transpiler', [Reflection.BindingFlags]'Public,Static')
$originalMargins = @($original | Where-Object {
    $_.opcode.Name -eq 'ldc.r4' -and [Math]::Abs([single]$_.operand - 1.2) -lt 0.000001
})
try {
    $transpilerArguments = [object[]]::new(1)
    $transpilerArguments[0] = $original
    $patched = @($transpiler.Invoke($null, $transpilerArguments))
}
catch {
    $failure = $_.Exception
    while ($failure.InnerException) {
        $failure = $failure.InnerException
    }
    throw $failure
}

$patchedMargins = @($patched | Where-Object {
    $_.opcode.Name -eq 'ldc.r4' -and [Math]::Abs([single]$_.operand - 1.2) -lt 0.000001
})
$sweepHelper = $patchType.GetMethod(
    'ActiveMovementSweepMultiplier', [Reflection.BindingFlags]'Public,Static')
$helperCalls = @($patched | Where-Object {
    $_.opcode.Name -eq 'call' -and $_.operand -eq $sweepHelper
})
if ($originalMargins.Count -ne 1 -or
    $patchedMargins.Count -ne 0 -or
    $helperCalls.Count -ne 1 -or
    $patched.Count -ne @($original).Count) {
    throw "Ballistic sweep patch mismatch: original 1.2=$($originalMargins.Count), patched 1.2=$($patchedMargins.Count), helper calls=$($helperCalls.Count), instructions=$(@($original).Count)->$($patched.Count)."
}

$projectileControllerType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.SpaceCombat.ProjectileController', $true)
$projectileStateType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TISpaceCombatProjectileState', $true)
if ($null -eq $ballisticType.GetMethod('Fire') -or
    $null -eq $projectileControllerType.GetMethod('ApplyDamage') -or
    @($projectileStateType.GetMethods() | Where-Object {
        $_.Name -eq 'CrossSectionalArea_m2' -and $_.IsStatic -and
        $_.GetParameters().Count -eq 2
    }).Count -ne 1) {
    throw 'One or more projectile geometry or durability targets are missing.'
}

$patchTypeNames = @(
    'TIEconomyMod.Patches.ProjectileColliderSizingPatch',
    'TIEconomyMod.Patches.ProjectileCrossSectionPatch',
    'TIEconomyMod.Patches.ProjectileMovementSweepPatch',
    'TIEconomyMod.Patches.NavalGunProjectileDurabilityPatch'
)
foreach ($patchTypeName in $patchTypeNames) {
    if ($null -eq $modAssembly.GetType($patchTypeName, $false)) {
        throw "Packaged projectile patch '$patchTypeName' is missing."
    }
}

Write-Host 'PASS: projectile geometry and durability targets exist and the guarded sweep rewrites exactly 120% to 100% for TI 1.0.51.'
