# Nose weapon scaling audit and A/B candidates

Follow-up: [nose batteries and enlarged spinal tier](nose-battery-and-spinal-tier-proposal-2026-09-25.md)
evaluates a new size-4-to-size-3 ratio and per-class weapon counts against the
current 0.10.0 baseline. It does not adopt this audit's size-1–3 A/B normalization.

2026-09-25. **Deferred research; no nose rebalancing in the active plan.**
Retained for user tuning and combat testing. All correction recommendations
below are exploratory, not implementation instructions for this release.
Reference mass denominators reflect the latest hull targets.
Companion to the
[capital heavy-mount plan](capital-heavy-mount-plan-2026-09-25.md).
Baseline: installed TI 1.0.53b templates merged with EEO 0.9.8 overrides.

## Definitions and interpretation

A uses nose weights 2/4/8/16 relative to a small hull weapon; B uses 3/6/12/24.
Hull size weights are 1/2/4. The capital mount proposal changes geometry, while
the nose audit evaluates a separate stat correction. Keep those effects distinct.

Mounting mass is `baseWeaponMass_tons`, excluding ammunition. Full-cycle interval
is `(cooldown + (salvo_shots−1) × intraSalvoCooldown) / salvo_shots`, matching the
native `averageCooldown_s`. Raw throughput is nominal energy per shot divided
by this interval, in MJ/s = MW. This is not the game's target/range-weighted
`EstimateDPS`, not damage points/s, and not observed combat damage.

- Laser/particle emitted energy: `shotPower_MJ`.
- Gun/magnetic/plasma projectile kinetic energy: `0.5 × warheadMass_kg × v_kps²`.
- Magnetic electrical energy: `0.5 × ammoMass_kg × v_kps² / efficiency`.
- Beam electrical energy: `shotPower_MJ / efficiency`.
- Plasma electrical energy: `(1000 × chargingEnergy_GJ + projectile kinetic MJ)
  / efficiency`.
- Naval gun electrical demand: mod `powerUse_MJ / efficiency` when supplied,
  otherwise native zero. Chemical propellant is not ship electrical demand.

Electrical columns are **full-cycle mean firing draw**, not installed reactor
requirements. The mod's design-heat calculation can use intra-salvo intervals;
burst input, capacitor capacity, reactor sizing and heat rejection need their
own checks. Particle damage channels, laser diffusion/armor, relative velocity,
accuracy, interception and overkill are excluded from raw throughput.

## Findings and proposed corrections

1. **Retain laser optical scaling.** Human nose shot energies are 200/300/350/400
   MJ; mass and aperture increase differently. This is not sufficient evidence
   of a balance error because aperture improves useful damage at range and
   against armor. A/B do not multiply laser raw energy, draw or mass. Test optical
   effectiveness at 200/500/800/1,000 km against representative armor and target
   cross sections before proposing laser corrections. Track target interception
   cadence separately from anti-ship output. A 2× or 3× design weight is not a
   universal on-target damage multiplier.
2. **Magnetic mounting masses are approximately linear in slot count**, e.g.
   Mk3 nose mounts 50/100/150/200 t, while intended capability doubles by size.
   Propose geometric support/mounting mass along with power and DPS. Exact A/B
   screening values are below. Preserve published range/velocity and cadence
   initially; solve for ammunition/damaging mass and verify the resulting physical
   and thermal costs. Update projectile geometry/collision data if a mass change
   implies a different projectile size.
3. **Rail size progressions are uneven.** Mk3 nose output is approximately
   55/73/133/211 MW; Mk1's two-cell cannon is actually below its one-cell peer in
   full-cycle output. Propose the A/B targets to remove such reversals. Retest
   the established rail-to-coil research progression, not just within-family sizes.
4. **Coils already exceed A in several small/medium nose sizes.** Mk3 actual
   230/418/691/998 MW compares with A 130/260/520/1,040 or B 195/390/780/1,560.
   Consequently A is not an all-buff correction. B is closer for smaller coils
   and raises large output. **Prefer B as the first non-laser screening candidate**
   if the goal is mainly increasing larger mounts; retain the option to exempt
   existing small mounts rather than applying a blanket nerf. Such exemptions
   require regenerated fit tables before implementation.
5. **Plasma's size-3→4 gain is weak:** Mk3 raw output is about 13.40→13.78 MW
   despite 420→560 t mass and higher electrical demand. A/B give concrete stronger
   candidates. Preserve range, projectile speed and interception immunity; these
   are separate advantages that justify family-specific tuning around the target.
6. **Ordinary ion/neutral particle noses already double within the nose family.**
   Their discrepancy is often the absolute hull-to-nose anchor. The strict A/B
   targets therefore can be much larger; do not mistake existing internally
   consistent doubling for an implementation defect. Retain channel fractions
   and dispersion unless explicitly retuned.
7. **Antimatter particle mass has a clear discontinuity:** 40/240/480 t at nose
   sizes 2/3/4, while energy and mean power double normally. The mechanical
   lowest-anchor screen gives 40/80/160 t, but the more suitable **recommended
   correction is 120/240/480 t**, raising the underweight size-2 mount and
   preserving the larger ones. No raw-energy correction is needed for internal
   doubling. There is no matching offensive hull counterpart: do not anchor this
   family to the defensive antimatter battery solely because its name matches.
