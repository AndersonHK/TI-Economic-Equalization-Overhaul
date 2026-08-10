# Government boundary curve

## Purpose

Government is a bounded institutional score. The same raw effort should have a
larger democratizing effect in a highly authoritarian state and a smaller effect
near full democracy. Negative changes should behave reciprocally: institutional
damage is resisted near zero and amplified near ten.

The previous Government priority added `166,667 / population` per completion,
while several negative changes retained TI's shallower population scaling. It
also applied every Government delta at full strength regardless of proximity to
the score boundaries. These independent formulas allowed passive low-Cohesion
losses and Oppression to overwhelm sustained Government investment.

## Formula

Let `g` be Government clamped to `[0, 10]`, `d` the raw signed change, and `F`
the configured boundary factor (default `3`):

```text
multiplier = F ^ (sign(d) * (1 - g / 5))
bounded change = d * multiplier
```

This continuous exponential curve is reciprocal by direction:

| Government | Positive change | Negative change |
|---:|---:|---:|
| 0 | x3 | x1/3 |
| 2.5 | x1.732 | x0.577 |
| 5 | x1 | x1 |
| 7.5 | x0.577 | x1.732 |
| 10 | x1/3 | x3 |

The Government priority's raw demographic change is doubled to
`333,333 / population`. Passive monthly low-Cohesion Government loss is halved
before applying the boundary curve. The curve then applies once to every
Government change, including Government, Oppression, Spoils, war, neighboring
democracies, Cohesion overflow, coups, revolutions, regime changes, secessions,
and events.

Priority-property patches apply the curve before returning their values so the
UI and direct-investment pricing remain truthful. The central `AddToDemocracy`
patch handles changes that do not originate in those properties and skips the
three already-transformed priority reasons.

## Unrest interaction

This change does not replace TI's Unrest equilibrium model. Its natural rest
state remains:

```text
clamp(10.5 - Cohesion - PCGDP / campaignDivisor
      + army suppression + xenoforming + hostile claims, 0, 10)
```

Unrest normally closes at most `0.25` points of the gap each month. When
Cohesion is exactly zero and Unrest is below its rest state, it closes as much
as `1.0` point per month. Government affects the rest state only indirectly:
lower Government makes armies suppress more Unrest but also makes hostile
claims contribute more Unrest. Without armies or hostile claims, changing
Government does not move the natural Unrest equilibrium.

## Verification

Formula tests cover both directions at Government `0`, `2.5`, `5`, `7.5`, and
`10`; monotonicity; the doubled Government base; the halved passive
low-Cohesion input; and prevention of double application for priority changes.
