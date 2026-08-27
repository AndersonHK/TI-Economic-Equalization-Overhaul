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

$capFeatureType = $modAssembly.GetType(
    'TIEconomyMod.Patches.OpenCycleReactorDemandFeature', $true)
$capRatedDriveHelper = $capFeatureType.GetMethod(
    'RequiredCapRatedDriveOutput_GW',
    [Reflection.BindingFlags]'Public,Static')
$ratedFitType = $modAssembly.GetType(
    'TIEconomyMod.Patches.DrivePowerPlantCompatibilityFeature', $true)
$ratedFitHelper = $ratedFitType.GetMethod(
    'FitsRatedOutput', [Reflection.BindingFlags]'Public,Static')
$compatibilityConsumers = @(
    $modAssembly.GetType(
        'TIEconomyMod.Patches.OpenCycleDrivePowerPlantCompatibilityPatch',
        $true).GetMethod('Prefix',
            [Reflection.BindingFlags]'Public,Static'),
    $modAssembly.GetType(
        'TIEconomyMod.Patches.OpenCycleValidDrivesForPowerPlantsPatch',
        $true).GetMethod('Prefix',
            [Reflection.BindingFlags]'Public,Static'),
    $modAssembly.GetType(
        'TIEconomyMod.Patches.HullScaledDriveCompatibilityPatch',
        $true).GetMethod('Prefix',
            [Reflection.BindingFlags]'Public,Static'),
    $modAssembly.GetType(
        'TIEconomyMod.Patches.HullScaledPowerPlantCompatibilityPatch',
        $true).GetMethod('Prefix',
            [Reflection.BindingFlags]'Public,Static')
)
if ($null -eq $capRatedDriveHelper -or $null -eq $ratedFitHelper) {
    throw 'Open-cycle cap-rated drive compatibility helper is missing.'
}
foreach ($compatibilityConsumer in $compatibilityConsumers) {
    if ($null -eq $compatibilityConsumer) {
        throw 'Open-cycle cap-rated drive compatibility consumer is missing.'
    }
    $readerArguments[0] = $compatibilityConsumer
    $readerArguments[1] = $null
    $compatibilityInstructions = @(
        $instructionReader[0].PSObject.BaseObject.Invoke(
            $null, $readerArguments))
    $ratedFitCalls = @($compatibilityInstructions | Where-Object {
        $_.opcode.Name -eq 'call' -and
        $_.operand -eq $ratedFitHelper
    })
    if ($ratedFitCalls.Count -ne 1) {
        throw "$($compatibilityConsumer.DeclaringType.Name).$($compatibilityConsumer.Name) must call rated-output compatibility exactly once."
    }
}

$directCapConsumers = @(
    $ratedFitHelper,
    $appearancePatchType.GetMethod(
        'DriveFitsEffectiveOutput',
        [Reflection.BindingFlags]'NonPublic,Static')
)
foreach ($directCapConsumer in $directCapConsumers) {
    if ($null -eq $directCapConsumer) {
        throw 'Open-cycle direct cap-rated demand consumer is missing.'
    }
    $readerArguments[0] = $directCapConsumer
    $readerArguments[1] = $null
    $directCapInstructions = @(
        $instructionReader[0].PSObject.BaseObject.Invoke(
            $null, $readerArguments))
    $capHelperCalls = @($directCapInstructions | Where-Object {
        $_.opcode.Name -eq 'call' -and
        $_.operand -eq $capRatedDriveHelper
    })
    if ($capHelperCalls.Count -ne 1) {
        throw "$($directCapConsumer.DeclaringType.Name).$($directCapConsumer.Name) must call cap-rated drive demand exactly once."
    }
}

$descriptionHelperType = $modAssembly.GetType(
    'TIEconomyMod.Patches.ShipModuleEnergyColumnCompatibilityPatch', $true)
$separatedPlantDescription = $descriptionHelperType.GetMethod(
    'GetSeparatedPowerPlantDescription',
    [Reflection.BindingFlags]'Public,Static')
