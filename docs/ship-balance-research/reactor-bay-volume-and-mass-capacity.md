# Reactor-bay volume and mass-capacity planning

Status: asset-measurement record and planning analysis. No gameplay rule is
implemented by this document.

Last reviewed: 2026-08-13

## Conclusions from the asset measurement

The human ship prefabs support treating the visibly separate aft section
surrounded by radiator hardware as an approximate reactor or machinery bay.
Every default human hull prefab contains exactly one active mesh with an
explicit `Radiators` name. It sits immediately forward of the separately
instantiated drive geometry.

This does **not** prove that the artists intended the complete interior to be a
reactor vessel. It does provide a repeatable, art-authored exterior envelope
that is substantially better evidence than measuring pixels in a combat
screenshot.

For the default Battlecruiser appearance, the named object is:

`Battlecruiser/Earth_Hull_Battlecruiser/Earth_Battlecruiser_Radiators`

Its visible mesh has the following prefab-local bounds:

- transverse width: `12.744 m`;
- transverse height: `11.925 m`;
- longitudinal length: `17.812 m`;
- elliptical-cylinder envelope: approximately `2,126 m3`;
- largest circular cylinder that fits the smaller transverse dimension:
  approximately `1,989 m3`;
- circular cylinder using the larger transverse dimension: approximately
  `2,272 m3`.

The same object also has a capsule collider measuring approximately
`11.600 x 11.600 x 22.130 m`. Interpreted as a true capsule it occupies about
`1,930 m3`; interpreted as its cylindrical bounding envelope it occupies about
`2,339 m3`. The visible mesh is the preferred reactor-bay evidence. Colliders
are authored for combat raycasting and sometimes bracket substantially more or
less space than the visible object.

The Battlecruiser therefore has a defensible **approximately 2,000-2,200 m3
outer reactor/machinery-section envelope** before deductions for shell and
frame structure, radiator roots and manifolds, separation hardware, access,
clearance, shielding, voids, and non-reactor machinery.

## Default human-hull measurements

The coordinate system has `x` and `y` as the transverse axes and `z` as the
longitudinal axis. The table uses the transformed axis-aligned bounds of the
active named radiator mesh.

Two cylinder values are retained:

- **inscribed circular cylinder** uses the smaller of the two transverse mesh
  bounds as its diameter. This is the safer limit for a circular package;
- **elliptical envelope** uses both transverse bounds. This is a useful upper
  description of the art envelope but is not necessarily usable by a circular
  reactor package.

| Hull | Named radiator mesh, X x Y x L | Inscribed circular cylinder | Elliptical-cylinder envelope | Elliptical ratio to Gunship |
|---|---:|---:|---:|---:|
| Gunship | 6.050 x 5.164 x 12.617 m | 264 m3 | 310 m3 | 1.00 |
| Escort | 6.050 x 5.164 x 12.617 m | 264 m3 | 310 m3 | 1.00 |
| Corvette | 6.050 x 5.164 x 12.617 m | 264 m3 | 310 m3 | 1.00 |
| Frigate | 6.092 x 5.200 x 15.648 m | 332 m3 | 389 m3 | 1.26 |
| Monitor | 6.050 x 5.164 x 18.363 m | 385 m3 | 451 m3 | 1.46 |
| Destroyer | 6.050 x 5.164 x 18.363 m | 385 m3 | 451 m3 | 1.46 |
| Cruiser | 12.744 x 11.925 x 17.812 m | 1,989 m3 | 2,126 m3 | 6.87 |
| Battlecruiser | 12.744 x 11.925 x 17.812 m | 1,989 m3 | 2,126 m3 | 6.87 |
| Lancer | 13.063 x 13.063 x 17.651 m | 2,366 m3 | 2,366 m3 | 7.64 |
| Battleship | 18.045 x 16.886 x 25.222 m | 5,648 m3 | 6,036 m3 | 19.50 |
| Dreadnought | 22.124 x 22.124 x 29.853 m | 11,476 m3 | 11,476 m3 | 37.07 |
| Titan | 24.692 x 24.692 x 33.319 m | 15,955 m3 | 15,956 m3 | 51.54 |

The repeated dimensions form recognizable art families:

- Gunship, Escort, and Corvette share one radiator-section mesh envelope;
- Monitor and Destroyer share a longer version of the same transverse scale;
- Cruiser and Battlecruiser share a much larger common envelope;
- the late large hulls then grow through Lancer, Battleship, Dreadnought, and
  Titan scales.

## Graphical-variant measurements selected for implementation

The initial table above describes appearance index 0 only. The runtime reactor
bay rule is keyed by `(hull dataName, resolved appearance index)`, using the same
`TISpaceShipTemplate.GetHullAppearanceIndex` value already supplied to drive
scaling. This distinction is material: several alternate and Dark Skies hulls
depict substantially larger or smaller aft machinery sections than the default
art for the same statistical hull.

The maintained raw dimensions and asset identifiers are in
[`reactor-bay-variant-volumes.csv`](reactor-bay-variant-volumes.csv). The
inscribed-cylinder volumes selected for gameplay are:

