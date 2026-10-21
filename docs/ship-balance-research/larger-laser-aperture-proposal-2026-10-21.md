# Larger laser apertures: art correction and deferred balance options

2026-10-21. **Proposal only. No implementation, build or deployment.**

Larger simulation apertures would improve armor penetration, often substantially,
without increasing raw shot energy or cooldown-based DPS. Larger **meshes alone**
would not. The [dimensional audit](barrel-and-lens-dimensional-audit-2026-10-21.md)
shows substantial room to improve visual progression, but also shows that most
existing laser art is already smaller than its current simulation aperture.
Making the visuals faithful to existing stats is therefore a separate decision
from giving lasers larger optics in the combat calculation.

## Recommended starting point

First choose and document a consistent visual scale policy, resolve whether
names should explicitly say radius or diameter, and improve family progression.
Preserve combat optics during that art pass. For a subsequent balance experiment,
**+25% aperture diameter/radius** is a useful first candidate. It gives meaningful
penetration improvements without jumping immediately to the much larger effects
of doubling diameter. This is a proposed test point, not a finding that it is
balanced. Do not apply it indiscriminately to PD or alien Xasers/Grasers.

| Option | What changes | Combat effect | Main consequence |
|---|---|---|---|
| A: art matches current simulation D | Cap, housing and possibly mount geometry | None from optics | Large fitting changes, especially alien noses |
| B: coherent compressed art | Keep an explicit constant art/simulation ratio within a family | None | Stronger tier readability without literal metre matching |
| C: enlarge existing simulation optics | `mirrorRadius_cm`, matching label and art policy | Better focus and penetration; unchanged raw energy/DPS | Broad rebalance of all affected templates and existing designs |
| D: new larger-aperture weapon variants | New templates, unlocks, art, fitting/cost choices | Same optical benefit if power is held fixed | More content and save/design/AI integration work |
| E: reinterpret existing names as diameter | Rename only, **or** halve radius fields | Rename-only: none; halve fields: substantial focus nerf | Must explicitly choose which operation is intended |

Option B best targets the user's priority of coherent family ratios with
manageable art scope. A physically literal Option A is also possible, but
requires a renewed hull and nose fitting study. Neither automatically authorizes C.

## Exact relationship checked against the current binary

The current `TILaserWeaponTemplate` uses:

```text
D = 2 * mirrorRadius_cm / 100                   [metres]
d = range_m * sqrt((1.22 * wavelength_m * Q / D)^2 + (2*jitter_rad)^2)
S = pi * d^2 / 4
E = S / 0.005
effective intact armor = armor^1.5 * E
intercepted shot fraction = min(1, target_area / S)
chipping fraction = clamp(E / 1000, 0, 0.25)
```

Lower `E` means less armor absorption. With fixed wavelength, quality and jitter,
let `F = (1.22*lambda*Q/D)^2` and `J = (2*j)^2`. Scaling aperture by `s` gives
`E_new / E_old = (F/s^2 + J)/(F + J)`. This ratio is independent of range;
absolute absorption still grows with range squared. If diffraction dominates,
+25% / +50% / +100% diameter gives about **-36% / -55.6% / -75%** absorption.
It does not mean those percentages more DPS or that percentage chance to pierce.

For a fully intercepted shot, no bonuses/resistance and intact normal-incidence
armor, nominal penetrating damage is `max(0, P*(1-c) - A^1.5*E)`, where
`P = shotPower_MJ/20`. The nominal armor threshold is
`(P*(1-c)/E)^(2/3)`. Damage is randomized, chipped armor can be bypassed, and
officer/faction/material/obliquity effects alter real results. These calculations
are controlled comparisons, not a combat simulation. Freshly checked formulas
and hashes are recorded in the [reproduction evidence](barrel-lens-audit/reproduction.md).
The [older mechanics explanation](laser-armor-penetration-and-jitter.md) provides
background but was not used as an unverified substitute for the current binary.

## Numerical aperture sensitivity

At 500 km, all listed weapons are within their attack targeting ranges. Keep
shot energy, cooldown, wavelength, beam quality, jitter and range unchanged.
Here `E` is an armor absorption multiplier, not a probability.

