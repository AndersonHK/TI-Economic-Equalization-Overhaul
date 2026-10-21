# Gunpowder render-comparison controls

Working default in 0.10.8: **Bake sizing into mesh copies on; Disable GPU
instancing for gunpowder materials off.** The accepted runtime trace records
this configuration, and the user confirms the cutout is fixed.

The controls are in Unity Mod Manager's EEO settings under **Gunpowder render
comparison (same dimensions; session only)**. They apply immediately to tracked
gunpowder renderers and rearm the short diagnostic trace. Restart resets defaults.

| Bake sizing | Disable instancing | Purpose |
|---|---|---|
| On | Off | Accepted default; original materials and their instancing setting |
| On | On | Isolate GPU instancing while retaining baked geometry |
| Off | Off | Reproduce the failing 0.10.7 transform-based representation |
| Off | On | Isolate instancing in the old transform-based representation |

All four choices preserve canonical dimensions, pivot/muzzle positions and
gameplay statistics. The instancing option uses private material copies;
the source materials are not edited. These are regression controls, not weapon
balance settings. The exact GPU-level failure mechanism remains unproven.

For a future regression, compare one setting at a time on the same paused
scene and camera angle. Preserve `Gunpowder art schema 1`, `Gunpowder material
probe`, `Gunpowder camera probe` and `Gunpowder render comparison` log lines.
The [implementation record](../gunpowder-canonical-art-implementation-2026-10-21.md)
and [archived investigation](../archive/gunpowder-rendering-2026-10-21/README.md)
explain the source and runtime checks.
