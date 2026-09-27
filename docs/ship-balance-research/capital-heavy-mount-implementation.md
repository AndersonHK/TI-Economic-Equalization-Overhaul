# Capital heavy mounts — EEO 0.10.0

## Symmetric human utility grids, 2026-09-25

Implemented and deployed: follow the annotated designer screenshots using JSON coordinates only.
Dreadnought utilities at `(3,2)` and `(3,4)` replace the center singleton and
extra top-row cell. Titan gains a symmetric two-column, three-row block at
`x=3,4; y=1,3,5`, retaining the three utilities above and below its hull weapons.
Keep serialized slot indices/types, counts, weapons, mass and crew unchanged.
Retain the original Titan vertical utility pair and the top/bottom horizontal
pairs. Earlier 0.10.0 layouts can have different multi-slot adjacency; existing
legacy-layout handling preserves their installed modules when a footprint no
longer fits or conflicts. This change adds no new save conversion or UI code.

Validate native bounds, reflected utility coordinates, rendered icon separation
and unchanged vanilla multi-slot utility index sets. Refresh only the hull-report
source hash, since physical measurements and slot counts are unchanged. Deploy
through the normal verification flow, then manually check both designer layouts.
Generate a separate Mothership diagram from its current JSON and native armor
offsets for review; the diagram is a schematic, not a captured game render.

Normal deployment passed all 37 validators, 1,204 formula assertions and the
105-row/210-patch matrix; 47 files copied. Geometry checks preserve 11 vanilla
Dreadnought utility placements, 14 vanilla Titan placements, and all 20 prior
Mothership placements. Both human utility arrays reflect about `y=3` without
rendered icon overlap. Authored and deployed hull JSON SHA256:
`ED0A6EE01A2F5931324B15897A923AB9EB6F5A822A25FE7BF02E9D1F7E6D4318`.
The package remains 0.10.0. Manual designer verification is pending.

Review diagrams: [Dreadnought](figures/dreadnought-designer-layout.png),
[Titan](figures/titan-designer-layout.png), and
[Alien Mothership](figures/alienmothership-designer-layout.png). Regenerate with
`python scripts/ship-balance/draw_designer_layouts.py` (Pillow required).
The Mothership diagram depicts the previously deployed layout; this revision
moves only the human Dreadnought and Titan utilities.

## JSON-only Mothership layout, 2026-09-25

Implemented and deployed: removed `CapitalUtilityDesignerRowPatch` and all cloning,
spacing and scaling code. Repack AlienMothership in its JSON slot array within
the native 10-column, 7-row coordinate map. Keep all 38 serialized slot indices
and types, all weapon coordinates, all 11 utilities, and existing module
adjacency. Move the original utility group down one coordinate row and move
the four added utilities together from `(4..7,8)` to `(0..3,0)`.

Drive/power/radiator controls move to `(0..2,4)`, propellant to `(9,0)`, lateral
armor to `(0,1)` and nose armor to `(8,6)`. Tail armor stays at `(0,6)`. Native
armor images are offset down one half-height row; the even tail/nose x-coordinate
sum prevents the lateral armor's extra half-column shift. Check rendered icon
separation using those native offsets, rather than treating armor anchors as
ordinary full-height module centers. No fitting, mass, crew or nose rule changes.

Validation preserves all 20 prior utility placements (11 singles and 9 larger
footprints) by serialized index, from vanilla and the earlier 0.10.0 layout.
Native bounds, unique coordinates, rendered armor offsets, weapon stability and
removal of the old patch all pass. Existing ship equipment is indexed by slot
and needs no new migration. All 37 validators, 1,204 formula assertions and the
105-row/210-patch matrix passed; 47 files deployed successfully. Actual designer
rendering and interaction remain manual acceptance items. Hull report metadata
was refreshed from unchanged measured rows; slot counts and physical geometry
did not change.

## Designer correction and catalog visibility, 2026-09-25

Historical correction: the row extension described in this section has since
been removed by the JSON-only layout above. Catalog hiding remains active.

