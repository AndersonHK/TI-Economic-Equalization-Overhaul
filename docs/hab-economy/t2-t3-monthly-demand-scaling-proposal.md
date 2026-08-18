# T2 and T3 monthly demand scaling proposal

Date: 2026-08-17  
Status: **implemented and deployed** with the approved T1 rules; manual
in-game testing pending

The machine-readable approved values are in
[`hab-module-maintenance-proposals.csv`](hab-module-maintenance-proposals.csv).
Direct generator output and crew are doubled in the template overrides, while
the runtime hab-cost rewrite doubles their construction resources and Boost
without changing physical mass or build time. Existing saves therefore retain
valid layouts and gain power headroom on load. Money maintenance remains
vanilla for every module.

Deployment verification on 2026-08-17 passed the complete TI 1.0.51 release
pipeline, including 1,070 formula assertions, guarded Harmony/IL checks, exact
110-module proposal coverage, and hash verification of all 44 deployed files.

## Method

This pass applies the approved T1 method documented in
[`t1-monthly-demand-scaling-proposal.md`](t1-monthly-demand-scaling-proposal.md):

- generous real monthly material loss is multiplied by the 7.0x
  ISS-to-game crew-supply scaler;
- a reduction stops at 50% of vanilla in this first pass;
- direct power generators then receive the approved 2x output, maintenance,
  crew, and construction resource/boost cost multiplier; and
- crew-generated water and volatiles remain separate from these tables.

Three preservation restrictions apply to the final result:

1. No resource requirement is added to a module that has none in vanilla.
2. No existing resource type is removed; reductions preserve the vanilla mix
   proportionally.
3. Maintenance does not increase unless the unscaled realistic estimate is
   itself greater than vanilla. A 7x-scaled target alone cannot justify an
   increase.

The first restriction supersedes the obsolete farm-specific exception and
keeps Hydroponics Bay, Farm, Agriculture Complex, and every other zero-resource
module unchanged.

All resource mixes are in game points per month; one point is 10 tonnes.
`B` = boost, `W` = water, `V` = volatiles, `M` = metals,
`N` = noble metals, and `F` = fissiles. Money is separate because it has no
physical mass conversion.

## Power-plant aggregation

The maintenance column in the full tables already includes the final 2x
power-plant multiplier.

| Tier | Generator | Power now | Planned power | Crew now | Planned crew | Pre-aggregation maintenance (t/mo) | Final maintenance (t/mo) | Construction resource/boost cost |
|---:|---|---:|---:|---:|---:|---:|---:|---:|
| T2 | Fission Reactor Array | 85 | 170 | 20 | 40 | 16.0 | 32.0 | 2x |
| T2 | Fusion Reactor Array | 170 | 340 | 25 | 50 | 25.25 | 50.5 | 2x |
| T2 | Heavy Fission Reactor Array | 128 | 256 | 20 | 40 | 16.5 | 33.0 | 2x |
| T2 | Heavy Fusion Reactor Array | 300 | 600 | 50 | 100 | 30.4 | 60.8 | 2x |
| T2 | Solar Array | 80 | 160 | 5 | 10 | 0 | 0 | 2x |
| T3 | Fission Reactor Farm | 250 | 500 | 100 | 200 | 47.5 | 95.0 | 2x |
| T3 | Fusion Reactor Farm | 500 | 1000 | 125 | 250 | 80.5 | 161.0 | 2x |
| T3 | Heavy Fission Reactor Farm | 375 | 750 | 100 | 200 | 48.75 | 97.5 | 2x |
| T3 | Heavy Fusion Reactor Farm | 900 | 1800 | 150 | 300 | 91.0 | 182.0 | 2x |
| T3 | Solar Farm | 240 | 480 | 25 | 50 | 0 | 0 | 2x |

Physical mass and build time remain unchanged. Construction cost means only
the resource and boost amount paid to build the generator.

The Heavy Fission Reactor Farm is the sole direct conflict between the raw 2x
maintenance result and the new increase restriction. Its ordinary scaled
proposal would produce 100.8 t/month after aggregation, but its unscaled
realistic estimate (7.2 t/month) is below vanilla (97.5 t/month). The final
value is therefore capped at 97.5 t/month; the table shows the effective
48.75 t/month pre-aggregation equivalent.

## T2 table

