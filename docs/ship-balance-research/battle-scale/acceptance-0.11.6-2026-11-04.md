# Stable 0.11.6 acceptance and repository checkpoint

2026-11-04. The user confirmed: **"The bug is fixed, the game is stable,
update the docs. Then commit and push."**

This accepts the deployed 0.11.6 baseline and closes the reported beam-rendering
bug. Earlier feedback accepted the laser subpixel correction, restored native
flow, alien violet/blue beams and laser PD. The final correction included particle
weapons in constant-width handling, removing their remaining camera-distance
widening. Particle width is the native authored width times captured effect
scale; laser width retains the fixed aperture/0.46 calibration.

The accepted package also contains the pending capital fitting/tier/save-layout,
designer viewport, wavelength precision, alien reactor allowance and alien armor
changes developed since v0.10.21. This checkpoint preserves that complete current
baseline. It does not turn every dated feature-specific checklist into a claim
of exhaustive testing. Performance and armor rework proposals remain plans;
the rejected armor calibration is retained in the archive, not implemented.

## Runtime identity and verification

- Release: **0.11.6**, targeting **Terra Invicta 1.0.53b**.
- DLL SHA256: `CB863D78AEBB41612A0497646CD19F1CD5FED7C98C6A927711E4AC3FAE388829`.
- Shader bundle SHA256: `09EAA8A2F649838295D456D6DE00507F51B9827C88AE3C5467C07312F0671D4B`.
- Normal deployment passed **44 validators / 1,887 formula assertions**, including
  79 particle-width checks; the broader checks retain 384 aperture comparisons,
  1,753 contact checks and 72 UV/refire/restore/reuse cases.
- All **57 source, installed and ZIP files match** the retained deployment
  manifest, rechecked before this commit. The ZIP contains the full ModFiles tree.
- Matrix: **123 features / 24 settings groups / 264 Harmony classes**.

[Deployment log](deployment-0.11.6.txt) and
[source/install/archive receipt](deployment-manifest-0.11.6.json).
The receipt's original pending-acceptance field is a historical deployment
snapshot; this dated user acceptance supersedes it.

Closure updates documentation and the implementation matrix only. It does not
rebuild, replace or redeploy the runtime the user tested. The agent did not
launch or operate the game. User acceptance is recorded separately from the
automated checks, native shader byte identity and CPU equation comparisons.
Git attributes also preserve the byte-hashed HLSL input without line-ending
normalization, so checkout does not invalidate the shipped shader manifest.

The rejected 0.11.3 core/vertex replacement remains documented as superseded.
Current rendering retains native vertex bytecode and the resolved profile;
subpixel filtering is limited to the unresolved core contribution. Bounded
material diagnostics remain included. Exact per-stroke attribution and exhaustive
asset/angle/pooling/GPU/15x performance coverage are not established by this
general stability confirmation.
