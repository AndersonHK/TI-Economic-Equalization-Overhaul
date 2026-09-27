# Titan and Mothership heavy-mount proposal

Historical planning snapshot. The approved change is implemented as **0.10.0**;
see the [implementation record](capital-heavy-mount-implementation.md) for the
final migration policy, release values and testing status. The baseline tables
below remain unchanged for comparison.

Date: 2026-09-25. Revision 4, incorporating explicit V0 mass targets and deferring
all nose-weapon rebalancing. **Planning only:** EEO 0.9.8 / Terra Invicta 1.0.53b
baseline; proposed release 0.9.9. No gameplay, version metadata, saves, build,
or deployment changed.

Each existing Titan or Mothership hull cell accepts one complete 2×2 weapon,
rendered independently at that cell's hardpoint. Smaller hull weapons become
invalid. The weapon retains its full physical costs. Add one utility cell to
Dreadnoughts, three to Titans, and four to Motherships.

## Settled direction and boundaries

- Apply compact heavy-only hull mounts to `Titan`, `AlienTitan`, and
  `AlienMothership`, for all appearances and human/AI owners. Only `FourHull`
  weapons qualify. Nose placement geometry remains unchanged.
- Utility additions cover both human and alien Dreadnought/Titan variants.
- **Do not change alien crew.** Their broader hull rebalance has not happened;
  only the explicitly requested hull mass increases are included. There is no alien base-crew charge for these new
  weapon or utility cells. Installed modules still contribute their existing
  module crew naturally; no alien crew template values are being retuned.
- Human base crew follows nose cells + weighted hull capacity + utility cells:
  Dreadnought 18→19; Titan 19→40. Crew is primarily flavor, particularly late
  game. It is not intended to counterbalance capital firepower.
- **Round human large bare hull + base crew to the nearest 100 t**, then adjust
  structural mass so the total reconciles. This proposal applies that convention
  to Cruiser, Battlecruiser, Lancer, Battleship, Dreadnought, and Titan, for every
  appearance, except the explicit V0 targets below take precedence. Smaller human
  hull masses remain as-is. Titan V1/V2/V3 retain the previous rounded targets
  of 4,300/3,500/5,200 t; this revision changes its V0 target only.
- **Explicit bare-mass V0 targets:** Titan 3,400 t; Alien Dreadnought 2,200 t;
  Alien Titan 2,600 t; Alien Mothership 8,500 t. Reconcile structural mass using
  the planned human crew or unchanged alien crew at 3 t per billet.
- Dreadnoughts retain small/medium weapons and gain utility capacity, making
  mixed armament more relevant. A later Dreadnought refactor remains possible.
- Some capital PD is intended. Large lasers gain penetration/range effectiveness
  from optics, whereas projectile interception is strongly limited by cadence
  and target servicing. Retain heavy laser defensive modes and legal heavy
  antimatter PD. **No additional PD ban is proposed.**
- Preserve hull volume, armor geometry, drive scaling, reactor bays, MC and hull
  construction times in this proposal. **All nose-weapon stat corrections are
  deferred** until the user's tuning and combat tests establish the direction.
  Existing nose identities, placement, mass, power, DPS, cadence, optics and
  ammunition remain unchanged. The audit and A/B models are research only.

## Revised hull, crew, and mass table

Arrows mean current→proposed; a single value is unchanged. Hull size units are
physical weapon-size capacity, not measured DPS. Bare mass includes structural
hull and base hull crew only, at 3 t per crew. V0 is appearance 0.

