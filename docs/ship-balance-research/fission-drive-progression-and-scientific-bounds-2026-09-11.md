# Fission drives: progression, changed tradeoffs, and scientific bounds

Date: 2026-09-11. **Research only; no gameplay changes or deployment.**

The current mod gives open-cycle fission propulsion **2.13–3.46 times as much drive-input power per tonne of propulsion-associated reactor** as a closed-cycle load on the same reactor. The strongest advantage occurs at Solid Core I. This confirms the direction of the suspected imbalance, although the current fission table does not quite reach 4×. It does **not** mean that open-cycle ships became 3.46× better than vanilla: most of their reactors also became heavier. Basic Nerva's propulsion-associated reactor mass is about **3.01× vanilla**, whereas Pulsar on Solid Core III is **9.6× vanilla**.

The scientific evidence argues for a selective reassessment. Ordinary solid-core cruise exhaust velocities are mostly well grounded. Their combat operating range is much harder to justify. Pulsar's performance requires a mechanism beyond ordinary solid-core hydrogen heating. Advanced gas-core exhaust velocities have historical concept-study precedents, but some of those precedents require the very radiator systems that the mod largely removes.

Three files make up this audit:

- This report explains the mechanics, comparisons, physical evidence, and tentative implications.
- The [complete data tables](fission-drive-data-tables-2026-09-11.md) cover **39 base drives, 24 reactors, drive-project prerequisites, reactor-project ladders, and all reactor coefficient deltas**.
- The [machine-readable snapshot](tables/fission-drive-audit-2026-09-11.json) preserves source hashes, merged values, computed operating points, and all compatible-class drive/reactor pairings.

## 1. Baseline and what the numbers mean

The baseline is the installed **Terra Invicta 1.0.53**, merged with EEO **0.9.7** source overrides. Installed drive and reactor override files match the repository byte for byte. Relevant installed ship-balance settings agree with the shipped defaults: corrected plant heat, open-cycle residual heat, the 0.01 retained fraction, thermal mass scaling, hull-drive scaling, reactor bays, and fuel volume limits are enabled. The entire settings file differs in other respects. The packaged mod DLL and deployed DLL match at SHA-256 `FE86DE4AD368D99B5C4432C26741D3A1F7A5889C3DC9FDE9D3E7A0CD494A569E`.

**None of the 39 fission drive templates has a changed thrust, exhaust velocity, drive efficiency, or combat multiplier.** The changes under discussion come from reactors, runtime accounting, hull scaling, and ship constraints. Older research documents contain different game snapshots and some unimplemented plans; their proposed coefficients are not used here.

Unless stated otherwise, tables use one thruster, hull-art multiplier **1**, no utility modifiers, no damage, and zero systems/weapon electrical load. Mass means the **drive-associated reactor contribution**, not total engine installation or ship mass. Plant heat excludes independent weapon/module heat. A failed capacity check is shown explicitly, even though its hypothetical mass can still be calculated. Values use the runtime power equation, not the rounded, sometimes stale JSON `req power` annotation.

Fresh disassembly of the installed assembly confirms `TIDriveTemplate.get_selfPowered`, `get_powerRequirement_GW`, `get_openCycleCooling`, `TISpaceShipTemplate.get_baseCombatThrust_N`, and `get_baseCombatExhaustVelocity_kps`. The mod's authoritative equations are in [PowerPlantThermalMath.cs](../../TIEconomyMod/Core/PowerPlantThermalMath.cs); consumers and hull scaling are in [BalancePatches.cs](../../TIEconomyMod/Patches/BalancePatches.cs) and [ShipPowerPatches.cs](../../TIEconomyMod/Patches/ShipPowerPatches.cs).

### Four distinctions that materially change the result

| Term | Meaning in this audit | Why it matters |
|---|---|---|
| Cruise exhaust velocity | `EV_kps`, the reaction-mass exhaust speed | It is neither spacecraft cruising speed nor transfer speed. Delta-v also depends on mass ratio. |
| Combat multiplier | `thrustCap = K`; thrust becomes `K F`, exhaust velocity becomes `v/K` | The game exchanges propellant economy for thrust at constant jet power. |
| Open-cycle cooling | Explicit `Open`, or `Calc` with single-thruster mass flow ≥3 kg/s for these non-pulsed thermal drives | Nerva and Pebble receive the discount despite their JSON saying `Calc`. |
| Self-powered drive | Fission-pulse and nuclear-salt-water classifications return **zero external drive power requirement** | Orion, Minimag, microfission, and Neutron Flux do not receive the reactor propulsion discount. Nonzero `req power` annotations on pulse drives are not runtime demand. |

All 30 `Fission_Thermal` entries depend on a selected reactor, including Fission Frag and Dusty Plasma. The seven `Fission_Pulse` and two `NuclearSaltWater` entries are self-powered. Their ship systems still need electrical power; zero external propulsion demand is not zero nuclear energy or zero engine hardware.

## 2. How the mod creates the relative advantage

Let `F` be cruise thrust in newtons, `v` cruise exhaust velocity in km/s, `ηd` drive efficiency, `η` plant efficiency, `b` plant specific-mass coefficient, `r=0.01`, and `s=0.5`.

