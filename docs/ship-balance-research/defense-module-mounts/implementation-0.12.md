# Orbital defense arrays in 0.12

## Approved design

Use the [measured turret placements](clearance.md) for the human and alien orbital modules. Each weapon has one cooldown and a top and bottom rendering. Alternate laser and magnetic deck weapons, with alternating magnetic/plasma roles on T3 when eligible plasma is researched. Use the approved full count of dedicated PD groups.

| Human module | Batteries | Dedicated PD | Active draw | Deferred draw | Active base mass | Deferred base mass |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Point Defense Array | 5 | 6 | 10 | 40 | 150 t | 550 t |
| Layered Defense Array | 25 | 12 | 40 | 400 | 750 t | 9,300 t |
| Battlestations | 46 | 20 | 240 | 2,000 | 6,000 t | 94,200 t |

The installed power unit has no canonical MW conversion. Use roughly 10 MW per power point as a tuning reference. Existing modules inherit live template values after a reload. No retroactive construction bill is charged to a save. Orbital armor coverage and the native mass-dependent HP formula are outside this release. Surface counterfire continues to use the native surface-to-orbit rules.

The alien layouts have 2/3, 8/6 and 8/6 batteries/PD. Their existing habitat economic values are the baseline until an alien rebalance is approved.

The three human economic templates are shared by orbital and surface habs. Ground defenses use the same power and base masses and retain their existing surface-to-orbit weapons. The active draws and masses restore the 0.11.19 values, including the base mass input to native orbital HP. The six compensating generation overrides from the initial 0.12 deployment are removed.

Revisit the deferred values during the broader module rework. The isolated defense increase made those modules outliers. Compare base mass, construction material tonnage, power generation, and station capacity across module families before setting new defense values. Keep the roughly 10 MW reference for the weapon recharge comparison.

## Call paths and validation

- The layout data binds the measured coordinates and model scaling to named module and role records. One selector uses native research eligibility and feeds the ordered rendered and simulated loadouts, module previews and range, and strategic strength. Existing-state initialization rebuilds the derived state.
- Rendered batteries use the fitted models and own-shell checks. Simulated combat uses the same weapon complement under the native formation abstraction. No physical turret pose is available to that simulation.
- Check each named module's count, paired coordinates and model scales, template economics and package version. Verify the layout adapters and the project-completion path. The normal deployment flow runs the build and validators before installing.

## Initial deployment and evidence

Deployed 0.12.0 to the installed Terra Invicta 1.0.65 on November 22, 2026. The release verification completed both validator pools and all 1,896 formula assertions. The orbital checks covered six loadouts, 294 physical mount poses, eight preset budget checks, the initializer's IL handoff and ten Harmony adapter targets. Eight adapters bound in the standalone CLR. The combat initializer and return-to-strategy hooks require Unity's native calls, so their in-scene binding, turret motion and visual cleanup remain manual tests.

A read-only pass over the campaign combat autosave found 62 completed human orbital defense modules on 27 stations. Jahn Station's two Layered Defense Arrays add 720 power draw. Native startup rebalances a hab with a power deficit using the new template values.

The deployment verified all 60 installed files. The deployed DLL SHA-256 is `327FAB20D590FFE8EE6B7A4942A659F8B6246A3833659CFCAF9BA8F5D98F1644`. The release ZIP SHA-256 is `9F2A7407E310E2EE5E5E7765E288E653698E1D80B555D28EEA41FBC2D0FE80EE`.

The player reported that the expanded defenses worked in game and supplied an image of the upgraded Layered Defense Array at Jahn Station. After that test, the player requested restoring the pre-0.12 draws until power generation and station slots are reworked. Jahn's two arrays return to a combined draw of 80 power.

The temporary draws were deployed as a data revision of 0.12.0 on November 22, 2026. The full release verification passed and the deployment verified all 60 installed files. I checked the three installed power and mass records and the unchanged layout hash. The ZIP for that power-only revision had SHA-256 `49B29B13E72CFDA338A27A83161639EEEFF146FDC6C7C3E4ADA8768B25820316`.

The follow-up mass revision was deployed on November 22, 2026. Full verification passed, and all 60 installed files matched the release. The installed template has the legacy draws and masses in the table above, and the orbital layout hash matches the power-only revision. The current release ZIP SHA-256 is `144A6CB1D8D194410B57464B1E6114F47D891BFAEF23B7B091BCEC8739EC1669`.

## Base mass comparison

| Module family | T1 | T2 | T3 |
| --- | ---: | ---: | ---: |
| Defense, active and pre-0.12 | **150 t** | **750 t** | **6,000 t** |
| Defense, deferred calculation | 550 t | 9,300 t | 94,200 t |
| Platform / Orbital / Ring Core | 25 t | 150 t | 1,200 t |
| Energy Lab / Research Center / Institute | 30 t | 150 t | 1,200 t |
| Solar Collector / Array / Farm | 160 t | 620 t | 1,800 t |
| Heavy Fusion Pile / Reactor Array / Reactor Farm | 180 t | 900 t | 7,200 t |
| Space Dock / Shipyard / Spaceworks | 120 t | 600 t | 4,800 t |
| Marine Platoon / Company / Battalion Barracks | 180 t | 900 t | 7,200 t |
| Particle Collider / Atomsmasher / Supercollider | 300 t | 1,500 t | 12,000 t |
| Interstellar Launching Laser | — | — | 375,000 t |

Values are nominal EEO template mass in metric tonnes. Site effects and material weights enter the construction calculation separately.
