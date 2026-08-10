[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$TargetManagedDir,
    [Parameter(Mandatory = $true)]
    [string]$ModAssemblyPath
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $ModAssemblyPath)) {
    throw "Mod assembly not found: $ModAssemblyPath"
}

function Load-AssemblyBytes {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required AI technology-selection validation assembly is missing: $Path"
    }
    return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
}

$harmonyAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($unityAssembly in Get-ChildItem `
    -LiteralPath $TargetManagedDir `
    -File `
    -Filter 'Unity*.dll') {
    [void](Load-AssemblyBytes $unityAssembly.FullName)
}
$fmodAssembly = Join-Path $TargetManagedDir 'FMODUnity.dll'
if (Test-Path -LiteralPath $fmodAssembly) {
    [void](Load-AssemblyBytes $fmodAssembly)
}
[void](Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$gameAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
$modAssembly = Load-AssemblyBytes $ModAssemblyPath

$aiType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.AIEvaluators', $true)
$factionType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TIFactionState', $true)
$techType = $gameAssembly.GetType('TITechTemplate', $true)
$bindingFlags = [Reflection.BindingFlags]::Public -bor `
    [Reflection.BindingFlags]::Static
$targets = @($aiType.GetMethods($bindingFlags) | Where-Object {
    if ($_.Name -ne 'SelectTech' -or $_.ReturnType -ne $techType) {
        return $false
    }
    $parameters = $_.GetParameters()
    return $parameters.Count -eq 3 -and
        $parameters[0].ParameterType -eq $factionType -and
        $parameters[1].ParameterType.IsGenericType -and
        $parameters[1].ParameterType.Name -eq 'List`1' -and
        $parameters[1].ParameterType.GetGenericArguments()[0] -eq $techType -and
        $parameters[2].ParameterType -eq [bool]
})
if ($targets.Count -ne 1) {
    throw "Expected one global SelectTech(faction, List<TITechTemplate>, bool) target; found $($targets.Count)."
}

$patchTypeName = 'TIEconomyMod.Patches.GlobalTechnologySoftSelectionPatch'
$patchType = $modAssembly.GetType($patchTypeName, $true)
$prefix = $patchType.GetMethod(
    'Prefix',
    [Reflection.BindingFlags]::Public -bor [Reflection.BindingFlags]::Static)
if ($null -eq $prefix -or $prefix.ReturnType -ne [bool]) {
    throw "$patchTypeName must expose a public static bool Prefix."
}

$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyId = 'ti.eeo.validate.ai-technology-selection.' +
    [Guid]::NewGuid().ToString('N')
$harmony = [Activator]::CreateInstance($harmonyType, @($harmonyId))
try {
    [void]$harmony.CreateClassProcessor($patchType).Patch()
}
catch {
    throw "Failed applying '$patchTypeName': $($_.Exception.ToString())"
}
finally {
    $harmony.UnpatchAll($harmonyId)
}

Write-Host 'PASS: soft global-technology selection target and Harmony prefix validate.'