```text
Pjet = F v / 2,000,000                     GW
D = Pjet / ηd                             GW of input accepted by drive
α = 1 - r(1 - η)

Open:    reactor contribution = Qoc = D/α
         mass contribution = b s D/α
         plant heat = D(1/α - 1)
         rated-cap use = s D/α

Closed:  reactor contribution = Qe = D/η
         mass contribution = b D/η
         plant heat = D(1/η - 1)
         rated-cap use = D

Same-input power-per-reactor-mass advantage, open/closed = α/(s η)
```

For auxiliary electrical demand `A`, add `b A/η` to either reactor mass before applying the one-tonne total plant floor. The common auxiliary contribution reduces the relative mass advantage. Also, input-power advantage is not exactly jet-power advantage when two drives have different `ηd`.

| Current reactor | η | Open t/GW of drive input | Closed t/GW of drive input | Open input-power/mass advantage | Maximum open input per unit of rated cap |
|---|---:|---:|---:|---:|---:|
| Solid Core I | 57.5% | 120.512 | 417.391 | **3.463×** | 1.9915× |
| Solid Core III | 62.5% | 84.316 | 268.800 | 3.188× | 1.9925× |
| Solid Core V | 67.5% | 24.078 | 71.111 | 2.953× | 1.9935× |
| Compact Solid Core V | 70% | 6.018 | 17.143 | 2.849× | 1.9940× |
| Molten Core II | 70.5% | 7.021 | 19.858 | 2.829× | 1.9941× |
| Vapor Core III | 89% | 3.504 | 7.865 | 2.245× | 1.9978× |
| Terawatt Gas Core III | 94% | 2.502 | 5.319 | 2.126× | 1.9988× |

The cap advantage is about **2×**, not 3–4×. Closed-cycle cap use counts delivered drive electricity, while its mass counts gross reactor input. These intentionally different denominators should not be merged into one “reactor output” comparison.

### Separate the accounting change from the reactor rebalance

| Comparison | Open cycle | Closed cycle | Interpretation |
|---|---|---|---|
| August demand model → mass-scaling model, holding reactor coefficients fixed | Mass `b D/α → 0.5 b D/α`, **−50%** | Mass `b D → b D/η`, **+73.9% at Solid Core I** | This accounting step alone strongly shifts relative balance. |
| Vanilla → current, Solid Core I, same powered drive input | Mass coefficient `40 → 120.512`, **+201.3%** | `40 → 417.391`, **+943.5%** | Both become heavier; closed cycle loses much more. |
| Vanilla → current, Solid Core III | Open **+201.1%** | Closed **+860%** | Explains why Pulsar feels especially punished in the regular solid-core branch. |
| Vanilla → current, Vapor Core III | Open **+40.2%** | Closed **+214.6%** | A late relative shift remains even at high electrical efficiency. |

The earlier model is documented in [open-cycle demand and heat](open-cycle-reactor-demand-and-heat.md) and the subsequent [mass-scaling implementation](open-cycle-reactor-mass-scaling-plan-2026-08-21.md). With those coefficients held fixed, the relative improvement from that particular transition is `2/η`; the current same-input open/closed ratio is `α/(sη)`. They differ slightly because the earlier open path already included `α`.

For propulsion-only heat, vanilla returns zero for open cooling and `D(1−ηvanilla)` for closed cooling, as confirmed in the installed `TIPowerPlantTemplate.WasteHeat_GW`. The pairing table therefore shows both the added open-cycle bleed and the corrected/increased closed-cycle heat. This is reactor-side heat accounting, not a complete physical engine heat budget.

The vanilla mass comparison uses its original `b D` drive-sizing convention. It is not a counterfactual in which today's corrected electrical accounting is retroactively applied to vanilla. This matters when interpreting the percentage deltas.

## 3. Technology progression and intended niches

“Intended” below means **inferred from localization, project dependencies, and numerical tradeoffs**. It is not a claim about an undocumented developer balancing target.

| Reactor technology or branch | Drive progression it enables | Original numerical/design niche | What the mod changes |
|---|---|---|---|
| Solid Core I | Nerva → Advanced Nerva; Nerva → Cermet | Relatively low exhaust velocity; increased thrust by building a larger thermal engine | Open-cycle discount preserves that niche, but high combat multipliers make it unusually powerful against electrical-accounted competitors. |
| Solid Core II–V | Dumbo → Heavy Dumbo; Cermet + Dumbo → Advanced Cermet; Pulsar at III → Advanced Pulsar at V | Heavy Dumbo pursues thrust. Pulsar pursues cruise and combat propellant economy | Pulsar pays both lower electrical efficiency and full conversion-associated reactor mass/heat. |
| Compact Solid Core I–V | Kiwi → Snare → Rover → Pebble → Advanced Pebble; Fission Frag after Compact II | Low installed mass and tighter output caps; fragment drive trades thrust for enormous EV | The compact branch is important for both open and closed drives. Open drives also consume roughly half the rated cap. |
| Molten Core I–III | Lars; Teardrop; Lars → Spinner → Pegasus | About twice solid-core EV; move from small engines toward very high thrust | Lars is closed in game; its neighbors are open. Thus a cooling flag now creates a substantial mass and heat discontinuity within liquid-core propulsion. |
| Molten Salt I–II | No dedicated drive family | Flexible reactor alternative for solid- and liquid-core drives | Compatibility allows both required classes. It can rescue an otherwise unattractive pairing; it must not be omitted from fleet comparisons. |
| Vapor Core I–III | Cavity → Advanced Cavity; Vortex → Advanced Vortex | Retained-fuel cavity versus exhaust-cooled vortex tradeoff | Vortex's mass and radiator advantage grows markedly; Cavity still buys higher EV. |
| Gas Core I–III | Quartz → Lightbulb; Pharos at III; Burner at II; Dusty Plasma after Gas I plus fragment/nozzle research | Thermal containment, high-EV seeded gas, and direct-fragment branches coexist | Physical mechanisms differ much more than their shared reactor class suggests. Research eligibility alone does not guarantee capacity. |
| Terawatt Gas Core I–III | Lodestar / Flare / Firestar respectively | Expensive, uncertain high-power branches | Lodestar remains closed; Flare and Firestar receive the direct-thermal discount. Their reactor coefficients also change independently. |
| Fission Pulse Drives and auxiliary sciences | Orion, microfission → Minimag; Advanced Orion with Heavy Pulsed Propulsion | Propulsion energy carried in pulse units; different structural and fuel economics | No direct reactor propulsion-mass benefit. Electrical systems, ship mass, and hull changes still affect them. |
| Advanced Fission Systems | Neutron Flux Lantern → Torch with Magnetic Nozzles | Speculative self-powered high-thrust/high-EV branch | Also outside the open-versus-closed reactor mass comparison. |

