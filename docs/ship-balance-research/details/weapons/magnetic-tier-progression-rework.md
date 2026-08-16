# Magnetic weapon tier-progression rework

Status: deployed; manual in-game testing pending  
Last reviewed: 2026-08-16  
Game data: installed Terra Invicta templates plus the mod overrides active before this pass

## Executive conclusion

The pre-pass coil progression did not match its research position. Coil I generally unlocks after Rail II and Coil II after Rail III, yet every regular Coil I/II mount had 50 km less range than its rail peer. Light batteries and light cannons also failed the intended sustained-damage handoff.

The final implementation uses five linked rules:

- the approved muzzle velocities remain locked at the 1.25 scale, rounded to 0.1 km/s;
- the approved targeting ranges remain locked at `floor(original × 1.25 / 50) × 50`;
- inter-salvo reload is unchanged;
- human light-coil batteries and cannons receive a 60% intra-salvo reduction, rounded upward: `ceil(original × 0.40)`;
- all other human coils and all alien magnetic weapons receive a 40% intra-salvo reduction, rounded upward: `ceil(original × 0.60)`.

Every intra-salvo interval must be no longer than its own inter-salvo reload; Coil I/II must also be no longer than the corresponding Rail II/III inter-salvo reload, including heavy and spinal siege coils. Whole-second rounding yields realized reductions of 58.3–60% for the light-coil exception and 25–40% for the global cohort; the lower alien percentages occur only where small original intervals round upward.

The only damage-input correction is Light Coilgun Battery Mk3 damaging mass, reduced from 11 to 10 kg. This preserves the locked velocity and range values while ensuring each regular Coilgun Battery mark delivers more than twice the modeled sustained damage of its light-battery counterpart.

The complete machine-readable comparison is in [`magnetic-tier-progression-rework.csv`](../../tables/magnetic-tier-progression-rework.csv).

## Method and definitions

“Original” means the installed template merged with the mod overrides active immediately before this rework. It does not mean unmodified vanilla.

`damage per shot (MJ) = 0.5 × damaging mass (kg) × velocity (km/s)²`

`cycle (s) = inter-salvo reload + (salvo shots - 1) × intra-salvo interval`

`sustained damage (MW) = damage per shot × salvo shots / cycle`

This model treats `cooldown_s` as the delay after a completed salvo. If runtime overlaps that reload with salvo execution, absolute output will differ, but same-template comparisons remain directionally useful.

## Rail handoff results

Range parentheses show the margin over the rail peer. DPS parentheses show the sustained-damage margin. The intra-salvo inequality is validated separately against the peer's effective inter-salvo reload.

