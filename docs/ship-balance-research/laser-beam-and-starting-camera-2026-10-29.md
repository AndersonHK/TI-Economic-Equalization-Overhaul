# Laser width and initial battle-camera framing

**Settings follow-up:** [0.10.14 makes the shared combat scales configurable](configurable-combat-scale-2026-10-30.md),
with a 10x/10x trial. Fixed-scale camera figures below describe the 0.10.13 baseline.

2026-10-29. **0.10.13 is built and deployed for manual testing**, after the user confirmed the 0.10.12
skirmish no longer crashes and reported oversized/non-aperture-related beams
and an initial camera position too far from the ships.

## Plan and inspected causes

- Native `BeamEffectController.OnWillRenderObject` applies a minimum width
  of camera distance times 0.005, independent of hull scale or laser aperture.
  Replace combat laser ribbon width with the visible emitter's measured lens
  width, evaluated from the current mesh bounds and transform so mesh baking
  and cinematic scale are included. Preserve beam endpoints, damage and lens
  statistics. Do not mistake the separately stored simulation mirror radius
  for the actual current mesh aperture.
- Use the retained dimensional survey for all sixteen native laser resources,
  guarded by the game/weapon-bundle identity. Unknown/changed art needs a
  declared fallback rather than a guessed lens opening.
- Native initial camera framing uses a 400-times-scale orbit plus additional
  fleet/station offsets. Scale both the initial orbit and offsets with the
  model reduction. Preserve far zoom, established close limits and overshoot
  protection.
- Retain the verified 0.10.12 initializer fix, extend guarded validation, and
  deploy through the normal game-closed flow. No commit or push is requested.

## Implemented laser rendering

Fresh installed `BeamEffectController.OnWillRenderObject` inspection shows:

```csharp
float b = Vector3.Distance(cameraPosition, start) / parentScale * 0.005f;
lineRenderer.startWidth = parentScale * Mathf.Max(beamWidth, b);
```

The resulting distance floor is `cameraDistance*0.005` regardless of model
scale. The installed game DLL still matches
`4A4B9AAE4154E444E9727204205D2D42AE8ED9E1C5F92CDC1280074A259D8350`.
The optics catalog also pins the measured shipweapons bundle. Reproduce the
native inspection with repository-local ILSpy `-t BeamEffectController` and
`-t PavonisInteractive.TerraInvicta.SpaceCombat.SpaceCombatCameraController`
against that DLL; generated full decompilations remain temporary.

[LaserBeamGeometry](../../TIEconomyMod/Core/LaserBeamGeometry.cs) loads sixteen
source-guarded profiles from
[the private optics catalog](../../TIEconomyMod/ModFiles/Config/laser-beam-optics.eeo).
Each stores the measured clear-aperture diameter divided by moving-mesh X/Y
dimensions from the retained dimensional survey. Applying those ratios to the
current mesh bounds and world transform follows physical/cinematic scaling,
runtime mount scaling, mirroring and private mesh baking automatically.

The smaller of the two resulting aperture axes becomes the ribbon diameter.
The native per-camera distance floor is bypassed for bound combat lasers.
The LineRenderer uses a unit width curve and that absolute world diameter as
its multiplier, avoiding a second inherited width multiplier. Its original
curve/multiplier restore when disabled or returned for reuse.

The native templates' `mirrorRadius_cm` describes a simulation radius that is
often much larger than current art. This change follows **visible lens geometry**;
it does not implement the deferred laser-art resizing or an aperture/damage
rebalance. Triple-opening art uses the measured diameter of one opening and
retains the native single beam. If source hashes, resource or mesh identity do
not match, the explicitly logged fallback uses `2*mirrorRadius_cm/100` metres
times model scale. Unknown art is not claimed to have a measured pupil.

Endpoints, target selection, colors, damage, diffraction/jitter calculations,
impact VFX and non-laser particle beams retain native behavior. Ribbon width is
constant along its visual length in this implementation; it is not a new
simulation of the beam's focused intensity profile. At wide views a physically
narrow beam may become subpixel rather than expanding to an unrelated thick
stripe. Visual acceptance must include both close emitter views and wide views.

## Starting camera

[BattleInitialCameraScalePatch](../../TIEconomyMod/Patches/LaserBeamCameraPatches.cs)
scales the two native 400-times-scale orbit initializers and four focal-offset
constants. The physical-mode initial orbit becomes 2 km instead of 40 km;
fleet focal offsets become 300 m up / 600 m aft instead of 6 km / 12 km.
Both are changed: reducing the orbit alone would still leave the camera far
from the fleet. Station offsets receive the same factor. In cinematic mode,
the opening framing follows the 1/10 reduction from its original baseline.
Orientation and existing closest/farthest zoom, clipping and dolly-stop controls
are retained. Very large/dispersed fleets need the manual framing check below.

## Verification and deployment

Normal `tools/deploy.ps1` completed with **41 validators, 1,209 formula assertions
and 52 deployed files**. The new coverage includes 192 aperture comparisons
across both model scales, baked/unbaked coordinates and mirroring, all native
laser model resources, an invalid-ratio rejection, and six exact initial-camera
constant rewrites with a rejected unsupported-IL case.

The first attempt stopped before copying because `[single].05` in Windows
PowerShell denotes a missing member rather than the intended numeric cast.
Adding leading zeros corrected the fixtures; positive-input guards prevent a
degenerate zero-valued test from passing. Earlier physical formation/explosion
fixtures in this validator received the same correction and now pass with their
intended nonzero values. The complete deployment flow was rerun successfully.

The initializer-crash regression guard now checks all 34 explicit battle-hook
classes. Thirty require Unity-host emission confirmation; standalone checks
still do not prove visual rendering. The confirmed 0.10.12 startup fix remains.

DLL SHA-256:
`50AA4FD476D462C0663C4C6ED99C502A55B001BEB3AA194E7EDA737C083F6757`.
[Deployment log](battle-scale/deployment-0.10.13.txt) and
[installed-file readback](battle-scale/deployment-manifest-0.10.13.json).
Only documentation/readback followed deployment. No commit/push was requested.

## Manual acceptance

- Open fresh ship-only and station skirmishes in both scale modes. Check the
  opening view centers near the fleet/station, including a dispersed large fleet.
- Compare small, medium and large human lenses and alien laser optics at close
  range. Beam width should follow the visible opening and remain unchanged when
  the camera moves while the same beam is active.
- Reuse beam effects across repeated fire, different laser families, dorsal/
  ventral mounts and battle re-entry. Check the disable/restore path and any
  strategic bombardment view for stale ribbon state.
- Inspect `Laser aperture beam:` logs for `measured=True`; review any fallback
  record before treating it as measured lens alignment. Check wide-view
  readability separately. The current beam/camera changes await this visual test.

## Subsequent width-only correction

[0.10.19](laser-width-only-2026-10-30.md) removes the rejected 0.10.18 renderer
replacement. Native laser appearance, width profile and endpoints are retained;
ribbon width follows the aperture and longitudinal texture repeat stays vanilla.
