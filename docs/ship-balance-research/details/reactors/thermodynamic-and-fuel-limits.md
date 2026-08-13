# Theoretical reactor limits: thermodynamics and fuel inventory

Last reviewed: 2026-08-13

## Scope within the research archive

This report preserves the thermodynamic and fuel-inventory perspective,
including the deliberately extreme `1 t/GWe` example. It is not a complete
reactor-size model. For fuel conduction, coolant interfaces, thermal stress,
neutron flux, converter trains, and maximum output per integrated unit, read
the sibling
[structural scaling and output-cap analysis](structural-scaling-and-output-caps.md).
The [reactor topic index](README.md) summarizes how the two facets fit
together.

## Direct answers

### 1. Extreme core-and-converter mass

There is no universal nonzero `tonnes/GW` limit for an instantaneous reactor
if lifetime, shielding, heat transfer, structural strength, and conversion
rate are all unconstrained. Thermodynamics limits efficiency; it does not
assign machinery a mass. A reversible Carnot converter also approaches its
limit only as power density approaches zero.

Once endurance is specified, fission fuel consumption creates a real lower
bound:

- ideal complete fission of U-235 releases about `8.21 × 10^13 J/kg`;
- `1 GW thermal-year` therefore consumes at least **0.384 t of U-235**;
- `1 GW electric-year` consumes `0.384 / efficiency` tonnes before allowing
  for incomplete burnup, breeding losses, reserve, structure, or machinery.

For a one-year ship reactor at 70% electric efficiency, the fuel-only floor is
**0.55 t/GWe**. A useful rebalance rule is:

- **1 t/GWe is an extreme science-fiction floor** for the core, converter, and
  at most about one year of ideally burned fuel, with shielding, radiators,
  coolant loops, distribution, redundancy, and maintenance access excluded;
- **3 t/GWe is already consumed by ideal fuel alone over roughly 5.5 years** at
  70% efficiency;
- there is no defensible route to arbitrarily small `t/GW` for a long-lived
  fission system unless fuel is accounted for elsewhere and replenished.

The `1 t/GWe` figure is a modeling boundary, not a forecast. It leaves only
about `0.45 t/GWe` for every non-fuel component in a one-year, 70%-efficient
plant.

### 2. Temperature advantage

With the game's Aluminum Fin radiator at `800 K`:

- `2,500 °C` solid core: **71.15% Carnot**
- localized `5,000 °C` molten core: **84.83% Carnot**
- localized `8,000 °C` vapor core: **90.33% Carnot**
- game-localized gas-core maximum of `25,000 °C` (`25,273 K`):
  **96.83% Carnot**

The installed component localization therefore defines a deliberately broad
`2,500 → 5,000 → 8,000 → 25,000 °C` progression. Its efficiency gains still
diminish with temperature: molten gains 13.68 percentage points over solid,
vapor gains another 5.50, and the gas-core maximum gains another 6.50.

## Why no mass-only theorem exists

A minimal reactor model can be written:

`M / Pe = Mfixed / Pe + mfuel / Pe + Mheat-transfer / Pe + Mconverter / Pe`

where:

- `Mfixed` is critical inventory, control, reflector, and startup hardware;
- `mfuel` depends on power, efficiency, endurance, and burnup;
- heat-transfer mass depends on allowable flux and surface areal density;
- converter mass depends on cycle, stress, heat-exchanger conductance, and
  acceptable efficiency at finite power.

The first term improves with scale. The fuel term becomes a constant
`t/GW-year`. The last two approach technology-dependent asymptotes rather than
vanishing. If their material limits are omitted, the equation has no finite
mass floor.

This is why “ignore everything except the reactor” must still define:

1. electric or thermal GW;
2. full-power endurance;
3. whether replacement fuel is part of the module;
4. heat-transfer mechanism and maximum flux;
5. whether the converter must deliver finite power;
6. which pressure vessel, reflector, and control masses remain in scope.

## Fuel-consumption lower bound

The IAEA uses approximately `200 MeV` released per fission. Combining that with
the U-235 atomic mass gives approximately `8.21 × 10^13 J/kg`.

| Conversion assumption | Efficiency | Ideal U-235 consumed per GWe-year |
|---|---:|---:|
| 2,500 °C solid-core Carnot ceiling | 71.15% | 0.540 t |
| 2,500 °C finite-rate reference | 46.29% | 0.830 t |
| 5,000 °C molten-core Carnot ceiling | 84.83% | 0.453 t |
| 5,000 °C finite-rate reference | 61.05% | 0.630 t |
| 8,000 °C vapor-core Carnot ceiling | 90.33% | 0.426 t |
| 8,000 °C finite-rate reference | 68.90% | 0.558 t |
| 25,000 °C gas-core Carnot ceiling | 96.83% | 0.397 t |
| 25,000 °C finite-rate reference | 82.21% | 0.468 t |

