# Mk1 railgun progression and conventional-gun power feasibility

Last reviewed: 2026-07-31  
Game data: installed Terra Invicta 1.0.49 templates

## Status and scope

This is a planning report, not an implementation record. It compares the three
human Mk1 railguns with their same-mount chemical predecessors after applying
the settled conservative conventional-projectile values. It also records the
human Mk1-Mk2 magnetic cadence decision and audits whether ordinary guns can use
the power, battery, heat, and user-interface path already used by lasers and
magnetic guns.

The revised magnetic-gun CSV proposal halves cooldown and intra-salvo cooldown
for human Mk1 and Mk2 railguns and coilguns only. Human Mk3 and alien rows are
untouched by this pass. Projectile mass, ammunition mass, velocity, range,
efficiency, and weapon mass remain vanilla on every affected row.

## Conventional planning baseline

The 30mm and 40mm decisions halve effective projectile mass but raise cadence
above the earlier near-throughput-neutral targets so the CIWS pass does not
strengthen already-powerful early missiles. The larger chemical guns instead
move toward real full-caliber projectile masses.

Velocity remains unchanged in this pass. Each changed field below includes its
vanilla baseline, planning target, and absolute delta; percentages describe the
change from vanilla.

| Weapon | Damaging mass: vanilla -> planned (delta) | Effective rate: vanilla -> planned (delta) | Impact: vanilla -> planned (delta) | Sustained output: vanilla -> planned (delta) |
|---|---:|---:|---:|---:|
| 30mm Autocannon | 3.5 -> 1.75 kg (-1.75; -50%) | 70.59 -> 180.00 rpm (+109.41; +155.0%) | 3.19 -> 1.59 MJ (-1.59; -50%) | 3.75 -> 4.78 MW (+1.03; +27.5%) |
| 40mm Autocannon | 6 -> 3 kg (-3; -50%) | 46.45 -> 100.00 rpm (+53.55; +115.3%) | 20.28 -> 10.14 MJ (-10.14; -50%) | 15.70 -> 16.90 MW (+1.20; +7.6%) |
| 6-inch Gun Battery | 22.5 -> 40 kg (+17.5; +77.8%) | 13.33 -> 13.33 rpm (0) | 22.05 -> 39.2 MJ (+17.15; +77.8%) | 4.90 -> 8.71 MW (+3.81; +77.8%) |
| 8-inch Gun Battery | 50 -> 90 kg (+40; +80%) | 10.67 -> 10.67 rpm (0) | 49.00 -> 88.2 MJ (+39.2; +80%) | 8.71 -> 15.68 MW (+6.97; +80%) |
| 10-inch Cannon | 90 -> 180 kg (+90; +100%) | 8.18 -> 8.18 rpm (0) | 88.20 -> 176.4 MJ (+88.2; +100%) | 12.03 -> 24.05 MW (+12.02; +100%) |

The 40mm figure demonstrates why sustained kinetic MW cannot be used alone to
rank these weapons. Its stream consists of small packetized impacts. The naval
guns and railguns concentrate energy into much larger individual armor tests.

## Vanilla and currently proposed Mk1 railguns

The power columns follow Terra Invicta's current magnetic-gun formula. Input
energy is based on complete `ammoMass_kg`, while kinetic damage is based on
`warheadMass_kg`. Mk1 efficiency is 25%.

| Mount progression | Mk1 railgun version | Loaded mass | Slug | Velocity | Impact | Cooldown | Kinetic output | Electrical input | Weapon heat | Range |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| 6-inch / one hull | Vanilla Light Railgun Battery Mk1 | 62 t | 9 kg | 3.0 km/s | 40.50 MJ | 30 s | 1.35 MW | 7.20 MW | 5.40 MW | 500 km |
| 6-inch / one hull | Current CSV proposal | 62 t | 9 kg | 3.0 km/s | 40.50 MJ | 15 s | 2.70 MW | 14.40 MW | 10.80 MW | 500 km |
| 8-inch / two hull | Vanilla Railgun Battery Mk1 | 128.8 t | 18 kg | 3.3 km/s | 98.01 MJ | 30 s | 3.27 MW | 17.42 MW | 13.07 MW | 650 km |
| 8-inch / two hull | Current CSV proposal | 128.8 t | 18 kg | 3.3 km/s | 98.01 MJ | 15 s | 6.53 MW | 34.85 MW | 26.14 MW | 650 km |
| 10-inch / one nose | Vanilla Light Rail Cannon Mk1 | 66 t | 22.5 kg | 3.6 km/s | 145.80 MJ | 45 s | 3.24 MW | 17.28 MW | 12.96 MW | 550 km |
| 10-inch / one nose | Current CSV proposal | 66 t | 22.5 kg | 3.6 km/s | 145.80 MJ | 22.5 s | 6.48 MW | 34.56 MW | 25.92 MW | 550 km |

### Finding

The cadence-only CSV proposal is a modest improvement, not a universal raw
damage upgrade after the chemical-projectile pass:

- the light battery's 40.5 MJ hit is 3.3% above the planned 6-inch 39.2 MJ hit;
- the two-slot battery's 98.0 MJ hit is 11.1% above the planned 8-inch 88.2 MJ;
- the light rail cannon's 145.8 MJ hit remains 17.3% below the planned 10-inch
  176.4 MJ hit;
- the three Mk1 railguns deliver approximately 31%, 42%, and 27% of their
  respective chemical sibling's planned sustained kinetic output.

