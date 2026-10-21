# Larger laser aperture sensitivity tables

Graser rows evaluate the authored 0.1 nm wavelength. The native field is an
integer and loaded rounding was not inspected; if it becomes zero, the already
negligible aperture-growth benefit becomes exactly zero.

Read with the [deferred proposal](../larger-laser-aperture-proposal-2026-10-21.md). At 500 km, normal incidence, intact armor, full intercepted shot, no bonuses or laser-resistant material; nominal values before random damage. Targeting range remains unchanged.

| Weapon | Range km | Raw damage pts/s | E at 500 km | Jitter share of E | E delta +25% D | E delta +50% D | E delta +100% D |
|---|---|---|---|---|---|---|---|
| 60cmIRPhaserBattery | 600 | 0.500 | 32.616 | 1.20% | -35.57% | -54.89% | -74.10% |
| 60cmGreenPhaserBattery | 600 | 0.500 | 14.714 | 2.67% | -35.04% | -54.07% | -73.00% |
| 60cmUVPhaserBattery | 600 | 0.500 | 3.973 | 9.88% | -32.44% | -50.06% | -67.59% |
| 120cmGreenPhaserBattery | 700 | 0.750 | 3.832 | 6.56% | -33.64% | -51.91% | -70.08% |
| 360cmGreenPhaserBattery | 850 | 1.250 | 0.559 | 28.79% | -25.64% | -39.56% | -53.41% |
| 240cmGreenPhaserCannon | 800 | 1.000 | 1.001 | 10.60% | -32.18% | -49.66% | -67.05% |
| 480cmGreenPhaserCannon | 900 | 1.500 | 0.287 | 21.92% | -28.11% | -43.38% | -58.56% |
| 720cmGreenPhaserCannon | 950 | 1.750 | 0.140 | 28.79% | -25.64% | -39.56% | -53.41% |
| 960cmGreenPhaserCannon | 1000 | 2.000 | 0.082 | 32.18% | -24.41% | -37.68% | -50.86% |
| Alien384cmXaserBattery | 900 | 0.500 | 0.161 | 99.93% | -0.02% | -0.04% | -0.05% |
| Alien1024cmGraserCannon | 1000 | 0.622 | 0.016 | 100.00% | -0.00% | -0.00% | -0.00% |


## Absolute nominal penetration examples

Armor threshold is the continuous A for zero nominal penetrating damage; values above it stop this nominal intact hit. Thresholds are not guaranteed penetration limits because damage is randomized. DPS below is penetration through intact 10 armor, not total chip/internal damage or simulated battle results.

