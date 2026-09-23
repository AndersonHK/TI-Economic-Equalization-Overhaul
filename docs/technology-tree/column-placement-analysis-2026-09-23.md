# Technology-tree columns, research prices, and the Future Tech overlap

Date: 2026-09-23. Status: pre-implementation research record. Companion to
[`technology-tree-adjustments-plan-2026-09-23.md`](../technology-tree-adjustments-plan-2026-09-23.md).

Implementation follow-up: the user approved the "Requested changes" draft,
which was implemented and deployed on 2026-09-23. In this historical analysis,
"current" and "baseline" mean the data before that implementation. The updated
preview defaults to the implemented repository and retains the former data as
"Before changes". The optional discount schedule and Future Tech UI correction
remain proposals; manual in-game validation is pending.

Loader correction after manual testing: the initial offline model assumed that
authored arrays replace inherited arrays. The game's default mode instead
merges by index, so the empty Neural Networks prerequisite override retained
Photonic Computing until `TITechTemplate.json` was added to the manifest's
`TemplatesToReplaceArrays`. Native-merger validation now verifies all 149
resolved technologies against their authored overrides and preserved baseline
fields. The historical before-change predictions below model the authored
arrays; they are not measurements of that earlier faulty deployed merge.

## Findings

**Increasing Future Tech prices will not normally put them in a separate
column.** Their placement uses a special end-game routine, after ordinary
cost and prerequisite placement. The code contains inconsistent final-column
bookkeeping that can put them in the same physical column as the rightmost
ordinary technology. An offline model reproduces the reported Proton-Proton
Fusion overlap.

The requested 40,000 costs for D-T Fusion and Nuclear Fusion Methodologies
are enough in the model to move the main fusion progression one column left.
Advanced Neural Networks also moves one column left with a 2,500 cost and no
prerequisite. With those changes, the model leaves Future Techs alone in the
last column without increasing their prices. That is a favorable result for
this data set, rather than a guarantee that the final-column bug is fixed.

There are two remaining placement conflicts: Magnetic Plasma Confinement
cannot share the column of its High Temperature Superconductors prerequisite,
and Proton-Proton remains after Clean Energy and Terawatt Fusion. Costs are
not direct column coordinates, so pricing alone needs to be evaluated against
the whole tree.

## Evidence and numbering

All column numbers below are **one-based**, including the first starter
column. Native code stores zero-based `node` indices. Thus report column 15
is native `node == 14`.

The analysis uses the installed game's templates merged with the current
repository's technology overrides: 149 global technology templates, including
seven Future Techs. The repository targets Terra Invicta 1.0.53. The exact
installed `Assembly-CSharp.dll` inspected has SHA-256:

`4A4B9AAE4154E444E9727204205D2D42AE8ED9E1C5F92CDC1280074A259D8350`

Primary evidence is the installed implementations of:

- `ResearchScreenController.InitializeFullTechTree`, `BuildTree`,
  `PlaceTechsBehindPrereqs`, `PlaceTechsBehindLowerCosts`,
  `PlaceTechsBehindSameTierPrereqs`, `HandleEndGameTechs`, and `SetupSpacing`;
- `TIGenericTechTemplate.TechPrereqs`;
- `TIGlobalResearchState.PostGlobalGameStateCreateInit_2` and `GetAllTechs`;
- `TemplateManager.IterateByClass`; and
- `ChildTechGridItemController.Init` and its `node`/`visited` fields.

The repository's `GlobalTechnologyResearchCostPatch` supplies the shipped
2.2 multiplier. The scenario templates supply the starter-tech list.
Detailed calculations are saved in
[`column-placement-evidence-2026-09-23.json`](column-placement-evidence-2026-09-23.json),
with the merged input in
[`merged-tech-snapshot-2026-09-23.json`](merged-tech-snapshot-2026-09-23.json).