| Hull | Appearance 0 | Appearance 1 | Appearance 2 | Appearance 3 |
|---|---:|---:|---:|---:|
| Gunship | 264.241 m3 | 452.197 m3 | 317.310 m3 | 712.242 m3 |
| Escort | 264.241 m3 | 452.197 m3 | 317.310 m3 | 712.242 m3 |
| Corvette | 264.241 m3 | 452.197 m3 | 604.707 m3 | 837.588 m3 |
| Frigate | 332.341 m3 | 675.444 m3 | 1,246.492 m3 | 1,233.527 m3 |
| Monitor | 384.582 m3 | 675.444 m3 | 2,617.607 m3 | 2,028.675 m3 |
| Destroyer | 384.582 m3 | 675.444 m3 | 2,617.607 m3 | 2,028.675 m3 |
| Cruiser | 1,989.242 m3 | 1,384.984 m3 | 3,930.638 m3 | 3,505.550 m3 |
| Battlecruiser | 1,989.243 m3 | 1,384.984 m3 | 3,930.638 m3 | 3,505.550 m3 |
| Lancer | 2,365.773 m3 | 2,090.292 m3 | 10,223.879 m3 | 8,072.644 m3 |
| Battleship | 5,648.074 m3 | 2,090.292 m3 | 5,464.773 m3 | 6,945.700 m3 |
| Dreadnought | 11,476.330 m3 | 2,090.293 m3 | 10,223.879 m3 | 10,952.622 m3 |
| Titan | 15,955.576 m3 | 6,290.837 m3 | 16,549.539 m3 | 15,840.889 m3 |

Indices 0 and 1 come from the base `ships` bundle. Indices 2 and 3 come from
the Dark Skies `ships_prm` bundle. If that DLC bundle is unavailable, vanilla
resolves requested index 2 to 0 and index 3 to 1 before selecting the model;
the capacity lookup deliberately consumes that resolved index and therefore
follows the art actually instantiated.

For an alien, modded, future, or otherwise unmeasured `(hull, appearance)`
pair, use the largest measured variant in the corresponding vanilla size band:

| Runtime size band | Fallback bay volume |
|---|---:|
| Small | 2,617.607 m3 |
| Medium | 3,930.638 m3 |
| Large | 16,549.539 m3 |
| Huge | 16,549.539 m3 |

Huge saturates at the largest measured Titan bay rather than extrapolating
beyond available human art. A fallback is a compatibility policy, not a claim
that the unmeasured asset has that volume, and should emit a one-time runtime
diagnostic naming the hull, appearance, size band, and selected value.

The progression is deliberately stepped and nonlinear. For example, Titan's
outer elliptical envelope is about `51.5` times Gunship's. These measurements
are therefore evidence for art-authored machinery-capacity tiers, not a reason
to assume that reactor output or mass should automatically scale in direct
proportion to the measured volume.

## Volume formulas

For transformed mesh dimensions `X`, `Y`, and longitudinal length `L`:

```text
D = min(X, Y)

inscribed circular volume = pi / 4 * D^2 * L
elliptical envelope       = pi / 4 * X * Y * L
```

For the Battlecruiser:

```text
inscribed circular volume
  = pi / 4 * 11.925^2 * 17.812
  = 1,989 m3

elliptical envelope
  = pi / 4 * 12.744 * 11.925 * 17.812
  = 2,126 m3
```

The inscribed circular value is the recommended primary fit volume if reactor
packages remain abstract circular cylinders. The elliptical value should be
retained as a visible-envelope upper reference.

## How the measurements were obtained

The repository's existing
[`scripts/ship-balance/measure_ship_prefabs.py`](../../scripts/ship-balance/measure_ship_prefabs.py)
loads the installed Terra Invicta `ships` Unity asset bundle with UnityPy. It:

1. opens each default human-hull prefab listed in `PREFABS`;
2. recursively walks the active transform hierarchy;
3. composes local translation, rotation, and scale into prefab-local world
   matrices;
4. transforms the eight corners of every mesh's local axis-aligned bound;
5. records the resulting prefab-local bounds in metre-scale Unity coordinates;
6. separately records the layer-17 combat raycast colliders.

The reactor-bay query filters the resulting mesh records for paths containing
`radiator`, combines any matches, and computes the dimensions and cylinder
volumes above. In pseudocode:

```python
records = walk_hull_prefab_and_compose_transforms(prefab)
radiator_meshes = [
    mesh for mesh in records["meshes"]
    if "radiator" in mesh["path"].lower()
]

minimum = componentwise_min(mesh.points.min(axis=0) for mesh in radiator_meshes)
maximum = componentwise_max(mesh.points.max(axis=0) for mesh in radiator_meshes)
x, y, length = maximum - minimum

diameter = min(x, y)
inscribed_m3 = pi / 4 * diameter**2 * length
elliptical_m3 = pi / 4 * x * y * length
```

The current script already supplies the prefab loading, transform composition,
mesh bounds, and collider records. A future tooling change can add the radiator
filter and volume calculations as maintained output rather than relying on the
one-off query used for this report.

The asset bundle measured on 2026-08-12 was:

```text
D:\Games\SteamLibrary\steamapps\common\Terra Invicta\
  TerraInvicta_Data\StreamingAssets\AssetBundles\ships
```

The reported numbers are asset-space measurements, not screenshot pixel
estimates and not values derived from the statistical hull-template cylinder.

## Interpretation and limitations

The visible section is fully enclosed and visually separated from the crewed
forward hull. Nothing in the default external model suggests routine crew
access. It is therefore reasonable for planning to treat it as a densely packed
uncrewed machinery volume with robotic maintenance, remote connections, and
major servicing performed only in dock.

That interpretation permits a higher average installed density than a crewed
engine room with aisles and occupied workstations. It does not permit treating
the entire geometric envelope as homogeneous reactor material:

- the mesh is an exterior envelope, not an interior solid;
- radiator roots, pumps, manifolds, retraction or deployment hardware, and
  coolant interfaces consume some of the section even though deployed radiator
  panel mass is accounted separately in game mechanics;
- load paths and thrust structure cross or surround the aft section;
- pressure vessels, magnetic coils, neutron or gamma shielding, power
  conversion, controls, and feed systems have technology-specific packing
  fractions;
- the drive is instantiated as separate visible geometry aft of this section,
  but a zero or very small drive-template mass implies that some drive hardware
  may be economically carried by the power-plant mass;
- some of that drive-attributed mass is outside the measured cylinder and some
  thrust structure is properly hull mass rather than either reactor or drive
  mass;
- fuel cells are canonically rechargeable by externally attached solar arrays,
  so the complete fuel-cell power-system mass cannot be assumed to live inside
  this cylinder;
