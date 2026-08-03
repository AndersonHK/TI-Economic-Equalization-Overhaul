# Weapon progression research

Last reviewed: 2026-08-02

The [gun and railgun progression report](gun-and-railgun-progression.md)
compares the 6-inch, 8-inch, and 10-inch conventional weapons with the railgun
families using equivalent mounts.

The [Mk1 railgun and gun-power follow-up](mk1-railgun-and-gun-power-followup.md)
tests the revised cadence-only human Mk1-Mk3 proposal against the settled
chemical projectiles, evaluates the one-shot conventional-gun candidate, and
audits the code path required to make ordinary guns consume ship power.

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
- The staged light rail battery, rail battery, and light rail cannon use heavier
  projectiles and shorter single-shot reloads; Mk1 and Mk2 are useful mid-game
  weapons while Mk3 remains the high-output endpoint.
- Every human coil and alien magnetic projectile is approximately 25% heavier,
  with damaging mass and durability kept in step. Their total cycles are about
  20% shorter through inter-salvo reload only; vanilla salvo rhythm is retained.
- Alien magnetic weapons retain their separate muzzle-velocity increase, so
  their damage growth can exceed the human coil progression.
- Railgun power draw and waste heat are important installed-system costs.
- Two crew are used for 6-inch, 8-inch, Light Railgun Battery Mk1–3, and Railgun
  Battery Mk1–3; Light Rail Cannon Mk1–3 uses three crew.
