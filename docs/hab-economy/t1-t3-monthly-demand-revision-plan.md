# Consolidated T1-T3 monthly demand revision plan

Date: 2026-08-24  
Status: **implemented and deployed; manual in-game testing pending**

This document supersedes the maintenance targets in the implemented T1 and
T2/T3 proposal documents for the next revision. It plans all three tiers as
one change set and one deployment. The 2026-08-24 quarter-floor clarification
and the full plan are approved for implementation.

## Revised decisions

1. Personal upkeep becomes **2 t water/year plus 2 t volatiles/year**.
2. The ISS-to-game logistics scaler becomes **3.99x**, rounded to **4.0x**.
3. A fixed module resource demand may fall to **25% of vanilla**, allowing a
   maximum reduction of **75%**, unless another invariant prevents that cut.
4. Physical estimates use the upper end of every documented realistic range.
5. T1-T3 were recalculated and implemented together with the same rules.
6. T1 and T2 solar masses rise to create a gently improving specific-mass
   progression. Solar Array and Solar Farm operating crew become 1 and 3.

The phrase “2 t of water and volatiles” is interpreted here as 2 t of each
resource per person-year, not 2 t combined.

## Crew-consumption yardstick

| Quantity | Water | Volatiles | Total |
|---|---:|---:|---:|
| Revised game upkeep, person-year | 2.000 t | 2.000 t | 4.000 t |
| Revised game upkeep, person-month | 166.7 kg | 166.7 kg | 333.3 kg |
| ISS one-way personal supplies, person-month | — | — | 83.6 kg |

The ISS reference includes packaged food (55.7 kg/person-month), oxygen
(25.6 kg), and water makeup at 98% recovery (2.3 kg). Therefore:

```text
333.3 / 83.6 = 3.987
planning scaler = 4.0x
```

Crew-generated water and volatile demand remains separate from fixed module
maintenance. The tables below change only the fixed resource mixes.

## Invariants and calculation

The following invariants apply after the physical calculation:

1. Do not add a resource requirement to a module that has none.
2. Do not remove an existing resource type; scale the vanilla mix
   proportionally.
3. Do not increase fixed resource maintenance unless the **unscaled upper
   realistic estimate** is already greater than vanilla maintenance.
4. Preserve the approved direct-generator aggregation: output, physical mass,
   construction resources/Boost, and ordinary maintenance represent two old
   plants. Zero-maintenance solar remains at zero.
5. Money maintenance remains vanilla and is not part of physical tonnage.

For an ordinary module:

```text
scaled target = upper realistic monthly demand × 4.0

if scaled target < vanilla:
    preliminary = max(scaled target, 25% of vanilla)
else:
    preliminary = scaled target

direct positive-power generator: preliminary ×= 2
then apply the three resource-preservation invariants
```

The 2x generator step is intentionally retained for save compatibility. For
reactors, it normally turns the ordinary 25% floor into an effective 50% floor
for the final two-plant module. The no-unjustified-increase invariant still
caps any excessive result at vanilla. The multiplier does not create resource
upkeep for solar generators.

## Revised solar mass and crew targets

One power point is treated as approximately 1 MW for physical comparison. The
game never explicitly assigns that unit, so the kg/kW column is an analytical
interpretation rather than a new game rule.

| Module | Power | Current mass | Planned mass | Current kg/kW | Planned kg/kW | Current crew | Planned crew |
|---|---:|---:|---:|---:|---:|---:|---:|
| Automated Solar Collector | 40 | 50 t | **160 t** | 1.250 | **4.000** | 0 | 0 |
| Solar Collector | 40 | 50 t | **160 t** | 1.250 | **4.000** | 0 | 0 |
| Solar Array | 160 | 230 t | **620 t** | 1.438 | **3.875** | 10 | **1** |
| Solar Farm | 480 | 1,800 t | **1,800 t** | 3.750 | **3.750** | 50 | **3** |

The rounded 160/620/1,800 t sequence makes specific mass improve slightly at
each tier: 4.000, 3.875, then 3.750 kg/kW. T3 mass and all solar output stay
unchanged. Because construction resource mass is derived from physical mass,
the T1 and T2 construction bills rise with these masses without adding a new
multiplier. Build time remains unchanged. Solar fixed resource maintenance
remains zero under invariant 1.

