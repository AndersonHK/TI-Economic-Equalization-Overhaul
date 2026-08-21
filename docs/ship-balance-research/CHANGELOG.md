# Ship rebalance planning changelog

This is a decision log for the proposed ship rebalance. Entries here describe
the balance decisions as well as their implementation status.

## 2026-08-20

### Implemented and deployed: open-cycle reactor demand and installed-drive heat consistency

- Model an open-cycle drive's required reactor output as
  `Q = D / (1 - f × (1 - efficiency))`, where `D` is useful drive input after
  hull-art scaling and `f` is the retained-loss fraction. The default `f = 1%`
  sends the other 99% of conversion loss into the propellant while conserving
  `Q = D + retained heat`.
- Make the ship-level drive-power getter the shared authority for plant mass,
  cost, design and live generation demand, waste heat, and localized output.
- Apply the same demand to ship and shipless compatibility, the duplicated
  static AI drive filter, reactor-bay volume, appearance-driven cluster
  reconciliation, and designer power text and sort values.
- Fix the pre-existing combat-burn inconsistency: `DriveHeat_GJ` now uses the
  installed ship-level requirement instead of raw drive-template power, so
  hull-art scaling and open-cycle coupling cannot diverge from cached live
  propulsion generation.
- Audit every installed-assembly use of raw drive power and plant waste heat.
  The `100 GW` fusion-art selector intentionally remains intrinsic/raw; no
  additional demand-bearing consumers remain uncovered.
- Record the formula, patch surface, audit, and manual test procedure in the
  [open-cycle reactor demand and heat report](open-cycle-reactor-demand-and-heat.md),
  and update the implementation matrix without replacing its in-progress
  unrelated entries.
- The TI 1.0.51 deployment passed **1,110 formula assertions**, all **157
  Harmony patches**, the **96-row** implementation matrix, complete release
  verification, packaging, and the **45-file** enabled-mod deployment. DLL
  SHA-256:
  `9C184342CD6842F6B3E1B543D5E77F54A44EDF8D3160EC1361D32BA90F8DA8F7`.
  Manual in-game testing remains pending.

## 2026-08-19

### Implemented and deployed: refit hull-appearance lock

- Extend `TISpaceShipTemplate.IsAValidRefitFor` with a postfix that rejects an
  otherwise-valid refit when its effective `GetHullAppearanceIndex` differs
  from the original design.
- Present `Hull appearance must match.` through the same localized red
  invalid-refit reason path used by vanilla hull, power-plant-class, drive,
  heat-sink, battery, utility, and weapon compatibility failures.
- Keep the rule under EEO's global enable state and compare effective
  appearance indices so vanilla's unavailable-DLC appearance resolution is
  preserved. Existing AI refit generation already retains the original
  effective appearance.
- Record the design and manual test procedure in the
  [refit hull-appearance lock](refit-hull-appearance-lock.md) report and add a
  dedicated row to the current implementation matrix.
- The TI 1.0.51 deployment passed **1,078 formula assertions**, all **144
  Harmony patches**, the **95-row** implementation matrix, release packaging,
  and the **44-file** enabled-mod deployment. DLL SHA-256:
  `D81039EB982F2B2AA75F51428F41302DA2E99B5C4460E6093F37BE844A26B573`.
  Manual in-game testing remains pending.

## 2026-08-18

### Implemented and deployed: held-drive art-cycle crash correction

- Diagnose the reported hard crash from `Player.log`: the stack runs through
  `OnCycleAltHull`, `ReactorBayAppearanceRefreshPatch.RefreshModulePanels`, and
  vanilla `UpdateModuleDataPanel`, which closes the game after a null-reference
  exception.
- Confirm the triggering UI state with the user's reproduction: an unsupported
  drive held by the cursor retains a selected preview but has no legal
  `selectedDragDestination`. The appearance refresh passed `None` as a slot
  type but still invoked vanilla's installed-panel path, which unconditionally
  dereferences `selectedDragDestination.currentPart`.
- Reconstruct the held selection through `SetSelectedShipPartFromMenu` after
  the new art multiplier and availability filter are applied. If the selected
  module still has no destination, return before the unsafe installed-panel
  call.
- When a destination exists, refresh its installed comparison from the
  destination's authoritative `currentPart`; remove the stale
  `currentlyInstalledModule` reflection field.
- Replace validation that merely required two unconditional detail-panel calls
  with guarded IL checks for selection reconstruction, lifecycle ordering, the
  null-destination branch, destination-backed installed state, and exactly one
  safe direct installed-panel call.
- Record the evidence and correction in the
  [art-style module-panel crash report](art-style-module-panel-crash-diagnosis-2026-08-18.md).
- The normal TI 1.0.51 deployment passed **1,078 formula assertions**, all
  **143 Harmony patches**, release packaging, and the **44-file** enabled-mod
  deployment. DLL SHA-256:
  `EC1FD2DB5BAA04D539825758F0D40EA30EA2015EC3F4BFF3FF99474E1DC46EFE`.
  Manual reproduction testing remains pending.

### Implemented and deployed: fission-reactor mass and gas-core capacity progression

- Increase every regular and compact solid-core reactor specific mass by 50%:
  regular I-V now use **240/204/168/72/48 t/GW**, and compact I-V
  (`SolidCoreFissionReactorVI-X`) use **36/30/24/18/12 t/GW**.
- Increase Molten Salt I/II from **10/8** to **15/12 t/GW** and Vapor Core
  I/II/III from **8/6/5** to **9/8/7 t/GW**.
- Establish a rising, display-safe top gas-core capacity ladder: Gas Core IV
  falls from **1,650 to 1,000 GW**, Gas Core V falls from **1,650 to 1,300
  GW**, and Gas Core VI rises from **1,650 to 1,700 GW** while increasing from
  **4 to 5 t/GW**. The rounded caps display as **1.0/1.3/1.7 TW** without
  clipping meaningful precision from the value.
- At a common 1,000 GW load, Gas Core IV/V/VI now weigh **7,000/6,000/5,000
  tonnes**. Their masses at their individual caps are **7,000/7,800/8,500
  tonnes**, so each tier buys both better specific mass and a larger usable
  output envelope.
- Record every before/after value, percentage delta, full-rating mass delta,
  and the resulting family progression in the
  [reactor progression adjustment report](reactor-progression-adjustment-2026-08-18.md).
- The normal TI 1.0.51 deployment passed **1,078 formula assertions**, all
  guarded validation, release packaging, and the **44-file** enabled-mod
  deployment. DLL SHA-256:
  `B8A30A49F23C839DC9878B61CC62349AC1FDA6CA80B4476FD7E38550F1BFFD5D`.
  Manual Ship Designer testing remains pending.

## 2026-08-16

### Implemented and deployed: minimum AI fuel, reactor, and engine-bay enforcement

- Move the existing faction drive-class appearance mapping into provisional AI
  design construction. Each human candidate now locks its deterministic art
  before power-plant selection; aliens use appearance 0 and refits preserve the
  original resolved appearance. The deferred role-aware, top-two randomized
  art-selection design remains documentation only.
- Cap `GetIdealPropellentTankCount` to the legal appearance/propellant capacity
  and recompute `actualDV` at that tank count. Human AI candidates and refits
  are therefore evaluated using performance the completed vessel can attain.
- Bound all direct alien tank assignments and the STO fighter increment. The
  alien's initial delta-v target and recurring 250 kps floor are both limited
  by current maximum achievable delta-v, preventing small ships from chasing
  an impossible tank count until their design-pass limit.
- Enforce reactor size and engine-bay volume through the same shared
  appearance-specific drive/power-plant compatibility methods during early
  plant selection, later drive variation, completed-design validation, fighter
  repair, and the final generated-design save guard.
- Add rocket-equation edge tests, guarded installed-game IL validation for the
  alien/STO tank paths, and Harmony application coverage for all seven new AI
  boundary patch classes.
- The normal TI 1.0.51 deployment passed **1,059 formula assertions**, all
  **142 Harmony patches**, release packaging, and the **35-file** enabled-mod
  deployment. DLL SHA-256:
  `FEB4910D517EEABD590DCC8B0CA7C64FDB9C19171E6C7AC5CB0DEF54DD0C3873`.
- Manual human-AI, alien, refit, and STO generation testing remains pending.

### Implemented and deployed: measured human drive art and flat variant masses

- Replaced the provisional human hull-only drive factors with the measured
  De Laval and magnetic factor for every graphical appearance. Pulsed/Orion
  drives remain fixed-size and fixed-mass at **1x**.
- The art-selection overlay now reports the same drive factors used by thrust,
  powered-drive requirement, module mass, material cost, and reactor
  compatibility logic.
- Added explicit flat empty structural masses for all **48 human hull
  appearances**. No main-volume, engine-bay, reactor-bay, or component-derived
  structural mass formula is applied; installed non-pulsed drive hardware is
  the only component weight scaled by the drive-art factor.
- Verified **1,059 formula assertions**, **142 Harmony patches**, all **48
  reactor-bay measurements**, all 28 hull templates, all 64 appearances, and
  both runtime geometry catalogs.

### Implemented and deployed: Solid Core Fission Reactor I capacity restoration

- Raise `SolidCoreFissionReactorI.maxOutput_GW` from **1 GW to 2 GW**.
- Retain the current **160 t/GW** specific mass and **57.5%** efficiency. The
  plant therefore reaches **320 t** at its full 2 GW rating; installations
  below the cap remain sized from their actual gross power requirement.
