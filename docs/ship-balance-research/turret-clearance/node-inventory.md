# Hull controller, extra-node and coordinate inventory

One logical hull weapon may be represented by two visible turrets. Dorsal plus ventral counts must not be added when estimating weapon capacity. Indices, not object names, select native controllers. “Extra” below means a controller index outside the set used by the UI’s ordinary OneHull placements.

## All appearances: declared versus authored nodes

| Class/art | UI cells | Dorsal | Ventral | Distinct indices | Extra indices | All-node nearest m | Ordinary nearest m |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Gunship/0 | 0 | 0 | 0 | 0 | none | — | — |
| Gunship/1 | 0 | 0 | 0 | 0 | none | — | — |
| Gunship/2 | 0 | 0 | 0 | 0 | none | — | — |
| Gunship/3 | 0 | 0 | 0 | 0 | none | — | — |
| Escort/0 | 2 | 2 | 2 | 2 | none | 15.60 | 15.60 |
| Escort/1 | 2 | 2 | 2 | 2 | none | 13.70 | 13.70 |
| Escort/2 | 2 | 2 | 2 | 2 | none | 17.65 | 17.65 |
| Escort/3 | 2 | 2 | 2 | 2 | none | 14.23 | 14.23 |
| Corvette/0 | 1 | 1 | 1 | 1 | none | — | — |
| Corvette/1 | 1 | 1 | 1 | 1 | none | — | — |
| Corvette/2 | 1 | 1 | 1 | 1 | none | — | — |
| Corvette/3 | 1 | 1 | 1 | 1 | none | — | — |
| Frigate/0 | 2 | 2 | 2 | 2 | none | 16.44 | 16.44 |
| Frigate/1 | 2 | 2 | 2 | 2 | none | 19.10 | 19.10 |
| Frigate/2 | 2 | 2 | 2 | 2 | none | 29.50 | 29.50 |
| Frigate/3 | 2 | 2 | 2 | 2 | none | 30.90 | 30.90 |
| Monitor/0 | 4 | 4 | 4 | 4 | none | 7.55 | 7.55 |
| Monitor/1 | 4 | 4 | 4 | 4 | none | 11.32 | 11.32 |
| Monitor/2 | 4 | 4 | 4 | 4 | none | 12.80 | 12.80 |
| Monitor/3 | 4 | 4 | 4 | 4 | none | 17.80 | 17.80 |
| Destroyer/0 | 2 | 2 | 2 | 2 | none | 21.98 | 21.98 |
| Destroyer/1 | 2 | 2 | 2 | 2 | none | 41.50 | 41.50 |
| Destroyer/2 | 2 | 2 | 2 | 2 | none | 56.90 | 56.90 |
| Destroyer/3 | 2 | 2 | 2 | 2 | none | 40.90 | 40.90 |
| Cruiser/0 | 3 | 3 | 3 | 3 | none | 26.44 | 26.44 |
| Cruiser/1 | 3 | 3 | 3 | 3 | none | 33.71 | 33.71 |
| Cruiser/2 | 3 | 3 | 3 | 3 | none | 43.70 | 43.70 |
| Cruiser/3 | 3 | 3 | 3 | 3 | none | 39.07 | 39.07 |
| Battlecruiser/0 | 2 | 2 | 2 | 2 | none | 64.51 | 64.51 |
| Battlecruiser/1 | 2 | 2 | 2 | 2 | none | 39.94 | 39.94 |
| Battlecruiser/2 | 2 | 2 | 2 | 2 | none | 74.70 | 74.70 |
| Battlecruiser/3 | 2 | 2 | 2 | 2 | none | 48.40 | 48.40 |
| Battleship/0 | 6 | 6 | 6 | 6 | none | 13.33 | 13.33 |
| Battleship/1 | 6 | 6 | 6 | 6 | none | 18.80 | 18.80 |
| Battleship/2 | 6 | 6 | 6 | 6 | none | 13.90 | 13.90 |
| Battleship/3 | 6 | 6 | 6 | 6 | none | 9.16 | 9.16 |
| Lancer/0 | 3 | 3 | 3 | 3 | none | 32.84 | 32.84 |
| Lancer/1 | 3 | 3 | 3 | 3 | none | 31.80 | 31.80 |
| Lancer/2 | 3 | 3 | 3 | 3 | none | 29.61 | 29.61 |
| Lancer/3 | 3 | 3 | 3 | 3 | none | 21.40 | 21.40 |
| Dreadnought/0 | 8 | 8 | 8 | 8 | none | 8.60 | 8.60 |
| Dreadnought/1 | 8 | 8 | 8 | 8 | none | 6.92 | 6.92 |
| Dreadnought/2 | 8 | 8 | 8 | 8 | none | 13.40 | 13.40 |
| Dreadnought/3 | 8 | 8 | 8 | 8 | none | 20.80 | 20.80 |
| Titan/0 | 6 | 6 | 6 | 6 | none | 13.04 | 13.04 |
| Titan/1 | 6 | 6 | 6 | 6 | none | 6.44 | 6.44 |
| Titan/2 | 6 | 6 | 6 | 6 | none | 17.90 | 17.90 |
| Titan/3 | 6 | 6 | 6 | 6 | none | 29.00 | 29.00 |
| STOFighter/0 | 1 | 1 | 1 | 1 | none | — | — |
| AlienGunship/0 | 0 | 0 | 0 | 0 | none | — | — |
| AlienEscort/0 | 2 | 2 | 2 | 2 | none | 18.70 | 18.70 |
| AlienCorvette/0 | 1 | 1 | 1 | 1 | none | — | — |
| AlienFrigate/0 | 3 | 3 | 3 | 3 | none | 25.30 | 25.30 |
| AlienMonitor/0 | 4 | 4 | 4 | 4 | none | 18.15 | 18.15 |
| AlienDestroyer/0 | 2 | 3 | 3 | 3 | 2 | 21.00 | 44.00 |
| AlienCruiser/0 | 4 | 4 | 4 | 4 | none | 24.62 | 24.62 |
| AlienBattlecruiser/0 | 3 | 3 | 3 | 3 | none | 39.68 | 39.68 |
| AlienBattleship/0 | 6 | 6 | 6 | 6 | none | 24.50 | 24.50 |
| AlienLancer/0 | 4 | 4 | 4 | 4 | none | 40.10 | 40.10 |
| AlienDreadnought/0 | 8 | 12 | 12 | 12 | 2,5,8,11 | 2.00 | 4.00 |
| AlienTitan/0 | 8 | 8 | 8 | 8 | none | 23.15 | 23.15 |
| AlienAssaultCarrier/0 | 6 | 6 | 6 | 6 | none | 12.00 | 12.00 |
| AlienMothership/0 | 16 | 20 | 20 | 20 | 5,8,11,14 | 0.00 | 0.00 |
| SalamanderGunship/0 | 1 | 1 | 1 | 1 | none | — | — |


## Extra-node candidates and padding consequences

AlienDestroyer has a middle alternative mount between two ordinary mounts. AlienDreadnought and AlienMothership have central merge nodes within much tighter left/right groups. A class with one alien appearance is consistent across its installed inventory, but this does not imply unseen variants. No surveyed human class has additional hull-controller indices beyond its UI node count.

### AlienDestroyer

Extra authored indices: **[2]**. Of these, legal native multi-cell placements select **[2]**; the remainder are authored alternatives not reached by the audited legal multi-cell grid mappings. All remain hypothetical candidates for a new explicit mapping.

| Context | Size | Padding m | Padding ratio | Ordinary pass | All nodes pass | Max count preserving ordinary | Additional nodes | Selected indices |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| combat | 1 | 0 | 0% | True | True | 3 | 1 | 0,1,2 |
| combat | 1 | 2 | 0% | True | True | 3 | 1 | 0,1,2 |
| combat | 1 | 5 | 0% | True | True | 3 | 1 | 0,1,2 |
| combat | 1 | 10 | 0% | True | True | 3 | 1 | 0,1,2 |
| combat | 1 | 0 | 25% | True | True | 3 | 1 | 0,1,2 |
| combat | 1 | 0 | 50% | True | True | 3 | 1 | 0,1,2 |
| combat | 1 | 5 | 25% | True | True | 3 | 1 | 0,1,2 |
| combat | 2 | 0 | 0% | True | True | 3 | 1 | 0,1,2 |
| combat | 2 | 2 | 0% | True | True | 3 | 1 | 0,1,2 |
| combat | 2 | 5 | 0% | True | True | 3 | 1 | 0,1,2 |
| combat | 2 | 10 | 0% | True | False | 2 | 0 | 0,1 |
| combat | 2 | 0 | 25% | True | True | 3 | 1 | 0,1,2 |
| combat | 2 | 0 | 50% | True | True | 3 | 1 | 0,1,2 |
| combat | 2 | 5 | 25% | True | True | 3 | 1 | 0,1,2 |
| combat | 4 | 0 | 0% | True | True | 3 | 1 | 0,1,2 |
| combat | 4 | 2 | 0% | True | True | 3 | 1 | 0,1,2 |
| combat | 4 | 5 | 0% | True | True | 3 | 1 | 0,1,2 |
| combat | 4 | 10 | 0% | True | False | 2 | 0 | 0,1 |
| combat | 4 | 0 | 25% | True | True | 3 | 1 | 0,1,2 |
| combat | 4 | 0 | 50% | True | True | 3 | 1 | 0,1,2 |
| combat | 4 | 5 | 25% | True | True | 3 | 1 | 0,1,2 |
| designer | 1 | 0 | 0% | True | True | 3 | 1 | 0,1,2 |
| designer | 1 | 2 | 0% | True | True | 3 | 1 | 0,1,2 |
| designer | 1 | 5 | 0% | True | True | 3 | 1 | 0,1,2 |
| designer | 1 | 10 | 0% | True | True | 3 | 1 | 0,1,2 |
| designer | 1 | 0 | 25% | True | True | 3 | 1 | 0,1,2 |
| designer | 1 | 0 | 50% | True | True | 3 | 1 | 0,1,2 |
| designer | 1 | 5 | 25% | True | True | 3 | 1 | 0,1,2 |
| designer | 2 | 0 | 0% | True | True | 3 | 1 | 0,1,2 |
| designer | 2 | 2 | 0% | True | True | 3 | 1 | 0,1,2 |
| designer | 2 | 5 | 0% | True | True | 3 | 1 | 0,1,2 |
| designer | 2 | 10 | 0% | True | False | 2 | 0 | 0,1 |
| designer | 2 | 0 | 25% | True | True | 3 | 1 | 0,1,2 |
| designer | 2 | 0 | 50% | True | True | 3 | 1 | 0,1,2 |
| designer | 2 | 5 | 25% | True | True | 3 | 1 | 0,1,2 |
| designer | 4 | 0 | 0% | True | True | 3 | 1 | 0,1,2 |
| designer | 4 | 2 | 0% | True | True | 3 | 1 | 0,1,2 |
| designer | 4 | 5 | 0% | True | True | 3 | 1 | 0,1,2 |
| designer | 4 | 10 | 0% | True | False | 2 | 0 | 0,1 |
| designer | 4 | 0 | 25% | True | True | 3 | 1 | 0,1,2 |
| designer | 4 | 0 | 50% | True | True | 3 | 1 | 0,1,2 |
| designer | 4 | 5 | 25% | True | True | 3 | 1 | 0,1,2 |


