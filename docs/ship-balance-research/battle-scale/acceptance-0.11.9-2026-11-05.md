# Accepted stable 0.11.9 checkpoint

2026-11-05. The user confirms: "The current version is stable and performant"
and requests a commit and push before further read-only performance inspection.

This accepts the deployed 0.11.9 collision-performance and waypoint-lifecycle
candidate. The [read-only identity manifest](acceptance-manifest-0.11.9-2026-11-05.json)
checks all 58 source/installed/archive files, allowing equivalent Settings.xml
reserialization. The open game reports 0.11.9. No rebuild or deployment was
performed for this checkpoint.

The prior normal deployment passed 45 validators and 1,887 formula assertions,
including 8,540 native ABI/contact cases. Unity-host evidence includes 1,764
contact checks, compatibility lifecycle checks and 37 waypoint-lifetime checks
across three synthetic cycles. User acceptance is distinct from those automated
results and does not make the failed 0.11.7 candidate a successful baseline.

The checkpoint also retains the accumulated Mission Control progression,
habitat-sector expansion and related research documents. Those proposals remain
unimplemented. Camera-follow changes and the separate friendly-collision/
avoidance policy remain deferred.

Further inspection of the running battle follows this checkpoint. Measurements
and possible optimizations must be reported separately from the accepted runtime;
no additional performance change is implied by this acceptance.
