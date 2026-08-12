# Land Warfare and Military Investment

Version 0.8.0 replaces the fixed and stepwise land-military economy with one
continuous set of formulas shared by gameplay, merger integration, tests, and
tooltips. This document is the implementation authority for the patch.

## Army value, construction, and upkeep

At Military technology `t`, an army's construction value is:

```text
ArmyCost(t) = 2 × 2^t
```

Build Army therefore costs 8, 16, 32, and 64 IP at technology 2, 3, 4, and 5.
Fractional technology is not rounded. A naval-deployment army remains one army,
not an army plus a separate navy equivalent.

Upkeep uses the army's existing `useHomeInvestmentFactor` predicate. "Home"
means idle in that army's specific home region, outside combat and operations,
while the region is not fully occupied.

```text
Home upkeep = t / 10
Away upkeep = t / 3
```

Alien invaders and megafauna retain their own vanilla army types and do not
enter human construction, modernization, or repair-value counts.

## Interpreting national effort

Priority weights represent shares of redirectable national effort, not literal
shares of all GDP. The balance calibration treats a fully allocated priority
bar as roughly 50% of GDP: capital formation maps mainly to Economy, health and
social protection to Welfare, fossil extraction and rent capture partly to
Spoils, and defense expenditure to upkeep plus Military and Build Army. Thus an
observed activity equal to 3% of GDP is initially comparable to about 6% of the
priority bar. Army and navy upkeep must be credited before assigning additional
Military or Build Army weights, or military effort is counted twice.

The worked United States, China, and Russia projection is documented in
`docs/military-investment-2022-2030-sanity-check.md`. At budget-consistent 2022
allocations none reaches its army or tech cap by 2030; only a deliberately
excessive doubled US stress case reaches both late in the decade.

## Continuous Military investment

For current technology `a`, target technology `b`, national cap `M`, and `N`
eligible surviving human national armies:

```text
ArmyUpgradeCost(a,b,N) = N × (ArmyCost(b) - ArmyCost(a))

CatchUpCostMultiplier(t,M) = 1 / (1 + max(0,M-t))

BaseDoctrineIntervalCost(n) = 500 × 2^(n-1)

ContinuousDoctrineRate(t) = 500 × ln(2) × 2^(t-1)

DoctrineCost(a,b,M)
    = integral from a to b of
      ContinuousDoctrineRate(t) × CatchUpCostMultiplier(t,M) dt

MiltechCost(a,b,N,M) = DoctrineCost(a,b,M)
                       + ArmyUpgradeCost(a,b,N)
```

Without catch-up, integer intervals 1→2 through 5→6 cost exactly 500, 1,000,
2,000, 4,000, and 8,000 IP. The exponential rate extends that sequence smoothly
to fractional technology. Catch-up is applied point-by-point inside the same
integral rather than freezing a discount at either endpoint. At technology 4
with cap 5, the marginal catch-up multiplier begins at 0.5 and doctrine from
4 to 5 costs about 2,883.59 IP. Each army adds 32 IP to that interval.

Military's ordinary completion threshold remains 1 IP. At every completion the
runtime solves the monotonic cost equation for the corresponding fractional
technology gain using double precision and bounded bisection. It recalculates
after army-count or cap changes. If less than 1 IP remains to the cap, that
fractional amount becomes the final threshold, preventing an overcharge.
Direct investment is integer-valued in Terra Invicta, so its maximum is the
floor of exact remaining integrated cost; national IP can supply a final
sub-IP remainder.

## Repair debt

Normal daily human army healing runs first. The patch then charges the actual
strength recovered:

```text
RepairCharge = 0.5 × ArmyCost(current technology)
                    × (strength after - strength before)
```

Build Army accumulation may become negative. This is persistent national repair
debt, not a negative construction bar. Positive additive investment entering
Build Army, Military, Build Navy, or Nuclear Weapons repays that debt before it
can advance its selected priority. If one investment is larger than the remaining
debt, the debt is set to zero and the exact remainder advances the originally
selected priority normally. Existing army-destruction refunds continue to offset
debt through Build Army accumulation.

