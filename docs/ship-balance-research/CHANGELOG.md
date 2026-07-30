# Ship rebalance planning changelog

This is a decision log for the proposed ship rebalance. Entries here describe
the balance decisions as well as their implementation status.

## 2026-07-29

### Settled for the first low-tech slice

**Implementation status:** applied in Economic Equalization Overhaul 0.7.4.
The four deferred starting engines remain unchanged. Items under “Still under
research” remain unimplemented; in particular, this release does not adopt a
hull-material recipe or change the vanilla crew construction-resource package.

- **Apex, Meteor, Neutron, and Venture:** defer all changes. The four starting
  engines remain at their current values until the wider propulsion progression
  is reviewed.
- **Water Heat Sink and Heavy Water Heat Sink:** set module crew to **0** in the
  eventual rebalance. Monitoring and maintenance belong to the ship's shared
  engineering crew.
- **10-inch Cannon:** reduce module crew from **4 to 3** in the eventual
  rebalance. The deliberately conservative abstraction is one commander, one
  shooter, and one loader.
- **Point-defense guns:** use **0 dedicated module crew** for both projectile
  and laser point-defense mounts. In this narrow slice, apply and table that
  decision only for the **30mm Autocannon: 1 → 0 crew**. Reload, maintenance,
  and supervision remain ship-level burdens.
- **Solid Core Fission Reactors I–V:** reduce efficiency by five percentage
  points in the eventual rebalance:
  - I: **75% → 70%**
  - II: **77.5% → 72.5%**
  - III: **80% → 75%**
  - IV: **82.5% → 77.5%**
  - V: **85% → 80%**
- **Compact Solid Core Fission Reactors I–V** (template identifiers
  `SolidCoreFissionReactorVI–X`): apply the same five-percentage-point
  reduction:
  - Compact I: **77.5% → 72.5%**
  - Compact II: **80% → 75%**
  - Compact III: **82.5% → 77.5%**
  - Compact IV: **85% → 80%**
  - Compact V: **87.5% → 82.5%**
- **Fuel Cells I–III:** set efficiencies to **63%, 65%, and 67%**,
  respectively.
- **Fuel Cells I–III:** set specific mass to **2.8, 1.8, and 0.48 kg/kW**,
  respectively. In the template's `specificPower_tGW` field these are
  **2,800, 1,800, and 480 t/GW**.
- **Crew support mass:** reduce the global allowance from **4 t to 3 t per
  crew member**. This remains a bundled abstraction rather than consumables
  alone.
- **Open-cycle drive cooling:** an open-cycle drive must retain a nonzero
  radiator burden; the vanilla 100% drive-heat exemption is not acceptable.
  NERVA component data support a value below 5%. The current research-led
  implementation draft is **1% of the drive-associated heat that the corrected
  closed-cycle formula would otherwise send to radiators**.
- **Gunship:** adopt **55 m length × 15 m diameter**, giving a cylindrical
  planning volume of **9,719 m³**. Set the hull to **171 t**; three crew at
  the settled allowance produce **180 t empty mass**.
- **Escort:** adopt **62 m length × 15 m diameter**, giving a cylindrical
  planning volume of **10,956 m³**. Set the hull to **338 t**; four crew
  produce **350 t empty mass**.
- **Corvette:** adopt **65 m length × 17 m diameter**, **14,754 m³** planning
  volume, **5 crew**, and **385 t hull mass**, producing **400 t empty mass**.
- **Frigate:** adopt **100 m length × 18 m diameter**, **25,447 m³** planning
  volume, **8 crew**, and **576 t hull mass**, producing **600 t empty mass**.
- **Monitor:** adopt **100 m length × 17 m diameter**, **22,698 m³** planning
  volume, **7 crew**, and **679 t hull mass**, producing **700 t empty mass**.
- **Destroyer:** adopt **100 m length × 23 m diameter**, **41,548 m³** planning
  volume, **9 crew**, and **873 t hull mass**, producing **900 t empty mass**.
- **Hull volume data:** use the calculated cylindrical volumes for the planning
  `volume` values as well as the runtime geometry, while noting that the
  installed compiled class currently ignores the JSON `volume` key.

### Settled for the second slice

**Implementation status:** incorporated into Economic Equalization Overhaul
0.7.5; the later reactor refinement below supersedes these interim efficiency
values.

