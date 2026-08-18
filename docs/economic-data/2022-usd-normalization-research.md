# Country economic normalization research

Status: research and provenance record. The geography-audited clamp proposal was
implemented in scenario JSON and deployed on 2026-08-17; see
`starting-economic-values-implementation.md` for implementation authority.

This note documents the country comparison tables prepared for the 2003, 2022,
and 2026 Terra Invicta starts. The compact CSV contains the requested nine GDP
per-capita comparisons. The detailed CSV retains the population, total GDP,
scenario scaling, source coverage, and calculation inputs needed to audit or
later implement a chosen basis.

## Artifacts

- [Nine-column GDP-per-capita comparison](country-gdpc-comparison-2022-usd.csv)
- [Full country economic basis](country-economic-basis-detail-2022-usd.csv)
- [Scenario-scaled clamp proposal CSV](country-economic-clamp-proposal-2022-usd.csv)
- [Geography-audited scenario-scaled clamp review workbook](country-economic-clamp-proposal-2022-usd-geography-audited.xlsx)

Both tables have 173 country rows. They cover every country owning at least one
of the 363 starting-region templates in each scenario. The 2003 start contains
172 such countries; 2022 and 2026 contain 173. South Yemen has a nation template
in the latter two data sets but owns no starting region, so it is not a separate
row.

## Vanilla data semantics

The scenario inputs come from the installed Terra Invicta templates:

- 2003: Dark Skies `2003_Nations`, `2003_Regions`, and `2003Start`.
- 2022: base-game `ModernNations`, `ModernRegions`, and `ModernDayStart`.
- 2026: base-game `2026_Nations`, `2026_Regions`, and `2026Start`.

Population is the sum of `population_Millions` for all starting regions owned by
a country. GDP is stored on the nation template, not independently on each
region. On campaign initialization the nation GDP is multiplied by the start
template's `globalStartingGDPScaling`; GDP per capita is then derived by dividing
that effective GDP by the sum of the nation's region populations.

| Start | Regions | Countries | GDP scaling | Effective vanilla GDP total | Vanilla population total |
|---|---:|---:|---:|---:|---:|
| 2003 | 363 | 172 | 1.59 | $53,442,423,527,938 | 6,412,605,647 |
| 2022 | 363 | 173 | 1.00 | $141,682,078,667,850 | 7,972,649,884 |
| 2026 | 363 | 173 | 0.87 | $158,803,932,463,121 | 8,293,772,835 |

The detailed table includes both raw nation-template GDP and effective in-game
GDP so that the scenario-wide scaling is visible rather than silently folded
into the result.

## External source and calculations

External country data comes from the IMF World Economic Outlook public SDMX
dataflow `IMF.RES:WEO(9.0.0)`, retrieved 2026-08-17. The dataflow metadata was
last updated 2025-10-08 and the selected country series generally report a
2025-09-30 update date. Consequently, the 2026 observations are forecasts from
that WEO vintage, not realized 2026 outcomes.

Series used:

- `NGDPD`: nominal GDP in current US dollars.
- `PPPGDP`: GDP in current international dollars at purchasing-power parity.
- `LP`: population.
- United States `NGDP_D`: GDP deflator used to express each year's current-dollar
  values in 2022-dollar purchasing power.

