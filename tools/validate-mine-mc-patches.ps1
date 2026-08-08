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
        throw "Required mine-MC validation assembly is missing: $Path"
    }
    return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
}

$harmonyAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($unityAssemblyName in @(
    'UnityEngine.CoreModule.dll',
    'UnityEngine.dll',
    'UnityEngine.IMGUIModule.dll',
    'UnityEngine.UI.dll',
    'UnityEngine.UIModule.dll',
    'Unity.TextMeshPro.dll'
)) {
    $unityAssemblyPath = Join-Path $TargetManagedDir $unityAssemblyName
    if (Test-Path -LiteralPath $unityAssemblyPath) {
        [void](Load-AssemblyBytes $unityAssemblyPath)
    }
}
[void](Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$gameAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
$modAssembly = Load-AssemblyBytes $ModAssemblyPath

$factionType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TIFactionState',
    $true)
$bodyType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TISpaceBodyState',
    $true)
$moduleType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TIHabModuleState',
    $true)
$resourceType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.FactionResource',
    $true)
$generalControlsType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.GeneralControlsController',
    $true)
$instanceFlags = [Reflection.BindingFlags]'Public,Instance'
$staticFlags = [Reflection.BindingFlags]'Public,Static'

$contracts = @(
    @{
        Target = $factionType.GetMethod(
            'GetMissionControlRequirementFromMineNetwork',
            $instanceFlags,
            $null,
            [Type[]]@([int]),
            $null)
        Patch = 'TIEconomyMod.Patches.MineNetworkMissionControlPatch'
        PatchMethod = 'Prefix'
        HarmonyArgument = 1
    },
    @{
        Target = $factionType.GetMethod(
            'GetMissionControlRequirementFromNextMine',
            $instanceFlags,
            $null,
            [Type[]]@($bodyType),
            $null)
        Patch = 'TIEconomyMod.Patches.NextMineMissionControlPatch'
        PatchMethod = 'Prefix'
        HarmonyArgument = 1
    },
    @{
        Target = $factionType.GetMethod(
            'GetMissionControlGainedFromTurningOffMine',
            $instanceFlags,
            $null,
            [Type[]]@($moduleType),
            $null)
        Patch = 'TIEconomyMod.Patches.DisabledMineMissionControlPatch'
        PatchMethod = 'Prefix'
        HarmonyArgument = 1
    },
    @{
        Target = $factionType.GetProperty(
            'SafeMineNextworkSize',
            $instanceFlags).GetMethod
        Patch = 'TIEconomyMod.Patches.FreeMineAllowancePatch'
        PatchMethod = 'Prefix'
        HarmonyArgument = 1
    },
    @{
        Target = $generalControlsType.GetMethod(
            'ResourceReportString',
            $staticFlags,
            $null,
            [Type[]]@($factionType, $resourceType),
            $null)
        Patch = 'TIEconomyMod.Patches.MissionControlUsageColorPatch'
        PatchMethod = 'Postfix'
        HarmonyArgument = 2
    }
)

$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyMethodType = $harmonyAssembly.GetType('HarmonyLib.HarmonyMethod', $true)
$patchMethod = @($harmonyType.GetMethods(
    [Reflection.BindingFlags]'Public,Instance') | Where-Object {
        $_.Name -eq 'Patch' -and $_.GetParameters().Count -eq 5
    })
if ($patchMethod.Count -ne 1) {
    throw "Expected one Harmony.Patch overload, found $($patchMethod.Count)."
}
$harmony = [Activator]::CreateInstance(
    $harmonyType,
    [object[]]@('ti-eeo.mine-mc-validation.' + [Guid]::NewGuid().ToString('N')))

foreach ($contract in $contracts) {
    if ($null -eq $contract.Target) {
        throw "Installed TI 1.0.51 mine-MC target for '$($contract.Patch)' was not found."
    }
    $patchType = $modAssembly.GetType($contract.Patch, $true)
    $patchMethodInfo = $patchType.GetMethod(
        $contract.PatchMethod,
        [Reflection.BindingFlags]'Public,Static')
    if ($null -eq $patchMethodInfo) {
        throw "Mine-MC patch '$($contract.Patch).$($contract.PatchMethod)' was not found."
    }

    $harmonyPatch = [Activator]::CreateInstance(
        $harmonyMethodType,
        [object[]]@($patchMethodInfo))
    $arguments = [object[]]::new(5)
    $arguments[0] = $contract.Target
    $arguments[$contract.HarmonyArgument] = $harmonyPatch
    try {
        $replacement = $patchMethod[0].PSObject.BaseObject.Invoke(
            $harmony,
            $arguments)
    }
    catch {
        if ($_.Exception.InnerException) {
            throw $_.Exception.InnerException
        }
        throw
    }
    if ($null -eq $replacement) {
        throw "Harmony did not emit '$($contract.Patch)'."
    }
}

$mathType = $modAssembly.GetType('TIEconomyMod.MineMissionControlMath', $true)
$tierCost = $mathType.GetMethod(
    'TierCost',
    [Reflection.BindingFlags]'NonPublic,Static')
foreach ($tier in 1..3) {
    $actual = $tierCost.Invoke($null, [object[]]@($tier))
    if ([int]$actual -ne $tier) {
        throw "Tier $tier mine costs $actual MC instead of $tier."
    }
}

$usageDisplayState = $mathType.GetMethod(
    'UsageDisplayState',
    [Reflection.BindingFlags]'NonPublic,Static')
foreach ($case in @(
    @{ Usage = 75.0; Capacity = 100.0; Expected = 'Normal' },
    @{ Usage = 75.01; Capacity = 100.0; Expected = 'Warning' },
    @{ Usage = 100.0; Capacity = 100.0; Expected = 'Warning' },
    @{ Usage = 100.01; Capacity = 100.0; Expected = 'OverCapacity' },
    @{ Usage = 0.0; Capacity = 0.0; Expected = 'Normal' }
)) {
    $actual = $usageDisplayState.Invoke(
        $null,
        [object[]]@([float]$case.Usage, [float]$case.Capacity)).ToString()
    if ($actual -ne $case.Expected) {
        throw "MC usage $($case.Usage)/$($case.Capacity) displayed as $actual instead of $($case.Expected)."
    }
}

Write-Host 'PASS: mine MC patches bind to TI 1.0.51 and Harmony emits all five replacements; Tier 1/2/3 costs are 1/2/3 and MC colors use the 75%/100% boundaries.'
