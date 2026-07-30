# Alien fleet, module, and ship-design audit

Last reviewed: 2026-07-29  
Game data: installed Terra Invicta 1.0.49 templates

For the composed interpretation and deferred status, see the
[alien research index](README.md).

## Scope

This is a static audit of:

- alien hulls in `TIShipHullTemplate.json`;
- predefined alien designs in `TISpaceShipTemplate.json`;
- the drives, reactors, radiators, heat sinks, batteries, utility modules, and
  weapons those designs reference.

It does not yet measure tactical AI behavior, formation logic, target
selection, retreat logic, or designs generated dynamically during a campaign.
Those can make a strong template perform poorly, or conceal a weak template
behind favorable behavior.

## Executive findings

1. The aliens have **15 hull types** and **17 predefined designs**, covering
   infiltration, interception, transport, line combat, assault-carrier, colony,
   and mothership roles.
2. Their module catalog contains genuinely superior systems, but many default
   designs use older alien weapons, the weaker alien battery, and the standard
   reactor.
3. **Nine predefined designs request more drive power than their reactor's
   `maxOutput_GW` ceiling.** The worst capital ships have only 15.6–31.3% of
   the drive's named power requirement available.
4. Base alien magnetic weapons are weaker in sustained impact output than the
   human Mk3 coilgun of the same mount size. Advanced and Gen3 alien magnetic
   weapons reverse that relationship, but they are used sparingly.
5. The strongest alien reactor and stronger alien battery are not used by any
   predefined design.
6. Capital-ship lateral and tail armor is often surprisingly thin. Equal
   numbers can therefore favor human fleets that bring concentrated,
   combat-optimized designs and exploit flank or rear exposure.
7. The design file contains two entries with the same `dataName`, `Ship35`,
   and one malformed trailing comma. Both are data-quality issues that should
   be resolved before using the file as a rebalance foundation.
8. Raw alien module buffs alone are unlikely to solve the observed problem.
   Power-plant pairing, tier selection, armor distribution, and tactical AI
   deserve priority.

## Alien hull catalog

Armor and installed modules belong to individual designs, not to the hull.

| Hull | Nose / hull hardpoints | Internal modules | Length × width | Hull mass | Crew | Integrity |
|---|---:|---:|---:|---:|---:|---:|
| Alien Gunship | 1 / 0 | 3 | 50 × 10 m | 192 t | 1 | 6 |
| Alien Escort | 0 / 2 | 4 | 75 × 10 m | 288 t | 2 | 10 |
| Alien Corvette | 1 / 1 | 4 | 75 × 10 m | 288 t | 3 | 10 |
| Alien Frigate | 1 / 3 | 5 | 125 × 15 m | 576 t | 10 | 20 |
| Alien Monitor | 1 / 4 | 5 | 150 × 15 m | 672 t | 16 | 22 |
| Alien Destroyer | 2 / 2 | 5 | 150 × 15 m | 672 t | 20 | 24 |
| Alien Cruiser | 2 / 4 | 7 | 200 × 20 m | 1,056 t | 25 | 36 |
| Alien Battlecruiser | 3 / 3 | 6 | 245 × 25 m | 1,440 t | 35 | 48 |
| Alien Battleship | 2 / 6 | 7 | 250 × 25 m | 1,536 t | 40 | 60 |
| Alien Lancer | 6 / 4 | 7 | 275 × 25 m | 1,536 t | 40 | 52 |
| Alien Dreadnought | 4 / 8 | 9 | 300 × 30 m | 2,016 t | 50 | 72 |
| Alien Titan | 6 / 8 | 7 | 400 × 30 m | 2,304 t | 60 | 90 |
| Alien Assault Carrier | 0 / 6 | 6 | 300 × 30 m | 2,688 t | 80 | 90 |
| Alien Mothership | 4 / 16 | 7 | 600 × 60 m | 7,680 t | 100 | 512 |
| Salamander Gunship | 1 / 1 | 1 | 35 × 8 m | 20 t | 1 | 3 |

The aliens do not merely use reskinned human hulls. Their frigate and larger
hulls are much longer, their Lancer emphasizes nose weapons, and their
Mothership has an exceptional 16 hull hardpoints and triple thruster
multiplier.

