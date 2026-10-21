# Does canonical geometry tighten gameplay size tiers?

Each distinct `(resource, scale, size)` receives one vote: identical laser wavelengths and weapon marks do not inflate counts, while genuinely different caliber/Gen3 scales remain separate. Main metric is horizontal full-pitch/yaw diameter, not the 3D sphere. Static launchers retain their original conservative comparison bounds. Median and IQR describe the bulk; max/min and CV expose outliers. No universal diameter ∝ slot-count law is asserted.

## Within-tier spread

| Scenario | Faction | Size | Archetypes | Min m | Q25 m | Median m | Q75 m | Max m | Max/min | CV |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| baseline | Human | 1 | 10 | 1.056 | 4.014 | 5.615 | 7.179 | 9.673 | 9.159 | 0.475 |
| baseline | Human | 2 | 7 | 2.386 | 5.538 | 6.327 | 6.617 | 9.024 | 3.783 | 0.309 |
| baseline | Human | 4 | 6 | 3.811 | 7.074 | 9.352 | 11.266 | 12.749 | 3.346 | 0.343 |
| baseline | Alien | 1 | 6 | 3.002 | 3.555 | 4.363 | 6.920 | 8.194 | 2.730 | 0.391 |
| baseline | Alien | 2 | 3 | 5.027 | 7.318 | 9.608 | 9.819 | 10.030 | 1.995 | 0.276 |
| baseline | Alien | 4 | 3 | 9.943 | 10.216 | 10.488 | 11.140 | 11.793 | 1.186 | 0.072 |
| canonical | Human | 1 | 11 | 0.913 | 1.880 | 2.578 | 5.672 | 7.330 | 8.027 | 0.608 |
| canonical | Human | 2 | 7 | 2.032 | 4.080 | 5.013 | 6.617 | 9.574 | 4.711 | 0.419 |
| canonical | Human | 4 | 6 | 4.715 | 5.901 | 7.343 | 9.859 | 12.749 | 2.704 | 0.347 |
| canonical | Alien | 1 | 7 | 3.002 | 4.148 | 6.220 | 7.909 | 8.449 | 2.815 | 0.351 |
| canonical | Alien | 2 | 4 | 7.017 | 8.175 | 9.295 | 13.520 | 23.989 | 3.418 | 0.546 |
| canonical | Alien | 4 | 4 | 8.667 | 10.132 | 11.207 | 31.169 | 89.298 | 10.303 | 1.136 |
| compact_optics | Human | 1 | 11 | 0.913 | 1.880 | 2.578 | 5.672 | 7.330 | 8.027 | 0.608 |
| compact_optics | Human | 2 | 7 | 2.032 | 4.080 | 5.013 | 6.617 | 9.574 | 4.711 | 0.419 |
| compact_optics | Human | 4 | 6 | 4.715 | 5.901 | 7.343 | 9.859 | 12.749 | 2.704 | 0.347 |
| compact_optics | Alien | 1 | 7 | 1.333 | 2.269 | 4.958 | 6.922 | 8.194 | 6.148 | 0.554 |
| compact_optics | Alien | 2 | 4 | 3.072 | 6.031 | 7.789 | 8.928 | 10.030 | 3.265 | 0.362 |
| compact_optics | Alien | 4 | 4 | 8.667 | 9.079 | 9.918 | 10.914 | 11.793 | 1.361 | 0.121 |
| named_diameter | Human | 1 | 11 | 0.913 | 1.783 | 2.578 | 5.672 | 7.330 | 8.027 | 0.645 |
| named_diameter | Human | 2 | 7 | 1.747 | 3.350 | 5.013 | 6.617 | 9.574 | 5.481 | 0.492 |
| named_diameter | Human | 4 | 6 | 4.715 | 5.344 | 6.202 | 7.613 | 12.749 | 2.704 | 0.377 |
| named_diameter | Alien | 1 | 7 | 3.002 | 3.781 | 4.958 | 6.922 | 8.194 | 2.730 | 0.352 |
| named_diameter | Alien | 2 | 4 | 7.017 | 8.175 | 9.295 | 10.521 | 11.994 | 1.709 | 0.195 |
| named_diameter | Alien | 4 | 4 | 8.667 | 10.132 | 11.207 | 20.007 | 44.649 | 5.151 | 0.786 |
| mag_relative | Human | 1 | 11 | 0.913 | 2.255 | 4.618 | 6.391 | 7.330 | 8.027 | 0.505 |
| mag_relative | Human | 2 | 7 | 3.494 | 5.696 | 6.857 | 8.515 | 9.574 | 2.741 | 0.298 |
| mag_relative | Human | 4 | 6 | 6.805 | 8.308 | 10.053 | 12.191 | 20.771 | 3.052 | 0.404 |
| mag_relative | Alien | 1 | 7 | 3.002 | 4.148 | 7.574 | 8.321 | 9.285 | 3.093 | 0.374 |
| mag_relative | Alien | 2 | 4 | 8.546 | 9.659 | 10.228 | 13.816 | 23.989 | 2.807 | 0.471 |
| mag_relative | Alien | 4 | 4 | 10.555 | 11.484 | 12.364 | 32.025 | 89.298 | 8.460 | 1.078 |
| retier_rail | Human | 1 | 11 | 0.913 | 1.880 | 2.578 | 5.672 | 7.330 | 8.027 | 0.608 |
| retier_rail | Human | 2 | 8 | 2.032 | 4.374 | 4.864 | 6.498 | 9.574 | 4.711 | 0.400 |
| retier_rail | Human | 4 | 5 | 5.599 | 6.805 | 7.882 | 10.518 | 12.749 | 2.277 | 0.297 |
| retier_rail | Alien | 1 | 7 | 1.333 | 2.269 | 4.958 | 6.922 | 8.194 | 6.148 | 0.554 |
| retier_rail | Alien | 2 | 4 | 3.072 | 6.031 | 7.789 | 8.928 | 10.030 | 3.265 | 0.362 |
| retier_rail | Alien | 4 | 4 | 8.667 | 9.079 | 9.918 | 10.914 | 11.793 | 1.361 | 0.121 |
| retier_coil | Human | 1 | 11 | 0.913 | 1.880 | 2.578 | 5.672 | 7.330 | 8.027 | 0.608 |
| retier_coil | Human | 2 | 8 | 2.032 | 4.374 | 5.306 | 6.498 | 9.574 | 4.711 | 0.390 |
| retier_coil | Human | 4 | 5 | 4.715 | 6.805 | 7.882 | 10.518 | 12.749 | 2.704 | 0.330 |
| retier_coil | Alien | 1 | 7 | 1.333 | 2.269 | 4.958 | 6.922 | 8.194 | 6.148 | 0.554 |
| retier_coil | Alien | 2 | 4 | 3.072 | 6.031 | 7.789 | 8.928 | 10.030 | 3.265 | 0.362 |
| retier_coil | Alien | 4 | 4 | 8.667 | 9.079 | 9.918 | 10.914 | 11.793 | 1.361 | 0.121 |
| retier_naval | Human | 1 | 10 | 0.913 | 1.853 | 2.561 | 4.417 | 7.330 | 8.027 | 0.615 |
| retier_naval | Human | 2 | 8 | 2.032 | 4.374 | 5.696 | 6.952 | 9.574 | 4.711 | 0.390 |
| retier_naval | Human | 4 | 6 | 4.715 | 5.901 | 7.343 | 9.859 | 12.749 | 2.704 | 0.347 |
| retier_naval | Alien | 1 | 7 | 1.333 | 2.269 | 4.958 | 6.922 | 8.194 | 6.148 | 0.554 |
| retier_naval | Alien | 2 | 4 | 3.072 | 6.031 | 7.789 | 8.928 | 10.030 | 3.265 | 0.362 |
| retier_naval | Alien | 4 | 4 | 8.667 | 9.079 | 9.918 | 10.914 | 11.793 | 1.361 | 0.121 |
| retier_pair | Human | 1 | 10 | 0.913 | 1.853 | 2.561 | 4.417 | 7.330 | 8.027 | 0.615 |
| retier_pair | Human | 2 | 9 | 2.032 | 4.667 | 5.013 | 6.857 | 9.574 | 4.711 | 0.379 |
| retier_pair | Human | 4 | 5 | 5.599 | 6.805 | 7.882 | 10.518 | 12.749 | 2.277 | 0.297 |
| retier_pair | Alien | 1 | 7 | 1.333 | 2.269 | 4.958 | 6.922 | 8.194 | 6.148 | 0.554 |
| retier_pair | Alien | 2 | 4 | 3.072 | 6.031 | 7.789 | 8.928 | 10.030 | 3.265 | 0.362 |
| retier_pair | Alien | 4 | 4 | 8.667 | 9.079 | 9.918 | 10.914 | 11.793 | 1.361 | 0.121 |



