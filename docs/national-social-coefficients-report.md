# Cohesion, Inequality, and Government coefficient report

## Scope and authority

This is a tuning inventory for Economic Equalization Overhaul (EEO) `0.9.2`
against Terra Invicta `1.0.51`. It covers:

- every EEO path that changes Cohesion, Inequality, or Government;
- retained TI 1.0.51 passive and priority paths that feed those scores;
- the shared boundary and population scalers that alter effective changes;
- project/global effects that change priority completion speed;
- installed event-effect templates that directly change the three scores; and
- the most important cross-couplings among the three scores.

The active shipped defaults in `TIEconomyMod/ModFiles/Settings.xml` match the
C# defaults in `TIEconomyMod/Main.cs` and the currently deployed mod settings.
All features discussed below are enabled.
The TI values were inspected from the installed `Assembly-CSharp.dll` and
`StreamingAssets/Templates` on 2026-08-14. Source and serialized settings are
the authority if this report later becomes stale.

"Government" below is TI's internal `democracy` score on `[0, 10]`.
"Per completion" means one completed national Investment Point (IP), before
priority-speed bonuses affect how quickly completions occur.

## Tuning summary

| Score | Primary positive controls | Primary negative controls | Boundary behavior |
|---|---|---|---|
| Cohesion | Unity; Knowledge when below 5; monthly movement toward the rest value; war/rival/event gains | Knowledge when above 5; Oppression above Government 5; monthly movement; annexation, war outcomes, events | Hard clamp `[0,10]`; no EEO boundary curve |
| Inequality | Economy; Spoils; climate damage; some events and territorial changes | Welfare; revolution/events; merger recomputation can move either way | Priority changes use EEO's smooth `[1,5,9]` curve; other reasons do not |
| Government | Government priority; peaceful-neighbor diffusion; events/regime changes | Spoils; Oppression; war; low Cohesion; zero-Cohesion overflow; coups/revolutions/events | Every change uses the reciprocal EEO factor-3 curve exactly once |

The most important hidden coupling for Cohesion tuning is Inequality. It enters
the Cohesion rest value directly, then independently raises the maximum monthly
downward movement. Government also changes the rest value through several
regime-dependent terms and changes Unity's direct Cohesion gain.

## Common scalers

### EEO inverse-population scaler

EEO's demographic priorities use a direct stock divisor:

```text
change = configured population divisor / population
```

This applies to Unity Cohesion, Knowledge Cohesion, Government Government gain,
and Spoils Government loss. It makes equal per-capita institutional movement
require proportional total IP.

### Retained TI population scaler

Retained TI priority/passive/event paths use:

```text
S(P) = (population / 50,000,000) ^ -0.35
```

`S = 1` at 50 million, about `0.785` at 100 million, and about `0.447` at
500 million. This still applies to Oppression's raw Government and Cohesion
changes, passive Government changes, zero-Cohesion spillover, and event effects
whose template name ends in `_PopScaled`.

### Priority completion-speed bonuses

These additive effects increase the IP sent to a priority; they do not change
the per-completion formulas below. Multiple owned effects stack through TI's
normal priority-bonus system.

| Affected score | Priority | Current effects |
|---|---|---|
| Cohesion | Knowledge | `+0.02` Their Computers; `+0.05` Media Literacy Training; `+0.10` Augmented Learning; `+0.10` The Ivory Tower; a defined global `+0.10` effect |
| Cohesion | Unity | `+0.05` Singleton Viruses; `+0.05` Social Media Campaigns; `+0.25` One People |
| Cohesion | Oppression | `+0.05` Security Measures, Global Ambition, or Hunter-Killer Tactical Units; `+0.10` Singleton Viruses, Counterinsurgency Operations, Global Empire, or Public Behavioral Monitoring |
| Inequality | Economy | `+0.05` from each of seven projects; `+0.10` Civilian Fusion Reactors; a defined unassigned `+0.02` and global `+0.10` effect |
| Inequality | Welfare | `+0.05` Civilian Fusion Reactors; `+0.05` Targeted Community Support |
| Inequality | Spoils | defined global `+0.10` bonus and `-0.10` penalty effects |
| Government | Government | `+0.05` Global Ambition; `+0.05` Media Literacy Training; `+0.10` Global Empire |
| Government | Oppression | same Oppression effects listed for Cohesion |
| Government | Spoils | same defined global `+0.10/-0.10` effects listed for Inequality |

