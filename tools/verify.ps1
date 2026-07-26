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
$testExecutable = Join-Path $repositoryRoot 'tests\FormulaTests\bin\Release\TIEconomyMod.FormulaTests.exe'
& $testExecutable $weights
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$manifestPath = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\ModInfo.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.GameVersion -ne '1.0.39') {
    throw "ModInfo.json targets '$($manifest.GameVersion)' instead of TI 1.0.39."
}
if ($manifest.AssemblyName -ne 'Assembly/TIEconomyMod.dll') {
    throw "ModInfo.json has unexpected AssemblyName '$($manifest.AssemblyName)'."
}

$assemblyPath = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\Assembly\TIEconomyMod.dll'
$assemblyFile = Get-Item -LiteralPath $assemblyPath
if ($assemblyFile.LastWriteTime -lt $buildStarted.AddSeconds(-2)) {
    throw 'Packaged DLL predates this verification build.'
}
$assemblyHash = (Get-FileHash -LiteralPath $assemblyPath -Algorithm SHA256).Hash

$requiredFiles = @(
    $manifestPath,
    $assemblyPath,
    $weights,
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
$imagePath = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles\Economic Equalization Overhaul.png'
if (Test-Path -LiteralPath $imagePath) {
    Copy-Item -LiteralPath $imagePath -Destination $stagingDirectory
}

$zipPath = Join-Path $artifactDirectory 'TIEconomyMod-0.4.0-ti1.0.39.zip'
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath
}
Compress-Archive -LiteralPath $stagingDirectory -DestinationPath $zipPath -CompressionLevel Optimal

Write-Host "PASS: release verification completed."
Write-Host "DLL SHA256: $assemblyHash"
Write-Host "Artifact: $zipPath"
Write-Host 'Forward-compatibility note: behavior/metadata target TI 1.0.39; this build also compiled against the installed TI 1.0.47 assemblies.'
