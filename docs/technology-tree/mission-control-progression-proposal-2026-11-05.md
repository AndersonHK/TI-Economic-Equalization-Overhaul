# Mission Control capacity and yield research proposal

Date: November 5, 2026. Status: **proposal only**. No gameplay templates,
assemblies, settings, saves, or deployed files were changed for this proposal.

## Recommendation

Add a finite research progression reaching **+40% to the GDP-supported part of
national MC construction capacity** and **+40% national MC yield** for a faction
that obtains all nonexclusive proposed projects. Alien Bandwidth adds a further
5 percentage points of yield for the Servants, for a 45% maximum. Preserve all
existing project benefits, including their MC priority bonuses.

Use space development and administrative institutions for construction capacity,
computing advances for a modest worldwide yield improvement, and operational
projects for faction-specific yield improvements. This gives both global research
and faction engineering a role. It also lets research improve a fully developed
nation immediately while opening room for further investment.

The first balance pass should leave research prices, prerequisites, project
availability, and unlock chances unchanged. The listed bonuses are new,
additive percentage points, not replacements for existing benefits.

## Construction capacity: global technologies

Each bonus applies to every human nation's GDP-supported regional capacity once
the global technology is complete. It is independent of which faction controls
the nation. The rationale column is a proposed gameplay interpretation of the
technology's existing description, not an existing effect.

| Technology | Added cap bonus | Base RP | Why this fits |
|---|---:|---:|---|
| Ad Astra | +5% | 5,000 | Space-oriented education and institutions support a larger ground-control sector. |
| Space Commerce | +5% | 20,000 | Commercial operators and regulated traffic spread infrastructure costs. |
| Our Space Future | +5% | 30,000 | A mature Earth-space society supports a broader network of control facilities. |
| White Collar Automation | +5% | 40,000 | Automated administration makes additional centers affordable to staff and manage. |
| Integrated Earth-Space Economy | +10% | 40,000 | A permanent, integrated space economy supports a major expansion of terrestrial infrastructure. |
| Administration Algorithms | +10% | 50,000 | Autonomous management lowers the organizational burden of maintaining additional centers. |
| **Total** | **+40%** | | |

The milestones follow two intersecting branches, rather than a mandatory single
order. Our Space Future requires Space Commerce; Integrated Earth-Space Economy
requires Our Space Future. White Collar Automation requires Self-Repairing
Software and Applied Artificial Intelligence; Administration Algorithms requires
White Collar Automation and Quantum Encryption. Their base prices alone do not
describe the research needed to reach them.

### Exact cap rule