The seven `+0.05` Economy projects are Algorithmic Economic Management,
Civilian Photonic Computing, Civilian Quantum Computing, Xenotourism,
Industrial Atomic Assemblers, Maglev Trains, and Self-Driving Vehicles.

## Cohesion

### Direct priority changes

#### Unity

```text
penalty = clamp(1 - 0.025 * (Education + Government), 0.50, 1)
delta = 3,333,333 / population * penalty
```

Current coefficients:

| Setting | Value | Role |
|---|---:|---|
| `unity.cohesionPopulationDivisor` | `3,333,333` | Base demographic gain |
| `unity.educationAndGovernmentPenaltyPerLevel` | `0.025` | Removes 2.5% per combined Education/Government point |
| `unity.minimumCohesionMultiplier` | `0.50` | Floors Unity at half strength |

At 100 million population, Education 8, and Government 6, the penalty is
`0.65` and the completion gives `+0.021667` Cohesion.

Unity also reduces Education by `-33,333 / population` per completion and its
propaganda pulse is `0.20` of TI's normal strength. Those do not directly add
Cohesion but change later Unity strength and public-opinion contributions to
the Cohesion rest value.

#### Knowledge

```text
step = min(abs(Cohesion - 5), 333,333 / population)
delta = +step below 5, -step above 5, 0 at 5
```

| Setting | Value | Role |
|---|---:|---|
| `knowledge.cohesionPopulationDivisor` | `333,333` | Maximum per-completion movement |
| `knowledge.cohesionTarget` | `5` | Destination that cannot be crossed |

At 100 million population the maximum change is `0.003333`.

#### Oppression (retained TI raw formula)

EEO does not replace Oppression's Cohesion getter:

```text
delta = 0                                      when Government <= 5
delta = -0.025 * (Government - 5) * S(P)      when Government > 5
```

The `-0.025` coefficient is TI's
`conditionalOppressionPriorityCohesionDecrease`; it is hardcoded in the game,
not exposed in `Settings.xml`. At Government 6 and 100 million population this
is about `-0.0196` per completion.

### Passive monthly rest-state movement

EEO retains TI's Cohesion rest-state structure with tuned components. The target is:

```text
rest = clamp(10.5
    + inequality term
    + falling-PCGDP term
    + population term
    + regional-distance term
    + hostile-claims term
    + rivals term
    + wars term
    + public/elite divide term
    + public-opinion dispersion term
    + autocracy term
    + anocracy term
    + high-democracy pull toward 5,
    0, 10)
```

The component formulas and current TI coefficients are:

| Component | Current formula/value |
|---|---|
| Inequality | `min(1, 0.5 + Education/20) * (6.75 - 2.25*Inequality)`; neutral at Inequality `3`, positive below it, negative above it |
| Population | `-(populationMillions ^ exponent)`, exponent `0.20`, or `0.30` for a one-region nation |
| Region dispersion | `max(-7.5, truncateTo0.01(-0.0025 * kmFromCapitalToPopulationCenter))` |
| Falling PCGDP | If current PCGDP is below the maximum recorded over the rolling last 40 quarters (with that peak floored at `100`): `-(1-current/peak)*Inequality`; otherwise `0` |
| Hostile claims | `-16 * hostileRegionPopulationShare * Government/10` |
| Rivals | Up to `+3`; `+0.5` per eligible rival, with remaining room reduced by the war term |
| Wars | Up to `+3`; low-government nations count all extant opponents, while Government `>=6` counts only opponents below 6 |
| Elite/public ideological distance | `-2 * vectorDistance * clamp(Government/10, 0, 1)`; full force at Government `10`, one-tenth force at `1` |
| Public-opinion dispersion | `-0.5 - 6*(antipathyRatio-0.5)` |
| Autocracy (`Government <4.0`) | `(4^1.285 - Government^1.285) * (10-Unrest)/10` |
| Anocracy (`4.0 <= Government <=6.0`) | `3*abs(5-Government)-2` |
| Democracy (`Government >=6.0`) | Pull the accumulated rest value toward 5 by up to `democracyCoefficient * (Government-6.0)`, still stopping at 5; default coefficient `1.0`, vanilla coefficient `0.5` |

