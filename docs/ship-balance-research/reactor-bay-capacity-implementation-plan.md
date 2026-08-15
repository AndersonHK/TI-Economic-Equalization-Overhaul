# Graphical-variant reactor-bay capacity implementation plan

Status: implemented and automatically verified on 2026-08-13; manual designer
interaction testing remains pending.

This document preserves the gameplay contract and asset measurements used to
implement hull-aware reactor capacity. It is also an input to a future hull
geometry refactor: graphical variants with larger engine installations and
larger reactor bays should eventually receive correspondingly larger runtime
hull dimensions rather than retaining geometry that contradicts their art.

## Capacity rule

Use the resolved graphical appearance exposed by
`TISpaceShipTemplate.GetHullAppearanceIndex`:

```text
bay mass allowance = variant bay volume * installed density / bay fraction
bay output limit    = bay mass allowance / reactor specificPower_tGW
effective output    = min(reactor maxOutput_GW, bay output limit)
```

The exact `(hull dataName, resolved appearance index)` controls bay volume.
Hull size class is a fallback only for an unmeasured alien, third-party, or
future pair. Radiator mass remains separate.

## Variant geometry table

These values are inscribed circular cylinders derived from the named aft
radiator/machinery mesh. Raw mesh dimensions, asset identifiers, and additional
precision are maintained in
[`reactor-bay-variant-volumes.csv`](tables/reactor-bay-variant-volumes.csv).

| Hull | Appearance 0 | Appearance 1 | Appearance 2 | Appearance 3 |
|---|---:|---:|---:|---:|
| Gunship | 264.241 m3 | 452.197 m3 | 317.310 m3 | 712.242 m3 |
| Escort | 264.241 m3 | 452.197 m3 | 317.310 m3 | 712.242 m3 |
| Corvette | 264.241 m3 | 452.197 m3 | 604.707 m3 | 837.588 m3 |
| Frigate | 332.341 m3 | 675.444 m3 | 1,246.492 m3 | 1,233.527 m3 |
| Monitor | 384.582 m3 | 675.444 m3 | 2,617.607 m3 | 2,028.675 m3 |
| Destroyer | 384.582 m3 | 675.444 m3 | 2,617.607 m3 | 2,028.675 m3 |
| Cruiser | 1,989.242 m3 | 1,384.984 m3 | 3,930.638 m3 | 3,505.550 m3 |
| Battlecruiser | 1,989.243 m3 | 1,384.984 m3 | 3,930.638 m3 | 3,505.550 m3 |
| Lancer | 2,365.773 m3 | 2,090.292 m3 | 10,223.879 m3 | 8,072.644 m3 |
| Battleship | 5,648.074 m3 | 2,090.292 m3 | 5,464.773 m3 | 6,945.700 m3 |
| Dreadnought | 11,476.330 m3 | 2,090.293 m3 | 10,223.879 m3 | 10,952.622 m3 |
| Titan | 15,955.576 m3 | 6,290.837 m3 | 16,549.539 m3 | 15,840.889 m3 |

Appearance 0 is the default Earth art, appearance 1 is the alternate Earth art,
and appearances 2-3 come from the Dark Skies `ships_prm` bundle. When the DLC
is unavailable, vanilla resolves 2 to 0 and 3 to 1; capacity must consume the
resolved value so it follows the model actually instantiated.

Fallback volumes use the largest measured graphical variant in each vanilla
runtime size band:

| Runtime size band | Fallback bay volume |
|---|---:|
| Small | 2,617.607 m3 |
| Medium | 3,930.638 m3 |
| Large | 16,549.539 m3 |
| Huge | 16,549.539 m3 |

Huge saturates at the largest measured Titan rather than extrapolating. Missing
exact pairs emit a one-time diagnostic containing hull, resolved appearance,
size band, and fallback value.

## Reactor architecture inputs

