# Projected weapon dimensions and size-tier distribution

Planning only. [Original weapon inventory](../turret-clearance/weapon-inventory.md), [canonical plan](../canonical-turret-scaling-plan-2026-10-21.md). The 3D sphere is the original conservative clearance metric; horizontal motion diameter is the later sampled-and-bounded footprint metric. These are not solid model widths. Proportional scaling is around the original mount origin; all optical/gameplay stats remain unchanged.

## Largest hull weapon at each size

| Scenario | Faction | Size | Limiting example | 3D sphere D m | Horizontal D m | Visible W × H × L m |
| --- | --- | --- | --- | --- | --- | --- |
| baseline | Human | 1 | 30mmAutocannon | 12.952 | 9.673 | 3.495 × 2.873 × 6.110 |
| baseline | Human | 2 | 8-inchCannon | 12.175 | 9.024 | 3.495 × 2.806 × 5.800 |
| baseline | Human | 4 | HeavyPlasmaBatteryMk1 | 14.637 | 12.749 | 5.843 × 2.349 × 9.576 |
| baseline | Alien | 1 | AlienLightMagBattery | 8.774 | 7.574 | 1.648 × 1.147 × 4.118 |
| baseline | Alien | 2 | AlienPlasmaBattery | 12.037 | 10.030 | 2.831 × 1.656 × 5.329 |
| baseline | Alien | 4 | AlienHeavyPlasmaBattery | 12.296 | 11.793 | 5.006 × 1.521 × 8.044 |
| canonical | Human | 1 | 6-inchCannon | 10.429 | 7.236 | 3.431 × 2.791 × 4.894 |
| canonical | Human | 2 | 8-inchCannon | 12.917 | 9.574 | 3.708 × 2.977 × 6.153 |
| canonical | Human | 4 | 360cmIRLaserBattery | 17.499 | 10.518 | 10.268 × 10.404 × 9.597 |
| canonical | Alien | 1 | Alien64cmOrangeLaserBattery | 11.688 | 8.449 | 3.994 × 2.657 × 5.578 |
| canonical | Alien | 2 | Alien128cmOrangeLaserBattery | 31.794 | 23.989 | 11.227 × 5.966 × 13.187 |
| canonical | Alien | 4 | Alien384cmOrangeLaserBattery | 96.966 | 89.298 | 46.224 × 14.041 × 62.985 |
| compact_optics | Human | 1 | 6-inchCannon | 10.429 | 7.236 | 3.431 × 2.791 × 4.894 |
| compact_optics | Human | 2 | 8-inchCannon | 12.917 | 9.574 | 3.708 × 2.977 × 6.153 |
| compact_optics | Human | 4 | 360cmIRLaserBattery | 17.499 | 10.518 | 10.268 × 10.404 × 9.597 |
| compact_optics | Alien | 1 | Gen3AlienLightMagBattery | 8.832 | 7.624 | 1.658 × 1.155 × 4.145 |
| compact_optics | Alien | 2 | AlienPlasmaBattery | 12.037 | 10.030 | 2.831 × 1.656 × 5.329 |
| compact_optics | Alien | 4 | Alien384cmOrangeLaserBattery | 21.640 | 9.216 | 9.216 × 10.865 × 9.216 |
| head_only | Human | 1 | 6-inchCannon | 10.490 | — | 3.495 × 2.822 × 4.919 |
| head_only | Human | 2 | 8-inchCannon | 12.719 | — | 3.495 × 2.875 × 6.069 |
| head_only | Human | 4 | HeavyPlasmaBatteryMk1 | 14.637 | — | 5.843 × 2.349 × 9.576 |
| head_only | Alien | 1 | Alien64cmOrangeLaserBattery | 9.713 | — | 3.994 × 2.393 × 5.578 |
| head_only | Alien | 2 | Alien128cmOrangeLaserBattery | 25.400 | — | 11.227 × 4.711 × 13.187 |
| head_only | Alien | 4 | Alien384cmOrangeLaserBattery | 81.699 | — | 32.657 × 10.746 × 40.049 |



## Every original hull template, projected separately

Shared resources can require different factors for calibers or generations. Identical wavelength/mark art is still listed so no weapon silently inherits another template’s scale. Compact differs from proportional only for alien lasers.

