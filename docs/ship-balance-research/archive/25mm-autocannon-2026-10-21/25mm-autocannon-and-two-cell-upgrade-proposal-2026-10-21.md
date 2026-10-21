# Deferred proposal: 25 mm autocannon and two-cell 30/40 mm batteries

**ARCHIVED 2026-10-21 at the user's request.** This 25 mm addition and two-cell autocannon plan is no longer the active proposal. It is retained as historical evidence; see the [canonical-size plan](../../canonical-turret-scaling-plan-2026-10-21.md).

2026-10-21. **Proposal only. No weapon, hull, save, model asset or runtime code is changed by this document. No build or deployment is requested.** The current implementation remains 0.10.4; the uncommitted Mothership work is outside this proposal.

The concept is sound: introduce a compact size-1 25 mm gun using the 30 mm art at **25/30 = 0.833333 scale**, and make the current hull-mounted 30/40 mm autocannons size 2 for new construction. This brings the human size-1 clearance limit close to the remaining naval-gun art. It does not exactly reach that limit, and the firing/slot balance requires a deliberate decision: two proposed 25 mm guns would outdamage one otherwise unchanged two-cell 30 mm gun.

“Binding point” here means the weapon that limits the allowed size-1 spacing, not a hull node or physical pivot location. No mount origin needs moving for this proposal. Uniformly scale the weapon hierarchy around that origin.

**Naming clarification:** the size-1 weapon is the **6-inch cannon**, which uses `shipweapons/8-Inch_Cannon`. The actual **8-inch cannon** is already `TwoHullHoriz` and uses `12-Inch_Cannon`. Both comparisons are given below. See the [mesh inspection](../../turret-clearance/weapon-shapes-and-monitor-2026-10-21.md) for the original shapes and common-scale figures.

## Scope and intended early progression

**Later dimensional finding (2026-10-21):** the
[barrel/lens audit](../../barrel-and-lens-dimensional-audit-2026-10-21.md) measures
approximately 155.4 mm openings in the current shared 30–40 mm mesh. Scaling
it by 25/30 yields 129.5 mm openings, preserving its original bore exaggeration.
This proposal's scale remains a fitting and visual-footprint choice; accurate
25 mm bores would require separate tube geometry edits. No implementation is
added by this clarification.

The proposed new-build lineup is:

- **25 mm, size 1:** basic fast-tracking point defense and very light ship combat. Available with `Project_Warships`.
- **30 mm, size 2:** larger packets and longer magazine endurance, using its existing art and per-weapon statistics initially. Its role relative to two 25 mm guns needs the balance gate below.
- **40 mm, size 2:** the later high-velocity, longer-range upgrade. Retain `Project_40mmAutocannon` and its research prerequisites/cost.
- **6-inch, size 1 / 8-inch, size 2:** keep the existing slower naval-gun branch. These are not renamed or resized.

This scope targets `30mmAutocannon` and `40mmAutocannon`, both hull weapons. It does **not** promote the distinct nose weapons `35mmAutocannon` or `40mmNoseAutocannon`, add a nose 25 mm gun, change missile/laser/magnetic statistics, or enlarge hull UI capacity. “Tier 1” below means the early weapon role; hull construction tiers are listed separately because Monitor, Destroyer, Cruiser and Battlecruiser are tier-2 hulls in the inspected data.

## Geometric effect: close to the size-1 naval asset, but not quite equal

Uniform scaling makes every linear dimension and motion-envelope diameter 5/6 of the original. It does not imply that every gameplay mass or damage field must follow the same linear law.

| Weapon/art | W × H × L m | Conservative 3D sphere diameter m | Refined horizontal diameter bound m |
| --- | --- | --- | --- |
| 30/40 mm current art | 3.495 × 2.873 × 6.110 | 12.952 | 9.673 |
| Proposed 25 mm | 2.913 × 2.394 × 5.092 | 10.793 | 8.061 |
| 6-inch weapon / 8-Inch art | 3.495 × 2.843 × 4.986 | 10.624 | 7.371 |
| 8-inch weapon / 12-Inch art | 3.495 × 2.806 × 5.800 | 12.175 | 9.024 |

