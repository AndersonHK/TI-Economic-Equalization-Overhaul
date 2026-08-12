# Ship rebalance planning changelog

This is a decision log for the proposed ship rebalance. Entries here describe
the balance decisions as well as their implementation status.

## 2026-08-12

### Implemented: graphical-variant and alien drive scaling

- Mod initialization now rolls back every Harmony patch from the EEO owner if
  any patch class fails, then disables the mod and logs the initialization
  error. This avoids leaving a silently partial balance patch set active.
- Extended the drive lookup with alien status, DLC-aware resolved graphical
  appearance, and the candidate or installed drive's nozzle family. Only alien
  hulls consume the new graphical factors. Human ships retain the approved
  pre-pass hull factors for every appearance and nozzle family: Cruiser 1.30,
  Battlecruiser 1.50, Lancer 1.72, Battleship 1.75, Dreadnought 2.00, and Titan
  2.50, with smaller hulls at 1.00.
- All fourteen standard alien hulls were measured from their standalone x1
  drive resources. Alien factors range from **1.000** for Gunship, Escort, and
  the below-baseline Corvette to **7.531** for Titan and **26.216** for
  Mothership. Alien resource naming is hull-specific and ignores nozzle family,
  so the measured factor is shared by magnetic and De Laval physics. The
  Salamander has no standalone alien drive prefab and retains **1.000**.
  Invalid alien appearances, unknown hulls, and this missing Salamander
  measurement emit one-time configuration errors that include the safe
  fallback value. Intentional authored 1.00 values do not produce errors.
- Scaled thrust now reaches live `TISpaceShipState.currentThrust_N`, not only
  design-template acceleration. The ship designer refreshes its cached drive
  table when the designer re-filters modules, and both the selected-module side
  panel and hover tooltip display scaled thrust, combat thrust, required power,
  drive mass, and material cost. Exhaust velocity and efficiency remain
  unchanged.
- Extended the maintained prefab-measurement script without changing its hull
  measurement path. Alien missing-resource results are recorded explicitly, and
  the measurement method and complete alien variant table are maintained in
  the hull/drive report.
- Full deployment validation passed against TI 1.0.51 with **731 formula
  assertions**, **111 Harmony patches** in the implementation matrix, guarded
  drive-display transpiler and live-description patch validation, release
  packaging, and **32-file** enabled-mod deployment.

### Implemented in 0.9.2: conservative alien propulsion and armor allocation

- Alien drive thrust becomes **1.2 / 3.8 / 10.5 MN**, exhaust velocity becomes
  **1,200 / 2,350 / 3,000 km/s**, and installed drive efficiency remains
  unchanged at **95 / 97 / 98%**.
- Alien reactor maximum output receives a flat **+400%** increase to **5,000 /
  32,000 / 107,550 GW**. Specific mass is halved to **0.50 / 0.175 / 0.025
  t/GW**, while efficiency becomes **99.5 / 99.8 / 99.95%**.
- The alien designer's armor-density numerator changes from **3,500** to
  **10,000 kg/m3**, requesting about **2.86x** its prior armor points for the
  same material. All existing role delta-v targets remain unchanged.
- Matching-tier reactor caps exceed six-drive electrical demand by roughly
  **9.9% / 15.9% / 11.5%**. Strict reactor selection and predefined-loadout
  normalization remain separate follow-up work.
- Full deployment validation passed against TI 1.0.51: **699 formula
  assertions**, **107 Harmony patches** in the implementation matrix, guarded
  `DesignAlienShip` IL validation, release packaging, and **32-file**
  enabled-mod deployment.

### Implemented in 0.9.1: alien magnetic progression

- Added the
  [alien magnetic, propulsion, and armor-design proposal](alien-weapons-propulsion-and-armor-proposal.md),
  based on installed templates layered with the mod's current magnetic
  overrides.
- Alien tier 1 receives modest velocity increases where its lead over human
  Mk1 is only marginal. Alien tier 2 receives the missing velocity and range
  leads over human Mk3 plus **75% efficiency**. Alien tier 3 receives monotonic
  range corrections plus **85% efficiency**; its already extreme projectile
  masses and velocities are retained.
- The half-nose Alien Mini Light Mag Cannon follows the tier-1 light-cannon
  velocity at **6.0 km/s** while retaining its existing range and efficiency.
- Release validation locks all **22 alien magnetic rows** and proves strict
  tier-1-over-human-Mk1 and tier-2-over-human-Mk3 dominance for projectile
  mass, damaging mass/durability, velocity, range, and efficiency.
- Full deployment validation passed against TI 1.0.51: **699 formula
  assertions**, 106 Harmony patches in the implementation matrix, guarded
  target-IL checks, ship-rebalance validation, release packaging, and
  **31-file** enabled-mod deployment.

### Proposed: matched alien propulsion progression and armor headroom

- The proposed drive ladder is **1.2 / 3.8 / 10.5 MN**, **1,200 / 3,600 /
  10,800 km/s**, and **98 / 99 / 99.9%** efficiency. The matched reactor caps
  are **5,000 / 42,000 / 350,000 GW**, with **0.10 / 0.014 / 0.0018 t/GW**
  specific mass and **99.5 / 99.8 / 99.95%** efficiency.
- Better templates do not by themselves repair reactor undersizing.
  Predefined designs retain explicit reactor assignments, while dynamic power-
  plant selection does not explicitly require output cap to meet design load.
  Both paths require pairing validation.
- Stronger propulsion creates physical armor headroom, but the current alien
  designer may exit before spending it and may stop filling all armor when one
  facing caps. A future implementation may correct that allocator while
  preserving every existing performance floor.

### Explicitly deferred: lower alien AI delta-v targets

- Retain the existing **800 / 900 / 600 / 200 / 300 km/s** role-group targets.
- The previously explored half-target concept is not part of the active
  proposal. Any reduction requires a later update after propulsion-first armor,
  generated-design, combat, and campaign evidence.