- This restores the installed vanilla output ceiling while preserving the
  mod's mass and efficiency rebalance. Reactor-bay geometry remains an
  independent effective-output limit.
- The normal TI 1.0.51 deployment passed **1,059 formula assertions**, all
  **142 Harmony patches**, the complete ship/reactor validation suite, release
  packaging, and **35-file** enabled-mod deployment. DLL SHA-256:
  `FEB4910D517EEABD590DCC8B0CA7C64FDB9C19171E6C7AC5CB0DEF54DD0C3873`.

### Implemented and deployed: density-aware hull-volume fuel capacity

- Added a runtime catalog for all **28 hull templates** and **64 graphical
  appearances**, generated from the maintained main-hull measurements. The
  selected art now directly controls the usable fuel envelope.
- Implemented the requested capacity order:
  `ceil(max(0, hull volume - module volume - total crew * 50 m3))`, followed by
  a whole-tank floor using the drive propellant's density and the vanilla
  **100-ton** tank mass.
- Default module allowances are **200 m3 per utility cell**, **250 m3 per hull
  weapon cell**, and **400 m3 per nose weapon cell**. Utility footprints use
  the multi-slot registry and weapons use their vanilla internal size.
- Added density defaults for liquid hydrogen, water, liquid xenon, liquid
  methane, liquid lithium, and the water-equivalent broad propellant classes.
  An optional `propellantDensity_kgm3` drive-template extension supports exact
  drive-family overrides without changing saves or the game's propellant enum.
- Clamp tank count before every designer refresh and again before saving. This
  covers drive changes, both art-change methods, module/crew changes, tank
  edits, and design loading. The propellant spinner displays current / maximum.
- Added a two-line overlay to the 3D model pane. Follow-up visual testing
  corrected the hull line to show independently measured De Laval/Magnetic art
  scales, the selected appearance's measured engine-bay volume, hull mass, and
  the hull template's base repair crew. The fuel line retains propellant,
  current/maximum tanks, and usable fuel volume; capacity continues to reserve
  the complete fitted design crew.
- Added the implementation report, compact runtime CSV, generator/export path,
  catalog reconciliation, settings/package validation, implementation-matrix
  entry, and **10 fuel formula assertions**.
- After the display correction, the normal TI 1.0.51 deployment passed **1,056
  formula assertions**, all **135 Harmony patches**, all **48 reactor-bay
  measurements**, the complete **64-appearance** volume/drive-scale catalog,
  release packaging, and **35-file** enabled-mod deployment. DLL SHA-256:
  `10B3B2BF6F6FF54FE2BE6CF5D2463A17CE94A108BEC586A958609D3F9FDCE901`.
- Manual Ship Designer testing of the corrected De Laval/Magnetic scales,
  measured bay volume, repair crew, and drive/art/module transitions remains
  pending.

### Documented and deployed: complete hull-appearance volume and slot inventory

- Added a generated inventory of all **28 installed human and alien hull
  templates** and all **64 graphical appearances**, including the special
  `STOFighter` and `SalamanderGunship` hulls. Each appearance has side and top
  orthographic renders, main-hull mesh bounds and an elliptical-envelope volume,
  nose/hull/utility slot counts, and exact source-model identity.
- The main-hull selection excludes the drive subtree, named radiator/reactor-bay
  meshes, and separately named engine, thruster, and reactor meshes. The report
  describes the result as an exterior comparison envelope rather than occupied
  or usable interior volume; `STOFighter` is explicitly marked inseparable.
- Added the maintained generator, CSV, JSON mesh-path evidence, 64 thumbnails,
  two contact sheets, documentation links, and a deployment validator that
  reconciles source hashes, installed template/model-resource coverage, slots,
  positive measurements, machinery exclusions, and all **66 PNGs**.
- A second complete asset run regenerated all **69 report artifacts
  byte-for-byte**. The normal TI 1.0.51 deployment then passed **938 formula
  assertions**, all **130 Harmony patches**, the full validation suite, release
  packaging, and **33-file** enabled-mod deployment. DLL SHA-256:
  `6840F6CE9450D64C896CE00C3446B7B7BCDDA233158DFF5287AAA23768F610FD`.
- This is evidence and tooling only: no hull or utility-slot gameplay value was
  changed. A normal mod-load regression smoke test remains pending.

### Implemented and deployed: coil and alien magnetic tier progression cadence revision

- Correct the technology handoff so Coilgun Mk1 strictly exceeds the matching
  Railgun Mk2 in range and modeled sustained damage, and Coilgun Mk2 strictly
  exceeds Railgun Mk3. All 18 regular and siege comparisons now pass.
- Scale all human coil and alien magnetic muzzle velocities by `1.25`, rounded
  to `0.1 km/s`; scale range by `1.25` and floor it to the nearest `50 km`;
  inter-salvo reload is unchanged.
- Apply a 60% intra-salvo reduction to human light coils with
  `ceil(previous intra-salvo × 0.40)`. Apply a 40% reduction to every other
  human coil and alien magnetic weapon with
  `ceil(previous intra-salvo × 0.60)`.
- Require every affected intra-salvo interval to be no longer than its own
  inter-salvo reload, and every Coil I/II interval to be no longer than the
  mapped Rail II/III inter-salvo reload, including siege coils.
- Require the modeled sustained damage of every regular Coilgun Battery mark to
  be more than twice its Light Coilgun Battery peer. Light Coilgun Battery Mk3
  damaging mass changes from 11 to 10 kg to clear the Mk3 threshold without
  changing the locked speed or range.
- Record the full 49-row original/proposed/delta analysis in
  [`magnetic-tier-progression-rework.csv`](tables/magnetic-tier-progression-rework.csv)
  and the detailed rationale in the
  [magnetic tier-progression report](details/weapons/magnetic-tier-progression-rework.md).
- The cadence revision passed **938 formula assertions**, all **130 Harmony
  patches**, the full TI 1.0.51 validation suite, packaging, and **33-file**
  enabled-mod deployment. Deployed DLL SHA-256:
  `6840F6CE9450D64C896CE00C3446B7B7BCDDA233158DFF5287AAA23768F610FD`.
- The deployed magnetic template is byte-identical to the repository template
  (SHA-256 `09870E5530E7B51FD8D9D08DA985B17C7E20B98E547651DE7B4269A2C0C49176`).
  Manual combat testing remains pending.

## 2026-08-13

### Third correction deployed: clamp on the actual art-cycle action

- Manual testing proved that the second correction prevented the null-reference
  crash but did not reconcile the installed cluster: x5 remained installed on
  art whose bay supports only x3, and decrement produced a confirmation sound
  followed by no state change.
- The failure was a lifecycle-target error. The arrow buttons invoke
  `OnCycleAltHull`, while the correction patched `SetAltHull`, which is used by
  template loading. Reconcile after both appearance mutation paths, with the
  interactive `OnCycleAltHull` path as the required acceptance target.
- Remove the null-module prefix from `SetModuleInSlot`. Silently cancelling an
  action after vanilla has played its confirmation sound is not valid behavior
  and is not a substitute for maintaining a valid installed drive.
- After clamping or removing the drive, force-refresh template caches, the ship
  performance panel, both module-table collections, and the selected/installed
  module detail panels. Appearance-dependent volume and output values must
  never remain stale after an art change.
- Guarded validation now requires both `OnCycleAltHull` and `SetAltHull`, the
  installed-count `GetVariation` path, the normal replacement/removal paths,
  cache/performance/filter refreshes, and both module-detail refreshes. The
  null-suppression prefix is explicitly forbidden.
- The correction passed **906 formula assertions**, all **123 Harmony patches**,
  and the full TI 1.0.51 validation suite, then deployed **33 files**. Deployed
  DLL SHA-256:
  `83553DF2FD25F8393D7BCE939F8DADED5506FC31247C1AE04CFC9DC68CA897FD`.

### Second correction deployed: reconcile drive clusters after appearance changes

- Reproduced the invalid state: a larger-bay appearance can install a drive
  cluster that remains serialized after cycling to smaller art; decrementing
  that now-invalid cluster enters a vanilla designer path that assumes the
  currently installed drive is valid and can crash.
- On `SetAltHull`, clamp the installed drive to the largest valid variation no
  larger than its current thruster count. If the new bay cannot fit even the
  x1 variation, remove the drive through the designer's normal slot-removal
  path.
- Replace the diagnostic-style contextual bay block with two presentation
  rows: `Reactor bay volume used / available` and `{used} / {available} m³`.
  Keep effective-output comparison behavior internal to selection.
- Verified and deployed against TI 1.0.51 with **906 formula assertions**, the
  guarded reconciliation IL requiring exactly one normal replacement and one
  normal removal path, all **123 Harmony patches**, and **33 deployed files**.
  The deployed DLL SHA-256 is
  `9D41A29288BC873CE0A9961AD9665ADAC8E4459ABB9155C0BF9D59CBF8A8E4BE`.
- Manual retest showed that the first correction still retained x5 and crashed
  when vanilla attempted x4. `Player.log` confirms the null reference remains
  in `ShipModuleDragDestination.OnDecreasePressed` -> `SetModuleInSlot`, while
  no reconciliation error was emitted. Replace the indirect patched-predicate
  check with a direct demand/effective-output comparison after `SetAltHull` has
  committed the new appearance and refreshed the panel.
