# TI Economic Equalization Overhaul

Current release: **0.9.2**, targeting **Terra Invicta 1.0.51**.

The mod replaces opaque, border-sensitive scaling with economic units that
remain understandable across countries, armies, habs, and spacecraft. It aims
to preserve the structure of Terra Invicta while making scale, physical cost,
and technological progression matter consistently.

## Design direction

Investment Points are linear in GDP: each $100B produces 1 monthly IP before
the low-income modifier. Each completion then follows the stock it changes:

- fixed assets and physical work have a fixed effect per IP;
- economic ratios divide by GDP;
- demographic ratios divide by population;
- force-wide effects price the affected force;
- space construction pays for material mass and physical freight.

This removes incentives to split or merge countries merely to multiply
per-capita effects. Large nations remain intentionally better at fixed-cost
projects such as Mission Control, Boost, and Armies. Resources are evaluated
relative to GDP, so the same physical endowment matters more to a smaller,
less-diversified economy.

The durable rules are in [the design directives](docs/design-directives.md).
The [documentation map](docs/README.md) distinguishes current authorities from
historical research and deferred work.

## Implemented scope

### Economy, society, and environment

- Factor-balance Economy growth: capital returns are constrained by effective
  labor and resources, while all 149 global technologies compound productivity
  and progressively substitute for those constraints.
- GDP-normalized Economy, Welfare, and Spoils Inequality effects at twice
  baseline strength, with smooth bounded behavior on TI's 1-9 scale; climate-
  tagged Inequality changes are also doubled.
- GDP-only Economy emissions; no direct atmospheric removal from Environment
  IP; GDP-relative sustainability transition; 0.90 warm-climate GDP damage; no
  direct Spoils gas pulse; land-relative nuclear damage.
- Nuclear strikes retain local destruction but do not apply an instantaneous
  GDP penalty to every human nation. The proposed Nuclear Winter trigger remains
  [explicitly deferred](docs/nuclear-winter-deferred.md).
- Economy and Spoils share the same live factor-balance GDP gain. Spoils keeps
  its full $60 base faction-cash payout.
- Population-normalized Unity, Knowledge, Government, Oppression, and selected
  Spoils social effects.
- Government investment uses a doubled demographic base and every Government
  change follows a smooth reciprocal boundary curve: positive changes range
  from x3 at Government 0 to x1/3 at 10, while negative changes range from
  x1/3 to x3. Passive low-Cohesion Government loss is halved before the curve.
- Configurable region conversion, decolonization, and fallout thresholds,
  defaulting to five times vanilla.

### Control, research, and councilors

- Country Control Point usage is multiplied by 1.20 before the active scenario
  modifier. The five management technologies independently reduce the exponent
  by 0.02/0.03/0.05/0.05/0.05.
- Project Control Point capacity values become additive percentages at their
  existing 5/10/20/40/120 values and multiply the complete non-project flat
  capacity base.
- Councilor base attributes retain the vanilla 25-point cap; traits and
  organizations may raise modified totals to 50. Councilors may equip up to 18
  organizations and use the full modified Administration total.
- National research from unowned, cracked-down, or otherwise unusable Control
  Point shares is conserved as neutral research divided evenly among the three
  global technology slots. It is displayed in gray and cannot finish a
  technology without faction contribution.
- Monthly national research uses a 0.0038 coefficient and the continuous
  per-capita-GDP factor `(perCapitaGDP + 12000) / 20000`, followed by the
  configured Education, Government, Cohesion, Unrest, and adviser factors.
- Global technologies cost 2.00 times research. Mission to Space, Skywatch, and
  We Are Not Alone receive an additional 2.00 multiplier. Faction projects cost
  1.40 times their vanilla research cost.
- AI global-technology choices keep every available candidate eligible. Native
  priority tiers become `1/2/4/10/14` weight multipliers, combined with a
  bounded `(median available cost / candidate cost)^0.75` cost factor.

### Military and starting scenarios

- Continuous Military investment combines a smooth doctrine cost with the exact
  equipment upgrade cost of every eligible army.
