# Alien magnetic weapons, propulsion, and armor-design proposal

Status: alien magnetic values implemented in Economic Equalization Overhaul
0.9.1. A conservative propulsion, reactor, and armor-allocation slice is
implemented in 0.9.2; lower delta-v targets and the stronger endpoint proposal
remain deferred.

Last reviewed: 2026-08-12

## Scope and baseline

This report layers the current Economic Equalization Overhaul magnetic-gun
overrides over the installed Terra Invicta templates, then compares the
resulting human railgun and coilgun progression with the three alien magnetic
families. It also inspects the installed alien drive, reactor, predefined-ship,
and runtime ship-designer data.

The report has three parts:

1. make alien tier 1 magnetic weapons unambiguously better than human Mk1
   railguns and coilguns on the requested projectile-quality axes;
2. make alien tier 2 better than human Mk3 and make alien tier 3 read as the
   next notional human tier; and
3. establish matched alien drive/reactor generations whose final tier is
   slightly better than the last human fusion generation.

The magnetic portion is implemented. Version 0.9.2 takes a conservative first
step on propulsion and armor allocation. Lower alien AI delta-v targets are
**explicitly deferred**: the current designer targets remain in force during
the propulsion-first test.

The earlier [alien fleet and module audit](details/aliens/fleet-and-module-audit.md)
remains the inventory of predefined ships and equipment. This report
supersedes its recommendation to defer all numerical alien changes, but does
not supersede its evidence about loadout and power-plant mismatches.

## Magnetic-weapon findings

The current modded magnetic catalog is closer to the requested hierarchy than
the older vanilla audit implied:

- Alien tier 1 already exceeds the best human Mk1 projectile mass,
  durability, velocity, and range at every comparable mount. Several velocity
  margins were only `0.1 km/s`, so version 0.9.1 gives them a clearer lead.
- Alien tier 2 projectile mass and durability already exceed human Mk3. Its
  targeting ranges merely equal Mk3, while the light battery and light cannon
  do not yet have the desired velocity lead.
- Alien tier 3 projectile mass and impact energy already exceed a reasonable
  Mk4 extrapolation. The inconsistent fields are range and efficiency, not
  projectile mass.
- Cooldown, magazine, crew, empty weapon mass, and projectile mass remain
  unchanged. Requiring every alien weapon to beat the fastest human reload as
  well as firing a much heavier projectile would multiply sustained damage
  much more than the stated tier relationship requires.

`Projectile / durability` below is complete projectile mass followed by the
damaging mass used by the current projectile-durability model. Neither value
changes in version 0.9.1. Impact-energy delta follows the square of the
velocity change. Input-energy delta also includes the efficiency change; a
negative value means the same impact costs less reactor energy per shot.

### Alien tier 1 versus human Mk1

| Weapon | Projectile / durability | Velocity, km/s | Delta | Range, km | Delta | Efficiency | Impact energy delta | Input energy delta |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Light Mag Battery | 19/16 kg | 4.6 -> 5.0 | +8.7% | 550 -> 550 | - | 60% -> 60% | +18.1% | +18.1% |
| Mag Battery | 38/32 kg | 5.5 -> 6.0 | +9.1% | 700 -> 700 | - | 60% -> 60% | +19.0% | +19.0% |
| Heavy Mag Battery | 75/64 kg | 6.4 -> 7.0 | +9.4% | 850 -> 850 | - | 60% -> 60% | +19.6% | +19.6% |
| Mini Light Mag Cannon | 50/43 kg | 5.5 -> 6.0 | +9.1% | 600 -> 600 | - | 60% -> 60% | +19.0% | +19.0% |
| Light Mag Cannon | 50/43 kg | 5.5 -> 6.0 | +9.1% | 600 -> 600 | - | 60% -> 60% | +19.0% | +19.0% |
| Mag Cannon | 100/85 kg | 6.7 -> 7.0 | +4.5% | 750 -> 750 | - | 60% -> 60% | +9.2% | +9.2% |
| Heavy Mag Cannon | 150/128 kg | 8.3 -> 8.3 | - | 850 -> 850 | - | 60% -> 60% | - | - |
| Spinal Mag Cannon | 200/170 kg | 10.0 -> 10.0 | - | 950 -> 950 | - | 60% -> 60% | - | - |

