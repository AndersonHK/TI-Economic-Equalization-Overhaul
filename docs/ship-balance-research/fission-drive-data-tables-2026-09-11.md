# Fission drive and reactor data tables — 2026-09-11

Research snapshot for installed Terra Invicta 1.0.53 + EEO 0.9.7. See the [analysis and scientific bounds](fission-drive-progression-and-scientific-bounds-2026-09-11.md) for interpretation, assumptions, and sources. No gameplay values were changed.

All drives are single-thruster templates at hull-art scale 1. D is computed runtime **external** input power: thermal for Open, electrical under current EEO accounting for Closed, and zero for Self. Drive efficiency ηd is separate from reactor electrical efficiency η. Self-powered entries still produce jet power and carry propulsion hardware/fuel; zero external D is not zero energy. Open includes resolved Calc cooling, using the single-thruster ≥3 kg/s rule.

Reactor mass coefficients exclude auxiliaries, the one-tonne floor, radiators and other ship mass. “Open advantage” is α/(sη), with α=1−0.01(1−η) and s=0.5. “Mass vs vanilla” uses vanilla bD and current bsD/α or bD/η, holding drive input fixed. The full pairing snapshot applies the one-tonne floor separately to each zero-auxiliary example.

The reactor frontier follows the primary drive-project chain and stops at reactor projects; earlier reactor tiers in that reactor's own ladder are not expanded there. It is neither an exhaustive science prerequisite list nor the cheapest reactor choice. Alternatives are shown explicitly in the project table: altPrereq0 replaces the **first** listed prerequisite, retaining the rest. Base project RP is the JSON cost, not the EEO-adjusted cost; the enabled project multiplier is 1.4. Maximum unlock chance is a template field before faction effects, not guaranteed acquisition probability.

Display labels use Compact Solid Core I–V for internal SolidCoreFissionReactorVI–X and Terawatt Gas Core I–III for internal GasCoreFissionReactorIV–VI. Gas and vapor plants share the Gas_Core_Fission compatibility class. Molten_Salt_Core_Fission additionally accepts Solid_Core_Fission and Liquid_Core_Fission drives.

## Complete reactor coefficients and cycle deltas


