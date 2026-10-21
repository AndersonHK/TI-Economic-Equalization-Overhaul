# Validation and deployment record

The study adds offline measurement/report tooling and research artifacts. It does
not implement the hypothetical fitting rules, relocate mounts, or rebalance weapons.

## Completed automated checks

Eleven research tests passed, including full installed template/appearance
coverage, source SHA-256 checks, native map/slot consistency, recomputed minimum
distances, stock/current hull-grid equivalence, kinetic energy and salvo-cycle
arithmetic, padding monotonicity, exact mixed-size packing, capital baseline
accounting, Markdown links/tables, 2,400 randomized rotated-mesh containment
checks and 60 random extra-node graphs compared with exhaustive enumeration.
See [the retained test output](research-validation.txt).

The independent distance recomputation initially differed by approximately
2e-15 m because Python and NumPy accumulated floating-point sums differently.
The equality assertion was corrected to a 10-decimal-place comparison; the
measurements did not change. The subsequent complete run passed all 11 tests.

## Normal build and deployment

`tools/deploy.ps1` completed successfully without `-SkipVerification`. Both
programmatic game-closed gates passed. It ran 28 assembly/patch validators and
11 data/report validators, plus 1,204 formula assertions; the implementation
matrix check covered 106 rows and 210 Harmony patches. All 47 package files
were verified against the deployment by SHA-256.

Package: EEO 0.10.3, target Terra Invicta 1.0.53b.
DLL SHA-256: `AD6CA0B4A1FA04C85C774C18318CB86D10B83F284CB9CCAA68D155DDAF02CBBA`.
See [the retained deployment output](deployment.txt).

The offline tooling lives in the repository, outside the game package. The
existing gameplay package was rebuilt and deployed as required by the repository
workflow. No gameplay source or configuration changes are part of this study,
and the build has not been rebuilt since that successful deployment.

## Native-code evidence

[Retained native method excerpts](native-method-excerpts.md) document the
preview/combat scale branch, paired hull rendering and salvo cadence against
the hashed installed assembly. Full asset paths, transforms and native index
maps are retained in the measurement JSON files.

## Manual checks still outstanding

Manual in-game validation remains outstanding. The measurement results are static
asset/code evidence, not captured combat rendering. In particular, verify the
preview/combat scale difference, Titan alternate-art close pairs, and Mothership
ventral overlapping origins before using these findings to implement new fitting.