| Reactor family | Installed density | Reported-mass bay fraction |
|---|---:|---:|
| Fuel cell | 1.20 t/m3 | 0.25 |
| Solid-core fission | 2.50 t/m3 | 0.50 |
| Molten-salt fission | 3.50 t/m3 | 0.55 |
| Liquid/molten-core fission | 2.50 t/m3 | 0.55 |
| Vapor/gas-core fission | 2.00 t/m3 | 0.45 |
| Electrostatic fusion | 1.00 t/m3 | 0.75 |
| Mirror fusion | 1.20 t/m3 | 0.75 |
| Toroidal/general magnetic fusion | 2.00 t/m3 | 0.75 |
| Hybrid fusion | 2.00 t/m3 | 0.75 |
| Z-pinch fusion | 2.50 t/m3 | 0.60 |
| Inertial fusion | 1.50 t/m3 | 0.60 |
| Antimatter plasma core | 2.50 t/m3 | 0.60 |
| Antimatter beam core | 3.00 t/m3 | 0.40 |

Unused antimatter solid/gas classes mirror the equivalent fission architecture.
An otherwise unhandled class uses `2.00 t/m3` and `0.75`.

## Runtime and interface contract

- Add `shipBalance.reactorBayCapacityEnabled`, default `true`, independently of
  hull drive scaling.
- Use effective output in both final hull-aware compatibility directions:
  candidate drive against selected plant and candidate plant against selected
  drive. The compared drive demand includes the existing hull drive multiplier.
- Ensure player design, cluster changes, refits, human and alien autodesign, and
  fighter design reach a final hull-aware check. Hull-less research scoring and
  broad catalogue prefilters retain theoretical output.
- Preserve existing saved ships. The rule restricts new selection and refit; it
  does not rewrite or disable loaded designs.
- Preserve vanilla auxiliary-system and weapon-power semantics. This pass limits
  drive selection rather than invalidating weapons or utilities.
- Contextual power-plant descriptions retain theoretical output and add bay
  volume, bay-derived output, effective output, and the binding reason. The
  selected-hull comparison table displays and sorts by effective output.

## Acceptance cases

- Molten Salt II on Gunship fits Pegasus x3/x5/x3/x6 on appearances 0/1/2/3.
- Molten Salt II on Frigate fits Pegasus x4 on appearance 0 and x6 on 1-3.
- Solid V on default Gunship rejects Heavy Dumbo x3 by bay; sufficiently large
  variants become limited by the plant's `60 GW` rating.
- Cruiser and larger Molten Salt II combinations are normally plant-rating
  limited even though their exact graphical volumes differ.
- Feature disablement restores theoretical-output compatibility without turning
  off hull drive scaling.

## Future hull-size refactor dependency

Do not infer future statistical hull size from reactor-bay volume alone. Retain
the complete hull visual envelope, drive/nozzle envelope, radiator-bay envelope,
combat colliders, hardpoint clearance, and the existing drive-scale measurements
as separate inputs. The future refactor should then:

1. key runtime hull dimensions and mass policy by resolved graphical variant;
2. increase statistical hull size where a larger engine and reactor bay are
   visibly present;
3. keep the reactor-bay lookup as the authoritative machinery-capacity input;
4. avoid applying a second implicit bay multiplier through enlarged statistical
   hull dimensions; and
5. retest thrust scaling, armor area and mass, collider alignment, targeting,
   radiator presentation, and construction cost together.

This preserves one coherent graphical source of truth while preventing the
later hull-size work from silently double-counting the same larger machinery.

## Implementation and verification record

The implementation uses the two existing final template compatibility gates,
so player design, thruster-cluster changes, refits, and human/alien/fighter
autodesign all consume the same hull-aware decision through
`validDrivesForPowerPlant` and `validPowerPlantsForDrive`. The broad catalogue
properties that supply those consumers remain unchanged; their final predicate
now applies the effective output.

Appearance changes patch `FleetsScreenController.SetAltHull` and refresh both
module-table row collections after vanilla updates the resolved appearance.
Disabling `shipBalance.reactorBayCapacityEnabled` restores the theoretical
maximum in compatibility and in already-instantiated comparison rows.

The normal `tools/deploy.ps1` workflow completed against Terra Invicta 1.0.51:

- 903 formula assertions passed, including all 48 measured pairs, all reactor
  architecture mappings, class fallbacks, invalid specific mass, feature
  disablement, the Gunship and Frigate acceptance clusters, and both
  compatibility directions;
