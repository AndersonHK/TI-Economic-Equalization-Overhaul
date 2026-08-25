# Conservative linear reactor-scaling plan

Status: planning analysis only. No template, runtime, or gameplay value is changed by this report.

Date: 2026-08-24

Baseline: Terra Invicta 1.0.51, the live EEO merged power-plant snapshot, and the active open-cycle thermal-mass model.

## Scope and two invariants

This deliberately replaces the broad redesign in the other August plans with only two levers:

1. Raise electrical specific mass for every shipped non-alien reactor with one linear rule keyed only to its existing open-cycle thermal-mass family. Preserve both the vanilla within-line progression and the family ordering `fusion > gas > molten > solid`.
2. Raise maximum output with the same proportional baseline in each of four balance buckets: ordinary fission, terawatt fission, fusion/advanced human, and alien. A later tier must produce a strictly larger full-rated reactor than the preceding tier.

Nothing else is proposed. Efficiencies, open-cycle multipliers, heat accounting, reactor-bay geometry, drive scaling, technology prerequisites, localization, and drive values remain unchanged. Fuel cells are excluded because they are not reactors.

Antimatter reactors are non-alien and therefore receive the specific-mass rule. For the output pass they sit in the `Fusion / advanced human` balance bucket so the requested four-bucket structure remains complete. This is a balance classification, not a claim that antimatter is fusion.

## Rule 1: one linear electrical-specific-mass formula

Let `s` be the already-implemented open-cycle thermal mass multiplier and let `SPvanilla` be the unmodified game value in `powerplant.csv`. For every non-alien reactor:

```text
K(s)        = 7.5 + 10s
SPproposed  = SPvanilla * K(s)                 [t/GW of mass-rated reactor output]
t/GWe       = SPproposed / plant efficiency   [closed-cycle electrical specific mass]
```

The same multiplier applies to every tier in a thermal family. That preserves the vanilla ratios and ordering inside each technology line instead of hand-tuning individual outliers. Alien specific masses remain at their current EEO values.

The `7.5` intercept is a conservative calibration to the live table: it is the smallest clean half-step intercept that makes every shipped non-alien reactor strictly heavier than its current value once the `10s` family term is added. Each additional `0.05` of thermal factor adds another `0.5x` vanilla specific mass.

| Thermal family | `s` | Vanilla multiplier `K(s)` | Open-cycle effective multiplier `s*K(s)` | Included paths |
|---|---:|---:|---:|---|
| Solid | 0.025 | 7.75x | 0.19375x | regular and compact solid-core fission |
| Molten | 0.05 | 8.00x | 0.40000x | molten-salt and liquid/molten-core fission |
| Gas | 0.10 | 8.50x | 0.85000x | vapor, gas-core, and terawatt gas-core fission |
| Mirror fusion | 0.15 | 9.00x | 1.35000x | mirror-cell fusion |
| General magnetic / hybrid | 0.20 | 9.50x | 1.90000x | hybrid fusion and antimatter beam core |
| Z-pinch / plasma | 0.25 | 10.00x | 2.50000x | Z-pinch fusion and antimatter plasma core |
| Electrostatic / toroidal / inertial | 0.30 | 10.50x | 3.15000x | electrostatic, tokamak, and inertial fusion |
| Antimatter solid-core default | 0.35 | 11.00x | 3.85000x | reserved for a shipped or future matching reactor |

The uplift order is therefore strictly fusion-family factors above gas, gas above molten, and molten above solid. It preserves vanilla per-line improvements; it does not assert that every fusion plant must literally weigh more per GWe than every early solid-core plant.

## Rule 2: proportional output lift plus one monotonic-mass floor

"Linear output increase" means the same `+10%` proportional baseline is applied independently inside each requested bucket. Friendly upward rounding is then used. There are no bespoke drive-fitting targets.

