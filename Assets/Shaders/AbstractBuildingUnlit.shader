Shader "Game/Buildings/AbstractBuildingUnlit_WindowsEmission"
{
    Properties
    {
        _MainTex ("Window Pattern", 2D) = "white" {}
        _Color ("Window Color", Color) = (1,1,1,1)
        _WallColor ("Wall Color", Color) = (0.12,0.12,0.12,1)
        _WindowEmission ("Window Emission", Range(0,5)) = 1.5
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Color;
            float4 _WallColor;
            float  _WindowEmission;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Patrón de ventanas
                fixed4 pattern = tex2D(_MainTex, i.uv);

                // Máscara de ventanas (zonas claras)
                float windowMask = dot(pattern.rgb, float3(0.333, 0.333, 0.333));
                windowMask = saturate(windowMask);

                // Color base del edificio (pared)
                fixed3 wall = _WallColor.rgb;

                // Color visible (pared + ventanas)
                fixed3 baseColor = lerp(wall, _Color.rgb, windowMask);

                // 🔥 Emisión SOLO en ventanas
                fixed3 emission = _Color.rgb * windowMask * _WindowEmission;

                // Salida final
                return fixed4(baseColor + emission, 1.0);
            }
            ENDHLSL
        }
    }
}
