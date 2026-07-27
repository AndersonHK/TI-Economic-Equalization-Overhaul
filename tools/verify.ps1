[CmdletBinding()]
param(
    [string]$TargetManagedDir
)

$ErrorActionPreference = 'Stop'
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = Split-Path -Parent $scriptDirectory
$buildStarted = Get-Date
$managedPathFile = Join-Path ([IO.Path]::GetTempPath()) (
    'ti-eeo-managed-' + [Guid]::NewGuid().ToString('N') + '.txt')

try {
    & (Join-Path $scriptDirectory 'build.ps1') -Configuration Release `
        -TargetManagedDir $TargetManagedDir `
        -WriteResolvedManagedDirPath $managedPathFile
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
    $resolvedManagedDir = Get-Content -LiteralPath $managedPathFile -Raw
}
finally {
    if (Test-Path -LiteralPath $managedPathFile) {
        Remove-Item -LiteralPath $managedPathFile
    }
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-target-il.ps1') `
    -TargetManagedDir $resolvedManagedDir
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$assemblyPath = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\Assembly\TIEconomyMod.dll'
powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-hab-connector-transpiler.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-hab-list-icon-transpiler.ps1') `
    -TargetManagedDir $resolvedManagedDir `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-hab-cost-rewrite.ps1') `
    -ModAssemblyPath $assemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $scriptDirectory 'validate-implementation-matrix.ps1') -RepositoryRoot $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$installation = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
$msbuild = Join-Path $installation 'MSBuild\Current\Bin\MSBuild.exe'
$testProject = Join-Path $repositoryRoot 'tests\FormulaTests\FormulaTests.csproj'
& $msbuild $testProject '/t:Rebuild' '/p:Configuration=Release' '/v:minimal' '/nologo'
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$weights = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\Config\economy-tech-weights.csv'
$defaultSettings = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\Settings.xml'
$missionOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIMissionTemplate.json'
$startOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIStartTimeTemplate.json'
$globalOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIGlobalConfig.json'
$habModuleOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIHabModuleTemplate.json'
$habOverrides = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\TIHabTemplate.json'
$nationLocalization = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\UINation.en'
$testExecutable = Join-Path $repositoryRoot 'tests\FormulaTests\bin\Release\TIEconomyMod.FormulaTests.exe'
& $testExecutable $weights
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$templatesDirectory = Join-Path (Split-Path -Parent $resolvedManagedDir) 'StreamingAssets\Templates'
$technologyTemplates = Join-Path $templatesDirectory 'TITechTemplate.json'
$installedTechnologyIds = @(
    Get-Content -LiteralPath $technologyTemplates -Raw |
        ConvertFrom-Json |
        ForEach-Object { $_.dataName }
)
$weightRows = @(Import-Csv -LiteralPath $weights)
if ($weightRows.Count -ne 149 -or $installedTechnologyIds.Count -ne 149) {
    throw "Technology catalog coverage is $($weightRows.Count)/$($installedTechnologyIds.Count), expected 149/149."
}
$weightHeaders = @($weightRows[0].PSObject.Properties.Name)
$expectedWeightHeaders = @(
    'tech_id',
    'enabled',
    'productivity_percent',
    'labor_substitution',
    'resource_substitution',
    'rationale'
)
if (($weightHeaders -join ';') -ne ($expectedWeightHeaders -join ';')) {
    throw "Technology CSV has unexpected columns: $($weightHeaders -join ', ')."
}
$duplicateTechnologyIds = $weightRows | Group-Object tech_id | Where-Object Count -gt 1
if ($duplicateTechnologyIds) {
    throw "Technology CSV contains duplicate IDs: $($duplicateTechnologyIds.Name -join ', ')."
}
$missingTechnologyIds = @($installedTechnologyIds | Where-Object { $_ -notin $weightRows.tech_id })
$unknownTechnologyIds = @($weightRows.tech_id | Where-Object { $_ -notin $installedTechnologyIds })
if ($missingTechnologyIds.Count -gt 0 -or $unknownTechnologyIds.Count -gt 0) {
    throw "Technology CSV mismatch. Missing: $($missingTechnologyIds -join ', '); unknown: $($unknownTechnologyIds -join ', ')."
}
$technologyProduct = 1.0
$futureLaborTotal = 0.0
$futureResourceTotal = 0.0
foreach ($row in $weightRows) {
    $productivity = [double]::Parse(
        $row.productivity_percent,
        [Globalization.CultureInfo]::InvariantCulture)
    $labor = [double]::Parse(
        $row.labor_substitution,
        [Globalization.CultureInfo]::InvariantCulture)
    $resources = [double]::Parse(
        $row.resource_substitution,
        [Globalization.CultureInfo]::InvariantCulture)
    if ($row.enabled -ne 'true' -or $productivity -le 0 -or
        $labor -le 0 -or $resources -le 0) {
        throw "Technology '$($row.tech_id)' is not enabled with three positive weights."
    }
    $technologyProduct *= 1.0 + $productivity / 100.0
    if ($row.tech_id -notin @('MissionToSpace', 'AdvancedChemicalRocketry')) {
        $futureLaborTotal += $labor
        $futureResourceTotal += $resources
    }
}
if ([Math]::Abs($technologyProduct - 3.40) -gt 0.00001) {
    throw "Full technology tree compounds to $technologyProduct instead of 3.40."
}
if ($futureLaborTotal -le 0 -or $futureResourceTotal -le 0) {
    throw 'Technology CSV future substitution totals must both be positive.'
}
$startingRows = @($weightRows | Where-Object {
    $_.tech_id -in @('MissionToSpace', 'AdvancedChemicalRocketry')
})
$startingProduct = 1.0
foreach ($row in $startingRows) {
    $startingProduct *= 1.0 + [double]::Parse(
        $row.productivity_percent,
        [Globalization.CultureInfo]::InvariantCulture) / 100.0
}
if ($startingRows.Count -ne 2 -or [Math]::Abs($startingProduct - 1.0201) -gt 0.0000001) {
    throw "Starting technology product is $startingProduct instead of 1.0201."
}

