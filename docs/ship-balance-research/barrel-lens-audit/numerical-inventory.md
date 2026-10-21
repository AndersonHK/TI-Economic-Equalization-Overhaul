# Complete barrel and aperture numerical inventory

Measured 2026-10-21; read with the [audit](../barrel-and-lens-dimensional-audit-2026-10-21.md). All dimensions use nominal resource scales, in metres unless labelled mm. Bore means visible polygon opening, not a modeled rifled tube. All template data are merged installed + repository overrides.

## Gunpowder: every weapon

| Weapon | Mount | Art ID | Caliber mm | Opening X mm | Inscribed mm | X delta | Bore-only target scale | Moving assembly L m |
|---|---|---|---|---|---|---|---|---|
| 30mmAutocannon | OneHull | 2 | 30.0 | 155.4 | 150.1 | +418.0% | 0.193 | 5.972 |
| 35mmAutocannon | HalfNose | 2 | 35.0 | 155.4 | 150.1 | +344.0% | 0.225 | 5.972 |
| 40mmNoseAutocannon | HalfNose | 2 | 40.0 | 155.4 | 150.1 | +288.5% | 0.257 | 5.972 |
| 40mmAutocannon | OneHull | 2 | 40.0 | 155.4 | 150.1 | +288.5% | 0.257 | 5.972 |
| 6-inchCannon | OneHull | 3 | 152.4 | 160.7 | 155.2 | +5.5% | 0.948 | 4.859 |
| 8-inchCannon | TwoHullHoriz | 0 | 203.2 | 198.3 | 191.5 | -2.4% | 1.025 | 5.680 |
| 10-inchCannon | OneNose | 0 | 254.0 | 198.3 | 191.5 | -21.9% | 1.281 | 5.680 |
| 12-inchCannon | TwoNoseVert | 1 | 304.8 | 267.0 | 257.9 | -12.4% | 1.141 | 6.562 |


## Magnetic art: all 21 resources

Bore X/Y are per opening; the count is graphical, not an extra DPS multiplier. Blank bores are unresolved closed faces. Complete round mass is used for the EEO projectile proxy, not divided by barrel count.

| ID | Art | Openings | Bore X mm | Bore Y mm | Outer lip X mm / face | Assembly L m | Projectile proxy range mm |
|---|---|---|---|---|---|---|---|
| 39 | small_railgun_battery | 2 | 99.5 | 99.5 | 141.4 | 2.967 | 45.2–45.2 |
| 32 | medium_railgun_battery | 2 | 85.5 | 85.5 | 121.5 | 3.620 | 58.3–58.3 |
| 26 | large_railgun_battery | 4 | 176.5 | 193.0 | 250.6 | 5.706 | 68.2–68.2 |
| 35 | small_coilgun_battery | 2 | 177.1 | 177.1 | 251.5 | 3.421 | 44.1–44.1 |
| 28 | medium_coilgun_battery | 2 | 177.1 | 177.1 | 251.5 | 3.421 | 54.8–54.8 |
| 22 | large_coilgun_battery | 4 | 144.6 | 144.6 | 205.3 | 5.000 | 69.1–69.1 |
| 40 | small_railgun_cannon | closed face | — | — | face 307.0 × 323.3 | 2.882 | 62.8–62.8 |
| 33 | medium_railgun_cannon | closed face | — | — | face 537.3 × 565.8 | 5.043 | 73.4–73.4 |
| 27 | large_railgun_cannon | 1 | 208.5 | 208.5 | 296.2 | 4.845 | 84.0–84.0 |
| 21 | huge_railgun_cannon | closed face | — | — | face 743.7 × 783.1 | 5.609 | 92.5–92.5 |
| 36 | small_coilgun_cannon | 1 | 88.5 | 88.5 | 125.7 | 3.092 | 58.9–58.9 |
| 29 | medium_coilgun_cannon | 3 | 81.2 | 81.2 | 115.3 | 4.035 | 74.6–74.6 |
| 23 | large_coilgun_cannon | 3 | 120.0 | 120.0 | 170.5 | 5.391 | 85.3–183.6 |
| 19 | huge_coilgun_cannon | 3 | 140.8 | 140.8 | 200.0 | 6.541 | 93.8–202.0 |
| 15 | alien_small_coilgun_battery | 6 | 66.0 | 66.0 | 93.8 | 4.118 | 50.0–61.3 |
| 10 | alien_medium_coilgun_battery | 9 | 93.5 | 93.5 | 132.8 | 5.245 | 63.1–76.9 |
| 6 | alien_large_coilgun_battery | 9 | 98.2 | 98.2 | 139.6 | 4.302 | 79.1–96.9 |
| 16 | alien_small_coilgun_cannon | 3 | 67.9 | 67.9 | 96.6 | 2.976 | 69.1–85.0 |
| 11 | alien_medium_coilgun_cannon | 3 | 96.0 | 96.0 | 136.4 | 4.203 | 87.1–106.7 |
| 7 | alien_large_coilgun_cannon | 1 | 161.2 | 94.9 | 201.6 | 4.271 | 99.7–134.4 |
| 4 | alien_huge_coilgun_cannon | 3 | 139.6 | 139.6 | 198.3 | 6.115 | 109.7–169.3 |


