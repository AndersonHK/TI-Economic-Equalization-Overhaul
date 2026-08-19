# Military technology ceiling

Status: implementation authority for global increases to national maximum
Military technology.

Game-data baseline: Terra Invicta 1.0.51

## Interpretation and decision

Maximum Military technology is a ceiling on the average quality of a nation's
fielded joint force. It does not immediately modernize any nation or army. The
score covers equipment, materials, sensors, communications, electronic
warfare, unmanned systems, training, logistics, and doctrine rather than only
the newest aircraft or weapons available to a country.

EEO reduces the shared `Effect_IncreaseMaxArmyTechLevel` value from 0.5 to
0.25. This changes every technology using the native all-nations instant
effect, including repeatable Future Military Science. Quarter-point steps make
the ceiling progress more gradually and allow military relevance to be spread
across several technological domains.

The mod adds the shared effect to three technologies:

- `CarbonNanotubes`, representing high-strength nanotube structures and the
  material basis of Nanotube Armor;
- `Superalloys`, representing high-performance structural alloys and the
  material basis of Foamed Metal Armor; and
- `DeuteriumHelium3Fusion`, representing the energy, thermal-management, and
  high-performance systems foundation of mature fusion-era warfare.

Diamondoids already carries the native effect and continues to represent the
Adamantane Armor material step. Its bonus is reduced to 0.25 by the shared
effect override; it is not an additional recipient.

## Finite human ceiling

Human nations initialize with a maximum Military technology of 5. The twelve
non-repeatable global technologies carrying the quarter-point increase are:

1. Terrestrial Military Science;
2. Augmented Reality;
3. Cybernetics;
4. Superalloys;
5. Next-Generation Aerospace;
6. Carbon Nanotubes;
7. Diamondoids;
8. Applied Artificial Intelligence;
9. Networked Global Defense;
10. Trans-Interface Warfare;
11. Coilguns; and
12. Deuterium-Helium-3 Fusion.

The finite ceiling is therefore:

```text
5.00 + 12 x 0.25 = 8.00
```

Augmented Reality is completed in both the 2022 and 2026 starting scenarios,
so their opening maximum is 5.25. This changes only the ceiling; each nation's
actual Military technology retains its scenario value and must be raised
through national Military investment.

Future Military Science remains an end-game repeatable technology. Each
completion adds another 0.25, so four completions raise the human ceiling from
8.00 to 9.00. Its escalating authored costs are 100,000, 200,000, 300,000, and
400,000 research for those four completions before global research-speed
modifiers, a total of 1,000,000.

## Alien comparison

Regular alien armies do not use the human national ceiling. Their actual
combat technology is calculated dynamically from alien abductions:

```text
Alien army technology = min(9.00, 6.75 + 0.0002 x abductions)
```

| Abductions | Alien army technology |
|---:|---:|
| 0 | 6.75 |
| 1,250 | 7.00 |
| 2,500 | 7.25 |
| 3,750 | 7.50 |
| 5,000 | 7.75 |
| 6,250 | 8.00 |
| 8,750 | 8.50 |
| 11,250 | 9.00 |

The finite human ceiling therefore matches alien armies at 6,250 abductions
and remains one full technology level below mature alien armies. Humans can
close that gap only through four increasingly expensive Future Military
Science completions. The comparison is deliberately asymmetric: 8.00 is only
the human national maximum, while the alien value is automatically applied as
the army's current combat technology.

## Existing-save behavior

The maximum-Military-technology effect is instant rather than continuously
recalculated. Template changes control future completions and new campaigns but
do not retroactively replace increments already applied in an existing save.
Existing campaigns that completed affected technologies under the former 0.5
value can therefore retain a higher ceiling and are not the balance authority
for testing this change.

## Verification contract

Automated verification pins the installed 0.5 effect as the vanilla baseline,
requires the mod override to preserve its all-nations instant behavior while
setting its value to 0.25, and checks the exact twelve finite recipients. It
also verifies that Carbon Nanotubes and Superalloys retain their installed
costs and that Deuterium-Helium-3 Fusion retains both of its original permanent
effects alongside the new Military-technology increase.