## 2026-08-10

### Implemented: remaining human-hull crew and rounded empty masses

- **One base crew per weapon or utility slot:** Cruiser, Battlecruiser, Lancer,
  Battleship, Dreadnought, and Titan now carry **12 / 10 / 14 / 14 / 18 / 19**
  base crew respectively. Together with the previously changed six hulls, all
  twelve standard human combat hulls now follow the same balance rule.
- **Three-tonne support allowance retained:** hull masses become **964 / 1,170 /
  1,958 / 1,558 / 2,346 / 3,143 t**. Adding three tonnes per base crew billet
  produces rounded hull-plus-crew empty masses of **1,000 / 1,200 / 2,000 /
  1,600 / 2,400 / 3,200 t**.
- **Geometry unchanged for the new six:** this pass does not override their
  length, diameter, or stored volume. The earlier Gunship-through-Destroyer
  model-informed geometry remains in force.

### Implemented: provisional hull-specific drive capacity

- **Human-hull multipliers:** Cruiser **1.30**, Battlecruiser **1.50**, Lancer
  **1.72**, Battleship **1.75**, Dreadnought **2.00**, and Titan **2.50**.
  Gunship through Destroyer and all alien hulls remain at **1.00**.
- **Constant exhaust velocity:** ship thrust, physical mass flow, powered-drive
  electrical demand, drive hardware mass, and drive material cost scale by the
  hull multiplier. Exhaust velocity and therefore delta-v per unit propellant
  remain unchanged. The game has no hull-level burn-duration/fuel-flow state,
  so mass flow is the physical `thrust / exhaust velocity` consequence rather
  than a separate runtime consumption variable.
- **Dependent burdens retained:** higher drive power feeds the existing reactor,
  waste-heat, and radiator calculations. Drive construction and refit costs
  include the larger hardware, and existing reactor maximum-output
  compatibility checks use the scaled drive demand.
- **Appearance-sensitive follow-up audited:** the selected appearance index,
  hull utility capacity, base and complete crew, rounded empty hull mass, and
  runtime cylinder volume are all reachable from the existing ship and hull
  templates without a save-format change. A future implementation should use
  stable hull properties for the class factor, a bounded normalized appearance
  modifier, and must not use free utility slots or patched complete dry mass.
- **Reactor caps deferred:** no hull-size reactor mass/output cap and no reactor
  template change are included in this pass.
- **Configuration:** `shipBalance.hullDriveScalingEnabled` independently returns
  the new runtime scaling to vanilla while leaving the other ship-balance
  features active.

### Documented: hull density, naval references, hardpoints, and drive assets

- Added the consolidated
  [human hull and drive-scaling report](human-hull-slots-and-drive-scaling.md),
  including per-hull and per-tier tonnes/slot, volume/slot, tonnes/crew, and
  volume/crew tables; Arleigh Burke, Ticonderoga, Zumwalt, Type 45, and Iowa-
  class context; hardpoint and multi-slot feasibility; the drive-asset method;
  nozzle-area ratios; and the reference Meteor x6 thrust-to-mass table.
- Moved the maintained UnityPy measurement program to
  `scripts/ship-balance/measure_ship_prefabs.py`. Its original hull measurement
  function remains independent of the added drive-resource and individual-
  nozzle functions.
- Full deployment validation passed against TI 1.0.51: **667 formula
  assertions**, 101 Harmony patches in the implementation matrix, guarded
  target-IL checks, ship-rebalance validation, package build, and enabled-mod
  deployment.

## 2026-08-04

### Implemented: complete chemical ammunition and nose-autocannon cadence

- **Large-caliber projectile progression:** retain the settled **40 / 90 /
  180 kg** damaging masses for the 6-, 8-, and 10-inch guns and implement the
  planned **320 kg** 12-inch projectile. The sequence remains within 5.1% of
  cubic caliber scaling from the 6-inch reference.
- **Complete ammunition mass:** increase `ammoMass_kg` to **90 / 200 / 400 /
  640 kg** for the 6-, 8-, 10-, and 12-inch guns. These values cover the
  projectile, an energy-scaled propellant charge for the shared 1.4 km/s
  muzzle velocity, and cartridge or bag-handling allowance. They affect loaded
  weapon mass and ammunition cost, not empty weapon mass or impact damage.
- **35mm nose autocannon:** change complete/damaging masses from **10 / 5.5 kg
  to 8.8 / 2.6 kg**, retain its four-shot salvo, and change intra/inter-salvo
  timing from **0.5 / 4.0 s to 0.4 / 1.75 s**. Its cycle becomes 2.95 seconds,
  effective cadence becomes 81.36 rpm, and sustained kinetic output becomes
  7.051 MJ/s (**-11.9%** versus vanilla).
- **40mm nose autocannon:** change complete/damaging masses from **10 / 6 kg
  to 8.8 / 2.8 kg**, retain its four-shot salvo, and change intra/inter-salvo
  timing from **0.5 / 4.0 s to 0.4 / 1.75 s**. Its cycle becomes 2.95 seconds,
  effective cadence becomes 81.36 rpm, and sustained kinetic output becomes
  12.833 MJ/s (**-13.0%** versus vanilla).
- **Unchanged scope:** salvo sizes, muzzle velocities, ranges, empty weapon
  masses, crew, and electrical values are unchanged. The existing hull 30mm
  and 40mm values remain as implemented on 2026-08-02.

## 2026-08-02

### Implemented: revised 30mm and 40mm CIWS cadence and mass

- **30mm:** retain the 10-shot salvo, **5.5 / 1.75 kg** complete/damaging
  projectile masses, 1.35 km/s velocity, and **0.25 s** intra-salvo spacing;
  round the inter-salvo reload to **1.0 s**. The 3.25-second total cycle yields
  **184.62 effective rpm** and **4.907 MJ/s** sustained kinetic output.
