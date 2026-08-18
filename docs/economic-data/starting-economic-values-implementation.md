# Starting economic values implementation

Status: implemented and deployed on 2026-08-17 for the 2003, 2022, and 2026
scenario overrides. New campaigns are required to instantiate the changed
starting nation and region templates.

## Source of truth

The implemented values come from
`country-economic-clamp-proposal-2022-usd.csv`. That reviewed table applies the
approved rule: retain vanilla GDP per capita when it lies between the
scenario-scaled nominal and PPP estimates, otherwise clamp it to the nearer of
those two estimates. Population uses the geography-audited UN WPP basis recorded
in `2022-usd-normalization-research.md`.

## JSON transformation

For every country-year row:

1. Write `proposed_json_initial_gdp` to the matching scenario nation template's
   `initialGDP` field.
2. Preserve the scenario's existing `globalStartingGDPScaling`. No runtime GDP
   modifier is added.
3. Set the sum of the country's owned region populations to
   `proposed_population` by proportionally scaling the regions' vanilla
   `population_Millions` values.
4. Correct the final region for decimal-rounding drift so the authored regional
   sum equals the proposed country total to the precision stored in JSON.
5. Preserve each country's vanilla internal regional population shares; no
   city- or province-level redistribution is inferred from national data.

The generated partial overrides are `TINationTemplate.json` and
`TIRegionTemplate.json`. Scenario-specific data names keep the 2003 and 2026
starts independent from the modern 2022 templates.

## Safeguards

`tools/sync-starting-economic-values.ps1` reproducibly generates the two JSON
files from the reviewed CSV and the installed vanilla/Dark Skies templates.
`tools/validate-starting-economic-overrides.ps1` verifies during deployment
that:

- all 518 proposal rows map to exactly one scenario nation;
- every proposed GDP is present and equal to the JSON override;
- all owned regions needed by a proposal are present exactly once;
- each overridden regional population is positive; and
- regional sums reproduce all 518 proposed populations within one person.

The 2003 Somaliland nation has no positive starting GDP and therefore has no
proposal row. Its vanilla regional population remains unchanged; the 2003
Somalia/Somaliland combined total already matches the audited territorial total.

## Verification and deployment

The normal `tools/deploy.ps1` flow completed successfully against Terra Invicta
1.0.51. It rebuilt the DLL and package, ran 1,070 formula assertions and every
existing IL/data validator, regenerated the economic overrides in an isolated
temporary directory, compared them byte-for-byte with the authored JSON, and
deployed the resulting 44-file package to the enabled-mod directory.

The deployment script skips copying a destination file only when its SHA-256
hash already equals the source. The final package-wide hash verification remains
mandatory, so an unchanged locked localization file cannot conceal a stale
deployment.
