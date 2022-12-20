Shader "Ocean/WaterHeight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            //Cull Back
            //Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            //ZTest LEqual
            //ZWrite On

            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "../OceanSimulationSampling.hlsl"
            #include "../OceanGlobals.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float4 positionOS : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Samples height and calculates a normal from it
            float3 CalculateNormal(float3 positionWS, float3 delta)
            {
                float RMinusL = SampleHeight(positionWS.xz - delta.xy, 1, 1) - SampleHeight(positionWS.xz - delta.zy, 1, 1);
                float BMinusT = SampleHeight(positionWS.xz - delta.yx, 1, 1) - SampleHeight(positionWS.xz - delta.yz, 1, 1);
                
                return normalize(float3(2 * RMinusL, 2* BMinusT, -4)).xzy;
            }

            float CalculateLightAngle(float3 delta, float3 positionWS){
                
                float3 lightDir = _MainLightPosition.xyz;
                float3 normal = -CalculateNormal(positionWS, delta);
                //return float4(waterHeight,waterHeight,waterHeight,1);
                
                return 1.0 - 2.0*acos(dot(lightDir, normal)) / OCEAN_PI;
                //return normal;//(1-dot(lightDir, normal)) * 2.0;
            }

            v2f vert (appdata input)
            {
                v2f o;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                o.positionCS = posInputs.positionCS;
                o.positionWS = posInputs.positionWS;
                o.positionOS = input.positionOS;
                o.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 delta = float3(1, 0, -1) * 0.2;
                float waterHeight = SampleHeight(i.positionWS.xz, 1, 1);


                float2 uv = mul(i.positionWS, Ocean_MainLightDirection).xy;
                float fromSurface = -i.positionWS.y + waterHeight / 3.0;
                float angle = CalculateLightAngle(delta, i.positionWS);
                return float4(angle.xxx, 1);
            }
            ENDHLSL
        }
    }
}
