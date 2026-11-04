# Fractional laser wavelength truncation: evidence and fix proposal

Update 2026-11-04: **fixed and deployed in 0.11.0** using the recommended
integer-angstrom adapter for both grasers. Native fields keep their units;
physical getter/spot conversion, decimal display and initialization cache reset
are covered by native-assembly regression checks. Live combat acceptance is
pending. See [implementation](asymmetric-tiers-implementation-2026-11-04.md).

Original analysis, 2026-11-03. Status: **confirmed data/native-type mismatch; fix proposed,
not implemented in this analysis**. Baseline: installed TI 1.0.53b and EEO
0.10.23. This fix is included in the
[tier-4–6 and size-5 implementation plan](tier-4-6-and-size-5-implementation-plan-2026-11-03.md).

## Confirmed scope

The installed laser template inventory uses wavelengths 0.1, 10, 270, 450, 540,
630, 810 and 1,080 nm. Exactly two templates have fractional values:

| Template | Native mount | Authored wavelength | Exact angstrom value | Native deserialization result |
| --- | --- | ---: | ---: | ---: |
| Alien768cmGraserCannon | ThreeNoseAngle | 0.1 nm | 1 Å | 0 nm |
| Alien1024cmGraserCannon | FourNose | 0.1 nm | 1 Å | 0 nm |

The current game assembly declares `public int wavelength_nm`. Its private
physical getter returns `wavelength_nm / 1e9` metres, and its localized wavelength
string formats the integer with `N0`. A standalone check using the installed
`StringSerializationAPI.Deserialize` reproduces zero for both graser records.
Controls retain 630, 450 and 10 nm for orange/violet/xaser templates. The
[analysis snapshot](hull-progression-audit-2026-11-03.json) includes the actual
field type and native-deserialization results. This is a typed native check,
not merely an assumption about JSON rounding. The live in-game template instance
was not inspected.

The value matters to physical beam-spot calculation, armor effectiveness and
range estimates. It also affects the wavelength displayed in tooltips. The
visible-light bombardment check compares the native nanometre field to 380–740;
both 0 and the intended 0.1 lie outside that interval, so this particular bug
does not accidentally make grasers atmospheric-bombardment weapons.

For these current grasers, nonzero pointing jitter already sets most of the
beam-spot floor. Correcting 0 -> 0.1 nm should therefore have a very small effect
on present beam area, rather than a large damage buff. It still fixes false
zero wavelength, preserves correct units and prevents later optics/jitter tuning
from operating on invalid physical data. It must not be bundled with an
unrelated damage increase and then credited for that increase.

## Recommended method: integer angstroms in a mod-owned precision layer

Use the user's preferred **integer ångström storage** for authoritative precision
records: 1 Å = 0.1 nm = 1e-10 m. This exactly covers every wavelength in the
current inventory. A native integer-nanometre value converts to Å by multiplying
by 10, while both fractional grasers receive an explicit value of 1.

Keep the game's existing native integer field and all ordinary template values
in **nanometres**. Introduce a versioned private precision catalog, initially
containing only the two affected IDs. That avoids duplicating the entire laser
inventory or changing the interpretation of other mods' existing native fields.
Its example record is logically:

```json
{"weapon":"Alien1024cmGraserCannon","wavelength_angstroms":1}
```

An authoritative accessor returns the precision override where present and
otherwise uses the existing native integer-nanometre value converted to Å.
Provide explicit conversion edges:

- Storage/editor: positive integer Å, with template-ID and range validation.
- Physics: Å * 1e-10, returned in metres to the existing beam formulas.
- Existing UI: Å / 10.0, formatted with enough fractional digits in nanometres;
  never integer division or `N0` on the fractional result. An optional tooltip
  may also show 1 Å, but it must label the units clearly.
- Compatibility checks: preserve the native nanometre field's meaning; use a
  physical accessor if a consumer must support a newly fractional threshold.

This is a formal storage-unit choice, not a claim that changing units removes
all floating-point representation error. Geometry calculations remain in Unity
floats. Integer Å preserves the authored value exactly until the conversion
edge; it has a 0.1 nm quantum. Future 0.01 nm or finer data would need a smaller
fixed unit (for example integer picometres) or decimal precision records.

## Code changes and fallout to address

