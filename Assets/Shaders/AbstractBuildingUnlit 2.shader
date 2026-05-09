Shader "Custom/AbstractBuildingUnlit_Color"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Building Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Patrón original (ventanas, ruido, etc.)
                fixed4 pattern = tex2D(_MainTex, i.uv);

                // 🔴 REEMPLAZO TOTAL DEL COLOR
                // El patrón solo modula alpha / intensidad si lo necesitas
                fixed4 finalColor;
                finalColor.rgb = _Color.rgb;
                finalColor.a   = pattern.a;

                return finalColor;
            }
            ENDHLSL
        }
    }
}
