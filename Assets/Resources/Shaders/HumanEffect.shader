Shader "Unlit/HumanEffect"
{
    SubShader
    {
        Tags
        {
            "Queue" = "Geometry"
            "RenderType" = "Opaque"
            "ForceNoShadowCasting" = "True"
        }

        Pass
        {
            Cull Off
            ZTest Always
            ZWrite Off
            Lighting Off
            LOD 100
            Tags
            {
                "LightMode" = "Always"
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float3 position : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct fragment_output
            {
                float4 color : SV_Target;
            };

            CBUFFER_START(DisplayRotationPerFrame)
            sampler2D _YTex;
            float4 _YTex_ST;
            sampler2D _CbcrTex;
            float4 _CbcrTex_ST;
            sampler2D _StencilTex;
            float4 _StencilTex_ST;
            float4x4 _DisplayRotationPerFrame;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.position = UnityObjectToClipPos(v.position);
                o.uv = mul(float3(v.texcoord, 1.0f), _DisplayRotationPerFrame).xy;
                return o;
            }

            // yCbCr decoding
            float3 YCbCrToSRGB(float y, float2 cbcr)
            {
                float b = y + cbcr.x * 1.772 - 0.886;
                float r = y + cbcr.y * 1.402 - 0.701;
                float g = y + dot(cbcr, float2(-0.3441, -0.7141)) + 0.5291;
                return float3(r, g, b);
            }

            fragment_output frag (v2f i)
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // background
                float y = tex2D(_YTex, i.uv).x;
                float2 cbcr = tex2D(_CbcrTex, i.uv).xy;
                float4 background = float4(YCbCrToSRGB(y, cbcr), 1);

                // stencil
                float stencil = tex2D(_StencilTex, i.uv).r;

                // effect
                fragment_output o;
                o.color = lerp(background, float4(1, 0, 0, 1), stencil);
                return o;
            }
            ENDHLSL
        }
    }
}