- The installed method commits `hullAppearanceIndex` before its panel refresh
  and postfix. Keep the compatible `SetAltHull` hook, but directly compare each
  variation's hull-scaled demand with `ReactorBayCapacityFeature` effective
  output instead of recursively consulting the patched vanilla predicate.
- Add a defensive null-module prefix to `SetModuleInSlot`; the crash log showed
  that unavailable x4 was arriving as null, which vanilla and the utility
  footprint postfix could dereference. Null selections are now ignored safely.
- The second correction passed **906 formula assertions**, explicit `SetAltHull`
  target-IL validation, direct-capacity/replacement/removal IL validation, all
  **123 Harmony patches**, and **33-file** deployment. Deployed DLL SHA-256:
  `9161DA31B26B4F72E1F7071AD0F062E13F967639081EC55E652C0F4109AFE695`.

### Implemented: graphical-variant reactor-bay capacity

- Measure all four human graphical appearances and key reactor-bay capacity by
  `(hull dataName, resolved appearance index)` rather than statistical hull
  class. Preserve the full implementation contract and future hull-size
  dependency in
  [the reactor-bay capacity plan](reactor-bay-capacity-implementation-plan.md).
- Use size-class maximums only as explicit, diagnosed fallbacks for unmeasured
  alien, third-party, or future pairs.
- Limit both final drive/plant compatibility directions to the minimum of
  theoretical reactor output and graphical-bay output. Preserve hull-less
  catalogue and research scoring, loaded designs, and vanilla weapon/auxiliary
  semantics.
- Add contextual power-plant descriptions, effective-output module-table rows,
  and an explicit refresh when the player cycles the graphical hull appearance.
- Extend the asset tool for base and Dark Skies bundles and maintain all 48
  source measurements in `reactor-bay-variant-volumes.csv`.
- Release validation passed against TI 1.0.51 with **903 formula assertions**,
  **123 Harmony patches**, all 48 measured pairs, guarded module-table and
  compatibility IL, packaging, and **33-file** enabled-mod deployment. Manual
  designer verification remains the final visual/interaction check. The final
  four post-deployment assertions explicitly lock the Frigate x4/x6 acceptance
  boundary and do not alter the deployed assembly.

### Documented: rebalanced fission mass against measured reactor bays

- Refresh the reactor-bay planning report with the doubled fission specific
  masses, revised efficiencies, and the new explicit gas-core overrides.
- Use Solid Core Fission Reactor V as the focused hull-fit example: measured bay
  volume limits Gunship, Escort, Corvette, and Frigate; the `60 GW` plant output
  limits Monitor and every larger hull.
- Record the highest-power fitting Solid V drive configuration for all twelve
  human hulls after applying the implemented hull drive multipliers. This is a
  planning calculation only; no reactor-bay gameplay restriction is added.
- Add a Molten Salt Fission Reactor II + Pegasus follow-up. The runtime
  compatibility exception makes the pairing valid; measured bay volume binds
  through Destroyer, while reactor output binds Cruiser and larger hulls. Under
  the current assumptions, no hull can use Pegasus x6 without exceeding either
  its bay allowance or the plant's `400 GW` output cap.

### Implemented follow-up: heavier, less efficient molten-salt reactors

- Set Molten Salt Fission Reactor I–II efficiency to `0.725` and `0.750`.
- Increase their `specificPower_tGW` to `10` and `8`, producing maximum-rated
  masses of `400 t` and `3,200 t` at the unchanged `40 GW` and `400 GW` caps.
- Release validation passed against TI 1.0.51 with **745 formula assertions**,
  **121 Harmony patches**, settled ship-rebalance validation, release
  packaging, and **33-file** enabled-mod deployment.

### Implemented: heavier fission-reactor progression and lower conversion efficiency

- Interpret the requested flat `2.5%` efficiency reduction as **2.5 percentage
  points** (`efficiency - 0.025`). This applies to Solid Core I–V, Compact Solid
  Core I–V, Molten Core I–III, and Molten Salt I–II.
- Double `specificPower_tGW` for those 15 reactors and for Vapor Core I–III.
- Set Vapor Core I–III efficiency to `0.87`, `0.88`, and `0.89`.
- Set Gas Core I–VI efficiency to `0.87`, `0.89`, `0.91`, `0.92`, `0.93`, and
  `0.94`, with `specificPower_tGW` of `20`, `16`, `10`, `7`, `6`, and `4`.
- Preserve maximum-output caps. Add a derived `massAtCap_tons` column to both
  power-plant CSVs, calculated as `maxOutput_GW × specificPower_tGW`. Keep
  `powerplant.csv` as the installed vanilla snapshot and update
  `powerplant-current.csv` with the live override-merged values.
- Release validation passed against TI 1.0.51 with **745 formula assertions**,
  **121 Harmony patches**, settled ship-rebalance validation, release
  packaging, and **33-file** enabled-mod deployment.

### Corrected: gas-core thermodynamic reference and research artifact location

- Record the installed component localization's explicit gas-core maximum of
  `25,000 °C` (`25,273 K`) rather than treating only NASA's `10,000–20,000 K`
  concept-study range as the available temperature reference.
- At an `800 K` sink, the localized maximum gives a **96.83% Carnot ceiling**
  and an **82.21% Curzon–Ahlborn reference**.
- Keep persistent generated research artifacts under `docs/`; reserve
  `outputs/` for disposable build or run products.

### Implemented: expanded horizontal 2x1 utility slice

- Return ISRU Module to its native 1x1 footprint.
- Add horizontal 2x1 footprints to Repair Bay, Salvage Bay, Spartans, Rangers,
  Immortals, Component Armor, automated Solar/Fission Platform Kits,
  Salamander Terror Unit Pod, Alien Army Pod, Alien Fusion Platform/Outpost
  Kits, Alien Repair Bay, Alien Surveillance Orbital, and Alien Surveillance
  Ring.
- Keep 2x2 balance assignments deferred. The runtime remains capable of 2x2
  occupancy, but this slice uses only 1x1 and horizontal 2x1 assignments.
- Render top-strip utility icons by scaling the icon child inside its
  layout-controlled catalog cell so a 2x1 module is visibly wide before it is
  selected or dragged.
- Release validation passed against TI 1.0.51 with **745 formula assertions**,
  **121 Harmony patches**, exact validation of all **30 utility footprint
  declarations**, release packaging, and **33-file** enabled-mod deployment.
  The deployed DLL SHA-256 is
  `EBC53087189CB18D53967B81C263F3FC574EE12C60F768493A643FAE0F874082`.

### Implemented: hull-weapon-equivalent catalog behavior and 2x1 expansion

- Multi-slot catalog eligibility now tests only the selected hull's slot
  geometry, not its current occupancy. A two- or four-cell part remains
  selectable after compatible cells are filled, matching hull weapons; a hull
  that cannot geometrically support the footprint greys it to the native 30%
  alpha and disables dragging. Actual drop legality still requires every cell
  in the resolved footprint to be empty.
- Catalog, selected-detail, drag, and installed graphics now reshape the
  existing game icon to advertise its footprint before placement. Horizontal
  parts are wide, vertical parts are narrow, and four-cell parts retain a
  square frame with a subtle 2x2 divider. No replacement icon art is included.
- Added horizontal 2x1 footprints to Flag Bridge, all Marine Assault Units, all
  six manual Solar/Fission/Fusion Platform and Outpost Kits, the automated
  Solar/Fission Outpost Kits, ISRU Module, and the six human heavy heat sinks.
  `MobileSpaceScienceLab` remains the horizontal 2x1 canary. Hull slot layouts
  remain untouched for the separately tracked hull-layout work.
- Validation now guards the hull-geometry/occupancy split, list-item alpha and
  icon patch surfaces, the complete utility and heat-sink assignment lists,
  and preservation of the original icon resources.
- Catalog footprint resizing is re-applied in the final list-item alpha pass,
  after module assignment and list layout, so the top module strip now displays
  the footprint rather than only the selected and installed views.
- Cyclotron remains explicitly single-slot. Its vanilla particle-beam-support
  prerequisite is ignored only while validating prospective placement, allowing
  the support module to be installed before a particle-beam weapon without
  bypassing any other native design rule.
- Release validation passed against TI 1.0.51 with **745 formula assertions**,
  **120 Harmony patches** in the implementation matrix, release packaging, and
  **33-file** enabled-mod deployment. Live designer and save/reopen behavior
  remain the manual acceptance gate.
- The horizontal-orientation and Cyclotron follow-up passed **745 formula
  assertions**, **121 Harmony patches**, release packaging, and **33-file**
  enabled-mod deployment. Top-strip rendering and live designer behavior remain
  the manual acceptance gate.

## 2026-08-12

### Implemented: multi-slot utility footprint runtime (playable 2x1 slice)

- Utility templates can now declare fixed `Single`, `TwoHorizontal`,
  `TwoVertical`, or `Four` footprints through the mod-owned
  `utilityFootprint` field. A design still stores one functional module at one
  anchor; every secondary cell is derived and resolves back to that anchor.
- Dragging a multi-cell utility onto any cell of a compatible empty utility
  region resolves a deterministic legal anchor. The designer blocks the full
  footprint, removes it as one part, and uses the game's native multi-slot
  frame while stretching the module's existing game icon to fill it. Removal
  explicitly restores the anchor's original size and position because native
  `SetEmpty()` does not undo the multi-slot position offset.
- The designer's native `ValidPartForDesign` catalog predicate was extended for
  multi-slot utilities. The initial occupancy-sensitive implementation was
  corrected on 2026-08-13 to match hull-weapon geometry-only availability.
