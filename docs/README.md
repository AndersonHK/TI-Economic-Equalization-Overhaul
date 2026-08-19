# Documentation map

This directory separates current implementation authority from historical
research and deferred work. Unless a document says otherwise, the source code,
default settings, automated verification, and current implementation matrix win
when prose and runtime behavior disagree.

## Current authorities

- [Project README](../README.md): release target, implemented scope, build,
  deployment, and smoke-test entry point.
- [Design directives](design-directives.md): durable economic and patch-design
  rules for future changes.
- [Manufacturing logistics](manufacturing-logistics.md): authoritative Version
  0.9 behavior for hab construction, founding, probes, routing, caching, AI
  planning, and player-facing explanations.
- [Mine Mission Control](mine-mission-control.md): authoritative tier-based mine
  MC formula, retired free-mine bonuses, and UI/AI integration.
- [Global technology AI selection](global-technology-ai-selection.md):
  authoritative soft-priority, relative-cost, and weighted-selection formula
  for AI replacement of completed global technologies.
- [Fusion technology tree rebalance](fusion-technology-tree-rebalance.md):
  authoritative D-T Fusion, Nuclear Fusion Methodologies, and D-D Fusion
  prerequisite graph, localization, costs, and manual tree-layout target.
- [Current implementation matrix](current-implementation-matrix.xlsx):
  patch-by-patch comparison with Terra Invicta 1.0.51 and the maintained-main
  baseline.
- [Patch sanity audit](patch-sanity-audit.md): scale, unit, and compatibility
  review of the implemented Harmony patches.
- [Cohesion, Inequality, and Government coefficient report](national-social-coefficients-report.md):
  comprehensive formula, coefficient, retained-vanilla, priority-speed, event,
  and cross-effect inventory for tuning the three national social scores.
- [Coup frequency and Inequality stabilization](coup-inequality-stabilization-report.md):
  zero-Cohesion organic-coup-loop analysis, implemented `-0.10` Inequality
  change, immediate Cohesion-equilibrium reset, and manual-test plan.
- [Land warfare and Military investment](land-warfare-and-military-investment.md):
  authoritative formulas for army value, upkeep, modernization, repair debt,
  combat ratings, and force-preserving transfers.
- [Economy growth calibration](economy-growth-calibration-plan.md) and
  [simulator guide](economy-growth-simulator.md): implemented factor-balance
  model, calibration envelope, and reproducibility rules.
- [Starting-force rebalance](starting-force-rebalance-2022-2026.md): implemented
  2022 and 2026 army/navy inventories and their source methodology.
- [Starting technology and project audit](starting-technology-2022-2026.md):
  authoritative 2022/2026 active research, completed technologies, starting
  projects, dependency audit, shared Space Tourism, Deep Space Propulsion, and
  Augmented Reality decisions, the Augmented Reality Military-cap effect, and
  the 2026 Skywatch and Outpost Habs progression decisions.
- [2003 starting technology audit](starting-technology-2003.md): Dark Skies
  opening technologies, completed projects, alien pacing, and current Economic
  Equalization compatibility limits.
- [Country economic normalization research](economic-data/2022-usd-normalization-research.md):
  auditable 2003/2022/2026 vanilla, nominal, and PPP GDP-per-capita comparisons,
  with populations, effective starting-GDP scaling, source coverage, the
  scenario-scaled plausibility-clamp proposal, and its gap-fill audit.
- [Starting economic values implementation](economic-data/starting-economic-values-implementation.md):
  authoritative JSON transformation, geography-audited population distribution,
  validation, and deployment record for the implemented 2003/2022/2026 nation
  GDP and regional population values.
- [Alien start, wormhole, and Earth-invasion audit](alien-start-wormhole-and-earth-invasion.md):
  scenario-by-scenario alien day-one assets, campaign-setting effects,
  wormhole income formulas, invasion timing, and assault-carrier landing AI.
- [Earth orbits, launch costs, and lunar resource semantics](orbits-and-lunar-resources/approved-design.md):
  authoritative inclination-aware Earth launch costs, four additional LEO
  bands, ISS/Tiangong migration, corrected resource semantics, and the
  implemented thirty-five-site Luna roster. Its companion
  [Luna yield comparison](orbits-and-lunar-resources/resource-yield-comparison.md)
  records the approved mass-grounded bands and vanilla Luna/Mars comparisons.

## Historical, exploratory, and deferred material

- [National harmonization eligibility plan](national-harmonization-eligibility-plan.md)
  traces the current dynamic claim-hostility and peaceful-unification gates,
  evaluates the proposed score, and records the not-yet-implemented patch and
  test plan.
- [Initial Economy plan](economy-growth-initial-plan.md) is a superseded design
  record. The calibration document and source describe the implemented model.
- [Hab capacity beyond twenty facilities](hab-slot-expansion-assessment.md) is a
  feasibility assessment, not implemented scope. The implemented T1 station
  sector visibility is recorded in the matrix and
  [station slot map](hab-station-slot-map.md).
- [Nuclear Winter modification](nuclear-winter-deferred.md) is explicitly
  deferred.
- [Military 2022-2030 sanity check](military-investment-2022-2030-sanity-check.md)
  is a controlled calibration exercise, not a historical forecast.
- [Ship balance research](ship-balance-research/README.md) contains evidence,
  planning decisions, and a dated implementation log. Each document should be
  read according to its status language; research proposals are not current
  gameplay merely because they remain useful design evidence. Its generated
  [hull graphical-variant and utility-slot report](ship-balance-research/hull-utility-slot-volume-report.md)
  inventories all installed human and alien appearances and separates the main
  hull art envelope from named reactor/radiator and engine geometry.

## Maintenance rule

When behavior changes, update the code, localization, tests, implementation
matrix, and the relevant current authority in the same change. Do not rewrite
historical research to pretend a later decision was always present; add a clear
status note or link to the superseding authority instead.