| Output bucket | Baseline rule | Reactor lines |
|---|---:|---|
| Fission | current cap x 1.10 | regular solid, compact solid, molten salt, molten core, vapor core, gas core I-III |
| Fission Terawatt | current cap x 1.10 | terawatt gas core I-III |
| Fusion / advanced human | current cap x 1.10 | all six human fusion branches plus antimatter |
| Alien | current cap x 1.10 | the three alien hybrid-fusion plants |

For tier `n > 1`, full-rated electrical/mass-rated plant mass is
`M = cap * SPproposed`. This is the invariant implied by the requested
`t/GWe` progression. Direct-thermal installations still multiply their
propulsion contribution by `s`; they are not a separate drive-specific target
in this plan. The proposed cap is the larger of the rounded +10% baseline and
the cap needed to satisfy:

```text
Mproposed[n] >= 1.05 * Mproposed[n-1]
```

The 5% margin makes "larger" robust against display rounding. It is deliberately a one-way floor: an already-large later tier never forces earlier tiers upward. This is more conservative than fitting a linear mass envelope backward through an entire branch.

| Unrounded cap | Upward rounding step |
|---:|---:|
| below 10 GW | 0.1 GW |
| 10-100 GW | 1 GW |
| 100-1,000 GW | 10 GW |
| 1-10 TW | 0.1 TW |
| 10-100 TW | 1 TW |
| 100 TW-1 PW | 10 TW |
| at least 1 PW | 0.1 PW |

## Line-level result

| Reactor line | Current cap ladder | Proposed cap ladder | Tiers where mass floor overrides baseline |
|---|---|---|---|
| Regular Solid Core | 2 GW / 3 GW / 10 GW / 30 GW / 60 GW | **2.2 GW / 3.3 GW / 11 GW / 33 GW / 66 GW** | none |
| Compact Solid Core | 0.75 GW / 2 GW / 4 GW / 6 GW / 10 GW | **0.9 GW / 2.2 GW / 4.4 GW / 6.6 GW / 11 GW** | none |
| Molten Salt | 40 GW / 400 GW | **44 GW / 440 GW** | none |
| Molten Core | 4 GW / 17 GW / 200 GW | **4.4 GW / 19 GW / 220 GW** | none |
| Vapor Core | 6.5 GW / 20 GW / 60 GW | **7.2 GW / 22 GW / 66 GW** | none |
| Gas Core | 8 GW / 33 GW / 150 GW | **8.8 GW / 37 GW / 170 GW** | none |
| Terawatt Gas Core | 1 TW / 1.3 TW / 1.7 TW | **1.1 TW / 3.3 TW / 13 TW** | tier 2, tier 3 |
| Electrostatic Fusion | 46 GW / 74 GW / 310 GW | **51 GW / 110 GW / 12 TW** | tier 2, tier 3 |
| Mirror Cell Fusion | 120 GW / 215 GW / 256 GW | **140 GW / 240 GW / 1.3 TW** | tier 3 |
| Tokamak Fusion | 128 GW / 401 GW / 624 GW / 1.26 TW / 5.06 TW | **150 GW / 450 GW / 950 GW / 2 TW / 11 TW** | tier 3, tier 4, tier 5 |
| Hybrid Fusion | 180 GW / 510 GW / 1.9 TW / 11.37 TW | **200 GW / 570 GW / 2.1 TW / 23 TW** | tier 4 |
| Z-Pinch Fusion | 260 GW / 610 GW / 2.51 TW / 3.97 TW / 7.59 TW | **290 GW / 680 GW / 2.8 TW / 11 TW / 680 TW** | tier 4, tier 5 |
| Inertial Fusion | 370 GW / 860 GW / 3.17 TW / 5.5 TW / 19.09 TW / 20.42 TW / 306.43 TW | **410 GW / 950 GW / 3.5 TW / 7.4 TW / 21 TW / 82 TW / 3 PW** | tier 4, tier 6, tier 7 |
| Antimatter (fusion balance bucket) | 1.2 TW / 11.15 TW / 66 TW / 3 PW | **1.4 TW / 15 TW / 160 TW / 35.4 PW** | tier 2, tier 3, tier 4 |
| Alien Hybrid Fusion | 5 TW / 32 TW / 107.55 TW | **5.5 TW / 36 TW / 270 TW** | tier 3 |

