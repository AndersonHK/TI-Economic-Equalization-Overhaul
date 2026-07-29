# Weapon automation and crew

Last reviewed: 2026-07-28

## Question

If humanity deploys weapons in space, will each mount still need multiple operators, or will weapons be automated with people retained for supervision, maintenance, and restocking?

The evidence strongly favors automated engagement machinery with centralized human authorization and maintenance. It does not support several people continuously operating every mount.

## What the game currently implies

Across ship-mounted weapons:

| Family | Ship weapons | Average listed crew | Listed range | Zero-crew entries |
|---|---:|---:|---:|---:|
| Laser | 88 | 4.45 | 2–8 | 0 |
| Magnetic | 70 | 3.94 | 0–6 | 1 |
| Missile | 57 | 2.00 | 0–6 | 21 |
| Particle | 33 | 2.36 | 0–8 | 1 |
| Plasma | 16 | 4.19 | 1–6 | 0 |

“Ship-mounted” here means a template whose `mount` contains `Hull` or `Nose`; station and region-defense mounts are excluded.

The pattern scales crew upward with mount size. That can be read in two ways:

1. literal operators assigned to the weapon; or
2. an abstract lifetime support burden covering maintenance, cooling, magazines, power conditioning, and damage control.

The first interpretation is weakly supported by modern practice. The second is much more defensible.

## CIWS: engagement is already autonomous

The U.S. Navy states that Phalanx automatically:

- searches,
- detects,
- evaluates,
- tracks,
- engages,
- performs kill assessment.

