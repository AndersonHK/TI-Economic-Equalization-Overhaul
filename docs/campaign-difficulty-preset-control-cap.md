# Campaign difficulty preset control-cap assessment

Status: implemented and deployed for Terra Invicta 1.0.51 on 2026-08-20;
manual new-campaign validation pending.

## Conclusion

The immediate goal is a good fit for a JSON override. Add the following scalar
to the existing `TIEconomyMod/ModFiles/TIGlobalConfig.json` object:

```json
"controlPointMaintenanceFreebies": 0
```

No Harmony patch, new template file, localization change, or
`TemplatesToReplace` manifest entry is needed for the capacity change.

This sets the **Base Control Point Capacity** shown by the new-campaign screen
to zero for Cinematic, Normal, Veteran, and Brutal. The setting is one shared
default, not a four-element difficulty table. Consequently, JSON supports the
requested value on every difficulty, but it cannot give each difficulty a
different base value without a code patch.

## How the preset is assembled

`StartMenuController.ResetCampaignDifficultyOptions()` rebuilds the campaign
controls when the selected difficulty changes, when the player restores the
long-campaign defaults, and before the accelerated preset applies its speed
multipliers.

The method gets the player and shared-AI base capacity through
`baseFreebiesCount()`:

```text
base capacity = TIGlobalConfig.controlPointMaintenanceFreebies
              + removed-faction compensation

removed-faction compensation =
    (7 - human faction count) *
    TIGlobalConfig.controlPointBonusMaintenanceFreebiesPerRemovedFaction
```

The compiled defaults are 125 base capacity, 50 per removed human faction,
and 25 capacity per UI slider tick. The standard seven-human-faction setup has
no removed-faction compensation. An explicit JSON value of zero therefore
puts the slider at zero ticks in all four standard difficulty presets.

Only a few difficulty differences in this screen are data-driven:

| Preset input | Source | JSON-only tuning? |
| --- | --- | --- |
| Base Control Point Capacity | Shared `controlPointMaintenanceFreebies` field | Yes, shared across all difficulties |
| AI Bonus Control Point Cap | `AI_BonusCPCap_C/N/V/B` fields selected by difficulty | Yes, independently by difficulty |
| AI Bonus Mission Control | `AI_BonusMissionControl_C/N/V/B` fields selected by difficulty | Yes, independently by difficulty |
| Cinematic combat realism toggles | Hardcoded comparison with selected difficulty | No |
| Research, alien progression, construction, and most other reset values | Mostly hardcoded slider values | Usually no |

Thus the wider idea of redefining arbitrary difficulty presets is only partly
JSON-addressable. The immediate shared base-cap change is in the addressable
part.

## Normal combat-realism defaults

The approved preset also disables both **Realistic Space Combat Scale** and
**Realistic Delta-V Usage** on Normal. The resulting matrix is:

| Difficulty | Realistic scale default | Realistic delta-V default |
| --- | --- | --- |
| Cinematic | On | On |
| Normal | Off | Off |
| Veteran | Off | Off |
| Brutal | Off | Off |

Terra Invicta hardcodes both vanilla defaults as
`lastSelectedDifficulty < 2`, where the dropdown is zero-based. This turns the
toggles on for Cinematic (`0`) and Normal (`1`). There is no JSON field for
either default.

Implement this with a postfix on
`StartMenuController.ResetCampaignDifficultyOptions()`. After the vanilla
reset, set both toggles without firing UI callbacks according to
`selectedDifficultyDropdown.value == 0`. This is narrower and easier to verify
than rewriting two comparison sequences in the original method. The vanilla
method has already cleared the custom-difficulty marker, and assigning the new
approved defaults without notifications preserves that preset status.

For the Long Campaign button, the complete vanilla path is
`OnLaunchLongCampaignClicked()` -> `SetDefaultCampaignOptions()` ->
`ResetAllCustomizations()` -> `ResetCampaignDifficultyOptions()`. The reset
clears `customDifficulty`, the postfix changes the two toggles without invoking
their callbacks, and campaign launch stores the still-false flag in
`ScenarioCustomizations`. Normal is stored as difficulty `2`. On victory,
`TINotificationQueueState.LogFactionWin()` awards `normalWin` when difficulty is
at least `2`, provided `ScenarioCustomizations.customDifficulty` is false.
Therefore the approved Normal defaults remain eligible for the Normal
difficulty achievement.

## Why the sparse override works

Terra Invicta loads the vanilla template list, finds mod files with the same
template filename, and matches objects by `dataName`. For each match it uses a
Newtonsoft `JObject.Merge` with ordinal property names. Scalars present in the
mod object replace the vanilla scalar; properties absent from the mod remain
vanilla. An explicit numeric zero is a value and is not treated as an absent or
null property.

The repository already ships a sparse `TIGlobalConfig.json` object with
`"dataName": "globalConfig"`. Adding the property there causes the merged
object to contain `controlPointMaintenanceFreebies: 0` before it is
deserialized into `TIGlobalConfig`. This remains effective even though the
vanilla 1.0.51 JSON omits that property, because the class's compiled 125
initializer is overwritten by the explicit merged value.

Array replacement settings in `ModInfo.json` do not apply: the change is a
scalar on one matched object.

## Runtime effect and limits

