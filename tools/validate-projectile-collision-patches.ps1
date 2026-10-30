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
        throw "Required projectile-collision assembly is missing: $Path"
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

$ballisticType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.SpaceCombat.BallisticProjectileController',
    $true)
$target = $ballisticType.GetMethod(
    'UpdateController', [Reflection.BindingFlags]'Public,Instance')
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
    return $instructionReader[0].PSObject.BaseObject.Invoke(
        $null, $arguments)
}

$readerArguments = [object[]]::new(2)
$readerArguments[0] = $target
$readerArguments[1] = $null
$original = $instructionReader[0].PSObject.BaseObject.Invoke(
    $null, $readerArguments)
$patchType = $modAssembly.GetType(
    'TIEconomyMod.Patches.ProjectileMovementSweepPatch', $true)
$transpiler = $patchType.GetMethod(
    'Transpiler', [Reflection.BindingFlags]'Public,Static')
$originalMargins = @($original | Where-Object {
    $_.opcode.Name -eq 'ldc.r4' -and [Math]::Abs([single]$_.operand - 1.2) -lt 0.000001
})
try {
    $transpilerArguments = [object[]]::new(1)
    $transpilerArguments[0] = $original
    $patched = @($transpiler.Invoke($null, $transpilerArguments))
}
catch {
    $failure = $_.Exception
    while ($failure.InnerException) {
        $failure = $failure.InnerException
    }
    throw $failure
}

$patchedMargins = @($patched | Where-Object {
    $_.opcode.Name -eq 'ldc.r4' -and [Math]::Abs([single]$_.operand - 1.2) -lt 0.000001
})
$sweepHelper = $patchType.GetMethod(
    'ActiveMovementSweepMultiplier', [Reflection.BindingFlags]'Public,Static')
$helperCalls = @($patched | Where-Object {
    $_.opcode.Name -eq 'call' -and $_.operand -eq $sweepHelper
})
if ($originalMargins.Count -ne 1 -or
    $patchedMargins.Count -ne 0 -or
    $helperCalls.Count -ne 1 -or
    $patched.Count -ne @($original).Count + 2) {
    throw "Ballistic sweep patch mismatch: original 1.2=$($originalMargins.Count), patched 1.2=$($patchedMargins.Count), helper calls=$($helperCalls.Count), instructions=$(@($original).Count)->$($patched.Count)."
}

$projectileControllerType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.SpaceCombat.ProjectileController', $true)
$projectileStateType = $gameAssembly.GetType(
    'PavonisInteractive.TerraInvicta.TISpaceCombatProjectileState', $true)
if ($null -eq $ballisticType.GetMethod('Fire') -or
    $null -eq $projectileControllerType.GetMethod('ApplyDamage') -or
    @($projectileStateType.GetMethods() | Where-Object {
        $_.Name -eq 'CrossSectionalArea_m2' -and $_.IsStatic -and
        $_.GetParameters().Count -eq 2
    }).Count -ne 1) {
    throw 'One or more projectile geometry or durability targets are missing.'
}

$patchTypeNames = @(
    'TIEconomyMod.Patches.ProjectileColliderSizingPatch',
    'TIEconomyMod.Patches.ProjectileCrossSectionPatch',
    'TIEconomyMod.Patches.ProjectileMovementSweepPatch',
    'TIEconomyMod.Patches.NavalGunProjectileDurabilityPatch'
)
foreach ($patchTypeName in $patchTypeNames) {
    if ($null -eq $modAssembly.GetType($patchTypeName, $false)) {
        throw "Packaged projectile patch '$patchTypeName' is missing."
    }
}

$geometryRegistryType = $modAssembly.GetType(
    'TIEconomyMod.ProjectileGeometryRegistry', $true)
$diameterLookup = $geometryRegistryType.GetMethod(
    'TryGetDiameter_mm', [Reflection.BindingFlags]'Public,Static')
$diameterInstructions = @(Read-Instructions $diameterLookup)
$diameterCalls = @($diameterInstructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo]
} | ForEach-Object { $_.operand.Name })
$ammoMassReads = @($diameterInstructions | Where-Object {
    $_.operand -is [Reflection.FieldInfo] -and
    $_.operand.Name -eq 'ammoMass_kg'
})
if ($diameterCalls -notcontains 'TryGet' -or
    $diameterCalls -notcontains 'MagneticProjectileDiameter_mm' -or
    $ammoMassReads.Count -ne 1) {
    throw 'Magnetic projectile geometry no longer prefers explicit diameter data and falls back to complete projectile mass.'
}

