[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$TargetManagedDir,
    [Parameter(Mandatory = $true)][string]$ModAssemblyPath
)
$ErrorActionPreference = 'Stop'
$modPath = (Resolve-Path -LiteralPath $ModAssemblyPath).Path
$references = @(
    (Join-Path $TargetManagedDir 'Assembly-CSharp.dll'),
    (Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll'),
    (Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'),
    (Join-Path $TargetManagedDir 'UnityEngine.CoreModule.dll'),
    (Join-Path $TargetManagedDir 'UnityEngine.dll'),
    (Join-Path $TargetManagedDir 'netstandard.dll'),
    $modPath, 'System.Core'
)
foreach ($file in Get-ChildItem -LiteralPath $TargetManagedDir -Filter '*.dll' -File) {
    if ($file.Name -like 'Unity*' -or $file.Name -in @('Newtonsoft.Json.dll', 'FMODUnity.dll')) {
        [void][Reflection.Assembly]::Load([IO.File]::ReadAllBytes($file.FullName))
    }
}
foreach ($path in $references) {
    if (Test-Path -LiteralPath $path) { [void][Reflection.Assembly]::Load([IO.File]::ReadAllBytes($path)) }
}
$source = Join-Path $PSScriptRoot 'validation\MissionProbabilityValidation.cs'
Add-Type -Path $source -ReferencedAssemblies $references
try { Write-Host ([MissionProbabilityValidation]::Run()) }
catch {
    Write-Host $_.Exception.ToString()
    throw
}
