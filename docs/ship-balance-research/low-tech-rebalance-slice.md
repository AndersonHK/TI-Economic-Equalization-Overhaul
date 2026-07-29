# Low-tech rebalance: first planning slice

Last reviewed: 2026-07-29  
Game data: Terra Invicta 1.0.49 installed templates and English localizations

> **Status:** research draft only. The [planning changelog](CHANGELOG.md) is
> authoritative where a later decision supersedes a value in this document.
> None of the balance values in these tables should be treated as approved for
> implementation unless the changelog marks it settled.

## Scope and method

This is a deliberately narrow first pass covering:

- Fuel Cells I–III
- Solid Core Fission Reactors I–V
- Compact Solid Core Fission Reactors I–V
- Apex, Meteor, Neutron, and Venture starting rockets
- Diana, Nerva, and Kiwi drives
- Lithium-Ion Battery
- Water and Heavy Water Heat Sinks
- 10-inch Cannon
- 30mm Autocannon
- Gunship, Escort, Corvette, Frigate, Monitor, and Destroyer hulls, consolidated
  in the
  [ship-type table](gunship-and-escort-hull-analysis.md)
- the runtime power-plant waste-heat calculation

Each table distinguishes three things:

- **Current** is the installed game value.
- **Evidence-led ideal** is the closest useful engineering anchor. It is not a
  prediction that a complete combat spacecraft can be built today.
- **Proposed** is the rounded gameplay value recommended for the mod. It is
  intentionally more generous than the evidence-led value, but substantially
  less generous than vanilla where vanilla is difficult to defend.

The English localization is treated as part of the specification. A familiar
name does not override what the description says the module actually is.

## What the localizations establish

| Game item | Identity established by the full localized description |
|---|---|
| Apex Solid Rocket | PBAN fuel with ammonium-perchlorate oxidizer |
| Meteor Liquid Rocket | refined kerosene and liquid oxygen |
| Neutron Liquid Rocket | hydrazine reacting with nitrogen tetroxide |
| Venture Liquid Rocket | cryogenic liquid hydrogen and oxygen |
| Fuel Cells I–III | alkaline hydrogen/oxygen fuel cells recharged by a solar array |
| Solid Core Fission I–V | adapted nuclear power plants operating around 2,500 °C |
| Lithium-Ion Battery | ions move through an electrolyte gel; lightweight but slow charging |
| Water Heat Sink | a tank of water heated while the ship cannot radiate |
| Heavy Water Heat Sink | explicitly a **larger water heat sink**, not a deuterium-water system |
| 10-inch Cannon | redesigned heavy naval artillery installed in a ship's nose |
| 30mm Autocannon | rapid-fire gun useful defensively or against unarmored targets |

The fuel-cell line is therefore best read as a regenerative fuel-cell energy
storage system plus its solar array. It is not a bare fuel-cell stack and it is
not consuming an unmodelled stream of expendable hydrogen and oxygen forever.

## Runtime interpretation: gross power, useful power, and heat

The installed runtime calculates a bare Gunship's useful system load as:

`(3 crew × 5 kW + tier-1 base load of 5 MW) × 1.10 = 5.5165 MW`

For Fuel Cell I, the displayed plant requirement is:

`5.5165 MW / 0.70 = 7.8807 MW gross input`

That agrees with the intended reading: roughly 8 MW is collected/generated,
5.52 MW reaches the ship systems, and the remainder is conversion loss.

Vanilla does not calculate that remainder correctly. It currently uses:

`vanilla heat = useful power × (1 - efficiency)`

For this example:

`5.5165 × 0.30 = 1.6550 MW`

Energy conservation requires input minus output:

`correct heat = useful power × (1 / efficiency - 1)`

`5.5165 × (1 / 0.70 - 1) = 2.3642 MW`

