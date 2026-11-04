# Hull mesh volumes and runtime capacity inputs — 2026-11-03

**Historical 0.10.23 input snapshot:** [0.10.24 now deploys the audited alien allowances](alien-reactor-bay-implementation-2026-11-04.md),
including three Mothership housing regions. The human tables below are unchanged;
the alien bay column records the old class fallbacks and is superseded by the
implementation's current input table. The original JSON evidence remains frozen.
Mothership/Titan already reached current reactor-template output ceilings with
their old bay values; the new geometry preserves those ceilings.

Analysis only; no runtime, save, build or deployment changes. Covers all **28
hull classes / 64 authored graphical appearances**, including STO Fighter and
Salamander. The linked [runtime input/results snapshot](hull-volume-capacity-comparison-2026-11-03.json)
retains every appearance, minimum/maximum appearance IDs, exact source hashes
and the values read through the packaged production methods.

## Three different sizing inputs

The code does not use one common mesh volume for all three purposes:

- **Fuel capacity:** `HullFuelCapacityFeature.TryGetSnapshot` reads measured
  main-hull envelope volume from `HullVolumeCatalog`. Actual fuel allowance is
  `ceil(max(0, mainHull - installedModuleVolume - totalCrew*crewVolume))`.
  At current defaults, crew volume is 50 m³ per billet, utility cells 200 m³,
  hull weapon cells 250 m³ and nose weapon cells 400 m³. Actual tank count also
  depends on propellant density and the 100 t tank unit. Thus a hull class has
  no unique final fuel capacity without a selected design and propellant.
- **Reactor capacity / displayed engine bay:** `ReactorBayCapacityFeature` and
  the designer's engine-bay overlay both call `ShipBalanceMath.ReactorBayVolume_m3`.
  Human classes have measured machinery-bay inputs. The reactor's class-specific
  density, installed-mass fraction, specific mass and thermal rules then determine
  supported output and occupied bay volume; bay m³ is not itself a GW rating.
- **Engine size/performance:** `HullDriveScalingFeature.GraphicalMultiplier`
  uses nozzle-family art coefficients, not another hull-volume field. Human
  De Laval/Magnetic coefficients derive from x6-drive transverse bounding-area
  ratios; alien coefficients are compiled per-class values. They scale drive
  thrust, power requirement, mass and cost. Pulsed/Orion remains 1×. Reactor-bay
  capacity constrains whether the scaled drive demand can be supported.

The volumes below are **gameplay envelope inputs**, not watertight material
volume, actual habitable volume, empty tank volume, or combat-world-scaled volume.
Main hull excludes the separate drive subtree and named machinery meshes, with
the documented STO Fighter single-mesh separation exception. Model measurements
use the maintained asset-report methodology, rather than the hull template's
serialized `volume` field or statistical length/diameter cylinder.

## Reading the statistics

Each triplet is **minimum / median / maximum** across that class's authored
appearances. The twelve standard human classes have four appearances; the other
sixteen classes have one. For four values the median is the mean of the middle
two; it need not correspond to a selectable appearance. Statistics are unweighted
by ownership, DLC selection, faction, current ship counts or hull appearance in
a particular save. Main-hull, bay and drive medians are computed independently;
their combined median row is not a synthetic valid ship design.

Tables round to 0.1 m³ and 0.001×; runtime uses single-precision inputs. Values
were obtained through actual packaged `HullVolumeCatalog`, `HullDriveScaleCatalog`
and `ShipBalanceMath` methods, and checked against the maintained source catalogs.
These are the inputs used when the corresponding features are enabled, not a
claim that every current live session has those features enabled.

## Human hull classes

