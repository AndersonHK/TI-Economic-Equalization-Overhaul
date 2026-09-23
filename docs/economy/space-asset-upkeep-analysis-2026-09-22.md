# National space assets and IP upkeep: latest-save analysis

Research only. No mod, settings, or save edits; no build/deployment is needed for this analysis.

## Findings

Recurring asset upkeep offset by increased GDP-derived IP does produce the intended
redistribution in this save. The direct beneficiary is **low asset intensity relative
to productive GDP**, rather than small size itself. Large, lightly developed nations
can benefit too; small launch specialists can lose. With the moderate example below,
132 of 154 human nations gain and 21 lose; North Korea remains at zero IP. All 102 nations lacking a spaceflight program
gain. Their combined increase is approximately 5.236 IP/month (+4.84%), while the United States alone
gains 2.891 IP/month. This is a modest incentive correction, not a large reversal of
the scale advantage in fixed-cost projects.

## Scope and method

Plan: inspect the latest native save read-only; verify installed MC/IP formulas;
model recurring upkeep and compensating gross output; compare world, nation, and
Project Exodus outcomes; test a separate Boost growth cap; retain a full snapshot.

Source: `Exitsave.gz`, written September 22, 2026 at 18:31:54 local time. Save date:
**June 16, 2039, 12:00**, Project Exodus, Normal, 2026 scenario. The saved national
IP difficulty multiplier is 100%. There are 154 extant human nations, no extant alien
nation, no regional occupation, and no advising councilors recorded at this moment.

The analysis uses current installed investment settings (GDP divisor $100B,
low-income range 0.70–1.00 through $15,000 GDP/capita, output multiplier 1.05).
It keeps military deductions and all national assets unchanged. MC means installed
national MC, Boost means raw national monthly production, and Funding means raw
national monthly Funding before federation sharing or CP/faction bonuses. Faction
stockpiles, organizations, hab incomes, spare-MC income, and MC use are not charged.
The owned CP share is used for the Exodus comparison; this is not a forecast of
priority efficiency, faction income, or future borders.

## Current MC cap

For each region, with GDP in **billions** and national Education E:

```text
D = max(200, 300 - 6 E)
structural regional cap = 1 + floor(regional GDP / D)
actual regional cap = max(existing regional MC, structural regional cap)
national cap = sum(actual regional caps in regions not fully occupied)
```

These are the installed game's `TIRegionState.maxMissionControl` and
`TINationState.maxMissionControl`; EEO does not replace them. Education 10 gives
D = 240, Education 15 gives D = 210, and D reaches its floor at Education 16.667.
A single $1T region with Education 10 has 5 MC capacity; a $100B region has 1.
This is **not** a single national GDP calculation: per-region +1 and rounding matter.
Regional GDP uses population weights, multiplied by 1.25 for a core economic region,
1.25 for a resource **or** oil region (once), and 0.5 for a colony. Weights normalize
over all regions in the nation.

Existing capacity is protected by the max: GDP decline does not automatically
demolish MC. The save has 976 installed MC, 1,135 current cap, and a 1,107 cap if
calculated without the existing-MC floor. Five nations account for the 28 protected
slots: India 17, EU 5, China 4, Russia 1, Vietnam 1. These are excesses summed
region by region; a nation can have both protected slots and headroom elsewhere.
41 nations are at their current cap, including 37 with a spaceflight program.

Funding already has a GDP cap: annual Funding <= 0.005 × GDP / 1,000,000,
or **5 × GDP in billions per year**. Thus $1T GDP supports up to $5,000/year
Funding. Unlike the MC getter, its change method clamps the stock to that cap.
Boost currently has no analogous GDP capacity cap.

## Upkeep and compensation model

Interpret the proposed IP cost as recurring monthly upkeep, not an increase in
the existing one-time construction cost. Current monthly gross IP is:

```text
G = 1.05 × (GDP / $100B) × [0.70 + 0.30 × clamp(GDP per capita / $15,000, 0, 1)]
S = G × (1 + adviser bonus) × (1 - occupation penalty) × (1 - Unrest penalty)
Unrest penalty = max(Unrest - 2, 0) / 10
existing available IP = max(0, S - army upkeep - navy upkeep)
U = a × installed MC + b × raw Boost/month + c × raw Funding/month
new available IP = max(0, pre-floor available IP + r × S - U)
```

The last line is anchored to serialized available IP for positive-IP nations,
preserving their current cached deductions. North Korea is already at zero: its
pre-floor balance is reconstructed as 0.157 gross IP minus 1.080 army upkeep,
and remains negative in every scenario. Compensation is solved after this floor;
for this save it equals `sum(U) / sum(S)` over the 153 positive-IP nations.
It is an increase to **gross GDP-derived output**, not a
percentage increase applied after military upkeep. Retain the current low-income
modifier. Charge upkeep after Unrest/occupation effects and army/navy deductions;
otherwise civil disorder would partially erase the cost of maintaining assets.

