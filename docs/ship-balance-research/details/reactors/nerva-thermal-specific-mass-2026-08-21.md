# NERVA-class thermal-reactor specific mass

Status: research and balance analysis only. No gameplay values are changed by
this report.

Last reviewed: 2026-08-21

## Bottom line

The historical four-gigawatt-class NERVA-family result was not hundreds of
tonnes per thermal gigawatt. Phoebus-2A demonstrated approximately `4.1 GWth`,
and the ROVER/NERVA program summary reports a minimum reactor specific mass of
`2.3 kg/MWth`. Because `1 kg/MW` is numerically `1 t/GW`, that is:

`2.3 t/GWth`, or about `9.4 t` of reactor at `4.1 GWth`.

That is a reactor boundary, not a complete flight installation. Historical
flight-engine estimates imply roughly `4-8 t/GWth` once pressure vessel,
reflector and controls, turbomachinery, piping, and nozzle are included. A
four-gigawatt engine therefore lands around `16-32 t` before mission-specific
shielding, structural separation, and any electrical conversion plant.

Modern materials do not justify another order-of-magnitude reduction. They
primarily buy fuel retention, higher temperature, life, repeatability, and
manufacturability. A modern high-enriched-uranium design might preserve or
modestly improve the historical mass envelope. A politically and
programmatically more plausible HALEU design is generally heavier because its
lower enrichment makes criticality harder. For balance work, a credible
four-gigawatt whole propulsion-reactor envelope is approximately:

- `4-7 t/GWth` (`16-28 t`) for an aggressive HEU, short-life propulsion unit;
- `6-11 t/GWth` (`24-44 t`) for a more conservative modern installation with
  HALEU and/or greater flight margin.

Those ranges are engineering inferences, not demonstrated 2020s four-gigawatt
systems. No modern program has built one.

## Keep four mass boundaries separate

The apparent contradiction between very light ROVER numbers and very heavy
space-reactor studies largely disappears when the counted hardware and output
unit are made explicit.

| Boundary | Output denominator | Hardware counted |
|---|---|---|
| ROVER reactor | `GWth` | Core, moderator, reflector, controls, and reactor pressure structure |
| Nuclear-thermal engine | `GWth` | Reactor plus hydrogen feed, turbomachinery, piping, and nozzle |
| Ship propulsion installation | `GWth` | Engine plus shadow shield, mounts, separation structure, and margins |
| Electric power plant | `GWe` | Reactor plus conversion, electrical machinery, radiators, and distribution |

An open-cycle NTR expels most reactor heat through its propellant. A space
electric reactor must convert heat to electricity and reject the remainder
through radiators. NASA electrical-reactor `kg/kWe` results therefore should
not be transferred to a NERVA reactor by merely relabeling the denominator.

## Historical anchors

### Phoebus-2A: the direct four-gigawatt answer

NASA's ROVER/NERVA achievement summary gives Phoebus-2A as `4,100 MWth` and
reports a minimum reactor specific mass of `2.3 kg/MWth`. The Los Alamos
program review independently records approximately the same `4,080 MWth`
test result and specific mass. This was ground demonstrated, not merely a
paper reactor.

| Quantity | Value |
|---|---:|
| Demonstrated thermal power | `4.08-4.10 GWth` |
| Minimum reactor specific mass | `2.3 t/GWth` |
| Implied reactor mass | `9.4 t` |