| Hull | Nose cells | Hull cells | Hull size units | Utilities | Base crew | Bare mass V0, t |
| --- | --- | --- | --- | --- | --- | --- |
| Gunship | 1 | 0 | 0 | 2 | 3 | 180 |
| Escort | 0 | 2 | 2 | 2 | 4 | 350 |
| Corvette | 1 | 1 | 1 | 3 | 5 | 400 |
| Frigate | 1 | 2 | 2 | 5 | 8 | 600 |
| Monitor | 0 | 4 | 4 | 3 | 7 | 700 |
| Destroyer | 2 | 2 | 2 | 5 | 9 | 900 |
| Cruiser | 2 | 3 | 3 | 7 | 12 | 1,000 |
| Battlecruiser | 3 | 2 | 2 | 5 | 10 | 1,200 |
| Lancer | 4 | 3 | 3 | 7 | 14 | 2,000 |
| Battleship | 2 | 6 | 6 | 6 | 14 | 1,600 |
| Dreadnought | 3 | 8 | 8 | 7 → 8 | 18 → 19 | 2,400 |
| Titan | 4 | 6 | 6 → 24 | 9 → 12 | 19 → 40 | 3,200 → 3,400 |
| AlienDreadnought | 4 | 8 | 8 | 9 → 10 | 50 | 2,166 → 2,200 |
| AlienTitan | 6 | 8 | 8 → 32 | 7 → 10 | 60 | 2,484 → 2,600 |
| AlienMothership | 4 | 16 | 16 → 64 | 7 → 11 | 100 | 7,980 → 8,500 |

Explicit V0 targets take precedence; otherwise rounding is applied after adding
human crew:

`new bare mass = round_to_100(old structural mass + 3 × new base crew)`

`new structural mass = new bare mass − 3 × new base crew`

For Dreadnought V0 this gives `round(2346 + 3×19) = 2400 t`, with **2343 t**
structure. Titan V0 uses the explicit **3400 t** target, with **3280 t**
structure after its 40 base crew. Alien Dreadnought/Titan/Mothership use
**2050 / 2420 / 8200 t** structural mass, respectively, plus their unchanged
**50 / 60 / 100** base crew. Relative to current bare masses, these four targets
add **200 / 34 / 116 / 520 t**, respectively. Human values must be updated in both the template baseline and the
appearance-specific mass catalog; changing only the JSON would be overridden
by the current runtime mass lookup. Rounding is a planning-time authored value,
not a runtime adjustment whenever a module's crew changes.

| Hull | Extra base crew | Crew support Δ, t | Net bare mass Δ, t | Extra water and volatiles, t/year EACH | Occupied capacity + base crew volume Δ, m³ |
| --- | --- | --- | --- | --- | --- |
| Dreadnought | 1 | 3 | 0.0 | 2 | 250 |
| Titan | 21 | 63 | 200.0 | 42 | 6150 |
| AlienDreadnought | 0 | 0 | 34 | 0 | 200 |
| AlienTitan | 0 | 0 | 116 | 0 | 6600 |
| AlienMothership | 0 | 0 | 520 | 0 | 12800 |

The volume column assumes all old hull cells and all added utility cells are
occupied, and includes only the base-crew difference. Weapon/module crew changes
are additional. Empty utility cells have no installed-module volume cost.
Defaults: 50 m³ per crew; 250 m³ per hull size unit; 200 m³ per utility cell;
2 t water and 2 t volatiles per crew annually. Compact heavies remain 1,000 m³
apiece. Alien utility growth still consumes module volume when occupied.

The existing construction calculation charges 2 t water + 2 t volatiles per
crew, whereas support dry mass is 3 t. This pre-existing discrepancy remains
documented separately. Extra human base crew adds 0.2 water/0.2 volatile resource
points for a Dreadnought and 4.2/4.2 for a Titan, before difficulty modifiers;
alien base-crew additions are zero. Structural-mass rounding and installed
equipment also alter construction costs. Do not silently fix the global crew
package as part of this refactor.

## Deferred nose research: comparison models A and B

Neither nose normalization variant is part of the active implementation plan.
They remain here as analytical weights and research for future combat testing;
choosing or implementing either one is not a prerequisite for the capital refactor.

Use the following **design weights relative to a small hull weapon**:

