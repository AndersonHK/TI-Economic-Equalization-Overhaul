# Earth Orbits, Launch Costs, and Lunar Resource Semantics

Status: approved and implemented design authority for Parts 1-5 of the
Earth-orbit and Luna-site change. The approved thirty-five-site roster, numeric
mining bands, vanilla comparisons, and mass rationale are recorded in
[resource-yield-comparison.md](resource-yield-comparison.md).

The implementation follows this authority in the mod templates, localization,
launch-cost patches, formula tests, and automatic validators. These orbit and
Luna map changes take effect when starting a new campaign.

## 1. Earth launch-cost correction

### Current defects

Vanilla `TISpaceObjectState.GenericTransferBoostFromEarthSurface` treats every
Earth LEO destination as exactly one Boost per ten tonnes, regardless of orbit
altitude or inclination. For destinations beyond LEO it begins the generic
transfer at `GameStateManager.LEOStates()[0]`, making cost depend on template
iteration order and effectively hard-coding Low Earth Orbit 1.

Two crew-movement paths in `TIHabModuleState` and `TISpaceShipState` contain
their own flat LEO cost and must be brought under the same corrected model.

### Approved launch-site scope

Boost remains a fungible global launch-capacity resource. For each cost
calculation, evaluate all operational Earth launch facilities with positive
Boost output and use the facility that produces the lowest launch delta-v. Do
not restrict candidates to facilities controlled by the launching faction.

This is intentionally separate from Boost production. The existing
latitude-based production bonus remains unchanged: it determines how
efficiently a nation creates Boost, not the cost of a particular destination.

If no operational launch facility exists, use a deterministic fallback based
on Earth map-region `boostLatitude` values so cost preview remains available.

### Approved ascent model

For every candidate launch facility:

1. Read its signed `boostLatitude`, the target-orbit altitude, and the absolute
   target inclination. Positive and negative inclinations of equal magnitude
   have equal launch costs.
2. Model an idealized two-impulse ascent from the rotating Earth:
   - inject from Earth-radius perigee into an ellipse whose apogee is the
     target circular-orbit radius;
   - treat the launch site's rotational velocity as a vector;
   - circularize at apogee;
   - when the launch-site latitude magnitude exceeds the target inclination,
     combine circularization with the unavoidable dogleg or plane change.
3. Select the facility with the lowest total ascent delta-v.

For launch-site latitude `phi`, target inclination `i`, and target orbital
radius `r`, use Earth gravitational and rotation values from the runtime body
state rather than duplicated real-world constants. The direct-ascent orbital
plane has inclination `j = max(abs(phi), abs(i))`. The target-plane difference
at circularization is `max(0, abs(phi) - abs(i))`.

The model is an abstraction of launch-vehicle performance, atmospheric loss,
and infrastructure. It is meant to get the relative altitude and inclination
costs right while retaining Terra Invicta's existing Boost scale.

### Calibration and Boost conversion

An equatorial launch to a circular 500 km, 0-degree orbit remains the reference
case and costs exactly one Boost per ten tonnes. For any Earth-interface orbit:

`Boost = mass_tons * spaceResourceToTons * exp((launchDV - referenceDV) / modifiedGenericEV)`

Use the faction's existing modified generic exhaust-velocity value, whose
unmodified value is 2.11 km/s. Do not clamp away a legitimate saving for a
lower orbit.

Illustrative costs with an ideally located launch site are:

| Destination | Boost per 10 tonnes |
|---|---:|
| 500 km, 0 degrees | 1.000 |
| 500 km, +/-20 degrees | 1.013 |
| 500 km, +/-40 degrees | 1.053 |
| 1,000 km, 0 degrees | 1.132 |
| 500 km, polar | 1.247 |

Actual costs may be slightly higher when no operational facility is at the
ideal latitude. A site equatorward of the requested inclination can launch
directly by changing azimuth. A site poleward of the requested inclination
cannot launch directly into that lower-inclination plane and incurs a large
dogleg penalty. The implementation must evaluate delta-v rather than merely
choose the numerically nearest latitude.