| Reactor (display name) | Cap vanilla → current GW | η vanilla → current | SP vanilla → current t/GW | Current open t/GW input | Current closed t/GW input | Open advantage × | Mass vs vanilla open / closed × |
|---|---|---|---|---|---|---|---|
| Solid Core I | 2 → 2 | 75% → 57.5% | 40 → 240 | 120.512 | 417.391 | 3.463 | 3.013 / 10.435 |
| Solid Core II | 6 → 3 | 77.5% → 60% | 34 → 204 | 102.41 | 340 | 3.32 | 3.012 / 10 |
| Solid Core III | 20 → 10 | 80% → 62.5% | 28 → 168 | 84.316 | 268.8 | 3.188 | 3.011 / 9.6 |
| Solid Core IV | 60 → 30 | 82.5% → 65% | 12 → 72 | 36.126 | 110.769 | 3.066 | 3.011 / 9.231 |
| Solid Core V | 125 → 60 | 85% → 67.5% | 8 → 48 | 24.078 | 71.111 | 2.953 | 3.01 / 8.889 |
| Compact Solid Core I | 1.5 → 0.75 | 77.5% → 60% | 6 → 36 | 18.072 | 60 | 3.32 | 3.012 / 10 |
| Compact Solid Core II | 5 → 2 | 80% → 62.5% | 5 → 30 | 15.056 | 48 | 3.188 | 3.011 / 9.6 |
| Compact Solid Core III | 12 → 4 | 82.5% → 65% | 4 → 24 | 12.042 | 36.923 | 3.066 | 3.011 / 9.231 |
| Compact Solid Core IV | 20 → 6 | 85% → 67.5% | 3 → 18 | 9.029 | 26.667 | 2.953 | 3.01 / 8.889 |
| Compact Solid Core V | 20 → 10 | 87.5% → 70% | 2 → 12 | 6.018 | 17.143 | 2.849 | 3.009 / 8.571 |
| Molten Salt I | 40 → 40 | 92% → 72.5% | 2 → 15 | 7.521 | 20.69 | 2.751 | 3.76 / 10.345 |
| Molten Salt II | 420 → 400 | 93% → 75% | 1.8 → 12 | 6.015 | 16 | 2.66 | 3.342 / 8.889 |
| Molten Core I | 8 → 4 | 85% → 67.5% | 4 → 16 | 8.026 | 23.704 | 2.953 | 2.007 / 5.926 |
| Molten Core II | 35 → 17 | 88% → 70.5% | 3.5 → 14 | 7.021 | 19.858 | 2.829 | 2.006 / 5.674 |
| Molten Core III | 420 → 200 | 90% → 72.5% | 3 → 12 | 6.017 | 16.552 | 2.751 | 2.006 / 5.517 |
| Vapor Core I | 6.5 → 6.5 | 90% → 87% | 4 → 9 | 4.506 | 10.345 | 2.296 | 1.126 / 2.586 |
| Vapor Core II | 20 → 20 | 92% → 88% | 3 → 8 | 4.005 | 9.091 | 2.27 | 1.335 / 3.03 |
| Vapor Core III | 60 → 60 | 92% → 89% | 2.5 → 7 | 3.504 | 7.865 | 2.245 | 1.402 / 3.146 |
| Gas Core I | 8 → 8 | 90% → 87% | 8 → 20 | 10.013 | 22.989 | 2.296 | 1.252 / 2.874 |
| Gas Core II | 33 → 33 | 91% → 89% | 5 → 16 | 8.009 | 17.978 | 2.245 | 1.602 / 3.596 |
| Gas Core III | 150 → 150 | 95% → 91% | 3 → 10 | 5.005 | 10.989 | 2.196 | 1.668 / 3.663 |
| Terawatt Gas Core I | 1,650 → 1,000 | 93% → 92% | 10 → 7 | 3.503 | 7.609 | 2.172 | 0.35 / 0.761 |
| Terawatt Gas Core II | 1,650 → 1,300 | 95% → 93% | 3.5 → 6 | 3.002 | 6.452 | 2.149 | 0.858 / 1.843 |
| Terawatt Gas Core III | 1,650 → 1,700 | 96% → 94% | 1 → 5 | 2.502 | 5.319 | 2.126 | 2.502 / 5.319 |


## Solid-core thermal and fragment


| Drive | Cruise kN | Cruise km/s | Combat × | Combat kN | Combat km/s | D GW | ηd | Cycle | Reactor frontier (primary drive chain) |
|---|---|---|---|---|---|---|---|---|---|
| Kiwi Drive | 33 | 8.77 | 10 | 330 | 0.877 | 0.207 | 70% | Open | Compact Solid Core I |
| Nerva Drive | 49 | 8.09 | 9 | 441 | 0.899 | 0.283 | 70% | Open | Solid Core I |
| Snare Drive | 73 | 8.83 | 10 | 730 | 0.883 | 0.46 | 70% | Open | Compact Solid Core II |
| Rover Drive | 245 | 8.18 | 10 | 2,450 | 0.818 | 1.432 | 70% | Open | Compact Solid Core III |
| Cermet Nerva | 134.4 | 9.81 | 12 | 1,612.8 | 0.818 | 0.942 | 70% | Open | Solid Core I |
| Advanced Nerva Drive | 334.061 | 8.09 | 9 | 3,006.549 | 0.899 | 1.93 | 70% | Open | Solid Core I |
| Dumbo | 400 | 8.3 | 9 | 3,600 | 0.922 | 2.29 | 72.5% | Open | Solid Core II |
| Advanced Cermet Nerva | 445.267 | 9.12 | 12 | 5,343.204 | 0.76 | 2.707 | 75% | Open | Solid Core I; Solid Core II |
| Heavy Dumbo | 3,500 | 8.09 | 9 | 31,500 | 0.899 | 19.528 | 72.5% | Open | Solid Core II; Solid Core IV |
| Pulsar Drive | 180 | 16 | 5 | 900 | 3.2 | 1.694 | 85% | Closed | Solid Core I; Solid Core III |
| Advanced Pulsar Drive | 240 | 32 | 5 | 1,200 | 6.4 | 4.267 | 90% | Closed | Solid Core I; Solid Core III; Solid Core V |
| Pebble Drive | 172.7 | 9.81 | 15 | 2,590.5 | 0.654 | 0.901 | 94% | Open | Compact Solid Core IV |
| Advanced Pebble Drive | 333.617 | 9.81 | 16 | 5,337.872 | 0.613 | 1.741 | 94% | Open | Compact Solid Core IV; Compact Solid Core V |
| Fission Frag Drive | 4.651 | 313.9 | 2 | 9.302 | 156.95 | 1.587 | 46% | Open | Compact Solid Core II |


