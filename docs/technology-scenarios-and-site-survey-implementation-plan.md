# Technology, Scenario, and Site-Survey Changes

Status: implemented, verified, packaged, and deployed in Version 0.9.4;
manual in-game validation pending

Game-data baseline: Terra Invicta 1.0.51

Decision date: 2026-08-21

## Scope and locked decisions

This change will:

1. increase the global-technology research-cost multiplier from `2.00` to
   `2.20`, adding another 20% of vanilla base cost;
2. complete `MissiontotheMoon` in the 2026 scenario and replace its active
   research slot with `MissiontoMars`;
3. complete `Project_OutpostCore` for every human faction at both the 2022 and
   2026 starts;
4. replace body-wide probe prospecting with site-targeted, landed survey drones
   whose delivered payload mass is exactly `0.325` metric tonnes per landing;
5. retain shipborne `SurveyPlanetFromFleetOperation` as a body-wide survey
   capability; and
6. preserve old saves in which body-level survey intel already marks the whole
   body prospected.

The faction-project research-cost multiplier remains `1.40`.

## Pre-implementation repository state

The currently deployed defaults are:

- global-technology research-cost multiplier: `2.00`;
- faction-project research-cost multiplier: `1.40`;
- 2022 active technologies: `Skywatch`, `WeAreNotAlone`, and `OutpostHabs`;
- 2026 active technologies: `DeepSystemSkywatch`, `WeAreNotAlone`, and
  `MissiontotheMoon`;
- 2026 completed technologies include `Skywatch` and `OutpostHabs`, but not
  `MissiontotheMoon`;
- neither modern scenario completes `Project_OutpostCore`; and
- `LaunchProbeOperation` targets a `TISpaceBodyState`, stores prospecting intel
  on the body, and calls `ProspectSpaceBody` on arrival.

The mod's Luna override contains **35 hab sites**. This corrects the earlier
nine-site assumption, which described the smaller vanilla roster rather than
the modded `TISpaceBodyTemplate`.

The current probe payload is calculated as:

```text
M_body = probePayloadBaseline_tons
       + probePayloadPerHabSite_tons * numberOfSites
       = 0.5 + 0.5 * 35
       = 18.0 tonnes
```

`TIFactionState.Prospected(TIHabSiteState)` currently delegates directly to the
site's parent body. Consequently, a completed probe reveals every site on that
body at once.

## Research-cost decision

The approved global-technology formula is:

```text
effectiveGlobalCost
    = vanillaAdjustedCost * globalTechnologyMultiplier
    = vanillaAdjustedCost * 2.20
```

Moving from `2.00` to `2.20` is an increase of `0.20 * vanilla base cost`, or a
10% increase relative to the mod's current effective cost. Examples without
additional vanilla modifiers are:

| Vanilla or authored cost | Current x2.00 | Approved x2.20 |
|---:|---:|---:|
| 1,000 | 2,000 | 2,200 |
| 2,500 (`MissiontoMars`) | 5,000 | 5,500 |
| 10,000 | 20,000 | 22,000 |

`MissionToSpace`, `Skywatch`, and `WeAreNotAlone` have authored overrides at
twice their installed vanilla costs. Their total cost becomes `2 * 2.20 = 4.40`
times vanilla base rather than the current `4.00` times base. This is retained
deliberately; those opening technologies were already singled out for slower
campaign pacing.

Faction projects remain:

```text
effectiveFactionProjectCost = vanillaAdjustedProjectCost * 1.40
```

## Approved scenario state

| Scenario | Active global research | Additional completed technology | Completed-project change |
|---|---|---|---|
| 2022 | `Skywatch`; `WeAreNotAlone`; `OutpostHabs` | none | add `Project_OutpostCore` |
| 2026 | `DeepSystemSkywatch`; `WeAreNotAlone`; `MissiontoMars` | add `MissiontotheMoon` | add `Project_OutpostCore` |

`MissiontoMars` is legal in 2026 because the scenario already completes both
of its installed prerequisites, `OutpostHabs` and `Skywatch`. Its 2,500-point
template cost becomes 5,500 with the approved multiplier.

Completing `MissiontotheMoon` grants its normal effects, including Luna
exploration access and its probe-speed effect. Completing
`Project_OutpostCore` bypasses the normal 90% starting unlock roll and the
300-point faction research project; it does not construct a base or spend
Mission Control.

