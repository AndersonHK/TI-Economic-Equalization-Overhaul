"""Calibrate TI starting Environment ratings against EDGAR country inventories.

Requires openpyxl and the official EDGAR 2025 CO2/CH4/N2O zip downloads. The
generated CSV is a persistent audit artifact; --apply also updates greenEconomy
in the mod's 2003, 2022, and 2026 nation overrides.
"""

import argparse
import csv
import io
import json
import math
import pathlib
import zipfile

import openpyxl


CO2_BASE = 2_000_000.0
CO2_DECAY = 0.25
CH4_BASE = 51_248.90
CH4_DECAY = 0.90
N2O_BASE = 1_452.04
N2O_DECAY = 0.90
STORAGE_OFFSET = 0.10
STARTING_CAP = 3.0
SCENARIO_SCALE = {2003: 1.59, 2022: 1.0, 2026: 0.87}
INVENTORY_YEAR = {2003: 2003, 2022: 2022, 2026: 2024}


def load_json(path):
    with path.open(encoding="utf-8-sig") as handle:
        return json.load(handle)


def inventory(zip_path, year):
    with zipfile.ZipFile(zip_path) as archive:
        workbook_name = next(name for name in archive.namelist() if name.endswith(".xlsx"))
        workbook = openpyxl.load_workbook(
            io.BytesIO(archive.read(workbook_name)), read_only=True, data_only=True
        )
    sheet = workbook["TOTALS BY COUNTRY"]
    rows = sheet.iter_rows(values_only=True)
    header = next(row for row in rows if row and row[0] == "IPCC_annex")
    year_index = list(header).index(f"Y_{year}")
    return {
        str(row[2]): float(row[year_index] or 0.0) * 1000.0
        for row in rows
        if row and row[2]
    }


def inherited(entry, base_by_name):
    value = dict(base_by_name.get(entry.get("referenceAlias"), {}))
    value.update(entry)
    return value


def scenario_rows(year, game, mod_entries, inventories):
    templates = game / "TerraInvicta_Data/StreamingAssets/Templates"
    vanilla = load_json(templates / "TINationTemplate.json")
    base_by_name = {row["dataName"]: row for row in vanilla if "_" not in row["dataName"]}
    vanilla_by_name = {row["dataName"]: row for row in vanilla}
    if year == 2003:
        scenario_templates = game / "DLC_Content/DarkSkies/2003_Scenario/Templates"
        source_by_name = {
            row["dataName"]: row for row in load_json(scenario_templates / "TINationTemplate.json")
        }
        region_entries = load_json(scenario_templates / "TIRegionTemplate.json")
        selected = [row for row in mod_entries if row["dataName"].startswith("2003_")]
    else:
        source_by_name = vanilla_by_name
        prefix = "2026_" if year == 2026 else ""
        selected = [
            row for row in mod_entries
            if (row["dataName"].startswith(prefix) if prefix else "_" not in row["dataName"])
        ]
        region_entries = load_json(templates / "TIRegionTemplate.json")
        region_entries = [
            row for row in region_entries
            if (row["dataName"].startswith(prefix) if prefix else "_" not in row["dataName"])
        ]

    population_by_name = {}
    for region in region_entries:
        name = region.get("sortNation")
        if name:
            population_by_name[name] = population_by_name.get(name, 0.0) + float(
                region.get("population_Millions", 0.0) or 0.0
            )

    result = []
    for override in selected:
        original = source_by_name.get(override["dataName"])
        if original is None:
            continue
        merged = inherited(original, base_by_name)
        merged.update(override)
        alias = merged.get("referenceAlias", merged["dataName"].split("_")[-1])
        alias_entry = base_by_name.get(alias, merged)
        iso_codes = list(merged.get("ISOCodes") or alias_entry.get("ISOCodes") or [])
        actual = {
            gas: sum(inventories[(year, gas)].get(code, 0.0) for code in iso_codes)
            for gas in ("CO2", "CH4", "N2O")
        }
        gdp_billions = float(merged.get("initialGDP", 0.0) or 0.0) * SCENARIO_SCALE[year] / 1e9
        if actual["CO2"] > 0.0 and gdp_billions > 0.0:
            intensity = actual["CO2"] / gdp_billions
            rating = math.log(intensity / CO2_BASE) / math.log(CO2_DECAY)
            calibrated = True
        else:
            old_stored = float(merged.get("greenEconomy", 1.0) or 1.0)
            rating = max(0.0, min(STARTING_CAP, 1.0 / old_stored))
            calibrated = False
        if rating < -1e-9 or rating > STARTING_CAP + 1e-9:
            raise ValueError(
                f"{year} {override['dataName']} requires rating {rating:.6f}, outside 0-{STARTING_CAP}"
            )
        rating = max(0.0, min(STARTING_CAP, rating))
        stored = 1.0 / (rating + STORAGE_OFFSET)
        population = population_by_name.get(alias_entry.get("friendlyName", ""), 0.0)
        result.append(
            {
                "scenario": year,
                "inventory_year": INVENTORY_YEAR[year],
                "data_name": override["dataName"],
                "alias": alias,
                "iso": ";".join(iso_codes),
                "gdp_billions": gdp_billions,
                "population_millions": population,
                "rating": rating,
                "stored_green_economy": stored,
                "calibrated_from_edgar": calibrated,
                "actual_co2_t": actual["CO2"],
                "predicted_co2_t": gdp_billions * CO2_BASE * CO2_DECAY ** rating,
                "actual_ch4_t": actual["CH4"],
                "predicted_ch4_t": population * CH4_BASE * CH4_DECAY ** rating,
                "actual_n2o_t": actual["N2O"],
                "predicted_n2o_t": population * N2O_BASE * N2O_DECAY ** rating,
            }
        )
    return result


