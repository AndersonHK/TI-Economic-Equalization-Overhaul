# Station Slot Cardinal Map

Terra Invicta's `HabitatsScreenController.PreviewStationModule` selects each
station cell by the key `S{sector}_M{slot}`. The corresponding
`StationGridCell` RectTransforms are serialized in
`TerraInvicta_Data/StreamingAssets/AssetBundles/ui`.

The relevant grid uses 75-unit cells. Its serialized coordinates establish the
following screen positions without relying on module artwork or screenshots:

| Internal sector | Slot | UI coordinate | Screen position |
|---:|---:|---:|---|
| 0 | 0 | `(262.5, -262.5)` | core |
| 0 | 1 | `(262.5, -187.5)` | north |
| 0 | 2 | `(337.5, -262.5)` | east |
| 0 | 3 | `(262.5, -337.5)` | south |
| 0 | 4 | `(187.5, -262.5)` | west |
| 2 | 0 | `(487.5, -262.5)` | outer/east junction |
| 2 | 1 | `(487.5, -337.5)` | south |
| 2 | 2 | `(412.5, -262.5)` | inward/west |
| 2 | 3 | `(487.5, -187.5)` | north |
| 4 | 0 | `(37.5, -262.5)` | outer/west junction |
| 4 | 1 | `(37.5, -187.5)` | north |
| 4 | 2 | `(112.5, -262.5)` | inward/east |
| 4 | 3 | `(37.5, -337.5)` | south |

## ISS layout

The intended starting ISS therefore uses:

| Visible sector | Internal sector | Slot assignment |
|---:|---:|---|
| 1 | 0 | north Quarters; east/west Solar Collectors; south Space Science Lab |
| 3 | 2 | outer/east Life Science Lab; inward/west Quarters |
| 2 | 4 | outer/west Quarters; inward/east Materials Lab |

This mapping is enforced by `tools/validate-hab-rebalance.ps1`.