The “finite-rate reference” is the Curzon–Ahlborn result
`1 - sqrt(Tcold / Thot)`. It is useful for showing how finite heat transfer
pulls performance away from Carnot, but it is not a universal upper bound for
all engine architectures.

These masses assume every U-235 atom is fissioned. Actual fuel inventory is
higher because real systems do not reach 100% burnup and require excess
reactivity, reserve, and non-fissile material.

### Endurance sensitivity at 70%

| Full-power endurance | Ideal fuel floor per GWe |
|---|---:|
| 180 days | 0.271 t |
| 1 year | 0.549 t |
| 5 years | 2.745 t |
| 10 years | 5.490 t |

The game does not consume reactor fuel over time. If reactor construction mass
is meant to include lifetime fuel, desired service life must become an
explicit balancing assumption.

## Heat-flux model

The Stefan–Boltzmann law provides a clean idealized surface-transfer bound:

`q = σ × (Thot⁴ - Tcold⁴)`

This is not a complete reactor design. It simply asks how much perfectly
coupled radiant area is needed to move heat from the core toward a converter.

| Hot side | Net ideal flux to 800 K sink | Ideal area per GW thermal |
|---|---:|---:|
| 2,773 K (2,500 °C) | 3.33 MW/m² | 300 m² |
| 5,273 K (5,000 °C localized molten core) | 43.82 MW/m² | 22.8 m² |
| 4,404 K uranium boiling point | 21.31 MW/m² | 46.9 m² |
| 8,273 K (8,000 °C localized vapor core) | 265.62 MW/m² | 3.76 m² |
| 10,000 K gas core | 567 MW/m² | 1.76 m² |
| 15,000 K gas core | 2,871 MW/m² | 0.35 m² |
| 25,273 K (game-localized 25,000 °C maximum) | 23,134 MW/m² | 0.043 m² |

Raising the localized solid-core reference from 2,500 °C to the molten-core
5,000 °C increases ideal radiative flux by roughly thirteen times while
raising Carnot efficiency by 13.68 points. The localized vapor and gas values
increase ideal radiant flux much more dramatically than efficiency.

NASA gas-core studies discuss uranium plasmas around `10,000–20,000 K`; one
test-reactor analysis used `15,000 K`. Those are concept-study temperatures,
not demonstrated power-plant operating points.

## Carnot and finite-rate comparison

| Hot-side case | Temperature | Carnot to 800 K | Curzon–Ahlborn reference | Gain over 2,500 °C Carnot |
|---|---:|---:|---:|---:|
| Solid core, localized | 2,773 K | **71.15%** | 46.29% | — |
| Molten core, localized | 5,273 K | **84.83%** | 61.05% | +13.68 pp |
| Vapor core, localized | 8,273 K | **90.33%** | 68.90% | +19.18 pp |
| Gas core, game-localized maximum | 25,273 K | **96.83%** | 82.21% | +25.68 pp |
| Uranium one-atmosphere boiling boundary | 4,404 K | **81.83%** | 57.38% | +10.68 pp |
| NASA gas-core study point | 10,000 K | **92.00%** | 71.72% | +20.85 pp |
| NASA gas-core test-reactor point | 15,000 K | **94.67%** | 76.91% | +23.52 pp |
| NASA gas-core comparison point | 20,000 K | **96.00%** | 80.00% | +24.85 pp |

The `4,404 K` uranium boiling point is a phase-boundary reference at one
atmosphere, not the game's vapor-core operating point. Pressure changes the
phase boundary, and the localization explicitly supplies the appropriate
`8,000 °C` balance anchor. Temperature gains have diminishing returns in Carnot
efficiency even as material, radiation, and confinement problems become much
harder.

## Relation to the game's fission progression

### Molten-core and molten-salt decisions

The implemented molten-core values are:

| Plant | Efficiency | 5,000 °C Carnot margin |
|---|---:|---:|
| Molten Core I | 67.5% | 17.33 pp below Carnot |
| Molten Core II | 70.5% | 14.33 pp below Carnot |
| Molten Core III | 72.5% | 12.33 pp below Carnot |

All three are below the localized hot-side Carnot ceiling but above the 61.05%
Curzon–Ahlborn reference. That is an optimistic but internally coherent range
for highly developed turbines and heat conversion.

Molten Salt I–II are implemented at `72.5–75%`, but their component
localization gives no operating temperature. They therefore cannot be assigned
a meaningful Carnot or Curzon–Ahlborn comparison without inventing a hot-side
assumption. “Molten salt” and “molten core” should not inherit the same
temperature merely because both contain the word molten.

