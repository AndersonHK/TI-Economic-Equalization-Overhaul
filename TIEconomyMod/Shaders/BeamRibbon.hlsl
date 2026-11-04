// Fragment-only correction. The bundle builder retains the native vertex
// program and its parameter layout byte-for-byte.
cbuffer Globals : register(b0)
{
    float4 _MainTex_ST : packoffset(c2);
    float4 _Color : packoffset(c3);
    float _LaserStartRadius : packoffset(c4.x);
    float _LaserMaxRadius : packoffset(c4.y);
    float _LaserFalloff : packoffset(c4.z);
    float _LaserDistanceFalloff : packoffset(c4.w);
    float _NoiseStart : packoffset(c5.x);
    float _NoiseTimescale : packoffset(c5.y);
    float _NoiseThreshold : packoffset(c5.z);
    float _NoiseOpacity : packoffset(c5.w);
    float _NoiseScaleStrength : packoffset(c6.x);
    float _NoiseScaleSpeed : packoffset(c6.y);
    float _NoiseExponent : packoffset(c6.z);
};
cbuffer Camera : register(b1) { float4 _Time : packoffset(c0); };
Texture2D _MainTex : register(t0);
SamplerState sampler_MainTex : register(s0);
struct BeamVarying
{
    float4 data : TEXCOORD0; // native transformed noise UV, along-beam u, transverse v
    float4 vertex : SV_POSITION;
};

float CorePrimitive(float coordinate, float exponent)
{
    float x = clamp(coordinate, -1, 1);
    return x - x * pow(abs(x), exponent) / (exponent + 1);
}

float NativeCoreWithSampling(float offset, float radius, float exponent, float halfUVFootprint)
{
    float pointCore = 1 - saturate(pow(abs(offset), exponent) / radius);
    float support = pow(max(radius, 1e-20), 1 / exponent);
    float h = halfUVFootprint / support;
    // Resolved native appearance is authoritative, including the flowing halo.
    if (h <= 0.25) return pointCore;
    float n = offset / support;
    float average = saturate((CorePrimitive(n + h, exponent) - CorePrimitive(n - h, exponent)) / (2 * h));
    float weight = smoothstep(0.25, 0.5, h);
    return lerp(pointCore, average, weight);
}

float3 Frag(BeamVarying input) : SV_Target
{
    // The original vertex program already applied _MainTex_ST in view space.
    float2 noiseUV = input.data.xy;
    noiseUV.y -= 0.15 * _NoiseTimescale * _Time.y;
    float noise = _MainTex.Sample(sampler_MainTex, noiseUV).r;
    float level = 0.5 + 0.1 * sin(31.4159265359 * _Time.y);
    float pulse = sin(3.14159265359 * _NoiseScaleSpeed * _Time.y) * _NoiseScaleStrength;
    float radius = _LaserStartRadius + _LaserMaxRadius * pow(max(input.data.z, 0.01), _LaserDistanceFalloff);
    float exponent = _LaserFalloff * (1 + pulse);
    float offset = input.data.w - 0.5;
    float core = NativeCoreWithSampling(offset, radius, exponent, 0.5 * fwidth(input.data.w));
    float distance = max(abs(offset) - radius, 0);
    float ramp = _NoiseStart > 0 ? saturate(input.data.z / _NoiseStart) : 1;
    float threshold = max(_NoiseThreshold * (1 + pulse), 1e-8);
    float mask = 1 - saturate(abs(noise - level) * distance * distance / (threshold * threshold * max(ramp, 1e-8)));
    float halo = _NoiseOpacity * pow(abs(1 - saturate(2 * distance)), _NoiseExponent) * ramp * ramp * mask;
    return (core + halo) * _Color.rgb * _Color.a;
}
