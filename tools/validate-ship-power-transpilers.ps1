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
$fleetsControllerType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.FleetsScreenController', $true)
$setAltHull = $fleetsControllerType.GetMethod(
    'SetAltHull', [Reflection.BindingFlags]'Public,Instance')
$onCycleAltHull = $fleetsControllerType.GetMethod(
    'OnCycleAltHull', [Reflection.BindingFlags]'Public,Instance')
if ($null -eq $setAltHull -or $null -eq $onCycleAltHull) {
    throw 'FleetsScreenController appearance mutation methods were not found.'
}
$readerArguments = [object[]]::new(2)
foreach ($appearanceMethod in @($onCycleAltHull, $setAltHull)) {
    $readerArguments[0] = $appearanceMethod
    $readerArguments[1] = $null
    $appearanceInstructions = @(
        $instructionReader[0].PSObject.BaseObject.Invoke(
            $null, $readerArguments))
    $appearanceWrites = @($appearanceInstructions | Where-Object {
        $_.opcode.Name -eq 'stfld' -and
        $_.operand -is [Reflection.FieldInfo] -and
        $_.operand.Name -eq 'hullAppearanceIndex'
    })
    $panelRefreshCalls = @($appearanceInstructions | Where-Object {
        $_.opcode.Name -eq 'call' -and
        $_.operand -is [Reflection.MethodInfo] -and
        $_.operand.Name -eq 'UpdateShipDesignDataPanelAndImage'
    })
    if ($appearanceWrites.Count -ne 1 -or $panelRefreshCalls.Count -ne 1) {
        throw "$($appearanceMethod.Name) must commit one appearance index and perform one designer-panel refresh before reactor-bay reconciliation."
    }
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
$reactorDisplayHelper = @($uiPatchType.GetMethods(
    [Reflection.BindingFlags]'Public,Static') | Where-Object {
        $_.Name -eq 'GetHullEffectivePowerPlantOutput' -and
        $_.GetParameters().Count -eq 2 -and
        $_.GetParameters()[1].ParameterType.Name -eq 'ShipModuleListItem'
    })[0]
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
$reactorDisplayCalls = @($patched | Where-Object {
    $_.opcode.Name -eq 'call' -and $_.operand -eq $reactorDisplayHelper
})
if ($patched.Count -ne (@($original).Count + 4) -or
    $uiHelperCalls.Count -ne 1 -or
    $driveDisplayCalls.Count -ne 3 -or
    $reactorDisplayCalls.Count -ne 1) {
    throw 'GenerateEntries must replace one EnergyUsage_GJ visibility call, add exactly three hull-scaled drive display calls, and replace one power-plant output display.'
}

$appearancePatchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.ReactorBayAppearanceRefreshPatch', $true)
$targetMethods = $appearancePatchType.GetMethod(
    'TargetMethods', [Reflection.BindingFlags]'Public,Static')
$appearanceTargets = @($targetMethods.Invoke($null, $null))
$appearanceTargetNames = @($appearanceTargets |
    ForEach-Object { $_.Name } | Sort-Object) -join ','
if ($appearanceTargetNames -ne 'OnCycleAltHull,SetAltHull') {
    throw 'Reactor-bay appearance reconciliation must target both OnCycleAltHull and SetAltHull.'
}
$reconcileDrive = $appearancePatchType.GetMethod(
    'ReconcileDriveCluster', [Reflection.BindingFlags]'NonPublic,Static')
if ($null -eq $reconcileDrive) {
    throw 'Reactor-bay appearance patch is missing drive reconciliation.'
}
$readerArguments[0] = $reconcileDrive
$readerArguments[1] = $null
$reconcileInstructions = @($instructionReader[0].PSObject.BaseObject.Invoke(
    $null, $readerArguments))
$setModuleCalls = @($reconcileInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo] -and
    $_.operand.Name -eq 'SetModuleInSlot'
})
$removeModuleCalls = @($reconcileInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo] -and
    $_.operand.Name -eq 'RemoveModuleFromSlot'
})
$variationCalls = @($reconcileInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo] -and
    $_.operand.Name -eq 'GetVariation'
})
$directCapacityCalls = @($reconcileInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo] -and
    $_.operand.Name -eq 'DriveFitsEffectiveOutput'
})
if ($setModuleCalls.Count -ne 1 -or $removeModuleCalls.Count -ne 1 -or
    $variationCalls.Count -ne 1 -or $directCapacityCalls.Count -lt 1) {
    throw 'Appearance reconciliation must directly test effective output, inspect installed-count drive variations, and use exactly one normal replacement/removal path.'
}

$fuelRefreshPatchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.FuelCapacityDesignerRefreshPatch', $true)
$fuelRefreshPrefix = $fuelRefreshPatchType.GetMethod(
    'Prefix', [Reflection.BindingFlags]'Public,Static')
$fuelRefreshPostfix = $fuelRefreshPatchType.GetMethod(
    'Postfix', [Reflection.BindingFlags]'Public,Static')
$fuelUiType = $modAssembly.GetType(
    'TIEconomyMod.Patches.FuelCapacityDesignerUi', $true)
$enforceFuel = $fuelUiType.GetMethod(
    'EnforceAndRefreshSpinner', [Reflection.BindingFlags]'Public,Static')
$refreshFuelOverlay = $fuelUiType.GetMethod(
    'RefreshOverlay', [Reflection.BindingFlags]'Public,Static')
if ($null -eq $fuelRefreshPrefix -or $null -eq $fuelRefreshPostfix -or
    $null -eq $enforceFuel -or $null -eq $refreshFuelOverlay) {
    throw 'Fuel-capacity designer refresh patch is incomplete.'
}
foreach ($fuelRefreshMethod in @($fuelRefreshPrefix, $fuelRefreshPostfix)) {
    $readerArguments[0] = $fuelRefreshMethod
    $readerArguments[1] = $null
    $fuelRefreshInstructions = @(
        $instructionReader[0].PSObject.BaseObject.Invoke(
            $null, $readerArguments))
    $expectedHelper = if ($fuelRefreshMethod.Name -eq 'Prefix') {
        $enforceFuel
    }
    else {
        $refreshFuelOverlay
    }
    $helperCalls = @($fuelRefreshInstructions | Where-Object {
        $_.opcode.Name -eq 'call' -and $_.operand -eq $expectedHelper
    })
    if ($helperCalls.Count -ne 1) {
        throw "Fuel-capacity $($fuelRefreshMethod.Name) must call its lifecycle helper exactly once."
    }
}

$appearancePostfix = $appearancePatchType.GetMethod(
    'Postfix', [Reflection.BindingFlags]'Public,Static')
$readerArguments[0] = $appearancePostfix
$readerArguments[1] = $null
$appearancePostfixInstructions = @(
    $instructionReader[0].PSObject.BaseObject.Invoke($null, $readerArguments))
$postfixCalls = @($appearancePostfixInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo]
} | ForEach-Object { $_.operand.Name })
if (@($postfixCalls | Where-Object { $_ -eq 'RefreshDesignerState' }).Count -ne 1 -or
    @($postfixCalls | Where-Object { $_ -eq 'RefreshRows' }).Count -ne 2 -or
    @($postfixCalls | Where-Object { $_ -eq 'RefreshModulePanels' }).Count -ne 1) {
    throw 'Appearance postfix must refresh designer state, both module tables, and contextual module panels.'
}

$refreshDesignerState = $appearancePatchType.GetMethod(
    'RefreshDesignerState', [Reflection.BindingFlags]'NonPublic,Static')
$readerArguments[0] = $refreshDesignerState
$readerArguments[1] = $null
$refreshStateInstructions = @(
    $instructionReader[0].PSObject.BaseObject.Invoke($null, $readerArguments))
