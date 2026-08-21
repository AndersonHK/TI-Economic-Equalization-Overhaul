# Starting Technology and Project Audit: 2022 and 2026

Status: implemented and authoritative

Game-data baseline: Terra Invicta 1.0.51

## Decision

Both the 2022 and 2026 scenarios complete `SpaceTourism`,
`DeepSpacePropulsionConcepts`, and `AugmentedReality`. The propulsion and
tourism technologies require only `MissionToSpace`, which is already completed
at each start; `AugmentedReality` has no global-technology prerequisite.

`SpaceTourism` represents the established commercial and institutional concept,
not construction of a dedicated orbital hotel. The separately researched
Tourist Berth project continues to represent implementation of permanent
commercial tourist accommodation.

`DeepSpacePropulsionConcepts` is an umbrella technology that supplies no ship
parts. It represents the established feasibility and systems-study foundation
for electrothermal, electrostatic, and electromagnetic propulsion. The branch
technologies and their drive projects remain researchable because they carry
the unresolved problem of scaling real tens-of-kilowatts systems into the
multi-megawatt ship components represented by the game.

`AugmentedReality` represents the established ability to overlay contextual
digital information on human vision for industrial, communications, and
military work. HoloLens shipped before 2022, HoloLens 2 entered commercial and
industrial use in 2019, and the U.S. Army was testing the HoloLens-derived IVAS
system by the 2022 start. Later consumer headsets improve usability and market
reach but are not required to establish the underlying technology. Completing
the global technology does not complete any of its dependent faction projects.
The mod adds the native `Effect_IncreaseMaxArmyTechLevel` effect, increasing the
maximum Military technology level for all human nations by 0.25. This represents
the force-wide ceiling created by networked sensors and communications,
helmet-mounted displays, augmented command interfaces, and contemporary drone
warfare; it raises the attainable ceiling without instantly modernizing any
nation's fielded equipment. To price that additional strategic value without
overtaking its direct successor, the mod raises `AugmentedReality`'s authored
research cost from 1,500 to 2,000. The global 2.2 research-cost multiplier makes
the in-game cost 4,400, still below `SpaceResearch` at 5,500. Both modern starts
already complete `AugmentedReality`, so the new price primarily affects the 2003
and custom research paths rather than their opening state.

The 2026 scenario additionally completes `Skywatch`, `OutpostHabs`, and
`MissiontotheMoon`. It continues the first lane with `DeepSystemSkywatch` and
replaces the completed lunar mission slot with `MissiontoMars`.

`DeepSystemSkywatch` is the direct 1,000-point successor to `Skywatch`. It
preserves the Space Science and faction-objective progression lane while making
the 2026 start distinct without granting an unrelated military, economic, or
shipbuilding technology. Its completion supplies `Effect_DeepSkywatch` and
`Effect_SpaceScan`.

`OutpostHabs` represents a credible first-generation architecture for sustained
off-world habitation, not an already constructed lunar base. By the 2026 start,
the completed Artemis II crewed lunar flight and the accumulated robotic lunar
landing, surface-operations, resource-survey, navigation, and communications
programs justify treating the global design foundation as established. The
separate `Project_OutpostCore` is scenario-completed for every faction at both
modern starts, representing operational small-outpost engineering without
constructing a base or consuming Mission Control. `MissiontotheMoon` is
completed in 2026, while `MissiontoMars` becomes the active continuation.

The intended resolved starts are:

| Scenario | Active global research | Completed global technologies |
|---|---|---|
| 2022 | `Skywatch`; `WeAreNotAlone`; `OutpostHabs` | `MissionToSpace`; `AdvancedChemicalRocketry`; `SpaceTourism`; `DeepSpacePropulsionConcepts`; `AugmentedReality` |
| 2026 | `DeepSystemSkywatch`; `WeAreNotAlone`; `MissiontoMars` | `MissionToSpace`; `AdvancedChemicalRocketry`; `SpaceTourism`; `DeepSpacePropulsionConcepts`; `AugmentedReality`; `Skywatch`; `OutpostHabs`; `MissiontotheMoon` |

## 2026 active-research replacements

`DeepSystemSkywatch` replaces completed `Skywatch`. `MissiontotheMoon` is also
completed, and `MissiontoMars` occupies its former active slot. The installed
Mars technology costs 2,500 before the global multiplier and requires
`OutpostHabs` plus `Skywatch`; both prerequisites are completed in 2026. Its
opening effective cost is therefore 5,500. This advances the scenario from an
established lunar program toward the next crewed deep-space destination.

The alternatives below apply specifically to the slot opened by completing
`Skywatch`. The selected option is listed first.

| Technology | Cost | Direct prerequisite | Balance character |
|---|---:|---|---|
| `DeepSystemSkywatch` | 1,000 | `Skywatch` | Direct continuation; preserves the original research lane. |
| `OrbitalShipbuilding` | 1,000 | `MissionToSpace` | Strongest practical acceleration; opens warship and orbital-construction projects. |
| `AdvancedMagnetics` | 1,000 | none | Broad energy and weapon-development branch; least connected to completing Skywatch. |