The direct source endpoint is
<https://api.imf.org/external/sdmx/3.0/data/dataflow/IMF.RES/WEO/9.0.0/>. The IMF's
[WEO dataset description](https://www.imf.org/external/datamapper/datasets/WEO)
and [WEO FAQ](https://data.imf.org/Datasets/WEO/Frequently-Asked-Questions)
provide the publication context.

For year `y`:

```text
2022-dollar multiplier[y] = US GDP deflator[2022] / US GDP deflator[y]
nominal GDP in 2022 USD[y] = NGDPD[y] * multiplier[y]
PPP GDP in 2022 international dollars[y] = PPPGDP[y] * multiplier[y]
GDP per capita[y] = adjusted GDP[y] / IMF population[y]
```

| Year | US GDP deflator | Multiplier to 2022 dollars |
|---|---:|---:|
| 2003 | 77.006115 | 1.5326421934 |
| 2022 | 118.022821 | 1.0000000000 |
| 2026 | 132.681448 | 0.8895201460 |

The GDP deflator is used rather than consumer CPI because the quantity being
normalized is total production. The nominal comparison still includes exchange
rate effects. The PPP comparison is labeled “2022 international dollars” because
it is not a market-exchange-rate US-dollar measure; applying the US deflator is
a transparent normalization of the international dollar's US purchasing-power
anchor.

## Table layout

The compact CSV has three metadata columns, followed by exactly nine comparison
columns grouped by year:

1. effective vanilla GDP per capita;
2. IMF nominal GDP per capita, inflation-adjusted to 2022 USD; and
3. IMF PPP GDP per capita, inflation-adjusted to 2022 international dollars.

Quality flags and the source URL follow those nine values. The detailed CSV adds,
for every year, the region count, vanilla population, raw and effective vanilla
GDP, IMF population, deflator and multiplier, adjusted total GDP, adjusted GDP
per capita, and coverage status.

## Coverage requiring manual review

IMF coverage is complete for 158 of the 173 country rows. Missing source values
are left blank. For multi-ISO Terra Invicta countries, available components are
summed and the result is explicitly marked `partial` when any component is
missing.

| Country | Coverage issue |
|---|---|
| Afghanistan | 2026 population, nominal GDP, and PPP GDP missing |
| Cuba | All three measures missing in all three years |
| Eritrea | All three measures missing in 2022 and 2026 |
| France | Monaco missing throughout; Andorra population also missing in 2003 |
| Italy | San Marino population missing in 2003 |
| Lebanon | All three measures missing in 2026 |
| North Korea | All three measures missing in all three years |
| Pakistan | 2026 nominal GDP missing; population and PPP GDP available |
| Palestine | All three measures missing in 2026 |
| Somalia | All three measures missing in 2003 |
| South Sudan | All three measures missing in 2003, before independence |
| Sri Lanka | All three measures missing in 2026 |
| Syria | All three measures missing in 2022 and 2026 |
| United Kingdom | Jersey and the Isle of Man missing throughout |
| Yemen | All three measures missing in 2003 |

These flags identify the first set of manual-adjustment cases. The compact table
should not be treated as a drop-in gameplay patch until those blanks and partial
aggregates have an explicit policy.

## Population geography audit

The first clamp draft used IMF WEO `LP` as its population denominator. That is
not safe for game geography: WEO country series can follow the population
reported by a recognized government or statistical jurisdiction, while a Terra
Invicta country may represent a full disputed territory, several sovereign
states, or overseas regions. Yemen exposed the problem most clearly: the IMF
2022 series reports 17,200,294 people, but the game owns both the Yemen and Aden
regions and therefore represents the full territorial country rather than only
the population under one reporting authority.

The revised proposal uses the United Nations
[World Population Prospects 2024](https://www.un.org/development/desa/pd/world-population-prospects-2024)
Medium variant as the population source. WPP covers countries and areas rather
than only WEO reporting jurisdictions. Its
[methodology](https://www.un.org/development/desa/pd/sites/www.un.org.development.desa.pd/files/files/documents/2024/Jul/undesa_pd_2024_wpp2024_methodology-report_web.pdf)
prefers de facto population and describes coverage adjustments when a census
does not cover the full referenced territory. The source is therefore a better
match for region ownership, though estimates for conflict states remain
uncertain; the methodology records Yemen's last census as 2004 and Somalia's as
1987.

Every proposed population more than 5% above or below vanilla was checked
against the game-owned region set and WPP country/area scope. The workbook's
`Population Audit` sheet contains all 39 threshold cases plus corrected
composites, for 67 audited country-year rows. The outcome distinguishes a real
estimate-vintage change from a geographic mismatch; a deviation is not capped
at 5% merely because it triggered review.

Key geographic corrections are:

- Yemen uses full-territory WPP population. The revised figures are 21,456,379
  in 2003, 38,222,876 in 2022, and 42,961,653 in 2026. The 2003 and 2026 values
  exactly match vanilla; the 2022 value is 21.87% above vanilla because WPP 2024
  revised the historical estimate, not because half the country disappeared.
- The WPP Somalia total includes Somaliland. It is partitioned between game
  Somalia and Somaliland using their vanilla regional shares in each scenario.
  This preserves the exact 2003 and 2026 combined totals and raises both 2022
  components by 4.75%, without double counting.
- Lesser Antilles adds Antigua and Barbuda, Dominica, Grenada, Saint Vincent and
  the Grenadines, and Saint Kitts and Nevis. The expanded set exactly reproduces
  the 2003 and 2026 vanilla populations.
- Micronesia adds the Marshall Islands, Nauru, and Palau; the expanded set
  reproduces the 2003 and 2026 vanilla values within one person.
- Polynesia adds Tuvalu and the Cook Islands; the expanded set exactly
  reproduces the 2003 and 2026 vanilla values.
- Denmark adds Greenland and the Faroe Islands; the Netherlands adds Aruba,
  Curaçao, Sint Maarten, and the Caribbean Netherlands; France, the United
  Kingdom, and the United States add the populations represented by their game
  overseas regions.

For a single-ISO game country, the manual check confirms that the game region
and WPP country/area refer to the same geographic unit. Large remaining
differences—such as Moldova, Lebanon, Syria, Turkmenistan, and Seychelles in
2022—are retained as updated WPP estimates, not treated as boundary mistakes.
The proposal CSV records the reviewed population scope, percentage deviation,
review status, and a country-specific geography note on every row.

## Follow-up proposal: scenario-scaled plausibility clamp

The follow-up proposal uses the existing start multiplier as the operational
inflation adjustment. For each country-year:

```text
nominal boundary = historical nominal GDP / filled population * scenario scaler
PPP boundary = historical PPP GDP / filled population * scenario scaler
lower = MIN(nominal boundary, PPP boundary)
upper = MAX(nominal boundary, PPP boundary)
proposed GDP/c = MAX(lower, MIN(vanilla GDP/c, upper))
proposed effective GDP = proposed GDP/c * proposed population
proposed JSON initialGDP = proposed effective GDP / scenario scaler
```

This is equivalent to the requested rule: if vanilla is below both external
values it moves to the lower, and therefore nearer, boundary; if it is above
both it moves to the higher boundary; if it lies between them it is retained.

Using the scenario multiplier rather than the independent IMF deflator is close
to, but not exactly the same as, a strict 2022-dollar conversion. The 2003
scenario's `1.59` scaler is 3.742% above the IMF-deflator multiplier of
`1.5326421934`; the 2026 `0.87` scaler is 2.194% below the IMF-deflator multiplier
of `0.8895201460`. The proposal intentionally accepts those small differences so
that historical source values can remain in nation JSON and the existing start
scaler performs the runtime normalization. The IMF deflator is retained only
inside inference calculations that compare a missing target year with a source
observation from another year.

| Start | Keep vanilla | Clamp to nominal | Clamp to PPP | Country rows |
|---|---:|---:|---:|---:|
| 2003 | 135 | 27 | 10 | 172 |
| 2022 | 153 | 11 | 9 | 173 |
| 2026 | 147 | 17 | 9 | 173 |

The proposal CSV uses one row per country-year, for 518 rows in total. It includes
the filled population, population scale factor, both scenario-scaled GDP/c
boundaries, the clamp decision, proposed effective GDP, and the JSON `initialGDP`
that would reproduce the proposal after the scenario multiplier is applied. The
workbook contains the same data with live clamp and GDP formulas, a summary,
policy sheet, and a component-level gap log.

### Secondary source and fill hierarchy

World Bank World Development Indicators were retrieved through the official
[Indicators API](https://datahelpdesk.worldbank.org/knowledgebase/articles/898581-api-basic-call-structures)
on 2026-08-17. The API response reports a 2026-07-13 last-update date. The added
series are:

- `SP.POP.TOTL`: population;
- `NY.GDP.MKTP.CD`: GDP in current US dollars; and
- `NY.GDP.MKTP.PP.CD`: GDP in current international dollars at PPP.

The numerical hierarchy is:

1. UN WPP Medium population for the reviewed game geography;
2. exact IMF target-year GDP observation;
3. exact World Bank target-year GDP observation;
4. population interpolation or same-country CAGR extrapolation, capped at
   ±5% per year;
5. GDP grown or backcast from the nearest country observation using median real
   GDP/c growth for the same World Bank region and income group, capped at ±8%
   per year;
6. if the country has no nominal history, the peer group's median nominal GDP/c;
7. if it has no PPP history, filled nominal GDP multiplied by the peer group's
   median PPP/nominal ratio.

A peer group requires at least five usable observations. The fallback sequence
is region plus income, region, income, then all available countries. All 271
secondary, inferred, proxy, and territorial-partition component operations are
listed in the workbook's `Gap Log`. At the country-year level, 476 of 518 rows
use complete primary GDP components and 42 use at least one secondary, proxy,
or inferred component. No proposed population, nominal boundary, or PPP boundary
remains blank.

Territorial exceptions are explicit:

- World Bank `CHI` (Channel Islands) is used as the population and nominal-GDP
  proxy for Terra Invicta's missing `JEY` component. This includes Guernsey as
  well as Jersey and therefore slightly overstates the United Kingdom aggregate.
- Somalia and Somaliland share the WPP/IMF Somalia territorial aggregate. Both
  population and GDP are partitioned by the scenario's vanilla regional
  population shares so that the two game countries sum to one external total.
- Small composite and overseas components absent from IMF are filled from exact
  World Bank observations when available and otherwise follow the documented
  peer-inference hierarchy.

The approved proposal is implemented in partial nation and region JSON
overrides. Changed country populations are distributed proportionally across
their existing region shares, with final-region decimal correction so each
country total matches the reviewed proposal. The scenario GDP scalers remain
unchanged.

## Interpretation for the tuning decision

- Normalizing to 2022 makes the 2022 external columns unchanged and provides a
  clear common unit for comparing starts. Choosing another base year would
  rescale every nominal and PPP result by a common price-index relationship; it
  would not remove exchange-rate or PPP methodology differences.
- Nominal GDP is the better proxy for access to internationally traded inputs,
  but it can make exchange-rate crises and commodity cycles dominate a country's
  apparent capacity.
- PPP GDP is the better proxy for domestic productive and consumption capacity,
  but it can overstate access to imported technology, weapons, launch services,
  and other tradeable inputs.
- A hybrid or manual policy is therefore defensible for gameplay. The missing
  and partial cases above, plus countries with extreme nominal/PPP spreads, are
  the natural review set before implementation.