These are offline predictions, not measurements from a running Unity screen.
The model uses a fresh technology-only node list, JSON template order, and
initial `lastNode = 0`. The real controller retains bookkeeping across tree
views, so the exact Future Tech column also needs an in-game check. Twenty
shuffled enumeration orders per scenario produced the same ordinary columns
and Future Tech physical column in these experiments. All modeled prerequisite
constraints passed, and all cost/prerequisite passes converged before their
iteration guards.

## 1. The selected tree view changes the algorithm

| View | Placement passes, in order |
|---|---|
| Full tree including faction projects | Seed roots; first prerequisite; repair all prerequisites; place Future Techs |
| Full tree showing global technologies only | Seed roots; first prerequisite; **cost redistribution**; repair all prerequisites; place Future Techs |
| Selected technology's filtered tree | Seed roots; first prerequisite, with alternative-prerequisite handling; repair prerequisites; place Future Techs |

The **120% rule runs in the technology-only full tree**. It is not a universal
rule applied to all three views. A cost adjustment that changes the column
in that view can leave the full tree with projects unchanged.

The tree is constructed from all registered technologies, not only completed
or currently researchable ones. Completion changes presentation and availability;
it does not itself make a technology a layout starter. The new 2026 completion
grant for Advanced Neural Networks therefore does not directly assign a column.

## 2. Initial placement: scenario starters and roots

`BuildTree` sorts a local copy of the item list by
`GetResearchCost(activePlayer)`. This is the effective cost used by the game,
including applicable modifiers. That locally sorted copy does not replace the
controller's list used by later passes.

For the 2022 and 2026 scenarios:

- `techTreeUIStarters` contains `WeAreNotAlone` and `MissionToSpace`.
- Those technologies are assigned column 1.
- Their unvisited direct dependents are initially assigned column 2.
- Other ordinary global technologies with no prerequisites start in column 2.
- Skywatch also has a special column-2 condition.
- Future Techs are counted but left for the end-game pass.

An ordinary global technology's cost does not prevent it from being seeded
as a root. The separate 100,000 effective-cost condition visible in this
method applies to the project branch of the code, not ordinary global roots.

Removing Photonic Computing from Advanced Neural Networks makes it a root
at this stage. It does **not** lock its final position to column 2.

## 3. The first-prerequisite pass

For an unvisited technology with prerequisites, the game initially assigns:

```text
node[technology] = node[first prerequisite] + 1
```

This pass uses only the first entry in the resolved prerequisite list, and
does not require that prerequisite to have finished its own placement.
Resolution preserves the JSON list's order and filters empty entries and
`PLACEHOLDER` entries. The code can repeat the traversal and has a guard after
21 traversals; on the modeled globals, the first traversal places all ordinary
non-roots.

This is not yet a topological layout. A technology can temporarily appear
before another prerequisite. The later repair pass handles that. Because
cost redistribution happens **between** these passes, temporary positions and
prerequisite ordering can influence the averages that drive cost movement.

## 4. Exact cost redistribution rule

For each column, the game calculates an arithmetic mean of the **authored
`researchCost` fields**, considering resident technologies costing at least 100:

```text
average[c] = int(float_sum(cost[t] for t in column c where cost[t] >= 100)
                 / number_of_qualifying_residents)

for each technology t:
    if node[t] > 0 and cost[t] > 1.2 * average[node[t]]:
        node[t] = min(node[t] + 1, 28)
```

Important details:

1. The inequality is **strictly greater**. Exactly 120% does not move.
2. The comparison is with the entire column's mean, including the technology
   itself, not with Proton-Proton or the cheapest technology individually.
3. The mean uses single-precision accumulation and is converted to an integer
   before multiplication by 1.2. For positive means this drops the fraction.
4. All column means are computed before that round's movements. A technology
   moves at most one column during each round.
5. Means are recomputed and the process repeats until no movements occur,
   or the guard stops it after the 21st round.
6. Column 1 is exempt from movement (`node == 0`). Technologies costing less
   than 100 are excluded from the means, although the movement loop itself
   has no separate 100-cost exclusion.
7. Normal movements use a 29-column limit, native indices 0 through 28.
8. There is no leftward movement in this pass. A cheaper price permits a node
   to stop earlier when the tree is rebuilt.

