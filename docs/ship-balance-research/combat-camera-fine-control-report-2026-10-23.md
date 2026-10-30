# Combat camera: close-range zoom and movement precision

**Follow-up:** [0.10.9 reduces the two camera speed minima twentyfold](battle-scale-implementation-2026-10-23.md).
The broader redesign below remains deferred; baseline numbers are pre-change.

2026-10-23. **Research only.** No code, configuration, build or deployment
changes. Findings come from fresh decompilation of the installed
`SpaceCombatCameraController`, installed global configuration, and searches of
the current EEO source. The mod supplies no camera override or camera
configuration fields. [Shared baseline and code evidence](battle-scale-camera-code-evidence-2026-10-23.md#baseline).

## Findings

The requested finer control fits the observed code problem. The native camera
already reduces speed toward its internal minimum zoom radius, but:

- Its minimum speed is still substantial in battle-distance units.
- Held movement keys translate the camera **once per rendered frame without
  multiplying by delta time**, so movement per second depends on FPS.
- Free-look wheel motion multiplies the scroll coefficient by the **maximum**
  movement speed, rather than the current movement speed.
- Speed follows an internal polar radius which can diverge from actual
  free-look viewing distance. Reaching the minimum radius does not stop further
  free-look forward motion.

Thus lowering one global speed value can provide relief, but a dependable fix
should distinguish free-look/follow behavior and compute close-range speed from
the actual view. [Movement](battle-scale-camera-code-evidence-2026-10-23.md#c2-keyboard-and-mouse-movement),
[zoom](battle-scale-camera-code-evidence-2026-10-23.md#c3-zoom-and-follow-behavior).

There is already a partial fine-control gesture: **hold Ctrl while scrolling
in free-look** for a one-quarter displacement target. This modifier is not
applied to follow-mode radius zoom or ordinary keyboard strafing. Its actual
feel was not tested in game.

## Input and mode routing

The controller reads `TIInputManager.cameraLeft/Right/Up/Down` rather than
hardcoded arrows. Native defaults are A/D/W/S, with T/B for zoom; profile
bindings can remap them. The user's arrow-key controls should be handled through
these bindings. Zoom uses `Input.GetAxis("Mouse ScrollWheel")`; held zoom keys
replace that axis with ±`combatCamera_keyZoomSpeed` each update.
[Input excerpts](battle-scale-camera-code-evidence-2026-10-23.md#c2-keyboard-and-mouse-movement).

| Mode/input | Native behavior |
|---|---|
| Free-look movement keys | Translate on camera-relative ground-plane axes |
| Free-look wheel/zoom keys | Translate toward/away along view forward and update an internal radius |
| Follow wheel/zoom keys | Change the polar orbit radius around the followed target |
| Follow movement keys | Switch to free-look; movement runs on a subsequent update |
| Middle-button drag | Separate movement/offset branches sharing the movement-speed field |
| Right-button drag | Rotation path, with additional movement branch in free-look |

`TIInputManager.acceptingInput`, `blockCombatZoom`, screen bounds, active
magnifier, UI drag handling and modifier keys supply existing input guards.
Any future patch needs to preserve them rather than replacing the entire
controller update unconditionally.

## Current configuration and formulas

The installed global config supplies the following. EEO's partial global
override contains none of these fields.
[Full configuration excerpt](battle-scale-camera-code-evidence-2026-10-23.md#c1-camera-configuration-and-initialization).

| Field | Value |
|---|---:|
| Minimum / maximum zoom | 20 / 5000 |
| Minimum / maximum movement speed | 5 / 90 |
| Minimum / maximum scroll coefficient | -200 / -600 |
| Key zoom axis | 0.01 |
| Minimum / maximum pan | -8000 / 8000 |
| Free-look zoom multiplier, in code | 0.25 |
| Polar-radius interpolation duration | 0.15 seconds |
| Free-look translation timer | 0.1 seconds |

Let `A=0.1` with cinematic scaling off or `A=1` with it on, `S=0.05A`, `r` be
the internal polar radius, `q` the scroll axis, and:

```text
t = clamp01((r/S - 20) / 4980)
movementSpeed = S × (5 + 85t)
scrollCoefficient = S × (-200 - 400t)
```

`HandleMovement` adds `1.5 × movementSpeed × inputAxis` to position every
update. `HandleZoom` changes the follow radius target by
`q × scrollCoefficient`; free-look separately sets a translation target with
magnitude `abs(q × scrollCoefficient) × 90 × 0.25`. Ctrl multiplies the latter
by another 0.25. [Code](battle-scale-camera-code-evidence-2026-10-23.md#c3-zoom-and-follow-behavior).

The interpolation smooths these changes; it does not make the target steps
small. For free-look, position is repeatedly lerped toward the target at
`Time.deltaTime × 10` during its short timer, so completed travel is smaller
than the target displacement and also depends on frame timing. The radius
interpolation instead uses `Time.unscaledDeltaTime`.

### Size of the minimum steps

These are **calculated examples**, not measured runtime movement. They assume
`t=0`, a single wheel event `q=0.1`, and no UI blocking or clamps. Real wheel
axis values depend on device/input configuration. Scene distances convert at
`1 u = 20 km`.

| Quantity at internal minimum zoom | Cinematic off | Cinematic on |
|---|---:|---:|
| Minimum follow radius | 2 km | 20 km |
| One-axis movement per rendered frame | 0.75 km | 7.5 km |
| One-axis movement per second at 30 FPS | 22.5 km/s | 225 km/s |
| One-axis movement per second at 60 FPS | 45 km/s | 450 km/s |
| One-axis movement per second at 144 FPS | 108 km/s | 1,080 km/s |
| Wheel free-look displacement target | 45 km | 450 km |
| Ctrl + wheel free-look displacement target | 11.25 km | 112.5 km |
| Follow-radius target change, before clamp | 2 km | 20 km |

At the initial internal radius `400S`, movement is faster still: `t≈0.0763`,
giving about 103.4 km/s at 60 FPS with cinematic scaling off. At the far end,
the scroll coefficient is only 3× its minimum and movement speed 18×, while
the permitted polar-radius range spans 250×. The near floor is therefore high
relative to the range of scene scales being inspected.
[Arithmetic](battle-scale-camera-code-evidence-2026-10-23.md#reproduction-and-arithmetic-checks).

This connects directly to the size investigation: the non-cinematic minimum
pan is 750 m per frame, versus a nominal 100 m ship enlarged to 2 km. At a
physically consistent scale the same ship would occupy just 100 m. Camera
precision should be addressed before shrinking ship geometry further.
[Scale report](rendered-battle-scale-and-collision-report-2026-10-23.md#the-actual-unit-conversion).

## Why internal zoom is a poor free-look distance measure

`HandleZoom` updates `_targetPolarOffset` within `ScaledMinZoom/ScaledMaxZoom`
and interpolates `_polarOffset.radius`. Free-look translation uses the scaled
input independently. It continues to create `_targetCameraPos` even when the
radius target has hit a clamp. `HandleMovement` recomputes a focal-point value
from a grid-plane intersection but does not synchronize the polar radius to
the actual camera-to-subject distance. Speed fields are refreshed during the
zoom timer, rather than recomputed from the actual scene distance every frame.
[Native methods](battle-scale-camera-code-evidence-2026-10-23.md#c2-keyboard-and-mouse-movement).

This means a patch which merely adds another multiplier based on
`_polarOffset.radius` can still be insensitive to how close the camera really
is to a ship. It can help, but it does not resolve that mismatch.

Follow mode is more directly tied to radius. `LookAtCombatant` and
`LookAtObject` choose:

```text
targetRadius = S × [20 + 4980 × modelScalingFactor]
```

That is approximately **2.498 km** with cinematic off and **69.8 km** with it
on. Consequently the same illustrated wheel input can consume a large fraction
of the remaining near-range radius in one event. Its clamp prevents a negative
radius but does not provide gradual inspection control.
[Focus and zoom code](battle-scale-camera-code-evidence-2026-10-23.md#c3-zoom-and-follow-behavior).

An additional initialization issue is visible statically:
`_scrollSpeedOffset` defaults to zero and is not assigned in
`InitializeCamera`. The first zoom input starts the zoom timer, whose update
then fills the coefficient. This predicts a first ineffective event for a
fresh controller. It needs runtime confirmation and is secondary to the
persistent speed problems.

## Proposed changes

### Small tuning option

Lower `combatCamera_minCameraMovementSpeed` and the magnitude of
`combatCamera_minScrollSpeedOffset`, preserving the far values. This is a
simple partial config override, but the two modes share the scroll coefficient,
free-look still multiplies by 90, and movement still depends on FPS. It is a
possible interim experiment, not the preferred complete fix. Do not select final
coefficients from these illustrative numbers alone.

### Recommended implementation

Use a combat-camera-specific patch with separate keyboard, wheel and mouse-drag
handling, preserving native input guards, focus/cycle behavior and callbacks.

1. **Measure actual view scale.** In follow mode use the true orbit/target
   distance. In free-look use a valid focus point or view-ray intersection
   with relevant scene geometry; retain a stable last-valid distance/fallback
   when looking into empty space or nearly parallel to the grid. A grid-only
   ray can give a huge distance at shallow angles and is insufficient alone.
2. **Scale movement per second.** A candidate relation is
   `panSpeed_u_per_second = kPan × viewDistance_u`, blended into the desired
   wide-view speed and clamped to useful limits. Multiply held-key motion by
   `Time.unscaledDeltaTime`. Handle diagonal normalization deliberately.
   Mouse-axis deltas need their own calibration; do not blindly multiply all
   native mouse paths by delta time as though they were held keys.
3. **Use proportional zoom.** For follow mode a bounded candidate is
   `newRadius = clamp(radius × exp(-kZoom × q), minimum, maximum)`. For
   free-look, derive a translation step from actual view distance and cap it
   before crossing the focus subject. Keep state synchronized after movement,
   follow/free transitions and ship cycling.
4. **Separate wheel events from held zoom keys.** Wheel-axis deltas are input
   events; key zoom needs a per-second rate. Do not introduce FPS dependence
   into one while fixing the other.
5. **Preserve close precision and wide travel.** Blend smoothly rather than
   switching abruptly at a zoom threshold. Retain Ctrl as an optional precision
   modifier and consider applying it consistently to keyboard movement and
   both zoom modes. Initial coefficients and any modifier expansion require
   a manual feel test; they are not approved tuning values in this report.

`HandleMovement`, `HandleZoom`, `InitializeCamera`, and focus/state transitions
are the relevant seams. A prefix which adjusts only `_cameraMovementSpeed`
cannot fix the free-look use of `maxCameraMovementSpeed`, the internal-radius
divergence, and wheel/key differences. Replacing all of `Update` would broaden
the patch unnecessarily. Field-access or narrow guarded IL changes must be
verified against the identified native DLL.

The native manager sets the main camera far clip plane but the inspected
combat-camera class does not establish an appropriate near clip for a proposed
true-scale ship. Read back the actual cameras' near planes/FOV before selecting
new minimum inspection distances. No near-clip value is assumed here.

## Validation required before acceptance

Implement only after authorization, using the repository's normal build,
automatic verification and deployment flow. Then compare recorded inputs:

- At near, intermediate and wide views, hold each movement binding for one
  second at 30/60/144 FPS. Confirm comparable world travel and comfortable
  screen travel, including diagonal input.
- Record the actual wheel-axis values and displacement for individual
  detents, repeated events and direction reversals; test a high-resolution
  wheel/trackpad if available. Distinguish target and completed movement.
- Exercise free-look, follow, movement out of follow, focus on a ship/object,
  ship cycling, destroyed targets and empty-space viewing.
- Test Ctrl, held zoom keys, mouse drags, UI hover/magnifier, input blocking,
  pause and simulation speeds 1×–15×. Camera feel should follow real time.
- Verify both cinematic flags and both campaign/skirmish entry paths. Inspect
  clipping, selection and callbacks at current geometry scale, then repeat
  after any future ship-scale change.

No manual camera behavior was tested in this report. The equations establish
why large steps and FPS dependence are plausible; they do not choose the final
comfortable speed for the user's setup.
