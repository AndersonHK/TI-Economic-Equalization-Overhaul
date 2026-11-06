# Battle collision worker implementation

**Superseded after live failure, 2026-11-05.** The compatibility bridge consumed
over 600 ms/frame after projectile firing. The prior headless fixture omitted
that production path. [0.11.8 rework and evidence](compatibility-query-rework-2026-11-05.md)
records the replacement and its required live retest. The measurements below
remain historical, not proof that 0.11.7 improved whole-game performance.

2026-11-04. **0.11.7 testing implementation**, following the approved
[performance plan](battle-fps-investigation-plan-2026-11-03.md). The last
user-accepted stable baseline remains 0.11.6. Rendered battle/FPS acceptance is
pending; the user requested analytical/headless verification while occupied with
another project.

## Implemented architecture

- Prepared geometry is exported once into flat, immutable arrays. Static trees,
  rigid bone-local trees and blended influences retain the existing contact
  mathematics. Missile assets are also exported during loading. Poses and the
  spatial index are separate mutable simulation data.
- Unity transform jobs capture current world matrices. An optimized x64 numerical
  kernel updates the spatial index and intersects completed native movement
  segments in small worker batches. Candidate inverse matrices are calculated
  only when needed. There is no second flight integration or added impact delay.
- The first managed-job prototype passed correctness checks but took several
  milliseconds when immediately joined. It was not accepted as near-zero cost.
  The native kernel removes that numerical bottleneck; both implementations are
  exercised against independent host expectations.
- Slots and buffers are reserved during preparation. Projectile reuse updates
  existing records. Pose membership uses reference counts, and inactive projectile
  pools are removed from active body registries. Visibility changes update the
  index without unconditional topology rebuilding. An adaptive bounds-quality
  check requests rebuilding when needed, rather than on a fixed frame interval.
- The main thread publishes compact flight records, waits at the native commit
  boundary and applies actual hits/range expiry through the native controller.
  Purely ballistic containers bypass their now-redundant pre-movement controller
  loop; mixed containers retain native missile updates. Direct container removals
  also update ownership counts. Launcher-clear timing remains covered.
- Generation checks reject results invalidated by destruction, reuse, faction or
  visual-part changes. A stale nearest contact is requeried over the same completed
  segment, so an invalid first target cannot hide a valid target behind it.
- Live mesh/bone damage boxes are replaced by disabled identity components.
  Missile/cone queries retain fitted-box semantics through a compatibility bridge;
  beam contact preserves its first-part/first-bone ordering. Ballistic hull contact
  remains precise triangle contact. Native damage and effects are not replaced.

The numerical DLL uses ABI `0x10002`, checked with twelve record-size contracts
and matrix-field offsets before patches are installed. A mismatched native/managed
package fails initialization. `tools/build.ps1` builds it with MSVC v143 x64,
Windows SDK 10 and a static runtime. Install the whole package, including
`Assembly/TIEconomyContact.dll`, rather than copying only the managed assembly.

## Behavior and geometric boundary

The completed travelled centerline, inherited launcher velocity, nearest valid
surface, current rigid/blended pose, faction filtering and native hit application
remain. Existing limits remain too: this is not a finite-radius projectile sweep
or exact continuous collision against rotating/deforming targets between poses.

Native low-count ship/station body colliders are retained for navigation and
ramming. This restores their body footprint instead of using the previous EEO
per-part damage-box footprint for body physics. The weapon geometry remains
separate. Body-contact footprint and ramming acceptance therefore need an actual
scene test; unchanged native callbacks do not prove identical physical contacts.
No new friendly collision damage or steering policy is included.

## Verification and measurement

The normal deployment flow includes process-closed guards, 45 validators,
1,887 formula assertions, the existing exact-contact cases and 8,525 independent
native ABI/geometry fixtures. Native fixtures include tiny, mirrored and
nonuniform transforms, rigid/blended/primitive contacts, nearest/strict-segment
selection, faction/live filtering, dormant-leaf activation, spatial refits and
parent-relative rebasing.

The Unity-host self-test runs during preparation and checks the managed reference
and native jobs, including moving blended poses and disabled collider identities.
The explicit headless harness also checks reserved-slot reuse, stale generations,
faction/size changes and zero-flight warmup. Its final results are recorded below.

