# TI 1.0.53 hab-upgrade affordability

Status: implemented, automatically verified, and deployed in EEO 0.9.6 on
2026-08-24. Manual in-game confirmation is pending.

## Reported regression

The installed-module panel can disable **Upgrade** even when the faction can
pay for a mixed space-stockpile and Earth/Boost delivery. The reported lunar
Settlement Mining Complex upgrade had an affordable substituted route, while
the single-upgrade button remained grey. **Upgrade All** did consider Boost.

## Diagnosis

TI 1.0.53 selects upgrade costs independently in several UI and execution
paths:

- `HabitatsScreenController.UpdateModulePreviewText` displays and tests only
  `CostFromSpace(..., isUpgrade: true, substituteBoost: false)` for the
  installed-module **Upgrade** button;
- `HabitatsScreenController.GetSpaceCost` retries with
  `substituteBoost: true` before starting an individual build action;
- `HabitatsScreenController.UpgradeAllModulesSelected` repeats the same
  unsubstituted-then-substituted selection loop for bulk execution;
- the bulk confirmation panels separately convert aggregate shortfalls to
  Money and Boost.

The button therefore rejects a route that the subsequent placement path would
accept. Maintaining the individual and bulk selection loops independently also
allows them to drift again.

## Implementation

1. Add one EEO upgrade-space-cost resolver. It prefers an affordable native
   stockpile quote, otherwise an affordable Boost-substituted quote, and falls
   back to the native quote when neither is affordable so the UI still shows
   the actual shortage.
2. Route both upgrade quotes in `UpdateModulePreviewText`, the final
   `GetSpaceCost` selection, and both bulk-execution quote sites through that
   resolver. Leave ordinary new-module baseline/alternative rows unchanged.
3. Add a focused validator that requires exactly two preview rewrites, exactly
   two bulk-execution rewrites, the individual-placement prefix, and successful
   placement-prefix Harmony emission against the installed TI 1.0.53 assembly.
   The two UI methods contain Unity internal calls that the desktop .NET
   Framework validator cannot detour safely, so their exact transpiler output
   is inspected without installing the detour; Unity/Mono applies those
   guarded rewrites at game startup.
4. Run the normal `tools\deploy.ps1` flow without skipped verification.

The implementation is in `TIEconomyMod/Patches/HabUpgradeCostPatches.cs`.
`HabUpgradeCostResolver` is the only quote-selection policy. Guarded rewrites
route the two installed-module preview quotes and the two bulk-execution quotes
through it; the individual placement helper is replaced as one unit. The
ordinary new-module preview rows remain native.

## Automatic verification and deployment

The final `tools\deploy.ps1` run completed without skipped verification:

- 179 Harmony patch classes are covered by 101 implementation-matrix rows;
- the focused validator found exactly two preview rewrites, one placement
  replacement, and two bulk-execution rewrites;
- all 1,172 formula assertions and the remaining TI 1.0.53 compatibility,
  data, image, and package checks passed;
- `TIEconomyMod-0.9.6-ti1.0.53.zip` was created;
- 46 files were deployed to the enabled mod directory;
- the deployed DLL SHA-256 is
  `C9D253F3DEEF0621BCF5DF2BE6A64EF62065F84EA1E71EE3C43F531D0C29D522`.

## Manual test

After deployment, reopen the affected lunar base and select the Outpost Mining
Complex:

1. With enough direct space resources, **Upgrade** is enabled and shows the
   unsubstituted space quote.
2. With a replaceable-resource shortfall but enough Money and Boost,
   **Upgrade** is enabled and shows the substituted quote.
3. With neither route affordable, **Upgrade** remains disabled and shows the
   native shortage.
4. **Upgrade All** and **Upgrade All of Type** still offer and execute the same
   substituted route selection as individual upgrades.