Empty columns cause a zero-count division in the native average calculation.
That is an implementation edge case, not an intended zero-cost tier. The
model's normal resident technologies never compare against an empty qualifying
column. None of the conclusions depends on choosing a platform-specific
integer conversion for NaN.

### Why there is no fixed price-to-column table

If a hypothetical column contains costs 100,000 and 150,000, its mean is
125,000. The threshold is 150,000, so neither moves. Thus even a technology
costing 150% of Proton-Proton would not necessarily leave their shared column
under the ordinary rule.

A lone technology's cost equals its column mean, so it does not move merely
because its absolute price is enormous. Changing one price can also change
whether *other* residents cross the threshold. This makes pricing effects
non-local and sometimes non-monotonic.

### Why the x2.2 multiplier does not select later cost columns

`BuildTree` sorts using effective costs, but this redistribution pass reads
the raw `researchCost` fields directly. The mod multiplies the return value
of `GetResearchCost`; it does not rewrite those fields. Consequently, the
displayed 88,000 cost for a 40,000 technology is still compared as 40,000
in this pass. A uniform multiplier also preserves global-tech price ordering.

## 5. Prerequisite repair overrides the price placement

After cost redistribution, `PlaceTechsBehindSameTierPrereqs` repeatedly applies:

```text
for each ordinary prerequisite p of technology t:
    if node[t] <= node[p]:
        node[t] = min(node[p] + 1, 28)
```

The process stops when stable or after the 41st traversal. For the ordinary
global technologies in this analysis, the result must lie after **every**
prerequisite. Reducing a price cannot remove that requirement.

Crucially, the cost pass is **not rerun** after prerequisite repair. Final
columns can therefore contain widely different prices, and their final means
are not the means used by the 120% rule. A final column's average should not
be treated as a price cap for that column.

The alternative-prerequisite branch also contains a duplicated `AltTechPrereq0`
comparison where one would expect `AltTechPrereq1`. None of the 149 global
templates in this input uses either alternate field, so that quirk does not
affect these fusion results. Projects and selected-tree views require their
own checks if their alternative prerequisites become part of a layout change.

## 6. Why Future Techs can share Proton-Proton's column

The seven installed Future Techs have `endGameTech = true`, no prerequisites,
and base costs of 100,000. During normal fresh initialization they are left
unvisited at native node 0. The cost pass cannot move node 0. Their prices
contribute only to the exempt first column's average, not to Proton-Proton's
column average.

Their actual placement occurs later in `HandleEndGameTechs`. Two related
bookkeeping issues are visible in the inspected code.

### The reserved final column can be occupied without advancing the reservation

Several ordinary placement paths update the tracker like this:

```csharp
if (tech.node > lastNode)
    lastNode = tech.node + 1;
```

Suppose an ordinary technology reaches native node 13. The tracker becomes
14, reserving column 15 for Future Techs. If another ordinary technology then
reaches native node 14, `14 > 14` is false. The reservation is not advanced,
even though its column is now occupied.

That sequence occurs in the baseline model: Proton-Proton first advances
the tracker to 14 and later reaches node 14 itself. Climate Change Mitigation
also ends in that same ordinary column.

### The Future Tech's physical column and recorded node disagree

The end-game method performs these two operations:

```csharp
tech.transform.SetParent(currentNodeContainer.transform.GetChild(lastNode).transform);
tech.node = lastNode + 1;
```

With `lastNode == 14`, the physical parent is column 15, but its recorded node
says column 16. This mismatch also matters to subsequent spacing logic, which
uses recorded nodes to identify same-column items.

The routine is separate from research pricing. Increasing costs does not repair
either bookkeeping issue.

### Future Tech price experiment

Only the seven Future Tech base prices were changed in each baseline trial:

| Price of each Future Tech | Proton-Proton physical column | Future Tech physical column |
|---:|---:|---:|
| 100,000 | 15 | 15 |
| 120,001 | 15 | 15 |
| 150,000 | 15 | 15 |
| 1,000,000 | 15 | 15 |

