# Nose batteries and a larger spinal tier — proposal

2026-09-25. **Planning only. No gameplay, JSON layouts, weapon templates, compiled assemblies or deployment changed by this proposal.** Baseline is the current EEO 0.10.0 package against installed TI 1.0.53b, including the already implemented capital heavy hull mounts. This is a new discussion candidate; it does not activate the earlier maximum-nose restriction or the deferred size-1–3 normalization.

## Assessment

I support separating **weapon size** from **how many weapons a ship can carry**. The proposal creates a useful distinction between a single large spinal weapon and a battery of several smaller nose weapons, without needing more weapon sizes immediately. Battleship 2×size 2 and Dreadnought 3×size 3 are credible starting points. Mothership 4×size 4 is a much larger combat change than its four displayed cells suggest.

The detailed exact-4× tables are reference calculations; the preferred family-specific delta method is set out below. The important qualification is the Lancer's role. At the exact-4× reference target, one Lancer has 33% more nose output than three size-3 weapons on a Dreadnought, at the same technology. But the Titan still carries the same spinal, and the Mothership carries four. The Lancer can be the **human nose specialist and an efficient way to field a spinal**, not the absolute nose-output leader. Its lead also needs appearance-specific testing: current Lancer V2 mass makes its proposed nose output per bare tonne lower than Dreadnought V2, despite the stronger gun.

My recommendation is to retain the proposed human counts for testing, treat the Mothership as a separate boss-scale acceptance case, and select alien weapon groups explicitly rather than promoting every alien nose cell. Do not apply a uniform raw-energy multiplier to lasers without optical/armor tests.

## Scope and assumptions

- Human Battlecruiser: retain one size-3 primary nose weapon.
- Human Battleship: one size-2 → **two size-2** weapons.
- Human Dreadnought: one size-3 → **three size-3** weapons.
- Human Lancer and Titan: retain one size-4 primary, using the enlarged size-4 tier.
- Alien Mothership: one size-4 → **four size-4** weapons.
- Smaller hulls retain their current size ladder and statistics.
- Alien Battleship, Dreadnought, Titan and Lancer: alternatives below, not settled values. The user specifically left the Alien Dreadnought/Titan decision open.
- For the numerical screen, interpret “4× mass and power” as **4× current size-3 mounting mass, nominal firepower and full-cycle mean electrical input**, within each family/tier. The user confirmed this broad direction, while preferring nuanced linear adjustments to each family's existing size-3→4 deltas. Exact 4× values below are a reference scenario, not selected final settings. This is not four times the existing size-4 weapon. No size-1–3 A/B normalization is assumed.
- Keep bare hull masses, base crew and utility counts unchanged in this candidate. Alien base crew is untouched. Additional weapon crew follows the eventual equipment definitions; crew remains flavor/support accounting, not a primary balancing lever. Existing rounded human bare-mass targets remain intact.

“Primary” comparisons use a largest-size nose loadout, not every possible current fitting or the highest raw-DPS optimizer. Smaller nose choices remain legal today. Whether the eventual new weapon groups should admit only their designated size is a separate approval; this document recommends consistent player/AI rules if that restriction is chosen.

## Preferred tuning method: scale existing deltas by family

The user's clarification favors preserving each family's existing progression and adjusting its size-3→4 difference. A useful explicit interpretation is:

`new size-4 metric = current size-3 metric + λ × (current size-4 metric − current size-3 metric)`

Here λ=1 preserves current size 4. **λ=4 amplifies the existing difference fourfold; it does not generally make size 4 four times size 3.** This formula is a proposed way to express the preference, not an already agreed universal coefficient. Use separate family/tier tuning, and derive coupled ammunition/energy/input values consistently. Mounting mass can have its own coefficient; it need not track raw energy exactly. Keep sizes 1–3 unchanged for this experiment.

These illustrative λ=4 values show why a shared multiplier is insufficient. The final column is the raw-output coefficient that would exactly reach the 4× reference; it is diagnostic, not a recommendation to apply that coefficient to mass, optics and power as well.

