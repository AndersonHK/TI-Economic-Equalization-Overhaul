# Formal plan: ship tiers 4–6 and size-5 nose weapons

**Latest classification proposal:** the [asymmetric five-tier plan](asymmetric-human-alien-tier-plan-2026-11-04.md)
supersedes this document's mirrored species mapping, occupied tier-6 band and
Alien Lancer/Titan mass implications. Human Titan and Alien Mothership are T5;
Alien Titan/Carrier are T4. The cadence and precision work below remains relevant,
but fitting must follow the newer tier table. These tier/N5 changes are not implemented.

Status: **tentative implementation plan, not implemented**. Date: 2026-11-03.
Scope follows the user's proposed six ship tiers, mandatory size-5 capital noses,
doubled size-5 sustained output, tier-4 Lancers, corresponding cost changes and
the wavelength-precision fix. The supporting
[progression and weapon analysis](hull-progression-and-capital-weapon-analysis-2026-11-03.md)
contains the save/unlock findings, same-tech inventory and tradeoffs.

## 1. Target rules

1. Establish one authoritative **ship tier** from 1 through 6, distinct from the
   native 1–3 construction-tier field.
2. Tier 4 includes both human and alien Lancers and Dreadnoughts. These use one
   heavy hull weapon per hull cell and one regular size-4 nose weapon per nose
   cell. Lancers must receive the complete compact system, not a label alone.
3. Tier 5 includes human and alien Titans; tier 6 is Alien Mothership. Their
   nose cells accept **only size 5**, one weapon per cell. Size 5 is also
   unavailable on lower-tier hulls. This makes it an actual equipment privilege.
4. Size 5 is a distinct, explicit weapon category. The initial inventory below
   comes from heavier same-project size-4 alternatives, including unnamed ones.
5. Each selected size-5 weapon delivers **twice its own current nominal sustained
   output** by halving its complete firing schedule. Preserve pulse/shot damage,
   mass, efficiency, projectile behavior and resource costs unless a separate
   balance decision changes them.
6. Apply size-5 capacity units to ship mass/crew and occupied volume; retain full
   weapon/magazine costs. Fix fractional laser wavelength handling at unit edges.

All other hulls retain their current layouts and native fitting rules unless
explicitly added to this plan. Auxiliary hulls/fighters require explicit entries
or exemptions; do not derive their combat role solely from length or native tier.

## 2. Authoritative tier and weapon catalogs

Introduce a mod-owned, versioned catalog shared by UI, fitting, AI, capacity,
costs and migration. Its format must not be discovered as a native template file
(the repository uses private `.eeo` catalogs for this purpose).

Hull records should include `dataName`, `shipTier`, native construction tier,
role, hull-mount mode, nose grade, and capacity-unit totals. Weapon records should
include `dataName`, logical size, native mount, family, allowed tiers and an
explicit standard counterpart for migration. Reject duplicate IDs, unknown
template IDs, impossible layouts and contradictory rules at load/validation.

Suggested formal classification, refined by the separate
[Frigate/Cruiser boundary plan](frigate-and-cruiser-tier-boundary-plan-2026-11-03.md):

| Ship tier | Main human classes | Main alien equivalents |
| --- | --- | --- |
| 1 | Gunship, Escort, Corvette | Corresponding light hulls |
| 2 | Frigate, Monitor, Destroyer | Corresponding escort hulls |
| 3 | Cruiser, Battlecruiser, Battleship | Corresponding fleet hulls; auxiliary classes handled explicitly |
| 4 | Lancer, Dreadnought | Alien Lancer, Alien Dreadnought |
| 5 | Titan | Alien Titan |
| 6 | No current human hull | Alien Mothership |

The native `consTier` remains a construction/infrastructure compatibility field,
not the newly authoritative combat tier. For example, Alien Dreadnought retains
native construction tier 2 while becoming formal ship tier 4. Human Tier-3–5
line hulls continue using native construction tier 3. Renaming the native field's
meaning to “ship tier” would require replacing its numerous AI, utility,
construction and power consumers; the existing AI includes expressions that
become zero/negative for tiers 4–6. This separate catalog is a formal gameplay
classification, not merely a UI description.

Size 5 can use `FourNose` as its **native geometry adapter**, because compact
mounts already occupy one cell. Do not append a value to a native enum or store
an out-of-range enum constant. The mod's size-5 category must actually control
catalog display, fitting, AI, effective occupied volume and capacity pricing.
Audit every native `internalSize` consumer before choosing whether to patch the
getter or translate at the affected boundaries. The recommended adapter keeps
native geometric size 4 and uses an explicit effective-size accessor for the
new category. A global `internalSize=5` override would also change native turret
training speed (`270/internalSize`) and scoring, which are not part of a
least-disruptive throughput change.

