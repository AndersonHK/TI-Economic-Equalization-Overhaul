# Multi-slot utility implementation

## Scope of the first playable slice

This change introduces gun-style footprint semantics for utility modules without
changing hull slot counts or coordinates. Hull-layout work is deliberately out
of scope because it is being developed independently.

The runtime supports four fixed footprints:

- `Single` (one cell);
- `TwoHorizontal` (two adjacent utility cells, left to right);
- `TwoVertical` (two adjacent utility cells, top to bottom); and
- `Four` (a contiguous two-by-two utility block).

The horizontal footprint is assigned to the approved large-module slice:

- Mobile Space Science Lab, Flag Bridge, every human Marine Assault Unit and
  faction assault-unit variant (Spartans, Rangers, and Immortals);
- Repair Bay, Salvage Bay, and Component Armor;
- all six manual Platform/Outpost Kits, the automated Solar/Fission Platform
  and Outpost Kits;
- Salamander Terror Unit Pod, Alien Army Pod, Alien Fusion Platform/Outpost
  Kits, Alien Repair Bay, Alien Surveillance Orbital, and Alien Surveillance
  Ring; and
- all six human heavy heat sinks.

ISRU remains a one-cell module. Four-cell placement logic is implemented but
no module uses it in this partial slice; 2x2 balance assignments remain deferred
until suitable two-by-two hull bays are authored by the parallel hull-layout
work.

## Data and persistence contract

`TIUtilityModuleTemplate.json` and `TIHeatSinkTemplate.json` records may define
the mod-owned string field `utilityFootprint`. Missing, null, or invalid fields
safely resolve to `Single`. The field is read from the raw scenario-aware
mod-template stream and is not added to the compiled game's template classes.

A placed utility continues to serialize as one `ModuleDataTemplateEntry`:

- `moduleName` identifies the one functional module; and
- `slot` identifies its anchor cell.

Secondary cells are derived. They never receive duplicate module entries, mass,
cost, power use, bonuses, damage state, or construction requirements.

Fixed footprint orientation is part of the template because the existing
single-anchor save representation has no field in which to persist a player's
rotation choice.

## Occupancy and designer contract

For a non-legacy layout:

1. Every footprint cell must be a utility cell in the same hull.
2. A drop is legal only when every footprint cell is empty.
3. Catalog eligibility is based only on whether the hull geometry contains a
   compatible footprint. Current occupancy is deliberately ignored, exactly as
   it is for multi-hardpoint hull weapons. A module stays draggable after other
   parts occupy that geometry, but is non-draggable and shown at the native 30%
   alpha when the selected hull can never fit its footprint.
4. Dropping on any cell covered by a valid empty footprint resolves to the same
   deterministic anchor-selection rule used by multi-slot weapons.
5. Every secondary cell resolves back to the module stored at its anchor.
6. Secondary destinations are blocked and hidden while the module is installed.
7. Removing the anchor clears and unblocks the complete footprint.

The base icon cell is 72 pixels. Catalog, selected-detail, drag, and installed
views all reuse the existing game icon and reshape it before placement: 2x1 is
wide and half-height, 1x2 is narrow and full-height, and 2x2 is square with a
subtle four-cell divider. Catalog layout owns its square list-item dimensions,
so the icon child is scaled within that cell after the native item update and
alpha pass. This mirrors the at-a-glance proportions of hull-weapon artwork
without fighting the list layout. The installed destination expands to 144 by
72 pixels for `TwoHorizontal`, 72 by 144 for `TwoVertical`, or 144 by 144 for
`Four`. Aspect preservation is disabled for a multi-slot preview so the
existing frame fills the footprint. Removal restores the destination's original
size, position, and aspect setting. Purpose-authored icons can replace this
baseline later, but must match the game's established framed and stylized icon
language.

## Existing-save boundary

Changing an existing utility from one cell to multiple cells can make an old
ship class impossible without discarding parts. Loading must never silently
delete or move those parts.

The first slice infers a legacy layout whenever any enlarged utility footprint:

- cannot be formed from the stored anchor;
- overlaps another stored module anchor; or
- overlaps another enlarged utility footprint.

