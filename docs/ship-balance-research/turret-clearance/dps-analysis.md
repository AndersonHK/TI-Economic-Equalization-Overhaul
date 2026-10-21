# Hull-weapon DPS and capacity deltas

All outputs are **raw sustained MJ/s**: muzzle kinetic energy for guns/magnetics/plasma or emitted energy for lasers/particles. Divide by 20 for nominal damage-points/s before armor and other target effects. This is a transparent capacity comparison, not expected delivered damage or the game’s weighted `EstimateDPS`. Nose weapons are unchanged and excluded; whole-ship percentage changes would be smaller when nose output is included.

## Calculation and baseline definitions

`cycle = cooldown + (salvoShots − 1) × intraSalvoCooldown`; `rawDPS = shotEnergy × salvoShots / cycle`. Kinetic shot energy is `0.5 × warheadMass_kg × muzzleVelocity_kps²` MJ. This uses warhead mass, not loaded ammo mass. Native `averageCooldown_s` and `TIGunTypeWeaponTemplate.BaseDamageAtRange_MJ` supply these rules. Laser/particle shotPower is emitted energy; no range loss, hit chance, penetration, chipping, PD interception, relative projectile velocity, officers, ammo depletion or cooling/power downtime is simulated.

**Stock** uses installed vanilla weapon statistics and stock grid footprints. **Current** uses repository merged weapon statistics, the same adjacency packing, and the already implemented FourHull-per-node policy for Titan, AlienTitan and AlienMothership. Packing is an exact maximum over mixed sizes within one weapon family and tier, allowing small weapons to fill remaining cells. Current baseline is a hypothetical homogeneous family design, not a claim that the AI selects it. Families without size 4 have zero eligible current capital loadout. **2/node** and **4/node** each put that size on all ordinary logical nodes; these are hypothetical fixed-size loadouts, not an “up to size” optimization. Deltas compare against Current. A geometry failure does not remove a DPS row: feasibility is reported separately.

## Same-family size scaling

| Family | Faction | D1 | D2 | D4 | D2 vs D1 | D4 vs D2 | D4 vs D1 | D2 / 2D1 | D4 / 4D1 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | Human | 4.91 | — | — | — | — | — | — | — |
| 40mmAutocannon | Human | 13.36 | — | — | — | — | — | — | — |
| Naval cannon | Human | 8.71 | 15.68 | — | +80.0% | — | — | 0.90 | — |
| RailgunBatteryMk1 | Human | 5.91 | 10.21 | 7.78 | +72.9% | -23.8% | +31.7% | 0.86 | 0.33 |
| RailgunBatteryMk2 | Human | 10.16 | 17.28 | 17.92 | +70.0% | +3.7% | +76.3% | 0.85 | 0.44 |
| RailgunBatteryMk3 | Human | 19.85 | 40.82 | 56.35 | +105.7% | +38.0% | +183.9% | 1.03 | 0.71 |
| CoilgunBatteryMk1 | Human | 13.07 | 32.95 | 88.93 | +152.1% | +169.9% | +580.6% | 1.26 | 1.70 |
| CoilgunBatteryMk2 | Human | 30.83 | 69.34 | 180.00 | +124.9% | +159.6% | +483.9% | 1.12 | 1.46 |
| CoilgunBatteryMk3 | Human | 65.01 | 139.22 | 350.66 | +114.1% | +151.9% | +439.4% | 1.07 | 1.35 |
| AlienMagBattery | Alien | 70.56 | 156.52 | 354.01 | +121.8% | +126.2% | +401.7% | 1.11 | 1.25 |
| AdvancedAlienMagBattery | Alien | 187.43 | 388.73 | 910.42 | +107.4% | +134.2% | +385.7% | 1.04 | 1.21 |
| Gen3AlienMagBattery | Alien | 502.53 | 1,497.69 | 3,326.54 | +198.0% | +122.1% | +562.0% | 1.49 | 1.65 |
| PointDefenseLaserTurret | Human | 10.00 | — | — | — | — | — | — | — |
| IRLaserBattery | Human | 3.33 | 5.00 | 8.33 | +50.0% | +66.7% | +150.0% | 0.75 | 0.62 |
| PointDefenseArcLaserTurret | Human | 12.50 | — | — | — | — | — | — | — |
| IRArcLaserBattery | Human | 5.00 | 7.50 | 12.50 | +50.0% | +66.7% | +150.0% | 0.75 | 0.62 |
| PointDefensePhaserTurret | Human | 16.67 | — | — | — | — | — | — | — |
| IRPhaserBattery | Human | 10.00 | 15.00 | 25.00 | +50.0% | +66.7% | +150.0% | 0.75 | 0.62 |
| GreenLaserBattery | Human | 3.33 | 5.00 | 8.33 | +50.0% | +66.7% | +150.0% | 0.75 | 0.62 |
| GreenArcLaserBattery | Human | 5.00 | 7.50 | 12.50 | +50.0% | +66.7% | +150.0% | 0.75 | 0.62 |
| GreenPhaserBattery | Human | 10.00 | 15.00 | 25.00 | +50.0% | +66.7% | +150.0% | 0.75 | 0.62 |
| UVLaserBattery | Human | 3.33 | 5.00 | 8.33 | +50.0% | +66.7% | +150.0% | 0.75 | 0.62 |
| UVArcLaserBattery | Human | 5.00 | 7.50 | 12.50 | +50.0% | +66.7% | +150.0% | 0.75 | 0.62 |
| UVPhaserBattery | Human | 10.00 | 15.00 | 25.00 | +50.0% | +66.7% | +150.0% | 0.75 | 0.62 |
| AlienPointDefenseLaserTurret | Alien | 26.67 | — | — | — | — | — | — | — |
| AlienOrangeLaserBattery | Alien | 8.00 | 12.00 | 15.00 | +50.0% | +25.0% | +87.5% | 0.75 | 0.47 |
| AlienVioletLaserBattery | Alien | 10.67 | 16.00 | 20.00 | +50.0% | +25.0% | +87.5% | 0.75 | 0.47 |
| AlienXaserBattery | Alien | — | — | 10.00 | — | — | — | — | — |
| PointDefenseE-Beamer | Human | 1.25 | — | — | — | — | — | — | — |
| E-BeamBattery | Human | 2.05 | 4.10 | 8.21 | +100.0% | +100.0% | +300.0% | 1.00 | 1.00 |
| PointDefenseIonBattery | Human | 1.67 | — | — | — | — | — | — | — |
| IonBattery | Human | 2.05 | 4.10 | 8.21 | +100.0% | +100.0% | +300.0% | 1.00 | 1.00 |
| ParticleBeamBattery | Human | — | 5.52 | 11.03 | — | +100.0% | — | — | — |
| AntimatterPointDefenseBattery | Human | — | 13.33 | 26.67 | — | +100.0% | — | — | — |
| AlienPointDefenseParticleBeam | Alien | 16.00 | — | — | — | — | — | — | — |
| PlasmaBatteryMk1 | Human | — | 4.50 | 7.11 | — | +58.0% | — | — | — |
| PlasmaBatteryMk2 | Human | — | 5.40 | 8.53 | — | +58.0% | — | — | — |
| PlasmaBatteryMk3 | Human | — | 6.75 | 10.67 | — | +58.0% | — | — | — |
| AlienPlasmaBattery | Alien | — | 13.23 | — | — | — | — | — | — |
| AlienHeavyPlasmaBattery | Alien | — | — | 18.38 | — | — | — | — | — |


## Slot-packing and nominal capacity

Counts are weapons, not capacity units. A size-4 weapon uses four ordinary cells under stock rules only when those cells form the legal rectangle. A 3-cell hull therefore need not fit any size 4. The capital override retains the logical node count but permits size 4 at each node.

| Class | Tier | Nodes H | Stock 1H | Stock 2H horizontal | Stock 2H vertical | Stock 4H | Current 4H | Proposed size-2 units | Proposed size-4 units |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Gunship | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| Escort | 1 | 2 | 2 | 0 | 1 | 0 | 0 | 4 | 8 |
| Corvette | 1 | 1 | 1 | 0 | 0 | 0 | 0 | 2 | 4 |
| Frigate | 1 | 2 | 2 | 1 | 0 | 0 | 0 | 4 | 8 |
| Monitor | 2 | 4 | 4 | 2 | 0 | 0 | 0 | 8 | 16 |
| Destroyer | 2 | 2 | 2 | 1 | 0 | 0 | 0 | 4 | 8 |
| Cruiser | 2 | 3 | 3 | 1 | 0 | 0 | 0 | 6 | 12 |
| Battlecruiser | 2 | 2 | 2 | 1 | 0 | 0 | 0 | 4 | 8 |
| Battleship | 3 | 6 | 6 | 2 | 3 | 1 | 1 | 12 | 24 |
| Lancer | 3 | 3 | 3 | 1 | 0 | 0 | 0 | 6 | 12 |
| Dreadnought | 3 | 8 | 8 | 4 | 4 | 2 | 2 | 16 | 32 |
| Titan | 3 | 6 | 6 | 2 | 3 | 1 | 6 | 12 | 24 |
| STOFighter | 1 | 1 | 1 | 0 | 0 | 0 | 0 | 2 | 4 |
| AlienGunship | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| AlienEscort | 1 | 2 | 2 | 1 | 0 | 0 | 0 | 4 | 8 |
| AlienCorvette | 1 | 1 | 1 | 0 | 0 | 0 | 0 | 2 | 4 |
| AlienFrigate | 1 | 3 | 3 | 1 | 0 | 0 | 0 | 6 | 12 |
| AlienMonitor | 2 | 4 | 4 | 2 | 2 | 1 | 1 | 8 | 16 |
| AlienDestroyer | 2 | 2 | 2 | 1 | 0 | 0 | 0 | 4 | 8 |
| AlienCruiser | 2 | 4 | 4 | 2 | 2 | 1 | 1 | 8 | 16 |
| AlienBattlecruiser | 2 | 3 | 3 | 1 | 0 | 0 | 0 | 6 | 12 |
| AlienBattleship | 2 | 6 | 6 | 2 | 3 | 1 | 1 | 12 | 24 |
| AlienLancer | 2 | 4 | 4 | 2 | 0 | 0 | 0 | 8 | 16 |
| AlienDreadnought | 2 | 8 | 8 | 4 | 4 | 2 | 2 | 16 | 32 |
| AlienTitan | 3 | 8 | 8 | 4 | 4 | 2 | 8 | 16 | 32 |
| AlienAssaultCarrier | 3 | 6 | 6 | 2 | 0 | 0 | 0 | 12 | 24 |
| AlienMothership | 3 | 16 | 16 | 8 | 8 | 4 | 16 | 32 | 64 |
| SalamanderGunship | 1 | 1 | 1 | 0 | 0 | 0 | 0 | 2 | 4 |


