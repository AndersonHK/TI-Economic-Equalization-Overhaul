# Ship designer edit performance plan

2026-11-21. Source and prefab audit of Terra Invicta 1.0.65 with EEO 0.11.15.
The reported symptom is a stall of over one second on some large-ship part
edits. The paths below are verified from the installed game assembly, the
installed UI bundle, and the mod source. Their individual wall times still
need a live capture. This is a plan for the edit path.

## First implementation and headless baseline

The Unity batch-mode baseline edited the Salvage Bay on a valid Dreadnought
with eight utilities, eleven weapons and 1,256 catalog widgets. The 0.11.15
no-route median was 960 ms for removal and 1,942 ms for installation. The
traced no-route install spent 1,956 ms in two catalog passes, including
34,216 full reactor snapshots. Combat score took 25 ms. With the configured
Earth route, two synchronous transfer searches took 19 ms combined. Stage
times are inclusive.

The first candidate keeps automatic native combat scoring and transfer
results. Each edit publishes once. The running part totals change at actual
native list writes. Fuel capacity uses occupancy and crew totals, and the
native mass and cost functions consume the installed parts subtotal. Reactor
allowance uses a hull, appearance and plant lookup. Installed reactor sizing
reuses the previous result when its inputs stay the same. The shared native
planner and score still run within the edit on changed inputs and count
toward the timing target. Nothing is queued for a later frame in this pass.

EEO 0.11.16 passed normal release verification and the deployment script
verified all 59 installed files against the source package. Installed DLL
SHA-256 is `7CA473158929C9C38917AA4FECA6F6D18E3324E8D39BC671B76B2F0A6FDB3388`.
The reactor boundary assertions include changing the plant's rated output
in place, so a cached allowance must observe changes to plant data as well
as appearance or plant identity.

### Matched headless results

| Salvage Bay edit | 0.11.15 median | 0.11.16 median | 0.11.16 maximum |
| --- | ---: | ---: | ---: |
| Remove, route unset | 960.2 ms | 17.9 ms | 18.4 ms |
| Install, route unset | 1,942.0 ms | 18.4 ms | 25.3 ms |
| Remove, route configured | 935.0 ms | 51.3 ms | 57.8 ms |
| Install, route configured | 1,951.7 ms | 47.5 ms | 55.5 ms |

These are warmed handler timings through the real designer methods in the
Unity host. There were five uninstrumented installs and six removals per
route condition, plus one traced install. Both versions used the same save
copy and route. Forced canvas work in the edit frame fell from medians of
29–31 ms to 2.5–4.1 ms. The next three headless frames per edit reached at most
10.9 ms, compared with 10.4 ms at baseline. The candidate schedules no background
or next-frame refresh. Rendered frame time still requires manual measurement.
[Raw samples and stage counts](ship-designer-edit-performance-headless-2026-11-21.json).

The traced no-route install invoked one panel update, zero full catalog passes
and zero full reactor capacity snapshots. Combat score took 18.0 ms of that
19.4 ms handler. The traced configured install invoked one native transfer
search at 9.2 ms, and its handler took 28.2 ms. The higher uninstrumented
route medians show that a single traced solve understates route variance.
The under-1-ms goal remains open. Even the forced canvas update exceeds that
goal on this fixture. The next implementation needs to reduce native score,
shared trajectory search, and UI publication costs.

The warmed test round trip preserved the design. Additional remove/add cases
on Salvage Bay, Magazine, and Heavy Ion Battery compared cached mass, heat
capacity, battery capacity, delta-v, acceleration and every resource against
native full recalculation. All passed. The capacity fixture clipped 132 tanks
to 131 with matching final cost. The test covered a populated human capital;
larger alien hulls, other routes, refits and rendered attachment changes still
need manual coverage.

### Save-load startup failure

