[CmdletBinding()]
param(
    [string]$TargetManagedDir
)

$ErrorActionPreference = 'Stop'
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = Split-Path -Parent $scriptDirectory
$buildStarted = Get-Date
$managedPathFile = Join-Path ([IO.Path]::GetTempPath()) (
    'ti-eeo-managed-' + [Guid]::NewGuid().ToString('N') + '.txt')

try {
    & (Join-Path $scriptDirectory 'build.ps1') -Configuration Release `
        -TargetManagedDir $TargetManagedDir `
        -WriteResolvedManagedDirPath $managedPathFile
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
    $resolvedManagedDir = Get-Content -LiteralPath $managedPathFile -Raw
}
finally {
    if (Test-Path -LiteralPath $managedPathFile) {
        Remove-Item -LiteralPath $managedPathFile
    }
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-target-il.ps1') `
    -TargetManagedDir $resolvedManagedDir
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$assemblyPath = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\Assembly\TIEconomyMod.dll'
powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-nuclear-gdp-transpiler.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-councilor-cap-transpiler.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-hab-connector-transpiler.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-hab-list-icon-transpiler.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-hab-cost-rewrite.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-ship-power-transpilers.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-alien-ship-design-patch.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-utility-footprint-patches.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath `
    -RepositoryRoot $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-skirmish-performance-patches.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-projectile-collision-patches.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-direct-fire-coordination-patches.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-weapon-cadence-patches.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-mine-mc-patches.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-earth-orbits-and-luna.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath `
    -RepositoryRoot $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-ai-technology-selection.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-refit-appearance-lock.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath `
    -RepositoryRoot $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-cohesion-rest-patches.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $scriptDirectory 'validate-implementation-matrix.ps1') -RepositoryRoot $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$installation = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
$msbuild = Join-Path $installation 'MSBuild\Current\Bin\MSBuild.exe'
$testProject = Join-Path $repositoryRoot 'tests\FormulaTests\FormulaTests.csproj'
& $msbuild $testProject '/t:Rebuild' '/p:Configuration=Release' '/v:minimal' '/nologo'
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$weights = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\Config\economy-tech-weights.csv'
$hullVolumes = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\Config\hull-variant-main-volumes.csv'
$hullDriveScales = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\Config\hull-variant-drive-scales.csv'
$defaultSettings = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\Settings.xml'
$missionOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIMissionTemplate.json'
$startOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIStartTimeTemplate.json'
$armyOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIArmyTemplate.json'
$metaOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIMetaTemplate.json'
$technologyOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TITechTemplate.json'
$effectOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIEffectTemplate.json'
$projectOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIProjectTemplate.json'
$globalOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIGlobalConfig.json'
$habModuleOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIHabModuleTemplate.json'
$habOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIHabTemplate.json'
$powerPlantOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIPowerPlantTemplate.json'
$driveOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIDriveTemplate.json'
$heatSinkOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIHeatSinkTemplate.json'
$gunOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIGunTemplate.json'
$laserWeaponOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TILaserWeaponTemplate.json'
$magneticGunOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIMagneticGunTemplate.json'
$shipHullOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIShipHullTemplate.json'
$utilityModuleOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIUtilityModuleTemplate.json'
$nationLocalization = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\UINation.en'
$effectLocalization = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIEffectTemplate.en'
$technologyLocalization = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TITechTemplate.en'
$scienceLocalization = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\UIScience.en'
$habModuleLocalization = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIHabModuleTemplate.en'
$projectLocalization = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIProjectTemplate.en'
$operationLocalization = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIOperationTemplate.en'
$codexLocalization = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\UICodex.en'
$generalControlsLocalization = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\UIGeneralControls.en'
$habUiLocalization = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\UIHabs.en'
$testExecutable = Join-Path $repositoryRoot 'tests\FormulaTests\bin\Release\TIEconomyMod.FormulaTests.exe'
& $testExecutable $weights
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$templatesDirectory = Join-Path (Split-Path -Parent $resolvedManagedDir) 'StreamingAssets\Templates'
powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-starting-forces.ps1') `
    -VanillaTemplatesDir $templatesDirectory `
    -RepositoryRoot $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-starting-economic-overrides.ps1') `
    -VanillaTemplatesDir $templatesDirectory `
    -RepositoryRoot $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-environment-model.ps1') `
    -RepositoryRoot $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$technologyTemplates = Join-Path $templatesDirectory 'TITechTemplate.json'
$installedTechnologyIds = @(
    Get-Content -LiteralPath $technologyTemplates -Raw |
        ConvertFrom-Json |
        ForEach-Object { $_.dataName }
)
$weightRows = @(Import-Csv -LiteralPath $weights)
if ($weightRows.Count -ne 149 -or $installedTechnologyIds.Count -ne 149) {
    throw "Technology catalog coverage is $($weightRows.Count)/$($installedTechnologyIds.Count), expected 149/149."
}
$weightHeaders = @($weightRows[0].PSObject.Properties.Name)
$expectedWeightHeaders = @(
    'tech_id',
    'enabled',
    'productivity_percent',
    'labor_substitution',
    'resource_substitution',
    'rationale'
)
if (($weightHeaders -join ';') -ne ($expectedWeightHeaders -join ';')) {
    throw "Technology CSV has unexpected columns: $($weightHeaders -join ', ')."
}
$duplicateTechnologyIds = $weightRows | Group-Object tech_id | Where-Object Count -gt 1
if ($duplicateTechnologyIds) {
    throw "Technology CSV contains duplicate IDs: $($duplicateTechnologyIds.Name -join ', ')."
}
$missingTechnologyIds = @($installedTechnologyIds | Where-Object { $_ -notin $weightRows.tech_id })
$unknownTechnologyIds = @($weightRows.tech_id | Where-Object { $_ -notin $installedTechnologyIds })
if ($missingTechnologyIds.Count -gt 0 -or $unknownTechnologyIds.Count -gt 0) {
    throw "Technology CSV mismatch. Missing: $($missingTechnologyIds -join ', '); unknown: $($unknownTechnologyIds -join ', ')."
}
$technologyProduct = 1.0
$futureLaborTotal = 0.0
$futureResourceTotal = 0.0
foreach ($row in $weightRows) {
    $productivity = [double]::Parse(
        $row.productivity_percent,
        [Globalization.CultureInfo]::InvariantCulture)
    $labor = [double]::Parse(
        $row.labor_substitution,
        [Globalization.CultureInfo]::InvariantCulture)
    $resources = [double]::Parse(
        $row.resource_substitution,
        [Globalization.CultureInfo]::InvariantCulture)
    if ($row.enabled -ne 'true' -or $productivity -le 0 -or
        $labor -le 0 -or $resources -le 0) {
        throw "Technology '$($row.tech_id)' is not enabled with three positive weights."
    }
    $technologyProduct *= 1.0 + $productivity / 100.0
    if ($row.tech_id -notin @('MissionToSpace', 'AdvancedChemicalRocketry')) {
        $futureLaborTotal += $labor
        $futureResourceTotal += $resources
    }
}
if ([Math]::Abs($technologyProduct - 3.40) -gt 0.00001) {
    throw "Full technology tree compounds to $technologyProduct instead of 3.40."
}
if ($futureLaborTotal -le 0 -or $futureResourceTotal -le 0) {
    throw 'Technology CSV future substitution totals must both be positive.'
}
$startingRows = @($weightRows | Where-Object {
    $_.tech_id -in @('MissionToSpace', 'AdvancedChemicalRocketry')
})
$startingProduct = 1.0
foreach ($row in $startingRows) {
    $startingProduct *= 1.0 + [double]::Parse(
        $row.productivity_percent,
        [Globalization.CultureInfo]::InvariantCulture) / 100.0
}
if ($startingRows.Count -ne 2 -or [Math]::Abs($startingProduct - 1.0201) -gt 0.0000001) {
    throw "Starting technology product is $startingProduct instead of 1.0201."
}

[xml]$settingsXml = Get-Content -LiteralPath $defaultSettings -Raw
$mainSource = Get-Content -LiteralPath (Join-Path $repositoryRoot 'TIEconomyMod\Main.cs') -Raw
$groupMatches = [regex]::Matches(
    $mainSource,
    'public\s+(?<type>\w+Settings)\s+(?<name>\w+)\s*=\s*new\s+\w+Settings\(\);')
foreach ($groupMatch in $groupMatches) {
    $groupType = $groupMatch.Groups['type'].Value
    $groupName = $groupMatch.Groups['name'].Value
    $classMatch = [regex]::Match(
        $mainSource,
        "public sealed class $groupType\s*\{(?<body>[\s\S]*?)\r?\n    \}")
    if (-not $classMatch.Success) {
        throw "Could not inspect defaults for $groupType."
    }
    $fieldMatches = [regex]::Matches(
        $classMatch.Groups['body'].Value,
        'public\s+(?<type>bool|float)\s+(?<name>\w+)\s*=\s*(?<value>[^;]+);')
    $groupNode = $settingsXml.Settings.$groupName
    if ($null -eq $groupNode) {
        throw "Default Settings.xml is missing group '$groupName'."
    }
    foreach ($fieldMatch in $fieldMatches) {
        $fieldName = $fieldMatch.Groups['name'].Value
        $fieldType = $fieldMatch.Groups['type'].Value
        $sourceValue = $fieldMatch.Groups['value'].Value.Trim().TrimEnd('f')
        $xmlNode = $groupNode.$fieldName
        if ($null -eq $xmlNode) {
            throw "Default Settings.xml is missing '$groupName.$fieldName'."
        }
        if ($fieldType -eq 'bool') {
            if ([bool]::Parse($sourceValue) -ne [bool]::Parse([string]$xmlNode)) {
                throw "Default Settings.xml does not match '$groupName.$fieldName'."
            }
        }
        else {
            $sourceNumber = [double]::Parse(
                $sourceValue,
                [Globalization.CultureInfo]::InvariantCulture)
            $xmlNumber = [double]::Parse(
                [string]$xmlNode,
                [Globalization.CultureInfo]::InvariantCulture)
            if ([Math]::Abs($sourceNumber - $xmlNumber) -gt 0.000000001) {
                throw "Default Settings.xml does not match '$groupName.$fieldName'."
            }
        }
    }
}
if (-not [bool]::Parse([string]$settingsXml.Settings.enabled)) {
    throw 'Default Settings.xml must enable the global mod toggle.'
}

$effectTemplates = Get-Content -LiteralPath (Join-Path $templatesDirectory 'TIEffectTemplate.json') -Raw |
    ConvertFrom-Json
$effectTemplateOverrides = Get-Content -LiteralPath $effectOverrides -Raw |
    ConvertFrom-Json
$expectedControlEffects = [ordered]@{
    Effect_ControlPointMaintenanceBonus160 = -120
    Effect_ControlPointMaintenanceBonus40 = -40
    Effect_ControlPointMaintenanceBonus20 = -20
    Effect_ControlPointMaintenanceBonus10 = -10
    Effect_ControlPointMaintenanceBonus3 = -5
}
$effectLocalizationText = Get-Content -LiteralPath $effectLocalization -Raw
foreach ($entry in $expectedControlEffects.GetEnumerator()) {
    $template = @($effectTemplates | Where-Object { $_.dataName -eq $entry.Key })
    if ($template.Count -ne 1 -or $template[0].operation -ne 'Additive' -or
        [double]$template[0].value -ne [double]$entry.Value -or
        -not [bool]$template[0].stackable) {
        throw "Installed control-capacity effect '$($entry.Key)' no longer matches the percentage conversion contract."
    }
    $key = "TIEffectTemplate.description.$($entry.Key)="
    if ([regex]::Matches($effectLocalizationText, "(?m)^$([regex]::Escape($key))").Count -ne 1 -or
        $effectLocalizationText -notmatch "(?m)^$([regex]::Escape($key)).*%") {
        throw "Project localization is missing the percentage tooltip for '$($entry.Key)'."
    }
}

$technologyLocalizationText = Get-Content -LiteralPath $technologyLocalization -Raw
$controlTechnologies = [ordered]@{
    ArrivalInternationalRelations = '0.02'
    UnityMovements = '0.03'
    GreatNations = '0.05'
    ArrivalGovernance = '0.05'
    Accelerando = '0.05'
}
foreach ($entry in $controlTechnologies.GetEnumerator()) {
    $technologyId = $entry.Key
    $key = "TITechTemplate.summary.$technologyId="
    $line = [regex]::Match(
        $technologyLocalizationText,
        "(?m)^$([regex]::Escape($key)).*$")
    $expectedSentence = "Reduces the Control Point economy-exponent score by $($entry.Value)."
    if (-not $line.Success -or $line.Value -notmatch [regex]::Escape($expectedSentence)) {
        throw "Technology localization has the wrong control-cost tooltip for '$technologyId'."
    }
}

& node (Join-Path $repositoryRoot 'tools\economy-growth-simulator.js') | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw 'Economy growth simulator failed.'
}

