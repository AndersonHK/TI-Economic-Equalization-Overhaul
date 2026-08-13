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

function Load-AssemblyBytes {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required utility-footprint validation assembly is missing: $Path"
    }
    return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
}

$harmonyAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($dependency in Get-ChildItem -LiteralPath $TargetManagedDir -File |
    Where-Object {
        $_.Name -like 'Unity*.dll' -or
        $_.Name -like 'Newtonsoft*.dll' -or
        $_.Name -eq 'FMODUnity.dll'
    }) {
    [void](Load-AssemblyBytes $dependency.FullName)
}
[void](Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$gameAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
$modAssembly = Load-AssemblyBytes $ModAssemblyPath
$iconVisualType = $modAssembly.GetType(
    'TIEconomyMod.UtilityFootprintIconVisuals', $true)

$shipType = $gameAssembly.GetType(
    'TISpaceShipTemplate', $true)
$partType = $gameAssembly.GetType(
    'TIShipPartTemplate', $true)
$dragType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.UI.ShipModuleDragDestination', $true)
$controllerType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.FleetsScreenController', $true)
$listItemType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.UI.Canvas_Prefabs.FleetsScreen.ShipModuleListItem',
    $true)
$factionType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TIFactionState', $true)
$vectorType = [UnityEngine.Vector2Int]

$getPart = $shipType.GetMethod(
    'GetPartInHullSlotIndex',
    [Reflection.BindingFlags]'Public,Instance',
    $null,
    [Type[]]@([int], [bool]),
    $null)
if ($null -eq $getPart -or $getPart.ReturnType -ne $partType) {
    throw 'GetPartInHullSlotIndex(int, bool) no longer matches the utility occupancy patch.'
}

$validPart = $shipType.GetMethod(
    'ValidPartForDesign',
    [Reflection.BindingFlags]'Public,Instance',
    $null,
    [Type[]]@($partType),
    $null)
if ($null -eq $validPart -or $validPart.ReturnType -ne [bool]) {
    throw 'ValidPartForDesign(TIShipPartTemplate) no longer matches the catalog availability patch.'
}

$legal = $dragType.GetMethod(
    'LegalModuleForSlot',
    [Reflection.BindingFlags]'Public,Instance')
if ($null -eq $legal -or $legal.ReturnType -ne [bool]) {
    throw 'ShipModuleDragDestination.LegalModuleForSlot is missing or changed.'
}
$legalParameters = $legal.GetParameters()
if ($legalParameters.Count -ne 3 -or
    $legalParameters[0].ParameterType -ne $partType -or
    $legalParameters[1].ParameterType -ne [bool] -or
    -not $legalParameters[2].ParameterType.IsByRef -or
    $legalParameters[2].ParameterType.GetElementType() -ne $vectorType) {
    throw 'LegalModuleForSlot must retain (TIShipPartTemplate, bool, ref Vector2Int).'
}

$setModule = $controllerType.GetMethod(
    'SetModuleInSlot', [Reflection.BindingFlags]'Public,Instance')
$removeModule = $controllerType.GetMethod(
    'RemoveModuleFromSlot', [Reflection.BindingFlags]'Public,Instance')
$slotDictionary = $controllerType.GetField(
    'shipModuleSlotDictionary',
    [Reflection.BindingFlags]'NonPublic,Instance')
$dragPrivateFields = @(
    'slotImage',
    'defaultPosition',
    'iconSize'
)
$missingDragFields = @($dragPrivateFields | Where-Object {
    $null -eq $dragType.GetField(
        $_, [Reflection.BindingFlags]'Public,NonPublic,Instance')
})
$updateListItem = $listItemType.GetMethod(
    'UpdateItem', [Reflection.BindingFlags]'NonPublic,Instance')
$setListItemAlpha = $listItemType.GetMethod(
    'SetAlpha', [Reflection.BindingFlags]'Public,Instance')