$localizedMassMethods = @($gameAssembly.GetType(
    'TIPowerPlantTemplate', $true).GetMethods(
        [Reflection.BindingFlags]'Public,Instance') | Where-Object {
            $_.Name -eq 'GetLocalizedMass' -and
            $_.GetParameters().Count -eq 1 -and
            $_.GetParameters()[0].ParameterType.Name -eq
                'TISpaceShipTemplate'
        })
if ($null -eq $separatedPlantDescription -or
    $localizedMassMethods.Count -ne 1) {
    throw 'Separated power-plant description or localized mass source is missing.'
}
if ($separatedPlantDescription.GetParameters().Count -ne 4 -or
    $separatedPlantDescription.GetParameters()[3].ParameterType -ne
        [bool]) {
    throw 'Separated power-plant descriptions must receive prospective state explicitly.'
}
$localizedMassMethod = $localizedMassMethods[0]
$readerArguments[0] = $separatedPlantDescription
$readerArguments[1] = $null
$plantDescriptionInstructions = @(
    $instructionReader[0].PSObject.BaseObject.Invoke(
        $null, $readerArguments))
$expectedPlantDescriptionKeys = @(
    'UI.Fleets.ElectricalOutputHeader',
    'UI.Fleets.ElectricalOutput',
    'UI.Fleets.ThermalOutput',
    'UI.Fleets.WasteHeat',
    'UI.Fleets.SpecificPowerWithOpenCycleMultiplier'
)
foreach ($expectedPlantDescriptionKey in $expectedPlantDescriptionKeys) {
    $keyLoads = @($plantDescriptionInstructions | Where-Object {
        $_.opcode.Name -eq 'ldstr' -and
        $_.operand -eq $expectedPlantDescriptionKey
    })
    if ($keyLoads.Count -ne 1) {
        throw "Separated power-plant descriptions must load '$expectedPlantDescriptionKey' exactly once."
    }
}
$obsoletePlantDescriptionKeys = @(
    'UI.Fleets.ReactorThermalOutput',
    'UI.Fleets.ElectricalGeneration',
    'UI.Fleets.OpenCycleDriveOutput',
    'UI.Fleets.OpenCycleCapUsage',
    'UI.Fleets.OpenCycleMassMultiplier',
    'UI.Fleets.WasteHeatToRadiators',
    'UI.Fleets.OpenCycleWasteHeat',
    'UI.Fleets.ElectricalWasteHeat',
    'UI.Fleets.ModuleWasteHeat',
    'UI.Fleets.MaximumRatedOutput'
)
foreach ($obsoletePlantDescriptionKey in $obsoletePlantDescriptionKeys) {
    $keyLoads = @($plantDescriptionInstructions | Where-Object {
        $_.opcode.Name -eq 'ldstr' -and
        $_.operand -eq $obsoletePlantDescriptionKey
    })
    if ($keyLoads.Count -ne 0) {
        throw "Compact power-plant descriptions must not load '$obsoletePlantDescriptionKey'."
    }
}
$snapshotType = $modAssembly.GetType(
    'TIEconomyMod.ShipPowerDemandSnapshot', $true)
$driveDemandField = $snapshotType.GetField('DriveDemand_GW')
$grossOpenCycleField = $snapshotType.GetField(
    'OpenCycleReactorOutput_GWth')
$electricalDemandField = $snapshotType.GetField(
    'UsefulElectricalDemand_GWe')
$multiplierField = $snapshotType.GetField(
    'OpenCycleThermalMassMultiplier')
