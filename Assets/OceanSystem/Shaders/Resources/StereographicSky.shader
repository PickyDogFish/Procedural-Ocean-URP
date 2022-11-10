Shader "Hidden/Ocean/StereographicSky"
{
    Properties
    {
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "Stereographic Sky"

            HLSLPROGRAM
            #pragma vertex BasicFullscreenVert
            #pragma fragment StereographicSkyFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "FullscreenVert.hlsl"
            #include "../OceanGlobals.hlsl"

            half4 StereographicSkyFrag(Varyings input) : SV_Target
            {
                float2 xy = (input.uv - 0.5) * 2 * 1.1;
                float sqrs = dot(xy, xy);
                float3 dir = float3(2 * xy.x, 1 - sqrs, 2 * xy.y) / (1 + sqrs);
                float3 col = SampleOceanSpecCube(dir);
                float t = saturate((-dir.y + Ocean_ReflectionsMaskRadius) * Ocean_ReflectionsMaskSharpness);
                col = lerp(col, Ocean_ReflectionsMaskColor.rgb, t * Ocean_ReflectionsMaskColor.w);
                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}