- **40mm ETC:** retain the six-shot salvo, 2.6 km/s velocity, and settled
  8.7 MJ/shot electrical-work value. Reduce complete/damaging projectile mass
  to **8.8 / 2.8 kg**, exactly 1.6 times the 30mm values; use **0.5 s**
  intra-salvo spacing and **1.75 s** inter-salvo reload. The 4.25-second total
  cycle yields **84.71 effective rpm** and **13.361 MJ/s** sustained output.
- **Role comparison:** the revised 40mm retains 53.4% more sustained kinetic
  output than the one-hull 6-inch cannon, but only 24.1% of its impact per shot
  while requiring 13.647 MW average electrical input. Salvo sizes, ranges,
  projectile velocities, weapon empty masses, and the 30mm power value remain
  unchanged.

### Implemented: coil and alien-mag projectile housekeeping

- **Projectile mass, not weapon mass:** all 27 human coil weapons and all 22
  alien magnetic weapons receive approximately **25% heavier projectiles**,
  rounded to whole kilograms. Complete mass and damaging/durability mass move
  together while preserving each projectile's vanilla composition as closely
  as whole-kilogram rounding permits. Gun/mount empty mass is unchanged.
- **Approximately 20% shorter total cycles:** every scoped weapon retains its
  vanilla salvo size and intra-salvo delay. Only the inter-salvo reload is
  reduced, producing actual total-cycle changes from **-18.8% to -20.8%** after
  whole-second rounding.
- **Alien velocity retained:** the previously implemented alien muzzle-velocity
  increase remains in force and combines with the mass and cadence changes.
- **Rails unchanged:** the staged human rail values are not altered by this
  housekeeping slice; the other 12 human rail rows remain vanilla.
- **Coverage:** the magnetic override now contains exactly 9 staged human rails,
  27 human coils, and 22 alien mags. Validation requires exact fields and values,
  preventing accidental salvo/intra-salvo overrides or damaging mass greater
  than complete projectile mass.

## 2026-08-01

### Implemented staged projectile and magnetic-weapon test slice

- **Chemical projectiles:** use **40 / 90 / 180 kg** damaging masses for the
  6-, 8-, and 10-inch guns while retaining their vanilla ammunition mass,
  salvo size, intra-salvo delay, and reload. Use **1.75 kg at 180 rpm** for the
  30mm and **3 kg at 100 rpm** for the hull 40mm; complete rounds remain at
  their vanilla mass.
- **Human rails:** change only the light rail battery, rail battery, and light
  rail cannon Mk1-Mk3 projectile masses and reloads. Complete projectile mass
  is **14 / 30 / 37.5 kg** by mount family; reloads are **8 / 6 / 4 s**,
  **12 / 9 / 6 s**, and **16 / 12 / 8 s**, respectively. Damaging mass retains
  each mark's vanilla damaging-to-complete-mass ratio. Salvo size and
  intra-salvo delay remain vanilla, so these railguns remain single-shot for
  this test.
- **Alien mags:** apply only the settled muzzle-velocity increase to all 22
  alien magnetic weapons. Their projectile mass, salvo, delay, reload, range,
  power, and efficiency remain vanilla.
- **Explicit exclusions:** human coils, all other human rail families, chemical
  ammunition mass, and the proposed rail salvos remain deferred for campaign
  testing.

### Field-test observation: staged human rails

- **Current rails are already decisive without salvos:** ships using the staged
  light rail battery, rail battery, and light rail cannon values destroyed their
  opposing ships easily. Their longer targeting range and higher projectile
  velocity were tactically decisive even though all three families retained
  vanilla one-shot salvos.
- **Salvo proposal rejected:** the planned 3 / 4 / 5-shot rail salvos are no
  longer required and should not be implemented. The next conservative test
  should retain the staged projectile masses and move reloads back toward
  vanilla before considering any other rail or coil change.

### Field-test observation: direct-fire saturation fallback

- **Long-range rails can saturate the entire enemy fleet before impact:** the
  direct-fire commitment system distributed rail projectiles across every
  hostile ship while the hostile fleet was still outside its own engagement
  range. Once every target crossed the expected-overkill threshold, automatic
  acquisition rejected all of them and the rail ships stopped firing. The
  in-flight projectiles did not actually destroy every ship, leaving surviving
  targets alive without continued fire.
- **Saturation must be a preference, not a firing prohibition:** automatic
  direct-fire targeting should prefer an eligible unsaturated target. If no
  eligible unsaturated target exists, it should fall back to an eligible
  saturated target and continue firing. Saturation alone must never reduce
  expected damage to zero or leave a weapon without a target when saturated
  enemies remain available.
- **Scope remains direct fire only:** this fallback is intended for plentiful
  gun, rail, coil, and plasma projectiles. Existing missile and torpedo
  saturation behavior remains unchanged.

### Implemented: direct-fire saturation as a priority malus

- **Lexicographic target priority:** saturation is now the first automatic
  direct-fire priority key rather than a binary eligibility gate. While any
  eligible unsaturated enemy ship exists, saturated ships are skipped. When
  every eligible enemy ship is saturated, all remain candidates and vanilla
  Weakest, Closest, or Strongest ordering selects the highest-priority one.
- **Continuous fallback fire:** expected direct-fire damage is suppressed on a
  saturated target only while an eligible unsaturated ship exists. If all
  eligible ships are saturated, guns, rails, coils, and plasma continue firing
  at the selected saturated target until no eligible targets remain.
- **Intent and missile isolation:** deliberate player primary targets still
  bypass automatic suppression. Missile weapons, missile-boat acquisition,
  missile saturation, and torpedo salvo behavior remain unchanged.
- **Coverage:** formula tests exercise mixed and all-saturated fleets; guarded
  IL validation requires both target acquisition and fire suppression to query
  the shared unsaturated-target scan while retaining the three vanilla target
  selection loops.

### Implemented: magnetic projectile geometry and durability coupling