The complete appendix shows every project's immediate prerequisites and base RP cost. Compact I–V have internal IDs `SolidCoreFissionReactorVI–X`; Terawatt Gas Core I–III have IDs `GasCoreFissionReactorIV–VI`. Confusing those names can make the tree look like a single ten-tier solid-core ladder.

Drive unlocks are not guaranteed on completing the science: the authored maximum unlock chances include **25% for Advanced Pulsar, 35% for Advanced Pebble, 20% for Pegasus, and 10% for Burner/Flare/Firestar**. These are template probability controls, not a prediction for a given campaign after faction modifiers. The appendix labels RP as **base template cost**; EEO's enabled project-cost multiplier is 1.4, and global research uses 2.2, before considering the game's other research mechanics. A higher-performance entry is not necessarily a guaranteed replacement.

Alternative prerequisites matter: Pulsar accepts Particle Beams instead of Arc Lasers; Pegasus accepts Teardrop instead of Spinner; Advanced Cavity accepts Gas Core II instead of Vapor Core III. Other prerequisites remain. The appendix's reactor-frontier summary follows the primary route; its explicit alternative column preserves these exceptions.

### Progression deltas between drives

These are changes **between the named drives**, not mod-versus-vanilla changes. All drive operating points themselves remain vanilla. Comparisons include related technology branches as well as direct project upgrades; the prerequisite appendix distinguishes them.

| Progression comparison | Δ cruise thrust | Δ cruise EV | Δ combat thrust | Δ combat EV | Δ drive input |
|---|---|---|---|---|---|
| Nerva → Advanced Nerva | +581.8% | +0% | +581.8% | +0% | +581.8% |
| Nerva → Cermet Nerva | +174.3% | +21.3% | +265.7% | -9.1% | +232.6% |
| Cermet Nerva → Advanced Cermet Nerva | +231.3% | -7% | +231.3% | -7% | +187.5% |
| Kiwi → Snare | +121.2% | +0.7% | +121.2% | +0.7% | +122.7% |
| Snare → Rover | +235.6% | -7.4% | +235.6% | -7.4% | +210.9% |
| Rover → Pebble | -29.5% | +19.9% | +5.7% | -20% | -37% |
| Pebble → Advanced Pebble | +93.2% | +0% | +106.1% | -6.2% | +93.2% |
| Pulsar → Advanced Pulsar | +33.3% | +100% | +33.3% | +100% | +151.9% |
| Lars → Fission Spinner | +451% | -9.8% | +414.3% | -3.3% | +397.1% |
| Fission Spinner → Pegasus | +1,196.3% | -9.6% | +1,196.3% | -9.6% | +1,071.8% |
| Vortex → Advanced Vortex | +0% | +24.8% | +0% | +24.8% | +24.8% |
| Cavity → Advanced Cavity | +485.1% | +51.4% | +524.1% | +41.9% | +785.7% |
| Quartz → Lightbulb | +247.5% | +11.5% | +334.4% | -10.8% | +287.4% |
| Lightbulb → Pharos | +117.6% | +25% | +132.1% | +17.2% | +172% |
| Pharos → Lodestar Fission Lantern | +1,136% | +23.1% | +1,444.9% | -1.5% | +1,298.5% |
| Burner → Flare | +3,140.7% | -49.3% | +2,600.6% | -39.1% | +1,543.9% |
| Flare → Firestar Fission Lantern | +42.9% | +42.9% | +57.1% | +29.9% | +104.1% |


Two telling examples: Advanced Vortex has **no thrust improvement** over Vortex; it spends 24.8% more power for 24.8% more EV. Advanced Nerva increases thrust and required power by the same factor, so at a common reactor it does not improve thrust per reactor mass. Its value is packaging more thrust into an engine slot/cluster and overcoming fixed ship mass. Conversely, Pebble improves drive efficiency as well as EV and combat multiplication, producing a more substantial qualitative step.

### Representative reactor pairings and vanilla deltas

These are deliberately explicit pairings, **not “best available reactor” claims**. They expose how reactor choice changes the comparison. All 30 externally powered drives have their complete class-compatible pairing results in the JSON snapshot.

