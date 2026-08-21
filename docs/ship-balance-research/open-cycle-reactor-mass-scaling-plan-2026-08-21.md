# Open-cycle reactor mass scaling and power UI plan

Status: implementation plan only. No gameplay values are changed by this
document.

Last reviewed: 2026-08-21

## Objective

Separate a ship power plant's physical reactor output from the mass-sizing
burden associated with two different uses of that output:

1. direct thermal power sent through an open-cycle drive; and
2. thermal input converted into electricity for closed-cycle drives, ship
   systems, and weapons.

The open-cycle drive must continue to advertise its full installed thermal
demand. The new scaler reduces only how strongly that thermal contribution
sizes plant mass, construction resources, and reactor-bay occupancy. It must
not reduce thrust, falsify reactor thermal output, or make an under-capacity
reactor compatible with a drive.

The same demand breakdown will correct four UI concepts:

- actual reactor thermal output and net electrical generation;
- drive demand and non-drive electrical demand;
- waste heat actually assigned to the radiators; and
- drive demand units: `GWth` for open-cycle drives and `GWe` for closed-cycle
  drives.

## Settled accounting model

Use one shared snapshot for every design, compatibility, mass, cost, volume,
heat, and UI consumer. Do not continue encoding several meanings into
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

`Hrad = Hoc + He`

The reactor's `maxOutput_GW` comparison uses `Qtotal`, not `Pmass`.

### Closed-cycle drive

The drive's displayed demand is electrical:

`drive display = D GWe`

Combine it with other useful electrical loads:

`electrical display = D + A GWe`

`Qe = (D + A) / eta`

`Qoc = 0`

`Qtotal = Pmass = Qe`

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

For example, Solid Core I currently uses `240 t/GW`. A target open-cycle
coefficient of `6-10 t/GWth` implies a multiplier of approximately
`0.025-0.0417`. Exact shipped values require a separate reactor-data
calibration pass after the mechanic is verified.

This multiplier solves the electrical-versus-direct-thermal distinction. It
does not solve the small-reactor fixed-mass floor identified by the NERVA
analysis. A later `openCycleFixedMass_tons` extension may be added if basic
Nerva installations become implausibly light, but it should not be silently
folded into the first implementation.

Add a feature switch:

`openCycleThermalMassScalingEnabled`

Disabling it uses `s = 1` while retaining the corrected power and heat
breakdown. This permits isolated regression testing and a safe fallback.

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
| `TotalReactorOutput_GWth` | Physical output used for plant caps |
| `MassRatedOutput_GW` | Scaled value used for mass and resources |
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
`TotalReactorOutput_GWth`. Treat `maxOutput_GW` consistently as maximum thermal
reactor output and update UI labels to `GWth`.

Audit `TISpaceShipState.CacheInternalPowerStats` separately. Its auxiliary
fields currently store gross thermal requirements, while its propulsion field
also participates in `drive.powerGen` electrical-gain behavior. Do not blindly
replace that field with `Qoc`: preserve the electrical-output semantics of
`powerGen`, and use the snapshot only where the field represents reactor
capacity. Add an IL guard around the audited assignments.

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

Reactor-bay fit becomes load-composition-dependent. Keep two limits:

- physical thermal capacity: `Qtotal <= maxOutput_GW`;
- geometric/mass capacity: plant mass or its derived volume from `Pmass` must
  fit the measured bay.

Update `ReactorBayCapacitySnapshot` to expose both actual thermal output and
mass-rated output. `BayVolumeUsed_m3` must be derived from `Pmass`, while
compatibility with the plant rating must use `Qtotal`.

The existing single `EffectiveOutput_GW` is no longer sufficient as a
context-free plant characteristic. For a selected ship, calculate remaining
open-cycle drive capacity after its electrical load:

`thermal headroom = maxOutput_GW - Qe`

`mass headroom = (bay-equivalent capacity - Qe) / s`

The permitted open-cycle contribution is the smaller headroom. Closed-cycle
headroom uses the ordinary unscaled path.

### 5. Compatibility and AI design

Replace every raw drive-versus-plant comparison with the snapshot result:

- `TIDriveTemplate.IsCompatible(TIPowerPlantTemplate)`;
- `TISpaceShipTemplate.ValidDrivesForPowerPlants`;
- `validDriveForShipsPowerPlant`;
- `ValidPowerPlantForShipsDrive`;
- appearance-driven drive-cluster reconciliation;
- reactor-bay used-volume and effective-capacity checks;
- AI candidate filtering and any capacity-boundary helper.

Shipless compatibility lacks systems and weapons, so it uses the candidate
drive alone. Ship-context compatibility includes every installed electrical
load. A factor must never make `Qtotal > maxOutput_GW` pass.

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

For an installed or prospective ship context, display these as separate lines:

- `Reactor thermal output: Qtotal GWth`;
- `Electrical generation: useful electrical demand GWe`;
- `Open-cycle drive output: Qoc GWth`, when present;
- `Waste heat to radiators: total radiator load GWth`;
- optional indented heat breakdown:
  `open-cycle bleed / electrical conversion / module heat`;
- `Open-cycle mass factor: x s`, when `s != 1`;
- existing installed mass, cost, efficiency, and maximum thermal output.

Do not label `Pmass` as reactor output. If exposed for debugging or advanced
tooltips, call it `Mass-sizing equivalent`, never `Power produced`.

### Power-plant module table

Keep the maximum-output column, but localize it as maximum **thermal** output.
In the selected module tooltip, show actual `Qtotal` and useful electrical
generation separately. Preserve the current hull/bay-limited warning, now
computed from the load-aware headroom calculation.

### Waste-heat UI

Replace the generic plant waste-heat line with `Waste heat to radiators` and
the exact `TISpaceShipTemplate.wasteHeat_GW` used to size the selected
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
5. **Capacity:** convert bay volume and every compatibility/AI consumer to the
   two-limit physical-output and mass-volume model.
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
- compatibility and maximum-output checks using `Qtotal`;
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
- no raw maximum-output comparison bypasses the snapshot;
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
| Plant cap boundary | Lower mass does not permit thermal output above `maxOutput_GW` |
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
5. Maximum-output compatibility uses actual thermal output and cannot be
   bypassed by the mass scaler.
6. The displayed waste heat to radiators exactly equals the value used for
   radiator mass, cost, and live cooling.
7. Design, AI, refit, save/load, and combat paths agree with the same snapshot.

