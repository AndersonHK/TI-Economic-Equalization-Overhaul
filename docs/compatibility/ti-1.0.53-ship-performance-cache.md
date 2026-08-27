# TI 1.0.53 ship performance cache and refit loading

## Symptom

After loading a campaign, the ship-class list and the refit designer could show
different wet mass, acceleration, delta-v, and propellant cost for an unchanged
design. In the reported Monitor example the class list showed 3,775 tons and
7.3 kps, while opening Upgrade showed 3,575 tons and 6.3 kps.

## Diagnosis

The 200-ton mass difference is exactly two standard 100-ton propellant tanks.
The corresponding acceleration, delta-v, and 20-water differences are all
consistent with the saved design having 12 tanks and the refit editor having 10.
The saved design values were therefore coherent; the Upgrade path was mutating
the design while loading it.

`FleetsScreenController.LoadShipTemplateIntoUI` constructs the editable copy in
stages. The fuel-capacity refresh patch could enforce the capacity of an
intermediate hull appearance before the saved appearance had been committed.
Once reduced, the tank count was not restored when the higher-capacity art was
selected.

There was a separate cache-reconciliation gap. The mod refreshed only
`dryMass_tons(true)` after its hull, drive, and reactor registries changed. The
game caches acceleration and delta-v independently, so those values could remain
based on an earlier mass model.

## Canonical behavior

- A saved design's propellant count is authoritative while opening Upgrade.
- Capacity enforcement is suspended while the editor is assembling an existing
  template.
- Once the saved hull appearance is final, the count is clamped only if it is
  genuinely illegal for that appearance.
- Performance caches are rebuilt through the game's
  `TISpaceShipTemplate.CacheTemplateValues(skipCost: true)` entry point, which
  force-recalculates dry mass, acceleration, delta-v, battery capacity, and heat
  capacity through the same functions used by the ship designer.
- The refit editor performs a full cache rebuild after finalizing the art and
  tank count because construction cost also depends on propellant.
- Loaded faction designs and instantiated ships both use the same performance
  refresh helper.

## Manual regression cases

1. Load a save containing a design whose selected hull art holds more fuel than
   another art for the same hull.
2. Record its tank count, wet mass, acceleration, delta-v, and propellant cost in
   the ship-class list.
3. Open Upgrade without changing any component.
4. Confirm the editor preserves the tank count and shows matching performance
   and propellant cost.
5. Cycle to an art whose true capacity is below that count and confirm the count
   is clamped only after that user-initiated art change.
6. Reload the save and confirm class-list values remain stable before and after
   opening Upgrade.

## Automated verification

The TI 1.0.53 release pipeline validates the staged-load guard, final-art tank
restoration, aggregate performance-cache call, faction save-load hook, and
Harmony binding. The complete release suite passed on 2026-08-25 with 24 patch
validators and 10 data/formula validators in two eight-worker pools (22.80 s
wall time).
