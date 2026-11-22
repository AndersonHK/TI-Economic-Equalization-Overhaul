"""Validate orbital mounts, release budgets and affected predefined habs."""
import json
import math
from pathlib import Path
import sys


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def merge(base, overrides):
    result = {record["dataName"]: record for record in read(base)}
    for record in read(overrides):
        key = record["dataName"]
        result[key] = result.get(key, {}) | record
    return result


repo = Path(__file__).resolve().parent.parent
stock = Path(sys.argv[1]).resolve().parent / "StreamingAssets/Templates"
package = repo / "TIEconomyMod/ModFiles"
modules = merge(stock / "TIHabModuleTemplate.json", package / "TIHabModuleTemplate.json")
presets = merge(stock / "TIHabTemplate.json", package / "TIHabTemplate.json")
expected = {
    "PointDefenseArray": (5, 6, 10, 150),
    "LayeredDefenseArray": (25, 12, 40, 750),
    "Battlestations": (46, 20, 240, 6000),
    "AlienPointDefenseArray": (2, 3, 25, 100),
    "AlienLayeredDefenseArray": (8, 6, 75, 500),
    "AlienBattlestations": (8, 6, 150, 2500),
}
catalog = read(package / "Config/orbital-defense-layouts.eeo")
assert catalog["schema"] == 1
assert {layout["module"] for layout in catalog["layouts"]} == set(expected)
total = 0
for layout in catalog["layouts"]:
    name = layout["module"]
    batteries, point_defense, power, mass = expected[name]
    assert (modules[name]["power"], modules[name]["baseMass_tons"]) == (-power, mass), name
    assert layout["tier"] == modules[name]["tier"], name
    mounts = layout["mounts"]
    assert len({mount["id"] for mount in mounts}) == len(mounts) == batteries + point_defense, name
    assert sum(mount["role"] == "PointDefense" for mount in mounts) == point_defense, name
    for face, sign in (("top", 1), ("bottom", -1)):
        assert len({tuple(mount[face]["fittedOrigin"]) for mount in mounts}) == len(mounts), name
        for mount in mounts:
            pose = mount[face]
            normal = pose["normal"]
            assert sign * normal[1] > 0 and math.isclose(math.hypot(*normal), 1, abs_tol=.001), name
            if name == "PointDefenseArray":
                assert mount["top"]["seat"][::2] == mount["bottom"]["seat"][::2], name
    total += 2 * len(mounts)

assert total == 294
for name, sunlight in (
    ("GatewayStation", 0), ("GatewayStationSkirmish", .6), ("ArchonStationSkirmish", .7),
    ("ValkyrieStation", 4), ("ValkyrieStationSkirmish", 4),
    ("KalininBase", 0), ("MaddenBase", 0), ("TerraSirenumBase", 0),
):
    net = 0
    for sector in presets[name]["sectors"]:
        for module in sector["habModuleNames"]:
            if module:
                template = modules[module]
                power = template["power"]
                net += round(power * sunlight) if "Solar_Power_Variable_Output" in template.get("specialRules", []) else power
    assert net >= 0, (name, net)

print(f"PASS: {total} measured orbital mount poses, six layouts, approved budgets, and eight preset power checks.")