- **All magnetic families covered:** every human railgun, human coilgun, siege
  coiler, and alien magnetic weapon now receives a physical collider diameter
  and cross-sectional area through the same generic geometry path used by
  chemical projectiles.
- **Mass-derived default:** an explicit `projectileDiameter_mm` template value
  remains authoritative. Without one, magnetic diameter is derived from
  `ammoMass_kg` as a 10:1 length-to-diameter tungsten-equivalent kinetic body
  at **19,300 kg/m3**. This produces a monotonic cube-root relationship, so
  future complete-projectile-mass edits automatically resize both the collider
  and physical cross section without duplicating values across template rows.
- **Durability remains natively coupled:** Terra Invicta already calculates a
  magnetic projectile's remaining effective mass as `warheadMass_kg` minus
  accumulated mass damage. Increasing damaging projectile mass therefore
  increases magnetic projectile durability by the same percentage. The
  comparison table now exposes both values and deltas so future mass proposals
  can keep damage and durability synchronized.
- **Coverage:** formula tests guard zero handling, the 10 kg reference diameter,
  and monotonic mass scaling. Runtime IL validation requires the explicit-data
  override, mass-derived magnetic fallback, and the game's
  `warheadMass_kg - massDamage_kg` durability relationship.

### Implemented in 0.8.4: skirmish roster and gun-lookup performance

- **Shared option catalogs:** main-menu skirmish rows now build localized ship
  options once per controller, language, import set, and alien-eligibility
  variant. Reinitialized rows retain their private option lists when the
  catalog is unchanged instead of rebuilding every design entry.
- **Vanilla behavior retained:** displayed combat scores, alien/import colors,
  newly imported design selection, add-row notification behavior, tooltips,
  damage images, and fleet-score calculation stay on their vanilla paths.
- **Safe invalidation:** changed ship/import counts or identities, language,
  and the imported-design setter invalidate the catalog. Disabling Ship Balance
  returns `PopulateShipDropdown` to the complete vanilla implementation.
- **Allocation-free gun fast path:** generic gun power data is resolved against
  every loaded `TIGunTemplate` during hydration and stored by reference.
  Ordinary `selfPowered`, energy, and heat getter calls no longer sort scenario
  tags or construct string keys. Unexpected dynamic templates retain the
  original scenario-aware lookup fallback.
- **Coverage:** deterministic tests require 10,000 stable row-cache accesses to
  invoke the option builder once and 100,000 identity lookups to retain the
  hydrated value. Runtime IL validation guards the vanilla refresh call chain,
  catalog-builder isolation, import invalidation, and both Harmony targets.

### Field-test observations after 0.8.3

- **Automatic gun allocation succeeded:** in repeated unattended skirmishes,
  ten 10-inch gunships now defeat ten 6-inch escorts reliably. The result
  reverses the earlier artificial advantage created by projectile collisions
  and whole-fleet target saturation.
- **Early combat triangle:** current testing supports the intended broad
  relationship `missiles > guns > point defense > missiles`. Missiles remain
  extremely effective, 10-inch guns are now dominant against competing gun
  ships, and point defense retains its anti-missile role.
- **Mk1 magnetic weapons remain weak:** none of the projectile-collision or
  automatic-target-allocation changes made the human Mk1 railgun family
  materially competitive. Its balance remains an open, separate problem.
- **Open performance regression:** adding ships in the main-menu skirmish
  roster now produces a large delay that grows with roster size. Static
  inspection confirms that every add performs a full
  `StartMenuController.PopulateSkirmishDropdowns` rebuild; every existing row
  then repopulates the complete ship-design dropdown. See
  [the skirmish roster performance investigation](skirmish-roster-performance-investigation.md).

### Implemented in 0.8.3: direct-fire commitment targeting

- **Automatic fire allocation:** live ballistic projectiles now reserve their
  vanilla expected direct damage against their intended ship. Ordinary Offense
  acquisition and nonmissile combat AI skip a target once aggregate committed
  damage exceeds the same structural-integrity and total-armor threshold used
  by vanilla missile saturation.
- **Projectile lifecycle:** commitments are registered only after an actual
  shot, removed on destruction, collision, impact, or battle cleanup, and
  pruned when a projectile is no longer moving toward its intended target.
  Per-target pruning is limited to once per rendered frame.
- **Player intent:** player-controlled deliberate primary targets remain
  authoritative. Focus, Bracket, Defense, Guardian, Idle, and Salvo behavior is
  not redirected by the automatic-fire gate.
- **Missile isolation:** missile target selection, saturation estimation,
  point-defense discounting, 15-second retarget timing, and quarter-ammunition
  salvo behavior are unchanged. Missile-carrier AI retains its vanilla target
  eligibility path.
- **Generic ballistic support:** the shared path covers conventional guns,
  magnetic weapons, and plasma without caliber or template identifiers. No
  projectile mass, velocity, cadence, damage, ammunition, or weapon power value
  changes in this release.

### Implemented in 0.8.2: TI 1.0.51 compatibility and gun-table load fix

- **Late-save initialization:** powered 0.8.1 gun rows conditionally gained an
  Energy Usage cell while the 35mm, nose 40mm, and 12-inch rows remained
  self-powered and one cell shorter. TI's ship-module table assumes every
  visible row has the same cell count, so late saves with a mixed unlock set
  failed in `ShipModuleTable.ResizeColumns` with a repeating
  `ArgumentOutOfRangeException`.
- **UI compatibility:** retain the intended five powered gun families and their
  balance values. During module-row construction only, conventional guns that
  still consume zero energy now participate in the Energy Usage column and
  display their real zero value. Gameplay power, battery, heat, and self-powered
  behavior are unchanged.
- **Retarget:** build and validate against the installed Terra Invicta 1.0.51
  assemblies. All guarded gameplay/UI IL anchors and the new module-row
  transpiler match the installed binary.