### Alien tier 2 versus human Mk3

| Weapon | Projectile / durability | Velocity, km/s | Delta | Range, km | Delta | Efficiency | Impact energy delta | Input energy delta |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Advanced Light Mag Battery | 19/17 kg | 6.2 -> 6.7 | +8.1% | 600 -> 650 | +8.3% | 70% -> 75% | +16.8% | +9.0% |
| Advanced Mag Battery | 38/34 kg | 7.8 -> 7.8 | - | 750 -> 800 | +6.7% | 70% -> 75% | - | -6.7% |
| Advanced Heavy Mag Battery | 75/68 kg | 9.4 -> 9.4 | - | 900 -> 950 | +5.6% | 70% -> 75% | - | -6.7% |
| Advanced Light Mag Cannon | 50/45 kg | 8.2 -> 8.6 | +4.9% | 650 -> 700 | +7.7% | 70% -> 75% | +10.0% | +2.7% |
| Advanced Mag Cannon | 100/90 kg | 10.0 -> 10.0 | - | 800 -> 850 | +6.3% | 70% -> 75% | - | -6.7% |
| Advanced Heavy Mag Cannon | 150/135 kg | 12.5 -> 12.5 | - | 900 -> 950 | +5.6% | 70% -> 75% | - | -6.7% |
| Advanced Spinal Mag Cannon | 200/180 kg | 15.0 -> 15.0 | - | 1,000 -> 1,050 | +5.0% | 70% -> 75% | - | -6.7% |

### Alien tier 3 as a notional human Mk4

| Weapon | Projectile / durability | Velocity, km/s | Delta | Range, km | Delta | Efficiency | Impact energy delta | Input energy delta |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Gen3 Light Mag Battery | 35/30 kg | 8.2 -> 8.2 | - | 650 -> 700 | +7.7% | 80% -> 85% | - | -5.9% |
| Gen3 Mag Battery | 69/60 kg | 10.3 -> 10.3 | - | 750 -> 850 | +13.3% | 80% -> 85% | - | -5.9% |
| Gen3 Heavy Mag Battery | 138/120 kg | 12.4 -> 12.4 | - | 900 -> 1,000 | +11.1% | 80% -> 85% | - | -5.9% |
| Gen3 Light Mag Cannon | 93/80 kg | 10.8 -> 10.8 | - | 800 -> 800 | - | 80% -> 85% | - | -5.9% |
| Gen3 Mag Cannon | 184/160 kg | 13.1 -> 13.1 | - | 900 -> 900 | - | 80% -> 85% | - | -5.9% |
| Gen3 Heavy Mag Cannon | 368/320 kg | 16.5 -> 16.5 | - | 1,000 -> 1,000 | - | 80% -> 85% | - | -5.9% |
| Gen3 Spinal Mag Cannon | 736/640 kg | 19.8 -> 19.8 | - | 1,000 -> 1,100 | +10.0% | 80% -> 85% | - | -5.9% |

The Gen3 spinal projectile is already a progression outlier at `736/640 kg`.
It receives no additional projectile or velocity increase.

Release validation locks all twenty-two alien magnetic rows and asserts that
every tier-1 mount strictly exceeds the corresponding human Mk1 rail and coil
maximum, and every tier-2 mount strictly exceeds the human Mk3 maximum, for
complete projectile mass, damaging mass/durability, velocity, targeting
range, and efficiency. The half-nose mini cannon is conservatively compared
against the larger one-nose human weapons because humans have no direct
half-nose magnetic equivalent.

## Conservative 0.9.2 implementation

The first implementation keeps the selected `1.2 / 3.8 / 10.5 MN` thrust
ladder but limits exhaust velocity to `1,200 / 2,350 / 3,000 km/s` and leaves
drive efficiency at the installed `95 / 97 / 98%`. Reactor output receives a
flat **+400%** increase (`new = current x 5`), specific mass is halved, and the
previously selected efficiency values are retained.

### Drives

