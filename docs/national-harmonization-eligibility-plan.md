# National harmonization eligibility plan

Status: proposed, not implemented. Analysis targets Terra Invicta 1.0.51 and
Economic Equalization Overhaul 0.9.2 as installed on 2026-08-15.

## Requested behavior

For an annexing/source nation `S` and an annexed/target nation `T`, the working
interpretation of the requested formula is:

```text
governmentDifference = abs(S.government - T.government)
inequalityDifference = abs(S.inequality - T.inequality)
knowledgeDifference = abs(S.knowledge - T.knowledge)
gdpRatio = max(S.GDP / T.GDP, T.GDP / S.GDP)

f = governmentDifference
  + inequalityDifference
  + knowledgeDifference
  + gdpRatio

modifier = (10 - T.unrest) / 10 + (10 - S.cohesion) / 10
harmonizationScore = modifier * f

claimThreshold = claim.hasHistoricalHostility ? 6 : 12
claimIsNonHostile = harmonizationScore <= claimThreshold
```

The sum defining `f` is an assumption. The request lists four components but
does not yet state their aggregation or weights. Both thresholds are inclusive:
an ordinary claim may score at most `12`, while a claim marked
`hostileClaim: true` may score at most `6`. Historical hostility therefore
makes peaceful integration substantially harder without making it permanently
impossible.

## Current game path

The existing `1.5` check is indirect rather than a dedicated unification rule:

1. `TINationState.HostileClaimDueToDemocracy(region)` returns true when the
   target region's nation has Government greater than the claimant's Government
   plus `TIGlobalConfig.democracyDecreaseToMakeHostileClaim` (`1.5`).
2. `ClaimWillBeHostile` uses that result for an otherwise ordinary claim.
3. `MyClaimOnOtherCapital(..., includeHostile: false)` rejects the resulting
   hostile capital claim.
4. `candidateUnifications` and `eligibleUnifications` call that method for
   federated countries. The eligible list also enforces same-faction control,
   the relations cooldown, and consolidated executive control in both nations.
5. `UnificationOption.GetPossibleTargets` exposes `eligibleUnifications` to the
   policy UI. Faction AI also consumes the same list, so changing the list keeps
   player and AI eligibility aligned.
6. `CanUnifyFeedback` calls the same methods but includes a hardcoded
   Government-difference explanation that will need replacement.
7. Once selected, `UnificationOption.OnPassage` calls `Unification`, which calls
   `AbsorbNation`; execution does not independently apply the `1.5` check.

The repository already patches `AbsorbNation` in
`TIEconomyMod/Patches/NationalMergerPatches.cs`, but only to recompute the final
merged Inequality. Military integration is handled separately. There is no
current eligibility patch.

## Scope decision

The revised scope is all human claim hostility, not only federated peaceful
unification. The harmonization score replaces both the one-directional
Government comparison for ordinary claims and the unconditional veto for
historically hostile claims. This requires patching `ClaimWillBeHostile` as the
authoritative decision point; patching `HostileClaimDueToDemocracy` alone cannot
work because vanilla returns hostile before calling it when the region is in
`hostileClaims`.

This broader hook intentionally changes every system that asks whether a human
claim would be hostile:

| Consumer | Revised behavior |
|---|---|
| Claims map and region list | Orange/hostile presentation follows the resolved threshold (`12` ordinary, `6` historical) rather than Government hierarchy or the raw stored flag. |
| Peaceful unification | A capital claim is peaceful only at score `<= 12`, or `<= 6` when historically hostile. |
| Peaceful non-capital transfer | `TransferRegionsOption` applies the same claim-specific threshold. |
| Federation formation and entry | Existing claim, alliance, cooldown, and control requirements remain, plus at least one claim link connecting the prospective member to the federation must score `<= 24`. Historical status does not lower this federation ceiling. |
| Federation claim overlays | `nonHostileClaims` and federation map hints use the claim's `12` or `6` threshold. |
| Secession and involuntary transfer | Candidate claimed regions use harmonization when deciding whether transfer to the claimant is non-hostile. |
| Forced territorial transfer | A claim acquired above its applicable threshold becomes or remains a stored hostile region, creating Cohesion/Unrest integration costs until Government or Unity investment legitimizes it. A historical claim acquired at score `<= 6` must not retain that burden. |

The following rules remain unchanged:

- regions without any claim are hostile;
- ordinary claims use threshold `12`;
- claims marked historically hostile use threshold `6` and can overcome that
  history through sufficiently close harmonization;
