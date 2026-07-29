# Hull resource-cost accounting

Last reviewed: 2026-07-29

This note traces how Terra Invicta prices an empty human ship hull and applies
the result to the six hulls currently in the low-tech rebalance slice. It is a
planning document only; no template or runtime value has been changed.

## Short answer

The game already accounts for the full mass of an empty hull, but it expresses
that mass in **ten-tonne resource units**:

`1 space-resource point = 10 tonnes`

The installed compiled default is `spaceResourceToTons = 0.1`, and the code
multiplies tonnes by this value. The field name is easy to read backwards; in
the construction formula it behaves as **resource points per tonne**.

For example, the installed Gunship is:

- 178 t of hull;
- 3 crew billets at the hardcoded 4 t/crew, or 12 t;
- 190 t empty mass;
- 19.0 total construction-resource points;
- `19.0 × 10 = 190 t`.

The small-looking number in the construction UI is therefore not evidence that
171 tonnes went uncharged. It is a unit-presentation problem.

There are nevertheless two genuine balance problems:

1. every ordinary human hull uses the same `10% volatiles / 70% base metals /
   20% noble metals` split, which assigns a very large fraction of a low-tech
   hull to the game's titanium/tungsten/precious-metal category; and
2. the planned change from 4 t to 3 t per crew must alter both dry mass and
   construction resources. Those are separate code paths.

## Exact compiled cost path

Runtime inspection of the installed `Assembly-CSharp.dll` gives this path:

1. `TISpaceShipTemplate.spaceResourceConstructionCost(...)` calls
   `hullTemplate.buildCost(0, 0)`.
2. Human hulls inherit `TIShipModuleTemplate.buildCost(...)`.
3. `buildMass_tons(...)` returns the hull template's `mass_tons`.
4. The build-cost method multiplies that mass by the compiled global
   `spaceResourceToTons`, whose default is `0.1`.
5. `ResourceCostBuilder.ToResourcesCost(multiplier)` multiplies each field in
   `weightedBuildMaterials` by that result. It does **not** normalize the
   weights.
6. The ship cost method separately adds baseline stores for every crew billet:
   `crewBaselineWater_tons` and `crewBaselineVolatiles_tons`, each multiplied
   by crew billets and by `spaceResourceToTons`.

For resource `r`, the hull term is therefore:

`hull cost[r] = hull mass_t × 0.1 points/t × weightedBuildMaterials[r]`

The compiled defaults for crew construction are:

`2 t water + 2 t volatiles = 4 t per crew`

The compiled dry-mass getter independently uses:

`crew mass = 4 t × crew billets`

Those two independent fours currently agree.

The mod's present `TIGlobalConfig.json` changes annual crew water and volatile
consumption to 3 t each, but it does not override either baseline construction
field. Annual support consumption is not the same calculation as the initial
crew package.

## What the material weights mean

The material fields are literal multipliers, not percentages enforced by the
engine.

- If the weights sum to `1.0`, resource-equivalent mass equals component mass.
- If they sum to more than `1.0`, the excess represents manufacturing feedstock,
  waste, spares, or an abstract construction premium.
- If they sum to less than `1.0`, some component mass is not billed.

All six installed human hulls in this slice sum to exactly `1.0`:

| Material | Installed weight |
|---|---:|
| Volatiles | 0.10 |
| Base metals | 0.70 |
| Noble metals | 0.20 |
| **Total** | **1.00** |

The localization matters here. Terra Invicta's “Base Metals” include iron,
nickel, copper, aluminum, lithium, silicon, and boron. “Noble Metals” include
not only gold-group precious metals but also titanium and tungsten. The current
20% noble fraction is therefore not literally 20% gold and platinum, although
it is still a strong specialty-metal charge for a low-tech pressure hull.

## Installed empty-hull bill

These values include the hull and its hull-defined crew billets, but no drive,
power plant, radiator, weapons, utilities, armor, or propellant. Values are
space-resource points; multiply the row total by ten for its tonne-equivalent.

| Hull | Water | Volatiles | Base metals | Noble metals | Total points | Accounted mass |
|---|---:|---:|---:|---:|---:|---:|
| Gunship | 0.60 | 2.38 | 12.46 | 3.56 | **19.00** | **190 t** |
| Escort | 0.80 | 4.30 | 24.50 | 7.00 | **36.60** | **366 t** |
| Corvette | 1.60 | 5.60 | 28.00 | 8.00 | **43.20** | **432 t** |
| Frigate | 4.00 | 10.00 | 42.00 | 12.00 | **68.00** | **680 t** |
| Monitor | 7.00 | 15.00 | 56.00 | 16.00 | **94.00** | **940 t** |
| Destroyer | 8.00 | 16.25 | 57.75 | 16.50 | **98.50** | **985 t** |

The Gunship check is:

- hull volatiles: `178 × 0.10 × 0.1 = 1.78`;
- crew volatiles: `3 × 2 × 0.1 = 0.60`;
- crew water: `3 × 2 × 0.1 = 0.60`;
- base metals: `178 × 0.70 × 0.1 = 12.46`;
- noble metals: `178 × 0.20 × 0.1 = 3.56`;
- total: `19.00 points = 190 t`.

The larger hulls look particularly volatile-heavy because their installed crew
counts add equal masses of water and volatiles. The hull itself is still 70%
base metal.

## Why changing the global conversion is the wrong first lever

Changing `spaceResourceToTons` from `0.1` to `1.0` would make the displayed
resource numbers numerically equal to tonnes, but it would also multiply the
affected construction and logistics economy by ten while mining income remained
expressed on its existing scale.