The Navy calls it the only deployed close-in weapon system capable of autonomously performing that entire chain ([U.S. Navy Phalanx fact file](https://www.navy.mil/DesktopModules/ArticleCS/Print.aspx%3FPortalId%3D1%26ModuleId%3D724%26Article%3D2167831)).

Phalanx still needs people. Its Block 1B magazine carries 1,550 rounds, the mount needs ammunition loading equipment, and the Navy trains dedicated technicians to operate, test, align, diagnose, and repair the system ([Navy enlisted classification manual](https://www.mynavyhr.navy.mil/Portals/55/Reference/NEOCS/Vol2/NEC_Vol_II_Entire_Manual_Oct_24.pdf?ver=2IaMCG60xzI9bUojuRIxrA%3D%3D)).

The important distinction is:

- **combat loop:** can be autonomous;
- **authorization and doctrine:** human responsibility;
- **reload, maintenance, repair, and certification:** human or robotic logistics work.

This is the closest existing analogue for future space point defense. The sensor-to-fire loop will be too fast for a person to aim each shot. A human will configure doctrine, authorize an engagement mode, supervise the system, and intervene when possible.

## Aegis: centralized automation with human weapon selection

The Navy describes Aegis as a centralized, automated command-and-control and weapons-control system designed from detection to kill ([U.S. Navy Aegis fact file](https://www.navy.mil/Resources/Fact-Files/Display-FactFiles/Article/2166739/aegis-weapon-system/)).

A current NAVSEA explanation says operators select a weapon while the Weapon Control System calculates timing, intercept points, and firing parameters ([NAVSEA, “The shield of the fleet”](https://www.navsea.navy.mil/Media/News/Article-View/Article/4223263/the-shield-of-the-fleet-the-aegis-combat-system-and-its-vital-role-in-us-navy-o/)).

That architecture is a better model for a space warship than assigning a gun crew to every mount:

- sensors create a common track picture;
- a combat system ranks threats and proposes engagements;
- a small team manages doctrine, priorities, identification, and authorization;
- local mounts execute fire-control solutions;
- technicians maintain the distributed hardware.

## Submarine torpedoes: autonomous weapon, human launch process

The Mk 48 is an acoustic-homing weapon with digital guidance and control ([U.S. Navy Mk 48 fact file](https://www.navy.mil/DesktopModules/ArticleCS/Print.aspx?Article=2167907&ModuleId=724&PortalId=1)). It can receive target-motion updates over a guidance wire, and the weapon sends telemetry back to the submarine ([NAVSEA Mk 48 support-equipment brochure](https://www.navsea.navy.mil/Portals/103/Documents/NUWC_Newport/QRpage/MK48.pdf)).

This means the torpedo is not manually steered in the sense of a person directly controlling every movement. The submarine combat system and operators provide targeting and optional updates; onboard sonar and guidance execute the terminal engagement.

The launch and reload process remains labor-intensive:

- The Navy's Virginia-class trainer requires operators to perform all steps to load, arm, and launch a torpedo or cruise missile.
- Block I/II simulations include team procedures, visual checks, buttons, dials, switches, and valves.

See the [Naval Air Warfare Center Virginia torpedo-room trainer](https://www.navair.navy.mil/nawctsd/VIRGINIA-Torpedo-Room-Block-III-VA-Torp-Rm-BLK-III-or-VA-BLKIII).

The Navy's submarine weapons specialists are responsible for operating and maintaining launch systems and for safe loading, unloading, shipping, storage, and limited weapon maintenance ([MyNavyHR Machinist's Mate—Submarines](https://www.mynavyhr.navy.mil/Career-Management/Community-Management/Enlisted/Submarine/MM-SS/)).

The conclusion is not “torpedoes need no crew.” It is:

- targeting and launch authority are human-team functions;
- guidance is substantially autonomous;
- mechanical handling, safety, and maintenance still consume crew;
- those people serve a magazine and launch system, not one continuous operator team per torpedo.

## Human control is likely to remain

Current U.S. policy requires autonomous and semi-autonomous weapons to allow commanders and operators to exercise appropriate levels of human judgment over the use of force ([DoD Directive 3000.09 update](https://www.defense.gov/News/Releases/Release/Article/3278076/dod-announces-update-to-dod-directive-300009-autonomy-in-weapon-systems/)).

This does not require manual aiming or a human approval click for every defensive shot. It requires system design, doctrine, authorization, and use that preserve appropriate judgment and responsibility. Point-defense systems already reconcile autonomy with human-established engagement modes.

For a future space combat model, it is reasonable to separate:

- **human authorization:** whether and under what rules a system may engage;
- **machine execution:** tracking, lead calculation, pointing, firing, and kill assessment;
- **human supervision:** monitoring confidence, identification, ammunition, heat, and system faults;
- **maintenance/logistics:** repair, reload, alignment, calibration, and inspection.

## Why space pushes toward more automation

This section is inference from the cited systems and basic operational constraints.

1. **Engagement times can be very short.** Kinetic interceptors, lasers, railguns, and missiles require rapid sensor fusion and aiming. Human reaction time is unsuitable for the inner defensive loop.

2. **Remote control may be delayed or jammed.** A spacecraft must be able to defend itself locally even when communications are unavailable.

3. **Weapons share sensors and power.** Central software must deconflict targets, firing arcs, radiator capacity, and power demand across the entire ship.

4. **Crew survival favors protected stations.** Sending people to individual mounts or external magazines during combat adds pressure-vessel volume, shielding, access routes, and casualties.

5. **Vacuum restocking favors machinery.** Large missiles, kinetic rounds, and replacement optics will be moved by autoloaders or robotics because suited manual handling is slow and dangerous.

6. **Maintenance does not disappear.** Optics foul, barrels erode, rails and insulators fatigue, launch cells need inspection, and power electronics fail. Automation shifts the crew toward technicians and damage-control teams.

## Are modern CIWS magazines manually reloaded?

Phalanx's engagement sequence is autonomous, but its fixed 1,550-round magazine and Navy technician/loading qualifications show that replenishment is a separate support evolution, not part of the autonomous firing loop ([U.S. Navy Phalanx fact file](https://www.navy.mil/DesktopModules/ArticleCS/Print.aspx%3FPortalId%3D1%26ModuleId%3D724%26Article%3D2167831)).

Public Navy material does not support claiming that a Phalanx automatically replenishes itself from a shipwide ammunition store. The conservative conclusion is:

- ready-use feed and firing are mechanized;
- replenishing the mount requires trained personnel and loading equipment;
- future space systems may automate more of this because external manual handling is especially unattractive.

## Are submarine torpedoes manually reloaded?

Modern submarines use handling equipment, but the process remains a trained team evolution with manual controls, checks, tube preparation, and casualty procedures. The Virginia-class trainer explicitly teaches operators to load and launch weapons and reproduces analog buttons, switches, and valves ([Virginia torpedo-room trainer](https://www.navair.navy.mil/nawctsd/VIRGINIA-Torpedo-Room-Block-III-VA-Torp-Rm-BLK-III-or-VA-BLKIII)).

So “manually reloaded” and “automatically reloaded” are both too simple:

- machinery bears and moves the approximately 1.7-tonne weapon;
- people supervise and execute the handling procedure;
- the weapon itself becomes autonomous or wire-guided after launch.

This is a strong analogue for future missiles: an autoloader moves the round, a combat system prepares it, and a small crew supervises the magazine rather than physically aiming the launcher.

## Crew-model recommendation

### Do not assign literal operators per mount

For most space weapons, use zero dedicated continuous operators on the mount itself. The weapon should draw from shared ship-level pools.

### Split crew into three functions

| Function | What it covers | Scaling |
|---|---|---|
| Combat-system watch | identification, doctrine, authorization, target priorities, battle management | Scales with simultaneous engagements and sensor complexity, not mount count |
| Weapon maintenance | electronics, optics, cooling, alignment, barrels/rails, launchers, diagnostics | Scales sublinearly with common mounts; rises with novelty and poor reliability |
| Ordnance handling | magazine inspection, reload, safing, moving rounds, replenishment | Scales with ammunition mass, magazine architecture, and automation |

### Suggested abstract values

These are balancing opinions, not claims about exact future staffing.

| Weapon type | Dedicated firing crew per mount | Shared support concept |
|---|---:|---|
| Point-defense laser/kinetic CIWS | 0 | One combat-system supervisor can oversee several mounts; maintenance pool handles emitters, barrels, sensors, and cooling |
| Fixed laser battery | 0 | Central fire control; technician burden based on optical/power-channel count |
| Missile launcher | 0 | Central engagement team plus magazine/launcher maintainers |
| Large kinetic cannon or railgun | 0 | Central fire control; 1–3 shared specialists for pulse power, rails/barrel, and feed machinery |
| Spinal particle/plasma weapon | 0 | Central fire control; high engineering maintenance burden rather than gun crew |
| Torpedo-like autonomous missile | 0 after launch | Human target authorization and optional updates; shared magazine handlers |

### Interpreting existing game crew values

If removing the values would disrupt balance, reinterpret them as support billets:

- `crew 1–2`: fractional share of a technician or ordnance team;
- `crew 3–4`: high maintenance/reload burden;
- `crew 5–8`: prototype system with poor reliability, extensive cooling/pulse-power equipment, or manual magazine handling.

Avoid explaining those values as people sitting at a weapon console for each mount. Modern naval systems already centralize that work.

## Recommended automation progression

| Automation tier | Engagement | Reload/handling | Maintenance | Balance effect |
|---|---|---|---|---|
| Early | Automated aiming, human authorization | Human-supervised machinery | Crew-intensive | Moderate crew and slow reload |
| Mature | Doctrine-bounded autonomous defense; centralized offensive authorization | Automated internal magazine | Condition-based maintenance | Low per-mount crew |
| Advanced | Distributed autonomous engagement under command intent | Robotic reload and inspection | Robotic replacement of line-replaceable units | Crew becomes ship-level supervision |
| Uncrewed combatant | Fully local autonomous execution within mission rules | Robotic or depot-only reload | Redundant modules, depot maintenance | No onboard crew, higher machinery/redundancy mass |

The design trade is not “crew or no cost.” Reducing crew should add automation, redundancy, diagnostics, robotics, protected data links, and modular repair mass.
