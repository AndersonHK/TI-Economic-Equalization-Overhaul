# Global Technology AI Selection

Status: implementation authority for the EEO global-technology chooser.

## Purpose

TI 1.0.51 groups available technologies by AI priority tier and discards every
group below the current maximum before it scores research value. The dense set
of critical, faction-preferred, and immediately enabling technologies can
therefore leave inexpensive early technologies at zero selection probability
for most or all of a campaign.

EEO replaces that hard gate for global technologies only. Every available
global technology receives a positive weighted-random selection chance.
Faction-project selection retains TI's native tier gate and detailed
project-role evaluation.

## Formula

For candidate technology `i`, define:

```text
relativeCost_i = medianAvailableCost / cost_i

costMultiplier_i =
    clamp(relativeCost_i ^ 0.75, 0.25, 4.00)

weight_i =
    categoryPreference_i
  * rolePreference_i
  * priorityMultiplier_i
  * costMultiplier_i
  * contextMultiplier_i
```

The median and every candidate cost use `TITechTemplate.GetResearchCost`, so
scenario speed, repeatable endgame scaling, and EEO's global research-cost
multiplier are observed consistently. A uniform cost multiplier cancels from
the relative-cost ratio.

Priority tiers retain their existing TI classification but become soft
multipliers:

| Effective tier | Multiplier |
|---:|---:|
| 0 or lower | 1 |
| 1 | 2 |
| 2-4 | 4 |
| 5-6 | 10 |
| 7 or higher | 14 |

When a faction has no available objective project and a technology leads to an
objective project through TI's native short lookahead, its effective tier has a
floor of 5. This replaces TI's separate `x100` objective-path score bonus.

Category and role preferences continue to come from the faction's live AI
profile. TI's contextual `x0.05` suppression for Space War technology while the
faction is not building ships is retained. Control Point-capacity effects also
retain TI's native modest multiplier, including its additional Dominate-mission
valuation.

## Removed duplicate priority effects

The soft priority multiplier is the sole global-technology representation of
critical, forced, and immediately enabling status. The chooser therefore does
not retain TI's post-tier:

- `x50` critical-technology multiplier;
- `x50` `cheapestForcedTechName` multiplier;
- `1 + 5N` multiplier for important direct children; or
- `x100` objective-path multiplier.

Leaving those multipliers in place would reproduce an effective hard gate even
after all tiers became eligible.

## Selection and safeguards

Normal AI selection remains weighted-random. Deterministic callers receive the
maximum-weight candidate. Candidate count intentionally contributes aggregate
probability: a large backlog of low-priority technologies should clean itself
up faster than a small backlog.

The chooser returns to vanilla when the feature is disabled or when candidate
costs or calculated weights are absent, non-finite, or otherwise invalid. It
adds no serialized state and does not change technology availability,
prerequisites, research allocation, completion leadership, or faction-project
selection.

## Calibration expectations

Before the cost clamp and with equal faction/context value:

- a technology costing four times as much has `1 / 4^0.75`, or about 35.4%, of
  the cheaper technology's weight at the same tier;
- a tier-5 technology costing 20 times as much as a tier-0 technology has about
  1.06 times its weight (`10 / 20^0.75`); and
- a tier-7 technology costing 50 times as much as a tier-0 technology has about
  0.74 times its weight (`14 / 50^0.75`).

These ratios let cheap backlog technologies compete without making expensive
strategic technologies ineligible.