$driveDemandLoads = @($plantDescriptionInstructions | Where-Object {
    $_.opcode.Name -in @('ldfld', 'ldflda') -and
    $_.operand -eq $driveDemandField
})
$grossOpenCycleLoads = @($plantDescriptionInstructions | Where-Object {
    $_.opcode.Name -in @('ldfld', 'ldflda') -and
    $_.operand -eq $grossOpenCycleField
})
$electricalDemandLoads = @($plantDescriptionInstructions | Where-Object {
    $_.opcode.Name -in @('ldfld', 'ldflda') -and
    $_.operand -eq $electricalDemandField
})
$multiplierLoads = @($plantDescriptionInstructions | Where-Object {
    $_.opcode.Name -in @('ldfld', 'ldflda') -and
    $_.operand -eq $multiplierField
})
if ($driveDemandLoads.Count -lt 1 -or
    $grossOpenCycleLoads.Count -ne 0 -or
    $electricalDemandLoads.Count -ne 1 -or
    $multiplierLoads.Count -ne 1) {
    throw ('Compact power rows must show useful drive/electrical output ' +
        'and the active multiplier without exposing gross open-cycle ' +
        'reactor input. Loads: drive=' + $driveDemandLoads.Count +
        ', gross=' + $grossOpenCycleLoads.Count + ', electrical=' +
        $electricalDemandLoads.Count + ', multiplier=' +
        $multiplierLoads.Count + '.')
}
$localizedMassCalls = @($plantDescriptionInstructions | Where-Object {
    $_.opcode.Name -eq 'callvirt' -and
    $_.operand -eq $localizedMassMethod
})
$labeledMassKeys = @($plantDescriptionInstructions | Where-Object {
    $_.opcode.Name -eq 'ldstr' -and
    $_.operand -eq 'UI.Fleets.PowerPlantMass'
})
if ($localizedMassCalls.Count -ne 0 -or $labeledMassKeys.Count -ne 0) {
    throw 'Separated power-plant descriptions must preserve the native mass row unchanged.'
}

$reactorBayDescription = $descriptionHelperType.GetMethod(
    'GetReactorBayDescription', [Reflection.BindingFlags]'Public,Static')
if ($null -eq $reactorBayDescription) {
    throw 'Reactor-bay description helper is missing.'
}
if ($reactorBayDescription.GetParameters().Count -ne 4 -or
    $reactorBayDescription.GetParameters()[3].ParameterType -ne [bool]) {
    throw 'Reactor-bay descriptions must receive prospective state explicitly.'
}
$readerArguments[0] = $reactorBayDescription
$readerArguments[1] = $null
$reactorBayDescriptionInstructions = @(
    $instructionReader[0].PSObject.BaseObject.Invoke(
        $null, $readerArguments))
$reactorBayVolumeKeys = @($reactorBayDescriptionInstructions |
    Where-Object {
        $_.opcode.Name -eq 'ldstr' -and
        $_.operand -eq 'UI.Fleets.ReactorBayCapacityVolume'
    })
$reactorBayHeaderKeys = @($reactorBayDescriptionInstructions |
    Where-Object {
        $_.opcode.Name -eq 'ldstr' -and
        $_.operand -eq 'UI.Fleets.ReactorBayCapacityHeader'
    })
if ($reactorBayVolumeKeys.Count -ne 1 -or
    $reactorBayHeaderKeys.Count -ne 1) {
    throw 'Reactor-bay description must append exactly one external header/value block.'
}

$powerPlantType = $gameAssembly.GetType('TIPowerPlantTemplate', $true)
$nativePlantDescriptions = @($powerPlantType.GetMethods(
    [Reflection.BindingFlags]'Public,Instance,DeclaredOnly') |
    Where-Object { $_.Name -eq 'GetDescriptionData' })
$nativeCrewMethod = $gameAssembly.GetType(
    'TIShipPartTemplate', $true).GetMethod(
        'GetLocalizedCrew', [Reflection.BindingFlags]'Public,Instance')
if ($nativePlantDescriptions.Count -ne 1 -or $null -eq $nativeCrewMethod) {
    throw 'Native power-plant description or crew row source is missing.'
}
$nativePlantDescriptionMethod =
    $nativePlantDescriptions[0].PSObject.BaseObject
$readerArguments[0] = $nativePlantDescriptionMethod
$readerArguments[1] = $null
$nativePlantDescriptionInstructions = @(
    $instructionReader[0].PSObject.BaseObject.Invoke(
        $null, $readerArguments))