## 3. Initial size-5 inventory

| Size-5 weapon | Standard size-4 counterpart | Shared project |
| --- | --- | --- |
| Spinal Siege Coiler Mk1 | Spinal Coiler Mk1 | Coil Cannon Mk1 |
| Spinal Siege Coiler Mk2 | Spinal Coiler Mk2 | Coil Cannon Mk2 |
| Spinal Siege Coiler Mk3 | Spinal Coiler Mk3 | Coil Cannon Mk3 |
| Alien Relativistic Particle Cannon | Alien Spinal Particle Cannon | Alien Master Project |
| Alien 1024 cm Graser Cannon | Alien 1024 cm Xaser Cannon | Alien Advanced Master Project |

Do not include Advanced Alien Spinal Mag Cannon or the violet laser merely
because they share a project with a weaker weapon: they are not heavier
same-family alternatives. Smaller `HeavySiegeCoiler` size-3 weapons remain a
separate decision; this initial split promotes the heavier **size-4** variants.
Human particle lances and regular heavy plasma weapons are not silently
reclassified into size 5 without a matched-category decision.

All five proposed N5 templates currently share a model resource with their
matched N4 counterpart. A new size label does not create a physically larger
mesh. Preserve the current art dimensions in this least-disruptive pass and
distinguish N5 in the equipment UI. Any later visual enlargement requires its
own measured hardpoint-clearance, aiming, muzzle, rendering and collision checks,
especially on six-nose alien hulls; do not scale those meshes automatically.

This launch inventory makes human size-5 noses a coil-only category and alien
size-5 noses particle/optical categories. That is a substantial doctrinal change.
The implementation must expose that clearly. It must not fabricate research,
fit a forbidden regular weapon as fallback, or silently hand the AI an invalid
design. Before implementing mandatory noses, settle one of these launch choices:

- Keep the small inventory and explicitly make access to at least one size-5
  weapon part of the Titan eligibility/research path; for present human weapons
  that means Coil Cannon Mk1 or a later mark.
- Add approved size-5 alternatives for other intended human/alien families,
  with new identities, costs, research paths and validation. This is a larger
  balance/content scope than the five-template split.

Empty noses may remain technically legal while research is incomplete, but an
AI design accepted as a completed capital design must meet an explicit minimum
legal armament rule. Do not imply that a blank bank satisfies the intended
capital progression just because the hull can be saved.

## 4. Least-disruptive doubling of sustained output

Halve **both** `cooldown_s` and `intraSalvoCooldown_s` for salvo weapons; halve
the single cooldown for single-shot weapons. Keep salvo shot count unchanged.
This doubles `shots / (cooldown + (shots-1)*intraSalvo)` exactly in the nominal
model, preserving per-hit thresholds, velocity, round durability and beam optics.
Changing only the final cooldown would not double a multi-shot weapon's output.

| Weapon | Final cooldown, old -> new (s) | Intra-salvo, old -> new (s) | Raw output, old -> new (MW) | New raw output / regular size-4 counterpart |
| --- | ---: | ---: | ---: | ---: |
| Siege Coiler I | 24 -> 12 | 22 -> 11 | 419.00 -> 837.99 | 3.669x |
| Siege Coiler II | 19 -> 9.5 | 15 -> 7.5 | 796.22 -> 1,592.45 | 3.590x |
| Siege Coiler III | 12 -> 6 | 11 -> 5.5 | 1,915.63 -> 3,831.25 | 3.840x |
| Alien Relativistic Particle | 64 -> 32 | 3 -> 1.5 | 43.886 -> 87.771 | 1.987x |
| Alien 1024 cm Graser | 36 -> 18 | Not applicable | 12.444 -> 24.889 | **1.333x** |

Native fields and normal cooldown handling support fractional seconds. Preserve
9.5/7.5/5.5/1.5 rather than rounding and losing the exact doubling. Update the
older whole-second magnetic-tuning assertions only for the explicit size-5
cohort; do not relax validation for other guns. Check native shot jitter,
50 ms scheduling, officer/fire-control modifiers and target acquisition in
managed scheduling tests and live combat.

Consequences: average electrical demand, average waste heat and ammunition
consumption per second double. Per-shot electrical demand remains fixed, while
the burst is delivered in half the time. Magazine endurance halves. The existing
Siege Mk3 nose bank of four weapons would demand about **25.015 GW** average
electrical input and **7.504 GW** average weapon waste heat, before reactor
waste heat, ship systems, other weapons and support changes.

