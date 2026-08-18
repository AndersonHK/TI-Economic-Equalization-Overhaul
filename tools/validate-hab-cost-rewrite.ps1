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
        throw "Required logistics validation assembly is missing: $Path"
    }
    return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
}

$ildasm = Get-ChildItem 'C:\Program Files (x86)\Microsoft SDKs\Windows' `
    -Recurse -Filter ildasm.exe -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '\\x64\\' } |
    Sort-Object FullName -Descending |
    Select-Object -First 1 -ExpandProperty FullName
if ([string]::IsNullOrWhiteSpace($ildasm)) {
    throw 'ildasm.exe was not found in the installed .NET Framework SDK.'
}

$probeDirectory = Join-Path ([IO.Path]::GetTempPath()) (
    'ti-eeo-hab-cost-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $probeDirectory | Out-Null
$outputPath = Join-Path $probeDirectory 'HabCostFromSpaceRewritePatch.il'

try {
    & $ildasm $ModAssemblyPath /text /nobar "/out:$outputPath"
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $outputPath)) {
        throw 'Could not disassemble the mod assembly.'
    }

    $assemblyIl = Get-Content -LiteralPath $outputPath -Raw
    $methodMatch = [regex]::Match(
        $assemblyIl,
        '(?s)\.class private abstract auto ansi sealed beforefieldinit ' +
        'TIEconomyMod\.Patches\.HabCostFromSpaceRewritePatch.*?' +
        '// end of method HabCostFromSpaceRewritePatch::Prefix')
    if (-not $methodMatch.Success) {
        throw 'Could not locate HabCostFromSpaceRewritePatch.Prefix IL.'
    }
    $methodIl = $methodMatch.Value
    foreach ($requiredCall in @(
        'HabLogistics::Quote\(',
        'HabConstructionCostRewrite::ToResourcesCost\(')) {
        if (-not [regex]::IsMatch($methodIl, $requiredCall)) {
            throw "Hab space-cost rewrite is missing IL call '$requiredCall'."
        }
    }
    if ([regex]::IsMatch(
        $methodIl,
        'MandatoryBoost\(|GetBoostSubstitutedCost\(')) {
        throw 'Legacy mandatory-Earth or direct vanilla substitution remains in the hab space-cost rewrite.'
    }
    if ([regex]::Matches(
            $methodIl,
            'HabLogistics::EffectiveDeliveryTime\(').Count -ne 1 -or
        [regex]::IsMatch(
            $methodIl,
            'TIEffectsState::SumEffectsModifiers\(')) {
        throw 'Hab delivery time must use the centralized payload-specific modifier exactly once.'
    }

    $buildMaterialsMatch = [regex]::Match(
        $assemblyIl,
        '(?s)\.class private abstract auto ansi sealed beforefieldinit ' +
        'TIEconomyMod\.Patches\.HabBuildMaterialsRewritePatch.*?' +
        '// end of method HabBuildMaterialsRewritePatch::Prefix')
    if (-not $buildMaterialsMatch.Success -or
        [regex]::Matches(
            $buildMaterialsMatch.Value,
            'HabRebalanceMath::GeneratorConstructionCostMultiplier\(').Count -ne 1 -or
        -not $buildMaterialsMatch.Value.Contains('TIHabModuleTemplate::power')) {
        throw 'Hab construction must apply the direct-generator 2x resource-cost multiplier exactly once.'
    }

    $probeMethodMatch = [regex]::Match(
        $assemblyIl,
        '(?s)\.class private abstract auto ansi sealed beforefieldinit ' +
        'TIEconomyMod\.Patches\.ProbeManufacturingCostPatch.*?' +
        '// end of method ProbeManufacturingCostPatch::Prefix')
    if (-not $probeMethodMatch.Success) {
        throw 'Could not locate ProbeManufacturingCostPatch.Prefix IL.'
    }
    $probeMethodIl = $probeMethodMatch.Value
    foreach ($requiredCall in @(
        'HabLogistics::EffectiveDeliveryTime\(',
        'HabLogistics::EarthDeliveryTime\(')) {
        if ([regex]::Matches($probeMethodIl, $requiredCall).Count -ne 1) {
            throw "Probe delivery time must contain one IL call '$requiredCall'."
        }
    }
    if ([regex]::IsMatch(
            $probeMethodIl,
            'TIEffectsState::SumEffectsModifiers\(')) {
        throw 'Probe transfer effects must not be applied outside the centralized delivery-time helper.'
    }

    $deliveryTimeMatch = [regex]::Match(
        $assemblyIl,
        '(?s)\.method assembly hidebysig static float32\s+' +
        'EffectiveDeliveryTime\(.*?' +
        '// end of method HabLogistics::EffectiveDeliveryTime')
    if (-not $deliveryTimeMatch.Success -or
        [regex]::Matches(
            $deliveryTimeMatch.Value,
            'TIEffectsState::SumEffectsModifiers\(').Count -ne 1) {
        throw 'Centralized logistics delivery time must apply exactly one effects context.'
    }

    $effectSnapshotMatch = [regex]::Match(
        $assemblyIl,
        '(?s)\.method assembly hidebysig static valuetype ' +
        'TIEconomyMod\.HabLogistics/LogisticsEffectSnapshot\s+' +
        'Capture\(.*?' +
        '// end of method LogisticsEffectSnapshot::Capture')
    if (-not $effectSnapshotMatch.Success -or
        -not $effectSnapshotMatch.Value.Contains(
            'TISpaceObjectState::ModifiedGenericTransferEV_kps') -or
        -not $effectSnapshotMatch.Value.Contains(
            'TIEffectsState::SumEffectsModifiers')) {
        throw 'Logistics cache identity is missing rocket-EV or off-window effect state.'
    }

    foreach ($patchType in @(
        'ProbeManufacturingCostPatch',
        'ProbeManufacturingOptionsPatch',
        'HabLogisticsAiPriorityPatch',
        'SystemAgnosticHabFoundingAvailabilityPatch',
        'SystemAgnosticHabFoundingTierPatch',
        'HabLogisticsModuleInvalidationPatch',
        'HabLogisticsHabInvalidationPatch')) {
        if (-not $assemblyIl.Contains("TIEconomyMod.Patches.$patchType")) {
            throw "Missing logistics patch type '$patchType'."
        }
    }

    if (-not $assemblyIl.Contains('TIEconomyMod.HabLogistics') -or
        -not $assemblyIl.Contains('TIEconomyMod.HabFreightQuote')) {
        throw 'Shared logistics route or freight cache types are missing.'
    }
    foreach ($removedType in @(
        'TIEconomyMod.HabLogisticsTooltips',
        'TIEconomyMod.Patches.HabEarthCostTooltipPatch',
        'TIEconomyMod.Patches.ResourcesCostLogisticsTooltipPatch',
        'TIEconomyMod.Patches.ProbeEarthCostTooltipPatch')) {
        if ($assemblyIl.Contains($removedType)) {
            throw "Removed multiline cost-label type '$removedType' is still present."
        }
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
    [void](Load-AssemblyBytes (
        Join-Path $TargetManagedDir 'Assembly-CSharp.dll'))
    $modAssembly = Load-AssemblyBytes $ModAssemblyPath
    $harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
    $harmonyId = 'ti.eeo.validate.hab-logistics.' +
        [Guid]::NewGuid().ToString('N')
    $harmony = [Activator]::CreateInstance($harmonyType, @($harmonyId))
    try {
        foreach ($patchTypeName in @(
            'TIEconomyMod.Patches.HabBuildMaterialsRewritePatch',
            'TIEconomyMod.Patches.HabBoostCostFromEarthRewritePatch',
            'TIEconomyMod.Patches.HabCostFromSpaceRewritePatch',
            'TIEconomyMod.Patches.SystemAgnosticHabFoundingAvailabilityPatch',
            'TIEconomyMod.Patches.SystemAgnosticHabFoundingTierPatch',
            'TIEconomyMod.Patches.ProbeManufacturingCostPatch',
            'TIEconomyMod.Patches.ProbeManufacturingOptionsPatch',
            'TIEconomyMod.Patches.HabLogisticsAiPriorityPatch',
            'TIEconomyMod.Patches.HabLogisticsModuleInvalidationPatch',
            'TIEconomyMod.Patches.HabLogisticsHabInvalidationPatch')) {
            $patchType = $modAssembly.GetType($patchTypeName, $true)
            try {
                [void]$harmony.CreateClassProcessor($patchType).Patch()
            }
            catch {
                throw "Failed applying '$patchTypeName': $($_.Exception.ToString())"
            }
        }
    }
    finally {
        $harmony.UnpatchAll($harmonyId)
    }

    Write-Host 'PASS: hab, founding, probe, effect-aware cache, payload-time, AI-priority, lazy-cache, and compact cost-label logistics IL is present.'
}
finally {
    $resolvedProbe = (Resolve-Path -LiteralPath $probeDirectory).Path
    $resolvedTemp = (Resolve-Path -LiteralPath ([IO.Path]::GetTempPath())).Path.TrimEnd('\')
    if (-not $resolvedProbe.StartsWith(
        $resolvedTemp + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Refusing to remove a hab-cost probe outside the system temp directory.'
    }
    Remove-Item -LiteralPath $resolvedProbe -Recurse
}
