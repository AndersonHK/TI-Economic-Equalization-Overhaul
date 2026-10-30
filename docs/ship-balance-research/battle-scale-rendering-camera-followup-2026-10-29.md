# Battle-scale rendering, trajectories and close zoom follow-up

2026-10-29. **0.10.10 is built and deployed for manual testing.** The automatic
checks pass. User confirmation of rendering, close-camera behavior and tactical
projectile appearance remains pending.

The user reports invisible stations and ships, oversized path/trajectory
visualizations, insufficient close zoom at physical scale, and side-view
free-look zoom passing the selected ship. The screenshots show the missing
solid geometry while markers, paths and some effects remain visible.

Retained manual evidence: [station screenshot](battle-scale/manual-invisible-station-2026-10-29.png),
[ship/side-view screenshot](battle-scale/manual-invisible-ship-2026-10-29.png),
and the [0.10.9 Player.log snapshot](battle-scale/manual-failure-player-2026-10-29.txt).

## Investigation and implemented correction

- Preserve the approved physical/cinematic world dimensions and mesh-fitted
  colliders. Reuse the accepted turret technique: private mesh copies with
  scale baked into vertices, keeping renderer transforms at their prior
  stable magnitude. Apply to ship and station solid renderers, preserving
  child world positions, weapon pivots/muzzles, materials and combat-exit
  restoration. The exact GPU cause remains unproven.
- Scale path/waypoint visual thickness and glyphs relative to the new model
  scale without shortening actual trajectories or changing future positions.
- Reduce the closest camera distance in proportion to model scaling (20x
  closer in physical mode), including world-camera clipping planes, and clamp
  free-look wheel travel before it can cross the focused/selected object.
- Add guarded native-binding and numeric geometry checks to the normal
  validation/deployment flow. Manual rendering and camera acceptance remain
  necessary after deployment; no commit is requested.
- The subsequent user request adds projectile tactical view at 50x closer
  range in physical mode. Scale the missile tracking envelope and its readable
  marker size; restore separate far-view ballistic markers above the same
  scaled envelope while retaining caliber bodies and colliders up close.

The installed game DLL still matches the pinned 1.0.53b SHA-256
`4A4B9AAE4154E444E9727204205D2D42AE8ED9E1C5F92CDC1280074A259D8350`.
The current Player.log identifies 0.10.9, records physical-mode collider
probes for station modules and ships, and has no corresponding proof that
solid meshes rendered correctly. The earlier
[accepted turret implementation](gunpowder-canonical-art-implementation-2026-10-21.md)
records that private mesh baking resolved the analogous user-observed defect.

## Ship and station render geometry

[CombatRenderGeometry](../../TIEconomyMod/Core/CombatRenderGeometry.cs) uses the
accepted turret method without enlarging physical objects. For each solid mesh,
vertices are multiplied by the model reduction (0.05 physical / 0.1 cinematic),
while its renderer transform is divided by that factor. Direct-child position
and scale receive the inverse compensation, preserving pivots, muzzles and
nested geometry. Co-located native collider geometry receives the vertex factor.
Generated damage boxes remain in the renderer's local frame and refit to the
new mesh bounds. UVs, normals, tangents, topology and materials are preserved.

Ship baking runs before its navigation bounds and mesh colliders are built.
Station baking runs after combat initialization has installed station weapons,
covering the station model and its non-weapon modules as well. Baking before
station weapon placement would have invalidated later authored mount offsets.
Native meshes and transforms are restored before strategic reparenting;
replacement damage meshes are also handled by the live baker.

Many native station and Dark Skies meshes disable CPU readability. The
[source manifest](battle-scale/mesh-fallback-sources-2026-10-29.json) records all
four input bundles, the installed DLL and generated file hashes. A reproducible
[read-only exporter](../../scripts/ship-balance/export_combat_mesh_fallbacks.py)
preserves the unavailable vertex attributes and submesh indices in private
compressed `.eeo` payloads. The 506 source entries resolve to 503 unique
name/count fingerprints and 435 distinct checksummed payloads. Duplicate
base/DLC radiator fingerprints are accepted only when their data matches.

The index loads compact metadata; only requested meshes are decoded. Base and
DLC data are separate files under 100 MiB each. Payload checksums, source DLL,
vertex counts, bounds and data layout are guarded. These files remain outside
native JSON discovery. The game bundles and shared source assets are not edited.

The earlier user-confirmed turret fix supports this choice. The screenshots and
old logs establish the visible failure, not its precise GPU/shader cause. A
successful bake or collider readback alone is not proof of correct rendering.

## Paths and camera

