# Skirmish load regression: premature asset-cache initialization

2026-10-29. **0.10.12 is built and deployed.** A fresh-process runtime log now
confirms successful EEO loading and progression through skirmish creation,
formation placement and projectile firing without the reported cache exception.

## Confirmed cause

[The retained 0.10.11 crash log](battle-scale/skirmish-startup-crash-0.10.11.txt)
captures both the original patching error and subsequent skirmish failure.

Player.log first records EEO failing during `Harmony.PatchAll`, specifically
while patching `CombatHabModuleController.DestroyHabModule`. Mono/Harmony
wrapper preparation initialized `AssetCacheManager`; its first static sprite
initializer attempted asset loading before global configuration was ready.
At the logged IL offset `0x000a`, it dereferences
`TemplateManager.global.pathArmyIconBackground`; the preceding getter has
returned a null configuration. The static initializer threw a NullReferenceException.
EEO rolled back its patches and reported Not loaded. Later skirmish visualizer
creation accessed that same failed type and threw TypeInitializationException.

This is a regression introduced by the new station-explosion hook, not evidence
that skirmish geometry or formation spacing caused the failure. Standalone
Windows/.NET Harmony emission did not reproduce Mono's initializer behavior.

## Correction plan

- Remove the patch from the native cache-reading destruction method. Keep its
  class/feature identity but attach a runtime observer from the already-used
  `InitializeForCombat` boundary instead.
- Observe newly spawned station-destruction children during combat and apply
  the existing geometry fitting there. Disarm the observer on combat exit.
- Reject startup Harmony targets which directly reference AssetCacheManager in
  original IL, without reading its values or invoking its static initializer.
- Use normal full validation/deployment, then require a fresh game process for
  the user retest: a failed CLR/Mono type initializer cannot recover in-process.

## Implemented fix

`BattleStationExplosionGeometryPatch` now patches `InitializeForCombat`, a
boundary already used successfully by the geometry work. It attaches
`CombatStationExplosionObserver`, which records current module children and
observes newly spawned children only after destruction is triggered in combat.
The existing explosion fitting is then applied without patching the native
cache-backed destruction method. The observer is disarmed before station
geometry returns to the strategic layer.

The validator checks the actual class's Harmony metadata, rejects the known
bad native method by inspecting its original IL, and scans every explicit
battle-hook target for direct AssetCacheManager member references before
trying Harmony emission. It does not invoke a cache member or read static
field values. This addresses the measured initialization-order failure; it
does not equate Windows/.NET patch emission with Unity/Mono runtime execution.

## Validation and deployment

Normal `tools/deploy.ps1` completed with **41 validators, 1,209 formula assertions
and 51 deployed files**. The matrix still has 116 features and 242 Harmony
patches: the station feature was retargeted, not removed. Thirty battle-hook
classes resolve, with 26 requiring Unity-host emission confirmation.

DLL SHA-256:
`447C23BCBC51274FE5DBF4B4F088ACDF1A4C6734C2E8E9233E10F66276ADDEC1`.
[Deployment log](battle-scale/deployment-0.10.12.txt) and
[installed-file readback](battle-scale/deployment-manifest-0.10.12.json).

A direct UI startup check was attempted using the computer-use skill. Its
`launch_app` call was denied with: `Computer Use was not approved to use terrainvicta`.
No alternate launch route was used to bypass that approval. Subsequently a new
0.10.12 game process appeared and its log recorded successful mod loading,
`Loading SkirmishModeScenario`, creation of Frigate/Corvette combat objects,
formation assignment, geometry probes and magnetic projectile fire. No EEO
initialization failure, AssetCacheManager exception or `Handling Exception`
appears in that retained run. This confirms the loading failure cleared in the
observed run; it is log-based evidence, not automated visual acceptance.
Only documentation/readback followed deployment; no commit/push was requested.

## Runtime evidence and remaining test

[Successful 0.10.12 runtime snapshot](battle-scale/skirmish-startup-success-0.10.12.txt)
preserves the completed loading/firing evidence. The snapshot also retains
existing Frigate/Corvette collider-bounds probes reporting relative error 99;
that separate geometry discrepancy is not explained by this initializer fix
and is not treated as a successful collider-fit test.
The user subsequently confirmed, "It didn't crash."

For further regression tests, launch a fresh process and load a skirmish. Confirm
`Loaded TI Economic Equalization Overhaul 0.10.12`, with no `EEO initialization
failed` or AssetCacheManager initializer exception, and reach the formation
screen. After that, exercise station-module destruction to confirm the observer
still sizes its effects. A poisoned 0.10.11 process cannot be repaired by simply
retrying the battle; restart is required.