### AlienDreadnought

Extra authored indices: **[2, 5, 8, 11]**. Of these, legal native multi-cell placements select **[5, 8, 11]**; the remainder are authored alternatives not reached by the audited legal multi-cell grid mappings. All remain hypothetical candidates for a new explicit mapping.

| Context | Size | Padding m | Padding ratio | Ordinary pass | All nodes pass | Max count preserving ordinary | Additional nodes | Selected indices |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| combat | 1 | 0 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 1 | 2 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 1 | 5 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 1 | 10 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 1 | 0 | 25% | False | False | infeasible ordinary set | — | — |
| combat | 1 | 0 | 50% | False | False | infeasible ordinary set | — | — |
| combat | 1 | 5 | 25% | False | False | infeasible ordinary set | — | — |
| combat | 2 | 0 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 2 | 2 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 2 | 5 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 2 | 10 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 2 | 0 | 25% | False | False | infeasible ordinary set | — | — |
| combat | 2 | 0 | 50% | False | False | infeasible ordinary set | — | — |
| combat | 2 | 5 | 25% | False | False | infeasible ordinary set | — | — |
| combat | 4 | 0 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 4 | 2 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 4 | 5 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 4 | 10 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 4 | 0 | 25% | False | False | infeasible ordinary set | — | — |
| combat | 4 | 0 | 50% | False | False | infeasible ordinary set | — | — |
| combat | 4 | 5 | 25% | False | False | infeasible ordinary set | — | — |
| designer | 1 | 0 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 1 | 2 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 1 | 5 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 1 | 10 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 1 | 0 | 25% | False | False | infeasible ordinary set | — | — |
| designer | 1 | 0 | 50% | False | False | infeasible ordinary set | — | — |
| designer | 1 | 5 | 25% | False | False | infeasible ordinary set | — | — |
| designer | 2 | 0 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 2 | 2 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 2 | 5 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 2 | 10 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 2 | 0 | 25% | False | False | infeasible ordinary set | — | — |
| designer | 2 | 0 | 50% | False | False | infeasible ordinary set | — | — |
| designer | 2 | 5 | 25% | False | False | infeasible ordinary set | — | — |
| designer | 4 | 0 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 4 | 2 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 4 | 5 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 4 | 10 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 4 | 0 | 25% | False | False | infeasible ordinary set | — | — |
| designer | 4 | 0 | 50% | False | False | infeasible ordinary set | — | — |
| designer | 4 | 5 | 25% | False | False | infeasible ordinary set | — | — |


### AlienMothership

Extra authored indices: **[5, 8, 11, 14]**. Of these, legal native multi-cell placements select **[8, 11, 14]**; the remainder are authored alternatives not reached by the audited legal multi-cell grid mappings. All remain hypothetical candidates for a new explicit mapping.

| Context | Size | Padding m | Padding ratio | Ordinary pass | All nodes pass | Max count preserving ordinary | Additional nodes | Selected indices |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| combat | 1 | 0 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 1 | 2 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 1 | 5 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 1 | 10 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 1 | 0 | 25% | False | False | infeasible ordinary set | — | — |
| combat | 1 | 0 | 50% | False | False | infeasible ordinary set | — | — |
| combat | 1 | 5 | 25% | False | False | infeasible ordinary set | — | — |
| combat | 2 | 0 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 2 | 2 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 2 | 5 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 2 | 10 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 2 | 0 | 25% | False | False | infeasible ordinary set | — | — |
| combat | 2 | 0 | 50% | False | False | infeasible ordinary set | — | — |
| combat | 2 | 5 | 25% | False | False | infeasible ordinary set | — | — |
| combat | 4 | 0 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 4 | 2 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 4 | 5 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 4 | 10 | 0% | False | False | infeasible ordinary set | — | — |
| combat | 4 | 0 | 25% | False | False | infeasible ordinary set | — | — |
| combat | 4 | 0 | 50% | False | False | infeasible ordinary set | — | — |
| combat | 4 | 5 | 25% | False | False | infeasible ordinary set | — | — |
| designer | 1 | 0 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 1 | 2 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 1 | 5 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 1 | 10 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 1 | 0 | 25% | False | False | infeasible ordinary set | — | — |
| designer | 1 | 0 | 50% | False | False | infeasible ordinary set | — | — |
| designer | 1 | 5 | 25% | False | False | infeasible ordinary set | — | — |
| designer | 2 | 0 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 2 | 2 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 2 | 5 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 2 | 10 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 2 | 0 | 25% | False | False | infeasible ordinary set | — | — |
| designer | 2 | 0 | 50% | False | False | infeasible ordinary set | — | — |
| designer | 2 | 5 | 25% | False | False | infeasible ordinary set | — | — |
| designer | 4 | 0 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 4 | 2 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 4 | 5 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 4 | 10 | 0% | False | False | infeasible ordinary set | — | — |
| designer | 4 | 0 | 25% | False | False | infeasible ordinary set | — | — |
| designer | 4 | 0 | 50% | False | False | infeasible ordinary set | — | — |
| designer | 4 | 5 | 25% | False | False | infeasible ordinary set | — | — |


## Exact duplicate positions

| Class/art | Side A | Index A | Side B | Index B | Position m | Controller A | Controller B |
| --- | --- | --- | --- | --- | --- | --- | --- |
| AlienMothership/0 | ventral | 1 | ventral | 18 | -196.500, -23.180, 96.000 | -2121023153996458251 | 700196163148817806 |


The duplicate Mothership objects have different controller IDs. This is not just one object listed twice. No correction or repositioning was made. Even if this pair were corrected, the 8 m ordinary central spacing and 4 m merge spacing would still need separate evaluation.

## Complete node coordinates

Every physical controller is included, including inactive alternative mounts. Coordinates are relative to prefab scene translation; paths and controller IDs allow direct source inspection.

### Gunship

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |


### Escort

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 6.720 | 12.560 | Small Forward Dorsal Hull Weapon Base |
| 0 | dorsal | 1 | yes | 0.000 | 6.720 | -3.040 | Small Rear Dorsal Hull Weapon Base |
| 0 | ventral | 0 | yes | 0.000 | -6.720 | 12.560 | Small Forward Ventral Hull Weapon Base |
| 0 | ventral | 1 | yes | 0.000 | -6.720 | -3.040 | Small Rear Ventral Hull Weapon Base |
| 1 | dorsal | 0 | yes | 0.000 | 6.950 | 15.320 | Small Forward Dorsal Hull Weapon Base |
| 1 | dorsal | 1 | yes | 0.000 | 6.900 | 1.620 | Small Rear Dorsal Hull Weapon Base |
| 1 | ventral | 0 | yes | 0.000 | -6.950 | 15.320 | Small Forward Ventral Hull Weapon Base |
| 1 | ventral | 1 | yes | 0.000 | -6.900 | 1.620 | Small Rear Ventral Hull Weapon Base |
| 2 | dorsal | 0 | yes | 0.000 | 6.700 | 20.400 | Small Forward Dorsal Hull Weapon Base |
| 2 | dorsal | 1 | yes | 0.000 | 6.700 | 2.750 | Small Rear Dorsal Hull Weapon Base |
| 2 | ventral | 0 | yes | 0.000 | -6.700 | 20.400 | Small Forward Ventral Hull Weapon Base |
| 2 | ventral | 1 | yes | 0.000 | -6.700 | 2.750 | Small Rear Ventral Hull Weapon Base |
| 3 | dorsal | 0 | yes | 0.000 | 5.600 | 14.380 | Small Forward Dorsal Hull Weapon Base |
| 3 | dorsal | 1 | yes | 0.000 | 5.600 | 0.150 | Small Rear Dorsal Hull Weapon Base |
| 3 | ventral | 0 | yes | 0.000 | -5.600 | 14.380 | Small Forward Ventral Hull Weapon Base |
| 3 | ventral | 1 | yes | 0.000 | -5.600 | 0.150 | Small Rear Ventral Hull Weapon Base |


