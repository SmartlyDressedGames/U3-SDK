Shader "Skins/Dawn" 
{
	Properties 
	{
		_AlbedoBase("Albedo Base", 2D) = "black" {}
		_MetallicBase("Metallic Base", 2D) = "black" {}
		_EmissionBase("Emission Base", 2D) = "black" {}
		_SunColor("Sun Color", Color) = (1.0, 0.5, 0.0, 1.0)
		_RaysColor("Rays Color", Color) = (0.9, 0.6, 0.0, 1.0)
		_SkyColor("Sky Color", Color) = (1.0, 0.0, 0.0, 1.0)
		_EquatorColor("Equator Color", Color) = (0.0, 1.0, 0.0, 1.0)
		_GroundColor("Ground Color", Color) = (0.0, 0.0, 1.0, 1.0)
		_RimPower("Rim Power", Float) = 1.0
	}

	SubShader 
	{
		Tags 
		{ 
			"RenderType"="Opaque" 
		}

		LOD 200
		
		CGPROGRAM

		#pragma surface surf Standard vertex:vert
		#pragma target 3.0

		sampler2D _AlbedoBase;
		sampler2D _MetallicBase;
		sampler2D _EmissionBase;
		fixed4 _SunColor;
		fixed4 _RaysColor;
		fixed4 _SkyColor;
		fixed4 _EquatorColor;
		fixed4 _GroundColor;
		float _RimPower;

		struct Input
		{
			float2 uv_AlbedoBase;
			float3 viewDir;
			float3 worldNormal;
			float3 localPos;
		};

		void vert(inout appdata_full v, out Input OUT)
		{
			UNITY_INITIALIZE_OUTPUT(Input, OUT);
			OUT.localPos = v.vertex.xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard OUT)
		{
			fixed4 albedoBase = tex2D(_AlbedoBase, IN.uv_AlbedoBase);
			fixed4 metallicBase = tex2D(_MetallicBase, IN.uv_AlbedoBase);
			fixed4 emissionBase = tex2D(_EmissionBase, IN.uv_AlbedoBase);

			fixed4 sphereColor = saturate(IN.worldNormal.y) * _SkyColor
				+ saturate(1.0 - abs(IN.worldNormal.y)) * _EquatorColor
				+ saturate(-IN.worldNormal.y) * _GroundColor;

			float lightAlpha = (sin(_Time.y + IN.localPos.y * -5.0) + 1.0) * 0.5; // Rempap [-1, 1] to [0, 1]
			fixed4 lightColor = lerp(_SunColor, _RaysColor, lightAlpha);

			float fresnelAlpha = pow(1.0 - saturate(dot(IN.viewDir, IN.worldNormal)), _RimPower);
			fixed4 skinColor = lerp(sphereColor, lightColor, fresnelAlpha);

			fixed4 albedo = albedoBase * albedoBase.a;
			fixed4 metallic = metallicBase * albedoBase.a;
			fixed4 emission = emissionBase * albedoBase.a + skinColor * (1.0 - albedoBase.a);

			OUT.Albedo = albedo.rgb;
			OUT.Alpha = albedo.a;
			OUT.Metallic = metallic.r;
			OUT.Smoothness = metallic.a;
			OUT.Emission = emission.rgb;
		}

		ENDCG
	} 

	FallBack "Diffuse"
}
