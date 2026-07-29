# Localization and unlock audit: early power plants

Last reviewed: 2026-07-29  
Game data: Terra Invicta 1.0.49 installed templates and English localizations

This is a read-only audit of the module descriptions, faction projects, and
global technologies associated with Solid Core Fission Reactor II–V, Compact
Solid Core Fission Reactor I–V, Fuel Cell II–III, and the first added drive
slice. It records what the game actually says before any balance values are
changed.

## Files checked

- `Templates/TIPowerPlantTemplate.json`
- `Templates/TIProjectTemplate.json`
- `Templates/TITechTemplate.json`
- `Localization/en/TIPowerPlantTemplate.en`
- `Localization/en/TIProjectTemplate.en`
- `Localization/en/TITechTemplate.en`

## Solid Core Fission Reactor II–V

### Module descriptions

Reactor II, III, IV, and V all use exactly the same localized description as
Reactor I:

> An adapted nuclear power plant that operates at around 2,500 degrees Celsius.

There is no localized claim that a later tier operates at a higher
temperature, changes coolant, adopts a different conversion cycle, uses a new
fuel, or ceases to be an adapted terrestrial plant.

| Module | Maximum output | Specific mass | Efficiency | Crew | Required project |
|---|---:|---:|---:|---:|---|
| Solid Core Fission Reactor I | 2 GW | 40 t/GW | 75.0% | 6 | Solid Core Fission Reactor I |
| Solid Core Fission Reactor II | 6 GW | 34 t/GW | 77.5% | 6 | Solid Core Fission Reactor II |
| Solid Core Fission Reactor III | 20 GW | 28 t/GW | 80.0% | 6 | Solid Core Fission Reactor III |
| Solid Core Fission Reactor IV | 60 GW | 12 t/GW | 82.5% | 6 | Solid Core Fission Reactor IV |
| Solid Core Fission Reactor V | 125 GW | 8 t/GW | 85.0% | 6 | Solid Core Fission Reactor V |

### Projects and global technology

The project-localization entries do not supply separate technical
descriptions. Every project from I through V has the summary `<shipmodule>`,
which directs the interface back to the associated ship-module information.

The actual prerequisite chain is:

`Solid Core Fission Systems` global technology  
→ `Solid Core Fission Reactor I` project  
→ `Solid Core Fission Reactor II` project  
→ `Solid Core Fission Reactor III` project  
→ `Solid Core Fission Reactor IV` project  
→ `Solid Core Fission Reactor V` project

Only Reactor I directly requires a global technology. Reactors II–V require
only the preceding faction project; they are not attached to four later global
technologies with new physical assumptions.

The full **Solid Core Fission Systems** technology description establishes:

- solid uranium-dioxide or plutonium-oxide fuel;
- a sustained chain reaction in zero gravity;
- heat captured by a pressurized-water cooling system designed for space;
- use in habitats and spacecraft;
- molten-core and gas-core fuels as explicitly future, separate systems.

It does not mention a 2,500 °C temperature, a Brayton or Rankine conversion
cycle, turbine inlet temperature, electrical efficiency, or an efficiency
improvement path. The 2,500 °C claim comes exclusively from the ship-module
description and remains unchanged across I–V.

### Balance implication

The localization supplies no new thermodynamic context that could exempt the
later tiers from the same temperature limit applied to Reactor I. The template
efficiency rises from 75% to 85% while the stated operating temperature remains
2,500 °C.

With the game's 800 K Aluminum Fin radiator:

`ηCarnot = 1 - 800 / 2773.15 = 71.15%`

All five listed efficiencies exceed that ceiling. Later tiers may reasonably
become lighter, larger, more reliable, or somewhat closer to the ceiling, but
the text does not establish a hotter source or colder sink that would permit
77.5–85% conversion efficiency.

