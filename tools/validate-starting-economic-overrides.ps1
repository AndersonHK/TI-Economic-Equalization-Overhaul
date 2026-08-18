[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$VanillaTemplatesDir,
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
$toolsDirectory = Join-Path $RepositoryRoot 'tools'
$proposalCsv = Join-Path $RepositoryRoot 'docs\economic-data\country-economic-clamp-proposal-2022-usd.csv'
$authoredDirectory = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles'
$nationOverrides = Join-Path $authoredDirectory 'TINationTemplate.json'
$regionOverrides = Join-Path $authoredDirectory 'TIRegionTemplate.json'

foreach ($required in @($proposalCsv, $nationOverrides, $regionOverrides)) {
    if (-not (Test-Path -LiteralPath $required)) {
        throw "Missing starting-economic input or override: $required"
    }
}

$streamingAssets = Split-Path -Parent $VanillaTemplatesDir
$dataDirectory = Split-Path -Parent $streamingAssets
$gameRoot = Split-Path -Parent $dataDirectory
$systemTemp = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')
$verificationDirectory = Join-Path $systemTemp ('ti-eeo-economic-' + [Guid]::NewGuid().ToString('N'))

try {
    New-Item -ItemType Directory -Path $verificationDirectory | Out-Null
    powershell -NoProfile -ExecutionPolicy Bypass -File `
        (Join-Path $toolsDirectory 'sync-starting-economic-values.ps1') `
        -GameInstallDir $gameRoot `
        -ProposalCsv $proposalCsv `
        -OutputDirectory $verificationDirectory
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    foreach ($filename in @('TINationTemplate.json', 'TIRegionTemplate.json')) {
        $expected = Join-Path $verificationDirectory $filename
        $actual = Join-Path $authoredDirectory $filename
        $expectedHash = (Get-FileHash -LiteralPath $expected -Algorithm SHA256).Hash
        $actualHash = (Get-FileHash -LiteralPath $actual -Algorithm SHA256).Hash
        if ($expectedHash -ne $actualHash) {
            throw "$filename is stale. Run tools\sync-starting-economic-values.ps1 and commit the regenerated file."
        }
    }

    $nationRows = Get-Content -LiteralPath $nationOverrides -Raw -Encoding UTF8 | ConvertFrom-Json
    $regionRows = Get-Content -LiteralPath $regionOverrides -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($nationRows.Count -ne 518) {
        throw "Expected 518 nation overrides, found $($nationRows.Count)."
    }
    if ($regionRows.Count -ne 1088) {
        throw "Expected 1088 region overrides, found $($regionRows.Count)."
    }
    if (@($nationRows | Group-Object dataName | Where-Object Count -ne 1).Count -gt 0) {
        throw 'Nation override dataNames are not unique.'
    }
    if (@($regionRows | Group-Object dataName | Where-Object Count -ne 1).Count -gt 0) {
        throw 'Region override dataNames are not unique.'
    }
    if (@($nationRows | Where-Object { [double]$_.initialGDP -le 0 }).Count -gt 0) {
        throw 'Nation overrides contain a non-positive initialGDP.'
    }
    if (@($regionRows | Where-Object { [double]$_.population_Millions -le 0 }).Count -gt 0) {
        throw 'Region overrides contain a non-positive population.'
    }

    Write-Host 'PASS: starting-economic overrides match all 518 reviewed country-year proposals.'
    Write-Host 'PASS: 518 nation GDP and 1088 regional population overrides are unique and positive.'
}
finally {
    if (Test-Path -LiteralPath $verificationDirectory) {
        $resolvedVerification = [IO.Path]::GetFullPath($verificationDirectory).TrimEnd('\')
        if (-not $resolvedVerification.StartsWith($systemTemp + '\', [StringComparison]::OrdinalIgnoreCase) -or
            (Split-Path -Leaf $resolvedVerification) -notlike 'ti-eeo-economic-*') {
            throw "Refusing to remove unexpected verification directory: $resolvedVerification"
        }
        Remove-Item -LiteralPath $resolvedVerification -Recurse -Force
    }
}
