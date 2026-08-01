# Conventional-gun power-consumption patch plan

Last reviewed: 2026-08-01  
Current runtime/code-path validation: installed Terra Invicta 1.0.51  
Original balance data provenance: installed Terra Invicta 1.0.49

## Status

The power-only portion of this plan is implemented in version 0.8.1. Gun mass,
projectile, ammunition, damage, range, and cadence changes remain deferred.

The objective is to make the 30mm, 40mm ETC, and 6-10-inch chemical guns use
Terra Invicta's existing reactor, battery, heat, and weapon-UI systems without
charging the chemical guns for projectile muzzle energy.

## Settled load assumptions

Use the upper end of the earlier engineering bands for the electrically driven
loader, mount, feed, and control equipment:

| Weapon | Settled average auxiliary load |
|---|---:|
| 30mm Autocannon | 0.100 MW |
| 6-inch Gun Battery | 0.150 MW |
| 8-inch Gun Battery | 0.250 MW |
| 10-inch Cannon | 0.300 MW |

Treat local weapon heat as a function of the generic JSON `efficiency` value,
the same way the powered-weapon classes do. Propellant continues to supply the
chemical guns' muzzle energy; barrel and propellant heat remain outside this
first electrical patch. Loader efficiencies for the ordinary guns are not yet
settled.

### Effective 0.8.1 power-only values

Version 0.8.1 deliberately retains vanilla projectile and firing-cycle data.
The ordinary guns use 100% module efficiency until loader losses are settled;
this preserves the electrical-load targets without inventing additional heat.

| Weapon | Vanilla cadence basis | `powerUse_MJ` | Efficiency | Electrical input/shot | Average input | Salvo-rate plant output | Module heat rate |
|---|---:|---:|---:|---:|---:|---:|---:|
| 30mm | 10 shots; 4 s cooldown; 0.5 s intra | 0.085 MJ | 100% | 0.085 MJ | 0.100 MW | 0.170 MW | 0 MW |
| 40mm ETC | 6 shots; 4 s cooldown; 0.75 s intra | 8.700 MJ | 90% | 9.6667 MJ | 7.484 MW | 12.889 MW | 1.289 MW |
| 6-inch | 4 shots; 12 s cooldown; 2 s intra | 0.675 MJ | 100% | 0.675 MJ | 0.150 MW | 0.338 MW | 0 MW |
| 8-inch | 4 shots; 15 s cooldown; 2.5 s intra | 1.40625 MJ | 100% | 1.40625 MJ | 0.250 MW | 0.563 MW | 0 MW |
| 10-inch | 3 shots; 16 s cooldown; 3 s intra | 2.200 MJ | 100% | 2.200 MJ | 0.300 MW | 0.733 MW | 0 MW |

The later 180/100-rpm autocannon figures below remain planning values and do
not describe the 0.8.1 runtime cadence.

### 40mm ETC convention

The planned 40mm damaging packet is 3 kg at 2.6 km/s. Use 1.0 km/s as the
chemical-only reference velocity and represent the kinetic-energy increment as
useful ETC work:

`ETC electrical energy = 0.5 * m * (v_ETC^2 - v_chemical^2)`

`= 0.5 * 3 kg * (2600^2 - 1000^2) = 8.64 MJ per rendered projectile`

At 100 rpm this is 14.4 MW of useful work. Add 0.06 MJ per shot for the
30mm-class feed and control load, giving **8.70 MJ useful work per rendered
projectile**. Set the inherited generic efficiency to **90%**, matching the
meaning of efficiency on lasers and magnetic weapons. The resulting electrical
input is **9.6667 MJ per shot**, or **16.111 MW average**, and local weapon heat
is **0.9667 MJ per shot**, or **1.611 MW average**.

The complete-combustion premise remains a deliberate balance abstraction: ETC
is represented as using plasma ignition to achieve exceptionally complete and
controlled propellant combustion rather than as a small railgun stage. The 90%
field is the efficiency of the complete electrically assisted weapon process,
not a claim that 10% of the propellant energy becomes electrical heat. Reactor
inefficiency still creates separate power-plant waste heat.

Army material supports the qualitative premise: ETC controls propellant burn
rate electrically, can improve launch performance, and has demonstrated about
a 15% performance improvement in a comparable-caliber program. It also notes
that an energetic working fluid requires much less electrical energy than a
pure electrothermal gun. The game's 1.0-to-2.6 km/s gain is much more ambitious
than those demonstrations, so the equivalent-work model should be documented
as a futuristic gameplay convention, not an experimentally established energy
budget.

