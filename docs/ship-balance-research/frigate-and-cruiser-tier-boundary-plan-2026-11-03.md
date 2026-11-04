# Frigate and cruiser tier boundaries — small follow-up plan

**Later species-specific revision:** the [asymmetric tier plan](asymmetric-human-alien-tier-plan-2026-11-04.md)
keeps human Cruiser/Battlecruiser in T3 but places Alien Cruiser in T2 and Alien
Lancer in T3. It supersedes the mirrored alien assignments proposed below.

Status: **proposal only; no gameplay, research or save changes**. Date: 2026-11-03.
Baseline: installed TI 1.0.53b + EEO 0.10.23. This refines the lower bands of the
[formal six-tier plan](tier-4-6-and-size-5-implementation-plan-2026-11-03.md), using
the [audited hull/unlock data](hull-progression-audit-2026-11-03.json). The 14
comparison hulls were rechecked against current installed templates and repository
overrides; their relevant values still agree with that snapshot.

## Recommendation

Promote **Cruiser and Battlecruiser to formal ship tier 3**, alongside Battleship.
Promote **Frigate to formal ship tier 2**, alongside Monitor and Destroyer.
Apply equivalent role classifications to the alien counterparts. Keep the native
construction tier separate, as already proposed for tiers 4–6.

| Formal ship tier | Human classes | Human unlock project | Human MC cost |
| --- | --- | --- | ---: |
| 1 | Gunship, Escort, Corvette | Interplanetary Warships | 1 |
| 2 | Frigate, Monitor, Destroyer | Patrol Vessels | 2 |
| 3 | Cruiser, Battlecruiser, Battleship | Fleet Combatants | 3 |
| 4 | Lancer, Dreadnought | Ships of the Line | 4 |
| 5 | Titan | Titans | 5 |
| 6 | No existing human hull; Alien Mothership | Alien branch | Separate alien economics |

This makes tiers 1–5 match **both existing human unlock waves and MC costs**.
It also removes the earlier reason to consider a separate Battleship unlock
project solely to distinguish tiers 2 and 3. A tier denotes a strategic cohort
with different roles, not equal durability or identical maximum weapon size.

## Scrutiny of the two boundaries

Human values below are current appearance-0 bare mass, native integrity and
ordinary shared weapon cells. These ships currently have no compact-capital
mount conversion. Integrity is not a complete armored-survivability measure.

| Hull | Native construction tier | Bare mass (t) | Integrity | Nose / hull cells | Utilities |
| --- | ---: | ---: | ---: | ---: | ---: |
| Corvette | 1 | 400 | 8 | 1 / 1 | 3 |
| Frigate | 1 | 600 | 12 | 1 / 2 | 5 |
| Monitor | 2 | 700 | 16 | 0 / 4 | 3 |
| Destroyer | 2 | 900 | 18 | 2 / 2 | 5 |
| Cruiser | 2 | 1,000 | 20 | 2 / 3 | 7 |
| Battlecruiser | 2 | 1,200 | 24 | 3 / 2 | 5 |
| Battleship | 3 | 1,600 | 40 | 2 / 6 | 6 |

**Frigate is geometrically upper tier 1, but functionally lower tier 2.** It is
still `smallHull` at exactly 100 m, but human Monitor and Destroyer are also
100 m and currently classified Small. That boundary cannot determine ship tier.
Compared with Corvette, Frigate adds 50% mass/integrity, one hull cell and two
utilities. Its five utilities match Destroyer, while its weaker single-cell
nose explains its lower mass. Patrol Vessels unlock and MC 2 place it naturally
with the first escort group. It should be the light, utility-oriented tier-2
choice rather than receive extra guns merely to justify promotion.

The alien case is stronger: Alien Frigate has 606 t bare mass / 20 integrity,
versus Alien Corvette's 297 / 10 and Alien Destroyer's 732 / 24. Frigate and
Destroyer both have four total weapon cells and five utilities; they divide
their weapon cells 1 nose + 3 hull versus 2 + 2. That is a plausible same-tier
role tradeoff. Alien MC is uniformly 1 here, so human MC reasoning cannot be
applied to the alien roster.

