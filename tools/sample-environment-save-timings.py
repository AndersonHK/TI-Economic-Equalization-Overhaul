"""Measure hypothetical Environment 5-to-10 timing from local TI saves."""

import argparse
import csv
import gzip
import json
import math
import pathlib
import re


REFERENCE_GDP_BILLIONS = 100.0
ADVANCEMENT_BASE_IP = 0.125
ADVANCEMENT_GROWTH_BASE = 1.5
LOW_INCOME_FLOOR = 0.70
LOW_INCOME_THRESHOLD = 15_000.0
IP_OUTPUT_MULTIPLIER = 1.05
STORAGE_OFFSET = 0.10


def advancement_cost(gdp_billions, start=5.0, end=10.0, cap=10.0):
    start_level = 10.0 * start / cap
    end_level = 10.0 * end / cap
    normalized = (
        ADVANCEMENT_GROWTH_BASE ** end_level - ADVANCEMENT_GROWTH_BASE ** start_level
    ) / (ADVANCEMENT_GROWTH_BASE - 1.0)
    return (
        gdp_billions / REFERENCE_GDP_BILLIONS * ADVANCEMENT_BASE_IP * normalized
    )


def read_sample(path):
    with gzip.open(path, "rt", encoding="utf-8-sig") as handle:
        save = json.load(handle)
    states = save.get("gamestates", {})
    metadata_entries = states.get("PavonisInteractive.TerraInvicta.TIMetadataState", [])
    metadata = metadata_entries[0].get("Value", {}) if metadata_entries else {}
    nations = states.get("PavonisInteractive.TerraInvicta.TINationState", [])
    eu_entry = next(
        (entry.get("Value", {}) for entry in nations if entry.get("Value", {}).get("templateName") == "EUA"),
        None,
    )
    if eu_entry is None:
        return None
    gdp_billions = float(eu_entry.get("GDP", 0.0)) / 1e9
    populations = eu_entry.get("historyPopulation") or []
    population_millions = float(populations[0]) if populations else 0.0
    pcgdp = gdp_billions * 1000.0 / population_millions if population_millions > 0 else 0.0
    income_progress = max(0.0, min(1.0, pcgdp / LOW_INCOME_THRESHOLD))
    income_multiplier = LOW_INCOME_FLOOR + (1.0 - LOW_INCOME_FLOOR) * income_progress
    calculated_monthly_ip = (
        gdp_billions / REFERENCE_GDP_BILLIONS * income_multiplier * IP_OUTPUT_MULTIPLIER
    )
    saved_monthly_ip = float(eu_entry.get("baseInvestmentPoints_month", 0.0) or 0.0)
    required_ip = advancement_cost(gdp_billions)
    current_months = required_ip / (0.10 * calculated_monthly_ip)
    saved_snapshot_months = (
        required_ip / (0.10 * saved_monthly_ip) if saved_monthly_ip > 0.0 else math.nan
    )
    old_stored = float(eu_entry.get("sustainability", 0.0) or 0.0)
    migrated_score = (
        max(0.0, min(10.0, 1.0 / old_stored - STORAGE_OFFSET))
        if old_stored > 0.0
        else 10.0
    )
    return {
        "save_name": path.name,
        "game_date": metadata.get("gameTimeString", "") or save_date_from_name(path.name),
        "gdp_trillions": gdp_billions / 1000.0,
        "population_millions": population_millions,
        "pcgdp": pcgdp,
        "saved_monthly_ip": saved_monthly_ip,
        "calculated_monthly_ip": calculated_monthly_ip,
        "required_ip_5_to_10": required_ip,
        "monthly_environment_ip_current_formula_at_10pct": calculated_monthly_ip * 0.10,
        "months_5_to_10_current_formula": current_months,
        "years_5_to_10_current_formula": current_months / 12.0,
        "monthly_environment_ip_saved_snapshot_at_10pct": saved_monthly_ip * 0.10,
        "months_5_to_10_saved_snapshot": saved_snapshot_months,
        "years_5_to_10_saved_snapshot": saved_snapshot_months / 12.0,
        "old_stored_sustainability": old_stored,
        "migrated_score": migrated_score,
    }


def save_date_from_name(name):
    match = re.search(r"_(\d{4}-\d{1,2}-\d{1,2})", name)
    return match.group(1) if match else ""


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--saves", type=pathlib.Path, required=True)
    parser.add_argument("--output", type=pathlib.Path, required=True)
    parser.add_argument("--names", nargs="*", help="Optional explicit save filenames")
    args = parser.parse_args()
    samples = []
    paths = [args.saves / name for name in args.names] if args.names else list(args.saves.glob("*.gz"))
    for path in sorted(paths, key=lambda value: value.stat().st_mtime):
        try:
            sample = read_sample(path)
        except (OSError, EOFError, json.JSONDecodeError):
            continue
        if sample:
            samples.append(sample)
    if not samples:
        raise RuntimeError("No European Union nation state was found in the supplied saves.")
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open("w", newline="", encoding="utf-8") as handle:
        writer = csv.DictWriter(handle, fieldnames=list(samples[0]))
        writer.writeheader()
        writer.writerows(samples)
    for sample in samples[-10:]:
        print(
            sample["save_name"], sample["game_date"],
            f"GDP ${sample['gdp_trillions']:.3f}T",
            f"PCGDP ${sample['pcgdp']:,.0f}",
            f"current formula {sample['months_5_to_10_current_formula']:.1f} months / "
            f"{sample['years_5_to_10_current_formula']:.2f} years",
            f"saved snapshot {sample['months_5_to_10_saved_snapshot']:.1f} months / "
            f"{sample['years_5_to_10_saved_snapshot']:.2f} years",
        )


if __name__ == "__main__":
    main()