| Family | S4 mount t at λ=4 | S4 raw MW at λ=4 | Raw output / size 3 | S4 mean draw MW at λ=4 | λ for raw output = 4×size 3 |
| --- | ---: | ---: | ---: | ---: | ---: |
| Rail Mk3 | 350.0 | 445.4 | 3.34× | 1,454.5 | 5.12 |
| Coil Mk3 | 350.0 | 1,916.9 | 2.77× | 3,141.7 | 6.77 |
| Siege coil Mk3 | 3,000.0 | 3,880.0 | 3.08× | 6,333.7 | 5.78 |
| Green Phaser | 1,195.0 | 55.0 | 1.57× | 137.5 | 21.00 |
| AdvancedAlien magnetic | 350.0 | 4,445.9 | 2.77× | 6,586.5 | 6.76 |
| Gen3Alien magnetic | 350.0 | 57,592.0 | 5.09× | 77,918.6 | 2.93 |
| Neutral particle | 400.0 | 82.8 | 5.00× | 551.7 | 3.00 |
| Plasma Mk3 | 980.0 | 14.9 | 1.11× | 381.8 | 105.00 |

For ordinary Mk3 coils, λ=4 yields **1,916.9 MW** nose output, below the Dreadnought battery's **2,073.6 MW**. The Lancer's nose is about 7.6% weaker and its complete coil output about 23.6% lower. Its V0 complete-armament density is 858.7 versus the Dreadnought's 875.4 MW per 1,000 t reference. Thus merely multiplying existing coil deltas by four does not maintain the stated Lancer role. The raw-output coefficient must exceed about **4.51** just to exceed three size-3 coils; about **6.77** reaches the four-size-3 reference. Actual hit quality can change combat results, but should be measured rather than assumed to close that gap.

At λ=4, four Mothership advanced-mag spinals give about **17,783 MW** nose output, **7.69× current**, with 1,400 t nose mounting mass and 26.35 GW mean nose input. The exact-4× reference elsewhere gives 25,642 MW, 11.08× current, 2,400 t and 37.99 GW. Both are substantial changes, but they are different scenarios.

Plasma's tiny existing raw-output gap would require λ=105 to reach the 4× reference. That is evidence to use a reviewed new endpoint or another parameter adjustment for plasma, not a reason to multiply every plasma delta by 105. Lasers likewise need optical/armor tests; particle families already near doubling overshoot the reference with λ=4. A family-specific blend of adjusted deltas and explicit endpoints is more credible than a universal coefficient.

**Preferred plan:** use the role curve to set acceptable performance bands, use scaled existing deltas to obtain initial family-specific candidates, and tune mounting mass, cadence, energy, input and ammunition coherently through combat tests. The remaining exact-4× tables are retained as a common reference for the upper-level ship-role comparison. The snapshot also contains λ=4 results for every compared hull and all 34 matched families.

## The intended firepower curve

Let P3 be the output of one size-3 weapon at a fixed family/tier. With size 4 = 4P3, the proposed nose outputs are:

| Hull | Proposed primary nose battery | Output relative to P3 |
| --- | --- | --- |
| Battlecruiser | 1×size 3 | 1 |
| Battleship | 2×size 2 | About 1 if size 3 doubles size 2; actual families vary |
| Dreadnought | 3×size 3 | 3 |
| Lancer | 1×size 4 | 4 |
| Titan | 1×size 4 | 4 |
| Mothership | 4×size 4 | 16 |

That makes a Lancer's nose 4/3 of the Dreadnought battery, not four times the Dreadnought. The battery still has three independent weapons: better target splitting, less overkill against small targets, and partial output after losing one mount. A spinal concentrates damage and can breach armor differently. Those are real role distinctions even when raw totals are close.

For continuity with the old planning notation, conceptual A nose weights become **2:4:8:32**, and B becomes **3:6:12:48**. These are relative design weights, not measured weapon statistics and not an instruction to rebalance sizes 1–3. B multiplies every nose contribution by 1.5; it does not multiply the unchanged hull contribution. The current conceptual comparator uses the earlier 2:4:8:16 notation. The actual-template tables below are the better guide to real deltas.