The pressurized-water statement also deserves care. It can describe an
intermediate heat-transport loop whose temperature is below the reactor core,
but it does not make 2,500 °C water a credible working fluid. If anything, an
intermediate loop and finite heat exchangers lower the achievable
source-to-electricity efficiency.

## Fuel Cell II–III

### Module descriptions

Fuel Cell I, II, and III all use the same localized description:

> Alkaline fuel cells consume hydrogen and oxygen to produce power and are
> recharged by a solar array.

Fuel Cell III does not change to a different electrochemical system. The text
continues to describe an alkaline hydrogen/oxygen regenerative system coupled
to solar generation.

| Module | Maximum output | Specific mass | Efficiency | Crew | Required project |
|---|---:|---:|---:|---:|---|
| Fuel Cell I | 0.2 GW | 2,800 t/GW | 70% | 0 | none |
| Fuel Cell II | 0.8 GW | 450 t/GW | 70% | 0 | Fuel Cell II |
| Fuel Cell III | 1.5 GW | 120 t/GW | 72% | 0 | Fuel Cell III |

### Fuel Cell II unlock

The Fuel Cell II faction project directly requires the **Space Agriculture**
global technology.

The full technology text describes a Closed Ecological Life Support System:

- food and oxygen production using light, power, and biological processes;
- plants or algae converting crew carbon dioxide into oxygen;
- recycling urine and wastewater into drinking water;
- recycling human and food waste as plant nutrients;
- later expansion from algae to leafy plants and fish.

It does not mention fuel cells, hydrogen storage, electrolysis, photovoltaic
efficiency, or improved alkaline-cell materials. It provides a thematic
closed-loop life-support connection, particularly through water and oxygen,
but no numerical basis for the roughly 6.2-fold specific-mass improvement from
Fuel Cell I to II.

### Fuel Cell III unlock

The Fuel Cell III faction project requires both:

- the **Designer Life Forms** global technology; and
- the Fuel Cell II faction project.

The full technology text describes deliberate genome design for livestock,
terraforming flora, food, pharmaceuticals, pets, and organisms that make space
settlement more survivable and pleasant. It does not mention electrochemistry,
hydrogen, oxygen storage, solar arrays, or energy conversion.

The biological prerequisite may imply better closed-loop management of water
and oxygen, but it does not explain the module's roughly 3.75-fold
specific-mass improvement over Fuel Cell II or establish a new fuel-cell
chemistry. The module text remains alkaline hydrogen/oxygen and solar
recharged.

## Compact Solid Core Fission Reactor I–V

The installed data does contain all five compact reactors. Their internal
template and project identifiers continue the ordinary solid-core numbering:

| Displayed module | Power-plant template | Project |
|---|---|---|
| Compact Solid Core Fission Reactor I | `SolidCoreFissionReactorVI` | `Project_SolidCoreFissionReactorVI` |
| Compact Solid Core Fission Reactor II | `SolidCoreFissionReactorVII` | `Project_SolidCoreFissionReactorVII` |
| Compact Solid Core Fission Reactor III | `SolidCoreFissionReactorVIII` | `Project_SolidCoreFissionReactorVIII` |
| Compact Solid Core Fission Reactor IV | `SolidCoreFissionReactorIX` | `Project_SolidCoreFissionReactorIX` |
| Compact Solid Core Fission Reactor V | `SolidCoreFissionReactorX` | `Project_SolidCoreFissionReactorX` |

### Project and technology context

All five compact project summaries are `<shipmodule>`. None supplies a new
project-level technical explanation. Every compact module uses the same
localized description:

> A miniaturized fission reactor offers space and mass-savings for our ships.

The unlock chain is:

`Solid Core Fission Reactor II` project  
→ `Compact Solid Core Fission Reactor I` project  
→ `Compact Solid Core Fission Reactor II` project  
→ `Compact Solid Core Fission Reactor III` project  
→ `Compact Solid Core Fission Reactor IV` project  
→ `Compact Solid Core Fission Reactor V` project