Vanilla understates the radiator-driving heat by `0.7093 MW`, or about `30%`
of the true loss. Put another way, the corrected radiator load is about `43%`
larger than vanilla's. The mod now contains a narrowly targeted Harmony prefix
for `TIPowerPlantTemplate.WasteHeat_GW` that applies the latter formula.
Open-cycle drive cooling currently retains vanilla's exemption. The
[drive/reactor pairing audit](drive-reactor-pairing-and-hull-geometry.md)
finds that large radiator relief is justified for direct nuclear-thermal
drives while firing, settles that the exemption must no longer be absolute,
and gives **1% of the drive-associated closed-cycle heat** as the current
research-led coefficient draft. No implementation change has yet been made.

There remains a broader modelling limitation: the fuel-cell templates have no
energy-capacity or eclipse-endurance field. Rebalancing mass, efficiency, and
maximum output can make the hardware less implausible, but cannot make the
regenerative storage cycle visible to the player.

## Starting rockets

The current chemical engines are unusually well anchored. Their thrust,
exhaust velocity, and mass closely reproduce the historical hardware named in
the templates. Only Apex is clearly underweight relative to the complete
Shuttle solid rocket booster it describes.

| Drive (×1) | Current thrust | Current exhaust velocity | Current mass | Real-world anchor | Evidence-led ideal | Planning disposition |
|---|---:|---:|---:|---|---|---|
| Apex | 14.82 MN | 2.60 km/s | 66 t | Shuttle SRB: about 14.68 MN and 89.1 t inert | 14.7 MN; 2.6 km/s; 90 t | **Deferred; retain current values** |
| Meteor | 15.48 MN | 2.98 km/s | 17 t | two F-1 engines: roughly 15.5 MN vacuum and 16.8 t | 15.5 MN; 3.0 km/s; 17 t | **Deferred; retain current values** |
| Neutron | 20.38 MN | 3.10 km/s | 13 t | twelve RD-253 engines: about 19.6 MN and 3.1 km/s effective exhaust velocity | 19.6 MN; 3.1 km/s; about 13 t | **Deferred; retain current values** |
| Venture | 9.28 MN | 4.44 km/s | 14 t | four RS-25 engines: about 9.12 MN vacuum and 14.1 t | 9.1 MN; 4.4 km/s; 14 t | **Deferred; retain current values** |

No changes to these four drives are planned in this slice. Apex's mass remains
an open realism question, but it is deferred until the wider propulsion
progression is reviewed.

Primary anchors:

- [NASA, Space Shuttle Solid Rocket Booster historical data](https://www.nasa.gov/wp-content/uploads/2023/04/sp-4012v7.pdf?emrc=3e369a)
- [NASA, Apollo spacecraft and launch-vehicle reference: F-1](https://www.nasa.gov/wp-content/uploads/static/history/alsj/csm_news_reference_h_missions.pdf)
- [NASA NTRS, RD-253 performance](https://ntrs.nasa.gov/api/citations/19940028594/downloads/19940028594.pdf)
- [NASA, RS-25 fact sheet](https://www.nasa.gov/wp-content/uploads/2025/04/sls-4963-sls-rs-25-engine-fact-sheet-508.pdf?emrc=bb1960)

## Regenerative fuel cells and solar arrays

NASA describes a regenerative fuel cell as a fuel cell, electrolyzer, reactant
processing and storage system that is recharged by an external photovoltaic
source. Demonstrated round-trip figures around 52% and a fundamental practical
limit below 60% make the game's 70–72% optimistic for the entire cycle.

The mass problem is more severe. NASA's current spacecraft solar-array survey
shows actual missions clustering around 30 W/kg and about 100 W/kg for a ROSA
product, with an empirical upper edge near 200 W/kg. Earlier deployable-array
concept studies targeted more than 500 W/kg, but that is the array alone. The
fuel cells, electrolyzer, tanks, pumps, power conditioning, deployment
structure, and radiators still have mass.

| Plant | Current cap | Current specific mass | Current efficiency | Evidence-led ideal | Proposed cap | Proposed specific mass | Proposed efficiency | Crew |
|---|---:|---:|---:|---|---:|---:|---:|---:|
| Fuel Cell I | 200 MW | 2.8 kg/kW | 70% | 25 MW; 30 kg/kW; 50% | **50 MW draft** | **2.8 kg/kW settled** | **63% settled** | **0** |
| Fuel Cell II | 800 MW | 0.45 kg/kW | 70% | 100 MW; 15 kg/kW; 55% | **200 MW draft** | **1.8 kg/kW settled** | **65% settled** | **0** |
| Fuel Cell III | 1,500 MW | 0.12 kg/kW | 72% | 250 MW; 10 kg/kW; 60% | **500 MW draft** | **0.48 kg/kW settled** | **67% settled** | **0** |

The settled specific masses correspond to approximately 357 W/kg, 556 W/kg,
and 2,083 W/kg before the regenerative storage cycle and solar array are
separated. The later tiers are deliberately progression-driven rather than
complete-system extrapolations.

### Effect on the bare Gunship

The plant scales to required gross power and has a one-tonne minimum; it does
not always weigh `cap × specific mass`.

| Plant | Current gross requirement | Current installed mass | Proposed gross requirement | Proposed installed mass | Proposed heat |
|---|---:|---:|---:|---:|---:|
| Fuel Cell I | 7.88 MW | 22.1 t | 8.76 MW | **24.52 t** | 3.24 MW |
| Fuel Cell II | 7.88 MW | 3.55 t | 8.49 MW | **15.28 t** | 2.97 MW |
| Fuel Cell III | 7.66 MW | 1.00 t floor | 8.23 MW | **3.95 t** | 2.72 MW |

This restores a reason to research later fuel cells without allowing the
starting power system to become effectively massless.

Primary anchors:

- [NASA TechPort, regenerative fuel-cell system and photovoltaic recharge](https://techport.nasa.gov/projects/116307)
- [NASA NTRS, 52% regenerative-fuel-cell demonstration and flight targets](https://ntrs.nasa.gov/citations/20070010455)
- [NASA Small Spacecraft State of the Art: power subsystems](https://www.nasa.gov/smallsat-institute/sst-soa/power-subsystems/)
- [NASA TechPort, Roll-Out Solar Array target](https://techport.nasa.gov/projects/8567)
- [NASA NTRS, Mega-ROSA 200–400 W/kg concept](https://ntrs.nasa.gov/citations/20130008777)
- [DOE fuel-cell stack targets, explicitly excluding storage and auxiliaries](https://www.energy.gov/cmei/fuels/doe-technical-targets-fuel-cell-systems-and-stacks-transportation-applications)

## Solid Core Fission Reactors I–V

The localization's 2,500 °C operating temperature is an extremely favorable
high-temperature premise, so the proposal allows better electrical conversion
than present low-temperature space reactors. It does not justify the installed
75–85% efficiency or gram-per-kilowatt mass.

NASA's older multimegawatt study estimated about 7–10 kg/kW for near-term
systems and called 5 kg/kW a reasonable advanced goal. Current kilowatt-class
space-fission studies are much heavier. NASA Brayton studies give roughly
20–34% system efficiency depending on radiator and cycle assumptions.

| Reactor | Current cap | Current kg/kW | Current efficiency | Current crew | Evidence-led ideal | Proposed cap | Proposed kg/kW | Proposed efficiency | Proposed crew |
|---|---:|---:|---:|---:|---|---:|---:|---:|---:|
| Solid I | 2 GW | 0.040 | 75.0% | 6 | 0.5 GW; 20 kg/kW; 25% | **0.5 GW draft** | **10 draft** | **70% settled** | **TBD** |
| Solid II | 6 GW | 0.034 | 77.5% | 6 | 2 GW; 15 kg/kW; 28% | **2 GW draft** | **8 draft** | **72.5% settled** | **TBD** |
| Solid III | 20 GW | 0.028 | 80.0% | 6 | 5 GW; 10 kg/kW; 30% | **5 GW draft** | **6 draft** | **75% settled** | **TBD** |
| Solid IV | 60 GW | 0.012 | 82.5% | 6 | 15 GW; 7.5 kg/kW; 35% | **15 GW draft** | **4 draft** | **77.5% settled** | **TBD** |
| Solid V | 125 GW | 0.008 | 85.0% | 6 | 40 GW; 5 kg/kW; 40% | **40 GW draft** | **3 draft** | **80% settled** | **TBD** |

Even the proposal is generous: Solid V is lighter and more efficient than the
aggressive NASA multimegawatt goal, while Solid I receives the favorable end of
near-term multimegawatt studies despite being entry-level technology.

For a bare Gunship's 5.5165 MW useful system load, the proposed reactor masses
would be approximately:

| Reactor | Gross requirement | Installed mass | Corrected waste heat |
|---|---:|---:|---:|
| Solid I | 7.88 MW | **79 t** | 2.36 MW |
| Solid II | 7.61 MW | **61 t** | 2.09 MW |
| Solid III | 7.36 MW | **44 t** | 1.84 MW |
| Solid IV | 7.12 MW | **28 t** | 1.60 MW |
| Solid V | 6.90 MW | **21 t** | 1.38 MW |

One module-level crew billet is retained as a maintenance, radiation-safety,
and damage-control burden—not as a person manually moving control rods. KRUSTY
demonstrated passive load following and temperature control, supporting
automated normal operation. A later, broader crew refactor should move this
billet into a shared engineering department rather than charging it per plant.

Primary anchors:

- [NASA NTRS, multimegawatt nuclear-power specific-mass study](https://ntrs.nasa.gov/citations/19910067849)
- [NASA NTRS, Brayton performance and mass sensitivity](https://ntrs.nasa.gov/api/citations/20220013600/downloads/ASCEND_22_Brayton_Performance_Mass_Sensitivity.pdf?attachment=true)
- [NASA NTRS, KRUSTY results and passive response](https://ntrs.nasa.gov/citations/20180007389)
- [NASA NTRS, KRUSTY reactor design and remote control](https://ntrs.nasa.gov/api/citations/20205009350/downloads/03-KRUSTY%20Reactor%20Design.pdf?attachment=true)

## Battery and heat sinks

### Lithium-Ion Battery

The current battery stores `12 GJ = 3.333 MWh`. At 11 tonnes that is
`303 Wh/kg`, higher than present complete flight packs. NASA's 2026 survey puts
commercial cells around 150–270 Wh/kg while cited spacecraft packs are
typically around 119–153 Wh/kg. A NASA battery workshop put state-of-the-art
packs near 150–170 Wh/kg.

| Field | Current | Evidence-led ideal | Proposed |
|---|---:|---:|---:|
| Capacity | 12 GJ | 12 GJ | **12 GJ** |
| Mass | 11 t | 25 t | **20 t** |
| Pack specific energy | 303 Wh/kg | 133 Wh/kg | **167 Wh/kg** |
| Recharge rate | 5 MW | 2 MW | **3 MW** |
| Full recharge time | 40 min | 100 min | **67 min** |
| Crew | 0 | 0 | **0** |

The proposal is at the optimistic end of current complete packs and preserves
the localization's claim that charging is slow.

Primary anchors:

- [NASA Small Spacecraft State of the Art: batteries](https://www.nasa.gov/smallsat-institute/sst-soa/power-subsystems/)
- [NASA NTRS battery workshop: pack-level specific energy](https://ntrs.nasa.gov/api/citations/20180001539/downloads/20180001539.pdf?attachment=true)

### Water and Heavy Water Heat Sinks

Both current heat sinks store `400 kJ/kg`. That is physically defensible for
water: melting ice absorbs about 334 kJ/kg, and warming the resulting water
adds about 4.186 kJ/kg per kelvin. The proposal allows `500 kJ/kg`, equivalent
to melting ice and warming the liquid by roughly 40 °C before tank and plumbing
mass. This is generous but still recognizable.

| Module | Current capacity | Current mass | Current crew | Evidence-led ideal | Proposed |
|---|---:|---:|---:|---|---|
| Water Heat Sink | 100 GJ | 250 t | 1 | 100 GJ; 225 t; 0 crew | **100 GJ; 200 t; 0 crew** |
| Heavy Water Heat Sink | 200 GJ | 500 t | 1 | 200 GJ; 450 t; 0 crew | **200 GJ; 400 t; 0 crew** |

No person needs to operate a tank continuously. Monitoring valves, pumps, and
leaks belongs to the ship's shared engineering complement. “Heavy Water” is a
size adjective here because the localization explicitly calls it a larger
water heat sink; assigning special deuterium properties would contradict the
game text.

Primary anchors:

- [NIST, specific heat of liquid water](https://nvlpubs.nist.gov/nistpubs/Legacy/IR/nistir6191.pdf)
- [USGS, latent heats of water](https://www.usgs.gov/water-science-school/science/sublimation-and-water-cycle)

## 10-inch Cannon

The closest historical scale anchor is naval artillery, but a space mount
needs a pressure enclosure or isolated machinery, recoil transfer, ammunition
handling, thermal control, fire control, and protection against vacuum and
temperature cycling. The U.S. Navy's 8-inch Mk 71 prototype is a useful
automation anchor: the complete prototype was about 78 tonnes, carried 75
rounds, achieved roughly 10–12 rounds per minute, and could be controlled by
one operator. Historical 10-inch naval projectiles were about 231 kg at roughly
0.82 km/s.

| Field | Current | Evidence-led ideal | Proposed |
|---|---:|---:|---:|
| Base mount mass | 125 t | 125 t | **110 t** |
| Module crew | 4 | 0 | **3** |
| Magazine | 300 rounds | 75 rounds | **120 rounds** |
| Ammo mass | 180 kg | 230 kg | **230 kg** |
| Effective warhead/projectile mass | 90 kg | 200 kg | **180 kg** |
| Muzzle velocity | 1.40 km/s | 0.85 km/s | **1.00 km/s** |
| Kinetic damage | 88.2 MJ | about 72 MJ | **90 MJ** |
| Salvo | 3 | sustained autoloading | **3** |
| Intra-salvo interval | 3 s | about 5–6 s | **4 s** |
| Cooldown | 16 s | about 6–10 rpm | **20 s** |
| Targeting range | 250 km | fire-control dependent | **250 km** |

The current loaded module is `125 t + 300 × 180 kg = 179 t`. The proposal is
`110 t + 120 × 230 kg = 137.6 t`. It keeps essentially the same per-hit kinetic
energy but trades the optimistic velocity and enormous magazine for a
full-calibre projectile and a still-generous ammunition supply.

The evidence-led module crew is zero because the hull's combat crew could
supervise automated fire control and an autoloader. The settled planning value
is deliberately more conservative: **three**, abstracted as one commander, one
shooter, and one loader.

Primary anchors:

- [Naval History and Heritage Command archive index: U.S. naval ordnance](https://maritime.org/doc/nara/ordnance.php)
- [U.S. Naval Institute, Mk 71 prototype characteristics](https://www.usni.org/magazines/proceedings/1975/december/professional-notes)

## 30mm Autocannon

The localization calls this a rapid-fire autocannon useful defensively or
against unarmored targets. Its template enables both attack and defense modes,
making it the first projectile point-defense mount in this slice.

| Field | Current | Evidence-led ideal | Proposed |
|---|---:|---:|---:|
| Base mount mass | 3 t | not reassessed in this slice | **3 t retained** |
| Magazine | 3,000 rounds | not reassessed | **3,000 retained** |
| Ammunition mass | 5.5 kg/round | not reassessed | **5.5 kg retained** |
| Loaded module mass | 19.5 t | not reassessed | **19.5 t retained** |
| Muzzle velocity | 1.35 km/s | not reassessed | **1.35 km/s retained** |
| Salvo | 10 shots, 0.5 s spacing | not reassessed | **retained** |
| Cooldown | 4 s | not reassessed | **4 s retained** |
| Module crew | 1 | 0 | **0 settled** |

The zero does not claim that ammunition, maintenance, barrel replacement, or
combat-system supervision require no people. It means a fast defensive mount
does not carry a dedicated operator billet. Those duties belong to shared
ordnance and combat-system crews, consistent with the broader [weapon
automation analysis](weapon-automation-and-crew.md).

## Early human hulls

The Gunship, Escort, Corvette, Frigate, Monitor, and Destroyer entries,
dimensions, structural integrity, authored visual bounds, ballistic colliders,
crew mass, and comparative mass analysis live in one authoritative [early
human hull analysis](gunship-and-escort-hull-analysis.md). They are
intentionally not repeated here.

## Consolidated proposed template values

These are the recommended rounded values for a later data patch.
`specificPower_tGW` is numerically `1,000 × kg/kW`.

| Template | Fields to change |
|---|---|
| ApexSolidRocketx1–x6 | **Deferred; retain current template values** |
| MeteorLiquidRocketx1–x6 | **Deferred; retain current template values** |
| NeutronLiquidRocketx1–x6 | **Deferred; retain current template values** |
| VentureLiquidRocketx1–x6 | **Deferred; retain current template values** |
| FuelCellI | settled: `specificPower_tGW: 2800`; `efficiency: 0.63`; draft cap: `maxOutput_GW: 0.05`; `crew: 0` |
| FuelCellII | settled: `specificPower_tGW: 1800`; `efficiency: 0.65`; draft cap: `maxOutput_GW: 0.20`; `crew: 0` |
| FuelCellIII | settled: `specificPower_tGW: 480`; `efficiency: 0.67`; draft cap: `maxOutput_GW: 0.50`; `crew: 0` |
| SolidCoreFissionReactorI | settled: `efficiency: 0.70`; draft: `maxOutput_GW: 0.5`; `specificPower_tGW: 10000`; crew TBD |
| SolidCoreFissionReactorII | settled: `efficiency: 0.725`; draft: `maxOutput_GW: 2`; `specificPower_tGW: 8000`; crew TBD |
| SolidCoreFissionReactorIII | settled: `efficiency: 0.75`; draft: `maxOutput_GW: 5`; `specificPower_tGW: 6000`; crew TBD |
| SolidCoreFissionReactorIV | settled: `efficiency: 0.775`; draft: `maxOutput_GW: 15`; `specificPower_tGW: 4000`; crew TBD |
| SolidCoreFissionReactorV | settled: `efficiency: 0.80`; draft: `maxOutput_GW: 40`; `specificPower_tGW: 3000`; crew TBD |
| Lithium-IonBattery | `energyCapacity_GJ: 12`; `rechargeRate_GJs: 0.003`; `mass_tons: 20`; `crew: 0` |
| WaterHeatSink | `heatCapacity_GJ: 100`; `mass_tons: 200`; `crew: 0` |
| HeavyWaterHeatSink | `heatCapacity_GJ: 200`; `mass_tons: 400`; `crew: 0` |
| 10-inchCannon | `baseWeaponMass_tons: 110`; `crew: 3`; `magazine: 120`; `ammoMass_kg: 230`; `warheadMass_kg: 180`; `muzzleVelocity_kps: 1.0`; `cooldown_s: 20`; `salvo_shots: 3`; `intraSalvoCooldown_s: 4`; `targetingRange_km: 250` |
| 30mmAutocannon | settled: `crew: 0`; retain all other current values pending a later weapon-performance review |
| Crew-mass runtime | settled: change `4 t × crew billets` to `3 t × crew billets` |
| Power-plant heat runtime | use `delivered × (1 / efficiency - 1)` |
| Open-cycle heat runtime | settled: no absolute exemption; research draft: retain `1%` of the drive-associated corrected closed-cycle heat |

The identifiers and field names above were checked against the installed
templates. The table is a balance specification, not yet a generated template
override.

## Remaining decisions before applying the data rebalance

1. Decide whether regenerative fuel cells need a new explicit stored-energy
   mechanic or whether mass and maximum output are enough for the first release.
2. Confirm that lowering reactor maximum output does not unintentionally break
   project unlocks or AI drive/reactor pairing.
3. Verify the 10-inch Cannon's exact salvo-cycle timing in combat.
4. Decide whether later automation technology should reduce the settled
   three-person cannon complement.
5. Check the settled three-tonne crew-mass change against every hull class;
   it is deliberately a ship-wide runtime change.
