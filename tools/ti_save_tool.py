#!/usr/bin/env python3
"""Inspect and safely transform Terra Invicta FullSerializer saves.

The tool deliberately separates semantic parsing from output validation.  Any
whole-document serialization must first pass the ``roundtrip-check`` command,
which requires the decompressed payload to be byte-identical.
"""

from __future__ import annotations

import argparse
import copy
import gzip
import hashlib
import io
import json
import math
import os
import struct
import sys
from pathlib import Path
from typing import Any, Dict, Iterable, Iterator, List, Optional, Sequence, Tuple


METADATA_TYPE = "PavonisInteractive.TerraInvicta.TIMetadataState"
FACTION_TYPE = "PavonisInteractive.TerraInvicta.TIFactionState"
FLEET_TYPE = "PavonisInteractive.TerraInvicta.TISpaceFleetState"
SHIP_TYPE = "PavonisInteractive.TerraInvicta.TISpaceShipState"
ORBIT_TYPE = "PavonisInteractive.TerraInvicta.TIOrbitState"
NOTIFICATION_TYPE = "PavonisInteractive.TerraInvicta.TINotificationQueueState"
OFFICER_TYPE = "PavonisInteractive.TerraInvicta.TIOfficerState"
HAB_TYPE = "PavonisInteractive.TerraInvicta.TIHabState"
NATION_TYPE = "PavonisInteractive.TerraInvicta.TINationState"
REGION_TYPE = "PavonisInteractive.TerraInvicta.TIRegionState"
TIME_EVENT_TYPE = "PavonisInteractive.TerraInvicta.TITimeEvent"
ATTACK_FLEET_GOAL_TYPE = (
    "PavonisInteractive.TerraInvicta.FactionGoal_AttackWithFleet"
)
JOIN_FLEET_GOAL_TYPE = "PavonisInteractive.TerraInvicta.FactionGoal_JoinFleet"


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest().upper()


def read_payload(path: Path) -> bytes:
    raw = path.read_bytes()
    return gzip.decompress(raw) if path.suffix.lower() == ".gz" else raw


def decode_payload(payload: bytes) -> str:
    text = payload.decode("utf-8-sig")
    # FullSerializer supports two C#-style escapes that RFC JSON parsers do
    # not. Translate them only while lexically inside strings; the native
    # printer below restores the original spelling.
    output: List[str] = []
    in_string = False
    index = 0
    while index < len(text):
        character = text[index]
        if not in_string:
            output.append(character)
            if character == '"':
                in_string = True
            index += 1
            continue
        if character == '"':
            output.append(character)
            in_string = False
            index += 1
            continue
        if character == "\\" and index + 1 < len(text):
            escaped = text[index + 1]
            if escaped in {"a", "0"}:
                output.append("\\u0007" if escaped == "a" else "\\u0000")
            else:
                output.append(character + escaped)
            index += 2
            continue
        output.append(character)
        index += 1
    return "".join(output)


class RawInt(int):
    """Integer retaining the source token used by FullSerializer."""

    def __new__(cls, token: str) -> "RawInt":
        value = int.__new__(cls, token)
        value.token = token
        return value


class RawFloat(float):
    """Float retaining precision, exponent spelling, and non-finite tokens."""

    def __new__(cls, token: str) -> "RawFloat":
        value = float.__new__(cls, token)
        value.token = token
        return value


def load_document(payload: bytes) -> Dict[str, Any]:
    document = json.loads(
        decode_payload(payload),
        parse_int=RawInt,
        parse_float=RawFloat,
        parse_constant=RawFloat,
    )
    if not isinstance(document, dict) or "gamestates" not in document:
        raise ValueError("Input is not a Terra Invicta SaveStructure document")
    return document


def escape_fullserializer_string(value: str) -> str:
    """Reproduce fsJsonPrinter.EscapeString, including UTF-16 code units."""

    escaped: List[str] = []
    substitutions = {
        '"': '\\"',
        "\\": "\\\\",
        "\a": "\\a",
        "\b": "\\b",
        "\f": "\\f",
        "\n": "\\n",
        "\r": "\\r",
        "\t": "\\t",
        "\0": "\\0",
    }
    for character in value:
        codepoint = ord(character)
        if codepoint > 127:
            encoded = character.encode("utf-16-be", errors="surrogatepass")
            escaped.extend(
                f"\\u{int.from_bytes(encoded[index:index + 2], 'big'):04x}"
                for index in range(0, len(encoded), 2)
            )
        else:
            escaped.append(substitutions.get(character, character))
    return "".join(escaped)


def _write_pretty(node: Any, stream: io.StringIO, depth: int) -> None:
    if node is None:
        stream.write("null")
    elif node is True:
        stream.write("true")
    elif node is False:
        stream.write("false")
    elif isinstance(node, RawInt):
        stream.write(node.token)
    elif isinstance(node, RawFloat):
        stream.write(node.token)
    elif isinstance(node, int):
        stream.write(str(node))
    elif isinstance(node, float):
        if math.isnan(node):
            stream.write("NaN")
        elif math.isinf(node):
            stream.write("Infinity" if node > 0 else "-Infinity")
        else:
            token = repr(node)
            stream.write(token if any(marker in token for marker in ".eE") else token + ".0")
    elif isinstance(node, str):
        stream.write('"')
        stream.write(escape_fullserializer_string(node))
        stream.write('"')
    elif isinstance(node, dict):
        stream.write("{\r\n")
        for index, (key, value) in enumerate(node.items()):
            if index:
                stream.write(",\r\n")
            stream.write("    " * (depth + 1))
            # FullSerializer does not call EscapeString for dictionary keys.
            stream.write(f'"{key}": ')
            _write_pretty(value, stream, depth + 1)
        stream.write("\r\n")
        stream.write("    " * depth)
        stream.write("}")
    elif isinstance(node, list):
        if not node:
            stream.write("[]")
            return
        stream.write("[\r\n")
        for index, value in enumerate(node):
            if index:
                stream.write(",\r\n")
            stream.write("    " * (depth + 1))
            _write_pretty(value, stream, depth + 1)
        stream.write("\r\n")
        stream.write("    " * depth)
        stream.write("]")
    else:
        raise TypeError(f"Unsupported serialized value: {type(node).__name__}")


def encode_document(document: Dict[str, Any]) -> bytes:
    # FullSerializer PrettyJson uses four spaces, CRLF, a UTF-8 BOM, unquoted
    # non-finite floats, expanded empty objects, and no final newline.
    stream = io.StringIO()
    _write_pretty(document, stream, 0)
    return b"\xef\xbb\xbf" + stream.getvalue().encode("utf-8")


def state_entries(document: Dict[str, Any], type_name: str) -> List[Dict[str, Any]]:
    entries = document["gamestates"].get(type_name, [])
    if not isinstance(entries, list):
        raise ValueError(f"State table is not an array: {type_name}")
    return entries


def state_id(value: Any) -> Optional[int]:
    if isinstance(value, dict):
        identifier = value.get("ID")
        if isinstance(identifier, dict) and isinstance(identifier.get("value"), int):
            return identifier["value"]
    return None


def ref_id(value: Any) -> Optional[int]:
    if isinstance(value, dict) and isinstance(value.get("value"), int):
        return value["value"]
    return None


def entry_value(entry: Dict[str, Any]) -> Dict[str, Any]:
    value = entry.get("Value")
    if not isinstance(value, dict):
        raise ValueError("Malformed state entry without an object Value")
    return value


def index_states(document: Dict[str, Any], type_name: str) -> Dict[int, Dict[str, Any]]:
    indexed: Dict[int, Dict[str, Any]] = {}
    for entry in state_entries(document, type_name):
        value = entry_value(entry)
        identifier = state_id(value)
        if identifier is None:
            raise ValueError(f"State in {type_name} has no numeric ID")
        if identifier in indexed:
            raise ValueError(f"Duplicate ID {identifier} in {type_name}")
        indexed[identifier] = value
    return indexed


def nested_ref(value: Dict[str, Any], *path: str) -> Optional[int]:
    node: Any = value
    for key in path:
        if not isinstance(node, dict):
            return None
        node = node.get(key)
    return ref_id(node)


def fleet_ship_ids(fleet: Dict[str, Any]) -> List[int]:
    ships = fleet.get("ships", [])
    if not isinstance(ships, list):
        return []
    return [identifier for identifier in (ref_id(item) for item in ships) if identifier is not None]


def fleet_display_name(fleet: Dict[str, Any], faction_id: Optional[int]) -> str:
    display_name = fleet.get("displayName")
    if isinstance(display_name, str) and display_name:
        return display_name
    names = fleet.get("displayNameByFaction", [])
    if isinstance(names, list):
        for item in names:
            if (
                isinstance(item, dict)
                and ref_id(item.get("Key")) == faction_id
                and isinstance(item.get("Value"), str)
            ):
                return item["Value"]
    return ""


def find_paths_to_ids(node: Any, identifiers: Iterable[int]) -> Iterator[Tuple[str, int]]:
    wanted = set(identifiers)
    stack: List[Tuple[str, Any]] = [("$", node)]
    while stack:
        path, current = stack.pop()
        if isinstance(current, dict):
            if set(current).issubset({"value", "$type"}):
                identifier = ref_id(current)
                if identifier in wanted:
                    yield path, identifier
            for key, child in reversed(list(current.items())):
                stack.append((f"{path}.{key}", child))
        elif isinstance(current, list):
            for index in range(len(current) - 1, -1, -1):
                stack.append((f"{path}[{index}]", current[index]))


def all_state_entries(document: Dict[str, Any]) -> Iterator[Tuple[str, int, Dict[str, Any]]]:
    for type_name, entries in document["gamestates"].items():
        if entries == {}:
            # FullSerializer emits an empty Dictionary<GameStateID, T> as an
            # empty object, while populated dictionaries use Key/Value arrays.
            continue
        if not isinstance(entries, list):
            raise ValueError(f"State table is not an array: {type_name}")
        for entry in entries:
            value = entry_value(entry)
            identifier = state_id(value)
            if identifier is None:
                raise ValueError(f"State in {type_name} has no numeric ID")
            key_identifier = ref_id(entry.get("Key"))
            if key_identifier != identifier:
                raise ValueError(
                    f"State key/value ID mismatch in {type_name}: {key_identifier} != {identifier}"
                )
            yield type_name, identifier, entry


