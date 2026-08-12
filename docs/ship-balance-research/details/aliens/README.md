# Alien fleet research

Last reviewed: 2026-08-12

The [fleet and module audit](fleet-and-module-audit.md) inventories alien
hulls, power plants, drives, weapons, utilities, and predefined designs.
The later
[magnetic, propulsion, and armor-design proposal](../../alien-weapons-propulsion-and-armor-proposal.md)
defines candidate numerical tiers and records the decision to leave alien AI
delta-v targets unchanged in the first propulsion slice.

## Current synthesis

The alien catalog contains strong late technology, but predefined ships do not
consistently use it. The audit found reactor/drive cap mismatches, continued
use of base magnetic weapons, non-use of the strongest reactor and battery,
thin armor on several line combatants, uneven point-defense coverage, and
template defects.

The player's campaign hypothesis is preserved separately from the template
findings:

- early missile saturation may exploit ineffective alien laser point defense;
- later fleets may compound that weakness with underpowered magnetic weapons;
- improving laser-PD behavior, assigning proper Advanced/Gen3 magnetic
  weapons, repairing power-plant pairings, and rearranging modules may matter
  more than a uniform numerical buff.

The alien magnetic progression is implemented in version 0.9.1. A conservative
0.9.2 slice is implemented with `1.2 / 3.8 / 10.5 MN` drives, `1,200 / 2,350 /
3,000 km/s` exhaust velocity, five-times-current reactor output, half-current
reactor specific mass, and a `3,500 -> 10,000 kg/m3` armor-allocation scalar.
Strict reactor-cap selection and predefined-loadout normalization remain
follow-up work. Lower AI delta-v targets remain explicitly deferred pending
generated-design, equal-cost combat, and campaign tests.
