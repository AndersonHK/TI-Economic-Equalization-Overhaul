# Constant particle-beam width without the camera-distance floor

2026-11-04. **0.11.6 user-accepted as stable.** The user confirms alien violet
lasers now look blue/correct, while broad white noisy beams remain. Alien particle
PD is a plausible source: its native effect is Particle Beam White. The selected
human ship also has Heavy Antimatter PD, so do not identify every white stroke
solely from the screenshot. Log particle weapon IDs to resolve that attribution.

## Confirmed omitted path

`BattleLaserEmitterPatch` and `LaserBeamGeometry.Apply` currently admit only
`TILaserWeaponTemplate`. `TIParticleWeaponTemplate` effects therefore retain
native `BeamEffectController.OnWillRenderObject`. Its combat formula is:

```text
endpointWidth = max(authoredBeamWidth * inheritedEffectScale,
                    cameraDistanceToEndpoint * 0.005)
```

The distance term ignores particle-weapon/model scaling. A line whose endpoints
are far from the camera can consequently become huge when its middle passes
near the camera. This is a concrete path omission and explains a mechanism for
the broad white beam. Exact shooter/type attribution still needs particle logs.

## Correction

Include particle weapons in the existing bind/restore/render callback handling.
For particles, capture the authored native width multiplier and inherited effect
scale at activation/reuse. Use their product as the constant world ribbon width:
the exact native base term with the camera-distance floor removed. Do not apply
the laser optical-aperture/0.46 estimate to a particle emitter or guess a lens.

Compatible Beam Weapon Line particle materials can use the existing native-vertex,
resolved-profile-preserving subpixel variant. Other particle shaders retain their
own shader. Preserve tint, texture, UV correction, flow, impact effects, damage
and movement; no shader source/asset changes are needed. Laser sizing remains
on its accepted aperture/0.46 branch. Setup remains on activation/reuse, with
no recurring width writes, mesh/collider work or resource construction.

Particles now receive the existing activation-time U-period compensation when
they enter this shared path. This corrects inherited ship-scale frequency using
the same formula as lasers; it does not replace the texture or its animation.

The current live audit confirms violet battery/cannon tints, textures and UVs are
unchanged across binding. The angstrom patch is unrelated. The working laser
path is a control; preserve the accepted violet/PD appearance during this fix.

## Validation and deployment

Normal deployment passed **44 validators and 1,887 formula assertions**, including
79 new particle authored-width/scale and native-floor comparison checks. Compiled
IL checks require both laser and particle types in binding and render guards.
All **57 source, installed and ZIP files match**. The HLSL, shader bundle and
shader manifest remain byte-identical to the accepted laser build.

The first deployment attempt stopped because the game was running. After the
user closed it, a new validation check incorrectly qualified the globally defined
template types; this check was corrected, and the complete normal flow was rerun
successfully. No verification was skipped and no failed attempt copied files.

- [Native path audit and screenshot](battle-scale/particle-beam-path-audit-2026-11-04.json).
- [Captured violet/PD material readback](battle-scale/laser-material-readback-0.11.5.txt).
- [Deployment log](battle-scale/deployment-0.11.6.txt).
- [Source/install/archive receipt](battle-scale/deployment-manifest-0.11.6.json).

DLL SHA256: `CB863D78AEBB41612A0497646CD19F1CD5FED7C98C6A927711E4AC3FAE388829`.
The matrix retains 123 features / 264 Harmony classes and 24 settings groups.

The regression checklist used for this iteration: reproduce the white-beam case with alien particle PD and human
antimatter PD. Inspect `particle=True` / `widthSource=authoredEffectScale` reports
to identify actual weapon IDs and captured widths. Compare close/wide zoom,
flow, native color, moving endpoints and reuse. Recheck the accepted violet and
laser PD controls. This remains a guide for future regression coverage.

The user subsequently confirmed **"The bug is fixed, the game is stable"** and
requested documentation, commit and push. [Stable acceptance record](battle-scale/acceptance-0.11.6-2026-11-04.md).
This closes the reported bug; it does not establish exact stroke attribution or
exhaustive battle/GPU benchmark coverage. The agent did not launch or operate the
game. Closure changes documentation only and preserves the tested runtime.
