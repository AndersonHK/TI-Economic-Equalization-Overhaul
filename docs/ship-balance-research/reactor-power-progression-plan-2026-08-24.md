# Reactor power progression and hull-scaling plan — 2026-08-24 revision

Status: planning analysis only. No gameplay values are changed by this report.

Originally drafted: 2026-08-20

Comprehensively revised: 2026-08-24

Game-data baseline: Terra Invicta 1.0.51 with the live EEO power-plant,
appearance-driven drive-scaling, and reactor-bay overrides.

## Revised conclusion

The progression should separate three jobs that the current values blur
together:

1. **Compact solid-core reactors** are light, low-output plants for Gunships,
   Escorts, and other low-scaling craft. Their proposed output ladder is
   **2 / 4 / 8 / 16 / 32 GW**.
2. **Ordinary fission and pre-terawatt fusion** rise through gigawatt and
   hundreds-of-gigawatts service but remain below `1,000 GW` when their names
   and technology gates do not claim terawatt output.
3. **Terawatt plants** are capital-ship machinery. Their final tiers are sized
   against actual appearance-scaled drives and the highest-scaling Titan, not
   the unscaled x6 template values.

The revised principal endpoints are:

| Line | Revised endpoint |
|---|---:|
| Regular Solid Core V | **648 GW** |
| Compact Solid Core V | **32 GW** |
| Gas Core III, last non-terawatt gas core | **950 GW** |
| Terawatt Gas Core I-III | **5.2 / 16 / 32 TW** |
| Tokamak V | **40 TW** |
| Hybrid IV | **82 TW** |
| Flow-Stabilized Z-Pinch | **60 TW** |
| Inertial V-VII | **140 TW / 150 TW / 2.2 PW** |

These are theoretical thermal-output caps. Reactor-bay geometry remains
authoritative. For example, the proposed `32 TW`, `2.30 t/GW` Terawatt Gas
Core III is bay-limited to about `30.61 TW` of closed-cycle reactor output on
Titan appearance 3. A hull-scaled Lodestar Fission Lantern x6 requests
`28.23 TW` of electrical drive power and `30.04 TW` from the 94%-efficient
plant. It fits at about `98%` of the measured bay.

The final post-terawatt fusion plants similarly support powerful scaled drives
at approximately `84-97%` bay occupancy. This replaces the old plan's
half-Titan target with a more appropriate **behemoth target of roughly
80-100%** for a top drive on the highest-scaling Titan.

## Runtime interpretation, including open-cycle mass scaling

Runtime inspection gives these relationships:

Let:

- `D` be the appearance-scaled installed drive demand;
- `A` be useful systems-and-weapons electrical demand;
- `eta` be power-plant efficiency;
- `r = 0.01` be the retained open-cycle heat fraction; and
- `s` be the plant class's open-cycle thermal mass multiplier.

The active model is:

```text
open-cycle coupling = 1 - r * (1 - eta)

open-cycle drive:
  Qoc    = D / open-cycle coupling
  Qe     = A / eta
  Qtotal = Qoc + Qe
  Pmass  = s * Qoc + Qe

closed-cycle drive:
  Qtotal = Pmass = (D + A) / eta

compatibility compares Qtotal with maxOutput_GW
plant mass and cost use Pmass * specificPower_tGW
bay volume uses Pmass * specificPower_tGW * bay fraction / density
```

The class defaults relevant to this report are:

| Plant class | Open-cycle mass multiplier `s` |
|---|---:|
| Solid core | 0.025 |
| Molten salt / liquid core | 0.05 |
| Gas core | 0.10 |
| Mirror cell fusion | 0.15 |
| Hybrid fusion | 0.20 |
| Z-pinch fusion | 0.25 |
| Electrostatic / tokamak / inertial fusion | 0.30 |

This changes the August 20 interpretation in two ways. A closed-cycle drive's
reactor demand is higher than its displayed drive demand by `1 / eta`; an
open-cycle drive still advertises almost its full thermal demand for the cap,
but only `s` times that demand determines plant mass, cost, and bay occupancy.
All fitting tables in this revision use those separate values.

Three consequences remain central:

1. Increasing `maxOutput_GW` makes a larger drive legal but does not force a
   lightly loaded plant to become larger.