### Corvette

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 4.540 | 11.950 | Small Forward Dorsal Hull Weapon Base |
| 0 | ventral | 0 | yes | 0.000 | -4.540 | 11.950 | Small Forward Ventral Hull Weapon Base |
| 1 | dorsal | 0 | yes | 0.000 | 8.200 | 2.400 | Small Forward Dorsal Hull Weapon Base |
| 1 | ventral | 0 | yes | 0.000 | -8.200 | 2.400 | Small Forward Ventral Hull Weapon Base |
| 2 | dorsal | 0 | yes | 0.000 | 7.700 | 1.350 | Small Forward Dorsal Hull Weapon Base |
| 2 | ventral | 0 | yes | 0.000 | -7.700 | 1.350 | Small Forward Ventral Hull Weapon Base |
| 3 | dorsal | 0 | yes | 0.000 | 7.800 | 3.500 | Small Forward Dorsal Hull Weapon Base |
| 3 | ventral | 0 | yes | 0.000 | -7.800 | 3.500 | Small Forward Ventral Hull Weapon Base |


### Frigate

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 6.100 | 24.930 | Small Forward Dorsal Hull Weapon Base |
| 0 | dorsal | 1 | yes | 0.000 | 6.100 | 8.492 | Small Rear Dorsal Hull Weapon Base |
| 0 | ventral | 0 | yes | 0.000 | -6.100 | 24.930 | Small Forward Ventral Hull Weapon Base |
| 0 | ventral | 1 | yes | 0.000 | -6.100 | 8.492 | Small Rear Ventral Hull Weapon Base |
| 1 | dorsal | 0 | yes | 0.000 | 6.620 | 17.500 | Small Forward Dorsal Hull Weapon Base |
| 1 | dorsal | 1 | yes | 0.000 | 6.620 | -1.600 | Small Rear Dorsal Hull Weapon Base |
| 1 | ventral | 0 | yes | 0.000 | -6.620 | 17.500 | Small Forward Ventral Hull Weapon Base |
| 1 | ventral | 1 | yes | 0.000 | -6.620 | -1.600 | Small Rear Ventral Hull Weapon Base |
| 2 | dorsal | 0 | yes | 0.000 | 8.200 | 22.500 | Small Forward Dorsal Hull Weapon Base |
| 2 | dorsal | 1 | yes | 0.000 | 8.200 | -7.000 | Small Rear Dorsal Hull Weapon Base |
| 2 | ventral | 0 | yes | 0.000 | -8.200 | 22.500 | Small Forward Ventral Hull Weapon Base |
| 2 | ventral | 1 | yes | 0.000 | -8.200 | -7.000 | Small Rear Ventral Hull Weapon Base |
| 3 | dorsal | 0 | yes | 0.000 | 8.100 | 25.800 | Small Forward Dorsal Hull Weapon Base |
| 3 | dorsal | 1 | yes | 0.000 | 8.100 | -5.100 | Small Rear Dorsal Hull Weapon Base |
| 3 | ventral | 0 | yes | 0.000 | -8.100 | 25.800 | Small Forward Ventral Hull Weapon Base |
| 3 | ventral | 1 | yes | 0.000 | -8.100 | -5.100 | Small Rear Ventral Hull Weapon Base |


### Monitor

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 8.178 | 21.540 | Small Dorsal Hull Weapon Base One |
| 0 | dorsal | 1 | yes | 0.000 | 7.950 | 11.740 | Small Dorsal Hull Weapon Base Two |
| 0 | dorsal | 2 | yes | 0.000 | 7.490 | 3.950 | Small Dorsal Hull Weapon Base Three |
| 0 | dorsal | 3 | yes | 0.000 | 7.950 | -3.590 | Small Dorsal Hull Weapon Base Four |
| 0 | ventral | 0 | yes | 0.000 | -8.178 | 21.540 | Small Ventral Hull Weapon Base One |
| 0 | ventral | 1 | yes | 0.000 | -7.950 | 11.740 | Small Ventral Hull Weapon Base Two |
| 0 | ventral | 2 | yes | 0.000 | -7.490 | 3.950 | Small Ventral Hull Weapon Base Three |
| 0 | ventral | 3 | yes | 0.000 | -7.950 | -3.590 | Small Ventral Hull Weapon Base Four |
| 1 | dorsal | 0 | yes | 0.000 | 7.820 | 35.420 | Small Dorsal Hull Weapon Base One |
| 1 | dorsal | 1 | yes | 0.000 | 7.820 | 22.900 | Small Dorsal Hull Weapon Base Two |
| 1 | dorsal | 2 | yes | 0.000 | 7.600 | -8.430 | Small Dorsal Hull Weapon Base Three |
| 1 | dorsal | 3 | yes | 0.000 | 7.700 | -19.750 | Small Dorsal Hull Weapon Base Four |
| 1 | ventral | 0 | yes | 0.000 | -7.820 | 35.420 | Small Ventral Hull Weapon Base One |
| 1 | ventral | 1 | yes | 0.000 | -7.820 | 22.900 | Small Ventral Hull Weapon Base Two |
| 1 | ventral | 2 | yes | 0.000 | -7.600 | -8.430 | Small Ventral Hull Weapon Base Three |
| 1 | ventral | 3 | yes | 0.000 | -7.700 | -19.750 | Small Ventral Hull Weapon Base Four |
| 2 | dorsal | 0 | yes | 0.000 | 10.400 | 52.500 | Small Dorsal Hull Weapon Base One |
| 2 | dorsal | 1 | yes | 0.000 | 10.400 | 34.400 | Small Dorsal Hull Weapon Base Two |
| 2 | dorsal | 2 | yes | 0.000 | 10.400 | -0.800 | Small Dorsal Hull Weapon Base Three |
| 2 | dorsal | 3 | yes | 0.000 | 10.400 | -13.600 | Small Dorsal Hull Weapon Base Four |
| 2 | ventral | 0 | yes | 0.000 | -10.400 | 52.500 | Small Ventral Hull Weapon Base One |
| 2 | ventral | 1 | yes | 0.000 | -10.400 | 34.400 | Small Ventral Hull Weapon Base Two |
| 2 | ventral | 2 | yes | 0.000 | -10.400 | -0.800 | Small Ventral Hull Weapon Base Three |
| 2 | ventral | 3 | yes | 0.000 | -10.400 | -13.600 | Small Ventral Hull Weapon Base Four |
| 3 | dorsal | 0 | yes | 0.000 | 11.750 | 47.500 | Small Dorsal Hull Weapon Base One |
| 3 | dorsal | 1 | yes | 0.000 | 11.750 | 19.200 | Small Dorsal Hull Weapon Base Two |
| 3 | dorsal | 2 | yes | 0.000 | 11.750 | 1.400 | Small Dorsal Hull Weapon Base Three |
| 3 | dorsal | 3 | yes | 0.000 | 11.750 | -25.250 | Small Dorsal Hull Weapon Base Four |
| 3 | ventral | 0 | yes | 0.000 | -11.750 | 47.500 | Small Ventral Hull Weapon Base One |
| 3 | ventral | 1 | yes | 0.000 | -11.750 | 19.200 | Small Ventral Hull Weapon Base Two |
| 3 | ventral | 2 | yes | 0.000 | -11.750 | 1.400 | Small Ventral Hull Weapon Base Three |
| 3 | ventral | 3 | yes | 0.000 | -11.750 | -25.250 | Small Ventral Hull Weapon Base Four |


### Destroyer

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 7.210 | 16.420 | Small Forward Dorsal Hull Weapon Base |
| 0 | dorsal | 1 | yes | 0.000 | 7.210 | -5.560 | Small Rear Dorsal Hull Weapon Base |
| 0 | ventral | 0 | yes | 0.000 | -7.210 | 16.420 | Small Forward Ventral Hull Weapon Base |
| 0 | ventral | 1 | yes | 0.000 | -7.210 | -5.560 | Small Rear Ventral Hull Weapon Base |
| 1 | dorsal | 0 | yes | 0.000 | 12.520 | 14.060 | Small Forward Dorsal Hull Weapon Base |
| 1 | dorsal | 1 | yes | 0.000 | 12.520 | -27.440 | Small Rear Dorsal Hull Weapon Base |
| 1 | ventral | 0 | yes | 0.000 | -12.520 | 14.060 | Small Forward Ventral Hull Weapon Base |
| 1 | ventral | 1 | yes | 0.000 | -12.520 | -27.440 | Small Rear Ventral Hull Weapon Base |
| 2 | dorsal | 0 | yes | 0.000 | 10.200 | 43.300 | Small Forward Dorsal Hull Weapon Base |
| 2 | dorsal | 1 | yes | 0.000 | 10.400 | -13.600 | Small Rear Dorsal Hull Weapon Base |
| 2 | ventral | 0 | yes | 0.000 | -10.200 | 43.300 | Small Forward Ventral Hull Weapon Base |
| 2 | ventral | 1 | yes | 0.000 | -10.400 | -13.600 | Small Rear Ventral Hull Weapon Base |
| 3 | dorsal | 0 | yes | 0.000 | 12.550 | 34.600 | Small Forward Dorsal Hull Weapon Base |
| 3 | dorsal | 1 | yes | 0.000 | 12.550 | -6.300 | Small Rear Dorsal Hull Weapon Base |
| 3 | ventral | 0 | yes | 0.000 | -12.550 | 34.600 | Small Forward Ventral Hull Weapon Base |
| 3 | ventral | 1 | yes | 0.000 | -12.550 | -6.300 | Small Rear Ventral Hull Weapon Base |


