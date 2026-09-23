# Molten- and gas-core containment: temperature, cooling, and demonstrated hardware

Date: 2026-09-11. Research follow-up only; no gameplay changes.

## Assessment

The pressure vessel does not survive by reaching the fuel's temperature. It must remain much cooler, with intervening flow, liners, shielding and active heat removal limiting the energy that reaches it. This is physically legitimate, but demonstrating the required heat-transfer and fuel-retention behavior is a major part of proving the engine.

The earlier [fusion timeline report](fusion-magnet-scaling-and-technology-timeline-2026-09-11.md) grouped molten salt and hot liquid fuel too broadly. Terra Invicta's Molten Core propulsion branch warrants a separate assessment. A molten fluoride salt reactor operating around 650 C is not a demonstrated precursor in every important engineering sense to a rotating liquid-uranium engine heating hydrogen to roughly 5,000 K.

I found no documented demonstration of a deliberately operated, controlled, sustained liquid-core fission propulsion reactor with fuel above 2,000 C in the sources reviewed. That statement excludes accidents, externally heated samples, simulant experiments, and isolated melting during fuel tests. It is a scoped literature finding, not an exhaustive proof that no experiment anywhere reached that condition.

The evidence supports treating hot liquid-core propulsion as an unproven advanced branch. It does not establish that it must arrive before practical magnetic fusion.

## 1. How can a vessel contain something hotter than its melting point?

Temperature and heat flow are different quantities. A hot fluid does not instantly bring every surrounding part to its own temperature. A wall can remain cooler if energy is removed as rapidly as it arrives and if the thermal gradients and mechanical stresses remain tolerable.

An illustrative local heat balance is:

```text
wall energy accumulation = incident heat - removed heat

steady wall temperature requires:
incident radiation + convection + nuclear heating = heat removed by cooling
```

For a simple layer, conduction gives a temperature drop approximately `q'' * thickness / conductivity`; the coolant interface adds approximately `q'' / heat_transfer_coefficient`. These are generic heat-transfer relations, not a sizing model for the NASA engine. They explain why coolant flow rate, channel geometry, wall thickness and local heat flux matter more than a comparison between fuel temperature and tungsten's melting point alone.

The pressure load and thermal protection can be assigned to different components. A cooled outer shell carries pressure; an inner liner and intervening material manage the heat. A thermal barrier alone cannot maintain a permanent temperature difference without heat removal. Even below melting, creep, corrosion, irradiation, thermal cycling and hot spots can make a material unusable.

### What the cited NASA gas-core study actually assumes