$nativeMassCalls = @($nativePlantDescriptionInstructions | Where-Object {
    $_.opcode.Name -eq 'call' -and
    $_.operand -eq $localizedMassMethod
})
$nativeCrewCalls = @($nativePlantDescriptionInstructions | Where-Object {
    $_.opcode.Name -eq 'call' -and
    $_.operand -eq $nativeCrewMethod
})
if ($nativeMassCalls.Count -ne 1 -or $nativeCrewCalls.Count -ne 1) {
    throw 'Native power-plant descriptions must still supply exactly one mass and one crew row.'
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
$fuelRefreshPrefixParameters = @($fuelRefreshPrefix.GetParameters())
if ($fuelRefreshPrefixParameters.Count -ne 2 -or
    $fuelRefreshPrefixParameters[1].ParameterType -ne [bool]) {
    throw 'Fuel-capacity designer refresh must receive the existing-template loading guard.'
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

$existingFuelLoadPatchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.ExistingShipFuelCapacityLoadPatch', $true)
$existingFuelLoadPrefix = $existingFuelLoadPatchType.GetMethod(
    'Prefix', [Reflection.BindingFlags]'Public,Static')
$existingFuelLoadPostfix = $existingFuelLoadPatchType.GetMethod(
    'Postfix', [Reflection.BindingFlags]'Public,Static')
if ($null -eq $existingFuelLoadPrefix -or
    $null -eq $existingFuelLoadPostfix -or
    -not $existingFuelLoadPrefix.GetParameters()[1].ParameterType.IsByRef) {
    throw 'Existing-design fuel loading must preserve the serialized tank count across UI assembly.'
}
$readerArguments[0] = $existingFuelLoadPostfix
$readerArguments[1] = $null
$existingFuelLoadInstructions = @(
    $instructionReader[0].PSObject.BaseObject.Invoke(
        $null, $readerArguments))
$existingFuelLoadCalls = @($existingFuelLoadInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo]
} | ForEach-Object { $_.operand.Name })
foreach ($requiredCall in @(
    'SetTankCountWithinCapacity',
    'CacheTemplateValues',
    'RefreshSpinner',
    'UpdateShipDesignDataPanelAndImage')) {
    if (@($existingFuelLoadCalls | Where-Object {
        $_ -eq $requiredCall
    }).Count -ne 1) {
        throw "Existing-design fuel loading must call $requiredCall exactly once after the final hull appearance is selected."
    }
}

$shipPowerRuntimeType = $modAssembly.GetType(
    'TIEconomyMod.Patches.ShipPowerRuntime', $true)
$refreshPerformanceCache = $shipPowerRuntimeType.GetMethod(
    'RefreshTemplatePerformanceCache',
    [Reflection.BindingFlags]'Public,Static')
$refreshPerformanceCaches = $shipPowerRuntimeType.GetMethod(
    'RefreshTemplatePerformanceCaches',
    [Reflection.BindingFlags]'Public,Static')
if ($null -eq $refreshPerformanceCache -or
    $null -eq $refreshPerformanceCaches) {
    throw 'Canonical ship-template performance cache refresh methods are missing.'
}
$readerArguments[0] = $refreshPerformanceCache
$readerArguments[1] = $null
$refreshPerformanceInstructions = @(
    $instructionReader[0].PSObject.BaseObject.Invoke(
        $null, $readerArguments))
$aggregateCacheCalls = @($refreshPerformanceInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo] -and
    $_.operand.Name -eq 'CacheTemplateValues'
})
if ($aggregateCacheCalls.Count -ne 1) {
    throw 'Ship performance reconciliation must use the game aggregate cache refresh exactly once.'
}
$factionCachePatchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.ShipDesignPerformanceSaveLoadCachePatch', $true)
$factionCachePostfix = $factionCachePatchType.GetMethod(
    'Postfix', [Reflection.BindingFlags]'Public,Static')
if ($null -eq $factionCachePostfix) {
    throw 'Loaded faction ship designs must refresh their performance caches.'
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
$selectedModuleRefreshCalls = @($modulePanelInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo] -and
    $_.operand.Name -eq 'SetSelectedShipPartFromMenu'
})
$destinationGetterCalls = @($modulePanelInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo] -and
    $_.operand.Name -eq 'get_selectedDragDestination'
})
$destinationPartReads = @($modulePanelInstructions | Where-Object {
    $_.operand -is [Reflection.FieldInfo] -and
    $_.operand.Name -eq 'currentPart'
})
if ($modulePanelCalls.Count -ne 1 -or
    $selectedModuleRefreshCalls.Count -ne 1 -or
    $destinationGetterCalls.Count -ne 1 -or
    $destinationPartReads.Count -ne 1) {
    throw 'Appearance panel refresh must reconstruct the selected module destination and perform one destination-backed installed-panel refresh.'
}
$selectedRefreshIndex = [Array]::IndexOf(
    $modulePanelInstructions, $selectedModuleRefreshCalls[0])