Every ordinary technology's modeled column was unchanged. This reproduces
the user's reported collision, but is not a live screen trace.

### Proposed durable correction, for review

In the end-game layout pass, derive the Future Tech destination from the
actual ordinary nodes in the current view:

```text
futureColumn = 1 + maximum column occupied by an ordinary displayed-tree item
for each Future Tech in this tree:
    parent it to futureColumn
    set its recorded node to the same futureColumn
```

For a tree including projects, ordinary items include those projects. Hidden
items already participating in layout need consistent treatment; the safest
initial scope is all ordinary items constructed for that view. Use the view's
own node container and handle its capacity explicitly rather than clamping
Future Techs onto an occupied final column. Current modeled trees are well
below the native limit.

This is a small UI-runtime correction, not a research-price change. It should
apply to each view that includes Future Techs, preserve `endGameTech` and
research effects, and give every Future Tech the same separate last column.
It remains a proposal, not an implemented patch.

## 7. Results of the specifically requested JSON edits

This experiment changes D-T and Methodologies to 40,000, and Advanced Neural
Networks to 2,500 with `prereqs: []`. Other costs and prerequisites stay current.
The 2026 completion grant does not change this placement calculation.

| Technology | Base price after edits | Current column | Predicted column |
|---|---:|---:|---:|
| Deuterium-Tritium Fusion | 40,000 | 9 | **8** |
| Nuclear Fusion Methodologies | 40,000 | 10 | **9** |
| Magnetic Plasma Confinement | 35,000 | 9 | **9** |
| Electrostatic Plasma Confinement | 15,000 | 11 | **10** |
| Inertial Plasma Confinement | 65,000 | 11 | **10** |
| Tokamaks | 25,000 | 11 | **10** |
| Z-Pinch Techniques | 50,000 | 11 | **10** |
| Deuterium-Deuterium Fusion | 75,000 | 12 | **11** |
| Deuterium-Helium-3 Fusion | 75,000 | 13 | **12** |
| Aneutronic Fusion | 75,000 | 14 | **13** |
| Proton-Proton Fusion | 100,000 | 15 | **14** |
| Terawatt Fusion Reactors | 100,000 | 14 | **13** |
| Clean Energy | 50,000 | 14 | **13** |
| Coilguns | 30,000 | 8 | **8** |
| High Temperature Superconductors | 40,000 | 8 | **8** |
| Advanced Neural Networks | 2,500 | 5 | **4** |
| Mission to Mars | 2,500 | 4 | **4** |
| Future Techs, physical parent | 100,000 each | 15 | **15** |

D-T shares Coilguns' column, although this model does not establish that it
will be immediately above it. Climate Change Mitigation also follows Clean
Energy one column left, from 15 to 14. No other ordinary technology outside
the table changes column in this experiment.

### A concrete example of the cost and prerequisite passes interacting

With the requested edits, both D-T and Methodologies finish the cost pass
in column 8. The last cost-pass mean there is 35,294, giving a threshold of
42,352.8. Their 40,000 prices fit below that threshold. Prerequisite repair
then moves Methodologies to column 9 because it requires D-T.

At the current 50,000 costs, a late cost-pass round sees a column-8 mean of
40,600, with a threshold of 48,720. Both entry technologies move into column
9. Prerequisite repair then moves Methodologies to column 10.

Advanced Neural Networks at 2,500 starts as a root in column 2, moves twice
under cost redistribution, and finishes in column 4, alongside Mission to
Mars. Its completion in 2026 is a separate scenario change.

### The remaining anchor conflicts

**Magnetic confinement:** High Temperature Superconductors stays in column 8
and is a direct prerequisite, so Magnetic Plasma Confinement must be in column
9 or later. Its proposed column-8 target cannot be obtained by lowering only
its price while keeping that prerequisite in column 8.

**Proton-Proton versus Clean Energy and Terawatt:** the shared branch is:

```text
D-He3 -> Aneutronic -> Proton-Proton
D-He3 -> Clean Energy
D-He3 -> Terawatt Fusion Reactors
```