| Mount | A | B |
| --- | ---: | ---: |
| Hull, 1×1 | 1 | 1 |
| Hull, two cells | 2 | 2 |
| Hull, 2×2 | 4 | 4 |
| Nose size 1 | 2 | 3 |
| Nose size 2 | 4 | 6 |
| Nose size 3 | 8 | 12 |
| Nose size 4 | 16 | 24 |

Both alternatives double nose capability each size step. Thus a size-4 nose is
**8× its size-1 nose counterpart**, and 16× or 24× the small-hull reference.
The original “16× size 1” wording is superseded by the clarified hull-relative
definition. A six-cell alien nose does not score 2^6: it can fit a four-cell
mount plus two more cells, giving 20 units in A or 30 in B.

The installed game does not consistently follow either progression. The
[nose audit and numerical A/B candidates](nose-weapon-scaling-audit-2026-09-25.md)
cover 120 weapons in 35 related families. They distinguish mounting mass,
full-cycle raw throughput, mean electrical draw, and optical effectiveness.

### Laser exception: optics are part of firepower

Larger lenses improve focusing, range performance, and armor penetration without
requiring proportional extra emitted energy. Geometrically increasing both
lens effectiveness and shot energy would compound their benefits beyond the
intended comparison with other weapons. **Do not force lasers onto geometric
shot-energy, power-draw, or raw-DPS curves.** Preserve their authored shot energy,
cooldown, efficiency, aperture, jitter, range, and mounting mass in both A and B
pending a target-based optical audit. Different laser generations can have
different mounting-mass baselines; that alone is not an error.

For example, the human laser nose series currently uses 200/300/350/400 MJ per
shot. A same-generation small hull laser uses 100 MJ, so size-1 nose emitted
energy is 2× here. Some mounting masses are approximately 3× and useful damage
at a chosen range can differ again. A and B therefore bracket design capability;
neither is asserted to describe universal current raw laser DPS.

## Firepower per bare mass and hull volume

These are **weighted capacity comparisons**, applying the same A or B weights
to today's layout and the proposed layout. “Current” here means current layout
under that model, not a claim that today's inconsistent weapons already deliver
those DPS ratios. Actual-template reference fits are provided separately.

Method: maximize legal, non-overlapping nose mount capacity using the native
slot geometry; add hull capacity. The capital hull contribution changes from
H to 4H. Unchanged hulls keep the same capacity. Nose hardpoint placement is
not altered. The mass denominator is bare hull + base crew; the volume is the
maintained appearance-specific main-hull mesh envelope, not usable internal
space. Below uses V0. All 51 appearances represented by these 15 hulls have
their own masses, volumes and density calculations in the linked JSON snapshot.

### A: nose weights 2 / 4 / 8 / 16

| Hull | Nose units | Total units, current → proposed | Bare mass, t | Envelope, m³ | Units / 1,000 t | Units / 100,000 m³ |
| --- | --- | --- | --- | --- | --- | --- |
| Gunship | 2 | 2 | 180 | 11,020 | 11.11 | 18.15 |
| Escort | 0 | 2 | 350 | 6,702 | 5.71 | 29.84 |
| Corvette | 2 | 3 | 400 | 5,071 | 7.50 | 59.16 |
| Frigate | 2 | 4 | 600 | 13,692 | 6.67 | 29.21 |
| Monitor | 0 | 4 | 700 | 17,160 | 5.71 | 23.31 |
| Destroyer | 4 | 6 | 900 | 22,062 | 6.67 | 27.20 |
| Cruiser | 4 | 7 | 1,000 | 45,820 | 7.00 | 15.28 |
| Battlecruiser | 8 | 10 | 1,200 | 43,684 | 8.33 | 22.89 |
| Lancer | 16 | 19 | 2,000 | 152,598 | 9.50 | 12.45 |
| Battleship | 4 | 10 | 1,600 | 105,143 | 6.25 | 9.51 |
| Dreadnought | 8 | 16 | 2,400 | 272,229 | 6.67 | 5.88 |
| Titan | 16 | 22 → 40 | 3,200 → 3,400 | 514,552 | 6.88 → 11.76 | 4.28 → 7.77 |
| AlienDreadnought | 16 | 24 | 2,166 → 2,200 | 302,990 | 11.08 → 10.91 | 7.92 |
| AlienTitan | 20 | 28 → 52 | 2,484 → 2,600 | 349,989 | 11.27 → 20.00 | 8.00 → 14.86 |
| AlienMothership | 16 | 32 → 80 | 7,980 → 8,500 | 49,197,069 | 4.01 → 9.41 | 0.07 → 0.16 |