| Drive | Reactor | Drive-associated reactor mass vanilla → EEO t | Δ mass | Plant heat vanilla → EEO MW | EEO cap use / cap GW | Combat kN/reactor t |
|---|---|---|---|---|---|---|
| Nerva Drive | Solid Core I | 11.33 → 34.12 | 201.3% | 0 → 1.21 | 0.142 / 2 | 12.92 |
| Advanced Nerva Drive | Solid Core I | 77.22 → 232.64 | 201.3% | 0 → 8.24 | 0.969 / 2 | 12.92 |
| Dumbo | Solid Core II | 77.85 → 234.48 | 201.2% | 0 → 9.2 | 1.149 / 3 | 15.35 |
| Pulsar Drive | Solid Core III | 47.44 → 455.38 | 860% | 338.82 → 1,016.47 | 1.694 / 10 | 1.98 |
| Pulsar Drive | Compact Solid Core III | 6.78 → 62.55 | 823.1% | 296.47 → 912.22 | 1.694 / 4 | 14.39 |
| Advanced Pulsar Drive | Solid Core V | 34.13 → 303.41 | 788.9% | 640 → 2,054.32 | 4.267 / 60 | 3.96 |
| Advanced Pulsar Drive | Compact Solid Core V | 8.53 → 73.14 | 757.1% | 533.33 → 1,828.57 | 4.267 / 10 | 16.41 |
| Rover Drive | Compact Solid Core III | 5.73 → 17.24 | 201.1% | 0 → 5.03 | 0.718 / 4 | 142.13 |
| Advanced Pebble Drive | Compact Solid Core V | 3.48 → 10.48 | 200.9% | 0 → 5.24 | 0.873 / 10 | 509.51 |
| Lars Drive | Molten Core II | 3.96 → 22.46 | 467.4% | 135.72 → 473.27 | 1.131 / 17 | 65.45 |
| Teardrop Drive | Molten Core II | 15.24 → 30.58 | 100.6% | 0 → 12.89 | 2.184 / 17 | 163.34 |
| Fission Spinner Drive | Molten Core II | 19.68 → 39.47 | 100.6% | 0 → 16.64 | 2.819 / 17 | 191.52 |
| Advanced Cavity Drive | Vapor Core III | 14.99 → 47.15 | 214.6% | 479.55 → 740.88 | 5.994 / 60 | 111.99 |
| Advanced Vortex Drive | Vapor Core III | 22.03 → 30.88 | 40.2% | 0 → 9.7 | 4.411 / 60 | 244.83 |
| Lightbulb Drive | Gas Core I | 39.26 → 112.83 | 187.4% | 490.8 → 733.38 | 4.908 / 8 | 54.38 |
| Dusty Plasma Drive | Gas Core I | 179.48 → 224.64 | 25.2% | 0 → 29.2 | 11.232 / 8 **FAIL** | 3.92 |
| Dusty Plasma Drive | Gas Core II | 112.17 → 179.68 | 60.2% | 0 → 24.71 | 11.23 / 33 | 4.9 |
| Firestar Fission Lantern | Terawatt Gas Core III | 147.06 → 367.87 | 150.2% | 0 → 88.29 | 73.574 / 1,700 | 299.02 |


The final column is an engine-system screening metric, not ship acceleration. It excludes hull, propellant, crew, radiator, weapons, and other equipment. In particular, Advanced Pebble's 509.5 kN per reactor tonne should not be interpreted as a ship capable of 52 g.

Concrete changes to the competitive landscape:

- **Pulsar remains much more propellant-efficient in combat.** Nerva's combat EV is 0.899 km/s versus Pulsar's 3.2 km/s. On the same Solid Core III, Nerva's combat thrust per reactor mass was about 2.93× Pulsar's in vanilla and is about **9.35×** now. Its endurance penalty survives that shift.
- **Compact reactors soften, but do not remove, the electrical penalty.** Pulsar on Compact III needs 62.55 t of reactor instead of 455.38 t on regular III. Those plants are reached through different projects and have different caps. Comparing only numbered regular reactors misses this option.
- **Lars changes role relative to Teardrop/Spinner.** At Molten II, Lars remains the smallest absolute reactor installation here, but Spinner provides about 2.93× its combat thrust per reactor tonne. The shared liquid-core class does not prevent the open/closed discontinuity.
- **Advanced Cavity versus Advanced Vortex changes from near parity to a marked Vortex advantage.** On Vapor III, vanilla combat thrust/reactor mass is approximately 352 versus 343 kN/t; EEO gives 112 versus 245 kN/t. Cavity retains 30.88 versus 24.48 km/s cruise EV and 1.93 versus 1.632 km/s combat EV.
- **Dusty Plasma is researchable before its nominal Gas I pairing fits at scale 1.** Its discounted cap charge is 11.23 GW, above Gas I's 8 GW. Gas II fits. Other compatible gas/vapor reactors or a sufficiently small hull-art scale can change that result.

### Whole ships retain additional tradeoffs

Hull scaling multiplies thrust, power demand, and drive hardware mass by the installed appearance's factor. It does not multiply EV or `thrustCap`. In a linear, propulsion-dominated comparison, thrust/reactor-mass ratios therefore survive common scaling; fixed ship mass, reactor floors, different nozzle-art families, and bay caps break that simplification. These tables should not be substituted for a matched-hull designer test.

