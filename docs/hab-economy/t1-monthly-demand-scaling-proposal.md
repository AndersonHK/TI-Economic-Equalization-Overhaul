# T1 monthly demand scaling proposal

Date: 2026-08-17  
Status: **implemented and deployed**, including the power-plant aggregation
rule below; manual in-game testing pending

The machine-readable approved values are in
[`hab-module-maintenance-proposals.csv`](hab-module-maintenance-proposals.csv).
The generator aggregation is deliberately save-compatible: existing placed
generators gain 2x output while their crew and monthly resource burden also
become 2x, so old layouts gain power headroom instead of shutting down. New
generator construction pays 2x resources and Boost through the runtime cost
rewrite; template mass and build time are unchanged. Money maintenance is
unchanged for every module.

Deployment verification on 2026-08-17 passed the complete TI 1.0.51 release
pipeline, including 1,070 formula assertions, guarded Harmony/IL checks, exact
110-module proposal coverage, and hash verification of all 44 deployed files.

## Scaling rule

This proposal uses total per-person supply mass as the bridge between real
life and Terra Invicta's economy.

The current game charges each crew member:

```text
3.5 t water/year + 3.5 t volatiles/year
= 7.0 t/year
= 583.3 kg/month
```

The ISS comparison includes all major one-way personal supplies:

```text
packaged food             55.7 kg/person-month
oxygen                    25.6 kg/person-month
water makeup at 98%        2.3 kg/person-month
------------------------------------------------
total                     83.6 kg/person-month
```

