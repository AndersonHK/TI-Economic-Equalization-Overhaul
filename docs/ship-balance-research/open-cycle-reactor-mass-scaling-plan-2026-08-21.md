# Open-cycle reactor mass scaling and power UI plan

Status: temporary flat-`0.5` gameplay calibration implemented and deployed
2026-08-24; manual in-game verification pending.

Last reviewed: 2026-08-24

Implementation scope note: authored `maxOutput_GW` progression remains owned
by the separate reactor-output task and is unchanged here. This implementation
does change how drive output consumes that existing rated cap: electrical drive
output counts one-for-one and open-cycle thermal output counts by `s`.

## Objective

Separate a ship power plant's physical reactor output from the mass-sizing
burden associated with two different uses of that output:

1. direct thermal power sent through an open-cycle drive; and
2. thermal input converted into electricity for closed-cycle drives, ship
   systems, and weapons.

The open-cycle drive must continue to advertise its full installed thermal
demand. The new scaler reduces how strongly that thermal contribution sizes
plant mass, construction resources, reactor-bay occupancy, and rated-cap use.
It must not reduce thrust, falsify reactor thermal output, or alter the authored
cap value itself.

The same demand breakdown will correct four UI concepts:

- actual reactor thermal output and net electrical generation;
- drive demand and non-drive electrical demand;
- waste heat actually assigned to the radiators; and
- drive demand units: `GWth` for open-cycle drives and `GWe` for closed-cycle
  drives.

## Settled accounting model

Use one shared snapshot for every design-side output, mass, cost, volume, heat,
cap, and UI consumer. Keep the separately owned cap values isolated from the
temporary weighting policy. Do not continue encoding several meanings into
`TISpaceShipTemplate.drivePowerRequirement_GW`.

Let:

- `D` = installed, hull-scaled drive demand;
- `A` = useful electrical demand from ship systems and weapons;
- `eta` = bounded plant electrical-conversion efficiency;
- `r` = retained open-cycle heat fraction, currently `0.01`;
- `s` = the selected plant's open-cycle thermal mass scaler;
- `Qoc` = actual reactor thermal output assigned to an open-cycle drive;
- `Qe` = reactor thermal input assigned to electricity production;
- `Qtotal` = actual total reactor thermal output;
- `Pmass` = gross-reactor-equivalent power used to size plant mass, cost, and
  bay occupancy;
- `Cdrive` = drive demand charged against the plant's rated output cap;
- `Hrad` = steady waste heat assigned to the radiators before separately
  modeled module heat is added.

### Open-cycle drive

The drive's displayed demand remains its full thermal demand:

`drive display = D GWth`

Retain the implemented bleed model:

`etaOpen = 1 - r * (1 - eta)`

`Qoc = D / etaOpen`

`Hoc = Qoc - D`

The non-drive electrical side is:

`electrical display = A GWe`

`Qe = A / eta`

`He = Qe - A`

The physical and mass-sizing totals are deliberately different:

`Qtotal = Qoc + Qe`

`Pmass = s * Qoc + Qe`

`Cdrive = s * Qoc`

`Hrad = Hoc + He`

`Qtotal` is the physically meaningful thermal-output value exposed to the UI.
`Cdrive`, not `Qtotal` or the full `Pmass`, is compared with `maxOutput_GW` for
drive compatibility. At the temporary `s = 0.5`, a 4 GW rated cap can support
either 4 GWe of closed-cycle drive output or 8 GWth of open-cycle output before
the small retained-heat correction. Ship systems and weapons retain their
existing cap semantics.

### Closed-cycle drive

The drive's displayed demand is electrical:

`drive display = D GWe`

Combine it with other useful electrical loads:

`electrical display = D + A GWe`

`Qe = (D + A) / eta`

`Qoc = 0`

`Qtotal = Pmass = Qe`

`Cdrive = D`

`Hrad = Qe - (D + A)`

This corrects the current mixed convention in which systems and weapons are
grossed up by plant efficiency but closed-cycle drive demand is not.

### Total radiator load

`TISpaceShipTemplate.wasteHeat_GW` remains the radiator-sizing value. It is:

`total radiator load = Hoc + He + separately modeled module heat`

The last term currently includes the design-rate heat of powered weapons.
The UI must report the resulting total as **Waste heat to radiators**, not as
an unexplained generic waste-heat number. The power-plant tooltip should also
show the open-cycle bleed and electrical-conversion components when nonzero.

