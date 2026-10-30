# Settings-backed combat scale: initial 10x trial

2026-10-30. **0.10.14 is built and deployed for the 10x/10x trial.** Automatic
checks passed. Visual behavior and settings UI still require in-game acceptance.

The user requested separate adjustable ship/projectile scale multipliers in
mod settings, initially both 10x, shared by the earlier dimensional fixes.
The settings will be authoritative in both cinematic modes. Ship scale includes
stations, geometry, collider fit, grid/waypoint/arrow sizing, camera framing,
  formation spacing, close camera movement/scroll minima and bounds-fitted explosions. Projectile scale includes its
body/collider geometry and tactical-display distance.

## Implementation plan

- Add `shipBalance.shipScaleMultiplier` and
  `shipBalance.projectileScaleMultiplier`, defaulting to 10 in both the settings
  class and packaged Settings.xml, visible in the normal mod settings UI.
- Validate finite values in a documented useful range, repairing invalid data
  to 10. Read/capture both multipliers at native combat initialization and
  release the capture on exit. Changes during a battle apply to the next battle.
- Route every previous scale consumer through those two captured values.
  Reference native cinematic/non-cinematic factors only when compensating
  existing UI formulas, not as an extra multiplier on the selected scale.
- Keep physical units at 0.00005 scene units/metre, so 10x means 0.0005 for
  both bodies. Gameplay distances, weapon statistics and damage are unchanged.
- Extend numeric, settings serialization/default/repair, lifecycle and shared
  consumer checks, then use the normal guarded deployment flow.

## Settings and operation

In the EEO mod manager's **Ship Balance** group, edit the ship/station or
projectile combat scale field. The same values are serialized in
[Settings.xml](../../TIEconomyMod/ModFiles/Settings.xml):

```xml
<shipBalance>
  <enabled>true</enabled>
  <shipScaleMultiplier>10</shipScaleMultiplier>
  <projectileScaleMultiplier>10</projectileScaleMultiplier>
  <!-- Other ship balance settings remain as before. -->
</shipBalance>
```

The two values are independent. The trial uses 10 for each in either cinematic
mode. Supported finite values are 0.01–1000; invalid values repair to 10.
Older XML without the fields receives constructor defaults of 10. The settings
range validates numeric input, not gameplay balance at every extreme value.

Edits in the mod UI are saved normally and take effect on the **next battle**.
The current battle retains its capture, including subsequently fired shots and
rearranged formations. Disk-only XML edits are loaded when restarting the game;
they are not polled during combat. All battle distances, velocities, masses,
weapon damage, physical damage areas and optical statistics remain unchanged.

## Shared sources and effects

[BattleScaleRuntime](../../TIEconomyMod/Patches/BattleGeometryPatches.cs) captures
the two validated values before native `SpaceCombatManager.Initialize`, and
releases the capture on `OnDisable`. Repeated capture does not overwrite an
active battle. Outside battle, readers use current settings/defaults.

| Consumer | Source |
|---|---|
| Ship/station mesh scale and matching damage colliders | Captured ship multiplier |
| Private renderer mesh bake and restore | Ship scale relative to native renderer matrices |
| Laser width | Current visible lens geometry, which inherits ship scale |
| Grid-line thickness, waypoint/arrows, camera opening/near limits and fleet spacing | Shared ship-relative factor |
| Close camera movement and scrolling minima | Shared ship factor, compensating native camera mode |
| Ship/part/station destruction effects | Visible geometry bounds after ship scaling |
| Ballistic/missile body and collider size | Captured projectile multiplier |
| Ballistic/missile tactical-display envelope and marker sizing | Shared projectile factor, independent of cinematic flag |

Existing native mode factors are used only to compensate formulas that already
include them. They do not multiply the selected geometry size again. Far camera
limits/speeds and screen-space text/pixel strokes retain their existing policies.
The tactical input plane keeps its reach while its visible line thickness follows
ship sizing.

At the trial values:

| Quantity | Value |
|---|---:|
| Ship/station units per authored metre | 0.0005 |
| Projectile units per metre | 0.0005 |
| Projectile tactical-display switch | 8 km |
| Initial camera orbit baseline | 20 km |
| Closest orbit baseline, before object-bounds standoff | 1 km |
| Tight formation base X/Y/Z spacing | 3 / 3 / 5 km |

The close-control packaged coefficients 0.25 movement / -10 scrolling remain
the earlier 1x reference. Runtime getters scale them by the same ship factor,
so a 10x ship receives proportionate close movement in both native modes.
No current ship-scale setting can change the projectile tactical-display range.

## Validation and deployment

Normal `tools/deploy.ps1` completed with **41 validators, 1,219 formula assertions,
384 aperture comparisons and 52 deployed files**. The matrix has 119 feature
rows and 249 Harmony patches. Checks cover constructor and packaged defaults,
old XML migration, independent 20x/5x serialization, invalid-value repair,
immutable active captures, release/recapture, tactical range and close controls
across both cinematic flags, and formation settings at multiple scales.

The original [settings snapshot](battle-scale/settings-pre-configurable-scale-2026-10-30.xml)
still matches the historical gunpowder-art baseline hash. The validator removes
only the two new fields before comparing its XML semantics against current
defaults, preserving all unrelated settings. Native source/bundle and crash
initializer guards continue to pass. Thirty-seven battle-hook classes resolve;
31 require Unity-host emission confirmation. This is not a claimed scene test.

DLL SHA-256:
`E5378DFABDBB1D1F83B1A65C40C0BB538C350736D752803F2BDD560E27EEB76C`.
[Deployment log](battle-scale/deployment-0.10.14.txt) and
[installed-file readback](battle-scale/deployment-manifest-0.10.14.json).
Only documentation/readback followed deployment. No commit/push was requested.
The pre-existing release-archive omissions remain documented separately from
the complete local deployment.

## Manual acceptance

Start a new battle with the default 10x/10x and compare hulls, station modules,
projectile bodies/colliders, laser widths, grid/arrows, formations, opening/close
camera and destruction effects. Check projectile tactical graphics near 8 km.

Change one multiplier in the mod UI during a battle: the current battle should
retain its original scales. Save and start the next battle to observe the new
value. Use different values (for example ship 20 / projectile 5) to verify
independence: the tactical switch becomes 4 km while ship-related visuals follow
20x. Repeat with the cinematic option toggled; geometry and tactical range should
follow the mod settings. Confirm save/restart persistence and strategic return.

## Subsequent contact/performance correction

[0.10.15](physical-projectile-contact-and-performance-2026-10-30.md) supersedes
recurring collider refresh/mesh replacement polling and beam-width application.
Static scale is configured during creation or reuse. The 10x/10x settings policy
above remains; collision follows the actual completed native flight segment.