Sources:

- [U.S. Army, ETC power-development overview](https://asc.army.mil/docs/pubs/alt/archives/1993/May-Jun_1993.PDF)
- [U.S. Army, electric-gun classes and ETC energetic working fluid](https://asc.army.mil/docs/pubs/alt/archives/1992/Mar-Apr_1992.PDF)
- [U.S. Army Armor, plasma ignition and XM-291 performance](https://www.benning.army.mil/armor/earmor/content/issues/2015/jan_mar/Elmonairy.html)

## Per-shot and vanilla powered-weapon accounting

For each chemical gun:

`loader energy per shot = settled average load * averageCooldown_s`

where Terra Invicta's `averageCooldown_s` is full salvo-cycle time divided by
rendered shots. For an ordinary gun with efficiency `eta`, the JSON useful-work
value is `target electrical input per shot * eta`. For the ETC gun, the settled
8.70 MJ is already its useful-work value and the 90% efficiency increases the
electrical input in the same way it does for a laser or railgun.

| Weapon | Planned cadence | JSON `powerUse_MJ` | Efficiency | Electrical input per shot | Average electrical input | Vanilla salvo-rate plant demand | Vanilla one-shot storage |
|---|---:|---:|---:|---:|---:|---:|---:|
| 30mm | 180 rpm; 10 shots; 0.25 s intra | `0.0333 * eta30` | TBD | 0.0333 MJ | 0.100 MW | 0.133 MW | 0.0333 MJ |
| 40mm ETC | 100 rpm; 6 shots; 0.375 s intra | 8.700 MJ | 90% | 9.6667 MJ | 16.111 MW | 25.778 MW | 9.6667 MJ |
| 6-inch | 13.33 rpm; 4 shots; 2 s intra | `0.675 * eta6` | TBD | 0.675 MJ | 0.150 MW | 0.338 MW | 0.675 MJ |
| 8-inch | 10.67 rpm; 4 shots; 2.5 s intra | `1.40625 * eta8` | TBD | 1.4063 MJ | 0.250 MW | 0.563 MW | 1.4063 MJ |
| 10-inch | 8.18 rpm; 3 shots; 3 s intra | `2.200 * eta10` | TBD | 2.200 MJ | 0.300 MW | 0.733 MW | 2.200 MJ |

The electrical-input column is what each shot removes from the ship. Unmodified
Terra Invicta sizes salvo-weapon generation as that input divided by intra-salvo
spacing and storage as one shot of input per installed powered weapon. These are
the same rules currently applied to lasers, railguns, and coilguns.

The previous plan proposed gun-specific corrections to average generator and
full-salvo buffer sizing. Do not implement them. If burst generation or one-shot
storage is later judged incorrect, change the shared powered-weapon calculation
for every weapon family rather than creating a second set of rules for guns.

## Revised template-driven code structure

### Architectural rule

Gun power must be data, not a list of weapon identifiers embedded in code. A
gun with a positive JSON power value should automatically enter the same
powered-weapon path as a railgun; a gun without the field should retain vanilla
zero-power behavior.

The proposed JSON contract is:

```json
{
  "dataName": "30mmAutocannon",
  "powerUse_MJ": 0.085,
  "efficiency": 1.0
}
```

- `powerUse_MJ` is useful electrical work per rendered shot, before conversion
  to the game's GJ unit. This mirrors `shotPower_MJ / efficiency` for lasers
  and kinetic energy divided by efficiency for magnetic guns.
- `efficiency` already exists on `TIShipWeaponTemplate` and is already hydrated
  from JSON.
- gun heat follows the same generic convention as powered weapons:
  `EnergyUsage_GJ * (1 - efficiency)`.
- `powerUse_MJ <= 0` means the gun remains self-powered.

The 40mm entry uses `powerUse_MJ: 8.70` and `efficiency: 0.9`. Precise loader
efficiencies for the ordinary guns can be selected in JSON without another code
change while preserving their settled electrical-input targets.

### Ideal upstream/source implementation

If the game class itself can be edited, add `public float powerUse_MJ` to
`TIGunTemplate`. Then replace the gun overrides with:

```csharp
public override bool selfPowered => powerUse_MJ <= 0f;

public override float EnergyUsage_GJ(float extraInput_MJ = 0f)
    => (powerUse_MJ + extraInput_MJ) / efficiency / 1000f;

public override float HeatGeneration_GJ(float extraInput_MJ = 0f)
    => EnergyUsage_GJ(extraInput_MJ) * (1f - efficiency);
```

That is the desired end state: one new serialized field and no downstream
special cases.

### Implemented mod hydration adapter

Harmony cannot add a serializable CLR field to the already-compiled
`TIGunTemplate`, and unknown JSON members are not retained on the deserialized
object. Do **not** depend on patching the serializer: the mod normally loads
after `TemplateManager.Initialize`, so such a patch can miss every gun template.
Version 0.8.1 uses one central runtime registry instead:

1. Read `ModTemplateManager.GetModsForTemplate("TIGunTemplate")`, which retains
   each active mod's `JObject` records and load order.
2. Reproduce the game's JSON merge/replacement order for the extension member,
   including later overrides and full-file replacement semantics.
3. Store the effective values by `dataName` plus normalized scenario tags, with
   an untagged fallback matching ordinary template-merge behavior.
4. Refresh immediately after `PatchAll()` and from a
   `TemplateManager.Initialize` postfix for the opposite startup order.
5. Have the three `TIGunTemplate` patches query that registry directly.

This is a hydration shim, not a weapon-profile database: it contains no gun
names and accepts any present or future gun record carrying the generic field.
Scenario-tagged duplicates must use the same identity/selection semantics as the
template merger rather than `dataName` alone. Rebinding must replace the prior
registry atomically so a partial or failed pass cannot leave stale values.

### Downstream behavior: no gun-specific seams

Do not patch any of the following for guns:

- `TISpaceShipTemplate.requiredWeaponsPowerGeneration_GW`;
- `TISpaceShipTemplate.requiredWeaponsPowerStorage_GJ`;
- `TISpaceShipState.WeaponHasPower`;
- `TISpaceShipState.FireWeapon`;
- power-plant waste heat;
- battery discharge;
- module energy display.

Those consumers already call the three virtual weapon members and therefore
will ingest the JSON values automatically. The existing heat-capacity precheck
does not use `HeatGeneration_GJ` consistently, but that is a shared powered-
weapon issue. If corrected, patch it globally for lasers and magnetic weapons
as well as guns, not through a gun-only exception.

### UI-consumer audit

The original 1.0.49 code audit found every direct UI consumer of weapon energy
and power state. The 1.0.51 compatibility audit found one additional invariant:
`ShipModuleListItem` conditionally creates the Energy Usage cell, while
`ShipModuleTable.ResizeColumns` assumes every visible row has the same number of
cells. Version 0.8.2 therefore forces that cell to exist for every conventional
gun row while displaying the gun's real value; the other consumers remain
generic and require no gun-specific patch:

- `TIShipWeaponTemplate.GetTruncatedDescriptionData` and
  `GetLocalizedEnergyUsage` supply the ordinary weapon/design description;
- `ShipModuleListItem` supplies the fleet and module-table energy row;
- `ShipWeaponUIController.Initialize` activates the combat energy panel for
  non-self-powered weapons;
- `ShipWeaponUIController.UpdateTooltip` adds energy usage plus the generic
  no-power and heat-capacity warnings;
- `ShipWeaponUIController.UpdateFireMode` reflects the generic
  `TISpaceShipState.WeaponHasPower` result, including the offline state;
- ship-design power, storage, reactor-mass, and waste-heat totals consume
  `requiredWeaponsPowerGeneration_GW` and `requiredWeaponsPowerStorage_GJ`,
  which already iterate the generic weapon interface.

There is no existing per-weapon efficiency or heat line to patch. Magnetic and
beam weapons likewise expose electrical input but not their efficiency as a
separate module-row value. If that information is desired, add it once for all
powered weapons rather than only for guns.

Verify that the displayed electrical inputs remain legible:

- 30mm: 0.000033 GJ per shot;
- 6-inch: 0.000675 GJ;
- 8-inch: 0.001406 GJ;
- 10-inch: 0.002200 GJ;
- 40mm: 0.009667 GJ.

`TIUtilities.FormatBigOrSmallNumber` uses its small-number path below 7, so the
30mm value should not round to zero. Confirm this in-game; only if the rendered
localization still loses precision should the generic formatter be changed.
Do not create a gun-only UI path.

### Save-load compatibility audit

The extension registry is runtime template metadata and must not be serialized.
This avoids adding a field to any saved game-state type. Existing saved weapon
and ship references continue to resolve to the normal `TIGunTemplate` objects.

On loading a save, `TISpaceShipState.PostGlobalGameStateCreateInit_2` calls
`CacheInternalPowerStats()`. That method recomputes weapon generation, one-shot
storage, reactor waste heat, and available auxiliary power from the currently
hydrated ship and weapon templates. Consequently, an old save does not retain a
stale zero-power gun cache and should not suffer a schema/deserialization
failure. It will deliberately acquire the new gun power draw and heat as soon
as it is loaded with the patch enabled.

Two compatibility caveats must be treated as test gates:

- an existing design can become heavier because its dynamically calculated
  power-plant requirement rises, while a saved `TISpaceShipState.currentMass_kg`
  may remain the pre-patch value; verify the game's normal post-load mass
  reconciliation before release, and if necessary fix that cache generically
  for all template balance changes rather than in the gun code;
- enabling or disabling the feature during an active session can leave already
  initialized combat panels or cached ship power values stale. Mark the setting
  restart-required unless a global recache and UI rebuild is implemented.

Removing the mod leaves the save schema unchanged and returns guns to vanilla
self-powered behavior. This must also be covered by a round-trip load test.

## Verification plan

### JSON hydration and formula tests

- Assert the five electrical-input-per-shot values in the table.
- Assert `powerUse_MJ` and inherited `efficiency` hydrate from active gun mod
  JSON array.
- Assert the immediate-after-initialize and Initialize-postfix startup orders
  produce identical registries.
- Assert later-mod override and full-file replacement semantics match the game.
- Assert a missing or zero `powerUse_MJ` retains vanilla self-powered
  behavior.
- Assert scenario-tagged duplicate templates retain their own extension values.
- Assert 40mm ETC useful work is 8.70 MJ.
- Assert 40mm `powerUse_MJ: 8.70` and `efficiency: 0.9` produce 0.0096667 GJ
  input and 0.0009667 GJ weapon heat.

### Patch-scope tests

- Confirm every gun with positive JSON power returns `selfPowered == false`.
- Confirm human and alien rail/coil behavior is byte-for-value unchanged.
- Confirm guns without the new JSON member remain self-powered.
- Confirm no weapon identifier appears in the hydration or behavior patch.

### Ship-design tests

- One of each weapon produces the vanilla salvo-rate generator contribution
  shown in the accounting table.
- Two identical weapons produce exactly twice the generation and storage.
- Battery requirement uses the same one-shot rule as other powered weapons.
- Power-plant mass and reactor waste heat respond to the added powered-weapon
  load under the same salvo-rate formula used for magnetic weapons.
- All five UI surfaces named in the audit show or react to the generic values;
  no gun-specific UI branch is present.

### Combat and regression tests

- Each rendered shot removes the expected energy.
- A depleted battery interrupts a salvo rather than permitting free fire.
- The 40mm applies 0.9667 MJ local weapon heat per shot at 90% efficiency while
  reactor inefficiency separately produces power-plant waste heat.
- 30mm and 40mm point-defense targeting and interception cadence remain
  unchanged by the power patch.
- Lasers, railguns, coilguns, missiles, bombardment, and strategic autoresolve
  retain their current energy behavior.

### Save regression tests

- Load a pre-patch save containing each affected gun; assert no serializer or
  missing-template error and verify the recomputed power/storage/heat totals.
- Save and reload with the patch enabled; assert identical extension values,
  power state, battery state, and ship mass before and after the round trip.
- Load the same save after disabling/removing the patch; assert that no custom
  serialized data is required and guns safely return to self-powered behavior.
- Compare `currentMass_kg` with dry mass plus carried consumables on an existing
  ship. Do not ship until any newly exposed cache mismatch is either reconciled
  generically or documented as established base-game save behavior.

## Implemented sequence

1. Add and test `GunPowerRegistry` with load-order and scenario-tag handling.
2. Patch the three `TIGunTemplate` power/heat members generically.
3. Add `powerUse_MJ` and `efficiency` to the five mod gun JSON overrides.
4. Correct shared auxiliary generation, reactor heat, module radiator load, and
   the pre-fire heat check for every powered weapon family.
5. Verify ship design, UI, battery drain, and firing with the 30mm and 40mm.
6. Verify the 6-, 8-, and 10-inch values without adding any new code cases.
7. Reconcile loaded ship mass to refreshed template mass plus saved propellant.
8. Run controlled CIWS, depleted-battery, and old-save smoke tests in game.
9. Treat any remaining generation, storage, or heat-gating issue as a global
   powered-weapon defect rather than a gun-patch defect.

The acceptance test for the architecture is simple: adding the two JSON fields
to another `TIGunTemplate` record must give it complete powered-weapon behavior
without changing C#.
