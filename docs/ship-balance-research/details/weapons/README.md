# Weapon progression research

Last reviewed: 2026-08-01

The [gun and railgun progression report](gun-and-railgun-progression.md)
compares the 6-inch, 8-inch, and 10-inch conventional weapons with the railgun
families using equivalent mounts.

The [Mk1 railgun and gun-power follow-up](mk1-railgun-and-gun-power-followup.md)
tests the cadence-only human Mk1-Mk2 proposal against the settled chemical
projectiles and audits the code path required to make ordinary guns consume
ship power.

The [conventional-gun power patch plan](gun-power-consumption-patch-plan.md)
turns that audit into an implementation design with settled average loads,
40mm ETC accounting at 90% efficiency, load-order-safe generic JSON binding,
complete UI/save coverage, minimal Harmony patch boundaries, and regression
tests.

## Current synthesis

- Conventional caliber progression increases shell mass while retaining the
  same velocity and range.
- One 8-inch battery is lighter and lands larger hits than two 6-inch
  batteries, but two 6-inch mounts have slightly greater sustained impact
  output.
- Mk1 railguns mostly buy range and individual impact; Mk3 is where sustained
  output clearly exceeds the conventional counterpart.
- Railgun power draw and waste heat are important installed-system costs.
- The third planning slice settles only crew reductions: two crew for 6-inch,
  8-inch, Light Railgun Battery Mk1–3, and Railgun Battery Mk1–3; three crew
  for Light Rail Cannon Mk1–3. Performance changes remain under research.
