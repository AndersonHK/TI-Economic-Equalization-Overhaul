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

## November 22 muzzle calibration plan

The benchmark is `Cruiser-vs-T2-Station.gz` in the Terra Invicta saves folder.
It is a byte-identical copy of `CombatAutosave.gz` and opens the encounter
between Kasai and Jahn Station, a T2 orbital. SHA-256 is
`7E8AD3A7828DFF764B70A0A3DFE6BA5A50E3286CCB5F5D4DE48DAD8A4D178733`.
The reference frame is at 27:35 combat time. The red arc PD emitter shows an
approximately 8-pixel bright band across an approximately 22-pixel clear
opening. The combat log confirms the measured-aperture path and the current
0.46 divisor.

### Calibration and constraints

1. Measure the clear opening and the first unobscured bright-core cross-section
   perpendicular to the beam at comparable depth. Use the existing frame as a
   reference and sample several pulse phases with the same camera and exposure.
   Record display width at a consistent brightness threshold, the phase median,
   and minimum and maximum values. Aim for a median muzzle core within about
   15% of the projected opening while preserving the material's pulse.
2. Start with a red arc muzzle gain of about **2.75×** from `22 / 8`. That is
   locally equivalent to a divisor of **0.17** from `0.46 × 8 / 22`. It is a
   screen-derived trial value. Keep constants per native beam profile and check
   each important tint independently. Use the measured clear aperture in the
   existing optics catalog as the geometry input.
3. Apply the gain to the transverse core coordinates in the existing fragment
   shader. Keep the current LineRenderer width and feed the original coordinates
   to the halo. Use a smooth transition along the shaft to the existing
   downstream profile. Keep the same subpixel integral and pass it the pixel
   footprint of the calibrated core coordinate. Replacing 0.46 with 0.17 across
   the entire ribbon would also enlarge its halo and grow the red arc's neutral
   far-end positive core from roughly 0.72 to 1.97 lens diameters. That change
   risks recreating the rejected cone. Validate that the fitted core stays
   inside the existing carrier across the pulse range.
4. Keep native vertex code, tints, halo flow, endpoint positions, and particle
   sizing. Use the shared visual path for ship and station emitters. The common
   shader applies to the ordinary laser, arc laser, and phaser profiles. The
   separate alien X-ray and gamma effect shader requires its own measurements
   before it can share a calibration rule.

The current analytic muzzle widths below are fractions of the ribbon at neutral
pulse. They distinguish the complete core support from the half-peak bright
region. They guide the profile sweep. Final display gains come from matched
captures because the visible band also depends on HDR tint and exposure.

| Native profile | Positive support | Half-peak width |
| --- | ---: | ---: |
| Laser | 0.1210 | 0.0560 |
| ArcLaser | 0.1214 | 0.0697 |
| Phaser | 0.3176 | 0.1824 |

### Verification before acceptance

Run the saved benchmark for red arc PD and the green arc station batteries.
Check a close standard laser and a 360 cm phaser, then check alien red PD and
violet beams using the common shader. Compare muzzle widths, along-beam profiles,
and bloom against matched baseline frames. Include slow or paused views and a
continuous zoom through the subpixel transition. Check refire and cleanup in
another battle. In the shader tests, exercise the neutral gain against the
existing equations, sweep pulse and distance values for clipping, and compare
halo samples before and after calibration.

The initial trial used an estimate from the frame. The implementation below
uses asset measurements and retains the frame as a visual reference.

### Asset-backed implementation design

The screenshot supplies a visual check. Derive the calibration from the
installed beam prefabs, material parameters, decoded texture channels, and
native shader instructions. Match the width at the 1/e² linear emission
contour (about 13.5% of peak) to the lens. Matching half peak would extend the
emitting core far outside the opening. The common shader generates its core
analytically. Its red texture channel drives the moving halo. Normalize the
common core at the emitter to the measured opening and return it to the native
width downstream. Preserve the physical width and animation of the old halo
inside a carrier large enough to hold the corrected core.

Alien X-ray and gamma use the native No Middle program with zero central
laser opacity. Determine the width of the two animated lobes across their
outer optical crossings. Sample the installed Phaser texture's red-channel
distribution over the native pulse and noise-level cycle. Apply the resulting
calibration to the lobes and keep the native dark center, tint and material
blend state. The particle effects use authored emitter scale and follow their
existing non-optical visual contract.

Generate both optical variants from the same installed bundle and guard their
vertex programs and coverage by effect. Validate the normalization across
weapon-profile parameters and pulse phases. Replay Kasai vs. Jahn Station for
visual acceptance and include the phased and alien laser cases.

### Measurement and deployment

The asset sweep matched 20 referenced LineRenderer effects, including 14
optical effects and six particle effects. Every optical prefab has a unit width
curve and stretched line coordinates. The common shader's calculated
pre-patch contour at neutral pulse was 0.224 lens diameters for ordinary laser,
0.235 for arc, 0.615 for phaser, and 0.271 for solid red PD. The new common
shader gives one lens diameter at the muzzle across the audited pulse ranges
while retaining the original carrier and physical halo width.

The No Middle measurement uses the red histogram of the installed 256×256
Phaser texture and 120 phases for each of two transfer assumptions. It places
the reference contour at 0.06960 of the native ribbon with phase/transfer
estimates from 0.06562 to 0.07212. The solver sets the muzzle gain from this
measurement. The aperture spans 35% of the wider rendering ribbon, leaving
room for the computed lobe tail down to 1% of peak. The gain eases back to the
old downstream profile. The asset builder records texture hashes, alpha
ranges and sampled widths in the original calibration evidence. The
[retained asset survey](battle-scale/laser-aperture-calibration-2026-11-22.json)
records those measurements separately from the runtime manifest.

