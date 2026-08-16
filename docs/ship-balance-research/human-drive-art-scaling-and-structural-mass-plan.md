# Human drive-art scaling and flat structural mass implementation

Status: implemented, validated, and deployed for Terra Invicta 1.0.51 on
2026-08-16.

## Scope

- Human De Laval and magnetic drives use the measured scale for the selected
  graphical hull appearance.
- Pulsed/Orion drives remain fixed-size, fixed-mass appendages at **1.000x** on
  every hull and appearance.
- Alien scaling remains unchanged.
- The hull-art selector reports the same De Laval and magnetic values used by
  runtime thrust, power, drive mass, and drive cost calculations.
- Reactor-bay validation continues to compare the scaled drive demand against
  the selected appearance's measured reactor-bay capacity.

The maintained runtime inputs are
`ModFiles/Config/hull-variant-drive-scales.csv` and
`ModFiles/Config/hull-variant-main-volumes.csv`. Missing human hull,
appearance, or nozzle-family data must produce an error and use vanilla 1x
rather than silently inheriting another variant.

## Flat empty-hull mass rule

Empty structural mass is an authored flat value for each human hull appearance.
There is no runtime component formula and no mass scaling from main-hull volume,
reactor-bay volume, or drive capacity. Appearance 0 retains the currently
balanced template mass. Other appearances use the explicit table below.

Installed De Laval and magnetic drive hardware remains independently scaled by
the selected appearance's drive-art factor. Pulsed/Orion drive hardware remains
at 1x. This keeps the hull mass stable when modules are swapped and makes every
mass shown in the art selector directly auditable.

## Measured per-engine scaling

Each cell is `De Laval / Magnetic`. These are the maintained art measurements
used for drive thrust, powered-drive requirement, module mass, and material
cost. Values below 1x are intentional. Pulsed/Orion drives are always 1x.

| Hull | V0 | V1 | V2 | V3 |
|---|---:|---:|---:|---:|
| Gunship | 1.000 / 1.000 | 1.277 / 0.902 | 0.668 / 0.397 | 1.596 / 1.327 |
| Escort | 1.000 / 1.000 | 1.277 / 0.902 | 0.668 / 0.397 | 1.596 / 1.327 |
| Corvette | 1.000 / 1.000 | 1.277 / 0.902 | 1.096 / 0.651 | 1.779 / 1.478 |
| Frigate | 1.553 / 1.377 | 1.277 / 0.902 | 1.775 / 1.055 | 2.302 / 1.913 |
| Monitor | 1.000 / 1.000 | 1.277 / 0.902 | 4.219 / 2.624 | 3.917 / 3.255 |
| Destroyer | 1.000 / 1.000 | 2.041 / 1.704 | 4.219 / 2.624 | 3.917 / 3.255 |
| Cruiser | 4.150 / 2.859 | 2.609 / 2.738 | 5.626 / 3.499 | 5.637 / 4.685 |
| Battlecruiser | 2.179 / 2.001 | 3.288 / 3.288 | 5.626 / 3.499 | 5.637 / 4.685 |
| Lancer | 1.715 / 1.715 | 2.847 / 2.900 | 8.543 / 5.313 | 9.831 / 8.170 |
| Battleship | 3.345 / 3.600 | 2.847 / 2.900 | 5.626 / 3.499 | 8.893 / 7.390 |
| Dreadnought | 6.744 / 4.325 | 6.902 / 6.757 | 11.769 / 7.320 | 12.048 / 10.012 |
| Titan | 8.112 / 6.593 | 10.029 / 10.392 | 14.660 / 9.118 | 25.203 / 20.944 |

## Authored empty structural hull masses

Masses are tonnes, exclude crew and installed modules, and are rounded to the
nearest whole tonne. V0 is unchanged by construction.

| Hull | V0 | V1 | V2 | V3 |
|---|---:|---:|---:|---:|
| Gunship | 171 | 187 | 174 | 205 |
| Escort | 338 | 375 | 345 | 406 |
| Corvette | 385 | 599 | 708 | 677 |
| Frigate | 576 | 633 | 802 | 891 |
| Monitor | 679 | 980 | 1,622 | 1,595 |
| Destroyer | 873 | 1,730 | 1,858 | 2,055 |
| Cruiser | 964 | 1,788 | 1,549 | 2,286 |
| Battlecruiser | 1,170 | 2,460 | 1,900 | 3,024 |
| Lancer | 1,958 | 2,472 | 3,848 | 3,865 |
| Battleship | 1,558 | 1,961 | 1,854 | 2,251 |
| Dreadnought | 2,346 | 2,906 | 2,521 | 3,559 |
| Titan | 3,143 | 4,208 | 3,408 | 5,089 |

## Reactor-bay safety invariant

All three paths must use the same drive multiplier:

1. installed-design `drivePowerRequirement_GW`;
2. candidate-drive and candidate-reactor compatibility checks;
3. appearance-change reconciliation, which reduces the thruster count or
   removes a drive if the new appearance cannot power even its x1 variation.

The flat hull-mass lookup is downstream of those checks and does not modify
reactor-bay volume, reactor specific power, or maximum output. Tests cover De
Laval, magnetic, and pulsed candidates across appearance changes, including a
measured scale below 1x.

## Validation and deployment

- Formula coverage passes **1,059 assertions**, including human appearance and
  nozzle selection, sub-1x factors, fixed 1x pulsed drives, all 48 flat human
  appearance masses, and reactor compatibility in both directions.
- The installed-game IL validators pass for the alien armor and alien/STO fuel
  capacity patches. The implementation matrix validates **86 rows**, **23
  settings groups**, and **142 Harmony patches**.
- All **48 reactor-bay measurements**, all 28 hull templates, all 64 graphical
  appearances, both runtime catalogs, and the 66 reference renders validate.
- The release artifact is `artifacts/TIEconomyMod-0.9.2-ti1.0.51.zip`. The
  deployed DLL SHA-256 is
  `FEB4910D517EEABD590DCC8B0CA7C64FDB9C19171E6C7AC5CB0DEF54DD0C3873`.

Manual Ship Designer testing should cycle several appearances on the same human
hull with De Laval, magnetic, and Orion drives. The art pane, module tooltip,
side-panel values, dry mass, acceleration, and reactor compatibility should all
change together; Orion must remain at 1x.
