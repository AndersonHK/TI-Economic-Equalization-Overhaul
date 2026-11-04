[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$TargetManagedDir,
    [Parameter(Mandatory = $true)][string]$ModAssemblyPath,
    [Parameter(Mandatory = $true)][string]$RepositoryRoot
)
$ErrorActionPreference = 'Stop'
function Load-AssemblyBytes([string]$Path) {
    return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
}
$harmonyAssembly = Load-AssemblyBytes (Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($file in Get-ChildItem -LiteralPath $TargetManagedDir -Filter '*.dll' | Where-Object {
    $_.Name -like 'Unity*.dll' -or $_.Name -like 'Newtonsoft*.dll' -or $_.Name -eq 'FMODUnity.dll'
}) { [void](Load-AssemblyBytes $file.FullName) }
[void](Load-AssemblyBytes (Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$gameAssembly = Load-AssemblyBytes (Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
$modAssembly = Load-AssemblyBytes $ModAssemblyPath
$layoutData=$modAssembly.GetType('TIEconomyMod.Core.ShipDesignerLayout').GetMethod('Load').Invoke($null,@([string](Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\Config\ship-designer-layout.eeo')))
$modAssembly.GetType('TIEconomyMod.Main').GetField('shipDesignerLayout').SetValue($null,$layoutData)
$harmony = [Activator]::CreateInstance($harmonyAssembly.GetType('HarmonyLib.Harmony'), @('eeo.capital.validation'))
$unityOnly = @()
try {
    $patches = @($modAssembly.GetTypes() | Where-Object {
        $_.Namespace -eq 'TIEconomyMod.Patches' -and $_.Name -like 'Capital*Patch'
    })
    foreach ($patch in $patches) {
        try { [void]$harmony.CreateClassProcessor($patch).Patch() }
        catch {
            $cause = $_.Exception.GetBaseException()
            if ($cause -is [Security.SecurityException] -and $cause.Message -like 'ECall methods*') {
                $unityOnly += $patch.Name
            } else { throw ($patch.Name + ': ' + $_.Exception.ToString()) }
        }
    }
    $hullType = $gameAssembly.GetType('TIShipHullTemplate', $true)
    $templateType = $gameAssembly.GetType('TISpaceShipTemplate', $true)
    $runtime = $modAssembly.GetType('TIEconomyMod.Core.CapitalMountRuntime', $true)
    $flags = [Reflection.BindingFlags]'Static,NonPublic'
    [void]$runtime.GetMethod('InstallSerializer', $flags).Invoke($null, @())
    $serializer = $gameAssembly.GetType('StringSerializationAPI', $true)
    $deserialize = $serializer.GetMethod('Deserialize')
    $serialize = $serializer.GetMethod('SerializeCompressed')
    $getState = $runtime.GetMethod('State', $flags)
    $legacy = $deserialize.Invoke($null, @($templateType, '{"hullName":"Titan","eeoCapitalLayout":0}'))
    $state = $getState.Invoke($null, @($legacy))
    $schemaField = $state.GetType().GetField('Schema', [Reflection.BindingFlags]'Instance,NonPublic')
    if ($schemaField.GetValue($state) -ne 0) { throw 'Legacy schema not preserved.' }
    $schemaField.SetValue($state, [int]1)
    $text = $serialize.Invoke($null, @($templateType, $legacy))
    if ($text -notmatch '"eeoCapitalLayout":1') { throw 'Capital schema missing from exported design.' }
    $copy = $deserialize.Invoke($null, @($templateType, $text))
    if ($schemaField.GetValue($getState.Invoke($null, @($copy))) -ne 1) { throw 'Capital schema failed round trip.' }
    $futureRejected = $false
    try { [void]$deserialize.Invoke($null, @($templateType, '{"hullName":"Titan","eeoCapitalLayout":99}')) }
    catch { $futureRejected = $true }
    if (-not $futureRejected) { throw 'Future schema accepted.' }
    # An earlier ID-only reference must resolve to the same ship with metadata from
    # its later full record. Native TIGameStateConverter swaps the instance here.
    $shipStateType = $gameAssembly.GetType('PavonisInteractive.TerraInvicta.TISpaceShipState', $true)
    $ships = $deserialize.Invoke($null, @($shipStateType.MakeArrayType(), '[{"value":123},{"ID":{"value":123},"eeoCapitalLayout":1}]'))
    if (-not [Object]::ReferenceEquals($ships[0], $ships[1]) -or $schemaField.GetValue($getState.Invoke($null, @($ships[0]))) -ne 1) {
        throw 'Resolved ship reference lost the migration marker.'
    }

    $stockPath = Join-Path (Split-Path -Parent $TargetManagedDir) 'StreamingAssets\Templates\TIShipHullTemplate.json'
    $stock = Get-Content -LiteralPath $stockPath -Raw | ConvertFrom-Json
    $overrides = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\TIShipHullTemplate.json') -Raw | ConvertFrom-Json
    $expected = @{
        Dreadnought = @(10,54,3084,3246); Titan = @(14,58,3672,3846)
        AlienDreadnought = @(13,50,2810,2960); AlienTitan = @(13,60,2880,3060)
        AlienMothership = @(20,100,9240,9540)
        Lancer = @(9,37,2435,2546); AlienAssaultCarrier = @(6,80,3060,3300)
    }
    $mountType = $gameAssembly.GetType('Mount', $true)
    if ($null -eq $mountType) { $mountType = $gameAssembly.GetTypes() | Where-Object Name -eq 'Mount' }
    foreach ($id in $expected.Keys) {
        $original = @($stock | Where-Object dataName -eq $id)[0]
        $override = @($overrides | Where-Object dataName -eq $id)[0]
        $authored = $override
        if ($id -in @('Lancer','AlienAssaultCarrier')) {
            $merged=$original.PSObject.Copy();foreach($prop in $override.PSObject.Properties){$merged|Add-Member -NotePropertyName $prop.Name -NotePropertyValue $prop.Value -Force};$override=$merged
        }
        $crew = if ($id -like 'Alien*') { $original.crew } else { $override.crew }
        if ($id -like 'Alien*' -and $null -ne $authored.crew) { throw "Alien crew override added: $id" }
        if ($override.internalModules -ne $expected[$id][0] -or $crew -ne $expected[$id][1] -or
            $override.mass_tons -ne $expected[$id][2] -or ($override.mass_tons + 3 * $crew) -ne $expected[$id][3]) {
            throw "Capital hull numbers do not reconcile: $id"
        }
        if (@($override.shipModuleSlots | Where-Object moduleSlotType -eq 'Utility').Count -ne $override.internalModules) {
            throw "Utility count disagrees with actual slots: $id"
        }
        for ($i = 0; $i -lt $original.shipModuleSlots.Count; $i++) {
            $oldSlot = $original.shipModuleSlots[$i]; $newSlot = $override.shipModuleSlots[$i]
            if ($oldSlot.moduleSlotType -ne $newSlot.moduleSlotType) { throw "Serialized slot type changed: $id $i" }
            $movable = ($oldSlot.moduleSlotType -like '*Armor') -or ($id -eq 'AlienMothership' -and ($oldSlot.moduleSlotType -ne 'HullHardPoint' -or $i -in @(14,18,19,20))) -or
                ($id -eq 'AlienTitan' -and $oldSlot.moduleSlotType -eq 'Utility' -and $oldSlot.x -in @(5,6,7) -and $oldSlot.y -eq 5) -or
                ($id -in @('Dreadnought','Titan') -and $oldSlot.moduleSlotType -eq 'Utility')
            if (-not $movable -and
                ($oldSlot.x -ne $newSlot.x -or $oldSlot.y -ne $newSlot.y)) {
                throw "Existing slot moved or renumbered: $id $i"
            }
        }
        $coordinates = @($override.shipModuleSlots | ForEach-Object { "$($_.x),$($_.y)" })
        if (($coordinates | Select-Object -Unique).Count -ne $coordinates.Count -or
            @($override.shipModuleSlots | Where-Object { $_.x -lt 0 -or $_.x -ge $layoutData.Columns -or $_.y -lt 0 -or $_.y -ge $layoutData.Rows }).Count) {
            throw "Slot outside native grid or duplicate coordinate: $id"
        }
        $newSlots = @($override.shipModuleSlots | Select-Object -Skip $original.shipModuleSlots.Count)
        foreach ($new in $newSlots) {
            # Armor is drawn half an icon lower by native SetupDesignerLayout.
            if (@($override.shipModuleSlots | Where-Object {
                $visibleY = $_.y + $(if ($_.moduleSlotType -like '*Armor') {1} else {0})
                $_.x -eq $new.x -and [Math]::Abs($visibleY - $new.y) -lt 2
            }).Count -ne 1) {
                throw "New slot overlaps another cell: $id $($new.x),$($new.y)"
            }
        }
        if ($id -in @('Dreadnought','Titan','AlienMothership')) {
            # Preserve every legal footprint by serialized index from both
            # vanilla and the earlier 0.10.0 four-cell extension.
            $legacySlots = @($original.shipModuleSlots)
            if ($id -eq 'AlienMothership') { $legacySlots += @(
                [pscustomobject]@{moduleSlotType='Utility';x=4;y=8}, [pscustomobject]@{moduleSlotType='Utility';x=5;y=8},
                [pscustomobject]@{moduleSlotType='Utility';x=6;y=8}, [pscustomobject]@{moduleSlotType='Utility';x=7;y=8}) }
            $footprints = @(@(@(0,0),@(1,0)), @(@(0,0),@(0,2)), @(@(0,0),@(1,0),@(0,2),@(1,2)))
            $preserved = @($legacySlots | Where-Object moduleSlotType -eq Utility).Count
            for ($anchor=0; $anchor -lt $legacySlots.Count; $anchor++) {
                if ($legacySlots[$anchor].moduleSlotType -ne 'Utility') { continue }
                foreach ($offsets in $footprints) {
                    $indices = @(); $valid = $true
                    foreach ($offset in $offsets) {
                        $targetX = $legacySlots[$anchor].x + $offset[0]; $targetY = $legacySlots[$anchor].y + $offset[1]
                        $index = -1
                        for ($j=0; $j -lt $legacySlots.Count; $j++) {
                            if ($legacySlots[$j].moduleSlotType -eq 'Utility' -and $legacySlots[$j].x -eq $targetX -and $legacySlots[$j].y -eq $targetY) { $index=$j; break }
                        }
                        if ($index -lt 0) { $valid=$false; break }; $indices += $index
                    }
                    if (-not $valid) { continue }
                    for ($k=0; $k -lt $indices.Count; $k++) {
                        $target = $override.shipModuleSlots[$indices[$k]]; $origin = $override.shipModuleSlots[$anchor]
                        if ($target.x -ne $origin.x + $offsets[$k][0] -or $target.y -ne $origin.y + $offsets[$k][1]) { throw "$id utility footprint changed at saved anchor $anchor" }
                    }
                    $preserved++
                }
            }
            # Native armor offsets must not cause the lateral icon to overlap a
            # utility. Require distinct 1-icon-wide/2-half-row-high rectangles.
            $tailX = ($override.shipModuleSlots | Where-Object moduleSlotType -eq TailArmor).x
            $noseX = ($override.shipModuleSlots | Where-Object moduleSlotType -eq NoseArmor).x
            $rects = @($override.shipModuleSlots | ForEach-Object {
                [pscustomobject]@{x=($_.x + $(if($_.moduleSlotType -eq 'LateralArmor' -and ($tailX+$noseX)%2) {0.5} else {0})); y=($_.y + $(if($_.moduleSlotType -like '*Armor') {1} else {0}))}
            })
            for ($a=0; $a -lt $rects.Count; $a++) { for ($b=$a+1; $b -lt $rects.Count; $b++) {
                if ([Math]::Abs($rects[$a].x-$rects[$b].x) -lt 1 -and [Math]::Abs($rects[$a].y-$rects[$b].y) -lt 2) { throw "$id native icons overlap at slots $a/$b" }
            } }
            if ($id -in @('Dreadnought','Titan')) {
                $utilityCoordinates = @($override.shipModuleSlots | Where-Object moduleSlotType -eq Utility | ForEach-Object { "$($_.x),$($_.y)" })
                foreach ($slot in @($override.shipModuleSlots | Where-Object moduleSlotType -eq Utility)) {
                    if ("$($slot.x),$(6-$slot.y)" -notin $utilityCoordinates) { throw "$id utility grid is not symmetric about y=3" }
                }
            }
            Write-Host "PASS: JSON-only $id fits native grid; $preserved prior utility footprints, hull coordinates and all saved slot indices preserved."
        }
        $hull = $deserialize.Invoke($null, @($hullType, [string]($override | ConvertTo-Json -Depth 20 -Compress)))
        if ($id -in @('Lancer','Dreadnought','AlienDreadnought','Titan','AlienTitan','AlienAssaultCarrier','AlienMothership')) {
            $slots = @($hull.shipModuleSlots | Where-Object { $_.moduleSlotType.ToString() -eq 'HullHardPoint' })
            foreach ($slot in $slots) {
                $footprint = $hull.WeaponSlotSet($slot, [Enum]::Parse($mountType, 'FourHull'))
                if ($footprint.Count -ne 1 -or $footprint[0].x -ne $slot.x -or $footprint[0].y -ne $slot.y) { throw "Heavy footprint merged slots: $id" }
                foreach ($small in @('OneHull','TwoHullHoriz','TwoHullVert','ThreeHullHoriz','HalfHull')) {
                    if ($hull.WeaponSlotSet($slot, [Enum]::Parse($mountType, $small)).Count -ne 0) { throw "Small footprint accepted: $id $small" }
                }
            }
            if ($hull.ValidBigWeaponSlotSets([Enum]::Parse($mountType, 'FourHull')).Count -ne $slots.Count) { throw "Missing independent heavy anchors: $id" }
            $noseSlots = @($hull.shipModuleSlots | Where-Object { $_.moduleSlotType.ToString() -eq 'NoseHardPoint' })
            foreach ($noseSlot in $noseSlots) {
                if ($hull.WeaponSlotSet($noseSlot, [Enum]::Parse($mountType, 'FourNose')).Count -ne 1) { throw "Missing independent nose anchor: $id" }
                foreach ($smallNose in @('OneNose','TwoNoseHoriz','TwoNoseVert','ThreeNoseAngle','HalfNose')) {
                    if ($hull.WeaponSlotSet($noseSlot, [Enum]::Parse($mountType, $smallNose)).Count) { throw "Small nose accepted: $id $smallNose" }
                }
            }
            if ($hull.ValidBigWeaponSlotSets([Enum]::Parse($mountType, 'FourNose')).Count -ne $noseSlots.Count) { throw "Missing nose slot sets: $id" }

        }
    }
    # Complete partial hull overrides for later native model/layout fixtures.
    $overrides = @($overrides | ForEach-Object {
        $row=$_
        if($null -eq $row.shipModuleSlots) {
            $full=@($stock|Where-Object dataName -eq $row.dataName)[0].PSObject.Copy()
            foreach($prop in $row.PSObject.Properties){$full|Add-Member -NotePropertyName $prop.Name -NotePropertyValue $prop.Value -Force}
            $full
        } else {$row}
    })
    # Test the real migration planner with a minimal faction and actual module types.
    $manager = $gameAssembly.GetType('PavonisInteractive.TerraInvicta.TemplateManager', $true)
    $add = @($manager.GetMethods() | Where-Object { $_.Name -eq 'Add' -and $_.GetParameters().Count -eq 3 })[0]
    function Add-TestTemplate([Type]$Type, [string]$Json) {
        $item = $deserialize.Invoke($null, @($Type, $Json))
        [void]$add.Invoke($null, @($item, $Type, $true))
        return $item
    }
    $globalType = $gameAssembly.GetType('TIGlobalConfig', $true)
    $global = Add-TestTemplate $globalType '{"dataName":"TIGlobalConfig","alienMasterProject":"Project_AlienMasterProject","alienAdvancedMasterProject":"Project_AlienAdvancedMasterProject"}'
    $self = $manager.GetField('self').GetValue($null)
    $manager.GetField('_global', [Reflection.BindingFlags]'Instance,NonPublic').SetValue($self, $global)
    $factionType = $gameAssembly.GetType('PavonisInteractive.TerraInvicta.TIFactionState', $true)
    $factionTemplateType = $gameAssembly.GetType('TIFactionTemplate', $true)
    $factionTemplate = Add-TestTemplate $factionTemplateType '{"dataName":"CapitalTestFaction","isAlien":false}'
    $faction = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($factionType)
    $gameStateType = $gameAssembly.GetType('PavonisInteractive.TerraInvicta.TIGameState', $true)
    $gameStateType.GetField('template', [Reflection.BindingFlags]'Instance,NonPublic').SetValue($faction, $factionTemplate)
    $faction.templateName = 'CapitalTestFaction'
    $faction.completedProjects = [Activator]::CreateInstance($factionType.GetField('completedProjects').FieldType)
    $titanRow = @($overrides | Where-Object dataName -eq 'Titan')[0]
    $titan = Add-TestTemplate $hullType ([string]($titanRow | ConvertTo-Json -Depth 20 -Compress))
    $gunType = $gameAssembly.GetType('TIMagneticGunTemplate', $true)
    $heavy = Add-TestTemplate $gunType '{"dataName":"HeavyRailgunBatteryMk1","mount":"FourHull","attackMode":true,"requiredProjectName":""}'
    $small = Add-TestTemplate $gunType '{"dataName":"LightRailgunBatteryMk1","mount":"OneHull","attackMode":true,"requiredProjectName":""}'
    $medium = Add-TestTemplate $gunType '{"dataName":"RailgunBatteryMk1","mount":"TwoHullHoriz","attackMode":true,"requiredProjectName":""}'
    $main = $modAssembly.GetType('TIEconomyMod.Main', $true)
    $settings = [Activator]::CreateInstance($modAssembly.GetType('TIEconomyMod.Settings', $true))
    $main.GetField('settings').SetValue($null, $settings)
    $main.GetField('enabled').SetValue($null, $false)
    # Both native catalog views use the same capital availability predicate.
    # Small hull weapons hide only on the three capital hulls; all nose sizes,
    # heavy hull weapons, unrelated parts and other hull classes are preserved.
    $hide = $modAssembly.GetType('TIEconomyMod.Patches.CapitalWeaponAvailabilityPatch').GetMethod('HiddenFromCatalog', $flags)
    foreach ($hullRow in $stock) {
        $catalogDesign = $deserialize.Invoke($null, @($templateType, ('{"hullName":"' + $hullRow.dataName + '"}')))
        foreach ($mountName in @('OneHull','TwoHullHoriz','TwoHullVert','FourHull','OneNose','TwoNoseVert','ThreeNoseAngle','FourNose')) {
            $part = [Activator]::CreateInstance($gunType)
            $part.mount = [Enum]::Parse($mountType, $mountName)
            $expectedHide = $hullRow.dataName -in @('Lancer','Dreadnought','AlienDreadnought','Titan','AlienTitan','AlienAssaultCarrier','AlienMothership') -and ($mountName -notin @('FourHull','FourNose') -or ($hullRow.dataName -in @('Titan','AlienMothership') -and $mountName -eq 'FourNose'))
            if ($hide.Invoke($null, @($catalogDesign,$part)) -ne $expectedHide) { throw "Wrong catalog visibility: $($hullRow.dataName) $mountName" }
        }
        if ($hide.Invoke($null, @($catalogDesign,$titan)) -or $hide.Invoke($null, @($catalogDesign,$null))) { throw 'Capital catalog filter hid an unrelated/null part.' }
    }
    if ($null -ne $modAssembly.GetType('TIEconomyMod.Patches.CapitalUtilityDesignerRowPatch')) {
        throw 'Bespoke designer row patch must be removed from the assembly.'
    }
    Write-Host 'PASS: capital hull and nose catalog filtering accepts only size 4; no bespoke designer-row patch remains.'
    $prepare = $runtime.GetMethod('Prepare', $flags)
    $commit = $runtime.GetMethod('Commit', $flags)
    foreach ($case in @(@('LightRailgunBatteryMk1',1), @('RailgunBatteryMk1',2), @('HeavyRailgunBatteryMk1',4))) {
        # (5,2) is a valid legacy 2x2 anchor in Titan's unchanged slot map.
        $anchor = 20
        $json = '{"dataName":"CapitalTestDesign","factionName":"CapitalTestFaction","hullName":"Titan","moduleTemplateEntries":[],"noseWeaponTemplateEntries":[],"fireModeTemplateEntries":[],"hullWeaponTemplateEntries":[{"moduleName":"' + $case[0] + '","slot":' + $anchor + '}]}'
        $design = $deserialize.Invoke($null, @($templateType, $json))
        $templateType.GetField('_designingFaction', [Reflection.BindingFlags]'Instance,NonPublic').SetValue($design, $faction)
        $plan = $prepare.Invoke($null, @($design))
        $weapons = $plan.GetType().GetField('Weapons', [Reflection.BindingFlags]'Instance,NonPublic').GetValue($plan)
        if ($weapons.Count -ne $case[1] -or @($weapons | Where-Object moduleName -ne 'HeavyRailgunBatteryMk1').Count) { throw "Incorrect expansion for $($case[0])" }
        [void]$commit.Invoke($null, @($plan))
        [void]$runtime.GetMethod('EnsureDesign', $flags).Invoke($null, @($design))
        if ($design.hullWeaponTemplateEntries.Count -ne $case[1]) { throw 'Second migration expanded again.' }
        if ($design.ValidPartForDesign($small)) { throw 'Capital catalog accepted a small weapon.' }
        if (-not $design.ValidAssignedSlotForLocation($heavy, $anchor)) { throw 'Capital rejected heavy anchor.' }
    }
    # Fresh auto-design and AI calls share the heavy-only selection path, even
    # when the caller supplies a mixed list containing an unresearched heavy.
    $faction.obsoletedShipParts = [Activator]::CreateInstance($factionType.GetField('obsoletedShipParts').FieldType)
    [void](Add-TestTemplate ($gameAssembly.GetType('TIProjectTemplate', $true)) '{"dataName":"Project_NotUnlocked"}')
    $locked = Add-TestTemplate $gunType '{"dataName":"CapitalLockedHeavy","mount":"FourHull","attackMode":true,"requiredProjectName":"Project_NotUnlocked"}'
    $weaponType = $gameAssembly.GetType('TIShipWeaponTemplate', $true)
    $scoresField = $weaponType.GetField('_combatScoresForRoles', [Reflection.BindingFlags]'Instance,NonPublic')
    $roleType = $design.role.GetType()
    $design.role = [Enum]::Parse($roleType, 'MM_SpaceSuperiority')
    $scores = $scoresField.GetValue($heavy)
    $scores[$design.role] = [single]100
    $options = [Array]::CreateInstance($weaponType, 3)
    $options[0] = $small; $options[1] = $heavy; $options[2] = $locked
    $ai = $factionType.GetMethod('SetShipDesignHullWeapons', [Reflection.BindingFlags]'Instance,NonPublic')
    foreach ($playerAutoDesign in @($true, $false)) {
        $aiArgs = @($playerAutoDesign, $design, $true, $options)
        [void]$ai.Invoke($faction, $aiArgs)
        $design = $aiArgs[1]
        $expectedWeapons = @($titan.shipModuleSlots | Where-Object { $_.moduleSlotType.ToString() -eq 'HullHardPoint' }).Count
        if ($design.hullWeaponTemplateEntries.Count -ne $expectedWeapons -or
            @($design.hullWeaponTemplateEntries | Where-Object moduleName -ne 'HeavyRailgunBatteryMk1').Count) {
            throw 'Fresh AI/auto-design fitted a small or locked weapon, or failed to fill the independent hull cells.'
        }
    }
    # Reproduce the main-menu failure: stock ships are scored through dummy
    # InitWithTemplate calls before there is any owning faction. Use the actual
    # installed roster layouts and weapon families, including reported Ship20a.
    $templateDirectory = Split-Path -Parent $stockPath
    foreach ($name in @('TITechTemplate','TIProjectTemplate','TIGunTemplate','TIMagneticGunTemplate','TILaserWeaponTemplate','TIParticleWeaponTemplate','TIPlasmaWeaponTemplate','TIMissileTemplate')) {
        $type = $gameAssembly.GetType($name, $true)
        foreach ($row in (Get-Content -LiteralPath (Join-Path $templateDirectory ($name + '.json')) -Raw | ConvertFrom-Json)) {
            if ($row.disable) { continue }
            [void](Add-TestTemplate $type ([string]($row | ConvertTo-Json -Depth 30 -Compress)))
        }
    }
    foreach ($id in @('Lancer','Dreadnought','AlienDreadnought','Titan','AlienTitan','AlienAssaultCarrier','AlienMothership')) {
        $original = @($stock | Where-Object dataName -eq $id)[0]
        $override = @($overrides | Where-Object dataName -eq $id)[0]
        foreach ($property in $override.PSObject.Properties) {
            $original | Add-Member -NotePropertyName $property.Name -NotePropertyValue $property.Value -Force
        }
        [void](Add-TestTemplate $hullType ([string]($original | ConvertTo-Json -Depth 30 -Compress)))
    }
    $rosterText = Get-Content -LiteralPath (Join-Path $templateDirectory 'TISpaceShipTemplate.json') -Raw
    # The authored roster contains trailing commas; use the game's tolerant JSON reader.
    $allStockDesigns = [Newtonsoft.Json.Linq.JArray]::Parse($rosterText).ToString() | ConvertFrom-Json
    $stockDesigns = @($allStockDesigns | Where-Object { $_.hullName -in @('Lancer','Dreadnought','AlienDreadnought','Titan','AlienTitan','AlienAssaultCarrier','AlienMothership') })
    if (-not ($stockDesigns | Where-Object dataName -eq 'Ship20a')) { throw 'Regression fixture no longer contains Ship20a.' }
    $newShipGuard = $modAssembly.GetType('TIEconomyMod.Patches.CapitalNewShipMigrationPatch', $true).GetMethod('Prefix')
    $policy = $modAssembly.GetType('TIEconomyMod.Core.CapitalMountPolicy', $true)
    foreach ($row in $stockDesigns) {
        # Keep real weapon layouts; omit unrelated propulsion preflight, which
        # requires the full Unity host. No faction object is created or assigned.
        $row.driveName = ''; $row.powerPlantName = ''; $row.radiatorName = ''
        $design = Add-TestTemplate $templateType ([string]($row | ConvertTo-Json -Depth 30 -Compress))
        if ($null -ne $design.designingFaction) { throw 'Ownerless regression fixture acquired a faction.' }
        $expectedCells = New-Object 'System.Collections.Generic.HashSet[int]'
        foreach ($entry in $design.hullWeaponTemplateEntries) {
            if ($entry.moduleName -eq 'Empty') { continue }
            if ($null -eq $entry.moduleTemplate.ref_weapon) { throw "Missing stock fixture weapon $($entry.moduleName) in $($row.dataName)" }
            foreach ($cell in $runtime.GetMethod('LegacySlots',$flags).Invoke($null,@($design,$entry,$false))) { [void]$expectedCells.Add($cell) }
        }
        $expectedCount = $expectedCells.Count
        [void]$newShipGuard.Invoke($null, @($design))
        [void]$newShipGuard.Invoke($null, @($design))
        if ($schemaField.GetValue($getState.Invoke($null, @($design))) -ne 3 -or
            $design.hullWeaponTemplateEntries.Count -ne $expectedCount -or
            @($design.hullWeaponTemplateEntries | Where-Object { $_.moduleTemplate.ref_weapon.mount.ToString() -ne 'FourHull' }).Count) {
            throw "Ownerless stock conversion failed or expanded twice: $($row.dataName)"
        }
        # Reading the hydrated list must keep every compact entry.
        if ($design.hullWeapons.Count -ne $expectedCount) { throw "Stock hydration lost weapons: $($row.dataName)" }
    }
    [void]$runtime.GetMethod('NormalizeStockDesigns', $flags).Invoke($null, @())
    $cachedFaction = $templateType.GetField('_designingFaction', [Reflection.BindingFlags]'Instance,NonPublic')
    # Every new capital class has independent size-4 nose anchors. Test native
    # model dispatch without Unity scene calls, including the two appended slots.
    foreach ($id in @('Lancer','Dreadnought','AlienDreadnought','Titan','AlienTitan','AlienAssaultCarrier','AlienMothership')) {
        $row = @($overrides | Where-Object dataName -eq $id)[0]
        $modelMethod = $modAssembly.GetType('TIEconomyMod.Patches.CapitalWeaponModelAnchorPatch').GetMethod('TargetMethods').Invoke($null,@()) |
            Where-Object { $_.DeclaringType.Name -eq ($id + 'Controller') } | Select-Object -First 1
        $modelType = $modelMethod.DeclaringType
        $model = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($modelType)
        $indices = @()
        for ($slotIndex=0; $slotIndex -lt $row.shipModuleSlots.Count; ++$slotIndex) {
            if ($row.shipModuleSlots[$slotIndex].moduleSlotType -ne 'NoseHardPoint') { continue }
            $indices += $modelType.GetMethod('SlotToWeaponMountIndex').Invoke($model, @($slotIndex, [Enum]::Parse($mountType,'FourNose')))
        }
        if (($indices | Select-Object -Unique).Count -ne $indices.Count) { throw "Shared capital nose model controllers: $id" }
        if ($id -eq 'AlienMothership') {
            if(($indices -join ',') -ne '4,2,1,3,5,6,7,8'){throw 'Mothership new nose slots have wrong model indices.'}
            $hullIndices=@()
            for($i=0;$i -lt $row.shipModuleSlots.Count;++$i){if($row.shipModuleSlots[$i].moduleSlotType -eq 'HullHardPoint'){
                $hullIndices+=$modelType.GetMethod('SlotToWeaponMountIndex').Invoke($model,@($i,[Enum]::Parse($mountType,'FourHull')))
            }}
            if($hullIndices.Count -ne 18 -or ($hullIndices|Select-Object -Unique).Count -ne 18 -or (($hullIndices|Select-Object -Last 2) -join ',') -ne '8,11'){throw 'Mothership extra hull model anchors overlap or select the wrong dormant pair.'}
        }
    }
    # Cover each legacy footprint; schema 1 preserves already-compact Titan hull
    # equipment but must expand dreadnought hulls (not compact before schema 2).
    $validLayout = $runtime.GetMethod('ValidLayout', $flags)
    foreach ($case in @(
        @('Titan',0,'LightRailCannonMk1','OneNose',7,1),
        @('Titan',0,'RailCannonMk1','TwoNoseVert',8,2),
        @('Titan',0,'RailCannonMk1','TwoNoseHoriz',9,2),
        @('Titan',0,'HeavyRailCannonMk1','ThreeNoseAngle',9,3),
        @('Titan',0,'SpinalRailgunMk1','FourNose',8,4),
        @('Titan',1,'SpinalRailgunMk1','FourNose',8,4),
        @('AlienMothership',0,'AlienSpinalMagCannon','FourNose',9,4),
        @('AlienMothership',1,'AlienSpinalMagCannon','FourNose',9,4)
    )) {
        $noseWeapon = Add-TestTemplate $gunType ('{"dataName":"' + $case[2] + '","mount":"' + $case[3] + '","attackMode":true,"requiredProjectName":""}')
        $json = '{"dataName":"NoseFootprintTest","hullName":"' + $case[0] + '","eeoCapitalLayout":' + $case[1] + ',"moduleTemplateEntries":[],"fireModeTemplateEntries":[],"hullWeaponTemplateEntries":[],"noseWeaponTemplateEntries":[{"moduleName":"' + $case[2] + '","slot":' + $case[4] + '}]}'
        $noseDesign = $deserialize.Invoke($null,@($templateType,$json))
        # Force the normal researched path without external campaign backups.
        $cachedFaction.SetValue($noseDesign,$faction)
        $plan = $prepare.Invoke($null,@($noseDesign))
        [void]$commit.Invoke($null,@($plan))
        if ($noseDesign.noseWeaponTemplateEntries.Count -ne $case[5] -or -not $validLayout.Invoke($null,@($noseDesign))) { throw "Nose migration failed: $($case -join '/')" }
        $noseSlots = $noseDesign.noseWeaponTemplateEntries.slot -join ','
        [void]$runtime.GetMethod('EnsureDesign',$flags).Invoke($null,@($noseDesign))
        if (($noseDesign.noseWeaponTemplateEntries.slot -join ',') -ne $noseSlots) { throw 'Nose conversion expanded twice.' }
    }
    foreach ($id in @('Titan','AlienTitan','AlienMothership','Dreadnought','AlienDreadnought')) {
        $row = @($overrides | Where-Object dataName -eq $id)[0]
        $anchor = -1
        for ($k=0;$k -lt $row.shipModuleSlots.Count;++$k) {
            $a = $row.shipModuleSlots[$k]
            if ($a.moduleSlotType -eq 'HullHardPoint' -and @($row.shipModuleSlots | Where-Object { $_.moduleSlotType -eq 'HullHardPoint' -and $_.x -eq ($a.x+1) -and $_.y -eq $a.y }).Count) { $anchor=$k; break }
        }
        $json = '{"dataName":"SchemaOneHull","hullName":"'+$id+'","eeoCapitalLayout":1,"moduleTemplateEntries":[],"noseWeaponTemplateEntries":[],"fireModeTemplateEntries":[],"hullWeaponTemplateEntries":[{"moduleName":"RailgunBatteryMk1","slot":'+$anchor+'}]}'
        $oldDesign=$deserialize.Invoke($null,@($templateType,$json))
        $plan=$prepare.Invoke($null,@($oldDesign)); [void]$commit.Invoke($null,@($plan))
        $expectedHullCount=if($id -like '*Dreadnought' -or $id -in @('Lancer','AlienAssaultCarrier')){2}else{1}
        if($oldDesign.hullWeaponTemplateEntries.Count -ne $expectedHullCount){throw "Schema-1 hull expansion wrong: $id"}
    }
    # Both manual assignment and the multi-mount core selector retain the actual
    # dropped cell, even on the last free cell (native selector otherwise shifts).
    $noseHeavy=Add-TestTemplate $gunType '{"dataName":"CapitalAiSpinal","mount":"FourNose","attackMode":true,"requiredProjectName":""}'
    $noseSmall=Add-TestTemplate $gunType '{"dataName":"CapitalAiSmallNose","mount":"OneNose","attackMode":true,"requiredProjectName":""}'
    $noseLocked=Add-TestTemplate $gunType '{"dataName":"CapitalAiLockedNose","mount":"FourNose","attackMode":true,"requiredProjectName":"Project_NotUnlocked"}'
    $coreMethod=$hullType.GetMethod('AssignCoreSlotOnMultiMountPlacement')
    $noseAi=$factionType.GetMethod('SetShipDesignNoseWeapons')
    $nose5=Add-TestTemplate $gunType '{"dataName":"SpinalSiegeCoilerMk2","mount":"FourNose","attackMode":true,"requiredProjectName":""}'
    $nose5Locked=Add-TestTemplate $gunType '{"dataName":"SpinalSiegeCoilerMk1","mount":"FourNose","attackMode":true,"requiredProjectName":"Project_NotUnlocked"}'

    foreach($id in @('Lancer','Dreadnought','AlienDreadnought','Titan','AlienTitan','AlienAssaultCarrier','AlienMothership')) {
        $aiDesign=$deserialize.Invoke($null,@($templateType,('{"dataName":"CapitalNoseAI","hullName":"'+$id+'","eeoCapitalLayout":3,"noseWeaponTemplateEntries":[],"hullWeaponTemplateEntries":[],"moduleTemplateEntries":[],"fireModeTemplateEntries":[]}')))
        $cachedFaction.SetValue($aiDesign,$faction)
        $chosenNose=if($id -in @('Titan','AlienMothership')){$nose5}else{$noseHeavy}
        $lockedNose=if($id -in @('Titan','AlienMothership')){$nose5Locked}else{$noseLocked}
        $noseOptions=[Array]::CreateInstance($weaponType,3);$noseOptions[0]=$noseSmall;$noseOptions[1]=$chosenNose;$noseOptions[2]=$lockedNose
        foreach($roleName in @('MM_SpaceSuperiority','LM_Protector')) {
            $aiDesign.role=[Enum]::Parse($roleType,$roleName)
            $scoresField.GetValue($chosenNose)[$aiDesign.role]=[single]100
            foreach($auto in @($true,$false)) {
                $aiArgs=@($auto,$aiDesign,$true,$noseOptions);[void]$noseAi.Invoke($faction,$aiArgs)
                $expectedNose=@($aiDesign.hullTemplate.shipModuleSlots|Where-Object {$_.moduleSlotType.ToString() -eq 'NoseHardPoint'}).Count
                if($aiDesign.noseWeaponTemplateEntries.Count -ne $expectedNose -or @($aiDesign.noseWeaponTemplateEntries|Where-Object moduleName -ne $chosenNose.dataName).Count){throw "Nose AI fitted small/locked weapons or left cells empty: $id/$roleName"}
            }
        }
        if($aiDesign.noseWeaponTemplateEntries.Count -eq 0){continue}
        $last=$aiDesign.noseWeaponTemplateEntries[-1].slot
        if(-not $aiDesign.ValidAssignedSlotForLocation($chosenNose,$last) -or $aiDesign.ValidAssignedSlotForLocation($noseSmall,$last)){throw "Nose assignment policy wrong: $id"}
        $selected=$coreMethod.Invoke($null,@($aiDesign,$chosenNose,$last))
        if($aiDesign.hullTemplate.slotIndex($selected) -ne $last){throw "Capital drop shifted to another cell: $id"}
    }
    Write-Host 'PASS: five capital model maps, six mothership nose anchors, all legacy nose footprints, schema-1 hull compatibility, independent core selection, both AI modes and Protector-role nose fitting.'
    # Built ships must retain fractional ammo, damage, repairs and reload plans
    # across simultaneous hull and nose conversion. New empty cells stay empty.
    $oldNose=Add-TestTemplate $gunType '{"dataName":"CoilCannonMk1","mount":"TwoNoseVert","attackMode":true,"magazine":100,"requiredProjectName":""}'
    [void](Add-TestTemplate $gunType '{"dataName":"SpinalSiegeCoilerMk1","mount":"FourNose","attackMode":true,"magazine":200,"requiredProjectName":""}')
    $oldHull=Add-TestTemplate $gunType '{"dataName":"HeavyRailgunBatteryMk1","mount":"FourHull","attackMode":true,"magazine":100,"requiredProjectName":""}'
    $builtDesign=$deserialize.Invoke($null,@($templateType,'{"dataName":"BuiltCapitalConversion","hullName":"Titan","eeoCapitalLayout":1,"moduleTemplateEntries":[],"fireModeTemplateEntries":[],"noseWeaponTemplateEntries":[{"moduleName":"CoilCannonMk1","slot":8}],"hullWeaponTemplateEntries":[{"moduleName":"HeavyRailgunBatteryMk1","slot":20}]}'))
    $builtPlan=$prepare.Invoke($null,@($builtDesign))
    $built=$deserialize.Invoke($null,@($shipStateType,'{"ID":{"value":987654},"noseWeapons":[],"hullWeapons":[],"utilityModules":[],"ammo":{},"damagedParts":[],"prevPartsBeingRepaired":[],"plannedResupplyAndRepair":{"modulesToRepair":[],"ammoToReload":{}}}'))
    $gameStateType.GetField('template',[Reflection.BindingFlags]'Instance,NonPublic').SetValue($built,$builtDesign)
    $moduleType=$gameAssembly.GetType('ModuleDataEntry',$true)
    $damageType=$gameAssembly.GetType('PavonisInteractive.TerraInvicta.DamagedShipPartData',$true)
    $noseEntry=[Activator]::CreateInstance($moduleType,@($oldNose,8))
    $hullEntry=[Activator]::CreateInstance($moduleType,@($oldHull,20))
    $built.noseWeapons.Add($noseEntry);$built.hullWeapons.Add($hullEntry)
    $built.ammo.Add($noseEntry,25);$built.ammo.Add($hullEntry,40)
    $built.plannedResupplyAndRepair.ammoToReload.Add($noseEntry,10)
    $built.damagedParts.Add([Activator]::CreateInstance($damageType,@($noseEntry,[single]0.375)))
    $built.plannedResupplyAndRepair.modulesToRepair.Add([Activator]::CreateInstance($damageType,@($noseEntry,[single]0.5)))
    $built.prevPartsBeingRepaired.Add([Activator]::CreateInstance($damageType,@($noseEntry,[single]0.25)))
    $shipPlan=$runtime.GetMethod('PrepareShip',$flags).Invoke($null,@($built,$builtPlan))
    if($built.noseWeapons.Count -ne 1 -or $built.ammo[$noseEntry] -ne 25){throw 'Built-ship preflight mutated live equipment.'}
    [void]$commit.Invoke($null,@($builtPlan))
    [void]$runtime.GetMethod('CommitShip',$flags).Invoke($null,@($shipPlan))
    if($built.noseWeapons.Count -ne 2 -or $built.hullWeapons.Count -ne 1 -or $built.ammo.ContainsKey($noseEntry)){throw 'Built-ship weapon banks or old-ammo cleanup failed.'}
    foreach($entry in $built.noseWeapons){
        if($built.ammo[$entry] -ne 50 -or $built.plannedResupplyAndRepair.ammoToReload[$entry] -ne 20){throw 'Nose ammo/reload fraction was not preserved.'}
        if(@($built.damagedParts|Where-Object {$_.module.Equals($entry) -and $_.damage -eq [single]0.375}).Count -ne 1 -or
           @($built.plannedResupplyAndRepair.modulesToRepair|Where-Object {$_.module.Equals($entry) -and $_.damage -eq [single]0.5}).Count -ne 1 -or
           @($built.prevPartsBeingRepaired|Where-Object {$_.module.Equals($entry) -and $_.damage -eq [single]0.25}).Count -ne 1){throw 'Nose damage/repair state was not preserved.'}
    }
    if($built.ammo[$built.hullWeapons[0]] -ne 40 -or $schemaField.GetValue($getState.Invoke($null,@($built))) -ne 3){throw 'Schema-1 hull ammo or built-ship schema was not preserved.'}
    Write-Host 'PASS: built-ship preflight is atomic; simultaneous hull/nose migration preserves ammunition fractions, reload requests, damage, current/prior repair plans and empty cells.'
    # Schema-2 ships already use one cell per weapon; never expand them again.
    foreach($id in @('Dreadnought','AlienDreadnought','Titan','AlienTitan','AlienMothership')) {
        $row=@($overrides|Where-Object dataName -eq $id)[0]
        $anchor=0;while($row.shipModuleSlots[$anchor].moduleSlotType -ne 'NoseHardPoint'){$anchor++}
        $name=if($id -like 'Alien*'){'AlienSpinalParticleCannon'}else{'SpinalCoilerMk2'}
        $fixture=$deserialize.Invoke($null,@($templateType,('{"dataName":"SchemaTwoSingleCell","hullName":"'+$id+'","eeoCapitalLayout":2,"hullWeaponTemplateEntries":[],"noseWeaponTemplateEntries":[{"moduleName":"'+$name+'","slot":'+$anchor+'}],"fireModeTemplateEntries":[],"moduleTemplateEntries":[]}')))
        $plan=$prepare.Invoke($null,@($fixture));[void]$commit.Invoke($null,@($plan))
        if($fixture.noseWeaponTemplateEntries.Count -ne 1 -or $fixture.noseWeaponTemplateEntries[0].slot -ne $anchor -or -not $validLayout.Invoke($null,@($fixture))){throw "Schema-2 nose expanded or invalid: $id"}
    }
    $lancer=$deserialize.Invoke($null,@($templateType,'{"dataName":"SchemaTwoLancer","hullName":"Lancer","eeoCapitalLayout":2,"moduleTemplateEntries":[],"fireModeTemplateEntries":[],"hullWeaponTemplateEntries":[{"moduleName":"LightRailgunBatteryMk1","slot":11}],"noseWeaponTemplateEntries":[{"moduleName":"SpinalRailgunMk1","slot":8}]}'))
    $plan=$prepare.Invoke($null,@($lancer));[void]$commit.Invoke($null,@($plan))
    if($lancer.hullWeaponTemplateEntries.Count -ne 1 -or $lancer.noseWeaponTemplateEntries.Count -ne 4 -or -not $validLayout.Invoke($null,@($lancer))){throw 'Human Lancer schema-2 conversion failed.'}
    $carrierRow=@($overrides|Where-Object dataName -eq 'AlienAssaultCarrier')[0]
    $carrierAnchor=-1
    for($i=0;$i -lt $carrierRow.shipModuleSlots.Count;++$i){
        $a=$carrierRow.shipModuleSlots[$i];if($a.moduleSlotType -ne 'HullHardPoint'){continue}
        $complete=$true
        foreach($offset in @(@(0,0),@(1,0))){if(-not @($carrierRow.shipModuleSlots|Where-Object {$_.moduleSlotType -eq 'HullHardPoint' -and $_.x -eq ($a.x+$offset[0]) -and $_.y -eq ($a.y+$offset[1])}).Count){$complete=$false;break}}
        if($complete){$carrierAnchor=$i;break}
    }
    if($carrierAnchor -lt 0){throw 'Carrier fixture lost its native two-cell horizontal hull footprint.'}
    [void](Add-TestTemplate $gunType '{"dataName":"RailgunBatteryMk1","mount":"TwoHullHoriz","attackMode":true,"requiredProjectName":""}')
    $carrier=$deserialize.Invoke($null,@($templateType,('{"dataName":"SchemaTwoCarrier","hullName":"AlienAssaultCarrier","eeoCapitalLayout":2,"moduleTemplateEntries":[],"fireModeTemplateEntries":[],"noseWeaponTemplateEntries":[],"hullWeaponTemplateEntries":[{"moduleName":"RailgunBatteryMk1","slot":'+$carrierAnchor+'}]}')))
    $plan=$prepare.Invoke($null,@($carrier));[void]$commit.Invoke($null,@($plan))
    if($carrier.hullWeaponTemplateEntries.Count -ne 2 -or $carrier.noseWeaponTemplateEntries.Count -ne 0 -or -not $validLayout.Invoke($null,@($carrier))){throw 'Carrier schema-2 conversion failed or added noses.'}
    # Lower-tier native fitting must retain a shared FourNose footprint, not
    # acquire four independent weapons when an old N5 identity is normalized.
    $native=$deserialize.Invoke($null,@($templateType,'{"dataName":"NativeAlienLancer","hullName":"AlienLancer","eeoCapitalLayout":2,"moduleTemplateEntries":[],"fireModeTemplateEntries":[],"hullWeaponTemplateEntries":[],"noseWeaponTemplateEntries":[{"moduleName":"AlienRelativisticParticleCannon","slot":7}]}'))
    $alienLancerRow=@($stock|Where-Object dataName -eq 'AlienLancer')[0]
    [void](Add-TestTemplate $hullType ([string]($alienLancerRow|ConvertTo-Json -Depth 30 -Compress)))
    $plan=$prepare.Invoke($null,@($native));[void]$commit.Invoke($null,@($plan))
    if($native.noseWeaponTemplateEntries.Count -ne 1 -or $native.noseWeaponTemplateEntries[0].moduleName -ne 'AlienSpinalParticleCannon' -or -not $validLayout.Invoke($null,@($native))){throw 'Lower-tier N5 downgrade changed native footprint or pair.'}
    $mixed=[Array]::CreateInstance($weaponType,2);$mixed[0]=$native.noseWeaponTemplateEntries[0].moduleTemplate.ref_weapon;$mixed[1]=$nose5
    $filterArgs=@($faction,$true,$native,$true,$mixed)
    $filter=$modAssembly.GetType('TIEconomyMod.Patches.CapitalAiNoseWeaponsPatch').GetMethod('Prefix')
    $delegates=$filter.Invoke($null,$filterArgs);$nativeChoices=@($filterArgs[4])
    if(-not $delegates -or $nativeChoices.Count -ne 1 -or $nativeChoices[0].dataName -ne 'AlienSpinalParticleCannon'){throw 'Native lower-tier AI choices leaked N5 or lost native delegation.'}
    Write-Host 'PASS: schema-2 compact noses remain single-cell; Human Lancer expands its old nose; native lower-tier grade normalization retains shared geometry.'
    # Save import has its own setter, bypassing both faction initialization and
    # the designer. Reproduce the reported six-phaser Titan with a positive
    # saved score, which otherwise prevents scoring from reaching a dummy ship.
    # Intercept backup IO only: validation must not create fake user backups.
    Add-Type -TypeDefinition @'
public static class CapitalImportBackupProbe {
    public static int Calls;
    public static bool Prefix() { Calls++; return false; }
}
'@
    $probeMethod = [CapitalImportBackupProbe].GetMethod('Prefix')
    $reportHook = [Activator]::CreateInstance($harmonyAssembly.GetType('HarmonyLib.HarmonyMethod'), @($probeMethod))
    [void]$harmony.Patch($runtime.GetMethod('WriteReport', $flags), $reportHook, $null, $null, $null)
    $importPatch = $modAssembly.GetType('TIEconomyMod.Patches.SkirmishImportedShipCacheInvalidationPatch', $true)
    [void]$harmony.CreateClassProcessor($importPatch).Patch()
    $importGuard = $importPatch.GetMethod('Prefix')
    $importListType = $importGuard.GetParameters()[0].ParameterType
    $imported = $deserialize.Invoke($null, @($templateType, @'
{"dataName":"CapitalImportedSixPhasers","factionName":"AppeaseCouncil","hullName":"Titan","propellantTanks":52,"refitIteration":5,"moduleTemplateEntries":[],"fireModeTemplateEntries":[],"noseWeaponTemplateEntries":[{"moduleName":"SpinalCoilerMk3","slot":8}],"hullWeaponTemplateEntries":[{"moduleName":"60cmIRPhaserBattery","slot":20},{"moduleName":"60cmIRPhaserBattery","slot":16},{"moduleName":"60cmIRPhaserBattery","slot":12},{"moduleName":"60cmIRPhaserBattery","slot":21},{"moduleName":"60cmIRPhaserBattery","slot":17},{"moduleName":"60cmIRPhaserBattery","slot":13}]}
'@))
    $scoreField = $templateType.GetField('_combatValue', [Reflection.BindingFlags]'Instance,NonPublic')
    $rawScoreField = $templateType.GetField('_unnormalizedCombatValue', [Reflection.BindingFlags]'Instance,NonPublic')
    $scoreField.SetValue($imported, [single]58)
    $rawScoreField.SetValue($imported, [single]580)
    if ($imported.hullWeapons.Count -ne 6) { throw 'Imported fixture failed to hydrate its old cached weapons.' }
    $imports = [Activator]::CreateInstance($importListType)
    $imports.Add($imported)
    $nonCapital = $deserialize.Invoke($null, @($templateType, '{"hullName":"Frigate","hullWeaponTemplateEntries":[]}'))
    $scoreField.SetValue($nonCapital, [single]11)
    $imports.Add($nonCapital)
    [void]$importGuard.Invoke($null, [object[]]@(,$imports))
    if ($schemaField.GetValue($getState.Invoke($null, @($imported))) -ne 3 -or
        $imported.hullWeapons.Count -ne 6 -or
        @($imported.hullWeaponTemplateEntries | Where-Object moduleName -ne '360cmIRPhaserBattery').Count -or
        ($imported.hullWeaponTemplateEntries.slot -join ',') -ne '20,16,12,21,17,13' -or
        $scoreField.GetValue($imported) -ne -1 -or $rawScoreField.GetValue($imported) -ne -1) {
        throw 'Save import failed to replace and invalidate the cached six-phaser layout before preview.'
    }
    if ($imported.propellantTanks -ne 52 -or $imported.refitIteration -ne 5 -or
        $imported.noseWeaponTemplateEntries.Count -ne 4 -or
        $imported.noseWeaponTemplateEntries[0].moduleName -ne 'SpinalSiegeCoilerMk3' -or
        ($imported.noseWeaponTemplateEntries.slot -join ',') -ne '8,9,7,10') { throw 'Import altered fuel/refit identity or failed to expand the four-cell nose.' }
    if ([CapitalImportBackupProbe]::Calls -ne 1 -or
        $scoreField.GetValue($nonCapital) -ne 11 -or
        $schemaField.GetValue($getState.Invoke($null, @($nonCapital))) -ne 0) { throw 'Import skipped the ownerless backup or altered a non-capital design.' }
    # Unity owns the actual score calculation. Supply its completed preview
    # cache, then test the real cached-score read across repeated import and the
    # startup guard. A second invalidation here causes the reported early ECM
    # lookup during fleet creation, when faction effects are still unavailable.
    $scoreField.SetValue($imported, [single]123)
    $rawScoreField.SetValue($imported, [single]1230)
    [void]$importGuard.Invoke($null, [object[]]@(,$imports))
    $templateType.GetField('_designingFaction', [Reflection.BindingFlags]'Instance,NonPublic').SetValue($imported, $faction)
    [void]$newShipGuard.Invoke($null, @($imported))
    if ($imported.TemplateSpaceCombatValue($false, [single]-1, [single]1, $false) -ne 123 -or
        $rawScoreField.GetValue($imported) -ne 1230 -or
        $imported.hullWeapons.Count -ne 6 -or [CapitalImportBackupProbe]::Calls -ne 1) {
        throw 'Repeated import/startup expanded migrated weapons or invalidated the completed preview score.'
    }
    Write-Host 'PASS: save-import hook converts six 60cm phasers before preview, requests an ownerless backup, and expands nose cells and preserves fuel/non-capitals; repeated import and startup preserve converted weapons and a completed combat-score cache.'
    # On return from combat the retained imported design has a cached faction,
    # but GameStateManager has torn down effects. Reproduce the native crash,
    # then exercise the same roster cleanup used before main-menu scoring.
    $menuRuntime = $modAssembly.GetType('TIEconomyMod.Patches.SkirmishDropdownCacheRuntime', $true)
    $prepareMenu = $menuRuntime.GetMethod('PrepareMenuDesigns', $flags)
    $cachedFaction = $templateType.GetField('_designingFaction', [Reflection.BindingFlags]'Instance,NonPublic')
    $ecmFailed = $false
    try { [void]$imported.ECMValue($true, $null) }
    catch {
        if ($_.Exception.GetBaseException() -is [NullReferenceException]) { $ecmFailed = $true } else { throw }
    }
    if (-not $ecmFailed) { throw 'Return-to-menu fixture no longer reproduces the stale-faction ECM crash.' }
    # A live state collection must prevent cleanup during campaign/skirmish load.
    $stateManager = $gameAssembly.GetType('PavonisInteractive.TerraInvicta.GameStateManager', $true)
    $stateDictionaryField = $stateManager.GetField('gamestates', [Reflection.BindingFlags]'Static,Public,NonPublic')
    $stateDictionary = $stateDictionaryField.GetValue($null)
    $savedDictionary = $stateDictionary
    $liveDictionary = [Activator]::CreateInstance($stateDictionary.GetType())
    $liveDictionary.Add($factionType, [Activator]::CreateInstance($stateDictionary.GetType().GetGenericArguments()[1]))
    try {
        $stateDictionaryField.SetValue($null, $liveDictionary)
        [void]$prepareMenu.Invoke($null, [object[]]@(,$imports))
        if (-not [Object]::ReferenceEquals($cachedFaction.GetValue($imported), $faction) -or
            $scoreField.GetValue($imported) -ne 123) { throw 'Menu cleanup altered an active game-state binding or score.' }
    } finally { $stateDictionaryField.SetValue($null, $savedDictionary) }
    [void]$prepareMenu.Invoke($null, [object[]]@(,$imports))
    if ($null -ne $cachedFaction.GetValue($imported) -or $scoreField.GetValue($imported) -ne -1 -or
        $imported.ECMValue($true, $null) -ne 0 -or
        $schemaField.GetValue($getState.Invoke($null, @($imported))) -ne 3 -or
        $imported.hullWeapons.Count -ne 6 -or $imported.propellantTanks -ne 52) {
        throw 'Return-to-menu cleanup failed to detach the old faction, refresh caches or preserve the converted ship.'
    }
    $scoreField.SetValue($imported, [single]124)
    [void]$prepareMenu.Invoke($null, [object[]]@(,$imports))
    if ($scoreField.GetValue($imported) -ne 124) { throw 'Menu cleanup invalidated an already-detached design again.' }
    Write-Host 'PASS: native ECM stale-faction crash reproduced and corrected after teardown; active-state bindings, migrated equipment and repeat caches are preserved.'
    # A missile's global-tech prerequisites admit a peer heavy engineering
    # project, but not a future branch, unknown root or cyclic project chain.
    $techType = $gameAssembly.GetType('TITechTemplate', $true)
    $projectType = $gameAssembly.GetType('TIProjectTemplate', $true)
    [void](Add-TestTemplate $techType '{"dataName":"CapitalSharedTech"}')
    [void](Add-TestTemplate $techType '{"dataName":"CapitalFutureTech"}')
    foreach ($json in @(
        '{"dataName":"CapitalMissileProject","prereqs":["CapitalSharedTech"]}',
        '{"dataName":"CapitalHeavyProject","prereqs":["CapitalSharedTech"]}',
        '{"dataName":"CapitalFutureProject","prereqs":["CapitalFutureTech"]}',
        '{"dataName":"CapitalUnknownRoot"}',
        '{"dataName":"CapitalAlternativeProject","prereqs":["CapitalFutureTech","CapitalFutureTech"],"altPrereq0":"CapitalSharedTech"}',
        '{"dataName":"CapitalCycleA","prereqs":["CapitalCycleB"]}',
        '{"dataName":"CapitalCycleB","prereqs":["CapitalCycleA"]}'
    )) { [void](Add-TestTemplate $projectType $json) }
    [void](Add-TestTemplate $hullType '{"dataName":"CapitalInferenceHull","alien":false}')
    $sourceMissile = Add-TestTemplate ($gameAssembly.GetType('TIMissileTemplate', $true)) '{"dataName":"CapitalSourceMissile","mount":"OneHull","requiredProjectName":"CapitalMissileProject"}'
    [void](Add-TestTemplate $gunType '{"dataName":"CapitalPeerCoilgunMk1","mount":"FourHull","attackMode":true,"requiredProjectName":"CapitalHeavyProject"}')
    [void](Add-TestTemplate $gunType '{"dataName":"CapitalFutureCoilgunMk3","mount":"FourHull","attackMode":true,"requiredProjectName":"CapitalFutureProject"}')
    [void](Add-TestTemplate $gunType '{"dataName":"CapitalRootCoilgunMk3","mount":"FourHull","attackMode":true,"requiredProjectName":"CapitalUnknownRoot"}')
    [void](Add-TestTemplate $gunType '{"dataName":"CapitalCycleCoilgunMk3","mount":"FourHull","attackMode":true,"requiredProjectName":"CapitalCycleA"}')
    [void](Add-TestTemplate $gunType '{"dataName":"CapitalAlternativeCoilgunMk3","mount":"FourHull","attackMode":true,"requiredProjectName":"CapitalAlternativeProject"}')
    $inferredDesign = $deserialize.Invoke($null, @($templateType, '{"dataName":"CapitalInferenceDesign","hullName":"CapitalInferenceHull","noseWeaponTemplateEntries":[],"hullWeaponTemplateEntries":[{"moduleName":"CapitalSourceMissile","slot":0}]}'))
    $eligible = $runtime.GetMethod('Eligible', $flags).Invoke($null, @($inferredDesign, $false))
    $replacement = $runtime.GetMethod('Replacement', $flags).Invoke($null, @($sourceMissile, $eligible, $false, $false))
    if ($replacement.dataName -ne 'CapitalPeerCoilgunMk1' -or
        @($eligible | Where-Object { $_.dataName -in @('CapitalFutureCoilgunMk3','CapitalRootCoilgunMk3','CapitalCycleCoilgunMk3','CapitalAlternativeCoilgunMk3') }).Count) {
        throw 'Stock technology inference selected an unsupported branch or missed a peer project.'
    }
    # The Mothership geometry catalog is validated without invoking Unity scene
    # APIs. Native controller lifecycle/aiming remains an in-game acceptance item.
    $geometry = $modAssembly.GetType('TIEconomyMod.Core.MothershipMountGeometry', $true)
    $noseGeometry = Get-Content -Raw (Join-Path $RepositoryRoot 'docs\ship-balance-research\capital-nose-geometry-2026-11-02.json') | ConvertFrom-Json
    if ($noseGeometry.rows.Count -ne 2) { throw 'Expected exactly two measured new Mothership nose anchors.' }
    for ($i=0;$i -lt 2;++$i) {
        $point=$geometry.GetMethod('AddedNosePosition').Invoke($null,@($i))
        $target=$noseGeometry.rows[$i].target
        if ([Math]::Abs($point.x-$target[0]) -gt 0.001 -or [Math]::Abs($point.y-$target[1]) -gt 0.001 -or [Math]::Abs($point.z-$target[2]) -gt 0.001) { throw 'Added Mothership nose anchor differs from measured geometry.' }
        if ($noseGeometry.rows[$i].surface_probes.Count -ne 5) { throw 'Missing nose surface probes.' }
    }
    if ($noseGeometry.rows[0].target[0] -ne -$noseGeometry.rows[1].target[0] -or
        $noseGeometry.rows[0].target[1] -ne $noseGeometry.rows[1].target[1] -or
        $noseGeometry.rows[0].target[2] -ne $noseGeometry.rows[1].target[2]) { throw 'Added Mothership nose anchors are not symmetric.' }
    $vectorType = $geometry.GetMethod('Target').ReturnType
    $catalog = Get-Content -Raw (Join-Path $RepositoryRoot 'docs\ship-balance-research\turret-clearance\mothership-layout-2026-10-21.json') | ConvertFrom-Json
    $assetPath = Join-Path (Split-Path -Parent $TargetManagedDir) 'StreamingAssets\AssetBundles\ships'
    if ((Get-FileHash -LiteralPath $assetPath -Algorithm SHA256).Hash -ne $catalog.source_bundle_sha256) {
        throw 'Mothership original-geometry fixture does not match installed ships bundle.'
    }
    $tops = [Array]::CreateInstance($vectorType, 20)
    $bottoms = [Array]::CreateInstance($vectorType, 20)
    $ctor = $vectorType.GetConstructor([type[]]@([single], [single], [single]))
    foreach ($row in $catalog.rows) {
        for ($side = 0; $side -lt 2; ++$side) {
            $key = if ($side -eq 0) { 'dorsal' } else { 'ventral' }
            $expected = $ctor.Invoke([object[]]@([single]$row.$key[0], [single]$row.$key[1], [single]$row.$key[2]))
            $actual = $geometry.GetMethod('Target').Invoke($null, @([int]$row.index, [bool]$side))
            if (-not $geometry.GetMethod('Matches').Invoke($null, @($actual, $expected))) { throw "Mothership target differs from measured catalog: $key $($row.index)" }
            $origKey = 'original_' + $key
            $original = $ctor.Invoke([object[]]@([single]$row.$origKey[0], [single]$row.$origKey[1], [single]$row.$origKey[2]))
            if (-not $geometry.GetMethod('Matches').Invoke($null, @($original, $geometry.GetMethod('Original').Invoke($null, @([int]$row.index, [bool]$side))))) {
                throw 'Mothership source guard differs from extracted prefab.'
            }
            if ($side -eq 0) { $tops[$row.index] = $original } else { $bottoms[$row.index] = $original }
        }
    }
    $guardArgs = New-Object object[] 3
    $guardArgs[0] = $tops; $guardArgs[1] = $bottoms; $guardArgs[2] = $null
    if (-not $geometry.GetMethod('ValidatePositions').Invoke($null, $guardArgs)) { throw 'Mothership rejected original asset geometry.' }
    for ($side = 0; $side -lt 2; ++$side) {
        if ($side -eq 0) { $values = $tops } else { $values = $bottoms }
        for ($i = 0; $i -lt 20; ++$i) {
            $saved = $values[$i]
            $values[$i] = $ctor.Invoke([object[]]@([single]9999, [single]0, [single]0))
            if ($geometry.GetMethod('ValidatePositions').Invoke($null, $guardArgs)) { throw "Mothership accepted changed source position at $side/$i" }
            $values[$i] = $saved
        }
    }
    for ($i = 0; $i -lt 20; ++$i) {
        $tops[$i] = $geometry.GetMethod('Target').Invoke($null, @($i, $false))
        $bottoms[$i] = $geometry.GetMethod('Target').Invoke($null, @($i, $true))
    }
    if (-not $geometry.GetMethod('ValidatePositions').Invoke($null, $guardArgs)) { throw 'Mothership placement is not idempotent.' }
    $minimum = [double]::PositiveInfinity
    for ($side = 0; $side -lt 2; ++$side) {
        if ($side -eq 0) { $points = $tops } else { $points = $bottoms }
        for ($a = 0; $a -lt 20; ++$a) { for ($b = $a + 1; $b -lt 20; ++$b) {
            $pa = $points[$a]; $pb = $points[$b]
            $distance = [Math]::Sqrt([Math]::Pow($pa.x-$pb.x,2)+[Math]::Pow($pa.y-$pb.y,2)+[Math]::Pow($pa.z-$pb.z,2))
            $minimum = [Math]::Min($minimum, $distance)
        } }
    }
    # The original all-faction preview sphere diameter is 29.018897 m,
    # larger than combat art. Keep at least 5 m even on that conservative path.
    if ($minimum -lt 34.19) { throw "Mothership size-4 geometry margin lost: $minimum" }
    $mother = @($overrides | Where-Object dataName -eq 'AlienMothership')[0]
    if (@($mother.shipModuleSlots | Where-Object moduleSlotType -eq 'HullHardPoint').Count -ne 18 -or $mother.mass_tons -ne 9240 -or @($mother.shipModuleSlots | Where-Object moduleSlotType -eq NoseHardPoint).Count -ne 8 -or $mother.shipModuleSlots.Count -ne 53) {
        throw 'Conditional Mothership UI/mass decision changed without additional UI capacity.'
    }
    $newNoses=Get-Content (Join-Path $RepositoryRoot 'docs\ship-balance-research\alien-capital-expansion\nose-geometry-0.11.2.json') -Raw|ConvertFrom-Json
    if($newNoses.sourceBundleSha256 -ne $noseGeometry.source_bundle_sha256){throw 'New nose geometry source hash differs from the guarded bundle.'}
    foreach($row in $newNoses.rows){
        $point=$geometry.GetMethod('AddedNosePosition').Invoke($null,@([int]($row.controller-5)))
        $target=$ctor.Invoke([object[]]@([single]$row.target[0],[single]$row.target[1],[single]$row.target[2]))
        if(-not $geometry.GetMethod('Matches').Invoke($null,@($point,$target)) -or $row.surface_probes.Count -ne 5){throw 'New bow nose anchor differs from probed mesh surface.'}
        for($side=0;$side -lt 2;++$side){for($i=0;$i -lt 20;++$i){
            $hullPoint=$geometry.GetMethod('Target').Invoke($null,@($i,[bool]$side))
            $gap=[Math]::Sqrt([Math]::Pow($point.x-$hullPoint.x,2)+[Math]::Pow($point.y-$hullPoint.y,2)+[Math]::Pow($point.z-$hullPoint.z,2))
            if($gap -lt 34.19){throw 'New nose mount collides with an existing hull controller envelope.'}
        }}
    }
    Write-Host 'PASS: Mothership source hash, 40 hull geometry guards, eight distinct nose and eighteen forward hull cells; new bow probes/clearance, 53 controls and 9240 t structure validate. Scene hierarchy/aiming remains Unity-host validation.'
    Write-Host "PASS: $($stockDesigns.Count) ownerless stock capital designs, including Ship20a, normalize through the new-ship guard without repeated expansion."
    Write-Host 'PASS: ownerless missile replacement follows static project prerequisites and rejects future, unknown-root and cyclic research paths.'
    Write-Host "PASS: $($patches.Count - $unityOnly.Count) capital patches applied; $($unityOnly.Count) target-resolved patches require Unity native calls. Save schema round trips; future schemas reject; migration expansion, AI eligibility, independent heavy cells and utility/mass/crew data validate."
    if ($unityOnly.Count) { Write-Host ('Unity-host validation remains: ' + ($unityOnly -join ', ')) }
}
catch { Write-Host $_.ScriptStackTrace; throw $_.Exception.ToString() }
finally { $harmony.UnpatchAll('eeo.capital.validation') }
