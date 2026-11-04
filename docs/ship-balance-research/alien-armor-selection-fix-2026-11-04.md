# Alien armor selection without exotic exclusion — 0.11.2

Status: **implemented and deployed in 0.11.2; manual campaign reload pending.**

Fix the [confirmed save-load crash](save-load-crash-armor-selection-2026-11-04.md)
at the common native `TIFactionState.GetBestArmor(bool)` selection point, covering
habitat scoring and alien ship-design callers. Restrict the change to actual
alien factions; human factions retain the complete native selection function,
including exotic-resource restrictions.

For aliens, treat the false argument as a request for a cheaper armor material:
compare the native construction bill for the same one-tonne build mass, valued
using the configured initial resource values. All seven space materials count,
including exotics and antimatter; no armor name, tier or recipe is hardcoded.
Apply the native obsolete/current preference within the cheapest cost tier,
then break ties by mass per armor point and ordinal identity. If that tier is
entirely obsolete, retain it as the native filtered-pool fallback does.

For true, retain the native premium objective: lowest mass per armor point
among currently preferred unlocked armors, with material price as a tie-breaker.
If all unlocked armors are obsolete, retain the native full-pool fallback.
With the current catalog, false selects Alien Adamantane and true Alien Exotic.
Material recipes, costs actually charged, physical properties, research gates,
human behavior, and native save-version metadata remain unchanged.

The material-price comparison is deliberately per equal build mass, matching
the native one-tonne cost probe. It is not a claim that the heavier material is
always cheaper for an equal-protection or whole-ship armor design. Configured
reference values work before campaign market state exists and are appropriate
for an alien faction with its own exotic supply.

Validate the installed native function before/after patching with current and
vanilla alien catalogs; unchanged human results; renamed/synthetic materials,
changed resource values, unlock exclusion and native obsolescence fallback.
Check that the fatal habitat call site still reaches this common selector.
Run normal guarded deploy.ps1 immediately once implementation is ready and
announce manual save-load readiness after copying completes. Reuse the current
untested 0.11.2 version as previously authorized. No save migration is required.

## Deployed verification

The real installed `GetBestArmor(false)` returns null with the current alien
catalog before the Harmony patch. After patching the same method, it returns
Alien Adamantane; true returns Alien Exotic. Native human catalog results are
identical before/after. A human fixture with only exotic-bearing records still
returns null for false, confirming its restriction was not bypassed.

Renamed records select by the same prices/performance. A synthetic exotic-only
material enters the budget choice when its configured resource value becomes
cheaper than a metal-only alternative. Registered but unavailable armor stays
unselected. Budget/premium obsolescence and the all-obsolete fallback pass.
The fatal habitat call site's reference to this common selector is verified.
No Unity scene, rendering or sound is produced by these regression checks.

The fixture normalizes the common native resource conversion factor to 1;
the current material scores are 22.15 / 28.90 per equal build mass. Native
damage-point masses are 0.25 / 0.175 kg for a 20 MJ damage point, equivalent to
50 / 35 kg per m² armor point at the native 0.005 m² plate area. Material cost
per equal mass and cost per equal protection are separate measures.

Normal guarded deployment passed **44 validators, 1,376 formula assertions**,
and the **122-feature / 263-patch** matrix. All **55 installed and archived
files** match. The original armor data and material costs are unchanged.
[Deployment receipt](alien-armor-selection-deployment-2026-11-04.json).

**Manual acceptance remains pending:** restart and retry
`Appeasesave00240_2051-12-31.gz`; check completion of habitat initialization,
then alien budget/premium armor choices and human exotic restrictions.
The native save version and existing mod metadata are unchanged. No commit
was created.

DLL SHA256: `14C9AEA128D912F4554DB229742E8BEDB9E859C708B5BF9110B1F446FDC991C3`.
