[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$RepositoryRoot)
$ErrorActionPreference='Stop'
$config=Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\Config'
$manifest=Get-Content (Join-Path $config 'laser-ribbon-manifest.eeo') -Raw | ConvertFrom-Json
if ($manifest.schema -ne 1 -or $manifest.coreRibbonRatio -ne .46 -or
    $manifest.unityVersion -ne '2020.3.49f1' -or $manifest.graphicsApi -ne 'Direct3D11' -or
    $manifest.shaderName -ne 'EEO/Beam Weapon Line' -or -not $manifest.nativeVertexUnchanged -or
    $manifest.vertexSha256 -ne '26EDD8DC65AF5CD30FA35FCFFEBE7F08E59A39FF3144E08DD66B0DCA3AA0D13F') { throw 'Unsupported laser sampling asset manifest.' }
$shaderPath=Join-Path $RepositoryRoot 'TIEconomyMod\Shaders\BeamRibbon.hlsl'
if ((Get-FileHash $shaderPath).Hash -ne $manifest.hlslSha256 -or
    (Get-FileHash (Join-Path $config 'laser-ribbon.eeo')).Hash -ne $manifest.bundleSha256) {
    throw 'Laser shader source or asset changed without rebuilding its guarded manifest.'
}
$source=Get-Content $shaderPath -Raw
foreach($contract in @(
    'noiseUV = input.data.xy',
    'noiseUV.y -= 0.15 * _NoiseTimescale * _Time.y',
    '_MainTex.Sample(sampler_MainTex, noiseUV).r',
    '0.5 + 0.1 * sin(31.4159265359 * _Time.y)',
    'sin(3.14159265359 * _NoiseScaleSpeed * _Time.y) * _NoiseScaleStrength',
    '_NoiseThreshold * (1 + pulse)',
    '_NoiseOpacity * pow(abs(1 - saturate(2 * distance)), _NoiseExponent)',
    'fwidth(input.data.w)', 'CorePrimitive', '_LaserFalloff * (1 + pulse)',
    'if (h <= 0.25) return pointCore', 'smoothstep(0.25, 0.5, h)'
)) {
    if (-not $source.Contains($contract)) { throw "Laser flow/sampling contract changed: $contract" }
}
if ($source -match '\bdiscard\b|clip\(' -or $source.Contains('Shapes')) { throw 'Unexpected beam clipping/replacement.' }
$code=Get-Content (Join-Path $RepositoryRoot 'TIEconomyMod\Core\LaserBeamGeometry.cs') -Raw
if (-not $code.Contains('LaserSamplingMath.RibbonWidth(diameter)') -or
    -not $code.Contains('LaserSamplingMath.ParticleRibbonWidth(originalMultiplier') -or
    -not $code.Contains('beamMaterial.shader=originalShader')) { throw 'Laser sizing/restoration contract changed.' }
if ($source.Contains('EEOBeam') -or $source.Contains('halfCarrier') -or $code.Contains('SetVector(')) { throw 'Rejected reconstructed/padded vertex path remains.' }
Write-Host 'PASS: fixed 46% ribbon calibration, native vertex identity, native resolved profile/scrolling flow, subpixel-only core filter and restoration contracts.'