Sources: [NASA ROVER/NERVA Program Achievements](https://ntrs.nasa.gov/api/citations/20060051740/downloads/20060051740.pdf),
[Los Alamos ROVER/NERVA program review](https://digital.library.unt.edu/ark:/67531/metadc1068425/m2/1/high_res_d/5335395.pdf), and
[NASA's nuclear-rocket ground-test history](https://ntrs.nasa.gov/search.jsp?R=20140008771).

`2.3 t/GWth` should be treated as a large-reactor lower anchor. It should not
be extended linearly to a `0.28 GW` engine: critical mass, reflector,
pressure-vessel, control, and turbopump floors do not shrink in proportion to
power.

### Whole-engine estimates

Historical studies used several inconsistent engine boundaries, but they
bracket the additional machinery:

- One assessment lists an NRX-class engine at `1.5 GWth` and `15,000 lb`, or
  approximately `4.5 t/GWth`, and a Phoebus-class engine at `4.5 GWth` and
  `40,000 lb`, or approximately `4.0 t/GWth`.
- A later NASA engine-system model gives approximately `41.7 t` at `5.32
  GWth` for its Phoebus-2A case, or `7.8 t/GWth`.
- XE-Prime's approximately `18.1 t` integrated ground engine at about `1.14
  GWth` works out to `15.9 t/GWth`. It is useful as a conservative test-engine
  boundary, not as the best flight-specific-mass result.

Sources: [Nuclear-rocket feasibility assessment](https://www.osti.gov/servlets/purl/5508515/),
[NASA NTR engine-system model](https://ntrs.nasa.gov/api/citations/20050081838/downloads/20050081838.pdf), and
[NASA nuclear-rocket history, including XE-Prime](https://ntrs.nasa.gov/api/citations/19920001919/downloads/19920001919.pdf).

The spread is a warning about bookkeeping, not evidence that the Phoebus
reactor result was wrong. Test equipment, flight hardware, shielding, and the
nozzle/feed system were not consistently included in the quoted mass.

## What 2020s engineering changes

### The modern reference designs are not dramatically lighter

A 2013 NASA comparison of NERVA-derived engines supplies a useful modern
counterexample to the assumption that new materials automatically collapse
specific mass:

| NERVA-derived concept | Thermal power | Reactor/core mass | Total engine mass | Reactor/core ratio | Engine ratio |
|---|---:|---:|---:|---:|---:|
| Small | `162 MWth` | `1,435 kg` | `1,730 kg` | `8.86 t/GWth` | `10.68 t/GWth` |
| Large | `555 MWth` | `2,645 kg` | `3,305 kg` | `4.77 t/GWth` | `5.96 t/GWth` |

Source: [NASA, Nuclear Thermal Rocket Simulation Using the Nuclear and
Chemical System Analysis Code](https://ntrs.nasa.gov/api/citations/20140006199/downloads/20140006199.pdf).

The larger design has much better effective specific mass because fixed
hardware is amortized over more power. Even so, its approximately `6 t/GWth`
whole-engine result is in the historical flight-estimate band, not ten or one
hundred times better.

### Materials improve the hard parts, chiefly temperature and life

ROVER fuel was limited by hot-hydrogen corrosion, coating cracks, differential
thermal expansion, radiation damage, and fuel loss. Modern work attacks those
problems with improved zirconium-carbide and tungsten barriers, CERMET and
uranium-nitride fuel systems, finer process control, and additive manufacture
of refractory components. These advances can support higher temperature,
better fuel retention, and more repeatable cooling passages.

They remain development items rather than proof of a flight-qualified mass
reduction. NASA's CERMET work explicitly identifies continuing technical and
programmatic challenges, and recent hot-hydrogen work on uranium-nitride
CERMET still reports high-temperature thermochemical limitations. The 2021
National Academies assessment likewise identified validation of approximately
`2,700 K` fuel operation without major deterioration as a key remaining NTP
task.

Sources: [NASA tungsten-cladding project](https://techport.nasa.gov/projects/18086),
[NASA CERMET fuel development](https://ntrs.nasa.gov/api/citations/20150016484/downloads/20150016484.pdf?attachment=true),
[NASA 2024 UN-CERMET hot-hydrogen testing](https://ntrs.nasa.gov/api/citations/20240002585/downloads/Hot%20H2%20Testing%20of%20UN%20Cermet%20for%20NTP.pdf), and
[National Academies space nuclear propulsion assessment](https://nap.nationalacademies.org/skim.php?chap=70-73&record_id=25977).

### HALEU trades logistics and safeguards for mass

The ROVER reactors used highly enriched uranium. Current US demonstration
concepts emphasize HALEU. Lower enrichment is attractive for handling and
nonproliferation, but it increases the fissile inventory and/or moderator and
reflector burden needed to reach criticality.

NASA's one-megawatt HALEU demonstrator study found reactor dimensions and mass
comparable to NERVA-class hardware despite radically lower output. An INL
design study found substantially lower thrust-to-reactor-weight ratios for
HALEU than for HEU across its design family. Modern material science therefore
does not guarantee lower `t/GWth`; enrichment choice can consume the gain.

Sources: [NASA HALEU NTP demonstrator study](https://ntrs.nasa.gov/api/citations/20200001000/downloads/20200001000.pdf?attachment=true) and
[INL fast-spectrum NTR design study](https://inl.elsevierpure.com/en/publications/preliminary-conceptual-design-of-fast-neutron-spectrum-nuclear-th).

## Consequences for the live open-cycle formula

The implemented open-cycle model makes a Solid Core I reactor supply:

`Q = D / (1 - 0.01 * (1 - 0.575))`

Thus a nominal `4 GW` NTR asks for `4.017073 GWth`. Its plant mass under a
purely linear `specificPower_tGW` field is:

| Assumed field | Installed mass at `4.017073 GWth` | Interpretation |
|---:|---:|---|
| `2.3 t/GW` | `9.24 t` | Phoebus reactor-only lower anchor |
| `4 t/GW` | `16.07 t` | Aggressive integrated engine |
| `6 t/GW` | `24.10 t` | Central large-engine estimate |
| `8 t/GW` | `32.14 t` | Conservative HEU / lighter HALEU envelope |
| `10 t/GW` | `40.17 t` | Conservative flight-installation envelope |
| Live `240 t/GW` | `964.10 t` | Electrical-plant-like mass, not a direct NTR |

The live `240 t/GW` Solid Core I value is therefore roughly `30-60` times the
historical whole-engine band at four gigawatts. That may be a useful balance
mass for electricity production, but it is not supported as the thermal
propulsion reactor mass.

## Why one linear field still fails at basic-NERVA scale

For the basic unscaled Nerva, the patched demand is `0.284208 GWth`. A
Phoebus-derived `2.3 t/GW` line would assign only `0.65 t`, while the live
`240 t/GW` line assigns `68.21 t`. Neither captures the physical fixed floor.

A better propulsion-reactor model is:

`M = M_fixed + a * Q`

Two illustrative balance curves are:

| Curve | Basic Nerva, `0.284208 GWth` | Four-GW NTR, `4.017073 GWth` |
|---|---:|---:|
| Heritage/aggressive HEU: `M = 8 + 4Q` | `9.14 t` | `24.07 t` |
| Conservative modern: `M = 12 + 6Q` | `13.71 t` | `36.10 t` |

These are calibration curves, not historical component equations. They are
chosen to respect the small-engine hardware floor and the demonstrated
Phoebus/modern large-engine envelope simultaneously.

If implementation remains limited to one linear field, the appropriate value
depends on the chosen design point:

- calibrating the `0.284 GW` basic Nerva to about `10-14 t` requires roughly
  `35-50 t/GW`;
- calibrating a four-gigawatt NTR to about `24-40 t` requires roughly
  `6-10 t/GW`.

No single linear `t/GW` value can satisfy both. The clean long-term solution
is a thermal-propulsion mass path with a fixed reactor/engine term plus a
thermal-power term, while electricity production retains its converter,
radiator, and electrical-specific-mass path. If the nozzle and feed system
remain represented by a zero-mass drive template, they must be included in the
thermal plant boundary; otherwise they can move to drive mass and the reactor
coefficient can approach the lower `2.3-4 t/GWth` band.