def serialized_node(node: Any) -> bytes:
    stream = io.StringIO()
    _write_pretty(node, stream, 0)
    return stream.getvalue().encode("utf-8")


def state_byte_map(document: Dict[str, Any]) -> Dict[Tuple[str, int], bytes]:
    return {
        (type_name, identifier): serialized_node(entry)
        for type_name, identifier, entry in all_state_entries(document)
    }


def metadata_fast_scan_succeeds(payload: bytes) -> bool:
    return any(
        b"TIMetadataState" in line and b"[" in line
        for line in payload.splitlines()[:200]
    )


def reference_integrity(document: Dict[str, Any]) -> Tuple[int, List[str]]:
    definitions: set = set()
    references: List[Tuple[str, str]] = []
    stack: List[Tuple[str, Any]] = [("$", document)]
    while stack:
        path, node = stack.pop()
        if isinstance(node, dict):
            definition = node.get("$id")
            if isinstance(definition, str):
                definitions.add(definition)
            reference = node.get("$ref")
            if isinstance(reference, str):
                references.append((path, reference))
            for key, child in node.items():
                stack.append((f"{path}.{key}", child))
        elif isinstance(node, list):
            for index, child in enumerate(node):
                stack.append((f"{path}[{index}]", child))
    unresolved = [f"{path} -> {reference}" for path, reference in references if reference not in definitions]
    return len(references), unresolved


def fullserializer_definition_map(node: Any) -> Dict[str, Dict[str, Any]]:
    definitions: Dict[str, Dict[str, Any]] = {}
    stack = [node]
    while stack:
        current = stack.pop()
        if isinstance(current, dict):
            identifier = current.get("$id")
            if isinstance(identifier, str):
                if identifier in definitions:
                    raise ValueError(f"Duplicate FullSerializer $id: {identifier}")
                definitions[identifier] = copy.deepcopy(current)
            stack.extend(current.values())
        elif isinstance(current, list):
            stack.extend(current)
    return definitions


def promote_unresolved_fullserializer_definitions(
    node: Any, source_definitions: Dict[str, Dict[str, Any]]
) -> List[Dict[str, str]]:
    promoted: List[Dict[str, str]] = []
    while True:
        current_definitions = set(fullserializer_definition_map(node))
        stack: List[Tuple[Any, Any, str, Any]] = [(None, None, "$", node)]
        unresolved_location: Optional[Tuple[Any, Any, str, str]] = None
        while stack:
            parent, key, path, current = stack.pop()
            if isinstance(current, dict):
                reference = current.get("$ref")
                if isinstance(reference, str) and reference not in current_definitions:
                    unresolved_location = (parent, key, path, reference)
                    break
                for child_key, child in reversed(list(current.items())):
                    stack.append((current, child_key, f"{path}.{child_key}", child))
            elif isinstance(current, list):
                for index in range(len(current) - 1, -1, -1):
                    stack.append((current, index, f"{path}[{index}]", current[index]))
        if unresolved_location is None:
            return promoted
        parent, key, path, reference = unresolved_location
        if parent is None:
            raise ValueError("Root object cannot be a FullSerializer $ref")
        definition = source_definitions.get(reference)
        if definition is None:
            raise ValueError(
                f"No source definition is available for unresolved $ref {reference} at {path}"
            )
        parent[key] = copy.deepcopy(definition)
        promoted.append({"id": reference, "path": path})


def nonfinite_counts(node: Any) -> Dict[str, int]:
    counts = {"Infinity": 0, "-Infinity": 0, "NaN": 0}
    stack = [node]
    while stack:
        current = stack.pop()
        if isinstance(current, RawFloat) and current.token in counts:
            counts[current.token] += 1
        elif isinstance(current, dict):
            stack.extend(current.values())
        elif isinstance(current, list):
            stack.extend(current)
    return counts


def remove_state_entries(document: Dict[str, Any], type_name: str, identifiers: set) -> int:
    entries = state_entries(document, type_name)
    retained = [entry for entry in entries if state_id(entry_value(entry)) not in identifiers]
    removed = len(entries) - len(retained)
    document["gamestates"][type_name] = retained
    return removed


def remove_reference_items(items: Any, identifiers: set) -> int:
    if not isinstance(items, list):
        return 0
    retained = [item for item in items if ref_id(item) not in identifiers]
    removed = len(items) - len(retained)
    items[:] = retained
    return removed


def remove_keyed_reference_items(items: Any, identifiers: set) -> int:
    if not isinstance(items, list):
        return 0
    retained = [
        item
        for item in items
        if not isinstance(item, dict) or ref_id(item.get("Key")) not in identifiers
    ]
    removed = len(items) - len(retained)
    items[:] = retained
    return removed


def cleanup_remaining_references(
    node: Any, fleet_ids: set, removed_ids: set, removed_ship_ids: set
) -> Dict[str, int]:
    changes = {
        "null_goto_targets": 0,
        "null_assigned_fleets": 0,
        "null_parent_fleets": 0,
        "null_attack_targets": 0,
        "null_join_goal_targets": 0,
        "null_log_goal_targets": 0,
        "removed_collection_references": 0,
        "nulled_layout_positions": 0,
        "removed_refit_queue_items": 0,
        "removed_preserved_fleet_ship_references": 0,
        "removed_alarms": 0,
    }
    removable_lists = {
        "pendingFleets",
        "assetsInOrbit",
        "landedFleets",
        "dockedFleets",
    }

    stack = [node]
    while stack:
        current = stack.pop()
        if isinstance(current, dict):
            for key in list(current):
                child = current[key]
                identifier = ref_id(child)
                if key == "gotoGameState" and identifier in removed_ids:
                    current[key] = None
                    changes["null_goto_targets"] += 1
                    continue
                if key == "assignedFleet" and identifier in fleet_ids:
                    current[key] = None
                    changes["null_assigned_fleets"] += 1
                    continue
                if key == "parentFleet" and identifier in fleet_ids:
                    current[key] = None
                    changes["null_parent_fleets"] += 1
                    continue
                if key == "attackTarget" and identifier in fleet_ids:
                    current[key] = None
                    if "importance" in current:
                        current["importance"] = RawInt("0")
                    changes["null_attack_targets"] += 1
                    continue
                if key == "targetFleet" and identifier in fleet_ids:
                    current[key] = None
                    changes["null_join_goal_targets"] += 1
                    continue
                if key == "GoalTarget" and identifier in fleet_ids:
                    current[key] = None
                    changes["null_log_goal_targets"] += 1
                    continue
                if key == "_itemList" and isinstance(child, list):
                    for index, item in enumerate(child):
                        if ref_id(item) in fleet_ids:
                            child[index] = None
                            changes["nulled_layout_positions"] += 1
                if key in removable_lists and isinstance(child, list):
                    changes["removed_collection_references"] += remove_reference_items(
                        child, fleet_ids
                    )
                if key == "alarms" and isinstance(child, list):
                    retained = [
                        alarm
                        for alarm in child
                        if not isinstance(alarm, dict)
                        or ref_id(alarm.get("associatedGameState")) not in fleet_ids
                    ]
                    changes["removed_alarms"] += len(child) - len(retained)
                    child[:] = retained
                if key == "nShipyardQueues" and isinstance(child, list):
                    for queue_entry in child:
                        if not isinstance(queue_entry, dict):
                            continue
                        queue = queue_entry.get("Value")
                        if not isinstance(queue, list):
                            continue
                        retained_queue = [
                            item
                            for item in queue
                            if not isinstance(item, dict)
                            or ref_id(item.get("originalSpaceShipState"))
                            not in removed_ship_ids
                        ]
                        changes["removed_refit_queue_items"] += len(queue) - len(
                            retained_queue
                        )
                        queue[:] = retained_queue
                if key == "preservedFleetCompositions" and isinstance(child, list):
                    for faction_entry in child:
                        if not isinstance(faction_entry, dict):
                            continue
                        records = faction_entry.get("Value")
                        if not isinstance(records, list):
                            continue
                        for record in records:
                            if not isinstance(record, dict):
                                continue
                            changes[
                                "removed_preserved_fleet_ship_references"
                            ] += remove_reference_items(
                                record.get("ships"), removed_ship_ids
                            )
                stack.append(child)
        elif isinstance(current, list):
            stack.extend(current)
    return changes


def _float32(value: float) -> float:
    """Round like the intermediate Unity Vector3/Quaternion float operations."""

    return struct.unpack("<f", struct.pack("<f", float(value)))[0]


def _vector_cross(left: Sequence[float], right: Sequence[float]) -> Tuple[float, float, float]:
    return (
        _float32(left[1] * right[2] - left[2] * right[1]),
        _float32(left[2] * right[0] - left[0] * right[2]),
        _float32(left[0] * right[1] - left[1] * right[0]),
    )


def _quaternion_rotate(
    quaternion: Sequence[float], vector: Sequence[float]
) -> Tuple[float, float, float]:
    xyz = quaternion[:3]
    cross = _vector_cross(xyz, vector)
    twice_cross = tuple(_float32(2.0 * value) for value in cross)
    second_cross = _vector_cross(xyz, twice_cross)
    return tuple(
        _float32(vector[index] + quaternion[3] * twice_cross[index] + second_cross[index])
        for index in range(3)
    )


def _angle_axis_rotate(
    angle_degrees: float, axis: Sequence[float], vector: Sequence[float]
) -> Tuple[float, float, float]:
    magnitude = math.sqrt(sum(float(value) * float(value) for value in axis))
    if magnitude == 0.0:
        return tuple(float(value) for value in vector)
    half_angle = math.radians(angle_degrees) / 2.0
    sine = math.sin(half_angle)
    quaternion = tuple(_float32(value / magnitude * sine) for value in axis) + (
        _float32(math.cos(half_angle)),
    )
    return _quaternion_rotate(quaternion, vector)


