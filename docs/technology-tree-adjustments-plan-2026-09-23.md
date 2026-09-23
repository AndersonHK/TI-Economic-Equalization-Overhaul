# Technology-tree adjustments: review plan

Status: implemented and deployed on 2026-09-23 after approval of the
interactive preview's "Requested changes" draft. Manual in-game checks pending.

## Approved implementation scope

Apply exactly the three prices and Neural Networks prerequisite edit in the
table below, plus the new-2026-campaign completion grant. The user accepted the
draft's resulting layout, including its documented differences from the original
anchor targets. The optional broader fusion discounts and separate Future Tech
runtime correction remain proposals outside this implementation. Run normal
deployment with verification, then create a commit with an explanatory message.

## Follow-up analysis and visual review

The detailed [column-placement report](technology-tree/column-placement-analysis-2026-09-23.md)
now explains the exact cost rule and the Future Tech column bookkeeping issue.
The [interactive preview](technology-tree/tech-tree-preview.html) models current
repository data and the requested edits. Its columns are predictions from the
native rules; vertical spacing is schematic. See the
[snapshot index](technology-tree/README.md) for current/proposed images.

The model predicts that the original three price edits plus the Neural Networks
prerequisite removal shift the main fusion chain and Neural Networks one column
left. Magnetic Plasma Confinement remains gated behind High Temperature
Superconductors. Clean Energy and Terawatt also move left, so Proton-Proton's
requested relative position remains unresolved. Future Tech price increases do
not separate their column; the report proposes a narrow UI bookkeeping fix.
An optional additional fusion discount schedule is provided for review, not
approved or implemented. The report's measured predictions supersede the
preliminary layout uncertainty below.

## Requested outcome

Move the fusion progression one UI column earlier, placing Deuterium-Tritium
Fusion directly above Coilguns and Proton-Proton Fusion beside Clean Energy,
before Terawatt Fusion Reactors. Discount the two entry technologies. Move
Advanced Neural Networks one column earlier, remove its prerequisite, reduce
its cost, and complete it at the 2026 start.

Interpret "back" as one column left/earlier. Interpret the fusion progression
as D-T, Nuclear Fusion Methodologies, the five confinement technologies
(Magnetic, Electrostatic, Inertial, Tokamaks, Z-Pinch), D-D, D-He3, Aneutronic,
and Proton-Proton. Treat Terawatt Fusion Reactors and Clean Energy as endpoint
references whose placement is to remain the target for comparison. This scope
interpretation is part of the review, since "all other fusion techs" could also
be read to include Terawatt Fusion Reactors.

## Exact JSON edits

In `TIEconomyMod/ModFiles/TITechTemplate.json`:

| Template | Field | Current | Proposed |
|---|---|---|---|
| `DeuteriumTritiumFusion` | `researchCost` | 50,000 inherited | 40,000 |
| `NuclearFusioninSpace` (Nuclear Fusion Methodologies) | `researchCost` | 50,000 inherited | 40,000 |
| `AdvancedNeuralNetworks` | `researchCost` | 5,000 inherited | 2,500 |
| `AdvancedNeuralNetworks` | `prereqs` | `["PhotonicComputing"]` inherited | `[]` |

40,000 matches installed `HighTemperatureSuperconductors`; 2,500 matches
`MissiontoMars`. At the shipped research multiplier of 2.2, the first two
technologies each change from 110,000 to 88,000 displayed research, and Advanced
Neural Networks changes from 11,000 to 5,500.

In `TIEconomyMod/ModFiles/TIStartTimeTemplate.json`, append
`AdvancedNeuralNetworks` exactly once to `2026Start.globalTechsCompleted`.
Keep the three active research slots intact. Leave the 2022 completion list
intact. The technology definition changes apply across scenarios; the completion
grant applies to new 2026 campaigns. Existing saves do not acquire this grant
merely by changing a scenario template.

Keep existing fusion prerequisite sets, effects, and AI metadata. No additional
fusion cost changes are specified. Completing Advanced Neural Networks makes
its downstream technologies and projects eligible subject to their other
requirements and normal project unlock rules; it does not complete them.

## Layout feasibility and decision

The installed templates expose no authored row/column coordinates. Inspection
of the installed `ResearchScreenController` confirms:

- `BuildTree` uses scenario `techTreeUIStarters` and prerequisite-free roots.
- `PlaceTechsBehindPrereqs` and `PlaceTechsBehindSameTierPrereqs` place nodes
  after their prerequisites.
- The technology-only full tree additionally calls
  `PlaceTechsBehindLowerCosts`, which compares costs with column averages.
- Vertical positions are subsequently calculated from connected technologies
  and adjusted to resolve overlaps.

Consequently, lowering the entry costs cannot be assumed to move every fusion
node exactly one column. Removing Photonic Computing makes Advanced Neural
Networks a root, but its final technology-only column remains subject to cost
placement. A fixed vertical position directly above Coilguns is also not an
ordinary JSON field edit.

Proposed approach: implement the explicit JSON edits after approval, and check
the resulting layout in-game against the requested anchors. Do not change
additional prerequisites or prices merely to manipulate the drawing. If the
automatic layout does not meet the requested positions, prepare a separate,
reviewable UI-layout change that preserves research requirements. Such a change
would require runtime code; it is not included in the JSON-only proposal.
The complete positioning request remains pending until verified or addressed
by that separately reviewed change.

