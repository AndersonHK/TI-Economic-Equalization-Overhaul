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
        throw "Required refit appearance-lock validation assembly is missing: $Path"
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

$shipType = $gameAssembly.GetType('TISpaceShipTemplate', $true)
$target = $shipType.GetMethod(
    'IsAValidRefitFor',
    [Reflection.BindingFlags]::Public -bor [Reflection.BindingFlags]::Instance,
    $null,
    @($shipType, [string].MakeByRefType(), [bool]),
    $null)
if ($null -eq $target -or $target.ReturnType -ne [bool]) {
    throw 'Expected bool TISpaceShipTemplate.IsAValidRefitFor(TISpaceShipTemplate, out string, bool).'
}

$patchTypeName = 'TIEconomyMod.Patches.ShipAppearanceRefitValidityPatch'
$patchType = $modAssembly.GetType($patchTypeName, $true)
$postfix = $patchType.GetMethod(
    'Postfix',
    [Reflection.BindingFlags]::Public -bor [Reflection.BindingFlags]::Static)
if ($null -eq $postfix -or $postfix.ReturnType -ne [void]) {
    throw "$patchTypeName must expose a public static void Postfix."
}

$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyId = 'ti.eeo.validate.refit-appearance.' +
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

$localizationPath = Join-Path $RepositoryRoot `
    'TIEconomyMod\ModFiles\UIGeneralControls.en'
$matches = @(Get-Content -LiteralPath $localizationPath | Where-Object {
    $_ -like 'UI.Fleets.RefitFailHullAppearance=*'
})
if ($matches.Count -ne 1 -or
    $matches[0] -ne
        'UI.Fleets.RefitFailHullAppearance=Hull appearance must match.') {
    throw 'The English refit appearance failure localization must exist exactly once.'
}

Write-Host 'PASS: refit appearance-lock target, Harmony postfix, and localization validate.'
