# Reactor progression adjustment

Status: implemented and deployed on 2026-08-18 for Terra Invicta 1.0.51,
including the display-rounding follow-up.

## Scope and interpretation

This pass changes power-plant capacity and specific mass only. Efficiencies,
crew, build materials, unlocks, and reactor-bay geometry are unchanged.

`specificPower_tGW` is the plant's specific mass in tonnes per gigawatt, so a
higher value makes every installation proportionally heavier. Installed mass
continues to be `max(1 t, gross required GW * specificPower_tGW)` and does not
default to the plant's full rated-output mass.

The requested `GasCoreFissionReactorIV` maximum of `1 TW` is implemented as
`maxOutput_GW: 1000`. Because the game's terawatt display clips finer
gigawatt precision, the follow-up rounds Gas Core V to `1.3 TW`
(`maxOutput_GW: 1300`) and Gas Core VI to `1.7 TW`
(`maxOutput_GW: 1700`).

## Implemented value changes

| Reactor | Field | Before | After | Delta |
|---|---|---:|---:|---:|
| Solid Core I | t/GW | 160 | 240 | +80 (+50%) |
| Solid Core II | t/GW | 136 | 204 | +68 (+50%) |
| Solid Core III | t/GW | 112 | 168 | +56 (+50%) |
| Solid Core IV | t/GW | 48 | 72 | +24 (+50%) |
| Solid Core V | t/GW | 32 | 48 | +16 (+50%) |
| Compact Solid Core I (`SolidCoreFissionReactorVI`) | t/GW | 24 | 36 | +12 (+50%) |
| Compact Solid Core II (`SolidCoreFissionReactorVII`) | t/GW | 20 | 30 | +10 (+50%) |
| Compact Solid Core III (`SolidCoreFissionReactorVIII`) | t/GW | 16 | 24 | +8 (+50%) |
| Compact Solid Core IV (`SolidCoreFissionReactorIX`) | t/GW | 12 | 18 | +6 (+50%) |
| Compact Solid Core V (`SolidCoreFissionReactorX`) | t/GW | 8 | 12 | +4 (+50%) |
| Molten Salt I | t/GW | 10 | 15 | +5 (+50%) |
| Molten Salt II | t/GW | 8 | 12 | +4 (+50%) |
| Vapor Core I | t/GW | 8 | 9 | +1 (+12.5%) |
| Vapor Core II | t/GW | 6 | 8 | +2 (+33.3%) |
| Vapor Core III | t/GW | 5 | 7 | +2 (+40%) |
| Gas Core IV | maximum output | 1,650 GW | 1,000 GW | -650 GW (-39.4%) |
| Gas Core V | maximum output | 1,650 GW | 1,300 GW | -350 GW (-21.2%) |
| Gas Core VI | t/GW | 4 | 5 | +1 (+25%) |
| Gas Core VI | maximum output | 1,650 GW | 1,700 GW | +50 GW (+3.0%) |

## Full-rating mass deltas

These figures illustrate the combined cap and specific-mass effects. They are
not a fixed mass charged to smaller installations.

| Reactor | Before at old cap | After at new cap | Delta |
|---|---:|---:|---:|
| Solid Core I | 320 t | 480 t | +160 t |
| Solid Core II | 408 t | 612 t | +204 t |
| Solid Core III | 1,120 t | 1,680 t | +560 t |
| Solid Core IV | 1,440 t | 2,160 t | +720 t |
| Solid Core V | 1,920 t | 2,880 t | +960 t |
| Compact Solid Core I | 18 t | 27 t | +9 t |
| Compact Solid Core II | 40 t | 60 t | +20 t |
| Compact Solid Core III | 64 t | 96 t | +32 t |
| Compact Solid Core IV | 72 t | 108 t | +36 t |
| Compact Solid Core V | 80 t | 120 t | +40 t |
| Molten Salt I | 400 t | 600 t | +200 t |
| Molten Salt II | 3,200 t | 4,800 t | +1,600 t |
| Vapor Core I | 52 t | 58.5 t | +6.5 t |
| Vapor Core II | 120 t | 160 t | +40 t |
| Vapor Core III | 300 t | 420 t | +120 t |
| Gas Core IV | 11,550 t at 1,650 GW | 7,000 t at 1,000 GW | -4,550 t |
| Gas Core V | 9,900 t at 1,650 GW | 7,800 t at 1,300 GW | -2,100 t |
| Gas Core VI | 6,600 t at 1,650 GW | 8,500 t at 1,700 GW | +1,900 t |

## Intended progression

- Both solid-core branches retain their existing output ceilings and their
  monotonic mass improvements, but every design pays 50% more plant mass.
- Molten Salt I to II remains a capacity jump from 40 to 400 GW while specific
  mass improves from 15 to 12 t/GW.
- Vapor Core I to III remains a compact 6.5/20/60 GW ladder, now improving
  cleanly from 9 to 8 to 7 t/GW.
- Gas Core I to VI keeps its 20/16/10/7/6/5 t/GW mass ladder. The top-end
  capacity is no longer flat: Gas Core IV, V, and VI now progress through
  1,000, 1,300, and 1,700 GW. At a shared 1,000 GW load they weigh 7,000,
  6,000, and 5,000 tonnes respectively.

## Verification record

The final display-rounded deployment pipeline passed:

- 1,078 patch-formula assertions;
- all guarded TI 1.0.51 IL and Harmony checks;
- the settled ship-rebalance override checks, including the new values and
  full-rating mass products;
- release packaging and the 44-file enabled-mod deployment.

The deployed DLL SHA-256 is
`B8A30A49F23C839DC9878B61CC62349AC1FDA6CA80B4476FD7E38550F1BFFD5D`.
The repository and deployed power-plant templates are byte-identical. Manual
Ship Designer confirmation that Gas Core IV/V/VI display as 1.0/1.3/1.7 TW
remains pending.