All three are comfortably above the planned 40mm's 10.14 MJ per rendered
projectile, but that is too low a bar for a new anti-ship weapon family. Their
range and the light rail cannon's mass are excellent; their impact and cadence
need another pass.

## Settled cadence-only Mk1-Mk2 proposal

The earlier larger-slug diagnostic candidate is superseded. The current
proposal retains vanilla projectiles and changes cadence only.

| Human family and mount group | Mk1 cooldown: vanilla -> proposed | Mk2 cooldown: vanilla -> proposed | Intra-salvo change | Full-cycle change |
|---|---:|---:|---:|---:|
| Rail batteries: light, standard, heavy | 30 -> 15 s | 20 -> 10 s | none; single shot | -50% |
| Rail cannons: light, standard, heavy, spinal | 45 -> 22.5 s | 30 -> 15 s | none; single shot | -50% |
| Coil batteries: light, standard, heavy | 40 -> 20 s | 30 -> 15 s | 10 -> 5 s | 60 -> 30 s (-50%) |
| Coil cannons: light, standard, heavy, spinal | 48 -> 24 s | 36 -> 18 s | 12 -> 6 s | 72 -> 36 s (-50%) |
| Siege coil cannons: heavy, spinal | 48 -> 24 s | 36 -> 18 s | Mk1 36 -> 18 s; Mk2 24 -> 12 s | Mk1 120 -> 60 s; Mk2 84 -> 42 s |

Because per-shot electrical input and efficiency stay vanilla while the full
cycle is halved, average electrical demand and weapon heat double for every
affected row. One-shot storage remains unchanged. No human Mk3 or alien row is
part of this cadence pass.

## Feasibility of gun power consumption

### Current class behavior

`TIGunTemplate` hardcodes all three relevant results:

- `selfPowered` returns `true`;
- `EnergyUsage_GJ(...)` returns zero;
- `HeatGeneration_GJ(...)` returns zero.

This means a JSON-only change cannot make the 30mm, 40mm, or naval guns consume
power. The `efficiency` field cannot overcome these method overrides.

Lasers and railguns use the generic non-self-powered weapon path. That path
already:

1. adds weapon generation demand to ship design;
2. adds one shot of required energy storage;
3. checks available reactor output and battery charge before firing;
4. deducts energy for every rendered shot;
5. applies weapon heat;
6. exposes per-shot energy use in the module UI;
7. includes weapon generation load in power-plant mass and reactor waste heat.

### Mod implementation route

A Harmony patch can opt any `TIGunTemplate` carrying a generic
`powerUse_MJ` JSON member into that existing path by overriding the three
hardcoded results above. Harmony cannot add a serializable field to the compiled
class, so a central extension adapter must bind the retained, load-ordered mod
JSON to the already loaded template objects. This avoids a weapon-name table
and lets inherited `efficiency` use the same input-and-heat semantics as lasers
and magnetic guns. See the dedicated power-patch plan for the startup-order,
UI-consumer, and save-load audits.

This is technically feasible and does not require writing a parallel battery or
combat-energy system. It is a moderate-risk patch rather than a data edit,
because several details need explicit policy:

- Chemical guns should receive only loader, mount-drive, control, and cooling
  energy, not their projectile kinetic energy. Propellant supplies the latter.
- The 40mm ETC weapon should add electrical energy for its velocity increment,
  conversion losses, and pulse-bank recharge.
- For salvo weapons, ship design sizes generation as energy per shot divided by
  intra-salvo spacing. This is burst-rate power, not full-cycle average power.
- Required storage is one shot per installed non-self-powered weapon. This is a
  useful approximation for lasers and railguns, but ETC pulse-bank sizing may
  merit a weapon-specific multiplier.
- The pre-fire heat-capacity check uses electrical input divided by power-plant
  efficiency rather than the weapon's `HeatGeneration_GJ` result. That existing
  behavior may reject shots more conservatively than intended and should be
  tested or patched before assigning large ETC pulses.
- Switching chemical guns to non-self-powered means a disabled power coupling
  or exhausted battery can prevent the loader from firing. That is mechanically
  plausible, but it is a gameplay change that should be intentional.

### Recommended division of loads

| Weapon family | Reactor-funded energy | Propellant-funded energy | Suggested implementation |
|---|---|---|---|
| 30mm chemical CIWS | feed, barrel drive, controls, cooling | muzzle energy | small fixed energy per packet; low heat |
| 6-10-inch chemical guns | autoloader, breech actuation, mount machinery, cooling | muzzle energy and most firing heat | fixed loader energy per rendered round; modest heat |
| 40mm ETC CIWS | feed and controls plus ETC pulse recharge and conversion losses | chemical share of muzzle energy | calculated or tabled ETC energy; substantial pulse storage and heat |
| Lasers and railguns | essentially the full shot input already modeled | none | retain vanilla class formulas |

### Conclusion

Giving guns ordinary electrical loads is practical with a compact Harmony
behavior patch plus a generic JSON-extension binding adapter, and it would
automatically use Terra Invicta's existing ship-design and combat systems. A
literal JSON-only edit remains insufficient because the compiled gun class has
no power field and hardcodes zero-use behavior.
The chemical calibers should consume kilowatt-to-sub-megawatt loader power,
whereas the 40mm ETC weapon can defensibly consume multi-megawatt or
tens-of-megawatts pulse-recharge power. The ETC gun is therefore the best first
test case, followed by modest fixed loader loads for the purely chemical guns.
