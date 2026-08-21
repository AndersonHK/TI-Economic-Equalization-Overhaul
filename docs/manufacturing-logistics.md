# Manufacturing Logistics

Status: implemented in Version 0.9.0 for Terra Invicta 1.0.51.

This document is the current authority for hab-module construction, hab
founding, and probe delivery. The system treats industrial facilities as a
faction-owned logistics network rather than a same-system permission flag.

## Player model

A factory manufactures at its own hab without a dock. To ship from that factory
to another hab, founding location, or probe destination, build an active dock or
shipyard on the same hab. Remote export is limited by the lower of the factory
tier and dock tier. A valid pair may serve destinations in any planetary system.

Earth is always the manufacturing root of last resort. Space industry reduces
Boost use; it does not make construction materials free.

## Hab construction choices

The two vanilla choices remain distinct.

### From Earth

- Buy every construction material with Money.
- Pay Boost for the complete payload from Earth's surface.
- Use the normal Earth launch baseline, transfer, and any destination landing
  cost.
- Preserve the vanilla founding-duration limit for the full-Earth option.

### From space

Let `M` be total material mass and `E` the material shortage bought on Earth.

1. Reserve available construction materials from the faction stockpile.
2. Buy shortages `E` with Money and Boost them from Earth.
3. Count that Earth shipment toward the minimum transported share.
4. Dispatch `P = max(0, M / 3 - E)` tonnes from the selected factory.
5. For a non-Earth origin, pay Water/Volatiles propellant for `P` using the
   probe rocket equation.
6. If material or propellant resources are still short, apply the existing
   Earth purchase and Boost substitution rules.

Construction materials are reserved before freight propellant. Human hab
modules now charge resources equal to their complete modified physical mass,
including upgrade and irradiation modifiers, rather than charging only the old
weighted share.

For a simplified 30-ton payload whose resources are all substitutable:

| Stockpile available | Bought and Boosted from Earth | Additional factory dispatch |
|---:|---:|---:|
| 30 t | 0 t | 10 t |
| 25 t | 5 t | 5 t |
| 15 t | 15 t | 0 t |
| 0 t | 30 t | 0 t |

The factory dispatch is a freight requirement, not a second material charge.
The full 30 tonnes are paid once through stockpile debits and Earth purchases.

If an eligible space factory exists, Earth does not compete with it as another
origin. The lowest-propellant eligible space route wins. Earth supplies the
mandatory dispatched share only when no space origin can perform the job.

## Factory and dock tiers

| Factory | Local manufacturing | Remote export requirement |
|---|---|---|
| T1 Construction Module | T1 payloads | Active T1+ dock on the same hab |
| T2 Nanofactory | T1-T2 payloads | Same-hab dock/shipyard; lower tier caps export |
| T3 Nanofacturing Complex | T1-T3 payloads | Same-hab dock/shipyard/spaceworks; lower tier caps export |

The factory, dock, and containing hab must be completed, active, powered,
non-destroyed, non-decommissioning, and explicitly owned by the requesting
faction.

- A factory may serve its exact containing hab through its full factory tier.
- Without a dock, it cannot serve another hab at the same site, orbit, body, or
  system.
- Remote export tier is `min(highest factory tier, highest dock tier)` on that
  hab.
- Foreign, allied, neutral, inactive, or merely non-hostile facilities are not
  valid origins.

## Route selection and freight

Routes cover all endpoint combinations:

- orbit to orbit: transfer;
- surface to orbit: launch plus transfer;
- orbit to surface: transfer plus landing;
- surface to surface: launch, transfer, and landing.

Zero freight cost is allowed only when source and destination are the exact
same hab. Each surface body contributes its own launch or landing delta-v.
Earth's one-Boost-to-LEO convention is part of Earth's launch cost, not a
universal surcharge on Moon, Mars, or orbital manufacturing.

The resolver evaluates valid interface orbits and ranks eligible space origins
by:

1. lowest propellant requirement;
2. shortest transfer time;
3. stable hab identity.

For non-Earth freight:

```text
propellantMass = payloadMass × (exp(totalDeltaV / modifiedExhaustVelocity) - 1)
```

Propellant uses the existing probe Water/Volatiles proportions. A zero-delta-v
same-hab job consumes zero freight propellant.

Rocket projects change `modifiedExhaustVelocity`; they never change route
delta-v. Their vanilla `GenericTransferEV_kps` progression therefore reduces
Water/Volatiles propellant for space freight and Boost for any Earth-supplied
portion of a mixed delivery. Route and freight caches include the faction's
current off-window-time and exhaust-velocity effect values so a completed
project changes a quote immediately, without waiting for time to advance.

Trajectory time comes from the vanilla generic-transfer calculation.
Solar Steamers reduces only an applicable off-launch-window penalty inside that
calculation. Afterward, hab and module deliveries apply
`GenericModuleTransferTime` exactly once, including Space Tugs, Nuclear
Freighters, and Fusion Freighters.

