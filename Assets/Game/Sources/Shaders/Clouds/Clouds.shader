Shader "Unlit/Clouds" 
{
	Properties 
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_RimColor("Rim Color", Color) = (1, 1, 1, 1)
		_RimPower("Rim Power", Range(0.5,8.0)) = 0.5
	}

	SubShader 
	{
		Tags 
		{ 
			"RenderType"="Opaque" 
		}
		LOD 200
		
		CGPROGRAM

		#pragma surface surf Lambert noambient
		#pragma target 3.0

		struct Input 
		{
			float3 viewDir;
		};

		fixed4 _Color;
		fixed4 _RimColor;
		float _RimPower;

		void surf (Input IN, inout SurfaceOutput OUT)
		{
			OUT.Albedo = _Color;
			half rim = 1.0f - saturate(dot(normalize(IN.viewDir), OUT.Normal));
			OUT.Emission = _RimColor * (0.5f + 0.5f * pow(rim, _RimPower));
		}

		ENDCG
	} 

	FallBack "Diffuse"
}
