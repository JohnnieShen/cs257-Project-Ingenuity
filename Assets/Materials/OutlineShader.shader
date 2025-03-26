Shader "Hidden/Edge Detection"
{
    Properties
    {
        _OutlineThickness ("Outline Thickness", Float) = 1
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Opaque"
        }

        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass 
        {
            Name "EDGE DETECTION OUTLINE"
            
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl" // needed to sample scene depth
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl" // needed to sample scene normals
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl" // needed to sample scene color/luminance

            float _OutlineThickness;
            float4 _OutlineColor;

            #pragma vertex Vert // vertex shader is provided by the Blit.hlsl include
            #pragma fragment frag

            // Edge detection kernel that works by taking the sum of the squares of the differences between diagonally adjacent pixels (Roberts Cross).
            float RobertsCross(float3 samples[4])
            {
                float3 d1 = samples[1] - samples[2];
                float3 d2 = samples[0] - samples[3];
                return sqrt(dot(d1, d1) + dot(d2, d2));
            }

            // The same kernel logic as above, but for a single-value instead of a vector3.
            float RobertsCross(float samples[4])
            {
                float d1 = samples[1] - samples[2];
                float d2 = samples[0] - samples[3];
                return sqrt(d1 * d1 + d2 * d2);
            }
            
            // Helper function to sample scene normals remapped from [-1, 1] range to [0, 1].
            float3 SampleSceneNormalsRemapped(float2 uv)
            {
                return SampleSceneNormals(uv) * 0.5 + 0.5;
            }

            // Helper function to sample scene luminance.
            float SampleSceneLuminance(float2 uv)
            {
                float3 color = SampleSceneColor(uv);
                return dot(color, float3(0.3, 0.59, 0.11));
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                // Screen-space coordinates which we will use to sample.
                float2 uv = IN.texcoord;
                
                #if defined(UNITY_UV_STARTS_AT_TOP)
                    uv.y = 1.0 - uv.y;
                #endif

                float2 texelSize = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);
                
                float half_width = _OutlineThickness * 0.5;
                
                float2 uvs[4];
                uvs[0] = uv + texelSize * float2(-half_width,  half_width);
                uvs[1] = uv + texelSize * float2( half_width,  half_width);
                uvs[2] = uv + texelSize * float2(-half_width, -half_width);
                uvs[3] = uv + texelSize * float2( half_width, -half_width);
                
                float3 normalSamples[4];
                float depthSamples[4];
                float luminanceSamples[4];

                for (int i = 0; i < 4; i++)
                {
                    depthSamples[i]      = SampleSceneDepth(uvs[i]);
                    normalSamples[i]     = SampleSceneNormalsRemapped(uvs[i]);
                    luminanceSamples[i]  = SampleSceneLuminance(uvs[i]);
                }
                
                // Apply edge detection kernel on the samples to compute edges.
                float edgeDepth      = RobertsCross(depthSamples);
                float edgeNormal     = RobertsCross(normalSamples);
                float edgeLuminance  = RobertsCross(luminanceSamples);
                
                // Threshold the edges (discontinuity must be above certain threshold to be counted as an edge). The sensitivities are hardcoded here.
                float depthThreshold      = 1.0 / 200.0;
                float normalThreshold     = 1.0 / 4.0;
                float luminanceThreshold  = 1.0 / 0.5;

                edgeDepth     = (edgeDepth     > depthThreshold)     ? 1 : 0;
                edgeNormal    = (edgeNormal    > normalThreshold)    ? 1 : 0;
                edgeLuminance = (edgeLuminance > luminanceThreshold) ? 1 : 0;
                
                // Combine the edges from depth/normals/luminance using the max operator.
                float edge = max(edgeDepth, max(edgeNormal, edgeLuminance));
                
                // Color the edge with a custom color.
                return edge * _OutlineColor;
            }
            ENDHLSL
        }
    }
}