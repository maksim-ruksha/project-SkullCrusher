Shader "Skull Crusher/Violenceable/Multimasked"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BlendSmoothness ("Blend Smoothness", Range(0.001, 1)) = 0.015

        _ViolenceTexture1 ("Violence Texture 1", 2D) = "red" {}
        _ViolenceMask1 ("Violence Mask 1", 2D) = "black" {}
        _ViolenceHeightMap1 ("Violence Height Map 1", 2D) = "black" {}

        _ViolenceTexture2 ("Violence Texture 2", 2D) = "blue" {}
        _ViolenceMask2 ("Violence Mask 2", 2D) = "black" {}
        _ViolenceHeightMap2 ("Violence Height Map 2", 2D) = "black" {}
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

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_ViolenceTexture1;
            float2 uv_ViolenceTexture2;
        };

        sampler2D _ViolenceTexture1;
        sampler2D _ViolenceMask1;
        sampler2D _ViolenceHeightMap1;

        sampler2D _ViolenceTexture2;
        sampler2D _ViolenceMask2;
        sampler2D _ViolenceHeightMap2;

        fixed4 _Color;

        fixed _BlendSmoothness;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        fixed3 height_blend(float4 value1, float alpha1,
                            float4 value2, float alpha2,
                            float4 value3, float alpha3
        )
        {
            float4 maximum = max(max(value1.a + alpha1, value2.a + alpha2), value3.a + alpha3) -
                _BlendSmoothness;

            float m1 = max(value1.a + alpha1 - maximum, 0);
            float m2 = max(value2.a + alpha2 - maximum, 0);
            float m3 = max(value3.a + alpha3 - maximum, 0);

            return (value1.rgb * m1 + value2.rgb * m2 + value3.rgb * m3) / (m1 + m2 + m3);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 mainColor = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            fixed4 violence1 = tex2D(_ViolenceTexture1, IN.uv_ViolenceTexture1);
            float mask1 = tex2D(_ViolenceMask1, IN.uv_ViolenceTexture1);
            float height1 = tex2D(_ViolenceHeightMap1, IN.uv_ViolenceTexture1);

            fixed4 violence2 = tex2D(_ViolenceTexture2, IN.uv_ViolenceTexture2);
            float mask2 = tex2D(_ViolenceMask2, IN.uv_ViolenceTexture2);
            float height2 = tex2D(_ViolenceHeightMap2, IN.uv_ViolenceTexture2);
            
            fixed3 blended;
            if (mask1 < 0.02 && mask2 < 0.02)
                blended = mainColor.rgb;
            else
            blended = height_blend(mainColor, 1 - mask1 * mask2 + height1 * height2, violence1, mask1 * height1, violence2,
                         mask2 * height2);

            o.Albedo = blended;
            o.Alpha = mainColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}