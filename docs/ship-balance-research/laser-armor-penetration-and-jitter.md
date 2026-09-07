# Laser size, armor penetration, ablation, X-rays, and jitter

Research date: 2026-09-07. Scope: explain the installed game's tactical ship-combat calculations and the existing EEO changes relevant to them. This is a documentation-only investigation; no gameplay, data templates, or tooling were changed and no build or deployment was required.

Method: inspect the installed laser/armor templates and English localization, decompile the current Assembly-CSharp.dll, trace beam firing through damage and armor handling, inspect EEO projectile patches, then calculate the examples below. The repository targets Terra Invicta 1.0.53 / EEO 0.9.7. The authoritative inspected DLL has SHA-256 `FF7916C2085DDBAFA5ACF1E8EA185D37E629096752BE388BA6FA1F627F027BB5`, last written 2026-08-31. Results describe that binary rather than assuming every historical version behaves identically.

## Why larger lasers have lower raw DPS per node

The size tradeoff is substantial armor penetration and range in exchange for lower raw DPS per node. This is established behavior in the calculations; the exact designer rationale for the numerical DPS progression is an inference, not a verified developer statement.

Human laser batteries have 100 / 150 / 250 MJ per shot at 1 / 2 / 4 nodes. Cannons have 200 / 300 / 350 / 400 MJ at 1 / 2 / 3 / 4 nodes. Ordinary, arc, and phaser attack cooldowns are respectively 30 / 20 / 10 seconds. A damage point represents 20 MJ. Within each generation the wavelengths share those energies and cooldowns, but differ in focus and therefore penetration.

The larger weapons have larger optics and lower jitter. They deliver more energy in each shot to a much smaller spot. Armor is subtracted separately from every hit, so several poorly penetrating small shots cannot simply pool their raw energy into one penetrating shot. Kinetics also benefit from overcoming a per-hit subtraction, but do not use the laser spot-area multiplier or laser armor exponent.

For example, at 400 km against intact 10-point armor, the green phaser cannons deliver nominal penetration DPS of 0 / 0.920 / 1.467 / 1.833 for the 1 / 2 / 3 / 4-node versions. Four small cannons have twice the raw DPS of the four-node cannon, but in this example their individual shots are all absorbed. Conversely, against unarmored targets large enough to intercept the whole spot, small weapons retain their raw DPS/node advantage. This does not by itself establish the optimal PD loadout: defensive cooldowns, engagement limits, targets, and projectile durability also matter.

## Exact laser calculation

Use the following notation:

- `r`: range in metres.
- `lambda`: wavelength in metres.
- `Q`: beam quality, with lower values giving a tighter beam. Human ordinary / arc / phaser values are 1.20 / 1.15 / 1.10.
- `D`: optic diameter in metres. The code uses **twice** `mirrorRadius_cm / 100`. Weapon labels such as “60 cm” correspond to the stored radius field; do not substitute the label directly as `D`.
- `j`: `jitter_Rad` in radians.
- `P`: intercepted shot damage in damage points, before chipping diversion.
- `A`: intact armor points after any applicable obliquity adjustment.

```text
spot diameter d = r * sqrt((1.22 * lambda * Q / D)^2 + (2*j)^2)
spot area S = pi*d^2/4
armor effectiveness E = S / 0.005
intercepted energy fraction f = min(1, target cross-sectional area / S)
P = shot energy MJ / 20 * f                  [before bonuses]
chipping fraction c = clamp(E / 1000, 0, 0.25)
effective armor = A^1.5 * E
nominal penetration damage = max(0, P*(1-c)*laser-resistance multiplier - A^1.5*E)
```

These last lines assume intact armor, no faction/officer modifiers, and normal shot handling. The actual `Damage` constructor independently randomizes direct and chipping damage to 80–120% of their nominal values. Near a penetration threshold, nominal damage is therefore not exactly mean damage. There are additional radiator, sensor, exposed-drive, and internal-damage paths; zero normal armor penetration does not guarantee absolutely no damage anywhere on a ship.

The displayed armor-effectiveness percentage is **not** a percentage chance to penetrate and is not the whole armor calculation. At 50% effectiveness, 10 armor becomes `10^1.5 * 0.5 = 15.81` damage points of absorption, not 5. Lower effectiveness is better for the laser. A bigger aperture helps roughly as diameter squared while diffraction dominates; jitter limits that improvement.

