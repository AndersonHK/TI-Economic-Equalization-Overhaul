# Economy Growth Calibration Plan

This plan replaces the isolated GDP-per-capita decay in `EconomyGrowthPatch`
with the factor-balance model defined in `design-directives.md`. It is a
calibrated proposal, not an implementation.

## Model

An Economy or Spoils completion buys a fixed addition to national capital. The
country's monthly number of Investment Points already scales with GDP, so the
formula below calculates the return of one IP rather than multiplying by
population or GDP a second time.

Use GDP per capita as capital per worker. Effective labor combines Education,
Government, Cohesion, and a smooth Core Economic region bonus:

```text
core = 1 + 1.20 * coreRegions / (2 + coreRegions)
laborSupport =
    core
    * (1 + 0.15 * Education)
    * (1 + 0.05 * Government)
    * (1.20 - 0.04 * abs(Cohesion - 5))
    / referenceLabor
```

`referenceLabor` is the same expression for one Core Economic region,
Education 7, Government 6, and Cohesion 5. A value above one means that labor
and institutions can productively support more capital than this reference
economy.

Resources remain relative to GDP, and land remains relative to population
density. Both are continuous, both require stability to be useful, and neither
receives a technology fade:

```text
resourceRatio = resourceRegions * $1T / max(GDP, $1B)
resourceCurve = resourceRatio^0.30 / (1 + resourceRatio^0.30)
resourceBonus = resourceCurve * stability

landRatio = 50 / max(populationDensity, 0.1)
landRelevance = 0.25 + 0.75 / (1 + GDPPerCapita / $30,000)
landBonus = 0.25 * landRatio / (1 + landRatio)
            * stability * landRelevance

resourceSupport = 1 + resourceBonus + landBonus
stability = clamp(1 - Unrest / 10, 0, 1)
```

For example, one resource region in a $1T stable economy gives
`resourceRatio = 1` and a `+50%` resource bonus. At $10T, the same region gives
`resourceRatio = 0.1` and a still-material `+33%`; at $100B it gives `+67%`.
The deliberately shallow `0.30` exponent lets oil and mineral endowments remain
useful to large diversified economies without making the benefit independent
of GDP. At density `50/km2`, the raw land curve is one half; before stability
and wealth relevance, that is a `+12.5%` support bonus.

Capital presses separately against labor and physical-resource constraints.
Technology raises the minimum return preserved under each constraint:

```text
technologyRelief(p) = p * (0.10 + 0.90 * p)

laborFloor = 0.35 + 0.65 * technologyRelief(laborTechnologyProgress)
resourceFloor = 0.45 + 0.55 * technologyRelief(resourceTechnologyProgress)

laborPressure = (GDPPerCapita / $37,500) / laborSupport
resourcePressure = (GDPPerCapita / $55,000) / resourceSupport

laborConstraint =
    laborFloor + (1 - laborFloor) / (1 + laborPressure^1.4)
resourceConstraint =
    resourceFloor + (1 - resourceFloor) / (1 + resourcePressure^1.2)
```

The return of one IP is then:

```text
GDP gain in billions =
    $1.00B
    * productivityTechnologyMultiplier
    * laborConstraint
    * resourceConstraint
    * (1 + resourceBonus + 0.25 * landBonus)
```

Resource and land endowments both relax the corresponding scarcity constraint
and lift the attainable return, with diminishing curves in both places.
Unrest does not receive a second whole-economy multiplier: TI already reduces
monthly IP by `max(Unrest - 2, 0) / 10`. It does still drive resource and land
benefits to zero because an unstable state cannot exploit those endowments
reliably.

For the canonical shape test—`$4T` GDP, `100M` people, two Resource regions,
one Core Economic region, density `50/km2`, Education `7`, Government `6`,
Cohesion `5`, and no Unrest—doubling capital alone increases total new GDP by
about `1.30x` at 2022 technology. Doubling capital, population, and Resource
regions together produces exactly `2x`. Capital-only doubling progresses
smoothly through about `1.36x`, `1.50x`, and `1.70x` at 25%, 50%, and 75%
weighted technology, reaching `1.93x` at maximum substitution. The constraints
therefore flatten gradually while fixed physical endowments retain a small
economic advantage instead of receiving a technology fade.

