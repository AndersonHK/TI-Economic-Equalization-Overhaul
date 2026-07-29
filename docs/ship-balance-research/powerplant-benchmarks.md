# Power-plant benchmarks

Last reviewed: 2026-07-29

## Unit normalization

The game field `specificPower_tGW` is named like a specific mass, not a specific power. Its conversion is:

`kg/kW = specificPower_tGW / 1,000`

Runtime inspection shows that the installed plant mass is:

`plant mass in tonnes = max(1, ship gross power requirement in GW × specificPower_tGW)`

`maxOutput_GW` is a compatibility and capacity ceiling, not the output used to
size every installation. Systems and weapon loads are divided by `efficiency`
to obtain their gross generation requirement. Vanilla then calculates heat as
`delivered × (1 - efficiency)`, which is too small. Input minus output is
`delivered × (1 / efficiency - 1)`. The mod's targeted runtime correction is
documented in [the low-tech rebalance slice](low-tech-rebalance-slice.md).

## Current game ranges

Non-alien power plants have the following ranges:

| Plant class | Output | Specific mass | Efficiency | Crew |
|---|---:|---:|---:|---:|
| Fuel cell | 0.2–1.5 GW | 0.12–2.8 kg/kW | 70–72% | 0 |
| Solid-core fission | 1.5–125 GW | 0.002–0.04 kg/kW | 75–87.5% | 3–6 |
| Molten-salt fission | 40–420 GW | 0.0018–0.002 kg/kW | 92–93% | 8 |
| Liquid-core fission | 8–420 GW | 0.003–0.004 kg/kW | 85–90% | 6 |
| Gas/vapor-core fission | 6.5–1,650 GW | 0.001–0.01 kg/kW | 90–96% | 5–8 |
| Magnetic/electrostatic fusion classes | 46–11,370 GW | 0.000005–0.005 kg/kW | 92–99% | 4–12 |
| Inertial fusion | 370–306,430 GW | 0.000002–0.004 kg/kW | 85–99.9% | 10–12 |
| Antimatter plasma core | 1,200–66,000 GW | 0.000004–0.0004 kg/kW | 99.75–99.8% | 12 |
| Antimatter beam core | 3,000,000 GW | 0.00000002 kg/kW | 99.9% | 12 |

The outputs are not necessarily impossible energy totals for a far-future civilization. The unsupported part is delivering them with the stated mass and efficiency.

## Fuel cells

The full game localization identifies these as alkaline hydrogen/oxygen fuel
cells **recharged by a solar array**. They are therefore regenerative
fuel-cell systems with photovoltaic primary power, not bare stacks or
unlimited consumable-reactant generators.