That gives a game logistics scaler of `583.3 / 83.6 = 6.98`, rounded to
**7.0x**. NASA's source values are 1.83 kg/person-day of food plus packaging,
0.84 kg/person-day of oxygen, one gallon/person-day gross water use, and 98%
water recovery. [NASA food-system evidence report](https://ntrs.nasa.gov/api/citations/20160011582/downloads/20160011582.pdf),
[NASA oxygen reference](https://ntrs.nasa.gov/api/citations/20110015921/downloads/20110015921.pdf?attachment=true),
and [NASA water-recovery milestone](https://www.nasa.gov/missions/station/iss-research/nasa-achieves-water-recovery-milestone-on-international-space-station/).

For each module:

```text
scaled target = generous real monthly demand * 7.0

if scaled target < vanilla:
    proposed = max(scaled target, 50% of vanilla)
else:
    proposed = scaled target
```

Thus this first pass never removes more than half of a module's vanilla bulk
upkeep. Three preservation restrictions apply after the physical calculation:

1. A module with no vanilla resource maintenance remains at zero.
2. A module with resource maintenance retains every existing resource type;
   reductions scale its mix proportionally.
3. Final resource maintenance does not rise above vanilla unless the unscaled
   realistic estimate itself is already greater than vanilla.

The first restriction replaces the former farm-specific exception. It reaches
the same result for Hydroponics Bay, Farm, and Agriculture Complex while also
protecting every other zero-resource module from newly introduced upkeep.

### Approved power-plant aggregation rule

After calculating the ordinary maintenance proposal, every module with direct
positive power output receives a 2x representation multiplier:

```text
power output                 x2
monthly material maintenance x2
crew                          x2
construction resource/boost cost x2
```

This means one placed power-plant module represents two of the former plant
units. Output and every principal burden double together, so operating and
construction burden per unit of generation remain unchanged. Old saves gain
power headroom immediately instead of having existing layouts power down and
forcing widespread redesign after the hab rebalance is installed.

The multiplier applies to direct generators (`power > 0`), not to a Solar
Mirror that modifies a separate collector. It is applied after the physical
maintenance calculation and the 50% reduction ceiling, then remains subject
to the three preservation restrictions. Zero crew remains zero when doubled.
"Construction cost" means resource and boost cost, not physical mass or build
time; those should remain unchanged unless separately approved.

For T1 this covers Automated Fission Pile, Automated Solar Collector, Fission
Pile, Fusion Pile, Heavy Fission Pile, Heavy Fusion Pile, and Solar Collector.
The table's proposed maintenance values include this final multiplier.

| Generator | Power now | Planned power | Crew now | Planned crew | Pre-aggregation maintenance (t/mo) | Final maintenance (t/mo) | Construction resource/boost cost |
|---|---:|---:|---:|---:|---:|---:|---:|
| Automated Fission Pile | 20 | 40 | 0 | 0 | 2.75 | 5.50 | 2x |
| Automated Solar Collector | 20 | 40 | 0 | 0 | 0 | 0 | 2x |
| Fission Pile | 20 | 40 | 1 | 2 | 4.00 | 8.00 | 2x |
| Fusion Pile | 40 | 80 | 1 | 2 | 7.60 | 15.20 | 2x |
| Heavy Fission Pile | 30 | 60 | 1 | 2 | 4.125 | 8.25 | 2x |
| Heavy Fusion Pile | 70 | 140 | 2 | 4 | 8.60 | 17.20 | 2x |
| Solar Collector | 20 | 40 | 0 | 0 | 0 | 0 | 2x |

Crew-generated water and volatile demand is not included in the module table
and is not changed by this proposal. It supplied the scaling yardstick and
continues to be charged separately by game code.

## Units and abbreviations

- All mixes are in game resource points per month.
- One point is 10 physical tonnes.
- `B` = boost, `W` = water, `V` = volatiles, `M` = metals,
  `N` = noble metals, and `F` = fissiles.
- Boost is counted by its deka-ton launch-capacity equivalent.
- Money is shown separately because it has no physical mass conversion.
- The delta is proposed physical-equivalent tonnes minus vanilla
  physical-equivalent tonnes per month.

## Full T1 table

| Module | Vanilla resource mix (points/mo) | Money/mo | Vanilla t/mo | Generous real t/mo | 7x target t/mo | Proposed resource mix (points/mo) | Proposed t/mo | Delta t/mo | Preferred non-resource balance lever |
|---|---|---:|---:|---:|---:|---|---:|---:|---|
| Administration Node | B 1; V .03; M .03; N .03 | 10 | 10.90 | 0.30 | 2.10 | B .5; V .015; M .015; N .015 | 5.45 | -5.45 | Money primary; construction secondary |
| Antimatter Trap | M .1; N .03 | 2 | 1.30 | 0.08 | 0.56 | M .05; N .015 | 0.65 | -0.65 | Power primary; construction |
| Automated Fission Pile | W .5; F .05 | 0 | 5.50 | 0.14 | 0.98 | W .5; F .05 | 5.50 | 0 | Construction; money if needed |
| Automated Mining Complex | W 2; V .5 | 0 | 25.00 | 1.50 | 10.50 | W 1; V .25 | 12.50 | -12.50 | Power primary; construction |
| Automated Outpost Core | — | 0 | 0 | 0.03 | 0.21 | — | 0 | 0 | Construction; resource upkeep unchanged |
| Automated Platform Core | — | 0 | 0 | 0.03 | 0.21 | — | 0 | 0 | Construction; resource upkeep unchanged |
| Automated Solar Collector | — | 0 | 0 | 0.03 | 0.21 | — | 0 | 0 | 2x plant rule; resource upkeep unchanged |
| Automated Solar Mirror | M .1; N .03 | 1 | 1.30 | 0.08 | 0.56 | M .05; N .015 | 0.65 | -0.65 | Construction; money if needed |
| Automated Supply Depot | — | 0 | 0 | 0.04 | 0.28 | — | 0 | 0 | Construction; resource upkeep unchanged |
| Broadcast Outlet | — | 4 | 0 | 0.08 | 0.56 | — | 0 | 0 | Money and power |
| Climate Lab | — | 2 | 0 | 0.10 | 0.70 | — | 0 | 0 | Power and money |
| Construction Module | W 1; V 1; M 3; N .25 | 3 | 52.50 | 0.25 | 1.75 | W .5; V .5; M 1.5; N .125 | 26.25 | -26.25 | Power primary; construction and money |
| Energy Lab | V 1; F .001 | 2 | 10.01 | 0.25 | 1.75 | V .5; F .0005 | 5.005 | -5.005 | Power primary; money |
| Fission Pile | W .5; V .25; F .05 | 2 | 8.00 | 0.10 | 0.70 | W .5; V .25; F .05 | 8.00 | 0 | Construction; retain physical fuel; money |
| Fusion Pile | W 1; V .5; F .02 | 3 | 15.20 | 0.15 | 1.05 | W 1; V .5; F .02 | 15.20 | 0 | Construction; money |
| Heavy Fission Pile | W .5; V .25; F .075 | 3 | 8.25 | 0.18 | 1.26 | W .5; V .25; F .075 | 8.25 | 0 | Construction; retain physical fuel; money |
| Heavy Fusion Pile | W 1; V .5; M .1; N .1; F .02 | 5 | 17.20 | 0.25 | 1.75 | W 1; V .5; M .1; N .1; F .02 | 17.20 | 0 | Construction; money |
| Hydroponics Bay | — | 0 | 0 | 0.50 | 3.50 | — | 0 | 0 | Resource upkeep unchanged |
| Information Science Lab | — | 2 | 0 | 0.08 | 0.56 | — | 0 | 0 | Power and money |
| Life Science Lab | W .5; V .5 | 2 | 10.00 | 0.25 | 1.75 | W .25; V .25 | 5.00 | -5.00 | Power and money |
| Listening Post | — | 5 | 0 | 0.08 | 0.56 | — | 0 | 0 | Power and money |
| Marine Platoon Barracks | V 1; M 1; N .1 | 3 | 21.00 | 0.45 | 3.15 | V .5; M .5; N .05 | 10.50 | -10.50 | Money primary; construction and power |
| Materials Lab | M .1; N .1 | 2 | 2.00 | 0.15 | 1.05 | M .0525; N .0525 | 1.05 | -0.95 | Power and money |
| Military Science Lab | — | 2 | 0 | 0.10 | 0.70 | — | 0 | 0 | Power and money |
| Outpost Core | — | 3 | 0 | 0.06 | 0.42 | — | 0 | 0 | Construction; money secondary |
| Outpost Mining Complex | W 2; V .5 | 6 | 25.00 | 1.50 | 10.50 | W 1; V .25 | 12.50 | -12.50 | Power primary; money and construction |
| Particle Collider | W 2; V 2; M 1; N 1; F 1 | 6 | 70.00 | 1.00 | 7.00 | W 1; V 1; M .5; N .5; F .5 | 35.00 | -35.00 | Power overwhelmingly; construction and money |
| Platform Core | — | 2 | 0 | 0.05 | 0.35 | — | 0 | 0 | Construction; money secondary |
| Point Defense Array | V .1; M .1 | 2 | 2.00 | 0.15 | 1.05 | V .0525; M .0525 | 1.05 | -0.95 | Power and money; construction secondary |
| Quarters | W .1; V .1 | 0 | 2.00 | 0.07 | 0.49 | W .05; V .05 | 1.00 | -1.00 | Money primary; power secondary |
| Social Science Lab | — | 3 | 0 | 0.08 | 0.56 | — | 0 | 0 | Money primary; power secondary |
| Solar Collector | — | 1 | 0 | 0.03 | 0.21 | — | 0 | 0 | 2x plant rule; resource upkeep unchanged |
| Solar Mirror | M .1; N .03 | 1 | 1.30 | 0.08 | 0.56 | M .05; N .015 | 0.65 | -0.65 | Construction; money if needed |
| Space Dock | M 1; N .1 | 0 | 11.00 | 0.50 | 3.50 | M .5; N .05 | 5.50 | -5.50 | Power and money; construction secondary |
| Space Science Lab | — | 2 | 0 | 0.10 | 0.70 | — | 0 | 0 | Power and money |
| Supply Depot | — | 0 | 0 | 0.04 | 0.28 | — | 0 | 0 | Construction; money secondary |
| Tourist Berth | B .2 | 0 | 2.00 | 0.24 | 1.68 | B .168 | 1.68 | -0.32 | Money primary |
| Xenology Lab | — | 2 | 0 | 0.12 | 0.84 | — | 0 | 0 | Power and money |

With the preservation restrictions and final power-plant aggregation included,
T1 non-crew material and boost-equivalent maintenance falls from
**301.46 t/month to 178.585 t/month**, a delta of **-122.875 t/month**.

## Basis for the generous real estimates

These are deliberately high planning allowances, not claims that comparable
ISS hardware actually consumes this much:

- Static cores, communications equipment, collectors, and depots receive
  0.03-0.08 t/month for filters, seals, electronics, lubricants, and occasional
  replacement assemblies.
- Research modules receive 0.08-0.25 t/month. The high end allows discarded
  samples, targets, gases, culture media, filters, and unusually active
  experiments in addition to hardware spares.
- Hydroponics' comparison estimate is 0.50 t/month for nutrient makeup,
  substrate, filters, failed pumps and lamps, seeds, imperfect water closure,
  and discarded biomass. Its maintenance remains zero under the general rule
  against adding resource requirements to a module that lacks them.
- Fission and fusion plants receive 0.10-0.25 t/month for physically burned
  fuel, coolant leakage, filters, control components, pumps, electronics, and
  other spares. These values remain generous compared with energy-derived fuel
  burn.
- Construction receives 0.25 t/month and the dock 0.50 t/month for tool wear,
  welding and printer consumables, lubricants, seals, filters, and failed
  machinery. Material incorporated into a project, ship, or repair remains an
  activity cost.
- Each mine receives 1.50 t/month, the largest ordinary estimate, because a
  mine can wear cutters and seals and lose process fluids while handling very
  large regolith throughput. A later model should scale most of this with
  actual mine production.
- The particle collider receives a full 1.00 t/month. This is intentionally
  extravagant for targets, cryogens, detector gas, coolant losses, filters,
  electronics, and activated component replacement. Beam particle mass itself
  is negligible.
- Marine barracks receive 0.45 t/month for uniforms, filters, training stores,
  weapon maintenance, and facility spares. Food, oxygen, and water remain in
  the separate 30-person crew charge, while combat ammunition should be an
  event cost.
- Administration's 0.30 t/month and tourism's 0.24 t/month include an
  allowance for personnel turnover and launch logistics, explaining why boost
  remains in their proposed mixes.

## How to use the other balance levers

Resource upkeep should represent matter that physically leaves inventory.
The other levers have different strategic effects and should not be treated as
perfect substitutes:

- **Construction cost** is best for cores, solar systems, power plants,
  depots, and other durable capital equipment. It controls expansion tempo
  without pretending the structure is continually thrown away.
- **Power consumption** is best for laboratories, mining, antimatter capture,
  construction machinery, docks, point defense, communications, hydroponics,
  and especially the particle collider. It forces architectural tradeoffs and
  directly represents continuous operation.
- **Money** is the blank check for ground control, software, specialist labor,
  licensing, training, data processing, replacement contracts, insurance,
  administration, and other support that is economically real but has no
  useful space-resource composition. It is the cleanest balancing lever when
  neither mass nor power explains the desired operating burden.

Money should therefore be primary for administration, social science,
quarters, marines, and tourism; power should be primary for active science and
industrial systems; construction cost should be primary for passive and
long-lived infrastructure. A module does not automatically need a compensating
nerf merely because an indefensible material sink was removed—the lever should
follow the service the module actually provides and the gameplay constraint it
is meant to create.

## Interpretation

The approved 50% ceiling makes this a cautious transitional pass rather than
the final physical target. The worst modules remain very generous after the
change:

- Particle Collider: 35 t/month proposed versus 7 t/month game-scaled target.
- Construction Module: 26.25 t/month versus 1.75 t/month target.
- Space Dock: 5.5 t/month versus 3.5 t/month target.
- Mining Complex: 12.5 t/month versus 10.5 t/month target.

This is implemented together with the T2 and T3 decisions. Post-deployment
gameplay testing can determine whether
the collider and construction module warrant a later second pass toward their
scaled targets, with their balance burden moved principally into power,
construction cost, and money. Reactor maintenance intentionally remains near
vanilla per placed module because the approved module also represents twice
the former generation capacity.