- **Physical projectile geometry:** add generic `projectileDiameter_mm` data
  for all eight conventional-gun templates: **30, 35, 40, 40, 152.4, 203.2,
  254, and 304.8 mm**. A fired projectile's collider now uses that diameter at
  the same cinematic scale as ship models instead of retaining the shared
  `BulletAutocannon` or `BulletGun` collider dimensions. The same data supplies
  physical cross-sectional area to beam damage and targeting calculations.
- **Collision sweep:** reduce the ballistic forward-ray sweep from **120% to
  100%** of movement during the update. The guarded transpiler requires exactly
  one matching TI 1.0.51 constant.
- **Projectile durability:** conventional-gun projectiles now accumulate the
  game's already-randomized direct plus chipping damage at **100 kg of effective
  projectile mass per damage point**. Destruction occurs when accumulated mass
  damage reaches `warheadMass_kg`. A head-on 6-inch hit comfortably neutralizes
  a 10-inch round, while a 30mm hit does not do so by itself. The impacting
  projectile still destroys itself through the existing impact path; no
  post-impact trajectory or deflection state is simulated.
- **Future magnetic support at the time:** projectile-diameter hydration was
  shared between `TIGunTemplate` and `TIMagneticGunTemplate`, but no magnetic
  geometry was activated in 0.8.2. The mass-derived implementation recorded
  above now supersedes that deferral.

### Implemented in 0.8.1: powered guns and coherent thermal accounting

- **Gun power only:** activate the existing powered-weapon path for the 30mm,
  40mm, 6-inch, 8-inch, and 10-inch guns through generic `powerUse_MJ` data.
  Retain every vanilla 1.0.49 mass, projectile, ammunition, velocity, range,
  damage, cooldown, and salvo value. At vanilla cadence, use **0.085, 8.70,
  0.675, 1.40625, and 2.20 MJ useful work per rendered shot**, respectively.
  The ordinary chemical guns retain their inherited **100%** efficiency until
  loader-loss values are settled; the 40mm uses **90%**, drawing **9.6667 MJ**
  and producing **0.9667 MJ local heat** per rendered shot.
- **Generic hydration:** retain the new field in a load-ordered runtime registry
  sourced from active `TIGunTemplate` mod JSON. The C# behavior contains no gun
  identifiers, supports scenario tags, observes full-template replacement, and
  works whether Unity Mod Manager starts before or after template initialization.
- **Reactor output and heat:** auxiliary generation now credits the ship power
  pool with net electrical output rather than pre-efficiency reactor input.
  Rejected plant heat is electrical output times
  `(1 - plant efficiency) / plant efficiency` and is applied once when power is
  generated. The redundant continuous systems-heat application is suppressed.
- **Weapon heat and radiators:** radiator design load now adds each powered
  weapon's own `HeatGeneration_GJ` at the same cooldown or intra-salvo interval
  used by vanilla generator sizing. This applies uniformly to beams, magnetic
  guns, plasma weapons, and newly powered conventional guns.
- **Heat gate:** the retracted/destroyed-radiator pre-fire check now reserves
  only the module heat that `FireWeapon` will actually apply. It no longer treats
  total reactor input energy as instantaneous weapon heat.
- **Save compatibility:** no custom value is serialized. Active gun power data
  is rebuilt before save selection; the game's existing post-load power recache
  consumes it. Loaded ship mass is reconciled to recalculated template dry mass
  plus saved propellant and propulsion values are marked dirty.

### Settled conservative gun projectiles and revised magnetic cadence

**Implementation status:** the chemical-projectile subset is implemented by the
staged test slice above. The broader magnetic cadence proposal recorded below
remains historical planning and is not the runtime configuration.

- **6-, 8-, and 10-inch projectile mass:** settle the conservative ends of the
  evidence-led bands at **40 kg, 90 kg, and 180 kg**, respectively. At the
  retained 1.4 km/s muzzle velocity these produce **39.2, 88.2, and 176.4 MJ**
  per rendered projectile. The 10-inch remains the largest relative improvement
  over vanilla.
- **Conventional naval-gun salvos retained:** keep the vanilla **4, 4, and 3
  shots per salvo**, **2, 2.5, and 3 s** intra-salvo spacing, and **12, 15, and
  16 s** between-salvo cooldowns for the 6-, 8-, and 10-inch guns. A single
  barrel can represent a small ready-service ammunition group followed by a
  longer magazine-handling cycle; retaining it also preserves the useful visual
  rhythm of naval-gun fire. The earlier one-shot candidate is rejected and is
  not to be implemented.
- **30mm Autocannon cadence:** retain the settled **1.75 kg** damaging packet
  and increase the target to **180 rendered rounds per minute** so that the
  projectile-CIWS pass does not inadvertently strengthen already-powerful
  early missiles. With ten-shot salvos and 0.25 s intra-salvo spacing, a
  cooldown of approximately **1.0833 s** produces exactly 180 rpm.
- **40mm Autocannon cadence:** retain the settled **3 kg** damaging packet and
  increase the target to **100 rendered rounds per minute**. With six-shot
  salvos and 0.375 s intra-salvo spacing, a cooldown of **1.725 s** produces
  exactly 100 rpm.
- **Human Mk1-Mk2 rail and coil cadence:** reduce both `cooldown_s` and
  `intraSalvoCooldown_s` by about **66%**, rounded to whole seconds. Rail-battery
  cooldowns become **30 / 20 -> 10 / 7 s** for Mk1/Mk2; rail-cannon cooldowns
  become **45 / 30 -> 15 / 10 s**.
- **Human Mk3 rail and coil cadence:** start from a **40%** reduction, then
  tighten only values needed to preserve each vanilla mark-to-mark improvement
  by at least one second. Rail batteries become **10 -> 6 s** and rail cannons
  **15 -> 9 s**. Coil-battery cooldowns become **13 / 10 / 9 s** across Mk1-3;
  ordinary coil-cannon and siege-coiler cooldowns become **16 / 12 / 11 s**.