The extreme-looking late caps are not independent ambitions. They are the mechanical consequence of retaining vanilla 10x-100x specific-mass breakthroughs while enforcing that the next full-rated reactor cannot become smaller. The tables expose every such case rather than smoothing it away.

## Complete reactor tables and deltas

Specific-mass columns are in `t/GW` of mass-rated reactor output. The `t/GWe` column divides by the unchanged efficiency. "Cap driver" is `mass floor` only where the +10% baseline would violate the tier-size invariant.

### Regular Solid Core

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Solid Core Fission Reactor I | 0.025 | 40 | 240 | **310** | +70 (+29.2%) | 417.391304 -> **539.130435** | 2 GW | **2.2 GW** | +0.2 GW (+10%) | 480 t -> **682 t** | - | +10% baseline |
| Solid Core Fission Reactor II | 0.025 | 34 | 204 | **263.5** | +59.5 (+29.2%) | 340 -> **439.166667** | 3 GW | **3.3 GW** | +0.3 GW (+10%) | 612 t -> **869.55 t** | +27.5% | +10% baseline |
| Solid Core Fission Reactor III | 0.025 | 28 | 168 | **217** | +49 (+29.2%) | 268.8 -> **347.2** | 10 GW | **11 GW** | +1 GW (+10%) | 1,680 t -> **2,387 t** | +174.5% | +10% baseline |
| Solid Core Fission Reactor IV | 0.025 | 12 | 72 | **93** | +21 (+29.2%) | 110.769231 -> **143.076923** | 30 GW | **33 GW** | +3 GW (+10%) | 2,160 t -> **3,069 t** | +28.6% | +10% baseline |
| Solid Core Fission Reactor V | 0.025 | 8 | 48 | **62** | +14 (+29.2%) | 71.111111 -> **91.851852** | 60 GW | **66 GW** | +6 GW (+10%) | 2,880 t -> **4,092 t** | +33.3% | +10% baseline |

### Compact Solid Core

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Compact Solid Core Fission Reactor I | 0.025 | 6 | 36 | **46.5** | +10.5 (+29.2%) | 60 -> **77.5** | 0.75 GW | **0.9 GW** | +0.15 GW (+20%) | 27 t -> **41.85 t** | - | +10% baseline |
| Compact Solid Core Fission Reactor II | 0.025 | 5 | 30 | **38.75** | +8.75 (+29.2%) | 48 -> **62** | 2 GW | **2.2 GW** | +0.2 GW (+10%) | 60 t -> **85.25 t** | +103.7% | +10% baseline |
| Compact Solid Core Fission Reactor III | 0.025 | 4 | 24 | **31** | +7 (+29.2%) | 36.923077 -> **47.692308** | 4 GW | **4.4 GW** | +0.4 GW (+10%) | 96 t -> **136.4 t** | +60% | +10% baseline |
| Compact Solid Core Fission Reactor IV | 0.025 | 3 | 18 | **23.25** | +5.25 (+29.2%) | 26.666667 -> **34.444444** | 6 GW | **6.6 GW** | +0.6 GW (+10%) | 108 t -> **153.45 t** | +12.5% | +10% baseline |
| Compact Solid Core Fission Reactor V | 0.025 | 2 | 12 | **15.5** | +3.5 (+29.2%) | 17.142857 -> **22.142857** | 10 GW | **11 GW** | +1 GW (+10%) | 120 t -> **170.5 t** | +11.1% | +10% baseline |

### Molten Salt

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Molten Salt Fission Reactor I | 0.05 | 2 | 15 | **16** | +1 (+6.7%) | 20.689655 -> **22.068966** | 40 GW | **44 GW** | +4 GW (+10%) | 600 t -> **704 t** | - | +10% baseline |
| Molten Salt Fission Reactor II | 0.05 | 1.8 | 12 | **14.4** | +2.4 (+20%) | 16 -> **19.2** | 400 GW | **440 GW** | +40 GW (+10%) | 4,800 t -> **6,336 t** | +800% | +10% baseline |

