# Combat grid, formation spacing and explosion sizing

**Startup regression follow-up:** [0.10.12 replaces the early station
destruction patch with a runtime observer](skirmish-asset-cache-startup-crash-2026-10-29.md).
The original 0.10.11 deployment and visual-test record below are historical.

2026-10-29. **0.10.11 is built and deployed for manual testing.** Automated
validation passed; in-game grid, formation and effect acceptance remains pending.

The user reports oversized blue grid lines, excessive spacing within fleets,
and explosions that do not match the reduced hull size. The supplied screenshot
shows the fleet-formation screen. These are separate paths from the trajectory
and waypoint fixes in the previous build.
[Retained screenshot](battle-scale/manual-grid-spacing-2026-10-29.png).
The [0.10.10 log snapshot](battle-scale/manual-grid-spacing-player-2026-10-29.txt)
preserves the pre-fix runtime baseline.

## Plan and measured code paths

- `CombatGrid.Update` independently assigns Shapes line thickness each frame;
  apply the same model-relative factor as other world-space UI. Preserve the
  tactical input plane's reach and the line endpoints.
- `TIFormationTemplate.GetSpacingOffset_km` calls
  `SpaceCombatManager.GetFormationScalingFactor` for combat setup/live combat,
  but uses 1 for strategic-layer placement. Reduce combat spacing with the hull
  scale: 1/20 in physical mode and 1/10 relative to former cinematic sizing.
  Preserve strategic positions and the opposing fleets' initial separation.
- Trace ship/station destruction and spawned particle effects, including
  world-scale cancellation and effects created below compensated renderer
  transforms. Apply absolute sizing with reuse/exit restoration; avoid double
  scaling already-correct effects or changing damage/blast ranges.
- Extend normal automated checks and deploy through `tools/deploy.ps1`.
  The game-closed guard remains required. No commit or push is requested.

## Implemented behavior

The [grid/spacing/effect patches](../../TIEconomyMod/Patches/CombatSpacingEffectsPatches.cs)
change blue grid thickness by 1/20 in physical mode and 1/10 relative to the
original cinematic mode. The coordinate grid retains its extent for picking;
only line thickness changes. Native per-frame thickness assignment cannot undo
the change, because the final setter receives the scale factor.

Combat formation factors become 0.005 physical and 0.1 cinematic. For example,
the tight pattern's base spacing vector changes from 6/6/10 km to
0.3/0.3/0.5 km in physical mode. Formation selection, rearrangement and
reinforcements use this common factor. Strategic placement explicitly bypasses
the combat getter and is unchanged. Initial separation between opposing fleets
is separate from spacing within a fleet and is retained.

### Explosions

Native `ShipModelController.AddExplosions` cancels parent world scale when
creating its fallback explosion objects; radiator code separately cancels a
parent's local scale. Dynamically spawned weapon and station effects also
inherit renderer transforms whose magnitude was deliberately increased by the
0.10.10 baking fix. A blanket extra 1/20 multiplier would therefore double-scale
some paths and leave others inconsistent.

[CombatExplosionGeometry](../../TIEconomyMod/Core/CombatExplosionGeometry.cs)
instead derives an absolute target from the visible geometry:

```text
effect world scale = visible target diameter / largest authored particle start diameter
effect local scale = effect world scale / absolute parent world scale, per axis
```

The visual diameter is the largest dimension of the active solid-mesh envelope.
The fallback per-part ship bursts fit that mesh part; the final ship explosion
fits the whole hull. Weapon/radiator effects fit their component, and station
destruction fits the affected module. Native later 0.5–1.5 small-burst and 2–3
final-burst variation remains, so a final shockwave can expand beyond the hull.
Initialization no longer relies on arbitrary inherited/inverse transform scales.

The [inspected explosion prefabs](battle-scale/explosion-prefab-baseline-2026-10-29.json)
contain hierarchy-scaled particles, including a 15-unit initial shockwave.
The new binder explicitly uses hierarchy particle scaling so child effects,
emission and sizes follow the normalized root. Newly born particles at an old
scale are cleared without changing effect timing. Absolute assignment avoids
compounding on repeated normalization, including particle-group effects spawned
after the renderer bake.

Ship effect transforms and particle scaling modes are restored before the
mesh-baking transforms on exit. New destruction instances own their temporary
state until destruction. The fixed local position jitter in ship bursts also
receives the model-relative factor; delay and size-randomization constants do not.
Weapon statistics, damage, missile blast radii and impact calculations are not
changed by these visual destruction-effect hooks.

## Validation and deployment

The normal `tools/deploy.ps1` flow passed **41 validators and 1,209 formula
assertions**, packaged the build and verified **51 deployed files**. The matrix
contains 116 feature rows and 242 Harmony patches. Checks include the guarded
grid setter and six spatial jitter constants, both formation modes across
setup/live/strategic/disabled cases, and explosion world-diameter invariance
under nonuniform and mirrored parent scales.

Thirty affected Harmony targets resolve; 26 require Unity-host emission
confirmation. Standalone checks do not prove particle appearance or live
formation behavior. The [deployment log](battle-scale/deployment-0.10.11.txt)
and [readback manifest](battle-scale/deployment-manifest-0.10.11.json) identify
the testing build. DLL SHA-256:
`62B68B7D41C2B734EAC6D7C4F95899A0BA944DC4841F8365FFB5EF6A0F8786C6`.

Only documentation and readback followed deployment. No commit or push was
requested; the current work remains uncommitted. The previously documented
release-ZIP file-list omissions remain separate from the complete local install.

## Manual checks

- Open a fresh formation screen in both scale modes. Compare blue-line width
  with hull size and verify grid toggling/picking still works.
- Check tight and dispersed patterns, formation swaps and incoming
  reinforcements. Verify large hulls have practical clearance and opposing
  fleets retain their normal initial separation.
- Destroy small and large ships, weapons and radiators; include station-module
  destruction and DLC particle-group sequences. Check initial blasts, delayed
  final shockwaves, positions, fade timing and no lingering effects.
- Return to the strategic view and enter another battle to check restoration
  and reuse. These visual checks are necessary before acceptance.