NASA references for the geometry are the
[Launch Site Processing and Scheduling System report](https://ntrs.nasa.gov/api/citations/20020000775/downloads/20020000775.pdf)
and the
[GDC Orbit Primer](https://science.nasa.gov/wp-content/uploads/2023/05/GDC_OrbitPrimer.pdf).

### Destinations beyond LEO

For a non-LEO destination, evaluate every instantiated Earth interface orbit
as a candidate parking orbit. For each parking-orbit and launch-site pair,
calculate:

`normalized launch surcharge + generic transfer delta-v + landing delta-v`

Use the minimum result before applying the existing Boost rocket equation.
This removes the `LEOStates()[0]` dependency and permits the cheapest parking
inclination to vary with the destination.

All construction previews, actual construction charges, founding costs,
resupply costs, and crew-movement costs must call the same authority. No
flat-LEO shortcut may remain.

## 2. Earth orbit changes

Add four orbit templates that otherwise copy Low Earth Orbit 1:

| Data name | English display name | Nominal altitude | Base inclination |
|---|---|---:|---:|
| `LowEarthOrbitPlus20` | Low Earth Orbit +20 degrees | 500 km | +20 degrees |
| `LowEarthOrbitPlus40` | Low Earth Orbit +40 degrees | 500 km | +40 degrees |
| `LowEarthOrbitMinus20` | Low Earth Orbit -20 degrees | 500 km | -20 degrees |
| `LowEarthOrbitMinus40` | Low Earth Orbit -40 degrees | 500 km | -40 degrees |

They retain Low Earth Orbit 1's altitude range, inclination range, station
capacity, Earth-LEO flag, interface-orbit flag, and other template properties.

Change these starting habitats to `LowEarthOrbitPlus40`:

- `InternationalSpaceStation`
- `InternationalSpaceStationSkirmish`
- `Tiangong`

Replace Earth's instantiated orbit list so it includes the four new templates
and omits `LowEarthOrbit3` and `LowEarthOrbit4`. The vanilla bespoke templates
may remain loaded, but Earth must not instantiate orbit states from them.

This is a new-campaign feature. Migration of existing saves that already
contain the bespoke station orbit states is outside the approved scope.

## 3. Space-resource semantics

The English localization defines the resource categories broadly:

- Water supports life support, hydrogen propellant, and deuterium.
- Volatiles currently lists carbon, nitrogen, oxygen, sulfur, chlorine,
  phosphorus, and hydrogen-bearing compounds.
- Base Metals includes iron, nickel, lead, zinc, copper, aluminum, tin,
  lithium, silicon, and boron.
- Noble Metals includes silver, gold, platinum, titanium, and tungsten.
- Fissiles includes uranium, thorium, and processed plutonium.

The implementation must correct the English explanatory text:

- Water supplies both hydrogen and oxygen through electrolysis.
- Volatiles covers carbon, nitrogen, sulfur, chlorine, phosphorus, and
  hydrogen-bearing compounds other than water.
- Oxygen bound into silicates and oxides is not a volatile resource. Lunar
  mineral oxygen belongs to regolith-processing mechanics, while water-ice
  oxygen belongs to Water.

The geology rules used for Luna are:

- Generic lunar regolith does not receive economically mineable Water.
- Generic lunar regolith does not receive a Volatiles output merely because it
  contains trace solar-wind carbon, nitrogen, or hydrogen.
- Evidence-supported polar cold traps may produce Water and non-water
  Volatiles.
- Evidence-supported pyroclastic deposits may receive a small Volatiles
  output for sulfur, halogens, and indigenous hydrogen, but not for bulk
  oxygen and not on the assumption of abundant carbon.
- Base Metals are widespread because the category includes abundant iron,
  aluminum, and silicon.
- Lunar Noble Metals output is principally a titanium proxy because true
  precious metals are not known to occur at comparable bulk abundance.
- Fissiles represent uranium and thorium and must remain mass-consistent with
  their ppm-scale occurrence even at KREEP and thorium anomalies.

NASA's representative Apollo-soil composition is approximately 42% oxygen,
21% silicon, 13% iron, 7% aluminum, 2% titanium, 100 ppm carbon, 80 ppm
nitrogen, and 1 ppm thorium. See
[NASA/CP-2007-214995](https://ntrs.nasa.gov/api/citations/20080003835/downloads/20080003835.pdf).
High-titanium mare basalt can contain 10-14 wt% TiO2, while highland material
is dominated by aluminum- and calcium-rich anorthosite. See
[NASA's lunar mare classification](https://ntrs.nasa.gov/citations/19940019897)
and
[NASA/TM-2010-216219](https://ntrs.nasa.gov/api/citations/20100017257/downloads/20100017257.pdf).

Ordinary lunar carbon is not an industrially rich resource. Returned rocks
typically contain about 10-50 ppm carbon and fines may reach roughly 200 ppm,
much of it associated with solar-wind exposure. See
[Carbon Chemistry of Apollo 14 Size-fractionated Fines](https://www.nature.com/articles/physci235106a0).

## 5. Implementation and verification sequence

Follow the repository's `Plan -> Document -> Implement -> Build -> Deploy ->
Test -> Document` workflow.

1. Complete and approve the thirty-five-site Luna roster, coordinates, profile
   bands, and mass rationale in the companion resource document and CSV.
2. Add a pure `EarthLaunchCostMath` authority and formula tests.
3. Add Harmony patches for generic Earth launch cost and every remaining
   flat-LEO crew or resupply path.
4. Add the four orbit templates and their localization.
5. Update the three starting-habitat orbit references.
6. Add site-specific lunar mining profiles rather than modifying broadly
   reused vanilla profiles.
7. Add or override the thirty-five lunar site templates and their localization.
8. Replace Earth's orbit array and Luna's habitat-site array. Add
   `TISpaceBodyTemplate.json` to `TemplatesToReplaceArrays` so these arrays do
   not merge by index.
9. Extend automatic validation to require:
   - exactly thirty-five unique Luna sites;
   - valid and unique coordinates and grid positions;
   - at least 200 km great-circle separation between every pair of Luna sites;
   - resolvable mining-profile and localization references;
   - all-zero fields for every resource declared absent;
   - exact +/- inclination cost symmetry;
   - exactly one Boost per ten tonnes for the reference orbit;
   - monotonic altitude cost at fixed inclination;
   - a large penalty for a poleward site launching to a lower inclination;
   - selection of the minimum-cost facility and parking orbit independent of
     collection order;
   - no remaining flat-LEO cost branch;
   - ISS, skirmish ISS, and Tiangong in the +40-degree orbit;
   - no instantiated Tiangong or ISS bespoke orbit;
   - comparison snapshots for vanilla Luna, vanilla Mars, and the approved
     thirty-five-site proposal.
10. Run `tools/deploy.ps1` without `-SkipVerification`. The script must perform
    the build, automated verification, game-process checks, and package copy.
11. Immediately report that the deployed build is ready for manual testing.
12. Test in a new campaign:
    - inspect all Earth interface orbits and station placement;
    - compare displayed and charged Boost costs at 0, +/-20, and +/-40 degrees;
    - test a destination beyond LEO;
    - test construction and crew resupply cost consistency;
    - inspect all thirty-five lunar map markers;
    - prospect the Moon across repeated seeds and confirm every output remains
      inside its approved band;
    - check that absent Water and Volatiles never appear;
    - check AI valuation and mine selection with very small fissile outputs.
13. Record build identifiers, automatic-test results, manual observations, and
    any subsequently approved balance adjustment in the documentation.

## Implementation record

The approved design was implemented and deployed on 2026-08-17 against Terra
Invicta 1.0.51.

- `tools/deploy.ps1` completed release verification without
  `-SkipVerification` and deployed 44 hash-verified files.
- The release DLL SHA-256 is
  `92DB6F63EB90BF86661E8C6A405D2BEB04D86C0CCA05A92D8B8ED94AE9123CCD`.
- Formula tests passed 1,070 assertions.
- Runtime validation applied all 142 maintained Harmony patches and specifically
  confirmed the three new Earth launch-cost patches against the installed game.
- Data validation confirmed the four inclination templates, retired bespoke
  station orbits, three migrated starting habitats, thirty-five unique lunar
  sites, thirty-five site-specific profiles, 595 compliant pairwise distances,
  exact approved bands, and corrected English resource semantics.
- The enabled package is under
  `Mods/Enabled/Economic Equalization Overhaul` in the detected game install.

Manual in-game testing remains pending. It must use a new campaign and follow
steps 12-13 above; any observed launch-cost, map-placement, resource-display,
or AI-valuation issue should be appended here with its campaign conditions.

### Manual-test observations

- 2026-08-17: the Earth overview's Moon list displayed the unresolved key
  `TIMiningProfileTemplate.displayName.EEOLunarSite01Mine` in Luna's Type
  column. The site-profile generator supplied descriptions but omitted the
  display-name localization required by that UI path. The deployed correction
  localizes every site-specific profile as the vanilla-compatible type
  `Lunar`, while retaining its geological role in the profile description, and
  to make this display-name key an automatic validation requirement.
- 2026-08-17: in the new-campaign Luna map, Shackleton, Cabeus, Haworth,
  Shoemaker, and Faustini overlapped in a tight south-polar cluster. Visual
  inspection showed the approximately 202 km pair readable while the 46-156 km
  pairs overlapped. The corrective roster retains Shackleton and replaces the
  other four with Plato, Humboldt, Clavius, and Gagarin. Automatic validation
  now requires at least 200 km great-circle separation; the revised roster's
  actual minimum is 324 km.
- 2026-08-17: the roster was expanded from thirty to thirty-five sites to
  represent additional plausible polar ice deposits without relaxing the
  200 km readability floor. Cabeus B, Malapert C, Wiechert U, Lovelace E, and
  Nansen A use mapped PSR positions and modest high-variance Water/Volatiles
  bands. The final minimum separation is 207 km. This roster and the preceding
  Type/display-name correction were deployed after full verification on the
  same date.
- 2026-08-18: the Intel screen's Mining Profile column showed the lunar
  profiles as wrapped geological explanations with repeated resource-range
  boilerplate, unlike vanilla's compact labels such as `Polar Regolith` and
  `Wet Martian Regolith`. Lunar profile descriptions will instead be concise
  geological-class labels no longer than 40 characters. The detailed geology
  and resource rationale remain in the approved yield CSV and design notes.
  All thirty-five replacements passed targeted data validation and full release
  verification (1,078 formula assertions, 93 implementation-matrix rows, and
  143 Harmony patches). Deployment remains pending because the process guard
  found Terra Invicta running and made no changes to the installed package.