## Cross-tier inversions

An inversion means a lower-slot-cost art has a larger horizontal bound than a higher-slot-cost art; lower fractions indicate a more visually ordered ladder. They are descriptive comparisons, not balance scores.

| Scenario | Faction | Size pair | Inversions | Pairs | Fraction |
| --- | --- | --- | --- | --- | --- |
| baseline | Human | 1 / 2 | 30 | 70 | 42.9% |
| baseline | Human | 2 / 4 | 9 | 42 | 21.4% |
| baseline | Human | 1 / 4 | 12 | 60 | 20.0% |
| baseline | Alien | 1 / 2 | 2 | 18 | 11.1% |
| baseline | Alien | 2 / 4 | 1 | 9 | 11.1% |
| baseline | Alien | 1 / 4 | 0 | 18 | 0.0% |
| canonical | Human | 1 / 2 | 23 | 77 | 29.9% |
| canonical | Human | 2 / 4 | 10 | 42 | 23.8% |
| canonical | Human | 1 / 4 | 8 | 66 | 12.1% |
| canonical | Alien | 1 / 2 | 3 | 28 | 10.7% |
| canonical | Alien | 2 / 4 | 4 | 16 | 25.0% |
| canonical | Alien | 1 / 4 | 0 | 28 | 0.0% |
| compact_optics | Human | 1 / 2 | 23 | 77 | 29.9% |
| compact_optics | Human | 2 / 4 | 10 | 42 | 23.8% |
| compact_optics | Human | 1 / 4 | 8 | 66 | 12.1% |
| compact_optics | Alien | 1 / 2 | 6 | 28 | 21.4% |
| compact_optics | Alien | 2 / 4 | 2 | 16 | 12.5% |
| compact_optics | Alien | 1 / 4 | 0 | 28 | 0.0% |
| named_diameter | Human | 1 / 2 | 26 | 77 | 33.8% |
| named_diameter | Human | 2 / 4 | 13 | 42 | 31.0% |
| named_diameter | Human | 1 / 4 | 11 | 66 | 16.7% |
| named_diameter | Alien | 1 / 2 | 2 | 28 | 7.1% |
| named_diameter | Alien | 2 / 4 | 4 | 16 | 25.0% |
| named_diameter | Alien | 1 / 4 | 0 | 28 | 0.0% |
| mag_relative | Human | 1 / 2 | 17 | 77 | 22.1% |
| mag_relative | Human | 2 / 4 | 6 | 42 | 14.3% |
| mag_relative | Human | 1 / 4 | 2 | 66 | 3.0% |
| mag_relative | Alien | 1 / 2 | 1 | 28 | 3.6% |
| mag_relative | Alien | 2 / 4 | 3 | 16 | 18.8% |
| mag_relative | Alien | 1 / 4 | 0 | 28 | 0.0% |
| retier_rail | Human | 1 / 2 | 26 | 88 | 29.5% |
| retier_rail | Human | 2 / 4 | 6 | 40 | 15.0% |
| retier_rail | Human | 1 / 4 | 5 | 55 | 9.1% |
| retier_rail | Alien | 1 / 2 | 6 | 28 | 21.4% |
| retier_rail | Alien | 2 / 4 | 2 | 16 | 12.5% |
| retier_rail | Alien | 1 / 4 | 0 | 28 | 0.0% |
| retier_coil | Human | 1 / 2 | 26 | 88 | 29.5% |
| retier_coil | Human | 2 / 4 | 8 | 40 | 20.0% |
| retier_coil | Human | 1 / 4 | 5 | 55 | 9.1% |
| retier_coil | Alien | 1 / 2 | 6 | 28 | 21.4% |
| retier_coil | Alien | 2 / 4 | 2 | 16 | 12.5% |
| retier_coil | Alien | 1 / 4 | 0 | 28 | 0.0% |
| retier_naval | Human | 1 / 2 | 18 | 80 | 22.5% |
| retier_naval | Human | 2 / 4 | 13 | 48 | 27.1% |
| retier_naval | Human | 1 / 4 | 5 | 60 | 8.3% |
| retier_naval | Alien | 1 / 2 | 6 | 28 | 21.4% |
| retier_naval | Alien | 2 / 4 | 2 | 16 | 12.5% |
| retier_naval | Alien | 1 / 4 | 0 | 28 | 0.0% |
| retier_pair | Human | 1 / 2 | 20 | 90 | 22.2% |
| retier_pair | Human | 2 / 4 | 8 | 45 | 17.8% |
| retier_pair | Human | 1 / 4 | 3 | 50 | 6.0% |
| retier_pair | Alien | 1 / 2 | 6 | 28 | 21.4% |
| retier_pair | Alien | 2 / 4 | 2 | 16 | 12.5% |
| retier_pair | Alien | 1 / 4 | 0 | 28 | 0.0% |