### Vapor core

Vapor Core I–III are implemented at `87–89%` against the localization's
`8,000 °C` anchor. They sit 3.33, 2.33, and 1.33 percentage points below the
90.33% Carnot ceiling, respectively, while remaining well above the 68.90%
Curzon–Ahlborn maximum-power reference. This is deliberately optimistic but no
longer thermodynamically impossible at the stated temperature.

### Gas core

The component localization for Gas Core Fission Reactors I–III says a rotating
vortex of gaseous fissiles can operate at up to `25,000 °C` (`25,273 K`). The
later terawatt variants cite stronger materials but do not restate a
temperature. Separately, the Gas Core Fission Systems technology localization
describes gaseous uranium hexafluoride held by superconducting magnets and
solid-state collectors capturing electromagnetic radiation and charged
particles. This is not an ordinary turbine cycle.

That fiction can justify a different efficiency family, but it does not make
efficiency arbitrary:

- the localized `25,273 K` maximum has a 96.83% Carnot ceiling and an 82.21%
  Curzon–Ahlborn reference against an 800 K sink;
- 96% thermal efficiency requires approximately `20,000 K`;
- direct photon or charged-particle conversion needs its own conversion-loss
  model rather than silently inheriting Carnot or assuming 100%;
- neutron and gamma energy that thermalizes still creates a radiator burden.

The implemented `87–94%` gas-core range is below Carnot if the localization's maximum
temperature is used as the hot reservoir, but every tier is above the
Curzon–Ahlborn maximum-power reference. It is therefore thermodynamically
possible only as an exceptionally optimistic system, especially because the
localized architecture invokes direct photon and charged-particle conversion
rather than a simple heat engine. Because `25,000 °C` is an “up to” value, it
should not be read as a guarantee that every tier operates at that temperature.

## Does physics impose a few-GW maximum?

No. A two-GW ceiling is not a fundamental nuclear-physics limit.

Critical mass is mainly a fixed cost, so increasing power can initially improve
specific mass. NASA gas-core concept work explicitly discussed multigigawatt
thermal reactors with tens of kilograms of active fuel. The reasons practical
reactors cluster rather than grow without limit are system constraints:

- heat-transfer area and coolant pumping;
- pressure-vessel stress;
- power-conversion train size;
- neutron and gamma shielding;
- control authority and transient energy;
- decay heat after shutdown;
- maintenance, refueling, and component lifetime;
- fault containment and redundancy.

Those constraints can make several smaller reactor-converter loops preferable
to one enormous loop, but the optimum is a design result rather than a law of
physics.

### Recommended modeling interpretation

- Use output caps to represent an architecture's largest controllable,
  maintainable unit, not a claim that fission stops above that number.
- Permit ships to cluster multiple units, paying duplicate control, vessel,
  shielding, and conversion mass.
- Give early solid reactors low single-digit-GW unit caps if the desired
  fiction is conservative.
- Allow later liquid and gas systems larger units only when their heat
  transfer, containment, and conversion technology advances.
- Do not use higher maximum output as permission for lower fuel mass, higher
  efficiency, and lower specific mass simultaneously.

The game's multi-terawatt late reactors are not ruled out by total fission
energy alone. Their combination of output, near-perfect efficiency, tiny mass,
and no lifetime fuel consumption is the unsupported package.

## Sources

- Terra Invicta 1.0.51 installed English `TIPowerPlantTemplate.en` and
  `TITechTemplate.en` localizations
- [IAEA, approximately 200 MeV released per fission](https://www-pub.iaea.org/MTCD/Publications/PDF/TE-1821_web.pdf)
- [IAEA thermophysical properties of uranium](https://www-pub.iaea.org/MTCD/Publications/PDF/IAEA-THPH_web.pdf)
- [NIST CODATA 2022 fundamental constants](https://physics.nist.gov/cuu/pdf/JPCRD2022CODATA.pdf)
- [Curzon and Ahlborn, efficiency at maximum power](https://doi.org/10.1119/1.10023)
- [Apertet et al., why Curzon–Ahlborn is not universal](https://doi.org/10.1103/PhysRevE.96.022119)
- [NASA gas-core reactor optical properties at 10,000–50,000 K](https://ntrs.nasa.gov/api/citations/19700022608/downloads/19700022608.pdf)
- [NASA gas-core test reactor at 15,000 K](https://ntrs.nasa.gov/citations/19730015935)
- [NASA review of multigigawatt gas-core concepts](https://ntrs.nasa.gov/api/citations/19930009659/downloads/19930009659.pdf)
