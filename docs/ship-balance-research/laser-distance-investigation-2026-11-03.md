# Laser beam continuity at distance: investigation and plan

**Superseded priority, 2026-11-04:** The user corrected the distant chunks to
generally one pixel tall and supplied original-resolution samples. Their
approximately 53-pixel repetition matches successive pixel-row crossings.
[The current core-sizing and sampling plan](laser-core-and-subpixel-plan-2026-11-04.md)
prioritizes lens-sized cores and pixel-footprint sampling. The multi-pixel-pill
description and halo-first priority below are historical; the noise-mask maths
remains a possible secondary mechanism, not an established cause.

2026-11-03. **Planning only.** The user reports that the width-only laser
correction looks right close up, but individual bright sections become shorter
when viewed from farther away. No gameplay code, settings, materials, build or
installed files were changed for this investigation. Existing capital-mount and
hull work in the shared checkout was preserved.

## Findings

**Second pass, following the user's clarification:** the symptom is regularly
repeated, several-pixel-tall pills, not merely a thin line intermittently
covering a pixel. The desired result is a physical thin line whose projected
width grows naturally from intermittent subpixel/one-pixel coverage to larger
sizes with distance/FOV. No fixed one-pixel minimum or enlarged ribbon is wanted.

The shader has a mechanism that can produce these bright islands: a tiled noise
field is converted into narrow **contour bands around an animated noise value**
and added as a halo around a continuous core. This nonlinear shaping occurs
**after** the mip-filtered texture sample. Mip averaging can therefore create
bright halo patches that were absent in the finer samples. The stored HDR colors
make even fairly small halo values conspicuous. This is a stronger lead for the
specified pill pattern than simply blaming thin geometry.

Our width mapping also deserves inspection: it sets the **entire ribbon** to
the lens diameter, while the native shader's bright core occupies only a small
fraction of that ribbon. The first controlled comparison should isolate that
core from the noise halo and inspect raw versus postprocessed output. It should
not change width and texture sampling simultaneously.

The existing correction additionally cancels ship scaling on **one of two
camera-plane axes**, not on the beam's length axis. The old U-as-longitudinal /
V-as-transverse explanation was incorrect. Two-axis cancellation remains a
small, justified follow-up experiment, but it does not by itself explain why a
continuous core would disappear at distance.

There is no beam mesh LOD switch in the audited controller/prefab path, and the
beam shader has one subshader/pass and one D3D11 vertex/fragment program pair.
However, its noise textures have mip chains, and its noise sample uses implicit
mip selection. **Texture LOD acting through the nonlinear halo is a concrete
candidate; geometric LOD is not supported by the inspected evidence.** Thin-line
coverage remains normal rendering behavior, not the leading corrective policy.

The [compact audit](battle-scale/laser-distance-audit-2026-11-03.json) retains
hashes, shader semantics, material values, mip settings and illustrative maths.
The installed native assembly hash still matches the earlier audit. The current
checkout manifest is 0.10.23; the two laser source files are unchanged from HEAD.

## Controller and shader trace

1. Native `BeamEffectController.Awake` obtains the LineRenderer, enables world
   space and captures its per-effect material's initial texture scale.
2. `OnEnable` assigns `initialTextureScale / max(transform.lossyScale)` to both
   texture axes. `ShipWeaponVisController.Fire` then reaches our binding postfix.
3. [LaserBeamGeometry](../../TIEconomyMod/Core/LaserBeamGeometry.cs) sets the
   measured-aperture `widthMultiplier` once on binding. Its
   `LaserTexturePeriodState.Configure` returns `(captured.x * modelRatio,
   captured.y)`. Active refiring reuses the captured value; disable restores it.
4. [BattleLaserRibbonPatch](../../TIEconomyMod/Patches/LaserBeamCameraPatches.cs)
   bypasses native `OnWillRenderObject` for bound combat lasers. The native
   combat callback sets endpoint widths to `max(inheritedScale * beamWidth,
   cameraDistance * 0.005)`. It does not change endpoints or texture scale.
5. Native `BeamWeaponController.LateUpdate` updates the actual emitter/target
   endpoints. No camera-distance segment-length or UV rewrite was found there.