The proposed 25 mm is **+1.6%** above the size-1 naval art under the old sphere metric, but **+9.4%** above it under the more relevant upright horizontal bound. Its width is smaller than the naval pedestal, while its barrel reach still controls the horizontal radius.

The human new-build size-1 horizontal ceiling would fall **16.7%**, from 9.673 to 8.061 m. Matching the 6-inch weapon's horizontal bound exactly would require an art factor of about **0.7620**, rather than 0.8333. That corresponds to roughly 22.86 mm only as a visual-scale analogy, not a ballistic requirement.

The old 30 mm mesh would become the human size-2 horizontal outlier at 9.673 m, above the current 8-inch weapon's 9.024 m. The constraint is redistributed between categories, not eliminated. Legacy one-cell 30/40 mm equipment would still need its original clearance treatment if preserved for existing saves.

| Remaining human size-1 art after promotion | Horizontal bound m |
| --- | --- |
| 8-Inch_Cannon | 7.371 |
| torpedo_launcher | 7.330 |
| 60_generic_battery | 6.726 |
| small_coilgun_battery | 6.056 |
| small_railgun_battery | 5.173 |
| point_defense_particle_beam_battery | 4.618 |

This is a **human new-build** conclusion. Across human and alien art together, the alien point-defense particle model has an 8.194 m horizontal bound, slightly above the proposed 25 mm's 8.061 m. An all-faction worst-case size-1 rule would therefore become limited by that alien resource. Static torpedo launchers are also listed by the inherited survey, but their hypothetical rotational envelope must not be mistaken for actual launcher motion.

If the 25 mm is added but the 30/40 mm guns remain selectable as size 1, the worst-case size-1 limit does **not** improve. The reclassification/legacy catalog policy is necessary to realize this benefit.

### Effect on nearby mounts

This table uses the smallest same-face horizontal distance across all four appearances and the new upright horizontal bound. It is a screening margin, not a proof of mesh collision when negative. The previous sphere tables remain archived and use a different metric.

| Class, worst appearance | Horizontal spacing m | Current 30 mm gap m | 25 mm gap m | 25 mm ≥5 m? |
| --- | --- | --- | --- | --- |
| Escort | 13.700 | 4.027 | 5.639 | yes |
| Frigate | 16.438 | 6.765 | 8.377 | yes |
| Monitor | 7.540 | -2.133 | -0.521 | no |
| Destroyer | 21.980 | 12.307 | 13.919 | yes |
| Cruiser | 26.110 | 16.437 | 18.049 | yes |
| Battlecruiser | 39.920 | 30.247 | 31.859 | yes |
| Battleship | 9.160 | -0.513 | 1.099 | no |
| Dreadnought | 6.920 | -2.753 | -1.141 | no |

The Escort gains a meaningful margin at this scale. Monitor/0 remains a close case under an independent all-aim bound: a 16.7% visual reduction is much smaller than the prior optional 50% compact-art concept. This proposal therefore does not certify all Monitor appearances for every aim direction, or authorize size 4 at every node. Corvette has only one logical hull mount and no neighbor-pair constraint; that does not prove hull/foundation clearance.

## Proposed 25 mm calibration

The working candidate copies the **current merged mod 30 mm**, not vanilla 30 mm values. For example, the current mod uses 1.75 kg damaging mass and a 0.25 s intra-salvo interval; cloning the vanilla record alone would start from different damage and cadence.

Use a geometrically similar projectile as the initial numerical reference: diameter ratio 5/6, area ratio 25/36 = 0.694444, mass ratio 125/216 = 0.578704. These are game-calibration assumptions, not a claim that the game's kilogram values represent a realistic individual 25/30 mm cartridge. Visual scaling does not itself force a projectile mass law.