$missingListItemFields = @('moduleTemplate', 'moduleIcon') | Where-Object {
    $null -eq $listItemType.GetField(
        $_, [Reflection.BindingFlags]'Public,NonPublic,Instance')
}
$missingDetailIconFields = @(
    'selectedModuleDataIcon',
    'installedModuleDataIcon'
) | Where-Object {
    $null -eq $controllerType.GetField(
        $_, [Reflection.BindingFlags]'Public,NonPublic,Instance')
}
$applyCatalogPreview = $iconVisualType.GetMethod(
    'ApplyCatalogPreview',
    [Reflection.BindingFlags]'Public,Static')
if ($null -eq $setModule -or $null -eq $removeModule -or
    $null -eq $slotDictionary -or $missingDragFields.Count -gt 0 -or
    $null -eq $updateListItem -or
    $null -eq $setListItemAlpha -or
    @($missingListItemFields).Count -gt 0 -or
    @($missingDetailIconFields).Count -gt 0 -or
    $null -eq $applyCatalogPreview) {
    throw 'The fleet designer placement/removal surface no longer matches the utility patches.'
}

$aiTargets = @($factionType.GetMethods(
    [Reflection.BindingFlags]'NonPublic,Instance') | Where-Object {
        $_.Name -eq 'GetBestUtilityModules'
    })
if ($aiTargets.Count -ne 1) {
    throw "Expected one GetBestUtilityModules AI target, found $($aiTargets.Count)."
}

$patchMethods = [ordered]@{
    'TIEconomyMod.Patches.UtilitySecondarySlotOccupancyPatch' = @('Postfix')
    'TIEconomyMod.Patches.MultiSlotUtilityDropLegalityPatch' = @('Prefix')
    'TIEconomyMod.Patches.MultiSlotUtilityDesignAvailabilityPatch' = @('Postfix')
    'TIEconomyMod.Patches.CyclotronProspectivePlacementPatch' = @('Postfix')
    'TIEconomyMod.Patches.MultiSlotUtilityCatalogIconPatch' = @('Postfix')
    'TIEconomyMod.Patches.MultiSlotUtilityCatalogAlphaPatch' = @('Postfix')
    'TIEconomyMod.Patches.MultiSlotUtilityDetailIconPatch' = @('Postfix')
    'TIEconomyMod.Patches.MultiSlotUtilityDesignerPlacementPatch' = @('Postfix')
    'TIEconomyMod.Patches.MultiSlotUtilityDesignerRemovalPatch' = @('Prefix', 'Postfix')
    'TIEconomyMod.Patches.MultiSlotUtilityAiPackingPatch' = @('Postfix')
}
foreach ($entry in $patchMethods.GetEnumerator()) {
    $patchType = $modAssembly.GetType($entry.Key, $true)
    $harmonyAttributes = @($patchType.GetCustomAttributes($false) |
        Where-Object { $_.GetType().FullName -eq 'HarmonyLib.HarmonyPatch' })
    if ($harmonyAttributes.Count -eq 0) {
        throw "Utility patch type '$($entry.Key)' has no HarmonyPatch target."
    }
    foreach ($methodName in $entry.Value) {
        $patchMethod = $patchType.GetMethod(
            $methodName,
            [Reflection.BindingFlags]'Public,Static')
        if ($null -eq $patchMethod) {
            throw "Utility patch type '$($entry.Key)' is missing public static $methodName."
        }
    }
}

$availabilityPatchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.MultiSlotUtilityDesignAvailabilityPatch',
    $true)
$cyclotronPatchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.CyclotronProspectivePlacementPatch',
    $true)
$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyId = 'ti.eeo.validate.utility-availability.' +
    [Guid]::NewGuid().ToString('N')
$harmony = [Activator]::CreateInstance($harmonyType, @($harmonyId))
try {
    [void]$harmony.CreateClassProcessor($availabilityPatchType).Patch()
    [void]$harmony.CreateClassProcessor($cyclotronPatchType).Patch()
}
catch {
    throw "Failed applying utility catalog or Cyclotron validation patches: $($_.Exception.ToString())"
}
finally {
    $harmony.UnpatchAll($harmonyId)
}

