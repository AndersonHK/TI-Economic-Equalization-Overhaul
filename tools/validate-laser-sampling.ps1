[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$RepositoryRoot)
$ErrorActionPreference='Stop'
$config=Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles\Config'
$manifest=Get-Content (Join-Path $config 'laser-ribbon-manifest.eeo') -Raw | ConvertFrom-Json
if ($manifest.schema -ne 3 -or
    $manifest.unityVersion -ne '2020.3.49f1' -or $manifest.graphicsApi -ne 'Direct3D11' -or
    $manifest.shaderName -ne 'EEO/Beam Weapon Line' -or -not $manifest.nativeVertexUnchanged -or
    $manifest.vertexSha256 -ne '26EDD8DC65AF5CD30FA35FCFFEBE7F08E59A39FF3144E08DD66B0DCA3AA0D13F' -or
    $manifest.programs[1].sha256 -ne '1F8EF95499908B3BD816C8FBC8FB3A0E80D58EDD2EBA72562DA3A0DE773FC7A2') { throw 'Unsupported laser sampling asset manifest.' }
$shaderPath=Join-Path $RepositoryRoot 'TIEconomyMod\Shaders\BeamRibbon.hlsl'
if ((Get-FileHash $shaderPath).Hash -ne $manifest.hlslSha256 -or
    (Get-FileHash (Join-Path $config 'laser-ribbon.eeo')).Hash -ne $manifest.bundleSha256 -or
    (Get-FileHash (Join-Path $config 'laser-aperture.eeo')).Hash -ne $manifest.aperture.bundleSha256) {
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
    'if (h <= 0.25) return pointCore', 'smoothstep(0.25, 0.5, h)',
    'exp2(mad(CoreMuzzleLogSlope, exponent, CoreMuzzleLogOffset))',
    'lerp(muzzleRadius, _LaserStartRadius + _LaserMaxRadius',
    'offset *= FlowUvScale / gain', 'min(abs(offset), radius) * _NoiseOpacity'
)) {
    if (-not $source.Contains($contract)) { throw "Laser flow/sampling contract changed: $contract" }
}
if ($source -match '\bdiscard\b|clip\(' -or $source.Contains('Shapes')) { throw 'Unexpected beam clipping/replacement.' }
$code=Get-Content (Join-Path $RepositoryRoot 'TIEconomyMod\Core\LaserBeamGeometry.cs') -Raw
if (-not $code.Contains('LaserSamplingMath.RibbonWidth(diameter, carrierRatio)') -or
    -not $code.Contains('LaserSamplingMath.ParticleRibbonWidth(originalMultiplier') -or
    -not $code.Contains('TryResolve(native, particle') -or
    -not $code.Contains('beamMaterial.shader=originalShader')) { throw 'Laser sizing/restoration contract changed.' }
if ($source.Contains('EEOBeam') -or $source.Contains('halfCarrier') -or $code.Contains('SetVector(')) { throw 'Rejected reconstructed/padded vertex path remains.' }

$lens = $manifest.aperture
$evidencePath = Join-Path $RepositoryRoot 'docs\ship-balance-research\battle-scale\laser-aperture-calibration-2026-11-22.json'
$evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
$common = @($lens.shaders | Where-Object nativeShader -eq 'TerraInvicta/Beam Weapon Line')
$flowVariant = @($lens.shaders | Where-Object nativeShader -eq 'TerraInvicta/Laser Beam No Middle')
$profiles = @($evidence.profiles.PSObject.Properties)
if ($common.Count -ne 1 -or $flowVariant.Count -ne 1 -or $profiles.Count -ne 4 -or
    $evidence.nativeBundleSha256 -ne $manifest.nativeBundleSha256 -or
    @($lens.shaders | Where-Object vertexSha256 -ne $manifest.vertexSha256).Count -ne 0 -or
    @($evidence.effects | Where-Object optical).Count -ne 14 -or
    @($evidence.effects | Where-Object { -not $_.optical }).Count -ne 6) {
    throw 'Incomplete optical or particle effect inventory.'
}

