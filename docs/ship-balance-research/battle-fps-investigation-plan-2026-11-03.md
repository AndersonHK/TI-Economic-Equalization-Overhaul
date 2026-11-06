# Battle FPS regression: investigation and proposed plan

2026-11-03 plan, **approved for implementation on 2026-11-04**. This document
records the preimplementation investigation and requirements, following the
[prepared-contact implementation](immutable-combat-contact-2026-10-30.md).
The [0.11.7 implementation/evidence](battle-worker-contact-2026-11-04.md) records
what was built and tested, including limits on the performance claims.

## Revised performance requirement

Revision requested by the user on 2026-11-03: favor loading-time preparation and
RAM over recurring CPU work. The objective is zero or near-zero **added main-thread
collision cost**, including the current ballistic pass and pose subset, while
preserving intended behavior. A substantial rewrite is acceptable. This also
sets the standard for other patches; see the [project performance directive](../design-directives.md#runtime-performance).

The existing timers measure CPU simulation work executed on the main thread,
not GPU rendering. That work delays frame delivery. Calling it simulation,
moving it to FixedUpdate, renaming its timer, or moving it to a worker followed
by an equivalent main-thread wait does not meet the objective.

Preparation and retained memory are preferred costs: prepare geometry, indices,
lookup tables, buffers and native bindings before engagement. Keep representation
and scheduling simple. More memory does not make arbitrary future motion or
animation precomputable, so changing poses and actual impacts still require
work; isolate and minimize that work rather than promise literally free collision.

## Finding and confidence

The strongest remaining suspect is the Unity-side cost of the expanded moving
collider/transform hierarchy. It is not yet a measured root cause. The prepared
ballistic query also has confirmed avoidable per-frame pose work and all-body /
all-projectile scans, but existing measurements do not support blaming it for
most of the current frame time. That is not a reason to retain its 1â€“3 ms/frame
cost. Rendering and native ship/weapon updates remain unmeasured alternatives
for the additional FPS loss and must be investigated alongside the redesign.

The user's observation that FPS scales with ship count is consistent with those
paths, but does not alone distinguish CPU physics, animation, rendering or GPU
work. No claim that FPS has been recovered follows from this investigation.

## Evidence from the current implementation and existing run

The inspected native assembly is TI 1.0.53b, SHA256
`4A4B9AAE4154E444E9727204205D2D42AE8ED9E1C5F92CDC1280074A259D8350`, matching
the pinned [native-source manifest](battle-scale/mesh-fallback-sources-2026-10-29.json).
Player.log was last written on November 2; it is existing evidence, not a fresh
controlled test. Its manager loader identifies mod 0.10.23, while a separate
hardcoded initialization message still says 0.10.14. The current package manifest
also identifies 0.10.23. Unity identifies itself as 2020.3.49f1.
The [bounded evidence snapshot](battle-scale/fps-investigation-2026-11-03.json)
retains source hashes, battle boundaries and selected log lines.

| Existing battle sample | FPS | Approx. total ms/frame | Ballistic pass ms/frame | Pose subset ms/frame |
|---|---:|---:|---:|---:|
| First battle, before ballistic queries, 15x | 47.7 | 20.96 | 0.000 | 0.000 |
| First battle, 1x | 43.9 | 22.78 | 1.807 | 0.994 |
| First battle, 15x | 42.2 | 23.70 | 1.864 | 0.985 |
| First battle, later contacts, 15x | 31.5 | 31.75 | 3.406 | 0.986 |
| Second battle, 1x | 59.5 | 16.81 | 0.930 | 0.519 |

Samples are consecutive windows from play, not matched-camera A/B tests. Speed
and pause labels are sampled at the end of each window; a window may straddle a
change. Pose time is included in ballistic time. Most frame time is outside the
timed pass; that remainder cannot be labeled GPU or physics time from this log.
Even eliminating the entire 1.807 ms pass would only take the 43.9 FPS sample to
about 47.7 FPS under a simple serial frame-budget assumption. It would not explain
a return to a substantially faster baseline.

The first battle registers 23 ships and 10,067 box configurations during setup;
the second registers 12 ships and 5,342. These are setup operations, **not measured
active collider counts**. Inactive model alternatives are included. The readback's
`boxes=57` for a human Battlecruiser counts parts, not all bone boxes: its loop
increments once per part, then unions the remaining boxes. Thus that label
substantially understates the created hierarchy.

The existing probe also keeps emitting after `PostCombat Container Destruction`.
Those campaign-screen windows, including `speed=43200`, must be excluded from
battle evidence. The proposed instrumentation must have explicit battle scope.

## Source trace

1. [CombatMeshGeometry.Bind/AddBox/Refresh](../../TIEconomyMod/Core/CombatMeshGeometry.cs)
   enumerates renderers including inactive alternatives. It creates a GameObject,
   Transform and enabled BoxCollider for every static part and every used skin
   bone envelope. Original damage colliders are disabled. The added components
   stay attached to moving/animated transforms even after the mod's second-frame
   readback disables its own LateUpdate. Renderer visibility is checked when
   admitting a hit, but is not coupled to the physical collider's enabled state.
   Inactive GameObjects are inactive in physics; a disabled renderer on an active
   object is a different case and needs counting.
2. Ballistics now query [PreparedContactWorld](../../TIEconomyMod/Core/PreparedContactWorld.cs)
   independently of PhysX. Removing the explicit `Physics.SyncTransforms` call
   removed its measured cost, but did not remove the physical collider hierarchy.
   Unity still has its normal transform/physics work. The installed runtime's
   `autoSyncTransforms` and actual `attachedRigidbody` assignments were not read
   from a live scene, so neither automatic-sync settings nor static-collider
   classification should be assumed.
3. [TryContact/Snapshot](../../TIEconomyMod/Core/CombatMeshGeometry.cs) snapshots
   every visible part, reads bone matrices and assembles body bounds before the
   first body-bounds rejection in a frame. A distant passing projectile therefore
   triggers pose work for an eligible ship even if no body candidate is admitted.
   One existing window has zero body candidates but 332,640 bone pose reads in
   roughly five seconds. Snapshots are shared across later queries that frame;
   they are not repeated full-mesh skinning per projectile.
4. `PreparedContactWorld.Cast` scans all registered bodies and all active rounds
   for each ballistic segment. Even friendly rounds incur the loop/filter cost.
   Worst-case scan work scales approximately as P*S + P*P, before narrow-phase
   work, where P is active rounds and S is bodies. No spatial world index exists.
   The current body-check metric omits the active-round loop.
5. [BattleProjectileGeometry](../../TIEconomyMod/Core/BattleProjectileGeometry.cs)
   adds an always-active physical sphere-mesh renderer per live round, plus a
   primitive collider and LateUpdate that checks tactical display eligibility.
   The physical body renderer is not suppressed at tactical distance. Its actual
   draw/GPU cost is unknown; it is separate from contact timing.
6. Native `MissileController.HasHitTarget` still uses `Physics.RaycastAll`.
   Shaped-nuclear checks use `TIUtilities.SimpleConeCastAll`. The native projectile
   filter resolves a collider's parent damageable; the mod's filter also requires
   an enabled registered collider. Disabling/deleting all fitted colliders would
   break these contracts without replacement. A complete consumer audit is
   required before changing the representation, including beams/point defense,
   selection, formation, splash damage, pooling and restoration.
7. Renderer mesh baking and collider dimensions are initialization/reuse work;
   their recurring setters were already removed. Laser width configuration also
   runs at binding. Repeating those earlier fixes is not a useful first step.

Unity 2020.3 documents that parent Transform changes can require updating child
colliders and that interleaving changes with queries can incur synchronization
cost ([autoSyncTransforms](https://docs.unity3d.com/2020.3/Documentation/ScriptReference/Physics-autoSyncTransforms.html)).
This supports testing the hierarchy hypothesis; it does not measure this game.
The version-matched [collider manual](https://docs.unity3d.com/2020.3/Documentation/Manual/CollidersOverview.html)
also does not justify the blanket claim that any moving static collider is
catastrophically expensive. Adding rigidbodies everywhere is not an established fix.

## Revised architecture and work sequence, after approval

### 1. Establish the behavior contract and a main-thread budget

Preserve the intended behavior, not the current machinery:

- One authoritative native flight calculation, including launcher velocity.
  Collision consumes its completed segment; it must not independently integrate
  or extrapolate another trajectory.
- Strict travelled centerline contact, nearest valid visible surface, current
  articulated/skinned pose, faction/destruction rules and native damage/impact
  behavior. Retain the current limitation of sampled target pose rather than
  quietly introducing or claiming continuous moving-target collision.
- No forward extension, snapping, hit probability, enlarged final hit volumes,
  stale animated poses or unapproved one-frame impact delay. Coarse bounds may
  produce false candidates, never false damage or missed true contacts.
- Preserve missile guidance, existing missile/cone query semantics, point
  defense, stations, selection/navigation and battle lifecycle. Any intentional
  behavior change is a separate decision, not a performance optimization.

Proposed engineering acceptance targets on the user's machine, to make
"near-zero" testable (targets, not measured promises):

| Cost | Target |
|---|---|
| Collision work on render-only frames with no simulation/state change; settled paused battle | No scans, pose gathers, queries or worker dispatch; at most a constant-time gate |
| Routine added main-thread collision overhead, 32 ships / 256 live rounds, including distant volleys | Mean <= 0.10 ms/frame and p95 <= 0.25 ms/frame |
| Main-thread pose sampling | No full part/bone walk; any necessary compact capture is included in the same total budget |
| Steady collision allocations | Zero per-frame managed allocations after preparation |
| Clustered impacts and larger stress cases | Report p95/p99/max, worker deadline misses and cost per committed hit; no concealed multi-ms stalls |

The total budget includes capture, marshaling/copies, scheduling, integration,
waiting, result validation and patch-added damage-dispatch overhead. Native damage
and effects remain visible in whole-frame measurements and separate event timing;
they must not become an unmeasured place to hide the rewritten pass. Test 48
arriving rounds against one target, then 64 ships / 1,024 live rounds as a scaling
stress case. Report actual achieved limits rather than broadening the budget
silently. The old metric must not disappear without an equivalent full accounting.

### 2. Prepare a compact collision world before engagement

Use immutable shared mesh data and flat arrays of indices, bounds and transforms.
Reuse the existing verified triangle/bone mathematics where appropriate; discard
per-part GameObjects and managed object graphs that exist only to support queries.
Spend loading time on static BVHs, rigid bone-local geometry, blend influence
metadata, conservative animation envelopes, model-variant lookup and reusable
worker/output buffers. Allocate pools/capacity with headroom before engagement;
prepare reinforcement assets before they become active. Account for any unavoidable
new-resource preparation during combat separately and move heavy preparation off
the main thread where safe.

Keep runtime body/part identities and generation numbers separate from Unity
Collider identities. Static geometry should never be revisited just because a
frame was drawn. Visibility, equipment changes, destruction and reuse update
explicit state at their lifecycle boundaries. Do not replace this with a recurring
whole-hierarchy discovery scan. Prewarming inactive variants is welcome; creating
live physical proxies for all of them is not.

Use conservative body-level spatial indexing followed by existing precise
geometry tests. Avoid the current all-bodies and all-rounds scan per projectile.
Choose the simplest suitable dynamic AABB tree or other measured broad phase;
long segments must traverse all intersected regions without candidate truncation.
The index is maintained by simulation changes, preferably on workers, not by
renderer callbacks. Spatial indexing is part of the proposed design, rather than
an optional micro-optimization after accepting quadratic scan behavior.

### 3. Run collision as batched simulation work, off the main thread

The native `ProjectileJobContainer` already schedules `ProjectilMovementJob`
in Update and completes it in LateUpdate. Our current postfix then performs a
second serial pass over all flights. Replace that serial collision pass with a
coherent simulation pipeline:

1. Publish a simulation epoch and immutable state inputs, with stable body/round
   IDs, active-part masks, origin/rebase data and completed native movement
   endpoints. Express dependencies on native movement and target pose production.
2. Perform broad-phase maintenance, candidate queries, matrix/pose arithmetic and
   precise intersections in batched worker work using prepared arrays. Use safe
   job dependencies and supported transform access where necessary; ordinary
   Unity objects and native damage callbacks must not be accessed from arbitrary
   worker threads.
3. Return compact contact candidates, then validate and commit only actual events
   on the main thread through a small native damage/lifecycle bridge. Avoid a
   second managed per-projectile UpdateController loop just to discover no hit.
   Range expiry and other original controller duties must remain covered by the
   simulation/lifecycle bridge.

Use the **actual native simulation advancement**, not a new independent clock or
second flight integrator. Re-rendering unchanged state must not repeat collision.
The native advance currently often occurs once per rendered frame; calling it a
simulation epoch alone does not reduce the rate. Off-thread computation and cheap
state transfer are essential. Moving to a different fixed timestep is not an
assumed optimization because it can change collision semantics.

Pose acquisition is a first-class design problem, not a cost left on the main
thread after moving triangle tests. Keep static and rigid-part geometry local,
read changing root/animation state in batches, and use conservative envelopes to
avoid detailed pose work for distant bodies. Evaluate deterministic articulated
transforms from authoritative animation state only where it reproduces the
renderer pose; otherwise use compatible worker-safe transform sampling or a
strictly budgeted compact main-thread gather. Precomputed discrete poses must
not freeze/interpolate away real contacts. Hidden, damaged and pooled parts need
explicit invalidation. Replace render-frame cache stamps with simulation/pose
versions while preserving necessary within-step invalidation.

Worker completion must precede the dependent damage/presentation boundary.
Schedule as early as valid inputs allow and overlap independent engine work.
A job scheduled in LateUpdate and immediately waited on is not sufficient.
Measure every completion wait; if no safe overlap window exists, revise scheduling
or the algorithm rather than defer impacts or allow stale results. Threads do
not create CPU capacity: bound concurrency, avoid oversubscription with native
jobs, and test worker saturation at 15x.

Batching changes mutation hazards. A hit can destroy a target or projectile before
another prepared result commits. Preserve native ordering where observable,
validate object generations/live state, and retain enough ordered candidates or
support bounded requery so discarding an invalid nearest hit does not lose a valid
hit behind it. Define handling of penetration, projectile interception, pooling,
rebasing and battle teardown before implementation; never read buffers still in use.

**Feasibility gate:** validate the installed Jobs/Collections APIs and the job
registration/data ownership path before selecting the execution backend. The
native movement job is Burst-annotated, but that does not establish that a newly
injected mod job will receive Burst compilation in this shipped player. Unity's
[Burst compilation documentation](https://docs.unity3d.com/ja/Packages/com.unity.burst@1.8/manual/compilation.html)
describes player AOT compilation. Benchmark the backend actually available:
compatible Unity jobs first, or a small persistent managed worker pool using
immutable plain data if necessary. Do not spawn Tasks per projectile or assume
an annotation makes mod code native. Unity also documents that
[job completion can stall the main thread](https://docs.unity3d.com/es/2021.1/Manual/JobSystemTroubleshooting.html);
those waits count against the target.

### 4. Remove duplicated physical collision infrastructure

The desired endpoint is one data-driven damage-geometry authority, without
thousands of moving EEO Collider/Transform objects duplicating the prepared world.
Audit every native consumer before replacement: missile raycasts, shaped-charge
cones, beam/point-defense targeting, damageable lookup, hit lists, selection,
formation, threat estimates and restoration. Record which consumers need candidate
selection, exact contact, proximity or only object identity.

Route consumers through the shared prepared query service or retain the smallest
verified compatibility interface that they need. Preserve each query's existing
semantics; do not silently convert missiles to ballistic timing or change splash
rules. A synchronous native query consumer can still force work/waits onto the
main thread: batch or reschedule where behavior permits, otherwise account for
and optimize its bounded query cost explicitly. A dummy collider for native hit
identity must not require a live detailed physics hierarchy. Its disabled-state
and filtering contracts need validation in the host.

Do not replace detailed hull contact with one sphere/box as the final damage
surface. Do not disable global physics synchronization to conceal stale native
query data. Simply attaching kinematic rigidbodies to thousands of proxies leaves
the duplicate representation and is not the preferred architecture.

### 5. Apply the same cost standard to other patches

Audit all recurring patch entry points, beginning with the battle path: threat
bounds and direct-fire commitment pruning, scale getters, camera/grid/path hooks,
laser render callbacks, station observers, projectile tactical displays and
native methods whose frequency makes an apparently small postfix expensive.
Continue the same inventory for campaign/UI patches; do not infer they are cheap
merely because they have no Update method. Report call frequency, allocations,
engine calls, cache invalidation and measured aggregate cost for each hotspot.

Move invariant work to loading and value changes. Use event-driven invalidation,
shared cached configuration and one batched update where dynamic work is needed.
Replace per-round MonoBehaviour display polling with shared camera/display state
and a measured batch if beneficial. Check added projectile draw calls, mesh/material
sharing and GPU/CPU rendering costs independently of collision; preserve physical
visibility and accepted scale/nose rendering corrections. Avoid a speculative
whole-repository rewrite: investigate every recurring cost, implement demonstrated
or structurally unnecessary work reductions within the approved plan, and bring
any additional gameplay changes back for review.

### 6. Measure, validate, deploy and compare

Measurement is an early design input and a final acceptance requirement, **not a
reason to retain the known serial pass until it proves to be the largest hotspot**.
Use a bounded, combat-scoped probe with speed/pause/state transitions separating
windows. Record whole-frame timings, main/worker costs and waits, GPU/rendering
metrics when available, allocations and live object counts. Enumerate supported
profiler counters once; missing counters are unavailable, not zero. Inventory
colliders at setup/lifecycle events rather than scanning the scene each frame.
Measure the probe's own overhead and disable detailed diagnostics for final timing.

Keep the earlier controlled tests: matched 8/16/32-ship skirmishes, paused/1x/15x,
pre-fire/distant volleys/clustered arrivals, fleet/close cameras, fixed graphics,
resolution and frame cap, warmed resources and repeated windows. Add the 64-ship
stress case and compare the slope of ms/frame against ship/projectile count.
Use collider-only suppression in a non-firing test and rendering-only suppression
that preserves contact state to locate remaining engine cost. These experiments
are diagnostic, not delivered gameplay modes. Retain the unchanged current build
and, if available, a genuinely comparable pre-patch baseline.

Before production integration, verify a compact worker-path prototype against the
current contact reference and an independent geometric oracle. Include moving
launchers, strict endpoints, empty broad-box corners, nearest surfaces, mirrored
scale, blended animation, long high-speed steps, multiple hits on one target,
interception, stale-generation rejection and ordered commit/requery behavior.
Test actual Unity pose acquisition and scheduling in addition to headless maths.
If the backend cannot achieve the budget without violating the behavior contract,
report that finding and revise the architecture rather than silently adopting
approximate contact or delayed damage.

After approved implementation is ready, run normal `tools/deploy.ps1` immediately,
including both game-closed assertions and automatic verification. Announce manual
readiness on successful deployment, then finish documentation while testing
proceeds. Test missiles/cones, point defense, stations, radiators/turrets,
hidden/destroyed parts, pooling, reinforcement and exit/reentry. Repeat the same
performance cases and report absolute main-thread savings, worker cost, waits,
frame-tail behavior and any remaining budget failures. Retire redundant old paths
and temporary diagnostic bypasses. Do not claim success from a zeroed/removed
legacy timer or synthetic query benchmark alone.

The user approved this replacement of the serial architecture. Implementation
and remaining acceptance work are tracked in the
[0.11.7 record](battle-worker-contact-2026-11-04.md); the budgets above remain
targets, not an automatic acceptance claim.