## Laser art: all 16 resources

Simulation diameter = 2 × stored radius. Alien multi-aperture area-equivalent diameter is sqrt(N) × d, an area comparison only; it is not a justified coherent-array diffraction diameter.

| ID | Art | Lenses | Radius cm | Sim D m | Per-lens D m | Area-equivalent D m | Per-lens delta | Art factor to sim D |
|---|---|---|---|---|---|---|---|---|
| 34 | point_defense_laser_battery | 1 | 30 | 0.600 | 0.694 | 0.694 | +15.7% | 0.865 |
| 37 | small_laser_battery | 1 | 60 | 1.200 | 1.069 | 1.069 | -10.9% | 1.123 |
| 30 | medium_laser_battery | 1 | 120 | 2.400 | 1.639 | 1.639 | -31.7% | 1.464 |
| 24 | large_laser_battery | 1 | 360 | 7.200 | 2.608 | 2.608 | -63.8% | 2.760 |
| 38 | small_laser_cannon | 1 | 240 | 4.800 | 1.591 | 1.591 | -66.8% | 3.017 |
| 31 | medium_laser_cannon | 1 | 480 | 9.600 | 2.683 | 2.683 | -72.1% | 3.578 |
| 25 | large_laser_cannon | 1 | 720 | 14.400 | 4.138 | 4.138 | -71.3% | 3.480 |
| 20 | huge_laser_cannon | 1 | 960 | 19.200 | 4.984 | 4.984 | -74.0% | 3.852 |
| 14 | alien_point_defense_laser_battery | 3 | 32 | 0.640 | 0.722 | 1.251 | +12.9% | 0.886 |
| 17 | alien_small_laser_battery | 2 | 64 | 1.280 | 0.528 | 0.747 | -58.8% | 2.424 |
| 12 | alien_medium_laser_battery | 3 | 128 | 2.560 | 0.536 | 0.929 | -79.0% | 4.772 |
| 8 | alien_large_laser_battery | 3 | 384 | 7.680 | 0.902 | 1.562 | -88.3% | 8.514 |
| 18 | alien_small_laser_cannon | 1 | 256 | 5.120 | 0.692 | 0.692 | -86.5% | 7.402 |
| 13 | alien_medium_laser_cannon | 1 | 512 | 10.240 | 1.033 | 1.033 | -89.9% | 9.913 |
| 9 | alien_large_laser_cannon | 1 | 768 | 15.360 | 1.033 | 1.033 | -93.3% | 14.869 |
| 5 | alien_huge_laser_cannon | 1 | 1024 | 20.480 | 1.384 | 1.384 | -93.2% | 14.795 |


