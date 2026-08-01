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
        throw "Required skirmish-cache validation assembly is missing: $Path"
    }
    return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
}

$harmonyAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($assemblyName in @(
    'Newtonsoft.Json.dll',
    'FMODUnity.dll',
    'Unity.Burst.dll',
    'Unity.Collections.dll',
    'Unity.Jobs.dll',
    'Unity.Mathematics.dll',
    'Unity.Entities.dll',
    'UnityEngine.CoreModule.dll',
    'UnityEngine.SharedInternalsModule.dll',
    'UnityEngine.dll',
    'UnityEngine.IMGUIModule.dll',
    'UnityEngine.ParticleSystemModule.dll',
    'UnityEngine.PhysicsModule.dll',
    'UnityEngine.UI.dll',
    'UnityEngine.UIModule.dll',
    'Unity.TextMeshPro.dll'
)) {
    $assemblyPath = Join-Path $TargetManagedDir $assemblyName
    if (Test-Path -LiteralPath $assemblyPath) {
        [void](Load-AssemblyBytes $assemblyPath)
    }
}
[void](Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$gameAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
$modAssembly = Load-AssemblyBytes $ModAssemblyPath

$patchProcessorType = $harmonyAssembly.GetType(
    'HarmonyLib.PatchProcessor', $true)
$instructionReader = @($patchProcessorType.GetMethods(
    [Reflection.BindingFlags]'Public,Static') | Where-Object {
        $_.Name -eq 'GetOriginalInstructions' -and
        $_.GetParameters().Count -eq 2 -and
        -not $_.GetParameters()[1].ParameterType.IsByRef
    })
if ($instructionReader.Count -ne 1) {
    throw "Expected one usable Harmony instruction reader, found $($instructionReader.Count)."
}

function Read-Instructions {
    param([Reflection.MethodBase]$Method)

    $arguments = [object[]]::new(2)
    $arguments[0] = $Method
    $arguments[1] = $null
    return @($instructionReader[0].PSObject.BaseObject.Invoke(
        $null, $arguments))
}

function Get-Calls {
    param([Reflection.MethodBase]$Method)

    return @(Read-Instructions $Method | Where-Object {
        $_.operand -is [Reflection.MethodInfo]
    } | ForEach-Object { $_.operand })
}

$rowType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.SkirmishShipListItemController',
    $true)
$startMenuType = $gameAssembly.GetType('StartMenuController', $true)
$populateDropdown = $rowType.GetMethod(
    'PopulateShipDropdown',
    [Reflection.BindingFlags]'Public,Instance')
$addSpecificShip = $rowType.GetMethod(
    'AddSpecificShip',
    [Reflection.BindingFlags]'Public,Instance')
$importSetter = $startMenuType.GetProperty(
    'ImportedShipTemplates',
    [Reflection.BindingFlags]'Public,Instance').GetSetMethod()
if ($null -eq $populateDropdown -or
    $null -eq $addSpecificShip -or
    $null -eq $importSetter) {
    throw 'A guarded TI 1.0.51 skirmish-menu target is missing.'
}

$populateCalls = @(Get-Calls $populateDropdown)
$combatValueCalls = @($populateCalls | Where-Object {
    $_.Name -eq 'TemplateSpaceCombatValue'
})
if ($combatValueCalls.Count -ne 1) {
    throw "Vanilla PopulateShipDropdown combat-value calls changed: $($combatValueCalls.Count)."
}
$fullRefreshCalls = @(Get-Calls $addSpecificShip | Where-Object {
    $_.Name -eq 'PopulateSkirmishDropdowns'
})
if ($fullRefreshCalls.Count -ne 1) {
    throw "Vanilla AddSpecificShip full-refresh calls changed: $($fullRefreshCalls.Count)."
}

$runtimeType = $modAssembly.GetType(
    'TIEconomyMod.Patches.SkirmishDropdownCacheRuntime', $true)
$runtimePopulate = $runtimeType.GetMethod(
    'Populate', [Reflection.BindingFlags]'Public,Static')
$buildCatalog = $runtimeType.GetMethod(
    'BuildCatalog', [Reflection.BindingFlags]'NonPublic,Static')
if ($null -eq $runtimePopulate -or $null -eq $buildCatalog) {
    throw 'Packaged skirmish dropdown cache runtime is incomplete.'
}

$runtimeCalls = @(Get-Calls $runtimePopulate)
foreach ($requiredCall in @(
    'GetOrCreate',
    'AddRange',
    'SetValueWithoutNotify',
    'RefreshShownValue'
)) {
    if (@($runtimeCalls | Where-Object {
        $_.Name -eq $requiredCall
    }).Count -ne 1) {
        throw "Skirmish cached population must call '$requiredCall' exactly once."
    }
}
if (@($runtimeCalls | Where-Object {
    $_.Name -eq 'TemplateSpaceCombatValue' -or $_.Name -eq 'T'
}).Count -ne 0) {
    throw 'Stable skirmish row population must not rebuild localized combat-score options.'
}

$catalogCalls = @(Get-Calls $buildCatalog)
if (@($catalogCalls | Where-Object {
    $_.Name -eq 'TemplateSpaceCombatValue'
}).Count -ne 1 -or
    @($catalogCalls | Where-Object {
        $_.Name -eq 'T'
    }).Count -ne 2) {
    throw 'Skirmish combat-score and localization work must be isolated in the cached catalog builder.'
}

$gunRegistryType = $modAssembly.GetType(
    'TIEconomyMod.GunPowerRegistry', $true)
$gunLookup = $gunRegistryType.GetMethod(
    'TryGetPowerUse_MJ', [Reflection.BindingFlags]'Public,Static')
$gunRefresh = $gunRegistryType.GetMethod(
    'Refresh', [Reflection.BindingFlags]'Public,Static')
$lookupCalls = @(Get-Calls $gunLookup)
if (@($lookupCalls | Where-Object {
    $_.Name -eq 'TryGetValue' -and
    $_.DeclaringType.IsGenericType -and
    $_.DeclaringType.GetGenericArguments()[0].Name -eq 'TIGunTemplate'
}).Count -ne 1 -or
    @($lookupCalls | Where-Object {
        $_.Name -eq 'TryGet' -and
        $_.DeclaringType.Name -eq 'TemplateFloatExtensionReader'
    }).Count -ne 1) {
    throw 'Gun power lookup must use one identity lookup with one dynamic-template fallback.'
}
$refreshCalls = @(Get-Calls $gunRefresh)
if (@($refreshCalls | Where-Object {
    $_.Name -eq 'GetAllTemplates'
}).Count -ne 1) {
    throw 'Gun power hydration must bind loaded template instances exactly once.'
}

$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyId = 'ti-eeo.skirmish-cache-validation.' +
    [Guid]::NewGuid().ToString('N')
$harmony = [Activator]::CreateInstance(
    $harmonyType, [object[]]@($harmonyId))
try {
    foreach ($patchTypeName in @(
        'TIEconomyMod.Patches.SkirmishShipDropdownCachePatch',
        'TIEconomyMod.Patches.SkirmishImportedShipCacheInvalidationPatch'
    )) {
        $patchType = $modAssembly.GetType($patchTypeName, $true)
        [void]$harmony.CreateClassProcessor($patchType).Patch()
    }
}
catch {
    $failure = $_.Exception
    while ($failure.InnerException) {
        $failure = $failure.InnerException
    }
    throw $failure
}
finally {
    $harmony.UnpatchAll($harmonyId)
}

Write-Host 'PASS: skirmish rows reuse cached option catalogs and hydrated guns use allocation-free identity lookups.'