| Weapon | Size | Scale | Original W/H/L m | Proportional W/H/L m | Sphere old m | Sphere new m | Sphere delta | Horizontal old m | Horizontal new m | Compact sphere m |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | 1 | 0.1999 | 3.495 / 2.873 / 6.110 | 0.699 / 0.574 / 1.221 | 12.952 | 2.589 | -10.363 | 9.673 | 1.933 | 2.589 |
| 40mmAutocannon | 1 | 0.2665 | 3.495 / 2.873 / 6.110 | 0.931 / 0.766 / 1.628 | 12.952 | 3.451 | -9.501 | 9.673 | 2.578 | 3.451 |
| 6-inchCannon | 1 | 0.9816 | 3.495 / 2.843 / 4.986 | 3.431 / 2.791 / 4.894 | 10.624 | 10.429 | -0.195 | 7.371 | 7.236 | 10.429 |
| 8-inchCannon | 2 | 1.0609 | 3.495 / 2.806 / 5.800 | 3.708 / 2.977 / 6.153 | 12.175 | 12.917 | +0.742 | 9.024 | 9.574 | 12.917 |
| LightRailgunBatteryMk1 | 1 | 0.4917 | 3.495 / 2.192 / 3.868 | 1.719 / 1.078 / 1.902 | 8.773 | 4.314 | -4.459 | 5.173 | 2.544 | 4.314 |
| LightRailgunBatteryMk2 | 1 | 0.4917 | 3.495 / 2.192 / 3.868 | 1.719 / 1.078 / 1.902 | 8.773 | 4.314 | -4.459 | 5.173 | 2.544 | 4.314 |
| LightRailgunBatteryMk3 | 1 | 0.4917 | 3.495 / 2.192 / 3.868 | 1.719 / 1.078 / 1.902 | 8.773 | 4.314 | -4.459 | 5.173 | 2.544 | 4.314 |
| RailgunBatteryMk1 | 2 | 0.7376 | 3.495 / 2.214 / 4.450 | 2.578 / 1.633 / 3.282 | 10.000 | 7.376 | -2.624 | 6.327 | 4.667 | 7.376 |
| RailgunBatteryMk2 | 2 | 0.7376 | 3.495 / 2.214 / 4.450 | 2.578 / 1.633 / 3.282 | 10.000 | 7.376 | -2.624 | 6.327 | 4.667 | 7.376 |
| RailgunBatteryMk3 | 2 | 0.7376 | 3.495 / 2.214 / 4.450 | 2.578 / 1.633 / 3.282 | 10.000 | 7.376 | -2.624 | 6.327 | 4.667 | 7.376 |
| HeavyRailgunBatteryMk1 | 4 | 0.4130 | 5.843 / 2.349 / 8.886 | 2.413 / 0.970 / 3.670 | 13.597 | 5.616 | -7.981 | 11.414 | 4.715 | 5.616 |
| HeavyRailgunBatteryMk2 | 4 | 0.4130 | 5.843 / 2.349 / 8.886 | 2.413 / 0.970 / 3.670 | 13.597 | 5.616 | -7.981 | 11.414 | 4.715 | 5.616 |
| HeavyRailgunBatteryMk3 | 4 | 0.4130 | 5.843 / 2.349 / 8.886 | 2.413 / 0.970 / 3.670 | 13.597 | 5.616 | -7.981 | 11.414 | 4.715 | 5.616 |
| LightCoilgunBatteryMk1 | 1 | 0.2696 | 3.495 / 2.312 / 4.312 | 0.942 / 0.623 / 1.162 | 9.371 | 2.526 | -6.845 | 6.056 | 1.633 | 2.526 |
| LightCoilgunBatteryMk2 | 1 | 0.2696 | 3.495 / 2.312 / 4.312 | 0.942 / 0.623 / 1.162 | 9.371 | 2.526 | -6.845 | 6.056 | 1.633 | 2.526 |
| LightCoilgunBatteryMk3 | 1 | 0.2696 | 3.495 / 2.312 / 4.312 | 0.942 / 0.623 / 1.162 | 9.371 | 2.526 | -6.845 | 6.056 | 1.633 | 2.526 |
| CoilgunBatteryMk1 | 2 | 0.3352 | 3.495 / 2.277 / 4.209 | 1.172 / 0.763 / 1.411 | 9.302 | 3.118 | -6.184 | 6.062 | 2.032 | 3.118 |
| CoilgunBatteryMk2 | 2 | 0.3352 | 3.495 / 2.277 / 4.209 | 1.172 / 0.763 / 1.411 | 9.302 | 3.118 | -6.184 | 6.062 | 2.032 | 3.118 |
| CoilgunBatteryMk3 | 2 | 0.3352 | 3.495 / 2.277 / 4.209 | 1.172 / 0.763 / 1.411 | 9.302 | 3.118 | -6.184 | 6.062 | 2.032 | 3.118 |
| HeavyCoilgunBatteryMk1 | 4 | 0.5174 | 5.843 / 2.349 / 8.038 | 3.023 / 1.215 / 4.159 | 13.118 | 6.787 | -6.331 | 10.822 | 5.599 | 6.787 |
| HeavyCoilgunBatteryMk2 | 4 | 0.5174 | 5.843 / 2.349 / 8.038 | 3.023 / 1.215 / 4.159 | 13.118 | 6.787 | -6.331 | 10.822 | 5.599 | 6.787 |
| HeavyCoilgunBatteryMk3 | 4 | 0.5174 | 5.843 / 2.349 / 8.038 | 3.023 / 1.215 / 4.159 | 13.118 | 6.787 | -6.331 | 10.822 | 5.599 | 6.787 |
| AlienLightMagBattery | 1 | 0.8211 | 1.648 / 1.147 / 4.118 | 1.353 / 0.942 / 3.381 | 8.774 | 7.205 | -1.569 | 7.574 | 6.220 | 7.205 |
| AlienMagBattery | 2 | 0.7303 | 2.831 / 1.640 / 5.250 | 2.067 / 1.198 / 3.834 | 11.807 | 8.623 | -3.184 | 9.608 | 7.017 | 8.623 |
| AlienHeavyMagBattery | 4 | 0.8717 | 5.143 / 1.562 / 7.223 | 4.483 / 1.362 / 6.296 | 10.222 | 8.910 | -1.312 | 9.943 | 8.667 | 8.910 |
| AdvancedAlienLightMagBattery | 1 | 0.8211 | 1.648 / 1.147 / 4.118 | 1.353 / 0.942 / 3.381 | 8.774 | 7.205 | -1.569 | 7.574 | 6.220 | 7.205 |
| AdvancedAlienMagBattery | 2 | 0.7303 | 2.831 / 1.640 / 5.250 | 2.067 / 1.198 / 3.834 | 11.807 | 8.623 | -3.184 | 9.608 | 7.017 | 8.623 |
| AdvancedAlienHeavyMagBattery | 4 | 0.8717 | 5.143 / 1.562 / 7.223 | 4.483 / 1.362 / 6.296 | 10.222 | 8.910 | -1.312 | 9.943 | 8.667 | 8.910 |
| Gen3AlienLightMagBattery | 1 | 1.0066 | 1.648 / 1.147 / 4.118 | 1.658 / 1.155 / 4.145 | 8.774 | 8.832 | +0.058 | 7.574 | 7.624 | 8.832 |
| Gen3AlienMagBattery | 2 | 0.8910 | 2.831 / 1.640 / 5.250 | 2.522 / 1.461 / 4.678 | 11.807 | 10.520 | -1.287 | 9.608 | 8.561 | 10.520 |
| Gen3AlienHeavyMagBattery | 4 | 1.0681 | 5.143 / 1.562 / 7.223 | 5.493 / 1.668 / 7.715 | 10.222 | 10.919 | +0.696 | 9.943 | 10.621 | 10.919 |
| PointDefenseLaserTurret | 1 | 0.8646 | 0.977 / 1.064 / 0.913 | 0.845 / 0.920 / 0.790 | 1.834 | 1.586 | -0.248 | 1.056 | 0.913 | 1.586 |
| 60cmIRLaserBattery | 1 | 1.1227 | 1.530 / 1.607 / 1.430 | 1.718 / 1.804 / 1.606 | 2.755 | 3.093 | +0.338 | 1.627 | 1.827 | 3.093 |
| 120cmIRLaserBattery | 2 | 1.4645 | 2.329 / 2.441 / 2.176 | 3.410 / 3.574 / 3.187 | 4.145 | 6.070 | +1.925 | 2.386 | 3.494 | 6.070 |
| 360cmIRLaserBattery | 4 | 2.7603 | 3.720 / 3.769 / 3.477 | 10.268 / 10.404 / 9.597 | 6.339 | 17.499 | +11.159 | 3.811 | 10.518 | 17.499 |
| PointDefenseArcLaserTurret | 1 | 0.8646 | 0.977 / 1.064 / 0.913 | 0.845 / 0.920 / 0.790 | 1.834 | 1.586 | -0.248 | 1.056 | 0.913 | 1.586 |
| 60cmIRArcLaserBattery | 1 | 1.1227 | 1.530 / 1.607 / 1.430 | 1.718 / 1.804 / 1.606 | 2.755 | 3.093 | +0.338 | 1.627 | 1.827 | 3.093 |
| 120cmIRArcLaserBattery | 2 | 1.4645 | 2.329 / 2.441 / 2.176 | 3.410 / 3.574 / 3.187 | 4.145 | 6.070 | +1.925 | 2.386 | 3.494 | 6.070 |
| 360cmIRArcLaserBattery | 4 | 2.7603 | 3.720 / 3.769 / 3.477 | 10.268 / 10.404 / 9.597 | 6.339 | 17.499 | +11.159 | 3.811 | 10.518 | 17.499 |
| PointDefensePhaserTurret | 1 | 0.8646 | 0.977 / 1.064 / 0.913 | 0.845 / 0.920 / 0.790 | 1.834 | 1.586 | -0.248 | 1.056 | 0.913 | 1.586 |
| 60cmIRPhaserBattery | 1 | 1.1227 | 1.530 / 1.607 / 1.430 | 1.718 / 1.804 / 1.606 | 2.755 | 3.093 | +0.338 | 1.627 | 1.827 | 3.093 |
| 120cmIRPhaserBattery | 2 | 1.4645 | 2.329 / 2.441 / 2.176 | 3.410 / 3.574 / 3.187 | 4.145 | 6.070 | +1.925 | 2.386 | 3.494 | 6.070 |
| 360cmIRPhaserBattery | 4 | 2.7603 | 3.720 / 3.769 / 3.477 | 10.268 / 10.404 / 9.597 | 6.339 | 17.499 | +11.159 | 3.811 | 10.518 | 17.499 |
| 60cmGreenLaserBattery | 1 | 1.1227 | 1.530 / 1.607 / 1.430 | 1.718 / 1.804 / 1.606 | 2.755 | 3.093 | +0.338 | 1.627 | 1.827 | 3.093 |
| 120cmGreenLaserBattery | 2 | 1.4645 | 2.329 / 2.441 / 2.176 | 3.410 / 3.574 / 3.187 | 4.145 | 6.070 | +1.925 | 2.386 | 3.494 | 6.070 |
| 360cmGreenLaserBattery | 4 | 2.7603 | 3.720 / 3.769 / 3.477 | 10.268 / 10.404 / 9.597 | 6.339 | 17.499 | +11.159 | 3.811 | 10.518 | 17.499 |
| 60cmGreenArcLaserBattery | 1 | 1.1227 | 1.530 / 1.607 / 1.430 | 1.718 / 1.804 / 1.606 | 2.755 | 3.093 | +0.338 | 1.627 | 1.827 | 3.093 |
| 120cmGreenArcLaserBattery | 2 | 1.4645 | 2.329 / 2.441 / 2.176 | 3.410 / 3.574 / 3.187 | 4.145 | 6.070 | +1.925 | 2.386 | 3.494 | 6.070 |
| 360cmGreenArcLaserBattery | 4 | 2.7603 | 3.720 / 3.769 / 3.477 | 10.268 / 10.404 / 9.597 | 6.339 | 17.499 | +11.159 | 3.811 | 10.518 | 17.499 |
| 60cmGreenPhaserBattery | 1 | 1.1227 | 1.530 / 1.607 / 1.430 | 1.718 / 1.804 / 1.606 | 2.755 | 3.093 | +0.338 | 1.627 | 1.827 | 3.093 |
| 120cmGreenPhaserBattery | 2 | 1.4645 | 2.329 / 2.441 / 2.176 | 3.410 / 3.574 / 3.187 | 4.145 | 6.070 | +1.925 | 2.386 | 3.494 | 6.070 |
| 360cmGreenPhaserBattery | 4 | 2.7603 | 3.720 / 3.769 / 3.477 | 10.268 / 10.404 / 9.597 | 6.339 | 17.499 | +11.159 | 3.811 | 10.518 | 17.499 |
| 60cmUVLaserBattery | 1 | 1.1227 | 1.530 / 1.607 / 1.430 | 1.718 / 1.804 / 1.606 | 2.755 | 3.093 | +0.338 | 1.627 | 1.827 | 3.093 |
| 120cmUVLaserBattery | 2 | 1.4645 | 2.329 / 2.441 / 2.176 | 3.410 / 3.574 / 3.187 | 4.145 | 6.070 | +1.925 | 2.386 | 3.494 | 6.070 |
| 360cmUVLaserBattery | 4 | 2.7603 | 3.720 / 3.769 / 3.477 | 10.268 / 10.404 / 9.597 | 6.339 | 17.499 | +11.159 | 3.811 | 10.518 | 17.499 |
| 60cmUVArcLaserBattery | 1 | 1.1227 | 1.530 / 1.607 / 1.430 | 1.718 / 1.804 / 1.606 | 2.755 | 3.093 | +0.338 | 1.627 | 1.827 | 3.093 |
| 120cmUVArcLaserBattery | 2 | 1.4645 | 2.329 / 2.441 / 2.176 | 3.410 / 3.574 / 3.187 | 4.145 | 6.070 | +1.925 | 2.386 | 3.494 | 6.070 |
| 360cmUVArcLaserBattery | 4 | 2.7603 | 3.720 / 3.769 / 3.477 | 10.268 / 10.404 / 9.597 | 6.339 | 17.499 | +11.159 | 3.811 | 10.518 | 17.499 |
| 60cmUVPhaserBattery | 1 | 1.1227 | 1.530 / 1.607 / 1.430 | 1.718 / 1.804 / 1.606 | 2.755 | 3.093 | +0.338 | 1.627 | 1.827 | 3.093 |
| 120cmUVPhaserBattery | 2 | 1.4645 | 2.329 / 2.441 / 2.176 | 3.410 / 3.574 / 3.187 | 4.145 | 6.070 | +1.925 | 2.386 | 3.494 | 6.070 |
| 360cmUVPhaserBattery | 4 | 2.7603 | 3.720 / 3.769 / 3.477 | 10.268 / 10.404 / 9.597 | 6.339 | 17.499 | +11.159 | 3.811 | 10.518 | 17.499 |
| AlienPointDefenseLaserTurret | 1 | 0.8861 | 2.403 / 2.460 / 2.188 | 2.129 / 2.180 / 1.939 | 5.748 | 5.093 | -0.655 | 3.767 | 3.338 | 3.438 |
| Alien64cmOrangeLaserBattery | 1 | 2.4244 | 1.648 / 1.096 / 2.301 | 3.994 / 2.657 / 5.578 | 4.821 | 11.688 | +6.867 | 3.485 | 8.449 | 4.376 |
| Alien128cmOrangeLaserBattery | 2 | 4.7720 | 2.353 / 1.250 / 2.763 | 11.227 / 5.966 / 13.187 | 6.663 | 31.794 | +25.131 | 5.027 | 23.989 | 7.448 |
| Alien384cmOrangeLaserBattery | 4 | 8.5144 | 5.429 / 1.649 / 7.397 | 46.224 / 14.041 / 62.985 | 11.388 | 96.966 | +85.577 | 10.488 | 89.298 | 21.640 |
| Alien64cmVioletLaserBattery | 1 | 2.4244 | 1.648 / 1.096 / 2.301 | 3.994 / 2.657 / 5.578 | 4.821 | 11.688 | +6.867 | 3.485 | 8.449 | 4.376 |
| Alien128cmVioletLaserBattery | 2 | 4.7720 | 2.353 / 1.250 / 2.763 | 11.227 / 5.966 / 13.187 | 6.663 | 31.794 | +25.131 | 5.027 | 23.989 | 7.448 |
| Alien384cmVioletLaserBattery | 4 | 8.5144 | 5.429 / 1.649 / 7.397 | 46.224 / 14.041 / 62.985 | 11.388 | 96.966 | +85.577 | 10.488 | 89.298 | 21.640 |
| Alien384cmXaserBattery | 4 | 8.5144 | 5.429 / 1.649 / 7.397 | 46.224 / 14.041 / 62.985 | 11.388 | 96.966 | +85.577 | 10.488 | 89.298 | 21.640 |
| PointDefenseE-Beamer | 1 | 1.0000 | 2.597 / 1.168 / 3.643 | 2.597 / 1.168 / 3.643 | 5.355 | 5.355 | +0.000 | 4.618 | 4.618 | 5.355 |
| LightE-BeamBattery | 1 | 1.0000 | 3.495 / 2.262 / 4.617 | 3.495 / 2.262 / 4.617 | 10.034 | 10.034 | +0.000 | 6.726 | 6.726 | 10.034 |
| E-BeamBattery | 2 | 1.0000 | 3.495 / 2.231 / 4.701 | 3.495 / 2.231 / 4.701 | 10.106 | 10.106 | +0.000 | 6.857 | 6.857 | 10.106 |
| HeavyE-BeamBattery | 4 | 1.0000 | 3.495 / 3.406 / 4.589 | 3.495 / 3.406 / 4.589 | 10.020 | 10.020 | +0.000 | 6.805 | 6.805 | 10.020 |
| PointDefenseIonBattery | 1 | 1.0000 | 2.597 / 1.168 / 3.643 | 2.597 / 1.168 / 3.643 | 5.355 | 5.355 | +0.000 | 4.618 | 4.618 | 5.355 |
| LightIonBattery | 1 | 1.0000 | 3.495 / 2.262 / 4.617 | 3.495 / 2.262 / 4.617 | 10.034 | 10.034 | +0.000 | 6.726 | 6.726 | 10.034 |
| IonBattery | 2 | 1.0000 | 3.495 / 2.231 / 4.701 | 3.495 / 2.231 / 4.701 | 10.106 | 10.106 | +0.000 | 6.857 | 6.857 | 10.106 |
| HeavyIonBattery | 4 | 1.0000 | 3.495 / 3.406 / 4.589 | 3.495 / 3.406 / 4.589 | 10.020 | 10.020 | +0.000 | 6.805 | 6.805 | 10.020 |
| ParticleBeamBattery | 2 | 1.0000 | 3.495 / 2.231 / 4.701 | 3.495 / 2.231 / 4.701 | 10.106 | 10.106 | +0.000 | 6.857 | 6.857 | 10.106 |
| HeavyParticleBeamBattery | 4 | 1.0000 | 3.495 / 3.406 / 4.589 | 3.495 / 3.406 / 4.589 | 10.020 | 10.020 | +0.000 | 6.805 | 6.805 | 10.020 |
| AntimatterPointDefenseBattery | 2 | 1.0000 | 2.236 / 1.485 / 3.802 | 2.236 / 1.485 / 3.802 | 6.833 | 6.833 | +0.000 | 5.013 | 5.013 | 6.833 |
| HeavyAntimatterPointDefenseBattery | 4 | 1.0000 | 2.236 / 1.574 / 5.259 | 2.236 / 1.574 / 5.259 | 9.669 | 9.669 | +0.000 | 7.882 | 7.882 | 9.669 |
| AlienPointDefenseParticleBeam | 1 | 1.0000 | 3.703 / 1.125 / 5.739 | 3.703 / 1.125 / 5.739 | 8.565 | 8.565 | +0.000 | 8.194 | 8.194 | 8.565 |
| PlasmaBatteryMk1 | 2 | 1.0000 | 2.729 / 1.355 / 4.381 | 2.729 / 1.355 / 4.381 | 7.409 | 7.409 | +0.000 | 6.378 | 6.378 | 7.409 |
| PlasmaBatteryMk2 | 2 | 1.0000 | 2.729 / 1.355 / 4.381 | 2.729 / 1.355 / 4.381 | 7.409 | 7.409 | +0.000 | 6.378 | 6.378 | 7.409 |
| PlasmaBatteryMk3 | 2 | 1.0000 | 2.729 / 1.355 / 4.381 | 2.729 / 1.355 / 4.381 | 7.409 | 7.409 | +0.000 | 6.378 | 6.378 | 7.409 |
| HeavyPlasmaBatteryMk1 | 4 | 1.0000 | 5.843 / 2.349 / 9.576 | 5.843 / 2.349 / 9.576 | 14.637 | 14.637 | +0.000 | 12.749 | 12.749 | 14.637 |
| HeavyPlasmaBatteryMk2 | 4 | 1.0000 | 5.843 / 2.349 / 9.576 | 5.843 / 2.349 / 9.576 | 14.637 | 14.637 | +0.000 | 12.749 | 12.749 | 14.637 |
| HeavyPlasmaBatteryMk3 | 4 | 1.0000 | 5.843 / 2.349 / 9.576 | 5.843 / 2.349 / 9.576 | 14.637 | 14.637 | +0.000 | 12.749 | 12.749 | 14.637 |
| AlienPlasmaBattery | 2 | 1.0000 | 2.831 / 1.656 / 5.329 | 2.831 / 1.656 / 5.329 | 12.037 | 12.037 | +0.000 | 10.030 | 10.030 | 12.037 |
| AlienHeavyPlasmaBattery | 4 | 1.0000 | 5.006 / 1.521 / 8.044 | 5.006 / 1.521 / 8.044 | 12.296 | 12.296 | +0.000 | 11.793 | 11.793 | 12.296 |
| KraitMissileBay | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| AnacondaMissileBay | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| CobraMissileBay | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| RacerMissileBay | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| RattlerMissileBay | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| RiverjackMissileBay | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| HarlequinMissileBay | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| KeelbackMissileBay | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| CopperheadMissileBay | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| ViperMissileBay | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| LanceheadMissileBay | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| PythonNuclearMissileBay | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| SidewinderNuclearMissileBay | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| HeraTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| HermesTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| HerculesTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| ArtemisTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| VulcanTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| ApolloTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| AthenaTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| AresTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| PoseidonTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| ZeusTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| CerebrusNuclearTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| HadesNuclearTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| NemesisNuclearTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| OlympusNuclearTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| AcheronNuclearTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| TartarusNuclearTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| StyxNuclearTorpedoBay | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| AntimatterTorpedoLauncher | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| KraitMissilePod | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| AnacondaMissilePod | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| CobraMissilePod | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| RacerMissilePod | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| RattlerMissilePod | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| RiverjackMissilePod | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| HarlequinMissilePod | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| KeelbackMissilePod | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| CopperheadMissilePod | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| ViperMissilePod | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| LanceheadMissilePod | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| PythonNuclearMissilePod | 1 | 1.0000 | 1.258 / 0.602 / 1.341 | 1.258 / 0.602 / 1.341 | 3.999 | 3.999 | +0.000 | 3.813 | 3.813 | 3.999 |
| HeraTorpedoPod | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| HermesTorpedoPod | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| HerculesTorpedoPod | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| ArtemisTorpedoPod | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| VulcanTorpedoPod | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| ApolloTorpedoPod | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| CerebrusNuclearTorpedoPod | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| HadesNuclearTorpedoPod | 1 | 1.0000 | 2.608 / 1.287 / 2.807 | 2.608 / 1.287 / 2.807 | 8.365 | 8.365 | +0.000 | 7.330 | 7.330 | 8.365 |
| GlitteringJewelMissileBay | 1 | 1.0000 | 1.221 / 0.615 / 1.221 | 1.221 / 0.615 / 1.221 | 3.434 | 3.434 | +0.000 | 3.002 | 3.002 | 3.434 |
| GlitteringJewelMissilePod | 1 | 1.0000 | 1.221 / 0.615 / 1.221 | 1.221 / 0.615 / 1.221 | 3.434 | 3.434 | +0.000 | 3.002 | 3.002 | 3.434 |
| IridescentStarTorpedoBay | 1 | 1.0000 | 2.000 / 1.031 / 2.000 | 2.000 / 1.031 / 2.000 | 5.680 | 5.680 | +0.000 | 4.958 | 4.958 | 5.680 |
| LuminousSwarmMissileBay | 1 | 1.0000 | 1.221 / 0.615 / 1.221 | 1.221 / 0.615 / 1.221 | 3.434 | 3.434 | +0.000 | 3.002 | 3.002 | 3.434 |
| BrilliantSkyMissileBay | 1 | 1.0000 | 1.221 / 0.615 / 1.221 | 1.221 / 0.615 / 1.221 | 3.434 | 3.434 | +0.000 | 3.002 | 3.002 | 3.434 |
| PredatoryStarTorpedoBay | 1 | 1.0000 | 2.000 / 1.031 / 2.000 | 2.000 / 1.031 / 2.000 | 5.680 | 5.680 | +0.000 | 4.958 | 4.958 | 5.680 |



