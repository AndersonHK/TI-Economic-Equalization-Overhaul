# Live projectile regression and compatibility-query replacement

Follow-up: the user confirms excellent performance and working missiles in 0.11.8.
A separate second-battle waypoint UI crash is addressed by
[0.11.9 lifecycle cleanup](repeat-battle-waypoint-cleanup-2026-11-05.md).

2026-11-05. **0.11.8 deployed for controlled retesting.** 0.11.7 is a failed
performance candidate, not an accepted baseline.

The user reports roughly 0.5 FPS after firing. The captured live intervals
first show 16–20 ms/frame in compatibility queries, then **630–652 ms/frame**
while unpaused at about 1.4–1.5 FPS. The initial averages understated the failure.
The ballistic worker pass is approximately 0.1–1.3 ms in those intervals.
[Captured evidence](battle-scale/live-contact-regression-2026-11-05.json).
These are aggregates, not proof of maximum whole-frame latency. Windows CPU
ETW recording was unavailable due profiling policy; a noninvasive nonsuspending
stack snapshot showed the main thread executing JIT code, without managed names.

`PreparedContactWorld.RaycastAll` performs a physics query and then walks every
body, every part/bone proxy and every round for **each missile/cone ray**. Each
walk performs main-thread Unity transform reads. The headless acceptance harness
tested ballistic jobs and omitted this production compatibility workload.

Replace that bridge with a separately indexed fitted-proxy world sharing prepared
pose slots. Capture current poses once per native missile query phase, with a
native movement dependency; preserve missile sweep arguments and inside-origin
rejection. Cone calls get a fresh snapshot at their entry boundary; beam-targeted
queries retain current targeted semantics. Lifecycle and explicit geometry changes
invalidate snapshots, and removals update live flags immediately. Do not use stale
poses, broaden damage geometry or reinstate thousands of live PhysX proxies.

Acceptance must exercise the actual production query wrapper at realistic missile
counts, including snapshot/wait/bridge costs, rather than only isolated kernels.
Add wall-clock frame distributions and separate compatibility stage/count metrics.
Full deployment and another live battle are required before calling this resolved.

## Implemented replacement

The missile hook now requests the nearest eligible fitted proxy from a native
spatial index. Friendly targets are rejected before narrow-phase work. A rejected
or destroyed candidate does not hide the next valid surface. Native missile
movement, sweep length, damage construction and hit handling remain in place.

Fitted static, bone and primitive bounds are prepared during loading. Current
transforms are captured once at the missile-query phase, with the native movement
job dependency. Each ray visits candidate bounds rather than all bodies/bones.
Cone calls refresh at their entry boundary and retain all-contact semantics;
beam queries retain their targeted current-pose path. Unprepared objects retain
an explicit native fallback, counted separately. Ordinary prepared-world queries
no longer call global PhysX raycasts or crawl renderer/bone hierarchies.

Query-phase changes, additions, geometry changes and reuse invalidate the snapshot.
Removals update live flags immediately without a full recapture. The independent
native fixtures additionally check all-hit and nearest proxy semantics, friendly
filtering, inside-origin rejection, masks and capacity overflow. The Unity-host
lifecycle checks cover compatibility pose invalidation, removal and reuse.

## Evidence and limits

[Full deployment checks](battle-scale/deployment-0.11.8.txt) passed and the
[installed-file manifest](battle-scale/deployment-manifest-0.11.8.json) verifies the
package, allowing only equivalent Settings.xml reserialization after normal exit.
The [headless host checks](battle-scale/headless-contact-0.11.8.txt) passed and
exited with code 0. No test process remains running.

The expanded [headless results](battle-scale/headless-contact-0.11.8.json) include
snapshot preparation, waits and repeated calls through the production no-contact
missile wrapper:

| Workload | Mean ms per batch | p95 | Maximum |
|---|---:|---:|---:|
| 32 ships / 500 queries without contact | 0.275 | 0.282 | 0.318 |
| 64 ships / 1000 queries without contact | 0.520 | 0.534 | 0.738 |
| 32 ships / 48 queries with arrivals | 0.309 | 0.321 | 0.493 |

These are synthetic scene measurements, not a before/after FPS claim for the
user's battle. The arrival fixture checks indexed geometry/bridging; it does not
simulate every native damage callback. The 0.10 ms target is not declared met.
Another live run of the same encounter remains the acceptance gate.

The probe now measures whole-frame wall time with Stopwatch and reports mean,
p95 and maximum, alongside compatibility rays, snapshot counts, snapshot/native/
bridge timings and fallback counts. Exclusive totals avoid counting compatibility
work twice when nested inside ballistic hit application. The native late-update
boundary remains a separately labeled inclusive measurement.

## Validation failure corrected

The previous 0.11.7 synthetic benchmark omitted the production missile/cone
compatibility scan. Its sub-millisecond worker timings did not establish end-to-end
performance. Retain those measurements as historical evidence, but treat 0.11.7
as a live-performance regression. Future acceptance must include every active
query consumer and whole-frame timing, particularly mixed missile/ballistic loads.
