[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$VanillaTemplatesDir,
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = Split-Path -Parent $scriptDirectory
}

function Read-LooseJson {
    param([Parameter(Mandatory = $true)][string]$Path)

    $text = Get-Content -LiteralPath $Path -Raw
    $text = [regex]::Replace($text, '(?m)//.*$', '')
    $text = [regex]::Replace($text, ',(?=\s*[\]}])', '')
    return $text | ConvertFrom-Json | ForEach-Object { $_ }
}

function Has-Property {
    param($Object, [string]$Name)
    return $null -ne $Object -and $Name -in $Object.PSObject.Properties.Name
}

function Resolve-Field {
    param($Override, $Vanilla, [string]$Name)

    if (Has-Property $Override $Name) {
        return $Override.$Name
    }
    if (Has-Property $Vanilla $Name) {
        return $Vanilla.$Name
    }
    throw "Army '$($Override.dataName)' has no '$Name' in either override or vanilla data."
}

function New-Expectation {
    param([int]$Armies, [int]$Navies, [double]$Strength)
    return [pscustomobject]@{
        Armies = $Armies
        Navies = $Navies
        Strength = $Strength
    }
}

$expected2022 = [ordered]@{
    DZA = New-Expectation 1 0 1
    AUS = New-Expectation 1 1 1
    BRA = New-Expectation 3 0 1
    CHN = New-Expectation 13 4 1
    EGY = New-Expectation 2 1 1
    ETH = New-Expectation 1 0 0.8
    EUA = New-Expectation 2 2 1
    DEU = New-Expectation 2 0 1
    GRE = New-Expectation 1 0 1
    IND = New-Expectation 11 2 1
    IDN = New-Expectation 2 0 1
    IRN = New-Expectation 3 0 1
    ISR = New-Expectation 2 0 1
    ITA = New-Expectation 2 1 1
    JPN = New-Expectation 2 2 1
    MMR = New-Expectation 1 0 0.7
    PRK = New-Expectation 4 0 1
    PAK = New-Expectation 4 0 1
    POL = New-Expectation 1 0 1
    RUS = New-Expectation 6 2 0.75
    SAU = New-Expectation 1 0 1
    KOR = New-Expectation 3 1 1
    ESP = New-Expectation 1 1 1
    THA = New-Expectation 2 0 1
    TUR = New-Expectation 3 1 1
    UKR = New-Expectation 5 0 0.75
    GBR = New-Expectation 2 2 1
    USA = New-Expectation 13 12 1
    VNM = New-Expectation 3 0 1
}

$expected2026 = [ordered]@{
    DZA = New-Expectation 1 0 1
    AUS = New-Expectation 1 1 1
    BRA = New-Expectation 3 0 1
    CHN = New-Expectation 14 6 1
    EGY = New-Expectation 2 1 1
    ETH = New-Expectation 1 0 0.75
    EUA = New-Expectation 2 2 1
    DEU = New-Expectation 2 0 1
    GRE = New-Expectation 1 0 1
    IND = New-Expectation 11 2 1
    IDN = New-Expectation 2 0 1
    IRN = New-Expectation 3 0 1
    ISR = New-Expectation 2 0 1
    ITA = New-Expectation 2 1 1
    JPN = New-Expectation 3 2 1
    MMR = New-Expectation 1 0 0.6
    PRK = New-Expectation 4 0 1
    PAK = New-Expectation 4 0 1
    POL = New-Expectation 2 0 1
    RUS = New-Expectation 8 1 0.75
    SAU = New-Expectation 2 0 1
    KOR = New-Expectation 4 1 1
    ESP = New-Expectation 1 1 1
    THA = New-Expectation 2 0 1
    TUR = New-Expectation 3 1 1
    UKR = New-Expectation 6 0 0.75
    GBR = New-Expectation 2 2 1
    USA = New-Expectation 13 13 1
    VNM = New-Expectation 3 0 1
}

$sortNationByCode = @{
    DZA = 'Algeria'; AUS = 'Australia'; BRA = 'Brazil'; CHN = 'China'
    EGY = 'Egypt'; ETH = 'Ethiopia'; EUA = 'France'; DEU = 'Germany'
    GRE = 'Greece'; IND = 'India'; IDN = 'Indonesia'; IRN = 'Iran'
    ISR = 'Israel'; ITA = 'Italy'; JPN = 'Japan'; MMR = 'Myanmar'
    PRK = 'NorthKorea'; PAK = 'Pakistan'; POL = 'Poland'; RUS = 'Russia'
    SAU = 'Saudi Arabia'; KOR = 'SouthKorea'; ESP = 'Spain'; THA = 'Thailand'
    TUR = 'Turkey'; UKR = 'Ukraine'; GBR = 'United Kingdom'; USA = 'USA'
    VNM = 'Vietnam'
}

$modFiles = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles'
$armyOverridePath = Join-Path $modFiles 'TIArmyTemplate.json'
$metaOverridePath = Join-Path $modFiles 'TIMetaTemplate.json'
$vanillaArmyPath = Join-Path $VanillaTemplatesDir 'TIArmyTemplate.json'
$vanillaRegionPath = Join-Path $VanillaTemplatesDir 'TIRegionTemplate.json'
foreach ($path in @($armyOverridePath, $metaOverridePath, $vanillaArmyPath, $vanillaRegionPath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Starting-force validation input is missing: $path"
    }
}

# Mod JSON must remain strict even though the installed vanilla files permit comments/trailing commas.
$overrides = @(Get-Content -LiteralPath $armyOverridePath -Raw |
    ConvertFrom-Json | ForEach-Object { $_ })