### Cruiser

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 7.350 | 47.370 | Hull Mount Dorsal Forward |
| 0 | dorsal | 1 | yes | 0.000 | 8.920 | 6.163 | Hull Mount Dorsal Middle |
| 0 | dorsal | 2 | yes | 0.000 | 13.100 | -19.947 | Hull Mount Dorsal Aft |
| 0 | ventral | 0 | yes | 0.000 | -3.722 | 49.617 | Hull Mount Ventral Forward |
| 0 | ventral | 1 | yes | 0.000 | -6.908 | 7.523 | Hull Mount Ventral Middle |
| 0 | ventral | 2 | yes | 0.000 | -6.210 | -33.507 | Hull Mount Ventral Aft |
| 1 | dorsal | 0 | yes | 0.000 | 13.540 | 35.529 | Hull Mount Dorsal Forward |
| 1 | dorsal | 1 | yes | 0.000 | 13.710 | 1.820 | Hull Mount Dorsal Middle |
| 1 | dorsal | 2 | yes | 0.000 | 11.891 | -34.678 | Hull Mount Dorsal Aft |
| 1 | ventral | 0 | yes | 0.000 | -13.540 | 35.529 | Hull Mount Ventral Forward |
| 1 | ventral | 1 | yes | 0.000 | -13.710 | 1.820 | Hull Mount Ventral Middle |
| 1 | ventral | 2 | yes | 0.000 | -11.891 | -34.678 | Hull Mount Ventral Aft |
| 2 | dorsal | 0 | yes | 0.000 | 11.300 | 56.100 | Hull Mount Dorsal Forward |
| 2 | dorsal | 1 | yes | 0.000 | 11.300 | 10.600 | Hull Mount Dorsal Middle |
| 2 | dorsal | 2 | yes | 0.000 | 11.300 | -33.100 | Hull Mount Dorsal Aft |
| 2 | ventral | 0 | yes | 0.000 | -11.300 | 56.100 | Hull Mount Ventral Forward |
| 2 | ventral | 1 | yes | 0.000 | -11.300 | 10.600 | Hull Mount Ventral Middle |
| 2 | ventral | 2 | yes | 0.000 | -11.300 | -33.100 | Hull Mount Ventral Aft |
| 3 | dorsal | 0 | yes | 0.000 | 16.650 | 37.750 | Hull Mount Dorsal Forward |
| 3 | dorsal | 1 | yes | 0.000 | 16.100 | -4.250 | Hull Mount Dorsal Middle |
| 3 | dorsal | 2 | yes | 0.000 | 17.400 | -43.300 | Hull Mount Dorsal Aft |
| 3 | ventral | 0 | yes | 0.000 | -16.650 | 37.750 | Hull Mount Ventral Forward |
| 3 | ventral | 1 | yes | 0.000 | -16.100 | -4.250 | Hull Mount Ventral Middle |
| 3 | ventral | 2 | yes | 0.000 | -17.400 | -43.300 | Hull Mount Ventral Aft |


### Battlecruiser

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 10.270 | 29.983 | Hull Mount Dorsal Forward |
| 0 | dorsal | 1 | yes | 0.000 | 12.200 | -34.500 | Hull Mount Dorsal Aft |
| 0 | ventral | 0 | yes | 0.000 | -3.722 | 29.983 | Hull Mount Ventral Forward |
| 0 | ventral | 1 | yes | 0.000 | -6.210 | -34.500 | Hull Mount Ventral Aft |
| 1 | dorsal | 0 | yes | 0.000 | 15.160 | 8.520 | Hull Mount Dorsal Forward |
| 1 | dorsal | 1 | yes | 0.000 | 16.280 | -31.400 | Hull Mount Dorsal Aft |
| 1 | ventral | 0 | yes | 0.000 | -15.160 | 8.520 | Hull Mount Ventral Forward |
| 1 | ventral | 1 | yes | 0.000 | -16.280 | -31.400 | Hull Mount Ventral Aft |
| 2 | dorsal | 0 | yes | 0.000 | 10.500 | 71.100 | Hull Mount Dorsal Forward |
| 2 | dorsal | 1 | yes | 0.000 | 10.500 | -3.600 | Hull Mount Dorsal Aft |
| 2 | ventral | 0 | yes | 0.000 | -10.500 | 71.100 | Hull Mount Ventral Forward |
| 2 | ventral | 1 | yes | 0.000 | -10.500 | -3.600 | Hull Mount Ventral Aft |
| 3 | dorsal | 0 | yes | 0.000 | 18.900 | 36.400 | Hull Mount Dorsal Forward |
| 3 | dorsal | 1 | yes | 0.000 | 18.900 | -12.000 | Hull Mount Dorsal Aft |
| 3 | ventral | 0 | yes | 0.000 | -18.900 | 36.400 | Hull Mount Ventral Forward |
| 3 | ventral | 1 | yes | 0.000 | -18.900 | -12.000 | Hull Mount Ventral Aft |


### Battleship

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 12.500 | 71.000 | Hull Mount Dorsal 1 |
| 0 | dorsal | 1 | yes | 0.000 | 12.500 | 35.000 | Hull Mount Dorsal 2 |
| 0 | dorsal | 2 | yes | 0.000 | 12.500 | 9.000 | Hull Mount Dorsal 3 |
| 0 | dorsal | 3 | yes | 0.000 | 14.450 | -23.000 | Hull Mount Dorsal 4 |
| 0 | dorsal | 4 | yes | 0.000 | 14.450 | -36.330 | Hull Mount Dorsal 5 |
| 0 | dorsal | 5 | yes | 0.000 | 15.620 | -72.710 | Hull Mount Dorsal 6 |
| 0 | ventral | 0 | yes | 0.000 | -9.750 | 71.000 | Hull Mount Ventral 1 |
| 0 | ventral | 1 | yes | 0.000 | -9.752 | 35.000 | Hull Mount Ventral 2 |
| 0 | ventral | 2 | yes | 0.000 | -9.727 | 9.000 | Hull Mount Ventral 3 |
| 0 | ventral | 3 | yes | 0.000 | -11.720 | -23.000 | Hull Mount Ventral 4 |
| 0 | ventral | 4 | yes | 0.000 | -11.720 | -37.320 | Hull Mount Ventral 5 |
| 0 | ventral | 5 | yes | 0.000 | -8.650 | -72.710 | Hull Mount Ventral 6 |
| 1 | dorsal | 0 | yes | 0.000 | 15.000 | 68.700 | Hull Mount Dorsal 1 |
| 1 | dorsal | 1 | yes | 0.000 | 15.000 | 44.450 | Hull Mount Dorsal 2 |
| 1 | dorsal | 2 | yes | 0.000 | 15.000 | 15.600 | Hull Mount Dorsal 3 |
| 1 | dorsal | 3 | yes | 0.000 | 15.000 | -3.200 | Hull Mount Dorsal 4 |
| 1 | dorsal | 4 | yes | 0.000 | 15.000 | -25.600 | Hull Mount Dorsal 5 |
| 1 | dorsal | 5 | yes | 0.000 | 15.000 | -56.800 | Hull Mount Dorsal 6 |
| 1 | ventral | 0 | yes | 0.000 | -15.000 | 68.700 | Hull Mount Ventral 1 |
| 1 | ventral | 1 | yes | 0.000 | -15.000 | 44.450 | Hull Mount Ventral 2 |
| 1 | ventral | 2 | yes | 0.000 | -15.000 | 15.600 | Hull Mount Ventral 3 |
| 1 | ventral | 3 | yes | 0.000 | -15.000 | -3.200 | Hull Mount Ventral 4 |
| 1 | ventral | 4 | yes | 0.000 | -15.000 | -25.600 | Hull Mount Ventral 5 |
| 1 | ventral | 5 | yes | 0.000 | -15.000 | -56.800 | Hull Mount Ventral 6 |
| 2 | dorsal | 0 | yes | 0.000 | 12.200 | 89.900 | Hull Mount Dorsal 1 |
| 2 | dorsal | 1 | yes | 0.000 | 11.800 | 58.300 | Hull Mount Dorsal 2 |
| 2 | dorsal | 2 | yes | 0.000 | 12.200 | 43.500 | Hull Mount Dorsal 3 |
| 2 | dorsal | 3 | yes | 0.000 | 11.800 | 28.700 | Hull Mount Dorsal 4 |
| 2 | dorsal | 4 | yes | 0.000 | 12.200 | -32.900 | Hull Mount Dorsal 5 |
| 2 | dorsal | 5 | yes | 0.000 | 12.200 | -46.800 | Hull Mount Dorsal 6 |
| 2 | ventral | 0 | yes | 0.000 | -12.200 | 89.900 | Hull Mount Ventral 1 |
| 2 | ventral | 1 | yes | 0.000 | -11.800 | 58.300 | Hull Mount Ventral 2 |
| 2 | ventral | 2 | yes | 0.000 | -12.200 | 43.500 | Hull Mount Ventral 3 |
| 2 | ventral | 3 | yes | 0.000 | -11.800 | 28.700 | Hull Mount Ventral 4 |
| 2 | ventral | 4 | yes | 0.000 | -12.200 | -32.900 | Hull Mount Ventral 5 |
| 2 | ventral | 5 | yes | 0.000 | -12.200 | -46.800 | Hull Mount Ventral 6 |
| 3 | dorsal | 0 | yes | 0.000 | 12.800 | 57.800 | Hull Mount Dorsal 1 |
| 3 | dorsal | 1 | yes | 0.000 | 12.800 | 33.640 | Hull Mount Dorsal 2 |
| 3 | dorsal | 2 | yes | 0.000 | 13.200 | 7.950 | Hull Mount Dorsal 3 |
| 3 | dorsal | 3 | yes | 0.000 | 13.200 | -1.210 | Hull Mount Dorsal 4 |
| 3 | dorsal | 4 | yes | 0.000 | 13.139 | -26.674 | Hull Mount Dorsal 5 |
| 3 | dorsal | 5 | yes | 0.000 | 13.139 | -50.500 | Hull Mount Dorsal 6 |
| 3 | ventral | 0 | yes | 0.000 | -12.800 | 57.800 | Hull Mount Ventral 1 |
| 3 | ventral | 1 | yes | 0.000 | -12.800 | 33.640 | Hull Mount Ventral 2 |
| 3 | ventral | 2 | yes | 0.000 | -13.200 | 7.950 | Hull Mount Ventral 3 |
| 3 | ventral | 3 | yes | 0.000 | -13.200 | -1.210 | Hull Mount Ventral 4 |
| 3 | ventral | 4 | yes | 0.000 | -13.139 | -26.674 | Hull Mount Ventral 5 |
| 3 | ventral | 5 | yes | 0.000 | -13.139 | -50.500 | Hull Mount Ventral 6 |