## Founding habs

Habs may be founded by full Earth purchase and launch, a ship-carried kit, or
the mixed space-resource option. The mixed option is system-agnostic and remains
available through Earth fallback.

A remote space factory replaces Earth only when its dock-capped export tier can
manufacture the requested core. Technology, core-tier, Mission Control, survey,
site, orbital-capacity, and other non-logistics eligibility rules remain
vanilla. Construction facilities provide an alternative manufacturing and
delivery route; they do not create additional orbital capacity.

## Probes

Probes are full-payload T1 manufacturing jobs and site-targeted landed survey
drones. Each mission delivers exactly 0.325 tonnes to one selected hab site;
payload no longer scales with the number of sites on the parent body.

- The body-level Launch Probe action opens the native two-stage hab-site picker.
- Bulk launch buttons, including those shown on Mission-to-* research
  completion notifications, count, price, and launch one drone for every
  eligible unsurveyed site rather than one probe per accessible body.
- Native exploration and colonization eligibility still gate probe launches.
- A site with a probe in flight cannot receive a duplicate mission, but the
  player may send probes concurrently to different sites.
- Arrival reveals only the selected site's resources. The body becomes fully
  prospected after every child site is surveyed.
- The Earth option buys and Boosts the complete probe to the selected surface
  site, including landing delta-v.
- The space option requires an owned active T1 factory and T1 dock on the same
  hab and quotes the complete route through landing at the selected site.
- The entire probe payload travels from the selected origin.
- Material composition remains the vanilla probe composition.
- Non-Earth freight consumes Water/Volatiles propellant.
- Earth purchase and Boost substitution cover shortages.
- Rocket projects reduce probe freight propellant and Earth Boost through the
  vanilla generic-transfer exhaust velocity; they do not alter route delta-v.
- Solar Steamers reduces applicable off-window trajectory penalties, and High
  Thrust Probes applies `ProbeTransferTime` exactly once to the resulting flight
  time. Hab-module freighter effects do not apply to probes.
- Faction contribution to the applicable Mission-to-* global technology reduces
  the post-arrival single-site survey time; it does not change flight time or
  freight cost.
- A base may be founded only at a specifically surveyed site. Existing saves
  with body-level survey intel remain fully surveyed.
- Shipborne Survey Planet operations remain body-wide and reveal all sites.

With Cryogenic Liquid-Fuel Rockets' 4.44 km/s effective exhaust velocity, a
0.325-tonne Earth-launched lunar drone costs approximately 0.1452-0.1454 Boost
depending on site latitude. Surveying all 35 modded lunar sites costs about
5.0842 Boost, compared with 4.7352 Boost for the retired 18-tonne body-wide
orbital probe calculation.

## AI network planning

Vanilla already values docks for refueling and ship construction and gives
construction modules local-founding value. Version 0.9 adds an explicit pairing
priority: AI hab strategy strongly favors a candidate factory or dock when it
completes the first same-hab pair in a major colonized system.

Earth-Moon receives the strongest priority. Other colonized planet and
dwarf-planet systems use the normal priority; a non-planetary system qualifies
after the faction owns at least two habs there. The bonus stops when a pair is
present or already under construction, preventing duplicate infrastructure
spam. Existing refueling and shipyard priorities are left intact.

## Lazy caching and performance

Each faction has a runtime-derived source summary containing local factory tier,
dock tier, effective export tier, and probe capability for each hab.

Two cache layers separate topology from resource balances:

- route cache: faction, normalized destination, payload tier/profile, topology
  generation, and time generation;
- cost cache: selected route generation plus the relevant faction resource
  balance.

Time advancement or unpausing marks route timing and ranking stale. Hab or
module state changes mark source topology and routes stale. Resource spending
marks only cost results stale. No invalidation performs a network scan and no
daily/background rebuild exists.

The next caller recomputes only what is stale. A cold route request scans
eligible origins once, so it is O(number of eligible origins). Repeated tooltip
and planner requests are average O(1) with respect to origin count: a dictionary
lookup plus fixed-size resource arithmetic.

All registries, generations, and quotes are runtime-derived and rebuilt lazily
after loading, so Version 0.9 adds no serialized save state.

## Player-facing text

Module, project, operation, and Codex text use the same concise rules:

- factories manufacture locally;
- remote delivery requires a same-hab factory-dock pair;
- lower facility tier caps export;
- origins may be in any system;
- building the pair reduces Boost use;
- Earth is the fallback.

The Earth and space purchase buttons retain the native compact resource totals,
Boost, and duration. Fixed-height button labels do not include multiline route,
stockpile, freight, or propellant diagnostics. The underlying quote still tracks
those values for payment, planning, and caching.
