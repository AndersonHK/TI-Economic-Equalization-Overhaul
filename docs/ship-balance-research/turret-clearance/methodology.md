# Measurement method and reproduction

## Sources and scope

Installed base and Dark Skies ship bundles, `shipweapons`, installed templates, repository partial overrides, and native integer controller mappings from the installed `Assembly-CSharp.dll`. Source hashes are below. The exporter invokes only `SlotToWeaponMountIndex` on uninitialized native controller instances; unavailable invalid-slot paths are recorded as null. Every used ordinary slot must map to a non-null, unique, in-range index. This avoids guessing from GameObject names or assuming serialized slots are sequential hull mounts. Native faction weapons are classified using the native `TIShipPartTemplate.isAlien` project rule (`Project_AlienMasterProject` / `Project_AlienAdvancedMasterProject`), including alien missiles with non-Alien names.

## Geometry and scale

Only the root base mesh and first-child weapon mesh are measured because those are the meshes copied by `ShipModelController.SetWeapon`. MeshHandler vertices retain Unity coordinates and are checked against the stored mesh bounds. Decorative VFX, fire-point transforms and arbitrary prefab scene placement are excluded. All hierarchy transforms are composed for hull nodes; scene-root translation is removed. Units follow the game art-space metre convention used by the existing hull survey, not each hull template’s separately declared dimensions.

Combat on non-simple hulls replaces base scale with the weapon resource root scale and weapon scale with the resource child scale. Designer preview retains each hull node’s authored placeholder scales. Simple hulls retain placeholder scales in both paths. Child position always comes from the resource. The renderer initializes a base-orientation parent while preserving world placement; the extracted authored parent scale is retained. The installed nodes have unit parent scale. Exact placeholder scales and resource scales are retained in JSON.

## Conservative swept envelope

For root scale matrix S, weapon-child offset p, and child-scaled vertex v, arbitrary rotations obey `||S(p + Rv)|| ≤ ||Sp|| + σmax(S) ||v||`. The bound radius is the maximum of that expression and the farthest scaled base vertex, multiplied by any inherited parent maximum scale. The sphere includes all yaw/elevation orientations, not just the forward pose. This is deliberately conservative, especially for nonuniform alien scales and offset pivots. The inventory also gives exact authored-pose dimensions and an authored-pose yaw-only radius; these are not interchangeable with a full-elevation clearance guarantee.

For each pair of different logical indices, including cross-face pairs, `gap = nodeDistance − radiusA − radiusB`. The proportional margin is `gap / (radiusA + radiusB)`; for equal turrets this is gap divided by the swept diameter. Absolute and proportional requirements combine with AND. We test 0, 2, 5 and 10 m; 25% and 50%; and 5 m plus 25%. Opposite-face copies of the same logical index are excluded from the neighbor test. No pair exists for single-node hulls, which are marked explicitly. A class passes only if every appearance and relevant node pair passes. The maximum size is searched independently: the biggest size-1 art can be larger than size-2 art.

## Extra nodes and fitting

Ordinary indices come from `OneHull` mapping of every declared HullHardPoint. All other controller indices are extra candidates, irrespective of their names. Legal multi-cell placements are derived from the native grid offsets: horizontal `(0,0),(1,0)`, vertical `(0,0),(0,2)`, four-cell rectangle. All-controller results require every candidate at once. The extra-subset search is exact: retain all ordinary mounts, then maximize compatible extras. An infeasible ordinary layout is reported as unavailable rather than silently deleting slots. New node use would require slot arrays, controller mapping, UI fitting, AI, save/refit migration, ammo/damage index handling and manual combat tests; extra graphics alone do not implement those behaviors.

## What the geometry does not prove

No occupied hull-mesh intersection test, radiator deployment sweep, nose-weapon clearance, fixed-launcher specialized envelope, occlusion/fire-arc validation, animation capture or in-game visual test is claimed. Sphere overlap only means this sufficient clearance test cannot certify the arrangement. It is not a mesh-intersection result. Zero-distance distinct mount origins are retained as a direct asset anomaly. A global inventory test additionally covers cross-faction art, but does not grant cross-faction weapon access.

## Reproduce