Multiplicative changes and nonpositive adjustments are not diverted. This keeps
priority damage, completion-cost subtraction, and other reductions attached to
their original priority. Direct Military investment counts repair debt plus the
remaining integrated Military-technology cost when calculating its integer
purchase limit, so repayment is not artificially capped by miltech progress.
Healing is never blocked by debt. Healing capped near full strength is charged
only for the amount actually restored.

While repair debt is negative, the nation-priorities panel replaces the separate
investment-value cells for each currently available debt-funding priority with
one centered display spanning their combined investment-column area. The source
set is Military, Build Army, Build Navy, and Nuclear Weapons. Hidden or invalid
priority rows do not reserve space in the span. The overlay is display-only: its
background blocks the internal row dividers in that column, it does not intercept
input, and the original per-priority values are restored when debt reaches zero,
the nation changes, or the panel closes.

On peaceful unification, the absorbing nation's debt remains and 100% of the
joining nation's negative debt transfers. Vanilla continues to govern positive
joining progress.

## Alien-flora assault damage

Army assaults against alien flora retain TI's existing success chance, outcome
table, technology mitigation, seven-day duration, and flora-removal formula.
Only the damage received by the army is scaled by the infestation present when
the assault resolves:

```text
FloraDamageScale = clamp(xenoforming level / 100, 0, 1)
ArmyDamage = VanillaFloraAssaultDamage × FloraDamageScale
```

An infestation at level 100 therefore remains fully dangerous. Levels 65, 30,
10, and 1 deal 65%, 30%, 10%, and 1% of the damage TI would otherwise roll for
the same outcome. Alien facilities and landed craft retain their vanilla army
assault damage. Surviving flora may still hide, regrow, spread, and be selected
again by an army's standing hunt order.

## Combat ratings and hit probability

Attack and defense now use an additive health penalty:

```text
StrengthPenalty = 1 - clamp(strength,0,1)
CombatRating = uninjured rating - StrengthPenalty
```

A half-strength army loses 0.5 rating and a zero-strength army loses 1 rating.
The following contributions are multiplied locally by 0.5 when calculating army
ratings: adviser Command, LEO army combat, own-region, rugged own-region,
core-economic own-region, crackdown, friendly-region cohesion, and realized
rugged/urban project modifiers. Friendly cohesion remains attacker-only.
Global adviser values and regional constants used by local defenses are not
changed.

For rating difference `d = Attack - Defense`:

```text
if d >= 0: P(hit) = 1 - 0.5 × 2^(-d)
if d <  0: P(hit) =     0.5 × 2^d
```

Differences -1, 0, and +1 produce 25%, 50%, and 75%. The curve is monotonic,
bounded, and symmetric: `P(-d) = 1 - P(d)`.

## Peaceful unification, conquest, and Alien transfers

The runtime uses explicit scoped contexts rather than changing every
`AbsorbNation` call. Peaceful `Unification` preserves and integrates surviving
human armies. A human conquest absorption destroys the conquered armies.
Unrelated direct territorial transfers keep their caller's vanilla behavior.
Transfers of human-army home regions into the Alien Nation preserve those human
armies and apply the same investment conservation as unification.

For lower original technology `L`, higher technology `H`, lower and higher
surviving cohort counts `NL` and `NH`, and the resulting cap `M`, the result
`T` is the unique value in `[L,H]` satisfying:

```text
DoctrineCost(T,H,M) + NH × (ArmyCost(H)-ArmyCost(T))
  = NL × (ArmyCost(T)-ArmyCost(L))
```

The left side is doctrine relinquished below `H` plus equipment value released
by downgrading the higher cohort. The right side is the lower cohort's required
equipment modernization. Cohort membership and technology are snapshotted
before ownership changes, while only armies that survive and reach the result
are counted. With no lower-tech surviving armies, the result is exactly `H`.
Alien regular armies and megafauna never participate; transferred human armies
do.

All solvers reject non-finite inputs, clamp successful outputs to their valid
bounds, and leave the vanilla result in place if a runtime calculation cannot
be completed safely.