| Coil | Rail peer | Original range vs peer | Proposed range vs peer | Original DPS vs peer | Proposed DPS vs peer |
|---|---|---:|---:|---:|---:|
| Light Coilgun Battery Mk1 | `LightRailgunBatteryMk2` | 500 km (-50) | 600 km (+50) | 6.33 vs 10.16 MW (-37.7%) | 13.07 vs 10.16 MW (+28.6%) |
| Light Coilgun Battery Mk2 | `LightRailgunBatteryMk3` | 550 km (-50) | 650 km (+50) | 12.15 vs 19.84 MW (-38.8%) | 30.83 vs 19.84 MW (+55.3%) |
| Coilgun Battery Mk1 | `RailgunBatteryMk2` | 650 km (-50) | 800 km (+100) | 17.31 vs 17.28 MW (+0.2%) | 32.95 vs 17.28 MW (+90.7%) |
| Coilgun Battery Mk2 | `RailgunBatteryMk3` | 700 km (-50) | 850 km (+100) | 33.08 vs 40.82 MW (-19.0%) | 69.34 vs 40.82 MW (+69.9%) |
| Heavy Coilgun Battery Mk1 | `HeavyRailgunBatteryMk2` | 800 km (-50) | 1000 km (+150) | 47.13 vs 17.92 MW (+163.1%) | 88.93 vs 17.92 MW (+396.4%) |
| Heavy Coilgun Battery Mk2 | `HeavyRailgunBatteryMk3` | 850 km (-50) | 1050 km (+150) | 86.40 vs 56.35 MW (+53.3%) | 180.00 vs 56.35 MW (+219.4%) |
| Light Coil Cannon Mk1 | `LightRailCannonMk2` | 550 km (-50) | 650 km (+50) | 17.35 vs 23.33 MW (-25.6%) | 36.26 vs 23.33 MW (+55.4%) |
| Light Coil Cannon Mk2 | `LightRailCannonMk3` | 600 km (-50) | 750 km (+100) | 34.22 vs 55.03 MW (-37.8%) | 84.34 vs 55.03 MW (+53.3%) |
| Coil Cannon Mk1 | `RailCannonMk2` | 700 km (-50) | 850 km (+100) | 48.24 vs 21.47 MW (+124.7%) | 88.00 vs 21.47 MW (+309.9%) |
| Coil Cannon Mk2 | `RailCannonMk3` | 750 km (-50) | 900 km (+100) | 89.38 vs 73.48 MW (+21.6%) | 176.09 vs 73.48 MW (+139.6%) |
| Heavy Coil Cannon Mk1 | `HeavyRailCannonMk2` | 800 km (-50) | 1000 km (+150) | 84.91 vs 40.79 MW (+108.2%) | 153.89 vs 40.79 MW (+277.3%) |
| Heavy Coil Cannon Mk2 | `HeavyRailCannonMk3` | 850 km (-50) | 1050 km (+150) | 153.34 vs 133.26 MW (+15.1%) | 300.52 vs 133.26 MW (+125.5%) |
| Spinal Coiler Mk1 | `SpinalRailgunMk2` | 900 km (-50) | 1100 km (+150) | 126.02 vs 67.18 MW (+87.6%) | 228.42 vs 67.18 MW (+240.0%) |
| Spinal Coiler Mk2 | `SpinalRailgunMk3` | 950 km (-50) | 1150 km (+150) | 226.24 vs 211.31 MW (+7.1%) | 443.52 vs 211.31 MW (+109.9%) |
| Heavy Siege Coiler Mk1 | `HeavyRailCannonMk2` | 800 km (-50) | 1000 km (+150) | 127.16 vs 40.79 MW (+211.8%) | 287.14 vs 40.79 MW (+604.0%) |
| Heavy Siege Coiler Mk2 | `HeavyRailCannonMk3` | 850 km (-50) | 1050 km (+150) | 242.46 vs 133.26 MW (+81.9%) | 528.98 vs 133.26 MW (+297.0%) |
| Spinal Siege Coiler Mk1 | `SpinalRailgunMk2` | 900 km (-50) | 1100 km (+150) | 189.94 vs 67.18 MW (+182.7%) | 419.00 vs 67.18 MW (+523.6%) |
| Spinal Siege Coiler Mk2 | `SpinalRailgunMk3` | 950 km (-50) | 1150 km (+150) | 376.34 vs 211.31 MW (+78.1%) | 796.22 vs 211.31 MW (+276.8%) |

All 18 Coil I/II handoffs, including four siege comparisons, now strictly exceed the rail peer in both range and modeled sustained damage. The smallest range margin is 50 km; larger mounts gain 100–150 km over their peers under the percentage rule.

## Battery hierarchy sanity check

| Tier | Light battery MW | Regular battery MW | Regular / light |
|---|---:|---:|---:|
| Mk1 | 13.07 | 32.95 | 2.52× |
| Mk2 | 30.83 | 69.34 | 2.25× |
| Mk3 | 65.01 | 139.22 | 2.14× |

All three ratios are strictly above 2.00×. Marks 1 and 2 pass without a damage-input adjustment. Mk3 would have landed just below 2.00× after the gentler regular-battery cadence; reducing only the light Mk3 damaging mass by 1 kg raises the ratio to 2.14×. The resulting 10 kg remains above vanilla's 8.75 kg and matches the light-battery Mk1/Mk2 damaging mass.

## Human coil changes

Each cell shows original → proposed (absolute delta), except sustained output, whose parenthesis is the relative delta.