### B: nose weights 3 / 6 / 12 / 24

Same masses and volumes as A:

| Hull | Nose units | Total units, current → proposed | Units / 1,000 t | Units / 100,000 m³ |
| --- | --- | --- | --- | --- |
| Gunship | 3 | 3 | 16.67 | 27.22 |
| Escort | 0 | 2 | 5.71 | 29.84 |
| Corvette | 3 | 4 | 10.00 | 78.88 |
| Frigate | 3 | 5 | 8.33 | 36.52 |
| Monitor | 0 | 4 | 5.71 | 23.31 |
| Destroyer | 6 | 8 | 8.89 | 36.26 |
| Cruiser | 6 | 9 | 9.00 | 19.64 |
| Battlecruiser | 12 | 14 | 11.67 | 32.05 |
| Lancer | 24 | 27 | 13.50 | 17.69 |
| Battleship | 6 | 12 | 7.50 | 11.41 |
| Dreadnought | 12 | 20 | 8.33 | 7.35 |
| Titan | 24 | 30 → 48 | 9.38 → 14.12 | 5.83 → 9.33 |
| AlienDreadnought | 24 | 32 | 14.77 → 14.55 | 10.56 |
| AlienTitan | 30 | 38 → 62 | 15.30 → 23.85 | 10.86 → 17.71 |
| AlienMothership | 24 | 40 → 88 | 5.01 → 10.35 | 0.08 → 0.18 |

Including noses materially changes the interpretation. Titan total capacity
increases **22→40 (+81.8%) in A**, or **30→48 (+60.0%) in B**, rather than a
fourfold increase in whole-ship firepower. The fourfold statement applies only
to its hull weapon capacity. Mothership totals become **32→80 (+150%)** or
**40→88 (+120%)**. Its enormous 49.2-million-m³ envelope remains an outlier;
this refactor does not attempt to bring alien mass and graphical volume into
the human balance model.

For human V0 comparisons, proposed Titan capacity per 1,000 bare tonnes is
11.76 in A or 14.12 in B; Lancer is 9.50 or 13.50; Dreadnought is 6.67 or 8.33.
Per 100,000 m³, Titan is 7.77 or 9.33, Lancer 12.45 or 17.69, and Dreadnought
5.88 or 7.35. Thus the proposal improves capital efficiency but does not make
the Titan best on every density measure.

### Actual-template reference fits

The active proposal uses existing weapon stats with the new mounts and hull
mass targets. Its updated reference fits are:

| Hull / family | Raw DPS, MW current → proposed | DPS / 1,000 t reference | DPS / 100,000 m³ |
| --- | --- | --- | --- |
| Titan / Green phaser | 85.0 → 190.0 | 18.9 → 31.1 | 16.5 → 36.9 |
| Titan / Coil Mk3 | 1,478.3 → 3,101.6 | 400.5 → 664.4 | 287.3 → 602.8 |
| AlienDreadnought / Advanced alien magnetic | 4,134.3 | 1,523.9 → 1,505.0 | 1,364.5 |
| AlienTitan / Advanced alien magnetic | 5,088.6 → 10,551.1 | 1,622.1 → 2,472.7 | 1,453.9 → 3,014.7 |
| AlienMothership / Advanced alien magnetic | 5,955.1 → 16,880.1 | 671.8 → 1,479.0 | 12.1 → 34.3 |