## Data model

Add a scenario-aware float extension to `TIPowerPlantTemplate.json`:

`openCycleThermalMassMultiplier`

Semantics:

- omitted: `1.0`, preserving vanilla/current mass behavior;
- positive and at most `1.0` in shipped data;
- malformed, zero, negative, NaN, or infinite: diagnose once and fall back to
  `1.0`;
- applied only when the selected drive has `openCycleCooling == true`.

Load it through the existing `TemplateFloatExtensionReader` pattern into a new
`PowerPlantScalingRegistry`, refreshed during template initialization. This
avoids changing the game's template class and keeps scenario-tag precedence
consistent with the existing propellant-density and gun-power extensions.

Interpret the multiplier as:

`desired direct-thermal t/GWth / ordinary plant specificPower_tGW`

### Temporary pre-rebalance gameplay calibration

Until reactor specific masses are rebalanced, every reactor class other than
`Fuel_Cell` and `Any_General` uses `s = 0.5`. Fuel-cell and general plants stay
at `1.0`. This deliberately gives direct/open-cycle propulsion a uniform 50%
mass, resource-cost, occupied-bay-volume, and rated-cap discount without
letting the currently uneven reactor progression amplify that discount
differently by technology path.

Solid Core I's live `240 t/GW` coefficient therefore becomes an effective
`120 t/GWth` for its open-cycle propulsion contribution during this temporary
calibration. Scenario-aware JSON values still override class defaults, and all
shipped non-fuel-cell/general overrides are also set to `0.5` so the temporary
policy is visible in data.

### Deferred post-rebalance technology targets

The differentiated engineering targets are retained for restoration after the
reactor rebalance, but are inactive for now:

| Power-plant path | Multiplier | Rationale |
|---|---:|---|
| Solid-core fission | `0.025` | Direct hydrogen heating omits the electrical plant while preserving the generous NERVA target requested for later `t/GWe` growth |
| Molten-salt/liquid-core fission | `0.05` | Direct exhaust remains favorable but requires more fluid containment and fuel management |
| Gas-core fission | `0.10` | Direct use avoids conversion hardware, but fuel containment and recovery remain substantial |
| Mirror fusion | `0.15` | Open magnetic geometry is unusually compatible with direct plasma exhaust |
| General magnetic/hybrid fusion | `0.20` | Direct exhaust saves conversion hardware but retains most confinement machinery |
| Z-pinch fusion | `0.25` | Pulsed/direct operation retains compression, switching, and chamber hardware |
| Electrostatic/toroidal/inertial fusion | `0.30` | Turning the reactor into a practical direct drive removes less of the core plant |
| Antimatter beam/plasma/gas/solid | `0.20/0.25/0.30/0.35` | Increasing interception and thermalization hardware reduces the direct-exhaust advantage |
| Fuel cell/general | `1.0` | No reactor-specific direct-thermal discount |

This multiplier solves the electrical-versus-direct-thermal distinction. It
does not solve the small-reactor fixed-mass floor identified by the NERVA
analysis. A later `openCycleFixedMass_tons` extension may be added if basic
Nerva installations become implausibly light, but it should not be silently
folded into the first implementation.

Add a feature switch:

`openCycleThermalMassScalingEnabled`

Disabling it uses `s = 1` for both mass and cap weighting while retaining the
corrected power and heat breakdown. This permits isolated regression testing
and a safe fallback.

## Shared implementation types

Extend `PowerPlantThermalMath.cs` with a pure, finite-safe calculation that
returns a `ShipPowerDemandSnapshot` containing at least:

| Field | Meaning |
|---|---|
| `DriveDemand_GW` | Hull-scaled demand shown on the drive |
| `DriveDemandIsThermal` | Open-cycle unit/label selector |
| `OpenCycleReactorOutput_GWth` | Actual direct-thermal reactor output |
| `UsefulElectricalDemand_GWe` | Closed drive plus systems and weapons |
| `ElectricalReactorInput_GWth` | Gross thermal input to conversion |
| `TotalReactorOutput_GWth` | Actual thermal output reported by the plant |
| `MassRatedOutput_GW` | Scaled value used for mass and resources |
| `CapRatedDriveDemand_GW` | Drive contribution compared with rated output |
| `OpenCycleWasteHeat_GW` | Retained drive heat |
| `ElectricalWasteHeat_GW` | Conversion loss |
| `PlantWasteHeat_GW` | Sum of the two plant heat components |

