[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$TargetManagedDir,
    [Parameter(Mandatory = $true)]
    [string]$ModAssemblyPath
)

$ErrorActionPreference = 'Stop'
$gameAssemblyPath = Join-Path $TargetManagedDir 'Assembly-CSharp.dll'
foreach ($assemblyPath in @($gameAssemblyPath, $ModAssemblyPath)) {
    if (-not (Test-Path -LiteralPath $assemblyPath)) {
        throw "Required campaign-difficulty validation assembly is missing: $assemblyPath"
    }
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
    'ti-eeo-campaign-difficulty-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $probeDirectory | Out-Null

function Read-MethodIl {
    param(
        [string]$AssemblyPath,
        [string]$TypeName,
        [string]$MethodName
    )

    $outputPath = Join-Path $probeDirectory ($TypeName + '.' + $MethodName + '.il')
    & $ildasm $AssemblyPath /text /nobar "/item:$TypeName::$MethodName" "/out:$outputPath"
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
    $target = Read-MethodIl `
        -AssemblyPath $gameAssemblyPath `
        -TypeName 'StartMenuController' `
        -MethodName 'ResetCampaignDifficultyOptions'
    Assert-Count $target 'StartMenuController::realismCombatScaleToggle' 1 `
        'Vanilla campaign realistic-scale toggle'
    Assert-Count $target 'StartMenuController::realismCombatDVMovementToggle' 1 `
        'Vanilla campaign realistic-delta-V toggle'
    Assert-Count $target 'ldc\.i4\.2' 2 `
        'Vanilla Cinematic-and-Normal realism thresholds'
    Assert-Count $target 'StartMenuController::DisableCustomDifficulty\(\)' 1 `
        'Vanilla preset-status reset'

    $longCampaign = Read-MethodIl `
        -AssemblyPath $gameAssemblyPath `
        -TypeName 'StartMenuController' `
        -MethodName 'OnLaunchLongCampaignClicked'
    Assert-Count $longCampaign 'StartMenuController::SetDefaultCampaignOptions\(\)' 1 `
        'Long-campaign default reset'
    Assert-Count $longCampaign 'StartMenuController::OnLaunchCampaignClicked\(\)' 1 `
        'Long-campaign standard launch path'

    $defaultCampaign = Read-MethodIl `
        -AssemblyPath $gameAssemblyPath `
        -TypeName 'StartMenuController' `
        -MethodName 'SetDefaultCampaignOptions'
    Assert-Count $defaultCampaign 'StartMenuController::ResetAllCustomizations\(\)' 1 `
        'Default-campaign customization reset'

    $resetAll = Read-MethodIl `
        -AssemblyPath $gameAssemblyPath `
        -TypeName 'StartMenuController' `
        -MethodName 'ResetAllCustomizations'
    Assert-Count $resetAll 'StartMenuController::ResetCampaignDifficultyOptions\(\)' 1 `
        'All-customizations difficulty reset'

    $launchCampaign = Read-MethodIl `
        -AssemblyPath $gameAssemblyPath `
        -TypeName 'StartMenuController' `
        -MethodName 'OnLaunchCampaignClicked'
    Assert-Count $launchCampaign 'StartMenuController::selectDifficultyDropdown' 1 `
        'Campaign selected-difficulty read at launch'
    Assert-Count $launchCampaign 'GameControl::startupDifficulty' 1 `
        'Campaign one-based difficulty storage'
    Assert-Count $launchCampaign 'StartMenuController::SetCustomCampaignOptions\(bool\)' 1 `
        'Campaign customization storage at launch'

    $customCampaign = Read-MethodIl `
        -AssemblyPath $gameAssemblyPath `
        -TypeName 'StartMenuController' `
        -MethodName 'SetCustomCampaignOptions'
    Assert-Count $customCampaign 'StartMenuController::customDifficulty' 1 `
        'Campaign custom-difficulty status read'
    Assert-Count $customCampaign 'ScenarioCustomizations::customDifficulty' 1 `
        'Campaign custom-difficulty status storage'

    $factionWin = Read-MethodIl `
        -AssemblyPath $gameAssemblyPath `
        -TypeName 'PavonisInteractive.TerraInvicta.TINotificationQueueState' `
        -MethodName 'LogFactionWin'
    Assert-Count $factionWin 'ScenarioCustomizations::customDifficulty' 1 `
        'Win-achievement custom-difficulty exclusion'
    Assert-Count $factionWin 'TIGlobalValuesState::get_difficulty\(\)[\s\S]*?ldc\.i4\.2[\s\S]*?blt\.s[\s\S]*?ldstr\s+"normalWin"[\s\S]*?TIFactionState::UnlockAchievement\(string\)' 1 `
        'Normal win-achievement difficulty threshold and unlock'

    $patch = Read-MethodIl `
        -AssemblyPath $ModAssemblyPath `
        -TypeName 'TIEconomyMod.Patches.CampaignDifficultyRealismDefaultsPatch' `
        -MethodName 'Postfix'
    Assert-Count $patch 'CampaignDifficultyDefaults::EnableCombatRealism\(' 1 `
        'Campaign realism default helper call'
    Assert-Count $patch 'StartMenuController::selectDifficultyDropdown' 1 `
        'Campaign selected-difficulty read'
    Assert-Count $patch 'StartMenuController::realismCombatScaleToggle' 1 `
        'Campaign realistic-scale override'
    Assert-Count $patch 'StartMenuController::realismCombatDVMovementToggle' 1 `
        'Campaign realistic-delta-V override'
    Assert-Count $patch 'UnityEngine\.UI\.Toggle::SetIsOnWithoutNotify\(bool\)' 2 `
        'Campaign non-notifying toggle assignments'
}
finally {
    Remove-Item -LiteralPath $probeDirectory -Recurse -Force
}

Write-Host 'PASS: campaign presets retain combat realism only on Cinematic; Long Campaign preserves Normal achievement eligibility.'