The companion audit also retains explicitly deferred A/B nose-correction
scenarios for research, using green phasers, Mk3 coils, and advanced alien magnetic weapons.
Those report actual nominal energy throughput and include weapon mounting mass
and weapon crew in the mass denominator. They exclude ammunition, armor,
propulsion, power plants, radiators, utilities, and fuel. These are reproducible
reference fits, not complete viable ships or combat DPS predictions.

For a Titan with one spinal coil plus its largest hull batteries:

- Current: 1,478.3 MW nominal raw output.
- Active proposal, retaining current weapon stats: 3,101.6 MW.
- Deferred research only, with nose candidate A: 3,144.1 MW.
- Deferred research only, with nose candidate B: 3,664.2 MW.

The active proposal's nominal output per 1,000 reference tonnes is recalculated
in the table above using the 3,400 t bare hull. Deferred candidate A increases output but also mounting
mass, so it can be less mass-efficient than the mount-only case. Required reactor,
radiator and ammunition changes must be included in the later complete-design
comparison. Laser raw output must not be compared across hulls as a substitute
for penetration and on-target damage at range.

For continuity, the earlier hull-only migration examples now include rounding:

| Titan hull armament | Hull + weapon crew | Hull + loaded weapons + crew, t |
| --- | --- | --- |
| 6 × LightCoilgunBatteryMk3 → 6 × HeavyCoilgunBatteryMk3 | 37 → 70 | 3,611 → 5,098 |
| 3 × CoilgunBatteryMk3 → 6 × HeavyCoilgunBatteryMk3 | 31 → 70 | 3,611 → 5,098 |
| 6 × 60cmGreenPhaserBattery → 6 × 360cmGreenPhaserBattery | 31 → 64 | 3,926 → 5,362 |

## Implementation and migration plan

1. **Lock the current weapon baseline.** Implement the capital mount/utility/mass
   plan using existing weapon stats. Keep every nose correction deferred until
   the user finishes tuning and combat tests. A/B remain research artifacts;
   neither is selected for this release.
2. **Separate physical properties from placement.** One hull-aware policy makes
   a legal capital `FourHull` weapon occupy only its anchor. Preserve its real
   mount and `internalSize=4`. Do not mutate global weapon templates to `OneHull`
   or multiply `hullHardpoints`. All physical costs use the actual weapon.
3. **Enforce eligibility throughout the designer.** Native-style disabled alpha
   via `ValidPartForDesign`; rejected illegal drops via `LegalModuleForSlot`;
   matching stored-design, copied-design, refit, import and completion checks.
   Patch occupancy and slot-set consumers so neighboring cells are independent.
4. **Render each cell independently.** Use heavy weapon art in a one-cell
   installed designer frame; preserve each existing 3D anchor and normal
   dorsal/ventral treatment. No neighboring-cell merges. Check all four human
   Titan appearances and alien capital models for pivots, aiming, clipping,
   effects, and duplicate functional mounts. Trace exact rendering hooks on the
   current assembly before implementation.
5. **Update hull data and rounded masses.** Add utility entries as well as
   `internalModules`; append entries to preserve serialized slot indices.
   Arrange useful adjacent utility pairs for the mod's larger utilities.
   Verify JSON array merge behavior. Update human hull crew, baseline mass,
   and the flat appearance mass lookup together. Set the three alien structural
   masses to the explicit targets minus unchanged crew support. Do not add a
   hidden runtime alien capacity-to-crew surcharge.
6. **Make AI fitting obey the same rules.** Audit `SetShipDesignHullWeapons`,
   `DesignAlienShip`, human AI, player autodesign, forced/skirmish designs and
   refits. Capitals budget one anchor per heavy and pay full physical cost.
   Faction unlocks, resource permissions, power and heat checks still apply.
   Small-weapon fleet roles must select eligible hulls instead of looping on
   an incompatible Titan. Invalidate weapon/role score caches after stat changes.