| Weapon | Current E | Jitter share of E | E delta +25% D | E delta +50% D | E delta +100% D |
|---|---:|---:|---:|---:|---:|
| 60 cm IR phaser battery | 32.616 | 1.20% | -35.57% | -54.89% | -74.10% |
| 60 cm green phaser battery | 14.714 | 2.67% | -35.04% | -54.07% | -73.00% |
| 60 cm UV phaser battery | 3.973 | 9.88% | -32.44% | -50.06% | -67.59% |
| 120 cm green phaser battery | 3.832 | 6.56% | -33.64% | -51.91% | -70.08% |
| 360 cm green phaser battery | 0.559 | 28.79% | -25.64% | -39.56% | -53.41% |
| 240 cm green phaser cannon | 1.001 | 10.60% | -32.18% | -49.66% | -67.05% |
| 480 cm green phaser cannon | 0.287 | 21.92% | -28.11% | -43.38% | -58.56% |
| 720 cm green phaser cannon | 0.140 | 28.79% | -25.64% | -39.56% | -53.41% |
| 960 cm green phaser cannon | 0.082 | 32.18% | -24.41% | -37.68% | -50.86% |
| Alien 384 cm Xaser battery | 0.161 | 99.93% | -0.02% | -0.04% | -0.05% |

The [full sensitivity tables](barrel-lens-audit/laser-sensitivity.md) include
all 44 weapon/scale scenarios, absolute apertures, spot diameters, raw DPS,
nominal armor thresholds and penetrating DPS against 10 armor. Machine-readable
inputs/results are in [laser-sensitivity.json](barrel-lens-audit/laser-sensitivity.json).

### Why the gameplay change can be disproportionate

| Green phaser | Aperture scale | Nominal penetrable armor threshold at 500 km | Penetrating DPS vs intact 10 armor | Raw DPS |
|---|---:|---:|---:|---:|
| 360 cm battery | 1.00 | 7.94 | 0.000 | 1.250 |
| Same | 1.25 | 9.67 | 0.000 | 1.250 |
| Same | 1.50 | 11.11 | 0.182 | 1.250 |
| Same | 2.00 | 13.21 | 0.427 | 1.250 |
| 480 cm cannon | 1.00 | 13.99 | 0.593 | 1.500 |
| Same | 1.25 | 17.43 | 0.848 | 1.500 |
| Same | 1.50 | 20.44 | 0.987 | 1.500 |
| Same | 2.00 | 25.17 | 1.124 | 1.500 |

DPS here is native damage points/s; multiply by 20 for the corresponding MJ/s
damage-equivalent. A 25% aperture increase gives the 480 cm cannon about **43%**
more nominal penetrating DPS in this example, despite unchanged raw DPS. The
360 cm battery crosses a nominal penetration threshold between 1.25× and 1.5×.
Random damage means nominal zero is not a guarantee of zero actual penetration.
At lower armor, benefits shrink as damage approaches the same energy ceiling.

Xasers are already nearly entirely jitter-limited, so larger apertures barely
help. Using the authored 0.1 nm value, the sampled alien 1024 cm Graser is still
more extreme: doubling D reduces E by less than 0.00001%. The native wavelength
field is an integer; loaded rounding was not observed. If it becomes zero,
aperture growth gives exactly zero benefit. Improving jitter is a separate, potentially much
more consequential proposal. Larger lenses should not automatically receive
better jitter, more range or more shot energy as an unexamined bundle.

PD also benefits when a projectile intercepts only part of the spot: the same
shot concentrates more energy onto a small target. It is not merely a ship-armor
buff. EEO's small projectile cross-sections and accumulated naval-shell durability
make this relevant. Test PD saturation and time-to-kill separately from ship DPS.
Targeting range caps, traverse and cooldowns do not automatically change with D.

## Concrete candidate dimensions

The values below retain the present naming convention: the displayed number
matches radius in centimetres. Prefer explicit radius/diameter wording in any
new descriptions to remove that ambiguity.