## Tier-2 example: Mk3 coilgun batteries

For the tier-2 examples Destroyer and Cruiser, medium Mk3 coil output is 139.21875 MJ/s per weapon and heavy is 350.659375 MJ/s. Both classes pass the native size-4 5 m conservative gap test in every appearance. The per-class tables below quantify output against the best current mixed-footprint family loadout; this differs from a comparison against all-small guns.

### Gunship: 0 ordinary nodes, tier 1

All-appearance conservative maximum at 5 m: **no nodes**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | 0.00 | 0.00 | — | — | — | — | — |
| 40mmAutocannon | 0.00 | 0.00 | — | — | — | — | — |
| Naval cannon | 0.00 | 0.00 | — | 0.00 | — | — | — |
| RailgunBatteryMk1 | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| RailgunBatteryMk2 | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| RailgunBatteryMk3 | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| CoilgunBatteryMk1 | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| CoilgunBatteryMk2 | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| CoilgunBatteryMk3 | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| PointDefenseLaserTurret | 0.00 | 0.00 | — | — | — | — | — |
| IRLaserBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| PointDefenseArcLaserTurret | 0.00 | 0.00 | — | — | — | — | — |
| IRArcLaserBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| PointDefensePhaserTurret | 0.00 | 0.00 | — | — | — | — | — |
| IRPhaserBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| GreenLaserBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| GreenArcLaserBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| GreenPhaserBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| UVLaserBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| UVArcLaserBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| UVPhaserBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| PointDefenseE-Beamer | 0.00 | 0.00 | — | — | — | — | — |
| E-BeamBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| PointDefenseIonBattery | 0.00 | 0.00 | — | — | — | — | — |
| IonBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| ParticleBeamBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| AntimatterPointDefenseBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| PlasmaBatteryMk1 | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| PlasmaBatteryMk2 | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| PlasmaBatteryMk3 | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |


### Escort: 2 ordinary nodes, tier 1

All-appearance conservative maximum at 5 m: **none certified**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | 7.50 | 9.81 | +30.8% | — | — | — | — |
| 40mmAutocannon | 31.40 | 26.72 | -14.9% | — | — | — | — |
| Naval cannon | 9.80 | 17.42 | +77.8% | 31.36 | +80.0% | — | — |
| RailgunBatteryMk1 | 2.70 | 11.81 | +337.5% | 20.42 | +72.9% | 15.55 | +31.7% |
| RailgunBatteryMk2 | 5.23 | 20.33 | +288.9% | 34.56 | +70.0% | 35.83 | +76.3% |
| RailgunBatteryMk3 | 13.61 | 39.69 | +191.7% | 81.65 | +105.7% | 112.70 | +183.9% |
| CoilgunBatteryMk1 | 7.59 | 26.13 | +244.1% | 65.89 | +152.1% | 177.87 | +580.6% |
| CoilgunBatteryMk2 | 15.55 | 61.65 | +296.4% | 138.69 | +124.9% | 360.00 | +483.9% |
| CoilgunBatteryMk3 | 28.94 | 130.02 | +349.3% | 278.44 | +114.1% | 701.32 | +439.4% |
| PointDefenseLaserTurret | 20.00 | 20.00 | +0.0% | — | — | — | — |
| IRLaserBattery | 6.67 | 6.67 | +0.0% | 10.00 | +50.0% | 16.67 | +150.0% |
| PointDefenseArcLaserTurret | 25.00 | 25.00 | +0.0% | — | — | — | — |
| IRArcLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| PointDefensePhaserTurret | 33.33 | 33.33 | +0.0% | — | — | — | — |
| IRPhaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| GreenLaserBattery | 6.67 | 6.67 | +0.0% | 10.00 | +50.0% | 16.67 | +150.0% |
| GreenArcLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| GreenPhaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| UVLaserBattery | 6.67 | 6.67 | +0.0% | 10.00 | +50.0% | 16.67 | +150.0% |
| UVArcLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| UVPhaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| PointDefenseE-Beamer | 2.50 | 2.50 | +0.0% | — | — | — | — |
| E-BeamBattery | 4.10 | 4.10 | +0.0% | 8.21 | +100.0% | 16.41 | +300.0% |
| PointDefenseIonBattery | 3.33 | 3.33 | +0.0% | — | — | — | — |
| IonBattery | 4.10 | 4.10 | +0.0% | 8.21 | +100.0% | 16.41 | +300.0% |
| ParticleBeamBattery | 0.00 | 0.00 | — | 11.03 | — | 22.07 | — |
| AntimatterPointDefenseBattery | 0.00 | 0.00 | — | 26.67 | — | 53.33 | — |
| PlasmaBatteryMk1 | 0.00 | 0.00 | — | 9.00 | — | 14.22 | — |
| PlasmaBatteryMk2 | 0.00 | 0.00 | — | 10.80 | — | 17.07 | — |
| PlasmaBatteryMk3 | 0.00 | 0.00 | — | 13.50 | — | 21.33 | — |


### Corvette: 1 ordinary nodes, tier 1

All-appearance conservative maximum at 5 m: **4***. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | 3.75 | 4.91 | +30.8% | — | — | — | — |
| 40mmAutocannon | 15.70 | 13.36 | -14.9% | — | — | — | — |
| Naval cannon | 4.90 | 8.71 | +77.8% | 15.68 | +80.0% | — | — |
| RailgunBatteryMk1 | 1.35 | 5.91 | +337.5% | 10.21 | +72.9% | 7.78 | +31.7% |
| RailgunBatteryMk2 | 2.61 | 10.16 | +288.9% | 17.28 | +70.0% | 17.92 | +76.3% |
| RailgunBatteryMk3 | 6.80 | 19.85 | +191.7% | 40.82 | +105.7% | 56.35 | +183.9% |
| CoilgunBatteryMk1 | 3.80 | 13.07 | +244.1% | 32.95 | +152.1% | 88.93 | +580.6% |
| CoilgunBatteryMk2 | 7.78 | 30.83 | +296.4% | 69.34 | +124.9% | 180.00 | +483.9% |
| CoilgunBatteryMk3 | 14.47 | 65.01 | +349.3% | 139.22 | +114.1% | 350.66 | +439.4% |
| PointDefenseLaserTurret | 10.00 | 10.00 | +0.0% | — | — | — | — |
| IRLaserBattery | 3.33 | 3.33 | +0.0% | 5.00 | +50.0% | 8.33 | +150.0% |
| PointDefenseArcLaserTurret | 12.50 | 12.50 | +0.0% | — | — | — | — |
| IRArcLaserBattery | 5.00 | 5.00 | +0.0% | 7.50 | +50.0% | 12.50 | +150.0% |
| PointDefensePhaserTurret | 16.67 | 16.67 | +0.0% | — | — | — | — |
| IRPhaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| GreenLaserBattery | 3.33 | 3.33 | +0.0% | 5.00 | +50.0% | 8.33 | +150.0% |
| GreenArcLaserBattery | 5.00 | 5.00 | +0.0% | 7.50 | +50.0% | 12.50 | +150.0% |
| GreenPhaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| UVLaserBattery | 3.33 | 3.33 | +0.0% | 5.00 | +50.0% | 8.33 | +150.0% |
| UVArcLaserBattery | 5.00 | 5.00 | +0.0% | 7.50 | +50.0% | 12.50 | +150.0% |
| UVPhaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| PointDefenseE-Beamer | 1.25 | 1.25 | +0.0% | — | — | — | — |
| E-BeamBattery | 2.05 | 2.05 | +0.0% | 4.10 | +100.0% | 8.21 | +300.0% |
| PointDefenseIonBattery | 1.67 | 1.67 | +0.0% | — | — | — | — |
| IonBattery | 2.05 | 2.05 | +0.0% | 4.10 | +100.0% | 8.21 | +300.0% |
| ParticleBeamBattery | 0.00 | 0.00 | — | 5.52 | — | 11.03 | — |
| AntimatterPointDefenseBattery | 0.00 | 0.00 | — | 13.33 | — | 26.67 | — |
| PlasmaBatteryMk1 | 0.00 | 0.00 | — | 4.50 | — | 7.11 | — |
| PlasmaBatteryMk2 | 0.00 | 0.00 | — | 5.40 | — | 8.53 | — |
| PlasmaBatteryMk3 | 0.00 | 0.00 | — | 6.75 | — | 10.67 | — |


### Frigate: 2 ordinary nodes, tier 1

All-appearance conservative maximum at 5 m: **none certified**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | 7.50 | 9.81 | +30.8% | — | — | — | — |
| 40mmAutocannon | 31.40 | 26.72 | -14.9% | — | — | — | — |
| Naval cannon | 9.80 | 17.42 | +77.8% | 31.36 | +80.0% | — | — |
| RailgunBatteryMk1 | 3.27 | 11.81 | +261.6% | 20.42 | +72.9% | 15.55 | +31.7% |
| RailgunBatteryMk2 | 6.22 | 20.33 | +226.8% | 34.56 | +70.0% | 35.83 | +76.3% |
| RailgunBatteryMk3 | 19.60 | 40.82 | +108.3% | 81.65 | +100.0% | 112.70 | +176.1% |
| CoilgunBatteryMk1 | 10.94 | 32.95 | +201.3% | 65.89 | +100.0% | 177.87 | +439.9% |
| CoilgunBatteryMk2 | 21.17 | 69.34 | +227.6% | 138.69 | +100.0% | 360.00 | +419.1% |
| CoilgunBatteryMk3 | 37.80 | 139.22 | +268.3% | 278.44 | +100.0% | 701.32 | +403.8% |
| PointDefenseLaserTurret | 20.00 | 20.00 | +0.0% | — | — | — | — |
| IRLaserBattery | 6.67 | 6.67 | +0.0% | 10.00 | +50.0% | 16.67 | +150.0% |
| PointDefenseArcLaserTurret | 25.00 | 25.00 | +0.0% | — | — | — | — |
| IRArcLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| PointDefensePhaserTurret | 33.33 | 33.33 | +0.0% | — | — | — | — |
| IRPhaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| GreenLaserBattery | 6.67 | 6.67 | +0.0% | 10.00 | +50.0% | 16.67 | +150.0% |
| GreenArcLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| GreenPhaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| UVLaserBattery | 6.67 | 6.67 | +0.0% | 10.00 | +50.0% | 16.67 | +150.0% |
| UVArcLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| UVPhaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| PointDefenseE-Beamer | 2.50 | 2.50 | +0.0% | — | — | — | — |
| E-BeamBattery | 4.10 | 4.10 | +0.0% | 8.21 | +100.0% | 16.41 | +300.0% |
| PointDefenseIonBattery | 3.33 | 3.33 | +0.0% | — | — | — | — |
| IonBattery | 4.10 | 4.10 | +0.0% | 8.21 | +100.0% | 16.41 | +300.0% |
| ParticleBeamBattery | 5.52 | 5.52 | +0.0% | 11.03 | +100.0% | 22.07 | +300.0% |
| AntimatterPointDefenseBattery | 13.33 | 13.33 | +0.0% | 26.67 | +100.0% | 53.33 | +300.0% |
| PlasmaBatteryMk1 | 4.50 | 4.50 | +0.0% | 9.00 | +100.0% | 14.22 | +216.0% |
| PlasmaBatteryMk2 | 5.40 | 5.40 | +0.0% | 10.80 | +100.0% | 17.07 | +216.0% |
| PlasmaBatteryMk3 | 6.75 | 6.75 | +0.0% | 13.50 | +100.0% | 21.33 | +216.0% |


