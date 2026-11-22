[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$GameInstallDir,
    [Parameter(Mandatory=$true)][string]$Save,
    [Parameter(Mandatory=$true)][string]$OutputDirectory,
    [ValidateSet('1','3','5','10','15')][string]$Speed = '10',
    [switch]$VerifyShell,
    [switch]$ThroughSalvo
)
$ErrorActionPreference = 'Stop'
if (Get-Process TerraInvicta -ErrorAction SilentlyContinue) { throw 'Close the existing Terra Invicta process before replaying.' }
$source = (Resolve-Path -LiteralPath $Save).Path
$directory = [IO.Directory]::CreateDirectory([IO.Path]::GetFullPath($OutputDirectory)).FullName
$copy = Join-Path $directory ([IO.Path]::GetFileName($source))
$report = Join-Path $directory 'frames.json'
$log = Join-Path $directory 'player.log'
if ((Test-Path $report) -or (Test-Path $log) -or (Test-Path $copy)) { throw 'Choose a fresh output directory for each run.' }
Copy-Item -LiteralPath $source -Destination $copy
$arguments = @('-batchmode','-noaudio','-force-d3d11','-logFile',('"' + $log + '"'),
    '--eeo-station-replay',('"' + $copy + '"'),('"' + $report + '"'),$Speed)
if ($VerifyShell) { $arguments += '--eeo-verify-shell' }
if ($ThroughSalvo) { $arguments += '--eeo-through-salvo' }
$process = Start-Process -FilePath (Join-Path $GameInstallDir 'TerraInvicta.exe') -WorkingDirectory $GameInstallDir -ArgumentList $arguments -WindowStyle Hidden -PassThru
$timeoutMs = [int](1000 * (30 + [Math]::Max(420, 150 + 1000 / [int]$Speed)))
if (!$process.WaitForExit($timeoutMs)) { $process.Kill(); $process.WaitForExit(); throw 'Replay timed out.' }
if ($process.ExitCode -ne 0 -or !(Test-Path -LiteralPath $report)) { throw "Replay failed. See $log." }
Write-Host "Recorded $report"
