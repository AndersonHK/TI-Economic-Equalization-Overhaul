# Physical projectile contacts and performance regression

2026-10-30. Version **0.10.15**, experimental testing build. Ship and projectile
settings remain **10x/10x**. This record supersedes recurring scale-refresh
behavior described in the earlier geometry reports; their dated deployment
receipts remain historical evidence.

## Problem and evidence boundary

The user marked a projectile disappearance/damage point ahead of the expected
visible hull contact, reported inconsistent near misses/hits, and requested
removal of any forced near-miss or predetermined-hit behavior if found. The
user also reported approximately tenfold slower gameplay with around twelve
ships. That ratio is user-observed; a controlled before/after profile is pending.

[Annotated example](battle-scale/manual-projectile-early-contact-2026-10-30.png).
Static source and IL were inspected from the installed Assembly-CSharp.dll,
SHA256 `4A4B9AAE4154E444E9727204205D2D42AE8ED9E1C5F92CDC1280074A259D8350`.
Automated tests below do not constitute an in-game contact or performance test.

## Confirmed causes

The native ballistic collision query starts at the projectile's current
position and casts forward using `transform.forward`, for speed times the
current frame duration (originally with a 1.2 margin). This is a forecast of
future travel, so it can report damage before the projectile reaches the hull.
The earlier mod margin correction removed the 20% extra length but retained
that forward-query phase.

The movement job uses a different direction: initial aim velocity **plus the
firing ship's velocity**. Native firing points the visual transform at the aim,
so a moving launcher can produce a world trajectory different from the query.
Manager projectile-controller updates also precede ship-controller updates;
the movement job finishes in `ProjectileJobContainer.LateUpdate`. No explicit
native physics-transform synchronization was found on this inspected path.
These defects explain a mechanism for early/off-path contacts; the screenshot
alone does not establish which defect dominated that particular shot.

Native `FilterRaycastsForHits` checks faction and destruction state but does
not require membership in registered damage geometry or explicitly reject
triggers. A sensor or selection volume must not become a physical damage hit.
Finally, fitted component boxes are still AABBs: their empty corners must not
be accepted as if they contained visible hull material.

The prior mod refresh assigned every fitted collider's center and size each
frame, polled every baked renderer for replacement, rewrote beam width curves
while rendering, and repeatedly assigned tactical-particle settings. These
operations are statically confirmed, redundant contributors. Their actual
share of the reported slowdown has not been measured.

Existing human-hull `maxRelativeBoundsError=99` readings are not proof of a
whole hull enlarged 100x. Very small authored helper meshes combined with a
minimum box dimension can dominate that ratio. This build lowers the artificial
minimum, excludes sub-0.1mm helpers, and also reports absolute world error.

## Predetermined hits and near misses

No predetermined hit roll, target snap or forced near-miss admission was found
in the audited **live ballistic** damage branch. `ProjectileWeapon` creates
initial firing data, projectile state adds launcher velocity, and the native
ballistic job advances the transform by constant world velocity times elapsed
time. Ballistic damage follows a filtered physics contact.

Direct-fire expected-damage estimates coordinate target assignments; they do
not apply ballistic damage. Threat spheres serve targeting/point-defense
prediction. They do not admit the completed-step contacts below. No such
prediction code was removed on speculation. Guided missiles, beam damage,
explosive damage and strategic auto-resolve are distinct behavior.

## One flight calculation, one completed path

`BallisticContactRuntime` does **not** integrate velocity or predict a second
projectile position. The native movement job remains the sole flight
integrator, updating the same transform used to render the body. At fire/reuse,
the mod records its initial position and aligns its visual axis with actual
world velocity. Positions are retained relative to the common parent to tolerate
combat-origin rebasing.

The normal pre-job ballistic `UpdateController` call is suppressed while the
feature is active. After the native container completes its job, the mod invokes
the native controller against the recorded previous and current rendered
positions. Collision uses only that completed segment, its actual direction
and its actual length. There is no forward extension, target snapping,
probability-based admission or parallel flight integration. Paused/lifecycle
checks remain native; a consumed segment advances the stored position.

A single physics-transform synchronization precedes queries for each resolving
container pass. Native raycast calls are guardedly replaced at the exact IL
site; native damage construction/application, contact placement, impact,
maximum range and destruction behavior remain in place. Reflection uses actual
flight direction rather than the reversed vector from the overshot endpoint.
The feature-disabled path retains native query behavior.

## Contact geometry

Only active registered damage colliders are admitted; triggers, hidden parts,
friendly bodies and destroyed targets are excluded. Simple fitted boxes remain
the broad phase. A narrow overlap capsule gathers candidates even when a
segment starts inside a component box; its 1e-7-unit radius is broad-phase-only,
not a damage tolerance.

For fitted static renderers, a shared local triangle BVH tests the actual visible
mesh, transformed by the renderer's current matrix. This rejects empty box
corners and keeps articulated parts aligned without resizing/cooking colliders.
Local ray direction is deliberately not renormalized, preserving the world
segment parameter under nonuniform/mirrored transforms. Skinned candidates
can bake a query snapshot once per queried frame; they are not rescaled.
Registered primitive projectiles retain their fitted primitive fallback.