The T2 and T3 crew figures are final module totals. They explicitly replace
the former automatic 2x solar-crew values; they are not doubled again.

## Aggregate result

| Tier | Modules | Vanilla fixed upkeep | Proposed fixed upkeep | Delta |
|---:|---:|---:|---:|---:|
| T1 | 38 | 301.46 t/mo | 89.5625 t/mo | -211.8975 t/mo |
| T2 | 35 | 1520.35 t/mo | 457.1125 t/mo | -1063.2375 t/mo |
| T3 | 37 | 4781.6 t/mo | 2049.05 t/mo | -2732.55 t/mo |
| **Combined** | **110** | **6603.41 t/mo** | **2595.725 t/mo** | **-4007.685 t/mo** |

This is a **60.69%** reduction in fixed physical upkeep across
the 110 modules. Money and separately generated crew upkeep are excluded.

## Units and abbreviations

Resource mixes are game points per month; one point is 10 tonnes. `B` = Boost,
`W` = water, `V` = volatiles, `M` = base metals, `N` = noble metals, and
`F` = fissiles. Money is shown separately because it has no physical mass
conversion. Deltas are proposed minus vanilla, so negative values are cuts.

## T1 proposal table

| Module | Vanilla resource mix (points/mo) | Money/mo | Vanilla t/mo | Upper realistic t/mo | 4x target t/mo | Proposed resource mix (points/mo) | Proposed t/mo | Delta t/mo | Preferred non-resource balance lever |
|---|---|---:|---:|---:|---:|---|---:|---:|---|
| Administration Node | B 1; V .03; M .03; N .03 | 10 | 10.9 | 0.3 | 1.2 | B 0.25; V 0.0075; M 0.0075; N 0.0075 | 2.725 | -8.175 | Money primary; construction secondary |
| Antimatter Trap | M .1; N .03 | 2 | 1.3 | 0.08 | 0.32 | M 0.025; N 0.0075 | 0.325 | -0.975 | Power primary; construction |
| Automated Fission Pile | W .5; F .05 | 0 | 5.5 | 0.14 | 0.56 | W 0.25; F 0.025 | 2.75 | -2.75 | Construction; money if needed |
| Automated Mining Complex | W 2; V .5 | 0 | 25 | 1.5 | 6 | W 0.5; V 0.125 | 6.25 | -18.75 | Power primary; construction |
| Automated Outpost Core | — | 0 | 0 | 0.03 | 0.12 | — | 0 | 0 | Construction; resource upkeep unchanged |
| Automated Platform Core | — | 0 | 0 | 0.03 | 0.12 | — | 0 | 0 | Construction; resource upkeep unchanged |
| Automated Solar Collector | — | 0 | 0 | 0.03 | 0.12 | — | 0 | 0 | 2x plant rule; resource upkeep unchanged |
| Automated Solar Mirror | M .1; N .03 | 1 | 1.3 | 0.08 | 0.32 | M 0.025; N 0.0075 | 0.325 | -0.975 | Construction; money if needed |
| Automated Supply Depot | — | 0 | 0 | 0.04 | 0.16 | — | 0 | 0 | Construction; resource upkeep unchanged |
| Broadcast Outlet | — | 4 | 0 | 0.08 | 0.32 | — | 0 | 0 | Money and power |
| Climate Lab | — | 2 | 0 | 0.025 | 0.1 | — | 0 | 0 | Power and money |
| Construction Module | W 1; V 1; M 3; N .25 | 3 | 52.5 | 0.25 | 1 | W 0.25; V 0.25; M 0.75; N 0.0625 | 13.125 | -39.375 | Power primary; construction and money |
| Energy Lab | V 1; F .001 | 2 | 10.01 | 0.12 | 0.48 | V 0.25; F 0.00025 | 2.5025 | -7.5075 | Power primary; money |
| Fission Pile | W .5; V .25; F .05 | 2 | 8 | 0.1 | 0.4 | W 0.25; V 0.125; F 0.025 | 4 | -4 | Construction; retain physical fuel; money |
| Fusion Pile | W 1; V .5; F .02 | 3 | 15.2 | 0.15 | 0.6 | W 0.5; V 0.25; F 0.01 | 7.6 | -7.6 | Construction; money |
| Heavy Fission Pile | W .5; V .25; F .075 | 3 | 8.25 | 0.18 | 0.72 | W 0.25; V 0.125; F 0.0375 | 4.125 | -4.125 | Construction; retain physical fuel; money |
| Heavy Fusion Pile | W 1; V .5; M .1; N .1; F .02 | 5 | 17.2 | 0.25 | 1 | W 0.5; V 0.25; M 0.05; N 0.05; F 0.01 | 8.6 | -8.6 | Construction; money |
| Hydroponics Bay | — | 0 | 0 | 0.5 | 2 | — | 0 | 0 | Resource upkeep unchanged |
| Information Science Lab | — | 2 | 0 | 0.015 | 0.06 | — | 0 | 0 | Power and money |
| Life Science Lab | W .5; V .5 | 2 | 10 | 0.18 | 0.72 | W 0.125; V 0.125 | 2.5 | -7.5 | Power and money |
| Listening Post | — | 5 | 0 | 0.08 | 0.32 | — | 0 | 0 | Power and money |
| Marine Platoon Barracks | V 1; M 1; N .1 | 3 | 21 | 0.45 | 1.8 | V 0.25; M 0.25; N 0.025 | 5.25 | -15.75 | Money primary; construction and power |
| Materials Lab | M .1; N .1 | 2 | 2 | 0.15 | 0.6 | M 0.03; N 0.03 | 0.6 | -1.4 | Power and money |
| Military Science Lab | — | 2 | 0 | 0.05 | 0.2 | — | 0 | 0 | Power and money |
| Outpost Core | — | 3 | 0 | 0.06 | 0.24 | — | 0 | 0 | Construction; money secondary |
| Outpost Mining Complex | W 2; V .5 | 6 | 25 | 1.5 | 6 | W 0.5; V 0.125 | 6.25 | -18.75 | Power primary; money and construction |
| Particle Collider | W 2; V 2; M 1; N 1; F 1 | 6 | 70 | 1 | 4 | W 0.5; V 0.5; M 0.25; N 0.25; F 0.25 | 17.5 | -52.5 | Power overwhelmingly; construction and money |
| Platform Core | — | 2 | 0 | 0.05 | 0.2 | — | 0 | 0 | Construction; money secondary |
| Point Defense Array | V .1; M .1 | 2 | 2 | 0.15 | 0.6 | V 0.03; M 0.03 | 0.6 | -1.4 | Power and money; construction secondary |
| Quarters | W .1; V .1 | 0 | 2 | 0.07 | 0.28 | W 0.025; V 0.025 | 0.5 | -1.5 | Money primary; power secondary |
| Social Science Lab | — | 3 | 0 | 0.015 | 0.06 | — | 0 | 0 | Money primary; power secondary |
| Solar Collector | — | 1 | 0 | 0.03 | 0.12 | — | 0 | 0 | 2x plant rule; resource upkeep unchanged |
| Solar Mirror | M .1; N .03 | 1 | 1.3 | 0.08 | 0.32 | M 0.025; N 0.0075 | 0.325 | -0.975 | Construction; money if needed |
| Space Dock | M 1; N .1 | 0 | 11 | 0.5 | 2 | M 0.25; N 0.025 | 2.75 | -8.25 | Power and money; construction secondary |
| Space Science Lab | — | 2 | 0 | 0.03 | 0.12 | — | 0 | 0 | Power and money |
| Supply Depot | — | 0 | 0 | 0.04 | 0.16 | — | 0 | 0 | Construction; money secondary |
| Tourist Berth | B .2 | 0 | 2 | 0.24 | 0.96 | B 0.096 | 0.96 | -1.04 | Money primary |
| Xenology Lab | — | 2 | 0 | 0.25 | 1 | — | 0 | 0 | Power and money |

