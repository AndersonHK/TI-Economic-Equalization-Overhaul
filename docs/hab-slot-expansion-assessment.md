# Hab Facility Capacity Beyond 20

## Current structural limit

Vanilla station state is organized as five sectors:

- internal sector 0: core plus four facilities;
- internal sectors 1–4: four facilities each.

That produces twenty facility slots plus the core. The limit is not merely a
balance constant:

- `TIHabState.maxSectors` and `maxSectorIdx` are literal `5` and `4`;
- `TIHabState.InitializeSector` creates five module states for the core sector
  and four for every outer sector;
- `HabitatsScreenController.PreviewStation` iterates five sectors, four outer
  slots, and five core slots;
- `TISectorState.UpdateModuleConnectorMap` contains explicit sector and slot
  topology;
- the station UI asset serializes cells only for `S0_M0` through `S0_M4` and
  `S1_M0` through `S4_M3`;
- station models contain a fixed set of module controllers and attachment
  transforms, which `HabModelController` discovers from the prefab.

Lists of sectors and modules are serialized dynamically, so save storage is not
the main obstacle. The fixed UI and model topology is.

## Practical expansion path: 24 facilities

The least invasive design is one additional facility in each existing outer
sector. It preserves five sectors and raises capacity from twenty to
twenty-four facilities.

Required work:

1. Set outer-sector capacity to five and append one empty
   `TIHabModuleState` during new-hab initialization and idempotent save repair.
2. Clone and place one `StationGridCell` per outer sector at runtime, using
   deterministic names `S1_M4` through `S4_M4`.
3. Extend `PreviewStation` and module-selection paths from four to five outer
   slots.
4. Define a symmetric connector topology for each new slot.
5. Clone or supply one additional module attachment transform and
   `HabModuleController` per outer sector in the station model.
6. Audit construction, decommissioning, combat targeting, destruction,
   capture, AI placement, and save/load with the fifth outer module present.

The state and ordinary construction paths are mostly list-driven. The highest
risk is visual/model integration: a slot that exists in state but lacks a UI
cell or 3D attachment controller can become invisible or unselectable.

## Adding sixth or later sectors

Adding sectors rather than slots would require replacing the fixed five-sector
model across:

- `maxSectors`/`maxSectorIdx`;
- ring and connection structures;
- sector-number switches;
- template layouts and UI keys;
- station preview artwork;
- 3D station prefabs and sector controllers;
- connector routing and combat representation.

This is substantially harder and should not be the first expansion approach.

## Confidence

A symmetric 24-facility prototype is feasible as a dedicated, test-heavy
feature slice. It is broader and riskier than the T1-sector work because it
must create new UI and 3D attachment points rather than reveal existing ones.

Arbitrary capacity or additional sectors remains a high-difficulty project.
The recommended sequence is:

1. prototype one fifth slot on one outer sector;
2. verify build, selection, save/load, and combat behavior;
3. generalize symmetrically to all four outer sectors;
4. only then consider more than twenty-four facilities.
