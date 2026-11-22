"""Summarize actual Unity frame intervals in a station battle replay."""
import argparse
import json
import math
import statistics
from pathlib import Path


def summarize(rows):
    if not rows:
        return None
    summary = {"frames": len(rows)}
    for key in ("wallMs", "shellMs", "jobCompleteMs", "shellQueries"):
        values = sorted(row[key] for row in rows)
        summary[key] = {
            "mean": round(statistics.mean(values), 5),
            "median": round(statistics.median(values), 5),
            "p95": round(values[math.ceil(.95 * len(values)) - 1], 5),
            "max": round(values[-1], 5),
        }
    return summary


def analyze(path):
    data = json.loads(path.read_text(encoding="utf-8-sig"))
    frames = data["frames"]
    launch = data["firstSalvo"]
    # Exclude the formation/initial loading period and the frame straddling launch.
    windows = {"approach": (-60, -1), "salvo": (0, 180), "settledSalvo": (60, 170)}
    output = {"save": data["save"], "speed": data["speed"], "firstSalvo": launch,
              "peakLiveMissiles": max(row["missiles"] for row in frames)}
    for name, (lo, hi) in windows.items():
        rows = [row for prior, row in zip(frames, frames[1:])
                if prior["combatSeconds"] >= launch + lo and row["combatSeconds"] <= launch + hi]
        output[name] = summarize(rows)
    peak = output["peakLiveMissiles"]
    output["atPeak"] = summarize([row for prior, row in zip(frames, frames[1:])
                                   if row["missiles"] == prior["missiles"] == peak])
    output["liveMissileBands"] = {
        f"{low}-{high}": summarize([row for prior, row in zip(frames, frames[1:])
                                     if low <= prior["missiles"] <= high and low <= row["missiles"] <= high])
        for low, high in ((1, 12), (13, 24), (25, 36), (37, 47), (48, 48))
    }
    return output


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("frames", type=Path, nargs="+")
    args = parser.parse_args()
    print(json.dumps({str(path): analyze(path) for path in args.frames}, indent=2))