## Liquid-core


| Drive | Cruise kN | Cruise km/s | Combat × | Combat kN | Combat km/s | D GW | ηd | Cycle | Reactor frontier (primary drive chain) |
|---|---|---|---|---|---|---|---|---|---|
| Lars Drive | 98 | 19.62 | 15 | 1,470 | 1.308 | 1.131 | 85% | Closed | Molten Core I |
| Teardrop Drive | 333 | 19.62 | 15 | 4,995 | 1.308 | 4.356 | 75% | Open | Molten Core II |
| Fission Spinner Drive | 540 | 17.7 | 14 | 7,560 | 1.264 | 5.622 | 85% | Open | Molten Core I; Molten Core II |
| Pegasus Drive | 7,000 | 16 | 14 | 98,000 | 1.143 | 65.882 | 85% | Open | Molten Core I; Molten Core II; Molten Core III |


## Vapor/gas-core and dusty plasma


| Drive | Cruise kN | Cruise km/s | Combat × | Combat kN | Combat km/s | D GW | ηd | Cycle | Reactor frontier (primary drive chain) |
|---|---|---|---|---|---|---|---|---|---|
| Vortex Drive | 504 | 19.62 | 15 | 7,560 | 1.308 | 7.063 | 70% | Open | Vapor Core II |
| Advanced Vortex Drive | 504 | 24.48 | 15 | 7,560 | 1.632 | 8.813 | 70% | Open | Vapor Core II; Vapor Core III |
| Cavity Drive | 56.4 | 20.4 | 15 | 846 | 1.36 | 0.677 | 85% | Closed | Vapor Core I |
| Advanced Cavity Drive | 330 | 30.88 | 16 | 5,280 | 1.93 | 5.994 | 85% | Closed | Vapor Core III; Vapor Core I |
| Quartz Drive | 117.7 | 18.3 | 12 | 1,412.4 | 1.525 | 1.267 | 85% | Closed | Gas Core I |
| Lightbulb Drive | 409 | 20.4 | 15 | 6,135 | 1.36 | 4.908 | 85% | Closed | Gas Core I |
| Pharos Drive | 890 | 25.5 | 16 | 14,240 | 1.594 | 13.35 | 85% | Closed | Gas Core III |
| Lodestar Fission Lantern | 11,000 | 31.4 | 20 | 220,000 | 1.57 | 186.703 | 92.5% | Closed | Terawatt Gas Core I |
| Dusty Plasma Drive | 5.504 | 3,750 | 160 | 880.64 | 23.438 | 22.435 | 46% | Open | Gas Core I; Compact Solid Core II |
| Burner Drive | 108 | 69 | 24 | 2,592 | 2.875 | 4.384 | 85% | Open | Gas Core II |
| Flare Drive | 3,500 | 35 | 20 | 70,000 | 1.75 | 72.059 | 85% | Open | Terawatt Gas Core II |
| Firestar Fission Lantern | 5,000 | 50 | 22 | 110,000 | 2.273 | 147.059 | 85% | Open | Terawatt Gas Core III |


