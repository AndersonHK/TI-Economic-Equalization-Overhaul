# Habitat Slot Progression and Capacity Expansion

Updated: 2026-11-05 (America/Los_Angeles).

Status: **phase 1 implementation plan only; not implemented or deployed**.
The existing human T1 station expansion remains the runtime baseline. Phase 2
retains the earlier feasibility research below and must wait for phase 1 manual
acceptance. This planning task changes documentation only.

## Phase 1: unlock existing sectors one tier earlier

### Intended result

Apply the existing station treatment to surface bases, and unlock the remaining
vanilla T3 sectors at T2 for both types. Use the existing five sectors, module
indices, UI cells, model attachment points, and connector assets.

Counts below exclude the core. A surface base's count includes its one reserved
mine position; it is not an additional freely assignable facility slot.

| Hab type | Tier | Current mod facility positions | Planned positions | Planned active internal sectors |
|---|---:|---:|---:|---|
| Human station | 1 | 12 | 12 | 0, 2, 4 |
| Human station | 2 | 12 | 20 | 0, 1, 2, 3, 4 |
| Human station | 3 | 20 | 20 | 0, 1, 2, 3, 4 |
| Human surface base | 1 | 4: mine + 3 general | 12: mine + 11 general | 0, 1, 2 |
| Human surface base | 2 | 12: mine + 11 general | 20: mine + 19 general | 0, 1, 2, 3, 4 |
| Human surface base | 3 | 20: mine + 19 general | 20: mine + 19 general | 0, 1, 2, 3, 4 |

With the core included, the target totals are **13 / 21 / 21** state positions.
T3 still unlocks T3 modules and their native capabilities; available land does
not grant higher-tier buildings. Keep research, core/module tiers, site maximum
tier, mine placement, power, costs, upkeep, construction times, logistics, and
one-per-hab rules intact. More buildings naturally change total power, upkeep,
production, and defenses; measure those consequences rather than adding an
unrequested balance adjustment.

Scope follows the existing human-hab change: player and AI human factions,
including automated human T1 cores where ordinary construction is allowed.
Alien habs retain native rules. Preserve static/scenario hab build restrictions;
do not turn a fixed authored hab into an expandable one. Distinguish native
`IsAlien()`/static restrictions from the owner's faction when checking capture.

There are no new lots, additional sectors, slot remaps, custom connector assets,
or increases beyond twenty facility positions in phase 1. If existing assets
cannot render the early unlock correctly, resolve that within the existing
topology or record a phase 1 blocker before taking on phase 2 geometry.

### Evidence and current implementation

The local game assembly was freshly decompiled for this plan with ILSpy
9.1.0.7988. Target: installed `TerraInvicta_Data/Managed/Assembly-CSharp.dll`,
7,201,792 bytes, SHA-256
`4A4B9AAE4154E444E9727204205D2D42AE8ED9E1C5F92CDC1280074A259D8350`.
Temporary decompilation is under `.tmp/hab-phase1-plan/`; the method names,
thresholds, and findings below are the retained research record. Recheck the
target hash and patch shapes before implementation if the game changes.

