# Luna Resource Yield Comparison and Working Revision

Status: research and unapproved Part 4 proposal. This document compares the
installed Terra Invicta 1.0.51 Luna and Mars profiles with a mass-grounded
thirty-site lunar proposal. Numeric proposal bands require user approval before
implementation.

## Units and generator behavior

Mining-profile `mean`, `width`, and `min` values describe monthly site output.
One displayed space-resource point is one decaton, or ten tonnes. Site output
is then multiplied by the active mining module and faction modifiers:

- Outpost Mining Complex: 1.0x
- Automated Mining Complex: 1.25x
- Settlement Mining Complex: 1.5x
- Colony Mining Complex: 2.0x

All comparisons below are base site output before those multipliers.

Vanilla does not use bounded ranges. A no-jump roll begins near
`mean +/- width / 2`, subject to a randomized minimum. Each successful jump
moves the center by another full width, with a 50% chance of moving downward.
This produces unbounded high tails and can also produce a positive result from
a zero mean. The `ordinary roll` tables approximate the no-jump band; `j`
shows the probability of continuing the geometric jump loop.

The proposed profiles instead use `jump = 0` and encode explicit low/high
bounds as `mean = (low + high) / 2`, `width = high - low`, and `min = 0`.
Leaving `min` at zero avoids the generator's separate 0.8-1.2 minimum
randomization while the positive mean or width keeps the resource active. An
absent resource sets mean, width, minimum, and jump to zero.

To increase campaign-to-campaign variation without changing the accepted
expected resource ratios, each positive twenty-site draft band `[low, high]`
was widened to:

`newLow = low / 4`

`newHigh = low + high - newLow`

The midpoint is therefore exactly unchanged. The ten added sites use the same
wide-band rule around geology-matched midpoint yields. This provides bounded
extremes without reintroducing vanilla's unbounded jump tails.

## Vanilla Luna

Luna has nine sites using five profiles. Five ordinary sites share
`LunarMine`; Peary and Shackleton use north/south polar profiles; Copernicus
and Mare Tranquillitatis have bespoke profiles.

| Vanilla profile | Sites | Water | Volatiles | Metals | Noble | Fissiles |
|---|---:|---:|---:|---:|---:|---:|
| LunarMine | 5 | none | 0-2.5, j0 | 4-17.5, j0.3 | none | 0-0.5, j0.5 |
| LunarNorthPolarMine | 1 | 5-15, j0.15 | 0-4, j0 | 5-15, j0.2 | none | 0-0.375, j0.4 |
| LunarSouthPolarMine | 1 | 5-15, j0.2 | 0-4, j0 | 5-15, j0.2 | none | 0-0.375, j0.4 |
| LunarCopernicusMine | 1 | none | 0-2.5, j0 | 10-20, j0.3 | none | 0.08-0.85, j0.4 |
| LunarTranquilityMine | 1 | none | 0-2.5, j0 | 8-15, j0.3 | 0.8-5, j0.2 | 0-0.5, j0.4 |

Consequences:

- Every nonpolar lunar profile with `volatiles_width > 0` can roll Volatiles
  despite having a zero mean.
- Generic lunar sites can roll several tonnes of fissile material per month,
  and jump tails can go much higher, despite bulk lunar U/Th being ppm-scale.
- Mare Tranquillitatis is the only vanilla lunar site with Noble Metals. Its
  output is defensible only because the localization includes titanium in that
  category.

## Vanilla Mars

Mars has twenty-five sites: thirteen ordinary, three lowland, two volcanic,
four north-polar, and three south-polar sites.

| Vanilla profile | Sites | Water | Volatiles | Metals | Noble | Fissiles |
|---|---:|---:|---:|---:|---:|---:|
| MartianMine | 13 | 17.5-22.5, j0.1 | 15-25, j0.2 | 10-30, j0.4 | 0-5, j0.2 | 0-0.625, j0.3 |
| MartianLowMine | 3 | 7.5-12.5, j0.2 | 10-20, j0.2 | 22.5-37.5, j0.4 | 0.8-10, j0.1 | 0-0.5, j0.3 |
| MartianVolcanoMine | 2 | 7.5-12.5, j0.1 | 20-30, j0.2 | 32.5-47.5, j0.3 | 10-20, j0.2 | 0-0.5, j0.3 |
| MartianNorthPolarMine | 4 | 45-55, j0.2 | 15-25, j0.2 | 5-15, j0.2 | 0-2.5, j0.1 | 0.16-1.2, j0.3 |
| MartianSouthPolarMine | 3 | 45-55, j0.2 | 15-25, j0.2 | 5-15, j0.2 | 0-2.5, j0.1 | 0-0.5, j0.3 |