At fixed weapon characteristics, spot diameter is exactly proportional to distance in this implementation, and spot area and armor effectiveness are proportional to distance squared. Ignoring the small chipping diversion, the maximum nominal penetrable armor is approximately `(P/E)^(2/3)`. Targeting range remains a separate hard constraint; a mathematical penetration range longer than the listed targeting range does not let a weapon fire farther.

In realistic combat scaling, lateral armor is increased for oblique hits before applying the laser's 1.5 exponent. The code divides the relevant lateral armor value by the cosine of the incidence angle relative to the side normal. The examples here use normal incidence. Cinematic mode skips that particular adjustment.

## Does it physically model laser ablation?

It models persistent armor damage, but through an abstract chipping system rather than a resolved thermal ablation simulation.

Armor materials have density and heat of vaporization. Their thickness per armor point is derived from enough material to consume 20 MJ when vaporized over a reference area of 0.005 square metres:

```text
mass per damage point (kg) = 20 / heat of vaporization (MJ/kg)
thickness per armor point (m) = 20 / (heat of vaporization * density * 0.005)
```

Those material properties establish the physical thickness and mass associated with the abstract armor stat. They do not cause the tactical code to maintain local temperatures, conduct heat between plates, accumulate melt depth at a tracked impact coordinate, or propagate an ablation plume. At equal armor points, ordinary intact laser absorption is determined by `A^1.5 * E`, with any laser-resistance specialty, rather than recalculating material vaporization from the laser fluence.

Each laser hit diverts `c = clamp(E/1000, 0, 0.25)` of intercepted damage into chipping. On an intact hit, the armor code applies the material's chipping multiplier and divides this amount by the facing's armor volume to increase `chippedPct`. It does not convert this quantity through `volume_damagePoint_m3` at that step. This is a game rule, not a dimensionally complete vaporization calculation. The chipping fraction is not itself the percentage of armor removed per shot.

Before processing a hit, the game rolls against the existing `chippedPct`. A hit in the abstract damaged area bypasses normal armor absorption. A 10% chipped facing gives a later ordinary single hit a 10% chance of taking that bypass path. Chipping normally leaves the integer `armorValue` intact; the separate shredding mechanic reduces that value. Penetrating damage that exits a ship can add further chipping. There is no mapped hole that another laser deliberately aims into.

One consequence of the formula is that a wider, worse-penetrating laser spot diverts a larger fraction into chipping, up to 25%. A blocked shot can therefore still degrade armor coverage. That is distinct from progressively drilling the same spot thinner.

## X-ray protection and penetration

The English tooltip is **“Points to halve X-ray damage.”** A lower value means fewer armor points are needed to halve the X-ray component, and thus stronger protection per point. It is not a flat defense value to subtract from a hit.

The displayed value is authored in the armor's `XRayResistance` specialty. Tactical ship radiation absorption instead uses physical thickness and `xRayHalfValue_cm`. For the inspected armors, the displayed value matches the ratio of half-value thickness to thickness per armor point, rounded to two decimals.

```text
t = armor points * thickness per point (cm)
h = xRayHalfValue_cm
nominal exponential transmission = 0.5^(t/h)
installed intact-hit transmission = min(0.0625, 0.5^(t/h))
X-ray component after armor = incoming X-ray component * transmission
```

The `min`, rather than `max`, is significant: this binary allows at most 6.25% transmission on an intact armored hit. Additional thickness only reduces transmission further once it exceeds four half-value thicknesses. This is an observed rule in `AbsorbAndApplyArmorDamage`, not a claim that the clamp is physically realistic or intentionally chosen. A chipped-area hit bypasses this intact-hit attenuation. Officer radiation protection and subsequent internal routing can reduce damage further; the transmitted fraction is not a promise that every internal component takes that percentage.

Steel requires approximately 0.267 armor points per X-ray halving, nanotube 2.533, and adamantane 4.822. Thus adamantane is much weaker than steel **per armor point** against this radiation component, while having much less mass per square metre per armor point. Exotic and hybrid armor improve X-ray shielding relative to adamantane. Progression is not monotonic across all armor properties.

This X-ray attenuation is used for the **X-ray fraction of particle-beam damage**. Particle damage is split into heat, X-ray, and baryonic components, with the latter having its own analogous half-value attenuation. Ordinary lasers, UV lasers, and even alien **Xasers** are `WeaponClass.Laser`, use the laser penetration rule, and return thermal damage. Their very short wavelength improves the spot-size term; the armor's X-ray half-value stat is not consulted for their laser hit. Hybrid's laser-resistance specialty is a separate modifier (0.75 to incoming laser direct damage on intact armor).

