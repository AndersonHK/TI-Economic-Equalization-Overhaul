# Terra Invicta save format, editing, and diagnostics

## Scope and version boundary

This document records format and implementation facts useful for building save
inspection tools, performing controlled offline edits, and debugging future
save/load patches. It describes the Terra Invicta 1.0.53 implementation and
native saves produced by that implementation. Recheck the decompiled entry
points after a game update; the format has no independent schema contract in
this repository.

Treat a save as a serialized object graph with formatting-sensitive metadata,
not as an arbitrary JSON document. A file can deserialize and load while still
behaving badly in the save-selection UI.

## Container and root structure

Compressed saves are ordinary gzip streams whose payload is the same
pretty-printed text used for uncompressed saves. Compression does not introduce
another archive member, header object, or binary serialization layer.

The root object is `SaveStructure`:

```text
SaveStructure
|- currentID: GameStateID
`- gamestates: Dictionary<Type, Dictionary<GameStateID, TIGameState>>
```

In serialized form, a representative skeleton is:

```json
{
  "currentID": {
    "value": 123
  },
  "gamestates": {
    "PavonisInteractive.TerraInvicta.TIMetadataState": [
      {
        "Key": {
          "value": 1
        },
        "Value": {
          "ID": {
            "value": 1
          },
          "$type": "PavonisInteractive.TerraInvicta.TIMetadataState"
        }
      }
    ]
  }
}
```

Important structural facts:

- `gamestates` is keyed by runtime state type. Each inner dictionary is emitted
  as an array of `{ "Key": ..., "Value": ... }` entries.
- A full state entry has a dictionary key and a state `ID`; these IDs should
  agree.
- `currentID` is the game-state allocator cursor. It must remain consistent
  with existing and newly assigned IDs. In observed native saves it was above
  the greatest live ID, but tools should treat that as a sanity check rather
  than assuming a particular increment policy.
- A numeric state ID is not necessarily globally unique in a naive text scan.
  Base/derived type tables and serialized references can repeat the same number.
  Resolve identity with the serializer's type and state rules.
- `SaveStructure.Load` reads the entire decompressed payload, applies a legacy
  compatibility insertion for missing `exists` fields, and deserializes it.

## Serializer dialect

The game uses FullSerializer through `StringSerializationAPI`, with a custom
`TIGameStateConverter`. It does not use PowerShell's JSON serializer, browser
`JSON.stringify`, or default Newtonsoft.Json behavior.

The payload is JSON-like, but it is not strictly standards-compliant JSON:

- `Infinity`, `-Infinity`, and `NaN` may appear as unquoted numeric tokens.
  FullSerializer's parser explicitly accepts them and its printer emits them.
- `$type` records polymorphic runtime types.
- `$id` and `$ref` preserve identity for ordinary shared or cyclic objects.
- Nested `TIGameState` values are normally serialized as `GameStateID`
  references instead of recursively embedding complete states. A reference can
  therefore look like `{ "value": 123 }`, optionally with type metadata.
- Integer width, floating-point spelling, property order, and reference
  placement can be changed by a generic round trip even when the resulting
  document looks equivalent.

Consequently, do not round-trip a complete save through `ConvertFrom-Json` /
`ConvertTo-Json`, `jq`, `JSON.stringify`, or a default JSON library. Typical
failure modes include rejecting non-finite numbers, converting them to strings
or zero, changing numeric precision, moving `$id` definitions, and compacting
the payload into one physical line.

The safest writer is the game's own path:

```csharp
fsJsonPrinter.PrettyJson(
    StringSerializationAPI.Serialize(typeof(SaveStructure), save),
    outputStream);