2. Increasing `specificPower_tGW` makes closed-cycle installations heavier and
   larger, while reducing the closed-cycle output that fits in a measured bay.
3. A high-output open-cycle drive can remain physically small because its
   effective specific mass is `s * specificPower_tGW`. Output caps alone cannot
   make every direct-thermal installation a bay-filling behemoth.

The behemoth target in this plan therefore applies to the selected
**closed-cycle capital-drive endpoints**. Compact direct-thermal drives are the
opposite case: the open-cycle multiplier is what lets them remain attractive on
small craft. If those reactors still look implausibly small, a fixed reactor
mass or volume floor is the appropriate follow-up; raising their caps is not.

## Hull-scaling reference cases

The previous draft used several unscaled x6 drive demands as descriptive
anchors. Those values explain the vanilla plant caps but are not valid EEO ship
design targets. The revised plan uses the actual runtime multipliers.

### Small compact-reactor cases

Gunship and Escort appearances 0 and 2 share the relevant measurements:

| Case | De Laval multiplier | Magnetic multiplier | Pulsed multiplier | Reactor bay |
|---|---:|---:|---:|---:|
| Gunship/Escort appearance 0 | 1.000000x | 1.000000x | 1.000000x | 264.241 m3 |
| Gunship/Escort appearance 2 | 0.668230x | 0.397033x | 1.000000x | 317.310 m3 |

Kiwi is a De Laval drive. Kiwi x6 therefore requests `1.240 GW` on appearance
0 and only `0.829 GW` on appearance 2. Every proposed compact plant can power
it, beginning with Compact Solid Core I at `2 GW`. The highest-drive checks use
the runtime nozzle getter: pulsed drives remain at `1x`; otherwise a nuclear
drive resolves to magnetic only when its single-thruster jet power is at least
`1 GW` and exhaust velocity is at least `87.5 km/s`. All other cases use the
De Laval multiplier.

### Highest-scaling Titan case

Titan appearance 3 is the maximum drive-scaling case for both nozzle families:

| Input | Value |
|---|---:|
| De Laval drive multiplier | **25.203374x** |
| Magnetic drive multiplier | **20.944447x** |
| Pulsed drive multiplier | **1.000000x** |
| Measured reactor bay | **15,840.889 m3** |

Every Titan table below applies the drive's actual nozzle family, its runtime
multiplier, plant efficiency, open/closed-cycle status, class mass multiplier,
plant cap, and measured bay capacity.

## Why the current plants look small

The current maximum-output fields were fitted closely to unscaled x6 drive
templates. EEO subsequently made drive power scale with graphical hull size,
but the plant caps remained unscaled. On Titan appearance 3, many current plants
cannot power even the x1 member of their intended drive family.

Specific-mass collapses create the second problem. Several advanced reactors
are tiny even when their mass-rated output equals their full theoretical cap.
This is the closed-cycle/full-electrical baseline; an open-cycle drive makes
them smaller again by its class multiplier:

| Plant | Current cap | Current t/GW | Full-rating mass | Full-rating volume | Default Titan fill |
|---|---:|---:|---:|---:|---:|
| Solid Core I | 2 GW | 240 | 480 t | 96 m3 | 0.60% |
| Gas Core III | 150 GW | 10 | 1,500 t | 338 m3 | 2.12% |
| Terawatt Gas Core III | 1,700 GW | 5 | 8,500 t | 1,913 m3 | 11.99% |
| Tokamak V | 5,060 GW | 0.1 | 506 t | 190 m3 | 1.19% |
| Hybrid IV | 11,370 GW | 0.05 | 569 t | 213 m3 | 1.34% |
| Flow-Stabilized Z-Pinch | 7,590 GW | 0.0068 | 52 t | 12 m3 | 0.08% |
| Inertial V | 19,090 GW | 0.25 | 4,773 t | 1,909 m3 | 11.96% |
| Inertial VII | 306,430 GW | 0.002 | 613 t | 245 m3 | 1.54% |

The largest adjacent specific-mass jumps are:

- Electrostatic II to III: `0.5` to `0.005 t/GW`, a **100x** improvement;
- Z-Pinch IV to Flow-Stabilized: `0.4` to `0.0068 t/GW`, a **58.8x**
  improvement;