| Weapon | Velocity km/s | Range km | Inter-salvo s | Intra-salvo s | Warhead kg | Full cycle s | Sustained MW |
|---|---:|---:|---:|---:|---:|---:|---:|
| Light Coilgun Battery Mk1 | 4.5 → 5.6 (+1.1) | 500 → 600 (+100) | 28 → 28 (+0) | 10 → 4 (-6) | 10 → 10 (+0) | 48 → 36 (-12) | 6.33 → 13.07 (+106.5%) |
| Light Coilgun Battery Mk2 | 5.4 → 6.8 (+1.4) | 550 → 650 (+100) | 18 → 18 (+0) | 10 → 4 (-6) | 10 → 10 (+0) | 48 → 30 (-18) | 12.15 → 30.83 (+153.7%) |
| Light Coilgun Battery Mk3 | 6.3 → 7.9 (+1.6) | 600 → 750 (+150) | 8 → 8 (+0) | 10 → 4 (-6) | 11 → 10 (-1) | 48 → 24 (-24) | 22.74 → 65.01 (+185.9%) |
| Coilgun Battery Mk1 | 5.4 → 6.8 (+1.4) | 650 → 800 (+150) | 28 → 28 (+0) | 10 → 6 (-4) | 19 → 19 (+0) | 48 → 40 (-8) | 17.31 → 32.95 (+90.3%) |
| Coilgun Battery Mk2 | 6.3 → 7.9 (+1.6) | 700 → 850 (+150) | 18 → 18 (+0) | 10 → 6 (-4) | 20 → 20 (+0) | 48 → 36 (-12) | 33.08 → 69.34 (+109.7%) |
| Coilgun Battery Mk3 | 7.2 → 9.0 (+1.8) | 750 → 900 (+150) | 8 → 8 (+0) | 10 → 6 (-4) | 22 → 22 (+0) | 48 → 32 (-16) | 59.40 → 139.22 (+134.4%) |
| Heavy Coilgun Battery Mk1 | 6.3 → 7.9 (+1.6) | 800 → 1000 (+200) | 28 → 28 (+0) | 10 → 6 (-4) | 38 → 38 (+0) | 48 → 40 (-8) | 47.13 → 88.93 (+88.7%) |
| Heavy Coilgun Battery Mk2 | 7.2 → 9.0 (+1.8) | 850 → 1050 (+200) | 18 → 18 (+0) | 10 → 6 (-4) | 40 → 40 (+0) | 48 → 36 (-12) | 86.40 → 180.00 (+108.3%) |
| Heavy Coilgun Battery Mk3 | 8.1 → 10.1 (+2.0) | 900 → 1100 (+200) | 8 → 8 (+0) | 10 → 6 (-4) | 44 → 44 (+0) | 48 → 32 (-16) | 150.36 → 350.66 (+133.2%) |
| Light Coil Cannon Mk1 | 5.4 → 6.8 (+1.4) | 550 → 650 (+100) | 34 → 34 (+0) | 12 → 5 (-7) | 23 → 23 (+0) | 58 → 44 (-14) | 17.35 → 36.26 (+109.0%) |
| Light Coil Cannon Mk2 | 6.3 → 7.9 (+1.6) | 600 → 750 (+150) | 22 → 22 (+0) | 12 → 5 (-7) | 25 → 25 (+0) | 58 → 37 (-21) | 34.22 → 84.34 (+146.5%) |
| Light Coil Cannon Mk3 | 8.1 → 10.1 (+2.0) | 650 → 800 (+150) | 10 → 10 (+0) | 12 → 5 (-7) | 27 → 27 (+0) | 58 → 30 (-28) | 76.36 → 229.52 (+200.6%) |
| Coil Cannon Mk1 | 6.3 → 7.9 (+1.6) | 700 → 850 (+150) | 34 → 34 (+0) | 12 → 8 (-4) | 47 → 47 (+0) | 58 → 50 (-8) | 48.24 → 88.00 (+82.4%) |
| Coil Cannon Mk2 | 7.2 → 9.0 (+1.8) | 750 → 900 (+150) | 22 → 22 (+0) | 12 → 8 (-4) | 50 → 50 (+0) | 58 → 46 (-12) | 89.38 → 176.09 (+97.0%) |
| Coil Cannon Mk3 | 9.0 → 11.3 (+2.3) | 800 → 1000 (+200) | 10 → 10 (+0) | 12 → 8 (-4) | 55 → 55 (+0) | 58 → 42 (-16) | 192.03 → 418.03 (+117.7%) |
| Heavy Coil Cannon Mk1 | 6.8 → 8.5 (+1.7) | 800 → 1000 (+200) | 34 → 34 (+0) | 12 → 8 (-4) | 71 → 71 (+0) | 58 → 50 (-8) | 84.91 → 153.89 (+81.3%) |
| Heavy Coil Cannon Mk2 | 7.7 → 9.6 (+1.9) | 850 → 1050 (+200) | 22 → 22 (+0) | 12 → 8 (-4) | 75 → 75 (+0) | 58 → 46 (-12) | 153.34 → 300.52 (+96.0%) |
| Heavy Coil Cannon Mk3 | 9.5 → 11.9 (+2.4) | 900 → 1100 (+200) | 10 → 10 (+0) | 12 → 8 (-4) | 82 → 82 (+0) | 58 → 42 (-16) | 318.99 → 691.19 (+116.7%) |
| Spinal Coiler Mk1 | 7.2 → 9.0 (+1.8) | 900 → 1100 (+200) | 34 → 34 (+0) | 12 → 8 (-4) | 94 → 94 (+0) | 58 → 50 (-8) | 126.02 → 228.42 (+81.3%) |
| Spinal Coiler Mk2 | 8.1 → 10.1 (+2.0) | 950 → 1150 (+200) | 22 → 22 (+0) | 12 → 8 (-4) | 100 → 100 (+0) | 58 → 46 (-12) | 226.24 → 443.52 (+96.0%) |
| Spinal Coiler Mk3 | 9.9 → 12.4 (+2.5) | 1000 → 1250 (+250) | 10 → 10 (+0) | 12 → 8 (-4) | 109 → 109 (+0) | 58 → 42 (-16) | 460.48 → 997.61 (+116.6%) |
| Heavy Siege Coiler Mk1 | 3.4 → 4.3 (+0.9) | 800 → 1000 (+200) | 24 → 24 (+0) | 36 → 22 (-14) | 704 → 704 (+0) | 96 → 68 (-28) | 127.16 → 287.14 (+125.8%) |
| Heavy Siege Coiler Mk2 | 3.8 → 4.8 (+1.0) | 850 → 1050 (+200) | 19 → 19 (+0) | 24 → 15 (-9) | 750 → 750 (+0) | 67 → 49 (-18) | 242.46 → 528.98 (+118.2%) |
| Heavy Siege Coiler Mk3 | 4.7 → 5.9 (+1.2) | 900 → 1100 (+200) | 12 → 12 (+0) | 18 → 11 (-7) | 821 → 821 (+0) | 48 → 34 (-14) | 566.75 → 1260.84 (+122.5%) |
| Spinal Siege Coiler Mk1 | 3.6 → 4.5 (+0.9) | 900 → 1100 (+200) | 24 → 24 (+0) | 36 → 22 (-14) | 938 → 938 (+0) | 96 → 68 (-28) | 189.94 → 419.00 (+120.6%) |
| Spinal Siege Coiler Mk2 | 4.1 → 5.1 (+1.0) | 950 → 1150 (+200) | 19 → 19 (+0) | 24 → 15 (-9) | 1000 → 1000 (+0) | 67 → 49 (-18) | 376.34 → 796.22 (+111.6%) |
| Spinal Siege Coiler Mk3 | 5.0 → 6.3 (+1.3) | 1000 → 1250 (+250) | 12 → 12 (+0) | 18 → 11 (-7) | 1094 → 1094 (+0) | 48 → 34 (-14) | 854.69 → 1915.63 (+124.1%) |