NASA describes a regenerative fuel-cell system as a fuel cell, electrolyzer,
reactant processing and storage, with external photovoltaic recharge
([NASA TechPort regenerative fuel cells](https://techport.nasa.gov/projects/116307)).
NASA's spacecraft solar-array survey reports actual mission hardware around
30 W/kg, a ROSA product around 100 W/kg, and an empirical maximum near 200 W/kg
([NASA Small Spacecraft State of the Art](https://www.nasa.gov/smallsat-institute/sst-soa/power-subsystems/)).

DOE's automotive direct-hydrogen stack target is `2,000 W/kg`, equivalent to `0.5 kg/kW`, and explicitly excludes hydrogen storage, power electronics, electric drive, and thermal/water/air-management ancillaries ([DOE transportation fuel-cell targets](https://www.energy.gov/cmei/fuels/doe-technical-targets-fuel-cell-systems-and-stacks-transportation-applications)).

DOE's solid-oxide program targets greater than `60%` electrical efficiency for stationary systems ([DOE Solid Oxide Fuel Cells](https://www.energy.gov/hgeo/solid-oxide-fuel-cells)). A NASA system study uses about `60%` stack efficiency for a 10 kW PEM fuel cell ([NASA fuel-cell thermal-management study](https://ntrs.nasa.gov/api/citations/20190001449/downloads/20190001449.pdf)).

### Game assessment

- Fuel Cell I at `2.8 kg/kW` implies 357 W/kg for the **entire**
  solar/regenerative system. That already exceeds the empirical solar-array
  maximum before fuel-cell hardware is counted.
- Fuel Cell II at `0.45 kg/kW` implies 2,222 W/kg, and Fuel Cell III at
  `0.12 kg/kW` implies 8,333 W/kg. Neither can be reconciled with the described
  solar array.
- `70–72%` is aggressive for the complete regenerative cycle. A NASA
  demonstration reported about 52% round-trip efficiency
  ([NASA NTRS regenerative fuel-cell demonstration](https://ntrs.nasa.gov/citations/20070010455)).
- The templates do not model stored-energy capacity or eclipse endurance, so
  they cannot currently express the most important operating limitation.

### Balance opinion

- Preserve zero operating crew.
- Include solar arrays, electrolyzer, reactant tanks, deployment structure,
  power conditioning, and thermal control in specific mass.
- Add stored-energy capacity and eclipse endurance if a later code change can
  support them.
- Treat `0.5 kg/kW` only as a modern **stack** anchor, never as the complete
  solar/regenerative system.
- Do not give early fuel cells hundreds of megawatts without correspondingly
  large collection area and heat-rejection penalties.

## Fission power

### Demonstrated and design anchors

KRUSTY was a ground-demonstrated `1 kWe` space fission system. NASA describes Kilopower as a `1–10 kWe` technology and reports successful nuclear operation in 2018 ([NASA KRUSTY reactor design](https://ntrs.nasa.gov/citations/20205009350)).

NASA testing targeted `2.5–6.5 W/kg`, equivalent to approximately `154–400 kg/kW`, at the kilowatt scale ([NASA KRUSTY electrically heated test](https://ntrs.nasa.gov/citations/20180001487)). A NASA review gives a representative space-rated 10 kWe Kilopower system mass of about `1,500 kg`, or `150 kg/kW` ([NASA, Frontiers of Space Power and Energy](https://ntrs.nasa.gov/api/citations/20210016143/downloads/NASA-TM-20210016143final.pdf)).

A 2022 NASA 40 kW surface-system study carried a predicted total mass near `8.9 tonnes` before an additional system margin, or roughly `223 kg/kW` ([NASA Fission Surface Power update](https://ntrs.nasa.gov/api/citations/20220012909/downloads/IAPG_Fission_Surface_Power_update.pdf)).

At much larger scale, an older NASA multimegawatt study estimated approximately `7–10 kg/kWe` for near-term SP-100 Rankine systems and called `5 kg/kWe` a reasonable advanced goal ([NASA multimegawatt nuclear power study](https://ntrs.nasa.gov/citations/19910067849)).

These values differ because power level, shielding, lifetime, radiator temperature, conversion cycle, redundancy, and mission requirements differ. The `5 kg/kW` figure is best treated as an aggressive large-system target, not current hardware.

### Game assessment

The game's solid-core fission range is `0.002–0.04 kg/kW`.

- Compared with the aggressive NASA `5 kg/kW` multimegawatt goal, the game is about 125 to 2,500 times lighter.
- The compact solid-core line reaches `0.002 kg/kW`, or about 2,500 times lighter than that aggressive goal.
- Compared with current kilowatt-class space systems near `150 kg/kW`, the difference reaches tens of thousands.
- Scaling to gigawatts should improve specific mass, but no demonstrated trend supports a drop from kilograms per kilowatt to grams or milligrams per kilowatt.

### Conversion efficiency

A recent NASA Brayton analysis found reference-case system efficiencies around `20–34%`, depending on configuration and radiator assumptions ([NASA Brayton performance and mass sensitivity](https://ntrs.nasa.gov/api/citations/20220013600/downloads/ASCEND_22_Brayton_Performance_Mass_Sensitivity.pdf?attachment=true)).

The game's solid-core plants use `75–87.5%`, while advanced fission classes use `85–96%`. Those figures are not credible for ordinary heat-engine conversion. They would require highly efficient direct conversion or a different definition of plant efficiency.

### Balance opinion

For a realism-oriented first pass:

- Current/near-term kilowatt fission: `100–250 kg/kW`, roughly `20–30%` electric efficiency.
- Aggressive future multimegawatt fission: `5–20 kg/kW`, roughly `25–45%`.
- Far-future direct conversion: do not go below roughly `1 kg/kW` without explicitly modeling a new conversion mechanism, shielding philosophy, and radiator temperature.
- Gas-core or liquid-core thermal performance should not automatically imply better electricity conversion; it may make materials and containment harder.

These are proposed balance envelopes, not forecasts.

## Fission reactor crew

KRUSTY demonstrated passive load following and passive temperature control under transients ([NASA KRUSTY results](https://ntrs.nasa.gov/citations/20180007389)). The reactor-design paper states that low-power systems could operate for years without control movement after startup and that higher-power systems might require only occasional control movement, potentially commanded remotely by a ground engineer ([KRUSTY Reactor Design](https://ntrs.nasa.gov/api/citations/20205009350/downloads/03-KRUSTY%20Reactor%20Design.pdf?attachment=true)).

This evidence argues against three to eight people continuously operating each future spacecraft reactor. It does not eliminate:

- inspection and repair,
- power-distribution maintenance,
- coolant and conversion-system maintenance,
- radiation monitoring,
- accident response and damage control,
- periodic overhaul,
- cyber and control-system supervision.

### Balance opinion

Make reactor crew a shared engineering department:

- zero dedicated real-time control operators per reactor during normal operation,
- a small ship-level watch team for the entire power system,
- maintenance staff that scales sublinearly with reactor count,
- extra crew or robotic mass for novel, low-reliability, or battle-damaged systems.

## Fusion power

There is no demonstrated fusion power plant supplying net electricity.

ITER is designed for `500 MW` of fusion heat from `50 MW` of plasma heating, but it will not convert that heat to electricity ([ITER, “What will ITER do?”](https://www.iter.org/fusion-energy/what-will-iter-do)). This `Q=10` target excludes much of the facility's auxiliary electrical demand.

NIF has produced target gain above four, but that ratio compares fusion yield with laser energy delivered to the target, not the electrical energy drawn by the laser facility ([LLNL NIF record](https://lasers.llnl.gov/news/target-breakthrough-enabled-fusion-record-nif)).

NASA describes direct conversion of charged fusion products as a research topic and notes that advanced fuels reduce neutron output at the cost of severe bremsstrahlung losses ([NASA advanced fusion power and thrust](https://www.nasa.gov/directorates/stmd/space-tech-research-grants/advanced-fusion-power-and-thrust-generation-with-centrifugally-confined-plasmas/)).

### Game assessment

- Output values from tens to hundreds of thousands of gigawatts have no prototype basis.
- Specific masses from `0.005` down to `0.000002 kg/kW` cannot be extrapolated from ITER or NIF.
- Efficiencies of `92–99.9%` are especially unsupported for neutron-rich D-T or D-D systems, whose energy must largely be captured as heat.
- Direct conversion might eventually improve efficiency for charged-particle fuels, but it does not justify applying near-perfect efficiency to every fusion confinement method.

### Balance opinion

- First-generation fusion electricity should be no lighter than aggressive mature fission and may be heavier.
- Use roughly `30–50%` delivered electrical efficiency for thermal neutron-rich fusion until a direct-conversion technology is specified.
- Reserve `50–70%` for late charged-particle direct conversion with explicit fuel and radiation tradeoffs.
- Treat efficiencies above `80%` as exceptional and technology-specific, not a generic fusion progression.
- Crew should be driven by maintainability and component life, not plasma control. Fast control loops will necessarily be automated.

## Antimatter power

No antimatter power reactor exists. Production and storage are laboratory-scale enabling problems. NASA design studies discuss containment, extraction, transport, and conversion as unresolved system elements ([NASA antimatter rocket concepts](https://ntrs.nasa.gov/search.jsp?R=19820013176)).

The game Antimatter Beam Core Reactor is rated for `3,000,000 GW` at
`0.00000002 kg/kW` and `99.9%` efficiency. A plant operating at that full
rating would weigh only 60 tonnes; smaller installations scale down to the
one-tonne runtime floor. This should be understood as setting technology, not a
scientific extrapolation.

### Balance opinion

- Avoid using current antimatter research to claim a plausible specific mass.
- Charge production infrastructure, trap mass, cryogenics, radiation shielding, conversion losses, and catastrophic containment risk.
- Introduce antimatter-catalyzed fission/fusion before pure antimatter power.

## Recommended first-pass plant envelopes

| Technology tier | Suggested specific mass | Suggested delivered efficiency | Crew treatment |
|---|---:|---:|---|
| Fuel cell stack | 0.5–3 kg/kW before reactants | 50–70% | Automated; maintenance only |
| Complete solar/regenerative fuel-cell system | 10–30+ kg/kW with present arrays; lower only with explicit advanced-array assumptions | 45–60% round trip | Shared engineering |
| Current/near-term space fission | 100–250 kg/kW | 20–30% | Remote/autonomous control, maintenance staff |
| Aggressive mature multimegawatt fission | 5–20 kg/kW | 25–45% | Shared engineering |
| First-generation fusion power | no defensible value; use ≥ mature fission as a placeholder | 30–50% for thermal conversion | High maintenance, automated plasma control |
| Charged-particle direct fusion | concept-only; 1–10 kg/kW as a balance placeholder | 50–70% conditional | Shared engineering |
| Antimatter | no research-grounded envelope | no research-grounded envelope | Infrastructure and containment dominate |

The central recommendation is to avoid a smooth sequence from `0.04 kg/kW` fission to `0.00000002 kg/kW` antimatter. Each major power technology should introduce a new set of system constraints rather than simply becoming lighter and more efficient.