- Inertial VI to VII: `0.068` to `0.002 t/GW`, a **34x** improvement.

Those jumps overwhelm the increased output. A more advanced full-rating plant
often becomes physically smaller than its predecessor.

## Terawatt naming and technology boundary

Installed English localization names Gas Core IV-VI **Terawatt Gas Core
Fission Reactor I-III**. Gas Core III should therefore remain below `1 TW`,
while Gas Core IV is the first fission plant allowed to cross that boundary.

The global **Terawatt Fusion Reactors** technology is not currently a clean
threshold. These plants exceed `1 TW` without requiring it:

| Pre-terawatt plant | Current cap | Matched unscaled x6 drive demand |
|---|---:|---:|
| Tokamak IV | 1,260 GW | Helion Torus Lantern: 1,246 GW |
| Hybrid III | 1,900 GW | Helion Plasmajet Lantern: 1,895 GW |
| Z-Pinch III | 2,510 GW | Zeta Helion Lantern: 2,503 GW |
| Z-Pinch IV | 3,970 GW | Zeta Borane Lantern: 3,959 GW |
| Inertial III | 3,170 GW | Helion Nova Lantern: 3,162 GW |
| Inertial IV | 5,500 GW | Borane Nova Lantern: 5,485 GW |

Tokamak V also reaches `5,060 GW` without a Terawatt Fusion Reactors
prerequisite. Its direct global gate is Proton-Proton Fusion. The revised plan
keeps it multi-terawatt and therefore adds Terawatt Fusion Reactors to its
project prerequisites.

Pre-terawatt plant caps are rounded to at most `950 GW`. It is intentional that
some early fusion architectures can no longer propel the highest-scaling Titan:
that hull should require terawatt technology for power-hungry fusion drives.

## Revised fission progression

### Output and specific mass

| Fission line | Current caps | Revised caps | Specific-mass treatment |
|---|---:|---:|---|
| Regular Solid Core I-V | 2 / 3 / 10 / 30 / 60 GW | **8 / 24 / 72 / 216 / 648 GW** | retain 240 / 204 / 168 / 72 / 48 t/GW |
| Compact Solid Core I-V | 0.75 / 2 / 4 / 6 / 10 GW | **2 / 4 / 8 / 16 / 32 GW** | retain 36 / 30 / 24 / 18 / 12 t/GW |
| Molten Core I-III | 4 / 17 / 200 GW | **32 / 160 / 900 GW** | retain 16 / 14 / 12 t/GW |
| Molten Salt I-II | 40 / 400 GW | **200 / 950 GW** | retain 15 / 12 t/GW |
| Vapor Core I-III | 6.5 / 20 / 60 GW | **100 / 300 / 900 GW** | retain 9 / 8 / 7 t/GW |
| Gas Core I-III | 8 / 33 / 150 GW | **150 / 450 / 950 GW** | retain 20 / 16 / 10 t/GW |
| Terawatt Gas Core I-III | 1 / 1.3 / 1.7 TW | **5.2 / 16 / 32 TW** | revise 7 / 6 / 5 to **6 / 4 / 2.30 t/GW** |

The regular Solid Core line now grows by approximately threefold steps from the
`8 GW` thermal anchor. The compact line grows by twofold steps and remains far
below the regular line, trading capacity for much lower mass per gigawatt.

Gas Core III is set to `950 GW`, the largest display-safe non-terawatt value.
The Terawatt Gas Core line is instead calibrated around scaled Titan Lodestar
clusters: x1, x3, and x6. The `5.2 TW` first step is intentionally below the
`5.45 TW` needed by Flare x3, so Lodestar x1 is the mechanically
highest-reactor-demand fit rather than being hidden by a slightly larger but
light open-cycle cluster.

### Compact solid-core line on small ships

Kiwi is open cycle, so it asks for almost its displayed thermal drive demand at
the cap while only 2.5% of that output is mass-rated. `D -> Q` below means
installed appearance-scaled drive demand followed by total reactor output.
Calculated mass precedes the ordinary `1 t` build-mass floor; bay accounting
continues to use the calculated mass-rated output.

