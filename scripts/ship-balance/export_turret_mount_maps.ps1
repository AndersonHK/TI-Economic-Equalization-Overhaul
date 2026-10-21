[CmdletBinding()]
param(
    [string]$GameInstallDir = 'D:\Games\SteamLibrary\steamapps\common\Terra Invicta',
    [string]$OutputPath
)
$ErrorActionPreference = 'Stop'
if (-not $OutputPath) {
    $OutputPath = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) '..\..\docs\ship-balance-research\turret-clearance\mount-maps.json'
}
$managed = Join-Path $GameInstallDir 'TerraInvicta_Data\Managed'
# Load bytes so research never locks installed assemblies. No scene, constructor,
# save, or mod patch is executed; the controller methods are integer switch maps.
foreach ($file in Get-ChildItem -LiteralPath $managed -Filter '*.dll' | Where-Object {
    $_.Name -like 'Unity*.dll' -or $_.Name -like 'Newtonsoft*.dll' -or $_.Name -eq 'FMODUnity.dll'
}) { [void][Reflection.Assembly]::Load([IO.File]::ReadAllBytes($file.FullName)) }
$assemblyPath = Join-Path $managed 'Assembly-CSharp.dll'
$assembly = [Reflection.Assembly]::Load([IO.File]::ReadAllBytes($assemblyPath))
$baseType = $assembly.GetType('PavonisInteractive.TerraInvicta.ShipModelController', $true)
$mountType = $baseType.GetMethod('SlotToWeaponMountIndex').GetParameters()[1].ParameterType
$hulls = Get-Content -Raw (Join-Path $GameInstallDir 'TerraInvicta_Data\StreamingAssets\Templates\TIShipHullTemplate.json') | ConvertFrom-Json
$maxSlot = ($hulls | ForEach-Object { $_.shipModuleSlots.Count } | Measure-Object -Maximum).Maximum + 16
$controllers = [ordered]@{}
try { $types = $assembly.GetTypes() }
catch [Reflection.ReflectionTypeLoadException] { $types = $_.Exception.Types | Where-Object { $null -ne $_ } }
foreach ($type in $types | Where-Object { $_.IsSubclassOf($baseType) -and -not $_.IsAbstract } | Sort-Object FullName) {
    $instance = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($type)
    $method = $type.GetMethod('SlotToWeaponMountIndex')
    $maps = [ordered]@{}
    foreach ($mount in @('OneHull', 'TwoHullHoriz', 'TwoHullVert', 'FourHull')) {
        $enumValue = [Enum]::Parse($mountType, $mount)
        $values = @(for ($slot = 0; $slot -lt $maxSlot; $slot++) {
            try { [int]$method.Invoke($instance, @([int]$slot, $enumValue)) }
            catch { $null } # Invalid slot paths can log through Unity; retained as unavailable.
        })
        $maps[$mount] = $values
    }
    $controllers[$type.FullName] = $maps
}
$result = [ordered]@{
    assembly_sha256 = (Get-FileHash -LiteralPath $assemblyPath -Algorithm SHA256).Hash
    assembly_version = $assembly.GetName().Version.ToString()
    slot_count = $maxSlot
    controllers = $controllers
}
$parent = Split-Path -Parent ([IO.Path]::GetFullPath($OutputPath))
[void][IO.Directory]::CreateDirectory($parent)
[IO.File]::WriteAllText([IO.Path]::GetFullPath($OutputPath), ($result | ConvertTo-Json -Depth 8 -Compress) + "`n", [Text.UTF8Encoding]::new($false))
Write-Output "Exported $($controllers.Count) native controller maps to $OutputPath"