Retain the [existing regional GDP allocation, Education denominator, occupation
filter, and existing-MC floor](../economy/space-asset-upkeep-analysis-2026-09-22.md#current-mc-cap):

```text
C = sum(completed global technology cap bonuses) / 100
D = max(200, 300 - 6 * national Education)
regional structural cap = 1 + floor(regional GDP in billions / D * (1 + C))
regional actual cap = max(existing regional MC, regional structural cap)
national cap = sum(regional actual caps in regions not fully occupied)
```

Apply C before regional rounding. Do not multiply the existing-MC floor or the
free base slot. This preserves existing facilities without repeatedly granting
new capacity from already-protected facilities. It also avoids expanding every
small region's free slot merely because the same worldwide technology completed.
The displayed national maximum will not rise by exactly 40%: base slots,
per-region rounding, and protected existing facilities still matter.

Completing a technology opens construction headroom; it does not construct MC.
GDP and Education continue to affect the cap. Alien nations retain their current
rules, and these bonuses do not change the separate national Boost cap.

## Yield: global computing technologies

These bonuses improve usable MC from existing national facilities for every
human faction. Each represents more missions coordinated by the same installed
ground-control infrastructure.

| Technology | Added yield | Base RP | Why this fits |
|---|---:|---:|---|
| Self-Repairing Software | +5% | 10,000 | Less downtime and less operator attention spent correcting software. |
| Quantum Computing | +5% | 15,000 | More complex scheduling and operational models can be processed. |
| Applied Artificial Intelligence | +5% | 25,000 | Routine monitoring and decisions can be automated. |
| Mind and Machine | +5% | 35,000 | Improved human-machine interfaces let operators supervise more activity. |
| **Global subtotal** | **+20%** | | |

Self-Repairing Software is on the Neural Networks branch. Quantum Computing
requires Advanced Atomic Manipulation, Advanced Neural Networks, and Advanced
Heat Management Concepts. Applied Artificial Intelligence follows Quantum
Computing. Mind and Machine also requires Cybernetics and Arrival Psychology.
These are dependency gates, not predictions of the calendar year of completion.

## Yield: faction projects

| Project | Added yield | Base RP | Existing benefits and reason for selection |
|---|---:|---:|---|
| Space Traffic Control | +10% | 1,200 | Keep +25% MC priority efficiency; a network specifically built to coordinate space traffic deserves the largest operational bonus. |
| Their Computers | +5% | 5,000 | Keep +25% MC priority efficiency and +2% Knowledge priority efficiency; alien spacecraft-control computing also improves existing centers. |
| Devolved Space Command | +5% | 2,500 | Keep disruption resilience and its organization grant; on-site crews resolve more issues without ground intervention. |
| **Nonexclusive project subtotal** | **+20%** | | |
| Alien Bandwidth, Servants only | +5% | 2,500 | Keep +25% MC priority efficiency and the existing 100-MC resource grant; piggyback communications provide a small additional operational benefit. |

Space Traffic Control requires **both White Collar Automation and Space Commerce**;
its inexpensive project cost does not make its yield bonus an opening-game reward.
Their Computers requires Quantum Computing and Their Technology, with the latter
gated by the AccessAlienTech milestone. Devolved Space Command requires
Administration Algorithms and Industrialization of Space. Alien Bandwidth retains
its Servants faction restriction and Hydra Diplomacy / faction-objective gates.

"Nonexclusive" does not mean guaranteed availability: Space Traffic Control has
a 67% faction-availability value, Their Computers 100%, and Devolved Space Command
80%, with guaranteed availability for Project Exodus. Their existing unlock rolls
also remain. Thus 40% is an obtainable nonexclusive maximum, not an automatic
entitlement for every faction. The exact source fields are retained in the
[candidate evidence](mission-control-progression-evidence-2026-11-05.json).

### Exact yield rule

```text
Yg = sum(completed global yield bonuses) / 100
Yf = sum(this faction's completed project yield bonuses) / 100
national MC supplied to faction = existing eligible national MC share * (1 + Yg + Yf)
```

All yield bonuses add within one pool. The common maximum is 1.40x, not
1.20 x 1.20, and the Servants maximum is 1.45x if all listed projects are obtained.
Apply the bonus after the native control-point share calculation, preserving
occupation and disabled-benefit handling. Keep fractional MC through faction
aggregation; do not round down separately for each control point.

Yield does not create facilities, alter their physical construction cap, or
increase their serialized stock. It applies only to national MC received by the
faction, excluding MC from habs, organizations, difficulty grants, and direct
resource grants such as Alien Bandwidth's existing 100 MC. Retain the native
downstream disruption behavior. All human factions, including AI factions, use
the same completion and ownership rules.

Keep [national MC upkeep](../economy/national-space-assets-implementation.md#approved-plan-2026-09-23)
at 0.05 IP/month per installed facility: yield gains represent improved efficiency
of existing infrastructure. Building more facilities still increases upkeep.
At +40% yield, upkeep per supplied MC is about 28.6% lower, a material additional
benefit that supports using modest individual bonuses.

## Scale of the combined change

On the previously inspected December 8, 2042 India snapshot, holding GDP,
Education, regional population shares, and borders fixed:

| Illustrative cumulative bonuses | Physical MC built | Construction maximum | MC supplied if one faction receives all national MC |
|---|---:|---:|---:|
| Current rules | 198 | 198 | 198.0 |
| +5% cap, +10% yield | 198 | 204 | 217.8 |
| +15% cap, +20% yield | 198 | 222 | 237.6 |
| +40% cap, +40% yield | 198 | 265 | 277.2 |
| Same research after building all 67 additional slots | 265 | 265 | 371.0 |

These are sensitivity examples, not claims that this save has completed the
proposed research combinations. Raw save material and calculation intermediates
remain in ignored temporary storage. Source saves were read without modification.

At high GDP, the two research lanes together approach 1.40 x 1.40 = **1.96x**
the GDP-supported national supply after the extra facilities are built. In this
India example, full build-out gives about 87.4% more supply than its current 198.
Before constructing anything, the maximum common yield bonus gives 40% more.
This is a substantial finite progression; it should be playtested before adding
more milestones or increasing individual percentages. Existing hab/ship demand
reductions remain additional benefits and are not included in these totals.

## Candidates to leave unchanged

| Candidate | Reason to withhold a new cap/yield bonus |
|---|---|
| Photonic Computing; Advanced Neural Networks | Both already enable valuable branches. Neural Networks is granted at the mod's 2026 start; direct bonuses would be uneven starting-scenario grants. Their downstream computing milestones carry the yield improvements. |
| Quantum Encryption | Protects communications; its Administration Algorithms and other project unlocks already carry the relevant capacity progression. Avoid rewarding every prerequisite in the same chain. |
| Fleet Logistics; Interplanetary Polities | Already gate the existing ship/hab MC-demand reductions; leave their specialization intact. |
| Independent Commands; Independent Habitats | Already provide 15% reductions to their respective ship/hab MC costs. Additional yield would reward the same MC relief twice. |
| Operations Center; Command Center project unlocks | Unlock physical hab modules that supply MC when built. Keep their reward tied to construction rather than granting a nationwide bonus on research alone. |
| Transnational Coordination; Transnational Management | Already expand faction control-point capacity, enabling more national assets. Their existing faction-management role is sufficient. |
| Civilian Photonic Computing; Civilian Quantum Computing; Algorithmic Economic Management | Consumer computing and economic management already improve economic/administrative capacity. Keep MC improvements on the dedicated computing globals and space-control projects. |
| Strategic Deception; Operational Misdirection | Their role is MC-usage concealment, which should remain separate from actual capacity and yield. |
| Exploration technologies; Gold Rush | Do not restore the retired free-mine allowance under another name; retain the [tier-based mine rule](../mine-mission-control.md). |
| Accelerando; Future Techs; Management Research | Withhold further MC bonuses in the first pass. A capstone would extend an already large combined benefit, and repeatable bonuses would make progression unbounded. |

## Evidence and implementation boundary

The review read all **149 global technology records** and screened **750 project
records** by name, effects, AI role, and prerequisites, then inspected the English
descriptions and summaries of the relevant candidates. Sources are the installed
game templates merged with current repository overrides, not the historical
September tree snapshot. The [source records and hashes](mission-control-progression-evidence-2026-11-05.json)
record the 14 proposed entries, direct prerequisites, base prices, existing effects,
availability fields, and faction restrictions. Base RP excludes runtime research
cost multipliers and faction-specific modifiers. No research prices are proposed
to change.

The native regional cap and national-to-faction MC getters currently contain no
technology multiplier. Template changes alone cannot implement the specified
rules. If approved for implementation, inspect existing effect contexts first,
then keep the selected records and percentages in data and add only the runtime
evaluation needed for the two missing calculations. Completion should be derived
from actual researched technologies/projects, including those already completed
in old saves; do not accumulate a multiplier by replaying research-completion
events or modify the saved facility counts.

Implementation would also need UI distinctions between physical facilities,
construction maximum, and supplied faction MC; immediate income/eligibility cache
refresh after research, ownership changes, and loading; preservation of existing
effects during template merging; and automated checks of stacking, rounding,
occupation, partial ownership, research availability, and old-save behavior.
Normal verified deployment and manual in-game acceptance would follow the
repository workflow. None of those implementation or deployment steps has been
performed for this research proposal.
