# TI 1.0.53b audit and hull mass tooltip

Date: 2026-09-23. Status: fixed, automatically verified, and deployed.
The user confirmed successful manual testing on 2026-09-23.

## Plan

1. Trace the designer wet-mass breakdown and replace its base-hull mass reads
   with the existing appearance-aware empty-hull calculation. Keep the native
   localization and percentage calculation, and the existing feature toggle.
2. Validate both mass and percentage reads against the installed method and
   exercise the Harmony patch with the normal release checks.
3. Compare installed 53B with preserved pre-update IL and recorded binary
   hashes. Inspect the release's changed scenario values for overlap with EEO's
   data overrides and compiled patches. Record limits of the historical data.
4. Update current compatibility documentation, retaining historical reports and
   the numeric game-version contract where the loader requires it.
5. Build, validate and deploy through `tools/deploy.ps1`, then record results
   and leave the build ready for manual variant-switching tests.

The user confirmed the preceding technology-tree changes work as intended.

## Hull mass correction

`FleetsScreenController.DesignerMassBreakdown(ship)` read
`ship.hullTemplate.buildMass_tons()` twice: once for hull tons and once for
its share of wet mass. The template mass matches appearance zero. Actual ship
mass and the art-panel overlay already use EEO's appearance-specific mass.

`HullVariantMassTooltipPatch` now redirects exactly those two expressions to
`HullVariantEmptyMassFeature.EmptyHullMass_tons(ship)`. It uses the ship passed
to the tooltip, so it follows the selected appearance without caching a second
value. Native localization, wet-mass denominator, rounding, and all other
breakdown rows remain intact. The shared helper retains the existing disabled,
alien, and missing-catalog fallbacks. This is a display correction; the ship's
physical mass calculation has not changed.

The transpiler checks the full expected hull-read expression and requires
exactly two replacements. Release validation checks both replacements, rejects
remaining raw hull reads, and asks Harmony to emit the patched native method.
The implementation matrix records the patch on the existing hull-balance row.

Manual test: select a human hull with different appearance masses, hover wet
mass, then change appearances in both directions. Hull tons should match the
art-panel empty-hull mass; the percentage should equal that mass divided by
wet mass, with native rounding. Appearance zero and disabled hull scaling
should retain their previous values.

## Binary audit

Steam's installed public build is **25216249**, corresponding to **1.0.53b**.
The game DLL did change:

| Evidence | SHA-256 |
| --- | --- |
| Pre-B game DLL recorded in the [laser audit](../ship-balance-research/laser-armor-penetration-and-jitter.md), written 2026-08-31 (53a period) | `FF7916C2085DDBAFA5ACF1E8EA185D37E629096752BE388BA6FA1F627F027BB5` |
| Installed 53B `Assembly-CSharp.dll`, written 2026-09-11 | `4A4B9AAE4154E444E9727204205D2D42AE8ED9E1C5F92CDC1280074A259D8350` |

A complete IL disassembly preserved on 2026-08-24 provides the original 53
code baseline. Comparing method signatures and bodies after removing comments
and blank lines found **37,571 old methods, 37,570 current methods, 37,567
unchanged methods, three changed methods, and one removed compiler predicate**.
No method was added. The comparison groups overloads and compiler-generated
methods under their disassembler names while preserving their order.

| Native method | Change from original 53 through 53B | EEO overlap |
| --- | --- | --- |
| `FactionGoal_InvadeEarth.ShouldWaitToInvade` | Removes bypasses for an extant Alien Administration and a human faction having overthrown it. Alien invasion readiness now respects quietness until it falls below 0.2. | No EEO patch on this method or the removed predicate. |
| `TIFactionState.NewCampaign` | Removes the invasion-focused campaign condition from initial invasion-goal creation; the quiet-campaign condition remains. | No EEO patch on this method. |
| `TIRegionAlienFacilityState.ResolveAssault` | Adds null-faction guards around milestones, rewards, and hate when an army assaults a facility. This is the 53a crash fix. | No EEO patch on this method. |
| Compiler predicate for `ShouldWaitToInvade` | Removes its `OverthrewAlienNation` test, no longer needed by the first change. | No EEO reference. |

