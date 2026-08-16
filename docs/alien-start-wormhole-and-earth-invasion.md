# Alien start, wormhole income, and Earth-invasion audit

Status: vanilla-behavior audit against installed Terra Invicta 1.0.51 and
Dark Skies; the Economic Equalization Overhaul does not change the behavior
described here

## Executive summary

- The 2022 and 2026 scenarios use the same alien space start: five
  `Silent Dawn` infiltrators carrying councilors, plus one `Quasar` destroyer.
  They also use the same fully built Alien HQ and Alien Station Alpha.
- The 2003 Dark Skies scenario selects `NoStartFleets`. It therefore starts
  with no alien ships, despite retaining the five modern fleet IDs in its
  `startingAlienCouncilorFleets` field. Its HQ and station are skeletal and
  initially have no shipyard. Alien construction goals fill them out later.
- The modern-start option **Add Alien Assault Carrier Fleet** adds three
  `Darkstar` assault carriers and one `Nova` frigate at Pluto. The game gives
  this fleet an invasion goal immediately. The option has no fleet template
  to instantiate in the 2003 scenario's `NoStartFleets` group.
- The modern wormhole operates at its full setup fraction from day one. Its
  fixed monthly resource line is 100 MC, 200 volatiles, 200 metals, 50 noble
  metals, 10 fissiles, 0.01 antimatter, and 10 base exotics. It supplies no
  water in the base template.
- Dark Skies changes the wormhole to include 200 water but starts its setup
  fraction at 15%. At the default 100% alien-progression setting, day-one
  output is therefore 15 MC; 30 each water, volatiles, and metals; 7.5 noble
  metals; 1.5 fissiles; and 0.0015 antimatter. Exotics also receive a
  difficulty multiplier.
- A normal 2022/2026 campaign does not create the first ordinary invasion goal
  until alien-adjusted campaign progress exceeds 12 years. The optional start
  fleet bypasses that wait. The 2003 scenario instead creates an unassigned
  invasion goal immediately but prevents departure while alien quietness is
  above 20%.
- An assault fleet only departs when it has enough normal space-combat value,
  can fulfill the fleet goal, has enough army-pod invasion value, and alien
  quietness is at or below 20%. Once it reaches an Earth interface orbit, it
  selects an undefended, army-free region using a weighted target score. Each
  eligible carrier is consumed to create a UFO landing; three alien armies
  appear 32 days later if the landing survives.

## Sources and interpretation boundaries

The static scenario and unit facts come from:

- base `TIStartTimeTemplate.json`, `TIMetaTemplate.json`,
  `TISpaceFleetTemplate.json`, `TISpaceShipTemplate.json`,
  `TIHabTemplate.json`, `TIHabModuleTemplate.json`, and
  `TIFactionTemplate.json` under the installed game's
  `TerraInvicta_Data/StreamingAssets/Templates` directory;
- the Dark Skies 2003 overrides of the same files under
  `DLC_Content/DarkSkies/2003_Scenario/Templates`.

Runtime formulas and gates were traced in `Assembly-CSharp.dll`, principally:

- `TIFactionState.NewCampaign` and `MonthlyFactionUpdate`;
- `TIGlobalConfig.AI_AliensWormholeSetupFraction`,
  `AI_AlienBaseQuietness`, `AI_GetExoticsMultiplier`, and
  `GetYearsUntilFirstAlienInvasionDifficultyScaling`;
- `TIGlobalValuesState.GetAlienProgressionModifiedDuration_*`;
- `AIDailyFactionPlanner.AliensCheckGoals`;
- `FactionGoal_InvadeEarth`;
- `AlienLandArmyOperation`;
- `AIEvaluators.SelectAlienArmyLandingRegion`;
- `TIRegionUFOLandingState.TriggerLanding` and `OnAlienArmyDeployed`.

The analysis distinguishes a template's fixed wormhole output from mining at
the randomly selected HQ site. Total alien income will also include that mine,
later bases, and other modules.

## Day-one comparison

