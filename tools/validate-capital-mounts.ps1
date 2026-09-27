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
        Dreadnought = @(8,19,2343,2400); Titan = @(12,40,3280,3400)
        AlienDreadnought = @(10,50,2050,2200); AlienTitan = @(10,60,2420,2600)
        AlienMothership = @(11,100,8200,8500)
    }
    $mountType = $gameAssembly.GetType('Mount', $true)
    if ($null -eq $mountType) { $mountType = $gameAssembly.GetTypes() | Where-Object Name -eq 'Mount' }
    foreach ($id in $expected.Keys) {
        $original = @($stock | Where-Object dataName -eq $id)[0]
        $override = @($overrides | Where-Object dataName -eq $id)[0]
        $crew = if ($id -like 'Alien*') { $original.crew } else { $override.crew }
        if ($id -like 'Alien*' -and $null -ne $override.crew) { throw "Alien crew override added: $id" }
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
            $movable = ($id -eq 'AlienMothership' -and $oldSlot.moduleSlotType -notin @('HullHardPoint','NoseHardPoint')) -or
                ($id -in @('Dreadnought','Titan') -and $oldSlot.moduleSlotType -eq 'Utility')
            if (-not $movable -and
                ($oldSlot.x -ne $newSlot.x -or $oldSlot.y -ne $newSlot.y)) {
                throw "Existing slot moved or renumbered: $id $i"
            }
        }
        $coordinates = @($override.shipModuleSlots | ForEach-Object { "$($_.x),$($_.y)" })
        if (($coordinates | Select-Object -Unique).Count -ne $coordinates.Count -or
            @($override.shipModuleSlots | Where-Object { $_.x -lt 0 -or $_.x -gt 9 -or $_.y -lt 0 -or $_.y -gt 6 }).Count) {
            throw "Slot outside native grid or duplicate coordinate: $id"
        }
        $newSlots = @($override.shipModuleSlots | Select-Object -Skip $original.shipModuleSlots.Count)
        foreach ($new in $newSlots) {
            # Armor is drawn half an icon lower by native SetupDesignerLayout.
            if (@($override.shipModuleSlots | Where-Object {
                $visibleY = $_.y + $(if ($_.moduleSlotType -like '*Armor') {1} else {0})
                $_.x -eq $new.x -and [Math]::Abs($visibleY - $new.y) -lt 2
            }).Count -ne 1) {
                throw "New utility overlaps another cell: $id $($new.x),$($new.y)"
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
            Write-Host "PASS: JSON-only $id fits native grid; $preserved prior utility footprints, all weapon coordinates and all saved slot indices preserved."
        }
        $hull = $deserialize.Invoke($null, @($hullType, [string]($override | ConvertTo-Json -Depth 20 -Compress)))
        if ($id -in @('Titan','AlienTitan','AlienMothership')) {
            $slots = @($hull.shipModuleSlots | Where-Object { $_.moduleSlotType.ToString() -eq 'HullHardPoint' })
            foreach ($slot in $slots) {
                $footprint = $hull.WeaponSlotSet($slot, [Enum]::Parse($mountType, 'FourHull'))
                if ($footprint.Count -ne 1 -or $footprint[0].x -ne $slot.x -or $footprint[0].y -ne $slot.y) { throw "Heavy footprint merged slots: $id" }
                foreach ($small in @('OneHull','TwoHullHoriz','TwoHullVert','ThreeHullHoriz','HalfHull')) {
                    if ($hull.WeaponSlotSet($slot, [Enum]::Parse($mountType, $small)).Count -ne 0) { throw "Small footprint accepted: $id $small" }
                }
            }
            if ($hull.ValidBigWeaponSlotSets([Enum]::Parse($mountType, 'FourHull')).Count -ne $slots.Count) { throw "Missing independent heavy anchors: $id" }
        }
    }
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
            $expectedHide = $hullRow.dataName -in @('Titan','AlienTitan','AlienMothership') -and $mountName -in @('OneHull','TwoHullHoriz','TwoHullVert')
            if ($hide.Invoke($null, @($catalogDesign,$part)) -ne $expectedHide) { throw "Wrong catalog visibility: $($hullRow.dataName) $mountName" }
        }
        if ($hide.Invoke($null, @($catalogDesign,$titan)) -or $hide.Invoke($null, @($catalogDesign,$null))) { throw 'Capital catalog filter hid an unrelated/null part.' }
    }
    if ($null -ne $modAssembly.GetType('TIEconomyMod.Patches.CapitalUtilityDesignerRowPatch')) {
        throw 'Bespoke designer row patch must be removed from the assembly.'
    }
    Write-Host 'PASS: capital-only hull catalog filtering preserves every nose size; no bespoke designer-row patch remains.'
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
    foreach ($id in @('Titan','AlienTitan','AlienMothership')) {
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
    $stockDesigns = @($allStockDesigns | Where-Object { $_.hullName -in @('Titan','AlienTitan','AlienMothership') })
    if (-not ($stockDesigns | Where-Object dataName -eq 'Ship20a')) { throw 'Regression fixture no longer contains Ship20a.' }
    $newShipGuard = $modAssembly.GetType('TIEconomyMod.Patches.CapitalNewShipMigrationPatch', $true).GetMethod('Prefix')
    $policy = $modAssembly.GetType('TIEconomyMod.Core.CapitalMountPolicy', $true)
    foreach ($row in $stockDesigns) {
        # Keep real weapon layouts; omit unrelated propulsion preflight, which
        # requires the full Unity host. No faction object is created or assigned.
        $row.driveName = ''; $row.powerPlantName = ''; $row.radiatorName = ''
        $design = Add-TestTemplate $templateType ([string]($row | ConvertTo-Json -Depth 30 -Compress))
        if ($null -ne $design.designingFaction) { throw 'Ownerless regression fixture acquired a faction.' }
        $expectedCount = 0
        foreach ($entry in $design.hullWeaponTemplateEntries) {
            if ($entry.moduleName -eq 'Empty') { continue }
            if ($null -eq $entry.moduleTemplate.ref_weapon) { throw "Missing stock fixture weapon $($entry.moduleName) in $($row.dataName)" }
            $expectedCount += $policy.GetMethod('LegacyOffsets').Invoke($null, @($entry.moduleTemplate.ref_weapon.mount.ToString())).Count
        }
        [void]$newShipGuard.Invoke($null, @($design))
        [void]$newShipGuard.Invoke($null, @($design))
        if ($schemaField.GetValue($getState.Invoke($null, @($design))) -ne 1 -or
            $design.hullWeaponTemplateEntries.Count -ne $expectedCount -or
            @($design.hullWeaponTemplateEntries | Where-Object { $_.moduleTemplate.ref_weapon.mount.ToString() -ne 'FourHull' }).Count) {
            throw "Ownerless stock conversion failed or expanded twice: $($row.dataName)"
        }
        # Reading the hydrated list must keep every compact entry.
        if ($design.hullWeapons.Count -ne $expectedCount) { throw "Stock hydration lost weapons: $($row.dataName)" }
    }
    [void]$runtime.GetMethod('NormalizeStockDesigns', $flags).Invoke($null, @())
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
    if ($schemaField.GetValue($getState.Invoke($null, @($imported))) -ne 1 -or
        $imported.hullWeapons.Count -ne 6 -or
        @($imported.hullWeaponTemplateEntries | Where-Object moduleName -ne '360cmIRPhaserBattery').Count -or
        ($imported.hullWeaponTemplateEntries.slot -join ',') -ne '20,16,12,21,17,13' -or
        $scoreField.GetValue($imported) -ne -1 -or $rawScoreField.GetValue($imported) -ne -1) {
        throw 'Save import failed to replace and invalidate the cached six-phaser layout before preview.'
    }
    if ($imported.propellantTanks -ne 52 -or $imported.refitIteration -ne 5 -or
        $imported.noseWeaponTemplateEntries.Count -ne 1 -or
        $imported.noseWeaponTemplateEntries[0].moduleName -ne 'SpinalCoilerMk3' -or
        $imported.noseWeaponTemplateEntries[0].slot -ne 8) { throw 'Import altered fuel, refit identity or nose equipment.' }
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
    Write-Host 'PASS: save-import hook converts six 60cm phasers before preview, requests an ownerless backup, and preserves nose/fuel/non-capitals; repeated import and startup preserve converted weapons and a completed combat-score cache.'
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
        $schemaField.GetValue($getState.Invoke($null, @($imported))) -ne 1 -or
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
    $eligible = $runtime.GetMethod('Eligible', $flags).Invoke($null, @($inferredDesign))
    $replacement = $runtime.GetMethod('Replacement', $flags).Invoke($null, @($sourceMissile, $eligible, $false))
    if ($replacement.dataName -ne 'CapitalPeerCoilgunMk1' -or
        @($eligible | Where-Object { $_.dataName -in @('CapitalFutureCoilgunMk3','CapitalRootCoilgunMk3','CapitalCycleCoilgunMk3','CapitalAlternativeCoilgunMk3') }).Count) {
        throw 'Stock technology inference selected an unsupported branch or missed a peer project.'
    }
    Write-Host "PASS: $($stockDesigns.Count) ownerless stock capital designs, including Ship20a, normalize through the new-ship guard without repeated expansion."
    Write-Host 'PASS: ownerless missile replacement follows static project prerequisites and rejects future, unknown-root and cyclic research paths.'
    Write-Host "PASS: $($patches.Count - $unityOnly.Count) capital patches applied; $($unityOnly.Count) target-resolved patches require Unity native calls. Save schema round trips; future schemas reject; migration expansion, AI eligibility, independent heavy cells and utility/mass/crew data validate."
    if ($unityOnly.Count) { Write-Host ('Unity-host validation remains: ' + ($unityOnly -join ', ')) }
}
catch { Write-Host $_.ScriptStackTrace; throw $_.Exception.ToString() }
finally { $harmony.UnpatchAll('eeo.capital.validation') }