- Army construction costs `2 × 2^miltech`; upkeep is `miltech / 10` at home and
  `miltech / 3` away. Repair creates persistent Build Army debt equal to half
  the value actually restored; positive Build Army, Military, Build Navy, and
  Nuclear Weapons IP repay it first, with any overshoot continuing into the
  selected priority. While debt remains, those priorities share one spanning
  repair-debt display in the investment column.
- Land combat uses up to a -1 additive strength penalty, half-strength situational
  modifiers, and a symmetric base-2 hit curve: 25%/50%/75% at rating differences
  -1/0/+1.
- Peaceful unification and transfer into the Alien Nation preserve eligible
  human armies and conserve doctrine/equipment value. Human conquest destroys
  conquered armies.
- Xenofauna is capped at 5 base Military technology; Purge and Enthrall Elites
  receive +1 defense. Damage to an army clearing alien flora scales linearly
  with the infestation level, reaching the vanilla amount at level 100.
- The 2022 and 2026 scenarios begin with Mission to Space, Advanced Chemical
  Rocketry, Space Tourism, Deep Space Propulsion Concepts, and Augmented Reality
  completed. The 2022 lineup keeps Outpost Habs active; the 2026 lineup completes
  it and continues that research lane with Mission to the Moon. Both starts also
  include the implemented historically rescaled army/navy inventories.
  Augmented Reality raises the maximum human Military technology level by 0.5,
  representing networked sensors, digital communications, augmented displays,
  and unmanned warfare without directly upgrading any nation's current level.
  Its authored research cost is 2,000, or 4,000 after EEO's global multiplier,
  keeping it below Space Research at 5,000.

### Habs and manufacturing logistics

- Active mines consume Mission Control equal to their tier: 1/2/3 MC for
  T1/T2/T3. The free-mine allowance and quadratic mine-network surcharge are
  removed.
- Human hab modules use approximately 1.5 times physical mass, rounded to the
  nearest five tonnes, and consume resources for that complete modified mass.
- The full-Earth option buys all materials with Money and Boosts the complete
  payload.
- The space option reserves stockpile materials, purchases and Boosts shortages
  from Earth, and transports at least one-third of construction mass. Earth
  shortages count toward that minimum.
- T1/T2/T3 factories manufacture through their tier. A factory serves its exact
  hab without a dock; remote export requires an active dock or shipyard on the
  same owned hab and is capped by the lower facility tier.
- Routes are system-agnostic and include surface launch, transfer, and landing
  delta-v. Non-Earth freight consumes Water/Volatiles propellant using the probe
  rocket equation; Earth remains the fallback.
- Hab founding and probes share the same origin rules. Probes are full-payload
  T1 jobs and require a T1 factory-dock pair for space launch.
- Route and cost results are cached separately and refreshed lazily. Resource
  changes do not rescan origins; warm tooltip and planner calls are average O(1)
  with respect to origin count.
- AI factions prioritize completing one same-hab factory-dock pair in each major
  colonized system, with the strongest priority in Earth-Moon.
- Human T1 stations expose visible sectors 1-3 and twelve facility slots. T1
  hab-list icons remain free of peripheral sector overlays; T2/T3 retain their
  vanilla composites. Starting ISS/Tiangong layouts, T1 crews, and consumables
  are rescaled.

### Earth launch orbits and Luna

- Four 500 km Earth interface orbits add base inclinations of +20, +40, -20,
  and -40 degrees while retaining Low Earth Orbit 1's authored variation.
  ISS, skirmish ISS, and Tiangong begin in the +40-degree orbit; their bespoke
  instantiated orbits are retired.
- Earth launch Boost cost evaluates every operational launch site and chooses
  the lowest-delta-v ascent for the destination altitude and inclination.
  Destinations beyond LEO also choose the cheapest instantiated Earth parking
  orbit instead of depending on template order.
- Launch cost uses a two-impulse ascent, Earth's runtime rotation and gravity,
  and the existing faction exhaust-velocity conversion. Boost-production
  latitude bonuses remain separate from destination launch cost.
