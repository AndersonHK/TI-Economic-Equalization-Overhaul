# Fission reactor structural scaling and output caps

Last reviewed: 2026-07-29

## Question this report answers

The companion report on
[thermodynamics and fuel inventory](thermodynamic-and-fuel-limits.md) asks how
light an ideal reactor could be if only energy content, conversion
thermodynamics, and a chosen fuel endurance are counted. That is a useful lower
bound, but it does not determine the maximum output of one practical reactor.

This report asks a different question:

> How does the maximum output of one reactor unit emerge from fuel conduction,
> coolant heat transfer, pressure containment, thermal stress, neutron damage,
> and the size of its conversion machinery?

The answer is not a universal `GW` ceiling. A large plant can repeat fuel
elements, coolant channels, pumps, and converters. The useful balance variable
is therefore the maximum output of one *integrated unit* before duplicating
loops and structures becomes preferable.

## Installed-game anchor

The installed template for `GasCoreFissionReactorVI` has:

| Field | Value |
|---|---:|
| Maximum output | 1,650 GW electric |
| Specific mass | 1 t/GW electric |
| Efficiency | 96% |
| Full-rating plant mass | 1,650 t |

The earlier suggestion of `1 t/GW` as an extreme lower-bound ratio therefore
does not constrain this reactor: the game already reaches it. It remains useful
as a fuel-inventory and thermodynamic perspective, but output cap, endurance,
heat-transfer area, and converter mass must be tested separately.

At full rating and 96% electrical efficiency, the reactor requires about
`1,719 GWth`. One year of ideal complete U-235 fission would consume roughly
`660 t` of fissile material. That is about 40% of the template's entire plant
mass before adding moderator, reflector, containment, coolant, converter,
shielding, controls, or structural margin.

## A compact theoretical model

For one reactor unit, use the lowest applicable limit:

`Pthermal,max = min(Pfuel conduction, Pcoolant interface, Pcoolant flow, Pstructure, Pneutronics, Pconverter / efficiency)`

The terms can be written as:

- fuel conduction: `q'''max × Vfuel`;
- coolant interface: `q''max × Awetted`;
- coolant enthalpy transport: `mass flow × cp × coolant temperature rise`;
- structure: the output at which thermal stress, pressure stress, creep,
  fatigue, or neutron damage reaches the design limit;
- neutronics: the controllable fission rate allowed by critical geometry,
  reactivity coefficients, delayed-neutron control, and material lifetime;
- converter: the rated heat input of the turbine, thermocouple, MHD channel,
  or direct-conversion system.

This structure makes the balancing issue legible. Raising neutron flux alone
does not help if heat cannot cross the fuel or coolant boundary. Raising core
temperature does not help if the converter or radiator cannot accept it.

## Solid-fuel radial conduction

For a uniformly heated cylindrical fuel element with radius `a`, constant
thermal conductivity `k`, and an allowed center-to-surface temperature
difference `ΔT`:

`q'''max = 4 k ΔT / a²`

