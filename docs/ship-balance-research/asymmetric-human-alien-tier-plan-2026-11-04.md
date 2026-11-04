# Asymmetric human and alien ship tiers — revised proposal

Date: 2026-11-04. **Implemented in 0.11.0 after approval.** See the
[implementation, final tables and test status](asymmetric-tiers-implementation-2026-11-04.md).
The proposal below is retained as the decision baseline; its conditional Carrier
choice was resolved in favor of six independent heavy hull mounts.

Original proposal status: **Tier/fitting proposal only.** The alien machinery-bay lookup
is separately implemented and deployed in 0.10.24; none of the new tier or N5
rules below are implemented. This is the latest classification proposal and
supersedes the one-to-one species mapping / occupied tier-6 band in the
[earlier plan](tier-4-6-and-size-5-implementation-plan-2026-11-03.md).

## Proposed categories

The user's requested moves are Alien Cruiser -> 2, Lancer -> 3, Titan and
Assault Carrier -> 4, and Mothership -> 5. Unmentioned assignments retain the
previous proposal: Alien Frigate/Monitor/Destroyer in 2, Battlecruiser/Battleship
in 3, and Dreadnought in 4. These are proposed **ship tiers**, not changes to
the current native `consTier` field.

| Ship tier | Human classes | Alien classes |
| --- | --- | --- |
| 1 | Gunship, Escort, Corvette | Gunship, Escort, Corvette |
| 2 | Frigate, Monitor, Destroyer | Frigate, Monitor, Destroyer, **Cruiser** |
| 3 | Cruiser, Battlecruiser, Battleship | Battlecruiser, Battleship, **Lancer** |
| 4 | Lancer, Dreadnought | Dreadnought, **Titan, Assault Carrier** |
| 5 | Titan | **Mothership** |

There is no currently occupied tier 6 in this revision. It can remain reserved;
do not invent a human Mothership counterpart to fill it. STO Fighter and
Salamander remain specialized auxiliaries outside this naval tier table.

## Do the categories cluster?

The [cluster snapshot](asymmetric-tier-clusters-2026-11-04.json) contains all 26
ranked hulls, native construction tiers, current equipment/cost/integrity values,
V0 and per-class appearance statistics, and gap ratios. Bay inputs were re-read
through the deployed 0.10.24 production methods. Main-hull measurements are the
maintained external envelopes, not solid/habitable volume.

Tables use **minimum / median / maximum across the classes in each tier**.
Both columns below use appearance 0; the median is across hull classes, not
across ships or all pooled skins. Human alternative-art and class-median data
remain in the snapshot and the [all-class capacity report](hull-volume-capacity-comparison-2026-11-03.md).

### Human V0 grouping

| Tier | Classes | Main hull V0, min / median / max (m³) | Current bay V0, min / median / max (m³) |
| --- | --- | ---: | ---: |
| 1 | Gunship, Escort, Corvette | 5,071 / 6,702 / 11,020 | 264 / 264 / 264 |
| 2 | Frigate, Monitor, Destroyer | 13,692 / 17,160 / 22,062 | 332 / 385 / 385 |
| 3 | Cruiser, Battlecruiser, Battleship | 43,684 / 45,820 / 105,143 | 1,989 / 1,989 / 5,648 |
| 4 | Lancer, Dreadnought | 152,598 / 212,414 / 272,229 | 2,366 / 6,921 / 11,476 |
| 5 | Titan | 514,552 / 514,552 / 514,552 | 15,956 / 15,956 / 15,956 |

### Alien grouping

| Tier | Classes | Main hull V0, min / median / max (m³) | Current bay V0, min / median / max (m³) |
| --- | --- | ---: | ---: |
| 1 | Gunship, Escort, Corvette | 7,358 / 10,892 / 13,174 | 178 / 263 / 318 |
| 2 | Frigate, Monitor, Destroyer, Cruiser | 30,660 / 52,631 / 75,476 | 776 / 1,742 / 2,572 |
| 3 | Battlecruiser, Battleship, Lancer | 166,672 / 172,808 / 201,325 | 3,545 / 5,860 / 9,486 |
| 4 | Dreadnought, Titan, Assault Carrier | 302,990 / 349,989 / 419,182 | 6,013 / 6,866 / 14,626 |
| 5 | Mothership | 49,197,068 / 49,197,068 / 49,197,068 | 137,457 / 137,457 / 137,457 |

