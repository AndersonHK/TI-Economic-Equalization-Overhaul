[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
$settingsPath = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\Settings.xml'
$nationPath = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\TINationTemplate.json'
$auditPath = Join-Path $RepositoryRoot 'docs\environment-calibration\historical-start-calibration.csv'

foreach ($path in @($settingsPath, $nationPath, $auditPath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing Environment model artifact: $path"
    }
}

[xml]$settings = Get-Content -LiteralPath $settingsPath -Raw
$environment = $settings.Settings.environment
$emissions = $settings.Settings.emissions
if ([double]$environment.startingTechnologyCap -ne 3 -or
    [double]$environment.maximumTechnologyCap -ne 10 -or
    [double]$environment.advancementCostGrowthBase -ne 1.5 -or
    [double]$emissions.co2DecayBase -ne 0.25 -or
    [double]$emissions.methaneDecayBase -ne 0.9 -or
    [double]$emissions.nitrousOxideDecayBase -ne 0.9) {
    throw 'Environment settings do not contain the calibrated geometric coefficients.'
}

$nations = Get-Content -LiteralPath $nationPath -Raw | ConvertFrom-Json
if ($nations.Count -ne 518) {
    throw "Expected 518 scenario nation overrides, found $($nations.Count)."
}
$missing = @($nations | Where-Object { $null -eq $_.greenEconomy })
if ($missing.Count -gt 0) {
    throw "$($missing.Count) nation overrides lack calibrated greenEconomy storage."
}

$offset = [double]$environment.storageRatingOffset
foreach ($nation in $nations) {
    $stored = [double]$nation.greenEconomy
    if ($stored -le 0) {
        throw "$($nation.dataName) has non-positive Environment storage."
    }
    $rating = 1 / $stored - $offset
    if ($rating -lt -0.00001 -or $rating -gt 3.00001) {
        throw "$($nation.dataName) starts at rating $rating outside the 0-3 technology cap."
    }
}

$rows = @(Import-Csv -LiteralPath $auditPath)
if ($rows.Count -ne 518) {
    throw "Expected 518 Environment calibration rows, found $($rows.Count)."
}
foreach ($year in @(2003, 2022, 2026)) {
    $matched = @($rows | Where-Object {
        [int]$_.scenario -eq $year -and $_.calibrated_from_edgar -eq 'True'
    })
    if ($matched.Count -lt 160) {
        throw "$year has only $($matched.Count) EDGAR-matched nations."
    }
    foreach ($gas in @('co2', 'ch4', 'n2o')) {
        $actual = ($matched | Measure-Object -Property ("actual_${gas}_t") -Sum).Sum
        $predicted = ($matched | Measure-Object -Property ("predicted_${gas}_t") -Sum).Sum
        $ratio = [double]$predicted / [double]$actual
        $tolerance = if ($gas -eq 'co2') { 0.00001 } else { 0.02 }
        if ([Math]::Abs($ratio - 1) -gt $tolerance) {
            throw "$year $gas historical ratio $ratio exceeds tolerance $tolerance."
        }
    }
}

$boundaryNames = @('2003_CHN', '2026_SWI')
foreach ($name in $boundaryNames) {
    $row = $rows | Where-Object data_name -eq $name | Select-Object -First 1
    if ($null -eq $row -or [double]$row.rating -lt 0 -or [double]$row.rating -gt 3) {
        throw "Boundary calibration $name is missing or outside the starting cap."
    }
}

Write-Host 'PASS: geometric Environment coefficients, scenario ratings, and historical audit validated.'
