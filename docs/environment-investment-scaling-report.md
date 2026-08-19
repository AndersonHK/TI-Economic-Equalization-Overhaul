# Geometric environment score, emissions, and atmospheric cleanup

## Status

Planning report only. No gameplay code, scenario data, build, or deployed package has been changed as part of this work.

This report supersedes the earlier proposal that every displayed Environment point should have the same cost. The revised design makes both emissions and advancement geometric, in the same broad structural spirit as Military technology, while retaining separate parameters for the two curves.

## Executive decision

The Environment rating should be a bounded, technology-relative decarbonization score rather than the reciprocal of a hidden sustainability value.

- The displayed score ranges from 0 to 10.
- Emissions fall geometrically as the score rises. A preliminary decay factor of 0.5 per point gives the right physical order of magnitude: score 1 emits half as much as score 0, score 2 emits one quarter, and score 2.9 emits about 13.4%.
- Advancement cost is geometric in the country's **fraction of the currently available technology range**, not in raw displayed points. Thus 0-to-1.5 under a cap of 3 costs exactly the same as 0-to-5 under a cap of 10, before country modifiers.
- Investment time must be essentially independent of country size when countries devote the same fraction of their IP to Environment. Required IP therefore scales with GDP just as IP generation does.
- The technology ceiling is separate from the score and from national starting conditions. Early scenarios can have an attainable ceiling near 3; future technology can raise it to 10.
- Rating 10 means routine national greenhouse-gas emissions are zero: the country is effectively carbon neutral.
- Only a country already at the absolute rating of 10 can use subsequent Environment completions for direct atmospheric removal. Removal is a fixed packet per completion, smaller than vanilla, and stops once the persistent greenhouse warming anomaly reaches 0 C.

This gives the score a consistent meaning: ordinary investment moves a country along a decarbonization curve, while the post-10 priority represents industrial carbon removal rather than further growth of an unbounded rating.

## What is failing now

The current display is effectively:

```text
displayed rating = 1 / raw Sustainability
```

The current late-game EU save has raw Sustainability `0.09398031`, which is a reciprocal score of `10.6405` before the UI caps it at `9.99+`. Raising it from displayed 5 to the cap only requires reducing the raw value from about `0.194` to `0.094`: just `0.10` of cleanup. That explains how an EU with roughly $168.6 trillion GDP can cross the apparent upper half of the scale in less than a year with under 10% of its IP.

The display is therefore not a real progression scale. Equal changes to the raw value become increasingly large displayed gains near zero. It also allows a nominally maximum country to continue producing substantial GDP-driven emissions. Under the current mod formula, the saved EU would still emit about 1.43 billion tonnes of CO2 per year at resource intensity 1 despite appearing to be beyond maximum rating.

The mod currently suppresses vanilla Environment-priority removal of CO2, methane, and nitrous oxide. This avoided vanilla's excessive atmospheric removal, but it leaves no distinct post-carbon-neutral use for the priority.

## Geometric emissions curve

For score `R` below 10, the initial CO2 model should be:

```text
CO2 tonnes/year =
    GDP in billions
  * score-zero CO2 tonnes per GDP-billion
  * emissionDecayBase ^ R
  * resource and sector adjustments
```

At `R >= 10`, routine GDP-driven CO2, CH4, and N2O emissions take a hard branch to zero. The curve approaches neutrality; the endpoint guarantees it.

An initial `emissionDecayBase` of 0.5 produces:

| Score | Emissions relative to score 0 | Interpretation |
|---:|---:|---|
| 0 | 100% | highly carbon-intensive reference economy |
| 0.415 | 75% | oil-versus-coal yardstick |
| 1 | 50% | gas-versus-coal yardstick |
| 2 | 25% | extensive fuel switching and efficiency |
| 2.9 | 13.4% | plausible near-starting-tech frontier |
| 3 | 12.5% | approximately one eighth of score 0 |
| 5 | 3.125% | deeply decarbonized advanced economy |
| 9 | 0.195% | residual emissions only |
| 10 | 0% exactly | carbon neutral endpoint |

The fuel labels are calibration yardsticks, not literal definitions of whole economies. A national score summarizes electricity, heating, transport, industry, agriculture, extraction, and land-use systems. Nevertheless, the curve captures the important physical fact that a score near the starting technology ceiling should emit a fraction of the pollution of an economy that burns coal and oil without restraint.

The decay base should be tested over approximately 0.45-0.65. A value of 0.5 is a strong first hypothesis because it maps gas to one point and places 2.9 at 13.4%, but historical fit decides the final number.

### Different gases need different calibration