This agrees with the developer's [53B announcement](https://steamstore-a.akamaihd.net/news/externalpost/steam_community_announcements/1843481262695523)
and [53a announcement](https://steamstore-a.akamaihd.net/news/externalpost/steam_community_announcements/1842212951311450),
retrieved through Steam's official news API. Those announcements describe
delayed DLC invasion timing and the factionless-army crash respectively.
The complete release suite also binds and checks EEO against the **current**
assembly, rather than assuming unchanged APIs from a version label.

Reproduction evidence: historical IL SHA-256
`59B6D1215689A7E37567EEB8A15435C127A4C3BCB4BE584F592828F482CD144C`;
current IL SHA-256
`AA6CFCD912742BBD5ABA9340ABDDC409AFC85708AA781B9B8E8E691BA7649FAD`.
Full decompilations remain temporary analysis products. The checked-in
[baseline inventory](ti-1.0.53b-baseline.json) records hashes of all 148 installed
managed DLLs, including the mod-loader dependencies. Only Assembly-CSharp has a
September write timestamp in this installed managed set; without earlier hashes
for all other DLLs, that is not proof they are byte-identical to original 53.

## Template and override audit

The current source inventory covers **59 base JSON files and 34 Dark Skies
scenario JSON files**. Two have September update timestamps:

| File / record | Current relevant values | EEO interaction |
| --- | --- | --- |
| `2003_Scenario/Templates/TIStartTimeTemplate.json` | `alienQuietDuration_years = 22.5` (the announcement's 50% increase implies 15 previously); setup duration 20, start/end income 0.15/1.5, progression modifier 0.75, starting progression -5 | EEO supplies only `ModernDayStart` and `2026Start` records. It does not overwrite the DLC start record or these fields. |
| `Broken_Earth_Scenario/Templates/TIGlobalConfig.json` | `extraYearsToDelayAlienInvasion_C/N/V/B = 16/9/4/0`; wormhole setup speeds 1.50/1.00/0.50/0.01; armies lost before buildup 0 | None of these fields is present in EEO's global override or referenced by its compiled patches. |

The Broken Earth file contains many scenario values; its timestamp alone does
not identify which individual fields changed in B. The table therefore reports
current invasion-related values, without inventing historical differences.
EEO's six authored global fields are control-point maintenance freebies,
councilor organization cap, two crew consumption rates, and two probe payload
terms. They do not intersect the listed invasion settings.

Beyond source inspection, the installed `JsonController.LoadJson` and
`CombineJson` were used to apply EEO to each scenario's start/global source
(falling back to the base file when the scenario has no override). Every
unspecified property in every existing record survived:

| Scenario | Global fields preserved | Start-record fields preserved |
| --- | ---: | ---: |
| 2003 | 95 | 43 |
| Broken Earth | 126 | 45 |

**Result:** no direct code conflict or overwrite of the reviewed 53B invasion
values was found, so no gameplay balance values were changed for compatibility.
EEO's broader economic and ship changes naturally still affect campaign balance.

**Historical limit:** no complete pre-B JSON snapshot was available. File
timestamps and release notes identify the reviewed change candidates but cannot
prove an exhaustive field-by-field historical diff. The saved baseline now
includes the pre-mod values and presence flags for every property explicitly
addressed by EEO across **1,989 matching base/scenario records**, in addition to
the 241 source hashes. Future audits can compare that baseline with the new game
and EEO to identify upstream changes to properties we override.

## Documentation and deployment

Current README, mod description, assembly description, startup log, and release
verification output now identify 1.0.53b. Historical reports retain their original
version labels. The numeric compatibility floor in `ModInfo.GameVersion` stays
`1.0.53`, as does the package filename; neither should be mistaken for a binary
identity check.

The final normal `tools/deploy.ps1` run completed with no skipped verification:

- 1,172 formula assertions passed.
- The implementation matrix covered 101 rows, 24 settings groups, and 182
  Harmony patches.
- All focused IL/Harmony checks passed, including both hull-tooltip reads.
- All technology merge, scenario, ship, hab, and other release checks passed.
- Both game-closed checks passed, and all 46 deployed files matched their
  packaged hashes.
- Deployed EEO DLL SHA-256:
  `46190153CE476D730B06AA952D05677BE0D6ACBE2095275E1DBCB5B185D5662F`.
- Package: `artifacts/TIEconomyMod-0.9.7-ti1.0.53.zip`.

The initial attempt placed the UI patch in a source file also compiled by the
dependency-free formula harness. That build correctly failed before deployment;
the patch was moved to the existing ship UI patch file and the full normal flow
rerun successfully. The user subsequently confirmed the deployed fix works.
