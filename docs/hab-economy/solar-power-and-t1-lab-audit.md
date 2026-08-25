# Solar power and T1 laboratory physical audit

Date: 2026-08-24  
Status: research complete; generator physical-mass correction implemented and
deployed; laboratory and T2/T3 solar operating proposals are analysis only

Deployment verification on 2026-08-24 passed the complete TI 1.0.51 release
pipeline, including 1,133 formula assertions, guarded Harmony/IL checks, exact
110-module coverage, and hash verification of all 46 deployed files. Manual
in-game acceptance is pending.

## Conclusions

1. The deployed generator construction bill is already 2x, but the physical
   template mass is not. The correction in this pass makes all 17 direct
   generators physically 2x and removes the separate runtime construction
   multiplier. Construction resources and Boost therefore remain 2x overall,
   rather than becoming 4x. Build time remains unchanged.
2. The solar construction recipe is 75% base metals, 20% volatiles, and 5%
   noble metals. That is a sensible abstraction for photovoltaic blankets,
   deployment structure, power electronics, and an associated radiator.
3. The hab interface does not assign a physical unit to one power point.
   Cross-checking the catalogue makes **1 power approximately 1 MW** the only
   coherent interpretation. This is an inference, not an explicit game rule.
4. A dedicated ISS-equivalent scientist and their share of research logistics
   plausibly account for roughly 0.09-0.33 t/month depending on discipline.
   The T1 Energy and Life labs currently charge 5.505 and 5.500 t/month
   including their scientist, far beyond that range. The zero-upkeep labs are
   closer in total mass only because the game scientist alone consumes
   0.5 t/month.
5. Continuous onboard staffing of 10 people for a T2 Solar Array and 50 for a
   T3 Solar Farm is not physically credible. A conservative crew-only pass
   would use 1 and 4. A later physics-led pass could also raise output to 230
   and 1,800 power respectively so specific power no longer gets worse with
   tier.

## Generator construction cost and physical mass

The current runtime rewrite computes a direct generator's construction
resources from twice its physical mass. Boost is then calculated from those
doubled resources. Thus a 25 t Solar Collector presently incurs a 50 t
resource/Boost construction bill, but remains a 25 t object.

The approved interpretation is simpler: a placed generator represents two
plants, so its physical mass becomes the already-charged construction mass.
The separate 2x cost multiplier is removed. This preserves the present bill
while correcting the object's mass and avoiding a 4x charge.

| Generator | Tier | Deployed physical mass (t) | Corrected physical mass (t) | Construction-equivalent mass before/after (t) | Power |
|---|---:|---:|---:|---:|---:|
| Automated Fission Pile | 1 | 45 | 90 | 90 | 40 |
| Automated Solar Collector | 1 | 25 | 50 | 50 | 40 |
| Fission Pile | 1 | 30 | 60 | 60 | 40 |
| Fusion Pile | 1 | 45 | 90 | 90 | 80 |
| Heavy Fission Pile | 1 | 60 | 120 | 120 | 60 |
| Heavy Fusion Pile | 1 | 90 | 180 | 180 | 140 |
| Solar Collector | 1 | 25 | 50 | 50 | 40 |
| Fission Reactor Array | 2 | 150 | 300 | 300 | 170 |
| Fusion Reactor Array | 2 | 225 | 450 | 450 | 340 |
| Heavy Fission Reactor Array | 2 | 300 | 600 | 600 | 256 |
| Heavy Fusion Reactor Array | 2 | 450 | 900 | 900 | 600 |
| Solar Array | 2 | 115 | 230 | 230 | 160 |
| Fission Reactor Farm | 3 | 1,200 | 2,400 | 2,400 | 500 |
| Fusion Reactor Farm | 3 | 1,800 | 3,600 | 3,600 | 1,000 |
| Heavy Fission Reactor Farm | 3 | 2,400 | 4,800 | 4,800 | 750 |
| Heavy Fusion Reactor Farm | 3 | 3,600 | 7,200 | 7,200 | 1,800 |
| Solar Farm | 3 | 900 | 1,800 | 1,800 | 480 |

Solar Mirrors remain excluded because they consume power and modify a separate
collector rather than generating positive power directly.

## What the game's resources mean

The installed English localization defines the categories broadly:

- **Volatiles:** carbon, nitrogen, oxygen, sulfur, chlorine, phosphorus, and
  hydrogen-bearing compounds. This category includes polymers, carbon-fiber
  matrix, adhesives, oxygen in glass, insulation, and ammonia coolant.
- **Base metals:** iron, nickel, lead, zinc, copper, aluminum, tin, lithium,
  and metalloids such as silicon and boron. This category includes cells,
  wiring, aluminum structure, and most radiator mass.
- **Noble metals:** silver, gold, platinum, plus corrosion-resistant titanium
  and tungsten. This category includes precious-metal contacts/coatings and
  titanium or tungsten hardware.

These are paraphrases of `UIGeneralControls.en`, lines 55-57, in the installed
TI 1.0.51 localization.

## Solar panels plus radiator: composition

Every direct solar generator uses the same construction ratio:

| Resource | Share | Corrected T1, 50 t | Corrected T2, 230 t | Corrected T3, 1,800 t |
|---|---:|---:|---:|---:|
| Volatiles | 20% | 10.0 t | 46.0 t | 360.0 t |
| Base metals | 75% | 37.5 t | 172.5 t | 1,350.0 t |
| Noble metals | 5% | 2.5 t | 11.5 t | 90.0 t |

A NASA flexible-array study gives a 48.50 kg solar-blanket bill of materials:
29.26 kg cells, 7.26 kg coverglass, 1.81 kg interconnectors, 4.04 kg substrate,
3.50 kg adhesive, and 2.63 kg of bus strips, insulation, leaders,
terminations, hinges, and jumpers. Classifying only the unambiguous mass gives:

- 29.26 kg of semiconductor cells as base material;
- the 7.26 kg silica coverglass as about 3.39 kg silicon/base and 3.87 kg
  oxygen/volatile by molecular mass;
- 7.54 kg substrate and adhesive as volatile; and
- 4.44 kg conductor and hardware mass that may fall anywhere between base and
  noble depending on alloy and contact material.

