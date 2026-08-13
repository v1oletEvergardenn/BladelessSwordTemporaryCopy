Shader "UI/UIGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (0.56,0.82,1,1)
        _GlowStrength ("Glow Strength", Range(0, 10)) = 2
        _Color ("Color", Color) = (1,1,1,1) // This is set by the UI Image component
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
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
            float4 _GlowColor;
            float _GlowStrength;
            float4 _Color; // UI Image color

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Use UI Image's color for tint and alpha
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                float glow = smoothstep(0.0, 1.0, col.a) * _GlowStrength;
                col.rgb += _GlowColor.rgb * glow;
                return fixed4(col.rgb, col.a);
            }
            ENDCG
        }
    }
}