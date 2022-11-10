// Pull in URP library functions and our own common functions
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

//Names must match properties exactly
float4 _ColorTint;
TEXTURE2D(_ColorMap); SAMPLER(sampler_ColorMap);
float4 _ColorMap_ST;
float _smoothness;

struct Attributes{
    float3 position : POSITION;
    float3 normalOS : NORMAL;
};

struct v2f{
    float4 positionCS : SV_POSITION;
};

float3 _LightDirection;

float4 GetShadowCasterPositionCS(float3 positionWS, float3 normalWS){
    float3 lightDirectionWS = _LightDirection;
    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
    #if UNITY_REVERSED_Z
        positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #else
        positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #endif
    return positionCS;
}

v2f vert(Attributes input){
    v2f output;
    VertexPositionInputs posInputs = GetVertexPositionInputs(input.position);
    VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

    output.positionCS = GetShadowCasterPositionCS(posInputs.positionWS, normInputs.normalWS);
    return output;
}

float4 frag(v2f input) : SV_TARGET{
    return 0;
}