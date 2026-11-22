# Shared implementation audit

The [repository rule](../../AGENTS.md#one-implementation-for-each-system)
requires one implementation for all consumers of a system EEO changes.
This inventory records concrete splits in the checked-out source and
the installed Terra Invicta 1.0.65 call paths.

## Confirmed splits

| System | Current split | Retroactive fix and verification |
|---|---|---|
| Fleet transfer planning | The 0.11.15 plane-change branch intercepted model selection and the inclination solver for a subset of routes involving real fleets. Other routes and virtual assets went through the native planner. The stock quick request could stop before the final fuel and duration checks. | 0.11.19 owns the request workflow for all fleets and validates candidates before a quick request succeeds. The exact three LEO choices passed, and the reference ordinary Earth and Mars routes matched stock. [Shared planner](../../TIEconomyMod/Core/FleetTransferPlanner.cs), [validation](../orbits-and-lunar-resources/approved-design.md#unified-planner-implementation-checkpoint). |
| Fleet transfer estimates | Native shipyard selection used a separate transfer estimate. Other AI paths used virtual fleets and `RequestTrajectories`. `AIEvaluators.GetEstimatedTransferTime_days` additionally scales the returned time by 0.8 for evaluation. | 0.11.19 uses the shared solver for the raw ETA and feasibility, with real-fleet and virtual-fleet parity checks. The downstream AI scoring multiplier remains a follow-up audit. [Request adapters](../../TIEconomyMod/Patches/InclinedOrbitTransferPatch.cs), [validation](../orbits-and-lunar-resources/approved-design.md#unified-planner-implementation-checkpoint). |
| Armor selection | `TIFactionState.GetBestArmor` uses EEO's selection for aliens and native selection for human factions. Both implement the same equipment-selection decision with separate logic. | Give all factions one selector with species and role eligibility supplied as policy data. Capture native human choices and accepted alien choices before moving either through the common selector. [Armor selection patch](../../TIEconomyMod/Patches/AlienArmorSelectionPatches.cs). |
| Army combat and logistics | With EEO army rules enabled, invalid mod results can return to the stock construction, upkeep, attack, defense, or hit-chance formula. This makes invalid-state handling choose a different rules implementation mid-operation. | Validate inputs at the shared boundary. Define deterministic handling for missing and invalid state under the active rules. Test construction and upkeep quotes against charges and combat odds against resolution. [Army patches](../../TIEconomyMod/Patches/MilitaryPatches.cs). |
| Global technology selection | The soft-selection hook sends invalid inputs to the native selector while the EEO system is enabled. | Define how the common selector handles missing scores, unusable candidates, and ties. Use the same candidate scoring for weighted and deterministic requests. Verify empty and partial lists and valid seeded selections. [Technology selection patch](../../TIEconomyMod/Patches/AiTechnologySelectionPatches.cs). |

## Verify before consolidating

- **Combat contact.** Prepared projectile queries and native raycasts
  can participate in the same enabled battle, and a native fallback
  body sends nearest-missile queries through the all-hit path. Compare
  the same ray, collider, pose, masks, and hit order across worker,
  compatibility, and fallback paths. If the result differs, centralize
  eligibility and nearest-hit selection and adapt unsupported geometry
  to that contract. Keep the existing battle performance checks.
  [Prepared contact paths](../../TIEconomyMod/Core/PreparedContactWorld.cs).
- **Fleet notification eligibility.** Ship completion and alarms own
  distinct UI candidate pools. 0.11.19 fixes the shipyard locality and
  alarm button index. The pools express action-specific
  eligibility, while both use the planner's authoritative feasibility
  result. The player's successful manual transfers provide the
  comparison cases. [Notification investigation](../orbits-and-lunar-resources/approved-design.md#ship-completion-and-sortie-shortcuts).
- **Logistics estimates.** Hab exports and Boost launch costs use
  generic transport calculations. Those quantities price abstract
  cargo transport. Audit callers during the transfer migration so
  a generic logistics result cannot stand in for the feasibility or
  arrival time of a specific fleet. [Hab logistics](../../TIEconomyMod/Core/HabLogistics.cs),
  [Earth launch costs](../../TIEconomyMod/Core/EarthLaunchCost.cs).

Expand this inventory when changing a system. Trace its public and
native entry points, identify which code makes the gameplay decision,
and use the same input fixture across callers to check agreement.
Differences in display, request purpose, or performance backend
should remain outside the calculation that establishes the result.