1. **Physical wavelength getter.** Patch the installed
   `TILaserWeaponTemplate.get_wavelength_m` to return the precision accessor's
   metre value. Prefer the narrow getter boundary over duplicating the beam-spot
   equation or transpiling every caller. Guard the method/field types against
   the current game assembly. Unaffected templates retain their existing result.
   Install early enough to avoid an already-inlined native getter; if runtime
   binding cannot guarantee that, replace the validated getter call in the
   single spot-calculation method with the same accessor rather than copying
   its optical formula. A live physical-value readback must verify the hook.
2. **Tooltip/display.** Patch `GetLocalizedWaveLength` for precision records so
   the 0.1 nm value survives formatting. Preserve localization and units.
3. **Cache initialization/reload.** Audit and invalidate affected templates'
   `_SpotDiameterPreciseFactor_m`, inherited `_damageMinRange`, and cached role/
   research scores if computed before the precision layer is active. Apply this
   during initialization/catalog reload, not every frame. Merely fixing the
   getter can leave an already cached zero-wavelength spot calculation in use.
4. **All direct consumers.** The freshly decompiled laser class has direct
   integer-field reads for physical conversion, atmospheric eligibility and
   localized display. Verify the rest of the game and connected mod code for
   additional direct reads. Current repository C# has no wavelength reads to
   migrate. Validate aperture/armor/PD/AI/range callers through the common getter.
5. **Catalog and compatibility.** Reject duplicate/unknown IDs, nonpositive or
   overflowing Å values and changed expected source definitions. Do not silently
   overwrite a different wavelength supplied by another mod: establish explicit
   precedence and emit a bounded mismatch diagnostic. Ordinary native fields
   still use their current JSON schema and unit.
   A native zero alone cannot distinguish another mod's 0.1 nm from 0.2 nm;
   compare effective raw template data where available, or document that this
   precision catalog is authoritative for those IDs with an explicit override/
   disable mechanism. Do not claim that checking the rounded integer detects
   every competing fractional override.
6. **Saves and deployment.** Hull/design saves identify weapon templates by name;
   the proposed fix leaves those identities intact and does not require a hull
   migration. Confirm no serialized embedded template/cache needs invalidation
   in the actual save/import paths. Keep native assemblies untouched and deploy
   the mod through its normal guarded flow.

**Do not** multiply the existing `wavelength_nm` values by ten in place and let
the rest of the game treat them as Å. That would make an 810 nm laser appear
to be 8,100 nm to unconverted code, alter diffraction, print wrong tooltips and
break the visible-light bombardment window. It requires changing every consumer
and third-party expectation. Similarly, changing the field type in the game's
assembly from `int` to `float` is a much broader binary/compatibility change than
an external mod precision accessor.

## Alternatives and selection

| Method | Benefit | Cost / limitation |
| --- | --- | --- |
| Mod-owned integer Å + narrow conversion/accessor patches | Exact current authored values, clear units, small affected scope, preserves native compatibility | New small catalog, getter/display hooks and cache handling |
| Mod-owned float or decimal nanometres | Also supports 0.1 nm and potentially finer values; minimal unit change in authoring | Still needs the same native getter/display/cache patches; float is approximate, decimal must convert for Unity physics |
| Reinterpret all native integer values as Å | One integer unit if every consumer is ported | Broad semantic change; existing templates/mods and direct consumers expect nm |
| Rewrite the native field to floating point | Native field could represent 0.1 | Changes the game binary/type layout and compatibility assumptions; inappropriate for this scoped mod fix |

The first method best matches the user's storage preference and the small
confirmed defect. The second is also technically sound if future sub-Å values
become a requirement. The latter two add fallout without fixing more of the
current problem.

## Verification and delivery proposal

Add typed tests against the installed assembly for both affected records and
the 10/450/630/810/1,080 nm controls. Check 1 Å -> 0.1 nm -> 1e-10 m, correct
localized fractional display, invalid/unknown catalog entries, load/reload cache
invalidation and unchanged visible-light atmospheric checks. Compare cached and
fresh spot/armor/range calculations over multiple distances and all affected
apertures. Existing beam-optics and rendering checks must continue to pass.

Then run the normal `tools/deploy.ps1` flow with the game-closed assertions;
announce test readiness immediately after deployment. In game, inspect both
graser tooltips, obtain a bounded physical-wavelength/spot readback, and compare
against xaser controls. Test a new session, a loaded save, a reused/imported
design, and combat firing. Record that precision correction separately from
the proposed size-5 cadence buff. No implementation or deployment is claimed
by the present analysis.
