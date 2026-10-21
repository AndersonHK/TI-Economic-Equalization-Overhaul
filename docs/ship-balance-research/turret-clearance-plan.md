# Turret clearance and hull-node research plan

Requested scope: build reproducible asset-measurement tooling and comprehensive
Markdown inventories for every installed hull appearance, hull weapon sizes 1,
2 and 4, optional merge-node reuse, and class-level damage-output deltas.
Gameplay fitting and weapon balance changes are not part of this study.

Completed report: [turret clearance and firepower audit](turret-clearance/README.md).
Automated extraction, numerical checks and required deployment completed;
manual rendered-behavior validation remains explicitly pending.

1. Read installed templates and merge the repository's partial overrides. Hash
   source assets and templates, and retain exact prefab and controller identities.
2. Measure weapon base and rotating assembly geometry with their authored
   transforms. Trace the game's mesh replacement and scale rules before applying
   those dimensions to hull mount transforms.
3. Enumerate every hull appearance, its dorsal/ventral controller lists, ordinary
   node mapping and alternate multi-cell/merge nodes. Do not count mirrored
   dorsal/ventral artwork as independent logical weapons.
4. Compare conservative rotation envelopes at several absolute and relative
   padding thresholds. Report class minima, variant exceptions, limiting pairs,
   ordinary-node and expanded-node scenarios separately. A failed conservative
   test is not proof that visible meshes collide; a passing pair test does not
   establish hull/radiator clearance or usable firing arcs.
5. Tabulate matched-family raw sustained output, stock footprint packing,
   current capital policy, and hypothetical one-weapon-per-node capacities.
   Keep geometric feasibility separate from combat effectiveness and fitting
   implementation requirements.
6. Validate coverage and calculations, run the repository's normal deployment
   workflow for the tooling change, and record actual results and any safe
   deployment blocker. Retain reports and machine-readable evidence here.
