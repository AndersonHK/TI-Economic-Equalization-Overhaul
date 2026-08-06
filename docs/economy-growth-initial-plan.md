# Initial Economy Growth Plan

Status: historical and superseded. The implemented model and accepted constants
are documented in [Economy Growth Calibration](economy-growth-calibration-plan.md)
and [Design Directives](design-directives.md).

This file preserves the exploratory sequence that led to the factor-balance
model. Future work should not treat its tentative ranges or future-tense tasks
as current implementation requirements.

## Proposed model at the time

### 1. Represent the two complementary constraints

Economy investment adds capital. Its return will be constrained separately by effective labor and effective resources:

```text
laborPressure =
    capital intensity / effective labor capacity

resourcePressure =
    capital intensity / effective resource capacity
```

Where:

- Capital intensity starts with GDP/c.
- Effective labor uses normalized Education, Government, Cohesion, and Core Economic regions.
- Effective resources use the existing continuous resources/GDP and land/population curves, gated by stability.
- Population does not appear independently because GDP/c already expresses capital relative to labor.

The resource and land curves stop being unrelated additive bonuses. They instead determine how much capital the country can productively absorb.

### 2. Use two smooth logistical constraints

```text
laborConstraint =
    laborFloor
    + (1 - laborFloor) / (1 + laborPressure ^ laborSlope)

resourceConstraint =
    resourceFloor
    + (1 - resourceFloor) / (1 + resourcePressure ^ resourceSlope)
```

Then:

```text
GDP gain per IP =
    calibrated base gain
    × technology productivity
    × laborConstraint
    × resourceConstraint
```

This produces the intended behavior:

- Adding capital alone raises output by less than the capital increase.
- Doubling capital, labor, and resources preserves the same per-IP return; doubled IP then produces doubled growth.
- The least-abundant factor becomes the effective bottleneck.
- No step tables or arbitrary wealth penalty are needed.

### 3. Give technology three semantic weights

Extend `economy-tech-weights.csv` to:

```text
tech_id
enabled
productivity_percent
labor_substitution
resource_substitution
rationale
```

Every technology receives some weight in all three columns, but the distribution differs:

- AI, computing, robotics, and automation: primarily labor substitution.
- Energy, materials, mining, fusion, and space industry: primarily resource substitution.
- Agriculture and biotechnology: primarily resource/land, with meaningful labor productivity.
- General economic and social technologies: approximately balanced.
- Small or narrowly military technologies: small spillovers everywhere.

The existing semantic magnitude remains: a transformative 4-point technology changes the curve more than a minor 0.5-point technology.

### 4. Model accumulated historical technology correctly

The 2022 scenario begins with a modern baseline, not the technological origin:

- Modern labor/resource floors represent engines, electrification, industrial agriculture, computers, global trade, and everything already developed.
- Future global technologies continue raising productivity and relaxing the constraints.

For each axis:

```text
axisProgress =
    completed axis weight / total available axis weight

axisFloor =
    modernFloor + (1 - modernFloor) × axisProgress
```

As labor substitution approaches 100%, `laborFloor` approaches `1`, making the labor constraint flat. Resource substitution behaves the same way. Only when both approach one does capital become effectively independent of both constraints.

Technology productivity remains the compounded lift applied to the entire curve.

### 5. Calibrate the modern soft cap

Initial calibration ranges:

- Strong growth should remain readily available through roughly $15k–$25k GDP/c.
- The modern logistical knee should begin around $30k–$50k effective GDP/c, depending on labor quality and resources.
- Developed economies should face noticeable—not catastrophic—diminishing returns.
- Resource-rich or highly educated countries should reach the knee later.
- Unstable countries should be unable to exploit their nominal factor abundance fully.

Target Economy-only growth at approximately 20–25% priority allocation:

| Country type | Initial annual target |
|---|---:|
| Stable developed | 2–3% |
| Stable middle-income | 3–5% |
| Stable developing/catch-up | 4–7% |
| Unstable or institutionally weak | Below comparable stable countries |

Spoils allocation adds the same GDP growth and must be included when evaluating actual national outcomes.

At maximum economic technology:

- Holding other factors equal, GDP/c should change marginal capital return by less than roughly 5%.
- Sustained 20% Economy allocation may produce approximately 10–20% annual growth.
- Earlier technology levels must not reach post-scarcity growth accidentally.

### 6. Build the simulator before patching gameplay

The simulator will sweep:

- GDP/c from $500 to at least $1 million.
- Education, Government, Cohesion, and core regions.
- Resource regions/GDP, density, and unrest.
- Labor- versus resource-focused technology paths.
- Representative 2022 countries.
- Early, middle, late, and completed technology catalogs.

It will chart:

- Total GDP gained per IP.
- GDP/c gained per IP.
- Annual growth at selected Economy/Spoils allocations.
- Labor and resource constraint contributions.
- Technology’s benefit at different capital intensities.

We will select constants from these results before changing gameplay.

### 7. Required invariants and tests

- Doubling capital alone produces less than twice the output.
- Doubling capital, labor, and resources produces approximately twice the output.
- Merging identical economies does not create a per-IP bonus.
- Every technology raises or preserves returns for every tested factor combination.
- AI-heavy paths disproportionately help labor-constrained economies.
- Energy/resource-heavy paths disproportionately help resource-constrained economies.
- Resource technology relaxes scarcity without subtracting the benefit of actual abundance.
- At full technology, marginal returns remain nearly constant across extreme GDP/c.
- Zero population density, zero resource regions, and extreme GDP remain finite.
- Spoils continues to invoke exactly the same live GDP return as Economy.

### 8. Implementation shape

The final patch should remain readable in one place:

- Remove the old `pcgdpScale`, `pcgdpDecay`, and `pcgdpDecayInterval`.
- Calculate the two pressures and logistic constraints inline in `EconomyGrowthPatch`.
- Keep the patch around 30–50 readable lines with representative numerical comments.
- Do not add a generic math helper.
- Update the Economy tooltip to show the final Economy/Spoils return plus labor, resource, and technology factors.
- Update the simulator, tests, settings defaults, technology CSV validation, and implementation matrix together.