- `MobileSpaceScienceLab` is the first playable canary and occupies a
  horizontal 2x1 pair. Four-cell runtime support is present, but no hull-layout
  changes are included in this slice.
- AI-selected utility lists are repacked into legal hull anchors. Existing
  designs that cannot accommodate newly enlarged utilities retain legacy 1x1
  interpretation instead of silently losing or moving parts.
- Full deployment validation passed against TI 1.0.51 with **744 formula
  assertions**, **117 Harmony patches** in the implementation matrix, guarded
  utility target-surface validation, release packaging, and **33-file**
  enabled-mod deployment. Live designer, save/reopen, and construction checks
  remain the manual acceptance gate.

### Implemented: graphical-variant and alien drive scaling

- Mod initialization now rolls back every Harmony patch from the EEO owner if
  any patch class fails, then disables the mod and logs the initialization
  error. This avoids leaving a silently partial balance patch set active.
- Extended the drive lookup with alien status, DLC-aware resolved graphical
  appearance, and the candidate or installed drive's nozzle family. Only alien
  hulls consume the new graphical factors. Human ships retain the approved
  pre-pass hull factors for every appearance and nozzle family: Cruiser 1.30,
  Battlecruiser 1.50, Lancer 1.72, Battleship 1.75, Dreadnought 2.00, and Titan
  2.50, with smaller hulls at 1.00.
- All fourteen standard alien hulls were measured from their standalone x1
  drive resources. Alien factors range from **1.000** for Gunship, Escort, and
  the below-baseline Corvette to **7.531** for Titan and **26.216** for
  Mothership. Alien resource naming is hull-specific and ignores nozzle family,
  so the measured factor is shared by magnetic and De Laval physics. The
  Salamander has no standalone alien drive prefab and retains **1.000**.
  Invalid alien appearances, unknown hulls, and this missing Salamander
  measurement emit one-time configuration errors that include the safe
  fallback value. Intentional authored 1.00 values do not produce errors.
- Scaled thrust now reaches live `TISpaceShipState.currentThrust_N`, not only
  design-template acceleration. The ship designer refreshes its cached drive
  table when the designer re-filters modules, and both the selected-module side
  panel and hover tooltip display scaled thrust, combat thrust, required power,
  drive mass, and material cost. Exhaust velocity and efficiency remain
  unchanged.
- Extended the maintained prefab-measurement script without changing its hull
  measurement path. Alien missing-resource results are recorded explicitly, and
  the measurement method and complete alien variant table are maintained in
  the hull/drive report.
- Full deployment validation passed against TI 1.0.51 with **731 formula
  assertions**, **111 Harmony patches** in the implementation matrix, guarded
  drive-display transpiler and live-description patch validation, release
  packaging, and **32-file** enabled-mod deployment.

### Implemented in 0.9.2: conservative alien propulsion and armor allocation

- Alien drive thrust becomes **1.2 / 3.8 / 10.5 MN**, exhaust velocity becomes
  **1,200 / 2,350 / 3,000 km/s**, and installed drive efficiency remains
  unchanged at **95 / 97 / 98%**.
- Alien reactor maximum output receives a flat **+400%** increase to **5,000 /
  32,000 / 107,550 GW**. Specific mass is halved to **0.50 / 0.175 / 0.025
  t/GW**, while efficiency becomes **99.5 / 99.8 / 99.95%**.
- The alien designer's armor-density numerator changes from **3,500** to
  **10,000 kg/m3**, requesting about **2.86x** its prior armor points for the
  same material. All existing role delta-v targets remain unchanged.
- Matching-tier reactor caps exceed six-drive electrical demand by roughly
  **9.9% / 15.9% / 11.5%**. Strict reactor selection and predefined-loadout
  normalization remain separate follow-up work.
- Full deployment validation passed against TI 1.0.51: **699 formula
  assertions**, **107 Harmony patches** in the implementation matrix, guarded
  `DesignAlienShip` IL validation, release packaging, and **32-file**
  enabled-mod deployment.

### Implemented in 0.9.1: alien magnetic progression

- Added the
  [alien magnetic, propulsion, and armor-design proposal](alien-weapons-propulsion-and-armor-proposal.md),
  based on installed templates layered with the mod's current magnetic
  overrides.
- Alien tier 1 receives modest velocity increases where its lead over human
  Mk1 is only marginal. Alien tier 2 receives the missing velocity and range
  leads over human Mk3 plus **75% efficiency**. Alien tier 3 receives monotonic
  range corrections plus **85% efficiency**; its already extreme projectile
  masses and velocities are retained.
- The half-nose Alien Mini Light Mag Cannon follows the tier-1 light-cannon
  velocity at **6.0 km/s** while retaining its existing range and efficiency.
- Release validation locks all **22 alien magnetic rows** and proves strict
  tier-1-over-human-Mk1 and tier-2-over-human-Mk3 dominance for projectile
  mass, damaging mass/durability, velocity, range, and efficiency.
- Full deployment validation passed against TI 1.0.51: **699 formula
  assertions**, 106 Harmony patches in the implementation matrix, guarded
  target-IL checks, ship-rebalance validation, release packaging, and
  **31-file** enabled-mod deployment.

### Proposed: matched alien propulsion progression and armor headroom

- The proposed drive ladder is **1.2 / 3.8 / 10.5 MN**, **1,200 / 3,600 /
  10,800 km/s**, and **98 / 99 / 99.9%** efficiency. The matched reactor caps
  are **5,000 / 42,000 / 350,000 GW**, with **0.10 / 0.014 / 0.0018 t/GW**
  specific mass and **99.5 / 99.8 / 99.95%** efficiency.
- Better templates do not by themselves repair reactor undersizing.
  Predefined designs retain explicit reactor assignments, while dynamic power-
  plant selection does not explicitly require output cap to meet design load.
  Both paths require pairing validation.
- Stronger propulsion creates physical armor headroom, but the current alien
  designer may exit before spending it and may stop filling all armor when one
  facing caps. A future implementation may correct that allocator while
  preserving every existing performance floor.

### Explicitly deferred: lower alien AI delta-v targets

- Retain the existing **800 / 900 / 600 / 200 / 300 km/s** role-group targets.
- The previously explored half-target concept is not part of the active
  proposal. Any reduction requires a later update after propulsion-first armor,
  generated-design, combat, and campaign evidence.

## 2026-08-10

### Implemented: remaining human-hull crew and rounded empty masses

- **One base crew per weapon or utility slot:** Cruiser, Battlecruiser, Lancer,
  Battleship, Dreadnought, and Titan now carry **12 / 10 / 14 / 14 / 18 / 19**
  base crew respectively. Together with the previously changed six hulls, all
  twelve standard human combat hulls now follow the same balance rule.
- **Three-tonne support allowance retained:** hull masses become **964 / 1,170 /
  1,958 / 1,558 / 2,346 / 3,143 t**. Adding three tonnes per base crew billet
  produces rounded hull-plus-crew empty masses of **1,000 / 1,200 / 2,000 /
  1,600 / 2,400 / 3,200 t**.
- **Geometry unchanged for the new six:** this pass does not override their
  length, diameter, or stored volume. The earlier Gunship-through-Destroyer
  model-informed geometry remains in force.

### Implemented: provisional hull-specific drive capacity

- **Human-hull multipliers:** Cruiser **1.30**, Battlecruiser **1.50**, Lancer
  **1.72**, Battleship **1.75**, Dreadnought **2.00**, and Titan **2.50**.
  Gunship through Destroyer and all alien hulls remain at **1.00**.
- **Constant exhaust velocity:** ship thrust, physical mass flow, powered-drive
  electrical demand, drive hardware mass, and drive material cost scale by the
  hull multiplier. Exhaust velocity and therefore delta-v per unit propellant
  remain unchanged. The game has no hull-level burn-duration/fuel-flow state,
  so mass flow is the physical `thrust / exhaust velocity` consequence rather
  than a separate runtime consumption variable.
- **Dependent burdens retained:** higher drive power feeds the existing reactor,
  waste-heat, and radiator calculations. Drive construction and refit costs
  include the larger hardware, and existing reactor maximum-output
  compatibility checks use the scaled drive demand.
- **Appearance-sensitive follow-up audited:** the selected appearance index,
  hull utility capacity, base and complete crew, rounded empty hull mass, and
  runtime cylinder volume are all reachable from the existing ship and hull
  templates without a save-format change. A future implementation should use
  stable hull properties for the class factor, a bounded normalized appearance
  modifier, and must not use free utility slots or patched complete dry mass.
- **Reactor caps deferred:** no hull-size reactor mass/output cap and no reactor
  template change are included in this pass.
- **Configuration:** `shipBalance.hullDriveScalingEnabled` independently returns
  the new runtime scaling to vanilla while leaving the other ship-balance
  features active.

### Documented: hull density, naval references, hardpoints, and drive assets

- Added the consolidated
  [human hull and drive-scaling report](human-hull-slots-and-drive-scaling.md),
  including per-hull and per-tier tonnes/slot, volume/slot, tonnes/crew, and
  volume/crew tables; Arleigh Burke, Ticonderoga, Zumwalt, Type 45, and Iowa-
  class context; hardpoint and multi-slot feasibility; the drive-asset method;
  nozzle-area ratios; and the reference Meteor x6 thrust-to-mass table.
