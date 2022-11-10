// Pull in URP library functions and our own common functions
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

//Names must match properties exactly
float4 _ColorTint;
TEXTURE2D(_ColorMap); SAMPLER(sampler_ColorMap);
float4 _ColorMap_ST;
float _smoothness;

struct Attributes{
    float3 position : POSITION;
    float2 uv : TEXCOORD0;
    float3 normalOS : NORMAL;
};

struct v2f{
    float4 positionCS : SV_POSITION;
    float2 uv: TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
};

v2f vert(Attributes input){
    v2f output;
    VertexPositionInputs posInputs = GetVertexPositionInputs(input.position);
    VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

    output.positionCS = posInputs.positionCS;
    output.normalWS = normInputs.normalWS;
    //applies tiling and similar things
    output.uv = TRANSFORM_TEX(input.uv, _ColorMap);
    output.positionWS = posInputs.positionWS;
    return output; 
}

float4 frag(v2f input) : SV_TARGET{
    float4 colorSample = SAMPLE_TEXTURE2D(_ColorMap, sampler_ColorMap, input.uv);

    //initializes all field as 0
    InputData lightingInput = (InputData)0;
    //could maybe skip normalize
    lightingInput.normalWS = normalize(input.normalWS);
    lightingInput.positionWS = input.positionWS;
    lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

    SurfaceData surfaceInput = (SurfaceData)0;

    surfaceInput.albedo = colorSample.rgb * _ColorTint.rgb;
    //surfaceInput.alpha = colorSample.a * _ColorTint.a;
    surfaceInput.specular = 1;
    surfaceInput.smoothness = _smoothness;
    

    //return colorSample * _ColorTint;
    return UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
}