| Class | Variants | Main hull, min / median / max (m³) | Reactor/engine bay, min / median / max (m³) | Bay source |
| --- | ---: | ---: | ---: | --- |
| Gunship | 4 | 3,452.3 / 5,024.7 / 11,019.7 | 264.2 / 384.8 / 712.2 | Measured |
| Escort | 4 | 5,625.4 / 6,238.0 / 6,946.0 | 264.2 / 384.8 / 712.2 | Measured |
| Corvette | 4 | 5,070.8 / 10,725.2 / 13,890.2 | 264.2 / 528.5 / 837.6 | Measured |
| Frigate | 4 | 13,692.5 / 16,191.4 / 22,332.8 | 332.3 / 954.5 / 1,246.5 | Measured |
| Monitor | 4 | 17,160.4 / 37,734.0 / 48,694.7 | 384.6 / 1,352.1 / 2,617.6 | Measured |
| Destroyer | 4 | 22,062.0 / 52,876.4 / 66,132.6 | 384.6 / 1,352.1 / 2,617.6 | Measured |
| Cruiser | 4 | 45,819.9 / 120,065.5 / 202,070.4 | 1,385.0 / 2,747.4 / 3,930.6 | Measured |
| Battlecruiser | 4 | 43,684.4 / 121,679.7 / 205,738.7 | 1,385.0 / 2,747.4 / 3,930.6 | Measured |
| Battleship | 4 | 105,142.7 / 149,256.1 / 168,180.6 | 2,090.3 / 5,556.4 / 6,945.7 | Measured |
| Lancer | 4 | 152,598.4 / 236,015.7 / 259,685.3 | 2,090.3 / 5,219.2 / 10,223.9 | Measured |
| Dreadnought | 4 | 207,412.8 / 341,355.5 / 542,945.6 | 2,090.3 / 10,588.3 / 11,476.3 | Measured |
| Titan | 4 | 407,508.8 / 690,023.5 / 987,672.4 | 6,290.8 / 15,898.2 / 16,549.5 | Measured |
| STO Fighter | 1 | 4,081.7 / 4,081.7 / 4,081.7 | 2,617.6 / 2,617.6 / 2,617.6 | **Fallback** |

## Alien hull classes

| Class | Variants | Main hull, min / median / max (m³) | Reactor/engine bay, min / median / max (m³) | Bay source |
| --- | ---: | ---: | ---: | --- |
| Alien Gunship | 1 | 10,892.2 / 10,892.2 / 10,892.2 | 2,617.6 / 2,617.6 / 2,617.6 | **Fallback** |
| Alien Escort | 1 | 7,358.2 / 7,358.2 / 7,358.2 | 2,617.6 / 2,617.6 / 2,617.6 | **Fallback** |
| Alien Corvette | 1 | 13,174.5 / 13,174.5 / 13,174.5 | 2,617.6 / 2,617.6 / 2,617.6 | **Fallback** |
| Alien Frigate | 1 | 30,659.9 / 30,659.9 / 30,659.9 | 2,617.6 / 2,617.6 / 2,617.6 | **Fallback** |
| Alien Monitor | 1 | 75,475.8 / 75,475.8 / 75,475.8 | 3,930.6 / 3,930.6 / 3,930.6 | **Fallback** |
| Alien Destroyer | 1 | 47,314.4 / 47,314.4 / 47,314.4 | 3,930.6 / 3,930.6 / 3,930.6 | **Fallback** |
| Alien Cruiser | 1 | 57,947.5 / 57,947.5 / 57,947.5 | 3,930.6 / 3,930.6 / 3,930.6 | **Fallback** |
| Alien Battlecruiser | 1 | 172,807.6 / 172,807.6 / 172,807.6 | 3,930.6 / 3,930.6 / 3,930.6 | **Fallback** |
| Alien Battleship | 1 | 201,325.2 / 201,325.2 / 201,325.2 | 16,549.5 / 16,549.5 / 16,549.5 | **Fallback** |
| Alien Lancer | 1 | 166,671.7 / 166,671.7 / 166,671.7 | 16,549.5 / 16,549.5 / 16,549.5 | **Fallback** |
| Alien Dreadnought | 1 | 302,989.8 / 302,989.8 / 302,989.8 | 16,549.5 / 16,549.5 / 16,549.5 | **Fallback** |
| Alien Titan | 1 | 349,989.2 / 349,989.2 / 349,989.2 | 16,549.5 / 16,549.5 / 16,549.5 | **Fallback** |
| Alien Assault Carrier | 1 | 419,181.8 / 419,181.8 / 419,181.8 | 16,549.5 / 16,549.5 / 16,549.5 | **Fallback** |
| Alien Mothership | 1 | 49,197,068.0 / 49,197,068.0 / 49,197,068.0 | 16,549.5 / 16,549.5 / 16,549.5 | **Fallback** |
| Salamander Gunship | 1 | 14,539.5 / 14,539.5 / 14,539.5 | 2,617.6 / 2,617.6 / 2,617.6 | **Fallback** |

