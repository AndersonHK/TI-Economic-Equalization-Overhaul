# Gun and railgun progression

Last reviewed: 2026-07-29  
Game data: installed Terra Invicta 1.0.49 templates

## Scope and method

This report follows the three conventional weapons that form the most direct
size progression:

- 6-inch Gun Battery: one hull hardpoint
- 8-inch Gun Battery: two hull hardpoints
- 10-inch Cannon: one nose hardpoint

It then compares each with the railgun family that uses the same mount:

- 6-inch Gun Battery ↔ Light Railgun Battery Mk1–3
- 8-inch Gun Battery ↔ Railgun Battery Mk1–3
- 10-inch Cannon ↔ Light Rail Cannon Mk1–3

The source fields are from `TIGunTemplate.json`,
`TIMagneticGunTemplate.json`, and their English localization files. The
current mod changes only the 10-inch Cannon's crew from four to three; the
other performance values below remain vanilla.

### Later planning overlay

After this performance analysis was completed, the third planning slice
settled crew values of:

- two for the 6-inch and 8-inch gun batteries;
- two for Light Railgun Battery Mk1–3 and Railgun Battery Mk1–3;
- three for Light Rail Cannon Mk1–3.

Those decisions are recorded in the [planning changelog](../../CHANGELOG.md)
but are not yet implemented. The tables below preserve the installed/template
crew values used when the analysis was performed, except for the already
implemented 10-inch change. No weapon-performance value is changed by the
planning overlay.

Derived kinetic energy uses the mass that the game calls `warheadMass_kg`:

`impact energy = 0.5 × warhead mass × muzzle velocity²`

This reproduces the conventional guns' explicit `damage_MJ` values exactly.
Loaded mass is base weapon mass plus magazine rounds times `ammoMass_kg`.
Sustained conventional output assumes the listed cooldown occurs after each
salvo:

`cycle time = cooldown + (salvo shots - 1) × intra-salvo cooldown`

If runtime instead overlaps part of the cooldown with the salvo, actual
sustained output will be higher. The relative size progression is unchanged.

## Executive findings

1. The 6-, 8-, and 10-inch weapons fire at the same `1.4 km/s` and have the
   same `250 km` targeting range. Larger caliber improves impact energy and
   armor chipping, not hit probability or time of flight.
2. The 8-inch battery is not a clean two-slot upgrade over two 6-inch
   batteries. It is lighter and uses fewer crew, but two 6-inch batteries
   deliver about 13% more sustained impact energy.
3. The 10-inch Cannon buys the strongest individual conventional shell, but
   its loaded mass is extreme: approximately `179 t` for a one-nose weapon.
4. Mk1 railguns are primarily range and single-hit upgrades. Their slow firing
   cycles make them worse in sustained impact output than the corresponding
   conventional gun.
5. Mk2 remains a sidegrade in sustained output. Mk3 is the point where all
   three corresponding railgun families decisively overtake the conventional
   weapon.
6. Railgun range grows roughly with muzzle velocity. Maximum-range flight time
   therefore remains on the order of two to three minutes rather than becoming
   instant. Evasion remains central to kinetic-weapon performance.
7. Railgun electrical inefficiency creates a material reactor and radiator
   load. That is the principal cost that the conventional-gun table does not
   show.

## Conventional gun progression

| Weapon | Mount | Base / loaded mass | Crew | Shell / damaging mass | Velocity | Impact | Salvo | Full cycle | Sustained impact | Range | Flat chipping |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| 6-inch Gun Battery | 1 hull | 25 / **52 t** | 3 | 45 / 22.5 kg | 1.4 km/s | 22.05 MJ | 4 | 18 s | **4.90 MW** | 250 km | 0.15 |
| 8-inch Gun Battery | 2 hull | 50 / **90 t** | 4 | 100 / 50 kg | 1.4 km/s | 49.00 MJ | 4 | 22.5 s | **8.71 MW** | 250 km | 0.20 |
| 10-inch Cannon | 1 nose | 125 / **179 t** | **3 modded** | 180 / 90 kg | 1.4 km/s | 88.20 MJ | 3 | 22 s | **12.03 MW** | 250 km | 0.25 |

The localization is consistent with the mechanics:

- the 6-inch is a small, short-range naval turret adapted for spacecraft;
- the 8-inch is a larger turret firing heavier shells;
- the 10-inch is heavy naval artillery redesigned as a nose weapon.

