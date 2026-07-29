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
        throw "Required councilor-cap test assembly is missing: $Path"
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

$councilorType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TICouncilorState',
    $true)
$targets = @($councilorType.GetMethods(
    [Reflection.BindingFlags]'Public,Instance') | Where-Object {
        $_.Name -eq 'GetAttribute' -and
        $_.GetParameters().Count -eq 7
    })
if ($targets.Count -ne 1) {
    throw "Expected one seven-parameter GetAttribute overload, found $($targets.Count)."
}
$target = $targets[0].PSObject.BaseObject

$vanillaCapProperty = $councilorType.GetProperty(
    'maxCouncilorAttribute',
    [Reflection.BindingFlags]'NonPublic,Instance')
if ($null -eq $vanillaCapProperty) {
    throw 'The private vanilla councilor-cap property was not found.'
}
$vanillaCapGetter = $vanillaCapProperty.GetGetMethod($true)

$patchProcessorType = $harmonyAssembly.GetType(
    'HarmonyLib.PatchProcessor',
    $true)
$instructionReaders = @($patchProcessorType.GetMethods(
    [Reflection.BindingFlags]'Public,Static') | Where-Object {
        $_.Name -eq 'GetOriginalInstructions' -and
        $_.GetParameters().Count -eq 2 -and
        -not $_.GetParameters()[1].ParameterType.IsByRef
    })
if ($instructionReaders.Count -ne 1) {
    throw "Expected one usable Harmony instruction reader, found $($instructionReaders.Count)."
}
$readerArguments = [object[]]::new(2)
$readerArguments[0] = $target
$readerArguments[1] = $null
$instructionReader = $instructionReaders[0].PSObject.BaseObject
$originalInstructionEnumerable = $instructionReader.Invoke($null, $readerArguments)
$originalInstructions = @($originalInstructionEnumerable)

$patchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.CouncilorTotalAttributeCapPatch',
    $true)
$transpiler = $patchType.GetMethod(
    'Transpiler',
    [Reflection.BindingFlags]'Public,Static')
$configuredCapGetter = $patchType.GetMethod(
    'GetConfiguredCap',
    [Reflection.BindingFlags]'Public,Static')
if ($null -eq $transpiler -or $null -eq $configuredCapGetter) {
    throw 'The packaged councilor-cap transpiler or helper was not found.'
}

$vanillaCallIndexes = @()
for ($index = 0; $index -lt $originalInstructions.Count; $index++) {
    $instruction = $originalInstructions[$index]
    if ($instruction.opcode.Name -match '^call' -and
        $instruction.operand -eq $vanillaCapGetter) {
        $vanillaCallIndexes += $index
    }
}
if ($vanillaCallIndexes.Count -ne 1) {
    throw "Expected one vanilla final-cap call, found $($vanillaCallIndexes.Count)."
}

$transpilerArguments = [object[]]::new(1)
$transpilerArguments[0] = $originalInstructionEnumerable
try {
    $transpiledInstructions = @($transpiler.Invoke($null, $transpilerArguments))
}
catch {
    if ($_.Exception.InnerException) {
        throw $_.Exception.InnerException
    }
    throw
}

if ($transpiledInstructions.Count -ne $originalInstructions.Count) {
    throw "Councilor-cap transpiler changed the instruction count from $(
        $originalInstructions.Count) to $($transpiledInstructions.Count)."
}

$configuredCallIndexes = @()
$remainingVanillaCalls = @()
for ($index = 0; $index -lt $transpiledInstructions.Count; $index++) {
    $instruction = $transpiledInstructions[$index]
    if ($instruction.opcode.Name -eq 'call' -and
        $instruction.operand -eq $configuredCapGetter) {
        $configuredCallIndexes += $index
    }
    if ($instruction.opcode.Name -match '^call' -and
        $instruction.operand -eq $vanillaCapGetter) {
        $remainingVanillaCalls += $index
    }
}
if ($configuredCallIndexes.Count -ne 1 -or $remainingVanillaCalls.Count -ne 0) {
    throw "Councilor-cap transpiler emitted $($configuredCallIndexes.Count) configured calls and left $(
        $remainingVanillaCalls.Count) vanilla calls."
}
if ($configuredCallIndexes[0] -ne $vanillaCallIndexes[0] -or
    $configuredCallIndexes[0] -lt 1 -or
    $transpiledInstructions[$configuredCallIndexes[0] - 1].opcode.Name -ne 'ldarg.0') {
    throw 'Councilor-cap helper replaced the wrong instruction or has an unexpected stack shape.'
}

$originalInstruction = $originalInstructions[$vanillaCallIndexes[0]]
$transpiledInstruction = $transpiledInstructions[$configuredCallIndexes[0]]
if ($originalInstruction.labels.Count -ne $transpiledInstruction.labels.Count -or
    $originalInstruction.blocks.Count -ne $transpiledInstruction.blocks.Count) {
    throw 'Councilor-cap transpiler did not preserve instruction labels and exception blocks.'
}

