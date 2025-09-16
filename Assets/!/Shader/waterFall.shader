Shader "Custom/waterFall"
{
   Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _DistortionStrength ("Distortion Strength", Range(0,0.2)) = 0.08
        _DistortionSpeed ("Distortion Speed", Range(0,5)) = 2
        _Alpha ("Alpha", Range(0,1)) = 0.7
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                float _DistortionStrength;
                float _DistortionSpeed;
                float _Alpha;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float2 WaterfallDistortUV(float2 uv, float time, float strength, float speed)
            {
                // Simple vertical distortion using sine wave
                float offset = sin(uv.y * 30 + time * speed) * strength;
                uv.x += offset;
                uv.y += time * speed * 0.1; // scroll down
                return uv;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float time = _Time.y;
                float2 uv = IN.uv;

                // Waterfall base texture
                half4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);

                // Screen UV for opaque texture
                float2 screenUV = IN.positionHCS.xy / IN.positionHCS.w;
                screenUV = screenUV * 0.5 + 0.5;

                // Distort the screen UV
                float2 distortedScreenUV = WaterfallDistortUV(screenUV, time, _DistortionStrength, _DistortionSpeed);

                // Sample the background using distorted screen UV
                half4 bgCol = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, distortedScreenUV);

                // Blend waterfall and distorted background
                half4 finalCol = lerp(bgCol, baseCol, baseCol.a);
                finalCol.a = baseCol.a * _Alpha;

                return finalCol;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward"
}
