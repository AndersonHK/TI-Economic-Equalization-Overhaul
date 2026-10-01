[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$TargetManagedDir,
    [Parameter(Mandatory = $true)][string]$ModAssemblyPath
)

$ErrorActionPreference = 'Stop'
function Load-AssemblyBytes([string]$Path) {
    return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
}
$harmonyAssembly = Load-AssemblyBytes (Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($file in Get-ChildItem -LiteralPath $TargetManagedDir -Filter 'Unity*.dll' -File) {
    [void](Load-AssemblyBytes $file.FullName)
}
foreach ($name in @('Newtonsoft.Json.dll', 'FMODUnity.dll', 'UnityModManager\UnityModManager.dll')) {
    [void](Load-AssemblyBytes (Join-Path $TargetManagedDir $name))
}
$game = Load-AssemblyBytes (Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
$mod = Load-AssemblyBytes $ModAssemblyPath
$instanceFlags = [Reflection.BindingFlags]'Instance,Public,NonPublic'
$staticFlags = [Reflection.BindingFlags]'Static,Public,NonPublic'
$nationType = $game.GetType('PavonisInteractive.TerraInvicta.TINationState', $true)
$factionType = $game.GetType('PavonisInteractive.TerraInvicta.TIFactionState', $true)
$pointType = $game.GetType('PavonisInteractive.TerraInvicta.TIControlPoint', $true)
$regionType = $game.GetType('PavonisInteractive.TerraInvicta.TIRegionState', $true)
$stateType = $game.GetType('PavonisInteractive.TerraInvicta.TIGameState', $true)
$idType = $game.GetType('PavonisInteractive.TerraInvicta.GameStateID', $true)
$patch = $mod.GetType('TIEconomyMod.Patches.AlienTerritoryDemandTargetsPatch', $true)
$postfix = $patch.GetMethod('Postfix', $staticFlags)
$enabled = $mod.GetType('TIEconomyMod.Main', $true).GetField('enabled', $staticFlags)
$proxy = $game.GetType('PavonisInteractive.TerraInvicta.GameStateManager', $true).
    GetField('alienProxyFaction', $staticFlags)
$script:nextId = 1000

# TINationState's static relationship cost reads this template on first use.
$missionType = $game.GetType('TIMissionTemplate', $true)
$missionPath = Join-Path (Split-Path -Parent $TargetManagedDir) 'StreamingAssets\Templates\TIMissionTemplate.json'
$missions = Get-Content -LiteralPath $missionPath -Raw | ConvertFrom-Json
$missionRow = @($missions | Where-Object dataName -eq 'SetNationalPolicy')[0]
$deserialize = $game.GetType('StringSerializationAPI', $true).GetMethod('Deserialize')
$mission = $deserialize.Invoke($null, @($missionType, [string]($missionRow | ConvertTo-Json -Depth 30)))
$manager = $game.GetType('PavonisInteractive.TerraInvicta.TemplateManager', $true)
$add = @($manager.GetMethods() | Where-Object { $_.Name -eq 'Add' -and $_.GetParameters().Count -eq 3 })[0]
[void]$add.Invoke($null, @($mission, $missionType, $true))

function New-State([Type]$Type) {
    $state = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($Type)
    $script:nextId++
    $id = [Activator]::CreateInstance($idType, @([int]$script:nextId))
    $stateType.GetProperty('ID', $instanceFlags).SetValue($state, $id, $null)
    return $state
}
function New-Target([object[]]$Owners) {
    $nation = New-State $nationType
    $points = [Activator]::CreateInstance($nationType.GetField('controlPoints').FieldType)
    foreach ($owner in $Owners) {
        $point = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($pointType)
        $pointType.GetField('<faction>k__BackingField', $instanceFlags).SetValue($point, $owner)
        $points.Add($point)
    }
    $nation.controlPoints = $points
    $nationType.GetField('<numControlPoints>k__BackingField', $instanceFlags).SetValue($nation, $Owners.Count)
    $region = New-State $regionType
    $regionType.GetField('<nation>k__BackingField', $instanceFlags).SetValue($region, $nation)
    return $region
}
function Assert-Targets([object]$Actor, [object[]]$Targets, [object[]]$Expected, [string]$Label) {
    $listType = [System.Collections.Generic.List``1].MakeGenericType($stateType)
    $list = [Activator]::CreateInstance($listType)
    foreach ($target in $Targets) { $list.Add($target) }
    $arguments = [object[]]@($Actor, $list)
    [void]$postfix.Invoke($null, $arguments)
    $actual = $arguments[1]
    if ($actual.Count -ne $Expected.Count) { throw "$Label returned $($actual.Count) targets; expected $($Expected.Count)." }
    for ($i = 0; $i -lt $Expected.Count; $i++) {
        if (-not [object]::ReferenceEquals($actual[$i], $Expected[$i])) { throw "$Label changed target identity/order." }
    }
}

$harmonyId = 'ti.eeo.validate.alien-territory.' + [Guid]::NewGuid().ToString('N')
$harmony = [Activator]::CreateInstance($harmonyAssembly.GetType('HarmonyLib.Harmony', $true), @($harmonyId))
$priorProxy = $proxy.GetValue($null)
$priorEnabled = $enabled.GetValue($null)
try {
    $methods = @($harmony.CreateClassProcessor($patch).Patch())
    $target = $game.GetType('TransferRegionsOption', $true).GetMethod(
        'GetPossibleTargets', [Type[]]@($nationType))
    $patchInfo = $harmonyAssembly.GetType('HarmonyLib.Harmony', $true).
        GetMethod('GetPatchInfo', $staticFlags).Invoke($null, @($target))
    if ($methods.Count -ne 1 -or $null -eq $patchInfo -or
        $harmonyId -notin $patchInfo.Owners) {
        throw 'Alien territory demand postfix did not bind to the expected target.'
    }
    $servants = New-State $factionType
    $other = New-State $factionType
    $proxy.SetValue($null, $servants)
    $enabled.SetValue($null, $true)
    $alien = New-State $nationType
    $alien.alienNation = $true
    $human = New-State $nationType
    $partial = New-Target @($other, $servants)
    $unowned = New-Target @($null, $servants)
    $full = New-Target @($servants, $servants)
    $single = New-Target @($servants)
    $nonServant = New-Target @($servants, $other)
    $targets = @($partial, $full, $unowned, $nonServant, $single)
    Assert-Targets $alien $targets @($full, $nonServant, $single) 'Alien demand ownership filter'
    Assert-Targets $human $targets $targets 'Human demand unchanged'
    Assert-Targets $alien @() @() 'Native empty list remains empty'
    $enabled.SetValue($null, $false)
    Assert-Targets $alien $targets $targets 'Disabled mod unchanged'
}
finally {
    $proxy.SetValue($null, $priorProxy)
    $enabled.SetValue($null, $priorEnabled)
    $harmony.UnpatchAll($harmonyId)
}
Write-Host 'PASS: alien territory demand Harmony binding, full/partial/unowned ownership, non-Servant targets, human policies, empty targets, and disabled-mod behavior.'