6. The compiled `TerraInvicta/Beam Weapon Line` shader transforms vertices into
   **view space**, then computes noise coordinates from `viewPosition.xy *
   _MainTex_ST.xy + _MainTex_ST.zw`. Animation shifts the noise sample's Y.
   The LineRenderer UVs serve a different purpose: cross-ribbon core shape and
   variation from beam start to end.

The shader's `_LaserDistanceFalloff` name is misleading for this investigation.
It raises the normalized along-beam UV coordinate to a power; it is not a
camera-distance fade. Material leftovers such as `_LaserDistanceScale`,
`_NoiseScale` and `_MinClipSpaceSize` are not bound parameters in this compiled
shader and should not be treated as working controls.

## How the shader can form repeated pills

This is not a sequence of pill sprites emitted down the path. All eleven audited
laser/arc/phaser prefabs have constant unit width curves and no firing particle
system. Their impact particles are placed at the target. The inspected texture
images are noise fields, not isolated pill decals.

The fragment program combines these quantities (simplified from the installed
bytecode, with the animation pulse set to zero for the examples):

```text
u = along-beam coordinate, clamped to at least 0.01
q = abs(cross-ribbon coordinate - 0.5)
radius = StartRadius + MaxRadius * u^DistanceFalloff
core = 1 - saturate(q^Falloff / radius)
d = max(q - radius, 0)
g = saturate(u / NoiseStart)
level = 0.5 + 0.1 * sin(10*pi*time)
mask = 1 - saturate(abs(noise - level) * d^2 / (NoiseThreshold^2 * g))
halo = NoiseOpacity * (1 - saturate(2*d))^NoiseExponent * g^2 * mask
output = (core + halo) * HDRColor
```

The `d=0` expression has the limit `mask=1` for finite noise; positive `u` and
zero NoiseStart give `g=1`. The full function also animates the threshold and
core exponent with the existing pulse. Crucially, the halo is strongest where
noise is **near** the moving level, rather than simply where the texture is
bright. Repeated or mirrored noise cells can produce repeated bright contour
crossings. Along a narrow ribbon, these can look like isolated rounded bulges.

A numerical example from the native ArcLaser parameters at `u=0.5`, zero pulse
and `level=0.5`: at `q=0.25`, noise samples 0.3 and 0.7 both produce **zero** halo,
but noise 0.5 produces **0.0363** halo. The red arc's native color setup converts
that to approximately **0.71 red intensity** before postprocessing. Thus an
arithmetic average of the two dark input samples creates a visibly bright
result after the mask. Averaging the already-shaped outputs would remain dark.
This is a worked sampling-order example, not a claim to have measured a live mip.

Texture minification can therefore change the size and prominence of islands,
not merely shorten all features by normal perspective. Bloom could expand those
HDR islands further; its actual contribution must be checked before calling it
the cause. The analytic core itself is independent of the noise sample and has
no periodic cuts. If a well-resolved core is absent before postprocessing, the
next investigation must check actual renderer state and shader inputs rather
than assigning that absence to the noise texture.

## The ribbon diameter is not the visible core diameter

`Configure` currently replaces the native width multiplier with measured lens
diameter. Native `beamWidth` values include 6 for arc lasers, 0.8/1.1 for phasers
and 1-2.5 for the standard/alien laser prefabs; those multipliers are discarded,
though their unit width curve and material profile are retained.

The material profile then narrows the continuous core again:

| Material | Positive-core width at start / full ribbon | Positive-core width halfway / full ribbon | Core FWHM halfway / full ribbon |
|---|---:|---:|---:|
| Standard/alien laser | 12.1% | 31.8% | 14.7% |
| Arc laser | 12.1% | 20.7% | 11.9% |
| Phaser | 31.8% | 45.5% | 26.2% |

These are zero-pulse shader calculations, not measured visual widths. Positive
support extends to `2 * radius^(1/Falloff)`; FWHM is
`2 * (radius/2)^(1/Falloff)`, as a fraction of ribbon width. For an illustrative
12-pixel arc ribbon, the core halfway along it has about **2.48 pixels of
positive support and 1.42 pixels FWHM**, while a noise-selected island can be
visible three pixels away from the center on either side. Several-pixel-tall
bulges are therefore compatible with a much thinner underlying line, without
requiring floating-point precision loss.

