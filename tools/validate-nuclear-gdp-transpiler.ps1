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
        throw "Required nuclear-GDP test assembly is missing: $Path"
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

function Read-Instructions {
    param([Reflection.MethodBase]$Method)

    $arguments = [object[]]::new(2)
    $arguments[0] = $Method
    $arguments[1] = $null
    $instructionEnumerable = $instructionReader.Invoke($null, $arguments)
    Write-Output -NoEnumerate $instructionEnumerable
}

function Invoke-Transpiler {
    param(
        [Reflection.MethodInfo]$Transpiler,
        [object]$Instructions
    )

    $arguments = [object[]]::new(1)
    $arguments[0] = $Instructions.PSObject.BaseObject
    try {
        return @($Transpiler.Invoke($null, $arguments))
    }
    catch {
        if ($_.Exception.InnerException) {
            throw $_.Exception.InnerException
        }
        throw
    }
}

$regionType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TIRegionState',
    $true)
$damageMethods = @($regionType.GetMethods(
    [Reflection.BindingFlags]'Public,Instance') | Where-Object {
        $_.Name -eq 'ApplyDamageToRegion' -and
        $_.GetParameters().Count -eq 7
    })
if ($damageMethods.Count -ne 1) {
    throw "Expected one seven-parameter ApplyDamageToRegion method, found $($damageMethods.Count)."
}
$damageInstructions = Read-Instructions $damageMethods[0].PSObject.BaseObject
$gdpPatchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.NuclearGlobalGdpPatch',
    $true)
$gdpTranspiler = $gdpPatchType.GetMethod(
    'Transpiler',
    [Reflection.BindingFlags]'Public,Static')
$configuredGdpChange = $gdpPatchType.GetMethod(
    'ApplyConfiguredGlobalGdpChange',
    [Reflection.BindingFlags]'Public,Static')
$nationType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TINationState',
    $true)
$vanillaGdpChanges = @($nationType.GetMethods(
    [Reflection.BindingFlags]'Public,Instance') | Where-Object {
        $_.Name -eq 'GDPPctChange' -and $_.GetParameters().Count -eq 2
    })
if ($vanillaGdpChanges.Count -ne 1) {
    throw "Expected one two-parameter GDPPctChange method, found $($vanillaGdpChanges.Count)."
}
$vanillaGdpChange = $vanillaGdpChanges[0].PSObject.BaseObject
$patchedDamage = Invoke-Transpiler $gdpTranspiler $damageInstructions
$configuredGdpCalls = @($patchedDamage | Where-Object {
    $_.opcode.Name -eq 'call' -and $_.operand -eq $configuredGdpChange
})
$remainingVanillaGdpCalls = @($patchedDamage | Where-Object {
    $_.opcode.Name -match '^call' -and $_.operand -eq $vanillaGdpChange
})
$directModifyGdpCalls = @($patchedDamage | Where-Object {
    $_.opcode.Name -match '^call' -and
    $_.operand -is [Reflection.MethodInfo] -and
    $_.operand.Name -eq 'ModifyGDP'
})
if ($configuredGdpCalls.Count -ne 3 -or
    $remainingVanillaGdpCalls.Count -ne 0 -or
    $directModifyGdpCalls.Count -lt 1) {
    throw "Nuclear GDP transpiler emitted unexpected calls: configured=$(
        $configuredGdpCalls.Count), vanilla=$($remainingVanillaGdpCalls.Count), direct=$(
        $directModifyGdpCalls.Count)."
}

Write-Host 'PASS: exactly three worldwide nuclear GDP calls are redirected while direct target GDP damage remains.'
