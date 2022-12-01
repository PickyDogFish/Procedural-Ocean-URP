Shader "Ocean/WaterHeight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "../OceanSimulationSampling.hlsl"

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionOS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata input)
            {
                v2f o;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS);
                o.positionOS = posInputs.positionCS;
                o.positionWS = posInputs.positionWS;
                o.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);
                float waterHeight = (SampleHeight(i.positionWS.xz, 1, 1) + 1) / 4.0;
                float3 waterDisplacement = SampleDisplacement(i.positionWS.xz, 1, 1);
                //return float4(waterHeight,waterHeight,waterHeight,1);
                return float4(waterHeight.xxx ,1);
            }
            ENDHLSL
        }
    }
}
