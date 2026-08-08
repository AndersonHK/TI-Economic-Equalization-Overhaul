# Mine Mission Control

Status: implementation authority for the tier-based mine Mission Control rule.

## Rule

Every active habitat mining complex consumes Mission Control equal to its
module tier:

- Tier 1 mining complex: 1 Mission Control.
- Tier 2 mining complex: 2 Mission Control.
- Tier 3 mining complex: 3 Mission Control.

For active mines with tiers `t1 ... tn`, total mine Mission Control is:

`mine MC = sum(ti)`

There is no free-mine allowance and no quadratic network surcharge. Mine count
by itself has no effect on cost. Upgrading an active mine from Tier 1 to Tier 2
or Tier 2 to Tier 3 therefore adds exactly 1 Mission Control. Disabling a mine
releases Mission Control equal to its current tier.

## Removed bonuses

The former `MCFreeSpaceMineNetwork` effects are removed from Mission to the
Moon, Mission to Mars, Mission to the Inner Planets, Mission to the Asteroids,
Mission to Jupiter, Mission to Saturn, Mission to the Outer Planets, Future
Space Science, and Gold Rush. Their exploration, probe-speed, and mining-output
effects remain unchanged.

Existing saves may still contain serialized copies of the retired effects. The
runtime safe-mine allowance is forced to zero, so those copies cannot alter the
tier formula.

## UI and AI integration

The faction Mission Control breakdown reports the sum of mine tiers as the
active-mine cost. Mine construction and upgrade tooltips explain that the cost
is tier-based and show the 1-MC increment for a new Tier 1 mine or a one-tier
upgrade. AI mine shutdown logic receives the full tier cost released by
disabling a mine.

The used-Mission-Control number in the resource bar is colored only by total
capacity utilization, never by mine count or the retired free-mine allowance:

- Normal at or below 75% utilization.
- Orange above 75% utilization through exactly 100%.
- Red above the Mission Control limit.

## Verification

The release pipeline validates all five Harmony targets against the installed TI
1.0.51 assembly, asks Harmony to emit each replacement, checks the 1/2/3 tier
mapping and the 75%/100% display boundaries, exercises the pure tier-sum
formula, verifies every retired effect override against vanilla, scans the
replacement localization, and requires this feature in the implementation
matrix. The deployed 0.9.0 build passed 634 formula assertions and the complete
release verification suite on 2026-08-07. Its DLL SHA-256 is
`7BB9A085920269096D14D8FA496F9357A95285617909745C143135809DE00490`.

Manual smoke test:

1. Compare the faction MC breakdown against the sum of active mine tiers.
2. Confirm building a T1 mine and upgrading a mine each show a +1 MC increment.
3. Disable a T2 or T3 mine and confirm it releases 2 or 3 MC respectively.
4. Inspect the affected exploration technologies and Gold Rush for the absence
   of a mine-allowance benefit.
5. Confirm MC usage is normal at 75%, orange just above 75% through the limit,
   and red above the limit.
