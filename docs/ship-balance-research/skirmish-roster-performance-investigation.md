# Skirmish roster performance investigation

Date: 2026-08-01  
Status: documented and explored; no implementation in this turn

## Reported regression

Adding ships to either side of the main-menu skirmish roster produces a delay
that becomes much larger as the roster grows. The issue is present before the
battle begins and therefore is not caused by projectile simulation, collision
geometry, direct-fire commitments, or combat target acquisition.

The regression was first noticed during the 0.8.x ship-balance work. The exact
first affected release has not yet been isolated by an A/B timing run.

## Confirmed vanilla call chain

Terra Invicta 1.0.51 does not incrementally append one roster row. The relevant
path is:

```text
SkirmishShipListItemController.AddShipSelected / AddSpecificShip
  -> StartMenuController.PopulateSkirmishDropdowns
     -> rebuild location, hab, faction, and both fleet controls
     -> resize and initialize every ship row on both sides
        -> SkirmishShipListItemController.PopulateShipDropdown
           -> iterate every available TISpaceShipTemplate
           -> format its localized class name and combat score
           -> allocate a TMP_Dropdown.OptionData entry
     -> StartMenuController.SetFleetScores
        -> revisit every selected ship on both sides
```

`DuplicateShip` reaches the same path through `AddSpecificShip`, and deletion
also calls the full dropdown rebuild.

Let `R` be the number of roster rows and `S` the number of available ship
designs. One addition performs approximately `O(R x S)` dropdown work plus
other `O(R)` and constant global-menu work. The delay of each click should grow
roughly linearly with `R`; the cumulative time needed to construct a large
roster grows approximately as `O(R^2 x S)`. This can feel exponential even
though the confirmed structure is quadratic cumulative growth rather than a
true exponential algorithm.

The installed vanilla ship-template file contains 46 records, five explicitly
hidden from skirmish. Imported designs can raise `S` further.

## Relationship to the 0.8.x changes

### Ruled out as direct causes

- The 0.8.3 direct-fire patches target projectile firing and destruction,
  attack-fire expected damage, and combat-AI target selection. None is called
  by the main-menu roster path.
- The 0.8.2 projectile geometry and durability patches target projectile fire,
  update, collision damage, and destruction. They likewise do not run here.
- The 0.8.0 crew-support getter is constant-time and does not scan fleets or
  templates.

### Plausible 0.8.1 amplifier

The 0.8.1 power and thermal work can make a cache miss in ship combat-value or
dry-mass calculation more expensive:

- radiator heat now iterates every installed weapon;
- required weapon power and heat call the newly patched gun getters; and
- `GunPowerRegistry.TryGetPowerUse_MJ` currently constructs a scenario-aware
  string key with `OrderBy` and `string.Join` on every lookup.

The last point allocates and sorts even though template identity and scenario
tags are stable after hydration. It is a worthwhile independent cleanup.
However, it does not explain roster-size scaling by itself. Ship combat values
are requested with `forceUpdate = false`, and positive cached values take the
fast path. The current and previous player logs contain no `NaN`, infinity, or
invalid-combat-value report. The full per-row dropdown rebuild is therefore the
confirmed multiplier; repeated power recalculation remains a hypothesis to
measure, not an established cause.

`ShipPowerRuntime.RefreshTemplateMassCaches` is called during template/mod
initialization, not once for every roster addition. The skirmish log also shows
the entire `PostGlobalInit2` phase completing in about 33 ms, so that load-time
repair is not a good explanation for a delay that increases with each row.

## Preferred correction

Preserve the vanilla roster lifecycle initially, but cache the common ship
dropdown options once per stable menu context instead of rebuilding them for
every row.

The cache key or invalidation state must include:

- skirmish scenario and selected faction/side;
- the available and imported ship-template set;
- current localization; and
- template-generation state after mod hydration.

The cached option text must still be built from the real
`TISpaceShipTemplate.TemplateSpaceCombatValue` after EEO's power, radiator,
mass, and crew values are active. Each row should then reuse or shallow-copy
the common option entries, select its own current design, and retain vanilla
tooltip and damage-image setup. This changes repeated menu presentation work;
it does not bypass or replace the ship-stat calculations that support the new
power feature.

This reduces the dominant dropdown work from approximately `O(R x S)` per
addition to `O(S + R)`: build the design options once when invalidated, then do
small per-row selection work. It is safer than replacing the entire 469-IL
`PopulateSkirmishDropdowns` method or maintaining a parallel skirmish setup
implementation.

As a separate low-risk optimization, hydrate gun power into a dictionary keyed
by `TIGunTemplate` identity (or precompute each template's stable lookup key)
so hot getters perform an allocation-free lookup. This preserves every power
and heat value and reduces the cost of legitimate ship-stat recalculation, but
it should not be presented as the complete roster fix until profiling confirms
its share.

## Deferred higher-risk alternative

An incremental roster refresh could replace the full rebuild after add,
duplicate, and delete, initializing only the changed row and the new add row.
That has the best theoretical cost, but it must reproduce private list-manager
state, row indices, add-button behavior, both-side score refresh, faction
changes, imported designs, and deletion shifts. It should be considered only
if common-option caching leaves material lag.

## Profiling and acceptance plan

Before implementation, add temporary counters/timers around:

- `StartMenuController.PopulateSkirmishDropdowns`;
- `SkirmishShipListItemController.PopulateShipDropdown`;
- `TISpaceShipTemplate.TemplateSpaceCombatValue` cache misses; and
- `GunPowerRegistry.TryGetPowerUse_MJ` calls and key construction.

Record the time to add ships 1, 5, 10, 20, and 40 on alternating sides, with
the same imported-design set, for vanilla, EEO with ship balance disabled, and
EEO fully enabled. A fix is acceptable when:

1. per-add time no longer multiplies by both roster rows and ship templates;
2. displayed combat scores are identical before and after the optimization;
3. gun electrical demand, radiator mass, dry mass, and heat remain identical;
4. add, duplicate, delete, faction change, imported design, and both-side
   roster flows remain correct; and
5. no main-menu cache survives a scenario, faction, localization, import, or
   template-generation change that should invalidate it.