[xml]$settingsXml = Get-Content -LiteralPath $defaultSettings -Raw
$mainSource = Get-Content -LiteralPath (Join-Path $repositoryRoot 'TIEconomyMod\Main.cs') -Raw
$groupMatches = [regex]::Matches(
    $mainSource,
    'public\s+(?<type>\w+Settings)\s+(?<name>\w+)\s*=\s*new\s+\w+Settings\(\);')
foreach ($groupMatch in $groupMatches) {
    $groupType = $groupMatch.Groups['type'].Value
    $groupName = $groupMatch.Groups['name'].Value
    $classMatch = [regex]::Match(
        $mainSource,
        "public sealed class $groupType\s*\{(?<body>[\s\S]*?)\r?\n    \}")
    if (-not $classMatch.Success) {
        throw "Could not inspect defaults for $groupType."
    }
    $fieldMatches = [regex]::Matches(
        $classMatch.Groups['body'].Value,
        'public\s+(?<type>bool|float)\s+(?<name>\w+)\s*=\s*(?<value>[^;]+);')
    $groupNode = $settingsXml.Settings.$groupName
    if ($null -eq $groupNode) {
        throw "Default Settings.xml is missing group '$groupName'."
    }
    foreach ($fieldMatch in $fieldMatches) {
        $fieldName = $fieldMatch.Groups['name'].Value
        $fieldType = $fieldMatch.Groups['type'].Value
        $sourceValue = $fieldMatch.Groups['value'].Value.Trim().TrimEnd('f')
        $xmlNode = $groupNode.$fieldName
        if ($null -eq $xmlNode) {
            throw "Default Settings.xml is missing '$groupName.$fieldName'."
        }
        if ($fieldType -eq 'bool') {
            if ([bool]::Parse($sourceValue) -ne [bool]::Parse([string]$xmlNode)) {
                throw "Default Settings.xml does not match '$groupName.$fieldName'."
            }
        }
        else {
            $sourceNumber = [double]::Parse(
                $sourceValue,
                [Globalization.CultureInfo]::InvariantCulture)
            $xmlNumber = [double]::Parse(
                [string]$xmlNode,
                [Globalization.CultureInfo]::InvariantCulture)
            if ([Math]::Abs($sourceNumber - $xmlNumber) -gt 0.000000001) {
                throw "Default Settings.xml does not match '$groupName.$fieldName'."
            }
        }
    }
}
if (-not [bool]::Parse([string]$settingsXml.Settings.enabled)) {
    throw 'Default Settings.xml must enable the global mod toggle.'
}

& node (Join-Path $repositoryRoot 'tools\economy-growth-simulator.js') | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw 'Economy growth simulator failed.'
}

powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $scriptDirectory 'validate-hab-rebalance.ps1') `
    -VanillaTemplatesDir $templatesDirectory `
    -RepositoryRoot $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$manifestPath = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\ModInfo.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.GameVersion -ne '1.0.49') {
    throw "ModInfo.json targets '$($manifest.GameVersion)' instead of TI 1.0.49."
}
if ($manifest.Version -ne '0.7.0') {
    throw "ModInfo.json version '$($manifest.Version)' does not match this release."
}
if ($manifest.AssemblyName -ne 'Assembly/TIEconomyMod.dll') {
    throw "ModInfo.json has unexpected AssemblyName '$($manifest.AssemblyName)'."
}

$missions = Get-Content -LiteralPath $missionOverrides -Raw | ConvertFrom-Json
$enthrall = $null
$purge = $null
foreach ($mission in $missions) {
    if ($mission.dataName -eq 'EnthrallElites') {
        $enthrall = $mission
    }
    elseif ($mission.dataName -eq 'Purge') {
        $purge = $mission
    }
}
if ($enthrall.resolutionMethod.defendingModifiers[0].flatModifier -ne 3 -or
    $purge.resolutionMethod.defendingModifiers[0].flatModifier -ne 4) {
    throw 'Mission overrides must add one flat defense to Enthrall Elites and Purge.'
}
$starts = Get-Content -LiteralPath $startOverrides -Raw | ConvertFrom-Json
$modernStart = $null
foreach ($start in $starts) {
    if ($start.dataName -eq 'ModernDayStart') {
        $modernStart = $start
    }
}
if (($modernStart.startingTechs -join ';') -ne 'Skywatch;WeAreNotAlone;OutpostHabs' -or
    ($modernStart.globalTechsCompleted -join ';') -ne
        'MissionToSpace;AdvancedChemicalRocketry') {
    throw 'The 2022 start override has unexpected current or completed technologies.'
}

