# Fusion drives: candidates for open-cycle propulsion

Date: 2026-09-27\
Status: historical pre-change research; the user subsequently approved all fusion drives as open cycle and doubled human fusion reactor specific masses for 0.10.1. See the [implementation record](fusion-open-cycle-implementation-2026-09-27.md). The inventories and numeric examples below preserve the pre-change evidence.\
Scope: installed main-game templates and English localization, overlaid with the current EEO workspace. All 26 human fusion-drive families and three alien families are covered, including their six engine-count variants. Scientific sources were checked on the report date.

## Assessment

**The descriptions support treating most human fusion drives as direct fusion propulsion rather than fusion-electric propulsion.** The strongest candidates are the Nova, Z-pinch, and Reflex families, plus the non-protium Torus drives and the two advanced hybrid Plasmajet drives. The early Fusor and Polywell drives are also plausible candidates, but need a clearer account of how plasma or reaction products reach the exhaust. The four proton-proton drives require a separate science-fiction assumption; existing fusion research does not substantiate their compact stellar-fusion power sources.

**Direct propulsion does not establish that almost all waste heat leaves through the exhaust.** A drive can bypass electrical conversion while retaining a hot blanket, radiation shielding, laser or beam power supplies, superconducting magnets, and substantial radiators. The appropriate direction is to distinguish direct drive power from electricity and then assess retained heat by architecture and fuel. Applying the existing open-cycle flag uniformly would combine these separate decisions.

The initial observation is correct for **all human fusion drives**: 156 records in 26 families explicitly have `cooling: Closed`. Two alien families add another 12 closed records. The six Advanced Alien Fusion Torch records instead use `Calc`; with the current EEO values they already resolve to open cycle. Thus the complete audited population is **174 fusion records: 168 explicitly closed and six calculated-open**.

Recommended classification for future consideration:

| Recommendation | Human families | Meaning |
|---|---:|---|
| Strong candidates for direct/open exhaust | 18 | Lore and a relevant physical propulsion mechanism support bypassing the main electrical-conversion path. Heat and mass benefits still need independent justification. |
| Conditional candidates | 4 | Triton/Deuteron Fusor and Triton/Deuteron Polywell: plausible exhaust interpretation, but insufficient detail and greater uncertainty in the underlying reactor concept. |
| Fiction-dependent candidates | 4 | Protium Fusor, Protium Torus, Protium Nova, Protium Converter: can be made internally consistent, but cannot be justified as straightforward extensions of demonstrated fusion research. |

These categories rank the **case for a propulsion architecture**, not technological readiness or the realism of the game's thrust, exhaust velocity, efficiency, or reactor mass.

## Evidence and interpretation rules

### What was inspected

The installed source root is:

`D:/Games/SteamLibrary/steamapps/common/Terra Invicta/TerraInvicta_Data/StreamingAssets/`

The audit read `Templates/TIDriveTemplate.json`, `TIPowerPlantTemplate.json`, `TIProjectTemplate.json`, and `TITechTemplate.json`, plus their matching files in `Localization/en/`. Supplied fields and localization keys in [EEO ModFiles](../../TIEconomyMod/ModFiles) were applied over those sources. The runtime behavior was checked in the installed `TerraInvicta_Data/Managed/Assembly-CSharp.dll`, particularly `TIDriveTemplate.openCycleCooling` and `TIProjectTemplate.summary`. The repository's existing compatibility baseline identifies this installation generation as [1.0.53b](../compatibility/ti-1.0.53b-baseline.json).

This is a main-game/EEO audit, not an audit of every DLC scenario or third-party mod load order. Numeric examples use the workspace values, not a saved ship with faction effects or hull scaling applied. No build, deployment, save edit, or in-game test was performed for this documentation-only task.

### Project descriptions are not independent essays here

All 26 human drive projects and all 27 human fusion-reactor projects have a localized summary of `<shipmodule>`. None of those 53 projects supplies a separate `TIProjectTemplate.description.<project ID>` entry in the inspected English sources. The runtime resolves the summary to the description of the first unlocked ship part. Consequently, the project summary and module localization are **the same evidence**, not two independent confirmations.

The tables below paraphrase that shared drive/project text and quote or paraphrase the associated reactor text. The global technology descriptions provide additional context. Every human drive project directly requires `MagneticNozzles` except Protium Converter, which inherits it through Protium Nova. A magnetic nozzle supports the plasma-exhaust interpretation, but it is not conclusive by itself: electrically heated plasma can also use one.

