# Missile roles and alien launcher exploration

Status: initial exploration followed by an approved alien magazine increase, 2026-09-25. The 0.9.9 template change is recorded below. Build, tests, validation and deployment are skipped for this follow-up at the user's request.

## Approved implementation: alien ammunition capacity ×2

Add a sparse `TIMissileTemplate.json` override containing only each alien launcher's ID and doubled magazine capacity. Advance `ModInfo.json` from 0.9.8 to 0.9.9. The user scoped this commit to documentation and JSON changes; assembly source version fields and the existing compiled DLL are not changed in this turn.

| Launcher ID | Previous magazine | 0.9.9 magazine |
|---|---:|---:|
| GlitteringJewelMissileBay | 16 | 32 |
| GlitteringJewelMissilePod | 4 | 8 |
| IridescentStarTorpedoBay | 8 | 16 |
| LuminousSwarmMissileBay | 36 | 72 |
| BrilliantSkyMissileBay | 16 | 32 |
| PredatoryStarTorpedoBay | 8 | 16 |

This covers all six alien entries, including the fighter pod and torpedo bays. Human templates, salvo size, cadence, warheads and guidance values retain their existing values. Normal magazine accounting increases loaded ammunition mass and resource cost with capacity. Runtime behavior, including ammunition on existing saved ships, has not been tested for this change.

The workbook, CSV and audit JSON remain the **pre-change installed-data baseline** from the exploration. Their current-value columns do not include this undeployed override. The exploratory options below are retained as research; only the magazine increase above is approved and implemented in source.

Completion for this follow-up: JSON override and package version authored; no build, tests, validation or deployment run, as authorized. A future normal release should synchronize assembly version metadata and rebuild before deployment.

## Question and working plan

Make guided weapons useful against maneuvering ships that evade magnetic fire, and against capital ships after their escorts are removed. Improve alien missile usefulness without making human escort spam stronger or merely increasing the opening salvo's lethality.

The initial pass inventories the installed missile templates, compares launcher throughput and ammunition endurance, inspects the installed combat code, and produces an Excel comparison with editable alien tuning inputs. Except for the subsequently approved magazine increase above, candidate changes remain experiments, not approved balance values.

## Deliverables

- [Missile comparison and alien tuning workbook](tables/missile-comparison-2026-09-25.xlsx).
- [Complete flattened installed-template CSV](tables/missiles-2026-09-25.csv).
- [Source snapshot, hashes, and predefined missile ship inventory](tables/missile-audit-2026-09-25.json).

## Design direction

Missiles should impose a persistent tactical threat: maneuver, stay near screening ships, turn vulnerable facings away, divert defensive fire, or expend countermeasures. A missile can contribute without hitting if it creates an opening for another weapon. That depends on movement and fire-control behavior as much as on damage values.

Three distinct roles are useful:

| Role | Desired advantage | Limitation and counterplay |
|---|---|---|
| Pursuit missile | Acceleration, turning and enough delta-v to punish isolated mobile ships | Modest payload; retreat toward screening ships; exhaust its maneuver reserve |
| Heavy torpedo | Dangerous payload against an exposed capital ship | Lower agility and launch throughput; escorts can intercept it |
| Swarm missile | Concentrated defensive workload, supporting other weapons | Small individual hits, finite ammunition and dependence on coordinated arrival |

The alien catalog already offers candidates: Glittering Jewel / Brilliant Sky for pursuit, Iridescent Star / Predatory Star for heavy torpedoes, and Luminous Swarm for small guided rounds. These are proposed roles, not proof that the existing AI uses them that way.

Increasing missile damage alone does little when no rounds survive. Increasing salvo size can defeat more PD but moves the same binary threshold. Increasing speed also increases kinetic penetrator damage, so it is not a pure survivability adjustment. More delta-v helps pursuit only if guidance preserves useful maneuver capability; launch range is not a guarantee of interception.

Heavy ships with enough of their own PD will remain missile-resistant after escorts die. The desired escort relationship also requires checking hull slot costs, defensive weapon opportunity costs, firing arcs and fleet spacing. Missile values alone cannot guarantee it.

## Experiments to consider