### Monitor: 4 ordinary nodes, tier 2

All-appearance conservative maximum at 5 m: **none certified**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | 15.01 | 19.63 | +30.8% | — | — | — | — |
| 40mmAutocannon | 62.80 | 53.44 | -14.9% | — | — | — | — |
| Naval cannon | 19.60 | 34.84 | +77.8% | 62.72 | +80.0% | — | — |
| RailgunBatteryMk1 | 6.53 | 23.62 | +261.6% | 40.84 | +72.9% | 31.10 | +31.7% |
| RailgunBatteryMk2 | 12.44 | 40.66 | +226.8% | 69.12 | +70.0% | 71.66 | +76.3% |
| RailgunBatteryMk3 | 39.19 | 81.65 | +108.3% | 163.30 | +100.0% | 225.39 | +176.1% |
| CoilgunBatteryMk1 | 21.87 | 65.89 | +201.3% | 131.78 | +100.0% | 355.74 | +439.9% |
| CoilgunBatteryMk2 | 42.34 | 138.69 | +227.6% | 277.38 | +100.0% | 720.00 | +419.1% |
| CoilgunBatteryMk3 | 75.60 | 278.44 | +268.3% | 556.88 | +100.0% | 1,402.64 | +403.8% |
| PointDefenseLaserTurret | 40.00 | 40.00 | +0.0% | — | — | — | — |
| IRLaserBattery | 13.33 | 13.33 | +0.0% | 20.00 | +50.0% | 33.33 | +150.0% |
| PointDefenseArcLaserTurret | 50.00 | 50.00 | +0.0% | — | — | — | — |
| IRArcLaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| PointDefensePhaserTurret | 66.67 | 66.67 | +0.0% | — | — | — | — |
| IRPhaserBattery | 40.00 | 40.00 | +0.0% | 60.00 | +50.0% | 100.00 | +150.0% |
| GreenLaserBattery | 13.33 | 13.33 | +0.0% | 20.00 | +50.0% | 33.33 | +150.0% |
| GreenArcLaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| GreenPhaserBattery | 40.00 | 40.00 | +0.0% | 60.00 | +50.0% | 100.00 | +150.0% |
| UVLaserBattery | 13.33 | 13.33 | +0.0% | 20.00 | +50.0% | 33.33 | +150.0% |
| UVArcLaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| UVPhaserBattery | 40.00 | 40.00 | +0.0% | 60.00 | +50.0% | 100.00 | +150.0% |
| PointDefenseE-Beamer | 5.00 | 5.00 | +0.0% | — | — | — | — |
| E-BeamBattery | 8.21 | 8.21 | +0.0% | 16.41 | +100.0% | 32.82 | +300.0% |
| PointDefenseIonBattery | 6.67 | 6.67 | +0.0% | — | — | — | — |
| IonBattery | 8.21 | 8.21 | +0.0% | 16.41 | +100.0% | 32.82 | +300.0% |
| ParticleBeamBattery | 11.03 | 11.03 | +0.0% | 22.07 | +100.0% | 44.14 | +300.0% |
| AntimatterPointDefenseBattery | 26.67 | 26.67 | +0.0% | 53.33 | +100.0% | 106.67 | +300.0% |
| PlasmaBatteryMk1 | 9.00 | 9.00 | +0.0% | 18.00 | +100.0% | 28.44 | +216.0% |
| PlasmaBatteryMk2 | 10.80 | 10.80 | +0.0% | 21.60 | +100.0% | 34.13 | +216.0% |
| PlasmaBatteryMk3 | 13.50 | 13.50 | +0.0% | 27.00 | +100.0% | 42.67 | +216.0% |


### Destroyer: 2 ordinary nodes, tier 2

All-appearance conservative maximum at 5 m: **4**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | 7.50 | 9.81 | +30.8% | — | — | — | — |
| 40mmAutocannon | 31.40 | 26.72 | -14.9% | — | — | — | — |
| Naval cannon | 9.80 | 17.42 | +77.8% | 31.36 | +80.0% | — | — |
| RailgunBatteryMk1 | 3.27 | 11.81 | +261.6% | 20.42 | +72.9% | 15.55 | +31.7% |
| RailgunBatteryMk2 | 6.22 | 20.33 | +226.8% | 34.56 | +70.0% | 35.83 | +76.3% |
| RailgunBatteryMk3 | 19.60 | 40.82 | +108.3% | 81.65 | +100.0% | 112.70 | +176.1% |
| CoilgunBatteryMk1 | 10.94 | 32.95 | +201.3% | 65.89 | +100.0% | 177.87 | +439.9% |
| CoilgunBatteryMk2 | 21.17 | 69.34 | +227.6% | 138.69 | +100.0% | 360.00 | +419.1% |
| CoilgunBatteryMk3 | 37.80 | 139.22 | +268.3% | 278.44 | +100.0% | 701.32 | +403.8% |
| PointDefenseLaserTurret | 20.00 | 20.00 | +0.0% | — | — | — | — |
| IRLaserBattery | 6.67 | 6.67 | +0.0% | 10.00 | +50.0% | 16.67 | +150.0% |
| PointDefenseArcLaserTurret | 25.00 | 25.00 | +0.0% | — | — | — | — |
| IRArcLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| PointDefensePhaserTurret | 33.33 | 33.33 | +0.0% | — | — | — | — |
| IRPhaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| GreenLaserBattery | 6.67 | 6.67 | +0.0% | 10.00 | +50.0% | 16.67 | +150.0% |
| GreenArcLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| GreenPhaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| UVLaserBattery | 6.67 | 6.67 | +0.0% | 10.00 | +50.0% | 16.67 | +150.0% |
| UVArcLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| UVPhaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| PointDefenseE-Beamer | 2.50 | 2.50 | +0.0% | — | — | — | — |
| E-BeamBattery | 4.10 | 4.10 | +0.0% | 8.21 | +100.0% | 16.41 | +300.0% |
| PointDefenseIonBattery | 3.33 | 3.33 | +0.0% | — | — | — | — |
| IonBattery | 4.10 | 4.10 | +0.0% | 8.21 | +100.0% | 16.41 | +300.0% |
| ParticleBeamBattery | 5.52 | 5.52 | +0.0% | 11.03 | +100.0% | 22.07 | +300.0% |
| AntimatterPointDefenseBattery | 13.33 | 13.33 | +0.0% | 26.67 | +100.0% | 53.33 | +300.0% |
| PlasmaBatteryMk1 | 4.50 | 4.50 | +0.0% | 9.00 | +100.0% | 14.22 | +216.0% |
| PlasmaBatteryMk2 | 5.40 | 5.40 | +0.0% | 10.80 | +100.0% | 17.07 | +216.0% |
| PlasmaBatteryMk3 | 6.75 | 6.75 | +0.0% | 13.50 | +100.0% | 21.33 | +216.0% |


### Cruiser: 3 ordinary nodes, tier 2

All-appearance conservative maximum at 5 m: **4**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | 11.26 | 14.72 | +30.8% | — | — | — | — |
| 40mmAutocannon | 47.10 | 40.08 | -14.9% | — | — | — | — |
| Naval cannon | 14.70 | 26.13 | +77.8% | 47.04 | +80.0% | — | — |
| RailgunBatteryMk1 | 4.62 | 17.72 | +283.8% | 30.63 | +72.9% | 23.33 | +31.7% |
| RailgunBatteryMk2 | 8.83 | 30.49 | +245.2% | 51.84 | +70.0% | 53.75 | +76.3% |
| RailgunBatteryMk3 | 26.40 | 60.67 | +129.8% | 122.47 | +101.9% | 169.04 | +178.6% |
| CoilgunBatteryMk1 | 14.73 | 46.01 | +212.3% | 98.84 | +114.8% | 266.80 | +479.8% |
| CoilgunBatteryMk2 | 28.94 | 100.17 | +246.1% | 208.03 | +107.7% | 540.00 | +439.1% |
| CoilgunBatteryMk3 | 52.27 | 204.23 | +290.7% | 417.66 | +104.5% | 1,051.98 | +415.1% |
| PointDefenseLaserTurret | 30.00 | 30.00 | +0.0% | — | — | — | — |
| IRLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| PointDefenseArcLaserTurret | 37.50 | 37.50 | +0.0% | — | — | — | — |
| IRArcLaserBattery | 15.00 | 15.00 | +0.0% | 22.50 | +50.0% | 37.50 | +150.0% |
| PointDefensePhaserTurret | 50.00 | 50.00 | +0.0% | — | — | — | — |
| IRPhaserBattery | 30.00 | 30.00 | +0.0% | 45.00 | +50.0% | 75.00 | +150.0% |
| GreenLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| GreenArcLaserBattery | 15.00 | 15.00 | +0.0% | 22.50 | +50.0% | 37.50 | +150.0% |
| GreenPhaserBattery | 30.00 | 30.00 | +0.0% | 45.00 | +50.0% | 75.00 | +150.0% |
| UVLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| UVArcLaserBattery | 15.00 | 15.00 | +0.0% | 22.50 | +50.0% | 37.50 | +150.0% |
| UVPhaserBattery | 30.00 | 30.00 | +0.0% | 45.00 | +50.0% | 75.00 | +150.0% |
| PointDefenseE-Beamer | 3.75 | 3.75 | +0.0% | — | — | — | — |
| E-BeamBattery | 6.15 | 6.15 | +0.0% | 12.31 | +100.0% | 24.62 | +300.0% |
| PointDefenseIonBattery | 5.00 | 5.00 | +0.0% | — | — | — | — |
| IonBattery | 6.15 | 6.15 | +0.0% | 12.31 | +100.0% | 24.62 | +300.0% |
| ParticleBeamBattery | 5.52 | 5.52 | +0.0% | 16.55 | +200.0% | 33.10 | +500.0% |
| AntimatterPointDefenseBattery | 13.33 | 13.33 | +0.0% | 40.00 | +200.0% | 80.00 | +500.0% |
| PlasmaBatteryMk1 | 4.50 | 4.50 | +0.0% | 13.50 | +200.0% | 21.33 | +374.1% |
| PlasmaBatteryMk2 | 5.40 | 5.40 | +0.0% | 16.20 | +200.0% | 25.60 | +374.1% |
| PlasmaBatteryMk3 | 6.75 | 6.75 | +0.0% | 20.25 | +200.0% | 32.00 | +374.1% |