### Lancer

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 8.000 | 65.930 | Hull Mount Dorsal 1 |
| 0 | dorsal | 1 | yes | 0.000 | 8.000 | 33.090 | Hull Mount Dorsal 2 |
| 0 | dorsal | 2 | yes | 0.000 | 10.000 | -37.000 | Hull Mount Dorsal 3 |
| 0 | ventral | 0 | yes | 0.000 | -8.000 | 65.930 | Hull Mount Ventral 1 |
| 0 | ventral | 1 | yes | 0.000 | -8.000 | 33.090 | Hull Mount Ventral 2 |
| 0 | ventral | 2 | yes | 0.000 | -10.000 | -37.000 | Hull Mount Ventral 3 |
| 1 | dorsal | 0 | yes | 0.000 | 12.100 | 39.400 | Hull Mount Dorsal 1 |
| 1 | dorsal | 1 | yes | 0.000 | 12.100 | -28.100 | Hull Mount Dorsal 2 |
| 1 | dorsal | 2 | yes | 0.000 | 12.100 | -59.900 | Hull Mount Dorsal 3 |
| 1 | ventral | 0 | yes | 0.000 | -12.100 | 39.400 | Hull Mount Ventral 1 |
| 1 | ventral | 1 | yes | 0.000 | -12.100 | -28.100 | Hull Mount Ventral 2 |
| 1 | ventral | 2 | yes | 0.000 | -12.100 | -59.900 | Hull Mount Ventral 3 |
| 2 | dorsal | 0 | yes | 0.000 | 15.700 | 76.800 | Hull Mount Dorsal 1 |
| 2 | dorsal | 1 | yes | 0.000 | 14.400 | -25.000 | Hull Mount Dorsal 2 |
| 2 | dorsal | 2 | yes | 0.000 | 15.100 | -54.600 | Hull Mount Dorsal 3 |
| 2 | ventral | 0 | yes | 0.000 | -15.700 | 76.800 | Hull Mount Ventral 1 |
| 2 | ventral | 1 | yes | 0.000 | -14.400 | -25.000 | Hull Mount Ventral 2 |
| 2 | ventral | 2 | yes | 0.000 | -15.100 | -54.600 | Hull Mount Ventral 3 |
| 3 | dorsal | 0 | yes | 0.000 | 17.800 | 3.980 | Hull Mount Dorsal 1 |
| 3 | dorsal | 1 | yes | 0.000 | 17.800 | -58.100 | Hull Mount Dorsal 2 |
| 3 | dorsal | 2 | yes | 0.000 | 17.800 | -79.500 | Hull Mount Dorsal 3 |
| 3 | ventral | 0 | yes | 0.000 | -17.800 | 3.980 | Hull Mount Ventral 1 |
| 3 | ventral | 1 | yes | 0.000 | -17.800 | -58.100 | Hull Mount Ventral 2 |
| 3 | ventral | 2 | yes | 0.000 | -17.800 | -79.500 | Hull Mount Ventral 3 |


### Dreadnought

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 15.000 | 101.750 | Hull Mount Dorsal 1 |
| 0 | dorsal | 1 | yes | 0.000 | 15.000 | 92.750 | Hull Mount Dorsal 2 |
| 0 | dorsal | 2 | yes | 0.000 | 15.000 | 83.920 | Hull Mount Dorsal 3 |
| 0 | dorsal | 3 | yes | 0.000 | 14.000 | 53.000 | Hull Mount Dorsal 4 |
| 0 | dorsal | 4 | yes | 0.000 | 14.000 | 38.000 | Hull Mount Dorsal 5 |
| 0 | dorsal | 5 | yes | 0.000 | 15.000 | 7.150 | Hull Mount Dorsal 6 |
| 0 | dorsal | 6 | yes | 0.000 | 15.000 | -2.150 | Hull Mount Dorsal 7 |
| 0 | dorsal | 7 | yes | 0.000 | 15.000 | -10.750 | Hull Mount Dorsal 8 |
| 0 | ventral | 0 | yes | 0.000 | -15.000 | 101.750 | Hull Mount Ventral 1 |
| 0 | ventral | 1 | yes | 0.000 | -15.000 | 92.750 | Hull Mount Ventral 2 |
| 0 | ventral | 2 | yes | 0.000 | -15.000 | 83.920 | Hull Mount Ventral 3 |
| 0 | ventral | 3 | yes | 0.000 | -14.000 | 53.000 | Hull Mount Ventral 4 |
| 0 | ventral | 4 | yes | 0.000 | -14.000 | 38.000 | Hull Mount Ventral 5 |
| 0 | ventral | 5 | yes | 0.000 | -15.000 | 7.150 | Hull Mount Ventral 6 |
| 0 | ventral | 6 | yes | 0.000 | -15.000 | -2.150 | Hull Mount Ventral 7 |
| 0 | ventral | 7 | yes | 0.000 | -15.000 | -10.750 | Hull Mount Ventral 8 |
| 1 | dorsal | 0 | yes | 0.000 | 17.700 | 118.100 | Hull Mount Dorsal 1 |
| 1 | dorsal | 1 | yes | 0.000 | 17.740 | 38.800 | Hull Mount Dorsal 2 |
| 1 | dorsal | 2 | yes | 0.000 | 17.740 | 31.880 | Hull Mount Dorsal 3 |
| 1 | dorsal | 3 | yes | 0.000 | 17.740 | 18.060 | Hull Mount Dorsal 4 |
| 1 | dorsal | 4 | yes | 0.000 | 17.740 | 9.655 | Hull Mount Dorsal 5 |
| 1 | dorsal | 5 | yes | 0.000 | 17.740 | -3.000 | Hull Mount Dorsal 6 |
| 1 | dorsal | 6 | yes | 0.000 | 17.740 | -23.550 | Hull Mount Dorsal 7 |
| 1 | dorsal | 7 | yes | 0.000 | 17.740 | -38.650 | Hull Mount Dorsal 8 |
| 1 | ventral | 0 | yes | 0.000 | -17.700 | 118.100 | Hull Mount Ventral 1 |
| 1 | ventral | 1 | yes | 0.000 | -17.740 | 38.800 | Hull Mount Ventral 2 |
| 1 | ventral | 2 | yes | 0.000 | -17.740 | 31.880 | Hull Mount Ventral 3 |
| 1 | ventral | 3 | yes | 0.000 | -17.740 | 18.060 | Hull Mount Ventral 4 |
| 1 | ventral | 4 | yes | 0.000 | -17.740 | 9.655 | Hull Mount Ventral 5 |
| 1 | ventral | 5 | yes | 0.000 | -17.740 | -3.000 | Hull Mount Ventral 6 |
| 1 | ventral | 6 | yes | 0.000 | -17.740 | -23.550 | Hull Mount Ventral 7 |
| 1 | ventral | 7 | yes | 0.000 | -17.740 | -38.650 | Hull Mount Ventral 8 |
| 2 | dorsal | 0 | yes | 0.000 | 13.900 | 129.400 | Hull Mount Dorsal 1 |
| 2 | dorsal | 1 | yes | 0.000 | 13.900 | 70.400 | Hull Mount Dorsal 2 |
| 2 | dorsal | 2 | yes | 0.000 | 13.900 | 56.950 | Hull Mount Dorsal 3 |
| 2 | dorsal | 3 | yes | 0.000 | 13.900 | 8.500 | Hull Mount Dorsal 4 |
| 2 | dorsal | 4 | yes | 0.000 | 13.900 | -4.900 | Hull Mount Dorsal 5 |
| 2 | dorsal | 5 | yes | 0.000 | 13.900 | -43.200 | Hull Mount Dorsal 6 |
| 2 | dorsal | 6 | yes | 0.000 | 13.900 | -68.800 | Hull Mount Dorsal 7 |
| 2 | dorsal | 7 | yes | 0.000 | 13.900 | -82.250 | Hull Mount Dorsal 8 |
| 2 | ventral | 0 | yes | 0.000 | -13.900 | 129.400 | Hull Mount Ventral 1 |
| 2 | ventral | 1 | yes | 0.000 | -13.900 | 70.400 | Hull Mount Ventral 2 |
| 2 | ventral | 2 | yes | 0.000 | -13.900 | 56.950 | Hull Mount Ventral 3 |
| 2 | ventral | 3 | yes | 0.000 | -13.900 | 8.500 | Hull Mount Ventral 4 |
| 2 | ventral | 4 | yes | 0.000 | -13.900 | -4.900 | Hull Mount Ventral 5 |
| 2 | ventral | 5 | yes | 0.000 | -13.900 | -43.200 | Hull Mount Ventral 6 |
| 2 | ventral | 6 | yes | 0.000 | -13.900 | -68.800 | Hull Mount Ventral 7 |
| 2 | ventral | 7 | yes | 0.000 | -13.900 | -82.250 | Hull Mount Ventral 8 |
| 3 | dorsal | 0 | yes | 0.000 | 22.000 | 125.000 | Hull Mount Dorsal 1 |
| 3 | dorsal | 1 | yes | 0.000 | 22.000 | 103.700 | Hull Mount Dorsal 2 |
| 3 | dorsal | 2 | yes | 0.000 | 22.000 | 67.350 | Hull Mount Dorsal 3 |
| 3 | dorsal | 3 | yes | 0.000 | 22.000 | 39.000 | Hull Mount Dorsal 4 |
| 3 | dorsal | 4 | yes | 0.000 | 22.000 | 6.500 | Hull Mount Dorsal 5 |
| 3 | dorsal | 5 | yes | 0.000 | 22.000 | -14.800 | Hull Mount Dorsal 6 |
| 3 | dorsal | 6 | yes | 0.000 | 22.000 | -37.300 | Hull Mount Dorsal 7 |
| 3 | dorsal | 7 | yes | 0.000 | 22.000 | -58.100 | Hull Mount Dorsal 8 |
| 3 | ventral | 0 | yes | 0.000 | -22.000 | 125.000 | Hull Mount Ventral 1 |
| 3 | ventral | 1 | yes | 0.000 | -22.000 | 103.700 | Hull Mount Ventral 2 |
| 3 | ventral | 2 | yes | 0.000 | -22.000 | 67.350 | Hull Mount Ventral 3 |
| 3 | ventral | 3 | yes | 0.000 | -22.000 | 39.000 | Hull Mount Ventral 4 |
| 3 | ventral | 4 | yes | 0.000 | -22.000 | 6.500 | Hull Mount Ventral 5 |
| 3 | ventral | 5 | yes | 0.000 | -22.000 | -14.800 | Hull Mount Ventral 6 |
| 3 | ventral | 6 | yes | 0.000 | -22.000 | -37.300 | Hull Mount Ventral 7 |
| 3 | ventral | 7 | yes | 0.000 | -22.000 | -58.100 | Hull Mount Ventral 8 |


