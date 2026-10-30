# Battle geometry and minimum camera speed implementation

**Manual follow-up, 2026-10-29:** the user observed invisible stations and ships,
oversized paths and insufficient close zoom. These failures are addressed by
the [0.10.10 follow-up](battle-scale-rendering-camera-followup-2026-10-29.md).
The original deployment evidence and testing status below remain historical.

2026-10-23. **EEO 0.10.9 is built and deployed for manual testing.** Automatic
validation passed. In-game appearance, live collider readbacks and gameplay
acceptance remain pending.

## Approved behavior

The user approved physically consistent combat metres with cinematic scaling
off, and the former non-cinematic enlargement with it on: ships/stations 20x,
projectiles 50x. Damage colliders must follow visible geometry closely using
simple shapes. Minimum camera movement and scrolling speed are reduced 20x;
the broader camera redesign remains deferred.

The prior research is committed and pushed as `90493da` on origin/master.
The [scale report](rendered-battle-scale-and-collision-report-2026-10-23.md)
and [camera report](combat-camera-fine-control-report-2026-10-23.md) retain the
pre-change baseline.

## Implemented behavior

- Retained 0.05 scene units/km for battle positions and velocities. Changed model
  and projectile scale getters to 0.00005 scene units/metre with cinematic off,
  and 0.001 / 0.0025 respectively with it on. Leave the shared formation/camera
  adjustment factor alone.
- Fitted damage geometry to visible solid model parts, excluding markers, VFX and
  invisible native selection/proximity volumes. Preserve functional selection
  and navigation separately and restore native model state on combat exit.
- Covered ships, station modules and all projectile families, including pooled
  reuse; keep physical damage areas and weapon statistics independent of art
  enlargement. Known-caliber rounds need matching visible bodies rather than
  native oversized particle sprites.
- Removed missile ray lookahead inflation that would otherwise produce impacts
  before physically reaching the much smaller targets.
- Reduced minimum camera movement from 5 to 0.25 and minimum scroll coefficient
  from -200 to -10 in the partial global template. Preserve maximum values and
  the native camera behavior otherwise.
- Added automatic numeric, native-binding and lifecycle-target checks; ran
  `tools/deploy.ps1` without skipping verification, and retain deployment
  evidence here. No live game process may be present during deployment.

| Combat mode | Ships and stations | Projectiles | Metre in scene units |
|---|---:|---:|---|
| Cinematic off | 1x | 1x | 0.00005 for both |
| Cinematic on | 20x | 50x | 0.001 models; 0.0025 projectiles |

These conversions use authored art-space metres. Statistical hull length and
width remain separate balance envelopes; meshes are not stretched to force a
particular template length. The tactical input grid retains its former extent
so shrinking physical models does not shrink the command plane.

### Matching solid visuals and damage geometry

[CombatMeshGeometry](../../TIEconomyMod/Core/CombatMeshGeometry.cs) places one
simple BoxCollider at each solid MeshRenderer/SkinnedMeshRenderer's local
bounding box. Each box inherits the same transform as its visual part, so
turret rotation, drive scale and radiator motion also move its collider.
Disabled/hidden parts have disabled boxes. Local axes are bounded below by
0.1 mm only to keep zero-thickness graphical surfaces valid in physics.

This is a component-wise bounds approximation, not a triangle-perfect
silhouette: a box includes some empty corners around a curved or concave mesh.
It avoids a single large hull box, cooked high-detail collision meshes, and the
known displaced native collider coordinates. Runtime probes report visual
versus collider AABB errors for manual verification.

Native damage boxes, capsules and spheres are disabled while replacements are
active. Ship physics/hit lists and station/projectile hit lists refer to the
new geometry. Selection/proximity triggers retain their independent,
non-damaging function and are excluded from the damage lists. Ship navigation
size and ship/station predicted threat spheres use the actual visible bounds.
The prediction sphere is a conservative enclosing proxy, not an extra hitbox.

The station formation-exit path re-enables native `CollisionObject` boxes;
`BattleHabFormationGeometryPatch` suppresses those immediately after native
configuration. Native geometry is also kept disabled by the part updater.
Combat exit restores the ship's original physics array and collider-enabled
states, removes generated boxes, and restores the station geometry before its
strategic reparenting.

### Projectile bodies

[BattleProjectileGeometry](../../TIEconomyMod/Core/BattleProjectileGeometry.cs)
replaces oversized ballistic flight sprites with simple visible bodies:

- Naval gun rounds use the configured caliber and a sphere collider.
- Magnetic rounds use the existing complete-round-mass diameter convention
  (19,300 kg/m³, length/diameter 10), with an elongated visible body and matching
  bounding box. The native logarithmic visibility multiplier is removed.
- Plasma uses the authored flight particle's maximum start diameter and child
  scale as its nominal metre diameter, with a matching visible sphere/collider.
  Plasma templates do not provide an independent physical diameter, so this
  preserves the art convention rather than inventing a new mass/density model.
- Missiles retain their authored meshes, with per-part boxes fitted after
  native `Fire`. They share the same metre conversion/projectile enlargement.

The [static projectile prefab snapshot](battle-scale/projectile-asset-baseline.json)
retains the source bundle hash, mesh bounds, primitive dimensions and authored
particle sizes used to identify the native mismatches.

Generated bodies/colliders are reused on pooled shots and hidden immediately
on destruction. Original ballistic flight renderers are disabled; exhaust,
tracking UI, impacts and destruction effects are separate non-damaging
visualizations. A named tracking marker is not the physical missile body.
Physical cross-sectional damage calculations remain in square metres.

The missile ray formerly added `0.023 u` (460 m) plus a 3.5x travel sweep.
The guarded patch uses no fixed padding and one frame of travel, matching the
existing EEO ballistic sweep. This avoids scoring a distant early hit against
a much smaller target. [Patch targets](../../TIEconomyMod/Patches/BattleGeometryPatches.cs).

### Camera limits

Only the two minima change in
[TIGlobalConfig.json](../../TIEconomyMod/ModFiles/TIGlobalConfig.json): movement
`5 / 20 = 0.25`, scroll coefficient `-200 / 20 = -10`. The far-end speeds are
unchanged. The native curve interpolates between the new minima and old maxima,
so the twentyfold reduction applies exactly at the near endpoint, not at every
zoom level. FPS-dependent movement and the full distance-based camera proposal
remain deferred as requested.

## Automatic validation and deployment

Final command: `tools/deploy.ps1`, with normal verification and both game-closed
assertions. Result: **41 validators, 1,209 formula assertions, 48 deployed files**.
The implementation matrix validates 109 feature rows and 226 Harmony patches.
The new validator checks both unit policies, exact camera values, native method
targets, missile IL replacement and a rejected unsupported IL layout.

Fourteen affected Harmony targets resolve; twelve cannot be fully emitted in a
standalone .NET process because the methods require Unity ECalls. Their live
binding, scene lifecycle and bounds remain in-game validation, not a claimed
automated Unity test. Existing gunpowder geometry/material and native JSON
discovery guards still pass.

Final deployed/package DLL SHA-256:
`76BBF60B6363B3C9A0E74B386D64F11132E4C20A806ECB0C81CC4531B69044D8`.
The packaged DLL, release archive and all deployed files are independently
compared in the [deployment manifest](battle-scale/deployment-manifest-0.10.9.json).
The [final deployment log](battle-scale/deployment-0.10.9.txt) preserves the
normal build/validation/copy result.

The first deployment attempt compiled successfully but stopped before copying
because two new bookkeeping checks mishandled the config's root array and the
matrix's template-key category. Both were corrected. A successful intermediate
deployment was followed by the station formation lifecycle guard; the final
build above passed the complete deployment flow again. Only documentation and
readback operations followed the final deployment.

## Required manual checks

Test campaign and skirmish combat with both cinematic flags. Inspect human and
alien ships, station modules, gunpowder/magnetic/plasma shots and missiles at
close range. Compare hits and near misses with the visible bodies; check
turret/drive/radiator animation, projectile reuse, destruction, selection,
navigation and return to the strategic view. Confirm finer scrolling and
movement near minimum zoom. Runtime readback will distinguish mesh bounds,
damage colliders and non-damaging markers.

Runtime log prefixes are `Combat geometry readback:` and
`Combat projectile readback:`. The former reports fitted-box count, replaced
primitive count and maximum relative visual/collider AABB error once per model
name per session. The latter reports projectile dimensions and scale per
weapon/mode. These logs are generated when the user runs a battle; they were
not captured as live evidence during this implementation turn.

The new gameplay implementation is left uncommitted for testing. The user
requested committing/pushing the previous turn's reports, which is complete.
