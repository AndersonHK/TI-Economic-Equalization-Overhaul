# Asymmetric ship tiers, size-5 noses and wavelength precision — 0.11.0

Status: **0.11.0 deployed on 2026-11-04 for manual testing**. Its values below
are the 0.11.0 baseline. The [0.11.1 utility expansion](capital-utility-expansion-0.11.1-2026-11-04.md)
updates utility counts, crew/mass and armor layout; fitting tiers remain. Implements the
[revised asymmetric plan](asymmetric-human-alien-tier-plan-2026-11-04.md).

## Selected implementation rules

- Authoritative mod ship tiers 1–5 follow the species-specific table. Native
  construction tier remains independent. Tier controls fitting policy and is
  exposed in hull information; no cosmetic-only reclassification.
- Human Lancer and Alien Assault Carrier join compact hull fitting. Lancer gets
  4 independent N4 noses + 3 heavy hull mounts; Carrier gets 6 heavy hull mounts
  and no nose. Alien Lancer stays native/shared T3; Alien Titan stays compact N4 T4.
- Human Titan and Alien Mothership exclusively use N5 noses. The five explicit
  N5 identities are the three Spinal Siege Coilers, Alien Relativistic Particle
  Cannon and Alien1024cmGraserCannon. N5 is unavailable on all lower tiers.
  Native FourNose remains the geometry adapter; effective N5 volume is five units.
- Halve both salvo intervals (including fractional seconds) for the selected
  N5 templates. Per-shot physics, efficiency, mass, magazine and art stay fixed.
  Graser therefore reaches 1.333x xaser raw sustained output; this implements
  the requested exact doubling of its own baseline rather than an extra buff.
- Apply planned base crew/structural changes: Human Lancer crew 35 / structure
  2395 t, Human Titan crew 56 / structure 3632 t, Carrier structure 3060 t with
  alien crew unchanged, Mothership structure 8700 t with crew unchanged.
  Preserve the corresponding human appearance differences. Other hull costs stay.
- Store the two fractional graser wavelengths as exact integer 1 Å entries in
  a prepared mod precision catalog. Convert at physics/UI boundaries; keep native
  nanometre fields compatible. Clear affected spot/range/role caches at template
  initialization, not per frame. Both the 768 cm and 1024 cm grasers are covered.

## Migration policy

Advance the capital layout marker to schema 3. Preserve schema-2 compact entries
one-for-one; expand only originally shared Lancer/Carrier footprints. Also
normalize lower-tier native layouts containing equipment newly tagged N5, keeping
their original footprint rather than expanding the bank.

Use explicit paired standard/N5 variants first. For unsupported old N4 families
on tier-5 ships, choose a researched legal same-species alternative, otherwise
retain the established narrow migration-only equipment exception through a
baseline Human Siege Coiler I / Alien Relativistic Particle Cannon. No research
is granted or newly exposed in normal catalogs. Newly forbidden N5 equipment
on lower tiers converts to its explicit standard counterpart. No additional
resource-refund mechanism is introduced by this fitting migration; conversions
and original equipment are recorded in the backed-up source/report.

Prepare campaign changes before mutation and archive the original. Preserve
occupied cells, empty cells, supported fire modes, ammunition fractions, damage,
reload/repair plans and existing fuel. Keep current over-capacity preservation
for built designs; new/edit/refit designs obey current limits. Stock/main-menu,
import, queues and repeated load paths must be idempotent.

## Validation and delivery

Implement, then immediately run normal `tools/deploy.ps1` without skipped
verification. The game must be closed at both script assertions. Announce
readiness immediately after deployment and finish documentation while the user
tests. New checks must exercise grades in all hull classes, actual AI filters,
schema-0/1/2 conversion, new model maps, cadence/power/volume, native wavelength
getter/call-site binding, decimal display and cache reset.

Normal guarded deployment passed **42 validators, 1,372 formula assertions,
121 matrix feature rows and 260 Harmony patches**. All **53 installed files**
match the package byte-for-byte. Both game-closed assertions passed. See the
[deployment receipt](asymmetric-tiers-deployment-0.11.0-2026-11-04.json) and
[runtime/data snapshot](asymmetric-tiers-runtime-0.11.0-2026-11-04.json).

