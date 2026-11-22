# Waypoint hover and cruiser missile performance

## Camera implementation plan

In the installed 1.0.65 assembly, hovering a waypoint invokes
`HandleOnWaypointReadyForInput`. It sets the camera block and assigns the
waypoint input owner, which drives highlighting, gizmo selection and editing.
A whole-assembly field-reference scan found two writes (ready and end) and
one read (`SpaceCombatCameraController.Update`). EEO also clears the field
during stale-owner and battle cleanup.

Keep native input ownership. At the camera's existing early return, allow
follow mode to proceed when the primary button is up. Preserve the block
during a press or drag and the existing free-look behavior. Release restores
follow even with the cursor still over the same waypoint. Exercise the
installed camera control flow headlessly across modes and button transitions,
then run the existing Unity waypoint teardown fixtures.

The guarded deployment passed on November 22 against game 1.0.65. The
validator executed the transpiled native gate with injected button states
across both modes, waypoint ownership on/off, press, hold and release.
The two validator pools passed (35 and 11), along with 1,896 formula
assertions. Deployment verified 61 files. A separate Unity batch run exited
0 after 37 waypoint lifetime checks across three battles and 1,764 worker
contact checks. The managed DLL SHA-256 is
`9552CD1A42523EBCFC38BC563273607D7A8AA18C37DF6B3EFDB99B5D586E9F07`.
The visible tracking and gizmo check belongs to the player.

## Performance evidence from Kasai versus Jahn Station

The `Cruiser-vs-T2-Station.gz` save (SHA-256
`7e8ad3a7828dff764b70a0a3dfe6ba5a50e3286ccb5f5d4de48dad8a4d178733`)
contains uninitialized encounter 309721. Fleet India-103 contains Kasai
(297723), with two Lancehead missile bays holding 24 rounds each. Jahn Station
(43097) is tier 2 and has two Layered Defense Arrays (43107 and 43115).
The save begins with zero serialized projectiles. This gives a repeatable
pre-launch baseline, salvo and station point-defense workload.

The following are selected five-second reports from **two existing player runs**
of the save on game 1.0.65. Each line's speed is sampled at report time.
Counts cover the entire interval. The FPS probe reports actual frame wall time.

| Log and line | Speed | Mean frame ms | p95 ms | Timed main geometry ms/frame | Managed triangle tests | Compatibility rays |
|---|---:|---:|---:|---:|---:|---:|
| Player 801, before missile rays | 15 | 6.944 | 7.288 | 0.0009 | 0 | 0 |
| Player 815, missiles active | 1 | 11.020 | 19.767 | 0.1295 | 13,448,352 | 21,792 |
| Player 821, missiles active | 10 | 246.091 | 260.225 | 0.1262 | 52,994,496 | 1,008 |
| Player-prev 1125, missiles active | 10 | 261.641 | 266.752 | 0.1589 | 53,568,000 | 960 |

Source identities for the two session logs are `59F1BFC1…ABC56899` (Player)
and `9C010AA6…CA01CD2` (Player-prev). At Player 821 the serial path also
visited 87,778,440 BVH nodes. The ballistic query, body candidate and native
fallback counts were all zero. The compatibility snapshot took 0.1083 ms/frame.
GC generation 0 stayed at 212 from Player 810 through 824. The 5× interval
at Player 824 already averaged 260.494 ms/frame. Use speed, active missiles
and simulation ticks per frame as independent measurements. The user's
observed linear scaling over a range of counts remains a hypothesis.

**First suspect: station weapon line of sight through its own hull.** The
station acquisition patch calls `SelectFace`, which calls `CanAim`, then
calls `CanAim` again on the selected face. Each positive-elevation check can
run a managed, nearest-triangle traversal of the defense module shell to
answer a yes/no obstruction question. Visualization selection also calls
`SelectFace`. The final fire check selects a face and checks its aligned
muzzle. These traversals increment the shared node/triangle counters outside
the reported geometry timers. The managed exact projectile fallback also
uses those counters, but its body checks were zero in both slow rows.
Target acquisition runs from the native quarter-second catch-up loop. A slow
frame at high simulation speed can run multiple acquisition passes before
rendering again. The logs identify an expensive workload and a coverage gap.
Direct timing by caller is still needed to quantify its share.
[Station query callers](../../TIEconomyMod/Patches/OrbitalDefensePatches.cs)
and [managed tree traversal](../../TIEconomyMod/Core/RenderedMeshRaycast.cs).

