# Coup frequency and Inequality stabilization

## Status and scope

The implemented design is a `-0.10` coup Inequality change followed by an
immediate Cohesion reset to the newly calculated rest state, with a floor of
zero. It was built, validated, packaged, and deployed as Economic Equalization
Overhaul (EEO) `0.9.3` against the installed Terra Invicta `1.0.51` assembly on
2026-08-18. Manual in-game testing remains pending.

## Findings

### Organic coups remain likely in the zero-Cohesion state

TI checks each nation for revolution and then organic coup once every four
days. Ignoring the elite modifier, the coup chance on each check is:

```text
risk index = (2*Unrest - Cohesion - Government) / 10
p(check) = 4 / 6000 * risk index
```

If the elite Spoils demand is not met and the controlling faction has less
than 65% public support (or there is no sole controller), TI adds:

```text
corruption - Spoils allocation
```

to the risk index after its division by 10. The addition is positive because
this branch is entered only when Spoils allocation is below corruption.

At Unrest 10 and Cohesion 0, the unmodified risk is:

| Government | Chance per four-day check | Approximate annual chance |
|---:|---:|---:|
| 0 | 0.1333% | 11.47% |
| 1 | 0.1267% | 10.93% |
| 2 | 0.1200% | 10.38% |
| 3 | 0.1133% | 9.84% |
| 5 | 0.1000% | 8.73% |

For a representative Government 1, Cohesion 0 country with PCGDP from $1,000
to $5,000, TI's corruption formula gives about `0.46-0.47`. With zero Spoils
allocation and the public-support condition met, annual organic coup risk rises
to about `13.4%`. Ten such countries produce about a 76% chance of at least one
organic coup somewhere in a year, before councilor missions or events.

The loop resets quickly. A coup reduces Unrest by a uniform `0` to `3` points,
an average of `1.5`. When Cohesion is exactly zero and resting Unrest is higher,
TI restores up to `1.0` Unrest per month rather than the normal `0.25`. The
country is therefore usually back near Unrest 10 after roughly one to three
monthly updates.

### Resting Unrest is capped at 10, but its raw target can exceed 10

The current formula is:

```text
raw unrest rest = 10.5 - Cohesion - PCGDP / campaign divisor
                  + army suppression + xenoforming + hostile claims
unrest rest = clamp(raw unrest rest, 0, 10)
```

The campaign divisor is starting global GDP multiplied by `6.26e-11`.
Consequently, very poor countries at zero Cohesion commonly have a raw target
above 10 even before positive xenoforming or hostile-claim terms. The gameplay
target itself is 10 because of the clamp, so Stabilize Nation cannot permanently
break the cycle while Cohesion remains zero.

### The coup's intended Government route is ineffective in the problem state

Vanilla TI gives a coup a uniform Government change from `-2` to `+1`, with a
raw mean of `-0.5`. EEO then applies its reciprocal factor-3 Government boundary
curve separately to positive and negative changes. The resulting expected coup
change is:

| Starting Government | Expected effective change under EEO |
|---:|---:|
| 0 | `+0.278` before the lower-bound clamp |
| 1 | `+0.125` |
| 2 | `-0.023` |
| 3 | `-0.171` |
| 5 | `-0.500` |

Thus coups do not reliably lower Government in the most authoritarian states;
at Government 0-1 they raise it on average. Even an actual Government reduction
has almost no immediate Cohesion benefit at Unrest 10. EEO's autocracy term is:

```text
(4^1.285 - Government^1.285) * (10-Unrest) / 10
```

and is therefore exactly zero at Unrest 10. The coup's temporary Unrest drop
allows only a short-lived partial benefit before rapid Unrest recovery.

### Current coups do not raise Inequality directly

The installed `TINationState.Coup` method contains no `AddToInequality` call.
Its national-value effects are:

| Score | Current coup effect |
|---|---:|
| Government | uniform `-2` to `+1`, then EEO Government curve |
| Unrest | uniform `-3` to `0` |
| Cohesion | uniform `-1` to `+1` |
| GDP | uniform `-10%` to `0%` |
| Inequality | no direct change |

The average 5% GDP loss nevertheless worsens the situation indirectly. It
raises resting Unrest through lower PCGDP and can add about
`0.05 * Inequality` to the falling-PCGDP penalty in the Cohesion rest formula
when the country had been at its recorded PCGDP peak. At Inequality 6 that is
about another `-0.30` to resting Cohesion after an average coup GDP loss.

## Approved change

Every completed coup receives a deterministic `-0.10` Inequality change. After
that change, Cohesion is set to the nation's newly calculated Cohesion rest
state, floored at zero. The calculation therefore includes the coup's new
Government, Unrest, GDP, control-point and public-opinion state, and Inequality.
It replaces the coup's randomized final Cohesion rather than waiting months for
passive movement to reach the new equilibrium.

EEO's Inequality contribution to resting Cohesion is:

```text
min(1, 0.5 + Education/20) * (6.75 - 2.25*Inequality)
```

The resting-Cohesion gain from `-0.10` Inequality alone is:

| Education | Resting-Cohesion gain |
|---:|---:|
| 3 | `+0.146` |
| 5 | `+0.169` |
| 8 | `+0.202` |
| 10+ | `+0.225` |

The direct Inequality effect is deliberately small. The main cycle-breaking
effect is the immediate equilibrium reset: a nation whose revised rest state is
positive leaves zero Cohesion at once, while a nation whose calculated result
remains negative stays at zero. Repeated coups can still accumulate additional
Inequality relief.

## Implementation

The implementation:

1. Adds configurable coup enable, `-0.10` Inequality, and Cohesion-reset settings
   to C# defaults, `Settings.xml`, and the formula-test settings stub.
2. Uses a guarded Harmony postfix on `TINationState.Coup` that calls
   `AddToInequality` once after a successful coup, reads the updated
   `cohesionRestState`, floors it at zero, and moves current Cohesion exactly to
   that target. TI has no coup-specific `InequalityChangeReason`; use
   `InqReason_EventEffects` unless a dedicated UI reason is worth a substantially
   more invasive enum/localization patch.
3. Applies it to organic, mission, and event coups consistently. It does not scale it
   by councilor mission strength: TI's existing national-value coup effects do
   not use that strength either.
4. Includes formula tests for the default, independent Cohesion-reset disable,
   full feature disable, and the lower bound. Target-IL validation confirms the
   expected `Coup` method still exists and retains the known signature.
5. Updates this report, the social-coefficients report, patch sanity audit,
   implementation matrix, README scope/version notes, and release notes as
   appropriate.
6. Uses the normal `tools/deploy.ps1` flow without `-SkipVerification`. Manual
   testing should cover organic and councilor coups, watching Inequality,
   resting Cohesion, resting Unrest, repeat-coup cadence, and whether intentional
   coup farming becomes attractive.

The principal tuning risk is not technical; it is incentive design. The reset
can raise or lower Cohesion sharply in any nation whose current value is far
from equilibrium, and the fixed Inequality reduction rewards coups even in
stable rich nations. Manual testing must therefore include deliberate coup use
outside failed states as well as the intended zero-Cohesion loop.

## Deployment record

The normal `tools/deploy.ps1` flow completed without `-SkipVerification` and
deployed 44 files to the enabled mod directory. Verification included 1,078
formula assertions, all 143 Harmony patch applications, the guarded TI 1.0.51
`Coup` signature and native effect inventory, settings/matrix consistency, and
release-package validation.

```text
Artifact: artifacts/TIEconomyMod-0.9.3-ti1.0.51.zip
DLL SHA-256: B8A30A49F23C839DC9878B61CC62349AC1FDA6CA80B4476FD7E38550F1BFFD5D
```