def cone_layout_position(layout: Dict[str, Any], index: int) -> Dict[str, float]:
    """Reproduce TIConeLayoutState.GetPositionForIndex for a stored layout."""

    width = int(layout["_width"])
    height = int(layout["_height"])
    if width <= 0 or height <= 0:
        raise ValueError("Docking layout has invalid dimensions")
    rotation_value = layout["_worldRotation"]
    position_value = layout["_worldPosition"]
    rotation = tuple(float(rotation_value[key]) for key in ("x", "y", "z", "w"))
    world_position = tuple(float(position_value[key]) for key in ("x", "y", "z"))

    per_layer = width * height
    column = index % per_layer % width
    row = index % per_layer // width
    layer = index // per_layer + 1
    column_fraction = _float32(column / width)
    row_fraction = _float32(row / height)

    vector = _quaternion_rotate(rotation, (0.0, 0.0, 1.0))
    up = _quaternion_rotate(rotation, (0.0, 1.0, 0.0))
    right = _quaternion_rotate(rotation, (1.0, 0.0, 0.0))
    vector = _angle_axis_rotate(-45.0, up, vector)
    vector = _angle_axis_rotate(_float32(90.0 * column_fraction), up, vector)
    vector = _angle_axis_rotate(-45.0, right, vector)
    vector = _angle_axis_rotate(_float32(90.0 * row_fraction), right, vector)
    distance = _float32(_float32(900.0 * layer) + 1500.0)
    return {
        key: float(_float32(_float32(vector[offset] * distance) + world_position[offset]))
        for offset, key in enumerate(("x", "y", "z"))
    }


def add_unique_state_reference(
    items: Any, identifier: int, type_name: Optional[str] = None
) -> bool:
    if not isinstance(items, list):
        raise ValueError("Expected a serialized state-reference list")
    if any(ref_id(item) == identifier for item in items):
        return False
    reference: Dict[str, Any] = {"value": RawInt(str(identifier))}
    if type_name:
        reference["$type"] = type_name
    items.append(reference)
    return True


def remove_goal_references(node: Any, goal_ids: set) -> int:
    """Remove goal IDs from faction goal collections and goal links."""

    removed = 0
    stack = [node]
    while stack:
        current = stack.pop()
        if isinstance(current, dict):
            for key, child in list(current.items()):
                if key == "targetFleetGoal" and ref_id(child) in goal_ids:
                    current[key] = None
                    removed += 1
                elif isinstance(child, list):
                    removed += remove_reference_items(child, goal_ids)
                    stack.extend(child)
                else:
                    stack.append(child)
        elif isinstance(current, list):
            stack.extend(current)
    return removed


def calculate_reference_region_gdp(
    reference_document: Dict[str, Any],
    region_id: int,
    expected_nation_id: int,
    current_population_millions: float,
) -> Dict[str, float]:
    """Project a region's pre-transfer GDP per capita onto current population."""

    nations = index_states(reference_document, NATION_TYPE)
    regions = index_states(reference_document, REGION_TYPE)
    nation = nations.get(expected_nation_id)
    region = regions.get(region_id)
    if nation is None or region is None:
        raise ValueError("Reference save lacks the expected nation or region")
    if nested_ref(region, "nation") != expected_nation_id:
        raise ValueError("Reference region is not owned by the expected destination nation")
    nation_region_ids = [ref_id(item) for item in nation.get("regions", [])]
    if region_id not in nation_region_ids:
        raise ValueError("Reference nation does not contain the reference region")

    population_by_region: Dict[int, float] = {}
    weight_by_region: Dict[int, float] = {}
    for identifier in nation_region_ids:
        reference_region = regions.get(identifier)
        if reference_region is None:
            raise ValueError(f"Reference nation points to missing region {identifier}")
        population = reference_region.get("populationInMillions")
        if not isinstance(population, (int, float)) or float(population) <= 0.0:
            raise ValueError(f"Reference region {identifier} has invalid population")
        population_by_region[identifier] = float(population)
        weight = float(population)
        if bool(reference_region.get("coreEconomicRegion")):
            weight *= 1.25
        if bool(reference_region.get("resourceRegion")) or bool(
            reference_region.get("oilRegion")
        ):
            weight *= 1.25
        if bool(reference_region.get("colonyRegion")):
            weight *= 0.5
        weight_by_region[identifier] = weight

    reference_gdp = nation.get("GDP")
    if not isinstance(reference_gdp, (int, float)) or float(reference_gdp) <= 0.0:
        raise ValueError("Reference nation has invalid GDP")
    total_population = sum(population_by_region.values())
    total_gdp_weight = sum(weight_by_region.values())
    reference_region_population = population_by_region[region_id]
    reference_region_gdp = (
        float(reference_gdp) * weight_by_region[region_id] / total_gdp_weight
    )
    reference_region_gdp_per_capita = reference_region_gdp / (
        reference_region_population * 1_000_000.0
    )
    current_region_gdp = (
        reference_region_gdp_per_capita * current_population_millions * 1_000_000.0
    )
    return {
        "reference_nation_gdp": float(reference_gdp),
        "reference_nation_population_millions": total_population,
        "reference_nation_gdp_weight": total_gdp_weight,
        "reference_region_population_millions": reference_region_population,
        "reference_region_gdp_weight": weight_by_region[region_id],
        "reference_region_gdp": reference_region_gdp,
        "reference_region_gdp_per_capita": reference_region_gdp_per_capita,
        "current_region_population_millions": current_population_millions,
        "current_region_gdp_transfer": current_region_gdp,
    }


