# Accepted 0.11.9: further performance inspection

Read-only inspection after committing and pushing the accepted stable runtime as
`d4d232a907a93d71dabd72c03008eaa3dcad9eb2`. No additional gameplay, camera,
instrumentation or deployment change was made. The user intentionally closed the
game after capture; this was not another reported crash.

## Evidence and limits

[Capture evidence](battle-scale/live-inspection-0.11.9-2026-11-05.json) retains
external samples, selected thread CPU deltas, raw earlier same-battle timing
lines, capture hashes and limitations. The collector read process/thread CPU
times and Windows GPU Engine utilization about every two seconds. It also took
one noninvasive, nonsuspending debugger snapshot. That snapshot did not resolve
the managed main-thread call chain and does not establish a function hotspot.

During a 30.45-second interval after the user unpaused, the main thread used
96.53% of one logical CPU, and process-wide CPU usage was equivalent to 1.48
logical CPUs. The game's sampled GPU 3D utilization averaged 11.92%, peaking at
27.28%. Before unpausing, the corresponding main-thread reading was 95.45%.
This supports investigating main-thread work first. GPU utilization is not GPU
frame duration; focus lookup failed, and camera/view state was not controlled.

The user supplied a Task Manager screenshot showing substantial unused CPU
capacity. Its charts represent logical processors, not individual game threads.
Parallel work can use that capacity only if it removes work or waits from the
frame's critical path; immediately waiting for a newly scheduled job does not
achieve that by itself.

The frame probe had exhausted its 120 five-second reports during the paused
wait before this capture. Therefore no new live-combat FPS, frame percentile or
per-stage timings are claimed. Earlier logs from this same battle contain eight
3x-speed intervals: compatibility snapshots cost 1.5020-4.4655 ms/frame; visual
bounds scans cost 0.6397-1.2867 ms/frame. Total measured main geometry cost was
2.4665-7.0294 ms/frame against whole-frame means of 48.221-156.589 ms. Snapshot
time is included in the collision/geometry total and must not be added twice.
These stages are worthwhile targets but do not explain most elapsed frame time.

## Proposed next work, not implemented

1. Obtain a fresh bounded capture with complete frame timing and attribution of
   main-thread update/render submission, native simulation, GC and job waits.
   Arm reporting on explicit capture or count active combat intervals so an
   idle paused wait cannot consume the entire observation window. Keep disabled
   capture overhead negligible. Identify the large unmeasured share before a
   broad threading rewrite.
2. Split compatibility snapshot timing into preparation, pose job wait and
   proxy-index wait. `CaptureCompatibilityPose` captures the shared active pose
   set; `IndexCompatibility` schedules and immediately completes its index job.
   The recorded battle had roughly 6,000 active poses. Investigate scheduling
   after authoritative movement becomes available and overlapping independent
   work until the query consumer, plus capturing only poses the proxy world
   needs. Avoid duplicate snapshots only when phase/epoch and mutation evidence
   proves reuse valid. Preserve fresh poses, same-frame damage changes, missile
   nearest-hit, cone all-hit and beam semantics; do not use stale poses to win a
   timing comparison.
3. Remove renderer-wide scans from the `BoundingBoxSize` getter's recurring
   path. `TryBounds` currently reads every visible renderer's bounds on each
   queried frame. Investigate prepared local geometry plus root transforms,
   with updates for animation and visibility changes. Verify bounds against the
   current path, especially rotating/deforming ships and destroyed parts;
   preserve navigation and future friendly-collision behavior.
4. Measure remaining per-projectile visual updates, native UI/waypoints and
   rendering submission before selecting further work. Existing mesh bake and
   mesh readback components disable themselves after initialization; their
   `LateUpdate` names alone are not evidence of recurring load. Independent
   numerical work is a candidate for workers; Unity object ownership and
   gameplay commits must remain correctly synchronized.

Acceptance for any future optimization requires whole-frame before/after data
at matched battle, speed and camera state, separate main-thread work and wait
costs, and collision/lifecycle regression checks. Existing headless query
benchmarks cannot prove a live FPS improvement.

## Separate confirmed camera defect

The user now confirms that merely hovering a waypoint temporarily cancels
camera follow. The desired future behavior is to preserve follow on hover and
suspend it only during actual waypoint click/drag interaction. Investigate the
native waypoint input-blocking condition separately, retaining gizmo picking,
drag suppression and the accepted repeat-battle cleanup. This confirmation does
not represent an implemented camera fix. Friendly-ship collision/avoidance work
also remains separately planned.