Additional prerequisites still apply, but the first path alone takes an extra
column. After the entry discounts, D-He3 lands in column 12, Aneutronic in 13,
and Proton-Proton in 14. Clean Energy and Terawatt land in 13.

Thus "Proton-Proton one column earlier" is achieved, while "beside Clean
Energy and before Terawatt" is not. Those two anchor technologies move left
too. One-at-a-time price sweeps for Clean Energy and Terawatt, sampled at 1,000
intervals through 200,000 and 10,000 intervals through 1,000,000, did not produce
the desired relative order with the other requested edits held fixed. This is
evidence against a simple price fix, not proof that no coordinated set of many
price changes could ever do so.

## 8. A price proposal consistent with the earlier progression

There is no algorithmically mandated price for a final column. We can either
retain downstream prices, since the two entry discounts already move the main
chain, or additionally discount the later fusion program as a balance choice.

The following **optional** schedule was tested as a complete set. It preserves
every ordinary technology's predicted column from the requested-edits table:

| Technology | Current base cost | Optional proposed base cost | Displayed at x2.2 |
|---|---:|---:|---:|
| D-T Fusion | 50,000 | 40,000 | 88,000 |
| Nuclear Fusion Methodologies | 50,000 | 40,000 | 88,000 |
| Magnetic Plasma Confinement | 35,000 | 30,000 | 66,000 |
| Electrostatic Plasma Confinement | 15,000 | 15,000 | 33,000 |
| Inertial Plasma Confinement | 65,000 | 60,000 | 132,000 |
| Tokamaks | 25,000 | 25,000 | 55,000 |
| Z-Pinch Techniques | 50,000 | 40,000 | 88,000 |
| D-D Fusion | 75,000 | 65,000 | 143,000 |
| D-He3 Fusion | 75,000 | 65,000 | 143,000 |
| Aneutronic Fusion | 75,000 | 65,000 | 143,000 |
| Proton-Proton Fusion | 100,000 | 75,000 | 165,000 |
| Advanced Neural Networks | 5,000 | 2,500 | 5,500 |

The pricing rationale is to retain the two requested reference matches, keep
the already inexpensive confinement branches at their existing prices, discount
the more expensive confinement branches, preserve a shared price for the three
intermediate fuel cycles, and retain a premium for Proton-Proton. These are
balance choices, not fitted unique solutions to the layout formula. This set
does not solve the magnetic-confinement or Clean Energy/Terawatt anchor conflicts.

Clean Energy, Terawatt, and Future Tech prices remain unchanged in this optional
schedule. Their target relationship should be resolved explicitly before
choosing any further price changes. Raising Future Tech prices to correct the
visual overlap would impose a gameplay cost without accomplishing that goal.

## 9. Review recommendation and validation plan

1. Retain the explicit 40,000 / 40,000 / 2,500 JSON proposal, the Neural
   Networks prerequisite removal, and its 2026 completion grant.
2. Choose whether the optional additional fusion discounts are desired for
   balance. They are not required for the predicted one-column main-chain move.
3. Treat a separate, internally consistent Future Tech column as a UI invariant;
   use the scoped end-game placement correction for a durable guarantee.
4. Resolve whether every confinement method must also move and whether
   Proton-Proton's precise relationship to Clean Energy/Terawatt is mandatory.
   Those targets require an additional graph/layout decision; the report does
   not silently alter research dependencies to achieve them.
5. Once the implementation scope is approved, update automated checks and run
   the normal `tools/deploy.ps1` build/verification/deployment flow. Its game-open
   assertions remain mandatory.
6. In-game, inspect technology-only, project-inclusive, and selected-tech views;
   reopen them in different orders; verify separate Future Techs, readable lines,
   correct research prices, and the new 2026 completion state. Record those
   results before calling the exact UI placement verified.

Vertical alignment remains a separate calculation: the game averages connected
node positions and resolves overlaps after horizontal placement. Neither a price
match nor a shared column guarantees "directly above Coilguns."
