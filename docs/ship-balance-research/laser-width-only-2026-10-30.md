# Vanilla laser appearance with width-only scaling

2026-10-30. **0.10.19 current visual result accepted by the user.** This undoes the rejected 0.10.18
renderer replacement. The prior subpixel diagnosis was not established and did
not justify replacing vanilla rendering. The user clarified that shrinking the
texture feature along the beam created gaps between its fixed-period repeats.

## Restored native behavior

The native LineRenderer, shader, texture, material color, width curve, endpoint
updates, lifetime, particles, lights and sounds are retained. The extra Shapes
renderer, its dependency and the SetBeamPoints endpoint bridge are removed.
No replacement geometry is created, and no renderer visibility override remains.

Only the native ribbon's widthMultiplier is set from the measured lens aperture.
The old per-camera distance-based width floor stays bypassed for bound combat
lasers. The native width curve/profile is no longer replaced with a unit curve.
The mod does not change LineRenderer positions, texture mode, texture asset,
UV offsets, shader, color or effect transform scale.

## Preserving the vanilla longitudinal repeat period

Native BeamEffectController.OnEnable sets both main-texture tiling axes to
initialTextureScale divided by the inherited effect scale. Consequently a
uniform ship-scale change also changes the longitudinal texture frequency,
even though the actual beam endpoints remain native world positions.

Let r be the configured model scale relative to the vanilla model factor for
the current mode. Native U becomes initialU/(vanillaParentScale*r). The correction
multiplies **U only** by r, yielding initialU/vanillaParentScale. This restores
the vanilla longitudinal feature/repeat length independently of ribbon width.
Native V and offsets remain as configured by the game.

The material is the per-effect instance that native Awake already creates;
shared prefab assets are not modified. UV is captured once per activation and
restored on disable. Re-firing an already-active effect reuses that captured
native value, so repeated binding cannot compound the correction. The width
multiplier restores on disable as well. No per-frame width/UV rewrite is added.

## Verification and deployment

Normal tools/deploy.ps1 passed **41 validators**, **1,219 formula assertions**,
**384 aperture comparisons**, **1,753 contact checks**, and verified all
**52 installed files**. The implementation matrix returns to 253 Harmony classes
across 119 rows. All 41 explicit battle hooks resolve; 34 require Unity-host
binding/behavior confirmation.

Seventy-two focused cases check vanilla U-period invariance across native parent
scales and model ratios, unchanged V, noncumulative active refiring, restoration
and reuse with newly captured native values. IL/dependency guards reject the
replacement renderer/endpoint bridge and changes to native endpoints, transforms,
texture/shader/color, width curve or visibility. These checks do not prove GPU
appearance; user acceptance of the current result is recorded below.

[Deployment log](battle-scale/deployment-0.10.19.txt).
[Installed-file readback](battle-scale/deployment-manifest-0.10.19.json).
DLL SHA256: `02AAEC2417CD044969ED17349A501EE2F7D39923E90A67F70DCE6329BAE604D3`.
Both process-closed guards passed. The archive retains the same twelve older
omissions, with its forty present package files matching. The installed package
contains all 52 files.

The 0.10.17 collision/performance implementation and independent 10x/10x settings
are retained. The user reported adequate performance in limited testing; full
15x clustered-arrival acceptance remains unconfirmed.

## Manual acceptance

The user confirmed **"Ok, it's looking good"** on 2026-10-30 and authorized
commit/push. [Acceptance record](battle-scale/acceptance-0.10.19-2026-10-30.md).
This closes the current visual correction; the following broader checks remain
coverage targets rather than claimed completed tests.


Restart and check native beam appearance at shallow angles, close and wide zoom,
with laser/arc/phaser effects and multiple ship scales/cinematic modes. Check that
only transverse width follows the aperture, longitudinal texture features retain
their original size/repeat spacing, native animation/effects remain and repeated
firing does not progressively change UV spacing. No agent launched or manually
played the game for this change.
