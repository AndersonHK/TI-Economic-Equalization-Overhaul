# TI Economic Equalization Overhaul

This branch targets Terra Invicta 1.0.49. It replaces opaque, border-sensitive
priority math with configurable formulas whose economic unit is visible in each
patch.

## Economic model

Investment Points are linear in GDP: each $100B produces 1 monthly IP before
the low-income modifier. Each completed IP then follows one stock rule:

- fixed assets and physical cleanup have a fixed effect per IP;
- changes to economic ratios divide by GDP;
- changes to demographic ratios divide by population;
- Military technology divides by the number of armies it upgrades.

This removes the vanilla incentive to split or merge countries solely to exploit
per-capita effects. Large nations remain intentionally better at buying fixed-cost
assets such as Mission Control, Boost, and Armies. Resource effects use resources
relative to GDP, so oil is economically more important to Saudi Arabia than to the
larger and more diversified United States.

See [docs/design-directives.md](docs/design-directives.md) for the rules future
patches must follow and [docs/patch-sanity-audit.md](docs/patch-sanity-audit.md)
for the current patch-by-patch review.

## Current scope

- Factor-balance Economy growth: capital returns are constrained by effective
  labor and resources, while all 149 global technologies compound productivity
  and progressively substitute for those constraints.
- GDP-normalized Economy, Welfare, and Spoils Inequality effects at x2 baseline
  strength, smooth bounded behavior on TI's 1-9 scale, and x2 climate impact.
- GDP-only economy emissions; fixed atmospheric removal per completed IP;
  GDP-relative sustainability transition; x0.90 warm-climate GDP damage; no
  direct Spoils gas pulse; land-relative nuclear damage.
- x1.05 Investment Point output; Economy and Spoils share the same live
  factor-balance GDP gain.
- Country Control Point usage is x1.20 before the active scenario multiplier,
  while the five management technologies retain the
  1.00/0.98/0.95/0.90/0.85/0.80 exponent progression.
- Project Control Point capacity values become additive percentages at their
  existing 5/10/20/40/120 values and multiply the complete non-project flat
  capacity base, including LEO and fixed scenario bonuses.
- Spoils retains its full $60 base faction-cash payout and has no direct gas pulse.
- Xenofauna capped at 5 base miltech; Purge and Enthrall Elites receive +1 defense.
- 2022 starts with Mission to Space and Advanced Chemical Rocketry completed.
- Human hab modules use approximately x1.5 physical mass, rounded to the nearest
  five tons, while retaining vanilla space-resource tonnage; all added mass must
  always arrive from Earth. Local-material substitution is resolved before that
  mandatory Boost is added, so an existing Boost shortage cannot be transfer-
  scaled a second time.
- Human T1 stations expose visible sectors 1-3/twelve facility slots and reuse the
  vanilla connector renderers between occupied arms while hab-list icon overlays
  remain tier-gated. The starting ISS and Tiangong layouts, T1 crews, and crew
  consumables are historically rescaled.
- Global technologies cost x1.20 research; faction projects retain vanilla costs.
- Population-normalized Unity, Knowledge, Government, Oppression, and selected
  Spoils social effects.
- Army-count-normalized Military technology and per-army upkeep.
- National mergers combine Military technology from 50% force structure and
  50% GDP, while merged Inequality approximates the combined income distribution.
- Surgical Unity and Spoils propaganda transpilers that preserve TI 1.0.49's
  complete priority-completion behavior.
- Configurable region conversion, decolonization, and fallout thresholds,
  defaulting to 5x vanilla, with gameplay and tooltip IL guarded together.
- Expanded tooltips that append live mod calculations to vanilla text.

The authoritative feature comparison is
[docs/current-implementation-matrix.xlsx](docs/current-implementation-matrix.xlsx).
The investigated path beyond the vanilla twenty-facility ceiling is documented
in [docs/hab-slot-expansion-assessment.md](docs/hab-slot-expansion-assessment.md).

## Configuration

Unity Mod Manager exposes grouped settings and a reset-to-default button.
Technology weights are tracked in
[TIEconomyMod/ModFiles/Config/economy-tech-weights.csv](TIEconomyMod/ModFiles/Config/economy-tech-weights.csv).
Every TI 1.0.49 global technology has productivity, labor-substitution, and
resource-substitution weights. Unknown IDs are logged and skipped, duplicate
IDs or zero future-axis totals fail validation, and changes require restart.
The packaged [default settings](TIEconomyMod/ModFiles/Settings.xml) are copied
on release deployment so a new balance version starts from its authored values.

## Build and verification

The project targets .NET Framework 4.8. Set `TI_TARGET_MANAGED_DIR`, or let the
build script locate Steam. References come from one selected installation so
Harmony and Unity Mod Manager are never mixed across versions.
On this machine that matched pair is Harmony 2.3.1.1 and Unity Mod Manager
0.27.14; the project does not copy either binary into the repository.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\build.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\verify.ps1
```

Verification rebuilds with warnings as errors, runs dependency-free formula
tests, validates all 110 hab-module overrides against the installed vanilla
templates, validates the implementation matrix against settings and Harmony
patches, verifies the guarded hab connector and list-icon transpilers, checks the
hab-cost substitution order, validates Control Point effects and localization,
checks the manifest/package layout, and creates
`artifacts/TIEconomyMod-0.7.1-ti1.0.49.zip`.

## Smoke test

Compare poor resource-rich/resource-poor, land-abundant/dense, stable/unstable,
early/late technology, small/large GDP, and small/large army-count countries.
Exercise all affected priorities at boundary and neutral values, then toggle each
feature and confirm the appended tooltip agrees with the observed change. Merge
countries with similar and dissimilar GDP/c, force structure, and population;
verify Military technology and Inequality against the documented examples.
For habs, compare Earth-built, locally supplied, and upgraded modules; confirm
the mandatory Boost floor and Earth delivery time. At a distant base, verify that
ample local materials plus insufficient Boost shows only the mandatory Boost
cost and never exceeds the all-Earth Boost cost. Then load an older save and
confirm human T1 stations expose visible sectors 1-3 while bases and alien
stations do not. Confirm T1 hab-list icons show no peripheral sector overlays,
while T2 and T3 icons retain their vanilla two- and four-overlay composites.