1. **Alien ammunition endurance first:** test a 1.5–2× magazine increase on deployed bays, retaining salvo count, launch timing and warheads. This creates additional attempts without raising the first salvo's size. It will not penetrate a stable, completely adequate PD screen by itself. Include the resulting ammunition mass and resource costs; do not buff mass-free magazines. Review fighter pods separately.
2. **Pulse compression, separately:** halve intra-salvo spacing and increase the post-salvo reload to preserve the original start-to-start cycle. For Glittering Jewel, 4 shots at 1-second spacing plus 4 seconds reload becomes 4 shots at 0.5-second spacing plus 5.5 seconds reload. It makes a sharper pulse without increasing long-run throughput, but remains a saturation mechanic. Luminous Swarm also has a defensive role, so its defensive behavior needs its own check.
3. **Roster and AI use:** test actual missile-focused alien designs and the currently unused torpedoes/swarm launcher. Coordinate arrival rather than merely launch time; account for overlapping friendly PD, target motion and missiles already committed. Prefer isolated mobile targets, then exposed capitals. Preserve a bounded ammunition reserve for later opportunities rather than withholding every shot indefinitely.
4. **Runtime durability, only if needed:** give appropriate missiles a limited damage budget and let heavier torpedoes demand more defensive work. This requires damage-path and PD-target-selection changes together. Raising a template mass or launcher HP field does not implement missile durability. Avoid making every human missile harder to stop, which directly strengthens escort spam.

Start with controlled, separate experiments before combining changes. Endurance is the smallest useful alien-only data pass; AI timing and target selection are the strongest candidates for creating the intended roles. Global PD nerfs, global missile HP buffs, blanket PD immunity, and larger nuclear warheads are poor first experiments because they also magnify the human opening-strike strategy.

## Future manual test design

Use identical fleets, initial range, relative velocity, formation, ship modules and commands across comparisons. Repeat fights because firing jitter, ECM and collision outcomes vary. Keep autoresolve results separate from manual combat.

| Test | What it answers |
|---|---|
| Mobile small target alone, then near a PD escort | Do pursuit missiles force maneuver or punish separation? |
| Capital alone, then with two and four escorts | Does escort survival meaningfully control torpedo threat? |
| Dense stationary PD formation versus separated ships | Does geometry matter, or only total PD count? |
| Screening ships removed partway through combat | Is useful ammunition still available afterward? |
| Matched fleet investment using human escort spam | Does the change unintentionally improve the dominant human strategy? |
| Same alien hulls with alternate launcher mixes | Is the weakness the launcher, its number of mounts, or loadout selection? |

Record missiles launched, intercepted, missed/ECM-defeated and impacting; first/last useful salvo time; ammunition remaining when screens collapse; target maneuver/delta-v cost; defensive fire diverted from magnetic rounds; ship damage and losses. Define acceptable outcomes after observing the baseline, rather than inventing a hit-rate target now.

## Evidence and workbook definitions

The installed local templates are authoritative for this pre-change snapshot; community tables may describe a different version. There are **57 entries: 51 human and 6 alien**, including half-hull pods. At the time of the initial exploration, no missile template override or missile-specific damage patch was found in the repository. Other mod changes, including firing checks and direct-fire coordination, can still affect the surrounding battle.

| Alien launcher | Rounds | Shots per salvo | Cycle, seconds | Nominal shots/minute | First-to-last launch, seconds | Acceleration, g | Delta-v, km/s |
|---|---:|---:|---:|---:|---:|---:|---:|
| Glittering Jewel Bay | 16 | 4 | 7 | 34.29 | 24 | 23.89 | 7.16 |
| Glittering Jewel Pod | 4 | 1 | 6 | 10.00 | 18 | 23.89 | 7.16 |
| Iridescent Star Torpedo Bay | 8 | 1 | 8 | 7.50 | 56 | 11.55 | 14.21 |
| Luminous Swarm Bay | 36 | 8 | 10 | 48.00 | 43 | 41.99 | 5.78 |
| Brilliant Sky Bay | 16 | 8 | 11 | 43.64 | 18 | 58.17 | 19.03 |
| Predatory Star Torpedo Bay | 8 | 1 | 6 | 10.00 | 42 | 24.95 | 19.35 |

These times start with the first launch at time zero and assume uninterrupted ideal firing. They exclude initial acquisition, jitter, ECM, disabled weapons, ammunition utilities, officer modifiers and AI pauses. A nominal shots/minute rate is not the number of rounds a small magazine can actually fire in a minute.

For comparison, Anaconda has 8-round salvos, 1-second spacing and 5-second reload: a 12-second cycle, 40 nominal rounds/minute, and 19 seconds from first to last launch from its 16-round magazine. Glittering Jewel therefore has superior kinematics but does not launch a larger opening salvo or sustain a higher per-launcher rate than that early human bay. Brilliant Sky already has 58.17 g acceleration and 19.03 km/s delta-v, suggesting that further across-the-board kinematic buffs should not be the default answer.

The predefined alien roster contains eight distinct missile-carrying designs and nine design/weapon pairs. All eight have Glittering Jewel bays or pods; the Mothership also has one Brilliant Sky bay. Bay counts are generally one or two. No predefined design contains Luminous Swarm, Iridescent Star or Predatory Star. This is a template inventory, not measured campaign spawn frequency or proof of every generated design's equipment. Audit generated designs and tech-based substitutions before changing the roster.

### Installed combat-code findings

