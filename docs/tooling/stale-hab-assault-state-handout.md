# Stale Hab Assault State Handout

## Purpose

This handout records the save data observed while repairing a persistent
"under bombardment" presentation on a surface hab. It intentionally does not
claim a root cause. A future session can use the evidence below to investigate
the bombardment lifecycle and implement a general code fix.

## Player-visible symptom

- Feoktistov Base on Ceres continued to be treated by the UI/gameplay as if it
  were under bombardment after the attacking fleet's bombardment had ended.
- The condition persisted across time and saves.

## Save examined

- Source: `Escapesave00354_2044-3-17.gz`
- The native decompressed payload passed a byte-identical parse/serialize
  round trip before inspection.

## Serialized hab state before repair

The affected state was
`PavonisInteractive.TerraInvicta.TIHabState` ID `54469`:

- `displayName`: `Feoktistov Base`
- `habType`: `Base`
- `tier`: `2`
- `faction`: `4795` (Project Exodus)
- `habSite`: `5181` (Hanami Planum, Ceres)
- `barycenter`: `79` (Ceres)
- `inCombat`: `false`
- `underBombardment`: `false`
- `underAssault`: `true`
- `conflictFleets`: `[212796]`
- `dockedFleets`: `[]`

The UI symptom therefore did not correspond to the serialized
`underBombardment` Boolean. The remaining live-state differences were the
`underAssault` Boolean and the retained `conflictFleets` entry.

## Referenced fleet data

- Fleet ID `212796` was absent from the complete
  `TISpaceFleetState` table in the examined save.
- The same ID remained in Feoktistov Base's `conflictFleets` collection.
- Thirteen historical entries in
  `TINotificationQueueState.panelSummaryQueue.Bombardment` retained
  `gotoGameState` references to fleet `212796`.
- Those log entries identify the missing fleet as Initiative fleet `India-62`
  and include bombardment activity on December 1, 2, 12, 13 and 30, 2042.
- The December 13 log records ship `Sargo` destroying Feoktistov Base's
  Outpost Mining Complex.
- Multiple entries explicitly record that India-62 ended its bombing run,
  including December 2, 13 and 30.

The historical notification entries were preserved during the save repair.
They are evidence of prior activity rather than live hab state.

## Save correction applied

Only `TIHabState` ID `54469` was changed:

- `underAssault`: `true` to `false`
- `underBombardment`: retained as `false`
- `conflictFleets`: `[212796]` to `[]`

No fleet, notification, goal, metadata or other top-level game state was
added, removed or changed.

## Future investigation targets

A future diagnostic session should trace the code paths that mutate these
three hab fields during:

1. bombardment initiation;
2. normal bombardment completion;
3. repeated bombardment runs by the same fleet;
4. attacking-fleet destruction, disbanding, merging or removal;
5. save/load after a bombardment has completed; and
6. conflict cleanup when a referenced fleet state no longer exists.

Useful reproduction assertions are:

- A completed bombardment leaves `underBombardment == false`.
- A hab with no active assault operation leaves `underAssault == false`.
- `conflictFleets` contains only extant fleets that are currently in conflict
  with the hab.
- Fleet removal cleans live hab conflict references without deleting
  historical notification records.

## Save-edit validation requirements

The repaired candidate should continue to require:

- byte-identical source and candidate parse/serialize round trips;
- unchanged metadata and `currentID`;
- no unresolved FullSerializer references;
- identical non-finite token counts;
- an unchanged set of top-level state IDs; and
- exactly one changed retained state: `TIHabState` ID `54469`.
