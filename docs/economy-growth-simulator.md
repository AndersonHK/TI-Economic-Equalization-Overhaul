# Economy Growth Simulator

Status: reproducible calibration record for the implemented factor-balance
defaults. Its vanilla 1.0.49 comparison is a historical benchmark; Version 0.9
runtime compatibility is validated against Terra Invicta 1.0.51.

## Purpose

`tools/economy-growth-simulator.js` is the reproducible calibration harness for
the Economy/Spoils growth redesign. It is not game code and it does not predict
history. It compares three formulas under identical simplified national inputs:

1. Terra Invicta 1.0.49 vanilla.
2. The pre-0.7.0 deployed Economic Equalization Overhaul benchmark.
3. The 0.7.0 factor-balance formula.

The simulator exists to answer shape and balance questions before patch code is
changed:

- Does one additional IP have diminishing returns when capital grows without
  labor or resources?
- Does proportional growth remain neutral when capital, labor, and resources
  grow together?
- Do resource-rich developed economies retain an advantage over otherwise
  comparable resource-poor economies?
- Can emerging economies sustain plausible catch-up growth when roughly
  30–70% of IP is assigned to Economy/Spoils?
- Do late technologies progressively loosen the factor constraints without
  making early technology almost linear?
- Do results remain sensible after TI's climate damage is deducted?

## Authoritative Code and Data

The checked-in simulator is:

```text
tools/economy-growth-simulator.js
```

Run its default comparison from the repository root with:

```powershell
node tools/economy-growth-simulator.js
```

Optional arguments are:

```text
--allocation 0.50
--end-year 2050
--end-temperature 2.7
--technology 0.50
```

The script contains:

- The extracted 2022 values for the representative countries.
- Named parameter sets for the 0.7.0 and pre-0.7.0 formulas.
- Separate gain and Investment Point calculations for 0.7.0, pre-0.7.0, and
  vanilla behavior.
- TI 1.0.49's climate-damage equation.
- The month-by-month projection loop.
- A dependency-free command-line report.

The checked-in script is the durable calibration record for the implemented
0.7.0 defaults. Any later visualization or gameplay retuning must be updated
from the same parameters and equations.

## Sources and Extraction

The formulas were recovered from the locally installed Terra Invicta 1.0.49
assemblies and templates, not estimated from tooltip text:

- `Assembly-CSharp.dll` supplied vanilla Economy, Investment Point, and climate
  behavior.
- The installed 2022 scenario supplied starting population, GDP per capita,
  Education, Government, Cohesion, Unrest, Inequality, and greenhouse gases.
- Installed region templates supplied Core Economic regions, Resource regions,
  population density, and population-weighted climate exposure.
- The deployed `Settings.xml` and current patch source supplied the as-is mod
  constants.

The extracted vanilla formulas used by the harness are:

```text
monthlyIP =
    GDP_billions^0.35
    * (1 - max(Unrest - 2, 0) / 10)

EconomyGDPPerCapitaGain =
    (3 + 1.5 * ResourceRegions + 1.5 * CoreRegions
       + 0.5 * Government + Education)
    * (Population / 50M)^-0.35
```

For temperature above `0.25 C`, TI's annual GDP damage is:

```text
excess = Temperature - 0.25
annualDamage =
    ((0.14577 * excess^2 + 0.31839 * excess)
    * 1.14^Inequality / 100)
    * populationWeightedClimateExposure
```

Beneficiary, Standard, and Vulnerable regional populations contribute exposure
weights `0`, `1`, and `2`. The annual damage is converted to its compounded
monthly equivalent before GDP growth is applied.

## Simulation Order

Each simulated month follows the same order:

1. Interpolate temperature and technology progress for that month.
2. Recalculate GDP per capita from the current GDP and fixed population.
3. Calculate the selected model's return from one Economy/Spoils IP.
4. Calculate monthly IP from the current GDP and fixed national conditions.
5. Assign the selected fraction of IP to GDP growth.
6. Calculate climate damage from temperature, current Inequality, and regional
   exposure.
7. Add priority GDP and deduct the compounded monthly climate loss.
8. Apply TI's climate-driven Inequality feedback.

Population, institutions, regions, density, and Unrest are deliberately held
fixed. The model excludes war, demographics, trade, advisers, army upkeep,
occupation, faction project effects, and endogenous emissions. These
omissions make it a controlled formula comparison rather than a historical
forecast.

For vanilla, all selected growth allocation is treated as Economy because
vanilla Spoils does not add GDP. This gives vanilla the favorable comparison.
Both mod models allow the allocation to represent any mix of
Economy and Spoils because both are intended to buy the same fixed GDP gain.

## Calibration Process

Each calibration round uses the following sequence:

1. **Preserve the benchmarks.** Vanilla and pre-0.7.0 formulas remain unchanged;
   only the 0.7.0 candidate and scenario assumptions may be tuned.
2. **Run shape tests.** Compare capital-only doubling, proportional doubling of
   all factors, zero resources, extreme density, zero/max technology, and high
   GDP per capita.
3. **Run national counterfactuals.** Remove Resource regions from the United
   States, Canada, and Australia to isolate their resource contribution.
4. **Run allocation bands.** Check 30%, 50%, and 70% Economy/Spoils allocation.
5. **Apply climate.** Compare gross and net growth using the same temperature
   path and TI regional exposure for all three models.
6. **Compare trajectories.** Inspect both initial net growth and the full
   GDP-per-capita path; a plausible first year is insufficient if the formula
   later explodes or collapses.
