# Fusion magnet scaling, reactor size, and overlapping technology timelines

Date: 2026-09-11. Status: exploration and research only; no gameplay changes.

## Findings

Early fusion and advanced fission should be allowed to coexist. There is no physical requirement to develop vapor-core or extreme gas-core fission before useful fusion. Conversely, a successful terrestrial fusion plant would not establish that a fusion rocket beats a fission rocket in mass, thrust, reliability, or cost.

The magnetic-field argument is substantially right, with two qualifications. Fusion power density can scale approximately with the **fourth power** of field, not exponentially. And the surface-area constraint is an obstacle to shrinking a reactor at fixed output, even though it makes scaling up a geometrically similar reactor at fixed power density difficult. These effects create an engineering optimum; they do not establish that every successive reactor should be smaller or larger.

My judgment is that useful terrestrial fusion preceding a continuous terawatt, 25,000 C gas-core fission system is a reasonable central scenario. Fusion preceding *every* vapor-core concept is a weaker proposition. These are comparative judgments, not demonstrated development schedules. A scenario in which extreme gas-core fission never becomes commercially worthwhile is also credible.

I interpret the question's “internal confinement” as **inertial confinement**. That is an independent fusion approach, not a demonstrated final stage that eliminates magnetic fusion's competitors.

## 1. What better magnets actually buy

For a thermal, approximately equal-temperature D-T plasma, write the fusion power density as

```text
q_f = n_D n_T <sigma v>(T) E_f
p approximately = (n_e + n_i) k_B T
beta = p / [B^2 / (2 mu_0)]
```

Holding fuel composition, temperature, pressure-profile shape, and the usable beta limit fixed gives `n proportional to beta B^2`, hence

```text
q_f proportional to beta^2 B^4
P_f proportional to beta^2 B^4 V_plasma
```