- breakaway/parent reunification keeps its vanilla claim exemptions;
- alien-specific handling remains vanilla;
- alliances, cooldowns, executive control, wars, and all other federation and
  unification policy requirements remain in force alongside the new score gate.

This scope avoids treating high Government as universal liberator status. The
Government contribution is now an absolute institutional difference, so being
more democratic than the target no longer grants a one-way annexation license.
High Cohesion still models the annexer's ability to carry out integration, but
its normalized contribution is bounded and must compete with differences in
Government, Inequality, Knowledge, and economic scale. That also prevents the
recent pro-Cohesion effects of high Government from independently restoring the
old high-democracy blobbing behavior.

The Russia-to-Ukraine starting-value result (`9.86`) illustrates the two-tier
rule. It passes the ordinary threshold of `12`, but fails the historical
threshold of `6`. Russia's high Cohesion still performs its intended integration
capacity role without erasing the invasion grievance.

The installed 1.0.51 data demonstrates both the intended pattern and a data gap.
`ClaimETHEritrea` has `hostileClaim: true`, so Ethiopia must reach the stricter
score of `6` to make its Eritrean claim non-hostile. The 2022 scenario starts
Russia and Ukraine in `WarRUSUKR`, but `ClaimRUSDonetsk`, `ClaimRUSKharkiv`,
`ClaimRUSKiev`, and `ClaimRUSOdesa` are ordinary claims without the historical
hostility flag. The active war blocks immediate peaceful unification, but that
does not provide the intended persistent grievance after peace. The first
implementation should therefore mark those four claims historically hostile.

This two-layer interpretation also models the suggested historical distinction:
a pre-invasion political realignment toward Russia could plausibly have followed
an ordinary-claim path if the countries converged and competing federation
attraction did not prevail, while the invasion creates a durable grievance that
requires much deeper convergence to overcome.

## Historical-claim data audit

### Approved additions

Apply the historical threshold to the corresponding duplicated records in each
scenario where the claim is external:

| Relationship | Data action |
|---|---|
| Russia -> Ukrainian regions | Set the Donetsk, Kharkiv, Kiev, and Odesa claim records to `hostileClaim: true` in the base, 2026, and 2070 datasets. |
| China -> Taiwan | Set `ClaimCHNTaiwan` and `Claim2026_CHN2026_Taiwan` to historical-hostile. In 2070 China already owns Taiwan and its owner claim is already marked hostile. |
| Pakistan -> India | Set the Pakistan claim on Jammu and Kashmir to historical-hostile in the base, 2026, and 2070 datasets. India's Greater India claims on Baluchistan, Peshawar, Punjab, and Sindh are already `hostileClaim: true` in all three datasets. |
| Both Koreas | Mark North Korea -> South Korea and South Korea -> North Korea historical-hostile. |
| China -> India | Mark China's Arunachal Pradesh claim historical-hostile. The 2070 record is already flagged; align earlier applicable scenarios. |
| Russia -> skeptical neighbors | Mark the claims on Georgia, Moldova, Estonia, Latvia, and Lithuania historical-hostile. These require threshold `6` even if a future Russia becomes moderately democratic and prosperous. Belarus and Central Asian claims remain ordinary unless separately approved. |
| Venezuela -> Guyana | Mark the Guyana claim historical-hostile. |
| Japan -> Russia | Mark the Sakhalin/Kurils claim historical-hostile. |
| Syria -> Lebanon | Mark the Lebanon claim historical-hostile. |
| Eritrea -> Ethiopia | Mark the Mekelle claim historical-hostile. Ethiopia -> Eritrea is already flagged. |
| Guatemala -> Belize | Mark the Belize claim historical-hostile. |
| Expansion-project families | Normalize all external claims granted by `RestoredWarsawPact`, `ForwardRussia`, and `LiberatingMainlandChina` to historical-hostile. Preserve existing flags and add the missing ones consistently across scenario variants. |

Interpreting approval of the project-family recommendation as full
normalization adds `7` logical Restored Warsaw Pact flags, `3` Forward Russia
flags, and `26` Liberating Mainland China flags per scenario family. Across the
installed 2003, 2022, 2026, and 2070 records, that is `144` currently unflagged
records. Keep this count in data validation so a game update cannot silently
expand or shrink the override set.

Do not automatically mark Ukraine's claim on Crimea historical-hostile as part
of the Russia-to-Ukraine change. Claim direction represents expected local
resistance to the claimant, so the two directions need not be symmetrical.

