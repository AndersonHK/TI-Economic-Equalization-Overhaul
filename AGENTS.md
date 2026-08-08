# Repository workflow

Use `Plan -> Document -> Implement -> Build -> Deploy -> Test -> Document` for
every gameplay, data, or tooling change. In practice, run `tools\deploy.ps1` as
soon as implementation is ready: it performs the required build and automatic
validation before copying the package, so do not split those steps into separate
agent turns or delay deployment for secondary review and documentation. Do not
pass `-SkipVerification` unless the user explicitly requests it.

After deployment completes, immediately tell the user that the build is ready for
manual testing, then finish any remaining analysis and documentation while that
testing proceeds. Manual in-game testing is part of the critical iteration path
and frequently exposes behavior that static analysis cannot anticipate.

Never deploy while Terra Invicta is open or while the user is manually testing.
Wait until the user confirms the game is closed before replacing the DLL. If any
command rebuilds the mod after the last successful deployment, the task is not
ready for handoff until that new build has passed the normal `deploy.ps1` flow.

`rg` and `gh` are not installed in this environment.