| Owning source or native method | Verified behavior | Consequence for phase 1 |
|---|---|---|
| [HabStationSectorRebalance](../TIEconomyMod/Patches/HabRebalancePatches.cs) | `ReconcileTierOneStation` assigns inactive station sectors 2 and 4 to the core faction at human T1; called after new-hab initialization and late save repair | Generalize to a shared type/tier policy; keep the existing T1 station result |
| [HabRebalanceMath](../TIEconomyMod/Core/HabRebalanceMath.cs) | Connector requirement drops from 2 to 1 only for active human T1 station sectors | Replace the narrow rule with explicit sector unlock requirements |
| `TIHabState.InitializeSector` | Creates five positions in sector 0 and four in each of sectors 1–4 | All state positions already exist; do not append or recreate anything |
| `TIHabState.InitializeNewHab` | Stations: sectors 2/4 require T2, 1/3 require T3. Bases: sectors 1/2 require T2, 3/4 require T3 | Activation differs by hab type; a station-only index list is incorrect for bases |
| `TIHabState.PostGameStateCreateInit_OnCreationOnly_1` | Scenario/template creation initializes sector owners from `TIHabTemplate`, then rebuilds connectors | Cover template/skirmish creation separately from ordinary founding |
| `TIHabState.CompleteModuleConstruction` | Core completion changes `tier`, activates native upgrade sectors, rebuilds connectors, then emits `HabModuleConstructionStatusChange` | Handle the T1→T2 transition in the same operation, before consumers see the final change event |
| `TIHabState.PostEverythingSaveRepair_8` | Late repair can initiate combat for docked hostile fleets | Establish upgraded state/connectors before visualizer/combat consumers; the existing late postfix alone is insufficient evidence of safe load ordering |
| `TISectorState.UpdateModuleConnectorMap` | Four station and four base outer-sector occupancy flags combine `hab.tier` with `HasAnyModules()`; thresholds are listed below | Update all eight sector gates while preserving occupancy and routing rules |
| `TIHabState.UpdateAllModuleConnectors` | Recomputes each module's connector flags and then station `ringStruct` or base `connStruct` | Refresh derived state after activation, including empty intermediate positions |
| `HabitatsScreenController.PreviewStation` / `PreviewBase` | Both iterate the existing five sectors and show owned sectors; base preview also reads six named connection cells | No extra UI cells should be needed; verify connector sprites, clicks, and refresh |
| `HabModelController.Initialize` / `HabModuleController.SetModuleValue` | Model discovers authored controllers, including inactive children, and binds module states by existing indices | Existing attachments are the first route; discovery does not prove lower-tier meshes/connectors render there |
| `TISectorState.ValidModuleForSlot` | Ordinary buildings must fit hab tier; base sector 0 slot 1 is reserved for the mine | Preserve these checks; more positions must not bypass tier or mine rules |
| `TIHabState.maxModules(int tier)` | Returns 5 / 13 / 21, including core | Audit callers for capacity assumptions; do not globally change an API with no hab/species context |
| `HabListItem.UpdateItem` and existing list-icon transpiler | Station overlays use sector activity; the mod suppresses peripheral station overlays at T1 | Keep actual tier artwork and the accepted T1 icon behavior; check newly active T2 overlays and base summaries |

Native inter-sector connector gates and intended replacements:

| Hab type | Internal sector | Native minimum tier | Phase 1 minimum tier |
|---|---:|---:|---:|
| Station | 1 | 3 | 2 |
| Station | 2 | 2 | 1 |
| Station | 3 | 3 | 2 |
| Station | 4 | 2 | 1 |
| Base | 1 | 2 | 1 |
| Base | 2 | 2 | 1 |
| Base | 3 | 3 | 2 |
| Base | 4 | 3 | 2 |

These are **sector unlock gates**, not the separate `m.moduleTemplate.tier`
checks that select connectors for the physical size of an individual module.
The current transpiler finds exactly two station T2 gates before the first
sector switch. Extending that search mechanically cannot cover the T3 station
gates or the later base branch.

No existing template capacity field is read in the inspected initialization and
connector paths. Scenario JSON specifies initial sector ownership and module
placement, not a general per-tier capacity rule. This is therefore a small
runtime-policy change, with template edits only if the startup audit proves
they are required for particular authored scenarios.

### Implementation sequence

1. **Record the baseline and finish the targeted asset/caller audit.** Capture
   matching source/package/game hashes and reference layouts before changes.
   Inventory the existing station and base cells, attachment controllers, and
   connector meshes for every newly exposed sector at T1/T2, including the
   preview and combat model paths. Trace their tier/visibility consumers and
   `maxModules` callers against the same installed assembly. A preliminary
   cached-source search found build-full/specialty goals using `maxModules(3)`
   and a refuelling-station goal using `maxModules(1)`; revalidate those callers
   before deciding whether any capacity-aware AI correction is necessary.