`SpaceTourism` and `DeepSpacePropulsionConcepts` are no longer replacement
options because they are completed before the active research slots are
resolved.

## Projects completed at both starts

The eight original scenario-granted projects have no global-technology
prerequisite: their prerequisites are either empty or other projects. The mod
also grants Reusable Rockets because both starts complete its sole prerequisite,
`AdvancedChemicalRocketry`, and grants Outpost Core as an established modern
small-surface-hab engineering package.

| Completed project | Project prerequisites |
|---|---|
| Solid-Fuel Space Rockets | none |
| Liquid-Fuel Rockets | Solid-Fuel Space Rockets |
| Cryogenic Liquid-Fuel Rockets | Liquid-Fuel Rockets |
| Platform Core | none |
| Life Science Lab | Platform Core, or Outpost Core as the alternative prerequisite |
| Materials Lab | Platform Core, or Outpost Core as the alternative prerequisite |
| Solar Collector | Platform Core, or Outpost Core as the alternative prerequisite |
| Space Science Lab | Platform Core, or Outpost Core as the alternative prerequisite |
| Reusable Rockets | `AdvancedChemicalRocketry` |
| Outpost Core | `OutpostHabs`; scenario completion intentionally bypasses the normal unlock timing |

## Projects and technologies gated by the shared completed technologies

Both starts complete `MissionToSpace`, `AdvancedChemicalRocketry`,
`SpaceTourism`, `DeepSpacePropulsionConcepts`, and `AugmentedReality`. Those technologies
participate in the following direct prerequisites. Project availability remains
subject to the template's faction and unlock-chance rules after all
prerequisites are satisfied.

| Completed technology | Project | Other required prerequisite |
|---|---|---|
| `MissionToSpace` | Quarters | Platform Core, or Outpost Core as the alternative prerequisite |
| `MissionToSpace` | Proxy Support Channel | Hydra Diplomacy |
| `AdvancedChemicalRocketry` | High Thrust Probes | none |
| `AdvancedChemicalRocketry` | Reusable Rockets | none; scenario-completed by the mod |
| `AdvancedChemicalRocketry` | Rocket Scientists | none |
| `AdvancedChemicalRocketry` | Space Tugs | none |
| `AdvancedChemicalRocketry` | Superheavy Rockets | none |
| `AdvancedChemicalRocketry` | Improved Interplanetary Rockets | Cryogenic Liquid-Fuel Rockets |
| `AdvancedChemicalRocketry` | Bootstrap Spaceflight Programs | Arrival Economics |
| `SpaceTourism` | Tourist Berth | Platform Core |
| `SpaceTourism` | Xenotourism | Xenofauna Harmony |

Because Platform Core and Cryogenic Liquid-Fuel Rockets are scenario-completed,
the prerequisite sets for Quarters and Improved Interplanetary Rockets are
already satisfied at both starts. Proxy Support Channel and Bootstrap
Spaceflight Programs still await their other prerequisite.

`DeepSpacePropulsionConcepts` directly opens the global technologies
Electrothermal Propulsion, Electromagnetic Propulsion, Advanced Heat Management
Concepts, Nuclear Fission in Space, and—after their additional prerequisites—
Electrostatic Propulsion and Magnetic Nozzles. It also contributes to Mission
to the Asteroids. It does not directly complete or grant a ship component.

`AugmentedReality` directly opens the global technologies `Cybernetics` and
`SpaceResearch` and contributes to faction projects including Augmented
Learning, Augmented Combat Training, Operations Center, Damage Control Drones,
and Interrogation Techniques. Those projects retain their other prerequisites,
unlock chances, and faction research costs. The technology also carries the
native all-nations `Effect_IncreaseMaxArmyTechLevel` effect, valued at 0.25.

In the 2026 start, completed `OutpostHabs` directly opens
`MissiontotheMoon`, `MissiontoMars`, and `SpaceMiningandRefining`; the scenario
completes the lunar mission and begins the Mars mission. Both modern starts
scenario-complete `Project_OutpostCore`, bypassing its normal 90% initial unlock
roll and 300-point faction-research cost.

## Post-implementation review of other 2022–2026 changes

The installed 1.0.51 technology graph contains several real-world developments
worth tracking, but the audit found no additional clean 2026 starting-roster
grant. In most cases the historical milestone is narrower than the game
technology, or the game's prerequisite chain assumes a different development
path.

