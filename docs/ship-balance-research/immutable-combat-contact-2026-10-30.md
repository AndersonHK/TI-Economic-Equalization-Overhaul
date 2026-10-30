# Immutable prepared collision geometry for combat

2026-10-30. **0.10.17 deployed for testing.** The acceptance target is 15x
simulation speed in a sensibly sized battle with several dozen projectiles
arriving at one target without contact stalls. Full-game acceptance is pending;
the arithmetic benchmark below is not a substitute for that test.
Ship/projectile settings remain 10x/10x.

## What the new evidence established

The user's additional observation that lag grows with ship count and nearby
projectiles matches two source findings and the fresh
[0.10.16 diagnostic log](battle-scale/contact-performance-0.10.16.txt):

- New tree construction had reached zero, but skin bake/refit still dominated
  contact time. Some passes took 80-130 ms. One five-second interval recorded
  38.022 ms/frame of contact work, including 32.648 ms of skinned updates and
  4.803 ms of explicit physics synchronization. Another recorded 35,564 segment
  queries and 648 skinned updates in five seconds.
- Threat prediction called `TryBounds` repeatedly; it walked every rendered
  part for every request. Its share of frame time was not separately measured,
  but the repeated ship-by-projectile bounds scan was confirmed in source.
- Synced probes identify `Earth_Spikes_Radiator` as a bad broad envelope:
  authored skin bounds span thousands of local units, while the visible posed
  surface is much smaller. These are real oversized candidates, not an error
  explained solely by tiny helpers or an unsynchronized first probe. The prior
  helper-only explanation was incorrect.

The read-only [asset audit](battle-scale/skin-bind-audit-2026-10-30.json) records
base/DLC source hashes and ten mesh records with bind poses. All ten are readable
and have no blend shapes. The large human radiator has 22,670 vertices, 20 bones
and 180 vertices with multiple bone influences. Blended vertices cannot be
approximated by assigning the whole surface to its dominant bone. Three Ark
radiator meshes have no authored weight arrays; they use renderer-frame geometry.
The source contains genuinely blended skin data, so simply freezing the initial
baked pose would lose animated collision correctness.

## Preparation before engagement

`PreparedSkinnedContact` builds shared immutable data during model preparation:

1. Read bind vertices, triangles, weights and bind matrices once. Honor the
   renderer's one/two/four-weight quality, normalizing retained influences.
2. Put triangles rigidly attached to one bone into shared bone-local trees.
   They can be queried by transforming the ray, without skinning vertices.
3. Prepare blended-triangle trees and conservative bone-local influence
   envelopes. Their vertices are evaluated from current bone poses only when
   candidate triangles need them. Bounds include the space between influences;
   a blended contact is not required to lie inside either individual bone box.
4. Fit simple physical boxes to actual bone-local vertex envelopes and attach
   them to their bones. Remove the old giant whole-radiator authored box.
   Native transforms move these proxies without collider dimension rewrites.
5. Allocate per-instance pose/query scratch in preparation. Prewarm missile
   mesh/tree resources from weapon prefabs during carrier initialization,
   including a final pass after ship initialization. A new/reinforcement model
   prepares before entering use. Native projectile instances still need their
   ordinary lifecycle binding; shared geometry is already warm.

Static mesh trees likewise prepare once and own their input arrays. Unreadable
static assets use the existing pinned CPU fallback. Ark vertices without skin
weights use the renderer frame. Future blend-shape assets are explicitly guarded;
this implementation targets the audited native base/DLC assets.

## Battle-time reads and necessary pose state

Prepared mesh vertices, triangles, bone envelopes and tree nodes are never
rewritten by contact queries. There is no `BakeMesh`, `GetVertices`, full-mesh
vertex copy, tree rebuilding or tree refitting in the battle contact path.

Animated and moving targets still require their current pose. A separate,
preallocated scratch object stores the small matrix snapshot once per queried
frame. Rigid bone trees remain in local coordinates. For blended nodes/vertices,
scratch memoizes only pose-derived bounds and points actually requested by
queries. It does not walk all vertices or rebuild all nodes. Later projectiles
reuse those same pose results. Epoch changes invalidate scratch logically,
without clearing/copying an entire mesh or tree. This is mutable query scratch,
not a claim that a moving game needs no changing state at all.

`PreparedContactWorld` uses a shared body snapshot, body-bound rejection and
then part-bound rejection before precise contact. A prepared part is visited
once per projectile; multiple bone proxy colliders cannot repeat its query.
Distant bodies receive a cheap bounds test rather than another per-projectile
skin conversion. Primitive rounds use analytic fitted sphere/box tests and
cached poses. The ballistic path does not call `Physics.SyncTransforms`,
`OverlapCapsuleNonAlloc` or use a physics ray to recover collider identity.
The pinned `RaycastHit.m_Collider` instance-ID contract is verified automatically.

A scalar slab ray caches reciprocal direction/parallel state once, avoiding
repeated divisions and vector indexing at every visited node. A reusable hit
buffer avoids an additional managed allocation per admitted contact. Native
contact filtering, damage, impact and lifecycle remain; unrelated native game
code can still allocate or perform physics work.