2. **Centralize unlock policy and reconcile existing state.** Replace the
   T1-station-only helper with one explicit policy for human station/base sector
   requirements. Activate only required inactive sectors using native
   `SetFaction`, taking the core sector's owner. Never steal a foreign-owned
   sector, overwrite an existing module, reset construction, or deactivate a
   populated sector. Reject malformed/incomplete state safely with a diagnostic;
   do not create replacement sectors. Repeated reconciliation must be a no-op
   without repeated ownership events, income invalidations, or full refreshes.

3. **Cover founding, scenario creation, upgrades, and load ordering.** Use the
   same policy for ordinary founding and template/skirmish creation. On core
   completion, reconcile after the actual tier changes and before the final
   native connector rebuild/event. Prefer a narrowly anchored injection there
   over a late postfix that lets UI/model listeners observe stale state. On
   save load, identify a safe point after the sector/module references and
   ownership services exist but before visualizer creation and possible combat
   initiation. Keep late repair as an idempotent safety net only if needed.
   Do not unlock T2-only sectors merely because an upgrade has been queued.
   Direct T2/T3 founding and T1→T3 jumps must also work. Capture/transfer must
   preserve native sector ownership, including mixed ownership.

4. **Extend connector gates without replacing the topology.** Match the eight
   native occupancy gates by hab-type branch, sector index, native threshold,
   and `HasAnyModules` call. Supply the policy's required tier for eligible
   habs; use the original threshold for aliens and excluded special cases.
   Preserve module-size gates, occupancy predicates, directional routing,
   station rings, and base connections. The validator must require exactly the
   expected sites and fail clearly on target drift. Do not lower every literal
   `2`/`3` or globally report a higher hab tier. Keep branch labels and exception
   metadata intact and verify the transformed method can be emitted/executed.

5. **Refresh derived state and integrate presentation/AI.** Rebuild native
   connector flags once after a changed reconciliation and ensure event
   subscribers see the coherent result. Audit list icons, available-slot
   summaries, build placement, model visibility, AI selection, and capacity
   heuristics. Keep the accepted T1 station-list overlay suppression; T2 may
   display its now-active sectors while retaining its true tier icon. Patch
   only demonstrated assumptions. Any capacity helper that needs species/hab
   context must receive it rather than changing `maxModules` globally.

6. **Build and deploy the complete phase 1 slice.** Add the automated coverage
   below to the normal verification path. Once implementation is ready, run
   `tools\deploy.ps1` immediately, with normal verification and its two game-
   closed assertions. If the game is open, stop deployment safely. After
   success, immediately tell the user the build is ready for manual testing;
   finish evidence/doc updates while playtesting proceeds. Repeat the normal
   deployment flow after every implementation fix or later rebuild.

7. **Iterate through manual acceptance, then document the actual result.**
   Retain package hashes, validation summaries, scenario/test descriptions,
   relevant screenshots/logs, observed defects, and retest outcomes. Update the
   project README, implementation matrix, this plan, and the
   [station slot map](hab-station-slot-map.md) for delivered behavior. Keep
   proposed, deployed, and manually accepted status distinct. Do not advance
   phase 2 on a successful build alone.

### Automated validation to add or extend

Extend [formula tests](../tests/FormulaTests/Program.cs), the
[connector transpiler validator](../tools/validate-hab-connector-transpiler.ps1),
[target IL checks](../tools/validate-target-il.ps1), and
[hab rebalance validation](../tools/validate-hab-rebalance.ps1), wiring any new
focused harness into [normal verification](../tools/verify.ps1).

- Exhaust the station/base × T1/T2/T3 × human/alien policy matrix, with null,
  inactive, foreign-owned, static, automated, and invalid-sector controls.
