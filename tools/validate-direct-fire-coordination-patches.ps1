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
        throw "Required direct-fire coordination assembly is missing: $Path"
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

$findTargetType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.GamePlayScript.AI.FindTargetShipLeafNode',
    $true)
$findTarget = $findTargetType.GetMethod(
    'TryAssignTargetShip',
    [Reflection.BindingFlags]'NonPublic,Instance')
if ($null -eq $findTarget) {
    throw 'FindTargetShipLeafNode.TryAssignTargetShip is missing.'
}

$targetPatchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.AutomaticShipTargetCommitmentGatePatch', $true)
$transpiler = $targetPatchType.GetMethod(
    'Transpiler', [Reflection.BindingFlags]'Public,Static')
$readerArguments = [object[]]::new(2)
$readerArguments[0] = $findTarget
$readerArguments[1] = $null
$original = $instructionReader[0].PSObject.BaseObject.Invoke(
    $null, $readerArguments)
try {
    $arguments = [object[]]::new(1)
    $arguments[0] = $original
    $patched = @($transpiler.Invoke($null, $arguments))
}
catch {
    $failure = $_.Exception
    while ($failure.InnerException) {
        $failure = $failure.InnerException
    }
    throw $failure
}

$runtimeType = $modAssembly.GetType(
    'TIEconomyMod.Patches.DirectFireTargetingRuntime', $true)
$candidateHelper = $runtimeType.GetMethod(
    'IsAutomaticTargetAvailable',
    [Reflection.BindingFlags]'Public,Static')
$candidateCalls = @($patched | Where-Object {
    $_.opcode.Name -eq 'call' -and $_.operand -eq $candidateHelper
})
if ($candidateCalls.Count -ne 3 -or
    $patched.Count -ne $original.Count + 18) {
    throw "Automatic target transpiler mismatch: helper calls=$($candidateCalls.Count), instructions=$($original.Count)->$($patched.Count)."
}

$suppressMethod = $runtimeType.GetMethod(
    'ShouldSuppressAutomaticFire',
    [Reflection.BindingFlags]'Public,Static')
$suppressCalls = @(Read-Instructions $suppressMethod | Where-Object {
    $_.operand -is [Reflection.MethodInfo]
} | ForEach-Object { $_.operand.Name })
foreach ($requiredCall in @(
    'get_isMissileWeapon',
    'get_currentFireMode',
    'get_combatAIControl',
    'get_primaryTarget',
    'IsSaturated'
)) {
    if ($suppressCalls -notcontains $requiredCall) {
        throw "Automatic fire gate no longer calls '$requiredCall'."
    }
}

$requiredTargets = @(
    @('PavonisInteractive.TerraInvicta.Ship.ProjectileWeapon', 'TryFire', 'Public,Instance'),
    @('PavonisInteractive.TerraInvicta.SpaceCombat.BallisticProjectileController', 'Fire', 'Public,Instance'),
    @('PavonisInteractive.TerraInvicta.SpaceCombat.ProjectileController', 'Destruct', 'Public,Instance'),
    @('PavonisInteractive.TerraInvicta.Jobs.ProjectileJobContainer', 'ClearAllJobs', 'Public,Instance'),
    @('PavonisInteractive.TerraInvicta.Ship.TIAttackFireMode', 'GetExpectedDamage', 'Public,Instance')
)
foreach ($target in $requiredTargets) {
    $type = $gameAssembly.GetType($target[0], $true)
    $method = $type.GetMethod(
        $target[1], [Reflection.BindingFlags]$target[2])
    if ($null -eq $method) {
        throw "Direct-fire target '$($target[0]).$($target[1])' is missing."
    }
}

$patchTypeNames = @(
    'TIEconomyMod.Patches.ProjectileWeaponCommitmentContextPatch',
    'TIEconomyMod.Patches.BallisticProjectileCommitmentPatch',
    'TIEconomyMod.Patches.ProjectileCommitmentCleanupPatch',
    'TIEconomyMod.Patches.ProjectileCommitmentBattleCleanupPatch',
    'TIEconomyMod.Patches.AutomaticFireCommitmentGatePatch',
    'TIEconomyMod.Patches.AutomaticShipTargetCommitmentGatePatch'
)
foreach ($patchTypeName in $patchTypeNames) {
    if ($null -eq $modAssembly.GetType($patchTypeName, $false)) {
        throw "Packaged direct-fire patch '$patchTypeName' is missing."
    }
}

foreach ($missileTypeName in @(
    'PavonisInteractive.TerraInvicta.GamePlayScript.AI.FindMissileTargetShipLeafNode',
    'PavonisInteractive.TerraInvicta.GamePlayScript.AI.FireMissilesLeafNode',
    'PavonisInteractive.TerraInvicta.Ship.SalvoFireMode'
)) {
    $missileType = $gameAssembly.GetType($missileTypeName, $true)
    foreach ($method in $missileType.GetMethods(
        [Reflection.BindingFlags]'Public,NonPublic,Instance,Static')) {
        $patchInfo = $harmonyAssembly.GetType(
            'HarmonyLib.Harmony', $true).GetMethod(
                'GetPatchInfo',
                [Reflection.BindingFlags]'Public,Static').Invoke(
                    $null, @($method))
        if ($null -ne $patchInfo) {
            $owners = @($patchInfo.Owners | Where-Object {
                $_ -eq 'EconomyMod'
            })
            if ($owners.Count -gt 0) {
                throw "Missile method '$missileTypeName.$($method.Name)' was unexpectedly patched."
            }
        }
    }
}

Write-Host 'PASS: direct-fire commitments patch actual ballistics and automatic acquisition while missile AI and deliberate fire paths remain untouched.'
