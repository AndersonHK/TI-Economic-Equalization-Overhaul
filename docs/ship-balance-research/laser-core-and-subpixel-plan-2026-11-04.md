# Lens-sized laser cores and subpixel sampling

**Current correction:** The user rejected 0.11.3's cone and close-up flow
regressions. [0.11.4 restores native vertices and the resolved profile](laser-native-profile-restoration-2026-11-04.md),
keeps the fixed aperture/0.46 ribbon calibration and limits filtering to
unresolved cores. The stable-support replacement proposed below was not visually
acceptable; these notes remain the investigation history.

**Implementation follow-up:** The user approved both fixes and a fixed 46%
core/ribbon estimate. [0.11.3 is deployed for testing](laser-core-sampling-implementation-2026-11-04.md),
with native flow retained and runtime shader loading/binding confirmed.
The following records the pre-implementation investigation and alternatives.

2026-11-04. **Research and planning only. No gameplay code or settings changed.**
This supersedes the diagnostic priority in the
[November 3 investigation](laser-distance-investigation-2026-11-03.md).
The user corrected the distant-beam observation: the bright chunks are generally
one pixel tall; contrast made them look larger. The user also measured roughly
40 pixels across a laser cavity and 20 pixels across its solid beam, on a
360 cm hull phaser. The lens must size the **core**, not the whole ribbon.

## Evidence from the supplied screenshots

Both attachments are retained without resizing or modification:

- [Distant-beam crop, 172x29](battle-scale/manual-laser-subpixel-crop-2026-11-04.png).
- [360 cm UV phasers and visible turrets, 3840x2160](battle-scale/manual-laser-360cm-phaser-2026-11-04.png).
- [Measurements, hashes and numerical examples](battle-scale/laser-core-sampling-audit-2026-11-04.json).

The small crop contains three similar bright patches. Selecting pixels with
green >=150, outside the ship silhouette, gives these zero-based coordinates:

| Patch | Bright row | Horizontal interval | Weighted center X |
|---|---:|---:|---:|
| First | 16 | 32-48 | 39.94 |
| Second | 15 | 85-101 | 93.07 |
| Third | 14 | 138-155 | 146.46 |

Their centers are 53.13 and 53.39 pixels apart, while the line rises exactly
one pixel row per patch. The fitted slope is -0.01878, or about -1.076 degrees;
its one-row crossing period is **53.26 pixels**. This is strong evidence of
**pixel-phase aliasing**: the bright sections coincide with successive row-center
crossings. It does not independently identify whether the missing coverage comes
from the rasterized quad, the fragment shader's core profile or a later resolve.

This evidence changes the priority. The previous noise/mip explanation remains
a possible secondary contributor, but the repeated chunks no longer justify
treating large textured halo islands as the leading explanation.

The large image also supports the user's near-view measurement. At X=1540,
the lowest firing beam spans 22 vertical pixels at green >=128, 20 at >=180,
and 15 at >=220, with blue >=200. Neighboring cuts have similar widths.
These are display-color thresholds, not linear-HDR FWHM measurements. The user's
40-pixel cavity measurement is retained as reported; the exact clear-aperture
edge was not independently fitted. A proper comparison measures across the beam
normal and the optical opening in that same direction, near the muzzle and at
comparable depth. Rim versus clear opening, angle, HDR saturation and bloom
affect the exact ratio, but do not invalidate the observed undersized core.

## Problem 1: physical core diameter

The current
[LaserBeamGeometry.Configure](../../TIEconomyMod/Core/LaserBeamGeometry.cs)
measures a graphical clear-aperture diameter `D`, then assigns
`line.widthMultiplier = D`. The shader subsequently narrows the continuous core
inside that ribbon. Thus the code currently equates the aperture with a
**rendering envelope**, not with the emitting core.

The stock `360cmUVPhaserBattery` uses `shipweapons/large_laser_battery` and
`spaceCombat/Phaser Beam Violet`. This is the Phaser material audited previously,
not the ArcLaser example used in the preceding turn. The current mod laser JSON
contains partial overrides; the full model/effect fields come from the stock
template. The 360 cm name is an identifier here: continue using actual graphical
clear aperture for render sizing rather than deriving a diameter from the label.

