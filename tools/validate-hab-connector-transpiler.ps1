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
        throw "Required connector-test assembly is missing: $Path"
    }
    return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
}

function Read-LoadedInt {
    param([object]$Instruction)

    switch ($Instruction.opcode.Name) {
        'ldc.i4.2' { return 2 }
        'ldc.i4.4' { return 4 }
        'ldc.i4' { return [int]$Instruction.operand }
        'ldc.i4.s' { return [int]$Instruction.operand }
        default { return $null }
    }
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

$sectorType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TISectorState',
    $true)
$targets = @($sectorType.GetMethods(
    [Reflection.BindingFlags]'Public,Static') | Where-Object {
        $_.Name -eq 'UpdateModuleConnectorMap' -and
        $_.GetParameters().Count -eq 2
    })
if ($targets.Count -ne 1) {
    throw "Expected one UpdateModuleConnectorMap overload, found $($targets.Count)."
}
$target = $targets[0].PSObject.BaseObject

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
$originalInstructions = $instructionReader.Invoke(
    $null,
    $readerArguments)

$patchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.TierOneStationConnectorMapPatch',
    $true)
$transpiler = $patchType.GetMethod(
    'Transpiler',
    [Reflection.BindingFlags]'NonPublic,Static')
if ($null -eq $transpiler) {
    throw 'The packaged connector transpiler was not found.'
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

$helperType = $modAssembly.GetType(
    'TIEconomyMod.Patches.HabStationSectorRebalance',
    $true)
$helper = $helperType.GetMethod(
    'ConnectorTierRequirement',
    [Reflection.BindingFlags]'NonPublic,Static')
$helperCallIndexes = @()
for ($index = 0; $index -lt $transpiledInstructions.Count; $index++) {
    $instruction = $transpiledInstructions[$index]
    if ($instruction.opcode.Name -eq 'call' -and
        $instruction.operand -eq $helper) {
        $helperCallIndexes += $index
    }
}

if ($transpiledInstructions.Count -ne $originalInstructions.Count + 4) {
    throw "Connector transpiler changed the instruction count by $(
        $transpiledInstructions.Count - $originalInstructions.Count) instead of 4."
}
if ($helperCallIndexes.Count -ne 2) {
    throw "Connector transpiler inserted $($helperCallIndexes.Count) helper calls instead of 2."
}

$patchedSectors = @()
foreach ($callIndex in $helperCallIndexes) {
    if ($callIndex -lt 2 -or
        $callIndex + 1 -ge $transpiledInstructions.Count -or
        $transpiledInstructions[$callIndex - 2].opcode.Name -ne 'ldarg.0' -or
        $transpiledInstructions[$callIndex + 1].opcode.Name -notmatch '^blt') {
        throw "Connector helper call at instruction $callIndex has an unexpected stack shape."
    }
    $patchedSectors += Read-LoadedInt (
        $transpiledInstructions[$callIndex - 1])
}
if (($patchedSectors -join ',') -ne '2,4') {
    throw "Connector transpiler targeted internal sectors '$(
        $patchedSectors -join ',')' instead of '2,4'."
}

$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyMethodType = $harmonyAssembly.GetType(
    'HarmonyLib.HarmonyMethod',
    $true)
$harmonyId = 'ti-eeo.connector-validation.' + [Guid]::NewGuid().ToString('N')
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
    throw 'Harmony did not emit a replacement connector-map method.'
}

Write-Host "PASS: connector transpiler rewrites exactly sectors 2 and 4 ($(
    $originalInstructions.Count) -> $($transpiledInstructions.Count) instructions) and Harmony emits the patched method."