| Module | Vanilla resource mix (points/mo) | Money/mo | Vanilla t/mo | Generous real t/mo | 7x target t/mo | Proposed resource mix (points/mo) | Proposed t/mo | Delta t/mo | Preferred non-resource balance lever |
|---|---|---:|---:|---:|---:|---|---:|---:|---|
| Administration Tower | B 4; V .1; M .1; N .1 | 30 | 43 | 1.5 | 10.5 | B 2; V .05; M .05; N .05 | 21.5 | -21.5 | Money |
| Antimatter Harvester | M .5; N .1 | 5 | 6 | .4 | 2.8 | M .25; N .05 | 3 | -3 | Power + construction |
| Atomsmasher | W 10; V 10; M 5; N 5; F 3 | 20 | 330 | 5 | 35 | W 5; V 5; M 2.5; N 2.5; F 1.5 | 165 | -165 | Power + construction + money |
| Climate Research Center | V .1; M .1; N .1 | 6 | 3 | .5 | 3.5 | V .1; M .1; N .1 | 3 | 0 | Power + money |
| Communications Hub | V .1; M .1; N .1 | 20 | 3 | .4 | 2.8 | V .0933; M .0933; N .0933 | 2.8 | -.2 | Money + power |
| Deep Space Telescope | — | 3 | 0 | .3 | 2.1 | — | 0 | 0 | Construction + power |
| Energy Research Center | V 3; M .1; N .1; F .005 | 6 | 32.05 | 1.25 | 8.75 | V 1.5; M .05; N .05; F .0025 | 16.025 | -16.025 | Power + money |
| Farm | — | 0 | 0 | 2.5 | 17.5 | — | 0 | 0 | Resource upkeep unchanged |
| Fission Reactor Array | W 2; V 1; F .2 | 6 | 32 | .5 | 3.5 | W 2; V 1; F .2 | 32 | 0 | 2x plant rule; construction |
| Fusion Reactor Array | W 3; V 2; F .05 | 10 | 50.5 | .75 | 5.25 | W 3; V 2; F .05 | 50.5 | 0 | 2x plant rule; construction |
| Heavy Fission Reactor Array | W 2; V 1; F .3 | 8 | 33 | .9 | 6.3 | W 2; V 1; F .3 | 33 | 0 | 2x plant rule; construction |
| Heavy Fusion Reactor Array | W 3; V 2; M .5; N .5; F .08 | 12 | 60.8 | 1.25 | 8.75 | W 3; V 2; M .5; N .5; F .08 | 60.8 | 0 | 2x plant rule; construction |
| Information Science Research Center | W 1; V .1; M .1; N .1 | 6 | 13 | .4 | 2.8 | W .5; V .05; M .05; N .05 | 6.5 | -6.5 | Power + money |
| Layered Defense Array | V 1; M 1; N .5 | 10 | 25 | .75 | 5.25 | V .5; M .5; N .25 | 12.5 | -12.5 | Power + money |
| Life Science Research Center | W 1; V 1 | 6 | 20 | 1.25 | 8.75 | W .5; V .5 | 10 | -10 | Power + money |
| Marine Company Barracks | V 2; M 2; N .2 | 10 | 42 | 2.25 | 15.75 | V 1; M 1; N .1 | 21 | -21 | Money + construction |
| Materials Research Center | W 1; V 1; M 1; N .5 | 6 | 35 | .75 | 5.25 | W .5; V .5; M .5; N .25 | 17.5 | -17.5 | Power + money |
| Military Science Research Center | V .1; M .1; N .1 | 6 | 3 | .5 | 3.5 | V .1; M .1; N .1 | 3 | 0 | Power + money |
| Nanofactory | W 3; V 3; M 10; N 1 | 10 | 170 | 1.25 | 8.75 | W 1.5; V 1.5; M 5; N .5 | 85 | -85 | Power + construction + money |
| Operations Center | V 5; M 5; N 2.5 | 30 | 125 | 5 | 35 | V 2.5; M 2.5; N 1.25 | 62.5 | -62.5 | Money + power |
| Orbital Core | — | 10 | 0 | .3 | 2.1 | — | 0 | 0 | Construction + money |
| Orbital Hospital | B 1; W 5; V 3 | 0 | 90 | 5 | 35 | B .5; W 2.5; V 1.5 | 45 | -45 | Money + power |
| Reconnaissance Array | V 1; M 1; N .1 | 15 | 21 | .4 | 2.8 | V .5; M .5; N .05 | 10.5 | -10.5 | Power + money |
| Research Campus | W 3; V 3 | 30 | 60 | 3 | 21 | W 1.5; V 1.5 | 30 | -30 | Money + power |
| Residential Module | B .5; W 3; V 1; M 1 | 0 | 55 | 1.5 | 10.5 | B .25; W 1.5; V .5; M .5 | 27.5 | -27.5 | Money |
| Settlement Core | — | 10 | 0 | .3 | 2.1 | — | 0 | 0 | Construction + money |
| Settlement Mining Complex | W 5; V 2 | 30 | 70 | 7.5 | 52.5 | W 3.75; V 1.5 | 52.5 | -17.5 | Power + money |
| Shipyard | M 3; N .5 | 0 | 35 | 2.5 | 17.5 | M 1.5; N .25 | 17.5 | -17.5 | Power + construction + money |
| Skunk Works | V 3; M 3; N .3 | 10 | 63 | 1.5 | 10.5 | V 1.5; M 1.5; N .15 | 31.5 | -31.5 | Power + money |
| Social Science Research Center | V .1; M .1; N .1 | 8 | 3 | .4 | 2.8 | V .0933; M .0933; N .0933 | 2.8 | -.2 | Money + power |
| Solar Array | — | 3 | 0 | .15 | 1.05 | — | 0 | 0 | 2x plant rule; resource upkeep unchanged |
| Solar Mirror Array | M 1; N .1 | 5 | 11 | .4 | 2.8 | M .5; N .05 | 5.5 | -5.5 | Construction |
| Space Hotel | B 3; W 3; V 2 | 0 | 80 | 8 | 56 | B 2.1; W 2.1; V 1.4 | 56 | -24 | Money |
| Space Science Research Center | V .1; M .1; N .1 | 6 | 3 | .5 | 3.5 | V .1; M .1; N .1 | 3 | 0 | Power + money |
| Xenoscience Research Center | V .1; M .1; N .1 | 6 | 3 | .6 | 4.2 | V .1; M .1; N .1 | 3 | 0 | Power + money |

