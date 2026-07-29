# Early human hull geometry, mass, crew, power, and volume audit

Last reviewed: 2026-07-29  
Game data: Terra Invicta 1.0.49 installed templates, compiled assembly, and
base Earth ship prefabs

This is a planning and research note. It does not apply balance changes. This
file is the authoritative ship-type table for the Gunship, Escort, Corvette,
Frigate, Monitor, and Destroyer; their entries are not duplicated in other
first-slice tables.

## Short answers

1. **The Gunship's base power is a hard-coded tier charge.** It is not derived
   from hull volume, mass, installed systems, or an engineering equipment list.
2. **The game does not model module volume.** Slots are categorical permissions,
   not equal physical volumes.
3. **The Gunship is large enough for its nominal slot count.** The installed
   50 m × 10 m template was already adequate; the settled 55 m × 15 m envelope
   is deliberately very generous for three crew. Whether a particular design
   fits still depends on the actual reactor, heat sink, weapon, and propellant
   volumes that the game currently ignores.
4. **A compact reactor core does not mean a compact power plant.** Shielding,
   heat transport, conversion machinery, radiators, controls, structure, and
   maintenance access usually outweigh and out-volume the fuel.
5. **The 1–1.6 GWe terrestrial cluster is not a coolant-pump hard limit.**
   Thermal hydraulics constrain core power density, but modern unit size is
   heavily shaped by economics, grid size, standard designs, manufacturable
   vessels, safety systems, and outage risk.
6. **The Escort's current mass premium is much larger than its geometry or slot
   change alone supports.** Its higher structural-integrity statistic can
   justify a meaningful premium, but treating that statistic as linearly
   proportional to hull mass would be a gameplay convention rather than an
   engineering law.
7. **The four larger hull masses are not random outliers.** Corvette, Frigate,
   and Monitor are exactly 50 t per point of structural integrity; Destroyer is
   45.8 t/SI. Their clearest outliers are crew growth and, for Monitor and
   Destroyer, statistical length that substantially exceeds the rendered hull.
8. **The Frigate has a likely prefab collision defect.** An active layer-17
   radiator box extends well behind the visible ship and default drive. It
   makes the authored ballistic envelope 143.5 m long even though the rendered
   assembly is only 102.7 m. This should be checked in live combat before any
   statistical geometry is matched to it.

## Consolidated installed six-hull ship-type table

All six have one drive, one power plant, and one radiator slot. The JSON
`volume` values are stored but ignored by the compiled hull class; the runtime
cylinder is calculated from length and width.

| Hull | Tier | Template L × D | Stored volume | Runtime cylinder | Hull mass | Crew | Empty at current 4 t/crew | Empty at settled 3 t/crew | SI | Nose / hull hardpoints | Utilities |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Gunship | 1 | 50 × 10 m | 1,963 m³ | 3,927 m³ | 178 t | 3 | 190 t | 187 t | 4 | 1 / 0 | 2 |
| Escort | 1 | 50 × 10 m | 1,963 m³ | 3,927 m³ | 350 t | 4 | 366 t | 362 t | 7 | 0 / 2 | 2 |
| Corvette | 1 | 65 × 15 m | 7,069 m³ | 11,486 m³ | 400 t | 8 | 432 t | 424 t | 8 | 1 / 1 | 3 |
| Frigate | 1 | 100 × 20 m | 23,562 m³ | 31,416 m³ | 600 t | 20 | 680 t | 660 t | 12 | 1 / 2 | 5 |
| Monitor | 2 | 125 × 20 m | 31,416 m³ | 39,270 m³ | 800 t | 35 | 940 t | 905 t | 16 | 0 / 4 | 3 |
| Destroyer | 2 | 125 × 20 m | 31,416 m³ | 39,270 m³ | 825 t | 40 | 985 t | 945 t | 18 | 2 / 2 | 5 |

In the installed statistical template the Escort occupies the same nominal
envelope and stored volume while its bare hull is almost twice as massive.
It is not quite true that the weapon-slot exchange and fourth crew member are
the only differences, however: the Escort also has `structuralIntegrity: 7`
against the Gunship's **4**, and its authored prefab is visibly broader.

### Settled rebalance dimensions, crew, and masses

The rebalance adopts rounded dimensions from the rendered hull-plus-default-
drive envelopes. In the hull template, `width_m` is the diameter because
runtime volume is:

`volume = π × (width_m / 2)² × length_m`

| Hull | Settled L × D | Cylinder / planning `volume` | Crew | SI | Settled hull mass | Hull mass/SI | Empty mass at 3 t/crew | Change from current empty mass |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Gunship | **55 × 15 m** | **9,719 m³** | **3** | 4 | **171 t** | 42.8 t/SI | **180 t** | −10 t |
| Escort | **62 × 15 m** | **10,956 m³** | **4** | 7 | **338 t** | 48.3 t/SI | **350 t** | −16 t |
| Corvette | **65 × 17 m** | **14,754 m³** | **5** | 8 | **385 t** | 48.1 t/SI | **400 t** | −32 t |
| Frigate | **100 × 18 m** | **25,447 m³** | **8** | 12 | **576 t** | 48.0 t/SI | **600 t** | −80 t |
| Monitor | **100 × 17 m** | **22,698 m³** | **7** | 16 | **679 t** | 42.4 t/SI | **700 t** | −240 t |
| Destroyer | **100 × 23 m** | **41,548 m³** | **9** | 18 | **873 t** | 48.5 t/SI | **900 t** | −85 t |

