[CmdletBinding()]
param(
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
    $RepositoryRoot = Split-Path -Parent $scriptDirectory
}
$matrixPath = Join-Path $RepositoryRoot 'docs\current-implementation-matrix.xlsx'
$mainPath = Join-Path $RepositoryRoot 'TIEconomyMod\Main.cs'
$patchPath = Join-Path $RepositoryRoot 'TIEconomyMod\Patches'

$requiredColumns = @(
    'feature_id',
    'category',
    'feature',
    'vanilla_1_0_51',
    'maintained_main_0_2_5',
    'current_mod',
    'config_keys',
    'implementation_refs',
    'status',
    'notes'
)
$validStatuses = @('implemented', 'preserved', 'updated', 'vanilla', 'deferred')

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-ZipEntryText {
    param(
        [IO.Compression.ZipArchive]$Archive,
        [string]$Name
    )

    $entry = $Archive.GetEntry($Name)
    if ($null -eq $entry) {
        return $null
    }

    $stream = $entry.Open()
    $reader = New-Object IO.StreamReader($stream)
    try {
        return $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
        $stream.Dispose()
    }
}

function Get-CellColumnIndex {
    param([string]$Reference)

    $letters = [regex]::Match($Reference, '^[A-Z]+').Value
    $index = 0
    foreach ($character in $letters.ToCharArray()) {
        $index = $index * 26 + ([int]$character - [int][char]'A' + 1)
    }
    return $index - 1
}

function Import-FirstWorksheet {
    param([string]$Path)

    $archive = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $sharedStrings = @()
        $sharedText = Get-ZipEntryText -Archive $archive -Name 'xl/sharedStrings.xml'
        if ($sharedText) {
            [xml]$sharedXml = $sharedText
            foreach ($item in $sharedXml.SelectNodes("//*[local-name()='si']")) {
                $sharedStrings += (($item.SelectNodes(".//*[local-name()='t']") |
                    ForEach-Object { $_.InnerText }) -join '')
            }
        }

        $sheetText = Get-ZipEntryText -Archive $archive -Name 'xl/worksheets/sheet1.xml'
        if (-not $sheetText) {
            throw 'Implementation matrix workbook has no first worksheet.'
        }

        [xml]$sheetXml = $sheetText
        $matrixRows = @()
        foreach ($rowNode in $sheetXml.SelectNodes("//*[local-name()='sheetData']/*[local-name()='row']")) {
            $values = @{}
            foreach ($cell in $rowNode.SelectNodes("./*[local-name()='c']")) {
                $columnIndex = Get-CellColumnIndex -Reference $cell.GetAttribute('r')
                $cellType = $cell.GetAttribute('t')
                if ($cellType -eq 'inlineStr') {
                    $value = (($cell.SelectNodes(".//*[local-name()='t']") |
                        ForEach-Object { $_.InnerText }) -join '')
                }
                else {
                    $valueNode = $cell.SelectSingleNode("./*[local-name()='v']")
                    $value = if ($null -eq $valueNode) { '' } else { $valueNode.InnerText }
                    if ($cellType -eq 's' -and $value -ne '') {
                        $value = $sharedStrings[[int]$value]
                    }
                }
                $values[$columnIndex] = $value
            }
            $matrixRows += ,$values
        }

        if ($matrixRows.Count -lt 2) {
            return @()
        }

        $headerIndexes = @($matrixRows[0].Keys | Sort-Object)
        $headers = @($headerIndexes | ForEach-Object { [string]$matrixRows[0][$_] })
        $objects = @()
        foreach ($matrixRow in $matrixRows | Select-Object -Skip 1) {
            $record = [ordered]@{}
            for ($index = 0; $index -lt $headers.Count; $index++) {
                $record[$headers[$index]] = if ($matrixRow.ContainsKey($headerIndexes[$index])) {
                    [string]$matrixRow[$headerIndexes[$index]]
                }
                else {
                    ''
                }
            }
            $objects += [pscustomobject]$record
        }
        return $objects
    }
    finally {
        $archive.Dispose()
    }
}

$rows = @(Import-FirstWorksheet -Path $matrixPath)
if ($rows.Count -eq 0) {
    throw 'Implementation matrix has no feature rows.'
}

$actualColumns = @($rows[0].PSObject.Properties.Name)
foreach ($column in $requiredColumns) {
    if ($column -notin $actualColumns) {
        throw "Implementation matrix is missing required column '$column'."
    }
}

$duplicates = $rows | Group-Object feature_id | Where-Object Count -gt 1
if ($duplicates) {
    throw "Duplicate feature IDs: $($duplicates.Name -join ', ')"
}

$requiredLogisticsFeatures = @(
    'hab_logistics_cost',
    'hab_logistics_origins',
    'hab_logistics_founding',
    'hab_logistics_probes',
    'hab_logistics_cache_ui',
    'hab_logistics_ai')
foreach ($featureId in $requiredLogisticsFeatures) {
    if ($featureId -notin $rows.feature_id) {
        throw "Implementation matrix is missing Version 0.9 feature '$featureId'."
    }
}

foreach ($row in $rows) {
    if ([string]::IsNullOrWhiteSpace($row.feature_id)) {
        throw 'A feature row has an empty feature_id.'
    }
    if ($row.status -notin $validStatuses) {
        throw "Feature '$($row.feature_id)' has invalid status '$($row.status)'."
    }
}

$mainSource = Get-Content -LiteralPath $mainPath -Raw
$groupMatches = [regex]::Matches(
    $mainSource,
    'public\s+\w+Settings\s+(?<name>\w+)\s*=\s*new\s+\w+Settings')
$settingGroups = @($groupMatches | ForEach-Object { $_.Groups['name'].Value })
$fieldNames = @([regex]::Matches(
    $mainSource,
    'public\s+(?:bool|float)\s+(?<name>\w+)\s*=') | ForEach-Object { $_.Groups['name'].Value })

foreach ($row in $rows) {
    foreach ($key in @($row.config_keys -split ';' | Where-Object { $_ -and $_ -ne 'none' })) {
        $parts = $key.Split('.')
        if ($parts.Count -eq 1) {
            if ($parts[0] -ne 'enabled') {
                throw "Feature '$($row.feature_id)' references unknown global setting '$key'."
            }
            continue
        }
        if ($parts[0] -notin $settingGroups -or $parts[-1] -notin $fieldNames) {
            throw "Feature '$($row.feature_id)' references unknown configuration key '$key'."
        }
    }
}

foreach ($group in $settingGroups) {
    if (-not ($rows.config_keys | Where-Object { $_ -match "(^|;)$([regex]::Escape($group))\." })) {
        throw "Settings group '$group' is not represented in the implementation matrix."
    }
}

$patchSource = (Get-ChildItem -LiteralPath $patchPath -Filter '*.cs' |
    Get-Content -Raw) -join "`n"
$patchClasses = [regex]::Matches(
    $patchSource,
    '\[HarmonyPatch[\s\S]*?\]\s*public\s+static\s+class\s+(?<name>\w+)') |
    ForEach-Object { $_.Groups['name'].Value } |
    Sort-Object -Unique
$allReferences = $rows.implementation_refs -join ';'
foreach ($patchClass in $patchClasses) {
    if ($allReferences -notmatch "(^|[;:])$([regex]::Escape($patchClass))($|[;:])") {
        throw "Harmony patch '$patchClass' is not covered by the implementation matrix."
    }
}

Write-Host "PASS: implementation matrix validates ($($rows.Count) rows; $($settingGroups.Count) settings groups; $($patchClasses.Count) Harmony patches)."