- alien and third-party hull appearances have not been directly measured;
- collider envelopes are useful cross-checks but are not reliable interior
  volumes. The Frigate collider is an especially obvious outlier relative to
  its visible radiator mesh.

The eventual cap should consequently distinguish three quantities:

1. **art envelope volume** from the named mesh;
2. **usable enclosed package volume** after a technology-dependent packing or
   occupancy factor;
3. **reported module mass**, only a fraction of which may physically occupy the
   enclosed bay when it also pays for external or aft drive hardware.

The following sections develop that mass-capacity model.

## What "density" means in this report

The density below is **effective installed package density in tonnes per cubic
metre**, not the density of fuel, salt, steel, uranium, or some other individual
material. It is:

```text
bay-contained plant mass / external volume occupied by that plant package
```

It therefore averages together dense structure and shielding with coolant
channels, vacuum, magnetic-field volume, insulation, plumbing, conversion
hardware, controls, and unavoidable clearances. A reactor with a dense core can
still have a low installed density if its vacuum chamber or magnets are large.

The ranges are planning estimates, not measured properties of fictional future
reactors. Their upper ends are intentionally generous because the depicted bay
is sealed, uncrewed, and separated from the rest of the hull. The model assumes:

- no routine crew aisle, workstation, or habitable access volume inside it;
- robotic inspection and remote operation;
- major servicing performed in dock after opening the enclosure;
- dense directional shielding and structural mass may raise average density;
- deployed radiator panels and their game-calculated mass remain separate;
- propellant tanks remain part of ship propellant mass, not reactor mass;
- the density already incorporates ordinary internal packing losses, so no
  second generic packing multiplier is applied.

### Engineering anchors

These anchors constrain the orders of magnitude without claiming that a future
ship reactor will reproduce any one terrestrial design:

- DOE's aggressive transportation fuel-cell stack targets are `2,000 W/kg`
  and `2,500 W/L`. Dividing volumetric by gravimetric power density implies a
  stack material density near `1.25 t/m3`, but DOE explicitly excludes storage,
  power electronics, electric drive, and thermal/water/air ancillaries. That is
  a useful upper anchor for a densely packed stack, not a complete regenerative
  system.
- The historical integrated XE-Prime nuclear-thermal engine was approximately
  `18.1 t`, `6.91 m` long, and `2.59 m` in diameter. Its cylindrical bounding
  envelope is about `36.4 m3`, giving only about `0.50 t/m3` even though it
  included the reactor, pressure vessel, nozzle, turbopump, and valves.
- NASA's SP-100 goal allocated `4.58 t` across reactor, shield, primary heat
  transport, conversion, controls, heat rejection, conditioning, and structure.
  Only `0.70 t` was the reactor itself. This demonstrates why neither fuel
  density nor bare-core density is a plant-density estimate. Radiator mass is
  removed from the proposed game model because Terra Invicta already calculates
  it separately.
- ORNL documents molten fluoride salts around `1.94-3.35 t/m3`, depending on
  composition and temperature. A salt-filled vessel can consequently be dense,
  but graphite, pumps, heat exchangers, piping, and voids prevent treating the
  whole plant as solid salt.
- ITER reports a `23,000 t` tokamak about `30 m` tall and `30 m` wide. A simple
  30 m diameter by 30 m cylindrical envelope averages about `1.08 t/m3` before
  counting its much larger external plant. Compact future magnets can improve
  this, but a toroidal reactor remains a vacuum-and-field machine rather than a
  solid block.
- LLNL describes NIF's 192-beam laser system as roughly three football fields
  long and 85 feet tall. Its `264,000 lb` target chamber alone is 10 m in
  diameter. Present inertial-confinement hardware is extraordinarily sparse;
  the upper range below assumes radical future miniaturization.
- Sandia's Z machine uses 36 large Marx generators, oil and water insulation,
  multi-metre pulse-forming capacitors, and a central vacuum chamber. This
  supports allowing a dense pulsed-power package while retaining substantial
  internal field, dielectric, and transmission-line volume.
- There is no demonstrated antimatter power reactor from which to obtain a
  density. NASA identifies production, long-term storage, and conversion to
  thrust as unresolved. The antimatter rows are explicitly generous balance
  placeholders for containment magnets, shielding, conversion, and feed
  hardware; they are not research-grounded forecasts.

## Effective installed-density estimates

The upper-bound column is the value used in the sample mass-cap calculations.
It should be understood as the densest complete enclosed package worth allowing
without contradicting the broad architecture. It is not a required minimum and
does not establish achievable power density.