There is no intervening global technology. Consequently, the only global-tech
context inherited by the compact line remains **Solid Core Fission Systems**:
solid uranium-dioxide or plutonium-oxide fuel and heat captured by a
pressurized-water cooling system designed for space. The compact text promises
miniaturization, not a hotter source, a colder sink, a different conversion
cycle, or a new fuel state.

| Module | Maximum output | Specific mass | Current efficiency | Crew |
|---|---:|---:|---:|---:|
| Compact I | 1.5 GW | 6 t/GW | 77.5% | 3 |
| Compact II | 5 GW | 5 t/GW | 80.0% | 3 |
| Compact III | 12 GW | 4 t/GW | 82.5% | 3 |
| Compact IV | 20 GW | 3 t/GW | 85.0% | 3 |
| Compact V | 20 GW | 2 t/GW | 87.5% | 3 |

The same thermodynamic criticism therefore applies to Compact I–V. The text
supports a mass and packaging improvement; it does not support efficiency
above the Carnot ceiling implied by the parent line's stated temperature and
the early radiators.

## Diana, Nova, Nerva, and Kiwi unlock context

### Chemical branch

**Advanced Chemical Rocketry** says scientists are exceeding the limits of
the current chemical-engine generation. Its full description still defines
the mechanism as fuel and oxidizer mixed and burned in a chamber. The claimed
improvements come from lighter materials, safer designs, and refining
propellants to make them more stable.

- **Superheavy Chemical Rockets**, which unlocks Diana, is summarized as new
  large-scale liquid-methane designs for heavy loads beyond Earth orbit.
- **Cryogenic-Fuel Space Rockets** investigates liquid hydrogen and other
  supercooled propellants.
- **Interplanetary Chemical Rockets**, which unlocks Nova after the cryogenic
  project, says only that new materials are being used to improve efficiency.

This establishes Diana as an advanced but ordinary methane/oxygen cluster.
Nova remains classified and described as chemical, but its module text adds
the extraordinary premise that "stabilized hydrogen fuel" produces otherwise
impossible efficiency. The project and global-tech text do not identify
metallic hydrogen or another specific phase.

### Nuclear-thermal branch

The Nerva and Kiwi project summaries are both `<shipmodule>`, so their only
drive-specific context comes from their module descriptions:

- Nerva: a solid-core fission design suitable for interplanetary journeys.
- Kiwi: a compact solid-core fission drive that exceeds chemical efficiency
  but lacks chemical thrust.

Nerva requires `Project_SolidCoreFissionReactorI`. Kiwi requires
`Project_SolidCoreFissionReactorVI`, which is Compact Solid Core Fission
Reactor I. Neither drive project adds a new global technology or changes the
solid-fuel, pressurized-water context inherited from **Solid Core Fission
Systems**.

## Audit conclusion

- **Solid Reactor II–V:** retain exactly the same 2,500 °C identity and receive
  no later global-tech text that changes their thermodynamic assumptions.
- **Fuel Cell II–III:** retain exactly the same alkaline hydrogen/oxygen,
  solar-recharged identity.
- **Compact Solid Reactor I–V:** all five exist; their projects add only a
  miniaturization claim and no new thermodynamic premise.
- **Diana:** remains an ordinary liquid-methane chemical rocket in both project
  and technology text.
- **Nova:** "stabilized hydrogen" is real localization, but neither its project
  nor its global technology identifies what stabilized state is intended.
- **Nerva and Kiwi:** inherit the same solid-core fission family; Kiwi is gated
  by Compact Solid Core I while Nerva is gated by ordinary Solid Core I.
- **Space Agriculture** supports a closed-loop life-support interpretation but
  does not establish improved solar or fuel-cell performance.
- **Designer Life Forms** is even further removed from power-generation
  engineering and supplies no numerical justification for Fuel Cell III.
- The large tier-to-tier improvements are therefore gameplay progression
  values, not extrapolations explicitly supported by the localized fiction.