The mod's [fuel-volume limit](fuel-volume-capacity-implementation.md) gives 100 t of hydrogen roughly **1,411 m³** of storage, versus about **100 m³** for water-equivalent `ReactionProducts`. This favors dense-propellant fragment and pulse branches in hull-volume-limited builds. The density is a gameplay representation; fuel cost and actual composition must still be compared separately. Reactor crew also contributes to the mod's 3 t per crew support allowance and fuel-space allocation.

At matched non-propulsion mass `Mfixed`, an unchanged drive's acceleration changes by `(Mfixed + Mold)/(Mfixed + Mnew)`, using like-for-like wet or dry mass consistently. It does not change by the isolated reactor-mass ratio unless that reactor dominates the total.

## 4. Combat thrust: energetically consistent, mechanically optimistic

From the runtime equations:

```text
Fcombat = K Fcruise
vcombat = vcruise / K
Pjet,combat = Pjet,cruise
mass flow = F/v
combat/cruise mass-flow ratio = K²
```

There is no free multiplication of jet energy here. Instead the game assumes a very broad constant-power operating range with no extra feed/nozzle hardware cost and no operating-point-dependent drive efficiency. A nuclear-thermal engine needs flow area, pressure head, heat transfer, turbopump capacity, and a suitable nozzle to realize those endpoints. Energy conservation is necessary but does not establish feasibility.

| Drive | Combat multiplier | Cruise → combat kg/s | Flow × | 100 t endurance cruise → combat seconds | Combat liquid-H₂ feed m³/s |
|---|---|---|---|---|---|
| Nerva Drive | 9 | 6.06 → 490.61 | 81 | 16,510.2 → 203.8 | 6.92 |
| Heavy Dumbo | 9 | 432.63 → 35,043.26 | 81 | 231.1 → 2.9 | 494.61 |
| Advanced Pebble Drive | 16 | 34.01 → 8,706.01 | 256 | 2,940.5 → 11.5 | 122.88 |
| Pulsar Drive | 5 | 11.25 → 281.25 | 25 | 8,888.9 → 355.6 | 3.97 |
| Pegasus Drive | 14 | 437.5 → 85,750 | 196 | 228.6 → 1.2 | 1,210.3 |
| Lightbulb Drive | 15 | 20.05 → 4,511.03 | 225 | 4,987.8 → 22.2 | 63.67 |
| Firestar Fission Lantern | 22 | 100 → 48,400 | 484 | 1,000 → 2.1 | 683.13 |
| Dusty Plasma Drive | 160 | 0 → 37.57 | 25,600 | 68,132,267.4 → 2,661.4 | Different propellant |


Endurance is for exactly 100 t of propellant at the unmodified operating point; it ignores reserve, RCS use, startup, shutdown, and acceleration caps. Liquid-hydrogen feed volume uses the mod's 70.85 kg/m³ storage density, not exhaust volume. For example, Nerva consumes one 100 t tank in approximately **204 seconds** at combat thrust, while Advanced Pebble consumes it in only **11.5 seconds**.

For comparison with physical scale, the historical Phoebus-2A test reached about **4.1 GWth, 930 kN, and 120 kg/s of hydrogen**, with a reported minimum reactor specific mass around **2.3 t/GWth**. This is a large ground-test reactor boundary, not a complete reusable spacecraft installation. The game's basic Nerva combat flow is about 491 kg/s despite only 0.283 GW of drive input. That comparison does not prove impossibility: its exhaust is much colder/slower. It does expose the feed-system assumption. [NASA, *Nuclear Propulsion—A Vital Technology for the Exploration of Mars and the Planets*](https://ntrs.nasa.gov/api/citations/19890001573/downloads/19890001573.pdf).

The constant-specific-heat idealization gives `v ∝ sqrt(T)` at unchanged molecular weight and expansion assumptions. A 9× EV reduction corresponds to approximately an **81× reduction in available specific thermal energy**, not an ordinary full-temperature NERVA throttle point. Do not extrapolate that relation into an exact cryogenic nozzle temperature: hydrogen phase changes, heat capacities, dissociation, pressure expansion, and feed enthalpy invalidate such a simple temperature calculation.