**Alien tiers 1–4 now cluster particularly well by main-hull envelope.** Their
ranges do not overlap. The next tier's smallest hull is 2.33x, 2.21x and 1.50x
the preceding tier's largest, respectively. Moving the small Cruiser into 2
and the relatively compact Lancer into 3 removes the two earlier boundary
problems without resizing art or fabricating capacity bonuses.

Native alien integrity also forms nonoverlapping ranges: **6–10 / 20–36 /
48–60 / 72–90 / 512** for tiers 1–5. This independently supports the new cohorts,
although integrity alone is not armored durability or combat value. Human
integrity retains a role exception: tier-4 Lancer's 36 is below tier-3
Battleship's 40, as expected for a more fragile nose specialist.

Human V0 also has nonoverlapping tier ranges. Alternative human art still
overlaps neighboring tiers: for example, the largest Destroyer exceeds the
smallest Cruiser, and some Cruiser/Battlecruiser art exceeds Battleship art.
The revised labels do not repair those physical differences; retain the
[art-family/mesh analysis](alien-reactor-bay-measurements-2026-11-04.md) as a
separate decision about appearance-dependent capacity.

**Tier 5 is not a tight cross-species size cluster.** Mothership is about
95.6x Human Titan V0 main-hull envelope, or 71.3x its class-median envelope.
Even the largest human Titan appearance is about 49.8x smaller. The current
Mothership bay proxy is about 8.6x the human Titan class-median bay, and its
integrity is 512 versus 64. Sharing tier 5 therefore means “top strategic
class within that species,” not equal physical size or equal fleet value.
Keep a distinct Mothership/boss role and production budget inside tier 5;
do not shrink it to make the labels numerically equivalent.

Reactor allowances are not strictly ordered even within one species: alien
tier-3 Lancer is about 9,486 m³, above tier-4 Dreadnought/Carrier at 6,013/6,866.
The tier-4 Titan reaches 14,626. These are role/geometry differences and partly
modeled machinery regions, not grounds to force every capacity number into a
monotonic tier sequence. All current tier-3/4/5 alien reactor families still
retain their template output ceilings under 0.10.24.

## Fitting consequences under the existing N5 proposal

Make grade eligibility **tier-based**, not a string test for “Titan”:
only tier 5 may use N5, and its occupied nose slots require N5. This means
Human Titan and Alien Mothership, exactly as the new classification indicates.
Alien Titan at tier 4 uses ordinary N4. A “both Titan identities may use N5”
rule would instead be an explicit tier-4 exception; it must not happen
accidentally because of a class-name match.

| Hull | Current fitting | Proposed fitting under the revised tier rules |
| --- | --- | --- |
| Human Lancer, T4 | Shared 4-cell nose; 3 ordinary hull cells | 4 independent N4 + 3 heavy hull mounts |
| Human Dreadnought, T4 | 3 independent N4 + 8 heavy hull mounts | Same counts; N5 variants unavailable |
| Human Titan, T5 | 4 independent N4 + 6 heavy hull mounts | 4 N5 + 6 heavy hull mounts |
| Alien Cruiser, T2 | Shared 2-cell nose; 4 ordinary hull cells | Existing native/shared fitting |
| Alien Lancer, T3 | Shared 6-cell nose; 4 ordinary hull cells | Existing native/shared fitting; **no compact conversion** |
| Alien Dreadnought, T4 | 4 independent N4 + 8 heavy hull mounts | Same counts; N5 variants unavailable |
| Alien Titan, T4 | 6 independent N4 + 8 heavy hull mounts | Same counts; **no N5 promotion** |
| Alien Assault Carrier, T4 | No nose; 6 ordinary hull cells | No nose; 6 heavy hull mounts **if the general T4 compact rule applies** |
| Alien Mothership, T5 | 6 independent N4 + 16 heavy hull mounts | 6 N5 + 16 heavy hull mounts |

The Carrier is the new fitting decision introduced by this proposal. Uniform
tier-4 compact hull fitting would quadruple its hull-cell capacity and remove
small turret/PD/missile choices under the current heavy-only approach. It adds
no nose slots. If transport-defense fitting should remain native, declare a
Carrier role exception in the authoritative catalog; merely assigning tier 4
does not silently resolve that choice. Its troop/logistics identity must survive
either implementation.