World gross IP is 2,291.512/month; gross after Unrest is 2,261.888; saved available
IP is 2,071.914. For the moderate case, upkeep costs 128.252/month: MC 48.800,
Boost 61.310, Funding 18.143. A **5.6705% relative increase** to gross output offsets
the resulting available-IP loss exactly. Set `outputMultiplier` from 1.05 to **1.1095406** (approximately 1.11),
or equivalently use a GDP divisor of about $94.634B while keeping 1.05. These are
alternative ways to implement the same output increase; do not apply both.

### Illustrative choices

Rates below are IP/month per one MC, per one Boost/month of production, and per
one dollar/month of national Funding. All choices independently preserve world
available IP at 2,071.914/month in this snapshot.

| Option | MC rate | Boost rate | Funding rate | Gross increase | New output multiplier | Exodus change/month |
| --- | --- | --- | --- | --- | --- | --- |
| Light | 0.025 | 0.050 | 0.0005 | 2.835% | 1.079770 | -2.391 |
| Moderate | 0.050 | 0.100 | 0.0010 | 5.671% | 1.109541 | -4.783 |
| Strong | 0.100 | 0.200 | 0.0020 | 11.341% | 1.169081 | -9.565 |
| Funding Weighted | 0.050 | 0.100 | 0.0030 | 7.275% | 1.126386 | -12.733 |
| Funding Heavy | 0.050 | 0.100 | 0.0050 | 8.879% | 1.143231 | -20.683 |

Moderate upkeep means 1 IP/month per 20 MC, per 10 monthly Boost, or per $1,000
monthly Funding. The funding-weighted alternative retains the moderate MC/Boost
rates but charges 3 IP/month per $1,000 monthly Funding.

India and China produce 65.88% of all national Funding in this save. India produces
$7,737.50/month, or 48.58% of its current GDP-based Funding cap; China produces
$4,215.44/month, or 24.87% of its cap. Tripling Funding upkeep therefore targets
the concentrated cash-producing nations more strongly. Funding stock measures
current extraction, not historical Spoils use or proof that a nation was exploited.
A historically looted nation with few surviving assets can still gain in this model.

### Selected national effects

All IP figures are monthly; percentage changes compare against saved investable IP.

| Nation | Current IP | MC | Boost/mo | Funding/mo | Moderate change | Moderate % | Funding-weighted change |
| --- | --- | --- | --- | --- | --- | --- | --- |
| United States of America | 332.966 | 156 | 72.14 | 2002.97 | +2.891 | +0.87% | +4.518 |
| India | 373.092 | 178 | 113.44 | 7737.50 | -5.223 | -1.40% | -14.259 |
| China | 378.043 | 178 | 141.53 | 4215.44 | -3.051 | -0.81% | -4.630 |
| European Union | 86.745 | 59 | 40.41 | 1055.24 | -2.079 | -2.40% | -2.501 |
| Russia | 77.345 | 49 | 17.33 | 59.84 | +0.549 | +0.71% | +1.785 |
| Algeria | 18.020 | 8 | 0.01 | 0.33 | +0.645 | +3.58% | +0.941 |
| Nigeria | 15.497 | 4 | 3.87 | 1.27 | +0.290 | +1.87% | +0.536 |
| Colombia | 5.879 | 0 | 0.14 | 10.44 | +0.309 | +5.25% | +0.382 |
| Kenya | 3.193 | 0 | 0.00 | 0.00 | +0.181 | +5.67% | +0.232 |
| Bhutan | 0.179 | 0 | 0.00 | 0.00 | +0.010 | +5.67% | +0.013 |
| Australia | 21.732 | 13 | 18.28 | 55.88 | -1.193 | -5.49% | -0.926 |
| Singapore | 9.424 | 4 | 8.28 | 25.08 | -0.518 | -5.50% | -0.417 |
| New Zealand | 2.901 | 2 | 2.32 | 0.17 | -0.168 | -5.79% | -0.122 |

The moderate case gives countries below $100B GDP +5.49% aggregate available IP,
those from $100B to $1T +2.09%, and those above $1T -0.28%. The United States
is the largest absolute winner because its asset burden is below the global
weighted average. New Zealand, Singapore, and Australia lose about 5.5–5.8%.
Thus there is no guarantee that a smaller country benefits.

