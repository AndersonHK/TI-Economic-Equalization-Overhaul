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
        throw "Required Cohesion validation assembly is missing: $Path"
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
[void](Load-AssemblyBytes (Join-Path $TargetManagedDir 'Assembly-CSharp.dll'))
$modAssembly = Load-AssemblyBytes $ModAssemblyPath

$patchTypeNames = @(
    'TIEconomyMod.Patches.CohesionRestBaseValuePatch',
    'TIEconomyMod.Patches.CohesionRestDetailBaseValuePatch',
    'TIEconomyMod.Patches.CohesionRestInequalityPatch',
    'TIEconomyMod.Patches.CohesionRestPublicElitePatch',
    'TIEconomyMod.Patches.CohesionRestAutocracyPatch',
    'TIEconomyMod.Patches.CohesionRestAnocracyPatch',
    'TIEconomyMod.Patches.CohesionRestDemocracyPatch'
)

$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyId = 'ti.eeo.validate.cohesion-rest.' +
    [Guid]::NewGuid().ToString('N')
$harmony = [Activator]::CreateInstance($harmonyType, @($harmonyId))
try {
    foreach ($patchTypeName in $patchTypeNames) {
        $patchType = $modAssembly.GetType($patchTypeName, $true)
        try {
            $patchedMethods = @($harmony.CreateClassProcessor($patchType).Patch())
        }
        catch {
            throw "Failed applying '$patchTypeName': $($_.Exception.ToString())"
        }
        if ($patchedMethods.Count -ne 1) {
            throw "'$patchTypeName' emitted $($patchedMethods.Count) target methods instead of one."
        }
    }
}
finally {
    $harmony.UnpatchAll($harmonyId)
}

Write-Host 'PASS: all seven Cohesion rest-state gameplay/detail patches bind and Harmony emits them together.'
