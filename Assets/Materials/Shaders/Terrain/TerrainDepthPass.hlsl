#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

CBUFFER_START(UnityPerMaterial)
CBUFFER_END

struct VertexInput{
    float4 vertex : POSITION;                   
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput{           
    float4 vertex : SV_POSITION;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID           
    UNITY_VERTEX_OUTPUT_STEREO                 
};

VertexOutput vert(VertexInput v){
    VertexOutput o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.vertex = TransformObjectToHClip(v.vertex.xyz);

    return o;
}

half4 frag(VertexOutput IN) : SV_TARGET{       
    return 0;
}