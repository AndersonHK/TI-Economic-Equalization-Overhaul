# DPS and fitting fallout of canonical art and optional tier moves

[Original DPS report](../turret-clearance/dps-analysis.md). Every art-only scenario preserves all 147 hull weapon templates, footprints, shot energies, cadences, mass, optics and ranges: **raw hull DPS delta = 0 for every class and every original family scenario**. More empty space is not permission to add guns. The previously proposed 25 mm does not exist. Nose DPS also remains unchanged.

## Optional heavy rail / heavy coil 4 → 2; 6-inch 1 → 2

Each family is tested separately. All Mk I/II/III heavy rail or coil variants change to `TwoHullHoriz`; in the Naval cannon rows only the 6-inch changes to `TwoHullHoriz`. Statistics stay fixed. Exact native grid packing chooses the highest sustained raw MJ/s mixed-size loadout within that family. Divide by 20 for damage points/s. Range/per-hit penetration and target maneuver are not modeled. These are slot-capacity outcomes, not endorsed balance settings. Capitals continue their current FourHull-only rule, exposing the incompatibility instead of silently allowing the demoted weapon. The combined rail-plus-naval option has these same separate family results.

| Class | Family | Original MJ/s | Retier MJ/s | Delta | Old guns | New guns | Old mass t | New mass t |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Gunship | RailgunBatteryMk1 | 0.000 | 0.000 | — | 0 | 0 | 0 | 0 |
| Gunship | RailgunBatteryMk2 | 0.000 | 0.000 | — | 0 | 0 | 0 | 0 |
| Gunship | RailgunBatteryMk3 | 0.000 | 0.000 | — | 0 | 0 | 0 | 0 |
| Gunship | CoilgunBatteryMk1 | 0.000 | 0.000 | — | 0 | 0 | 0 | 0 |
| Gunship | CoilgunBatteryMk2 | 0.000 | 0.000 | — | 0 | 0 | 0 | 0 |
| Gunship | CoilgunBatteryMk3 | 0.000 | 0.000 | — | 0 | 0 | 0 | 0 |
| Gunship | Naval cannon | 0.000 | 0.000 | — | 0 | 0 | 0 | 0 |
| Escort | RailgunBatteryMk1 | 11.812 | 11.812 | +0.0% | 2 | 2 | 100 | 100 |
| Escort | RailgunBatteryMk2 | 20.328 | 20.328 | +0.0% | 2 | 2 | 90 | 90 |
| Escort | RailgunBatteryMk3 | 39.690 | 39.690 | +0.0% | 2 | 2 | 80 | 80 |
| Escort | CoilgunBatteryMk1 | 26.133 | 26.133 | +0.0% | 2 | 2 | 100 | 100 |
| Escort | CoilgunBatteryMk2 | 61.653 | 61.653 | +0.0% | 2 | 2 | 90 | 90 |
| Escort | CoilgunBatteryMk3 | 130.021 | 130.021 | +0.0% | 2 | 2 | 80 | 80 |
| Escort | Naval cannon | 17.422 | 0.000 | -100.0% | 2 | 0 | 50 | 0 |
| Corvette | RailgunBatteryMk1 | 5.906 | 5.906 | +0.0% | 1 | 1 | 50 | 50 |
| Corvette | RailgunBatteryMk2 | 10.164 | 10.164 | +0.0% | 1 | 1 | 45 | 45 |
| Corvette | RailgunBatteryMk3 | 19.845 | 19.845 | +0.0% | 1 | 1 | 40 | 40 |
| Corvette | CoilgunBatteryMk1 | 13.067 | 13.067 | +0.0% | 1 | 1 | 50 | 50 |
| Corvette | CoilgunBatteryMk2 | 30.827 | 30.827 | +0.0% | 1 | 1 | 45 | 45 |
| Corvette | CoilgunBatteryMk3 | 65.010 | 65.010 | +0.0% | 1 | 1 | 40 | 40 |
| Corvette | Naval cannon | 8.711 | 0.000 | -100.0% | 1 | 0 | 25 | 0 |
| Frigate | RailgunBatteryMk1 | 11.812 | 11.812 | +0.0% | 2 | 2 | 100 | 100 |
| Frigate | RailgunBatteryMk2 | 20.328 | 20.328 | +0.0% | 2 | 2 | 90 | 90 |
| Frigate | RailgunBatteryMk3 | 40.824 | 56.348 | +38.0% | 1 | 1 | 80 | 160 |
| Frigate | CoilgunBatteryMk1 | 32.946 | 88.934 | +169.9% | 1 | 1 | 100 | 200 |
| Frigate | CoilgunBatteryMk2 | 69.344 | 180.000 | +159.6% | 1 | 1 | 90 | 180 |
| Frigate | CoilgunBatteryMk3 | 139.219 | 350.659 | +151.9% | 1 | 1 | 80 | 160 |
| Frigate | Naval cannon | 17.422 | 15.680 | -10.0% | 2 | 1 | 50 | 50 |
| Monitor | RailgunBatteryMk1 | 23.625 | 23.625 | +0.0% | 4 | 4 | 200 | 200 |
| Monitor | RailgunBatteryMk2 | 40.656 | 40.656 | +0.0% | 4 | 4 | 180 | 180 |
| Monitor | RailgunBatteryMk3 | 81.648 | 112.696 | +38.0% | 2 | 2 | 160 | 320 |
| Monitor | CoilgunBatteryMk1 | 65.892 | 177.868 | +169.9% | 2 | 2 | 200 | 400 |
| Monitor | CoilgunBatteryMk2 | 138.689 | 360.000 | +159.6% | 2 | 2 | 180 | 360 |
| Monitor | CoilgunBatteryMk3 | 278.438 | 701.319 | +151.9% | 2 | 2 | 160 | 320 |
| Monitor | Naval cannon | 34.844 | 31.360 | -10.0% | 4 | 2 | 100 | 100 |
| Destroyer | RailgunBatteryMk1 | 11.812 | 11.812 | +0.0% | 2 | 2 | 100 | 100 |
| Destroyer | RailgunBatteryMk2 | 20.328 | 20.328 | +0.0% | 2 | 2 | 90 | 90 |
| Destroyer | RailgunBatteryMk3 | 40.824 | 56.348 | +38.0% | 1 | 1 | 80 | 160 |
| Destroyer | CoilgunBatteryMk1 | 32.946 | 88.934 | +169.9% | 1 | 1 | 100 | 200 |
| Destroyer | CoilgunBatteryMk2 | 69.344 | 180.000 | +159.6% | 1 | 1 | 90 | 180 |
| Destroyer | CoilgunBatteryMk3 | 139.219 | 350.659 | +151.9% | 1 | 1 | 80 | 160 |
| Destroyer | Naval cannon | 17.422 | 15.680 | -10.0% | 2 | 1 | 50 | 50 |
| Cruiser | RailgunBatteryMk1 | 17.719 | 17.719 | +0.0% | 3 | 3 | 150 | 150 |
| Cruiser | RailgunBatteryMk2 | 30.492 | 30.492 | +0.0% | 3 | 3 | 135 | 135 |
| Cruiser | RailgunBatteryMk3 | 60.669 | 76.193 | +25.6% | 2 | 2 | 120 | 200 |
| Cruiser | CoilgunBatteryMk1 | 46.013 | 102.001 | +121.7% | 2 | 2 | 150 | 250 |
| Cruiser | CoilgunBatteryMk2 | 100.171 | 210.827 | +110.5% | 2 | 2 | 135 | 225 |
| Cruiser | CoilgunBatteryMk3 | 204.229 | 415.670 | +103.5% | 2 | 2 | 120 | 200 |
| Cruiser | Naval cannon | 26.133 | 15.680 | -40.0% | 3 | 1 | 75 | 50 |
| Battlecruiser | RailgunBatteryMk1 | 11.812 | 11.812 | +0.0% | 2 | 2 | 100 | 100 |
| Battlecruiser | RailgunBatteryMk2 | 20.328 | 20.328 | +0.0% | 2 | 2 | 90 | 90 |
| Battlecruiser | RailgunBatteryMk3 | 40.824 | 56.348 | +38.0% | 1 | 1 | 80 | 160 |
| Battlecruiser | CoilgunBatteryMk1 | 32.946 | 88.934 | +169.9% | 1 | 1 | 100 | 200 |
| Battlecruiser | CoilgunBatteryMk2 | 69.344 | 180.000 | +159.6% | 1 | 1 | 90 | 180 |
| Battlecruiser | CoilgunBatteryMk3 | 139.219 | 350.659 | +151.9% | 1 | 1 | 80 | 160 |
| Battlecruiser | Naval cannon | 17.422 | 15.680 | -10.0% | 2 | 1 | 50 | 50 |
| Battleship | RailgunBatteryMk1 | 35.438 | 35.438 | +0.0% | 6 | 6 | 300 | 300 |
| Battleship | RailgunBatteryMk2 | 60.984 | 60.984 | +0.0% | 6 | 6 | 270 | 270 |
| Battleship | RailgunBatteryMk3 | 121.338 | 152.386 | +25.6% | 4 | 4 | 240 | 400 |
| Battleship | CoilgunBatteryMk1 | 115.068 | 204.002 | +77.3% | 3 | 4 | 300 | 500 |
| Battleship | CoilgunBatteryMk2 | 241.653 | 421.653 | +74.5% | 3 | 4 | 270 | 450 |
| Battleship | CoilgunBatteryMk3 | 480.680 | 831.340 | +73.0% | 3 | 4 | 240 | 400 |
| Battleship | Naval cannon | 52.267 | 31.360 | -40.0% | 6 | 2 | 150 | 100 |
| Lancer | RailgunBatteryMk1 | 17.719 | 17.719 | +0.0% | 3 | 3 | 150 | 150 |
| Lancer | RailgunBatteryMk2 | 30.492 | 30.492 | +0.0% | 3 | 3 | 135 | 135 |
| Lancer | RailgunBatteryMk3 | 60.669 | 76.193 | +25.6% | 2 | 2 | 120 | 200 |
| Lancer | CoilgunBatteryMk1 | 46.013 | 102.001 | +121.7% | 2 | 2 | 150 | 250 |
| Lancer | CoilgunBatteryMk2 | 100.171 | 210.827 | +110.5% | 2 | 2 | 135 | 225 |
| Lancer | CoilgunBatteryMk3 | 204.229 | 415.670 | +103.5% | 2 | 2 | 120 | 200 |
| Lancer | Naval cannon | 26.133 | 15.680 | -40.0% | 3 | 1 | 75 | 50 |
| Dreadnought | RailgunBatteryMk1 | 47.250 | 47.250 | +0.0% | 8 | 8 | 400 | 400 |
| Dreadnought | RailgunBatteryMk2 | 81.312 | 81.312 | +0.0% | 8 | 8 | 360 | 360 |
| Dreadnought | RailgunBatteryMk3 | 163.296 | 225.392 | +38.0% | 4 | 4 | 320 | 640 |
| Dreadnought | CoilgunBatteryMk1 | 177.868 | 355.737 | +100.0% | 2 | 4 | 400 | 800 |
| Dreadnought | CoilgunBatteryMk2 | 360.000 | 720.000 | +100.0% | 2 | 4 | 360 | 720 |
| Dreadnought | CoilgunBatteryMk3 | 701.319 | 1402.637 | +100.0% | 2 | 4 | 320 | 640 |
| Dreadnought | Naval cannon | 69.689 | 62.720 | -10.0% | 8 | 4 | 200 | 200 |
| Titan | RailgunBatteryMk1 | 46.656 | 0.000 | -100.0% | 6 | 0 | 1200 | 0 |
| Titan | RailgunBatteryMk2 | 107.495 | 0.000 | -100.0% | 6 | 0 | 1080 | 0 |
| Titan | RailgunBatteryMk3 | 338.088 | 0.000 | -100.0% | 6 | 0 | 960 | 0 |
| Titan | CoilgunBatteryMk1 | 533.605 | 0.000 | -100.0% | 6 | 0 | 1200 | 0 |
| Titan | CoilgunBatteryMk2 | 1080.000 | 0.000 | -100.0% | 6 | 0 | 1080 | 0 |
| Titan | CoilgunBatteryMk3 | 2103.956 | 0.000 | -100.0% | 6 | 0 | 960 | 0 |
| Titan | Naval cannon | 0.000 | 0.000 | — | 0 | 0 | 0 | 0 |
| STOFighter | RailgunBatteryMk1 | 5.906 | 5.906 | +0.0% | 1 | 1 | 50 | 50 |
| STOFighter | RailgunBatteryMk2 | 10.164 | 10.164 | +0.0% | 1 | 1 | 45 | 45 |
| STOFighter | RailgunBatteryMk3 | 19.845 | 19.845 | +0.0% | 1 | 1 | 40 | 40 |
| STOFighter | CoilgunBatteryMk1 | 13.067 | 13.067 | +0.0% | 1 | 1 | 50 | 50 |
| STOFighter | CoilgunBatteryMk2 | 30.827 | 30.827 | +0.0% | 1 | 1 | 45 | 45 |
| STOFighter | CoilgunBatteryMk3 | 65.010 | 65.010 | +0.0% | 1 | 1 | 40 | 40 |
| STOFighter | Naval cannon | 8.711 | 0.000 | -100.0% | 1 | 0 | 25 | 0 |



## Capital policy blocker

Titan, AlienTitan and Mothership currently admit only `FourHull` weapons and treat each as one cell. Demoting a human heavy rail/coil family removes its only eligible capital entry; human Titan homogeneous family capacity becomes zero in the table. Alien capitals are unaffected by these human retiers. Keeping the old template as a capital-only legacy entry or introducing an explicit new heavy-role eligibility policy would require separate source, save-migration, UI and AI work. A mount enum edit alone is not safe.

## Other balance consequences

Size changes can affect native internal-size-derived HP and training behavior as well as packing. The inspected training initialization uses 270/internalSize, so 4 → 2 can double that rate unless explicitly decoupled. Existing designs, stock designs, upgrade choices, refits, ammo records and native controller merge mappings must be reviewed. Mass/crew/power are not automatically reduced with visual volume. No stat compensation is applied in these tables.
