# T1 hab monthly demand realism audit

Date: 2026-08-17  
Game data: Terra Invicta 1.0.51, Steam build 24479907  
Mod data: Economic Equalization Overhaul 0.9.2

## Scope and conclusion

This is a research and planning audit, not a balance implementation.

Subsequent planning decision: the T1 pass based on this audit is approved for
later implementation, with two gameplay overrides. Direct power generators
will represent two former plant units by doubling output, maintenance, crew,
and construction resource/boost cost, subject to the final maintenance caps.
No new resource requirements will be added to a module that lacks them, no
existing resource type will be removed, and maintenance will not rise unless
the unscaled realistic estimate already exceeds vanilla. This general rule
supersedes the earlier farm-specific exception. See
[`t1-monthly-demand-scaling-proposal.md`](t1-monthly-demand-scaling-proposal.md)
for the approved table.

The current T1 demands are not physically credible as recurring maintenance.
The largest entries behave as though installed inventory is discarded and
replaced every month. The underlying design error is not merely that the
numbers are high: fixed support demand currently mixes four different things
that should have different models:

1. crew consumables;
2. leakage, wear, and replacement parts;
3. experiment or industrial feedstock that scales with use; and
4. construction material that is already charged when something is built.

For the current mod, vanilla 1.0.51 still supplies `supportMaterials_month`.
The mod changes T1 mass and crew but does not change that field. Crew life
support is then added by game code, separately from the template's support
materials.

One space-resource point equals 10 metric tonnes:

```text
physical tonnes = resource points / spaceResourceToTons
                = resource points / 0.1
                = resource points * 10
```

The game's per-crew burden is 3.5 t water/year plus 3.5 t
volatiles/year, or 0.583 t/month per crew member.

## ISS baseline