The JSON `volume` key is currently ignored by the compiled hull class, but
matching it to the calculated cylinder removes the contradictory stored value
and makes the planning data self-consistent. These are geometric envelope
volumes, not a claim that every cubic metre is usable internal space.

The previously recorded 171 t Gunship and 338 t Escort values are **hull
masses**, not empty masses. After adding the settled crew allowance, their empty
masses are 180 t and 350 t. The Escort has **1,237 m³ (12.7%)** more cylindrical
volume and remains **170 t (94.4%)** heavier when empty; most of that retained
premium still represents its higher structural-integrity tier.

### Why the installed hull masses follow structural integrity

The installed human hull progression reveals a nearly explicit balancing rule:
**about 50 tonnes of hull mass per point of structural integrity**. Eight of
the twelve ordinary human hulls land exactly on that ratio, and the fleet
median is 50 t/SI.

| Hull | Installed hull mass | Structural integrity | Mass per SI |
|---|---:|---:|---:|
| Gunship | 178 t | 4 | 44.5 t/SI |
| Escort | 350 t | 7 | **50.0 t/SI** |
| Corvette | 400 t | 8 | **50.0 t/SI** |
| Frigate | 600 t | 12 | **50.0 t/SI** |
| Monitor | 800 t | 16 | **50.0 t/SI** |
| Destroyer | 825 t | 18 | 45.8 t/SI |
| Human-hull fleet median | — | — | **50.0 t/SI** |

The current 172 t hull difference can therefore be reconstructed almost
entirely as a progression formula:

`3 additional SI × 50 t/SI = 150 t`

plus the Gunship sitting 22 t below the 200 t that a strict 50 t/SI rule would
assign it. That explains the data author's number. It does **not** make 50
t/SI a physical law, and it means the mass difference is principally buying
abstract durability rather than the second weapon interface.

The same observation changes the diagnosis for the four larger hulls. Their
mass is aggressive as physical structure, but it is internally systematic.
Reducing mass without deciding what SI represents would sever the game's
primary durability-cost rule. Crew, dimensions, and collider consistency can
be audited independently before that decision.

## Authored visual size and ballistic raycast geometry

