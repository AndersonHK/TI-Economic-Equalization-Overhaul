# Minimum AI fuel-capacity and appearance-lock plan

Status: implemented and deployed on 2026-08-16. This minimum pass favors
deterministic, auditable behavior over role-aware appearance optimization.

## Implemented behavior

- An exception-safe AI design context applies the faction's existing
  vanilla-style drive-class appearance immediately after every provisional
  `SetDriveTemplate` call. Human candidates therefore have a locked appearance
  before `GetBestPowerPlant`; aliens use appearance 0; refits preserve the
  original resolved appearance.
- `GetIdealPropellentTankCount` is capped at the selected hull/appearance,
  drive-propellant, module, and crew capacity. Its `actualDV` output is
  recomputed with the rocket equation at the legal count, so candidate scoring
  never consumes the discarded, impossible delta-v.
- The alien designer caps all three direct tank assignments, lowers its initial
  target to achievable delta-v, and replaces the vanilla 250 kps target floor
  with `min(250 kps, current maximum achievable delta-v)`. This prevents a
  small vessel's unreachable target from being raised again every 25 passes.
- The STO fighter's direct tank increment is capped. Its completed propulsion
  is checked against reactor size and engine-bay volume; if necessary, the
  repair path tries lower-thruster drive variants and compatible allowed power
  plants without exceeding the capacity rules.
- Normal designs and refits receive a completed-design boundary check. The
  AI save action repeats the fuel, drive/reactor, and engine-bay invariant as a
  defensive guard, without modifying loaded legacy templates.
- The reactor and engine-bay checks reuse the shared patched
  `validDriveForShipsPowerPlant` and `ValidPowerPlantForShipsDrive` paths.
  Consequently early power-plant selection, later drive-variation checks, and
  the final invariant all use the same appearance-specific limits.

The TI 1.0.51 deployment passed 1,059 formula assertions, all 142 Harmony
patches, guarded alien/STO IL validation, release packaging, and the 35-file
enabled-mod deployment. Manual AI/alien ship-generation testing remains
pending.

## Decision

The minimum pass will not ask the AI to optimize or randomly select graphical
hull variants. A human AI design will use a fixed vanilla-style mapping from
the candidate drive to one appearance. That appearance will be assigned and
locked before any appearance-dependent design calculation.

Aliens continue to use appearance 0. AI refits preserve the original ship
design's appearance. Human and alien surface-to-orbit fighters retain their
authored/default appearance unless a separate fighter rule is documented.

The richer role-aware and randomized alternative is preserved separately in
[the hypothetical appearance-selection plan](ai-hull-appearance-selection-hypothetical.md).

## Deterministic human mapping

Use the faction template's existing fields and vanilla drive-class mapping:

| Candidate drive | Configured appearance |
|---|---:|
| Chemical | `hullIndex_chem` |
| Electrothermal, electromagnetic, or electrostatic | `hullIndex_electric` |
| Fission thermal, fission pulse, or nuclear salt water | `hullIndex_fission` |
| Fusion thermal or fusion pulse at no more than 100 GW | `hullIndex_fusion` |
| Fusion thermal or fusion pulse above 100 GW | `hullIndex_fusion_adv` |
| Antimatter | `hullIndex_amat` |
| Any unhandled classification | `hullIndex_default` |

Pass the configured value through `TIUtilities.GetHullAppearanceIndex` before
storing or consuming it. This preserves vanilla's DLC fallback, under which
configured appearances 2 and 3 resolve to 0 and 1 when the relevant bundles
are unavailable.

All seven installed human factions currently share the same values:

```text
default=0, chemical=2, electric=2, fission=0,
fusion=3, advanced fusion=1, antimatter=1
```

The mapping is deterministic for a given drive template, including its
thruster variation and resulting power requirement. Existing drive-keyed AI
caches therefore remain meaningful in this minimum pass.

## Human design order

Vanilla currently assigns the AI appearance only after it has selected the
winning design. Move the same mapping into candidate construction:

```text
choose the role and hull using the existing logic

for each candidate drive:
    create the provisional ship
    install the candidate drive
    resolve and lock its deterministic appearance
    choose a power plant compatible with that appearance and drive
    choose radiator, armor, weapons, and utility modules
    calculate complete crew and dry mass
    calculate maximum legal tanks
    request the ideal tank count and clamp it to the maximum
    calculate and cache actual delta-v and acceleration

apply the existing candidate filters and scoring
```

The late vanilla appearance assignment may remain as an idempotent safeguard,
but it must resolve to the same value and must not change a completed design.

Once the cap-aware ideal-tank result supplies actual delta-v, the existing
human designer already compares different drives and can prefer a drive that
reaches the target. When none reaches it, the existing relative filters and
score can select a lower-delta-v candidate. No appearance retry or appearance
optimization is required.

If `maximumTanks` is zero, discard that candidate before normal scoring because
`TISpaceShipTemplate.ValidTemplate` requires at least one propellant tank.

