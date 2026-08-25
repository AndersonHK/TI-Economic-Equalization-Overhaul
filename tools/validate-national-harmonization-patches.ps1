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
        throw "Required claim-harmonization validation assembly is missing: $Path"
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
    Join-Path $TargetManagedDir 'Newtonsoft.Json.dll'))
[void](Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$gameAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
$modAssembly = Load-AssemblyBytes $ModAssemblyPath

$nationType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TINationState', $true)
$regionType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TIRegionState', $true)
$instanceFlags = [Reflection.BindingFlags]'Public,Instance'
$contracts = @(
    @{
        Target = $nationType.GetMethod(
            'HostileClaimDueToDemocracy', $instanceFlags, $null,
            [Type[]]@($nationType), $null)
        Patch = 'TIEconomyMod.Patches.HostileClaimCompatibilityPatch'
    },
    @{
        Target = $nationType.GetMethod(
            'ClaimWillBeHostile', $instanceFlags, $null,
            [Type[]]@($regionType, [bool]), $null)
        Patch = 'TIEconomyMod.Patches.ClaimWillBeHostilePatch'
    },
    @{
        Target = $nationType.GetMethod(
            'WillBeHostileExplanation', $instanceFlags, $null,
            [Type[]]@($regionType), $null)
        Patch = 'TIEconomyMod.Patches.WillBeHostileExplanationPatch'
    }
)

$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyId = 'ti.eeo.validate.claim-harmonization.' +
    [Guid]::NewGuid().ToString('N')
$harmony = [Activator]::CreateInstance($harmonyType, @($harmonyId))
try {
    foreach ($contract in $contracts) {
        if ($null -eq $contract.Target) {
            throw "Installed TI 1.0.53 claim target for '$($contract.Patch)' was not found."
        }
        $patchType = $modAssembly.GetType($contract.Patch, $true)
        try {
            $patchedMethods = @($harmony.CreateClassProcessor($patchType).Patch())
        }
        catch {
            throw "Failed applying '$($contract.Patch)': $($_.Exception.ToString())"
        }
        if ($patchedMethods.Count -ne 1) {
            $actualTargets = ($patchedMethods | ForEach-Object {
                $_.ToString()
            }) -join '; '
            throw "'$($contract.Patch)' emitted $($patchedMethods.Count) replacement(s) [$actualTargets] instead of one."
        }
    }
}
finally {
    $harmony.UnpatchAll($harmonyId)
}

Write-Host 'PASS: all three claim-harmonization decision and explanation patches bind to the TI 1.0.53 nation/region contracts and Harmony emits them together.'
