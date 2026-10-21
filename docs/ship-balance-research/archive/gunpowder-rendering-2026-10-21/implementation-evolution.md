# Canonical gunpowder art: 0.10.8 implementation

**Archived 2026-10-21.** Historical planning/investigation state; later outcomes supersede pending or provisional conclusions below. See the [current authority](../../gunpowder-canonical-art-implementation-2026-10-21.md). Original numerical evidence is retained.

2026-10-21. Authorized scope: existing gunpowder weapon art only. Documentation
checkpoint `d247f16`; previously deployed Mothership baseline `859a99b`.

**Runtime follow-up:** the user confirmed acceptable scales and successful
0.10.6 startup, but reported a flat grey 40 mm material from combat start.
The [material investigation and deployed 0.10.7 pass](material-inspection/README.md)
records source/UV checks, a native repair-material cache defect, and diagnostic
instrumentation. User testing confirmed the cutout remains. The
[0.10.8 comparison](material-inspection/background-cutout-0.10.8.md)
is now deployed, preserving the same dimensions with baked mesh copies and
adding a separate instancing test. Do not treat the cutout as resolved yet.

## Implementation plan

1. Add a versioned, per-template visual profile catalog for all eight gunpowder
   templates. Compute scale from canonical caliber / measured inscribed bore.
   Guard the source game/bundle hashes and source mesh/transform identities.
2. Apply instance-local base scale after native SetWeapon, preserving source
   child scale and offsets. Scaling the parent scales mesh, pivot and fire-point
   once. Never modify shared meshes, physics templates, hull nodes or mount tiers.
3. Normalize these eight weapons to resource scale in designer, combat and
   simple-hull paths. Restore prior native scales before reusing a controller,
   including switching to other weapons or disabling the feature, so scaling
   does not accumulate or leak into coils, rails, lasers or missiles.
4. Validate schema, all targets, repeated apply/restore transitions, source
   geometry, Harmony binding and preservation of all gameplay data. Run normal
   deployment immediately after implementation is ready; report manual-test
   readiness, then complete documentation/matrix updates.

## Canonical targets

| Weapon | Caliber mm | Approximate full-art factor | Mount |
|---|---:|---:|---|
| 30mmAutocannon | 30 | 0.199859 | OneHull |
| 35mmAutocannon | 35 | 0.233169 | HalfNose |
| 40mmAutocannon | 40 | 0.266478 | OneHull |
| 40mmNoseAutocannon | 40 | 0.266478 | HalfNose |
| 6-inchCannon | 152.4 | 0.981646 | OneHull |
| 8-inchCannon | 203.2 | 1.060939 | TwoHullHoriz |
| 10-inchCannon | 254.0 | 1.326173 | OneNose |
| 12-inchCannon | 304.8 | 1.181638 | TwoNoseVert |

The factor applies to the whole weapon about its original mount origin. It does
not change projectile mass/velocity, collider diameter, DPS, cadence, crew,
power, magazine, HP, tracking rate or footprint. No 25 mm weapon is introduced.
The fixed canonical preview policy differs from the planning tables that kept
native placeholder scaling; normal combat resource dimensions follow the plan.

## Implemented behavior and schema

**Current package: 0.10.8; loader correction introduced in 0.10.6.** The initial 0.10.5 deployment passed offline checks
but failed the user's game startup: native ModManager recursively treats any
path containing `.json` as template data, including private Config files.
The private profile now has extension **`.eeo`**, retaining JSON content for
our reader while avoiding native discovery. A `.json.txt` suffix would not work.
The corrected validator executes the actual native predicate, reproduces the
old `.json`/`.json.backup` selection, verifies `.eeo` exclusion, and checks all
25 selected template files have array roots. Deployment removes the obsolete
private `.json` from the installed package. See the [startup failure evidence](startup-and-deployment/startup-json-failure.png).

The [schema-1 profile catalog](../../../../TIEconomyMod/ModFiles/Config/gunpowder-art-profiles.eeo)
is separate from game templates. [GunpowderArtCatalog](../../../../TIEconomyMod/Core/GunpowderArtCatalog.cs)
requires exactly the eight approved weapon IDs, finite positive calibration and
scale values, complete mesh/transform fingerprints, and matching installed game
assembly/weapon-bundle SHA-256 hashes. Unknown fields, unknown/future schema,
duplicates and malformed profiles reject. A load rejection disables this art
feature with a warning while other mod features continue using native art.

