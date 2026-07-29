# Propulsion benchmarks

Last reviewed: 2026-07-28

## What is being compared

The current drive template contains 541 rows, largely because each underlying drive is repeated with different thruster counts. Restricting the comparison to `thrusters = 1` leaves 96 base entries across nine game classifications.

The two most useful first-order relationships are:

`Isp = exhaust velocity / g0`

`jet power = thrust × exhaust velocity / 2`

For an electrically powered drive:

`input power = jet power / efficiency`

NASA's explanation of specific impulse supplies the first relationship and the physical meaning of exhaust velocity ([NASA Glenn, “Specific Impulse”](https://www1.grc.nasa.gov/beginners-guide-to-aeronautics/specific-impulse/)).

The game generally obeys these equations. Across the 87 powered base drives, the median absolute discrepancy between listed power and calculated input power is about `0.002%`; the largest is the low-power Resistojet at about `10.3%`, explained by its listed power being rounded to only `0.002 GW`. The disagreement with current engineering is usually in power-system mass, thruster-system mass, cooling, lifetime, or maturity.

## Game-wide base-drive ranges

The mass column below is an inferred installed drive-hardware term:

`flatMass_tons + specificPower_kgMW × requiredPower_GW`

It excludes the separately selected power plant and radiator.

| Game classification | Base families | Thrust range | Exhaust-velocity range | Requested-power range | Inferred drive mass | Evidence maturity |
|---|---:|---:|---:|---:|---:|---|
| Chemical | 7 | 2.50–20.38 MN | 2.6–21.6 km/s | none | 10–66 t | Operational at lower exhaust velocities |
| Electrothermal | 5 | 1–12 kN | 2.9–19.62 km/s | 2–73 MW | 0 t | Operational class; game scale is extrapolated |
| Electrostatic | 4 | 3.3–10 kN | 19.62–210 km/s | 61 MW–1.105 GW | 0 t | Operational class; game thrust is large-array extrapolation |
| Electromagnetic | 8 | 1–28 kN | 9.81–425 km/s | 58 MW–3.975 GW | 0–3.62 t | Mixed: tested plasma drives to concepts |
| Fission thermal | 30 | 4.65 kN–11 MN | 8.09–3,750 km/s | 207 MW–186.7 GW | 0 t | Ground-demonstrated NTP through concept-only FFRE |
| Fission pulse | 7 | 24 kN–24 MN | 42.1–157 km/s | 1.922 GW–1.44 TW | 0.734–367 t | Concept study |
| Nuclear salt water | 2 | 12.9–13 MN | 66–1,700 km/s | direct-power field is zero | 0 t | Concept study |
| Fusion thermal | 29 | 19.2 kN–9.76 MN | 270–10,256 km/s | 7.534 GW–51.07 TW | 0–10 t | Concept study to speculative |
| Antimatter | 4 | 1.1–10 MN | 360–14,720 km/s | 198.4 GW–73.75 TW | 0 t | Speculative integrated system |

Zero drive mass should not be read as a physically massless engine. It indicates that the current template has no flat or specific-power mass charge for that drive.

## Chemical propulsion

### Demonstrated anchor