### What caliber actually changes

Because velocity is fixed, energy scales directly with damaging projectile
mass:

| Comparison | Impact-energy multiplier | Momentum multiplier | Range multiplier |
|---|---:|---:|---:|
| 8-inch vs 6-inch | 2.22× | 2.22× | 1.00× |
| 10-inch vs 6-inch | 4.00× | 4.00× | 1.00× |
| 10-inch vs 8-inch | 1.80× | 1.80× | 1.00× |

This is a pure heavier-shell progression. There is no longer barrel, better
propellant, higher velocity, or improved fire-control progression.

### Slot and crew comparison

Two 6-inch batteries occupy the same two hull slots as one 8-inch battery:

| Two-slot package | Loaded mass | Crew | Sustained impact | Individual impact |
|---|---:|---:|---:|---:|
| Two 6-inch batteries | 104 t | 6 | **9.80 MW** | 22.05 MJ |
| One 8-inch battery | **90 t** | **4** | 8.71 MW | **49.00 MJ** |

The 8-inch battery is therefore a mass- and crew-efficient weapon for landing
larger hits, but not a raw damage-per-slot upgrade. That can be a legitimate
role distinction if larger individual impacts perform better against armor.

### Ammunition endurance

| Weapon | Rounds | Ammunition mass | Total magazine impact energy | Approx. continuous firing time |
|---|---:|---:|---:|---:|
| 6-inch | 600 | 27 t | 13.23 GJ | 45.0 min |
| 8-inch | 400 | 40 t | 19.60 GJ | 37.5 min |
| 10-inch | 300 | 54 t | 26.46 GJ | 36.7 min |

The larger weapons carry more total damaging energy but less firing time. All
three magazines are extremely deep for normal Terra Invicta engagements.

## Railgun analogues

### One-hull battery: 6-inch to Light Railgun Battery

| Weapon | Base / loaded mass | Crew | Efficiency | Damaging slug | Velocity | Impact | Cooldown | Sustained impact | Avg. waste heat | Range |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| 6-inch Gun Battery | 25 / 52 t | 3 | — | 22.5 kg | 1.4 km/s | 22.05 MJ | salvo | **4.90 MW** | negligible weapon heat | 250 km |
| Light Railgun Battery Mk1 | 50 / 62 t | 3 | 25% | 9.0 kg | 3.0 km/s | 40.50 MJ | 30 s | 1.35 MW | 4.05 MW | 500 km |
| Light Railgun Battery Mk2 | 45 / 57 t | 3 | 30% | 9.6 kg | 3.3 km/s | 52.27 MJ | 20 s | 2.61 MW | 6.10 MW | 550 km |
| Light Railgun Battery Mk3 | 40 / **52 t** | 3 | 35% | 10.5 kg | 3.6 km/s | 68.04 MJ | 10 s | **6.80 MW** | 12.64 MW | 600 km |

Mk1 fits the localization's “vast improvement” only if range and impact per hit
are valued more highly than sustained damage. Mk3 finally matches the loaded
mass of the 6-inch battery while providing 3.09 times the impact per shot and
1.39 times the sustained output.

### Two-hull battery: 8-inch to Railgun Battery

| Weapon | Base / loaded mass | Crew | Efficiency | Damaging slug | Velocity | Impact | Cooldown | Sustained impact | Avg. waste heat | Range |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| 8-inch Gun Battery | 50 / 90 t | 4 | — | 50 kg | 1.4 km/s | 49.00 MJ | salvo | **8.71 MW** | negligible weapon heat | 250 km |
| Railgun Battery Mk1 | 100 / 128.8 t | 4 | 25% | 18.0 kg | 3.3 km/s | 98.01 MJ | 30 s | 3.27 MW | 9.80 MW | 650 km |
| Railgun Battery Mk2 | 90 / 118.8 t | 4 | 30% | 19.2 kg | 3.6 km/s | 124.42 MJ | 20 s | 6.22 MW | 14.52 MW | 700 km |
| Railgun Battery Mk3 | 80 / 108.8 t | 4 | 35% | 21.0 kg | 4.32 km/s | 195.96 MJ | 10 s | **19.60 MW** | 36.39 MW | 750 km |

Mk1 doubles individual impact and more than doubles range, but delivers only
38% of the conventional battery's sustained impact output. Mk3 delivers four
times the individual impact and 2.25 times the sustained output, at the cost of
18.8 additional loaded tonnes and a substantial continuous heat burden.

