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

Write-Host 'PASS: DesignAlienShip replaces its single 3500 kg/m3 armor-allocation constant with 10000 kg/m3.'
