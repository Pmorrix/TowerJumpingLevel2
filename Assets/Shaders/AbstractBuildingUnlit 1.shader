Shader "Game/Buildings/BuildingHighTower Lit (URP, Windows)"
{
    Properties
    {
        _WallColor ("Wall Color", Color) = (0.1,0.1,0.1,1)
        _WindowOnColor ("Window On Color", Color) = (1,1,1,1)
        _WindowOffColor ("Window Off Color", Color) = (0,0,0,1)

        _WindowGrid ("Window Grid (X,Y)", Vector) = (10,15,0,0)
        _WindowPadding ("Window Padding", Range(0,0.45)) = 0.2
        _WindowRandomness ("Window Randomness", Range(0,1)) = 0.5
        _WindowEmission ("Window Emission", Range(0,5)) = 3.0
        _SideDarken ("Side Darken", Range(0,1)) = 0.4
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        // ---------- FORWARD PASS ----------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            float4 _WallColor;
            float4 _WindowOnColor;
            float4 _WindowOffColor;
            float4 _WindowGrid;
            float  _WindowPadding;
            float  _WindowRandomness;
            float  _WindowEmission;
            float  _SideDarken;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 facadeUV = float2(frac(i.worldPos.x), frac(i.worldPos.y));
                float2 gridUV = facadeUV * _WindowGrid.xy;
                float2 cellUV = frac(gridUV);
                float2 cellID = floor(gridUV);

                float windowMask =
                    step(_WindowPadding, cellUV.x) *
                    step(_WindowPadding, cellUV.y) *
                    step(_WindowPadding, 1.0 - cellUV.x) *
                    step(_WindowPadding, 1.0 - cellUV.y);

                float rnd = hash21(cellID);
                float windowOn = step(rnd, _WindowRandomness);

                float side = abs(dot(normalize(i.normal), float3(1,0,0)));
                float sideDark = lerp(1.0 - _SideDarken, 1.0, side);

                float3 baseColor =
                    lerp(
                        _WallColor.rgb,
                        lerp(_WindowOffColor.rgb, _WindowOnColor.rgb, windowOn),
                        windowMask
                    ) * sideDark;

                float3 emission =
                    _WindowOnColor.rgb *
                    windowMask *
                    windowOn *
                    _WindowEmission;

                return float4(baseColor + emission, 1.0);
            }
            ENDHLSL
        }

        // ---------- SHADOW CASTER PASS ----------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