**Fallback is not a measured alien machinery-bay volume.** The runtime has 48
measured human bay entries and no measured bay entries for the other 16 class/
appearance combinations. Its current fallback values are Small 2,617.607 m³,
Medium 3,930.638 m³ and Large/Huge 16,549.539 m³, derived from the corresponding
human maximum. This includes STO Fighter and Salamander. Every listed main-hull
volume, by contrast, has a catalog entry; no runtime-cylinder fuel fallback was
needed for this inventory.

## Actual engine-art multipliers

These are the engine sizing coefficients the runtime really uses. **They are
dimensionless, not m³.** A coefficient is not a linear dimension or raw engine
volume. Human values below 1× are intentional. The first alien light hulls have
a 1× floor; Salamander has no standalone alien drive resource and falls back to
1×. STO Fighter lacks the human drive-art catalog row and also uses the safe
1× fallback. All classes use **1 / 1 / 1** for Pulsed/Orion.

| Class | De Laval, min / median / max (×) | Magnetic, min / median / max (×) | Source |
| --- | ---: | ---: | --- |
| Gunship | 0.668 / 1.138 / 1.596 | 0.397 / 0.951 / 1.327 | Measured human variants |
| Escort | 0.668 / 1.138 / 1.596 | 0.397 / 0.951 / 1.327 | Measured human variants |
| Corvette | 1.000 / 1.186 / 1.779 | 0.651 / 0.951 / 1.478 | Measured human variants |
| Frigate | 1.277 / 1.664 / 2.302 | 0.902 / 1.216 / 1.913 | Measured human variants |
| Monitor | 1.000 / 2.597 / 4.219 | 0.902 / 1.812 / 3.255 | Measured human variants |
| Destroyer | 1.000 / 2.979 / 4.219 | 1.000 / 2.164 / 3.255 | Measured human variants |
| Cruiser | 2.609 / 4.888 / 5.637 | 2.738 / 3.179 / 4.685 | Measured human variants |
| Battlecruiser | 2.179 / 4.457 / 5.637 | 2.001 / 3.393 / 4.685 | Measured human variants |
| Battleship | 2.847 / 4.486 / 8.893 | 2.900 / 3.550 / 7.390 | Measured human variants |
| Lancer | 1.715 / 5.695 / 9.831 | 1.715 / 4.107 / 8.170 | Measured human variants |
| Dreadnought | 6.744 / 9.335 / 12.048 | 4.325 / 7.038 / 10.012 | Measured human variants |
| Titan | 8.112 / 12.345 / 25.203 | 6.593 / 9.755 / 20.944 | Measured human variants |
| STO Fighter | 1.000 / 1.000 / 1.000 | 1.000 / 1.000 / 1.000 | 1× fallback |
| Alien Gunship | 1.000 / 1.000 / 1.000 | 1.000 / 1.000 / 1.000 | Alien class coefficient |
| Alien Escort | 1.000 / 1.000 / 1.000 | 1.000 / 1.000 / 1.000 | Alien class coefficient |
| Alien Corvette | 1.000 / 1.000 / 1.000 | 1.000 / 1.000 / 1.000 | Alien class coefficient |
| Alien Frigate | 1.144 / 1.144 / 1.144 | 1.144 / 1.144 / 1.144 | Alien class coefficient |
| Alien Monitor | 2.202 / 2.202 / 2.202 | 2.202 / 2.202 / 2.202 | Alien class coefficient |
| Alien Destroyer | 3.384 / 3.384 / 3.384 | 3.384 / 3.384 / 3.384 | Alien class coefficient |
| Alien Cruiser | 3.291 / 3.291 / 3.291 | 3.291 / 3.291 / 3.291 | Alien class coefficient |
| Alien Battlecruiser | 3.445 / 3.445 / 3.445 | 3.445 / 3.445 / 3.445 | Alien class coefficient |
| Alien Battleship | 3.445 / 3.445 / 3.445 | 3.445 / 3.445 / 3.445 | Alien class coefficient |
| Alien Lancer | 3.291 / 3.291 / 3.291 | 3.291 / 3.291 / 3.291 | Alien class coefficient |
| Alien Dreadnought | 3.445 / 3.445 / 3.445 | 3.445 / 3.445 / 3.445 | Alien class coefficient |
| Alien Titan | 7.531 / 7.531 / 7.531 | 7.531 / 7.531 / 7.531 | Alien class coefficient |
| Alien Assault Carrier | 3.445 / 3.445 / 3.445 | 3.445 / 3.445 / 3.445 | Alien class coefficient |
| Alien Mothership | 26.216 / 26.216 / 26.216 | 26.216 / 26.216 / 26.216 | Alien class coefficient |
| Salamander Gunship | 1.000 / 1.000 / 1.000 | 1.000 / 1.000 / 1.000 | 1× fallback |

