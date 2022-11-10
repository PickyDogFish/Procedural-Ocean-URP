Shader "Custom/TerrainHLSL"
{
    Properties
    {
        [Header(Surface options)]
        [MainTexture] _ColorMap("Color", 2D) = "white" {}
        _Smoothness("Smoothness", Float) = 0
    }
    SubShader
    {
        Tags {"RenderPipeline" = "UniversalPipeline"}
        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"} // Tells unity this is the main lighting pass

            HLSLPROGRAM

            #define _SPECULAR_COLOR8
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #pragma vertex vert
            #pragma fragment frag

            #include "TerrainForwardPass.hlsl"
            ENDHLSL
        }
        Pass {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"} // Tells unity this is the main lighting pass

            ColorMask 0

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "TerrainShadowCasterPass.hlsl"
            ENDHLSL
        }
        Pass {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "TerrainDepthPass.hlsl"

            ENDHLSL

        }
    }
}