## Common alien power and thermal systems

### Reactors

| Reactor | Maximum output | Specific mass | Efficiency | Crew | Used by predefined designs |
|---|---:|---:|---:|---:|---:|
| Alien Hybrid Confinement Fusion | 1,000 GW | 1.00 t/GW | 99.0% | 4 | **Yes, most designs** |
| Alien Advanced Hybrid Confinement Fusion | 6,400 GW | 0.35 t/GW | 99.5% | 4 | Monitor, Destroyer, Cruiser, Mothership |
| Alien Super-Advanced Hybrid Confinement Fusion | 21,510 GW | 0.05 t/GW | 99.8% | 4 | **No** |

The super-advanced reactor is not an unreachable template placeholder: it is
assigned to `Project_AlienAdvancedMasterProject`, as are other modules that do
appear in predefined designs. Its non-use is therefore a design-progression
choice or omission.

These are not categorically beyond the human technology tree. Human
Hybrid-Confinement Fusion IV also reaches `0.05 t/GW`, while late human
inertial-fusion and antimatter plants go considerably lighter and higher in
output. If the intended lore is “aliens remain a tier beyond the human
research ceiling,” the numeric hierarchy does not currently enforce it.

### Drives

| Drive, x1 basis | Thrust | Exhaust velocity | Efficiency | Gross required power | Cooling |
|---|---:|---:|---:|---:|---|
| Alien Fusion Lantern | 0.50 MN | 633 km/s | 95% | 166.6 GW | closed |
| Alien Fusion Torch | 1.59 MN | 1,300 km/s | 97% | 1,065.5 GW | closed |
| Super Kronos chemical | 2.50 MN | 21.6 km/s | 100% | self-powered | open |

The alien torch is powerful relative to most of the campaign, but it is well
below the top of the human research tree. Human Pion and Protium Converter
torches reach much higher thrust and `10,256–14,720 km/s` exhaust velocity.
The game's late human technology can therefore surpass the alien baseline by
design.

### Thermal storage and radiators

| Module | Mass | Capacity / temperature | Crew | Predefined use |
|---|---:|---:|---:|---|
| Alien Lithium Heat Sink | 256 t | 1,050 GJ | 0 | Almost universal |
| Alien Exotic Heat Sink | 500 t | 3,600 GJ | 0 | Dreadnought and Mothership |
| Diamondoid Spike | power-scaled | 1,650 K, 15 kW/kg | 0 | Fighter and colony ship |
| Exotic Tendril | power-scaled | 2,500 K, 25 kW/kg | 0 | Main combat fleet |

The standard heat sink stores `4.10 GJ/t`; the exotic sink stores `7.20 GJ/t`.
The exotic system is much better per tonne but is used only on two designs.

### Battery

| Battery | Mass | Energy | Recharge | Crew | Predefined use |
|---|---:|---:|---:|---:|---|
| Alien Superconducting Coil | 18 t | 128 GJ | 0.075 GJ/s | 1 | Most combat designs |
| Alien Exotic Nanowire | 17 t | 256 GJ | 0.100 GJ/s | 1 | **None** |

The unused battery has twice the capacity, faster recharge, one tonne less
mass, and more hit points. Every predefined combat design nevertheless uses
the weaker unit.

## Reactor and drive compatibility

Prior runtime inspection established that `maxOutput_GW` is a plant capacity
ceiling, while installed plant mass is based on the ship's actual gross power
requirement. Comparing each predefined drive's `req power` with its paired
reactor gives:

| Design | Drive | Required power | Reactor cap | Cap / request | Result |
|---|---|---:|---:|---:|---|
| Silent Dawn Infiltrator | Torch x2 | 2,130.9 GW | 1,000 GW | 46.9% | **over cap** |
| Comet Escort | Lantern x4 | 666.3 GW | 1,000 GW | 150.1% | valid |
| Pulsar Corvette | Torch x1 | 1,065.5 GW | 1,000 GW | 93.9% | **over cap** |
| generic Alien Escort | Lantern x4 | 666.3 GW | 1,000 GW | 150.1% | valid |
| Nova Frigate | Torch x2 | 2,130.9 GW | 1,000 GW | 46.9% | **over cap** |
| Cluster Monitor | Torch x6 | 6,392.8 GW | 6,400 GW | 100.1% | valid |
| Quasar Destroyer | Torch x1 | 1,065.5 GW | 6,400 GW | 600.7% | valid |
| Neutron Cruiser | Torch x6 | 6,392.8 GW | 6,400 GW | 100.1% | valid |
| Galaxy Battlecruiser | Torch x3 | 3,196.4 GW | 1,000 GW | 31.3% | **over cap** |
| Magnetar Battleship | Torch x4 | 4,261.9 GW | 1,000 GW | 23.5% | **over cap** |
| Electron Lancer | Torch x4 | 4,261.9 GW | 1,000 GW | 23.5% | **over cap** |
| Darkstar Assault Carrier | Torch x6 | 6,392.8 GW | 1,000 GW | 15.6% | **over cap** |
| Nebula Dreadnought | Torch x6 | 6,392.8 GW | 1,000 GW | 15.6% | **over cap** |
| Orion Titan | Torch x6 | 6,392.8 GW | 1,000 GW | 15.6% | **over cap** |
| Blazar Mothership | Torch x6 | 6,392.8 GW | 6,400 GW | 100.1% | valid |
| Alien Orbital Fighter | Super Kronos x1 | self-powered | 1,000 GW | — | valid |
| Shiny Colony Frigate | Lantern x2 | 333.2 GW | 1,000 GW | 300.2% | valid |

Nine designs exceed their plant cap. The `x1` Torch itself asks for 6.5% more
than the standard plant can supply, so every standard-reactor Torch design is
technically mismatched.

This can directly create underwhelming ships if the runtime limits available
drive power. The capital ships most expected to feel technologically superior
are the worst pairings. Replacing their standard reactor with the existing
6,400 GW advanced reactor would make Torch x1–x6 valid without inventing new
technology.

## Alien weapon tiers

### Magnetic weapons versus human Mk3 coilguns

Average impact power below is projectile kinetic energy divided by cooldown.

| Mount | Base alien weapon | Alien impact / average | Human Mk3 coil analogue | Human impact / average |
|---|---|---:|---|---:|
| 1 hull | Alien Light Mag Battery | 65 MJ / **4.4 MW** | Light Coilgun Battery Mk3 | 174 MJ / **8.7 MW** |
| 2 hull | Alien Mag Battery | 204 MJ / **11.3 MW** | Coilgun Battery Mk3 | 454 MJ / **22.7 MW** |
| 4 hull | Alien Heavy Mag Battery | 588 MJ / **27.2 MW** | Heavy Coilgun Battery Mk3 | 1,148 MJ / **57.4 MW** |
| 1 nose | Alien Light Mag Cannon | 300 MJ / **12.0 MW** | Light Coil Cannon Mk3 | 718 MJ / **29.9 MW** |

The base alien magnetic family produces roughly half the sustained impact of
the corresponding human Mk3 coilgun. Alien efficiency is good at 60%, but
that does not compensate for lower lethality in an equal-number battle.

The stronger alien modules are a different story:

| Weapon | Mount | Impact | Average impact |
|---|---|---:|---:|
| Advanced Alien Mag Battery | 2 hull | 486 MJ | 40.5 MW |
| Advanced Alien Heavy Mag Battery | 4 hull | 1,400 MJ | 97.2 MW |
| Gen3 Alien Light Mag Battery | 1 hull | 476 MJ | 47.6 MW |
| Gen3 Alien Light Mag Cannon | 1 nose | 2,205 MJ | 183.7 MW |
| Advanced Alien Spinal Mag Cannon | 4 nose | 9,555 MJ | 199.1 MW |

These are convincingly superior. The problem is allocation:

- Pulsar, Quasar, Neutron, Galaxy, and Magnetar use base alien magnetic guns;
- Electron and Orion receive Gen3 light weapons;
- Darkstar and Blazar receive advanced batteries;
- Nebula receives the advanced spinal cannon.

The faction's technology is not uniformly weak. Its predefined fleet mixes
multiple quality generations.

### Point defense