| Existing label | Current radius cm | +25% radius cm | +50% radius cm | Current D m | +25% D m | +50% D m |
|---|---:|---:|---:|---:|---:|---:|
| Hull 60 | 60 | 75 | 90 | 1.2 | 1.5 | 1.8 |
| Hull 120 | 120 | 150 | 180 | 2.4 | 3.0 | 3.6 |
| Hull 360 | 360 | 450 | 540 | 7.2 | 9.0 | 10.8 |
| Nose 240 | 240 | 300 | 360 | 4.8 | 6.0 | 7.2 |
| Nose 480 | 480 | 600 | 720 | 9.6 | 12.0 | 14.4 |
| Nose 720 | 720 | 900 | 1080 | 14.4 | 18.0 | 21.6 |
| Nose 960 | 960 | 1200 | 1440 | 19.2 | 24.0 | 28.8 |

These are candidate values, not new implemented weapons. A distinct large-lens
variant could keep shot energy/cadence and slot count while paying additional
mass/material cost, or occupy more slots. Either choice needs deliberate AI
scoring and progression review. Same slots and cost with better penetration
would usually dominate the original at ship attack, with possible differences
in chipping and any deliberately retained tradeoffs. More lens area does not
by itself require more beam generation energy if shot power stays fixed.

## Art and fitting fallout

| Human art | Current visible aperture m | Art factor to current simulation | Art factor to +25% simulation | Art factor to +50% simulation |
|---|---:|---:|---:|---:|
| Small hull | 1.069 | 1.123× | 1.403× | 1.684× |
| Medium hull | 1.639 | 1.464× | 1.831× | 2.197× |
| Large hull | 2.608 | 2.760× | 3.450× | 4.140× |
| Small nose | 1.591 | 3.017× | 3.771× | 4.525× |
| Medium nose | 2.683 | 3.578× | 4.473× | 5.367× |
| Large nose | 4.138 | 3.480× | 4.350× | 5.220× |
| Spinal nose | 4.984 | 3.852× | 4.815× | 5.778× |

Scaling an existing visual by 1.25× while raising the simulation aperture by
1.25× preserves its current dimensional error. Matching the simulation requires
the larger compound factors above. Enlarging just the cap may intersect its
own gimbal/housing; enlarging the whole resource also expands the stationary
base and swept envelope. Those are different implementation choices.

If using compressed art instead, anchor a family to one agreed size. Holding
the present small human hull aperture at 1.0688 m gives medium/large targets
of **2.1377 / 6.4130 m** for exact 1:2:6 ratios. These require **1.304× / 2.459×**
their current apertures even without literal simulation-metre matching.
Holding the small nose at 1.5912 m gives **3.1825 / 4.7737 / 6.3649 m** for
the other sizes, requiring **1.186× / 1.154× / 1.277×**. Nose ratio correction
is therefore much more modest than literal nose aperture matching.

Alien literal matching is a much larger redesign: heavy nose art would need
roughly 14.9× its current opening diameter before adding any new optical buff.
Its multi-lens batteries also require an explicit array interpretation. Do not
reuse the old turret-clearance tables as proof these enlarged shapes fit.

## Scope and acceptance criteria for a future implementation

| Workstream | Existing weapons enlarged | New variants added |
|---|---|---|
| Data | Radius changes across selected wavelength/generation records | New IDs, radius, costs, slot/mass choices, project/tech links |
| Art | Shared-resource scale or selective mesh edits; preserve optical centers | New/reused resources with per-variant scale policy |
| Descriptions | Correct aperture terminology and stats | New names, localization, icons and unlock text |
| Saves/designs | Existing template IDs change behavior for existing ships | Preserve old IDs; new construction/refit paths and stock designs |
| AI | Re-evaluate focus/damage valuation and upgrade choices | Additional selection/obsolescence rules and role differentiation |
| Geometry | All selected hull and nose variants, mounts, preview/combat scales | Same checks plus new slot-footprint legality |
| Combat | Ship armor thresholds, chipping, PD, faction asymmetry | Same checks plus whether originals retain a useful role |

Keep the first experiment narrow: one human hull family and one nose family,
hold energy/cadence/jitter/range fixed, measure both +25% and +50% optics, and
compare several armor/range cases before generalizing. Check ordinary/arc/phaser
and IR/green/UV separately. A result on green phasers does not establish the
balance of every laser generation.

Implementation would follow the repository's normal documented build/deploy/
validation workflow, with the game closed as asserted by deployment tooling.
Manual testing must confirm normal and elevated aim, base/barbette contact,
symmetry, VFX emergence, preview/combat consistency and old-save behavior.
The current task leaves all of these changes deferred.