### Battlecruiser: 2 ordinary nodes, tier 2

All-appearance conservative maximum at 5 m: **4**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | 7.50 | 9.81 | +30.8% | — | — | — | — |
| 40mmAutocannon | 31.40 | 26.72 | -14.9% | — | — | — | — |
| Naval cannon | 9.80 | 17.42 | +77.8% | 31.36 | +80.0% | — | — |
| RailgunBatteryMk1 | 3.27 | 11.81 | +261.6% | 20.42 | +72.9% | 15.55 | +31.7% |
| RailgunBatteryMk2 | 6.22 | 20.33 | +226.8% | 34.56 | +70.0% | 35.83 | +76.3% |
| RailgunBatteryMk3 | 19.60 | 40.82 | +108.3% | 81.65 | +100.0% | 112.70 | +176.1% |
| CoilgunBatteryMk1 | 10.94 | 32.95 | +201.3% | 65.89 | +100.0% | 177.87 | +439.9% |
| CoilgunBatteryMk2 | 21.17 | 69.34 | +227.6% | 138.69 | +100.0% | 360.00 | +419.1% |
| CoilgunBatteryMk3 | 37.80 | 139.22 | +268.3% | 278.44 | +100.0% | 701.32 | +403.8% |
| PointDefenseLaserTurret | 20.00 | 20.00 | +0.0% | — | — | — | — |
| IRLaserBattery | 6.67 | 6.67 | +0.0% | 10.00 | +50.0% | 16.67 | +150.0% |
| PointDefenseArcLaserTurret | 25.00 | 25.00 | +0.0% | — | — | — | — |
| IRArcLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| PointDefensePhaserTurret | 33.33 | 33.33 | +0.0% | — | — | — | — |
| IRPhaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| GreenLaserBattery | 6.67 | 6.67 | +0.0% | 10.00 | +50.0% | 16.67 | +150.0% |
| GreenArcLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| GreenPhaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| UVLaserBattery | 6.67 | 6.67 | +0.0% | 10.00 | +50.0% | 16.67 | +150.0% |
| UVArcLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| UVPhaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| PointDefenseE-Beamer | 2.50 | 2.50 | +0.0% | — | — | — | — |
| E-BeamBattery | 4.10 | 4.10 | +0.0% | 8.21 | +100.0% | 16.41 | +300.0% |
| PointDefenseIonBattery | 3.33 | 3.33 | +0.0% | — | — | — | — |
| IonBattery | 4.10 | 4.10 | +0.0% | 8.21 | +100.0% | 16.41 | +300.0% |
| ParticleBeamBattery | 5.52 | 5.52 | +0.0% | 11.03 | +100.0% | 22.07 | +300.0% |
| AntimatterPointDefenseBattery | 13.33 | 13.33 | +0.0% | 26.67 | +100.0% | 53.33 | +300.0% |
| PlasmaBatteryMk1 | 4.50 | 4.50 | +0.0% | 9.00 | +100.0% | 14.22 | +216.0% |
| PlasmaBatteryMk2 | 5.40 | 5.40 | +0.0% | 10.80 | +100.0% | 17.07 | +216.0% |
| PlasmaBatteryMk3 | 6.75 | 6.75 | +0.0% | 13.50 | +100.0% | 21.33 | +216.0% |


### Battleship: 6 ordinary nodes, tier 3

All-appearance conservative maximum at 5 m: **none certified**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | 22.51 | 29.44 | +30.8% | — | — | — | — |
| 40mmAutocannon | 94.20 | 80.17 | -14.9% | — | — | — | — |
| Naval cannon | 29.40 | 52.27 | +77.8% | 94.08 | +80.0% | — | — |
| RailgunBatteryMk1 | 10.48 | 35.44 | +238.3% | 61.26 | +72.9% | 46.66 | +31.7% |
| RailgunBatteryMk2 | 23.14 | 60.98 | +163.5% | 103.68 | +70.0% | 107.50 | +76.3% |
| RailgunBatteryMk3 | 69.96 | 121.34 | +73.4% | 244.94 | +101.9% | 338.09 | +178.6% |
| CoilgunBatteryMk1 | 37.36 | 115.07 | +208.0% | 197.68 | +71.8% | 533.61 | +363.7% |
| CoilgunBatteryMk2 | 70.85 | 241.65 | +241.1% | 416.07 | +72.2% | 1,080.00 | +346.9% |
| CoilgunBatteryMk3 | 124.62 | 480.68 | +285.7% | 835.31 | +73.8% | 2,103.96 | +337.7% |
| PointDefenseLaserTurret | 60.00 | 60.00 | +0.0% | — | — | — | — |
| IRLaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| PointDefenseArcLaserTurret | 75.00 | 75.00 | +0.0% | — | — | — | — |
| IRArcLaserBattery | 30.00 | 30.00 | +0.0% | 45.00 | +50.0% | 75.00 | +150.0% |
| PointDefensePhaserTurret | 100.00 | 100.00 | +0.0% | — | — | — | — |
| IRPhaserBattery | 60.00 | 60.00 | +0.0% | 90.00 | +50.0% | 150.00 | +150.0% |
| GreenLaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| GreenArcLaserBattery | 30.00 | 30.00 | +0.0% | 45.00 | +50.0% | 75.00 | +150.0% |
| GreenPhaserBattery | 60.00 | 60.00 | +0.0% | 90.00 | +50.0% | 150.00 | +150.0% |
| UVLaserBattery | 20.00 | 20.00 | +0.0% | 30.00 | +50.0% | 50.00 | +150.0% |
| UVArcLaserBattery | 30.00 | 30.00 | +0.0% | 45.00 | +50.0% | 75.00 | +150.0% |
| UVPhaserBattery | 60.00 | 60.00 | +0.0% | 90.00 | +50.0% | 150.00 | +150.0% |
| PointDefenseE-Beamer | 7.50 | 7.50 | +0.0% | — | — | — | — |
| E-BeamBattery | 12.31 | 12.31 | +0.0% | 24.62 | +100.0% | 49.23 | +300.0% |
| PointDefenseIonBattery | 10.00 | 10.00 | +0.0% | — | — | — | — |
| IonBattery | 12.31 | 12.31 | +0.0% | 24.62 | +100.0% | 49.23 | +300.0% |
| ParticleBeamBattery | 11.03 | 11.03 | +0.0% | 33.10 | +200.0% | 66.21 | +500.0% |
| AntimatterPointDefenseBattery | 26.67 | 26.67 | +0.0% | 80.00 | +200.0% | 160.00 | +500.0% |
| PlasmaBatteryMk1 | 9.00 | 9.00 | +0.0% | 27.00 | +200.0% | 42.67 | +374.1% |
| PlasmaBatteryMk2 | 10.80 | 10.80 | +0.0% | 32.40 | +200.0% | 51.20 | +374.1% |
| PlasmaBatteryMk3 | 13.50 | 13.50 | +0.0% | 40.50 | +200.0% | 64.00 | +374.1% |


### Lancer: 3 ordinary nodes, tier 3

All-appearance conservative maximum at 5 m: **4**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | 11.26 | 14.72 | +30.8% | — | — | — | — |
| 40mmAutocannon | 47.10 | 40.08 | -14.9% | — | — | — | — |
| Naval cannon | 14.70 | 26.13 | +77.8% | 47.04 | +80.0% | — | — |
| RailgunBatteryMk1 | 4.62 | 17.72 | +283.8% | 30.63 | +72.9% | 23.33 | +31.7% |
| RailgunBatteryMk2 | 8.83 | 30.49 | +245.2% | 51.84 | +70.0% | 53.75 | +76.3% |
| RailgunBatteryMk3 | 26.40 | 60.67 | +129.8% | 122.47 | +101.9% | 169.04 | +178.6% |
| CoilgunBatteryMk1 | 14.73 | 46.01 | +212.3% | 98.84 | +114.8% | 266.80 | +479.8% |
| CoilgunBatteryMk2 | 28.94 | 100.17 | +246.1% | 208.03 | +107.7% | 540.00 | +439.1% |
| CoilgunBatteryMk3 | 52.27 | 204.23 | +290.7% | 417.66 | +104.5% | 1,051.98 | +415.1% |
| PointDefenseLaserTurret | 30.00 | 30.00 | +0.0% | — | — | — | — |
| IRLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| PointDefenseArcLaserTurret | 37.50 | 37.50 | +0.0% | — | — | — | — |
| IRArcLaserBattery | 15.00 | 15.00 | +0.0% | 22.50 | +50.0% | 37.50 | +150.0% |
| PointDefensePhaserTurret | 50.00 | 50.00 | +0.0% | — | — | — | — |
| IRPhaserBattery | 30.00 | 30.00 | +0.0% | 45.00 | +50.0% | 75.00 | +150.0% |
| GreenLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| GreenArcLaserBattery | 15.00 | 15.00 | +0.0% | 22.50 | +50.0% | 37.50 | +150.0% |
| GreenPhaserBattery | 30.00 | 30.00 | +0.0% | 45.00 | +50.0% | 75.00 | +150.0% |
| UVLaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| UVArcLaserBattery | 15.00 | 15.00 | +0.0% | 22.50 | +50.0% | 37.50 | +150.0% |
| UVPhaserBattery | 30.00 | 30.00 | +0.0% | 45.00 | +50.0% | 75.00 | +150.0% |
| PointDefenseE-Beamer | 3.75 | 3.75 | +0.0% | — | — | — | — |
| E-BeamBattery | 6.15 | 6.15 | +0.0% | 12.31 | +100.0% | 24.62 | +300.0% |
| PointDefenseIonBattery | 5.00 | 5.00 | +0.0% | — | — | — | — |
| IonBattery | 6.15 | 6.15 | +0.0% | 12.31 | +100.0% | 24.62 | +300.0% |
| ParticleBeamBattery | 5.52 | 5.52 | +0.0% | 16.55 | +200.0% | 33.10 | +500.0% |
| AntimatterPointDefenseBattery | 13.33 | 13.33 | +0.0% | 40.00 | +200.0% | 80.00 | +500.0% |
| PlasmaBatteryMk1 | 4.50 | 4.50 | +0.0% | 13.50 | +200.0% | 21.33 | +374.1% |
| PlasmaBatteryMk2 | 5.40 | 5.40 | +0.0% | 16.20 | +200.0% | 25.60 | +374.1% |
| PlasmaBatteryMk3 | 6.75 | 6.75 | +0.0% | 20.25 | +200.0% | 32.00 | +374.1% |


### Dreadnought: 8 ordinary nodes, tier 3

