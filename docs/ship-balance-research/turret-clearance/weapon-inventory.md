# Complete hull-weapon art inventory

All 147 installed hull-weapon records are represented, including point defense, static launchers and HalfHull pods (classified with size 1 for this requested 1/2/4 survey). Technology variants frequently share the same mesh; counting them as separate art would distort the largest-size search. Dimensions and radii below use combat resource scale on a normal hull. Preview dimensions depend on the node’s placeholder scale. The mathematical bound and coordinate rules are in [methodology](methodology.md).

## Largest by conservative full-rotation diameter

This is the largest clearance envelope, not necessarily the widest forward-facing model. The complete dimensional-winner table follows so “largest” is not reduced to one ambiguous metric.

| Inventory | Size | Example weapon | Art | X × Y × Z m | Yaw diameter m | Swept diameter bound m |
| --- | --- | --- | --- | --- | --- | --- |
| Human | 1 | 30mmAutocannon | 30mm_Autocannon | 3.495 × 2.873 × 6.110 | 9.478 | 12.952 |
| Human | 2 | 8-inchCannon | 12-Inch_Cannon | 3.495 × 2.806 × 5.800 | 8.835 | 12.175 |
| Human | 4 | HeavyPlasmaBatteryMk1 | large_plasma_battery | 5.843 × 2.349 × 9.576 | 12.645 | 14.637 |
| Alien | 1 | AlienLightMagBattery | alien_small_coilgun_battery | 1.648 × 1.147 × 4.118 | 7.024 | 8.774 |
| Alien | 2 | AlienPlasmaBattery | alien_medium_plasma_battery | 2.831 × 1.656 × 5.329 | 9.402 | 12.037 |
| Alien | 4 | AlienHeavyPlasmaBattery | alien_large_plasma_battery | 5.006 × 1.521 × 8.044 | 11.673 | 12.296 |
| All | 1 | 30mmAutocannon | 30mm_Autocannon | 3.495 × 2.873 × 6.110 | 9.478 | 12.952 |
| All | 2 | 8-inchCannon | 12-Inch_Cannon | 3.495 × 2.806 × 5.800 | 8.835 | 12.175 |
| All | 4 | HeavyPlasmaBatteryMk1 | large_plasma_battery | 5.843 × 2.349 × 9.576 | 12.645 | 14.637 |


| Inventory | Size | Largest metric | Art | Value |
| --- | --- | --- | --- | --- |
| Human | 1 | width X | 30mm_Autocannon | 3.495 |
| Human | 1 | height Y | 30mm_Autocannon | 2.873 |
| Human | 1 | length Z | 30mm_Autocannon | 6.110 |
| Human | 1 | AABB envelope m³ | 30mm_Autocannon | 61.357 |
| Human | 2 | width X | 12-Inch_Cannon | 3.495 |
| Human | 2 | height Y | 12-Inch_Cannon | 2.806 |
| Human | 2 | length Z | 12-Inch_Cannon | 5.800 |
| Human | 2 | AABB envelope m³ | 12-Inch_Cannon | 56.883 |
| Human | 4 | width X | large_railgun_battery | 5.843 |
| Human | 4 | height Y | large_laser_battery | 3.769 |
| Human | 4 | length Z | large_plasma_battery | 9.576 |
| Human | 4 | AABB envelope m³ | large_plasma_battery | 131.420 |
| Alien | 1 | width X | alien_point_defense_particle_beam_battery | 3.703 |
| Alien | 1 | height Y | alien_point_defense_laser_battery | 2.460 |
| Alien | 1 | length Z | alien_point_defense_particle_beam_battery | 5.739 |
| Alien | 1 | AABB envelope m³ | alien_point_defense_particle_beam_battery | 23.902 |
| Alien | 2 | width X | alien_medium_plasma_battery | 2.831 |
| Alien | 2 | height Y | alien_medium_plasma_battery | 1.656 |
| Alien | 2 | length Z | alien_medium_plasma_battery | 5.329 |
| Alien | 2 | AABB envelope m³ | alien_medium_plasma_battery | 24.981 |
| Alien | 4 | width X | alien_large_laser_battery | 5.429 |
| Alien | 4 | height Y | alien_large_laser_battery | 1.649 |
| Alien | 4 | length Z | alien_large_plasma_battery | 8.044 |
| Alien | 4 | AABB envelope m³ | alien_large_laser_battery | 66.225 |
| All | 1 | width X | alien_point_defense_particle_beam_battery | 3.703 |
| All | 1 | height Y | 30mm_Autocannon | 2.873 |
| All | 1 | length Z | 30mm_Autocannon | 6.110 |
| All | 1 | AABB envelope m³ | 30mm_Autocannon | 61.357 |
| All | 2 | width X | 12-Inch_Cannon | 3.495 |
| All | 2 | height Y | 12-Inch_Cannon | 2.806 |
| All | 2 | length Z | 12-Inch_Cannon | 5.800 |
| All | 2 | AABB envelope m³ | 12-Inch_Cannon | 56.883 |
| All | 4 | width X | large_railgun_battery | 5.843 |
| All | 4 | height Y | large_laser_battery | 3.769 |
| All | 4 | length Z | large_plasma_battery | 9.576 |
| All | 4 | AABB envelope m³ | large_plasma_battery | 131.420 |