```

`StringSerializationAPI` resets `TIGameStateConverter` after every serialize or
deserialize operation. A tool that directly hosts these components must retain
that lifecycle behavior so state identities do not leak between operations.

## Pretty printing is a UI compatibility requirement

The save-selection screen does not deserialize `SaveStructure` to obtain its
summary. `TIMetadataState.LoadMetaData` uses a separate line-oriented fast path:

1. A `.gz` save is decompressed to a temporary `.json` file.
2. `FindMetadataFromSave` initially reads only the first 200 physical lines.
3. It looks for one line containing both `TIMetadataState` and `[`.
4. It tracks array nesting using line-level `Contains("[")` and
   `Contains("]")` checks.
5. It extracts fields by trimming lines and splitting values from their keys.
6. A caller may retry by reading every line when the fast search finds nothing.

This makes native line layout part of the effective schema. A compact one-line
payload can still pass the full loader but force an expensive whole-file
metadata scan and leave UI fields blank because the line parser cannot isolate
the expected keys. Preserve native `fsJsonPrinter.PrettyJson` output. At a
minimum, verify that the metadata type marker begins within the first 200 lines
and that its fields remain on lines the metadata parser can consume.

## Choosing an editing architecture

Use these approaches in descending order of safety.

### 1. In-game or game-assembly editor

Load the save through `SaveStructure.Load` or the running game's state manager,
apply domain operations, and save through `StringSerializationAPI` plus
`fsJsonPrinter.PrettyJson`. An in-game debug command is preferable for complex
deletions because existing domain methods clean up ownership, queues, missions,
goals, intel, scheduled events, and cached relationships.

When hosting game assemblies outside Unity, pin the tool to the exact game
version and initialize all dependencies the touched state types require.
Deserializing is not proof that post-load repair or gameplay initialization can
run successfully.

### 2. Token-aware surgical offline editor

For a small scalar or a tightly bounded state change, decompress to `.tmp/`,
locate the exact object and property, and replace only the intended token span.
Preserve every untouched byte, including non-finite tokens, `$id` placement,
property order, indentation, and line endings. Recompress the resulting native-
style payload.

This approach needs a parser or scanner that understands strings, escapes,
balanced objects/arrays, and the FullSerializer numeric extensions. Regular
expressions alone are unsafe for selecting nested state objects.

### 3. Configured compatible object-graph editor

A third-party serializer is acceptable only after fixtures prove that it
preserves the FullSerializer dialect, reference identity, runtime types,
numeric precision, and metadata layout. Always compare its no-op round trip
against a native save before using it for mutations.

## Graph-safe mutation rules

Changing a scalar field is usually local. Adding or removing a `TIGameState` is
graph surgery.

Before deleting a state:

- identify the full state entry, its `GameStateID`, runtime type, owner, and
  container relationships;
- find every typed state reference to the target ID, not just textual matches
  in the same type table;
- reproduce the relevant domain cleanup for collections, operations, alarms,
  goals, intel, missions, scheduled events, and notification targets;
- remove or null references according to normal game semantics;
- remove the state entry only after incoming references are handled; and
- leave `currentID` monotonic unless the game's own allocator deliberately
  changes it.

Historical logs and tombstones may contain references to states that are no
longer live. Native saves can therefore contain some apparently missing typed
state references. Validate against the source baseline and reject *new*
dangling references introduced by an edit rather than assuming every numeric
reference must resolve to a live state.

FullSerializer `$ref` values are different: every `$ref` should resolve to a
corresponding `$id` in the serialized object graph. Deleting the first
definition of a shared object while retaining its `$ref` uses is a schema
failure.

Prefer calling existing lifecycle methods over imitating them offline. Useful
examples include state-specific destroy/disband methods and cleanup methods on
factions, notification queues, goals, intel, missions, and time events. Inspect
the current implementation before relying on any method name or cleanup list.

## Safe offline workflow

1. Close Terra Invicta before reading or writing its save directory.
2. Copy the source to a new filename; never overwrite the only known-good save.
3. Record file length, modification time, and SHA-256 hash.
4. Decompress into a unique directory under `.tmp/`.
5. Inventory the source before editing: state types, entry counts, IDs,
   references, metadata position, and non-finite tokens.
6. Make only the requested graph or scalar changes.
7. Run the structural and semantic checks below.
8. Recompress to a distinct candidate filename.
9. Recheck gzip integrity and the decompressed candidate.
10. Copy the candidate into the save directory while the game is closed.
11. Select and load it in-game, then inspect `Player.log`.
12. Save it under another name from inside the game and compare that native
    re-save with both the source and candidate.

The native re-save is a useful normalization oracle. Expected differences can
include expired history cleanup, rebuilt caches, and values recalculated during
post-load repair. Unexpected state-type changes, property loss, new references,
or broad numeric changes require investigation.

## Read-only inspection tools

### Repository utility

`tools/ti_save_tool.py` implements the FullSerializer 1.0.53 text dialect,
including UTF-8 BOM, CRLF layout, expanded empty objects, four-space
indentation, raw numeric lexemes, non-finite values, and C#-style string
escapes. It intentionally refuses a mutation unless parsing and serializing the
source reproduces the complete decompressed payload byte for byte.

Typical generic commands are:

```powershell
python tools/ti_save_tool.py inspect --input C:\path\to\save.gz --body "body-name"
python tools/ti_save_tool.py roundtrip-check --input C:\path\to\save.gz
python tools/ti_save_tool.py references --input C:\path\to\save.gz --id 12345
python tools/ti_save_tool.py remove-ships `
    --input C:\path\to\source.gz `
    --output .tmp\edited-save.gz `
    --audit .tmp\edited-save.audit.json `
    --fleet-id 12345 `
    --ship-id 23456 `
    --ship-id 34567 `
    --expected-faction-id 6789 `
    --expected-body "body-name" `
    --expected-hull "AlienBattleship"
python tools/ti_save_tool.py remove-fleet `
    --input C:\path\to\source.gz `
    --output .tmp\edited-save.gz `
    --audit .tmp\edited-save.audit.json `
    --fleet-id 12345 `
    --expected-ship-count 1 `
    --expected-faction-id 6789 `
    --expected-body "body-name"
```

The removal operation validates its location, ownership, and ship-count
preconditions; removes fleet and ship states; reproduces serialized faction
intel, ownership, notification, goal, and container cleanup; and fails if any
unsupported reference to a removed ID remains. Its audit compares serialized
bytes for every retained state, validates `$id`/`$ref`, confirms metadata fast-
scan compatibility, and reloads the written gzip payload for a second byte-
identical round trip.

Fleet removal also discovers ships which point to the fleet but are temporarily
absent from its `ships` list, as can occur during refits. It removes their
shipyard queue entries, associated officer states, dock-layout slots, and other
lifecycle references. When a removed state contained the first `$id` definition
of an ordinary shared FullSerializer object still used elsewhere, the tool
promotes that exact definition to the first surviving `$ref`; it never leaves
an unresolved reference or invents a replacement object.

`remove-ships` is the narrower operation for deleting selected members while
retaining their fleet. It requires explicit ship IDs and verifies that every
ship exists, belongs to the selected fleet in both directions, and (when
requested) resolves through the owning faction's serialized `shipDesigns`
table to the expected hull. It removes the ship references from the fleet,
deletes associated officer states and lifecycle references, and applies the
same byte-stability, metadata, state-delta, and `$id`/`$ref` gates as fleet
removal. It refuses to leave an empty fleet; use `remove-fleet` when every ship
must be deleted so fleet-level lifecycle cleanup is not skipped.

Combat autosaves need an additional precondition check. Inspect
`TISpaceCombatState`, projectile states, and all incoming references to the
selected ships before editing. A pre-initialization combat snapshot can cache
the two participating fleets and can also retain ship references in
`preservedFleetCompositions`. Those records describe fleets temporarily merged
into a combat participant and are consulted when splitting survivors after the
battle. For a bounded pre-initialization removal, delete the selected ship from
both the active fleet and every preserved record while retaining the record and
its other ships. Do not assume the same is safe after combat assets, targets,
projectiles, or waypoints have been initialized: the editor must either
implement and validate those current-version cleanup semantics or refuse the
mutation when unsupported references remain.

This is still an offline graph editor, not a substitute for manual load,
simulation, log, and native re-save testing. Re-run its no-op round-trip gate
after every game patch before enabling edits for that version.

The default Windows save directory is:

```text
%USERPROFILE%\Documents\My Games\TerraInvicta\Saves
```

The game can be configured to use an alternate location, so confirm the active
profile setting rather than hard-coding the default in an editor.

PowerShell can inventory and hash files without interpreting the payload:

```powershell
$saveDir = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games\TerraInvicta\Saves'
Get-ChildItem -LiteralPath $saveDir -File | Sort-Object LastWriteTime -Descending
Get-FileHash -Algorithm SHA256 -LiteralPath 'C:\path\to\candidate.gz'
```

.NET's `System.IO.Compression.GZipStream` is sufficient to unpack and repack the
container. Use it only as the transport layer; it does not make a generic JSON
serializer safe. Store expanded payloads, reports, and comparison output under
`.tmp/`, never under `docs/` or the game save directory.

For large payloads, prefer streaming/token-aware tools. A whole-file object
model uses substantial memory and makes unintended global rewrites more likely.
Useful comparison reports include:

- per-type state counts and ID sets;
- `Key.value` versus `Value.ID.value` mismatches;
- source-only and candidate-only property paths;
- `$id` definitions and `$ref` uses;
- incoming references to each deliberately changed state;
- locations and counts of `Infinity`, `-Infinity`, and `NaN`;
- metadata start line and field extraction result; and
- scalar changes grouped by owning state ID and property path.

## Validation gates

A candidate is ready for in-game testing only when all applicable checks pass.

### Container and formatting

- The source hash is unchanged.
- The candidate is a valid gzip stream and decompresses without trailing or
  truncated data.
- The payload has native pretty-printed line structure rather than a single
  giant line.
- The fast metadata scan succeeds within the first 200 lines and returns all
  expected fields.
- The game serializer can deserialize the payload when an exact-version harness
  is available.

### Object graph

- State types and per-type count changes match the intended operation.
- Every retained full entry has matching dictionary-key and state IDs.
- Added IDs are deliberate and do not collide; `currentID` remains allocator-
  safe.
- Every `$ref` resolves to a retained `$id`.
- No retained live reference targets a deliberately deleted state.
- Baseline dangling typed references are not increased.
- Property sets and serialized value kinds are unchanged outside expected
  schema-version or mutation paths.

### Values and semantics

- Non-finite numeric token locations and meanings are preserved unless a
  targeted field intentionally changes.
- Large integers and floating-point values retain precision.
- Only approved fields and necessary relationship cleanup differ.
- Derived caches are either unchanged or known to be rebuilt by post-load
  repair.
- A native in-game re-save has explainable differences from the candidate.

Do not use a strict RFC JSON validator as the only syntax gate: it will reject
native non-finite numeric tokens. Use FullSerializer's parser or a compatible
dialect-aware scanner.

## Runtime verification and logs

The primary runtime log is normally:

```text
%USERPROFILE%\AppData\LocalLow\Pavonis Interactive\TerraInvicta\Player.log
```

Capture or timestamp the log before testing so old warnings are not attributed
to the candidate. Confirm the normal load phases, including messages associated
with loading game states, post-load save repair, the loaded state count, and
100% completion. Search the same interval for:

- deserialization, conversion, missing-state, and invalid-cast exceptions;
- FullSerializer assertion failures;
- `GameStateNotFound` and unresolved-reference errors;
- save-repair exceptions;
- missing template/type errors; and
- repeated UI exceptions after opening the save-selection screen.

Successful completion of the full loader does not validate metadata-screen
behavior. Test save selection, displayed metadata, load, several simulation
ticks, and an immediate native re-save as separate gates.

## Save/load implementation entry points

For Terra Invicta 1.0.53, begin future investigations at:

- `SaveStructure`: container model, gzip read path, compatibility insertion,
  and root deserialization.
- `GameStateManager.SaveAllGameStates`: metadata refresh, root construction,
  native pretty printing, and gzip write path.
- `GameStateManager.LoadAllGameStates`: root load and state-manager assignment.
- `StringSerializationAPI`: shared FullSerializer instance and converter reset
  lifecycle.
- `TIGameStateConverter`: full-state versus ID-reference behavior and identity
  reuse during deserialization.
- `GameStateID`: serialized ID representation and runtime lookups.
- `TIMetadataState.SetValues`: fields refreshed immediately before a save.
- `TIMetadataState.LoadMetaData` and `FindMetadataFromSave`: temporary
  decompression and formatting-sensitive metadata extraction.
- `CreateSaveFileScrollList` and `MetadataScreenController`: save-selection and
  metadata-display call sites.
- State lifecycle methods such as ship destruction, fleet disbanding, archival,
  and the corresponding owner/goal/intel/event cleanup routines.

## Future save/load tooling and patch opportunities

A maintainable editor should use a versioned operation model rather than raw
property assignments. Each operation should identify its target state, declare
preconditions, invoke or reproduce domain cleanup, and emit a machine-readable
before/after audit. A no-op round-trip fixture and representative saves from
each supported game version should be part of automated tests.

If patching the game's save UI or load path, the highest-value improvements are:

- replace the line-oriented metadata search with a streaming, dialect-aware
  parser that reads directly from gzip;
- avoid writing a temporary `.json` file merely to display metadata;
- add an explicit format/schema version and migration registry;
- validate state IDs and references before committing loaded state;
- write atomically through a temporary file followed by replace/rename;
- keep a small metadata header or sidecar for fast save-list display;
- move save-list indexing off the UI thread; and
- expose domain-safe debug commands for querying and mutating state graphs.

Any metadata-reader patch must remain compatible with existing native saves and
must not assume strict JSON while FullSerializer continues to emit non-finite
tokens.
