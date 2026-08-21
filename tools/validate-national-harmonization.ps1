[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$VanillaTemplatesDir,
    [string]$DlcTemplatesDir,
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
$sourcePath = Join-Path $RepositoryRoot 'TIEconomyMod\Patches\NationalHarmonizationPatches.cs'
$settingsPath = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\Settings.xml'
$basePath = Join-Path $VanillaTemplatesDir 'TIBilateralTemplate.json'
if (-not (Test-Path -LiteralPath $sourcePath) -or
    -not (Test-Path -LiteralPath $settingsPath) -or
    -not (Test-Path -LiteralPath $basePath)) {
    throw 'National harmonization validation is missing source, settings, or vanilla bilateral templates.'
}

$source = Get-Content -LiteralPath $sourcePath -Raw
$settings = Get-Content -LiteralPath $settingsPath -Raw
foreach ($required in @(
    'claimant.perCapitaGDP',
    'target.perCapitaGDP',
    'Project_RestoredWarsawPact',
    'Project_ForwardRussia',
    'Project_LiberatingMainlandChina',
    'GameControl.DLCValidated',
    'ModManager.dlcNames.Contains(DarkSkies)',
    'scenarioTemplate.requiredDLC.Contains',
    'return !is2003',
    '"GTM"',
    '"ArunchalPradesh"')) {
    if (-not $source.Contains($required)) {
        throw "National harmonization source is missing required contract token '$required'."
    }
}
if ($source.Contains('claimant.GDP') -or $source.Contains('target.GDP')) {
    throw 'National harmonization must use GDP per capita, not total national GDP.'
}
foreach ($required in @(
    '<ordinaryThreshold>6</ordinaryThreshold>',
    '<historicalThreshold>3</historicalThreshold>',
    '<federationThreshold>12</federationThreshold>')) {
    if (-not $settings.Contains($required)) {
        throw "National harmonization settings are missing '$required'."
    }
}

function Read-Claims {
    param([string]$Path)
    return @(Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json |
        Where-Object { $_.relationType -eq 'Claim' })
}

function Normalize-Id {
    param([string]$Value)
    return $Value -replace '^(2003_|2026_|2070_)', ''
}

function Assert-ClaimSet {
    param(
        [object[]]$Claims,
        [int]$ExpectedScenarioCount,
        [switch]$IncludeRussiaUkraine
    )
    $pairs = @(
        'CHN|Taiwan',
        'PAK|JammuandKashmir',
        'PRK|SouthKorea',
        'KOR|NorthKorea',
        'CHN|ArunchalPradesh',
        'RUS|Georgia',
        'RUS|Moldova',
        'RUS|Estonia',
        'RUS|Latvia',
        'RUS|Lithuania',
        'VEN|Guyana',
        'JPN|SakhalinKurils',
        'SYR|Lebanon',
        'ERI|Mekelle',
        'GTM|Belize'
    )
    if ($IncludeRussiaUkraine) {
        $pairs += @(
            'RUS|Donetsk',
            'RUS|Kharkiv',
            'RUS|Kiev',
            'RUS|Odesa'
        )
    }
    foreach ($pair in $pairs) {
        $ids = $pair.Split('|')
        $matches = @($Claims | Where-Object {
            (Normalize-Id $_.nation1) -eq $ids[0] -and
            (Normalize-Id $_.region1) -eq $ids[1]
        })
        if ($matches.Count -ne $ExpectedScenarioCount) {
            throw "Expected $ExpectedScenarioCount claim record(s) for $($ids[0]) -> $($ids[1]); found $($matches.Count)."
        }
    }
}

$baseClaims = Read-Claims $basePath
Assert-ClaimSet -Claims $baseClaims -ExpectedScenarioCount 3 -IncludeRussiaUkraine
$projectNames = @(
    'Project_RestoredWarsawPact',
    'Project_ForwardRussia',
    'Project_LiberatingMainlandChina'
)
$baseMissingProjectFlags = @($baseClaims | Where-Object {
    $_.projectUnlockName -in $projectNames -and $_.hostileClaim -ne $true
}).Count
if ($baseMissingProjectFlags -ne 108) {
    throw "Expected 108 base-scenario project-family flags to normalize; found $baseMissingProjectFlags."
}

if (-not [string]::IsNullOrWhiteSpace($DlcTemplatesDir)) {
    $dlcPath = Join-Path $DlcTemplatesDir 'TIBilateralTemplate.json'
    if (Test-Path -LiteralPath $dlcPath) {
        $dlcClaims = Read-Claims $dlcPath
        Assert-ClaimSet -Claims $dlcClaims -ExpectedScenarioCount 1
        $ukraineRegions = @('Donetsk', 'Kharkiv', 'Kiev', 'Odesa')
        foreach ($region in $ukraineRegions) {
            $claim = @($dlcClaims | Where-Object {
                (Normalize-Id $_.nation1) -eq 'RUS' -and
                (Normalize-Id $_.region1) -eq $region
            })
            if ($claim.Count -ne 1 -or $claim[0].hostileClaim -eq $true) {
                throw "Dark Skies Russia -> $region must exist exactly once and remain ordinary."
            }
        }
        $dlcMissingProjectFlags = @($dlcClaims | Where-Object {
            $_.projectUnlockName -in $projectNames -and $_.hostileClaim -ne $true
        }).Count
        if ($dlcMissingProjectFlags -ne 36) {
            throw "Expected 36 Dark Skies project-family flags to normalize; found $dlcMissingProjectFlags."
        }
        if ($baseMissingProjectFlags + $dlcMissingProjectFlags -ne 144) {
            throw 'The approved four-scenario project-family normalization count changed.'
        }
    }
}

Write-Host 'PASS: national harmonization data, settings, historical claims, and Dark Skies gate validated.'
