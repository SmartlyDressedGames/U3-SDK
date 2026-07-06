Shader "Unturned/Vehicle"
{
    Properties
    {
        _Color ("Tint Color", Color) = (1,1,1,1)
        _PaintColor ("Paint Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags
		{
			"RenderType" = "Opaque"
		}

        LOD 200

		Stencil
		{
			Ref 1
			WriteMask 1
			Pass Replace
		}

        CGPROGRAM

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
		float4 _Color;
		float4 _PaintColor;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
			float4 textureColor = tex2D(_MainTex, IN.uv_MainTex);
			float4 baseColor = lerp(_PaintColor, textureColor, textureColor.a);
			float4 c = baseColor * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }

        ENDCG
    }

    FallBack "Diffuse"
}