NASA lists the current RS-25 at about `2.28 MN` vacuum thrust and `3,515 kg` engine mass ([NASA RS-25 fact sheet](https://www.nasa.gov/wp-content/uploads/2025/04/sls-4963-sls-rs-25-engine-fact-sheet-508.pdf?emrc=bb1960)). NASA also summarizes chemical exhaust velocity as below approximately `4.4 km/s`, or an `Isp` below roughly 450 seconds ([NASA technical paper](https://ntrs.nasa.gov/citations/20220006853)).

### Game comparison

| Game drive | Thrust | Exhaust velocity | Flat mass | Assessment |
|---|---:|---:|---:|---|
| Apex Solid Rocket x1 | 14.82 MN | 2.60 km/s | 66 t | Plausible performance region for a very large solid motor; dry/loaded-mass definitions matter. |
| Meteor Liquid Rocket x1 | 15.48 MN | 2.98 km/s | 17 t | Very high thrust density, but not outside the broad chemical regime. |
| Neutron Liquid Rocket x1 | 20.38 MN | 3.10 km/s | 13 t | About 2.4 times the RS-25 thrust per engine tonne; aggressive. |
| Venture Liquid Rocket x1 | 9.28 MN | 4.44 km/s | 14 t | Near the demonstrated high-performance chemical ceiling. |
| Nova Liquid Rocket x1 | 7.85 MN | 5.30 km/s | 15 t | Beyond the ordinary chemical ceiling; needs an extraordinary propellant cycle or reclassification. |
| Super Kronos Liquid Rocket x1 | 2.50 MN | 21.60 km/s | 10 t | Not credible as a chemical rocket. This exhaust velocity belongs in externally heated, nuclear, or electric propulsion. |

### Balance opinion

- Keep roughly `2.6–4.5 km/s` as the modern-to-advanced chemical band.
- Values around `5 km/s` should be rare, fragile, or depend on unusually low-molecular-weight propellants and extreme chamber conditions.
- A `21.6 km/s` “chemical” engine should be renamed/reclassified or reduced by about a factor of five.
- High chemical thrust is believable, but the ship must also carry very large propellant flow, feed systems, tanks, and short burn endurance.

## Electrothermal propulsion

Resistojets and arcjets are established technologies. They add external heat to a propellant, trading electrical power for better exhaust velocity while remaining much lower in specific impulse than advanced ion or plasma drives. NASA's current state-of-the-art survey lists real electrothermal devices alongside Hall and ion systems and emphasizes that their higher thrust comes with lower specific impulse ([NASA Small Spacecraft Propulsion survey](https://www.nasa.gov/smallsat-institute/sst-soa/in-space_propulsion/)).

The game's `1–12 kN` devices at `2–73 MW` are not forbidden by first principles. They are industrial-scale arrays far above flight hardware. Their current zero mass is the implausible part.

### Balance opinion

- Electrothermal drives are a good low-tech bridge between chemical and high-specific-impulse electric propulsion.
- Give them substantial power-processing, heating-chamber, feed-system, and radiator mass.
- Their advantage should be propellant flexibility and moderate thrust, not zero-mass hardware.

## Hall and ion propulsion

### Demonstrated anchors

NASA's 12.5 kW HERMeS/AEPS Hall thruster demonstrated more than `0.6 N` thrust at about `3,000 s Isp` in its technology program ([NASA HERMeS presentation](https://ntrs.nasa.gov/api/citations/20180000816/downloads/20180000816.pdf)). The flight-oriented AEPS system operates up to about 13.3 kW and over 2,600 seconds specific impulse ([NASA AEPS development paper](https://ntrs.nasa.gov/citations/20190032202)).

NASA's NEXT ion thruster demonstrated:

- `0.54–6.9 kW` input,
- up to `236 mN`,
- about `4,190 s Isp`,
- about `71%` peak efficiency,
- `13.5 kg` thruster mass with harness,
- more than 51,000 hours of operation.

See the [NASA NEXT characteristics](https://discovery.larc.nasa.gov/pdf_files/01-NEXT-SBenson-2.pdf) and [NASA long-duration test release](https://www.nasa.gov/news-release/nasa-thruster-achieves-world-record-5-years-of-operation/).

### Game Hall drive

The game Hall Drive x1 has:

- `3,300 N` thrust,
- `19.62 km/s` exhaust velocity,
- `53%` efficiency,
- `61 MW` requested power,
- zero drive-specific mass.

At the same order of specific impulse, `3,300 N` is roughly 5,500 HERMeS-class thrusters by thrust. Their combined power would be tens of megawatts, so the game's 61 MW requirement is physically coherent. The problem is that thousands of channels, cathodes, gimbals, power processors, propellant distributors, and thermal interfaces cannot plausibly have zero mass or zero maintenance burden.

### Game ion drive

The game Ion Drive x1 has:

- `3,300 N` thrust,
- `78.4 km/s` exhaust velocity,
- `95%` efficiency,
- `136 MW` requested power,
- zero drive-specific mass.

Matching only the thrust with NEXT-class heads would require about 14,000 thrusters and at least about 189 tonnes of thruster heads before power processors, harnesses, gimbals, support structure, xenon feed, and redundancy. The game asks for nearly twice NEXT's exhaust velocity and a higher efficiency, so a mature integrated installation should be heavier and more power-intensive than a simple NEXT array.

### Balance opinion

- The game's Hall and ion thrust values can be retained if they visibly represent immense arrays powered at tens to thousands of megawatts.
- Add thruster-specific mass. A zero-mass electric drive hides one of the dominant scaling costs.
- Keep lifetime and erosion as differentiators. NEXT's 51,000-hour test is impressive but not equivalent to indefinite combat maneuvering at maximum power.
- Penalize very high exhaust velocity with lower thrust at fixed power. This is not merely a balancing convention; it follows directly from the jet-power equation.

## VASIMR and electromagnetic plasma drives

The ground-tested VX-200 VASIMR operated at:

- `200 kW`,
- approximately `5.8 N`,
- approximately `4,900 s Isp`, or about `48 km/s`,
- approximately `72%` efficiency.

See the [NASA technical summary](https://ntrs.nasa.gov/api/citations/20130001782/downloads/20130001782.pdf) and [VX-200 test paper](https://www.adastrarocket.com/technical-papers-archives/Jared_IEPC11-154.pdf). NASA TechPort describes VASIMR as approaching TRL 5 and gives a future flight-unit expectation of about `4 kg/kWe` ([NASA TechPort project 125579](https://techport.nasa.gov/projects/125579)).

The game VASIMR x1 uses:

- `1,000 N`,
- `147 km/s`,
- `60%` efficiency,
- `123 MW`.

This is an aggressive but recognizable extrapolation. It raises exhaust velocity by about three and thrust by about 172 relative to VX-200; the corresponding power increase of hundreds is expected. Applying the TechPort flight-unit goal of `4 kg/kWe` to 123 MW would yield roughly 492 tonnes for the propulsion equipment, whereas the current drive template charges zero.

### Balance opinion

- The power number is reasonable for the requested performance.
- The zero hardware mass is not.
- A VASIMR-style system should pay for large superconducting magnets, RF generators, thermal control, and radiator area.
- VASIMR should be treated as a variable operating envelope rather than separate magic engines: more thrust means lower exhaust velocity at fixed power.

## Nuclear thermal propulsion

NASA states that nuclear thermal propulsion offers high thrust at roughly twice chemical propellant efficiency ([NASA Space Nuclear Propulsion](https://www.nasa.gov/space-technology-mission-directorate/tdm/space-nuclear-propulsion/)). Historical NERVA design targets were approximately:

- `75,000 lbf`, or about `334 kN`,
- `825 s Isp`, or about `8.09 km/s`,
- multiple restarts,
- at least 10 hours at rated temperature.

See the [NERVA engine performance summary](https://ntrs.nasa.gov/api/citations/19710017291/downloads/19710017291.pdf) and NASA's [NERVA technology review](https://ntrs.nasa.gov/api/citations/20150002614/downloads/20150002614.pdf?attachment=true).

The game's Advanced Nerva x1 is almost an exact match at `334,061 N` and `8.09 km/s`. This is one of the most defensible advanced-drive entries in the template. The basic Nerva uses the same exhaust velocity but only `49 kN`, representing a smaller engine.

The game's many higher fission-thermal entries at `16–69 km/s` are not ordinary solid-core NTP. They require gas-core, vapor-core, open-cycle, or direct plasma assumptions. Materials no longer support treating them as a smooth improvement to NERVA.

### Balance opinion

- Preserve Advanced Nerva's performance point.
- Charge reactor, pressure vessel, turbomachinery, shielding/stand-off, and engine mass explicitly.
- Separate solid-core NTP from gas-core and open-cycle concepts with a major technology and reliability discontinuity.
- Do not let a modest material upgrade multiply exhaust velocity by ten while retaining solid-core-like reliability.

## Nuclear pulse propulsion

NASA's review of external nuclear pulse propulsion describes a high-thrust system with roughly `2,500–5,000 s Isp`, about `0.5 g` acceleration in studied architectures, and severe contamination/EMP constraints that push operation beyond geosynchronous orbit ([NASA propulsion architecture assessment](https://ntrs.nasa.gov/api/citations/20000085870/downloads/20000085870.pdf)). Project Orion received substantial government study but never flew an integrated nuclear-pulse vehicle ([NASA, “Nuclear Pulse Propulsion: Orion and Beyond”](https://ntrs.nasa.gov/archive/nasa/casi.ntrs.nasa.gov/20000096503.pdf)).

The game Orion x1 is `16 MN` at about `4,293 s Isp`. That sits inside the published concept range and is a reasonable paper-design value. Advanced Orion at about `12,237 s Isp` exceeds the cited pusher-plate range and needs a different impulse-coupling technology, not just an incremental upgrade.

### Balance opinion

- Orion's performance is more defensible than its operational convenience.
- Model minimum ship size, shock structure, nuclear pulse-unit magazine mass, stand-off restrictions, poor tactical throttling, and political/environmental constraints.
- Treat Advanced Orion as a distinct magnetic or fusion-pulse branch.

## Fission-fragment propulsion

NASA NIAC work reported:

- a direct fission-fragment concept at about `527,000 s Isp` and very low thrust,
- a gas-augmented form near `32,000 s Isp` and about `1,000 lbf` or `4.45 kN`,
- very large spacecraft and radiator requirements.

See NASA's [FFRE spacecraft studies](https://ntrs.nasa.gov/citations/20150002578) and [concept assessment](https://ntrs.nasa.gov/citations/20160010095).

The game's Fission Frag Drive x1 is `4.651 kN` at about `32,009 s Isp`, an extremely close match to the gas-augmented NIAC case. Dusty Plasma at `3,750 km/s` is below the NIAC direct concept's approximately `5,168 km/s` exhaust velocity.

This is good science-fiction sourcing. It is not evidence that such a system can be made compact, durable, or combat-ready. The NASA study itself calls out a very large vehicle and extensive radiator area.

### Balance opinion

- Keep the performance relationship.
- Add enormous reactor, magnetic nozzle, radiator, shielding, and low-density-core structural costs.
- Make the high-thrust gas-augmented mode consume more propellant and lose specific impulse.

## Nuclear salt water

The game's Neutron Flux engines combine meganeuton thrust with `66–1,700 km/s` exhaust velocity and list no external power requirement or drive mass. Robert Zubrin's original published concept targeted high thrust at about `10,000 s Isp`, or roughly `98 km/s` exhaust velocity ([Zubrin, “Nuclear Salt Water Rockets”](https://interstellar-flight.ru/design/base_e/nswr.pdf)). There is no prototype reactor, injector, containment system, or ground-test program from which the integrated performance can be validated. The game's lower entry is in the paper concept's broad performance region; the `1,700 km/s` torch is a much more speculative extension.

### Balance opinion

- Classify this as concept-only, beyond gas-core fission.
- Add extreme radiological hazard, non-reusability or short life, massive propellant handling constraints, and restricted use near inhabited assets.
- Do not assign zero engine mass simply because the power is generated in the propellant.

## Fusion propulsion

Fusion is not yet a net-electric power-plant technology. ITER is designed for `500 MW` fusion output from `50 MW` of plasma heating, but will not generate electricity ([ITER objectives](https://www.iter.org/fusion-energy/what-will-iter-do)). NIF has achieved target gain above four, but that compares fusion yield with laser energy delivered to the target rather than the total facility electricity input ([LLNL NIF record](https://lasers.llnl.gov/news/target-breakthrough-enabled-fusion-record-nif)).

NASA fusion-propulsion studies are explicit concept programs. One Fusion Driven Rocket study proposes direct energy transfer to propellant at exhaust velocity above `30 km/s`, while listing physics validation and subscale breakeven tests as unfinished work ([NASA Fusion Driven Rocket](https://www.nasa.gov/general/the-fusion-driven-rocket-nuclear-propulsion-through-direct-conversion-of-fusion-energy/)). NASA's PuFF concept proposes about `30,000 s Isp`, or roughly `294 km/s` ([NASA PuFF](https://www.nasa.gov/general/pulsed-fission-fusion-puff-propulsion-concept/)).

The game's lowest fusion entries begin around `270 km/s`, close to PuFF's concept target. Its highest exceed `10,000 km/s`, for which there is no integrated engineering validation.

### Balance opinion

- Early fusion drives should begin as large, low-duty-cycle, maintenance-intensive pulse systems near the PuFF/Fusion Driven Rocket concept range.
- Do not infer compactness from plasma physics alone. Magnets, neutron shielding, tritium breeding, heat rejection, pulse power, and structural fatigue are system-level constraints.
- Treat aneutronic and direct-conversion claims as later branches with their own bremsstrahlung and fuel-cycle penalties.

## Antimatter propulsion

Antimatter annihilation has extraordinary theoretical energy density, but the practical problems are production, collection, storage, extraction, radiation management, and conversion to directed thrust. NASA's antimatter-rocket work is a design study, not a prototype ([NASA antimatter rocket concepts](https://ntrs.nasa.gov/search.jsp?R=19820013176)). NASA also concluded that conventional antimatter propulsion was impractical with the production infrastructure considered, while antimatter-catalyzed systems required far smaller quantities ([NASA antimatter production study](https://ntrs.nasa.gov/citations/19990080056)).

The game's `360–14,720 km/s` exhaust velocities are not obviously forbidden by annihilation energy, but no empirical basis exists for the associated engine mass, 99.8% efficiency, lifetime, or meganeuton thrust.

### Balance opinion

- Do not describe antimatter values as extrapolated modern engineering.
- Use antimatter first as an ignition catalyst for fission/fusion before allowing a pure beam-core torch.
- Make production and storage infrastructure the dominant constraint.
- Apply severe containment failure consequences and nonzero radiation/thermal-management mass.

## Suggested propulsion realism tiers

| Tier | Technologies | Numerical treatment |
|---|---|---|
| 1: operational extrapolation | Chemical, resistojet, arcjet, Hall, ion | Anchor to demonstrated thrust-to-power, engine mass, lifetime, and propellant storage. |
| 2: grounded development | NERVA-class NTP, high-power Hall/ion arrays, VASIMR | Allow scaling, but charge complete-system mass and radiators. |
| 3: historical/concept engineering | Orion, gas-core fission, fission fragment | Preserve paper performance while imposing large architecture and reliability costs. |
| 4: unvalidated fusion | PuFF, tokamak/mirror/direct fusion drives | Use broad ranges and strong uncertainty penalties; no claim of prototype validation. |
| 5: speculative | Nuclear salt water at torch performance, pure antimatter torch | Balance as setting technology, not extrapolated modern hardware. |
