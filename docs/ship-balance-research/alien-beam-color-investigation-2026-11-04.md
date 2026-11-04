# Alien beam color and native effect investigation

**Closed in the stable baseline:** The user confirms the final bug fixed and
game stable in [0.11.6](battle-scale/acceptance-0.11.6-2026-11-04.md).
The diagnostic uncertainty below describes the earlier investigation, not an
outstanding request to retint the now-accepted laser effects.

**Subsequent result:** The user now confirms alien violet beams look blue and
correct. Live 0.11.5 audits show unchanged tint, texture and UV across binding.
The remaining broad white strokes may be particle PD; [0.11.6 corrects the
omitted particle sizing path](particle-beam-scale-correction-2026-11-04.md).
The diagnostic history below is retained without claiming it changed colors.

2026-11-04. The user accepts the subpixel correction and the appearance of human
arc PD and alien red PD. They report white, unpleasant alien main beams,
including two ships fitted with violet lasers. Preserve the accepted rendering
paths and investigate the remaining case separately.

## Confirmed source trace

The angstrom adapter only changes the physical wavelength conversion, related
spot/range caches and displayed precision for the two zero-rounded graser IDs.
It does not select a beam resource, tint, intensity or material. Violet 450 nm
and X-ray 10 nm records do not use its precision override.

| Weapon family | Stock effect | Shader | Native tint setup |
|---|---|---|---|
| Alien violet | Alien Laser Beam Blue | Beam Weapon Line | Blue HDR, approximately (1.95, 5.47, 30.93) after BeamEffectController setup |
| Alien X-ray | Alien Laser XRay | Laser Beam No Middle | White HDR, approximately (17.59, 17.59, 17.59) |
| Alien gamma | Alien Laser Gamma Ray | Laser Beam No Middle | Green HDR, approximately (0.90, 143.69, 36.86) |
| Alien laser PD | Solid Laser Red | Beam Weapon Line | Authored red |

The current game log identifies `Alien512cmVioletLaserCannon` with `filtered=True`,
`Alien768cmXaserCannon` with `filtered=False`, and alien laser PD with
`filtered=True`. Thus the violet report cannot be dismissed as an X-ray-only
effect choice. The X-ray branch currently uses the original special shader;
the successful common-shader correction is not applied to that shader.

The violet HDR triplet can saturate all three display channels in its bright
center. That is a plausible source of whiteness, not proof of what happened in
the supplied frame. Static prefab values do not establish the actual live tint
before and after changing the common shader. The special No Middle effect has
its own layout, zero LaserOpacity, high noise exponent and a different halo
formula; it must not be passed through the common beam shader as a shortcut.

## Next controlled build: bounded material diagnostics

**0.11.5 is deployed for diagnostic testing.** It adds one material comparison per weapon/cinematic key, piggybacking on the
existing bounded beam report. Capture actual effect, original/bound shader,
RGBA, texture name and texture scale across binding. Do not recolor, normalize
HDR brightness, change wavelength data, replace effects or edit either shader.
The working human/alien PD paths, constant sizing and subpixel behavior remain
unchanged. This is a diagnostic build, not a claimed white-beam fix.

Use the resulting live values to distinguish a material-state regression from
native HDR clipping and the separate native X-ray effect. Any subsequent visual
change must target the measured cause, preserving the accepted PD rendering and
native animated flow. The user's screenshots and the static audit are retained
with the deployment evidence below.

## Validation and handoff

Normal deployment passed **44 validators and 1,808 formula assertions**. All
**57 source, installed and ZIP files match**. The HLSL, compiled shader asset and
shader manifest are **byte-identical to 0.11.4**; the diagnostic does not write
colors, textures, UVs or render state. It only reads and logs state once per
existing weapon/cinematic report key. There is no per-frame material audit.

- [Static native asset/color audit and supplied screenshots](battle-scale/alien-beam-color-audit-2026-11-04.json).
- [Deployment log](battle-scale/deployment-0.11.5.txt).
- [Source/install/archive receipt](battle-scale/deployment-manifest-0.11.5.json).

DLL SHA256: `030177B294CB0B81767185684475EC4ADC048DBD5F58D10F04CFFD8B4C2451A4`.
Restart and fire a violet alien weapon and an X-ray weapon. The bounded
`Laser material audit` lines capture `tintBefore`, `tintAfter`, `tintChanged`,
texture/UV equality and actual shader/effect names. A changed tint would identify
a binding-state problem; an unchanged blue HDR tint would rule out that reset
and motivate checking exposure/postprocessing against a native-shader control.
Neither outcome alone certifies the final GPU color.

Alien main-beam appearance remains unresolved. Native X-ray white is a confirmed
asset choice; the user's violet-weapon report remains an independent issue.
No colors or effects have been retuned, and no commit/push was requested.
