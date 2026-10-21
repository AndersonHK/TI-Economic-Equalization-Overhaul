# Canonical turret projection report set

2026-10-21. **Planning evidence; nothing here is implemented.** Start with the
[canonical scaling plan](../canonical-turret-scaling-plan-2026-10-21.md).
The [original turret audit](../turret-clearance/README.md) remains a preserved
native-asset baseline, and the [25 mm plan is archived](../archive/25mm-autocannon-2026-10-21/README.md).

## Projected copies and comparison reports

- [Weapon inventory and all 166 target records](weapon-inventory.md)
- [All 28 classes / 64 appearances, seven padding rules and numeric deltas](class-clearance.md)
- [Current-legal maximum packings, including corrected Mothership](legal-layouts.md)
- [Authored/extra-node capacity](node-inventory.md)
- [Visual size versus slot cost](size-tier-coherence.md)
- [Art-only zero-DPS change and optional retier fallout](dps-analysis.md)
- [Methodology, assumptions and limitations](methodology.md)
- [Reproduction recipe](reproduction.md) and [verification](verification.json)

## Scenario keys

| Key | Meaning |
|---|---|
| `baseline` | Original measured art and native nodes; exact original fit reproduction |
| `canonical` | Whole-model proportional scale to gun/proxy bore or native optical diameter |
| `head_only` | Same scale on moving assembly only; original base and pivot retained |
| `compact_optics` | Canonical proportional elsewhere; new dimensioned single-aperture heads for alien hull lasers |
| `named_diameter` | Rejected laser-label-as-diameter sensitivity; PD still follows native field |
| `mag_relative` | Canonical optics/guns; magnetic progression anchored to original light-gun art |
| `retier_rail` | Compact-optics geometry; heavy human rail batteries 4 → 2 |
| `retier_coil` | Compact-optics geometry; heavy human coil batteries 4 → 2 |
| `retier_naval` | Compact-optics geometry; 6-inch gun 1 → 2 |
| `retier_pair` | Compact-optics geometry; heavy rails 4 → 2 and 6-inch 1 → 2 |

## Data

Every `*-measurements.json` holds the full projected native-node fit records.
[current-mothership.json](current-mothership.json) separately applies the
deployed 0.10.4 geometry, avoiding a false comparison between its original
duplicates and the corrected layout. [Weapon projections](weapon-projections.json),
[all targets](all-weapon-targets.json), [legal layouts](legal-layouts.json),
[retier DPS](retier-dps.json), [tier statistics](tier-statistics.json), and
[paired-cohort statistics](paired-tier-statistics.json) support the tables.
[Input hashes](input-hashes.json) identify retained evidence.

No base/hull surface intersection, detailed gimbal contact, radiator sweep,
target-tracking animation or nose-mount clearance is certified by these reports.
The compact head is a proposed geometric envelope, not an already authored mesh.
