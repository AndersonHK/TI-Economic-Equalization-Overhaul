# Patch Sanity Audit

Status: current through Version 0.9.3 and Terra Invicta 1.0.51.

This audit checks the major economic, national, and logistics patch families against the directives in
`design-directives.md`. “Stock” names the quantity that prevents linear GDP/IP
from making a proportional effect scale incorrectly.

## TI 1.0.51 retarget

The local 1.0.51 compatibility audit treats the installed binary as
authoritative: all 149 global technology IDs match, the project compiles against
the installed assemblies, and every guarded gameplay/UI IL anchor—including the
climate-damage signature and threshold—matches exactly. No economy, social, or
land-warfare formula changed for this retarget.

The 0.8.1 conventional-gun power patch did expose a separate UI invariant on a
late save: powered gun rows gained an Energy Usage cell while unpowered gun rows
did not, and TI's `ShipModuleTable.ResizeColumns` indexed all visible rows using
the longest row shape. Version 0.8.2 keeps actual energy values unchanged but
ensures every conventional-gun comparison row creates that column.

| Patch | Stock / unit | Sanity result |
|---|---|---|
| `InvestmentPointsPatch` | GDP / $100B | Aligned. Output remains legible and linear; the low-income ramp models capital constraints without a discontinuity, and the configurable x1.05 output adjustment is uniform at every national scale. |
| `ControlPointCostPatch` | National economy score per CP | Aligned. It sums explicit per-technology exponent reductions of 0.02/0.03/0.05/0.05/0.05, so granted or out-of-order technologies behave correctly while the normal path remains 1/.98/.95/.90/.85/.80. It preserves free alien CPs, applies the selected x1.20 country-cost increase, then applies TI 1.0.51's live scenario maintenance multiplier. |
| `ControlPointCapacityPatch` | Complete non-project flat faction capacity | Aligned. It removes only the five known flat project-effect values from vanilla's result, then reinterprets their existing 5/10/20/40/120 values as additive percentage points over campaign/scenario, AI, councilor, and LEO capacity. Repeatable Management Research effects stack, unrelated modifiers and the alien 20,000 cap remain unchanged. |
| `MineNetworkMissionControlPatch`, `NextMineMissionControlPatch`, `DisabledMineMissionControlPatch`, `FreeMineAllowancePatch`, and `MissionControlUsageColorPatch` | Active-mine MC total, build/upgrade increment, shutdown savings, legacy allowances, and MC utilization display | Aligned. Active mine tiers sum directly, so T1/T2/T3 cost 1/2/3 MC, each one-tier upgrade adds 1, and shutdown releases the current tier. The safe-mine getter is zeroed for old saves; technology/project overrides remove only `MCFreeSpaceMineNetwork` effects while retaining exploration, probe-speed, and mining-output effects. MC usage is normal through 75% capacity, orange above 75% through the limit, and red over the limit; mine count no longer affects that color. |
| `CouncilorTotalAttributeCapPatch` | Final modified councilor attribute | Aligned. Only the final clamp in `GetAttribute` rises from 25 to the configurable 50. Stored attributes, generation, training, augmentation eligibility, and negative-trait ceilings keep TI's vanilla 25-point base cap; organization and positive-trait bonuses can use the added headroom. Mission quality remains normalized to 25 so modified values above 25 retain their intended additional effect. |
| `CouncilorAvailableAdministrationCapPatch` | Administration remaining for organizations | Aligned. The final `availableAdministration` ceiling now uses the configured total cap of 50 instead of the private 25-point base cap, allowing positive organization and trait bonuses to provide usable assignment capacity. |
| `CouncilorOrganizationWeightCapPatch` | Total organization tier usage | Aligned. `SufficientCapacityForOrg` compares projected organization weight with the configured 50-point total ceiling rather than the 25-point augmentation ceiling. Its separate available-Administration check still accounts for organization tier cost, organization Administration bonuses, and negative modifiers. |
| `CouncilorAttributeCapTooltipPatch` | Councilor stat-detail UI | Aligned. The tooltip keeps its councilor-specific base ceiling at 25 and changes only the absolute modified ceiling to the configured 50. |
| `CouncilorRuntimeCaps` and `TIGlobalConfig.json` organization cap | Organization count, UI, and AI | Aligned. The global maximum rises from 15 to the configurable 18 and is synchronized into TI's live global object at runtime. Assignment rejection, its tooltip, org management, and AI all read that shared value; Administration capacity remains a separate constraint. |
| `ArmyUpkeepPatch` | Fixed cost per army | Aligned. Every army pays its own home/away and miltech-dependent upkeep; large nations only benefit by having more IP to support more units. |
| `XenofaunaStrengthPatch` | Megafauna combat rating | Aligned. It changes only the configured maximum from 6 to 5 while preserving TI's abduction-driven progression and any explicit bonus technology level. |
| `ResearchPatch` | Population, human capital, institutions | Aligned. This is national productive output rather than an IP completion, so linear population with Education squared is economically legible. The default 0.0038 coefficient and `(PCGDP + $12,000) / $20,000` income factor are continuous, equal 0.60 at zero income, and avoid the former flat low-income plateau. |
| `EconomyGrowthPatch` | Fixed total GDP gain per IP constrained by factor balance | Aligned. GDP/c expresses capital per worker; normalized Core/Education/Government/Cohesion support labor, continuous resources/GDP and land/person support physical inputs, and two smooth constraints make capital-only growth diminish without penalizing proportional national scale. Technology lifts productivity and moves both return floors toward one. The complete calculation remains inline and divides by population only at TI's getter boundary. |
| `SpoilsGdpGrowthPatch` | Fixed total GDP gain per IP | Aligned. It captures exactly one Economy completion's aggregate GDP before Spoils changes other national values, then applies that amount alongside Spoils' distinct social, environmental, and cash effects. |
| `ClimateInequalityPatch` | Vanilla climate-driven Inequality delta | Aligned. It quadruples only changes tagged `InqReason_ClimateChange`; GDP loss, Education damage, priorities, events, revolution, secession, and annexation remain untouched. |
| `EconomyInequalityPatch` | GDP | Aligned after stock correction. Resources/GDP affects the raw change only while Abundance is enabled, and the continuous 1-9 boundary transform reaches x3 for inward changes while suppressing outward changes at the extremes. |
| `WelfareInequalityPatch` | GDP | Aligned after stock correction. A tenfold economy gets one tenth the per-IP rating change and approximately tenfold IP. |
| `SpoilsInequalityPatch` | GDP | Aligned after stock correction. Resource dependence matters strongly in small economies and weakly in diversified large economies; disabling Abundance removes that premium. |
| `CoupSocialResetPatch` | Failed-state social equilibrium | Aligned as a deliberate cycle breaker. Every completed coup applies one small `-0.10` Inequality change, then moves Cohesion to the rest state recalculated after TI's Government, Unrest, GDP, control-point, and randomized Cohesion effects. The zero floor preserves TI's lower bound when the revised equilibrium remains negative. |
| `KnowledgeEducationPatch` | Population | Aligned. Education is a demographic stock and receives smooth diminishing returns at high Education. |
| `KnowledgeCohesionPatch` | Population and distance to neutral | Aligned. It cannot jump across the target and larger populations require proportionally more completions. |
| `CohesionRestBaseValuePatch`, `CohesionRestDetailBaseValuePatch` | Configured rest-state base | Aligned. Gameplay replaces one guarded base constant, while the detail breakdown replaces both its displayed-base and internal-total constants. Both paths reduce 16 to 10.5 without altering the remaining aggregation order. |
| `CohesionRestInequalityPatch` | Education and Inequality | Aligned. Inequality 3 is neutral, lower values strengthen the Cohesion rest state, and higher values weaken it through `min(1, 0.5 + Education/20) * (6.75 - 2.25*Inequality)`. |
| `CohesionRestPublicElitePatch` | Government and ideological distance | Aligned. The retained ideological-distance penalty is scaled by Government/10, clamped to `[0,1]`, so democratic representation determines how fully an elite/public divide affects Cohesion. |
| `CohesionRestAutocracyPatch`, `CohesionRestAnocracyPatch`, `CohesionRestDemocracyPatch` | Regime-specific Cohesion effects | Aligned. Autocracy uses `(4^1.285 - Government^1.285) * (10-Unrest)/10` below 4.0. Anocracy runs from 4.0 through the shared 6.0 boundary. Democracy begins at that same boundary and uses `democracyCoefficient * (Government - 6.0)` with coefficient 1.0, stopping at Cohesion 5. |
| `GovernmentDemocracyPatch`, `GovernmentChangeCurvePatch` | Population / institutions and bounded score | Aligned after interaction correction. Government investment uses `333,333 / population`, while every positive and negative Government change passes exactly once through the smooth reciprocal x3/x1/3 boundary curve. Passive low-Cohesion pressure is halved before transformation. |
| `MilitaryTechnologyPatch` | Army count | Aligned after stock correction. One completion is divided among every army it upgrades; the catch-up multiplier remains smooth and never penalizes leaders. |
| `MilitaryMergerPatch` | Army/navy force structure and GDP | Aligned. The 50/50 blend represents inherited doctrine/equipment and the industrial base that sustains modernization; it replaces only the final merged rating. |
| `InequalityMergerPatch` | Two population income distributions | Aligned. Population shares, GDP/c separation, existing distribution width, and the finite-sample correction approximate the merged Gini without a step table or arbitrary disparity bonus. |
| `ClaimWillBeHostilePatch`, `HistoricalClaimRegistry`, and `HarmonizedRegionTransferPatch` | Cross-national social/economic distance and immutable grievance history | Aligned. Government, Inequality, Knowledge, and the symmetric per-capita-GDP ratio form one compatibility core; target Unrest and source Cohesion apply the approved directional multiplier. Inclusive `6`/`3` thresholds replace democracy-as-liberation and allow exceptional convergence to overcome historical hostility. Invalid GDP/c and missing claims fail closed, while a peacefully acquired historical region drops the mutable integration burden without erasing its future historical classification. |
| `CanFormFederationPatch`, `CanAddNationPatch`, and `FormFederationPatch` | Best actual cross-boundary claim link | Aligned. The inclusive `12` ceiling makes federation a looser preparatory relationship than integration while retaining TI's alliance, cooldown, executive-control, enemy, and breakaway checks. The execution patch prevents stale UI state from bypassing the gate; scenario startup assembly still uses TI's explicit bypass. |
| Historical-claim scenario registry | Active scenario and DLC entitlement | Aligned. Native flags are preserved; reviewed additions and the three expansion-project families are normalized from active bilateral templates. Dark Skies identifiers are never resolved unless TI has completed DLC validation, the entitlement list contains `DarkSkies`, and the active scenario requires it. Russia-to-Ukraine is explicitly excluded in 2003. |
| `OppressionUnrestPatch` | Population | Aligned. Repression has diminishing effectiveness in democratic systems and cannot drive Unrest below zero in one completion. |
| `OppressionDemocracyCurvePatch` | Bounded Government score | Aligned with the shared Government curve. The vanilla raw Oppression Government loss is resisted near zero, unchanged at five, and amplified near ten; UI and direct-investment pricing see the transformed getter value. |
| `EnvironmentSustainabilityPatch` | GDP; fallout per land area | Aligned. Transition cost follows the dirty capital stock; concentrated nuclear damage makes progress harder without changing cleanup IP per blast. |
| `EnvironmentCo2RemovalPatch` | No direct atmospheric removal | Aligned. Environment IP reduces national carbon intensity; completions at zero Sustainability return no global CO2 pulse. |
| `EnvironmentMethaneRemovalPatch` | No direct atmospheric removal | Aligned. Completions at zero Sustainability return no global methane pulse. |
| `EnvironmentNitrousOxideRemovalPatch` | No direct atmospheric removal | Aligned. Completions at zero Sustainability return no global nitrous-oxide pulse. |
| `ClimateGdpDamagePatch` | Vanilla warm-climate GDP damage | Aligned. It scales only negative common-method results above 0.25 C by the configured x0.90, keeping gameplay and climate displays synchronized while leaving cold/neutral outcomes and climate Inequality unchanged. |
| `EconomyEmissionsPatch` | GDP × carbon intensity | Aligned. GDP, not population or borders, produces emissions; resources/GDP adds a bounded extractive-economy intensity premium. |
| `UnityCohesionPatch` | Population | Aligned. Population scaling prevents unified countries becoming easier to homogenize; Education and Government reduce the effect with a floor. |
| `UnityEducationPatch` | Population | Aligned. The small secondary Education loss scales with the people affected. |
| `UnityPropagandaPatch` | Vanilla demographic effect × configured strength | Aligned. It changes only one config field load and preserves TI 1.0.51 claims and completion logic. |
| `SpoilsGovernmentPatch` | Population / institutions and bounded score | Aligned with inverse-population behavior followed by the shared reciprocal Government curve. |
| `SpoilsSustainabilityPatch` | GDP | Aligned. Spoils damages carbon intensity, so its per-IP change falls with the economy and rises with resource dependence while Abundance is enabled. |
| `SpoilsPropagandaPatch` | Vanilla demographic effect × configured strength | Aligned. It preserves payout, CP, corruption, and Sustainability, scales propaganda, then deletes vanilla's final direct atmospheric-emissions block. |
| `SpoilsMoneyPatch` | Fixed payout × resources/GDP × Government | Aligned. The full $60 base is retained. The curve is continuous from no resource premium toward the configured maximum; no region-count table remains, and disabling Abundance leaves the base/Government payout. |
| `GlobalTechnologyResearchCostPatch`, `FactionProjectResearchCostPatch` | Global technology and faction-project research costs | Aligned. They apply configurable x2.00 and x1.40 multipliers after TI computes speed, endgame, and repeatable costs, respectively. |
| `GlobalTechnologySoftSelectionPatch` | AI global-technology weighted lottery | Aligned. It removes only the maximum-tier hard filter and duplicate post-tier strategic bonuses. Faction category/role valuations, native tier classification, objective-path detection, Space War suppression, Control Point context, and weighted-random selection remain inputs to the documented soft formula. Faction-project selection remains vanilla. |
| `EconomyRegionThresholdPatch` | Fixed accumulated IP | Aligned. Region conversion is a fixed capital project; default x5 makes it harder without border-sensitive scaling. |
| `DecolonizationThresholdPatch` | Fixed accumulated IP | Aligned. The threshold is a political project cost and uses the same guarded multiplier. |
| `FalloutCleanupThresholdPatch` | Fixed accumulated IP per detonation | Aligned. Every blast costs the same to clean; damage concentration is handled separately by land area. |
| `PriorityTooltipPatch` | UI mirror | Aligned. It appends the shared Economy/Spoils return, productivity, both technology-progress axes, labor/resource pressure and constraints, abundance, and climate GDP multiplier while replacing exactly the same five threshold loads as gameplay. |
| `InvestmentTooltipPatch` | UI mirror | Aligned. It exposes GDP base IP, the low-income multiplier, and fixed army/navy upkeep without replacing vanilla text. |
| `TIMissionTemplate.json` Purge override | Mission difficulty | Aligned. The defender's flat modifier rises from 3 to 4 while all national-scale, support, councilor, and alien modifiers remain vanilla. |
| `TIMissionTemplate.json` Enthrall Elites override | Mission difficulty | Aligned. The defender's flat modifier rises from 2 to 3 while retaining vanilla's GDP-based target-nation defense and all other mission modifiers. |
| `TIStartTimeTemplate.json` 2022 override | Starting global technologies | Aligned. Mission to Space and Advanced Chemical Rocketry begin completed; Outpost Habs replaces Mission to Space in the active list so the scenario retains three valid research choices. |
| `HabBuildMaterialsRewritePatch` | Complete modified module mass | Aligned. Rebalanced human modules pay resources for their full physical mass, including upgrade and irradiation modifiers, rather than treating the former weighted share as the whole bill. |
| `HabCostFromSpaceRewritePatch` and `HabLogistics` | Material stockpile, Earth shortage, mandatory freight, and propellant | Aligned. Materials are charged once; Earth shortages count toward the one-third transport floor; non-Earth freight pays Water/Volatiles propellant from route delta-v. Earth is the fallback only when no eligible space factory exists. |
| Factory source registry and route resolver | Owned active factory/dock capability | Aligned. Exact-hab local manufacture uses factory tier; remote export requires a same-hab dock and uses the lower tier. Foreign, inactive, destroyed, unpowered, and decommissioning sources are excluded, and planetary-system borders do not restrict routing. |
| Founding and probe logistics patches | Core or full probe payload | Aligned. Founding preserves non-logistics restrictions while sharing the system-agnostic network. Probes are full-payload T1 jobs and require a T1 factory-dock pair for space launch. |
| Logistics route and cost caches | Topology/time generation and resource-balance generation | Aligned. Staleness is marked without scanning; the next request recomputes only the stale layer. Warm UI and planner calls are O(1) with respect to origin count, and no serialized cache state is added. |
| `HabLogisticsAiPriorityPatch` | First export-capable pair per major colonized system | Aligned. The strategic bonus applies only when a factory or dock completes a same-hab pair, is strongest in Earth-Moon, and ends once a pair is present or committed. Existing dock/refueling priorities remain intact. |