- **Molten reactors I–V:** reduce efficiency by five percentage points. The
  five installed TI 1.0.49 modules are split between the Molten Salt and Molten
  Core families, so record them by their exact template identities:
  - Molten Salt Fission Reactor I (`MoltenSaltFissionReactorI`):
    **92% → 87%**
  - Molten Salt Fission Reactor II (`MoltenSaltFissionReactorII`):
    **93% → 88%**
  - Molten Core Fission Reactor I (`MoltenCoreFissionReactorI`):
    **85% → 80%**
  - Molten Core Fission Reactor II (`MoltenCoreFissionReactorII`):
    **88% → 83%**
  - Molten Core Fission Reactor III (`MoltenCoreFissionReactorIII`):
    **90% → 85%**
- **40mm Autocannon** (`40mmAutocannon`): reduce module crew from **1 to 0**.
  Reloading, maintenance, and engagement supervision remain shared ship-level
  duties.
- **Point Defense Laser Turret** (`PointDefenseLaserTurret`): reduce module
  crew from **2 to 0**. This is the first laser point-defense mount and follows
  the previously settled rule that point-defense fire-control loops do not
  receive dedicated operators.

### Settled for the third slice

**Implementation status:** incorporated into Economic Equalization Overhaul
0.7.5, except for the explicitly deferred alien-fleet work. The weapon-crew
values are current; the later reactor refinement below supersedes these
interim power-plant efficiencies.

- **Fuel Cells I–III:** reduce the first-slice efficiencies by another five
  percentage points:
  - Fuel Cell I: **63% → 58%**
  - Fuel Cell II: **65% → 60%**
  - Fuel Cell III: **67% → 62%**
- **Solid Core Fission Reactors I–V:** reduce the first-slice efficiencies by
  another five percentage points:
  - I: **70% → 65%**
  - II: **72.5% → 67.5%**
  - III: **75% → 70%**
  - IV: **77.5% → 72.5%**
  - V: **80% → 75%**
- **Compact Solid Core Fission Reactors I–V** (template identifiers
  `SolidCoreFissionReactorVI–X`): reduce the first-slice efficiencies by
  another five percentage points:
  - Compact I: **72.5% → 67.5%**
  - Compact II: **75% → 70%**
  - Compact III: **77.5% → 72.5%**
  - Compact IV: **80% → 75%**
  - Compact V: **82.5% → 77.5%**
- **Molten reactors:** reduce the second-slice efficiencies by another five
  percentage points:
  - Molten Salt Fission Reactor I: **87% → 82%**
  - Molten Salt Fission Reactor II: **88% → 83%**
  - Molten Core Fission Reactor I: **80% → 75%**
  - Molten Core Fission Reactor II: **83% → 78%**
  - Molten Core Fission Reactor III: **85% → 80%**
- **6-inch Gun Battery** (`6-inchCannon`): reduce crew from **3 to 2**.
- **8-inch Gun Battery** (`8-inchCannon`): reduce crew from **4 to 2**.
- **Light Railgun Batteries Mk1–3** (`LightRailgunBatteryMk1–3`, one hull
  hardpoint): reduce crew from **3 to 2**.
- **Railgun Batteries Mk1–3** (`RailgunBatteryMk1–3`, two hull hardpoints):
  reduce crew from **4 to 2**.
- **Light Rail Cannons Mk1–3** (`LightRailCannonMk1–3`, the one-nose
  successors to the 10-inch Cannon): reduce crew from **4 to 3**.
- **Alien fleet rebalance:** defer numeric and loadout changes to a future
  slice. The working hypothesis is that early missile saturation exploits weak
  alien laser point defense, while later fleets are held back by old magnetic
  weapons and invalid or weak reactor/drive pairings. The future slice should
  test stronger laser point-defense behavior, proper Advanced/Gen3 magnetic
  weapons, adequate power plants, and revised module placement together rather
  than applying a uniform faction-wide stat buff.

Supporting analyses are archived by topic:

- [reactor thermodynamics and fuel inventory](details/reactors/thermodynamic-and-fuel-limits.md);
- [reactor structural scaling and output caps](details/reactors/structural-scaling-and-output-caps.md);
- [fuel-cell catalysis, specific power, and thermal limits](details/fuel-cells/catalysis-power-density-and-thermal-limits.md);
- [gun and railgun progression](details/weapons/gun-and-railgun-progression.md);
- [alien fleet and module audit](details/aliens/fleet-and-module-audit.md).

