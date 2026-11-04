[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$TargetManagedDir,
    [Parameter(Mandatory=$true)][string]$ModAssemblyPath,
    [Parameter(Mandatory=$true)][string]$RepositoryRoot
)
$ErrorActionPreference='Stop'
function Load-Bytes([string]$p){[Reflection.Assembly]::Load([IO.File]::ReadAllBytes($p))}
$ha=Load-Bytes (Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
[void](Load-Bytes (Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
foreach($f in Get-ChildItem $TargetManagedDir -Filter '*.dll'|Where-Object{$_.Name -like 'Unity*.dll' -or $_.Name -like 'Newtonsoft*.dll' -or $_.Name -eq 'FMODUnity.dll'}){[void](Load-Bytes $f.FullName)}
$game=Load-Bytes (Join-Path $TargetManagedDir 'Assembly-CSharp.dll');$mod=Load-Bytes $ModAssemblyPath
$ser=$game.GetType('StringSerializationAPI',$true).GetMethod('Deserialize')
$manager=$game.GetType('PavonisInteractive.TerraInvicta.TemplateManager',$true)
$add=@($manager.GetMethods()|Where-Object{$_.Name -eq 'Add' -and $_.GetParameters().Count -eq 3})[0]
function Register([Type]$t,[string]$json){$v=$ser.Invoke($null,@($t,$json));[void]$add.Invoke($null,@($v,$t,$true));return $v}
$global=Register $game.GetType('TIGlobalConfig',$true) '{"dataName":"TIGlobalConfig","spaceResourceToTons":1}'
$manager.GetField('_global',[Reflection.BindingFlags]'Instance,NonPublic').SetValue($manager.GetField('self').GetValue($null),$global)
$mission=$game.GetType('TIMissionTemplate',$true)
foreach($name in @('PassTechnology','Contact','Protect')){[void](Register $mission ('{"dataName":"'+$name+'"}'))}
$armorType=$game.GetType('TIShipArmorTemplate',$true);$factionType=$game.GetType('PavonisInteractive.TerraInvicta.TIFactionState',$true)
$flags=[Reflection.BindingFlags]'Instance,Public,NonPublic';$cache=$factionType.GetField('cachedAllowedArmors',$flags);$obsolete=$factionType.GetField('obsoletedShipParts',$flags)
$best=$factionType.GetMethod('GetBestArmor');$gameState=$game.GetType('PavonisInteractive.TerraInvicta.TIGameState',$true)
$templates=Join-Path (Split-Path $TargetManagedDir) 'StreamingAssets\Templates'
$native=Get-Content (Join-Path $templates 'TIShipArmorTemplate.json') -Raw|ConvertFrom-Json
$changes=Get-Content (Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\TIShipArmorTemplate.json') -Raw|ConvertFrom-Json
function Armors([bool]$alien,[bool]$current,[bool]$rename=$false){
    $list=[Activator]::CreateInstance($cache.FieldType)
    foreach($original in $native|Where-Object{($_.dataName -like 'Alien*') -eq $alien}){
        $row=$original.PSObject.Copy()
        if($current){$change=@($changes|Where-Object dataName -eq $row.dataName);if($change.Count){foreach($p in $change[0].PSObject.Properties){$row|Add-Member -NotePropertyName $p.Name -NotePropertyValue $p.Value -Force}}}
        if($rename){$row.dataName=if($list.Count -eq 0){'RenamedFirstMaterial'}else{'RenamedSecondMaterial'}}
        $list.Add($ser.Invoke($null,@($armorType,[string]($row|ConvertTo-Json -Depth 25 -Compress))))
    }
    return ,$list
}
function Faction([bool]$alien,$armors){
    $name=if($alien){'ArmorProbeAlien'}else{'ArmorProbeHuman'}
    $ft=Register $game.GetType('TIFactionTemplate',$true) ('{"dataName":"'+$name+'","isAlien":'+$alien.ToString().ToLowerInvariant()+'}')
    $f=[Runtime.Serialization.FormatterServices]::GetUninitializedObject($factionType)
    $gameState.GetField('template',$flags).SetValue($f,$ft);$f.templateName=$name
    $cache.SetValue($f,$armors);$obsolete.SetValue($f,[Activator]::CreateInstance($obsolete.FieldType));return $f
}
function Selected($f,[bool]$premium){$r=$best.Invoke($f,@($premium));if($null -eq $r){return $null};return $r.dataName}
function Expect($f,[bool]$premium,[string]$name){$r=Selected $f $premium;if($r -ne $name){throw "Armor selection expected '$name', got '$r' (premium=$premium)"}}
$alienArmors=Armors $true $true;$alien=Faction $true $alienArmors
$vanillaAlien=Faction $true (Armors $true $false)
$human=Faction $false (Armors $false $true)
if($null -ne (Selected $alien $false)){throw 'Original native crash precondition no longer reproduces.'}
Expect $vanillaAlien $false 'AlienAdamantaneArmor'
$humanBefore=@((Selected $human $false),(Selected $human $true))
$harmony=[Activator]::CreateInstance($ha.GetType('HarmonyLib.Harmony'),@('eeo.alien.armor.selection.validation'))
try{
    [void]$harmony.CreateClassProcessor($mod.GetType('TIEconomyMod.Patches.AlienBestArmorSelectionPatch',$true)).Patch()
    Expect $alien $false 'AlienAdamantaneArmor';Expect $alien $true 'AlienExoticArmor'
    Expect $human $false $humanBefore[0];Expect $human $true $humanBefore[1]
    # A human with only exotic-bearing candidates still follows the native
    # restriction, rather than receiving the alien exception.
    $humanRestricted=Faction $false $alienArmors
    if($null -ne (Selected $humanRestricted $false)){throw 'Human exotic gate was removed.'}
    Expect $humanRestricted $true 'AlienExoticArmor'
    # Selection cannot depend on the two built-in data names.
    $renamed=Armors $true $true $true
    $renamedFaction=Faction $true $renamed
    Expect $renamedFaction $false 'RenamedFirstMaterial';Expect $renamedFaction $true 'RenamedSecondMaterial'
    # Cost changes, not identities or exotic presence, must change the budget
    # result. Premium selection continues to prefer protection per mass.
    $synthetic=[Activator]::CreateInstance($cache.FieldType)
    $synthetic.Add($ser.Invoke($null,@($armorType,'{"dataName":"MetalMaterial","heatofVaporization_MJkg":50,"weightedBuildMaterials":{"metals":1}}')))
    $synthetic.Add($ser.Invoke($null,@($armorType,'{"dataName":"RareMaterial","heatofVaporization_MJkg":100,"weightedBuildMaterials":{"exotics":1}}')))
    $syntheticFaction=Faction $true $synthetic
    Expect $syntheticFaction $false 'MetalMaterial';Expect $syntheticFaction $true 'RareMaterial'
    $oldPrice=$global.initialExoticsValue
    try{$global.initialExoticsValue=2;Expect $syntheticFaction $false 'RareMaterial'}finally{$global.initialExoticsValue=$oldPrice}
    # Registered but unavailable records must not become free unlocks.
    [void](Register $armorType '{"dataName":"UnavailableFreeArmor","heatofVaporization_MJkg":1000,"weightedBuildMaterials":{"water":0}}')
    Expect $alien $false 'AlienAdamantaneArmor';Expect $alien $true 'AlienExoticArmor'
    [void]$alien.obsoletedShipParts.Add('AlienAdamantaneArmor')
    Expect $alien $false 'AlienAdamantaneArmor';Expect $alien $true 'AlienExoticArmor'
    [void]$alien.obsoletedShipParts.Add('AlienExoticArmor')
    Expect $alien $false 'AlienAdamantaneArmor';Expect $alien $true 'AlienExoticArmor'
    $cost=$mod.GetType('TIEconomyMod.Patches.AlienArmorSelection',$true).GetMethod('MaterialCost')
    $budget=[double]$cost.Invoke($null,@($alienArmors[0]));$premium=[double]$cost.Invoke($null,@($alienArmors[1]))
    if($budget -ge $premium -or $budget -le 0){throw 'Current material prices do not reproduce the requested ordering.'}
    $hab=$game.GetType('TIHabModuleTemplate',$true).GetMethod('SpaceCombatValue_Base');$il=$hab.GetMethodBody().GetILAsByteArray()
    if($il[0xa2] -ne 0x6f -or $hab.Module.ResolveMethod([BitConverter]::ToInt32($il,0xa3)) -ne $best){throw 'Fatal habitat armor call site changed.'}
    Write-Host ('PASS: native null reproduction repaired for aliens; budget/premium material scores {0:N5}/{1:N5}, masses {2}/{3} kg per point.' -f $budget,$premium,$alienArmors[0].mass_damagePoint_kg,$alienArmors[1].mass_damagePoint_kg)
    Write-Host 'PASS: actual native getter binding, unchanged human choices/exotic restriction, renamed materials, data-valued exotic costs, unavailable records, obsolescence fallback and habitat call-site routing validate.'
    Write-Host 'Manual validation remains: loading the reported campaign and native habitat/AI initialization in Unity.'
}
finally{$harmony.UnpatchAll('eeo.alien.armor.selection.validation')}