## Every distinct weapon resource

| Resource | Sizes | Templates | X m | Y m | Z m | Yaw diameter m | Swept diameter m | Root scale |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 30mm_Autocannon | 1 | 2 | 3.495 | 2.873 | 6.110 | 9.478 | 12.952 | 0.10088, 0.10088, 0.10088 |
| 8-Inch_Cannon | 1 | 1 | 3.495 | 2.843 | 4.986 | 7.209 | 10.624 | 0.10088, 0.10088, 0.10088 |
| 12-Inch_Cannon | 2 | 1 | 3.495 | 2.806 | 5.800 | 8.835 | 12.175 | 0.10088, 0.10088, 0.10088 |
| small_railgun_battery | 1 | 3 | 3.495 | 2.192 | 3.868 | 5.126 | 8.773 | 0.10088, 0.10088, 0.10088 |
| medium_railgun_battery | 2 | 3 | 3.495 | 2.214 | 4.450 | 6.250 | 10.000 | 0.10088, 0.10088, 0.10088 |
| large_railgun_battery | 4 | 3 | 5.843 | 2.349 | 8.886 | 11.316 | 13.597 | 0.10088, 0.10088, 0.10088 |
| small_coilgun_battery | 1 | 3 | 3.495 | 2.312 | 4.312 | 6.005 | 9.371 | 0.10088, 0.10088, 0.10088 |
| medium_coilgun_battery | 2 | 3 | 3.495 | 2.277 | 4.209 | 5.824 | 9.302 | 0.10088, 0.10088, 0.10088 |
| large_coilgun_battery | 4 | 3 | 5.843 | 2.349 | 8.038 | 9.680 | 13.118 | 0.10088, 0.10088, 0.10088 |
| alien_small_coilgun_battery | 1 | 3 | 1.648 | 1.147 | 4.118 | 7.024 | 8.774 | 0.10000, 0.10000, 0.10000 |
| alien_medium_coilgun_battery | 2 | 3 | 2.831 | 1.640 | 5.250 | 9.451 | 11.807 | 0.14424, 0.14424, 0.14424 |
| alien_large_coilgun_battery | 4 | 3 | 5.143 | 1.562 | 7.223 | 9.822 | 10.222 | 0.41640, 0.13419, 0.41640 |
| point_defense_laser_battery | 1 | 3 | 0.977 | 1.064 | 0.913 | 0.994 | 1.834 | 0.16247, 0.16247, 0.16247 |
| small_laser_battery | 1 | 9 | 1.530 | 1.607 | 1.430 | 1.556 | 2.755 | 0.25446, 0.25446, 0.25446 |
| medium_laser_battery | 2 | 9 | 2.329 | 2.441 | 2.176 | 2.368 | 4.145 | 0.38721, 0.38721, 0.38721 |
| large_laser_battery | 4 | 9 | 3.720 | 3.769 | 3.477 | 3.783 | 6.339 | 0.61853, 0.61853, 0.61853 |
| alien_point_defense_laser_battery | 1 | 1 | 2.403 | 2.460 | 2.188 | 3.228 | 5.748 | 0.14586, 0.14586, 0.14586 |
| alien_small_laser_battery | 1 | 2 | 1.648 | 1.096 | 2.301 | 3.455 | 4.821 | 0.10000, 0.10000, 0.10000 |
| alien_medium_laser_battery | 2 | 2 | 2.353 | 1.250 | 2.763 | 4.933 | 6.663 | 0.10000, 0.10000, 0.10000 |
| alien_large_laser_battery | 4 | 3 | 5.429 | 1.649 | 7.397 | 10.164 | 11.388 | 0.43959, 0.14167, 0.43959 |
| point_defense_particle_beam_battery | 1 | 2 | 2.597 | 1.168 | 3.643 | 4.585 | 5.355 | 0.10088, 0.10088, 0.10088 |
| 60_generic_battery | 1 | 2 | 3.495 | 2.262 | 4.617 | 6.648 | 10.034 | 0.10088, 0.10088, 0.10088 |
| 120_generic_battery | 2 | 3 | 3.495 | 2.231 | 4.701 | 6.778 | 10.106 | 0.10088, 0.10088, 0.10088 |
| 240_generic_battery | 4 | 3 | 3.495 | 3.406 | 4.589 | 6.688 | 10.020 | 0.10088, 0.10088, 0.10088 |
| 480_generic_cannon | 2 | 1 | 2.236 | 1.485 | 3.802 | 4.913 | 6.833 | 0.10088, 0.10088, 0.10088 |
| 960_generic_cannon | 4 | 1 | 2.236 | 1.574 | 5.259 | 7.812 | 9.669 | 0.10088, 0.10088, 0.10088 |
| alien_point_defense_particle_beam_battery | 1 | 1 | 3.703 | 1.125 | 5.739 | 8.135 | 8.565 | 0.29984, 0.09663, 0.29984 |
| medium_plasma_battery | 2 | 3 | 2.729 | 1.355 | 4.381 | 5.265 | 7.409 | 0.10088, 0.10088, 0.10088 |
| large_plasma_battery | 4 | 3 | 5.843 | 2.349 | 9.576 | 12.645 | 14.637 | 0.10088, 0.10088, 0.10088 |
| alien_medium_plasma_battery | 2 | 1 | 2.831 | 1.656 | 5.329 | 9.402 | 12.037 | 0.15479, 0.15479, 0.15479 |
| alien_large_plasma_battery | 4 | 1 | 5.006 | 1.521 | 8.044 | 11.673 | 12.296 | 0.40533, 0.13063, 0.40533 |
| missile_launcher | 1 | 25 | 1.258 | 0.602 | 1.341 | 1.690 | 3.999 | 0.10088, 0.10088, 0.10088 |
| torpedo_launcher | 1 | 26 | 2.608 | 1.287 | 2.807 | 3.504 | 8.365 | 0.10088, 0.10088, 0.10088 |
| alien_missile_launcher | 1 | 4 | 1.221 | 0.615 | 1.221 | 1.475 | 3.434 | 0.10000, 0.10000, 0.10000 |
| alien_torpedo_launcher | 1 | 2 | 2.000 | 1.031 | 2.000 | 2.417 | 5.680 | 0.10000, 0.10000, 0.10000 |