function Get-ShaderNumber([string]$name) {
    $match = [regex]::Match($source, 'static const float ' + [regex]::Escape($name) + ' = (-?[0-9.]+);')
    if (-not $match.Success) { throw "Missing baked beam coefficient: $name" }
    [double]::Parse($match.Groups[1].Value, [Globalization.CultureInfo]::InvariantCulture)
}

$legacy = .46
$target = .80
$level = $evidence.referenceContour
if ([Math]::Abs($level - [Math]::Exp(-2)) -gt 1e-12) { throw 'Unexpected optical emission contour.' }
$slope = Get-ShaderNumber 'CoreMuzzleLogSlope'
$offset = Get-ShaderNumber 'CoreMuzzleLogOffset'
$flowGain = Get-ShaderNumber 'FlowMuzzleGain'
$flowUvScale = Get-ShaderNumber 'FlowUvScale'
$flowFadeEnd = Get-ShaderNumber 'FlowFadeEnd'
$checks = 0
foreach ($item in $profiles) {
    $p = $item.Value.parameters
    $distanceStart = [Math]::Pow(.01, $p._LaserDistanceFalloff)
    $nativeEnd = $p._LaserStartRadius + $p._LaserMaxRadius
    foreach ($pulseStep in -16..16) {
        $exponent = $p._LaserFalloff * (1 + $pulseStep / 16.0 * $p._NoiseScaleStrength)
        $muzzle = [Math]::Pow(2, $slope * $exponent + $offset)
        $width = 2 * [Math]::Pow((1 - $level) * $muzzle, 1 / $exponent) / $legacy
        if ([Math]::Abs($width - $target) -gt 1e-10) { throw 'Muzzle optical contour misses the target width.' }
        foreach ($shaft in 1..100) {
            $u = $shaft / 100.0
            $s = [Math]::Pow($u, $p._LaserDistanceFalloff)
            $r = $muzzle + ($nativeEnd - $muzzle) * ($s - $distanceStart)/(1 - $distanceStart)
            if ([Math]::Pow($r, 1 / $exponent) -gt .5 * $legacy / $common[0].carrierRatio -or
                ($shaft -eq 100 -and [Math]::Abs($r - $nativeEnd) -gt 1e-10)) {
                throw "Core clipped or downstream width changed for $($item.Name)."
            }
            $checks++
        }
    }
}
$flow = $evidence.flow
if ($common[0].carrierRatio -ne $legacy -or $flow.carrierRatio -ne $flowVariant[0].carrierRatio -or
    $flow.parameters._LaserOpacity -ne 0 -or
    [Math]::Abs($flowUvScale - $legacy / $flow.carrierRatio) -gt 1e-10 -or
    [Math]::Abs($flow.opticalWidth * $flowGain / $legacy - $target) -gt 1e-10 -or
    $flowFadeEnd -ne $flow.fadeEnd) {
    throw 'No Middle must keep its calibrated carrier and hollow core.'
}
foreach ($model in $flow.phaseWidths.PSObject.Properties.Value) {
    if ($model.min * $flowGain / $legacy -lt .94 * $target -or $model.max * $flowGain / $legacy -gt 1.06 * $target) {
        throw 'Texture-backed optical calibration misses its target over the sampled pulse.'
    }
}
$p = $flow.parameters
$tail = (1 - [Math]::Pow($flow.retainedTailThreshold, 1 / $p._NoiseExponent)) / 2
foreach ($shaft in 10..1000) {
    $u = $shaft / 1000.0
    $t = [Math]::Min(1, ($u - .01)/($flowFadeEnd - .01))
    $gain = 1 + ($flowGain - 1) * (1 - $t*$t*(3 - 2*$t))
    $r = $p._LaserStartRadius + $p._LaserMaxRadius * [Math]::Pow($u, $p._LaserDistanceFalloff)
    if (($r + $tail) * $gain -gt .5 * $legacy / $flow.carrierRatio) {
        throw 'Flow carrier cuts into the verified bright lobe or its 1% tail.'
    }
    $checks++
}
Write-Host "PASS: $checks core and flow samples; 14 calibrated laser effects; six native particle effects; native vertices, legacy particle fragment and scrolling texture contracts."