- Exercise founding, template creation, direct T2/T3 founding, completed and
  cancelled upgrades, and load reconciliation. Assert sectors, module IDs,
  ownership, queued jobs, and mine/core coordinates survive unchanged except
  for the intended inactive-sector activation. Repeating repair must produce
  no additional state or ownership events.
- Execute the transformed connector calculation on representative initialized
  states. Cover a lone building at each newly exposed coordinate, sparse and
  fully populated sectors, empty intermediate positions, mixed module tiers,
  under-construction and destroyed modules. Assert boundary connections and
  occupancy semantics; compare alien and unchanged T3 results to native output.
- Check all eight native gate matches, emitted IL, and event ordering. A helper
  formula test or a replacement-count test alone cannot establish that the
  live method is patched correctly or that a renderer receives updated flags.
- Check mine-only placement, module-tier rejection, site-tier limits, AI slot
  selection, and the accepted ISS/Tiangong assignments. Update old tests that
  explicitly expect T1 bases to stay gated without weakening alien controls.

### Manual acceptance matrix

Use separately named test saves; preserve originals and follow the
[save-format rules](tooling/terra-invicta-save-format-and-editing.md) if fixtures
need editing. Persistent evidence belongs in an appropriate topic directory
under `docs/`; temporary harness output belongs under `.tmp/` or `outputs/`.

| Test group | Required checks |
|---|---|
| Fresh founding | Human T1 station and T1 surface base expose the intended 12 facility positions; direct T2 founding exposes 20; automated human cores retain their native build restrictions |
| Existing campaigns | Load pre-change T1/T2 stations and bases, including an upgrade in progress; new sectors appear without waiting for a rebuild, tier upgrade, or extra save/reload; existing jobs and modules survive |
| Scenario/skirmish creation | ISS/Tiangong and authored hab creation retain their assignments; available skirmish station fixtures use the same policy without exposing restricted static builds |
| Every new coordinate | Select, queue, cancel, complete, upgrade, depower, decommission, destroy, and rebuild in each new sector; test every slot at least as a lone occupied coordinate, then sparse and full layouts |
| Surface base presentation | Check T1/T2 module grid, mine reservation, empty mine, occupied mine, all six named base connection cells, clicks/tooltips, and the surface backgrounds on representative sites (for example Luna and Mars) |
| Station presentation | Check T2 newly unlocked sectors with T1/T2 modules, T1 regression layouts, boundary and ring joins, empty intermediate positions, preview/strategic model, and list overlays; retain real tier labels |
| Core transitions | T1→T2 unlocks the remaining sectors at completion; queued/cancelled upgrade does not unlock early; T2→T3 keeps all indices/modules and grants native T3 buildings; test a direct T1→T3 transition if reachable |
| Combat and persistence | Enter rendered combat with defenses in each newly unlocked station sector, including T1 station regressions and T2 mixed modules. Verify attachment geometry, connectors, targeting/firing, damage/destruction, and post-battle reconstruction. For bases, test native bombardment/assault and post-combat state rather than assuming an orbital station combat model |
| Save/reload after use | Save occupied, damaged, rebuilding, and upgraded layouts; fully restart the game and reload; verify slot ownership/IDs, appearance, construction queues, connectors, and available-slot counts |
| Ownership and AI | Transfer/capture, including mixed-sector ownership where available; verify no foreign sector is reassigned by repair. Observe human AI placing buildings in expanded sectors and respecting tier, power, cost, and logistics requirements |
| Controls | Alien habs and native T3 layouts retain their behavior; static authored restrictions remain; no duplicate controllers, missing/overlapping buildings, dangling required joins, or recurring exceptions |

### What “rock solid” means before phase 2

All automated checks must pass through normal deployment. The manual groups
above must have recorded outcomes for fresh and existing habs, both hab types,
T1/T2 layouts, upgrade transitions, occupied-slot reload, and station combat.
Resolve and retest every missing connection, invisible/unselectable module,
ownership error, lost job, or reproducible exception in this scope. Record any
untested case explicitly; lack of evidence is not acceptance.