All-appearance conservative maximum at 5 m: **none certified**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | 30.02 | 39.25 | +30.8% | — | — | — | — |
| 40mmAutocannon | 125.61 | 106.89 | -14.9% | — | — | — | — |
| Naval cannon | 39.20 | 69.69 | +77.8% | 125.44 | +80.0% | — | — |
| RailgunBatteryMk1 | 15.55 | 47.25 | +203.8% | 81.67 | +72.9% | 62.21 | +31.7% |
| RailgunBatteryMk2 | 35.83 | 81.31 | +126.9% | 138.24 | +70.0% | 143.33 | +76.3% |
| RailgunBatteryMk3 | 112.70 | 163.30 | +44.9% | 326.59 | +100.0% | 450.78 | +176.1% |
| CoilgunBatteryMk1 | 59.53 | 177.87 | +198.8% | 263.57 | +48.2% | 711.47 | +300.0% |
| CoilgunBatteryMk2 | 110.59 | 360.00 | +225.5% | 554.76 | +54.1% | 1,440.00 | +300.0% |
| CoilgunBatteryMk3 | 191.36 | 701.32 | +266.5% | 1,113.75 | +58.8% | 2,805.27 | +300.0% |
| PointDefenseLaserTurret | 80.00 | 80.00 | +0.0% | — | — | — | — |
| IRLaserBattery | 26.67 | 26.67 | +0.0% | 40.00 | +50.0% | 66.67 | +150.0% |
| PointDefenseArcLaserTurret | 100.00 | 100.00 | +0.0% | — | — | — | — |
| IRArcLaserBattery | 40.00 | 40.00 | +0.0% | 60.00 | +50.0% | 100.00 | +150.0% |
| PointDefensePhaserTurret | 133.33 | 133.33 | +0.0% | — | — | — | — |
| IRPhaserBattery | 80.00 | 80.00 | +0.0% | 120.00 | +50.0% | 200.00 | +150.0% |
| GreenLaserBattery | 26.67 | 26.67 | +0.0% | 40.00 | +50.0% | 66.67 | +150.0% |
| GreenArcLaserBattery | 40.00 | 40.00 | +0.0% | 60.00 | +50.0% | 100.00 | +150.0% |
| GreenPhaserBattery | 80.00 | 80.00 | +0.0% | 120.00 | +50.0% | 200.00 | +150.0% |
| UVLaserBattery | 26.67 | 26.67 | +0.0% | 40.00 | +50.0% | 66.67 | +150.0% |
| UVArcLaserBattery | 40.00 | 40.00 | +0.0% | 60.00 | +50.0% | 100.00 | +150.0% |
| UVPhaserBattery | 80.00 | 80.00 | +0.0% | 120.00 | +50.0% | 200.00 | +150.0% |
| PointDefenseE-Beamer | 10.00 | 10.00 | +0.0% | — | — | — | — |
| E-BeamBattery | 16.41 | 16.41 | +0.0% | 32.82 | +100.0% | 65.64 | +300.0% |
| PointDefenseIonBattery | 13.33 | 13.33 | +0.0% | — | — | — | — |
| IonBattery | 16.41 | 16.41 | +0.0% | 32.82 | +100.0% | 65.64 | +300.0% |
| ParticleBeamBattery | 22.07 | 22.07 | +0.0% | 44.14 | +100.0% | 88.28 | +300.0% |
| AntimatterPointDefenseBattery | 53.33 | 53.33 | +0.0% | 106.67 | +100.0% | 213.33 | +300.0% |
| PlasmaBatteryMk1 | 18.00 | 18.00 | +0.0% | 36.00 | +100.0% | 56.89 | +216.0% |
| PlasmaBatteryMk2 | 21.60 | 21.60 | +0.0% | 43.20 | +100.0% | 68.27 | +216.0% |
| PlasmaBatteryMk3 | 27.00 | 27.00 | +0.0% | 54.00 | +100.0% | 85.33 | +216.0% |


### Titan: 6 ordinary nodes, tier 3

All-appearance conservative maximum at 5 m: **none certified**. Already FourHull-only per node; size-2 rows would require reversing that restriction.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | 22.51 | 0.00 | -100.0% | — | — | — | — |
| 40mmAutocannon | 94.20 | 0.00 | -100.0% | — | — | — | — |
| Naval cannon | 29.40 | 0.00 | -100.0% | 94.08 | — | — | — |
| RailgunBatteryMk1 | 10.48 | 46.66 | +345.4% | 61.26 | +31.3% | 46.66 | +0.0% |
| RailgunBatteryMk2 | 23.14 | 107.50 | +364.5% | 103.68 | -3.5% | 107.50 | +0.0% |
| RailgunBatteryMk3 | 69.96 | 338.09 | +383.3% | 244.94 | -27.6% | 338.09 | +0.0% |
| CoilgunBatteryMk1 | 37.36 | 533.61 | +1328.2% | 197.68 | -63.0% | 533.61 | +0.0% |
| CoilgunBatteryMk2 | 70.85 | 1,080.00 | +1424.4% | 416.07 | -61.5% | 1,080.00 | +0.0% |
| CoilgunBatteryMk3 | 124.62 | 2,103.96 | +1588.3% | 835.31 | -60.3% | 2,103.96 | +0.0% |
| PointDefenseLaserTurret | 60.00 | 0.00 | -100.0% | — | — | — | — |
| IRLaserBattery | 20.00 | 50.00 | +150.0% | 30.00 | -40.0% | 50.00 | +0.0% |
| PointDefenseArcLaserTurret | 75.00 | 0.00 | -100.0% | — | — | — | — |
| IRArcLaserBattery | 30.00 | 75.00 | +150.0% | 45.00 | -40.0% | 75.00 | +0.0% |
| PointDefensePhaserTurret | 100.00 | 0.00 | -100.0% | — | — | — | — |
| IRPhaserBattery | 60.00 | 150.00 | +150.0% | 90.00 | -40.0% | 150.00 | +0.0% |
| GreenLaserBattery | 20.00 | 50.00 | +150.0% | 30.00 | -40.0% | 50.00 | +0.0% |
| GreenArcLaserBattery | 30.00 | 75.00 | +150.0% | 45.00 | -40.0% | 75.00 | +0.0% |
| GreenPhaserBattery | 60.00 | 150.00 | +150.0% | 90.00 | -40.0% | 150.00 | +0.0% |
| UVLaserBattery | 20.00 | 50.00 | +150.0% | 30.00 | -40.0% | 50.00 | +0.0% |
| UVArcLaserBattery | 30.00 | 75.00 | +150.0% | 45.00 | -40.0% | 75.00 | +0.0% |
| UVPhaserBattery | 60.00 | 150.00 | +150.0% | 90.00 | -40.0% | 150.00 | +0.0% |
| PointDefenseE-Beamer | 7.50 | 0.00 | -100.0% | — | — | — | — |
| E-BeamBattery | 12.31 | 49.23 | +300.0% | 24.62 | -50.0% | 49.23 | +0.0% |
| PointDefenseIonBattery | 10.00 | 0.00 | -100.0% | — | — | — | — |
| IonBattery | 12.31 | 49.23 | +300.0% | 24.62 | -50.0% | 49.23 | +0.0% |
| ParticleBeamBattery | 11.03 | 66.21 | +500.0% | 33.10 | -50.0% | 66.21 | +0.0% |
| AntimatterPointDefenseBattery | 26.67 | 160.00 | +500.0% | 80.00 | -50.0% | 160.00 | +0.0% |
| PlasmaBatteryMk1 | 9.00 | 42.67 | +374.1% | 27.00 | -36.7% | 42.67 | +0.0% |
| PlasmaBatteryMk2 | 10.80 | 51.20 | +374.1% | 32.40 | -36.7% | 51.20 | +0.0% |
| PlasmaBatteryMk3 | 13.50 | 64.00 | +374.1% | 40.50 | -36.7% | 64.00 | +0.0% |


### STOFighter: 1 ordinary nodes, tier 1

All-appearance conservative maximum at 5 m: **4***. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | 3.75 | 4.91 | +30.8% | — | — | — | — |
| 40mmAutocannon | 15.70 | 13.36 | -14.9% | — | — | — | — |
| Naval cannon | 4.90 | 8.71 | +77.8% | 15.68 | +80.0% | — | — |
| RailgunBatteryMk1 | 1.35 | 5.91 | +337.5% | 10.21 | +72.9% | 7.78 | +31.7% |
| RailgunBatteryMk2 | 2.61 | 10.16 | +288.9% | 17.28 | +70.0% | 17.92 | +76.3% |
| RailgunBatteryMk3 | 6.80 | 19.85 | +191.7% | 40.82 | +105.7% | 56.35 | +183.9% |
| CoilgunBatteryMk1 | 3.80 | 13.07 | +244.1% | 32.95 | +152.1% | 88.93 | +580.6% |
| CoilgunBatteryMk2 | 7.78 | 30.83 | +296.4% | 69.34 | +124.9% | 180.00 | +483.9% |
| CoilgunBatteryMk3 | 14.47 | 65.01 | +349.3% | 139.22 | +114.1% | 350.66 | +439.4% |
| PointDefenseLaserTurret | 10.00 | 10.00 | +0.0% | — | — | — | — |
| IRLaserBattery | 3.33 | 3.33 | +0.0% | 5.00 | +50.0% | 8.33 | +150.0% |
| PointDefenseArcLaserTurret | 12.50 | 12.50 | +0.0% | — | — | — | — |
| IRArcLaserBattery | 5.00 | 5.00 | +0.0% | 7.50 | +50.0% | 12.50 | +150.0% |
| PointDefensePhaserTurret | 16.67 | 16.67 | +0.0% | — | — | — | — |
| IRPhaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| GreenLaserBattery | 3.33 | 3.33 | +0.0% | 5.00 | +50.0% | 8.33 | +150.0% |
| GreenArcLaserBattery | 5.00 | 5.00 | +0.0% | 7.50 | +50.0% | 12.50 | +150.0% |
| GreenPhaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| UVLaserBattery | 3.33 | 3.33 | +0.0% | 5.00 | +50.0% | 8.33 | +150.0% |
| UVArcLaserBattery | 5.00 | 5.00 | +0.0% | 7.50 | +50.0% | 12.50 | +150.0% |
| UVPhaserBattery | 10.00 | 10.00 | +0.0% | 15.00 | +50.0% | 25.00 | +150.0% |
| PointDefenseE-Beamer | 1.25 | 1.25 | +0.0% | — | — | — | — |
| E-BeamBattery | 2.05 | 2.05 | +0.0% | 4.10 | +100.0% | 8.21 | +300.0% |
| PointDefenseIonBattery | 1.67 | 1.67 | +0.0% | — | — | — | — |
| IonBattery | 2.05 | 2.05 | +0.0% | 4.10 | +100.0% | 8.21 | +300.0% |
| ParticleBeamBattery | 0.00 | 0.00 | — | 5.52 | — | 11.03 | — |
| AntimatterPointDefenseBattery | 0.00 | 0.00 | — | 13.33 | — | 26.67 | — |
| PlasmaBatteryMk1 | 0.00 | 0.00 | — | 4.50 | — | 7.11 | — |
| PlasmaBatteryMk2 | 0.00 | 0.00 | — | 5.40 | — | 8.53 | — |
| PlasmaBatteryMk3 | 0.00 | 0.00 | — | 6.75 | — | 10.67 | — |


