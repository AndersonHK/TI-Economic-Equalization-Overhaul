# Capital turret and nose expansion — 2026-11-02

## Plan

Extend compact heavy hull mounts to human and alien Dreadnoughts. On those
hulls, both Titans and the Alien Mothership, require FourNose weapons with one
weapon per existing nose cell. Keep weapon statistics and full equipment costs.
Use the ordinary model's individual nose anchors for independent weapons.

Investigate four additional Mothership nose cells and symmetric model anchors.
The ordinary designer has 10 columns and seven half-height rows. Its 38 existing
controls already approach the maximum 40 nonoverlapping full-height icons;
42 controls cannot fit without overlap or a bespoke grid extension. The user
approved two additional cells instead: six nose cells and 40 controls total.
Repack only nose/nose-armor coordinates, append indices 38/39, and retain all
utility adjacency and serialized slot indices/types. Clone two complete native
nose base/barrel/fire-point hierarchies onto symmetric outer head surfaces at
`(-80,0,255.6295)` and `(80,0,255.6295)` model coordinates. Repeated model builds
must reuse these anchors. [Measured surface probes](capital-nose-geometry-2026-11-02.json)
record the native head geometry; Unity rendering and aiming remain manual checks.

Migration schema 2 must accept vanilla/schema-0 and previous schema-1 saves.
Expand each legacy footprint into one appropriate heavy weapon per occupied
cell; retain schema-1 compact hull entries without expanding them a second time.
Convert nose equipment in designs and built ships, including ammunition,
damage, repairs, reload plans and fire modes. Leave previously empty cells empty.
Prepare all plans and archive the original before committing campaign changes.

Add one human crew billet per added effective weapon cell (a compact mount is
four effective cells). Calculate incremental structural reinforcement separately
from the full weapon mass already charged by equipment. Update all four human
appearance masses, data assertions and migration/AI coverage. Deploy with
`tools/deploy.ps1`, announce readiness, and record automated and manual limits.

## Implementation and validation

Version 0.10.22 implements independent FourHull/FourNose footprints, catalog
filtering, assignment/drop validation, core-slot selection, full-design guards,
model anchor selection and AI/player auto-design on all five hulls. Weapon
templates retain their original statistics, mass, cost, heat, power and magazines.
Occupied weapon volume still charges the full internal size of each weapon.
No size-4 weapon research is granted; if no eligible weapon exists, new designs
leave that weapon bank empty. Legacy migration retains its explicit equipment
exception and original backup/save-blocker behavior.

Schema 2 expands nose weapons using the previous geometry, including the old
Mothership nose coordinates. Schema-1 Titan/Mothership hull entries are already
compact and are preserved cell-for-cell; schema-1 dreadnoughts still need hull
expansion. Previously empty cells, including the two appended Mothership cells,
remain empty during migration. New AI designs fill all eligible independent cells.

Incremental capacity is measured from the previous mod release, not vanilla's
obsolete human crew values. Each compact size-4 cell adds three effective weapon
cells. Reinforcement is budgeted at 20 t per added effective cell, in addition
to weapon equipment mass; each added human billet contributes the existing 3 t
crew mass. Bare totals round to the nearest 100 t, then structural mass is
reconciled. This preserves the prior human appearance mass differences.

| Hull | Hull cells | Nose cells | Added effective cells | Base crew | Structural mass (t) | Bare mass (t) |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Human Dreadnought | 8 | 3 | 24 + 9 = 33 | 19 → 52 | 2,343 → 3,044 | 2,400 → 3,200 |
| Human Titan | 6 | 4 | 12 | 40 → 52 | 3,280 → 3,544 | 3,400 → 3,700 |
| Alien Dreadnought | 8 | 4 | 24 + 12 = 36 | 50 | 2,050 → 2,750 | 2,200 → 2,900 |
| Alien Titan | 8 | 6 | 18 | 60 | 2,420 → 2,820 | 2,600 → 3,000 |
| Alien Mothership | 16 | 6 | 24 − 4 = 20 | 100 | 8,200 → 8,600 | 8,500 → 8,900 |

Human structural masses for appearances 0–3 are Dreadnought
`3044 / 3644 / 3244 / 4244 t` and Titan `3544 / 4444 / 3644 / 5344 t`.

Normal deployment completed on 2026-11-02 against installed TI 1.0.53b:

- All 41 validators (30 runtime/binding jobs and 11 data/formula jobs), 1,226
  formula assertions, and the 120-row / 255-patch implementation matrix passed.
- Focused checks cover all five capital model maps; independent hull/nose cells;
  every legacy nose size; schema-1 Titan versus Dreadnought handling; both player
  and AI design modes including Protector roles; actual dropped-cell selection;
  and seven installed stock capital designs including Ship20/Ship20a.
- Stock Ship20 contains overlapping native hull footprints. Conversion preserves
  explicit anchors first and fills the remaining occupied union once. Duplicate
  explicit anchors and footprints outside the relevant bank still fail preflight.
- Built-ship preflight and commit tests preserve ammunition fractions, requested
  reloads, damage fractions and both current/prior repair plans across hull and
  nose conversion. Import/preview, repeated conversion and return-to-menu cache
  regressions continue to pass. Schema-1 serialization/reference markers remain
  readable; converted designs and ships carry schema 2.
- The Mothership's 40 controls fit the native grid without overlap; all previous
  utility footprints and serialized index/types are preserved. Measured new nose
  targets are symmetric. Native scene cloning, rendering, aiming and muzzle
  effects require Unity-host verification.
- `tools/deploy.ps1` checked that Terra Invicta was closed before validation and
  again before replacement; all 52 installed files matched the package.

DLL SHA-256: `D119EF7A23B491D15247FBDB707907B8689E267E0932BE279441B12FF41FA020`.
Release package: `artifacts/TIEconomyMod-0.10.22-ti1.0.53.zip`.

Manual acceptance remains pending: fit and remove weapons on all five classes,
including the last free nose cell; view all human appearances; verify both new
Mothership guns render, aim and fire from separate anchors; and load, save,
reload and refit an older campaign. Confirm the four previously occupied legacy
Mothership nose cells convert while the two new cells remain empty. Automated
fixtures establish conversion semantics, not complete campaign or combat parity.