CO2 is closely connected to fuel and industrial energy. Much of CH4 and N2O instead comes from agriculture, waste, leakage, and fertilizer use. Applying one CO2-shaped multiplier to all three gases would produce an attractive curve but poor historical results.

The implementation should therefore expose separate score-zero intensities and decay parameters for CO2, CH4, and N2O, or split each gas into reducible and hard-to-abate components. All routine components still reach zero at rating 10 to preserve the requested carbon-neutral endpoint. The design must also explicitly decide whether fluorinated gases remain outside the simulation or require representation; they should not be silently treated as CO2.

## Geometric advancement cost

The cost curve should be defined independently from the emissions curve and normalized to the current technology ceiling. Let:

```text
R = current displayed Environment score
C = current technology ceiling
L = 10 * R / C
```

`L` is advancement position on a normalized 0-to-10 technology envelope. For a score increase from `a` to `b` while the ceiling is `C`:

```text
required IP =
    GDP / referenceGDP
  * baseIP
  * (costGrowthBase ^ (10 * b / C) - costGrowthBase ^ (10 * a / C))
  / (costGrowthBase - 1)
```

This creates the required invariant:

```text
Cost(0 -> 1.5, ceiling 3) = Cost(0 -> 5, ceiling 10)
```

Both endpoints are 50% of the technology then available. More generally, any two movements through the same percentage of their respective ceilings have the same base cost. The inverse cumulative function converts fractional IP into fractional score progress, avoiding whole-completion quantization and making partial investment continuous.

GDP appears in both required IP and national IP generation. Consequently, two countries at comparable development that spend the same percentage of their IP take approximately the same time to advance, regardless of absolute economy size. Rich countries retain advantages arising from better government, cohesion, technology, or priority bonuses, but not an unintended orders-of-magnitude advantage simply because GDP is large.

Military technology demonstrates the useful structure, but its approximate factor of 2 per normalized level would make completing the envelope excessively expensive. A preliminary factor of 1.5 gives the following costs, expressed relative to the first tenth of the available range:

| Progress through current cap | Rating at cap 3 | Rating at cap 10 | Cumulative cost |
|---:|---:|---:|---:|
| 10% | 0.3 | 1 | 1.00 |
| 30% | 0.9 | 3 | 4.75 |
| 50% | 1.5 | 5 | 13.19 |
| 90% | 2.7 | 9 | 74.89 |
| 100% | 3 | 10 | 113.33 |

The final tenth of either range costs 38.44 times the first tenth: 2.7-to-3 and 9-to-10 are equivalent advancement stages.

When technology raises `C`, an existing country's displayed rating `R` does not fall. Its normalized position `L` becomes lower because a larger technological range has opened. Previously spent IP is neither refunded nor recomputed; subsequent marginal progress is priced using the new ceiling. The final `baseIP` and `costGrowthBase` should be selected from gameplay time targets, including the observed late-game EU case. The advancement and emissions bases remain separate configuration values even though both are geometric.

## Technology-relative ceiling

The scale describes progress relative to all technologies that can eventually be unlocked, but a country cannot invest through technology it does not possess.

- Scenario and global technology define an explicit current ceiling, expected to be near 3 around contemporary starts and capable of reaching 10 in late game.
- Advancement difficulty is measured as progress through that current ceiling: half of cap 3 and half of cap 10 are the same cost milestone.
- A country at the current ceiling remains below carbon neutrality unless that ceiling is 10.
- Reaching an early ceiling does not unlock atmospheric cleanup.
- New technologies raise the ceiling and make more of the same geometric curve accessible; they do not redefine old scores.

The current mechanism derives the global sustainability floor from the cleanest starting nation. That creates a circular dependency: recalibrating one nation's historical emissions can change the technology limit for the whole world. It also produces inconsistent inferred starting ceilings of roughly 4.05 in 2003, 2.91 in 2022, and 3.18 in 2026. The replacement must store or calculate the technology ceiling independently of starting national ratings.

## Carbon neutrality and post-10 atmospheric cleanup

Rating 10 is a hard semantic boundary:

1. Routine GDP-driven national CO2, CH4, and N2O emissions are zero.
2. The displayed rating remains exactly 10 and never becomes 10.4 or `9.99+`.
3. Further completed Environment investment removes a fixed packet of greenhouse gases from the atmosphere.
4. The packet is independent of GDP and population. Fractional investment receives a proportional fraction of the packet.
5. Removal is clipped so CO2, CH4, and N2O cannot cross their zero-warming reference concentrations.
6. Removal stops when persistent greenhouse forcing reaches a 0 C anomaly. Temporary negative aerosol forcing must not be used to justify continued removal.
7. At rating 10 and zero persistent greenhouse warming, the priority has no remaining benefit and the AI should stop funding it.

