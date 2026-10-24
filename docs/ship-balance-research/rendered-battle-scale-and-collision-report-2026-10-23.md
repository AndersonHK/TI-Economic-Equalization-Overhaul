# Rendered battle scale, projectile size, and collision geometry

2026-10-23. **Research only.** No gameplay, template, asset, setting, build or
deployment changes. Baseline: EEO 0.10.8 and the installed Terra Invicta DLL
used by the repository's 1.0.53b compatibility record. Native methods were
freshly decompiled; the installed mod DLL matches the repository package.
[Hashes, excerpts and reproduction](battle-scale-camera-code-evidence-2026-10-23.md#baseline).

## Findings

The apparent size problem has a concrete code basis. Battle positions use
`0.05 scene units per kilometre`, but ship art uses `0.01 × A` per nominal
art-space metre, where `A` is 1 with cinematic scaling on and 0.1 with it off.
Consequently ships are **200× or 20× enlarged in linear dimensions relative
to battle distances**. Turning cinematic scaling off reduces the discrepancy;
it does not produce a common metre/kilometre scale. Projectile visuals use a
separate, still larger multiplier. The mod's caliber-based ballistic colliders
also retain the ship enlargement factor.
[Native conversions](battle-scale-camera-code-evidence-2026-10-23.md#s1-combat-scale-and-world-units),
[existing collider patch](../../TIEconomyMod/Patches/ProjectileCollisionPatches.cs).

There are three separate geometry issues:

1. The global enlargement relative to battle distances, confirmed below.
2. Individual ship prefab dimensions versus declared hull length/width. The
   engine does not automatically fit meshes to those template dimensions.
3. Authored colliders versus visible meshes. A smaller global multiplier keeps
   their relative mismatch; it cannot repair a displaced collider.

Smaller ships can reduce close-range aiming parallax. The inspected code does
**not** support a general claim that large collision boxes are blocking hull
guns from firing. Hull weapons pass the general arc gate, the inspected hull
weapon templates have 180° pivot limits, and friendly ship colliders are
filtered from ballistic hits. Training, mount orientation and muzzle placement
remain independent concerns.
[Firing checks](#hull-gun-firing-angles).

## The actual unit conversion

`SpaceCombatManager.km_to_scale`, velocity conversions and their inverses all
use 0.05. Thus one physically consistent metre would occupy `0.00005` scene
units. Changing only `GetScalingAdjustmentFactor` cannot establish that scale
without also changing formation, camera and other consumers of that shared
factor.
[Source](battle-scale-camera-code-evidence-2026-10-23.md#s1-combat-scale-and-world-units).

| Quantity | Cinematic on, A=1 | Cinematic off, A=0.1 | Consistent metre scale |
|---|---:|---:|---:|
| Ship scale, scene units per nominal metre | 0.01 | 0.001 | 0.00005 |
| Ship linear enlargement | 200× | 20× | 1× |
| Generic projectile visual multiplier | 0.025 | 0.0025 | 0.00005 |
| Generic projectile enlargement, per nominal resource metre | 500× | 50× | 1× |
| A nominal 100 m hull's apparent battle-space length | 20 km | 2 km | 0.1 km |
| EEO 254 mm ballistic collider's apparent width | 50.8 m | 5.08 m | 0.254 m |

The projectile rows describe transform multipliers, **not measured particle
diameters**. Resource dimensions, particle settings and the magnetic mass
multiplier further affect what is visible. A particular ship mesh need not
actually be 100 m just because its template says so. The scale ratios are
exact consequences of the inspected code; final per-asset dimensions need
their own measurements.

The installed English UI calls the toggle **Cinematic Combat Scaling** and
describes the alternative as realistic sizes. That label is looser than the
conversion above. EEO's `CampaignDifficultyRealismDefaultsPatch` sets this flag
on only for Cinematic when campaign options are reset. It does not rewrite an
existing campaign's saved flag or the separate skirmish selection. The current
battle's flag was not inspected, so both cases are reported.
[Default patch](../../TIEconomyMod/Patches/CampaignDifficultyPatches.cs),
[default policy](../../TIEconomyMod/Core/CampaignDifficultyDefaults.cs).

## Ships: visuals, hits, and statistical dimensions

`CombatShipController.Initialize` reparents the ship visualizer into combat
and assigns its scale from `modelScalingFactor`. It obtains its hit colliders
from children of `ShipModelController`. Descendant hull colliders and weapon
mounts therefore inherit the visualizer scale. Merely changing `length_m` or
`width_m` does not resize that prefab.
[Native setup](battle-scale-camera-code-evidence-2026-10-23.md#s2-ship-rendering-and-colliders).

| Geometry | Main role | Consequence of changing visualizer scale |
|---|---|---|
| Prefab meshes and attached mounts | Appearance, muzzle position | Scale together |
| Prefab HurtBox children | Actual ballistic ray hits | Scale with their ancestor |
| Root/trigger colliders | Selection/proximity/navigation behavior | Also require review; do not treat every collider as a damage hitbox |
| Hull template length/width | Armor, inertia, damage cross-section and other balance calculations | Not rewritten by a visual scale change |
| Predicted threat sphere | Projectile threat/defense reasoning | Uses template length times `modelScalingFactor` |

`TISpaceCombatProjectileState.WillHitSphere` uses the **full template length as
a radius**, rather than half-length or the actual collider envelope. It is a
conservative threat proxy, not proof of a physical collision. Changing the
model-scale getter would affect this proxy; scaling only spawned visuals would
leave it unchanged. Native damage cross-section is delegated to ship state and
is distinct from scaled scene collider bounds.
[Code and source map](battle-scale-camera-code-evidence-2026-10-23.md#s2-ship-rendering-and-colliders),
[earlier geometry analysis](drive-reactor-pairing-and-hull-geometry.md#hull-length-and-width-what-the-runtime-actually-uses).

Earlier prefab measurements already expose independent discrepancies: the
base Frigate has a displaced radiator collider extending behind its visible
model; the Destroyer's decorative width exceeds its ballistic width. Those
remain **historical static measurements**, not observations from this turn's
gameplay. This turn verified that the installed base/DLC ship and weapon bundle
hashes still match the retained turret survey. It did not rerun all prefab
extraction or verify animated collider states.
[Measured hull/collider tables](gunship-and-escort-hull-analysis.md#authored-visual-size-and-ballistic-raycast-geometry),
[bundle identity](battle-scale-camera-code-evidence-2026-10-23.md#baseline).

## Projectiles: what is already corrected and what remains large

Native `BallisticProjectileController.Fire` assigns `projectileScalingFactor`
to its transform. Magnetic projectiles multiply it again by
`log10(warheadMass_kg)`. This is a visibility convention, not a caliber-derived
shape. `MagBulletParticleController.UpdateMass` changes particle alpha as mass
is lost; it does not derive a physical diameter.
[Ballistic setup](battle-scale-camera-code-evidence-2026-10-23.md#s3-projectile-visuals-and-hit-detection).

EEO already changes NavalGun and Magnetic collisions:

- Gun diameters come from `projectileDiameter_mm`; magnetic fallback derives a
  cylinder diameter from complete round mass, density 19,300 kg/m³ and
  length/diameter 10. This is a documented modeling assumption.
- After firing, collider dimensions are compensated for transform scale so
  their world width is `diameter_m × modelScalingFactor`. They therefore remain
  20×/200× physical width in battle coordinates, while being much smaller than
  native generic boxes. The implementation approximates a round with a
  caliber-sized box/sphere/capsule, rather than a full elongated projectile.
- Damage cross-section separately uses `π × physical_radius_m²`. It is already
  in physical units and should not be divided by another 20 or 200.
- The ballistic forward-ray sweep is reduced from native 1.2× to 1.0× travel.
  It is still a **ray**, not a sphere/box sweep using the projectile's own
  width. Resizing a projectile collider principally changes it as a target for
  interception/other ray hits; it does not directly give its outbound ray a
  smaller collision radius.

[Sizer and gating](../../TIEconomyMod/Patches/ProjectileCollisionPatches.cs),
[math](../../TIEconomyMod/Core/ProjectileCollisionMath.cs),
[diameter registry](../../TIEconomyMod/Core/ProjectileGeometryRegistry.cs),
[native hit path](battle-scale-camera-code-evidence-2026-10-23.md#s3a-projectile-hitboxes-and-friendly-filtering).

Visual particles, trails, flashes and impact effects are not resized by the
caliber collider patch. Missiles and plasma are outside its diameter registry.
Missiles separately scale the renderer and a distance-dependent tracking
display. Root MeshRenderer handling in projectile pooling adds another
conditional scale path. Therefore **do not infer missile collider dimensions
from `projectileScalingFactor` alone**. Per-prefab collider, layer, renderer,
particle scaling mode and pooled reuse inspection is still required.
[Missile/pool source map](battle-scale-camera-code-evidence-2026-10-23.md#s3-projectile-visuals-and-hit-detection).

## Hull-gun firing angles

The inspected chain is:

1. Target acquisition selects a target and predicted aim point.
2. `Weapon.InArc` applies explicit pivot checks to nose weapons; the hull path
   returns true. `SelectWeaponVisualization` chooses dorsal/ventral using the
   target direction relative to the ship center and its up vector.
3. `ShipWeaponVisController.RotateToTarget` checks its own mount-up direction
   against `pivotRange_deg`, then trains yaw and elevation at `270/internalSize`
   degrees per simulation second.
4. `OnTarget` requires alignment within 1° for a non-static launcher.
   `TryFireCommon` also checks cooldown, target and weapon operability. EEO's
   cadence scheduler explicitly rotates the selected turret before trying fire.

[Native checks and template counts](battle-scale-camera-code-evidence-2026-10-23.md#s5-hull-gun-aiming-and-firing),
[turret training](battle-scale-camera-code-evidence-2026-10-23.md#s5a-turret-training-and-alignment),
[EEO cadence](../../TIEconomyMod/Patches/WeaponCadencePatches.cs).

All 90 installed hull records across gunpowder, magnetic, laser, particle and
plasma families have `pivotRange_deg=180`. The corresponding EEO overrides do
not change that field. Thus the inspected hull pivot check admits the full
angular range, including directions below an individual mount's local horizon.
There is no general own-hull occlusion raycast in the common firing gate.
Ballistic hit filtering skips friendly ships/projectiles; beam firing applies
damage directly to its selected target after common checks, using target-only
collider raycasts to locate the impact point.
[Evidence](battle-scale-camera-code-evidence-2026-10-23.md#s5-hull-gun-aiming-and-firing),
[friendly filtering](battle-scale-camera-code-evidence-2026-10-23.md#s3a-projectile-hitboxes-and-friendly-filtering).

**Expected benefit, not yet tested:** smaller mount offsets relative to
unchanged ship separation reduce aiming parallax and can make dorsal/ventral
selection and turret training less visually awkward at close ranges. They do
not expand those already broad limits or resolve intrinsic mount placement.
Uniformly shrinking a hull and all its turrets leaves turret/hull overlap and
the local angle of self-obstruction unchanged. The accepted Mothership mount
correction and canonical gunpowder art work must be preserved.
[Current mount geometry](mothership-symmetric-mount-implementation-2026-10-21.md),
[accepted gun art](gunpowder-canonical-art-implementation-2026-10-21.md).

## Proposed implementation path

These are options for a later authorized implementation, not changes made here.

1. **Improve camera precision first.** True-scale ships need closer inspection
   distances, suitable clipping and reliable selection. The current camera's
   free-look movement already exceeds individual ship dimensions by a large
   amount. [Camera report](combat-camera-fine-control-report-2026-10-23.md).
2. **Introduce an explicit geometry-unit policy.** Retain `0.05 u/km` for
   positions, speeds and weapon ranges; use `0.00005 u/m` for physical geometry.
   A ship visualizer would shrink by 1/20 from the current non-cinematic mode
   or 1/200 from cinematic mode. Treat an intermediate readability multiplier
   as a deliberate option with stated enlargement, rather than calling it
   physical scale. Avoid changing hull template dimensions to achieve this.
3. **Keep visual and hit geometry coherent.** Apply the policy to ship/hab
   models, HurtBox colliders, muzzle points and predictive threat sizes.
   Review root triggers, selection targets, range overlays, navigation,
   formation spacing, radiators and effects individually. Do not simply change
   the shared adjustment factor: its other consumers would change too.
4. **Handle projectile families explicitly.** Put caliber collision dimensions
   into the same world-unit conversion. Audit missiles/plasma and pooling;
   separately offer readable markers/tracers without inflating hitboxes.
   Preserve physical damage cross-sections, masses and weapon statistics.
5. **Then repair asset-specific defects.** Compare each appearance's declared
   dimensions, mesh and colliders. Decide whether the target is authored art
   dimensions or the statistical envelope; nonuniform length/width fitting can
   distort turrets and drives. Do not mix this decision into the global unit fix.

This would be a gameplay change: smaller target geometry affects ballistic hit
rates, interception and close encounters. Beams still use physical/statistical
area formulas, so relative weapon effectiveness can change even if displayed
weapon statistics do not. Auto-resolve is a separate path and was not audited
for parity here.

## Validation required before acceptance

For an implementation, follow the repository's normal documented
build/deploy/verification flow, then perform controlled manual battles:

- Read back transforms and world bounds for human/alien ships, a station,
  NavalGun/Magnetic/Missile/Plasma projectiles and their selection markers.
  Compare a known model dimension with the measured battle-space separation.
- Test freshly spawned and repeatedly reused pooled projectiles. Record
  near-miss hits, interceptions, hit-layer membership and physical damage area.
- At fixed separation and technology, test dorsal/ventral fire from targets
  above, below and in the ship plane, including close passes. Log chosen mount,
  aim error, training state and reason for failing fire. Compare before/after
  rather than judging the visible cone alone.
- Test controls, clipping, selection, formation, navigation, docking into/out
  of combat and restoration to the strategic visualizer in both scale modes.
- Compare matched battles for ballistic versus beam effectiveness; check saves
  and separate auto-resolve assumptions before claiming balance parity.

**Evidence limit:** this investigation confirms code paths and arithmetic.
It does not demonstrate actual rendered pixel sizes, live collider bounds,
improved firing behavior or battle balance. No build/deploy was appropriate
for this report-only request.