- Moved the maintained UnityPy measurement program to
  `scripts/ship-balance/measure_ship_prefabs.py`. Its original hull measurement
  function remains independent of the added drive-resource and individual-
  nozzle functions.
- Full deployment validation passed against TI 1.0.51: **667 formula
  assertions**, 101 Harmony patches in the implementation matrix, guarded
  target-IL checks, ship-rebalance validation, package build, and enabled-mod
  deployment.

## 2026-08-04

### Implemented: complete chemical ammunition and nose-autocannon cadence

- **Large-caliber projectile progression:** retain the settled **40 / 90 /
  180 kg** damaging masses for the 6-, 8-, and 10-inch guns and implement the
  planned **320 kg** 12-inch projectile. The sequence remains within 5.1% of
  cubic caliber scaling from the 6-inch reference.
- **Complete ammunition mass:** increase `ammoMass_kg` to **90 / 200 / 400 /
  640 kg** for the 6-, 8-, 10-, and 12-inch guns. These values cover the
  projectile, an energy-scaled propellant charge for the shared 1.4 km/s
  muzzle velocity, and cartridge or bag-handling allowance. They affect loaded
  weapon mass and ammunition cost, not empty weapon mass or impact damage.
- **35mm nose autocannon:** change complete/damaging masses from **10 / 5.5 kg
  to 8.8 / 2.6 kg**, retain its four-shot salvo, and change intra/inter-salvo
  timing from **0.5 / 4.0 s to 0.4 / 1.75 s**. Its cycle becomes 2.95 seconds,
  effective cadence becomes 81.36 rpm, and sustained kinetic output becomes
  7.051 MJ/s (**-11.9%** versus vanilla).
- **40mm nose autocannon:** change complete/damaging masses from **10 / 6 kg
  to 8.8 / 2.8 kg**, retain its four-shot salvo, and change intra/inter-salvo
  timing from **0.5 / 4.0 s to 0.4 / 1.75 s**. Its cycle becomes 2.95 seconds,
  effective cadence becomes 81.36 rpm, and sustained kinetic output becomes
  12.833 MJ/s (**-13.0%** versus vanilla).
- **Unchanged scope:** salvo sizes, muzzle velocities, ranges, empty weapon
  masses, crew, and electrical values are unchanged. The existing hull 30mm
  and 40mm values remain as implemented on 2026-08-02.

## 2026-08-02

### Implemented: revised 30mm and 40mm CIWS cadence and mass

- **30mm:** retain the 10-shot salvo, **5.5 / 1.75 kg** complete/damaging
  projectile masses, 1.35 km/s velocity, and **0.25 s** intra-salvo spacing;
  round the inter-salvo reload to **1.0 s**. The 3.25-second total cycle yields
  **184.62 effective rpm** and **4.907 MJ/s** sustained kinetic output.
- **40mm ETC:** retain the six-shot salvo, 2.6 km/s velocity, and settled
  8.7 MJ/shot electrical-work value. Reduce complete/damaging projectile mass
  to **8.8 / 2.8 kg**, exactly 1.6 times the 30mm values; use **0.5 s**
  intra-salvo spacing and **1.75 s** inter-salvo reload. The 4.25-second total
  cycle yields **84.71 effective rpm** and **13.361 MJ/s** sustained output.
- **Role comparison:** the revised 40mm retains 53.4% more sustained kinetic
  output than the one-hull 6-inch cannon, but only 24.1% of its impact per shot
  while requiring 13.647 MW average electrical input. Salvo sizes, ranges,
  projectile velocities, weapon empty masses, and the 30mm power value remain
  unchanged.

### Implemented: coil and alien-mag projectile housekeeping

- **Projectile mass, not weapon mass:** all 27 human coil weapons and all 22
  alien magnetic weapons receive approximately **25% heavier projectiles**,
  rounded to whole kilograms. Complete mass and damaging/durability mass move
  together while preserving each projectile's vanilla composition as closely
  as whole-kilogram rounding permits. Gun/mount empty mass is unchanged.
- **Approximately 20% shorter total cycles:** every scoped weapon retains its
  vanilla salvo size and intra-salvo delay. Only the inter-salvo reload is
  reduced, producing actual total-cycle changes from **-18.8% to -20.8%** after
  whole-second rounding.
- **Alien velocity retained:** the previously implemented alien muzzle-velocity
  increase remains in force and combines with the mass and cadence changes.
- **Rails unchanged:** the staged human rail values are not altered by this
  housekeeping slice; the other 12 human rail rows remain vanilla.
- **Coverage:** the magnetic override now contains exactly 9 staged human rails,
  27 human coils, and 22 alien mags. Validation requires exact fields and values,
  preventing accidental salvo/intra-salvo overrides or damaging mass greater
  than complete projectile mass.

## 2026-08-01

### Implemented staged projectile and magnetic-weapon test slice

- **Chemical projectiles:** use **40 / 90 / 180 kg** damaging masses for the
  6-, 8-, and 10-inch guns while retaining their vanilla ammunition mass,
  salvo size, intra-salvo delay, and reload. Use **1.75 kg at 180 rpm** for the
  30mm and **3 kg at 100 rpm** for the hull 40mm; complete rounds remain at
  their vanilla mass.
- **Human rails:** change only the light rail battery, rail battery, and light
  rail cannon Mk1-Mk3 projectile masses and reloads. Complete projectile mass
  is **14 / 30 / 37.5 kg** by mount family; reloads are **8 / 6 / 4 s**,
  **12 / 9 / 6 s**, and **16 / 12 / 8 s**, respectively. Damaging mass retains
  each mark's vanilla damaging-to-complete-mass ratio. Salvo size and
  intra-salvo delay remain vanilla, so these railguns remain single-shot for
  this test.
- **Alien mags:** apply only the settled muzzle-velocity increase to all 22
  alien magnetic weapons. Their projectile mass, salvo, delay, reload, range,
  power, and efficiency remain vanilla.
- **Explicit exclusions:** human coils, all other human rail families, chemical
  ammunition mass, and the proposed rail salvos remain deferred for campaign
  testing.

### Field-test observation: staged human rails

- **Current rails are already decisive without salvos:** ships using the staged
  light rail battery, rail battery, and light rail cannon values destroyed their
  opposing ships easily. Their longer targeting range and higher projectile
  velocity were tactically decisive even though all three families retained
  vanilla one-shot salvos.
- **Salvo proposal rejected:** the planned 3 / 4 / 5-shot rail salvos are no
  longer required and should not be implemented. The next conservative test
  should retain the staged projectile masses and move reloads back toward
  vanilla before considering any other rail or coil change.

### Field-test observation: direct-fire saturation fallback

- **Long-range rails can saturate the entire enemy fleet before impact:** the
  direct-fire commitment system distributed rail projectiles across every
  hostile ship while the hostile fleet was still outside its own engagement
  range. Once every target crossed the expected-overkill threshold, automatic
  acquisition rejected all of them and the rail ships stopped firing. The
  in-flight projectiles did not actually destroy every ship, leaving surviving
  targets alive without continued fire.
- **Saturation must be a preference, not a firing prohibition:** automatic
  direct-fire targeting should prefer an eligible unsaturated target. If no
  eligible unsaturated target exists, it should fall back to an eligible
  saturated target and continue firing. Saturation alone must never reduce
  expected damage to zero or leave a weapon without a target when saturated
  enemies remain available.
- **Scope remains direct fire only:** this fallback is intended for plentiful
  gun, rail, coil, and plasma projectiles. Existing missile and torpedo
  saturation behavior remains unchanged.

### Implemented: direct-fire saturation as a priority malus

- **Lexicographic target priority:** saturation is now the first automatic
  direct-fire priority key rather than a binary eligibility gate. While any
  eligible unsaturated enemy ship exists, saturated ships are skipped. When
  every eligible enemy ship is saturated, all remain candidates and vanilla
  Weakest, Closest, or Strongest ordering selects the highest-priority one.
- **Continuous fallback fire:** expected direct-fire damage is suppressed on a
  saturated target only while an eligible unsaturated ship exists. If all
  eligible ships are saturated, guns, rails, coils, and plasma continue firing
  at the selected saturated target until no eligible targets remain.
- **Intent and missile isolation:** deliberate player primary targets still
  bypass automatic suppression. Missile weapons, missile-boat acquisition,
  missile saturation, and torpedo salvo behavior remain unchanged.
- **Coverage:** formula tests exercise mixed and all-saturated fleets; guarded
  IL validation requires both target acquisition and fire suppression to query
  the shared unsaturated-target scan while retaining the three vanilla target
  selection loops.

### Implemented: magnetic projectile geometry and durability coupling

- **All magnetic families covered:** every human railgun, human coilgun, siege
  coiler, and alien magnetic weapon now receives a physical collider diameter
  and cross-sectional area through the same generic geometry path used by
  chemical projectiles.
- **Mass-derived default:** an explicit `projectileDiameter_mm` template value
  remains authoritative. Without one, magnetic diameter is derived from
  `ammoMass_kg` as a 10:1 length-to-diameter tungsten-equivalent kinetic body
  at **19,300 kg/m3**. This produces a monotonic cube-root relationship, so
  future complete-projectile-mass edits automatically resize both the collider
  and physical cross section without duplicating values across template rows.
- **Durability remains natively coupled:** Terra Invicta already calculates a
  magnetic projectile's remaining effective mass as `warheadMass_kg` minus
  accumulated mass damage. Increasing damaging projectile mass therefore
  increases magnetic projectile durability by the same percentage. The
  comparison table now exposes both values and deltas so future mass proposals
  can keep damage and durability synchronized.