### Molten Core

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Molten Core Fission Reactor I | 0.05 | 4 | 16 | **32** | +16 (+100%) | 23.703704 -> **47.407407** | 4 GW | **4.4 GW** | +0.4 GW (+10%) | 64 t -> **140.8 t** | - | +10% baseline |
| Molten Core Fission Reactor II | 0.05 | 3.5 | 14 | **28** | +14 (+100%) | 19.858156 -> **39.716312** | 17 GW | **19 GW** | +2 GW (+11.8%) | 238 t -> **532 t** | +277.8% | +10% baseline |
| Molten Core Fission Reactor III | 0.05 | 3 | 12 | **24** | +12 (+100%) | 16.551724 -> **33.103448** | 200 GW | **220 GW** | +20 GW (+10%) | 2,400 t -> **5,280 t** | +892.5% | +10% baseline |

### Vapor Core

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Vapor Core Fission Reactor I | 0.1 | 4 | 9 | **34** | +25 (+277.8%) | 10.344828 -> **39.08046** | 6.5 GW | **7.2 GW** | +0.7 GW (+10.8%) | 58.5 t -> **244.8 t** | - | +10% baseline |
| Vapor Core Fission Reactor II | 0.1 | 3 | 8 | **25.5** | +17.5 (+218.8%) | 9.090909 -> **28.977273** | 20 GW | **22 GW** | +2 GW (+10%) | 160 t -> **561 t** | +129.2% | +10% baseline |
| Vapor Core Fission Reactor III | 0.1 | 2.5 | 7 | **21.25** | +14.25 (+203.6%) | 7.865169 -> **23.876404** | 60 GW | **66 GW** | +6 GW (+10%) | 420 t -> **1,403 t** | +150% | +10% baseline |

### Gas Core

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Gas Core Fission Reactor I | 0.1 | 8 | 20 | **68** | +48 (+240%) | 22.988506 -> **78.16092** | 8 GW | **8.8 GW** | +0.8 GW (+10%) | 160 t -> **598.4 t** | - | +10% baseline |
| Gas Core Fission Reactor II | 0.1 | 5 | 16 | **42.5** | +26.5 (+165.6%) | 17.977528 -> **47.752809** | 33 GW | **37 GW** | +4 GW (+12.1%) | 528 t -> **1,573 t** | +162.8% | +10% baseline |
| Gas Core Fission Reactor III | 0.1 | 3 | 10 | **25.5** | +15.5 (+155%) | 10.989011 -> **28.021978** | 150 GW | **170 GW** | +20 GW (+13.3%) | 1,500 t -> **4,335 t** | +175.7% | +10% baseline |

### Terawatt Gas Core

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Gas Core Fission Reactor IV | 0.1 | 10 | 7 | **85** | +78 (+1114.3%) | 7.608696 -> **92.391304** | 1 TW | **1.1 TW** | +100 GW (+10%) | 7,000 t -> **93,500 t** | - | +10% baseline |
| Gas Core Fission Reactor V | 0.1 | 3.5 | 6 | **29.75** | +23.75 (+395.8%) | 6.451613 -> **31.989247** | 1.3 TW | **3.3 TW** | +2000 GW (+153.8%) | 7,800 t -> **98,175 t** | +5% | **mass floor** |
| Gas Core Fission Reactor VI | 0.1 | 1 | 5 | **8.5** | +3.5 (+70%) | 5.319149 -> **9.042553** | 1.7 TW | **13 TW** | +11300 GW (+664.7%) | 8,500 t -> **110,500 t** | +12.6% | **mass floor** |