That brackets the bare blanket at **67.3-76.5% base, 23.5% volatile, and
0-9.2% noble**. [NASA solar-blanket mass breakdown](https://ntrs.nasa.gov/api/citations/19730020274/downloads/19730020274.pdf)

The associated structure and radiator push the mixture toward base metals.
NASA describes the ISS radiator as aluminum facesheets and honeycomb with
stainless-steel flow channels; one assembly is 1,123 kg for 147 m². The
working fluid is ammonia, while coatings, insulation, hoses, and composite
deployment members contribute volatile material. [NASA ISS radiator technology](https://ntrs.nasa.gov/api/citations/20100033102/downloads/20100033102.pdf)
The photovoltaic cooling loop's approximately 53 lb ammonia inventory and its
historic 1.5 lb/year leak show that coolant is a modest construction inventory
and a tiny maintenance flow, not the majority of system mass.
[NASA PVTCS leak history](https://ntrs.nasa.gov/citations/20140000513)

**Assessment:** 75/20/5 lies inside the blanket evidence and remains sensible
after adding aluminum-heavy support and radiator hardware. Five percent noble
is generous, but defensible under TI's unusually broad noble category, which
includes titanium and tungsten. No solar construction-composition change is
recommended.

The radiator comparison is deliberately conservative. Photovoltaic cells
radiate most unconverted sunlight directly from their own area; a separate
pumped radiator primarily cools batteries, conversion electronics, and the
loads supplied by the array. Assigning an entire ISS-class radiator to each
array therefore overstates the radiator burden of generation alone.

## What one hab power means

No hab localization or template labels the abstract unit as kW, MW, or GW.
Three interpretations can nevertheless be tested:

| Interpretation | T1 Solar Collector | T1 Particle Collider | Interstellar Launching Laser | Assessment |
|---|---:|---:|---:|---|
| 1 power = 1 kW | 40 kW | 100 kW | 5 MW | Collider and launching laser far too small |
| **1 power = 1 MW** | **40 MW** | **100 MW** | **5 GW** | Coherent order of magnitude |
| 1 power = 1 GW | 40 GW | 100 GW | 5 TW | T1 equipment implausibly enormous |

The ship UI separately and explicitly formats power in MW and GW, while hab
power stays unitless. The approximately-MW interpretation is therefore a
physical reading of the balance scale, not proof that every point is exactly
one nameplate megawatt.

For comparison, the eight legacy ISS arrays provide roughly 75-90 kW average
station power, and a pair of iROSAs had 1,380 kg launch mass and provides more
than 40 kW. That pair is at most 34.5 kg/kW before assigning any separate
radiator. [NASA ISS facts](https://www.nasa.gov/international-space-station/space-station-facts-and-figures/),
[NASA CRS-22 manifest](https://www.nasa.gov/wp-content/uploads/2021/05/spacex_crs-22_mision_overview_high_res.pdf)

At 1 point ≈ 1 MW, corrected TI solar specific mass is:

| Module | Corrected mass | Current output | Specific mass | Specific power |
|---|---:|---:|---:|---:|
| T1 Solar Collector | 50 t | 40 MW | 1.250 kg/kW | 800 W/kg |
| T2 Solar Array | 230 t | 160 MW | 1.438 kg/kW | 696 W/kg |
| T3 Solar Farm | 1,800 t | 480 MW | 3.750 kg/kW | 267 W/kg |

These values are 9-28 times the iROSA's array-only specific power. That is
aggressive but acceptable for a forward-looking technology tree, particularly
if module mass excludes station-wide storage and distribution. The strange
part is progression: specific power becomes worse at every tier.

## Solar maintenance automation and crew

Solar arrays need monitoring, switching, inspection, occasional repair, and
eventual replacement. They do not need people continuously walking the array.
In orbit there is no dust removal, routine inspection is performed by cameras
and telemetry, and dangerous external work is episodic. On a surface base,
dust, thermal cycling, cabling, and deployment faults add work, but autonomous
cleaning and inspection robots are far cheaper than maintaining dozens of
people solely for the array.

ISS experience illustrates both sides:

- legacy arrays were designed for 15 years and operated for more than 20;
- each iROSA needed two installation EVAs, but deployment itself took about
  ten minutes and operation is remotely monitored;
- leak localization can be performed with a robot arm and a remotely operated
  leak detector, although final hardware replacement may still require an
  EVA; and
- a serious cooling-loop leak is an exceptional repair event, not a permanent
  staffing requirement.

[NASA iROSA life and installation](https://www.nasa.gov/missions/station/new-solar-arrays-to-power-nasas-international-space-station-research/),
[NASA remote leak-location history](https://ntrs.nasa.gov/citations/20190030293)

The current 10 and 50 crew imply 20,800 and 104,000 onboard labor-hours per
year at one 40-hour job per person. No flight experience supports that order
of magnitude. It is more plausible that most monitoring belongs to ground or
station-wide operations, with a small local team maintaining robotics,
switchgear, coolant, and replacement inventory.

### T2/T3 proposal for a later pass

| Module | Current mass | Current power | Current crew | Conservative crew-only | Physics-led power/crew | Resulting kg/kW |
|---|---:|---:|---:|---:|---:|---:|
| T2 Solar Array | 230 t | 160 | 10 | 1 | 230 / 1 | 1.000 |
| T3 Solar Farm | 1,800 t | 480 | 50 | 4 | 1,800 / 4 | 1.000 |

Recommendation: implement the crew-only values first if minimizing balance
disruption is paramount. For a later energy-economy pass, use the physics-led
outputs as the cleaner endpoint. They give T2 and T3 equal specific power,
still improve modestly over T1's 1.25 kg/kW, and leave solar's distance and
illumination dependence as its strategic limitation. The T3 increase is
large, so reactor cost, fuel independence, distance from the Sun, and surface
day/night behavior should be tested together before approval.

## T1 laboratory consumption with one scientist

### Flight-program anchors

NASA's ISS utilization statistics provide the best aggregate normalization:

- Expeditions 59/60 used 5,831 kg research upmass, returned 3,064 kg, and
  recorded 2,857 research crew-hours.
- Expeditions 61/62 used 4,421 kg upmass, returned 1,059 kg, and recorded
  2,589 research crew-hours.
- Across Expeditions 0-62, research used 88,766 kg upmass, 27,682 kg downmass,
  and 48,545 crew-hours.

At a 40-hour scientist-week, these work out to roughly **0.30-0.35 t gross
upmass per scientist-month** and **0.17-0.23 t net upmass per
scientist-month**. Gross upmass includes durable racks, replacement hardware,
packaging, and samples later returned; it is an upper bound on actual
consumption. [NASA ISS utilization statistics](https://www.nasa.gov/wp-content/uploads/2021/08/expeditions_0-62_statistics_brochure_-_final.pdf)

A lower-utilization example is CRS-5: 577 kg of science cargo supported 256
investigations during Expeditions 42 and 43, while NASA described the station's
research schedule as about 40 crew-hours per week. Spreading that one manifest
over six months is about 0.096 t per full-time scientist-month, again including
hardware rather than only expended matter.
[NASA CRS-5 manifest](https://www.nasa.gov/wp-content/uploads/2018/07/spacex_crs-5_factsheet.pdf),
[NASA research-time history](https://www.nasa.gov/missions/station/marshall-supports-15-years-of-international-space-station-discoveries/)

Skylab provides a useful independent boundary. It flew as a fully outfitted
dry workshop, hosted nearly 300 experiments over 171 occupied days, and was
designed to support its three crewed visits without workshop resupply. Limited
experiment cargo arrived with the Apollo crews, but there was no multi-tonne
monthly laboratory feedstock stream. Its 1,225 kg food system supported three
three-person missions for 156 days, or about 78.5 kg per person-month including
food packaging and support items—close to the 83.6 kg ISS personal-supply
yardstick used in the earlier audit. [NASA Skylab overview](https://www.nasa.gov/skylab/),
[NASA Skylab workshop report](https://ntrs.nasa.gov/api/citations/19740020215/downloads/19740020215.pdf),
[NASA Skylab food system](https://ntrs.nasa.gov/search.jsp?R=19740059359)

### Discipline estimates and game comparison

The ranges below deliberately include the scientist's 0.0836 t/month ISS
personal supplies plus a generous allocation for experiment packages, rack
spares, filters, samples, reagents, targets, and packaging. They are therefore
closer to a logistics planning allowance than strict chemical consumption.
The game column includes the mod's 0.5 t/month water-and-volatiles charge for
one scientist.

| T1 lab | Real lab allowance (t/mo) | Real total with scientist (t/mo) | Game module (t/mo) | Game total with scientist (t/mo) | Game / real-total range |
|---|---:|---:|---:|---:|---:|
| Climate | 0.005-0.025 | 0.089-0.109 | 0 | 0.500 | 4.6-5.6x |
| Energy | 0.030-0.120 | 0.114-0.204 | 5.005 | 5.505 | 27.0-48.3x |
| Information Science | 0.002-0.015 | 0.086-0.099 | 0 | 0.500 | 5.1-5.8x |
| Life Science | 0.050-0.180 | 0.134-0.264 | 5.000 | 5.500 | 20.8-41.0x |
| Materials | 0.030-0.150 | 0.114-0.234 | 1.050 | 1.550 | 6.6-13.6x |
| Military Science | 0.010-0.050 | 0.094-0.134 | 0 | 0.500 | 3.7-5.3x |
| Social Science | 0.002-0.015 | 0.086-0.099 | 0 | 0.500 | 5.1-5.8x |
| Space Science | 0.005-0.030 | 0.089-0.114 | 0 | 0.500 | 4.4-5.6x |
| Xenology | 0.080-0.250 | 0.164-0.334 | 0 | 0.500 | 1.5-3.0x |

Interpretation by discipline:

- Climate, information, social, military, and space science are dominated by
  sensors, computing, observation, and data. Their marginal matter flow is
  small once durable hardware exists.
- Life science has the strongest routine claim on water, gases, culture media,
  sample containers, cold-stowage packaging, and sterilization supplies.
- Materials and energy research can expend samples, targets, gases, furnace
  liners, filters, and cryogenic stores, but tonnes per month are still not
  supported by ISS-scale operations.
- Xenology is unknowable by definition. The high band assumes aggressive
  sampling, containment, sterilization, destructive testing, and discarded
  protective hardware.

The zero-upkeep labs should not automatically receive new resource categories;
that would violate the approved preservation rule. Instead, a later lab pass
can reduce Energy, Life, and Materials and decide whether the already-inflated
crew charge is sufficient abstraction for the other disciplines. No lab value
is changed by this report.