- **Intra-salvo detail:** ordinary coil batteries use **3 / 3 / 6 s** and
  ordinary coil cannons **4 / 4 / 7 s** across Mk1-3. Vanilla does not improve
  these spacings between marks, so the Mk3 40% target is retained. Siege
  coilers do improve in vanilla and therefore use **12 / 8 / 7 s**, preserving
  at least a one-second improvement at each mark.
- **Magnetic scope boundary:** this cadence pass changes no non-cadence proposal
  field and no alien magnetic-weapon row. The proposal CSV contains typed
  numeric cells without stray leading apostrophes.
- **Conventional-gun electrical-load targets:** plan average auxiliary loads at
  the upper ends of the engineering bands: **0.100 MW** for the 30mm,
  **0.150 MW** for the 6-inch, **0.250 MW** for the 8-inch, and **0.300 MW** for
  the 10-inch. These loads represent feeds, autoloaders, mount machinery,
  controls, and cooling rather than chemical muzzle energy.
- **40mm ETC electrical convention:** represent the 3 kg packet's kinetic-energy
  increment from a 1.0 km/s chemical reference to 2.6 km/s, plus its auxiliary
  load, as **8.70 MJ useful work per rendered projectile**. Set the inherited
  powered-weapon efficiency to **90%**. At 100 rpm this means **9.6667 MJ
  electrical input per projectile**, **16.111 MW average electrical input**, and
  **0.9667 MJ local weapon heat per projectile** (**1.611 MW average**). This is
  a futuristic balance abstraction for controlled propellant combustion, not
  an experimentally established energy budget.
- **Gun-power implementation status:** superseded by the 0.8.1 implementation
  above. The power fields and shared thermal fixes are implemented; the staged
  chemical projectile and cadence subset is now active, while all remaining
  ammunition, mass, and cadence proposals stay deferred.

## 2026-07-31

### Planned conventional-gun mass and projectile pass

**Implementation status:** planning decision only; not implemented. No weapon
template values are changed by this entry. Exact values not stated below remain
subject to the railgun-progression and ammunition-endurance reviews.

- **Weapon empty mass:** bring the conventional-gun base-mount masses toward
  plausible empty automated-mount values. Keep empty mount mass distinct from
  ammunition mass when setting and reporting the targets. The evidence review
  does not support forcing the already-light 6-inch and 8-inch base mounts
  below their present 25 t and 50 t merely to make every caliber lighter; their
  final values remain open pending a consistent mounting-and-recoil model.
- **10-inch Cannon empty-system mass:** use approximately **145 t** as the
  evidence-led empty weapon-system target. This target refers to the complete
  mount, autoloader, recoil structure, and associated machinery before
  ammunition; it is not a loaded-mass target. Loaded mass must be calculated by
  adding the separately revised magazine and complete-round mass. This
  supersedes the earlier **110 t empty / 137.6 t loaded** working example in
  `low-tech-rebalance-slice.md`.
- **6-, 8-, and 10-inch projectile hierarchy:** increase effective damaging
  projectile mass toward real full-caliber comparisons instead of retaining
  the compressed vanilla values of **22.5, 50, and 90 kg**. Use approximate
  planning bands of **40-45 kg, 90-115 kg, and 180-230 kg**, respectively. Give
  the 10-inch Cannon the largest relative benefit so that it becomes the
  decisive single-hit weapon of the chemical-gun family. Final damage, rate of
  fire, magazine, and loaded-mass values remain to be reconciled together.
- **30mm Autocannon projectile and cadence:** halve effective damaging
  projectile mass from **3.5 to 1.75 kg** and double its rate of fire. With the
  current ten-shot salvo structure, the direct cadence translation is
  **0.5 to 0.25 s** intra-salvo spacing and **4 to 2 s** cooldown. This retains
  approximately the same sustained kinetic throughput while halving damage per
  rendered projectile and doubling the number of armor interactions.
- **40mm Autocannon projectile and cadence:** halve effective damaging
  projectile mass from **6 to 3 kg** and double its rate of fire. With the
  current six-shot salvo structure, the direct cadence translation is
  **0.75 to 0.375 s** intra-salvo spacing and **4 to 2 s** cooldown. This also
  retains approximately the same sustained kinetic throughput while making the
  abstraction a smaller packet of physical 40mm rounds.
- **Autocannon ammunition accounting:** reassess complete-round mass and
  magazine depth separately from damaging projectile mass. Halving
  `warheadMass_kg` does not by itself settle `ammoMass_kg`, and the present
  3,000- and 2,000-shot magazines should not be carried forward automatically
  when the rendered-projectile packet size and cadence change.
- **Gun electrical demand:** conventional chemical guns should not be charged
  for muzzle kinetic energy as reactor output. A later code-supported pass may
  assign autoloader, mount-drive, and cooling loads; the 40mm
  electrothermal-chemical weapon should additionally pay for its electrically
  assisted velocity gain and pulse-power losses.

## 2026-07-29

### Settled for the first low-tech slice

**Implementation status:** applied in Economic Equalization Overhaul 0.7.4.
The four deferred starting engines remain unchanged. Items under “Still under
research” remain unimplemented; in particular, this release does not adopt a
hull-material recipe or change the vanilla crew construction-resource package.

- **Apex, Meteor, Neutron, and Venture:** defer all changes. The four starting
  engines remain at their current values until the wider propulsion progression
  is reviewed.
- **Water Heat Sink and Heavy Water Heat Sink:** set module crew to **0** in the
  eventual rebalance. Monitoring and maintenance belong to the ship's shared
  engineering crew.
- **10-inch Cannon:** reduce module crew from **4 to 3** in the eventual
  rebalance. The deliberately conservative abstraction is one commander, one
  shooter, and one loader.