## Complete gun/magnetic/laser target ledger, including noses

Gun and magnetic targets use polygon inscribed diameter, allowing a centered circular projectile rather than only matching corner-to-corner width. Magnetic diameters are EEO collision proxies, not authored calibers. No sabot clearance is invented. Closed rail faces stay unresolved at scale 1; plasma, particles and missiles retain original art and are covered in the hull table above. Lens targets are native 2 × radius, including PD.

| Template | Mount | Reference | Measured m | Target m | Scale | Proportional W × H × L m |
| --- | --- | --- | --- | --- | --- | --- |
| 30mmAutocannon | OneHull | declared caliber / inscribed muzzle | 0.1501 | 0.0300 | 0.1999 | 0.699 × 0.574 × 1.221 |
| 35mmAutocannon | HalfNose | declared caliber / inscribed muzzle | 0.1501 | 0.0350 | 0.2332 | 0.815 × 0.670 × 1.425 |
| 40mmNoseAutocannon | HalfNose | declared caliber / inscribed muzzle | 0.1501 | 0.0400 | 0.2665 | 0.931 × 0.766 × 1.628 |
| 40mmAutocannon | OneHull | declared caliber / inscribed muzzle | 0.1501 | 0.0400 | 0.2665 | 0.931 × 0.766 × 1.628 |
| 6-inchCannon | OneHull | declared caliber / inscribed muzzle | 0.1552 | 0.1524 | 0.9816 | 3.431 × 2.791 × 4.894 |
| 8-inchCannon | TwoHullHoriz | declared caliber / inscribed muzzle | 0.1915 | 0.2032 | 1.0609 | 3.708 × 2.977 × 6.153 |
| 10-inchCannon | OneNose | declared caliber / inscribed muzzle | 0.1915 | 0.2540 | 1.3262 | 4.635 × 3.721 × 7.692 |
| 12-inchCannon | TwoNoseVert | declared caliber / inscribed muzzle | 0.2579 | 0.3048 | 1.1816 | 4.130 × 3.345 × 7.925 |
| LightRailgunBatteryMk1 | OneHull | derived magnetic proxy | 0.0919 | 0.0452 | 0.4917 | 1.719 × 1.078 × 1.902 |
| LightRailgunBatteryMk2 | OneHull | derived magnetic proxy | 0.0919 | 0.0452 | 0.4917 | 1.719 × 1.078 × 1.902 |
| LightRailgunBatteryMk3 | OneHull | derived magnetic proxy | 0.0919 | 0.0452 | 0.4917 | 1.719 × 1.078 × 1.902 |
| RailgunBatteryMk1 | TwoHullHoriz | derived magnetic proxy | 0.0790 | 0.0583 | 0.7376 | 2.578 × 1.633 × 3.282 |
| RailgunBatteryMk2 | TwoHullHoriz | derived magnetic proxy | 0.0790 | 0.0583 | 0.7376 | 2.578 × 1.633 × 3.282 |
| RailgunBatteryMk3 | TwoHullHoriz | derived magnetic proxy | 0.0790 | 0.0583 | 0.7376 | 2.578 × 1.633 × 3.282 |
| HeavyRailgunBatteryMk1 | FourHull | derived magnetic proxy | 0.1650 | 0.0682 | 0.4130 | 2.413 × 0.970 × 3.670 |
| HeavyRailgunBatteryMk2 | FourHull | derived magnetic proxy | 0.1650 | 0.0682 | 0.4130 | 2.413 × 0.970 × 3.670 |
| HeavyRailgunBatteryMk3 | FourHull | derived magnetic proxy | 0.1650 | 0.0682 | 0.4130 | 2.413 × 0.970 × 3.670 |
| LightRailCannonMk1 | OneNose | closed rail face: bore unresolved | — | 0.0628 | 1.0000 | 2.236 × 1.383 × 3.833 |
| LightRailCannonMk2 | OneNose | closed rail face: bore unresolved | — | 0.0628 | 1.0000 | 2.236 × 1.383 × 3.833 |
| LightRailCannonMk3 | OneNose | closed rail face: bore unresolved | — | 0.0628 | 1.0000 | 2.236 × 1.383 × 3.833 |
| RailCannonMk1 | TwoNoseVert | closed rail face: bore unresolved | — | 0.0734 | 1.0000 | 2.882 × 1.943 × 6.089 |
| RailCannonMk2 | TwoNoseVert | closed rail face: bore unresolved | — | 0.0734 | 1.0000 | 2.882 × 1.943 × 6.089 |
| RailCannonMk3 | TwoNoseVert | closed rail face: bore unresolved | — | 0.0734 | 1.0000 | 2.882 × 1.943 × 6.089 |
| HeavyRailCannonMk1 | ThreeNoseAngle | derived magnetic proxy | 0.1926 | 0.0840 | 0.4364 | 0.959 × 0.632 × 2.181 |
| HeavyRailCannonMk2 | ThreeNoseAngle | derived magnetic proxy | 0.1926 | 0.0840 | 0.4364 | 0.959 × 0.632 × 2.181 |
| HeavyRailCannonMk3 | ThreeNoseAngle | derived magnetic proxy | 0.1926 | 0.0840 | 0.4364 | 0.959 × 0.632 × 2.181 |
| SpinalRailgunMk1 | FourNose | closed rail face: bore unresolved | — | 0.0925 | 1.0000 | 3.495 × 2.250 × 5.937 |
| SpinalRailgunMk2 | FourNose | closed rail face: bore unresolved | — | 0.0925 | 1.0000 | 3.495 × 2.250 × 5.937 |
| SpinalRailgunMk3 | FourNose | closed rail face: bore unresolved | — | 0.0925 | 1.0000 | 3.495 × 2.250 × 5.937 |
| LightCoilgunBatteryMk1 | OneHull | derived magnetic proxy | 0.1636 | 0.0441 | 0.2696 | 0.942 × 0.623 × 1.162 |
| LightCoilgunBatteryMk2 | OneHull | derived magnetic proxy | 0.1636 | 0.0441 | 0.2696 | 0.942 × 0.623 × 1.162 |
| LightCoilgunBatteryMk3 | OneHull | derived magnetic proxy | 0.1636 | 0.0441 | 0.2696 | 0.942 × 0.623 × 1.162 |
| CoilgunBatteryMk1 | TwoHullHoriz | derived magnetic proxy | 0.1636 | 0.0548 | 0.3352 | 1.172 × 0.763 × 1.411 |
| CoilgunBatteryMk2 | TwoHullHoriz | derived magnetic proxy | 0.1636 | 0.0548 | 0.3352 | 1.172 × 0.763 × 1.411 |
| CoilgunBatteryMk3 | TwoHullHoriz | derived magnetic proxy | 0.1636 | 0.0548 | 0.3352 | 1.172 × 0.763 × 1.411 |
| HeavyCoilgunBatteryMk1 | FourHull | derived magnetic proxy | 0.1335 | 0.0691 | 0.5174 | 3.023 × 1.215 × 4.159 |
| HeavyCoilgunBatteryMk2 | FourHull | derived magnetic proxy | 0.1335 | 0.0691 | 0.5174 | 3.023 × 1.215 × 4.159 |
| HeavyCoilgunBatteryMk3 | FourHull | derived magnetic proxy | 0.1335 | 0.0691 | 0.5174 | 3.023 × 1.215 × 4.159 |
| LightCoilCannonMk1 | OneNose | derived magnetic proxy | 0.0817 | 0.0589 | 0.7209 | 1.612 × 1.009 × 2.906 |
| LightCoilCannonMk2 | OneNose | derived magnetic proxy | 0.0817 | 0.0589 | 0.7209 | 1.612 × 1.009 × 2.906 |
| LightCoilCannonMk3 | OneNose | derived magnetic proxy | 0.0817 | 0.0589 | 0.7209 | 1.612 × 1.009 × 2.906 |
| CoilCannonMk1 | TwoNoseVert | derived magnetic proxy | 0.0750 | 0.0746 | 0.9953 | 2.795 × 1.909 × 4.540 |
| CoilCannonMk2 | TwoNoseVert | derived magnetic proxy | 0.0750 | 0.0746 | 0.9953 | 2.795 × 1.909 × 4.540 |
| CoilCannonMk3 | TwoNoseVert | derived magnetic proxy | 0.0750 | 0.0746 | 0.9953 | 2.795 × 1.909 × 4.540 |
| HeavyCoilCannonMk1 | ThreeNoseAngle | derived magnetic proxy | 0.1109 | 0.0853 | 0.7691 | 2.105 × 1.580 × 4.211 |
| HeavyCoilCannonMk2 | ThreeNoseAngle | derived magnetic proxy | 0.1109 | 0.0853 | 0.7691 | 2.105 × 1.580 × 4.211 |
| HeavyCoilCannonMk3 | ThreeNoseAngle | derived magnetic proxy | 0.1109 | 0.0853 | 0.7691 | 2.105 × 1.580 × 4.211 |
| SpinalCoilerMk1 | FourNose | derived magnetic proxy | 0.1301 | 0.0938 | 0.7207 | 2.519 × 1.635 × 4.831 |
| SpinalCoilerMk2 | FourNose | derived magnetic proxy | 0.1301 | 0.0938 | 0.7207 | 2.519 × 1.635 × 4.831 |
| SpinalCoilerMk3 | FourNose | derived magnetic proxy | 0.1301 | 0.0938 | 0.7207 | 2.519 × 1.635 × 4.831 |
| HeavySiegeCoilerMk1 | ThreeNoseAngle | derived magnetic proxy | 0.1109 | 0.1836 | 1.6557 | 4.532 × 3.401 × 9.066 |
| HeavySiegeCoilerMk2 | ThreeNoseAngle | derived magnetic proxy | 0.1109 | 0.1836 | 1.6557 | 4.532 × 3.401 × 9.066 |
| HeavySiegeCoilerMk3 | ThreeNoseAngle | derived magnetic proxy | 0.1109 | 0.1836 | 1.6557 | 4.532 × 3.401 × 9.066 |
| SpinalSiegeCoilerMk1 | FourNose | derived magnetic proxy | 0.1301 | 0.2020 | 1.5528 | 5.427 × 3.523 × 10.409 |
| SpinalSiegeCoilerMk2 | FourNose | derived magnetic proxy | 0.1301 | 0.2020 | 1.5528 | 5.427 × 3.523 × 10.409 |
| SpinalSiegeCoilerMk3 | FourNose | derived magnetic proxy | 0.1301 | 0.2020 | 1.5528 | 5.427 × 3.523 × 10.409 |
| AlienLightMagBattery | OneHull | derived magnetic proxy | 0.0609 | 0.0500 | 0.8211 | 1.353 × 0.942 × 3.381 |
| AlienMagBattery | TwoHullHoriz | derived magnetic proxy | 0.0863 | 0.0631 | 0.7303 | 2.067 × 1.198 × 3.834 |
| AlienHeavyMagBattery | FourHull | derived magnetic proxy | 0.0907 | 0.0791 | 0.8717 | 4.483 × 1.362 × 6.296 |
| AlienMiniLightMagCannon | HalfNose | derived magnetic proxy | 0.0628 | 0.0691 | 1.1007 | 1.382 × 1.247 × 3.413 |
| AlienLightMagCannon | OneNose | derived magnetic proxy | 0.0628 | 0.0691 | 1.1007 | 1.382 × 1.247 × 3.413 |
| AlienMagCannon | TwoNoseVert | derived magnetic proxy | 0.0887 | 0.0871 | 0.9819 | 1.529 × 1.407 × 4.159 |
| AlienHeavyMagCannon | ThreeNoseAngle | derived magnetic proxy | 0.0949 | 0.0997 | 1.0495 | 1.433 × 1.458 × 4.483 |
| AlienSpinalMagCannon | FourNose | derived magnetic proxy | 0.1290 | 0.1097 | 0.8504 | 2.174 × 1.881 × 5.317 |
| AdvancedAlienLightMagBattery | OneHull | derived magnetic proxy | 0.0609 | 0.0500 | 0.8211 | 1.353 × 0.942 × 3.381 |
| AdvancedAlienMagBattery | TwoHullHoriz | derived magnetic proxy | 0.0863 | 0.0631 | 0.7303 | 2.067 × 1.198 × 3.834 |
| AdvancedAlienHeavyMagBattery | FourHull | derived magnetic proxy | 0.0907 | 0.0791 | 0.8717 | 4.483 × 1.362 × 6.296 |
| AdvancedAlienLightMagCannon | OneNose | derived magnetic proxy | 0.0628 | 0.0691 | 1.1007 | 1.382 × 1.247 × 3.413 |
| AdvancedAlienMagCannon | TwoNoseVert | derived magnetic proxy | 0.0887 | 0.0871 | 0.9819 | 1.529 × 1.407 × 4.159 |
| AdvancedAlienHeavyMagCannon | ThreeNoseAngle | derived magnetic proxy | 0.0949 | 0.0997 | 1.0495 | 1.433 × 1.458 × 4.483 |
| AdvancedAlienSpinalMagCannon | FourNose | derived magnetic proxy | 0.1290 | 0.1097 | 0.8504 | 2.174 × 1.881 × 5.317 |
| Gen3AlienLightMagBattery | OneHull | derived magnetic proxy | 0.0609 | 0.0613 | 1.0066 | 1.658 × 1.155 × 4.145 |
| Gen3AlienMagBattery | TwoHullHoriz | derived magnetic proxy | 0.0863 | 0.0769 | 0.8910 | 2.522 × 1.461 × 4.678 |
| Gen3AlienHeavyMagBattery | FourHull | derived magnetic proxy | 0.0907 | 0.0969 | 1.0681 | 5.493 × 1.668 × 7.715 |
| Gen3AlienLightMagCannon | OneNose | derived magnetic proxy | 0.0628 | 0.0850 | 1.3537 | 1.699 × 1.533 × 4.197 |
| Gen3AlienMagCannon | TwoNoseVert | derived magnetic proxy | 0.0887 | 0.1067 | 1.2032 | 1.874 × 1.724 × 5.096 |
| Gen3AlienHeavyMagCannon | ThreeNoseAngle | derived magnetic proxy | 0.0949 | 0.1344 | 1.4155 | 1.933 × 1.967 × 6.046 |
| Gen3AlienSpinalMagCannon | FourNose | derived magnetic proxy | 0.1290 | 0.1693 | 1.3129 | 3.356 × 2.905 × 8.209 |
| PointDefenseLaserTurret | OneHull | native optical diameter | 0.6939 | 0.6000 | 0.8646 | 0.845 × 0.920 × 0.790 |
| 60cmIRLaserBattery | OneHull | native optical diameter | 1.0688 | 1.2000 | 1.1227 | 1.718 × 1.804 × 1.606 |
| 120cmIRLaserBattery | TwoHullHoriz | native optical diameter | 1.6388 | 2.4000 | 1.4645 | 3.410 × 3.574 × 3.187 |
| 360cmIRLaserBattery | FourHull | native optical diameter | 2.6084 | 7.2000 | 2.7603 | 10.268 × 10.404 × 9.597 |
| 240cmIRLaserCannon | OneNose | native optical diameter | 1.5912 | 4.8000 | 3.0165 | 6.271 × 6.941 × 5.861 |
| 480cmIRLaserCannon | TwoNoseVert | native optical diameter | 2.6829 | 9.6000 | 3.5782 | 13.963 × 14.800 × 13.051 |
| 720cmIRLaserCannon | ThreeNoseAngle | native optical diameter | 4.1379 | 14.4000 | 3.4800 | 20.268 × 20.988 × 18.944 |
| 960cmIRLaserCannon | FourNose | native optical diameter | 4.9843 | 19.2000 | 3.8521 | 27.582 × 28.001 × 25.779 |
| PointDefenseArcLaserTurret | OneHull | native optical diameter | 0.6939 | 0.6000 | 0.8646 | 0.845 × 0.920 × 0.790 |
| 60cmIRArcLaserBattery | OneHull | native optical diameter | 1.0688 | 1.2000 | 1.1227 | 1.718 × 1.804 × 1.606 |
| 120cmIRArcLaserBattery | TwoHullHoriz | native optical diameter | 1.6388 | 2.4000 | 1.4645 | 3.410 × 3.574 × 3.187 |
| 360cmIRArcLaserBattery | FourHull | native optical diameter | 2.6084 | 7.2000 | 2.7603 | 10.268 × 10.404 × 9.597 |
| 240cmIRArcLaserCannon | OneNose | native optical diameter | 1.5912 | 4.8000 | 3.0165 | 6.271 × 6.941 × 5.861 |
| 480cmIRArcLaserCannon | TwoNoseVert | native optical diameter | 2.6829 | 9.6000 | 3.5782 | 13.963 × 14.800 × 13.051 |
| 720cmIRArcLaserCannon | ThreeNoseAngle | native optical diameter | 4.1379 | 14.4000 | 3.4800 | 20.268 × 20.988 × 18.944 |
| 960cmIRArcLaserCannon | FourNose | native optical diameter | 4.9843 | 19.2000 | 3.8521 | 27.582 × 28.001 × 25.779 |
| PointDefensePhaserTurret | OneHull | native optical diameter | 0.6939 | 0.6000 | 0.8646 | 0.845 × 0.920 × 0.790 |
| 60cmIRPhaserBattery | OneHull | native optical diameter | 1.0688 | 1.2000 | 1.1227 | 1.718 × 1.804 × 1.606 |
| 120cmIRPhaserBattery | TwoHullHoriz | native optical diameter | 1.6388 | 2.4000 | 1.4645 | 3.410 × 3.574 × 3.187 |
| 360cmIRPhaserBattery | FourHull | native optical diameter | 2.6084 | 7.2000 | 2.7603 | 10.268 × 10.404 × 9.597 |
| 240cmIRPhaserCannon | OneNose | native optical diameter | 1.5912 | 4.8000 | 3.0165 | 6.271 × 6.941 × 5.861 |
| 480cmIRPhaserCannon | TwoNoseVert | native optical diameter | 2.6829 | 9.6000 | 3.5782 | 13.963 × 14.800 × 13.051 |
| 720cmIRPhaserCannon | ThreeNoseAngle | native optical diameter | 4.1379 | 14.4000 | 3.4800 | 20.268 × 20.988 × 18.944 |
| 960cmIRPhaserCannon | FourNose | native optical diameter | 4.9843 | 19.2000 | 3.8521 | 27.582 × 28.001 × 25.779 |
| 60cmGreenLaserBattery | OneHull | native optical diameter | 1.0688 | 1.2000 | 1.1227 | 1.718 × 1.804 × 1.606 |
| 120cmGreenLaserBattery | TwoHullHoriz | native optical diameter | 1.6388 | 2.4000 | 1.4645 | 3.410 × 3.574 × 3.187 |
| 360cmGreenLaserBattery | FourHull | native optical diameter | 2.6084 | 7.2000 | 2.7603 | 10.268 × 10.404 × 9.597 |
| 240cmGreenLaserCannon | OneNose | native optical diameter | 1.5912 | 4.8000 | 3.0165 | 6.271 × 6.941 × 5.861 |
| 480cmGreenLaserCannon | TwoNoseVert | native optical diameter | 2.6829 | 9.6000 | 3.5782 | 13.963 × 14.800 × 13.051 |
| 720cmGreenLaserCannon | ThreeNoseAngle | native optical diameter | 4.1379 | 14.4000 | 3.4800 | 20.268 × 20.988 × 18.944 |
| 960cmGreenLaserCannon | FourNose | native optical diameter | 4.9843 | 19.2000 | 3.8521 | 27.582 × 28.001 × 25.779 |
| 60cmGreenArcLaserBattery | OneHull | native optical diameter | 1.0688 | 1.2000 | 1.1227 | 1.718 × 1.804 × 1.606 |
| 120cmGreenArcLaserBattery | TwoHullHoriz | native optical diameter | 1.6388 | 2.4000 | 1.4645 | 3.410 × 3.574 × 3.187 |
| 360cmGreenArcLaserBattery | FourHull | native optical diameter | 2.6084 | 7.2000 | 2.7603 | 10.268 × 10.404 × 9.597 |
| 240cmGreenArcLaserCannon | OneNose | native optical diameter | 1.5912 | 4.8000 | 3.0165 | 6.271 × 6.941 × 5.861 |
| 480cmGreenArcLaserCannon | TwoNoseVert | native optical diameter | 2.6829 | 9.6000 | 3.5782 | 13.963 × 14.800 × 13.051 |
| 720cmGreenArcLaserCannon | ThreeNoseAngle | native optical diameter | 4.1379 | 14.4000 | 3.4800 | 20.268 × 20.988 × 18.944 |
| 960cmGreenArcLaserCannon | FourNose | native optical diameter | 4.9843 | 19.2000 | 3.8521 | 27.582 × 28.001 × 25.779 |
| 60cmGreenPhaserBattery | OneHull | native optical diameter | 1.0688 | 1.2000 | 1.1227 | 1.718 × 1.804 × 1.606 |
| 120cmGreenPhaserBattery | TwoHullHoriz | native optical diameter | 1.6388 | 2.4000 | 1.4645 | 3.410 × 3.574 × 3.187 |
| 360cmGreenPhaserBattery | FourHull | native optical diameter | 2.6084 | 7.2000 | 2.7603 | 10.268 × 10.404 × 9.597 |
| 240cmGreenPhaserCannon | OneNose | native optical diameter | 1.5912 | 4.8000 | 3.0165 | 6.271 × 6.941 × 5.861 |
| 480cmGreenPhaserCannon | TwoNoseVert | native optical diameter | 2.6829 | 9.6000 | 3.5782 | 13.963 × 14.800 × 13.051 |
| 720cmGreenPhaserCannon | ThreeNoseAngle | native optical diameter | 4.1379 | 14.4000 | 3.4800 | 20.268 × 20.988 × 18.944 |
| 960cmGreenPhaserCannon | FourNose | native optical diameter | 4.9843 | 19.2000 | 3.8521 | 27.582 × 28.001 × 25.779 |
| 60cmUVLaserBattery | OneHull | native optical diameter | 1.0688 | 1.2000 | 1.1227 | 1.718 × 1.804 × 1.606 |
| 120cmUVLaserBattery | TwoHullHoriz | native optical diameter | 1.6388 | 2.4000 | 1.4645 | 3.410 × 3.574 × 3.187 |
| 360cmUVLaserBattery | FourHull | native optical diameter | 2.6084 | 7.2000 | 2.7603 | 10.268 × 10.404 × 9.597 |
| 240cmUVLaserCannon | OneNose | native optical diameter | 1.5912 | 4.8000 | 3.0165 | 6.271 × 6.941 × 5.861 |
| 480cmUVLaserCannon | TwoNoseVert | native optical diameter | 2.6829 | 9.6000 | 3.5782 | 13.963 × 14.800 × 13.051 |
| 720cmUVLaserCannon | ThreeNoseAngle | native optical diameter | 4.1379 | 14.4000 | 3.4800 | 20.268 × 20.988 × 18.944 |
| 960cmUVLaserCannon | FourNose | native optical diameter | 4.9843 | 19.2000 | 3.8521 | 27.582 × 28.001 × 25.779 |
| 60cmUVArcLaserBattery | OneHull | native optical diameter | 1.0688 | 1.2000 | 1.1227 | 1.718 × 1.804 × 1.606 |
| 120cmUVArcLaserBattery | TwoHullHoriz | native optical diameter | 1.6388 | 2.4000 | 1.4645 | 3.410 × 3.574 × 3.187 |
| 360cmUVArcLaserBattery | FourHull | native optical diameter | 2.6084 | 7.2000 | 2.7603 | 10.268 × 10.404 × 9.597 |
| 240cmUVArcLaserCannon | OneNose | native optical diameter | 1.5912 | 4.8000 | 3.0165 | 6.271 × 6.941 × 5.861 |
| 480cmUVArcLaserCannon | TwoNoseVert | native optical diameter | 2.6829 | 9.6000 | 3.5782 | 13.963 × 14.800 × 13.051 |
| 720cmUVArcLaserCannon | ThreeNoseAngle | native optical diameter | 4.1379 | 14.4000 | 3.4800 | 20.268 × 20.988 × 18.944 |
| 960cmUVArcLaserCannon | FourNose | native optical diameter | 4.9843 | 19.2000 | 3.8521 | 27.582 × 28.001 × 25.779 |
| 60cmUVPhaserBattery | OneHull | native optical diameter | 1.0688 | 1.2000 | 1.1227 | 1.718 × 1.804 × 1.606 |
| 120cmUVPhaserBattery | TwoHullHoriz | native optical diameter | 1.6388 | 2.4000 | 1.4645 | 3.410 × 3.574 × 3.187 |
| 360cmUVPhaserBattery | FourHull | native optical diameter | 2.6084 | 7.2000 | 2.7603 | 10.268 × 10.404 × 9.597 |
| 240cmUVPhaserCannon | OneNose | native optical diameter | 1.5912 | 4.8000 | 3.0165 | 6.271 × 6.941 × 5.861 |
| 480cmUVPhaserCannon | TwoNoseVert | native optical diameter | 2.6829 | 9.6000 | 3.5782 | 13.963 × 14.800 × 13.051 |
| 720cmUVPhaserCannon | ThreeNoseAngle | native optical diameter | 4.1379 | 14.4000 | 3.4800 | 20.268 × 20.988 × 18.944 |
| 960cmUVPhaserCannon | FourNose | native optical diameter | 4.9843 | 19.2000 | 3.8521 | 27.582 × 28.001 × 25.779 |
| AlienPointDefenseLaserTurret | OneHull | native optical diameter | 0.7223 | 0.6400 | 0.8861 | 2.129 × 2.180 × 1.939 |
| Alien64cmOrangeLaserBattery | OneHull | native optical diameter | 0.5280 | 1.2800 | 2.4244 | 3.994 × 2.657 × 5.578 |
| Alien128cmOrangeLaserBattery | TwoHullHoriz | native optical diameter | 0.5365 | 2.5600 | 4.7720 | 11.227 × 5.966 × 13.187 |
| Alien384cmOrangeLaserBattery | FourHull | native optical diameter | 0.9020 | 7.6800 | 8.5144 | 46.224 × 14.041 × 62.985 |
| Alien256cmOrangeLaserCannon | OneNose | native optical diameter | 0.6917 | 5.1200 | 7.4020 | 11.754 × 11.505 × 13.452 |
| Alien512cmOrangeLaserCannon | TwoNoseVert | native optical diameter | 1.0330 | 10.2400 | 9.9128 | 19.577 × 19.160 × 26.271 |
| Alien768cmOrangeLaserCannon | ThreeNoseAngle | native optical diameter | 1.0330 | 15.3600 | 14.8693 | 31.388 × 30.049 × 59.476 |
| Alien1024cmOrangeLaserCannon | FourNose | native optical diameter | 1.3843 | 20.4800 | 14.7950 | 37.817 × 37.525 × 65.324 |
| Alien64cmVioletLaserBattery | OneHull | native optical diameter | 0.5280 | 1.2800 | 2.4244 | 3.994 × 2.657 × 5.578 |
| Alien128cmVioletLaserBattery | TwoHullHoriz | native optical diameter | 0.5365 | 2.5600 | 4.7720 | 11.227 × 5.966 × 13.187 |
| Alien384cmVioletLaserBattery | FourHull | native optical diameter | 0.9020 | 7.6800 | 8.5144 | 46.224 × 14.041 × 62.985 |
| Alien256cmVioletLaserCannon | OneNose | native optical diameter | 0.6917 | 5.1200 | 7.4020 | 11.754 × 11.505 × 13.452 |
| Alien512cmVioletLaserCannon | TwoNoseVert | native optical diameter | 1.0330 | 10.2400 | 9.9128 | 19.577 × 19.160 × 26.271 |
| Alien768cmVioletLaserCannon | ThreeNoseAngle | native optical diameter | 1.0330 | 15.3600 | 14.8693 | 31.388 × 30.049 × 59.476 |
| Alien1024cmVioletLaserCannon | FourNose | native optical diameter | 1.3843 | 20.4800 | 14.7950 | 37.817 × 37.525 × 65.324 |
| Alien384cmXaserBattery | FourHull | native optical diameter | 0.9020 | 7.6800 | 8.5144 | 46.224 × 14.041 × 62.985 |
| Alien256cmXaserCannon | OneNose | native optical diameter | 0.6917 | 5.1200 | 7.4020 | 11.754 × 11.505 × 13.452 |
| Alien512cmXaserCannon | TwoNoseVert | native optical diameter | 1.0330 | 10.2400 | 9.9128 | 19.577 × 19.160 × 26.271 |
| Alien768cmXaserCannon | ThreeNoseAngle | native optical diameter | 1.0330 | 15.3600 | 14.8693 | 31.388 × 30.049 × 59.476 |
| Alien1024cmXaserCannon | FourNose | native optical diameter | 1.3843 | 20.4800 | 14.7950 | 37.817 × 37.525 × 65.324 |
| Alien768cmGraserCannon | ThreeNoseAngle | native optical diameter | 1.0330 | 15.3600 | 14.8693 | 31.388 × 30.049 × 59.476 |
| Alien1024cmGraserCannon | FourNose | native optical diameter | 1.3843 | 20.4800 | 14.7950 | 37.817 × 37.525 × 65.324 |