The subsequent [all-class volume audit](hull-volume-capacity-comparison-2026-11-03.md)
places human Frigate at the lower capacity end of tier 2: median main hull
16,191 m³ / reactor bay 954.5 m³, versus Destroyer's 52,876 / 1,352.1. This
supports a lighter utility escort, not equal physical capacity within the tier.

**Cruiser is the weakest tier-3 boundary in direct combat.** Against human
Destroyer it gains only 11% bare mass, 2 integrity and one hull cell; nose size
remains 2. Its defining upgrade is seven utilities instead of five, plus the
later Fleet Combatants project and MC 3. Put it at the lower edge of tier 3 as
the general-purpose/endurance cruiser. Do not describe it as a lighter Battleship:
Battleship has double its integrity and hull-cell count, at the same MC, while
Cruiser is cheaper/lighter and has one more utility. Whether that utility and
support advantage earns its MC is a genuine balance question to test later.

Alien Cruiser more clearly crosses the boundary: 1,131 t / 36 integrity / four
hull cells / seven utilities versus Alien Destroyer's 732 / 24 / two / five.
It is 55% heavier and 50% tougher by raw integrity. Its tier-3 placement is
well supported independently of the human unlock grouping.

The same volume audit strengthens human Cruiser's support-role case: median
main hull 120,065 m³ and bay 2,747.4 m³, respectively 2.27x and 2.03x Destroyer.
Individual appearances overlap; alien bay values are size-band fallbacks, so
they cannot establish a measured alien reactor-capacity boundary.
The follow-up [alien asset audit](alien-reactor-bay-measurements-2026-11-04.md)
now proposes geometry-bounded candidates: Alien Cruiser about 2,332 m³ versus
Destroyer 2,572. This reinforces the need to distinguish role/integrity from a
strict physical-capacity ladder; these inputs are now deployed in 0.10.24.

Battlecruiser belongs with Cruiser: same human project and MC, trading two
utilities and one hull cell for one extra nose cell, 20% mass and 20% integrity.
Battleship remains the much tougher broadside branch of the same cohort. Weapon
roles, not a demand for equal stats, justify all three sharing tier 3.

## Scope and implementation sequence

1. Update the planned authoritative ship-tier catalog and UI/grouping to the
   table above, including species-specific entries. Tier stays attached to the
   hull class, not a graphical appearance or measured mesh-size ranking.
2. Preserve existing unlock projects, MC, slots, mass, crew and weapon permissions
   for this classification pass. In particular **tier 3 does not grant compact
   heavy turrets or multiple large noses**: those remain the tier-4+ rule in the
   larger plan. Existing Frigate, Cruiser and Battlecruiser fits remain valid.
3. Preserve native `consTier` initially: Frigate 1; Cruiser/Battlecruiser 2.
   Changing it is a separate mechanical decision. A 1 -> 2 or 2 -> 3 change adds
   5 MW of native systems demand, changes construction-time modifiers, utility
   eligibility and AI behavior; tier >=3 also invokes the native maximum-engine
   selection. For human Cruiser 2 -> 3, T1/T2 yard modifiers become 50% slower
   and T3 becomes 67% slower before other effects. A label change must not
   introduce those penalties accidentally.
4. Validate all hull mappings, exact unlock/MC alignment for humans, alien
   exceptions, appearance-independent classification and unchanged legal fits.
   When implemented, use the normal build/deploy/test workflow. No save-layout
   migration is needed for classification alone; any new persistent metadata
   must still have a safe missing-field default.
5. Test Frigate vs Corvette/Destroyer and Cruiser vs Destroyer/Battleship on
   matched equipment, equal MC and equal resource budgets. Assess utility/support
   missions as well as duels. If Cruiser still fails to earn MC 3, tune its role
   separately; do not preemptively add compact mounts or a blanket damage buff.

Recommended result: **tier-2 Frigate and tier-3 Cruiser/Battlecruiser**, with
their boundary roles made explicit. This produces the cleanest progression
using existing research, while reserving actual combat and infrastructure
retuning for measured follow-up decisions.
