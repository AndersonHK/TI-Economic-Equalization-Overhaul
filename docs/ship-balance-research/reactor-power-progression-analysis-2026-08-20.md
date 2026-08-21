# Reactor power-progression and large-hull scaling analysis

Status: planning analysis only. No gameplay values are changed by this report.

Game-data baseline: Terra Invicta 1.0.51 with the live EEO power-plant,
appearance-driven drive-scaling, and reactor-bay overrides as of 2026-08-20.

## Conclusion

There is enough numerical room to raise the fission curve substantially while
keeping every non-terawatt-labelled reactor below `1,000 GW`:

- set the first Solid Core reactor to **8 GW**;
- let the ordinary fission lines climb through tens and hundreds of gigawatts;
- place Gas Core III at **900 GW**;
- let the localized Terawatt Gas Core I-III line rise through approximately
  **1.5 / 3.5 / 7 TW**.

The `7 TW` Gas Core VI endpoint is not an arbitrary large number. At its current
`5 t/GW`, it occupies about `7,875 m3`, or **49% of the default Titan reactor
bay**, at full rating. The geometry-derived hard limit in that bay is about
`14.2 TW`, so roughly `5-8 TW` is a comfortable useful range and `14 TW` is the
upper geometric boundary before changing its specific mass or bay model.

Fusion needs a split treatment. Fusion plants before the global **Terawatt
Fusion Reactors** technology should be compressed below `1 TW`; post-terawatt
plants can then rise by a genuine order of magnitude. The current data do the
opposite in several branches: six pre-terawatt plants already rate from
`1.26-5.50 TW`.

Raising `maxOutput_GW` alone will not solve the small-reactor observation.
`maxOutput_GW` is a compatibility ceiling. Installed mass and occupied bay
volume follow the ship's actual load multiplied by `specificPower_tGW`. Several
late fusion plants are tiny even at full rating because their specific mass
falls by factors of 10-100 between adjacent generations. The output-cap pass
therefore needs a paired specific-mass pass.

## What the runtime fields actually do

Runtime inspection gives these relevant relationships:

```text
drive generation demand = hull-scaled drive power
ship generation demand  = drive demand
                        + systems demand / plant efficiency
                        + weapon demand / plant efficiency

installed plant mass    = max(1 t, ship generation demand * specificPower_tGW)
occupied reactor volume = drive demand * specificPower_tGW
                        * reported-mass bay fraction / installed density

effective drive capacity = min(maxOutput_GW, geometry-derived bay limit)
```

The drive path is not divided by power-plant efficiency. For a thermal drive,
an `8 GW` first-reactor rating can therefore be represented directly as
`maxOutput_GW: 8` in the current abstraction. Efficiency still affects hotel,
utility, and weapon generation as well as rejected heat.

Two consequences matter:

1. Increasing the cap makes a larger drive legal, but a lightly loaded plant
   remains light and small.
2. Increasing specific mass makes every installation heavier and larger, and
   also lowers the output that fits in a given graphical reactor bay.

## Why the current plants look small

The table uses current EEO values and asks how much of the default Titan's
`15,955.576 m3` bay a plant occupies **at its own full theoretical rating**.
Actual installations can be smaller still.

| Plant | Current cap | Current t/GW | Full-rating mass | Full-rating volume | Titan bay fill |
|---|---:|---:|---:|---:|---:|
| Solid Core I | 2 GW | 240 | 480 t | 96 m3 | 0.60% |
| Gas Core III | 150 GW | 10 | 1,500 t | 338 m3 | 2.12% |
| Terawatt Gas Core VI | 1,700 GW | 5 | 8,500 t | 1,913 m3 | 11.99% |
| Tokamak III | 624 GW | 1 | 624 t | 234 m3 | 1.47% |
| Tokamak V | 5,060 GW | 0.1 | 506 t | 190 m3 | 1.19% |
| Hybrid IV | 11,370 GW | 0.05 | 569 t | 213 m3 | 1.34% |
| Z-Pinch III | 2,510 GW | 1.4 | 3,514 t | 843 m3 | 5.29% |
| Flow-Stabilized Z-Pinch | 7,590 GW | 0.0068 | 52 t | 12 m3 | 0.08% |
| Inertial Fusion V | 19,090 GW | 0.25 | 4,773 t | 1,909 m3 | 11.96% |
| Inertial Fusion VII | 306,430 GW | 0.002 | 613 t | 245 m3 | 1.54% |

The largest discontinuities are especially revealing:

- Electrostatic II to III improves from `0.5` to `0.005 t/GW`: **100x**.
- Z-Pinch IV to Flow-Stabilized improves from `0.4` to `0.0068 t/GW`:
  **58.8x**.
- Inertial VI to VII improves from `0.068` to `0.002 t/GW`: **34x**.

Those jumps overwhelm the higher output. A more advanced full-rating reactor
often becomes physically smaller than its predecessor.

## Terawatt naming audit

The installed English localization names Gas Core IV-VI **Terawatt Gas Core
Fission Reactor I-III**. That gives a clean fission threshold: Gas Core III
should remain below `1,000 GW`, while Gas Core IV is the first fission plant
allowed to cross it.

The global **Terawatt Fusion Reactors** technology is currently not a clean
threshold. These plants exceed one terawatt without requiring it:

| Pre-terawatt plant | Current cap | Matched unscaled x6 drive demand |
|---|---:|---:|
| Tokamak IV | 1,260 GW | Helion Torus Lantern: 1,246 GW |
| Hybrid III | 1,900 GW | Helion Plasmajet Lantern: 1,895 GW |
| Z-Pinch III | 2,510 GW | Zeta Helion Lantern: 2,503 GW |
| Z-Pinch IV | 3,970 GW | Zeta Borane Lantern: 3,959 GW |
| Inertial III | 3,170 GW | Helion Nova Lantern: 3,162 GW |
| Inertial IV | 5,500 GW | Borane Nova Lantern: 5,485 GW |

Tokamak V is also rated at `5,060 GW` without a Terawatt Fusion Reactors
prerequisite. Its direct global gate is Proton-Proton Fusion. If it remains a
multi-terawatt plant, its project needs an explicit Terawatt Fusion Reactors
prerequisite.

The near equality between each plant cap and its x6 drive demand shows that the
vanilla cap curve was fitted to unscaled x6 drive templates. EEO now multiplies
drive demand by graphical hull/appearance scale, but the plant caps still use
the old unscaled values. That is why the late plants stop supporting even x1
drives on the largest hulls.

## Fission capacity ladder

This is a coherent rounded planning ladder, not implementation authority.

| Fission line | Current caps | Planning ladder |
|---|---:|---:|
| Solid Core I-V | 2 / 3 / 10 / 30 / 60 GW | **8 / 16 / 32 / 64 / 125 GW** |
| Compact Solid Core I-V | 0.75 / 2 / 4 / 6 / 10 GW | **4 / 8 / 16 / 32 / 64 GW** |
| Molten Core I-III | 4 / 17 / 200 GW | **16 / 64 / 300 GW** |
| Molten Salt I-II | 40 / 400 GW | **100 / 600 GW** |
| Vapor Core I-III | 6.5 / 20 / 60 GW | **32 / 128 / 500 GW** |
| Gas Core I-III | 8 / 33 / 150 GW | **64 / 256 / 900 GW** |
| Terawatt Gas Core I-III | 1.0 / 1.3 / 1.7 TW | **1.5 / 3.5 / 7 TW** |

This reverses most of the previous output halving without merely restoring the
old values. It supplies a regular capacity progression, uses the labels as a
real boundary, and makes reactor-bay geometry relevant:

- Solid Core I at `8 GW` is bay-limited to `5.51 GW` in a default Gunship,
  `6.92 GW` in a default Frigate, and almost exactly `8.01 GW` in a default
  Monitor/Destroyer. Full rating therefore becomes a physically larger-hull
  capability rather than something every starter hull can exploit.
- Gas Core III at `900 GW` is bay-limited to about `884 GW` in a default
  Cruiser and can reach full rating in larger bays. It is almost exactly the
  largest useful sub-terawatt value for that plant's current `10 t/GW`.
- Terawatt Gas Core VI at `7 TW` uses about half a default Titan bay. Its current
  geometry-derived Titan limit is `14.18 TW`.

## Fusion capacity ladder

The cleanest curve uses four bands instead of allowing every confinement branch
to establish an unrelated scale:

| Technology band | Capacity rule |
|---|---:|
| First D-T plants | approximately **100-400 GW** |
| D-D plants | approximately **400-800 GW** |
| D-He3/aneutronic plants before Terawatt Fusion Reactors | **900-950 GW maximum** |
| Plants that require Terawatt Fusion Reactors | multi-terawatt; sized against large-hull x1 demand |

A rounded branch-level planning ladder is:

| Fusion line | Planning caps |
|---|---:|
| Electrostatic I-III | **100 / 400 / 950 GW** |
| Mirror Cell I-III | **150 / 500 / 950 GW** |
| Tokamak I-V | **150 / 500 / 900 / 950 GW / 20 TW** |
| Hybrid I-IV | **200 / 600 / 950 GW / 40 TW** |
| Z-Pinch I-IV, Flow-Stabilized | **300 / 700 / 900 / 950 GW / 30 TW** |
| Inertial I-VII | **400 / 800 / 900 / 950 GW / 70 TW / 75 TW / 1.1 PW** |

Tokamak V should join the post-terawatt group by prerequisite. The first four
entries in the Z-Pinch and Inertial lines stay sub-terawatt even though several
are currently above the boundary.

The high post-terawatt endpoints are driven by EEO's graphical hull scaling,
not by a desire to make x6 Titan engines universal. A one-thruster installation
already becomes a much larger engine on a Titan:

| Top x1 drive | Base demand | Titan appearance 0 | Titan appearance 3 | Current matching plant cap | Planning cap |
|---|---:|---:|---:|---:|---:|
| Lodestar Fission Lantern | 187 GW | 1.51 TW | 4.71 TW | 1.7 TW | **7 TW** |
| Protium Torus Lantern | 842 GW | 5.55 TW | 17.63 TW | 5.06 TW | **20 TW** |
| Borane Plasmajet Torch | 1.89 TW | 12.49 TW | 39.67 TW | 11.37 TW | **40 TW** |
| Zeta Deuteron Torch | 1.26 TW | 8.33 TW | 26.46 TW | 7.59 TW | **30 TW** |
| Protium Converter Torch | 51.07 TW | 336.72 TW | 1.070 PW | 306.43 TW | **1.1 PW** |

The current top cap fails to support x1 on default Titan appearance 0 in every
fusion family in the table. The planning caps support x1 on every measured
Titan appearance. Supporting x6 everywhere would require another sixfold cap
increase and is not a necessary baseline because the appearance multiplier has
already enlarged each nominal thruster.

## Specific-mass companion pass

The capacity ladder should be paired with a slower specific-mass progression.
Before Terawatt Fusion Reactors, no fusion plant needs to fall below roughly
`2-3 t/GW`; first-generation plants can remain near `4-6 t/GW`. This keeps the
fusion transition competitive with late fission without introducing 34x-100x
single-step mass improvements.

For the proposed post-terawatt endpoints, the following values make a
full-rating installation consume roughly half of a default Titan bay:

| Plant | Planning cap | Current t/GW | Planning t/GW | Full-rating mass | Full-rating volume | Titan fill |
|---|---:|---:|---:|---:|---:|---:|
| Terawatt Gas Core VI | 7 TW | 5 | **5 retained** | 35,000 t | 7,875 m3 | 49% |
| Tokamak V | 20 TW | 0.1 | **about 1.0** | 20,000 t | 7,500 m3 | 47% |
| Hybrid IV | 40 TW | 0.05 | **about 0.5** | 20,000 t | 7,500 m3 | 47% |
| Flow-Stabilized Z-Pinch | 30 TW | 0.0068 | **about 1.0** | 30,000 t | 7,200 m3 | 45% |
| Inertial VII | 1.1 PW | 0.002 | **about 0.02** | 22,000 t | 8,800 m3 | 55% |

The Flow-Stabilized correction is the largest: raising only its cap would leave
a `30 TW` full-rating plant occupying about `49 m3`. Its specific mass needs a
large correction because the existing `0.0068 t/GW` value, not its cap, is the
primary reason it is tiny.

Inertial V and VI can use approximately `0.30` and `0.25 t/GW` with the proposed
`70/75 TW` caps. The top Inertial VII can then make its exceptional late-game
step to about `0.02 t/GW` without collapsing to the current `0.002 t/GW`.

## Recommended implementation boundary

A future gameplay pass should change these elements together:

1. apply the fission capacity ladder;
2. cap all pre-Terawatt-Fusion plants below `1 TW`;
3. add Terawatt Fusion Reactors to Tokamak V and audit the matched high-power
   drive projects so no stranded drive/reactor pair remains;
4. lift the post-terawatt caps far enough to support x1 on all measured Titan
   appearances;
5. replace the abrupt fusion specific-mass collapses with the companion ladder;
6. regenerate the merged power-plant snapshot and validate every scaled
   drive/plant/hull/appearance pairing before deployment.

The main balance choice still open is whether the target full-rating size on a
Titan should be approximately one-quarter, one-half, or three-quarters of its
reactor bay. The numbers above use **one-half** for the top plant in each line.
That is large enough to make the machinery visually and economically material
while preserving room for the bay model to distinguish Titan appearances and
to reject over-clustered drives.