- guarded IL validation found the intended module-table call and both final
  compatibility targets, and all 123 Harmony patch classes applied;
- the 48-row maintained geometry CSV and its measurement-tool recognition
  contract passed ship-rebalance validation;
- release packaging completed with DLL SHA-256
  `BEB137D07E7A079D8D19B8BA00990EF41FDB2ABE2CDD8D01DE5C3858DEDC9BAB`; and
- 33 files deployed to the enabled mod directory.

Manual testing should now verify the visible row and tooltip text while cycling
appearances, add/remove-thruster behavior, refit selection, an autodesigned
ship, and the documented Molten Salt II and Solid V acceptance cases. Record
observations here once tested in game.

## First manual-test correction

Manual testing found that changing from a bay supporting Pegasus x5 to one
supporting only x3 left x5 installed. Decrementing that stale cluster to x4
could crash because vanilla's spinner path assumes its current drive remains a
valid selection. The appearance postfix now reconciles before refreshing rows:

1. enumerate the current drive family's x1-x6 variations;
2. consider only counts no larger than the currently installed cluster;
3. select the largest variation accepted by the final hull-aware gate;
4. install it through `SetModuleInSlot`; or
5. if x1 is invalid, clear the drive through `RemoveModuleFromSlot`.

Those standard designer methods keep the slot, spinner, model and derived
statistics synchronized. The contextual reactor text was also reduced to the
player-facing occupied-volume statement:

```text
Reactor bay volume used / available
{used} / {available} m³
```

Occupied volume is derived from hull-scaled drive power, reactor specific mass,
reported-mass bay fraction, and installed density. The correction passed 906
formula assertions and guarded reconciliation IL validation, then deployed on
2026-08-13 with DLL SHA-256
`9D41A29288BC873CE0A9961AD9665ADAC8E4459ABB9155C0BF9D59CBF8A8E4BE`.

The first correction did not resolve the manual reproduction. The subsequent
crash log showed a null x4 module entering
`ShipModuleDragDestination.OnDecreasePressed` and the patched
`FleetsScreenController.SetModuleInSlot`, with no reconciliation diagnostic.
The second correction therefore removes the indirect validity call: it compares
each variation's hull-scaled power demand directly with the selected plant's
effective output. `SetAltHull` IL validation locks the order in which it commits
`hullAppearanceIndex`, refreshes the designer panel, and reaches the postfix.

As a separate defensive invariant, `SetModuleInSlot` now rejects null modules
before vanilla or the multi-slot utility postfix can dereference them. The
second correction deployed with DLL SHA-256
`9161DA31B26B4F72E1F7071AD0F062E13F967639081EC55E652C0F4109AFE695`.
The reconciliation logs its exact replacement or removal, providing direct
runtime evidence for the next manual retest.

## Third manual-test correction

The second correction targeted the wrong appearance lifecycle method. The ship
designer's left/right art arrows call `OnCycleAltHull`; `SetAltHull` is used
when loading a template. Consequently, the deployed postfix never ran during
the reported reproduction. The null-module prefix merely converted the old
crash into a confirmation-sound no-op and must be removed.

The corrected contract is:

1. patch both `OnCycleAltHull` and `SetAltHull` so interactive changes and
   designer loading share the same invariant;
2. after the resolved appearance has been committed, compare the currently
   installed drive variation—not a family or base template—with the bay's
   effective output;
3. replace it with the largest fitting variation no larger than the installed
   count, or remove it when x1 cannot fit;
4. refresh the spinner/slot through the normal designer mutation path; and
5. recalculate template caches and refresh the ship-performance panel, both
   module catalog tables, and both contextual module-detail panels.

The UI acceptance condition is immediate: cycling from an x5-capable variant
to an x3-capable variant displays x3 before the user can operate its spinner,
and every appearance-dependent number updates during that same action.

This correction passed 906 formula assertions and all 123 Harmony patches,
including guarded validation of both appearance targets and every required UI
refresh path. It deployed 33 files against TI 1.0.51 on 2026-08-13 with DLL
SHA-256
`83553DF2FD25F8393D7BCE939F8DADED5506FC31247C1AE04CFC9DC68CA897FD`.
The exact manual reproduction remains the final acceptance test.