Rivalry by itself is not sufficient. `hostileClaim` should represent persistent
resident resistance to rule by the claimant, not merely poor diplomatic
relations or a disputed border.

### Scenario-date verification

Historical hostility must be scenario-specific rather than blindly copied
between starts. In particular, Russia's Ukrainian claims remain ordinary in a
2003 start because it predates the 2014 invasion. A campaign beginning in 2003
must not acquire that later grievance merely because the calendar reaches 2014;
only events that actually occur in that campaign should change claim history.

The Dark Skies DLC stores its start in
`DLC_Content/DarkSkies/2003_Scenario/Templates` and uses separate `2003_*`
nation, region, and bilateral identifiers. Its start date is March 31, 2003, so
scenario-specific overrides can be explicit without calendar inference:

- Keep `Claim2003_RUS2003_Donetsk`, `Claim2003_RUS2003_Kharkiv`,
  `Claim2003_RUS2003_Kiev`, and `Claim2003_RUS2003_Odesa` ordinary. They are
  ordinary in the DLC data now and must remain so.
- Keep the four 2003 Greater India claims on Pakistan historical-hostile; they
  are already flagged in the DLC.
- Add the missing 2003 historical flags for China/Taiwan,
  Pakistan/Jammu-and-Kashmir, both Koreas, China/Arunachal Pradesh,
  Russia/Georgia, Russia/Moldova, Russia/Estonia, Russia/Latvia,
  Russia/Lithuania, Venezuela/Guyana, Japan/Sakhalin-Kurils, Syria/Lebanon,
  Eritrea/Mekelle, and Guatemala/Belize.
- Normalize the DLC variants of `RestoredWarsawPact`, `ForwardRussia`, and
  `LiberatingMainlandChina` in the same way as the later scenarios. The DLC
  currently has respectively `7`, `3`, and `26` unflagged records in those
  project families.

At 2003 starting values, Russia -> Ukraine scores approximately `14`. Keeping
the claims ordinary therefore allows federation under the `24` ceiling but not
peaceful integration until the score reaches `12`. China -> Taiwan scores about
`32` and cannot initially federate; India -> Pakistan and Pakistan -> India
score about `15` and `16`, allowing federation but not historical integration.

Never edit the installed DLC files. Ship scenario-specific template overrides
or register the classification from the loaded `2003_*` identifiers.

Dark Skies content has an explicit game-owned gate. Its `2003Scenario` meta
template declares `requiredDLC: ["DarkSkies"]`, and the game validates required
DLC names against `ModManager.dlcNames`. Runtime registration must wait until
`GameControl.DLCValidated`, require `ModManager.dlcNames.Contains("DarkSkies")`,
and require the active scenario's `requiredDLC` to contain `DarkSkies` before
resolving any `2003_*` identifiers. `GameControl.DLCValidated` alone is not an
ownership check. When the DLC is absent or a non-Dark-Skies scenario is active,
the 2003 registration path is skipped completely and must not log missing
template warnings.

## Federation harmonization gate

Federation formation and entry use a third inclusive threshold:

```text
federationEligible = bestQualifyingClaimLinkScore <= 24
```

The qualifying links are the same cross-boundary claims vanilla already uses to
permit federation. For two standalone nations, evaluate every claim in either
direction between them. For entry into an existing federation, evaluate claims
from any member onto the prospective nation and claims from the prospective
nation onto any member. Score each link in claim direction—claimant as source,
current region owner as target—and use the lowest valid score. All existing
alliance, enemy, cooldown, executive-control, breakaway, and claim requirements
still apply.

Using the actual claim links avoids inventing an annexer for a federation of
nominal peers. It also avoids scoring only against the current federation lead,
which interacts badly with the unbounded total-GDP ratio: at starting values,
Germany -> Estonia scores roughly `144` despite being a plausible federation
relationship. A compatible claim-bearing member may instead provide the link,
while later unification still requires the stricter score on the specific
capital claim.

Historical designation does not change the federation threshold. This produces
a deliberate progression:

| Claim | Score | Harmonization result |
|---|---:|---|
| Historical | `<= 6` | May federate and is non-hostile for peaceful integration. |
| Historical | `> 6` and `<= 24` | May federate but remains hostile for integration. |
| Ordinary | `<= 12` | May federate and is non-hostile for peaceful integration. |
| Ordinary | `> 12` and `<= 24` | May federate but cannot integrate peacefully. |
| Either | `> 24` | Cannot form or enter the federation through that claim link. |

