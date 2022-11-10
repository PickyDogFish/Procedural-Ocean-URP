Shader "Custom/MyLitShadow"
{
    Properties
    {
        [Header(Surface options)]
        [MainTexture] _ColorMap("Color", 2D) = "white" {}
        [MainColor]_ColorTint("Tint", Color) = (1,1,1,1)
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

            #include "MyLitForwardPass.hlsl"
            ENDHLSL
        }
        Pass {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"} // Tells unity this is the main lighting pass

            ColorMask 0



            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "MyLitShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