## Self-powered fission pulse and salt water


| Drive | Cruise kN | Cruise km/s | Combat × | Combat kN | Combat km/s | D GW | ηd | Cycle | Reactor frontier (primary drive chain) |
|---|---|---|---|---|---|---|---|---|---|
| Neutron Flux Lantern | 12,900 | 66 | 2 | 25,800 | 33 | 0 | 80% | Self | Independent branch |
| Neutron Flux Torch | 13,000 | 1,700 | 2 | 26,000 | 850 | 0 | 80% | Self | Independent branch |
| Z-pinch Microfission Drive | 24 | 156.96 | 40 | 960 | 3.924 | 0 | 98% | Self | Independent branch |
| Neutronium Microfission Drive | 180 | 156.96 | 80 | 14,400 | 1.962 | 0 | 99% | Self | Independent branch |
| Antimatter Microfission Drive | 275 | 132.44 | 60 | 16,500 | 2.207 | 0 | 98% | Self | Independent branch |
| Minimag Orion | 642 | 93.16 | 40 | 25,680 | 2.329 | 0 | 100% | Self | Independent branch |
| Advanced Minimag Orion | 1,870 | 157 | 40 | 74,800 | 3.925 | 0 | 100% | Self | Independent branch |
| Orion Drive | 16,000 | 42.1 | 5 | 80,000 | 8.42 | 0 | 100% | Self | Independent branch |
| Advanced Orion Drive | 24,000 | 120 | 5 | 120,000 | 24 | 0 | 100% | Self | Independent branch |


## Exact immediate drive-project prerequisites


