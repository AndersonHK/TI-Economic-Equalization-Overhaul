# Alien combat acceleration and cruise limits

2026-10-31. **0.10.21 built and deployed for testing:** alien combat remains **7 G** and the user
requested alien cruise **3.5 G**. The gameplay overrides are
`maxAlienCombatAcceleration_g = 7` and `maxAlienCruiseAcceleration_g = 3.5`
in `TIGlobalConfig.globalConfig`.
The user explicitly selected 7 G after learning the full human limit is 6.5 G.
Version 0.10.20 introduced the combat field; 0.10.21 adds only the cruise field.

## Actual limits in the installed game and this mod

| Crew acceleration tolerance | Human baseline | Human with all relevant research | Human with research and best officer | Alien baseline before | Alien baseline after |
|---|---:|---:|---:|---:|---:|
| Combat | 3 G | 5 G | 6.5 G | 4 G | **7 G** |
| Cruise | 2 G | 3 G | 3 G | 2.5 G | **3.5 G** |

These are tolerance caps, not guaranteed thrust/mass performance for every
ship design. Native faction/officer modifiers are added to the species baseline
when present. The alien columns describe its starting baseline, not an assertion
that no future/native modifier could ever add to it.

## Human combat progression

The following nonrepeatable projects each grant the stackable additive
`Effect_IncreaseMaxSurvivableCombatAcceleration` worth **+0.5 G**:

| Project | Combat bonus | Cruise bonus |
|---|---:|---:|
| High-Thrust Ergonomics | +0.5 G | none |
| Astronaut Fitness Regimen | +0.5 G | none |
| Acceleration Pharmaceuticals | +0.5 G | none |
| High-G Recombinants | +0.5 G | +1 G |

Thus research can raise human combat tolerance from 3 to 5 G. A Medical Officer
adds +0.5/+1/+1.5 G at ranks 1/2/3. Native code selects effects at the **exact
rank**, not all lower ranks cumulatively. The template permits one Medical
Officer per ship and requires at least 40 crew. The maximum with a rank-3
Medical Officer is therefore **3 + 4*0.5 + 1.5 = 6.5 G**.
Template/designer calculations include research; the ship-state getter also
adds its actual officers. This explains why an individual ship's cap can exceed
the research-only figure. Availability/unlock chances still govern acquiring
projects; these figures describe what is attainable, not a guaranteed unlock.

## Cruise limits

Human cruise tolerance starts at **2 G**. High-G Recombinants grants the
permanent additive `Effect_IncreaseMaxSurvivableCruiseAcceleration` worth
**+1 G**, raising it to **3 G**. The other three projects above add no cruise G.
No stock officer template grants `MaxSurvivableCruiseAcceleration`, even though
the native ship getter supports the effect type. A top Medical Officer therefore
does not raise cruise tolerance beyond 3 G.

The native alien starting cruise limit is **2.5 G**. The user subsequently
selected **3.5 G**, giving aliens a **0.5 G** baseline advantage over fully
researched humans. Human limits, projects, officers and drives remain unchanged. The checked
Dark Skies content supplies scenario folders/assets rather than another general
acceleration project/officer override.

## Source trace and selected change

The installed 1.0.53b Assembly-CSharp.dll remains SHA256
`4A4B9AAE4154E444E9727204205D2D42AE8ED9E1C5F92CDC1280074A259D8350`.
Native `TIGlobalConfig` field initializers define human combat/cruise at 3/2 G
and alien combat/cruise at 4/2.5 G. Those defaults are not explicitly listed in
the stock partial global JSON. A public-field JSON override is sufficient;
no new C# gameplay patch is needed.

`TISpaceShipState.maxCombatAccleration_g` (native spelling) and
`maxCruiseAcceleration_g` choose the species baseline, add faction effects, then
add ship officer effects. Native templates cap acceleration by thrust/wet mass
and the corresponding tolerance times 9.80665 m/s² per G. Research and officer
data and source hashes are retained in the
[compact audit](acceleration-limits/native-acceleration-audit-2026-10-31.json).

The user accepted **7 G**, superseding the initial strict human-maximum-plus-1
proposal. The new alien baseline is +3 G over native aliens, +2 G over
research-only humans and +0.5 G over the best-officer human cap.

The same native global field also feeds the alien angular-velocity ceiling.
That derived ceiling increases by sqrt(7/4), approximately **32.3%**. This is an
existing consumer of the one chosen parameter, not a second data override.

## Build, deployment and remaining verification

Version 0.10.20 added only the combat JSON field; 0.10.21 adds only the
cruise field relative to that build. No gameplay C# logic changes are needed.
Assembly/release metadata increments to 0.10.21. The strict global-data whitelist
was updated for this authorized field after initially rejecting its addition;
the complete deployment flow was rerun successfully.

Normal tools/deploy.ps1 passed **41 validators**, **1,219 formula assertions**,
**384 aperture comparisons**, **1,753 contact checks** and **72 laser UV/reuse
checks**, then verified **52 installed files**. The implementation matrix now
has 120 features / 253 Harmony classes. JSON guards require 7 G combat / 3.5 G cruise and reject
unrequested human-limit overrides.

[Deployment log](acceleration-limits/deployment-0.10.21.txt).
[Installed readback](acceleration-limits/deployment-manifest-0.10.21.json).
DLL SHA256: `19B1F3FA7894ACA852DFDBDD99C946EFD675AE6A661CCA0EA10221F2AA5EA375`.
The archive retains the twelve documented older omissions; the installed package
is complete. Commit/push was subsequently requested with stable-baseline
acceptance on 2026-11-02, recorded below.

Restart the game to load the templates. Check the alien combat cap in a fresh
skirmish/design and actual performance on a thrust-capable ship. Confirm human
and cruise values remain as above; actual ship acceleration may be lower because
of thrust/mass or operating state. Live existing-save cache refresh and in-game
acceptance have not been exercised by the agent.

## Stable baseline acceptance: 2026-11-02

The user confirmed "Commit and push, we are stable." This accepts the deployed
0.10.21 baseline, including the 7 G combat / 3.5 G cruise alien caps and inherited
combat geometry, camera, collision and laser changes. Human limits remain
unchanged. The historical deployment manifests keep their original
pending-acceptance fields; this dated acceptance supersedes that status.

This closure updates documentation without rebuilding or replacing the accepted
runtime. Source and installed package hashes are checked against the retained
52-file deployment manifest before committing. No new agent in-game tests or
exhaustive 15x scenario coverage are implied; existing technical limitations
remain documented.

## Cruise follow-up scope

The requested cruise increase is 2.5→3.5 G (+1 G / +40% to the native cap).
Combat remains 7 G, so both alien starting caps are now 0.5 G above the
corresponding fully upgraded human maxima (combat 6.5 G, cruise 3 G). This adds
no new human changes and does not increase the already-configured rotation
ceiling further. Actual ship performance is still thrust/mass limited.