This is the familiar high-field argument described by [MIT's high-field fusion program](https://www-new.psfc.mit.edu/research/topics/high-field-pathway-fusion-power). Doubling field can permit sixteen times the fusion power in the same plasma volume **under those assumptions**. It does not mean an existing machine could safely accept that increase.

The following is an illustrative derivation, not a reactor design calculation. For geometrically similar plasmas, `V proportional to L^3` and enclosing area `A proportional to L^2`. At fixed fusion output and beta:

```text
V proportional to B^-4
L proportional to B^(-4/3)
A proportional to B^(-8/3)
P_f / A proportional to B^(8/3)
```

| Field relative to reference | Available plasma power density | Plasma volume at fixed power | Linear size at fixed power | Average wall loading at fixed power |
|---|---:|---:|---:|---:|
| 1.0 | 1.00x | 1.000x | 1.000x | 1.00x |
| 1.5 | 5.06x | 0.198x | 0.582x | 2.95x |
| 2.0 | 16.00x | 0.0625x | 0.397x | 6.35x |

Thus, spending all of a doubled field on miniaturization increases average wall loading by approximately **6.35 times**. The attractive plasma scaling creates a harder surrounding machine.

These relations also omit the confinement time. The relevant ignition/gain conditions involve temperature, density, and energy confinement together; pressure capacity alone does not guarantee a burning plasma. See [ITER's Lawson-criterion glossary](https://www.iter.org/fusion-glossary). Density limits, turbulence, radiation, alpha-particle confinement, impurities, and stability can prevent operation at the assumed beta and temperature. Tokamaks and stellarators have different confinement scalings; a single B^4 law cannot predict their relative net output.

Higher field can therefore be spent in several ways: reducing size, increasing output, reducing required beta, relaxing confinement demands, or moving coils farther from the plasma to accommodate shielding. It need not all become higher rated power.

### Magnet field and plasma field are different quantities

Use on-axis or a consistently defined plasma field in comparisons. A quoted peak field on the superconducting conductor is generally higher. The ratio depends on geometry, the central column, coil placement, and intervening shielding.

High-temperature superconductors still require cryogenic operation. Their useful advance is retaining substantial current-carrying capability at high fields and higher cryogenic temperatures, not room-temperature operation. The [SPARC model-coil paper](https://arxiv.org/abs/2308.12301) reports a demonstrated 20.1 T peak conductor field and a structural case accommodating nearly 1 GPa stress. This is a magnet milestone, not a reactor demonstration.

The magnetic pressure scale is

```text
p_B = B^2 / (2 mu_0)
```

It is approximately 10 MPa at 5 T, 40 MPa at 10 T, and 159 MPa at 20 T. These calculated values are **not** the peak structural stress: coil geometry can amplify local stresses substantially. Stronger superconductors do not remove the mass of structures that carry magnetic forces, or the need to manage quenches and stored magnetic energy.

## 2. ITER, Helios, and the compact high-field comparison

Helios is **Thea Energy's** proposed planar-coil stellarator. It is distinct from Proxima Fusion's Stellaris and from the company Helion. For reproducibility, the Helios numerical comparison below uses Table 1 of the December 2025 preconceptual overview. The [September 2026 publication announcement](https://thea.energy/press-release/thea-energy-publishes-the-most-practical-fusion-power-plant-design-and-a-clear-path-toward-energy-on-the-grid/) confirms publication of the design series and still describes approximately 400 MW electric output; it does not establish achieved plant performance. The journal overview could not be retrieved in full during this review, so the earlier table is explicitly versioned.

| Quantity | ITER design | ARC, 2015 academic concept | Helios, December 2025 concept |
|---|---:|---:|---:|
| Confinement | Tokamak | Tokamak | Quasi-axisymmetric stellarator |
| Major radius | 6.2 m | 3.3 m | 8.0 m |
| Plasma magnetic field | 5.3 T | 9.2 T | 6.0 T on-axis |
| Plasma volume | 840 m3 | 141 m3 | 500 m3 |
| Fusion power | 500 MW | 525 MW | 958 MW |
| Fusion power / plasma volume, calculated | 0.60 MW/m3 | 3.72 MW/m3 | 1.92 MW/m3 |
| Electric role | No electricity generation | Proposed 200–250 MWe | Proposed 390 MWe net |

Sources: [ITER parameters, Forschungszentrum Juelich](https://www.fz-juelich.de/en/ifn/ifn-1/forschung/iter), [ITER major radius](https://www.iter.org/node/20687/spot-differences-0), [ARC paper](https://arxiv.org/abs/1409.3540), [ARC heat-exhaust follow-up](https://arxiv.org/abs/1809.10555), and [Helios overview, v1](https://arxiv.org/html/2512.08027v1). The Juelich page's old startup date is not used.

Helios is denser in fusion output per plasma volume than ITER, but has a larger major radius. Its **20 T coil limit is not a 20 T plasma field**. It reserves at least 1.2 m between plasma and coils for breeding and shielding. This illustrates why improved magnets can support engineering space and maintainability rather than maximum shrinkage. The comparison is between different missions and modeled operating points, not measured gains attributable solely to magnets. [Helios overview](https://arxiv.org/html/2512.08027v1)

ARC is the cleaner illustration of the compact high-field argument. Its 2015 concept approaches ITER's fusion output at much smaller plasma volume, while its heat-exhaust study explicitly addresses the consequences of concentrated power. These figures are **not current specifications for CFS's commercial ARC**: [2026 MIT modeling](https://library.psfc.mit.edu/catalog/reports/2020/26ja/26ja023/abstract.php) instead studies an 11.4 T operating point targeting about 1.13 GW fusion power.

SPARC provides another useful distinction: the [2020 design paper](https://doi.org/10.1017/S0022377820001257) gives a 1.85 m major radius, 12.2 T plasma field, and a nominal modeled fusion output around 140 MW. It is a gain experiment, not an integrated electricity plant. Small plasma experiments can omit or relax systems required for sustained commercial generation.

Stellarators are not inherently confined to Helios's design point. The [2025 Stellaris study](https://doi.org/10.1016/j.fusengdes.2025.114868) proposes approximately 2,700 MW fusion power in 425 m3 of plasma, or a calculated 6.35 MW/m3. That is a substantially different choice of performance and engineering assumptions, not proof of a universally superior architecture.

## 3. Surface area: why both smaller and larger can be difficult

“Capturing power scales with surface” is a useful first approximation if the permitted load per unit area is fixed. Neutrons actually deposit energy throughout a blanket volume; coolant channels provide internal heat-transfer area. Blanket thickness, absorption, internal flow, and local peaking matter. Still, the neutron power incident per unit enclosing area is a crucial constraint.

For a geometrically similar reactor at fixed field and plasma power density, output grows as `L^3`, but enclosing area only as `L^2`. Average wall loading therefore grows as `L`. This disfavors unlimited enlargement at unchanged operating conditions. At fixed output, however, making the reactor **larger** provides more surface and eases that load. The two statements apply to different comparisons.

There are at least three different engineering problems:

| Interface | Governing difficulty | Consequence for size |
|---|---|---|
| First wall and breeding blanket | Neutron irradiation, volumetric heating, coolant compatibility, component life | Finite allowable power per enclosing area and a shielding thickness that does not scale away |
| Divertor or exhaust region | Concentrated escaping plasma power and particle removal | Local loads can be limiting even when average wall load looks acceptable |
| Final heat rejection | Waste heat, sink temperature, heat-transfer area | Compact reactor cores do not imply compact cooling systems |

The [DOE Fusion Science and Technology Roadmap, June 2026](https://www.energy.gov/documents/fusion-science-and-technology-roadmap) treats structural materials, plasma-facing components, blankets, and the fuel cycle as separate unresolved development areas. A paper's projected component lifetime or breeding ratio is a design result, not validation under decades of reactor service.

For a spacecraft, the final interface is especially demanding. In an idealized deep-space radiator calculation:

```text
P_waste = emissivity * StefanBoltzmannConstant * A_emitting * T_radiator^4
```

At emissivity 0.9, rejecting **1 GW of waste heat** requires the following calculated total emitting area. This neglects solar heating, mutual irradiation, view-factor losses, temperature gradients, and plumbing limitations.

| Radiator temperature | Total emitting area |
|---|---:|
| 800 K | 47,839 m2 |
| 1,000 K | 19,595 m2 |
| 1,500 K | 3,871 m2 |

An ideal two-sided panel has half this planform area. These are geometric lower-bound illustrations, not mass estimates. The radiator operates at the waste-heat temperature, not the fusion plasma temperature; hotter rejection also constrains the achievable thermal-cycle efficiency.

Modularity is one possible response. Splitting a fixed plasma volume into N geometrically similar units gives total enclosing area proportional to `N^(1/3)`. That improves surface per unit output in this toy model, but duplicates shielding, magnets, pumps, controls, and maintenance interfaces. Moreover, every unit must independently achieve adequate confinement. The derivation gives a reason to explore modules, not a reason to expect arbitrarily small fusion reactors.

A long linear machine is another important exception to uniform geometric scaling: lengthening it at fixed cross-section increases both volume and lateral area approximately in proportion. End losses and stability then become central. “Large” needs to distinguish longer machines, thicker machines, and clusters of machines.

## 4. Architecture strengths, weaknesses, and likely niches

The roles below are engineering interpretations, conditional on successful development. None of the listed fusion concepts establishes an operational net-electric fusion plant or spacecraft drive.

| Architecture | Principal strength | Principal difficulty | Plausible role |
|---|---|---|---|
| Conventional/high-field tokamak | Extensive experimental foundation; relatively simple axisymmetric main coils; strong compactness benefit from higher field | Plasma-current disruptions, steady-state current sustainment, exhaust, central shielding space | Early gain demonstrations and thermal-electric plants; later spacecraft electric power if total mass becomes competitive |
| Spherical tokamak | High achievable beta offers another route to power density | Tight central-column space for conductors, shielding, cooling, and maintenance | Potentially compact plants; compact plasma alone does not settle reactor mass |
| Optimized stellarator | Confinement does not require the large driven plasma current of a tokamak; attractive steady-state operation | Three-dimensional field optimization, coil/build tolerances, fast-particle retention, exhaust and access | Long-duration stationary plants, habitats, or large spacecraft power systems |
| Magnetic mirror | Linear geometry, accessible ends, potential compatibility with direct conversion and exhaust | End losses, electron heat losses, stability and power recirculation | Modular/linear plants and potentially propulsion-oriented systems |
| Field-reversed configuration (FRC) | High-beta compact plasma; possible axial propulsion coupling | Sustainment, stability, confinement and driver/heating efficiency at relevant fusion conditions | Candidate compact direct-fusion drive; possibly pulsed or steady depending on implementation |
| Z-pinch | Plasma current supplies the confining field, reducing dependence on a large external magnet set | Instabilities, electrodes, pulsed-power recovery, repeatability and component life | Potential compact or pulsed systems if reactor-scale performance is achieved |
| Inertial confinement | Fusion occurs in small transient targets; demonstrated target ignition | Efficient drivers, inexpensive uniform targets, repetition rate, chamber recovery and fatigue | Pulsed propulsion and potentially stationary plants; tiny targets need not mean tiny engines |
| Electrostatic / IEC / Polywell-type | Attractive simple or compact geometry and potential charged-particle conversion | Particle/electron losses, thermalization, radiation and, for gridded devices, grid interception | High-uncertainty branch; small neutron-source operation is not power-plant validation |

Sources for the underlying distinctions: [PPPL on stellarators and tokamaks](https://www.pppl.gov/news/2024/return-roots-pppl-builds-its-first-stellarator-decades-and-opens-door-research-new-plasma), [NSTX-U spherical-tokamak program](https://www.pppl.gov/nstx-u), [WHAM mirror physics study](https://www.cambridge.org/core/journals/journal-of-plasma-physics/article/physics-basis-for-the-wisconsin-hts-axisymmetric-mirror-wham/35CCAE07989A73709B38C15F38A5CDBE), [NASA Direct Fusion Drive study](https://ntrs.nasa.gov/api/citations/20170003126/downloads/20170003126.pdf), [FuZE measurements](https://arxiv.org/abs/2408.05171), [LLNL inertial-fusion driver requirements](https://lift.llnl.gov/research-areas/ife/driver-technology), and [University of Maryland IEC research](https://sppl.umd.edu/projects/multigrid/).

Thea's planar-coil approach specifically attempts to move some manufacturing difficulty from complex coil shapes into arrays of controllable simpler coils. That is a potentially important industrial advance, not just a stronger-field claim. [Thea's Eos architecture](https://thea.energy/eos/) describes the demonstrator intended to validate this path.

Closed toroidal confinement has no simple continuously open nozzle. Turning a tokamak or stellarator power plant into a direct fusion rocket requires a credible route for extracting energy or particles while maintaining the core. Mirrors and FRCs are geometrically attractive for this purpose, but allowing useful exhaust while retaining adequate fuel and energy confinement is itself the problem to solve.

The NASA Direct Fusion Drive study explores a roughly megawatt-class FRC-based engine, with modeled thrust of 2.5–5 N per MW and about 10,000 s specific impulse. It demonstrates that compact, non-pulse-only first-generation concepts belong in the design space; it does not demonstrate that such an engine works. [NASA study](https://ntrs.nasa.gov/api/citations/20170003126/downloads/20170003126.pdf)

## 5. The fuel and energy pathway matter as much as confinement

D-T releases roughly 14.1 MeV in a neutron and 3.5 MeV in a charged alpha particle: approximately 80% and 20% of the reaction energy. [DOE/Sandia reaction-energy reference](https://www.energy.gov/sites/prod/files/2019/06/f63/The-Sandia-New-Mexico-Tritium-Story-Capabilities-and-R-and-D.pdf)

Magnets cannot directly turn the neutron share into a jet. A D-T drive can absorb neutron energy in propellant or other material, potentially disposing of much of that heat through exhaust, but doing so adds its own coupling, shielding, and geometry constraints. Alternatively it can let some neutrons escape, sacrificing energy utilization and still protecting vulnerable equipment. Also, alpha energy used to sustain the plasma is unavailable for simultaneous direct extraction. The complete power balance is essential.

D-D, D-He3, and p-B11 change the tradeoffs, not just the fuel label. D-D has neutron-producing branches and produces tritium; D-He3 still has side reactions in a deuterium plasma and a challenging fuel supply; p-B11's attractive charged products come with much more demanding plasma conditions and radiation losses. NASA explicitly treats advanced fuels and direct conversion as research problems in [its centrifugal-confinement project](https://www.nasa.gov/directorates/stmd/space-tech-research-grants/advanced-fusion-power-and-thrust-generation-with-centrifugally-confined-plasmas/). [Princeton's p-B11 analysis](https://collaborate.princeton.edu/en/publications/bremsstrahlung-constraints-on-proton-boron-11-inertial-fusion/) also finds serious bremsstrahlung and alpha-ash constraints in inertial concepts. Inertial confinement does not automatically remove advanced-fuel difficulty.

Keep three outputs distinct:

```text
fusion reaction power
  -> captured thermal or charged-particle power
  -> net electricity and/or directed exhaust kinetic power
```

For magnetic fusion, plasma Q compares fusion output to heating delivered to the plasma. For laser fusion, target gain compares fusion yield to laser energy arriving at the target. Neither is facility electrical gain. [ITER's objectives](https://www.iter.org/faqs?untranslated=1) and [LLNL's energy-security explanation](https://lasers.llnl.gov/science/energy-security) make this distinction important.

For a nonrelativistic jet, `P_jet = F v_e / 2`. As a calculated example, 1 GW of directed jet power gives 20 kN at 100 km/s exhaust velocity, or 2 kN at 1,000 km/s. Better propellant economy therefore does not by itself imply better acceleration. The useful comparison is thrust and mission performance per **complete installed system mass**, including heat rejection, not reactor power alone.

## 6. Why fusion can arrive before extreme fission without replacing it

The fission branch must be split into distinct concepts:

Follow-up: [molten- and gas-core containment and demonstrated hardware](molten-and-gas-core-containment-and-demonstration-2026-09-11.md) treats the previously underdeveloped Molten Core branch separately. It distinguishes molten-salt experience from hot liquid-fuel propulsion and explains why the vessel must remain far cooler than the fuel.

| Fission category | What changes physically | Why it is not simply the next guaranteed step |
|---|---|---|
| Solid fuel, including advanced solid-core thermal rockets | Heat generated in solid fuel and transferred to coolant/propellant | Fuel temperature, erosion, irradiation and heat-transfer limits |
| Molten-salt fuel | Fissile compounds dissolved in liquid salt | Chemistry, corrosion, circulation and fuel-cycle hardware; demonstrated operation does not validate several-thousand-degree liquid-core rockets |
| Hot liquid / molten-core propulsion | Liquid fuel heats propellant directly or radiatively, sometimes with centrifugal retention | Cool boundaries, fuel retention, vaporization, multiphase flow, rotation and transient control remain unproven together at proposed temperatures |
| Contained vapor fuel | Fuel exists as vapor inside a material/coolant system | Condensation, chemistry, containment and temperature limits remain |
| Closed gas-core light-bulb concept | Hot fissile gas transfers radiation across a transparent barrier | Barrier transparency, cooling, radiation damage and deposition |
| Open gas-core rocket | Hot fissile gas transfers heat to escaping propellant | Fuel retention, mixing, criticality/control, pressure and wall cooling |
| Continuous terawatt gas-core system | Vast average power as well as high temperature | A further scale/lifetime challenge beyond demonstrating gas-core operation |

Historical studies establish that “vapor core” is not synonymous with the game's hottest plasma reactor. A [NASA survey](https://ntrs.nasa.gov/api/citations/19930015551/downloads/19930015551.pdf) describes contained UF4 vapor concepts with proposed fuel temperatures around 4,000–5,000 K. A separate [burst-mode vapor-core study](https://ntrs.nasa.gov/archive/nasa/casi.ntrs.nasa.gov/19960048080.pdf) considers hundreds of megawatts for thousands of seconds. Neither validates a continuous terawatt installation.

A [1971 NASA open gas-core study](https://ntrs.nasa.gov/api/citations/19710027591/downloads/19710027591.pdf) analyzes a **6,000 MW** rocket with 4,400 s specific impulse. The [nuclear light-bulb reference study](https://ntrs.nasa.gov/api/citations/19710024150/downloads/19710024150.pdf) proposes **4,600 MW** and 1,870 s. These are conceptual engine ratings, not achieved tests. One terawatt is about 167 times the former thermal rating, before asking whether the system can operate continuously or generate electricity efficiently.

Higher core temperature and larger total output are independent axes. A hot gas region is not proof that a pressure vessel, optical barrier, generator, or coolant circuit can operate at that temperature. A closed thermal-electric system also cannot treat the core temperature as the heat-engine inlet temperature without an actual conversion pathway.

The reason to expect fusion to compete early is therefore **comparative program maturity and applicability**, not that gaseous fission violates fundamental physics. Fusion has active integrated power-plant programs and major magnet and plasma experiments. The extreme gas-core systems used as game anchors remain conceptual in the evidence reviewed here. Funding priorities could change their relative progress; no public schedule establishes an inevitable lead measured in decades.

Possible coexistence roles, conditional on both technologies succeeding:

- Mature solid-core fission: modest power, dependable starts, industrial familiarity, thermal propulsion where high thrust matters.
- Fusion electricity: long-duration habitat, industrial, or ship loads if lifetime and fuel-cycle economics become attractive.
- Gas/vapor-core thermal propulsion: a specialized high-thrust, intermediate-exhaust-velocity role where direct heating beats the mass of a fusion/electric installation.
- Direct fusion propulsion: long-range missions where propellant economy compensates for lower thrust or more complex machinery.

These niches do not require that fission always be large and fusion always compact. Small fission can remain especially useful, while a high-availability fusion plant may deliberately be large. Conversely, compact fusion engines could appear before any practical gas-core rocket.

## 7. Timeline: distinguish targets, scenarios, and breakthroughs

Public schedules establish intent, not arrival dates. Thea advertises grid operation in the 2030s; Proxima plans Alpha in the early 2030s and a later commercial plant in that decade. DOE's program aims to enable a mid-2030s pilot. ITER's revised plan places research operations in 2034 and D-T operations in 2039. These programs have different objectives, so ITER's timetable is not a global lower bound on useful fusion. Sources: [Thea roadmap](https://thea.energy/fusion-technology/), [Proxima roadmap](https://proximafusion.com/about), [DOE Office of Fusion](https://www.energy.gov/fusion/office-fusion), [IPP on ITER's baseline](https://www.ipp.mpg.de/5434912/ITER_baseline_2024), [ITER baseline summary](https://www.iter.org/few-lines?trk=public_profile_project-title).

For game worldbuilding, I would use the following **conditional planning windows**, not forecast confidence intervals. They assume sustained investment and substantial success in fusion engineering. Slower development remains possible; some branches may never mature.

| Technology/capability | Conditional placement | What must actually be demonstrated |
|---|---|---|
| Improved solid-core fission and thermal propulsion | 2020s–2040s development and deployment efforts | Mission-qualified fuel, engine/system life, launch and operating infrastructure |
| First D-T fusion-electric pilots, tokamak and/or stellarator | 2030s optimistic; 2040s with slower integration | Net facility electricity, repeatable operation, credible maintenance and fuel supply |
| More dependable D-T fusion electricity | 2040s–2050s if pilots succeed | Component endurance, tritium-cycle performance, availability and economic construction |
| Useful spacecraft fusion power or modest direct-fusion propulsion | 2040s–2060s speculative; 2050s is a defensible game setting | Whole-system specific mass, startup, exhaust coupling, shielding and space heat rejection |
| Hot liquid / molten-core fission propulsion | A possible overlapping advanced branch; no evidence-based arrival date or mandatory precedence over fusion | Nuclear demonstration of cooled containment, fuel retention, high-temperature heat transfer and useful engine operation |
| Vapor-core / gas-core fission demonstrators and specialized engines | Potentially overlapping 2040s–2060s, or never; no evidence-based ordering against all fusion concepts | Architecture-specific fuel retention, materials, thermofluids and integrated operation |
| Commercial or propulsion-relevant inertial fusion | A parallel development track; 2040s–2060s is a scenario, not a required position after magnetic fusion | Efficient repeated ignition, target production/injection and chamber/driver endurance |
| Practical advanced-fuel fusion | Place behind demonstrated fuel-specific milestones; 2050s onward only as an optimistic fictional assumption | Favorable total energy balance, radiation control, fuel supply and direct conversion/exhaust |
| Continuous terawatt gas-core fission or high-acceleration fusion torch | Breakthrough-gated rather than assigned a routine calendar date | Integrated average power, mass, heat handling and lifetime at radically larger scale |

Inertial confinement has already achieved target ignition at NIF, including the [8.6 MJ April 2025 result](https://lasers.llnl.gov/science/achieving-fusion-ignition). That has not demonstrated net facility power or a practical repetitive engine. [LLNL's driver program](https://lift.llnl.gov/research-areas/ife/driver-technology) identifies efficient multi-megajoule laser operation around 10 Hz as an energy-plant requirement. Do not confuse peak pulse power with average useful output: `P_average = pulse energy * repetition rate`.

For this setting, the strongest chronological choice is to let first useful fusion overlap ordinary advanced fission, then gate extreme gas-core fission and fusion torches on their own engineering achievements. A “fusion-first” outcome against exotic fission is reasonable. An “inertial inevitably wins everything” endpoint is not established.

## 8. Consequences for future EEO planning

The August [reactor progression plan](reactor-power-progression-plan-2026-08-24.md) and [conservative linear scaling plan](reactor-conservative-linear-scaling-plan-2026-08-24.md) were gameplay proposals. Their full-rated mass and output ordering should not be read as physical scaling laws. In particular, requiring each later reactor to be physically larger is a balance constraint, not a consequence of fusion engineering.

An eventual redesign could distinguish these independent developments:

1. Better superconductors and magnet structures: compactness, field strength, or operating margin.
2. Better confinement/control: viable density, temperature, gain, stability and recirculating power.
3. Better first walls, blankets and exhaust: sustained power per area and service life.
4. Better power conversion and radiators: useful electrical output per installed mass.
5. Better fuels and direct propulsion: neutron fraction, fuel logistics and exhaust coupling.

This supports compact and large variants within a technology family rather than a universal `fission -> fusion -> inertial` ladder. A later compact reactor could deliver the same useful power with less mass. A later large reactor could deliver more power at similar wall loading. Neither improvement requires the other to disappear.

For any future specific-mass model, distinguish a minimum self-sustaining module from incremental output: magnets, shielding, startup equipment and auxiliaries do not disappear when a lightly loaded ship requests little power. Above that floor, compare complete modules and their lifetime, rather than treating all fusion output as continuously scalable down to a one-tonne installation. This report proposes the distinction, not values or an implementation.

Two earlier benchmark recommendations are corrected alongside this report: the mandatory large-pulse starting point for fusion drives, and the universal lower bound tying first-generation fusion-electric mass to mature fission. A conservative heavy-fusion placeholder remains reasonable; it must be labeled a modeling choice.

## Verification and research limits

All numerical examples derived here use the stated fixed-parameter assumptions. The field/volume/wall-load ratios, magnetic pressures, and radiator areas were independently evaluated with PowerShell arithmetic. This is not a transport simulation, engineering mass model, or probability forecast.

The comparison uses dated primary design studies and institutional sources. Project websites provide targets, while historical NASA concepts provide design precedents. No exhaustive worldwide proof of absence, vendor cost audit, or detailed technology-unlock audit was performed. The report does not validate ship-scale masses from terrestrial plasma volumes. Research-only changes require no build or deployment.