### AlienGunship: 0 ordinary nodes, tier 1

All-appearance conservative maximum at 5 m: **no nodes**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| AdvancedAlienMagBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| Gen3AlienMagBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| AlienPointDefenseLaserTurret | 0.00 | 0.00 | — | — | — | — | — |
| AlienOrangeLaserBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| AlienVioletLaserBattery | 0.00 | 0.00 | — | 0.00 | — | 0.00 | — |
| AlienXaserBattery | 0.00 | 0.00 | — | — | — | 0.00 | — |
| AlienPointDefenseParticleBeam | 0.00 | 0.00 | — | — | — | — | — |
| AlienPlasmaBattery | 0.00 | 0.00 | — | 0.00 | — | — | — |
| AlienHeavyPlasmaBattery | 0.00 | 0.00 | — | — | — | 0.00 | — |


### AlienEscort: 2 ordinary nodes, tier 1

All-appearance conservative maximum at 5 m: **4**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 22.67 | 156.52 | +590.5% | 313.04 | +100.0% | 708.02 | +352.3% |
| AdvancedAlienMagBattery | 67.50 | 388.73 | +475.9% | 777.47 | +100.0% | 1,820.83 | +368.4% |
| Gen3AlienMagBattery | 242.89 | 1,497.69 | +516.6% | 2,995.38 | +100.0% | 6,653.08 | +344.2% |
| AlienPointDefenseLaserTurret | 53.33 | 53.33 | +0.0% | — | — | — | — |
| AlienOrangeLaserBattery | 16.00 | 16.00 | +0.0% | 24.00 | +50.0% | 30.00 | +87.5% |
| AlienVioletLaserBattery | 21.33 | 21.33 | +0.0% | 32.00 | +50.0% | 40.00 | +87.5% |
| AlienXaserBattery | 0.00 | 0.00 | — | — | — | 20.00 | — |
| AlienPointDefenseParticleBeam | 32.00 | 32.00 | +0.0% | — | — | — | — |
| AlienPlasmaBattery | 13.23 | 13.23 | +0.0% | 26.46 | +100.0% | — | — |
| AlienHeavyPlasmaBattery | 0.00 | 0.00 | — | — | — | 36.75 | — |


### AlienCorvette: 1 ordinary nodes, tier 1

All-appearance conservative maximum at 5 m: **4***. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 8.70 | 70.56 | +710.7% | 156.52 | +121.8% | 354.01 | +401.7% |
| AdvancedAlienMagBattery | 25.92 | 187.43 | +623.1% | 388.73 | +107.4% | 910.42 | +385.7% |
| Gen3AlienMagBattery | 95.26 | 502.53 | +427.6% | 1,497.69 | +198.0% | 3,326.54 | +562.0% |
| AlienPointDefenseLaserTurret | 26.67 | 26.67 | +0.0% | — | — | — | — |
| AlienOrangeLaserBattery | 8.00 | 8.00 | +0.0% | 12.00 | +50.0% | 15.00 | +87.5% |
| AlienVioletLaserBattery | 10.67 | 10.67 | +0.0% | 16.00 | +50.0% | 20.00 | +87.5% |
| AlienXaserBattery | 0.00 | 0.00 | — | — | — | 10.00 | — |
| AlienPointDefenseParticleBeam | 16.00 | 16.00 | +0.0% | — | — | — | — |
| AlienPlasmaBattery | 0.00 | 0.00 | — | 13.23 | — | — | — |
| AlienHeavyPlasmaBattery | 0.00 | 0.00 | — | — | — | 18.38 | — |


### AlienFrigate: 3 ordinary nodes, tier 1

All-appearance conservative maximum at 5 m: **4**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 31.37 | 227.08 | +623.9% | 469.57 | +106.8% | 1,062.03 | +367.7% |
| AdvancedAlienMagBattery | 93.42 | 576.16 | +516.7% | 1,166.20 | +102.4% | 2,731.25 | +374.0% |
| Gen3AlienMagBattery | 338.15 | 2,000.22 | +491.5% | 4,493.07 | +124.6% | 9,979.62 | +398.9% |
| AlienPointDefenseLaserTurret | 80.00 | 80.00 | +0.0% | — | — | — | — |
| AlienOrangeLaserBattery | 24.00 | 24.00 | +0.0% | 36.00 | +50.0% | 45.00 | +87.5% |
| AlienVioletLaserBattery | 32.00 | 32.00 | +0.0% | 48.00 | +50.0% | 60.00 | +87.5% |
| AlienXaserBattery | 0.00 | 0.00 | — | — | — | 30.00 | — |
| AlienPointDefenseParticleBeam | 48.00 | 48.00 | +0.0% | — | — | — | — |
| AlienPlasmaBattery | 13.23 | 13.23 | +0.0% | 39.69 | +200.0% | — | — |
| AlienHeavyPlasmaBattery | 0.00 | 0.00 | — | — | — | 55.12 | — |


### AlienMonitor: 4 ordinary nodes, tier 2

All-appearance conservative maximum at 5 m: **4**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 54.40 | 354.01 | +550.8% | 626.09 | +76.9% | 1,416.05 | +300.0% |
| AdvancedAlienMagBattery | 162.00 | 910.42 | +462.0% | 1,554.93 | +70.8% | 3,641.66 | +300.0% |
| Gen3AlienMagBattery | 585.41 | 3,326.54 | +468.2% | 5,990.76 | +80.1% | 13,306.15 | +300.0% |
| AlienPointDefenseLaserTurret | 106.67 | 106.67 | +0.0% | — | — | — | — |
| AlienOrangeLaserBattery | 32.00 | 32.00 | +0.0% | 48.00 | +50.0% | 60.00 | +87.5% |
| AlienVioletLaserBattery | 42.67 | 42.67 | +0.0% | 64.00 | +50.0% | 80.00 | +87.5% |
| AlienXaserBattery | 10.00 | 10.00 | +0.0% | — | — | 40.00 | +300.0% |
| AlienPointDefenseParticleBeam | 64.00 | 64.00 | +0.0% | — | — | — | — |
| AlienPlasmaBattery | 26.46 | 26.46 | +0.0% | 52.92 | +100.0% | — | — |
| AlienHeavyPlasmaBattery | 18.38 | 18.38 | +0.0% | — | — | 73.50 | +300.0% |


### AlienDestroyer: 2 ordinary nodes, tier 2

All-appearance conservative maximum at 5 m: **4**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 22.67 | 156.52 | +590.5% | 313.04 | +100.0% | 708.02 | +352.3% |
| AdvancedAlienMagBattery | 67.50 | 388.73 | +475.9% | 777.47 | +100.0% | 1,820.83 | +368.4% |
| Gen3AlienMagBattery | 242.89 | 1,497.69 | +516.6% | 2,995.38 | +100.0% | 6,653.08 | +344.2% |
| AlienPointDefenseLaserTurret | 53.33 | 53.33 | +0.0% | — | — | — | — |
| AlienOrangeLaserBattery | 16.00 | 16.00 | +0.0% | 24.00 | +50.0% | 30.00 | +87.5% |
| AlienVioletLaserBattery | 21.33 | 21.33 | +0.0% | 32.00 | +50.0% | 40.00 | +87.5% |
| AlienXaserBattery | 0.00 | 0.00 | — | — | — | 20.00 | — |
| AlienPointDefenseParticleBeam | 32.00 | 32.00 | +0.0% | — | — | — | — |
| AlienPlasmaBattery | 13.23 | 13.23 | +0.0% | 26.46 | +100.0% | — | — |
| AlienHeavyPlasmaBattery | 0.00 | 0.00 | — | — | — | 36.75 | — |


### AlienCruiser: 4 ordinary nodes, tier 2

All-appearance conservative maximum at 5 m: **4**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 54.40 | 354.01 | +550.8% | 626.09 | +76.9% | 1,416.05 | +300.0% |
| AdvancedAlienMagBattery | 162.00 | 910.42 | +462.0% | 1,554.93 | +70.8% | 3,641.66 | +300.0% |
| Gen3AlienMagBattery | 585.41 | 3,326.54 | +468.2% | 5,990.76 | +80.1% | 13,306.15 | +300.0% |
| AlienPointDefenseLaserTurret | 106.67 | 106.67 | +0.0% | — | — | — | — |
| AlienOrangeLaserBattery | 32.00 | 32.00 | +0.0% | 48.00 | +50.0% | 60.00 | +87.5% |
| AlienVioletLaserBattery | 42.67 | 42.67 | +0.0% | 64.00 | +50.0% | 80.00 | +87.5% |
| AlienXaserBattery | 10.00 | 10.00 | +0.0% | — | — | 40.00 | +300.0% |
| AlienPointDefenseParticleBeam | 64.00 | 64.00 | +0.0% | — | — | — | — |
| AlienPlasmaBattery | 26.46 | 26.46 | +0.0% | 52.92 | +100.0% | — | — |
| AlienHeavyPlasmaBattery | 18.38 | 18.38 | +0.0% | — | — | 73.50 | +300.0% |


### AlienBattlecruiser: 3 ordinary nodes, tier 2

All-appearance conservative maximum at 5 m: **4**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 31.37 | 227.08 | +623.9% | 469.57 | +106.8% | 1,062.03 | +367.7% |
| AdvancedAlienMagBattery | 93.42 | 576.16 | +516.7% | 1,166.20 | +102.4% | 2,731.25 | +374.0% |
| Gen3AlienMagBattery | 338.15 | 2,000.22 | +491.5% | 4,493.07 | +124.6% | 9,979.62 | +398.9% |
| AlienPointDefenseLaserTurret | 80.00 | 80.00 | +0.0% | — | — | — | — |
| AlienOrangeLaserBattery | 24.00 | 24.00 | +0.0% | 36.00 | +50.0% | 45.00 | +87.5% |
| AlienVioletLaserBattery | 32.00 | 32.00 | +0.0% | 48.00 | +50.0% | 60.00 | +87.5% |
| AlienXaserBattery | 0.00 | 0.00 | — | — | — | 30.00 | — |
| AlienPointDefenseParticleBeam | 48.00 | 48.00 | +0.0% | — | — | — | — |
| AlienPlasmaBattery | 13.23 | 13.23 | +0.0% | 39.69 | +200.0% | — | — |
| AlienHeavyPlasmaBattery | 0.00 | 0.00 | — | — | — | 55.12 | — |


### AlienBattleship: 6 ordinary nodes, tier 2

