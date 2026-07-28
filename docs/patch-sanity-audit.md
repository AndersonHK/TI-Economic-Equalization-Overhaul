# Patch Sanity Audit

This audit checks each Harmony patch against the directives in
`design-directives.md`. “Stock” names the quantity that prevents linear GDP/IP
from making a proportional effect scale incorrectly.

## TI 1.0.49 retarget

The 1.0.49 release is a compatibility hotfix over 1.0.47: its published scope is
DLC support, save/load and notification crash guards, old-federation
initialization, and technology-tree prerequisite display fixes. It does not
announce national-priority or economic balance changes. The local compatibility
audit still treats the installed binary as authoritative: all 149 global
technology IDs match, the project compiles against the installed assemblies, and
every guarded gameplay/UI IL anchor—including the climate-damage signature and
threshold—matches exactly. No formula or configuration default changed for this
retarget.

Release notes:
https://store.steampowered.com/news/app/1176470/view/696519283891505791

| Patch | Stock / unit | Sanity result |
|---|---|---|
| `InvestmentPointsPatch` | GDP / $100B | Aligned. Output remains legible and linear; the low-income ramp models capital constraints without a discontinuity, and the configurable x1.05 output adjustment is uniform at every national scale. |
| `ControlPointCostPatch` | National economy score per CP | Aligned. It retains the configured technology exponent sequence and free alien CPs, applies the selected x1.20 country-cost increase, then applies TI 1.0.49's live scenario maintenance multiplier. It deliberately does not adopt vanilla's global-GDP normalization. |
| `ControlPointCapacityPatch` | Complete non-project flat faction capacity | Aligned. It removes only the five known flat project-effect values from vanilla's result, then reinterprets their existing 5/10/20/40/120 values as additive percentage points over campaign/scenario, AI, councilor, and LEO capacity. Repeatable Management Research effects stack, unrelated modifiers and the alien 20,000 cap remain unchanged. |
| `ArmyUpkeepPatch` | Fixed cost per army | Aligned. Every army pays its own home/away and miltech-dependent upkeep; large nations only benefit by having more IP to support more units. |
| `XenofaunaStrengthPatch` | Megafauna combat rating | Aligned. It changes only the configured maximum from 6 to 5 while preserving TI's abduction-driven progression and any explicit bonus technology level. |
| `ResearchPatch` | Population, human capital, institutions | Aligned. This is national productive output rather than an IP completion, so linear population with Education squared is economically legible. |
| `EconomyGrowthPatch` | Fixed total GDP gain per IP constrained by factor balance | Aligned. GDP/c expresses capital per worker; normalized Core/Education/Government/Cohesion support labor, continuous resources/GDP and land/person support physical inputs, and two smooth constraints make capital-only growth diminish without penalizing proportional national scale. Technology lifts productivity and moves both return floors toward one. The complete calculation remains inline and divides by population only at TI's getter boundary. |
| `SpoilsGdpGrowthPatch` | Fixed total GDP gain per IP | Aligned. It captures exactly one Economy completion's aggregate GDP before Spoils changes other national values, then applies that amount alongside Spoils' distinct social, environmental, and cash effects. |
| `ClimateInequalityPatch` | Vanilla climate-driven Inequality delta | Aligned. It doubles only changes tagged `InqReason_ClimateChange`; GDP loss, Education damage, priorities, events, revolution, secession, and annexation remain untouched. |
| `EconomyInequalityPatch` | GDP | Aligned after stock correction. Resources/GDP affects the raw change only while Abundance is enabled, and the continuous 1-9 boundary transform makes extremes progressively harder. |
| `WelfareInequalityPatch` | GDP | Aligned after stock correction. A tenfold economy gets one tenth the per-IP rating change and approximately tenfold IP. |
| `SpoilsInequalityPatch` | GDP | Aligned after stock correction. Resource dependence matters strongly in small economies and weakly in diversified large economies; disabling Abundance removes that premium. |
| `KnowledgeEducationPatch` | Population | Aligned. Education is a demographic stock and receives smooth diminishing returns at high Education. |
| `KnowledgeCohesionPatch` | Population and distance to neutral | Aligned. It cannot jump across the target and larger populations require proportionally more completions. |
| `GovernmentDemocracyPatch` | Population / institutions | Aligned. It changes a society-wide institutional score rather than buying a fixed asset. |
| `MilitaryTechnologyPatch` | Army count | Aligned after stock correction. One completion is divided among every army it upgrades; the catch-up multiplier remains smooth and never penalizes leaders. |
| `MilitaryMergerPatch` | Army/navy force structure and GDP | Aligned. The 50/50 blend represents inherited doctrine/equipment and the industrial base that sustains modernization; it replaces only the final merged rating. |
| `InequalityMergerPatch` | Two population income distributions | Aligned. Population shares, GDP/c separation, existing distribution width, and the finite-sample correction approximate the merged Gini without a step table or arbitrary disparity bonus. |
| `OppressionUnrestPatch` | Population | Aligned. Repression has diminishing effectiveness in democratic systems and cannot drive Unrest below zero in one completion. |
| `EnvironmentSustainabilityPatch` | GDP; fallout per land area | Aligned. Transition cost follows the dirty capital stock; concentrated nuclear damage makes progress harder without changing cleanup IP per blast. |
| `EnvironmentCo2RemovalPatch` | Fixed atmospheric quantity per IP | Aligned. No demographic divisor; larger economies remove more only by spending more IP. |
| `EnvironmentMethaneRemovalPatch` | Fixed atmospheric quantity per IP | Aligned for the same reason as CO2. |
| `EnvironmentNitrousOxideRemovalPatch` | Fixed atmospheric quantity per IP | Aligned for the same reason as CO2. |
| `ClimateGdpDamagePatch` | Vanilla warm-climate GDP damage | Aligned. It scales only negative common-method results above 0.25 C by the configured x0.90, keeping gameplay and climate displays synchronized while leaving cold/neutral outcomes and climate Inequality unchanged. |
| `EconomyEmissionsPatch` | GDP × carbon intensity | Aligned. GDP, not population or borders, produces emissions; resources/GDP adds a bounded extractive-economy intensity premium. |
| `UnityCohesionPatch` | Population | Aligned. Population scaling prevents unified countries becoming easier to homogenize; Education and Government reduce the effect with a floor. |
| `UnityEducationPatch` | Population | Aligned. The small secondary Education loss scales with the people affected. |
| `UnityPropagandaPatch` | Vanilla demographic effect × configured strength | Aligned. It changes only one config field load and preserves TI 1.0.49 claims and completion logic. |
| `SpoilsGovernmentPatch` | Population / institutions | Aligned with the selected inverse-population behavior. |
| `SpoilsSustainabilityPatch` | GDP | Aligned. Spoils damages carbon intensity, so its per-IP change falls with the economy and rises with resource dependence while Abundance is enabled. |
| `SpoilsPropagandaPatch` | Vanilla demographic effect × configured strength | Aligned. It preserves payout, CP, corruption, and Sustainability, scales propaganda, then deletes vanilla's final direct atmospheric-emissions block. |
| `SpoilsMoneyPatch` | Fixed payout × resources/GDP × Government | Aligned. The full $60 base is retained. The curve is continuous from no resource premium toward the configured maximum; no region-count table remains, and disabling Abundance leaves the base/Government payout. |
| `GlobalTechnologyResearchCostPatch` | Global technology research cost | Aligned. It applies a uniform configurable x1.20 multiplier after TI computes the cost; faction projects use a different template and remain unchanged. |
| `EconomyRegionThresholdPatch` | Fixed accumulated IP | Aligned. Region conversion is a fixed capital project; default x5 makes it harder without border-sensitive scaling. |
| `DecolonizationThresholdPatch` | Fixed accumulated IP | Aligned. The threshold is a political project cost and uses the same guarded multiplier. |
| `FalloutCleanupThresholdPatch` | Fixed accumulated IP per detonation | Aligned. Every blast costs the same to clean; damage concentration is handled separately by land area. |
| `PriorityTooltipPatch` | UI mirror | Aligned. It appends the shared Economy/Spoils return, productivity, both technology-progress axes, labor/resource pressure and constraints, abundance, and climate GDP multiplier while replacing exactly the same five threshold loads as gameplay. |
| `InvestmentTooltipPatch` | UI mirror | Aligned. It exposes GDP base IP, the low-income multiplier, and fixed army/navy upkeep without replacing vanilla text. |
| `TIMissionTemplate.json` Purge override | Mission difficulty | Aligned. The defender's flat modifier rises from 3 to 4 while all national-scale, support, councilor, and alien modifiers remain vanilla. |
| `TIMissionTemplate.json` Enthrall Elites override | Mission difficulty | Aligned. The defender's flat modifier rises from 2 to 3 while retaining vanilla's GDP-based target-nation defense and all other mission modifiers. |
| `TIStartTimeTemplate.json` 2022 override | Starting global technologies | Aligned. Mission to Space and Advanced Chemical Rocketry begin completed; Outpost Habs replaces Mission to Space in the active list so the scenario retains three valid research choices. |

## Compatibility risks deliberately guarded

- Threshold, Unity propaganda, and Spoils propaganda transpilers require exact
  replacement counts and throw during initialization if TI changes the expected IL.
- Feature and global toggles return prefixes to vanilla and make transpiler helpers
  return live vanilla field values.
- Invalid or non-finite calculations retain vanilla or use a documented safe value.
- The metadata and assembly target TI 1.0.49. Verification compiles against the
  installed 1.0.49 assemblies and confirms every guarded gameplay/UI IL anchor.
- The control-cost patch reads `CPMaintenanceModifier` from the live start-time
  template, so scenario balance changes are honored without hiding the mod's
  economy-score/exponent formula.
- The capacity patch enumerates only the five installed stackable project-effect
  IDs and verification locks their additive values and the faction-effect API.
- Resource-dependent side effects consistently observe the Abundance feature
  toggle in gameplay and tooltips.

## Deferred audit item

Event-driven Inequality changes still bypass the shared boundary curve. They
remain the next logical extension because events can still make abrupt changes
near the limits even though Economy, Welfare, and Spoils completions cannot.