**Important acceptance issue:** “double each size-5 weapon” is not equivalent
to “every size-5 weapon has at least twice the size-4 counterpart's output.”
Graser begins at two-thirds the xaser's raw throughput, so doubling reaches
only 1.333x. It retains a concentration/armor advantage. If the desired minimum
is 2x raw output versus size 4 for every family, graser needs a 3x change from
its old baseline (or another approved adjustment), rather than the universal
2x rule. Do not silently add that extra buff. Validate its armor-weighted
advantage and decide whether +33% raw output satisfies “substantial upgrade.”

## 5. Tier-4 Lancers and their complete consequences

Promote both Lancers to the tier-4 compact policy, while keeping the shared
human Ships of the Line unlock with Dreadnought. Preserve saved slot indices.
Human Lancer: **four regular size-4 noses and three heavy hull weapons**.
Alien Lancer: **six regular size-4 noses and four heavy hull weapons**, subject
to explicit acceptance of that stronger alien nose-specialist identity.

This adds 21 effective weapon units to human Lancer (nose +12, hull +9) and
30 to Alien Lancer (nose +18, hull +12). Human base crew therefore becomes
**35**, not 26: 26 would account only for its nose conversion. Alien crew
remains 40 under the current human-only crew rule. Human Lancer remains a
nose specialist relative to Dreadnought's eight heavy broadside weapons;
Alien Lancer has six noses versus Alien Dreadnought's four, but four versus
eight heavy hull weapons and fewer utilities/integrity.

Add both model-controller types to compact mount handling, check distinct
native anchors for every slot and human appearance, and validate their icon/
core-slot/cell occupancy paths. Reuse authored individual anchors where they
exist. No extra cells are requested, so do not assume new meshes or a bespoke
designer row are necessary. Any missing/overlapping physical anchors require
a measured, symmetric correction and live visual check.

## 6. Capacity, crew, hull mass and resource costs

Use the [all-class volume/capacity audit](hull-volume-capacity-comparison-2026-11-03.md)
and completed [alien mesh/bay audit](alien-reactor-bay-measurements-2026-11-04.md)
alongside these provisional prices. Main hull, machinery bay and engine-art
coefficients are different inputs. The new Mothership geometry supports a
roughly 137,500 m³ bounded allowance versus Alien Titan's modeled 14,600, but
both inputs are now deployed by 0.10.24. Both hulls already reach all current alien
reactors' theoretical output ceilings with the old runtime fallback. Do not
assume a larger bay alone supplies more GW; validate the entire N5 propulsion,
electrical, thermal and battery budget. The proposed Mothership 9,000 t bare
price below remains prior capacity-unit bookkeeping, not a final structural/
cost calibration of its much larger geometry.

Size-5 nose cells count as **five** effective capacity units. Tier-4 noses count
as four; heavy hull cells retain the current four-unit budget. Continue one new
human base billet per added effective unit and 3 t support per human billet.
Use the current 20 t structural reinforcement per additional effective unit,
then reconcile the same nearest-100-t bare-mass convention. These are proposed
initial values using the existing rule, not fully simulated balance targets.

| Hull | Proposed weapon layout | Added effective units from 0.10.23 | Base crew | Proposed bare mass, V0 (t) | Proposed structure (t) |
| --- | --- | ---: | ---: | ---: | ---: |
| Human Lancer | 4xN4 + 3 heavy hull | 21 | 14 -> **35** | 2,000 -> **2,500** | **2,395** |
| Alien Lancer | 6xN4 + 4 heavy hull | 30 | 40 | 1,656 -> **2,300** | **2,180** |
| Human Titan | 4xN5 + 6 heavy hull | 4 | 52 -> **56** | 3,700 -> **3,800** | **3,632** |
| Alien Titan | 6xN5 + 8 heavy hull | 6 | 60 | 3,000 -> **3,100** | **2,920** |
| Mothership | 6xN5 + 16 heavy hull | 6 | 100 | 8,900 -> **9,000** | **8,700** |

Nearest-100 rounding can put the final increase slightly above/below the raw
20 t/unit budget; for example Alien Titan's raw bare target is 3,120 t and
rounds to 3,100 t. Preserve existing appearance differences: proposed human
Lancer bare masses 2,500 / 3,000 / 4,400 / 4,400 t, and Titan
3,800 / 4,700 / 3,900 / 5,600 t, with structural masses reconciled using the new
base crew. Dreadnoughts' physical counts and base crew remain as currently set.