## T2 proposal table

| Module | Vanilla resource mix (points/mo) | Money/mo | Vanilla t/mo | Upper realistic t/mo | 4x target t/mo | Proposed resource mix (points/mo) | Proposed t/mo | Delta t/mo | Preferred non-resource balance lever |
|---|---|---:|---:|---:|---:|---|---:|---:|---|
| Administration Tower | B 4; V .1; M .1; N .1 | 30 | 43 | 1.5 | 6 | B 1; V 0.025; M 0.025; N 0.025 | 10.75 | -32.25 | Money |
| Antimatter Harvester | M .5; N .1 | 5 | 6 | 0.4 | 1.6 | M 0.133333; N 0.026667 | 1.6 | -4.4 | Power + construction |
| Atomsmasher | W 10; V 10; M 5; N 5; F 3 | 20 | 330 | 5 | 20 | W 2.5; V 2.5; M 1.25; N 1.25; F 0.75 | 82.5 | -247.5 | Power + construction + money |
| Climate Research Center | V .1; M .1; N .1 | 6 | 3 | 0.5 | 2 | V 0.066667; M 0.066667; N 0.066667 | 2 | -1 | Power + money |
| Communications Hub | V .1; M .1; N .1 | 20 | 3 | 0.4 | 1.6 | V 0.053333; M 0.053333; N 0.053333 | 1.6 | -1.4 | Money + power |
| Deep Space Telescope | — | 3 | 0 | 0.3 | 1.2 | — | 0 | 0 | Construction + power |
| Energy Research Center | V 3; M .1; N .1; F .005 | 6 | 32.05 | 1.25 | 5 | V 0.75; M 0.025; N 0.025; F 0.00125 | 8.0125 | -24.0375 | Power + money |
| Farm | — | 0 | 0 | 2.5 | 10 | — | 0 | 0 | Resource upkeep unchanged |
| Fission Reactor Array | W 2; V 1; F .2 | 6 | 32 | 0.5 | 2 | W 1; V 0.5; F 0.1 | 16 | -16 | 2x plant rule; construction |
| Fusion Reactor Array | W 3; V 2; F .05 | 10 | 50.5 | 0.75 | 3 | W 1.5; V 1; F 0.025 | 25.25 | -25.25 | 2x plant rule; construction |
| Heavy Fission Reactor Array | W 2; V 1; F .3 | 8 | 33 | 0.9 | 3.6 | W 1; V 0.5; F 0.15 | 16.5 | -16.5 | 2x plant rule; construction |
| Heavy Fusion Reactor Array | W 3; V 2; M .5; N .5; F .08 | 12 | 60.8 | 1.25 | 5 | W 1.5; V 1; M 0.25; N 0.25; F 0.04 | 30.4 | -30.4 | 2x plant rule; construction |
| Information Science Research Center | W 1; V .1; M .1; N .1 | 6 | 13 | 0.4 | 1.6 | W 0.25; V 0.025; M 0.025; N 0.025 | 3.25 | -9.75 | Power + money |
| Layered Defense Array | V 1; M 1; N .5 | 10 | 25 | 0.75 | 3 | V 0.25; M 0.25; N 0.125 | 6.25 | -18.75 | Power + money |
| Life Science Research Center | W 1; V 1 | 6 | 20 | 1.25 | 5 | W 0.25; V 0.25 | 5 | -15 | Power + money |
| Marine Company Barracks | V 2; M 2; N .2 | 10 | 42 | 2.25 | 9 | V 0.5; M 0.5; N 0.05 | 10.5 | -31.5 | Money + construction |
| Materials Research Center | W 1; V 1; M 1; N .5 | 6 | 35 | 0.75 | 3 | W 0.25; V 0.25; M 0.25; N 0.125 | 8.75 | -26.25 | Power + money |
| Military Science Research Center | V .1; M .1; N .1 | 6 | 3 | 0.5 | 2 | V 0.066667; M 0.066667; N 0.066667 | 2 | -1 | Power + money |
| Nanofactory | W 3; V 3; M 10; N 1 | 10 | 170 | 1.25 | 5 | W 0.75; V 0.75; M 2.5; N 0.25 | 42.5 | -127.5 | Power + construction + money |
| Operations Center | V 5; M 5; N 2.5 | 30 | 125 | 5 | 20 | V 1.25; M 1.25; N 0.625 | 31.25 | -93.75 | Money + power |
| Orbital Core | — | 10 | 0 | 0.3 | 1.2 | — | 0 | 0 | Construction + money |
| Orbital Hospital | B 1; W 5; V 3 | 0 | 90 | 5 | 20 | B 0.25; W 1.25; V 0.75 | 22.5 | -67.5 | Money + power |
| Reconnaissance Array | V 1; M 1; N .1 | 15 | 21 | 0.4 | 1.6 | V 0.25; M 0.25; N 0.025 | 5.25 | -15.75 | Power + money |
| Research Campus | W 3; V 3 | 30 | 60 | 3 | 12 | W 0.75; V 0.75 | 15 | -45 | Money + power |
| Residential Module | B .5; W 3; V 1; M 1 | 0 | 55 | 1.5 | 6 | B 0.125; W 0.75; V 0.25; M 0.25 | 13.75 | -41.25 | Money |
| Settlement Core | — | 10 | 0 | 0.3 | 1.2 | — | 0 | 0 | Construction + money |
| Settlement Mining Complex | W 5; V 2 | 30 | 70 | 7.5 | 30 | W 2.142857; V 0.857143 | 30 | -40 | Power + money |
| Shipyard | M 3; N .5 | 0 | 35 | 2.5 | 10 | M 0.857143; N 0.142857 | 10 | -25 | Power + construction + money |
| Skunk Works | V 3; M 3; N .3 | 10 | 63 | 1.5 | 6 | V 0.75; M 0.75; N 0.075 | 15.75 | -47.25 | Power + money |
| Social Science Research Center | V .1; M .1; N .1 | 8 | 3 | 0.4 | 1.6 | V 0.053333; M 0.053333; N 0.053333 | 1.6 | -1.4 | Money + power |
| Solar Array | — | 3 | 0 | 0.15 | 0.6 | — | 0 | 0 | 2x plant rule; resource upkeep unchanged |
| Solar Mirror Array | M 1; N .1 | 5 | 11 | 0.4 | 1.6 | M 0.25; N 0.025 | 2.75 | -8.25 | Construction |
| Space Hotel | B 3; W 3; V 2 | 0 | 80 | 8 | 32 | B 1.2; W 1.2; V 0.8 | 32 | -48 | Money |
| Space Science Research Center | V .1; M .1; N .1 | 6 | 3 | 0.5 | 2 | V 0.066667; M 0.066667; N 0.066667 | 2 | -1 | Power + money |
| Xenoscience Research Center | V .1; M .1; N .1 | 6 | 3 | 0.6 | 2.4 | V 0.08; M 0.08; N 0.08 | 2.4 | -0.6 | Power + money |

