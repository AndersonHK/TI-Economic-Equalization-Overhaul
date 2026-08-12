# Human hull slots, crew, naval references, and drive scaling

Status: measurement record and implementation specification for the 2026-08-12
graphical-variant drive-scaling pass.

This report brings together the tier 1-3 hull comparison, the real-world naval
cross-check, the weapon/utility-slot code audit, and the rendered drive-asset
measurements. It distinguishes geometric envelopes from usable volume and
measured art from provisional gameplay multipliers.

## Conclusions

- One base crew billet per weapon or utility slot is a coherent game-wide
  balance rule for human hulls. It is a shared command, engineering,
  maintenance, logistics, medical, and damage-control pool, not one literal
  operator seated at each slot.
- At that rule, crew per slot is constant by definition. The meaningful tier
  discontinuity is the envelope allocated to each slot: the weighted runtime
  cylinder rises from about **3,044 m³/slot** at tier 1 to **4,584 m³/slot** at
  tier 2 and **13,115 m³/slot** at tier 3.
- The tier 3 hulls therefore are slot-poor relative to their statistical
  envelopes. More utility slots alone have diminishing value because utilities
  range from small electronics to enormous repair and salvage facilities and
  the game has no module-volume fit test.
- Existing weapon slots correspond to actual hull hardpoint/controller
  infrastructure. Increasing the JSON counts alone is unsafe. Multi-slot
  weapons render as one weapon at a core mount while reserving adjacent diagram
  cells; making a two- or four-cell weapon consume one cell is feasible in code,
  but it changes the capacity model and requires placement, occupancy, refit,
  AI, validation, and UI work. It does not create more physical turret mounts.
- Utilities are intrinsically one-slot modules in the current code. A genuine
  multi-slot utility is feasible but is a larger system change because utility
  placement has none of the weapon adjacency/core-slot machinery.
- Rendered drives clearly scale with hull. The six-nozzle Titan has roughly
  **8.11 times** the default Gunship De Laval transverse bounding area and
  **6.59 times** its default magnetic area. Human measurements remain research
  evidence for a later balance pass; current human gameplay retains the
  previously approved conservative hull factors.
- Alien drive resources are hull-specific but not nozzle-family-specific. The
  installed standard alien hulls each have one graphical appearance and one
  corresponding drive-resource family, so their measured factor is stable
  across alien drive physics types. The Salamander has no standalone alien
  drive resource and deliberately retains factor 1.00.
- Reactor hull-size caps remain deferred. Existing reactor mass, output limits,
  and engine-section geometry need their own rebalance before adding a second
  hull-dependent constraint.

## Data and method

### Game templates and runtime formulas

The installed `TIShipHullTemplate.json` supplied tier, length, diameter, hull
mass, crew, structural integrity, hardpoint counts, and utility counts. The mod
override was applied on top for the six hulls already changed.

The compiled hull class does not read the JSON `volume` value when it needs hull
volume. It evaluates a full cylindrical envelope:

`V = π × (width / 2)² × length`

Accordingly, every volume ratio below uses that runtime cylinder. It is an
external planning envelope, not usable pressurized volume. Tapered geometry,
structure, armor, tanks, machinery, access, and voids reduce usable volume.

The empty-mass columns use the mod's settled crew-support allowance:

`empty mass = hull mass + crew × 3 t`

The counted balance slots are:

`slots = nose hardpoints + hull hardpoints + utility slots`

Drive, power-plant, radiator, and armor positions are excluded because every
hull receives those separately and they are not part of the one-crew-per-slot
rule.

### Asset measurement

`scripts/ship-balance/measure_ship_prefabs.py` loads the installed `ships`
Unity asset bundle
with UnityPy and recursively composes each prefab transform. Its original hull
path remains separate from the drive-resource path:

- `measure_hull` measures active hull meshes and layer-17 raycast colliders;
- `measure_drive` measures the default drive already embedded in a ship prefab;
- `measure_drive_resource` measures a named standalone drive prefab;
- `measure_individual_drive_nozzles` splits mesh topology into connected
  components and associates the largest plausible bell component with each
  serialized `ThrusterPoint`;