## Jitter, hit chance, ships, and projectiles

Laser jitter is a fixed angular beam-quality parameter, not a random angular offset sampled when firing. The tactical `BeamWeapon.TryFire` applies a beam damage source directly to its selected target. Its collider raycast establishes a hit position; it does not turn jitter into a random miss.

The jitter-only contribution to effective **diameter** is `2*j*r`. The full diameter combines that contribution and diffraction in quadrature. The table's diameters are not maximum misses, one-sigma hit probabilities, or circular error probable. The code does not define them as those statistical quantities.

Against ships, the beam spot is usually much smaller than the ship cross-section, so the full shot is intercepted and jitter primarily hurts armor penetration. Against small projectiles, a spot larger than the projectile delivers only its area fraction. For circular cross-sections this fraction is `min(1, (projectile diameter / spot diameter)^2)`. That is reduced energy per hit, not that fraction as a hit probability. Doubling range quarters intercepted energy once the target is smaller than the spot.

Example: a 60 cm green phaser battery has a 12.24 cm spot at 200 km and a 24.48 cm spot at 400 km. A circular 10 cm projectile intercepts about 66.7% and 16.7% of the respective shots. The 400 km figure is a geometric illustration outside that battery's normal PD engagement envelope, not a claim it will take that defensive shot.

Projectiles do not run through the ship armor subtraction. Magnetic rounds instead accumulate mass damage (10 kg per direct damage point in the inspected vanilla laser-hit path), so lower intercepted beam energy means slower destruction. Missiles use their destruction handler after a delivered hit, with targeting/minimum-damage gates affecting whether the beam fires in the first place. Consequently one universal laser hit-chance percentage would be misleading. PD also has range, pivot, cooldown, target saturation, and threat-selection rules.

EEO's existing ship-balance feature replaces modeled gun/magnetic projectile cross-sections with caliber-based circular areas and sizes ballistic colliders accordingly. Its naval-shell durability patch accumulates `100 kg * (direct + chipping damage points)` rather than vanilla naval shells' immediate laser-hit destruction. These patches make small-projectile energy interception especially relevant to this mod. The only current EEO laser-template override is the basic PD turret's crew requirement; no laser optics or shot-power override is present there.

Gun launch code uses a predicted intercept and sets launch velocity directly toward it, adding shooter velocity. It does not apply laser jitter or sample a random launch-spread term in that path. Projectile weapons can miss because of finite flight time, subsequent target maneuver, prediction, and collider geometry. Ship/missile targets use a second-order intercept estimate; ballistic targets use a first-order estimate. These effects do not imply a universal distance-only hit-probability formula.

There is a separate ECM acquisition roll. The inspected `Weapon.TargetChance` calculates `1 + targeting bonus + accumulated ECM-defeat bonus - target ECM * min(1, range / targeting range)`. Acquisition failures impose a cooldown; this is separate from laser jitter and is not a fresh jitter miss roll on every beam shot. Sensors and the current acquisition state also affect when the roll occurs.

## Evidence and reproducibility

Primary sources are the locally installed game, not historical wiki formulas:

- Game root: `D:\Games\SteamLibrary\steamapps\common\Terra Invicta`.
- `TerraInvicta_Data\StreamingAssets\Templates\TILaserWeaponTemplate.json`: energy, cadence, optics, jitter, range, and alien laser definitions.
- `TerraInvicta_Data\StreamingAssets\Templates\TIShipArmorTemplate.json`: density, vaporization energy, half-value thicknesses, and specialties.
- `TerraInvicta_Data\StreamingAssets\Localization\en\TIShipArmorTemplate.en`: the exact half-damage tooltip meaning.
- Current DLL `TILaserWeaponTemplate`: `SpotDiameterPrecise_m`, `SpotAreaPrecise_m2`, `ArmorEffectivenessAtRange`, `ModifyArmorValueForLaserShot`, `DamageAtRange_MJ`, `chipping`.
- Current DLL `TIShipWeaponTemplate.DamageAtRange_points`, `TIShipArmorTemplate`, `TISpaceShipState.ArmorData`, `AbsorbAndApplyArmorDamage`, `ApplyInternalDamage`, `ApplyInternalRadiationDamage`.
- Current DLL `Ship.BeamWeapon`, `Ship.Damage`, `Ship.HullSection`, `Ship.Weapon`, `SpaceCombat.ProjectileController`, `SpaceCombat.MissileController`, and `TISpaceCombatProjectileState`.
- EEO source: `TIEconomyMod\Patches\ProjectileCollisionPatches.cs`, `TIEconomyMod\Core\ProjectileCollisionMath.cs`, and `TIEconomyMod\ModFiles\TILaserWeaponTemplate.json`. The deployed laser override was also inspected and matches the repository override.