The 0.11.16 player log showed `Use Mods:False` after the game recreated its
profile. Unity Mod Manager still loaded the EEO DLL. EEO applied Harmony
patches before validating the stock hull templates, which fail EEO's layout
validation at AlienCruiser. The startup catch left the template refresh hook
installed. It threw again during campaign template initialization, before the
save was read.

The fix makes native template initialization the activation boundary. EEO
will wait until Terra Invicta has merged the installed package's hull override
and remove its runtime hooks and serializer processor when returning to stock
templates. Hull layout validation runs before runtime patch installation, and
failed initialization rolls back the runtime patch owner. [Runtime lifecycle](../../TIEconomyMod/Main.cs#L35).

0.11.17 passed release verification and deployed 59 files. The installed DLL
SHA-256 is `C22A1A2D2C2F78D1D70588FB0FB14D82D386F89DEF1393967676F8D52F52BD52`.
The repaired live profile enabled native mods with all other bytes preserved
at the time of the repair.

An isolated Unity batch-mode run started with native mods off and
reinitialized the stock templates without the EEO designer hooks or the
AlienCruiser error. It then enabled mods in the same process and completed
loading a copy of the 2044-11-09 exit save. The active EEO designer hooks
and the Dreadnought edit and fuel checks passed after load. The saved
campaign used for the fixture was left intact.

### Rendered acceptance and transfer-planner return

The player accepted 0.11.17's save loading and edit performance. Combat
score, reactor, power and arrival-time readouts updated appropriately in
their manual test. The reported remaining regression is a missing ship
model after closing the designer's transfer planner. Changing the hull
appearance brings back the model.

Native designer exit destroys the camera and its model. The return callback
reopens the already-selected designer tab with `update: false`, which skips
the normal preview creation. Clear the references when the camera is retired
and use the native preview builder if the designer is reopened without a
camera or model. Keep the selected appearance, editable design, and existing
background. Verify the round trip through the actual Fleets and Intel screens.

0.11.18 passed normal release verification and deployed 59 files. In isolated
Unity batch mode, the designer's Transfer button and Intel's close button
restored the Dreadnought preview both after the old camera had been destroyed
and when closing within the same frame. Both cases preserved appearance index
2, the editable template and the existing background. Reopening the designer
while its preview was healthy reused the existing camera and model. The
headless run verified the object hierarchy and active renderers. The player
then confirmed the ship art stayed visible after closing the transfer
planner in 0.11.18 and accepted this checkpoint. The installed DLL and
release archive match the tested SHA-256
`7593241B5EEE26C26BA6894D160E4432530B16B231E3C7F66658312D72DE6939`.
[Preview lifecycle hooks](../../TIEconomyMod/Patches/ShipDesignerViewportPatches.cs#L228).

## What runs on each edit

`FleetsScreenController.SetModuleInSlot` runs `RemoveModuleFromSlot` for a
utility install, including an empty slot. A weapon install runs it for every
cell returned by `WeaponSlotSet`. Removal then recalculates construction cost,
dry mass, heat capacity and battery capacity, updates the installed-part and
ship panels, optionally repopulates the role dropdown, filters the entire
catalog, and updates the transfer preview. The add path repeats the cost and
mass refresh, ship panel, catalog, role and transfer work after placement.
For a regular utility or single-cell gun that means **two** cost refreshes,
catalog passes and transfer update attempts. For a weapon covering `F` cells
it means **`F + 1`** under the normal install path. The native suppression
flag applies to combat score during nested removal. The full catalog and
transfer work still run with that flag set. A removal of an already empty
slot also runs the refresh tail. Updating selection after install additionally
refreshes the part details and walks all destination widgets to set highlights.

| Work | Scaling and trigger |
| --- | --- |
| Transfer duration preview | With configured endpoints and an existing result, `UpdateTransferInfo` force-reads cruise acceleration and calls `UpdateTransferPlannerParameters`. This assigns acceleration and delta-v input text one field at a time. The installed Intel screen binds **both** `onValueChanged` events to `TransferPlanner.UpdateThrustProfile`. Each changed field can run `ThrustProfileTool.ComputeCandidateTrajectories` → `MasterTransferPlanner.RequestTrajectories` synchronously, then sort and refresh the trajectory UI. Two changed fields can therefore solve twice, first using the new acceleration with the previous delta-v. Equal strings short-circuit in TMP. Without an active preview the designer takes the early no-transfer path. Solver cost depends on route, candidate search and flight constraints as well as repeated design-stat reads. |
| Catalog and reactor checks | `FilterAvailableShipModules` visits both UI lists. For each eligible drive row, `ValidPartForDesign` builds the entire `validDrivesForPowerPlant` list before checking membership. Power plant rows similarly build `validPowerPlantsForDrive`. The installed lists have separate icon and comparison rows. The same templates are checked repeatedly across the two lists. In the worst case there are quadratic numbers of propulsion pair checks in the respective catalog sizes. Each patched pair calls `EffectiveOutput_GW`, which calls the full reactor-bay snapshot, including auxiliary and weapon-power demand that the cap does not use. Comparison-row `SetAlpha` also refreshes drive text and calls the full snapshot twice for a reactor row. Other catalog work includes utility grouping and beam prerequisites, mount geometry, repeated localization and table layout on visibility changes. [Compatibility patches](../../TIEconomyMod/Patches/BalancePatches.cs#L826). [Reactor snapshot](../../TIEconomyMod/Patches/BalancePatches.cs#L335). [Row refresh](../../TIEconomyMod/Patches/ShipPowerPatches.cs#L773). |
| Cost, mass, crew, heat and power | The incomplete-design getters resolve, validate and allocate lists of utilities and weapons on each read. The cost loop reads `magazineModuleMultiplier` for each weapon. Native power reads scan all weapons and can call a utility scan for every powered laser or particle weapon. The mod adds another weapon heat walk with the same bonus lookup. Thus some reads are `O(W × U)` with `W` installed weapons and `U` utilities, before slot validation. A complete reactor demand snapshot includes crew, utilities and weapons. It runs for reactor mass and cost as well as the catalog above. The data panel forces delta-v, which forces another dry-mass pass, and transfer parameters can force it again. [Power demand](../../TIEconomyMod/Patches/BalancePatches.cs#L188). [Weapon heat](../../TIEconomyMod/Patches/ShipPowerPatches.cs#L1165). |
| Fuel allowance | `TryGetSnapshot` resolves the utility, hull-weapon and nose-weapon lists and sums footprint volume, then reads the scanning `crewBillets` getter twice. An ordinary panel refresh with the feature active and a drive installed requests the snapshot for enforcement, the propellant spinner label, and the overlay. The prefix already assigns a lower tank count only when over capacity, in **one** write. The waste is repeated full capacity calculation and UI writes. Cost is refreshed before the panel prefix gets a chance to clamp, so the refactor must ensure the published cost uses the final fuel count. [Capacity and enforcement](../../TIEconomyMod/Patches/FuelCapacityPatches.cs#L46). [Volume scan](../../TIEconomyMod/Patches/FuelCapacityPatches.cs#L285). [UI patch](../../TIEconomyMod/Patches/FuelCapacityPatches.cs#L638). |
| Slot lookups and placement | The native slot lookup walks hull slots and calls linear `slotIndex` in that loop, giving a worst case of `O(S²)` in hull slots `S` before searching installed entries. Native large-weapon fitting builds candidate sets by walking hardpoints and resolving neighbors by another slot scan. Incomplete-design weapon getters can run that validation for each weapon. On a miss, the mod utility occupancy patch can scan utility entries and resolve each multi-cell footprint by slot scans. Multi-cell utility drop checks build occupancy by calling the native lookup for each candidate slot of the right type. `GetBestDropDestinationForModule` then tries legality across the grid, repeats `ValidPartForDesign` per eligible destination and sorts candidates. These multiply the scans in auto-placement and some detail-pane refreshes. The capital compact-mount patch simplifies specific weapon footprints. [Utility placement and occupancy](../../TIEconomyMod/Core/UtilityFootprintRuntime.cs#L44). [Capital mounts](../../TIEconomyMod/Patches/CapitalMountPatches.cs#L16). |
| Combat score | For a valid design, the full panel can call `TemplateSpaceCombatValue(forceUpdate: true, fidelity: 0.6)` once per frame. `UnnormalizedTemplateSpaceCombatValue` builds weapon data and creates a dummy ship for each test combat, iterates attacks, applies hits and revisits the functioning weapon list during the simulation. The designer's fidelity argument still takes this simulation path. Normalization can also rebuild the alien baseline by grouping campaign ships and evaluating their designs when dynamic scoring is enabled. This path runs without a transfer route. |
| Ship preview | A qualifying install (`hasModel`, drive or reactor) or removal (also includes weapons explicitly) can destroy and instantiate the entire preview in `UpdateConstructionCameraImage`. `InitializeModelOnly` instantiates the hull prefab and `BuildShip` configures its weapon controllers and installed weapons. Size scales with hierarchy and equipment. On the Mothership the mod also prepares and validates the mount geometry on the new instance. Replacement can cause intermediate and final rebuilds because nested removal can request an image refresh. [Mothership preview hook](../../TIEconomyMod/Patches/MothershipMountPatches.cs#L11). [Weapon visual hook](../../TIEconomyMod/Patches/GunpowderArtPatches.cs#L331). |
| Refit, validity and role checks | The panel calls `IsAValidRefitFor(..., getReason: true)`. Eligible refits build grouping and weapon counts for both designs. `CanSaveCurrentDesign`, scoring and role refresh also query `ValidTemplate`, whose capital patch scans installed weapon entries. `PopulateRoleDropdown` clears and creates the same localized options again, and a changed selection can invoke the role callback and panel refresh. These costs compound when publication happens once per footprint cell. The viewport fitter is requested by layout setup, UI scaling and screen show, so it is a separate measurement for hull or viewport changes. [Capital guards](../../TIEconomyMod/Patches/CapitalMountPatches.cs#L283). [Viewport triggers](../../TIEconomyMod/Patches/ShipDesignerViewportPatches.cs#L204). |

The largest structural issues are whole-catalog checks that multiply propulsion
and ship size, and two potentially expensive synchronous operations that repeat
on edits, transfer search and combat score. A preview rebuild adds another cost
on rendered weapons and machinery. To rank those by actual time, capture the
same edit with and without a route, with a valid armed design and an incomplete
one, and with and without a changed visual. The installed finite-thrust plane
change patch requires a real `TISpaceFleetState`. The transfer planner builds a
`TIVirtualSpaceFleet`, so ordinary designer previews use the native solver.
[Plane change guard](../../TIEconomyMod/Patches/InclinedOrbitTransferPatch.cs#L51).

On a stable route, the two notifying field writes are the two possible solve
triggers per refresh. When the destination itself changes, the thrust tool's
`Target` setter can also solve before `UpdateThrustProfile` installs the new
actor. Stage the destination and actor together when refreshing a changed route.

## Refactor sequence

1. **Measure and commit each edit once.** Add opt-in counters and nested timing
   at the real add, drop, delete and replacement handlers, including selection
   updates and Unity UI work in the input frame. Put the native edit entry
   points under a single editing scope. Resolve the target, displaced anchor
   parts and affected cells before publication. Remove each distinct displaced
   part once, skip empty removals and no-op edits, and publish after the final
   mutation and fuel check. Keep normal slot legality, fire modes, armor
   spinner behavior, save eligibility and selection semantics. Route nested
   callbacks through the same scope. Use the existing native methods and
   targeted Harmony call-site changes where possible, with guarded bindings
   for the installed version. Treat load, hull change and auto-design as bulk
   operations with a single final commit. Preserve the existing saved-appearance
   rule that delays fuel clamping until all parts and the final appearance are in
   place. This step can start with one full final stat calculation as a parity
   reference. [Load-time fuel handling](../../TIEconomyMod/Patches/FuelCapacityPatches.cs#L667).

2. **Isolate transfer, combat score and 3D work from the edit.** Stage the
   transfer values together with `SetTextWithoutNotify`. Invalidate the preview
   only when its effective acceleration or delta-v, route, departure constraints,
   relevant options or orbital context changes. Match the native units and
   rounding at the planner boundary. For a changed key, mark the duration
   updating and coalesce rapid edits into a single request for the latest ship.
   Keep loss of drive, unset route and transfer failures explicit. Publish only
   results whose key still matches. Use immutable inputs and move solver work
   off the UI thread only after isolating accesses to game state and Unity. A
   debounce alone still leaves a long main-thread stall. Keep the automatic
   native path until the shared solver can safely provide the same result
   within the frame budget. Preserve normal use of the full Intel planner.

   Key the displayed combat score on its actual inputs, including combat
   acceleration, delta-v, weapon and ammo state, damage-relevant modules,
   armor, role and the normalization baseline. Cost or capacity-only changes
   can reuse the score when these inputs are unchanged. Coalesce dirty score
   work and measure dummy-ship and normalization costs separately. Preserve
   the exact native score by reusing valid results or isolating the simulation
   over a snapshot. A main-thread fallback needs bounded slices to update
   automatically within budget. For the preview, retain the hull instance
   across ordinary edits, update only the changed attachment and footprint,
   and use a full build on hull/appearance changes or unsupported attachment
   changes. Reuse existing model helpers where safe and verify mount order,
   gunpowder appearance, empty cells and camera framing. Time cold asset loads
   and any deferred Unity work so the stall cannot move to a subsequent frame.

3. **Split reactor limits from reactor demand, then narrow catalog updates.**
   Factor the existing bay limit calculation into a query keyed by hull,
   appearance, candidate plant and settings/template generation. The current
   compatibility predicate uses scaled drive demand and the effective cap for
   that pair. Ordinary utility and weapon changes do not alter the pair or
   its limit. Reevaluate propulsion fit on a relevant change, preserving the
   existing drive-cluster fallback and the legality rules for save and refit.
   Calculate actual reactor and radiator sizing from the final aggregate
   electrical/thermal loads. A part can change sizing, cost or mass below the
   cap without invalidating rated compatibility. For individual rows use the
   existing pair predicate directly where equivalent. If a caller needs a full
   compatibility list, compute it once per relevant context. Reuse it for both
   UI views. Key drive and reactor column values on the inputs they actually
   display. Index rows by dependency such as utility group, battery family,
   drive restrictions and first/last laser or particle weapon. Refresh only
   affected rows, and validate newly shown rows on demand against current
   state. Full catalog visibility and width updates belong to actual changes
   in hull, unlocks, obsolescence, filter, tabs or locale. Retain capital and
   multi-cell legality checks at the final drop boundary.

4. **Resolve the changed contributions once.** Build hull coordinate, slot
   type and footprint indexes once per applicable layout, with occupancy and
   anchor lookups for the editable ship. Share those lookups for allowed
   destinations, replacement and deletion from secondary cells. Keep native
   serialized lists and stable ordering. A remaining ordered-list removal can
   be linear with a small constant. Measure it before changing representation.
   Maintain crew, occupied utility/hull/nose capacity cells, module mass and
   resource costs, battery and heat capacities, special-module counts,
   magazine multiplier and beam bonuses from the parts added and removed.
   Preserve existing capacity-size policy and single counting of multi-cell
   parts. Compute `maximum = floor(available fuel volume / volume per tank)`
   with the existing math, input guards and rounding, using those aggregates.
   If current tanks exceed the maximum, set the count once before publishing
   costs, mass and performance. Otherwise leave the count alone. In either
   case the spinner and overlay consume the same final snapshot and only write
   changed values. Changing a drive, hull or appearance invalidates density or
   layout inputs as applicable. Loading a legacy design must preserve its
   existing capacity semantics through conversion and editing.

   Read mass, cost, power, heat, acceleration and delta-v from the same
   committed revision. For modifiers that really affect other parts, such as
   magazines or beam boosts, recompute only the affected weapon family, and
   share the utility summary across the pass. Calculate the dependent reactor
   and radiator once. Start with scoped caches for the designer, including
   reuse of resolved part lists within a revision. Rebuild on template swap,
   load/refit/reset, settings changes or an untracked native mutation. Include
   direct spinner/armor/fuel/fire-mode callbacks in the mutation inventory.
   Keep the native save and finish-design validation as authoritative and
   verify parity against full recalculation there. Avoid broad caching of
   mutable campaign ship templates.

5. **Publish changed UI values only.** Paint the cells, selection, immediately
   relevant stats and legality from the committed result. Cache role options
   until their inputs change, and use non-notifying writes for programmatic
   projection of the model. Reevaluate refit checks when required counts or
   constraints change. Bind tooltip delegates once per view and fill details
   on demand. Update only changed highlights and rendered strings. Keep
   derived jobs keyed to the current editor/ship revision and discard old
   completions on a design switch, reset, refit exit or screen close.

## Acceptance

- Capture the same operations before and after on a small design, a populated
  human capital and a populated Mothership, with a late-game catalog. Include
  drag/drop and Install, removal and replacement, empty-slot no-ops, a
  secondary-cell removal, first/last beam, magazine change, fuel cap boundary,
  drive/reactor changes, a rendered gun, and an existing-design refit. Repeat
  the ordinary edits with no route, unchanged planner inputs and changed
  planner inputs. Include rapid successive edits, switching designs mid-job,
  a difficult route, and a save/reopen. Record build identity, hardware, hull
  and installed part counts, route and UI state for comparison.
- Define **under 1 ms at p95** of attributable edit CPU time through the
  first visual update for ordinary warmed edits, per representative large-hull
  case. Include destination selection, mutation, capacity adjustment, UI
  writes and the edit's Unity layout, canvas and preview work. Track deferred
  work by frame and report wall time to first visible update and to fresh
  score/transfer results separately. Report p50, p95, p99, maximum, allocation and
  solver/model/SCV counts over repeated samples. A deferred stall fails the
  responsiveness goal. Record cold asset loads and structural changes with
  their own timings. Use the native rendered designer for final acceptance.
- Ordinary single-part edits should have one final publication, no inline
  transfer solve or test-combat simulation, no full catalog pass, and no full
  hull rebuild for an attachment-only change. Work should track the changed
  cells and affected dependencies as ship and catalog size grow. Confirm
  capacity rounding and clipping, reactor fit and thermal demand, mass, cost,
  crew, budgets, refit eligibility, mount rules, fire modes and saved design
  parity against full calculations from the same final ship. Confirm transfer
  success and failure display and that no outdated result appears as current.

## Evidence identifiers

- Native methods inspected in the installed `Assembly-CSharp.dll`, SHA-256
  `64D88E996C63B4254675D608E859AAC523B43488625937D8E2E9B1E04A3E8B3B`.
- The installed `StreamingAssets/AssetBundles/ui`, SHA-256
  `B2F63FE775934CA8DD650F7197A346CEB22626565601976CD738DBB4FCB5172C`,
  contains persistent `m_OnValueChanged` calls on the planner's acceleration,
  delta-v and departure input components. The serialized target points to the
  component owning those three fields. `TMP_InputField.SetText` in the installed
  TMPro assembly confirms synchronous notification on unequal strings.