## Shared tank calculation

Patch `TISpaceShipTemplate.GetIdealPropellentTankCount` so every caller receives
a legal result:

```text
legal_tanks = min(vanilla_ideal_tanks, maximum_tanks)
actual_delta_v = modified_EV * ln(
    (dry_mass + legal_tanks * propellant_tank_mass) / dry_mass)
```

Do not report vanilla's uncapped `actualDV` after reducing the result. The AI's
drive cache, candidate filters, transfer checks, and score must all consume the
same attainable delta-v that the finished design will have.

This hook covers normal human design and most refit calculation. It does not
cover alien and fighter loops that modify `propellantTanks` directly.

## Refits

Set the provisional refit's appearance from `original.hullAppearanceIndex`
before selecting its drive and power plant or calculating capacity. Then apply
the shared ideal-tank cap.

Preserve vanilla's rule that an AI refit is rejected when it cannot retain the
original design's delta-v. The minimum pass must not change art to rescue a
refit.

## Alien design

Assign and lock appearance 0 immediately after creating the alien provisional
design. Retain vanilla's initial drive choice and armor-tuning behavior for the
minimum pass.

Replace unrestricted tank growth with a capacity-aware target:

```text
maximum_tanks = capacity of the current complete design
maximum_delta_v = delta-v at maximum_tanks
effective_delta_v_target = min(vanilla_delta_v_target, maximum_delta_v)
```

Each tank addition is limited to remaining capacity. Recompute capacity and
the effective target whenever drive variation, crew, modules, or another input
to the capacity model changes. Reaching the cap is a normal terminal condition,
not a reason to continue adding tanks until `designPasses` expires.

For this minimum pass, an alien design may settle for the selected drive's
maximum attainable delta-v. A later drive-aware pass may retry other drive
families when another propellant or exhaust velocity would perform better.

If appearance 0 and the selected drive cannot carry one tank, the candidate is
invalid and must not be saved. Log this separately from an ordinary
insufficient-delta-v result so a future drive fallback can diagnose it.

## Fighter paths

Audit `DesignSTOFighter` and any other direct `propellantTanks++` path. Clamp
each addition to remaining capacity and terminate normally at the cap. These
paths use their authored/default appearance in the minimum pass and do not
participate in human capital-ship appearance mapping.

## Final enforcement invariant

Player-designer UI reconciliation is not an AI guard. AI designs call
`SaveShipDesignAction` and `TIFactionState.SaveShipDesign` without passing
through `FleetsScreenController.SaveDesign`.

Add a central pre-save check for every newly saved design:

```text
0 < propellantTanks <= maximumTanks(hull, locked appearance, drive, modules, crew)
```

The creator should already have produced a legal count. The save check is a
defensive invariant and diagnostic, not the primary mechanism for teaching the
AI. A save-time clamp alone would let candidate scoring use impossible
performance and choose the wrong drive.

Loaded legacy designs and already-built ships require a separately documented
compatibility decision; this plan governs newly generated and newly saved AI
designs.

## Interaction with future variant statistics

Locking appearance early is the compatibility boundary for future
appearance-specific hull mass, dimensions, hit profile, drive scale, reactor
capacity, and offsets. Every runtime and AI calculation must read those values
from the same `(hull, resolved appearance, candidate drive)` context. Do not
mutate shared hull-template fields.

Fission-pulse graphical scale and x1-only behavior may be enforced by a shared
drive-architecture helper. That rule is independent of appearance selection
and can be consumed by both the minimum and hypothetical designers.

## Implementation sequence

1. Add a pure deterministic helper that resolves the vanilla-style human AI
   appearance from faction template and candidate drive.
2. Assign that appearance immediately after `SetDriveTemplate` in human
   candidate construction, before power-plant selection.
3. Cap `GetIdealPropellentTankCount` and recompute its `actualDV` output.
4. Preserve appearance before AI refit compatibility and tank calculations.
5. Make alien and fighter direct tank loops capacity-aware and terminating.
6. Add the central new-design save invariant.
7. Add guarded IL validation for every patched AI target.

## Acceptance cases

- Repeating the same human AI design inputs resolves the same appearance for
  every candidate drive.
- Every appearance-dependent reactor, mass, drive, and fuel calculation sees
  the locked appearance before it runs.
- A human candidate requesting more tanks than fit is scored using capped
  delta-v and can lose to a better drive.
- If no human drive reaches requested delta-v, the designer can return its best
  otherwise-valid lower-delta-v design rather than save an over-cap design.
- An alien design never increments beyond appearance 0's capacity and does not
  exhaust design passes solely because its requested delta-v exceeds the cap.
- An AI refit keeps the original appearance and is rejected when the legal cap
  cannot preserve the original delta-v.
- Fighter design terminates at its cap.
- No newly saved AI design has zero tanks or more than its computed maximum.
- Disabling the fuel-cap feature restores vanilla tank selection without
  changing the chosen deterministic appearance.