| Drive | Base project RP | Immediate prerequisites (all unless alternative) | Alternative field | Maximum unlock chance |
|---|---|---|---|---|
| Kiwi Drive | 250 | Compact Solid Core I | — | 60% |
| Nerva Drive | 300 | Solid Core I | — | 100% |
| Snare Drive | 500 | Compact Solid Core II | — | 60% |
| Rover Drive | 750 | Compact Solid Core III | — | 60% |
| Cermet Nerva | 750 | Nerva Drive | — | 75% |
| Advanced Nerva Drive | 1,000 | Nerva Drive | — | 60% |
| Dumbo | 1,000 | Solid Core II | — | 60% |
| Advanced Cermet Nerva | 1,500 | Cermet Nerva Drive; Dumbo Drive | — | 50% |
| Heavy Dumbo | 2,500 | Dumbo Drive; Solid Core IV | — | 40% |
| Pulsar Drive | 2,000 | Arc Lasers; Nerva Drive; Solid Core III | altPrereq0=Particle Beams | 50% |
| Advanced Pulsar Drive | 4,000 | Superconducting Magnets; Pulsar Drive; Solid Core V | — | 25% |
| Pebble Drive | 1,500 | Compact Solid Core IV | — | 100% |
| Advanced Pebble Drive | 1,500 | Pebble Drive; Compact Solid Core V | — | 35% |
| Lars Drive | 1,500 | Molten Core I | — | 100% |
| Teardrop Drive | 2,000 | Molten Core II | — | 100% |
| Fission Spinner Drive | 5,000 | Lars Drive; Molten Core II | — | 60% |
| Pegasus Drive | 12,000 | Fission Spinner Drive; Molten Core III | altPrereq0=Teardrop Drive | 20% |
| Vortex Drive | 1,500 | Vapor Core II | — | 100% |
| Advanced Vortex Drive | 1,500 | Vortex Drive; Vapor Core III | — | 30% |
| Cavity Drive | 750 | Vapor Core I | — | 60% |
| Advanced Cavity Drive | 1,500 | Vapor Core III; Cavity Drive; Superconducting Magnets | altPrereq0=Gas Core II | 20% |
| Quartz Drive | 250 | Gas Core I | — | 100% |
| Lightbulb Drive | 1,500 | Quartz Drive | — | 100% |
| Pharos Drive | 2,000 | Superalloys; Gas Core III | — | 70% |
| Lodestar Fission Lantern | 10,000 | Magnetic Force Manipulation; Terawatt Gas Core I | — | 15% |
| Fission Frag Drive | 1,000 | Advanced Carbon Manipulation; Compact Solid Core II | — | 50% |
| Dusty Plasma Drive | 5,000 | Magnetic Nozzles; Gas Core I; Fission Frag Drive | — | 50% |
| Burner Drive | 5,000 | Superalloys; Gas Core II | — | 10% |
| Flare Drive | 15,000 | Magnetic Force Manipulation; Terawatt Gas Core II | — | 10% |
| Firestar Fission Lantern | 20,000 | Superconducting Magnets; Superalloys; Terawatt Gas Core III | — | 10% |
| Neutron Flux Lantern | 35,000 | Advanced Fission Systems | — | 15% |
| Neutron Flux Torch | 150,000 | Neutron Flux Lantern; Magnetic Nozzles | — | 10% |
| Z-pinch Microfission Drive | 1,000 | Z-Pinch Techniques; Fission Pulse Drives | — | 25% |
| Neutronium Microfission Drive | 2,000 | Ultracold Neutron Containment; Fission Pulse Drives | — | 15% |
| Antimatter Microfission Drive | 5,000 | Antimatter Containment; Fission Pulse Drives | — | 25% |
| Minimag Orion | 5,000 | Z-Pinch Microfission Drive | — | 100% |
| Advanced Minimag Orion | 10,000 | Minimag Orion Drive | — | 50% |
| Orion Drive | 10,000 | Fission Pulse Drives | — | 100% |
| Advanced Orion Drive | 25,000 | Heavy Pulsed Propulsion; Orion Drive | — | 50% |


## Reactor project ladder


| Reactor | Base project RP | Immediate prerequisites |
|---|---|---|
| Solid Core I | 250 | Solid Core Fission Systems |
| Solid Core II | 500 | Solid Core I |
| Solid Core III | 600 | Solid Core II |
| Solid Core IV | 700 | Solid Core III |
| Solid Core V | 800 | Solid Core IV |
| Compact Solid Core I | 700 | Solid Core II |
| Compact Solid Core II | 800 | Compact Solid Core I |
| Compact Solid Core III | 900 | Compact Solid Core II |
| Compact Solid Core IV | 1,000 | Compact Solid Core III |
| Compact Solid Core V | 1,200 | Compact Solid Core IV |
| Molten Salt I | 800 | Molten Core I; Solid Core III |
| Molten Salt II | 1,500 | Molten Salt I; Molten Core III |
| Molten Core I | 500 | Molten Core Fission Systems |
| Molten Core II | 1,000 | Molten Core I; Advanced Carbon Manipulation |
| Molten Core III | 2,000 | Molten Core II; Carbon Nanotubes |
| Vapor Core I | 800 | Gas Core Fission Systems; Molten Core II |
| Vapor Core II | 1,200 | Vapor Core I; Advanced Carbon Manipulation |
| Vapor Core III | 1,500 | Superalloys; Carbon Nanotubes; Vapor Core II |
| Gas Core I | 1,000 | Gas Core Fission Systems |
| Gas Core II | 1,250 | Gas Core I |
| Gas Core III | 1,500 | Gas Core II; Carbon Nanotubes |
| Terawatt Gas Core I | 10,000 | Advanced Fission Systems; Gas Core II |
| Terawatt Gas Core II | 10,000 | Terawatt Gas Core I |
| Terawatt Gas Core III | 10,000 | Terawatt Gas Core II |