| Compact plant | Revised cap | Kiwi x6 on A0: `D -> Q`; mass; bay | Kiwi x6 on A2: `D -> Q`; mass; bay |
|---|---:|---|---|
| Compact I | 2 GW | 1.240 -> 1.245 GW; 1.120 t; 0.085% | 0.829 -> 0.832 GW; 0.749 t; 0.047% |
| Compact II | 4 GW | 1.240 -> 1.245 GW; 0.934 t; 0.071% | 0.829 -> 0.832 GW; 0.624 t; 0.039% |
| Compact III | 8 GW | 1.240 -> 1.244 GW; 0.747 t; 0.057% | 0.829 -> 0.832 GW; 0.499 t; 0.031% |
| Compact IV | 16 GW | 1.240 -> 1.244 GW; 0.560 t; 0.042% | 0.829 -> 0.831 GW; 0.374 t; 0.024% |
| Compact V | 32 GW | 1.240 -> 1.244 GW; 0.373 t; 0.028% | 0.829 -> 0.831 GW; 0.249 t; 0.016% |

Every tier therefore powers Kiwi x6 on both small appearances. Later compact
plants do not become larger at the same load: their improving specific mass
makes the same installation lighter. That is the intended compact-line role,
although the sub-tonne calculations are also direct evidence for a future
fixed-mass floor if visual or criticality plausibility demands one.

For the upper mechanical boundary, the next table ranks all matching human
solid-core drives by actual reactor output `Q`. `O` and `C` mark open and closed
cycle. These are compatibility results, not claims that every drive is
available at the compact plant's unlock date.

| Compact plant | Highest fitting on A0 (`D -> Q`; cycle; bay) | Highest fitting on A2 (`D -> Q`; cycle; bay) |
|---|---|---|
| Compact I | Advanced Nerva x1 (1.929 -> 1.937 GW; O; 0.13%) | Rover x2 (1.913 -> 1.921 GW; O; 0.11%) |
| Compact II | Advanced Nerva x2 (3.858 -> 3.873 GW; O; 0.22%) | Advanced Nerva x3 (3.868 -> 3.882 GW; O; 0.18%) |
| Compact III | Fission Frag x5 (7.935 -> 7.963 GW; O; 0.36%) | Advanced Nerva x6 (7.735 -> 7.762 GW; O; 0.29%) |
| Compact IV | Pulsar x6 (10.165 -> 15.059 GW; C; 20.52%) | Heavy Dumbo x1 (13.054 -> 13.096 GW; O; 0.37%) |
| Compact V | Advanced Pulsar x5 (21.333 -> 30.476 GW; C; 27.68%) | Heavy Dumbo x2 (26.108 -> 26.186 GW; O; 0.50%) |

This split is useful rather than contradictory. Direct-thermal Kiwi, Nerva,
Rover, Frag, and Dumbo packages remain extremely light. Closed-cycle Pulsar
packages consume the plant's full specific mass and occupy a meaningful fifth
to quarter of the small bay. The compact line can therefore serve both the
light nuclear-thermal niche and modest low-scale electric craft without
becoming a Titan power source.

## Complete current and revised human fusion progression

Efficiencies are shown to make the current plants explicit. This plan changes
capacity and specific mass only; it retains the listed efficiencies pending a
separate thermodynamic pass.

