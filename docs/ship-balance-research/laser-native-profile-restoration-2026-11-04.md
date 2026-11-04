# Restore native close-up beam rendering after 0.11.3

**Subsequent feedback:** The user reports the subpixel issue solved and accepts
tested human arc PD and alien red PD. Alien main-beam appearance, including two
violet-equipped ships rendering white, remains open. [0.11.5 adds bounded
diagnostics without shader changes](alien-beam-color-investigation-2026-11-04.md).

2026-11-04. **0.11.4 deployed for testing.** The user rejected the 0.11.3 cone/taper and loss of the native
flowing appearance. [Cone screenshot](battle-scale/manual-laser-cone-0.11.3.png),
[close-view regression](battle-scale/manual-laser-flow-regression-0.11.3.png).
The initially forwarded designer screenshot was not beam evidence; the user
supplied the corrected cone attachment directly.

## Cause and corrective scope

0.11.3 went beyond the requested calibration: it replaced the native core with
a separately normalized profile and reconstructed/padded the vertices according
to camera depth. The wider solid core changed the balance against the original
halo. Equation checks on the halo alone did not establish preservation of the
complete image. The vertex path also introduced new distance-dependent carrier
width; its removal eliminates that added geometry behavior. These code differences
are established; their individual share of the observed taper was not GPU-captured.

For 0.11.4, retain the constant `ribbon width = graphical aperture / 0.46`.
Copy the installed native vertex program **byte-for-byte**, with its native
interface and parameter block. Remove endpoint uniforms, reconstructed vertices,
GPU padding and the extra native-endpoint callback. Native LineRenderer geometry,
bounds, transforms and texture coordinate generation remain authoritative.

Restore the complete native point-sampled fragment profile for resolved beams,
including its core, longitudinal radius, radial pulse and flowing noise halo.
The approved 46% value is a fixed **calibration estimate**, not permission to
replace the authored profile with a new flat-support core. Native profile
animation remains intact; the CPU ribbon multiplier does not fluctuate.

Only the analytic core contribution blends to its footprint average as its
native support becomes unresolved. At four or more pixels across the support,
use the original point contribution exactly; between four and two pixels, blend
smoothly; at two pixels or less, use the filter. Keep native noise sampling and
halo untouched. No texture replacement, noise simplification, new one-pixel width
floor or camera-distance geometry expansion is introduced.

This deliberately restores native raster coverage. At extreme distances where
the whole ribbon misses all raster samples, fragments can still be absent; a
fragment-only filter cannot fix absent geometry coverage. Do not claim arbitrary
distance continuity or add another geometry change to hide that limit.

## Verification and deployment

Normal deployment passed **44 validators and 1,808 formula assertions**. The
417 sampling assertions include phase/energy/quadrature coverage, plus exact
resolved-profile checks and native-core energy cases. All **57 source, installed
and ZIP files match**. Both process-closed guards passed before replacement.

The entire native vertex subprogram wrapper is byte-identical, as are its
metadata, property layout and render state. Native vertex DXBC SHA256:
`26EDD8DC65AF5CD30FA35FCFFEBE7F08E59A39FF3144E08DD66B0DCA3AA0D13F`.
There is no endpoint-uniform callback or new vertex geometry program.

An independent comparison of 1,800 complete resolved RGB equation cases, using
native laser/arc/phaser profiles, gives maximum absolute error 2.67e-15 in double
arithmetic. This is stronger than 0.11.3's halo-only check, but still does not
prove GPU appearance, flow animation or performance.

- [Native restoration audit](battle-scale/laser-native-restoration-validation-0.11.4.json).
- [Deployment log](battle-scale/deployment-0.11.4.txt).
- [Source/install/archive readback](battle-scale/deployment-manifest-0.11.4.json).
- [Original screenshot hashes](battle-scale/laser-regression-images-0.11.3.json).

DLL SHA256: `362F04E8DB2128D666C2C760FDDE706FAB7CD16DA194B8BF7CCD075F949334D0`.
The implementation matrix retains 123 features and now maps 264 Harmony classes,
with 41 explicit battle targets / 34 Unity-host binding cases.

Restart and reproduce both supplied camera views. Confirm no newly introduced
cone, the original flowing close-up structure, fixed calibration, smooth zoom
and acceptable shallow-diagonal sampling. Native perspective/profile effects
are retained; this change removes the added vertex behavior rather than claiming
to eliminate every possible native perspective width difference. Appearance,
animation and performance acceptance remain pending. The agent did not launch
or operate the game. No commit or push was requested.