### Exact-4× reference: capacity and density, appearance V0

| Hull | Bare t | Volume m³ | Nose A units now → proposal | Total A units now → proposal | Proposed nose units / 1,000 t | Proposed nose units / 100,000 m³ | Proposed nose units / MC |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Gunship | 180.0 | 11,019.7 | 2 → 2 | 2 → 2 | 11.1 | 18.1 | 2.0 |
| Corvette | 400.0 | 5,070.8 | 2 → 2 | 3 → 3 | 5.0 | 39.4 | 2.0 |
| Frigate | 600.0 | 13,692.5 | 2 → 2 | 4 → 4 | 3.3 | 14.6 | 1.0 |
| Destroyer | 900.0 | 22,062.0 | 4 → 4 | 6 → 6 | 4.4 | 18.1 | 2.0 |
| Cruiser | 1,000.0 | 45,819.9 | 4 → 4 | 7 → 7 | 4.0 | 8.7 | 1.3 |
| Battlecruiser | 1,200.0 | 43,684.4 | 8 → 8 | 10 → 10 | 6.7 | 18.3 | 2.7 |
| Lancer | 2,000.0 | 152,598.4 | 16 → 32 | 19 → 35 | 16.0 | 21.0 | 8.0 |
| Battleship | 1,600.0 | 105,142.7 | 4 → 8 | 10 → 14 | 5.0 | 7.6 | 2.7 |
| Dreadnought | 2,400.0 | 272,229.4 | 8 → 24 | 16 → 32 | 10.0 | 8.8 | 6.0 |
| Titan | 3,400.0 | 514,552.1 | 16 → 32 | 40 → 56 | 9.4 | 6.2 | 6.4 |
| AlienBattleship | 1,656.0 | 201,325.2 | 4 → 8 | 10 → 14 | 4.8 | 4.0 | — |
| AlienDreadnought | 2,200.0 | 302,989.8 | 16 → 24 | 24 → 32 | 10.9 | 7.9 | — |
| AlienLancer | 1,656.0 | 166,671.7 | 20 → 36 | 24 → 40 | 21.7 | 21.6 | — |
| AlienTitan | 2,600.0 | 349,989.2 | 20 → 36 | 52 → 68 | 13.8 | 10.3 | — |
| AlienMothership | 8,500.0 | 49,197,068.6 | 16 → 128 | 80 → 192 | 15.1 | 0.3 | — |

A hull cell contributes one unit on ordinary ships and four on Titan, AlienTitan and Mothership under the deployed heavy-mount rule. “Total” adds that hull capacity to nose weights; it is not measured DPS. The alien Battleship 2×size 2 and Alien Dreadnought 3×size 3 rows are comparison cases only. Alien Lancer/Titan rows retain one spinal plus two size-1 secondaries for this baseline comparison; the alternatives below change that.

Volume is the current **measured main-hull envelope** used by the mod's volume catalogue, not the older template `volume` field, solid material volume, or a weapon's occupied volume. Mothership V0 is approximately 49.20 million m³ in that catalogue, explaining its low envelope-density result. That denominator does not make its absolute output harmless. Alien Mission Control values are not a comparable player economy constraint, so the table omits those ratios.

## Actual family deltas: exact-4× reference tier

Raw output below is nominal shot energy divided by the full salvo cycle, in MJ/s = MW. It is **not damage points/s or measured damage on target**. Mean electrical input is not peak firing demand or the required installed reactor rating. Mounting mass excludes ammunition, crew support and the rest of the ship.

