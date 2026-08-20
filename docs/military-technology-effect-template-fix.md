# Military technology effect template launch fix

Date: 2026-08-19

## Symptom

Terra Invicta 1.0.51 aborted its first modded launch and set its bad-template recovery flag. The following launch disabled mods and displayed **Bad Template Files Detected**.

`Player-prev.log` identified the first fatal error:

```text
Cannot add invalid template: No context or instant trigger assigned for Effect_IncreaseMaxArmyTechLevel
```

The later `LocalizationManager` and `Loc` exceptions occurred during teardown after template initialization had already failed.

## Cause

The Mil Tech rebalance supplied this partial `TIEffectTemplate` override:

```json
{
  "dataName": "Effect_IncreaseMaxArmyTechLevel",
  "value": 0.25
}
```

The override assumed template fields would be merged individually with the vanilla effect. Terra Invicta instead replaced the complete matching effect object. This discarded its `NationMaxMiltechChange` instant trigger, all-nations target, instant duration, and duration sentinel, leaving an invalid effect.

## Fix and prevention

The override now reproduces the complete vanilla instant-effect definition while retaining the intended value of `0.25`:

```json
{
  "dataName": "Effect_IncreaseMaxArmyTechLevel",
  "instantEffect": "NationMaxMiltechChange",
  "value": 0.25,
  "effectTarget": "AllNations",
  "effectDuration": "instant",
  "duration_months": -1
}
```

The release verifier previously checked those structural fields only on the installed vanilla template and checked only `value` on the mod override. It now requires the complete structural contract on the override itself. The corrected package passed the normal release verification and deployment workflow before another launch attempt.