The overridden rest-state components are controlled by:

| Setting | Value |
|---|---:|
| `cohesionRest.baseValue` | `10.5` |
| `cohesionRest.autocracyAnocracyBoundary` | `4.0` |
| `cohesionRest.autocracyExponent` | `1.285` |
| `cohesionRest.anocracyDemocracyBoundary` | `6.0` |
| `cohesionRest.democracyCoefficient` | `1.0` |
| `cohesionRest.inequalityEducationBaseMultiplier` | `0.50` |
| `cohesionRest.inequalityEducationDivisor` | `20` |
| `cohesionRest.inequalityOffset` | `6.75` |
| `cohesionRest.inequalityCoefficient` | `2.25` |
| `cohesionRest.publicEliteGovernmentDivisor` | `10` |

TI's detail tooltip contains one base constant for the printed `Base value` row
and a second base constant in its independently accumulated total. EEO replaces
both with `cohesionRest.baseValue`; every other displayed component calls the
same patched getter used by gameplay. Release verification applies all seven
Cohesion gameplay/detail patches together so a detail-patch failure cannot pass
validation and silently roll back the full mod again.

Monthly movement then closes the gap:

```text
if Cohesion < rest:
    delta = min(0.10, rest - Cohesion)
if Cohesion > rest:
    declineCap = clamp(max(0, Inequality - 3)^2 / 10, 0.10, 0.25)
    delta = -min(declineCap, Cohesion - rest)
```

Thus Inequality `<=4` permits at most `-0.10` per month, while Inequality `5`
or higher reaches the `-0.25` cap. These `0.10/0.25` caps and all rest-state
coefficients are retained TI constants, not EEO settings.

### Hard bounds and zero-Cohesion spillover

TI clamps Cohesion to `[0,10]`. When a negative change would push it below
zero, half of the overshoot becomes Unrest. If Government is above 5, up to
`Government-5` of the overshoot is instead converted to Government loss; both
pieces use a `0.50` coefficient and the retained population scaler. The
Government portion subsequently passes through EEO's Government boundary
curve.

### Retained one-off TI changes

These are not configurable in EEO:

| Trigger | Current base coefficient/behavior |
|---|---|
| Declaring war on a newly created rival | Up to `-5`, within a `90`-day rivalry window, scaled by `Government/10`; game cap is `-10` |
| Declaring war on an established rival | `+1` when TI's stability/regime conditions pass |
| Being the target of war | `+3` |
| Answering an ally's defensive war call | `+1` |
| Answering an ally's offensive war call | `+0.5` |
| Region annexation | `-0.25` per applicable region |
| White peace/war end | Recaptures prior war Cohesion gains using war role multipliers `2.0` for attackers and `0.5` for defenders, plus annexed-region and Government terms |
| Revolution | Randomized within TI's hardcoded `-3` to `+3` Cohesion range |
| Other native reasons | Army lost, regime change, secession, coup, independence/release, breakaway, and scripted effects retain their TI formulas |

### Installed event-effect templates