## Implementation and verification sequence after review

1. Apply the JSON edits above, preserving existing unrelated worktree changes.
2. Update `tools/verify.ps1`: its exact override allowlist, fusion cost checks,
   2026 completed-tech list, and Advanced Neural Networks prerequisite/cost
   assertions. Keep reference-template drift checks and graph validation.
3. Run `tools/deploy.ps1` without `-SkipVerification` immediately when the
   implementation is ready. It builds, validates, and checks that Terra Invicta
   is closed before validation and again before file replacement.
4. Immediately announce readiness for manual testing after successful deployment.
5. In a new 2026 campaign, confirm Advanced Neural Networks is completed and
   the active research slots remain correct. Check a new 2022 campaign retains
   it as researchable. Verify the three displayed prices, the removed
   prerequisite, and the fusion layout in both full-tree modes. Inspect the
   selective prerequisite view for readable connections as well.
6. Update `docs/fusion-technology-tree-rebalance.md` and
   `docs/starting-technology-2022-2026.md` with implemented values and results.
   Correct the former's stale 2.0 multiplier examples. Record any remaining
   layout discrepancy explicitly before proposing the separate UI change.

## Inspection basis

Read the repository overrides and installed Steam game templates on 2026-09-23.
Inspected the installed assembly's research-screen implementation using a
temporary decompilation under `.tmp/`. No build, deployment, or gameplay edits
were performed for this planning pass.

## Implementation and deployment record

### Manual-test correction: technology array merge semantics

The user reported that Advanced Neural Networks still had its old prerequisite
in the deployed game. Inspection found that `ModInfo.json` selected the game's
default index-wise `MergeArrayHandling.Merge` for `TITechTemplate.json`.
An authored `prereqs: []` does not clear an inherited array in that mode.
The scalar 2,500 cost and 2026 completion grant were present in both source
and deployed files; the missing merge configuration invalidated the assumption
that the JSON arrays resolved as authored.

Correction plan: add `TITechTemplate.json` to `TemplatesToReplaceArrays`, then
validate merged technologies and scenarios through the installed game's own
`JsonController.LoadJson` / `CombineJson` implementation. Check every authored
override field, preserve omitted baseline fields and unmodified technologies,
and reproduce the old empty-array failure as a regression check. This also makes
the already-authored shortened fusion prerequisite/effect arrays resolve exactly
as designed. Keep the technology prices and scenario grant already approved.
Run normal deployment and include the correction in the same change set. The
user subsequently undid the unpushed commit, so create a replacement commit
containing the approved work and correction rather than amending its predecessor.

Correction result: `TITechTemplate.json` now selects array replacement in
`ModInfo.json`. The new native-merger validator reproduced the old failure and
failed against the old manifest, then passed with the corrected manifest. It
checks all 149 technology records, every supplied override field, preservation
of omitted baseline fields, the resolved 2,500 cost and empty prerequisite list,
and the exact scenario arrays including the 2026-only grant. The scalar cost
already merged to 2,500 under the old mode; the confirmed defect was the retained
array content, not a missing authored price edit.

The corrected `tools/deploy.ps1` run passed the full suite, including the new
native-merger check and 1,172 formula assertions, and verified all 46 deployed
files. Both game-closed checks passed. Manual retesting of the game UI remains
pending. The package version and DLL hash below are unchanged because the fix
is in the manifest and validation tooling, not the runtime assembly.

### Original implementation and initial deployment

Implemented the approved JSON edits and updated `tools/verify.ps1` to validate
the two installed price references, both fusion entry discounts, the explicit
empty Neural Networks prerequisite array, and the exact 2026 completion list.
The existing prerequisite/effect/AI checks and acyclic-graph validation remain.

`tools/deploy.ps1` completed its normal flow without `-SkipVerification` on
2026-09-23. Both game-closed assertions passed; release verification passed,
including 1,172 formula assertions and the implementation matrix's 101 rows,
24 settings groups, and 181 Harmony patches. All 46 deployed package files
matched their source hashes. The first attempt passed verification but lacked
permission to remove an installed stale DLL cache; the successful retry used
the same normal deployment flow with filesystem access to the game directory.

- Package: `artifacts/TIEconomyMod-0.9.7-ti1.0.53.zip`.
- Destination: `D:\Games\SteamLibrary\steamapps\common\Terra Invicta\Mods\Enabled\Economic Equalization Overhaul`.
- DLL SHA-256: `8B3BF6218FB952DDD25C4D03D285405F7D5BA8F7612FFB017D3AC88626353786`.
- The interactive preview now defaults to the implemented repository data and
  retains the original data as "Before changes". All 142 ordinary columns match
  the independent model for both versions; refreshed snapshots and browser
  interaction checks passed with no JavaScript page errors.

Manual testing remains pending: check full-tree placement and connection lines,
the 88,000 / 88,000 / 5,500 displayed prices, a new 2026 game's completed Neural
Networks, and a new 2022 game's still-researchable Neural Networks. Existing
saves receive no automatic completion grant from the scenario-template edit.