def mutate_relocate_fleets_and_transfer_region(
    document: Dict[str, Any],
    fleet_ids: Sequence[int],
    station_id: int,
    expected_faction_id: int,
    expected_destination_orbit_id: int,
    region_id: int,
    from_nation_id: int,
    to_nation_id: int,
    gdp_reference: Optional[Dict[str, float]] = None,
) -> Dict[str, Any]:
    """Dock selected fleets and perform a structural region-ownership repair."""

    selected_fleet_ids = list(dict.fromkeys(fleet_ids))
    if not selected_fleet_ids:
        raise ValueError("At least one fleet ID is required")
    if len(selected_fleet_ids) != len(fleet_ids):
        raise ValueError("Fleet IDs must be unique")

    fleets = index_states(document, FLEET_TYPE)
    habs = index_states(document, HAB_TYPE)
    orbits = index_states(document, ORBIT_TYPE)
    nations = index_states(document, NATION_TYPE)
    regions = index_states(document, REGION_TYPE)
    station = habs.get(station_id)
    if station is None:
        raise ValueError(f"Station {station_id} does not exist")
    station_faction_id = nested_ref(station, "faction")
    if station_faction_id != expected_faction_id:
        raise ValueError(
            f"Station {station_id} faction {station_faction_id} does not match expected {expected_faction_id}"
        )
    target_orbit_id = nested_ref(station, "orbitState")
    if target_orbit_id is None or target_orbit_id not in orbits:
        raise ValueError(f"Station {station_id} has no valid orbit")
    target_orbit = orbits[target_orbit_id]
    target_body_id = nested_ref(station, "barycenter")
    if target_body_id is None:
        raise ValueError(f"Station {station_id} has no barycenter")

    docked_fleets = station.get("dockedFleets")
    layout = station.get("_dockedShipAbovePositions")
    if not isinstance(docked_fleets, list) or not isinstance(layout, dict):
        raise ValueError(f"Station {station_id} has no serialized docking collections")
    layout_items = layout.get("_itemList")
    if not isinstance(layout_items, list):
        raise ValueError(f"Station {station_id} has no docking layout item list")
    orbit_assets = target_orbit.get("assetsInOrbit")
    if not isinstance(orbit_assets, list):
        raise ValueError(f"Orbit {target_orbit_id} has no asset collection")

    orbital_assets: Dict[int, Dict[str, Any]] = {}
    orbital_assets.update(fleets)
    orbital_assets.update(habs)
    used_anomalies = {
        float(asset["_meanAnomalyAtEpoch_Rad"])
        for identifier in (ref_id(item) for item in orbit_assets)
        if identifier in orbital_assets
        for asset in [orbital_assets[identifier]]
        if isinstance(asset.get("_meanAnomalyAtEpoch_Rad"), (int, float))
    }
    station_anomaly = station.get("_meanAnomalyAtEpoch_Rad")
    if not isinstance(station_anomaly, (int, float)):
        raise ValueError("Target station lacks valid orbital anomaly data")
    # TIOrbitState serializes its template name, not the template's semimajor
    # axis. Infer the native docking step from an existing docked asset. A
    # station with no such sample must be handled by an exact-version template
    # loader rather than guessing an orbit radius.
    anomaly_differences = [
        abs(float(fleets[identifier]["_meanAnomalyAtEpoch_Rad"]) - float(station_anomaly))
        for identifier in (ref_id(item) for item in docked_fleets)
        if identifier in fleets
        and isinstance(fleets[identifier].get("_meanAnomalyAtEpoch_Rad"), (int, float))
        and float(fleets[identifier]["_meanAnomalyAtEpoch_Rad"])
        != float(station_anomaly)
    ]
    if not anomaly_differences:
        raise ValueError(
            "Cannot infer docking anomaly spacing: target station has no offset orbital asset"
        )
    anomaly_step = min(anomaly_differences)

    fleet_details = []
    for fleet_id in selected_fleet_ids:
        fleet = fleets.get(fleet_id)
        if fleet is None:
            raise ValueError(f"Fleet {fleet_id} does not exist")
        faction_id = nested_ref(fleet, "faction")
        destination_id = nested_ref(fleet, "trajectory", "destinationOrbit")
        if faction_id != expected_faction_id:
            raise ValueError(
                f"Fleet {fleet_id} faction {faction_id} does not match expected {expected_faction_id}"
            )
        if destination_id != expected_destination_orbit_id:
            raise ValueError(
                f"Fleet {fleet_id} destination {destination_id} does not match expected {expected_destination_orbit_id}"
            )
        if nested_ref(fleet, "orbitState") is not None or fleet.get("dockedLocation") is not None:
            raise ValueError(f"Fleet {fleet_id} is not exclusively in transit")
        if fleet.get("currentOperations") != []:
            raise ValueError(f"Fleet {fleet_id} has unsupported active operations")

        fleet["trajectory"] = None
        fleet["orbitState"] = copy.deepcopy(station["orbitState"])
        fleet["dockedLocation"] = {
            "value": RawInt(str(station_id)),
            "$type": HAB_TYPE,
        }
        fleet["barycenter"] = copy.deepcopy(station["barycenter"])
        fleet["_epoch_JYears"] = copy.deepcopy(station["_epoch_JYears"])
        fleet["epoch_DateTime"] = copy.deepcopy(station["epoch_DateTime"])
        proposed_anomaly = float(station_anomaly)
        while proposed_anomaly in used_anomalies:
            proposed_anomaly += anomaly_step
        fleet["_meanAnomalyAtEpoch_Rad"] = proposed_anomaly
        used_anomalies.add(proposed_anomaly)
        if "globalPosition" in station:
            fleet["globalPosition"] = copy.deepcopy(station["globalPosition"])
        fleet["inAccelerationPhase"] = False
        fleet["inDecelerationPhase"] = False
        fleet["unavailableForOperations"] = False
        fleet["returnToOperationsTime"] = None

        add_unique_state_reference(orbit_assets, fleet_id, FLEET_TYPE)
        add_unique_state_reference(docked_fleets, fleet_id)
        try:
            layout_index = next(
                index for index, item in enumerate(layout_items) if item is None
            )
            layout_items[layout_index] = {
                "value": RawInt(str(fleet_id)),
                "$type": FLEET_TYPE,
            }
        except StopIteration:
            layout_index = len(layout_items)
            layout_items.append(
                {"value": RawInt(str(fleet_id)), "$type": FLEET_TYPE}
            )
        fleet["dockOffset"] = cone_layout_position(layout, layout_index)
        fleet_details.append(
            {
                "fleet_id": fleet_id,
                "fleet_name": fleet_display_name(fleet, faction_id),
                "ship_ids": fleet_ship_ids(fleet),
                "old_destination_orbit_id": destination_id,
                "new_orbit_id": target_orbit_id,
                "station_id": station_id,
                "layout_index": layout_index,
                "mean_anomaly_rad": proposed_anomaly,
                "dock_offset": copy.deepcopy(fleet["dockOffset"]),
            }
        )

    goal_ids: set = set()
    goal_type_by_id: Dict[int, str] = {}
    goal_count_by_fleet = {identifier: 0 for identifier in selected_fleet_ids}
    for goal_type in (ATTACK_FLEET_GOAL_TYPE, JOIN_FLEET_GOAL_TYPE):
        for entry in state_entries(document, goal_type):
            goal = entry_value(entry)
            assigned_fleet_id = nested_ref(goal, "assignedFleet")
            if assigned_fleet_id not in goal_count_by_fleet:
                continue
            if nested_ref(goal, "faction") != expected_faction_id:
                raise ValueError(
                    f"Goal {state_id(goal)} assigns fleet {assigned_fleet_id} from another faction"
                )
            identifier = state_id(goal)
            if identifier is None:
                raise ValueError("Assigned fleet goal has no ID")
            goal_ids.add(identifier)
            goal_type_by_id[identifier] = goal_type
            goal_count_by_fleet[assigned_fleet_id] += 1
    missing_goals = [identifier for identifier, count in goal_count_by_fleet.items() if count != 1]
    if missing_goals:
        raise ValueError(
            "Expected exactly one active assigned goal for each fleet; mismatch for "
            + ", ".join(str(identifier) for identifier in missing_goals)
        )
    for goal_type in (ATTACK_FLEET_GOAL_TYPE, JOIN_FLEET_GOAL_TYPE):
        remove_state_entries(
            document,
            goal_type,
            {identifier for identifier, kind in goal_type_by_id.items() if kind == goal_type},
        )
    removed_goal_references = remove_goal_references(document, goal_ids)

    time_event_ids: set = set()
    for entry in state_entries(document, TIME_EVENT_TYPE):
        event = entry_value(entry)
        if event.get("eventName") == "PlayerAlarm" and nested_ref(
            event, "eventObject2ID"
        ) in goal_count_by_fleet:
            identifier = state_id(event)
            if identifier is not None:
                time_event_ids.add(identifier)
    remove_state_entries(document, TIME_EVENT_TYPE, time_event_ids)
    removed_alarm_records = 0
    for faction in index_states(document, FACTION_TYPE).values():
        alarms = faction.get("alarms")
        if not isinstance(alarms, list):
            continue
        retained = [
            alarm
            for alarm in alarms
            if not isinstance(alarm, dict)
            or nested_ref(alarm, "alarmEvent") not in time_event_ids
        ]
        removed_alarm_records += len(alarms) - len(retained)
        alarms[:] = retained

    region = regions.get(region_id)
    old_nation = nations.get(from_nation_id)
    new_nation = nations.get(to_nation_id)
    if region is None or old_nation is None or new_nation is None:
        raise ValueError("Region or nation precondition state does not exist")
    if nested_ref(region, "nation") != from_nation_id:
        raise ValueError(
            f"Region {region_id} is not owned by source nation {from_nation_id}"
        )
    old_regions = old_nation.get("regions")
    new_regions = new_nation.get("regions")
    if not isinstance(old_regions, list) or not isinstance(new_regions, list):
        raise ValueError("Source or destination nation has no region collection")
    if sum(ref_id(item) == region_id for item in old_regions) != 1:
        raise ValueError("Source nation does not contain the region exactly once")
    if any(ref_id(item) == region_id for item in new_regions):
        raise ValueError("Destination nation already contains the region")
    remove_reference_items(old_regions, {region_id})
    add_unique_state_reference(new_regions, region_id)
    region["nation"] = {"value": RawInt(str(to_nation_id))}

    gdp_transfer = None
    if gdp_reference is not None:
        gdp_transfer = float(gdp_reference["current_region_gdp_transfer"])
        old_gdp = old_nation.get("GDP")
        new_gdp = new_nation.get("GDP")
        if not isinstance(old_gdp, (int, float)) or not isinstance(new_gdp, (int, float)):
            raise ValueError("Source or destination nation has invalid GDP")
        if gdp_transfer <= 0.0 or gdp_transfer >= float(old_gdp):
            raise ValueError("Reference-derived GDP transfer is invalid for the current source nation")
        old_nation["GDP"] = float(old_gdp) - gdp_transfer
        new_nation["GDP"] = float(new_gdp) + gdp_transfer
        gdp_reference = {
            **gdp_reference,
            "source_nation_gdp_before": float(old_gdp),
            "source_nation_gdp_after": float(old_nation["GDP"]),
            "destination_nation_gdp_before": float(new_gdp),
            "destination_nation_gdp_after": float(new_nation["GDP"]),
            "combined_gdp_before": float(old_gdp) + float(new_gdp),
            "combined_gdp_after": float(old_nation["GDP"]) + float(new_nation["GDP"]),
        }

    return {
        "fleet_ids": selected_fleet_ids,
        "fleet_details": fleet_details,
        "station_id": station_id,
        "station_name": station.get("displayName"),
        "station_orbit_id": target_orbit_id,
        "station_orbit_name": target_orbit.get("displayName"),
        "station_body_id": target_body_id,
        "removed_goal_ids": sorted(goal_ids),
        "removed_goal_references": removed_goal_references,
        "removed_time_event_ids": sorted(time_event_ids),
        "removed_alarm_records": removed_alarm_records,
        "region_id": region_id,
        "region_name": region.get("displayName") or region.get("templateName"),
        "from_nation_id": from_nation_id,
        "from_nation_name": old_nation.get("displayName") or old_nation.get("templateName"),
        "to_nation_id": to_nation_id,
        "to_nation_name": new_nation.get("displayName") or new_nation.get("templateName"),
        "region_transfer_scope": (
            "structural ownership plus reference-derived, globally conserved GDP; "
            "other national statistics and political side effects unchanged"
            if gdp_reference is not None
            else "structural ownership only; national economic/statistical side effects unchanged"
        ),
        "gdp_reference": gdp_reference,
    }


