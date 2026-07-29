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

### Still under research

- Fuel Cells I–III maximum output and whether to expose stored energy/recharge
- Solid Core Fission Reactors I–V maximum output, specific mass, volume, and crew
- Compact Solid Core Fission Reactors I–V maximum output, specific mass,
  volume, and crew
- Diana, Nerva, and Kiwi drives
- Lithium-Ion Battery
- Water and Heavy Water Heat Sink capacity and mass
- 10-inch Cannon mass, magazine, projectile, velocity, and firing cycle
- 30mm Autocannon mass, magazine, projectile, velocity, and firing cycle; its
  zero-crew decision is already settled
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
