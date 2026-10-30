[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$TargetManagedDir,
    [Parameter(Mandatory=$true)][string]$ModAssemblyPath,
    [Parameter(Mandatory=$true)][string]$RepositoryRoot
)
$ErrorActionPreference = 'Stop'
function Load-Bytes([string]$Path) { [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path)) }
$harmony = Load-Bytes (Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($file in Get-ChildItem $TargetManagedDir -Filter '*.dll' | Where-Object {
    $_.Name -like 'Unity*.dll' -or $_.Name -like 'Newtonsoft*.dll' -or $_.Name -eq 'FMODUnity.dll' -or
    $_.Name -eq 'ShapesRuntime.dll' -or $_.Name -like 'Vectrosity*.dll'
}) { [void](Load-Bytes $file.FullName) }
[void](Load-Bytes (Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$game = Load-Bytes (Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
$mod = Load-Bytes $ModAssemblyPath
$flags = [Reflection.BindingFlags]'Public,NonPublic,Static,Instance'
$targets = @(
    @('PavonisInteractive.TerraInvicta.SpaceCombatManager','get_modelScalingFactor'),
    @('PavonisInteractive.TerraInvicta.SpaceCombatManager','get_projectileScalingFactor'),
    @('PavonisInteractive.TerraInvicta.SpaceCombatManager','OnEnable'),
    @('PavonisInteractive.TerraInvicta.SpaceCombat.CombatShipController','get_BoundingBoxSize'),
    @('PavonisInteractive.TerraInvicta.SpaceCombat.CombatShipController','Initialize'),
    @('PavonisInteractive.TerraInvicta.SpaceCombat.CombatShipController','ReturnToStrategyLayerFleet'),
    @('PavonisInteractive.TerraInvicta.SpaceCombat.CombatHabModuleController','InitializeForCombat'),
    @('PavonisInteractive.TerraInvicta.SpaceCombatManager','ReturnHabToStrategyLayer'),
    @('PavonisInteractive.TerraInvicta.SpaceCombatManager','ConfigureHabCollisionObjectsForCombat'),
    @('PavonisInteractive.TerraInvicta.SpaceCombat.MissileController','Fire'),
    @('PavonisInteractive.TerraInvicta.SpaceCombat.ProjectileController','Destruct'),
    @('PavonisInteractive.TerraInvicta.TISpaceCombatProjectileState','WillHitSphere')
)
foreach ($entry in $targets) {
    $type = $game.GetType($entry[0],$true)
    if ($null -eq $type.GetMethod($entry[1],$flags)) { throw "Missing native battle geometry target: $entry" }
}
$reader = @($harmony.GetType('HarmonyLib.PatchProcessor',$true).GetMethods() | Where-Object {
    $_.Name -eq 'GetOriginalInstructions' -and $_.GetParameters().Count -eq 2 -and
    -not $_.GetParameters()[1].ParameterType.IsByRef
})[0]
function Assert-NoEarlyAssetCacheReference([Reflection.MethodBase]$Method) {
    $body = @($reader.Invoke($null,[object[]]@($Method,$null)))
    foreach ($instruction in $body) {
        $member = $instruction.operand -as [Reflection.MemberInfo]
        if ($null -ne $member -and $null -ne $member.DeclaringType -and
            $member.DeclaringType.FullName -eq 'PavonisInteractive.TerraInvicta.AssetCacheManager') {
            throw "Unsafe early Harmony target references AssetCacheManager: $($Method.DeclaringType.FullName).$($Method.Name)"
        }
    }
}
# This is the actual failed 0.10.11 target. Detect it by metadata/IL only;
# never read its static fields and never poison the game's cache in the test.
$oldStationTarget = $game.GetType('PavonisInteractive.TerraInvicta.SpaceCombat.CombatHabModuleController',$true).GetMethod('DestroyHabModule',$flags)
$unsafeRejected = $false
try { Assert-NoEarlyAssetCacheReference $oldStationTarget } catch { $unsafeRejected = $true }
if (-not $unsafeRejected) { throw 'The known 0.10.11 cache-initializer regression was not detected.' }
$stationPatch = $mod.GetType('TIEconomyMod.Patches.BattleStationExplosionGeometryPatch',$true)
$metadata = @($stationPatch.GetCustomAttributes($harmony.GetType('HarmonyLib.HarmonyPatch'),$false))
if ($metadata.Count -ne 1 -or $metadata[0].info.methodName -ne 'InitializeForCombat' -or
    $metadata[0].info.declaringType.FullName -ne 'PavonisInteractive.TerraInvicta.SpaceCombat.CombatHabModuleController') {
    throw 'Station effects must attach after combat initialization, not by patching cache-backed destruction.'
}
$missile = $game.GetType('PavonisInteractive.TerraInvicta.SpaceCombat.MissileController',$true)
$method = $missile.GetMethod('HasHitTarget',$flags)
$instructions = $reader.Invoke($null,[object[]]@($method,$null))
$patch = $mod.GetType('TIEconomyMod.Patches.BattleMissileSweepPatch',$true)
$patched = @($patch.GetMethod('Transpiler').Invoke($null,[object[]]@(,$instructions)))
foreach ($name in @('Padding','Sweep')) {
    $calls = @($patched | Where-Object { $_.operand -is [Reflection.MethodInfo] -and $_.operand.Name -eq $name -and $_.operand.DeclaringType -eq $patch })
    if ($calls.Count -ne 1) { throw "Missile sweep must replace exactly one $name constant." }
}
# Synthetic negative case: native layout drift must be rejected.
$listType = [System.Collections.Generic.List``1].MakeGenericType($harmony.GetType('HarmonyLib.CodeInstruction',$true))
$empty = [Activator]::CreateInstance($listType)
$rejected = $false
try { $null = @($patch.GetMethod('Transpiler').Invoke($null,[object[]]@(,$empty))) } catch { $rejected = $true }
if (-not $rejected) { throw 'Missile sweep accepted an unsupported empty IL layout.' }
$math = $mod.GetType('TIEconomyMod.Core.BattleScaleMath',$true)
foreach ($multiplier in @(1,10,20,50,1000)) {
    $ship = [double]$math.GetMethod('ModelScale').Invoke($null,[object[]]@([single]$multiplier))
    $projectile = [double]$math.GetMethod('ProjectileScale').Invoke($null,[object[]]@([single]$multiplier))
    $expected = 0.00005*$multiplier
    if ([Math]::Abs($ship-$expected) -gt 0.00000001 -or [Math]::Abs($projectile-$expected) -gt 0.00000001) {
        throw 'Combat settings multiplier does not convert metres consistently.'
    }
}
$config = (Get-Content (Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\TIGlobalConfig.json') -Raw | ConvertFrom-Json)[0]
if ($config.combatCamera_minCameraMovementSpeed -ne .25 -or $config.combatCamera_minScrollSpeedOffset -ne -10) { throw 'Camera minima must be exactly one twentieth of native.' }
if (@($config.PSObject.Properties.Name | Where-Object { $_ -like 'combatCamera_*' }).Count -ne 2) { throw 'Only the two approved camera minima may be overridden.' }
$projectile = $game.GetType('PavonisInteractive.TerraInvicta.SpaceCombat.ProjectileController',$true)
if ($projectile.GetField('projectileParticleSystem',$flags).FieldType.FullName -ne 'UnityEngine.ParticleSystem') { throw 'Projectile particle binding changed.' }
foreach ($name in @('CombatMeshGeometry','BattleProjectileGeometry')) {
    if ($null -eq $mod.GetType("TIEconomyMod.Core.$name",$false)) { throw "Missing runtime geometry implementation: $name" }
}
# Regression coverage for the manually observed 0.10.9 rendering/zoom failures.
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.IO.Compression
$catalogPath = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\Config\combat-unreadable-meshes.eeo'
$fallback = $mod.GetType('TIEconomyMod.Core.CombatUnreadableMeshes',$true)
$records = $fallback.GetMethod('Load').Invoke($null,[object[]]@([string]$catalogPath,[string](Join-Path $TargetManagedDir 'Assembly-CSharp.dll')))
$payloads = $fallback.GetMethod('VerifyPayloads').Invoke($null,@())
if ($records -ne 503 -or $payloads -lt 400) { throw 'Expected complete base/DLC unreadable mesh coverage.' }
$manifest = Get-Content (Join-Path $RepositoryRoot 'docs\ship-balance-research\battle-scale\mesh-fallback-sources-2026-10-29.json') -Raw | ConvertFrom-Json
if ((Get-FileHash $catalogPath).Hash -ne $manifest.sha256) { throw 'Fallback index differs from the extracted source record.' }
foreach ($property in $manifest.payload_files.PSObject.Properties) {
    if ((Get-FileHash (Join-Path $RepositoryRoot $property.Name)).Hash -ne $property.Value) { throw 'Fallback payload hash mismatch.' }
}
$gameRoot = Split-Path -Parent (Split-Path -Parent $TargetManagedDir)
foreach ($property in $manifest.sources.PSObject.Properties) {
    if ((Get-FileHash (Join-Path $gameRoot $property.Name)).Hash -ne $property.Value) { throw "Fallback source bundle changed: $($property.Name)" }
}
$viewMath = $mod.GetType('TIEconomyMod.Core.CombatViewMath',$true)
if ([Math]::Abs($viewMath.GetMethod('RelativeModelScale').Invoke($null,[object[]]@($false,[single]10))-.5) -gt .000001 -or
    [Math]::Abs($viewMath.GetMethod('RelativeModelScale').Invoke($null,[object[]]@($true,[single]10))-.05) -gt .000001) { throw 'Ship-relative UI must follow the configured multiplier.' }
if ([Math]::Abs($viewMath.GetMethod('RelativeProjectileScale').Invoke($null,[object[]]@([single]10))-.2) -gt .000001) { throw 'Projectile tactical range must follow the configured multiplier.' }
$unity = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'UnityEngine.CoreModule' } | Select-Object -First 1
$vector = $unity.GetType('UnityEngine.Vector3',$true)
function Vec([single]$X,[single]$Y,[single]$Z) { [Activator]::CreateInstance($vector,[object[]]@($X,$Y,$Z)) }
$dolly = $viewMath.GetMethod('SafeDollyPoint')
$opticsType = $mod.GetType('TIEconomyMod.Core.LaserOpticsCatalog',$true)
$opticsPath = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\Config\laser-beam-optics.eeo'
$bundlePath = Join-Path $gameRoot 'TerraInvicta_Data\StreamingAssets\AssetBundles\shipweapons'
$optics = $opticsType.GetMethod('Load').Invoke($null,[object[]]@([string]$opticsPath,[string](Join-Path $TargetManagedDir 'Assembly-CSharp.dll'),[string]$bundlePath))
if ($optics.Count -ne 16) { throw 'Expected sixteen measured laser optics resources.' }
$dimensions = Get-Content (Join-Path $RepositoryRoot 'docs\ship-balance-research\barrel-lens-audit\dimensions.json') -Raw | ConvertFrom-Json
$lensChecks = 0
foreach ($entry in $dimensions | Where-Object { $null -ne $_.aperture_m }) {
    $lookupArgs = [object[]]@([string]$entry.resource,$null)
    if (-not $opticsType.GetMethod('TryGet').Invoke($optics,$lookupArgs)) { throw 'Measured aperture resource is missing.' }
    $opticsProfile = $lookupArgs[1]
    foreach ($modelScale in @([single]0.00005,[single]0.0005,[single]0.001)) {
        foreach ($bake in @([single]1,[single]0.05,[single]0.1,[single]0.5)) {
            if ($modelScale -le 0 -or $bake -le 0) { throw 'Invalid positive aperture test inputs.' }
            foreach ($mirror in @([single]1,[single]-1)) {
                $size = Vec ($entry.moving_dimensions_m[0]*$bake) ($entry.moving_dimensions_m[1]*$bake) ($entry.moving_dimensions_m[2]*$bake)
                $scale = Vec ($modelScale/$bake*$mirror) ($modelScale/$bake) ($modelScale/$bake)
                $actual = $opticsProfile.GetType().GetMethod('WorldDiameter').Invoke($opticsProfile,[object[]]@($size,$scale))
                if ([Math]::Abs($actual-$entry.aperture_m*$modelScale) -gt .00000001) { throw 'Beam diameter diverges from visible aperture after scale/bake/mirroring.' }
                $lensChecks++
            }
        }
    }
}
$laserTemplates = Get-Content (Join-Path $gameRoot 'TerraInvicta_Data\StreamingAssets\Templates\TILaserWeaponTemplate.json') -Raw | ConvertFrom-Json
foreach ($laser in $laserTemplates | Where-Object { $_.modelResource }) {
    if (-not $opticsType.GetMethod('TryGet').Invoke($optics,[object[]]@([string]$laser.modelResource,$null))) { throw "Uncovered laser model resource: $($laser.modelResource)" }
}
$badOptics = Get-Content $opticsPath -Raw | ConvertFrom-Json
$badOptics.profiles[0].xRatio = 0
$badPath = Join-Path $RepositoryRoot '.tmp\laser-beam-scale\invalid-optics.eeo'
[IO.File]::WriteAllText($badPath,($badOptics|ConvertTo-Json -Depth 12),(New-Object Text.UTF8Encoding($false)))
$rejected = $false
try { $null = $opticsType.GetMethod('Load').Invoke($null,[object[]]@([string]$badPath,[string](Join-Path $TargetManagedDir 'Assembly-CSharp.dll'),[string]$bundlePath)) } catch { $rejected = $true }
if (-not $rejected) { throw 'Invalid zero aperture ratio accepted.' }
$origin = Vec 0 0 0
foreach ($case in @(
    @(@(0,0,10),@(0,0,-20),@(0,0,2)),
    @(@(10,0,0),@(-20,0,0),@(2,0,0)),
    @(@(0,10,0),@(0,-20,0),@(0,2,0)),
    @(@(0,0,10),@(0,0,5),@(0,0,5)),
    @(@(0,0,1),@(0,0,0),@(0,0,1)),
    @(@(0,0,1),@(0,0,3),@(0,0,3)),
    @(@(5,0,10),@(5,0,-20),@(5,0,-20))
)) {
    $a = Vec $case[0][0] $case[0][1] $case[0][2]
    $b = Vec $case[1][0] $case[1][1] $case[1][2]
    $actual = $dolly.Invoke($null,[object[]]@($a,$b,$origin,[single]2))
    if ([Math]::Abs($actual.x-$case[2][0]) -gt .001 -or [Math]::Abs($actual.y-$case[2][1]) -gt .001 -or [Math]::Abs($actual.z-$case[2][2]) -gt .001) { throw 'Camera dolly crossing/retreat guard failed.' }
}
$viewTargets = @(
    @('PavonisInteractive.TerraInvicta.SpaceCombat.SpaceCombatCameraController','HandleZoom','BattleDollyStopPatch',1),
    @('PavonisInteractive.TerraInvicta.SpaceCombat.WaypointVisual','UpdateWaypointScale','BattleWaypointScalePatch',4),
    @('PavonisInteractive.TerraInvicta.SpaceCombatManager','RenderLines','BattlePathThicknessPatch',1),
    @('PavonisInteractive.TerraInvicta.CombatGrid','Update','BattleGridThicknessPatch',1),
    @('PavonisInteractive.TerraInvicta.SpaceCombat.CombatShipController','ToggleExplosions','BattleExplosionJitterPatch',6),
    @('PavonisInteractive.TerraInvicta.SpaceCombat.SpaceCombatCameraController','InitializeCamera','BattleInitialCameraScalePatch',6)
)
foreach ($entry in $viewTargets) {
    $target = $game.GetType($entry[0],$true).GetMethod($entry[1],$flags)
    $original = $reader.Invoke($null,[object[]]@($target,$null))
    $patchType = $mod.GetType(('TIEconomyMod.Patches.'+$entry[2]),$true)
    $result = @($patchType.GetMethod('Transpiler').Invoke($null,[object[]]@(,$original)))
    $helpers = @($result | Where-Object { $_.operand -is [Reflection.MethodInfo] -and $_.operand.DeclaringType.FullName -eq 'TIEconomyMod.Patches.CombatViewRuntime' })
    if ($helpers.Count -ne $entry[3]) { throw "Wrong guarded rewrite count: $($entry[2])" }
    $negative = $false
    try { $null = @($patchType.GetMethod('Transpiler').Invoke($null,[object[]]@(,$empty))) } catch { $negative = $true }
    if (-not $negative) { throw "Unsupported IL accepted by $($entry[2])" }
}
$binder = [Activator]::CreateInstance($harmony.GetType('HarmonyLib.Harmony'),@('eeo.battle.geometry.validation'))
$unityOnly = 0
$main = $mod.GetType('TIEconomyMod.Main',$true)
$oldEnabled = $main.GetField('enabled').GetValue($null)
$oldSettings = $main.GetField('settings').GetValue($null)
$manager = $game.GetType('PavonisInteractive.TerraInvicta.SpaceCombatManager',$true)
$cachedScale = $manager.GetField('_cachedScalingAdjustmentFactor',$flags)
$oldScale = $cachedScale.GetValue($null)
$globalValues = $game.GetType('PavonisInteractive.TerraInvicta.TIGlobalValuesState',$true).GetField('isSpaceCombatEnabled',$flags)
$oldCombat = $globalValues.GetValue($null)
try {
    foreach ($name in @('BattleModelScalePatch','BattleProjectileScalePatch','BattleGridScalePatch',
        'BattleShipBoundsPatch','BattleShipGeometryPatch','BattleShipGeometryRestorePatch',
        'BattleHabGeometryPatch','BattleHabGeometryRestorePatch','BattleHabFormationGeometryPatch','BattleMissileGeometryPatch',
        'BattleProjectileHidePatch','BattleThreatGeometryPatch','BattleMissileSweepPatch','ProjectileColliderSizingPatch',
        'BattleHabRenderBakePatch','BattleClosestZoomPatch','BattleCameraClipPatch','BattleCameraClipRestorePatch',
        'BattleDollyStopPatch','BattleWaypointScalePatch','BattlePathThicknessPatch','BattleProjectileTacticalPatch',
        'BattleGridThicknessPatch','BattleFormationSpacingPatch','BattleShipExplosionGeometryPatch','BattleExplosionJitterPatch',
        'BattleRadiatorExplosionGeometryPatch','BattleWeaponExplosionGeometryPatch','BattleStationExplosionGeometryPatch','BattleParticleGroupExplosionPatch',
        'BattleLaserEmitterPatch','BattleLaserRibbonPatch','BattleLaserRibbonRestorePatch','BattleInitialCameraScalePatch','BattleScaleSettingsCapturePatch',
        'BattleCloseMovementScalePatch','BattleCloseScrollScalePatch',
        'BallisticCompletedStepRegistrationPatch','BallisticCompletedStepPhasePatch',
        'BallisticCompletedStepResolvePatch','ProjectileRegisteredGeometryFilterPatch')) {
        $patchType = $mod.GetType("TIEconomyMod.Patches.$name",$true)
        foreach ($attribute in $patchType.GetCustomAttributes($harmony.GetType('HarmonyLib.HarmonyPatch'),$false)) {
            if ($null -ne $attribute.info.declaringType -and $attribute.info.methodName) {
                $nativeTarget = $attribute.info.declaringType.GetMethod($attribute.info.methodName,$flags)
                if ($null -eq $nativeTarget) { throw "Missing early battle patch target: $name" }
                Assert-NoEarlyAssetCacheReference $nativeTarget
            }
        }
        try { [void]$binder.CreateClassProcessor($patchType).Patch() }
        catch {
            $cause = $_.Exception.GetBaseException()
            if ($cause -is [Security.SecurityException] -and $cause.Message -like 'ECall methods*') { $unityOnly++ }
            else { throw }
        }
    }
    $main.GetField('settings').SetValue($null,[Activator]::CreateInstance($main.GetField('settings').FieldType))
    $main.GetField('enabled').SetValue($null,$true)
    $settingsType = $main.GetField('settings').FieldType
    $cfg = $main.GetField('settings').GetValue($null)
    $shipCfg = $settingsType.GetField('shipBalance').GetValue($cfg)
    $shipField = $shipCfg.GetType().GetField('shipScaleMultiplier')
    $projectileField = $shipCfg.GetType().GetField('projectileScaleMultiplier')
    if ($shipField.GetValue($shipCfg) -ne 10 -or $projectileField.GetValue($shipCfg) -ne 10) { throw 'Fresh settings must default to 10x/10x.' }
    $xmlType = [Xml.Serialization.XmlSerializer]
    $serializer = New-Object Xml.Serialization.XmlSerializer($settingsType)
    $oldXmlPath = Join-Path $RepositoryRoot 'docs\ship-balance-research\battle-scale\settings-pre-configurable-scale-2026-10-30.xml'
    $xmlReader = New-Object IO.StringReader([IO.File]::ReadAllText($oldXmlPath))
    try { $migrated = $serializer.Deserialize($xmlReader) } finally { $xmlReader.Dispose() }
    $migratedShip = $settingsType.GetField('shipBalance').GetValue($migrated)
    if ($shipField.GetValue($migratedShip) -ne 10 -or $projectileField.GetValue($migratedShip) -ne 10) { throw 'Older XML must receive constructor scale defaults.' }
    [xml]$defaults = Get-Content (Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\Settings.xml') -Raw
    if ($defaults.Settings.shipBalance.shipScaleMultiplier -ne '10' -or $defaults.Settings.shipBalance.projectileScaleMultiplier -ne '10') { throw 'Packaged trial settings must be 10x/10x.' }
    $scaleRuntime = $mod.GetType('TIEconomyMod.Patches.BattleScaleRuntime',$true)
    foreach ($mode in @([single]0.1,[single]1)) {
        $cachedScale.SetValue($null,$mode)
        $scaleRuntime.GetMethod('Release').Invoke($null,@())
        $shipField.SetValue($shipCfg,[single]10); $projectileField.SetValue($shipCfg,[single]10)
        $scaleRuntime.GetMethod('Capture').Invoke($null,@())
        $shipField.SetValue($shipCfg,[single]20); $projectileField.SetValue($shipCfg,[single]5)
        $scaleRuntime.GetMethod('Capture').Invoke($null,@())
        foreach ($name in @('ModelScale','ProjectileScale')) {
            if ([Math]::Abs($scaleRuntime.GetProperty($name).GetValue($null,$null)-.0005) -gt .00000001) { throw 'In-battle setting edit changed captured geometry.' }
        }
        $closeMoveArgs = [object[]]@([single]0.25)
        $closeScrollArgs = [object[]]@([single]-10)
        $mod.GetType('TIEconomyMod.Patches.BattleCloseMovementScalePatch',$true).GetMethod('Postfix').Invoke($null,$closeMoveArgs)
        $mod.GetType('TIEconomyMod.Patches.BattleCloseScrollScalePatch',$true).GetMethod('Postfix').Invoke($null,$closeScrollArgs)
        # Native camera Scale is 0.05*A; both modes must yield the same close
        # world movement/scroll coefficient for the same captured ship size.
        if ([Math]::Abs($closeMoveArgs[0]*0.05*$mode-0.0125) -gt .000001 -or
            [Math]::Abs($closeScrollArgs[0]*0.05*$mode+0.5) -gt .000001) { throw 'Close camera controls do not track captured ship size across modes.' }
        if ([Math]::Abs($scaleRuntime.GetProperty('ProjectileRatio').GetValue($null,$null)-.2) -gt .000001) { throw 'Tactical display range depends on cinematic flag rather than projectile scale.' }
        $scaleRuntime.GetMethod('Release').Invoke($null,@())
        $scaleRuntime.GetMethod('Capture').Invoke($null,@())
        if ([Math]::Abs($scaleRuntime.GetProperty('ModelScale').GetValue($null,$null)-.001) -gt .00000001 -or
            [Math]::Abs($scaleRuntime.GetProperty('ProjectileScale').GetValue($null,$null)-.00025) -gt .00000001) { throw 'Next battle failed to read independent 20x/5x scales.' }
        $scaleRuntime.GetMethod('Release').Invoke($null,@())
    }
    $writer = New-Object IO.StringWriter
    try {
        $serializer.Serialize($writer,$cfg)
        $reader2 = New-Object IO.StringReader($writer.ToString())
        try { $roundTrip = $serializer.Deserialize($reader2) } finally { $reader2.Dispose() }
        $roundTripShip = $settingsType.GetField('shipBalance').GetValue($roundTrip)
        if ($shipField.GetValue($roundTripShip) -ne 20 -or $projectileField.GetValue($roundTripShip) -ne 5) { throw 'Scale XML round trip lost edited values.' }
    } finally { $writer.Dispose() }
    foreach ($bad in @([single]0,[single]-1,[single]::NaN,[single]::PositiveInfinity,[single]1001)) {
        $shipField.SetValue($shipCfg,$bad); $projectileField.SetValue($shipCfg,$bad)
        $logger = [Action[string]]{param($message)}
        $settingsType.GetMethod('ValidateAndRepair').Invoke($cfg,[object[]]@($logger))
        if ($shipField.GetValue($shipCfg) -ne 10 -or $projectileField.GetValue($shipCfg) -ne 10) { throw 'Invalid scale settings were not repaired.' }
    }
    $spacing = $game.GetType('TIFormationTemplate',$true).GetMethod('GetSpacingOffset_km',$flags)
    foreach ($mode in @(@([single]0.1,[single]1,[double]0.3,[double]0.5),@([single]0.1,[single]10,[double]3,[double]5),@([single]1,[single]10,[double]3,[double]5),@([single]0.1,[single]20,[double]6,[double]10),@([single]1,[single]20,[double]6,[double]10))) {
        if ($mode[0] -le 0 -or $mode[1] -le 0 -or $mode[2] -le 0 -or $mode[3] -le 0) { throw 'Invalid positive formation test inputs.' }
        $scaleRuntime.GetMethod('Release').Invoke($null,@())
        $shipField.SetValue($shipCfg,[single]$mode[1])
        $cachedScale.SetValue($null,[single]$mode[0])
        $globalValues.SetValue($null,$false)
        $positions = $spacing.Invoke($null,[object[]]@($true,$false))
        if ([Math]::Abs($positions[0].x-$mode[2]) -gt .00001 -or [Math]::Abs($positions[0].z-$mode[3]) -gt .00001) { throw 'Combat formation spacing does not follow model scale.' }
        $strategic = $spacing.Invoke($null,[object[]]@($true,$true))
        if ($strategic[0].x -ne 60 -or $strategic[0].z -ne 100) { throw 'Combat scaling leaked into strategic formation spacing.' }
        $globalValues.SetValue($null,$true)
        $reinforcement = $spacing.Invoke($null,[object[]]@($false,$false))
        if ([Math]::Abs($reinforcement[0].x-$mode[2]) -gt .00001) { throw 'Live/reinforcement spacing diverges from setup.' }
        $main.GetField('enabled').SetValue($null,$false)
        $native = $spacing.Invoke($null,[object[]]@($true,$false))
        if ([Math]::Abs($native[0].x-60*$mode[0]) -gt .00001) { throw 'Disabled formation scaling must retain native behavior.' }
        $main.GetField('enabled').SetValue($null,$true)
    }
    $explosionMath = $mod.GetType('TIEconomyMod.Core.CombatExplosionGeometry',$true)
    foreach ($diameter in @([single]0.005,[single]0.1)) {
        if ($diameter -le 0) { throw 'Invalid positive explosion test diameter.' }
        $worldScale = [single]$explosionMath.GetMethod('WorldScale').Invoke($null,[object[]]@([single]$diameter,[single]15))
        foreach ($parent in @(@(.001,.002,.003),@(-.02,.01,-.005),@(1,1,1))) {
            $v = Vec $parent[0] $parent[1] $parent[2]
            $local = $explosionMath.GetMethod('LocalScale').Invoke($null,[object[]]@($v,[single]$worldScale))
            foreach ($axis in @('x','y','z')) {
                $idx = @('x','y','z').IndexOf($axis)
                if ([Math]::Abs($local.$axis*[Math]::Abs($parent[$idx])*15-$diameter) -gt .000001) { throw 'Effect world diameter depends on renderer/parent scale.' }
            }
        }
    }
} finally {
    $binder.UnpatchAll('eeo.battle.geometry.validation')
    $runtimeType = $mod.GetType('TIEconomyMod.Patches.BattleScaleRuntime',$true)
    $runtimeType.GetMethod('Release').Invoke($null,@())
    $main.GetField('enabled').SetValue($null,$oldEnabled)
    $main.GetField('settings').SetValue($null,$oldSettings)
    $cachedScale.SetValue($null,$oldScale)
    $globalValues.SetValue($null,$oldCombat)
}
Write-Host 'PASS: battle metre conversions, both cinematic factors, native geometry lifecycle targets, guarded missile sweep and negative case, exact 20x camera minima.'
Write-Host "PASS: 41 Harmony targets resolved; $unityOnly require Unity-host binding confirmation."
Write-Host "PASS: $records unreadable mesh fingerprints and $payloads unique checksummed payloads; source bundle identity; guarded waypoint/path/dolly rewrites and negative cases; 7 camera crossing/retreat cases."
Write-Host 'PASS: formation setup/live/strategic/disabled cases in both modes; explosion world-diameter invariance under nonuniform and mirrored renderer scales; grid and six spatial jitter rewrites.'
Write-Host 'PASS: known DestroyHabModule cache-initializer target rejected; all 41 explicit battle hook targets checked for direct early AssetCacheManager references; station hook resolves to InitializeForCombat.'
Write-Host "PASS: sixteen aperture resources cover native laser models; $lensChecks visible-aperture scale/bake/mirror comparisons; invalid ratio rejected; six initial camera radius/offset rewrites and negative guard."
Write-Host 'PASS: 10x settings/default migration, independent 20x/5x XML round trip, invalid repair, immutable active-battle capture, next-battle recapture, shared close-camera scaling and mode-independent projectile tracking range.'
Write-Host 'Unity mesh/collider world bounds require the in-game readback; this validator does not instantiate a Unity scene.'

# Vanilla renderer/texture contract. No replacement renderer, endpoint writes,
# UV offsets, shader/color replacement, transform scale or width-curve changes.
$laser=$mod.GetType('TIEconomyMod.Core.LaserBeamGeometry',$true)
foreach($method in $laser.GetMethods([Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')) {
    if($null -eq $method.GetMethodBody()){continue}
    $body=@($reader.Invoke($null,[object[]]@($method,$null)))
    if(@($body | Where-Object {$_.operand -is [Reflection.MethodInfo] -and $_.operand.Name -in @('AddComponent','SetPositions','SetPosition','set_localScale','set_widthCurve','set_mainTexture','set_mainTextureOffset','set_shader','set_color','set_enabled')}).Count){throw "Vanilla beam contract violated by $($method.Name)"}
}
if($mod.GetType('TIEconomyMod.Patches.BattleLaserContinuousEndpointsPatch',$false)){throw 'Rejected endpoint bridge remains packaged.'}
if(@($mod.GetReferencedAssemblies() | Where-Object Name -eq 'ShapesRuntime').Count){throw 'Rejected Shapes renderer dependency remains.'}
$textureState=$mod.GetType('TIEconomyMod.Core.LaserTexturePeriodState',$true)
$vector2=$unity.GetType('UnityEngine.Vector2',$true)
function UV([single]$X,[single]$Y){[Activator]::CreateInstance($vector2,[object[]]@($X,$Y))}
$uvChecks=0
foreach($legacyParent in @([single]0.001,[single]0.01,[single]0.00013)) {
    foreach($ratio in @([single]0.0005,[single]0.05,[single]0.5,[single]1,[single]5,[single]50)) {
        $state=[Activator]::CreateInstance($textureState)
        $native=UV (.5/($legacyParent*$ratio)) (.05/($legacyParent*$ratio))
        $adjusted=$textureState.GetMethod('Configure').Invoke($state,[object[]]@($native,$ratio))
        if([Math]::Abs($adjusted.x-.5/$legacyParent) -gt .001 -or $adjusted.y -ne $native.y){throw 'Vanilla repeat length or native V changed.'};$uvChecks++
        $repeat=$textureState.GetMethod('Configure').Invoke($state,[object[]]@($adjusted,$ratio))
        if($repeat.x -ne $adjusted.x -or $repeat.y -ne $adjusted.y){throw 'Active refiring compounded texture correction.'};$uvChecks++
        $restored=$textureState.GetMethod('Restore').Invoke($state,[object[]]@($adjusted))
        if($restored.x -ne $native.x -or $restored.y -ne $native.y){throw 'Native UV state did not restore.'};$uvChecks++
        $next=UV 4 7
        $rebinding=$textureState.GetMethod('Configure').Invoke($state,[object[]]@($next,[single]0.25))
        if($rebinding.x -ne 1 -or $rebinding.y -ne 7){throw 'Reused effect did not recapture native UV state.'};$uvChecks++
    }
}
Write-Host "PASS: vanilla LineRenderer/material/texture/profile/endpoints preserved; no Shapes dependency/bridge; $uvChecks longitudinal-period/V/refire/restore/reuse cases. Visual acceptance remains Unity-host testing."