[CombatViewPatches](../../TIEconomyMod/Patches/CombatViewPatches.cs) fixes two
remaining world-space UI scales: trajectory thickness and waypoint gain/minimum.
They follow the model reduction; the waypoint maximum already uses the model
scale natively and is not scaled twice. Path coordinates, timings and actual
travel distances are untouched. Vectrosity strokes already expressed in screen
pixels retain their readable pixel widths.

The closest polar radius now follows the model reduction, permitting a baseline
of 100 m rather than 2 km in physical mode. An object-bounds standoff prevents
the close limit entering a larger object. The main and combat-UI cameras' near
clips receive the same reduction and are restored when combat ends. For the
observed 0.01-unit near plane, physical-mode clipping becomes 0.0005 units
(10 m). Far viewing distance and the earlier minimum speed settings are retained.

Free-look's position target is clamped **before** native interpolation. The
guard checks the entire dolly segment against the selected/focused object's
standoff sphere, including large steps whose endpoint is already beyond it.
It allows retreat from an inside start and does not prevent empty-space flight.
This addresses the separate overshoot that changing the polar-radius limit
alone cannot fix.

## Projectile tactical display

Native missile symbols hide inside a fixed 2-unit (40 km) camera envelope.
The new envelope follows the projectile reduction: **800 m in physical mode**
(50x closer) and 4 km in cinematic mode (10x closer than the former cinematic
baseline). Symbol sizing is evaluated in equivalent old distance units and
returned to current units, keeping its angular readability at the new switch.

Native ballistic flight effects do not have that explicit missile distance
switch. 0.10.9 disabled their renderers when adding caliber bodies. The new
ballistic tactical overlay reuses those flight effects above the same scaled
envelope, with a two-pixel minimum marker size. Inside it, only the physical
body remains. Tactical graphics do not enlarge colliders. UI hiding and shot
destruction suppress the overlays; pooled shots reuse them.
The [native particle baseline](battle-scale/tracking-particle-baseline-2026-10-29.json)
records the inspected sprite/tracer rendering and size-floor fields.

## Validation and deployment

Normal `tools/deploy.ps1` completed with **41 validators, 1,209 formula
assertions and 51 deployed files**. The matrix covers 113 features and 234
Harmony patches. The added coverage verifies all fallback payload data and
source hashes, the guarded path/waypoint/dolly rewrites and rejected empty IL,
model/projectile scale ratios, and seven dolly crossing/retreat cases.

Twenty-two affected Harmony targets resolve. Nineteen need Unity-host emission
confirmation because standalone .NET cannot execute their Unity ECalls. This
is explicitly not a scene-rendering or live-camera test.

The [final deployment log](battle-scale/deployment-0.10.10.txt) and
[readback manifest](battle-scale/deployment-manifest-0.10.10.json) identify the
installed build and package. DLL SHA-256:
`2A310BEBA75B956AC9F07F2DCDDDDC30271E48A7B541BD194C860B5753C14815`.
Earlier attempts stopped before copying: first on a reflection argument wrapper
and missing validator dependency, then on archive omission of the mesh payloads.
The corrected normal flow verifies the packaged payload hashes before copying.

The final broad readback also exposed a pre-existing archive limitation: its
explicit file list omits eleven older template/localization files and the PDB.
The installed deployment includes all 51 source files; the 39 files present in
the release ZIP match their sources, including all new mesh data. The manifest
names the omissions and confirms they were also absent in the 0.10.9 archive.
This local testing deployment is complete; the older archive-list defect was
not broadened into a separate packaging refactor here.

No commit or push was requested. The 0.10.9 work and this follow-up remain
uncommitted.

## Manual acceptance

Retest the same station battle and shrunken ships from the supplied screenshots,
using both scale modes and both follow/free-look cameras. Check solid hulls and
station modules before/after damage, turret aiming, and strategic return/re-entry.
Check both base and Dark Skies appearances, including station parts requiring
the unreadable-mesh fallback.

At maximum close zoom, test bow, stern and side approaches, repeated large wheel
events, reversal and Ctrl scrolling. Confirm the camera can inspect the model
without clipping it away or crossing it. Check waypoint picking and drag gizmos
after the reduced visual scales; the actual trajectory endpoints must still
match the flight path.

Observe gunpowder, magnetic and plasma shots and missiles while crossing the
800 m physical-mode tactical-display boundary, with debug UI shown and hidden.
Check pooling, impacts and destruction leave no stale marker.

New log prefixes are `Combat render bake:`, `Combat render bake readback:` and
`Combat close camera:`. Their numeric readbacks support diagnosis; user visual
confirmation is still required before declaring the rendering regression fixed.
