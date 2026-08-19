# Fusion technology tree rebalance

Status: implementation authority for the late-game fusion prerequisite graph.

Game-data baseline: Terra Invicta 1.0.51

## Decision

`DeuteriumTritiumFusion` moves before `NuclearFusioninSpace`, whose localized
name is Nuclear Fusion Methodologies. It retains its installed authored research
cost of 50,000, its Energy category, its Space Development AI role, and both of
its permanent global effects. It becomes an AI-critical technology and directly
requires the three technologies that previously unlocked Nuclear Fusion
Methodologies:

- `AdvancedSuperconductors`;
- `NuclearFissioninSpace`; and
- `AdvancedHeatManagementConcepts`.

`NuclearFusioninSpace` retains its installed authored cost of 50,000 and its
global fusion-technology effect. Its sole prerequisite becomes
`DeuteriumTritiumFusion`. This establishes the sequence from a practical
deuterium-tritium fuel cycle and tritium-breeding blanket to the systematic
development of multiple fusion-confinement architectures.

`DeuteriumDeuteriumFusion` retains its installed authored cost of 75,000 and
both permanent global effects. Its former direct `DeuteriumTritiumFusion`
prerequisite is replaced by all five approved fusion-method technologies, while
its vanilla `Superalloys` requirement remains direct:

- `MagneticPlasmaConfinementTechniques`;
- `ElectrostaticPlasmaConfinement`;
- `InertialPlasmaConfinementTechniques`;
- `Tokamaks`; and
- `ZPinchTechniques`; and
- `Superalloys`.

Terra Invicta combines a global technology's `prereqs` list with AND semantics,
so all five methods and Superalloys are required. Tokamaks and Z-Pinch
Techniques already require Magnetic Plasma Confinement and Nuclear Fusion
Methodologies, while the other method branches preserve their installed
auxiliary prerequisites. The result therefore keeps Deuterium-Tritium Fusion
and Nuclear Fusion Methodologies transitively mandatory, while preserving
Superalloys as D-D Fusion's direct material-science gate.

EEO's global 2.0 research-cost multiplier produces displayed costs of 100,000
for Deuterium-Tritium Fusion, 100,000 for Nuclear Fusion Methodologies, and
150,000 for Deuterium-Deuterium Fusion.

## Localization

The installed Nuclear Fusion Methodologies description predicts that practical
deuterium-tritium reactors will follow the methodologies technology. The mod
replaces that description so it instead treats the completed D-T fuel cycle as
the experimental foundation from which magnetic, electrostatic, inertial,
tokamak, and Z-pinch systems are developed.

The installed Deuterium-Tritium Fusion and Deuterium-Deuterium Fusion text
remains compatible. D-T describes positive net energy and tritium breeding,
while D-D describes the combined confinement knowledge now represented by its
five prerequisites.

## Downstream projects and modules

No faction-project or module prerequisite is changed. Moving D-T earlier makes
its direct projects eligible at the new point in the global tree, subject to
their other prerequisites and normal unlock rolls. These include the Fusion
Pile, Civilian Fusion Reactors, and Muon Spiker. Mirror Cell Fusion Reactor I
also becomes eligible when Magnetic Plasma Confinement is complete. The other
first-generation fusion power plants continue to require D-T plus their
installed confinement-method technology or technologies.

## Technology-tree layout

The game derives node placement from prerequisites and costs; technology
templates do not expose authored screen coordinates. The intended full-tree
result is:

- Deuterium-Tritium Fusion moves one column earlier and appears above Coilguns;
- Nuclear Fusion Methodologies follows it in the next column; and
- Deuterium-Deuterium Fusion remains downstream of all five method branches and
  directly requires Superalloys.

Exact vertical placement and connection-line readability require manual
in-game verification because they are produced by the runtime tree-layout
algorithm.

## Verification contract

Automated verification pins the installed 1.0.51 costs, effects, AI metadata,
and prerequisite baselines before checking the three exact overrides. It also
checks that every referenced method exists and that the resulting technology
graph is acyclic. Manual testing must confirm the full-tree placement, selected
technology Requirements and Unlocks panels, path-cost display, and existing-save
behavior for already completed technologies.