## Order of performance work

1. **Reproduce and account for a whole slow frame.** Restore a copy of this
   encounter in a headless Unity combat-scene replay, with the rendered
   combat controllers active. Pin targeting, fire modes, formation, camera
   and simulation-time boundaries. Record pre-fire, first salvo, peak
   missiles and post-intercept at 1×, 5×, 10× and 15×. Capture at least three
   runs at a fixed speed, separate warmup, and treat speed changes as mixed
   intervals. Measure time per frame and per simulated second, ship/module,
   mount, live missile and acquisition candidate counts. Bracket station
   acquisition, face choice, obstruction and aligned fire checks outside the
   inner triangle loop. Attribute counters per caller. Split simulation
   ticks, weapon/PD AI, controller updates, movement scheduling/copies,
   worker waits and rendering. Use a preallocated trace buffer and arm on
   salvo so paused time cannot exhaust it. Compare with the player's rendered
   baseline. A null graphics device can establish CPU costs.
2. **Eliminate repeated station queries before scheduling workers.** Give
   acquisition, face selection and execution one mount-clearance service.
   Have face selection return its eligibility. Share only identical
   predicted queries within the same pose and target sample. Check the real
   aligned muzzle again at fire time. Build immutable local shell trees at
   loading, precompute rigid mount data and use an exact segment-any-hit
   traversal with immediate exit on the first obstruction. Check clear rays,
   blocked rays, both faces blocked, grazing and endpoint hits, swapped
   faces, rotated modules, different poses and moving/intercept targets
   against the current implementation. Preserve elevation, tie-breaking and
   muzzle rules. Time clear and blocked rays separately.
3. **Batch independent clearance and targeting calculations.** At the next
   acquisition tick, gather active poses and authoritative target/lead data
   once. Schedule independent mount/candidate pairs over shared read-only
   shell data, with contiguous results in native memory. Overlap this batch
   with unrelated ship/module work until targeting actually consumes the
   result. Commit choices, rotations, ammo and damage on the main thread in
   native order. Revalidate target generation, weapon readiness and muzzle
   pose after earlier shots in the tick. Recompute invalidated decisions
   under the same service. Complete or retire pending work at teardown.
   Never speculate across dependent simulation ticks. Compare candidates
   examined, worker time, scheduling and **main-thread wait** as well as
   whole-frame time. Choose batch sizes and the small-workload serial path
   using that measurement.
4. **Isolate missile and ship scaling.** In disposable replays independently
   vary active missiles (e.g. 0, 12, 24, 48), attacking ships and defending
   modules. Count calls per tick as well as per frame. In 1.0.65 the movement
   job already runs on workers, but its container copies managed lists into
   temporary native arrays and a transform array each update, waits and
   writes back every projectile in LateUpdate. Consider persistent slots
   updated at spawn/removal and a compact result commit if those costs are
   measurable. Track all consumers, ordering and pooled reuse. In mixed
   containers the controller loop still visits every missile. The native
   tracking display updates before the pause check and EEO recomputes distance
   and writes scale again. Centralize that calculation, sample the camera
   once per frame and apply changed visual properties on the main thread.
   For ship AI, measure `GetAllThreateningProjectiles` and
   `GetAllMissilesTargetingMe`, which each scan the projectile collection
   when invoked. Batch numerical threat tests or maintain target/faction
   lists at lifecycle boundaries if candidate counts justify it. These paths
   can scale with both ship and missile count.
5. **Revisit larger fleets using their own benchmark.** The older large-fleet
   measurements put synchronous pose/index snapshots at 1.5–4.5 ms/frame and
   visual bounds at 0.6–1.3 ms/frame. Test a proxy-only pose set and earlier
   scheduling after authoritative movement with the first dependent query
   as consumer. Prepare rigid bounds at loading and update animated or
   destroyed parts from the same pose/visibility generation. Validate
   nearest/all-hit, sweep endpoints, cone/beam behavior and bounds before
   changing any routing. [Earlier fleet inspection](live-performance-inspection-2026-11-05.md).

Compare each candidate with the same save and ship-count control using
median and p95 whole-frame time, main CPU time, waits, allocations, and
achieved simulation seconds per wall second. Report the measured gain for
each change independently. Require parity on target, chosen face, launch
order, hit/occlusion, damage and final combat state. Exercise repeat battles,
destruction and retargeting. Reject a timing improvement obtained by skipping
simulation steps, deferring hits a frame or altering geometry.