| Parameter | Current 30 mm | Working 25 mm candidate |
| --- | --- | --- |
| Mount | OneHull | OneHull |
| Art scale relative to 30 mm | 1.000000 | 0.833333 |
| Projectile diameter mm | 30 | 25 |
| Damaging mass kg | 1.750000 | 1.012731 |
| Loaded ammunition mass per shot kg | 5.500000 | 3.182870 |
| Muzzle velocity km/s | 1.350 | 1.350 |
| Shots per salvo | 10 | 10 |
| Intra-salvo interval s | 0.250 | 0.200 |
| Post-salvo cooldown s | 1.000 | 1.000 |
| Magazine shots | 3000 | 3000 |
| Targeting range km | 200 | 200 |
| Bare weapon mass t | 3.000 | 2.000 (proposed rounded value) |
| Additional weapon crew | 0 | 0 |
| powerUse_MJ per shot | 0.085000 | 0.049190 |
| Efficiency | 1.0 | 1.0 |
| Flat chipping | 0 | 0 |
| Attack / defense mode | true / true | true / true |
| Projectile PD-targetable setting | false | false |
| Unlock | Project_Warships | Project_Warships |

The 0.20 s interval gives 25% faster intra-burst shots but only **16.1% more sustained shots/s**, because the 1 s post-salvo pause remains. It aligns with the existing 50 ms weapon-check grid. A superficially modest 0.225 s value could be rounded up by scheduled checks and fail to deliver the intended improvement; actual firing timestamps must be tested. See [WeaponCadenceMath](../../../../TIEconomyMod/Core/WeaponCadenceMath.cs).

The 2 t bare mass is a proposed rounded mechanism value, not an automatic consequence of mesh scale. Keeping the current 3 t would be a valid stricter copy variant. Power is provisionally scaled with the same cubic workload ratio. Keep the magazine at 3000 for the initial copy; a 3500-shot alternative nearly restores the current 30 mm's continuous-fire endurance at additional ammunition mass.

Calculate damage from `warheadMass_kg` and `muzzleVelocity_kps`; do not trust the inherited `damage_MJ` annotation, which is already inconsistent with some current partial overrides. A new full template should keep any retained metadata consistent.

## Per-weapon and per-cell comparison

Raw damage below is muzzle kinetic energy before hit probability, relative velocity, armor, chipping allocation, interception, power interruptions and aiming time. `cycle = cooldown + (shots − 1) × intraSalvoCooldown`; `DPS = 0.5 × warheadMass × velocity² × shots / cycle`. Magazine mass follows `magazine × ammoMass_kg`. Endurance is magazine divided by the sustained shot rate: a steady-cycle estimate, not exact first-to-final-shot timing. Loaded masses exclude ship crew support and other modules.

| Weapon | Cells | Shot MJ | Shots/s | Raw MJ/s | MJ/s/cell | Loaded mass t | Endurance s | Average electric MW |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 25 mm proposed | 1 | 0.923 | 3.571 | 3.296 | 3.296 | 11.549 | 840.0 | 0.176 |
| 30 mm current | 1 | 1.595 | 3.077 | 4.907 | 4.907 | 19.500 | 975.0 | 0.262 |
| 30 mm promoted | 2 | 1.595 | 3.077 | 4.907 | 2.453 | 19.500 | 975.0 | 0.262 |
| 40 mm current | 1 | 9.464 | 1.412 | 13.361 | 13.361 | 42.600 | 1,416.7 | 13.647 |
| 40 mm promoted | 2 | 9.464 | 1.412 | 13.361 | 6.680 | 42.600 | 1,416.7 | 13.647 |
| 6-inch current | 1 | 39.200 | 0.222 | 8.711 | 8.711 | 79.000 | 2,700.0 | 0.150 |
| 8-inch current | 2 | 88.200 | 0.178 | 15.680 | 7.840 | 130.000 | 2,250.0 | 0.250 |

The working 25 mm candidate has **-42.1% per-shot energy**, **-32.8% sustained raw output**, and **-13.8% continuous-fire endurance relative to current 30 mm. Moving an unchanged gun to two cells halves its raw output per cell; it does not automatically double its gun mass, ammunition or power draw.

