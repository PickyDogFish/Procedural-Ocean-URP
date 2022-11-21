#include "OceanGlobals.hlsl"

half3 sampleCaustics(half2 uv, float colorSplit){
    half r = SAMPLE_TEXTURE2D(Ocean_CausticsTex, samplerOcean_CausticsTex, uv).r;
    half g = SAMPLE_TEXTURE2D(Ocean_CausticsTex, samplerOcean_CausticsTex, uv + half2(0, colorSplit)).r;
    half b = SAMPLE_TEXTURE2D(Ocean_CausticsTex, samplerOcean_CausticsTex, uv + half2(colorSplit, colorSplit)).r;

    return half3(r,g,b);
}


float3 AddCaustics(float3 positionWS, float3 sceneColor){
    //calculating caustics
    half2 uv = mul(positionWS, Ocean_MainLightDirection).xy;
    half2 uv1 = _Time * Ocean_TexPan1 + uv * Ocean_TexScale1;
    half2 uv2 = _Time * Ocean_TexPan2 + uv * Ocean_TexScale2;
    half3 caustics1 = sampleCaustics(uv1, Ocean_ColorSplit);
    half3 caustics2 = sampleCaustics(uv2, Ocean_ColorSplit);
    half luminanceMask = lerp(1, Luminance(sceneColor), Ocean_LuminanceMaskStrength);
    float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
    half light = MainLightRealtimeShadow(shadowCoord);
    half heightPercent = 1 - saturate(-positionWS.y / Ocean_Height);
    half topFade = saturate((Ocean_TopFade + positionWS.y)/Ocean_TopFade);

    //adding caustics to scene color
    return sceneColor + min(caustics1, caustics2) * light * (saturate(heightPercent - topFade));
}