## Consequences for the proposed tier boundaries

- **Human Frigate is the smaller end of tier 2.** Its median main-hull envelope
  is 16,191 m³, versus Monitor 37,734 and Destroyer 52,876. Its median bay is
  954.5 m³ versus their 1,352.1. Its five-utility escort role and Patrol Vessels/
  MC-2 placement support tier 2, but the physical support envelope is lower.
- **Human Cruiser's support capacity strengthens its tier-3 case.** Its median
  main-hull envelope is 120,065 m³, about 2.27× Destroyer's, and its median bay
  is 2,747.4 m³, about 2.03×. Battlecruiser has almost the same main-hull median
  and the same bay median. The Cruiser's modest raw integrity/mass gain therefore
  understates its geometric endurance/power-support advantage.
- **The appearance ranges overlap.** The largest Destroyer hull envelope is
  66,133 m³, exceeding the smallest Cruiser's 45,820; Destroyer's largest bay
  (2,617.6) also exceeds Cruiser's smallest (1,385.0). A formal class tier is not
  a guarantee that every skin is physically superior to every lower-tier skin.
- **Alien class ordering is not monotonic in mesh volume.** Alien Monitor's
  main-hull envelope (75,476) exceeds Alien Cruiser's (57,948), although the
  latter has a larger compiled engine coefficient. Both currently receive the
  same medium fallback reactor bay. Alien Cruiser's measured hull is only
  1.22× Destroyer's, not the human median 2.27×. Do not copy human geometric
  conclusions onto alien reactor capacity without actual bay measurements.
- **Mothership is an extreme fuel-envelope outlier, not a larger coded reactor
  bay.** Its main-hull envelope is about 49.2 million m³ and its engine coefficient
  is 26.216×, but its fallback reactor bay equals Alien Lancer/Dreadnought/Titan/
  Assault Carrier at 16,549.5 m³. This mismatch matters to the proposed N5 power
  and cooling budgets and merits a separate measured-bay audit before balancing
  flagship capability from hull volume alone.

The [Frigate/Cruiser boundary plan](frigate-and-cruiser-tier-boundary-plan-2026-11-03.md)
should therefore use these as three separate capacity axes. Median values support
the proposed human grouping, but individual appearances and alien fallback
limitations require explicit checks rather than a universal volume cutoff.

## Source authorities

Runtime: `HullVolumeCatalog`, `HullDriveScaleCatalog`, `ShipBalanceMath`,
`HullFuelCapacityFeature`, `ReactorBayCapacityFeature`, `HullDriveScalingFeature`
and `FuelCapacityDesignerUi` in the current source/package. The production
probe invokes the numerical readers/helpers without running a Unity scene.

Measurement records: [hull volume and appearances](hull-utility-slot-volume-report.md),
[reactor bay measurements](reactor-bay-volume-and-mass-capacity.md),
[drive-art scaling](human-drive-art-scaling-and-structural-mass-plan.md), and
[fuel accounting](fuel-volume-capacity-implementation.md). The current tables
are based on the runtime catalogs and compiled math, not historical mass values
in those earlier narratives.