[GunpowderArtSetWeaponPatch](../../../../TIEconomyMod/Patches/GunpowderArtPatches.cs)
restores previously captured native scales before native controller reuse.
After native SetWeapon copies the new meshes and fire point, it checks the
template/resource, scenario-aware projectile caliber, base/child/fire-point
hierarchy, mesh names/counts/bounds and local offsets. It then assigns the
absolute root resource scale multiplied by the profile factor and the source
child scale. All offsets inherit the parent factor exactly once. No shared
mesh, source prefab or game template is mutated; there is no saved-game schema
or migration attached to this visual profile schema.

Rebuilding a gun view repeats the same absolute target, rather than multiplying
an already resized object. Switching to another weapon restores native scales
before its initialization. The global mod toggle and saving settings refresh
tracked visible controllers; `shipBalance.enabled` gates the pass. A scale
changed by another system after application is not blindly restored. Changing
the profile file requires reloading the mod/game; it is not a live editor.

Designer/simple-hull normalization is deliberate. The whole ship's parent
transform still applies, but gun profiles no longer inherit art-variant-specific
placeholder base/child scales. This makes their caliber consistent with normal
combat resource dimensions. Enlarged naval guns and newly small autocannons can
therefore look different from a mere scalar multiplication of the old preview.
Muzzle launch origins follow the resized visible gun; projectile statistics,
tracking calculations and collider calibration are unchanged.

## Build, deployment and validation

Deployed through normal `tools/deploy.ps1`, without SkipVerification, to the
enabled Economic Equalization Overhaul package. Both required game-closed checks
ran. The final pass validated **40 jobs**, **1,204 existing formula assertions**,
**300 new managed lifecycle assertions**, twelve invalid-schema cases, typed
JSON round-trip, source hashes and all eight target calibrations. All **41**
pre-existing root gameplay/localization/settings files retain their original
SHA-256 hashes. The new profile is packaged separately under Config.

The corrected 0.10.6 deployment hash-verified **48 files**. DLL SHA-256:
`11C643A84B1D0C73F21ED1D49A0E44EA6946E9785F939815B573D180911A65D9`.
The [current deployment log](startup-and-deployment/deployment-0.10.6.txt),
[initial 0.10.5 deployment log](startup-and-deployment/deployment-0.10.5.txt) and
[gameplay-data baseline](../../gunpowder-art/gameplay-data-baseline.json) retain evidence.
The first verification attempt stopped before deployment on a PowerShell
reflection argument and the missing matrix row; both were corrected before the
successful 0.10.5 deployment. Its subsequent startup failure exposed the missing
native file-discovery check described above; 0.10.6 adds that check and passed
the full build/validation/deployment flow. No checks were skipped.

Harmony resolves the SetWeapon patch target; applying that Unity-internal target
and constructing actual scene transforms requires the game host. Offline tests
exercise the managed snapshot/restore logic, not a live Unity scene. Mesh guards,
aiming, muzzle VFX and appearance quality remain part of manual acceptance.

## Manual acceptance, build ready

- Compare 30/35/40 mm and 6/8/10/12-inch hull/nose fittings in the designer.
- Repeatedly swap shared-resource variants (30 ↔ 40 mm; 8 ↔ 10-inch where legal),
  then switch to a laser/magnetic weapon. Check scale resets and no accumulation.
- Inspect small guns on their existing mounts, all four human appearances,
  forward/elevated aiming, muzzle flash placement, and normal combat rendering.
- Check a simple-hull exofighter with 35/40 mm nose guns and an existing save.
- Confirm weapon size tiers and displayed statistics remain unchanged.

Runtime Unity hierarchy/aiming and appearance quality require in-game acceptance;
offline checks above are separate from that manual test. On first use, the log
prints `Gunpowder art schema 1` with weapon, preview/ship context and factor.
`Gunpowder art guard retained native art` identifies any profile that was skipped.