A starting-value scan of the 88 ungated external claim directions in the base
scenario found 51 (`58.0%`) at or below `24`. This is not an eligibility
forecast—it omits alliances, projects, federation membership, and cooldowns—but
it indicates that `24` is meaningfully restrictive without reducing federation
availability to only already-integrable pairs. The same scan over the Dark Skies
2003 data found 44 of 82 links (`53.7%`) at or below `24`.

## Formula assessment

### Both domestic terms are normalized

Target Unrest and missing source Cohesion each contribute `[0,1]`, so the
combined multiplier ranges from `0` to `2`:

| Source Cohesion | Target Unrest 0 | Target Unrest 5 | Target Unrest 10 |
|---:|---:|---:|---:|
| 10 | 1.0 | 0.5 | 0.0 |
| 8 | 1.2 | 0.7 | 0.2 |
| 5 | 1.5 | 1.0 | 0.5 |
| 3 | 1.7 | 1.2 | 0.7 |
| 0 | 2.0 | 1.5 | 1.0 |

At source Cohesion 3 and target Unrest 0, `f` must be at most approximately
`7.06`. At source Cohesion 8 and target Unrest 3, as in the Russia-to-Ukraine
starting-value example below, `f` may be as high as approximately `13.33`.

### Total GDP ratio is a strong size penalty

The symmetric ratio is dimensionless and direction-independent, but it grows
without bound. It will normally dominate the three bounded score differences
when one nation is much larger than the other. It also means some familiar
large/small unions cannot pass at low target Unrest even with perfect source
Cohesion.

Using starting-template values and the additive interpretation of `f`:

| Source -> target | Core `f` | Modifier | Score |
|---|---:|---:|---:|
| USA -> Canada | 13.25 | 1.70 | 22.53 |
| China -> Taiwan | 26.06 | 1.20 | 31.27 |
| Russia -> Ukraine | 10.96 | 0.90 | 9.86 |

These are formula examples, not assertions that every pair is immediately a
legal in-game unification candidate.

A broad sensitivity scan over the 414 base nation-template records with
complete positive GDP and social values produced these ordered-pair pass rates:

| Rule | Pass rate |
|---|---:|
| Current Government-only comparison | 66.5% |
| Corrected harmonization formula | 29.0% |

This scan deliberately ignores claims, federation membership, control, and
cooldowns, so it is not a gameplay eligibility forecast. It demonstrates the
formula's relative selectivity.

### Directionality and boundary behavior

The core is symmetric, but Unrest and Cohesion make the final score directional.
That is appropriate if annexer capacity and target instability are meant to
matter separately.

Notable boundaries are:

- equal GDP and equal social scores give `f = 1`, not zero;
- source Cohesion 10 plus target Unrest 10 gives a zero multiplier, allowing
  any finite disparity;
- higher target Unrest always makes annexation easier;
- higher source Cohesion always makes annexation easier;
- at exactly `12`, annexation is allowed;
- at exactly `6`, a historical claim is allowed;
- at exactly `24`, a qualifying claim link permits federation;
- a historical score above `6` and at or below `12` remains hostile even though
  the same score would pass for an ordinary claim;
- zero, negative, NaN, or infinite GDP must fail closed rather than divide by
  zero or create an accidental zero score.

The live scores should be evaluated when the target list and feedback are
requested. No score should be saved, because Government, Inequality, Knowledge,
GDP, Unrest, and Cohesion all change during play.

## Implementation plan