$refreshStateCalls = @($refreshStateInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo]
} | ForEach-Object { $_.operand.Name })
foreach ($requiredRefresh in @(
    'CacheTemplateValues',
    'UpdateShipDesignDataPanelAndImage',
    'UpdateTransferInfo')) {
    if ($refreshStateCalls -notcontains $requiredRefresh) {
        throw "Appearance designer refresh is missing $requiredRefresh."
    }
}
$filterField = $appearancePatchType.GetField(
    'filterAvailableShipModules',
    [Reflection.BindingFlags]'NonPublic,Static')
$filterMethod = $filterField.GetValue($null)
if ($null -eq $filterMethod -or
    $filterMethod.Name -ne 'FilterAvailableShipModules') {
    throw 'Appearance designer refresh must resolve FilterAvailableShipModules.'
}

$refreshModulePanels = $appearancePatchType.GetMethod(
    'RefreshModulePanels', [Reflection.BindingFlags]'NonPublic,Static')
$readerArguments[0] = $refreshModulePanels
$readerArguments[1] = $null
$modulePanelInstructions = @(
    $instructionReader[0].PSObject.BaseObject.Invoke($null, $readerArguments))
$modulePanelCalls = @($modulePanelInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo] -and
    $_.operand.Name -eq 'UpdateModuleDataPanel'
})
if ($modulePanelCalls.Count -ne 2) {
    throw 'Appearance refresh must update installed and selected module detail panels.'
}

$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyId = 'ti-eeo.ship-power-validation.' +
    [Guid]::NewGuid().ToString('N')
$harmony = [Activator]::CreateInstance(
    $harmonyType, [object[]]@($harmonyId))
# UpdateShipDesignDataPanelAndImage is structurally validated above; the
# PowerShell/CoreCLR harness cannot detour that Unity method because Harmony's
# generated wrapper trips the host's ECall restriction. Unity Mono patches it.
$patchTypeNames = @(
    'TIEconomyMod.Patches.GunPowerTemplateInitializationPatch',
    'TIEconomyMod.Patches.ShipPowerSaveLoadCachePatch',
    'TIEconomyMod.Patches.GunSelfPoweredPatch',
    'TIEconomyMod.Patches.GunEnergyUsagePatch',
    'TIEconomyMod.Patches.GunHeatGenerationPatch',
    'TIEconomyMod.Patches.ShipModuleEnergyColumnCompatibilityPatch',
    'TIEconomyMod.Patches.HullScaledDriveDescriptionPatch',
    'TIEconomyMod.Patches.ReactorBayPowerPlantDescriptionPatch',
    'TIEconomyMod.Patches.HullScaledDriveTooltipPatch',
    'TIEconomyMod.Patches.HullScaledDriveTableRefreshPatch',
    'TIEconomyMod.Patches.ReactorBayAppearanceRefreshPatch',
    'TIEconomyMod.Patches.PropellantDensityTemplateInitializationPatch',
    'TIEconomyMod.Patches.FuelCapacitySpinnerLabelPatch',
    'TIEconomyMod.Patches.FuelCapacitySaveGuardPatch',
    'TIEconomyMod.Patches.AiShipDesignCapacityBoundaryPatch',
    'TIEconomyMod.Patches.AiShipRefitCapacityBoundaryPatch',
    'TIEconomyMod.Patches.AiShipEarlyAppearanceSelectionPatch',
    'TIEconomyMod.Patches.FuelCapacityIdealTankCountPatch',
    'TIEconomyMod.Patches.AlienShipFuelCapacityPatch',
    'TIEconomyMod.Patches.StoFighterFuelCapacityPatch',
    'TIEconomyMod.Patches.SavedShipCapacityInvariantPatch',
    'TIEconomyMod.Patches.HullScaledDriveCompatibilityPatch',
    'TIEconomyMod.Patches.HullScaledPowerPlantCompatibilityPatch',
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

Write-Host 'PASS: ship-power transpilers replace the validated heat and module-table calls, reactor-bay compatibility targets apply, and all ship-power patch classes apply.'