| Game power-plant class | Reasonable planning range | Generous upper bound used | Reasoning |
|---|---:|---:|---|
| Fuel cell | 0.25-1.20 t/m3 | **1.20 t/m3** | Dense stacks can approach the DOE-derived 1.25 t/m3 anchor, while electrolyzer, water/reactant handling and tanks lower the complete internal average. Solar collection is external. |
| Solid-core fission | 0.40-2.50 t/m3 | **2.50 t/m3** | XE-Prime's integrated envelope was about 0.50 t/m3. A sealed future package with compact shielding can be much denser, but coolant channels and pressure-vessel volume remain. |
| Molten-salt fission | 1.00-3.50 t/m3 | **3.50 t/m3** | Hot salts themselves are roughly 2-3.35 t/m3; dense vessel, shield and machinery can raise a tightly fitted local average, while graphite and loops lower it. |
| Liquid/molten-core fission | 0.60-2.50 t/m3 | **2.50 t/m3** | Dense liquid fuel and containment coexist with pumps, separation, pressure boundaries and large thermal-flow passages. |
| Vapor/gas-core fission | 0.25-2.00 t/m3 | **2.00 t/m3** | The active fuel is low density and needs containment or field volume. Heavy pressure, shielding and conversion hardware keep the overall package from being extremely light. |
| Electrostatic-confinement fusion | 0.10-1.00 t/m3 | **1.00 t/m3** | Vacuum chamber and electrostatic grid volume dominate; the upper end assumes very compact high-field hardware and dense shielding. |
| Mirror magnetic-confinement fusion | 0.15-1.20 t/m3 | **1.20 t/m3** | Linear vacuum volume and end magnets inhibit dense packing; shielding and superconducting structure provide much of the mass. |
| Toroidal magnetic-confinement fusion | 0.40-2.00 t/m3 | **2.00 t/m3** | ITER's machine-only bounding density is about 1.08 t/m3. The upper end grants compact future magnets and a tightly enclosed uncrewed package. |
| Hybrid-confinement fusion | 0.30-2.00 t/m3 | **2.00 t/m3** | The game does not define one physical layout. Use the broad magnetic-fusion envelope until each hybrid design receives a specific architecture. |
| Z-pinch fusion | 0.40-2.50 t/m3 | **2.50 t/m3** | Capacitor, switch and transmission hardware can be dense, but insulation, field spacing, vacuum and pulse-forming geometry occupy substantial volume. |
| Inertial-confinement fusion | 0.10-1.50 t/m3 | **1.50 t/m3** | Current laser facilities are extremely sparse. The upper end assumes compact drivers, rapid target handling and shielding far beyond demonstrated integration. |
| Antimatter plasma core | 0.40-2.50 t/m3 | **2.50 t/m3** | Placeholder dominated by traps, magnets, shielding, conversion and failure containment rather than the negligible fuel volume. |
| Antimatter beam core | 0.50-3.00 t/m3 | **3.00 t/m3** | Deliberately generous placeholder for a compact beamed annihilation system; no experimental plant validates it. |

Alien hybrid reactors share the hybrid-confinement class but are outside this
human-hull exercise. Their art, technology and balance should be measured
separately rather than inheriting human bay limits automatically.

## Separating enclosed mass from reported module mass

The game calculates power-plant mass as:

```text
reported plant mass = max(1 t, requested gross output in GW * specificPower_tGW)
```

`specificPower_tGW` is therefore tonnes per GW despite its name.

Many representative drives have `0 t` flat drive mass and no
power-proportional drive-mass charge. Reading the power-plant result as a bare
reactor would make their nozzles, pumps, magnets, pulse-forming hardware, feed
systems and thrust interfaces physically massless. For this planning model,
some reported plant mass may consequently live aft of the cylinder as drive
hardware or elsewhere on the hull as support structure.

Define:

```text
Vbay        = inscribed circular bay volume in m3
rho         = generous installed density in t/m3
f_bay       = fraction of reported module mass physically inside the bay
M_enclosed  = f_bay * M_reported

M_reported,max = Vbay * rho / f_bay
```

The last expression is the game-facing mass cap. A lower `f_bay` permits a
larger reported module mass because more of that mass is attributed to external
solar equipment, aft drive hardware, distributed power conditioning, or other
non-bay structure.

This is an accounting model, not permission to make hardware disappear. Every
kilogram excluded from the bay must still be conceptually located somewhere on
the ship. Some thrust-frame mass may properly belong to base hull mass rather
than to either module. A future implementation should avoid charging the same
hardware once in hull mass, again in drive mass, and a third time in plant mass.

### Generous attribution cases used for samples

| Selected drive and plant | `f_bay` used | Plausible planning band | Why part of the reported mass is outside the cylinder |
|---|---:|---:|---|
| Grid Drive + Fuel Cell III | **0.25** | 0.25-0.50 | Canonical regenerative solar array is externally attached; electric-drive hardware, array deployment, distribution and some conditioning are outside the bay. This is the most generous case. |
| Nerva Drive + Solid Core Fission I | **0.50** | 0.45-0.65 | Zero drive mass means the integrated nozzle, turbopumps, valves and thrust hardware must be carried somewhere. Core, reflector and directional shield remain in or against the bay. |
| Pegasus Drive + Molten Core Fission III | **0.55** | 0.50-0.70 | Fluid-core feed, nozzle and aft containment form part of the drive, while much of the core plant and shielding remain enclosed. |
| Pegasus Drive + Molten Salt Fission II | **0.55** | 0.50-0.70 | The runtime explicitly permits molten-salt plants to substitute for liquid-core plants. Pegasus still has zero drive mass, so feed, nozzle and thrust hardware require the same substantial off-bay attribution. |
| Firestar Fission Lantern + Gas Core Fission VI | **0.45** | 0.40-0.60 | A fission lantern is strongly integrated with its aft propulsive hardware; the zero-mass drive entry makes a low bay share reasonable. |
| Zeta Deuteron Torch + Flow-Stabilized Z-Pinch | **0.60** | 0.55-0.75 | Pulsed-power plant and chamber remain enclosed, while magnetic nozzle, feed and thrust interfaces extend aft. |
| Protium Converter Torch + Inertial Fusion VII | **0.60** | 0.55-0.75 | Drivers, chamber and conversion occupy the bay; target feed, magnetic exhaust and thrust hardware account for a material external share. |
| Pion Torch + Antimatter Beam Core | **0.40** | 0.35-0.55 | The beam-handling and magnetic-nozzle architecture is inseparable from aft propulsion hardware. The number is a generous fictional allocation, not measured engineering. |

For an ordinary closed-cycle electric drive with a separately credible drive
mass, a safer default would be `f_bay = 0.70-0.85`. The low sample fractions
should not become a universal loophole.

## Current plant ranges after the mod override

The following combines the installed-game inventory in `powerplant.csv` with
the current overrides in `TIEconomyMod/ModFiles/TIPowerPlantTemplate.json`.
Alien hybrid plants are excluded.

