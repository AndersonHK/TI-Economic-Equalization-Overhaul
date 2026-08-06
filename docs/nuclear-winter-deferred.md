# Nuclear Winter modification — deferred

Status: deferred in Version 0.9.0. Terra Invicta 1.0.51 vanilla threshold,
cloud, and achievement behavior is preserved; the investigation below records
the earlier 1.0.49 experiment.

The proposed Nuclear Winter change is not active. Terra Invicta's vanilla
particulate threshold, cloud behavior, and `nuclearWinter` achievement logic
have been restored unchanged.

## Intended behavior

- Cloud cover would activate only when stratospheric aerosols caused more than
  1°C of cooling.
- Nuclear Winter and its achievement would require both that aerosol threshold
  and a net global temperature anomaly below 0°C.
- Cloud cover would depend on aerosols alone.

## Investigation

TI 1.0.49 implements this behavior in compiled code rather than data templates:

- `TIGlobalValuesState.AddStratosphericAerosols_ppm` contains the particulate
  crossing checks and calls `UnlockAchievement("nuclearWinter")`.
- `RotateCloudsSolarSystemScene.InitAlbedoControl` initializes the cloud material
  from a separate hardcoded threshold.
- `EarthParticulateThresholdChanges` drives the cloud material; no separate
  narrative-event or notification template was found.

This means the proposed change could not be implemented safely in JSON alone.

## Failed attempts

1. The four live particulate comparisons were rewritten from the vanilla
   0.01 ppm threshold to a strict 0.03885 ppm threshold. A combined
   aerosol-plus-net-temperature achievement check was injected at the common
   return from the aerosol update.
2. The achievement check was moved to a Harmony postfix so it would run after
   every normal aerosol update. The saved-scene cloud initializer was also
   rewritten to use the strict 0.03885 ppm threshold.
3. Both approaches passed static IL validation and formula tests, but repeated
   fresh-save play tests reached well beyond both intended gates without the
   expected observed result.

Achievement testing was also complicated by account-wide achievement state and
the achievement-enabling mod's console restrictions, while the particulate event
itself has only a cloud-material listener. These uncertainties were not sufficient
reason to retain an unverified runtime patch.

## Deferred scope

A future attempt should begin with runtime instrumentation that independently
records aerosol method entry, threshold transitions, event dispatch, active-player
resolution, and the final achievement API call in a clean test profile. Until
then, no Nuclear Winter or cloud-threshold modification is shipped.

The separate change suppressing worldwide GDP percentage penalties from nuclear
strikes remains active. Direct GDP and population damage in the target region
and all other local nuclear effects remain vanilla.
