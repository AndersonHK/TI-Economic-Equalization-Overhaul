# Orbital defense power and armor check

November 22, 2026. This compares the proposed human defense modules with installed Terra Invicta 1.0.65 and EEO 0.11.19. The proposed armor points remain 5, 10, and 15. The loadout sizes come from the [mount and clearance proposal](clearance.md). The tables below preserve the comparisons behind the deferred draws and masses. The [current 0.12 build](implementation-0.12.md) uses the pre-0.12 power and base mass.

## Hab power units

The game sums integer hab power through the module and station power methods. The hab UI and official power documentation use the unit `Power`. Ship weapons and reactors separately use GJ per shot and MW/GW. I found no native conversion or published canonical value for one hab point. The earlier EEO solar study used roughly 1 MW per power point as a physical interpretation for industry and solar equipment. Treat 1, 10, and 100 MW per point as scenarios when comparing with weapons. [Official Power documentation](https://wiki.hoodedhorse.com/Terra_Invicta/Power) · [EEO solar power audit](../../hab-economy/solar-power-and-t1-lab-audit.md#what-one-hab-power-means)

| Proposed module | Hab power | At 1 MW per point | At 10 MW per point | At 100 MW per point |
| --- | ---: | ---: | ---: | ---: |
| T1 Point Defense Array | 40 | 40 MW | 400 MW | 4 GW |
| T2 Layered Defense Array | 500 | 500 MW | 5 GW | 50 GW |
| T3 Battlestations | 3,800 | 3.8 GW | 38 GW | 380 GW |

In the installed EEO grid one Fission Pile yields 40 power and one Heavy Fusion Reactor Array yields 600. Two Heavy Fusion Reactor Farms supply 3,600 power, so a single proposed T3 needs another 200 before budgeting for the rest of its station. A third heavy farm makes 5,400 total.

### Electrical demand at full cadence

Average MW is electrical MJ per shot divided by average seconds per shot. For a salvo of N shots, the average shot interval is `(cooldown + (N − 1) × intra-salvo delay) ÷ N`. Laser input is emitted MJ divided by efficiency. Magnetic input uses total accelerated ammunition mass and muzzle velocity, divided by efficiency. Plasma includes charging energy and the kinetic energy of the plasma. Naval cannon figures use EEO's modeled electrical consumption for the feed and handling systems.

| Tier | Green arc battery | Railgun Mk2 battery | Green phaser battery | Coilgun Mk3 battery | Other example |
| --- | ---: | ---: | ---: | ---: | --- |
| T1 size-1 | 16.7 MW | 42.4 MW | 25.0 MW | 120.7 MW | 6-inch naval cannon at 0.15 MW |
| T2 size-2 | 25.0 MW | 72.0 MW | 37.5 MW | 226.0 MW | 8-inch naval cannon at 0.25 MW |
| T3 size-4 | 41.7 MW | 74.6 MW | 62.5 MW | 569.3 MW | Heavy Plasma Battery Mk3 at 235 MW |

Dedicated PD averages 35.7 MW for arc laser, 37.0 MW for phaser, and 0.262 MW for EEO's 30 mm autocannon. The basic PD laser averages 40.0 MW. Representative shot inputs include 500 MJ for the 120 cm green arc laser and 3,643 MJ for the heavy Mk3 coilgun. The averages are electrical recharge demand across repeated volleys. A synchronized volley draws from stored energy at a higher instantaneous rate.

| Tier | Illustrative laser/magnetic split | Green arc + Mk2 rail + arc PD | Green phaser + Mk3 coil + phaser PD | Required MW per hab point for those two cases |
| --- | --- | ---: | ---: | ---: |
| T1 | 3 lasers, 2 magnetic, 6 PD | 349 MW | 539 MW | 8.72 / 13.47 |
| T2 | 13 lasers, 12 magnetic, 12 PD | 1.618 GW | 3.644 GW | 3.24 / 7.29 |
| T3 | 23 lasers, 23 magnetic, 20 PD | 3.390 GW | 15.271 GW | 0.89 / 4.02 |

Each top/bottom pair contributes one active weapon. The calculation assumes every group fires continuously at its template cadence, including PD, and leaves out module support loads. The split is illustrative because a final weapon mix has yet to be specified. A T1 with five naval cannons and six 30 mm PD needs only about 2.32 MW for weapons, while six arc PD alone use 214 MW at full cadence. At 10 MW per point, the proposed draws cover the mid-tech examples and the late T2/T3 examples. The late T1 example needs about 54 points before support loads at that conversion. The native station weapons have a module operability gate and no battery recharge budget tied to the hab power number.

## Hull size and armor

The following are native art dimensions from the installed hulls. The dimensions use the metre convention from the ship mesh audit. The projected area is the convex silhouette viewed perpendicular to the deck. It captures the rough size of the area exposed on that axis, with some enclosed empty space counted inside the convex outline. Station measurements use the isolated module shell including connectors and the T3 towers. Ship measurements use the base appearance main hull with attached weapons and separate engine meshes excluded. [Same-scale mesh silhouettes](module-ship-silhouettes.png)

| Hull | Length × width × height | Convex deck projection |
| --- | --- | ---: |
| T1 Point Defense Array | 24.6 × 31.5 × 8.5 | 618 m² |
| Escort | 39.3 × 14.9 × 14.5 | 532 m² |
| T2 Layered Defense Array | 87.3 × 91.6 × 21.3 | 7,323 m² |
| Monitor | 71.6 × 16.6 × 18.4 | 1,030 m² |
| T3 Battlestations | 145.7 × 151.6 × 67.5 | 20,176 m² |
| Battleship | 170.3 × 28.7 × 27.4 | 4,593 m² |
| Dreadnought | 264.0 × 43.6 × 30.1 | 10,160 m² |

T1 exposes about 1.2 times the Escort's plan area, T2 about 7.1 times the Monitor's, and T3 about twice the Dreadnought's. Those are space claims from meshes. The native armor routine does not use those areas to choose armor points.

Station orbital armor is a fixed all-around `5 × tier`. At full HP every eligible hit gets the armor check. At half HP the station applies its armor to half of incoming hits on average. Ships use separate nose, side and tail values and coverage. In the campaign combat autosave, 215 active human ships had a median **15 nose / 1 per side / 2 tail**, with up to 80 at the nose. Thus station armor is light against heavily armored noses and substantial against typical side armor in that snapshot. [Ship armor rules](../armor-system-as-is-2026-11-04.md)

| Tier | Fixed orbital armor | HP at current mass | HP at proposed mass | Nominal nanotube thickness | Two flat covers of the deck silhouette |
| --- | ---: | ---: | ---: | ---: | ---: |
| T1 | 5 | 66 | 244 | 0.39 m | ~835 t |
| T2 | 10 | 166 | 2,064 | 0.79 m | ~19,800 t |
| T3 | 15 | 888 | 13,941 | 1.18 m | ~81,800 t |

HP uses the native formula `truncate(0.3 × baseMass_tons / tier / mass_damagePoint_kg)` with the same EEO nanotube material held fixed. HP therefore rises sharply even with the original armor ratings. The plate comparison applies the game's ship-material mapping to two hypothetical flat covers. It excludes sidewalls, weapon equipment and supports. It puts a physical scale on the protection values, while the game assigns module armor and HP without buying plates by mesh area.

### Inputs checked

The electrical values use merged installed weapon templates and the installed assembly methods for beam, magnetic and plasma energy usage. EEO naval gun input uses [WeaponPowerMath](../../../TIEconomyMod/Core/WeaponPowerMath.cs) and the installed gun override. The module power and armor values were checked against `TIHabModuleState.ModulePower`, `WeaponCanFire`, `TIHabModuleTemplate.StationModuleArmorPoints`, `BaseStationModuleHitPoints`, and `CombatHabModuleController.ApplyDamage`. Mesh bounds and projected convex hulls came from the six installed module/ship prefabs under comparison plus the Battleship reference.
