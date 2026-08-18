[CmdletBinding()]
param(
    [string]$GameInstallDir,
    [string]$TargetManagedDir,
    [switch]$SkipVerification
)

$ErrorActionPreference = 'Stop'
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = Split-Path -Parent $scriptDirectory
$relativeDestination = 'Mods\Enabled\Economic Equalization Overhaul'

function Add-SteamGameCandidates {
    param(
        [System.Collections.Generic.List[string]]$Candidates,
        [string]$SteamRoot
    )

    if ([string]::IsNullOrWhiteSpace($SteamRoot)) {
        return
    }
    $libraryRoots = New-Object System.Collections.Generic.List[string]
    $libraryRoots.Add($SteamRoot)
    $libraryFile = Join-Path $SteamRoot 'steamapps\libraryfolders.vdf'
    if (Test-Path -LiteralPath $libraryFile) {
        $libraryText = Get-Content -LiteralPath $libraryFile -Raw
        foreach ($match in [regex]::Matches($libraryText, '"path"\s+"(?<path>[^"]+)"')) {
            $libraryRoots.Add($match.Groups['path'].Value.Replace('\\', '\'))
        }
    }
    foreach ($libraryRoot in @($libraryRoots | Select-Object -Unique)) {
        $Candidates.Add((Join-Path $libraryRoot 'steamapps\common\Terra Invicta'))
    }
}

function Find-TerraInvictaInstall {
    param([string]$ExplicitGameDir, [string]$ExplicitManagedDir)

    $candidates = New-Object System.Collections.Generic.List[string]
    if (-not [string]::IsNullOrWhiteSpace($ExplicitGameDir)) {
        $candidates.Add($ExplicitGameDir)
    }
    if (-not [string]::IsNullOrWhiteSpace($env:TI_GAME_INSTALL_DIR)) {
        $candidates.Add($env:TI_GAME_INSTALL_DIR)
    }

    $managedCandidates = @($ExplicitManagedDir, $env:TI_TARGET_MANAGED_DIR) |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    foreach ($managed in $managedCandidates) {
        $managedFull = [IO.Path]::GetFullPath($managed)
        if ((Split-Path -Leaf $managedFull) -eq 'Managed' -and
            (Split-Path -Leaf (Split-Path -Parent $managedFull)) -eq 'TerraInvicta_Data') {
            $candidates.Add((Split-Path -Parent (Split-Path -Parent $managedFull)))
        }
    }

    $steamRoots = New-Object System.Collections.Generic.List[string]
    foreach ($location in @('HKCU:\Software\Valve\Steam', 'HKLM:\Software\WOW6432Node\Valve\Steam')) {
        if (Test-Path -LiteralPath $location) {
            $properties = Get-ItemProperty -LiteralPath $location
            foreach ($property in @('SteamPath', 'InstallPath')) {
                if ($properties.$property) {
                    $steamRoots.Add([string]$properties.$property)
                }
            }
        }
    }
    if (${env:ProgramFiles(x86)}) {
        $steamRoots.Add((Join-Path ${env:ProgramFiles(x86)} 'Steam'))
    }
    foreach ($steamRoot in @($steamRoots | Select-Object -Unique)) {
        Add-SteamGameCandidates -Candidates $candidates -SteamRoot $steamRoot
    }

    foreach ($candidate in @($candidates | Select-Object -Unique)) {
        $assembly = Join-Path $candidate 'TerraInvicta_Data\Managed\Assembly-CSharp.dll'
        if (Test-Path -LiteralPath $assembly) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }
    throw 'Could not locate the Terra Invicta install root. Set TI_GAME_INSTALL_DIR, TI_TARGET_MANAGED_DIR, or pass -GameInstallDir.'
}

function Assert-TerraInvictaClosed {
    $runningProcesses = @(Get-Process -Name 'TerraInvicta' -ErrorAction SilentlyContinue)
    if ($runningProcesses.Count -gt 0) {
        $processIds = ($runningProcesses | Select-Object -ExpandProperty Id) -join ', '
        throw "Refusing to deploy while Terra Invicta is running (PID: $processIds). Close the game and rerun tools\deploy.ps1."
    }
}

$gameRoot = Find-TerraInvictaInstall -ExplicitGameDir $GameInstallDir -ExplicitManagedDir $TargetManagedDir
$managedDirectory = Join-Path $gameRoot 'TerraInvicta_Data\Managed'
Assert-TerraInvictaClosed
if (-not $SkipVerification) {
    & (Join-Path $scriptDirectory 'verify.ps1') -TargetManagedDir $managedDirectory
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

$source = (Resolve-Path -LiteralPath (Join-Path $repositoryRoot 'TIEconomyMod\ModFiles')).Path.TrimEnd('\')
$destination = [IO.Path]::GetFullPath((Join-Path $gameRoot $relativeDestination)).TrimEnd('\')
$resolvedGameRoot = (Resolve-Path -LiteralPath $gameRoot).Path.TrimEnd('\')
$requiredPrefix = $resolvedGameRoot + [IO.Path]::DirectorySeparatorChar
if (-not $destination.StartsWith($requiredPrefix, [StringComparison]::OrdinalIgnoreCase) -or
    (Split-Path -Leaf $destination) -ne 'Economic Equalization Overhaul' -or
    (Split-Path -Leaf (Split-Path -Parent $destination)) -ne 'Enabled') {
    throw "Refusing to deploy outside '$relativeDestination' under the detected game install."
}

# Verification rebuilds the package and may take long enough for the game to be
# launched afterward. Recheck at the final mutation boundary before touching the
# enabled mod directory.
Assert-TerraInvictaClosed

$enabledDirectory = Split-Path -Parent $destination
if (-not (Test-Path -LiteralPath $enabledDirectory)) {
    New-Item -ItemType Directory -Path $enabledDirectory -Force | Out-Null
}
if (-not (Test-Path -LiteralPath $destination)) {
    New-Item -ItemType Directory -Path $destination | Out-Null
}

$sourceFiles = @(Get-ChildItem -LiteralPath $source -Recurse -File)
$sourceRelative = @{}
foreach ($file in $sourceFiles) {
    $relative = $file.FullName.Substring($source.Length + 1)
    $sourceRelative[$relative] = $file.FullName
}

# Mirror the authored package so removed templates cannot linger in the enabled mod.
foreach ($file in @(Get-ChildItem -LiteralPath $destination -Recurse -File)) {
    $relative = $file.FullName.Substring($destination.Length + 1)
    if (-not $sourceRelative.ContainsKey($relative)) {
        $resolvedFile = [IO.Path]::GetFullPath($file.FullName)
        if (-not $resolvedFile.StartsWith($destination + '\', [StringComparison]::OrdinalIgnoreCase)) {
            throw "Refusing to remove stale file outside the deployment directory: $resolvedFile"
        }
        Remove-Item -LiteralPath $resolvedFile
    }
}
foreach ($directory in @(Get-ChildItem -LiteralPath $destination -Recurse -Directory |
        Sort-Object { $_.FullName.Length } -Descending)) {
    if (@(Get-ChildItem -LiteralPath $directory.FullName -Force).Count -eq 0) {
        Remove-Item -LiteralPath $directory.FullName
    }
}

foreach ($entry in $sourceRelative.GetEnumerator()) {
    $targetFile = Join-Path $destination $entry.Key
    $targetDirectory = Split-Path -Parent $targetFile
    if (-not (Test-Path -LiteralPath $targetDirectory)) {
        New-Item -ItemType Directory -Path $targetDirectory -Force | Out-Null
    }
    if ((Test-Path -LiteralPath $targetFile) -and
        (Get-FileHash -LiteralPath $entry.Value -Algorithm SHA256).Hash -eq
            (Get-FileHash -LiteralPath $targetFile -Algorithm SHA256).Hash) {
        continue
    }
    Copy-Item -LiteralPath $entry.Value -Destination $targetFile -Force
}

$deployedFiles = @(Get-ChildItem -LiteralPath $destination -Recurse -File)
if ($deployedFiles.Count -ne $sourceFiles.Count) {
    throw "Deployment contains $($deployedFiles.Count) files; source contains $($sourceFiles.Count)."
}
foreach ($entry in $sourceRelative.GetEnumerator()) {
    $targetFile = Join-Path $destination $entry.Key
    if (-not (Test-Path -LiteralPath $targetFile) -or
        (Get-FileHash -LiteralPath $entry.Value -Algorithm SHA256).Hash -ne
            (Get-FileHash -LiteralPath $targetFile -Algorithm SHA256).Hash) {
        throw "Deployed file does not match source: $($entry.Key)"
    }
}

Write-Host "PASS: deployed $($sourceFiles.Count) files."
Write-Host "Game install: $resolvedGameRoot"
Write-Host "Destination (game-relative): $relativeDestination"
Write-Host "Destination: $destination"
