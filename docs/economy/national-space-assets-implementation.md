# National space asset upkeep and Boost cap — 0.9.8

## UI correction plan (2026-09-23)

Manual feedback identified a missing blank line before the GDP base/available
paragraph. Add a second line break after the introduction, then run the normal
verified deployment. This spacing-only follow-up retains version 0.9.8.
Completed and deployed on 2026-09-23: all 36 validators and 1,185 formula
assertions passed, with all 46 installed files matching the package. The added
blank line was visually accepted by the user on 2026-09-23 after manual testing.

Replace the appended IP diagnostic section with one native-style tooltip. Keep
the introduction and base/available IP narrative; render adviser, occupation,
Unrest, home/away army, navy, and MC/Boost/Funding upkeep as conditional prose
bullets using actual runtime deductions. Describe repair debt separately from
monthly upkeep. Remove the duplicate output calculation and legacy fixed army
rates. Show Boost as current (maximum), retain its trend arrow, and describe
that layout using the same prose as the MC tooltip while preserving the 31-day
change line. This is a UI-only correction to 0.9.8, with no formula/version change.
Build and deploy through the normal verified flow after implementation.

Implemented and deployed through `tools/deploy.ps1` on 2026-09-23. The IP tooltip
now replaces the legacy fixed-rate bullets with conditional native prose using
live army/navy factors and separate MC, Boost and Funding deductions. The Boost
resource row shows current (maximum), retaining its trend sprite; its tooltip
uses MC-style wording and retains the native 31-day change. No balance or version
changes were made for these UI corrections.

Both string tooltip patches passed Harmony emission validation. The display
refresh method cannot be detoured in desktop PowerShell because of Unity native
ECall restrictions, so its target signature, Harmony attributes and compiled IL
are checked instead, consistent with the repository's other Unity UI validators.
The user accepted the tooltip and Boost UI corrections after manual testing,
including the final paragraph-spacing fix, on 2026-09-23. This confirms the
reported UI issues are resolved; it does not establish coverage of every
gameplay acceptance target below.

## Approved plan (2026-09-23)

- Set investment outputMultiplier to 1.10 (not the snapshot-neutral 1.10954).
- Deduct monthly IP upkeep: 0.05 per installed national MC, 0.10 per raw
  monthly Boost production, and 0.001 per raw monthly national Funding.
- Apply deductions after the existing Unrest, occupation, adviser and military
  calculation; floor remaining IP at zero. Do not tax faction stockpiles or
  federation receipts. Recompute from the underlying stocks, never compound a
  previous deduction or persist new save fields.
- Cap national monthly Boost production at
  `2 * GDP_billions / max(200, 300 - 6 * Education)`. Preserve existing over-cap
  production. Gate the shared priority validity used by AI/UI and spending;
  constrain direct-investment quantity and positive facility increments,
  including initial spaceflight grants. Negative changes remain available.
- Show upkeep and Boost capacity in investment/priority tooltips.
- Bump release metadata, assembly and package validation from 0.9.7 to 0.9.8.

Implementation uses shared pure arithmetic plus narrowly scoped Harmony patches.
The upkeep postfix runs on the vanilla cache recomputation, so the deduction is
not compounded from the previous cached value. Both upkeep and the cap can be
disabled independently under investment settings; disabling investment also
disables them. Alien nations retain their existing behavior.

The direct-investment batch limit assumes the best eligible latitude gain,
accounts for accumulated progress, and preserves the game's annual allowance.
This conservative limit prevents buying many completions past the cap; slower
launch sites can purchase another batch. The final facility increment can be
partial. Negative changes and existing above-cap assets are never clamped away.

GDP/Education changes refresh priority eligibility when they cross capacity.
The investment tooltip shows the three asset deductions separately; the Boost
priority tooltip shows current raw monthly production and its economic cap.
The pre-existing army/navy tooltip was also corrected to count navy upkeep only
for naval-deployment armies, matching the actual IP calculation.

## Status

Implemented, built, validated, and deployed on 2026-09-23 as **0.9.8**.
The final normal `tools/deploy.ps1` flow passed 25 patch/documentation validators,
11 formula/data validators, and **1,185 formula assertions**. Its two game-process
assertions passed and all **46 deployed files** matched the source package.
The installed Settings.xml was independently checked for 1.10 output, k=2, and
0.05/0.10/0.001 upkeep. No rebuild followed the successful deployment.

Pure tests cover monthly/annual units, zero-IP flooring, the Education denominator
floor, additive capacity across borders, final partial increments, over-cap
preservation, damage, floating-point closure, and direct-investment bounds.
The installed-assembly validator checks approved defaults and Harmony emission
for the five gameplay patch classes (six target methods) and both tooltip patches,
plus the Boost display contract/IL described above. It does not instantiate live
nations: their static initialization requires scenario templates in the running
game. Save/load and UI/AI behavior remain manual in-game acceptance checks.

The implementation matrix was updated and visually checked, including both UI
correction entries. Existing validation rules were preserved. The full matrix
validator covers 102 features and 189 patches.

Final DLL SHA-256:
`3EC747C238FFC2CBEF544E6E140B5072A9FEE2CF10F1AAA92FA9C301FD4A6AD3`.
Package: `artifacts/TIEconomyMod-0.9.8-ti1.0.53.zip`.
Installed under `D:/Games/SteamLibrary/steamapps/common/Terra Invicta/Mods/Enabled/Economic Equalization Overhaul`.

On the previously analyzed 2039-06-16 snapshot, approved settings predict world
available IP falling from 2,071.914 to 2,051.363 per month, a **0.992% reduction**.
This is an offline estimate with unchanged assets, Unrest, and military state.

Broader gameplay test coverage remains unconfirmed. Targets: reload the June 2039 save;
inspect India/US IP deductions; confirm Singapore and New Zealand retain Boost
but cannot expand while above the k=2 cap; inspect a below-cap country's direct
investment and a spaceflight-program completion.