| Family | S4 mounting t now → target | S4 raw MW now → target | S4 mean draw MW now → target | Raw output delta |
| --- | --- | --- | --- | --- |
| Rail Mk3 | 200.0 → 600.0 | 211.3 → 533.0 | 690.0 → 1,740.5 | +152.3% |
| Coil Mk3 | 200.0 → 600.0 | 997.6 → 2,764.8 | 1,634.4 → 4,527.7 | +177.1% |
| Siege coil Mk3 | 1,500.0 → 4,000.0 | 1,915.6 → 5,043.4 | 3,126.8 → 8,231.5 | +163.3% |
| Green Phaser | 715.0 → 2,220.0 | 40.0 → 140.0 | 100.0 → 350.0 | +250.0% |
| UV Phaser | 715.0 → 2,220.0 | 40.0 → 140.0 | 133.3 → 466.7 | +250.0% |
| AdvancedAlien magnetic | 200.0 → 600.0 | 2,313.4 → 6,410.5 | 3,427.3 → 9,497.0 | +177.1% |
| Gen3Alien magnetic | 200.0 → 600.0 | 22,885.2 → 45,265.1 | 30,962.3 → 61,241.0 | +97.8% |
| Neutral particle | 160.0 → 320.0 | 33.1 → 66.2 | 220.7 → 441.4 | +100.0% |
| Alien particle | 800.0 → 1,600.0 | 44.2 → 117.8 | 220.9 → 589.1 | +166.7% |
| Plasma Mk3 | 560.0 → 1,680.0 | 13.8 → 53.6 | 222.0 → 674.7 | +288.9% |

The exact calculations cover 34 families with both size 3 and size 4 in the [data snapshot](tables/nose-battery-proposal-2026-09-25.json). The family map comes from the earlier audit; all metrics above were recalculated from freshly merged installed/mod templates with source hashes. The remaining families and unique spinal weapons require explicit mappings, not name-based extrapolation. Conventional cannons lack size-3/4 counterparts; unique neutron/relativistic weapons have no size-3 family anchor.

The proposal is substantially different across families: Mk3 coil size 4 grows about 177% in raw output, while Mk3 plasma grows about 289%. Gen3 alien magnetic size 4 is already close to twice size 3, so its increase is about 98%. Equal size ratios will not correct existing differences between weapon families or research tiers.

### Laser exception

Green/UV phaser rows are **literal-rule sensitivity examples**, not my recommended final settings. Size-3/4 human lenses are 720/960 cm. The larger lens already concentrates energy better at range; quadrupling size-3 emitted energy while retaining the larger aperture would increase effectiveness beyond a simple 4× relationship in some engagements. Armor thresholds make the effect nonlinear. Conversely, effective firepower targets cannot reliably be converted into a single universal electrical multiplier.

For lasers, first test the current size-4 optical weapon, then vary emitted energy, input, cadence and mounting mass separately. A lower shot-energy multiplier, slower cadence, or an effective-output target at selected armor/range conditions are alternatives. Do not multiply aperture, energy and firing rate together. Larger lasers remain poor replacements for rapid-fire escort PD; penetration improvements do not give the same benefit against unarmored projectiles. Some capital laser PD is acceptable and does not invalidate the fleet-role goal.

For kinetics, a conservative experimental direction is to retain velocity/range/cadence initially and solve for projectile and ammunition mass to reach the target. Recalculate electrical input consistently; do not independently set incompatible ammo mass, velocity, efficiency and power values. For plasma, charging energy and kinetic energy must be considered separately. For particle weapons, preserve or deliberately retune damage-channel fractions. Final template fields need family-specific review.

## Representative complete armaments: exact-4× reference

Human rows use ordinary Mk3 coils; alien rows use advanced alien magnetic weapons. All rows keep the current legal hull battery fit, using the heavy-mount doctrine. Human Lancer carries one medium and one light hull battery; Battleship one heavy and two light; Dreadnought two heavy; Titan six heavy. The Mothership retains sixteen heavy batteries. These fixed fits isolate the proposed nose changes; they are not guaranteed optimal battle loadouts or equal-tech human/alien comparisons.

Reference mass = current V0 bare hull including base crew + weapon mounting mass. It excludes ammunition, weapon crew support, armor, utilities, drive, reactor, battery, radiator and fuel. Consequently these are armament-density screens, not completed-ship performance or buildable-design guarantees.