8. **Alien particle masses reverse at size 2:** 150/80/400/800 t. A provisional
   nose-local series is 150/300/600/1,200 t and corresponding doubled output.
   This is deferred weapon mounting-mass research; the active plan changes only
   the explicitly selected hull masses and leaves all alien crew values untouched.
   No matching offensive hull anchor exists, so A and B do
   not independently fix the family's absolute baseline yet.
9. **Naval cannons need an exception for chemical propulsion and caliber.** The
   12-inch currently has no mod electrical-input extension, unlike the 10-inch.
   A concrete conservative proposal preserves the 10-inch at 125 t, 24.05 MW
   raw output and 0.30 MW mean electrical input, and sets the 12-inch to
   **250 t, 48.11 MW, 0.60 MW**. At its present average interval of 12.5 s, that
   last value requires 7.5 MJ electrical input per shot. This nose-local option
   is preferable to mechanically reducing both mounting masses to small-hull
   multiples. Check projectile mass/caliber and ammunition together.
10. **Siege coils and unique spinal weapons stay separate families.** For a siege
    size-3→4 series the within-family screen doubles current size-3 metrics,
    rather than treating it as an ordinary coil cannon. Unique spinal neutron
    and alien relativistic particle weapons have no size ladder and remain
    unchanged pending dedicated anchors.

These are proposals for review, not approved gameplay values. The full strict
screen below intentionally exposes downwards corrections. Recommended special
cases above take precedence over treating every table row as an implementation
instruction. Reference fits below use regular coils/lasers/advanced alien mags,
so the antimatter and naval-gun exceptions do not alter those fit results.

## Current nose families

Ordered by size within each family. Averages include complete salvo cycles.

