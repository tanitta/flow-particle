Shader "Custom/Particle" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader {
        Pass{
            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            /* #pragma geometry geom */
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            struct VertexData
            {
                float4 position;
                float4 color;
            };

            struct Input {
                float2 uv_MainTex;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                /* float2 uv  : TEXCOORD0; */
                float4 col : COLOR;
            };

            StructuredBuffer<VertexData> _vertices;

            half _Glossiness;
            half _Metallic;
            fixed4 _Color;


            v2f vert(uint id : SV_VertexID)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(_vertices[id].position);
                /* o.pos = float4(_particles[id].position, 1); */
                o.col = _vertices[id].color;
                return o;
            }

            // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            // #pragma instancing_options assumeuniformscalingj

            /* void geom(){} */

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.col;
                return col;
            }
            ENDCG
        }
    }
}