### One-nose cannon: 10-inch to Light Rail Cannon

| Weapon | Base / loaded mass | Crew | Efficiency | Damaging slug | Velocity | Impact | Cooldown | Sustained impact | Avg. waste heat | Range |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| 10-inch Cannon | 125 / **179 t** | **3 modded** | — | 90 kg | 1.4 km/s | 88.20 MJ | salvo | 12.03 MW | negligible weapon heat | 250 km |
| Light Rail Cannon Mk1 | 60 / 66 t | 4 | 25% | 22.5 kg | 3.6 km/s | 145.80 MJ | 45 s | 3.24 MW | 9.72 MW | 550 km |
| Light Rail Cannon Mk2 | 55 / 61 t | 4 | 30% | 24.0 kg | 4.32 km/s | 223.95 MJ | 30 s | 7.46 MW | 17.42 MW | 600 km |
| Light Rail Cannon Mk3 | 50 / **56 t** | 4 | 35% | 26.25 kg | 5.18 km/s | 352.18 MJ | 15 s | **23.48 MW** | 43.60 MW | 650 km |

This is the sharpest technology discontinuity:

- even Mk1 has 1.65 times the impact and 2.2 times the range;
- every rail cannon is more than 110 tonnes lighter when loaded;
- Mk1 and Mk2 still lose sustained output because of long cooldowns;
- Mk3 has four times the impact, almost twice the sustained output, and less
  than one third of the loaded mass.

The 10-inch Cannon is therefore a credible transitional weapon, not a system
that remains competitive once mature rail cannon exist.

## Flight time and practical range

| Weapon | Maximum range | Velocity | Nominal time to maximum range |
|---|---:|---:|---:|
| Conventional 6/8/10-inch | 250 km | 1.4 km/s | 179 s |
| Light Railgun Battery Mk1 | 500 km | 3.0 km/s | 167 s |
| Light Railgun Battery Mk3 | 600 km | 3.6 km/s | 167 s |
| Railgun Battery Mk3 | 750 km | 4.32 km/s | 174 s |
| Light Rail Cannon Mk1 | 550 km | 3.6 km/s | 153 s |
| Light Rail Cannon Mk3 | 650 km | 5.18 km/s | 125 s |

The template ranges are tuned so that faster projectiles generally receive
longer permitted engagement ranges. Railguns improve close-range hit
probability, but shooting at the new maximum range still gives an alert target
minutes to maneuver.

## Power and thermal cost

For an idealized magnetic launcher:

`electrical input per shot = kinetic impact energy / efficiency`

`waste heat per shot = electrical input - kinetic impact energy`

The tables above average that waste over cooldown. Mk3 improves efficiency, but
its much higher firing rate causes the largest average heat load. A Light Rail
Cannon Mk3, for example, produces about `43.6 MW` of launcher waste heat while
delivering `23.5 MW` of average projectile kinetic power.

This is a desirable balancing mechanism. A railgun should not be judged only
by weapon mass: reactor input, radiator capacity, battery discharge, and
cooldown all belong to the installed weapon system.

## Balance assessment

### Coherent features

- Larger conventional shells cause proportionally larger hits and chipping.
- Turrets trade barrel alignment and velocity for wide firing arcs.
- Nose rail cannon are lighter and faster than equivalently slotted batteries.
- Railgun generations improve mass, efficiency, velocity, cooldown, and range
  together.
- Mk1 and Mk2 have meaningful transitional roles rather than being automatic
  damage upgrades.

### Values worth revisiting later

- The 10-inch Cannon's `125 t` base mass is exceptionally punitive relative to
  both the 8-inch battery and the first light rail cannon.
- The 8-inch battery is slightly weaker per hull slot than two 6-inch
  batteries. If armor mechanics do not reward its larger hits sufficiently,
  it has no damage-based niche.
- Mk1 railguns are described as dramatic improvements, but their sustained
  output is only 27–38% of the corresponding conventional weapon.
- Railgun crew is unchanged across generations even while fire control,
  loading, and power switching become more automated. This should eventually
  be reviewed under the same ship-level maintenance model as point defense.
- All conventional calibers have identical muzzle velocity and range. A modest
  barrel-length or fire-control progression could make the family feel less
  like a simple shell-mass ladder.

No weapon-performance values are settled by this report. The later crew
decisions are recorded in the planning overlay above.
