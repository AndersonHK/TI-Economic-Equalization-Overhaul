[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$TargetManagedDir,
    [string]$WriteResolvedManagedDirPath
)

$ErrorActionPreference = 'Stop'
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = Split-Path -Parent $scriptDirectory

function Find-TerraInvictaManagedDirectory {
    param([string]$ExplicitPath)

    $candidates = New-Object System.Collections.Generic.List[string]
    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        $candidates.Add($ExplicitPath)
    }
    if (-not [string]::IsNullOrWhiteSpace($env:TI_TARGET_MANAGED_DIR)) {
        $candidates.Add($env:TI_TARGET_MANAGED_DIR)
    }

    $steamRoots = New-Object System.Collections.Generic.List[string]
    $registryLocations = @(
        'HKCU:\Software\Valve\Steam',
        'HKLM:\Software\WOW6432Node\Valve\Steam'
    )
    foreach ($location in $registryLocations) {
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
        if ([string]::IsNullOrWhiteSpace($steamRoot)) {
            continue
        }
        $libraryRoots = New-Object System.Collections.Generic.List[string]
        $libraryRoots.Add($steamRoot)
        $libraryFile = Join-Path $steamRoot 'steamapps\libraryfolders.vdf'
        if (Test-Path -LiteralPath $libraryFile) {
            $libraryText = Get-Content -LiteralPath $libraryFile -Raw
            foreach ($match in [regex]::Matches($libraryText, '"path"\s+"(?<path>[^"]+)"')) {
                $libraryRoots.Add($match.Groups['path'].Value.Replace('\\', '\'))
            }
        }

        foreach ($libraryRoot in @($libraryRoots | Select-Object -Unique)) {
            $candidates.Add((Join-Path $libraryRoot 'steamapps\common\Terra Invicta\TerraInvicta_Data\Managed'))
        }
    }

    foreach ($candidate in @($candidates | Select-Object -Unique)) {
        $assembly = Join-Path $candidate 'Assembly-CSharp.dll'
        $umm = Join-Path $candidate 'UnityModManager\UnityModManager.dll'
        $harmony = Join-Path $candidate 'UnityModManager\0Harmony.dll'
        if ((Test-Path -LiteralPath $assembly) -and
            (Test-Path -LiteralPath $umm) -and
            (Test-Path -LiteralPath $harmony)) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw 'Could not locate TerraInvicta_Data\Managed with a matched Unity Mod Manager and Harmony pair. Set TI_TARGET_MANAGED_DIR.'
}

function Find-MSBuild {
    $vswhere = if (${env:ProgramFiles(x86)}) {
        Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    }
    else {
        $null
    }
    if ($vswhere -and (Test-Path -LiteralPath $vswhere)) {
        $installation = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
        if ($installation) {
            $candidate = Join-Path $installation 'MSBuild\Current\Bin\MSBuild.exe'
            if (Test-Path -LiteralPath $candidate) {
                return $candidate
            }
        }
    }

    $command = Get-Command msbuild.exe -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }
    throw 'MSBuild was not found. Install Visual Studio Build Tools with the .NET Framework 4.8 targeting pack.'
}

$managedDirectory = Find-TerraInvictaManagedDirectory -ExplicitPath $TargetManagedDir
if (-not [string]::IsNullOrWhiteSpace($WriteResolvedManagedDirPath)) {
    Set-Content -LiteralPath $WriteResolvedManagedDirPath -Value $managedDirectory -NoNewline
}
$msbuild = Find-MSBuild
$project = Join-Path $repositoryRoot 'TIEconomyMod\TIEconomyMod.csproj'

Write-Host "Target assemblies: $managedDirectory"
$arguments = @(
    $project,
    '/t:Rebuild',
    "/p:Configuration=$Configuration",
    "/p:TargetManagedDir=$managedDirectory",
    '/v:minimal',
    '/nologo'
)
& $msbuild @arguments
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$output = if ($Configuration -eq 'Release') {
    Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\Assembly\TIEconomyMod.dll'
}
else {
    Join-Path $repositoryRoot 'TIEconomyMod\bin\Debug\TIEconomyMod.dll'
}
if (-not (Test-Path -LiteralPath $output)) {
    throw "Build completed without expected output '$output'."
}
Write-Host "Built: $output"