def write_csv(path, rows):
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", newline="", encoding="utf-8") as handle:
        writer = csv.DictWriter(handle, fieldnames=list(rows[0]))
        writer.writeheader()
        writer.writerows(rows)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--game", type=pathlib.Path, required=True)
    parser.add_argument("--data-dir", type=pathlib.Path, required=True)
    parser.add_argument("--repo", type=pathlib.Path, default=pathlib.Path(__file__).resolve().parents[1])
    parser.add_argument("--apply", action="store_true")
    args = parser.parse_args()

    inventory_files = {
        "CO2": "IEA_EDGAR_CO2_1970_2024.zip",
        "CH4": "EDGAR_CH4_1970_2024.zip",
        "N2O": "EDGAR_N2O_1970_2024.zip",
    }
    inventories = {
        (scenario, gas): inventory(args.data_dir / filename, INVENTORY_YEAR[scenario])
        for scenario in (2003, 2022, 2026)
        for gas, filename in inventory_files.items()
    }
    mod_path = args.repo / "TIEconomyMod/ModFiles/TINationTemplate.json"
    mod_entries = load_json(mod_path)
    rows = []
    for year in (2003, 2022, 2026):
        rows.extend(scenario_rows(year, args.game, mod_entries, inventories))

    by_name = {row["data_name"]: row for row in rows}
    if args.apply:
        for entry in mod_entries:
            row = by_name.get(entry["dataName"])
            if row:
                entry["greenEconomy"] = round(row["stored_green_economy"], 7)
        with mod_path.open("w", encoding="utf-8", newline="\n") as handle:
            json.dump(mod_entries, handle, indent=4, ensure_ascii=False)
            handle.write("\n")

    audit_path = args.repo / "docs/environment-calibration/historical-start-calibration.csv"
    write_csv(audit_path, rows)
    for year in (2003, 2022, 2026):
        matched = [row for row in rows if row["scenario"] == year and row["calibrated_from_edgar"]]
        print(
            year,
            "rating", f"{min(row['rating'] for row in matched):.4f}-{max(row['rating'] for row in matched):.4f}",
            "CO2", f"{sum(row['predicted_co2_t'] for row in matched) / sum(row['actual_co2_t'] for row in matched):.4f}",
            "CH4", f"{sum(row['predicted_ch4_t'] for row in matched) / sum(row['actual_ch4_t'] for row in matched):.4f}",
            "N2O", f"{sum(row['predicted_n2o_t'] for row in matched) / sum(row['actual_n2o_t'] for row in matched):.4f}",
        )


if __name__ == "__main__":
    main()
