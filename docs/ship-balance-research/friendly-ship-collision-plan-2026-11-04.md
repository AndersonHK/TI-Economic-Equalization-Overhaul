# Ship overlap, collision damage and friendly avoidance

2026-11-04. **Plan only; not implemented.** This follows the
[collision performance work](battle-worker-contact-2026-11-04.md). The user asks
that physical ship collisions and friendly avoidance be addressed separately
after performance fixes are in place.

## Report and confirmed code distinctions

The user reports severe lag when accelerating ships overlap, no registered
collision damage (at least between friendlies), and ineffective friendly
avoidance. The supplied screenshot shows overlapping ship models. It does not
establish callback frequency, physical time of impact or the damage calculation.

The pinned native assembly distinguishes three systems:

- `CombatShipController.OnCollisionEnter` applies its ramming logic only when the
  other controller equals `primaryTarget`. It then uses mass ordering and can
  destroy the lighter participant. This is not general accidental-collision
  damage, and must not simply be enabled for every friendly overlap.
- `OnTriggerEnter/Exit` registers nearby ships with waypoint navigation.
  `WaypointNavigationController.AddAgentShipController` deduplicates controllers,
  but `RemoveAgentShipController` removes one on any exit, without counting other
  overlapping colliders. Multiple colliders can therefore generate a premature
  removal. This is a source-supported failure mechanism, not yet a reproduced
  explanation of the reported encounter.
- Native navigation has collision checking and avoidance code, including
  recalculation flags, staggered checks and an intentional-collision branch.
  The symptom does not mean that avoidance is absent from the game. Neighbor
  tracking, scaled bounds, prediction horizon and maneuver feasibility need
  separate verification.

The performance patch removes the live EEO mesh/bone damage-box hierarchy while
retaining native ship/station body colliders and existing ramming/navigation
callbacks. It does not introduce friendly collision damage or a new avoidance
policy. Weapon-contact faction filtering remains unchanged.

## Proposed sequence

1. Reproduce the reported crossing with two friendly ships, then vary faction,
   primary-target selection, speed, acceleration and timestep. Record body-pair
   overlap, trigger enter/exit counts, neighbor membership, predicted contact time,
   avoidance attempts and damage events. Distinguish missed detection, deliberate
   damage gating and an avoidance maneuver that is physically infeasible.
2. Repair neighbor lifetime at body level. Either maintain overlap membership
   until the last relevant collider exits, or replace trigger-derived neighbors
   with a simulation spatial query. Ensure one stable body ID and generation per
   ship/module, clean removal on destruction, and no dependence on damage-mesh
   collider count. A regression case must enter with two colliders and exit with
   only one while the other still overlaps.
3. Define the physical-contact contract separately from weapon raycasts. Ship
   collisions need finite-volume, relative-motion sweeps and a time of impact;
   the projectile centerline test is insufficient. Precompute hull/part geometry
   and conservative bounds during loading, then refine candidate body pairs on
   workers. Include turning, acceleration and high-speed crossings between
   sampled poses. Do not treat broad bounding-box corners as physical hulls.
4. Define accidental-impact consequences before changing damage. Audit native
   `CollisionImpact` units and energy/mass handling. Propose a bounded accidental
   collision response distinct from the intentional-ramming destruction branch.
   Friendly ships must participate in detection; that does not authorize changing
   friendly projectile damage. Radiator-only contact versus main-hull contact
   also needs an explicit rule rather than automatic whole-ship destruction.
5. Improve avoidance at maneuver proposal time and simulation updates. Use
   predicted swept paths, attainable acceleration/turning and available delta-v.
   Resolve reciprocal conflicts with stable priorities so both ships do not
   repeatedly choose the same evasive direction. Apply navigation clearance only
   to steering, not to physical damage admission. Intentional ramming may exempt
   the chosen target from avoidance, while still avoiding other friendly ships.
6. Commit each physical contact once per body pair/contact episode. Avoid damage
   duplication from multiple parts and repeated callbacks. Keep detection and
   steering off the rendering path; measure main-thread event application and
   waits under the same performance standard as the current work.

## Acceptance cases

| Case | Required evidence |
|---|---|
| Two friendlies accelerating through each other's paths | Neighbor remains registered; feasible avoidance occurs; actual contact is detected if avoidance fails |
| One collider exits while another remains | No premature neighbor removal |
| Crossing entirely between frames, including 15x | Swept finite-body detection does not tunnel |
| Hostile ship selected/not selected as primary target | Intentional ramming remains distinct from accidental contact |
| Empty broad-box corner or near miss | No physical damage from empty space |
| Radiator/turret versus main hull | Contact classification follows the approved damage policy |
| Several ships converging or already interpenetrating | Stable resolution, no callback storm or repeated damage loop |
| Station approach, reinforcements, pooling and battle exit | Stable identities and cleanup; no stale neighbors |

Rendered in-game reproduction and behavior acceptance are still required. This
plan intentionally leaves damage-policy changes for review; none are smuggled
into the performance release.