| Weapon | D scale | D m | Spot m | Nominal armor threshold | Nominal DPS vs A=10 |
|---|---|---|---|---|---|
| 60cmIRPhaserBattery | 1 | 1.200 | 0.4557 | 0.28 | 0.000 |
| 60cmIRPhaserBattery | 1.25 | 1.500 | 0.3658 | 0.38 | 0.000 |
| 60cmIRPhaserBattery | 1.5 | 1.800 | 0.3061 | 0.48 | 0.000 |
| 60cmIRPhaserBattery | 2 | 2.400 | 0.2319 | 0.70 | 0.000 |
| 60cmGreenPhaserBattery | 1 | 1.200 | 0.3061 | 0.48 | 0.000 |
| 60cmGreenPhaserBattery | 1.25 | 1.500 | 0.2467 | 0.65 | 0.000 |
| 60cmGreenPhaserBattery | 1.5 | 1.800 | 0.2074 | 0.81 | 0.000 |
| 60cmGreenPhaserBattery | 2 | 2.400 | 0.1590 | 1.16 | 0.000 |
| 60cmUVPhaserBattery | 1 | 1.200 | 0.1590 | 1.16 | 0.000 |
| 60cmUVPhaserBattery | 1.25 | 1.500 | 0.1307 | 1.51 | 0.000 |
| 60cmUVPhaserBattery | 1.5 | 1.800 | 0.1124 | 1.85 | 0.000 |
| 60cmUVPhaserBattery | 2 | 2.400 | 0.0905 | 2.47 | 0.000 |
| 120cmGreenPhaserBattery | 1 | 2.400 | 0.1562 | 1.56 | 0.000 |
| 120cmGreenPhaserBattery | 1.25 | 3.000 | 0.1272 | 2.05 | 0.000 |
| 120cmGreenPhaserBattery | 1.5 | 3.600 | 0.1083 | 2.55 | 0.000 |
| 120cmGreenPhaserBattery | 2 | 4.800 | 0.0854 | 3.50 | 0.000 |
| 360cmGreenPhaserBattery | 1 | 7.200 | 0.0596 | 7.94 | 0.000 |
| 360cmGreenPhaserBattery | 1.25 | 9.000 | 0.0514 | 9.67 | 0.000 |
| 360cmGreenPhaserBattery | 1.5 | 10.800 | 0.0464 | 11.11 | 0.182 |
| 360cmGreenPhaserBattery | 2 | 14.400 | 0.0407 | 13.21 | 0.427 |
| 240cmGreenPhaserCannon | 1 | 4.800 | 0.0798 | 4.63 | 0.000 |
| 240cmGreenPhaserCannon | 1.25 | 6.000 | 0.0657 | 6.01 | 0.000 |
| 240cmGreenPhaserCannon | 1.5 | 7.200 | 0.0566 | 7.33 | 0.000 |
| 240cmGreenPhaserCannon | 2 | 9.600 | 0.0458 | 9.72 | 0.000 |
| 480cmGreenPhaserCannon | 1 | 9.600 | 0.0427 | 13.99 | 0.593 |
| 480cmGreenPhaserCannon | 1.25 | 12.000 | 0.0362 | 17.43 | 0.848 |
| 480cmGreenPhaserCannon | 1.5 | 14.400 | 0.0321 | 20.44 | 0.987 |
| 480cmGreenPhaserCannon | 2 | 19.200 | 0.0275 | 25.17 | 1.124 |
| 720cmGreenPhaserCannon | 1 | 14.400 | 0.0298 | 25.04 | 1.308 |
| 720cmGreenPhaserCannon | 1.25 | 18.000 | 0.0257 | 30.50 | 1.421 |
| 720cmGreenPhaserCannon | 1.5 | 21.600 | 0.0232 | 35.03 | 1.483 |
| 720cmGreenPhaserCannon | 2 | 28.800 | 0.0204 | 41.66 | 1.544 |
| 960cmGreenPhaserCannon | 1 | 19.200 | 0.0229 | 38.88 | 1.739 |
| 960cmGreenPhaserCannon | 1.25 | 24.000 | 0.0199 | 46.86 | 1.803 |
| 960cmGreenPhaserCannon | 1.5 | 28.800 | 0.0181 | 53.29 | 1.837 |
| 960cmGreenPhaserCannon | 2 | 38.400 | 0.0161 | 62.44 | 1.872 |
| Alien384cmXaserBattery | 1 | 7.680 | 0.0320 | 17.71 | 0.288 |
| Alien384cmXaserBattery | 1.25 | 9.600 | 0.0320 | 17.71 | 0.288 |
| Alien384cmXaserBattery | 1.5 | 11.520 | 0.0320 | 17.72 | 0.288 |
| Alien384cmXaserBattery | 2 | 15.360 | 0.0320 | 17.72 | 0.288 |
| Alien1024cmGraserCannon | 1 | 20.480 | 0.0100 | 126.69 | 0.608 |
| Alien1024cmGraserCannon | 1.25 | 25.600 | 0.0100 | 126.69 | 0.608 |
| Alien1024cmGraserCannon | 1.5 | 30.720 | 0.0100 | 126.69 | 0.608 |
| Alien1024cmGraserCannon | 2 | 40.960 | 0.0100 | 126.69 | 0.608 |

