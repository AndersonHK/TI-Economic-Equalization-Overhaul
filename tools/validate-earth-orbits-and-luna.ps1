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
$modFiles = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles'
$docs = Join-Path $RepositoryRoot 'docs\orbits-and-lunar-resources'

function Read-Json([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Missing required orbit/Luna file: $Path"
    }
    $parsed = Get-Content -LiteralPath $Path -Raw -Encoding UTF8 |
        ConvertFrom-Json
    foreach ($item in $parsed) {
        Write-Output $item
    }
}

function Assert-Close(
    [double]$Expected,
    [double]$Actual,
    [string]$Label,
    [double]$Tolerance = 0.0000001) {
    if ([Math]::Abs($Expected - $Actual) -gt $Tolerance) {
        throw "$Label is $Actual; expected $Expected."
    }
}

$orbits = @(Read-Json (Join-Path $modFiles 'TIOrbitTemplate.json'))
$expectedOrbits = [ordered]@{
    LowEarthOrbitPlus20 = 20
    LowEarthOrbitPlus40 = 40
    LowEarthOrbitMinus20 = -20
    LowEarthOrbitMinus40 = -40
}
if ($orbits.Count -ne 4) {
    throw "Expected four added Earth orbit templates, found $($orbits.Count)."
}
foreach ($entry in $expectedOrbits.GetEnumerator()) {
    $orbit = @($orbits | Where-Object dataName -eq $entry.Key)
    if ($orbit.Count -ne 1) {
        throw "Expected one orbit template '$($entry.Key)'."
    }
    foreach ($field in @(
        @{ Name = 'altitude_km'; Value = 500 },
        @{ Name = 'semiMajorAxisRange_km'; Value = 100 },
        @{ Name = 'inclinationRange_Deg'; Value = 20 },
        @{ Name = 'stationCapacity'; Value = 8 })) {
        Assert-Close $field.Value $orbit[0].($field.Name) `
            "$($entry.Key).$($field.Name)"
    }
    Assert-Close $entry.Value $orbit[0].inclination_Deg `
        "$($entry.Key).inclination_Deg"
    if (-not $orbit[0].interfaceOrbit -or -not $orbit[0].earthLEO) {
        throw "$($entry.Key) must be an Earth LEO interface orbit."
    }
}

$bodies = @(Read-Json (Join-Path $modFiles 'TISpaceBodyTemplate.json'))
$earth = @($bodies | Where-Object dataName -eq 'Earth')
$luna = @($bodies | Where-Object dataName -eq 'Luna')
if ($earth.Count -ne 1 -or $luna.Count -ne 1) {
    throw 'Earth and Luna must each have one space-body array override.'
}
$earthOrbitNames = @($earth[0].orbits)
foreach ($name in $expectedOrbits.Keys) {
    if ($name -notin $earthOrbitNames) {
        throw "Earth does not instantiate '$name'."
    }
}
foreach ($removed in @('LowEarthOrbit3', 'LowEarthOrbit4')) {
    if ($removed -in $earthOrbitNames) {
        throw "Earth still instantiates bespoke orbit '$removed'."
    }
}

$modInfo = Read-Json (Join-Path $modFiles 'ModInfo.json')
if ('TISpaceBodyTemplate.json' -notin @($modInfo.TemplatesToReplaceArrays)) {
    throw 'ModInfo does not replace TISpaceBodyTemplate arrays.'
}

$habs = @(Read-Json (Join-Path $modFiles 'TIHabTemplate.json'))
foreach ($habName in @(
    'InternationalSpaceStation',
    'InternationalSpaceStationSkirmish',
    'Tiangong')) {
    $hab = @($habs | Where-Object dataName -eq $habName)
    if ($hab.Count -ne 1 -or
        $hab[0].orbitTemplateName -ne 'LowEarthOrbitPlus40') {
        throw "$habName is not assigned to LowEarthOrbitPlus40."
    }
}

$sites = @(Read-Json (Join-Path $modFiles 'TIHabSiteTemplate.json'))
$profiles = @(Read-Json (
    Join-Path $modFiles 'TIMiningProfileTemplate.json'))
$approved = @(Import-Csv -LiteralPath (
    Join-Path $docs 'lunar-site-yield-proposal.csv'))
$expectedSiteCount = 35
if ($sites.Count -ne $expectedSiteCount -or
    $profiles.Count -ne $expectedSiteCount -or
    $approved.Count -ne $expectedSiteCount -or
    @($luna[0].habSites).Count -ne $expectedSiteCount) {
    throw "Luna must have exactly $expectedSiteCount sites, profiles, approved rows, and body references."
}
if (@($sites.dataName | Sort-Object -Unique).Count -ne $expectedSiteCount -or
    @($profiles.dataName | Sort-Object -Unique).Count -ne $expectedSiteCount -or
    @($sites | ForEach-Object { "$($_.X),$($_.Y)" } |
        Sort-Object -Unique).Count -ne $expectedSiteCount -or
    @($sites | ForEach-Object { "$($_.latitude),$($_.longitude)" } |
        Sort-Object -Unique).Count -ne $expectedSiteCount) {
    throw 'Luna site IDs, profiles, grid positions, and coordinates must be unique.'
}

$siteLoc = [IO.File]::ReadAllText((
    Join-Path $modFiles 'TIHabSiteTemplate.en'))
$profileLoc = [IO.File]::ReadAllText((
    Join-Path $modFiles 'TIMiningProfileTemplate.en'))
$resources = @('water', 'volatiles', 'metals', 'nobles', 'fissiles')
for ($index = 0; $index -lt $expectedSiteCount; $index++) {
    $site = $sites[$index]
    $profile = @($profiles | Where-Object dataName -eq `
        $site.miningProfileName)
    if ($profile.Count -ne 1) {
        throw "$($site.dataName) does not resolve one mining profile."
    }
    if ($site.dataName -notin @($luna[0].habSites)) {
        throw "$($site.dataName) is not referenced by Luna."
    }
    if (-not $siteLoc.Contains(
            "TIHabSiteTemplate.displayName.$($site.dataName)=")) {
        throw "$($site.dataName) is missing English localization."
    }
    if (-not $profileLoc.Contains(
            "TIMiningProfileTemplate.displayName.$($profile[0].dataName)=Lunar")) {
        throw "$($profile[0].dataName) is missing the English display-name localization used by the planetoid Type column."
    }
    $descriptionPrefix =
        "TIMiningProfileTemplate.description.$($profile[0].dataName)="
    $descriptionLines = @($profileLoc -split '\r?\n' | Where-Object {
        $_.StartsWith($descriptionPrefix, [StringComparison]::Ordinal)
    })
    if ($descriptionLines.Count -ne 1) {
        throw "$($profile[0].dataName) must have exactly one English description."
    }
    $description = $descriptionLines[0].Substring($descriptionPrefix.Length)
    if ([String]::IsNullOrWhiteSpace($description) -or
        $description.Length -gt 40 -or
        $description.Contains('.') -or
        $description.Contains('Resource outputs')) {
        throw "$($profile[0].dataName) description must be a concise Mining Profile label: '$description'."
    }
    if ($site.friendlyName -ne $approved[$index].site) {
        throw "Site $($index + 1) does not match the approved roster order."
    }
    if ($site.latitude -lt -90 -or $site.latitude -gt 90 -or
        $site.longitude -lt -180 -or $site.longitude -gt 180) {
        throw "$($site.dataName) has invalid coordinates."
    }

    foreach ($resource in $resources) {
        $low = [double]$approved[$index].("${resource}_low")
        $high = [double]$approved[$index].("${resource}_high")
        $mean = [double]$profile[0].("${resource}_mean")
        $width = [double]$profile[0].("${resource}_width")
        $minimum = [double]$profile[0].("${resource}_min")
        $jump = [double]$profile[0].("${resource}_jump")
        Assert-Close (($low + $high) / 2.0) $mean `
            "$($site.dataName) $resource mean"
        Assert-Close ($high - $low) $width `
            "$($site.dataName) $resource width"
        Assert-Close 0 $minimum "$($site.dataName) $resource minimum"
        Assert-Close 0 $jump "$($site.dataName) $resource jump"
        if ($low -eq 0 -and $high -eq 0 -and
            ($mean -ne 0 -or $width -ne 0 -or
             $minimum -ne 0 -or $jump -ne 0)) {
            throw "$($site.dataName) absent $resource is not all-zero."
        }
    }
}

for ($left = 0; $left -lt $sites.Count; $left++) {
    for ($right = $left + 1; $right -lt $sites.Count; $right++) {
        $latitudeLeft = [double]$sites[$left].latitude * [Math]::PI / 180.0
        $latitudeRight = [double]$sites[$right].latitude * [Math]::PI / 180.0
        $longitudeDelta = ([double]$sites[$right].longitude -
            [double]$sites[$left].longitude) * [Math]::PI / 180.0
        $cosine = [Math]::Sin($latitudeLeft) * [Math]::Sin($latitudeRight) +
            [Math]::Cos($latitudeLeft) * [Math]::Cos($latitudeRight) *
            [Math]::Cos($longitudeDelta)
        $centralAngle = [Math]::Acos(
            [Math]::Max(-1.0, [Math]::Min(1.0, $cosine)))
        $distanceKm = 1738.1 * $centralAngle
        if ($distanceKm -lt 200.0) {
            throw ("{0} and {1} are only {2:N1} km apart; Luna hab sites " +
                "must be separated by at least 200 km to prevent marker overlap.") -f
                $sites[$left].dataName, $sites[$right].dataName, $distanceKm
        }
        if ([Math]::Abs($sites[$left].latitude -
                $sites[$right].latitude) -lt 10 -and
            [Math]::Abs($sites[$left].longitude -
                $sites[$right].longitude) -lt 10) {
            throw "$($sites[$left].dataName) and $($sites[$right].dataName) would trigger the game's automatic coordinate displacement."
        }
    }
}

$controls = [IO.File]::ReadAllText((
    Join-Path $modFiles 'UIGeneralControls.en'))
foreach ($text in @(
    'supplies both hydrogen and oxygen through electrolysis',
    'Oxygen bound into silicates and oxides is not included')) {
    if (-not $controls.Contains($text)) {
        throw "Resource semantics localization is missing '$text'."
    }
}

$snapshot = @(Import-Csv -LiteralPath (
    Join-Path $docs 'vanilla-luna-mars-profile-snapshot.csv'))
if (@($snapshot | Where-Object body -eq 'Luna' |
        Measure-Object -Property site_count -Sum).Sum -ne 9 -or
    @($snapshot | Where-Object body -eq 'Mars' |
        Measure-Object -Property site_count -Sum).Sum -ne 25) {
    throw 'Vanilla Luna/Mars comparison snapshot has changed unexpectedly.'
}

function Load-AssemblyBytes([string]$Path) {
    return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
}

$harmonyAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($assemblyName in @(
    'Assembly-CSharp-firstpass.dll',
    'FMODUnity.dll',
    'Unity.Burst.dll',
    'Unity.Collections.dll',
    'Unity.Jobs.dll',
    'Unity.Mathematics.dll',
    'Unity.Entities.dll',
    'UnityEngine.CoreModule.dll',
    'UnityEngine.dll',
    'UnityEngine.IMGUIModule.dll',
    'UnityEngine.PhysicsModule.dll',
    'UnityEngine.UI.dll',
    'UnityEngine.UIModule.dll',
    'Unity.TextMeshPro.dll',
    'Newtonsoft.Json.dll')) {
    $path = Join-Path $TargetManagedDir $assemblyName
    if (Test-Path -LiteralPath $path) {
        [void](Load-AssemblyBytes $path)
    }
}
[void](Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$gameAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
$modAssembly = Load-AssemblyBytes $ModAssemblyPath
$spaceObjectType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TISpaceObjectState', $true)
$factionType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TIFactionState', $true)
$gameStateType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TIGameState', $true)
$moduleType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TIHabModuleState', $true)
$shipType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TISpaceShipState', $true)
$plannerType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.MasterTransferPlanner', $true)
$planePatchName = 'TIEconomyMod.Patches.InclinedOrbitTransferPatch'
$solverPatchName = 'TIEconomyMod.Patches.FiniteThrustPlaneChangePatch'
$contracts = @(
    @{
        Type = $planePatchName
        Method = 'Postfix'
        HarmonyArgument = 2
        Target = $plannerType.GetMethod('GetTrajectoryModelsForConditions')
    },
    @{
        Type = $solverPatchName
        Method = 'Prefix'
        HarmonyArgument = 1
        Target = $plannerType.GetMethod('CalculateInclinationChangeTransfers',
            [Reflection.BindingFlags]'NonPublic,Static')
    },
    @{
        Type = 'TIEconomyMod.Patches.GenericEarthLaunchCostPatch'
        Method = 'Prefix'
        HarmonyArgument = 1
        Target = $spaceObjectType.GetMethod(
            'GenericTransferBoostFromEarthSurface',
            [Reflection.BindingFlags]'Public,Static',
            $null,
            [Type[]]@($factionType, $gameStateType, [float]),
            $null)
    },
    @{
        Type = 'TIEconomyMod.Patches.HabCrewEarthLaunchCostPatch'
        Method = 'Postfix'
        HarmonyArgument = 2
        Target = $moduleType.GetMethod('DecommissionModuleCost')
    },
    @{
        Type = 'TIEconomyMod.Patches.ShipCrewEarthLaunchCostPatch'
        Method = 'Postfix'
        HarmonyArgument = 2
        Target = $shipType.GetMethod('ScuttleCost')
    })
$harmonyType = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true)
$harmonyMethodType = $harmonyAssembly.GetType('HarmonyLib.HarmonyMethod', $true)
$patchApi = @($harmonyType.GetMethods(
    [Reflection.BindingFlags]'Public,Instance') | Where-Object {
        $_.Name -eq 'Patch' -and $_.GetParameters().Count -eq 5
    })
if ($patchApi.Count -ne 1) {
    throw "Expected one five-argument Harmony.Patch API, found $($patchApi.Count)."
}
$harmony = [Activator]::CreateInstance(
    $harmonyType,
    [object[]]@('ti-eeo.earth-launch-validation.' +
        [Guid]::NewGuid().ToString('N')))
foreach ($contract in $contracts) {
    if ($null -eq $contract.Target) {
        throw "Installed launch-cost target for $($contract.Type) was not found."
    }
    $patchType = $modAssembly.GetType($contract.Type, $true)
    $method = $patchType.GetMethod(
        $contract.Method,
        [Reflection.BindingFlags]'NonPublic,Static')
    if ($null -eq $method) {
        throw "Missing launch-cost patch $($contract.Type).$($contract.Method)."
    }
    $harmonyPatch = [Activator]::CreateInstance(
        $harmonyMethodType,
        [object[]]@($method))
    $arguments = [object[]]::new(5)
    $arguments[0] = $contract.Target
    $arguments[$contract.HarmonyArgument] = $harmonyPatch
    try {
        $replacement = $patchApi[0].PSObject.BaseObject.Invoke(
            $harmony,
            $arguments)
    }
    catch {
        if ($_.Exception.InnerException) {
            throw $_.Exception.InnerException
        }
        throw
    }
    if ($null -eq $replacement) {
        throw "Harmony did not emit $($contract.Type)."
    }
}

$nativeNamespace = 'PavonisInteractive.TerraInvicta.'
$fields = [Reflection.BindingFlags]'Instance,NonPublic'
$orbitType = $gameAssembly.GetType($nativeNamespace + 'TIOrbitState', $true)
$orbitTemplateType = $gameAssembly.GetType('TIOrbitTemplate', $true)
$bodyType = $gameAssembly.GetType($nativeNamespace + 'TISpaceBodyState', $true)
$assetType = $gameAssembly.GetType($nativeNamespace + 'TISpaceAssetState', $true)
$fleetType = $gameAssembly.GetType($nativeNamespace + 'TISpaceFleetState', $true)
$modelType = $gameAssembly.GetType($nativeNamespace + 'TrajectoryModel', $true)
$modelListType = [System.Collections.Generic.List`1].MakeGenericType($modelType)
$body = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($bodyType)
$fleet = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($fleetType)
$orbitField = $assetType.GetField('<orbitState>k__BackingField', $fields)
$patch = $modAssembly.GetType($planePatchName, $true).GetMethod(
    'Postfix', [Reflection.BindingFlags]'Static,NonPublic')

function New-TestOrbit([int]$Id, [double]$Inclination, [double]$Axis_m, [double]$Node = 310) {
    $orbit = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($orbitType)
    $template = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($orbitTemplateType)
    $orbit.ID = [Activator]::CreateInstance($orbit.ID.GetType(), [object[]]@($Id))
    $orbit.barycenter = $body
    $orbit.semiMajorAxis_m = $Axis_m
    $orbit.semiMajorAxis_km = $Axis_m / 1000
    $orbit.exists = $true
    $orbitTemplateType.GetField('eccentricity').SetValue($template, 0.0)
    $orbitTemplateType.GetField('inclination_Deg').SetValue($template, $Inclination)
    $orbitTemplateType.GetField('longAscendingNode_Deg').SetValue($template, $Node)
    $orbitTemplateType.GetField('argPeriapsis_Deg').SetValue($template, 300.0)
    $gameStateType.GetField('template', $fields).SetValue($orbit, $template)
    return $orbit
}

function Assert-TransferModels($Origin, $Destination, [string[]]$Before, [string[]]$Expected) {
    $orbitField.SetValue($fleet, $Origin)
    $models = [Activator]::CreateInstance($modelListType)
    foreach ($name in $Before) {
        $models.Add([Enum]::Parse($modelType, $name))
    }
    [void]$patch.Invoke($null, [object[]]@($fleet, $Destination, $models))
    if (($models -join ',') -cne ($Expected -join ',')) {
        throw "Unexpected transfer models from orbit $($Origin.ID) to $($Destination.ID). Expected $Expected, found $models."
    }
}

$axis_m = 6856751.107
$leo = New-TestOrbit 1 0 $axis_m
$inclined = @(
    (New-TestOrbit 2 20 $axis_m),
    (New-TestOrbit 3 40 $axis_m),
    (New-TestOrbit 4 -20 $axis_m),
    (New-TestOrbit 5 -40 $axis_m)
)
foreach ($band in $inclined) {
    Assert-TransferModels $leo $band OrbitPhasing InclinationChange
    Assert-TransferModels $band $leo OrbitPhasing InclinationChange
}
Assert-TransferModels $inclined[1] $inclined[3] OrbitPhasing InclinationChange
Assert-TransferModels $leo $inclined[1] @('OrbitPhasing', 'ImpulseMicrothrustHybrid') InclinationChange
Assert-TransferModels $leo $leo OrbitPhasing OrbitPhasing
Assert-TransferModels $leo (New-TestOrbit 6 0 $axis_m) OrbitPhasing OrbitPhasing
Assert-TransferModels $leo (New-TestOrbit 7 40 ($axis_m + 500000)) ImpulseMicrothrustHybrid ImpulseMicrothrustHybrid
Assert-TransferModels $leo (New-TestOrbit 8 40 $axis_m 270) OrbitPhasing OrbitPhasing

$habType = $gameAssembly.GetType($nativeNamespace + 'TIHabState', $true)
$station = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($habType)
$habTypeField = $habType.GetField('<habType>k__BackingField', $fields)
$habTypeField.SetValue($station, [Enum]::Parse($habTypeField.FieldType, 'Station'))
$orbitField.SetValue($station, $inclined[1])
Assert-TransferModels $leo $station OrbitPhasing InclinationChange
Assert-TransferModels $inclined[1] $station OrbitPhasing OrbitPhasing

$targetFleet = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($fleetType)
$orbitField.SetValue($targetFleet, $inclined[1])
Assert-TransferModels $leo $targetFleet OrbitPhasing InclinationChange
$trajectoryType = $gameAssembly.GetType($nativeNamespace + 'Trajectory_Patched', $true)
$fleetType.GetField('<trajectory>k__BackingField', $fields).SetValue(
    $targetFleet, [Runtime.Serialization.FormatterServices]::GetUninitializedObject($trajectoryType))
Assert-TransferModels $leo $targetFleet OrbitPhasing OrbitPhasing
Write-Host 'PASS: equal-altitude orbit, station, and idle-fleet plane-change dispatch and native-route controls.'

$uiType = $gameAssembly.GetType($nativeNamespace + 'ThrustProfileTool', $true)
$messagePatch = $modAssembly.GetType('TIEconomyMod.Patches.TransferDepartureWindowMessagePatch', $true)
$prefix = $messagePatch.GetMethod('Prefix', [Reflection.BindingFlags]'Static,NonPublic')
$patchTarget = @($messagePatch.GetCustomAttributesData() | Where-Object {
    $_.AttributeType.FullName -eq 'HarmonyLib.HarmonyPatch' -and
    $_.ConstructorArguments.Count -eq 2 -and
    $_.ConstructorArguments[0].Value -eq $uiType -and
    $_.ConstructorArguments[1].Value -eq 'UpdateUI'
})
if ($patchTarget.Count -ne 1 -or $null -eq $uiType.GetMethod('UpdateUI') -or
    $null -eq $uiType.GetMethod('DisplayFailureReport') -or
    $prefix.GetParameters()[0].ParameterType -ne $uiType) {
    throw 'The transfer departure-window message must bind to the native thrust profile UI.'
}
$messageCheck = $messagePatch.GetMethod(
    'HasContradictoryAccelerationMinimum', [Reflection.BindingFlags]'Static,NonPublic')
$resultType = $gameAssembly.GetType($nativeNamespace + 'TransferResult', $true)
$outcomeType = $resultType.GetNestedType('Outcome')
$missedWindow = [Activator]::CreateInstance($resultType, [object[]]@(
    [Enum]::Parse($outcomeType, 'Fail_LaunchInPast'), -2565.793, 10780.384))
$realMinimum = [Activator]::CreateInstance($resultType, [object[]]@(
    [Enum]::Parse($outcomeType, 'Fail_BurnLongerThanHalfOrbit'), 8000.0, 6000.0))
if (!$messageCheck.Invoke($null, [object[]]@($missedWindow, 0.2660899)) -or
    $messageCheck.Invoke($null, [object[]]@($realMinimum, 0.2660899))) {
    throw 'The transfer UI must report a missed departure without suppressing a burn-length rejection.'
}
Write-Host 'PASS: the thrust profile distinguishes a missed departure from an actual acceleration requirement.'

. (Join-Path $PSScriptRoot 'validation\Test-FiniteThrustPlaneChange.ps1')

foreach ($typeName in @(
    'TIEconomyMod.Core.EarthLaunchCostMath',
    'TIEconomyMod.EarthLaunchCost')) {
    if ($null -eq $modAssembly.GetType($typeName, $false)) {
        throw "Missing centralized launch-cost type '$typeName'."
    }
}

Write-Host "PASS: Earth orbits, launch-cost authority, station migration, and $expectedSiteCount Luna sites validated."