- `measure_drive_resources` enumerates default and alternate De Laval,
  magnetic, and pulse resources without changing the hull result.
- `measure_alien_drive_resources` measures the hull-specific alien x1 and x6
  resources and records unavailable resources explicitly.

The old hull output was compared before and after the drive functions were
added and remained byte-for-byte equivalent. Bounds are axis-aligned mesh
envelopes in prefab-local coordinates; transverse area is `x size × y size`.
It is an objective visual-scale proxy, not true nozzle exit area or occupied
solid volume. Connected-component measurements are reported separately so a
large shared mounting plate is not silently treated as six nozzle bells.

## Human hull ratios after this pass

The later six hulls retain their vanilla statistical dimensions in this pass.
Only their crew and hull mass change. The first six use the already implemented
model-informed dimensions and masses.

| Tier | Hull | Runtime cylinder | Weapon + utility slots | Empty mass | t/slot | m³/slot | Crew | t/crew | m³/crew |
|---:|---|---:|---:|---:|---:|---:|---:|---:|---:|
| 1 | Gunship | 9,719 m³ | 3 | 180 t | 60.0 | 3,240 | 3 | 60.0 | 3,240 |
| 1 | Escort | 10,956 m³ | 4 | 350 t | 87.5 | 2,739 | 4 | 87.5 | 2,739 |
| 1 | Corvette | 14,754 m³ | 5 | 400 t | 80.0 | 2,951 | 5 | 80.0 | 2,951 |
| 1 | Frigate | 25,447 m³ | 8 | 600 t | 75.0 | 3,181 | 8 | 75.0 | 3,181 |
| 2 | Monitor | 22,698 m³ | 7 | 700 t | 100.0 | 3,243 | 7 | 100.0 | 3,243 |
| 2 | Destroyer | 41,548 m³ | 9 | 900 t | 100.0 | 4,616 | 9 | 100.0 | 4,616 |
| 2 | Cruiser | 54,978 m³ | 12 | 1,000 t | 83.3 | 4,581 | 12 | 83.3 | 4,581 |
| 2 | Battlecruiser | 54,978 m³ | 10 | 1,200 t | 120.0 | 5,498 | 10 | 120.0 | 5,498 |
| 3 | Lancer | 201,062 m³ | 14 | 2,000 t | 142.9 | 14,362 | 14 | 142.9 | 14,362 |
| 3 | Battleship | 98,175 m³ | 14 | 1,600 t | 114.3 | 7,012 | 14 | 114.3 | 7,012 |
| 3 | Dreadnought | 264,581 m³ | 18 | 2,400 t | 133.3 | 14,699 | 18 | 133.3 | 14,699 |
| 3 | Titan | 288,634 m³ | 19 | 3,200 t | 168.4 | 15,191 | 19 | 168.4 | 15,191 |

Because this pass sets one crew per counted slot, `t/crew` equals `t/slot` and
`m³/crew` equals `m³/slot`. The duplicate columns are retained to make the
relationship explicit and to compare with naval complements below.

Weighted by the total slots in each tier:

| Tier | Hulls | Empty mass/slot | Runtime cylinder/slot | Crew/slot |
|---:|---:|---:|---:|---:|
| 1 | 4 | 76.5 t | 3,044 m³ | 1.00 |
| 2 | 4 | 100.0 t | 4,584 m³ | 1.00 |
| 3 | 4 | 141.5 t | 13,115 m³ | 1.00 |

The tier 3 result is not subtle: it has 2.86 times the tier 2 envelope per slot
and 4.31 times the tier 1 value. Some increase is desirable for armor,
redundancy, propellant, power, heat rejection, access, and damage tolerance,
but the statistical hulls presently grow much faster than their module choice.

## Crew and rounded empty-mass change

All twelve human combat hulls preserve a clean empty-mass landmark. At 3 t per
crew billet, the new hull mass is the landmark minus the new crew allowance.

