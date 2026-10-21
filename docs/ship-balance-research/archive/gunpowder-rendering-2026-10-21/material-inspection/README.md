# 40 mm grey-material investigation

**Archived 2026-10-21.** Historical planning/investigation state; later outcomes supersede pending or provisional conclusions below. See the [current authority](../../../gunpowder-canonical-art-implementation-2026-10-21.md). Original numerical evidence is retained.

**Latest finding:** the object reveals the sky/Earth through its silhouette;
0.10.7 did not resolve it despite tracing correct material bindings. See the
[deployed 0.10.8 same-size rendering comparison](background-cutout-0.10.8.md).

2026-10-21. User reports that scales look right, but the 40 mm is a flat grey
mask from the start of combat; 10-inch cannon textures are visible. The initial
suggestion that this was merely a plainer source texture was insufficient and
has been withdrawn as an explanation for the reported mask.

## Evidence before the runtime material pass

- The 0.10.6 log confirms catalog loading and 40 mm / 10-inch profile application
  in combat, without a gunpowder source-geometry guard failure.
- Both resources have one mesh/submesh material, UVs spanning their atlas, and
  Standard materials with albedo, normal, emission and metallic/smoothness maps.
  Their textures are 512×512 with ten mip levels; streaming is disabled in the
  source assets. Scaling code changes no material, texture or UV field.
- Controlled unlit source-texture renders at matched apparent size retain the
  atlas after resizing. This is offline evidence, not a reproduction of the
  user's Unity rendering fault. The 30/40 mm receiver really has plainer source
  panels, but that does not account for loss of all material detail in-game.
- All 184 inspected base/moving renderers across six human classes and all four
  appearances have one material/submesh. None of their damage-layer renderer
  lists includes a weapon. Thus the hypothesized leftover laser-cap slot or
  hull damage overlay is not supported by these inspected prefabs. Unity does
  draw surplus material slots over the last submesh, but that general behavior
  is not established as the cause here. [Unity renderer documentation](https://docs.unity3d.com/ja/2021.2/Manual/class-MeshRenderer.html).
- Native SetWeapon calls Initialize before copying the resource materials.
  InitializeCommon caches `_originalBaseMaterial` and `_originalWeaponMaterial`
  from the previous renderer state. OnWeaponRepaired restores those caches.
  OnWeaponDestroyedExplosion also incorrectly saves the weapon material into
  the base cache. This is a verified lifecycle defect in the inspected code,
  but its role in this particular fresh-combat screenshot is not yet proven.

## Narrow corrective and diagnostic plan

For the eight profiled gunpowder templates only, copy the resource's complete
material arrays after the existing geometry guard and cache the correct first
materials for native restoration. On actual repair, restore the current
profile's source materials again, preventing the native base-cache mix-up.
Never change shader parameters, textures, UVs, scale factors or gameplay data.
Do not override destroyed-state visuals continuously.

Add a short, read-only renderer probe after activation to record material count,
shader, albedo identity/dimensions and property-block presence. Observe delayed
bindings as well as immediate assignment. This will show whether a subsequent
runtime path replaces the material. A repaired screenshot is needed before
claiming the user's grey-mask case is resolved.

The source extraction, hull renderer inventory and controlled texture renders
in this directory are retained as evidence. No graphics asset is repainted.

## User runtime observations

- [40 mm grey view](forty-mm-grey.png) and [closer detail](forty-mm-grey-detail.png).
- [10-inch correctly textured](ten-inch-textured.png).
- [6-inch on Frigate](frigate-six-inch.png), [heavy coil on Battleship](battleship-heavy-coil.png),
  and [heavy weapons on the corrected Mothership](mothership-heavy-guns.png).
- [0.10.6 runtime log excerpts](runtime-0.10.6-evidence.txt) confirm successful
  startup and applied preview/combat art profiles. The prior JSON startup failure
  is resolved in this run. Material appearance remains the separate issue.

The user's scale acceptance is recorded without expanding scope to human coils,
railguns, alien optics or further hull resizing. The Mothership screenshot
supports the roomy appearance of its present guns; it does not independently
certify every future enlarged optical system.

## Build and deployment status

0.10.7 source implements the narrow material pass and a bounded read-only
renderer probe at activation frames 1, 30 and 120. The probe records material
and shader names, albedo identity/resolution, color, UV scale, and property-block
albedo override. It does not continually rewrite materials or hide destruction
effects. Source/target mesh and one-submesh/one-material guards remain in force.
A material/probe failure keeps the accepted canonical dimensions.

An isolated build under `.tmp/gunpowder-art/material-build/` compiled and passed
the focused validator: 300 lifecycle assertions, eight caliber calibrations,
twelve invalid-schema cases, typed JSON round trip, native discovery exclusion,
six material-layout cases and both native repair-cache fields/patch targets.
Gameplay data and the eight scale profiles are unchanged. These checks do not
claim that the live screenshot problem is fixed.

**Deployed 0.10.7:** the first attempt correctly stopped while Terra Invicta was
running. After the user closed it, normal deploy.ps1 rebuilt, passed the full
40-validator flow and hash-verified all 48 deployed files. Both game-closed
assertions ran; verification was not skipped. The existing 1,204 formula
assertions, 300 managed lifecycle assertions and six material-layout cases pass.

DLL SHA-256: `875605E75CFDEC2B8078674F57225E4CE02D25B7D3D49A9BA0AD0C96CC451098`.
See [deployment log](deployment-0.10.7.txt). The user is testing the 40 mm mask
against the textured 10-inch comparison. The runtime cause is not considered
confirmed or resolved until that test and/or renderer trace establishes it.

The material pass remains uncommitted pending the user's test. Laser work,
including the compact alien optical-head proposal, remains deferred until the
user confirms this pass works and proceeds with that next phase.