## T3 proposal table

| Module | Vanilla resource mix (points/mo) | Money/mo | Vanilla t/mo | Upper realistic t/mo | 4x target t/mo | Proposed resource mix (points/mo) | Proposed t/mo | Delta t/mo | Preferred non-resource balance lever |
|---|---|---:|---:|---:|---:|---|---:|---:|---|
| Administration Complex | B 12; V .5; M .5; N .5 | 90 | 135 | 12 | 48 | B 4.266667; V 0.177778; M 0.177778; N 0.177778 | 48 | -87 | Money |
| Agriculture Complex | — | 0 | 0 | 20 | 80 | — | 0 | 0 | Resource upkeep unchanged |
| Antimatter Farm | M 1.5; N 1 | 10 | 25 | 3.2 | 12.8 | M 0.768; N 0.512 | 12.8 | -12.2 | Power + construction |
| Argus Complex | V 3; M 3; N .3 | 30 | 63 | 3 | 12 | V 0.75; M 0.75; N 0.075 | 15.75 | -47.25 | Power + money |
| Battlestations | V 3; M 3; N 1 | 30 | 70 | 6 | 24 | V 1.028571; M 1.028571; N 0.342857 | 24 | -46 | Power + construction + money |
| Civilian Complex | B 1; W 10; V 6; M 2 | 0 | 190 | 10 | 40 | B 0.25; W 2.5; V 1.5; M 0.5 | 47.5 | -142.5 | Money |
| Climate Institute | V .5; M .5; N .5 | 20 | 15 | 4 | 16 | V 0.5; M 0.5; N 0.5 | 15 | 0 | Power + money |
| Colony Core | — | 20 | 0 | 2.4 | 9.6 | — | 0 | 0 | Construction + money |
| Colony Mining Complex | W 15; V 6 | 60 | 210 | 60 | 240 | W 15; V 6 | 210 | 0 | Power + money |
| Command Center | V 10; M 10; N 5 | 100 | 250 | 40 | 160 | V 6.4; M 6.4; N 3.2 | 160 | -90 | Money + power |
| Energy Institute | V 10; M .5; N .5; F .01 | 18 | 110.1 | 10 | 40 | V 3.633061; M 0.181653; N 0.181653; F 0.003633 | 40 | -70.1 | Power + money |
| Fission Reactor Farm | W 6; V 3; F .5 | 18 | 95 | 4 | 16 | W 3; V 1.5; F 0.25 | 47.5 | -47.5 | 2x plant rule; construction |
| Foundry | V 10; M 10; N 1 | 30 | 210 | 12 | 48 | V 2.5; M 2.5; N 0.25 | 52.5 | -157.5 | Power + money |
| Fusion Reactor Farm | W 10; V 6; F .1 | 30 | 161 | 6 | 24 | W 5; V 3; F 0.05 | 80.5 | -80.5 | 2x plant rule; construction |
| Geriatrics Facility | B 3; W 15; V 10 | 0 | 280 | 30 | 120 | B 1.285714; W 6.428571; V 4.285714 | 120 | -160 | Money + power |
| Heavy Fission Reactor Farm | W 6; V 3; F .75 | 24 | 97.5 | 7.2 | 28.8 | W 3.544615; V 1.772308; F 0.443077 | 57.6 | -39.9 | 2x plant rule; construction |
| Heavy Fusion Reactor Farm | W 10; V 6; M 1; N 1; F .2 | 36 | 182 | 10 | 40 | W 5; V 3; M 0.5; N 0.5; F 0.1 | 91 | -91 | 2x plant rule; construction |
| Helium-3 Mine | W 2; V 2; M 10; N 1.5 | 30 | 155 | 8 | 32 | W 0.5; V 0.5; M 2.5; N 0.375 | 38.75 | -116.25 | Power + construction |
| Information Science Institute | W 3; V .5; M .5; N .5 | 18 | 45 | 3.2 | 12.8 | W 0.853333; V 0.142222; M 0.142222; N 0.142222 | 12.8 | -32.2 | Power + money |
| Interstellar Launching Laser | — | 20 | 0 | 300 | 1200 | — | 0 | 0 | Power + construction |
| Life Science Institute | W 3; V 3 | 18 | 60 | 10 | 40 | W 2; V 2 | 40 | -20 | Power + money |
| Marine Battalion Barracks | V 3; M 3; N .3 | 30 | 63 | 18 | 72 | V 3; M 3; N 0.3 | 63 | 0 | Money + construction |
| Materials Institute | W 3; V 3; M 3; N 1.5 | 18 | 105 | 6 | 24 | W 0.75; V 0.75; M 0.75; N 0.375 | 26.25 | -78.75 | Power + money |
| Media Center | V .5; M .5; N .5 | 100 | 15 | 3.2 | 12.8 | V 0.426667; M 0.426667; N 0.426667 | 12.8 | -2.2 | Money + power |
| Military Science Institute | V .5; M .5; N .5 | 18 | 15 | 4 | 16 | V 0.5; M 0.5; N 0.5 | 15 | 0 | Power + money |
| Nanofacturing Complex | W 10; V 10; M 30; N 3 | 20 | 530 | 10 | 40 | W 2.5; V 2.5; M 7.5; N 0.75 | 132.5 | -397.5 | Power + construction + money |
| Research University | W 10; V 10 | 100 | 200 | 24 | 96 | W 4.8; V 4.8 | 96 | -104 | Money + power |
| Ring Core | — | 20 | 0 | 2.4 | 9.6 | — | 0 | 0 | Construction + money |
| Sentinel Complex | — | 20 | 0 | 10 | 40 | — | 0 | 0 | Power + construction + money |
| Social Science Institute | V .5; M .5; N .5 | 24 | 15 | 3 | 12 | V 0.4; M 0.4; N 0.4 | 12 | -3 | Money + power |
| Solar Farm | — | 5 | 0 | 1.1 | 4.4 | — | 0 | 0 | 2x plant rule; resource upkeep unchanged |
| Soletta | M 3; N .5 | 10 | 35 | 3.2 | 12.8 | M 1.097143; N 0.182857 | 12.8 | -22.2 | Construction |
| Space Resort | B 6; W 10; V 5 | 0 | 210 | 45 | 180 | B 5.142857; W 8.571429; V 4.285714 | 180 | -30 | Money |
| Space Science Institute | V .5; M .5; N .5 | 18 | 15 | 4 | 16 | V 0.5; M 0.5; N 0.5 | 15 | 0 | Power + money |
| Spaceworks | M 10; N 1 | 0 | 110 | 20 | 80 | M 7.272727; N 0.727273 | 80 | -30 | Power + construction + money |
| Supercollider | W 30; V 30; M 20; N 20; F 10 | 120 | 1100 | 40 | 160 | W 7.5; V 7.5; M 5; N 5; F 2.5 | 275 | -825 | Power + construction + money |
| Xenoscience Institute | V .5; M .5; N .5 | 18 | 15 | 4.8 | 19.2 | V 0.5; M 0.5; N 0.5 | 15 | 0 | Power + money |