- **Coverage:** formula tests guard zero handling, the 10 kg reference diameter,
  and monotonic mass scaling. Runtime IL validation requires the explicit-data
  override, mass-derived magnetic fallback, and the game's
  `warheadMass_kg - massDamage_kg` durability relationship.

### Implemented in 0.8.4: skirmish roster and gun-lookup performance

- **Shared option catalogs:** main-menu skirmish rows now build localized ship
  options once per controller, language, import set, and alien-eligibility
  variant. Reinitialized rows retain their private option lists when the
  catalog is unchanged instead of rebuilding every design entry.
- **Vanilla behavior retained:** displayed combat scores, alien/import colors,
  newly imported design selection, add-row notification behavior, tooltips,
  damage images, and fleet-score calculation stay on their vanilla paths.
- **Safe invalidation:** changed ship/import counts or identities, language,
  and the imported-design setter invalidate the catalog. Disabling Ship Balance
  returns `PopulateShipDropdown` to the complete vanilla implementation.
- **Allocation-free gun fast path:** generic gun power data is resolved against
  every loaded `TIGunTemplate` during hydration and stored by reference.
  Ordinary `selfPowered`, energy, and heat getter calls no longer sort scenario
  tags or construct string keys. Unexpected dynamic templates retain the
  original scenario-aware lookup fallback.
- **Coverage:** deterministic tests require 10,000 stable row-cache accesses to
  invoke the option builder once and 100,000 identity lookups to retain the
  hydrated value. Runtime IL validation guards the vanilla refresh call chain,
  catalog-builder isolation, import invalidation, and both Harmony targets.

### Field-test observations after 0.8.3

- **Automatic gun allocation succeeded:** in repeated unattended skirmishes,
  ten 10-inch gunships now defeat ten 6-inch escorts reliably. The result
  reverses the earlier artificial advantage created by projectile collisions
  and whole-fleet target saturation.
- **Early combat triangle:** current testing supports the intended broad
  relationship `missiles > guns > point defense > missiles`. Missiles remain
  extremely effective, 10-inch guns are now dominant against competing gun
  ships, and point defense retains its anti-missile role.
- **Mk1 magnetic weapons remain weak:** none of the projectile-collision or
  automatic-target-allocation changes made the human Mk1 railgun family
  materially competitive. Its balance remains an open, separate problem.
- **Open performance regression:** adding ships in the main-menu skirmish
  roster now produces a large delay that grows with roster size. Static
  inspection confirms that every add performs a full
  `StartMenuController.PopulateSkirmishDropdowns` rebuild; every existing row
  then repopulates the complete ship-design dropdown. See
  [the skirmish roster performance investigation](skirmish-roster-performance-investigation.md).

### Implemented in 0.8.3: direct-fire commitment targeting

- **Automatic fire allocation:** live ballistic projectiles now reserve their
  vanilla expected direct damage against their intended ship. Ordinary Offense
  acquisition and nonmissile combat AI skip a target once aggregate committed
  damage exceeds the same structural-integrity and total-armor threshold used
  by vanilla missile saturation.
- **Projectile lifecycle:** commitments are registered only after an actual
  shot, removed on destruction, collision, impact, or battle cleanup, and
  pruned when a projectile is no longer moving toward its intended target.
  Per-target pruning is limited to once per rendered frame.
- **Player intent:** player-controlled deliberate primary targets remain
  authoritative. Focus, Bracket, Defense, Guardian, Idle, and Salvo behavior is
  not redirected by the automatic-fire gate.
- **Missile isolation:** missile target selection, saturation estimation,
  point-defense discounting, 15-second retarget timing, and quarter-ammunition
  salvo behavior are unchanged. Missile-carrier AI retains its vanilla target
  eligibility path.
- **Generic ballistic support:** the shared path covers conventional guns,
  magnetic weapons, and plasma without caliber or template identifiers. No
  projectile mass, velocity, cadence, damage, ammunition, or weapon power value
  changes in this release.

### Implemented in 0.8.2: TI 1.0.51 compatibility and gun-table load fix

- **Late-save initialization:** powered 0.8.1 gun rows conditionally gained an
  Energy Usage cell while the 35mm, nose 40mm, and 12-inch rows remained
  self-powered and one cell shorter. TI's ship-module table assumes every
  visible row has the same cell count, so late saves with a mixed unlock set
  failed in `ShipModuleTable.ResizeColumns` with a repeating
  `ArgumentOutOfRangeException`.
- **UI compatibility:** retain the intended five powered gun families and their
  balance values. During module-row construction only, conventional guns that
  still consume zero energy now participate in the Energy Usage column and
  display their real zero value. Gameplay power, battery, heat, and self-powered
  behavior are unchanged.
- **Retarget:** build and validate against the installed Terra Invicta 1.0.51
  assemblies. All guarded gameplay/UI IL anchors and the new module-row
  transpiler match the installed binary.
- **Physical projectile geometry:** add generic `projectileDiameter_mm` data
  for all eight conventional-gun templates: **30, 35, 40, 40, 152.4, 203.2,
  254, and 304.8 mm**. A fired projectile's collider now uses that diameter at
  the same cinematic scale as ship models instead of retaining the shared
  `BulletAutocannon` or `BulletGun` collider dimensions. The same data supplies
  physical cross-sectional area to beam damage and targeting calculations.
- **Collision sweep:** reduce the ballistic forward-ray sweep from **120% to
  100%** of movement during the update. The guarded transpiler requires exactly
  one matching TI 1.0.51 constant.
- **Projectile durability:** conventional-gun projectiles now accumulate the
  game's already-randomized direct plus chipping damage at **100 kg of effective
  projectile mass per damage point**. Destruction occurs when accumulated mass
  damage reaches `warheadMass_kg`. A head-on 6-inch hit comfortably neutralizes
  a 10-inch round, while a 30mm hit does not do so by itself. The impacting
  projectile still destroys itself through the existing impact path; no
  post-impact trajectory or deflection state is simulated.
- **Future magnetic support at the time:** projectile-diameter hydration was
  shared between `TIGunTemplate` and `TIMagneticGunTemplate`, but no magnetic
  geometry was activated in 0.8.2. The mass-derived implementation recorded
  above now supersedes that deferral.

### Implemented in 0.8.1: powered guns and coherent thermal accounting

- **Gun power only:** activate the existing powered-weapon path for the 30mm,
  40mm, 6-inch, 8-inch, and 10-inch guns through generic `powerUse_MJ` data.
  Retain every vanilla 1.0.49 mass, projectile, ammunition, velocity, range,
  damage, cooldown, and salvo value. At vanilla cadence, use **0.085, 8.70,
  0.675, 1.40625, and 2.20 MJ useful work per rendered shot**, respectively.
  The ordinary chemical guns retain their inherited **100%** efficiency until
  loader-loss values are settled; the 40mm uses **90%**, drawing **9.6667 MJ**
  and producing **0.9667 MJ local heat** per rendered shot.
- **Generic hydration:** retain the new field in a load-ordered runtime registry
  sourced from active `TIGunTemplate` mod JSON. The C# behavior contains no gun
  identifiers, supports scenario tags, observes full-template replacement, and
  works whether Unity Mod Manager starts before or after template initialization.
- **Reactor output and heat:** auxiliary generation now credits the ship power
  pool with net electrical output rather than pre-efficiency reactor input.
  Rejected plant heat is electrical output times
  `(1 - plant efficiency) / plant efficiency` and is applied once when power is
  generated. The redundant continuous systems-heat application is suppressed.
- **Weapon heat and radiators:** radiator design load now adds each powered
  weapon's own `HeatGeneration_GJ` at the same cooldown or intra-salvo interval
  used by vanilla generator sizing. This applies uniformly to beams, magnetic
  guns, plasma weapons, and newly powered conventional guns.
- **Heat gate:** the retracted/destroyed-radiator pre-fire check now reserves
  only the module heat that `FireWeapon` will actually apply. It no longer treats
  total reactor input energy as instantaneous weapon heat.
- **Save compatibility:** no custom value is serialized. Active gun power data
  is rebuilt before save selection; the game's existing post-load power recache
  consumes it. Loaded ship mass is reconciled to recalculated template dry mass
  plus saved propellant and propulsion values are marked dirty.

### Settled conservative gun projectiles and revised magnetic cadence

**Implementation status:** the chemical-projectile subset is implemented by the
staged test slice above. The broader magnetic cadence proposal recorded below
remains historical planning and is not the runtime configuration.

- **6-, 8-, and 10-inch projectile mass:** settle the conservative ends of the
  evidence-led bands at **40 kg, 90 kg, and 180 kg**, respectively. At the
  retained 1.4 km/s muzzle velocity these produce **39.2, 88.2, and 176.4 MJ**
  per rendered projectile. The 10-inch remains the largest relative improvement
  over vanilla.
- **Conventional naval-gun salvos retained:** keep the vanilla **4, 4, and 3
  shots per salvo**, **2, 2.5, and 3 s** intra-salvo spacing, and **12, 15, and
  16 s** between-salvo cooldowns for the 6-, 8-, and 10-inch guns. A single
  barrel can represent a small ready-service ammunition group followed by a
  longer magazine-handling cycle; retaining it also preserves the useful visual
  rhythm of naval-gun fire. The earlier one-shot candidate is rejected and is
  not to be implemented.