## Landed survey-drone mass references

The selected `0.325`-tonne payload is a Surveyor-class lander with a modest
systems and instrumentation margin. It is grounded in three actual robotic
lunar missions:

| Mission | Published mass used for comparison | Survey role |
|---|---:|---|
| Surveyor 1 | 294.3 kg at touchdown | Pre-Apollo imagery and lunar surface-property investigation |
| SLIM | approximately 200 kg dry main body; 700-730 kg wet | Precision landing, spectroscopy, and two deployable surface robots |
| Chandrayaan-3 | 1,752 kg lander module including its 26 kg rover | Thermal, seismic, plasma, and elemental-composition measurements |

Primary sources:

- [NASA Surveyor 1 mission profile](https://science.nasa.gov/mission/surveyor-1/)
- [JAXA SLIM media kit](https://global.jaxa.jp/countdown/slim/SLIM-mediakit-EN_2308.pdf)
- [JAXA SLIM landing results](https://global.jaxa.jp/press/2024/01/20240125-1_e.html)
- [ISRO Chandrayaan-3 mission details](https://www.isro.gov.in/ISRO_EN/Chandrayaan3_Details.html)

The physical mission masses and the game's launch-equivalent tonnage are not
the same quantity. The game begins with delivered payload mass and applies its
rocket equation to determine Boost or space-resource propellant.

## Earth-launch and lunar-landing equations

The mod's Earth launch equation is:

```text
Boost = deliveredMass_tonnes
      * spaceResourceToTons
      * exp(normalizedDeltaV_kps / modifiedGenericTransferEV_kps)
```

For the two modern starts:

```text
spaceResourceToTons             = 0.1 Boost per LEO-equivalent tonne
modifiedGenericTransferEV_kps   = 4.44 km/s
Earth-to-low-lunar-orbit delta-v = 4.294550443 km/s
```

Generic lunar landing cost is added by
`TIHabSiteState.DeltaVToLandFromInterface_kps`. Across the mod's 35 lunar-site
latitudes, landing delta-v is approximately 2.352-2.356 km/s. Therefore:

```text
Earth-to-lunar-site normalized delta-v ~= 6.647-6.650 km/s
mass ratio = exp(normalizedDeltaV / 4.44) ~= 4.468-4.472

Boost per 0.325-tonne landing
    = 0.325 * 0.1 * mass ratio
    ~= 0.1452-0.1454 Boost
```

The corresponding launch-equivalent tonnage is approximately 1.452-1.454
tonnes per site. Sending one 0.325-tonne drone to all 35 lunar sites costs
approximately **5.0842 Boost** in total before any later propulsion changes.

For comparison, the current all-body probe flies only to low lunar orbit:

```text
current body probe payload = 18.0 tonnes
orbital mass ratio = exp(4.294550443 / 4.44) = 2.63067664
current body-probe Boost = 18.0 * 0.1 * 2.63067664 = 4.735218 Boost
```

Thus the approved individual-site system costs about 0.3490 more Boost across
the complete Moon, a difference of approximately 7.37%. The selected physical
reference restores individual surveying while keeping the total lunar-survey
cost in the same balance range as the current body-wide probe.

For non-Earth manufacturing, the existing logistics equation remains:

```text
propellantMass
    = deliveredPayloadMass
    * (exp(totalRouteDeltaV / modifiedGenericTransferEV) - 1)
```

The route must terminate at the selected surface site so that lunar landing
delta-v is included. Probe materials retain the installed metals, volatiles,
noble-metals, and fissiles fractions.

## Approved site-survey behavior

### Targeting and state

- The Launch Probe action remains visible from a space body, but uses
  `TIOperationTargeting_HabSite` to highlight and select eligible surface sites.
- The delivered probe mass is exactly `0.325` tonnes, independent of the number
  of sites and without the current irradiation multiplier.
- A site at intel `0.1` has a drone en route; a site at intel `1.0` is surveyed.
- A second drone may be launched to a different site while another is in
  flight. A duplicate mission to the same site is prohibited.
- Completing one mission reveals only the selected site's resources.
- When every site is surveyed, the normal body-level `ProspectSpaceBody`
  completion is invoked so existing AI reactions, milestones, events, and
  body-level queries continue to work.
- A body-level intel value of `1.0` continues to make every child site count as
  surveyed. This preserves completed surveys in old saves and permits the
  retained fleet-survey operation to reveal a whole body.
- A legacy in-flight probe whose pending operation still targets a body will
  complete using its original body-wide behavior rather than fail or choose an
  arbitrary site after loading.

### Base founding

- A faction may found a base at a vacant site only when that specific site is
  surveyed, unless legacy/body-wide intel already marks the whole body
  surveyed.
- Found Base target lists must filter out unsurveyed sites rather than exposing
  every vacant site as soon as any one site has been surveyed.
- Existing technology, Mission Control, construction, ownership, and logistics
  requirements remain unchanged.

### AI and fleet surveys

- AI `FactionGoal_ProspectSites` remains body-oriented, but its probe launch is
  normalized to the highest-value eligible unscanned site. After completion,
  the goal remains valid until no eligible site remains.
- AI launches are sequential per existing prospecting goal; player launches
  may be concurrent at different sites.
- `SurveyPlanetFromFleetOperation` remains body-wide. It represents the later,
  shipborne survey capability and intentionally bypasses the landed-drone
  sequence.

## Finalized implementation plan

### 1. Document and safeguard the current state

- Keep `docs/starting-technology-2022-2026.md` authoritative for the currently
  deployed build until implementation and deployment succeed.
- Use this document as the approved pre-implementation authority.
- Confirm a clean or understood worktree before editing and preserve unrelated
  user changes.

### 2. Apply coefficient and scenario-template changes

- In `TIEconomyMod/Main.cs`, change the default
  `TechnologySettings.researchCostMultiplier` from `2.00f` to `2.20f` and
  update the adjacent explanatory example.
- In `TIEconomyMod/ModFiles/Settings.xml`, change
  `<researchCostMultiplier>` from `2` to `2.2`; leave
  `<projectResearchCostMultiplier>` at `1.4`.
- In `TIEconomyMod/ModFiles/TIGlobalConfig.json`, set
  `probePayloadBaseline_tons` to `0.325` and
  `probePayloadPerHabSite_tons` to `0`.
- In `TIEconomyMod/ModFiles/TIStartTimeTemplate.json`:
  - add `Project_OutpostCore` to `projectsCompleted` for `ModernDayStart`;
  - replace `MissiontotheMoon` with `MissiontoMars` in the `2026Start`
    `startingTechs` array;
  - add `MissiontotheMoon` to the 2026 `globalTechsCompleted` array; and
  - add `Project_OutpostCore` to the 2026 `projectsCompleted` array.
- Preserve array order, exact template identifiers, and JSON merge semantics.

### 3. Add site-survey runtime helpers

- Add `TIEconomyMod/Core/ProbeSurveyRuntime.cs` and include it in
  `TIEconomyMod.csproj`.
- Centralize:
  - target normalization from body to site for AI calls;
  - eligible-site enumeration;
  - site intel checks for surveyed and en-route states;
  - the fixed `0.325`-tonne delivered mass;
  - per-site scan duration;
  - launch and completion state transitions;
  - final all-sites body completion; and
  - legacy body-target handling.
- Use runtime-derived faction intel only; introduce no new serialized fields.

### 4. Convert Launch Probe to site targeting and landed costs

- Extend `TIEconomyMod/Patches/HabLogisticsPatches.cs` or add a focused
  `ProbeSurveyPatches.cs` file.
- Patch `LaunchProbeOperation.GetTargetingMethod` to return
  `TIOperationTargeting_HabSite`.
- Patch `GetPossibleTargets`, `OpVisibleToActor`, and operation validation so
  only unscanned sites without an incoming drone are selectable.
- Normalize body targets supplied by AI to the best eligible site before
  confirmation.
- Replace the launch and completion calls inside `OnOperationConfirm` and
  `ExecuteOperation` with site-aware helpers while retaining native cost
  payment, pending-operation events, milestones, and operation-executed events.
- Rework both `SpaceCost` and `EarthCost` around a fixed 0.325-tonne payload and
  the selected `TIHabSiteState` destination. Earth costs must call
  `EarthLaunchCost.CalculateBoost(faction, site, 0.325f)` so landing delta-v is
  included. Space logistics must quote the site itself, not its parent body's
  interface orbit.
- Retain material composition, Earth-purchase Money charges, space-resource
  substitution, construction time, transfer effects, and route ranking.
- Apply the existing mission-technology contribution modifier to a single-site
  scan. Treat an unoccupied site as the current one-site expression
  `max(1, 2 * contributionRemaining)` scan days.
- Make operation text and arrival/launch notifications identify the selected
  site and explain that the drone lands and surveys only that site.

### 5. Patch survey queries and base-founding filters

- Patch `TIFactionState.Prospected(TIHabSiteState)` to return true when the
  faction has site intel `>= 1`, or when legacy/body-wide parent intel is
  `>= 1`.
- Keep `Prospected(TISpaceBodyState)` body-level and set it only after all sites
  are surveyed or a fleet/legacy body survey completes.
- Make body-oriented candidate and in-progress queries aggregate child-site
  state, including `CandidateForProspecting`, `CanProspectWithProbe`,
  `ProspectingSpaceBody`, `ProspectorEnRoute`, `ProspectorArrival`, and the
  body's soon-to-be-prospected lists.
- Patch `FoundBaseOperation.GetPossibleTargets` to retain only vacant sites
  surveyed by the acting faction. Patch body-level founding visibility so it
  becomes available when at least one eligible surveyed vacant site exists.
- Audit resource display, hab-site tooltips, advice, milestones, AI planning,
  and notification callers for assumptions that site survey always equals
  body survey.

### 6. Add automated verification

- Add `tools/validate-probe-site-survey.ps1` and call it from `tools/verify.ps1`.
- Validate the target Terra Invicta 1.0.51 method signatures and the Harmony
  patches for targeting, launch, completion, survey queries, and founding.
- Extend JSON/config verification to assert:
  - global multiplier `2.20` in code defaults and shipped settings;
  - project multiplier remains `1.40`;
  - 2022/2026 scenario arrays exactly match the approved table;
  - Mission to Mars prerequisites are satisfied by the 2026 completed set;
  - both scenarios contain `Project_OutpostCore` exactly once; and
  - probe payload coefficients are `0.325` and `0`.
- Add formula tests for:
  - fixed payload mass;
  - Earth-to-site Boost at low- and high-latitude lunar sites;
  - approximately 5.0842 total Boost for all 35 sites with the modern-start
    exhaust velocity;
  - space-origin landing propellant;
  - duplicate-target exclusion;
  - partial versus complete body survey state;
  - legacy body-intel compatibility; and
  - last-site body completion.
- Update the implementation-matrix workbook and its validator for the new
  patch surface.

### 7. Update authoritative documentation and localization

- After code behavior is complete, update
  `docs/starting-technology-2022-2026.md` with the 2.20 multiplier, Mission to
  Moon completion, Mission to Mars active slot, and starting Outpost Core.
- Update `docs/manufacturing-logistics.md` so probes are documented as
  site-targeted landed drones rather than body-wide orbital payloads.
- Update `docs/orbits-and-lunar-resources/approved-design.md` with the 0.325-ton
  site-survey cost and corrected full-Moon comparison.
- Promote this plan's implemented decisions in `docs/README.md`, retaining this
  file as the dated decision and equation record.
- Update `TIOperationTemplate.en` and any required notification/localization
  keys with concise site-specific wording.

### 8. Build, validate, and deploy as one critical step

- Run `tools/deploy.ps1` without `-SkipVerification` as soon as implementation
  is ready.
- Let the script assert that Terra Invicta is closed, rebuild the DLL, run all
  validators and formula tests, package the mod, recheck the process state, and
  copy the verified build.
- If the game-open assertion fails, stop safely and report the deployment
  blocker. Do not replace installed files manually.
- Immediately after successful deployment, tell the user the build is ready
  for manual testing while final documentation review continues.

### 9. Manual in-game test matrix

- Start a new 2022 game and confirm Outpost Core is completed while the three
  active global technologies remain unchanged.
- Start a new 2026 game and confirm Mission to the Moon is completed, Mission to
  Mars occupies its active slot at 5,500 research, and Outpost Core is
  completed.
- Confirm a representative 1,000-point global technology costs 2,200 and
  faction projects still use x1.40.
- From the Moon, launch a probe, select a specific highlighted site, and verify
  an Earth cost near 0.145 Boost with Cryogenic Liquid-Fuel Rockets.
- Verify low- and high-latitude sites differ only by the expected landing
  delta-v amount.
- Launch probes concurrently to two different sites and verify a duplicate
  launch to either site is blocked.
- Advance time and confirm each arrival reveals only its selected site; sibling
  resources remain hidden.
- Confirm a base may be founded at a surveyed vacant site but not an unsurveyed
  one.
- Confirm the last site marks the Moon fully prospected and triggers the normal
  body-level reactions.
- Verify a same-hab factory/dock space option includes surface landing
  propellant and remains distinct from the Earth option.
- Save and reload with partial surveys and probes in flight; confirm site state,
  pending operations, and target names survive.
- Load a pre-change save with a body already prospected and confirm all child
  sites remain available.
- Use a survey-capable fleet and confirm its retained body-wide survey reveals
  all sites.
- Observe at least one AI faction through a site-probe cycle and confirm it
  selects a legal site, does not repeat surveyed sites, and continues its goal
  until the body is complete.

### 10. Closeout

- Record automated and manual results in this document or the appropriate
  authoritative documents.
- Re-run normal deployment if any post-deployment edit rebuilds the mod.
- Keep manual validation status explicit until the scenario, survey, founding,
  persistence, and AI checks have passed.

## Implementation and deployment record

### Manual-test correction: bulk probe launch

The first 0.9.4 in-game test exposed a separate native path that was not part of
the original `LaunchProbeOperation` conversion. The research-completion popup
creates a `LaunchAllProbeOperation`; its inherited implementation enumerated
eligible **space bodies**, so Mission to Mars displayed `Launch 7 Probes` even
though Mars has 25 surface sites. It also summed and launched only one
0.325-tonne drone per body.

The corrected invariant is that the bulk operation enumerates every eligible,
unsurveyed site without a drone already en route. That one site list is the
authority for the displayed count, aggregate resource cost, affordability
check, and launch execution. A completely unsurveyed Mars therefore contributes
25 probes to the total; other eligible moons or previously unlocked bodies add
their remaining sites. The ordinary single-site operation, AI normalization,
legacy body-targeted probes, and shipborne body-wide surveys retain their
documented behavior.

Version 0.9.4 implements the locked decisions above. The implementation adds
`ProbeSurveyRuntime.cs`, `ProbeSurveyPatches.cs`, and the dedicated
`validate-probe-site-survey.ps1` gate; updates the configuration, scenario
templates, localization, authorities, formula tests, and implementation matrix;
and preserves body-level intel as the compatibility path for old saves and
fleet surveys.

On 2026-08-21, `tools/deploy.ps1` completed without `-SkipVerification` against
the installed Terra Invicta 1.0.51 assemblies. Results included:

- all 17 new probe-cost, survey-state, targeting, arrival, and founding patch
  classes applied dynamically through Harmony;
- exact scenario-array, prerequisite, payload, and 35-site lunar-cost checks;
- 1,123 dependency-free formula assertions;
- implementation-matrix validation across 99 rows, 24 settings groups, and 173
  Harmony patches;
- release archive `artifacts/TIEconomyMod-0.9.4-ti1.0.51.zip`; and
- 45 packaged files deployed to the enabled mod directory.

Manual in-game results remain pending and should be recorded against the matrix
in section 9.

The first manual test then confirmed that Mission to Mars still exposed the
native `LaunchAllProbeOperation` body list: seven accessible bodies produced the
`Launch 7 Probes` label and only 1.9 Boost of aggregate cost. The correction
described above was implemented and redeployed on 2026-08-21. The deployment
again completed the full verification suite without `-SkipVerification`,
validated the installed 25-site Mars roster and the new bulk-site Harmony
target, passed all 1,123 formula assertions, and deployed 46 files. The deployed
DLL SHA-256 is
`AC5935F0E22E65B818B7DC92DF133510228FAD97CFB6350189D042B5F57381D2`.

Manual retest remains pending: complete Mission to Mars with no other eligible
body and confirm `Launch 25 Site Probes`; with other eligible sites, confirm the
label and cost include those sites too. Clicking the button must create one
incoming drone marker for every counted site.