“Associated reactor” means the reactor project expressly required by the drive's unlock project. In the designer, compatibility is principally a **reactor-class and available-output** rule, not an immutable one-drive/one-reactor pairing. Later reactors in the class can power earlier drives. Fuel names therefore describe the intended technology lineage; the compatibility system should not be mistaken for a fully modeled fuel-cycle match.

### Three different meanings must remain separate

1. **Direct fusion propulsion:** fusion products or fusion-heated propellant supply thrust without first converting the bulk propulsion power to electricity.
2. **Open exhaust / propellant cooling:** some energy and matter leave the spacecraft, reducing the energy that onboard radiators must reject. This says nothing by itself about the fraction of neutron or photon energy intercepted by the ship.
3. **Closed magnetic confinement:** the geometry of the confined plasma. A toroidal reactor can still transfer plasma or energy to an open propulsion stream.

Likewise, **direct energy conversion** often means converting charged-particle energy directly into *electricity*. That phrase does not establish direct thrust. A reactor can support both operating modes.

## What real fusion research supports

### Direct propulsion is a real research category

NASA's Fusion Driven Rocket study proposes fusion energy deposition into an expendable metal liner that becomes propellant. Its conceptual mechanism avoids converting the main propulsion power to electricity and uses the ejected material to intercept radiation. This is a useful example of what would actually justify substantial exhaust heat removal; none of the audited drives explicitly specifies that expendable lithium-liner architecture. It would be inappropriate to grant every fusion drive its claimed thermal advantages. [NASA, Fusion Driven Rocket](https://www.nasa.gov/general/the-fusion-driven-rocket-nuclear-propulsion-through-direct-conversion-of-fusion-energy/).