| Family | Sizes | Current mounting mass, t | Current raw DPS, MW | Current mean electrical draw, MW |
| --- | --- | --- | --- | --- |
| Rail Mk1 | 1 / 2 / 3 / 4 | 60 / 120 / 180 / 240 | 11.4 / 9.3 / 16.9 / 26.8 | 60.8 / 49.8 / 90.2 / 143.1 |
| Coil Mk1 | 1 / 2 / 3 / 4 | 60 / 120 / 180 / 240 | 36.3 / 88.0 / 153.9 / 228.4 | 97.7 / 235.9 / 407.5 / 607.5 |
| Siege coil Mk1 | 3 / 4 | 1,000 / 1,500 | 287.1 / 419.0 | 765.2 / 1,116.7 |
| Rail Mk2 | 1 / 2 / 3 / 4 | 55 / 110 / 165 / 220 | 23.3 / 21.5 / 40.8 / 67.2 | 97.2 / 89.4 / 169.9 / 279.9 |
| Coil Mk2 | 1 / 2 / 3 / 4 | 55 / 110 / 165 / 220 | 84.3 / 176.1 / 300.5 / 443.5 | 174.3 / 369.8 / 627.8 / 924.0 |
| Siege coil Mk2 | 3 / 4 | 1,000 / 1,500 | 529.0 / 796.2 | 1,102.6 / 1,658.8 |
| Rail Mk3 | 1 / 2 / 3 / 4 | 50 / 100 / 150 / 200 | 55.0 / 73.5 / 133.3 / 211.3 | 179.7 / 239.9 / 435.1 / 690.0 |
| Coil Mk3 | 1 / 2 / 3 / 4 | 50 / 100 / 150 / 200 | 229.5 / 418.0 / 691.2 / 997.6 | 376.5 / 684.1 / 1,131.9 / 1,634.4 |
| Siege coil Mk3 | 3 / 4 | 1,000 / 1,500 | 1,260.8 / 1,915.6 | 2,057.9 / 3,126.8 |
| IR Laser | 1 / 2 / 3 / 4 | 300 / 500 / 700 / 900 | 6.7 / 10.0 / 11.7 / 13.3 | 26.7 / 40.0 / 46.7 / 53.3 |
| IR ArcLaser | 1 / 2 / 3 / 4 | 235 / 395 / 555 / 715 | 10.0 / 15.0 / 17.5 / 20.0 | 28.6 / 42.9 / 50.0 / 57.1 |
| IR Phaser | 1 / 2 / 3 / 4 | 150 / 250 / 350 / 450 | 20.0 / 30.0 / 35.0 / 40.0 | 44.4 / 66.7 / 77.8 / 88.9 |
| Green Laser | 1 / 2 / 3 / 4 | 300 / 500 / 700 / 900 | 6.7 / 10.0 / 11.7 / 13.3 | 33.3 / 50.0 / 58.3 / 66.7 |
| Green ArcLaser | 1 / 2 / 3 / 4 | 235 / 395 / 555 / 715 | 10.0 / 15.0 / 17.5 / 20.0 | 33.3 / 50.0 / 58.3 / 66.7 |
| Green Phaser | 1 / 2 / 3 / 4 | 235 / 395 / 555 / 715 | 20.0 / 30.0 / 35.0 / 40.0 | 50.0 / 75.0 / 87.5 / 100.0 |
| UV Laser | 1 / 2 / 3 / 4 | 300 / 500 / 700 / 900 | 6.7 / 10.0 / 11.7 / 13.3 | 66.7 / 100.0 / 116.7 / 133.3 |
| UV ArcLaser | 1 / 2 / 3 / 4 | 235 / 395 / 555 / 715 | 10.0 / 15.0 / 17.5 / 20.0 | 50.0 / 75.0 / 87.5 / 100.0 |
| UV Phaser | 1 / 2 / 3 / 4 | 235 / 395 / 555 / 715 | 20.0 / 30.0 / 35.0 / 40.0 | 66.7 / 100.0 / 116.7 / 133.3 |
| Alien magnetic | 1 / 2 / 3 / 4 | 50 / 100 / 150 / 200 | 151.2 / 329.1 / 472.0 / 675.3 | 293.0 / 645.3 / 921.8 / 1,324.2 |
| AdvancedAlien magnetic | 1 / 2 / 3 / 4 | 50 / 100 / 150 / 200 | 477.2 / 1,004.5 / 1,602.6 / 2,313.4 | 706.9 / 1,488.1 / 2,374.2 / 3,427.3 |
| Gen3Alien magnetic | 1 / 2 / 3 / 4 | 50 / 100 / 150 / 200 | 2,082.9 / 5,613.1 / 11,316.3 / 22,885.2 | 2,848.6 / 7,594.2 / 15,310.2 / 30,962.3 |
| Alien OrangeLaser | 1 / 2 / 3 / 4 | 246 / 416 / 587 / 758 | 8.0 / 10.7 / 16.0 / 18.7 | 22.9 / 30.5 / 45.7 / 53.3 |
| Alien VioletLaser | 1 / 2 / 3 / 4 | 246 / 416 / 587 / 758 | 10.7 / 14.2 / 21.3 / 24.9 | 21.3 / 28.4 / 42.7 / 49.8 |
| Alien Xaser | 1 / 2 / 3 / 4 | 333 / 547 / 760 / 973 | 8.0 / 10.7 / 16.0 / 18.7 | 20.0 / 26.7 / 40.0 / 46.7 |
| Alien Graser | 3 / 4 | 1,400 / 1,827 | 10.7 / 12.4 | 35.6 / 41.5 |
| Electron | 2 / 3 / 4 | 30 / 60 / 120 | 6.2 / 12.3 / 24.6 | 41.0 / 82.1 / 164.1 |
| Ion | 1 / 2 / 3 / 4 | 15 / 30 / 60 / 120 | 3.1 / 6.2 / 12.3 / 24.6 | 20.5 / 41.0 / 82.1 / 164.1 |
| Neutral particle | 1 / 2 / 3 / 4 | 20 / 40 / 80 / 160 | 4.1 / 8.3 / 16.6 / 33.1 | 27.6 / 55.2 / 110.3 / 220.7 |
| Antimatter particle | 2 / 3 / 4 | 40 / 240 / 480 | 9.2 / 18.5 / 36.9 | 92.3 / 184.6 / 369.2 |
| Alien particle | 1 / 2 / 3 / 4 | 150 / 80 / 400 / 800 | 13.1 / 19.6 / 29.5 / 44.2 | 65.5 / 98.2 / 147.3 / 220.9 |
| Plasma Mk1 | 3 / 4 | 540 / 720 | 8.9 / 9.2 | 202.4 / 295.9 |
| Plasma Mk2 | 3 / 4 | 480 / 640 | 10.7 / 11.0 | 173.5 / 236.8 |
| Plasma Mk3 | 3 / 4 | 420 / 560 | 13.4 / 13.8 | 168.7 / 222.0 |
| Alien plasma | 3 / 4 | 390 / 520 | 19.3 / 19.8 | 181.8 / 237.1 |
| Naval gun | 1 / 2 | 125 / 225 | 24.1 / 25.1 | 0.3 / 0.0 |

Coverage: 120 weapons across 35 size families. The remaining five nose templates
are 35mm/40mm half-nose autocannons, AlienMiniLightMagCannon (half-nose),
SpinalNeutronLance and AlienRelativisticParticleCannon. Half-nose fighter mounts
are outside this size-1–4 normalization; the two unique spinal weapons lack a
within-family comparator. All five remain in the source snapshot.

## Anchors and correction method

For a matched hull reference with weight u, divide each reference metric by u.
Multiply that unit mass, raw DPS and mean electrical input by A or B's nose
weight. Some families begin at a two-cell hull battery, so u=2 rather than
inventing a nonexistent small weapon. Each technology mark uses its own anchor.

Where no role-matched hull weapon exists, infer a local reference from the
smallest nose in that family, and only correct subsequent size ratios. A and B
then show the same provisional values; an absolute x2-versus-x3 comparison
would require a separately chosen hull-equivalent reference. The laser families
retain all current metrics in both variants.

