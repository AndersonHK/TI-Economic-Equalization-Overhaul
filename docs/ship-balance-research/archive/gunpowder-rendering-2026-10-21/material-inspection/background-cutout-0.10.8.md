# Background-shaped gunpowder cutout: 0.10.8 comparison

**Archived 2026-10-21.** Historical planning/investigation state; later outcomes supersede pending or provisional conclusions below. See the [current authority](../../../gunpowder-canonical-art-implementation-2026-10-21.md). Original numerical evidence is retained.

2026-10-21. **0.10.7 did not fix the reported problem.** The new screenshots
show Earth/sky through the turret silhouette while the hull behind it is hidden.
This is not adequately explained by an untextured grey surface. The apparent
depth/color-pass mismatch is a diagnosis to test, not yet a proven shader defect.

The 0.10.7 renderer traces show the correct Standard material, albedo, UV scale,
opaque color, one material/submesh and no property-block override at frames
1, 30 and 120. The source materials have default render queues, no disabled
passes, opaque blend settings and enabled GPU instancing. All inspected albedo
alpha pixels are 255. Source meshes are CPU-readable with unit normals/tangents.
Thus repeated material rebinding is not a demonstrated solution.

## Controlled comparison

0.10.8 prepares two session-only controls in the mod's settings panel:

1. **Bake sizing into mesh copies** (default on). Clone source meshes, multiply
   only vertices by the canonical factor, recalculate bounds and retain all
   source UVs, normals, tangents, materials and topology. Keep resource root
   scale; multiply child pivot and fire-point local positions by the factor.
   Multiply fire-point scale too, preserving attached VFX size. This changes
   the representation, not world geometry or caliber.
2. **Disable GPU instancing for gunpowder materials** (default off). Use cached
   private material copies with only `enableInstancing` changed. This isolates
   the instanced draw path without editing a shared source material or shader.

The four combinations can be compared without restarting, adding weapons or
changing the camera. Both unchecked reproduces 0.10.7's rendering representation.
No unlit shader, recoloring, alpha workaround or laser change is included.

For source vertex v, child scale C, rotation R, pivot p, root scale S and scalar s:

`(s S) (p + R C v) = S (s p + R C (s v))`.

The same identity applies to muzzle location. Native mesh/position/scale state
is restored before controller reuse or mode switching, preventing accumulation.
The profile catalog and all gameplay data stay unchanged. Unreadable geometry
falls back to the existing canonical transform path with a warning.

Expanded probes record actual queue/blend/depth state, enabled shader passes,
instancing, mesh identity, world scale, layer, static batching, additional vertex
streams and active camera paths. Runtime observation is still required to identify
which path causes the cutout. The build must pass normal guarded deployment
before being described as ready for testing.

## Deployment and test instructions

**0.10.8 is deployed.** The full normal flow passed 40 validators, 1,204 existing
formula assertions, 300 lifecycle assertions, six material-layout cases and
**864 compiled baking-versus-transform coordinate comparisons**, including
source mesh-bound corners and muzzle points under four elevation angles.
All 48 deployed files were hash-checked; gameplay data and profile calibrations
are unchanged. A first verification attempt stopped on a PowerShell array
expression in the new test; it was corrected and the entire flow reran without
skipping checks.

DLL SHA-256: `483727F3FB55435FCBCAED420128176AADC41946C7AA5E311455AD614D377A87`.
[Deployment log](../../../gunpowder-art/deployment-0.10.8.txt).

Test the default first on the same 40 mm setup. In Unity Mod Manager's EEO
settings, find **Gunpowder render comparison (same dimensions; session only)**.
If the cutout persists, enable the GPU-instancing checkbox. Turning both boxes
off reproduces 0.10.7's representation. Changes apply immediately to tracked
gunpowder renderers and rearm their probes; restart resets the session defaults.
Record which combination changes the cutout. The candidate is not called a
confirmed fix until the live comparison succeeds.

The new [sky-background](background-cutout-sky.png) and
[Earth-background](background-cutout-earth.png) screenshots establish that the
earlier “grey mask” description missed the important background-reveal behavior.
The [0.10.7 trace](runtime-0.10.7-cutout.txt) records the correct bound material
despite that visible failure. No new commit or laser implementation is included.
