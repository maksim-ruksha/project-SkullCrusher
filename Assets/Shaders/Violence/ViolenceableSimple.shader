Shader "Skull Crusher/Violenceable/Simple"
{
    Properties
    {
        _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        _ViolenceTexture ("Violence Texture", 2D) = "red" {}
        _ViolenceMask ("Violence Mask", 2D) = "black" {}
        _ViolenceHeightMap ("Violence Height Map", 2D) = "black" {}


        _BlendSmoothness ("Blend Smoothness", Range(0.001, 1)) = 0.015
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        sampler2D _ViolenceMask;
        sampler2D _ViolenceTexture;
        sampler2D _ViolenceHeightMap;

        float _BlendSmoothness;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_ViolenceTexture;
            float2 uv_ViolenceMask;
        };

        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        fixed3 height_blend(float4 value1, float alpha1, float4 value2, float alpha2)
        {
            float4 maximum = max(value1.a + alpha1, value2.a + alpha2) - _BlendSmoothness;

            float m1 = max(value1.a + alpha1 - maximum, 0);
            float m2 = max(value2.a + alpha2 - maximum, 0);

            return (value1.rgb * m1 + value2.rgb * m2) / (m1 + m2);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 mainColor = tex2D(_MainTex, IN.uv_MainTex) * _Color;


            fixed4 violenceColor = tex2D(_ViolenceTexture, IN.uv_ViolenceTexture);
            fixed violenceAmount = tex2D(_ViolenceMask, IN.uv_ViolenceMask);

            float violenceHeight = tex2D(_ViolenceHeightMap, IN.uv_ViolenceTexture).a;

            fixed3 blended;
            if (violenceAmount < 0.02)
                blended = mainColor.rgb;
            else
                blended = height_blend(mainColor, (1 - violenceAmount), violenceColor,
                                       violenceHeight * violenceAmount);

            //o.Albedo = lerp(mainColor, violenceColor, violenceAmount);
            o.Albedo = float4(blended, 1);
            o.Alpha = mainColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}