The game currently uses zero-warming reference concentrations of approximately 325.68 ppm CO2, 1.3 ppm CH4, and 0.29 ppm N2O. The removal packet should initially be tested at 25-50% of the corresponding vanilla priority packet, with 25% as the conservative first candidate. Its final magnitude is a gameplay parameter: it must produce useful multi-year cleanup without allowing a few large countries to erase centuries of accumulation in months.

## Historical scenario audit

The model must be evaluated against all three supported starting settings. Official EDGAR 2025 country inventories provide actual CO2, CH4, and N2O through 2024. Since 2026 emissions are not yet historical, the 2026 scenario must use 2024 country distribution plus the latest Global Carbon Budget projection as a documented proxy rather than claiming a nonexistent 2026 observation.

The current scenario templates imply the following effective world GDP after scenario scaling:

| Start | Effective world GDP |
|---|---:|
| 2003 | $71.37 trillion |
| 2022 | $146.40 trillion |
| 2026 | $165.24 trillion |

EDGAR totals and the subset mapped to the 167 represented Terra Invicta starting nations are:

| Benchmark year | Coverage | CO2 | CH4 | N2O |
|---|---|---:|---:|---:|
| 2003 | EDGAR world | 27.642 Gt | 268.762 Mt | 7.720 Mt |
| 2003 | TI-mapped nations | 26.647 Gt | 267.764 Mt | 7.615 Mt |
| 2022 | EDGAR world | 38.548 Gt | 326.062 Mt | 9.328 Mt |
| 2022 | TI-mapped nations | 37.307 Gt | 325.079 Mt | 9.195 Mt |
| 2024 proxy for 2026 | EDGAR world | 39.633 Gt | 332.893 Mt | 9.538 Mt |
| 2024 proxy for 2026 | TI-mapped nations | 38.209 Gt | 331.934 Mt | 9.392 Mt |

The mapped totals are the appropriate target for summing simulated nations; the full world totals are a cross-check for mapping omissions and international categories. Global Carbon Budget 2025 estimates 2024 fossil CO2 at about 37.8 Gt and projects 2025 at about 38.1 Gt. Differences from EDGAR are expected because the inventories do not use identical scopes, particularly for non-fossil and process categories.

### Existing scores cannot simply be retained

A shared 0.5 emissions-decay curve fitted to the existing national ratings cannot reproduce all starts with one physical score-zero intensity. A preliminary country-weighted pass produced world ratios of approximately:

| Start | Predicted / historical emissions using existing scores |
|---|---:|
| 2003 | 0.63 |
| 2022 | 1.13 |
| 2026 using 2024 proxy | 1.14 |

The direction of the error changes by scenario. Tuning one global coefficient would merely move the mismatch elsewhere. The existing scenario ratings must therefore be back-solved from historical national emissions, and not treated as authoritative inputs.

Every back-solved national score must fit inside its scenario's explicit technology range without clipping:

```text
0 <= starting national score <= scenario technology ceiling
```

This is a hard calibration constraint across the entire dataset, with 2003 China and 2026 Switzerland serving as useful dirty-economy and clean-economy boundary checks. If either country—or any other represented country—would require a score below 0 or above its period cap, the score-zero baseline, decay, or justified sector/resource adjustment must be refitted. Silently clamping the score is not acceptable because it would conceal an emissions mismatch.

For each country and gas, the calibration begins with:

```text
R = log(actual emissions / (GDP * score-zero intensity * adjustments))
    / log(emissionDecayBase)
```

Resource extraction, sector composition, and unavoidable inventory-boundary differences must then be handled explicitly. A single GDP-plus-score formula cannot reproduce both petroleum exporters and service economies at country level. Adjustments should be few, legible, and derived from stable scenario data rather than individual unexplained fudge factors.

## Proposed acceptance margins

Historical agreement should be judged at both world and country scale:

- TI-mapped CO2 total within +/-10% for the 2003 and 2022 starts.
- 2026 start within +/-10% of the documented 2024-2025 proxy band, labelled as a projection test rather than historical validation.
- Major emitting countries within +/-25% for CO2, covering at least 80% of mapped emissions by weight.
- Smaller, aggregated, or boundary-mismatched countries within +/-35-50%, with outliers documented.
- Global CH4 and N2O within +/-15%; major country emitters within approximately +/-30-35% until agricultural and waste-sector structure is richer.
- Oil-intensive and gas-intensive economies should fall near the 75% and 50% fuel yardsticks after controlling for their non-energy sectors.
- A country near score 2.9 should normally emit only a fraction of an otherwise comparable score-0 country, with approximately 13% as the 0.5-decay starting hypothesis.
- Every starting nation fits within `[0, scenario cap]` without score clipping, explicitly including 2003 China and 2026 Switzerland.
- The advancement-cost identity `Cost(0 -> 1.5, cap 3) = Cost(0 -> 5, cap 10)` holds exactly before national modifiers.
- A score-10 country emits zero routine greenhouse gases regardless of GDP.
- Simulated atmospheric concentration change must also be checked. Matching emitted tonnes is insufficient if tonne-to-ppm conversion or natural sinks produce implausible one-year concentration growth.