The Princeton Direct Fusion Drive work studies an FRC plasma with a surrounding flow of additional propellant and a magnetic nozzle. Its report separately addresses shielding, radiation, electric-power conversion, and a recirculating mode without thrust. This supports integrated propulsion and electrical generation, rather than assuming that the presence of a reactor or generator makes a drive electric. An FRC is not the same reactor as a Polywell or a simple magnetic mirror, so this is an architectural comparison, not an identification of a game drive with PFRC. [Thomas et al., NIAC Phase II final report, 2019, especially §§2.2, 3.2, 3.6 and Appendix A](https://ntrs.nasa.gov/api/citations/20190031807/downloads/20190031807.pdf).

### A tokamak need not imply electric propulsion

NASA's *Discovery II* spherical-torus study describes extraction of energetic plasma through an open divertor into a magnetic nozzle, mixing with added hydrogen. That route bypasses the main electrical conversion stage while retaining a toroidal fusion core. The report explicitly treats the divertor as conceptual and identifies unresolved plasma pumping and confinement issues. It supports considering the Torus drives, not claiming that an ordinary terrestrial tokamak can simply be fitted with a nozzle. [Williams et al., NASA/TM-2005-213559, pp. 25–27](https://ntrs.nasa.gov/api/citations/20050160960/downloads/20050160960.pdf).

### Pulsed fusion still needs electricity and radiators

The VISTA study provides a historical inertial-confinement propulsion analogue using D-T fuel. NASA's later Z-pinch propulsion study separately budgets propulsion heat rejection, electrical power conversion, and crew cooling; its example includes a 460.9 MW high-temperature heat-rejection requirement. Those are conceptual design results, not demonstrated flight engines or recommended EEO heat values. They show why electrically powered ignition and substantial radiators can coexist with direct fusion thrust. [VISTA study](https://ntrs.nasa.gov/citations/19880003642), [NASA Z-Pinch Pulsed Plasma Propulsion Technology Development, §5.5](https://ntrs.nasa.gov/api/citations/20110008519/downloads/20110008519.pdf).

There is experimental support for parts of these approaches. LLNL's ignition results demonstrate target gain relative to laser energy delivered to the target; this is not net electrical output for the whole facility or a working rocket. The University of Washington's sheared-flow Z-pinch work reports sustained neutron production, not a demonstrated terawatt spacecraft reactor. [LLNL, fusion ignition](https://lasers.llnl.gov/science/achieving-fusion-ignition), [LLNL, energy-system distinction](https://lasers.llnl.gov/science/energy-security), [UW, sheared-flow fusion experiments](https://www.aa.washington.edu/news/article/2019-04-05/plasma-flows-may-provide-missing-ingredient-cheaper-more-compact-route).

### Fuel matters as much as confinement

| Fuel family | Relevant physics | Consequence for an open-cycle proposal |
|---|---|---|
| D-T / Triton | About 80% of fusion energy is carried by uncharged neutrons. Magnetic nozzles cannot steer that neutron energy. | A direct plasma exhaust is plausible, but retained blanket/shield heating or escaping neutron losses need explicit treatment. A low radiator burden does not imply high jet efficiency. |
| D-D / Deuteron | The principal branches produce either helium-3 plus a neutron or tritium plus a proton. Burning the produced tritium adds D-T neutron production. | Do not treat this as a clean charged-particle-only exhaust. The secondary burn assumptions materially change the energy budget. |
| D-He3 / Helion | The main reaction produces an alpha particle and a proton; D-D side reactions can still produce neutrons. | A particularly attractive direct-exhaust candidate, with residual radiation, shielding, and auxiliary electrical requirements. |
| p-B11 / Borane | The main reaction produces three alpha particles. Bremsstrahlung and maintaining a favorable plasma energy balance are major obstacles. | A good *exhaust concept* if the reactor works, but “aneutronic” does not mean cold, lossless, or demonstrated. |
| p-p / Protium | The initiating stellar reaction involves the weak interaction and produces a positron and neutrino; the chain is not merely efficient electromagnetic heating of ordinary hydrogen. | A compact high-power reactor needs an extraordinary assumption beyond stronger confinement. Choose cycle behavior as fiction, not as a research-derived prediction. |

Sources: [ITER, D-T energy partition](https://www.iter.org/fusion-energy/making-it-work); [PFRC report, Appendix A.3–A.6, D-D/D-He3 reactions and losses](https://ntrs.nasa.gov/api/citations/20190031807/downloads/20190031807.pdf); [Ochs et al., *Bremsstrahlung constraints on proton-boron 11 inertial fusion*, 2026](https://doi.org/10.1063/5.0305034); [Gould and Guessoum, proton-proton reaction cross-section study](https://ntrs.nasa.gov/citations/19900059067).

The p-B11 study finds severe radiation-trapping and compression requirements under its modeled ICF assumptions. It supports caution about Borane readiness, not a proof that every possible p-B11 concept is impossible. The p-p comparison likewise concerns the absence of an established compact power-producing mechanism, not a prohibition on fictional late-game technology.

## Drive-by-drive recommendations

Names below omit the localized `x1` suffix. Each row covers all six engine-count variants. **Strong** means a strong direct-propulsion candidate subject to the thermal caveats above; **conditional** calls for an explicit exhaust design; **fiction** requires unsupported p-p reactor assumptions. Reactor numbers identify the unlocking reactor project.

### Mirror-cell / Reflex: strong candidates

The Mirror Cell Fusion Reactor I–III localization describes a linear reactor using magnetic mirrors to contain and direct plasma. This is the clearest reactor-family wording for controlled plasma extraction. Its module-project summaries reuse that text. The magnetic-confinement technology also discusses directing plasma for propulsion, not only generating electricity.

| Drive | Associated reactor | Drive/project localization, paraphrased | Verdict and reason |
|---|---|---|---|
| Triton Reflex Drive | Mirror Cell I | D-T mirror confinement enables low continuous thrust over long journeys. | **Strong.** An axial plasma outlet is consistent with the linear reactor. The D-T blanket and neutron load prevent assuming near-total exhaust cooling. |
| Deuteron Reflex Drive | Mirror Cell II | Better materials permit hotter fusion and greater thrust. | **Strong.** It inherits the same mirror architecture; the upgrade does not introduce an electric thruster. D-D radiation remains relevant. |
| Helion Reflex Drive | Mirror Cell III | D-He3 mirror-cell fusion provides cleaner propulsion power. | **Strong; high priority.** The fuel and plasma-directing reactor text make a coherent direct-exhaust interpretation. “Clean” should be read comparatively. |

The open geometry is a physically relevant reason for this recommendation, not evidence that magnetic mirrors are automatically efficient. End losses, maintaining confinement while extracting useful exhaust, and auxiliary power must still be resolved. NASA distinguishes conversion to electricity from conversion to thrust in its research on centrifugally confined fusion plasmas. [NASA, advanced fusion power and thrust generation](https://www.nasa.gov/directorates/stmd/space-tech-research-grants/advanced-fusion-power-and-thrust-generation-with-centrifugally-confined-plasmas/).

### Z-pinch: strong candidates, including Firefly

Z-Pinch Fusion Reactor I–IV localization describes magnetic compression of reaction plasma. The Flow-Stabilized Z-Pinch reactor adds sheared-flow stabilization and terawatt operation. More decisively, the **Z-Pinch Techniques** global description explicitly couples the fusion energy to a spacecraft's magnetic nozzle. All five drives use `ReactionProducts` propellant and `DriveActive` power generation in the templates. These fields corroborate an integrated engine interpretation, although neither alone proves its heat budget.

| Drive | Associated reactor | Drive/project localization, paraphrased | Verdict and reason |
|---|---|---|---|
| Zeta Triton Drive | Z-Pinch I | Z-pinched tritium/deuterium fusion supplies thrust. | **Strong.** Direct nozzle use is explicit in the parent technology. Neutron-heavy D-T limits the power available as magnetically directed exhaust. |
| Zeta Deuteron Drive | Z-Pinch II | Z-pinched D-D fusion powers a high-efficiency drive. | **Strong.** Same direct architecture and reaction-product propellant; D-D radiation must remain in the accounting. |
| Zeta Deuteron Fusion Torch | Flow-Stabilized Z-Pinch | The “Firefly” uses Z-pinched deuterium and high-speed plasma. | **Strong; high priority.** Its plasma-exhaust description and specialized stabilization reactor agree. Terawatt capability is game extrapolation, not a laboratory result. |
| Zeta Helion Fusion Lantern | Z-Pinch III | Stabilized D-He3 fusion plasma enables efficient long-range travel. | **Strong; high priority.** Charged main-reaction products are well suited to the intended magnetic nozzle. |
| Zeta Borane Fusion Lantern | Z-Pinch IV | Protons fuse with boron-11 to provide powerful propulsion. | **Strong architecture case, highly speculative reactor.** Reaction-product propellant is consistent with directing alpha particles, but photon losses cannot be directed by that nozzle. |

NASA's pulsed Z-pinch study is an architectural analogue, not an exact model of the game's flow-stabilized Firefly. Pulsed magneto-inertial operation and sheared-flow stabilization must not be treated as identical simply because both use a pinch. The cited NASA and UW work support different parts of the comparison.

### Inertial confinement / Nova: strong except for the p-p assumption

Inertial Confinement Fusion Reactor I–VII localization describes powerful lasers inducing sustained fusion. In research terms, sustained power from ICF requires repeated fuel-target shots and energy recovery, rather than a permanently burning pellet. The global technology emphasizes material durability, heat rejection, and neutron handling—evidence that the reactor is not thermally cost-free. The drive templates use `DriveActive` power generation.

| Drive | Associated reactor | Drive/project localization, paraphrased | Verdict and reason |
|---|---|---|---|
| Triton Nova Drive | Inertial I | Laser-induced D-T fusion heats hydrogen propellant. | **Strong; the most explicit early example.** Fusion-to-propellant heating is stated outright. This is not evidence that its laser or neutron blanket needs negligible power/cooling. |
| Deuteron Nova Fusion Lantern | Inertial II | Refined inertial confinement greatly increases fusion-drive thrust. | **Strong.** The family progression supports retaining Triton Nova's direct-heating mechanism. D-D is identified by the reactor project's fuel prerequisite and template notes. |
| Helion Nova Lantern | Inertial III | Cleaner, more efficient D-He3 fusion powers medium-range spacecraft. | **Strong.** Consistent with the same pulsed/direct family, now with more favorable main reaction products. |
| Helion Nova Fusion Torch | Inertial IV | A terawatt-capable D-He3 drive nicknamed “Daedalus.” | **Strong; high priority.** The ICF identity and Daedalus reference support a pulsed fusion exhaust. The name does not establish that every detail matches the historical design. |
| Borane Nova Lantern | Inertial V | Proton-boron inertial fusion provides powerful, efficient propulsion. | **Strong architecture case, highly speculative reactor.** Continue the family's exhaust interpretation, but retain laser, photon, and target-engineering costs. The 2026 p-B11 ICF research particularly cautions against easy performance extrapolation. |
| Protium Nova Torch | Inertial VI | Direct proton-proton fusion supplies powerful propulsion. | **Fiction.** Direct propulsion fits the family, but “direct proton-proton” names the reaction and does not independently specify an exhaust path. The fuel-cycle breakthrough is unsupported by practical fusion research. |
| Protium Converter Torch | Inertial VII | A miniature stellar source powers a sub-petawatt drive with interstellar potential. | **Fiction; strong internal direct-exhaust reading.** `ReactionProducts` propellant strengthens the case. “Converter” does not prove electrical conversion, and cannot cure the p-p feasibility problem. |

VISTA is the principal research analogue used here. The Daedalus name is also consistent with a historical fusion-propulsion study, but this report does not import its engineering assumptions into the game. [BIS, review of the Daedalus propulsion system](https://bis-space.com/shop/product/project-icarus-a-review-of-the-daedalus-main-propulsion-system/).

### Toroidal confinement / Torus: candidates despite the closed magnetic core

Fusion Tokamak I–V localization describes a toroidal field containing plasma and collecting charged particles to generate power. This explicitly supports a power-generation mode; it does **not** establish that all drive power must pass through that mode. The drive's own wording and the distinction between core confinement and exhaust flow are decisive.

| Drive | Associated reactor | Drive/project localization, paraphrased | Verdict and reason |
|---|---|---|---|
| Triton Torus Drive | Tokamak I | A tokamak-derived fusion drive uses tritium fusion to heat propellant. | **Strong.** The drive explicitly describes heating propellant; a direct thermal/plasma path fits better than assuming an unstated electric thruster. Retaining an internal blanket is possible. |
| Deuteron Torus Drive | Tokamak II | An enhanced D-D drive uses magnetic plasma containment. | **Strong family-level case.** Read as the successor to Triton Torus, with a defined outlet or heat-transfer path still required. |
| Helion Torus Lantern | Tokamak III | Magnetically confined D-He3 produces moderate thrust and efficiency. | **Strong family-level case.** A Discovery-II-like exhaust arrangement is a relevant conceptual precedent, with material and divertor heat still present. |
| Protium Torus Fusion Lantern | Tokamak V | Extreme magnetic forces fuse free protons under stellar-like conditions. | **Fiction.** A direct outlet is conceivable within the lore; stronger fields alone do not validate the proposed p-p reactor. |

Tokamak IV has the same reactor localization and is part of the upgrade chain, but **no human fusion drive project in this set directly selects it as its unlocking reactor**. Do not invent a missing Torus drive to make the numbering one-to-one.

These are weaker candidates for *very low retained heat* than for *direct drive power*. A proposed implementation should explain plasma extraction, blanket coupling, or both. Merely relabeling a power-producing tokamak as open cycle would omit the engineering change that makes the comparison work.

### Hybrid confinement / Polywell and Plasmajet

Hybrid Confinement Fusion Reactor I–II localization combines electrostatic and magnetic confinement. III–IV explicitly adds inertial confinement. The project progression reinforces that distinction: III requires inertial-confinement technology and D-He3; IV requires aneutronic fusion, terawatt reactors, and plasma weapons in addition to III.

| Drive | Associated reactor | Drive/project localization, paraphrased | Verdict and reason |
|---|---|---|---|
| Triton Polywell Drive | Hybrid I | Multiple confinement techniques produce modest thrust efficiently. | **Conditional.** It is presented as a fusion drive, but neither the drive nor reactor specifies how the heated hydrogen escapes. Define a cusp outlet or another direct transfer path before granting open-cycle behavior. |
| Deuteron Polywell Drive | Hybrid II | A second hybrid generation greatly improves thrust and efficiency. | **Conditional.** Inherits the same ambiguity; higher performance does not resolve the exhaust geometry or D-D radiation losses. |
| Helion Plasmajet Fusion Lantern | Hybrid III | Adding rudimentary inertial confinement improves the hybrid drive. | **Strong, with moderate textual confidence.** The Plasmajet identity, hydrogen propellant, and changed reactor architecture together support an integrated plasma exhaust. The exact mechanism remains inferred. |
| Borane Plasmajet Fusion Torch | Hybrid IV | A powerful hybrid propulsion system fuses protons with boron-11. | **Strong architecture case, highly speculative reactor.** The same advanced hybrid family makes direct exhaust reasonable, subject to p-B11 radiation and gain limitations. |

There is no basis for equating the game's hybrid electrostatic/magnetic/inertial machine with one demonstrated real-world reactor. The Direct Fusion Drive and Fusion Driven Rocket studies show possible ways to couple fusion plasma to propellant, not experimental validation of these specific hybrid drives. That is why the early Polywells receive a conditional rating and the late Plasmajets retain a confidence caveat.

### Electrostatic confinement / Fusor

Electrostatic Confinement Fusion Reactor I–III localization describes electron-beam-induced fusion in a compact reactor. The parent electrostatic technology discusses heating fuel and converting charged-particle energy into useful power. All three drive templates specify `ReactionProducts`, which is meaningful evidence for an exhaust made from the reacting material rather than an unrelated electrically accelerated propellant.

| Drive | Associated reactor | Drive/project localization, paraphrased | Verdict and reason |
|---|---|---|---|
| Triton Fusor Drive | Electrostatic I | Strong electric fields induce fusion in a compact drive. | **Conditional.** Electric fields are part of fusion production, not proof of an electric propulsion stage. Reaction-product exhaust favors direct propulsion, but extraction, confinement losses, and neutron deposition are unspecified. |
| Deuteron Fusor Drive | Electrostatic II | A second-generation electrostatic drive extracts more energy from D-D fusion. | **Conditional.** Same direct-exhaust reading is possible; the efficiency claim does not establish reactor gain or small thermal losses. |
| Protium Fusor Drive | Electrostatic III | A very small engineered system emulates stellar fusion. | **Fiction.** Reaction-product propellant favors direct exhaust within the game, but both its compact IEC power source and p-p fuel cycle require extraordinary assumptions. |

Electric confinement and direct fusion thrust are compatible in principle. The reason for caution is the reactor and extraction mechanism, not the presence of electric fields. NASA has investigated alternative IEC designs while explicitly acknowledging severe confinement and loss difficulties; that research is not proof of a flight-capable, net-power Fusor. [NASA, Continuous Electrode Inertial Electrostatic Confinement Fusion](https://www.nasa.gov/general/continuous-electrode-inertial-electrostatic-confinement-fusion/).

### Alien fusion drives

All three alien drive descriptions merely identify a powerful fusion reaction. All three alien reactor descriptions identify a mixture of electrostatic and inertial confinement. The hidden master projects provide no separate English drive/reactor description in the inspected localization. Template notes identify D-D hybrid fusion, but this is weaker evidence than an explicit localized explanation of the energy path.

| Drive | Associated reactor lineage | Current behavior | Recommendation |
|---|---|---|---|
| Alien Fusion Lantern | Alien Hybrid and Alien Advanced Hybrid reactors share `Project_AlienMasterProject` with it | Explicitly closed | **Conditional.** Direct exhaust is consistent with the hybrid theme, but the text does not resolve it. Do not use superior alien technology alone as a thermal justification. |
| Alien Fusion Torch | Same master-project/class lineage | Explicitly closed | **Conditional.** Same evidence limitation; not uniquely paired to one reactor tier by its project. |
| Advanced Alien Fusion Torch | Alien Super Advanced Hybrid reactor shares `Project_AlienAdvancedMasterProject` | `Calc`, currently resolves open | **Already open in this workspace.** It is not a conversion candidate. |

For the advanced torch, the installed getter returns open for a non-pulsed `Calc` drive when the **single-engine** mass flow is at least 3 kg/s. Its EEO values give `10,500,000 N / 3,000,000 m/s = 3.5 kg/s`. All six variants consult the single-engine template. This is a game threshold, not a scientific criterion for disposing of neutron and photon heat.

## Conflicts in the localization that should not become physical assumptions

The parent technology text is useful evidence of intended fiction, but several claims are simplified or incorrect:

- **D-T Fusion** describes a lithium breeding blanket built into the reactor chamber. That supports retained material and neutron heating unless a propulsion-specific arrangement is supplied. It argues against silently assuming every D-T engine has a completely exposed burn with nearly all radiation escaping.
- **D-He3 Fusion** describes only a proton as the reaction product and speaks categorically about no neutron radiation. The main reaction also produces an alpha particle; deuterium side reactions can produce neutrons. Charged-particle collection may mean electrical generation, whereas a nozzle would extract momentum.
- **Aneutronic Fusion** says all released energy is electromagnetic radiation. For p-B11, the main fusion products are alpha particles; bremsstrahlung is a loss channel, not a correct description of all fusion output. If all usable energy really were photons, a magnetic nozzle could not redirect it as charged-particle exhaust.
- **Proton-Proton Fusion** describes extraordinary power obtained by pressing protons together and collecting electromagnetic radiation and electrons. This omits the weak-interaction bottleneck and neutrino losses and does not accurately specify the chain's reaction products.

These corrections follow the fuel sources above. They do not invalidate the game's choice to include speculative technology, but they prevent a localization error from serving as the evidence for an extreme heat or efficiency bonus.

## Why flipping `cooling` alone is not a complete correction

The current [thermal calculation](../../TIEconomyMod/Core/PowerPlantThermalMath.cs) treats a closed drive's requested power as useful electrical demand. With an open drive, it instead computes direct reactor demand separately from systems and weapons. The [ship power patches](../../TIEconomyMod/Patches/ShipPowerPatches.cs) feed `drive.openCycleCooling` into that accounting when the thermal feature is enabled.

Using `D` for requested drive power, `S` for auxiliary electrical demand, `η` for plant efficiency, and `f` for the retained fraction parameter, the current formulas are:

```text
Closed:
  electrical demand = D + S
  reactor input     = (D + S) / η
  plant waste heat  = (D + S) × (1/η − 1)

Open:
  direct coupling   = 1 − f × (1 − η)
  direct input      = D / direct coupling
  direct waste heat = direct input × f × (1 − η)
  electrical input  = S / η
```

The shipped settings use `f = 0.01`. This is **1% of the model's conversion-loss term**, not 1% of total fusion power. At 85% plant efficiency, the open direct heat term is only 0.15% of the calculated direct reactor input.

For illustration, one unscaled Triton Nova has 355,200 N thrust, 270 km/s exhaust velocity, and 80% drive efficiency. Its jet power is 47.952 GW and its requested drive power is 59.94 GW. Paired with Inertial I at 85% plant efficiency, ignoring auxiliary loads:

| Quantity | Current closed accounting | Hypothetical open accounting |
|---|---:|---:|
| Reactor input assigned to the drive | 70.518 GW | 60.030 GW |
| Plant waste heat assigned to the drive | 10.578 GW | 0.090 GW |
| Reactor mass-rating contribution with default open multiplier | 70.518 GW | 30.015 GW |

This roughly 117-fold reduction in the **plant waste-heat term** follows the game's formulas, not a demonstrated D-T thermal balance. The table is not total ship heat: it does not independently determine how the drive's own inefficiency, neutron leakage, intercepted radiation, or ignition system should be partitioned.

The current [reactor mass-scaling default](../../TIEconomyMod/Core/PowerPlantScalingMath.cs) is 0.5 for fusion classes, unless overridden through the [registry](../../TIEconomyMod/Core/PowerPlantScalingRegistry.cs). Thus an open flag can also materially change reactor mass and output-cap accounting. Removing an electrical conversion stage could reduce mass, but does not halve the fusion core, shielding, magnets, or laser equipment by physical necessity.

A scientifically grounded future model would separately account for:

1. Energy reaching directed exhaust, including nozzle and propellant-coupling losses.
2. Neutrons and photons escaping without striking the ship: lost propulsion energy, but not onboard radiator heat.
3. Radiation and particle energy deposited in permanent structures or blankets.
4. Heat taken away by expendable propellant or liners, with the corresponding mass flow.
5. Electrical recirculation for lasers, beams, plasma heating, pumps, and cryogenics, in addition to ship systems and weapons.
6. Confinement and shielding mass versus power-conversion equipment that can actually be reduced.

This is an analysis requirement, not an implementation plan or a request to redesign the mod this turn. In particular, retaining closed cooling temporarily for an unresolved candidate would be a conservative modeling choice, not proof that its fictional engine is fusion-electric.

## Recommended order for a later decision

First resolve **Triton Nova, Helion Nova Torch, Zeta Deuteron Torch, Zeta Helion Lantern, and Helion Reflex**. Together they cover explicit fusion-heated propellant, pulsed ICF, flow-stabilized pinch, and mirror exhaust. They make the clearest case for separating direct propulsion from electrical plant demand.

Extend the architecture decision across the remaining non-protium Nova, Z-pinch, and Reflex drives, while keeping fuel-specific thermal differences. Consider the non-protium Torus family with an explicit divertor/propellant-transfer interpretation, and the advanced Plasmajets with an explicitly acknowledged hybrid-engine inference.

Defer automatic Fusor/early-Polywell changes until the intended exhaust mechanism is stated. Treat all four Protium drives as a separate fiction-consistency decision. Audit the two closed alien families separately; the advanced alien torch already receives open-cycle handling.

**No reviewed human fusion-drive description unambiguously identifies a separate, bulk fusion-electric propulsion stage.** That supports the user's concern. The evidence is strongest for changing how direct propulsion power is represented, and weaker for granting the existing universal open-cycle heat and mass benefits unchanged.

## Audit appendix: exact identifiers and project associations

For human drives, the drive project ID is `Project_` followed by the drive stem below; the six module IDs append `x1` through `x6` to that stem. The associated reactor project ID is `Project_` followed by the reactor ID. These are exact template identifiers, including capitalization; they can differ from localized names.

| Drive stem | Unlocking reactor ID | Fuel interpretation |
|---|---|---|
| `TritonFusorDrive` | `ElectrostaticConfinementFusionReactorI` | D-T |
| `DeuteronFusorDrive` | `ElectrostaticConfinementFusionReactorII` | D-D |
| `ProtiumFusorDrive` | `ElectrostaticConfinementFusionReactorIII` | p-p |
| `TritonReflexDrive` | `MirrorCellFusionReactorI` | D-T |
| `DeuteronReflexDrive` | `MirrorCellFusionReactorII` | D-D |
| `HelionReflexDrive` | `MirrorCellFusionReactorIII` | D-He3 |
| `TritonTorusDrive` | `FusionTokamakI` | D-T |
| `DeuteronTorusDrive` | `FusionTokamakII` | D-D |
| `HelionTorusLantern` | `FusionTokamakIII` | D-He3 |
| `ProtiumTorusLantern` | `FusionTokamakV` | p-p |
| `TritonPolywellDrive` | `HybridConfinementFusionReactorI` | D-T |
| `DeuteronPolywellDrive` | `HybridConfinementFusionReactorII` | D-D |
| `HelionPlasmajetLantern` | `HybridConfinementFusionReactorIII` | D-He3 |
| `BoranePlasmajetTorch` | `HybridConfinementFusionReactorIV` | p-B11 |
| `ZetaTritonDrive` | `ZPinchFusionReactorI` | D-T |
| `ZetaDeuteronDrive` | `ZPinchFusionReactorII` | D-D |
| `ZetaDeuteronTorch` | `FlowStabilizedZPinchFusionReactor` | D-D |
| `ZetaHelionLantern` | `ZPinchFusionReactorIII` | D-He3 |
| `ZetaBoraneLantern` | `ZPinchFusionReactorIV` | p-B11 |
| `TritonNovaDrive` | `InertialConfinementFusionReactorI` | D-T |
| `DeuteronNovaLantern` | `InertialConfinementFusionReactorII` | D-D |
| `HelionNovaLantern` | `InertialConfinementFusionReactorIII` | D-He3 |
| `HelionNovaTorch` | `InertialConfinementFusionReactorIV` | D-He3 |
| `BoraneNovaLantern` | `InertialConfinementFusionReactorV` | p-B11 |
| `ProtiumNovaTorch` | `InertialConfinementFusionReactorVI` | p-p |
| `ProtiumConverterTorch` | `InertialConfinementFusionReactorVII` | p-p |

Fuel interpretations combine drive localization, template notes, and reactor-project prerequisites; they are not an additional implemented compatibility restriction. For example, Helion Nova Torch still explicitly uses D-He3 even though its Inertial IV unlock requires the broader `AneutronicFusion` technology.

For source lookup, use `TIDriveTemplate.description.<stem>x1` and `TIPowerPlantTemplate.description.<reactor ID>` in English localization; use `TIProjectTemplate.summary.Project_<stem>` for the project-summary marker. Additional relevant technology keys are `MagneticNozzles`, `MagneticPlasmaConfinementTechniques`, `ElectrostaticPlasmaConfinement`, `Tokamaks`, `ZPinchTechniques`, `InertialPlasmaConfinementTechniques`, and the five fuel technologies discussed above.

Verification for this report consists of a complete family/variant inventory, project-prerequisite and localization-key checks, source inspection of the runtime summary/cooling getters, checks of EEO thermal settings and formulas, and primary-source research. The report introduces no gameplay, data-template, or tooling change requiring deployment.