| Hull / family | Nose raw MW now → proposal | Nose mounting t now → proposal | Nose mean draw GW now → proposal | Total raw MW now → proposal | Raw MW / 1,000 t reference now → proposal | Raw MW / 100,000 m³ now → proposal |
| --- | --- | --- | --- | --- | --- | --- |
| Gunship / Coil Mk3 | 229.5 → 229.5 | 50.0 → 50.0 | 0.4 → 0.4 | 229.5 → 229.5 | 997.9 → 997.9 | 2,082.8 → 2,082.8 |
| Battlecruiser / Coil Mk3 | 691.2 → 691.2 | 150.0 → 150.0 | 1.1 → 1.1 | 830.4 → 830.4 | 580.7 → 580.7 | 1,900.9 → 1,900.9 |
| Lancer / Coil Mk3 | 997.6 → 2,764.8 | 200.0 → 600.0 | 1.6 → 4.5 | 1,201.8 → 2,969.0 | 518.0 → 1,091.5 | 787.6 → 1,945.6 |
| Battleship / Coil Mk3 | 418.0 → 836.1 | 100.0 → 200.0 | 0.7 → 1.4 | 898.7 → 1,316.7 | 463.3 → 645.5 | 854.8 → 1,252.3 |
| Dreadnought / Coil Mk3 | 691.2 → 2,073.6 | 150.0 → 450.0 | 1.1 → 3.4 | 1,392.5 → 2,774.9 | 485.2 → 875.4 | 511.5 → 1,019.3 |
| Titan / Coil Mk3 | 997.6 → 2,764.8 | 200.0 → 600.0 | 1.6 → 4.5 | 3,101.6 → 4,868.7 | 680.2 → 981.6 | 602.8 → 946.2 |
| AlienBattleship / Adv. alien mag | 1,004.5 → 2,008.9 | 100.0 → 200.0 | 1.5 → 3.0 | 2,289.7 → 3,294.2 | 1,147.2 → 1,571.7 | 1,137.3 → 1,636.3 |
| AlienDreadnought / Adv. alien mag | 2,313.4 → 4,807.8 | 200.0 → 450.0 | 3.4 → 7.1 | 4,134.3 → 6,628.7 | 1,519.9 → 2,231.9 | 1,364.5 → 2,187.8 |
| AlienLancer / Adv. alien mag | 3,267.8 → 7,364.8 | 300.0 → 700.0 | 4.8 → 10.9 | 4,045.2 → 8,142.3 | 1,911.7 → 3,236.2 | 2,427.1 → 4,885.2 |
| AlienTitan / Adv. alien mag | 3,267.8 → 7,364.8 | 300.0 → 700.0 | 4.8 → 10.9 | 10,551.1 → 14,648.1 | 2,524.2 → 3,198.3 | 3,014.7 → 4,185.3 |
| AlienMothership / Adv. alien mag | 2,313.4 → 25,641.8 | 200.0 → 2,400.0 | 3.4 → 38.0 | 16,880.1 → 40,208.5 | 1,499.1 → 2,987.3 | 34.3 → 81.7 |

Key implications:

- **Battleship:** its nose doubles; complete coil output rises about 46.5%. Two size-2 coils actually give 21% more raw nose output than the Battlecruiser's one size-3 coil, because the current family is inconsistent with perfect doubling. Larger shots/range/penetration may still favor the Battlecruiser. At equal MC cost, the Battleship is a stronger generalist, so test whether Battlecruiser speed, price and concentrated fire remain valuable.
- **Dreadnought:** nose output triples; complete coil output nearly doubles. Its three size-3 coils give about 2,074 MW of raw nose output and independent targeting, making a clear battery battleship role.
- **Lancer:** complete coil output rises about 147%; 2,969 MW is only about 7% above the Dreadnought's 2,775 MW once hull weapons are counted. Both cost four MC. Its advantage should be spinal hit quality, lower mass and concentration of offense, while the Dreadnought has more utility/hull coverage and redundancy.
- **Titan:** complete coil output rises about 57%. It matches the Lancer's nose and has roughly 64% greater complete output. This is compatible with a large generalist role but rules out saying the Lancer is the strongest individual warship.
- **Mothership:** advanced magnetic nose output becomes **11.08× current**, because four mounts combine with a 2.77× per-spinal increase. Complete output rises about **138%**, and the mounting-mass-adjusted raw density almost doubles. Nose mounting mass rises 2,200 t and mean nose electrical input reaches about **38 GW**. With Gen3 magnetic weapons, four proposed spinals alone would demand about **245 GW mean input**, before hull weapons or propulsion. Those are separate progression cases needing explicit tests.