Deployed November 22, 2026 with 14,191 calibration checks and full release
verification. All 61 installed files matched. Both new shaders retain their
native vertex programs, material layouts and render states. The shared
particle fallback has the same compiled fragment as before. The installed
aperture bundle SHA-256 is
`39467F2B049578427BA129A4778963004E99C3263EC1476E963CB36EAD3FE4C1`.
The measured contour is linear emission. Apparent width after bloom and tone
mapping, including the near-to-far transition in the benchmark, remains an
in-game visual check.

### Jahn Station readback and proposed width trim

In the green arc salvo at 14:03, the player estimates 58 pixels of visible
beam against a 46-pixel lens. This is a display-space measurement in a
blooming salvo. Crosswise scans of a near-muzzle section give a median
53, 49, or 40 pixels above RGB thresholds of 180, 200, or 220 respectively.
The beam obscures the aperture boundary in this frame, so those readings
cannot determine the exact post-processing contribution.

The mounted 120 cm Green Arc Laser Battery reports `measured=True`, a
0.0008194089 world-unit aperture, and the calibrated common shader. Fresh
extraction of the stock medium battery gives a 1.6388177 m optical cap and a
1.9709892 m moving head. At the recorded 10× model setting, the former
reproduces the log, while the full head would be 0.0009854946 world units.
The [archived material mask](barrel-lens-audit/human-laser-1.png) shows the
same cap. Reprojecting the installed diffuse texture's alpha on that
submesh yields a median 152/255 in every radial band, including its edge.
The material uses alpha blending, with no separate alpha-masked inner pupil.

The current common shader places the 13.5%-of-peak emission contour at the
aperture. The green arc material's positive core therefore reaches
1.11–1.14 times the aperture across the native pulse. Its green channel
still emits about 1.71 in linear HDR at the calibrated edge before bloom and
tone mapping. Both explain why the visible result can be wider than the
geometric aperture, though this frame cannot isolate bloom quantitatively.
Matching the zero-emission boundary to the aperture would reduce arc width
by only about 10–12%, versus the approximately 21% indicated by 46/58.

For a quick adjustment, try a shared optical muzzle scale of **0.80**.
Multiply the target contour width by that scale for the common core and the
texture-backed No Middle lobes, then retain each path's existing blend
back to the downstream profile. Keep 1/e² as the reference level, and
preserve the measured aperture and carrier widths. The common shader keeps
its independent halo coordinates and particle emitters use their authored
width. At 0.80, the arc's modeled positive muzzle core fits within
0.89–0.91 of the aperture, with room for the visible fringe. If the displayed
width tracks the scale proportionally, the reported 58-pixel band lands
near 46 pixels. Treat that as an A/B starting point.

Extend the existing contour and clipping checks to the shared 0.80 target.
Compare an isolated green arc and Kasai's red PD in the saved benchmark,
along with a phaser, an ordinary laser and an alien X-ray or gamma effect.
Use matched exposure and several pulse phases, and compare the original
and candidate widths close to the lens and farther along the shaft.

### Folded calibration coefficients

The width trim uses 0.80 at the muzzle for both optical paths. The shader
contains final coefficients. The build only compiles and packages the
variants. The runtime manifest carries bundle identity, shader mappings
and carrier sizes.

For the common core, let `p` be the material's current pulse-dependent
exponent. The original 0.46 carrier ratio and 0.80 trim give a target
half-width of `0.46 × 0.80 / 2 = 0.184` in ribbon coordinates.
The radius parameter for the 1/e² contour is
`0.184^p / (1 − exp(−2))`. Folding that into base-two logarithms gives
a slope of **−2.442222328605074** and offset of **0.2097872745459192**.
The shader evaluates one exponential so the target width holds through
the material's pulse. The existing shaft progress interpolates the muzzle
radius to the native endpoint radius.

For No Middle, the measured contour spans **0.0695991496026513** of the
native ribbon. The final near-source gain is
`0.46 × 0.80 / 0.0695991496026513 = 5.287420925412881`.
The carrier remapping is `0.46 / 0.35 = 1.3142857142857145`. These two
compiled values preserve the wider carrier and ease the correction back
to the native shaft. The common carrier remains 0.46, so the common path
uses its transverse coordinates directly. Particle activation keeps the
authored width and shader path.

To regenerate the No Middle reference, extract the installed Phaser
texture's red histogram. Use the installed shader's muzzle coordinate
`u = 0.01` and compute the authored radius and ramp there. For each of
120 samples over one second, derive the animated threshold and noise
level from the material. At distance `d` beyond the lobe peak, average
`max(0, 1 − abs(red − level) × d² / (threshold² × ramp))` over that
histogram. Multiply by `(1 − 2d)^NoiseExponent`. Bisect the outer
crossing at 1/e² of peak, then double the sum of the authored radius
and crossing distance. Repeat for decoded and sRGB-linearized red.
Take the median of the two phase medians. The retained survey records
the input material parameters, texture fingerprints and resulting widths.

Deployed the folded coefficients and 0.80 trim on November 22, 2026.
The validator reads the actual shader coefficients, checks the
aperture-relative target against the retained profiles and sweeps pulse and
shaft position for clipping. Full release verification passed with
14,191 calibration checks and 61 verified install files. The installed
aperture bundle SHA-256 is
`F0033C043AAAFC84D2B085D7A037BE46C718715D15725E0469DDC21694FCB79F`.
The benchmark remains the visual check for apparent width after bloom.
The player accepted the resulting appearance for this checkpoint.