For that material the native core is:

```text
q = abs(cross-ribbon UV - 0.5)
radius = StartRadius + MaxRadius * u^DistanceFalloff
p = Falloff * (1 + pulse)
core = max(0, 1 - q^p / radius)
positive-core width / ribbon width = 2 * radius^(1/p)
```

At the emitter's clamped `u=0.01`, the Phaser source radius is about 0.10025.
Its pulse changes the exponent, and therefore the physical width even if the
ribbon multiplier is constant:

| Pulse state | Core support / ribbon | Support for a hypothetical 40 px ribbon |
|---|---:|---:|
| Minimum | 17.1% | 6.83 px |
| Zero pulse | 31.8% | 12.70 px |
| Maximum | 46.0% | 18.40 px |

These are geometric-support calculations before HDR/bloom, not a fit proving
the screenshot's pulse or actual ribbon width. They explain why a bright
20-pixel-looking core inside an approximately 40-pixel opening is plausible.
They also show why **a universal 2x multiplier is not a general correction**.

Recommended sizing contract:

1. `D` is the graphical clear-aperture diameter in world units, captured at
   creation/reuse. It defines the physical core diameter at the emitter.
2. The ribbon is a carrier large enough for the core, glow and sampling support.
   Its full width is not the physical laser diameter.
3. Normalize the shader's radial coordinates/profile against the physical core.
   Keep vanilla color, animated noise and brightness character where compatible.
   Glow is allowed outside the core; it does not determine the core diameter.
4. The proposed default is a stable core envelope. Existing pulse and along-beam
   flare should affect the profile/glow without accidentally shrinking the
   emitting diameter. This is a visual normalization, not a new diffraction,
   focus, damage or energy simulation.

A managed-only multiplier can normalize a **reference phase**: for the Phaser's
zero-pulse emitter support it would make ribbon width about `3.15*D`. It cannot
keep the core at D across native pulse phases and along-beam profile changes.
That is a useful sizing experiment, not a complete implementation of the above
contract. Do not bake in the Paint ratio or perform pulse-dependent CPU rescaling.

The physical edge, intensity profile and measured bright plateau must remain
distinct. FWHM is useful for validation, but a saturated screenshot's white
band is not an exact physical boundary. Widening the core intentionally changes
its rendered footprint; subpixel filtering should conserve the energy of the
chosen new profile, rather than silently restore the old undersized footprint.

## Problem 2: sampling the projected core

The installed fragment program declares ordinary pixel-frequency linear
interpolation and evaluates the analytic core at that interpolated coordinate.
There is no footprint integral or derivative-based filtering of the core.
Filtering the noise texture's mipmaps does not filter this separate core formula.
Geometry MSAA alone is also not a guarantee that the procedural core is evaluated
or averaged correctly within each covered pixel.

For perspective projection, a camera-facing physical width approximately maps to:

```text
widthPixels = D * renderTargetHeight * projectionMatrix[1,1] / (2 * viewDepth)
```

Use the active camera/render-target projection and resolution, not a fixed FOV
or the displayed screenshot size. The shader's radial profile maps this envelope
to its intensity distribution. There should be no fixed minimum bright width,
distance-dependent texture-length change or jump at a chosen zoom level.

When a narrow core is sampled only at pixel centers, it can be full-bright at
one crossing and absent for many intervening columns. The crop's shallow slope
makes those intervals long. A pixel should instead receive the average radiance
over the portion of its footprint intersected by the core.

A one-dimensional numerical illustration uses a 0.3-pixel-wide core with profile
`max(0, 1 - (2*abs(x)/0.3)^1.25)`, before HDR/tonemapping:

| Core center offset | Point sample in row 0 | Integrated row 0 | Integrated row 1 | Total across rows |
|---|---:|---:|---:|---:|
| 0 px | 1.000 | 0.167 | 0 | 0.167 |
| 0.2 px | 0 | 0.167 | 0 | 0.167 |
| 0.4 px | 0 | 0.157 | 0.010 | 0.167 |
| 0.5 px | 0 | 0.083 | 0.083 | 0.167 |

This is an arithmetic box-filter example along the normal axis, not a Unity
render or a complete rotated-pixel reconstruction. It demonstrates the goal:
energy moves smoothly between pixels instead of creating full-bright periodic
chunks. A still thinner core contributes proportionally less radiance. This
does not turn it into a constant-brightness one-pixel beam.

## Proposed implementation direction, still unimplemented

First establish the core sizing contract in a controlled near-view experiment.
Then test footprint-aware filtering of the same native-style radial core. The
most direct shader approach uses the projected cross-core coordinate and its
pixel derivatives to integrate or approximate the profile over a pixel. Preserve
native endpoints, line length, texture animation and effects; do not reintroduce
the rejected Shapes/solid-beam replacement.

There are two sampling boundaries to check:

- If the ribbon already covers the relevant pixels, filtering the procedural
  core may be sufficient. The larger carrier needed for lens-to-core sizing may
  improve this, but cannot guarantee coverage at arbitrary distances.
- If rasterization never invokes the fragment shader for a pixel intersected
  by the thin core/filter footprint, a small padded carrier or adequate
  multisample treatment is also needed. Such support must be weighted by true
  fractional radiance. It changes where integration can run, not D, beam length,
  colliders or a minimum bright beam width.

This distinction revisits the blanket removal of raster-support work in the
November 3 plan. Rejecting a minimum visible-width floor remains correct, but
sampling support may still be necessary to integrate a genuinely thin line.
Do not add it until the actual missing-coverage stage is identified.

Keep D and other physical scale inputs immutable after creation/reuse. Changing
camera projection and pixel footprints belong in the graphics pipeline, alongside
ordinary vertex projection, rather than CPU loops that resize every beam each
frame. No new frame-time mesh preparation, collider writes or resource allocation
is proposed. Shader cost must be measured with the intended battle workload.

The native shader audit/export is disassembly, not usable shader source. Before
committing to a shader implementation, establish a compatible shader asset/build
path for the installed Unity/game version. A material-only sizing adjustment
and a correct sampling shader are different scopes; do not present the former
as proof the latter has been solved.

## Validation sequence

1. Record actual aperture D, ribbon width, Phaser parameters/pulse, camera
   projection, render-target size and AA/postprocessing mode. Compare the same
   clear opening and nearby core perpendicular to the beam, at native resolution.
2. Validate core-to-aperture sizing while the beam is well resolved. Check
   several pulse phases and positions along the beam, then other laser families,
   cinematic modes, ship scales and station emitters. Separate geometric support,
   half-maximum width and postprocessed bright width.
3. At distant zoom, use a smooth core-only control and retain the physical D.
   Translate the camera by fractions of a pixel and change the shallow beam angle.
   Pixel-phase gaps should track row/column crossings; for this crop their
   predicted horizontal period is about 53 pixels. Changing noise phase/tiling
   independently helps distinguish any remaining texture pattern.
4. Compare before and after postprocessing and AA resolve to find the first
   stage where the continuous contribution is lost. Test shallow/steep diagonals,
   horizontal/vertical beams and continuous zoom through the subpixel transition.
5. Verify footprint-filtered radiance remains stable as the line moves between
   pixels, then restore native noise/glow and check their minification separately.
   The prior two-axis texture correction remains an independent issue; it does
   not filter the analytic core. Do not bundle all changes into one comparison.
6. Check refiring, pooling and cleanup, then measure graphics and battle costs.
   Any implemented experiment would subsequently use the normal build/deploy
   workflow; no such implementation or deployment is part of this planning turn.

The supplied screenshots now support a concrete core-sizing defect and a strong
pixel-phase sampling lead. They do not prove the exact GPU stage responsible.
No game launch, code change, build, deployment, commit or push occurred here.