| Tier | Thrust, current -> 0.9.2 | Delta | EV, current -> 0.9.2 | Delta | Efficiency | x1 electrical demand | x6 electrical demand |
|---|---:|---:|---:|---:|---:|---:|---:|
| Alien Fusion Lantern | 0.50 -> 1.20 MN | +140.0% | 633 -> 1,200 km/s | +89.6% | 95% unchanged | 757.9 GW | 4,547.4 GW |
| Alien Fusion Torch | 1.59 -> 3.80 MN | +139.0% | 1,300 -> 2,350 km/s | +80.8% | 97% unchanged | 4,603.1 GW | 27,618.6 GW |
| Advanced Alien Fusion Torch | 4.39 -> 10.50 MN | +139.2% | 1,600 -> 3,000 km/s | +87.5% | 98% unchanged | 16,071.4 GW | 96,428.6 GW |

### Reactors

| Tier | Output, current -> 0.9.2 | Delta | Specific mass, current -> 0.9.2 | Delta | Efficiency | Full-cap mass, current -> 0.9.2 |
|---|---:|---:|---:|---:|---:|---:|
| Alien Hybrid | 1,000 -> 5,000 GW | +400.0% | 1.0 -> 0.50 t/GW | -50.0% | 99.0% -> 99.5% | 1,000 -> 2,500 t |
| Alien Advanced Hybrid | 6,400 -> 32,000 GW | +400.0% | 0.35 -> 0.175 t/GW | -50.0% | 99.5% -> 99.8% | 2,240 -> 5,600 t |
| Alien Super-Advanced Hybrid | 21,510 -> 107,550 GW | +400.0% | 0.05 -> 0.025 t/GW | -50.0% | 99.8% -> 99.95% | 1,075.5 -> 2,688.75 t |

Each revised reactor cap exceeds six matching drives by approximately `9.9% /
15.9% / 11.5%`. This repairs the nominal matching-tier drive-cap issue and
gives the designer more output headroom, but it does not prove that every
predefined or dynamically generated ship selects the matching reactor.

### Minimal armor-allocation change

The installed designer converts its provisional armor score through an assumed
`3,500 kg/m3` density before applying the selected armor's real density. The
0.9.2 patch changes only that numerator to `10,000 kg/m3`:

```csharp
num3 = Mathf.RoundToInt(
    (float)num3 * 10000f / bestArmor.density_kgm3);
```

For a fixed armor material this requests about **2.86 times** the prior armor
points, not exactly twice. The result is still constrained by the rest of the
installed design method, and no delta-v target is lowered.

## Deferred high-end alien propulsion proposal

The human endpoint establishes two distinct requirements:

- Protium Converter Torch: `9.76 MN`, `10,256 km/s`, and `98%` efficient;
- Inertial Confinement Fusion Reactor VII: `306,430 GW`, `0.002 t/GW`,
  and `99.9%` efficient.

The final human reactor is itself an exceptional jump from Inertial VI. A
literal alien endpoint above it therefore requires a large change from the
current alien line even though the proposed endpoint is only slightly above
the human endpoint.

### Deferred drives

| Tier | Current thrust | Proposed | Delta | Current EV | Proposed | Delta | Efficiency | Proposed x1 power | Proposed x6 power |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Alien Fusion Lantern | 0.50 MN | 1.20 MN | +140.0% | 633 km/s | 1,200 km/s | +89.6% | 95% -> 98% | 735 GW | 4,408 GW |
| Alien Fusion Torch | 1.59 MN | 3.80 MN | +139.0% | 1,300 km/s | 3,600 km/s | +176.9% | 97% -> 99% | 6,909 GW | 41,455 GW |
| Advanced Alien Fusion Torch | 4.39 MN | 10.50 MN | +139.2% | 1,600 km/s | 10,800 km/s | +575.0% | 98% -> 99.9% | 56,757 GW | 340,541 GW |

### Deferred reactors

| Tier | Output, current -> proposed | Delta | Specific mass, current -> proposed | Delta | Efficiency | Full-cap mass, current -> proposed |
|---|---:|---:|---:|---:|---:|---:|
| Alien Hybrid | 1,000 -> 5,000 GW | +400.0% | 1.0 -> 0.10 t/GW | -90.0% | 99.0% -> 99.5% | 1,000 -> 500 t |
| Alien Advanced Hybrid | 6,400 -> 42,000 GW | +556.3% | 0.35 -> 0.014 t/GW | -96.0% | 99.5% -> 99.8% | 2,240 -> 588 t |
| Alien Super-Advanced Hybrid | 21,510 -> 350,000 GW | +1,527.0% | 0.05 -> 0.0018 t/GW | -96.4% | 99.8% -> 99.95% | 1,075.5 -> 630 t |

