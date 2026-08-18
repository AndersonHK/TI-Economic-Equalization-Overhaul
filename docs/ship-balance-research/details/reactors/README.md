# Reactor research

Last reviewed: 2026-08-18

## Current synthesis

No single tons-per-gigawatt number answers both reactor mass and maximum
reactor size.

- The [thermodynamics and fuel-inventory analysis](thermodynamic-and-fuel-limits.md)
  establishes a valuable lower bound. At 70% electrical efficiency, ideal
  U-235 alone costs about `0.55 t/GWe-year`; its `1 t/GWe` example is an
  intentionally extreme core-plus-converter floor with most real machinery
  excluded.
- The [structural scaling and output-cap analysis](structural-scaling-and-output-caps.md)
  asks how large one cooled and controlled unit can be. Solid fuel is usually
  limited first by conduction, coolant interfaces, pressure loss, material
  damage, and conversion machinery—not by the raw neutron fission rate.

The installed base-game `GasCoreFissionReactorVI` is `1 t/GW`, `96%`
efficient, and capped at `1,650 GWe`. The live rebalance override is now
`5 t/GW` and `94%`, with a display-safe `1,700 GWe` cap and an `8,500 t` plant
at full rating. This confirms that a lower-bound ratio cannot be used as a
late-game nerf by itself; containment, recovery, shielding, conversion,
cooling, endurance, and repeated trains remain separate constraints.

## Most useful balance model

Represent a plant as:

`mass = repeated-train fixed mass + linear power mass + endurance fuel + shielding`

Each core technology should also have a maximum output per integrated
reactor/loop/converter train. Larger ships may cluster trains but must pay the
duplicated fixed mass. This produces a meaningful scale penalty without
claiming that fission has a universal two-gigawatt physical ceiling.

The detailed structural report offers provisional train-cap bands for testing.
They are explicitly research hypotheses, not settled changelog values.