7. **Verify unchanged nose behavior.** Do not apply any audit target to weapon
   templates or runtime stats. Include a regression check that nose placement,
   mounting mass, power, DPS, cadence, optics and ammunition retain the baseline.
8. **Add a save-persistent migration schema**, separate from display version,
   and advance release metadata together when implemented. Inspect the save
   extension surface before selecting the marker mechanism. The conversion
   must run once and never infer old/new state solely from whether mounts
   overlap. Archive a backup and a dry-run report before committing migration.
9. **Build → Deploy → Test → Document.** Once implementation is ready, run
   `tools\deploy.ps1` with its normal verification and programmatic game-closed
   assertions. Immediately announce readiness for manual testing after successful
   deployment, then finish documentation while testing proceeds. Planning-only
   documentation and analytical calculations require no mod deployment.

Confirmed integration surfaces include `TIShipHullTemplate.WeaponSlotSet`,
`ValidBigWeaponSlotSets`, `AssignCoreSlotOnMultiMountPlacement`, and
`TISpaceShipTemplate.ValidAssignedSlotForLocation`, `GetPartInHullSlotIndex`,
`ValidPartForDesign`, `AreWeaponModulesValidForRefit`. Existing utility footprint,
fuel capacity, ship power, and alien design patches must remain compatible.

### Migration rules

| Old capital hull installation | Result |
| --- | --- |
| One 1×1 weapon | One eligible heavy at that cell |
| One two-cell weapon, either orientation | Two eligible heavies, one per formerly occupied cell |
| One 2×2 weapon | Four copies, one per formerly occupied cell |
| Empty cell | Remains empty |
| Nose weapon | Unchanged identity, position and all weapon stats |
| Existing utility | Preserved at its slot; new slots start empty |

Resolve old footprints before switching the design to compact semantics. Prefer
same-family and same-tier heavy counterparts: rail/coil Mk, laser wavelength
and generation, plasma Mk, particle family. Use explicit mappings. Missiles and
naval guns have no 2×2 equivalents in the installed catalog; use a documented,
deterministic ranking of eligible unlocked heavy weapons (prefer highest
unlocked magnetic generation/Mk, then explicitly ranked alternatives).
Native faction eligibility is required in addition to project unlocks.

Cover design catalogs including obsolete classes, existing ships, build queues,
refit old/new references, imports and skirmish designs. Deduplicate shared
objects. Rebuild per-weapon state arrays; preserve proportional damage and
ammunition depletion and supported fire modes. Preserve ship identity,
location, orders, appearance and unrelated equipment. Refresh derived mass,
crew, resource, power, heat, volume, combat-score and visual caches.

Proposed migration economics: free one-time conversion with no retroactive debit;
future construction/refits pay new costs; preserve construction completion
fraction if a queue's total duration changes. Include these in release notes.

Preflight detects a faction with no eligible heavy unlocked, or a converted
design exceeding power/thermal/fuel limits. These need an agreed exception or
repair policy before release. Do not grant research, delete equipment or fuel,
or save a partially converted campaign silently. Revalidate designs affected
by hull mass changes as well as compact-mount conversions; no nose-stat
migration is included.
Only mark conversion successful after the complete design/state is validated.

## Verification and balance acceptance

Verify exact hull eligibility, one-cell independent occupancy, disabled catalog
state and rejected drops, saved/refit/AI validation, and no duplicate modules or
double costs. Assert unchanged alien crew and unchanged nose stats; reconcile
the four explicit V0 bare-mass targets and other human large appearances'
rounded totals. Check utility slot indices,
geometry and compatibility with existing legacy utility handling.