Exodus holds all CPs in India, Russia, Myanmar, Sri Lanka, and Bhutan, plus one
of China's six CPs. That represents 520.160 monthly available IP. The moderate
case lowers it to **515.378 (-0.92%)**; the funding-weighted case lowers it to
**507.427 (-2.45%)**. India's losses dominate; Russia and the smaller holdings gain.

## A similar Boost cap

A national GDP/Education cap is preferable to directly copying MC's regional
integer floor and free slot. Launch facilities may be concentrated in a small
equatorial region while supported by the entire country's economy.

```text
structural Boost capacity/month = k × national GDP in billions / max(200, 300 - 6 E)
```

For example, k = 1 gives 4.167 monthly Boost per $1T GDP at Education 10.
This borrows MC's economic scaling, not a physical launch-capacity estimate.
The existing latitude-sensitive construction gains and launch routing already
represent launch geography. A second latitude multiplier needs separate calibration.

Do not copy the per-region +1 bonus or integer rounding into Boost. At unchanged
Education this proposed cap is additive in GDP, with no free capacity generated by
splitting a nation. Education changes on unification can still alter the cap.

| k | World structural capacity/mo | Nations above cap | Production above cap/mo |
| --- | --- | --- | --- |
| 0.5 | 443.499 | 33 | 217.657 |
| 1 | 886.998 | 13 | 30.073 |
| 2 | 1773.997 | 2 | 0.604 |

At k = 1, the affected nations are:

| Nation | Existing Boost/mo | Structural cap/mo |
| --- | --- | --- |
| Australia | 18.278 | 9.321 |
| Japan | 27.758 | 22.007 |
| Singapore | 8.275 | 3.838 |
| Argentina | 5.990 | 3.893 |
| Israel | 4.113 | 2.215 |
| Switzerland | 5.417 | 3.925 |
| New Zealand | 2.323 | 1.159 |
| Chile | 2.755 | 1.788 |
| Malaysia | 7.508 | 6.566 |
| United Kingdom | 18.080 | 17.170 |
| Egypt | 8.058 | 7.337 |
| Vietnam | 5.288 | 4.845 |
| Austria | 3.058 | 2.764 |

China remains below the k = 1 cap (141.535 / 160.112), India below it
(113.438 / 147.697), and the EU almost exactly at it (40.406 / 40.528).
This cap targets unusually concentrated launch economies more than large powers.

Preserve existing Boost, as MC preserves existing facilities. If current production
is at/above the structural cap, block new Boost construction until GDP/Education
catches up; do not cut existing output. The 30.073 Boost/month above the k = 1
cap (4.91% of current production) is therefore **not removed** in these estimates.
If a destructive clamp were chosen instead, upkeep and the compensation would
need recalibration. The k = 0.5 option is very restrictive (33 nations already
above cap), while k = 2 barely constrains the current save (2 nations).

## Design assessment and suggested first calibration

- Start with the moderate rates and approximately **1.11 outputMultiplier** if the
  objective is a small ongoing opportunity cost. The exact snapshot-neutral value
  is 1.1095406. Use the funding-weighted variant and 1.1263858 if reducing the
  attraction of heavily developed Funding sources is a central aim.
- Pair either with a **k = 1 Boost growth cap** and preservation of current assets
  as the first candidate to test. This limits future specialization without erasing
  a player's past investment. Upkeep and the cap have different jobs: the former
  reduces available IP immediately; the latter only constrains future construction.
- Apply costs to physical national stocks before federation redistribution, including
  unused MC. Charging only used MC would allow banking free capacity or shifting
  burdens between factions. Charging faction Boost stockpiles would tax saving
  rather than installed launch production. Do not charge hab/organization output
  to unrelated national budgets.
- Keep rates linear and avoid per-nation allowances, exemptions, or minimum fees.
  Those would introduce incentives to split or merge countries. The unchanged
  low-income multiplier still disadvantages poor GDP: this proposal does not remove it.
- The modifier preserves the **world total now**, not every nation or every future
  year. As assets grow faster than GDP, net IP will fall relative to the current
  system; as GDP catches up, it recovers. Set a fixed calibrated modifier, rather
  than automatically refunding each new facility's upkeep through a moving global
  compensation factor. Automatic compensation would externalize expansion costs.
- Existing assets remain useful in exchange for upkeep. Rates must be assessed
  against alternative cash/Boost/MC sources and future large-scale operations,
  not only their present total. Funding is a continuing diversion of national effort;
  its cost is conceptually different from physical facility maintenance.
- A completed implementation needs transparent tooltip deductions, construction
  guards for normal/direct investment and spaceflight-program grants, compatible
  AI priority/cap handling, and explicit handling of the zero-IP floor. In an extreme
  overbuilt/collapsed nation, simply clamping to zero does not actually fund the
  unpaid upkeep; mothballing or a recovery rule is a separate design decision.