def mutate_remove_fleet(
    document: Dict[str, Any],
    fleet_id: int,
    expected_ship_count: int,
    expected_faction_id: Optional[int],
    expected_body: Optional[str],
) -> Dict[str, Any]:
    fleets = index_states(document, FLEET_TYPE)
    if fleet_id not in fleets:
        raise ValueError(f"Fleet ID {fleet_id} is not present")
    fleet = fleets[fleet_id]
    declared_ship_ids = fleet_ship_ids(fleet)
    if len(declared_ship_ids) != expected_ship_count:
        raise ValueError(
            f"Fleet {fleet_id} has {len(declared_ship_ids)} ships; expected {expected_ship_count}"
        )
    if len(set(declared_ship_ids)) != len(declared_ship_ids):
        raise ValueError(f"Fleet {fleet_id} contains duplicate ship references")
    faction_id = nested_ref(fleet, "faction")
    if expected_faction_id is not None and faction_id != expected_faction_id:
        raise ValueError(
            f"Fleet {fleet_id} belongs to faction {faction_id}; expected {expected_faction_id}"
        )

    orbits = index_states(document, ORBIT_TYPE)
    orbit_id = nested_ref(fleet, "orbitState")
    destination_id = nested_ref(fleet, "trajectory", "destinationOrbit")
    orbit_text = " ".join(
        str(orbits.get(identifier, {}).get("displayName") or "")
        for identifier in (orbit_id, destination_id)
        if identifier is not None
    )
    if expected_body and expected_body.lower() not in orbit_text.lower():
        raise ValueError(
            f"Fleet {fleet_id} current/destination orbit does not contain {expected_body!r}: {orbit_text!r}"
        )

    ships = index_states(document, SHIP_TYPE)
    missing_ships = [identifier for identifier in declared_ship_ids if identifier not in ships]
    if missing_ships:
        raise ValueError(f"Fleet references missing ship states: {missing_ships}")

    associated_ship_ids = [
        identifier
        for identifier, ship in ships.items()
        if nested_ref(ship, "fleet") == fleet_id
    ]
    if not set(declared_ship_ids).issubset(associated_ship_ids):
        raise ValueError("A declared fleet ship does not point back to its fleet")
    additional_associated_ship_ids = sorted(
        set(associated_ship_ids) - set(declared_ship_ids)
    )
    ship_ids = declared_ship_ids + additional_associated_ship_ids
    ship_id_set = set(ship_ids)

    officers = index_states(document, OFFICER_TYPE)
    officer_ids = {
        identifier
        for ship_id in ship_ids
        for identifier in (
            ref_id(item) for item in ships[ship_id].get("officers", [])
        )
        if identifier is not None
    }
    officer_ids.update(
        identifier
        for identifier, officer in officers.items()
        if nested_ref(officer, "ship") in ship_id_set
    )

    fleet_ids = {fleet_id}
    removed_ids = fleet_ids | ship_id_set | officer_ids
    initial_references = list(find_paths_to_ids(document, removed_ids))

    if remove_state_entries(document, FLEET_TYPE, fleet_ids) != 1:
        raise ValueError(f"Did not remove exactly one fleet state for ID {fleet_id}")
    if remove_state_entries(document, SHIP_TYPE, ship_id_set) != len(ship_ids):
        raise ValueError("Did not remove exactly the fleet's ship states")
    if remove_state_entries(document, OFFICER_TYPE, officer_ids) != len(officer_ids):
        raise ValueError("Did not remove exactly the fleet officers' states")

    faction_changes: List[Dict[str, int]] = []
    for faction_state_id, faction in index_states(document, FACTION_TYPE).items():
        changes = {
            "faction_id": faction_state_id,
            "fleets": remove_reference_items(faction.get("fleets"), fleet_ids),
            "intel": remove_keyed_reference_items(faction.get("intel"), removed_ids),
            "highestIntel": remove_keyed_reference_items(
                faction.get("highestIntel"), removed_ids
            ),
        }
        if any(value for key, value in changes.items() if key != "faction_id"):
            faction_changes.append(changes)

    cleanup_changes = cleanup_remaining_references(
        document, fleet_ids, removed_ids, ship_id_set
    )
    remaining_references = list(find_paths_to_ids(document, removed_ids))
    if remaining_references:
        formatted = ", ".join(path for path, _ in remaining_references[:10])
        raise ValueError(f"Unsupported references to removed IDs remain: {formatted}")

    return {
        "fleet_id": fleet_id,
        "fleet_name": fleet_display_name(fleet, faction_id),
        "faction_id": faction_id,
        "ship_ids": ship_ids,
        "declared_ship_ids": declared_ship_ids,
        "additional_associated_ship_ids": additional_associated_ship_ids,
        "officer_ids": sorted(officer_ids),
        "orbit_id": orbit_id,
        "destination_orbit_id": destination_id,
        "orbit_description": orbit_text,
        "initial_reference_count": len(initial_references),
        "faction_changes": faction_changes,
        "cleanup_changes": cleanup_changes,
    }


def mutate_remove_ships(
    document: Dict[str, Any],
    fleet_id: int,
    ship_ids: Sequence[int],
    expected_faction_id: Optional[int],
    expected_body: Optional[str],
    expected_hull: Optional[str],
) -> Dict[str, Any]:
    selected_ship_ids = list(ship_ids)
    if not selected_ship_ids:
        raise ValueError("At least one ship ID is required")
    if len(set(selected_ship_ids)) != len(selected_ship_ids):
        raise ValueError("Duplicate ship IDs were supplied")

    fleets = index_states(document, FLEET_TYPE)
    if fleet_id not in fleets:
        raise ValueError(f"Fleet ID {fleet_id} is not present")
    fleet = fleets[fleet_id]
    declared_ship_ids = fleet_ship_ids(fleet)
    selected_ship_id_set = set(selected_ship_ids)
    missing_memberships = sorted(selected_ship_id_set - set(declared_ship_ids))
    if missing_memberships:
        raise ValueError(
            f"Selected ships are not members of fleet {fleet_id}: {missing_memberships}"
        )
    if len(selected_ship_id_set) == len(declared_ship_ids):
        raise ValueError(
            "Ship removal would leave an empty fleet; use remove-fleet instead"
        )

    faction_id = nested_ref(fleet, "faction")
    if expected_faction_id is not None and faction_id != expected_faction_id:
        raise ValueError(
            f"Fleet {fleet_id} belongs to faction {faction_id}; expected {expected_faction_id}"
        )

    orbits = index_states(document, ORBIT_TYPE)
    orbit_id = nested_ref(fleet, "orbitState")
    destination_id = nested_ref(fleet, "trajectory", "destinationOrbit")
    orbit_text = " ".join(
        str(orbits.get(identifier, {}).get("displayName") or "")
        for identifier in (orbit_id, destination_id)
        if identifier is not None
    )
    if expected_body and expected_body.lower() not in orbit_text.lower():
        raise ValueError(
            f"Fleet {fleet_id} current/destination orbit does not contain {expected_body!r}: {orbit_text!r}"
        )

    ships = index_states(document, SHIP_TYPE)
    missing_ship_states = sorted(selected_ship_id_set - set(ships))
    if missing_ship_states:
        raise ValueError(f"Selected ship states are missing: {missing_ship_states}")
    incorrect_backrefs = sorted(
        identifier
        for identifier in selected_ship_ids
        if nested_ref(ships[identifier], "fleet") != fleet_id
    )
    if incorrect_backrefs:
        raise ValueError(
            f"Selected ships do not point back to fleet {fleet_id}: {incorrect_backrefs}"
        )

    hull_details: List[Dict[str, Any]] = []
    if expected_hull:
        factions = index_states(document, FACTION_TYPE)
        faction = factions.get(faction_id)
        if faction is None:
            raise ValueError(f"Owning faction state {faction_id} is missing")
        designs = faction.get("shipDesigns", [])
        if not isinstance(designs, list):
            raise ValueError(f"Faction {faction_id} shipDesigns is not an array")
        hull_by_template = {
            design.get("dataName"): design.get("hullName")
            for design in designs
            if isinstance(design, dict)
            and isinstance(design.get("dataName"), str)
        }
        for identifier in selected_ship_ids:
            ship = ships[identifier]
            template_name = ship.get("templateName")
            actual_hull = hull_by_template.get(template_name)
            hull_details.append(
                {
                    "ship_id": identifier,
                    "display_name": ship.get("displayName"),
                    "template_name": template_name,
                    "hull_name": actual_hull,
                }
            )
            if actual_hull != expected_hull:
                raise ValueError(
                    f"Ship {identifier} resolves to hull {actual_hull!r}; expected {expected_hull!r}"
                )

    officers = index_states(document, OFFICER_TYPE)
    officer_ids = {
        identifier
        for ship_id in selected_ship_ids
        for identifier in (ref_id(item) for item in ships[ship_id].get("officers", []))
        if identifier is not None
    }
    officer_ids.update(
        identifier
        for identifier, officer in officers.items()
        if nested_ref(officer, "ship") in selected_ship_id_set
    )

    removed_ids = selected_ship_id_set | officer_ids
    initial_references = list(find_paths_to_ids(document, removed_ids))

    removed_memberships = remove_reference_items(fleet.get("ships"), selected_ship_id_set)
    if removed_memberships != len(selected_ship_ids):
        raise ValueError(
            f"Removed {removed_memberships} fleet memberships; expected {len(selected_ship_ids)}"
        )
    if remove_state_entries(document, SHIP_TYPE, selected_ship_id_set) != len(
        selected_ship_ids
    ):
        raise ValueError("Did not remove exactly the selected ship states")
    if remove_state_entries(document, OFFICER_TYPE, officer_ids) != len(officer_ids):
        raise ValueError("Did not remove exactly the selected ships' officer states")

    faction_changes: List[Dict[str, int]] = []
    for faction_state_id, faction in index_states(document, FACTION_TYPE).items():
        changes = {
            "faction_id": faction_state_id,
            "intel": remove_keyed_reference_items(faction.get("intel"), removed_ids),
            "highestIntel": remove_keyed_reference_items(
                faction.get("highestIntel"), removed_ids
            ),
        }
        if any(value for key, value in changes.items() if key != "faction_id"):
            faction_changes.append(changes)

    cleanup_changes = cleanup_remaining_references(
        document, set(), removed_ids, selected_ship_id_set
    )
    remaining_references = list(find_paths_to_ids(document, removed_ids))
    if remaining_references:
        formatted = ", ".join(path for path, _ in remaining_references[:10])
        raise ValueError(f"Unsupported references to removed IDs remain: {formatted}")

    return {
        "fleet_id": fleet_id,
        "fleet_name": fleet_display_name(fleet, faction_id),
        "faction_id": faction_id,
        "ship_ids": selected_ship_ids,
        "ship_names": [ships[identifier].get("displayName") for identifier in selected_ship_ids],
        "expected_hull": expected_hull,
        "hull_details": hull_details,
        "officer_ids": sorted(officer_ids),
        "orbit_id": orbit_id,
        "destination_orbit_id": destination_id,
        "orbit_description": orbit_text,
        "remaining_fleet_ship_ids": fleet_ship_ids(fleet),
        "initial_reference_count": len(initial_references),
        "faction_changes": faction_changes,
        "cleanup_changes": cleanup_changes,
    }


def summarize_fleets(document: Dict[str, Any], body_filter: Optional[str]) -> List[Dict[str, Any]]:
    factions = index_states(document, FACTION_TYPE)
    orbits = index_states(document, ORBIT_TYPE)
    orbit_names = {
        identifier: str(value.get("displayName") or value.get("templateName") or "")
        for identifier, value in orbits.items()
    }
    faction_names = {
        identifier: str(
            value.get("displayName")
            or value.get("factionName")
            or value.get("templateName")
            or value.get("name")
            or ""
        )
        for identifier, value in factions.items()
    }

    summaries: List[Dict[str, Any]] = []
    for identifier, fleet in index_states(document, FLEET_TYPE).items():
        orbit_id = nested_ref(fleet, "orbitState")
        destination_id = nested_ref(fleet, "trajectory", "destinationOrbit")
        origin_id = nested_ref(fleet, "trajectory", "originOrbit")
        searchable = " ".join(
            [orbit_names.get(orbit_id, ""), orbit_names.get(destination_id, "")]
        ).lower()
        if body_filter and body_filter.lower() not in searchable:
            continue
        faction_id = nested_ref(fleet, "faction")
        summaries.append(
            {
                "id": identifier,
                "name": fleet_display_name(fleet, faction_id),
                "faction_id": faction_id,
                "faction": faction_names.get(faction_id, ""),
                "ship_ids": fleet_ship_ids(fleet),
                "orbit_id": orbit_id,
                "orbit": orbit_names.get(orbit_id, ""),
                "origin_orbit_id": origin_id,
                "origin_orbit": orbit_names.get(origin_id, ""),
                "destination_orbit_id": destination_id,
                "destination_orbit": orbit_names.get(destination_id, ""),
            }
        )
    return summaries


