# Military Investment: 2022-2030 Sanity Check

Status: controlled calibration exercise for the implemented land-military
formulas, not a historical forecast.

This is a rough balance projection for the United States, China, and Russia
under the version 0.8 land-military formulas. Its purpose is to test whether a
plausible priority allocation again drives the largest powers to maximum armies
and the Military technology cap before 2030, as version 0.7 did.

The reproducible calculation is `tools/military-investment-simulator.js`.

## Interpreting priority weights as national effort

Investment Points are an abstraction of effort that can be redirected toward
national goals, not a claim that the state literally spends 100% of GDP. The
calibration assumption is that a completely allocated priority bar represents
about 50% of GDP: even a cohesive country under total-war mobilization cannot
continuously redirect its entire output.

This gives a useful first-order translation:

```text
Priority share = observed activity as share of GDP / 50%
```

For example, US gross capital formation was about one fifth of GDP around the
scenario start, so an Economy allocation near 40% is economically legible.
Health and social protection map mainly to Welfare; fossil-fuel extraction and
corruption/rent capture map partly to Spoils. These mappings are deliberately
approximate and overlap at the edges. They are calibration anchors, not a
national-accounts identity. Source for the capital-formation concept and data:
https://data.worldbank.org/indicator/NE.GDI.TOTL.ZS?locations=US

SIPRI reports 2022 military burdens of 3.5% of GDP for the United States, 1.6%
for China, and 4.1% for Russia. Against a 50%-of-GDP effort pool, those become
total military-effort shares of 7.0%, 3.2%, and 8.2%. Source:
https://www.sipri.org/sites/default/files/2023-04/2304_fs_milex_2022.pdf

Army and navy upkeep is already removed from monthly IP before priorities are
funded. The budget-consistent case therefore counts automatic upkeep toward the
military burden, then assigns the residual 80% to Military technology and 20%
to Build Army. The 80/20 division is a balance assumption: existing-force
modernization should dominate deliberate force expansion. A second, generous
case applies the full translated burden to priorities after upkeep, knowingly
double-counting upkeep. A third stress case doubles those already-generous
priority weights.

## Starting state and effective IP

The installed TI 1.0.49 templates supply GDP, population, Unrest, Military
technology, starting armies, strength, deployment, and regions. The current mod
then supplies monthly IP and upkeep. No adviser, occupation, conquest,
unification, direct investment, or further combat damage is assumed.

```text
Raw monthly IP = GDP / $100B × 1.05
Unrest penalty = max(0, Unrest - 2) / 10
Effective IP = Raw IP × (1 - Unrest penalty) - army/navy upkeep
```

| Nation | 2022 GDP | Unrest | Raw IP/mo | After unrest | Upkeep/mo | Effective IP/mo | Effective IP/yr | Tech | Armies / max |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| United States | $22.305T | 3 | 234.20 | 210.78 | 6.75 | 204.03 | 2,448 | 4.50 | 6 / 14 |
| China | $26.557T | 2 | 278.84 | 278.84 | 2.56 | 276.28 | 3,315 | 3.90 | 4 / 26 |
| Russia | $3.842T | 4 | 40.34 | 32.27 | 2.47 | 29.80 | 358 | 3.70 | 3 / 6 |

The vanilla army limit is the lesser of unoccupied non-colony regions and
`1 + floor(population / 25 million)`, after the 5-million first-army threshold.
That makes the United States and Russia population-limited at 14 and 6, while
China is region-limited at 26.

The upkeep estimate preserves the literal 2022 deployment: one US army and one
Russian army are away; the others are home. It also preserves vanilla's 0.5-IP
navy surcharge for six US, two Chinese, and one Russian naval-deployment armies.
Russia begins with two damaged armies missing 1.20 total strength. Their eventual
repair charge is about 15.60 IP. After its template's initial 40% Build Army
progress, Russia begins the projection at about -5.20 IP of repair debt.