`_PopScaled` values are multiplied by `S(P)`. `_ToExtreme` changes point away
from Cohesion 5. `instantRnd` is TI's template randomization parameter.

| Effect template | Value | `instantRnd` |
|---|---:|---:|
| Massive Cohesion Loss | `-8` | `0.50` |
| Major Cohesion Gain / Loss | `+2 / -2` | `0.05` |
| Minor Cohesion Gain / Loss | `+1 / -1` | `0.02` |
| Mild Cohesion Gain / Loss | `+0.5 / -0.5` | `0.02` |
| Secondary Cohesion Gain / Loss | `+1 / -1` | `0.02` |
| Global Cohesion Gain | `+1` | none |
| Minor Global Cohesion Gain | `+0.5` | none |
| Global / Major Global / Total Global Loss | `-1 / -2 / -6` | none |
| Major / Minor Cohesion Outward, population-scaled | `2 / 1` | none |
| Major / Minor Global Cohesion Outward | `2 / 1` | none |
| Minor Global Human Cohesion Outward | `1` | none |

## Inequality

### Shared priority boundary curve

Let `I` be Inequality, with current bounds `1/5/9`, and let `d` be a signed raw
priority change:

```text
position = (I - 5) / 4
magnitude = abs(position)^2
direction = sign(d) * position
boundary = direction < 0 ? 1 + (3 - 1)*magnitude : 1 - magnitude
final delta = d * boundary
```

| Inequality | Positive multiplier | Negative multiplier |
|---:|---:|---:|
| 1 | `3.00` | `0.00` |
| 3 | `1.50` | `0.75` |
| 5 | `1.00` | `1.00` |
| 7 | `0.75` | `1.50` |
| 9 | `0.00` | `3.00` |

| Setting | Value |
|---|---:|
| `inequality.minimum` | `1` |
| `inequality.neutral` | `5` |
| `inequality.maximum` | `9` |
| `inequality.exponent` | `2` |
| `inequality.maximumDirectionalMultiplier` | `3` |

This curve applies only to the three priority getters. Climate, events,
revolution, secession, and other direct `AddToInequality` calls bypass it.

### Economy

```text
resourceRatio = resourceRegions * 1,000 / max(GDP_billions, 1)
resourceCurve = resourceRatio^0.30 / (1 + resourceRatio^0.30)
resourceMultiplier = 1 + 0.60 * resourceCurve
raw = +0.0015 * 100 / max(GDP_billions, 1) * resourceMultiplier
final = raw * boundary
```

### Welfare

```text
raw = -0.01333332 * 100 / max(GDP_billions, 1)
final = raw * boundary
```

### Spoils

```text
resourceRatio = resourceRegions * 1,000 / max(GDP_billions, 1)
resourceCurve = resourceRatio^0.30 / (1 + resourceRatio^0.30)
resourceMultiplier = 1 + 1.00 * resourceCurve
raw = +0.00666668 * 100 / max(GDP_billions, 1) * resourceMultiplier
final = raw * boundary
```

Current coefficients:

| Setting | Value | Used by |
|---|---:|---|
| `inequality.referenceGdpBillions` | `100` | Economy, Welfare, Spoils |
| `inequality.minimumGdpBillions` | `1` | Denominator floor |
| `inequality.economyChangeAtReferenceGdp` | `+0.0015` | Economy |
| `inequality.welfareChangeAtReferenceGdp` | `-0.01333332` | Welfare |
| `inequality.spoilsChangeAtReferenceGdp` | `+0.00666668` | Spoils |
| `inequality.economyMaximumResourceMultiplier` | `0.60` | Adds up to `+60%` to Economy's raw delta |
| `inequality.spoilsMaximumResourceMultiplier` | `1.00` | Adds up to `+100%` to Spoils' raw delta |
| `abundance.referenceGdpPerResourceRegionBillions` | `1,000` | Shared resource ratio |
| `abundance.resourceCurveExponent` | `0.30` | Shared resource curve |
| `abundance.minimumGdpBillions` | `1` | Resource-ratio GDP floor |