This implementation is a **travelled centerline** mesh test, not finite-radius
swept-body collision. A grazing body whose centerline misses can still miss.
Targets use their current completed-frame pose; this is not exact continuous
collision for rapidly translating/rotating targets between poses. Skinned broad
bounds remain their initialized authored envelope. Meshes assigned after
initialization require an explicit lifecycle rebind; per-frame replacement
polling has been removed. No-solid-mesh fallback retains native geometry with
a warning. These are explicit limits, not proof of perfect collision for every
asset and every relative motion.

High-speed display sampling can also make the last visible projectile frame
precede impact. Native impact/destruction remains immediate after a contact;
this build does not hold the projectile on screen for an artificial extra frame.

## Initialization-only scale and performance changes

- Mesh baking and fitted box center/size setup occur during creation/combat
  preparation or projectile reuse. The collider component's second-frame probe
  reads bounds once and disables itself. Renderer baking similarly stops after
  its readback. Articulated transforms continue animating normally.
- Beam curves and aperture width are configured at fire/reuse. Camera rendering
  checks eligibility without rewriting curves or recalculating lens width.
- Projectile tactical visibility and pixel settings are assigned only when the
  camera crosses the threshold or screen height changes. Hidden projectile
  components disable themselves until reuse.
- Scale remains a per-battle settings snapshot. Saving settings during combat
  no longer refreshes gunpowder art on already baked battle instances.
- BVHs build lazily and cache per shared mesh. Broad queries reuse growing
  buffers without silently truncating candidates. Active flights use reusable
  work lists and constant-time removal. No Unity mesh collider cooking is added.

A bounded probe emits `Combat contact performance:` every five seconds for
its first twelve intervals. It reports FPS, contact-stage milliseconds/frame,
BVH-build and physics-sync subcosts, configurations, query/candidate/contact
counts and triangle/node visits. Build/sync cost is **included within** contact
cost; do not sum those values. GC0 is the process's cumulative collection count.
The first sixteen verified contacts log start/end/contact coordinates and
part names. These diagnostics support measurement; no performance multiplier
or stable-baseline claim follows from removing redundant setters alone.

## Automated validation and deployment

Normal `tools/deploy.ps1` performs the build, process-closed assertions, focused
native/IL validation, formula assertions, package verification and installation.
The full result is retained in [the deployment log](battle-scale/deployment-0.10.15.txt)
and [installed-file manifest](battle-scale/deployment-manifest-0.10.15.json).
Both process-closed assertions passed. All 52 installed files match the authored
package at readback; DLL SHA256 is
`5317A25EAB75D03A8C6A264F3A37056726322AA7E6B9623E7267248E21261025`.
The archive retains twelve pre-existing omissions (eleven older template/text
files and the PDB); its forty present package files match. The installed package
is complete; the archive omissions are enumerated in the manifest.


Coverage includes 211 contact cases: future-segment rejection, strict endpoint,
empty broad-box corner, nearest contact, reversed/nonunit direction,
initial-aim divergence and branched BVH traversal against an independent
analytic plane/triangle result. IL guards require one completed-step cast and
one corrected reflection. Regression checks reject recurring collider, mesh
and beam setters in their frame/render callbacks. The general suite retains
1,219 formula assertions and 384 aperture comparisons. All 41 explicit battle
hook classes resolve; Unity-host binding and actual scene behavior remain
separate from headless verification. The implementation matrix maps 253 Harmony
classes across 119 feature rows.

## Manual acceptance still required

1. Restart and create the same skirmish with the same settings, ship count,
   camera and simulation speed. Compare performance to the earlier build and
   committed baseline; retain fresh logs for both. Separate cold BVH construction
   from later intervals and check collider configurations stop increasing after
   objects are prepared.
2. Track rounds from stationary and moving launchers, from side/oblique angles.
   Verify visible impacts are on hulls and empty box corners remain misses.
   Check first contact traces: contactAlong must lie within the completed step.
3. Test station modules, moving/rotating ships, radiators, turrets, missiles as
   damage targets and point defense. Test cinematic on/off and ship/projectile
   settings independently. Check pooled rounds and exit/re-enter battle.
4. Verify no recurring invisible hulls, clipped close view, oversize beams,
   stale collider geometry, startup errors or severe frame degradation.

Until those runtime results exist this remains an **experimental testing
build**, not an accepted stable baseline. No gameplay acceptance or measured
performance recovery is claimed by this report.

## Subsequent contact-stall correction

[0.10.16](projectile-contact-stall-2026-10-30.md) supersedes lazy contact-time tree
construction and per-queried-frame skinned topology reconstruction. Shared
static trees now prepare at setup; skinned trees refit in place. It also corrects
the evidentiary limit of the tiny-helper explanation for bounds error 99 and adds
post-sync/per-part probes. The single native flight calculation remains.
