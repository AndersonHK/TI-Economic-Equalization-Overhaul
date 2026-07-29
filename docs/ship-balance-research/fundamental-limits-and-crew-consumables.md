# Fundamental limits and six-month crew consumables

Last reviewed: 2026-07-29

This note supplies the numerical anchors requested before making further
low-tech balance decisions. A theoretical ceiling and a usable design
assumption are not the same thing, so both are shown where possible.

## Summary numbers

| Question | Strict or chemistry-level ceiling | Useful planning anchor |
|---|---:|---:|
| Solar conversion in space | about 68% for an ideal unlimited-junction, unconcentrated cell; 86.6% under ideal concentration | **40% cell efficiency** for an extremely advanced but recognizable AM0 multijunction array; roughly **35% whole-panel** after packing and wiring |
| Hydrogen/oxygen fuel cell | about **84%** at 298 K on a higher-heating-value basis | **60–70%** for an advanced fuel-cell stack; lower for a complete regenerative round trip |
| 2,500 °C heat engine with TI's 800 K aluminum radiator | **71.2% Carnot efficiency** | substantially below 71%; **35–45%** is already generous for a complete space system |
| Conventional intercalation lithium-ion | approximately **550–600 Wh/kg** for ideal active materials | **350–400 Wh/kg at cell level** as a generous advanced value; 500 Wh/kg is an aggressive lithium-metal research target |
| Six-month food and personal consumables | about **0.37 t per crew member** using NASA mass-flow allowances | **0.4–0.45 t per crew member**, before oxygen, water, shared spares, and reserve margin |
| Six-month metabolic oxygen | **0.148–0.151 t per crew member** at nominal activity | allow **0.17–0.20 t** if stored, including activity and reserve |
| Six-month water without recycling | **0.522 t** for drinking, rehydration, and minimal hygiene; up to **0.900 t** with wash and flush water | use the mission's actual recovery fraction rather than hiding all water in crew mass |

## Solar-panel efficiency in space

“Ideal” needs three separate definitions:

1. a single solar cell;
2. a multijunction cell;
3. a complete deployable panel, including inactive gaps, interconnects,
   shielding, conductors, and deployment structure.

NASA/JPL gives the following theoretical AM0 figures:

- about **25%** for a single-junction GaAs cell;
- about **40%** for a triple-junction cell;
- calculations around **45%** for optimized AM0 triple-junction III-V/Si
  structures.

For broader thermodynamic context, detailed-balance calculations under the AM0
space spectrum approach **68%** with an unlimited number of junctions. NASA
gives **86.6%** for an infinite-junction cell under concentrated sunlight. The
latter requires concentrating optics and does not describe a normal flat
spacecraft panel.

NREL has demonstrated 39.2–39.5% under terrestrial one-sun illumination, while
NASA's current spacecraft survey calls about 40% the theoretical limit for the
triple-junction class. AM0 spectrum, operating temperature, radiation damage,
packing, and wiring make cell efficiency higher than complete-panel
efficiency.

### Planning conclusion

Use **40% as an optimistic cell-level ceiling** for a recognizable advanced
space array. If the game number is intended to represent the whole deployed
panel, **35% is a generous planning value**. Values near 68% require
effectively unlimited spectral splitting; 86.6% also requires concentration.

Primary sources:

- [NASA/JPL, Solar Power Technologies for Future Planetary Science Missions](https://solarsystem.nasa.gov/system/downloadable_items/715_Solar_Power_Tech_Report_FINAL.PDF)
- [Martí and Araújo, limiting efficiency under the AM0 spectrum](https://www.sciencedirect.com/science/article/abs/pii/S0038092X13005124)
- [NASA, III-V/Si multijunction space-photovoltaic calculations](https://www.nasa.gov/directorates/stmd/space-tech-research-grants/development-of-iii-v-si-multijunction-space-photovoltaics/)
- [NASA Small Spacecraft State of the Art: power subsystems](https://www.nasa.gov/smallsat-institute/sst-soa/power-subsystems/)
- [NREL six-junction efficiency record](https://www.nrel.gov/news/detail/press/2020/nrel-six-junction-solar-cell-sets-two-world-records-for-efficiency)

## Theoretical hydrogen/oxygen fuel-cell efficiency

For the reaction:

`H₂ + ½O₂ → H₂O(l)`

the maximum reversible electrical work at 298 K is the Gibbs free-energy
change. Dividing it by the reaction enthalpy gives the ideal higher-heating-
value efficiency:

`ηideal = ΔG / ΔH ≈ 0.83`

NASA's hydrogen/oxygen fuel-cell survey rounds this upper limit to **about
84% at 298 K and atmospheric pressure**. The same source reports advanced
alkaline hydrogen/oxygen hardware around 62–68%, which is a useful distinction
between a thermodynamic ceiling and machinery.

This 84% figure is for the fuel-cell discharge reaction, not for a complete
solar-array → electrolyzer → storage → fuel-cell round trip. Compression,
electrolysis overpotential, pumps, gas processing, storage leakage, and the
fuel-cell losses all reduce the round-trip result. NASA regenerative-fuel-cell
work reports roughly 52% demonstrated round-trip efficiency and treats less
than 60% as the basic practical envelope.

### Planning conclusion

- **84%:** strict ideal hydrogen/oxygen fuel-cell ceiling at room temperature,
  HHV basis.
- **60–70%:** generous future stack.
- **50–60%:** more defensible complete regenerative cycle before solar-array
  conversion is included.

Primary sources:

- [NASA NTRS, hydrogen/oxygen fuel-cell upper efficiency](https://ntrs.nasa.gov/api/citations/19760015576/downloads/19760015576.pdf)
- [NASA TechPort, regenerative fuel-cell system](https://techport.nasa.gov/projects/116307)
- [NASA NTRS, regenerative fuel-cell round-trip demonstration](https://ntrs.nasa.gov/citations/20070010455)

## Carnot limit: 2,500 °C hot side and aluminum cold side

Carnot efficiency is:

`ηCarnot = 1 - Tcold / Thot`

Temperatures must be absolute. A 2,500 °C hot side is:

`Thot = 2,773.15 K`

The installed Aluminum Fin radiator uses `operatingTemp_K: 800`. NASA has
studied an aluminum space radiator with a maximum allowable inlet temperature
of 833 K. Pure aluminum freezes/melts at 933.47 K, but the melting point is not
a responsible structural operating limit.

| Cold-side assumption | Carnot ceiling |
|---|---:|
| 400 K low-temperature aluminum radiator | 85.6% |
| **800 K Terra Invicta Aluminum Fin** | **71.2%** |
| 833 K NASA aluminum maximum-inlet study | 70.0% |
| 933.47 K aluminum melting point—not a usable design point | 66.3% |

A colder radiator raises thermal efficiency but requires much more radiating
area. A hotter radiator shrinks the radiator but reduces the maximum possible
engine efficiency. The relevant comparison for Terra Invicta is therefore the
800 K row, not room temperature and not aluminum's melting point.

No real cycle reaches Carnot. Turbine and compressor inefficiency, finite heat
exchanger temperature differences, pressure losses, alternator losses, pumps,
shielding, and radiator plumbing all lower delivered efficiency. NASA Brayton
space-power studies around 20–34% provide a much firmer engineering anchor.

### Planning conclusion

At the game's own aluminum-radiator temperature, **any claimed efficiency
above 71.2% is thermodynamically impossible** for a 2,500 °C heat source.
An early solid-core plant at 75% already exceeds that ceiling. A complete-system
value in the **30–40%** range is evidence-led; **45%** is a generous advanced
allowance.

Primary sources:

- [NASA NTRS, aluminum space-radiator maximum inlet of 833 K](https://ntrs.nasa.gov/api/citations/19710028762/downloads/19710028762.pdf?attachment=true)
- [NIST, aluminum freezing point of 933.473 K](https://www.nist.gov/si-redefinition/kelvin/kelvin-its-90)
- [NASA NTRS, Brayton system efficiency and radiator sensitivity](https://ntrs.nasa.gov/api/citations/20220013600/downloads/ASCEND_22_Brayton_Performance_Mass_Sensitivity.pdf?attachment=true)

## Highest theoretical lithium-ion cell density

Here “density” is interpreted as gravimetric specific energy in `Wh/kg`,
because that controls the game's module mass.

There is no single maximum for everything that can be called “lithium-ion.”
The answer changes when graphite is replaced by silicon or lithium metal, or
when an intercalation cathode is replaced by conversion, sulfur, or oxygen
chemistry. Those changes can cross into “beyond lithium-ion” even though
lithium ions still move through the electrolyte.

Useful boundaries are:

- graphite has a theoretical capacity of **372 mAh/g**;
- conventional graphite/intercalation active-material pairs are roughly
  **550–600 Wh/kg** in ideal calculations;
- inactive cell mass—electrolyte, separator, current collectors, tabs, casing,
  safety hardware, and excess electrode capacity—prevents a complete cell from
  reaching the active-material figure;
- DOE's Battery500 program treats **500 Wh/kg at cell level** as an aggressive
  research goal and uses lithium metal rather than ordinary graphite.

Some conversion/intercalation materials have chemistry-level theoretical
values above 1,000 Wh/kg. For example, iron-fluoride cathode research cites
1,922 Wh/kg for the cathode reaction. That is not a 1,922 Wh/kg complete,
rechargeable cell and should not be used to justify an early generic
Lithium-Ion Battery.

### Planning conclusion

- **About 600 Wh/kg:** defensible theoretical active-material ceiling for a
  recognizable conventional lithium-ion chemistry.
- **350–400 Wh/kg:** generous future complete-cell assumption.
- **500 Wh/kg:** aggressive lithium-metal cell target; not an ordinary rugged
  spacecraft pack.
- Packs should be lower than cells after structure, switching, thermal control,
  containment, and redundancy.

The current game module stores 12 GJ in 11 t, or **303 Wh/kg at module level**.
That is above present rugged spacecraft packs but below a generous advanced
cell ceiling. It is not ruled out by lithium-ion chemistry alone.

Primary sources:

- [DOE Battery500 progress and 500 Wh/kg cell goal](https://www.energy.gov/cmei/articles/battery500-progress-update)
- [DOE advanced-battery working document: 557 Wh/kg materials-level example](https://www.energy.gov/documents/17-advancedbatteriespdf)
- [Argonne BatPaC comparison of cell and pack specific energy](https://publications.anl.gov/anlpubs/2023/08/183571.pdf)
- [Oak Ridge/Nature research on the 1,922 Wh/kg iron-fluoride cathode reaction](https://impact.ornl.gov/en/publications/high-energy-density-and-reversibility-of-iron-fluoride-cathode-en)

## Six-month crew supplies, oxygen, and water

For comparability, six months is treated as **180 days**.

Modern nuclear submarines are a good operational analogy but not a stored-mass
analogy for air and water. NAVSEA states that submarines produce oxygen and
purified water from seawater and are limited primarily by food. The National
Museum of the U.S. Navy likewise describes onboard freshwater distillation.
A spacecraft must instead carry consumables or close its recycling loops.

### Food and personal consumables

NASA mission models use:

- food: **1.56 kg per crew-day**, including about 0.72 kg of water in food;
- an older Shuttle-derived allowance: **1.64 kg per crew-day**, including food,
  contained water, and packaging;
- wipes and towels: **0.195 kg per crew-day**;
- trash bags: **0.011 kg per crew-day**;
- health-care consumables: **0.090 kg per crew-day**;
- operational supplies: **20 kg per crew member** for a mission under one year.

Using the packaging-inclusive food allowance:

| Item | Per day | Per crew for 180 days |
|---|---:|---:|
| Food, contained water, and packaging | 1.64 kg | 295 kg |
| Wipes, towels, trash bags, and medical consumables | 0.296 kg | 53 kg |
| Operational supplies | — | 20 kg |
| **Subtotal** | — | **368 kg** |

Rounding to **0.4–0.45 t per crew member** allows a modest reserve and personal
items. It does not include ship spares, ammunition, spacesuits, or shared
maintenance equipment.

### Oxygen

NASA uses **0.82–0.84 kg O₂ per crew-day** for ordinary metabolism. A current
NASA human-systems brief gives **1.083 kg/day** for a 75 kg person when a daily
exercise session is included.

| Activity assumption | Per crew for 180 days |
|---|---:|
| 0.82 kg/day nominal | 148 kg O₂ |
| 0.84 kg/day nominal | 151 kg O₂ |
| 1.083 kg/day with exercise | 195 kg O₂ |

If oxygen is made by electrolyzing water, NASA uses **1.125 kg water per
kilogram of O₂**. Producing 148 kg of oxygen would therefore consume about
166 kg of water before any hydrogen/CO₂ recovery loop is credited.

### Water

NASA's basic exploration allowance is **2.9 kg per crew-day**:

- drinking: 2.0 kg/day;
- food rehydration: 0.5 kg/day;
- hygiene: 0.4 kg/day.

That totals **522 kg per crew member for 180 days** without recycling. A broader
life-support model supplies 5.0 kg/day when wash water and urine flush are
included, or **900 kg over 180 days**. NASA's public ISS shorthand of about one
gallon per day lies between these values at roughly **681 kg over 180 days**.

| Water model | Six-month gross use per crew |
|---|---:|
| Drinking, food rehydration, minimal hygiene | 522 kg |
| ISS one-gallon-per-day shorthand | 681 kg |
| Drinking, food preparation, wash, and flush | 900 kg |

These are gross flows, not necessarily stored makeup water. At 90% recovery,
the 522 kg minimal allowance needs roughly 52 kg of makeup water, plus losses,
reserve, and any water electrolyzed for oxygen. Recycling hardware and spare
filters then replace much of the stored-water mass.

### Combined mass cases

| Six-month case, per crew | Food and personal supplies | Oxygen | Water | Total |
|---|---:|---:|---:|---:|
| Naval-style onboard air/water generation | 0.40–0.45 t | generated | generated | **0.40–0.45 t**, plus machinery |
| Stored oxygen, 90% recovery of 522 kg water | 0.40–0.45 t | 0.15 t | about 0.05 t makeup | **0.60–0.65 t**, plus recycling machinery and reserve |
| Stored oxygen and minimal water, no recycling | 0.40–0.45 t | 0.15 t | 0.52 t | **1.07–1.12 t** |
| Stored oxygen and full 5 kg/day water, no recycling | 0.40–0.45 t | 0.15 t | 0.90 t | **1.45–1.50 t** |

### Planning conclusion

For a six-month patrol, **one tonne per crew member is a reasonable rounded
allowance only if it includes consumables and assumes limited water recycling
or deliberately austere water use**. With competent recycling, consumables can
fall near 0.6 t per crew member, but the recycling plant, tanks, atmosphere,
crew accommodations, and reserve still need to be charged somewhere.

Four tonnes per crew member is not supported by consumables alone. It can only
be defended if the game deliberately bundles a large share of pressure hull,
life-support machinery, radiation shelter, workstations, accommodations,
escape equipment, and long-duration spares into the crew-mass term.

Primary sources:

- [NAVSEA, submarines generate oxygen and purified water; food limits patrols](https://www.navsea.navy.mil/Portals/103/Documents/PSNSY_IMF/News%20Releases/2013%20Naval%20Nuclear%20Propulsion%20Program.pdf?ver=2017-03-02-113143-683)
- [U.S. Navy, modern submarine freshwater distillation](https://www.history.navy.mil/content/history/museums/nmusn/education/educational-resources/history-of-submarines/elements-of-submarine-operation.html)
- [NASA NTRS, crew metabolic food, water, oxygen, and CO₂ rates](https://ntrs.nasa.gov/api/citations/20150004543/downloads/20150004543.pdf?attachment=true)
- [NASA NTRS, deep-space logistics allowances](https://ntrs.nasa.gov/api/citations/20150003005/downloads/20150003005.pdf)
- [NASA NTRS, Shuttle food including packaging](https://ntrs.nasa.gov/api/citations/19860004609/downloads/19860004609.pdf)
- [NASA ECLSS technical brief: oxygen with exercise](https://www.nasa.gov/wp-content/uploads/2023/12/ochmo-tb-002-eclss.pdf?emrc=6763cba012405)
- [NASA, ISS water recovery and one-gallon-per-day gross need](https://www.nasa.gov/missions/station/iss-research/nasa-achieves-water-recovery-milestone-on-international-space-station/)