| System | Mass | Shot | Cooldown | Efficiency | Range |
|---|---:|---:|---:|---:|---:|
| Alien PD Laser | 21 t | 64 MJ | 2.4 s | 35% | 350 km |
| Human PD Phaser | 20 t | 50 MJ | 3.0 s | 45% | 350 km |
| Alien PD Particle Beam | **1 t** | 32 MJ | 2.0 s | 20% | 350 km |

The alien laser is only a moderate improvement over the best human point
defense: 60% greater average shot energy with worse efficiency and essentially
the same mass and range. The one-tonne alien particle beam is the more
distinctively advanced system.

PD distribution is uneven. Frigate and larger combatants usually carry one to
three dedicated mounts, but the Escort, Corvette, and Destroyer depend on
dual-purpose batteries or have no dedicated point defense.

### Other weapons

The fleet uses a broad combined-arms catalog:

- Glittering Jewel penetrator missiles on Escorts, Frigate, Battleship,
  Lancer, colony ship, and Mothership;
- Brilliant Sky advanced missiles only on the Mothership;
- heavy plasma batteries on Monitor, Dreadnought, and Titan;
- violet lasers across Destroyer through Mothership;
- X-ray lasers on the Lancer;
- a gamma-ray cannon on the Titan;
- a relativistic particle cannon on the Mothership.

The most exotic weapons are concentrated in a few capital classes. That makes
the fleet thematically varied, but it also means many equal-number engagements
pit optimized human late-game weapons against older alien modules.

## Predefined ship layouts

Armor is nose / lateral / tail.

| Design | Role | Armor | Main weapons | Notable utility modules |
|---|---|---:|---|---|
| Silent Dawn Infiltrator | councilor transport | 12 / 2 / 0 | 256 cm orange laser cannon | infiltration pod, lithium sink, battery |
| Comet Escort | intruder | 10 / 3 / 3 | missile bay, 64 cm orange laser | sink, Hydron Trap, ECM |
| Pulsar Corvette | interdictor | 8 / 5 / 4 | light mag battery and cannon | sink, Muon Spiker, Hydron Trap, battery |
| generic Alien Escort | intruder | 10 / 3 / 3 | missile bay, 64 cm orange laser | sink, Hydron Trap, ECM, battery |
| Nova Frigate | interdictor | 15 / 3 / 3 | two missile bays, PD laser, orange cannon | sink, drive/EV boosts, ECM, battery |
| Cluster Monitor | intruder | 25 / 7 / 10 | heavy plasma, violet cannon | sink, drive/EV boosts, ECM, battery |
| Quasar Destroyer | interdictor | 8 / 3 / 3 | orange laser, base mag battery, violet cannon | sink, boosts, surveillance, battery |
| Neutron Cruiser | councilor transport | 40 / 7 / 10 | base mag, particle PD, laser PD, violet cannon | infiltration pod, ECM, targeting, boosts |
| Galaxy Battlecruiser | space superiority | 10 / 4 / 5 | particle PD, base mag, large violet cannon | two sinks, boosts, ECM, battery |
| Magnetar Battleship | space superiority | 12 / 5 / 5 | particle PD, missile bay, base heavy mag, violet cannon | two sinks, boosts, ECM, surveillance |
| Electron Lancer | space superiority | 25 / 5 / 5 | two X-ray cannon, Gen3 mag, missile, two PD | three sinks, boosts, ECM |
| Darkstar Assault Carrier | army carrier | 15 / 10 / 10 | advanced mag, three PD, violet laser | army pod, sink, boosts, ECM |
| Nebula Dreadnought | space superiority | 20 / 20 / 10 | heavy plasma, two PD lasers, violet battery, advanced spinal mag | four sinks including exotic, repair, ECM |
| Orion Titan | space superiority | 40 / 10 / 20 | gamma cannon, two Gen3 mag cannon, plasma, two PD, two violet lasers | two sinks, repair, ECM |
| Blazar Mothership | patrol | 25 / 25 / 25 | particle cannon, advanced heavy mag, missiles, lasers, two PD | two exotic sinks, repair, ECM, two targeting computers |
| Alien Orbital Fighter | interceptor | 3 / 1 / 0 | missile pod, mini mag cannon | none |
| Shiny Colony Frigate | colony ship | 6 / 1 / 6 | two missile bays, PD laser, orange cannon | three colony kits, ECM |