Add a context helper that builds the snapshot from raw template fields,
hull-art drive scaling, the selected power plant, settings, and the registry.
It must not call patched power getters internally; otherwise the new getters
will recurse.

Preserve these invariants in the math layer:

- `Qtotal = Qoc + Qe`;
- open cycle: `Qoc = D + Hoc`;
- electrical side: `Qe = useful electrical demand + He`;
- `Pmass <= Qtotal` whenever `0 < s <= 1`;
- open cycle: `Cdrive = s * Qoc`; closed cycle: `Cdrive = D`;
- disabling the scaler gives `Pmass = Qtotal`;
- no input produces NaN or infinity.

## Runtime and gameplay patch surface

### 1. Restore a single meaning to drive demand

Change the current `drivePowerRequirement_GW` postfix so it applies hull-art
scaling only. It must return `D`, not the open-cycle reactor output.

That value now means:

- thermal power accepted by an open-cycle drive; or
- electrical power accepted by a closed-cycle drive.

Any consumer needing reactor input must use the shared snapshot explicitly.

### 2. Reactor output and electrical generation

Patch `shipPowerProductionRequirement_GW` to return
`TotalReactorOutput_GWth`. Label the unchanged `maxOutput_GW` value as maximum
rated output in UI, distinct from actual reactor thermal output.

Audit `TISpaceShipState.CacheInternalPowerStats` separately. Preserve the
electrical-output semantics of `drive.powerGen`; the existing live heat patches
continue to use installed, hull-scaled drive demand and the common thermal math
without rewriting those cache fields.

### 3. Plant mass, resources, and cached ship mass

Patch these consumers to use `MassRatedOutput_GW`:

- `TISpaceShipTemplate.powerPlantMass_tons`;
- `TISpaceShipTemplate.powerPlantBuildCost`;
- all construction-cost paths that bypass the property;
- refit and repair costs that rebuild a plant from a power argument;
- dry-mass cache refresh and live-ship mass restoration after load.

The one-ton vanilla minimum remains unless the later fixed-mass feature is
approved.

### 4. Reactor bay geometry

Update `ReactorBayCapacitySnapshot` to expose both actual thermal output and
mass-rated output. `BayVolumeUsed_m3` is derived from `Pmass`, so electrical
loads remain fully represented while open-cycle propulsion receives its
technology multiplier.

The existing `EffectiveOutput_GW` and bay-output limit values remain the rated
capacity boundary. Compatibility compares the new `Cdrive` value with that
boundary; the authored and geometry-derived cap values themselves do not
change.

### 5. Compatibility and AI design

The same `s` value used for mass sizing also weights open-cycle drive demand in
every drive-versus-plant rated-output comparison. Closed-cycle electrical drive
demand remains one-for-one. Coverage includes:

- `TIDriveTemplate.IsCompatible(TIPowerPlantTemplate)`;
- `TISpaceShipTemplate.ValidDrivesForPowerPlants`;
- `validDriveForShipsPowerPlant`;
- `ValidPowerPlantForShipsDrive`;
- appearance-driven drive-cluster reconciliation;
- reactor-bay used-volume and effective-capacity checks;
- AI candidate filtering and any capacity-boundary helper.

Broader reinterpretation of ship-context system and weapon loads remains
deferred. The temporary weighting can make an open-cycle pairing pass with the
same authored cap, but never changes that cap number.

### 6. Heat and live combat

Refactor `TIPowerPlantTemplate.WasteHeat_GW` through the shared thermal math.
Use the same breakdown for:

- design radiator sizing and cost;
- `TISpaceShipState.DriveHeat_GJ` during a combat burn;
- cached live-ship waste heat;
- generated-power heat corrections; and
- the powered-weapon heat addition.

For an open-cycle burn, combat heat is `Hoc`; for a closed-cycle burn, it is
the drive's share of electrical conversion loss. Hull-art scaling must occur
before either calculation.

## UI plan

### Drive rows, descriptions, and tooltips

The current UI helper displays reactor output as the drive's required power.
Replace it with the snapshot's unscaled-by-plant-mass `DriveDemand_GW` and
cycle-specific labels:

- open cycle: `Thermal drive demand: 4.00 GWth`;
- closed cycle: `Electrical drive demand: 4.00 GWe`.

The numeric sort value remains `D`, so changing the selected power plant does
not reorder drives merely because plant efficiency or mass multiplier changed.

### Power-plant descriptions and tooltips

Use the compact vanilla-shaped power-plant table:

1. Classification.
2. Electrical Output for installed designs, meaning useful installed
   electrical demand.
3. Thermal Output only when an installed open-cycle drive is present, meaning
   useful work `D` delivered to the drive rather than gross reactor input
   `Qoc`.
4. Mass for installed designs.
5. One Waste Heat row for installed designs with a drive.
6. Crew when nonzero.
7. Efficiency.
8. Specific Power, with the active open-cycle mass/cap multiplier in
   parentheses after the value.
9. Max Output To Drive.
10. Build Cost for installed designs, or Cost per GW prospectively.

Keep reactor-bay used/available volume outside the table as a separate block.
Do not display separate rated-cap-use, multiplier, or heat-breakdown rows; the
underlying calculations and compatibility checks remain unchanged.

The compact presentation was deployed on 2026-08-24 after passing 1,172
formula assertions, all 176 Harmony patches, the 100-row implementation
matrix, release verification, and the 46-file deployment. Source and deployed
DLL SHA-256 is
`5A709534E45DECA9165F9361808046D0C183A2302445B0E21FF3E5B0C29F8CEB`.
Manual rendered-table confirmation remains pending.

Do not label `Pmass` as reactor output. If exposed for debugging or advanced
tooltips, call it `Mass-sizing equivalent`, never `Power produced`.

### Power-plant module table

Keep the vanilla `Max Output To Drive` wording. Preserve the current
hull/bay-limited warning, computed from the load-aware headroom calculation.

### Waste-heat UI

Retain one concise `Waste Heat` row showing the exact
`TISpaceShipTemplate.wasteHeat_GW` used to size the selected
radiator. This ensures the displayed number, radiator mass/cost, and live
cooling rate share one source.

Phase one should use existing module descriptions, rows, and tooltip text so
it does not require prefab replacement. A permanently visible designer-summary
panel would be a separate asset/UI-layout project if desired later.

Add all new localization keys to `UIGeneralControls.en`; do not depend on
English string replacement to identify fields.

## Implementation sequence

1. **Document and baseline:** retain this plan; record current Basic Nerva,
   four-gigawatt NTR, one representative closed-cycle drive, and mixed
   system/weapon designs before changing code.
2. **Pure math:** add `ShipPowerDemandSnapshot` and tests for open, closed,
   mixed, disabled, and malformed cases.
3. **Template data:** add `PowerPlantScalingRegistry`, the scenario-aware JSON
   extension, refresh hook, setting, and validation.
4. **Core getters:** restore drive-demand semantics; patch total thermal
   output, plant mass, cost, waste heat, and cache refresh.
5. **Capacity:** convert bay used volume to mass-rated output and route every
   player and AI compatibility path through cap-rated drive demand without
   changing authored cap values.
6. **Runtime heat:** reconcile design heat, combat drive heat, live cached
   waste heat, and electrical-generation heat.
7. **UI:** add cycle-specific drive labels and the separate reactor,
   electrical, and radiator-heat lines, with numeric sort values.
8. **Audit:** search the target assembly and mod for all raw consumers of
   `powerRequirement_GW`, `drivePowerRequirement_GW`,
   `shipPowerProductionRequirement_GW`, `specificPower_tGW`,
   `maxOutput_GW`, `WasteHeat_GW`, `powerPlantMass_tons`, and plant
   `buildCost`/`buildMass` calls.
9. **Build, deploy, and automated validation:** run the normal
   `tools\deploy.ps1` flow without `-SkipVerification`. The script must block
   safely if Terra Invicta is open.
10. **Manual test:** test the matrix below immediately after deployment, then
    document measured UI and mass results in the changelog and implementation
    matrix.

## Automated verification

### Formula tests

Cover:

- open-cycle conservation at `r = 0`, `0.01`, and `1`;
- closed-cycle gross-up at several efficiencies;
- mixed drive, systems, and powered-weapon loads;
- multiplier `1`, representative small values, missing values, and malformed
  values;
