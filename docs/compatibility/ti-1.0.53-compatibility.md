# Terra Invicta 1.0.53 compatibility

Status: fixed, automatically verified, and deployed on 2026-08-24. Manual
in-game confirmation is pending.

## Reported regression

After Terra Invicta updated from 1.0.51 to 1.0.53, compiled EEO behavior
disappeared. The reported save displayed vanilla-like Control Point and Mission
Control values, including `318 / 219` Control Points and `9 / 28` Mission
Control.

## Diagnosis

The Unity Mod Manager log is authoritative for this failure. EEO 0.9.4 loads
its configuration catalogs, then Harmony aborts `PatchAll` while applying
`HostileClaimCompatibilityPatch`:

```text
Parameter "region" not found in method
TINationState.HostileClaimDueToDemocracy(TINationState testNation)
```

TI 1.0.51 supplied the claimed `TIRegionState region` directly. TI 1.0.53 now
supplies only the owning `TINationState testNation`. EEO's loader correctly
rolls back the partially installed Harmony set and reports that all compiled
features are disabled. Control Point capacity/cost and mine Mission Control are
therefore symptoms of the same all-or-nothing initialization failure, not two
independent formula regressions. Template overrides still merge because the
game processes them separately from EEO's assembly.

The previous target-IL check did not detect the change because it asserted only
that the method still read the vanilla Government threshold. The focused mine
Mission Control validator also loaded an incomplete set of Unity dependencies
in-process, preventing a reliable binding check against the updated game.

## Implementation plan

1. Retarget `HostileClaimCompatibilityPatch` to the 1.0.53 `testNation`
   contract. Calculate the same directional national harmonization score and
   use the ordinary inclusive threshold for this regionless compatibility
   hook. Exact per-region historical thresholds remain authoritative in
   `ClaimWillBeHostilePatch` and the external-claim presentation helpers.
2. Add a focused Harmony binding validator for the three claim-harmonization
   decision and explanation hooks. It must emit them together against the
   installed assembly so an invalid parameter or missing target fails
   deployment even when a narrower IL assertion still passes. The remaining
   patch families retain their existing focused emit validators; standalone
   .NET cannot emit the complete game patch set because Unity internal-call
   methods are patchable only inside the Unity runtime.
3. Update the focused target guard to require the 1.0.53
   `HostileClaimDueToDemocracy(TINationState testNation)` signature.
4. Retarget release metadata and current documentation to TI 1.0.53 and bump
   EEO from 0.9.4 to 0.9.5. Historical 1.0.51 records remain unchanged.
5. Run the normal `tools\deploy.ps1` path without skipping verification.

The updated Dark Skies ship bundle also invalidated the documented hull-report
source hash. Regeneration exposed a tooling bottleneck: the report serialized
every Unity mesh to OBJ text and reparsed it before rendering, then processed
all appearances serially. Before rerunning deployment, replace that round trip
with direct Unity mesh-buffer extraction and move independent thumbnail CPU
work to an eight-process pool. The generated tables, runtime CSVs, and images
must remain deterministic and pass the existing hull-report validator.
Direct extraction intentionally retains the decoded vertex values beyond the
OBJ exporter's nine significant decimal digits. The resulting thumbnail pixel
changes are visually immaterial but preserve the more accurate source geometry.

## Verification and manual test

Automatic acceptance requires the complete release suite, including the new
claim-harmonization bind, all focused Harmony/IL checks, formula tests,
packaging, and mirrored deployment.

After deployment, launch TI and confirm:

1. Unity Mod Manager reports EEO 0.9.5 as loaded, with no initialization
   exception.
2. The affected save no longer shows `318 / 219`; Control Point capacity uses
   the configured percentage project-bonus conversion.
3. Mine Mission Control again charges active mines by tier (`1 / 2 / 3`) and
   the top-bar warning colors change only above 75% and 100% usage.
4. A claimed region and a capital-unification target use the harmonization
   score rather than the retired Government-only hostility test.

## Completed automatic verification

The normal `tools\deploy.ps1` pipeline completed without skipped checks:

- all three national-harmonization hooks bound and emitted against the TI
  1.0.53 nation/region contracts;
- all five mine Mission Control replacements bound and emitted, including the
  `1 / 2 / 3` tier costs and `75% / 100%` warning boundaries;
- the complete implementation matrix found 176 Harmony patches across 100
  rows and 24 settings groups;
- 1,172 formula assertions passed;
- the refreshed hull report validated 28 templates, 64 appearances, both
  runtime catalogs, four source hashes, and 66 PNGs;
- the full-precision, eight-process hull generator completed in 34.34 seconds;
- release verification produced DLL SHA-256
  `3DE7695DC873C102D505A40D8ED64F78D42EDEE9D363F34A972CBE9A36A4D408`;
- `TIEconomyMod-0.9.5-ti1.0.53.zip` was created and 46 files were deployed to
  `Mods\Enabled\Economic Equalization Overhaul`.
