Shader "Skull Crusher/Violenceable/Util/Violence Spread And Mesh Unwrapping"
{
    Properties
    {
        _Unwrap("Unwrap", Range(0.0, 1.0)) = 1.0
        _HeightMap("Height Map", 2D) = "clear" {}
        _BlendSmoothness("Blend Smoothness", Range(0.001, 1)) = 0.1
        _Contrast ("Contrast", Range(0.0, 10.0)) = 1.0
        _Brightness ("Brightness", Range(-5.0, 5.0)) = 0.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 localPosition: TEXCOORD1;
            };

            float _Unwrap;

            sampler2D _HeightMap;
            float4 _HeightMap_ST;

            float _BlendSmoothness;

            float3 _DamageLocalPosition;
            // float3 _DamageNormal;
            float _DamageRadius;
            float _DamageAmount;
            float _Contrast;
            float _Brightness;


            v2f vert(appdata v)
            {
                v2f o;

                float4 standard = UnityObjectToClipPos(v.vertex);
                float4 unwrapped = float4(2 * v.uv1.xy * float2(1, -1) + float2(-1, 1), 1, 1);
                unwrapped.y *= -_ProjectionParams.x;

                o.vertex = lerp(standard, unwrapped, _Unwrap);
                o.uv = v.uv;
                o.localPosition = v.vertex;
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed height_blend(float value1, float alpha1, float value2, float alpha2)
            {
                float maximum = max(value1 + alpha1, value2 + alpha2) - _BlendSmoothness;

                float m1 = max(value1 + alpha1 - maximum, 0);
                float m2 = max(value2 + alpha2 - maximum, 0);

                return (value1 * m1 + value2 * m2) / (m1 + m2);
            }


            float4 frag(v2f i) : SV_Target
            {
                float height = tex2D(_HeightMap, i.uv).a;
                float distance = length(_DamageLocalPosition - i.localPosition) * 0.5;

                float impact = (_DamageRadius - distance) / _DamageRadius;
                float corrected = _Contrast * (impact - 0.5) + 0.5 + _Brightness;
                float blended = height_blend(corrected, height * corrected, 0, 1 - height * corrected);
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                //return float4(contrasted, contrasted, contrasted, 1);
                return blended;
            }
            ENDCG
        }
    }
}