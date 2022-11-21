Shader "Unlit/Caustic"
{
    Properties
    {
        _CausticsTex ("CausticsTexture", 2D) = "black" {}
        _TexScale1 ("CausticsScale", float) = 1.0
        _TexScale2 ("CausticsScale2", float) = 0.75
        _TexPan1 ("CausticsPan1", Vector) = (1.0, 0.0, 0.0, 0.0)
        _TexPan2 ("CausticsPan2", Vector) = (0.0, 1.0, 0.0, 0.0)
        _LuminanceMaskStrength ("LuminanceMaskStrength", float) = 0.25
        _ColorSplit ("ColorSplit", float) = 0.01
        _Height ("MaxDepth", float) = 50.0
        _TopFade ("TopFade", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "Caustics"

            Blend OneMinusDstColor One
            ZWrite On
            ZTest Always
            Cull Front

            HLSLPROGRAM

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT


            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"


            struct attributes
            {
                float4 positionOS : POSITION;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                //float3 positionWS : POSITION1;
            };

            TEXTURE2D(_CausticsTex);
            SAMPLER(sampler_CausticsTex);

            v2f vert (attributes input)
            {
                v2f output;
                //VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                //output.positionWS = posInputs.positionWS;
                return output;
            }

            half4x4 _MainLightDirection;
            float _TexScale1;
            float _TexScale2;
            float2 _TexPan1;
            float2 _TexPan2;
            float _LuminanceMaskStrength;
            float _ColorSplit;
            float _Height;
            float _TopFade;


            half3 sampleTexture(half2 uv, float colorSplit){
                half r = SAMPLE_TEXTURE2D(_CausticsTex, sampler_CausticsTex, uv).r;
                half g = SAMPLE_TEXTURE2D(_CausticsTex, sampler_CausticsTex, uv + half2(0, colorSplit)).r;
                half b = SAMPLE_TEXTURE2D(_CausticsTex, sampler_CausticsTex, uv + half2(colorSplit, colorSplit)).r;

                return half3(r,g,b);
            }

            half4 frag (v2f input) : SV_Target
            {
                float2 positionNDC = input.positionCS.xy / _ScaledScreenParams.xy;

                #if UNITY_REVERSED_Z
                    real depth = SampleSceneDepth(positionNDC);
                #else
                    real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(positionNDC))
                #endif

                float3 positionWS = ComputeWorldSpacePosition(positionNDC, depth, UNITY_MATRIX_I_VP);
                
                float3 positionOS = TransformWorldToObject(positionWS);
                half boundingBoxMask = all(step(positionOS, 0.5) * (1 - step(positionOS, -0.5)));

                half2 uv = mul(positionWS, _MainLightDirection).xy;

                half2 uv1 = _Time * _TexPan1 + uv * _TexScale1;
                half2 uv2 = _Time * _TexPan2 + uv * _TexScale2;

                half3 caustics1 = sampleTexture(uv1, _ColorSplit);
                half3 caustics2 = sampleTexture(uv2, _ColorSplit);

                // luminance mask
                half3 sceneColor = SampleSceneColor(positionNDC);
                half sceneLuminance = Luminance(sceneColor);
                half luminanceMask = lerp(1, sceneLuminance, _LuminanceMaskStrength);

                float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
                half light = MainLightRealtimeShadow(shadowCoord);
                
                half heightPercent = 1 - saturate(-positionWS.y / _Height);
                half topFade = saturate((_TopFade + positionWS.y)/_TopFade);

                return half4(min(caustics1, caustics2) * boundingBoxMask * light * (saturate(heightPercent - topFade)), 1);
                //return half4(half3(heightPercent, heightPercent, heightPercent) * boundingBoxMask, 1);
            }
            ENDHLSL
        }
    }
}