The installed `Assembly-CSharp.dll` was freshly decompiled for the relevant types. SHA-256: `4a4b9aae4154e444e9727204205d2d42ae8ed9e1c5f92cdc1280074a259d8350`. Repository metadata targets Terra Invicta 1.0.53b / mod 0.9.8; the hash identifies the exact assembly inspected.

- `TIShipWeaponTemplate.salvo_shots` defaults to 1; omitted intra-salvo timing and flat damage default to zero. `averageCooldown_s` confirms `(reload + (salvo − 1) × gap) / salvo`.
- `TIProjectileWeaponTemplate.buildMass_tons` adds the ammunition magazine to the empty launcher. Glittering Jewel weighs 3 t empty but 23.48 t loaded. Doubling its magazine from 16 to 32 raises loaded mass to 43.96 t and first-to-last launch time from 24 to 52 seconds.
- `MissileController.ApplyDamage` immediately sets `beenDestroyed = true`. It does not subtract damage from a missile HP pool. Nuclear/AOE missiles have a conditional nearby explosion path. `TIProjectileWeaponTemplate.minDamageForPDToFire` uses global `DP_DestroyMissile`, whose class default is 0.15 damage points; the installed global JSON does not override that field. This is a defensive firing threshold, not a missile hit-point budget. Increasing `systemMass_kg`, `ammoMass_kg` or launcher HP does not by itself create a tougher missile.
- `FindMissileTargetShipLeafNode.TryAssignTarget` selects the nearest eligible unsaturated target in the inspected path. Its role-specific constructor settings do not turn that method into an isolation/escort-screen assessment. `FireMissilesLeafNode.Execute` also honors saturation, with a structural-damage exception.
- `CombatShipController.EstimatedMaxProjectilesPointDefenseCanHandle` sums `ceil(120 / cooldown)` for operable hull weapons with defensive or guardian fire modes. `EstimatedIncomingMissileDamage` ignores that many incoming rounds before summing estimated damage; `UpdateIsMissileSaturated` compares the remainder with a hull/armor-based kill threshold. It **does account for PD**, but does not model actual arrival timing or nearby escorts in this calculation. Whether this is responsible for observed alien failures remains a hypothesis requiring combat observation.
- `MissileController.Fire` passes acceleration, delta-v, rotation, thrust ramp, turn ramp and maneuver angle into the projectile movement system. `TIMissileTemplate.EstimatedImpactVelocity_kps` uses half delta-v; actual impact damage uses relative velocity. Template guidance fields are real tuning inputs, but this pass did not fully audit the guidance job or prove how each changes pursuit outcomes.
- Existing direct-fire coordination code explicitly excludes missiles from relevant targeting paths. Improvements made for magnetic/beam coordination should not be assumed to coordinate alien missile salvos.

### Workbook calculation definitions

The comparison tab puts aliens first and links the installed values from the source tab. The source tab retains source order, absent fields as blanks, all original fields and flattened nested material weights. Friendly names and IDs are both retained, including the source's `PoseidonTorpedoBay` / Hestia naming.

Let `S` be salvo size, `G` the intra-salvo gap, `R` the post-salvo reload and `M` the magazine:

- Cycle = `R + (S − 1) × G`.
- Nominal rounds/minute = `60 × S / cycle`.
- First-to-last launch = `floor((M − 1) / S) × cycle + ((M − 1) mod S) × G`.
- Loaded launcher tonnes = empty tonnes + `M × ammoMass_kg / 1000`.
- Nominal kinetic MJ = `0.5 × warheadMass_kg × (deltaV_kps / 2)^2`, multiplied by 0.1 for explosive warheads, and zero for nuclear, shaped nuclear and antimatter warheads. Add `flatDamage_MJ` for nominal total damage.
- Direct share = nominal total × `(1 − flatChipping)`. The remainder represents chipping allocation, not energy that simply disappears. Armor interaction, projectile survival, ECM, relative ship motion, nuclear area/cone geometry and faction/officer effects are not simulated.
- Nominal MJ/minute = nominal total per shot × nominal rounds/minute. This is a conditional damage rate while ammunition lasts, not expected battle DPS.

The alien tuning tab starts at the current values. Amber inputs edit magazine count, intra-salvo gap and reload; formulas update cycle, rate, exhaustion time and loaded mass. It includes the predefined alien mount inventory. Its examples describe separate experiments, not a combined recommended patch. It is not connected to game files.

### Verification

Checked 57 unique template IDs, all six alien entries, source-preserving CSV extraction, every row's cadence/exhaustion arithmetic and nominal damage calculation. Tested and restored a doubled Glittering Jewel magazine and a compressed pulse preserving its seven-second cycle. Formula inspection found no errors. Rendered and reviewed all three worksheets plus the roster and damage columns. Exported the Excel file with formulas and cached results; native desktop Excel behavior has not been tested. No build or deployment was run.
