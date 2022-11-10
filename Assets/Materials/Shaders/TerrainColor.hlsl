#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

const static float epsilon = 1E-8;

float InverseLerp(float a, float b, float value){
    return saturate((value-a)/(b-a));
}

void HeightColor_float(float3 position, float3 meshNormal, float minHeight, float maxheight, float4x4 heightBlend, float4x4 colors0123, float4x4 colors4567, float4x4 texScale, UnityTexture2D texSnow, UnityTexture2D texStone, UnityTexture2D texStonyGrass, UnityTexture2D texGrass, UnityTexture2D texSand, UnitySamplerState samplerState, out float3 terrainColor){
    float heightPercent = InverseLerp(minHeight, maxheight, position.y);
    UnityTexture2D textures[8];
    


    terrainColor = float3(0,0,0);
    for (int i = 0; i < 4; i++){
        float drawStrength = InverseLerp(-heightBlend[i%4][2+i/4]/2 + epsilon, heightBlend[i%4][2+i/4]/2 + epsilon, heightPercent - heightBlend[i%4][i/4]);//heightBlend[i/4, i%4], heightBlend[i+8], heightPercent - heightBlend[i]);
        terrainColor = terrainColor * (1-drawStrength) + colors0123[i].xyz * drawStrength;
    }
    for (int j = 4; j < 8; j++){
        float drawStrength = InverseLerp(-heightBlend[j%4][2+j/4]/2 + epsilon, heightBlend[j%4][2+j/4]/2 + epsilon, heightPercent - heightBlend[j%4][j/4]);//heightBlend[i/4, i%4], heightBlend[i+8], heightPercent - heightBlend[i]);
        terrainColor = terrainColor * (1-drawStrength) + colors4567[j-4].xyz * drawStrength;
    }
    //terrainColor = float3(heightBlend[0][0],heightBlend[1][0],heightBlend[2][0]);

    float3 scaledWorldPos = position/texScale[0][0];
    float3 blendAxes = abs(meshNormal);
    blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;
    float3 xProjection = SAMPLE_TEXTURE2D(texSand, samplerState, scaledWorldPos.yz) * blendAxes.x;
    float3 yProjection = SAMPLE_TEXTURE2D(texSand, samplerState, scaledWorldPos.xz) * blendAxes.y;
    float3 zProjection = SAMPLE_TEXTURE2D(texSand, samplerState, scaledWorldPos.xy) * blendAxes.z;

    terrainColor = xProjection + yProjection + zProjection;
}

#endif