$destinationGetterIndex = [Array]::IndexOf(
    $modulePanelInstructions, $destinationGetterCalls[0])
$destinationPartIndex = [Array]::IndexOf(
    $modulePanelInstructions, $destinationPartReads[0])
$modulePanelCallIndex = [Array]::IndexOf(
    $modulePanelInstructions, $modulePanelCalls[0])
$destinationNullBranches = @()
for ($i = $destinationGetterIndex + 1; $i -lt $destinationPartIndex; $i++) {
    if ($modulePanelInstructions[$i].opcode.Name -like 'brtrue*' -or
        $modulePanelInstructions[$i].opcode.Name -like 'brfalse*') {
        $destinationNullBranches += $modulePanelInstructions[$i]
    }
}
if ($selectedRefreshIndex -ge $destinationGetterIndex -or
    $destinationGetterIndex -ge $destinationPartIndex -or
    $destinationPartIndex -ge $modulePanelCallIndex -or
    $destinationNullBranches.Count -lt 1) {
    throw 'Appearance panel refresh must rebuild selection before reading its destination and null-gate that destination before the installed-panel call.'
}
$staleInstalledModuleField = $appearancePatchType.GetField(
    'currentlyInstalledModule', [Reflection.BindingFlags]'NonPublic,Static')
if ($null -ne $staleInstalledModuleField) {
    throw 'Appearance panel refresh must not reuse the stale currentlyInstalledModule field.'
}

$installedDriveHeatType = $modAssembly.GetType(
    'TIEconomyMod.Patches.InstalledDriveHeatPatch', $true)
$installedDriveHeatPrefix = $installedDriveHeatType.GetMethod(
    'Prefix', [Reflection.BindingFlags]'Public,Static')
$readerArguments[0] = $installedDriveHeatPrefix
$readerArguments[1] = $null
$installedDriveHeatInstructions = @(
    $instructionReader[0].PSObject.BaseObject.Invoke(
        $null, $readerArguments))
$installedPowerCalls = @($installedDriveHeatInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo] -and
    $_.operand.Name -eq 'get_drivePowerRequirement_GW'
})
$thermalMathCalls = @($installedDriveHeatInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo] -and
    $_.operand.Name -eq 'PlantWasteHeat_GW'
})
$rawDrivePowerCalls = @($installedDriveHeatInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo] -and
    $_.operand.DeclaringType.Name -eq 'TIDriveTemplate' -and
    $_.operand.Name -eq 'get_powerRequirement_GW'
})
if ($installedPowerCalls.Count -ne 1 -or
    $thermalMathCalls.Count -ne 1 -or
    $rawDrivePowerCalls.Count -ne 0) {
    throw 'Installed drive heat must use the ship-level drive requirement and shared thermal math without reading raw drive-template power.'
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
    'TIEconomyMod.Patches.ShipDesignPerformanceSaveLoadCachePatch',
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
    'TIEconomyMod.Patches.OpenCycleDrivePowerPlantCompatibilityPatch',
    'TIEconomyMod.Patches.OpenCycleValidDrivesForPowerPlantsPatch',
    'TIEconomyMod.Patches.HullScaledDriveCompatibilityPatch',
    'TIEconomyMod.Patches.HullScaledPowerPlantCompatibilityPatch',
    'TIEconomyMod.Patches.SeparatedShipPowerProductionPatch',
    'TIEconomyMod.Patches.OpenCyclePowerPlantMassPatch',
    'TIEconomyMod.Patches.OpenCyclePowerPlantCostPatch',
    'TIEconomyMod.Patches.InstalledDriveHeatPatch',
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

Write-Host 'PASS: ship-power transpilers replace the validated heat and module-table calls, all five drive-cap consumers use cap-rated demand, compact installed/prospective reactor rows preserve useful-output semantics and native mass/crew data, reactor-bay compatibility targets apply, and all ship-power patch classes apply.'
