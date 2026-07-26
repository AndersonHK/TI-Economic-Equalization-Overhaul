[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$TargetManagedDir
)

$ErrorActionPreference = 'Stop'
$assemblyPath = Join-Path $TargetManagedDir 'Assembly-CSharp.dll'
if (-not (Test-Path -LiteralPath $assemblyPath)) {
    throw "Target assembly not found: $assemblyPath"
}

$ildasm = Get-ChildItem 'C:\Program Files (x86)\Microsoft SDKs\Windows' `
    -Recurse -Filter ildasm.exe -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '\\x64\\' } |
    Sort-Object FullName -Descending |
    Select-Object -First 1 -ExpandProperty FullName
if ([string]::IsNullOrWhiteSpace($ildasm)) {
    throw 'ildasm.exe was not found in the installed .NET Framework SDK.'
}

$probeDirectory = Join-Path ([IO.Path]::GetTempPath()) (
    'ti-eeo-il-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $probeDirectory | Out-Null

function Read-MethodIl {
    param(
        [string]$TypeName,
        [string]$MethodName
    )

    $outputPath = Join-Path $probeDirectory ($MethodName + '.il')
    & $ildasm $assemblyPath /text /nobar "/item:$TypeName::$MethodName" "/out:$outputPath"
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $outputPath)) {
        throw "Could not disassemble $TypeName::$MethodName."
    }
    return Get-Content -LiteralPath $outputPath -Raw
}

function Assert-Count {
    param(
        [string]$Text,
        [string]$Pattern,
        [int]$Expected,
        [string]$Description
    )

    $actual = [regex]::Matches($Text, $Pattern).Count
    if ($actual -ne $Expected) {
        throw "${Description}: expected $Expected IL matches, found $actual."
    }
}

try {
    $nation = 'PavonisInteractive.TerraInvicta.TINationState'
    $economy = Read-MethodIl $nation 'OnEconomyPriorityComplete'
    Assert-Count $economy 'ldfld\s+int32 TIGlobalConfig::numEcosForCoreOilRegion' 1 'Economy oil threshold'
    Assert-Count $economy 'ldfld\s+int32 TIGlobalConfig::numEcosForCoreMiningRegion' 1 'Economy mining threshold'
    Assert-Count $economy 'ldfld\s+int32 TIGlobalConfig::numEcosForCoreEcoRegion' 1 'Economy core threshold'

    $welfare = Read-MethodIl $nation 'OnWelfarePriorityComplete'
    Assert-Count $welfare 'ldc\.i4\s+0x3e8' 1 'Welfare decolonization threshold'

    $environment = Read-MethodIl $nation 'OnEnvironmentPriorityComplete'
    Assert-Count $environment 'ldc\.i4\.s\s+100' 1 'Environment fallout threshold'

    $tooltip = Read-MethodIl 'PavonisInteractive.TerraInvicta.PriorityListItemController' 'priorityTipStr'
    Assert-Count $tooltip 'ldfld\s+int32 TIGlobalConfig::numEcosForCoreOilRegion' 1 'Tooltip oil threshold'
    Assert-Count $tooltip 'ldfld\s+int32 TIGlobalConfig::numEcosForCoreMiningRegion' 1 'Tooltip mining threshold'
    Assert-Count $tooltip 'ldfld\s+int32 TIGlobalConfig::numEcosForCoreEcoRegion' 1 'Tooltip core threshold'
    Assert-Count $tooltip 'ldc\.i4\s+0x3e8' 1 'Tooltip decolonization threshold'
    Assert-Count $tooltip 'ldc\.i4\.s\s+100' 1 'Tooltip fallout threshold'

    $unity = Read-MethodIl $nation 'OnUnityPriorityComplete'
    Assert-Count $unity 'ldfld\s+float32 TIGlobalConfig::unityPublicOpinionBaseStrength' 1 'Unity propaganda strength'

    $spoils = Read-MethodIl $nation 'OnSpoilsPriorityComplete'
    Assert-Count $spoils 'ldfld\s+float32 TIGlobalConfig::spoilsPriorityPublicOpinionScaling' 1 'Spoils propaganda scaling'

    $emissions = Read-MethodIl $nation 'GHGsFromEconomy_tons'
    Assert-Count $emissions 'TINationState::get_GDP\(\)' 1 'Economy emissions GDP input'

    Write-Host 'PASS: target IL contains every guarded TI 1.0.39/forward-compatible patch point.'
}
finally {
    $resolvedProbe = (Resolve-Path -LiteralPath $probeDirectory).Path
    $resolvedTemp = (Resolve-Path -LiteralPath ([IO.Path]::GetTempPath())).Path.TrimEnd('\')
    if (-not $resolvedProbe.StartsWith(
        $resolvedTemp + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Refusing to remove an IL probe directory outside the system temp directory.'
    }
    Remove-Item -LiteralPath $resolvedProbe -Recurse
}
