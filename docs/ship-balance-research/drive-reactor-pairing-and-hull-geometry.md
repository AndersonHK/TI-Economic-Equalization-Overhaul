# Drive/reactor pairing, open-cycle cooling, and hull geometry

Last reviewed: 2026-07-29  
Game data: Terra Invicta 1.0.49 installed templates, English localization, and
runtime assembly

> **Status:** research draft only. No values in this document are approved
> changes unless they also appear as settled decisions in the
> [planning changelog](CHANGELOG.md).

This pass covers Diana, Nova, Nerva, Kiwi, the Compact Solid Core reactor line,
the Escort hull, and the runtime meaning of hull length, hull width, and
open-cycle drive cooling.

## Executive assessment

- **Diana is broadly defensible.** It is explicitly a nine-engine
  methane/oxygen cluster. Its `380 s` equivalent specific impulse and
  `2.24 MN` per engine are credible high-end vacuum methalox values. The
  current 14-tonne cluster mass cannot be checked against an official Raptor
  engine mass, but it is not an obvious outlier.
- **Nova is not ordinary chemical propulsion.** "Stabilized hydrogen" most
  plausibly refers to atomic hydrogen trapped in a cryogenic matrix, although
  metastable metallic hydrogen is another possible reading. Both are
  speculative storage technologies. The game's `540 s` is modest relative to
  old atomic-hydrogen paper studies, but the enabling storage assumption has
  not been demonstrated.
- **Nerva's performance is conservative in thrust and historical in exhaust
  velocity.** Its `825 s` is the historical NERVA flight-engine target, while
  its `49 kN` is much smaller than the approximately `247–334 kN` historical
  prototype/design scale.
- **Kiwi uses a historical test-reactor name for a fictional compact flight
  engine.** Its `894 s` is plausible as an aggressive modern solid-core target,
  but the historical Kiwi reactors were nonflight proof-of-principle hardware.
- **The main Nerva/Kiwi balance defect is mass allocation.** Both drive
  templates have zero hardware mass. Nerva initially borrows about `11.3 t`
  from Solid Core I; Kiwi borrows only about `1.24 t` from Compact Solid Core I.
  The latter is especially hard to reconcile with a critical reactor,
  reflector, pressure vessel, controls, turbopumps, nozzle, and shielding.
- **Open-cycle radiator relief is physically well motivated during an NTR
  burn, but 100% relief at all times is too generous.** Most core and nozzle
  heat can leave in the hydrogen exhaust. Shutdown decay heat, radiation
  deposition, tank heating, and residual structural losses remain. NERVA
  component data permit a value below 5%; **1% of the drive-associated
  closed-cycle heat is the current research-led draft**.
- **Hull length and width are gameplay geometry, not prefab scale.** Changing
  them changes armor mass, turning, cross-section, formation spacing, target
  threat tests, and UI selection dimensions. It does not rescale the visible
  ship mesh or the prefab's raycast hit colliders.

## Installed drive values

The following figures come from `drives.csv`. Specific impulse is calculated
as `exhaust velocity / g0`; propellant flow as `thrust / exhaust velocity`.

| Drive ×1 | Localized identity | Thrust | Exhaust velocity | Isp | Flow | Template drive mass | Required plant output |
|---|---|---:|---:|---:|---:|---:|---:|
| Diana | heavy-duty liquid methane/oxygen; template note "Raptor Vacuum ×9" | 20.16 MN | 3.73 km/s | 380 s | 5,405 kg/s | 14 t | 0 |
| Nova | chemical rocket using "stabilized hydrogen" | 7.85 MN | 5.30 km/s | 540 s | 1,481 kg/s | 15 t | 0 |
| Nerva | solid-core fission drive for interplanetary journeys | 49 kN | 8.09 km/s | 825 s | 6.06 kg/s | 0 t | 0.283 GW |
| Kiwi | compact, "criticality limited" solid-core fission drive | 33 kN | 8.77 km/s | 894 s | 3.76 kg/s | 0 t | 0.207 GW |

The powered-drive relationship is internally consistent:

`jet power = thrust × exhaust velocity / 2`

