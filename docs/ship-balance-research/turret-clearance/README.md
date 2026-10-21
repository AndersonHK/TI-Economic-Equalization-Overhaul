# Turret clearance and firepower audit

**Canonical-size projection:** the separate
[projected report set](../canonical-turret-projection/README.md) compares proposed
physics-based dimensions against this exact baseline, including tier-coherence,
legal loadouts and corrected Mothership geometry. This original snapshot remains
unchanged by the projected numbers.

**Current follow-ups:** [weapon shape and Monitor interpretation](weapon-shapes-and-monitor-2026-10-21.md)
distinguishes actual mesh dimensions from conservative motion envelopes.
[Mothership geometry in 0.10.4](../mothership-symmetric-mount-implementation-2026-10-21.md)
changes runtime mount positions. The tables below remain the original native-asset
research snapshot committed as `d5cf300`; they do not include that runtime correction.

Research snapshot: 2026-10-20 (user local date). Gameplay changes proposed here are not implemented. Measurements cover **28 hull classes, all 64 installed appearances, 147 hull-weapon templates, and 35 distinct weapon resources**. The geometry is decoded from installed assets; firing arcs and rendered combat behavior have not been manually tested.

## Principal findings

- A class-wide large-turret rule does **not** follow hull size monotonically. At a conservative **5 m edge gap**, all four human Destroyer, Cruiser, Battlecruiser and Lancer appearances certify size 4 at every ordinary node. Escort, Frigate, Monitor, Battleship, Dreadnought and Titan do not certify that rule across every appearance. A failed bound is not proof of mesh collision.
- The designer and combat use different scale paths. `ShipModelController.SetWeapon` replaces base/weapon scales only when there is a ship state and the hull is not simple. This makes preview-based judgments insufficient; see the paired tables below and [methodology](methodology.md).
- **AlienDestroyer has 3 paired controller indices for 2 UI hull cells**. Its middle merge mount is a credible extra-gun candidate. AlienDreadnought has 12 for 8, and AlienMothership 20 for 16; their closely clustered central mounts prevent treating all extras as free capacity. No human appearance has extra hull controller indices.
- **Mothership ventral indices 1 and 18 are distinct controllers at exactly the same position**. The central ordinary left/right mounts are 8 m apart; merge nodes bisect those gaps to 4 m. The asset does not support an unconditional all-node large-turret clearance claim.
- Raw firepower gains depend strongly on weapon family. The DPS report keeps complete stock, current-mod, size-2-per-node, and size-4-per-node baselines separate. Existing Titan/AlienTitan/Mothership size-4-per-node behavior is included in the current-mod baseline, not counted again as a new gain.

## Reports and evidence

- [Methodology, reproduction, formulas and limitations](methodology.md)
- [Largest weapons and complete resource/template inventory](weapon-inventory.md)
- [Every class and appearance: padding thresholds and clearance deltas](class-clearance.md)
- [Extra/merge nodes and complete coordinate catalog](node-inventory.md), with [pairwise distance matrices](node-distance-matrices.md)
- [DPS families, every class, capacity and mass deltas](dps-analysis.md)
- [Machine-readable measurements](measurements.json) and [native controller maps](mount-maps.json)
- [Validation and deployment record](validation.md)

## Class-wide ordinary-node maximum size

Native faction weapon inventory; every installed appearance must pass. `none certified` means no tested size certifies this conservative margin, not that stock weapons cannot work. `4*` denotes one logical mount: there is no neighboring hull turret, so this is a vacuous pair-spacing result, not a hull-fit recommendation. Opposite-face copies of the same logical weapon are not a second capacity slot.

| Class | Tier | Art variants | UI nodes | Combat ≥2 m | Combat ≥5 m | Combat ≥10 m | Preview ≥5 m |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Gunship | 1 | 4 | 0 | no nodes | no nodes | no nodes | no nodes |
| Escort | 1 | 4 | 2 | none certified | none certified | none certified | none certified |
| Corvette | 1 | 4 | 1 | 4* | 4* | 4* | 4* |
| Frigate | 1 | 4 | 2 | 2 | none certified | none certified | none certified |
| Monitor | 2 | 4 | 4 | none certified | none certified | none certified | none certified |
| Destroyer | 2 | 4 | 2 | 4 | 4 | none certified | 4 |
| Cruiser | 2 | 4 | 3 | 4 | 4 | 4 | 4 |
| Battlecruiser | 2 | 4 | 2 | 4 | 4 | 4 | 4 |
| Battleship | 3 | 4 | 6 | none certified | none certified | none certified | none certified |
| Lancer | 3 | 4 | 3 | 4 | 4 | none certified | 4 |
| Dreadnought | 3 | 4 | 8 | none certified | none certified | none certified | none certified |
| Titan | 3 | 4 | 6 | none certified | none certified | none certified | none certified |
| STOFighter | 1 | 1 | 1 | 4* | 4* | 4* | 4* |
| AlienGunship | 1 | 1 | 0 | no nodes | no nodes | no nodes | no nodes |
| AlienEscort | 1 | 1 | 2 | 4 | 4 | none certified | 4 |
| AlienCorvette | 1 | 1 | 1 | 4* | 4* | 4* | 4* |
| AlienFrigate | 1 | 1 | 3 | 4 | 4 | 4 | 4 |
| AlienMonitor | 2 | 1 | 4 | 4 | 4 | none certified | 4 |
| AlienDestroyer | 2 | 1 | 2 | 4 | 4 | 4 | 4 |
| AlienCruiser | 2 | 1 | 4 | 4 | 4 | 4 | 4 |
| AlienBattlecruiser | 2 | 1 | 3 | 4 | 4 | 4 | 4 |
| AlienBattleship | 2 | 1 | 6 | 4 | 4 | 4 | 4 |
| AlienLancer | 2 | 1 | 4 | 4 | 4 | 4 | 4 |
| AlienDreadnought | 2 | 1 | 8 | none certified | none certified | none certified | none certified |
| AlienTitan | 3 | 1 | 8 | 4 | 4 | 4 | 4 |
| AlienAssaultCarrier | 3 | 1 | 6 | 1 | none certified | none certified | none certified |
| AlienMothership | 3 | 1 | 16 | none certified | none certified | none certified | none certified |
| SalamanderGunship | 1 | 1 | 1 | 4* | 4* | 4* | 4* |
