Shader "Unturned/Placement Preview"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"RenderType"="Transparent"
		}
		LOD 200
		Cull Off
	
		CGPROGRAM
		#pragma surface surf Lambert alpha:fade

		float4 _Color;

		struct Input
		{
			float3 viewDir;
			float3 worldNormal;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			o.Emission = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	Fallback "Legacy Shaders/Transparent/VertexLit"
}
