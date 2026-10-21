# Canonical gunpowder art — accepted 0.10.8 implementation

2026-10-21. **Implemented, deployed and user-confirmed working.** The user
confirmed that 0.10.8 resolves the invisible/background-cutout turret problem.
The successful runtime trace records **baked=True, nonInstanced=False**. This
supports the mesh-baked path; it does not establish the exact GPU-level cause
of the failing transform-scaled path. See the [acceptance record](gunpowder-art/manual-acceptance-2026-10-21.md).

## Scope and canonical dimensions

Only the eight existing gunpowder templates receive canonical art sizing.
Gameplay statistics, weapon tiers, projectile calibration, hull nodes and
capital fitting rules remain unchanged. No 25 mm weapon was added. Human
magnetic weapons, lasers and the compact alien optical-head proposal are
still deferred; the [canonical plan](canonical-turret-scaling-plan-2026-10-21.md)
and [projected tables](canonical-turret-projection/README.md) remain planning
evidence for that future work.

| Template | Caliber mm | Art factor | Unchanged mount |
|---|---:|---:|---|
| 30mmAutocannon | 30 | 0.199859 | OneHull |
| 35mmAutocannon | 35 | 0.233169 | HalfNose |
| 40mmAutocannon | 40 | 0.266478 | OneHull |
| 40mmNoseAutocannon | 40 | 0.266478 | HalfNose |
| 6-inchCannon | 152.4 | 0.981646 | OneHull |
| 8-inchCannon | 203.2 | 1.060939 | TwoHullHoriz |
| 10-inchCannon | 254.0 | 1.326173 | OneNose |
| 12-inchCannon | 304.8 | 1.181638 | TwoNoseVert |

Factors equal canonical caliber divided by the measured muzzle polygon's
inscribed diameter. Shared model resources have separate per-template factors,
including the 30/35/40 mm and 8/10-inch combinations. The
[barrel audit](barrel-and-lens-dimensional-audit-2026-10-21.md) retains the measured
inputs and distinguishes bores from receiver dimensions and full swept bounds.

## Accepted rendering path

[GunpowderArtGeometry](../../TIEconomyMod/Core/GunpowderArtGeometry.cs) creates
cached private copies of the base and moving meshes. It scales vertices by the
canonical factor and recalculates bounds, retaining source UVs, normals,
tangents, topology and materials. The resource root/child scales remain at
their original values; pivot and fire-point local positions receive the factor.
Fire-point scale also receives it, preserving the size of attached muzzle VFX.

This gives the same world-space geometry as multiplying the root scale:
`(s S)(p + R C v) = S(s p + R C(s v))`. It avoids the much smaller renderer
transforms introduced by the first implementation. No shared source mesh or
prefab is edited. Unreadable meshes fall back to canonical transform sizing
with a warning.

[GunpowderArtRuntime](../../TIEconomyMod/Patches/GunpowderArtPatches.cs) restores
native meshes, positions and scales before controller reuse or mode changes.
Absolute targets prevent accumulation. The source material arrays are bound
explicitly, and native repair caches receive the correct selected-weapon
materials. The repair postfix restores those materials for profiled gunpowder
weapons only. It does not continuously overwrite destroyed-state rendering.

Designer, combat and simple-hull views use canonical resource dimensions rather
than variant-specific placeholder scales. The overall ship transform still
applies. This differs from the historical projection's preview tables, which
preserved native placeholder scaling; the canonical combat dimensions agree.
Muzzle origins follow the resized gun, while projectile statistics and tracking
rules are unchanged.

## Profile schema and safeguards

The JSON-formatted catalog is stored as
[`Config/gunpowder-art-profiles.eeo`](../../TIEconomyMod/ModFiles/Config/gunpowder-art-profiles.eeo).
The non-JSON extension is essential: native ModManager recursively selects
paths containing `.json`, including Config files and `.json.backup` names.
This is a private visual schema, not a native template or save schema.

[GunpowderArtCatalog](../../TIEconomyMod/Core/GunpowderArtCatalog.cs) requires
schema 1, exactly eight approved IDs, finite calibration/scales, complete source
fingerprints and matching game-assembly/weapon-bundle hashes. Unknown fields,
future schemas, duplicates and malformed records reject. Runtime guards check
resource identity, scenario-aware caliber, mesh names/counts/bounds, hierarchy,
pivot and fire-point offsets. Invalid catalog input disables this art feature
with a warning while other mod features retain native art behavior.

The top-level mod toggle and saving settings refresh tracked controllers;
`shipBalance.enabled` gates application. Profile-file edits require mod/game
reload. The implementation adds no saved-game migration.

## Retained comparison controls

The EEO panel in Unity Mod Manager retains session-only controls useful for
regression testing. **Bake sizing into mesh copies** defaults on. **Disable GPU
instancing for gunpowder materials** defaults off and uses private material
copies when selected. Both preserve dimensions and gameplay data. Both off
reproduces the old transform-based path; that path is retained for comparison,
not recommended as the working default. See the [control guide](gunpowder-art/render-comparison.md).

Bounded probes log actual renderer state at activation frames 1, 30 and 120.
They read material/shader/texture identity, blend/depth settings, passes,
instancing, mesh scale, batching, extra vertex streams and camera paths. They
do not repaint surfaces. The native material-cache defect found during the
investigation was real, but material rebinding alone in 0.10.7 did not fix the
cutout; the evidence does not justify calling it the cutout's root cause.

## Validation and deployment

The normal `tools/deploy.ps1` flow ran with both game-closed assertions and
without SkipVerification. It passed **40 validator jobs**, **1,204 existing
formula assertions**, **300 managed lifecycle assertions**, **864 baked versus
transform coordinate comparisons**, six material-layout cases, twelve invalid
schema cases, typed JSON round-trip, source hashes and native JSON-discovery
checks. All 41 pre-existing root gameplay/localization/settings files are
byte-identical to the [pre-art baseline](gunpowder-art/gameplay-data-baseline.json).

Deployment hash-verified **48 files**. DLL SHA-256:
`483727F3FB55435FCBCAED420128176AADC41946C7AA5E311455AD614D377A87`.
[Deployment log](gunpowder-art/deployment-0.10.8.txt) and
[deployment readback](gunpowder-art/verification-0.10.8.json) retain the automated
evidence. [Manual acceptance](gunpowder-art/manual-acceptance-2026-10-21.md)
records the later successful user test. It is not an exhaustive in-game test
of every hull variant, caliber, repair sequence or rendering backend.

The [archived investigation](archive/gunpowder-rendering-2026-10-21/README.md)
preserves the 0.10.5 startup failure, 0.10.6/0.10.7 cutouts, rejected hypotheses,
source texture renders, logs and screenshots. Current behavior is described
here; historical pending-test statements there reflect their dated snapshots.
