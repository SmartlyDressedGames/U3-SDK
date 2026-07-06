Shader "Skins/Liquid" 
{
	Properties 
	{
		_AlbedoBase("Albedo Base", 2D) = "" {}
		_AlbedoSkin0("Albedo Skin 0", 2D) = "black" {}
		_AlbedoSkin1("Albedo Skin 1", 2D) = "black" {}
		_MetallicBase("Metallic Base", 2D) = "black" {}
		_MetallicSkin0("Metallic Skin 0", 2D) = "black" {}
		_MetallicSkin1("Metallic Skin 1", 2D) = "black" {}
		_EmissionBase("Emission Base", 2D) = "black" {}
		_EmissionSkin0("Emission Skin 0", 2D) = "black" {}
		_EmissionSkin1("Emission Skin 1", 2D) = "black" {}
		_RainbowScale("Rainbow UV Speed", Vector) = (0, 0, 0, 0)
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

		fixed4 _Color;
		sampler2D _AlbedoBase;
		sampler2D _AlbedoSkin0;
		sampler2D _AlbedoSkin1;
		sampler2D _MetallicBase;
		sampler2D _MetallicSkin0;
		sampler2D _MetallicSkin1;
		sampler2D _EmissionBase;
		sampler2D _EmissionSkin0;
		sampler2D _EmissionSkin1;
		fixed4 _RainbowScale;

		struct Input
		{
			float2 uv_AlbedoBase;
			float2 uv_AlbedoSkin0;
		};

		void vert(inout appdata_full v, out Input OUT)
		{
			UNITY_INITIALIZE_OUTPUT(Input, OUT);
		}

		void surf(Input IN, inout SurfaceOutputStandard OUT)
		{
			fixed2 uv0 = float2(_RainbowScale.x, _RainbowScale.y) * _Time.y + IN.uv_AlbedoSkin0; // scale uv and offset by time
			fixed2 uv1 = float2(_RainbowScale.z, _RainbowScale.w) * _Time.y + IN.uv_AlbedoSkin0; // scale uv and offset by time

			fixed4 albedoBase = tex2D(_AlbedoBase, IN.uv_AlbedoBase);
			fixed4 albedoSkin0 = tex2D(_AlbedoSkin0, uv0);
			fixed4 albedoSkin1 = tex2D(_AlbedoSkin1, uv1);

			fixed4 metallicBase = tex2D(_MetallicBase, IN.uv_AlbedoBase);
			fixed4 metallicSkin0 = tex2D(_MetallicSkin0, uv0);
			fixed4 metallicSkin1 = tex2D(_MetallicSkin1, uv1);

			fixed4 emissionBase = tex2D(_EmissionBase, IN.uv_AlbedoBase);
			fixed4 emissionSkin0 = tex2D(_EmissionSkin0, uv0);
			fixed4 emissionSkin1 = tex2D(_EmissionSkin1, uv1);

			fixed4 albedo = albedoBase  * albedoBase.a + albedoSkin0 * (1.0 - albedoBase.a) * albedoSkin0.a + albedoSkin1 * (1.0 - albedoBase.a) * (1.0 - albedoSkin0.a);
			fixed4 metallic = metallicBase * albedoBase.a + metallicSkin0 * (1.0 - albedo.a) * metallicSkin0.a + metallicSkin1 * (1.0 - albedo.a) * (1.0 - metallicSkin0.a);
			fixed4 emission = emissionBase * albedoBase.a + emissionSkin0 * (1.0 - albedo.a) * emissionSkin0.a + emissionSkin1 * (1.0 - albedo.a) * (1.0 - emissionSkin0.a);

			OUT.Albedo = albedo.rgb;
			OUT.Alpha = albedo.a;

			OUT.Metallic = metallic.r;
			OUT.Smoothness = metallic.a;

			// 2023-01-31: Multiplying by 2 is hack to make emissive glow consistent throughout the game.
			// Previously all standard materials with emissive were updated to use color 2.0. (public issue #3680)
			OUT.Emission = emission.rgb * 2.0;
		}

		ENDCG
	} 

	FallBack "Diffuse"
}
