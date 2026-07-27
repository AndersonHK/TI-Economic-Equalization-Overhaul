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
        throw "Required hab-list-icon test assembly is missing: $Path"
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

$harmonyAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($unityAssembly in Get-ChildItem `
    -LiteralPath $TargetManagedDir `
    -File `
    -Filter 'Unity*.dll') {
    [void](Load-AssemblyBytes $unityAssembly.FullName)
}
[void](Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'FMODUnity.dll'))
[void](Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$gameAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
$modAssembly = Load-AssemblyBytes $ModAssemblyPath

$listItemTypeName =
    'PavonisInteractive.TerraInvicta.HabListItem'
$habTypeName =
    'PavonisInteractive.TerraInvicta.TIHabState'
$sectorTypeName =
    'PavonisInteractive.TerraInvicta.TISectorState'
$listItemType = $gameAssembly.GetType($listItemTypeName, $true)
$target = $listItemType.GetMethod(
    'UpdateItem',
    [Reflection.BindingFlags]'Public,Instance')
if ($null -eq $target) {
    throw 'The guarded HabListItem.UpdateItem method was not found.'
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
$readerArguments = [object[]]::new(2)
$readerArguments[0] = $target
$readerArguments[1] = $null
$originalInstructions =
    $instructionReaders[0].PSObject.BaseObject.Invoke(
        $null,
        $readerArguments)

$originalActiveCalls = @($originalInstructions | Where-Object {
        Test-MethodOperand $_ $sectorTypeName 'get_active'
    }).Count
$originalTierCalls = @($originalInstructions | Where-Object {
        Test-MethodOperand $_ $habTypeName 'get_tier'
    }).Count

$patchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.TierOneStationListIconPatch',
    $true)
$transpiler = $patchType.GetMethod(
    'Transpiler',
    [Reflection.BindingFlags]'NonPublic,Static')
if ($null -eq $transpiler) {
    throw 'The packaged hab-list-icon transpiler was not found.'
}
$transpilerArguments = [object[]]::new(1)
$transpilerArguments[0] = $originalInstructions
try {
    $transpiledInstructions = @($transpiler.Invoke(
        $null,
        $transpilerArguments))
}
catch {
    if ($_.Exception.InnerException) {
        throw $_.Exception.InnerException
    }
    throw
}

$transpiledActiveCalls = @($transpiledInstructions | Where-Object {
        Test-MethodOperand $_ $sectorTypeName 'get_active'
    }).Count
$transpiledTierCalls = @($transpiledInstructions | Where-Object {
        Test-MethodOperand $_ $habTypeName 'get_tier'
    }).Count
if ($transpiledInstructions.Count -ne $originalInstructions.Count + 6) {
    throw "Hab-list-icon transpiler changed instruction count by $(
        $transpiledInstructions.Count - $originalInstructions.Count
    ) instead of 6."
}
if ($transpiledActiveCalls -ne $originalActiveCalls) {
    throw "Hab-list-icon active-sector calls changed from $originalActiveCalls to $(
        $transpiledActiveCalls)."
}
if ($transpiledTierCalls -ne $originalTierCalls + 1) {
    throw "Hab-list-icon tier calls changed from $originalTierCalls to $(
        $transpiledTierCalls) instead of increasing by one."
}

$guardMatches = 0
for ($index = 0;
    ($index + 7) -lt $transpiledInstructions.Count;
    $index++) {
    $isActiveGetter = Test-MethodOperand `
        $transpiledInstructions[$index] `
        $sectorTypeName `
        'get_active'
    $isTierGetter = Test-MethodOperand `
        $transpiledInstructions[$index + 3] `
        $habTypeName `
        'get_tier'
    if ($isActiveGetter -and
        $transpiledInstructions[$index + 1].opcode.Name -eq 'ldarg.0' -and
        $transpiledInstructions[$index + 2].opcode.Name -eq 'ldfld' -and
        $isTierGetter -and
        $transpiledInstructions[$index + 4].opcode.Name -eq 'ldc.i4.1' -and
        $transpiledInstructions[$index + 5].opcode.Name -eq 'cgt' -and
        $transpiledInstructions[$index + 6].opcode.Name -eq 'and' -and
        $transpiledInstructions[$index + 7].opcode.Name -match '^brfalse') {
        $guardMatches++
    }
}
if ($guardMatches -ne 1) {
    throw "Expected one tier-gated station-sector icon condition, found $(
        $guardMatches)."
}

Write-Host "PASS: hab-list station-sector overlays are tier-gated ($(
    $originalInstructions.Count) -> $(
    $transpiledInstructions.Count) instructions); one guarded rewrite."