Require user acceptance of the resulting in-game layouts and connector behavior
before beginning new lots or remapping. Phase 2 should start from an accepted
phase 1 layout/connector map and a retained occupied-save fixture. This makes
later failures attributable to newly authored geometry rather than unfinished
early-unlock behavior.

### Planning outcome and remaining uncertainty

The state and UI paths support the proposed progression without changing list
sizes or coordinate identities. The main uncertainty is whether every newly
exposed coordinate has usable lower-tier geometry and connector rendering in
all native visual contexts. The targeted asset audit and rendered playtests are
hard gates; this plan does not claim that they have already passed. No mod
build, deployment, save modification, or live gameplay test was performed for
this documentation task.

## Phase 2: retained capacity research (deferred)

The following is the earlier station-focused feasibility assessment for going
beyond the existing twenty facility positions. Its 24-position approach is a
candidate, not an approved new layout. Resume and extend the research to the
desired hab types, author connectors, and choose slot remaps only after phase 1
acceptance. It does not establish that equivalent new surface-base geometry
already exists.

### Current structural limit

Vanilla station state is organized as five sectors:

- internal sector 0: core plus four facilities;
- internal sectors 1–4: four facilities each.

That produces twenty facility slots plus the core. The limit is not merely a
balance constant:

- `TIHabState.maxSectors` and `maxSectorIdx` are literal `5` and `4`;
- `TIHabState.InitializeSector` creates five module states for the core sector
  and four for every outer sector;
- `HabitatsScreenController.PreviewStation` iterates five sectors, four outer
  slots, and five core slots;
- `TISectorState.UpdateModuleConnectorMap` contains explicit sector and slot
  topology;
- the station UI asset serializes cells only for `S0_M0` through `S0_M4` and
  `S1_M0` through `S4_M3`;
- station models contain a fixed set of module controllers and attachment
  transforms, which `HabModelController` discovers from the prefab.

Lists of sectors and modules are serialized dynamically, so save storage is not
the main obstacle. The fixed UI and model topology is.

### Practical expansion path: 24 facilities

The least invasive design is one additional facility in each existing outer
sector. It preserves five sectors and raises capacity from twenty to
twenty-four facilities.

Required work:

1. Set outer-sector capacity to five and append one empty
   `TIHabModuleState` during new-hab initialization and idempotent save repair.
2. Clone and place one `StationGridCell` per outer sector at runtime, using
   deterministic names `S1_M4` through `S4_M4`.
3. Extend `PreviewStation` and module-selection paths from four to five outer
   slots.
4. Define a symmetric connector topology for each new slot.
5. Clone or supply one additional module attachment transform and
   `HabModuleController` per outer sector in the station model.
6. Audit construction, decommissioning, combat targeting, destruction,
   capture, AI placement, and save/load with the fifth outer module present.

The state and ordinary construction paths are mostly list-driven. The highest
risk is visual/model integration: a slot that exists in state but lacks a UI
cell or 3D attachment controller can become invisible or unselectable.

### Adding sixth or later sectors

Adding sectors rather than slots would require replacing the fixed five-sector
model across:

- `maxSectors`/`maxSectorIdx`;
- ring and connection structures;
- sector-number switches;
- template layouts and UI keys;
- station preview artwork;
- 3D station prefabs and sector controllers;
- connector routing and combat representation.

This is substantially harder and should not be the first expansion approach.

### Confidence

A symmetric 24-facility prototype is feasible as a dedicated, test-heavy
feature slice. It is broader and riskier than the T1-sector work because it
must create new UI and 3D attachment points rather than reveal existing ones.

Arbitrary capacity or additional sectors remains a high-difficulty project.
The recommended sequence is:

1. prototype one fifth slot on one outer sector;
2. verify build, selection, save/load, and combat behavior;
3. generalize symmetrically to all four outer sectors;
4. only then consider more than twenty-four facilities.