### Settled reactor mass-and-output refinement

**Implementation status:** applied in Economic Equalization Overhaul 0.7.5.
These values supersede the earlier reactor targets where they overlap.

- Reduce the efficiency of every touched solid, compact-solid, molten-salt, and
  molten-core reactor by another five percentage points:
  - Solid I–V: **60%, 62.5%, 65%, 67.5%, 70%**
  - Compact Solid I–V: **62.5%, 65%, 67.5%, 70%, 72.5%**
  - Molten Salt I–II: **77%, 78%**
  - Molten Core I–III: **70%, 73%, 75%**
- Double reactor specific mass:
  - Solid I–V: **80, 68, 56, 24, 16 t/GW**
  - Compact Solid I–V: **12, 10, 8, 6, 4 t/GW**
  - Molten Salt I–II: **4, 3.6 t/GW**
  - Molten Core I–III: **8, 7, 6 t/GW**
- Double Fuel Cell I–III specific mass as part of the same overall
  specific-mass pass: **5,600, 3,600, and 960 t/GW**, equivalent to
  **5.6, 3.6, and 0.96 kg/kW**. This clarification changes fuel-cell specific
  mass only; their output-cap and stored-energy questions remain open.
- **Fuel-cell mass-accounting boundary:** power-plant specific mass includes
  the fuel-cell and regenerative hardware plus the solar arrays described by
  the localization. Radiator panels are not included in power-plant mass.
  Efficiency determines rejected heat and therefore the separate ship
  radiator requirement.
- Set the current reactor maximum-output targets:
  - Solid I–V: **1, 3, 10, 30, 60 GW**
  - Compact Solid I–V: **0.75, 2, 4, 6, 10 GW**
  - Molten Salt I–II: **40, 400 GW**
  - Molten Core I–III: **4, 17, 200 GW**
- Molten Salt II is deliberately capped at **400 GW** because Pegasus Drive
  x6 requires approximately **395 GW**.
- With those output caps and specific masses, plant mass at maximum output is:
  - Solid I–V: **80, 204, 560, 720, 960 t**
  - Compact Solid I–V: **9, 20, 32, 36, 40 t**
  - Molten Salt I–II: **160, 1,440 t**
  - Molten Core I–III: **32, 119, 1,200 t**

### Still under research

- Fuel Cells I–III maximum output and whether to expose stored energy/recharge;
  their doubled specific masses are settled
- finer sequence adjustments to the current solid, compact-solid, molten-salt,
  and molten-core efficiency, output, and specific-mass targets
- reactor volume and crew requirements
- Diana, Nerva, and Kiwi drives
- Lithium-Ion Battery
- Water and Heavy Water Heat Sink capacity and mass
- 10-inch Cannon mass, magazine, projectile, velocity, and firing cycle
- 30mm Autocannon mass, magazine, projectile, velocity, and firing cycle; its
  zero-crew decision is already settled
- 40mm Autocannon mass, magazine, projectile, velocity, and firing cycle; its
  zero-crew decision is already settled
- Point Defense Laser Turret mass, power, efficiency, optics, range, and firing
  cycle; its zero-crew decision is already settled
- 6-inch and 8-inch Gun Battery performance and ammunition; their two-crew
  decisions are already settled
- Light Railgun Battery, Railgun Battery, and Light Rail Cannon performance,
  power, heat, and ammunition; their crew decisions are already settled
- alien laser point-defense effectiveness, magnetic-weapon tier allocation,
  reactor/drive pairing, armor, and module placement; all alien changes remain
  deferred pending controlled combat tests and more campaign experience
- whether module volume should eventually be enforced or remain an audit-only
  planning metric
- whether the Frigate's active radiator collider extending behind the visible
  ship is a live-combat hitbox defect that should receive a separate prefab fix
- correction of the power-plant waste-heat formula
- whether the draft 1% open-cycle residue should be raised for balance, plus
  how to represent shutdown decay heat
- whether hull visual scale, prefab hit colliders, and statistical
  length/width should remain coupled or receive separate mod-side controls
- hull construction-material composition and the exact 3 t/crew construction
  package; the current research candidate is a metal-forward, mass-conserving
  split, but it is not yet a settled decision