Threat bounds have a separate frame/root-pose cache, and the combatant geometry
lookup avoids repeated component discovery. This removes repeated full-part
bounds scans across projectile threat checks. It stays separate from the
post-movement collision pose snapshot so an early threat request does not seed
collision with an earlier target pose.

## One trajectory remains authoritative

The native movement job remains the only ballistic flight integrator. Collision
uses the actual previous/current rendered positions after that job completes.
There is no second velocity integrator, forward admission margin, target snap
or probability-based contact. Moving-launcher velocity is retained.

The query is still a travelled centerline test, not a finite-radius swept round.
Target animation/motion is sampled at the current frame; this is not exact
continuous collision for a rotating/deforming target between frames. Native
missile guidance/AOE and beam behavior are not reimplemented here. Bone boxes
are simple conservative proxies; ballistic narrow-phase blended/static mesh
geometry is the contact authority. Visibility/destruction is checked during
admission. Unannounced runtime mesh replacement is not polled; prepared native
variants/lifecycle handling and manual testing remain important.

## Automated checks and measured workload

Normal deployment passed all **41 validators**, **1,219 formula assertions**,
**384 aperture comparisons** and verified **52 installed files**. The geometry
coverage includes 211 existing mesh cases plus 1,542 rigid/blended/primitive,
immutability, renderer-frame and pose-cache checks: **1,753 contact checks**.

An independent plane/edge reference checks posed triangles under translation,
rotation, nonuniform/mirrored scale and different weight quality. Tests cover a
blended surface between separated bone envelopes, strict segment limits, empty
corners, primitive starts inside volumes and unchanged prepared data. Replaying
projectile queries in the same pose must not recompute cached pose results.
IL guards reject live baking/refit/vertex-copy/global-sync calls. The hit-identity
contract is tested without instantiating a Unity scene.

The synthetic stress workload uses:

- 48 arriving segments, with a long completed path of 60 scene units. This
  exercises long-step querying; it does not advance the game clock at 15x.
- 32 body bounds, with 19 overlapping skins at the target, each containing
  22,704 vertices and 20 bones, including blended vertices. Bone regions are
  deliberately interleaved, making their broad envelopes loose.
- 20 warm-up frames and 220 measured batches. Separate preallocated pose scratch
  is invalidated each frame, so the test does not reuse one frozen pose cache
  across the entire benchmark.

The final normal-deployment run measured **p50 4.583 ms, p95 4.820 ms, maximum
8.797 ms**, with **zero measured managed allocation** in the query batches.
An earlier implementation of the same stress workload measured p95 53.116 ms;
these development runs were not a controlled full-game A/B benchmark. The
current result includes worst-case overlapping skin queries, but excludes Unity
pose-fetch overhead, rendering, AI, native projectile creation and other combat
systems. It is evidence of query cost and allocation behavior, not evidence that
the full battle now meets the 15x target.

## Deployment and acceptance

[Normal deployment log](battle-scale/deployment-0.10.17.txt).
[Installed-file manifest](battle-scale/deployment-manifest-0.10.17.json).
DLL SHA256: `E561BBA0295F7AB2888E3659AA4C946001A6F53DACE10A505491FFEA6981C6BF`.
Both closed-process guards passed. All 52 installed files were hash-verified;
the archive has the same twelve older omissions, with its forty present package
files matching. No new templates or scale settings were introduced.

Restart and repeat the same battle at **15x**, with a few dozen arrivals against
one target, then sustain later volleys. Compare normal speed and 15x at the same
camera, fleet and effects settings. Check impact location while radiators are
retracted, deploying and deployed; include Ark/DLC models, stations, point defense
and pooled/reinforcement assets. Watch baseline frame time as well as impacts:
there are more fitted bone proxies, so native physics/animation overhead also
needs runtime observation even though ballistic queries bypass physics syncing.

The bounded log now records actual `speed` and pause state, FPS, maximum contact
pass, shared pose time, body/part candidates, bounds scans, evaluated blended
bounds and mixed vertices. Tree builds/collider configuration should stop after
preparation, apart from genuinely new instance/resource preparation. Bone/pose
work should be shared across arriving projectiles rather than grow as full-mesh
work per projectile. `syncMs/frame` is the mod's legacy explicit-sync stage;
zero does not mean that Unity performs no other physics work.

The full 15x gameplay target, visual correspondence and native-host acceptance
are **pending**. This build is not yet an accepted stable baseline. No agent
launched or manually played the game for this change.

## Separate prior-build log observation

At final readback, Player.log still identified 0.10.16, not a 0.10.17 run.
It also contained a paused shutdown/disable exception in native
`TISpaceFleetState.Disband`, reached through ship destruction and
`SpaceCombatManager.OnDisable`. That is separate from the measured contact
stall and is not attributed to this change. No fix for that exception is claimed;
its reproduction/causality remains uninvestigated here.

## Limited user testing follow-up

The user subsequently reported adequate performance in limited testing. The
[width-only follow-up](laser-width-only-2026-10-30.md) records that feedback
and fresh logs. A logged 15x interval has no contact candidates, so the full
clustered-arrival target remains unconfirmed. The contact implementation is
retained by the laser-only visual correction.
