# Hull-volume fuel capacity implementation

## Scope

The ship designer will expose the hull and engine information that is currently
implicit in the selected 3D appearance, and it will limit propellant tanks by
the remaining measured main-hull envelope.

The maintained hull-volume source is
`tables/hull-variant-volume-and-slots.csv`. Its main-hull selection excludes the
drive subtree and named reactor, radiator, engine, and thruster meshes. These
axis-aligned elliptical envelopes are gameplay allowances, not claims that the
art is a watertight or fully pressurized solid.

## Capacity model

For the selected `(hull, appearance)` pair:

```text
module_volume = utility_cells * 200 m3
              + hull_weapon_cells * 250 m3
              + nose_weapon_cells * 400 m3

fuel_volume = ceil(max(0,
    measured_main_hull_volume
    - module_volume
    - total_crew * 50 m3))

volume_per_100_tank = 100000 kg / propellant_density_kg_per_m3
maximum_tanks = floor(fuel_volume / volume_per_100_tank)
```

`total_crew` is the complete current design crew, including hull, drive,
reactor, radiator, weapons, and utility modules. Multi-slot weapons use their
vanilla `internalSize`; multi-slot utilities use the EEO footprint registry.
The separate drive, reactor, radiator, and armor slots are not counted in
`module_volume`, because the measured main-hull selection excludes the named
aft machinery and the requested formula reserves only modules and crew.

The default bulk-density table is:

| Game propellant | Modeled material | Density (kg/m3) | Volume per 100 t (m3) |
|---|---|---:|---:|
| `Hydrogen` | liquid hydrogen | 70.85 | 1,411.43 |
| `Water` | liquid water | 997.00 | 100.30 |
| `NobleGases` | liquid xenon | 2,942.00 | 33.99 |
| `Volatiles` | liquid methane | 422.60 | 236.63 |
| `Metals` | liquid lithium | 534.00 | 187.27 |
| `ReactionProducts` | water-equivalent dense reaction mass | 1,000.00 | 100.00 |
| `Anything` | water-equivalent fallback | 1,000.00 | 100.00 |

The density lookup supports an optional positive `propellantDensity_kgm3`
extension on an individual `TIDriveTemplate` record. This permits later
drive-family distinctions without expanding the game's broad `Propellant`
enum or changing saved designs.

## Designer behavior

The 3D model pane receives a compact two-line overlay:

```text
Drive scale: De Laval 4.150x | Magnetic 2.859x | Pulsed 1x | Engine bay 2,464.7 m³ | Hull mass 964 t | Repair crew 12
Fuel Hydrogen | 4 / 31 tanks | 44,130 m³ available
```

The two `Drive scale` values are measured graphical x6-nozzle envelope ratios
for the selected human art variant, normalized independently to the default
Gunship De Laval and Magnetic resources. They describe model scale only and do
not replace EEO's approved gameplay drive multiplier. Alien art has one shared
thruster family, so both measured labels use its appearance scale. Pulsed art
remains at its authored 1x scale. `Engine bay` is the
selected art variant's measured reactor/engine-bay volume, using the same
measurement and fallback path as the reactor-bay capacity feature. `Hull mass`
uses the selected human appearance's authored flat structural mass. `Repair crew`
is the hull template's base repair complement (`TIShipHullTemplate.crew`), not
the complete fitted design crew shown in the performance panel and used by the
fuel-volume equation.

The tank count is clamped after direct tank edits and before every designer
performance refresh. That covers drive replacement, drive removal/reinstall,
module and crew changes, loading an existing design, `OnCycleAltHull`, and
`SetAltHull`. The propellant spinner is then refreshed to the clamped value and
shows the current maximum.

Unknown or malformed measured-volume data uses the hull template's runtime
cylinder as a logged fallback. A missing drive does not destroy a provisional
tank count; the count is reconciled as soon as a drive supplies a density.

## AI planning boundary

Player-designer reconciliation does not by itself make AI-generated templates
capacity-aware. The implemented minimum AI integration adds a deterministic
early appearance lock, capped ideal-tank delta-v, direct alien/fighter loop
handling, reactor/engine-bay legality checks, and a final save invariant. The
full behavior and its deferred alternatives are specified in the
[minimum AI fuel-capacity plan](ai-fuel-capacity-minimum-plan.md).

A more elaborate role-aware design that ranks graphical appearances for each
candidate drive and introduces controlled top-two randomness is preserved as
[hypothetical future work](ai-hull-appearance-selection-hypothetical.md). It is
not required for the minimum enforcement pass.

## Verification

- Formula tests cover ceiling order, crew/module subtraction, density
  conversion, liquid-hydrogen capacity, water-equivalent capacity, capped
  rocket-equation delta-v, and invalid inputs.
- The hull-report validator requires runtime hull-volume data to match all 64
  documented appearance rows.
- Harmony validation requires the designer refresh, tank edit, appearance,
  spinner-label, AI appearance/context, ideal-tank, completed-design, alien,
  fighter, and save-invariant hooks. Installed-game IL validation requires
  exactly three alien tank setters, one capacity-aware alien target clamp, one
  capacity-aware target floor, and one STO tank setter.
- Normal deployment runs the complete build and verification suite before
  copying the mod.
