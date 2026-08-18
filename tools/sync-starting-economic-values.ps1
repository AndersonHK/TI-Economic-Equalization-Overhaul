[CmdletBinding()]
param(
    [string]$GameInstallDir,
    [string]$ProposalCsv,
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = Split-Path -Parent $scriptDirectory

if ([string]::IsNullOrWhiteSpace($ProposalCsv)) {
    $ProposalCsv = Join-Path $repositoryRoot 'docs\economic-data\country-economic-clamp-proposal-2022-usd.csv'
}
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repositoryRoot 'TIEconomyMod\ModFiles'
}

function Find-GameInstall {
    param([string]$ExplicitPath)

    $candidates = @(
        $ExplicitPath,
        $env:TI_GAME_INSTALL_DIR,
        'D:\Games\SteamLibrary\steamapps\common\Terra Invicta',
        (Join-Path ${env:ProgramFiles(x86)} 'Steam\steamapps\common\Terra Invicta')
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique
    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath (Join-Path $candidate 'TerraInvicta_Data\StreamingAssets\Templates\TINationTemplate.json')) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }
    throw 'Could not locate Terra Invicta. Pass -GameInstallDir or set TI_GAME_INSTALL_DIR.'
}

function Read-TemplateJson {
    param([string]$Path)

    $raw = [IO.File]::ReadAllText($Path, [Text.Encoding]::UTF8)
    $withoutComments = [regex]::Replace($raw, '(?m)^\s*//.*$', '')
    $strictJson = [regex]::Replace($withoutComments, ',\s*([}\]])', '$1')
    return @($strictJson | ConvertFrom-Json)
}

function Normalize-NationName {
    param([string]$Value)

    $normalized = $Value.Normalize([Text.NormalizationForm]::FormD)
    $builder = [Text.StringBuilder]::new()
    foreach ($character in $normalized.ToCharArray()) {
        if ([Globalization.CharUnicodeInfo]::GetUnicodeCategory($character) -ne
            [Globalization.UnicodeCategory]::NonSpacingMark) {
            [void]$builder.Append($character)
        }
    }
    return ([regex]::Replace($builder.ToString().ToLowerInvariant(), '\band\b|[^a-z0-9]', ''))
}

$ownerAliases = @{
    belgium = 'belgiumluxembourg'
    gulfstates = 'qatarbahrain'
    melanesianstates = 'melanesia'
    micronesianstates = 'micronesia'
    polynesianstates = 'polynesia'
    unitedstatesofamerica = 'usa'
}

function Get-OwnerKey {
    param([string]$FriendlyName)

    $key = Normalize-NationName ($FriendlyName -replace '^(2003_|2026_)', '')
    if ($ownerAliases.ContainsKey($key)) {
        return $ownerAliases[$key]
    }
    return $key
}

$gameRoot = Find-GameInstall -ExplicitPath $GameInstallDir
$baseTemplates = Join-Path $gameRoot 'TerraInvicta_Data\StreamingAssets\Templates'
$darkSkiesTemplates = Join-Path $gameRoot 'DLC_Content\DarkSkies\2003_Scenario\Templates'
$scenarioSpecs = @(
    [pscustomobject]@{ Year = 2003; Directory = $darkSkiesTemplates; NationSet = '2003_Nations'; RegionSet = '2003_Regions' },
    [pscustomobject]@{ Year = 2022; Directory = $baseTemplates; NationSet = 'ModernNations'; RegionSet = 'ModernRegions' },
    [pscustomobject]@{ Year = 2026; Directory = $baseTemplates; NationSet = '2026_Nations'; RegionSet = '2026_Regions' }
)

$proposalRows = @(Import-Csv -LiteralPath $ProposalCsv -Encoding UTF8)
if ($proposalRows.Count -ne 518) {
    throw "Expected 518 proposal rows, found $($proposalRows.Count)."
}
$proposalIndex = @{}
foreach ($row in $proposalRows) {
    $key = "$($row.country_id)|$($row.scenario_year)"
    if ($proposalIndex.ContainsKey($key)) {
        throw "Duplicate proposal row: $key"
    }
    $proposalIndex[$key] = $row
}

$nationOverrides = [System.Collections.Generic.List[object]]::new()
$regionOverrides = [System.Collections.Generic.List[object]]::new()
$seenNations = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$seenRegions = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$mappedProposalKeys = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)