| Fusion plant | Current cap | Revised cap | Current -> revised t/GW | Current efficiency | Gate interpretation |
|---|---:|---:|---:|---:|---|
| Electrostatic I | 46 GW | **200 GW** | 1 -> **6** | 95% | D-T, pre-terawatt |
| Electrostatic II | 74 GW | **600 GW** | 0.5 -> **4** | 95% | D-D, pre-terawatt |
| Electrostatic III | 310 GW | **950 GW** | 0.005 -> **2.5** | 95% | P-P/exotics, deliberately sub-terawatt |
| Mirror Cell I | 120 GW | **400 GW** | 5 -> **6** | 93% | D-T, pre-terawatt |
| Mirror Cell II | 215 GW | **800 GW** | 4 -> **4** | 95% | D-D, pre-terawatt |
| Mirror Cell III | 256 GW | **950 GW** | 0.8 -> **3** | 97% | D-He3, pre-terawatt |
| Tokamak I | 128 GW | **450 GW** | 4 -> **6** | 92% | D-T, pre-terawatt |
| Tokamak II | 401 GW | **900 GW** | 2 -> **5** | 95% | D-D, pre-terawatt |
| Tokamak III | 624 GW | **950 GW** | 1 -> **4** | 96% | D-He3, pre-terawatt |
| Tokamak IV | 1.26 TW | **950 GW** | 0.5 -> **3** | 98.5% | project-chain plant, restored below boundary |
| Tokamak V | 5.06 TW | **40 TW** | 0.1 -> **1** | 99% | add Terawatt Fusion Reactors prerequisite |
| Hybrid I | 180 GW | **600 GW** | 2 -> **6** | 97% | D-T, pre-terawatt |
| Hybrid II | 510 GW | **900 GW** | 1 -> **5** | 98% | D-D, pre-terawatt |
| Hybrid III | 1.90 TW | **950 GW** | 0.5 -> **3** | 99% | D-He3, restored below boundary |
| Hybrid IV | 11.37 TW | **82 TW** | 0.05 -> **0.5** | 99% | Terawatt Fusion Reactors |
| Z-Pinch I | 260 GW | **900 GW** | 3 -> **6** | 95% | D-T, pre-terawatt |
| Z-Pinch II | 610 GW | **950 GW** | 2 -> **5** | 95% | D-D, pre-terawatt |
| Z-Pinch III | 2.51 TW | **950 GW** | 1.4 -> **4** | 96% | D-He3, restored below boundary |
| Z-Pinch IV | 3.97 TW | **950 GW** | 0.4 -> **3** | 98% | aneutronic, restored below boundary |
| Flow-Stabilized Z-Pinch | 7.59 TW | **60 TW** | 0.0068 -> **1** | 99.5% | Terawatt Fusion Reactors |
| Inertial I | 370 GW | **950 GW** | 4 -> **6** | 85% | D-T, pre-terawatt |
| Inertial II | 860 GW | **950 GW** | 2 -> **5** | 89% | D-D, pre-terawatt |
| Inertial III | 3.17 TW | **950 GW** | 1 -> **4** | 92% | D-He3, restored below boundary |
| Inertial IV | 5.50 TW | **950 GW** | 0.5 -> **3** | 95% | aneutronic, restored below boundary |
| Inertial V | 19.09 TW | **140 TW** | 0.25 -> **0.25** | 97.5% | Terawatt Fusion Reactors |
| Inertial VI | 20.42 TW | **150 TW** | 0.068 -> **0.25** | 99% | downstream of terawatt line |
| Inertial VII | 306.43 TW | **2.2 PW** | 0.002 -> **0.018** | 99.9% | Accelerando endpoint |

The pre-terawatt specific-mass values deliberately improve slowly from roughly
`6` toward `2.5-3 t/GW`. Electrostatic III and Flow-Stabilized Z-Pinch lose
their implausible 100x and 58.8x single-step improvements.

The post-terawatt values are selected jointly with scaled drive demand. Some,
such as final Terawatt Gas Core, become lighter per gigawatt than today; the
much larger installed demand still makes the resulting reactor a bay-filling
machine rather than a tiny module.

## Highest fitting drives on the highest-scaling Titan

This table uses Titan appearance 3. Drives are selected from the matching human
power-plant class and ranked by actual reactor output `Q`, subject to both the
plant cap and bay. Generic `Any_General` and alien drives are excluded. The
cells report appearance-scaled drive demand `D -> Q`; `O` and `C` mark
open- and closed-cycle operation. This distinction prevents an open-cycle
drive's small mass footprint from being mistaken for unscaled power demand.