Test 1/2/4-cell migration, mixed families and empty cells, damage/ammo state,
shared references, queues/refits, and save→reload→save idempotence. Check no-unlock
and power/fuel preflight explicitly. Validate A/B arithmetic as deferred research
only. Preserve existing progression tests for rail/coil tiers; no nose
consistency pass is included in this release.

In-game: place adjacent heavies, remove one, save/reopen, refit, and fire each
independently. Verify designer, preview and combat art across appearances.
Compare capital-only and capital-plus-screen fleets under missile saturation,
kinetic pressure, and armored opponents. Use equal-resource and equal-MC
comparisons separately. Measure range/armor-dependent damage, shots per second,
missile leakage, thermal downtime, survival, acceleration and strategic range.
Capital PD capability is an intended feature, not a failure criterion.

## Appearance-specific human large-hull rounding

| Hull | Art | Structural mass, t | Bare hull + base crew, t |
| --- | --- | --- | --- |
| Cruiser | 0 | 964 | 1,000 |
| Cruiser | 1 | 1,788 → 1,764 | 1,824 → 1,800 |
| Cruiser | 2 | 1,549 → 1,564 | 1,585 → 1,600 |
| Cruiser | 3 | 2,286 → 2,264 | 2,322 → 2,300 |
| Battlecruiser | 0 | 1,170 | 1,200 |
| Battlecruiser | 1 | 2,460 → 2,470 | 2,490 → 2,500 |
| Battlecruiser | 2 | 1,900 → 1,870 | 1,930 → 1,900 |
| Battlecruiser | 3 | 3,024 → 3,070 | 3,054 → 3,100 |
| Lancer | 0 | 1,958 | 2,000 |
| Lancer | 1 | 2,472 → 2,458 | 2,514 → 2,500 |
| Lancer | 2 | 3,848 → 3,858 | 3,890 → 3,900 |
| Lancer | 3 | 3,865 → 3,858 | 3,907 → 3,900 |
| Battleship | 0 | 1,558 | 1,600 |
| Battleship | 1 | 1,961 → 1,958 | 2,003 → 2,000 |
| Battleship | 2 | 1,854 → 1,858 | 1,896 → 1,900 |
| Battleship | 3 | 2,251 → 2,258 | 2,293 → 2,300 |
| Dreadnought | 0 | 2,346 → 2,343 | 2,400 |
| Dreadnought | 1 | 2,906 → 2,943 | 2,960 → 3,000 |
| Dreadnought | 2 | 2,521 → 2,543 | 2,575 → 2,600 |
| Dreadnought | 3 | 3,559 → 3,543 | 3,613 → 3,600 |
| Titan | 0 | 3,143 → 3,280 | 3,200 → 3,400 |
| Titan | 1 | 4,208 → 4,180 | 4,265 → 4,300 |
| Titan | 2 | 3,408 → 3,380 | 3,465 → 3,500 |
| Titan | 3 | 5,089 → 5,080 | 5,146 → 5,200 |

## Evidence

- [Nose audit, A/B correction targets, and actual-template reference fits](nose-weapon-scaling-audit-2026-09-25.md).
- [Merged source snapshot, hashes, both variants, and all appearance calculations](tables/capital-heavy-mount-proposal-2026-09-25.json).
- [Hull overrides](../../TIEconomyMod/ModFiles/TIShipHullTemplate.json),
  [flat appearance masses](../../TIEconomyMod/Core/ShipBalanceMath.cs),
  [default settings](../../TIEconomyMod/ModFiles/Settings.xml), and
  [module-volume accounting](../../TIEconomyMod/Patches/FuelCapacityPatches.cs).
- [Measured hull envelopes](hull-utility-slot-volume-report.md).

Installed templates are merged by dataName with current repository partial
overrides. Geometry uses the inspected native x+1/y+2 adjacency rules. Salvo
throughput uses the native averageCooldown formula. The tables are analytical
standard-catalog/default-settings results, not a live-save inventory or completed
combat test. No gameplay implementation or deployment was performed.