| Family | Reference | Reference weight | A/B treatment |
| --- | --- | --- | --- |
| Rail Mk1 | LightRailgunBatteryMk1 | 1 | Hull-relative A/B |
| Coil Mk1 | LightCoilgunBatteryMk1 | 1 | Hull-relative A/B |
| Siege coil Mk1 | HeavySiegeCoilerMk1 | 8 | Within-family doubling only; no matched hull anchor |
| Rail Mk2 | LightRailgunBatteryMk2 | 1 | Hull-relative A/B |
| Coil Mk2 | LightCoilgunBatteryMk2 | 1 | Hull-relative A/B |
| Siege coil Mk2 | HeavySiegeCoilerMk2 | 8 | Within-family doubling only; no matched hull anchor |
| Rail Mk3 | LightRailgunBatteryMk3 | 1 | Hull-relative A/B |
| Coil Mk3 | LightCoilgunBatteryMk3 | 1 | Hull-relative A/B |
| Siege coil Mk3 | HeavySiegeCoilerMk3 | 8 | Within-family doubling only; no matched hull anchor |
| IR Laser | 60cmIRLaserBattery | 1 | Preserve optical family |
| IR ArcLaser | 60cmIRArcLaserBattery | 1 | Preserve optical family |
| IR Phaser | 60cmIRPhaserBattery | 1 | Preserve optical family |
| Green Laser | 60cmGreenLaserBattery | 1 | Preserve optical family |
| Green ArcLaser | 60cmGreenArcLaserBattery | 1 | Preserve optical family |
| Green Phaser | 60cmGreenPhaserBattery | 1 | Preserve optical family |
| UV Laser | 60cmUVLaserBattery | 1 | Preserve optical family |
| UV ArcLaser | 60cmUVArcLaserBattery | 1 | Preserve optical family |
| UV Phaser | 60cmUVPhaserBattery | 1 | Preserve optical family |
| Alien magnetic | AlienLightMagBattery | 1 | Hull-relative A/B |
| AdvancedAlien magnetic | AdvancedAlienLightMagBattery | 1 | Hull-relative A/B |
| Gen3Alien magnetic | Gen3AlienLightMagBattery | 1 | Hull-relative A/B |
| Alien OrangeLaser | Alien64cmOrangeLaserBattery | 1 | Preserve optical family |
| Alien VioletLaser | Alien64cmVioletLaserBattery | 1 | Preserve optical family |
| Alien Xaser | Alien256cmXaserCannon | 2 | Preserve optical family |
| Alien Graser | Alien768cmGraserCannon | 8 | Preserve optical family |
| Electron | LightE-BeamBattery | 1 | Hull-relative A/B |
| Ion | LightIonBattery | 1 | Hull-relative A/B |
| Neutral particle | ParticleBeamBattery | 2 | Hull-relative A/B |
| Antimatter particle | AntimatterParticleCannon | 4 | Within-family doubling only; no matched hull anchor |
| Alien particle | AlienLightParticleCannon | 2 | Within-family doubling only; no matched hull anchor |
| Plasma Mk1 | PlasmaBatteryMk1 | 2 | Hull-relative A/B |
| Plasma Mk2 | PlasmaBatteryMk2 | 2 | Hull-relative A/B |
| Plasma Mk3 | PlasmaBatteryMk3 | 2 | Hull-relative A/B |
| Alien plasma | AlienPlasmaBattery | 2 | Hull-relative A/B |
| Naval gun | 6-inchCannon | 1 | Hull-relative A/B |

## Numerical screening targets

Each cell is current → A → B. These are unrounded target metrics; final authored
fields should be sensibly rounded and then recalculated. Mounting mass targets
exclude ammunition. Laser rows are omitted because they remain unchanged.