- Luna has thirty-five named, geographically distributed sites with bounded,
  site-specific mining profiles. Water is confined to supported polar cold
  traps, Volatiles to cold traps and supported pyroclastic deposits, and trace
  Fissiles to KREEP or thorium-related provinces.
- Water localization now identifies both hydrogen and oxygen from water;
  Volatiles no longer categorizes bulk mineral oxygen as a volatile resource.

The design, formulas, geology rationale, and vanilla Luna/Mars comparison are
in [Earth orbits, launch costs, and lunar resources](docs/orbits-and-lunar-resources/approved-design.md).

The complete logistics rules and examples are in
[Manufacturing Logistics](docs/manufacturing-logistics.md).

### Ships and weapons

- Conventional guns may draw template-authored electrical power; reactor output,
  module heat, radiator sizing, and firing heat gates use one coherent thermal
  accounting chain.
- Projectile colliders follow caliber and damaging mass, magnetic durability
  follows damaging mass, and direct-fire AI avoids oversaturating targets while
  preserving deliberate player targeting and missile behavior. All ship and hab
  weapons run their native acquisition and fire checks on a 50 ms combat-time
  grid, so authored intra-salvo intervals in 50 ms increments are respected.
- The skirmish roster reuses cached option catalogs, and gun lookup uses
  allocation-free template identity on ordinary paths.
- The settled fuel-cell, fission-plant, crew, hull, gun, rail, coil, and alien
  magnetic slices are documented in the
  [ship balance research log](docs/ship-balance-research/CHANGELOG.md). Items
  explicitly marked proposed or deferred there are not current gameplay.

The authoritative patch-by-patch comparison is the
[current implementation matrix](docs/current-implementation-matrix.xlsx), and
the [patch sanity audit](docs/patch-sanity-audit.md) reviews scale and
compatibility assumptions.

## Configuration

Unity Mod Manager exposes grouped settings and a reset-to-default button.
Technology weights are stored in
[economy-tech-weights.csv](TIEconomyMod/ModFiles/Config/economy-tech-weights.csv).
Every TI 1.0.51 global technology has productivity, labor-substitution, and
resource-substitution weights. Unknown IDs are logged and skipped; duplicate
IDs, invalid values, or zero future-axis totals fail validation. CSV changes
require a restart.

The packaged [default settings](TIEconomyMod/ModFiles/Settings.xml) are copied
during release deployment so a new balance version begins from its authored
defaults.

## Compatibility and save behavior

Version 0.9.2 is built and guarded against the installed Terra Invicta 1.0.51
assemblies. Transpilers validate their expected IL shapes and fail verification
when the target changes.

The manufacturing source registry, cache generations, routes, and quotes are
runtime-derived. They add no serialized state and rebuild lazily after loading,
so existing saves remain compatible. Other affected systems likewise avoid new
save fields unless a document explicitly says otherwise.

The new Earth orbit-state roster, starting-station placement, and thirty-five-site
Luna map are new-campaign features. Existing campaigns retain the orbit and site
states serialized when they were created.

The current release archive is:

```text
artifacts/TIEconomyMod-0.9.2-ti1.0.51.zip
```

## Build and verification

The project targets .NET Framework 4.8. Set `TI_TARGET_MANAGED_DIR`, or let the
build script locate Steam. References come from one selected installation so
Harmony and Unity Mod Manager are not mixed across versions.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\build.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\verify.ps1
```

Verification rebuilds with warnings as errors and checks:

- dependency-free formula assertions;
- every guarded TI 1.0.51 IL patch point and dynamic Harmony application;
- all 110 hab-module overrides and logistics localization;
- construction, founding, probe, AI-priority, lazy-cache, and compact cost-label patches;
- the implementation matrix against settings and patch references;
- 2022/2026 starting forces and navy floors;
- package layout, version metadata, and release archive contents.
- Earth launch formula calibration and minimum-cost site/parking selection;
- the four inclination bands, starting-station migration, thirty-five lunar sites,
  site-specific resource bounds, 200 km minimum separation, and corrected
  resource localization.

## Deployment

In this repository, deploy means: run release verification, then mirror
`TIEconomyMod/ModFiles` into `Mods/Enabled/Economic Equalization Overhaul`
relative to the detected Terra Invicta install root.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\deploy.ps1
```