Each reactor cap is deliberately just above six matching drives. The resulting
full-cap plant masses stay in a narrow `500-630 t` band instead of becoming
lighter merely because the nominal output grows. At the proposed six-drive
loads, the plant masses are about `441 / 580 / 613 t` before other propulsion,
cooling, and ship-system burdens.

The Advanced Alien Fusion Torch currently uses hydrogen, and standard alien
utility selection can double hydrogen exhaust velocity. A `10,800 km/s` raw
drive would therefore become `21,600 km/s` when that bonus applies. Before
implementation, the advanced drive must either use reaction products or be
made ineligible for that multiplier. Otherwise the installed system would be
more than twice, rather than slightly, above the final human fusion drive.

### Closest human drive equivalents

For this comparison, "closest" means the human x1 fusion drive with the
smallest combined logarithmic distance in thrust and exhaust velocity. This
avoids choosing a nominally adjacent technology that is close in one field
but orders of magnitude away in the other. Efficiency is reported but is not
used to choose the peer.

| Alien proposed tier | Closest human fusion drive | Thrust, alien / human | Delta | EV, alien / human | Delta | Efficiency, alien / human | Delta | x1 power, alien / human | Delta |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Alien Fusion Lantern | Protium Torus Lantern | 1.20 / 1.68 MN | -28.6% | 1,200 / 952 km/s | +26.1% | 98% / 95% | +3.0 pp | 734.7 / 841.8 GW | -12.7% |
| Alien Fusion Torch | Protium Nova Torch | 3.80 / 6.60 MN | -42.4% | 3,600 / 1,000 km/s | +260.0% | 99% / 97% | +2.0 pp | 6,909.1 / 3,402.1 GW | +103.1% |
| Advanced Alien Fusion Torch | Protium Converter Torch | 10.50 / 9.76 MN | +7.6% | 10,800 / 10,256 km/s | +5.3% | 99.9% / 98% | +1.9 pp | 56,756.8 / 51,070.7 GW | +11.1% |

The middle alien tier occupies a genuine hole in the human catalog. Protium
Nova wins the distance calculation only narrowly over Protium Converter, so
it should not be read as a close one-for-one analogue.

### Closest human reactor equivalents

For reactors, "closest" means the human fusion reactor with the smallest
combined logarithmic distance in maximum output and specific mass. Lower
specific mass is better. Efficiency is again reported but not used to choose
the peer.

| Alien proposed tier | Closest human fusion reactor | Output, alien / human | Delta | Specific mass, alien / human | Delta | Efficiency, alien / human | Delta | Full-cap mass, alien / human | Delta |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Alien Hybrid | Fusion Tokamak V | 5,000 / 5,060 GW | -1.2% | 0.100 / 0.100 t/GW | - | 99.5% / 99.0% | +0.5 pp | 500 / 506 t | -1.2% |
| Alien Advanced Hybrid | Inertial Confinement VI | 42,000 / 20,420 GW | +105.7% | 0.014 / 0.068 t/GW | -79.4% | 99.8% / 99.0% | +0.8 pp | 588 / 1,388.6 t | -57.7% |
| Alien Super-Advanced Hybrid | Inertial Confinement VII | 350,000 / 306,430 GW | +14.2% | 0.0018 / 0.0020 t/GW | -10.0% | 99.95% / 99.9% | +0.05 pp | 630 / 612.9 t | +2.8% |

The middle alien reactor likewise bridges a wide human VI-to-VII discontinuity.
Its full-cap mass is lower than Inertial VI because its specific mass improves
much faster than its output rises.

## Will the propulsion changes repair reactor undersizing?

**Not by themselves.** There are two separate design paths.

### Predefined ships

Predefined alien ships store an explicit reactor template. Raising the three
reactor templates does not move a ship from Alien Hybrid to Alien Advanced or
Super-Advanced. A capital ship that retains the tier-1 reactor while receiving
the implemented tier-2 drive would become more severely underspecified: six
tier-2 drives require about `27,619 GW`, while the tier-1 cap is only
`5,000 GW`.

