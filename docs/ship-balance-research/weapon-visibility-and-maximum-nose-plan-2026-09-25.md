# Weapon visibility and maximum nose mounts

New discussion candidate: [nose batteries and enlarged spinal tier](nose-battery-and-spinal-tier-proposal-2026-09-25.md).
Its proposed counts differ from the single-primary table below, and alien
Dreadnought/Titan choices remain open. Neither nose proposal is implemented.

Status: **nose proposal deferred by the user**, pending a decision on alien
ships. Nose fitting, AI selection, weapon statistics and migration remain
unchanged. The approved subset is now implemented in 0.10.0: hide forbidden
small hull weapons on Titan, AlienTitan and AlienMothership in both designer
catalog views. See the [implementation record](capital-heavy-mount-implementation.md)
for deployment and testing status. Broader filtering on other hulls is still
only a proposal. The sections below retain the deferred design for discussion.

## Intended behavior

Hide weapons that the selected hull cannot legally mount, using the same row
visibility behavior as obsolete parts. This is a per-hull filter; never mark
those weapons globally obsolete. Switching hulls restores the applicable rows.

Titan, AlienTitan and AlienMothership catalogs show only FourHull hull weapons.
Other ships retain their current hull-weapon sizes, with physically incompatible
mounts hidden. For nose weapons, allow and display only the largest mount size
supported by that hull's geometry. All researched technologies/families at that
size remain choices; this does not select only the highest technology tier.

Use the hull's maximum, independent of current research or occupied cells.
A partially filled nose does not make smaller weapons legal. If no weapon of
the required size is unlocked, show an explanatory empty state and allow the
nose to remain empty. Do not silently fall back to a smaller size. Families
without that size are absent: for example, the conventional 10-inch/12-inch
nose cannons cannot be offered on a size-3 or size-4 hull. No research is granted.

Existing obsolete/research filters still apply. “Show obsolete” may reveal
obsolete weapons of a legal size, but must not reveal or enable forbidden sizes.
Parts temporarily unavailable because of the current equipment or capacity
remain governed by existing availability rules. Avoid hiding every part for
which `ValidPartForDesign` returns false: that also covers drive/reactor pairing,
utility grouping, and other conditions which can change while editing.

## Hull results

These are physical fitting sizes, not firepower multipliers. Native geometry
was queried from the installed 1.0.53b assembly, with current mod hull overrides.
The [geometry snapshot](tables/nose-maximum-mount-plan-2026-09-25.json) records
all 28 hulls, exact slot sets and the assembly hash.

| Hull group | Proposed nose size | Result |
| --- | ---: | --- |
| Human Gunship, Corvette, Frigate | 1 | One size-1 weapon |
| Human Destroyer, Cruiser, Battleship | 2 | One size-2 weapon |
| Human Battlecruiser, Dreadnought | 3 | One size-3 weapon |
| Human Lancer, Titan | 4 | One size-4 weapon |
| Human Escort, Monitor | None | No nose weapons |
| Alien Gunship, Corvette, Frigate, Monitor | 1 | One size-1 weapon |
| Alien Destroyer, Cruiser, Battleship | 2 | One size-2 weapon |
| Alien Battlecruiser | 3 | One size-3 weapon |
| Alien Dreadnought, Mothership | 4 | One size-4 weapon |
| Alien Lancer, Titan | 4 | One size-4 weapon; two cells unused |
| Alien Escort, Assault Carrier | None | No nose weapons |
| STO Fighter, Salamander Gunship | Native fighter mount | Preserve the specialized HalfNose equipment path |

**Alien six-cell consequence:** both AlienLancer and AlienTitan have six nose
cells but only one complete FourNose footprint. Strict maximum-size fitting
therefore removes their smaller secondary nose weapons. This plan follows that
literal rule. Two size-3 weapons, or a size-4 plus a size-2, would require an
explicit exception and is not the default proposal. Their hull mounts remain
available under their existing rules.

The fighter exception preserves the separate native fighter equipment category;
it does not promote a HalfNose fighter weapon into a ship-sized weapon merely
because the craft's UI has one nose cell.

## Shared rule and UI integration

1. Introduce one hull-compatibility policy shared by visibility, placement,
   design validation, player auto-design, AI and migration. Determine the
   largest valid nose footprint from actual slots, checking both size-2
   orientations. Validate distinct slot indices and slot types: native FourNose
   geometry repeats one neighbor check and should not be trusted solely from
   the returned list length. Do not infer maximum size from total cell count.
