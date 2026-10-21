# Gunpowder art manual acceptance

**User-confirmed working on 2026-10-21, release 0.10.8.** The user reported:
“The fix worked,” and requested documentation organization, a detailed commit
and push. This supersedes earlier pending-test statements and the failed
0.10.7 material-only attempt.

The retained [runtime trace](accepted-runtime-trace-2026-10-21.txt) records the
40 mm profile in combat with `baked=True nonInstanced=False`. No render-comparison
toggle change is present in the captured trace. This supports accepting the
default mesh-baked path without requiring the optional non-instancing override.
It does not prove which GPU/shader operation made the prior transform path fail.

Accepted scope is the existing gunpowder art pass and the reported invisible
turret case. Earlier screenshots established acceptable dimensions. Automated
checks additionally verify unchanged gameplay data, tiers, canonical targets,
controller lifecycle math and source compatibility. No claim is made that the
user exhaustively tested all eight calibers on every art variant, all damage/
repair sequences or other graphics backends.

The deployed DLL remains
`483727F3FB55435FCBCAED420128176AADC41946C7AA5E311455AD614D377A87`.
This documentation cleanup does not rebuild or change the deployed game package.
Future laser work, including compact alien optics, remains a separate phase.