## Verification and limitations

The MC and national IP getters were freshly decompiled from the installed
`D:/Games/SteamLibrary/steamapps/common/Terra Invicta/TerraInvicta_Data/Managed/Assembly-CSharp.dll`.
The installed TIGlobalConfig JSON confirms regional GDP weights and the 0.5 navy
deduction. EEO's source and installed Settings.xml agree on the gross IP parameters.
Relevant methods are `TIRegionState.NationalGDPProportion`, `maxMissionControl`,
`TINationState.SetBaseInvestmentPoints_month`, `maxFunding_year`,
`ChangeAnnualSpaceFundingValue`, and `OnBoostPriorityComplete`.
Repository references: [gross IP patch](../../TIEconomyMod/Patches/NationalValuesPatches.cs),
[army upkeep patch](../../TIEconomyMod/Patches/MilitaryPatches.cs),
[design directives](../design-directives.md).

The save's serialized available-IP value is authoritative for the baseline. A
separate simple army-location reconstruction agrees within 0.058 IP/month for
153 nations; Ethiopia differs by 0.762 IP/month. The simple check omits
the army-in-battle home/away condition, whose size is consistent with that difference,
but its exact cause was not established. This cross-check is not substituted for
the saved baseline. Gross increases use current GDP, population, and Unrest;
minor cache/float discrepancies may appear after an actual daily game update.
This is an offline estimate, not a replay of the game's update loop.

All five models conserve world available IP to less than 1e-8 IP/month after
clamping. North Korea remains at zero; no additional nation falls to zero. No save mutation occurs; its SHA-256
was checked before and after analysis. No attempt was made to predict future
construction, military deployments, adviser changes, wars, or AI response.

Save SHA-256: `73887723214c7895749e1ce782d328fd6ae1bebc5e163072b872a795a556e501`.

Full inputs, region-level MC calculations, five scenario outputs, and cap comparisons: [JSON snapshot](space-asset-upkeep-snapshot-2039-06-16.json).

## All nations: moderate scenario