- **Point-defense guns:** use **0 dedicated module crew** for both projectile
  and laser point-defense mounts. In this narrow slice, apply and table that
  decision only for the **30mm Autocannon: 1 → 0 crew**. Reload, maintenance,
  and supervision remain ship-level burdens.
- **Solid Core Fission Reactors I–V:** reduce efficiency by five percentage
  points in the eventual rebalance:
  - I: **75% → 70%**
  - II: **77.5% → 72.5%**
  - III: **80% → 75%**
  - IV: **82.5% → 77.5%**
  - V: **85% → 80%**
- **Compact Solid Core Fission Reactors I–V** (template identifiers
  `SolidCoreFissionReactorVI–X`): apply the same five-percentage-point
  reduction:
  - Compact I: **77.5% → 72.5%**
  - Compact II: **80% → 75%**
  - Compact III: **82.5% → 77.5%**
  - Compact IV: **85% → 80%**
  - Compact V: **87.5% → 82.5%**
- **Fuel Cells I–III:** set efficiencies to **63%, 65%, and 67%**,
  respectively.
- **Fuel Cells I–III:** set specific mass to **2.8, 1.8, and 0.48 kg/kW**,
  respectively. In the template's `specificPower_tGW` field these are
  **2,800, 1,800, and 480 t/GW**.
- **Crew support mass:** reduce the global allowance from **4 t to 3 t per
  crew member**. This remains a bundled abstraction rather than consumables
  alone.
- **Open-cycle drive cooling:** an open-cycle drive must retain a nonzero
  radiator burden; the vanilla 100% drive-heat exemption is not acceptable.
  NERVA component data support a value below 5%. The current research-led
  implementation draft is **1% of the drive-associated heat that the corrected
  closed-cycle formula would otherwise send to radiators**.
- **Gunship:** adopt **55 m length × 15 m diameter**, giving a cylindrical
  planning volume of **9,719 m³**. Set the hull to **171 t**; three crew at
  the settled allowance produce **180 t empty mass**.
- **Escort:** adopt **62 m length × 15 m diameter**, giving a cylindrical
  planning volume of **10,956 m³**. Set the hull to **338 t**; four crew
  produce **350 t empty mass**.
- **Corvette:** adopt **65 m length × 17 m diameter**, **14,754 m³** planning
  volume, **5 crew**, and **385 t hull mass**, producing **400 t empty mass**.
- **Frigate:** adopt **100 m length × 18 m diameter**, **25,447 m³** planning
  volume, **8 crew**, and **576 t hull mass**, producing **600 t empty mass**.
- **Monitor:** adopt **100 m length × 17 m diameter**, **22,698 m³** planning
  volume, **7 crew**, and **679 t hull mass**, producing **700 t empty mass**.
- **Destroyer:** adopt **100 m length × 23 m diameter**, **41,548 m³** planning
  volume, **9 crew**, and **873 t hull mass**, producing **900 t empty mass**.
- **Hull volume data:** use the calculated cylindrical volumes for the planning
  `volume` values as well as the runtime geometry, while noting that the
  installed compiled class currently ignores the JSON `volume` key.

### Settled for the second slice

**Implementation status:** incorporated into Economic Equalization Overhaul
0.7.5; the later reactor refinement below supersedes these interim efficiency
values.

- **Molten reactors I–V:** reduce efficiency by five percentage points. The
  five installed TI 1.0.49 modules are split between the Molten Salt and Molten
  Core families, so record them by their exact template identities:
  - Molten Salt Fission Reactor I (`MoltenSaltFissionReactorI`):
    **92% → 87%**
  - Molten Salt Fission Reactor II (`MoltenSaltFissionReactorII`):
    **93% → 88%**
  - Molten Core Fission Reactor I (`MoltenCoreFissionReactorI`):
    **85% → 80%**
  - Molten Core Fission Reactor II (`MoltenCoreFissionReactorII`):
    **88% → 83%**
  - Molten Core Fission Reactor III (`MoltenCoreFissionReactorIII`):
    **90% → 85%**
- **40mm Autocannon** (`40mmAutocannon`): reduce module crew from **1 to 0**.
  Reloading, maintenance, and engagement supervision remain shared ship-level
  duties.
- **Point Defense Laser Turret** (`PointDefenseLaserTurret`): reduce module
  crew from **2 to 0**. This is the first laser point-defense mount and follows
  the previously settled rule that point-defense fire-control loops do not
  receive dedicated operators.

### Settled for the third slice

**Implementation status:** incorporated into Economic Equalization Overhaul
0.7.5, except for the explicitly deferred alien-fleet work. The weapon-crew
values are current; the later reactor refinement below supersedes these
interim power-plant efficiencies.

- **Fuel Cells I–III:** reduce the first-slice efficiencies by another five
  percentage points:
  - Fuel Cell I: **63% → 58%**
  - Fuel Cell II: **65% → 60%**
  - Fuel Cell III: **67% → 62%**
- **Solid Core Fission Reactors I–V:** reduce the first-slice efficiencies by
  another five percentage points:
  - I: **70% → 65%**
  - II: **72.5% → 67.5%**
  - III: **75% → 70%**
  - IV: **77.5% → 72.5%**
  - V: **80% → 75%**
- **Compact Solid Core Fission Reactors I–V** (template identifiers
  `SolidCoreFissionReactorVI–X`): reduce the first-slice efficiencies by
  another five percentage points:
  - Compact I: **72.5% → 67.5%**
  - Compact II: **75% → 70%**
  - Compact III: **77.5% → 72.5%**
  - Compact IV: **80% → 75%**
  - Compact V: **82.5% → 77.5%**
- **Molten reactors:** reduce the second-slice efficiencies by another five
  percentage points:
  - Molten Salt Fission Reactor I: **87% → 82%**
  - Molten Salt Fission Reactor II: **88% → 83%**
  - Molten Core Fission Reactor I: **80% → 75%**
  - Molten Core Fission Reactor II: **83% → 78%**
  - Molten Core Fission Reactor III: **85% → 80%**