### Electrostatic Fusion

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Electrostatic Confinement Fusion Reactor I | 0.3 | 1 | 1 | **10.5** | +9.5 (+950%) | 1.052632 -> **11.052632** | 46 GW | **51 GW** | +5 GW (+10.9%) | 46 t -> **535.5 t** | - | +10% baseline |
| Electrostatic Confinement Fusion Reactor II | 0.3 | 0.5 | 0.5 | **5.25** | +4.75 (+950%) | 0.526316 -> **5.526316** | 74 GW | **110 GW** | +36 GW (+48.6%) | 37 t -> **577.5 t** | +7.8% | **mass floor** |
| Electrostatic Confinement Fusion Reactor III | 0.3 | 0.005 | 0.005 | **0.0525** | +0.0475 (+950%) | 0.005263 -> **0.055263** | 310 GW | **12 TW** | +11690 GW (+3771%) | 1.55 t -> **630 t** | +9.1% | **mass floor** |

### Mirror Cell Fusion

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Mirror Cell Fusion Reactor I | 0.15 | 5 | 5 | **45** | +40 (+800%) | 5.376344 -> **48.387097** | 120 GW | **140 GW** | +20 GW (+16.7%) | 600 t -> **6,300 t** | - | +10% baseline |
| Mirror Cell Fusion Reactor II | 0.15 | 4 | 4 | **36** | +32 (+800%) | 4.210526 -> **37.894737** | 215 GW | **240 GW** | +25 GW (+11.6%) | 860 t -> **8,640 t** | +37.1% | +10% baseline |
| Mirror Cell Fusion Reactor III | 0.15 | 0.8 | 0.8 | **7.2** | +6.4 (+800%) | 0.824742 -> **7.42268** | 256 GW | **1.3 TW** | +1044 GW (+407.8%) | 204.8 t -> **9,360 t** | +8.3% | **mass floor** |

### Tokamak Fusion

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Fusion Tokamak I | 0.3 | 4 | 4 | **42** | +38 (+950%) | 4.347826 -> **45.652174** | 128 GW | **150 GW** | +22 GW (+17.2%) | 512 t -> **6,300 t** | - | +10% baseline |
| Fusion Tokamak II | 0.3 | 2 | 2 | **21** | +19 (+950%) | 2.105263 -> **22.105263** | 401 GW | **450 GW** | +49 GW (+12.2%) | 802 t -> **9,450 t** | +50% | +10% baseline |
| Fusion Tokamak III | 0.3 | 1 | 1 | **10.5** | +9.5 (+950%) | 1.041667 -> **10.9375** | 624 GW | **950 GW** | +326 GW (+52.2%) | 624 t -> **9,975 t** | +5.6% | **mass floor** |
| Fusion Tokamak IV | 0.3 | 0.5 | 0.5 | **5.25** | +4.75 (+950%) | 0.507614 -> **5.329949** | 1.26 TW | **2 TW** | +740 GW (+58.7%) | 630 t -> **10,500 t** | +5.3% | **mass floor** |
| Fusion Tokamak V | 0.3 | 0.1 | 0.1 | **1.05** | +0.95 (+950%) | 0.10101 -> **1.060606** | 5.06 TW | **11 TW** | +5940 GW (+117.4%) | 506 t -> **11,550 t** | +10% | **mass floor** |

### Hybrid Fusion

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Hybrid Confinement Fusion Reactor I | 0.2 | 2 | 2 | **19** | +17 (+850%) | 2.061856 -> **19.587629** | 180 GW | **200 GW** | +20 GW (+11.1%) | 360 t -> **3,800 t** | - | +10% baseline |
| Hybrid Confinement Fusion Reactor II | 0.2 | 1 | 1 | **9.5** | +8.5 (+850%) | 1.020408 -> **9.693878** | 510 GW | **570 GW** | +60 GW (+11.8%) | 510 t -> **5,415 t** | +42.5% | +10% baseline |
| Hybrid Confinement Fusion Reactor III | 0.2 | 0.5 | 0.5 | **4.75** | +4.25 (+850%) | 0.505051 -> **4.79798** | 1.9 TW | **2.1 TW** | +200 GW (+10.5%) | 950 t -> **9,975 t** | +84.2% | +10% baseline |
| Hybrid Confinement Fusion Reactor IV | 0.2 | 0.05 | 0.05 | **0.475** | +0.425 (+850%) | 0.050505 -> **0.479798** | 11.37 TW | **23 TW** | +11630 GW (+102.3%) | 568.5 t -> **10,925 t** | +9.5% | **mass floor** |