The script locates the game through Steam configuration or
`TI_GAME_INSTALL_DIR` / `TI_TARGET_MANAGED_DIR`. An explicit install root may be
supplied with `-GameInstallDir`. It removes stale files only inside the exact
enabled-mod destination and verifies every deployed file by SHA-256.

## Smoke-test checklist

### Economy and national systems

- Compare poor/resource-rich, poor/resource-poor, land-abundant/dense,
  stable/unstable, early/late technology, and small/large economies.
- Exercise affected priorities at boundary and neutral values; compare each
  appended calculation with the observed result.
- Merge similar and dissimilar countries and verify Military technology,
  surviving armies, and Inequality against the authoritative formulas.
- At Military technology 4/cap 5, confirm doctrine from 4 to 5 costs about
  2,883.59 IP plus 32 IP per army; Build Army costs 32 IP and healing creates
  debt repaid first by later Build Army, Military, Build Navy, or Nuclear Weapons
  investment.
- Confirm -1/0/+1 land-combat rating differences show 25%/50%/75% hit odds.
- Verify neutral research equals the sum of unusable national Control Point
  shares, appears gray in all three slots, and waits below completion until a
  faction contributes.

### Habs, founding, and probes

- Compare a full-Earth quote with the mixed space quote for the same module;
  verify full material mass is charged exactly once.
- For a 30-ton substitutable payload with 30/25/15/0 tonnes available, verify
  Earth purchases are 0/5/15/30 tonnes and additional factory dispatch is
  10/5/0/0 tonnes.
- Confirm an undocked factory serves only its exact hab. Add a dock on the same
  hab and verify remote construction works across Earth-Moon, Mars, and another
  planetary system, subject to the lower facility tier.
- Depower, destroy, decommission, or transfer the source and verify it is no
  longer eligible. Allied or merely non-hostile foreign facilities must never
  qualify.
- Exercise orbit-to-orbit, surface-to-orbit, orbit-to-surface, and surface-to-
  surface routes. Same-hab freight should consume zero propellant; other routes
  should include the appropriate launch and landing delta-v.
- Found each supported hab tier through Earth fallback, a valid remote pair, and
  a ship kit while confirming technology, Mission Control, survey, site, and
  capacity restrictions remain active.
- Launch single and multiple probes from Earth and from a T1 factory-dock pair;
  the entire space-built probe payload must travel from its origin.
- Repeat planner and cost requests before and after resource spending, time
  advancement, and hab changes; stale results should refresh only when requested.

### Earth launch costs and Luna

- Start a new campaign and inspect Earth interface orbits. Confirm the four new
  +/-20- and +/-40-degree bands exist, Low Earth Orbit 3/4 do not instantiate,
  and ISS/Tiangong use the +40-degree band without bespoke station orbits.
- Compare previewed and charged Boost at 0, +/-20, and +/-40 degrees from
  different launch-site distributions. Equal positive/negative inclination
  magnitudes should cost the same; a poleward dogleg should be expensive.
- Compare a destination beyond LEO and verify the selected launch path is not
  tied to Low Earth Orbit 1. Exercise construction and crew resupply so their
  costs agree with the shared authority.
- Inspect all thirty-five Luna markers and prospect across several campaign seeds.
  Every resource must remain inside its approved bounded band; dry sites must
  never gain Water or Volatiles. Check small Fissiles values display, accumulate,
  and affect AI mine valuation sensibly.

### Compatibility and UI

- Load an older save and verify caches rebuild without save errors.
- Confirm T1 human stations expose sectors 1-3 while bases and alien stations do
  not, and T1/T2/T3 hab-list overlays retain their intended tier behavior.
- Review module, project, operation, and Codex text for the concise rule: local
  factory; same-hab dock for export; lower tier caps; any-system route; less
  Boost; Earth fallback. Earth and space purchase buttons should retain their
  native compact costs and duration without appended route diagnostics.
