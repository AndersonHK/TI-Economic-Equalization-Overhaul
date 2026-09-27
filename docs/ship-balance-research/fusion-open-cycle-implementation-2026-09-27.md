# Fusion open-cycle rebalance — 0.10.1

Date: 2026-09-27
Status: implemented and deployed as 0.10.1; manual in-game testing pending.

## Approved scope and plan

The user approved doubling `specificPower_tGW` (tonnes per GW) for all 27
human fusion reactors and setting `cooling: Open` explicitly on all 174 human
and alien fusion drives, including every x1–x6 variant and all Protium drives.
The three alien reactor specific masses retain their existing values.

Use sparse template overrides. Preserve reactor efficiency, maximum output,
crew and materials, and drive thrust, exhaust velocity, efficiency, propellant,
power-generation mode and project requirements. Retain the current open-cycle
thermal accounting, 1% retained-loss parameter and 0.5 default thermal mass
multiplier. This adopts the existing gameplay model; it does not claim those
parameters are experimentally validated for every fusion architecture.

Advance package version, assembly metadata, startup log, verification and
release archive to 0.10.1. Extend the existing ship-rebalance validator to
cover the full fusion population, human specific-mass changes, and preservation
of the existing alien performance overrides. Run `tools/deploy.ps1` with normal
verification immediately after implementation. Its two process checks must
prevent deployment while Terra Invicta is running.

After deployment, update the Excel-readable reactor and drive CSV tables under
`tables/`, recomputing reactor `massAtCap_tons`. Keep the base-game reactor
snapshot `powerplant.csv` as its labeled baseline and update
`powerplant-current.csv` as the effective mod table. Update the affected fusion
rows in `drives.csv` and document that overlay. No separate reactor or drive
XLSX exists in this directory.

## Relationship to the research

The [candidate report](fusion-drive-open-cycle-candidates-2026-09-27.md)
preserves the pre-change evidence. This approved decision includes all its
conditional and fiction-dependent candidates, not only the strongest cases.
The Advanced Alien Fusion Torch previously resolved open through `Calc`; it
now becomes explicitly open independent of mass flow.

Doubling the human specific-mass coefficient does not imply a twofold increase
in fitted ship reactor mass: changing the drive power path also invokes the
existing thermal mass multiplier and changes gross demand. Electrical systems
and weapons still use the electrical conversion path. Radiator and reactor
mass changes should be assessed together in the designer.

## Verification and manual acceptance

Normal `tools/deploy.ps1` completed successfully: all 37 validators and 1,204
formula assertions passed; 47 package files were deployed and hash-verified.
The ship-rebalance validator checks all 27 human reactor coefficients against
twice the installed baseline, mass-only override fields, all 174 explicit Open
drive overrides, and the unchanged alien thrust/EV/efficiency and reactor values.
Package and assembly versions are 0.10.1 and 0.10.1.0 respectively. Release
archive: `artifacts/TIEconomyMod-0.10.1-ti1.0.53.zip`.

The table edits retain their row order and schemas: 27 human rows in
`powerplant-current.csv` have doubled specific mass and recomputed mass at cap;
all 174 fusion rows in `drives.csv` now have Open cooling. The drive table also
refreshes the existing 18 alien thrust/EV/power overrides so its fusion rows
agree with the effective package. The vanilla `powerplant.csv` remains a
comparison baseline. The implementation matrix records the new fusion scope.

Manual testing should inspect a human
fusion ship and an alien ship, check the open-cycle label and reactor mass,
exercise x1/x6 drive variants, and confirm designer mass/heat and saved-ship
performance refresh without errors. Include a Protium drive and a non-fusion
control. No manual result is claimed until observed in game.