### Z-Pinch Fusion

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Z-Pinch Fusion Reactor I | 0.25 | 3 | 3 | **30** | +27 (+900%) | 3.157895 -> **31.578947** | 260 GW | **290 GW** | +30 GW (+11.5%) | 780 t -> **8,700 t** | - | +10% baseline |
| Z-Pinch Fusion Reactor II | 0.25 | 2 | 2 | **20** | +18 (+900%) | 2.105263 -> **21.052632** | 610 GW | **680 GW** | +70 GW (+11.5%) | 1,220 t -> **13,600 t** | +56.3% | +10% baseline |
| Z-Pinch Fusion Reactor III | 0.25 | 1.4 | 1.4 | **14** | +12.6 (+900%) | 1.458333 -> **14.583333** | 2.51 TW | **2.8 TW** | +290 GW (+11.6%) | 3,514 t -> **39,200 t** | +188.2% | +10% baseline |
| Z-Pinch Fusion Reactor IV | 0.25 | 0.4 | 0.4 | **4** | +3.6 (+900%) | 0.408163 -> **4.081633** | 3.97 TW | **11 TW** | +7030 GW (+177.1%) | 1,588 t -> **44,000 t** | +12.2% | **mass floor** |
| Flow Stabilized Z-Pinch Fusion Reactor | 0.25 | 0.0068 | 0.0068 | **0.068** | +0.0612 (+900%) | 0.006834 -> **0.068342** | 7.59 TW | **680 TW** | +672410 GW (+8859.2%) | 51.612 t -> **46,240 t** | +5.1% | **mass floor** |

### Inertial Fusion

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Inertial Confinement Fusion Reactor I | 0.3 | 4 | 4 | **42** | +38 (+950%) | 4.705882 -> **49.411765** | 370 GW | **410 GW** | +40 GW (+10.8%) | 1,480 t -> **17,220 t** | - | +10% baseline |
| Inertial Confinement Fusion Reactor II | 0.3 | 2 | 2 | **21** | +19 (+950%) | 2.247191 -> **23.595506** | 860 GW | **950 GW** | +90 GW (+10.5%) | 1,720 t -> **19,950 t** | +15.9% | +10% baseline |
| Inertial Confinement Fusion Reactor III | 0.3 | 1 | 1 | **10.5** | +9.5 (+950%) | 1.086957 -> **11.413043** | 3.17 TW | **3.5 TW** | +330 GW (+10.4%) | 3,170 t -> **36,750 t** | +84.2% | +10% baseline |
| Inertial Confinement Fusion Reactor IV | 0.3 | 0.5 | 0.5 | **5.25** | +4.75 (+950%) | 0.526316 -> **5.526316** | 5.5 TW | **7.4 TW** | +1900 GW (+34.5%) | 2,750 t -> **38,850 t** | +5.7% | **mass floor** |
| Inertial Confinement Fusion Reactor V | 0.3 | 0.25 | 0.25 | **2.625** | +2.375 (+950%) | 0.25641 -> **2.692308** | 19.09 TW | **21 TW** | +1910 GW (+10%) | 4,773 t -> **55,125 t** | +41.9% | +10% baseline |
| Inertial Confinement Fusion Reactor VI | 0.3 | 0.068 | 0.068 | **0.714** | +0.646 (+950%) | 0.068687 -> **0.721212** | 20.42 TW | **82 TW** | +61580 GW (+301.6%) | 1,389 t -> **58,548 t** | +6.2% | **mass floor** |
| Inertial Confinement Fusion Reactor VII | 0.3 | 0.002 | 0.002 | **0.021** | +0.019 (+950%) | 0.002002 -> **0.021021** | 306.43 TW | **3 PW** | +2693570 GW (+879%) | 612.86 t -> **63,000 t** | +7.6% | **mass floor** |

