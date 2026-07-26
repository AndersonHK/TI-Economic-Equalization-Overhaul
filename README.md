# TI Economic Equalization Overhaul

This branch is a selective Terra Invicta 1.0.32 compatibility and balance port. It preserves the intended economic behavior of the starting mod, adopts the maintained mod's current priority API and package structure, and adds configurable technology, abundance, inequality, threshold, and tooltip systems.

## Implemented in this slice

- Investment Points, control-point cost, army upkeep, research, Knowledge, Military, and Spoils money retain the starting mod's intended behavior.
- The former Knowledge democracy effect now patches Government.
- The former Military unrest effect now patches Oppression; other Oppression behavior remains vanilla.
- Economy growth uses a total-GDP formula, a smooth core-economic region curve, compounded global-technology weights, and saturating resource/land curves.
- Economy, Welfare, and Spoils inequality changes use smooth directional scaling, with every resource effect measured against national GDP.
- Region thresholds default to vanilla and can be configured.
- Vanilla tooltips are preserved and receive live EEO sections.
- A global toggle and per-feature toggles return execution to vanilla when disabled.

Environment balance, Unity, other Spoils behavior, event-driven inequality, and deliberate TI 1.0.39 adaptation are deferred. The authoritative feature-by-feature comparison is [docs/current-implementation-matrix.xlsx](docs/current-implementation-matrix.xlsx).

## Configuration

Normal formula settings are stored by Unity Mod Manager and exposed in grouped UI sections. The reset button restores all defaults.

Technology weights live in [TIEconomyMod/ModFiles/Config/economy-tech-weights.csv](TIEconomyMod/ModFiles/Config/economy-tech-weights.csv). Its columns are:

- `tech_id`: exact global technology data name
- `enabled`: `true` or `false`
- `percent`: contribution to the compounded Economy multiplier
- `rationale`: short semantic classification

Unknown IDs are logged and skipped. Duplicate IDs fail mod validation. Changes require a game restart.

## Build and verification

The project targets .NET Framework 4.8. No game or mod-loader DLL is copied into the repository. `tools/build.ps1` resolves the selected Terra Invicta installation from `TI_TARGET_MANAGED_DIR` or Steam's library configuration, then uses the matched Unity Mod Manager and Harmony pair found in that installation.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\build.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\verify.ps1
```

Verification:

- rebuilds with warnings treated as errors;
- runs dependency-free formula tests;
- validates the implementation matrix against every settings group and Harmony patch;
- validates the manifest and package paths;
- confirms the packaged DLL was just rebuilt;
- creates `artifacts/TIEconomyMod-0.3.0-ti1.0.32.zip`.

The currently installed TI 1.0.39 assemblies are used as an informational forward-compilation check. This slice intentionally keeps TI 1.0.32 behavior and metadata.

## Install for a smoke test

Extract the archive so the installed mod directory contains:

```text
TIEconomyMod/
  Assembly/TIEconomyMod.dll
  Config/economy-tech-weights.csv
  ModInfo.json
```

Test poor resource-rich and resource-poor nations, land-abundant and dense nations, unstable nations, early/mid/late technology saves, all three bounded inequality priorities, Government/Knowledge/Military/Oppression completions, and feature toggles with the expanded tooltips.