This is not a recommendation to multiply all current widths by an inverse
fraction. Such a change would alter the accepted close appearance and could
enlarge the noisy envelope. First establish whether the intended lens width
means core width or outer glow width, then compare measured widths. The target
remains the physical line's normal projected width, not the glow's diameter.

## What the one-axis correction leaves behind

Let `S` be the vanilla inherited scale for the same mode and `r` the configured
model scale divided by the vanilla reference. Native activation yields texture
scale `(initialX, initialY) / (S*r)`. Our correction yields:

```text
current = (initialX / S, initialY / (S*r))
vanilla = (initialX / S, initialY / S)
```

Both axes feed camera-plane noise sampling. Leaving Y alone therefore changes
the rate at which a vertical or diagonal beam crosses the noise field. For the
default 10x ship setting, `r=0.5` with cinematic mode off and `r=0.05` with it on:

| Mode | Horizontal UV rate versus vanilla | Vertical UV rate versus vanilla | 45-degree UV rate versus vanilla |
|---|---:|---:|---:|
| Cinematic off | 1x | 2x | 1.58x |
| Cinematic on | 1x | 20x | 14.16x |

The table describes texture-coordinate travel for equal initial X/Y tiling,
which the audited laser/arc/phaser materials have. It does not predict the exact
length of every bright patch in a two-dimensional noise texture. Camera rotation
changes the direction through that field. Camera translation can change its
phase. Dolly distance also changes its projected size and sampling footprint;
there is no explicit distance multiplier on the texture coordinates themselves.

## LOD, filtering and coverage

The `spacecombat` and `vfx` bundles contain no LODGroups. The eleven audited
standard/alien laser, arc and phaser ribbons have two positions and use the
same beam shader. Its single `LOD 100` declaration is a shader quality rating,
not a chain of distance-selected beam representations. This does not certify
every external visibility system, but it gives no basis for changing ship LOD.

| Noise texture | Size | Mip levels | Serialized filter | Anisotropy | Mip bias | Streaming |
|---|---:|---:|---|---:|---:|---|
| noiseBurnUp | 1024x1024 | 11 | Bilinear | 1 | 0 | Off |
| ArcLaser | 512x512 | 10 | Bilinear | 1 | 0 | Off |
| Phaser | 256x256 | 9 | Bilinear | 1 | 0 | Off |

The fragment program uses an ordinary texture `sample`, with no explicit mip
index or bias. Its noise passes through nonlinear brightness shaping, so a
different mip can change the extent/contrast of bright patches without changing
the ribbon's endpoints. Streaming being off does not disable mip selection.
Live mip choices have not been captured.

The distinction is between ordinary thin-line coverage and a surviving noisy
halo. A line becoming intermittent at less than a pixel is allowed by the user's
target behavior. That does not explain away larger periodic pills, nor justify
inflating the line to a fixed pixel width. The core/halo separation above is now
the primary diagnostic; the rejected 0.10.18 renderer replacement remains rejected.

For scale only: a 1 m aperture at ship scale 10 gives a 0.0005-unit ribbon.
At 2160-pixel viewport height and 60-degree vertical perspective FOV, its full
width would be approximately 1.87 pixels at 10 km camera depth, 0.94 at 20 km
and 0.19 at 100 km. The actual aperture, FOV, depth and camera settings must be
measured; these are calculations, not observations of the user's battle.
For a camera-facing width at depth `z`, the perspective relationship is:

```text
projectedWidthPixels = worldWidth * viewportHeight / (2 * z * tan(verticalFOV/2))
```

This should describe the intended physical line/core, with the material profile
accounted for separately. There should be no clamp to one pixel, jump at a zoom
threshold, distance-based texture stretching or forced segment-length increase.

## Candidate fixes and order of experiments

