# Fixed laser core sizing and filtered subpixel rendering

**Rejected and superseded:** The user reported widening cones and destruction
of the close-up flow appearance in 0.11.3. Its halo-only equation check did not
establish full visual preservation. [0.11.4 restores the native geometry and
resolved profile](laser-native-profile-restoration-2026-11-04.md), removes the
reconstructed/padded vertices and limits filtering to unresolved cores. The
following is the historical record of the rejected build, not current guidance.

2026-11-04. **0.11.3 deployed for manual testing**, following the user's approval
of [the core/sampling plan](laser-core-and-subpixel-plan-2026-11-04.md).

The approved constant is **core/ribbon = 0.46**, so the authored ribbon width is
`measured clear-aperture diameter / 0.46`, approximately **2.173913x** the aperture.
The core envelope remains that measured diameter; pulse and along-beam noise
must not resize it. The native material's family-specific radial falloff is
retained inside the fixed envelope, including the native animated exponent and
brightness character. Native color, noise animation, impact/fire
effects, lifetime and line length are retained; targeting/damage are unchanged
by this visual fix.

The subpixel correction uses a small native-style D3D11 shader variant. It
integrates the radial core over a derivative-sized pixel footprint, reducing
radiance as the physical core becomes subpixel rather than holding a bright
one-pixel floor. A GPU-only carrier margin supplies rasterization support; it
does not increase the physical core or extend the centerline endpoints.

Physical size/material inputs configure on creation or reuse. Native endpoint
updates mirror changed positions into two shader vectors; there is no recurring
CPU scale computation, collider/mesh work or resource creation. The shader asset
loads before battle and is source/hash guarded. Its isolated UnityFS container
reuses the installed 2020.3.49f1 serialization layout with compiled shader code;
it contains no ship prefabs or replacement textures.

The old U-only activation correction remains unchanged in this experiment so
the separate two-axis noise issue is not bundled into the core/sampling fix.
The rejected Shapes renderer is not restored. Native shader/material state and
width restore on disable; pooling must recapture native state without compounding.

## Scrolling flow preservation

The original texture instance remains on the native per-effect material. There
is no replacement texture, solid-beam renderer or simplified flow stand-in.
The variant retains the native view-plane sample coordinates, `_MainTex_ST`,
`0.15 * _NoiseTimescale * _Time.y` scroll, animated noise-contour threshold,
noise exponent/opacity, pulse speed/strength, color and additive render state.
The radial brightness profile still animates inside the fixed physical support.
Only the former pulse/longitudinal resizing of that support is normalized away.

An independent division-order comparison checks 1,800 combinations of native
laser/arc/phaser parameters, time, noise, cross position and beam position.
Maximum absolute halo-intensity difference is 1.67e-16 in double arithmetic.
This establishes algebraic preservation, not pixel-identical GPU output.
[Shader/container/interface audit](battle-scale/laser-shader-validation-0.11.3.json).

## Build, deployment and readback

Normal `tools/deploy.ps1` passed **44 validators** (33 patch and 11 data),
**1,659 formula assertions**, 384 aperture comparisons, 1,753 contact checks and
72 UV/refire/restore/reuse cases. The new formula coverage adds 268 assertions
for fixed ratio, phase/zoom/pulse energy and independent numerical quadrature.
The implementation matrix records 123 features / 265 Harmony classes; all 42
explicit battle targets resolve, with 35 needing Unity-host binding confirmation.

The compiled shader bundle contains only a Shader and its AssetBundle catalog.
Compiled constant slots and native texture binding were inspected, and both
program hashes match the manifest. Compilation/disassembly created no Unity
scene, graphics device, image or sound. The GPU filter uses a derivative-width
box approximation to the cross-core footprint; it is not exact two-dimensional
rotated-pixel integration.

Both game-closed deployment guards passed. All **57 source, installed and ZIP
package files match**. The current ZIP includes the full package tree.
DLL SHA256: `B3E90CF32DEAF08BA7D18E2708EA3B1039806D3BED8BF512C023C6C4EED5F878`.
Shader bundle SHA256: `B72F498130FBE255E5EE8030982A3CC0123B7AF1145AAF753CC9062542ACDE1A`.

- [Deployment log](battle-scale/deployment-0.11.3.txt).
- [Installed/archive readback](battle-scale/deployment-manifest-0.11.3.json).
- [Current game startup/binding excerpt](battle-scale/laser-runtime-load-0.11.3.txt).

The user's subsequent game session reports successful loading of the fixed 46%
shader and multiple live emitters with `filtered=True` and ribbon/core ratio
2.173913. This confirms Unity-host loading and component binding in that session.
It does not confirm the visual result or performance. The agent did not launch
or operate the game. No commit/push was requested.

The loader guards the current Windows Direct3D11/native-source hashes. If the
variant is unavailable it logs `Laser sampling shader unavailable; fixed ribbon
sizing only` and retains the original shader; that fallback does not claim the
sampling correction. This session's startup log confirms the full variant loaded.

## Remaining manual checks

Restart after deployment and compare the clear optical aperture with the core
at several camera angles and brightness-pulse phases. Check the 360 cm phaser
case, standard lasers, arcs, alien emitters and stations. Verify the original
scrolling flow remains recognizable at close range, with native color/glow,
motion, endpoints, depth occlusion and effects.

Zoom continuously through the one-pixel transition, including shallow diagonals
like the supplied crop. Check smooth brightness transfer between rows rather
than bright 53-pixel-separated chunks. Verify no bright minimum-width floor,
oversized glow or clipping at screen edges. Check moving targets, paused/refired
effects, pooling and cinematic modes. Graphics cost and the intended 15x battle
workload remain manual acceptance items.

To rebuild the shader after editing `Shaders/BeamRibbon.hlsl`, use
`tools/build-laser-shader.py` with the installed game directory and UnityPy 1.25.4
available to its Python interpreter. It invokes Windows' offline D3D compiler
and writes the private bundle/manifest; it never starts Unity. Normal validation
rejects stale HLSL/bundle hashes. Always run normal deployment after rebuilding.