| Power-plant class | Current output range | Current t/GW range | Full-rating mass range |
|---|---:|---:|---:|
| Fuel cell | 0.2-1.5 GW | 960-5,600 t/GW | 1,120-2,880 t |
| Solid-core fission | 0.75-60 GW | 8-160 t/GW | 18-1,920 t |
| Molten-salt fission | 40-400 GW | 8-10 t/GW | 400-3,200 t |
| Liquid/molten-core fission | 4-200 GW | 12-16 t/GW | 64-2,400 t |
| Vapor/gas-core fission | 6.5-1,650 GW | 4-20 t/GW | 52-11,550 t |
| Electrostatic-confinement fusion | 46-310 GW | 0.005-1 t/GW | 1.55-46 t |
| Mirror magnetic-confinement fusion | 120-256 GW | 0.8-5 t/GW | 204.8-860 t |
| Toroidal magnetic-confinement fusion | 128-5,060 GW | 0.1-4 t/GW | 506-802 t |
| Hybrid-confinement fusion | 180-11,370 GW | 0.05-2 t/GW | 360-950 t |
| Z-pinch fusion | 260-7,590 GW | 0.0068-3 t/GW | 51.6-3,514 t |
| Inertial-confinement fusion | 370-306,430 GW | 0.002-4 t/GW | 612.9-4,772.5 t |
| Antimatter plasma core | 1,200-66,000 GW | 0.004-0.4 t/GW | 264-480 t |
| Antimatter beam core | 3,000,000 GW | 0.00002 t/GW | 60 t |

The ranges expose why a volume rule cannot repair the whole progression by
itself. At the late end, the current specific masses are so small that even the
smallest bay can hold many full-rating plants according to the linear formula.
The output ratings and t/GW values remain independent balance variables and,
as already concluded in `powerplant-benchmarks.md`, are liable to become much
heavier in later realism passes.

## Sample maximum reported module mass by hull

This table applies the generous density and `f_bay` values above to each hull's
inscribed circular bay volume. Values are maximum **game-reported plant-module
mass**, not mass physically inside the cylinder. Enclosed mass at the cap is
simply `Vbay * rho`. Displayed masses are rounded to the nearest tonne.

| Hull | Circular bay | Grid + FC III | Nerva + SC I | Pegasus + MC III | Firestar + GC VI | Zeta + FS Z-pinch | Protium + ICF VII | Pion + AM beam |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Gunship | 264 m3 | 1,267 t | 1,320 t | 1,200 t | 1,173 t | 1,100 t | 660 t | 1,980 t |
| Escort | 264 m3 | 1,267 t | 1,320 t | 1,200 t | 1,173 t | 1,100 t | 660 t | 1,980 t |
| Corvette | 264 m3 | 1,267 t | 1,320 t | 1,200 t | 1,173 t | 1,100 t | 660 t | 1,980 t |
| Frigate | 332 m3 | 1,594 t | 1,660 t | 1,509 t | 1,476 t | 1,383 t | 830 t | 2,490 t |
| Monitor | 385 m3 | 1,848 t | 1,925 t | 1,750 t | 1,711 t | 1,604 t | 963 t | 2,888 t |
| Destroyer | 385 m3 | 1,848 t | 1,925 t | 1,750 t | 1,711 t | 1,604 t | 963 t | 2,888 t |
| Cruiser | 1,989 m3 | 9,547 t | 9,945 t | 9,041 t | 8,840 t | 8,288 t | 4,973 t | 14,918 t |
| Battlecruiser | 1,989 m3 | 9,547 t | 9,945 t | 9,041 t | 8,840 t | 8,288 t | 4,973 t | 14,918 t |
| Lancer | 2,366 m3 | 11,357 t | 11,830 t | 10,755 t | 10,516 t | 9,858 t | 5,915 t | 17,745 t |
| Battleship | 5,648 m3 | 27,110 t | 28,240 t | 25,673 t | 25,102 t | 23,533 t | 14,120 t | 42,360 t |
| Dreadnought | 11,476 m3 | 55,085 t | 57,380 t | 52,164 t | 51,004 t | 47,817 t | 28,690 t | 86,070 t |
| Titan | 15,955 m3 | 76,584 t | 79,775 t | 72,523 t | 70,911 t | 66,479 t | 39,888 t | 119,663 t |

These large values are not proposed reactor masses. They are generous geometric
ceilings designed to answer whether a reported mass could plausibly be located
within and around the depicted aft section. They should always be combined with
a plant's output ceiling, t/GW formula, minimum critical size, shielding model,
and drive architecture.

## Selected current drive/reactor pairings

All selected drive templates currently have zero flat drive mass and zero
power-proportional drive mass. That makes them useful stress tests for the
mass-attribution model. Drive power is the current one-thruster requested plant
output before the human hull multiplier.

| Selected pairing | Drive power x1 | Current plant max | Current t/GW | Plant mass at full rating | Requested mass across hull factors 1.00-2.50 | Output compatibility |
|---|---:|---:|---:|---:|---:|---|
| Grid + Fuel Cell III | 1.105 GW | 1.5 GW | 960 t/GW | 1,440 t | 1,061-2,652 t theoretical | Gunship through Cruiser only; larger factors exceed output before volume |
| Nerva + Solid Core Fission I | 0.283 GW | 1 GW | 160 t/GW | 160 t | 45.3-113.2 t | All listed hulls |
| Pegasus + Molten Core Fission III | 65.882 GW | 200 GW | 12 t/GW | 2,400 t | 790.6-1,976.5 t | All listed hulls |
| Firestar Fission Lantern + Gas Core Fission VI | 147.059 GW | 1,650 GW | 4 t/GW | 6,600 t | 588.2-1,470.6 t | All listed hulls |
| Zeta Deuteron Torch + Flow-Stabilized Z-Pinch | 1,263.411 GW | 7,590 GW | 0.0068 t/GW | 51.6 t | 8.6-21.5 t | All listed hulls |
| Protium Converter Torch + Inertial Fusion VII | 51,070.694 GW | 306,430 GW | 0.002 t/GW | 612.9 t | 102.1-255.4 t | All listed hulls |
| Pion Torch + Antimatter Beam Core | 73,747.495 GW | 3,000,000 GW | 0.00002 t/GW | 60 t | 1.47-3.69 t | All listed hulls |