| Asset | 2003 Dark Skies | 2022 | 2026 |
|---|---|---|---|
| Alien ships | None | 5 infiltrators + 1 destroyer | 5 infiltrators + 1 destroyer |
| Optional assault start | No corresponding fleet in `NoStartFleets` | +3 carriers +1 frigate | +3 carriers +1 frigate |
| Active alien councilors | 1, at the initial Earth crashdown | 6: 1 crashdown + 5 aboard infiltrators | Same as 2022 |
| Alien HQ | Skeletal base: colony core, reactor farm, wormhole | Fully built 21-module fortress/base | Same as 2022 |
| Alien Station Alpha | Ring core + reactor farm only | Core, shipyard, factory, garrisons, layered defenses, battlestation, reactors | Same as 2022 |
| Starting resource stockpile | Empty template list | Empty template list | Empty template list |
| Base faction MC income | 50/year | 50/year | 50/year |
| Starting alien project | `Project_AlienMasterProject` | Same | Same |
| Alien strategic intel | Full knowledge of space bodies | Same | Same |

The project and intel are established by alien-faction initialization rather
than scenario-specific data. The alien advanced master project is not a
day-one asset; it is awarded later by progression or alternate runtime
triggers.

### The modern six ships

| Fleet | Orbit | Ship | Purpose |
|---|---|---|---|
| `alienFleet12020` | Low Jupiter | Silent Dawn-class Infiltrator | Councilor transport |
| `alienFleet22020` | Low Saturn | Silent Dawn-class Infiltrator | Councilor transport |
| `alienFleet32020` | Low Uranus | Silent Dawn-class Infiltrator | Councilor transport |
| `alienFleet42020` | Low Neptune | Silent Dawn-class Infiltrator | Councilor transport |
| `alienFleet52020` | Low Pluto | Silent Dawn-class Infiltrator | Councilor transport |
| `alienFleet62020` | High Salacia | Quasar-class Destroyer | Interdictor/surveillance combatant |

The five infiltrators are the fleets named by the start-time template and each
receives one of the initial alien councilors. The Salacia destroyer is selected
by the modern scenario's fleet meta-group but is not a councilor start fleet.

### Optional assault fleet

When **Add Alien Assault Carrier Fleet** is enabled, the modern scenario also
instantiates `alienInvasionFleet72020` in High Pluto orbit:

- 3 Darkstar-class Assault Carriers (`Ship19`), each with an Alien Army Pod;
- 1 Nova-class Frigate (`Ship15`).

`TIFactionState.NewCampaign` detects the army-pod capability and assigns this
fleet an importance-19 `FactionGoal_InvadeEarth` immediately. This setting
therefore bypasses the ordinary first-invasion-goal timer, although the fleet
must still transfer from Pluto, survive, and meet the normal landing gates.

The 2003 scenario selects `NoStartFleets`, not `ModernStartFleets`. Its
start-time template's five modern fleet names do not instantiate those fleets;
initialization falls back to the scenario's quiet-period councilor count, which
is one on day one. This corrects the earlier statement in
`starting-technology-2003.md` that five alien councilor fleets exist at the
2003 start.

## Wormhole output

### Template output before runtime multipliers

| Monthly item | 2003 wormhole | 2022/2026 wormhole |
|---|---:|---:|
| Mission Control | 100 | 100 |
| Water | 200 | 0 |
| Volatiles | 200 | 200 |
| Metals | 200 | 200 |
| Noble metals | 50 | 50 |
| Fissiles | 10 | 10 |
| Antimatter | 0.01 | 0.01 |
| Exotics, before alien multiplier | 10 | 10 |

The modern template's missing water field resolves to zero. The Dark Skies
override explicitly adds 200 water per month.

The 100 MC is capacity/income returned by the same module-income function as
the resource line. Separately, the alien faction template supplies 50 MC per
year. Neither should be mistaken for a starting stockpile.

### Campaign settings that alter wormhole output

Let:

- `S` = the custom **Alien Progression Rate** multiplier (1.0 at the default
  slider position);
- `t` = elapsed calendar years;
- `M` = the scenario's alien progression modifier (0.75 in 2003, 1.0 in the
  modern starts);
- `W` = the difficulty's wormhole setup-speed factor.

The 2003 setup fraction is:

```text
setupProgress = clamp01(t * S^2 * M / (20 * W))
wormholeFraction = lerp(0.15, 1.50, setupProgress)
```

Difficulty supplies these factors:

| Difficulty | `W` | 2003 calendar years to 150% at `S = 1` |
|---|---:|---:|
| Cinematic | 1.50 | 40.00 |
| Normal | 1.00 | 26.67 |
| Veteran | 0.75 | 20.00 |
| Brutal | 0.50 | 13.33 |

The squared `S` is literal runtime behavior: progression speed multiplies
elapsed alien progress and also divides the nominal setup duration.

The modern start templates leave setup duration at its zero default, so
`AI_AliensWormholeSetupFraction` returns 1.0 immediately. Their non-exotic
wormhole line is therefore not changed by difficulty or progression rate.

Exotics receive an additional multiplier before the wormhole setup fraction:

```text
exotics/month = 10 * difficultyExoticsMultiplier * S * wormholeFraction
```

| Difficulty | Exotics multiplier | 2022/2026 exotics/month at `S = 1` | 2003 day-one exotics/month at `S = 1` |
|---|---:|---:|---:|
| Cinematic | 1 | 10 | 1.5 |
| Normal | 2 | 20 | 3.0 |
| Veteran | 3 | 30 | 4.5 |
| Brutal | 4 | 40 | 6.0 |

If the active player is an alien proxy, the code reverses the effective
difficulty (`5 - difficulty`) for wormhole setup and exotics. Thus the easier
human-facing setting is treated as the stronger alien-support setting for a
Servants-style player, and vice versa.

The separate **Alien Mining Rate** setting changes extraction at mines, not
the wormhole facility's fixed resource fields. Alien hab- and ship-construction
speed affect how quickly the aliens can turn income into infrastructure and
fleets, but do not change the fixed wormhole line.

### 2003 day-one fixed wormhole line

At `t = 0`, the setup fraction is 0.15 on every difficulty. At `S = 1`, before
adding any other alien income, it yields:

| MC | Water | Volatiles | Metals | Nobles | Fissiles | Antimatter |
|---:|---:|---:|---:|---:|---:|---:|
| 15 | 30 | 30 | 30 | 7.5 | 1.5 | 0.0015 |

Use the prior difficulty table for exotics.

## When an invasion goal exists

### Ordinary 2022/2026 start

Without the optional assault fleet, the alien daily planner creates the first
invasion goal when adjusted alien progress is strictly greater than the
difficulty threshold:

```text
adjustedProgressYears = max(0, startingProgressYears + t * S * M)
```

For both modern starts, `startingProgressYears = 0` and `M = 1`.

| Difficulty | Threshold | Default-rate implication |
|---|---:|---|
| Cinematic | 16 years | Just after 16 campaign years |
| Normal | 12 years | Just after 12 campaign years |
| Veteran | 6 years | Just after 6 campaign years |
| Brutal | 0 years | First daily check after progress becomes positive |

At Normal and `S = 1`, this means roughly October 2034 for the 2022 start and
February 2038 for the 2026 start. This is goal creation, not the landing date;
fleet design, construction, assembly, transfer, combat, and target preparation
still follow.

### 2003 quiet start

Because the 2003 template has a positive quiet duration, alien initialization
creates an unassigned invasion goal on day one. `ReadyForTransferToTarget`
then blocks departure while alien quietness exceeds 0.2.

Ignoring human actions that reduce quietness, 2003 quietness is:

```text
quietness = 1 - clamp01(t * S^2 * 0.75 / (15 * Q))
```

where `Q` is 1.20 Cinematic, 1.00 Normal, 0.85 Veteran, or 0.70 Brutal.
The earliest quietness-only transfer gates at the default rate are therefore:

| Difficulty | Years until quietness <= 0.2 | Approximate campaign date |
|---|---:|---|
| Cinematic | 19.2 | June 2022 |
| Normal | 16.0 | March 2019 |
| Veteran | 13.6 | November 2016 |
| Brutal | 11.2 | June 2014 |