At Inequality 5 and no resource regions, a $100B economy receives the exact
configured reference changes. A $1T economy receives one tenth as much per
completion, while EEO's GDP-linear IP production gives roughly ten times as
many completions at equal allocation.

### Climate

Only changes tagged `InqReason_ClimateChange` are multiplied by:

```text
inequality.climateChangeMultiplier = 4.0
```

TI normally converts its modeled annual climate GDP-loss fraction into an
Inequality increase at one fifth of that fraction. EEO quadruples the resulting
delta. The separate EEO `environment.climateGdpDamageMultiplier = 0.90` scales
warm GDP damage but does not feed the Inequality call, so the two climate knobs
are independent in the current implementation.

### National mergers

EEO replaces the final merger value with a two-distribution Gini approximation.
Inputs are both populations, both PCGDP values, and both prior Inequality
scores. Current coefficients are:

| Setting | Value |
|---|---:|
| `nationalMergers.inequalityMinimum` | `1` |
| `nationalMergers.inequalityMaximum` | `9` |
| `nationalMergers.minimumPerCapitaGdp` | `1` |
| `nationalMergers.inequalityBoundaryEpsilon` | `0.000001` |

The result is clamped inside `(1,9)` by epsilon. Similar distributions stay
similar; large PCGDP gaps add a between-population term. Sequential mergers are
not perfectly associative because TI persists only the merged summary score.

### Retained one-off TI changes

| Trigger | Current base coefficient/behavior |
|---|---|
| Exceeding 9 | Score clamps to 9; the overshoot causes equal Cohesion loss and Unrest gain |
| Federation formation | Up to `+0.05` Inequality under TI's federation formula |
| Resource/colony region annexation | `+0.25` |
| Global energy crisis | Base `+1.5` |
| Revolution | Hardcoded range `-3` to `0` |
| Other reasons | Secession and ordinary event effects retain TI formulas; merger annexation is subsequently replaced by EEO's distribution formula |

### Vanilla research modifiers currently bypassed

TI defines `Effect_EconomyInequalityReduction = x0.90` in context
`Economy_InequalityMultiplier` and
`Effect_WelfareInequalityReductionEffectiveness = x1.25` in context
`WelfareInequalityReductionBonus`. EEO's Economy and Welfare prefixes fully
replace the getters and do not call those contexts. Therefore neither modifier
changes the current EEO per-completion deltas. This is a compatibility/tuning
fact, not an additional multiplier to apply to the formulas above.

### Installed event-effect templates

| Effect template | Value | `instantRnd` | Scaling |
|---|---:|---:|---|
| Major Inequality Gain | `+2.5` | `0.25` | `S(P)` |
| Moderate Inequality Gain | `+1` | `0.10` | `S(P)` |
| Minor Inequality Gain | `+0.3` | `0.05` | `S(P)` |
| Major Inequality Loss | `-2.5` | `0.25` | `S(P)` |
| Moderate Inequality Loss | `-1` | `0.10` | `S(P)` |
| Minor Inequality Loss | `-0.3` | `0.05` | `S(P)` |
| Secondary Inequality Increase | `+0.3` | `0.02` | unscaled |

Event effects bypass EEO's Inequality boundary curve.

## Government

### Shared EEO boundary curve

Every Government change is transformed exactly once. Let `g` be Government
clamped to `[0,10]`, `d` the raw signed change, and `F=3`:

```text
multiplier = F ^ (sign(d) * (1 - g/5))
final delta = d * multiplier
```

| Government | Positive multiplier | Negative multiplier |
|---:|---:|---:|
| 0 | `3.000` | `0.333` |
| 2.5 | `1.732` | `0.577` |
| 5 | `1.000` | `1.000` |
| 7.5 | `0.577` | `1.732` |
| 10 | `0.333` | `3.000` |