| Weapon | Size | Mounting mass, t C → A → B | Raw DPS, MW C → A → B | Mean electrical draw, MW C → A → B |
| --- | --- | --- | --- | --- |
| LightRailCannonMk1 | 1 | 60.00 → 100.00 → 150.00 | 11.39 → 11.81 → 17.72 | 60.75 → 63.00 → 94.50 |
| RailCannonMk1 | 2 | 120.00 → 200.00 → 300.00 | 9.33 → 23.62 → 35.44 | 49.77 → 126.00 → 189.00 |
| HeavyRailCannonMk1 | 3 | 180.00 → 400.00 → 600.00 | 16.92 → 47.25 → 70.88 | 90.25 → 252.00 → 378.00 |
| SpinalRailgunMk1 | 4 | 240.00 → 800.00 → 1,200.00 | 26.83 → 94.50 → 141.75 | 143.11 → 504.00 → 756.00 |
| LightCoilCannonMk1 | 1 | 60.00 → 100.00 → 150.00 | 36.26 → 26.13 → 39.20 | 97.73 → 67.95 → 101.92 |
| CoilCannonMk1 | 2 | 120.00 → 200.00 → 300.00 | 88.00 → 52.27 → 78.40 | 235.91 → 135.89 → 203.84 |
| HeavyCoilCannonMk1 | 3 | 180.00 → 400.00 → 600.00 | 153.89 → 104.53 → 156.80 | 407.49 → 271.79 → 407.68 |
| SpinalCoilerMk1 | 4 | 240.00 → 800.00 → 1,200.00 | 228.42 → 209.07 → 313.60 | 607.50 → 543.57 → 815.36 |
| HeavySiegeCoilerMk1 | 3 | 1,000.00 → 1,000.00 → 1,000.00 | 287.14 → 287.14 → 287.14 | 765.16 → 765.16 → 765.16 |
| SpinalSiegeCoilerMk1 | 4 | 1,500.00 → 2,000.00 → 2,000.00 | 419.00 → 574.28 → 574.28 | 1,116.73 → 1,530.32 → 1,530.32 |
| LightRailCannonMk2 | 1 | 55.00 → 90.00 → 135.00 | 23.33 → 20.33 → 30.49 | 97.20 → 84.70 → 127.05 |
| RailCannonMk2 | 2 | 110.00 → 180.00 → 270.00 | 21.47 → 40.66 → 60.98 | 89.44 → 169.40 → 254.10 |
| HeavyRailCannonMk2 | 3 | 165.00 → 360.00 → 540.00 | 40.79 → 81.31 → 121.97 | 169.94 → 338.80 → 508.20 |
| SpinalRailgunMk2 | 4 | 220.00 → 720.00 → 1,080.00 | 67.18 → 162.62 → 243.94 | 279.94 → 677.60 → 1,016.40 |
| LightCoilCannonMk2 | 1 | 55.00 → 90.00 → 135.00 | 84.34 → 61.65 → 92.48 | 174.30 → 133.58 → 200.37 |
| CoilCannonMk2 | 2 | 110.00 → 180.00 → 270.00 | 176.09 → 123.31 → 184.96 | 369.78 → 267.16 → 400.75 |
| HeavyCoilCannonMk2 | 3 | 165.00 → 360.00 → 540.00 | 300.52 → 246.61 → 369.92 | 627.76 → 534.33 → 801.49 |
| SpinalCoilerMk2 | 4 | 220.00 → 720.00 → 1,080.00 | 443.52 → 493.23 → 739.84 | 924.00 → 1,068.66 → 1,602.99 |
| HeavySiegeCoilerMk2 | 3 | 1,000.00 → 1,000.00 → 1,000.00 | 528.98 → 528.98 → 528.98 | 1,102.63 → 1,102.63 → 1,102.63 |
| SpinalSiegeCoilerMk2 | 4 | 1,500.00 → 2,000.00 → 2,000.00 | 796.22 → 1,057.96 → 1,057.96 | 1,658.80 → 2,205.26 → 2,205.26 |
| LightRailCannonMk3 | 1 | 50.00 → 80.00 → 120.00 | 55.03 → 39.69 → 59.54 | 179.68 → 129.60 → 194.40 |
| RailCannonMk3 | 2 | 100.00 → 160.00 → 240.00 | 73.48 → 79.38 → 119.07 | 239.95 → 259.20 → 388.80 |
| HeavyRailCannonMk3 | 3 | 150.00 → 320.00 → 480.00 | 133.26 → 158.76 → 238.14 | 435.13 → 518.40 → 777.60 |
| SpinalRailgunMk3 | 4 | 200.00 → 640.00 → 960.00 | 211.31 → 317.52 → 476.28 | 689.98 → 1,036.80 → 1,555.20 |
| LightCoilCannonMk3 | 1 | 50.00 → 80.00 → 120.00 | 229.52 → 130.02 → 195.03 | 376.47 → 241.47 → 362.20 |
| CoilCannonMk3 | 2 | 100.00 → 160.00 → 240.00 | 418.03 → 260.04 → 390.06 | 684.05 → 482.93 → 724.40 |
| HeavyCoilCannonMk3 | 3 | 150.00 → 320.00 → 480.00 | 691.19 → 520.08 → 780.12 | 1,131.92 → 965.87 → 1,448.80 |
| SpinalCoilerMk3 | 4 | 200.00 → 640.00 → 960.00 | 997.61 → 1,040.17 → 1,560.25 | 1,634.35 → 1,931.74 → 2,897.61 |
| HeavySiegeCoilerMk3 | 3 | 1,000.00 → 1,000.00 → 1,000.00 | 1,260.84 → 1,260.84 → 1,260.84 | 2,057.89 → 2,057.89 → 2,057.89 |
| SpinalSiegeCoilerMk3 | 4 | 1,500.00 → 2,000.00 → 2,000.00 | 1,915.63 → 2,521.68 → 2,521.68 | 3,126.84 → 4,115.77 → 4,115.77 |
| AlienLightMagCannon | 1 | 50.00 → 80.00 → 120.00 | 151.17 → 141.12 → 211.68 | 292.97 → 279.30 → 418.95 |
| AlienMagCannon | 2 | 100.00 → 160.00 → 240.00 | 329.12 → 282.24 → 423.36 | 645.33 → 558.60 → 837.90 |
| AlienHeavyMagCannon | 3 | 150.00 → 320.00 → 480.00 | 471.97 → 564.48 → 846.72 | 921.82 → 1,117.20 → 1,675.80 |
| AlienSpinalMagCannon | 4 | 200.00 → 640.00 → 960.00 | 675.32 → 1,128.96 → 1,693.44 | 1,324.15 → 2,234.40 → 3,351.60 |
| AdvancedAlienLightMagCannon | 1 | 50.00 → 80.00 → 120.00 | 477.16 → 374.85 → 562.27 | 706.91 → 558.60 → 837.90 |
| AdvancedAlienMagCannon | 2 | 100.00 → 160.00 → 240.00 | 1,004.46 → 749.70 → 1,124.55 | 1,488.10 → 1,117.20 → 1,675.80 |
| AdvancedAlienHeavyMagCannon | 3 | 150.00 → 320.00 → 480.00 | 1,602.61 → 1,499.40 → 2,249.10 | 2,374.24 → 2,234.40 → 3,351.60 |
| AdvancedAlienSpinalMagCannon | 4 | 200.00 → 640.00 → 960.00 | 2,313.43 → 2,998.80 → 4,498.20 | 3,427.30 → 4,468.80 → 6,703.20 |
| Gen3AlienLightMagCannon | 1 | 50.00 → 80.00 → 120.00 | 2,082.86 → 1,005.06 → 1,507.59 | 2,848.61 → 1,379.50 → 2,069.25 |
| Gen3AlienMagCannon | 2 | 100.00 → 160.00 → 240.00 | 5,613.08 → 2,010.13 → 3,015.19 | 7,594.16 → 2,759.00 → 4,138.50 |
| Gen3AlienHeavyMagCannon | 3 | 150.00 → 320.00 → 480.00 | 11,316.27 → 4,020.25 → 6,030.38 | 15,310.24 → 5,517.99 → 8,276.99 |
| Gen3AlienSpinalMagCannon | 4 | 200.00 → 640.00 → 960.00 | 22,885.21 → 8,040.51 → 12,060.76 | 30,962.34 → 11,035.99 → 16,553.98 |
| ElectronLance | 2 | 30.00 → 40.00 → 60.00 | 6.15 → 8.21 → 12.31 | 41.03 → 54.70 → 82.05 |
| HeavyElectronLance | 3 | 60.00 → 80.00 → 120.00 | 12.31 → 16.41 → 24.62 | 82.05 → 109.40 → 164.10 |
| SpinalElectronLance | 4 | 120.00 → 160.00 → 240.00 | 24.62 → 32.82 → 49.23 | 164.10 → 218.80 → 328.21 |
| LightIonCannon | 1 | 15.00 → 20.00 → 30.00 | 3.08 → 4.10 → 6.15 | 20.51 → 27.35 → 41.03 |
| IonCannon | 2 | 30.00 → 40.00 → 60.00 | 6.15 → 8.21 → 12.31 | 41.03 → 54.70 → 82.05 |
| HeavyIonCannon | 3 | 60.00 → 80.00 → 120.00 | 12.31 → 16.41 → 24.62 | 82.05 → 109.40 → 164.10 |
| SpinalIonCannon | 4 | 120.00 → 160.00 → 240.00 | 24.62 → 32.82 → 49.23 | 164.10 → 218.80 → 328.21 |
| LightParticleLance | 1 | 20.00 → 25.00 → 37.50 | 4.14 → 5.52 → 8.28 | 27.59 → 36.78 → 55.17 |
| ParticleLance | 2 | 40.00 → 50.00 → 75.00 | 8.28 → 11.03 → 16.55 | 55.17 → 73.56 → 110.34 |
| HeavyParticleLance | 3 | 80.00 → 100.00 → 150.00 | 16.55 → 22.07 → 33.10 | 110.34 → 147.13 → 220.69 |
| SpinalParticleLance | 4 | 160.00 → 200.00 → 300.00 | 33.10 → 44.14 → 66.21 | 220.69 → 294.25 → 441.38 |
| AntimatterParticleCannon | 2 | 40.00 → 40.00 → 40.00 | 9.23 → 9.23 → 9.23 | 92.31 → 92.31 → 92.31 |
| HeavyAntimatterParticleCannon | 3 | 240.00 → 80.00 → 80.00 | 18.46 → 18.46 → 18.46 | 184.62 → 184.62 → 184.62 |
| SpinalAntimatterParticleCannon | 4 | 480.00 → 160.00 → 160.00 | 36.92 → 36.92 → 36.92 | 369.23 → 369.23 → 369.23 |
| AlienLightParticleCannon | 1 | 150.00 → 150.00 → 150.00 | 13.09 → 13.09 → 13.09 | 65.45 → 65.45 → 65.45 |
| AlienParticleCannon | 2 | 80.00 → 300.00 → 300.00 | 19.64 → 26.18 → 26.18 | 98.18 → 130.91 → 130.91 |
| AlienHeavyParticleCannon | 3 | 400.00 → 600.00 → 600.00 | 29.45 → 52.36 → 52.36 | 147.27 → 261.82 → 261.82 |
| AlienSpinalParticleCannon | 4 | 800.00 → 1,200.00 → 1,200.00 | 44.18 → 104.73 → 104.73 | 220.91 → 523.64 → 523.64 |
| PlasmaCannonMk1 | 3 | 540.00 → 1,280.00 → 1,920.00 | 8.93 → 18.00 → 27.00 | 202.40 → 504.44 → 756.67 |
| HeavyPlasmaCannonMk1 | 4 | 720.00 → 2,560.00 → 3,840.00 | 9.19 → 36.00 → 54.00 | 295.94 → 1,008.89 → 1,513.33 |
| PlasmaCannonMk2 | 3 | 480.00 → 1,120.00 → 1,680.00 | 10.72 → 21.60 → 32.40 | 173.48 → 454.00 → 681.00 |
| HeavyPlasmaCannonMk2 | 4 | 640.00 → 2,240.00 → 3,360.00 | 11.03 → 43.20 → 64.80 | 236.75 → 908.00 → 1,362.00 |
| PlasmaCannonMk3 | 3 | 420.00 → 960.00 → 1,440.00 | 13.40 → 27.00 → 40.50 | 168.66 → 454.00 → 681.00 |
| HeavyPlasmaCannonMk3 | 4 | 560.00 → 1,920.00 → 2,880.00 | 13.78 → 54.00 → 81.00 | 221.95 → 908.00 → 1,362.00 |
| AlienPlasmaCannon | 3 | 390.00 → 800.00 → 1,200.00 | 19.29 → 52.92 → 79.38 | 181.76 → 505.84 → 758.76 |
| AlienHeavyPlasmaCannon | 4 | 520.00 → 1,600.00 → 2,400.00 | 19.85 → 105.84 → 158.76 | 237.11 → 1,011.68 → 1,517.52 |
| 10-inchCannon | 1 | 125.00 → 50.00 → 75.00 | 24.05 → 17.42 → 26.13 | 0.30 → 0.30 → 0.45 |
| 12-inchCannon | 2 | 225.00 → 100.00 → 150.00 | 25.09 → 34.84 → 52.27 | 0.00 → 0.60 → 0.90 |