7. **Record decisions.** Update this document, the calibration plan, the script,
   tests, and gameplay defaults together before release.

## Calibration History

### Round 1: Factor balance and exact climate baseline

The first climate-aware proposal used:

```text
base GDP gain per IP                 $1.00B
labor knee                           $40,000 GDP/c
resource knee                        $55,000 GDP/c
initial labor floor                  0.35
initial resource floor               0.45
constraint relief                    p * (0.25 + 0.75p)
full-tree productivity multiplier    3.7964x
default 2050 temperature             3.0 C
```

It established the desired broad shape and corrected the near-zero US return
in the deployed formula. At 50% allocation, initial climate-adjusted growth was
approximately `2.6%` in the US, `5.6%` in Canada, `4.6%` in Australia, `2.4%`
in Germany, `2.5%` in the UK, `5.7%` in China, and `3.9%` in India.

The remaining concerns were that the assumed climate trajectory and full-tree
technology lift were slightly too strong, while the initial constraint-relief
curve became linear too readily.

### Round 2: Milder climate path and more back-loaded technology

The second calibration preserves TI's climate-damage equation exactly but
changes the default scenario from `3.0 C` to `2.7 C` in 2050. This is a scenario
assumption, not a proposed climate gameplay patch; the temperature control can
still reproduce the `3.0 C` stress case.

The proposed full-tree productivity multiplier falls from `3.7964x` to `3.40x`.
The deployed benchmark retains its actual `3.7964x` trajectory, so retuning the
proposal cannot silently improve or weaken the as-is comparison.

Constraint relief changes from:

```text
p * (0.25 + 0.75p)
```

to:

```text
p * (0.10 + 0.90p)
```

At 25%, 50%, and 75% weighted progress, the new curve supplies approximately
`8%`, `28%`, and `58%` of the possible relief, rather than `11%`, `31%`, and
`61%`. Every technology still helps immediately, but the earliest part of the
tree is less linear and the strongest substitution effects remain late-game.

With 50% progress and the milder `2.7 C` climate path, the 0.7.0 candidate reaches
approximately `$127k` US GDP/c, `$230k` Canadian GDP/c, `$164k` Australian
GDP/c, `$108k` German GDP/c, `$96k` UK GDP/c, `$71k` Chinese GDP/c, and `$25k`
Indian GDP/c in 2050. Starting rates are unchanged because both rounds begin
with the same 2022 technology and `1.2601 C` temperature.

The two small adjustments mostly offset for climate-exposed economies while
still reducing technology-driven divergence:

| Country | Round 1 2050 GDP/c | Round 2 2050 GDP/c | Change |
|---|---:|---:|---:|
| United States | $125,817 | $126,602 | +0.6% |
| Canada | $249,720 | $230,260 | -7.8% |
| Australia | $167,798 | $164,061 | -2.2% |
| Germany | $108,490 | $107,669 | -0.8% |
| United Kingdom | $96,540 | $96,027 | -0.5% |
| China | $71,352 | $70,500 | -1.2% |
| India | $24,928 | $25,311 | +1.5% |

Canada has zero direct vanilla climate exposure in the installed 2022 region
templates, so its change cleanly displays the moderated technology trajectory.
For the other countries, the milder climate path offsets part of that reduction.

The canonical factor-shape test uses a `$4T`, `100M`-person economy with two
Resource regions and otherwise reference conditions. Doubling capital alone
raises new GDP by `1.30x` at starting technology, then `1.36x`, `1.50x`, and
`1.70x` at 25%, 50%, and 75% weighted progress, and `1.93x` at maximum
technology. Doubling capital, population, and resources together remains
exactly scale-neutral at `2x`.

### Round 3: Punctual constraint and climate adjustments

Only two proposal constants changed:

```text
labor knee                    $40,000 -> $37,500 GDP/c
climate GDP damage multiplier     1.00 -> 0.90
```

The lower labor knee raises labor pressure at every GDP-per-capita level, with
the largest practical penalty where effective labor support is scarce. The
climate multiplier weakens GDP loss by 10% for a given temperature, Inequality,
and regional exposure. Vanilla and the deployed benchmark remain at `1.00`.

At the default 2050 assumptions, the two changes partially offset for
climate-exposed countries: the US projection moves from `$126.6k` to `$130.8k`,
Australia from `$164.1k` to `$167.3k`, and Germany from `$107.7k` to `$109.9k`.
Canada receives no direct TI climate damage in its starting regions, so it
isolates the stronger labor constraint and falls from `$230.3k` to `$226.3k`.

## Maintenance Rule

The Round 3 constants are the implemented 0.7.0 defaults. Technology
productivity is now authored from all 149 installed global technologies and
normalized to the simulator's 3.40x full-tree target; the two completed 2022
technologies retain their exact 1.0201x product and do not advance either
future substitution axis.

The simulator is successful only if another session can reproduce both the
numbers and the reasoning without relying on conversation history. Whenever a
formula or default changes, update all of:

1. `tools/economy-growth-simulator.js`
2. `docs/economy-growth-simulator.md`
3. `docs/economy-growth-calibration-plan.md`
4. `TIEconomyMod/Patches/EconomyPatches.cs` and the default settings
5. Formula tests and the implementation matrix
6. The interactive comparison

Do not copy the simulator into gameplay as an abstraction layer. The C# patch
must continue to inline the accepted formula, use the same evaluation order,
and explain representative numerical effects directly beside the math.
