# Economic and Patch Design Directives

These rules are the balance authority for future work. Compatibility scaffolding
may change between TI releases; the units and incentives below should not.

## Runtime performance

User direction, 2026-11-03: favor one-time loading preparation and retained RAM
over recurring CPU work. Added main-thread milliseconds deserve severe scrutiny,
even when a patch's work is correctly classified as simulation rather than
rendering. This applies to all patches, including frequently called getters and
UI hooks, not only explicit Update/LateUpdate methods.

Prepare immutable data, lookup tables and reusable buffers before use. Update
changing state at its actual lifecycle/simulation boundary; rendering an unchanged
state must not repeat simulation work. Batch necessary work and use workers where
safe. Count main-thread capture, copies, scheduling, result handling and worker
waits as part of the cost. Moving a stall to a different callback or timer does
not remove it. Preserve intended behavior; a different and substantially simpler
implementation is preferable when the current architecture imposes recurring cost.

For battle collision, the target is zero or near-zero added main-thread ballistic
and pose overhead, with no repeated full ship/bone scans or duplicate physical
collider hierarchy maintained solely for an independent collision calculation.
Actual hit application still needs a small, measured native integration step.
The [revised battle FPS plan](ship-balance-research/battle-fps-investigation-plan-2026-11-03.md)
sets proposed budgets, behavior constraints and validation gates. The
[implementation record](ship-balance-research/battle-worker-contact-2026-11-04.md)
distinguishes headless evidence from full-battle acceptance. Do not trade away correct collision, introduce stale poses or
delay damage merely to meet a timer target.

## Armor, weapon energy and officer skill

User direction, 2026-11-04: officers should improve critical-hit/weak-spot
selection and/or aim precision, not multiply weapon damage or impact energy.
An identical projectile arriving with identical mass, velocity and contact
conditions must not gain joules because its firing officer is more experienced.
Retain meaningful officer progression through hit quality; the exact replacement
mechanic is part of the pending [armor rework plan](ship-balance-research/armor-rework-plan.md).

The rework should produce a **small general armor buff**. Powerful weapons should
still penetrate under-armored targets, while maximally armored late-game capitals
should not be easily destroyed by standard hull-mounted weapons. The user's
battleship/cruiser analogy describes the intended relationship: cruiser gunfire
should be unlikely to penetrate an intact battleship belt on its first hit;
torpedoes and dedicated heavy penetrators remain credible threats. This is a
gameplay analogy, not a literal claim about torpedo impact location or naval
armor physics. Routine one-hit kills should not dominate the late-game balance.

Further user clarification: include **even the heaviest hull batteries** in the
initial protection goal; do not reclassify them as automatic exceptions. Preserve
separate hull-battery and nose-weapon power progressions, with room for lower-tier
nose weapons to exceed higher-tier batteries. Test the user's 15–20-point
mid-game nose-armor scale as well as heavily armored end-game Titans.

Armor area must follow the selected ship appearance's actual armorable exterior
and the surface assigned to each armor node. Larger variants require more area
and material at the same thickness. Logical 2D armor-grid extent scales with that
area; numerical resolution must not secretly change physical wound size. Hits
affect a finite area according to projectile, beam or warhead behavior. The plan
must decide actual-contact versus abstract hit placement and component targeting
explicitly. Use delivered energy as the damage source, without random or
officer-created joules. If the conservative 80% baseline is chosen, represent
it through the single armor-work divisor described below.

Armor strengthening must use **one global divisor in
armor loss/work**, applied uniformly to every impact and material. Do not add
weapon-specific work normalization, battery-spread tuning, a balancing laser
area floor or an extra internal-damage coefficient. Express an 80% conservative
baseline through that same divisor if chosen; do not also multiply energy by 0.8.
The final divisor value is not selected. The prior multi-parameter proposal is
rejected, not an implementation target.

Sparse adaptive surface tiles are desirable if they can be implemented cheaply
through the existing engine. At about **ten impacts per frame, another 1 ms of
frame time for armor is unacceptable**. Measure the whole added patch path:
contact/state access, adapter, local numeric processing and notifications. Favor
hot contiguous arrays, cached handles and minimal reads/writes; array size alone
does not establish expense. No per-hit allocation/reflection/component searches,
duplicate geometry queries or recurring full-grid scans. Feasibility must be
demonstrated on the actual game runtime, not inferred from arithmetic alone.

