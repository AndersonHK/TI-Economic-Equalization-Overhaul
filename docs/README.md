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
- [Current implementation matrix](current-implementation-matrix.xlsx):
  patch-by-patch comparison with Terra Invicta 1.0.51 and the maintained-main
  baseline.
- [Patch sanity audit](patch-sanity-audit.md): scale, unit, and compatibility
  review of the implemented Harmony patches.
- [Land warfare and Military investment](land-warfare-and-military-investment.md):
  authoritative formulas for army value, upkeep, modernization, repair debt,
  combat ratings, and force-preserving transfers.
- [Economy growth calibration](economy-growth-calibration-plan.md) and
  [simulator guide](economy-growth-simulator.md): implemented factor-balance
  model, calibration envelope, and reproducibility rules.
- [Starting-force rebalance](starting-force-rebalance-2022-2026.md): implemented
  2022 and 2026 army/navy inventories and their source methodology.

## Historical, exploratory, and deferred material

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
  gameplay merely because they remain useful design evidence.

## Maintenance rule

When behavior changes, update the code, localization, tests, implementation
matrix, and the relevant current authority in the same change. Do not rewrite
historical research to pretend a later decision was always present; add a clear
status note or link to the superseding authority instead.