The hull factors used are the currently implemented values: `1.00` through
Destroyer, `1.30` Cruiser, `1.50` Battlecruiser, `1.72` Lancer, `1.75`
Battleship, `2.00` Dreadnought, and `2.50` Titan. The factor scales powered-drive
requirement and therefore plant mass.

### Volume headroom at each selected fit

Each cell is:

```text
maximum reported module mass / current requested module mass
```

`OUT` means the plant's current maximum output is exceeded before geometry is
considered.

| Hull | Grid + FC III | Nerva + SC I | Pegasus + MC III | Firestar + GC VI | Zeta + FS Z-pinch | Protium + ICF VII | Pion + AM beam |
|---|---:|---:|---:|---:|---:|---:|---:|
| Gunship | 1.2x | 29.2x | 1.5x | 2.0x | 128.2x | 6.5x | 1,343.7x |
| Escort | 1.2x | 29.2x | 1.5x | 2.0x | 128.2x | 6.5x | 1,343.7x |
| Corvette | 1.2x | 29.2x | 1.5x | 2.0x | 128.2x | 6.5x | 1,343.7x |
| Frigate | 1.5x | 36.7x | 1.9x | 2.5x | 161.2x | 8.1x | 1,689.8x |
| Monitor | 1.7x | 42.5x | 2.2x | 2.9x | 186.5x | 9.4x | 1,955.6x |
| Destroyer | 1.7x | 42.5x | 2.2x | 2.9x | 186.5x | 9.4x | 1,955.6x |
| Cruiser | 6.9x | 169.0x | 8.8x | 11.6x | 742.2x | 37.5x | 7,781.4x |
| Battlecruiser | OUT | 146.5x | 7.6x | 10.0x | 643.2x | 32.5x | 6,743.9x |
| Lancer | OUT | 151.9x | 7.9x | 10.4x | 667.0x | 33.7x | 6,993.6x |
| Battleship | OUT | 356.4x | 18.6x | 24.4x | 1,565.4x | 79.0x | 16,412.3x |
| Dreadnought | OUT | 633.6x | 33.0x | 43.4x | 2,783.0x | 140.4x | 29,178.2x |
| Titan | OUT | 704.7x | 36.7x | 48.2x | 3,095.2x | 156.2x | 32,451.8x |

The result is diagnostic:

- Fuel Cell III nearly fills the small-hull allowance even after assigning 75%
  of its reported mass outside the bay. Its output ceiling, not bay volume,
  rejects the Battlecruiser and larger hull-scaled Grid fits.
- Molten Core III now reaches a `2,400 t` full-rating mass. The generous
  small-bay Pegasus cap remains about `1,201 t`, so geometry limits that pairing
  to about `100 GW` at the current `12 t/GW`. The selected one-drive fit uses
  about `66 GW` before hull scaling.
- Gas Core VI's full-rating mass is now `6,600 t`. The small-hull Firestar cap
  is about `1,174 t`, giving a geometry ceiling near `294 GW` at the current
  `4 t/GW`. The selected one-drive fit itself uses about `147 GW`.
- Solid-core output ceilings bind long before these generous geometric caps.
  Solid IV only marginally reaches the bay ceiling on the three smallest hulls.
  Solid V is the first standard solid-core plant for which volume produces a
  useful set of distinct hull/drive examples, so it receives the focused table
  below; Solid I-IV are not enumerated separately.
- Current late Z-pinch, inertial-fusion and antimatter specific masses make the
  geometry almost irrelevant. A Pion plant requesting tens of thousands of GW
  weighs only a few tonnes. Increasing their t/GW values is prerequisite to a
  meaningful reactor-bay constraint.

## Solid Core Fission Reactor V: largest drive-load examples

The rebalanced Solid Core Fission Reactor V has:

| Maximum output | Specific mass | Efficiency | Full-rating mass |
|---:|---:|---:|---:|
| 60 GW | 32 t/GW | 67.5% | 1,920 t |

For this table, the solid-core planning assumptions remain `2.5 t/m3` and
`f_bay = 0.50`. The maximum game-reported module mass is consequently five
times the inscribed circular bay volume:

```text
Solid V reported mass allowance = Vbay * 2.5 / 0.50 = Vbay * 5 t/m3
geometry-derived output ceiling = reported mass allowance / 32 t/GW
effective ceiling               = min(60 GW, geometry-derived ceiling)
```

"Largest" means the compatible drive configuration with the highest unscaled
`req power` in the current `drives.csv`, after multiplying that demand by the
hull's implemented drive factor. This selects the fit that loads Solid V most
heavily. It does not mean highest thrust, latest technology, or best combat
drive. Unlock chronology is deliberately ignored: this is a physical and
runtime compatibility exercise across all drive templates that require the
`Solid_Core_Fission` class.

