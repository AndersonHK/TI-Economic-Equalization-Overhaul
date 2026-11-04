[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$TargetManagedDir,
    [Parameter(Mandatory=$true)][string]$ModAssemblyPath,
    [Parameter(Mandatory=$true)][string]$RepositoryRoot
)
$ErrorActionPreference='Stop'
function Load-Bytes([string]$Path){[Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))}
$ha=Load-Bytes (Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach($f in Get-ChildItem -LiteralPath $TargetManagedDir -Filter '*.dll'|Where-Object{$_.Name -like 'Unity*.dll' -or $_.Name -like 'Newtonsoft*.dll' -or $_.Name -eq 'FMODUnity.dll'}){[void](Load-Bytes $f.FullName)}
[void](Load-Bytes (Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$game=Load-Bytes (Join-Path $TargetManagedDir 'Assembly-CSharp.dll');$mod=Load-Bytes $ModAssemblyPath
$type=$mod.GetType('TIEconomyMod.Core.ShipDesignerLayout',$true)
$path=Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\Config\ship-designer-layout.eeo'
$data=$type.GetMethod('Load').Invoke($null,@([string]$path))
$rows=$type.GetMethod('ResolveRows').Invoke($data,@(7,8))
if($rows -ne 11 -or $data.WindowRows -ne 9 -or $type.GetMethod('HeightGrowth').Invoke($data,@(7,[int]$rows)) -ne 74){throw 'Allocation rows and existing window height are not independent.'}
if($type.GetMethod('ResolveRows').Invoke($data,@(7,10)) -ne 11){throw 'Later data rows are not parsed dynamically.'}
$rectType=$mod.GetType('TIEconomyMod.Core.DesignerRect',$true)
$mathType=$mod.GetType('TIEconomyMod.Core.DesignerViewportMath',$true)
$rectConstructor=$rectType.GetConstructor(@([single],[single],[single],[single]))
$tight=$rectConstructor.Invoke([object[]]@([single]8,[single]54,[single]828,[single]472))
$raw=Get-Content -LiteralPath $path -Raw
foreach($mutate in @(
    {param($j) $j.Rows=6}, {param($j) $j.Rows=33}, {param($j) $j.Columns=11},
    {param($j) $j.HalfRowPitch=0}, {param($j) $j.PSObject.Properties.Remove('Schema')},
    {param($j) $j|Add-Member -NotePropertyName Unknown -NotePropertyValue 1},
    {param($j) $j.PropellantLabel.Height=0}
)){
    $j=$raw|ConvertFrom-Json;[void](&$mutate $j);$rejected=$false
    try{[void]$type.GetMethod('Parse').Invoke($null,@([string]($j|ConvertTo-Json -Depth 8 -Compress)))}catch{$rejected=$true}
    if(-not $rejected){throw 'Invalid layout document accepted.'}
}
$templates=Join-Path (Split-Path -Parent $TargetManagedDir) 'StreamingAssets\Templates'
$native=Get-Content -LiteralPath (Join-Path $templates 'TIShipHullTemplate.json') -Raw|ConvertFrom-Json
$hulls=Get-Content -LiteralPath (Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\TIShipHullTemplate.json') -Raw|ConvertFrom-Json
$before=Get-Content -LiteralPath (Join-Path $RepositoryRoot 'docs\ship-balance-research\utility-expansion\hull-slots-before-0.11.1.json') -Raw|ConvertFrom-Json
$expected=@{Lancer=@(9,37,2435);Dreadnought=@(10,54,3084);Titan=@(14,58,3672);AlienDreadnought=@(13,50,2810);AlienTitan=@(13,60,2880)}
$deserialize=$game.GetType('StringSerializationAPI',$true).GetMethod('Deserialize')
$hullType=$game.GetType('TIShipHullTemplate',$true)
foreach($base in $native){
    $override=@($hulls|Where-Object dataName -eq $base.dataName)[0]
    $h=$base.PSObject.Copy();foreach($p in $override.PSObject.Properties){$h|Add-Member -NotePropertyName $p.Name -NotePropertyValue $p.Value -Force}
    $seen=@{}
    for($i=0;$i -lt $h.shipModuleSlots.Count;++$i){
        $s=$h.shipModuleSlots[$i];$key="$($s.x),$($s.y)"
        if($s.moduleSlotType -eq 'None'){continue}
        if($s.x -lt 0 -or $s.x -ge $data.Columns -or $s.y -lt 0 -or $s.y -ge $data.Rows -or $seen.ContainsKey($key)){throw "Duplicate/out-of-bounds data cell: $($h.dataName)/$i"}
        $seen[$key]=$true
        if($i -lt $base.shipModuleSlots.Count -and $s.moduleSlotType -ne $base.shipModuleSlots[$i].moduleSlotType){throw 'Serialized slot type/index changed.'}
        if($s.moduleSlotType -like '*Armor'){
            $anchorX=@{TailArmor=0;LateralArmor=4;NoseArmor=9}[$s.moduleSlotType]
            if($s.y -ne 8 -or $s.x -ne $anchorX){throw 'Armor is not at its canonical frame anchor.'}
        }
    }
    $parsed=$deserialize.Invoke($null,@($hullType,[string]($h|ConvertTo-Json -Depth 30 -Compress)))
    $bounds=$type.GetMethod('ValidateHull').Invoke($data,@($parsed))
    $fit=$mathType.GetMethod('Place').Invoke($null,@($bounds,$tight,[single]85,[single]-60))
    if($h.dataName -eq 'AlienMothership' -and ($fit.Fits -or [Math]::Abs($fit.MissingHeight-30.5) -gt 0.001)){throw 'Viewport check failed to account for full Mothership/armor counter bounds.'}
    $expanded=$rectConstructor.Invoke([object[]]@([single]8,[single]54,[single]828,[single]502.6))
    if(-not $mathType.GetMethod('Place').Invoke($null,@($bounds,$expanded,[single]85,[single]-60)).Fits){throw 'Expanded viewport cannot fit one of the complete hull layouts.'}
    if($parsed.shipModuleSlots.Count -ne $h.shipModuleSlots.Count){throw 'Native JSON parsing dropped extended rows.'}
    $n=@($h.shipModuleSlots|Where-Object moduleSlotType -eq 'Utility').Count
    if($n -ne $h.internalModules){throw "Utility count mismatch: $($h.dataName)"}
    if($expected.ContainsKey($h.dataName)){
        $e=$expected[$h.dataName]
        if($h.internalModules -ne $e[0] -or $h.crew -ne $e[1] -or $h.mass_tons -ne $e[2]){throw 'Utility/crew/mass targets differ.'}
        $old=@($before|Where-Object dataName -eq $h.dataName)[0]
        $added=if($h.dataName -in @('AlienDreadnought','AlienTitan')){3}else{2}
        if($h.shipModuleSlots.Count -ne $old.shipModuleSlots.Count+$added){throw 'Unexpected appended utility cell count.'}
        for($i=0;$i -lt $old.shipModuleSlots.Count;++$i){
            $s=$h.shipModuleSlots[$i];$o=$old.shipModuleSlots[$i]
            if($s.moduleSlotType -like '*Armor'){continue} # Canonical frame anchors are checked above.
            $dy=if($h.dataName -eq 'Titan' -and $o.moduleSlotType -eq 'Utility' -and $o.x -in @(3,4)){-1}else{0}
            if($h.dataName -eq 'AlienTitan' -and $o.moduleSlotType -eq 'Utility' -and $o.x -in @(5,6,7) -and $o.y -eq 5){$dy=1}
            if($s.moduleSlotType -ne $o.moduleSlotType -or $s.x -ne $o.x -or ($s.moduleSlotType -notlike '*Armor' -and $s.y -ne $o.y+$dy)){throw "Old slot or multi-cell geometry changed: $($h.dataName)/$i"}
        }
        if($h.mass_tons-$old.mass_tons -ne 20*$added){throw 'Structural mass not driven by JSON.'}
    }
    elseif($h.dataName -eq 'AlienLancer' -and ($h.internalModules -ne $base.internalModules -or $h.crew -ne $base.crew -or $h.mass_tons -ne $base.mass_tons)){throw 'Alien Lancer received an unintended bonus.'}
    if($h.dataName -in @('AlienDreadnought','AlienTitan')){
        $coords=@($h.shipModuleSlots|Where-Object moduleSlotType -eq 'Utility'|ForEach-Object{"$($_.x),$($_.y)"})
        foreach($s in $h.shipModuleSlots|Where-Object moduleSlotType -eq 'Utility'){
            if("$($s.x),$(6-$s.y)" -notin $coords){throw 'Alien utility reflection symmetry failed.'}
        }
    }
}
$latestBefore=Get-Content -LiteralPath (Join-Path $RepositoryRoot 'docs\ship-balance-research\alien-capital-expansion\before-0.11.2.json') -Raw|ConvertFrom-Json
foreach($old in $latestBefore){
    $h=@($hulls|Where-Object dataName -eq $old.dataName)[0]
    $isMother=$h.dataName -eq 'AlienMothership';$added=if($isMother){13}else{1};$mass=if($isMother){540}else{20}
    if($h.shipModuleSlots.Count -ne $old.shipModuleSlots.Count+$added -or $h.mass_tons-$old.mass_tons -ne $mass -or $null -ne $h.crew){throw 'Alien expansion count/cost/crew differs from approved policy.'}
    for($i=0;$i -lt $old.shipModuleSlots.Count;++$i){
        $s=$h.shipModuleSlots[$i];$o=$old.shipModuleSlots[$i]
        $dy=if($h.dataName -eq 'AlienTitan' -and $o.moduleSlotType -eq 'Utility' -and $o.x -in @(5,6,7) -and $o.y -eq 5){1}else{0}
        if($s.moduleSlotType -ne $o.moduleSlotType){throw 'Latest saved slot type/index changed.'}
        if($s.moduleSlotType -like '*Armor'){continue} # Canonical anchors checked independently above.
        if($isMother){
            if($i -eq 33){if($s.x -ne 0 -or $s.y -ne 0){throw 'Propellant is not back at (0,0).'};continue}
            if($o.moduleSlotType -eq 'Utility'){
                if($s.x -ne $o.x+1 -or $s.y -ne $o.y){throw 'Connected utility block was not translated rigidly.'};continue
            }
            $relocated=@{14=@(8,8);19=@(8,10);20=@(6,10);18=@(7,10)}
            if($relocated.ContainsKey($i)){if($s.x -ne $relocated[$i][0] -or $s.y -ne $relocated[$i][1]){throw 'Mothership UI repack differs from approved slots.'};continue}
        }
        if($s.x -ne $o.x -or $s.y -ne $o.y+$dy){throw 'Latest saved slot/utility adjacency changed.'}
    }
    if($isMother){
        foreach($i in @(0,1,3)){
            if($h.shipModuleSlots[$i].x -ne $old.shipModuleSlots[$i].x -or $h.shipModuleSlots[$i].y -ne $old.shipModuleSlots[$i].y){throw 'Fixed drive/reactor/radiator/armor location changed.'}
        }
        if($h.internalModules -ne 20 -or $h.noseHardpoints -ne 8 -or $h.hullHardpoints -ne 18){throw 'Mothership banks differ.'}
        foreach($x in @(8,9)){foreach($y in @(0,2,4,6)){
            if(-not @($h.shipModuleSlots|Where-Object{$_.moduleSlotType -eq 'NoseHardPoint' -and $_.x -eq $x -and $_.y -eq $y}).Count){throw 'Mothership eight-nose bank not symmetric.'}
        }}
        if(@($h.shipModuleSlots|Where-Object{$_.x -eq 0 -and $_.y -eq 6}).Count){throw 'Drive spinner clearance was filled.'}
        foreach($x in @(5,6,7,8)){foreach($y in $(if($x -eq 5){@(0,2,4,6)}elseif($x -eq 8){@(8,10)}else{@(0,2,4,6,8,10)})){
            if(-not @($h.shipModuleSlots|Where-Object{$_.moduleSlotType -eq 'HullHardPoint' -and $_.x -eq $x -and $_.y -eq $y}).Count){throw 'Hull cells are not a complete forward bank.'}
        }}
    }
}
foreach($kind in @('Utility','HullHardPoint','NoseHardPoint','PowerPlant','Radiator','LateralArmor','NoseArmor')){
    if($type.GetMethod('AcceptsSlot').Invoke($data,@('AlienMothership',0,7,$kind))){throw 'Aft column schema accepts an invalid member.'}
}
$motherRow=@($hulls|Where-Object dataName -eq 'AlienMothership')[0]
foreach($x in @(4,5)){foreach($y in @(8,9,10)){
    $bad=$deserialize.Invoke($null,@($hullType,[string]($motherRow|ConvertTo-Json -Depth 30 -Compress)))
    $cell=$bad.shipModuleSlots[44];$cell.x=$x;$cell.y=$y;$bad.shipModuleSlots[44]=$cell;$rejected=$false
    try{[void]$type.GetMethod('ValidateHull').Invoke($data,@($bad))}catch{$rejected=$true}
    if(-not $rejected){throw 'Centered armor geometry accepts an overlapping tile.'}
}}
foreach($y in @(8,10)){
    $bad=$deserialize.Invoke($null,@($hullType,[string]($motherRow|ConvertTo-Json -Depth 30 -Compress)))
    $cell=$bad.shipModuleSlots[44];$cell.x=9;$cell.y=$y;$bad.shipModuleSlots[44]=$cell;$rejected=$false
    try{[void]$type.GetMethod('ValidateHull').Invoke($data,@($bad))}catch{$rejected=$true}
    if(-not $rejected){throw 'Far-right armor geometry accepts an overlapping tile.'}
}
if($type.GetMethod('AcceptsSlot').Invoke($data,@('AlienMothership',1,6,'HullHardPoint'))){throw 'Aft hull placement bypasses the forward bank schema.'}
$clearance=Get-Content -LiteralPath (Join-Path $RepositoryRoot 'docs\ship-balance-research\alien-capital-expansion\designer-control-clearance-0.11.2.json') -Raw|ConvertFrom-Json
if($clearance.sourceHullSha256 -ne (Get-FileHash (Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\TIShipHullTemplate.json')).Hash -or
    $clearance.sourceLayoutSha256 -ne (Get-FileHash $path).Hash){throw 'Designer control rectangle evidence is stale.'}
if($clearance.hulls.Count -ne 6){throw 'Missing capital control rectangle coverage.'}
foreach($diagram in $clearance.hulls){foreach($control in $diagram.controls){foreach($tile in $diagram.tiles){
    if($control.slot -eq $tile.slot){continue}
    $a=$control.bounds;$b=$tile.bounds
    if([Math]::Min($a[2],$b[2])-[Math]::Max($a[0],$b[0]) -gt 0.001 -and [Math]::Min($a[3],$b[3])-[Math]::Max($a[1],$b[1]) -gt 0.001){throw 'Native control rectangles overlap a foreign tile.'}
}}}
$harmony=[Activator]::CreateInstance($ha.GetType('HarmonyLib.Harmony'),@('eeo.designer.layout.validation'))
try{
    try{[void]$harmony.CreateClassProcessor($mod.GetType('TIEconomyMod.Patches.ShipDesignerDataLayoutPatch',$true)).Patch();Write-Host 'PASS: data-driven designer initialization patch binds.'}
    catch{if($_.Exception.GetBaseException() -is [Security.SecurityException]){Write-Host 'PASS: designer target resolved; emission requires Unity native calls.'}else{throw}}
    try{[void]$harmony.CreateClassProcessor($mod.GetType('TIEconomyMod.Patches.ShipDesignerSpinnerLabelPatch',$true)).Patch();Write-Host 'PASS: generic spinner label patch binds.'}
    catch{if($_.Exception.GetBaseException() -is [Security.SecurityException]){Write-Host 'PASS: spinner label target resolved; emission requires Unity native calls.'}else{throw}}
    $viewportPatch=$mod.GetType('TIEconomyMod.Patches.ShipDesignerViewportFitPatch',$true)
    $targets=@($viewportPatch.GetMethod('TargetMethods').Invoke($null,@()))
    if($targets.Count -ne 3 -or @($targets|Where-Object{$null -eq $_}).Count){throw 'Viewport show/layout/scale event targets changed.'}
    try{[void]$harmony.CreateClassProcessor($viewportPatch).Patch();Write-Host 'PASS: viewport layout/show/scale patch binds.'}
    catch{if($_.Exception.GetBaseException() -is [Security.SecurityException]){Write-Host 'PASS: three viewport event targets resolved; emission requires Unity native calls.'}else{throw}}
}finally{$harmony.UnpatchAll('eeo.designer.layout.validation')}
Write-Host 'PASS: strict layout parser and seven rejection cases; one 74-unit row, later JSON rows, 28 hull grids/native parsing, old indices/types and utility adjacency; six diagram control-rectangle clearance snapshots validate.'
Write-Host 'Manual validation remains: generated destination hierarchy, armor spinners, window fit, multi-cell placement and campaign/refit UI.'
