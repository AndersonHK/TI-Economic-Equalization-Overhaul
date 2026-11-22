# Allied Federation and unification plan

Planning proposal for Terra Invicta 1.0.65, 2026-11-16.

## Federation acceptance

An eligible invitation to form or join a federation should receive 100% AI
acceptance when all of the following are true.

- The proposing nation and the responding nation are allies. Their executive
  factions have both an active Non-Aggression Pact and an active Information
  Sharing agreement. Use `HasNAP(otherFaction, includeToBeDiscarded: false)`
  for the pact. The game stores ongoing sharing directionally in each
  recipient's `intelSharingFactions` list. Require the entries in both
  directions for mutual Information Sharing. A one-time intel exchange
  does not establish that agreement. Keep the existing same-faction and
  alien/proxy acceptance paths.
- The responding nation is a minor power. Use the game's
  `!TINationState.SignificantPower` as the proposed definition. It includes
  three-control-point Vietnam and uses the game's existing criteria when a
  country grows.
- The proposing nation has a live claim on the responding nation's capital.
  Follow the game's current/original-capital setting and allow a currently
  hostile claim for this check. Federation already has its own harmonization
  test.
- The responding executive faction controls no other extant nation with a
  live claim on that same capital. Scan the faction's executive nations and
  exclude the responding nation itself. Count hostile capital claims as
  competing claims because they may become usable for unification later.
  Use claims present in the campaign, so project completion and changes of
  executive control take effect without country-specific rules.

Apply the exception at `StratPolicyResponseSelector.ChanceFederation`, which
`JoinFederationOption.AIAgreeChance` calls. Today's scorer returns 0 for a
foreign executive faction after the same-faction and alien/proxy checks. Leave
that result in place when the exception fails, and retain the native formula
for an uncontrolled executive. Verify both the displayed odds and the AI
reply. A human-controlled executive gets the existing response prompt.

Offer eligibility still runs through `TINationState.CanFormFederation` or
`TIFederationState.CanAddNation`, including the existing best-direction
harmonization limit of 12. For an invitation from a federation, evaluate
the lead nation's claim and the prospective member's executive. A request
from a minor to join an existing federation does not qualify merely because
another member could claim the applicant.

| Invitation | Proposed AI reply, assuming eligibility and both agreements |
| --- | --- |
| European Union → Switzerland | 100% when the Swiss executive has no other executive nation claiming the Swiss capital. |
| Greater India → Vietnam | 100% when the Vietnamese executive has no other executive nation claiming Hanoi. |
| South American Federation → Venezuela | Keep the refusal when the Venezuelan executive also controls Greater Brazil and it holds the competing capital claim. |
| North American Federation → Mexico | Keep the refusal when the Mexican executive also controls the Latin American Federation and it holds the competing capital claim. |

## Unification after federation

In the federated branch of `TINationState.eligibleUnifications`, replace the
comparison to `target.TotalOwningFaction` with a check of every target
control point, including the executive. Each point needs an owner. The
owner can be the unifier's executive faction or a faction that has both an
active Non-Aggression Pact and mutual Information Sharing with it.
Check each owner against the unifier directly using the same treaty predicate.
An unowned point or an owner without that relationship fails the check.

Use this predicate in the corresponding federated line of
`TINationState.CanUnifyFeedback` and update the wording. Keep the branch's
existing federation membership, non-hostile capital claim, bilateral
cooldown and consolidated-executive checks. The breakaway paths keep their
own rules. Evaluate current owners and treaties whenever eligibility is checked.

`UnificationOption.GetPossibleTargets` already uses the eligibility list,
but a different executive faction can receive a confirmation prompt and
`ChanceUnification` currently returns 0 for that executive. Give the AI a
100% response for a currently eligible federated unification that passes the
allied ownership rule. Keep human confirmation and recheck current
eligibility before executing the newly allowed cross-faction merger.
`AbsorbNation` clears the absorbed nation's points, including allied points.
The merger uses the existing rules for the surviving nation's points.

## June 16, 2044 save

Read-only inspection of `Escapesave00358_2044-6-16.gz` in the 2026 campaign.
`Exitsave.gz` was byte-identical. The save records game version 1.0.65.

- Project Exodus holds the Indian executive. Vietnam's three points are
  Resistance, Protectorate, Resistance in point order. India and Vietnam
  are national allies and have no federation membership in this snapshot.
  Exodus and the Resistance have an active Non-Aggression Pact and both
  record Information Sharing with each other. Exodus has neither agreement
  with the Protectorate.
- India claims Hanoi and Ho Chi Minh City. No other extant
  Resistance-executive nation claims Hanoi, including Indonesia, which the
  Resistance does control. China has a Servant executive. Vietnam also
  claims Phnom Penh and Vientiane, both currently Indian regions.
- The bilateral cooldown in the save expires on July 2, 2044. After it expires,
  the saved claims and national stats provide a federation score of about
  **7.30** from Vietnam's claims in India, within the limit of **12**. The
  reported **0%** matches the game's foreign-executive response rule.
- Taking the Protectorate point for Exodus leaves only Exodus and Resistance
  ownership, satisfying the proposed ownership check while both treaties
  are active.
  The Hanoi claim runs in the opposite direction and scores about **15.63**
  against the ordinary unification limit of **6**. Waiting out the
  post-federation cooldown and taking the point alone cannot guarantee
  unification at those stats. The plan retains harmonization. An exemption
  for an allied executive's consent would be a separate design decision.

## Acceptance tests

1. Load the June 16 save, wait out the current cooldown, and offer the
   federation to Vietnam. Verify a 100% preview, an accepted reply and actual
   federation membership with the Vietnamese control points still held by
   their respective factions.
2. After federation, confirm the Protectorate point fails the ownership
   check. Capture that point for Exodus and confirm the ownership check
   passes while the Resistance retains the executive and its other point
   and both treaties are active.
   Verify the cooldown and claim checks independently in the feedback.
   Once the capital claim passes harmonization and the cooldown clears,
   verify the unification action becomes available and absorbs Vietnam.
3. Cover both example pairs for competing claims. Isolate the veto with a
   qualifying minor in an automated case so it exercises the claim check
   even if a campaign's example target qualifies as a significant power.
   Also cover an unclaimed capital, a regional-only claim, a significant-power
   target, same-faction acceptance and the uncontrolled executive formula.
   Check that a pact alone, Information Sharing alone, and a pact with
   one-way sharing all leave the AI refusal in place.
4. For unification, cover a fully owned target, points split across the
   unifier and a treaty partner, all target points held by the treaty partner,
   an unowned point, and a point held by a faction missing either agreement.
   Confirm that either agreement ending before execution removes the
   exception, that the AI planner and player UI agree, and that a human
   executive still chooses how to respond.

The existing [harmonization design](national-harmonization-eligibility-plan.md)
documents the claim scoring and federation eligibility rules.
