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
        throw "Required hab-upgrade validation assembly is missing: $Path"
    }
    return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
}

function Test-MethodOperand {
    param(
        [object]$Instruction,
        [string]$DeclaringTypeName,
        [string]$MethodName
    )

    if ($null -eq $Instruction.operand) {
        return $false
    }
    $operand = $Instruction.operand.PSObject.BaseObject
    return $operand -is [Reflection.MethodBase] -and
        $operand.DeclaringType.FullName -eq $DeclaringTypeName -and
        $operand.Name -eq $MethodName
}

function Get-OriginalInstructions {
    param(
        [Reflection.MethodBase]$Target,
        [Reflection.MethodInfo]$InstructionReader
    )

    $arguments = [object[]]::new(2)
    $arguments[0] = $Target
    $arguments[1] = $null
    return @($InstructionReader.PSObject.BaseObject.Invoke($null, $arguments))
}

function Invoke-Transpiler {
    param(
        [Reflection.Assembly]$ModAssembly,
        [string]$PatchTypeName,
        [object[]]$Instructions
    )

    $patchType = $ModAssembly.GetType($PatchTypeName, $true)
    $transpiler = $patchType.GetMethod(
        'Transpiler',
        [Reflection.BindingFlags]'NonPublic,Static')
    if ($null -eq $transpiler) {
        throw "Packaged transpiler '$PatchTypeName' was not found."
    }

    $codeInstructionType = $transpiler.GetParameters()[0].ParameterType.GetGenericArguments()[0]
    $instructionListType = [System.Collections.Generic.List``1].MakeGenericType(
        $codeInstructionType)
    $instructionList = [Activator]::CreateInstance($instructionListType)
    foreach ($instruction in $Instructions) {
        [void]$instructionList.Add($instruction.PSObject.BaseObject)
    }
    $arguments = [object[]]::new(1)
    $arguments[0] = $instructionList
    try {
        return @($transpiler.Invoke($null, $arguments))
    }
    catch {
        if ($_.Exception.InnerException) {
            throw $_.Exception.InnerException
        }
        throw
    }
}

$harmonyAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($unityAssembly in Get-ChildItem `
    -LiteralPath $TargetManagedDir `
    -File `
    -Filter 'Unity*.dll') {
    [void](Load-AssemblyBytes $unityAssembly.FullName)
}
foreach ($optionalAssembly in @(
    'FMODUnity.dll',
    'Newtonsoft.Json.dll')) {
    $optionalPath = Join-Path $TargetManagedDir $optionalAssembly
    if (Test-Path -LiteralPath $optionalPath) {
        [void](Load-AssemblyBytes $optionalPath)
    }
}
[void](Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$gameAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
$modAssembly = Load-AssemblyBytes $ModAssemblyPath

$controllerType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.HabitatsScreenController',
    $true)
$moduleStateType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TIHabModuleState',
    $true)
$listType = [System.Collections.Generic.List``1].MakeGenericType(
    $moduleStateType)
$bindingFlags = [Reflection.BindingFlags]'Public,NonPublic,Instance'
$targets = [ordered]@{
    Preview = $controllerType.GetMethod(
        'UpdateModulePreviewText',
        $bindingFlags,
        $null,
        [Type[]]@([bool], [bool]),
        $null)
    Placement = $controllerType.GetMethod(
        'GetSpaceCost',
        $bindingFlags,
        $null,
        [Type[]]@([bool], $moduleStateType),
        $null)
    Bulk = $controllerType.GetMethod(
        'UpgradeAllModulesSelected',
        $bindingFlags,
        $null,
        [Type[]]@($listType),
        $null)
}
foreach ($entry in $targets.GetEnumerator()) {
    if ($null -eq $entry.Value) {
        throw "Guarded hab-upgrade target '$($entry.Key)' was not found."
    }
}

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
    throw "Expected one usable Harmony instruction reader, found $(
        $instructionReaders.Count)."
}
$instructionReader = $instructionReaders[0]
$moduleTemplateTypeName = 'TIHabModuleTemplate'
$resolverTypeName = 'TIEconomyMod.Patches.HabUpgradeCostResolver'

$previewOriginal = Get-OriginalInstructions `
    $targets.Preview `
    $instructionReader
$placementOriginal = Get-OriginalInstructions `
    $targets.Placement `
    $instructionReader
$bulkOriginal = Get-OriginalInstructions `
    $targets.Bulk `
    $instructionReader

$previewCostCalls = @($previewOriginal | Where-Object {
        Test-MethodOperand $_ $moduleTemplateTypeName 'CostFromSpace'
    }).Count
$placementCostCalls = @($placementOriginal | Where-Object {
        Test-MethodOperand $_ $moduleTemplateTypeName 'CostFromSpace'
    }).Count
$bulkCostCalls = @($bulkOriginal | Where-Object {
        Test-MethodOperand $_ $moduleTemplateTypeName 'CostFromSpace'
    }).Count
if ($previewCostCalls -ne 4 -or
    $placementCostCalls -ne 2 -or
    $bulkCostCalls -ne 2) {
    throw "TI hab-upgrade cost call shape changed: preview=$previewCostCalls, placement=$placementCostCalls, bulk=$bulkCostCalls."
}

$previewPatched = Invoke-Transpiler `
    $modAssembly `
    'TIEconomyMod.Patches.HabUpgradePreviewCostPatch' `
    $previewOriginal
$bulkPatched = Invoke-Transpiler `
    $modAssembly `
    'TIEconomyMod.Patches.HabUpgradeBulkExecutionCostPatch' `
    $bulkOriginal

foreach ($case in @(
    [pscustomobject]@{
        Name = 'preview'
        Instructions = $previewPatched
        RemainingNative = 2
    },
    [pscustomobject]@{
        Name = 'bulk execution'
        Instructions = $bulkPatched
        RemainingNative = 0
    })) {
    $resolverCalls = @($case.Instructions | Where-Object {
            Test-MethodOperand $_ $resolverTypeName 'SelectSpaceCostCall'
        }).Count
    $nativeCalls = @($case.Instructions | Where-Object {
            Test-MethodOperand $_ $moduleTemplateTypeName 'CostFromSpace'
        }).Count
    if ($resolverCalls -ne 2 -or
        $nativeCalls -ne $case.RemainingNative) {
        throw "Hab-upgrade $($case.Name) rewrite produced $resolverCalls shared calls and $nativeCalls native calls."
    }
}

$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyId = 'ti.eeo.validate.hab-upgrade.' +
    [Guid]::NewGuid().ToString('N')
$harmony = [Activator]::CreateInstance($harmonyType, @($harmonyId))
try {
    foreach ($patchTypeName in @(
        'TIEconomyMod.Patches.HabUpgradePlacementCostPatch')) {
        $patchType = $modAssembly.GetType($patchTypeName, $true)
        try {
            $patchedMethods = @($harmony.CreateClassProcessor($patchType).Patch())
        }
        catch {
            throw "Failed applying '$patchTypeName': $($_.Exception.ToString())"
        }
        if ($patchedMethods.Count -ne 1) {
            throw "'$patchTypeName' patched $($patchedMethods.Count) methods instead of one."
        }
    }
}
finally {
    $harmony.UnpatchAll($harmonyId)
}

Write-Host 'PASS: hab-upgrade preview, individual placement, and bulk execution share one guarded Boost-substitution resolver (2 + 1 + 2 quote sites; exact transpiler output and placement Harmony emission verified).'