$mainType = $modAssembly.GetType('TIEconomyMod.Main', $true)
$settingsType = $modAssembly.GetType('TIEconomyMod.Settings', $true)
$settings = [Activator]::CreateInstance($settingsType)
$mainType.GetField(
    'settings',
    [Reflection.BindingFlags]'Public,Static').SetValue($null, $settings)
$mainType.GetField(
    'enabled',
    [Reflection.BindingFlags]'Public,Static').SetValue($null, $true)
$helperArguments = [object[]]::new(1)
$helperArguments[0] = $null
$configuredValue = [int]$configuredCapGetter.Invoke($null, $helperArguments)
if ($configuredValue -ne 50) {
    throw "Configured councilor total cap is $configuredValue instead of 50."
}

$runtimeCapsType = $modAssembly.GetType(
    'TIEconomyMod.Patches.CouncilorRuntimeCaps',
    $true)
$configuredOrganizationCapGetter = $runtimeCapsType.GetMethod(
    'GetConfiguredOrganizationCap',
    [Reflection.BindingFlags]'Public,Static')
$configuredOrganizationCap = [int]$configuredOrganizationCapGetter.Invoke(
    $null,
    [object[]]::new(0))
if ($configuredOrganizationCap -ne 18) {
    throw "Configured councilor organization cap is $configuredOrganizationCap instead of 18."
}

$availableAdministrationTarget = $councilorType.GetProperty(
    'availableAdministration',
    [Reflection.BindingFlags]'Public,Instance').GetGetMethod()
$availableReaderArguments = [object[]]::new(2)
$availableReaderArguments[0] = $availableAdministrationTarget
$availableReaderArguments[1] = $null
$availableInstructionEnumerable = $instructionReader.Invoke(
    $null,
    $availableReaderArguments)
$availableOriginalInstructions = @($availableInstructionEnumerable)
$availablePatchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.CouncilorAvailableAdministrationCapPatch',
    $true)
$availableTranspiler = $availablePatchType.GetMethod(
    'Transpiler',
    [Reflection.BindingFlags]'Public,Static')
$availableTranspilerArguments = [object[]]::new(1)
$availableTranspilerArguments[0] = $availableInstructionEnumerable
$availablePatchedInstructions = @($availableTranspiler.Invoke(
    $null,
    $availableTranspilerArguments))
$availableVanillaCalls = @($availableOriginalInstructions | Where-Object {
    $_.opcode.Name -match '^call' -and $_.operand -eq $vanillaCapGetter
})
$availableConfiguredCalls = @($availablePatchedInstructions | Where-Object {
    $_.opcode.Name -eq 'call' -and $_.operand -eq $configuredCapGetter
})
$availableRemainingVanillaCalls = @($availablePatchedInstructions | Where-Object {
    $_.opcode.Name -match '^call' -and $_.operand -eq $vanillaCapGetter
})
if ($availablePatchedInstructions.Count -ne $availableOriginalInstructions.Count -or
    $availableVanillaCalls.Count -ne 1 -or
    $availableConfiguredCalls.Count -ne 1 -or
    $availableRemainingVanillaCalls.Count -ne 0) {
    throw 'Available-Administration transpiler did not replace exactly one 25-point cap.'
}

$orgStateType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TIOrgState',
    $true)
$sufficientTargets = @($councilorType.GetMethods(
    [Reflection.BindingFlags]'Public,Instance') | Where-Object {
        $_.Name -eq 'SufficientCapacityForOrg' -and
        $_.GetParameters().Count -eq 1 -and
        $_.GetParameters()[0].ParameterType -eq $orgStateType
    })
if ($sufficientTargets.Count -ne 1) {
    throw "Expected one SufficientCapacityForOrg target, found $($sufficientTargets.Count)."
}
$sufficientTarget = $sufficientTargets[0].PSObject.BaseObject
$sufficientReaderArguments = [object[]]::new(2)
$sufficientReaderArguments[0] = $sufficientTarget
$sufficientReaderArguments[1] = $null
$sufficientInstructionEnumerable = $instructionReader.Invoke(
    $null,
    $sufficientReaderArguments)
$sufficientOriginalInstructions = @($sufficientInstructionEnumerable)
$weightPatchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.CouncilorOrganizationWeightCapPatch',
    $true)
$weightTranspiler = $weightPatchType.GetMethod(
    'Transpiler',
    [Reflection.BindingFlags]'Public,Static')
$configuredWeightGetter = $weightPatchType.GetMethod(
    'GetConfiguredOrgCapacityMaximum',
    [Reflection.BindingFlags]'Public,Static')
$vanillaWeightGetter = $councilorType.GetMethod(
    'GetClampedMaxStatValue',
    [Reflection.BindingFlags]'Public,Instance')
$weightTranspilerArguments = [object[]]::new(1)
$weightTranspilerArguments[0] = $sufficientInstructionEnumerable
$sufficientPatchedInstructions = @($weightTranspiler.Invoke(
    $null,
    $weightTranspilerArguments))
