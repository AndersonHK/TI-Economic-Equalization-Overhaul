# Technology-tree analysis and visual previews

These are research and review artifacts accompanying the technology JSON edits
approved and deployed on 2026-09-23. The interactive preview and current snapshots
now reflect the implemented prices and prerequisite change. Before-change
snapshots preserve the original review baseline. No UI runtime patch was added.

- [Detailed column-placement report](column-placement-analysis-2026-09-23.md)
- [Interactive preview](tech-tree-preview.html)
- [Calculated evidence](column-placement-evidence-2026-09-23.json)
- [Merged repository technology snapshot](merged-tech-snapshot-2026-09-23.json)

## Snapshots

| View | Before changes | Current repository: implemented |
|---|---|---|
| Fusion and reference technologies | [PNG](tech-tree-before-fusion.png) · [SVG](tech-tree-before-fusion.svg) | [PNG](tech-tree-current-fusion.png) · [SVG](tech-tree-current-fusion.svg) |
| All global technologies | [PNG](tech-tree-before-all.png) · [SVG](tech-tree-before-all.svg) | [PNG](tech-tree-current-all.png) · [SVG](tech-tree-current-all.svg) |
| Neural Networks and immediate neighbors | [PNG](tech-tree-before-neural.png) · [SVG](tech-tree-before-neural.svg) | [PNG](tech-tree-current-neural.png) · [SVG](tech-tree-current-neural.svg) |

The archived `tech-tree-requested-*` snapshots are the user-reviewed draft;
their prices and prerequisite changes are now implemented: D-T = 40,000,
Methodologies = 40,000, and Neural Networks = 2,500 with no prerequisite.
The optional broader discount schedule and Future Tech UI correction remain
unimplemented. The deployed scenario also grants Neural Networks to new 2026
campaigns. The saved analysis JSON and merged input snapshot retain the original
review baseline; their `current` fields mean pre-implementation data.

## Fast iteration

Open the preview in a browser. It is self-contained and makes no network calls.

1. Select a node or choose a technology from the list.
2. Edit its base research cost or comma-separated prerequisite identifiers.
3. Select **Apply to preview**. All 149 global technologies are recalculated;
   the chosen scope only limits which nodes are drawn.
4. Use **Save SVG snapshot** to keep a scalable visual of the draft.
5. Use **Save draft overrides** to download a candidate technology JSON. This
   preserves the current override fields and adds the draft cost/prerequisite
   changes. It does not write to the repository or deploy the game.
6. After actual repository edits, re-select
   `TIEconomyMod/ModFiles/TITechTemplate.json` with **Refresh from repository
   JSON**. The selected overrides are merged over the embedded installed
   vanilla baseline, replacing the preview's previous repository snapshot.

Use the Future Tech checkbox to compare the proposed separate-column rule.
This changes only the preview's layout and is never included in JSON exports.
The scenario's 2026 completion grant is not exported by this technology-only
editor; it belongs in `TIStartTimeTemplate.json`.

## Fidelity and limits

- Columns reproduce the inspected technology-only cost/prerequisite passes.
- Native Future Tech parent placement is modeled separately from its inconsistent
  recorded node. The optional checkbox uses max ordinary column + 1 instead.
- Vertical order, spacing, node appearance, and line routing are schematic.
  These are not game screenshots or guarantees of exact vertical adjacency.
- The preview models fresh initialization and the 2022/2026 starter pair. It
  does not simulate a project-inclusive tree, selected-tree reflow, or controller
  state retained across previously opened UI views.
- Input snapshots are from the installed game plus repository data on
  2026-09-23. A game update, additional mod, or different scenario requires
  refreshing the baseline/model as appropriate.
- Unknown prerequisites, cycles, unsupported alternate global prerequisites,
  and non-converging layouts are rejected before replacing the drawn result.

## Verification record

Manual testing exposed a missing technology-array replacement setting after the
initial deployment. The corrected manifest now uses `TemplatesToReplaceArrays`
for `TITechTemplate.json`, making empty/shortened arrays resolve as the preview
assumes. The preview is an authored-data model; the original before-change
snapshots were not captures of the game's effective default-merged arrays.
The release gate now executes the game's native `JsonController` on all 149
technologies and the scenario templates, checking supplied and preserved fields.

The browser implementation matched the independent Python model for all 142
ordinary technologies in both the before-change and implemented scenarios.
Six current/before PNG/SVG snapshot pairs were rendered with headless Edge;
the original review snapshots are also retained.
Checks also covered price edits, draft exports, JSON refresh, unknown/cyclic
prerequisites, separate Future Tech placement, and Future Tech price independence.
No JavaScript page errors were reported. In-game comparison remains pending.