$assemblyFile = Get-Item -LiteralPath $assemblyPath
if ($assemblyFile.LastWriteTime -lt $buildStarted.AddSeconds(-2)) {
    throw 'Packaged DLL predates this verification build.'
}
$assemblyHash = (Get-FileHash -LiteralPath $assemblyPath -Algorithm SHA256).Hash

$requiredFiles = @(
    $manifestPath,
    $assemblyPath,
    $weights,
    $defaultSettings,
    $missionOverrides,
    $startOverrides,
    $globalOverrides,
    $habModuleOverrides,
    $habOverrides,
    $nationLocalization,
    (Join-Path $repositoryRoot 'docs\current-implementation-matrix.xlsx')
)
foreach ($requiredFile in $requiredFiles) {
    if (-not (Test-Path -LiteralPath $requiredFile)) {
        throw "Required release input is missing: $requiredFile"
    }
}

$artifactDirectory = Join-Path $repositoryRoot 'artifacts'
if (-not (Test-Path -LiteralPath $artifactDirectory)) {
    New-Item -ItemType Directory -Path $artifactDirectory | Out-Null
}
$stagingDirectory = Join-Path $artifactDirectory 'TIEconomyMod'
if (Test-Path -LiteralPath $stagingDirectory) {
    $resolvedArtifacts = (Resolve-Path -LiteralPath $artifactDirectory).Path
    $resolvedStaging = (Resolve-Path -LiteralPath $stagingDirectory).Path
    if (-not $resolvedStaging.StartsWith($resolvedArtifacts + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Refusing to replace a staging directory outside artifacts.'
    }
    Remove-Item -LiteralPath $resolvedStaging -Recurse
}
New-Item -ItemType Directory -Path (Join-Path $stagingDirectory 'Assembly') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stagingDirectory 'Config') -Force | Out-Null
Copy-Item -LiteralPath $manifestPath -Destination $stagingDirectory
Copy-Item -LiteralPath $assemblyPath -Destination (Join-Path $stagingDirectory 'Assembly')
Copy-Item -LiteralPath $weights -Destination (Join-Path $stagingDirectory 'Config')
Copy-Item -LiteralPath $defaultSettings -Destination $stagingDirectory
Copy-Item -LiteralPath $missionOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $startOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $globalOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $habModuleOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $habOverrides -Destination $stagingDirectory
Copy-Item -LiteralPath $nationLocalization -Destination $stagingDirectory
$imagePath = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\Economic Equalization Overhaul.png'
if (Test-Path -LiteralPath $imagePath) {
    Copy-Item -LiteralPath $imagePath -Destination $stagingDirectory
}

$zipPath = Join-Path $artifactDirectory 'TIEconomyMod-0.7.0-ti1.0.49.zip'
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath
}
Compress-Archive -LiteralPath $stagingDirectory -DestinationPath $zipPath -CompressionLevel Optimal

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $packagedDll = $archive.Entries |
        Where-Object { $_.FullName.Replace('\', '/') -eq 'TIEconomyMod/Assembly/TIEconomyMod.dll' } |
        Select-Object -First 1
    if ($null -eq $packagedDll) {
        throw 'Release archive does not contain Assembly/TIEconomyMod.dll.'
    }
    $packagedSettings = $archive.Entries |
        Where-Object { $_.FullName.Replace('\', '/') -eq 'TIEconomyMod/Settings.xml' } |
        Select-Object -First 1
    $packagedWeights = $archive.Entries |
        Where-Object { $_.FullName.Replace('\', '/') -eq 'TIEconomyMod/Config/economy-tech-weights.csv' } |
        Select-Object -First 1
    if ($null -eq $packagedSettings -or $null -eq $packagedWeights) {
        throw 'Release archive is missing default Settings.xml or economy-tech-weights.csv.'
    }
    $stream = $packagedDll.Open()
    $sha256 = [Security.Cryptography.SHA256]::Create()
    try {
        $packagedHash = ([BitConverter]::ToString($sha256.ComputeHash($stream))).Replace('-', '')
    }
    finally {
        $sha256.Dispose()
        $stream.Dispose()
    }
}
finally {
    $archive.Dispose()
}
if ($packagedHash -ne $assemblyHash) {
    throw 'Packaged DLL does not match the newly built binary.'
}

Write-Host "PASS: release verification completed."
Write-Host "DLL SHA256: $assemblyHash"
Write-Host "Artifact: $zipPath"
Write-Host 'Compatibility target: TI 1.0.49 installed assemblies and guarded IL patch points.'