### Appearance sensitivity

Using the same proposed coil fits, total raw MW per 1,000 t reference mass is:

| Hull | V0 | V1 | V2 | V3 |
| --- | ---: | ---: | ---: | ---: |
| Lancer | 1,091.5 | 922.0 | 642.6 | 642.6 |
| Dreadnought | 875.4 | 736.0 | 823.4 | 635.0 |
| Titan | 981.6 | 830.8 | 962.2 | 720.2 |

The Lancer's efficiency lead is therefore **not universal**. Lancer V2 is 3,900 t bare versus Dreadnought V2 at 2,600 t. Their nose A weights per 1,000 bare tonnes are 8.21 versus 9.23. If “king of nose weapons” means best efficiency across every appearance, this proposal alone does not achieve that; an appearance-mass/role review would be additional scope. Full 45-appearance results for the selected 15 hulls are in the snapshot.

## Alien scaling alternatives

Current geometry gives the Alien Dreadnought **four** nose cells; Alien Lancer and Alien Titan each have **six**. The six-cell arrangement supports one native size-4 footprint plus two small secondary cells, not automatically two native size-4 weapons. Count and adjacency must be treated separately.

Use explicit per-class weapon groups rather than blindly promoting every cell. For comparison, these numbers use advanced alien magnetic weapons and the proposed size-4 tier. They exclude hull armament and any small secondary noses.

| Hull | Candidate | P3 equivalents | Nose mounting t | Raw MW | Mean input GW |
| --- | --- | ---: | ---: | ---: | ---: |
| AlienDreadnought | 1×size 4 | 4 | 600 | 6,410.5 | 9.5 |
| AlienDreadnought | 2×size 3 | 2 | 300 | 3,205.2 | 4.7 |
| AlienDreadnought | 3×size 3 | 3 | 450 | 4,807.8 | 7.1 |
| AlienDreadnought | 4×size 3 | 4 | 600 | 6,410.5 | 9.5 |
| AlienTitan | 1×size 4 | 4 | 600 | 6,410.5 | 9.5 |
| AlienTitan | 2×size 3 | 2 | 300 | 3,205.2 | 4.7 |
| AlienTitan | 6×size 3 | 6 | 900 | 9,615.7 | 14.2 |
| AlienTitan | 2×size 4 | 8 | 1,200 | 12,820.9 | 19.0 |
| AlienTitan | 6×size 4 | 24 | 3,600 | 38,462.8 | 57.0 |
| AlienMothership | 4×size 4 | 16 | 2,400 | 25,641.8 | 38.0 |

My first test candidate would be **Alien Battleship 2×size 2, Alien Dreadnought 2×size 3, Alien Titan 2×size 4, Mothership 4×size 4**. Keep the Alien Lancer as one spinal, with its existing small secondaries initially, until its distinct role is decided. This produces a specialist/battery/capital progression without demanding that every visible legacy cell become a full spinal.

The Alien Dreadnought is still buffed relative to today's advanced magnetic nose: 2×size 3 is 3,205 MW versus the current 2,313 MW spinal, about +39%. Three size-3 weapons is a reasonable stronger alternative (+108%) and mirrors the human class. Preserving one newly enlarged spinal instead gives 6,410 MW (+177%) and makes it a compact spinal platform; choose that only if overlap with Alien Lancer is intentional. Four size-3 weapons also total 6,410 MW, but with battery targeting and survivability characteristics.

Alien Titan 2×size 4 supplies 8 P3, halfway to the Mothership's 16 P3 before hull weapons. Six size-3 weapons give 6 P3 and a different saturation/target-splitting role. Six size-4 weapons give 24 P3—**50% more nose output than the proposed Mothership**—so I would reject automatic one-spinal-per-cell promotion there. The suggested 2×size-4 Titan has about **19.0 GW total mean nose input**, versus 4.8 GW for its current spinal plus two size-1 advanced-mag secondaries: approximately **14.2 GW additional**.