Assess first-hit penetration, cumulative degradation, weak-spot damage and
internal lethality separately. The desired local-depth/area-damage model and
these durability targets are linked requirements; changing one does not prove
the others are satisfied. The [current late-game calculations](ship-balance-research/armor-system-as-is-2026-11-04.md#12-late-game-coiler-lethality-400-armor-titans-and-200-armor-battlecruisers)
provide the baseline, not the desired balance.

## Fresh campaigns and save migration

Fresh campaigns started on the current mod version are the primary design and
acceptance path. Migration is secondary, but old saves must be converted to the
current rules with best-effort preservation of existing data; leaving them on
the old rules is not the compatibility strategy. Preserve equipment, fuel and
ship state when conversion exceeds current capacity. Apply current limits when
the player edits or refits the design. Migration must not grant research.

Substantial changes with one-way save conversions warrant a minor-version
advance: the compact capital heavy-mount change is release **0.10.0**.

## One IP buys one understandable quantity

Monthly Investment Points scale almost linearly with GDP. A completed IP should
therefore buy a legible quantity, and any divisor must describe the stock being
changed:

- Fixed assets or physical work use a fixed effect per IP. Mission Control,
  Boost, Armies, and fallout cleanup are examples.
- Economic proportions divide by GDP. Sustainability/carbon intensity,
  Inequality, and similar national economic ratios must not change faster merely
  because a large economy produces more IP.
- Demographic proportions divide by population. Education, Cohesion,
  Government legitimacy, Unrest, and public-opinion effects should require more
  total IP when more people or institutions are being moved.
- Force-wide effects price the affected force explicitly. Military investment
  contains a doctrine component plus the exact change in construction value for
  every eligible army, evaluated continuously at fractional technology. Army
  construction value follows `2 × 2^tech`, not a polynomial in technology.
  Undiscounted doctrine intervals double each level from a 500-IP technology-1
  base; the exponential extension and smooth catch-up discount are integrated
  continuously rather than evaluated as integer bands.

At equal priority allocation, `GDP-scaled IP × GDP-inverse effect` produces a
roughly scale-neutral change to economic ratios. This is the core protection
against unification and balkanization exploits.

Priority weights are shares of redirectable national effort, not a statement
that governments can continuously command all GDP. For calibration, a fully
allocated priority bar represents roughly 50% of GDP. An observed activity's
GDP share is therefore divided by 50% to estimate its priority share: US gross
capital formation near 20% of GDP suggests an Economy weight near 40%, while
military expenditure maps to army/navy upkeep plus Military and Build Army.
Automatic upkeep must be credited before assigning discretionary military
weights so the same effort is not counted twice. Welfare and Spoils use the
same approximate national-accounts interpretation for health/social spending
and extraction/rent capture, without pretending the categories are exhaustive
or non-overlapping.

## Scale and unification

Unification must not accumulate free per-capita bonuses while also pooling GDP.
Splitting a country must not multiply national-ratio effects. Conversely, larger
countries are intentionally advantaged at fixed-cost projects: they can devote
more absolute capital to Mission Control, Boost, and Armies.

Country mergers must also combine the stocks that national ratings summarize.
Peaceful Military integration conserves doctrine and army equipment value:
lower-tech armies consume modernization value while any high-tech armies that
fall below their original level release equipment value. GDP is not a proxy for
this stock. Navies do not add a second army equivalent because a naval-deployment
army already appears once in the national army collection.
Inequality represents the merged income distribution, not a simple average of
two ratings: population sets the size of each distribution, GDP per capita its
center, and existing Inequality its approximate width. Similar distributions
should overlap with little change; large income gaps should form a disruptive
second mode. The finite-population Gini correction is retained, so one rich and
one poor individual approaches maximum Inequality while two equally sized mass
populations at the same income extremes approach the midpoint.

Sequential three-country mergers are not perfectly order-independent because TI
persists only one Military technology value and one Inequality value, not the
component force and income distributions. Each merger snapshots only the two
live pre-transfer cohorts. Keep that limitation explicit and do not introduce
hidden historical state merely to make the operation associative.

## Space construction is a logistics network

The implementation authority and worked examples are in
[Manufacturing Logistics](manufacturing-logistics.md). This section states the
durable design rule.

Hab construction pays resources for the complete physical mass. The Earth
option purchases and launches the whole payload; the space option uses the
faction stockpile, purchases shortages on Earth, and physically dispatches at
least one-third of hab-module mass. Earth shipments count toward that minimum.
Non-Earth freight consumes Water and Volatiles as rocket-equation propellant,
including the launch and landing costs of surface origins and destinations.

Factories manufacture locally through their own tier. Remote exports require a
same-hab dock and are capped by the lower factory/dock tier; ownership must be
explicit and planetary-system boundaries never constrain routing. Earth is the
fallback root only when no valid space origin exists. Probes are full-payload
Tier 1 manufacturing jobs. Route and quote caches invalidate on time, resource,
or hab changes and refresh lazily so repeated UI and AI queries do not rescan
every origin. Invalidation marks cached work stale; it must not perform the
scan itself or create a daily/background rebuild. AI hab planning prioritizes
one same-hab factory-dock pair per colonized major system, with the strongest
priority in the Earth-Moon system.

Economy's reference-GDP Inequality change is three times its original EEO
coefficient; Welfare and Spoils are twice their original EEO coefficients.
Climate-driven Inequality is multiplied by four at its uniquely identified
mutation reason. Priority changes moving inward from an Inequality boundary
reach a directional `x3` multiplier, while outward changes fall to zero at the
boundary. Events, secession, and revolution remain vanilla; annexation uses the
country-merger distribution formula described above.

The Cohesion rest state treats Inequality 3 (the Gini 0.30 low/medium threshold)
as neutral: `min(1, 0.5 + Education/20) * (6.75 - 2.25*Inequality)`. Lower
Inequality strengthens Cohesion and higher Inequality weakens it. The
elite/public ideological-distance penalty is multiplied by Government divided
by 10, clamped to `[0,1]`, so it has full force at Government 10 and one-tenth
force at Government 1.

The Cohesion rest-state base is `10.5`. Below Government `4.0`, Autocracy's
regime contribution is
`(4^1.285 - Government^1.285) * (10-Unrest)/10`. Anocracy begins at Government
`4.0`, matching the government-type labels shown in the UI, and retains its
the `3*abs(5-Government)-2` contribution through Government `6.0`.
Democracy begins at the same shared `6.0` boundary and pulls the
accumulated rest value toward Cohesion 5 by
`cohesionRest.democracyCoefficient * (Government - 6.0)`, with a default
coefficient of `1.0` (vanilla is `0.5`). The result remains clamped at 5 so the
stronger pull cannot overshoot its target. The tooltip breakdown must use the
same configured base and patched component getters as gameplay.

## Economy growth is factor balance, not a wealth penalty

National production is constrained by three complementary factors: capital,
labor, and resources. If a balanced economy has one unit of each and produces
one unit of output, adding capital alone must produce less than twice the output.
Doubling capital, effective labor, and effective resources together should
produce approximately twice the output. This is constant returns to national
scale with diminishing returns to an input that outruns its complements.

An Economy completion adds capital. TI does not expose a separate capital stock,
so GDP per capita is the practical proxy for accumulated capital relative to
labor. Education, Government, Cohesion, and Core Economic regions describe how
effectively labor and institutions can support that capital. Resource regions
relative to GDP and land per person describe the complementary physical inputs.
The growth formula must compare these factors through scale-independent ratios;
population or borders must not be added again as free output multipliers.

The marginal return to capital follows a smooth logistical constraint:

- capital-poor economies with adequate labor and resources receive high returns;
- balanced economies remain on the productive middle of the curve;
- capital-heavy economies approach a soft cap when labor or resources cannot
  productively absorb more investment;
- scaling all three factors together leaves the per-IP return approximately
  unchanged, while the larger economy produces proportionally more IP.

Technology changes both the height and shape of this return curve. It raises
productivity for every factor combination and loosens the logistical constraint,
preserving more marginal return when capital greatly exceeds labor or resources.
At sufficiently advanced technology, the high-capital tail becomes nearly
linear: automation substitutes for labor, and advanced energy, materials,
agriculture, mining, and space industry substitute for scarce local resources.
Advanced economies benefit disproportionately because they were constrained
most strongly by the former soft cap.

Every global technology participates, but with semantic weights. AI and
automation primarily relax the labor constraint; energy, materials, mining, and
space-industry technologies primarily relax land and resource constraints;
biotechnology and agriculture primarily improve biological and land
productivity; general-purpose technologies affect both. Narrow military or
spacecraft technologies receive small spillovers rather than no effect. All also
contribute some productivity lift. The 2022 curve is not a zero-technology
curve: its baseline already embodies accumulated historical technology, while
global technologies completed during play continue moving it toward
post-scarcity behavior.

AI selection of the next global technology is also soft rather than exclusive.
TI's critical, forced, enabling, and expansion tiers become bounded priority
multipliers, while a bounded relative-cost term lets inexpensive backlog
technologies compete. Candidate count intentionally adds aggregate probability,
so a large backlog corrects faster than a small one. The authoritative formula
and its global-tech-only scope are in
[Global Technology AI Selection](global-technology-ai-selection.md).

Do not implement GDP per capita as an isolated exponential wealth penalty.
High GDP per capita locates a nation on the capital-to-complements curve; its
effect must depend on effective labor, resources, and the current technology-
enabled degree of substitution.

The softly canonical 0.7.0 calibration uses a $1B national base gain, $37,500
labor and $55,000 resource knees, starting return floors 0.35 and 0.45,
pressure exponents 1.4 and 1.2, and technology relief
`p * (0.10 + 0.90p)`. The 2022 starting technologies compound to 1.0201x; all
149 TI 1.0.51 global technologies compound to 3.40x and complete both
substitution axes. Change these together through the simulator rather than
tuning one national example in isolation.

## Resources, land, and historical structure

Resource importance is measured relative to GDP, not by a hardcoded region table.
The same oil output is transformative for a small economy and marginal for a
large diversified one. Resource benefits and emissions remain physically relevant
at high technology, but their economic share declines smoothly as GDP grows.

Land abundance is land per person, gated by stability. Its agricultural and
forestry significance falls with wealth, but cheap land retains a smaller housing
and industrial benefit. Neither land nor resources receive an artificial
technology fade. Technologies may relax the production constraint created by
scarcity; they do not subtract an abundance bonus from countries that possess
useful land or resources.

## Environment and nuclear damage

Economy emissions equal GDP times carbon intensity; population has no independent
term. Economy growth therefore raises total emissions without mechanically
changing emissions per unit of GDP. Spoils adds the same fixed total GDP as an
Economy completion while also worsening inequality, institutions, and carbon
intensity; it does not inject a second direct atmospheric gas pulse.

Sustainability transition divides by GDP: replacing ten times as much dirty
capital takes roughly ten times the IP. Rich grids receive no automatic advantage
over poorer economies that can leapfrog legacy infrastructure. Environment IP
reduces national carbon intensity only; once Sustainability reaches zero, further
completions do not remove gases directly from the shared atmosphere.

Nuclear damage is proportional to detonations per land area, so a strike is more
concentrated in Singapore than Kazakhstan. Cleanup cost remains fixed per
detonation; land area changes the damage, not the price of removing one blast's
fallout.

The current climate calibration multiplies only negative GDP damage above TI's
0.25 C warm threshold by 0.90. Cold benefits, neutral outcomes, regional
exposure, and climate-driven Inequality retain their existing behavior.

## Formula and patch style

- Prefer one smooth, continuous high-school-level expression over step tables.
- Put compact economic formulas in the patch, normally within 20-50 readable
  lines. Shared stateful systems such as logistics routing and caching belong in
  a named service with fixed inputs and explicit invalidation.
- Comment units, direction, representative inputs, approximate outputs, and the
  relevant vanilla contrast.
- Prefixes return to vanilla when disabled or invalid.
- Transpilers replace the smallest possible field/constant load, count expected
  replacements, and fail loudly if TI's IL changes.
- Tooltip math must use the same getters or reproduce the same visible inputs as
  gameplay. Preserve useful vanilla text and append only the concise rules and
  live values a player needs. Fixed-height cost buttons retain their native
  compact cost and duration labels; do not append multiline diagnostics. Do not
  add mod labels, migration history, or comparisons with retired behavior to
  gameplay text.

## National harmonization and claim legitimacy

Claim legitimacy is based on the directional compatibility of an annexing or
claimant nation `S` with the annexed or region-owning nation `T`; Government
alone is not a proxy for liberation. The authoritative score is:

```text
f = abs(S.Government - T.Government)
  + abs(S.Inequality - T.Inequality)
  + abs(S.Knowledge - T.Knowledge)
  + max(S.GDPPerCapita / T.GDPPerCapita,
        T.GDPPerCapita / S.GDPPerCapita)

modifier = (10 - T.Unrest) / 10 + (10 - S.Cohesion) / 10
harmonizationScore = modifier * f
```

GDP means current per-capita GDP (`TINationState.perCapitaGDP`, or GDP/c), not
total national GDP. Unrest and Cohesion are defensively clamped to `[0,10]`;
non-finite inputs or non-positive GDP/c fail closed. A real ordinary
claim is non-hostile at an inclusive score of `6` or less. A claim with an
immutable historical-hostility designation is non-hostile only at `3` or less,
so history can be overcome but requires substantially deeper convergence.
Regions with no claim remain hostile, and alien claim behavior remains TI's.

Federation formation and entry retain TI's alliance, cooldown, executive,
enemy, breakaway, and cross-boundary-claim requirements. In addition, the best
actual directional claim link connecting the prospective parties must score
`12` or less. Historical status does not lower this federation ceiling, and
scenario-defined startup federation assembly bypasses it.

Historical designation and an integrated hostile-region burden are separate.
The designation selects the `3` threshold whenever ownership is external; the
runtime burden may still be removed after a peaceful transfer without erasing
the historical standard for a future ownership change. Scenario history is
explicit: Russia's claims on Donetsk, Kharkiv, Kiev, and Odesa are historical
in the 2022/2026/2070 families but ordinary in the Dark Skies 2003 start.

## Asymmetric naval progression and weapon grades

Native save-version metadata belongs to upstream game developers. Never advance
that version to identify mod changes. Store mod migration/layout versions in
separate namespaced metadata such as `eeoCapitalLayout`; keep unrelated native
save compatibility fields unchanged. Appending index-preserving utility slots
in 0.11.1 does not require advancing the existing mod-owned schema 3.

The 0.11.0 species-specific naval tier catalog is authoritative for compact
fitting/weapon grade and UI labels. Native construction tiers remain separate
game AI metadata. Human Lancer/Dreadnought and Alien Dreadnought/Titan/Carrier
are T4; Human Titan and Alien Mothership are T5. Alien Cruiser is T2 and Alien
Lancer T3. Only T5 accepts N5, and its occupied noses require N5. All T4+ hull
cells use one heavy weapon each; Carrier retains no nose bank. N5 uses native
FourNose geometry but counts five capacity units. Its five approved templates
have exactly doubled sustained output through halved firing intervals.

Fractional graser wavelengths belong in the mod integer-angstrom precision
catalog, converted at physics/UI edges without reinterpreting native/save nm
fields. New fitting grades require save-version migration that distinguishes
previously compact cells from legacy shared footprints and preserves built
state. See [implementation and migration policy](ship-balance-research/asymmetric-tiers-implementation-2026-11-04.md).

## Fixed designer system anchors

Armor, drive, reactor, radiator and fuel controls are fixed anchors. Repack
weapon and utility cells around them; do not move fixed controls to gain room.
Mothership side armor is (4,8), tail/nose armor (0,8)/(9,8), drive/reactor/radiator
(0,4)/(1,4)/(2,4), and propellant (0,0). Their row follows an explicitly
authorized window/grid expansion, rather than an opportunistic hull repack.

Canonical designer armor controls use tail column 0, lateral anchor column 4
with the native half-cell horizontal offset, and nose column 9. Do not add
a separate protected-area graphic: infer blocked cells from actual centered
icons and count-control bounds. JSON rules still reject invalid aft members.

## Designer layout ownership and visible-window validation

When JSON owns cell positions, disable the native centered GridLayoutGroup;
allocated destinations are not the same as visible content rows. Validate
post-layout visible rectangles below headers/toolbars, including armor inputs
and Max labels, and preserve the upper notification reservation. Requested
panel dimensions and a standalone slot diagram do not prove screen fit. Use
bounded runtime containment checks/logs alongside pure geometry/API tests.
