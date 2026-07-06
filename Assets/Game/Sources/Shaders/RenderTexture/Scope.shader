Shader "Unlit/Scope"
{
  	Properties 
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Alpha ("Alpha", Float) = 0.5
	}

	SubShader 
	{
		Tags 
		{ 
			"RenderType" = "Opaque" 
		}

		LOD 200

		CGPROGRAM

		#pragma surface surf Standard
		#pragma target 3.0

		sampler2D _MainTex;
		float _Alpha;

		struct Input 
		{
			float2 uv_MainTex;
			float3 worldPos;
			float3 worldNormal;
		};

		void surf (Input IN, inout SurfaceOutputStandard OUT) 
		{
			const float3 unscopedAlbedo = float3(0.3333333, 0.3843137, 0.3764706);

			float3 albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
			OUT.Albedo = lerp(unscopedAlbedo, float3(0, 0, 0), _Alpha);
			OUT.Emission = lerp(float3(0, 0, 0), albedo, _Alpha);
			OUT.Metallic = lerp(0.2, 0.0, _Alpha);
		}

		ENDCG
	}

	FallBack "Diffuse"
}