Current 40 mm is already much more electrically demanding than 30 mm: approximately 13.647 versus 0.262 MW average under uninterrupted fire. The proposal preserves that existing difference rather than silently normalizing it. Efficiency 1.0 produces zero module waste heat in the current auxiliary-electricity formula for 25/30 mm; this is a model convention, not a claim that firing the gun generates no physical heat. See [GunPowerRegistry](../../../../TIEconomyMod/Core/GunPowerRegistry.cs) and [WeaponPowerMath](../../../../TIEconomyMod/Core/WeaponPowerMath.cs).

### The weak 30 mm intermediate is the main balance problem

At equal two-cell occupancy, two working-candidate 25 mm guns produce **6.592 MJ/s**, versus **4.907 MJ/s** for one unchanged 30 mm: about **34.3% more raw damage**, with two independently targetable weapons. The 30 mm retains larger individual packets, lower installed mass than two complete 25 mm units, and longer continuous-fire endurance, but has the same 200 km range and 1.35 km/s muzzle velocity. Do not assume those differences automatically produce a useful upgrade.

One promoted 40 mm remains strong: **13.361 MJ/s**, about twice two candidate 25 mm guns, with 350 km range and 2.6 km/s velocity. Its later research, mass and power demand create meaningful tradeoffs. The 6-inch gun's high raw per-cell output is also not directly comparable to CIWS: it fires much more slowly and its projectiles retain their different targetability/chipping behavior.

### Sensitivity and decisions before implementation

| 25 mm variant | Damaging mass kg | Shot MJ | Raw MJ/s | Vs current 30 mm | Two 25 / one promoted 30 |
| --- | --- | --- | --- | --- | --- |
| Geometrically similar projectile (working) | 1.012731 | 0.923 | 3.296 | -32.8% | 1.343 |
| Same-length projectile / area scaling | 1.215278 | 1.107 | 3.955 | -19.4% | 1.612 |
| Balance-first lighter packet | 0.750000 | 0.683 | 2.441 | -50.3% | 0.995 |

The lighter 0.75 kg packet variant nearly equalizes two 25 mm guns with one promoted 30 mm, but cuts early one-cell raw firepower about 50% relative to today's 30 mm. It is a balance choice, not cubic caliber scaling. It could materially weaken early fleets against missiles.

Alternatively, retaining the working 25 mm candidate would require roughly a **34.3%** raw-output improvement to the two-cell 30 mm merely to break even with two 25 mm units. At unchanged cadence/velocity, that means damaging mass about **2.350984 kg** instead of 1.75 kg. That would be an additional rebalance, not part of simply moving it to size 2. Larger packets may also cross point-defense kill thresholds, so equal raw DPS is not equal interception performance.

**Recommendation:** keep the working candidate as the transparent geometric baseline, but make 30 mm's intended role a release gate. Do not silently nerf 25 mm further or buff 30 mm merely to make the progression table look tidy. Choose the lighter-packet variant, a stronger 30 mm, or an intentionally non-dominant specialist only after matched early-combat/PD tests.

## A size-2 mount needs an orientation decision

For the minimal implementation, use `TwoHullHoriz` for the widened 30/40 mm records, consistent with the current 8-inch weapon. This has a consequential exception: **Escort has a vertical two-cell fit but no horizontal one**. It would keep two 25 mm guns but lose new-build access to both widened autocannons. Corvette also cannot fit a size-2 hull weapon.

A single fixed `TwoHullVert` choice would preserve Escort access but exclude Frigate, Monitor, Destroyer, Cruiser and Battlecruiser from the widened gun. If preserving both orientations is required, add a per-weapon flexible-footprint resolver and reconcile designer icons, core/secondary occupancy, removal, AI packing and native controller mapping. Do not just return two cells from a custom helper while leaving those other paths inconsistent.