def command_inspect(args: argparse.Namespace) -> int:
    payload = read_payload(args.input)
    document = load_document(payload)
    metadata = entry_value(state_entries(document, METADATA_TYPE)[0])
    result = {
        "input": str(args.input),
        "compressed_sha256": sha256(args.input.read_bytes()),
        "payload_sha256": sha256(payload),
        "payload_bytes": len(payload),
        "line_count": payload.count(b"\n") + 1,
        "metadata": {
            "playerFactionName": metadata.get("playerFactionName"),
            "gameTimeString": metadata.get("gameTimeString"),
        },
        "fleets": summarize_fleets(document, args.body),
    }
    print(json.dumps(result, indent=2, ensure_ascii=False))
    return 0


def command_roundtrip_check(args: argparse.Namespace) -> int:
    source = read_payload(args.input)
    regenerated = encode_document(load_document(source))
    identical = source == regenerated
    result = {
        "input": str(args.input),
        "source_payload_sha256": sha256(source),
        "regenerated_payload_sha256": sha256(regenerated),
        "byte_identical": identical,
        "source_bytes": len(source),
        "regenerated_bytes": len(regenerated),
    }
    if not identical:
        limit = min(len(source), len(regenerated))
        mismatch = next((index for index in range(limit) if source[index] != regenerated[index]), limit)
        result["first_mismatch_offset"] = mismatch
        result["source_window_hex"] = source[mismatch : mismatch + 32].hex(" ")
        result["regenerated_window_hex"] = regenerated[mismatch : mismatch + 32].hex(" ")
    print(json.dumps(result, indent=2))
    return 0 if identical else 1


def command_references(args: argparse.Namespace) -> int:
    payload = read_payload(args.input)
    document = load_document(payload)
    references = [
        {"path": path, "id": identifier}
        for path, identifier in find_paths_to_ids(document, args.id)
    ]
    print(
        json.dumps(
            {
                "input": str(args.input),
                "ids": args.id,
                "reference_count": len(references),
                "references": references,
            },
            indent=2,
        )
    )
    return 0


def write_gzip_atomic(path: Path, payload: bytes) -> None:
    if path.exists():
        raise ValueError(f"Refusing to overwrite existing output: {path}")
    if path.suffix.lower() != ".gz":
        raise ValueError("Edited output must use the .gz extension")
    if not path.parent.is_dir():
        raise ValueError(f"Output directory does not exist: {path.parent}")
    temporary = path.with_name(path.name + ".tmp")
    if temporary.exists():
        raise ValueError(f"Temporary output already exists: {temporary}")
    try:
        temporary.write_bytes(gzip.compress(payload, compresslevel=9, mtime=0))
        os.replace(str(temporary), str(path))
    finally:
        if temporary.exists():
            temporary.unlink()