The percentage rules change all 27 human intra-salvo rows. Light batteries use 10 → 4 seconds and light cannons use 12 → 5 seconds. Other batteries use 10 → 6 seconds, other cannons use 12 → 8 seconds, and siege intervals use 36/24/18 → 22/15/11 seconds. Inter-salvo reloads are unchanged. Human sustained-output changes span +81.3% to +200.6%.

## Alien magnetic changes

| Weapon | Velocity km/s | Range km | Inter-salvo s | Intra-salvo s | Warhead kg | Full cycle s | Sustained MW |
|---|---:|---:|---:|---:|---:|---:|---:|
| Alien Light Mag Battery | 5.0 → 6.3 (+1.3) | 550 → 650 (+100) | 9 → 9 (+0) | 5 → 3 (-2) | 16 → 16 (+0) | 24 → 18 (-6) | 33.33 → 70.56 (+111.7%) |
| Alien Mag Battery | 6.0 → 7.5 (+1.5) | 700 → 850 (+150) | 11 → 11 (+0) | 6 → 4 (-2) | 32 → 32 (+0) | 29 → 23 (-6) | 79.45 → 156.52 (+97.0%) |
| Alien Heavy Mag Battery | 7.0 → 8.8 (+1.8) | 850 → 1050 (+200) | 13 → 13 (+0) | 7 → 5 (-2) | 64 → 64 (+0) | 35 → 28 (-7) | 181.27 → 354.01 (+95.3%) |
| Alien Mini Light Mag Cannon | 6.0 → 7.5 (+1.5) | 600 → 750 (+150) | 18 → 18 (+0) | 5 → 3 (-2) | 43 → 43 (+0) | 28 → 24 (-4) | 82.93 → 151.17 (+82.3%) |
| Alien Light Mag Cannon | 6.0 → 7.5 (+1.5) | 600 → 750 (+150) | 18 → 18 (+0) | 5 → 3 (-2) | 43 → 43 (+0) | 28 → 24 (-4) | 82.93 → 151.17 (+82.3%) |
| Alien Mag Cannon | 7.0 → 8.8 (+1.8) | 750 → 900 (+150) | 22 → 22 (+0) | 6 → 4 (-2) | 85 → 85 (+0) | 34 → 30 (-4) | 183.75 → 329.12 (+79.1%) |
| Alien Heavy Mag Cannon | 8.3 → 10.4 (+2.1) | 850 → 1050 (+200) | 32 → 32 (+0) | 9 → 6 (-3) | 128 → 128 (+0) | 50 → 44 (-6) | 264.54 → 471.97 (+78.4%) |
| Alien Spinal Mag Cannon | 10.0 → 12.5 (+2.5) | 950 → 1150 (+200) | 43 → 43 (+0) | 12 → 8 (-4) | 170 → 170 (+0) | 67 → 59 (-8) | 380.60 → 675.32 (+77.4%) |
| Advanced Alien Light Mag Battery | 6.7 → 8.4 (+1.7) | 650 → 800 (+150) | 4 → 4 (+0) | 5 → 3 (-2) | 17 → 17 (+0) | 24 → 16 (-8) | 79.49 → 187.43 (+135.8%) |
| Advanced Alien Mag Battery | 7.8 → 9.8 (+2.0) | 800 → 1000 (+200) | 5 → 5 (+0) | 6 → 4 (-2) | 34 → 34 (+0) | 29 → 21 (-8) | 178.32 → 388.73 (+118.0%) |
| Advanced Alien Heavy Mag Battery | 9.4 → 11.8 (+2.4) | 950 → 1150 (+200) | 6 → 6 (+0) | 7 → 5 (-2) | 68 → 68 (+0) | 35 → 26 (-9) | 431.64 → 910.42 (+110.9%) |
| Advanced Alien Light Mag Cannon | 8.6 → 10.8 (+2.2) | 700 → 850 (+150) | 13 → 13 (+0) | 5 → 3 (-2) | 45 → 45 (+0) | 28 → 22 (-6) | 237.73 → 477.16 (+100.7%) |
| Advanced Alien Mag Cannon | 10.0 → 12.5 (+2.5) | 850 → 1050 (+200) | 16 → 16 (+0) | 6 → 4 (-2) | 90 → 90 (+0) | 34 → 28 (-6) | 529.41 → 1004.46 (+89.7%) |
| Advanced Alien Heavy Mag Cannon | 12.5 → 15.6 (+3.1) | 950 → 1150 (+200) | 23 → 23 (+0) | 9 → 6 (-3) | 135 → 135 (+0) | 50 → 41 (-9) | 843.75 → 1602.61 (+89.9%) |
| Advanced Alien Spinal Mag Cannon | 15.0 → 18.8 (+3.8) | 1050 → 1300 (+250) | 31 → 31 (+0) | 12 → 8 (-4) | 180 → 180 (+0) | 67 → 55 (-12) | 1208.96 → 2313.43 (+91.4%) |
| Gen3 Alien Light Mag Battery | 8.2 → 10.3 (+2.1) | 700 → 850 (+150) | 4 → 4 (+0) | 4 → 3 (-1) | 30 → 30 (+0) | 24 → 19 (-5) | 252.15 → 502.53 (+99.3%) |
| Gen3 Alien Mag Battery | 10.3 → 12.9 (+2.6) | 850 → 1050 (+200) | 5 → 5 (+0) | 5 → 3 (-2) | 60 → 60 (+0) | 30 → 20 (-10) | 636.54 → 1497.69 (+135.3%) |
| Gen3 Alien Heavy Mag Battery | 12.4 → 15.5 (+3.1) | 1000 → 1250 (+250) | 6 → 6 (+0) | 6 → 4 (-2) | 120 → 120 (+0) | 36 → 26 (-10) | 1537.60 → 3326.54 (+116.3%) |
| Gen3 Alien Light Mag Cannon | 10.8 → 13.5 (+2.7) | 800 → 1000 (+200) | 6 → 6 (+0) | 4 → 3 (-1) | 80 → 80 (+0) | 26 → 21 (-5) | 1076.68 → 2082.86 (+93.5%) |
| Gen3 Alien Mag Cannon | 13.1 → 16.4 (+3.3) | 900 → 1100 (+200) | 8 → 8 (+0) | 5 → 3 (-2) | 160 → 160 (+0) | 33 → 23 (-10) | 2496.15 → 5613.08 (+124.9%) |
| Gen3 Alien Heavy Mag Cannon | 16.5 → 20.6 (+4.1) | 1000 → 1250 (+250) | 14 → 14 (+0) | 6 → 4 (-2) | 320 → 320 (+0) | 38 → 30 (-8) | 5731.58 → 11316.27 (+97.4%) |
| Gen3 Alien Spinal Mag Cannon | 19.8 → 24.8 (+5.0) | 1100 → 1350 (+250) | 23 → 23 (+0) | 7 → 5 (-2) | 640 → 640 (+0) | 52 → 43 (-9) | 12109.34 → 22885.21 (+89.0%) |