| Hull | Hull factor | Solid V mass allowance | Geometry ceiling | Binding ceiling | Highest-power fitting drive | Scaled demand | Plant mass | Allowance used |
|---|---:|---:|---:|---|---|---:|---:|---:|
| Gunship | 1.00 | 1,321 t | 41.29 GW | bay | Heavy Dumbo x2 | 39.070 GW | 1,250.2 t | 94.6% |
| Escort | 1.00 | 1,321 t | 41.29 GW | bay | Heavy Dumbo x2 | 39.070 GW | 1,250.2 t | 94.6% |
| Corvette | 1.00 | 1,321 t | 41.29 GW | bay | Heavy Dumbo x2 | 39.070 GW | 1,250.2 t | 94.6% |
| Frigate | 1.00 | 1,662 t | 51.92 GW | bay | Heavy Dumbo x2 | 39.070 GW | 1,250.2 t | 75.2% |
| Monitor | 1.00 | 1,923 t | 60.09 GW | 60 GW output | Heavy Dumbo x3 | 58.604 GW | 1,875.3 t | 97.5% |
| Destroyer | 1.00 | 1,923 t | 60.09 GW | 60 GW output | Heavy Dumbo x3 | 58.604 GW | 1,875.3 t | 97.5% |
| Cruiser | 1.30 | 9,947 t | 310.84 GW | 60 GW output | Heavy Dumbo x2 | 50.791 GW | 1,625.3 t | 16.3% |
| Battlecruiser | 1.50 | 9,947 t | 310.84 GW | 60 GW output | Heavy Dumbo x2 | 58.605 GW | 1,875.4 t | 18.9% |
| Lancer | 1.72 | 11,828 t | 369.63 GW | 60 GW output | Advanced Pulsar Drive x6 | 44.032 GW | 1,409.0 t | 11.9% |
| Battleship | 1.75 | 28,242 t | 882.56 GW | 60 GW output | Advanced Pulsar Drive x6 | 44.800 GW | 1,433.6 t | 5.1% |
| Dreadnought | 2.00 | 57,382 t | 1,793.19 GW | 60 GW output | Advanced Pulsar Drive x6 | 51.200 GW | 1,638.4 t | 2.9% |
| Titan | 2.50 | 79,775 t | 2,492.95 GW | 60 GW output | Advanced Pulsar Drive x5 | 53.333 GW | 1,706.6 t | 2.1% |

Four examples show how to read the table:

- **Gunship:** Heavy Dumbo x2 requests `39.070 GW` and gives a `1,250.2 t`
  plant. Heavy Dumbo x3 requests `58.604 GW`, below the reactor's nominal
  `60 GW` output but above the Gunship bay's `41.29 GW` geometry ceiling. This
  is a genuinely volume-limited pairing.
- **Monitor:** Heavy Dumbo x3 requests `58.604 GW` and gives a `1,875.3 t`
  plant, consuming `97.5%` of the generous mass allowance. Both constraints are
  close, but the plant's `60 GW` output is slightly lower than the bay-derived
  `60.09 GW` ceiling.
- **Battlecruiser:** hull scaling turns Heavy Dumbo x2's base `39.070 GW` into
  `58.605 GW`, producing a `1,875.4 t` plant. Heavy Dumbo x3 would require
  `87.906 GW`, so output rejects it even though the bay could accommodate much
  more mass.
- **Titan:** the `2.50` hull factor makes Advanced Pulsar x5 request
  `53.333 GW`. Its `1,706.6 t` plant uses only `2.1%` of the very large bay
  allowance; Solid V's output is overwhelmingly the constraint.

The apparent inversion on large hulls is a consequence of the current hull
drive multiplier: the same drive resource becomes more powerful and more
power-hungry on a larger hull. A Titan therefore reaches the fixed `60 GW`
plant ceiling with a lower unscaled drive configuration than a Monitor. The
bay itself is not forcing the Titan down to Advanced Pulsar x5.

## Molten Salt Fission Reactor II + Pegasus follow-up

This is a valid game pairing. `Pegasus Drive` declares
`Liquid_Core_Fission`, but the runtime compatibility rule contains an explicit
exception allowing a `Molten_Salt_Core_Fission` plant to power drives requiring
either solid-core or liquid-core fission. The same exception is used when the
designer builds its list of valid drives.

The rebalanced Molten Salt Fission Reactor II has:

| Maximum output | Specific mass | Efficiency | Full-rating mass |
|---:|---:|---:|---:|
| 400 GW | 8 t/GW | 75% | 3,200 t |

The follow-up uses the molten-salt upper density of `3.5 t/m3` and the existing
Pegasus `f_bay = 0.55`. This is generous: 45% of the reported module mass may
be in the zero-mass drive, nozzle, feed system, thrust interface, or other
off-bay hardware.

```text
MS II reported mass allowance  = Vbay * 3.5 / 0.55
geometry-derived output ceiling = reported mass allowance / 8 t/GW
effective ceiling               = min(400 GW, geometry-derived ceiling)
```

The candidate set here is deliberately limited to Pegasus x1-x6. The table
selects the largest Pegasus cluster whose hull-scaled demand fits both the bay
and the plant output.

| Hull | Hull factor | MS II mass allowance | Geometry ceiling | Binding ceiling | Largest Pegasus cluster | Scaled demand | Plant mass | Allowance used |
|---|---:|---:|---:|---|---|---:|---:|---:|
| Gunship | 1.00 | 1,682 t | 210.20 GW | bay | Pegasus x3 | 197.647 GW | 1,581.2 t | 94.0% |
| Escort | 1.00 | 1,682 t | 210.20 GW | bay | Pegasus x3 | 197.647 GW | 1,581.2 t | 94.0% |
| Corvette | 1.00 | 1,682 t | 210.20 GW | bay | Pegasus x3 | 197.647 GW | 1,581.2 t | 94.0% |
| Frigate | 1.00 | 2,115 t | 264.34 GW | bay | Pegasus x4 | 263.529 GW | 2,108.2 t | 99.7% |
| Monitor | 1.00 | 2,447 t | 305.93 GW | bay | Pegasus x4 | 263.529 GW | 2,108.2 t | 86.1% |
| Destroyer | 1.00 | 2,447 t | 305.93 GW | bay | Pegasus x4 | 263.529 GW | 2,108.2 t | 86.1% |
| Cruiser | 1.30 | 12,660 t | 1,582.47 GW | 400 GW output | Pegasus x4 | 342.588 GW | 2,740.7 t | 21.6% |
| Battlecruiser | 1.50 | 12,660 t | 1,582.47 GW | 400 GW output | Pegasus x4 | 395.294 GW | 3,162.3 t | 25.0% |
| Lancer | 1.72 | 15,054 t | 1,881.74 GW | 400 GW output | Pegasus x3 | 339.953 GW | 2,719.6 t | 18.1% |
| Battleship | 1.75 | 35,944 t | 4,493.02 GW | 400 GW output | Pegasus x3 | 345.882 GW | 2,767.1 t | 7.7% |
| Dreadnought | 2.00 | 73,032 t | 9,128.94 GW | 400 GW output | Pegasus x3 | 395.294 GW | 3,162.4 t | 4.3% |
| Titan | 2.50 | 101,531 t | 12,691.41 GW | 400 GW output | Pegasus x2 | 329.413 GW | 2,635.3 t | 2.6% |

