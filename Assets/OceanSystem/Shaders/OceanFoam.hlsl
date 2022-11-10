﻿#if !defined(OCEAN_FOAM_INCLUDED)
#define OCEAN_FOAM_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "OceanGlobals.hlsl"
#include "OceanSimulationSampling.hlsl"
#include "OceanMaterialProps.hlsl"

struct FoamInput
{
	float4x4 derivatives;
	float2 worldUV;
    float viewDist;
	float4 lodWeights;
	float4 shoreWeights;
    float4 positionNDC;
    float viewDepth;
	float time;
	float3 viewDir;
	float3 normal;
};

struct FoamData
{
	float2 coverage;
	float3 normal;
	float3 albedo;
};

float2 RotateUV(float2 uv, float2 center, float2 rotation, float sgn)
{
    uv -= center;
    float s = rotation.y;
    float c = rotation.x;
    float2x2 rMatrix = float2x2(c, -sgn * s, sgn * s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;
    uv = mul(uv, rMatrix);
    uv += center;
    return uv;
}

float4 MixTurbulence(float4x4 t, float4 foamWeights, float4 mixWeights)
{
	return (t[0] * foamWeights.x + t[1] * foamWeights.y + t[2] * foamWeights.z + t[3] * foamWeights.w) / dot(foamWeights * mixWeights, 1);
}

float DeepFoam(float2 worldUV, float3 viewDir, float3 normal, float time)
{
	float2 parallaxDir = (viewDir.xz / (dot(normal, viewDir)) + 0.5 * normal.xz);
    float2 uv = worldUV - parallaxDir * _UnderwaterFoamParallax - Ocean_WindDirection * time;
    uv = TRANSFORM_TEX(uv, _FoamUnderwaterTexture);
    float value = SAMPLE_TEXTURE2D(_FoamUnderwaterTexture, sampler_FoamUnderwaterTexture, uv).r;
    return value;
}

float2 Coverage(float4x4 t, float4 mixWeights, float2 worldUV, float deepFoam, float bias)
{
	float4 turbulence = MixTurbulence(t, Ocean_FoamCascadesWeights, mixWeights * ACTIVE_CASCADES);
    float foamValueCurrent = lerp(turbulence.y, turbulence.x, Ocean_FoamSharpness);
    float foamValuePersistent = (turbulence.z + turbulence.w) * 0.5;
    foamValueCurrent = lerp(foamValueCurrent, foamValuePersistent, Ocean_FoamPersistence);
    foamValueCurrent -= 1;
    foamValuePersistent -= 1;
	
    float trailTexture;
	
    float trailTexture0 = SAMPLE_TEXTURE2D(_FoamTrailTexture, sampler_FoamTrailTexture,
		RotateUV(worldUV, 0, Ocean_FoamTrailDirection0, 1) / Ocean_FoamTrailTextureSize0).r;
    if (Ocean_FoamTrailBlendValue > 0)
    {
        float trailTexture1 = SAMPLE_TEXTURE2D(_FoamTrailTexture, sampler_FoamTrailTexture,
		RotateUV(worldUV, 0, Ocean_FoamTrailDirection1, 1) / Ocean_FoamTrailTextureSize1).r;
        trailTexture = lerp(trailTexture0, trailTexture1, Ocean_FoamTrailBlendValue);
    }   
	else
    {
        trailTexture = trailTexture0;
    }
	
    foamValuePersistent += saturate(foamValuePersistent + 1) * trailTexture * Ocean_FoamTrailTextureStrength;
    float foamValue = max((foamValuePersistent + Ocean_FoamTrail * (1 - bias)), foamValueCurrent + Ocean_FoamCoverage * (1 - bias));
	
    float surfaceFoam = saturate(foamValue * Ocean_FoamDensity);
    float shallowUnderwaterFoam = saturate((foamValue + 0.1 * Ocean_FoamUnderwater) * Ocean_FoamDensity);
    float deepUnderwaterFoam = deepFoam * saturate((foamValue + Ocean_FoamUnderwater * 0.25) * Ocean_FoamDensity * 0.8);
    return float2(surfaceFoam, max(shallowUnderwaterFoam, deepUnderwaterFoam));
}

float ContactFoam(float4 positionNDC, float viewDepth, float2 worldUV, float time)
{
    float depthDiff = LinearEyeDepth(SampleSceneDepth(positionNDC.xy / positionNDC.w), _ZBufferParams) 
		- viewDepth;
    float contactTexture = SAMPLE_TEXTURE2D(_ContactFoamTexture, sampler_ContactFoamTexture,
		TRANSFORM_TEX(worldUV, _ContactFoamTexture)).r;
	contactTexture = saturate(1 - contactTexture);
	depthDiff = abs(depthDiff) * contactTexture;
	return saturate(10 * (_ContactFoam * 2 - depthDiff));
}

FoamData GetFoamData(FoamInput i)
{
	FoamData data;
	data.coverage = 0;
	data.normal = float3(0, 1, 0);
	data.albedo = 1;
	
	#if !defined(WAVES_FOAM_ENABLED) && !defined(CONTACT_FOAM_ENABLED)
	return data;
	#endif
	
	#ifdef WAVES_FOAM_ENABLED
	float4x4 turbulence = SampleTurbulence(i.worldUV, i.lodWeights * i.shoreWeights);
	float deepFoam = DeepFoam(i.worldUV, i.viewDir, i.normal, i.time);
	float bias = SAMPLE_TEXTURE2D(_FoamDetailMap, sampler_FoamDetailMap,
		TRANSFORM_TEX(i.worldUV, _FoamDetailMap) * 0.01).r;
	bias *= saturate(i.viewDist / Ocean_LengthScales.x * 0.5);
	data.coverage = Coverage(turbulence, i.lodWeights, i.worldUV, deepFoam, bias);
	data.coverage *= 1 - saturate((_WorldSpaceCameraPos.y + i.viewDist * 0.5 - 2000) * 0.0005);
	float4 normalWeights = saturate(float4(1, 0.66, 0.33, 0) + _FoamNormalsDetail) * ACTIVE_CASCADES;
	data.normal = NormalFromDerivatives(i.derivatives, normalWeights);
	#endif
	
	#ifdef CONTACT_FOAM_ENABLED
	data.coverage.x = saturate(data.coverage.x + ContactFoam(i.positionNDC, i.viewDepth, i.worldUV, i.time));
	#endif
	
    float2 uv = TRANSFORM_TEX(i.worldUV, _FoamAlbedo);
    data.albedo = SAMPLE_TEXTURE2D(_FoamAlbedo, sampler_FoamAlbedo, uv).r;
	return data;
}

#endif