NASA describes the ISS as a 419.725 t station with a normal crew of seven and
75-90 kW from eight solar arrays. It is therefore an unusually conservative
comparison for small T1 modules: the ISS is much larger, older, and more
complex than one module, yet all of its crew supplies, experiments, spares,
replacement hardware, and some propulsion logistics share the same cargo
stream. [NASA ISS facts and figures](https://www.nasa.gov/international-space-station/space-station-facts-and-figures/)

Two cargo bounds are useful:

- NASA's CRS-2 planning requirement was 14.25-16.75 t/year of pressurized
  cargo plus 1.5-4 t/year unpressurized: 15.75-20.75 t/year, or
  1.31-1.73 t/month, for the US commercial segment.
- A deliberately generous 2013 manifest-capacity calculation that adds the
  listed Dragon, Cygnus, Progress, ATV, and HTV flights is approximately
  40.4-41.8 t/year, or 3.37-3.48 t/month, for the whole station. This is
  transport capacity, not maintenance consumption, and includes food, water,
  propellant, new experiments, packaging, and upgrades.

Source: [NASA OIG cargo vehicle and annual requirement tables](https://oig.nasa.gov/wp-content/uploads/2024/02/ig-13-019.pdf)
and [NASA OIG CRS-2 requirements](https://oig.nasa.gov/docs/IG-14-031.pdf).

Consequently, a single T1 module demanding even 3.5 t/month already consumes
the equivalent of a generous historical cargo stream for the entire ISS.

### Crew consumables

NASA's real-life values give a more direct crew comparison:

| Per person | Game | ISS reference | Game/reference |
|---|---:|---:|---:|
| Water, gross use | 292 kg/month | about 115 kg/month | 2.5x |
| Water, net makeup at 98% recovery | 292 kg/month | about 2.3 kg/month | 127x |
| Volatiles versus food + oxygen | 292 kg/month | about 81 kg/month | 3.6x |
| Combined one-way supply | 583 kg/month | about 84 kg/month | 7.0x |

The ISS gross-water figure follows NASA's one gallon per crew member per day.
Its US-segment system has demonstrated 98% recovery. Food plus packaging is
about 1.83 kg/person/day, and metabolic oxygen is about
0.84 kg/person/day. [NASA water recovery milestone](https://www.nasa.gov/missions/station/iss-research/nasa-achieves-water-recovery-milestone-on-international-space-station/),
[NASA food-system evidence report](https://ntrs.nasa.gov/api/citations/20160011582/downloads/20160011582.pdf),
and [NASA oxygen-generation reference](https://ntrs.nasa.gov/api/citations/20110015921/downloads/20110015921.pdf?attachment=true).

The combined comparison is intentionally conservative: it assumes oxygen is
supplied as new mass even though the ISS generates oxygen from recovered water.
Even an early T1 system recovering only 90% of water would need roughly
93 kg/person/month for water makeup, food, packaging, and oxygen, still about
6.3 times below the game value.

## Current merged T1 audit

`Bulk` is template water, volatiles, metals, noble metals, and fissiles
converted to tonnes. `Crew` is the additional game-generated life-support
mass. `Boost eq.` converts boost's deka-ton launch-capacity unit to tonnes but
keeps it separate from physical cargo. `Annual/mass` is annual bulk plus crew
throughput divided by installed module mass; it excludes boost.

| Module | Mod mass (t) | Mod crew | Bulk (t/mo) | Crew (t/mo) | Boost eq. (t/mo) | Total (t/mo) | Annual/mass |
|---|---:|---:|---:|---:|---:|---:|---:|
| Administration Node | 45 | 4 | 0.90 | 2.33 | 10.0 | 13.23 | 86% |
| Antimatter Trap | 30 | 1 | 1.30 | 0.58 | 0 | 1.88 | 75% |
| Automated Fission Pile | 45 | 0 | 5.50 | 0 | 0 | 5.50 | 147% |
| Automated Mining Complex | 375 | 0 | 25.00 | 0 | 0 | 25.00 | 80% |
| Automated Outpost Core | 30 | 0 | 0 | 0 | 0 | 0 | 0% |
| Automated Platform Core | 30 | 0 | 0 | 0 | 0 | 0 | 0 | 0% |
| Automated Solar Collector | 25 | 0 | 0 | 0 | 0 | 0 | 0% |
| Automated Solar Mirror | 75 | 0 | 1.30 | 0 | 0 | 1.30 | 21% |
| Automated Supply Depot | 25 | 0 | 0 | 0 | 0 | 0 | 0% |
| Broadcast Outlet | 45 | 1 | 0 | 0.58 | 0 | 0.58 | 16% |
| Climate Lab | 30 | 1 | 0 | 0.58 | 0 | 0.58 | 23% |
| Construction Module | 45 | 2 | 52.50 | 1.17 | 0 | 53.67 | 1,431% |
| Energy Lab | 30 | 1 | 10.01 | 0.58 | 0 | 10.59 | 424% |
| Fission Pile | 30 | 1 | 8.00 | 0.58 | 0 | 8.58 | 343% |
| Fusion Pile | 45 | 1 | 15.20 | 0.58 | 0 | 15.78 | 421% |
| Heavy Fission Pile | 60 | 1 | 8.25 | 0.58 | 0 | 8.83 | 177% |
| Heavy Fusion Pile | 90 | 2 | 17.20 | 1.17 | 0 | 18.37 | 245% |
| Hydroponics Bay | 180 | 1 | 0 | 0.58 | 0 | 0.58 | 4% |
| Information Science Lab | 30 | 1 | 0 | 0.58 | 0 | 0.58 | 23% |
| Life Science Lab | 30 | 1 | 10.00 | 0.58 | 0 | 10.58 | 423% |
| Listening Post | 40 | 2 | 0 | 1.17 | 0 | 1.17 | 35% |
| Marine Platoon Barracks | 180 | 30 | 21.00 | 17.50 | 0 | 38.50 | 257% |
| Materials Lab | 30 | 1 | 2.00 | 0.58 | 0 | 2.58 | 103% |
| Military Science Lab | 30 | 1 | 0 | 0.58 | 0 | 0.58 | 23% |
| Outpost Core | 30 | 2 | 0 | 1.17 | 0 | 1.17 | 47% |
| Outpost Mining Complex | 375 | 4 | 25.00 | 2.33 | 0 | 27.33 | 88% |
| Particle Collider | 300 | 2 | 70.00 | 1.17 | 0 | 71.17 | 285% |
| Platform Core | 25 | 2 | 0 | 1.17 | 0 | 1.17 | 56% |
| Point Defense Array | 150 | 1 | 2.00 | 0.58 | 0 | 2.58 | 21% |
| Quarters | 90 | 1 | 2.00 | 0.58 | 0 | 2.58 | 34% |
| Social Science Lab | 25 | 1 | 0 | 0.58 | 0 | 0.58 | 28% |
| Solar Collector | 25 | 0 | 0 | 0 | 0 | 0 | 0% |
| Solar Mirror | 75 | 0 | 1.30 | 0 | 0 | 1.30 | 21% |
| Space Dock | 120 | 4 | 11.00 | 2.33 | 0 | 13.33 | 133% |
| Space Science Lab | 30 | 1 | 0 | 0.58 | 0 | 0.58 | 23% |
| Supply Depot | 25 | 0 | 0 | 0 | 0 | 0 | 0% |
| Tourist Berth | 25 | 2 | 0 | 1.17 | 2.0 | 3.17 | 56% |
| Xenology Lab | 30 | 1 | 0 | 0.58 | 0 | 0.58 | 23% |

Money is omitted because it has no defensible conversion to physical material
without first defining what one game-money unit represents.

## First-principles findings by system

### Particle collider: 70 t/month is physically indefensible

The template consumes 20 t water, 20 t volatiles, 10 t metals, 10 t noble
metals, and 10 t fissiles every month. Accelerated particles do not require
bulk feedstock. For a deliberately extreme 10 MW continuous proton beam at
1 GeV, the beam contains only about 0.27 grams of protons per month. At a much
lower 1 MeV, the same enormous beam power would use about 0.27 kg/month.
Targets, detector gas, filters, cryogenic losses, and failed electronics could
raise total consumables to kilograms or perhaps low tonnes for an unusually
wasteful design, but not 70 t/month. Coolant, shielding, magnets, and vacuum
hardware are circulating or installed inventory, not monthly losses.

The closest ISS comparison, AMS-02, is an 8.5 t, 2.5 kW particle-physics
instrument designed to operate for the ISS lifetime. It required a major
cooling-system repair after years, not replacement of its own mass every
month. [NASA AMS-02 facts](https://www.nasa.gov/international-space-station/alpha-magnetic-spectrometer-ams-02/)
and [NASA AMS repair history](https://heasarc.gsfc.nasa.gov/docs/heasarc/missions/ams.html).

The game's particle collider alone demands about 21 entire ISS cargo streams.
Its annual 854 t bulk-plus-crew throughput is over twice the present ISS mass.

### Space dock: tools are capital, not consumables

The dock consumes 10 t common metal and 1 t noble metal each month before
crew support. Cutting inserts, seals, lubricants, filters, welding wire, and
failed machine components are credible demands, but a machine shop does not
discard its lathes, robot arms, pressure vessels, and tooling stock monthly.

For scale, the ISS Harmony utility and docking node is 14.8 t. The game's dock
discards nearly one Harmony-equivalent of metal every month and more than its
own 120 t mass each year. [NASA Harmony module](https://www.nasa.gov/international-space-station/harmony-module/)

A credible idle 120 t orbital dock might replace roughly 0.05-0.3% of its dry
mass per month: about 0.06-0.36 t/month of mixed parts. Material incorporated
into a new ship or a major refit belongs in that ship/refit cost and should
scale with work performed.

### Construction module: fixed demand double-counts production

The module consumes 52.5 t/month even while idle: 10 t water, 10 t volatiles,
30 t metals, and 2.5 t noble metals. That is 630 t/year for a 45 t machine,
before crew supplies. It is not maintenance; it is industrial feedstock.

The ISS Mobile Servicing System uses Canadarm2, Dextre, and a mobile base to
assemble, handle, and maintain very large objects. Its major robotic elements
are tonne-scale capital equipment, and parts are replaced when they fail.
[NASA ISS utilization guide](https://www.nasa.gov/sites/default/files/atoms/files/np-2015-05-022-jsc-iss-guide-2015-update-111015-508c.pdf)

If the game intends the construction module to manufacture projects, its
water, volatile, and metal use should be a small loss or process overhead
applied to actual construction mass. The constructed object's resource cost
already pays for incorporated material.

### Fission piles: fuel demand is tens to hundreds of times too high

Each ordinary or automated pile burns 0.5 t of `fissiles` per month. NRC's
current commercial-fuel burnup limit is roughly 62 GW-day per metric tonne of
uranium. A 10 MW thermal reactor therefore consumes only about 4.8 kg of heavy
metal per month at that burnup; a 10 MW electric reactor at 33% efficiency
uses about 14.5 kg/month. The game value is about 34-103 times those already
conservative heavy-metal figures. If `fissiles` means the fissile isotope
rather than total enriched fuel, the discrepancy is larger.
[NRC burnup reference](https://www.nrc.gov/reactors/power/atf/technologies/burnup)

The additional 5 t/month water and 2.5 t/month volatiles in a crewed pile are
also inventory rather than demand. A space reactor's primary coolant must be
closed-loop; its radiator rejects heat without throwing coolant away.

### Fusion piles: energy, not bulk fuel, should dominate

There is no ISS or commercial fusion counterpart, so only an energy balance
is defensible. D-T fusion releases roughly 3.4e14 J/kg of combined fuel. A
10 MW thermal source running continuously uses about 0.08 kg/month; even a
30 MW thermal source uses about 0.23 kg/month before breeding and processing
losses. The current piles consume 0.2 t/month of `fissiles` plus 15 t/month of
water and volatile material. That is not reactor fuel burn; it again treats
closed-loop working inventory as disposable.

### Laboratories: inconsistent and generally far too high

Real ISS laboratory modules are approximately 10-16 t: Destiny is 14.5 t,
Columbus 10.3 t, and Kibo's pressurized lab 15.9 t. The T1 lab masses of
25-30 t are conservative, but their support is incoherent:

- information, climate, military, social, space, and xenology labs have no
  bulk experiment feedstock;
- the materials lab discards 1 t metal plus 1 t noble metal per month;
- the life-science lab discards 5 t water plus 5 t volatiles per month; and
- the energy lab discards 10 t volatiles per month.

The last two consume nearly a complete real ISS laboratory module each month.
Credible research feedstock is experiment-dependent and normally kg- to
hundreds-of-kg scale per month. It should scale with research activity, while
generic rack, pump, filter, and electronics spares should scale with installed
mass and failure rate. [NASA Destiny](https://www.nasa.gov/international-space-station/destiny-laboratory-module/),
[NASA Columbus](https://www.nasa.gov/international-space-station/columbus-laboratory-module/),
and [NASA Kibo](https://www.nasa.gov/international-space-station/japanese-experiment-module-kibo/).

### Mining complexes: possibly high throughput, but the present model is wrong

Twenty tonnes of water and five tonnes of volatiles per month could be
plausible only as process losses from a very large mine. Unlike a lab or dock,
a mine may handle hundreds or thousands of tonnes of regolith, consume drill
bits, lose volatiles, and discard tailings. The correct comparison is therefore
not ISS upkeep but mine output.

The current 25 t/month charge is fixed, independent of extracted tonnage,
ore, gravity, process, or utilization. It is 300 t/year, 80% of the automated
mine's installed mass, and the crewed mine adds another 28 t/year of inflated
life support. A physical model should use a small consumables fraction of
actual throughput, plus a much smaller fixed machinery-spares term.

### Solar collectors and mirrors: one is credible, one is not

The crewless solar collector has no monthly material demand, which is a good
first approximation. Solar arrays degrade and occasionally need electronics,
batteries, or blanket repair, but they do not steadily consume structural
metal.

The solar mirror consumes 1.0 t metal and 0.3 t noble metal per month. That is
15.6 t/year or 21% of its 75 t installed mass annually. ISS array and truss
elements installed from 2000 through 2009 served for decades; later roll-out
arrays augmented them rather than replacing one-fifth of their mass every
year. [NASA integrated truss history](https://www.nasa.gov/international-space-station/integrated-truss-structure/)

### Quarters and hydroponics point in opposite directions

Quarters already pay crew life support but also consume another 1 t water and
1 t volatiles each month. Unless the module houses additional uncounted
residents, this duplicates the same requirement. The hardware itself should
need filters, cleaning supplies, textiles, and occasional spares, measured in
kilograms to low hundreds of kilograms per month.

Hydroponics has no bulk support at all. A closed system can recycle most water,
but it still needs nutrient replacement, seeds, filters, lamps, pumps, growth
substrate, and compensation for imperfect closure. Zero is as suspicious as
the other entries are excessive.

### Administration, military, and tourism

- The Administration Node consumes 10 t/month-equivalent of boost plus
  0.9 t/month of bulk material and four inflated crew loads. An office and
  communications center should be electronics-spares dominated; a standing
  ten-tonne monthly launch allocation is larger than a typical whole-station
  cargo month.
- Thirty marines require about 1.67 t/month of packaged food, 0.77 t/month of
  oxygen if none is regenerated, and about 0.07 t/month net water makeup at
  98% recovery. The game charges 17.5 t/month crew support plus 21 t/month
  module support. Training ammunition and combat losses should be event-driven.
- A Tourist Berth's 2 t/month boost allocation is defensible only if it means
  continuous passenger turnover. If the occupants are resident, transport is
  a one-time arrival/departure cost rather than upkeep.

## Planning model for a later rebalance

The physical model should preserve gameplay abstractions but separate causes:

1. **Crew water:** choose an explicit closure level. A rugged early T1 system
   at 90% recovery needs about 0.14 t/person/year of makeup water; 98% recovery
   needs about 0.028 t/person/year. The current value is 3.5 t/person/year.
2. **Crew volatiles:** food and packaging establish about 0.67 t/person/year.
   Adding unrecovered oxygen gives roughly 0.97 t/person/year. The current
   value is 3.5 t/person/year.
3. **Fixed spares:** use installed mass times a reliability allowance. A
   planning band of 0.5-3% of dry mass per year is conservative; even a severe
   12%/year case is only 1%/month.
4. **Process consumables:** laboratories, mines, hydroponics, and antimatter
   systems should consume kg- or throughput-scaled feedstock appropriate to
   their task. Coolant inventory is paid at construction; only leakage is
   upkeep.
5. **Fuel:** derive fission/fusion fuel from thermal output, duty cycle,
   efficiency, and burnup, not module mass.
6. **Construction and dock materials:** charge incorporated material to the
   object built or repaired. Fixed upkeep covers only tooling wear and failed
   parts.
7. **Combat and transport:** ammunition, battle damage, passenger launch, and
   vehicle propellant should be event-driven rather than an idle monthly tax.

For a 30 t T1 module, a 2% annual hardware replacement allowance is only
0.05 t/month, or 0.005 resource points/month. That illustrates the scale
problem: several current entries are 1-7 resource points per month, hundreds
to more than a thousand times a normal fixed-spares allowance.

## Bottom line

The user's intuition is correct. A space dock can consume metal and a particle
accelerator can consume gas, targets, filters, and coolant makeup. What is not
credible is treating the installed machine inventory as a monthly flow. The
current data make several individual T1 modules consume more mass per month
than the entire ISS historically receives, and make several modules process
one to fourteen times their own dry mass every year while idle.

The next balance pass should therefore change the model before tuning exact
numbers: drastically lower fixed support, correct per-crew closed-loop
consumption, and move industrial feedstock, construction material, ammunition,
and passenger transport to activity-dependent costs.
