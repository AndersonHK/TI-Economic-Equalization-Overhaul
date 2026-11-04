# Armor rework plan

Status: **one global armor-work divisor; architecture/performance investigation only**.
Updated 2026-11-04. The [as-is audit](armor-system-as-is-2026-11-04.md) owns current
mechanics, and its [late-game coiler cases](armor-system-as-is-2026-11-04.md#12-late-game-coiler-lethality-400-armor-titans-and-200-armor-battlecruisers)
explain the reported capital-ship lethality. This document owns the emerging
rework requirements. It does not authorize implementing unspecified formulas.

The separately requested [alien material update](alien-armor-material-update-2026-11-04.md)
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
[design directives](../design-directives.md#armor-weapon-energy-and-officer-skill).
The user rejected the previous multi-parameter calibration. Armor strengthening
uses **one uniform divisor**. Its final value is not selected; no implementation
or measured runtime-performance result is claimed.

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
stores **volume**, not surface area. The [existing mesh report](hull-volume-capacity-comparison-2026-11-03.md)
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

## One balance parameter: the armor-work divisor

User correction, 2026-11-04: **increase the divisor used to calculate armor points
removed by an impact, uniformly for every impact and armor material.** Do not
replace that request with a battery-versus-nose calibration, projectile-specific
work factors, extra laser spreading chosen for balance, or a new internal-damage
mitigation coefficient.

Use a single parameter D. In reference-area units, D is MJ required to remove
one armor point across the existing 0.005 m² reference patch. It replaces the
baseline 20 MJ in this armor-work conversion. For allocated impact energy E_i,
remaining armor points A_i and actual covered area S_i:

```text
area_factor_i = S_i / 0.005 m²
work_per_point_i = D * area_factor_i
removed_points_i = min(A_i, E_i_MJ / work_per_point_i)
armor_work_i_MJ = removed_points_i * work_per_point_i
residual_energy_i_MJ = E_i_MJ - armor_work_i_MJ
```

Equivalently, if the selected physical/footprint model already calculates an
unscaled loss, `scaled_loss = unscaled_loss / K`, where `K = D/20`. **D and K
are two notations for the same control, not two settings.** Precompute the
reciprocal or normalized coefficients at initialization; the hot loop need not
perform divisions for every patch.

| Example divisor | Uniform loss relative to D=20, before clamping at zero armor |
|---|---:|
| 20 MJ | 100% |
| 25 MJ | 80% |
| 30 MJ | 66.7% |
| 40 MJ | 50% |

These are examples, not a selected value. The same scalar applies to nose, side
and tail armor, every material, and every weapon's armor-removal channel. Preserve
intended material properties and physical energy/footprint differences. A larger
divisor uniformly increases work per point; it does not imply every battle lasts
the same percentage longer because breach thresholds, aim and overlapping scars
remain nonlinear.

The reference-area expression follows the native material relation:
`rho * H * depth_per_point = 4000 MJ/m²`. In depth form, the same change is
`removed_depth = E_i / [K * rho * H * S_i]`, capped to remaining depth.
Thus it does not require changing authored thickness, density, armor mass or
material-cost weights. The approved alien material values remain intact.

**Keep physical delivered energy unchanged.** A bullet still carries ½mv² at its
actual relative impact velocity. Remove the random damage-energy roll and
officer energy multipliers under the prior agreed direction. If we choose the
old minimum-roll equivalent of 80%, express it as D=25 rather than applying both
0.8 energy coupling and D=25. The previous proposal's independent 0.8 coupling
setting is withdrawn. This armor-work conversion is not a declaration that the
physical bullet contains less energy or that all internal HP should also receive
a new divisor.

Allocated energy means energy actually assigned to the armor channel by the
chosen interaction model. Do not silently pool previously separate channels and
then call the result an 80% reduction. The native system's existing direct/chip
split and its separate penetration rule need an explicit integration decision;
simply increasing the old `chippedPct` denominator would only slow coverage loss
and would not strengthen its independent direct-penetration threshold. In the
new local-work solver, erosion and breakthrough should share the same work ledger
so the single divisor applies consistently.

If testing shows one global value cannot satisfy every desired matchup, report
the tradeoff. Do not quietly add weapon-specific multipliers to force the tests
to pass. Hull/nose progression, 15–20-point mid-game armor and maximum capital
protection remain the evaluation cases, not separate calibration knobs.

## Architecture decisions remain separate from the divisor

Preserve the physical-area, actual-contact, precision and variant requirements
above. An impact footprint still distributes a finite energy budget across the
covered patches. Larger projectiles, wide beams and blasts can affect multiple
tiles. These are geometric/weapon-interaction inputs, not extra armor-buff
settings. Do not enlarge a particular family's footprint just to meet a desired
penetration threshold.

Sparse adaptive tiles remain the preferred spatial architecture **if the patched
engine can execute them within budget**. It is reasonable for a hot contiguous
array loop to perform hundreds of arithmetic operations cheaply. The unresolved
cost is the full patched path: fetching state, identifying the contact region,
adapting the native damage call, processing local data, and publishing the result.
Array size alone is not evidence that the model is too expensive, and tree
sparsity alone is not evidence that it is efficient.

Keep these interactions open until the representation and engine integration are
settled:

- Laser ablation, transmission through pre-existing or newly opened holes, and
  physical optical footprints. No extra thermal-area floor is selected for tuning.
- Kinetic/explosive footprints and any existing energy partitions. No C0
  per-weapon work normalization or C1 battery spreading profile remains selected.
- Coarse versus explicit component coverage and internal energy propagation.
  No new 0.25 internal-deposition factor is selected.
- Exit armor must spend the remaining energy through the same work ledger,
  without preserving the native damage-times-chipping amplification or creating
  energy. Detailed internal/exit behavior is an architecture question, not a
  second balance coefficient introduced by default.
- Existing material specialties, radiation channels and legacy shredding need
  adapters that preserve their intended meanings and apply the single armor-work
  scalar consistently wherever armor material is removed.

The [rejected calibration and its figures](archive/armor-rework-calibration-2026-11-04/rejected-calibration-plan.md)
are archived for provenance. Their numerical promises about battery protection,
laser pulse counts and internal damage **must not be used to describe this plan**.

## Engine integration and performance: a hard gate

User requirement: **another 1 ms of frame time for armor at around ten impacts
per frame is unacceptable.** The budget covers the complete added path, not just
the numerical solver. Performance is non-negotiable; the ideal spatial model is
conditional on demonstrating an efficient patch against this engine.

Working engineering target, proposed rather than user-specified: at ten impacts
per frame, aim for no more than **0.10 ms total added armor cost at p95**, with
**p99 below 0.25 ms** and worst-frame spikes reported. A result near 1 ms fails
even if the isolated arithmetic kernel is fast. Those targets are not measured
results or a promise that the current integration can meet them.

### Hot-path shape

1. **Enter once at impact.** Capture physical energy/contact data before native
   randomization and replace the superseded armor calculation. Avoid running the
   native resolver and a second spatial resolver for the same hit. Preserve only
   required native side effects through an explicit adapter.
2. **Use prepared state handles.** Bind ship/material/appearance state at creation
   or load. Carry a validated integer/reference handle from contact into damage
   processing. Avoid repeated reflection, Harmony discovery, template searches,
   `GetComponent`, string-key lookups or LINQ inside the armor loop. Cache guards
   and compatible accessors during initialization.
3. **Reuse the winning contact.** Existing `BallisticContactRuntime` already calls
   `PreparedContactWorld.Cast`. Extend the existing winning intersection with
   chart/triangle metadata if needed; do not repeat its body traversal, pose reads,
   BVH search or mesh query just to find an armor tile. For beams/abstract combat,
   define an equally narrow entry path. Never search the whole hull at impact.
4. **Operate on contiguous numeric storage.** Compare fixed-size chunks and flat
   adaptive-node arrays with integer child indices. Keep hot depth/child/flag data
   compact and cold metadata separate. No object-per-tile tree, virtual interface
   call per cell, per-cell engine object, or cache-unfriendly pointer graph.
5. **Allocate before combat.** Prepare pools and scratch buffers; split/refine by
   taking array slots, not allocating managed objects. No list resizing, dictionary
   growth, array copies or temporary collections per impact. A direct macrocell
   index can resolve a sparse block without a chain of associative lookups.
6. **Touch only the footprint.** Process a small contiguous stencil or local node
   traversal; integrate uniform fully covered blocks directly. Logical arrays may
   have thousands of entries without requiring thousands of accesses every hit.
   Count actual reads/writes and refinement work, not just floating-point operations.
7. **Publish once.** Update compact node summaries incrementally, mark UI state
   dirty, and reuse the existing notification cadence. No event per tile or
   immediate heatmap rendering/full integrity scan after every impact. Nested
   secondary impacts require bounded/re-entrant context, not an unsafe global slot.

Keep untouched armor implicit and geometry shared per appearance, but benchmark
whether modest dense chunks beat a fully adaptive traversal. Sparse storage is
useful for memory; it is not a reason to sacrifice locality. Preserve physical
footprint/resolution invariance whichever numeric layout wins.

### Benchmark the integration before committing to the architecture

No prototype implementation is authorized in this planning turn. For a later
approved feasibility pass, compare these paths on the user's actual Unity/Mono
runtime and machine:

| Measurement | What it isolates |
|---|---|
| Existing native/mod impact path | Baseline to subtract; includes work already being paid for |
| Empty replacement adapter with cached state/contact | Patch boundary and state-access overhead |
| Flat/chunked array kernel with recorded impact inputs | Arithmetic and memory locality without engine calls |
| Full candidate spatial path | Added contact metadata, state access, refinement, solver and notifications |
| Same adapter with node histogram | Whether simplifying representation helps; it will not fix expensive engine calls shared by both |

Use 0, 1, 10 and burst-impact workloads; repeated hot hits and scattered hits
across ships/variants; pristine, partially damaged and heavily fragmented armor;
small projectile/laser footprints and wide explosions. Report total added
main-thread time, p50/p95/p99/max, nodes touched, allocation/GC and memory high
water marks. Include normal and maximum game speed. A 10-impact frame budget is
not a per-impact budget, and dividing the average by ten cannot hide spikes.

Measure warmed combat steady state separately from preparation/first-use costs.
First-use geometry/reflection/JIT work must be moved to preparation where possible,
and any unavoidable first-impact spike must still be reported. Compare visible
frame pacing as well as custom timers. Do not allocate/log profilers per tile.

Prefer a simple synchronous managed kernel if it meets the target. A native helper,
jobs or workers are not automatically cheaper once transitions, copying and waits
are included. Introduce them only if measured evidence warrants the complexity.
Do not postpone damage, alter shot order or silently reduce spatial accuracy to
hide cost. If engine integration dominates, fix that boundary first; abandoning
tiles for a histogram does not by itself remove that overhead.

Prepare surface envelopes, areas, chart mappings, adjacency, footprint lookup
data and component regions offline/load-time. No armor processing on no-hit frames
apart from already-needed cached presentation. No extra per-frame skinning, pose
capture, physics synchronization, collider updates or whole-fleet scan for armor.

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

1. Keep the **single global divisor** as the only armor-strength tuning control.
   Choose its value from reproducible mid-/late-game cases after the base
   footprint/energy interpretation is explicit. No weapon-specific calibration
   may be added to satisfy a failed matchup without a new user decision.
2. Prove the patch boundary/state access is cheap before investing in a full
   adaptive surface system. Compare contiguous chunks/adaptive numeric arrays and
   the histogram using the same integration harness and timing scopes.
3. Prepare real per-node areas for representative art variants; verify armor
   mass/cost, logical extent and point-to-thickness consistency. Current volume
   catalogs are not substitute surface measurements.
4. Check conserved allocated energy, uniform divisor application, no officer/RNG
   energy creation, finite footprint overlap, seams, silhouette clipping, grazing
   incidence, repeated same-point hits, and cell-size/pose/presentation invariance.
5. Decide contact versus abstract sampling and component targeting explicitly.
   Preserve hull/nose power headroom and both the 15–20-point mid-game and heavily
   armored late-capital reference cases. Evaluate hit-count distributions, not
   only average damage; the rejected C0/C1 results are not evidence for this design.
6. Audit internal/exit propagation, exposed components, repair/refit/save state,
   stations, PD, autoresolve, AI estimates and UI. Keep correctness and performance
   gates coupled: passing one does not waive the other.
7. Only after a later implementation request and a successful feasibility result,
   proceed through the normal guarded build/deploy/manual-test workflow.

## Status

The earlier multi-parameter calibration is rejected and archived. The active
plan now uses one uniform armor-work divisor and treats efficient patch integration
as the deciding feasibility test for sparse adaptive tiles. Documentation only:
no gameplay code/data, runtime prototype, benchmark, build, deployment or commit
was made in this correction. Performance estimates are targets, not results.