## Family progression deltas

Each row compares adjacent sizes. Projectile ratios use ordinary Mk1 / base alien rounds, excluding siege and Gen3. Muzzle ratio uses per-opening X width; a change in opening count is separately shown.

| Family | IDs | Openings | Round mass ratio | Proxy diameter ratio | Bore X ratio | Ratio error | Assembly L ratio |
|---|---|---|---|---|---|---|---|
| Hull rail | 39 → 32 | 2 → 2 | 2.143 | 1.289 | 0.860 | -33.3% | 1.220 |
| Hull rail | 32 → 26 | 2 → 4 | 1.600 | 1.170 | 2.063 | +76.4% | 1.576 |
| Hull coil | 35 → 28 | 2 → 2 | 1.923 | 1.244 | 1.000 | -19.6% | 1.000 |
| Hull coil | 28 → 22 | 2 → 4 | 2.000 | 1.260 | 0.816 | -35.2% | 1.461 |
| Nose coil | 36 → 29 | 1 → 3 | 2.032 | 1.267 | 0.918 | -27.6% | 1.305 |
| Nose coil | 29 → 23 | 3 → 3 | 1.492 | 1.143 | 1.479 | +29.4% | 1.336 |
| Nose coil | 23 → 19 | 3 → 3 | 1.330 | 1.100 | 1.173 | +6.7% | 1.213 |
| Alien hull mag | 15 → 10 | 6 → 9 | 2.000 | 1.260 | 1.417 | +12.4% | 1.274 |
| Alien hull mag | 10 → 6 | 9 → 9 | 1.974 | 1.254 | 1.051 | -16.2% | 0.820 |
| Alien nose mag | 16 → 11 | 3 → 3 | 2.000 | 1.260 | 1.412 | +12.1% | 1.412 |
| Alien nose mag | 11 → 7 | 3 → 1 | 1.500 | 1.145 | 1.679 | +46.7% | 1.016 |
| Alien nose mag | 7 → 4 | 1 → 3 | 1.333 | 1.101 | 0.866 | -21.3% | 1.432 |


| Family | IDs | Simulation D ratio | Art aperture ratio | Ratio error |
|---|---|---|---|---|
| Human hull laser | 37 → 30 | 2.000 | 1.533 | -23.3% |
| Human hull laser | 30 → 24 | 3.000 | 1.592 | -46.9% |
| Human nose laser | 38 → 31 | 2.000 | 1.686 | -15.7% |
| Human nose laser | 31 → 25 | 1.500 | 1.542 | +2.8% |
| Human nose laser | 25 → 20 | 1.333 | 1.205 | -9.7% |
| Alien hull laser | 17 → 12 | 2.000 | 1.016 | -49.2% |
| Alien hull laser | 12 → 8 | 3.000 | 1.681 | -44.0% |
| Alien nose laser | 18 → 13 | 2.000 | 1.493 | -25.3% |
| Alien nose laser | 13 → 9 | 1.500 | 1.000 | -33.3% |
| Alien nose laser | 9 → 5 | 1.333 | 1.340 | +0.5% |


## All 166 ship weapon templates

IDs link to measured resources above. Magnetic proxy is a mod collision convention, not a declared caliber. Laser entries list simulation aperture diameter; complete source values, wavelengths, jitter and ranges are in [inventory.json](inventory.json).