The headless workload uses actual Unity jobs and transforms, with synthetic
geometry/poses: 40 static parts plus four 20-bone skins per ship, 2,048 triangles
per shared asset, and physical projectile targets. It measures state publication,
dispatch, immediate completion and result reading. Immediate completion includes
all waits and permits no artificial independent-work delay to hide them. It does
not execute a full battle's AI, native damage/effects, GPU rendering or every
production registry/controller callback. Thus it is neither a rendered FPS test
nor proof that the full production path meets the budget.

[Final headless results](battle-scale/headless-contact-0.11.7.json) and [host self-test excerpt](battle-scale/headless-contact-0.11.7.txt): **1,764 host contact checks and lifecycle checks passed; exit code 0**.

| Headless workload | Mean ms/batch, including waits | p95 ms/batch | Maximum ms/batch |
|---|---:|---:|---:|
| 32 ships / 256 rounds distant paths | 0.247 | 0.308 | 0.356 |
| 32 ships / 48 rounds clustered arrivals | 0.305 | 0.362 | 0.529 |
| 64 ships / 1024 rounds distant paths | 0.488 | 0.564 | 0.662 |

The routine fixture **does not meet the proposed 0.10 ms average / 0.25 ms p95 target**. These measured sub-millisecond costs are not described as zero, and dispatch-only time is not substituted for the total. Real battle FPS and the full production-path budget remain unverified.

[Deployment verification](battle-scale/deployment-0.11.7.txt) verified 58 installed files before testing. The [post-run source/installed/archive manifest](battle-scale/deployment-manifest-0.11.7.json) verifies all archive entries, 57 byte-identical installed files and a normal Settings.xml reserialization with equivalent values. Runtime-generated caches are listed separately. The headless process exited; no test-launched game was left running.

The [managed prototype measurements](battle-scale/headless-contact-managed-prototype-2026-11-04.json) explain why numerical work moved to native code. Its earlier fixture differs from the final one, so these are development measurements, not a controlled FPS comparison.

The Mono player does not expose the per-thread allocation API used by this
harness. Allocation values are explicitly unavailable, not reported as zero.
The ordinary path reuses its buffers, but that structural fact is not a measured
zero-allocation result for the entire game.

## Runtime diagnostics and remaining costs

`Combat contact performance` now records preparation, explicit waits, commits,
native late-update boundary time, synchronous missile/cone/beam compatibility
queries, threat/view bounds scans and ballistic spawning. `mainCollisionMs/frame`
includes preparation, completion/commit and compatibility queries;
`mainGeometryMs/frame` adds bounds scans and measured ballistic spawning.
The native boundary contains existing game work and possible implicit waits; it
must be compared with a matched baseline, not added to the other totals.
Legacy triangle/pose counters describe serial paths only, not native-worker work.
Post-combat campaign-screen samples are excluded.

The [recurring-callback inventory](battle-scale/recurring-patch-inventory-2026-11-04.json)
records the source audit, not measured costs for every patch. Mesh baking and
dimensions remain setup-only. Beam appearance configuration remains bind-time.
Outstanding runtime profiling includes native physics/body callbacks, rendering,
per-round tactical displays, direct-fire/weapon cadence, and synchronous
threat/view bounds or missile/cone consumers. Native query compatibility preserves
their timing contract; they have not all become asynchronous. A zero legacy pose
timer must not be interpreted as zero total added frame cost.

## Manual acceptance still required

Repeat matched 8/16/32-ship skirmishes, paused/1x/15x, pre-fire/distant volleys/
clustered impacts, and close/fleet camera views. Check native damage, point defense,
missiles, shaped charges, stations, animated/destroyed parts, reinforcements,
pooling and battle exit/reentry. Include the reported overlapping-friendly case
for performance observation without claiming its damage/avoidance policy is fixed.

The separate [ship collision and friendly-avoidance plan](friendly-ship-collision-plan-2026-11-04.md)
describes that next behavioral iteration. No commit or push is part of this task.

## Additional ship-overlap report

The user reports severe lag when accelerating friendly ships overlap, without
collision damage or useful friendly avoidance. The attached screenshot shows
overlapping ships but cannot establish callback count or timing. Inspection of
the pinned native `CombatShipController.OnCollisionEnter` finds that damage is
gated on the other controller equaling `primaryTarget`; this is not a general
friendly-collision damage implementation. Native OnTriggerEnter/Exit passes ship
and station colliders to waypoint navigation. Added damage proxies can therefore
affect native physics/navigation independently of ballistic query time.

Keep ship-body contact/navigation separate from weapon faction filtering and
weapon damage meshes. Preserve native ship-body behavior during this performance
iteration. Plan friendly collision/avoidance separately after performance
verification; do not silently enable friendly fire or invent ramming damage rules.