| Plant | Current cap and highest fit (`D -> Q`; cycle; bay) | Revised cap and highest fit (`D -> Q`; cycle; bay) |
|---|---|---|
| Regular Solid Core V | 60 GW: Snare x5 (58.018 -> 58.207 GW; O; 0.09%) | 648 GW: Advanced Pulsar x4 (430.146 -> 637.253 GW; C; 38.62%) |
| Compact Solid Core V | 10 GW: Nerva x1 (7.133 -> 7.154 GW; O; 0.003%) | 32 GW: Kiwi x6 (31.252 -> 31.346 GW; O; 0.012%) |
| Molten Salt II | 400 GW: Dumbo x6 (346.244 -> 347.112 GW; O; 0.21%) | 950 GW: Advanced Pulsar x6 (645.206 -> 860.275 GW; C; 10.24%) |
| Molten Core III | 200 GW: Lars x5 (142.525 -> 196.586 GW; C; 3.28%) | 900 GW: Fission Spinner x6 (850.211 -> 852.555 GW; O; 0.71%) |
| Vapor Core III | 60 GW: Cavity x3 (51.163 -> 57.486 GW; C; 0.57%) | 900 GW: Vortex x5 (890.082 -> 891.063 GW; O; 0.89%) |
| Gas Core III | 150 GW: Quartz x4 (127.731 -> 140.363 GW; C; 1.99%) | 950 GW: Dusty Plasma x2 (939.777 -> 940.624 GW; O; 1.34%) |
| Terawatt Gas Core I | 1.0 TW: Advanced Cavity x6 (906.465 -> 985.288 GW; C; 9.80%) | 5.2 TW: Lodestar x1 (4.706 -> 5.115 TW; C; **43.59%**) |
| Terawatt Gas Core II | 1.3 TW: Advanced Vortex x5 (1.111 -> 1.111 TW; O; 0.95%) | 16 TW: Lodestar x3 (14.117 -> 15.179 TW; C; **86.24%**) |
| Terawatt Gas Core III | 1.7 TW: Pharos x4 (1.346 -> 1.432 TW; C; 10.17%) | 32 TW: Lodestar x6 (28.233 -> 30.035 TW; C; **98.12%**) |
| Electrostatic III | 310 GW: Deuteron Fusor x1 (256.465 -> 269.963 GW; C; 0.01%) | 950 GW: Triton Fusor x5 (789.019 -> 830.547 GW; C; 9.83%) |
| Mirror Cell III | 256 GW: none | 950 GW: Helion Reflex x1 (890.327 -> 917.863 GW; C; 10.86%) |
| Tokamak IV, pre-terawatt | 1.26 TW: Triton Torus x2 (887.940 -> 901.462 GW; C; 1.07%) | 950 GW: Triton Torus x2 (887.940 -> 901.462 GW; C; 6.40%) |
| Tokamak V | 5.06 TW: Helion Torus Lantern x1 (4.350 -> 4.393 TW; C; 1.04%) | 40 TW: Protium Torus Lantern x2 (35.261 -> 35.617 TW; C; **84.32%**) |
| Hybrid III, pre-terawatt | 1.90 TW: Triton Polywell x3 (1.775 -> 1.793 TW; C; 2.12%) | 950 GW: Triton Polywell x1 (591.660 -> 597.636 GW; C; 4.24%) |
| Hybrid IV | 11.37 TW: Deuteron Polywell x6 (10.472 -> 10.578 TW; C; 1.25%) | 82 TW: Borane Plasmajet Torch x2 (79.337 -> 80.138 TW; C; **94.86%**) |
| Z-Pinch IV, pre-terawatt | 3.97 TW: Zeta Triton x4 (3.547 -> 3.619 TW; C; 2.19%) | 950 GW: Zeta Triton x1 (886.704 -> 904.800 GW; C; 4.11%) |
| Flow-Stabilized Z-Pinch | 7.59 TW: Zeta Deuteron x3 (6.285 -> 6.316 TW; C; 0.07%) | 60 TW: Zeta Borane Lantern x4 (55.277 -> 55.555 TW; C; **84.17%**) |
| Inertial IV, pre-terawatt | 5.50 TW: Triton Nova x4 (5.022 -> 5.286 TW; C; 6.67%) | 950 GW: **none** |
| Inertial V | 19.09 TW: Deuteron Nova Lantern x6 (17.759 -> 18.214 TW; C; 11.50%) | 140 TW: Helion Nova Torch x2 (133.220 -> 136.636 TW; C; **86.26%**) |
| Inertial VI | 20.42 TW: Borane Nova Lantern x1 (19.147 -> 19.341 TW; C; 3.32%) | 150 TW: Protium Nova Torch x2 (142.509 -> 143.948 TW; C; **90.87%**) |
| Inertial VII | 306.43 TW: Protium Nova Torch x4 (285.017 -> 285.303 TW; C; 1.44%) | 2.2 PW: Protium Converter Torch x2 (2.139 -> 2.141 PW; C; **97.33%**) |

