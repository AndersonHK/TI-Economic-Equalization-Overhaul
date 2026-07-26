# Patch Sanity Audit

This audit checks each Harmony patch against the directives in
`design-directives.md`. “Stock” names the quantity that prevents linear GDP/IP
from making a proportional effect scale incorrectly.

| Patch | Stock / unit | Sanity result |
|---|---|---|
| `InvestmentPointsPatch` | GDP / $100B | Aligned. Output is legible and linear; the low-income ramp models capital constraints without a discontinuity. |
| `ControlPointCostPatch` | National economy score per CP | Aligned with the retained technology exponent sequence. It makes later governance technology reduce the burden smoothly while preserving free alien CPs. |
| `ArmyUpkeepPatch` | Fixed cost per army | Aligned. Every army pays its own home/away and miltech-dependent upkeep; large nations only benefit by having more IP to support more units. |
| `ResearchPatch` | Population, human capital, institutions | Aligned. This is national productive output rather than an IP completion, so linear population with Education squared is economically legible. |
| `EconomyGrowthPatch` | Fixed total GDP gain per IP | Aligned. The getter converts the total gain to per-capita form only because TI expects that unit; GDP-linear IP makes aggregate monthly growth approximately scale-neutral. |
| `EconomyInequalityPatch` | GDP | Aligned after stock correction. Resources/GDP affects the raw change and the continuous 1-9 boundary transform makes extremes progressively harder. |
| `WelfareInequalityPatch` | GDP | Aligned after stock correction. A tenfold economy gets one tenth the per-IP rating change and approximately tenfold IP. |
| `SpoilsInequalityPatch` | GDP | Aligned after stock correction. Resource dependence matters strongly in small economies and weakly in diversified large economies. |
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
| `EconomyEmissionsPatch` | GDP × carbon intensity | Aligned. GDP, not population or borders, produces emissions; resources/GDP adds a bounded extractive-economy intensity premium. |
| `UnityCohesionPatch` | Population | Aligned. Population scaling prevents unified countries becoming easier to homogenize; Education and Government reduce the effect with a floor. |
| `UnityEducationPatch` | Population | Aligned. The small secondary Education loss scales with the people affected. |
| `UnityPropagandaPatch` | Vanilla demographic effect × configured strength | Aligned. It changes only one config field load and preserves TI 1.0.39 claims and completion logic. |
| `SpoilsGovernmentPatch` | Population / institutions | Aligned with the selected inverse-population behavior. |
| `SpoilsSustainabilityPatch` | GDP | Aligned. Spoils damages carbon intensity, so its per-IP change falls with the economy and rises with resource dependence. |
| `SpoilsPropagandaPatch` | Vanilla demographic effect × configured strength | Aligned. It preserves payout, CP, corruption, sustainability, and emissions branches. |
| `SpoilsMoneyPatch` | Fixed payout × resources/GDP × Government | Aligned. The curve is continuous from no resource premium toward the configured maximum; no region-count table remains. |
| `EconomyRegionThresholdPatch` | Fixed accumulated IP | Aligned. Region conversion is a fixed capital project; default x5 makes it harder without border-sensitive scaling. |
| `DecolonizationThresholdPatch` | Fixed accumulated IP | Aligned. The threshold is a political project cost and uses the same guarded multiplier. |
| `FalloutCleanupThresholdPatch` | Fixed accumulated IP per detonation | Aligned. Every blast costs the same to clean; damage concentration is handled separately by land area. |
| `PriorityTooltipPatch` | UI mirror | Aligned. It appends live calculations and replaces exactly the same five threshold loads as gameplay. |
| `InvestmentTooltipPatch` | UI mirror | Aligned. It exposes GDP base IP, the low-income multiplier, and fixed army/navy upkeep without replacing vanilla text. |

## Compatibility risks deliberately guarded

- Threshold, Unity propaganda, and Spoils propaganda transpilers require exact
  replacement counts and throw during initialization if TI changes the expected IL.
- Feature and global toggles return prefixes to vanilla and make transpiler helpers
  return live vanilla field values.
- Invalid or non-finite calculations retain vanilla or use a documented safe value.
- The metadata target is TI 1.0.39; compilation against installed TI 1.0.47 is a
  forward check, not a claim that 1.0.47 behavior has been fully designed.

## Deferred audit item

Event-driven Inequality changes still bypass the shared boundary curve. They
remain the next logical extension because events can still make abrupt changes
near the limits even though Economy, Welfare, and Spoils completions cannot.
