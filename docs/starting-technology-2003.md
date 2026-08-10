# Starting Technology and Project Audit: 2003

Status: audited against Terra Invicta 1.0.51 and Dark Skies; no 2003 start
override is implemented by the mod

## Scenario start

The Dark Skies scenario begins on 31 March 2003 with no completed global
technologies. Its three active global-research slots contain bespoke
`Millennium` variants rather than the standard modern-start roots:

| Active technology | Category | Cost | Native scenario role |
|---|---|---:|---|
| Democratization of Space | Space Science | 15,000 | Repairs early space infrastructure and observation capacity. |
| Exascale Computing | Information Science | 15,000 | Repairs environmental investment and Materials, Life, and Energy research. |
| Digital Society | Social Science | 15,000 | Repairs control capacity, Influence, Investigation, and Social/Information research. |

`WeAreNotAlone` is a technology-tree UI root but is neither active nor completed
at campaign creation. It remains separately researchable at 2,500 points in the
2003 variant.

## What completing the active technologies does

### Democratization of Space

- extends shared alien-space-asset tracking to Mars;
- restores Boost, Mission Control, and Spaceflight Program investment output;
- adds 50% Space Science and 30% Military Science research;
- directly opens the 2003 variants of Mission to Space (1,000), Advanced
  Chemical Rocketry (1,000), and Skywatch (2,000).

### Exascale Computing

- restores 0.5 Environment-priority output;
- adds 30% Materials, Life, and Energy research;
- directly opens Advanced Carbon Manipulation (1,000) and Biotechnology
  (1,500);
- contributes to Principles of Space Warfare after Orbital Shipbuilding and to
  the Research Grants: Communication project after Arrival Mass Communications.

### Digital Society

- removes a ten-point control-management penalty;
- adds one Investigation to current councilors and the recruit pool;
- restores 25% Influence from public opinion;
- adds 30% Social and Information Science research;
- directly opens Augmented Reality (1,500), while Independence Movements and
  Arrival Psychology retain their additional prerequisites;
- contributes to Their Movements after Their Operations.

## Starting projects

The scenario completes six projects:

1. Solid-Fuel Space Rockets
2. Liquid-Fuel Rockets
3. Cryogenic Liquid-Fuel Rockets
4. Platform Core
5. Solar Collector
6. Space Science Lab

Unlike the mod's 2022 and 2026 starts, 2003 does not begin with Life Science
Lab, Materials Lab, or Reusable Rockets. Reusable Rockets cannot enter its
normal unlock pool until Democratization of Space and then Advanced Chemical
Rocketry are completed.

## Alien pacing associated with the technology reset

The start gives the aliens a 15-year quiet period, a 20-year setup duration,
0.75x progression, and -5 starting progression years. The five initial alien
councilor fleets use the same fleet IDs as the modern scenarios, but strategic
progression is deliberately delayed while humanity completes the expensive
opening technologies.

## Economic Equalization compatibility finding

The mod does not override `2003Start`. Its soft global-technology-selection
patch remains usable because it scores the candidate templates' own cost,
category, role, tier, and effects rather than consulting the economy-weight
catalog.

The economy technology catalog covers the 149 base technology IDs but none of
the three Dark Skies-only starting IDs. Completing Democratization of Space,
Exascale Computing, or Digital Society therefore applies all native Dark Skies
effects but currently contributes no Economic Equalization productivity,
labor-substitution, or resource-substitution weight. Base technologies reached
after them resume normal catalog progress. This is an identified compatibility
gap, not silently treated as balanced behavior.