& node (Join-Path $repositoryRoot 'tools\military-investment-simulator.js') '--verify' | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw 'Military investment simulator failed.'
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-hab-rebalance.ps1') `
    -VanillaTemplatesDir $templatesDirectory `
    -RepositoryRoot $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-ship-rebalance.ps1') `
    -VanillaTemplatesDir $templatesDirectory `
    -RepositoryRoot $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-hull-variant-report.ps1') `
    -VanillaTemplatesDir $templatesDirectory `
    -RepositoryRoot $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$manifestPath = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\ModInfo.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.GameVersion -ne '1.0.51') {
    throw "ModInfo.json targets '$($manifest.GameVersion)' instead of TI 1.0.51."
}
if ($manifest.Version -ne '0.9.3') {
    throw "ModInfo.json version '$($manifest.Version)' does not match this release."
}
if ($manifest.AssemblyName -ne 'Assembly/TIEconomyMod.dll') {
    throw "ModInfo.json has unexpected AssemblyName '$($manifest.AssemblyName)'."
}

$missions = Get-Content -LiteralPath $missionOverrides -Raw | ConvertFrom-Json
$enthrall = $null
$purge = $null
foreach ($mission in $missions) {
    if ($mission.dataName -eq 'EnthrallElites') {
        $enthrall = $mission
    }
    elseif ($mission.dataName -eq 'Purge') {
        $purge = $mission
    }
}
if ($enthrall.resolutionMethod.'$type' -ne 'TIMissionResolution_Contested' -or
    $purge.resolutionMethod.'$type' -ne 'TIMissionResolution_Contested' -or
    $enthrall.resolutionMethod.defendingModifiers[0].flatModifier -ne 3 -or
    $purge.resolutionMethod.defendingModifiers[0].flatModifier -ne 4) {
    throw 'Mission overrides must retain their contested resolution type and add one flat defense to Enthrall Elites and Purge.'
}

$logisticsLocalization = @(
    $habModuleLocalization,
    $projectLocalization,
    $operationLocalization,
    $codexLocalization)
foreach ($localizationPath in $logisticsLocalization) {
    if (-not (Test-Path -LiteralPath $localizationPath)) {
        throw "Missing logistics localization '$localizationPath'."
    }
    $localizationText = Get-Content -LiteralPath $localizationPath -Raw
    if ($localizationText -match 'same planetary system|additional Orbitals|nearby\.') {
        throw "Legacy locality wording remains in '$localizationPath'."
    }
}
if (-not (Get-Content -LiteralPath $habModuleLocalization -Raw).Contains('on the same hab') -or
    -not (Get-Content -LiteralPath $operationLocalization -Raw).Contains('factory-dock pair in any system') -or
    -not (Get-Content -LiteralPath $codexLocalization -Raw).Contains('reduce Boost use; Earth is the fallback')) {
    throw 'Logistics localization does not concisely describe paired, system-agnostic routing.'
}
$documentationIndex = Join-Path $repositoryRoot 'docs\README.md'
$logisticsDocumentation = Join-Path $repositoryRoot 'docs\manufacturing-logistics.md'
foreach ($documentationPath in @($documentationIndex, $logisticsDocumentation)) {
    if (-not (Test-Path -LiteralPath $documentationPath)) {
        throw "Missing current documentation authority '$documentationPath'."
    }
}
$logisticsDocumentationText = Get-Content -LiteralPath $logisticsDocumentation -Raw
foreach ($requiredRule in @(
    'P = max(0, M / 3 - E)',
    'same-hab factory-dock pair',
    'Rocket projects change `modifiedExhaustVelocity`',
    'Solar Steamers reduces only an applicable off-launch-window penalty',
    'Earth-Moon receives the strongest priority',
    'No invalidation performs a network scan')) {
    if (-not $logisticsDocumentationText.Contains($requiredRule)) {
        throw "Manufacturing logistics documentation is missing '$requiredRule'."
    }
}
if ($logisticsDocumentationText -match 'same planetary system|additional Orbitals|nearby factory') {
    throw 'Manufacturing logistics documentation retains obsolete routing wording.'
}
$starts = Get-Content -LiteralPath $startOverrides -Raw | ConvertFrom-Json
$modernStart = $null
$start2026 = $null
foreach ($start in $starts) {
    if ($start.dataName -eq 'ModernDayStart') {
        $modernStart = $start
    }
    elseif ($start.dataName -eq '2026Start') {
        $start2026 = $start
    }
}
if ($null -eq $modernStart -or
    ($modernStart.startingTechs -join ';') -ne
        'Skywatch;WeAreNotAlone;OutpostHabs' -or
    ($modernStart.globalTechsCompleted -join ';') -ne
        'MissionToSpace;AdvancedChemicalRocketry;SpaceTourism;DeepSpacePropulsionConcepts;AugmentedReality') {
    throw 'The 2022 start must retain Skywatch and Outpost Habs as active research and complete the five approved baseline technologies.'
}
if ($null -eq $start2026 -or
    ($start2026.startingTechs -join ';') -ne
        'DeepSystemSkywatch;WeAreNotAlone;MissiontotheMoon' -or
    ($start2026.globalTechsCompleted -join ';') -ne
        'MissionToSpace;AdvancedChemicalRocketry;SpaceTourism;DeepSpacePropulsionConcepts;AugmentedReality;Skywatch;OutpostHabs') {
    throw 'The 2026 start must complete the five approved baseline technologies plus Skywatch and Outpost Habs, replacing them with Deep System Skywatch and Mission to the Moon.'
}
foreach ($scenario in @($modernStart, $start2026)) {
    $duplicateStartingTechnologies = @(
        $scenario.startingTechs |
            Where-Object { $_ -in $scenario.globalTechsCompleted })
    if ($duplicateStartingTechnologies.Count -gt 0) {
        throw "Start '$($scenario.dataName)' has technologies both active and completed: $($duplicateStartingTechnologies -join ', ')."
    }
}
$expectedCompletedStartProjects = @(
    'Project_Solid-FuelSpaceRockets',
    'Project_Liquid-FuelRockets',
    'Project_CryogenicLiquid-FuelRockets',
    'Project_LifeScienceLab',
    'Project_MaterialsLab',
    'Project_PlatformCore',
    'Project_SolarCollector',
    'Project_SpaceScienceLab',
    'Project_ReusableRockets'
)
foreach ($scenario in @($modernStart, $start2026)) {
    if (($scenario.projectsCompleted -join ';') -ne
        ($expectedCompletedStartProjects -join ';')) {
        throw "Start '$($scenario.dataName)' must complete the eight baseline projects and Reusable Rockets."
    }
}
$vanillaTechnologyPath = Join-Path $templatesDirectory 'TITechTemplate.json'
$vanillaTechnologies =
    Get-Content -LiteralPath $vanillaTechnologyPath -Raw | ConvertFrom-Json
$technologyCostOverrides =
    Get-Content -LiteralPath $technologyOverrides -Raw | ConvertFrom-Json
$expectedTechnologyIds = @('MissionToSpace', 'Skywatch', 'WeAreNotAlone')
foreach ($technologyId in $expectedTechnologyIds) {
    $vanillaTechnology = @(
        $vanillaTechnologies | Where-Object { $_.dataName -eq $technologyId })
    $overrideTechnology = @(
        $technologyCostOverrides | Where-Object { $_.dataName -eq $technologyId })
    if ($vanillaTechnology.Count -ne 1 -or
        $overrideTechnology.Count -ne 1 -or
        [double]$overrideTechnology[0].researchCost -ne
            2 * [double]$vanillaTechnology[0].researchCost) {
        throw "Technology '$technologyId' must override the installed vanilla research cost at exactly x2."
    }
}
$mineEffectByTechnology = [ordered]@{
    FutureTechSpaceScience = 'Effect_SpaceMineFreebies1'
    MissiontoJupiter = 'Effect_SpaceMineFreebies6'
    MissiontoMars = 'Effect_SpaceMineFreebies6'
    MissiontoSaturn = 'Effect_SpaceMineFreebies6'
    MissiontotheAsteroids = 'Effect_SpaceMineFreebies6'
    MissiontotheInnerPlanets = 'Effect_SpaceMineFreebies3'
    MissiontotheMoon = 'Effect_SpaceMineFreebies3'
    MissiontotheOuterPlanets = 'Effect_SpaceMineFreebies6'
}
$expectedOverrideIds = @($expectedTechnologyIds) +
    @($mineEffectByTechnology.Keys) +
    @(
        'AugmentedReality',
        'CarbonNanotubes',
        'Superalloys',
        'DeuteriumTritiumFusion',
        'NuclearFusioninSpace',
        'DeuteriumDeuteriumFusion',
        'DeuteriumHelium3Fusion'
    )
if ($technologyCostOverrides.Count -ne $expectedOverrideIds.Count -or
    @($technologyCostOverrides | Where-Object {
        $_.dataName -notin $expectedOverrideIds
    }).Count -ne 0) {
    throw 'Technology overrides must contain the three doubled starts, eight retired free-mine effect edits, the four Military-ceiling additions, and the three approved fusion-tree edits.'
}
$vanillaAugmentedReality = @($vanillaTechnologies | Where-Object {
    $_.dataName -eq 'AugmentedReality'
})
$augmentedRealityOverride = @($technologyCostOverrides | Where-Object {
    $_.dataName -eq 'AugmentedReality'
})
$maximumArmyTechnologyEffect = @($effectTemplates | Where-Object {
    $_.dataName -eq 'Effect_IncreaseMaxArmyTechLevel'
})
$maximumArmyTechnologyEffectOverride = @($effectTemplateOverrides | Where-Object {
    $_.dataName -eq 'Effect_IncreaseMaxArmyTechLevel'
})
if ($vanillaAugmentedReality.Count -ne 1 -or
    [double]$vanillaAugmentedReality[0].researchCost -ne 1500 -or
    ($vanillaAugmentedReality[0].effects -join ';') -ne '' -or
    $augmentedRealityOverride.Count -ne 1 -or
    [double]$augmentedRealityOverride[0].researchCost -ne 2000 -or
    ($augmentedRealityOverride[0].effects -join ';') -ne
        'Effect_IncreaseMaxArmyTechLevel' -or
    $maximumArmyTechnologyEffect.Count -ne 1 -or
    $maximumArmyTechnologyEffect[0].instantEffect -ne 'NationMaxMiltechChange' -or
    [double]$maximumArmyTechnologyEffect[0].value -ne 0.5 -or
    $maximumArmyTechnologyEffect[0].effectTarget -ne 'AllNations' -or
    $maximumArmyTechnologyEffect[0].effectDuration -ne 'instant' -or
    $effectTemplateOverrides.Count -ne 1 -or
    $maximumArmyTechnologyEffectOverride.Count -ne 1 -or
    $maximumArmyTechnologyEffectOverride[0].instantEffect -ne 'NationMaxMiltechChange' -or
    [double]$maximumArmyTechnologyEffectOverride[0].value -ne 0.25 -or
    $maximumArmyTechnologyEffectOverride[0].effectTarget -ne 'AllNations' -or
    $maximumArmyTechnologyEffectOverride[0].effectDuration -ne 'instant' -or
    [int]$maximumArmyTechnologyEffectOverride[0].duration_months -ne -1) {
    throw 'Augmented Reality must cost 2,000 and use the native all-nations maximum Military technology effect, whose modded value must be +0.25.'
}
$expectedVanillaArmyTechnologyRecipients = @(
    'AppliedArtificialIntelligence',
    'Coilguns',
    'Cybernetics',
    'Diamondoids',
    'FutureTechMilitaryScience',
    'NetworkedGlobalDefense',
    'NextGenerationAerospace',
    'TerrestrialMilitaryScience',
    'TransInterfaceWarfare'
)
$vanillaArmyTechnologyRecipients = @($vanillaTechnologies | Where-Object {
    $_.effects -contains 'Effect_IncreaseMaxArmyTechLevel'
} | Select-Object -ExpandProperty dataName | Sort-Object)
if (($vanillaArmyTechnologyRecipients -join ';') -ne
    (($expectedVanillaArmyTechnologyRecipients | Sort-Object) -join ';')) {
    throw 'The installed finite and repeatable maximum Military technology recipients have changed from the pinned 1.0.51 baseline.'
}
$militaryTechnologyAdditions = [ordered]@{
    CarbonNanotubes = [pscustomobject]@{
        Cost = 3500
        VanillaEffects = ''
        ModEffects = 'Effect_IncreaseMaxArmyTechLevel'
    }
    Superalloys = [pscustomobject]@{
        Cost = 5000
        VanillaEffects = ''
        ModEffects = 'Effect_IncreaseMaxArmyTechLevel'
    }
    DeuteriumHelium3Fusion = [pscustomobject]@{
        Cost = 75000
        VanillaEffects = 'Effect_GlobalFusionTechIncrease;Effect_Economy_BasePCGDPIncrease02'
        ModEffects = 'Effect_GlobalFusionTechIncrease;Effect_Economy_BasePCGDPIncrease02;Effect_IncreaseMaxArmyTechLevel'
    }
}
foreach ($addition in $militaryTechnologyAdditions.GetEnumerator()) {
    $vanillaTechnology = @($vanillaTechnologies | Where-Object {
        $_.dataName -eq $addition.Key
    })
    $overrideTechnology = @($technologyCostOverrides | Where-Object {
        $_.dataName -eq $addition.Key
    })
    if ($vanillaTechnology.Count -ne 1 -or
        [double]$vanillaTechnology[0].researchCost -ne [double]$addition.Value.Cost -or
        ($vanillaTechnology[0].effects -join ';') -ne $addition.Value.VanillaEffects -or
        $overrideTechnology.Count -ne 1 -or
        ($overrideTechnology[0].effects -join ';') -ne $addition.Value.ModEffects) {
        throw "Military technology ceiling addition '$($addition.Key)' does not preserve its pinned cost and existing effects."
    }
}
$finiteArmyTechnologyRecipients = @(
    $expectedVanillaArmyTechnologyRecipients | Where-Object {
        $_ -ne 'FutureTechMilitaryScience'
    }
) + @('AugmentedReality') + @($militaryTechnologyAdditions.Keys)
if ($finiteArmyTechnologyRecipients.Count -ne 12 -or
    (5.0 + 0.25 * $finiteArmyTechnologyRecipients.Count) -ne 8.0) {
    throw 'The authored non-repeatable global technologies must produce a human maximum Military technology ceiling of exactly 8.00.'
}
$fusionMethodPrerequisites = @(
    'MagneticPlasmaConfinementTechniques',
    'ElectrostaticPlasmaConfinement',
    'InertialPlasmaConfinementTechniques',
    'Tokamaks',
    'ZPinchTechniques'
)
$vanillaDeuteriumTritiumFusion = @($vanillaTechnologies | Where-Object {
    $_.dataName -eq 'DeuteriumTritiumFusion'
})
$deuteriumTritiumFusionOverride = @($technologyCostOverrides | Where-Object {
    $_.dataName -eq 'DeuteriumTritiumFusion'
})
$vanillaFusionMethodologies = @($vanillaTechnologies | Where-Object {
    $_.dataName -eq 'NuclearFusioninSpace'
})
$fusionMethodologiesOverride = @($technologyCostOverrides | Where-Object {
    $_.dataName -eq 'NuclearFusioninSpace'
})
$vanillaDeuteriumDeuteriumFusion = @($vanillaTechnologies | Where-Object {
    $_.dataName -eq 'DeuteriumDeuteriumFusion'
})
$deuteriumDeuteriumFusionOverride = @($technologyCostOverrides | Where-Object {
    $_.dataName -eq 'DeuteriumDeuteriumFusion'
})
if ($vanillaDeuteriumTritiumFusion.Count -ne 1 -or
    [double]$vanillaDeuteriumTritiumFusion[0].researchCost -ne 50000 -or
    [bool]$vanillaDeuteriumTritiumFusion[0].AI_criticalTech -or
    ($vanillaDeuteriumTritiumFusion[0].prereqs -join ';') -ne
        'NuclearFusioninSpace;Neutronics' -or
    ($vanillaDeuteriumTritiumFusion[0].effects -join ';') -ne
        'Effect_GlobalFusionTechIncrease;Effect_Economy_BasePCGDPIncrease02' -or
    $deuteriumTritiumFusionOverride.Count -ne 1 -or
    -not [bool]$deuteriumTritiumFusionOverride[0].AI_criticalTech -or
    ($deuteriumTritiumFusionOverride[0].prereqs -join ';') -ne
        'AdvancedSuperconductors;NuclearFissioninSpace;AdvancedHeatManagementConcepts' -or
    $deuteriumTritiumFusionOverride[0].PSObject.Properties.Name -contains
        'researchCost' -or
    $deuteriumTritiumFusionOverride[0].PSObject.Properties.Name -contains
        'effects') {
    throw 'D-T Fusion must retain its installed 50,000 cost and effects, become AI-critical, and inherit the three former Fusion Methodologies prerequisites.'
}
if ($vanillaFusionMethodologies.Count -ne 1 -or
    [double]$vanillaFusionMethodologies[0].researchCost -ne 50000 -or
    ($vanillaFusionMethodologies[0].prereqs -join ';') -ne
        'AdvancedSuperconductors;NuclearFissioninSpace;AdvancedHeatManagementConcepts' -or
    ($vanillaFusionMethodologies[0].effects -join ';') -ne
        'Effect_GlobalFusionTechIncrease' -or
    $fusionMethodologiesOverride.Count -ne 1 -or
    ($fusionMethodologiesOverride[0].prereqs -join ';') -ne
        'DeuteriumTritiumFusion' -or
    $fusionMethodologiesOverride[0].PSObject.Properties.Name -contains
        'researchCost' -or
    $fusionMethodologiesOverride[0].PSObject.Properties.Name -contains
        'effects') {
    throw 'Nuclear Fusion Methodologies must retain its installed 50,000 cost and fusion effect and require only D-T Fusion.'
}
if ($vanillaDeuteriumDeuteriumFusion.Count -ne 1 -or
    [double]$vanillaDeuteriumDeuteriumFusion[0].researchCost -ne 75000 -or
    ($vanillaDeuteriumDeuteriumFusion[0].prereqs -join ';') -ne
        'DeuteriumTritiumFusion;Superalloys' -or
    ($vanillaDeuteriumDeuteriumFusion[0].effects -join ';') -ne
        'Effect_GlobalFusionTechIncrease;Effect_Economy_BasePCGDPIncrease02' -or
    $deuteriumDeuteriumFusionOverride.Count -ne 1 -or
    ($deuteriumDeuteriumFusionOverride[0].prereqs -join ';') -ne
        ((@($fusionMethodPrerequisites) + @('Superalloys')) -join ';') -or
    $deuteriumDeuteriumFusionOverride[0].PSObject.Properties.Name -contains
        'researchCost' -or
    $deuteriumDeuteriumFusionOverride[0].PSObject.Properties.Name -contains
        'effects') {
    throw 'D-D Fusion must retain its installed 75,000 cost and effects and require Superalloys plus all five approved fusion methods.'
}
$fusionDescriptionKey = 'TITechTemplate.description.NuclearFusioninSpace='
if ([regex]::Matches(
        $technologyLocalizationText,
        "(?m)^$([regex]::Escape($fusionDescriptionKey))").Count -ne 1 -or
    $technologyLocalizationText -notmatch
        'Practical deuterium-tritium fusion and tritium breeding' -or
    $technologyLocalizationText -notmatch
        'static electric fields, inertial ignition, toroidal tokamaks, and Z-pinch') {
    throw 'Nuclear Fusion Methodologies localization must describe D-T Fusion as its completed foundation and the downstream method families.'
}
$technologyPrerequisites = @{}
foreach ($technology in $vanillaTechnologies) {
    $technologyPrerequisites[$technology.dataName] = @(
        $technology.prereqs | Where-Object {
            -not [string]::IsNullOrWhiteSpace([string]$_)
        }
    )
}
foreach ($technology in $technologyCostOverrides) {
    if ($technology.PSObject.Properties.Name -contains 'prereqs') {
        $technologyPrerequisites[$technology.dataName] = @(
            $technology.prereqs | Where-Object {
                -not [string]::IsNullOrWhiteSpace([string]$_)
            }
        )
    }
}
$visitState = @{}
function Test-TechnologyPrerequisiteGraph {
    param(
        [string]$TechnologyId,
        [hashtable]$Prerequisites,
        [hashtable]$State
    )
    if ($State[$TechnologyId] -eq 1) {
        throw "Technology prerequisite graph contains a cycle at '$TechnologyId'."
    }
    if ($State[$TechnologyId] -eq 2) {
        return
    }
    $State[$TechnologyId] = 1
    foreach ($prerequisiteId in @(
            $Prerequisites[$TechnologyId] | Where-Object {
                -not [string]::IsNullOrWhiteSpace([string]$_)
            })) {
        if (-not $Prerequisites.ContainsKey($prerequisiteId)) {
            throw "Technology '$TechnologyId' references missing prerequisite '$prerequisiteId'."
        }
        Test-TechnologyPrerequisiteGraph `
            -TechnologyId $prerequisiteId `
            -Prerequisites $Prerequisites `
            -State $State
    }
    $State[$TechnologyId] = 2
}
foreach ($technologyId in @($technologyPrerequisites.Keys)) {
    Test-TechnologyPrerequisiteGraph `
        -TechnologyId $technologyId `
        -Prerequisites $technologyPrerequisites `
        -State $visitState
}
foreach ($entry in $mineEffectByTechnology.GetEnumerator()) {
    $vanillaTechnology = @($vanillaTechnologies | Where-Object {
        $_.dataName -eq $entry.Key
    })
    $overrideTechnology = @($technologyCostOverrides | Where-Object {
        $_.dataName -eq $entry.Key
    })
    $expectedEffects = @($vanillaTechnology[0].effects | Where-Object {
        $_ -ne $entry.Value
    })
    if ($vanillaTechnology.Count -ne 1 -or
        $overrideTechnology.Count -ne 1 -or
        ($overrideTechnology[0].effects -join ';') -ne
            ($expectedEffects -join ';') -or
        ($overrideTechnology[0].effects -join ';') -match
            'Effect_SpaceMineFreebies') {
        throw "Technology '$($entry.Key)' does not preserve every effect except its retired free-mine bonus."
    }
}
$vanillaProjects = Get-Content -LiteralPath (
    Join-Path $templatesDirectory 'TIProjectTemplate.json') -Raw | ConvertFrom-Json
$reusableRockets = @($vanillaProjects | Where-Object {
    $_.dataName -eq 'Project_ReusableRockets'
})
if ($reusableRockets.Count -ne 1 -or
    ($reusableRockets[0].prereqs -join ';') -ne
        'AdvancedChemicalRocketry') {
    throw 'Reusable Rockets must retain Advanced Chemical Rocketry as its sole installed prerequisite.'
}
$projectTemplateOverrides = @(
    Get-Content -LiteralPath $projectOverrides -Raw | ConvertFrom-Json)
$vanillaGoldRush = @($vanillaProjects | Where-Object {
    $_.dataName -eq 'Project_GoldRush'
})
$goldRushOverride = @($projectTemplateOverrides | Where-Object {
    $_.dataName -eq 'Project_GoldRush'
})
$expectedGoldRushEffects = @($vanillaGoldRush[0].effects | Where-Object {
    $_ -ne 'Effect_SpaceMineFreebies6_SingleFaction'
})
if ($projectTemplateOverrides.Count -ne 1 -or
    $vanillaGoldRush.Count -ne 1 -or
    $goldRushOverride.Count -ne 1 -or
    ($goldRushOverride[0].effects -join ';') -ne
        ($expectedGoldRushEffects -join ';')) {
    throw 'Gold Rush must preserve its mining bonus while removing only the free-mine effect.'
}
$mineUiText = (Get-Content -LiteralPath $generalControlsLocalization -Raw) +
    (Get-Content -LiteralPath $habUiLocalization -Raw)
if ($mineUiText -match 'allowed mines|free.mine|quadratic|network of mines and catapults' -or
    $mineUiText -notmatch 'Tier 1 costs 1' -or
    $mineUiText -notmatch 'equal to their tier') {
    throw 'Mine Mission Control localization must describe tier costs without a free-mine allowance.'
}
$globals = @(Get-Content -LiteralPath $globalOverrides -Raw | ConvertFrom-Json)
$globalConfig = @($globals | Where-Object { $_.dataName -eq 'globalConfig' })
if ($globalConfig.Count -ne 1 -or
    [int]$globalConfig[0].councilorMaxOrgs -ne 18 -or
    [double]$globalConfig[0].crewWaterConsumptionTons_year -ne 3 -or
    [double]$globalConfig[0].crewVolatilesConsumptionTons_year -ne 3) {
    throw 'Global configuration must set the councilor organization cap to 18 and preserve the 3-ton crew-resource overrides.'
}

$assemblyFile = Get-Item -LiteralPath $assemblyPath
if ($assemblyFile.LastWriteTime -lt $buildStarted.AddSeconds(-2)) {
    throw 'Packaged DLL predates this verification build.'
}
$assemblyVersion = [Reflection.AssemblyName]::GetAssemblyName($assemblyPath).Version.ToString()
if ($assemblyVersion -ne '0.9.3.0') {
    throw "Assembly version '$assemblyVersion' does not match release 0.9.3."
}
$assemblyHash = (Get-FileHash -LiteralPath $assemblyPath -Algorithm SHA256).Hash

$requiredFiles = @(
    $manifestPath,
    $assemblyPath,
    $weights,
    $defaultSettings,
    $missionOverrides,
    $startOverrides,
    $armyOverrides,
    $metaOverrides,
    $technologyOverrides,
    $effectOverrides,
    $projectOverrides,
    $globalOverrides,
    $habModuleOverrides,
    $habOverrides,
    $powerPlantOverrides,
    $driveOverrides,
    $heatSinkOverrides,
    $gunOverrides,
    $laserWeaponOverrides,
    $magneticGunOverrides,
    $shipHullOverrides,
    $utilityModuleOverrides,
    $nationLocalization,
    $effectLocalization,
    $technologyLocalization,
    $scienceLocalization,
    $generalControlsLocalization,
    $habUiLocalization,
    (Join-Path $repositoryRoot 'docs\current-implementation-matrix.xlsx')
)
foreach ($requiredFile in $requiredFiles) {
    if (-not (Test-Path -LiteralPath $requiredFile)) {
        throw "Required release input is missing: $requiredFile"
    }
}

$artifactDirectory = Join-Path $repositoryRoot 'artifacts'
if (-not (Test-Path -LiteralPath $artifactDirectory)) {
    New-Item -ItemType Directory -Path $artifactDirectory | Out-Null
}
$stagingDirectory = Join-Path $artifactDirectory 'TIEconomyMod'
if (Test-Path -LiteralPath $stagingDirectory) {
    $resolvedArtifacts = (Resolve-Path -LiteralPath $artifactDirectory).Path
    $resolvedStaging = (Resolve-Path -LiteralPath $stagingDirectory).Path
    if (-not $resolvedStaging.StartsWith($resolvedArtifacts + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Refusing to replace a staging directory outside artifacts.'
    }
    Remove-Item -LiteralPath $resolvedStaging -Recurse
}
New-Item -ItemType Directory -Path (Join-Path $stagingDirectory 'Assembly') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stagingDirectory 'Config') -Force | Out-Null
Copy-Item -LiteralPath $manifestPath -Destination $stagingDirectory
Copy-Item -LiteralPath $assemblyPath -Destination (Join-Path $stagingDirectory 'Assembly')
Copy-Item -LiteralPath $weights -Destination (Join-Path $stagingDirectory 'Config')
Copy-Item -LiteralPath $hullVolumes -Destination (Join-Path $stagingDirectory 'Config')
Copy-Item -LiteralPath $hullDriveScales -Destination (Join-Path $stagingDirectory 'Config')
Copy-Item -LiteralPath $defaultSettings -Destination $stagingDirectory
Copy-Item -LiteralPath $missionOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $startOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $armyOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $metaOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $technologyOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $effectOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $projectOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $globalOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $habModuleOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $habOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $powerPlantOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $driveOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $heatSinkOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $gunOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $laserWeaponOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $magneticGunOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $shipHullOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $utilityModuleOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $nationLocalization -Destination $stagingDirectory
Copy-Item -LiteralPath $effectLocalization -Destination $stagingDirectory
Copy-Item -LiteralPath $technologyLocalization -Destination $stagingDirectory
Copy-Item -LiteralPath $scienceLocalization -Destination $stagingDirectory
Copy-Item -LiteralPath $generalControlsLocalization -Destination $stagingDirectory
Copy-Item -LiteralPath $habUiLocalization -Destination $stagingDirectory
Copy-Item -LiteralPath $habModuleLocalization -Destination $stagingDirectory
Copy-Item -LiteralPath $projectLocalization -Destination $stagingDirectory
Copy-Item -LiteralPath $operationLocalization -Destination $stagingDirectory
Copy-Item -LiteralPath $codexLocalization -Destination $stagingDirectory
$imagePath = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\Economic Equalization Overhaul.png'
if (Test-Path -LiteralPath $imagePath) {
    Copy-Item -LiteralPath $imagePath -Destination $stagingDirectory
}

$zipPath = Join-Path $artifactDirectory 'TIEconomyMod-0.9.3-ti1.0.51.zip'
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath
}
Compress-Archive -LiteralPath $stagingDirectory -DestinationPath $zipPath -CompressionLevel Optimal

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $packagedDll = $archive.Entries |
        Where-Object { $_.FullName.Replace('\', '/') -eq 'TIEconomyMod/Assembly/TIEconomyMod.dll' } |
        Select-Object -First 1
    if ($null -eq $packagedDll) {
        throw 'Release archive does not contain Assembly/TIEconomyMod.dll.'
    }
    $packagedSettings = $archive.Entries |
        Where-Object { $_.FullName.Replace('\', '/') -eq 'TIEconomyMod/Settings.xml' } |
        Select-Object -First 1
    $packagedWeights = $archive.Entries |
        Where-Object { $_.FullName.Replace('\', '/') -eq 'TIEconomyMod/Config/economy-tech-weights.csv' } |
        Select-Object -First 1
    $packagedHullVolumes = $archive.Entries |
        Where-Object { $_.FullName.Replace('\', '/') -eq 'TIEconomyMod/Config/hull-variant-main-volumes.csv' } |
        Select-Object -First 1
    $packagedHullDriveScales = $archive.Entries |
        Where-Object { $_.FullName.Replace('\', '/') -eq 'TIEconomyMod/Config/hull-variant-drive-scales.csv' } |
        Select-Object -First 1
    $packagedEffectLocalization = $archive.Entries |
        Where-Object { $_.FullName.Replace('\', '/') -eq 'TIEconomyMod/TIEffectTemplate.en' } |
        Select-Object -First 1
    $packagedTechnologyLocalization = $archive.Entries |
        Where-Object { $_.FullName.Replace('\', '/') -eq 'TIEconomyMod/TITechTemplate.en' } |
        Select-Object -First 1
    $packagedEffectOverrides = $archive.Entries |
        Where-Object { $_.FullName.Replace('\', '/') -eq 'TIEconomyMod/TIEffectTemplate.json' } |
        Select-Object -First 1
    $packagedStartingForceFiles = @(
        'TIEconomyMod/TIArmyTemplate.json',
        'TIEconomyMod/TIMetaTemplate.json'
    )
    foreach ($packagedStartingForceFile in $packagedStartingForceFiles) {
        if ($null -eq ($archive.Entries |
            Where-Object { $_.FullName.Replace('\', '/') -eq $packagedStartingForceFile } |
            Select-Object -First 1)) {
            throw "Release archive is missing $packagedStartingForceFile."
        }
    }
    $packagedShipFiles = @(
        'TIEconomyMod/TIPowerPlantTemplate.json',
        'TIEconomyMod/TIDriveTemplate.json',
        'TIEconomyMod/TIHeatSinkTemplate.json',
        'TIEconomyMod/TIGunTemplate.json',
        'TIEconomyMod/TILaserWeaponTemplate.json',
        'TIEconomyMod/TIMagneticGunTemplate.json',
        'TIEconomyMod/TIShipHullTemplate.json',
        'TIEconomyMod/TIUtilityModuleTemplate.json'
    )
    foreach ($packagedShipFile in $packagedShipFiles) {
        if ($null -eq ($archive.Entries |
            Where-Object { $_.FullName.Replace('\', '/') -eq $packagedShipFile } |
            Select-Object -First 1)) {
            throw "Release archive is missing $packagedShipFile."
        }
    }
    foreach ($packagedLogisticsLocalization in @(
        'TIEconomyMod/TIHabModuleTemplate.en',
        'TIEconomyMod/TIProjectTemplate.en',
        'TIEconomyMod/TIOperationTemplate.en',
        'TIEconomyMod/UICodex.en')) {
        if ($null -eq ($archive.Entries |
            Where-Object { $_.FullName.Replace('\', '/') -eq $packagedLogisticsLocalization } |
            Select-Object -First 1)) {
            throw "Release archive is missing $packagedLogisticsLocalization."
        }
    }
    foreach ($packagedMineMcFile in @(
        'TIEconomyMod/TIProjectTemplate.json',
        'TIEconomyMod/UIGeneralControls.en',
        'TIEconomyMod/UIHabs.en')) {
        if ($null -eq ($archive.Entries |
            Where-Object { $_.FullName.Replace('\', '/') -eq $packagedMineMcFile } |
            Select-Object -First 1)) {
            throw "Release archive is missing $packagedMineMcFile."
        }
    }
    if ($null -eq $packagedSettings -or $null -eq $packagedWeights -or
        $null -eq $packagedHullVolumes -or $null -eq $packagedHullDriveScales -or
        $null -eq $packagedEffectLocalization -or $null -eq $packagedTechnologyLocalization -or
        $null -eq $packagedEffectOverrides) {
        throw 'Release archive is missing settings, measured hull volumes/drive scales, technology weights, or control-point localization.'
    }
    $stream = $packagedDll.Open()
    $sha256 = [Security.Cryptography.SHA256]::Create()
    try {
        $packagedHash = ([BitConverter]::ToString($sha256.ComputeHash($stream))).Replace('-', '')
    }
    finally {
        $sha256.Dispose()
        $stream.Dispose()
    }
}
finally {
    $archive.Dispose()
}
if ($packagedHash -ne $assemblyHash) {
    throw 'Packaged DLL does not match the newly built binary.'
}

Write-Host "PASS: release verification completed."
Write-Host "DLL SHA256: $assemblyHash"
Write-Host "Artifact: $zipPath"
Write-Host 'Compatibility target: TI 1.0.51 installed assemblies and guarded IL patch points.'