The global 40% intra-salvo target changes all 22 alien rows. Alien sustained-output changes span +77.4% to +135.8%. Their inter-salvo reloads remain unchanged.

## Interpretation and risks

The range correction is now proportional rather than a flat translation. Because original ranges are 50 km increments and the scaled result is floored, realized range increases vary from 18.2% to 25.0%, or 100–250 km in absolute terms.

Velocity still enters kinetic damage quadratically, so the locked 1.25 velocity factor raises per-shot damage by roughly 55–59% except for the targeted Light Battery Mk3 adjustment, where the net per-shot increase is 42.9%. The intra-salvo rules shorten the full cycle while leaving the pause between completed salvos untouched.

Manual testing should focus on:

1. Light Coil I versus Rail II and Light Coil II versus Rail III, verifying that the handoff feels like an upgrade without deleting the rail niche.
2. Light versus regular batteries, confirming the 60%/40% intra-salvo split and the Mk3 10 kg light-battery warhead preserve distinct roles.
3. Heavy and spinal siege coils, confirming that 36/24/18 → 22/15/11 seconds reads as a linear cadence progression in combat.
4. Alien magnetic weapons across all mount sizes, because the global 40% target reduction is propagated to every tier.
5. Projectile interception and evasion, because higher velocity shortens defensive reaction time.

## Automated acceptance criteria

`tools/validate-ship-rebalance.ps1` asserts:

- all 18 Coil I/II handoffs, including siege coils, strictly exceed their rail peer in range and modeled sustained damage;
- every affected intra-salvo interval is no longer than its own inter-salvo reload;
- every Coil I/II intra-salvo interval is no longer than the peer rail's inter-salvo reload;
- each Mk2→Mk3 velocity ratio stays within 1% of the installed ratio;
- every human coil range follows the 1.25 scale and floor-to-50 rule;
- every inter-salvo value is unchanged from the original research snapshot;
- every human light-coil intra-salvo value equals `ceil(original × 0.40)`;
- every other human coil and alien-mag intra-salvo value equals `ceil(original × 0.60)`;
- regular Coilgun Battery sustained damage is strictly greater than twice Light Coilgun Battery sustained damage at all three marks;
- alien magnetic weapons retain strict performance dominance over their mapped human references;
- affected reloads and ranges are whole-valued and velocities are rounded to 0.1 km/s.