Weapon equipment already pays its full mass, magazine and material costs.
Do not charge a 1,500 t siege mechanism again through bare hull mass. Conversely,
the modest bare-hull increase does not represent the whole capital price:
four human N5 siege weapons alone add 6,600 t, plus the increased power/cooling
plant. Increasing hull structure automatically changes its material bill through
the native resource-cost calculation. Record full build/refit resource vectors,
reactor/radiator/battery costs and feasible propulsion, not just nominal tonnage.

Update occupied-volume accounting to five units (default **2,000 m³**) per N5,
rather than native `internalSize=4` / 1,600 m³. Preflight fuel/engine-bay limits
on legacy ships. Do not rescale measured hull geometry or drive art merely
because the formal tier number changed. Keep MC cost and construction-time
retuning as explicit additional knobs: current human Titan MC 5 and alien MC 1
should not be silently multiplied by the new tier. The first balance pass can
retain them while the resulting full resource/support cost is measured.

## 7. Wavelength precision fix

Implement the separately documented
[integer-ångström precision proposal](laser-wavelength-precision-bug-2026-11-03.md)
as a focused fix, preferably before measuring the final size-5 optical balance.
It affects **both** Alien768cmGraserCannon and Alien1024cmGraserCannon; only the
latter is promoted to N5 by this plan. Correct physical wavelength and UI units
without rewriting every native nanometre field into a different unit.

## 8. Save, design and migration behavior

Introduce a new migration schema after schema 2. Existing compact layouts must
remain compact; do not expand their size-4 entries across the old footprint
again. Expand legacy shared Lancer footprints cell-for-cell into the new
independent mounts. On tier-5/6 noses, replace regular N4 equipment with the
explicit paired N5 alternative only where one exists; retain old occupancy and
ammo/damage/repair fractions. Empty cells remain empty.

Two cases need an explicit policy before implementation/deployment:

- A legacy Titan/Mothership has an N4 family with **no** N5 counterpart, such as
  a human laser/plasma or alien magnetic cannon. Do not arbitrarily cross-convert
  it into a different family without an approved replacement table.
- A tier-4 Dreadnought currently carries a weapon promoted to N5. Strict new
  fitting requires replacing it with its N4 counterpart; this is a downgrade of
  expensive installed equipment. Choose a disclosed replacement/refund policy,
  or approve a built-ship-only grandfather exception until refit. Do not silently
  delete its material value or claim that an exception meets strict new-fit rules.

Produce a read-only preflight report from the current save covering affected
designs, built ships and queues, proposed replacements, resource deltas,
unsupported families and capacity overruns. Archive the original save before
committing migration, prepare all conversions before mutation, and retain the
save blocker on failure. Preserve queued dates, propellant where the approved
legacy-capacity exception applies, fire modes, ammunition fractions and repair
plans. No research grants or free ammo refill. New designs/refits obey the new
strict rules and do not inherit broad legacy exceptions.

## 9. Implementation order and acceptance

1. Document the final tier/size catalogs, five-template split, cadence deltas,
   cost/crew targets, unlock policy, and unresolved migration/coverage choices.
2. Correct and validate wavelength precision as a small independent patch.
3. Implement authoritative classification and common eligibility/capacity helpers;
   wire designer lists, icons, drops, core-slot selection, save/refit validation,
   stock/import normalization, player auto-design and AI to them.
4. Extend Lancer layouts/model mapping, implement the selected N5 cadence and
   mass/crew changes, then extend schema migration with concrete replacement maps.
5. As soon as each implementation is ready, run `tools/deploy.ps1` without
   skipping verification. It must assert the game is closed before validation
   and again before replacing files. Announce manual-test readiness immediately
   after deployment, then complete analysis/documentation while testing proceeds.

Required checks include all 28 hull templates and every graphical appearance;
seven affected compact hull classes; same-size family exclusions; all five N5
cadences and exactly-2x nominal throughput; fractional/native cooldown behavior;
unchanged per-shot geometry/damage/energy; full resource and capacity accounting;
no-N5-research AI/design outcomes; both Lancer species' distinct anchors;
schema-0/1/2 campaigns, stock and imported designs, built ships, refits and queues;
idempotence, atomic preflight, resource/damage/ammo preservation and future-schema
rejection. The normal existing validators and implementation matrix still apply.

Live acceptance must cover the new tiers and size labels; all legal/illegal
fitting transitions; aiming, renderer scale and muzzle effects; long firing
sequences with adequate and inadequate batteries/cooling; heavily armored
stations and ships, agile ships and layered PD; equal-slot and equal-budget
fleet comparisons; both AI factions; and an actual legacy-save load/save/reload.
Source tests and startup evidence alone do not establish coherent gameplay.