## Compatibility risks deliberately guarded

- Threshold, Unity propaganda, and Spoils propaganda transpilers require exact
  replacement counts and throw during initialization if TI changes the expected IL.
- The three councilor-cap transpilers each require exactly one replacement in
  `GetAttribute`, `availableAdministration`, and `SufficientCapacityForOrg`;
  verification also locks the separate base-stat, augmentation,
  organization-count, rejection-tooltip, and stat-tooltip IL anchors.
- Feature and global toggles return prefixes to vanilla and make transpiler helpers
  return live vanilla field values.
- Invalid or non-finite calculations retain vanilla or use a documented safe value.
- The metadata and assembly target TI 1.0.51. Verification compiles against the
  installed 1.0.51 assemblies and confirms every guarded gameplay/UI IL anchor,
  including the conventional-gun module-row compatibility patch.
- The control-cost patch reads `CPMaintenanceModifier` from the live start-time
  template, so scenario balance changes are honored without hiding the mod's
  economy-score/exponent formula.
- The capacity patch enumerates only the five installed stackable project-effect
  IDs and verification locks their additive values and the faction-effect API.
- Resource-dependent side effects consistently observe the Abundance feature
  toggle in gameplay and tooltips.
- Logistics validators dynamically apply the hab-cost, founding, probe,
  AI-priority, and cache-invalidation Harmony classes against the installed
  1.0.51 assemblies. They also reject the removed multiline cost-label patches;
  localization packaging is checked separately.

## Deferred audit item

Other event-driven Inequality changes still bypass the shared boundary curve.
They remain the next logical extension because events can still make abrupt
changes near the limits even though Economy, Welfare, Spoils, and the fixed
coup adjustment cannot.