Native-assembly checks reproduce fractional nanometre deserialization, apply
all five new Harmony patches, verify both physical getters, ordinary/competing
wavelengths, decimal UI conversion and invalidation of three cached results.
The guarded spot transpiler verifies exactly one native conversion edge.
Migration fixtures cover schema 0/1/2, import, stock catalogs, AI filtering,
built ammunition/damage/repair/reload state, and repeated normalization. New
Lancer/Carrier controller targets and independent slot mapping resolve.

**Manual acceptance is pending.** Restart the game, open a prior campaign,
inspect the backed-up conversion report, check Lancer/Carrier placement and
refit, verify N5 exclusion on Alien Titan/other lower classes and N5-only noses
on Human Titan/Mothership, inspect both graser tooltips, and compare cadence in
combat. Native assembly/fixture checks do not establish live renderer behavior,
full campaign balance, or performance. No commit was created.

## Deployed categories and fitting

| Tier | Human classes | Alien classes |
| --- | --- | --- |
| 1 | Gunship, Escort, Corvette | Gunship, Escort, Corvette |
| 2 | Frigate, Monitor, Destroyer | Frigate, Monitor, Destroyer, Cruiser |
| 3 | Cruiser, Battlecruiser, Battleship | Battlecruiser, Battleship, Lancer |
| 4 | Lancer, Dreadnought | Dreadnought, Titan, Assault Carrier |
| 5 | Titan | Mothership |

STO Fighter/Salamander remain auxiliary/unranked. Hull descriptions display the
authoritative mod tier and compact fitting rules. Native `consTier` stays 1–3
for game AI construction semantics. Existing hull-project prerequisites,
availability rolls, project costs and MC are retained: the tier labels do not
grant the Human Lancer project to an existing faction.

| Changed hull | Noses | Heavy hull cells | Base crew | Structure / bare mass (t), V0 |
| --- | --- | ---: | ---: | ---: |
| Human Lancer | 4 independent N4 | 3 | 35 | 2,395 / 2,500 |
| Human Titan | 4 independent N5 | 6 | 56 | 3,632 / 3,800 |
| Alien Assault Carrier | None | 6 | 80 | 3,060 / 3,300 |
| Alien Mothership | 6 independent N5 | 16 | 100 | 8,700 / 9,000 |

Carrier retains troop/logistics data; the approved heavy-only conversion removes
small turret/PD/missile fitting choices. Human Lancer V0–3 bare masses are
2,500 / 3,000 / 4,400 / 4,400 t. Human Titan V0–3 are
3,800 / 4,700 / 3,900 / 5,600 t. Alien Lancer and Alien Titan retain 1,656 and
3,000 t bare masses and their existing native N4 / compact N4 fitting.

## Size-5 cadence and physical costs

| Template | Reload / intra-salvo (s) | Full cycle before → after (s) |
| --- | ---: | ---: |
| Spinal Siege Coiler I | 12 / 11 | 68 → 34 |
| Spinal Siege Coiler II | 9.5 / 7.5 | 49 → 24.5 |
| Spinal Siege Coiler III | 6 / 5.5 | 34 → 17 |
| Alien Relativistic Particle Cannon | 32 / 1.5 | 70 → 35 |
| Alien 1024 cm Graser Cannon | 18 / single shot | 36 → 18 |

Each retains its native model, mount HP/training semantics, per-shot damage,
velocity, efficiency, mass, magazine and resource recipe. Sustained damage,
average input/heat and ammunition expenditure therefore double. Endurance in
seconds halves for the finite-magazine coilers. Native internal size 4 remains
a compatibility adapter; effective module volume counts 5 for fuel capacity,
and weapon information displays size 5. Tier-5 fitting accepts no regular-N4
fallback in new designs. The 768 cm graser remains N3 with its old cadence.

## Precision implementation limits

`WavelengthPrecision` owns exact integer-angstrom records. Only the two known
zero-rounded graser records override the native field. Getter/spot and display
edges convert to metres/nanometres; native integer-nm fields/save units are not
reinterpreted. Existing nonzero competing template overrides are respected.
Future fractional custom weapons require a precision record; this is not a
general rewrite of the game's template serialization system.

DLL SHA256: `7D3CA8AFFF15D219227221D3D160B009B2977B2974ADD340D824AFA0D45CFFDE`.