| Template | Mount | Art ID | Nominal / derived diameter | Meaning | Complete round kg |
|---|---|---|---|---|---|
| 30mmAutocannon | OneHull | 2 | 30.0 | bore mm | 5.5 |
| 35mmAutocannon | HalfNose | 2 | 35.0 | bore mm | 8.8 |
| 40mmNoseAutocannon | HalfNose | 2 | 40.0 | bore mm | 8.8 |
| 40mmAutocannon | OneHull | 2 | 40.0 | bore mm | 8.8 |
| 6-inchCannon | OneHull | 3 | 152.4 | bore mm | 90.0 |
| 8-inchCannon | TwoHullHoriz | 0 | 203.2 | bore mm | 200.0 |
| 10-inchCannon | OneNose | 0 | 254.0 | bore mm | 400.0 |
| 12-inchCannon | TwoNoseVert | 1 | 304.8 | bore mm | 640.0 |
| LightRailgunBatteryMk1 | OneHull | 39 | 45.2 | proxy mm | 14 |
| LightRailgunBatteryMk2 | OneHull | 39 | 45.2 | proxy mm | 14 |
| LightRailgunBatteryMk3 | OneHull | 39 | 45.2 | proxy mm | 14 |
| RailgunBatteryMk1 | TwoHullHoriz | 32 | 58.3 | proxy mm | 30 |
| RailgunBatteryMk2 | TwoHullHoriz | 32 | 58.3 | proxy mm | 30 |
| RailgunBatteryMk3 | TwoHullHoriz | 32 | 58.3 | proxy mm | 30 |
| HeavyRailgunBatteryMk1 | FourHull | 26 | 68.2 | proxy mm | 48 |
| HeavyRailgunBatteryMk2 | FourHull | 26 | 68.2 | proxy mm | 48 |
| HeavyRailgunBatteryMk3 | FourHull | 26 | 68.2 | proxy mm | 48 |
| LightRailCannonMk1 | OneNose | 40 | 62.8 | proxy mm | 37.5 |
| LightRailCannonMk2 | OneNose | 40 | 62.8 | proxy mm | 37.5 |
| LightRailCannonMk3 | OneNose | 40 | 62.8 | proxy mm | 37.5 |
| RailCannonMk1 | TwoNoseVert | 33 | 73.4 | proxy mm | 60 |
| RailCannonMk2 | TwoNoseVert | 33 | 73.4 | proxy mm | 60 |
| RailCannonMk3 | TwoNoseVert | 33 | 73.4 | proxy mm | 60 |
| HeavyRailCannonMk1 | ThreeNoseAngle | 27 | 84.0 | proxy mm | 90 |
| HeavyRailCannonMk2 | ThreeNoseAngle | 27 | 84.0 | proxy mm | 90 |
| HeavyRailCannonMk3 | ThreeNoseAngle | 27 | 84.0 | proxy mm | 90 |
| SpinalRailgunMk1 | FourNose | 21 | 92.5 | proxy mm | 120 |
| SpinalRailgunMk2 | FourNose | 21 | 92.5 | proxy mm | 120 |
| SpinalRailgunMk3 | FourNose | 21 | 92.5 | proxy mm | 120 |
| LightCoilgunBatteryMk1 | OneHull | 35 | 44.1 | proxy mm | 13 |
| LightCoilgunBatteryMk2 | OneHull | 35 | 44.1 | proxy mm | 13 |
| LightCoilgunBatteryMk3 | OneHull | 35 | 44.1 | proxy mm | 13 |
| CoilgunBatteryMk1 | TwoHullHoriz | 28 | 54.8 | proxy mm | 25 |
| CoilgunBatteryMk2 | TwoHullHoriz | 28 | 54.8 | proxy mm | 25 |
| CoilgunBatteryMk3 | TwoHullHoriz | 28 | 54.8 | proxy mm | 25 |
| HeavyCoilgunBatteryMk1 | FourHull | 22 | 69.1 | proxy mm | 50 |
| HeavyCoilgunBatteryMk2 | FourHull | 22 | 69.1 | proxy mm | 50 |
| HeavyCoilgunBatteryMk3 | FourHull | 22 | 69.1 | proxy mm | 50 |
| LightCoilCannonMk1 | OneNose | 36 | 58.9 | proxy mm | 31 |
| LightCoilCannonMk2 | OneNose | 36 | 58.9 | proxy mm | 31 |
| LightCoilCannonMk3 | OneNose | 36 | 58.9 | proxy mm | 31 |
| CoilCannonMk1 | TwoNoseVert | 29 | 74.6 | proxy mm | 63 |
| CoilCannonMk2 | TwoNoseVert | 29 | 74.6 | proxy mm | 63 |
| CoilCannonMk3 | TwoNoseVert | 29 | 74.6 | proxy mm | 63 |
| HeavyCoilCannonMk1 | ThreeNoseAngle | 23 | 85.3 | proxy mm | 94 |
| HeavyCoilCannonMk2 | ThreeNoseAngle | 23 | 85.3 | proxy mm | 94 |
| HeavyCoilCannonMk3 | ThreeNoseAngle | 23 | 85.3 | proxy mm | 94 |
| SpinalCoilerMk1 | FourNose | 19 | 93.8 | proxy mm | 125 |
| SpinalCoilerMk2 | FourNose | 19 | 93.8 | proxy mm | 125 |
| SpinalCoilerMk3 | FourNose | 19 | 93.8 | proxy mm | 125 |
| HeavySiegeCoilerMk1 | ThreeNoseAngle | 23 | 183.6 | proxy mm | 938 |
| HeavySiegeCoilerMk2 | ThreeNoseAngle | 23 | 183.6 | proxy mm | 938 |
| HeavySiegeCoilerMk3 | ThreeNoseAngle | 23 | 183.6 | proxy mm | 938 |
| SpinalSiegeCoilerMk1 | FourNose | 19 | 202.0 | proxy mm | 1250 |
| SpinalSiegeCoilerMk2 | FourNose | 19 | 202.0 | proxy mm | 1250 |
| SpinalSiegeCoilerMk3 | FourNose | 19 | 202.0 | proxy mm | 1250 |
| AlienLightMagBattery | OneHull | 15 | 50.0 | proxy mm | 19 |
| AlienMagBattery | TwoHullHoriz | 10 | 63.1 | proxy mm | 38 |
| AlienHeavyMagBattery | FourHull | 6 | 79.1 | proxy mm | 75 |
| AlienMiniLightMagCannon | HalfNose | 16 | 69.1 | proxy mm | 50 |
| AlienLightMagCannon | OneNose | 16 | 69.1 | proxy mm | 50 |
| AlienMagCannon | TwoNoseVert | 11 | 87.1 | proxy mm | 100 |
| AlienHeavyMagCannon | ThreeNoseAngle | 7 | 99.7 | proxy mm | 150 |
| AlienSpinalMagCannon | FourNose | 4 | 109.7 | proxy mm | 200 |
| AdvancedAlienLightMagBattery | OneHull | 15 | 50.0 | proxy mm | 19 |
| AdvancedAlienMagBattery | TwoHullHoriz | 10 | 63.1 | proxy mm | 38 |
| AdvancedAlienHeavyMagBattery | FourHull | 6 | 79.1 | proxy mm | 75 |
| AdvancedAlienLightMagCannon | OneNose | 16 | 69.1 | proxy mm | 50 |
| AdvancedAlienMagCannon | TwoNoseVert | 11 | 87.1 | proxy mm | 100 |
| AdvancedAlienHeavyMagCannon | ThreeNoseAngle | 7 | 99.7 | proxy mm | 150 |
| AdvancedAlienSpinalMagCannon | FourNose | 4 | 109.7 | proxy mm | 200 |
| Gen3AlienLightMagBattery | OneHull | 15 | 61.3 | proxy mm | 35 |
| Gen3AlienMagBattery | TwoHullHoriz | 10 | 76.9 | proxy mm | 69 |
| Gen3AlienHeavyMagBattery | FourHull | 6 | 96.9 | proxy mm | 138 |
| Gen3AlienLightMagCannon | OneNose | 16 | 85.0 | proxy mm | 93 |
| Gen3AlienMagCannon | TwoNoseVert | 11 | 106.7 | proxy mm | 184 |
| Gen3AlienHeavyMagCannon | ThreeNoseAngle | 7 | 134.4 | proxy mm | 368 |
| Gen3AlienSpinalMagCannon | FourNose | 4 | 169.3 | proxy mm | 736 |
| PointDefenseLaserTurret | OneHull | 34 | 0.600 | optic m | — |
| 60cmIRLaserBattery | OneHull | 37 | 1.200 | optic m | — |
| 120cmIRLaserBattery | TwoHullHoriz | 30 | 2.400 | optic m | — |
| 360cmIRLaserBattery | FourHull | 24 | 7.200 | optic m | — |
| 240cmIRLaserCannon | OneNose | 38 | 4.800 | optic m | — |
| 480cmIRLaserCannon | TwoNoseVert | 31 | 9.600 | optic m | — |
| 720cmIRLaserCannon | ThreeNoseAngle | 25 | 14.400 | optic m | — |
| 960cmIRLaserCannon | FourNose | 20 | 19.200 | optic m | — |
| PointDefenseArcLaserTurret | OneHull | 34 | 0.600 | optic m | — |
| 60cmIRArcLaserBattery | OneHull | 37 | 1.200 | optic m | — |
| 120cmIRArcLaserBattery | TwoHullHoriz | 30 | 2.400 | optic m | — |
| 360cmIRArcLaserBattery | FourHull | 24 | 7.200 | optic m | — |
| 240cmIRArcLaserCannon | OneNose | 38 | 4.800 | optic m | — |
| 480cmIRArcLaserCannon | TwoNoseVert | 31 | 9.600 | optic m | — |
| 720cmIRArcLaserCannon | ThreeNoseAngle | 25 | 14.400 | optic m | — |
| 960cmIRArcLaserCannon | FourNose | 20 | 19.200 | optic m | — |
| PointDefensePhaserTurret | OneHull | 34 | 0.600 | optic m | — |
| 60cmIRPhaserBattery | OneHull | 37 | 1.200 | optic m | — |
| 120cmIRPhaserBattery | TwoHullHoriz | 30 | 2.400 | optic m | — |
| 360cmIRPhaserBattery | FourHull | 24 | 7.200 | optic m | — |
| 240cmIRPhaserCannon | OneNose | 38 | 4.800 | optic m | — |
| 480cmIRPhaserCannon | TwoNoseVert | 31 | 9.600 | optic m | — |
| 720cmIRPhaserCannon | ThreeNoseAngle | 25 | 14.400 | optic m | — |
| 960cmIRPhaserCannon | FourNose | 20 | 19.200 | optic m | — |
| 60cmGreenLaserBattery | OneHull | 37 | 1.200 | optic m | — |
| 120cmGreenLaserBattery | TwoHullHoriz | 30 | 2.400 | optic m | — |
| 360cmGreenLaserBattery | FourHull | 24 | 7.200 | optic m | — |
| 240cmGreenLaserCannon | OneNose | 38 | 4.800 | optic m | — |
| 480cmGreenLaserCannon | TwoNoseVert | 31 | 9.600 | optic m | — |
| 720cmGreenLaserCannon | ThreeNoseAngle | 25 | 14.400 | optic m | — |
| 960cmGreenLaserCannon | FourNose | 20 | 19.200 | optic m | — |
| 60cmGreenArcLaserBattery | OneHull | 37 | 1.200 | optic m | — |
| 120cmGreenArcLaserBattery | TwoHullHoriz | 30 | 2.400 | optic m | — |
| 360cmGreenArcLaserBattery | FourHull | 24 | 7.200 | optic m | — |
| 240cmGreenArcLaserCannon | OneNose | 38 | 4.800 | optic m | — |
| 480cmGreenArcLaserCannon | TwoNoseVert | 31 | 9.600 | optic m | — |
| 720cmGreenArcLaserCannon | ThreeNoseAngle | 25 | 14.400 | optic m | — |
| 960cmGreenArcLaserCannon | FourNose | 20 | 19.200 | optic m | — |
| 60cmGreenPhaserBattery | OneHull | 37 | 1.200 | optic m | — |
| 120cmGreenPhaserBattery | TwoHullHoriz | 30 | 2.400 | optic m | — |
| 360cmGreenPhaserBattery | FourHull | 24 | 7.200 | optic m | — |
| 240cmGreenPhaserCannon | OneNose | 38 | 4.800 | optic m | — |
| 480cmGreenPhaserCannon | TwoNoseVert | 31 | 9.600 | optic m | — |
| 720cmGreenPhaserCannon | ThreeNoseAngle | 25 | 14.400 | optic m | — |
| 960cmGreenPhaserCannon | FourNose | 20 | 19.200 | optic m | — |
| 60cmUVLaserBattery | OneHull | 37 | 1.200 | optic m | — |
| 120cmUVLaserBattery | TwoHullHoriz | 30 | 2.400 | optic m | — |
| 360cmUVLaserBattery | FourHull | 24 | 7.200 | optic m | — |
| 240cmUVLaserCannon | OneNose | 38 | 4.800 | optic m | — |
| 480cmUVLaserCannon | TwoNoseVert | 31 | 9.600 | optic m | — |
| 720cmUVLaserCannon | ThreeNoseAngle | 25 | 14.400 | optic m | — |
| 960cmUVLaserCannon | FourNose | 20 | 19.200 | optic m | — |
| 60cmUVArcLaserBattery | OneHull | 37 | 1.200 | optic m | — |
| 120cmUVArcLaserBattery | TwoHullHoriz | 30 | 2.400 | optic m | — |
| 360cmUVArcLaserBattery | FourHull | 24 | 7.200 | optic m | — |
| 240cmUVArcLaserCannon | OneNose | 38 | 4.800 | optic m | — |
| 480cmUVArcLaserCannon | TwoNoseVert | 31 | 9.600 | optic m | — |
| 720cmUVArcLaserCannon | ThreeNoseAngle | 25 | 14.400 | optic m | — |
| 960cmUVArcLaserCannon | FourNose | 20 | 19.200 | optic m | — |
| 60cmUVPhaserBattery | OneHull | 37 | 1.200 | optic m | — |
| 120cmUVPhaserBattery | TwoHullHoriz | 30 | 2.400 | optic m | — |
| 360cmUVPhaserBattery | FourHull | 24 | 7.200 | optic m | — |
| 240cmUVPhaserCannon | OneNose | 38 | 4.800 | optic m | — |
| 480cmUVPhaserCannon | TwoNoseVert | 31 | 9.600 | optic m | — |
| 720cmUVPhaserCannon | ThreeNoseAngle | 25 | 14.400 | optic m | — |
| 960cmUVPhaserCannon | FourNose | 20 | 19.200 | optic m | — |
| AlienPointDefenseLaserTurret | OneHull | 14 | 0.640 | optic m | — |
| Alien64cmOrangeLaserBattery | OneHull | 17 | 1.280 | optic m | — |
| Alien128cmOrangeLaserBattery | TwoHullHoriz | 12 | 2.560 | optic m | — |
| Alien384cmOrangeLaserBattery | FourHull | 8 | 7.680 | optic m | — |
| Alien256cmOrangeLaserCannon | OneNose | 18 | 5.120 | optic m | — |
| Alien512cmOrangeLaserCannon | TwoNoseVert | 13 | 10.240 | optic m | — |
| Alien768cmOrangeLaserCannon | ThreeNoseAngle | 9 | 15.360 | optic m | — |
| Alien1024cmOrangeLaserCannon | FourNose | 5 | 20.480 | optic m | — |
| Alien64cmVioletLaserBattery | OneHull | 17 | 1.280 | optic m | — |
| Alien128cmVioletLaserBattery | TwoHullHoriz | 12 | 2.560 | optic m | — |
| Alien384cmVioletLaserBattery | FourHull | 8 | 7.680 | optic m | — |
| Alien256cmVioletLaserCannon | OneNose | 18 | 5.120 | optic m | — |
| Alien512cmVioletLaserCannon | TwoNoseVert | 13 | 10.240 | optic m | — |
| Alien768cmVioletLaserCannon | ThreeNoseAngle | 9 | 15.360 | optic m | — |
| Alien1024cmVioletLaserCannon | FourNose | 5 | 20.480 | optic m | — |
| Alien384cmXaserBattery | FourHull | 8 | 7.680 | optic m | — |
| Alien256cmXaserCannon | OneNose | 18 | 5.120 | optic m | — |
| Alien512cmXaserCannon | TwoNoseVert | 13 | 10.240 | optic m | — |
| Alien768cmXaserCannon | ThreeNoseAngle | 9 | 15.360 | optic m | — |
| Alien1024cmXaserCannon | FourNose | 5 | 20.480 | optic m | — |
| Alien768cmGraserCannon | ThreeNoseAngle | 9 | 15.360 | optic m | — |
| Alien1024cmGraserCannon | FourNose | 5 | 20.480 | optic m | — |


