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

    $coup = Read-MethodIl $nation 'Coup'
    Assert-Count $coup `
        'Coup\(\[opt\] class .*TICouncilorState councilor,\s+\[opt\] int32 strength\)' `
        1 `
        'Coup signature'
    Assert-Count $coup 'TINationState::AddToDemocracy\(' 1 'Coup Government change'
    Assert-Count $coup 'TINationState::AddToUnrest\(' 1 'Coup Unrest change'
    Assert-Count $coup 'TINationState::AddToCohesion\(' 1 'Coup Cohesion change'
    Assert-Count $coup 'TINationState::GDPPctChange\(' 1 'Coup GDP change'
    Assert-Count $coup 'TINationState::AddToInequality\(' 0 'Vanilla coup Inequality change'

    $emissions = Read-MethodIl $nation 'GHGsFromEconomy_tons'
    Assert-Count $emissions 'TINationState::get_GDP\(\)' 1 'Economy emissions GDP input'

    $climateDamage = Read-MethodIl $nation 'MeanAnnualGDPDamage'
    Assert-Count $climateDamage `
        'MeanAnnualGDPDamage\(float32 tempAnomaly_C,\s+float32 inequality\)' `
        1 `
        'Climate GDP damage signature'
    Assert-Count $climateDamage 'ldc\.r4\s+0\.25' 2 'Climate warm-damage threshold'
    Assert-Count $climateDamage 'ldc\.r4\s+-0\.99000001' 1 'Climate damage floor'

    $absorb = Read-MethodIl $nation 'AbsorbNation'
    Assert-Count $absorb 'TINationState::TransferRegionsControlTo\(' 1 'National merger region transfer'
    Assert-Count $absorb 'TINationState::ClearArmies\(\)' 1 'National merger joining-force cleanup'

    $controlCost = Read-MethodIl $nation 'get_ControlPointMaintenanceCost'
    Assert-Count $controlCost 'ldfld\s+float32 TIStartTimeTemplate::CPMaintenanceModifier' 1 'Control-point scenario multiplier'

    $controlCapacity = Read-MethodIl `
        'PavonisInteractive.TerraInvicta.TIFactionState' `
        'GetControlPointMaintenanceFreebieCap'
    Assert-Count $controlCapacity 'TIEffectsState::SumEffectsModifiers\(' 1 'Control-point capacity effect total'
    Assert-Count $controlCapacity 'System\.Linq\.Enumerable::Sum<' 2 'Control-point councilor and LEO capacity'
    Assert-Count $controlCapacity 'ldc\.r4\s+20000\.' 1 'Alien control-point capacity'

    $factionEffects = Read-MethodIl `
        'PavonisInteractive.TerraInvicta.TIEffectsState' `
        'GetFactionEffectsForContext'
    Assert-Count $factionEffects 'System\.Linq\.Enumerable::ToList<class TIEffectTemplate>' 1 'Faction effect enumeration API'

    $councilor = 'PavonisInteractive.TerraInvicta.TICouncilorState'
    $councilorAttribute = Read-MethodIl $councilor 'GetAttribute'
    Assert-Count $councilorAttribute `
        'TICouncilorState::get_maxCouncilorAttribute\(\)' `
        1 `
        'Councilor final attribute cap'

    $availableAdministration = Read-MethodIl $councilor 'get_availableAdministration'
    Assert-Count $availableAdministration `
        'TICouncilorState::get_maxCouncilorAttribute\(\)' `
        1 `
        'Councilor available-Administration cap'
    Assert-Count $availableAdministration `
        'TICouncilorState::GetAttribute\(' `
        1 `
        'Councilor available-Administration total'

    $sufficientOrgCapacity = Read-MethodIl $councilor 'SufficientCapacityForOrg'
    Assert-Count $sufficientOrgCapacity `
        'TICouncilorState::GetClampedMaxStatValue\(' `
        1 `
        'Councilor organization-weight cap'
    Assert-Count $sufficientOrgCapacity `
        'ldfld\s+int32 TIGlobalConfig::councilorMaxOrgs' `
        1 `
        'Councilor assignment organization-count cap'

    $prospectiveOrgCapacity = Read-MethodIl $councilor 'AreProspectiveOrgsValid'
    Assert-Count $prospectiveOrgCapacity `
        'ldfld\s+int32 TIGlobalConfig::councilorMaxOrgs' `
        1 `
        'Councilor prospective organization-count cap'

    $modifyCouncilorAttribute = Read-MethodIl $councilor 'ModifyAttribute'
    Assert-Count $modifyCouncilorAttribute `
        'TICouncilorState::get_maxCouncilorAttribute\(\)' `
        1 `
        'Councilor stored attribute cap'

    $clampedCouncilorMaximum = Read-MethodIl $councilor 'GetClampedMaxStatValue'
    Assert-Count $clampedCouncilorMaximum `
        'TICouncilorState::get_maxCouncilorAttribute\(\)' `
        1 `
        'Councilor augmentation cap'

    $councilorOrgCapacity = Read-MethodIl $councilor 'SpareCapacityForOrgs'
    Assert-Count $councilorOrgCapacity `
        'ldfld\s+int32 TIGlobalConfig::councilorMaxOrgs' `
        1 `
        'Councilor organization-count cap'

    $councilorStatDetail = Read-MethodIl `
        'PavonisInteractive.TerraInvicta.CouncilGridController' `
        'StatDetail'
    Assert-Count $councilorStatDetail `
        'TICouncilorState::GetClampedMaxStatValue\(' `
        1 `
        'Councilor base-cap tooltip'
    Assert-Count $councilorStatDetail `
        'ldflda\s+int32 TIGlobalConfig::maxCouncilorAttribute' `
        1 `
        'Councilor hard-cap tooltip'

    $orgPurchase = Read-MethodIl `
        'PavonisInteractive.TerraInvicta.CouncilGridController' `
        'StartOrgPurchase'
    Assert-Count $orgPurchase `
        'ldstr\s+"UI\.Councilor\.Orgs\.InsufficientAdminStat"' `
        1 `
        'Councilor organization rejection tooltip'
    Assert-Count $orgPurchase `
        'ldflda\s+int32 TIGlobalConfig::councilorMaxOrgs' `
        1 `
        'Councilor organization rejection limit'

    $megafauna = Read-MethodIl 'PavonisInteractive.TerraInvicta.TIMegafaunaArmyState' 'get_techLevel'
    Assert-Count $megafauna 'ldc\.r4\s+6\.' 1 'Xenofauna vanilla technology ceiling'

    $technologyCost = Read-MethodIl 'TITechTemplate' 'GetResearchCost'
    Assert-Count $technologyCost 'ldfld\s+float32 TIGenericTechTemplate::researchCost' 1 'Global technology research cost'

    $projectCost = Read-MethodIl 'TIProjectTemplate' 'GetResearchCost'
    Assert-Count $projectCost 'ldfld\s+float32 TIGenericTechTemplate::researchCost' 2 'Faction project research cost'

    $controlPoint = 'PavonisInteractive.TerraInvicta.TIControlPoint'
    $controlPointOwned = Read-MethodIl $controlPoint 'get_owned'
    Assert-Count $controlPointOwned 'TIControlPoint::get_faction\(\)' 1 'Control-point ownership faction source'
    Assert-Count $controlPointOwned 'TIGameState::op_Inequality\(' 1 'Control-point ownership null comparison'

    $crackdown = Read-MethodIl $controlPoint 'ResolveCrackdownEffect'
    Assert-Count $crackdown 'TIControlPoint::set_benefitsDisabled\(bool\)' 1 'Crackdown benefit suppression'
    Assert-Count $crackdown 'TIControlPoint::SetFaction\(' 0 'Crackdown ownership retention'

    $nationalIncome = Read-MethodIl `
        'PavonisInteractive.TerraInvicta.TIFactionState' `
        'GetYearlyIncomeFromNations'
    Assert-Count $nationalIncome `
        'TINationState::GetMonthlyResearchFromControlPoint\(' `
        2 `
        'Faction national-research accounting'
    Assert-Count $nationalIncome `
        'TIControlPoint::get_benefitsDisabled\(\)' `
        5 `
        'Crackdown faction-benefit exclusions'

    $jointResearch = Read-MethodIl `
        'PavonisInteractive.TerraInvicta.Actions.JointResearchDailyUpdate' `
        'Execute'
    Assert-Count $jointResearch `
        'TIGlobalResearchState::CheckForCompletedTechs\(\)' `
        1 `
        'Daily global-research completion update'

    $globalResearch = 'PavonisInteractive.TerraInvicta.TIGlobalResearchState'
    $completedTechs = Read-MethodIl $globalResearch 'CheckForCompletedTechs'
    Assert-Count $completedTechs `
        'TIGlobalResearchState::OnTechFinished\(int32\)' `
        1 `
        'Global-technology completion dispatcher'

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

    $regionDamage = Read-MethodIl `
        'PavonisInteractive.TerraInvicta.TIRegionState' `
        'ApplyDamageToRegion'
    Assert-Count $regionDamage 'AllExtantHumanNations\(\)' 3 'Nuclear global GDP enumerations'
    Assert-Count $regionDamage 'TINationState::GDPPctChange\(' 3 'Nuclear global GDP calls'
    Assert-Count $regionDamage `
        'ldc\.i4\.3\s+IL_[^:]+:\s+callvirt.*TINationState::GDPPctChange\(' `
        1 `
        'Nuclear region-damage GDP reason'
    Assert-Count $regionDamage `
        'ldc\.i4\.7\s+IL_[^:]+:\s+callvirt.*TINationState::GDPPctChange\(' `
        1 `
        'Core-economic global GDP reason'
    Assert-Count $regionDamage `
        'ldc\.i4\.8\s+IL_[^:]+:\s+callvirt.*TINationState::GDPPctChange\(' `
        1 `
        'Core-resource global GDP reason'

    $driveCompatibility = Read-MethodIl 'TIDriveTemplate' 'IsCompatible'
    Assert-Count $driveCompatibility `
        'TIDriveTemplate::get_powerRequirement_GW\(\)' `
        1 `
        'Drive compatibility raw power input'
    Assert-Count $driveCompatibility `
        'TIPowerPlantTemplate::maxOutput_GW' `
        1 `
        'Drive compatibility plant-output cap'

    $staticDriveFilter = Read-MethodIl `
        'TISpaceShipTemplate' `
        'ValidDrivesForPowerPlants'
    Assert-Count $staticDriveFilter `
        'TIDriveTemplate::get_powerRequirement_GW\(\)' `
        1 `
        'Static drive filtering raw power input'
    Assert-Count $staticDriveFilter `
        'TIPowerPlantTemplate::maxOutput_GW' `
        1 `
        'Static drive filtering plant-output cap'

    $shipState = 'PavonisInteractive.TerraInvicta.TISpaceShipState'
    $driveHeat = Read-MethodIl $shipState 'DriveHeat_GJ'
    Assert-Count $driveHeat `
        'TIDriveTemplate::get_powerRequirement_GW\(\)' `
        1 `
        'Combat drive heat raw template power bypass'

    $powerCache = Read-MethodIl $shipState 'CacheInternalPowerStats'
    Assert-Count $powerCache `
        'TISpaceShipTemplate::get_drivePowerRequirement_GW\(\)' `
        1 `
        'Live propulsion generation ship-template power source'

    Write-Host 'PASS: target IL contains every guarded TI 1.0.51 patch point, including coup effects, research ownership, councilor caps, climate damage, nuclear GDP effects, and ship drive-power consumers.'
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