All-appearance conservative maximum at 5 m: **4**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 71.81 | 495.13 | +589.5% | 939.13 | +89.7% | 2,124.07 | +329.0% |
| AdvancedAlienMagBattery | 213.84 | 1,285.27 | +501.0% | 2,332.40 | +81.5% | 5,462.49 | +325.0% |
| Gen3AlienMagBattery | 775.92 | 4,331.60 | +458.3% | 8,986.14 | +107.5% | 19,959.23 | +360.8% |
| AlienPointDefenseLaserTurret | 160.00 | 160.00 | +0.0% | — | — | — | — |
| AlienOrangeLaserBattery | 48.00 | 48.00 | +0.0% | 72.00 | +50.0% | 90.00 | +87.5% |
| AlienVioletLaserBattery | 64.00 | 64.00 | +0.0% | 96.00 | +50.0% | 120.00 | +87.5% |
| AlienXaserBattery | 10.00 | 10.00 | +0.0% | — | — | 60.00 | +500.0% |
| AlienPointDefenseParticleBeam | 96.00 | 96.00 | +0.0% | — | — | — | — |
| AlienPlasmaBattery | 26.46 | 26.46 | +0.0% | 79.38 | +200.0% | — | — |
| AlienHeavyPlasmaBattery | 18.38 | 18.38 | +0.0% | — | — | 110.25 | +500.0% |


### AlienLancer: 4 ordinary nodes, tier 2

All-appearance conservative maximum at 5 m: **4**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 45.33 | 313.04 | +590.5% | 626.09 | +100.0% | 1,416.05 | +352.3% |
| AdvancedAlienMagBattery | 135.00 | 777.47 | +475.9% | 1,554.93 | +100.0% | 3,641.66 | +368.4% |
| Gen3AlienMagBattery | 485.79 | 2,995.38 | +516.6% | 5,990.76 | +100.0% | 13,306.15 | +344.2% |
| AlienPointDefenseLaserTurret | 106.67 | 106.67 | +0.0% | — | — | — | — |
| AlienOrangeLaserBattery | 32.00 | 32.00 | +0.0% | 48.00 | +50.0% | 60.00 | +87.5% |
| AlienVioletLaserBattery | 42.67 | 42.67 | +0.0% | 64.00 | +50.0% | 80.00 | +87.5% |
| AlienXaserBattery | 0.00 | 0.00 | — | — | — | 40.00 | — |
| AlienPointDefenseParticleBeam | 64.00 | 64.00 | +0.0% | — | — | — | — |
| AlienPlasmaBattery | 26.46 | 26.46 | +0.0% | 52.92 | +100.0% | — | — |
| AlienHeavyPlasmaBattery | 0.00 | 0.00 | — | — | — | 73.50 | — |


### AlienDreadnought: 8 ordinary nodes, tier 2

All-appearance conservative maximum at 5 m: **none certified**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 108.80 | 708.02 | +550.8% | 1,252.17 | +76.9% | 2,832.09 | +300.0% |
| AdvancedAlienMagBattery | 324.00 | 1,820.83 | +462.0% | 3,109.87 | +70.8% | 7,283.32 | +300.0% |
| Gen3AlienMagBattery | 1,170.81 | 6,653.08 | +468.2% | 11,981.52 | +80.1% | 26,612.31 | +300.0% |
| AlienPointDefenseLaserTurret | 213.33 | 213.33 | +0.0% | — | — | — | — |
| AlienOrangeLaserBattery | 64.00 | 64.00 | +0.0% | 96.00 | +50.0% | 120.00 | +87.5% |
| AlienVioletLaserBattery | 85.33 | 85.33 | +0.0% | 128.00 | +50.0% | 160.00 | +87.5% |
| AlienXaserBattery | 20.00 | 20.00 | +0.0% | — | — | 80.00 | +300.0% |
| AlienPointDefenseParticleBeam | 128.00 | 128.00 | +0.0% | — | — | — | — |
| AlienPlasmaBattery | 52.92 | 52.92 | +0.0% | 105.84 | +100.0% | — | — |
| AlienHeavyPlasmaBattery | 36.75 | 36.75 | +0.0% | — | — | 147.00 | +300.0% |


### AlienTitan: 8 ordinary nodes, tier 3

All-appearance conservative maximum at 5 m: **4**. Already FourHull-only per node; size-2 rows would require reversing that restriction.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 108.80 | 2,832.09 | +2503.0% | 1,252.17 | -55.8% | 2,832.09 | +0.0% |
| AdvancedAlienMagBattery | 324.00 | 7,283.32 | +2147.9% | 3,109.87 | -57.3% | 7,283.32 | +0.0% |
| Gen3AlienMagBattery | 1,170.81 | 26,612.31 | +2173.0% | 11,981.52 | -55.0% | 26,612.31 | +0.0% |
| AlienPointDefenseLaserTurret | 213.33 | 0.00 | -100.0% | — | — | — | — |
| AlienOrangeLaserBattery | 64.00 | 120.00 | +87.5% | 96.00 | -20.0% | 120.00 | +0.0% |
| AlienVioletLaserBattery | 85.33 | 160.00 | +87.5% | 128.00 | -20.0% | 160.00 | +0.0% |
| AlienXaserBattery | 20.00 | 80.00 | +300.0% | — | — | 80.00 | +0.0% |
| AlienPointDefenseParticleBeam | 128.00 | 0.00 | -100.0% | — | — | — | — |
| AlienPlasmaBattery | 52.92 | 0.00 | -100.0% | 105.84 | — | — | — |
| AlienHeavyPlasmaBattery | 36.75 | 147.00 | +300.0% | — | — | 147.00 | +0.0% |


### AlienAssaultCarrier: 6 ordinary nodes, tier 3

All-appearance conservative maximum at 5 m: **none certified**. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 62.74 | 454.16 | +623.9% | 939.13 | +106.8% | 2,124.07 | +367.7% |
| AdvancedAlienMagBattery | 186.84 | 1,152.32 | +516.7% | 2,332.40 | +102.4% | 5,462.49 | +374.0% |
| Gen3AlienMagBattery | 676.30 | 4,000.44 | +491.5% | 8,986.14 | +124.6% | 19,959.23 | +398.9% |
| AlienPointDefenseLaserTurret | 160.00 | 160.00 | +0.0% | — | — | — | — |
| AlienOrangeLaserBattery | 48.00 | 48.00 | +0.0% | 72.00 | +50.0% | 90.00 | +87.5% |
| AlienVioletLaserBattery | 64.00 | 64.00 | +0.0% | 96.00 | +50.0% | 120.00 | +87.5% |
| AlienXaserBattery | 0.00 | 0.00 | — | — | — | 60.00 | — |
| AlienPointDefenseParticleBeam | 96.00 | 96.00 | +0.0% | — | — | — | — |
| AlienPlasmaBattery | 26.46 | 26.46 | +0.0% | 79.38 | +200.0% | — | — |
| AlienHeavyPlasmaBattery | 0.00 | 0.00 | — | — | — | 110.25 | — |


### AlienMothership: 16 ordinary nodes, tier 3

All-appearance conservative maximum at 5 m: **none certified**. Already FourHull-only per node; size-2 rows would require reversing that restriction.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 217.60 | 5,664.18 | +2503.0% | 2,504.35 | -55.8% | 5,664.18 | +0.0% |
| AdvancedAlienMagBattery | 648.00 | 14,566.65 | +2147.9% | 6,219.73 | -57.3% | 14,566.65 | +0.0% |
| Gen3AlienMagBattery | 2,341.62 | 53,224.62 | +2173.0% | 23,963.04 | -55.0% | 53,224.62 | +0.0% |
| AlienPointDefenseLaserTurret | 426.67 | 0.00 | -100.0% | — | — | — | — |
| AlienOrangeLaserBattery | 128.00 | 240.00 | +87.5% | 192.00 | -20.0% | 240.00 | +0.0% |
| AlienVioletLaserBattery | 170.67 | 320.00 | +87.5% | 256.00 | -20.0% | 320.00 | +0.0% |
| AlienXaserBattery | 40.00 | 160.00 | +300.0% | — | — | 160.00 | +0.0% |
| AlienPointDefenseParticleBeam | 256.00 | 0.00 | -100.0% | — | — | — | — |
| AlienPlasmaBattery | 105.84 | 0.00 | -100.0% | 211.68 | — | — | — |
| AlienHeavyPlasmaBattery | 73.50 | 294.00 | +300.0% | — | — | 294.00 | +0.0% |


### SalamanderGunship: 1 ordinary nodes, tier 1

All-appearance conservative maximum at 5 m: **4***. Stock grid footprints remain the current fitting rule.

| Family | Stock MJ/s | Current MJ/s | Current vs stock | 2/node MJ/s | 2/node Δ | 4/node MJ/s | 4/node Δ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 8.70 | 70.56 | +710.7% | 156.52 | +121.8% | 354.01 | +401.7% |
| AdvancedAlienMagBattery | 25.92 | 187.43 | +623.1% | 388.73 | +107.4% | 910.42 | +385.7% |
| Gen3AlienMagBattery | 95.26 | 502.53 | +427.6% | 1,497.69 | +198.0% | 3,326.54 | +562.0% |
| AlienPointDefenseLaserTurret | 26.67 | 26.67 | +0.0% | — | — | — | — |
| AlienOrangeLaserBattery | 8.00 | 8.00 | +0.0% | 12.00 | +50.0% | 15.00 | +87.5% |
| AlienVioletLaserBattery | 10.67 | 10.67 | +0.0% | 16.00 | +50.0% | 20.00 | +87.5% |
| AlienXaserBattery | 0.00 | 0.00 | — | — | — | 10.00 | — |
| AlienPointDefenseParticleBeam | 16.00 | 16.00 | +0.0% | — | — | — | — |
| AlienPlasmaBattery | 0.00 | 0.00 | — | 13.23 | — | — | — |
| AlienHeavyPlasmaBattery | 0.00 | 0.00 | — | — | — | 18.38 | — |


## Mount expansion: AlienDestroyer

The middle alternative node changes logical capacity from 2 to 3 (+50%). Its minimum all-node spacing is 21 m, versus 44 m for the two ordinary nodes. The full-rotation 5 m test passes for all three native size categories. The table multiplies the same weapon by three; it does not double dorsal/ventral graphics.

