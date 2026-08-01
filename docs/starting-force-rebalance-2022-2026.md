# Starting Army and Navy Rebalance: 2022 and 2026

Status: implemented in the mod's starting-force JSON

Research cutoff: 1 August 2026

## Recommendation in one page

Use one army counter for roughly 100,000 active personnel contributing to a
deployable joint-service force capable of sustained offensive operations. This
includes the aviation, naval, logistics, intelligence, and command personnel
that make land formations usable abroad; it is not limited to the army branch.
Exclude training and base establishments that do not support deployable forces,
internal-security forces, low-readiness conscripts, and reserves that are
neither mobilized nor integrated into field formations.

Use one real-world navy equivalent for one carrier strike group (CSG), or for a
blue-water task-force pool with comparable aggregate combat, submarine,
amphibious, and replenishment capacity. A CSG is not just a carrier: official US
descriptions include the carrier and air wing, cruiser/destroyer escorts, and
support, with submarines commonly assigned as well. [The US Government
Accountability Office gives a representative composition](https://www.gao.gov/assets/a271867.html),
and [US Carrier Strike Group 2 provides a concrete operational
example](https://www.c2f.usff.navy.mil/csg2/).

The headline proposal is:

| Snapshot | Vanilla armies | Proposed armies | Strength-adjusted armies | Vanilla navies | Proposed navies |
|---|---:|---:|---:|---:|---:|
| 2022 scenario | 27 | 97 | 93.75 | 12 | 32 |
| 2026 scenario | 27 | 105 | 100.85 | 12 | 34 |

This is still conservative relative to raw personnel totals. The counter is a
measure of a **joint-service offensive package**, not only 100,000 members of a
country's army branch. Active personnel across the armed forces provide the
starting estimate, navies impose a hard floor, and military expenditure tests
whether the result understates the ability to equip, support, and replace major
formations. The US calibration is thirteen counters: about 1.3 million active
personnel, exceptional joint support, and sufficient spending for thirteen
sustained packages. Miltech still represents equipment and doctrinal quality.

## Conversion method

### Army counters

For each country and date:

```text
offensive pool = total active regular armed forces
                 x usable joint-service share

personnel estimate = conservative rounding(offensive pool / 100,000)

army counters = max(personnel estimate,
                    navy equivalents,
                    spending-supported floor)
```

The deployable share is a judgment band, not a spurious decimal statistic:

| Force structure | Typical retained share | Treatment |
|---|---:|---|
| Professional, expeditionary, well-supported | 80-100% | Count the joint package: maneuver troops, aviation, logistics, intelligence, sealift, and command capacity. |
| Mixed professional/conscript conventional force | 60-80% | Count ready joint-service formations; discount training, garrison, and short-service manpower. |
| Garrison-heavy, internal-security, or poorly sustained force | 30-55% | Count only personnel plausibly contributing to an external offensive. |
| Mobilized wartime force | case-specific | Count field formations, then use `startingStrength` for real attrition and under-manning. |

This distinction is necessary because the joint basis includes air, naval, and
support personnel but still excludes most paramilitary, internal-security,
inactive reserve, and purely institutional manpower. Even the World Bank/IISS
headline series includes active paramilitary personnel when they can replace
regular forces and can therefore overstate the usable pool. See the
[World Bank indicator definition and limitations](https://databank.worldbank.org/metadataglossary/world-development-indicators/series/MS.MIL.TOTL.P1).
The European Defence Agency separately tracks deployable and sustainable forces,
which is conceptually much closer to the quantity wanted here than gross
headcount. See the [EDA Defence Data portal](https://eda.europa.eu/publications-and-data/defence-data).

Counts are whole counters. `startingStrength` is retained for forces damaged or
seriously under-manned at the scenario date; it is not used as a substitute for
low military technology. Equipment, training, command, and doctrine belong in
`miltech`.

### Military-expenditure cross-check

Military spending is the second independent yardstick because it captures
capabilities that a land-personnel count misses: aviation, intelligence,
precision weapons, logistics, readiness, stock replacement, and the expensive
institutional system behind deployable forces. The scenario tables use SIPRI
current-dollar expenditure for 2022 and the latest complete year, 2025, for the
2026 start. These are calibration figures, not claims that dollars translate
directly into soldiers. See the [SIPRI Military Expenditure Database](https://www.sipri.org/databases/milex).

For a conservative check, approximately **$75 billion of annual expenditure per
counter** is used as the expensive-force reference point. The spending check can
raise an obviously undercounted force, but never reduces a personnel-supported
count. It is a guardrail rather than a universal divisor: nominal exchange
rates understate the purchasing power of China, India, and other lower-cost
forces, while nuclear forces, pensions, military aid, wartime replacement, and
procurement spikes can inflate expenditure without producing another
deployable package.

For countries already qualifying for at least one counter by personnel or navy,
the auditable lower-bound check is:

```text
spending-supported floor = ceiling(SIPRI current-US$ spending / $75 billion)
```

Thus 2025 US spending independently supports thirteen counters, while Germany
and Saudi Arabia cross the two-counter threshold. China and India remain driven
primarily by personnel and force structure rather than nominal-dollar spending.

### Navy equivalents and the hard floor

Terra Invicta does not store an independent navy counter. `DeploymentType.Naval`
is a flag on an army, and the runtime caps navies by army count. This is treated
as a design constraint rather than a reason to discard naval capacity:

```text
proposed army counters >= proposed navy equivalents
```

Every navy equivalent in the tables is therefore represented by a naval army;
there is no separate compressed total. For maritime powers, the extra army
counters represent the air, sealift, logistics, command, and expeditionary
capacity accompanying the fleet rather than an assertion that the country has
that many literal 100,000-person field armies. The United States' FY2025 force
structure had ten carrier strike groups, 31 active Army brigade combat teams,
and three Marine Expeditionary Forces; see the [Congressional Research Service
force-structure table](https://www.congress.gov/crs-product/IN12447).
The US Navy is legally required to maintain at least eleven operational aircraft
carriers, although not every carrier is an available CSG at the same instant;
see the [CRS Navy force-structure report](https://www.congress.gov/crs-product/RL32665).

For fleet conversion, one navy equivalent is a **sustained** CSG-equivalent, not
the displacement of only the ships photographed sailing in formation. A typical
CSG at sea may displace roughly 180,000-220,000 tonnes, but maintaining one as
usable national capacity also consumes ships in maintenance and training,
replacement escorts, logistics, and shore-supported rotation. This proposal
therefore uses roughly **350,000 tonnes of eligible commissioned fleet inventory
per sustained equivalent**, then applies a readiness and blue-water-sustainment
adjustment.

Eligible inventory includes carriers and aviation ships, major surface
combatants, attack submarines, amphibious ships, and fleet replenishment ships.
It excludes coast guards, patrol craft, mine-warfare flotillas, strategic
ballistic-missile submarines, and most special-purpose auxiliaries. This avoids
both carrier-only undercounting and broad-tonnage inflation.

Published totals vary materially with those inclusion rules. A narrow
conventional-warship comparison commonly places the US fleet around 4.5 million
tonnes and the PLAN around 2.0-2.5 million, while broader inventories that add
more auxiliaries produce substantially larger totals for both. The useful
conclusion is a range, not a false exact number: US aggregate naval capacity in
2026 is roughly twice China's, even though China has more hulls.

The 2026 estimate therefore assigns the United States **13** equivalents and
China **6**, a 2.17:1 ratio. The US number does not claim thirteen operational
supercarrier groups. Official FY2025 force structure listed ten CSGs; treating
roughly eight as near-term available, discounting the remainder for maintenance
and workups, and then crediting big-deck amphibious groups, surface action
groups, attack submarines, and the replenishment fleet produces about thirteen
aggregate equivalents. [CRS distinguishes CSGs, amphibious ready groups, and
surface action groups](https://www.congress.gov/crs-products/product/pdf/IF/IF10486/30),
while the [US Navy notes that LHA/LHD assault ships resemble small carriers and
anchor amphibious or expeditionary groups](https://www.navy.mil/Resources/Fact-Files/Display-FactFiles/Article/2169814/no/amphibious-assault-ships-lhdlhar/).

China's six include two mature carrier groups, partial credit for `Fujian` at
the scenario date, four Type 075 amphibious assault ships, and the aggregate
power of more than 140 major surface combatants, submarines, and a growing but
still shallower replenishment network. The underlying 2024-2025 inventory is
described in the [US Department of Defense China report](https://media.defense.gov/2024/Dec/18/2003615520/-1/-1/0/MILITARY-AND-SECURITY-DEVELOPMENTS-INVOLVING-THE-PEOPLES-REPUBLIC-OF-CHINA-2024%20.PDF).

The 350,000-tonne value is a centerline rather than a hard minimum. A balanced,
high-readiness carrier or amphibious fleet slightly below it can round to one;
an old, coastal, strategically oriented, or poorly supported fleet well above
it can be discounted. This produces the following audit table:

| Navy | 2022 eq. | 2026 eq. | Calibration judgment |
|---|---:|---:|---|
| United States | 12 | 13 | Carrier force plus the uniquely large LHA/LHD, SSN, surface-action, amphibious, and replenishment inventory. |
| China | 4 | 6 | Third carrier, fourth Type 075, many new major combatants, and improved sustainment; still discounted against US global readiness and support depth. |
| Russia | 2 | 1 | Large nominal and submarine tonnage, but strategic boats are excluded and conventional blue-water readiness has deteriorated. |
| Japan | 2 | 2 | Large, modern surface and submarine fleet with excellent readiness, but limited aviation and distant sustainment. |
| India | 2 | 2 | Two carriers and a growing balanced fleet; 2025 growth improves confidence rather than crossing a third-equivalent threshold. |
| France | 2 | 2 | One complete carrier group plus SSNs, amphibious aviation ships, major escorts, and independent logistics. |
| United Kingdom | 2 | 2 | Two large carriers and SSNs, discounted for air-wing, escort, and auxiliary availability. |
| South Korea | 1 | 1 | Modern regional surface/submarine force and amphibious lift, but not two sustained CSG equivalents. |
| Italy | 1 | 1 | Carrier/amphibious aviation, escorts, submarines, and replenishment reach one balanced equivalent. |
| Spain | 1 | 1 | One light-carrier/amphibious-centered regional task-force equivalent. |
| Turkey | 1 | 1 | `Anadolu`, escorts, submarines, and support reach one regional equivalent. |
| Australia | 1 | 1 | High-quality submarines, escorts, LHDs, and allied sustainment reach one equivalent; the joint-force army floor carries its flag. |
| Egypt | 1 | 1 | Two Mistrals plus modern frigates, corvettes, and submarines narrowly reach one regional equivalent. |
| **Total** | **32** | **34** | |

As a cross-check, a detailed January 2026 displacement inventory finds the US
Navy larger than the Chinese and Russian navies combined, while recording the
PLAN above three million tonnes after commissioning `Fujian`, a fourth Type 075,
and numerous new destroyers and frigates. It also records Turkey at about
335,000 tonnes, Spain at 241,000, Egypt at 230,000, and Australia at 218,000;
see the [aggregate displacement inventory and methodology](https://chuckhillscgblog.net/2026/01/11/top-ten-navies-by-aggregate-displacement-1-january-2026-phoenix_jz/).

## 2022 scenario starting forces

Snapshot: 30 September 2022, matching `ModernDayStart`. Vanilla has 27 nominal
armies (25.65 after starting strength) in only fourteen countries and twelve
navies. The proposal adds seventy counters. This is the largest conceptual
change from vanilla: the count now represents joint offensive capacity rather
than land-branch maneuver personnel alone.

`V A/N` means vanilla armies/navies. `SIPRI spend` is full-year 2022 military
expenditure in current US$ billions; `n/a` means SIPRI publishes no usable
figure. `Delta A/N` compares the proposal with vanilla.

| Nation | V A/N | SIPRI spend | Armies | Avg. strength | Navies | Delta A/N |
|---|---:|---:|---:|---:|---:|---:|
| Algeria | 0/0 | 9.1 | 1 | 1.00 | 0 | +1/0 |
| Australia | 0/0 | 32.4 | 1 | 1.00 | 1 | +1/+1 |
| Brazil | 0/0 | 20.5 | 3 | 1.00 | 0 | +3/0 |
| China | 4/2 | 291.6 | 13 | 1.00 | 4 | +9/+2 |
| Egypt | 1/0 | 4.6 | 2 | 1.00 | 1 | +1/+1 |
| Ethiopia | 0/0 | 1.0 | 1 | 0.80 | 0 | +1/0 |
| France | 1/1 | 54.7 | 2 | 1.00 | 2 | +1/+1 |
| Germany | 0/0 | 56.1 | 2 | 1.00 | 0 | +2/0 |
| Greece | 0/0 | 8.8 | 1 | 1.00 | 0 | +1/0 |
| India | 4/1 | 79.9 | 11 | 1.00 | 2 | +7/+1 |
| Indonesia | 0/0 | 10.1 | 2 | 1.00 | 0 | +2/0 |
| Iran | 1/0 | 7.5 | 3 | 1.00 | 0 | +2/0 |
| Israel | 1/0 | 22.8 | 2 | 1.00 | 0 | +1/0 |
| Italy | 0/0 | 33.7 | 2 | 1.00 | 1 | +2/+1 |
| Japan | 0/0 | 43.1 | 2 | 1.00 | 2 | +2/+2 |
| Myanmar | 0/0 | 2.5 | 1 | 0.70 | 0 | +1/0 |
| North Korea | 1/0 | n/a | 4 | 1.00 | 0 | +3/0 |
| Pakistan | 1/0 | 10.3 | 4 | 1.00 | 0 | +3/0 |
| Poland | 0/0 | 15.3 | 1 | 1.00 | 0 | +1/0 |
| Russia | 3/1 | 104.4 | 6 | 0.75 | 2 | +3/+1 |
| Saudi Arabia | 0/0 | 70.9 | 1 | 1.00 | 0 | +1/0 |
| South Korea | 1/0 | 46.4 | 3 | 1.00 | 1 | +2/+1 |
| Spain | 0/0 | 20.7 | 1 | 1.00 | 1 | +1/+1 |
| Thailand | 0/0 | 6.0 | 2 | 1.00 | 0 | +2/0 |
| Turkey | 1/0 | 15.0 | 3 | 1.00 | 1 | +2/+1 |
| Ukraine | 1/0 | 41.5 | 5 | 0.75 | 0 | +4/0 |
| United Kingdom | 1/1 | 64.0 | 2 | 1.00 | 2 | +1/+1 |
| United States | 6/6 | 860.7 | 13 | 1.00 | 12 | +7/+6 |
| Vietnam | 0/0 | 7.1 | 3 | 1.00 | 0 | +3/0 |
| **Total** | **27/12** | **—** | **97** | **93.75 effective** | **32** | **+70/+20** |

The US anchor is thirteen. FY2022 active end strength across the Army, Navy,
Marine Corps, Air Force, and Space Force was approximately 1.30 million, before
the selected reserve; the professional joint force and $860.7 billion spending
level support retaining essentially the full active total. See the [official
FY2022 population-representation tables](https://prhome.defense.gov/Portals/52/Documents/MRA_Docs/MPP/AP/poprep/2022/Appendix_F_US_Supp%20Tables_.pdf?ver=Rp8y4Leh32HAWeGMsXaJPA%3D%3D)
and [Department of Defense demographics release](https://www.defense.gov/News/Releases/Release/article/3580676/defense-department-report-shows-decline-in-armed-forces-population-while-percen/).

China rises to thirteen rather than seven. The 2022 US Department of Defense
report counted about two million active personnel across the PLA, including
1.04 million in the ground force, 13 group armies, 81 combined-arms brigades,
seven airborne brigades, and eight marine brigades. A roughly 65% usable share
preserves a substantial discount for conscripts, institutions, static missions,
and sustainment limits; see the [report's force table](https://media.defense.gov/2022/Nov/29/2003122279/-1/-1/1/2022-MILITARY-AND-SECURITY-DEVELOPMENTS-INVOLVING-THE-PEOPLES-REPUBLIC-OF-CHINA.PDF).

India's eleven, Brazil's three, Indonesia's two, and similar increases follow
from counting the joint active force rather than only maneuver formations.
Russia and Ukraine receive six and five nominal counters but start at 0.75
strength, separating mobilized capacity from wartime attrition and under-manning.

## 2026 scenario starting forces

Snapshot: 31 January 2026, matching `2026Start`. This is the least certain
table: national publications and open-source estimates lag the scenario date,
and Russia and Ukraine actively conceal or contest force data. It should be
reviewed at each major game-data update.

`SIPRI spend` is full-year 2025 expenditure in current US$ billions, used as
the latest complete spending benchmark available at the scenario start.

| Nation | V A/N | SIPRI spend | Armies | Avg. strength | Navies | Delta A/N |
|---|---:|---:|---:|---:|---:|---:|
| Algeria | 0/0 | 25.4 | 1 | 1.00 | 0 | +1/0 |
| Australia | 0/0 | 35.3 | 1 | 1.00 | 1 | +1/+1 |
| Brazil | 0/0 | 23.9 | 3 | 1.00 | 0 | +3/0 |
| China | 4/2 | 335.5 | 14 | 1.00 | 6 | +10/+4 |
| Egypt | 1/0 | 2.5 | 2 | 1.00 | 1 | +1/+1 |
| Ethiopia | 0/0 | 0.5 | 1 | 0.75 | 0 | +1/0 |
| France | 1/1 | 68.0 | 2 | 1.00 | 2 | +1/+1 |
| Germany | 0/0 | 113.6 | 2 | 1.00 | 0 | +2/0 |
| Greece | 0/0 | 8.4 | 1 | 1.00 | 0 | +1/0 |
| India | 4/1 | 92.1 | 11 | 1.00 | 2 | +7/+1 |
| Indonesia | 0/0 | 15.0 | 2 | 1.00 | 0 | +2/0 |
| Iran | 1/0 | 7.4 | 3 | 1.00 | 0 | +2/0 |
| Israel | 1/0 | 48.3 | 2 | 1.00 | 0 | +1/0 |
| Italy | 0/0 | 48.1 | 2 | 1.00 | 1 | +2/+1 |
| Japan | 0/0 | 62.2 | 3 | 1.00 | 2 | +3/+2 |
| Myanmar | 0/0 | n/a | 1 | 0.60 | 0 | +1/0 |
| North Korea | 1/0 | n/a | 4 | 1.00 | 0 | +3/0 |
| Pakistan | 1/0 | 11.9 | 4 | 1.00 | 0 | +3/0 |
| Poland | 0/0 | 46.8 | 2 | 1.00 | 0 | +2/0 |
| Russia | 3/1 | 190.4 | 8 | 0.75 | 1 | +5/0 |
| Saudi Arabia | 0/0 | 83.2 | 2 | 1.00 | 0 | +2/0 |
| South Korea | 1/0 | 47.8 | 4 | 1.00 | 1 | +3/+1 |
| Spain | 0/0 | 40.2 | 1 | 1.00 | 1 | +1/+1 |
| Thailand | 0/0 | 6.0 | 2 | 1.00 | 0 | +2/0 |
| Turkey | 1/0 | 30.0 | 3 | 1.00 | 1 | +2/+1 |
| Ukraine | 1/0 | 84.1 | 6 | 0.75 | 0 | +5/0 |
| United Kingdom | 1/1 | 89.0 | 2 | 1.00 | 2 | +1/+1 |
| United States | 6/6 | 954.4 | 13 | 1.00 | 13 | +7/+7 |
| Vietnam | 0/0 | 10.5 | 3 | 1.00 | 0 | +3/0 |
| **Total** | **27/12** | **—** | **105** | **100.85 effective** | **34** | **+78/+22** |

The modern European counts use the same joint-force rule rather than army-branch
headcount. Germany, for example, had about 186,400 total personnel in the NATO
estimate but only 63,974 in the Army in June 2026; two counters represent the
supported national military system, not two literal German field armies. See
[NATO's 2014-2025
defence tables](https://www.nato.int/content/dam/nato/webready/documents/finance/def-exp-2025-en.pdf)
and the [Bundeswehr's current personnel breakdown](https://www.bundeswehr.de/de/organisation/zahlen-daten-fakten/personalzahlen-bundeswehr).
France, Germany, Italy, Poland, and the UK therefore receive two counters;
Spain remains at one because its smaller active force and one navy equivalent
do not support rounding up again.

Ukraine's armed and security forces were estimated at roughly 850,000 to one
million in 2025, but public estimates put the number directly engaged on the
front at no more than about 300,000. Six counters at 0.75 strength represent
450,000 effective joint-force personnel: more than the frontline count because
the model now includes deployable support, but still well below raw mobilized
headcount. See the [UK government's 2025 Ukraine military
service assessment](https://www.gov.uk/government/publications/ukraine-country-policy-and-information-notes/country-policy-and-information-note-military-service-ukraine-june-2022-accessible).

The 2026 naval increase is concentrated in China and the United States. India
grew substantially but remains inside the two-equivalent band. France remains
at two navies because its official 2025 inventory includes a
carrier, three amphibious helicopter carriers, fifteen first-rank destroyers,
attack submarines, and logistics ships; its two army counters satisfy the hard
floor. See [France's Defence Key Figures 2025](https://www.defense.gouv.fr/sites/default/files/ministere-armees/Chiffres_Cle%CC%81s_2025_UK.pdf).
Japan's three joint-force counters support two navies; its 2025 white paper
shows four escort flotillas, two submarine flotillas, an amphibious rapid
deployment brigade, and a joint operations command. See
[Defense of Japan 2025](https://www.mod.go.jp/j/press/wp/wp2025/pdf/DOJ2025_Digest_EN.pdf).

## Implementation notes

### Implemented data work

The implementation performs the full scenario replacement rather than editing
only the existing 27 records:

1. `TIArmyTemplate.json` adds every new counter and overrides deployment and
   strength on vanilla records where required.
2. `TIMetaTemplate.json` replaces `ModernArmies` and `2026_Armies` with the full
   97- and 105-record inventories; defining a template alone does not load it.
3. Every new 2026 army uses a valid scenario-prefixed start and home region.
4. `deploymentType` is `Naval` for exactly the number in **Navies**.
5. Reduced starting strength is limited to the wartime or severely degraded
   forces shown in the tables.
6. New records use stable country/index IDs and neutral joint-force names rather
   than pretending the abstraction maps cleanly to literal corps or divisions.
7. `tools/validate-starting-forces.ps1` checks scenario membership, region
   ownership, strength, totals, and the armies-at-least-navies invariant against
   the installed vanilla templates.

### Army-cap conflict

Vanilla `TINationState.allowedArmies` is the minimum of eligible region count and
`1 + floor(population / 25 million)`. That map-driven cap is not the same thing
as the 100,000-person joint-force abstraction. The proposed four-counter North
and South Korean forces exceed their one-region map cap even though their
real-world manpower supports the recommendation. Starting templates may load
above the build cap, but doing so creates an inconsistent rule.

The JSON-only implementation necessarily uses the middle policy below:

| Policy | Result | Recommendation |
|---|---|---|
| Preserve vanilla region cap | Compress the Koreas and any other one-region power back to one counter. | Safe but abandons the stated joint-force scale. |
| Patch the cap for starting forces only | Load historical counters above the construction cap; losses cannot all be rebuilt. | **Implemented for this JSON slice.** |
| Replace the region term with a force-scale cap | Permit counters according to population/economic support while using regions only for basing. | **Preferred** if the 100,000-person definition becomes a design authority. |

The last option is the only one that makes the proposed scale durable through
play, but it is a separate balance change and should be tested with army upkeep,
AI construction, unification, and region transfer.

### Naval hard-floor implementation

There is no longer a compressed naval column. Every proposed navy equivalent is
assigned to an army, and every country's army count is at least its navy count.
For the United States this means thirteen armies and thirteen navies in 2026;
for Australia it means one army and one navy despite the land component alone
being smaller than 100,000. This follows the engine's representation directly
and treats the carrier army as a joint expeditionary package.

## Balance risks and validation

- The new global army stock nearly quadruples nominal army upkeep. Re-run the existing
  military investment and national-priority calibration before shipping.
- China, India, and the United States gain the most counters and therefore the largest military
  technology upgrade bill under the mod's force-wide equipment formula. This
  is correct in direction but must be checked against their starting budgets.
- More initial armies make early faction control of a major state more decisive.
  Test early wars, coups, unrest damage, and AI threat evaluation.
- Navies rise from twelve to thirty-two or thirty-four. Test overseas AI
  movement and amphibious-war frequency; a realistic inventory can still make
  poor gameplay if every naval army deploys simultaneously.
- Do not let the spending cross-check duplicate `miltech`: expenditure can
  justify the existence and sustainment of a counter, while combat quality
  remains a separate technology/stat calculation.
- Re-audit 2026 Russia and Ukraine immediately before implementation because
  those estimates are both fast-changing and unusually uncertain.

## Source hierarchy and uncertainty

The proposal prefers official national statistics and force-structure reports,
then NATO/EDA and IISS-derived comparative series. All of them still require
judgment: personnel totals mix services and support roles, national definitions
differ, some states publish unreliable numbers, nominal spending has different
purchasing power, and possession of equipment is not the same as deployable
readiness. The counts in this report are therefore balance recommendations
supported by public data, not claims of exact order of battle.

Confidence by snapshot:

| Snapshot | Army confidence | Navy confidence | Reason |
|---|---|---|---|
| 2022 | Medium-high | High | Scenario date is fixed and contemporary official/IISS-derived data is mature. |
| 2026 | Medium-low | Medium | Current force expansion, wartime losses, and delayed reporting make the snapshot provisional. |
