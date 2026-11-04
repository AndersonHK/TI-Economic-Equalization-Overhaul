# Alien armor material update — 2026-11-04

Status: **implemented and deployed in 0.11.2; ready for manual testing.** This is separate
from the future armor-system rework in the [Step 0 audit](armor-system-as-is-2026-11-04.md).

**Runtime regression found:** [save-load crash diagnosis](save-load-crash-armor-selection-2026-11-04.md) confirms that the new exotic-bearing baseline leaves native no-exotics armor selection empty for remote alien ground bases. The subsequent [alien-only selection fix](alien-armor-selection-fix-2026-11-04.md) is deployed: budget mode compares material prices without exotic exclusion, premium mode retains mass efficiency, and humans remain native.

## Approved behavior

Alien Adamantane keeps its native template identity, alien unlock and selection
rules, but adopts human Hybrid's physical properties, radiation protection,
laser resistance and material cost weights. Alien Exotic remains the superior
mass/thickness tier: 1.75 cm and 35 kg/m² per armor point, versus 2.5 cm and
50 kg/m² for the new Alien Adamantane. Both receive a 0.75 laser damage multiplier.

The user confirmed matching Hybrid's radiation protection **per armor point**
for Alien Exotic. Its physical half-value depths therefore scale with thickness:
3.15 cm for X-rays and 7.7 cm for baryons, giving 1.8 and 4.4 points per halving.
Its existing material-cost weights stay intact. Template IDs and display names
remain AlienAdamantaneArmor / AlienExoticArmor.

| Property | Alien Adamantane | Alien Exotic |
|---|---:|---:|
| Density, kg/m³ | 2,000 | 2,000 |
| Heat of vaporization, MJ/kg | 80 | 114.28571428571429 |
| Thickness, cm/point | 2.5 | 1.75 |
| Areal mass, kg/m²/point | 50 | 35 |
| X-ray physical half-value, cm | 4.5 | 3.15 |
| Baryon physical half-value, cm | 11 | 7.7 |
| Displayed points to halve X-rays / baryons | 1.8 / 4.4 | 1.8 / 4.4 |
| Laser damage multiplier | 0.75 | 0.75 |
| Volatiles / metals / nobles / exotics weights | .38 / .45 / .165 / .005 | .40 / .44 / .15 / .01 |

Native equations require `heatOfVaporization = 4000 / arealMass` and
`density = arealMass / thickness_m`. Thus the Exotic values are derived rather
than an unrelated density or mass patch. Floating-point readback uses tolerance.

The cost recipe is per ton: on a unit-area/per-point basis Adamantane uses
19 / 22.5 / 8.25 / 0.25 kg of those materials; Exotic uses
14 / 15.4 / 5.25 / 0.35 kg. Exotic requires 40% more exotics per unit-area point,
despite 30% lower total areal mass. Whole-ship armor costs also include native
geometry and realism-mode factors.

## Implementation and validation

Two partial `TIShipArmorTemplate.json` overrides replace their specialty arrays
explicitly through the manifest and are included in the project/package. All
other material templates and omitted fields are preserved. The existing installed
native JSON merger validator now exercises both overrides, checks the actual
native material getters for thickness/mass and shielding consistency, verifies
Hybrid-equivalent properties/costs, and preserves identities, unlocks and all
untouched records. The release archive check now requires the armor template.

Normal `tools/deploy.ps1` ran immediately after implementation, with both
game-closed checks and all verification enabled. The first copy attempt was
blocked by filesystem permissions after verification passed; rerunning the same
guarded deployment with the required filesystem access succeeded. The current
untested 0.11.2 package version used by the concurrent capital revision is retained.

This changes material data, not chipping, penetration, critical-hit, or spatial
damage algorithms. Native coverage loss divides by armor volume: thinner armor
can chip faster at equal point values. The chosen materials are lighter and
more compact; this does not promise superiority under every combat metric.
Existing designs refer to these identities, but native cached mass/damage state
and live-save behavior still require manual load/designer/combat checks.

## Outcome

Normal guarded build, **43 validators and 1,376 formula assertions passed**.
All **55 installed files** match the authored package, and the archived armor
template matches byte-for-byte. The [deployment receipt](alien-armor-deployment-2026-11-04.json)
records the assembly/template hashes and authored values. The implementation
matrix's existing alien propulsion/armor row now includes this material change;
the matrix validator passed after the edit (121 rows, 262 Harmony patches).

Manual checks remain: reload a campaign/skirmish, inspect both alien armor
tooltips (2.50/1.75 cm; 1.80/4.40 radiation points; 25% laser resistance),
compare designer armor mass/costs, and check existing designs in combat. Native
mass/cache/save behavior has not been freshly verified in-game. No commit made.