The [1971 gas-core paper](https://ntrs.nasa.gov/api/citations/19710027591/downloads/19710027591.pdf) proposes a titanium-alloy pressure shell, a beryllium-oxide reflector/moderator, and a porous or slotted cavity liner. It allocates 7% of reactor power to neutron/gamma heating in the reflector, removed through a separate cooling/radiator system. At its 6,000 MW design point, that is a calculated **420 MW**. The paper explicitly leaves several important subsystems only lightly examined, including the liner and nozzle. This is preliminary component sizing with assumed thermofluid behavior, not a demonstration that the complete engine can operate.

A related [NASA gas-core research review](https://ntrs.nasa.gov/api/citations/19710014431/downloads/19710014431.pdf) discusses adding absorbing material to hydrogen so that it intercepts radiation which would otherwise reach the walls. It also reports electrically driven plasma simulators. Those experiments help test individual mechanisms; their heating power must not be reported as demonstrated nuclear gas-core output.

The broad proposed energy paths are:

```text
hot fissile gas -> radiation absorbed by flowing propellant -> exhaust
hot region -> residual wall heat -> cooling circuit
neutrons and gamma rays -> heating inside structures -> cooling circuit
```

Coolant behind the wall removes heat already absorbed. A protective flow near or through the liner can reduce heat reaching the solid in the first place. Radiation absorption in the propellant is a third mechanism. The opacity, mixing, flow stability and local temperature distributions determine whether this combination works.

The 7% is specific to that study and is not a universal retained-heat fraction for gas-core rockets. Nor does it represent every possible engine heat load. It is sufficient to show that exhausting hot propellant does not automatically eliminate large radiator requirements.

## 2. Molten salt and molten-core rocket fuel are different technologies

| Category | What is liquid? | Relevant engineering issue |
|---|---|---|
| Molten-salt fuel reactor | Fissile compounds dissolved in a carrier salt | Salt chemistry, corrosion, circulation, fission products and vessel life |
| Liquid-metal-cooled reactor | Coolant such as sodium; the nuclear fuel may remain solid | Coolant being liquid does not establish molten-fuel operation |
| Molten-metal fuel reactor | A fissile metal/alloy intentionally operated as liquid | Fuel/container compatibility, retention and cooling |
| Hot liquid-core rocket | Very hot liquid fuel transfers energy to propellant, directly or radiatively | Maintaining cool boundaries while retaining fuel and producing high-temperature exhaust |

Deliberate molten-metal nuclear operation does have history: LAMPRE used molten plutonium-alloy fuel. Contemporary [Los Alamos work on the fuel](https://digital.library.unt.edu/ark%3A/67531/metadc1034345/m2/1/high_res_d/4587710.pdf) and the [1964 conference proceedings](https://digitallibrary.un.org/record/848859/files/A-CONF-28-P-17-864-VOL-11-E.pdf) document the program and reactor tests. That prevents the categorical claim that only molten salts have ever been used as liquid nuclear fuel. It does not supply evidence for a several-thousand-degree propulsion reactor.

Historical liquid-core rocket concepts include rotating fuel-bearing layers and radiant heat transfer. NASA's [1967 liquid-core performance study](https://ntrs.nasa.gov/api/citations/19670030774/downloads/19670030774.pdf) analyzes a rotating tube with a liquid carbide fuel-bearing inner surface, and the tradeoff between higher temperature and heavy fuel vapor entering the exhaust. Not all liquid-core concepts use the same heat-transfer path.

### The centrifugal liquid-core proposal

The modern Centrifugal Nuclear Thermal Rocket (CNTR) proposes a rotating annulus of liquid fuel. Rotation retains the dense liquid toward the outside, while propellant passes inward through the fuel and heats before reaching the exhaust cavity.

NASA's [2023 challenges paper](https://ntrs.nasa.gov/citations/20230000621) targets **5,000 K propellant** while requiring structures to be cooled before the propellant enters the liquid. It separately specifies fuel-contact material compatibility to at least **1,500 K**. That latter value is a materials requirement, not a demonstrated wall temperature or guaranteed operating limit. Rotation, injection and containment must operate together; a simple pot of uniformly 5,000 K liquid is not the proposed arrangement.

This makes the user's concern well founded. A controlled thermal gradient and favorable flow pattern must survive fission heating, boiling/vaporization, gas bubbles, changes in fuel distribution, startup and shutdown. Rotation holds liquid spatially; it does not thermally insulate the wall. Keeping the fuel molten removes the requirement that the fuel itself retain a solid shape, but transfers difficulty into fluid mechanics and containment.

The [2026 CNTR progress paper](https://doi.org/10.1016/j.actaastro.2026.07.066), available online August 4, still identifies fuel vaporization as a critical feasibility problem, including effects on performance and operating duration. It describes modeling and mitigation research, not a successful integrated nuclear engine test. Fuel loss also means that an engine's modeled exhaust velocity does not, by itself, establish acceptable endurance or fuel use.

## 3. What temperatures have actually been demonstrated?

These entries distinguish the measured quantity from a design target. They are selected reference cases, not a universal reactor-temperature record list.

| System | Temperature and what it means | Evidence status |
|---|---|---|
| Molten-Salt Reactor Experiment, 1965–1969 | Approximately **650 C**, average core operating temperature | Actual liquid-fuel reactor operation |
| Aircraft Reactor Experiment, 1954 | Up to approximately **860 C**, salt outlet operating temperature | Actual liquid-fuel reactor operation |
| Pewee solid-core nuclear rocket | Approximately **2,550 K / 2,277 C** exhaust and **2,750 K / 2,477 C** peak fuel temperature | Actual nuclear ground test; solid-core rather than molten-core |
| CNTR hot liquid-core engine | Approximately **5,000 K / 4,727 C** propellant target | Concept and subsystem research; not demonstrated engine output |

Sources: [ORNL materials assessment on MSRE](https://info.ornl.gov/sites/publications/Files/Pub123216.pdf), [DOE molten-salt workshop report on ARE](https://www.govinfo.gov/content/pkg/GOVPUB-E-PURL-gpo119726/pdf/GOVPUB-E-PURL-gpo119726.pdf), [NASA Rover/NERVA summary](https://ntrs.nasa.gov/api/citations/19920001873/downloads/19920001873.pdf), and the CNTR reference above. Celsius conversions are calculated as `K - 273.15`, rounded to whole degrees.

Thus “we have not established hot molten-core propulsion above 2,000 C” is consistent with the reviewed evidence. “We have never operated a nuclear rocket reactor above 2,000 C” would be incorrect. Solid fuel can include refractory compounds and a structural matrix; its behavior cannot be inferred from the melting temperature of pure uranium metal.

There are real small-scale experiments along the liquid-core development path. The [2023/2024 bubbly-flow paper](https://doi.org/10.1016/j.actaastro.2023.12.012) studies relevant fluid dynamics and identifies Galinstan as a uranium simulant for bubble formation. Such experiments test parts of the flow model. They do not establish simultaneous fission heating, high-temperature materials survival, fuel retention and useful thrust.

## 4. Is a tokamak actually easier?

A tokamak provides a clearer physical separation between its extremely hot, dilute core plasma and material walls: magnetic fields confine charged particles. Its temperature is a measure of particle energy, not the temperature of a dense liquid bath touching the chamber. [ITER's explanation](https://www.iter.org/node/20687/hot-hotter-hottest) discusses this distinction.

However, magnetic fields do not insulate against photons and neutrons. Escaping plasma energy must also be exhausted. Fusion has its own demanding actively cooled material interfaces: [CEA's divertor research](https://irfm.cea.fr/en/2017/02/a-divertor-for-demo/) discusses approximately 10 MW/m2 stationary and 20 MW/m2 transient heat loads. These engineering problems remain even after magnetic confinement succeeds.

The useful comparison is therefore between **different unresolved integration problems**. Hot liquid-core fission inherits established fission physics but lacks a demonstrated integrated propulsion system at its proposed temperatures. Tokamaks have a much richer record of relevant plasma experiments but must still demonstrate practical net-electric operation, component life, fuel-cycle integration and, for propulsion, competitive installed mass and exhaust coupling.

My judgment: the present evidence gives no reason to assume that a practical 5,000 K liquid-core rocket is necessarily easier, earlier, or a prerequisite for useful fusion. Equally, its lack of a working prototype does not prove that a fusion spacecraft engine will arrive first.

## 5. Does a vehicle reactor need an earlier power-station version?

It needs progressive validation, but not necessarily the sequence “grid reactor, then vehicle.” Propulsion can proceed from materials and nonnuclear flow tests through nuclear ground-test engines to flight qualification. Rover/NERVA followed a propulsion-specific ground-test program; the NASA summary documents multiple reactors, engine tests, endurance runs and restart tests.

A thermal rocket deliberately expels its hot working fluid. A stationary plant normally recirculates coolant and operates a converter over much longer service intervals. Different cooling opportunities and lifetime requirements mean that success in either application does not automatically establish the other.

For EEO, retain molten-core propulsion as a separate speculative branch with its own engineering milestones. Do not use demonstrated molten-salt operation to justify its temperatures, or use a generic “fission is mature” statement to place it automatically before fusion. No numerical rebalance or unlock change is made here.

## 6. Why so many old concepts, but no flown NERVA engine?

The principal historical explanation is a mismatch between advanced propulsion development and sustained mission funding. It is not evidence that every advanced concept was technically ready, or that nuclear propulsion had been demonstrated impossible.

### Research followed expectations about future missions

NASA's [NERVA history](https://www.nasa.gov/rocket-systems-area-nuclear-rockets/) describes its intended uses in long-range Mars missions and possible Apollo upper stages. Those expectations supported research into a broad range of nuclear engines. Once fission was available as a heat source, investigating liquid fuel, vapor, radiation transfer and alternative propellant arrangements was a natural theoretical extension. Identifying a promising operating point was much cheaper than completing an engine qualification program; the resulting literature contains both substantial experiments and much more speculative designs.

The dates also reflect successive funding opportunities rather than one uninterrupted research campaign. The 1989 Space Exploration Initiative prompted another wave of advanced-propulsion studies: [NASA's 1994 review](https://ntrs.nasa.gov/citations/19940018595) explicitly links renewed nuclear propulsion interest to the proposed human Mars program. Therefore a 1990s design can be a revival or reassessment of an earlier idea rather than the last step of decades of continuous development.

### The intended customers did not become an operating transport system

NASA's [history of human Mars planning](https://www.nasa.gov/wp-content/uploads/2023/04/sp-4521.pdf) records the budget dispute over whether a nuclear rocket was worthwhile without commitment to the much larger Mars program. It describes the termination of the remaining nuclear rocket program in the FY1974 budget alongside the contraction of human spaceflight plans. This supports a narrower conclusion than “politicians cancelled a finished Mars engine”: the technology was advancing, but its mission, flight-development funding and larger transportation architecture were not secured.

My mission-level interpretation is that nuclear thermal propulsion faced a difficult business case for the missions actually purchased. Better exhaust velocity can reduce propellant needs, especially for demanding transfers. It does not automatically compensate for reactor mass, hydrogen tanks, development expenditure, qualification, or launch integration on a small or infrequent mission. Chemical propulsion, gravity assists, and solar-electric propulsion can be preferable when they already satisfy the customer's requirements. A crewed Mars transport program or frequent heavy interplanetary traffic changes that tradeoff substantially.

This creates a recurring dependency: mission planners hesitate to depend on an unqualified engine, while engine developers struggle to obtain qualification funding without a committed mission. That is an interpretation of the program history, not a universal explanation for every cancellation.

### Ground-test success still left flight-specific work

NASA documents historical fuel erosion/cracking and modern work on fuel manufacture, integrated engine design and an acceptable ground-test approach. [NASA NTP development account](https://www.nasa.gov/directorates/stmd/tech-demo-missions-program/nuclear-thermal-propulsion-game-changing-technology-for-deep-space-exploration/)

The [2021 National Academies assessment](https://www.nationalacademies.org/read/25977/chapter/4) identifies reactor development and long-duration hydrogen storage among the remaining challenges. Hydrogen must be stored near 20 K; maintaining a large inventory over a long mission is different from supplying a ground test from nearby tanks. Whole-vehicle requirements also include shielding, post-shutdown heat removal, restarts, structural integration and launch qualification. Demonstrating specific impulse and reactor power does not independently validate these mission requirements.

Nor does a several-decade hiatus preserve a manufacturing program. NASA's [facility history](https://www.nasa.gov/rocket-systems-area-final-years/) records the January 1973 cancellation, mothballing of facilities and dispersal of personnel. Reports preserve measurements and methods, but they do not preserve an active supply chain, experienced production team or qualified flight article. Modern materials and computation help; the capability must still be rebuilt and validated.

### Nuclear hardware has flown, and development has not entirely stopped

The United States launched the SNAP-10A fission reactor in 1965 for electrical power. That was not a NERVA-style engine heating propellant directly. [DOE SNAP overview](https://www.energy.gov/etec/system-nuclear-auxiliary-power-snap-overview)

As of this review, NASA's [Space Reactor-1 Freedom page](https://www.nasa.gov/mission/space-reactor-1-freedom/) targets a late-2028 nuclear-electric propulsion demonstration beyond Earth orbit. It is a future mission target, not a flight result, and it follows a different energy-conversion path from nuclear thermal propulsion. NASA also describes continuing [propulsion technology maturation](https://www.nasa.gov/space-technology-mission-directorate/tdm/space-nuclear-propulsion/). Avoid characterizing the entire field as abandoned since 1973.

For Terra Invicta, the useful interpretation is an accelerated investment scenario. Its sustained demand for interplanetary transport and conflict supplies a motivation that historical civil programs repeatedly lacked. That makes development of dormant concepts plausible within the setting, while their masses, efficiencies, lifetimes and development dates still require scrutiny. A number taken from a serious concept paper is a defensible speculative anchor; it is not automatically an accurate prediction of an operational engine.

## Research limits

Several NASA and publisher full-text endpoints failed during this pass. Where that occurred, the analysis uses searchable primary-source excerpts, abstracts and institutional reports, and avoids asserting an unverified whole-engine temperature profile. The negative finding about an above-2,000 C molten-core demonstration is scoped accordingly. Existing unrelated research and gameplay files were not altered.