$metaOverrides = @(Get-Content -LiteralPath $metaOverridePath -Raw |
    ConvertFrom-Json | ForEach-Object { $_ })
$vanillaArmies = @(Read-LooseJson $vanillaArmyPath)
$regions = @(Read-LooseJson $vanillaRegionPath)

$duplicateOverrides = @($overrides | Group-Object dataName | Where-Object Count -ne 1)
if ($duplicateOverrides.Count -gt 0) {
    throw "TIArmyTemplate overrides contain duplicate IDs: $($duplicateOverrides.Name -join ', ')."
}
$overrideById = @{}
foreach ($army in $overrides) { $overrideById[$army.dataName] = $army }
$vanillaById = @{}
foreach ($army in $vanillaArmies) { $vanillaById[$army.dataName] = $army }
$regionById = @{}
foreach ($region in $regions) { $regionById[$region.dataName] = $region }

function Test-Scenario {
    param(
        [string]$MetaName,
        [string]$Prefix,
        [System.Collections.IDictionary]$Expected
    )

    $meta = @($metaOverrides | Where-Object dataName -eq $MetaName)
    if ($meta.Count -ne 1 -or $meta[0].templateType -ne 'TIArmyTemplate') {
        throw "TIMetaTemplate must contain exactly one '$MetaName' TIArmyTemplate set."
    }
    $ids = @($meta[0].templateNames)
    if (@($ids | Sort-Object -Unique).Count -ne $ids.Count) {
        throw "'$MetaName' contains duplicate army IDs."
    }

    $resolved = New-Object System.Collections.Generic.List[object]
    foreach ($id in $ids) {
        if (($Prefix -eq '' -and $id.StartsWith('2026_')) -or
            ($Prefix -ne '' -and -not $id.StartsWith($Prefix))) {
            throw "Army '$id' has the wrong scenario prefix for '$MetaName'."
        }
        $override = $overrideById[$id]
        $vanilla = $vanillaById[$id]
        if ($null -eq $override -and $null -eq $vanilla) {
            throw "Army '$id' is selected by '$MetaName' but has no template."
        }
        $unprefixed = if ($Prefix -eq '') { $id } else { $id.Substring($Prefix.Length) }
        $match = [regex]::Match($unprefixed, '^(?<code>[A-Z]{3})Army(?<index>[1-9][0-9]*)$')
        if (-not $match.Success) {
            throw "Army ID '$id' does not follow the scenario/country/index convention."
        }
        $code = $match.Groups['code'].Value
        if (-not $Expected.Contains($code)) {
            throw "Army '$id' belongs to unexpected country code '$code'."
        }
        $startRegion = [string](Resolve-Field $override $vanilla 'startRegionStr')
        $homeRegion = [string](Resolve-Field $override $vanilla 'homeRegionStr')
        $deployment = [string](Resolve-Field $override $vanilla 'deploymentType')
        $strength = [double](Resolve-Field $override $vanilla 'startingStrength')
        if (-not $regionById.ContainsKey($startRegion) -or -not $regionById.ContainsKey($homeRegion)) {
            throw "Army '$id' references missing start/home region '$startRegion'/'$homeRegion'."
        }
        if ($regionById[$homeRegion].sortNation -ne $sortNationByCode[$code]) {
            throw "Army '$id' home region '$homeRegion' belongs to '$($regionById[$homeRegion].sortNation)', not '$($sortNationByCode[$code])'."
        }
        if ($deployment -notin @('Standard', 'Naval')) {
            throw "Army '$id' has unsupported deployment type '$deployment'."
        }
        if ($strength -le 0 -or $strength -gt 1) {
            throw "Army '$id' has invalid starting strength '$strength'."
        }
        $resolved.Add([pscustomobject]@{
            Id = $id
            Code = $code
            Deployment = $deployment
            Strength = $strength
        })
    }

    $expectedTotal = 0
    $expectedNavies = 0
    $expectedEffective = 0.0
    foreach ($entry in $Expected.GetEnumerator()) {
        $code = [string]$entry.Key
        $expectation = $entry.Value
        $country = @($resolved | Where-Object Code -eq $code)
        $navies = @($country | Where-Object Deployment -eq 'Naval').Count
        $effective = ($country | Measure-Object Strength -Sum).Sum
        if ($country.Count -ne $expectation.Armies -or $navies -ne $expectation.Navies -or
            [Math]::Abs($effective - $expectation.Armies * $expectation.Strength) -gt 0.000001) {
            throw "'$MetaName' $code resolves to $($country.Count) armies/$navies navies/$effective effective; expected $($expectation.Armies)/$($expectation.Navies)/$($expectation.Armies * $expectation.Strength)."
        }
        if ($country.Count -lt $navies) {
            throw "'$MetaName' $code violates armies >= navies."
        }
        $expectedTotal += $expectation.Armies
        $expectedNavies += $expectation.Navies
        $expectedEffective += $expectation.Armies * $expectation.Strength
    }
    if ($resolved.Count -ne $expectedTotal) {
        throw "'$MetaName' has $($resolved.Count) armies instead of $expectedTotal."
    }
    Write-Host "PASS: $MetaName resolves to $expectedTotal armies, $expectedNavies navies, and $expectedEffective effective strength."
}

Test-Scenario 'ModernArmies' '' $expected2022
Test-Scenario '2026_Armies' '2026_' $expected2026

$selectedIds = @($metaOverrides.templateNames)
$orphanOverrides = @($overrides | Where-Object dataName -notin $selectedIds)
if ($orphanOverrides.Count -gt 0) {
    throw "Army overrides are not selected by either scenario: $($orphanOverrides.dataName -join ', ')."
}

Write-Host 'PASS: starting-force JSON, regions, scenario membership, strength, and navy floors validated.'