Fixed the designer-entry exception in `CapitalUtilityDesignerRowPatch`.
The native designer uses each destination's `defaultPosition`; its optional
`shipModuleSlotGrid` component is absent in the reported scene. Ordinary hulls
now require no layout mutation. The extension runs only when a hull needs extra
rows, derives new cell positions from existing row/column spacing, and preserves
the native parent scale for restoration when switching back to a smaller hull.

Implemented the approved catalog subset: hide forbidden small hull weapons in
both icon and table views for Titan, AlienTitan and AlienMothership. Use the
existing FourHull-only rule, preserve native research/obsolete filtering and
restore rows through native filtering when changing hulls. Nose eligibility,
placement, AI, statistics and migration remain unchanged. The maximum-nose
proposal is deferred pending a decision on the six-cell alien hulls.

Normal deployment passed all 37 validators, 1,204 formula assertions and the
105-row/211-patch matrix, then copied 47 matching files. A real invocation of
the designer prefix with a native 70-cell map and no GridLayoutGroup preserves
that map on repeated calls. Added checks cover independent extra-cell positions,
restoration of layout scale, and capital-only filtering across all 28 hulls and
all nose sizes. Actual Unity row creation, icon/table visibility and designer
navigation remain manual checks. No new save schema or version bump is needed
for this existing-rule UI correction; the package remains 0.10.0.

## Save-load initialization correction, 2026-09-25

The next manual test reached campaign migration successfully (9 designs and
21 ships) before a premature performance-cache refresh dereferenced the still
uninitialized faction-effects table. The refresh now waits for effects-state
phase 2; the shared helper also guards early calls from load and settings save.
The original serialized campaign was archived before conversion. Full normal
deployment passed again with 47 matching files; the same-save reload is pending
manual verification. See the [cache diagnosis and regression checks](../compatibility/ti-1.0.53-ship-performance-cache.md).

## Main-menu startup regression, 2026-09-25

Manual testing found `InvalidOperationException: No owning faction for legacy
capital design Ship20a`. The main menu constructs dummy ships while scoring its
stock skirmish roster, before campaign factions exist. The new InitWithTemplate
guard incorrectly treated that stock-template path as faction-owned migration.

Correction: normalize stock capital templates before performance caches
are refreshed, keeping same-family/tier heavy equivalents. For missiles and
other weapons without a larger equivalent, consult the static project/technology
registry: infer prerequisites from the authored hull and weapons, then choose
the highest-ranked heavy weapon whose engineering chain is supported by that
global-tech floor. Alternatives satisfy only their corresponding prerequisite
slot; ambiguous source branches are not assumed researched. Unknown roots and
cycles do not qualify. Baseline heavy mags remain the last-resort fallback.
No research state is created or granted. Explicit `Empty` roster entries are
vacant cells and are skipped.

Do not create campaign migration backups for stock catalog entries. Keep actual
faction research filtering for owned designs. Regression checks exercise
the ownerless InitWithTemplate guard with the installed stock capital roster,
including Ship20a, and verify repeated initialization cannot expand it again.

The corrected 0.10.0 build passed the normal deployment flow on 2026-09-25:
all four installed stock capital layouts convert and hydrate without a faction;
the inference fixture admits a peer engineering project while rejecting future,
unknown-root, cyclic and partially satisfied alternative paths. All 47 installed
files match the package. An actual main-menu relaunch remains the manual check.

## Implemented capital rules

Implementation follows revision 4 of the September 25 capital proposal. The
separate alien missile magazine update already occupies 0.9.9 and is retained.
No nose weapon statistics or geometry change.

Titan, AlienTitan and AlienMothership accept only FourHull weapons, one per
existing hull cell. Physical size, cost, mass, power, heat and ammunition remain
those of the heavy weapon. Designer icons and model hardpoint selection use a
one-cell footprint without changing shared weapon templates.

