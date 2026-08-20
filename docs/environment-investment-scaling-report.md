# Environment investment and emissions implementation report

Status: implemented, automatically validated, and deployed for Terra Invicta 1.0.51 on 2026-08-19.

## Result

The Environment system now treats the displayed 0-10 value as a technology-bounded sustainability rating. The reciprocal value remains only as an internal save-compatible carrier.

- National emissions decline geometrically with the displayed rating.
- Rating 10 is an exact carbon-neutral endpoint for routine CO2, CH4, and N2O emissions.
- Environment advancement cost is geometric and normalized to the fraction of the current technology cap.
- Required IP scales linearly with GDP, cancelling the mod's GDP-linear IP output for developed countries. Equal allocation shares therefore take equal time regardless of economy size.
- The starting technology cap is 3 and rises independently of national ratings to an absolute cap of 10.
- Only nations already at rating 10 can use Environment completions for fixed atmospheric removal.
- Starting ratings for all 518 country-scenario entries were recalibrated against 2003, 2022, and 2024 emissions data, with 2024 used explicitly as the 2026 proxy.

This directly fixes the reported late-game failure. A very large EU can no longer traverse the upper half of the rating in under a year simply because its GDP is enormous, and a country displayed at 10 no longer continues routine GDP-driven emissions.

## Rating and save compatibility

The runtime conversion is:

```text
rating = clamp(1 / storedSustainability - 0.1, 0, 10)
storedSustainability = 1 / (rating + 0.1)
```

The `0.1` offset allows the existing strictly positive save field to represent rating 0 and preserves late-game saves without a destructive migration. The supplied EU's stored value of `0.09398031` converts to the absolute cap of 10. Old values are clamped at the new endpoints and all new UI paths show the real rating and current technology cap.

## Geometric emissions

Below rating 10, annual routine emissions are:

```text
CO2 tonnes = GDP_billions * 2,000,000 * 0.25^rating * resourceIntensity
CH4 tonnes = population_millions * 51,248.90 * 0.90^rating
N2O tonnes = population_millions * 1,452.04 * 0.90^rating
```

At rating 10 all three getters take a hard branch to zero. The endpoint is therefore truly carbon neutral rather than merely close to neutral.

CO2 uses the steep `0.25` base required to fit the observed contemporary range inside the starting cap of 3. It also makes the fuel-intensity yardsticks intuitive:

| Rating | CO2 relative to rating 0 | Interpretation |
|---:|---:|---|
| 0 | 100% | extremely carbon-intensive reference economy |
| 0.208 | 75% | oil-versus-coal yardstick |
| 0.5 | 50% | gas-versus-coal yardstick |
| 1 | 25% | major fuel switching and efficiency |
| 2 | 6.25% | deeply cleaned power and transport systems |
| 2.9 | 1.79% | near the contemporary technology frontier |
| 3 | 1.56% | starting-cap frontier |
| 5 | 0.098% | advanced, residual-carbon economy |
| 9 | 0.00038% | negligible residual CO2 |
| 10 | 0% exactly | carbon neutral |

The oil and gas rows are whole-economy calibration yardsticks, not claims that a nation's entire inventory consists of one fuel. CH4 and N2O use their own shallower `0.90` curves because their historical totals are much more strongly explained by population, agriculture, waste, and fertilizer use than by GDP. Keeping separate gas curves avoids forcing an attractive CO2 curve onto physically different inventories.

## Geometric advancement cost

Let `R` be the current rating, `C` the current technology cap, and `L = 10R/C` the position on the normalized technology envelope. Moving from rating `a` to `b` costs:

```text
required IP =
    GDP_billions / 100
  * 0.125
  * (1.5^(10b/C) - 1.5^(10a/C))
  / (1.5 - 1)
```

The growth base of `1.5` makes later progress increasingly difficult. In particular, 9-to-10 is much harder than 0-to-1. Fractional IP is inverted through the cumulative curve, so progress is continuous even though Terra Invicta delivers priorities through completion events.

Normalizing rating by the current cap provides the requested exact identity:

```text
Cost(0 -> 1.5, cap 3) = Cost(0 -> 5, cap 10)
```

More generally, the same fractional movement through any cap has the same GDP-normalized cost. When technology raises the cap, the country's displayed rating does not fall; additional technological headroom simply becomes available.

At developed-country income, monthly national IP is `GDP_billions / 100 * 1.05`. Since advancement cost also scales with `GDP_billions / 100`, GDP cancels from the time equation. Before Environment project bonuses or nuclear-fallout penalties, a 10% allocation from 5 to 10 is:

```text
normalized cost factor = (1.5^10 - 1.5^5) / 0.5 = 100.142578125
months = 0.125 * 100.142578125 / (1.05 * 0.10)
       = 119.217 months
       = 9.935 years
```

## Technology cap

The cap no longer depends on whichever country happens to have the cleanest starting value. It begins at 3 and gains explicit headroom from global technologies:

| Global technology | Cap increase |
|---|---:|
| Arrival International Development | +1 |
| Clean Energy | +2 |
| Climate Change Mitigation | +2 |
| Designer Lifeforms | +1 |
| Integrated Earth-Space Economy | +1 |
| **Maximum** | **10** |

A nation at rating 3 while the cap is 3 is at the contemporary frontier, but it is not carbon neutral and cannot perform atmospheric removal. Cleanup is reserved for the absolute rating of 10.

## Rating-10 atmospheric removal

Below rating 10, Environment investment changes the national rating and does not directly remove atmospheric gas. At rating 10, every completed Environment priority instead removes a fixed packet:

| Gas | Removed per completion |
|---|---:|
| CO2 | 0.00008125 ppm |
| CH4 | 0.000000625 ppm |
| N2O | 0.000000625 ppm |