| Human hull | Construction tier | Size-1 cells | TwoHullHoriz guns | TwoHullVert guns | Promotion consequence |
| --- | --- | --- | --- | --- | --- |
| Gunship | 1 | 0 | 0 | 0 | No hull guns |
| Escort | 1 | 2 | 0 | 1 | 25 mm only from this branch |
| Corvette | 1 | 1 | 0 | 0 | 25 mm only from this branch |
| Frigate | 1 | 2 | 1 | 0 | Widened guns consume pairs |
| Monitor | 2 | 4 | 2 | 0 | Widened guns consume pairs |
| Destroyer | 2 | 2 | 1 | 0 | Widened guns consume pairs |
| Cruiser | 2 | 3 | 1 | 0 | Widened guns consume pairs |
| Battlecruiser | 2 | 2 | 1 | 0 | Widened guns consume pairs |
| Battleship | 3 | 6 | 2 | 3 | Widened guns consume pairs |
| Lancer | 3 | 3 | 1 | 0 | Widened guns consume pairs |
| Dreadnought | 3 | 8 | 4 | 4 | Widened guns consume pairs |
| Titan | 3 | 6 | 2 | 3 | FourHull-only capital; no new access |

Counts are exact disjoint native grid packings, not `floor(cellCount/2)` guesses. Titan's raw grid capacities are shown for completeness, but the current capital policy rejects these small hull weapons. Alien weapon families are unchanged. STO Fighter and other simple/special hulls require their own eligibility checks; this proposal does not assume that a displayed hull cell accepts every standard `OneHull` weapon.

## Hull-output fallout

These are fully populated **hull-weapon** comparisons; nose armament stays unchanged. “30-pair mix” means the maximum number of horizontal 30 mm batteries plus 25 mm in leftover cells. “40-pair mix” does the equivalent with 40 mm. The 30-pair mix is not the raw-DPS optimum when all 25 mm is stronger. Research, ammo, point defense and mass remain separate considerations.

| Hull | Today: all 30 | Proposed: all 25 | Proposed 30-pair mix | Today: all 40 | Proposed 40-pair mix | 40-era Δ |
| --- | --- | --- | --- | --- | --- | --- |
| Gunship | 0.000 | 0.000 | 0.000 | 0.000 | 0.000 | — |
| Escort | 9.813 | 6.592 | 6.592 | 26.722 | 6.592 | -75.3% |
| Corvette | 4.907 | 3.296 | 3.296 | 13.361 | 3.296 | -75.3% |
| Frigate | 9.813 | 6.592 | 4.907 | 26.722 | 13.361 | -50.0% |
| Monitor | 19.627 | 13.184 | 9.813 | 53.444 | 26.722 | -50.0% |
| Destroyer | 9.813 | 6.592 | 4.907 | 26.722 | 13.361 | -50.0% |
| Cruiser | 14.720 | 9.888 | 8.203 | 40.083 | 16.657 | -58.4% |
| Battlecruiser | 9.813 | 6.592 | 4.907 | 26.722 | 13.361 | -50.0% |
| Battleship | 29.440 | 19.775 | 16.405 | 80.166 | 33.314 | -58.4% |
| Lancer | 14.720 | 9.888 | 8.203 | 40.083 | 16.657 | -58.4% |
| Dreadnought | 39.254 | 26.367 | 19.627 | 106.888 | 53.444 | -50.0% |

Replacing a full one-cell 30 mm layout with the working 25 mm candidate lowers this portion of raw output by 32.8% on every affected hull. Promoting 40 mm generally halves the count on even, horizontally pairable layouts; Escort and Corvette instead fall back to 25 mm and lose about 75.3% of their previous all-40-mm raw hull output. These are substantial balance changes, even though no hull node is added or removed.

Gunship has no hull weapons, so this proposal does not alter its nose gun. Frigate/Monitor unlock with `Project_PatrolVessels`; starting Escort/Corvette users would have the new 25 mm at `Project_Warships`, but no horizontal two-cell autocannon upgrade on those hulls. Keep the slower 6-inch branch available as its existing alternative.

