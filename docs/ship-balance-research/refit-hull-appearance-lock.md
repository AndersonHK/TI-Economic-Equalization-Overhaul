# Refit hull-appearance lock

Status: implemented and deployed on 2026-08-19 for Terra Invicta 1.0.51.

## Rule

A refit must preserve the original design's effective hull appearance. A
candidate with a different `GetHullAppearanceIndex` is invalid even when its
`hullTemplate` is unchanged.

This treats the graphical variant as part of the physical ship class. Several
EEO systems attach measured hull mass, fuel volume, reactor-bay volume, and
drive-art scale to the appearance index, so changing appearance during a refit
would otherwise change more than presentation.

## Integration point

Vanilla centralizes refit compatibility in
`TISpaceShipTemplate.IsAValidRefitFor(oldShipTemplate, out reason, getReason)`.
It already rejects hull, power-plant-class, drive, heat-sink, battery, utility,
and weapon incompatibilities and supplies the red invalid-refit reason used by
the fleet UI.

EEO will add a Harmony postfix to that method. When vanilla accepted the
candidate but the effective appearance indices differ, the postfix will:

- change the result to `false`;
- leave reason generation empty when `getReason` is false; and
- otherwise use the same two-newline and `TIUtilities.RedLine` presentation as
  vanilla with a localized `UI.Fleets.RefitFailHullAppearance` message.

The check will be gated by the mod's master enabled state. AI refit generation
already seeds provisional refits with the original effective appearance, so
legal AI behavior remains compatible with the new shared validity rule.

## Verification

Automated verification must establish that the target game method and expected
signature still exist, the mod assembly carries the postfix, and the English
localization key is packaged exactly once. The normal `tools/deploy.ps1` flow
then performs the complete build, validation, and enabled-mod copy.

Manual in-game checks:

1. Open an existing design for refit and change only its hull appearance.
2. Confirm the UI marks the refit invalid and displays "Hull appearance must
   match."
3. Restore the original appearance and confirm this reason disappears.
4. Confirm an otherwise legal same-appearance refit can still be saved and
   applied.

## Deployment result

The normal deployment flow validated the target method, Harmony postfix, and
localization; all 1,078 formula assertions; all 144 Harmony patches; the
95-row implementation matrix; release packaging; and the 44-file enabled-mod
copy. The deployed DLL SHA-256 is
`D81039EB982F2B2AA75F51428F41302DA2E99B5C4460E6093F37BE844A26B573`.

Manual in-game testing remains pending.
