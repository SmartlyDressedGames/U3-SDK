Shader "Custom/Water_Fallback"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}

        LOD 200
		Cull Off

        CGPROGRAM

        #pragma surface surf Standard alpha
        #pragma target 3.0

        struct Input
        {
			float3 viewDir;
        };

        fixed4 _BaseColor;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			o.Albedo = _BaseColor.rgb;
			o.Metallic = 0;
			o.Smoothness = 0.9;
			o.Alpha = 0.9;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