- **6-inch Gun Battery** (`6-inchCannon`): reduce crew from **3 to 2**.
- **8-inch Gun Battery** (`8-inchCannon`): reduce crew from **4 to 2**.
- **Light Railgun Batteries Mk1–3** (`LightRailgunBatteryMk1–3`, one hull
  hardpoint): reduce crew from **3 to 2**.
- **Railgun Batteries Mk1–3** (`RailgunBatteryMk1–3`, two hull hardpoints):
  reduce crew from **4 to 2**.
- **Light Rail Cannons Mk1–3** (`LightRailCannonMk1–3`, the one-nose
  successors to the 10-inch Cannon): reduce crew from **4 to 3**.
- **Alien fleet rebalance:** defer numeric and loadout changes to a future
  slice. The working hypothesis is that early missile saturation exploits weak
  alien laser point defense, while later fleets are held back by old magnetic
  weapons and invalid or weak reactor/drive pairings. The future slice should
  test stronger laser point-defense behavior, proper Advanced/Gen3 magnetic
  weapons, adequate power plants, and revised module placement together rather
  than applying a uniform faction-wide stat buff.

Supporting analyses are archived by topic:

- [reactor thermodynamics and fuel inventory](details/reactors/thermodynamic-and-fuel-limits.md);
- [reactor structural scaling and output caps](details/reactors/structural-scaling-and-output-caps.md);
- [fuel-cell catalysis, specific power, and thermal limits](details/fuel-cells/catalysis-power-density-and-thermal-limits.md);
- [gun and railgun progression](details/weapons/gun-and-railgun-progression.md);
- [alien fleet and module audit](details/aliens/fleet-and-module-audit.md).

### Settled reactor mass-and-output refinement

**Implementation status:** applied in Economic Equalization Overhaul 0.7.5.
These values supersede the earlier reactor targets where they overlap.

- Reduce the efficiency of every touched solid, compact-solid, molten-salt, and
  molten-core reactor by another five percentage points:
  - Solid I–V: **60%, 62.5%, 65%, 67.5%, 70%**
  - Compact Solid I–V: **62.5%, 65%, 67.5%, 70%, 72.5%**
  - Molten Salt I–II: **77%, 78%**
  - Molten Core I–III: **70%, 73%, 75%**
- Double reactor specific mass:
  - Solid I–V: **80, 68, 56, 24, 16 t/GW**
  - Compact Solid I–V: **12, 10, 8, 6, 4 t/GW**
  - Molten Salt I–II: **4, 3.6 t/GW**
  - Molten Core I–III: **8, 7, 6 t/GW**
- Double Fuel Cell I–III specific mass as part of the same overall
  specific-mass pass: **5,600, 3,600, and 960 t/GW**, equivalent to
  **5.6, 3.6, and 0.96 kg/kW**. This clarification changes fuel-cell specific
  mass only; their output-cap and stored-energy questions remain open.
- **Fuel-cell mass-accounting boundary:** power-plant specific mass includes
  the fuel-cell and regenerative hardware plus the solar arrays described by
  the localization. Radiator panels are not included in power-plant mass.
  Efficiency determines rejected heat and therefore the separate ship
  radiator requirement.
- Set the current reactor maximum-output targets:
  - Solid I–V: **1, 3, 10, 30, 60 GW**
  - Compact Solid I–V: **0.75, 2, 4, 6, 10 GW**
  - Molten Salt I–II: **40, 400 GW**
  - Molten Core I–III: **4, 17, 200 GW**
- Molten Salt II is deliberately capped at **400 GW** because Pegasus Drive
  x6 requires approximately **395 GW**.
- With those output caps and specific masses, plant mass at maximum output is:
  - Solid I–V: **80, 204, 560, 720, 960 t**
  - Compact Solid I–V: **9, 20, 32, 36, 40 t**
  - Molten Salt I–II: **160, 1,440 t**
  - Molten Core I–III: **32, 119, 1,200 t**

### Still under research

- Fuel Cells I–III maximum output and whether to expose stored energy/recharge;
  their doubled specific masses are settled
- finer sequence adjustments to the current solid, compact-solid, molten-salt,
  and molten-core efficiency, output, and specific-mass targets
- reactor volume and crew requirements
- Diana, Nerva, and Kiwi drives
- Lithium-Ion Battery
- Water and Heavy Water Heat Sink capacity and mass
- 10-inch Cannon mass, magazine, projectile, velocity, and firing cycle
- 30mm Autocannon mass, magazine, projectile, velocity, and firing cycle; its
  zero-crew decision is already settled
- 40mm Autocannon mass, magazine, projectile, velocity, and firing cycle; its
  zero-crew decision is already settled
- Point Defense Laser Turret mass, power, efficiency, optics, range, and firing
  cycle; its zero-crew decision is already settled
- 6-inch and 8-inch Gun Battery performance and ammunition; their two-crew
  decisions are already settled
- Light Railgun Battery, Railgun Battery, and Light Rail Cannon performance,
  power, heat, and ammunition; their crew decisions are already settled
- alien laser point-defense effectiveness, magnetic-weapon tier allocation,
  reactor/drive pairing, armor, and module placement; all alien changes remain
  deferred pending controlled combat tests and more campaign experience
- whether module volume should eventually be enforced or remain an audit-only
  planning metric
- whether the Frigate's active radiator collider extending behind the visible
  ship is a live-combat hitbox defect that should receive a separate prefab fix
- correction of the power-plant waste-heat formula
- whether the draft 1% open-cycle residue should be raised for balance, plus
  how to represent shutdown decay heat
- whether hull visual scale, prefab hit colliders, and statistical
  length/width should remain coupled or receive separate mod-side controls
- hull construction-material composition and the exact 3 t/crew construction
  package; the current research candidate is a metal-forward, mass-conserving
  split, but it is not yet a settled decision