## Incidental native behavior must be controlled explicitly

Changing `mount` changes `internalSize`, which is used beyond UI occupancy:

| Behavior | Current size 1 | Native size 2 without compensation | Recommended narrow policy |
| --- | --- | --- | --- |
| Turret training rate | 270 degrees/s | 135 degrees/s | Preserve 270 for unchanged physical 30/40 art unless a tracking nerf is explicitly chosen |
| Default module hit points when hp is absent | 3 | 6 | Explicitly set hp=3 if the change is intended to price footprint only |
| Independent weapons on a two-cell pair | 2 | 1 | Accept as the intended fitting consequence |
| Base mass/ammo/power per installed gun | Existing values | No automatic doubling | Preserve initially; compare whole-loadout totals |
| Core visual mount | OneHull mapping | TwoHullHoriz/Vert mapping | Audit every legal core, including merged/alternative positions |

The native methods inspected are `TIShipWeaponTemplate.internalSize`, `TIShipPartTemplate.hitPoints`, and `ShipWeaponVisController.InitializeCommon`. Keeping the old training rate requires a scoped runtime override; it is not a native JSON field already available here. Raw DPS tables assume weapons are already aimed and do not include this tracking effect. The source-method excerpts and current assembly hash are retained in the numerical evidence snapshot.

Thus this is more than a new template plus two mount-enum edits. If the lowest-code, native-behavior option is preferred instead, retain the native 135 degrees/s and 6 HP and explicitly classify the result as an additional PD-tracking nerf and module-durability buff.

## Model, projectile and data implementation scope

**New record.** Create a full `TIGunTemplate` record for `25mmAutocannon` from the merged 30 mm baseline, with a unique localized name, catalog order and clear icon/tooltip. Reusing the icon art initially is reasonable, but the label must distinguish 25 mm. A new ID cannot inherit missing stock fields merely by being a sparse override. Preserve normal per-record merging; do not replace the complete gun array.

**Gun art.** Reuse the 30 mm mesh geometry at a uniform factor 5/6. Do not mutate its shared prefab or shared mesh, which would also shrink 30/40 mm. A separate resource or a weapon-ID-scoped instance scaler are options. `SetWeapon` applies resource scales in combat but retains hull placeholders in preview, so a cloned resource root scale alone is insufficient to guarantee the intended preview size. Handle both branches, restore original scale when switching away from 25 mm, and prevent repeated preview/refit initialization from multiplying the scale repeatedly. Scale the base hierarchy once, including child offsets and the fire point; do not scale both base and child independently by 5/6, which would compound the reduction. The base origin remains on the existing hull mount.

**Projectile physics.** Set `projectileDiameter_mm=25` through the existing [ProjectileGeometryRegistry](../../../../TIEconomyMod/Core/ProjectileGeometryRegistry.cs), reduce damaging and ammunition masses explicitly, and preserve velocity for the initial comparison. The diameter controls collider size and cross-sectional area through [ProjectileCollisionPatches](../../../../TIEconomyMod/Patches/ProjectileCollisionPatches.cs); it does not itself reduce kinetic damage. The cross-sectional area is 0.000490874 m², 30.6% below the 30 mm value.

**Projectile graphics.** The existing diameter patch sizes colliders, not necessarily the visible tracer/mesh. Verify `BulletAutocannon` and muzzle effects separately; any visual shrink must be new-ID-specific because 30/40 mm share those resources. Retain the copied non-PD-targetable setting initially; do not infer universal invulnerability from that flag.

**Electrical data.** Add the new `powerUse_MJ` entry so [GunPowerRegistry](../../../../TIEconomyMod/Core/GunPowerRegistry.cs) binds the 25 mm correctly. Caliber and power extensions are independent of native gun template fields. Test both enabled and disabled feature paths. No global change to naval-gun physics, shared launcher behavior or capital fitting is implied.

## Save, refit and stock-design fallout

