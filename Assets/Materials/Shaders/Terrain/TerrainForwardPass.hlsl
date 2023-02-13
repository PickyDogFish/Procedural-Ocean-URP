// Pull in URP library functions and our own common functions
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

const static int maxLayerCount = 8;
const static float epsilon = 1E-4;

int layerCount;
float3 tintColors[maxLayerCount];
float tintStrengths[maxLayerCount];
float startHeights[maxLayerCount];
float blendStrengths[maxLayerCount];
float textureScales[maxLayerCount];

float minHeight;
float maxHeight;

TEXTURE2D_ARRAY(textures);
SAMPLER(sampler_textures);


//Names must match properties exactly
TEXTURE2D(_ColorMap); SAMPLER(sampler_ColorMap);
float4 _ColorMap_ST;
float _smoothness;

struct Attributes{
    float3 position : POSITION;
    float2 uv : TEXCOORD0;
    float3 normalOS : NORMAL;
};

struct v2f{
    float3 positionOS : POSITION1;
    float4 positionCS : SV_POSITION;
    float2 uv: TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    //DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);
};

float InverseLerp(float a, float b, float value){
    return saturate((value-a)/(b-a));
}

float3 Triplanar(float3 posWS, float scale, float3 blendAxes, int textureIndex){
    float3 scaledPosWS = posWS/scale;
    float3 xProjection = SAMPLE_TEXTURE2D_ARRAY(textures, sampler_textures, scaledPosWS.yz, textureIndex) * blendAxes.x;
    float3 yProjection = SAMPLE_TEXTURE2D_ARRAY(textures, sampler_textures, scaledPosWS.xz, textureIndex) * blendAxes.y;
    float3 zProjection = SAMPLE_TEXTURE2D_ARRAY(textures, sampler_textures, scaledPosWS.xy, textureIndex) * blendAxes.z;

    return xProjection + yProjection + zProjection;
}

v2f vert(Attributes input){
    v2f output;
    VertexPositionInputs posInputs = GetVertexPositionInputs(input.position);
    VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

    output.positionCS = posInputs.positionCS;
    output.normalWS = normInputs.normalWS;
    //applies tiling and similar things
    output.uv = TRANSFORM_TEX(input.uv, _ColorMap);
    output.positionWS = posInputs.positionWS;
    output.positionOS = input.position;
    return output; 
}

float4 frag(v2f input) : SV_TARGET{
    float heightPercent = InverseLerp(minHeight, maxHeight, input.positionWS.y);

    float3 blendAxes = abs(input.normalWS);
    blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

    float3 terrainColor = float3(0,0,0);
    for (int i = 0; i < layerCount; i++){
        float drawStrength = InverseLerp(-blendStrengths[i]/2 - epsilon, blendStrengths[i]/2 , heightPercent - startHeights[i]);
        float3 baseColor = tintColors[i] * tintStrengths[i];
        float3 texColor = Triplanar(input.positionOS, textureScales[i], blendAxes, i) * (1 - tintStrengths[i]);
        terrainColor = terrainColor * (1-drawStrength) + (baseColor + texColor) * drawStrength;
    }




    //initializes all fields as 0
    InputData lightingInput = (InputData)0;
    //could maybe skip normalize
    lightingInput.normalWS = normalize(input.normalWS);
    lightingInput.positionWS = input.positionWS;
    lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);


    SurfaceData surfaceInput = (SurfaceData)0;
    surfaceInput.albedo = terrainColor;
    //surfaceInput.alpha = colorSample.a * _ColorTint.a;
    surfaceInput.specular = 1;
    surfaceInput.smoothness = _smoothness;

    //lightingInput.normalWS = NormalizeNormalPerPixel(input.normalWS);
    
    //lightingInput.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, lightingInput.normalWS);

    //return colorSample * _ColorTint;
    float4 colorPBR = UniversalFragmentPBR(lightingInput, surfaceInput);
    if (length(colorPBR) <= 0.05){
        terrainColor = terrainColor * 0.05;
        colorPBR = float4(terrainColor.x, terrainColor.y, terrainColor.z, 1);
    }
    return colorPBR;
}