# TI Economic Equalization Overhaul

This branch targets Terra Invicta 1.0.39. It replaces opaque, border-sensitive
priority math with configurable formulas whose economic unit is visible in each
patch. It also compiles against the currently installed TI 1.0.47 assemblies as
a forward-compatibility check.

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

- Smooth total-GDP Economy growth with compounded technology weights and
  GDP-relative resource / density-relative land abundance.
- GDP-normalized Economy, Welfare, and Spoils Inequality effects with smooth
  bounded behavior on TI's 1-9 scale.
- GDP-only economy emissions; fixed atmospheric removal per completed IP;
  GDP-relative sustainability transition; land-relative nuclear damage.
- Population-normalized Unity, Knowledge, Government, Oppression, and selected
  Spoils social effects.
- Army-count-normalized Military technology and per-army upkeep.
- National mergers combine Military technology from 50% force structure and
  50% GDP, while merged Inequality approximates the combined income distribution.
- Surgical Unity and Spoils propaganda transpilers that preserve TI 1.0.39's
  complete priority-completion behavior.
- Configurable region conversion, decolonization, and fallout thresholds,
  defaulting to 5x vanilla, with gameplay and tooltip IL guarded together.
- Expanded tooltips that append live mod calculations to vanilla text.

The authoritative feature comparison is
[docs/current-implementation-matrix.xlsx](docs/current-implementation-matrix.xlsx).

## Configuration

Unity Mod Manager exposes grouped settings and a reset-to-default button.
Technology weights are tracked in
[TIEconomyMod/ModFiles/Config/economy-tech-weights.csv](TIEconomyMod/ModFiles/Config/economy-tech-weights.csv).
Unknown technology IDs are logged and skipped, duplicate IDs fail validation,
and changes require restart.

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
tests, validates the implementation matrix against settings and Harmony patches,
checks the manifest/package layout, and creates
`artifacts/TIEconomyMod-0.5.0-ti1.0.39.zip`.

## Smoke test

Compare poor resource-rich/resource-poor, land-abundant/dense, stable/unstable,
early/late technology, small/large GDP, and small/large army-count countries.
Exercise all affected priorities at boundary and neutral values, then toggle each
feature and confirm the appended tooltip agrees with the observed change. Merge
countries with similar and dissimilar GDP/c, force structure, and population;
verify Military technology and Inequality against the documented examples.