T2 non-crew material and boost-equivalent maintenance falls from
**1,520.35 t/month to 890.425 t/month**, a delta of **-629.925 t/month**.

## T3 table

| Module | Vanilla resource mix (points/mo) | Money/mo | Vanilla t/mo | Generous real t/mo | 7x target t/mo | Proposed resource mix (points/mo) | Proposed t/mo | Delta t/mo | Preferred non-resource balance lever |
|---|---|---:|---:|---:|---:|---|---:|---:|---|
| Administration Complex | B 12; V .5; M .5; N .5 | 90 | 135 | 12 | 84 | B 7.4667; V .3111; M .3111; N .3111 | 84 | -51 | Money |
| Agriculture Complex | — | 0 | 0 | 20 | 140 | — | 0 | 0 | Resource upkeep unchanged |
| Antimatter Farm | M 1.5; N 1 | 10 | 25 | 3.2 | 22.4 | M 1.344; N .896 | 22.4 | -2.6 | Power + construction |
| Argus Complex | V 3; M 3; N .3 | 30 | 63 | 3 | 21 | V 1.5; M 1.5; N .15 | 31.5 | -31.5 | Power + money |
| Battlestations | V 3; M 3; N 1 | 30 | 70 | 6 | 42 | V 1.8; M 1.8; N .6 | 42 | -28 | Power + construction + money |
| Civilian Complex | B 1; W 10; V 6; M 2 | 0 | 190 | 10 | 70 | B .5; W 5; V 3; M 1 | 95 | -95 | Money |
| Climate Institute | V .5; M .5; N .5 | 20 | 15 | 4 | 28 | V .5; M .5; N .5 | 15 | 0 | Power + money |
| Colony Core | — | 20 | 0 | 2.4 | 16.8 | — | 0 | 0 | Construction + money |
| Colony Mining Complex | W 15; V 6 | 60 | 210 | 60 | 420 | W 15; V 6 | 210 | 0 | Power + money |
| Command Center | V 10; M 10; N 5 | 100 | 250 | 40 | 280 | V 10; M 10; N 5 | 250 | 0 | Money + power |
| Energy Institute | V 10; M .5; N .5; F .01 | 18 | 110.1 | 10 | 70 | V 6.3579; M .3179; N .3179; F .0064 | 70 | -40.1 | Power + money |
| Fission Reactor Farm | W 6; V 3; F .5 | 18 | 95 | 4 | 28 | W 6; V 3; F .5 | 95 | 0 | 2x plant rule; construction |
| Foundry | V 10; M 10; N 1 | 30 | 210 | 12 | 84 | V 5; M 5; N .5 | 105 | -105 | Power + money |
| Fusion Reactor Farm | W 10; V 6; F .1 | 30 | 161 | 6 | 42 | W 10; V 6; F .1 | 161 | 0 | 2x plant rule; construction |
| Geriatrics Facility | B 3; W 15; V 10 | 0 | 280 | 30 | 210 | B 2.25; W 11.25; V 7.5 | 210 | -70 | Money + power |
| Heavy Fission Reactor Farm | W 6; V 3; F .75 | 24 | 97.5 | 7.2 | 50.4 | W 6; V 3; F .75 | 97.5 | 0 | 2x plant rule; construction |
| Heavy Fusion Reactor Farm | W 10; V 6; M 1; N 1; F .2 | 36 | 182 | 10 | 70 | W 10; V 6; M 1; N 1; F .2 | 182 | 0 | 2x plant rule; construction |
| Helium-3 Mine | W 2; V 2; M 10; N 1.5 | 30 | 155 | 8 | 56 | W 1; V 1; M 5; N .75 | 77.5 | -77.5 | Power + construction |
| Information Science Institute | W 3; V .5; M .5; N .5 | 18 | 45 | 3.2 | 22.4 | W 1.5; V .25; M .25; N .25 | 22.5 | -22.5 | Power + money |
| Interstellar Launching Laser | — | 20 | 0 | 300 | 2100 | — | 0 | 0 | Power + construction |
| Life Science Institute | W 3; V 3 | 18 | 60 | 10 | 70 | W 3; V 3 | 60 | 0 | Power + money |
| Marine Battalion Barracks | V 3; M 3; N .3 | 30 | 63 | 18 | 126 | V 3; M 3; N .3 | 63 | 0 | Money + construction |
| Materials Institute | W 3; V 3; M 3; N 1.5 | 18 | 105 | 6 | 42 | W 1.5; V 1.5; M 1.5; N .75 | 52.5 | -52.5 | Power + money |
| Media Center | V .5; M .5; N .5 | 100 | 15 | 3.2 | 22.4 | V .5; M .5; N .5 | 15 | 0 | Money + power |
| Military Science Institute | V .5; M .5; N .5 | 18 | 15 | 4 | 28 | V .5; M .5; N .5 | 15 | 0 | Power + money |
| Nanofacturing Complex | W 10; V 10; M 30; N 3 | 20 | 530 | 10 | 70 | W 5; V 5; M 15; N 1.5 | 265 | -265 | Power + construction + money |
| Research University | W 10; V 10 | 100 | 200 | 24 | 168 | W 8.4; V 8.4 | 168 | -32 | Money + power |
| Ring Core | — | 20 | 0 | 2.4 | 16.8 | — | 0 | 0 | Construction + money |
| Sentinel Complex | — | 20 | 0 | 10 | 70 | — | 0 | 0 | Power + construction + money |
| Social Science Institute | V .5; M .5; N .5 | 24 | 15 | 3 | 21 | V .5; M .5; N .5 | 15 | 0 | Money + power |
| Solar Farm | — | 5 | 0 | 1.1 | 7.7 | — | 0 | 0 | 2x plant rule; resource upkeep unchanged |
| Soletta | M 3; N .5 | 10 | 35 | 3.2 | 22.4 | M 1.92; N .32 | 22.4 | -12.6 | Construction |
| Space Resort | B 6; W 10; V 5 | 0 | 210 | 45 | 315 | B 6; W 10; V 5 | 210 | 0 | Money |
| Space Science Institute | V .5; M .5; N .5 | 18 | 15 | 4 | 28 | V .5; M .5; N .5 | 15 | 0 | Power + money |
| Spaceworks | M 10; N 1 | 0 | 110 | 20 | 140 | M 10; N 1 | 110 | 0 | Power + construction + money |
| Supercollider | W 30; V 30; M 20; N 20; F 10 | 120 | 1100 | 40 | 280 | W 15; V 15; M 10; N 10; F 5 | 550 | -550 | Power + construction + money |
| Xenoscience Institute | V .5; M .5; N .5 | 18 | 15 | 4.8 | 33.6 | V .5; M .5; N .5 | 15 | 0 | Power + money |

