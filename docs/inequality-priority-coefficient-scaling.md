# Inequality priority coefficient scaling

Status: implemented, validated, and deployed on 2026-08-20; manual in-game
testing pending.

## Decision

Multiply the current Economy, Welfare, and Spoils priority Inequality
coefficients by `1.5`. The sign of each coefficient is retained, so Economy
and Spoils increase Inequality 50% faster and Welfare reduces it 50% faster.

| Priority | Current coefficient | Calculation | Approved coefficient |
|---|---:|---:|---:|
| Economy | `+0.0015` | `0.0015 * 1.5` | `+0.00225` |
| Welfare | `-0.01333332` | `-0.01333332 * 1.5` | `-0.01999998` |
| Spoils | `+0.00666668` | `0.00666668 * 1.5` | `+0.01000002` |

These are the raw changes for a completed priority investment in a `$100B`
economy at neutral Inequality `5`, before Economy or Spoils resource modifiers.
The existing GDP normalization and boundary transform continue to apply.

| Priority | `$100B`, Inequality 5 | `$1T`, Inequality 5 |
|---|---:|---:|
| Economy | `+0.00225` | `+0.000225` |
| Welfare | `-0.01999998` | `-0.001999998` |
| Spoils | `+0.01000002` | `+0.001000002` |

At equal priority allocation, GDP-linear investment-point production offsets
the tenfold smaller per-completion change in the `$1T` example, as before.

## Explicitly unchanged

This is only a priority-coefficient calibration. It does not alter:

- `inequality.climateChangeMultiplier = 4.0` or any Environment/climate
  setting;
- `inequality.coupInequalityChange = -0.10`;
- the `1 / 5 / 9` Inequality bounds and neutral point;
- the exponent `2` or maximum directional multiplier `3`;
- the Economy maximum resource bonus `0.60`;
- the Spoils maximum resource bonus `1.00`;
- events, revolutions, secessions, annexations, or national mergers.

Climate-tagged Inequality changes therefore remain exactly quadrupled. They do
not use the three priority coefficients or the priority boundary curve.

## Verification and manual test

Automated formula tests must assert all three new `$100B`, Inequality `5`
values and retain the existing climate assertion (`0.02 -> 0.08`). The normal
deployment validation must pass before packaging.

For manual testing, compare Economy, Welfare, and Spoils tooltips or completed
investment effects in a nation near `$100B` GDP and Inequality `5`. Confirm the
new priority changes while a climate-tagged `0.02` Inequality input still
becomes `0.08`.

## Deployment record

`tools/deploy.ps1` completed successfully against the installed Terra Invicta
1.0.51 assemblies on 2026-08-20. Release verification passed, including `1,110`
formula assertions and the implementation-matrix validator. The climate test
continued to assert `0.02 -> 0.08`. The script deployed 45 files to the enabled
Economic Equalization Overhaul mod directory.

The packaged artifact is `artifacts/TIEconomyMod-0.9.3-ti1.0.51.zip`; the built
DLL SHA-256 is
`ACF6DD5DE58AD6309CEFB3E2F8B7CED7D124EDD2AE6C2C68E4035E3F5C497DE1`.
