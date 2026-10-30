# Projectile contact stall: 0.10.15 follow-up

2026-10-30. **0.10.16 deployed for testing**, not accepted as a stable baseline.
The ship/projectile settings remain 10x/10x. The sole native ballistic movement
calculation and strict completed rendered segment from 0.10.15 are retained.

## Observed failure and evidence

The user reports normal performance away from contact and below 1 FPS while
rounds approach ships, recovering after the relevant rounds impact. The fresh
0.10.15 [diagnostic excerpt](battle-scale/contact-performance-0.10.15.txt) shows:

| Interval | FPS | Contact ms/frame | BVH construction ms/frame | Physics sync ms/frame | New trees |
|---|---:|---:|---:|---:|---:|
| Before mesh contact | 36.1 | 16.276 | 0 | 15.989 | 0 |
| First logged hull contacts | 17.5 | 45.476 | 29.229 | 15.637 | 64 |
| Following hull contacts | 26.8 | 26.485 | 10.744 | 15.387 | 34 |

The first contact interval spans approximately five seconds and 87 frames;
construction therefore consumed about 2.5 seconds across that interval.
This is measured contact-time setup, not a speculative broad-phase diagnosis.
It is a confirmed contributor, not a complete measurement of the worst stall:
0.10.15's twelve-report probe ended after one minute and missed the later
sub-1-FPS period described by the user.

The separate synchronization cost is consistently around 16-20 ms/frame.
Construction and sync are included within contact-stage timing; do not add
all three columns. The actual cost of queries after warm-up was comparatively
small in these captured intervals. No measured post-fix speedup is claimed.

## Cause in code

`CombatMeshGeometry.RaycastVisual` called `RenderedMeshRaycast.For` only when
a projectile overlapped a part. The cache built an entire triangle BVH on a
cache miss. Several previously unseen parts could therefore build trees in
the same combat frame. Building lazily deferred substantial initialization work
until the first close projectile pass.

Skinned meshes had a second problem: every queried frame baked a snapshot,
invalidated its cached tree and rebuilt the topology. Static trees were cached,
but the skinned path could repeat the expensive construction. The logs do not
separate which of the 98 observed builds were static versus skinned, so this
report does not assign the whole stall to skinning alone.

## Implemented correction

- `CombatMeshGeometry.Refresh` prepares static contact trees during object
  setup, sharing them per mesh. Each part retains the prepared tree reference;
  `RaycastVisual` no longer constructs, looks up or invalidates a tree. This can
  increase initial loading/preparation time; it removes that cold work from
  approaching-projectile queries.
- Skinned parts prepare their topology once. A queried animated snapshot
  updates retained vertex storage and refits existing leaf/ancestor bounds
  in place, once per queried frame. It does not sort, allocate new BVH topology
  or invalidate the cache. Animation sampling is still dynamic work; it is
  timed separately and may warrant further optimization if measurements demand it.
- Registered primitive rounds use their already fitted primitive collider.
  Their decorative sphere mesh no longer triggers a lazy triangle-tree build
  from the contact path.
- The strict completed-path/visible-static-mesh test is preserved. There is no
  return to forward prediction, aim-direction casting, enlarged hit tolerances,
  target snapping or predetermined contacts. Static scale and collider
  dimensions are still configured during creation/reuse, never each frame.

Physics synchronization is retained to avoid stale target poses. Its measured
16-20 ms/frame cost is unresolved by this narrow correction. Removing it without
an alternative pose-consistent broad phase would risk reintroducing collision
mismatch. The next runtime result should distinguish this floor from the
reported catastrophic contact stall.

## Diagnostics and collider-fit uncertainty

The periodic probe now runs for up to 120 five-second intervals (about ten
minutes), rather than twelve. It adds maximum individual contact-pass duration,
skinned-refit count and refit milliseconds/frame. A separate capped set of
32 messages records contact passes taking at least 50 ms, including after the
periodic window. Slow-pass counts are cumulative within the current periodic
interval, not individual-pass deltas. This keeps the diagnostic interpretation
explicit while bounding log output.

The old bounds probes reported relative errors near 99 **and absolute errors
over two scene units**. The earlier suggestion that tiny helper parts alone
explained those readings was insufficient. The second-frame probe can run
before the explicit physics synchronization, so stale PhysX bounds are also
possible. 0.10.16 records both initial and post-sync probes, plus the worst
part's renderer bounds, physics bounds, local box size, child scale, renderer
scale and mesh bounds. These are readbacks; they do not rescale objects.

Whether the large discrepancy persists after synchronization is unresolved.
The exact static-mesh narrow phase still prevents an oversized broad box alone
from admitting damage. No claim of perfect collider fit follows from the old
aggregate ratios. Finite projectile-radius sweeps, exact continuously moving
or rotating target contact and initialization-late mesh replacement remain
limitations described in the [0.10.15 report](physical-projectile-contact-and-performance-2026-10-30.md).

## Verification and installed state

The normal deployment completed with both game-closed assertions, all
41 validators, 1,219 formula assertions, 384 aperture comparisons and
52 installed files verified. The 211 existing contact cases remain, with four
additional translated/reflected skinned-refit cases checking that nearest-hit
results update while the allocated tree topology stays the same. IL guards
reject contact callbacks that call BVH construction/cache invalidation.
The implementation matrix remains 119 feature rows / 253 Harmony classes.

[Deployment log](battle-scale/deployment-0.10.16.txt).
[Installed-file readback](battle-scale/deployment-manifest-0.10.16.json).
DLL SHA256: `C07F9BC2BD1C089D8F120EEB7FB819EEAA435AF09003B484B84E075F2111058F`.
The local archive retains the same twelve older packaging omissions; its forty
present package files match. The installed package has all 52 files.

## Runtime acceptance

Restart and recreate the same skirmish with the same ship count, settings,
formation, camera and time speed. Follow the first volley through approach and
impact, then later volleys past the original one-minute logging cutoff.
Check whether the sub-1-FPS episode is gone and whether impact positions remain
correct. A loading-time preparation pause is distinct from a combat-time stall.

For a fresh battle, static-tree construction should occur during preparation,
not on first hull contact. Compare `bvhBuildMs/frame`, `skinRefitMs/frame`,
`syncMs/frame`, `maxPassMs` and any `Slow ballistic contact pass` messages.
Compare `:initial` and `:synced` geometry readbacks before deciding whether a
fit discrepancy is real. Do not call this stable until actual gameplay and
performance results support it. No game launch or manual playtest was performed
by the agent.

## Subsequent immutable-geometry correction

[0.10.17](immutable-combat-contact-2026-10-30.md) replaces the live skin
bake/refit path with prepared bone-local forests and separate shared pose/query
scratch. It also removes ballistic physics sync and repeated threat-bound scans.
The 0.10.16 logs above remain historical evidence; full 15x acceptance is pending.