Fresh decompilation extracts and the disposable calculation script are under `.tmp/laser-armor-audit/`. The persistent tables follow. Calculations use full-precision template numbers and the equations above; printed values are rounded. No in-game experiment was performed, and these tables isolate mechanics rather than predict whole-battle DPS.

## Human green phaser size comparison

Nominal shots, full spot intercepted, intact 10-point armor, normal incidence, no bonuses or specialties. Damage points = MJ / 20. Penetration includes the chipping diversion but excludes the independent 0.8–1.2 random rolls. Values are nominal, not the exact expected value across random rolls.

| Weapon | Nodes | Raw DPS | DPS/node | Spot at 400 km (cm) | Armor multiplier at 400 km | Internal DPS vs 10 armor at 400 km | Nominal penetration limit vs 10 armor (km) |
|---|---:|---:|---:|---:|---:|---:|---:|
| 60 cm Green Phaser Battery | 1 | 0.500 | 0.500 | 24.48 | 9.417 | 0.000 | 51.8 |
| 120 cm Green Phaser Battery | 2 | 0.750 | 0.375 | 12.49 | 2.452 | 0.000 | 124.4 |
| 360 cm Green Phaser Battery | 4 | 1.250 | 0.3125 | 4.77 | 0.358 | 0.119 | 420.5 |
| 240 cm Green Phaser Cannon | 1 | 1.000 | 1.000 | 6.39 | 0.641 | 0.000 | 280.9 |
| 480 cm Green Phaser Cannon | 2 | 1.500 | 0.750 | 3.42 | 0.183 | 0.920 | 643.1 |
| 720 cm Green Phaser Cannon | 3 | 1.750 | 0.583 | 2.39 | 0.089 | 1.467 | 995.0 |
| 960 cm Green Phaser Cannon | 4 | 2.000 | 0.500 | 1.83 | 0.053 | 1.833 | 1384.0 |

## Human jitter values

Nanoradians (1 nrad = 1e-9 rad). Values are identical across IR, green, and UV for a given mount and generation. PD uses its own row.

| Mount | Basic laser (nrad) | Arc laser (nrad) | Phaser (nrad) | Phaser jitter-only diameter at 100 km (cm) | At 500 km (cm) | At 1,000 km (cm) |
|---|---:|---:|---:|---:|---:|---:|
| PD | 100 | 75 | 50 | 1.00 | 5.00 | 10.00 |
| 60 cm Battery | 90 | 75 | 50 | 1.00 | 5.00 | 10.00 |
| 120 cm Battery | 72 | 60 | 40 | 0.80 | 4.00 | 8.00 |
| 360 cm Battery | 58 | 48 | 32 | 0.64 | 3.20 | 6.40 |
| 240 cm Cannon | 46 | 38 | 26 | 0.52 | 2.60 | 5.20 |
| 480 cm Cannon | 37 | 31 | 20 | 0.40 | 2.00 | 4.00 |
| 720 cm Cannon | 29 | 25 | 16 | 0.32 | 1.60 | 3.20 |
| 960 cm Cannon | 24 | 20 | 13 | 0.26 | 1.30 | 2.60 |

## Actual green phaser spot diameters

These include diffraction and jitter, in centimetres. Out-of-targeting-range entries are mathematical extrapolations, not permission for the weapon to fire.