| Technology or branch | 2022–2026 milestone | Starting-roster finding |
|---|---|---|
| `AdvancedNeuralNetworks` | Large multimodal neural networks demonstrated broad text, image, and professional-task performance beginning in 2023. | Strongest 2026-only candidate by description. Do not grant yet: the game requires `PhotonicComputing`, while practical modern AI remains electronic rather than dependent on a completed all-photonic computing transition. This needs a prerequisite/design decision, not a roster-only edit. |
| `Biotechnology` | The FDA approved Casgevy in December 2023, the first approved CRISPR/Cas9 therapy. | Important 2026 milestone, but the generic Biotechnology field was already mature in 2022. Treating this as a completed technology would be more defensible for both starts than for 2026 alone. |
| `AugmentedReality` | HoloLens shipped in 2016, HoloLens 2 shipped in 2019, and the Army tested the HoloLens-derived IVAS system before and during 2022. Later consumer systems broadened adoption. | Completed in both starts: the game's industrial, communications, and military sensory-overlay threshold had already been crossed by 2022. |
| `OutpostHabs` | Artemis II completed its crewed lunar flight in April 2026 after several years of robotic lunar landing, surface-operations, and resource-survey missions. | Completed only in 2026 as a global architecture. Outpost Core is scenario-completed in both modern starts as practical small-outpost engineering; Mission to the Moon is completed in 2026 and Mission to Mars becomes active. |
| `HighEnergyLasers` | A tactically relevant 300 kW-class laser was delivered to the U.S. Department of Defense on 15 September 2022. | The milestone predates the modern campaign start and therefore does not distinguish 2026. The technology also directly exposes Laser Engine and E-Beam Drive projects, so completing it carries more hardware significance than an umbrella concept grant. |
| Fusion branches | Lawrence Livermore achieved laboratory fusion ignition on 5 December 2022 and repeated higher-yield shots afterward. | A major post-start scientific milestone, but far short of `NuclearFusioninSpace`, which assumes complete space reactors, power conversion, heat rejection, and propulsion integration. Highlight only; do not complete. |
| Asteroid mission technologies | DART changed an asteroid's orbit on 26 September 2022; OSIRIS-REx returned a Bennu sample on 24 September 2023. | These materially strengthen planetary-defense and asteroid-operations experience, but do not satisfy the game's `MissiontotheAsteroids` package of Mars access, space mining, and deep-space propulsion. |
| Electric and nuclear-electric propulsion | Gateway PPE hardware reached integration, a 120 kW lithium MPD thruster fired in 2026, and NASA selected the SR-1 Freedom fission-electric Mars pathfinder. | These support the now-completed `DeepSpacePropulsionConcepts`. They do not justify completing its hardware-bearing propulsion branches or `NuclearFissioninSpace`; the flight demonstrations remain future and far below the game's multi-megawatt drives. |
| Reusable launch systems | SpaceX caught a returning Super Heavy booster in 2024. | Already represented: Reusable Rockets is completed in both starts, so no further change is required. |

Primary references: [GPT-4 technical report](https://arxiv.org/abs/2303.08774),
[FDA Casgevy approval](https://www.fda.gov/news-events/press-announcements/fda-approves-first-gene-therapies-to-treat-patients-sickle-cell-disease),
[Microsoft HoloLens 2 shipping](https://news.microsoft.com/source/features/innovation/hololens-2-shipping-to-customers/),
[U.S. Army IVAS testing](https://www.army.mil/article/259714/soldiers_test_integrated_augmented_reality_tech_with_stryker_vehicles),
[NASA Artemis II completion](https://www.nasa.gov/news-release/nasa-welcomes-record-setting-artemis-ii-moonfarers-back-to-earth/),
[NASA CLPS lunar surface campaign](https://www.nasa.gov/missions/artemis/clps/fourth-launch-of-nasa-instruments-planned-for-near-moons-south-pole/),
[300 kW laser delivery](https://news.lockheedmartin.com/2022-09-15-Lockheed-Martin-Delivers-Its-Highest-Powered-Laser-to-Date-to-US-Department-of-Defense),
[LLNL fusion ignition](https://www.llnl.gov/news/ignition),
[NASA DART results](https://www.nasa.gov/science-research/planetary-science/from-impact-to-innovation-a-year-of-science-and-triumph-for-historic-dart-mission/),
[NASA OSIRIS-REx](https://science.nasa.gov/mission/osiris-rex/),
[NASA 2026 MPD test](https://www.nasa.gov/missions/tech-demonstration/nasa-fires-up-powerful-lithium-fed-thruster-for-trips-to-mars/), and
[NASA SR-1 Freedom](https://www.nasa.gov/mission/space-reactor-1-freedom/).

## Projects unlocked by default without technologies

Four general human-faction projects are genuinely available by default without
a technology, project, objective, milestone, nation, scenario, or faction
prerequisite. Each has a 100% initial unlock chance and is repeatable:

| Project | Cost | Function |
|---|---:|---|
| Commercial Research | 100 | Grants 100 Money. |
| Audience Research | 100 | Grants 25 Influence. |
| Operations Research | 100 | Grants 20 Operations. |
| Management Research | 1,500 | Grants `Effect_ControlPointMaintenanceBonus3`. |

Other projects with no technology or project prerequisite are not default
human unlocks: Alien Master Project is restricted to the alien faction; Their
Signatures, Their Methods, Their Operations, Hydra Biology, Hydra Language, and
A New Home require campaign objectives and sometimes a specific faction; Alien
Flora requires the Detect Xenoforming milestone. Platform Core and Solid-Fuel
Space Rockets have no prerequisites but a 0% initial unlock roll in the base
templates; the 2022 and 2026 scenario templates bypass that roll by marking both
projects completed.
