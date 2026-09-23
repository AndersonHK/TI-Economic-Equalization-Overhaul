[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$TargetManagedDir,
    [Parameter(Mandatory = $true)]
    [string]$ModAssemblyPath
)

$ErrorActionPreference = 'Stop'

function Load-AssemblyBytes {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required national-space-asset validation assembly is missing: $Path"
    }
    return [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($Path))
}

$harmonyAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\0Harmony.dll')
foreach ($unityAssembly in Get-ChildItem `
    -LiteralPath $TargetManagedDir `
    -File `
    -Filter 'Unity*.dll') {
    [void](Load-AssemblyBytes $unityAssembly.FullName)
}
[void](Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'UnityModManager\UnityModManager.dll'))
$gameAssembly = Load-AssemblyBytes (
    Join-Path $TargetManagedDir 'Assembly-CSharp.dll')
[void](Load-AssemblyBytes (Join-Path $TargetManagedDir 'FMODUnity.dll'))
$modAssembly = Load-AssemblyBytes $ModAssemblyPath

$harmony = New-Object HarmonyLib.Harmony ('ti-eeo.space-assets.' + [Guid]::NewGuid().ToString('N'))
$names = @('NationalSpaceAssetUpkeepPatch', 'BoostCapPriorityPatch',
    'BoostCapFacilityPatch', 'BoostCapDirectInvestmentPatch', 'BoostCapEconomicChangePatch',
    'InvestmentTooltipPatch', 'NationalBoostTooltipPatch')
foreach ($name in $names) {
    $type = $modAssembly.GetType('TIEconomyMod.Patches.' + $name, $true)
    $patched = $harmony.CreateClassProcessor($type).Patch()
    if ($null -eq $patched -or $patched.Count -eq 0) {
        throw "Harmony failed to emit $name."
    }
}

# Like the other Unity UI validators, inspect the display contract and IL rather
# than detouring it: desktop CLR cannot JIT this native Unity UI method (ECall).
# The two string-only tooltip patches above still undergo full Harmony emission.
$controller = $gameAssembly.GetType('PavonisInteractive.TerraInvicta.NationInfoController', $true)
$target = $controller.GetMethod('UpdatePrimaryDisplayElements',
    [Reflection.BindingFlags]'NonPublic,Instance')
$displayPatch = $modAssembly.GetType('TIEconomyMod.Patches.NationalBoostValuePatch', $true)
$postfix = $displayPatch.GetMethod('Postfix')
$attribute = @($displayPatch.GetCustomAttributes($false) | Where-Object {
    $_ -is [HarmonyLib.HarmonyPatch]
})
if ($null -eq $target -or $target.ReturnType -ne [void] -or
    $target.GetParameters().Count -ne 0 -or
    $attribute.Count -ne 1 -or $attribute[0].info.declaringType -ne $controller -or
    $attribute[0].info.methodName -ne $target.Name -or
    $null -eq $postfix -or $postfix.GetParameters().Count -ne 1 -or
    $postfix.GetParameters()[0].ParameterType -ne $controller -or
    $postfix.GetParameters()[0].Name -ne '__instance' -or
    -not $postfix.IsDefined([HarmonyLib.HarmonyPostfix], $false)) {
    throw 'National Boost display patch no longer matches the native refresh contract.'
}
$reader = @([HarmonyLib.PatchProcessor].GetMethods() | Where-Object {
    $_.Name -eq 'GetOriginalInstructions' -and $_.GetParameters().Count -eq 2 -and
    -not $_.GetParameters()[1].ParameterType.IsByRef
})[0]
$instructions = @($reader.Invoke($null, [object[]]@($postfix, $null)))
$calls = @($instructions | Where-Object {
    $_.operand -is [Reflection.MethodInfo]
} | ForEach-Object { $_.operand.Name })
foreach ($required in @('get_nation', 'ShowCap', 'get_currentBoost_month',
    'CapMonth', 'FormatBigOrSmallNumber', 'get_text', 'StartsWith', 'Substring', 'SetText')) {
    if ($calls -notcontains $required) {
        throw "Boost display patch is missing required behavior: $required"
    }
}

$main = $modAssembly.GetType('TIEconomyMod.Main', $true)
$settings = [Activator]::CreateInstance($modAssembly.GetType('TIEconomyMod.Settings', $true))
$main.GetField('settings').SetValue($null, $settings)
$main.GetField('enabled').SetValue($null, $true)
$investment = $settings.investment
if ([Math]::Abs($investment.outputMultiplier - 1.1) -gt 0.00001 -or
    $investment.boostCapFactor -ne 2) { throw 'Incorrect approved defaults.' }

if ([Math]::Abs($investment.missionControlUpkeep - .05) -gt .00001 -or
    [Math]::Abs($investment.boostUpkeep - .1) -gt .00001 -or
    [Math]::Abs($investment.fundingUpkeep - .001) -gt .000001 -or
    -not $investment.spaceAssetUpkeepEnabled -or -not $investment.boostCapEnabled) {
    throw 'Incorrect Moderate upkeep defaults.'
}
# Real nation instances require initialized scenario templates in their static
# constructor. Validate Harmony binding here; pure boundary/unit tests run in
# FormulaTests, and save/load plus priority UI behavior is tested in-game.
Write-Host 'PASS: national space asset and tooltip patches emit, Boost display contract/IL validate, and approved defaults match.'
