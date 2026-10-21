# Projection methodology and boundaries

2026-10-21. This is a separate projected copy of the original clearance study.
The original reports and measurement JSON are not overwritten.

## Inputs and baseline reproduction

Use the original `turret-clearance/measurements.json` for 28 classes, 64
appearances, 147 hull templates, all nodes and placeholders, legal footprint
mappings, seven padding rules and current raw weapon output. Use the subsequent
mesh-inspection snapshot for the sampled horizontal-motion bounds, and the
barrel/lens audit for 41 gun/magnetic/laser resources and 166 ship templates.
All source artifacts are hashed in [input-hashes.json](input-hashes.json).

Original posed vertices are transformed back through the stored root scale,
child scale, position and quaternion to reconstruct the raw components. Their
full-rotation spheres reproduce the original radii. Applying scale one then
reproduces all **5,376 fit records**, including pass flags, exact extra-node
subsets and minimum edge gaps. The maximum gap discrepancy is zero in this run.

## Canonical targets

Gun caliber is the declared projectile diameter. Magnetic caliber is a
provisional reference from EEO's existing tungsten-cylinder collision convention,
using complete round mass and L/D 10. Neither that mass nor a visual barrel count
is divided among the openings. Polygon **inscribed diameter** is used to fit a
centered circular projectile between flats; the prior audit also reported X/Y
corner spans, which give slightly different scale factors. No additional sabot,
rifling or mechanical clearance is invented.

Laser diameter is twice `mirrorRadius_cm`, including PD. This follows the user's
final instruction that simulation physics takes precedence over interpreting the
localized number as a diameter. A projected mesh change does not change this
parameter, beam quality, jitter, shot power or cooldown. Closed nose rail faces
remain unresolved; their whole-resource factor stays one pending a designed
opening. Unmeasured plasma, particle and missile canonical dimensions are not
invented; original art is retained in all hull-inventory maxima.

## Proportional and moving-only models

For base vertices `b`, child vertices `v`, child offset `p`, root scale `S` and
child scale `C`, the original conservative radius is:

```text
Rb = max norm(S*b)
Rp = norm(S*p)
Rv = max(abs(S)) * max norm(C*v)
R = max(Rb, Rp + Rv)
```

Proportional canonical scale `s` gives `R' = s*R`; all visible dimensions and
the prior horizontal bound scale by the same factor. Moving-only scale gives
`R' = max(Rb, Rp + s*Rv)`. The latter does not seat an enlarged/shrunken head
automatically in its unchanged cradle. It is a geometric sensitivity, not a
completed mesh design. Its horizontal continuous bound was not recomputed and
is shown as unavailable rather than substituting the 3D sphere.

Designer rows apply the same geometric factors to the original placeholder
scale path, preserving comparison with the prior study. This assumes the factor
is applied to actual geometry/pivots/firepoints, or equivalently through a proper
instance hook. **Changing only the resource root scale would not produce these
preview results**, because native SetWeapon normally retains preview placeholders.
The same applies to simple hulls. Any implementation that chooses to normalize
all previews to combat dimensions must remeasure that distinct policy.

## Compact alien optical-head contract

This alternative replaces each alien hull laser's multiple visible optical
elements with one modeled system: aperture D, housing sphere diameter 1.20D,
original stationary base, head center at `(0, baseTop + 0.60D, 0)`. The base top
is its actual maximum Y, clamped at zero. The head's lower point is tangent to
the base-top plane; this does not establish detailed cradle/support contact.

```text
R = max(original base radius, baseTop + 1.20D)
horizontal diameter = max(original base XZ swept diameter, 1.20D)
```

The proposed replacement uses normalized transforms and explicitly applies them
in designer/combat, so this scenario's alien laser previews use the same resource
dimensions as combat. This preview policy difference is intentional and recorded,
not attributed to geometry alone. Nonlaser and human entries retain the canonical
proportional comparison. The compact head's box corners are used only for AABB
dimensions; horizontal motion uses the sphere, not that box's diagonal.

This contract is a geometric packaging proposal, not a solved optical/mechanical
assembly. The sphere can contain a circular D aperture with the stated outside
allowance, but optical depth, support, material, radiator/power infrastructure
and animation require later modeling. Multiple filled apertures do not receive
an assumed area-to-diffraction conversion. Nose optics need a new attachment
study and are not granted the hull concept's clearance certificates.

## Node pairing and extra slots

Keep the original native node positions, inherited scales and footprints.
Opposite-side copies of one logical index are not independent capacity slots;
other cross-face pairs remain tested. For different indices,
`gap = distance - radiusA - radiusB`. Absolute and proportional padding combine
with AND. A class certificate requires every appearance to pass. Search sizes
1, 2 and 4 independently; a largest passing size is not a monotonic guarantee.
Single-node `4*` is a vacuous neighbor result, never proof of hull fit.

Extra-node subsets retain all ordinary nodes and maximize compatible extras
exactly. No UI expansion is performed. Corrected Mothership coordinates are
overlaid in a separate artifact and in the current-legal layout study; they are
not quietly substituted into the original-native comparison.

## Legal loadouts, DPS and tier coherence

The legal-layout supplement enumerates all maximum-cardinality same-weapon
packings using real grid offsets and merge controllers. Current capitals admit
only FourHull, one per ordinary cell. Worst and best maximum-packing gaps are
kept separate. An unavailable footprint is not treated as a zero-size turret.

Retier output uses the original report's exact mixed-family packing optimizer
and merged stats. Heavy rail/coil candidates become TwoHullHoriz separately;
the naval candidate changes only the 6-inch to TwoHullHoriz. Capitals retain
their current policy so the incompatibility is visible. Art-only output remains
exactly unchanged. Raw MJ/s excludes armor, range loss, PD saturation, maneuver,
magazine exhaustion and power/heat downtime.

Visual-coherence tables use horizontal motion diameter, quartiles, CV and
cross-tier inversion frequency. They report both distinct per-scenario art and
a fixed paired cohort of canonical `(resource, factor, original size)`
representatives. Identical wavelength/mark art is not counted repeatedly, while
different caliber/Gen3 targets retain independent weights. No general law that
physical diameter must equal slot count is imposed.

All sphere failures are **not certified**, not confirmed collisions. The
projection does not inspect full hull/body intersections, true support contacts,
VFX, deployment animations or live firing arcs. Final authored models must be
measured again before accepting the implementation.
