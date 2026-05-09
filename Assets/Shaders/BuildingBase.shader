Shader "Game/Buildings/Building Base Unlit (URP, Shadowed)"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.55, 0.35, 0.35, 1)
        _TopColor("Top Color", Color) = (0.80, 0.70, 0.70, 1)
        _SideTint("Side Tint (Multiplier)", Color) = (0.92, 0.92, 0.92, 1)

        _GradientStrength("Gradient Strength", Range(0,1)) = 0.55
        _RooftopHeight("Rooftop Height", Range(0,0.5)) = 0.14
        _RooftopSoftness("Rooftop Softness", Range(0.001,0.2)) = 0.02

        _SideDarken("Side Darken", Range(0,1)) = 0.18
        _InvertSide("Invert Side (0/1)", Float) = 0

        _VariationAmount("Variation Amount", Range(0,0.25)) = 0.05
        _VariationScale("Variation Scale", Range(0.05,5)) = 0.35
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }

        // ------------------------------------------------------------
        // Forward Unlit (tu look)
        // ------------------------------------------------------------
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 positionOS  : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _TopColor;
                float4 _SideTint;

                float _GradientStrength;
                float _RooftopHeight;
                float _RooftopSoftness;

                float _SideDarken;
                float _InvertSide;

                float _VariationAmount;
                float _VariationScale;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS  = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionOS  = IN.positionOS.xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Object-space normalized coords for Unity cube (-0.5..+0.5)
                float y01 = saturate(IN.positionOS.y + 0.5);
                float x01 = saturate(IN.positionOS.x + 0.5);

                // Vertical gradient
                float tGrad = saturate(y01 * _GradientStrength);
                float3 col = lerp(_BaseColor.rgb, _TopColor.rgb, tGrad);

                // Rooftop band
                float roofStart = 1.0 - _RooftopHeight;
                float roofRaw   = y01 - roofStart;
                float roofMask  = smoothstep(0.0, _RooftopSoftness, roofRaw);
                col = lerp(col, _TopColor.rgb, roofMask);

                // Fake side shading (2.5D)
                float sideT = (_InvertSide > 0.5) ? (1.0 - x01) : x01;
                float sideFactor = lerp(1.0, 1.0 - _SideDarken, sideT);
                col *= sideFactor * _SideTint.rgb;

                // Subtle variation by world X
                float s = sin(IN.positionWS.x * _VariationScale);
                float s01 = s * 0.5 + 0.5;
                float varFactor = lerp(1.0 - _VariationAmount, 1.0 + _VariationAmount, s01);
                col *= varFactor;

                return half4(col, 1.0);
            }
            ENDHLSL
        }

        // ------------------------------------------------------------
        // ShadowCaster pass (NECESARIO para proyectar sombras en URP)
        // ------------------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            Cull Back
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // ------------------------------------------------------------
        // DepthOnly pass (recomendado para compatibilidad/orden de render)
        // ------------------------------------------------------------
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }

            ZWrite On
            ZTest LEqual
            Cull Back
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }

    FallBack Off
}
