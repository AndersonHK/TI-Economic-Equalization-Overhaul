# Open-cycle reactor demand and heat accounting

Status: implemented and deployed 2026-08-20; manual in-game testing pending

## Problem

The existing open-cycle correction retains a configurable fraction of the
power plant's drive-associated conversion loss as ship heat. The default is
`1%`. Reactor output demand, plant mass, compatibility, and reactor-bay volume
still use the unadjusted drive requirement, however. That violates the energy
balance implied by the correction: if the other `99%` of the would-be
conversion loss enters the propellant as useful drive energy, the reactor can
be throttled to the useful drive demand plus the small residual loss.

The audit also found a pre-existing hull-art scaling bug. Live ships cache
propulsion generation from the ship-level, art-scaled
`TISpaceShipTemplate.drivePowerRequirement_GW`, while
`TISpaceShipState.DriveHeat_GJ` reads the raw drive-template requirement.
Combat-burn heat therefore bypasses the installed hull-art multiplier and can
disagree with the same ship's cached propulsion generation.

`TISpaceShipTemplate.ValidDrivesForPowerPlants` separately duplicates the raw
drive-versus-plant-cap comparison instead of calling the ordinary candidate
compatibility method. That is not classified here as a previously missed bug;
it is a required consumer to patch so the new open-cycle demand rule reaches
AI candidate filtering.

## Settled model

Let:

- `D0` be the drive template's power requirement;
- `S` be the installed hull-art drive multiplier;
- `D = D0 * S` be useful drive input after cluster and art scaling;
- `eta` be power-plant efficiency;
- `f` be the retained open-cycle fraction, normally `0.01`.

The effective open-cycle coupling is:

`etaOpen = eta + (1 - f) * (1 - eta)`

or equivalently:

`etaOpen = 1 - f * (1 - eta)`

Required reactor output and residual heat are:

`Q = D / etaOpen`

`H = Q - D = Q * f * (1 - eta)`

This conserves energy exactly: reactor output equals useful drive input plus
retained heat. Closed-cycle drive accounting and electrical system/weapon
accounting remain unchanged.

With Solid Core Fission Reactor I at `57.5%` efficiency, `etaOpen` is
`99.575%`. A basic unscaled Nerva requirement of `0.283 GW` therefore becomes
about `0.284208 GW`, leaving about `1.208 MW` as ship heat. A hypothetical
`4 GW` requirement becomes about `4.017073 GW`, leaving about `17.073 MW`.

## Required patch surface

The implementation must use one shared formula in all of these paths:

1. Installed `TISpaceShipTemplate.drivePowerRequirement_GW`, after hull-art
   scaling, so plant mass, plant cost, total generation demand, and live-ship
   cached propulsion generation inherit the corrected value.
2. `TIPowerPlantTemplate.WasteHeat_GW`, using the corrected open-cycle reactor
   demand to calculate `Q - D` rather than a disconnected heat-only estimate.
3. `TIDriveTemplate.IsCompatible(TIPowerPlantTemplate)` for candidate-specific
   checks without a ship context.
4. `TISpaceShipTemplate.ValidDrivesForPowerPlants`, whose duplicated vanilla
   comparison bypasses the ordinary compatibility method.
5. Both ship-context drive/power-plant compatibility directions, applying the
   order `template -> hull-art scale -> open-cycle coupling -> bay limit`.
6. Reactor-bay used volume and appearance-driven drive-cluster reconciliation.
7. Designer required-power text and numeric sort values.
8. `TISpaceShipState.DriveHeat_GJ`, which must use the installed, art-scaled
   drive requirement rather than raw template power.

The raw `TIDriveTemplate.powerRequirement_GW` remains unchanged because a
drive template has no selected power plant. The `100 GW` fusion appearance
threshold also remains based on intrinsic drive scale rather than reactor
conversion loss.

## Feature controls and fallbacks

The demand correction is active only when ship balance, corrected plant heat,
and open-cycle residual heat are all enabled. Disabling residual heat restores
the existing one-for-one open-cycle demand. Efficiency and residual fractions
are bounded to safe ranges, and invalid data must not produce NaN or infinity
in mass, compatibility, or radiator calculations.

## Verification requirements

- Formula tests must prove `Q = D + H`, identity behavior at zero retained
  heat or perfect plant efficiency, full closed-conversion behavior at a
  retained fraction of one, and safe malformed-input behavior.
- Regression tests must cover hull-art scaling before open-cycle coupling,
  plant-cap boundaries, reactor-bay used volume, and disabled feature states.
- Target-assembly validation must guard the vanilla methods whose duplicated
  or bypassing behavior motivated the patches.
- Patch-application validation must apply every new Harmony class against the
  installed Terra Invicta assembly.
- Manual testing must cover Nerva reactor changes, drive clusters, hull
  appearance cycling, bay clamping, a closed-cycle control, and a live combat
  burn.

## Implemented result

The shared thermal helper now computes effective open-cycle coupling, required
reactor output, and retained heat with finite, bounded inputs. The installed
ship getter applies hull-art scaling before open-cycle coupling, which makes
plant mass, plant construction cost, design and live generation demand, and
localized plant output inherit the same value.

The exceptional consumers are patched explicitly:

- drive-to-plant and plant-to-drive ship compatibility;
- shipless `TIDriveTemplate.IsCompatible` calls and the duplicated static AI
  drive filter;
- reactor-bay used volume and appearance-driven cluster reconciliation;
- designer power text and numeric sort values; and
- `TISpaceShipState.DriveHeat_GJ`, which now uses the installed ship-level
  requirement instead of raw drive-template power.

The final raw-consumer audit found only the expected template localization,
two compatibility comparisons, the ship-level getter, combat heat, designer
table values, and the `100 GW` graphical fusion selector. All demand-bearing
and displayed-power consumers are covered. The fusion selector deliberately
remains raw because it chooses intrinsic drive art rather than reactor size.
The installed assembly contains only two calls to
`TIPowerPlantTemplate.WasteHeat_GW`: ship-template radiator sizing already uses
the patched ship-level demand, and combat drive heat is covered by the new
installed-drive prefix.

## Automated verification and deployment

The normal TI 1.0.51 deployment passed:

- **1,110 formula assertions**, including energy conservation, feature
  fallbacks, hull-art ordering, output-boundary rejection, static AI filtering,
  and reactor-bay used volume;
- target-assembly IL guards for all four raw or duplicated consumers;
- patch application for the three new Harmony classes;
- **157 Harmony patches** covered by the **96-row** implementation matrix;
- release packaging and the complete repository verification suite; and
- deployment of **45 files** to the enabled mod directory.

Deployed DLL SHA-256:
`9C184342CD6842F6B3E1B543D5E77F54A44EDF8D3160EC1361D32BA90F8DA8F7`.

Manual testing should compare an open-cycle Nerva design before and after
plant selection, cycle through hull appearances and drive clusters, confirm a
closed-cycle control remains unchanged, and execute a combat burn while
watching heat accumulation.