| Option | Purpose | Assessment |
|---|---|---|
| Temporarily set noise opacity to zero; compare output before and after postprocessing | Identify whether the pills belong to the halo and whether bloom expands them | First comparison. Preserve width, texture scale, endpoints, color, core and FOV. If pills remain in the raw core, inspect the active renderer/shader state next. |
| Measure physical aperture, full ribbon, core and glow widths separately | Check the direct aperture-to-full-ribbon assignment | Audit before any calibration change. Preserve the accepted close appearance; do not blindly enlarge widths by the inverse core fraction. |
| Cancel inherited scaling on both texture axes, once per activation | Restore the native two-dimensional noise scale at any beam angle | Small independent correction. Compare island spacing/angle dependence; do not promise it fixes minification or core continuity. |
| Compare texture filtering/mips on a private, prepared texture copy | Isolate raw-noise minification feeding the nonlinear mask | Diagnostic. Pinning a fine mip can establish sensitivity but can shimmer. Trilinear filtering smooths transitions; it does not solve the sampling-order problem by itself. |
| Filter or reduce unresolved halo modulation while preserving the continuous core | Make noisy detail converge to a smooth thin-line appearance under minification | Conditional production direction if the first comparisons confirm the halo. Correctly filter the shaped intensity or use a validated approximation; preserve close noise/color and ordinary projected width. GPU cost must be bounded. |

The two-axis experiment would change `LaserTexturePeriodState` to cancel `r` on
both components of the captured activation scale. It would retain restoration,
noncompounding active refire and recapture after reuse. Current validation
explicitly requires V to remain unchanged; those assertions must be revised to
test **both camera-plane axes against the native reference**, rather than simply
asserting the existing implementation.

Filtering experiments must not mutate the shared vanilla texture. Any private
copy/cache and material setup belongs before battle or at activation, with
explicit cleanup and restoration. No per-frame texture construction, CPU mesh
work, collider writes or camera-driven UV rescaling is proposed.

The earlier proposal to expand raster coverage is removed from the corrective
plan. Standard projection and antialiasing should determine the physical line's
pixel footprint. If the halo is responsible, work on its minification behavior
inside the existing rendering path instead. Simply disabling mipmaps, shifting
texture offsets, changing LineRenderer texture mode or lengthening texture cells
with camera distance would not establish a correct solution. In particular,
LineRenderer tiling mode does not change this shader's camera-plane noise input.
No new renderer, solid-beam substitution or restored distance width floor is
planned.

## Validation plan before choosing a final patch

1. Capture the same firing pair at several camera depths with fixed orientation,
   including a range where pills are several pixels tall. Record actual aperture,
   world endpoints, full ribbon width, material/core profile, texture scale, FOV,
   resolution and quality settings. Account for animated noise phase; identify
   whether the effect is standard laser, arc or phaser.
2. At the same poses, compare the native appearance and core-only diagnostic
   before and after bloom/other postprocessing. Measure the thin line separately
   from the bright island envelope. This distinguishes periodic noise structure
   from whole-ribbon width, core visibility and postprocessing enlargement.
3. Compare core projection with the FOV/depth calculation, and island length with
   local repeat spacing and projected beam length. Ordinary perspective should
   shrink all world-sized features smoothly. A larger dark fraction or growing
   island/core prominence is the defect; intermittent subpixel line coverage is
   acceptable. Do not force a one-pixel minimum.
4. Compare two-axis cancellation and isolated mip/filtering controls separately,
   including horizontal/diagonal/vertical angles, cinematic off/on, default 10x
   and another ship scale. Keep endpoints and the accepted close width fixed.
   A graphics capture can verify the actual shader, raw pixel values, constants
   and sampling footprint. Do not treat a material name or numerical example as
   a reproduction of the user's render.
5. Check refiring while active, disable/re-enable, pooling, stations and different
   camera angles. Confirm setup still occurs on activation, and compare battle
   performance before accepting any shader/filtering fallback.

These experiments require a separately authorized implementation/testing pass.
For this planning pass, native asset inspection and shader disassembly ran
without a Unity scene, rendering, audio or game launch. No build, deployment,
commit or push was performed. The earlier close-view acceptance remains valid;
the newly reported distant-view defect is open.
