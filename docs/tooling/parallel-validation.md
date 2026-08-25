# Parallel validation and deployment

## Execution model

`tools/verify.ps1` builds the mod once, then runs independent validators through
an eight-worker PowerShell runspace pool. Each worker launches its assigned
validator in a separate child process.

The two levels serve different safety purposes:

- The runspace pool provides a bounded thread scheduler instead of creating all
  validators at once.
- Child-process isolation prevents validators which load `Assembly-CSharp`,
  Harmony, Unity, or the mod DLL from sharing an AppDomain or patch state.

Validator output is buffered per task and printed in declaration order. All
workers are joined and any nonzero exit code fails verification with the names
of the failed tasks. This avoids interleaved logs and prevents one passing task
from hiding another task's failure.

## Serialization boundaries

These operations remain serialized:

1. Build the release DLL.
2. Build the formula-test executable.
3. Perform inline cross-file release assertions.
4. Stage and compress the release package.
5. Recheck that Terra Invicta is closed and mirror the package into the enabled
   mod directory.

Only read-only, independent validators are pooled. The starting-economic check
uses its own GUID-named temporary directory and cleans it after completion.

## Configuration

The default and minimum worker count is eight:

```powershell
tools\verify.ps1 -ValidationThreads 8
tools\deploy.ps1 -ValidationThreads 8
```

The value may be raised to 32 for machines with more suitable cores. Deployment
passes the value to verification; it never makes package or install mutations
concurrent.

## Initial result

On the development machine, the first complete eight-worker verification took
25.37 seconds. After warm filesystem caches, the final deployment verification
took 21.06 seconds. The immediately preceding sequential deployment
verifications took about 88 seconds each. All 34 pooled validators, the release
build, 1,172 formula assertions, packaging checks, and archive hashing remained
enabled.