- **30mm Autocannon cadence:** retain the settled **1.75 kg** damaging packet
  and increase the target to **180 rendered rounds per minute** so that the
  projectile-CIWS pass does not inadvertently strengthen already-powerful
  early missiles. With ten-shot salvos and 0.25 s intra-salvo spacing, a
  cooldown of approximately **1.0833 s** produces exactly 180 rpm.
- **40mm Autocannon cadence:** retain the settled **3 kg** damaging packet and
  increase the target to **100 rendered rounds per minute**. With six-shot
  salvos and 0.375 s intra-salvo spacing, a cooldown of **1.725 s** produces
  exactly 100 rpm.
- **Human Mk1-Mk2 rail and coil cadence:** reduce both `cooldown_s` and
  `intraSalvoCooldown_s` by about **66%**, rounded to whole seconds. Rail-battery
  cooldowns become **30 / 20 -> 10 / 7 s** for Mk1/Mk2; rail-cannon cooldowns
  become **45 / 30 -> 15 / 10 s**.
- **Human Mk3 rail and coil cadence:** start from a **40%** reduction, then
  tighten only values needed to preserve each vanilla mark-to-mark improvement
  by at least one second. Rail batteries become **10 -> 6 s** and rail cannons
  **15 -> 9 s**. Coil-battery cooldowns become **13 / 10 / 9 s** across Mk1-3;
  ordinary coil-cannon and siege-coiler cooldowns become **16 / 12 / 11 s**.
- **Intra-salvo detail:** ordinary coil batteries use **3 / 3 / 6 s** and
  ordinary coil cannons **4 / 4 / 7 s** across Mk1-3. Vanilla does not improve
  these spacings between marks, so the Mk3 40% target is retained. Siege
  coilers do improve in vanilla and therefore use **12 / 8 / 7 s**, preserving
  at least a one-second improvement at each mark.
- **Magnetic scope boundary:** this cadence pass changes no non-cadence proposal
  field and no alien magnetic-weapon row. The proposal CSV contains typed
  numeric cells without stray leading apostrophes.
- **Conventional-gun electrical-load targets:** plan average auxiliary loads at
  the upper ends of the engineering bands: **0.100 MW** for the 30mm,
  **0.150 MW** for the 6-inch, **0.250 MW** for the 8-inch, and **0.300 MW** for
  the 10-inch. These loads represent feeds, autoloaders, mount machinery,
  controls, and cooling rather than chemical muzzle energy.
- **40mm ETC electrical convention:** represent the 3 kg packet's kinetic-energy
  increment from a 1.0 km/s chemical reference to 2.6 km/s, plus its auxiliary
  load, as **8.70 MJ useful work per rendered projectile**. Set the inherited
  powered-weapon efficiency to **90%**. At 100 rpm this means **9.6667 MJ
  electrical input per projectile**, **16.111 MW average electrical input**, and
  **0.9667 MJ local weapon heat per projectile** (**1.611 MW average**). This is
  a futuristic balance abstraction for controlled propellant combustion, not
  an experimentally established energy budget.
- **Gun-power implementation status:** superseded by the 0.8.1 implementation
  above. The power fields and shared thermal fixes are implemented; the staged
  chemical projectile and cadence subset is now active, while all remaining
  ammunition, mass, and cadence proposals stay deferred.

## 2026-07-31

### Planned conventional-gun mass and projectile pass

**Implementation status:** planning decision only; not implemented. No weapon
template values are changed by this entry. Exact values not stated below remain
subject to the railgun-progression and ammunition-endurance reviews.

- **Weapon empty mass:** bring the conventional-gun base-mount masses toward
  plausible empty automated-mount values. Keep empty mount mass distinct from
  ammunition mass when setting and reporting the targets. The evidence review
  does not support forcing the already-light 6-inch and 8-inch base mounts
  below their present 25 t and 50 t merely to make every caliber lighter; their
  final values remain open pending a consistent mounting-and-recoil model.
- **10-inch Cannon empty-system mass:** use approximately **145 t** as the
  evidence-led empty weapon-system target. This target refers to the complete
  mount, autoloader, recoil structure, and associated machinery before
  ammunition; it is not a loaded-mass target. Loaded mass must be calculated by
  adding the separately revised magazine and complete-round mass. This
  supersedes the earlier **110 t empty / 137.6 t loaded** working example in
  `low-tech-rebalance-slice.md`.
- **6-, 8-, and 10-inch projectile hierarchy:** increase effective damaging
  projectile mass toward real full-caliber comparisons instead of retaining
  the compressed vanilla values of **22.5, 50, and 90 kg**. Use approximate
  planning bands of **40-45 kg, 90-115 kg, and 180-230 kg**, respectively. Give
  the 10-inch Cannon the largest relative benefit so that it becomes the
  decisive single-hit weapon of the chemical-gun family. Final damage, rate of
  fire, magazine, and loaded-mass values remain to be reconciled together.
- **30mm Autocannon projectile and cadence:** halve effective damaging
  projectile mass from **3.5 to 1.75 kg** and double its rate of fire. With the
  current ten-shot salvo structure, the direct cadence translation is
  **0.5 to 0.25 s** intra-salvo spacing and **4 to 2 s** cooldown. This retains
  approximately the same sustained kinetic throughput while halving damage per
  rendered projectile and doubling the number of armor interactions.
- **40mm Autocannon projectile and cadence:** halve effective damaging
  projectile mass from **6 to 3 kg** and double its rate of fire. With the
  current six-shot salvo structure, the direct cadence translation is
  **0.75 to 0.375 s** intra-salvo spacing and **4 to 2 s** cooldown. This also
  retains approximately the same sustained kinetic throughput while making the
  abstraction a smaller packet of physical 40mm rounds.
- **Autocannon ammunition accounting:** reassess complete-round mass and
  magazine depth separately from damaging projectile mass. Halving
  `warheadMass_kg` does not by itself settle `ammoMass_kg`, and the present
  3,000- and 2,000-shot magazines should not be carried forward automatically
  when the rendered-projectile packet size and cadence change.
- **Gun electrical demand:** conventional chemical guns should not be charged
  for muzzle kinetic energy as reactor output. A later code-supported pass may
  assign autoloader, mount-drive, and cooling loads; the 40mm
  electrothermal-chemical weapon should additionally pay for its electrically
  assisted velocity gain and pulse-power losses.

## 2026-07-29

### Settled for the first low-tech slice

**Implementation status:** applied in Economic Equalization Overhaul 0.7.4.
The four deferred starting engines remain unchanged. Items under “Still under
research” remain unimplemented; in particular, this release does not adopt a
hull-material recipe or change the vanilla crew construction-resource package.

- **Apex, Meteor, Neutron, and Venture:** defer all changes. The four starting
  engines remain at their current values until the wider propulsion progression
  is reviewed.
- **Water Heat Sink and Heavy Water Heat Sink:** set module crew to **0** in the
  eventual rebalance. Monitoring and maintenance belong to the ship's shared
  engineering crew.
- **10-inch Cannon:** reduce module crew from **4 to 3** in the eventual
  rebalance. The deliberately conservative abstraction is one commander, one
  shooter, and one loader.
- **Point-defense guns:** use **0 dedicated module crew** for both projectile
  and laser point-defense mounts. In this narrow slice, apply and table that
  decision only for the **30mm Autocannon: 1 → 0 crew**. Reload, maintenance,
  and supervision remain ship-level burdens.
- **Solid Core Fission Reactors I–V:** reduce efficiency by five percentage
  points in the eventual rebalance:
  - I: **75% → 70%**
  - II: **77.5% → 72.5%**
  - III: **80% → 75%**
  - IV: **82.5% → 77.5%**
  - V: **85% → 80%**
- **Compact Solid Core Fission Reactors I–V** (template identifiers
  `SolidCoreFissionReactorVI–X`): apply the same five-percentage-point
  reduction:
  - Compact I: **77.5% → 72.5%**
  - Compact II: **80% → 75%**
  - Compact III: **82.5% → 77.5%**
  - Compact IV: **85% → 80%**
  - Compact V: **87.5% → 82.5%**
- **Fuel Cells I–III:** set efficiencies to **63%, 65%, and 67%**,
  respectively.
- **Fuel Cells I–III:** set specific mass to **2.8, 1.8, and 0.48 kg/kW**,
  respectively. In the template's `specificPower_tGW` field these are
  **2,800, 1,800, and 480 t/GW**.
- **Crew support mass:** reduce the global allowance from **4 t to 3 t per
  crew member**. This remains a bundled abstraction rather than consumables
  alone.
- **Open-cycle drive cooling:** an open-cycle drive must retain a nonzero
  radiator burden; the vanilla 100% drive-heat exemption is not acceptable.
  NERVA component data support a value below 5%. The current research-led
  implementation draft is **1% of the drive-associated heat that the corrected
  closed-cycle formula would otherwise send to radiators**.
- **Gunship:** adopt **55 m length × 15 m diameter**, giving a cylindrical
  planning volume of **9,719 m³**. Set the hull to **171 t**; three crew at
  the settled allowance produce **180 t empty mass**.
- **Escort:** adopt **62 m length × 15 m diameter**, giving a cylindrical
  planning volume of **10,956 m³**. Set the hull to **338 t**; four crew
  produce **350 t empty mass**.
- **Corvette:** adopt **65 m length × 17 m diameter**, **14,754 m³** planning
  volume, **5 crew**, and **385 t hull mass**, producing **400 t empty mass**.