The five initial N5 templates and the 2x-own-throughput cadence change from the
earlier plan remain proposals. Under the strict tier-based rule, the two alien
N5 candidates (Relativistic Particle and 1024 cm Graser) become Mothership-only;
Alien Titan can still use their ordinary N4 counterparts. The smaller 768 cm
graser remains its existing nose-size category. The
[fractional wavelength fix proposal](laser-wavelength-precision-bug-2026-11-03.md)
still covers both graser sizes before final optical balance assessment.

## Revised initial mass/crew implications

Use the same proposed 20 t reinforcement per added effective unit, +1 human base
billet per unit, 3 t human crew support and nearest-100-t bare rounding. These
remain provisional planning prices, not data changes in 0.10.24.

| Hull | Added effective units from current fitting | Proposed base crew | Current -> proposed V0 bare mass (t) |
| --- | ---: | ---: | ---: |
| Human Lancer | 21 | 14 -> 35 | 2,000 -> 2,500 |
| Human Titan | 4, for N4 -> N5 | 52 -> 56 | 3,700 -> 3,800 |
| Alien Lancer | **0** | 40 | **1,656 -> 1,656** |
| Alien Titan | **0** | 60 | **3,000 -> 3,000** |
| Alien Assault Carrier | 18, **if compact heavy hulls are selected** | 80 | 2,928 -> 3,300 |
| Alien Mothership | 6, for N4 -> N5 | 100 | 8,900 -> 9,000, subject to separate strategic-hull pricing review |

This withdraws the earlier Alien Lancer 2,300 t and Alien Titan 3,100 t targets:
their respective compact/N5 conversions no longer occur. A tier label alone
does not change mass or round the existing 1,656 t Lancer to another value.
The Carrier's conditional structural target is 3,060 t plus its unchanged
80 crew at 3 t each. Existing Dreadnought values remain unchanged.

Weapon equipment, magazines, reactor/radiator/battery demand and their resource
costs are additional. Human MC 1–5 still follows its hull-project ladder;
standard alien MC remains 1 in current data. Do not automatically turn the
new alien tier into its MC cost or claim matching tier means equal economics.
Mothership's 9,000 t bookkeeping target needs a dedicated cost/role decision,
because its hull geometry is so far outside the human tier-5 size range.

## Implementation plan after category review

1. Keep the already deployed 0.10.24 bay update independent of this proposal.
   Store species-specific tier/role records for every naval hull; no one-to-one
   class-name equivalence and no changes to native construction-tier semantics.
2. Use the authoritative tier/role mapping consistently in UI, weapon grade
   eligibility, AI, capacity pricing and save/refit validation. Resolve the
   Carrier compact/transport exception explicitly before implementing fitting.
3. Add Human Lancer to compact model/slot handling. Do not add Alien Lancer.
   Retain Alien Titan's existing compact N4 behavior. Carrier model anchors,
   PD availability and payload role need validation if heavy fitting is selected.
4. Apply N5-only noses only to Human Titan and Alien Mothership. Preserve the
   agreed exact cadence-doubling method, and the graser's still-open 1.333x
   post-doubling raw output versus xaser acceptance issue. No fallback regular
   nose weapons or granted research on new tier-5 designs.
5. Update migration preflight: regular N4s on the two tier-5 hulls need explicit
   N5 counterparts; weapons newly tagged N5 on tier-4 Alien Titan/Dreadnought
   become lower-tier conflicts. Preserve hardware/fuel/state best-effort on
   existing ships while moving saves onto the current rules; editing/refitting
   applies strict limits. Do not silently replace an unsupported family or
   discard expensive equipment. Review actual conversion/resource deltas first.
6. Validate classification, fitting, stock/new AI designs, empty/unresearched
   grade outcomes, model anchors, capacity/resource calculations and schema-0/1/2
   load/refit/import/queue behavior. Then use normal guarded deployment and live
   category/fitting/combat/save acceptance. Classification tables alone are not
   implementation or balance proof.

Assessment: **the revised alien tiers 1–4 are cleaner geometric cohorts** than
the one-to-one species plan. Tier 5 is a deliberate strategic-ceiling category
containing very unequal hulls. The remaining fitting and economic decisions
should acknowledge that asymmetry rather than erase it.