2. Extend `FleetsScreenController.FilterAvailableShipModules` for both
   `shipModuleListItems` (drag icons) and `shipModuleListItemsB` (table rows).
   Apply visibility after native research/obsolete filtering, refresh table
   widths when visibility changes, and clear a selected catalog item or drag
   that has become invalid. Respect `loadingExistingTemplate` and refresh once
   loading completes. Cover hull changes, refit/import, both catalog views,
   search/sort and the obsolete toggle.
3. Enforce the same predicate in `ValidPartForDesign`, assigned-slot/drop
   validation, completed-design validation and save/refit actions. UI hiding
   alone must not allow a stale selection or imported design to bypass fitting.
   Do not change graphical footprints or compress nose weapons into single cells.

## AI and auto-design

The current `TIFactionState.SetShipDesignNoseWeapons` does more than choose the
largest weapon. It excludes size-3/4 weapons for Protector roles, selects one
primary weapon, then fills unused cells with size-1 weapons. Filtering the
candidate list alone would leave some roles with no primary weapon.

Replace that selection/placement path with the shared maximum-size policy,
preserving researched-equipment checks, exotic-resource allowances, native
role scoring and obsolete preference among legal weapons. Remove the role-based
size prohibition and the size-1 filler pass. Fill only complete, non-overlapping
maximum-size footprints. Apply this identically to human AI, alien AI and player
auto-design. When no legal weapon is available, clear stale nose entries and
return an explicit no-legal-nose outcome; combat design generation can select
another hull or rely on legal hull weapons. Prevent retry loops and accidental
unarmed combat designs. Preserve the specialized fighter generation path.

## Existing saves and imports

Keep fresh-save behavior the first acceptance target. Compatibility still means
migrating existing data, not retaining old fitting rules indefinitely.

Add a separate nose-layout migration marker, preserving the existing capital
hull marker so that already-converted hull weapons never expand again. Run nose
conversion before any new validation, hydration or scoring that could discard
old entries. Cover campaign designs, obsolete/refit designs, construction queues,
built ships, saved-design imports and stock/imported skirmish templates.

Proposed conversion policy:

- Back up source data and generate a per-design conversion report before commit.
- Preserve already-legal maximum-size equipment and its anchor where possible.
  For smaller-only layouts, prioritize the family/tier of the largest installed
  nose weapon; resolve ties deterministically, preferring the more advanced
  installed tier within a family. Use a reviewed replacement map rather than
  comparing inconsistent cross-family DPS figures.
- Fill legal maximum-size footprints with same-family equivalents where
  available, otherwise a researched legal replacement. Use static project
  prerequisites without a faction. If necessary, use a documented legacy
  equipment exception without granting a project, as in capital hull migration.
- Where multiple old weapons consolidate into one, report the many-to-one
  mapping. Preserve already-legal weapons' ammunition/damage fractions; for
  consolidated weapons, use capacity-weighted ammunition fractions among old
  projectile weapons and occupied-cell-weighted damage fractions. Remap repair
  and reload orders once per new module. Do not sum old ammunition counts across
  different calibers or silently repair an existing legal maximum-size weapon.
  Archive redundant old entries, including the alien six-cell secondary weapons.
- Keep ship identity, fuel, hull appearance, armor and unrelated equipment.
  Recompute mass, power, heat and combat caches at safe lifecycle boundaries.
  Preserve excess fuel/reactor load until refit, then enforce current limits.
  Failed conversion must preserve source data and produce a report for repair.

This needs a dedicated migration preflight against representative saves before
release. Recommend **0.11.0** for the additional one-way nose-layout conversion;
the crash-fix build remains 0.10.0.

## Balance and verification

Nose DPS, mass, draw, cooldown, range, optics and projectile statistics remain
unchanged. The earlier nose-stat rebalance remains deferred. Restricting size
still changes fleet behavior: fewer independent shots, less target splitting
and weaker nose point-defense coverage on larger hulls. Six-cell alien ships
also lose their small secondary weapons. Combat testing must measure that
effect separately from any future weapon-stat changes.

Implement → run the normal build/deploy validators → notify for manual testing
→ document results. Add checks for every hull/appearance and nose mount,
empty/partially filled noses, both size-2 orientations, six-cell aliens,
fighter equipment, missing research, all-obsolete legal choices, Protector
auto-design, stale drag/drop/save attempts and UI row restoration. Test all
migration entry points, ammunition/damage/order remapping, repeated save/load,
and the complete menu → import → combat → menu → second combat cycle.

Success: player catalogs contain only legal sizes, every placement/action and
AI path agrees, obsolete flags are untouched, all old designs convert exactly
once, and the full lifecycle completes without stale faction/effects references.