## Planning and implementation sequence

No implementation should begin until the curve parameters and historical targets are reviewed.

### 1. Freeze the calibration dataset

- Preserve EDGAR 2025 CO2, CH4, and N2O snapshots and the Global Carbon Budget proxy under `docs/` with source metadata.
- Preserve the TI-to-ISO mapping, exclusions, scenario GDP scaling, and inventory-boundary notes.
- Generate per-country comparison tables for 2003, 2022, and 2026/2024-proxy.

### 2. Calibrate the physical curve

- Fit CO2 decay over the proposed 0.45-0.65 range.
- Fit score-zero intensity and a small set of resource/sector adjustments.
- Determine whether CH4 and N2O need independent bases or reducible/residual components.
- Back-solve starting national scores and require every result to remain within its scenario ceiling without clipping; use 2003 China and 2026 Switzerland as explicit boundary cases.
- Define explicit technology ceilings for each scenario independently of those scores.

### 3. Calibrate the advancement curve

- Test cost growth factors approximately 1.4, 1.5, 1.6, and 2.0.
- Choose `baseIP` from target calendar times at representative priority shares.
- Verify normalized-cap equivalence, especially 0-to-1.5 at cap 3 versus 0-to-5 at cap 10.
- Verify equal-share advancement time across small, medium, and very large economies.
- Reproduce the saved $168.6T EU case and ensure 5-to-10 takes an intentional late-game time rather than less than a year at under 10% allocation.

### 4. Implement pure model functions

- Add explicit score, cumulative cost, inverse progress, gas emissions, technology ceiling, and cleanup eligibility functions.
- Remove reciprocal display semantics and protect save migration.
- Keep physical, cost, scenario, and cleanup parameters separately configurable.

### 5. Recalibrate scenario data

- Apply reviewed starting scores for 2003, 2022, and 2026.
- Add explicit starting technology ceilings.
- Produce before/after country and world audit tables.

### 6. Implement runtime behavior

- Convert Environment investment to continuous geometric progress.
- Route emissions through the per-gas curve with a hard score-10 neutral branch.
- Enable only score-10 fixed-packet atmospheric cleanup.
- Clip cleanup at the zero-warming references and update AI allocation logic.
- Update UI text to show current score, ceiling, progress to the next point, emissions effect, and post-10 cleanup state.

### 7. Verify, deploy, and manually test

- Add unit tests for geometric cost inversion, fractional investment, normalized-cap equivalence, GDP-size neutrality, gas endpoints, ceiling behavior, cleanup gating, and concentration clipping.
- Run scenario regression tests for 2003, 2022, and 2026 against the acceptance margins.
- Test save migration using the supplied late-game save.
- Only after implementation authorization, use the repository's normal build-and-deploy flow and announce the package for immediate in-game testing.
- Record observed manual results and final calibrated parameters in this report.

## Required design checks before implementation

The following remain calibration choices rather than hidden assumptions:

- final emissions decay base, initially 0.5;
- separate CH4 and N2O curve structure;
- final cost growth base and calendar-time target;
- explicit 2003, 2022, and 2026 technology ceilings;
- limited resource/sector adjustment model;
- post-10 removal packet, initially tested at 25-50% of vanilla;
- save conversion from reciprocal Sustainability to the new score;
- treatment of fluorinated gases and land-use CO2.

## Sources

- European Commission Joint Research Centre, [EDGAR 2025 greenhouse-gas dataset](https://edgar.jrc.ec.europa.eu/dataset_ghg2025), providing country CO2, CH4, and N2O inventories through 2024.
- Global Carbon Project, [Global Carbon Budget 2025](https://essd.copernicus.org/articles/18/3211/2026/index.html), providing the 2024 estimate, 2025 projection, uncertainty, and global fuel composition.
- Local Terra Invicta 1.0.51 templates and decompiled game behavior for scenario atmosphere, GDP scaling, sustainability display, emissions conversion, temperature contribution, and vanilla priority packets.
- Supplied late-game save for the EU progression and residual-emissions case.