### Titan

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 19.830 | 138.570 | Hull Mount Dorsal 1 |
| 0 | dorsal | 1 | yes | 0.000 | 19.830 | 121.420 | Hull Mount Dorsal 2 |
| 0 | dorsal | 2 | yes | 0.000 | 19.000 | 68.930 | Hull Mount Dorsal 3 |
| 0 | dorsal | 3 | yes | 0.000 | 19.000 | 55.890 | Hull Mount Dorsal 4 |
| 0 | dorsal | 4 | yes | 0.000 | 19.700 | 3.390 | Hull Mount Dorsal 5 |
| 0 | dorsal | 5 | yes | 0.000 | 19.700 | -16.575 | Hull Mount Dorsal 6 |
| 0 | ventral | 0 | yes | 0.000 | -19.830 | 138.570 | Hull Mount Ventral 1 |
| 0 | ventral | 1 | yes | 0.000 | -19.830 | 121.420 | Hull Mount Ventral 2 |
| 0 | ventral | 2 | yes | 0.000 | -19.000 | 68.930 | Hull Mount Ventral 3 |
| 0 | ventral | 3 | yes | 0.000 | -19.000 | 55.890 | Hull Mount Ventral 4 |
| 0 | ventral | 4 | yes | 0.000 | -19.700 | 3.390 | Hull Mount Ventral 5 |
| 0 | ventral | 5 | yes | 0.000 | -19.700 | -16.575 | Hull Mount Ventral 6 |
| 1 | dorsal | 0 | yes | 0.000 | 26.600 | 97.110 | Hull Mount Dorsal 1 |
| 1 | dorsal | 1 | yes | 0.000 | 26.600 | 70.860 | Hull Mount Dorsal 2 |
| 1 | dorsal | 2 | yes | 0.000 | 26.600 | 41.580 | Hull Mount Dorsal 3 |
| 1 | dorsal | 3 | yes | 0.000 | 26.600 | 35.140 | Hull Mount Dorsal 4 |
| 1 | dorsal | 4 | yes | 0.000 | 26.600 | 2.360 | Hull Mount Dorsal 5 |
| 1 | dorsal | 5 | yes | 0.000 | 26.600 | -5.950 | Hull Mount Dorsal 6 |
| 1 | ventral | 0 | yes | 0.000 | -26.600 | 97.110 | Hull Mount Ventral 1 |
| 1 | ventral | 1 | yes | 0.000 | -26.600 | 70.860 | Hull Mount Ventral 2 |
| 1 | ventral | 2 | yes | 0.000 | -26.600 | 41.580 | Hull Mount Ventral 3 |
| 1 | ventral | 3 | yes | 0.000 | -26.600 | 35.140 | Hull Mount Ventral 4 |
| 1 | ventral | 4 | yes | 0.000 | -26.600 | 2.360 | Hull Mount Ventral 5 |
| 1 | ventral | 5 | yes | 0.000 | -26.600 | -5.950 | Hull Mount Ventral 6 |
| 2 | dorsal | 0 | yes | 0.000 | 19.300 | 153.000 | Hull Mount Dorsal 1 |
| 2 | dorsal | 1 | yes | 0.000 | 19.300 | 125.000 | Hull Mount Dorsal 2 |
| 2 | dorsal | 2 | yes | 0.000 | 19.300 | 63.700 | Hull Mount Dorsal 3 |
| 2 | dorsal | 3 | yes | 0.000 | 19.300 | 23.000 | Hull Mount Dorsal 4 |
| 2 | dorsal | 4 | yes | 0.000 | 19.300 | -62.400 | Hull Mount Dorsal 5 |
| 2 | dorsal | 5 | yes | 0.000 | 19.300 | -80.300 | Hull Mount Dorsal 6 |
| 2 | ventral | 0 | yes | 0.000 | -19.300 | 153.000 | Hull Mount Ventral 1 |
| 2 | ventral | 1 | yes | 0.000 | -19.300 | 125.000 | Hull Mount Ventral 2 |
| 2 | ventral | 2 | yes | 0.000 | -19.300 | 63.700 | Hull Mount Ventral 3 |
| 2 | ventral | 3 | yes | 0.000 | -19.300 | 23.000 | Hull Mount Ventral 4 |
| 2 | ventral | 4 | yes | 0.000 | -19.300 | -62.400 | Hull Mount Ventral 5 |
| 2 | ventral | 5 | yes | 0.000 | -19.300 | -80.300 | Hull Mount Ventral 6 |
| 3 | dorsal | 0 | yes | 0.000 | 31.100 | 120.000 | Hull Mount Dorsal 1 |
| 3 | dorsal | 1 | yes | 0.000 | 31.100 | 72.200 | Hull Mount Dorsal 2 |
| 3 | dorsal | 2 | yes | 0.000 | 31.100 | 23.160 | Hull Mount Dorsal 3 |
| 3 | dorsal | 3 | yes | 0.000 | 31.100 | -40.500 | Hull Mount Dorsal 4 |
| 3 | dorsal | 4 | yes | 0.000 | 31.100 | -76.200 | Hull Mount Dorsal 5 |
| 3 | dorsal | 5 | yes | 0.000 | 31.100 | -105.200 | Hull Mount Dorsal 6 |
| 3 | ventral | 0 | yes | 0.000 | -31.100 | 120.000 | Hull Mount Ventral 1 |
| 3 | ventral | 1 | yes | 0.000 | -31.100 | 72.200 | Hull Mount Ventral 2 |
| 3 | ventral | 2 | yes | 0.000 | -31.100 | 23.160 | Hull Mount Ventral 3 |
| 3 | ventral | 3 | yes | 0.000 | -31.100 | -40.500 | Hull Mount Ventral 4 |
| 3 | ventral | 4 | yes | 0.000 | -31.100 | -76.200 | Hull Mount Ventral 5 |
| 3 | ventral | 5 | yes | 0.000 | -31.100 | -105.200 | Hull Mount Ventral 6 |


### STOFighter

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 1.039 | -4.040 | Small Forward Dorsal Hull Weapon Base |
| 0 | ventral | 0 | yes | 0.000 | -3.412 | 0.798 | Small Forward Ventral Hull Weapon Base |


### AlienGunship

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |


### AlienEscort

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 5.000 | -13.200 | Dorsal Turret Mount Base |
| 0 | dorsal | 1 | yes | 0.000 | 5.000 | 5.500 | Dorsal Turret Mount Base (1) |
| 0 | ventral | 0 | yes | 0.000 | -5.000 | -13.200 | Ventral Turret Mount Base |
| 0 | ventral | 1 | yes | 0.000 | -5.000 | 5.500 | Ventral Turret Mount Base (1) |


