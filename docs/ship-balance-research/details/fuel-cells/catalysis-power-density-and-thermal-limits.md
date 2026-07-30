# Hydrogen fuel cells: catalysis, specific power, and thermal limits

Last reviewed: 2026-07-30

## Scope

The game's localization describes alkaline hydrogen/oxygen fuel cells
recharged by solar arrays. This report separates four questions that are easy
to conflate:

1. the reversible chemical efficiency;
2. catalyst activity and electrode power density;
3. complete-stack and balance-of-plant specific power;
4. heat rejection at alkaline-fuel-cell temperature.

It also evaluates whether engineered biological catalysts could plausibly
outperform synthetic systems by mass or throughput.

## Settled planning values used here

These are changelog decisions, not yet implemented template values:

| Plant | Efficiency | Specific mass | Maximum output | Whole-plant specific power |
|---|---:|---:|---:|---:|
| Fuel Cell I | 58% | 5,600 t/GW | 0.2 GW | 0.179 kW/kg |
| Fuel Cell II | 60% | 3,600 t/GW | 0.8 GW | 0.278 kW/kg |
| Fuel Cell III | 62% | 960 t/GW | 1.5 GW | 1.042 kW/kg |

These masses supersede the earlier `2,800 / 1,800 / 480 t/GW` planning values,
which remain preserved in the changelog as decision history. Fuel Cell III is
now about half the DOE transportation target of `2 kW/kg` for the **stack
alone**. DOE explicitly excludes hydrogen storage, power
electronics, electric drive, and thermal/water/air management from that stack
target
([DOE transportation fuel-cell targets](https://www.energy.gov/cmei/fuels/doe-technical-targets-fuel-cell-systems-and-stacks-transportation-applications)).
The game description additionally requires solar array, electrolyzer, reactant
storage, and recharge plumbing. The revised `0.96 kg/kW` remains highly
optimistic for the described complete regenerative plant.

### Mass-accounting boundary

Fuel-cell `specificPower_tGW` includes the stack, regenerative hardware, and
the solar arrays named by the localization. Radiator panels are not included
in power-plant mass. They remain separate ship modules whose required capacity
is determined by plant efficiency and rejected heat.

## Reversible chemistry

For:

`H2 + 1/2 O2 → H2O(l)`

near room temperature:

- Gibbs free energy `ΔG ≈ 237.1 kJ/mol`;
- enthalpy `ΔH ≈ 285.8 kJ/mol`;
- reversible voltage `Erev ≈ 1.229 V`;
- thermoneutral voltage `Eth ≈ 1.481 V`.

The thermochemical values are consistent with the NIST/JANAF water data and
standard formation values
([NIST Chemistry WebBook](https://webbook.nist.gov/cgi/cbook.cgi?Name=water&cTG=on)).

The maximum HHV electrical efficiency at cell voltage `V` is:

`efficiencyHHV = V / 1.481`

and unavoidable cell heat relative to useful electricity is:

`heat / electricity = (1.481 - V) / V = 1 / efficiency - 1`

The reversible limit based on `ΔG/ΔH` is about 83%, not 100%. Even a reversible
cell rejects the entropy term as heat unless environmental heat is deliberately
included in a different thermodynamic convention.

| Planned efficiency | Implied cell voltage | Waste heat per MW electric |
|---:|---:|---:|
| 58% | 0.859 V | 0.724 MW |
| 60% | 0.889 V | 0.667 MW |
| 62% | 0.918 V | 0.613 MW |

These voltages are demanding but not thermodynamically impossible. The system
problem is sustaining high current density at that voltage while carrying all
reactant, water, cooling, and recharge hardware.

## Catalyst activity is not plant specific power

A catalyst reduces activation overpotential and allows a target current at a
higher voltage. It does not change `ΔG`, `ΔH`, or the reversible voltage.

For an electrode with current density `j`, cell voltage `V`, and stack areal
mass `mA`:

`electrical power per active area = j × V`

`heat per active area = j × (Eth - V)`

`stack specific power = j × V / mA`

Maximum useful throughput therefore requires all of the following at once:

- rapid hydrogen oxidation at the anode;
- much more difficult oxygen reduction at the cathode;
- proton or hydroxide transport through the electrolyte;
- gas diffusion without flooding or drying;
- electron collection with low ohmic loss;
- removal of product water and heat;
- catalyst durability at the chosen voltage and current.

DOE's component targets illustrate the distinction. A total precious-metal
loading near `0.125 mg/cm²` is only one part of the electrode; plates,
membranes, gas-diffusion media, compression, seals, manifolds, and coolant still
set stack mass
([DOE fuel-cell component targets](https://www.energy.gov/cmei/fuels/doe-technical-targets-polymer-electrolyte-membrane-fuel-cell-components)).

At the same DOE target's `0.3 A/cm² at 0.8 V`, each square centimeter produces
`0.24 W`. Dividing by `0.125 mg` gives a superficially spectacular
`1.92 MW/kg` of precious-metal catalyst. Yet the complete stack target is only
`2 kW/kg`: roughly one thousand times lower. This is exactly why
“catalytic conversion per catalyst mass” cannot be used as plant specific
power.

At lower catalyst loading, high-current performance also becomes limited by
local oxygen and proton transport rather than simply by a shortage of active
sites
([DOE-sponsored high-power low-platinum analysis](https://www.osti.gov/biblio/1504240)).

## Reactant throughput

At `0.9 V`, Faraday's law gives approximately:

| Per 1 MW electric | Flow |
|---|---:|
| Hydrogen consumed | 11.6 g/s |
| Oxygen consumed | 92.1 g/s |
| Water produced | 103.7 g/s |

At one gigawatt, those become `11.6 kg/s` hydrogen, `92.1 kg/s` oxygen, and
`103.7 kg/s` water. A regenerative plant must reverse these flows while
charging. The electrolyzer and solar array are therefore major machinery, not
flavor text.

## Low-temperature radiator penalty

NASA and DOE references place alkaline fuel cells around or below `100°C`
([NASA alkaline fuel-cell assessment](https://ntrs.nasa.gov/api/citations/20110023643/downloads/20110023643.pdf),
[DOE fuel-cell technology comparison](https://www.energy.gov/cmei/fuels/comparison-fuel-cell-technologies)).

At `90°C`, an ideal one-sided radiator with emissivity `0.9` radiates only:

`εσT⁴ ≈ 0.888 kW/m²`

Ignoring view-factor, plumbing, shadowing, degradation, and reserve margin:

| Planned efficiency | Ideal radiator area per MW electric |
|---:|---:|
| 58% | 816 m² |
| 60% | 751 m² |
| 62% | 691 m² |

At each plant's maximum output:

| Plant | Waste heat | Ideal radiator area | Mass at 5 kg/m² |
|---|---:|---:|---:|
| Fuel Cell I, 0.2 GW | 145 MW | 163,000 m² | 816 t |
| Fuel Cell II, 0.8 GW | 533 MW | 601,000 m² | 3,004 t |
| Fuel Cell III, 1.5 GW | 919 MW | 1,036,000 m² | 5,179 t |

Expressed per electric output, that separate ideal radiator is approximately
`4.08`, `3.76`, and `3.45 kg/kW` respectively. The corresponding revised
nominal plant masses are `5.6`, `3.6`, and `0.96 kg/kW`. The radiator is
slightly lighter than Fuel Cell I's nominal plant, approximately equal to Fuel
Cell II, and much heavier than Fuel Cell III. It remains excluded from
power-plant mass in all three cases.

The game's 800 K aluminum radiator cannot passively accept heat from a
roughly 363 K fuel cell. A heat pump could raise the rejection temperature,
but even a reversible heat pump from `363 K` to `800 K` requires at least
`1.20 J` of work per joule lifted. At 62% cell efficiency that minimum pumping
work is about `0.74 MW` per original `1 MW` electric output, before real
inefficiency. This is the strongest physical objection to gigawatt alkaline
fuel cells: not theoretical chemistry, but low-grade heat at enormous scale.

## What an ideal synthetic system might achieve

There is no catalyst-only upper bound in `kW/kg` for a complete fuel cell.
Making catalyst loading arbitrarily small eventually exposes another
bottleneck. A sober extrapolation should distinguish:

- **catalyst mass:** potentially tiny relative to the plant;
- **stack mass:** bounded by active area, separators, current collectors,
  compression, seals, and coolant passages;
- **balance of plant:** pumps, reactant conditioning, water separation,
  controls, power electronics, and heat exchangers;
- **regenerative hardware:** electrolyzer, tanks, solar collection, deployment
  structure, and recharge conditioning;
- **radiator:** accounted as a separate ship module and potentially heavier
  than the nominal plant at alkaline temperature.

Fuel Cell III's revised `1.042 kW/kg` whole-plant figure is less aggressive
than the earlier target, but it must include the solar array and regenerative
hardware described by the localization. The radiator is charged separately.

## Could engineered biology win?

Hydrogenases are impressive molecular catalysts. Genetic and protein
engineering can plausibly improve:

- oxygen tolerance;
- catalyst orientation and electron transfer;
- temperature and radiation stability;
- selectivity in mixed or contaminated gas;
- resistance to carbon monoxide or sulfur compounds;
- operation without scarce platinum-group metals.

Those are meaningful advantages. They could reduce purification or membrane
requirements in niche systems and make a biohybrid system superior on cost,
fuel tolerance, or low-concentration scavenging.

The evidence does not support superiority in maximum power per installed
kilogram:

- many active hydrogenases are oxygen-sensitive;
- random enzyme orientation leaves some active sites poorly coupled to the
  electrode;
- proteins require hydration and a stable chemical environment;
- protective polymer films add diffusion distance and inactive mass;
- enzymes denature and must be replaced;
- oxygen reduction, gas transport, water handling, and cooling remain even if
  hydrogen oxidation is excellent.

A recent review reports hydrogenase H2/O2 cells around `1.67–6.1 mW/cm²`,
with some systems reaching approximately `8 mW/cm²`; it also identifies oxygen
sensitivity, orientation, and stability as central limits
([hydrogenase electrocatalysis review](https://pmc.ncbi.nlm.nih.gov/articles/PMC10657181/)).
That is roughly two orders of magnitude below a PEM-class areal output near
`1 W/cm²`, before considering the supporting biological matrix.

### Impartial judgment

| Question | Judgment |
|---|---|
| Can engineered enzymes meaningfully improve a fuel cell? | Yes, especially selectivity, poison tolerance, low-temperature operation, and precious-metal avoidance. |
| Can an enzyme have exceptional catalytic turnover? | Yes; active-site turnover alone can be excellent. |
| Does that imply better whole-stack throughput per mass? | No. Transport, orientation, hydration, stability, cathode kinetics, and heat rejection dominate. |
| Most plausible high-power endpoint | Synthetic or biomimetic catalysts using lessons from hydrogenases, possibly with replaceable biohybrid electrodes for niche duties. |
| Likely winner for maximum spacecraft power density | A purely synthetic high-temperature or advanced membrane/electrode system, not a living or protein-heavy stack. |

This conclusion is contingent rather than absolute. A future engineered enzyme
could outperform today's catalysts at an active site, but it would still need
an electrode and thermal architecture that beats mature synthetic systems.

## Balance implications

- The planned `58/60/62%` efficiencies are generous but within the
  thermodynamic envelope for fuel-cell discharge.
- Corrected waste heat must use `delivered × (1 / efficiency - 1)`.
- Gigawatt alkaline plants demand implausibly large low-temperature radiators
  unless the game implicitly includes a heat pump or a different
  high-temperature chemistry.
- Fuel Cell III's settled specific power is now about half a modern
  stack-only target, but it must also cover solar collection and regenerative
  hardware that the stack target excludes.
- Biological integration can justify reliability, fuel-tolerance, or resource
  advantages; it should not provide a large generic specific-power bonus.

No additional fuel-cell value is settled by this report.
