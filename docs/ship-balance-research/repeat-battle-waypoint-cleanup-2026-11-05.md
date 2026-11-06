# Repeat-battle waypoint cleanup

Subsequent acceptance: the user confirms the current 0.11.9 version is stable
and performant and requests a checkpoint. [Acceptance record](battle-scale/acceptance-0.11.9-2026-11-05.md).
The test limits below describe the evidence available when the fix was deployed.

2026-11-05. The user confirms excellent 0.11.8 performance and working missiles,
but reports a crash during second-battle setup. Camera-follow changes are
explicitly deferred.

[Captured exception](battle-scale/repeat-battle-crash-2026-11-05.json): formation
UI dropdown initialization calls ReinitializeWaypoints, CleanUpWaypoints and
EndInputHandling. The static active waypoint controller then calls
ClearGizmoVisuals and dereferences a missing/destroyed yaw gizmo. This is a
managed waypoint UI failure, not a collision-buffer access violation.

Native waypoint controllers retain camera/model event subscriptions. Native
skirmish teardown destroys containers, while CombatShipController.OnDestroy
only disables path drawing. WaypointNavigationController also retains a static
active input controller and placement visual. These lifetimes explain how a
later formation cleanup can reach a visual from a previous battle.

## Plan

- Track live navigators at construction and retire them at cleanup/battle
  boundaries. Unsubscribe their render and camera/model/visual callbacks before
  releasing controller references.
- Make teardown idempotent and tolerate already-destroyed Unity visuals. Do not
  call a GameObjectDictionary getter that recreates a destroyed container.
- Clear discarded static input/placement owners. Preserve ownership when another
  live navigator remains active during an individual navigator's cleanup.
- Guard stale input ownership at native EndInputHandling without suppressing
  unrelated exceptions or changing live camera-follow/hover behavior.
- Validate live and destroyed visuals, repeated cleanup, callback removal and
  a second synthetic battle in the Unity host. Run normal deployment checks.

The native camera Update returns early when waypoint input blocks camera motion,
including hover. That is a plausible explanation of the separately reported
follow pause, but this candidate does not change that policy.

## Implementation and verification: 0.11.9

Deployed through the normal build/verification/process-guard flow. The
[deployment log](battle-scale/deployment-0.11.9.txt) and
[installed manifest](battle-scale/deployment-manifest-0.11.9.json) record the result.
All 58 authored files are verified after testing, with only equivalent numeric
XML reserialization permitted for Settings.xml. The native collision DLL hash is
identical to 0.11.8; the accepted missile/performance implementation is retained.

Waypoint navigators are registered once at construction. Their cleanup now
removes render and controller camera/model/visual subscriptions, releases an
input owner only when it belongs to the controller being destroyed, clears the
controller collection and deactivates the existing container. Weak retirement
state makes repeated cleanup harmless without retaining retired navigators.
Battle entry/exit resets discarded static owners. Cleanup checks the backing
container reference rather than invoking its recreating GameObject getter.

A stale-input guard prevents the reported EndInputHandling path from calling
into dead or partially destroyed gizmos. This is not a catch-and-ignore wrapper:
unrelated exceptions remain visible. Live input ownership and camera-follow
policy are preserved.

Standalone CLR validators resolve the fields/targets; Unity-specific methods
require host binding. In the actual Unity player, all patches loaded and
[37 host checks passed](battle-scale/waypoint-lifetime-host-0.11.9.txt) across three
synthetic battle cycles. Cases include valid ownership, destroyed yaw gizmos,
already-destroyed visual hierarchies, repeated native cleanup calls, event
subscription removal, preservation of unrelated camera subscribers and no
resurrection of destroyed containers. Existing collision tests and the
[headless workload](battle-scale/headless-contact-0.11.9.json) also passed.

The headless process exited with code 0. Actual first-battle exit followed by a
second-battle formation setup remains the manual acceptance case. Camera hover
follow behavior is deliberately deferred, as requested. No commit was made.