T3 non-crew material and boost-equivalent maintenance falls from
**4,781.6 t/month to 3,346.3 t/month**, a delta of **-1,435.3 t/month**.

## Estimate basis and interpretation

The estimates extend T1 module families primarily by installed mass and
functional throughput:

- research centers and institutes scale T1 experiment supplies and fixed
  spares with their much larger laboratories;
- modules with zero vanilla resource upkeep retain zero under the general
  preservation rule, including farms, cores, direct solar generators, the
  Sentinel Complex, and the Interstellar Launching Laser;
- reactor fuel and spares scale with output and plant size before the separate
  2x representation multiplier;
- nanofactories, foundries, shipyards, and spaceworks include generous tooling
  and process losses, while material incorporated into actual construction
  remains an activity cost;
- settlement and colony mines receive the largest conventional allowances
  because wear and process-fluid loss should scale with extraction throughput;
- hospitals, residential modules, hotels, resorts, and barracks include
  facility and turnover supplies, while food, oxygen, and water remain in crew
  upkeep; and
- the Interstellar Launching Laser comparison estimate of 300 real t/month is
  0.96% of its 375,000 t mass per year. Its 2,100 t/month scaled target remains
  analytical only because the module has no vanilla resource upkeep.

The revised restrictions produce no maintenance increases in T2 or T3. When a
scaled target exceeds vanilla but the unscaled realistic estimate does not,
the module remains at vanilla and its preferred balance lever can carry any
additional gameplay burden. This affects the colony mine, command center,
large population modules, and several institutes. The launching laser and
other zero-resource modules likewise remain balanced through power,
construction cost, money, and their existing mechanics rather than a newly
invented material sink.