The `none` result for revised Inertial IV is intentional and informative. Even
the Triton Nova x1 requests about `1.26 TW` after the Titan appearance-3
magnetic multiplier. A sub-terawatt inertial plant cannot power that drive on
this hull; Inertial V and Terawatt Fusion Reactors become the entry point.

The compact line tells the opposite story. Compact V's highest-reactor-output
matching fit on this Titan is only Kiwi x6, and the open-cycle installation
occupies `0.012%` of the bay. That is acceptable because a compact reactor is a
small-craft option, not a capital-ship endpoint; it also shows why a fixed mass
floor, rather than a higher cap, is the lever to use if this remains visually
too small.

## Closed-cycle size of the revised behemoth plants

The full-rating column is a closed-cycle/electrical mass baseline, so
`Pmass = Qtotal`. It is applicable to every selected Titan endpoint in the
highest-fit table above. An open-cycle drive on the same reactor would retain
the full `Qtotal` compatibility check but multiply its propulsion contribution
to `Pmass` by the class value `s`.

| Plant | Theoretical cap | Revised t/GW | Full-cap closed-cycle volume | Titan A3 fill | Geometry result |
|---|---:|---:|---:|---:|---|
| Terawatt Gas Core I | 5.2 TW | 6.0 | 7,020 m3 | 44.3% | plant cap binds |
| Terawatt Gas Core II | 16 TW | 4.0 | 14,400 m3 | 90.9% | plant cap binds |
| Terawatt Gas Core III | 32 TW | 2.30 | 16,560 m3 | 104.5% | bay caps closed-cycle output near 30.61 TW |
| Tokamak V | 40 TW | 1.0 | 15,000 m3 | 94.7% | plant cap binds |
| Hybrid IV | 82 TW | 0.5 | 15,375 m3 | 97.1% | plant cap binds |
| Flow-Stabilized Z-Pinch | 60 TW | 1.0 | 14,400 m3 | 90.9% | plant cap binds |
| Inertial V | 140 TW | 0.25 | 14,000 m3 | 88.4% | plant cap binds |
| Inertial VI | 150 TW | 0.25 | 15,000 m3 | 94.7% | plant cap binds |
| Inertial VII | 2.2 PW | 0.018 | 15,840 m3 | 100.0% | cap and bay nearly coincide |

This is a stronger capital-ship statement than the old half-bay proposal. It
also retains real appearance sensitivity: changing to a Titan appearance with
a smaller measured bay can reduce the fitting drive cluster through the normal
designer reconciliation path.

## Implementation boundary

A future gameplay pass should change these elements together:

1. apply the revised fission caps and the three Terawatt Gas Core specific
   masses;
2. apply the complete human fusion cap and specific-mass table;
3. add Terawatt Fusion Reactors to Tokamak V;
4. audit the project prerequisites of every drive that crosses `1 TW` so the
   plant and drive thresholds remain synchronized;
5. regenerate the merged power-plant snapshot;
6. validate every drive variation against every compatible reactor, hull,
   appearance, nozzle multiplier, open/closed-cycle state, plant efficiency,
   thermal mass multiplier, and measured bay;
7. explicitly test the compact line on Gunship/Escort appearances 0 and 2 and
   the behemoth lines on Titan appearance 3;
8. separately decide whether compact reactors need an open-cycle fixed-mass or
   fixed-volume floor; do not inflate their output ladder to solve that visual
   issue;
9. run the normal `tools\deploy.ps1` flow and begin manual Ship Designer testing
   immediately after a successful deployment.

The remaining balance question is whether top terawatt plants should support
the mechanically highest-demand fitting drive, as modeled here, or privilege
the nominally latest drive even when an earlier x4/x6 combination consumes
slightly more power. Compatibility should remain purely power-based; project
ordering and AI scoring are better places to express a preference for the
latest drive.