Vanilla Mars is therefore a deliberately generous gameplay benchmark. It is
much richer than vanilla Luna in Water and Volatiles, ordinary Martian metals
overlap the richest lunar metals, and volcanic Mars far exceeds any proposed
lunar site. The Mars Noble and Fissile profiles are not themselves a strict
geochemical mass model and should be used as a balance comparison rather than
scientific ground truth.

The broad ordering is nevertheless sensible. Mars has accessible atmospheric
CO2, widespread hydrated material and buried ice, and basaltic regolith rich in
iron, silicon, and aluminum. NASA reports greater than 20 wt% near-surface
water-equivalent ice in parts of the north polar region, while representative
Martian soil contains roughly 46.5 wt% SiO2, 10.5 wt% Al2O3, and combined iron
oxides around 16 wt%. See
[NASA's north-polar water map](https://science.nasa.gov/photojournal/north-polar-water-ice-by-weight)
and
[Martian dust and soil composition](https://ntrs.nasa.gov/api/citations/20170005414/downloads/20170005414.pdf?attachment=true).

## Mass-grounded lunar scale

Use 10,000-50,000 tonnes of excavated or selectively beneficiated feedstock per
month as a plausibility envelope for one base site. This is not a new gameplay
stat and need not be exposed to the player. It is an accounting check that
prevents unlike resources from being assigned impossible mass ratios.

| Resource | Representative lunar occurrence | Consequence for modeled output |
|---|---|---|
| Water | Effectively absent from ordinary sunlit regolith; roughly 3-10 wt% in the LCROSS Cabeus impact material, but heterogeneous | Only evidence-supported polar cold traps produce Water. Cabeus retains a 14-point expected yield but may roll from 2.5 to 25.5, still below vanilla Mars polar output. |
| Volatiles | Ordinary C about 100 ppm and N about 80 ppm; sulfur is higher in some basalts but diffuse. Cabeus cold-trap ice includes CO, CO2, NH3, H2S, SO2, methanol, and methane in addition to water | Generic sites are below the economic cutoff. Polar non-water output should usually be about one-tenth to one-third of Water. Pyroclastic output remains below 0.5 point. |
| Base Metals | Representative soil includes about 21% Si, 13% Fe, and 7% Al; highlands exchange iron for more aluminum/calcium while maria are iron-rich | Useful feed is abundant, but separation and refining are limiting. Expected site yields remain 6-20 points, while the widened campaign rolls span 1-36 points. |
| Noble Metals | True Au/Ag/PGE abundances are trace. High-Ti mare basalt contains 10-14 wt% TiO2, equivalent to roughly 6-8 wt% elemental Ti | Nonzero lunar Noble output is chiefly titanium. A high-Ti expected yield of 3.5 points may roll from 0.5 to 6.5; very-low-Ti sites remain much poorer. |
| Fissiles | Representative soil is about 1 ppm Th; KREEP samples contain about 9-16 ppm Th and 2.5-4.1 ppm U; Compton-Belkovich is about 14-26 ppm Th | Background production rounds to zero. Even exceptional sites should generally produce hundredths, not whole points: tens to hundreds of kilograms per month rather than tens of tonnes. |

Sources:

- [NASA representative lunar-soil composition](https://ntrs.nasa.gov/api/citations/20080003835/downloads/20080003835.pdf)
- [NASA mare/highland composition](https://ntrs.nasa.gov/api/citations/20100017257/downloads/20100017257.pdf)
- [High-Ti mare basalt classification](https://ntrs.nasa.gov/citations/19940019897)
- [LCROSS Cabeus water estimate](https://ntrs.nasa.gov/citations/20110012430)
- [Apollo 14 KREEP U/Th measurements](https://www.nasa.gov/wp-content/uploads/static/history/alsj/a14/as14psr.pdf)
- [Compton-Belkovich thorium reconstruction](https://agupubs.onlinelibrary.wiley.com/doi/full/10.1002/2014JE004719)

One resource point is ten tonnes. Thus:

- `5.0` Noble Metals means 50 tonnes per month. This is plausible only where
  titanium dominates the category; it is not a claim of 50 tonnes of gold or
  platinum.
- `0.01` Fissiles means 0.1 tonne, or 100 kg, per month.
- `0.095` Fissiles means 0.95 tonne per month and already requires a very rich,
  selectively mined anomaly or throughput near the top of the accounting
  envelope.
- A vanilla `0.5`-point lunar fissile roll means five tonnes per month and is
  inconsistent with a common-throughput mass comparison.

## Working thirty-site proposal

These are bounded base monthly resource points before mine multipliers. A dash
means guaranteed zero. This table supersedes the first draft for discussion but
is not yet approved for implementation.

| Site | Geological role | Water | Volatiles | Metals | Noble | Fissiles |
|---|---|---:|---:|---:|---:|---:|
| Mare Imbrium | Low-Ti mare and Procellarum/KREEP influence | - | - | 3-27 | 0.0625-1.1875 | 0.00125-0.02875 |
| Peary Crater | Sparse north-polar ice | 0.25-4.75 | 0.05-1.15 | 1.5-14.5 | - | - |
| D'Alembert Crater | Feldspathic farside highlands | - | - | 1.75-17.25 | - | - |
| Copernicus Crater | Excavated Imbrium/KREEP material | - | - | 2.75-26.25 | 0.025-0.575 | 0.00125-0.02875 |
| Mare Tranquillitatis | Best-established high-Ti mare | - | - | 3.5-32.5 | 0.5-6.5 | - |
| Korolev Crater | Farside highlands/basin material | - | - | 1.75-17.25 | - | - |
| Tycho Crater | Young feldspathic highland ejecta | - | - | 2-19 | - | - |
| Shackleton Crater | South-polar cold trap, less constrained than Cabeus | 0.75-10.25 | 0.2-3.1 | 1.25-12.75 | - | - |
| Tsiolkovskiy Crater | Farside mare basalt floor | - | - | 2.75-26.25 | 0.0625-1.1875 | - |
| Cabeus Crater | LCROSS-confirmed rich cold-trap material | 2.5-25.5 | 0.5-7.5 | 1-11 | - | - |
| Haworth Crater | Evidence-supported south-polar ice | 1.25-15.75 | 0.25-4.75 | 1-11 | - | - |
| Shoemaker Crater | Evidence-supported south-polar ice | 1-13 | 0.25-3.75 | 1-11 | - | - |
| Aristarchus Plateau | Fe/Ti-rich pyroclastic deposit and KREEP vicinity | - | 0.025-0.575 | 4-36 | 0.25-3.75 | 0.00125-0.03375 |
| Oceanus Procellarum | Mare basalt in the Procellarum KREEP Terrane | - | - | 3.5-32.5 | 0.125-2.375 | 0.00125-0.03375 |
| Mare Serenitatis | High-Ti mare basalt | - | - | 3.5-32.5 | 0.375-5.125 | 0.00025-0.01075 |
| Mare Crisium | Very-low-Ti mare basalt | - | - | 3-28 | 0.025-0.575 | - |
| Marius Hills | Volcanic and pyroclastic province | - | 0.0125-0.2875 | 3.5-32.5 | 0.0625-1.1875 | 0.00075-0.02225 |
| South Pole-Aitken Basin | Mafic lower-crust/basin material with local Th | - | - | 2.5-24.5 | 0.025-0.575 | 0.00025-0.01075 |
| Schrödinger Basin | Mafic basin and localized pyroclastic material | - | 0.0125-0.2875 | 3-29 | 0.0625-1.1875 | 0.00025-0.01075 |
| Compton-Belkovich Volcanic Complex | Silicic thorium anomaly | - | - | 2-20 | 0.0125-0.2875 | 0.005-0.095 |
| Faustini Crater | LRO-supported south-polar ice deposit | 1.25-15.75 | 0.25-4.75 | 1-11 | - | - |
| Orientale Basin | Young multi-ring basin and mixed ejecta | - | - | 2.25-22.75 | 0.025-0.575 | - |
| Mare Moscoviense | Largest farside mare deposit | - | - | 3-29 | 0.125-2.375 | - |
| Gruithuisen Domes | Silicic volcanic domes | - | - | 2-20 | 0.0125-0.2875 | 0.00075-0.02225 |
| Mons Rümker | Broad volcanic-dome complex | - | 0.0125-0.2875 | 3.5-32.5 | 0.0625-1.1875 | 0.00075-0.02225 |
| Hadley-Apennine | Apollo 15 mare/highland/KREEP boundary | - | - | 2.75-27.25 | 0.025-0.675 | 0.00075-0.02225 |
| Taurus-Littrow | Apollo 17 high-Ti pyroclastic deposit | - | 0.025-0.575 | 4-36 | 0.5-6.5 | 0.00025-0.01075 |
| Kepler Crater | Procellarum impact excavation | - | - | 2.75-26.25 | 0.025-0.575 | 0.00125-0.02875 |
| Rima Bode | Regional Fe/Ti-rich pyroclastic deposit | - | 0.0125-0.2875 | 3.5-32.5 | 0.125-2.375 | 0.00075-0.02225 |
| Reiner Gamma | High-priority lunar swirl over mare material | - | - | 3-28 | 0.025-0.575 | - |

The ten-site expansion deliberately adds only one new Water site. LRO studies
specifically identify Faustini among the south-polar craters with evidence or
strong potential for ice; adding unrelated polar landmarks merely to preserve
a numerical site ratio would violate the approved geography rule. See
[NASA's 2024 LRO ice summary](https://science.nasa.gov/solar-system/moon/nasas-lro-lunar-ice-deposits-are-widespread/).

The added dry sites expand the represented geology rather than adding generic
duplicates. Gruithuisen represents uncommon silicic volcanism; Mare
Moscoviense adds the largest farside mare; Taurus-Littrow and Rima Bode add
well-characterized regional pyroclastic deposits; and Reiner Gamma is a major
exploration target whose resource profile remains that of its underlying mare
material rather than treating the magnetic swirl as an ore body. Sources:
[NASA on the Gruithuisen Domes](https://science.nasa.gov/resource/a-lunar-mystery-the-gruithuisen-domes/),
[NASA/NTRS pyroclastic resource assessment](https://ntrs.nasa.gov/citations/19930008051),
[NASA on Reiner Gamma](https://science.nasa.gov/photojournal/reiner-gamma/), and
[NASA's Moscoviense overview](https://www.nasa.gov/wp-content/uploads/2025/12/moon-image-cards.pdf?emrc=9efece).

## Comparison summary

Summing the ordinary no-jump bands provides a useful whole-body comparison.
Because vanilla profiles have jump tails, their realized totals can exceed the
listed upper values. Proposal totals are strictly bounded.

| Body/profile set | Sites | Water total | Volatiles total | Metals total | Noble total | Fissiles total |
|---|---:|---:|---:|---:|---:|---:|
| Vanilla Luna, ordinary bands | 9 | 10-30 | 0-25.5 | 48-152.5 | 0.8-5 | 0.08-4.6 |
| Working Luna proposal | 30 | 7-85 | 1.6-27.3 | 76-726 | 2.5125-39.6375 | 0.016-0.403 |
| Vanilla Mars, ordinary bands | 25 | 580-740 | 370-620 | 297.5-702.5 | 22.4-152.5 | 0.64-16.925 |

The proposal's midpoint lunar metal output is about four times vanilla Luna's
ordinary-band midpoint. Most of that is the increase from nine to thirty sites:
mean metal output per site rises from about 11.1 to 13.4 points. Lunar Water
rises because six rather than two named polar sites can contain ice, but average
output per icy site falls. Proposed aggregate Volatiles remain close to vanilla
Luna's ordinary midpoint despite the larger site count. Aggregate Fissiles fall
by roughly 91% versus vanilla's ordinary midpoint.

Relative to vanilla Luna, the working proposal:

- increases the site count from 9 to 30;
- keeps ordinary lunar Water at zero and restricts it to six polar sites;
- removes accidental generic Volatiles and permits only polar or
  evidence-supported pyroclastic output;
- keeps most lunar Metals inside vanilla's broad range while making geological
  provinces distinct;
- adds several titanium-bearing Noble sites but keeps even the best at or below
  vanilla Tranquillitatis's ordinary upper band;
- reduces lunar Fissiles by roughly one to two orders of magnitude and confines
  them to mapped KREEP or thorium-related provinces.

Relative to vanilla Mars:

- no lunar site approaches Mars's ordinary Water or Volatiles, except that
  Cabeus overlaps the bottom of ordinary Martian Water;
- the richest lunar metal sites overlap ordinary Mars but remain below Martian
  lowland and volcanic profiles;
- the best high-Ti lunar Noble sites remain below Martian volcanic output;
- lunar Fissiles become much smaller than vanilla Mars because this proposal
  applies an explicit mass-abundance constraint that vanilla Mars does not.

## Questions remaining before approval

1. Is an approximately fourfold increase in aggregate expected lunar metal
   output acceptable as the consequence of expanding from nine to thirty sites?
2. Are sub-`0.01` fissile values acceptable for the interface and AI, assuming
   automatic tests confirm they display, accumulate, and influence site
   valuation correctly?