## Technology weighting

Change `Config/economy-tech-weights.csv` to:

```text
tech_id,enabled,productivity_percent,labor_substitution,resource_substitution,rationale
```

Every global technology should have a non-zero spillover in all three columns;
the weights describe emphasis, not exclusive categories:

- AI, computing, robotics, and automation receive high labor-substitution
  weight and lower resource-substitution weight.
- Energy, materials, mining, launch, and space-industry technologies receive
  high resource-substitution weight and lower labor-substitution weight.
- Agriculture and biotechnology lean toward resource and land substitution.
- General-purpose social and economic technologies are approximately balanced.
- Weapon-, propulsion-, and spacecraft-only technologies receive small
  non-zero spillovers rather than being treated as economically inert.

Productivity remains the compounded product of each completed technology's
configured percentage, with the configurable safety cap retained at `4x`.
Labor and resource progress are each the completed weight divided by the total
enabled weight for that axis. Redistribute the existing weights when adding the
currently omitted technologies so the whole-tree productivity multiplier stays
near the calibrated `3.40x` instead of reaching the cap prematurely.

The relief curve gives every technology an immediate effect but back-loads the
flattening of the capital constraint. At 25%, 50%, 75%, and 100% weighted
progress it supplies about 8%, 28%, 58%, and 100% of the possible constraint
relief. The 2022 baseline already embodies historical technology; Mission to
Space and Advanced Chemical Rocketry provide the starting in-game `1.0201x`
productivity multiplier, while new global technologies move the additional
progress axes from zero toward one.

## Climate model and comparison baselines

The comparison uses TI 1.0.47's installed climate-damage code. Temperature
begins at `1.2601 C`, calculated from the 2022
scenario's starting CO2, methane, and nitrous oxide values. It then follows a
configurable linear path. The calibrated default reaches `2.7 C` in 2050; the
temperature control retains `3.0 C` as a reproducible stress case. This changes
only the simulator's scenario assumption, not TI's climate equation.

For temperature `T > 0.25 C`, TI calculates:

```text
excess = T - 0.25
annualClimateDamage =
    ((0.14577 * excess^2 + 0.31839 * excess)
    * 1.14^Inequality / 100)
    * populationWeightedRegionExposure
```

Vanilla and the deployed benchmark use that damage unchanged. The proposal
multiplies only the resulting GDP damage by `0.90`; temperature, regional
exposure, and Inequality feedback retain their normal meanings.

The installed region templates assign Beneficiary, Standard, and Vulnerable
populations exposure weights `0`, `1`, and `2`. TI converts the annual loss to
an equivalent compounded monthly GDP loss. Every month also adds one fifth of
that monthly loss to Inequality; the deployed mod's existing `2x` climate
Inequality setting is applied to the current and proposed models, while vanilla
uses `1x`.

The comparison baselines are:

- **Vanilla 1.0.47:** monthly IP is `GDP_billions^0.35`; Economy adds
  `(3 + 1.5 * resources + 1.5 * cores + 0.5 * Government + Education) *
  (population / 50M)^-0.35` dollars per person per IP.
- **Current deployed mod:** monthly IP uses the mod's linear GDP formula and
  Economy uses the current `0.33 * 0.40`, `6 * 0.96^(GDP/c / 1000)` formula.
- **Proposed:** the factor-balance formula above and the same linear-IP formula
  as the current mod.

All three include TI's normal Unrest IP penalty. Advisers, army upkeep,
occupation, population change, faction-project Economy modifiers, and
endogenous greenhouse-gas feedback are excluded so the national growth
formulas are being compared on the same inputs. In vanilla, all selected
growth allocation is treated as Economy because vanilla Spoils does not add
GDP; this is therefore the favorable vanilla comparison.

## Calibration results

The simulation holds population, Education, Government, Cohesion, Unrest,
regions, and density fixed. It recalculates GDP, GDP per capita, Investment
Points, resource/GDP, climate damage, and climate-driven Inequality every
month. These are still controlled comparisons rather than historical
forecasts: war, demographics, political change, trade, events, and autonomous
productivity growth are deliberately absent.