Utility slots are appended, preserving all existing slot indices. Mothership
utilities use an additional designer row so they do not overlap weapon cells.
Human Dreadnought/Titan crew becomes 19/40; alien base crew remains unchanged.
Structural masses implement the approved bare-mass targets and rounded human
appearance totals in the proposal.

Migration uses an explicit FullSerializer extension, `eeoCapitalLayout`, on
designs and built ships. Missing/zero means legacy; schema 1 means compact.
New designs and clones carry the marker; migration never guesses from overlap.
Legacy footprints expand to individual heavy weapons. Same-family/tier matches
come first; otherwise eligible magnetic weapons rank ahead of other heavy
families, deterministically. Normal fitting and AI use native faction build
eligibility; migration has the narrowly scoped legacy exception below.

All conversion plans are prepared before campaign mutation. The original
serialized campaign and a conversion report are archived before committing.
Fresh campaigns are the primary acceptance path; old saves are converted with
best-effort data preservation. If no heavy is unlocked, migration alone uses the
same-family heavy (or baseline heavy rail/magnetic battery), without granting
research or exposing that weapon in the normal catalog. Damage fractions,
ammunition fractions and supported firing
modes carry across; unrelated ship state and queued construction dates remain.
The user approved preserving over-limit legacy ships. The exception is saved on
the converted class. Designer/refit copies do not inherit it: their fuel is
clamped and native reactor/drive capacity checks apply. No propellant is removed
from an existing ship during migration.

## Automated validation and deployment

On 2026-09-25, the normal `tools/deploy.ps1` flow completed against Terra Invicta
1.0.53b with **1,204 formula assertions**, **37 validation jobs** across its two
pools, and the **105-row / 210-patch implementation matrix** passing. All **47
deployed files** matched the authored package. The script checked that the game
was closed before validation and immediately before copying.

The focused capital validator applies 20 capital patch classes to actual game
methods, checks persistent design and resolved ship-reference markers, rejects
future schemas, exercises 1/2/4-cell migration and repeated-conversion safety,
and runs both AI and player auto-design with small, heavy and locked choices.
It verifies independent hull anchors, existing slot-index preservation, utility
geometry, approved masses and unchanged alien crew. Eight non-missile weapon
source hashes match the pre-change planning snapshot.

The icon and catalog patches resolve their targets but need Unity native calls to execute.
Rendered icons, the added designer row, model appearances, full campaign
load/reload and live refit/combat behavior remain manual acceptance items.

Deployed DLL SHA-256:
`1D6E0510FD234DCF83BAE3A7A1E5E0FF767A28EC370628989DDC7FF6E0043501`
(JSON-only Mothership layout; supersedes earlier 0.10.0 binaries).

Deployment directory:
`D:/Games/SteamLibrary/steamapps/common/Terra Invicta/Mods/Enabled/Economic Equalization Overhaul`.

## Release values

Bare mass includes structural mass plus base crew at 3 tonnes per crew member.
These are V0 values; the proposal archive retains the complete appearance and
firepower-density comparisons.

| Hull | Utility slots | Base crew | Structural mass (t) | Bare mass (t) |
| --- | ---: | ---: | ---: | ---: |
| Dreadnought | 8 | 19 | 2,343 | 2,400 |
| Titan | 12 | 40 | 3,280 | 3,400 |
| Alien Dreadnought | 10 | 50 | 2,050 | 2,200 |
| Alien Titan | 10 | 60 | 2,420 | 2,600 |
| Alien Mothership | 11 | 100 | 8,200 | 8,500 |

## Skirmish save-import correction (2026-09-25)

The reported Edgeoya Block 5 Titan retained six 60 cm IR phaser batteries in
the skirmish preview. Save import assigns `StartMenuController.ImportedShipTemplates`
and bypasses both campaign initialization and the designer import hook. The
saved positive combat score also bypassed the dummy-ship conversion guard.
Conversion consequently happened only at `InitWithTemplate` during skirmish
bootstrap. Invalidating the score there caused `ECMValue` to query the still
uninitialized effects state while fleets were being created (phase 1).

