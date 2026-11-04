# Alien machinery-bay runtime update — 2026-11-04

Status: **implemented and deployed as 0.10.24**. This implements
the [audited alien machinery allowances](alien-reactor-bay-measurements-2026-11-04.md).
It does not implement the proposed tier or size-5 changes.

## Plan

Add the fifteen audited appearance-0 allowances to the existing production
`ShipBalanceMath.ReactorBayVolume_m3` path. Keep the twelve human measured tables
unchanged. Distinguish eleven geometry-derived machinery proxies from four
calibrated small-hull fallbacks; do not report inferred internal reactor volumes
as directly measured reactor chambers. Unknown hulls/appearances retain the
existing conservative size-band fallback.

Use static prepared constants with no live mesh reads or recurring geometry
work. Both reactor compatibility and the designer's engine-bay overlay must
resolve the same values. Preserve saved hull/module identities, existing
equipment and fuel; current limits apply when editing/refitting or generating
new designs. No slot/layout migration or weapon conversion is required.

All three current alien reactors already reach their theoretical output caps
on Mothership and Titan; this geometry correction alone raises neither cap.
Several smaller hulls have lower effective limits. Verify the actual cap-rated
drive comparison and existing save/load behavior, and retain the existing AI
repair/rejection boundaries for newly generated invalid propulsion fits.

Add numeric/metadata coverage for all fifteen hulls, human/unknown-appearance
negative cases and reactor cap behavior. Advance the package to 0.10.24 and run
`tools/deploy.ps1` without skipping verification as soon as implementation is
ready. Announce manual-test readiness immediately after deployment; finish the
separate asymmetric-tier proposal and documentation afterward.

## Deployed inputs

| Hull | Runtime allowance (m³) | Source |
| --- | ---: | --- |
| AlienGunship | 262.779 | Calibrated small-hull fallback |
| AlienEscort | 177.520 | Calibrated small-hull fallback |
| AlienCorvette | 317.839 | Calibrated small-hull fallback |
| AlienFrigate | 1,152.553 | Geometry-derived machinery proxy |
| AlienMonitor | 776.259 | Geometry-derived machinery proxy |
| AlienDestroyer | 2,571.761 | Geometry-derived machinery proxy |
| AlienCruiser | 2,332.440 | Geometry-derived machinery proxy |
| AlienBattlecruiser | 3,544.629 | Geometry-derived machinery proxy |
| AlienBattleship | 5,860.080 | Geometry-derived machinery proxy |
| AlienLancer | 9,486.201 | Geometry-derived machinery proxy |
| AlienDreadnought | 6,012.812 | Geometry-derived machinery proxy |
| AlienTitan | 14,625.724 | Geometry-derived machinery proxy |
| AlienAssaultCarrier | 6,865.571 | Geometry-derived machinery proxy |
| AlienMothership | 137,456.800 | Geometry-derived machinery proxy |
| SalamanderGunship | 350.771 | Calibrated small-hull fallback |

The public `TryGetAlienReactorBayVolume_m3` provides the prepared appearance-0
values and calibrated-fallback metadata. `ReactorBayVolume_m3` selects the same
values for compatibility and the designer overlay. Unknown appearances do not
silently inherit index 0. The human measured map and unknown-hull class fallback
remain unchanged; fallback diagnostics now describe their actual source.

## Validation and deployment

On 2026-11-04 the full normal deployment passed **41 validators, 1,339 formula
assertions and the 120-row / 255-patch matrix**. The script checked that the game
was closed before validation and before replacement; all **52 installed files**
matched the package. The new checks cover every configured alien hull, source
metadata, unknown appearances/hulls, preserved human measurement semantics, lower
small-hull effective limits and unchanged current Titan/Mothership reactor ceilings.

DLL SHA-256: `E0160A34098A6B0C41EFD7AC0AD4154D65F90F25C3917A64F18FAE999A6414C5`.
Package: `artifacts/TIEconomyMod-0.10.24-ti1.0.53.zip`.

A separate read-only check used the actual production propulsion-compatibility
helper, current settings, merged drive/plant templates and the latest save's
alien designs: **27 unique configurations covering 68 design records**. Twenty-four
configurations remain valid. Three formerly valid configurations become over-limit:

| Hull / saved propulsion | Required cap-rated output (GW) | New effective maximum (GW) |
| --- | ---: | ---: |
| Alien Gunship, Fusion Torch x2 + basic Hybrid reactor | 4,603.3 | 1,401.5 |
| Alien Gunship, Fusion Torch x2 + advanced Hybrid reactor | 4,603.2 | 4,004.3 |
| Alien Escort, Fusion Lantern x3 + basic Hybrid reactor | 1,136.9 | 946.8 |

No migration rewrites their stored equipment or fuel. Designer clones/art refresh
and refits enforce current limits and may reduce a cluster or clear it if no x1
variation fits. Newly generated AI designs retain the existing capacity-repair/
rejection boundaries. These native-helper checks do not constitute a full campaign
load or end-to-end live AI generation test. The
[deployed input and preflight snapshot](alien-reactor-bay-runtime-0.10.24-2026-11-04.json)
records the runtime rows and all cases.

Manual acceptance remains: verify engine-bay numbers on each affected hull,
load/save/reload an existing campaign without losing hardware/fuel, edit/refit an
over-limit design, and observe fresh alien designs across available reactor tech.
The proposed asymmetric tiers, Carrier compact fitting and N5 rules are separate
planning work and are not included in this build.