- **Frigate:** adopt **100 m length × 18 m diameter**, **25,447 m³** planning
  volume, **8 crew**, and **576 t hull mass**, producing **600 t empty mass**.
- **Monitor:** adopt **100 m length × 17 m diameter**, **22,698 m³** planning
  volume, **7 crew**, and **679 t hull mass**, producing **700 t empty mass**.
- **Destroyer:** adopt **100 m length × 23 m diameter**, **41,548 m³** planning
  volume, **9 crew**, and **873 t hull mass**, producing **900 t empty mass**.
- **Hull volume data:** use the calculated cylindrical volumes for the planning
  `volume` values as well as the runtime geometry, while noting that the
  installed compiled class currently ignores the JSON `volume` key.

### Settled for the second slice

**Implementation status:** incorporated into Economic Equalization Overhaul
0.7.5; the later reactor refinement below supersedes these interim efficiency
values.

- **Molten reactors I–V:** reduce efficiency by five percentage points. The
  five installed TI 1.0.49 modules are split between the Molten Salt and Molten
  Core families, so record them by their exact template identities:
  - Molten Salt Fission Reactor I (`MoltenSaltFissionReactorI`):
    **92% → 87%**
  - Molten Salt Fission Reactor II (`MoltenSaltFissionReactorII`):
    **93% → 88%**
  - Molten Core Fission Reactor I (`MoltenCoreFissionReactorI`):
    **85% → 80%**
  - Molten Core Fission Reactor II (`MoltenCoreFissionReactorII`):
    **88% → 83%**
  - Molten Core Fission Reactor III (`MoltenCoreFissionReactorIII`):
    **90% → 85%**
- **40mm Autocannon** (`40mmAutocannon`): reduce module crew from **1 to 0**.
  Reloading, maintenance, and engagement supervision remain shared ship-level
  duties.
- **Point Defense Laser Turret** (`PointDefenseLaserTurret`): reduce module
  crew from **2 to 0**. This is the first laser point-defense mount and follows
  the previously settled rule that point-defense fire-control loops do not
  receive dedicated operators.

### Settled for the third slice

**Implementation status:** incorporated into Economic Equalization Overhaul
0.7.5, except for the explicitly deferred alien-fleet work. The weapon-crew
values are current; the later reactor refinement below supersedes these
interim power-plant efficiencies.

- **Fuel Cells I–III:** reduce the first-slice efficiencies by another five
  percentage points:
  - Fuel Cell I: **63% → 58%**
  - Fuel Cell II: **65% → 60%**
  - Fuel Cell III: **67% → 62%**
- **Solid Core Fission Reactors I–V:** reduce the first-slice efficiencies by
  another five percentage points:
  - I: **70% → 65%**
  - II: **72.5% → 67.5%**
  - III: **75% → 70%**
  - IV: **77.5% → 72.5%**
  - V: **80% → 75%**
- **Compact Solid Core Fission Reactors I–V** (template identifiers
  `SolidCoreFissionReactorVI–X`): reduce the first-slice efficiencies by
  another five percentage points:
  - Compact I: **72.5% → 67.5%**
  - Compact II: **75% → 70%**
  - Compact III: **77.5% → 72.5%**
  - Compact IV: **80% → 75%**
  - Compact V: **82.5% → 77.5%**
- **Molten reactors:** reduce the second-slice efficiencies by another five
  percentage points:
  - Molten Salt Fission Reactor I: **87% → 82%**
  - Molten Salt Fission Reactor II: **88% → 83%**
  - Molten Core Fission Reactor I: **80% → 75%**
  - Molten Core Fission Reactor II: **83% → 78%**
  - Molten Core Fission Reactor III: **85% → 80%**
- **6-inch Gun Battery** (`6-inchCannon`): reduce crew from **3 to 2**.
- **8-inch Gun Battery** (`8-inchCannon`): reduce crew from **4 to 2**.
- **Light Railgun Batteries Mk1–3** (`LightRailgunBatteryMk1–3`, one hull
  hardpoint): reduce crew from **3 to 2**.
- **Railgun Batteries Mk1–3** (`RailgunBatteryMk1–3`, two hull hardpoints):
  reduce crew from **4 to 2**.
- **Light Rail Cannons Mk1–3** (`LightRailCannonMk1–3`, the one-nose
  successors to the 10-inch Cannon): reduce crew from **4 to 3**.
- **Alien fleet rebalance:** defer numeric and loadout changes to a future
  slice. The working hypothesis is that early missile saturation exploits weak
  alien laser point defense, while later fleets are held back by old magnetic
  weapons and invalid or weak reactor/drive pairings. The future slice should
  test stronger laser point-defense behavior, proper Advanced/Gen3 magnetic
  weapons, adequate power plants, and revised module placement together rather
  than applying a uniform faction-wide stat buff.

Supporting analyses are archived by topic:

- [reactor thermodynamics and fuel inventory](details/reactors/thermodynamic-and-fuel-limits.md);
- [reactor structural scaling and output caps](details/reactors/structural-scaling-and-output-caps.md);
- [fuel-cell catalysis, specific power, and thermal limits](details/fuel-cells/catalysis-power-density-and-thermal-limits.md);
- [gun and railgun progression](details/weapons/gun-and-railgun-progression.md);
- [alien fleet and module audit](details/aliens/fleet-and-module-audit.md).

### Settled reactor mass-and-output refinement

**Implementation status:** applied in Economic Equalization Overhaul 0.7.5.
These values supersede the earlier reactor targets where they overlap.

- Reduce the efficiency of every touched solid, compact-solid, molten-salt, and
  molten-core reactor by another five percentage points:
  - Solid I–V: **60%, 62.5%, 65%, 67.5%, 70%**
  - Compact Solid I–V: **62.5%, 65%, 67.5%, 70%, 72.5%**
  - Molten Salt I–II: **77%, 78%**
  - Molten Core I–III: **70%, 73%, 75%**
- Double reactor specific mass:
  - Solid I–V: **80, 68, 56, 24, 16 t/GW**
  - Compact Solid I–V: **12, 10, 8, 6, 4 t/GW**
  - Molten Salt I–II: **4, 3.6 t/GW**
  - Molten Core I–III: **8, 7, 6 t/GW**
- Double Fuel Cell I–III specific mass as part of the same overall
  specific-mass pass: **5,600, 3,600, and 960 t/GW**, equivalent to
  **5.6, 3.6, and 0.96 kg/kW**. This clarification changes fuel-cell specific
  mass only; their output-cap and stored-energy questions remain open.
- **Fuel-cell mass-accounting boundary:** power-plant specific mass includes
  the fuel-cell and regenerative hardware plus the solar arrays described by
  the localization. Radiator panels are not included in power-plant mass.
  Efficiency determines rejected heat and therefore the separate ship
  radiator requirement.
- Set the current reactor maximum-output targets:
  - Solid I–V: **1, 3, 10, 30, 60 GW**
  - Compact Solid I–V: **0.75, 2, 4, 6, 10 GW**
  - Molten Salt I–II: **40, 400 GW**
  - Molten Core I–III: **4, 17, 200 GW**
- Molten Salt II is deliberately capped at **400 GW** because Pegasus Drive
  x6 requires approximately **395 GW**.
- With those output caps and specific masses, plant mass at maximum output is:
  - Solid I–V: **80, 204, 560, 720, 960 t**
  - Compact Solid I–V: **9, 20, 32, 36, 40 t**
  - Molten Salt I–II: **160, 1,440 t**
  - Molten Core I–III: **32, 119, 1,200 t**

### Still under research

- Fuel Cells I–III maximum output and whether to expose stored energy/recharge;
  their doubled specific masses are settled
- finer sequence adjustments to the current solid, compact-solid, molten-salt,
  and molten-core efficiency, output, and specific-mass targets
- reactor volume and crew requirements
- Diana, Nerva, and Kiwi drives
- Lithium-Ion Battery
- Water and Heavy Water Heat Sink capacity and mass
- 10-inch Cannon mass, magazine, projectile, velocity, and firing cycle
- 30mm Autocannon mass, magazine, projectile, velocity, and firing cycle; its
  zero-crew decision is already settled
- 40mm Autocannon mass, magazine, projectile, velocity, and firing cycle; its
  zero-crew decision is already settled
- Point Defense Laser Turret mass, power, efficiency, optics, range, and firing
  cycle; its zero-crew decision is already settled
- 6-inch and 8-inch Gun Battery performance and ammunition; their two-crew
  decisions are already settled
- Light Railgun Battery, Railgun Battery, and Light Rail Cannon performance,
  power, heat, and ammunition; their crew decisions are already settled
- alien laser point-defense effectiveness, magnetic-weapon tier allocation,
  reactor/drive pairing, armor, and module placement; all alien changes remain
  deferred pending controlled combat tests and more campaign experience
- whether module volume should eventually be enforced or remain an audit-only
  planning metric
- whether the Frigate's active radiator collider extending behind the visible
  ship is a live-combat hitbox defect that should receive a separate prefab fix
- correction of the power-plant waste-heat formula
- whether the draft 1% open-cycle residue should be raised for balance, plus
  how to represent shutdown decay heat
- whether hull visual scale, prefab hit colliders, and statistical
  length/width should remain coupled or receive separate mod-side controls
- hull construction-material composition and the exact 3 t/crew construction
  package; the current research candidate is a metal-forward, mass-conserving
  split, but it is not yet a settled decision