## Paired-cohort comparison

For direct before/after inference, hold the set of canonical `(resource, factor, original size)` representatives fixed across scenarios. Thus the original shared 30/40 mm asset is counted once for each of its two future scales in the baseline as well. These paired rows avoid attributing a change in cohort composition to better art.

| Scenario | Faction | Size | N | Min m | Median m | Max m | CV |
| --- | --- | --- | --- | --- | --- | --- | --- |
| baseline | Human | 1 | 11 | 1.056 | 6.056 | 9.673 | 0.474 |
| baseline | Human | 2 | 7 | 2.386 | 6.327 | 9.024 | 0.309 |
| baseline | Human | 4 | 6 | 3.811 | 9.352 | 12.749 | 0.343 |
| canonical | Human | 1 | 11 | 0.913 | 2.578 | 7.330 | 0.608 |
| canonical | Human | 2 | 7 | 2.032 | 5.013 | 9.574 | 0.419 |
| canonical | Human | 4 | 6 | 4.715 | 7.343 | 12.749 | 0.347 |
| compact_optics | Human | 1 | 11 | 0.913 | 2.578 | 7.330 | 0.608 |
| compact_optics | Human | 2 | 7 | 2.032 | 5.013 | 9.574 | 0.419 |
| compact_optics | Human | 4 | 6 | 4.715 | 7.343 | 12.749 | 0.347 |
| named_diameter | Human | 1 | 11 | 0.913 | 2.578 | 7.330 | 0.645 |
| named_diameter | Human | 2 | 7 | 1.747 | 5.013 | 9.574 | 0.492 |
| named_diameter | Human | 4 | 6 | 4.715 | 6.202 | 12.749 | 0.377 |
| mag_relative | Human | 1 | 11 | 0.913 | 4.618 | 7.330 | 0.505 |
| mag_relative | Human | 2 | 7 | 3.494 | 6.857 | 9.574 | 0.298 |
| mag_relative | Human | 4 | 6 | 6.805 | 10.053 | 20.771 | 0.404 |
| retier_rail | Human | 1 | 11 | 0.913 | 2.578 | 7.330 | 0.608 |
| retier_rail | Human | 2 | 8 | 2.032 | 4.864 | 9.574 | 0.400 |
| retier_rail | Human | 4 | 5 | 5.599 | 7.882 | 12.749 | 0.297 |
| retier_coil | Human | 1 | 11 | 0.913 | 2.578 | 7.330 | 0.608 |
| retier_coil | Human | 2 | 8 | 2.032 | 5.306 | 9.574 | 0.390 |
| retier_coil | Human | 4 | 5 | 4.715 | 7.882 | 12.749 | 0.330 |
| retier_naval | Human | 1 | 10 | 0.913 | 2.561 | 7.330 | 0.615 |
| retier_naval | Human | 2 | 8 | 2.032 | 5.696 | 9.574 | 0.390 |
| retier_naval | Human | 4 | 6 | 4.715 | 7.343 | 12.749 | 0.347 |
| retier_pair | Human | 1 | 10 | 0.913 | 2.561 | 7.330 | 0.615 |
| retier_pair | Human | 2 | 9 | 2.032 | 5.013 | 9.574 | 0.379 |
| retier_pair | Human | 4 | 5 | 5.599 | 7.882 | 12.749 | 0.297 |
| baseline | Alien | 1 | 7 | 3.002 | 4.958 | 8.194 | 0.372 |
| baseline | Alien | 2 | 4 | 5.027 | 9.608 | 10.030 | 0.239 |
| baseline | Alien | 4 | 4 | 9.943 | 10.216 | 11.793 | 0.072 |
| canonical | Alien | 1 | 7 | 3.002 | 6.220 | 8.449 | 0.351 |
| canonical | Alien | 2 | 4 | 7.017 | 9.295 | 23.989 | 0.546 |
| canonical | Alien | 4 | 4 | 8.667 | 11.207 | 89.298 | 1.136 |
| compact_optics | Alien | 1 | 7 | 1.333 | 4.958 | 8.194 | 0.554 |
| compact_optics | Alien | 2 | 4 | 3.072 | 7.789 | 10.030 | 0.362 |
| compact_optics | Alien | 4 | 4 | 8.667 | 9.918 | 11.793 | 0.121 |
| named_diameter | Alien | 1 | 7 | 3.002 | 4.958 | 8.194 | 0.352 |
| named_diameter | Alien | 2 | 4 | 7.017 | 9.295 | 11.994 | 0.195 |
| named_diameter | Alien | 4 | 4 | 8.667 | 11.207 | 44.649 | 0.786 |
| mag_relative | Alien | 1 | 7 | 3.002 | 7.574 | 9.285 | 0.374 |
| mag_relative | Alien | 2 | 4 | 8.546 | 10.228 | 23.989 | 0.471 |
| mag_relative | Alien | 4 | 4 | 10.555 | 12.364 | 89.298 | 1.078 |
| retier_rail | Alien | 1 | 7 | 1.333 | 4.958 | 8.194 | 0.554 |
| retier_rail | Alien | 2 | 4 | 3.072 | 7.789 | 10.030 | 0.362 |
| retier_rail | Alien | 4 | 4 | 8.667 | 9.918 | 11.793 | 0.121 |
| retier_coil | Alien | 1 | 7 | 1.333 | 4.958 | 8.194 | 0.554 |
| retier_coil | Alien | 2 | 4 | 3.072 | 7.789 | 10.030 | 0.362 |
| retier_coil | Alien | 4 | 4 | 8.667 | 9.918 | 11.793 | 0.121 |
| retier_naval | Alien | 1 | 7 | 1.333 | 4.958 | 8.194 | 0.554 |
| retier_naval | Alien | 2 | 4 | 3.072 | 7.789 | 10.030 | 0.362 |
| retier_naval | Alien | 4 | 4 | 8.667 | 9.918 | 11.793 | 0.121 |
| retier_pair | Alien | 1 | 7 | 1.333 | 4.958 | 8.194 | 0.554 |
| retier_pair | Alien | 2 | 4 | 3.072 | 7.789 | 10.030 | 0.362 |
| retier_pair | Alien | 4 | 4 | 8.667 | 9.918 | 11.793 | 0.121 |

