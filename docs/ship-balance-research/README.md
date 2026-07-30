# Ship balance research

Last reviewed: 2026-07-30

This dossier compares Terra Invicta's current ship-drive, power-plant, and weapon-crew values with demonstrated hardware, active prototypes, historical engineering programs, and explicitly speculative studies.

The game data used here comes from the installed templates:

- `TIDriveTemplate.json`
- `TIPowerPlantTemplate.json`
- `TILaserWeaponTemplate.json`
- `TIMagneticGunTemplate.json`
- `TIMissileTemplate.json`
- `TIParticleWeaponTemplate.json`
- `TIPlasmaWeaponTemplate.json`

The comparison is organized into:

- [Planning changelog](CHANGELOG.md)
- [Propulsion benchmarks](propulsion-benchmarks.md)
- [Power-plant benchmarks](powerplant-benchmarks.md)
- [Weapon automation and crew](weapon-automation-and-crew.md)
- [Low-tech rebalance: first planning slice](low-tech-rebalance-slice.md)
- [Fundamental limits and six-month crew consumables](fundamental-limits-and-crew-consumables.md)
- [Early power-plant localization and unlock audit](localization-and-unlock-audit.md)
- [Early human hull geometry, mass, crew, power, and volume audit](gunship-and-escort-hull-analysis.md)
- [Drive/reactor pairing, open-cycle cooling, and hull geometry](drive-reactor-pairing-and-hull-geometry.md)
- [Hull resource-cost accounting and metal-use options](hull-resource-cost-accounting.md)
- [Detailed research archive](details/README.md)

## Documentation structure

This file is the dense, composed overview. General topic reports sit beside
it; narrower and more comprehensive analyses live under `details/`, where
topic indexes summarize and reconcile their different perspectives. Earlier
analyses are retained and cited when a later model adds another facet rather
than being silently replaced.

## Reading the confidence labels

| Label | Meaning |
|---|---|
| **Operational** | Flight-proven or routinely fielded hardware. |
| **Ground demonstrated** | Relevant hardware has operated on the ground, but not necessarily as a complete flight system. |
| **Engineering target** | A funded program has a defined design target, but the complete capability has not been demonstrated. |
| **Concept study** | A serious technical study exists, but major enabling hardware or physics validation is missing. |
| **Speculative** | No integrated experimental basis exists from which mass, reliability, or production rate can be extrapolated responsibly. |

The label applies to the technology, not to whether an individual numerical estimate was calculated correctly.

## Headline findings

1. **The game often gets the energy relationship right.** For powered drives, its thrust, exhaust velocity, efficiency, and requested power usually obey the expected jet-power relationship. A high thrust value is therefore not automatically impossible if the ship is assumed to carry a sufficiently large power source.

2. **The power source is usually the optimistic step.** The game's fission plants span roughly `0.001–0.04 kg/kW` for non-alien designs, compared with an aggressive NASA multimegawatt study goal of about `5 kg/kW` and much heavier current kilowatt-class systems. That is an orders-of-magnitude gap, not a modest extrapolation.

3. **Several advanced drive values are traceable to real concept studies.** Advanced NERVA closely matches the historical NERVA design point. Orion and the fission-fragment drive also sit near published concept-study figures. This makes them defensible as technology concepts, but not as demonstrated mass, lifetime, or reliability.

4. **The least defensible chemical entries are easy to identify.** NASA summarizes chemical exhaust velocity as below about `4.4 km/s`. The game's Venture drive sits at that boundary, while Nova at `5.3 km/s` and especially Super Kronos at `21.6 km/s` need a non-chemical energy source or a deliberately fictional material/propellant assumption.

5. **Per-mount weapon operators are unlikely.** Phalanx already performs search through kill assessment autonomously, while Aegis automates the engagement computation with human weapon selection. Modern torpedoes guide themselves after launch, although loading, tube preparation, maintenance, and command authorization still require people.

6. **Automation does not remove the crew burden; it moves it.** A credible future model should distinguish combat-system supervision, maintenance, damage control, and ordnance handling instead of assigning several continuous operators to every installed weapon or reactor.

7. **Reactor mass and reactor size require different models.** Ideal fission
   fuel establishes a real `t/GW-year` lower bound, but it does not set the
   largest cooled, controlled unit. The preferred planning model combines a
   linear mass term, endurance fuel, fixed mass per repeated
   reactor/loop/converter train, and a technology-specific output cap. See the
   [reactor synthesis](details/reactors/README.md).

8. **Gas Core Fission Reactor VI already reaches the earlier extreme mass
   floor.** Its template is `1 t/GW`, `96%` efficient, and capped at
   `1,650 GWe`; a one-year ideal fuel load at full rating would itself be about
   `660 t`. Specific mass alone therefore cannot evaluate it.

9. **Gigawatt alkaline fuel cells are limited by low-grade heat.** The planned
   `58/60/62%` discharge efficiencies are possible, but a roughly `90°C`
   stack needs about `691–816 m²` of ideal radiator per electric megawatt.
   Radiator mass is accounted separately, while plant specific mass includes
   the solar arrays described by the localization. After the later mass
   doubling, Fuel Cell III supplies `1.04 kW/kg` before that separate radiator
   burden. See the
   [fuel-cell synthesis](details/fuel-cells/README.md).

10. **The alien catalog is stronger than its predefined fleet makes it look.**
    Reactor/drive mismatches, old magnetic weapons, uneven point defense,
    light armor, and non-use of stronger modules justify a future integrated
    design pass. Raw buffs remain deferred pending controlled tests. See the
    [alien synthesis](details/aliens/README.md).

## Working balance recommendations

These are design opinions derived from the evidence, not measured future facts.

- Treat exhaust velocity as the easiest advanced-propulsion number to justify, and total system mass, radiator mass, lifetime, and power generation as the hard constraints.
- Do not extrapolate a laboratory thruster by scaling only its thrust. Scale power processing, cooling, gimbals, cabling, propellant feed, redundancy, and lifetime hardware as well.
- Give historical nuclear-thermal designs strong thrust and about twice chemical exhaust velocity, but charge meaningful engine/reactor/shielding mass.
- Treat gas-core fission, nuclear salt water, fusion, and antimatter as separate uncertainty tiers rather than a smooth continuation of current fission technology.
- Consider changing weapon `crew` from literal mount operators into a maintenance/readiness burden, or replace it with ship-level combat-system and ordnance departments.
- Consider changing reactor `crew` from per-plant operators into a shared engineering complement that scales sublinearly with reactor count and rises with plant novelty, damage-control requirements, and maintainability.

## Important caveats

- A game's `mass` field may represent only the module and not shielding, radiators, propellant tanks, cabling, structural reinforcement, or maintenance access.
- `specificPower_tGW` converts numerically to kilograms per kilowatt by dividing by 1,000.
- Power-plant mass scales with the ship's required gross production rather than
  automatically using the plant's maximum-output rating.
- The drive-template fields imply a hardware-mass term of approximately:

  `flatMass_tons + specificPower_kgMW × requiredPower_GW`

  This follows from the units, but the game code should be checked before treating it as the exact runtime formula.
- Runtime inspection confirms that systems and weapon requirements are divided
  by plant efficiency to obtain gross generation. Vanilla then understates
  rejected heat by using `delivered × (1 - efficiency)`; the corrected
  input-minus-output expression is `delivered × (1 / efficiency - 1)`.
- Scaling from kilowatts to gigawatts can improve specific mass, but it does not justify assuming that shielding, heat rejection, power conversion, and distribution approach zero mass.