The following measurements come from active meshes and layer-17 colliders in
the six installed base Earth prefabs. They are local prefab metres before the
combat visualizer's common `0.01` scale. The axes are `x width × y height × z
length`; the table reorders them as **length × width × height**.

| Hull | Template L × D | Bare rendered AABB | Rendered with default drive | Bare raycast AABB | Raycast with default drive |
|---|---:|---:|---:|---:|---:|
| Gunship | 50 × 10 m | 45.110 × 14.216 × 30.194 m | 55.250 × 14.216 × 30.194 m | 41.500 × 8.420 × 13.876 m | 55.974 × 8.420 × 13.876 m |
| Escort | 50 × 10 m | 51.766 × 14.940 × 14.516 m | 61.906 × 14.940 × 14.516 m | 51.956 × 12.693 × 10.327 m | 62.096 × 12.693 × 10.327 m |
| Corvette | 65 × 15 m | 55.093 × 16.797 × 9.008 m | 65.233 × 16.797 × 9.008 m | 51.236 × 12.649 × 8.593 m | 65.585 × 12.649 × 8.593 m |
| Frigate | 100 × 20 m | 80.423 × 17.760 × 15.056 m | 102.743 × 17.760 × 16.152 m | **143.523 × 17.760 × 17.227 m** | **143.523 × 17.760 × 17.227 m** |
| Monitor | 125 × 20 m | 89.381 × 16.567 × 18.409 m | 99.357 × 16.567 × 18.409 m | 90.358 × 16.800 × 16.800 m | 98.790 × 16.800 × 16.800 m |
| Destroyer | 125 × 20 m | 88.433 × 23.306 × 17.049 m | 98.408 × 23.306 × 17.049 m | 89.126 × 13.400 × 12.922 m | 99.113 × 13.400 × 12.922 m |

For exact placement in prefab coordinates:

| Bare-hull raycast envelope | Minimum x, y, z | Maximum x, y, z |
|---|---:|---:|
| Gunship | −4.210, −6.938, −16.000 m | +4.210, +6.938, +25.500 m |
| Escort | −6.346, −5.164, −20.334 m | +6.346, +5.164, +31.623 m |
| Corvette | −6.325, −4.296, −16.125 m | +6.325, +4.296, +35.111 m |
| Frigate | −8.544, −7.689, **−92.900 m** | +9.216, +9.538, +50.623 m |
| Monitor | −8.400, −8.400, −54.051 m | +8.400, +8.400, +36.307 m |
| Destroyer | −6.700, −6.461, −52.496 m | +6.700, +6.461, +36.630 m |

Gunship, Escort, and Corvette use the same 8.165 m-wide default-drive
collider. Monitor and Destroyer use an equally wide drive farther aft. The
Frigate's default-drive assembly is larger at 13.035 × 14.331 × 24.664 m.
Multiplying any dimensions above by `0.01` gives combat scene units; the common
scale does not change relative sizes or mesh/collider alignment.

The Gunship's 30.194 m rendered height is misleading if read as solid hull. It
comes from the non-colliding `Earth_Gunship_Bridge_Detail` mesh, which contains
long decorative projections. Its ballistic envelope is much narrower. Weapons,
radiators, and replacement drives are modular children, so a completed design's
overall bounds depend on the installed equipment.

The Destroyer's 23.306 m rendered width is likewise decorative: its widest
ballistic collider is only 13.4 m. Conversely, the Frigate's extreme raycast
length is not visible geometry. Its `Earth_Frigate_Radiators` box is 35 m long
and sits at `z = −92.9` to `−57.9 m`, behind a default drive ending at
`z = −52.1 m`. The radiator mesh itself is near the hull rather than the
collider. Unless runtime code relocates or disables that box, shots can hit an
invisible region behind the ship. Treat this as a probable prefab defect, not
evidence that the Frigate should be 144 m long.

The actual bare-hull raycast primitives are:

| Hull | Collider child | Shape | Local size, width × height × length |
|---|---|---|---:|
| Gunship | Bridge | box | 7.874 × 13.876 × 22.857 m |
| Gunship | Crew section | capsule | 8.420 × 8.420 × 14.000 m |
| Gunship | Radiator root | box | 6.050 × 5.164 × 8.000 m |
| Escort | Crew section | box | 8.593 × 10.327 × 25.660 m |
| Escort | Head | capsule | 8.000 × 8.000 × 10.000 m |
| Escort | Radiator root | box | 6.050 × 5.164 × 12.617 m |
| Escort | Starboard wing | box | 1.571 × 5.322 × 32.401 m |
| Escort | Port wing | box | 1.571 × 5.322 × 32.401 m |
| Corvette | Bridge | box | 12.649 × 8.593 × 25.660 m |
| Corvette | Crew section | capsule | 8.400 × 8.400 × 18.000 m |
| Corvette | Radiator root | box | 6.050 × 5.164 × 8.250 m |
| Frigate | Propellant tanks | capsule | 15.796 × 15.796 × 17.924 m |
| Frigate | Crew section | box | 17.760 × 12.112 × 29.365 m |
| Frigate | Bridge | capsule | 13.941 × 13.941 × 11.419 m |
| Frigate | Radiator box, likely misplaced | box | 15.475 × 13.209 × 35.000 m |
| Frigate | Nose | capsule | 8.103 × 8.103 × 7.294 m |
| Monitor | Bridge | capsule | 12.400 × 12.400 × 17.844 m |
| Monitor | Crew detail | capsule | 16.060 × 16.060 × 32.380 m |
| Monitor | Crew section | capsule | 16.320 × 16.320 × 32.400 m |
| Monitor | Radiator root | capsule | 6.620 × 6.620 × 21.450 m |
| Monitor | Rear | box | 8.091 × 13.660 × 27.229 m |
| Monitor | Tanks | capsule | 16.800 × 16.800 × 19.690 m |
| Destroyer | Rear | box | 13.400 × 10.049 × 15.518 m |
| Destroyer | Bridge | capsule | 11.800 × 11.800 × 17.840 m |
| Destroyer | Crew section | box | 12.112 × 12.922 × 29.365 m |
| Destroyer | Radiator root | capsule | 5.200 × 5.200 × 18.340 m |

The prefabs also contain a very large root capsule, but it is on Unity layer 2
(`Ignore Raycast`) and is not the ballistic hit geometry. The listed layer-17
children are the colliders reached by combat raycasts. Summing their primitive
volumes—only a coarse check because they overlap—gives about 3,371 m³ for the
Gunship and 3,582 m³ for the Escort, just **6.3% more**, not 97% more. The
Frigate's detached radiator box makes the same calculation meaningless until
the suspected collider defect is resolved.

Template geometry and prefab geometry are separate systems. Template
`length_m` and `width_m` drive armor, maneuver, formation, cross-section, and
predictive-threat calculations. The rendered mesh and actual impact raycasts
come from the prefab. Changing one set of numbers does not reshape the other.

## Objective accounting for the Escort mass premium

There is no physical conversion from “one hardpoint” or one point of
`structuralIntegrity` to tonnes in the data, so a single exact answer would be
false precision. The defensible incremental accounting is:

| Increment over Gunship | Plausible mass | Reasoning |
|---|---:|---|
| One additional crew allowance | **+3 t settled** | The ship-wide planning rule already bundles the person, supplies, and a share of support systems |
| Net one additional weapon interface | **+10–20 t hull** | Two lateral foundations replace one axial foundation; weapon, ammunition, loader, and most mount machinery remain charged to the weapon module |
| Broader wings, distribution, access, and local reinforcement | **+10–20 t hull** | The Escort prefab adds two long side structures and a longer collider envelope |
| Greater global robustness, if SI 7 is retained | **+20–40 t hull** | Allows redundancy, stronger frames, compartmentation, and damage tolerance without pretending SI scales linearly with metal mass |
| **Defensible total premium** | **+40–80 t including crew** | Rounded balance range, not a measured design |

Applied to the installed 178 t Gunship hull, that accounting implies roughly
**218–253 t** for the Escort hull, with **238 t** as the same central +60 t
comparison. With settled crew mass, the central empty ships would be 187 t and
250 t.

The selected balance values retain substantially more of the game's integrity
premium:

| Settled comparison | Gunship | Escort | Premium |
|---|---:|---:|---:|
| Hull mass | **171 t** | **338 t** | **+167 t** |
| Crew mass | 9 t | 12 t | +3 t |
| Empty mass | **180 t** | **350 t** | **+170 t (+94.4%)** |
| Hull mass per SI | 42.8 t/SI | 48.3 t/SI | — |

This is more generous to Escort durability than the physical incremental
estimate. It is nevertheless internally consistent with the game's progression:
the selected hulls remain close to the approximately 50 t/SI family rule while
reducing both empty masses. The retained 170 t empty-mass gap should therefore
be read primarily as the price of SI 7 versus SI 4, not as the mass of one
additional hardpoint and one crew member.

## Corvette through Destroyer: settled rebalance comparison

The installed progression below explains the original values; the subsequent
tables record the settled replacements.

### Progression between installed hulls

| Transition | Runtime cylinder | Hull mass | Crew | SI | Weapon hardpoints | Utilities | Main observation |
|---|---:|---:|---:|---:|---:|---:|---|
| Escort → Corvette | 3,927 → 11,486 m³ (+193%) | 350 → 400 t (+14%) | 4 → 8 (+100%) | 7 → 8 | 2 → 2 | 2 → 3 | Much larger envelope for a modest hull-mass increase; crew doubles |
| Corvette → Frigate | 11,486 → 31,416 m³ (+174%) | 400 → 600 t (+50%) | 8 → 20 (+150%) | 8 → 12 | 2 → 3 | 3 → 5 | Volume grows faster than hull mass, but crew grows faster than both mass and slots |
| Frigate → Monitor | 31,416 → 39,270 m³ (+25%) | 600 → 800 t (+33%) | 20 → 35 (+75%) | 12 → 16 | 3 → 4 | 5 → 3 | The strongest crew discontinuity: two utilities are lost while fifteen crew are added |
| Monitor → Destroyer | unchanged | 800 → 825 t (+3%) | 35 → 40 (+14%) | 16 → 18 | 4 → 4 | 3 → 5 | Same template geometry; Destroyer buys two utilities and two SI for little hull mass but five crew |

The installed hull-mass progression is mostly the 50 t/SI rule. The most
obvious independent problem was crew: it accelerated from one person per
weapon/utility slot on the small hulls to roughly four or five people per such
slot. The settled calls reduce that discontinuity.

### Crew and empty-mass comparison

| Hull | Installed crew | Settled crew | Weapon + utility slots | Settled crew per such slot | Settled hull mass | Settled empty mass | Change from current empty mass |
|---|---:|---:|---:|---:|---:|---:|---:|
| Gunship | 3 | **3** | 3 | 1.0 | **171 t** | **180 t** | −10 t |
| Escort | 4 | **4** | 4 | 1.0 | **338 t** | **350 t** | −16 t |
| Corvette | 8 | **5** | 5 | 1.0 | **385 t** | **400 t** | −32 t |
| Frigate | 20 | **8** | 8 | 1.0 | **576 t** | **600 t** | −80 t |
| Monitor | 35 | **7** | 7 | 1.0 | **679 t** | **700 t** | −240 t |
| Destroyer | 40 | **9** | 9 | 1.0 | **873 t** | **900 t** | −85 t |

Slot count is not a complete staffing model: larger ships need more
watchstanders, distributed maintenance, medical coverage, damage control, and
redundancy. It is nevertheless difficult to justify the sharp increase as
literal module operators when drives, reactors, fire control, and point defense
are substantially automated. Fixed command and engineering functions should
grow sublinearly with hull volume, while damage-control staffing should track
compartments and survivability rather than weapon count.

The settled sequence regularizes all six hulls at **one base crew member per
weapon or utility slot**. This is a balance rule, not a claim that each person
operates one module. The complement is a shared command, engineering,
maintenance, logistics, medical, and damage-control pool; individual weapons
and local control loops remain automated.

### Statistical geometry versus the rendered ships

| Hull | Installed L × D | Rendered hull + drive L × W × H | Geometry finding | Settled L × D |
|---|---:|---:|---|---:|
| Corvette | 65 × 15 m | 65.233 × 16.797 × 9.008 m | Length already matches; hull is wide and vertically shallow | **65 × 17 m** |
| Frigate | 100 × 20 m | 102.743 × 17.760 × 16.152 m | Installed length is already close; ignore the 143.5 m anomalous collider | **100 × 18 m** |
| Monitor | 125 × 20 m | 99.357 × 16.567 × 18.409 m | Installed template overstates rendered length by about 26% | **100 × 17 m** |
| Destroyer | 125 × 20 m | 98.408 × 23.306 × 17.049 m | Installed template overstates length by about 27%; settled diameter follows the complete visual span | **100 × 23 m** |

Using the runtime cylinder formula, the settled planning volumes are:

| Hull | Settled geometry | Cylinder / planning volume |
|---|---:|---:|
| Corvette | 65 × 17 m | 14,754 m³ |
| Frigate | 100 × 18 m | 25,447 m³ |
| Monitor | 100 × 17 m | 22,698 m³ |
| Destroyer | 100 × 23 m | 41,548 m³ |

The Destroyer decision intentionally follows its 23.3 m complete visual span
even though the ballistic envelope is only 13.4 m wide. This preserves the
selected statistical size, but it also leaves predictive geometry substantially
wider than the current raycast hitbox unless the collider is revised later.

### Settled mass pattern

| Hull | Installed hull | Settled hull | Hull change | SI | Settled t/SI | Crew change | Empty-mass change |
|---|---:|---:|---:|---:|---:|---:|---:|
| Corvette | 400 t | **385 t** | −15 t | 8 | 48.1 | 8 → **5** | 432 → **400 t** |
| Frigate | 600 t | **576 t** | −24 t | 12 | 48.0 | 20 → **8** | 680 → **600 t** |
| Monitor | 800 t | **679 t** | −121 t | 16 | 42.4 | 35 → **7** | 940 → **700 t** |
| Destroyer | 825 t | **873 t** | **+48 t** | 18 | 48.5 | 40 → **9** | 985 → **900 t** |

Corvette, Frigate, and Destroyer converge near **48 t/SI**. Monitor is the
deliberately lighter exception at 42.4 t/SI, close to the settled Gunship's
42.8 t/SI. Destroyer is also the only hull whose structural mass increases;
its much smaller crew complement still reduces total empty mass by 85 t.

### Implementation cautions

1. Setting Monitor and Destroyer to exactly **100 m** moves them from the
   runtime's medium-hull category (`>100 m` and `<200 m`) into the small category
   (`≤100 m`). Audit every size-category branch before applying the templates.
2. Verify the Frigate radiator collider in live combat and fix it independently
   of the settled 100 m length if it remains active.
3. Destroyer's settled 23 m statistical diameter is substantially wider than
   its 13.4 m raycast envelope. Decide whether predictive and ballistic geometry
   should remain intentionally different.
4. The JSON `volume` key remains ignored by the compiled hull class. The new
   volumes are authoritative planning values unless a runtime volume consumer
   is added.

## Where the Gunship's base power comes from

The compiled method
`TISpaceShipTemplate.get_requiredSystemsPower_GW()` calculates:

`systems power = 1.10 × (crew billets × 0.000005 GW`
`                         + hull construction tier × 0.005 GW`
`                         + installed utility loads)`

This means:

- **5 kW per crew billet**;
- **5 MW per construction tier**;
- the explicit `powerRequirement_MW` of each installed utility module;
- a final **10% design margin**.

Weapons and drives are calculated separately. The base term is not a sum of
life support, sensors, pumps, avionics, maneuvering equipment, or hotel loads.

For a bare Gunship:

`1.10 × (3 × 5 kW + 1 × 5 MW) = 5.5165 MW`

The 5 MW tier term contributes 5.5 MW after margin. Crew contributes only
16.5 kW after margin. In other words, almost all the displayed bare-hull load
is an unexplained progression constant.

The tier term is also independent of hull class:

| Construction tier | Human hulls | Hard-coded base before margin |
|---:|---|---:|
| 1 | Gunship, Escort, Corvette, Frigate | 5 MW |
| 2 | Monitor, Destroyer, Cruiser, Battlecruiser | 10 MW |
| 3 | Battleship, Lancer, Dreadnought, Titan | 15 MW |

A 178 t Gunship and a 600 t Frigate therefore receive the same base power. A
3,200 t Titan receives only three times the Gunship's base term. This is a
technology/construction-tier progression rule, not a physical hotel-load
model.

### Planning interpretation

Five megawatts is generous but not absurd for an operating warship if it
bundles active sensors, communications, thermal pumps, flight computers,
attitude control, internal environmental systems, and combat readiness.
However, those systems are not actually enumerated by the code. The value
should not be used as evidence that a three-person empty hull intrinsically
needs 5 MW.

## What “volume” means in the game

The Gunship JSON contains:

- `length_m: 50`
- `width_m: 10`
- `volume: 1963`

The compiled `TIShipHullTemplate` class has no field corresponding to the JSON
`volume` key. Its `volume_m3` property instead calculates a perfect cylinder:

`π × (width / 2)² × length`

For the Gunship:

`π × 5² × 50 = 3,927 m³`

The configured `1963` happens to be almost exactly half of that result, but it
is not read by the compiled hull class. Runtime geometric uses such as hull
comparison and damage geometry call the 3,927 m³ cylinder property.

More importantly, none of the following template classes contains a module
volume field:

- power plants;
- drives;
- radiators;
- utility modules;
- batteries;
- heat sinks;
- guns.

There is no accumulated module-volume total and no fit check. A utility slot
can accept a 25 t Laser Engine, a 200 t laboratory, a 700 t Repair Bay, or a
1,000 t Salvage Bay without changing the hull-space calculation.

### Practical geometric envelope

The installed 3,927 m³ cylinder is an external geometric envelope, not usable
machinery volume. Tapered ends, pressure shells, frames, thrust structure, tanks,
passageways, cable and pipe runs, maintenance clearance, micrometeoroid
protection, and inaccessible voids all reduce the useful amount.

The settled 55 m × 15 m Gunship produces a much larger **9,719 m³** cylinder.
For planning, **4,900–6,300 m³ of allocatable internal volume** preserves the
same generous assumption that roughly 50–65% can become compartments or
machinery bays. This is a design allowance, not a value enforced by the game.

## A first Gunship volume budget

This budget is deliberately broad. Its purpose is to test whether the hull
concept works, not to pretend that every component has already been designed.

| Function | Gross planning allocation | Notes |
|---|---:|---|
| Crew cabin, bridge, life support, exercise and stores | 120–180 m³ | Includes inaccessible outfitting volume around the net habitable volume |
| 10-inch nose cannon and magazine | 200–350 m³ | Recoil path, autoloader, handling equipment and service access dominate projectile volume |
| Power plant, conversion machinery and directional shielding | 250–500 m³ | Appropriate placeholder for a multi-megawatt crewed system; full-power-cap plants may be much larger |
| Drive and aft thrust structure | 250–450 m³ | Excludes propellant |
| Radiator roots, pumps, manifolds and stowage | 50–150 m³ | Deployed radiating surface remains external |
| Two ordinary utility modules | 200–400 m³ total | About 100–200 m³ each; exceptionally massive utilities need individual treatment |
| Propellant, pressurant and tanks | 200–800 m³ | Mission-dependent and not limited by the game's single propellant icon |
| Hull frames, pressure shell, distribution, access and unusable voids | 500–900 m³ | Distributed throughout the vehicle |

The middle of these ranges is approximately 2,400 m³. It uses less than half
of the settled allocatable-volume band, leaving substantial margin for larger
power, propulsion, protection, and propellant systems. The Gunship is therefore
not too small for its slot diagram. The remaining danger is that abstract slots
still allow combinations whose real volume falls outside even this generous
budget.

### Crew volume

An older NASA long-duration habitat sizing curve gives:

`net habitable m³/person = 4.8827 × ln(mission days) - 3.9113`

At 180 days this is approximately **21.4 m³ per person**, or **64 m³ net** for
three people. Net habitable volume excludes structure, equipment, stores, and
many inaccessible spaces. Allowing 120–180 m³ gross for the three-person
habitable section is therefore conservative but comfortable.

For contrast, Artemis II Orion provides only 9.3 m³ habitable and 19.6 m³
pressurized volume for four people, but only for a mission of roughly ten days.

Primary anchors:

- [NASA, long-duration habitable-volume sizing](https://ntrs.nasa.gov/api/citations/20120009534/downloads/20120009534.pdf)
- [NASA Artemis II Reference Guide](https://www.nasa.gov/wp-content/uploads/2026/01/a2-reference-guide-012825.pdf)

## Approximate volumes of the selected modules

These are installed-volume estimates, not template values.

| Module or subsystem | Engineering estimate | Main uncertainty |
|---|---:|---|
| Lithium-Ion Battery, 12 GJ/11 t | 10–25 m³ | Cell volumetric energy density, containment, cooling and service clearance |
| Water Heat Sink, current 250 t | 270–300 m³ | Water alone is about 250 m³; tank structure and ullage add volume |
| Heavy Water Heat Sink, current 500 t | 540–600 m³ | Localization says it is simply a larger water tank |
| 10-inch Cannon, current loaded mass 179 t | 200–350 m³ gross bay | The 54 t of ammunition occupies only about 7 m³ as solid steel; handling and machinery dominate |
| Fuel-cell/electrolyzer machinery at about 8.2–8.8 MW | 20–60 m³ before reactant storage and solar stowage | Alkaline-system scale-up, redundancy and regenerative-cycle equipment |
| Multi-megawatt fission plant | 250–500 m³ planning allocation | Crew shield, separation, conversion system, coolant loops and required radiator |
| Aluminum radiator for bare-hull reactor losses | roughly 70–120 m² deployed, depending on efficiency and detailed radiator rules | Panel orientation, two-sided radiation, plumbing and combat retraction |

The heat sinks illustrate why a universal “one utility slot” volume cannot be
made physical. The Heavy Water Heat Sink consumes roughly 5–6% of the lower
settled Gunship allocatable-volume estimate, while many smaller utilities use
far less.

### Fuel cells under the settled planning values

For a bare Gunship's 5.5165 MW useful load:

| Fuel cell | Efficiency | Gross production | Specific mass | Installed mass | 35%-efficient array area at 1 AU |
|---|---:|---:|---:|---:|---:|
| I | 63% | 8.756 MW | 2.8 kg/kW | 24.52 t | 18,400 m² |
| II | 65% | 8.487 MW | 1.8 kg/kW | 15.28 t | 17,800 m² |
| III | 67% | 8.234 MW | 0.48 kg/kW | 3.95 t | 17,300 m² |

DOE's aggressive integrated transportation-fuel-cell target of 850 W/L would
put the fuel-cell power hardware alone near 10 m³ at these outputs. Alkaline
regenerative equipment also needs an electrolyzer, water management, pumps,
thermal hardware, reactant tanks, and controls, so 20–60 m³ before energy
storage is a generous placeholder.

The array result is much more consequential. If the localized solar array must
continuously supply the listed gross power near Earth, it needs roughly
18,000 m² even at an optimistic 35% complete-panel efficiency. The array would
be more than 130 m on a side if square. Its mass and stowage cannot credibly fit
inside Fuel Cell III's 3.95 t installed allowance.

If instead the array recharges the fuel cells slowly between high-power
periods, its area can be smaller—but the game supplies no stored-energy
capacity, discharge duration, or recharge-duty-cycle field. A unique physical
volume cannot be assigned until that missing time dimension is chosen.

Primary anchors:

- [DOE integrated fuel-cell power-density targets](https://www.energy.gov/cmei/fuels/doe-technical-targets-fuel-cell-systems-and-stacks-transportation-applications)
- [NASA regenerative fuel-cell architecture](https://ntrs.nasa.gov/api/citations/20160004090/downloads/20160004090.pdf?attachment=true)

## Why a reactor plant is much larger than its fuel

NASA's SP-100 work provides a particularly useful mass decomposition for a
compact space reactor. One system goal divided approximately 4,580 kg into:

| Subsystem | Mass |
|---|---:|
| Reactor | 700 kg |
| Shield | 1,000 kg |
| Primary heat transport | 500 kg |
| Instrumentation and control | 290 kg |
| Power conversion | 370 kg |
| Heat rejection | 850 kg |
| Power conditioning, control and distribution | 390 kg |
| Mechanical structure | 480 kg |

Only about 15% of this total is assigned to the reactor itself. The shield and
heat-rejection hardware each outweigh it.

Another SP-100 design had a **0.55 m diameter × 0.75 m high reactor** producing
2.4 MW thermal. The core and vessel occupied only about 0.18 m³, while complete
25–75 kWe systems were estimated at 3.2–5.0 t including an 810 kg shield.

A later 100 kWe reactor-Brayton concept used a converter assembly about 1.8 m
in diameter and 2.6 m long and had a complete system mass of 4,115 kg. The
system, not the core, is the useful comparison.

Primary anchors:

- [NASA SP-100 system mass breakdown](https://ntrs.nasa.gov/api/citations/19890003294/downloads/19890003294.pdf)
- [NASA SP-100 core dimensions and shielded system mass](https://ntrs.nasa.gov/api/citations/19950015535/downloads/19950015535.pdf)
- [NASA 100 kWe reactor-Brayton configuration](https://ntrs.nasa.gov/api/citations/20150012286/downloads/20150012286.pdf)

### Shielding can be reduced, not erased

A spacecraft does not need terrestrial concrete containment. It can place the
reactor aft, put the crew forward, use a conical shadow shield, increase
separation, and place water or propellant in the protected line of sight.
Those are legitimate mass-saving advantages.

They do not make crew shielding free. NASA crew-rated studies find that
shielding can dominate the system:

- a manned Mars rover study estimated **8.6–20.6 t** of shielding for
  0.1–1 MW thermal reactors;
- a shadow-shielded crewed platform study still found approximately
  **12–20 t** with a 70 m boom, depending on power and conversion system.

The settled Gunship is 55 m long and depicts the plant inside the hull. It can use
directional geometry and water/propellant as multifunction shielding, but some
fixed shield and separation allowance should remain.

Primary anchors:

- [NASA, manned rover reactor-shield analysis](https://ntrs.nasa.gov/citations/19930029796)
- [NASA, shadow shielding and separation trade](https://ntrs.nasa.gov/api/citations/19890003294/downloads/19890003294.pdf?attachment=true)

### Current game scaling on a bare Gunship

Power-plant mass is:

`max(1 tonne, required gross GW × specificPower_tGW)`

At the newly selected efficiencies, the bare Gunship requires only
6.90–7.88 MW gross from Solid Reactor I–V. With the current template specific
masses, the calculated plants would weigh only 0.055–0.315 t, so every one
hits the **one-tonne minimum**.

This explains why the displayed reactor occupies a negligible implied fraction
of the ship. The game is not placing a full 2–125 GW plant in the Gunship; it
is creating a demand-sized plant whose current specific mass is so low that
the generic one-tonne floor becomes the controlling rule.

At each reactor's full rating, current template masses would instead be:

| Reactor | Maximum output | Current full-rating mass |
|---|---:|---:|
| I | 2 GW | 80 t |
| II | 6 GW | 204 t |
| III | 20 GW | 560 t |
| IV | 60 GW | 720 t |
| V | 125 GW | 1,000 t |

Those full-capacity plants are not suitable for a Gunship even though the slot
system does not prohibit them. The design requirement, rather than the
template maximum, determines installed mass.

NASA concept studies provide useful comparison points:

- nearer-term SP-100 Rankine concepts: roughly **7–10 kg/kWe** in favorable
  studies, with other complete concepts much heavier;
- a 1.7 MWe nuclear-electric system: **24.8 kg/kWe** for power,
  conditioning, and propulsion equipment;
- a highly advanced 10 MWe Brayton study: approximately **2 kg/kWe**, while
  warning that crew-rated shielding dominates surface-system mass.

The game's current Solid I value is **0.04 kg/kW**, fifty times lighter than
the highly advanced 2 kg/kW study before making a crew-shielding allowance.

Primary anchors:

- [NASA, SP-100/Rankine specific-mass studies](https://ntrs.nasa.gov/api/citations/19930005607/downloads/19930005607.pdf)
- [NASA, 1.7 MWe nuclear-electric system](https://ntrs.nasa.gov/citations/19930065919)
- [NASA, advanced multi-megawatt Brayton scaling](https://ntrs.nasa.gov/api/citations/20010016863/downloads/20010016863.pdf)

## Is 1–1.6 GWe a thermodynamic reactor-size limit?

Not in the simple sense.

Coolant pumping and thermal hydraulics absolutely constrain **power density**:

- fuel and cladding have temperature limits;
- critical heat flux and flow stability limit heat removal;
- higher flow through small passages raises pressure drop and pump work;
- pressure vessels, pipes, heat exchangers and turbomachinery grow with flow;
- shutdown heat must still be rejected after fission stops.

These prevent arbitrary power from being extracted from a fixed-size core.
They do not impose a universal plant-output wall near 1–1.6 GWe. A larger core,
longer vessel, more coolant loops, larger steam generators, or multiple
modules can raise total output.

The AP1000 is illustrative: the NRC rates it at about 3.4 GW thermal and at
least 1 GW electric. Compared with the AP600, its reactor vessel retained the
same diameter but became longer, while the core, steam generators, coolant
pumps, pressurizer, containment volume, safety systems, and turbine capacity
were increased.

IAEA material attributes the historical move toward units as large as
1,600 MWe largely to economies of scale. The practical upper choice must also
account for grid stability and the reserve generation required if one large
unit trips. Current certified small modular designs demonstrate that much
smaller outputs are technically possible; for example, the NuScale US460
design uses 77 MWe modules.

Thus the terrestrial cluster is primarily an engineering-economic optimum
among available standard designs, not evidence that coolant pumping suddenly
consumes all net power above 1.6 GWe.

Primary anchors:

- [NRC AP1000 safety evaluation](https://www.nrc.gov/reading-rm/doc-collections/nuregs/staff/sr1793/initial/index)
- [NRC description of pressurized-water reactor systems](https://www.nrc.gov/reactors/power/pwrs)
- [IAEA discussion of economies of scale and grid-size constraints](https://nucleus.iaea.org/sites/connect/MSNpublic/Pages/nct/downloads/Pub1633Web-39794849.pdf)
- [NRC NuScale US460 project overview](https://www.nrc.gov/reactors/new-reactors/advanced/who-were-working-with/applicant-projects/nuscale-us460)

### What changes in space

A ship does not have a river, ocean, or atmosphere as a heat sink. Radiator
area grows approximately in proportion to rejected power at a fixed
temperature. Coolant flow, pipe cross-section, pump work, conversion
machinery, and power distribution also grow with output.

For spacecraft, the stronger progression constraints are therefore:

1. maximum manageable temperature;
2. radiator area and vulnerability;
3. coolant-loop pressure drop and pump work;
4. conversion-machinery specific mass;
5. radiation geometry and shield mass;
6. heat-exchanger and maintenance volume.

This supports the Children of a Dead Earth intuition that very high compact
output becomes progressively costly. It does not support a universal
one-gigawatt cutoff.

## Reactor crew

NASA's current lunar fission goal is a 40 kWe system under six tonnes that can
operate for ten years without human intervention. That demonstrates that
continuous manual control is not necessary for a small, isolated reactor.

It is not a close operational analogue for a multi-megawatt combat plant.
Present aircraft-carrier reactor departments include reactor controls,
electrical, mechanical, propulsion, laboratory/radiological, and emergency
response functions. A U.S. Navy account describes more than 400 sailors
assigned to reactor casualty-assistance teams on one carrier.

Most of those people are not needed to move control rods continuously. They
provide watch rotations, maintenance, chemistry and radiological control,
electrical distribution, steam-plant operation, training, inspections,
firefighting, casualty response, and damage control.

Primary anchors:

- [NASA 40 kWe, ten-year unattended fission-system goal](https://www.nasa.gov/centers-and-facilities/glenn/nasas-fission-surface-power-project-energizes-lunar-exploration/)
- [U.S. Navy carrier reactor roles](https://www.navy.mil/Press-Office/News-Stories/display-news/Article/4144413/uss-john-c-stennis-implements-reactor-pin-program-to-enhance-readiness-and-prof/)
- [U.S. Navy reactor casualty-assistance staffing](https://www.navy.mil/Press-Office/News-Stories/display-news/Article/2263805/department-in-the-spotlight-casualty-assistance-teams-keep-the-ship-moving/)

### Planning conclusion on crew

Allowing reactor crew is reasonable if the value represents maintenance,
radiological control, watchstanding, and combat damage control rather than
manual normal operation.

Six billets for a multi-megawatt plant are not excessive by present naval
standards; they are an extremely automated abstraction. Six people permit only
two people on each of three watches before leave, illness, maintenance teams,
or battle casualties. The strongest criticism of the vanilla reactor is
therefore its mass and missing volume, not the existence of reactor crew.

## Recommended planning model

A physically clearer reactor abstraction would have:

`installed mass = fixed plant floor + power-scaled machinery`

and similarly:

`installed volume = fixed shield/control/maintenance volume`
`                 + power-scaled core/conversion/cooling volume`

The fixed term represents the minimum reactor vessel, controls, directional
shield, startup and shutdown systems, access, and containment. The variable
term represents increasing core, coolant flow, conversion, distribution, and
heat-rejection equipment.

The current formula has only a generic one-tonne floor and a linear
power-specific mass. That is why a crewed fission plant can collapse to one
tonne while still demanding six operators.

For these hulls specifically:

- use the settled **55 m × 15 m diameter** statistical envelope and its
  **9,719 m³** cylindrical volume for the Gunship;
- use **62 m × 15 m diameter** and **10,956 m³** for the Escort;
- initially treat roughly **4,900–6,300 m³** for the Gunship and
  **5,500–7,100 m³** for the Escort as generous allocatable-volume audit bands,
  pending detailed tapered-hull and machinery-layout models;
- reserve **120–180 m³** for three crew on a six-month mission;
- reserve **250–500 m³** for an early crewed fission power bay;
- treat the reactor shield as a directional fixed-mass floor, with water and
  propellant allowed to serve as additional multifunction shielding;
- assign estimated volume per module in the research workbook rather than
  assuming equal utility slots;
- decide whether eventual gameplay should enforce volume or use it only as a
  balance-audit metric.
