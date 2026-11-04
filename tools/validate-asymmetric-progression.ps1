[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$TargetManagedDir,
    [Parameter(Mandatory = $true)][string]$ModAssemblyPath,
    [Parameter(Mandatory = $true)][string]$RepositoryRoot
)
$ErrorActionPreference = 'Stop'
function Load-Bytes([string]$Path) { [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path)) }
$ha = Load-Bytes (Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach($file in Get-ChildItem -LiteralPath $TargetManagedDir -Filter '*.dll' | Where-Object {
    $_.Name -like 'Unity*.dll' -or $_.Name -like 'Newtonsoft*.dll' -or $_.Name -eq 'FMODUnity.dll'
}) { [void](Load-Bytes $file.FullName) }
[void](Load-Bytes (Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$game = Load-Bytes (Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
$mod = Load-Bytes $ModAssemblyPath
$policy = $mod.GetType('TIEconomyMod.Core.CapitalMountPolicy', $true)
$precision = $mod.GetType('TIEconomyMod.Core.WavelengthPrecision', $true)
$feature = $mod.GetType('TIEconomyMod.Patches.LaserWavelengthFeature', $true)
$harmony = [Activator]::CreateInstance($ha.GetType('HarmonyLib.Harmony'), @('eeo.asymmetric.validation'))
$unityOnly = @()
try {
    foreach($name in @('ShipTierDescriptionPatch','Size5NoseMountTypePatch','LaserWavelengthMetresPatch','LaserWavelengthSpotPatch','LaserWavelengthDescriptionPatch')) {
        try { [void]$harmony.CreateClassProcessor($mod.GetType('TIEconomyMod.Patches.' + $name, $true)).Patch() }
        catch {
            $cause=$_.Exception.GetBaseException()
            if($cause -is [Security.SecurityException] -and $cause.Message -like 'ECall methods*') { $unityOnly += $name }
            else { throw }
        }
    }
    $tiers=@{
        Gunship=1;Escort=1;Corvette=1;Frigate=2;Monitor=2;Destroyer=2;Cruiser=3;Battlecruiser=3;Battleship=3;Lancer=4;Dreadnought=4;Titan=5
        AlienGunship=1;AlienEscort=1;AlienCorvette=1;AlienFrigate=2;AlienMonitor=2;AlienDestroyer=2;AlienCruiser=2;AlienBattlecruiser=3;AlienBattleship=3;AlienLancer=3;AlienDreadnought=4;AlienTitan=4;AlienAssaultCarrier=4;AlienMothership=5
    }
    $n5=@('SpinalSiegeCoilerMk1','SpinalSiegeCoilerMk2','SpinalSiegeCoilerMk3','AlienRelativisticParticleCannon','Alien1024cmGraserCannon')
    foreach($id in $tiers.Keys) {
        if($policy.GetMethod('ShipTier').Invoke($null,@($id)) -ne $tiers[$id]){throw "Incorrect tier: $id"}
        foreach($weapon in $n5) {
            if($policy.GetMethod('WeaponAllowed').Invoke($null,@($id,$weapon,'FourNose',$false)) -ne ($tiers[$id] -eq 5)){throw "Incorrect grade gate: $id/$weapon"}
            if($policy.GetMethod('WeaponAllowed').Invoke($null,@($id,$weapon,'FourHull',$true))){throw 'N5 accepted as hull weapon.'}
        }
        if($policy.GetMethod('WeaponAllowed').Invoke($null,@($id,'SpinalCoilerMk1','FourNose',$false)) -ne ($tiers[$id] -ne 5)){throw "N4 gate failed: $id"}
    }
    Write-Host 'PASS: 26 asymmetric naval tiers; all five N5 identities restricted to Human Titan/Alien Mothership, N4 excluded there.'

    $templates=Join-Path (Split-Path -Parent $TargetManagedDir) 'StreamingAssets\Templates'
    $audit=Get-Content -LiteralPath (Join-Path $RepositoryRoot 'docs\ship-balance-research\hull-progression-audit-2026-11-03.json') -Raw | ConvertFrom-Json
    $serializer=$game.GetType('StringSerializationAPI',$true).GetMethod('Deserialize')
    foreach($family in @('TIMagneticGunTemplate','TIParticleWeaponTemplate','TILaserWeaponTemplate')) {
        $native=Get-Content -LiteralPath (Join-Path $templates ($family+'.json')) -Raw | ConvertFrom-Json
        $overrides=Get-Content -LiteralPath (Join-Path $RepositoryRoot ('TIEconomyMod\ModFiles\'+$family+'.json')) -Raw | ConvertFrom-Json
        foreach($id in $n5) {
            $base=@($native | Where-Object dataName -eq $id); if($base.Count -eq 0){continue}
            $merged=$base[0].PSObject.Copy();$change=@($overrides | Where-Object dataName -eq $id)
            if($change.Count -ne 1){throw "Missing N5 override: $id"}
            foreach($p in $change[0].PSObject.Properties){$merged | Add-Member -NotePropertyName $p.Name -NotePropertyValue $p.Value -Force}
            $pair=@($audit.sameUnlockSameFamilyPairs | Where-Object {$_.second.name -eq $id})
            if($pair.Count -ne 1){throw "Missing independent baseline: $id"}
            $old=$pair[0].second
            $cycle=[double]$merged.cooldown_s + ([double]$merged.salvo_shots-1)*[double]$merged.intraSalvoCooldown_s
            if([Math]::Abs($cycle*2-$old.cycle_s) -gt 0.00001){throw "Not exactly doubled output: $id/$cycle"}
            foreach($field in @('mass_tons','baseWeaponMass_tons','magazine','salvo_shots','pulseEnergy_MJ','efficiency','modelResource','requiredProjectName')) {
                $oldValue=$base[0].PSObject.Properties[$field];$newValue=$merged.PSObject.Properties[$field]
                if($null -ne $oldValue -and $oldValue.Value -ne $newValue.Value){throw "Per-shot/mass/art/research changed: $id/$field"}
            }
            foreach($fieldPair in @(@('ammoMass_kg','ammoMass_kg'),@('muzzleVelocity_kps','speed_kps'),@('warheadMass_kg','damagingMass_kg'),@('targetingRange_km','range_km'))) {
                $record=$old.PSObject.Properties[$fieldPair[1]]
                if($null -ne $record -and [Math]::Abs([double]$merged.($fieldPair[0])-[double]$record.Value) -gt 0.00001){throw "Historical per-shot baseline changed: $id/$($fieldPair[0])"}
            }
            $instance=$serializer.Invoke($null,@($game.GetType($family,$true),[string]($merged | ConvertTo-Json -Depth 30 -Compress)))
            if($instance.internalSize -ne 4 -or $policy.GetMethod('CapacitySize').Invoke($null,@($id,[int]$instance.internalSize)) -ne 5){throw "N5 native geometry/effective capacity mismatch: $id"}
        }
    }
    Write-Host 'PASS: all five N5 cycles halve against the recorded pre-change baseline; per-shot properties preserved, native geometry 4/effective volume 5.'

    [void]$feature.GetMethod('Validate').Invoke($null,@())
    $laser=$game.GetType('TILaserWeaponTemplate',$true)
    $flags=[Reflection.BindingFlags]'Instance,Public,NonPublic'
    $getter=$laser.GetMethod('get_wavelength_m',$flags)
    $spot=$laser.GetField('_SpotDiameterPreciseFactor_m',$flags)
    $range=$laser.GetField('_damageMinRange',$flags)
    $scores=$game.GetType('TIShipWeaponTemplate',$true).GetField('_combatScoresForRoles',$flags)
    foreach($id in @('Alien768cmGraserCannon','Alien1024cmGraserCannon')) {
        $weapon=$serializer.Invoke($null,@($laser,('{"dataName":"'+$id+'","wavelength_nm":0.1}')))
        if($weapon.wavelength_nm -ne 0){throw 'Native decimal rounding contract changed; reevaluate adapter.'}
        $metres=[single]$getter.Invoke($weapon,@())
        if([Math]::Abs($metres-1e-10) -gt 1e-16){throw "Graser wavelength still truncated: $id/$metres"}
        if($precision.GetMethod('Angstroms').Invoke($null,@($id,0)) -ne 1L){throw 'Exact angstrom storage failed.'}
        $text=$precision.GetMethod('NanometresText').Invoke($null,@($id,0))
        if([decimal]::Parse($text,[Globalization.CultureInfo]::CurrentCulture) -ne [decimal]0.1){throw 'Decimal wavelength display failed.'}
        $spot.SetValue($weapon,[single]123)
        foreach($field in @($range,$scores)) {
            $dict=[Activator]::CreateInstance($field.FieldType)
            $keyType=$field.FieldType.GetGenericArguments()[0]
            $key=if($keyType.IsEnum){[Enum]::ToObject($keyType,0)}else{[single]1}
            $dict.Add($key,[single]42);$field.SetValue($weapon,$dict)
        }
        [void]$feature.GetMethod('Invalidate').Invoke($null,@($weapon))
        if($spot.GetValue($weapon) -ne -1 -or $range.GetValue($weapon).Count -ne 0 -or $scores.GetValue($weapon).Count -ne 0){throw 'Graser stale-cache invalidation failed.'}
    }
    $ordinary=$serializer.Invoke($null,@($laser,'{"dataName":"OrdinaryControl","wavelength_nm":500}'))
    if([Math]::Abs([single]$getter.Invoke($ordinary,@())-5e-7) -gt 1e-12){throw 'Ordinary wavelength changed.'}
    $override=$serializer.Invoke($null,@($laser,'{"dataName":"Alien1024cmGraserCannon","wavelength_nm":2}'))
    if([Math]::Abs([single]$getter.Invoke($override,@())-2e-9) -gt 1e-15){throw 'Nonzero competing override ignored.'}
    Write-Host 'PASS: actual native fractional deserialization reproduced; both graser getters return 1e-10 m, decimal UI conversion and all three caches verified; ordinary/competing values preserved.'
    Write-Host ('PASS: five progression/precision patch targets resolved. Unity-host-only binding exclusions: '+($unityOnly -join ', '))
}
catch { Write-Host $_.ScriptStackTrace; throw }
finally { $harmony.UnpatchAll('eeo.asymmetric.validation') }
