Shader "Custom/PathDecal"
{
    Properties
    { 
    }

    // The SubShader block containing the Shader code.
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "Queue" = "AlphaTest" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }


        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            // This line defines the name of the vertex shader.
            #pragma vertex vert
            // This line defines the name of the fragment shader.
            #pragma fragment frag
            float _Radius;
            float _HeightInfluence;
            float4 _Segments[256];
            int _LinesCount;

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // The DeclareDepthTexture.hlsl file contains utilities for sampling the
            // Camera depth texture.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
            };

            // The vertex shader definition with properties defined in the Varyings
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN)
            {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings OUT;
                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous clip space.
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // Returning the output.
                return OUT;
            }

            // The fragment shader definition.
            // The Varyings input structure contains interpolated values from the
            // vertex shader. The fragment shader uses the `positionHCS` property
            // from the `Varyings` struct to get locations of pixels.
            half4 frag(Varyings IN) : SV_Target
            {
                // To calculate the UV coordinates for sampling the depth buffer,
                // divide the pixel location by the render target resolution
                // _ScaledScreenParams.
                float2 UV = IN.positionHCS.xy / _ScaledScreenParams.xy;

                // Sample the depth from the Camera depth texture.
                #if UNITY_REVERSED_Z
                    real depth = SampleSceneDepth(UV);
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
                #endif

                // Reconstruct the world space positions.
                float3 worldPos = ComputeWorldSpacePosition(UV, depth, UNITY_MATRIX_I_VP);
                bool inside = false;
                [loop]
                for(int i = 0; i < _LinesCount; i++) {
                    float3 segStart = _Segments[i].xyz;
                    segStart.y *= _HeightInfluence;
                    float3 segEnd = _Segments[i+1].xyz;
                    segEnd.y *= _HeightInfluence;
                    float3 worldPoint = worldPos.xyz;
                    worldPoint.y *= _HeightInfluence;


                    float3 nextSegment = segEnd;
                    float3 b = nextSegment - segStart;
                    float3 a = worldPoint - segStart;
                    float bLen = length(b);
                    float progress = (dot(a, b) / bLen);
                    if(progress < 0.0f) {
                        float startCapDist = length(a);
                        if(startCapDist > _Radius || i == 0){
                            continue;
                        }
                    }  
                    if(progress > bLen){
                        float endCapDist = length(worldPoint - segEnd);
                        if(endCapDist > _Radius ){
                            continue;
                        }
                        continue;
                    }
                    float dHdorff = length(a - progress * normalize(b));
                    if(dHdorff < _Radius){
                        inside = true;
                        break;
                    }
                }
                if(!inside)
                    discard;

                // The following part creates the checkerboard effect.
                // Scale is the inverse size of the squares.
                uint scale = 10;
                // Scale, mirror and snap the coordinates.
                uint3 worldIntPos = uint3(abs(worldPos.xyz * scale));
                // Divide the surface into squares. Calculate the color ID value.
                bool white = ((worldIntPos.x) & 1) ^ (worldIntPos.y & 1) ^ (worldIntPos.z & 1);
                // Color the square based on the ID value (black or white).
                half4 color = white ? half4(1,1,1,0.5f) : half4(0,0,0,0.5);

                // Set the color to black in the proximity to the far clipping
                // plane.
                #if UNITY_REVERSED_Z
                    // Case for platforms with REVERSED_Z, such as D3D.
                    if(depth < 0.0001)
                        discard;
                #else
                    // Case for platforms without REVERSED_Z, such as OpenGL.
                    if(depth > 0.9999)
                        discard;
                #endif
                
                return color;
            }
            ENDHLSL
        }
    }
}