Implemented: convert and back up imported capital designs before the native
import setter registers them or the dropdown scores them. An idempotent
normalization guard also runs in catalog construction. The normal main-menu
scoring path then caches the converted design before skirmish startup; the
new-ship guard preserves that score and layout. No nose, balance, or research
changes are involved.

The normal deployment flow passed all 37 validators, 1,204 formula assertions,
and the 105-row/210-patch implementation matrix, then deployed 47 matching files.
The new regression reproduces the reported six-phaser layout and positive saved
score, checks conversion before preview, original-slot and nose/fuel preservation,
backup invocation without a faction, and unchanged non-capital designs. After
supplying a completed preview score, it exercises repeated import, the new-ship
guard and the native cached-score read; no second expansion or invalidation
occurs. The skirmish validator also verifies normalization precedes scoring and
that Harmony binds the setter prefix. Backup IO is intercepted in this test to
avoid writing test designs into the user's backup directory. Actual Unity score
recalculation and import-to-combat execution remain manual acceptance items.

## Skirmish return-to-menu correction (2026-09-25)

The first imported skirmish battle completed successfully. On return, retained imported templates still cached the
departed faction in `_designingFaction`; scoring called `ECMValue` after native
game-state teardown had cleared effects. Before main-menu catalog scoring,
detach cached faction references from the whole roster and invalidate derived
values. Only do this when `GameStateManager.HasGamestates` is false; active
campaign/skirmish faction bindings must remain untouched. Apply the same cleanup
before explicit save import. Validate the actual ECM failure and recovery,
repeat safety, schema/equipment preservation, and the active-state exclusion.

Implemented and deployed through the normal flow: all 37 validators, 1,204
formula assertions, the 105-row/210-patch matrix and all 47 deployed files passed.
The added regression reproduces the native `ECMValue` null-reference exception
with a cached faction and no effects, then verifies the actual native call
succeeds after cleanup. Active-state faction bindings, converted weapons,
migration markers and repeat-use score caches are preserved. The full Unity
return-to-menu cycle remains the manual acceptance check.

## Manual acceptance order

1. Start a fresh campaign with 0.10.0. In the designer, check each capital hull:
   small hull weapons are hidden in both icon and table catalogs, heavy weapons fit independently in every
   hull cell, adjacent weapons remain separate, and all utility slots work.
2. Check auto-design and generated AI capitals for heavy-only hull weapons.
   Inspect all graphical appearances and combat turret positions, including the
   Mothership's repacked utility slots in the native designer grid.
3. Confirm the listed mass/crew values, full heavy weapon costs and capacity
   use, normal refit limits, and unchanged nose fitting and weapon statistics.
4. Load an older campaign containing small, medium and heavy capital weapons,
   queued construction and damaged/depleted ships. Check 1/2/4-weapon expansion,
   faction fallback choices, preserved fuel and fractional damage/ammunition.
5. Save and reload the migrated campaign: no second expansion should occur.
   Edit/refit an over-capacity migrated design and verify current limits apply.
6. Import the older save into Skirmish. Before beginning combat, the reported
   Titan should list six 360 cm IR Phaser Batteries instead of six 60 cm
   batteries, with its Spinal Coiler Mk3 unchanged. Start combat, then repeat
   the import/start sequence to check that the weapon count does not expand.
7. Finish the imported battle, return to the menu, inspect the retained roster,
   and start a second battle. Repeat after re-importing the same save.
8. After loading the reported save, open a new design and an existing design.
   Check Titan/AlienTitan/Mothership in both icon and table views, including
   “Show obsolete.” Switch to Dreadnought/other hulls and verify eligible small
   weapons return. Inspect all 11 Mothership utilities and the repositioned
   equipment/armor controls; the designer should retain native size throughout.
   Nose choices stay native.

Pre-conversion backups and reports are written under
`%LOCALAPPDATA%/TIEconomyMod/CapitalMigrationBackups`. This is a one-way layout
change; the original backup is the recovery path when testing an older release.