### AlienCorvette

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 6.677 | -2.040 | Dorsal Turret Mount Base |
| 0 | ventral | 0 | yes | 0.000 | -3.817 | -2.560 | Ventral Turret Mount Base |


### AlienFrigate

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 4.720 | 40.500 | Dorsal Forward Turret Mount Base |
| 0 | dorsal | 1 | yes | 0.000 | 6.751 | -6.650 | Dorsal Mid Turret Mount Base |
| 0 | dorsal | 2 | yes | 0.000 | 6.756 | -31.950 | Dorsal Aft Turret Mount Base |
| 0 | ventral | 0 | yes | 0.000 | -4.522 | 40.500 | Ventral Forward Turret Mount Base |
| 0 | ventral | 1 | yes | 0.000 | -6.341 | -7.170 | Ventral Mid Turret Mount Base |
| 0 | ventral | 2 | yes | 0.000 | -6.349 | -32.470 | Ventral Aft Turret Mount Base  |


### AlienMonitor

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 8.000 | -37.200 | Dorsal Turret Mount Base |
| 0 | dorsal | 1 | yes | 0.000 | 8.000 | -19.000 | Dorsal Turret Mount Base (1) |
| 0 | dorsal | 2 | yes | 0.000 | 8.440 | 8.500 | Dorsal Turret Mount Base (2) |
| 0 | dorsal | 3 | yes | 0.000 | 8.440 | 26.650 | Dorsal Turret Mount Base (3) |
| 0 | ventral | 0 | yes | 0.000 | -8.000 | -37.200 | Ventral Turret Mount Base |
| 0 | ventral | 1 | yes | 0.000 | -8.000 | -19.000 | Ventral Turret Mount Base (1) |
| 0 | ventral | 2 | yes | 0.000 | -8.440 | 8.500 | Ventral Turret Mount Base (2) |
| 0 | ventral | 3 | yes | 0.000 | -8.440 | 26.650 | Ventral Turret Mount Base (3) |


### AlienDestroyer

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 8.850 | 3.000 | Forward Dorsal Turret Mount Base |
| 0 | dorsal | 1 | yes | 0.000 | 8.850 | -41.000 | Rear Dorsal Turret Mount Base |
| 0 | dorsal | 2 | extra | 0.000 | 8.850 | -20.000 | Dorsal Medium Turret Mount Base |
| 0 | ventral | 0 | yes | 0.000 | -7.157 | 3.000 | Forward Ventral Turret Mount Base |
| 0 | ventral | 1 | yes | 0.000 | -7.157 | -41.000 | Rear Ventral Turret Mount Base |
| 0 | ventral | 2 | extra | 0.000 | -7.157 | -20.000 | Dorsal Ventral Turret Mount Base |


### AlienCruiser

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 8.320 | -10.850 | Dorsal Turret Mount Base |
| 0 | dorsal | 1 | yes | 0.000 | 8.280 | 18.500 | Dorsal Turret Mount Base (1) |
| 0 | dorsal | 2 | yes | 0.000 | 7.725 | 59.910 | Dorsal Turret Mount Base (2) |
| 0 | dorsal | 3 | yes | 0.000 | 7.700 | 84.530 | Dorsal Turret Mount Base (3) |
| 0 | ventral | 0 | yes | 0.000 | -8.320 | -10.850 | Ventral Turret Mount Base |
| 0 | ventral | 1 | yes | 0.000 | -8.280 | 18.500 | Ventral Turret Mount Base (1) |
| 0 | ventral | 2 | yes | 0.000 | -7.800 | 59.910 | Ventral Turret Mount Base (2) |
| 0 | ventral | 3 | yes | 0.000 | -7.850 | 84.530 | Ventral Turret Mount Base (3) |


### AlienBattlecruiser

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 9.832 | 94.100 | Dorsal Turret Mount Base 1 |
| 0 | dorsal | 1 | yes | 0.000 | 11.311 | 54.450 | Dorsal Turret Mount Base 2 |
| 0 | dorsal | 2 | yes | 0.000 | 11.458 | -8.380 | Dorsal Turret Mount Base 3 |
| 0 | ventral | 0 | yes | 0.000 | -9.678 | 94.100 | Ventral Turret Mount Base 1 |
| 0 | ventral | 1 | yes | 0.000 | -11.352 | 54.450 | Ventral Turret Mount Base 2 |
| 0 | ventral | 2 | yes | 0.000 | -11.817 | -8.380 | Ventral Turret Mount Base 3 |


### AlienBattleship

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 11.400 | 119.500 | Dorsal Turret Mount Base 1 |
| 0 | dorsal | 1 | yes | 0.000 | 11.400 | 95.000 | Dorsal Turret Mount Base 2 |
| 0 | dorsal | 2 | yes | 0.000 | 11.400 | 41.700 | Dorsal Turret Mount Base 3 |
| 0 | dorsal | 3 | yes | 0.000 | 11.400 | 16.750 | Dorsal Turret Mount Base 4 |
| 0 | dorsal | 4 | yes | 0.000 | 12.500 | -40.750 | Dorsal Turret Mount Base 5 |
| 0 | dorsal | 5 | yes | 0.000 | 12.500 | -85.000 | Dorsal Turret Mount Base 6 |
| 0 | ventral | 0 | yes | 0.000 | -11.400 | 119.500 | Ventral Turret Mount Base 1 |
| 0 | ventral | 1 | yes | 0.000 | -11.400 | 95.000 | Ventral Turret Mount Base 2 |
| 0 | ventral | 2 | yes | 0.000 | -11.400 | 41.700 | Ventral Turret Mount Base 3 |
| 0 | ventral | 3 | yes | 0.000 | -11.400 | 16.750 | Ventral Turret Mount Base 4 |
| 0 | ventral | 4 | yes | 0.000 | -12.500 | -40.750 | Ventral Turret Mount Base 5 |
| 0 | ventral | 5 | yes | 0.000 | -12.500 | -85.000 | Ventral Turret Mount Base 6 |


### AlienLancer

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 12.350 | -93.600 | Dorsal Turret Mount Base |
| 0 | dorsal | 1 | yes | 0.000 | 12.350 | -53.500 | Dorsal Turret Mount Base (1) |
| 0 | dorsal | 2 | yes | 0.000 | 8.450 | 17.250 | Dorsal Turret Mount Base (2) |
| 0 | dorsal | 3 | yes | 0.000 | 8.750 | 88.125 | Dorsal Turret Mount Base (3) |
| 0 | ventral | 0 | yes | 0.000 | -11.730 | -93.600 | Ventral Turret Mount Base |
| 0 | ventral | 1 | yes | 0.000 | -11.730 | -53.500 | Ventral Turret Mount Base (1) |
| 0 | ventral | 2 | yes | 0.000 | -8.450 | 17.250 | Ventral Turret Mount Base (2) |
| 0 | ventral | 3 | yes | 0.000 | -8.750 | 88.125 | Ventral Turret Mount Base (3) |


### AlienDreadnought

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 2.000 | 14.304 | 50.750 | Dorsal Turret Mount Base FFR |
| 0 | dorsal | 1 | yes | -2.000 | 14.304 | 50.750 | Dorsal Turret Mount Base FFL |
| 0 | dorsal | 2 | extra | 0.000 | 14.304 | 50.790 | Dorsal Turret Mount Base FFC |
| 0 | dorsal | 3 | yes | 2.000 | 14.304 | 42.570 | Dorsal Turret Mount Base FBR |
| 0 | dorsal | 4 | yes | -2.000 | 14.304 | 42.570 | Dorsal Turret Mount Base FBL |
| 0 | dorsal | 5 | extra | 0.000 | 14.304 | 42.480 | Dorsal Turret Mount Base FBC |
| 0 | dorsal | 6 | yes | 2.000 | 14.304 | 4.210 | Dorsal Turret Mount Base BFR |
| 0 | dorsal | 7 | yes | -2.000 | 14.304 | 4.210 | Dorsal Turret Mount Base BFL |
| 0 | dorsal | 8 | extra | 0.000 | 14.304 | 4.250 | Dorsal Turret Mount Base BFC |
| 0 | dorsal | 9 | yes | 2.000 | 14.304 | -13.510 | Dorsal Turret Mount Base BBR |
| 0 | dorsal | 10 | yes | -2.000 | 14.304 | -13.510 | Dorsal Turret Mount Base BBL |
| 0 | dorsal | 11 | extra | 0.000 | 14.304 | -13.600 | Dorsal Turret Mount Base BBC |
| 0 | ventral | 0 | yes | 2.000 | -14.304 | 50.750 | Ventral Turret Mount Base FFR (1) |
| 0 | ventral | 1 | yes | -2.000 | -14.304 | 50.750 | Ventral Turret Mount Base FFL (1) |
| 0 | ventral | 2 | extra | 0.000 | -14.304 | 50.790 | Ventral Turret Mount Base FFC (1) |
| 0 | ventral | 3 | yes | 2.000 | -14.304 | 42.570 | Ventral Turret Mount Base FBR (1) |
| 0 | ventral | 4 | yes | -2.000 | -14.304 | 42.570 | Ventral Turret Mount Base FBL (1) |
| 0 | ventral | 5 | extra | 0.000 | -14.304 | 42.480 | Ventral Turret Mount Base FBC (1) |
| 0 | ventral | 6 | yes | 2.000 | -14.304 | 4.210 | Ventral Turret Mount Base BFR (1) |
| 0 | ventral | 7 | yes | -2.000 | -14.304 | 4.210 | Ventral Turret Mount Base BFL (1) |
| 0 | ventral | 8 | extra | 0.000 | -14.304 | 4.250 | Ventral Turret Mount Base BFC (1) |
| 0 | ventral | 9 | yes | 2.000 | -14.304 | -13.510 | Ventral Turret Mount Base BBR (1) |
| 0 | ventral | 10 | yes | -2.000 | -14.304 | -13.510 | Ventral Turret Mount Base BBL (1) |
| 0 | ventral | 11 | extra | 0.000 | -14.304 | -13.600 | Ventral Turret Mount Base BBC (1) |