None of these alien count choices are settled. In particular, the human-style 3×size-3 Alien Dreadnought in the comparison tables is not the recommended 2×size-3 test candidate and must not be mistaken for an approved value.

## Mass, fuel, delta-v and reactor consequences

At unchanged exhaust velocity and propellant load:

`delta-v = exhaust velocity × ln(1 + propellant mass / dry mass)`

Adding mounting mass, ammunition and support equipment reduces both delta-v and acceleration. For thrust held constant, `new acceleration / old acceleration = old wet mass / new wet mass`. These equations give direction and magnitude only once an actual design's drive, fuel and supporting equipment are specified.

A mass-only illustration with 10,000 t initial dry mass and 10,000 t propellant:

| Extra dry mass | Delta-v change | Acceleration change |
| --- | ---: | ---: |
| 100 t, Battleship coil mount increment | −0.72% | −0.50% |
| 300 t, Dreadnought coil mount increment | −2.12% | −1.48% |
| 400 t, Lancer/Titan coil mount increment | −2.80% | −1.96% |
| 2,200 t, Mothership advanced-mag mount increment | −13.63% | −9.91% |

These use the same artificial reference ship to isolate sensitivity, not each class's predicted delta-v. Extra ammunition, reactor/radiator/battery mass, lost tank volume, or a different drive may dominate. At a fixed wet-mass budget, heavier machinery also displaces fuel, making the penalty larger.

The current occupied-volume rule uses `weapon.internalSize × 400 m³` for nose weapons. A size-4 stat increase alone therefore does **not** increase its occupied volume. Compressing its UI footprint must not reduce its physical accounting further.

| Hull | Current nose m³ | Proposed at native size m³ | Δ m³ | Proposed if S4 counts as 12 m³ | Δ m³ |
| --- | --- | --- | --- | --- | --- |
| Lancer | 1600 | 1600 | 0 | 4800 | 3200 |
| Battleship | 800 | 1600 | 800 | 1600 | 800 |
| Dreadnought | 1200 | 3600 | 2400 | 3600 | 2400 |
| Titan | 1600 | 1600 | 0 | 4800 | 3200 |
| AlienDreadnought | 1600 | 3600 | 2000 | 3600 | 2000 |
| AlienMothership | 1600 | 6400 | 4800 | 19200 | 17600 |

The last columns show an optional physical-volume model: a proposed size-4 weapon occupies four times a size-3 weapon, i.e. **12 internal accounting units / 4,800 m³**, while retaining the size-4 equipment category. This requires a separate volume parameter or policy; do not overload the graphical mount size. It is a candidate, not an approved requirement. Even its Mothership +17,600 m³ is small against the present 49.20-million-m³ envelope, so occupied volume alone is unlikely to balance that ship.

Reactor fit must be checked at the current design/burst demand as well as sustained mean demand. Larger shots may require larger batteries even at the same mean draw. Added waste heat requires radiator/heat-sink checks. Human reactor-bay limits vary strongly by appearance: Lancer V0 is about 2,366 m³, V1 2,090, V2 10,224 and V3 8,073. Thus the apparently efficient V0/V1 Lancers may have tighter power constraints than the heavier variants. No valid power-plant/drive combination or acceleration figure can be promised from raw weapon tables alone.

## Fitting, rendering and AI plan — only after approval