def command_remove_fleet(args: argparse.Namespace) -> int:
    if args.input.resolve() == args.output.resolve():
        raise ValueError("Input and output paths must be different")
    if args.output.exists():
        raise ValueError(f"Refusing to overwrite existing output: {args.output}")
    if args.audit and args.audit.exists():
        raise ValueError(f"Refusing to overwrite existing audit: {args.audit}")

    source_archive = args.input.read_bytes()
    source_archive_hash = sha256(source_archive)
    source_payload = read_payload(args.input)
    source_document = load_document(source_payload)
    source_roundtrip = encode_document(source_document)
    if source_roundtrip != source_payload:
        raise ValueError(
            "Source failed byte-identical FullSerializer round-trip; refusing to edit"
        )

    source_state_bytes = state_byte_map(source_document)
    source_nonfinite = nonfinite_counts(source_document)
    source_fullserializer_definitions = fullserializer_definition_map(source_document)
    source_reference_count, source_unresolved = reference_integrity(source_document)
    if source_unresolved:
        raise ValueError(
            f"Source contains unresolved $ref values; first: {source_unresolved[0]}"
        )
    source_current_id = serialized_node(source_document.get("currentID"))

    operation = mutate_remove_fleet(
        source_document,
        fleet_id=args.fleet_id,
        expected_ship_count=args.expected_ship_count,
        expected_faction_id=args.expected_faction_id,
        expected_body=args.expected_body,
    )
    operation["promoted_fullserializer_definitions"] = (
        promote_unresolved_fullserializer_definitions(
            source_document, source_fullserializer_definitions
        )
    )
    removed_ids = {
        operation["fleet_id"],
        *operation["ship_ids"],
        *operation["officer_ids"],
    }
    candidate_payload = encode_document(source_document)
    if not metadata_fast_scan_succeeds(candidate_payload):
        raise ValueError("Candidate fails the first-200-line metadata scan")

    # Serialize once, parse again, and require the independently reloaded object
    # to produce the exact same bytes before it can be written.
    candidate_document = load_document(candidate_payload)
    if encode_document(candidate_document) != candidate_payload:
        raise ValueError("Candidate payload is not byte-stable after reload")
    if serialized_node(candidate_document.get("currentID")) != source_current_id:
        raise ValueError("currentID changed during fleet removal")
    if list(find_paths_to_ids(candidate_document, removed_ids)):
        raise ValueError("Candidate still contains references to removed state IDs")

    candidate_reference_count, candidate_unresolved = reference_integrity(candidate_document)
    if candidate_unresolved:
        raise ValueError(
            f"Candidate contains unresolved $ref values; first: {candidate_unresolved[0]}"
        )

    candidate_state_bytes = state_byte_map(candidate_document)
    source_keys = set(source_state_bytes)
    candidate_keys = set(candidate_state_bytes)
    removed_state_keys = sorted(source_keys - candidate_keys)
    added_state_keys = sorted(candidate_keys - source_keys)
    expected_removed_state_keys = sorted(
        [(FLEET_TYPE, operation["fleet_id"])]
        + [(SHIP_TYPE, identifier) for identifier in operation["ship_ids"]]
        + [(OFFICER_TYPE, identifier) for identifier in operation["officer_ids"]]
    )
    if removed_state_keys != expected_removed_state_keys:
        raise ValueError(f"Unexpected removed states: {removed_state_keys}")
    if added_state_keys:
        raise ValueError(f"Unexpected added states: {added_state_keys}")

    changed_retained_states = sorted(
        key
        for key in source_keys & candidate_keys
        if source_state_bytes[key] != candidate_state_bytes[key]
    )
    metadata_keys = [key for key in source_keys if key[0] == METADATA_TYPE]
    if any(key in changed_retained_states for key in metadata_keys):
        raise ValueError("Metadata state changed during fleet removal")

    write_gzip_atomic(args.output, candidate_payload)
    written_archive = args.output.read_bytes()
    written_payload = gzip.decompress(written_archive)
    if written_payload != candidate_payload:
        raise ValueError("Written gzip payload differs from the validated candidate bytes")
    if encode_document(load_document(written_payload)) != written_payload:
        raise ValueError("Written output fails byte-identical parse/serialize validation")
    if sha256(args.input.read_bytes()) != source_archive_hash:
        raise ValueError("Source archive changed during editing")

    audit = {
        "tool": "ti_save_tool.py",
        "operation": "remove-fleet",
        "input": str(args.input),
        "output": str(args.output),
        "source_archive_sha256": source_archive_hash,
        "source_payload_sha256": sha256(source_payload),
        "candidate_archive_sha256": sha256(written_archive),
        "candidate_payload_sha256": sha256(written_payload),
        "source_payload_bytes": len(source_payload),
        "candidate_payload_bytes": len(written_payload),
        "source_roundtrip_byte_identical": source_roundtrip == source_payload,
        "candidate_roundtrip_byte_identical": encode_document(candidate_document)
        == candidate_payload,
        "written_payload_byte_identical_to_validated_candidate": written_payload
        == candidate_payload,
        "metadata_fast_scan": metadata_fast_scan_succeeds(written_payload),
        "operation_details": operation,
        "removed_state_keys": [
            {"type": type_name, "id": identifier}
            for type_name, identifier in removed_state_keys
        ],
        "added_state_count": len(added_state_keys),
        "retained_state_count": len(source_keys & candidate_keys),
        "byte_identical_retained_state_count": len(source_keys & candidate_keys)
        - len(changed_retained_states),
        "changed_retained_states": [
            {"type": type_name, "id": identifier}
            for type_name, identifier in changed_retained_states
        ],
        "remaining_references_to_removed_ids": 0,
        "source_fullserializer_ref_count": source_reference_count,
        "candidate_fullserializer_ref_count": candidate_reference_count,
        "candidate_unresolved_fullserializer_refs": 0,
        "source_nonfinite_tokens": source_nonfinite,
        "candidate_nonfinite_tokens": nonfinite_counts(candidate_document),
        "source_unchanged": sha256(args.input.read_bytes()) == source_archive_hash,
    }
    if args.audit:
        args.audit.write_text(json.dumps(audit, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(audit, indent=2))
    return 0


def command_remove_ships(args: argparse.Namespace) -> int:
    if args.input.resolve() == args.output.resolve():
        raise ValueError("Input and output paths must be different")
    if args.output.exists():
        raise ValueError(f"Refusing to overwrite existing output: {args.output}")
    if args.audit and args.audit.exists():
        raise ValueError(f"Refusing to overwrite existing audit: {args.audit}")

    source_archive = args.input.read_bytes()
    source_archive_hash = sha256(source_archive)
    source_payload = read_payload(args.input)
    source_document = load_document(source_payload)
    source_roundtrip = encode_document(source_document)
    if source_roundtrip != source_payload:
        raise ValueError(
            "Source failed byte-identical FullSerializer round-trip; refusing to edit"
        )

    source_state_bytes = state_byte_map(source_document)
    source_nonfinite = nonfinite_counts(source_document)
    source_fullserializer_definitions = fullserializer_definition_map(source_document)
    source_reference_count, source_unresolved = reference_integrity(source_document)
    if source_unresolved:
        raise ValueError(
            f"Source contains unresolved $ref values; first: {source_unresolved[0]}"
        )
    source_current_id = serialized_node(source_document.get("currentID"))

    operation = mutate_remove_ships(
        source_document,
        fleet_id=args.fleet_id,
        ship_ids=args.ship_id,
        expected_faction_id=args.expected_faction_id,
        expected_body=args.expected_body,
        expected_hull=args.expected_hull,
    )
    operation["promoted_fullserializer_definitions"] = (
        promote_unresolved_fullserializer_definitions(
            source_document, source_fullserializer_definitions
        )
    )
    removed_ids = {*operation["ship_ids"], *operation["officer_ids"]}
    candidate_payload = encode_document(source_document)
    if not metadata_fast_scan_succeeds(candidate_payload):
        raise ValueError("Candidate fails the first-200-line metadata scan")

    candidate_document = load_document(candidate_payload)
    if encode_document(candidate_document) != candidate_payload:
        raise ValueError("Candidate payload is not byte-stable after reload")
    if serialized_node(candidate_document.get("currentID")) != source_current_id:
        raise ValueError("currentID changed during ship removal")
    if list(find_paths_to_ids(candidate_document, removed_ids)):
        raise ValueError("Candidate still contains references to removed state IDs")

    candidate_reference_count, candidate_unresolved = reference_integrity(candidate_document)
    if candidate_unresolved:
        raise ValueError(
            f"Candidate contains unresolved $ref values; first: {candidate_unresolved[0]}"
        )

    candidate_state_bytes = state_byte_map(candidate_document)
    source_keys = set(source_state_bytes)
    candidate_keys = set(candidate_state_bytes)
    removed_state_keys = sorted(source_keys - candidate_keys)
    added_state_keys = sorted(candidate_keys - source_keys)
    expected_removed_state_keys = sorted(
        [(SHIP_TYPE, identifier) for identifier in operation["ship_ids"]]
        + [(OFFICER_TYPE, identifier) for identifier in operation["officer_ids"]]
    )
    if removed_state_keys != expected_removed_state_keys:
        raise ValueError(f"Unexpected removed states: {removed_state_keys}")
    if added_state_keys:
        raise ValueError(f"Unexpected added states: {added_state_keys}")

    changed_retained_states = sorted(
        key
        for key in source_keys & candidate_keys
        if source_state_bytes[key] != candidate_state_bytes[key]
    )
    metadata_keys = [key for key in source_keys if key[0] == METADATA_TYPE]
    if any(key in changed_retained_states for key in metadata_keys):
        raise ValueError("Metadata state changed during ship removal")

    write_gzip_atomic(args.output, candidate_payload)
    written_archive = args.output.read_bytes()
    written_payload = gzip.decompress(written_archive)
    if written_payload != candidate_payload:
        raise ValueError("Written gzip payload differs from the validated candidate bytes")
    if encode_document(load_document(written_payload)) != written_payload:
        raise ValueError("Written output fails byte-identical parse/serialize validation")
    if sha256(args.input.read_bytes()) != source_archive_hash:
        raise ValueError("Source archive changed during editing")

    audit = {
        "tool": "ti_save_tool.py",
        "operation": "remove-ships",
        "input": str(args.input),
        "output": str(args.output),
        "source_archive_sha256": source_archive_hash,
        "source_payload_sha256": sha256(source_payload),
        "candidate_archive_sha256": sha256(written_archive),
        "candidate_payload_sha256": sha256(written_payload),
        "source_payload_bytes": len(source_payload),
        "candidate_payload_bytes": len(written_payload),
        "source_roundtrip_byte_identical": source_roundtrip == source_payload,
        "candidate_roundtrip_byte_identical": encode_document(candidate_document)
        == candidate_payload,
        "written_payload_byte_identical_to_validated_candidate": written_payload
        == candidate_payload,
        "metadata_fast_scan": metadata_fast_scan_succeeds(written_payload),
        "operation_details": operation,
        "removed_state_keys": [
            {"type": type_name, "id": identifier}
            for type_name, identifier in removed_state_keys
        ],
        "added_state_count": len(added_state_keys),
        "retained_state_count": len(source_keys & candidate_keys),
        "byte_identical_retained_state_count": len(source_keys & candidate_keys)
        - len(changed_retained_states),
        "changed_retained_states": [
            {"type": type_name, "id": identifier}
            for type_name, identifier in changed_retained_states
        ],
        "remaining_references_to_removed_ids": 0,
        "source_fullserializer_ref_count": source_reference_count,
        "candidate_fullserializer_ref_count": candidate_reference_count,
        "candidate_unresolved_fullserializer_refs": 0,
        "source_nonfinite_tokens": source_nonfinite,
        "candidate_nonfinite_tokens": nonfinite_counts(candidate_document),
        "source_unchanged": sha256(args.input.read_bytes()) == source_archive_hash,
    }
    if args.audit:
        args.audit.write_text(json.dumps(audit, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(audit, indent=2))
    return 0


def command_relocate_fleets_transfer_region(args: argparse.Namespace) -> int:
    if args.input.resolve() == args.output.resolve():
        raise ValueError("Input and output paths must be different")
    if args.output.exists():
        raise ValueError(f"Refusing to overwrite existing output: {args.output}")
    if args.audit and args.audit.exists():
        raise ValueError(f"Refusing to overwrite existing audit: {args.audit}")

    source_archive = args.input.read_bytes()
    source_archive_hash = sha256(source_archive)
    source_payload = read_payload(args.input)
    source_document = load_document(source_payload)
    source_roundtrip = encode_document(source_document)
    if source_roundtrip != source_payload:
        raise ValueError(
            "Source failed byte-identical FullSerializer round-trip; refusing to edit"
        )

    source_state_bytes = state_byte_map(source_document)
    source_nonfinite = nonfinite_counts(source_document)
    source_fullserializer_definitions = fullserializer_definition_map(source_document)
    source_reference_count, source_unresolved = reference_integrity(source_document)
    if source_unresolved:
        raise ValueError(
            f"Source contains unresolved $ref values; first: {source_unresolved[0]}"
        )
    source_current_id = serialized_node(source_document.get("currentID"))

    gdp_reference = None
    if args.reference_input:
        reference_archive = args.reference_input.read_bytes()
        reference_payload = read_payload(args.reference_input)
        reference_document = load_document(reference_payload)
        if encode_document(reference_document) != reference_payload:
            raise ValueError(
                "GDP reference save failed byte-identical FullSerializer round-trip"
            )
        current_region = index_states(source_document, REGION_TYPE).get(args.region_id)
        if current_region is None or not isinstance(
            current_region.get("populationInMillions"), (int, float)
        ):
            raise ValueError("Current region has no valid population")
        gdp_reference = calculate_reference_region_gdp(
            reference_document,
            region_id=args.region_id,
            expected_nation_id=args.to_nation_id,
            current_population_millions=float(current_region["populationInMillions"]),
        )
        gdp_reference.update(
            {
                "reference_input": str(args.reference_input),
                "reference_archive_sha256": sha256(reference_archive),
                "reference_payload_sha256": sha256(reference_payload),
                "reference_roundtrip_byte_identical": True,
            }
        )

    operation = mutate_relocate_fleets_and_transfer_region(
        source_document,
        fleet_ids=args.fleet_id,
        station_id=args.station_id,
        expected_faction_id=args.expected_faction_id,
        expected_destination_orbit_id=args.expected_destination_orbit_id,
        region_id=args.region_id,
        from_nation_id=args.from_nation_id,
        to_nation_id=args.to_nation_id,
        gdp_reference=gdp_reference,
    )
    operation["promoted_fullserializer_definitions"] = (
        promote_unresolved_fullserializer_definitions(
            source_document, source_fullserializer_definitions
        )
    )
    removed_ids = {
        *operation["removed_goal_ids"],
        *operation["removed_time_event_ids"],
    }

    candidate_payload = encode_document(source_document)
    if not metadata_fast_scan_succeeds(candidate_payload):
        raise ValueError("Candidate fails the first-200-line metadata scan")
    candidate_document = load_document(candidate_payload)
    if encode_document(candidate_document) != candidate_payload:
        raise ValueError("Candidate payload is not byte-stable after reload")
    if serialized_node(candidate_document.get("currentID")) != source_current_id:
        raise ValueError("currentID changed during compound save edit")
    remaining_removed_references = list(
        find_paths_to_ids(candidate_document, removed_ids)
    )
    if remaining_removed_references:
        raise ValueError(
            "Candidate still contains references to removed goal/event IDs; first: "
            f"{remaining_removed_references[0]}"
        )

    candidate_reference_count, candidate_unresolved = reference_integrity(
        candidate_document
    )
    if candidate_unresolved:
        raise ValueError(
            f"Candidate contains unresolved $ref values; first: {candidate_unresolved[0]}"
        )

    candidate_fleets = index_states(candidate_document, FLEET_TYPE)
    candidate_habs = index_states(candidate_document, HAB_TYPE)
    candidate_orbits = index_states(candidate_document, ORBIT_TYPE)
    for fleet_id in operation["fleet_ids"]:
        fleet = candidate_fleets[fleet_id]
        if fleet.get("trajectory") is not None:
            raise ValueError(f"Fleet {fleet_id} retained a trajectory")
        if nested_ref(fleet, "orbitState") != operation["station_orbit_id"]:
            raise ValueError(f"Fleet {fleet_id} does not reference the target orbit")
        if nested_ref(fleet, "dockedLocation") != operation["station_id"]:
            raise ValueError(f"Fleet {fleet_id} does not reference the target station")
    candidate_station = candidate_habs[operation["station_id"]]
    candidate_orbit = candidate_orbits[operation["station_orbit_id"]]
    candidate_station_fleets = {
        ref_id(item) for item in candidate_station.get("dockedFleets", [])
    }
    candidate_orbit_assets = {
        ref_id(item) for item in candidate_orbit.get("assetsInOrbit", [])
    }
    candidate_layout_fleets = {
        ref_id(item)
        for item in candidate_station.get("_dockedShipAbovePositions", {}).get(
            "_itemList", []
        )
        if item is not None
    }
    wanted_fleets = set(operation["fleet_ids"])
    if not wanted_fleets.issubset(candidate_station_fleets):
        raise ValueError("Target station is missing a relocated fleet")
    if not wanted_fleets.issubset(candidate_orbit_assets):
        raise ValueError("Target orbit is missing a relocated fleet")
    if not wanted_fleets.issubset(candidate_layout_fleets):
        raise ValueError("Target docking layout is missing a relocated fleet")

    candidate_nations = index_states(candidate_document, NATION_TYPE)
    candidate_region = index_states(candidate_document, REGION_TYPE)[
        operation["region_id"]
    ]
    old_region_ids = {
        ref_id(item)
        for item in candidate_nations[operation["from_nation_id"]].get("regions", [])
    }
    new_region_ids = {
        ref_id(item)
        for item in candidate_nations[operation["to_nation_id"]].get("regions", [])
    }
    if nested_ref(candidate_region, "nation") != operation["to_nation_id"]:
        raise ValueError("Transferred region does not reference its new nation")
    if operation["region_id"] in old_region_ids:
        raise ValueError("Transferred region remains in the old nation's collection")
    if operation["region_id"] not in new_region_ids:
        raise ValueError("Transferred region is absent from the new nation's collection")

    candidate_state_bytes = state_byte_map(candidate_document)
    source_keys = set(source_state_bytes)
    candidate_keys = set(candidate_state_bytes)
    removed_state_keys = sorted(source_keys - candidate_keys)
    added_state_keys = sorted(candidate_keys - source_keys)
    expected_removed_state_keys = sorted(
        [(TIME_EVENT_TYPE, identifier) for identifier in operation["removed_time_event_ids"]]
        + [
            (
                ATTACK_FLEET_GOAL_TYPE
                if (ATTACK_FLEET_GOAL_TYPE, identifier) in source_keys
                else JOIN_FLEET_GOAL_TYPE,
                identifier,
            )
            for identifier in operation["removed_goal_ids"]
        ]
    )
    if removed_state_keys != expected_removed_state_keys:
        raise ValueError(f"Unexpected removed states: {removed_state_keys}")
    if added_state_keys:
        raise ValueError(f"Unexpected added states: {added_state_keys}")

    changed_retained_states = sorted(
        key
        for key in source_keys & candidate_keys
        if source_state_bytes[key] != candidate_state_bytes[key]
    )
    allowed_changed_states = {
        *((FLEET_TYPE, identifier) for identifier in operation["fleet_ids"]),
        (HAB_TYPE, operation["station_id"]),
        (ORBIT_TYPE, operation["station_orbit_id"]),
        (REGION_TYPE, operation["region_id"]),
        (NATION_TYPE, operation["from_nation_id"]),
        (NATION_TYPE, operation["to_nation_id"]),
        (FACTION_TYPE, args.expected_faction_id),
    }
    # Arrival alarms belong to whichever observing faction created them, so
    # discover any additional changed faction state rather than hard-coding a
    # player-faction ID. No other state type may change.
    unexpected_changes = [
        key
        for key in changed_retained_states
        if key not in allowed_changed_states and key[0] != FACTION_TYPE
    ]
    if unexpected_changes:
        raise ValueError(f"Unexpected retained state changes: {unexpected_changes}")
    metadata_keys = [key for key in source_keys if key[0] == METADATA_TYPE]
    if any(key in changed_retained_states for key in metadata_keys):
        raise ValueError("Metadata state changed during compound save edit")

    write_gzip_atomic(args.output, candidate_payload)
    written_archive = args.output.read_bytes()
    written_payload = gzip.decompress(written_archive)
    if written_payload != candidate_payload:
        raise ValueError("Written gzip payload differs from the validated candidate bytes")
    if encode_document(load_document(written_payload)) != written_payload:
        raise ValueError("Written output fails byte-identical parse/serialize validation")
    if sha256(args.input.read_bytes()) != source_archive_hash:
        raise ValueError("Source archive changed during editing")

    audit = {
        "tool": "ti_save_tool.py",
        "operation": "relocate-fleets-transfer-region",
        "input": str(args.input),
        "output": str(args.output),
        "source_archive_sha256": source_archive_hash,
        "source_payload_sha256": sha256(source_payload),
        "candidate_archive_sha256": sha256(written_archive),
        "candidate_payload_sha256": sha256(written_payload),
        "source_payload_bytes": len(source_payload),
        "candidate_payload_bytes": len(written_payload),
        "source_roundtrip_byte_identical": source_roundtrip == source_payload,
        "candidate_roundtrip_byte_identical": encode_document(candidate_document)
        == candidate_payload,
        "written_payload_byte_identical_to_validated_candidate": written_payload
        == candidate_payload,
        "metadata_fast_scan": metadata_fast_scan_succeeds(written_payload),
        "operation_details": operation,
        "removed_state_keys": [
            {"type": type_name, "id": identifier}
            for type_name, identifier in removed_state_keys
        ],
        "added_state_count": len(added_state_keys),
        "retained_state_count": len(source_keys & candidate_keys),
        "byte_identical_retained_state_count": len(source_keys & candidate_keys)
        - len(changed_retained_states),
        "changed_retained_states": [
            {"type": type_name, "id": identifier}
            for type_name, identifier in changed_retained_states
        ],
        "remaining_references_to_removed_ids": 0,
        "source_fullserializer_ref_count": source_reference_count,
        "candidate_fullserializer_ref_count": candidate_reference_count,
        "candidate_unresolved_fullserializer_refs": 0,
        "source_nonfinite_tokens": source_nonfinite,
        "candidate_nonfinite_tokens": nonfinite_counts(candidate_document),
        "source_unchanged": sha256(args.input.read_bytes()) == source_archive_hash,
    }
    if args.audit:
        args.audit.write_text(json.dumps(audit, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(audit, indent=2))
    return 0


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    subparsers = parser.add_subparsers(dest="command", required=True)

    inspect_parser = subparsers.add_parser("inspect", help="List fleet state around a body")
    inspect_parser.add_argument("--input", type=Path, required=True)
    inspect_parser.add_argument("--body", help="Case-insensitive substring in the current/destination orbit name")
    inspect_parser.set_defaults(func=command_inspect)

    roundtrip_parser = subparsers.add_parser(
        "roundtrip-check",
        help="Require a parse/serialize no-op to reproduce the native payload byte for byte",
    )
    roundtrip_parser.add_argument("--input", type=Path, required=True)
    roundtrip_parser.set_defaults(func=command_roundtrip_check)

    references_parser = subparsers.add_parser(
        "references", help="List every serialized GameStateID-shaped path to selected IDs"
    )
    references_parser.add_argument("--input", type=Path, required=True)
    references_parser.add_argument("--id", type=int, action="append", required=True)
    references_parser.set_defaults(func=command_references)

    remove_parser = subparsers.add_parser(
        "remove-fleet",
        help="Remove one fleet and its ships with normal serialized relationship cleanup",
    )
    remove_parser.add_argument("--input", type=Path, required=True)
    remove_parser.add_argument("--output", type=Path, required=True)
    remove_parser.add_argument("--audit", type=Path)
    remove_parser.add_argument("--fleet-id", type=int, required=True)
    remove_parser.add_argument("--expected-ship-count", type=int, required=True)
    remove_parser.add_argument("--expected-faction-id", type=int)
    remove_parser.add_argument("--expected-body")
    remove_parser.set_defaults(func=command_remove_fleet)

    remove_ships_parser = subparsers.add_parser(
        "remove-ships",
        help="Remove selected ships while retaining their non-empty fleet",
    )
    remove_ships_parser.add_argument("--input", type=Path, required=True)
    remove_ships_parser.add_argument("--output", type=Path, required=True)
    remove_ships_parser.add_argument("--audit", type=Path)
    remove_ships_parser.add_argument("--fleet-id", type=int, required=True)
    remove_ships_parser.add_argument(
        "--ship-id", type=int, action="append", required=True
    )
    remove_ships_parser.add_argument("--expected-faction-id", type=int)
    remove_ships_parser.add_argument("--expected-body")
    remove_ships_parser.add_argument("--expected-hull")
    remove_ships_parser.set_defaults(func=command_remove_ships)

    relocate_parser = subparsers.add_parser(
        "relocate-fleets-transfer-region",
        help=(
            "Dock selected in-transit fleets at one station and repair one region's "
            "structural nation ownership"
        ),
    )
    relocate_parser.add_argument("--input", type=Path, required=True)
    relocate_parser.add_argument(
        "--reference-input",
        type=Path,
        help=(
            "Optional intact pre-transfer save used to restore the region's "
            "historical GDP per capita while conserving combined GDP"
        ),
    )
    relocate_parser.add_argument("--output", type=Path, required=True)
    relocate_parser.add_argument("--audit", type=Path)
    relocate_parser.add_argument(
        "--fleet-id", type=int, action="append", required=True
    )
    relocate_parser.add_argument("--station-id", type=int, required=True)
    relocate_parser.add_argument("--expected-faction-id", type=int, required=True)
    relocate_parser.add_argument(
        "--expected-destination-orbit-id", type=int, required=True
    )
    relocate_parser.add_argument("--region-id", type=int, required=True)
    relocate_parser.add_argument("--from-nation-id", type=int, required=True)
    relocate_parser.add_argument("--to-nation-id", type=int, required=True)
    relocate_parser.set_defaults(func=command_relocate_fleets_transfer_region)

    return parser


def main(argv: Optional[Sequence[str]] = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)
    try:
        return int(args.func(args))
    except (OSError, ValueError, KeyError, json.JSONDecodeError) as exc:
        parser.error(str(exc))
        return 2


if __name__ == "__main__":
    sys.exit(main())