### AlienTitan

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 14.400 | -125.500 | Dorsal Turret Mount Base |
| 0 | dorsal | 1 | yes | 0.000 | 14.400 | -101.200 | Dorsal Turret Mount Base (1) |
| 0 | dorsal | 2 | yes | 0.000 | 14.400 | -69.700 | Dorsal Turret Mount Base (2) |
| 0 | dorsal | 3 | yes | 0.000 | 14.400 | -41.500 | Dorsal Turret Mount Base (3) |
| 0 | dorsal | 4 | yes | 0.000 | 14.400 | 22.000 | Dorsal Turret Mount Base (4) |
| 0 | dorsal | 5 | yes | 0.000 | 14.400 | 58.850 | Dorsal Turret Mount Base (5) |
| 0 | dorsal | 6 | yes | 0.000 | 14.400 | 101.750 | Dorsal Turret Mount Base (6) |
| 0 | dorsal | 7 | yes | 0.000 | 14.400 | 124.900 | Dorsal Turret Mount Base (7) |
| 0 | ventral | 0 | yes | 0.000 | -14.400 | -125.500 | Ventral Turret Mount Base |
| 0 | ventral | 1 | yes | 0.000 | -14.400 | -101.200 | Ventral Turret Mount Base (1) |
| 0 | ventral | 2 | yes | 0.000 | -14.400 | -69.700 | Ventral Turret Mount Base (2) |
| 0 | ventral | 3 | yes | 0.000 | -14.400 | -41.500 | Ventral Turret Mount Base (3) |
| 0 | ventral | 4 | yes | 0.000 | -14.400 | 22.000 | Ventral Turret Mount Base (4) |
| 0 | ventral | 5 | yes | 0.000 | -14.400 | 58.850 | Ventral Turret Mount Base (5) |
| 0 | ventral | 6 | yes | 0.000 | -14.400 | 101.750 | Ventral Turret Mount Base (6) |
| 0 | ventral | 7 | yes | 0.000 | -14.400 | 124.900 | Ventral Turret Mount Base (7) |


### AlienAssaultCarrier

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 14.304 | 52.000 | Dorsal Turret Mount Base 1 |
| 0 | dorsal | 1 | yes | 0.000 | 14.304 | 40.000 | Dorsal Turret Mount Base 2 |
| 0 | dorsal | 2 | yes | 0.000 | 14.304 | 28.000 | Dorsal Turret Mount Base 3 |
| 0 | dorsal | 3 | yes | 0.000 | 14.304 | -6.000 | Dorsal Turret Mount Base 4 |
| 0 | dorsal | 4 | yes | 0.000 | 14.304 | -18.000 | Dorsal Turret Mount Base 5 |
| 0 | dorsal | 5 | yes | 0.000 | 14.304 | -30.000 | Dorsal Turret Mount Base 6 |
| 0 | ventral | 0 | yes | 0.000 | -14.304 | 52.000 | Ventral Turret Mount Base 1 |
| 0 | ventral | 1 | yes | 0.000 | -14.304 | 40.000 | Ventral Turret Mount Base 2 |
| 0 | ventral | 2 | yes | 0.000 | -14.304 | 28.000 | Ventral Turret Mount Base 3 |
| 0 | ventral | 3 | yes | 0.000 | -14.304 | -6.000 | Ventral Turret Mount Base 4 |
| 0 | ventral | 4 | yes | 0.000 | -14.304 | -18.000 | Ventral Turret Mount Base 5 |
| 0 | ventral | 5 | yes | 0.000 | -14.304 | -30.000 | Ventral Turret Mount Base 6 |


### AlienMothership

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | -108.500 | 25.000 | 249.000 | Dorsal Turret Mount Base 70 |
| 0 | dorsal | 1 | yes | -196.500 | 25.100 | 96.000 | Dorsal Turret Mount Base 60 |
| 0 | dorsal | 2 | yes | -196.500 | 25.350 | -161.400 | Dorsal Turret Mount Base 50 |
| 0 | dorsal | 3 | yes | -88.400 | 25.350 | -273.750 | Dorsal Turret Mount Base 40 |
| 0 | dorsal | 4 | yes | -4.000 | 88.890 | 3.000 | Dorsal Turret Mount Base 72 |
| 0 | dorsal | 5 | extra | 0.000 | 88.890 | 3.000 | Dorsal Turret Mount Base 7C |
| 0 | dorsal | 6 | yes | 4.000 | 88.890 | 3.000 | Dorsal Turret Mount Base 74 |
| 0 | dorsal | 7 | yes | -4.000 | 88.910 | -7.000 | Dorsal Turret Mount Base 62 |
| 0 | dorsal | 8 | extra | 0.000 | 88.910 | -7.000 | Dorsal Turret Mount Base 6C |
| 0 | dorsal | 9 | yes | 4.000 | 88.910 | -7.000 | Dorsal Turret Mount Base 64 |
| 0 | dorsal | 10 | yes | -4.000 | 88.910 | -17.000 | Dorsal Turret Mount Base 52 |
| 0 | dorsal | 11 | extra | 0.000 | 88.910 | -17.000 | Dorsal Turret Mount Base 5C |
| 0 | dorsal | 12 | yes | 4.000 | 88.910 | -17.000 | Dorsal Turret Mount Base 54 |
| 0 | dorsal | 13 | yes | -4.000 | 88.910 | -27.000 | Dorsal Turret Mount Base 42 |
| 0 | dorsal | 14 | extra | 0.000 | 88.910 | -27.000 | Dorsal Turret Mount Base 4C |
| 0 | dorsal | 15 | yes | 4.000 | 88.910 | -27.000 | Dorsal Turret Mount Base 44 |
| 0 | dorsal | 16 | yes | 108.500 | 25.000 | 249.000 | Dorsal Turret Mount Base 76 |
| 0 | dorsal | 17 | yes | 196.500 | 25.100 | 96.000 | Dorsal Turret Mount Base 66 |
| 0 | dorsal | 18 | yes | 196.500 | 25.350 | -161.400 | Dorsal Turret Mount Base 56 |
| 0 | dorsal | 19 | yes | 88.400 | 25.350 | -273.750 | Dorsal Turret Mount Base 46 |
| 0 | ventral | 0 | yes | -108.500 | -22.966 | 249.000 | Ventral Turret Mount Base 70 |
| 0 | ventral | 1 | yes | -196.500 | -23.180 | 96.000 | Ventral Turret Mount Base 60 |
| 0 | ventral | 2 | yes | -196.500 | -23.590 | -161.400 | Ventral Turret Mount Base 50 |
| 0 | ventral | 3 | yes | -88.400 | -23.590 | -273.750 | Ventral Turret Mount Base 40 |
| 0 | ventral | 4 | yes | -4.000 | -88.820 | 3.000 | Ventral Turret Mount Base 72 |
| 0 | ventral | 5 | extra | 0.000 | -88.820 | 3.000 | Ventral Turret Mount Base 7C |
| 0 | ventral | 6 | yes | 4.000 | -88.820 | 3.000 | Ventral Turret Mount Base 74 |
| 0 | ventral | 7 | yes | -4.000 | -88.820 | -7.000 | Ventral Turret Mount Base 62 |
| 0 | ventral | 8 | extra | 0.000 | -88.820 | -7.000 | Ventral Turret Mount Base 6C |
| 0 | ventral | 9 | yes | 4.000 | -88.820 | -7.000 | Ventral Turret Mount Base 64 |
| 0 | ventral | 10 | yes | -4.000 | -88.820 | -17.000 | Ventral Turret Mount Base 52 |
| 0 | ventral | 11 | extra | 0.000 | -88.820 | -17.000 | Ventral Turret Mount Base 5C |
| 0 | ventral | 12 | yes | 4.000 | -88.820 | -17.000 | Ventral Turret Mount Base 54 |
| 0 | ventral | 13 | yes | -4.000 | -88.820 | -27.000 | Ventral Turret Mount Base 42 |
| 0 | ventral | 14 | extra | 0.000 | -88.820 | -27.000 | Ventral Turret Mount Base 4C |
| 0 | ventral | 15 | yes | 4.000 | -88.820 | -27.000 | Ventral Turret Mount Base 44 |
| 0 | ventral | 16 | yes | 108.500 | -22.966 | 249.000 | Ventral Turret Mount Base 76 |
| 0 | ventral | 17 | yes | 196.500 | -23.180 | 96.000 | Ventral Turret Mount Base 66 |
| 0 | ventral | 18 | yes | -196.500 | -23.180 | 96.000 | Ventral Turret Mount Base 56 |
| 0 | ventral | 19 | yes | 88.400 | -23.590 | -273.750 | Ventral Turret Mount Base 46 |


### SalamanderGunship

| Art | Side | Index | Ordinary? | X m | Y m | Z m | Object |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | dorsal | 0 | yes | 0.000 | 1.700 | 6.310 | Small Forward Dorsal Hull Weapon Base |
| 0 | ventral | 0 | yes | 0.000 | -1.916 | 4.443 | Small Forward Ventral Hull Weapon Base |
