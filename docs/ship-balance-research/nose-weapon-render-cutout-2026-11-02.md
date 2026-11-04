# Nose weapon background-cutout correction — 2026-11-02

## Plan and evidence

The user reports background-shaped cutouts on some nose weapons, matching the
previously accepted [mesh-baked gunpowder correction](gunpowder-canonical-art-implementation-2026-10-21.md).
The screenshot identifies Heavy Rail Cannon Mk3 on a human Battlecruiser, outside
the capital fitting change. Its native resource `shipweapons/large_railgun_cannon`
has root scale `0.0634083152` and moving-part scale `1.17842889`; the corresponding
large rail battery uses root scale `0.100879997` and moving-part scale `1`.
The eight calibrated conventional-gun profiles do not include rail cannons.

The current 10x combat trace already records whole-model mesh baking with vertex
factor `0.5`, but that factor alone does not keep every small nose renderer's
matrix at a useful scale. Extend the existing combat mesh-baking pass for nose
weapon renderers: choose a per-renderer factor by halving the normal combat
factor until every nonzero world-matrix axis is at least `0.001` after inverse
transform compensation. Power-of-two steps bound cache variants and avoid a
separate mesh for tiny floating-point differences between ship instances.

Bake vertices and collider geometry by that factor and compensate the renderer
scale, child pivots/scales and muzzle hierarchy inversely, exactly as in the
existing pass. Preserve normals, tangents, UVs, topology, materials and world
geometry. Preserve the ordinary factor on hulls, hull weapons and other renderers.
Do this only during model preparation; restore original instance state through
the existing return-to-strategy/destruction path. No repeated frame-time baking,
weapon-stat changes, global prefab edits or material replacement.

Add compiled checks for tiny/normal/invalid and mirrored/nonuniform matrix axes,
both combat scaling modes, and world-space mesh/pivot/muzzle equivalence. Run the
normal deployment before requesting a visual comparison of the same cannon.

## Implementation and validation

Implemented and deployed as 0.10.23. The adjustment identifies the base and
moving MeshFilters through each nose controller's explicit object references,
including inactive weapon objects, rather than assuming where the controller
component sits in the hierarchy. Existing whole-model preparation, CPU-readable
fallbacks and mesh caches remain shared. Each part records its own factor for
the bounded readback; the original state restores through the existing lifecycle.

Validation on 2026-11-02 passed all 41 jobs, 1,226 formula assertions, the
120-row / 255-patch implementation matrix and 1,008 added coordinate comparisons.
The added checks exercise the measured heavy-rail scale, binary cache steps,
minimum matrix-axis floor, tiny/normal/invalid axes, mirrored/nonuniform parent
scales and aimed mesh/muzzle points. Compiled-method inspection verifies that
the selector is wired into the actual mesh-preparation method. Existing combat,
laser-aperture, conventional-gun baking and capital migration checks also pass.

`tools/deploy.ps1` asserted that the game was closed before validation and again
before replacement. All 52 installed files match the final package.
DLL SHA-256: `7173D6DB1132094A6498F4F7D4F40F277CF212E06E3E944211C12B0B0C9B4383`.
Package: `artifacts/TIEconomyMod-0.10.23-ti1.0.53.zip`.

The first verification attempt found a PowerShell expression-grouping error in
the added coordinate checks. It was corrected; focused checks and the complete
normal deployment flow then passed without skipping validation.

Manual acceptance remains pending: reproduce the Heavy Rail Cannon Mk3 view,
check both the base and moving barrel against hull, sky and planet backgrounds,
then check aiming, firing, damage/repair and return-to-strategy reuse. Runtime
logs now report `noseParts` and `minimumNoseRendererAxis` with the existing
two-frame mesh-bound readback. Numeric equivalence and successful deployment
do not establish that the GPU cutout has disappeared; the screenshot comparison
is the acceptance test. The exact shader/GPU cause remains unproven.