foreach ($spec in $scenarioSpecs) {
    $meta = Read-TemplateJson (Join-Path $spec.Directory 'TIMetaTemplate.json')
    $nations = Read-TemplateJson (Join-Path $spec.Directory 'TINationTemplate.json')
    $regions = Read-TemplateJson (Join-Path $spec.Directory 'TIRegionTemplate.json')
    $nationIds = [System.Collections.Generic.HashSet[string]]::new(
        [string[]](($meta | Where-Object dataName -eq $spec.NationSet).templateNames),
        [StringComparer]::Ordinal)
    $regionIds = [System.Collections.Generic.HashSet[string]]::new(
        [string[]](($meta | Where-Object dataName -eq $spec.RegionSet).templateNames),
        [StringComparer]::Ordinal)
    $scenarioRegions = @($regions | Where-Object { $regionIds.Contains([string]$_.dataName) })

    foreach ($nation in @($nations | Where-Object { $nationIds.Contains([string]$_.dataName) -and [double]$_.initialGDP -gt 0 })) {
        $countryKey = if ([string]::IsNullOrWhiteSpace([string]$nation.referenceAlias)) {
            ([string]$nation.dataName) -replace '^(2003_|2026_)', ''
        } else {
            [string]$nation.referenceAlias
        }
        $proposalKey = "$countryKey|$($spec.Year)"
        if (-not $proposalIndex.ContainsKey($proposalKey)) {
            continue
        }
        $proposal = $proposalIndex[$proposalKey]
        $ownerKey = Get-OwnerKey ([string]$nation.friendlyName)
        $ownedRegions = @($scenarioRegions | Where-Object {
            (Normalize-NationName ([string]$_.sortNation)) -eq $ownerKey
        })
        if ($ownedRegions.Count -eq 0) {
            throw "No $($spec.Year) regions found for $($nation.dataName) ($($proposal.country)); owner key '$ownerKey'."
        }

        if (-not $seenNations.Add([string]$nation.dataName)) {
            throw "Duplicate nation dataName across generated overrides: $($nation.dataName)"
        }
        $initialGdp = [long]::Parse($proposal.proposed_json_initial_gdp, [Globalization.CultureInfo]::InvariantCulture)
        $nationOverrides.Add([pscustomobject][ordered]@{
            dataName = [string]$nation.dataName
            initialGDP = $initialGdp
        })

        $targetPopulationMillions = [decimal]::Parse(
            $proposal.proposed_population,
            [Globalization.CultureInfo]::InvariantCulture) / 1000000
        $vanillaPopulationMillions = [decimal]0
        foreach ($region in $ownedRegions) {
            $vanillaPopulationMillions += [decimal]$region.population_Millions
        }
        if ($vanillaPopulationMillions -le 0) {
            throw "Non-positive vanilla population for $($proposal.country) $($spec.Year)."
        }

        $scaledValues = [System.Collections.Generic.List[decimal]]::new()
        $runningTotal = [decimal]0
        for ($index = 0; $index -lt $ownedRegions.Count; $index++) {
            if ($index -eq $ownedRegions.Count - 1) {
                $scaled = $targetPopulationMillions - $runningTotal
            } else {
                $scaled = [Math]::Round(
                    ([decimal]$ownedRegions[$index].population_Millions / $vanillaPopulationMillions) * $targetPopulationMillions,
                    9,
                    [MidpointRounding]::AwayFromZero)
                $runningTotal += $scaled
            }
            if ($scaled -le 0) {
                throw "Generated non-positive population for region $($ownedRegions[$index].dataName)."
            }
            $scaledValues.Add($scaled)
        }
        $authoredPopulation = ($scaledValues | Measure-Object -Sum).Sum * 1000000
        $targetPopulation = [decimal]::Parse(
            $proposal.proposed_population,
            [Globalization.CultureInfo]::InvariantCulture)
        if ([Math]::Abs([decimal]$authoredPopulation - $targetPopulation) -gt 1) {
            throw "Population rounding drift exceeds one person for $($proposal.country) $($spec.Year)."
        }

        for ($index = 0; $index -lt $ownedRegions.Count; $index++) {
            $region = $ownedRegions[$index]
            if (-not $seenRegions.Add([string]$region.dataName)) {
                throw "Duplicate region dataName across generated overrides: $($region.dataName)"
            }
            $regionOverrides.Add([pscustomobject][ordered]@{
                dataName = [string]$region.dataName
                population_Millions = $scaledValues[$index]
            })
        }
        [void]$mappedProposalKeys.Add($proposalKey)
    }
}

$unmapped = @($proposalIndex.Keys | Where-Object { -not $mappedProposalKeys.Contains($_) })
if ($unmapped.Count -gt 0) {
    throw "Unmapped proposal rows: $($unmapped -join ', ')"
}
if ($nationOverrides.Count -ne 518) {
    throw "Expected 518 nation overrides, generated $($nationOverrides.Count)."
}

if (-not (Test-Path -LiteralPath $OutputDirectory)) {
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
}
$nationPath = Join-Path $OutputDirectory 'TINationTemplate.json'
$regionPath = Join-Path $OutputDirectory 'TIRegionTemplate.json'
$jsonOptions = @{ Depth = 5 }
$utf8NoBom = [Text.UTF8Encoding]::new($false)
[IO.File]::WriteAllText($nationPath, (($nationOverrides | ConvertTo-Json @jsonOptions) + [Environment]::NewLine), $utf8NoBom)
[IO.File]::WriteAllText($regionPath, (($regionOverrides | ConvertTo-Json @jsonOptions) + [Environment]::NewLine), $utf8NoBom)

Write-Host "PASS: generated $($nationOverrides.Count) nation and $($regionOverrides.Count) region overrides."
Write-Host "Nation output: $nationPath"
Write-Host "Region output: $regionPath"
