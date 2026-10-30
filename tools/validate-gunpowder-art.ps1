[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$TargetManagedDir,
    [Parameter(Mandatory = $true)][string]$ModAssemblyPath,
    [Parameter(Mandatory = $true)][string]$RepositoryRoot
)
$ErrorActionPreference = 'Stop'
function Load-Bytes([string]$Path) { return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path)) }
$harmonyAssembly = Load-Bytes (Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($file in Get-ChildItem -LiteralPath $TargetManagedDir -Filter '*.dll' | Where-Object {
    $_.Name -like 'Unity*.dll' -or $_.Name -like 'Newtonsoft*.dll' -or $_.Name -eq 'FMODUnity.dll'
}) { [void](Load-Bytes $file.FullName) }
[void](Load-Bytes (Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$gamePath = Join-Path $TargetManagedDir 'Assembly-CSharp.dll'
$game = Load-Bytes $gamePath
$mod = Load-Bytes $ModAssemblyPath
$bundlePath = Join-Path (Split-Path -Parent $TargetManagedDir) 'StreamingAssets\AssetBundles\shipweapons'
$profilePath = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\Config\gunpowder-art-profiles.eeo'
$catalogType = $mod.GetType('TIEconomyMod.Core.GunpowderArtCatalog', $true)
$snapshotType = $mod.GetType('TIEconomyMod.Core.GunpowderScaleSnapshot', $true)
$load = $catalogType.GetMethod('Load')
$catalog = $load.Invoke($null, [object[]]@([string]$profilePath, [string]$gamePath, [string]$bundlePath))
if ($catalog.Count -ne 8) { throw 'Expected eight gunpowder art profiles.' }
# Exercise the actual native discovery predicate, not just our private reader.
# The game uses Contains(".json"), so .json.txt would also be unsafe.
$flags = [Reflection.BindingFlags]'Static,Instance,Public,NonPublic'
$managerType = $game.GetType('PavonisInteractive.TerraInvicta.Modding.ModManager',$true)
$predicates = @($managerType.GetNestedTypes([Reflection.BindingFlags]'Public,NonPublic') | ForEach-Object {
    $_.GetMethods($flags) | Where-Object {
        $_.Name -like '*LoadJsonMods*' -and $_.ReturnType -eq [bool] -and
        $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType -eq [string]
    }
})
if ($predicates.Count -ne 1) { throw 'Native mod JSON discovery predicate changed; inspect it before deployment.' }
$predicate = $predicates[0]
$predicateOwner = if ($predicate.IsStatic) { $null } else { [Activator]::CreateInstance($predicate.DeclaringType,$true) }
function Native-Discovers([string]$Path) {
    return [bool]$predicate.Invoke($predicateOwner,[object[]]@([string]$Path))
}
if (-not (Native-Discovers 'Mods/Enabled/EEO/Config/gunpowder-art-profiles.json') -or
    -not (Native-Discovers 'Mods/Enabled/EEO/Config/gunpowder-art-profiles.json.backup') -or
    (Native-Discovers 'Mods/Enabled/EEO/Config/gunpowder-art-profiles.eeo') -or
    (Native-Discovers 'Mods/Enabled/EEO/ModInfo.json')) { throw 'Unexpected native JSON discovery behavior.' }
$newtonsoftAssembly = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'Newtonsoft.Json' } | Select-Object -First 1
$parseArray = $newtonsoftAssembly.GetType('Newtonsoft.Json.Linq.JArray',$true).GetMethod('Parse',[type[]]@([string]))
$nativeJsonCount=0
foreach ($file in Get-ChildItem -LiteralPath (Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles') -File -Recurse) {
    $relative = $file.FullName.Substring((Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles').Length).Replace('\','/')
    if (Native-Discovers $relative) {
        if ($relative.Contains('/Config/')) { throw "Private config will enter the native template loader: $relative" }
        [void]$parseArray.Invoke($null,[object[]]@([string][IO.File]::ReadAllText($file.FullName)))
        $nativeJsonCount++
    }
}
$json = Get-Content -LiteralPath $profilePath -Raw
$doc = $json | ConvertFrom-Json
$research = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'docs\ship-balance-research\canonical-turret-projection\all-weapon-targets.json') -Raw | ConvertFrom-Json
$guns = Get-Content -LiteralPath (Join-Path (Split-Path -Parent $TargetManagedDir) 'StreamingAssets\Templates\TIGunTemplate.json') -Raw | ConvertFrom-Json
$overrides = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\TIGunTemplate.json') -Raw | ConvertFrom-Json
$vectorType = $catalogType.GetMethod('Vector').ReturnType
$ctor = $vectorType.GetConstructor([type[]]@([single],[single],[single]))
function Vec([single]$X, [single]$Y, [single]$Z) { return $ctor.Invoke([object[]]@($X,$Y,$Z)) }
$near = $catalogType.GetMethod('Near')
$originalBase = Vec .31 .42 .53
$originalChild = Vec .7 .8 .9
$state = [Activator]::CreateInstance($snapshotType)
$checks = 0
$geometry = $mod.GetType('TIEconomyMod.Core.GunpowderArtGeometry',$true)
$scalePoint = $geometry.GetMethod('ScalePoint')
$geometryChecks = 0
$materialGuard = $catalogType.GetMethod('MaterialLayoutMatches')
foreach ($case in @(@(1,1,1,1,$true),@(1,1,1,2,$false),@(1,1,2,1,$false),@(2,1,1,1,$false),@(1,2,1,1,$false),@(1,1,0,1,$false))) {
    $actual = $materialGuard.Invoke($null,[object[]]@([int]$case[0],[int]$case[1],[int]$case[2],[int]$case[3]))
    if ($actual -ne [bool]$case[4]) { throw 'Gunpowder material layout guard failed.' }
}
$weaponVis = $game.GetType('PavonisInteractive.TerraInvicta.ShipWeaponVisController',$true)
foreach ($name in @('_originalBaseMaterial','_originalWeaponMaterial')) {
    $field = $weaponVis.GetField($name,[Reflection.BindingFlags]'Instance,NonPublic')
    if ($null -eq $field -or $field.FieldType.FullName -ne 'UnityEngine.Material') { throw 'Native repair-material cache layout changed.' }
}
foreach ($row in $doc.profiles) {
    $argsForGet = [object[]]@([string]$row.weapon, $null)
    if (-not $catalogType.GetMethod('TryGet').Invoke($catalog, $argsForGet)) { throw 'Profile lookup failed.' }
    $p = $argsForGet[1]
    # Compare original root-scale sizing against the actual compiled baking
    # operation at eight source bounds corners plus the fire point, under aim.
    $points = @()
    foreach ($sx in @(-1,1)) { foreach ($sy in @(-1,1)) { foreach ($sz in @(-1,1)) {
        $points += ,@(($row.movingMesh.boundsCenter[0]+$sx*$row.movingMesh.boundsSize[0]/2),
            ($row.movingMesh.boundsCenter[1]+$sy*$row.movingMesh.boundsSize[1]/2),
            ($row.movingMesh.boundsCenter[2]+$sz*$row.movingMesh.boundsSize[2]/2))
    } } }
    $points += ,@($row.firePointPosition[0],$row.firePointPosition[1],$row.firePointPosition[2])
    $pivot = Vec $row.childPosition[0] $row.childPosition[1] $row.childPosition[2]
    $bakedPivot = $scalePoint.Invoke($null,[object[]]@($pivot,[single]$p.Factor))
    foreach ($point in $points) {
        $v = Vec $point[0] $point[1] $point[2]
        $bv = $scalePoint.Invoke($null,[object[]]@($v,[single]$p.Factor))
        $a=@(($v.x*$row.childScale[0]),($v.y*$row.childScale[1]),($v.z*$row.childScale[2]))
        $b=@(($bv.x*$row.childScale[0]),($bv.y*$row.childScale[1]),($bv.z*$row.childScale[2]))
        foreach ($angle in @(0,.37,1.57079632679,-.85)) {
            $co=[Math]::Cos($angle); $si=[Math]::Sin($angle)
            $ra=@($a[0],($co*$a[1]-$si*$a[2]),($si*$a[1]+$co*$a[2]))
            $rb=@($b[0],($co*$b[1]-$si*$b[2]),($si*$b[1]+$co*$b[2]))
            $pp=@($pivot.x,$pivot.y,$pivot.z); $bp=@($bakedPivot.x,$bakedPivot.y,$bakedPivot.z)
            $oldScale=@($p.FinalBaseScale.x,$p.FinalBaseScale.y,$p.FinalBaseScale.z)
            for ($axis=0;$axis -lt 3;$axis++) {
                $oldPoint=$oldScale[$axis]*($pp[$axis]+$ra[$axis])
                $newPoint=$row.rootScale[$axis]*($bp[$axis]+$rb[$axis])
                if ([Math]::Abs($oldPoint-$newPoint) -gt .00001) { throw 'Baked art changed canonical world geometry or muzzle position.' }
                $geometryChecks++
            }
        }
    }
    $r = @($research | Where-Object name -eq $row.weapon)[0]
    $g = @($guns | Where-Object dataName -eq $row.weapon)[0]
    $override = @($overrides | Where-Object dataName -eq $row.weapon)[0]
    if ($p.Resource -ne $g.modelResource -or $p.Resource -ne $r.resource -or
        [Math]::Abs($p.CaliberMm - $override.projectileDiameter_mm) -gt 0.0001 -or
        [Math]::Abs($p.Factor - $r.factor) -gt 0.000001 -or
        [Math]::Abs($p.SourceInscribedMm * $p.Factor - $p.CaliberMm) -gt 0.0001) { throw "Calibration mismatch: $($row.weapon)" }
    # Reuse a preview controller with non-resource scale, then swap caliber.
    # Absolute targets must repeat without compounding, and restore the original.
    for ($cycle=0; $cycle -lt 12; ++$cycle) {
        $state.Capture($originalBase,$originalChild,$p.FinalBaseScale,$p.FinalChildScale)
        if (-not $state.Release($p.FinalBaseScale,$p.FinalChildScale)) { throw 'Expected native-scale restoration.' }
        if (-not $near.Invoke($null,@($state.NativeBase,$originalBase)) -or
            -not $near.Invoke($null,@($state.NativeChild,$originalChild)) -or $state.Applied) { throw 'Restore drift.' }
        if ($state.Release($p.FinalBaseScale,$p.FinalChildScale)) { throw 'Double restore must do nothing.' }
        $checks += 3
    }
    $state.Capture($originalBase,$originalChild,$p.FinalBaseScale,$p.FinalChildScale)
    if ($state.Release((Vec 9 9 9),$p.FinalChildScale) -or $state.Applied) { throw 'Must not overwrite a foreign scale edit.' }
    $checks++
}
foreach ($name in @('LightCoilgunBatteryMk1','PointDefenseLaserTurret','25mmAutocannon','Unknown')) {
    if ($catalogType.GetMethod('TryGet').Invoke($catalog,[object[]]@($name,$null))) { throw 'Non-gunpowder profile leaked.' }
    $checks++
}
# Every gameplay/localization file has identical bytes to the pre-art baseline.
$baseline = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'docs\ship-balance-research\gunpowder-art\gameplay-data-baseline.json') -Raw | ConvertFrom-Json
foreach ($property in $baseline.PSObject.Properties) {
    if ($property.Name -eq 'TIEconomyMod\ModFiles\Settings.xml') {
        # Preserve the original byte/hash evidence; compare all prior settings
        # semantically after removing only the two newly approved scale fields.
        $priorPath = Join-Path $RepositoryRoot 'docs\ship-balance-research\battle-scale\settings-pre-configurable-scale-2026-10-30.xml'
        if ((Get-FileHash $priorPath).Hash -ne $property.Value) { throw 'Historical settings snapshot no longer matches the art baseline.' }
        $prior = New-Object Xml.XmlDocument
        $prior.Load($priorPath)
        $current = New-Object Xml.XmlDocument
        $current.Load((Join-Path $RepositoryRoot $property.Name))
        foreach ($field in @('shipScaleMultiplier','projectileScaleMultiplier')) {
            $node = $current.SelectSingleNode("/Settings/shipBalance/$field")
            if ($null -eq $node -or $node.InnerText -ne '10') { throw "Expected 10x trial setting: $field" }
            [void]$node.ParentNode.RemoveChild($node)
        }
        if ($prior.OuterXml -ne $current.OuterXml) { throw 'Unrelated settings changed during the configurable scale pass.' }
        continue
    }
    # 0.10.9 deliberately changes only the two camera minima in this file.
    # The historical byte baseline remains intact; verify its other fields.
    if ($property.Name -eq 'TIEconomyMod\ModFiles\TIGlobalConfig.json') {
        $global = Get-Content -LiteralPath (Join-Path $RepositoryRoot $property.Name) -Raw | ConvertFrom-Json
        $expected = @{ dataName='globalConfig'; controlPointMaintenanceFreebies=0; councilorMaxOrgs=18;
            combatCamera_minCameraMovementSpeed=0.25; combatCamera_minScrollSpeedOffset=-10;
            crewWaterConsumptionTons_year=2; crewVolatilesConsumptionTons_year=2;
            probePayloadBaseline_tons=0.325; probePayloadPerHabSite_tons=0 }
        if (@($global).Count -ne 1 -or @($global[0].PSObject.Properties).Count -ne $expected.Count) { throw 'Unexpected global override fields.' }
        foreach ($key in $expected.Keys) {
            if ($global[0].$key -ne $expected[$key]) { throw "Unexpected global override value: $key" }
        }
        continue
    }
    if ((Get-FileHash -LiteralPath (Join-Path $RepositoryRoot $property.Name) -Algorithm SHA256).Hash -ne $property.Value) {
        throw "Gameplay data changed during the art pass: $($property.Name)"
    }
}
$temp = Join-Path $RepositoryRoot ('.tmp\gunpowder-art\schema-' + [Guid]::NewGuid().ToString('N') + '.json')
$utf8 = New-Object Text.UTF8Encoding($false)
function Reject([scriptblock]$Mutate) {
    $copy = $json | ConvertFrom-Json
    & $Mutate $copy
    [IO.File]::WriteAllText($temp,($copy | ConvertTo-Json -Depth 20),$utf8)
    $rejected=$false
    try { [void]$load.Invoke($null,[object[]]@([string]$temp,[string]$gamePath,[string]$bundlePath)) } catch { $rejected=$true }
    if (-not $rejected) { throw 'Invalid art schema accepted.' }
}
try {
    Reject { param($x) $x.schema=99 }
    Reject { param($x) $x.profiles[0].sourceInscribedMm=0 }
    Reject { param($x) $x.profiles[0].caliberMm=-1 }
    Reject { param($x) $x.profiles[0].rootScale[0]=0 }
    Reject { param($x) $x.profiles[0].firePointPosition=@(1,2) }
    Reject { param($x) $x.profiles[0].weapon='LightCoilgunBatteryMk1' }
    Reject { param($x) $x.profiles[0].weapon=$x.profiles[1].weapon }
    Reject { param($x) $x.profiles[0].baseMesh.vertices=0 }
    Reject { param($x) $x.profiles[0].PSObject.Properties.Remove('childScale') }
    Reject { param($x) $x | Add-Member -NotePropertyName typo -NotePropertyValue 1 }
    Reject { param($x) $x.weaponBundleSha256=('0'*64) }
    Reject { param($x) $x.profiles=@($x.profiles[0]) }
    $newtonsoft = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'Newtonsoft.Json' } | Select-Object -First 1
    $convert = $newtonsoft.GetType('Newtonsoft.Json.JsonConvert',$true)
    $typed = $convert.GetMethod('DeserializeObject',[type[]]@([string],[type])).Invoke($null,[object[]]@([string]$json,$mod.GetType('TIEconomyMod.Core.GunpowderArtDocument',$true)))
    $roundTrip = $convert.GetMethod('SerializeObject',[type[]]@([object])).Invoke($null,@($typed))
    [IO.File]::WriteAllText($temp,$roundTrip,$utf8)
    if ($load.Invoke($null,[object[]]@([string]$temp,[string]$gamePath,[string]$bundlePath)).Count -ne 8) { throw 'Art schema round trip failed.' }
} finally { if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp } }
$harmony = [Activator]::CreateInstance($harmonyAssembly.GetType('HarmonyLib.Harmony'),@('eeo.gunpowder.art.validation'))
$unityOnly=$false
try {
    foreach ($patchName in @('GunpowderArtSetWeaponPatch','GunpowderArtRepairMaterialPatch')) {
        try { [void]$harmony.CreateClassProcessor($mod.GetType(('TIEconomyMod.Patches.'+$patchName),$true)).Patch() }
        catch {
            $cause=$_.Exception.GetBaseException()
            if ($cause -is [Security.SecurityException] -and $cause.Message -like 'ECall methods*') { $unityOnly=$true }
            else { throw }
        }
    }
} finally { $harmony.UnpatchAll('eeo.gunpowder.art.validation') }
Write-Host "PASS: gunpowder art schema 1, 8 calibrated profiles, 12 invalid-schema guards, typed round trip, $checks managed lifecycle assertions and unchanged gameplay data."
Write-Host "PASS: source assembly/bundle hashes match; SetWeapon patch resolves (Unity-only binding: $unityOnly). Actual scene hierarchy, mesh fingerprints, muzzle placement and visual appearance require the in-game check."
Write-Host "PASS: actual native recursive JSON predicate excludes private .eeo profiles; reproduced rejection boundary for .json and .json.backup; $nativeJsonCount discovered template files have array roots."
Write-Host 'PASS: six material-layout guard cases, both native repair-cache fields and both gunpowder patch targets validate. Material binding/renderer probes require Unity-host confirmation.'
Write-Host "PASS: $geometryChecks baked-versus-transform coordinate comparisons preserve canonical bounds and muzzle positions under aiming."