The inverse-square dependence on fuel radius is why high-output solid reactors
use many thin pins or plates rather than one large fuel block. The IAEA notes
that UO2 has low thermal conductivity and that center temperature and radial
temperature profile govern important fuel behavior; conductivity also degrades
with temperature and burnup
([IAEA fuel-temperature discussion](https://www-pub.iaea.org/MTCD/Publications/PDF/te_970_prn.pdf)).

Illustrative values for `ΔT = 1,000 K`:

| Effective conductivity | Fuel radius | Conduction-limited power density | Fuel-only mass at 10 t/m³ per GWth |
|---:|---:|---:|---:|
| 2 W/m·K | 5 mm | 0.32 GW/m³ | 31.3 t |
| 10 W/m·K | 5 mm | 1.60 GW/m³ | 6.25 t |
| 20 W/m·K | 5 mm | 3.20 GW/m³ | 3.13 t |
| 2 W/m·K | 2 mm | 2.00 GW/m³ | 5.00 t |
| 10 W/m·K | 2 mm | 10.0 GW/m³ | 1.00 t |
| 20 W/m·K | 2 mm | 20.0 GW/m³ | 0.50 t |

These are fuel-only figures. At 50% electrical efficiency, double them per
GWe, then add cladding, coolant void, moderator or reflector, control hardware,
pressure vessel, manifolds, pumps, converter, shielding, and service access.

### What happens as the reactor grows

If a solid core were enlarged while preserving one monolithic conduction
distance, its power grows only approximately with radius while its mass grows
with radius cubed. Real reactors avoid that penalty by repeating small fuel
elements and coolant channels. Consequently:

- there is no fundamental two-gigawatt fission ceiling;
- the *fuel-cell geometry* should remain small even when total plant output is
  large;
- a very large plant is physically a cluster of repeated thermal cells,
  coolant loops, and conversion trains even if the game displays one module.

## Neutron flux is not usually the first ceiling

A rough fission-power density is:

`q''' = neutron flux × fissile atom density × fission cross section × energy per fission`

Using a thermal flux of `10^15 n/cm²/s`, an illustrative 5% U-235 atom
density, a thermal fission cross section near 585 barns, and 200 MeV per
fission gives a nuclear heat-generation scale of about `45 GW/m³`.

That is not a recommended operating point. It shows that plausible neutron
flux can drive much more heat than ordinary solid fuel can conduct away. ORNL
reports a peak HFIR flux of `2.5 × 10^15 n/cm²/s`
([ORNL HFIR overview](https://www.ornl.gov/media/83440)). Higher flux also
accelerates swelling, embrittlement, gas production, control difficulty, and
component replacement. For early solid reactors, cooling and material lifetime
are therefore more useful balance constraints than a bare fission-rate limit.

## Coolant and interface scaling

Heat removal must satisfy both:

`P = q'' × Awetted`

and:

`P = mass flow × cp × ΔTcoolant`

Increasing wetted area requires more cladding, channels, manifolds, and pressure
boundary. Increasing mass flow requires larger passages and pumping power.
Pressure drop rises rapidly with velocity, while pump and pipe mass do not
vanish at high scale. A design that solves fuel conduction with very fine
channels may create excessive pressure loss, clogging sensitivity, erosion, or
manufacturing complexity.

This explains why coolant and conversion hardware can dominate a plausible
reactor design even when the fissile fuel is compact.

## Thermal stress and pressure containment

A useful first stress scale is:

`thermal stress ≈ E α ΔT / (1 - ν)`

where `E` is Young's modulus, `α` thermal expansion, and `ν` Poisson's ratio.
Real components relieve some stress by expansion, joints, geometry, creep, and
temperature grading, but sharp gradients remain costly.

For a thin pressure vessel:

`wall thickness / vessel radius ≈ pressure / allowable stress`

At fixed pressure and material, vessel mass grows broadly with contained
volume. Very large hot pressure boundaries also face creep, weld inspection,
fatigue, transient control, and battle-damage problems. Liquid and gas cores
remove the solid fuel's internal conduction gradient, but they do not remove
the walls, heat exchanger, separator, or converter limits.

## Why civil reactors cluster rather than scale without limit

The common civil-reactor range around one gigawatt electric is an engineering
and economic optimum, not a law of fission. Larger units must concentrate:

- a very large pressure vessel and primary loop;
- turbine-generator train limits;
- grid and outage risk;
- emergency cooling and decay-heat removal;
- manufacturing, transport, and inspection constraints.

A spacecraft removes some terrestrial constraints but adds radiator area,
launch or in-space fabrication, mass, acceleration loads, micrometeoroids, and
combat survivability. A credible far-future design may exceed civil output,
but it should do so by explicit replication and improved materials rather than
by assuming scale has no penalty. The IAEA's PRIS database is the appropriate
reference catalog for real unit capacities and design characteristics
([IAEA PRIS](https://pris.iaea.org/PRIS/About.aspx)).

## Technology-specific implications

| Core type | Constraint relieved | New or strengthened constraint | Sensible qualitative scaling |
|---|---|---|---|
| Solid | none; heat must cross fuel and cladding | fuel conduction, cladding temperature, channel pressure loss | repeat many thin fuel elements; comparatively low unit cap |
| Molten/liquid | eliminates solid-fuel centerline gradient and permits online mixing | corrosion, erosion, pumps, critical inventory outside core, heat exchangers | larger unit output is plausible, but loop and exchanger mass remain |
| Vapor | hotter fuel and potentially higher-temperature conversion | phase control, condensation, deposition, pressure boundary | higher efficiency, difficult materials and cleanup |
| Gas core | very high source temperature; possible radiative transfer or MHD conversion | fissile-gas confinement, opacity, wall heat flux, fuel separation and loss | high output is plausible only with a correspondingly large chamber and converter |

NASA gas-core studies examined uranium plasma at `10,000 K` and above,
illustrating why gas-core temperature is a different regime rather than a
modest molten-core upgrade
([NASA review of multigigawatt gas-core concepts](https://ntrs.nasa.gov/api/citations/19930009659/downloads/19930009659.pdf)).

## Candidate balance model

Do not use one universal `t/GW` line. Give each technology:

1. a linear plant term in `t/GW`;
2. a fixed mass per independent reactor/loop/converter train;
3. a maximum output per train;
4. an endurance or fuel-inventory term;
5. shielding and radiator terms that are charged elsewhere only if the game
   actually accounts for them.

A compact expression is:

`Mplant = N × Mfixed + a × Pgross + Mfuel(Pthermal, endurance) + Mshield`

where:

`N = ceiling(Pgross / Punit cap)`

This makes clustering visible. Crossing the unit cap adds a new vessel, pump
set, control system, and converter instead of allowing perfectly smooth scale.

### Provisional cap bands for testing

These bands are balance hypotheses, not theoretical constants:

| Technology | Provisional output per integrated train | What must be repeated above it |
|---|---:|---|
| Early solid core | 1–5 GWe | fuel assembly, primary loop, converter |
| Advanced/compact solid core | 5–20 GWe | primary loop and converter; fuel elements remain subdivided |
| Molten/liquid core | 20–100 GWe | vessel/loop, heat exchanger, converter |
| Vapor core | 50–250 GWe | vapor circuit, separator/condensor, converter |
| Gas core | 100–500 GWe | plasma chamber, confinement and fuel-recovery system, converter |

The ranges intentionally overlap. Materials, operating pressure, temperature,
conversion method, lifetime, and redundancy matter more than the phase label
alone. A `1,650 GWe` gas-core template could be represented as four or more
very advanced trains; presenting it as one seamless reactor should carry an
explicit mass and reliability benefit that needs justification.

## Conclusions

- The earlier `1 t/GW` figure is a lower-bound facet, not a useful standalone
  cap for the game's most advanced fission plants.
- Solid-core output should be raised by repeating small fuel elements, not by
  increasing their conduction distance.
- Plausible neutron flux is capable of generating more heat than ordinary
  solid fuel can remove; thermal transport and material damage are the more
  informative early limits.
- Molten and gas cores can reach higher temperatures and unit outputs, but
  their containment, heat-exchange, and conversion machinery must grow too.
- The cleanest game model is a linear specific-mass term plus fixed mass and a
  per-train output cap, not one universal tons-per-gigawatt floor.

No reactor cap or mass change is settled by this report.
