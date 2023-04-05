Shader "Hidden/UnderwaterSunShafts"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _  _MAIN_LIGHT_SHADOWS_CASCADE
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "Assets/OceanSystem/Shaders/OceanSimulationSampling.hlsl"

            //Boilerplate code, we aren't doind anything with our vertices or any other input info,
            // because technically we are working on a quad taking up the whole screen
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformWorldToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            //I set up these uniforms from the ScriptableRendererFeature
            float _Scattering;
            float _ScatteringDirection;
            float _Absorption;
            float _SchlickScattering;
            float _Steps;
            float _JitterVolumetric;
            float _MaxDistance;
            float _Threshold;
            
            float _DepthIntensity;


            float3 CalculateNormal(float3 positionWS, float3 delta)
            {

                float RMinusL = SampleHeight(positionWS.xz - delta.xy, 1, 1) - SampleHeight(positionWS.xz - delta.zy, 1, 1);
                float BMinusT = SampleHeight(positionWS.xz - delta.yx, 1, 1) - SampleHeight(positionWS.xz - delta.yz, 1, 1);
                
                return normalize(float3(2 * RMinusL, 2* BMinusT, -4)).xzy;
            }

            float LightNormalAngle(float3 delta, float3 positionWS)
            {
                float3 lightDir = _MainLightPosition.xyz;
                float3 normal = -CalculateNormal(positionWS, delta);

                //mapping the angle to the 0-1 range.
                return 1.0 - 2.0*acos(dot(lightDir, normal)) / 3.1415926535;
            }

            float WaveAten(float3 surfacePositionWS)
            {
                float3 delta = float3(1, 0, -1) * 0.1;
                float angle = LightNormalAngle(delta, surfacePositionWS);
                return angle;
            }
            






            //Unity already has a function that can reconstruct world space position from depth
            float3 GetWorldPos(float2 uv)
            {
                #if UNITY_REVERSED_Z
                    float depth = SampleSceneDepth(uv);
                #else
                    // Adjust z to match NDC for OpenGL
                    float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(uv));
                #endif
                return ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP);
            }

            // Mie scaterring approximated with Henyey-Greenstein phase function.
            float HGPhaseFunction(float lightDotView)
            {
                float result = 1.0f - _ScatteringDirection * _ScatteringDirection;
                result /= (4.0f * PI * pow(1.0f + _ScatteringDirection * _ScatteringDirection - (2.0f * _ScatteringDirection) * lightDotView, 1.5f));
                return result;
            }

            //standart hash
            float random( float2 p )
            {
                return frac(sin(dot(p, float2(41, 289)))*45758.5453 )-0.5; 
            }

            float random01( float2 p )
            {
                return frac(sin(dot(p, float2(41, 289)))*45758.5453 ); 
            }
            
            //from Ronja https://www.ronja-tutorials.com/post/047-invlerp_remap/
            float invLerp(float from, float to, float value)
            {
                return (value - from) / (to - from);
            }

            float remap(float origFrom, float origTo, float targetFrom, float targetTo, float value)
            {
                float rel = invLerp(origFrom, origTo, value);
                return lerp(targetFrom, targetTo, rel);
            }

            //this implementation is loosely based on http://www.alexandre-pestana.com/volumetric-lights/ and https://fr.slideshare.net/BenjaminGlatzel/volumetric-lighting-for-many-lights-in-lords-of-the-fallen

            // #define MIN_STEPS 25

            float3 frag (v2f i) : SV_Target
            {
                //first we get the world space position of every pixel on screen
                float3 worldPos = GetWorldPos(i.uv);

                //we find out our ray info, that depends on the distance to the camera
                float3 startPosition = _WorldSpaceCameraPos;
                float3 rayVector = worldPos - startPosition;
                float3 rayDirection =  normalize(rayVector);
                float rayLength = length(rayVector);

                rayLength = min(rayLength, _MaxDistance);
                worldPos = startPosition + rayDirection * rayLength;

                float stepLength = rayLength / _Steps;
                float3 step = rayDirection * stepLength;
                
                //to eliminate banding we sample at diffent depths for every ray, this way we obfuscate the shadowmap patterns
                float rayStartOffset = random01(i.uv) * stepLength * _JitterVolumetric;
                float3 currentPosition = startPosition + rayStartOffset * rayDirection;

                float accumFog = 0;

                float phase = HGPhaseFunction(dot(rayDirection, _MainLightPosition.xyz));

                //we ask for the shadow map value at different depths, if the sample is in light we compute the contribution at that point and add it
                for (float j = 0; j < _Steps - 1; j++)
                {
                    //getting the water surface position
                    float3 surfacePositionWS = currentPosition - currentPosition.y * _MainLightPosition.xyz;
                    half shadowValue = MainLightRealtimeShadow(TransformWorldToShadowCoord(currentPosition));
                    //the amount of light that enters the water
                    float lightValue = WaveAten(surfacePositionWS);

                    //if it is in light
                    if(shadowValue > _Threshold && lightValue > _Threshold)
                    {                       
                        float prepustnost = exp(-(_Scattering + _Absorption) * length(surfacePositionWS - currentPosition));
                        lightValue *= prepustnost;
                        float inscattering = _Scattering * lightValue * phase;
                        inscattering *= exp(-(_Scattering + _Absorption) * stepLength * j);
                        float waterHeight = SampleHeight(surfacePositionWS.xz, 1, 1);
                        if (currentPosition.y < waterHeight){
                            //brightness should follow exponential function. _DepthIntensity is user controlled.
                            //accumFog += pow(inscattering, (1 + (-currentPosition.y + waterHeight)/_DepthIntensity));
                            accumFog += inscattering;
                        }
                    }
                    currentPosition += step;
                }
                //we need the average value, so we divide between the amount of samples 
                accumFog /= _Steps;
                
                return accumFog;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Gaussian Blur x"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex =  TransformWorldToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            int _GaussSamples;
            float _GaussAmount;
            //bilateral blur from 
            static const float gauss_filter_weights[] = { 0.14446445, 0.13543542, 0.11153505, 0.08055309, 0.05087564, 0.02798160, 0.01332457, 0.00545096} ;         
            #define BLUR_DEPTH_FALLOFF 100.0

            float3 frag (v2f i) : SV_Target
            {
                float col =0;
                float accumResult =0;
                float accumWeights=0;
                //depth at the current pixel
                float depthCenter;  
                #if UNITY_REVERSED_Z
                    depthCenter = SampleSceneDepth(i.uv);  
                #else
                    // Adjust z to match NDC for OpenGL
                    depthCenter = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(i.uv));
                #endif

                for(float index=-_GaussSamples;index<=_GaussSamples;index++){
                    //we offset our uvs by a tiny amount 
                    float2 uv= i.uv+float2(  index*_GaussAmount/1000,0);
                    //sample the color at that location
                    float kernelSample = tex2D(_MainTex, uv);
                    //depth at the sampled pixel
                    float depthKernel;
                    #if UNITY_REVERSED_Z
                        depthKernel = SampleSceneDepth(uv);
                    #else
                        // Adjust z to match NDC for OpenGL
                        depthKernel = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(uv));
                    #endif
                    //weight calculation depending on distance and depth difference
                    float depthDiff = abs(depthKernel-depthCenter);
                    float r2= depthDiff*BLUR_DEPTH_FALLOFF;
                    float g = exp(-r2*r2);
                    float weight = g * gauss_filter_weights[abs(index)];
                    //sum for every iteration of the color and weight of this sample 
                    accumResult+=weight*kernelSample;
                    accumWeights+=weight;
                }
                //final color
                col= accumResult/accumWeights;

                return col;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Gaussian Blur y"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex =  TransformWorldToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            int _GaussSamples;
            float _GaussAmount;
            #define BLUR_DEPTH_FALLOFF 100.0
            static const float gauss_filter_weights[] = { 0.14446445, 0.13543542, 0.11153505, 0.08055309, 0.05087564, 0.02798160, 0.01332457, 0.00545096 } ;


            float3 frag (v2f i) : SV_Target
            {
                float col = 0;
                float accumResult = 0;
                float accumWeights = 0;
                
                if(_GaussAmount > 0){
                    for(float index = -_GaussSamples; index <= _GaussSamples; index ++){
                        float2 uv = i.uv + float2 (0, index * _GaussAmount / 1000);
                        float kernelSample = tex2D(_MainTex, uv);
                        float depthKernel;
                        float depthCenter;  
                        #if UNITY_REVERSED_Z
                            depthCenter = SampleSceneDepth(i.uv);
                            depthKernel = SampleSceneDepth(uv);
                        #else
                            // Adjust z to match NDC for OpenGL
                            depthCenter = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(i.uv));
                            depthKernel = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(uv));
                        #endif
                        float depthDiff = abs(depthKernel - depthCenter);
                        float r2 = depthDiff*BLUR_DEPTH_FALLOFF;
                        float g = exp(-r2 * r2);
                        float weight = g * gauss_filter_weights[abs(index)];
                        accumResult += weight * kernelSample;
                        accumWeights += weight;
                    }
                    col = accumResult / accumWeights;
                    
                }
                else{
                    col = tex2D(_MainTex,i.uv);
                }

                return col;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Compositing"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
       

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformWorldToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            sampler2D _MainTex;

            TEXTURE2D (_UnderwaterSunShaftsTexture);
            SAMPLER(sampler_UnderwaterSunShaftsTexture);
            TEXTURE2D  (_LowResDepth);
            SAMPLER(sampler_LowResDepth);

            float4 _SunMoonColor;
            float4 _Tint;
            float _Intensity;
            float _Downsample;

            float3 frag (v2f i) : SV_Target
            {
                half3 skyColor = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);

                _SunMoonColor = _MainLightColor * _Tint * float4(skyColor, 1);
                float col = 0;

                int offset =0;
                float d0 = SampleSceneDepth(i.uv);

                float d1 = _LowResDepth.Sample(sampler_LowResDepth, i.uv, int2(0, 1)).x;
                float d2 = _LowResDepth.Sample(sampler_LowResDepth, i.uv, int2(0, -1)).x;
                float d3 =_LowResDepth.Sample(sampler_LowResDepth, i.uv, int2(1, 0)).x;
                float d4 = _LowResDepth.Sample(sampler_LowResDepth, i.uv, int2(-1, 0)).x;

                d1 = abs(d0 - d1);
                d2 = abs(d0 - d2);
                d3 = abs(d0 - d3);
                d4 = abs(d0 - d4);

                float dmin = min(min(d1, d2), min(d3, d4));

                if (dmin == d1)
                offset = 0;

                else if (dmin == d2)
                offset = 1;

                else if (dmin == d3)
                offset = 2;

                else  if (dmin == d4)
                offset = 3;

                col = 0;
                switch(offset)
                {
                    case 0:
                        col = _UnderwaterSunShaftsTexture.Sample(sampler_UnderwaterSunShaftsTexture, i.uv, int2(0, 1));
                    break;
                    case 1:
                        col = _UnderwaterSunShaftsTexture.Sample(sampler_UnderwaterSunShaftsTexture, i.uv, int2(0, -1));
                    break;
                    case 2:
                        col = _UnderwaterSunShaftsTexture.Sample(sampler_UnderwaterSunShaftsTexture, i.uv, int2(1, 0));
                    break;
                    case 3:
                        col = _UnderwaterSunShaftsTexture.Sample(sampler_UnderwaterSunShaftsTexture, i.uv, int2(-1, 0));
                    break;
                    default:
                        col =  _UnderwaterSunShaftsTexture.Sample(sampler_UnderwaterSunShaftsTexture, i.uv);
                    break;
                }

                float3 screen = tex2D(_MainTex, i.uv);

                float3 finalShaft = saturate(col) * normalize(_SunMoonColor) * _Intensity;

                float3 finalColor = screen + finalShaft;

                return finalColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "SampleDepth"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformWorldToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float frag (v2f i) : SV_Target
            {
                #if UNITY_REVERSED_Z
                    float depth = SampleSceneDepth(i.uv);
                #else
                    // Adjust z to match NDC for OpenGL
                    float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(i.uv));
                #endif
                return depth;
            }
            ENDHLSL
        }
    }
}