| Scenario | Faction | Size pair | Inversions | Pairs | Fraction |
| --- | --- | --- | --- | --- | --- |
| baseline | Human | 1 / 2 | 37 | 77 | 48.1% |
| baseline | Human | 2 / 4 | 9 | 42 | 21.4% |
| baseline | Human | 1 / 4 | 15 | 66 | 22.7% |
| canonical | Human | 1 / 2 | 23 | 77 | 29.9% |
| canonical | Human | 2 / 4 | 10 | 42 | 23.8% |
| canonical | Human | 1 / 4 | 8 | 66 | 12.1% |
| compact_optics | Human | 1 / 2 | 23 | 77 | 29.9% |
| compact_optics | Human | 2 / 4 | 10 | 42 | 23.8% |
| compact_optics | Human | 1 / 4 | 8 | 66 | 12.1% |
| named_diameter | Human | 1 / 2 | 26 | 77 | 33.8% |
| named_diameter | Human | 2 / 4 | 13 | 42 | 31.0% |
| named_diameter | Human | 1 / 4 | 11 | 66 | 16.7% |
| mag_relative | Human | 1 / 2 | 17 | 77 | 22.1% |
| mag_relative | Human | 2 / 4 | 6 | 42 | 14.3% |
| mag_relative | Human | 1 / 4 | 2 | 66 | 3.0% |
| retier_rail | Human | 1 / 2 | 26 | 88 | 29.5% |
| retier_rail | Human | 2 / 4 | 6 | 40 | 15.0% |
| retier_rail | Human | 1 / 4 | 5 | 55 | 9.1% |
| retier_coil | Human | 1 / 2 | 26 | 88 | 29.5% |
| retier_coil | Human | 2 / 4 | 8 | 40 | 20.0% |
| retier_coil | Human | 1 / 4 | 5 | 55 | 9.1% |
| retier_naval | Human | 1 / 2 | 18 | 80 | 22.5% |
| retier_naval | Human | 2 / 4 | 13 | 48 | 27.1% |
| retier_naval | Human | 1 / 4 | 5 | 60 | 8.3% |
| retier_pair | Human | 1 / 2 | 20 | 90 | 22.2% |
| retier_pair | Human | 2 / 4 | 8 | 45 | 17.8% |
| retier_pair | Human | 1 / 4 | 3 | 50 | 6.0% |
| baseline | Alien | 1 / 2 | 3 | 28 | 10.7% |
| baseline | Alien | 2 / 4 | 2 | 16 | 12.5% |
| baseline | Alien | 1 / 4 | 0 | 28 | 0.0% |
| canonical | Alien | 1 / 2 | 3 | 28 | 10.7% |
| canonical | Alien | 2 / 4 | 4 | 16 | 25.0% |
| canonical | Alien | 1 / 4 | 0 | 28 | 0.0% |
| compact_optics | Alien | 1 / 2 | 6 | 28 | 21.4% |
| compact_optics | Alien | 2 / 4 | 2 | 16 | 12.5% |
| compact_optics | Alien | 1 / 4 | 0 | 28 | 0.0% |
| named_diameter | Alien | 1 / 2 | 2 | 28 | 7.1% |
| named_diameter | Alien | 2 / 4 | 4 | 16 | 25.0% |
| named_diameter | Alien | 1 / 4 | 0 | 28 | 0.0% |
| mag_relative | Alien | 1 / 2 | 1 | 28 | 3.6% |
| mag_relative | Alien | 2 / 4 | 3 | 16 | 18.8% |
| mag_relative | Alien | 1 / 4 | 0 | 28 | 0.0% |
| retier_rail | Alien | 1 / 2 | 6 | 28 | 21.4% |
| retier_rail | Alien | 2 / 4 | 2 | 16 | 12.5% |
| retier_rail | Alien | 1 / 4 | 0 | 28 | 0.0% |
| retier_coil | Alien | 1 / 2 | 6 | 28 | 21.4% |
| retier_coil | Alien | 2 / 4 | 2 | 16 | 12.5% |
| retier_coil | Alien | 1 / 4 | 0 | 28 | 0.0% |
| retier_naval | Alien | 1 / 2 | 6 | 28 | 21.4% |
| retier_naval | Alien | 2 / 4 | 2 | 16 | 12.5% |
| retier_naval | Alien | 1 / 4 | 0 | 28 | 0.0% |
| retier_pair | Alien | 1 / 2 | 6 | 28 | 21.4% |
| retier_pair | Alien | 2 / 4 | 2 | 16 | 12.5% |
| retier_pair | Alien | 1 / 4 | 0 | 28 | 0.0% |