At campaign creation, the screen converts the slider value back into capacity
and stores it in `ScenarioCustomizations.controlPointMaintenanceFreebieBonus`.
Campaign initialization then copies that value into the saved global
`controlPointMaintenanceFreebies` value. This makes the change a new-campaign
setting; it should not retroactively alter an existing save.

The campaign screen's custom-difficulty check compares the slider with the
same modded global default. A zero slider therefore remains the recognized
preset default and does not add the custom-difficulty marker or, by itself,
disable difficulty achievements.

Zero base capacity does **not** mean a faction's total Control Point Capacity
is zero. The runtime total also includes:

- the difficulty-specific AI-only capacity bonus for non-player factions;
- councilor Control Point Capacity, principally Administration;
- capacity from habs; and
- effect modifiers.

It removes only the shared flat 125. The Veteran and Brutal AI bonuses remain
at their vanilla values unless separately changed.

The removed-faction compensation also remains 50 per missing human faction.
That is consistent with the narrow request to set the base to zero. If the
design intent is instead "no flat opening capacity under any faction-count
configuration," `controlPointBonusMaintenanceFreebiesPerRemovedFaction` must
also be overridden to zero. That should be a separate explicit decision.

## Implementation plan

Follow the repository's `Plan -> Document -> Implement -> Build -> Deploy ->
Test -> Document` sequence:

1. **Plan:** use the shared-zero interpretation, retain the existing
   removed-faction compensation, and disable both combat-realism defaults on
   Normal while retaining them on Cinematic.
2. **Document:** record the approved behavior here and add the implemented
   behavior to the current implementation matrix/README as appropriate.
3. **Implement:** add `"controlPointMaintenanceFreebies": 0` to the existing
   `globalConfig` override, then add the narrow campaign-screen postfix for
   the two Normal combat-realism defaults.
4. **Validate:** extend `tools/verify.ps1` so the global-config assertion
   requires the property to exist as an integer and equal zero. Prefer also
   testing a vanilla-plus-mod `JObject` merge so explicit zero regression is
   covered rather than only parsing the authored file.
5. **Build and deploy:** run `tools/deploy.ps1` without
   `-SkipVerification`. It will assert that Terra Invicta is closed, rebuild,
   run automatic validation, and mirror the package into the enabled mod.
6. **Manual test immediately after deployment:**
   - open a new standard campaign and cycle through all four difficulties;
   - confirm Base Control Point Capacity resets to `0` each time;
   - confirm no custom-difficulty marker appears;
   - confirm Cinematic resets both combat-realism toggles on, while Normal,
     Veteran, and Brutal reset both off;
   - start Long Campaign with the default Normal settings, confirm both combat
     toggles are off and no custom-difficulty marker appears, and treat the
     campaign as eligible for the Normal difficulty achievement;
   - select the accelerated preset and confirm it retains `0`;
   - start a seven-human-faction campaign and confirm the Control Point ledger
     reports a zero base component while councilor-derived capacity remains;
   - optionally remove one human faction and confirm the documented 50-point
     compensation still applies.
7. **Document:** record the deployed game version, automated results, and
   manual-test outcome. If the reduced-faction result is undesirable, make the
   compensation change a separate planned adjustment rather than silently
   expanding this one.

## Risk assessment

Implementation risk is low. The override uses the normal mod merge path and an
existing sparse template file. The main gameplay risk is intentional: early
Control Point Capacity will depend much more heavily on councilor
Administration, and Veteran/Brutal AI factions will still receive their preset
AI-only bonuses. The main compatibility risk is another mod overriding the
same scalar later in load order; the last merged value wins.

## Implementation record

The implemented package adds the explicit zero scalar to
`TIGlobalConfig.json` and applies
`CampaignDifficultyRealismDefaultsPatch.Postfix()` after Terra Invicta resets
the campaign controls. `CampaignDifficultyDefaults.EnableCombatRealism()` is
the tested zero-based difficulty rule.

`tools/validate-campaign-difficulty-patches.ps1` guards the installed 1.0.51
reset method's two vanilla `< 2` comparison constants and both toggle fields,
then verifies that the packaged postfix reads the selected difficulty, calls
the helper once, and performs exactly two non-notifying assignments. It also
guards the Long Campaign reset-and-launch call chain, the reset's
`DisableCustomDifficulty()` call, storage of that flag in
`ScenarioCustomizations`, Normal's one-based difficulty value, and the
`normalWin` achievement gate (`customDifficulty == false` and difficulty at
least `2`).
`tools/verify.ps1` also requires the explicit global-config property and value.

The normal `tools/deploy.ps1` flow completed successfully on 2026-08-20:

- campaign-difficulty target and packaged-patch IL validation passed;
- all 1,096 formula assertions passed, including the four difficulty cases;
- the 96-row implementation matrix passed its source/config/patch audit;
- release verification completed against the installed TI 1.0.51 assemblies;
- the package deployed 45 files to the enabled mod directory; and
- the deployed DLL SHA-256 was
  `A0A5AE661689AFEAA8CF1CBA158394979C94BB57DC1A569EF73A3AA848C1BDF0`.

Manual validation remains the final step: cycle all four new-campaign
difficulties, start Long Campaign on default Normal and confirm both realism
toggles are off with no custom marker, exercise the accelerated preset reset,
and start a seven-human-faction campaign to inspect the zero base-capacity
ledger component. A completed default-Normal Long Campaign remains eligible
for `normalWin`; the automated IL guard covers that otherwise impractical
end-to-end achievement check.