## Armor assessment

Alien designs generally armor the nose more heavily, but several nominal line
combatants are thin even at the front:

- Destroyer: `8 / 3 / 3`
- Battlecruiser: `10 / 4 / 5`
- Battleship: `12 / 5 / 5`
- Lancer: `25 / 5 / 5`
- Dreadnought: `20 / 20 / 10`
- Titan: `40 / 10 / 20`

The Assault Carrier's `15 / 10 / 10` protects a ship containing a `10,000 t`
army pod. The Mothership is balanced at `25 / 25 / 25`, but that is not
necessarily heavy protection for the largest target in the game.

If human players design narrow-purpose fleets with high nose armor and keep
formation discipline, equal ship count is a misleading comparison. Humans may
bring much more effective armor and offensive mass per combatant.

## Utility-module pattern

Most fusion combatants receive:

- Alien Lithium Heat Sink;
- Alien Muon Spiker: 35% thrust multiplier;
- Alien Hydron Trap: 2× exhaust-velocity multiplier for hydrogen;
- Alien Superconducting Coil Battery;
- Alien ECM on larger hulls.

This is a sensible high-level package. Weaknesses are in its application:

- several small ships omit the battery;
- only two ships use the superior exotic heat sink;
- no ship uses the superior nanowire battery;
- only Cruiser and Mothership use targeting computers;
- repair capability appears only on Dreadnought, Titan, and Mothership;
- surveillance and mission modules consume slots on ships that may enter
  combat.

## Template defects

### Duplicate identifier

`TISpaceShipTemplate.json` contains two records with `dataName: "Ship35"`:

- Comet-class Escort
- Cluster-class Monitor

Depending on loader behavior, one can overwrite the other, both can coexist
with ambiguous lookup, or later tooling can reject the file. Each design needs
a unique identifier.

### Malformed JSON

One `Empty` module record contains a trailing comma before its closing brace.
Strict JSON parsers reject the complete file. Terra Invicta's loader evidently
tolerates it, but research and mod tooling should not have to repair vanilla
syntax before reading the designs.

## Overall judgment

The alien catalog is not uniformly undertuned. Its best kinetic, power, and
specialized modules are formidable. The predefined fleet is under-realizing
that catalog.

The strongest evidence for a real design problem is:

1. reactor/drive cap mismatches on nine designs;
2. use of base magnetic weapons that lose to human Mk3 coilguns;
3. non-use of the strongest reactor and battery;
4. light armor on several line combatants;
5. inconsistent point-defense and targeting support;
6. data defects in the design catalog.

This supports the user's observation that equal-number human fleets can win
without losses. It does not yet show that every alien module needs a numeric
buff.

## Recommended next audit

Before changing alien module stats:

1. give every predefined design a unique template identifier;
2. pair every Torch xN with a reactor whose cap meets `req power`;
3. define alien equipment generations and assign them by campaign date or
   alien strategic progress;
4. compare capital-ship armor mass, not only armor points and ship count;
5. replace base magnetic weapons on late line combatants with Advanced or Gen3
   equivalents;
6. test whether additional PD and targeting computers improve AI outcomes;
7. record tactical-AI behavior in controlled equal-cost battles;
8. compare equal fleet **mass and resource cost**, not equal hull count.

Only after those tests should raw alien weapons, reactors, or hulls receive a
general buff.

## Deferred player-observation hypothesis

The current player-observation hypothesis is:

- early human fleets defeat aliens by saturating them with missiles because
  alien laser weapons are ineffective in the point-defense role;
- later human fleets combine that weakness with the poor output of base alien
  magnetic weapons;
- improving alien laser point-defense potential and assigning proper
  Advanced/Gen3 magnetic weapons may have a larger combat effect than a broad
  statistical buff;
- reactor-cap mismatches and utility placement should be fixed in the same
  design pass, because weapons cannot be evaluated independently of available
  power, batteries, heat sinks, targeting support, and defensive coverage.

This remains a hypothesis from observed campaign play rather than a settled
balance decision. Alien changes are deferred until a future slice can combine
more campaign experience with controlled missile-saturation and equal-cost
combat tests.
