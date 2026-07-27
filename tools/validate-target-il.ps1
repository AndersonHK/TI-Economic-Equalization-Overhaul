[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$TargetManagedDir
)

$ErrorActionPreference = 'Stop'
$assemblyPath = Join-Path $TargetManagedDir 'Assembly-CSharp.dll'
if (-not (Test-Path -LiteralPath $assemblyPath)) {
    throw "Target assembly not found: $assemblyPath"
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
    'ti-eeo-il-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $probeDirectory | Out-Null

function Read-MethodIl {
    param(
        [string]$TypeName,
        [string]$MethodName
    )

    $outputPath = Join-Path $probeDirectory ($MethodName + '.il')
    & $ildasm $assemblyPath /text /nobar "/item:$TypeName::$MethodName" "/out:$outputPath"
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $outputPath)) {
        throw "Could not disassemble $TypeName::$MethodName."
    }
    return Get-Content -LiteralPath $outputPath -Raw
}

function Assert-Count {
    param(
        [string]$Text,
        [string]$Pattern,
        [int]$Expected,
        [string]$Description
    )

    $actual = [regex]::Matches($Text, $Pattern).Count
    if ($actual -ne $Expected) {
        throw "${Description}: expected $Expected IL matches, found $actual."
    }
}

try {
    $nation = 'PavonisInteractive.TerraInvicta.TINationState'
    $economy = Read-MethodIl $nation 'OnEconomyPriorityComplete'
    Assert-Count $economy 'ldfld\s+int32 TIGlobalConfig::numEcosForCoreOilRegion' 1 'Economy oil threshold'
    Assert-Count $economy 'ldfld\s+int32 TIGlobalConfig::numEcosForCoreMiningRegion' 1 'Economy mining threshold'
    Assert-Count $economy 'ldfld\s+int32 TIGlobalConfig::numEcosForCoreEcoRegion' 1 'Economy core threshold'

    $welfare = Read-MethodIl $nation 'OnWelfarePriorityComplete'
    Assert-Count $welfare 'ldc\.i4\s+0x3e8' 1 'Welfare decolonization threshold'

    $environment = Read-MethodIl $nation 'OnEnvironmentPriorityComplete'
    Assert-Count $environment 'ldc\.i4\.s\s+100' 1 'Environment fallout threshold'

    $tooltip = Read-MethodIl 'PavonisInteractive.TerraInvicta.PriorityListItemController' 'priorityTipStr'
    Assert-Count $tooltip 'ldfld\s+int32 TIGlobalConfig::numEcosForCoreOilRegion' 1 'Tooltip oil threshold'
    Assert-Count $tooltip 'ldfld\s+int32 TIGlobalConfig::numEcosForCoreMiningRegion' 1 'Tooltip mining threshold'
    Assert-Count $tooltip 'ldfld\s+int32 TIGlobalConfig::numEcosForCoreEcoRegion' 1 'Tooltip core threshold'
    Assert-Count $tooltip 'ldc\.i4\s+0x3e8' 1 'Tooltip decolonization threshold'
    Assert-Count $tooltip 'ldc\.i4\.s\s+100' 1 'Tooltip fallout threshold'

    $unity = Read-MethodIl $nation 'OnUnityPriorityComplete'
    Assert-Count $unity 'ldfld\s+float32 TIGlobalConfig::unityPublicOpinionBaseStrength' 1 'Unity propaganda strength'

    $spoils = Read-MethodIl $nation 'OnSpoilsPriorityComplete'
    Assert-Count $spoils 'ldfld\s+float32 TIGlobalConfig::spoilsPriorityPublicOpinionScaling' 1 'Spoils propaganda scaling'

    $emissions = Read-MethodIl $nation 'GHGsFromEconomy_tons'
    Assert-Count $emissions 'TINationState::get_GDP\(\)' 1 'Economy emissions GDP input'

    $absorb = Read-MethodIl $nation 'AbsorbNation'
    Assert-Count $absorb 'TINationState::TransferRegionsControlTo\(' 1 'National merger region transfer'
    Assert-Count $absorb 'TINationState::ClearArmies\(\)' 1 'National merger joining-force cleanup'

    $controlCost = Read-MethodIl $nation 'get_ControlPointMaintenanceCost'
    Assert-Count $controlCost 'ldfld\s+float32 TIStartTimeTemplate::CPMaintenanceModifier' 1 'Control-point scenario multiplier'

    $megafauna = Read-MethodIl 'PavonisInteractive.TerraInvicta.TIMegafaunaArmyState' 'get_techLevel'
    Assert-Count $megafauna 'ldc\.r4\s+6\.' 1 'Xenofauna vanilla technology ceiling'

    $technologyCost = Read-MethodIl 'TITechTemplate' 'GetResearchCost'
    Assert-Count $technologyCost 'ldfld\s+float32 TIGenericTechTemplate::researchCost' 1 'Global technology research cost'

    $habTemplate = 'TIHabModuleTemplate'
    $boostCost = Read-MethodIl $habTemplate 'BoostCostFromEarth'
    Assert-Count $boostCost 'TISpaceObjectState::GenericTransferBoostFromEarthSurface\(' 1 'Hab Earth boost conversion'

    $spaceCost = Read-MethodIl $habTemplate 'CostFromSpace'
    Assert-Count $spaceCost 'TISpaceObjectState::GenericTransferTimeFromEarthsSurface_d\(' 1 'Hab Earth transfer time'
    Assert-Count $spaceCost 'TIEffectsState::SumEffectsModifiers\(' 1 'Hab transfer-time effect'

    $habState = 'PavonisInteractive.TerraInvicta.TIHabState'
    $newHab = Read-MethodIl $habState 'InitializeNewHab'
    Assert-Count $newHab 'TIHabState::InitializeSector\(' 9 'Station and base sector initialization'

    $completeModule = Read-MethodIl $habState 'CompleteModuleConstruction'
    Assert-Count $completeModule 'TISectorState::SetFaction\(' 8 'Vanilla station and base upgrade sectors'

    $repairHab = Read-MethodIl $habState 'PostEverythingSaveRepair_8'
    Assert-Count $repairHab 'TIHabState::ActiveModules\(\)' 1 'Per-hab save repair hook'

    $connectorMap = Read-MethodIl `
        'PavonisInteractive.TerraInvicta.TISectorState' `
        'UpdateModuleConnectorMap'
    Assert-Count $connectorMap `
        'TIHabState::get_tier\(\)\s+IL_[^:]+:\s+ldc\.i4\.2\s+IL_[^:]+:\s+blt(?:\.s)?\s+IL_' `
        4 `
        'Hab connector tier-two gates'
    Assert-Count $connectorMap `
        'TIHabState::sectors\s+IL_[^:]+:\s+ldc\.i4\.2\s+IL_[^:]+:\s+callvirt.*get_Item\(int32\)\s+IL_[^:]+:\s+callvirt.*TISectorState::HasAnyModules\(\)' `
        2 `
        'Hab internal-sector-two connector checks'
    Assert-Count $connectorMap `
        'TIHabState::sectors\s+IL_[^:]+:\s+ldc\.i4\.4\s+IL_[^:]+:\s+callvirt.*get_Item\(int32\)\s+IL_[^:]+:\s+callvirt.*TISectorState::HasAnyModules\(\)' `
        2 `
        'Hab internal-sector-four connector check'

    $habListItem = Read-MethodIl `
        'PavonisInteractive.TerraInvicta.HabListItem' `
        'UpdateItem'
    Assert-Count $habListItem `
        'HabListItem::habState\s+IL_[^:]+:\s+ldfld.*TIHabState::sectors\s+IL_[^:]+:\s+ldloc\.1\s+IL_[^:]+:\s+callvirt.*get_Item\(int32\)\s+IL_[^:]+:\s+callvirt.*TISectorState::get_active\(\)\s+IL_[^:]+:\s+brfalse(?:\.s)?\s+IL_' `
        1 `
        'Hab-list station-sector icon loop'

    Write-Host 'PASS: target IL contains every guarded TI 1.0.47 patch point.'
}
finally {
    $resolvedProbe = (Resolve-Path -LiteralPath $probeDirectory).Path
    $resolvedTemp = (Resolve-Path -LiteralPath ([IO.Path]::GetTempPath())).Path.TrimEnd('\')
    if (-not $resolvedProbe.StartsWith(
        $resolvedTemp + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Refusing to remove an IL probe directory outside the system temp directory.'
    }
    Remove-Item -LiteralPath $resolvedProbe -Recurse
}