The compiled field is reused by ship components, armor, propellant and tanks,
crew stores, hab construction/support calculations, probes, repairs, and
resupply. It is therefore not a hull-only presentation setting.

If hulls need to cost more than their incorporated mass, use a hull-specific
manufacturing multiplier and say what the excess represents. Do not silently
reinterpret the global resource unit.

## Two mass-consistent ways to implement the settled 3 t/crew decision

### Minimal data-oriented route

1. Patch `TISpaceShipTemplate.get_crewMass_tons()` from 4 t to the settled
   3 t/crew.
2. Override `crewBaselineWater_tons` and
   `crewBaselineVolatiles_tons` from 2.0 to 1.5 each.
3. Keep each hull's material weights summing to 1.0.

This is easy to audit and exactly mass-conserving, but it says all three tonnes
of crew allowance are water and volatile material. That is not a good physical
description of a bundle that also stands for bunks, pressure-volume share,
life-support machinery, suits, tanks, workstations, and spares.

### Better physical abstraction

Use a three-ton crew package such as this provisional research split:

| Crew-package component | Tonnes per crew | Purpose represented |
|---|---:|---|
| Water | 0.6 | makeup water, stored reserve, and oxygen feedstock |
| Volatiles | 0.6 | food, oxygen/atmosphere, polymers, and personal supplies |
| Base metals | 1.8 | accommodations, tanks, ECLSS hardware, suits, workstations, and shared spares |
| **Total** | **3.0** | |

The exact split is a balance choice, not a measured Artemis value. It is,
however, consistent with the six-month consumables research: consumables alone
do not justify 3 t/crew, so most of the bundle should be durable hardware rather
than water and food.

There is no compiled `crewBaselineMetals_tons` field. Implementing this better
split therefore requires a narrowly scoped construction-cost patch:

- set the existing baseline fields to 0.6 t water and 0.6 t volatiles;
- add 1.8 t of base-metal cost per total crew billet;
- use the same billet count as the vanilla cost method, so module crew is not
  accidentally omitted;
- preserve the `0.1 points/t` conversion and invalidate the ship cost cache when
  a design changes.

## Candidate low-tech hull mix

For a metal-forward but still generous low-tech hull, a useful starting
candidate is:

| Material | Installed | Candidate |
|---|---:|---:|
| Volatiles | 10% | **10%** |
| Base metals | 70% | **85%** |
| Noble metals | 20% | **5%** |
| **Total** | **100%** | **100%** |

This is not yet a settled decision. Five percent noble metals still leaves a
meaningful allowance for titanium, tungsten, corrosion-resistant hardware, and
electronics. If that is judged too aggressive, `80/10/10` for
base/volatile/noble is the cautious alternative. Raising base metals without
reducing another field would cease to be a mass composition and become a
manufacturing-overhead multiplier.

## What the settled hulls would cost under the candidate

The following comparison uses all previously settled hull masses and crew
counts, the candidate `10% volatile / 85% base / 5% noble` hull mix, and the
provisional `0.6 t water / 0.6 t volatile / 1.8 t base-metal` crew package.
It remains a scenario, not a changelog decision.

| Hull | Water | Volatiles | Base metals | Noble metals | Total points | Settled empty mass |
|---|---:|---:|---:|---:|---:|---:|
| Gunship | 0.18 | 1.89 | 15.08 | 0.86 | **18.00** | **180 t** |
| Escort | 0.24 | 3.62 | 29.45 | 1.69 | **35.00** | **350 t** |
| Corvette | 0.30 | 4.15 | 33.63 | 1.93 | **40.00** | **400 t** |
| Frigate | 0.48 | 6.24 | 50.40 | 2.88 | **60.00** | **600 t** |
| Monitor | 0.42 | 7.21 | 58.98 | 3.40 | **70.00** | **700 t** |
| Destroyer | 0.54 | 9.27 | 75.83 | 4.37 | **90.00** | **900 t** |

Small rounding differences in the displayed columns do not alter the exact row
totals. In every case:

`10 × total resource points = settled empty mass`

This model uses considerably more base metal while lowering total mass where
the hull/crew decisions already call for it. It avoids the false choice between
“charge the whole mass” and “use more metals”: the game can do both while
retaining its ten-tonne resource unit.

## Recommended planning rule

For later implementation and spreadsheet auditing:

1. Treat `weightedBuildMaterials` as a physical mass allocation and require its
   ordinary-material fields to sum to 1.0.
2. Show both resource points and tonne-equivalent mass in the workbook.
3. Add an automatic check:
   `10 × sum(construction resource points) - displayed dry mass`.
4. Expect zero for a bare hull plus crew, before any deliberately declared
   manufacturing overhead.
5. Keep manufacturing overhead in a separate, named multiplier rather than
   hiding it in material weights.
6. Audit the whole resource economy separately if base metals remain
   underutilized after hull composition is corrected; mining abundance and
   non-hull module recipes may be the larger cause.

## Local evidence

- Installed hull data:
  `TerraInvicta_Data/StreamingAssets/Templates/TIShipHullTemplate.json`
- Installed resource descriptions:
  `TerraInvicta_Data/StreamingAssets/Localization/en/UIGeneralControls.en`
- Compiled methods:
  `TIShipModuleTemplate.buildMass_tons`,
  `TIShipModuleTemplate.buildCost`,
  `TISpaceShipTemplate.get_crewMass_tons`, and
  `TISpaceShipTemplate.spaceResourceConstructionCost`
- Compiled global defaults:
  `spaceResourceToTons = 0.1`,
  `crewBaselineWater_tons = 2`,
  `crewBaselineVolatiles_tons = 2`