1. Define an explicit per-hull nose-group profile: supported weapon sizes, number of groups, displayed anchor(s), physical volume and model firing anchors. Human Battleship's two cells each become a size-2 group, Dreadnought's three cells each become a size-3 group, and Mothership's four cells each become a size-4 group. Alien alternatives need their own explicit mapping. Preserve native smaller-ship geometry unless a separate restriction is approved.
2. Keep display layout in JSON, with symmetric coordinates. **JSON coordinates alone cannot grant multiple oversized nose weapons**: native weapon footprints, validation and model assumptions must also be adapted, similar to the capital hull-mount policy. Do not expand the UI with bespoke rows. For alien two-group options, choose how the old four/six-cell geometry maps to two groups, rather than leaving misleading spare active cells.
3. Use one eligibility/placement policy for icon/table visibility, drops, refits, validation, AI and player auto-design. If a group is maximum-size-only, hide other sizes using the existing catalog mechanism. Current AI chooses a primary and fills small noses, with size-3/4 restrictions for Protector roles; it needs group-aware fitting and role handling. A catalog filter by itself is insufficient.
4. Preserve weapon research and resource constraints. Check missing size families and unlocks: early conventional cannons cannot populate size-3/4-only groups. Do not grant research or let AI retry impossible designs indefinitely. All nose sizes/AI behavior remain unchanged until implementation is separately approved.
5. Verify independent firing arcs, muzzle assignment, model transforms, ammunition, damage, repair orders and targeting for every new group. Four independently firing Mothership spinals must not share invalid or duplicated runtime module identities.

## Save conversion and release plan — only after approval

Fresh-save fitting and AI are the first acceptance path. Always migrate older saves best-effort and preserve data, including campaign designs, ships, queues, stock rosters and imported skirmish/saved designs. Archive the source and report conversions. Add a versioned nose-layout marker separate from the already deployed capital-hull marker; repeated loads must not multiply equipment again. An eventual release with new one-way nose geometry should be **0.11.0 or later**, not silently another 0.10.0 package.

Suggested conversion for the named human/Mothership changes: expand occupied legacy nose cells to the new group anchored there, using the same researched family/tier at the required size where possible; leave truly empty cells empty. For a native full Mothership spinal, its four occupied cells become four new spinals. A full human size-2/3 nose similarly becomes two/three weapons. Report this intentional equipment expansion. For alien options combining cells into fewer groups, use a documented many-to-one mapping rather than this rule.

Preserve ammunition/damage fractions through deterministic mappings; do not copy raw round counts across calibers or silently repair/reload old ships. Preserve research, identities, fuel, armor, utility equipment and unrelated state. If faction state is unavailable, use the static prerequisite mapping; preserve a documented legacy equipment fallback rather than losing the save. Retain fuel/reactor over-limit states until refit, then apply current limits. The existing no-data-loss policy takes precedence over rejecting old saves.

## Combat acceptance and decisions remaining

Test in distinct stages: changed nose counts with old weapon stats; the enlarged size-4 tier; then the combined candidate. This separates multiplicity effects from weapon-stat effects. Test fresh designs first, then migrated damaged/partially loaded ships and skirmish imports, including return-to-menu and designer entry after loading.

Compare equal-MC human fleets and equal-resource/equal-wet-mass fleets separately. Include current ordinary Mk3 coils, siege coils, plasma, particle weapons and lasers; advanced and Gen3 alien loadouts are separate tests. Measure useful damage, time to kill, overkill, projectile interception, heat/power interruptions, ammunition endurance, losses, acceleration and delta-v. Include small evasive targets, armored capitals and station assaults, several engagement ranges, both concentrated and split targets, and every hull appearance. Validate that fast escort lasers still provide worthwhile PD under saturation.

Decisions still required before implementation:

- Is the Lancer's goal strongest single human nose weapon, best practical spinal platform, or best efficiency across all appearances? The first is shared with Titan; the last needs additional work.
- Which alien Dreadnought/Titan group counts and sizes, and what happens to Alien Lancer secondary noses?
- What family-specific delta coefficients and acceptable combat-output bands should replace the exact-4× reference? The broad mass/firepower/electrical direction is confirmed; lasers need their own optical target.
- Should enlarged spinals receive proportional physical occupied volume, and should sizes be exclusive within each new group?
- Which unique/siege weapon families participate, and which remain special exceptions?

**Recommendation:** proceed to a later controlled experiment with the proposed human counts, the Mothership count as a separate acceptance target, and explicit alien alternatives. Use family-specific adjustments to existing deltas as the starting method, with explicit endpoints where the current progression is unsuitable. Keep the literal 4× rule as a comparison reference, not the final universal formula.