| Family | Size | Two nodes MJ/s | Three nodes MJ/s | Gain MJ/s | Gain | Three-node mass t |
| --- | --- | --- | --- | --- | --- | --- |
| AlienMagBattery | 1 | 141.12 | 211.68 | 70.56 | +50.0% | 120.00 |
| AlienMagBattery | 2 | 313.04 | 469.57 | 156.52 | +50.0% | 240.00 |
| AlienMagBattery | 4 | 708.02 | 1,062.03 | 354.01 | +50.0% | 480.00 |
| AdvancedAlienMagBattery | 1 | 374.85 | 562.28 | 187.43 | +50.0% | 120.00 |
| AdvancedAlienMagBattery | 2 | 777.47 | 1,166.20 | 388.73 | +50.0% | 240.00 |
| AdvancedAlienMagBattery | 4 | 1,820.83 | 2,731.25 | 910.42 | +50.0% | 480.00 |
| Gen3AlienMagBattery | 1 | 1,005.06 | 1,507.59 | 502.53 | +50.0% | 120.00 |
| Gen3AlienMagBattery | 2 | 2,995.38 | 4,493.07 | 1,497.69 | +50.0% | 240.00 |
| Gen3AlienMagBattery | 4 | 6,653.08 | 9,979.62 | 3,326.54 | +50.0% | 480.00 |
| AlienPointDefenseLaserTurret | 1 | 53.33 | 80.00 | 26.67 | +50.0% | 63.00 |
| AlienOrangeLaserBattery | 1 | 16.00 | 24.00 | 8.00 | +50.0% | 354.00 |
| AlienOrangeLaserBattery | 2 | 24.00 | 36.00 | 12.00 | +50.0% | 480.00 |
| AlienOrangeLaserBattery | 4 | 30.00 | 45.00 | 15.00 | +50.0% | 993.00 |
| AlienVioletLaserBattery | 1 | 21.33 | 32.00 | 10.67 | +50.0% | 354.00 |
| AlienVioletLaserBattery | 2 | 32.00 | 48.00 | 16.00 | +50.0% | 480.00 |
| AlienVioletLaserBattery | 4 | 40.00 | 60.00 | 20.00 | +50.0% | 993.00 |
| AlienXaserBattery | 4 | 20.00 | 30.00 | 10.00 | +50.0% | 1,320.00 |
| AlienPointDefenseParticleBeam | 1 | 32.00 | 48.00 | 16.00 | +50.0% | 3.00 |
| AlienPlasmaBattery | 2 | 26.46 | 39.69 | 13.23 | +50.0% | 600.00 |
| AlienHeavyPlasmaBattery | 4 | 36.75 | 55.12 | 18.38 | +50.0% | 1,200.00 |


AlienDreadnought 8→12 and Mothership 16→20 are arithmetic +50% and +25% ceilings at fixed weapon size, respectively. They are **not clearance-certified capacity gains**: the ordinary sets already fail the conservative tests, and central merge nodes tighten spacing further. See [extra-node scenarios](node-inventory.md).

## Weapon mass changes for representative advanced families

Weapon-only dry mass excludes ammo, reactor, radiator, battery and crew-support changes. It is a lower-scope accounting comparison, not a complete ship mass or delta-v calculation.

| Class | Family | Current t | 2/node t | 2/node Δt | 4/node t | 4/node Δt |
| --- | --- | --- | --- | --- | --- | --- |
| Gunship | CoilgunBatteryMk3 | 0.00 | 0.00 | 0.00 | 0.00 | 0.00 |
| Gunship | UVPhaserBattery | 0.00 | 0.00 | 0.00 | 0.00 | 0.00 |
| Escort | CoilgunBatteryMk3 | 80.00 | 160.00 | 80.00 | 320.00 | 240.00 |
| Escort | UVPhaserBattery | 230.00 | 310.00 | 80.00 | 630.00 | 400.00 |
| Corvette | CoilgunBatteryMk3 | 40.00 | 80.00 | 40.00 | 160.00 | 120.00 |
| Corvette | UVPhaserBattery | 115.00 | 155.00 | 40.00 | 315.00 | 200.00 |
| Frigate | CoilgunBatteryMk3 | 80.00 | 160.00 | 80.00 | 320.00 | 240.00 |
| Frigate | UVPhaserBattery | 230.00 | 310.00 | 80.00 | 630.00 | 400.00 |
| Monitor | CoilgunBatteryMk3 | 160.00 | 320.00 | 160.00 | 640.00 | 480.00 |
| Monitor | UVPhaserBattery | 460.00 | 620.00 | 160.00 | 1,260.00 | 800.00 |
| Destroyer | CoilgunBatteryMk3 | 80.00 | 160.00 | 80.00 | 320.00 | 240.00 |
| Destroyer | UVPhaserBattery | 230.00 | 310.00 | 80.00 | 630.00 | 400.00 |
| Cruiser | CoilgunBatteryMk3 | 120.00 | 240.00 | 120.00 | 480.00 | 360.00 |
| Cruiser | UVPhaserBattery | 345.00 | 465.00 | 120.00 | 945.00 | 600.00 |
| Battlecruiser | CoilgunBatteryMk3 | 80.00 | 160.00 | 80.00 | 320.00 | 240.00 |
| Battlecruiser | UVPhaserBattery | 230.00 | 310.00 | 80.00 | 630.00 | 400.00 |
| Battleship | CoilgunBatteryMk3 | 240.00 | 480.00 | 240.00 | 960.00 | 720.00 |
| Battleship | UVPhaserBattery | 690.00 | 930.00 | 240.00 | 1,890.00 | 1,200.00 |
| Lancer | CoilgunBatteryMk3 | 120.00 | 240.00 | 120.00 | 480.00 | 360.00 |
| Lancer | UVPhaserBattery | 345.00 | 465.00 | 120.00 | 945.00 | 600.00 |
| Dreadnought | CoilgunBatteryMk3 | 320.00 | 640.00 | 320.00 | 1,280.00 | 960.00 |
| Dreadnought | UVPhaserBattery | 920.00 | 1,240.00 | 320.00 | 2,520.00 | 1,600.00 |
| Titan | CoilgunBatteryMk3 | 960.00 | 480.00 | -480.00 | 960.00 | 0.00 |
| Titan | UVPhaserBattery | 1,890.00 | 930.00 | -960.00 | 1,890.00 | 0.00 |
| STOFighter | CoilgunBatteryMk3 | 40.00 | 80.00 | 40.00 | 160.00 | 120.00 |
| STOFighter | UVPhaserBattery | 115.00 | 155.00 | 40.00 | 315.00 | 200.00 |
| AlienGunship | Gen3AlienMagBattery | 0.00 | 0.00 | 0.00 | 0.00 | 0.00 |
| AlienGunship | AlienVioletLaserBattery | 0.00 | 0.00 | 0.00 | 0.00 | 0.00 |
| AlienEscort | Gen3AlienMagBattery | 80.00 | 160.00 | 80.00 | 320.00 | 240.00 |
| AlienEscort | AlienVioletLaserBattery | 236.00 | 320.00 | 84.00 | 662.00 | 426.00 |
| AlienCorvette | Gen3AlienMagBattery | 40.00 | 80.00 | 40.00 | 160.00 | 120.00 |
| AlienCorvette | AlienVioletLaserBattery | 118.00 | 160.00 | 42.00 | 331.00 | 213.00 |
| AlienFrigate | Gen3AlienMagBattery | 120.00 | 240.00 | 120.00 | 480.00 | 360.00 |
| AlienFrigate | AlienVioletLaserBattery | 354.00 | 480.00 | 126.00 | 993.00 | 639.00 |
| AlienMonitor | Gen3AlienMagBattery | 160.00 | 320.00 | 160.00 | 640.00 | 480.00 |
| AlienMonitor | AlienVioletLaserBattery | 472.00 | 640.00 | 168.00 | 1,324.00 | 852.00 |
| AlienDestroyer | Gen3AlienMagBattery | 80.00 | 160.00 | 80.00 | 320.00 | 240.00 |
| AlienDestroyer | AlienVioletLaserBattery | 236.00 | 320.00 | 84.00 | 662.00 | 426.00 |
| AlienCruiser | Gen3AlienMagBattery | 160.00 | 320.00 | 160.00 | 640.00 | 480.00 |
| AlienCruiser | AlienVioletLaserBattery | 472.00 | 640.00 | 168.00 | 1,324.00 | 852.00 |
| AlienBattlecruiser | Gen3AlienMagBattery | 120.00 | 240.00 | 120.00 | 480.00 | 360.00 |
| AlienBattlecruiser | AlienVioletLaserBattery | 354.00 | 480.00 | 126.00 | 993.00 | 639.00 |
| AlienBattleship | Gen3AlienMagBattery | 240.00 | 480.00 | 240.00 | 960.00 | 720.00 |
| AlienBattleship | AlienVioletLaserBattery | 708.00 | 960.00 | 252.00 | 1,986.00 | 1,278.00 |
| AlienLancer | Gen3AlienMagBattery | 160.00 | 320.00 | 160.00 | 640.00 | 480.00 |
| AlienLancer | AlienVioletLaserBattery | 472.00 | 640.00 | 168.00 | 1,324.00 | 852.00 |
| AlienDreadnought | Gen3AlienMagBattery | 320.00 | 640.00 | 320.00 | 1,280.00 | 960.00 |
| AlienDreadnought | AlienVioletLaserBattery | 944.00 | 1,280.00 | 336.00 | 2,648.00 | 1,704.00 |
| AlienTitan | Gen3AlienMagBattery | 1,280.00 | 640.00 | -640.00 | 1,280.00 | 0.00 |
| AlienTitan | AlienVioletLaserBattery | 2,648.00 | 1,280.00 | -1,368.00 | 2,648.00 | 0.00 |
| AlienAssaultCarrier | Gen3AlienMagBattery | 240.00 | 480.00 | 240.00 | 960.00 | 720.00 |
| AlienAssaultCarrier | AlienVioletLaserBattery | 708.00 | 960.00 | 252.00 | 1,986.00 | 1,278.00 |
| AlienMothership | Gen3AlienMagBattery | 2,560.00 | 1,280.00 | -1,280.00 | 2,560.00 | 0.00 |
| AlienMothership | AlienVioletLaserBattery | 5,296.00 | 2,560.00 | -2,736.00 | 5,296.00 | 0.00 |
| SalamanderGunship | Gen3AlienMagBattery | 40.00 | 80.00 | 40.00 | 160.00 | 120.00 |
| SalamanderGunship | AlienVioletLaserBattery | 118.00 | 160.00 | 42.00 | 331.00 | 213.00 |


## Combat interpretation

Larger laser apertures can change armor penetration without a proportionate raw-MJ/s gain; particles have range/dispersion effects; magnetic shot velocity, packet damage, ammo endurance and PD survival matter independently. Plasma and early rail sizes can have worse footprint-normalized raw output. Point-defense-only families are inventoried but cannot be treated as anti-ship batteries. Missiles are inventoried geometrically, but no nonexistent size-2/4 family is synthesized. Their delivered damage depends on warhead and terminal velocity, so no misleading cross-family missile DPS ranking is provided. Nose output, fitting legality, targeting behavior, simultaneous engagement arcs, reactor demand, cooling, ammo and total ship economics remain required checks for any later balance implementation.
