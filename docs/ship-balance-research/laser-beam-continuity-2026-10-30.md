# Continuous aperture-scaled laser rendering

**Rejected/superseded:** The user rejected this renderer replacement. Its
subpixel explanation was not established and was an incorrect basis for changing
vanilla appearance. [0.10.19 removes it](laser-width-only-2026-10-30.md), restores
the native renderer and applies width-only scaling with vanilla longitudinal
texture spacing. The following is a historical record, not current guidance.


2026-10-30. **0.10.18 deployed for manual visual testing.**
The user reports adequate performance in limited 0.10.17 testing and shows
regular bright dashes/gaps along lasers that should appear continuous.
[Original screenshot](battle-scale/manual-laser-discontinuity-2026-10-30.png).
The complete 15x arrival workload is not yet certified by that feedback.

## Diagnosis and scope of evidence

The old aperture fix correctly narrowed the world-space ribbon, but retained
the native beam material and rasterization. A very thin shallow-diagonal ribbon
can cross pixel rows with insufficient coverage and appear broken. The screenshot's
periodic pattern is consistent with this subpixel effect. A screenshot alone
cannot isolate all GPU/shader contributions.

The [read-only asset inspection](battle-scale/beam-continuity-assets-2026-10-30.json)
confirms that the main native laser, arc and phaser effects use
`TerraInvicta/Beam Weapon Line` and noisy textures such as `noiseBurnUp`,
`ArcLaser` and `Phaser`. Their LineRenderer texture mode is Stretch; this is
not simply a dashed-line setting. Native `BeamEffectController.OnEnable`
also divides the initial texture scale by inherited effect scale, while the
old native per-camera width floor concealed the narrow-ribbon coverage issue.

The replacement addresses the textured/subpixel representation. It does not
claim to reproduce the old shader's failure in a headless renderer test.
No global camera AA/MSAA or graphics-quality setting is changed.

## Implemented representation

Bound combat lasers now use a solid additive billboard `Shapes.Line`, from
`ShapesRuntime.dll` already included in the game and used by its tactical UI.
[The library's documentation](https://acegikmo.com/shapes/docs#anti-aliasing)
describes local antialiasing and thin-line coverage fading: subpixel lines
use a pixel footprint with reduced opacity, including world-space thickness.
That expected shader behavior must still be checked in the installed game.

- The measured lens aperture remains the world-space Thickness. The old
  distance-proportional beam enlargement is not restored. Width/style/color
  configure at fire/reuse, with dashing disabled and no endpoint extension.
- The line uses an identity world transform, independent of the effect's
  inherited ship scale, and the same render layer as the original ribbon.
  Its color comes from the native per-effect material after native beam color
  and intensity setup. The textured interior is replaced by a uniform core.
- A postfix on native `SetBeamPoints` forwards its actual moving endpoints.
  This callback changes positions only when necessary; it does not recalculate
  aperture, reset width, construct a renderer or modify collision geometry.
  Camera-dependent subpixel handling is left to the existing shader.
- The original ribbon is hidden while the solid core is active. Native beam
  duration, targeting, damage, fire/hit particles, lights and sounds remain.
  Particle beams and non-laser effects retain their original renderer.
- Disable/reuse hides the standalone core and restores native line width/state
  without re-enabling a disabled effect. Destroying the owning component destroys
  its extra renderer. No shared native material is modified by this replacement.

This is a visual representation change, not a new laser-intensity, diffraction
or damage simulation. Thin far-away lasers may remain faint because coverage
fades rather than painting a fully bright, artificially enlarged stripe.
Native endpoint updating continues; there is no mesh/tree preparation in the
per-frame endpoint callback. The collision preparation/read-only query changes
from 0.10.17 are retained.

## Verification and deployment

Normal `tools/deploy.ps1` passed **41 validators**, **1,219 formula assertions**,
**384 aperture comparisons**, **1,753 contact checks**, and verified all
**52 installed files**. The implementation matrix maps 254 Harmony classes
across 119 feature rows. All 42 explicit battle hook classes resolve;
35 require native Unity-host binding/behavior confirmation.

Additional guards resolve the installed Shapes line properties, native endpoint
hook, one Bind-only aperture-thickness assignment and absence of renderer
construction or width reset in render/frame callbacks. These source/API checks
do not establish final GPU appearance. No headless Unity scene, window, image
or sound was created by validation.

[Deployment log](battle-scale/deployment-0.10.18.txt).
[Installed-file readback](battle-scale/deployment-manifest-0.10.18.json).
DLL SHA256: `2A3E0A6B0987EDC9E462907BA30F54DE3296F65F3A48A7F77832F3367AD628EB`.
Both game-closed guards passed. All 52 installed files matched at readback.
The local archive retains the same twelve older omissions; its forty present
package files match. No new settings or native templates were introduced.

## Performance feedback and remaining acceptance

The user reports performance was adequate in limited 0.10.17 testing. The
[fresh limited-run excerpt](battle-scale/contact-performance-0.10.17-limited.txt)
contains contact intervals around 1-2 ms/frame and a recorded 15x interval,
but that 15x interval has zero contact candidates. It therefore supports the
limited feedback without proving the full 15x clustered-arrival target.

Restart and check shallow-diagonal beams like the screenshot, horizontal/vertical
and steep beams, close lens views and wide views. Test laser/arc/phaser colors,
moving targets, multiple cameras/cinematic modes and multiple ship scales.
Verify native impact endpoints, brightness near subpixel transitions and that
pooled/disabled/destroyed beams do not leave duplicate or orphan strokes.
Retest performance at 15x with a few dozen arrivals against one target.

Beam continuity, style/brightness and pooling remain manual acceptance items.
The agent did not launch or manually play the game for this change.