| Nation | GDP $B | Existing IP/mo | MC / cap | Boost/mo | Funding/mo | Net IP change/mo | Change % |
| --- | --- | --- | --- | --- | --- | --- | --- |
| China | 40674.66 | 378.0425 | 178 / 178 | 141.535 | 4215.445 | -3.0510 | -0.81% |
| India | 38223.57 | 373.0916 | 178 / 178 | 113.438 | 7737.503 | -5.2228 | -1.40% |
| United States of America | 33435.93 | 332.9657 | 156 / 156 | 72.135 | 2002.971 | +2.8915 | +0.87% |
| European Union | 10021.45 | 86.7452 | 59 / 59 | 40.406 | 1055.239 | -2.0790 | -2.40% |
| Russia | 8047.55 | 77.3452 | 49 / 49 | 17.331 | 59.839 | +0.5486 | +0.71% |
| Indonesia | 5589.38 | 47.6594 | 26 / 26 | 11.008 | 78.000 | +0.8491 | +1.78% |
| Japan | 5443.54 | 47.5148 | 25 / 25 | 27.758 | 334.332 | -1.1190 | -2.36% |
| Brazil | 4657.61 | 44.4511 | 22 / 23 | 14.342 | 7.583 | +0.2314 | +0.52% |
| Germany | 4619.17 | 41.9076 | 21 / 21 | 13.538 | 549.348 | -0.2029 | -0.48% |
| United Kingdom | 4115.73 | 39.0349 | 21 / 21 | 18.080 | 122.301 | -0.5298 | -1.36% |
| Canada | 4019.35 | 39.1559 | 21 / 21 | 6.737 | 171.880 | +0.4976 | +1.27% |
| Türkiye | 3970.98 | 38.7660 | 18 / 18 | 4.468 | 106.167 | +0.9113 | +2.35% |
| Iran | 3101.16 | 29.9686 | 16 / 16 | 6.547 | 21.321 | +0.3704 | +1.24% |
| Italy | 3098.70 | 28.5669 | 14 / 14 | 11.267 | 380.423 | -0.3621 | -1.27% |
| Saudi Arabia | 2979.02 | 28.3112 | 14 / 14 | 6.702 | 111.583 | +0.2920 | +1.03% |
| South Korea | 2940.89 | 25.8772 | 12 / 12 | 8.348 | 462.167 | -0.1459 | -0.56% |
| Spain | 2454.03 | 23.8621 | 12 / 12 | 1.260 | 146.383 | +0.5888 | +2.47% |
| Australia | 2250.97 | 21.7318 | 13 / 13 | 18.278 | 55.884 | -1.1935 | -5.49% |
| Mexico | 1985.88 | 16.9913 | 10 / 10 | 5.417 | 48.446 | -0.0069 | -0.04% |
| Egypt | 1892.67 | 17.6881 | 9 / 9 | 8.058 | 177.767 | -0.3317 | -1.88% |
| Nigeria | 1885.58 | 15.4973 | 4 / 9 | 3.872 | 1.274 | +0.2903 | +1.87% |
| Algeria | 1757.41 | 18.0196 | 8 / 8 | 0.008 | 0.333 | +0.6452 | +3.58% |
| Malaysia | 1630.79 | 16.7365 | 8 / 8 | 7.508 | 19.667 | -0.1995 | -1.19% |
| Thailand | 1509.59 | 15.4505 | 7 / 7 | 3.365 | 2.086 | +0.2102 | +1.36% |
| Vietnam | 1240.38 | 12.0177 | 7 / 7 | 5.288 | 14.463 | -0.2118 | -1.76% |
| Philippines | 1135.17 | 6.7693 | 1 / 6 | 0.895 | 0.082 | +0.2443 | +3.61% |
| Iraq | 1120.01 | 11.4137 | 5 / 5 | 0.356 | 0.000 | +0.3812 | +3.34% |
| Norway | 1022.09 | 9.3958 | 5 / 5 | 0.995 | 51.250 | +0.2078 | +2.21% |
| Ireland | 1017.00 | 10.6740 | 5 / 5 | 0.878 | 15.333 | +0.2524 | +2.36% |
| Switzerland | 957.11 | 10.0447 | 4 / 4 | 5.417 | 1.083 | -0.1729 | -1.72% |
| Argentina | 947.83 | 9.9522 | 4 / 5 | 5.990 | 5.399 | -0.2400 | -2.41% |
| Singapore | 897.51 | 9.4239 | 4 / 4 | 8.275 | 25.083 | -0.5182 | -5.50% |
| Romania | 843.75 | 8.8520 | 5 / 5 | 1.695 | 5.570 | +0.0773 | +0.87% |
| United Arab Emirates | 826.18 | 8.6749 | 4 / 4 | 2.358 | 79.417 | -0.0233 | -0.27% |
| Colombia | 778.86 | 5.8795 | 0 / 4 | 0.142 | 10.444 | +0.3088 | +5.25% |
| Czechia | 739.67 | 7.7665 | 4 / 4 | 0.867 | 4.500 | +0.1492 | +1.92% |
| Uzbekistan | 690.77 | 7.2530 | 1 / 4 | 0.008 | 0.000 | +0.3605 | +4.97% |
| Austria | 669.92 | 7.0342 | 3 / 3 | 3.058 | 1.635 | -0.0586 | -0.83% |
| Denmark | 633.07 | 6.6472 | 3 / 4 | 0.615 | 0.167 | +0.1653 | +2.49% |
| Ethiopia | 575.24 | 3.4373 | 0 / 5 | 0.008 | 0.000 | +0.2559 | +7.45% |
| Finland | 557.96 | 5.8533 | 3 / 3 | 0.240 | 0.167 | +0.1580 | +2.70% |
| Israel | 551.37 | 5.7894 | 1 / 3 | 4.113 | 1.386 | -0.1344 | -2.32% |
| Peru | 514.40 | 4.1162 | 1 / 3 | 0.770 | 1.327 | +0.1050 | +2.55% |
| South Africa | 513.60 | 1.7666 | 0 / 3 | 0.000 | 9.452 | +0.0907 | +5.14% |
| Hungary | 513.50 | 5.3918 | 3 / 3 | 0.330 | 0.000 | +0.1227 | +2.28% |
| Kenya | 494.45 | 3.1928 | 0 / 3 | 0.000 | 0.000 | +0.1810 | +5.67% |
| Angola | 493.23 | 2.7295 | 0 / 3 | 0.000 | 0.166 | +0.1546 | +5.66% |
| Morocco | 448.01 | 2.9658 | 0 / 3 | 0.000 | 0.333 | +0.1678 | +5.66% |
| Myanmar | 445.01 | 3.5589 | 0 / 2 | 0.000 | 0.000 | +0.2211 | +6.21% |
| Ecuador | 440.41 | 3.6647 | 0 / 2 | 0.000 | 0.000 | +0.2078 | +5.67% |
| Chile | 439.14 | 4.6110 | 1 / 3 | 2.755 | 2.122 | -0.0661 | -1.43% |
| Qatar-Bahrain | 427.18 | 4.4854 | 1 / 2 | 1.408 | 13.000 | +0.0505 | +1.13% |
| Oman | 375.09 | 3.9385 | 0 / 2 | 0.000 | 0.000 | +0.2233 | +5.67% |
| Venezuela | 374.92 | 3.4567 | 0 / 2 | 0.000 | 0.000 | +0.1960 | +5.67% |
| Ghana | 357.63 | 1.5223 | 0 / 2 | 0.000 | 0.000 | +0.0863 | +5.67% |
| Sri Lanka | 350.16 | 2.9789 | 0 / 2 | 0.000 | 0.083 | +0.1688 | +5.67% |
| Congo | 329.68 | 1.9567 | 0 / 3 | 0.000 | 0.000 | +0.1110 | +5.67% |
| Kuwait | 300.67 | 3.1571 | 2 / 2 | 0.615 | 6.000 | +0.0115 | +0.36% |
| Cameroon | 291.80 | 2.5943 | 0 / 2 | 0.000 | 0.000 | +0.1471 | +5.67% |
| New Zealand | 276.31 | 2.9013 | 2 / 2 | 2.323 | 0.167 | -0.1679 | -5.79% |
| Uganda | 256.59 | 1.6919 | 0 / 1 | 0.000 | 0.000 | +0.0965 | +5.70% |
| Dominican Republic | 254.70 | 2.2579 | 1 / 2 | 0.222 | 6.417 | +0.0494 | +2.19% |
| Ivory Coast | 250.49 | 1.8237 | 0 / 1 | 0.000 | 0.000 | +0.1216 | +6.67% |
| Slovakia | 246.31 | 2.5863 | 1 / 1 | 0.942 | 0.000 | +0.0025 | +0.10% |
| Belarus | 233.81 | 2.4550 | 1 / 1 | 0.593 | 0.083 | +0.0298 | +1.21% |
| Tunisia | 228.65 | 1.8689 | 0 / 1 | 0.000 | 0.000 | +0.1060 | +5.67% |
| Bulgaria | 210.16 | 2.2067 | 1 / 1 | 0.268 | 0.000 | +0.0483 | +2.19% |
| Guatemala | 209.35 | 0.8890 | 0 / 1 | 0.000 | 0.000 | +0.0504 | +5.67% |
| Tanzania | 196.44 | 0.3909 | 0 / 3 | 0.000 | 0.000 | +0.0222 | +5.67% |
| Paraguay | 183.79 | 1.5926 | 0 / 1 | 0.000 | 0.083 | +0.0902 | +5.67% |
| Cambodia | 176.25 | 1.6054 | 0 / 1 | 0.000 | 0.000 | +0.0910 | +5.67% |
| Lithuania | 175.14 | 1.8390 | 1 / 1 | 0.038 | 0.000 | +0.0504 | +2.74% |
| Cuba | 174.44 | 1.8316 | 0 / 1 | 0.000 | 0.000 | +0.1039 | +5.67% |
| Panama | 163.41 | 1.6359 | 0 / 1 | 0.000 | 0.000 | +0.0928 | +5.67% |
| Jordan | 154.30 | 1.5291 | 0 / 1 | 0.000 | 0.000 | +0.0867 | +5.67% |
| Serbia | 144.54 | 1.3108 | 0 / 1 | 0.000 | 1.989 | +0.0723 | +5.52% |
| Sudan | 144.45 | 1.1254 | 0 / 3 | 0.000 | 0.000 | +0.0638 | +5.67% |
| Libya | 142.59 | 1.2496 | 0 / 2 | 0.000 | 0.000 | +0.0709 | +5.67% |
| Bolivia | 141.10 | 1.1987 | 0 / 1 | 0.000 | 0.250 | +0.0677 | +5.65% |
| Mali | 139.66 | 0.8423 | 0 / 2 | 0.000 | 0.000 | +0.0478 | +5.67% |
| Slovenia | 136.59 | 1.4342 | 1 / 1 | 0.000 | 0.000 | +0.0313 | +2.18% |
| Syria | 136.49 | 1.1178 | 0 / 1 | 0.000 | 0.000 | +0.0634 | +5.67% |
| Mongolia | 127.62 | 1.2747 | 0 / 1 | 0.000 | 0.000 | +0.0723 | +5.67% |
| Costa Rica | 125.20 | 1.1747 | 0 / 1 | 0.000 | 0.000 | +0.0666 | +5.67% |
| Burkina Faso | 118.57 | 0.7131 | 0 / 1 | 0.000 | 0.000 | +0.0409 | +5.74% |
| Senegal | 114.79 | 0.7011 | 0 / 1 | 0.000 | 0.000 | +0.0398 | +5.67% |
| Guinea | 105.16 | 0.8139 | 0 / 1 | 0.000 | 0.000 | +0.0462 | +5.67% |
| El Salvador | 99.13 | 0.8422 | 0 / 1 | 0.000 | 0.000 | +0.0478 | +5.67% |
| Niger | 98.78 | 0.5637 | 0 / 1 | 0.000 | 0.000 | +0.0324 | +5.75% |
| Gabon | 92.48 | 0.9712 | 0 / 1 | 0.000 | 0.000 | +0.0551 | +5.67% |
| Uruguay | 91.68 | 0.6011 | 0 / 1 | 0.218 | 0.000 | +0.0122 | +2.04% |
| Rwanda-Burundi | 84.49 | 0.4382 | 0 / 1 | 0.000 | 0.000 | +0.0248 | +5.67% |
| Zimbabwe | 83.77 | 0.3523 | 0 / 1 | 0.000 | 0.000 | +0.0200 | +5.67% |
| Benin | 83.67 | 0.5512 | 0 / 1 | 0.000 | 0.000 | +0.0312 | +5.67% |
| Lesser Antilles States | 82.73 | 0.8304 | 0 / 1 | 0.000 | 0.000 | +0.0471 | +5.67% |
| Mauritania | 79.40 | 0.7686 | 0 / 1 | 0.000 | 0.000 | +0.0436 | +5.67% |
| Laos | 77.83 | 0.7050 | 0 / 1 | 0.000 | 0.083 | +0.0399 | +5.66% |
| Tajikistan | 73.42 | 0.6321 | 0 / 1 | 0.000 | 0.000 | +0.0358 | +5.67% |
| Chad | 73.18 | 0.5756 | 0 / 2 | 0.000 | 0.000 | +0.0326 | +5.67% |
| Zambia | 68.31 | 0.2193 | 0 / 1 | 0.000 | 0.000 | +0.0124 | +5.67% |
| Republic of the Congo | 68.25 | 0.4666 | 0 / 1 | 0.000 | 0.000 | +0.0265 | +5.67% |
| Bosnia-Herzegovina | 68.02 | 0.6825 | 0 / 1 | 0.000 | 4.000 | +0.0347 | +5.08% |
| Guyana | 67.07 | 0.7043 | 0 / 1 | 0.000 | 0.000 | +0.0399 | +5.67% |
| Lebanon | 64.69 | 0.6092 | 0 / 1 | 0.000 | 0.000 | +0.0345 | +5.67% |
| Nicaragua | 60.32 | 0.3670 | 0 / 1 | 0.000 | 0.000 | +0.0208 | +5.67% |
| Armenia | 58.52 | 0.5953 | 0 / 1 | 0.000 | 0.000 | +0.0338 | +5.67% |
| Mauritius | 57.54 | 0.6042 | 0 / 1 | 0.000 | 0.000 | +0.0343 | +5.67% |
| Malawi | 57.43 | 0.3696 | 0 / 1 | 0.000 | 0.000 | +0.0210 | +5.67% |
| Papua New Guinea | 57.23 | 0.3248 | 0 / 1 | 0.000 | 0.000 | +0.0184 | +5.67% |
| Kyrgyzstan | 55.11 | 0.4856 | 0 / 1 | 0.008 | 0.000 | +0.0267 | +5.50% |
| Honduras | 53.28 | 0.1559 | 0 / 1 | 0.000 | 0.000 | +0.0088 | +5.67% |
| Madagascar | 52.51 | 0.2001 | 0 / 1 | 0.000 | 0.000 | +0.0113 | +5.67% |
| North Korea | 51.20 | 0.0000 | 0 / 1 | 0.000 | 0.079 | +0.0000 | N/A (zero baseline) |
| Jamaica | 48.69 | 0.4500 | 0 / 1 | 0.000 | 2.000 | +0.0235 | +5.23% |
| Estonia | 48.21 | 0.5062 | 0 / 1 | 0.000 | 0.000 | +0.0287 | +5.67% |
| Namibia | 46.54 | 0.2698 | 0 / 1 | 0.000 | 0.000 | +0.0153 | +5.67% |
| Palestine | 46.54 | 0.4101 | 0 / 1 | 0.000 | 0.000 | +0.0233 | +5.67% |
| Albania | 46.08 | 0.4839 | 0 / 1 | 0.000 | 1.000 | +0.0264 | +5.46% |
| Equatorial Guinea | 44.17 | 0.4637 | 0 / 1 | 0.000 | 0.000 | +0.0263 | +5.67% |
| Haiti | 43.85 | 0.2258 | 0 / 1 | 0.000 | 0.000 | +0.0128 | +5.67% |
| Brunei Darussalam | 42.75 | 0.4488 | 0 / 1 | 0.000 | 3.000 | +0.0225 | +5.00% |
| Sierra Leone | 41.95 | 0.2636 | 0 / 1 | 0.000 | 0.000 | +0.0149 | +5.67% |
| Moldova | 41.46 | 0.4099 | 0 / 1 | 0.000 | 2.000 | +0.0212 | +5.18% |
| Somalia | 41.05 | 0.2433 | 0 / 1 | 0.000 | 0.000 | +0.0138 | +5.67% |
| North Macedonia | 40.17 | 0.4218 | 0 / 1 | 0.000 | 0.990 | +0.0229 | +5.44% |
| Togo | 38.40 | 0.2894 | 0 / 1 | 0.000 | 0.000 | +0.0164 | +5.67% |
| Botswana | 35.35 | 0.0921 | 0 / 1 | 0.000 | 0.000 | +0.0052 | +5.67% |
| Kosovo | 34.64 | 0.3637 | 0 / 1 | 0.000 | 4.000 | +0.0166 | +4.57% |
| Mozambique | 31.16 | 0.0583 | 0 / 2 | 0.000 | 0.000 | +0.0033 | +5.67% |
| South Yemen | 29.84 | 0.2371 | 0 / 1 | 0.000 | 0.000 | +0.0134 | +5.67% |
| Yemen | 29.24 | 0.2190 | 0 / 1 | 0.000 | 0.000 | +0.0124 | +5.67% |
| Iceland | 28.71 | 0.3015 | 0 / 1 | 0.000 | 0.000 | +0.0171 | +5.67% |
| South Sudan | 19.84 | 0.1469 | 0 / 1 | 0.000 | 0.000 | +0.0083 | +5.67% |
| Lesotho | 19.16 | 0.1159 | 0 / 1 | 0.000 | 0.000 | +0.0066 | +5.67% |
| Melanesian States | 18.53 | 0.1480 | 0 / 1 | 0.000 | 0.000 | +0.0084 | +5.67% |
| Bhutan | 17.02 | 0.1787 | 0 / 1 | 0.000 | 0.000 | +0.0101 | +5.67% |
| The Bahamas | 15.57 | 0.1634 | 0 / 1 | 0.000 | 0.000 | +0.0093 | +5.67% |
| Montenegro | 14.97 | 0.1571 | 0 / 1 | 0.000 | 0.000 | +0.0089 | +5.67% |
| Liberia | 14.10 | 0.0841 | 0 / 1 | 0.000 | 0.000 | +0.0048 | +5.67% |
| The Gambia | 13.27 | 0.0928 | 0 / 1 | 0.000 | 0.000 | +0.0053 | +5.67% |
| Djibouti | 13.03 | 0.1173 | 0 / 1 | 0.000 | 0.000 | +0.0067 | +5.67% |
| Suriname | 12.24 | 0.1030 | 0 / 1 | 0.000 | 0.000 | +0.0058 | +5.67% |
| Somaliland | 11.96 | 0.0624 | 0 / 1 | 0.000 | 0.000 | +0.0035 | +5.67% |
| Eswatini | 11.08 | 0.0490 | 0 / 1 | 0.000 | 0.000 | +0.0028 | +5.67% |
| Central African Republic | 10.73 | 0.0822 | 0 / 2 | 0.000 | 0.000 | +0.0047 | +5.67% |
| Guinea-Bissau | 7.86 | 0.0506 | 0 / 1 | 0.000 | 0.000 | +0.0029 | +5.67% |
| Cabo Verde | 6.00 | 0.0446 | 0 / 1 | 0.000 | 0.000 | +0.0025 | +5.67% |
| Timor-Leste | 5.85 | 0.0387 | 0 / 1 | 0.000 | 0.000 | +0.0022 | +5.67% |
| Belize | 5.25 | 0.0370 | 0 / 1 | 0.000 | 0.000 | +0.0021 | +5.67% |
| Seychelles | 3.22 | 0.0338 | 0 / 1 | 0.000 | 0.000 | +0.0019 | +5.67% |
| Comoros | 1.87 | 0.0044 | 0 / 1 | 0.000 | 0.000 | +0.0003 | +5.67% |
| Polynesian States | 1.85 | 0.0135 | 0 / 1 | 0.000 | 0.000 | +0.0008 | +5.67% |
| Micronesian States | 1.28 | 0.0071 | 0 / 1 | 0.000 | 0.000 | +0.0004 | +5.67% |
| São Tomé and Príncipe | 0.39 | 0.0012 | 0 / 1 | 0.000 | 0.000 | +0.0001 | +5.67% |