All enlarged utilities in such a design retain one-cell occupancy. A newly
created or successfully repacked design uses modern multi-cell occupancy. This
keeps old saves and built ships operational while allowing the designer and AI
to create valid new layouts. Preventing continued construction of legacy
classes and offering an explicit modernization workflow remain follow-up work
after the playable footprint behavior is validated.

## Verification contract

Automated tests cover footprint offsets, geometry-only catalog compatibility,
anchor selection from every covered cell, occupancy rejection at drop time,
deterministic candidate ordering, and the absence of duplicate cells.
Target-assembly validation must also assert the patched method surface and the
complete assignment list before deployment.

Manual testing for this slice is:

1. Load an existing save and open the ship designer.
2. Select a hull with a horizontal utility pair.
3. Confirm 2x1 catalog icons are wide before selecting or dragging them.
4. Place a Mobile Space Science Lab by dropping it on either cell.
5. Confirm the lab spans and blocks both cells.
6. Fill every compatible pair and confirm the lab remains selectable, matching
   a two-hardpoint hull weapon; confirm the attempted drop is rejected until a
   pair is empty.
7. Remove the lab and confirm both cells become available without moving the
   anchor outline.
8. Place one requested 2x1 part into a horizontal pair and confirm both cells are
   occupied.
9. Save the design, leave the designer, reopen it, and confirm the same anchor,
   footprint, artwork, mass, cost, and effects.
10. Queue construction of the saved design.

## 2026-08-12 deployment result

The playable slice passed the TI 1.0.51 target-surface validator, all 744
formula assertions, the 117-patch implementation-matrix audit, release
packaging, and deployment of 33 files to the enabled mod directory. The
automated validator also locks the canary to `MobileSpaceScienceLab =
TwoHorizontal` and rejects any canary icon override.

In-game designer interaction, save/reopen behavior, and construction remain
manual acceptance tests because they depend on live Unity UI state and save
serialization.

## 2026-08-13 deployment result

The hull-weapon-parity correction, pre-placement footprint graphics, requested
2x1 assignments, and heavy-heat-sink extension passed the TI 1.0.51 guarded
target suite, all 745 formula assertions, the 120-patch implementation-matrix
audit, release packaging, and deployment of 33 files. The deployed DLL SHA-256
is `39E3CA5947DA9F0EDF58B7B26D649C91231B33A4FE47DF17C9CEC185AAF36BDD`.

Live catalog alpha, drag/drop feedback, save/reopen, and construction remain the
manual acceptance gate.

The approved large-module expansion subsequently returned ISRU to `Single`,
added the 15 approved human and alien utilities to `TwoHorizontal`, and changed
top-strip rendering to scale the icon child inside the native square catalog
cell. The follow-up passed the TI 1.0.51 guarded target suite, all 745 formula
assertions, the 121-patch implementation-matrix audit, exact validation of all
30 utility footprint declarations, release packaging, and deployment of 33
files. The deployed DLL SHA-256 is
`EBC53087189CB18D53967B81C263F3FC574EE12C60F768493A643FAE0F874082`.

## Cyclotron prospective placement

Vanilla rejects a Cyclotron until the prospective design already contains a
particle-beam weapon because the module carries `ParticleBeamPowerBonus`. In
the designer this makes every destination appear blocked even though Cyclotron
is a single-slot utility. The compatibility patch re-runs vanilla design
validation for Cyclotron with only that prerequisite temporarily omitted, so it
may be installed before its supported weapon while every other native validity
rule remains in force. Its footprint is explicitly declared `Single`.

## 2026-08-13 horizontal-correction deployment result

The corrected 2x1 assignments, final-pass catalog-thumbnail resize, and
Cyclotron prospective-placement exception passed the TI 1.0.51 guarded target
suite, all 745 formula assertions, the 121-patch implementation-matrix audit,
release packaging, and deployment of 33 files. The deployed DLL SHA-256 is
`98B1ED94783B3622158285FFA2B8A8364389F9F3CDE5D9D40044F7B6A87DF5AB`.

Live top-strip rendering, horizontal placement/removal, Cyclotron placement,
save/reopen, and construction remain the manual acceptance gate.
