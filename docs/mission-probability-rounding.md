# Mission probability rounding — 0.10.3

## Plan and behavior contract

Apply one probability policy to contested councilor missions, including alien
missions. Automatic missions, fleet operations, and combat hit chances are
outside this change.

1. Floor the calculated success probability to 0.1 percentage-point steps:
   `0.305% -> 0.3%` and `0.099% -> 0%`. The exception is a raw chance strictly
   above 99.95%, which becomes 100% before flooring; exactly 99.95% still
   becomes 99.9%. Compare at the game's float precision without an epsilon.
   Use the same value for AI evaluation, UI, and resolution.
2. Compare an unrounded random roll against that probability, using a strict
   success boundary. Zero chance must fail even when the RNG returns zero.
   Preserve the native critical-outcome proportions and turned-agent sabotage.
   Promoted 100% chances still require a favorable critical-success roll: the
   existing strict `roll < chance / 10` rule gives a 10% critical-success band,
   with ordinary success otherwise. Do not round or promote the random roll.
3. Show one decimal place below 1% and above 99%; retain whole percentages in
   the middle and exact 0%/100% endpoints, even when extra precision is requested.
   Display the effective chance without further upward rounding.
   Result notifications may retain extra precision to distinguish roll/chance.
4. Exclude targets whose chance remains zero with maximum currently affordable
   bonus spending. Keep a target available when spending can make it possible;
   disable confirmation at a selected spend that still gives zero. Recheck at
   assignment so AI objectives, repeats, and alternate UI routes cannot spend
   resources on zero-chance orders. Abort zero-chance AI missions already queued
   in saves through the native abort path.
5. Raise manifest, assembly, loader log, verification, and package version from
   0.10.2 to 0.10.3. Preserve existing unrelated changes.
   The subsequent >99.95% certainty adjustment retains version 0.10.3.

## Implementation and validation plan

Patch the shared contested probability and outcome methods; both chance-string
overloads; concrete mission target validators; assignment and AI risk/abort
checks; the mission confirmation UI; and direct chance formatting in the space
councilor marker and result notification. Evaluate target availability without
recursive target revalidation. Account for resources already committed to the
current mission when validating it.

Validate floating-point boundaries, flooring idempotence, zero-roll failure,
rare success and near-certain failure, 100% behavior, formatting, affordable
boosts, native invalid-target preservation, disabled-mod behavior, and Harmony
bindings against the installed TI 1.0.53b assembly. Run `tools/deploy.ps1` for
the build, full verification, and process-guarded deployment.

## Manual checks

- Inspect a mission below 1% and above 99%; confirm 0.1% precision in target
  lists, mission pane, markers, and new result notifications.
- A raw chance strictly above 99.95% must show `100%` and succeed on normal
  rolls, without making every outcome a critical success.
- A target at zero even with maximum affordable spending must be unavailable.
- A target rescued by spending must remain selectable; confirmation must become
  available only once the resource slider produces at least 0.1% success.
- Ordinary missions and automatic missions must remain selectable normally.
- Advance an existing save with an impossible AI order; verify normal abort
  handling and no free success. Check the log for patch/runtime exceptions.

No save schema or stored ownership changes are required. Existing notification
text is not rewritten. Numerical behavior changes on loading the new mod.

## Status

The >99.95% certainty adjustment is implemented and deployed without changing
version 0.10.3. The normal full deployment verification passed again, including
the strict 99.95% boundary and its adjacent representable floats. An additional
10,000-roll test through the patched outcome method produced 1,000 critical
successes and 9,000 ordinary successes at a promoted chance, preserving every
unrounded roll. The critical threshold and its adjacent floats, both chance-text
overloads, and the RNG's exact upper endpoint also passed.

Implemented and deployed as 0.10.3 against the installed TI 1.0.53b assemblies.
The normal `tools/deploy.ps1` flow completed successfully, including the game
process guards, release verification, packaging, and deployment of 47 files.
The full verification included 1,204 formula assertions.

The focused `tools/validate-mission-probability.ps1` validator exercises
`tools/validation/MissionProbabilityValidation.cs` against the built mod and
installed game assemblies. It passed:

- All 999 interior probability-step boundaries and their adjacent float values,
  plus flooring idempotence.
- Deterministic roll samples verifying effective probabilities, strict zero
  failure, exact 100% success, and preserved turned-agent sabotage.
- Fractional and whole-percent formatting, including exact `0%` and `100%`
  even when the caller requests additional decimal places.
- Both chance-string overloads, affordable and already-paid resource boosts,
  native invalid-target preservation, and non-recursive target revalidation.
- AI risk/abort checks and rejected assignment without changing the existing
  order or reaching native assignment side effects; disabled-mod behavior.
- 27 Harmony method bindings and the expected direct-formatting IL call sites.

The standalone validator cannot JIT Unity-native UI methods. The confirmation
refresh, confirmation-click guard, and direct UI/notification formatting patch
classes therefore still require the manual checks above inside the game; their
runtime UI behavior is not claimed as tested by the standalone harness.

Implementation: `TIEconomyMod/Patches/MissionProbabilityPatches.cs`. No save
migration is needed. On 2026-10-01, the user confirmed that the fix landed and
requested committing and pushing the release. This is user-reported acceptance;
individual manual checklist results were not recorded.
