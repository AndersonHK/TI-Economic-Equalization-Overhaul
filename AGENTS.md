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

Never deploy while Terra Invicta is open. Do not ask the user to confirm that it
is closed: `tools\deploy.ps1` must assert this programmatically before validation
and again immediately before replacing any files. Treat a failed assertion as a
safe deployment blocker. If any command rebuilds the mod after the last
successful deployment, the task is not ready for handoff until that new build
has passed the normal `deploy.ps1` flow.

`rg` and `gh` are not installed in this environment.

When asked to commit, stage the whole workspace by default with
`git add -A -- '*'` from the repository root. Honor explicitly scoped
commits and keep parallel edits separate when there is a concrete reason.
Use stashes for deliberate temporary isolation, never as a default way
to exclude uncertain changes from a commit.

Test Terra Invicta changes headlessly. Use save-based replays and
automated validators for campaign cases. Leave interactive game
testing to the user.

# One implementation for each system

When EEO changes the rules of a system, own the complete implementation
for all consumers of those rules. Route every caller through the same
service, including player actions, AI, previews, estimates, validation,
execution, and saved-state recovery where applicable. Do not replace one
case and leave native code or another EEO implementation responsible
for the other cases of the same decision.

The shared service can use specialized algorithms, data-driven policies,
and optimized backends. It owns their selection, inputs, invariants,
and result validation. A quick query can stop early after it has a
result that passes the same checks as the full query. Compatibility
adapters and fallback backends must preserve the same rules. A feature
that is disabled in its entirety may leave the native system in charge.

Before changing a system, inventory its callers and check for paths
that bypass the shared implementation. Treat existing splits as
retroactive fixes. Record the owning entry points and how to reproduce
or test each split in the [implementation audit](docs/tooling/shared-implementation-audit.md).
Verify parity across consumers before switching the system over.

# Documentation artifacts

Place persistent documentation and research artifacts under `docs/` in the
appropriate topic directory. This includes generated CSV snapshots, workbooks,
reports, charts, and other files intended for later reference. Do not place
documentation under `outputs/`; reserve `outputs/` for disposable build or run
products. Temporary verification files belong under `.tmp/`.