| Weapon | 100 km | 200 km | 400 km | 600 km | 1,000 km | Targeting range (km) |
|---|---:|---:|---:|---:|---:|---:|
| 60 cm Green Phaser Battery | 6.12 | 12.24 | 24.48 | 36.73 | 61.21 | 600 |
| 120 cm Green Phaser Battery | 3.12 | 6.25 | 12.49 | 18.74 | 31.24 | 700 |
| 360 cm Green Phaser Battery | 1.19 | 2.39 | 4.77 | 7.16 | 11.93 | 850 |
| 240 cm Green Phaser Cannon | 1.60 | 3.19 | 6.39 | 9.58 | 15.97 | 800 |
| 480 cm Green Phaser Cannon | 0.85 | 1.71 | 3.42 | 5.13 | 8.54 | 900 |
| 720 cm Green Phaser Cannon | 0.60 | 1.19 | 2.39 | 3.58 | 5.96 | 950 |
| 960 cm Green Phaser Cannon | 0.46 | 0.92 | 1.83 | 2.75 | 4.58 | 1000 |

## X-ray armor values

Transmission columns include the installed ship-code cap of 6.25%, assume an intact hit, and apply to the particle weapon X-ray component before internal routing. Smaller points-to-halve values mean better protection per armor point.

| Armor | cm per armor point | cm to halve X-rays | Calculated points to halve | Displayed points to halve | Transmission at 5 armor | Transmission at 20 armor |
|---|---:|---:|---:|---:|---:|---:|
| Steel Armor | 7.493 | 2 | 0.267 | 0.27 | 0.000229% | 0.000000% |
| Titanium Armor | 9.463 | 4.2 | 0.444 | 0.44 | 0.040633% | 0.000000% |
| Silicon Carbide Armor | 12.587 | 9.1 | 0.723 | 0.72 | 0.828149% | 0.000000% |
| Boron Carbide Armor | 22.231 | 22.2 | 0.999 | 1 | 3.109857% | 0.000094% |
| Composite Armor | 13.817 | 15.3 | 1.107 | 1.11 | 4.372690% | 0.000366% |
| Foamed Metal Armor | 18.116 | 29.4 | 1.623 | 1.62 | 6.250000% | 0.019506% |
| Nanotube Armor | 7.857 | 19.9 | 2.533 | 2.53 | 6.250000% | 0.419773% |
| Adamantane Armor | 3.733 | 18 | 4.822 | 4.82 | 6.250000% | 5.642827% |
| Exotic Armor | 2.597 | 5.2 | 2.002 | 2 | 6.250000% | 0.098335% |
| Hybrid Armor | 2.500 | 4.5 | 1.800 | 1.8 | 6.250000% | 0.045209% |

## Alien ship laser jitter

Wavelength values in this table are the authored JSON values; no alien penetration examples are calculated here.

| Weapon | Jitter (nrad) | Beam quality | Wavelength (nm) |
|---|---:|---:|---:|
| Alien Point Defense Laser Turret | 50 | 1.1 | 810 |
| Alien 64 cm Orange Laser Battery | 50 | 1.1 | 630 |
| Alien 128 cm Orange Laser Battery | 40 | 1.1 | 630 |
| Alien 384 cm Orange Laser Battery | 32 | 1.1 | 630 |
| Alien 256 cm Orange Laser Cannon | 26 | 1.1 | 630 |
| Alien 512 cm Orange Laser Cannon | 20 | 1.1 | 630 |
| Alien 768 cm Orange Laser Cannon | 16 | 1.1 | 630 |
| Alien 1024 cm Orange Laser Cannon | 13 | 1.1 | 630 |
| Alien 64 cm Violet Laser Battery | 50 | 1.1 | 450 |
| Alien 128 cm Violet Laser Battery | 40 | 1.1 | 450 |
| Alien 384 cm Violet Laser Battery | 32 | 1.1 | 450 |
| Alien 256 cm Violet Laser Cannon | 26 | 1.1 | 450 |
| Alien 512 cm Violet Laser Cannon | 20 | 1.1 | 450 |
| Alien 768 cm Violet Laser Cannon | 16 | 1.1 | 450 |
| Alien 1024 cm Violet Laser Cannon | 13 | 1.1 | 450 |
| Alien 384 cm Xaser Battery | 32 | 1.05 | 10 |
| Alien 256 cm Xaser Cannon | 26 | 1.05 | 10 |
| Alien 512 cm Xaser Cannon | 20 | 1.05 | 10 |
| Alien 768 cm Xaser Cannon | 16 | 1.05 | 10 |
| Alien 1024 cm Xaser Cannon | 13 | 1.05 | 10 |
| Alien 768 cm Graser Cannon | 13 | 1.05 | 0.1 |
| Alien 1024 cm Graser Cannon | 10 | 1.05 | 0.1 |