Follow the repository's `Plan -> Document -> Implement -> Build -> Deploy ->
Test -> Document` workflow.

### 1. Plan: confirmed mathematical contract

The initial implementation uses the following confirmed contract:

1. `f` is the unweighted sum shown above, rather than a product, mean, weighted
   sum, or normalized distance.
2. GDP means total `TINationState.GDP`, not per-capita GDP.
3. Ordinary claims use an inclusive threshold of `12`; claims designated
   historical-hostile use an inclusive threshold of `6`.
4. A passing historical claim is peaceful and must not create or retain a
   hostile-region integration burden after transfer.
5. Federation formation or entry requires at least one actual cross-boundary
   claim link with an inclusive score of `24` or less, irrespective of whether
   that claim is ordinary or historical.

### 2. Document: establish authority before coding

- Add the confirmed equation, source/target terminology, inclusive threshold,
  and scope to `docs/design-directives.md`.
- Mark this document as approved and update
  `docs/national-social-coefficients-report.md` with the new cross-stat use.
- Reserve implementation-matrix rows for the dynamic-claim rule, player-facing
  explanations, and downstream peaceful-transfer/unification behavior.

### 3. Implement: isolate math from Harmony

- Add `Core/NationalHarmonizationMath.cs` with a pure calculator returning the
  component differences, GDP ratio, core, modifier, total score, and validity.
- Use `double` for GDP ratios and the aggregate, clamp only the already bounded
  `[0,10]` Unrest/Cohesion inputs defensively, and fail closed on non-finite or
  non-positive GDP.
- Add a top-level `ClaimHarmonizationSettings` group rather than placing a
  global claim rule under `NationalMergerSettings`. At minimum it needs
  `enabled`, `ordinaryThreshold = 12`, `historicalThreshold = 6`, and
  `federationThreshold = 24`; validate every threshold as finite and
  non-negative, require the historical threshold not to exceed the ordinary
  threshold, and require the federation threshold not to be lower than either
  integration threshold. Keep weights out until weights are actually part of
  the approved design.
- Add one authoritative claim evaluator returning score, applicable threshold,
  historical status, and resolved hostility. Preserve template historical
  classification separately from the mutable runtime `hostileClaims` list so a
  peacefully integrated region can lose its unrest burden without erasing its
  historical classification if ownership changes again.
- Patch `TINationState.ClaimWillBeHostile` for human claimants to use the
  authoritative evaluator. Retain `HostileClaimDueToDemocracy` only as a
  compatibility wrapper for direct callers. When the feature is disabled, run
  vanilla claim logic against the resulting scenario data; the separately
  approved historical-data corrections remain active. Preserve missing-claim
  and alien behavior explicitly.
- Add bilateral data overrides for the approved base-game records, preserving
  every other field and verifying the 2022, 2026, and 2070 variants. Register
  the approved Dark Skies classifications at runtime rather than shipping
  unconditional references to DLC-only identifiers; explicitly leave Russia ->
  Ukraine ordinary in the 2003 start.
- Put all `2003_*` registration behind the game-owned Dark Skies gate:
  DLC validation complete, `ModManager.dlcNames` contains `DarkSkies`, and the
  active scenario meta template requires `DarkSkies`. Do not probe DLC template
  identifiers on the base-game path.
- Patch the direct `hostileClaims.Contains(...)` consumers that describe or gate
  an external claim, including `CanUnifyFeedback`, map coloration, claim-list
  icons, nation-region rows, and policy target icons. Runtime consumers of an
  already integrated hostile region, such as Cohesion/Unrest and secession,
  should continue using the stored burden.
- Patch the region-transfer path so a historical claim resolved non-hostile at
  score `<= 6` does not remain in runtime `hostileClaims` after acquisition. A
  claim acquired above its applicable threshold must retain vanilla hostile
  integration costs and legitimization behavior.
- Let `nonHostileClaims`, capital-claim unification, region-transfer targeting,
  and secession selection consume the patched result through the authoritative
  evaluator. Do not duplicate the formula in each caller.
- Add a federation-link evaluator that enumerates the same claim directions
  accepted by vanilla and returns the best valid link and score. Patch
  `TINationState.CanFormFederation` and `TIFederationState.CanAddNation` to
  require that score to be at most `24`. Preserve `AddNation(..., startup: true)`
  so scenario-defined starting federations bypass the runtime gate.
- Patch `CanFormFederationFeedback` and `CanJoinFederationFeedback` to identify
  the best claim link, live score, and `24` threshold. Add a defensive execution
  check for `FormFederation`, which unlike `AddNation` does not currently
  revalidate eligibility before mutating state.
- Ensure both formerly non-hostile/high-score claims become hostile and formerly
  Government-blocked/low-score claims become non-hostile. This is a replacement,
  not an extra permissive condition.
- Patch `WillBeHostileExplanation`, `CanUnifyFeedback`, and English localization
  to show the total score, whether the claim is ordinary or historical, its
  applicable threshold, and preferably the four core components plus the
  Unrest/Cohesion modifier. Remove every obsolete `1.5`
  Government-only explanation, including map/region tooltips.
- Register the new core source in both project files and update default
  `Settings.xml`.

### 4. Build and automatic validation

- Add formula tests for identical inputs, inverse GDP-ratio symmetry, source/
  target directionality, monotonic Unrest and Cohesion effects, exact `12` and
  `6` threshold inclusion, just-over-threshold rejection, and invalid GDP
  fail-closed behavior.
- Add integration-style stub tests proving the patch can make an ordinary claim
  hostile or non-hostile, that score `12` is non-hostile, and that missing,
  historical, alien, and disabled-feature paths retain their intended
  behavior. In particular, prove that a historical score in `(6, 12]` remains
  hostile, a score at or below `6` becomes non-hostile, and a peaceful transfer
  does not leave a runtime hostile-region burden.
- Add federation tests for exact `24` inclusion, just-over-`24` rejection,
  selection of the lowest valid bidirectional claim link, a candidate linked to
  a non-lead federation member, no qualifying claim, invalid scores, startup
  federation bypass, and defensive rejection at execution.
- Add a data validation that enumerates every approved historical claim by
  scenario, rejects missing or duplicated records, and fails if Russia's
  Ukrainian claims are historical in 2003 or ordinary in 2022/2026/2070.
- Add DLC-gate tests for Dark Skies absent, installed but a base scenario active,
  active 2003 scenario, and a Dark Skies save. Prove the first two paths never
  resolve or warn about `2003_*` templates.
- Add target-IL validation for `HostileClaimDueToDemocracy`,
  `ClaimWillBeHostile`, `CanFormFederation`, `TIFederationState.CanAddNation`,
  the direct map call, and the expected downstream claim consumers so a game
  update fails safely.
- Run `tools\deploy.ps1` without `-SkipVerification`. It performs the required
  rebuild, verification, game-process assertions, and package copy.
- Immediately report that the deployed build is ready for manual testing.

### 5. Manual in-game test matrix

Test at least these cases in a disposable save:

1. An ordinary claim at score just below 12 is green/non-hostile on the map and
   in the region list.
2. An ordinary claim at exactly 12 remains non-hostile.
3. An ordinary claim just above 12 is orange/hostile and its tooltip shows the
   correct score and components.
4. A historical claim at exactly 6 is non-hostile; one just above 6 is hostile;
   both surfaces identify the stricter historical threshold.
5. A capital claim blocked by the old Government gap but passing harmonization
   becomes eligible for peaceful unification.
6. A capital claim allowed by the old Government gap but failing harmonization
   becomes ineligible.
7. A non-capital ordinary claim passes or fails peaceful region transfer using
   the same score.
8. Changing source Cohesion and target Unrest moves claim hostility and
   unification eligibility in the intended direction and refreshes the UI.
9. USA/Canada and China/Taiwan fail under starting values; Germany/France passes;
   and Russia/Ukraine's `9.86` passes as ordinary but fails as historical.
10. A historical claim scoring at most 6 transfers without leaving a hostile
    region burden; if that region later leaves, its external claim again uses
    the historical threshold.
11. A forced transfer above the applicable threshold records the expected
    hostile region, applies the vanilla Cohesion/Unrest burden, and can still be
    legitimized by Government or Unity investment.
12. Secession and involuntary-transfer candidate selection follows the new
    hostility result without exceptions.
13. Ethiopia/Eritrea uses threshold 6. The approved Russia/Ukraine,
    China/Taiwan, and both directions of India/Pakistan use threshold 6 in every
    applicable scenario. Missing claims remain hostile; breakaway reunification,
    federation entry, and alien behavior retain vanilla behavior.
14. AI-controlled nations see the same target list and complete an eligible
    unification or region transfer without an exception.
15. A standalone pair or prospective federation member with a best claim-link
    score exactly `24` is offered; a score just above `24` is absent and its
    feedback names the failed threshold.
16. A prospective member may qualify through a compatible claim link with a
    non-lead federation member; incompatible unrelated members do not become
    alternate score targets.
17. Scenario-defined starting federations load unchanged even when a member
    would fail the live `24` gate.
18. In the 2003 start, Russia's Ukrainian claims are ordinary. In 2022, 2026,
    and 2070 they are historical. All other approved flags match the verified
    scenario-date matrix.
19. With Dark Skies unavailable or a base scenario active, no `2003_*` lookup or
    warning occurs. With an entitled Dark Skies 2003 campaign active, the DLC
    historical matrix registers and persists through save/load.

### 6. Final documentation

After manual results, update the implementation matrix, patch sanity audit,
README feature summary, social-coefficient report, and this document with the
implemented formula, patch points, automated results, and any observed UI or AI
limitations.
