# Art-style module-panel crash diagnosis

Date: 2026-08-18  
Status: diagnosed, corrected, validated, and deployed on 2026-08-18 for Terra
Invicta 1.0.51.

## Finding

The crash is caused by the mod's post-art-change module-detail refresh calling
vanilla `FleetsScreenController.UpdateModuleDataPanel` for the installed panel
while `selectedDragDestination` is null. Vanilla dereferences
`selectedDragDestination.currentPart` without a null check on its installed
panel path.

This is a UI-lifecycle/state-synchronization defect. The hull multiplier and
reactor-output calculations correctly trigger compatibility reevaluation, but
the module selection's destination context is not rebuilt before the forced
detail-panel refresh.

## Runtime evidence

The current `Player.log` ends at lines 8162-8188 with:

```text
Handling Exception
Game speed at time of crash: 0
NullReferenceException: Object reference not set to an instance of an object
  at ...FleetsScreenController.UpdateModuleDataPanel_Patch1(...)
  at TIEconomyMod.Patches.ReactorBayAppearanceRefreshPatch.RefreshModulePanels(...)
  at TIEconomyMod.Patches.ReactorBayAppearanceRefreshPatch.Postfix(...)
  at ...FleetsScreenController.OnCycleAltHull_Patch1(...)
...
Closing Terra Invicta due to exception
```

This establishes the complete triggering route: the art-arrow button invokes
`OnCycleAltHull`, the mod's appearance postfix runs, and its explicit module
panel refresh enters the patched vanilla panel method and throws.

There is no `Reactor bay appearance reconciliation clamped drive` or
`...removed drive` message in the log. The installed-drive replacement/removal
branch therefore did not run during this incident. The log does not preserve
the selected hull, reactor, or drive names.

## Logic sequence

1. Vanilla `SetSelectedShipPartFromMenu` calls
   `GetBestDropDestinationForModule(part)` and stores its result in
   `selectedDragDestination`.
2. `GetBestDropDestinationForModule` returns null when no slot both accepts the
   part and passes `newShipTemplate.ValidPartForDesign(module)`. The mod's
   hull-scaled drive/reactor compatibility postfixes participate in that
   validity result.
3. A drive that the current reactor cannot support can therefore remain the
   currently selected preview while its best destination is null.
4. Clicking the art arrow invokes vanilla `OnCycleAltHull`, which commits the
   new `hullAppearanceIndex` and refreshes the ship panel. A smaller measured
   engine multiplier lowers scaled drive demand and can change the selected
   drive's compatibility.
5. The mod's `ReactorBayAppearanceRefreshPatch.Postfix` then reconciles any
   installed drive, refreshes cached designer state and availability, refreshes
   both module tables, and calls `RefreshModulePanels`.
6. `RefreshModulePanels` reads `selectedDragDestination`. When an installed
   detail module is present, it calls
   `UpdateModuleDataPanel(false, installed, false, slotType)`. It translates a
   null destination only into `ShipModuleSlotType.None`; it does not suppress
   the installed-panel call or reconstruct the destination.
7. Vanilla's `isSelected == false` branch immediately evaluates
   `selectedDragDestination.currentPart` for the installed fire-mode and delete
   buttons. Because the destination is null, it throws before the panel can be
   refreshed.

The compiled `RefreshModulePanels` IL places the installed-panel call at
`IL_0055`; the crash reports the caller at the immediately preceding
destination-selection sequence around `IL_004a`. The selected-panel call is
later at `IL_0083`, so the evidence points to the installed-panel refresh.

## Relationship to the earlier appearance crash

This is not the August 13 failure mode.

- The earlier stack ran through `ShipModuleDragDestination.OnDecreasePressed`
  and `SetModuleInSlot` after an oversized installed drive cluster survived an
  art change.
- The current stack runs directly through `OnCycleAltHull`,
  `RefreshModulePanels`, and `UpdateModuleDataPanel`.
- The installed-drive reconciliation emitted no replacement/removal log in the
  current incident.

The third correction fixed the installed drive invariant but added a forced
refresh of both contextual panels. That refresh assumed a destination existed,
which is false for an incompatible selected module.

## Scope

The smaller engine multiplier exposed the defect by changing compatibility,
but the crash condition is broader:

```text
art-change postfix
+ non-null installed detail module
+ null selectedDragDestination
= unsafe installed-panel refresh
```

Any art change that reaches this UI state may crash. `SetAltHull` shares the
same postfix and is theoretically exposed, although the reported and confirmed
path is the interactive `OnCycleAltHull` action.

There is no evidence here of an incorrect drive-demand multiplier, reactor-bay
output, or gas-core template value. The fault is the stale/null UI destination
context after compatibility and availability are recomputed.

## Correction requirements

A correction should preserve the installed-drive reconciliation while making
panel refresh lifecycle-safe:

1. Recompute the best destination for a retained selected module after the new
   appearance and module filter have been applied.
2. Never invoke vanilla's installed-panel path when
   `selectedDragDestination` is null; passing a null part or `None` slot type
   is not sufficient because vanilla dereferences the destination first.
3. Clear or refresh stale installed/selected panel state through safe designer
   actions rather than directly replaying a panel method whose preconditions
   are not met.
4. Preserve the existing clamp/remove behavior for genuinely incompatible
   installed drive variations.
5. Replace the current structural validation that merely requires exactly two
   `UpdateModuleDataPanel` calls with validation of the null-destination gate
   and destination reconstruction path.

Required manual regression cases should include an incompatible selected drive
with no destination, transitions that make it valid and leave it invalid,
installed-drive clamp and removal cases, both art-cycle directions, and a
normal selection with a valid destination.

## Approved correction design

The user confirmed that the incompatible drive was being held by the cursor.
This is the exact state represented by a non-null selected module and a null
best drop destination.

The approved implementation will:

1. refresh the selected cursor module first through vanilla
   `SetSelectedShipPartFromMenu`, which recomputes its best destination under
   the newly committed art multiplier and refreshes the selected panel;
2. read `selectedDragDestination` again after that reconstruction;
3. stop the contextual installed-panel refresh if the destination is still
   null, leaving the safe selected preview active;
4. when a destination exists, use its authoritative `currentPart` rather than
   the potentially stale `currentlyInstalledModule` reflection field; and
5. update the guarded IL validation to require destination reconstruction, a
   null gate, destination-backed installed state, and only one direct installed
   `UpdateModuleDataPanel` call.

## Implementation and verification

The approved design is implemented in `ReactorBayAppearanceRefreshPatch`:

- `SetSelectedShipPartFromMenu` now reconstructs the held module's best
  destination and refreshes its selected preview after art compatibility is
  recalculated.
- A still-null destination returns before the unsafe vanilla installed-panel
  path.
- A valid destination supplies both the slot type and authoritative installed
  `currentPart`; the stale `currentlyInstalledModule` reflection field has been
  removed.
- The guarded IL validator now requires the reconstruction call, destination
  getter, null branch, destination-backed part read, and one direct installed
  panel refresh in lifecycle-safe order. It rejects restoration of the stale
  installed-module field.

The normal deployment pipeline passed 1,078 formula assertions, all 143
Harmony patches, guarded TI 1.0.51 validation, release packaging, and the
44-file enabled-mod deployment. Deployed DLL SHA-256:
`EC1FD2DB5BAA04D539825758F0D40EA30EA2015EC3F4BFF3FF99474E1DC46EFE`.

Manual confirmation of the exact held-incompatible-drive art-cycle sequence
remains pending.
