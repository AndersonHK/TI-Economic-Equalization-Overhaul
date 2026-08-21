# Orbital hab ambient-hazard exposure scaling

Status: implemented and deployed on 2026-08-21 for Terra Invicta 1.0.51.

## Problem

Terra Invicta selects narrative events from a global weighted pool before it
selects a faction and a target hab. Consequently, a sparse early-space economy
can receive the same ambient hab-hazard roll frequency as a mature system with
dozens of stations. A faction with one two-module station can therefore lose
both useful modules in a short period even though its physical exposure is
tiny.

The installed 1.0.51 templates identify two generic hazards that can destroy a
human hab module:

- `event_MeteorStrike`: base weight `1`, monthly weight growth `0.02`, and a
  60-month global cooldown. Its undefended result can destroy a module.
- `event_HabAccident`: base weight `2`, monthly weight growth `0.1`, a 12-month
  global cooldown, and a 36-month target cooldown. It retains its native
  requirement that the target already have a positive support-failure level.

`event_OrbitalDebrisStrike` requires debris in the target orbit, and
`event_HabModuleMalfunction` is a forced consequence of exceeding mission
control. Those events represent player-created conditions and are deliberately
not reduced. Sabotage, revolt, smuggling, containment, combat, and other
explicitly caused losses are also outside this change.

## Approved equation

Let `H` be the number of extant orbital habs owned by all human factions. The
selection-weight multiplier for the two ambient hazards is:

```text
exposure(H) = clamp(H / 30, 0, 1)
adjustedWeight = nativeWeight * exposure(H)
```

Examples:

| Human orbital habs | Ambient-hazard weight |
| ---: | ---: |
| 0 | 0% of native |
| 1 | 3.33% of native |
| 2 | 6.67% of native |
| 15 | 50% of native |
| 29 | 96.67% of native |
| 30+ | 100% of native |

The global count is required by the game's ordering: the event is weighted
before a faction or target exists. Native target selection remains intact, so a
faction with more eligible stations still supplies more possible targets and
receives proportionally more exposure. Event cooldowns, weight growth, target
conditions, defense outcomes, and damage outcomes remain native.

Because the two native event templates combine orbital and surface hab targets,
the multiplier applies to the global occurrence of those templates rather than
only to the station ultimately selected. Splitting the templates would be a
larger compatibility-sensitive content rewrite; this implementation instead
changes only the event-selection weight.

## Implementation and verification plan

1. Patch the event system's weighted-selector callback and alter only the two
   approved event keys.
2. Keep the exposure equation in a pure helper with boundary tests at 0, 1, 2,
   15, 29, 30, and above 30 habs.
3. Validate the installed 1.0.51 event templates, exact private selector target,
   Harmony binding, and explicit exclusions for debris and mission-control
   failures.
4. Build, run the full automated verification suite, and deploy through
   `tools\deploy.ps1`.
5. Manually test an early campaign with one orbital hab and a mature save with
   at least 30 orbital habs. Statistical frequency validation requires repeated
   event rolls; a single quiet or destructive interval is not conclusive.

## Deployment record

`tools\deploy.ps1` completed the normal build, validation, packaging, and copy
flow on 2026-08-21. The deployed assembly SHA-256 is
`99AB2E0EDEEFD6E114F2A7ED57F12365E5D80D8992E10D5DF8BBB25148A9C975`.
The verification run included:

- exact Harmony binding to the TI 1.0.51 narrative-event weighted selector;
- installed-template checks for the two scaled events and the two explicit
  caused-hazard exclusions;
- formula boundaries and event-scope exclusions among 1,135 passing formula
  assertions;
- the full repository verification suite and deployment of 46 mod files.

The implementation is recorded as `hab_ambient_hazard_exposure` in the current
implementation matrix. Manual in-game frequency testing remains the final
behavioral check.
