# Rejected armor calibration proposal

Archived 2026-11-04 after the user rejected the multi-parameter calibration. C0/C1 weapon work factors, battery spread scaling, the added laser thermal-area floor and the 0.25 internal-deposition tuning are **not the active plan**. Retained figures/calculations below are evidence of that rejected proposal, not current balance authority. Use the [current armor rework plan](../../armor-rework-plan.md), whose balance control is one uniform armor-work divisor. The spatial/geometry discussion was not rejected wholesale.

---

# Armor rework plan

Status: **candidate architecture and formulas proposed; no implementation**.
Updated 2026-11-04. The [as-is audit](../../armor-system-as-is-2026-11-04.md) owns current
mechanics, and its [late-game coiler cases](../../armor-system-as-is-2026-11-04.md#12-late-game-coiler-lethality-400-armor-titans-and-200-armor-battlecruisers)
explain the reported capital-ship lethality. This document owns the emerging
rework requirements. It does not authorize implementing unspecified formulas.

The separately requested [alien material update](../../alien-armor-material-update-2026-11-04.md)
has already been implemented; it is not evidence that this rework is deployed.

## Agreed direction

1. **Laser ablation.** Move lasers toward removing armor instead of primarily
   bypassing it through the current focused-beam penetration calculation. Decide
   later how much direct penetration remains under different conditions.
2. **Partial thickness and weak spots.** Ideally track a 2D ship-surface tile
   array with remaining armor thickness, an area affected by each hit, random
   spot selection, and potentially components underneath specific coverage.
   A simpler abstraction is acceptable, provided its limitations are explicit.
   The meaningful quantity is how much area has lost how much depth, not only
   the current binary full-thickness-versus-hole coverage percentage.
3. **Officer skill affects hit quality.** Remove officer-driven increases to
   damage/energy. An officer may improve critical/weak-spot hit probability and/or
   reduce bullet aim noise. Preserve useful progression without manufacturing
   additional kinetic joules or multiplying the damage of an otherwise identical
   impact. Selection between these replacement effects and their magnitudes is
   still open.
4. **Modest overall armor improvement.** A small general armor buff is desirable.
   Strong weapons must still penetrate under-armored ships. Maximum practical
   armor on late-game capitals should provide reliable initial protection from
   standard hull-mounted weapons, rather than merely postponing sudden death
   until a small velocity/bonus threshold is crossed.
5. **Heavy penetration remains relevant.** The intended analogy is cruiser guns
   versus an intact battleship belt, with torpedoes and dedicated nose penetrators
   retaining a credible penetration role. Do not turn maximum armor into universal
   immunity, or turn ordinary hull guns into routinely decisive first-hit capital
   killers. Avoid routine one-hit kills as the dominant late-game outcome.

These requirements are also recorded in the repository's
[design directives](../../../design-directives.md#armor-weapon-energy-and-officer-skill).
The numerical examples below are proposed calibration cases, not accepted final
balance values or measured runtime performance.

## Officer changes to include in implementation planning

The current +61% example in the audit combines +10% faction technology with
+51% from rank-3 Gunnery/Captain/Intelligence officers. The officer contribution
must not survive as a direct damage multiplier in the intended model. The faction
technology is a separate source; this direction does not automatically approve
removing or retaining all non-officer damage effects.

Audit all officer damage consumers, including `GunDamage`, `BeamDamage`,
`MissileDamage` and `GlobalDamage`, rather than fixing only the magnetic weapon
formula. Update officer descriptions, AI estimates and autoresolve consistently.
Existing targeting and other officer benefits should retain their intended roles.
Review defensive officer damage-reduction effects separately: distinguish better
damage control/repair from changing the physical energy of an incoming hit;
blanket removal of those effects has not been decided.

A useful controlled check is two identical impacts with identical mass, relative
velocity, material, local thickness, angle and component conditions: changing
the firing officer alone must not change the incoming energy or per-impact
damage calculation. Better aim can change where a projectile lands and whether
it hits at all. A trajectory perturbation should preserve authored muzzle-speed
magnitude; any changed relative impact energy must arise from the resulting
physical trajectory, not an officer multiplier.

Working interpretation for the crit design: let officer skill influence access
to weak coverage or vulnerable components under the chosen armor model. Avoid
recreating a large unconditional bypass of intact armor that defeats the desired
capital protection. This is a proposed guardrail, not a settled crit formula.

## Aim-noise implementation status

The user recalls mod-added random aim noise from an earlier session. Repository
history confirms an earlier implementation at commit `e8d0adb`:
`Core/PointDefensePerturbationRegistry.cs` reads `pdAngularSpread_deg` for naval
guns, and `PointDefenseTrajectoryPerturbationPatch` temporarily changes
`ProjectileWeapon.targetedPosition` while firing. The associated historical
record describes 0.004/0.003-degree stratified cones for autocannon missile
defense, not general ship-to-ship magnetic dispersion.

The registry, trajectory patch and its spread helpers are absent from the current
source tree/project during the 2026-11-04 check. The current source search found
no general ballistic random aim-noise implementation. This does not deny the
earlier work; it means the plan must reconcile what was retained, removed, or
exists elsewhere before attaching officer precision to it. Do not claim a live
general dispersion hook or restore an entire historical PD subsystem implicitly.
Native laser jitter is a beam-spot parameter and is not that ballistic mechanic.

## Balance cases the design must satisfy

| Case | Intended behavior to evaluate |
|---|---|
| Strong weapon vs under-armored ship | Penetration remains credible and materially punishes insufficient protection. |
| Standard hull gun vs intact maximum practical capital armor | First-hit penetration should be unlikely across the agreed normal late-game engagement envelope; ordinary fire can still contribute through degradation and repeated attacks. |
| Heavy/spinal penetrator or torpedo vs armored capital | Retains a serious anti-armor role; assess internal lethality separately from whether it penetrates. |
| Repeated hits on an already weakened area | Local depth/coverage affects outcomes progressively; precision can exploit damage without adding energy. |
| Officer-ranked vs unranked otherwise identical weapon | Better hit placement/quality, unchanged physical weapon output. |
| Modest armor on smaller ships | Armor still buys useful protection; the general buff should not erase heavy-weapon effectiveness. |

User clarification: **even the largest heavy hull batteries belong inside the
initial capital-protection requirement**. They are not an automatic anti-capital
exception. Preserve a separate nose-weapon progression: lower-tier nose weapons
can exceed higher-tier hull batteries, leaving headroom for late nose penetrators.
Mount grade, technological generation and salvo size are separate axes.

Additional user reference: mid-game ships with advanced gas-fission reactors and
nanotube armor may have only **15–20 nose points**. Both that scale and heavily
armored end-game Titans must remain meaningful. Do not flatten the power ladder
to fix the 400-point case, and do not accept routine two-shot capital destruction
by cruiser batteries merely because those batteries are technically high tier.
"Maximum practical armor" means attainable designs under caps and mass/cost
constraints. The normal engagement-velocity envelope still needs validation.

Use the 400-Hybrid Titan and 200-Hybrid Battlecruiser observations as reference
cases, with actual struck-facing values, weapon identity, closure, surviving
projectile mass, officer/faction effects, and single impact versus salvo clearly
separated. Include hull-mounted weapons in the new comparison set: the existing
case study concentrated on spinal and siege weapons and does not by itself prove
the standard-hull-gun target.

Measure first-hit penetration, armor depth/area removed, internal damage per hit,
hits/time to disable and destroy, and frequency of unusually early kills. Vary
closing velocity, obliquity, material, officer rank, firing cadence and existing
damage. Report distributions, not only average time to kill. Preserve useful
weapon roles and modest overall strengthening rather than achieving the target
solely by making every weapon uniformly ineffective.

## Recommended architecture and alternatives

Recommend a **contact-driven surface atlas with sparse adaptive tiles**, using
the same energy solver for simpler comparison models. Store a continuous remaining
depth on each represented patch, not an integer number of full armor layers.
The atlas is a set of 2D charts covering the armored exterior. It is not the
art texture UV layout and not a full 3D voxel interior.

| Representation | Strengths | Important losses/costs | Recommendation |
|---|---|---|---|
| One remaining-thickness scalar per armor node | O(1) state/update; simplest | Averages damage over the entire ship region; no persistent local weak spots or component targeting | Reference baseline only |
| Area-weighted depth histogram per node, e.g. 8–16 bands | Small state, O(bands) updates; represents percentages at different depths | No adjacency or identifiable patch to revisit; broad hits and repeated aimed hits need statistical assumptions | Best fallback if spatial model misses budget |
| Fixed dense equal-area grid | Simple indexing and adjacency; persistent targeting | Resolving centimetre laser spots across capital-sized surfaces consumes excessive RAM; coarse cells smear damage | Useful numerical reference, not preferred storage |
| Sparse adaptive grid on a prepared atlas | Persistent local depth, variable footprints, component-region mapping; pristine space implicit | More complex boundary/overlap logic and save state; requires convergence/performance checks | Preferred candidate |
| Damage stamped directly into every render triangle | Immediate visual correspondence | Art tessellation, duplicated surfaces, animation and mesh changes become gameplay state; expensive and fragile | Reject as the authoritative armor representation |

Use broad component regions initially, rather than building a detailed physical
interior. Leave a path to explicit component coverage without making it necessary
for the first armor solver. If adaptive tiles prove too expensive, choose the
histogram model explicitly before implementation/release; do not silently change
representation or armor strength under load.

## Surface area, art variants and armor nodes

The user requests two linked changes: larger ship appearances must require a
larger area to armor, and the 2D representation's extent must follow the surface
owned by its armor node. At equal thickness, a larger surface means more armor
mass/cost and more places to be hit; it is not free survivability.

A larger hull can also be easier to hit. Its greater surface only dilutes repeat
damage when impacts are distributed over that surface; a perfectly tracked aim
point does not become safer merely because unused armor exists elsewhere.

Existing `HullVolumeCatalog` is already keyed by hull and appearance index, but
stores **volume**, not surface area. The [existing mesh report](../../hull-volume-capacity-comparison-2026-11-03.md)
covers 64 appearances and preserves bounds. For example, Titan variant bounds
include about 49×40×265 m and 73×62×277 m. These establish differing geometry,
not measured armor surface areas. Do not infer actual area from `volume^(2/3)`
unless explicitly reporting a rough estimate for geometrically similar models.

Proposed offline preparation, keyed by `(hull ID, appearance, geometry revision)`:

1. Define an **armorable exterior envelope** from structural hull meshes, including
   armored machinery housings where appropriate. Exclude deployed radiators,
   weapon barrels, exhaust, decorations, duplicate/internal faces and surfaces
   occluded inside other hull pieces. The fuel-volume exclusion list cannot be
   reused blindly: excluded machinery housings may still need armor.
2. Prepare a simplified exterior shell and chart seams. Transform vertices into
   canonical ship metres before measuring. For an accepted exterior triangle,
   `area = 0.5 * |(b-a) × (c-a)|`. Raw render-triangle sums alone are insufficient
   because overlapping/internal geometry can inflate them.
3. Assign each envelope triangle to Nose, Tail, Left or Right coverage. Left and
   Right share the designer's lateral armor material/points but retain separate
   damage state and separately measured areas. Map dorsal/ventral surfaces into
   those side charts consistently; do not double-count them or quietly add new
   purchasable armor nodes. Author/validate cap boundaries for irregular hulls.
4. Store chart area weights, seams/adjacency, normals, render-contact-to-chart
   mapping, and coarse component-region ownership. Atlas UV square area is not
   automatically physical area; carry the metric/area weights explicitly.
5. Validate every appearance and archive the accepted envelope measurements.
   A uniformly scaled hull by factor s must have area s² and corresponding
   armor mass s² at fixed thickness. Nonuniform variants need actual measurement.

Combat presentation scale, camera zoom, native cinematic enlargement and the
mod's ship/projectile render-scale settings must not multiply physical armor
area. Art-variant dimensions in canonical metres do. Freeze the armor metric to
the authored reference pose; animation should move the surface, not create and
destroy armor capacity.

For target cell pitch h metres and armor-node area S:

```text
N_equivalent ≈ ceil(S / h²)
N_u ≈ ceil(L_u / h), N_v ≈ ceil(L_v / h)
```

Use masks/area weights for nonrectangular charts; the final cell areas sum to S.
Keep a comparable **physical** resolution across variants. A fixed 64×64 array
for every ship would give large ships much larger damage patches and conceal the
very size effect being requested. Adaptive storage need not allocate all
N_equivalent leaves; its logical extent still derives from S.

Proposed mass/cost convention:

```text
initial depth d0 = armor points * native material metres-per-point
armor mass kg = rho * sum(cell_surface_area * d0)
construction cost = native material weights applied to that mass
```

This is a panel-prism convention on the reference hull surface, not a claim to
compute an exact finite-thickness offset solid around an irregular hull. At
400 Hybrid points, thickness is 10 m, so the difference from a curved-shell
volume is material. Compare and explicitly accept that convention before
changing costs; do not claim thin-shell physical accuracy at that thickness.
Preserve native point-to-thickness relationships, including the approved alien
material values. Do not automatically scale maximum permitted **depth** with
appearance size; that is a separate design decision.

Replace the class-cylinder mass/coverage calculations with the same prepared
surface convention throughout designer mass/cost, integrity, repair and AI.
Otherwise the player pays for one area but fights with another. Static design
armor mass may remain on the ship mass ledger after damage in the first version;
physical shedding/thrust changes would require a separate approved lifecycle rule.

## Granularity must resolve the damage, not set its size

The user's observation is correct for a model that damages exactly one complete
cell: more cells reduce repeat-cell probability. However, that rule also changes
the physical size of every wound when resolution changes. It cannot be the final
area model if projectiles and beams have independent footprints.

For surface area S and a previously damaged physical patch of area F, a uniformly
sampled **point center** lands inside that patch with probability F/S. It is not
`1/N` unless the patch happens to equal one cell. Two finite footprints can
partly overlap even when their centers lie in different cells; overlap must be
evaluated in physical space. With repeated aim at a particular location, neither
uniform probability applies.

Example: on 1,000 m², a 0.01 m² scar has a uniform center-rehit probability of
0.001%, whether represented by one cell or 100 smaller cells. For a uniform
100 MJ deposition over that patch in Hybrid, with `rho*H=160,000 MJ/m³`, depth
loss is 0.0625 m. Spreading the same energy over an entire 1 m² cell instead
gives only 0.000625 m. That 100-fold difference is a numerical artifact.

Require convergence: halving h with fixed physical inputs should leave removed
volume, scar area, penetration energy and repeated-hit results nearly unchanged.
Proposed acceptance tolerance is 1% for integrated energy/volume/area and a
separately reported near-boundary penetration error. Do not demand identical
binary penetration exactly at a threshold, where small integration errors matter.
Use common shot centers/seeds when comparing resolutions.

For sparse adaptive tiles, start with proposed 2 m macrotiles and refine only
where depth/footprint boundaries need it. Resolve small cores relative to their
physical radius, not by a universal one-centimetre allocation across the hull.
Uniform interior blocks can remain coarse. Partially covered boundary leaves
must split or retain area/depth fractions with bounded error; averaging a tiny
hole into a large uniformly thinner cell would reintroduce the problem.

## Impact location, aiming and precision

**Recommended live-combat rule: the actual contact determines the armor location.**
Change aim before the shot is fired, let the projectile travel, then use the
contact on the target's surface. Do not show a nose strike while secretly applying
it to a random aft tile, or roll a second random tile after an accurately aimed hit.

| Approach | Consequences |
|---|---|
| Actual contact → atlas coordinate | Facing, maneuver, visible impact and local damage agree; supports drilling and component targeting; needs reliable contact metadata |
| Contact selects node; random point inside node | Cheapest compatible abstraction, but precise aim cannot reliably revisit visible damage or select underlying components; can make center-focused beams unrealistically spread across the hull |
| Random point over projected visible surface | Reasonable for abstract/autoresolve attacks when correctly weighted; does not reproduce individual visible tactical trajectories |

Use the first for tactical combat and the third for autoresolve or source paths
without a physical contact. For abstract sampling, weight candidate surface
patches by `area * max(0, -normal·incomingDirection) * visibility * aimDensity`,
then sample a point within the selected patch. Uniform node-area sampling is only
an explicitly chosen broad-aim special case. Never sample uniformly over atlas
UVs or tile indices when physical areas differ.

The current ballistic query returns contact point/normal/collider, but its
prepared triangle traversals do not expose triangle ID/barycentric coordinates.
Plan to return that metadata from the **existing winning intersection**, map it
to canonical surface coordinates, and carry it into armor resolution. Do not
add another whole-mesh search/raycast at impact. Skinned contacts need original
triangle identity and pose-to-reference mapping; a world point alone is not a
stable material coordinate on an animated surface. The existing HullSection →
ship-state boundary currently discards detailed contact position and must change.
Beam intersection must likewise use the nearest valid target surface and a
defined failure path, rather than relying on the current first-collider/default
point behavior.

### Aiming at components

Allow an optional desired region such as propulsion, nose weapons or central
systems. Aim at the **exterior covering that region**. Armor still stands between
the attack and the component, and another surface can occlude the desired point.
Do not grant knowledge of exact hidden component coordinates/armor pixels merely
because the simulation stores them. Start with known coarse regions; exact
component coverage requires an authored layout and a sensor/knowledge rule.

Nose weapons retain their pivot/forward-facing restrictions. A gun selects an
intercept and launch-direction perturbation; it cannot steer toward a component
after firing. Lasers can aim more precisely and avoid ballistic flight-time lead
error, while their optical spot remains a separate energy-distribution footprint.
Do not equate optical diffraction/jitter with target-tracking accuracy.

Proposed direction-error model, evaluated only when firing:

```text
sigma_angle² = sigma_hardware² + [sigma_tracking/(1 + k*officer_rank)]²
sigma_target² ≈ (range*sigma_angle)² + sigma_target_state²
               + (flight_time*sigma_velocity_estimate)²
```

The final term applies to ballistic prediction; tracking latency can remain for
beams. `k=0.1` is a trial sensitivity, not selected tuning. Sample a two-axis
angular offset using a stable shot seed, preserving muzzle-speed magnitude.
Correct lead/aim changes resulting geometry; rank never multiplies energy.
Initially prefer this officer-precision benefit over an additional independent
critical roll, which risks double-counting skill and restoring arbitrary bypass.

If all weapons keep aiming at the center perfectly, a spatial model can produce
*more* concentrated damage than the current random-coverage model. Precision is
therefore essential to calibration. For a 5 cm-radius damaged patch and a centered
isotropic Gaussian aim distribution, the probability the next shot center falls
inside is `1-exp(-r²/(2*sigma²))`: 95.6% for sigma=2 cm, 3.08% for 20 cm, and
0.0312% for 2 m. These are illustrative errors at the target, not proposed weapon
accuracy stats. Finite spot overlap still needs the footprint calculation.

Target selection should use prepared component-region candidates and cached
coarse damage/observation summaries, at acquisition/salvo boundaries. Do not scan
every damaged tile every frame or give AI perfect knowledge of enemy pinholes.

## Energy accounting and the conservative baseline

Use joules/MJ as the source of truth. Kinetics use surviving projectile mass and
relative impact velocity; beams use emitted/intercepted pulse energy; explosive
weapons add their explicit warhead yield. Remove the independent 0.8–1.2 direct
and chipping damage rolls. Randomness belongs in aim/contact and genuinely
unresolved internal placement, not extra physical energy.

```text
kinetic E_delivered_MJ = 0.5 * m_kg * |v_relative_km_s|²
E_coupled = 0.8 * E_delivered                 proposed conservative baseline
E_other = 0.2 * E_delivered
```

Keep E_delivered unchanged in physical telemetry. The 0.8 is an explicit gameplay
damage-coupling convention: the remainder is non-damaging/unmodeled heat, ejecta
or other loss. It is not a claim that the bullet had fewer joules. An alternative
is a 25 MJ damage unit instead of 20, but that would require a separately
consistent armor-work conversion; do not apply both and accidentally double-buff.

For comparison candidate **C0**, retain the current kinetic core/halo energy
shares initially: `c=flatChipping`, `E_core=(1-c)*E_coupled`,
`E_halo=c*E_coupled`. These become actual local damage kernels, not abstract hole
probability growth. This gives 80% of the old nominal direct budget, equivalent
to 25% more intact stopping capacity in the calibrated reference case.
Simply changing full damage to `0.8*P` while deleting the old 35% diversion would
instead raise the ordinary spinal's direct budget from 272 to 335 points.

Officer damage boosts are removed. Audit faction effects separately: a project
called Vital Point Shell Targeting should logically change targeting rather than
add kinetic energy. Genuine hardware power/mass/speed improvements can change
the physical input once. The numerical candidates below exclude old abstract
faction damage multipliers; retaining them would require retuning the protection
envelope, not quietly claiming the same results.

## Shared patch solver and weapon footprints

Let d be remaining normal thickness in metres, rho material density, H native
vaporization MJ/kg, and b the **actual overlap area** of a footprint with a
surface patch. Let Q be effective removal/penetration work per m³:

```text
W = Q * b * d                              work to remove that covered column
E_absorbed = min(E_allocated, W)
removed_depth = E_absorbed / (Q*b)
d_new = max(0, d - removed_depth)
E_residual = E_allocated - E_absorbed
```

Each channel's energy allocation integrates its normalized physical footprint
over the surface. Portions missing the ship are not redistributed onto the
remaining hit cells. A wide spot crossing holes, intact armor and silhouette
edges must produce the corresponding split; do not give every touched tile the
whole shot. Handle interception once, rather than multiplying native cross-section
loss and exact footprint clipping twice.

For incidence cosine mu, the surface footprint of a perpendicular beam/core area
F expands to approximately F/mu locally. Using that expanded area in W already
accounts for the longer material path; **do not also divide depth by mu**.
At grazing angles use actual clipping/visibility or a defined glancing path,
rather than an arbitrary minimum cosine that increases penetration. Curved charts
need per-patch metric/normals and seam-aware footprint traversal.

### Kinetic core and halo: a calibrated first model

Use a compact core plus a surrounding damage halo. The initial core cross-section
comes from the authored projectile diameter or the current mass-derived geometry;
the latter is a gameplay estimate used by PD, not a measured penetrator nose shape.
Start with a top-hat core and halo annulus; smooth radial profiles can be compared
later without changing the energy/area contract. A proposed outer halo area is
four times the core area, with the core excluded from the annulus.

Purely vaporizing the physical projectile cross-section is not an adequate
ballistic law. For example, with Q=rho*H and the current mass-derived diameters,
the heavy and spinal siege rounds' 80%-energy limits are only about 108 and 135
armor points at muzzle speed, respectively. That destroys their intended
penetration roles. Real impact behavior also involves deformation, fragmentation
and spall, and depends on projectile/target geometry and impact conditions;
[NASA's impact overview](https://hvit.jsc.nasa.gov/hypervelocity-testing/impact-physics.html)
supports treating this as a calibrated mechanical-work approximation, not calling
vaporization energy a validated kinetic penetration formula.

Proposed C0 calibration uses:

```text
Q_core = kappa_weapon * rho * H
kappa_weapon = 0.005 m² / F_core_reference
Q_halo = rho * H                           initial thermal/removal approximation
```

At normal incidence on intact armor, native `rho*H*d_per_point=4000 MJ/m²`, so
the reference core costs exactly 20 MJ per armor point. Thus first-hit thresholds
remain comparable to the old direct channel at its minimum roll, while damage
becomes localized. Kappa is an explicit **gameplay work calibration**, not a new
measured material constant. Keep it fixed to the authored reference projectile;
do not recompute it from tile resolution, closing velocity or the enlarged C1
footprint below. Otherwise those effects would cancel out.

This normalization deliberately does not infer all penetration physics from the
PD diameter model. A later alternative could author mechanical work/penetrator
shape profiles from scratch, but it needs its own calibration matrix. Material
kinetic/chipping resistance can be represented by greater required work (e.g.
Q divided by a 0.75 damage multiplier), rather than deleting delivered joules.
That conversion changes post-threshold residuals and must be compared separately;
the numerical tables here use Hybrid, whose kinetic multiplier is 1.

### C1: satisfy the heavy-battery protection requirement

C0 alone fails the clarified goal: the Gen3 Alien Heavy Mag Battery has a
432-point conservative core at muzzle speed, about 756 at +5 km/s closure and
1,170 at +10. A small global adjustment cannot stop those on 400 armor while
otherwise retaining identical impact concentration. An additional role/footprint
change is required; hiding it in grid resolution would obscure the balance.

Propose comparing a **diffuse battery-ammunition impact profile** with a coherent
nose-penetrator profile. Broaden the battery core/halo while holding its energy
and calibrated work coefficient fixed. One unapproved, smooth trial rule is:

```text
g_battery = 1 + 2*sqrt(reference warhead mass / 120 kg)
F_core_battery = g_battery * F_core_reference
F_outer_halo = 4 * F_core_battery
g_coherent_nose_penetrator = 1
```

The current heaviest alien battery is the 120 kg reference, giving g=3. This is
an authored ammunition-construction/impact-spreading profile, **not a physics
penalty applied because a gun happens to sit on the hull**. The same projectile
fired from a different mount must retain the same profile. The research calculator
uses current hull/nose categories as a provisional assignment only; implementation
must define explicit profiles and inspect all templates.

The square-root rule leaves power headroom: penetration continues to grow with
mass and velocity; it has no absolute damage/armor cap. It is a balance hypothesis,
not a measured cratering law. It also changes battery behavior substantially more
than the modest 0.8 general baseline. Compare it against individually authored
profile families before approval; do not present the entire rework as only a
20% adjustment.

| Weapon | C0 fresh-armor threshold at muzzle speed | C1 threshold at muzzle speed | C1 threshold at +10 km/s closure |
|---|---:|---:|---:|
| Mk2 medium coil battery | 18.7 | 10.3 | 52.9 |
| Mk2 heavy coil battery | 48.6 | 22.6 | 100.5 |
| Mk2 medium nose coil cannon | 52.7 | 52.7 | 234.7 |
| Mk2 heavy nose coil cannon | 89.9 | 89.9 | 374.6 |
| Mk3 heavy hull coil battery | 67.3 | 30.4 | 120.6 |
| Gen3 alien medium mag battery | 149.8 | 62.0 | 195.5 |
| Gen3 alien heavy mag battery | 432.5 | 144.2 | 390.2 |
| Mk3 ordinary spinal coiler | 217.9 | 217.9 | 711.0 |
| Mk3 heavy siege coiler | 428.7 | 428.7 | 3,113.4 |
| Mk3 spinal siege coiler | 651.3 | 651.3 | 4,360.0 |

These are points of intact Hybrid-equivalent protection breached by the **core**,
not internal damage. Normal incidence, no abstract damage bonuses, no PD mass
loss, and pristine armor. The C1 example protects 400 points from the selected
heavy batteries across a trial 0–10 km/s closure interval, while preserving nose
headroom. It does not promise immunity at arbitrary velocity or after erosion.
The highest battery at +10 is close to the boundary; verify the actual minimum
"nearly 400" design, all other weapon families, retained physical upgrades and
other materials before treating this as an accepted margin.

The 15–20-point mid-game reference remains distinct: medium Mk2 batteries can
wear it down; the heavy Mk2 battery can just breach it at muzzle speed; Mk2 nose
cannons remain substantially more dangerous. Existing data already demonstrates
the separate ladder: Mk2 heavy nose coil cannon has 112.3 nominal direct points
versus Mk3 heavy hull battery's 84.2. Do not translate every user reference to
"tier" into weapon Mk; compare actual mount grade and technology explicitly.

### Lasers: ablation before transmission

Keep optical spot size/range/interception, but remove the current `A^1.5*E`
armor subtraction and its chipping fraction from the new armor solver. For an
intact covered patch use thermal work `Q_laser=rho*H` and material absorption
alpha (trial preservation of Hybrid's 0.75 laser multiplier):

```text
E_absorbable = alpha * E_patch
depth removed = min(d_old, E_absorbable/(rho*H*b))
```

For a clean first candidate, a pulse encountering positive old thickness ablates
that patch; any excess after opening it is booked as surface/plume loss, with no
same-pulse internal transmission there. Subsequent pulses through an already
open patch transmit their allocated energy. Already-open and still-covered
parts of a wide footprint are evaluated separately. This makes drilling a
sequence, not the current immediate penetration shortcut. It is an explicit
gameplay pulse rule, not a claim about real laser drilling time.

Pure optical fluence may still drill too quickly. At 400 km the native 960 cm
green phaser's spot is only 0.000264 m². Its 400 MJ pulse, after 0.8 coupling and
0.75 intact absorption, removes about **227 armor points at the exact same spot**
under the simple thermal model: 400 points breach in two pulses, and the third
can transmit. Laser precision makes this a crucial balance case.

Compare a finite effective thermal/swept area:

```text
F_damage = F_optical + F_thermal
```

A trial `F_thermal=0.005 m²` changes that example to 11.4 points per pulse,
36 perfectly repeated pulses to breach, first transmission on pulse 37.
This is a proposed spreading parameter, not a measured diffusion coefficient;
zero and nonzero cases bracket a very large balance difference. It affects damage
deposition, **not optical interception**. Wider spots still spread energy across
more armor and smaller optics still matter, but sufficiently tight beams reach
a thermal-area floor. The final parameter must be tested against aperture,
wavelength, pulse energy and cadence progression rather than chosen solely to
make one Titan live longer.

### Missiles, torpedoes, explosions and particles

Give each weapon an explicit core/jet/blast/fragment energy partition whose
fractions sum to at most one. Broad blast energy is spread over its intercepted
solid angle/surface footprint; a shaped penetrator concentrates an explicit
fraction into a smaller core. Do not multiply full warhead yield by every touched
tile, or assume that a larger blast automatically penetrates more deeply.
Torpedo penetration must follow its yield/concentration, not an unconditional
"ignore armor" flag. Retain dedicated anti-capital roles through those profiles.

Represent many fragments with bounded, energy-weighted groups or an integrated
kernel, not one managed object per fragment. Validate penetration variance as
well as total energy against finer samples. Particle heat and radiation remain
distinct; transmission uses local thickness/half-value depth, with the existing
6.25% clamp reviewed separately rather than silently carried into a new model.
Legacy integer shredding needs a defined energy/footprint adapter, including
strategy-layer calls with no weapon or contact. Do not invent joules for such
events without documenting their source convention.

## Internal damage and exit energy

Armor cannot meet the anti-one-shot goal solely by stopping more first hits.
For kinetics, propose an initial **internal deposition fraction eta=0.25** of
residual core energy; the remainder continues as projectile/debris energy toward
an exit or is accounted for in a defined loss channel. This replaces unlimited
recursive internal damage; it must not be layered on top of that router and then
charged again. Use the existing 20 MJ per damage-point conversion only for energy
actually assigned to components. Explosive/laser deposition fractions can differ
and need separate calibration.

At muzzle speed, C0/C1 coherent nose profiles give these illustrative ordinary
internal budgets before defensive modifiers and secondary explosions:

| Impact | Residual core points after armor | Internal points at eta=0.25 |
|---|---:|---:|
| Heavy siege Mk3 into 200 armor | 228.7 | 57.2 |
| Heavy siege Mk3 into 400 armor | 28.7 | 7.2 |
| Spinal siege Mk3 into 400 armor | 251.3 | 62.8 |

These improve the ratio to 24-point Battlecruiser / 64-point Titan structural
systems without guaranteeing a hit count. Routing, holes, multiple projectiles,
high closure and secondary explosions can still be lethal. A torpedo or major
magazine failure can remain exceptional; repeated routine cruiser shots should
not reproduce those exceptional outcomes.

For an initial coarse interior, use a bounded entry-to-exit sequence of authored
regions, with energy-weighted component interaction only within the traversed
regions. Reuse weighted native component tables locally until explicit coverage
is authored, but do not let their random recursion teleport the remaining energy
through arbitrary regions repeatedly. Components receive at most their allocated
deposition budget; excess/overkill must be returned to the energy ledger.

Apply remaining exit energy to the exit patch through the same work solver,
potentially with a broader debris footprint. Remove the native
`remaining damage * original chipping amount / volume` path. No duplicated exit
energy, free exit hole, or unaccounted extra internal reapplication is allowed.
Secondary explosions have separate, explicit energy sources; otherwise the
bookkeeping equation is:

```text
E_delivered = E_armor_work + E_internal_deposited + E_escape
              + E_reflected_or_surface_loss + E_other
```

## Runtime design and cost limits

All geometry/envelope extraction, surface areas, chart mappings, region masks,
adjacency, kernel lookup data and shared asset metadata are prepared offline or
at load time. Runtime state uses indexed struct arrays/pools; no per-tile Unity
objects, colliders, transforms, textures or per-frame mesh baking.

- **No-hit frames:** no armor-grid traversal or depth updates. Cached summaries
  serve HUD/AI; update dirty regions only after damage/repair.
- **Small impacts:** one existing contact query plus an atlas lookup and traversal
  of the local footprint. Cost is proportional to intersected adaptive leaves,
  not total hull area. Keep a global shot sequence for deterministic mutations.
- **Wide footprints:** update fully covered uniform blocks analytically; traverse
  mixed/boundary blocks. Stencils and chart adjacency are shared/prepared. A large
  explosion over heavily fragmented state is the worst case and must be measured.
- **Memory:** pristine regions are implicit. Dense float depth alone on a
  hypothetical 50,000 m² ship costs 0.2 MB at 1 m pitch, 20 MB at 10 cm, and 2 GB
  at 1 cm; 200 ships multiply those costs. This is why fine dense arrays are not
  recommended. With 16-byte pooled tree nodes, 10,000 distinct impacts averaging
  100 added nodes would instead use about 16 MB of mutable state **in total for
  that example**, plus roots/indexing/shared metadata. Node count is an assumption,
  not a measured guarantee; overlapping/complex damage can differ substantially.
- **Allocation:** prepare pool capacity from fleet/test bounds. Do not allocate
  managed objects per impact. Near-uniform siblings may merge only within a
  stated depth/volume tolerance; never heal holes merely to stay within a budget.
- **Load/memory limits:** benchmark before selecting caps. If a hard resource cap
  cannot preserve error limits, revise the representation rather than clipping
  affected tiles, spreading energy over untouched cells, or changing armor with
  battle load. The histogram alternative is a deliberate design fallback.

Proposed benchmark gate: no added whole-fleet recurring work and no per-impact
GC allocation; target at most 0.25 ms aggregate armor work for a 50-small-impact
frame on the user's machine, with broad-blast and 15× battle spikes reported
separately. This is an ambitious **acceptance target, not a performance result**.
Include added contact metadata, indexing, summaries, worker synchronization and
worst-frame cost. Moving a delay to a worker and immediately waiting does not
meet the budget. Do not reorder impacts or postpone damage across simulation
frames to conceal overruns.

## Save, repair, UI and other consumers

Physical inputs must be captured before native `Ship.Damage` randomizes them;
do not try to reconstruct joules from the already randomized direct/chip fields.
The eventual integration needs an explicit impact context shared by ballistic,
beam, blast and simulated-combat entry points, carried through the current
Hull/ArmorData information-loss boundary. Reuse the existing contact result;
avoid per-hit object allocations and unsafe global state during nested explosions.

Exposed radiators, sensors and drives must use actual exposed component contacts
or explicitly sampled exposed surfaces. Keeping native independent pre-armor
damage rolls on an otherwise armored hull strike would undermine the new physical
coverage model. An exposed hit spends its energy once; it must not also deliver
a duplicate full-energy hit to armor behind it.

Persist a versioned atlas identity, original material/depth and sparse damaged
state keyed by ship identity and armor node. Save numerical damage state, not
Unity objects. A deterministic seed may place legacy chipped coverage as holes
whose **area** matches old chippedPct, while old shredded armorValue sets the
remaining non-hole depth. Do not turn old holes into free restored armor or spread
every old hole uniformly into thinning without explicitly approving that change.

Appearance changes must reproject conserved remaining armor/depletion or use an
explicit refit replacement cost. They cannot heal damage, preserve a smaller
variant's cheap mass on a larger hull, or reroll hole placement on reload. Handle
atlas revision changes explicitly. Native save identity, FullSerializer behavior,
combat copies and mid-battle save support need a compatibility audit before code.

Repair restores missing material volume/work by region; summaries should expose
remaining mass/coverage and depth distribution, rather than labeling one averaged
integrity number as thickness everywhere. A coarse heatmap is enough initially;
render only on demand from cached data. Update designer costs and AI valuations
for selected appearance/armor geometry.

Stations, PD projectiles, autoresolve and strategic damage need explicit adapters.
Ship armor changes must not accidentally make PD ineffective through shared laser
damage methods. Abstract combat samples the same prepared surfaces/aim model and
uses the same energy solver; it need not instantiate scene meshes. Every handler
must document whether it supplies a real contact, a sampled contact or an
energy-equivalent legacy event.

## Validation and decisions before implementation

1. Approve contact-driven armor versus the histogram fallback; retain the user's
   physical-area/variant requirements in either case.
2. Prepare actual per-node armor envelopes for representative small/large art
   variants before claiming concrete tile counts, mass costs or durability ratios.
3. Compare C0 and C1 plus authored ammunition-profile alternatives. C0 is a useful
   control but fails the clarified heaviest-hull-battery goal. C1 is an illustrative
   candidate that meets the selected pristine 400-point/0–10 km/s cases; it still
   requires broader testing and user acceptance of its battery-role changes.
4. Choose laser thermal/swept footprint and aim precision together. The two-pulse
   versus 36-pulse example shows why testing just laser energy or just grid size
   is insufficient. Set visible targeting/knowledge limits before enabling exact
   component or weak-spot aim.
5. Calibrate internal deposition and region routing; assess complete hit-count and
   time-to-disable distributions, including bursts/volleys, against both 15–20
   mid-game and maximum late-capital armor. Preserve the hull/nose power ladder.
6. Check energy conservation, no officer/RNG energy creation, cell-size convergence,
   partial footprint overlap, seams, silhouette clipping, glancing hits, repeated
   same-point hits, untouched-point independence, wide hits spanning holes, and
   pose/presentation-scale invariance. Halving resolution must not change armor
   strength by a large amount.
7. Check area/mass scaling, all weapon/material families, PD, station/autoresolve
   parity of rules, repair/refit/save round trips, UI/AI estimates and performance
   under sustained maximum-speed combat. Only then select final parameters and
   prepare implementation/build/deployment through the normal guarded workflow.

The [calculation snapshot](candidate-calculations-2026-11-04.json)
and [comparison figure](energy-and-targeting-candidates.png)
are **analytical planning artifacts**, not a tile-engine prototype or gameplay
validation. Calculations retain source hashes and distinguish candidate C0/C1,
physical energy, calibrated work and illustrative area/precision assumptions.
No runtime/gameplay implementation, build, deployment or commit occurred in this
planning update.

Analytical checks passed for 52 energy/threshold cases, twelve exact rectangular
footprint integrations at different grid pitches/depths, monotone mass headroom,
the laser pulse examples and source-hash stability. Local documentation links,
anchors and formatting were checked, and the figure was visually inspected.
These checks establish arithmetic consistency only; adaptive-grid correctness,
real surface measurements, full-battle balance and runtime cost remain untested.