Predefined loadouts therefore need an explicit pairing correction or a
load-time normalizer:

- Lantern -> Alien Hybrid;
- Fusion Torch -> Alien Advanced Hybrid; and
- Advanced Fusion Torch -> Alien Super-Advanced Hybrid.

The normalizer should require reactor output to meet total drive and ship
load with a small validation margin. It should fail validation rather than
silently deploy an underpowered pairing.

### Dynamically designed ships

`DesignAlienShip` asks `GetBestPowerPlant` for a compatible reactor, but the
installed method filters by power-plant class and permitted build resources,
then selects by research score. It does not explicitly require
`maxOutput_GW >= design demand`. The existing mismatches demonstrate that
class compatibility is insufficient.

Dynamic design therefore also needs an output-cap eligibility check. If no
permitted reactor can power the selected drive count, the designer must reduce
the drive tier or count rather than return an underspecified ship.

## Will the propulsion changes create armor headroom?

**Physically, yes; algorithmically, not reliably without a small designer
correction.**

The implemented line creates headroom in two direct ways:

1. higher exhaust velocity needs much less propellant to reach the unchanged
   role delta-v target;
2. higher thrust supports more dry mass at the same acceleration.

The conservative reactor values repair matching-tier output caps but do not
create reactor-mass headroom. At six-drive electrical demand, plant mass is
approximately `2,274 / 4,833 / 2,411 t`, compared with roughly `999 / 2,237 /
1,075 t` for the installed vanilla pairs. The direct `10,000 kg/m3` armor
scalar is therefore the principal allocator change in this slice.

Those effects should permit substantially more armor without lowering the
existing `200-900 km/s` role targets. However, the current alien designer can
exit immediately when its starting configuration already meets delta-v and
acceleration. Higher exhaust velocity makes that early-success path more
likely. Its armor-filling loop can also stop after one facing reaches a cap,
even when other facings and performance margins remain available.

The propulsion slice should therefore preserve every current delta-v target
but make the armor allocator consume genuine surplus performance:

- always enter the armor-fill phase after a valid propulsion solution;
- retain the current delta-v, cruise-acceleration, and combat-acceleration
  floors;
- continue filling uncapped facings until all facings are capped or the next
  point would violate a performance floor; and
- record final armor mass, facing values, delta-v, acceleration, propellant
  tanks, reactor load, and reactor cap for validation.

This is not authorization to halve or otherwise reduce alien delta-v.

Automated 0.9.2 validation confirms all eighteen drive rows, three reactor
rows, the one-constant armor transpiler, matching-tier six-drive cap headroom,
and unchanged drive efficiencies. Generated designs still require manual
in-game testing because static checks cannot prove the designer reaches every
intended armor and performance outcome.

## Explicitly deferred delta-v changes

| Role group | Current designer target | Active proposal | Status |
|---|---:|---:|---|
| Councilor, surveillance, colony | 800 km/s | 800 km/s | unchanged |
| Carrier and long-range combat | 900 km/s | 900 km/s | unchanged |
| Standoff, superiority, strike | 600 km/s | 600 km/s | unchanged |
| Defender, patrol, interceptor | 200 km/s | 200 km/s | unchanged |
| Default | 300 km/s | 300 km/s | unchanged |

Any lower target, including the previously explored half-target concept,
belongs to a future update after the new propulsion and target-preserving
armor allocator have been tested in generated designs and campaign play.

## Required validation before implementation is settled

- Assert the tier-1 and tier-2 magnetic inequalities for all seven mount
  families after mod overrides are layered over the installed templates.
- Confirm all twenty-two alien magnetic rows retain the implemented projectile,
  velocity, range, efficiency, cooldown, and mass values.
- Assert that x1 through x6 of every alien drive fit the assigned reactor cap.
- Generate every alien role with and without exotic and antimatter builds;
  reject every drive/reactor mismatch.
- Compare generated armor and propulsion statistics before and after the
  propulsion change while leaving role delta-v targets identical.
- Test whether stronger propulsion actually reaches the armor-fill phase and
  whether all eligible facings continue filling.
- Run equal-cost magnetic, missile-saturation, and mixed-weapon skirmishes,
  followed by campaign construction and mobility testing.