Use Python 3.12 with NumPy, UnityPy and Pillow (the shared hull catalog module imports Pillow). The existing `.tmp/unitypy` dependency directory is recognized. No network, game asset write or gameplay configuration write is performed by either research script.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/ship-balance/export_turret_mount_maps.ps1
python scripts/ship-balance/measure_turret_clearance.py
# Regenerate Markdown from saved evidence using standard-library Python:
python scripts/ship-balance/report_turret_clearance.py
python scripts/ship-balance/test_turret_clearance.py
```

Both extraction scripts accept an installation path and output destination (`--game-install-dir` / `-GameInstallDir`; `--output-root` / `-OutputPath`). All retained artifacts belong under `docs/`. A stale native assembly hash is rejected before measurement. To inspect a given variant, locate its `hull` and `appearance` record in `measurements.json`; it contains all coordinates, native mapping, padding decisions and limiting pairs. Markdown nodes and variant tables expose the same inventory. For a single saved variant use `python scripts/ship-balance/report_turret_clearance.py --hull Titan --appearance 1`. Add `--geometry` to the test command for randomized envelope containment and exhaustive extra-subset checks; those checks require the extraction dependencies.

## Source SHA-256 inventory

| Source | SHA-256 |
| --- | --- |
| D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\Managed\Assembly-CSharp.dll | 4A4B9AAE4154E444E9727204205D2D42AE8ED9E1C5F92CDC1280074A259D8350 |
| C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\docs\ship-balance-research\turret-clearance\mount-maps.json | B033C346D5EA3FCC4B522491875E371AA1F8CEE693DE9C277F03A778805522D3 |
| C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\scripts\ship-balance\measure_turret_clearance.py | 0FAFD1407A78E270027884919D07ABBA0430AA8608E286624A647FBC5AD6EA9D |
| C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\scripts\ship-balance\export_turret_mount_maps.ps1 | 4A98B5E5C29DAE9793C9FD699BBD66472F0ABF337BD161A6F55E76A2FFFFB71D |
| C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\scripts\ship-balance\report_turret_clearance.py | FEA169E71949788BB0CCCBDA44672CC807CF64A1D7AF86CE25A651C2E2D24930 |
| C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\Core\CapitalMountPolicy.cs | 7CC9B4A6E36EEBE557AAFE6DB38D97C5DD4085FD0F9A052A3D145D7CED792DD0 |
| C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\Patches\CapitalMountPatches.cs | BE3AA5F8FE6EB15AAB3570FC4BC49FBB9D36C60E2C481BF3A3EA44AD9C2E9A9A |
| D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\Templates\TIShipHullTemplate.json | 36952BDDFFCBEBE1C3AB2C2141B7D7DF53F985C0EED326C5A43A9B52BD86826B |
| C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\ModFiles\TIShipHullTemplate.json | ED0A6EE01A2F5931324B15897A923AB9EB6F5A822A25FE7BF02E9D1F7E6D4318 |
| D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\AssetBundles\ships | F1804254B5C6F2C2FFBF78C333738F462BAA18023AC5AACF9A8EACAA4D9F09A4 |
| D:\Games\SteamLibrary\steamapps\common\Terra Invicta\DLC_Content\DarkSkies\AssetBundles\ships_prm | 58F8B233CD7814F3CD536B46725369F69D6EC58AA260E580DE9D1E1268CF5D63 |
| D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\AssetBundles\shipweapons | 9DFCF8E93536DC400A00428AF2D6C32226BAC44F1ECD88B874D8F946E9EA912E |
| D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\Templates\TIGunTemplate.json | 1D7093358EC282391003CB11E6566CC09258881F4EA2C10179FDE9AFD6723662 |
| C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\ModFiles\TIGunTemplate.json | 62E0B9BDB6EEE4257DE0EF69335CC153104335789978DAF01F9CAF8FC1DCDF73 |
| D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\Templates\TIMagneticGunTemplate.json | DB4FCE6D3B9AE8AB31B10D2D0CDEFF1A18079C964FA545E7D3C0F127167BA5E3 |
| C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\ModFiles\TIMagneticGunTemplate.json | 09870E5530E7B51FD8D9D08DA985B17C7E20B98E547651DE7B4269A2C0C49176 |
| D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\Templates\TILaserWeaponTemplate.json | EB2953A9B5C747E9392C891A8DDFF837BA65A129F138C334843D2EA295A76C32 |
| C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\ModFiles\TILaserWeaponTemplate.json | FFDD44E9B41A65608A0B60E5F60070B11AAE81A4EF3C8CDB7472F6CABCFC7ED6 |
| D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\Templates\TIParticleWeaponTemplate.json | 8E65189EE90D07F628D8CB5393374AD4DC40E348E67E4A1E7B30B32FEC926F16 |
| D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\Templates\TIPlasmaWeaponTemplate.json | 53D756601396A4F282940B5632CBA7D24C1ACEE4BCE10F94F93AA343417159B8 |
| D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\Templates\TIMissileTemplate.json | 823ADB7550F07D3E504889626459F3C68E87D45191AA67419D365C16C455351F |
| C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\ModFiles\TIMissileTemplate.json | 2F05AB66BE79BE16904158361FB5BEB689D234216D8FFCD1CBFCE8BA8FA04F01 |