These are one quarter of the corresponding vanilla base packets and are independent of GDP and population. Each gas is clipped at the game's zero-warming reference concentration: 325.68 ppm CO2, 1.3 ppm CH4, and 0.29 ppm N2O. The priority remains valid while any persistent greenhouse concentration exceeds its reference and stops once all three have reached the zero-greenhouse-warming baseline. It cannot overshoot into artificial negative greenhouse forcing.

Fallout cleanup remains separate. A rating-10 country can still fund the priority for fallout/decontamination even when no greenhouse removal remains.

## Historical calibration

The calibration tool combines the mod's reviewed scenario GDP and population data with EDGAR country inventories. CO2 back-solves the national rating using the `0.25` curve. CH4 and N2O use the same ratings with independently fitted population coefficients and `0.90` decay. Nations without a direct EDGAR mapping are small scenario aggregates and retain a bounded legacy-derived rating.

All 518 entries fit within the starting technology cap without clipping. The 167 EDGAR-matched nations in each scenario produce:

| Scenario | Matched rating range | Predicted / observed CO2 | CH4 | N2O |
|---:|---:|---:|---:|---:|
| 2003 | 0.0237-2.9267 | 1.0000 | 1.0117 | 1.0079 |
| 2022 | 0.6238-2.7806 | 1.0000 | 0.9914 | 0.9930 |
| 2026 using 2024 proxy | 0.7849-2.8154 | 1.0000 | 0.9991 | 1.0004 |

CO2 totals match by construction because CO2 is the rating calibration target. The independent CH4 and N2O totals remain within 1.2% in every scenario.

The requested boundary examples both fit comfortably under cap 3:

| Country-scenario | Calibrated rating | CO2 inventory |
|---|---:|---:|
| China 2003 | 0.4065 | 4.773 Gt/year |
| Switzerland 2003 | 2.2762 | 45.86 Mt/year |
| China 2026 | 1.1293 | 13.125 Gt/year |
| Switzerland 2026 | 2.8154 | 33.73 Mt/year |

The persistent country-level audit is `docs/environment-calibration/historical-start-calibration.csv`. The economic override generator now consumes that audit, so later GDP/population regeneration cannot silently erase the calibrated ratings.

## EU save samples: 5 to 10 at 10% IP

The following samples use the EU GDP and population stored in the supplied saves. `Deployed formula` recomputes monthly IP through the currently deployed Economic Equalization formula. `Serialized snapshot` instead uses the older cached monthly IP value literally present in each save. Both exclude Environment project-effect bonuses and nuclear-fallout penalties.

| Save date | EU GDP | Per-capita GDP | Deployed formula | Serialized snapshot |
|---|---:|---:|---:|---:|
| 2041-11-03 | $73.165T | $79,034 | 9.93 years | 10.70 years |
| 2044-10-01 | $97.337T | $104,872 | 9.93 years | 10.62 years |
| 2046-09-08 | $121.382T | $129,877 | 9.93 years | 10.52 years |
| 2047-10-01 | $140.125T | $149,150 | 9.93 years | 10.44 years |
| 2048-03-10 | $147.931T | $156,894 | 9.93 years | 10.42 years |
| 2049-02-16 | $168.552T | $177,418 | 9.93 years | 10.37 years |

The sample closest to the reported `$150k` per-capita case is 2047-10-01 at `$149,150`; it takes 119.2 months under the deployed formula, or 125.3 months if its serialized IP snapshot is used. The latest save requires 21,099.1 total Environment IP from 5 to 10 and supplies 176.98 Environment IP/month at a 10% allocation, again giving 119.2 months.

The equality across the deployed-formula column is intentional, not rounding coincidence: all samples exceed the low-income threshold, so GDP cancels exactly. Poor countries below `$15,000` per capita retain the mod's existing 70%-to-100% income productivity ramp and will take modestly longer, but no longer take orders of magnitude longer merely because they have a small economy.

## Verification and remaining manual checks

Automated release verification passed after implementation:

- 1,092 formula assertions, including storage round trips, geometric inversion, cap normalization, GDP scaling, gas curves, hard neutrality, cleanup clipping, and fallout effects;
- all 153 Harmony patches covered by the implementation matrix and emitted against installed TI 1.0.51 assemblies;
- all 518 scenario nation overrides regenerated and verified with a calibrated Environment value;
- aggregate 2003, 2022, and 2026-proxy emissions checks;
- normal package build and deployment to the enabled mod directory.

Manual in-game testing should now concentrate on UI readability, fractional progress across monthly priority completions, project-effect modifiers, AI behavior at the current cap and absolute cap, and whether roughly ten years at 10% feels correct in a real late-game campaign. The coefficients are deliberately exposed in `Settings.xml`, so the calendar target and cleanup packet can be tuned without redesigning the model.

## Artifacts and sources

- `docs/environment-calibration/historical-start-calibration.csv`: country-scenario emissions and rating audit.
- `docs/environment-calibration/eu-save-timing-samples.csv`: reproducible EU save calculations.
- `tools/calibrate-environment-model.py`: calibration and scenario-rating generator.
- `tools/sample-environment-save-timings.py`: save sampler and timing model.
- European Commission Joint Research Centre, [EDGAR 2025 greenhouse-gas dataset](https://edgar.jrc.ec.europa.eu/dataset_ghg2025), country CO2, CH4, and N2O inventories through 2024.
- Global Carbon Project, [Global Carbon Budget 2025](https://essd.copernicus.org/articles/18/3211/2026/index.html), used to contextualize the 2024/2025 proxy for the 2026 scenario.
- Local Terra Invicta 1.0.51 templates, guarded patch points, safe atmospheric references, and priority behavior.