$effectiveMassGetter = $projectileStateType.GetProperty(
    'effectiveMass_kg').GetGetMethod()
$effectiveMassInstructions = @(Read-Instructions $effectiveMassGetter)
$effectiveMassFields = @($effectiveMassInstructions | Where-Object {
    $_.operand -is [Reflection.FieldInfo]
} | ForEach-Object { $_.operand.Name })
foreach ($requiredField in @('warheadMass_kg', 'massDamage_kg')) {
    if ($effectiveMassFields -notcontains $requiredField) {
        throw "Magnetic projectile durability no longer derives from '$requiredField'."
    }
}

Write-Host 'PASS: projectile geometry covers explicit calibers and mass-derived magnetic rounds, magnetic durability follows damaging mass, and the guarded sweep rewrites exactly 120% to 100% for TI 1.0.51.'

$runtime = $modAssembly.GetType('TIEconomyMod.Core.BallisticContactRuntime',$true)
foreach ($helper in @('CastCompletedStep','ReflectFlight')) {
    $calls = @($patched | Where-Object { $_.operand -is [Reflection.MethodInfo] -and $_.operand.DeclaringType -eq $runtime -and $_.operand.Name -eq $helper })
    if ($calls.Count -ne 1) { throw "Expected one completed-step $helper call." }
}
# Ensure initialization-only geometry does not regress to per-frame writes.
foreach ($entry in @(@('CombatMeshGeometry','LateUpdate'),@('CombatRenderGeometry','LateUpdate'),@('LaserBeamGeometry','Apply'))) {
    $body = @(Read-Instructions ($modAssembly.GetType('TIEconomyMod.Core.'+$entry[0],$true).GetMethod($entry[1],[Reflection.BindingFlags]'Public,NonPublic,Instance')))
    if (@($body | Where-Object { $_.operand -is [Reflection.MethodInfo] -and $_.operand.Name -in @('Refresh','BakedMesh','set_size','set_center','set_widthCurve','set_widthMultiplier','set_sharedMesh') }).Count -ne 0) { throw "Recurring geometry writes in $entry" }
}
$unity = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'UnityEngine.CoreModule' } | Select-Object -First 1
$vector = $unity.GetType('UnityEngine.Vector3',$true)
function Vec([single]$X,[single]$Y,[single]$Z) { [Activator]::CreateInstance($vector,[object[]]@($X,$Y,$Z)) }
$points = [Array]::CreateInstance($vector,6)
$points.SetValue((Vec 10 -1 -1),0);$points.SetValue((Vec 10 1 -1),1);$points.SetValue((Vec 10 0 1),2)
$points.SetValue((Vec 20 -1 -1),3);$points.SetValue((Vec 20 1 -1),4);$points.SetValue((Vec 20 0 1),5)
$bvhType = $modAssembly.GetType('TIEconomyMod.Core.RenderedMeshRaycast',$true)
$constructor = $bvhType.GetConstructor([type[]]@($vector.MakeArrayType(),[int[]]))
$tree = $constructor.Invoke([object[]]@($points,[int[]]@(0,1,2,3,4,5)))
function Check-Ray($Origin,$Direction,[single]$Maximum,[bool]$Expected,[single]$Distance) {
    $args = [object[]]@($Origin,$Direction,$Maximum,[single]0,(Vec 0 0 0))
    $hit=$bvhType.GetMethod('Intersect').Invoke($tree,$args)
    if($hit -ne $Expected -or ($hit -and [Math]::Abs($args[3]-$Distance) -gt .0001)) { throw "Mesh travelled-segment mismatch: hit=$hit distance=$($args[3])" }
}
Check-Ray (Vec 0 0 0) (Vec 1 0 0) 5 $false 0
Check-Ray (Vec 0 0 0) (Vec 1 0 0) 15 $true 10
Check-Ray (Vec 0 0 0) (Vec 1 0 0) 9.99 $false 0
Check-Ray (Vec 0 0 0) (Vec 1 0 0) 10 $true 10
Check-Ray (Vec 0 .9 .9) (Vec 1 0 0) 30 $false 0
Check-Ray (Vec 11 0 0) (Vec -1 0 0) 5 $true 1
Check-Ray (Vec 0 0 0) (Vec 2 0 0) 15 $true 5
Check-Ray (Vec 0 0 0) (Vec -2 0 0) 15 $false 0
Check-Ray (Vec 0 0 0) (Vec 1 1 0) 15 $false 0
Check-Ray (Vec 10 0 0) (Vec 1 0 0) 5 $true 0
# Force internal BVH branches and compare against an independent plane/triangle
# analytic result over 201 lines through a stack of triangles.
$points = [Array]::CreateInstance($vector,96);$indices=[int[]]::new(96)
for($i=0;$i -lt 32;$i++) {
    $points.SetValue((Vec (10+$i) -1 -1),3*$i);$points.SetValue((Vec (10+$i) 1 -1),(3*$i+1));$points.SetValue((Vec (10+$i) 0 1),(3*$i+2))
    for($j=0;$j -lt 3;$j++){$indices[3*$i+$j]=3*$i+$j}
}
$tree=$constructor.Invoke([object[]]@($points,$indices))
for($i=-100;$i -le 100;$i++) {
    $y=$i/80.0;$z=.21
    $expected=[Math]::Abs($y) -le (1-$z)/2
    Check-Ray (Vec 0 $y $z) (Vec 1 0 0) 50 $expected 10
}
Write-Host 'PASS: completed-step cast/reflection guards; no recurring collider/mesh/beam setters; 211 exact mesh contact cases including future rejection, empty box corner, inherited-direction divergence, nonunit transformed direction, backface and BVH branch traversal.'

