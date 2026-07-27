# Economic and Patch Design Directives

These rules are the balance authority for future work. Compatibility scaffolding
may change between TI releases; the units and incentives below should not.

## One IP buys one understandable quantity

Monthly Investment Points scale almost linearly with GDP. A completed IP should
therefore buy a legible quantity, and any divisor must describe the stock being
changed:

- Fixed assets or physical work use a fixed effect per IP. Mission Control,
  Boost, Armies, direct atmospheric removal, and fallout cleanup are examples.
- Economic proportions divide by GDP. Sustainability/carbon intensity,
  Inequality, and similar national economic ratios must not change faster merely
  because a large economy produces more IP.
- Demographic proportions divide by population. Education, Cohesion,
  Government legitimacy, Unrest, and public-opinion effects should require more
  total IP when more people or institutions are being moved.
- Force-wide effects divide by the affected force. Military technology divides
  by army count because one completion upgrades every army.

At equal priority allocation, `GDP-scaled IP × GDP-inverse effect` produces a
roughly scale-neutral change to economic ratios. This is the core protection
against unification and balkanization exploits.

## Scale and unification

Unification must not accumulate free per-capita bonuses while also pooling GDP.
Splitting a country must not multiply national-ratio effects. Conversely, larger
countries are intentionally advantaged at fixed-cost projects: they can devote
more absolute capital to Mission Control, Boost, and Armies.

Country mergers must also combine the stocks that national ratings summarize.
Military technology is half the equipment/doctrine embodied in existing armies
and navies and half the GDP-backed industrial base that can sustain them.
Inequality represents the merged income distribution, not a simple average of
two ratings: population sets the size of each distribution, GDP per capita its
center, and existing Inequality its approximate width. Similar distributions
should overlap with little change; large income gaps should form a disruptive
second mode. The finite-population Gini correction is retained, so one rich and
one poor individual approaches maximum Inequality while two equally sized mass
populations at the same income extremes approach the midpoint.

Sequential three-country mergers are not perfectly order-independent because TI
persists only one Military technology value and one Inequality value, not the
component force and income distributions. Keep that limitation explicit and do
not introduce hidden historical state merely to make the operation associative.

Economy, Welfare, and Spoils use a twofold per-completion increase inside their
existing GDP-normalized Inequality formulas. Climate-driven Inequality is also
doubled at its uniquely identified mutation reason. Events, secession, and
revolution remain vanilla; annexation uses the country-merger distribution
formula described above.

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

Do not implement GDP per capita as an isolated exponential wealth penalty.
High GDP per capita locates a nation on the capital-to-complements curve; its
effect must depend on effective labor, resources, and the current technology-
enabled degree of substitution.

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
over poorer economies that can leapfrog legacy infrastructure. Direct atmospheric
removal is fixed physical work per IP.

Nuclear damage is proportional to detonations per land area, so a strike is more
concentrated in Singapore than Kazakhstan. Cleanup cost remains fixed per
detonation; land area changes the damage, not the price of removing one blast's
fallout.

## Formula and patch style

- Prefer one smooth, continuous high-school-level expression over step tables.
- Put the complete economic logic in the patch, normally within 20-50 readable
  lines; do not hide it behind a generic math utility.
- Comment units, direction, representative inputs, approximate outputs, and the
  relevant vanilla contrast.
- Prefixes return to vanilla when disabled or invalid.
- Transpilers replace the smallest possible field/constant load, count expected
  replacements, and fail loudly if TI's IL changes.
- Tooltip math must use the same getters or reproduce the same visible inputs as
  gameplay. Preserve vanilla tooltip text and append the mod section.