$sufficientVanillaCalls = @($sufficientOriginalInstructions | Where-Object {
    $_.opcode.Name -match '^call' -and $_.operand -eq $vanillaWeightGetter
})
$sufficientConfiguredCalls = @($sufficientPatchedInstructions | Where-Object {
    $_.opcode.Name -eq 'call' -and $_.operand -eq $configuredWeightGetter
})
$sufficientRemainingVanillaCalls = @($sufficientPatchedInstructions | Where-Object {
    $_.opcode.Name -match '^call' -and $_.operand -eq $vanillaWeightGetter
})
if ($sufficientPatchedInstructions.Count -ne $sufficientOriginalInstructions.Count -or
    $sufficientVanillaCalls.Count -ne 1 -or
    $sufficientConfiguredCalls.Count -ne 1 -or
    $sufficientRemainingVanillaCalls.Count -ne 0) {
    throw 'Organization-weight transpiler did not replace exactly one 25-point cap.'
}

$attributeType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.CouncilorAttribute',
    $true)
$weightHelperArguments = [object[]]::new(2)
$weightHelperArguments[0] = $null
$weightHelperArguments[1] = [Enum]::ToObject($attributeType, 5)
$configuredWeightValue = [int]$configuredWeightGetter.Invoke(
    $null,
    $weightHelperArguments)
if ($configuredWeightValue -ne 50) {
    throw "Configured organization-weight cap is $configuredWeightValue instead of 50."
}

$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyMethodType = $harmonyAssembly.GetType(
    'HarmonyLib.HarmonyMethod',
    $true)
$harmonyId = 'ti-eeo.councilor-cap-validation.' + [Guid]::NewGuid().ToString('N')
$harmony = [Activator]::CreateInstance(
    $harmonyType,
    [object[]]@($harmonyId))
$harmonyTranspiler = [Activator]::CreateInstance(
    $harmonyMethodType,
    [object[]]@($transpiler))
$patchMethods = @($harmonyType.GetMethods(
    [Reflection.BindingFlags]'Public,Instance') | Where-Object {
        $_.Name -eq 'Patch' -and
        $_.GetParameters().Count -eq 5
    })
if ($patchMethods.Count -ne 1) {
    throw "Expected one Harmony.Patch overload, found $($patchMethods.Count)."
}
$patchArguments = [object[]]::new(5)
$patchArguments[0] = $target
$patchArguments[1] = $null
$patchArguments[2] = $null
$patchArguments[3] = $harmonyTranspiler
$patchArguments[4] = $null
try {
    $replacementMethod = $patchMethods[0].PSObject.BaseObject.Invoke(
        $harmony,
        $patchArguments)
}
catch {
    if ($_.Exception.InnerException) {
        throw $_.Exception.InnerException
    }
    throw
}
if ($null -eq $replacementMethod) {
    throw 'Harmony did not emit a replacement councilor GetAttribute method.'
}

$availableHarmonyTranspiler = [Activator]::CreateInstance(
    $harmonyMethodType,
    [object[]]@($availableTranspiler))
$availablePatchArguments = [object[]]::new(5)
$availablePatchArguments[0] = $availableAdministrationTarget
$availablePatchArguments[1] = $null
$availablePatchArguments[2] = $null
$availablePatchArguments[3] = $availableHarmonyTranspiler
$availablePatchArguments[4] = $null
$availableReplacementMethod = $patchMethods[0].PSObject.BaseObject.Invoke(
    $harmony,
    $availablePatchArguments)
if ($null -eq $availableReplacementMethod) {
    throw 'Harmony did not emit a replacement available-Administration method.'
}

$weightHarmonyTranspiler = [Activator]::CreateInstance(
    $harmonyMethodType,
    [object[]]@($weightTranspiler))
$weightPatchArguments = [object[]]::new(5)
$weightPatchArguments[0] = $sufficientTarget
$weightPatchArguments[1] = $null
$weightPatchArguments[2] = $null
$weightPatchArguments[3] = $weightHarmonyTranspiler
$weightPatchArguments[4] = $null
$weightReplacementMethod = $patchMethods[0].PSObject.BaseObject.Invoke(
    $harmony,
    $weightPatchArguments)
if ($null -eq $weightReplacementMethod) {
    throw 'Harmony did not emit a replacement organization-capacity method.'
}

$tooltipPatch = $modAssembly.GetType(
    'TIEconomyMod.Patches.CouncilorAttributeCapTooltipPatch',
    $true)
if ($null -eq $tooltipPatch.GetMethod(
    'Postfix',
    [Reflection.BindingFlags]'Public,Static')) {
    throw 'The councilor attribute-cap tooltip postfix was not found.'
}

Write-Host "PASS: councilor cap validation enforces 18 organizations, replaces all three intended 25-point total/organization clamps with 50, preserves base-stat paths, and Harmony emits every patched method."