$overridePath = Join-Path $RepositoryRoot (
    'TIEconomyMod\ModFiles\TIUtilityModuleTemplate.json')
$parsedOverrides = Get-Content -LiteralPath $overridePath -Raw |
    ConvertFrom-Json
$overrides = @($parsedOverrides | ForEach-Object { $_ })
$expectedHorizontalUtilities = @(
    'MobileSpaceScienceLab',
    'FlagBridge',
    'MarineAssaultUnit',
    'AdvancedMarineAssaultUnit',
    'EliteMarineAssaultUnit',
    'SolarPlatformKit',
    'SolarOutpostKit',
    'FissionPlatformKit',
    'FissionOutpostKit',
    'FusionPlatformKit',
    'FusionOutpostKit',
    'AutomatedSolarOutpostKit',
    'AutomatedFissionOutpostKit',
    'RepairBay',
    'SalvageBay',
    'Spartans',
    'Rangers',
    'Immortals',
    'ComponentArmor',
    'AutomatedSolarPlatformKit',
    'AutomatedFissionPlatformKit',
    'SalamanderTerrorUnitPod',
    'AlienArmyPod',
    'AlienFusionPlatformKit',
    'AlienFusionOutpostKit',
    'AlienRepairBay',
    'AlienSurveillanceOrbital',
    'AlienSurveillanceRing'
)
if ($overrides.Count -ne
        ($expectedHorizontalUtilities.Count + 2)) {
    throw "Expected 30 utility footprint declarations, found $($overrides.Count)."
}
$cyclotron = @($overrides | Where-Object dataName -eq 'Cyclotron')
if ($cyclotron.Count -ne 1 -or
    $cyclotron[0].utilityFootprint -ne 'Single') {
    throw 'Cyclotron must retain an explicit Single footprint.'
}
$isru = @($overrides | Where-Object dataName -eq 'ISRUModule')
if ($isru.Count -ne 1 -or
    $isru[0].utilityFootprint -ne 'Single') {
    throw 'ISRU Module must retain an explicit Single footprint.'
}
foreach ($dataName in $expectedHorizontalUtilities) {
    $entry = @($overrides | Where-Object dataName -eq $dataName)
    if ($entry.Count -ne 1 -or
        $entry[0].utilityFootprint -ne 'TwoHorizontal') {
        throw "Utility '$dataName' must have a TwoHorizontal footprint."
    }
}
if (@($overrides | Where-Object {
    $null -ne $_.iconResource -or $null -ne $_.iconImagePath
}).Count -ne 0) {
    throw 'Footprint overrides must retain their existing game icons.'
}

$heatSinkOverridePath = Join-Path $RepositoryRoot (
    'TIEconomyMod\ModFiles\TIHeatSinkTemplate.json')
$parsedHeatSinkOverrides = Get-Content -LiteralPath $heatSinkOverridePath -Raw |
    ConvertFrom-Json
$heatSinkOverrides = @(
    $parsedHeatSinkOverrides | ForEach-Object { $_ })
$expectedLargeHeatSinks = @(
    'HeavyWaterHeatSink',
    'HeavyPotassiumHeatSink',
    'HeavySodiumHeatSink',
    'HeavyLithiumHeatSink',
    'HeavyMoltenSaltHeatSink',
    'HeavyExoticHeatSink'
)
foreach ($dataName in $expectedLargeHeatSinks) {
    $entry = @($heatSinkOverrides | Where-Object dataName -eq $dataName)
    if ($entry.Count -ne 1 -or
        $entry[0].utilityFootprint -ne 'TwoHorizontal') {
        throw "Large heat sink '$dataName' must have a TwoHorizontal footprint."
    }
}

Write-Host 'PASS: multi-slot part targets, hull-only catalog compatibility, Cyclotron prospective placement, preview graphics, patch metadata, and footprint data validate.'