For implementation, preserving cadence requires per-shot energy equal to target
raw throughput × average interval. For magnetics solve damaging mass from that
energy and existing muzzle velocity, and solve ammunition mass separately from
target electrical energy and efficiency; assert damaging mass ≤ projectile mass.
For beams, set shot energy and compatible efficiency explicitly if those two
targets require it. For plasma, solve charging energy after accounting for
projectile kinetic energy and efficiency; reject negative charging requirements.
Chemical guns receive a separately authored electrical input, never an electrical
bill equal to the projectile's chemically supplied energy.

If meeting an exact target requires implausible projectile mass, efficiency,
cooling, or burst power, tune the target within that family rather than quietly
breaking energy accounting. Ammo endurance, magazine size, manufacturing cost,
heat and full-system mass must be recomputed. A/B are comparisons to guide this
pass; they are not a substitute for complete ship-design and combat tests.

## Reference-fit firepower per mass and volume

Four stages: **C** current templates/layout; **M** the active capital proposal
with updated hull mass targets and unchanged weapon stats; **A/B** deferred
research only, adding the respective regular non-laser nose screening targets.
Lasers remain unchanged after M. M is the only planned implementation variant.

Fit rule: maximize weighted geometric capacity, breaking ties toward fewer
weapons (largest mounts), with a deterministic placement order. This represents
a heavy-mount doctrine, **not** the fit maximizing current raw DPS. For example,
one large laser can have lower emitted MW than several smaller lasers while
delivering much better useful damage through armor at distance.