| Tier | Hull | Weapon slots | Utilities | Old crew | New crew | New crew support | Old hull mass | New hull mass | New empty mass | Empty-mass change |
|---:|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| 1 | Gunship | 1 | 2 | 3 | **3** | 9 t | 178 t | **171 t** | **180 t** | −7 t |
| 1 | Escort | 2 | 2 | 4 | **4** | 12 t | 350 t | **338 t** | **350 t** | −12 t |
| 1 | Corvette | 2 | 3 | 8 | **5** | 15 t | 400 t | **385 t** | **400 t** | −24 t |
| 1 | Frigate | 3 | 5 | 20 | **8** | 24 t | 600 t | **576 t** | **600 t** | −60 t |
| 2 | Monitor | 4 | 3 | 35 | **7** | 21 t | 800 t | **679 t** | **700 t** | −205 t |
| 2 | Destroyer | 4 | 5 | 40 | **9** | 27 t | 825 t | **873 t** | **900 t** | −45 t |
| 2 | Cruiser | 5 | 7 | 60 | **12** | 36 t | 1,000 t | **964 t** | **1,000 t** | −180 t |
| 2 | Battlecruiser | 5 | 5 | 70 | **10** | 30 t | 1,200 t | **1,170 t** | **1,200 t** | −210 t |
| 3 | Lancer | 7 | 7 | 100 | **14** | 42 t | 2,000 t | **1,958 t** | **2,000 t** | −300 t |
| 3 | Battleship | 8 | 6 | 80 | **14** | 42 t | 1,600 t | **1,558 t** | **1,600 t** | −240 t |
| 3 | Dreadnought | 11 | 7 | 120 | **18** | 54 t | 2,400 t | **2,346 t** | **2,400 t** | −360 t |
| 3 | Titan | 10 | 9 | 120 | **19** | 57 t | 3,200 t | **3,143 t** | **3,200 t** | −360 t |

“Old empty mass” for the delta is old hull mass plus the mod's 3 t allowance
for every old billet. The hull-mass reduction is not a second crew discount: it
keeps the selected empty hull-plus-crew total rounded after the billet count is
reduced.

## Real-world naval cross-check

Surface ships are useful as a warning against naive linear scaling, not as a
direct conversion to spacecraft. Displacement includes seawater displacement;
the dimension product below is only a length × beam × draft bounding-box proxy,
not internal volume. VLS cells also omit guns, torpedoes, aviation, sensors,
propulsion, hotel loads, and the combat system that makes a cell useful.

| Ship/reference | Displacement | Crew | VLS cells | t/crew | Crew/VLS cell | t/VLS cell | Box proxy | Box/crew | Box/VLS cell |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| USS Arleigh Burke (DDG-51, Flight I) | about 8,362 metric t | 300+ | 90 | ≤27.9 | ≥3.33 | 92.9 | about 29,183 m³ | ≤97.3 m³ | 324 m³ |
| USS Antietam (CG-54, Ticonderoga class) | about 9,957 metric t | 330 | 122 | 30.2 | 2.70 | 81.6 | about 30,000 m³ | 90.9 m³ | 246 m³ |
| Zumwalt class | 15,995 metric t | 197 | 80 | 81.2 | 2.46 | 199.9 | — | — | — |
| Type 45/Daring class | 7,350 metric t | 260 deployment complement | 48 | 28.3 | 5.42 | 153.1 | — | — | — |

Sources and choices:

- The [U.S. Navy DDG-51 fact file](https://www.navy.mil/Resources/Fact-Files/Display-FactFiles/Article/2169871/destroyers-ddg/destroyers-ddg-51/)
  gives the class dimensions, 8,230-9,700 long-ton displacement range, and
  Flight IIA/III complements. The official
  [USS Arleigh Burke characteristics page](https://www.surflant.usff.navy.mil/Organization/Operational-Forces/Destroyers/USS-Arleigh-Burke-DDG-51/About-Us/Characteristics/)
  gives the lead ship's 153.8 × 20.4 × 9.3 m dimensions, 29+61 cells, and
  “300+” crew. The table uses the class's low-end 8,230-long-ton figure and
  converts it at 1.0160469 metric tonnes per long ton.
- Naval History and Heritage Command's
  [USS Antietam entry](https://www.history.navy.mil/research/histories/ship-histories/danfs/a/antietam-iii.html)
  gives 9,800 tons, 567 × 55 × 34 ft, complement 330, and two 61-cell Mk 41
  launchers. The table treats the naval displacement as long tons for the
  metric conversion.
- The [U.S. Navy DDG-1000 fact file](https://www.navy.mil/Resources/Fact-Files/Display-FactFiles/Article/2391800/destroyers-ddg-1000/)
  gives 15,995 metric tonnes, crew 197, and 80 PVLS cells. Its fact file does
  not give draft, so no box volume is invented.
- The [Royal Navy Daring-class page](https://www.royalnavy.mod.uk/equipment/ships/daring-class)
  gives 7,350 tonnes and describes a 260-sailor deployment; the
  [Sea Viper upgrade notice](https://www.royalnavy.mod.uk/news/2022/may/24/20220524-sea-viper)
  gives 48 vertical-launch silos. The official page does not give both beam and
  draft, so its box columns are blank.

The comparison supports two restrained conclusions. First, larger and more
automated modern ships can carry far more tonnes per person: Zumwalt is roughly
81 t/crew versus about 28-30 t/crew for the other destroyer/cruiser references.
Second, cells per crew do not scale monotonically with size; mission,
automation, aviation, sensors, maintenance policy, and damage-control doctrine
matter at least as much as displacement.

For historical scale, Naval History and Heritage Command lists
[USS Missouri](https://www.history.navy.mil/research/histories/ship-histories/danfs/m/missouri-iii.html)
at 45,000 tons and 1,921 crew, about 23.4 tons per person before unit
conversion. Its armor and weapon batteries made it much heavier than smaller
contemporary combatants, but modern automation allows Zumwalt to exceed it
greatly in mass per crew. The naval intuition “larger ships can be more spacious
per person” is directionally plausible, not a universal scaling law.

### Why the naval ratios do not transfer directly to Terra Invicta

- A surface ship's waterplane, buoyancy, and unpressurized weather decks are
  unlike a spacecraft pressure vessel.
- A spacecraft needs vacuum pressure structure, radiation and micrometeoroid
  protection, closed-loop life support, large heat-rejection surfaces, and
  propellant whose volume depends strongly on chemistry and mission delta-v.
- A VLS cell is mostly a storage/launch interface. A game weapon slot may mean
  a turret, magazine, power conversion, cooling, fire control, and structural
  load path.
- Even the rebalance Gunship is a 55 m spacecraft with a 180 t empty
  hull-plus-crew mass. It is already in the scale regime of Starship by length
  and the ISS by mass, so extrapolating from patrol boats or aircraft is less
  useful than it first appears.

## Weapon hardpoint and multi-slot feasibility

The runtime hull owns a serialized `shipModuleSlots` list. Multi-slot weapon
mount enums (`TwoHullHoriz`, `TwoHullVert`, `ThreeHullHoriz`, `FourHull`, and
their nose equivalents) call `WeaponSlotSet` to find adjacent compatible cells.
Only the core slot stores the weapon; `GetPartInHullSlot(..., true)` reports the
same weapon in its reserved secondary cells.

Visual construction then iterates the stored weapons and calls `SetWeapon` once
per weapon, indexing the prefab's nose/dorsal/ventral controller arrays from the
core slot. Therefore:

1. A four-slot cannon is one real rendered cannon, not four turrets.
2. Its other three slots are abstract capacity reservations.
3. Adding hardpoint entries beyond the prefab's prepared controller arrays can
   index missing mounts or place weapons at inappropriate transforms.
4. Current declared human hardpoints are the safe, authored capacity. Extra
   nose or hull slots require a per-prefab visual-mount audit and likely prefab
   edits, not just template numbers.

Allowing a two- or four-slot hull weapon to consume one diagram slot is
technically feasible. The least invasive design would add a hull-size capacity
rule at placement/validation time while leaving the weapon's mount and visual
resource unchanged. It still must update all of the following together:

- valid-slot-set generation and drag/drop placement;
- secondary-slot occupancy and removal;
- saved template validation and refit comparison;
- AI design construction;
- designer UI footprints and tooltips;
- any combat-value logic that infers capability from `internalSize`.

That approach increases weapon capacity without inventing a new turret
transform, but multiple weapons still need distinct core hardpoints to render as
distinct turrets. It is deferred in this pass.

A multi-slot utility needs new core/secondary occupancy semantics because the
utility path currently matches one module entry to one utility slot. It is
feasible, but a utility-specific size/capacity value plus designer, AI, refit,
serialization, and validation changes would be required. Reusing weapon mount
enums would incorrectly imply weapon geometry. This too remains deferred.

## Rendered drive measurements

The drive resource path is hull-specific: the game builds names such as
`Earth_Titan_DeLavalx6`, then `SetDrive` copies that resource's mesh, material,
scale, and serialized thruster points into the ship. Six thrusters on a Gunship
and six on a Titan therefore share a drive-template statistic but do not share
the same rendered engine geometry.

The table normalizes transverse bounding area to the Gunship for each nozzle
family. “Range” spans default and alternate human appearances. The geometric
center is `sqrt(min × max)`, a useful midpoint for a multiplicative range.

| Tier | Hull | Default De Laval | Default magnetic | Range across appearances | Geometric center |
|---:|---|---:|---:|---:|---:|
| 1 | Gunship | 1.00 | 1.00 | 1.00-1.00 | 1.00 |
| 1 | Escort | 1.00 | 1.00 | 1.00-1.00 | 1.00 |
| 1 | Corvette | 1.00 | 1.00 | 1.00-1.00 | 1.00 |
| 1 | Frigate | 1.55 | 1.38 | 1.00-1.55 | 1.21 |
| 2 | Monitor | 1.00 | 1.00 | 1.00-1.00 | 1.00 |
| 2 | Destroyer | 1.00 | 1.00 | 1.00-1.89 | 1.32 |
| 2 | Cruiser | 4.15 | 2.86 | 2.04-4.15 | 2.93 |
| 2 | Battlecruiser | unresolved* | 2.00 | 2.00-3.65 | 2.66 |
| 3 | Lancer | 1.72 | 1.72 | 1.72-3.22 | 2.14 |
| 3 | Battleship | 3.35 | 3.60 | 2.23-3.60 | 3.05 |
| 3 | Dreadnought | 6.74 | 4.33 | 4.33-7.49 | 5.86 |
| 3 | Titan | 8.11 | 6.59 | 6.59-11.52 | 8.34 |

\* The default Battlecruiser De Laval mesh joins the bell to shared topology,
so the connected-component nozzle method deliberately reports it unresolved
rather than manufacturing a result.

For the default six-nozzle endpoints:

| Resource | Mean individual bounding size | Mean transverse bound | Ratio |
|---|---:|---:|---:|
| Gunship De Laval x6 | 2.792 × 2.792 × 2.364 m | 7.798 m² | 1.000 |
| Titan De Laval x6 | 7.953 × 7.953 × 6.732 m | 63.260 m² | 8.112 |
| Gunship magnetic x6 | 4.351 × 4.353 × 0.482 m | 19.414 m² | 1.000 |
| Titan magnetic x6 | 11.171 × 11.176 × 1.237 m | 128.001 m² | 6.593 |

The alternate Titan reaches 7.854 times the Gunship De Laval reference and
11.524 times the magnetic reference. Appearance dependence is one reason not to
treat art area as a precise physical thrust law.

### Measured per-engine scaling: alien graphical variants

Alien `TIDriveTemplate.modelResource` ignores De Laval/magnetic classification
and constructs a hull-specific resource name. Each standard alien hull in the
installed 1.0.51 templates has exactly one model resource at resolved
appearance index 0. Consequently, separate De Laval and magnetic rows would be
duplicates; the one factor below applies to both engine types for that visual
variant.

The standalone x1 prefab is the reproducible per-drive-unit reference. For a
normal resource its transverse proxy is the smaller transverse dimension
squared, which avoids treating small asymmetry as meaningful. The Mothership x1
resource contains three serialized thruster points spread across one cluster,
so its proxy is `min(x, y)^2 x 3`; this counts its three visible drive units
without counting the empty spacing between them as nozzle area. Every row is
normalized to the Alien Gunship x1 proxy of 32.490 m2. Gameplay never reduces a
drive below its template baseline, so the Corvette's measured 0.758 becomes an
effective factor of 1.000.

| Hull | Variant | x1 resource bounds (m) | Thruster points | Proxy (m2) | Measured ratio | Implemented factor |
|---|---:|---:|---:|---:|---:|---:|
| Alien Gunship | 0 | 5.700 x 5.710 x 17.165 | 1 | 32.490 | 1.000 | **1.000** |
| Alien Escort | 0 | 5.700 x 5.710 x 17.165 | 1 | 32.490 | 1.000 | **1.000** |
| Alien Corvette | 0 | 4.962 x 4.970 x 14.942 | 1 | 24.621 | 0.758 | **1.000** |
| Alien Frigate | 0 | 6.096 x 6.106 x 18.358 | 1 | 37.161 | 1.144 | **1.144** |
| Alien Monitor | 0 | 8.458 x 8.472 x 25.469 | 1 | 71.538 | 2.202 | **2.202** |
| Alien Destroyer | 0 | 10.485 x 10.503 x 31.575 | 1 | 109.935 | 3.384 | **3.384** |
| Alien Cruiser | 0 | 10.340 x 10.357 x 31.136 | 1 | 106.916 | 3.291 | **3.291** |
| Alien Battlecruiser | 0 | 10.579 x 10.597 x 31.856 | 1 | 111.915 | 3.445 | **3.445** |
| Alien Lancer | 0 | 10.340 x 10.357 x 31.136 | 1 | 106.916 | 3.291 | **3.291** |
| Alien Battleship | 0 | 10.579 x 10.597 x 31.856 | 1 | 111.915 | 3.445 | **3.445** |
| Alien Dreadnought | 0 | 10.579 x 10.597 x 31.856 | 1 | 111.915 | 3.445 | **3.445** |
| Alien Titan | 0 | 15.642 x 15.668 x 47.102 | 1 | 244.672 | 7.531 | **7.531** |
| Alien Assault Carrier | 0 | 10.579 x 10.597 x 31.856 | 1 | 111.915 | 3.445 | **3.445** |
| Alien Mothership | 0 | 166.849 x 16.850 x 95.160 | 3 | 851.768 | 26.216 | **26.216** |
| Salamander Gunship | 0 | unavailable: no standalone alien drive prefab | - | - | - | **1.000 fallback** |

These are graphical ratios, not claims about nozzle exit area or propulsion
technology. In particular, the Mothership result is exceptional and should be
treated as a high-risk balance value during manual testing.

## Alien graphical-variant scaling and approved human factors

Human ships retain these approved hull-only factors for every graphical
appearance and drive nozzle family:

| Hull | Thrust, flow, powered-drive requirement, and drive-module factor |
|---|---:|
| Gunship through Destroyer | 1.00 |
| Cruiser | **1.30** |
| Battlecruiser | **1.50** |
| Lancer | **1.72** |
| Battleship | **1.75** |
| Dreadnought | **2.00** |
| Titan | **2.50** |

The default and alternate human De Laval/magnetic measurements above are not
yet gameplay multipliers. They remain available for a later human graphical
balance pass after the engine-section method is settled. This implementation
changes only alien graphical scaling; alien authored resources do not
distinguish De Laval from magnetic nozzles. For a selected factor `k`:

- thrust becomes `k × template thrust`;
- exhaust velocity is unchanged;
- physical mass flow `thrust / exhaust velocity` therefore becomes
  `k × template mass flow`;
- a non-self-powered drive's requested electrical power becomes
  `k × template drive power`;
- drive hardware mass and drive material cost become
  `k × template drive mass/cost`;
- reactor and radiator mass/cost follow indirectly from the higher requested
  power and rejected heat through the existing ship calculations.

Terra Invicta does not integrate burn duration or expose hull-level mass flow;
it computes delta-v from exhaust velocity and propellant mass. The mass-flow
increase is therefore a required physical consequence and documentation value,
not a separate fuel-per-second state variable to patch. Constant exhaust
velocity also means a given propellant mass provides the same delta-v while the
higher thrust expends it in less physical burn time.

The runtime passes hull data name, alien status, the DLC-aware resolved
appearance index, and the candidate or installed drive's nozzle to the lookup.
Human hulls deliberately ignore appearance and nozzle and use the approved
hull table. Alien hulls use their measured appearance table and ignore nozzle
because their resource path is nozzle-independent. Existing reactor
`maxOutput_GW` compatibility remains respected after drive power is scaled, but
this pass adds no hull-size reactor cap and changes no reactor template.

The live-ship `currentThrust_N` calculation and every designer presentation use
the same lookup. Cached module-browser rows refresh whenever the designer
re-filters part availability, including hull changes and design loads. The
selected-module side panel and hover tooltip resolve the current design live and
replace localized thrust, combat thrust, required power, drive mass, and
material cost with scaled values. Exhaust velocity and efficiency stay at
template values. This prevents the browser from continuing to advertise the
108 kN Burner Drive baseline, for example, when a Cruiser design is selected.

### Feasibility of appearance- and hull-data-driven scaling

The implementation receives the complete `TISpaceShipTemplate` and passes the
hull data name, alien status, resolved appearance index, and candidate or
installed drive nozzle to the lookup. No new save field is required. Other hull
inputs remain available but are deliberately excluded from the lookup:

| Candidate input | Existing runtime source | Technical availability | Main limitation |
|---|---|---|---|
| Visual appearance | `hullAppearanceIndex`, `GetHullAppearanceIndex`, and `hullTemplate.modelResource[index]` | **High** | The maintained measurements cover the default and first alternate drive art, while appearance indices 2 and 3 use DLC/premium resources that still need measurement. Appearance changes must also force all cached mass, cost, power, compatibility, and acceleration displays to refresh. |
| Hull utility capacity | `hullTemplate.internalModules` or the utility entries in `shipModuleSlots` | **Very high** | It is a stable hull property, but a generic utility berth is not evidence of engine-room volume. It also partly duplicates the one-crew-per-slot signal. |
| Unoccupied utility slots | `moduleTemplateEntries` compared with the hull's utility slots | **Technically high, design risk high** | Thrust would change as unrelated utilities are installed or removed. That makes refits alter drive capacity and can create surprising power-compatibility transitions. It should not be used. |
| Base hull crew | `ship.hullTemplate.crew` | **Very high** | Under the settled rule it mostly restates total weapon-plus-utility capacity. |
| Complete design crew | `ship.crewBillets` | **High, design risk medium** | It includes drive, reactor, radiator, weapon, and utility crew, so two fits of the same hull could receive different engine capacity. |
| Rounded empty hull mass | `hullTemplate.mass_tons` plus the configured base-crew support mass | **Very high** | This is stable and already balanced, but it is a gameplay mass rather than a direct engine-section measurement. Calling patched `dryMass_tons` from the multiplier would be recursive and must be avoided. |
| Runtime cylinder volume | `hullTemplate.volume_m3`, computed from `length_m` and `width_m` | **Very high** | It is the full statistical cylinder, not usable interior or aft machinery volume, and may differ substantially from the prefab. |

The game already serializes the selected appearance on each design, the
visualizer uses the DLC-aware appearance getter, and
`TIDriveTemplate.modelResource(hull, appearanceIndex)` chooses matching human
drive art. Those human resources are measured here but do not affect current
gameplay. Invalid alien appearances, unknown future alien hulls, and the
Salamander use the safe 1.00 baseline and emit a one-time configuration error.
Known Gunship/Escort 1.00 values and the Corvette's measured below-baseline
clamp are intentional and do not log errors.

This deliberately does not use free utilities, complete design crew, patched
dry mass, or full-cylinder volume. Those values either change with unrelated
module choices, recurse through the scaled drive mass, or represent the whole
hull rather than the engine section. A later engine-section measurement can
replace the nozzle-envelope proxy without changing the runtime key.

### Approved human thrust-to-mass table

This is the active human comparison for every appearance and nozzle family.
For a reproducible educated module-mass estimate, it uses the installed
six-thruster Meteor liquid rocket: **92.886 MN**, **2.98 km/s**, and **102 t**.
Its existing template mass is preferable to inventing a new engine density.
The scaled module mass is 102 t × `k`; the reference ship mass is the new empty
hull-plus-crew mass plus that engine only.

| Tier | Hull | Factor | Scaled thrust | Scaled drive mass | Empty + drive | Thrust/reference mass | Reference acceleration |
|---:|---|---:|---:|---:|---:|---:|---:|
| 1 | Gunship | 1.00 | 92.886 MN | 102.00 t | 282.00 t | 329.4 N/kg | 33.59 g |
| 1 | Escort | 1.00 | 92.886 MN | 102.00 t | 452.00 t | 205.5 N/kg | 20.96 g |
| 1 | Corvette | 1.00 | 92.886 MN | 102.00 t | 502.00 t | 185.0 N/kg | 18.87 g |
| 1 | Frigate | 1.00 | 92.886 MN | 102.00 t | 702.00 t | 132.3 N/kg | 13.49 g |
| 2 | Monitor | 1.00 | 92.886 MN | 102.00 t | 802.00 t | 115.8 N/kg | 11.81 g |
| 2 | Destroyer | 1.00 | 92.886 MN | 102.00 t | 1,002.00 t | 92.7 N/kg | 9.45 g |
| 2 | Cruiser | 1.30 | 120.752 MN | 132.60 t | 1,132.60 t | 106.6 N/kg | 10.87 g |
| 2 | Battlecruiser | 1.50 | 139.329 MN | 153.00 t | 1,353.00 t | 103.0 N/kg | 10.50 g |
| 3 | Lancer | 1.72 | 159.764 MN | 175.44 t | 2,175.44 t | 73.4 N/kg | 7.49 g |
| 3 | Battleship | 1.75 | 162.551 MN | 178.50 t | 1,778.50 t | 91.4 N/kg | 9.32 g |
| 3 | Dreadnought | 2.00 | 185.772 MN | 204.00 t | 2,604.00 t | 71.3 N/kg | 7.27 g |
| 3 | Titan | 2.50 | 232.215 MN | 255.00 t | 3,455.00 t | 67.2 N/kg | 6.85 g |

These are deliberately **not complete designs**. Armor, propellant, reactor,
radiator, weapons, and utilities all increase mass, so the acceleration column
is an upper bound used to compare hulls under one consistent module assumption.
For powered drives, the scaled reactor and radiator burden will reduce actual
thrust-to-mass further and by a technology-dependent amount.

## Deferred follow-up

1. Measure engine-section meshes and thrust structure separately from nozzle
   resources; the current area proxy cannot determine reactor bay capacity.
2. Rebalance reactor specific mass and fixed mass before choosing any hull-size
   output or reactor-mass cap.
3. Test the provisional drive factors in the designer with chemical,
   nuclear-thermal, electric, fusion, and open-cycle drives, because reactor and
   radiator feedback differs sharply.
4. If tier 3 remains undernourished, prototype single-cell capacity discounts
   for large hull weapons before adding visual hardpoints, and design a separate
   size system for utilities rather than treating every utility as identical.