There is a real high-thrust analogue: **LOX-augmented NTR (LANTR)** adds oxygen downstream of a hydrogen nuclear-thermal engine. A NASA design study gives about **2.75× thrust with roughly 30% lower Isp at oxygen/hydrogen mixture ratio 3**. It adds both mass flow and chemical energy; it does not establish a generic 9–24× constant-power multiplier for a single-propellant NTR. [Borowski et al., NASA TM-106726](https://ntrs.nasa.gov/api/citations/19950005290/downloads/19950005290.pdf?attachment=true).

High multiplication is more conceptually natural for a **mass-loaded fragment beam** than for a solid-core heat exchanger. NASA FFRE studies explicitly explore trading beam EV for thrust by adding neutral gas. That supports variable operating points in principle, while leaving the game Dusty Plasma's specific **160×** multiplier and 25,600× flow range unvalidated. [NASA, *Studies of Fission Fragment Rocket Engine Propelled Spacecraft*](https://ntrs.nasa.gov/citations/20150002578).

## 5. Scientific grounding and conditional upper bounds

The bounds below are **architecture-specific screening envelopes**, not immutable physical ceilings or predictions of what will be built. “Conservative upper edge” means a point close to demonstrated conditions or an explicitly identified engineering reference. “Optimistic edge” means an advanced fuel target or a concept-study point whose mass, lifetime, and cooling assumptions must accompany it.

`EV [km/s] = Isp [s] × 0.00980665`. Exhaust velocity alone cannot validate thrust, engine mass, or durability.

| Architecture and game parallels | Conservative upper edge/reference | Optimistic edge or envelope | Evidence and implication |
|---|---|---|---|
| Solid-core hydrogen: Nerva, Kiwi, Snare, Rover, Dumbo | Roughly **850–900 s = 8.34–8.83 km/s** | Advanced particle/fuel concepts about **950–1,050 s = 9.32–10.30 km/s** | Ground-tested solid-core technology; upper range is a design study. Most game cruise EVs fit. [NASA small NTR study](https://ntrs.nasa.gov/citations/20160014802), [NASA particle-bed Mars study](https://ntrs.nasa.gov/citations/19920055983). |
| Cermet / Pebble / Advanced Pebble | Around **900 s = 8.83 km/s** as a conservative development reference | Approximately **1,000–1,050 s = 9.81–10.30 km/s** with advanced fuel/heat transfer | Their 9.12–9.81 km/s is an ambitious but recognizable target; this does not validate 12–16× combat multiplication. [NASA particle-bed study](https://ntrs.nasa.gov/citations/19920055983). |
| Pulsar / Advanced Pulsar, if interpreted as ordinary solid-core thermal engines | Same solid-core thermal bound | **No identified ordinary solid-core basis for 16–32 km/s**; a separate nonthermal mechanism needs its own model | Superheating in pulses does not by itself remove fuel/material limits. These correspond to about **1,632/3,263 s**. The localization and laser/particle-beam/magnet prerequisites suggest a more exotic hybrid, but do not specify enough engineering to validate it. |
| Liquid/droplet/centrifugal core: Lars, Teardrop, Spinner, Pegasus | Historical less ambitious liquid-core band **1,200–1,500 s = 11.77–14.71 km/s** | LARS concept **>2,000 s, ≳19.61 km/s**, at about 6,000 K | Game 16–19.62 km/s is in optimistic liquid-core territory, not automatically impossible. Older work highlights fuel entrainment/loss; the LARS result is conceptual. [Rom, NASA TM-X-1685](https://ntrs.nasa.gov/citations/19690000736), [NASA LARS concept](https://ntrs.nasa.gov/citations/19910012832). |
| Retained-fuel gas-core lightbulb: Quartz, Cavity, Lightbulb, Pharos, Lodestar | Reference lightbulb **1,870 s = 18.34 km/s**, about **409 kN, 4.6 GWth, 31.75 t engine** | Treat higher **25–31 km/s** game values as additional speculation; no integrated validated upper bound was found | The reference already requires extreme containment, transparent-wall cooling and radiation transport. Quartz closely matches EV; Lightbulb matches thrust but has **11.2% greater EV**; Pharos/Lodestar go further. [NASA reference lightbulb design](https://ntrs.nasa.gov/api/citations/19710000930/downloads/19710000930.pdf). |
| Exhausting gas-core: Vortex, Burner, Flare, Firestar | Regenerative-only historical screening edge around **3,000 s = 29.42 km/s** | Radiator-assisted studies reach roughly **5,000–7,000 s = 49.03–68.65 km/s** | Vortex's 19.62–24.48 km/s fits the lower regime. Burner at 69 and Firestar at 50 km/s need optimistic gas-core assumptions and substantial heat rejection, not just fuel venting. [NASA gas-core reassessment](https://ntrs.nasa.gov/api/citations/19710014431/downloads/19710014431.pdf), [1970 project status](https://ntrs.nasa.gov/api/citations/19710010820/downloads/19710010820.pdf). |
| Direct / mass-loaded fission fragments: Fission Frag, Dusty Plasma | No ground-demonstrated complete FFRE bound. Mass-loaded study point **32,000 s ≈313.81 km/s** | Direct-beam study point **527,000 s ≈5,168 km/s** | Fission Frag at 313.9 km/s and 4.651 kN closely resembles the mass-loaded study point. Dusty Plasma's 3,750 km/s is conceptually in the direct-beam range, but its thrust, radiators and containment must be checked separately. [NASA FFRE study](https://ntrs.nasa.gov/citations/20150002578), [authors' 2023 follow-up](https://doi.org/10.3389/frspt.2023.1191300). |
| External nuclear pulse and salt water | No integrated demonstrated propulsion envelope | Retain as separate concept-study/speculative branches; no universal numerical upper bound claimed here | Explosion energy is not a validation of pusher/magnetic-nozzle lifetime, pulse repetition, or continuous-flow stability. They do not share the thermal-reactor mass discount. [General Atomic pulse-vehicle study](https://ntrs.nasa.gov/api/citations/19760065935/downloads/19760065935.pdf), [Ewig & Andrews, Mini-Mag Orion](https://doi.org/10.2514/6.2003-4525), [Zubrin, NSWR original publication](https://bis-space.com/shop/product/nuclear-salt-water-rockets-high-thrust-at-10000-sec-isp/). |

The solid-core historical evidence is stronger than the advanced-concept evidence. The National Academies documents the Rover/NERVA ground campaign, Pewee's high-temperature operation, fuel/coating degradation, restart behavior, and the challenge of rapid transients. Engine/propellant subsystems are integral to that assessment. Those results support the **8–10 km/s scale**, not a massless, indefinitely reusable thermal engine with unrestricted flow. [National Academies, *Space Nuclear Propulsion for Human Mars Exploration*, chapter 2](https://www.nationalacademies.org/read/25977/chapter/4).

### The two different meanings of “closed cycle”

In a **nuclear lightbulb**, “closed cycle” principally describes retaining the nuclear fuel behind a transparent barrier. Radiation still directly heats hydrogen that leaves the spacecraft. It does **not** mean converting the drive's entire power through a turbine/generator and then an electric thruster. Lars likewise describes direct heating by liquid nuclear fuel.

In the current mod, `openCycleCooling == false` routes all drive input through the electrical accounting path. This charges the lightbulb/Lars families a conversion efficiency that is not necessarily part of their real energy path. A real lightbulb can nevertheless need large radiators: NASA's radiative-transfer analysis explicitly finds transparent-wall UV absorption and buffer-gas heat loads that require space heat rejection. **Fuel retention, power conversion, and heat rejection are three separate design choices.** [Rodgers, Latham & Krascella, NASA lightbulb heat-transfer study](https://ntrs.nasa.gov/citations/19720027679).

Thus, the open-cycle advantage is physically motivated relative to **nuclear electric propulsion**, but its application to every closed-cooling thermal concept is a modeling shortcut. Correcting the electrical denominator was useful; making the cooling flag determine the energy pathway creates a new conflation. This report identifies that issue without changing it.

### High gas-core EV is conditional on heat rejection

The 1971 NASA gas-core study distinguishes the roughly 3,000 s regenerative-only regime from much higher radiator-assisted performance; its abstract gives radiator-assisted points around 2,500–6,500 s at 20–400 kN. Those figures are concept estimates, not test results. [Ragsdale & Willis](https://ntrs.nasa.gov/api/citations/19710014431/downloads/19710014431.pdf).

Consequently, Burner at 69 km/s is close to the extreme historical **7,000 s** envelope, not evidence that a nearly radiator-free drive is feasible. Firestar at 50 km/s is in the radiator-assisted band and adds **5 MN of cruise thrust**. Flare at 35 km/s also lies above that particular study's regenerative-only screening edge. These are conditional comparisons to specified architectures, not universal prohibitions on later inventions.

The mod retains only `r(1−η)` of gross open-cycle propulsion reactor output: **0.425% at Solid Core I, 0.11% at Gas Core II, and 0.06% at Terawatt Gas Core III**. This fraction is not a measured consequence of gas-core opacity, neutron deposition, wall loading, fragment escape, or shielding geometry. High electrical efficiency automatically makes direct-drive heat smaller even when the real mechanism would not have that relationship.

For Firestar, this model produces only **88.3 MW** of propulsion-associated plant heat from about 147 GW drive input. It also has `ηd=0.85`; how the remaining input energy is divided among uncollimated exhaust, radiation escaping to space, and heat deposited in the vehicle is a separate question. It would be incorrect simply to assign all non-jet energy to radiators, but equally incorrect to assume it is all harmlessly expelled.

The same issue applies to fragment drives. Their `ηd=0.46` means a large non-jet energy channel, while the selected gas reactor can reduce retained plant heat to a tiny fraction of power. NASA's FFRE spacecraft work explicitly includes extensive heat-rejection hardware. High fragment velocity is not a license for negligible onboard heat. [NASA FFRE study](https://ntrs.nasa.gov/citations/20150002578).

### Engine specific mass: distinguish the denominator and hardware boundary

| Comparison | Value | What can be concluded |
|---|---:|---|
| Historical Phoebus reactor lower anchor | About **2.3 t/GWth** at about 4.1 GWth | A large propulsion reactor can be far lighter per thermal GW than an electric plant per electrical GW. Nozzle, feed, shielding, lifetime and small-size floors still matter. |
| EEO Solid Core I open input coefficient | **120.5 t/GW** | Basic regular solid-core is still very heavy against that historical large-reactor boundary; its relative buff does not make the coefficient unrealistically light. |
| EEO Compact Solid Core V open input coefficient | **6.018 t/GW** | Much closer to thermal-engine scales, but extrapolating linearly down to a few tonnes needs a fixed-hardware and criticality check. |
| Historical lightbulb concept, complete engine reference | **31.75 t / 4.6 GWth ≈6.90 t/GWth** | A concept estimate with a different boundary from the game's drive-associated plant mass. At 409 kN, engine T/W is about **1.31**. |
| EEO Lightbulb + Gas Core I | **112.8 t reactor contribution** for 409 kN cruise | Game reactor alone gives T/W ≈**0.37** cruise; combat multiplication raises it to ≈**5.55** while EV falls to 1.36 km/s. Cruise and combat endpoints must not be mixed when comparing to the historical engine. |

Historical thermal numbers above come from [NASA's nuclear-propulsion review](https://ntrs.nasa.gov/api/citations/19890001573/downloads/19890001573.pdf) and [the lightbulb reference](https://ntrs.nasa.gov/api/citations/19710000930/downloads/19710000930.pdf). Specific mass includes different hardware in different sources; none gives a universal future minimum for every size and endurance.

One citation correction to the existing [NERVA mass report](details/reactors/nerva-thermal-specific-mass-2026-08-21.md): NTRS **20060051740** is titled *Turbopump Design and Analysis Approach for Nuclear Thermal Rockets*, not *ROVER/NERVA Program Achievements*. The 4.1 GW / 2.3 kg/MW result is independently supported by **NASA TM-101354, NTRS 19890001573**, cited here. This does not overturn the numerical anchor, but its source should be identified correctly.

## 6. What the evidence supports before changing values

| Finding | Confidence | Balance implication |
|---|---|---|
| Open-cycle reactor input-power/mass advantage is 2.13–3.46× with current fission coefficients | High: source equations and current templates | Preserve explicit relative-versus-vanilla comparisons; early electrical drives need particular attention. |
| Most ordinary solid-core cruise EV is recognizable nuclear-thermal performance | High for historical scale; moderate for advanced fuels | A blanket reduction of all fission cruise EV is not scientifically indicated. |
| Combat multiplication conserves jet power but assumes a very large flow range | High for game behavior; no validation for the mechanical endpoints | Multiplier reductions are more defensible as an engineering calibration than as an energy-conservation fix. |
| Pulsar's 16–32 km/s needs more than ordinary solid-core heating | High for thermal interpretation; mechanism uncertain | Define the pulse/beam mechanism before choosing its final EV bound or conversion pathway. |
| Retained-fuel thermal drives are not necessarily electric drives | High | Lars/lightbulb penalties deserve an accounting review, independently of their EV. |
| Highest gas-core EV requires architecture-specific radiator and containment assumptions | High that these constraints exist; uncertain future performance | Do not retain optimistic EV while automatically discarding the associated heat-rejection burden. |
| Fragment drives can trade very high EV for thrust through mass loading | Supported by concept studies | A different combat envelope from NERVA can be justified, but the exact multiplier and heat/mass budget remain speculative. |

No numerical changes below are implemented. The primary outcome of this audit is the evidence and comparison set above.

### Verification and source-access limits

The calculations were checked for complete coverage, unique identifiers, unchanged drive operating fields, class compatibility (including molten salt), power and combat-flow identities, mass/heat equations, capacity flags, table structure, and local links. Selected vanilla methods were freshly disassembled from the installed 1.0.53 assembly. This is static research verification, not an in-game validation of a ship build.

Scientific claims use primary NASA/contractor studies, an original FFRE follow-up paper, and the National Academies assessment. Several NTRS direct downloads returned HTTP 403; for those, the consulted evidence was the NTRS abstract and search-indexed passages of the primary report, not a complete reread of every page. The NSWR publisher page establishes the original publication; its paywalled full text was not reviewed, so no detailed NSWR engineering upper bound is asserted. Dates refer to the studies, not recent crawl/upload dates.

## Addendum: tentative adjustment directions

These are **sensitivity examples, not a recommended final patch**. A first pass could keep ordinary solid-core cruise EV largely intact, investigate smaller thermal combat multipliers, and review the closed-cooling thermal accounting before cutting the performance of its competitors. For high-EV gas-core drives, an explicit radiator burden is a scientifically stronger option than treating every source-backed EV as excessive.

The following examples hold cruise thrust and drive efficiency fixed, retain the current selected-reactor coefficients, and ignore floors/auxiliary demand. Thus lowering EV also lowers jet power, required reactor power, and propulsion-associated reactor mass proportionally. This compensating mass reduction is important: an EV cut by itself is not a pure nerf.

| Illustrative isolated edit | Cruise EV km/s before → after | Combat multiplier before → after | Δ cruise thrust | Δ input power/reactor propulsion mass | Δ combat thrust | Δ combat EV | Δ cruise Δv at fixed mass ratio |
|---|---|---|---|---|---|---|---|
| Nerva Drive | 8.09 → 8.09 | 9 → 3 | 0% | 0% | -66.7% | 200% | 0% |
| Advanced Pebble Drive | 9.81 → 9.81 | 16 → 4 | 0% | 0% | -75% | 300% | 0% |
| Advanced Pulsar Drive | 32 → 16 | 5 → 3 | 0% | -50% | -40% | -16.7% | -50% |
| Burner Drive | 69 → 29.4 | 24 → 6 | 0% | -57.4% | -75% | 70.4% | -57.4% |
| Firestar Fission Lantern | 50 → 29.4 | 22 → 6 | 0% | -41.2% | -72.7% | 115.6% | -41.2% |


The Nerva 3× and Pebble 4× caps are exploratory gameplay values; **LANTR does not scientifically certify them**, because its propellants and energy budget differ. The 16 km/s Advanced Pulsar example is merely an intermediate sensitivity point, still above ordinary solid-core bounds. The 29.4 km/s gas-core examples represent choosing the historical regenerative-only screening regime; retaining 35–69 km/s with an explicit heat-rejection model is an alternative, not an error to be automatically removed.

Reducing `K` also **increases combat EV** and sharply reduces combat propellant use. At unchanged cruise EV, Nerva 9→3 cuts peak thrust by 66.7% but triples combat EV and cuts flow by 88.9%. That may improve practical combat endurance even while lowering acceleration. A balance test must measure both.

If a future change is pursued, the most informative matched-hull comparisons are Nerva versus Pulsar on the same reactor; Pulsar on regular versus compact reactors; Lars versus Teardrop/Spinner; Advanced Cavity versus Advanced Vortex on Vapor III; and high-EV gas-core versus fragment/pulse alternatives at fixed fuel volume. Record cruise acceleration, peak combat acceleration, usable combat delta-v, radiator mass/heat, reactor/bay use, tank count, fuel cost, and sustained burn time. This is a proposed later iteration, not testing performed for this research report.
