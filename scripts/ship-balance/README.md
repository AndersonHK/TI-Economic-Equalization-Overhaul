# Ship asset measurement scripts

`measure_ship_prefabs.py` is the maintained Unity-asset measurement tool used
by the human hull and drive-scaling research. It preserves the original hull
measurement path and keeps drive measurements in separate functions.

## Requirements

- Python 3
- NumPy
- UnityPy
- A local Terra Invicta installation containing the `ships` asset bundle

The script first imports UnityPy from the active Python environment. For the
repository research setup it also recognizes an unpacked local dependency at
`.tmp/unitypy`; that directory remains generated/untracked. Any unpacked native
extensions must match the Python version used to run the script (the documented
local snapshot is a Python 3.12 build).

The default bundle path matches the installation used for the documented
measurements. Change `BUNDLE` locally when Terra Invicta is installed elsewhere.

## Output

Running the script writes one JSON document to standard output. Each measured
human or alien hull contains:

- visual hull and raycast-collider envelopes;
- the combined hull-plus-default-drive envelope;
- default embedded-drive bounds;
- standalone default and alternate drive-resource bounds; and
- separate connected-component estimates for individual six-engine nozzles.

Human drive resources are grouped into the installed default and alternate
appearances and De Laval, magnetic, and pulse families. Alien hull templates
have one installed graphical appearance and one hull-specific alien thruster
family, so their output records appearance index 0 and measures the x1 and x6
resources. The Salamander Gunship has no standalone drive resource in the
installed `ships` bundle and is reported as unavailable rather than assigned a
manufactured measurement.

Example:

```powershell
python scripts/ship-balance/measure_ship_prefabs.py > ship-measurements.json
```

The JSON is measurement evidence, not a gameplay override. Axis-aligned bounds
and transverse bounding areas must not be described as occupied volume or true
nozzle exit area.