### Antimatter (fusion balance bucket)

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Antimatter Plasma Core Reactor I | 0.25 | 0.4 | 0.4 | **4** | +3.6 (+900%) | 0.401003 -> **4.010025** | 1.2 TW | **1.4 TW** | +200 GW (+16.7%) | 480 t -> **5,600 t** | - | +10% baseline |
| Antimatter Plasma Core Reactor II | 0.25 | 0.04 | 0.04 | **0.4** | +0.36 (+900%) | 0.0401 -> **0.401003** | 11.15 TW | **15 TW** | +3850 GW (+34.5%) | 446 t -> **6,000 t** | +7.1% | **mass floor** |
| Antimatter Plasma Core Reactor III | 0.25 | 0.004 | 0.004 | **0.04** | +0.036 (+900%) | 0.004008 -> **0.04008** | 66 TW | **160 TW** | +94000 GW (+142.4%) | 264 t -> **6,400 t** | +6.7% | **mass floor** |
| Antimatter Beam Core Reactor | 0.2 | 0.00002 | 0.00002 | **0.00019** | +0.00017 (+850%) | 0.00002 -> **0.00019** | 3 PW | **35.4 PW** | +32400000 GW (+1080%) | 60 t -> **6,726 t** | +5.1% | **mass floor** |

### Alien Hybrid Fusion

| Plant | `s` | Vanilla SP | Current SP | Proposed SP | SP delta | Current -> proposed t/GWe | Current cap | Proposed cap | Cap delta | Current -> proposed full mass | Mass vs prior tier | Cap driver |
|---|---:|---:|---:|---:|---:|---|---:|---:|---:|---|---:|---|
| Alien Hybrid Confinement Fusion Reactor | 0.2 | 1 | 0.5 | **0.5** | unchanged | 0.502513 -> **0.502513** | 5 TW | **5.5 TW** | +500 GW (+10%) | 2,500 t -> **2,750 t** | - | +10% baseline |
| Alien Advanced Hybrid Confinement Fusion Reactor | 0.2 | 0.35 | 0.175 | **0.175** | unchanged | 0.175351 -> **0.175351** | 32 TW | **36 TW** | +4000 GW (+12.5%) | 5,600 t -> **6,300 t** | +129.1% | +10% baseline |
| Alien Super Advanced Hybrid Confinement Fusion Reactor | 0.2 | 0.05 | 0.025 | **0.025** | unchanged | 0.025013 -> **0.025013** | 107.55 TW | **270 TW** | +162450 GW (+151%) | 2,689 t -> **6,750 t** | +7.1% | **mass floor** |

## Invariant audit

- Non-alien reactor specific masses raised: **55 / 55**.
- Alien specific masses intentionally unchanged: **3 / 3**.
- Maximum-output caps raised: **58 / 58**.
- Later-tier full-rated mass failures: **0**.
- Tiers requiring the monotonic-mass floor instead of only the baseline: **18**.

## Deliberate non-goals and implementation boundary

This plan does not optimize for any particular drive, hull, appearance, or reactor-bay fill. It also does not repair technology labels, move project prerequisites, alter efficiencies, change open-cycle factors, or impose a sub-terawatt gate. Those would add goals beyond the two invariants.

If implemented later, the change should modify only `specificPower_tGW` and `maxOutput_GW`, regenerate the merged power-plant snapshot, validate all 58 reactor rows and every adjacent tier mass, and then use the normal build/deploy/manual-test workflow.

## Data sources

- `docs/ship-balance-research/tables/powerplant.csv`: vanilla specific masses and caps.
- `docs/ship-balance-research/tables/powerplant-current.csv`: current merged EEO values and efficiencies.
- `TIEconomyMod/Core/PowerPlantScalingMath.cs`: thermal-family default multipliers.
- `TIEconomyMod/ModFiles/TIPowerPlantTemplate.json`: explicit live fission and alien overrides.
- `docs/ship-balance-research/open-cycle-reactor-mass-scaling-plan-2026-08-21.md`: runtime meaning of `s`, thermal demand, and mass-rated output.
