# Save-load crash: no exotics-free alien armor — 2026-11-04

Status: **diagnosis confirmed; fixed in the subsequent alien-only 0.11.2
[selection update](alien-armor-selection-fix-2026-11-04.md).** The investigation
below records the original unpatched behavior.
Observed with EEO 0.11.2 / TI 1.0.53b while loading
`Appeasesave00240_2051-12-31.gz`.

## Fatal path

Player.log records successful save deserialization, capital migration of
45 designs / 155 built ships, and canvas initialization including the fleet
designer. The fatal exception occurs afterward during habitat initialization:

```text
NullReferenceException
TIHabModuleTemplate.SpaceCombatValue_Base [0x000a7]
TIHabModuleTemplate.SpaceCombatValue
TIHabModuleState.SetSpaceCombatWeapons
TIHabModuleState.PostInitializationInit_4
SolarSystemBootstrap.LoadGame
```

The installed assembly's instruction at **IL_00a7** calls
`TIShipArmorTemplate.get_mass_damagePoint_kg()` on the preceding
`TIFactionState.GetBestArmor(bool)` result. This identifies the null armor
selection, rather than a missing weapon or a ship-grid exception.

For alien ground bases, the caller permits exotic armor only on the natural
body containing the faction's primary habitat. Elsewhere it calls
`GetBestArmor(false)`. That native method filters out every armor whose
`buildCost(1).GetSingleCostValue(Exotics)` is nonzero, then selects the lightest
remaining armor. An empty result returns null; the caller dereferences it.

## Trigger and native reproduction

The recent Alien Adamantane material update changed its exotics weight from
**0 to 0.005**. Alien Exotic already has weight 0.01. Consequently neither
alien armor remains eligible for the native no-exotics branch.

A read-only fixture used the installed assembly, real armor template
deserialization, native build costs and actual `GetBestArmor`, with both alien
armors available and an empty obsolete set. Only required initialization
templates/global conversion scale were supplied; no Unity scene or save state
was run or modified.

| Data | allowExotics | Native result |
| --- | --- | --- |
| Vanilla | false | AlienAdamantaneArmor |
| Vanilla | true | AlienExoticArmor |
| Current overrides | false | **null** |
| Current overrides | true | AlienExoticArmor |

The loaded save contains 17 alien ground bases, 14 on bodies other than the
primary habitat's body. The exact failing habitat/module instance is not named
in the exception log. The native null-selection regression and matching fatal
instruction are independently established.

The approved material change is documented in
[the armor implementation](alien-armor-material-update-2026-11-04.md). Resolving
the crash requires reconciling the exotic-bearing baseline armor with native
no-exotics selection. Preserving the approved recipe requires an explicit,
valid baseline choice for this path; changing the recipe back would alter the
approved material costs. The current investigation does not choose or apply
either gameplay-policy change.

## Evidence retained

Diagnostic working files under `.tmp/save-load-crash-2026-11-04/` contain the
captured Player/ModManager logs, installed assembly IL, reproduction script and
four-result JSON. Original logs were read from LocalLow/Pavonis Interactive/
TerraInvicta/Player.log and the installed UnityModManager/Log.txt. The source
save and its migration backup were not edited. No rebuild or deployment was
performed for this diagnosis.