These are lower bounds, not scheduled landings. The 2003 aliens begin without
a shipyard or ships, must complete their HQ/station buildup, construct and
assemble a sufficient invasion fleet, travel to Earth, and handle defenses.
Human milestones and actions can reduce quietness earlier than this baseline.

## How the assault fleet decides to depart and land

### 1. Build and assembly requirement

An invasion goal's initial desired invasion value is:

```text
3 armies * 50 assault value = 150
```

Each Alien Army Pod represents the three armies produced by one surviving
carrier landing. After the aliens have lost more than three invasion armies,
the goal adds 50 desired assault value for each additional loss, capped at 12
increments. Desired value therefore ranges from 150 to 750, equivalent to one
through five carrier loads.

The goal orders `ArmyCarrier` as its primary role and gives secondary weight to
escort/interdiction/space-combat roles. A fleet is ready to transfer only if:

1. alien quietness is no greater than 0.2;
2. its ordinary space-combat value meets the goal's requested combat value;
3. `CanFulfillGoal` succeeds; and
4. its invasion value meets the desired assault value.

### 2. Prepare a viable Earth target

Every eighth calendar day, an invasion goal checks whether any legal landing
target exists. If none exists, it selects a preferred region while temporarily
allowing hostile anti-space defenses. If the selected region has such defenses
and no equivalent attack goal already exists, it creates a fleet-attack goal
against that region's space-defense facility.

Thus the AI can deliberately suppress defenses before attempting to land; it
does not simply choose a random defended region and sacrifice the carrier.

### 3. Enter an Earth interface orbit

`AlienLandArmyOperation.CanLandArmy` requires the fleet to be:

- not assigned to a transfer;
- not in or waiting for combat;
- in an interface orbit;
- orbiting Earth; and
- carrying at least one ship with the Alien Army Pod special rule.

### 4. Select the landing region

Normal operation selection calls `SelectAlienArmyLandingRegion(false)`, which
excludes regions with hostile anti-space defenses. A region is also excluded
if it already has a UFO landing, already has an alien fleet operation assigned
to it, or contains any armies.

Positive candidates are chosen randomly with weights that strongly favor:

- an existing Alien Nation region: weight 1,000,000;
- a nation allied with the Alien Nation: base weight 10,000;
- a region adjacent to the Alien Nation, once it exists: base weight 10,000;
- regions with stage-three xenoforming: score x2;
- regions with an alien facility: score x2.

Other scoring behavior:

- if defenders exist elsewhere in the nation, add `1000 / national military
  strength`;
- if no such defenders exist, connectivity can add or subtract a small score;
- coastal regions have their score halved;
- nuclear-armed, non-permanent-alien-allied targets have their score multiplied
  by 0.1.

This makes the AI prefer reinforcement of an established alien foothold,
friendly proxy territory, or an undefended inland region with alien
infrastructure. It strongly dislikes nuclear targets and will not initiate the
operation into a region that already contains an army.

### 5. Consume the carriers and create the landing

When the operation executes, every army-eligible ship in the fleet:

1. triggers a visible UFO landing with 1,200 HP in the selected region;
2. transfers any councilor passengers to that region;
3. is destroyed/consumed by the landing operation.

The landing schedules army deployment for 32 days later. If it remains extant,
it spawns three alien armies per carrier landing. The landing then completes
its buildup one day after the armies appear. If this is the first foothold, the
Alien Nation is established there and war/alliance state is updated.

## Practical reading of the settings

- **Difficulty** changes modern exotics, the modern first-invasion goal timer,
  2003 quietness duration, and 2003 wormhole ramp duration. It does not change
  the ordinary modern six-ship roster.
- **Alien Progression Rate** changes alien strategic clocks. In the quiet/setup
  formulas it has a squared timing effect and linearly changes exotics income.
- **Add Alien Assault Carrier Fleet** is the only examined campaign option that
  directly changes the modern day-one alien ship roster. It also gives those
  ships an invasion goal immediately.
- **Alien Mining Rate**, **Alien Hab Construction Speed**, and **Alien Ship
  Construction Speed** affect the economy-to-fleet pipeline after the start;
  they do not rewrite fixed wormhole resource fields or the ordinary modern
  starting roster.

