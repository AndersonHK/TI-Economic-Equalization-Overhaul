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
        throw "Required alien ship-design test assembly is missing: $Path"
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
$instructionReader = $instructionReaders[0].PSObject.BaseObject

$factionType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TIFactionState',
    $true)
$designMethods = @($factionType.GetMethods(
    [Reflection.BindingFlags]'Public,Instance') | Where-Object {
        $_.Name -eq 'DesignAlienShip' -and $_.GetParameters().Count -eq 8
    })
if ($designMethods.Count -ne 1) {
    throw "Expected one eight-parameter DesignAlienShip method, found $($designMethods.Count)."
}
$readerArguments = [object[]]::new(2)
$readerArguments[0] = $designMethods[0].PSObject.BaseObject
$readerArguments[1] = $null
$instructions = $instructionReader.Invoke($null, $readerArguments)

$patchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.AlienShipArmorAllocationPatch',
    $true)
$transpiler = $patchType.GetMethod(
    'Transpiler',
    [Reflection.BindingFlags]'Public,Static')
$arguments = [object[]]::new(1)
$arguments[0] = $instructions.PSObject.BaseObject
try {
    $patched = @($transpiler.Invoke($null, $arguments))
}
catch {
    if ($_.Exception.InnerException) {
        throw $_.Exception.InnerException
    }
    throw
}

$oldConstants = @($patched | Where-Object {
    $_.opcode.Name -eq 'ldc.r4' -and [single]$_.operand -eq [single]3500
})
$newConstants = @($patched | Where-Object {
    $_.opcode.Name -eq 'ldc.r4' -and [single]$_.operand -eq [single]10000
})
if ($oldConstants.Count -ne 0 -or $newConstants.Count -ne 1) {
    throw "Alien armor transpiler emitted old=$($oldConstants.Count), new=$($newConstants.Count) constants."
}

$fuelFeatureType = $modAssembly.GetType(
    'TIEconomyMod.Patches.HullFuelCapacityFeature',
    $true)
$setTankCount = $fuelFeatureType.GetMethod(
    'SetTankCountWithinCapacity',
    [Reflection.BindingFlags]'Public,Static')
$clampTarget = $fuelFeatureType.GetMethod(
    'ClampDeltaVTargetToCapacity',
    [Reflection.BindingFlags]'Public,Static')
$targetFloor = $fuelFeatureType.GetMethod(
    'DeltaVTargetFloorForCapacity',
    [Reflection.BindingFlags]'Public,Static')
$fuelPatchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.AlienShipFuelCapacityPatch',
    $true)
$fuelTranspiler = $fuelPatchType.GetMethod(
    'Transpiler',
    [Reflection.BindingFlags]'Public,Static')
$fuelArguments = [object[]]::new(1)
$fuelArguments[0] = $instructions.PSObject.BaseObject
try {
    $fuelPatched = @($fuelTranspiler.Invoke($null, $fuelArguments))
}
catch {
    if ($_.Exception.InnerException) {
        throw $_.Exception.InnerException
    }
    throw
}
$alienTankSetters = @($fuelPatched | Where-Object {
    $_.opcode.Name -eq 'call' -and $_.operand -eq $setTankCount
})
$alienTargetClamps = @($fuelPatched | Where-Object {
    $_.opcode.Name -eq 'call' -and $_.operand -eq $clampTarget
})
$alienTargetFloors = @($fuelPatched | Where-Object {
    $_.opcode.Name -eq 'call' -and $_.operand -eq $targetFloor
})
if ($alienTankSetters.Count -ne 3 -or $alienTargetClamps.Count -ne 1 -or
    $alienTargetFloors.Count -ne 1) {
    throw "Alien fuel transpiler emitted setters=$($alienTankSetters.Count), targetClamps=$($alienTargetClamps.Count), targetFloors=$($alienTargetFloors.Count)."
}

$fighterMethods = @($factionType.GetMethods(
    [Reflection.BindingFlags]'Public,Instance') | Where-Object {
        $_.Name -eq 'DesignSTOFighter' -and $_.GetParameters().Count -eq 2
    })
if ($fighterMethods.Count -ne 1) {
    throw "Expected one two-parameter DesignSTOFighter method, found $($fighterMethods.Count)."
}
$readerArguments[0] = $fighterMethods[0].PSObject.BaseObject
$readerArguments[1] = $null
$fighterInstructions = $instructionReader.Invoke($null, $readerArguments)
$fighterPatchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.StoFighterFuelCapacityPatch',
    $true)
$fighterTranspiler = $fighterPatchType.GetMethod(
    'Transpiler',
    [Reflection.BindingFlags]'Public,Static')
$fighterArguments = [object[]]::new(1)
$fighterArguments[0] = $fighterInstructions.PSObject.BaseObject
try {
    $fighterPatched = @($fighterTranspiler.Invoke($null, $fighterArguments))
}
catch {
    if ($_.Exception.InnerException) {
        throw $_.Exception.InnerException
    }
    throw
}
$fighterTankSetters = @($fighterPatched | Where-Object {
    $_.opcode.Name -eq 'call' -and $_.operand -eq $setTankCount
})
if ($fighterTankSetters.Count -ne 1) {
    throw "STO fighter fuel transpiler emitted setters=$($fighterTankSetters.Count)."
}

Write-Host 'PASS: alien armor and alien/STO fuel-capacity transpilers match the installed game IL.'