- `Pmass` changing without `D` or `Qtotal` changing;
- mass, cost, and bay volume using `Pmass`;
- open-cycle cap weighting at `s`, closed-cycle one-for-one cap use, and
  unchanged authored cap values;
- radiator sizing using the same heat displayed in UI;
- feature-disabled parity and finite fallbacks.

The four-gigawatt Solid Core I fixture should explicitly prove that changing
`s` can move installed mass from the current electrical-like result toward the
NTR range while the drive still displays `4 GWth` and the reactor still
reports approximately `4.017 GWth` before auxiliary electrical load.

### Structural and IL validation

Update target-assembly guards for every vanilla consumer whose semantics are
being replaced. Update the existing ship-power transpiler validator to assert:

- all module-table drive/output replacements occur exactly once;
- every drive compatibility path uses cap-rated drive demand;
- design mass and cost use mass-rated output;
- waste heat and live combat heat use the common model; and
- template initialization refreshes the new registry.

Update formula assertion counts, patch-application verification, release-file
parity, and the implementation matrix in the same change.

## Manual test matrix

| Case | Required observations |
|---|---|
| Basic Nerva + Solid Core I | Drive shows `GWth`; plant mass falls with `s`; actual thermal output does not |
| Four-GW open-cycle fixture | Thermal output, mass-sized output, bay use, and retained radiator heat match the snapshot |
| Closed-cycle electric drive | Drive shows `GWe`; reactor `GWth` includes efficiency gross-up; scaler has no effect |
| Mixed systems and powered weapons | Electrical demand and conversion heat appear separately and total radiator heat matches radiator sizing |
| Plant cap boundary | `4 GWe` and `8 GWth` consume the same rated capacity at `s = 0.5`; authored `maxOutput_GW` is unchanged |
| Reactor-bay boundary | Mass scaling changes bay occupancy; physical and geometry limits reject independently |
| Hull appearance/drive cluster changes | Demand, output, mass, bay volume, UI, and candidate filtering refresh together |
| Live combat burn | Open-cycle bleed and closed-cycle conversion heat accumulate correctly |
| Save/load | Live mass and cached thermal/electrical/heat values match the design |
| Feature disabled | Multiplier returns to one without corrupting UI or saved designs |

## Acceptance criteria

The feature is complete only when all of these statements are simultaneously
true:

1. An open-cycle drive always displays its full installed thermal demand.
2. A closed-cycle drive always displays its useful electrical demand.
3. Plant UI reports actual reactor `GWth` separately from useful electrical
   `GWe`.
4. Plant mass, resource cost, and bay occupancy use the scaled open-cycle
   contribution plus the ordinary gross electrical contribution.
5. Maximum-output compatibility charges open-cycle drive output at `s` and
   closed-cycle electrical drive output one-for-one, without changing any
   authored `maxOutput_GW` value.
6. The displayed waste heat to radiators exactly equals the value used for
   radiator mass, cost, and live cooling.
7. Design, AI, refit, save/load, and combat paths agree with the same snapshot.

## Deployment result

The normal `tools\deploy.ps1` path completed against the installed TI 1.0.51
assemblies on 2026-08-24. It passed 1,172 formula assertions, all 176 Harmony
patch registrations, the 100-row implementation matrix, the guarded
ship-power and target-IL validators, release packaging, and deployment of 46
files. The packaged and deployed DLLs match at SHA-256
`8F7F33E3953244C095C0E69E763FECF3469EF0F162922A8AC15F9C4BD76DDB4A`.

Manual verification should now exercise the matrix above with particular
attention to prospective-versus-installed tooltip parity, the new rated-cap
boundary, mixed open-cycle and auxiliary electrical loads, refit resource cost,
save/load mass restoration, and combat radiator heat.

The first 2026-08-24 UI follow-up was rejected in manual testing. Vanilla's
mass and crew strings were already labelled with `<rcol>…</rcol>`; the actual
fault was that the injected rows did not use this native column markup. The
corrective follow-up preserves every vanilla row and requires each injected
field to contain one balanced `<rcol>` pair, including the rated-cap and
reactor-bay rows, so the complete description remains one table. The verified
46-file deployment passed 1,172 assertions and all 176 Harmony patches; source
and deployed DLL SHA-256 is
`9CBEC6289FF3588ECD0DC149927ED9E1726D58480AD45A6B8AE608C9F5AF6D11`.
Manual rendered-table confirmation remains pending.
