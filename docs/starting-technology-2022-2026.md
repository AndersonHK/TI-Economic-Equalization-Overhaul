# Starting Technology and Project Audit: 2022 and 2026

Status: implemented and authoritative

Game-data baseline: Terra Invicta 1.0.51

## Decision

The 2026 scenario completes `Skywatch` and replaces it in the three active
global-research slots with `DeepSystemSkywatch`. The 2022 scenario remains
unchanged.

`DeepSystemSkywatch` is the direct 1,000-point successor to `Skywatch`. It
preserves the Space Science and faction-objective progression lane while making
the 2026 start distinct without granting an unrelated military, economic, or
shipbuilding technology. Its completion supplies `Effect_DeepSkywatch` and
`Effect_SpaceScan`.

The intended resolved starts are:

| Scenario | Active global research | Completed global technologies |
|---|---|---|
| 2022 | `Skywatch`; `WeAreNotAlone`; `OutpostHabs` | `MissionToSpace`; `AdvancedChemicalRocketry` |
| 2026 | `DeepSystemSkywatch`; `WeAreNotAlone`; `OutpostHabs` | `MissionToSpace`; `AdvancedChemicalRocketry`; `Skywatch` |

## Replacement alternatives

All alternatives below are legal from the technologies completed at the 2026
start. The selected option is listed first.

| Technology | Cost | Direct prerequisite | Balance character |
|---|---:|---|---|
| `DeepSystemSkywatch` | 1,000 | `Skywatch` | Direct continuation; preserves the original research lane. |
| `DeepSpacePropulsionConcepts` | 1,000 | `MissionToSpace` | Accelerates early drives, reactors, and heat-management development. |
| `OrbitalShipbuilding` | 1,000 | `MissionToSpace` | Strongest practical acceleration; opens warship and orbital-construction projects. |
| `SpaceTourism` | 1,000 | `MissionToSpace` | Civilian/economic space emphasis with a narrower early payoff. |
| `AdvancedMagnetics` | 1,000 | none | Broad energy and weapon-development branch; least connected to completing Skywatch. |

`MissiontoMars` is not a valid immediate replacement even though `Skywatch` is
completed: it also requires `OutpostHabs`, which is still active research rather
than completed technology at campaign creation.

## Projects completed at both starts

The eight original scenario-granted projects have no global-technology
prerequisite: their prerequisites are either empty or other projects. The mod
also grants Reusable Rockets because both starts complete its sole prerequisite,
`AdvancedChemicalRocketry`.

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

## Projects gated by the 2022 completed technologies

The 2022 start completes `MissionToSpace` and `AdvancedChemicalRocketry`.
Those technologies participate in the following direct project prerequisites.
Project availability remains subject to the template's faction and unlock-chance
rules after all prerequisites are satisfied.

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

Because Platform Core and Cryogenic Liquid-Fuel Rockets are scenario-completed,
the prerequisite sets for Quarters and Improved Interplanetary Rockets are
already satisfied at both starts. Proxy Support Channel and Bootstrap
Spaceflight Programs still await their other prerequisite.

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