The following net 2022 annual rates assume 50% of monthly IP goes to a
GDP-growing priority. They already deduct the starting climate loss. The 2050
columns assume a path to `2.7 C` and 50% weighted technology completion. The
deployed benchmark keeps its actual `3.7964x` full-tree productivity trajectory;
only the proposal uses the newly calibrated `3.40x` trajectory:

| Country | Proposed net | Current net | Vanilla net | Proposed 2050 GDP/c | Current 2050 GDP/c | Vanilla 2050 GDP/c |
|---|---:|---:|---:|---:|---:|---:|
| United States | 2.6% | 0.7% | 3.1% | $130,770 | $66,362 | $95,184 |
| Canada | 5.4% | 3.9% | 3.9% | $226,334 | $92,273 | $114,324 |
| Australia | 4.6% | 2.8% | 3.2% | $167,273 | $72,807 | $82,931 |
| Germany | 2.4% | 1.9% | 2.5% | $109,923 | $70,421 | $77,250 |
| United Kingdom | 2.5% | 2.6% | 2.7% | $98,129 | $66,739 | $68,813 |
| China | 5.7% | 7.8% | 8.6% | $71,824 | $50,634 | $55,728 |
| India | 4.0% | 8.7% | 8.6% | $26,327 | $38,480 | $21,373 |
| Nigeria | 2.2% | 3.8% | 4.5% | $9,376 | $14,285 | $8,538 |
| Brazil | 4.6% | 8.5% | 5.5% | $48,765 | $46,775 | $31,432 |
| Mexico | 2.0% | 3.8% | 2.1% | $28,684 | $32,300 | $19,818 |
| Egypt | 3.4% | 6.2% | 3.6% | $34,869 | $39,733 | $21,990 |
| Indonesia | 4.9% | 9.2% | 5.8% | $42,064 | $41,595 | $23,523 |

The resource counterfactual is now visible in the calibration: at 50%
allocation, resource endowment adds about `0.9` percentage points to US gross
growth and materially more to Canada and Australia. After their different
Unrest and TI climate-exposure penalties, the proposed net rates are about
`2.6%`, `5.6%`, and `4.6%`, versus `2.4%` for Germany and `2.5%` for the UK.

The comparison confirms that both old baselines are aggressive where it
matters most. Even after roughly `1%` of starting climate drag, vanilla gives
China and India about `8.6%` annual growth at 50% allocation; the current mod
gives India `8.7%`, Brazil `8.5%`, and Indonesia `9.2%`. Vanilla has no
GDP-per-capita return constraint at all, so its Economy effect remains
mathematically unbounded even when the increasing climate loss eventually
drives net growth toward zero. The current mod has a bound, but its starting
low-income return is too large and then collapses too sharply.

Maximum technology is intentionally a post-scarcity boundary, not the expected
midgame state. The calibrated tree finishes near `3.40x`, below the configured
`4x` safety cap, and the more back-loaded relief curve keeps most of the
campaign below the nearly linear tail.

## Implementation sequence

1. Expand and validate the technology CSV. Reject duplicate IDs; log and skip
   unknown IDs; require finite, non-negative weights and a positive total for
   each axis.
2. Add the new constants to the existing Economy settings group. Remove the old
   standalone GDP-per-capita decay and technology-fade settings rather than
   carrying dead compatibility switches.
3. Inline the complete calculation in `EconomyGrowthPatch` in the order shown
   above. Keep the patch readable end to end and comment the representative
   `$1T`/`$10T` resource examples and the 2022 versus maximum-technology shape.
4. Make the Economy and historical-GDP tooltips call the same Economy getter
   and display base gain, technology lift, labor constraint, resource
   constraint, resource/land lift, and the shared Economy/Spoils result.
5. Add dependency-free sweeps for GDP/c from `$500` to `$1M`, zero to maximum
   Unrest, zero resources, near-zero density, and technology progress from zero
   to one. Assert finite output, monotonic technology relief, decreasing
   marginal return when capital alone grows, scale neutrality when all factors
   grow together, and near-linearity at maximum substitution.
6. Re-run the representative-country simulation using values extracted from the
   installed 1.0.47 templates or a 2022 save before accepting defaults. Treat
   the table above as the calibration envelope, not an exact snapshot contract.
