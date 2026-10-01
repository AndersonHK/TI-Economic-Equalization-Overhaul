# Alien territory demands — 0.10.2

## Plan and scope

Require the Servants to own every control point in a nation before the Alien
Administration can demand one of its regions. Executive ownership alone must
not qualify, and an unowned point must prevent the demand too.

The installed TI 1.0.53b implementation uses `TransferRegionsOption` (Demand
Claim), not a `TINarrativeEventTemplate`. Its `GetPossibleTargets` already
checks claims, capital exclusion, peace, consolidated executive control,
relations cooldowns, hostility, and `CanTransferTerritoryToAliens`. The AI
recognizes an alien demand against an `IsAlienProxy` executive as guaranteed
one-way expansion.

Add a Harmony postfix that filters the existing eligible region list only
when the demanding nation is alien and the target executive is the Servants
(`IsAlienProxy`). Preserve all native restrictions and other factions' policy
behavior. Check actual control-point owners, not public opinion, executive
consolidation, or nominal control-point capacity. The Assume Control councilor
mission is outside this change.

Bump manifest, assembly, startup log, verification, and release artifact from
0.10.1 to 0.10.2. Run `tools/deploy.ps1` with normal verification, then perform
manual in-game acceptance.

## Validation and manual acceptance

Implemented in `AlienTerritoryDemandTargetsPatch`. Automated validation checks
Harmony binding against the installed game and exercises mixed ownership,
complete Servant ownership, unowned points, a single-point nation, non-Servant
executives, human demands, an empty native target list, and the disabled mod.

On 2026-09-27, the normal `tools/deploy.ps1` workflow passed the Release build,
all 27 patch validators, formula tests and all 11 data/simulation validators.
It packaged `artifacts/TIEconomyMod-0.10.2-ti1.0.53.zip` and deployed 47 files to
`Mods/Enabled/Economic Equalization Overhaul`, verifying deployed hashes.
Terra Invicta was checked closed before verification and again before copying.
The implementation matrix now includes `alien_territory_demands` and passes
its 106-row validation. Manual in-game acceptance remains pending.

Manual checks pending:

1. With Servant executive control and a different faction holding at least one
   point, verify the Alien Administration cannot select that nation's regions
   for Demand Claim.
2. Repeat with an unowned point; the nation must remain ineligible.
3. Give the Servants all points. Otherwise eligible non-capital claims should
   be available again and use the native demand/response behavior.
4. Check that ordinary human Demand Claim policies still behave normally.

The filter affects newly evaluated policy targets; it does not cancel a demand
already queued before the ownership change or before loading this version.