This produces stronger bay binding than Solid V:

- Gunship, Escort, and Corvette fit Pegasus x3. Pegasus x4 remains below the
  reactor's `400 GW` rating at 1x scaling but exceeds their `210.20 GW` bay
  ceiling.
- Frigate fits Pegasus x4 by only about `6.5 t` of reported mass under this
  model. Pegasus x5 is rejected by volume.
- Monitor and Destroyer also stop at Pegasus x4; their `305.93 GW` geometry
  ceiling rejects Pegasus x5's `329.412 GW` demand.
- Cruiser and larger hulls have ample volume. Their selected cluster is set by
  the `400 GW` output cap after hull scaling.

Pegasus x6 illustrates the interaction especially well. At 1x scaling it asks
for `395.294 GW` and produces a `3,162.4 t` plant, nearly maximizing Molten Salt
II without exceeding its output. None of the existing hulls can actually use
that combination under the current model:

- all six 1x-scaling hulls have bays too small for it;
- Cruiser and every larger bay has enough volume, but the hull multiplier
  pushes Pegasus x6 above `400 GW`.

At the generous `3.5 t/m3` density, a 1x Pegasus x6 fit would require the bay
to contain no more than approximately `29.2%` of reported mass on
Gunship/Escort/Corvette, `36.8%` on Frigate, or `42.6%` on Monitor/Destroyer.
The planning assumption is `55%`. Allowing x6 on one of those small hulls would
therefore require attributing most of a nominally `3,162 t` plant outside the
reactor enclosure, which is possible as a game abstraction but no longer the
most plausible reading of the art.

## Recommended interpretation for a later implementation

Do not implement one rule of the form `reactor mass <= density * bay volume`
without attribution and technology context. A safer sequence is:

1. Key the maintained bay-volume table by human hull and resolved appearance.
2. Give each plant architecture an effective installed-density range and use a
   reviewed upper bound for permissive fit validation.
3. Give each drive/plant architecture a reported-mass bay fraction, or better,
   move identifiable propulsion mass into the drive template and use a higher,
   more stable plant bay fraction.
4. Calculate `M_reported,max = Vbay * rho / f_bay`.
5. Convert that to a geometry-derived output ceiling with
   `M_reported,max / specificPower_tGW`.
6. Apply the lower of geometry-derived output, plant `maxOutput_GW`, and any
   future critical-core or repeated-train ceiling.
7. Continue calculating radiator mass separately from plant efficiency and
   waste heat.
8. Show the limiting reason in the ship designer: plant output, hull bay,
   minimum reactor train, or incompatible drive class.

The density and bay-fraction values in this report are intentionally generous
research defaults. Before implementation they need a balance decision on
whether the goal is merely to prevent visually absurd fits or to impose a much
stronger technology progression.

## Reproducibility and sources

Repository inputs:

- [`measure_ship_prefabs.py`](../../scripts/ship-balance/measure_ship_prefabs.py)
  for asset transforms, mesh bounds and collider extraction;
- [`powerplant.csv`](powerplant.csv) for the installed-game plant inventory;
- [`TIPowerPlantTemplate.json`](../../TIEconomyMod/ModFiles/TIPowerPlantTemplate.json)
  for current mod overrides;
- [`drives.csv`](drives.csv) for drive requirements and mass fields;
- [`human-hull-slots-and-drive-scaling.md`](human-hull-slots-and-drive-scaling.md)
  for current human hull factors;
- [`powerplant-benchmarks.md`](powerplant-benchmarks.md) and
  [`drive-reactor-pairing-and-hull-geometry.md`](drive-reactor-pairing-and-hull-geometry.md)
  for the existing physical and runtime audit.

Primary engineering anchors:

- [DOE transportation fuel-cell system and stack targets](https://www.energy.gov/cmei/fuels/doe-technical-targets-fuel-cell-systems-and-stacks-transportation-applications)
- [NASA regenerative fuel-cell architecture](https://ntrs.nasa.gov/api/citations/20160004090/downloads/20160004090.pdf?attachment=true)
- [NASA XE-Prime/NERVA review](https://ntrs.nasa.gov/api/citations/19920001919/downloads/19920001919.pdf)
- [NASA SP-100 mass decomposition](https://ntrs.nasa.gov/api/citations/19890003294/downloads/19890003294.pdf)
- [ORNL molten-salt reactor fuel-loop data](https://info.ornl.gov/sites/publications/Files/Pub135162.pdf)
- [ORNL candidate molten-salt properties](https://info.ornl.gov/sites/publications/Files/Pub110694.pdf)
- [ITER machine dimensions and mass](https://www.iter.org/sites/default/files/media/a1_iter_fusion_machinev2.pdf)
- [LLNL NIF User Guide](https://nifuserguide.llnl.gov/sites/nifuserguide/files/2024-08/NIF-User-Guide-Revised-8-9-2024.pdf)
- [Sandia Z machine architecture](https://www.sandia.gov/z-machine/about-z/how-z-works/)
- [NASA overview of antimatter enabling problems](https://ntrs.nasa.gov/api/citations/20100026039/downloads/20100026039.pdf?attachment=true)