At the starting force sizes, reaching tech 5 would already cost about 2,035 IP
for the United States, 3,151 for China, and 3,348 for Russia. Newly constructed
armies make the remaining upgrade path more expensive.

## Budget-consistent allocation

After crediting starting upkeep toward the observed military burden, the fixed
priority weights are:

| Nation | Military | Build Army | Combined discretionary | Automatic upkeep also counted |
|---|---:|---:|---:|---:|
| United States | 3.78% | 0.95% | 4.73% | Yes |
| China | 1.84% | 0.46% | 2.30% | Yes |
| Russia | 2.24% | 0.56% | 2.80% | Yes |

With 2022 GDP, Unrest, borders, deployment, and the tech-5 cap held fixed, the
annual path is:

| Year | US tech | US armies | China tech | China armies | Russia tech | Russia armies |
|---:|---:|---:|---:|---:|---:|---:|
| 2022 start | 4.500 | 6 | 3.900 | 4 | 3.700 | 3 |
| 2023 | 4.532 | 6 | 3.945 | 4 | 3.708 | 3 |
| 2024 | 4.563 | 7 | 3.988 | 5 | 3.715 | 3 |
| 2025 | 4.593 | 7 | 4.028 | 5 | 3.723 | 3 |
| 2026 | 4.621 | 8 | 4.067 | 6 | 3.731 | 3 |
| 2027 | 4.648 | 8 | 4.103 | 6 | 3.738 | 3 |
| 2028 | 4.673 | 9 | 4.138 | 7 | 3.745 | 3 |
| 2029 | 4.698 | 9 | 4.171 | 7 | 3.753 | 3 |
| 2030 | 4.722 | 9 | 4.202 | 7 | 3.760 | 3 |

From 2022 through 2030, this spends about 738/184 Military/Build IP in the
United States, 487/122 in China, and 64/16 in Russia. None reaches either cap.
Russia's Build allocation repays its starting repair debt but does not finish a
new army.

## Sensitivity and cap risk

| Case | United States, 2030 | China, 2030 | Russia, 2030 |
|---|---|---|---|
| Budget-consistent | tech 4.722, 9/14 armies | tech 4.202, 7/26 | tech 3.760, 3/6 |
| Budget-consistent with 2.6%/5.7%/4.2% annual GDP growth | tech 4.740, 10/14 | tech 4.256, 8/26 | tech 3.770, 3/6 |
| Full burden spent again through priorities | tech 4.806, 11/14 | tech 4.293, 9/26 | tech 3.864, 4/6 |
| Twice the full burden through priorities | tech 5.000, 14/14 | tech 4.544, 12/26 | tech 3.997, 5/6 |

Only the last, intentionally excessive case reproduces the old failure for one
country: the United States reaches its army limit in early 2028 and tech 5 in
mid-2029. China and Russia still reach neither cap. The normal and generous
cases remain well below the tech-5 cap, so a global unlock to 5.5 in the early
2030s does not affect their pre-2030 path.

The current formulas therefore look conservatively balanced for peacetime 2022
military burdens. The clearest remaining risk is not ordinary national spending
but a player maintaining roughly twice the already upkeep-double-counted US
military allocation for most of the decade. That is an appropriate path to a
late-decade cap, not the near-automatic early saturation seen in version 0.7.

## Boundaries of the estimate

- Priority weights are coordinated nationally; split faction control can reduce
  or redirect the modeled spending.
- GDP-growth sensitivity uses the existing economy simulator's 50%-Economy
  initial net rates as a deliberately generous constant-growth bound.
- Population, regions, Unrest, adviser bonuses, deployments, and the cap remain
  fixed. No wars, occupations, new navies, direct investment, or further repair
  bills occur.
- Build Army and Military investments are applied daily. Costs reprice after
  every fractional tech change and every completed army.
- The model shifts a blocked military allocation to the other military priority;
  once both caps are reached, additional military effort is unused.
