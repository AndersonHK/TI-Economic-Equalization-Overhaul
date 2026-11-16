# Terra Invicta 1.0.65 port

EEO 0.11.14 targets Steam public build 26435642. The previous EEO release
(0.11.13) targeted 1.0.53b, Steam build 25216249. The
[official release notes](https://store.steampowered.com/news/app/1176470/view/655997268258718160)
highlight the tech tree redesign, late-game AI performance, fixes, and Dark
Skies updates. The port uses the installed files as the compatibility authority.

## Installed game comparison

The [1.0.65 baseline](ti-1.0.65-baseline.json) records 248 files: 148 managed
DLLs, 59 base templates, 36 Dark Skies templates, and five asset bundles. It
snapshots 3,725 fields that EEO overrides across 2,200 stock records.

| Area | Difference from the [1.0.53b baseline](ti-1.0.53b-baseline.json) |
| --- | --- |
| Managed assemblies | Only `Assembly-CSharp.dll` changed among 148 DLLs. The Harmony and Unity Mod Manager dependencies matched. |
| Base templates | Six changed: localization, meta, narrative events, power plants, projects, and space bodies. |
| Millennium | Ten changed templates and a new scenario global config. |
| Broken Earth | Nine changed templates and a new scenario mission file. |
| Audited asset bundles | `ships` and `ships_prm` changed. `habmodules`, `shipweapons`, and `spacecombat` matched the prior pins. |

The full IL disassemblies contain 37,570 methods in 1.0.53b and 37,780 in
1.0.65. After stripping comments and normalizing common compiler-generated
ordinals, the comparison flagged 867 changed, 618 added, and 423 removed
method-name groups, with overloads grouped together. These are review
candidates; compiler output can change without a gameplay change. The review
checked patch targets, including reflection-only targets, caller contracts,
and native behavior inside methods EEO replaces.

## Port changes and compatibility decisions

| Area | Result |
| --- | --- |
| Refuelling station goals | EEO now changes only the capacity expression, preserving the native resupply check and its new `checkInactives` argument. Tests exercise partial and full T1–T3 layouts and a full station refused by the resupply check. |
| Habitat construction charges | The native space construction path now permits Earth Boost substitution for active human factions only. EEO's quote follows that rule. |
| Ambient hab hazards | Retargeted the regenerated selector that feeds the monthly weighted event draw. The separate dictionary-building selector is unaffected. |
| Research, transfers, and AI | The research panel still receives the independent research display. Trajectory enum renumbering does not change EEO's enum-based plane-change dispatch. Changes to native ship candidate selection, hab caches, and bulk control point handling stay in the native method bodies where EEO amends them. |
| Template merging | Adapted validation to the native `CombineJson` signature, which now takes the template name. Verified that EEO's array replacements affect the authored fields and preserve omitted stock fields and unrelated records. Tests cover the base tech, hull, armor, and space body data, both Dark Skies configs and starts, and the new Broken Earth Grant Alien Control mission. |
| Millennium research | The old baseline covered two changed upstream fields in records EEO overrides: Millennium Advanced Neural Networks went from 4,000 to 5,000 RP and dropped Photonic Computing. EEO retains its authored 2,500 RP and empty prerequisite list, while preserving the Millennium tag and other upstream fields. EEO does not override the new DLC invasion timing fields. |
| Art and collision | Re-extraction matched all 438 archived mesh payload entries byte for byte. The 506 source records (503 runtime fingerprints) match after changing the assembly hash. All 40 native Mothership turret anchors matched, as did Mothership and four Titan vertex/index arrays. All 64 hull report rows, both runtime tables, and 66 images matched after regeneration. |

The [asset pins](ti-1.0.65-asset-sources.json) retain the prior Mothership
fixture's provenance and bind the checks to the verified current bundle.
The [hull report](../ship-balance-research/hull-utility-slot-volume-report.md)
records both current ships bundle hashes. The old baseline did not capture
every asset, localization file, or untouched template field. Historical
field comparisons are limited to the data recorded there.

## Verification and deployment

On November 16, 2026, `tools/deploy.ps1 -ValidationThreads 16` passed all 45
validator jobs and the release package checks, including 1,896 formula
assertions and 8,540 native collision checks. In a separate standalone Harmony
probe, 262 patch classes emitted and 74 reached Unity-only internal calls.
No other failure remained in that probe. The deploy script asserted the game
was closed and verified all 58 deployed files against the package by SHA-256.

- Archive: `artifacts/TIEconomyMod-0.11.14-ti1.0.65.zip`
- Installed: `Mods/Enabled/Economic Equalization Overhaul`
- Managed mod SHA-256: `7F75E51A0BA08FC637B822DEE962AC5EC257CA34C295BB9DC649DF6450C95A08`

The user reported that the deployed build appeared to work and requested a
commit. That report did not specify which scenarios or combat checks were
exercised. The focused smoke-test checklist is:

1. Confirm TI 1.0.65 and EEO 0.11.14 at startup, then load a copy of an existing
   campaign and advance through an AI planning period.
2. Open the redesigned tech tree and a research panel. Check EEO prices,
   prerequisite links, and independent research progress.
3. Start Millennium and Broken Earth with Dark Skies enabled. Check that both
   initialize and that a hab construction or upgrade quote is available.
4. Quote a same-altitude LEO station rendezvous. Check Mothership and Titan
   designs and play two successive skirmishes to cover mounts, beams, hits,
   and waypoint cleanup.