## All 37 records without ship art

These are region and base defenses; their simulation optics are inventoried, but no ship-resource geometry is assigned. They are not silently counted as zero-size weapons.

| Template | Mount | Simulation D m |
|---|---|---|
| RegionDefenseIRLaser | RegionDefense | 72.000 |
| RegionDefenseGreenLaser | RegionDefense | 72.000 |
| RegionDefenseIRArcLaser | RegionDefense | 72.000 |
| RegionDefenseGreenArcLaser | RegionDefense | 72.000 |
| RegionDefenseIRPhaser | RegionDefense | 72.000 |
| RegionDefenseGreenPhaser | RegionDefense | 72.000 |
| T1BaseIRLaser | T1BaseDefense | 19.200 |
| T1BaseGreenLaser | T1BaseDefense | 19.200 |
| T1BaseUVLaser | T1BaseDefense | 19.200 |
| T1BaseIRArcLaser | T1BaseDefense | 19.200 |
| T1BaseGreenArcLaser | T1BaseDefense | 19.200 |
| T1BaseUVArcLaser | T1BaseDefense | 19.200 |
| T1BaseIRPhaser | T1BaseDefense | 19.200 |
| T1BaseGreenPhaser | T1BaseDefense | 19.200 |
| T1BaseUVPhaser | T1BaseDefense | 19.200 |
| T2BaseIRLaser | T2BaseDefense | 28.800 |
| T2BaseGreenLaser | T2BaseDefense | 28.800 |
| T2BaseUVLaser | T2BaseDefense | 28.800 |
| T2BaseIRArcLaser | T2BaseDefense | 28.800 |
| T2BaseGreenArcLaser | T2BaseDefense | 28.800 |
| T2BaseUVArcLaser | T2BaseDefense | 28.800 |
| T2BaseIRPhaser | T2BaseDefense | 28.800 |
| T2BaseGreenPhaser | T2BaseDefense | 28.800 |
| T2BaseUVPhaser | T2BaseDefense | 28.800 |
| T3BaseIRLaser | T3BaseDefense | 38.400 |
| T3BaseGreenLaser | T3BaseDefense | 38.400 |
| T3BaseUVLaser | T3BaseDefense | 38.400 |
| T3BaseIRArcLaser | T3BaseDefense | 38.400 |
| T3BaseGreenArcLaser | T3BaseDefense | 38.400 |
| T3BaseUVArcLaser | T3BaseDefense | 38.400 |
| T3BaseIRPhaser | T3BaseDefense | 38.400 |
| T3BaseGreenPhaser | T3BaseDefense | 38.400 |
| T3BaseUVPhaser | T3BaseDefense | 38.400 |
| AlienRegionDefenseLaser | RegionDefense | 81.920 |
| AlienT1BaseDefenseLaser | T1BaseDefense | 20.480 |
| AlienT2BaseDefenseLaser | T2BaseDefense | 40.960 |
| AlienT3BaseDefenseLaser | T3BaseDefense | 61.440 |

