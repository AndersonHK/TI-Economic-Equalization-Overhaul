# Designer viewport offset and full-layout fit — 0.11.2

Status: **deployed in 0.11.2 on 2026-11-04; manual viewport acceptance pending.**

The reported Titan screenshot clips the upper utility row beneath the class
header while leaving substantial space below armor counters. The current row
adapter uses a fixed origin and requested panel height without measuring the
post-layout visible rectangle. Native canvas scaling uses a 1920-wide reference
and the configured UI scale; FleetsScreenController reserves 100 UI units at
the top (85 at its smaller vertical-scale branch). That screen budget must be
respected rather than assuming the requested window height is visible.

Use a JSON-defined content region, header keepouts, padding and screen margin.
After native layout/scaling, measure those rectangles in a common grid space.
Fit the maximum bounds of all 28 hull layouts, including actual armor offsets,
count inputs and labels. Move the complete grid origin, preserving all logical
slot/system anchors, bank counts, pitch and weapon/model mappings. Include the
active rendered rects as an additional runtime bounds check.

If the visible region is too short, enlarge the central region and matching
outer panel by the measured shortage. Fit the resulting outer window within
the actual root canvas below native top reservation, reducing its uniform
scale only when screen bounds require it. Recheck after layout settles. Avoid
per-frame work: schedule bounded layout passes on initialization, hull layout,
show and UI-scale changes, then disable the observer. Log actual content/view,
window, origin, scale and fit status for manual acceptance.

Test the pure placement/window-fit math without UI creation, including clipped
origin, full Mothership row 10, insufficient region and reserved top-screen
space. Validate all hull bounds through the installed assembly. Runtime Unity
rectangles, fonts and visible clipping still require a manual reload/designer
check. No save, ship statistics, logical anchors or tier data should change.

## Measured cause and preflight

The installed prefab has an enabled GridLayoutGroup on Ship Modules Layout,
with middle-center alignment, 72-unit cells, 2-unit horizontal spacing and
-35-unit vertical spacing (37-unit half-row pitch). Neither the outer panel nor
central panel has a native height-driving component in that prefab.

Native centering at seven rows in a 342-unit grid gives first-center y=60.
After eleven rows are allocated in the 416-unit grid, centering gives y=23:
exactly 37 units above the JSON origin. The adapter must disable this competing
layout group when owning coordinates, and ensure Show cannot restore it.

The full catalog envelope is 738 × 448.5 UI units including armor labels.
Native header/toolbar keepouts can leave a 418-unit content rectangle in the
nominal expanded panel. The pure preflight measures the 30.5-unit shortage,
fits the full layout after expansion, and checks a 100-unit upper notification
reservation plus a six-unit lower margin. At a 1080-unit reference canvas the
example outer-window fit uses scale 0.9696366; smaller reference canvases also
remain contained. These are native-prefab arithmetic cases, not observed live
window measurements. Runtime world-rectangle logs will establish actual fit.

[Screen-budget cases](designer-viewport-screen-budget-2026-11-04.json).

## Verification and runtime acceptance

The isolated, renamed UI fixture compiles against the installed game/Unity
assemblies without rebuilding the mod or creating Unity objects. Normal guarded
deployment then passed **44 validators / 1,391 formula assertions**, including
the centered-layout 37-unit regression, visible-region shortage, post-header
placement, five notification/screen budgets, all 28 hull extents and three UI
event patch targets. The **123-feature / 264-patch** matrix validates.
All **55 installed files and archive counterparts** match. See the
[deployment receipt](designer-viewport-deployment-2026-11-04.json).

This includes the entire Mothership row 10 and armor Max/count labels, not just
the empty Titan's visible weapon cells. The runtime observer measures world
rectangles after canvas updates, disables the competing GridLayoutGroup,
reapplies the common origin, and includes active images/spinner/label rectangles
in addition to the catalog envelope. It expands matching center/outer dimensions
only when the measured viewport is too small. Native upper reservation is
preserved; uniform outer-panel scaling ensures the resulting window stays in
the actual root canvas with configured margins.

The observer runs for at most three visible layout passes after initialization,
class-layout, show and UI-scale events, then disables itself. There is no normal
per-frame fit work after settling. A runtime log line reports `catalog`,
`visible`, `origin`, `panel`, `scale`, `all-content-inside`, and
`window-on-screen`. A warning indicates failed containment or unavailable room.

**Manual validation remains required.** Restart, open Titan then Mothership,
verify the first row clears the header and the last row/armor labels remain
inside the region, inspect preview/catalog boundaries, and try a UI-scale or
resolution change. Review the runtime containment log in Player.log. Numerical
and native-binding checks are not a claim that the live screenshot has already
been confirmed. No commit was created.

DLL SHA256: `FC5B2B7D7BD5AAE1BB42E1F3AA35B6AA6EAFD3F59D42E44419F723DAD8C7949D`.