**Do not simply change the existing IDs' mount enums in an established campaign.** Previously legal one-cell 30/40 mm weapons can overlap a neighboring weapon after loading, lose a required core cell, or map multiple logical guns to the same visual controller. Cached designs, saved blueprints, built ships, AI caches, ammo/reload keys, damaged parts and pending construction all need an explicit compatibility policy.

There is already a concrete stock conflict: installed design **Ship30**, a Battleship, has `40mmAutocannon` at slot **17** and `RattlerMissileBay` at slot **13**. A `TwoHullHoriz` weapon anchored at 17 consumes slots **17 and 13**. A naïve enum change makes those occupied cells overlap. This was identified from the actual roster and hull-grid mapping, not inferred from cell counts.

**Recommended compatibility architecture:** add versioned two-cell records (for example `30mmAutocannonHull2` and `40mmAutocannonHull2`) while retaining the old one-cell records for existing equipment. Hide/reject the legacy records for new construction and AI selection, and give the new records the familiar displayed weapon names with explicit size-2 labeling. Keep existing ships stable until a defined refit/conversion path is selected. Hiding icons alone is insufficient: construction from old saved designs must also be gated or converted.

Update fresh-campaign/stock rosters separately. Ship30 can either retain its cell pattern with a 25 mm replacement or trade another weapon for a widened 40 mm; those choices have different combat value. Do not silently remove a missile or grant an extra gun. For current campaigns, any automatic conversion must preserve damage, ammo fraction and fire modes and report replaced/removed equipment; downgrading a 40 mm to 25 mm is a major change and should not be hidden inside save loading.

Legacy compatibility means the geometric ceiling improves for **new legal designs**, not retroactively for every ship still carrying a legacy one-cell autocannon. Future measurement tooling should report new-build eligibility separately from legacy templates. A name-only scan found direct occurrences in the gun catalog, the 40 mm project's friendly name, and Ship30; it does not inventory user saves or dynamically generated AI designs.

Capital legacy conversion, research/obsolete filtering, skirmish rosters, refit eligibility, mixed human/alien ownership and any hab use must be checked. The current FourHull-only Titan/Mothership rule remains unchanged and does not make the new guns capital-eligible.

## Proposed delivery stages and acceptance gates

1. **Approve the calibration and footprint policy.** Choose geometric versus balance-first projectile mass, fixed horizontal versus flexible size-2 placement, the 30 mm role, and whether native HP/training side effects are preserved or compensated. This document does not settle those by implementation.
2. **Add and validate 25 mm in isolation.** Full template/localization, per-ID model scaling, projectile caliber and power registration. Check preview/combat/refit scale restoration and actual shot timestamps. This stage alone does not lower the size-1 catalog maximum while 30/40 mm remain selectable there.
3. **Introduce widened 30/40 mm with compatibility handling.** Verify every human hull's legal footprints and controller mapping, resolve Ship30, block new-build use of legacy records, and test save/import/refit/ammo/damage behavior before retiring any aliases.
4. **Run matched early-combat tests.** Compare 25/30/40 mm at equal cells, loaded mass, resource cost and power; test anti-missile performance against multiple target masses, crossing targets and saturated salvos, not just raw ship DPS. Compare four small guns with two paired guns on Monitor, and starting Escort/Corvette viability after losing new-build 40 mm access.
5. **Only then finalize values and release.** Use the repository's normal build/deploy/automatic-validation flow once implementation is separately authorized. Manual checks must include every changed art/camera branch and both broadside copies of a logical hull gun.

No global weapon buff, hull slot addition, nose reclassification, Mothership redesign, arbitrary save rewrite, or implementation-matrix claim of an implemented feature is part of this deferred proposal.

## Evidence and arithmetic

The [calculation snapshot](25mm-proposal-2026-10-21.json) retains the merged baseline records, proposed candidate, derived metrics, exact native hull packings and source SHA-256 hashes. All proposed numbers are planning values. Damage uses the mod's current kinetic/auxiliary-power conventions, not a real-world cartridge specification. No Unity session or new combat simulation was run for this proposal.