Mass denominator = bare hull + installed weapon hardware + weapon crew at
3 t/billet. It excludes ammunition consistently in every stage, and excludes
armor, propulsion, reactors, radiators, utilities and fuel. Volume is the V0
main-hull envelope. The snapshot records every weapon in each reference fit and
its aggregate power demand. No power-feasibility or faction-budget claim is made.
Compare within one family; emitted particle/laser energy and kinetic energy
are not interchangeable predictions of actual combat damage.

| Hull / family | Raw DPS, MW C → M → A → B | DPS / 1,000 t reference C → M → A → B | DPS / 100,000 m³ C → M → A → B |
| --- | --- | --- | --- |
| Gunship / Green phaser | 20.0 → 20.0 → 20.0 → 20.0 | 46.8 → 46.8 → 46.8 → 46.8 | 181.5 → 181.5 → 181.5 → 181.5 |
| Gunship / Coil Mk3 | 229.5 → 229.5 → 130.0 → 195.0 | 948.4 → 948.4 → 478.0 → 625.1 | 2,082.8 → 2,082.8 → 1,179.9 → 1,769.8 |
| Escort / Green phaser | 20.0 → 20.0 → 20.0 → 20.0 | 33.8 → 33.8 → 33.8 → 33.8 | 298.4 → 298.4 → 298.4 → 298.4 |
| Escort / Coil Mk3 | 130.0 → 130.0 → 130.0 → 130.0 | 290.2 → 290.2 → 290.2 → 290.2 | 1,940.2 → 1,940.2 → 1,940.2 → 1,940.2 |
| Corvette / Green phaser | 30.0 → 30.0 → 30.0 → 30.0 | 39.1 → 39.1 → 39.1 → 39.1 | 591.6 → 591.6 → 591.6 → 591.6 |
| Corvette / Coil Mk3 | 294.5 → 294.5 → 195.0 → 260.0 | 576.4 → 576.4 → 360.5 → 447.6 | 5,808.4 → 5,808.4 → 3,846.2 → 5,128.2 |
| Frigate / Green phaser | 35.0 → 35.0 → 35.0 → 35.0 | 34.6 → 34.6 → 34.6 → 34.6 | 255.6 → 255.6 → 255.6 → 255.6 |
| Frigate / Coil Mk3 | 368.7 → 368.7 → 269.2 → 334.2 | 489.0 → 489.0 → 343.4 → 405.6 | 2,693.0 → 2,693.0 → 1,966.3 → 2,441.1 |
| Monitor / Green phaser | 30.0 → 30.0 → 30.0 → 30.0 | 29.2 → 29.2 → 29.2 → 29.2 | 174.8 → 174.8 → 174.8 → 174.8 |
| Monitor / Coil Mk3 | 278.4 → 278.4 → 278.4 → 278.4 | 315.0 → 315.0 → 315.0 → 315.0 | 1,622.6 → 1,622.6 → 1,622.6 → 1,622.6 |
| Destroyer / Green phaser | 45.0 → 45.0 → 45.0 → 45.0 | 30.5 → 30.5 → 30.5 → 30.5 | 204.0 → 204.0 → 204.0 → 204.0 |
| Destroyer / Coil Mk3 | 557.3 → 557.3 → 399.3 → 529.3 | 503.4 → 503.4 → 342.1 → 424.4 | 2,525.8 → 2,525.8 → 1,809.7 → 2,399.1 |
| Cruiser / Green phaser | 55.0 → 55.0 → 55.0 → 55.0 | 32.4 → 32.4 → 32.4 → 32.4 | 120.0 → 120.0 → 120.0 → 120.0 |
| Cruiser / Coil Mk3 | 622.3 → 622.3 → 464.3 → 594.3 | 495.4 → 495.4 → 352.8 → 425.7 | 1,358.1 → 1,358.1 → 1,013.3 → 1,297.0 |
| Battlecruiser / Green phaser | 50.0 → 50.0 → 50.0 → 50.0 | 25.8 → 25.8 → 25.8 → 25.8 | 114.5 → 114.5 → 114.5 → 114.5 |
| Battlecruiser / Coil Mk3 | 830.4 → 830.4 → 659.3 → 919.3 | 568.8 → 568.8 → 404.5 → 513.6 | 1,900.9 → 1,900.9 → 1,509.2 → 2,104.5 |
| Lancer / Green phaser | 65.0 → 65.0 → 65.0 → 65.0 | 21.5 → 21.5 → 21.5 → 21.5 | 42.6 → 42.6 → 42.6 → 42.6 |
| Lancer / Coil Mk3 | 1,201.8 → 1,201.8 → 1,244.4 → 1,764.5 | 509.5 → 509.5 → 444.6 → 565.7 | 787.6 → 787.6 → 815.5 → 1,156.3 |
| Battleship / Green phaser | 75.0 → 75.0 → 75.0 → 75.0 | 29.1 → 29.1 → 29.1 → 29.1 | 71.3 → 71.3 → 71.3 → 71.3 |
| Battleship / Coil Mk3 | 898.7 → 898.7 → 740.7 → 870.7 | 452.1 → 452.1 → 361.7 → 409.2 | 854.8 → 854.8 → 704.5 → 828.2 |
| Dreadnought / Green phaser | 85.0 → 85.0 → 85.0 → 85.0 | 23.4 → 23.4 → 23.4 → 23.4 | 31.2 → 31.2 → 31.2 → 31.2 |
| Dreadnought / Coil Mk3 | 1,392.5 → 1,392.5 → 1,221.4 → 1,481.4 | 477.2 → 477.2 → 395.5 → 456.1 | 511.5 → 511.5 → 448.7 → 544.2 |
| Titan / Green phaser | 85.0 → 190.0 → 190.0 → 190.0 | 18.9 → 31.1 → 31.1 → 31.1 | 16.5 → 36.9 → 36.9 → 36.9 |
| Titan / Coil Mk3 | 1,478.3 → 3,101.6 → 3,144.1 → 3,664.2 | 400.5 → 664.4 → 615.5 → 675.1 | 287.3 → 602.8 → 611.0 → 712.1 |
| AlienDreadnought / Advanced alien magnetic | 4,134.3 → 4,134.3 → 4,819.6 → 6,319.0 | 1,523.9 → 1,505.0 → 1,512.3 → 1,801.8 | 1,364.5 → 1,364.5 → 1,590.7 → 2,085.6 |
| AlienTitan / Advanced alien magnetic | 5,088.6 → 10,551.1 → 11,031.8 → 12,906.1 | 1,622.1 → 2,472.7 → 2,314.2 → 2,497.8 | 1,453.9 → 3,014.7 → 3,152.0 → 3,687.6 |
| AlienMothership / Advanced alien magnetic | 5,955.1 → 16,880.1 → 17,565.4 → 19,064.8 | 671.8 → 1,479.0 → 1,481.9 → 1,566.2 | 12.1 → 34.3 → 35.7 → 38.8 |

## Data and validation

[Source hashes, merged templates, per-weapon metrics, A/B targets and exact
reference fits](tables/capital-heavy-mount-proposal-2026-09-25.json).
The current native mean-cooldown and electrical-energy formulas were traced in
local decompiled game code; current repository power patches were checked for
overrides. Arithmetic checks cover geometry, non-overlap, alien hull invariants,
human rounded mass reconciliation, A/B target ratios, and reproduced fit sums.
No runtime files were changed and no build or deployment was run.
