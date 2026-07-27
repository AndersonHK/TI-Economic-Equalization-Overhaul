[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ModAssemblyPath
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $ModAssemblyPath)) {
    throw "Mod assembly not found: $ModAssemblyPath"
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
    'ti-eeo-hab-cost-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $probeDirectory | Out-Null
$outputPath = Join-Path $probeDirectory 'HabCostFromSpaceRewritePatch.il'

try {
    & $ildasm $ModAssemblyPath /text /nobar "/out:$outputPath"
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $outputPath)) {
        throw 'Could not disassemble the mod assembly.'
    }

    $assemblyIl = Get-Content -LiteralPath $outputPath -Raw
    $methodMatch = [regex]::Match(
        $assemblyIl,
        '(?s)\.class private abstract auto ansi sealed beforefieldinit ' +
        'TIEconomyMod\.Patches\.HabCostFromSpaceRewritePatch.*?' +
        '// end of method HabCostFromSpaceRewritePatch::Prefix')
    if (-not $methodMatch.Success) {
        throw 'Could not locate HabCostFromSpaceRewritePatch.Prefix IL.'
    }
    $methodIl = $methodMatch.Value
    $substitution = [regex]::Match(
        $methodIl,
        'TIResourcesCost::GetBoostSubstitutedCost\(')
    $mandatoryBoost = [regex]::Match(
        $methodIl,
        'HabConstructionCostRewrite::MandatoryBoost\(')
    $addCost = [regex]::Match(
        $methodIl,
        'TIResourcesCost::AddCost\(valuetype [^\r\n]*FactionResource,')

    if (-not $substitution.Success -or
        -not $mandatoryBoost.Success -or
        -not $addCost.Success) {
        throw 'Hab space-cost rewrite is missing substitution or mandatory-Boost IL.'
    }
    if ($substitution.Index -ge $mandatoryBoost.Index -or
        $mandatoryBoost.Index -ge $addCost.Index) {
        throw (
            'Mandatory Boost must be calculated and added only after ordinary ' +
            'material substitution.')
    }

    $getBoostSubstitutedCostCalls = [regex]::Matches(
        $methodIl,
        'TIResourcesCost::GetBoostSubstitutedCost\(').Count
    $mandatoryBoostCalls = [regex]::Matches(
        $methodIl,
        'HabConstructionCostRewrite::MandatoryBoost\(').Count
    if ($getBoostSubstitutedCostCalls -ne 1 -or $mandatoryBoostCalls -ne 1) {
        throw (
            'Expected exactly one ordinary-material substitution and one ' +
            "mandatory-Boost calculation; found $getBoostSubstitutedCostCalls " +
            "and $mandatoryBoostCalls.")
    }

    Write-Host (
        'PASS: ordinary materials are substituted before mandatory Boost is ' +
        'added, preventing double transfer scaling.')
}
finally {
    $resolvedProbe = (Resolve-Path -LiteralPath $probeDirectory).Path
    $resolvedTemp = (Resolve-Path -LiteralPath ([IO.Path]::GetTempPath())).Path.TrimEnd('\')
    if (-not $resolvedProbe.StartsWith(
        $resolvedTemp + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Refusing to remove a hab-cost probe outside the system temp directory.'
    }
    Remove-Item -LiteralPath $resolvedProbe -Recurse
}