# Cold construction and topology allocation must never re-enter contact queries.
foreach ($entry in @(@('CombatMeshGeometry','RaycastVisual'),@('BallisticContactRuntime','CastCompletedStep'))) {
    $method=$modAssembly.GetType('TIEconomyMod.Core.'+$entry[0],$true).GetMethod($entry[1],[Reflection.BindingFlags]'Public,Static,Instance')
    $body=@(Read-Instructions $method)
    if (@($body | Where-Object { $_.operand -is [Reflection.MethodInfo] -and $_.operand.DeclaringType.Name -eq 'RenderedMeshRaycast' -and $_.operand.Name -in @('For','Invalidate') }).Count -ne 0) { throw "Contact-time BVH construction/invalidation returned in $entry" }
}
if($bvhType.GetMethod('Refit')) { throw 'Prepared static trees must not expose live refitting.' }
foreach($entry in @(@('CombatMeshGeometry','RaycastVisual'),@('CombatMeshGeometry','Snapshot'),@('CombatMeshGeometry','TryContact'),@('BallisticContactRuntime','AfterMovement'),@('PreparedContactWorld','Cast'))) {
    $method=$modAssembly.GetType('TIEconomyMod.Core.'+$entry[0],$true).GetMethod($entry[1],[Reflection.BindingFlags]'Public,NonPublic,Static,Instance')
    $body=@(Read-Instructions $method)
    if(@($body | Where-Object { $_.operand -is [Reflection.MethodInfo] -and $_.operand.Name -in @('BakeMesh','Refit','GetVertices','set_vertices','SyncTransforms','OverlapCapsuleNonAlloc','RaycastAll') }).Count) {throw "Expensive mutable geometry/physics work returned in $entry"}
}
$colliderField=$unity.GetType('UnityEngine.RaycastHit',$false)
# RaycastHit lives in the physics module; validate the pinned identity contract
# without invoking collider-object lookup or constructing a Unity scene.
$physics=[AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'UnityEngine.PhysicsModule' } | Select-Object -First 1
$hitType=$physics.GetType('UnityEngine.RaycastHit',$true)
$field=$hitType.GetField('m_Collider',[Reflection.BindingFlags]'NonPublic,Instance')
if($null -eq $field -or $field.FieldType -ne [int]) {throw 'Pinned RaycastHit collider identity changed.'}
$worldType=$modAssembly.GetType('TIEconomyMod.Core.PreparedContactWorld',$true)
$identity=$worldType.GetMethod('HitForId').Invoke($null,[object[]]@([int]12345,(Vec 1 2 3),(Vec 0 1 0),[single]4))
if($field.GetValue($identity) -ne 12345 -or $identity.distance -ne 4) {throw 'Prepared hit identity emission failed.'}
$references=@($ModAssemblyPath,(Join-Path $TargetManagedDir 'UnityEngine.CoreModule.dll'),(Join-Path $TargetManagedDir 'UnityEngine.PhysicsModule.dll'))
Add-Type -Path (Join-Path $PSScriptRoot 'validation\PreparedContactChecks.cs') -ReferencedAssemblies $references
Write-Host ([PreparedContactChecks]::Run())
