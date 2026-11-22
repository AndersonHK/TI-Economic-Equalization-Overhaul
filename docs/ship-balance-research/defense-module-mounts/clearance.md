# Orbital turret clearance and placement proposal

Measured on November 22, 2026 against the installed Terra Invicta 1.0.65 module shells and weapon prefabs. The counts below are weapon groups. Each group has a separately positioned top and bottom model.

[Human layouts at fitted scale](human-turret-clearance.png) · [Alien placement proposal](alien-turret-placements.png) · [Slot coordinates, model scales and measurements](clearance-results.json)

[Power and armor check](power-and-armor.md) compares the proposed draws with weapon recharge and the hull sizes with ship armor.

## Human modules

| Module | Batteries + PD | Physical positions | Pairs unresolved at native scales | Smallest fitted separation | Largest seating lift |
| --- | ---: | ---: | ---: | ---: | ---: |
| T1 Point Defense Array | 5 + 6 | 22 | 54 | 0.23 | 1.26 |
| T2 Layered Defense Array | 25 + 12 | 74 | 10 | 2.51 | 0.48 |
| T3 Battlestations | 46 + 20 | 132 | 16 | 0.35 | 1.07 |

Distances are in prefab-local units. Every fitted slot cleared the isolated hull and all other swept envelopes. The unscaled pair counts identify cases where the envelopes could overlap. A conservative envelope overlap alone does not establish mesh contact.

- **T1** centers five batteries on the large round markings and six PD heads on the small emplacements. The bottom mirrors the top pattern with separate seat heights from the underside geometry. The full-size small laser, railgun, coilgun and PD laser models fit. The tested bounds use the 30 mm model at 66% for a battery and 17% for PD, and the larger naval battery model at 87%.
- **T2** fills the 25 deck pads and 12 bevel pads on each half. The deck keeps full-size rail, coil, and laser models. I tested the naval fallback that uses the 12-inch prefab at 84% and the 30 mm model at 79% on the deck. Bevel PD uses the full laser head or a 33% version of the 30 mm model. Its local elevation starts at the sloped pad's tangent.
- **T3** fits one size-4 battery at each of the 46 rectangle centers and five PD heads along each edge. The end PD positions shift along the rim to clear the nearest panel battery and stay clear of the raised towers. The size-4 battery models fit at native scale. As on T2, the 30 mm PD fallback is tested at 33% scale. The figures cover the rectangular deck and edge PD proposal.

### Weapon points and proposed economy

Each battery contributes its slot size and each PD group contributes 1. The fully armed pre-0.12 loadouts have 3, 5, and 13 points respectively. The linear estimates multiply each tier's pre-0.12 EEO values by the proposed points divided by those points. The deferred figures follow the [0.12 design](implementation-0.12.md) after the weapon power comparison. The active build uses pre-0.12 power and base mass pending the broader module rework.

| Tier | Weapon points | Linear power | Linear base mass | Deferred power | Deferred base mass |
| --- | ---: | ---: | ---: | ---: | ---: |
| T1 | 5 × 1 + 6 = 11 | 36.67 | 550 t | 40 | 550 t |
| T2 | 25 × 2 + 12 = 62 | 496 | 9,300 t | 400 | 9,300 t |
| T3 | 46 × 4 + 20 = 204 | 3,766.15 | 94,153.85 t | 2,000 | 94,200 t |

## Alien modules

| Module | Proposed batteries + PD | Placement | Smallest fitted separation |
| --- | ---: | --- | ---: |
| T1 Alien Point Defense Array | 2 + 3 | Two outboard batteries and central PD at the existing endcaps, plus two PD heads on the rounded hull shoulders | 0.22 |
| T2 Alien Layered Defense Array | 8 + 6 | Eight batteries and four PD around the ring, with the other two PD on the existing polar pedestals | 1.59 |
| T3 Alien Battlestations | 8 + 6 | Four batteries on the central pyramid shoulders and four on the outer housings, with PD at the central pedestals and on the inner face of each outer pyramid | 3.47 |

T1 has the tightest fit of the six. I used the size-1 alien coilgun model at 57% of its native size between the endcap weapons. Alien T2 and T3 fit the measured weapon models at native scale. The raised housings on the four outer pyramids take short adapters with up to 2.55 units of lift. These proposed layouts use the visible geometry as the design input.

## Own-hull firing restrictions

The sweep checks measure mechanical clearance. The following independent check casts rays from the modeled muzzle against the same module's shell. Samples cover 16 azimuths at each elevation and one ray directly outward at 90°. Angles are relative to each mount's local tangent plane. Table entries are the number of physical positions with at least one blocked sample.

| Module | 0° | 10° | 20° | 35° | 60° | 90° |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Human T1 | 12 | 6 | 6 | 6 | 4 | 0 |
| Human T2 | 12 | 0 | 0 | 0 | 0 | 0 |
| Human T3 | 54 | 54 | 42 | 20 | 2 | 0 |
| Alien T1 | 4 | 4 | 4 | 0 | 0 | 0 |
| Alien T2 | 25 | 25 | 25 | 13 | 1 | 0 |
| Alien T3 | 24 | 24 | 16 | 16 | 16 | 0 |

Human T1's six small top emplacements sit in a recess roughly 2.84 units below the main deck. All six top PD heads lose some directions at 35°, and four lose a direction at 60°. The mirrored bottom PD clear all sampled bearings from 10° upward. The human T3 mounts with a blocked 60° sample are two panel batteries near the raised towers. The alien T3 restrictions at that elevation affect the four central pyramid shoulder batteries and four inner-roof PD on each half. The outward-facing outer-housing batteries clear every sampled direction at 60°. Preserving these hull shapes calls for mount-specific line-of-fire filtering, particularly for the recessed T1 PD, T3 batteries and alien ring mounts.

## Method and limits

I reused the [ship turret mesh extraction method](../turret-clearance/methodology.md) with the station-specific placement path. The installed combat initializer clones the weapon prefab and copies the mount position and rotation. The weapon retains its prefab scale. The audit decoded 22 eligible model resources and considered smaller eligible hull weapons in each battery slot. PD includes the laser heads and the human autocannon fallback. The rendered turrets use a representative mix from the same measured meshes and show the fitted sizes and seating collars.

The mechanical test encloses the base and pitching head in a conservative cylinder for continuous full azimuth and outward pitch from 0° through 90°. Hull triangles are clipped against a square enclosing the footprint to bound the highest shell point under the sweep. Each proposed collar lifts the swept geometry at least 0.09 units above that bound. Pair tests include the collars and both physical faces. They use separating projections and require at least 0.18 units of separation. Positive results certify separation for the modeled motion even when adjacent turrets traverse independently.

A separate check verified that the envelope contains the geometry of each of the 22 models at 61 evenly spaced pitch poses. The yaw rotation preserves the radius about the mounting axis.

The firing samples use one representative weapon per position. The isolated-shell study does not cover neighboring station modules, fired shots crossing another moving turret, below-tangent pitches, or how the live game resolves targeting. These checks support geometry and placement decisions before a gameplay patch.
