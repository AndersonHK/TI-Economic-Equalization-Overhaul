# Archived gunpowder rendering investigation

**Superseded by the user-accepted 0.10.8 implementation on 2026-10-21.**
Use the [current implementation](../../gunpowder-canonical-art-implementation-2026-10-21.md)
and [current evidence index](../../gunpowder-art/README.md). Pending-test statements
inside these files describe the state when each investigation was written.

| Record | Historical significance |
|---|---|
| [Implementation evolution](implementation-evolution.md) | Preserves the earlier mixed 0.10.5–0.10.8 narrative before replacement with a current authority |
| [Material investigation](material-inspection/README.md) | Source textures, UVs, renderer slots, material-cache defect and rejected hypotheses |
| [Background-cutout comparison design](material-inspection/background-cutout-0.10.8.md) | Reasoning for mesh-baked sizing and independent instancing comparison |
| [First validation failure](startup-and-deployment/first-verification-failure.txt) | Validator argument and missing matrix-row failures |
| [0.10.5 deployment](startup-and-deployment/deployment-0.10.5.txt) | Offline success before the private-JSON startup failure |
| [Startup failure screenshot](startup-and-deployment/startup-json-failure.png) | Native recursive JSON loader rejected the private object schema |
| [0.10.6 deployment](startup-and-deployment/deployment-0.10.6.txt) / [readback](startup-and-deployment/verification-0.10.6.json) | `.eeo` packaging correction; material problem remained |
| [0.10.7 deployment](material-inspection/deployment-0.10.7.txt) / [readback](material-inspection/deployment-verification-0.10.7.json) | Material restoration and probes; user still observed background cutouts |

All source texture atlases, offline renders, hull material inventories, runtime
traces and user screenshots are retained. The [complete catalog](../../../CATALOG.md)
indexes each file individually. No evidence was deleted or silently relabelled
as a successful test of an earlier build.