## Deployment record

The complete revision was deployed on 2026-08-24 through `tools\deploy.ps1`.
The pipeline rebuilt the mod, passed **1,159 formula assertions**, validated all
110 hab-module overrides and their starting-station consequences, packaged the
release, and hash-checked all **46 deployed files** against the authored mod.

- Release artifact: `artifacts/TIEconomyMod-0.9.4-ti1.0.51.zip`
- Artifact SHA256: `78E629F62A7DBE6933BC0DB97AD2D38AEB7048BC3415576CF9B40331D482AA20`
- DLL SHA256: `DF8946BFB4D416740569F08B65267C48F5BE5C0F780BABDEFF6BC6146C5C7606`
- Compatibility target: Terra Invicta 1.0.51
- Manual acceptance: pending old-save and new-construction testing

## Estimate handling

The T1 laboratory estimates use the upper endpoints from the dedicated
[solar and T1 laboratory audit](solar-power-and-t1-lab-audit.md): 0.025 t/mo
for Climate, 0.120 Energy, 0.015 Information Science, 0.180 Life Science,
0.150 Materials, 0.050 Military Science, 0.015 Social Science, 0.030 Space
Science, and 0.250 Xenology. Those figures exclude personal scientist
supplies, which the game charges through crew upkeep.

Other rows retain the deliberately generous physical estimates documented in
the prior [T1](t1-monthly-demand-scaling-proposal.md) and
[T2/T3](t2-t3-monthly-demand-scaling-proposal.md) analyses. Where those
analyses offered a range, this revision uses its upper endpoint.

## Implemented one-pass workflow

1. **Plan and document:** froze this 110-row table and the solar mass/crew
   targets as the approved specification.
2. **Implement:** updated the machine-readable proposal, all T1-T3 maintenance
   values, global crew water/volatile use, solar masses, and solar crew in one
   change set.
3. **Build, validate, and deploy:** completed the normal `tools\deploy.ps1`
   pipeline with its Terra Invicta process guards and without bypassing checks.
4. **Test:** manual comparison of representative old saves and new construction
   remains pending, with focus on hab power, crew logistics, module maintenance
   mixes, solar construction mass, and UI rounding.
5. **Document:** automated results and hashes are recorded above; manual
   findings will be added after in-game acceptance.