## Every template and art mapping

Raw MJ/s means emitted energy or muzzle kinetic energy before target effects. Missiles have no single comparable raw DPS here and are marked “target dependent”; their geometry is fully included.

| Template | Faction | Size / mount | Resource | Raw MJ/s | Mass t | Range km |
| --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | Human | 1 / OneHull | 30mm_Autocannon | 4.91 | 3.00 | 200 |
| 40mmAutocannon | Human | 1 / OneHull | 30mm_Autocannon | 13.36 | 25.00 | 350 |
| 6-inchCannon | Human | 1 / OneHull | 8-Inch_Cannon | 8.71 | 25.00 | 250 |
| 8-inchCannon | Human | 2 / TwoHullHoriz | 12-Inch_Cannon | 15.68 | 50.00 | 250 |
| LightRailgunBatteryMk1 | Human | 1 / OneHull | small_railgun_battery | 5.91 | 50.00 | 500 |
| LightRailgunBatteryMk2 | Human | 1 / OneHull | small_railgun_battery | 10.16 | 45.00 | 550 |
| LightRailgunBatteryMk3 | Human | 1 / OneHull | small_railgun_battery | 19.85 | 40.00 | 600 |
| RailgunBatteryMk1 | Human | 2 / TwoHullHoriz | medium_railgun_battery | 10.21 | 100.00 | 650 |
| RailgunBatteryMk2 | Human | 2 / TwoHullHoriz | medium_railgun_battery | 17.28 | 90.00 | 700 |
| RailgunBatteryMk3 | Human | 2 / TwoHullHoriz | medium_railgun_battery | 40.82 | 80.00 | 750 |
| HeavyRailgunBatteryMk1 | Human | 4 / FourHull | large_railgun_battery | 7.78 | 200.00 | 800 |
| HeavyRailgunBatteryMk2 | Human | 4 / FourHull | large_railgun_battery | 17.92 | 180.00 | 850 |
| HeavyRailgunBatteryMk3 | Human | 4 / FourHull | large_railgun_battery | 56.35 | 160.00 | 900 |
| LightCoilgunBatteryMk1 | Human | 1 / OneHull | small_coilgun_battery | 13.07 | 50.00 | 600 |
| LightCoilgunBatteryMk2 | Human | 1 / OneHull | small_coilgun_battery | 30.83 | 45.00 | 650 |
| LightCoilgunBatteryMk3 | Human | 1 / OneHull | small_coilgun_battery | 65.01 | 40.00 | 750 |
| CoilgunBatteryMk1 | Human | 2 / TwoHullHoriz | medium_coilgun_battery | 32.95 | 100.00 | 800 |
| CoilgunBatteryMk2 | Human | 2 / TwoHullHoriz | medium_coilgun_battery | 69.34 | 90.00 | 850 |
| CoilgunBatteryMk3 | Human | 2 / TwoHullHoriz | medium_coilgun_battery | 139.22 | 80.00 | 900 |
| HeavyCoilgunBatteryMk1 | Human | 4 / FourHull | large_coilgun_battery | 88.93 | 200.00 | 1,000 |
| HeavyCoilgunBatteryMk2 | Human | 4 / FourHull | large_coilgun_battery | 180.00 | 180.00 | 1,050 |
| HeavyCoilgunBatteryMk3 | Human | 4 / FourHull | large_coilgun_battery | 350.66 | 160.00 | 1,100 |
| AlienLightMagBattery | Alien | 1 / OneHull | alien_small_coilgun_battery | 70.56 | 40.00 | 650 |
| AlienMagBattery | Alien | 2 / TwoHullHoriz | alien_medium_coilgun_battery | 156.52 | 80.00 | 850 |
| AlienHeavyMagBattery | Alien | 4 / FourHull | alien_large_coilgun_battery | 354.01 | 160.00 | 1,050 |
| AdvancedAlienLightMagBattery | Alien | 1 / OneHull | alien_small_coilgun_battery | 187.43 | 40.00 | 800 |
| AdvancedAlienMagBattery | Alien | 2 / TwoHullHoriz | alien_medium_coilgun_battery | 388.73 | 80.00 | 1,000 |
| AdvancedAlienHeavyMagBattery | Alien | 4 / FourHull | alien_large_coilgun_battery | 910.42 | 160.00 | 1,150 |
| Gen3AlienLightMagBattery | Alien | 1 / OneHull | alien_small_coilgun_battery | 502.53 | 40.00 | 850 |
| Gen3AlienMagBattery | Alien | 2 / TwoHullHoriz | alien_medium_coilgun_battery | 1,497.69 | 80.00 | 1,050 |
| Gen3AlienHeavyMagBattery | Alien | 4 / FourHull | alien_large_coilgun_battery | 3,326.54 | 160.00 | 1,250 |
| PointDefenseLaserTurret | Human | 1 / OneHull | point_defense_laser_battery | 10.00 | 20.00 | 250 |
| 60cmIRLaserBattery | Human | 1 / OneHull | small_laser_battery | 3.33 | 150.00 | 600 |
| 120cmIRLaserBattery | Human | 2 / TwoHullHoriz | medium_laser_battery | 5.00 | 200.00 | 700 |
| 360cmIRLaserBattery | Human | 4 / FourHull | large_laser_battery | 8.33 | 400.00 | 850 |
| PointDefenseArcLaserTurret | Human | 1 / OneHull | point_defense_laser_battery | 12.50 | 20.00 | 300 |
| 60cmIRArcLaserBattery | Human | 1 / OneHull | small_laser_battery | 5.00 | 115.00 | 600 |
| 120cmIRArcLaserBattery | Human | 2 / TwoHullHoriz | medium_laser_battery | 7.50 | 155.00 | 700 |
| 360cmIRArcLaserBattery | Human | 4 / FourHull | large_laser_battery | 12.50 | 315.00 | 850 |
| PointDefensePhaserTurret | Human | 1 / OneHull | point_defense_laser_battery | 16.67 | 20.00 | 350 |
| 60cmIRPhaserBattery | Human | 1 / OneHull | small_laser_battery | 10.00 | 75.00 | 600 |
| 120cmIRPhaserBattery | Human | 2 / TwoHullHoriz | medium_laser_battery | 15.00 | 100.00 | 700 |
| 360cmIRPhaserBattery | Human | 4 / FourHull | large_laser_battery | 25.00 | 200.00 | 850 |
| 60cmGreenLaserBattery | Human | 1 / OneHull | small_laser_battery | 3.33 | 150.00 | 600 |
| 120cmGreenLaserBattery | Human | 2 / TwoHullHoriz | medium_laser_battery | 5.00 | 200.00 | 700 |
| 360cmGreenLaserBattery | Human | 4 / FourHull | large_laser_battery | 8.33 | 400.00 | 850 |
| 60cmGreenArcLaserBattery | Human | 1 / OneHull | small_laser_battery | 5.00 | 115.00 | 600 |
| 120cmGreenArcLaserBattery | Human | 2 / TwoHullHoriz | medium_laser_battery | 7.50 | 155.00 | 700 |
| 360cmGreenArcLaserBattery | Human | 4 / FourHull | large_laser_battery | 12.50 | 315.00 | 850 |
| 60cmGreenPhaserBattery | Human | 1 / OneHull | small_laser_battery | 10.00 | 115.00 | 600 |
| 120cmGreenPhaserBattery | Human | 2 / TwoHullHoriz | medium_laser_battery | 15.00 | 155.00 | 700 |
| 360cmGreenPhaserBattery | Human | 4 / FourHull | large_laser_battery | 25.00 | 315.00 | 850 |
| 60cmUVLaserBattery | Human | 1 / OneHull | small_laser_battery | 3.33 | 150.00 | 600 |
| 120cmUVLaserBattery | Human | 2 / TwoHullHoriz | medium_laser_battery | 5.00 | 200.00 | 700 |
| 360cmUVLaserBattery | Human | 4 / FourHull | large_laser_battery | 8.33 | 400.00 | 850 |
| 60cmUVArcLaserBattery | Human | 1 / OneHull | small_laser_battery | 5.00 | 115.00 | 600 |
| 120cmUVArcLaserBattery | Human | 2 / TwoHullHoriz | medium_laser_battery | 7.50 | 155.00 | 700 |
| 360cmUVArcLaserBattery | Human | 4 / FourHull | large_laser_battery | 12.50 | 315.00 | 850 |
| 60cmUVPhaserBattery | Human | 1 / OneHull | small_laser_battery | 10.00 | 115.00 | 600 |
| 120cmUVPhaserBattery | Human | 2 / TwoHullHoriz | medium_laser_battery | 15.00 | 155.00 | 700 |
| 360cmUVPhaserBattery | Human | 4 / FourHull | large_laser_battery | 25.00 | 315.00 | 850 |
| AlienPointDefenseLaserTurret | Alien | 1 / OneHull | alien_point_defense_laser_battery | 26.67 | 21.00 | 350 |
| Alien64cmOrangeLaserBattery | Alien | 1 / OneHull | alien_small_laser_battery | 8.00 | 118.00 | 700 |
| Alien128cmOrangeLaserBattery | Alien | 2 / TwoHullHoriz | alien_medium_laser_battery | 12.00 | 160.00 | 800 |
| Alien384cmOrangeLaserBattery | Alien | 4 / FourHull | alien_large_laser_battery | 15.00 | 331.00 | 900 |
| Alien64cmVioletLaserBattery | Alien | 1 / OneHull | alien_small_laser_battery | 10.67 | 118.00 | 700 |
| Alien128cmVioletLaserBattery | Alien | 2 / TwoHullHoriz | alien_medium_laser_battery | 16.00 | 160.00 | 800 |
| Alien384cmVioletLaserBattery | Alien | 4 / FourHull | alien_large_laser_battery | 20.00 | 331.00 | 900 |
| Alien384cmXaserBattery | Alien | 4 / FourHull | alien_large_laser_battery | 10.00 | 440.00 | 900 |
| PointDefenseE-Beamer | Human | 1 / OneHull | point_defense_particle_beam_battery | 1.25 | 5.00 | 200 |
| LightE-BeamBattery | Human | 1 / OneHull | 60_generic_battery | 2.05 | 10.00 | 300 |
| E-BeamBattery | Human | 2 / TwoHullHoriz | 120_generic_battery | 4.10 | 20.00 | 400 |
| HeavyE-BeamBattery | Human | 4 / FourHull | 240_generic_battery | 8.21 | 40.00 | 500 |
| PointDefenseIonBattery | Human | 1 / OneHull | point_defense_particle_beam_battery | 1.67 | 5.00 | 200 |
| LightIonBattery | Human | 1 / OneHull | 60_generic_battery | 2.05 | 10.00 | 500 |
| IonBattery | Human | 2 / TwoHullHoriz | 120_generic_battery | 4.10 | 20.00 | 600 |
| HeavyIonBattery | Human | 4 / FourHull | 240_generic_battery | 8.21 | 40.00 | 700 |
| ParticleBeamBattery | Human | 2 / TwoHullHoriz | 120_generic_battery | 5.52 | 25.00 | 600 |
| HeavyParticleBeamBattery | Human | 4 / FourHull | 240_generic_battery | 11.03 | 50.00 | 700 |
| AntimatterPointDefenseBattery | Human | 2 / TwoHullHoriz | 480_generic_cannon | 13.33 | 40.00 | 400 |
| HeavyAntimatterPointDefenseBattery | Human | 4 / FourHull | 960_generic_cannon | 26.67 | 120.00 | 600 |
| AlienPointDefenseParticleBeam | Alien | 1 / OneHull | alien_point_defense_particle_beam_battery | 16.00 | 1.00 | 350 |
| PlasmaBatteryMk1 | Human | 2 / TwoHullHoriz | medium_plasma_battery | 4.50 | 320.00 | 600 |
| PlasmaBatteryMk2 | Human | 2 / TwoHullHoriz | medium_plasma_battery | 5.40 | 280.00 | 700 |
| PlasmaBatteryMk3 | Human | 2 / TwoHullHoriz | medium_plasma_battery | 6.75 | 240.00 | 800 |
| HeavyPlasmaBatteryMk1 | Human | 4 / FourHull | large_plasma_battery | 7.11 | 640.00 | 700 |
| HeavyPlasmaBatteryMk2 | Human | 4 / FourHull | large_plasma_battery | 8.53 | 560.00 | 800 |
| HeavyPlasmaBatteryMk3 | Human | 4 / FourHull | large_plasma_battery | 10.67 | 480.00 | 900 |
| AlienPlasmaBattery | Alien | 2 / TwoHullHoriz | alien_medium_plasma_battery | 13.23 | 200.00 | 900 |
| AlienHeavyPlasmaBattery | Alien | 4 / FourHull | alien_large_plasma_battery | 18.38 | 400.00 | 1,000 |
| KraitMissileBay | Human | 1 / OneHull | missile_launcher | target dependent | 15.00 | 800 |
| AnacondaMissileBay | Human | 1 / OneHull | missile_launcher | target dependent | 10.00 | 800 |
| CobraMissileBay | Human | 1 / OneHull | missile_launcher | target dependent | 10.00 | 800 |
| RacerMissileBay | Human | 1 / OneHull | missile_launcher | target dependent | 10.00 | 500 |
| RattlerMissileBay | Human | 1 / OneHull | missile_launcher | target dependent | 5.00 | 800 |
| RiverjackMissileBay | Human | 1 / OneHull | missile_launcher | target dependent | 5.00 | 800 |
| HarlequinMissileBay | Human | 1 / OneHull | missile_launcher | target dependent | 5.00 | 800 |
| KeelbackMissileBay | Human | 1 / OneHull | missile_launcher | target dependent | 5.00 | 800 |
| CopperheadMissileBay | Human | 1 / OneHull | missile_launcher | target dependent | 15.00 | 800 |
| ViperMissileBay | Human | 1 / OneHull | missile_launcher | target dependent | 10.00 | 800 |
| LanceheadMissileBay | Human | 1 / OneHull | missile_launcher | target dependent | 10.00 | 800 |
| PythonNuclearMissileBay | Human | 1 / OneHull | missile_launcher | target dependent | 15.00 | 800 |
| SidewinderNuclearMissileBay | Human | 1 / OneHull | missile_launcher | target dependent | 15.00 | 800 |
| HeraTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 10.00 | 1,000 |
| HermesTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 10.00 | 1,000 |
| HerculesTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 10.00 | 1,000 |
| ArtemisTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 15.00 | 1,000 |
| VulcanTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 10.00 | 1,000 |
| ApolloTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 10.00 | 1,000 |
| AthenaTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 15.00 | 1,000 |
| AresTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 10.00 | 1,000 |
| PoseidonTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 10.00 | 1,000 |
| ZeusTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 10.00 | 1,000 |
| CerebrusNuclearTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 25.00 | 1,000 |
| HadesNuclearTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 20.00 | 1,000 |
| NemesisNuclearTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 20.00 | 1,000 |
| OlympusNuclearTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 20.00 | 1,000 |
| AcheronNuclearTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 20.00 | 1,000 |
| TartarusNuclearTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 20.00 | 1,000 |
| StyxNuclearTorpedoBay | Human | 1 / OneHull | torpedo_launcher | target dependent | 20.00 | 1,000 |
| AntimatterTorpedoLauncher | Human | 1 / OneHull | torpedo_launcher | target dependent | 50.00 | 1,000 |
| KraitMissilePod | Human | 1 / HalfHull | missile_launcher | target dependent | 7.00 | 800 |
| AnacondaMissilePod | Human | 1 / HalfHull | missile_launcher | target dependent | 5.00 | 800 |
| CobraMissilePod | Human | 1 / HalfHull | missile_launcher | target dependent | 5.00 | 800 |
| RacerMissilePod | Human | 1 / HalfHull | missile_launcher | target dependent | 5.00 | 500 |
| RattlerMissilePod | Human | 1 / HalfHull | missile_launcher | target dependent | 2.00 | 800 |
| RiverjackMissilePod | Human | 1 / HalfHull | missile_launcher | target dependent | 2.00 | 800 |
| HarlequinMissilePod | Human | 1 / HalfHull | missile_launcher | target dependent | 2.00 | 800 |
| KeelbackMissilePod | Human | 1 / HalfHull | missile_launcher | target dependent | 2.00 | 800 |
| CopperheadMissilePod | Human | 1 / HalfHull | missile_launcher | target dependent | 7.00 | 800 |
| ViperMissilePod | Human | 1 / HalfHull | missile_launcher | target dependent | 5.00 | 800 |
| LanceheadMissilePod | Human | 1 / HalfHull | missile_launcher | target dependent | 5.00 | 800 |
| PythonNuclearMissilePod | Human | 1 / HalfHull | missile_launcher | target dependent | 7.00 | 800 |
| HeraTorpedoPod | Human | 1 / HalfHull | torpedo_launcher | target dependent | 5.00 | 800 |
| HermesTorpedoPod | Human | 1 / HalfHull | torpedo_launcher | target dependent | 2.00 | 800 |
| HerculesTorpedoPod | Human | 1 / HalfHull | torpedo_launcher | target dependent | 10.00 | 1,000 |
| ArtemisTorpedoPod | Human | 1 / HalfHull | torpedo_launcher | target dependent | 7.00 | 800 |
| VulcanTorpedoPod | Human | 1 / HalfHull | torpedo_launcher | target dependent | 5.00 | 800 |
| ApolloTorpedoPod | Human | 1 / HalfHull | torpedo_launcher | target dependent | 10.00 | 1,000 |
| CerebrusNuclearTorpedoPod | Human | 1 / HalfHull | torpedo_launcher | target dependent | 12.00 | 800 |
| HadesNuclearTorpedoPod | Human | 1 / HalfHull | torpedo_launcher | target dependent | 10.00 | 800 |
| GlitteringJewelMissileBay | Alien | 1 / OneHull | alien_missile_launcher | target dependent | 3.00 | 1,000 |
| GlitteringJewelMissilePod | Alien | 1 / HalfHull | alien_missile_launcher | target dependent | 3.00 | 1,000 |
| IridescentStarTorpedoBay | Alien | 1 / OneHull | alien_torpedo_launcher | target dependent | 10.00 | 1,000 |
| LuminousSwarmMissileBay | Alien | 1 / OneHull | alien_missile_launcher | target dependent | 10.00 | 600 |
| BrilliantSkyMissileBay | Alien | 1 / OneHull | alien_missile_launcher | target dependent | 10.00 | 1,200 |
| PredatoryStarTorpedoBay | Alien | 1 / OneHull | alien_torpedo_launcher | target dependent | 20.00 | 1,200 |
