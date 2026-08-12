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
        throw "Required ship-power validation assembly is missing: $Path"
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

$shipType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TISpaceShipState', $true)
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

$cases = @(
    @(
        'CombatPerQuarterSecondChanges',
        'TIEconomyMod.Patches.GeneratedPowerHeatPatch',
        'ApplyCorrectedGenerationHeat'
    ),
    @(
        'CombatPerSecondChanges',
        'TIEconomyMod.Patches.DuplicateSystemsHeatPatch',
        'ApplyLegacySystemsHeat'
    )
)

foreach ($case in $cases) {
    $targetName = $case[0]
    $patchTypeName = $case[1]
    $helperName = $case[2]
    $target = $shipType.GetMethod(
        $targetName, [Reflection.BindingFlags]'Public,Instance')
    if ($null -eq $target) {
        throw "Target method '$targetName' was not found."
    }

    $readerArguments = [object[]]::new(2)
    $readerArguments[0] = $target
    $readerArguments[1] = $null
    $original = $instructionReader[0].PSObject.BaseObject.Invoke(
        $null, $readerArguments)

    $patchType = $modAssembly.GetType($patchTypeName, $true)
    $transpiler = $patchType.GetMethod(
        'Transpiler', [Reflection.BindingFlags]'Public,Static')
    $helper = $patchType.GetMethod(
        $helperName, [Reflection.BindingFlags]'Public,Static')
    if ($null -eq $transpiler -or $null -eq $helper) {
        throw "Packaged ship-power patch '$patchTypeName' is incomplete."
    }

    try {
        $transpilerArguments = [object[]]::new(1)
        $transpilerArguments[0] = $original
        $patched = @($transpiler.Invoke($null, $transpilerArguments))
    }
    catch {
        if ($_.Exception.InnerException) {
            throw $_.Exception.InnerException
        }
        throw
    }

    $helperCalls = @($patched | Where-Object {
        $_.opcode.Name -eq 'call' -and $_.operand -eq $helper
    })
    if ($patched.Count -ne @($original).Count -or $helperCalls.Count -ne 1) {
        throw "$targetName must replace exactly one ApplyHeat call without changing instruction count."
    }
}

$moduleListItemType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.UI.Canvas_Prefabs.FleetsScreen.ShipModuleListItem',
    $true)
$generateEntries = $moduleListItemType.GetMethod(
    'GenerateEntries', [Reflection.BindingFlags]'NonPublic,Instance')
if ($null -eq $generateEntries) {
    throw 'ShipModuleListItem.GenerateEntries was not found.'
}
$readerArguments = [object[]]::new(2)
$readerArguments[0] = $generateEntries
$readerArguments[1] = $null
$original = $instructionReader[0].PSObject.BaseObject.Invoke(
    $null, $readerArguments)
$uiPatchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.ShipModuleEnergyColumnCompatibilityPatch', $true)
$uiTranspiler = $uiPatchType.GetMethod(
    'Transpiler', [Reflection.BindingFlags]'Public,Static')
$uiHelper = $uiPatchType.GetMethod(
    'EnergyUsageForTableVisibility',
    [Reflection.BindingFlags]'Public,Static')
$driveDisplayHelpers = @(
    'GetHullScaledDriveThrust',
    'GetHullScaledDrivePower',
    'GetHullScaledDriveCost'
) | ForEach-Object {
    $helperName = $_
    @($uiPatchType.GetMethods([Reflection.BindingFlags]'Public,Static') |
        Where-Object {
            $_.Name -eq $helperName -and
            $_.GetParameters().Count -eq 2 -and
            $_.GetParameters()[1].ParameterType.Name -eq 'ShipModuleListItem'
        })[0]
}
try {
    $transpilerArguments = [object[]]::new(1)
    $transpilerArguments[0] = $original
    $patched = @($uiTranspiler.Invoke($null, $transpilerArguments))
}
catch {
    $messages = [Collections.Generic.List[string]]::new()
    $patchException = $_.Exception
    while ($null -ne $patchException) {
        $messages.Add($patchException.ToString())
        $patchException = $patchException.InnerException
    }
    throw ($messages -join "`nCaused by:`n")
}
$uiHelperCalls = @($patched | Where-Object {
    $_.opcode.Name -eq 'call' -and $_.operand -eq $uiHelper
})
$driveDisplayCalls = @($patched | Where-Object {
    $_.opcode.Name -eq 'call' -and $driveDisplayHelpers -contains $_.operand
})
if ($patched.Count -ne (@($original).Count + 3) -or
    $uiHelperCalls.Count -ne 1 -or
    $driveDisplayCalls.Count -ne 3) {
    throw 'GenerateEntries must replace one EnergyUsage_GJ visibility call and add exactly three hull-scaled drive display calls.'
}

$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyId = 'ti-eeo.ship-power-validation.' +
    [Guid]::NewGuid().ToString('N')
$harmony = [Activator]::CreateInstance(
    $harmonyType, [object[]]@($harmonyId))
$patchTypeNames = @(
    'TIEconomyMod.Patches.GunPowerTemplateInitializationPatch',
    'TIEconomyMod.Patches.ShipPowerSaveLoadCachePatch',
    'TIEconomyMod.Patches.GunSelfPoweredPatch',
    'TIEconomyMod.Patches.GunEnergyUsagePatch',
    'TIEconomyMod.Patches.GunHeatGenerationPatch',
    'TIEconomyMod.Patches.ShipModuleEnergyColumnCompatibilityPatch',
    'TIEconomyMod.Patches.HullScaledDriveDescriptionPatch',
    'TIEconomyMod.Patches.HullScaledDriveTooltipPatch',
    'TIEconomyMod.Patches.HullScaledDriveTableRefreshPatch',
    'TIEconomyMod.Patches.PoweredWeaponRadiatorHeatPatch',
    'TIEconomyMod.Patches.WeaponHeatCapacityPrecheckPatch',
    'TIEconomyMod.Patches.AuxiliaryElectricalGenerationPatch',
    'TIEconomyMod.Patches.GeneratedPowerHeatPatch',
    'TIEconomyMod.Patches.DuplicateSystemsHeatPatch'
)
try {
    foreach ($patchTypeName in $patchTypeNames) {
        $patchType = $modAssembly.GetType($patchTypeName, $true)
        $processor = $harmony.CreateClassProcessor($patchType)
        [void]$processor.Patch()
    }
}
catch {
    $messages = [Collections.Generic.List[string]]::new()
    $patchException = $_.Exception
    while ($null -ne $patchException) {
        $messages.Add($patchException.ToString())
        $patchException = $patchException.InnerException
    }
    throw ($messages -join "`nCaused by:`n")
}
finally {
    $harmony.UnpatchAll($harmonyId)
}

Write-Host 'PASS: ship-power transpilers replace the validated heat and module-table calls and all 0.8.2 ship-power patch classes apply.'