`plant output requested = jet power / drive efficiency`

Nerva therefore has about `198 MW` of jet power and asks for `283 MW` at its
70% drive efficiency. Kiwi has about `145 MW` of jet power and asks for
`207 MW`.

## Diana: nine-engine methalox cluster

The localization and project text agree: Diana is a large liquid-methane and
liquid-oxygen chemical rocket. The template note specifies nine Raptor Vacuum
engines.

The game assigns each engine:

- `20.16 MN / 9 = 2.24 MN` thrust;
- `14 t / 9 = 1.56 t` of cluster mass;
- `380 s` equivalent vacuum specific impulse.

The current FAA description of Starship reports nine total Raptor engines,
liquid methane/oxygen propellants, and approximately `28 MN` maximum Starship
thrust. It does not say that all nine are vacuum engines and it does not
publish individual engine mass. The game's cluster is therefore lower-thrust
than that current mixed nine-engine stage, but it is in the same engineering
neighborhood rather than being an impossible extrapolation.

Source: [FAA, Final LC-39A Starship environmental review, Appendix B1](https://www.faa.gov/space/stakeholder_engagement/spacex_starship_ksc/SpaceX-SSH-LC-39A-Final-EIS-Volume-II-AppB1_ESA-Consultation-USFWS-Part1.pdf).

### Planning opinion

Retain Diana's thrust, exhaust velocity, and mass for now. It is a useful
grounded upper chemical benchmark. The main future question is whether a ship
should be allowed to treat a nine-engine, roughly 20-MN cluster as one
universally mountable drive, not whether the engine values are intrinsically
absurd.

## Nova: what "stabilized hydrogen" probably means

The phrase is not explained by the unlocking project or by **Advanced Chemical
Rocketry**. That technology still describes combustion, lighter materials,
and making propellants more stable. It does not name metallic hydrogen.

The closest real research phrase is **atomic hydrogen stabilized in solid
molecular hydrogen at cryogenic temperature**. Atomic hydrogen recombines into
ordinary H2 and releases stored chemical energy. NASA studies have explicitly
used "stabilized" for this matrix-isolation idea:

- approximately `750 s` was predicted for a mixture containing at least 15%
  atomic hydrogen by mass;
- broader paper studies considered roughly `600–1,500 s`;
- production, storage, transfer, millikelvin/kelvin refrigeration, and
  preventing premature recombination were the dominant unresolved problems.

Sources:

- [NASA NTRS, requisite temperatures for stabilizing atomic H in solid H2](https://ntrs.nasa.gov/citations/19780034328)
- [NASA NTRS, atomic hydrogen propellants: historical perspectives](https://ntrs.nasa.gov/citations/19930040237)
- [NASA NTRS, new propellants and cryofuels](https://ntrs.nasa.gov/citations/20060045675)

**Metastable metallic hydrogen** is the other possible interpretation. It has
long been proposed as a very high-energy rocket propellant, but a 2023
experiment did not observe it remaining metastable at zero pressure. That
makes it an even more speculative reading, and the game does not use the word
"metallic."

Source: [APL Materials/OSTI, Metallic hydrogen: Study of metastability](https://www.osti.gov/pages/biblio/2578428).

### Planning opinion

Read Nova as a fictional, partially stabilized atomic-hydrogen cryofuel unless
later localization says otherwise. Its `540 s` is actually conservative
relative to atomic-hydrogen paper performance, but it is not a conservative
technology claim. The balance cost should appear in:

- extremely difficult cryogenic storage;
- boiloff/refrigeration power;
- safety and spontaneous-recombination risk;
- specialized tanks and feed equipment;
- likely low technology readiness and high cost.

Calling it a normal incremental chemical rocket hides the real assumption.
Nova should either remain a speculative bridge technology with those burdens,
or its exhaust velocity should return toward the approximately `450 s` upper
range of conventional hydrogen/oxygen engines.

## Nerva and Kiwi against Rover/NERVA

### Historical anchors

NASA's Rover/NERVA program summary reports approximately:

- `1,100 MWt` and `55,000 lbf` for the NRX series;
- approximately `850 s` equivalent demonstrated specific impulse;
- a later flight-engine target of `75,000 lbf` (`334 kN`) and `825 s`.

The XE-Prime integrated ground engine included the reactor, pressure vessel,
nozzle, turbopump, and valves. It weighed about `40,000 lb` (`18.1 t`), was
designed for `55,430 lbf` (`246.6 kN`) at `1,140 MWt`, and measured about
`6.91 m` long by `2.59 m` diameter.

Sources:

- [NASA NTRS, nuclear propulsion technology review and XE-Prime data](https://ntrs.nasa.gov/api/citations/19920001919/downloads/19920001919.pdf)
- [NASA report to Congress, NERVA flight-engine target](https://ntrs.nasa.gov/api/citations/19690006381/downloads/19690006381.pdf)
- [NASA NTRS, Rover/NERVA program summary](https://ntrs.nasa.gov/api/citations/19930074438/downloads/19930074438.pdf)

The historical Kiwi reactors were deliberately named for a flightless bird:
they were early nonflight proof-of-principle reactors. NERVA was the
operational-engine development descended from that work. A game "Kiwi Drive"
can reasonably be inspired by a compact modern derivative, but it is not a
literal historical Kiwi engine.

Source: [NASA NTRS, contemporary description of Kiwi and NERVA development](https://ntrs.nasa.gov/api/citations/19650001545/downloads/19650001545.pdf).

### Performance verdict

| Item | Verdict |
|---|---|
| Nerva, 49 kN | Conservative relative to the historical integrated engine and flight design |
| Nerva, 825 s | Exactly the historical NERVA flight target; reasonable |
| Kiwi, 33 kN | Plausible for a deliberately small engine, although not a historical Kiwi value |
| Kiwi, 894 s | Aggressive but within modern solid-core NTP design ambition near 900 s |
| Both at 70% drive efficiency | A tolerable bookkeeping efficiency if it represents thermal-to-jet coupling, not electricity generation |
| Both at 0 t drive mass | Only defensible if the selected reactor mass is explicitly treated as the complete integrated reactor-engine assembly |

### Pairing mass in the current game

Power-plant mass scales with the output the design needs, not automatically
with the plant's maximum rating.

| Drive | First required plant | Output needed | Plant specific mass | Implied plant mass | Drive mass | Implied combined mass |
|---|---|---:|---:|---:|---:|---:|
| Nerva | Solid Core Fission I | 0.283 GW | 40 t/GW | 11.32 t | 0 t | 11.32 t |
| Kiwi | Compact Solid Core Fission I | 0.207 GW | 6 t/GW | 1.24 t | 0 t | 1.24 t |

Nerva's 11.3-tonne first pairing is not obviously too heavy; if anything, the
historical XE-Prime shows why a nonzero absolute mass floor matters. The game
is extrapolating downward from gigawatt-class reactors as though reactor mass
were perfectly linear.

Kiwi exposes that problem much more clearly. A 1.24-tonne integrated nuclear
rocket has to contain every item listed above and still maintain criticality.
Its own template note says "criticality limited," yet its mass formula has no
criticality floor.

This is also why a `100–200 t/GW` electricity-producing reactor benchmark
should not be applied mechanically to a direct NTR. A NERVA-type engine does
not need a thermocouple/turbine generator between the core and the propellant.
It directly heats hydrogen. It still needs the core, reflector, controls,
pressure vessel, turbopumps, nozzle, shutdown cooling, shielding strategy, and
vehicle stand-off.

### Planning envelope, not a settled call

- Preserve Nerva's `8.09 km/s` and Kiwi's `8.77 km/s` pending progression
  review.
- Preserve their thrust for now; both are conservative small-engine values.
- Add either a nonzero drive mass or a minimum reactor mass. Doing both without
  defining what each module contains would double-count hardware.
- A useful first research floor is roughly `5–15 t` for a small integrated
  solid-core NTR before crew shielding, with the lower end deliberately
  generous. This is an extrapolation, not a demonstrated small engine.
- Keep electricity-producing reactor mass and direct-thermal NTR mass as
  separate balance problems.

## Open-cycle cooling: where the current abstraction works

The runtime's `Calc` rule treats Nerva and Kiwi as open cycle because each
single-thruster propellant flow exceeds `3 kg/s`. When a drive is open cycle,
the power-plant waste-heat method removes the entire drive-related load from
the radiator calculation. Systems and weapons remain in the radiator load.

A NERVA-type expander cycle strongly supports large radiator relief during a
burn. Cold hydrogen:

1. cools the nozzle, pressure vessel, neutron reflector, and control drums;
2. drives the turbopumps after collecting that heat;
3. passes through the core and absorbs fission heat;
4. exits through the nozzle.

Source: [NASA NTRS, Nuclear Thermal Propulsion: A Proven Growth Technology](https://ntrs.nasa.gov/api/citations/20120003776/downloads/20120003776.pdf).

Thus most propulsion heat is intentionally carried away by propellant. A large
steady propulsion radiator would defeat much of the point of a direct thermal
rocket.

## Where 100% becomes too generous

The exemption is not literally complete:

- neutron and gamma radiation deposits heat in structures and nearby
  propellant tanks;
- pumps, bearings, control actuators, electronics, and structure absorb heat;
- plume/nozzle radiation and conduction do not all enter the working fluid;
- after shutdown, fission-product decay and stored sensible heat continue
  without the main exhaust flow.

Historical shutdown studies used hydrogen pulse cooling for days, trading
away propellant to avoid a large radiator. Other studies explicitly compared
continued hydrogen cooling with an auxiliary space radiator. A NERVA-based
example still produced about `1.35 MW` of afterheat one hour after a
25-minute burn, falling to about `36.5 kW` after 24 hours.

A more useful lower-bound calculation comes from a full-power NERVA component
heating analysis. Its reference engine operated at **1,515 MW thermal**. The
primary nozzle and aluminum pressure vessel received substantial radiation
heating, but the design deliberately cooled both with liquid hydrogen before
that hydrogen entered the core and left through the nozzle. The optional
10,000 lb crew disk shield was deliberately uncooled and absorbed **88
BTU/s**, or **92.8 kW**:

`92.8 kW / 1,515,000 kW = 0.0061% of reactor thermal power`

That figure is not the complete spacecraft heat load: the uncooled thrust
structure, controls, bearings, electronics, conducted heat, and shutdown heat
remain. It nevertheless proves that an unavoidable radiator fraction need not
be as high as 5% during steady thrust. The afterheat example is also only
**0.086% of rated power** at one hour, although it occurs precisely when the
main hydrogen flow has stopped.

Sources:

- [NASA TN D-3629, nuclear-rocket shutdown cooling](https://ntrs.nasa.gov/api/citations/19660025921/downloads/19660025921.pdf)
- [NASA NTRS, NERVA afterheat and pulse cooling](https://ntrs.nasa.gov/api/citations/19890001573/downloads/19890001573.pdf)
- [NASA NTRS, auxiliary radiator versus hydrogen shutdown cooling](https://ntrs.nasa.gov/api/citations/19710028762/downloads/19710028762.pdf)
- [NASA NTRS, radiation heating in selected NERVA engine components](https://ntrs.nasa.gov/citations/19720010009)

### Planning decision and minimum

The evidence allows a floor below 5%. There is no universal thermodynamic
minimum above zero for a running open-cycle engine: with sufficiently complete
regenerative cooling, heat collected from the engine can be added to the
propellant and exhausted. Real structures and shutdown transients prevent
literal zero, but the NERVA component numbers support a sub-one-percent
steady-state residue.

For a static game abstraction that has no post-burn decay curve, **1% is the
recommended conservative proof-of-concept floor**. It is high compared with the
quantified disk-shield heating, provides margin for unquantified structures and
controls, and leaves a small continuous stand-in for shutdown heat. Five percent
is defensible as a balance penalty, but the engineering data do not require it.

Do not simply turn Nerva and Kiwi into closed-cycle drives. Use this hierarchy:

| Drive architecture | Propulsion heat credited to exhaust during thrust | Residual model |
|---|---:|---|
| Ordinary chemical | effectively 100% of reaction/engine heat | small ship-system burden only |
| Direct solid-core NTR | **99% planning draft** | **1%** residual stand-in for structural heating and unmodelled shutdown heat |
| Open-cycle electric drive using propellant as coolant | approximately 70–90%, architecture-dependent | power electronics and conversion losses still radiate |
| Closed-cycle electric drive | 0% | all conversion and drive loss reaches radiators |

For the existing power-plant heat method, apply the fraction only to the
drive-associated portion that a closed-cycle drive would have sent to the
radiator:

`radiator heat = system heat`
`              + 0.01 × drive useful power × (1 / plant efficiency - 1)`

Shutdown can later receive its own mechanic:

- charge a small quantity of hydrogen after each burn for decay-heat pulse
  cooling; or
- add a bounded post-burn radiator load that decays with time.

The **nonzero requirement is settled**. The **1% coefficient is a research-led
draft** awaiting the user's final balance call; it is not a measured universal
engine efficiency.

## Hull length and width: what the runtime actually uses

Runtime inspection shows `length_m` and `width_m` feeding:

- cylindrical reported hull volume;
- nose, lateral, and tail armor mass and armor-depth limits;
- armor cap-angle coverage;
- cross-sectional-area calculations;
- moment-of-inertia, angular acceleration, and angular-velocity limits;
- fleet formation and relative-position spacing;
- AI hull categorization and some design choices;
- defensive-fire range padding;
- projectile-threat and predicted-sphere interception tests;
- mouse/selection collider dimensions and camera framing.

Length thresholds also classify ordinary human hulls as small (`≤100 m`),
medium (`>100 and <200 m`), or large (`≥200 m`), which can affect downstream
logic.

### What they do not do

The visible hull is instantiated from the `modelResource` prefab. Entering
combat applies one uniform global combat scale to the complete visualizer.
There is no per-hull rescaling from `length_m` or `width_m`.

Actual ballistic impacts use Unity raycasts against the colliders found under
that instantiated model. Those colliders are authored into the prefab and are
scaled with the visual model by the same global factor. Editing template
length or width alone does not reshape them.

At the same time, predicted projectile-threat tests approximate a ship as a
sphere using template length. Therefore changing length can alter point-defense
and threat decisions even though the raycast hitbox and visible mesh stay
unchanged. The template and prefab can become internally inconsistent.

### Can appearance and collision be changed separately?

Yes, but not with another ordinary hull-template number.

1. **Visual size:** patch the spawned ship visualizer's local scale, or replace
   the hull prefab/asset bundle with a resized model.
2. **Raycast hit geometry:** scale or replace the prefab's child colliders.
3. **Statistical/predictive geometry:** patch uses of `length_m` and `width_m`,
   or introduce a mod-side per-hull dictionary for visual scale, collision
   dimensions, and balance dimensions.

The safest design is to keep three explicit concepts:

| Concept | Used for |
|---|---|
| Balance length/width | armor, turning, cross-section, formation |
| Visual scale | rendered mesh and attachment points |
| Hit geometry | prefab colliders and predictive threat radius |

A visual-only scale patch must also inspect weapon mounts, drive plumes,
radiator deployment, damage layers, and selection reticles because those are
children of the model. A collider-only patch must update the predictive sphere
or point defense will reason about a different target size than projectiles
actually hit.

## Ship-type geometry

The measured early-hull meshes, individual raycast colliders,
structural-integrity values, and comparative mass analysis are consolidated in
the authoritative [early human hull
analysis](gunship-and-escort-hull-analysis.md) rather than duplicated here.

## Questions left open

- Should direct NTR drives contain the complete reactor-engine mass, with a
  separate small hotel-power plant, instead of borrowing an electricity plant?
- What minimum critical core/reflector mass should apply before linear t/GW
  scaling begins?
- Is NTR shutdown heat best represented as propellant expenditure, a temporary
  radiator load, or a fixed operational abstraction?
- Should hull template geometry be changed when the statistical dimensions and
  measured prefab colliders intentionally serve different purposes?