| Setting | Value |
|---|---:|
| `government.boundaryCurveFactor` | `3` |
| Curve midpoint | hardcoded `5` |
| Government bounds | hardcoded `0/10` |

The three priority getters are transformed before UI/direct-investment reads.
The central `AddToDemocracy` patch transforms all other reasons and skips those
three priority reasons to prevent double application.

### Government priority

```text
raw = 333,333 / population
final = raw * boundaryCurve
```

| Setting | Value |
|---|---:|
| `government.democracyPopulationDivisor` | `333,333` |

At 100 million population the raw change is `+0.003333`; it becomes `+0.010`
at Government 0, remains `+0.003333` at 5, and becomes `+0.001111` at 10.

### Spoils

```text
raw = -66,667 / population
final = raw * boundaryCurve
```

| Setting | Value |
|---|---:|
| `spoils.governmentPopulationDivisor` | `-66,667` |

At 100 million population the raw loss is about `-0.000667`; the effective
loss is about `-0.000222` at Government 0, `-0.000667` at 5, and `-0.002000`
at 10.

### Oppression (retained TI raw formula)

```text
raw = -0.0025 * S(P)
final = raw * boundaryCurve
```

The raw `-0.0025` is TI's `oppressionPriorityDemocracyDecrease` and is not an
EEO setting.

### Passive monthly changes

All retained raw changes use `S(P)` and then EEO's boundary curve:

| Trigger | Current raw formula |
|---|---|
| At war | `-0.01 * S(P)` every month |
| Peaceful-neighbor diffusion | `+0.005 * S(P)` if at least one adjacent human nation exists and every adjacent human nation considered by TI is both at peace and more democratic |
| Low Cohesion | Each month, probability `max(0,(4-Cohesion)/4)` of `-0.01*S(P)`; EEO first multiplies it by `government.passiveLowCohesionMultiplier = 0.50` |
| Zero-Cohesion overflow | Up to `-0.50 * min(overshoot, Government-5) * S(P)` when Government is above 5 |

The passive low-Cohesion path is the only Government reason receiving the
extra `0.50` multiplier. At-war, neighbor, zero-Cohesion, coup, revolution,
regime-change, secession, and event changes receive only the boundary curve.

### Other retained TI Government sources

The central curve covers the complete reason enum: Government, Oppression,
Spoils, low Cohesion, zero Cohesion, revolution, coup, regime change, amicable
release, secession, at war, nearby democracies, and event effect. These source
magnitudes remain in the game assembly or event templates; EEO exposes only the
curve factor and low-Cohesion multiplier.

### Installed event-effect templates

All of these unscaled template values subsequently pass through EEO's
Government boundary curve:

| Effect template | Value | `instantRnd` |
|---|---:|---:|
| Small Democracy Loss | `-0.01` | none |
| Democracy Loss 0.2 | `-0.2` | `0.01` |
| Democracy Loss 0.5 | `-0.5` | `0.02` |
| Democracy Loss 1 | `-1` | `0.25` |
| Democracy Loss 5 | `-5` | `0.50` |
| Democracy Loss 7 | `-7` | `0.50` |
| Secondary Democracy Loss 1 | `-1` | `0.25` |
| Small Democracy Gain | `+0.25` | none |

## Cross-effects and downstream consequences

These do not directly change the named score, but they matter when tuning its
coefficient because they change incentives or feedback:

| Source/input | Downstream effect | Current coefficients |
|---|---|---|
| Unity or Spoils priority | Cohesion rest state through public opinion | Each propaganda pulse is `0.20` of TI's normal public-opinion shift; opinion then enters the rest formula through `-0.5 - 6*(antipathyRatio-0.5)` |
| Oppression priority | Cohesion rest state through Unrest | Unrest reduction uses `2,222,222 / population`, multiplied by `(10-Government)/10`; lower Unrest raises the autocracy contribution and reduces other instability risks |
| Cohesion | Economy growth labor support | `1.20 - 0.04*abs(Cohesion-5)`; reference Cohesion `5` |
| Cohesion | National research | `1.25 - 0.10*abs(Cohesion-5)` |
| Cohesion | Passive Government loss | Below 4, monthly probability `(4-Cohesion)/4`; raw `-0.01*S(P)`, then EEO `x0.50` and boundary curve |
| Cohesion | Zero-bound spillover | Negative overshoot creates Unrest and, above Government 5, Government loss at the `0.50` split coefficient |
| Inequality | Cohesion target | `min(1, 0.5+Education/20) * (6.75-2.25*I)`; neutral at `I=3` |
| Inequality | Cohesion decline speed | `clamp((I-3)^2/10, 0.10, 0.25)` |
| Government | Unity Cohesion gain | Each point combines with Education for a `-0.025` Unity multiplier, floored at `0.50` |
| Government | Cohesion rest state | Changes autocracy/anocracy/high-democracy, hostile-claim, rival, and war components; also scales the elite/public-distance penalty by `clamp(Government/10,0,1)` |
| Government | Economy growth labor support | `1 + 0.05*Government`, normalized to reference Government `6` |
| Government | National research | `max(Government,0.10)^0.20` |
| Government | Spoils payout | `1.30 - 0.03*Government`, clamped over Government `[0,10]`; gives `x1.30` at 0 and `x1.00` at 10 |
| Government | Oppression Unrest reduction | EEO formula fades linearly as `(10-Government)/10` |

## Practical tuning map

For isolated first-pass changes:

- Change Unity's direct Cohesion strength with
  `unity.cohesionPopulationDivisor`. Change its regime/education sensitivity
  with `educationAndGovernmentPenaltyPerLevel` or its floor. Knowledge has a
  separate, much smaller neutralizing divisor.
- Change priority-driven Inequality speed with the three
  `*ChangeAtReferenceGdp` coefficients. Change resistance near 1/9 with
  `inequality.exponent` and `maximumDirectionalMultiplier`; change resource
  dependence with the two maximum resource multipliers. The GDP reference
  shifts all three together.
- Change direct democratization with
  `government.democracyPopulationDivisor`. Change all Government gains and
  losses, including events, with `government.boundaryCurveFactor`. Change only
  the low-Cohesion passive loss with `passiveLowCohesionMultiplier`.
- If Cohesion still collapses after direct-priority tuning, inspect Inequality
  and the retained rest-state coefficients before increasing Unity again. The
  monthly rest system can dominate the per-completion deltas.
- If Government balance differs by nation size, remember that Government and
  Spoils use direct inverse population while retained Oppression/passive/event
  paths use TI's shallower exponent scaler.

Changes to retained TI constants require a new patch and a corresponding
setting; editing `Settings.xml` alone cannot alter them.

## Source map

- EEO settings: `TIEconomyMod/Main.cs` and
  `TIEconomyMod/ModFiles/Settings.xml`
- Cohesion/Government priorities and curve:
  `TIEconomyMod/Patches/SocialPriorityPatches.cs`
- Unity and Spoils:
  `TIEconomyMod/Patches/EnvironmentUnitySpoilsPatches.cs`
- Economy Inequality: `TIEconomyMod/Patches/EconomyPatches.cs`
- Government curve math: `TIEconomyMod/Core/GovernmentMath.cs`
- Merger Inequality: `TIEconomyMod/Patches/NationalMergerPatches.cs`
- Formula verification: `tests/FormulaTests/Program.cs`
- Existing design authority: `docs/design-directives.md` and
  `docs/government-boundary-curve.md`
- TI 1.0.51 retained formulas: installed `Assembly-CSharp.dll`, principally
  `TINationState`, `TIGlobalConfig`, and `TIEffectsState`
- TI effect values: installed `StreamingAssets/Templates/TIEffectTemplate.json`
  and `TIProjectTemplate.json`
