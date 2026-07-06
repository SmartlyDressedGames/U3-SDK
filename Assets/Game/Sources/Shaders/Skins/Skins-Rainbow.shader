Shader "Skins/Rainbow" 
{
	Properties 
	{
		_AlbedoBase("Albedo Base", 2D) = "" {}
		_MetallicBase("Metallic Base", 2D) = "black" {}
		_EmissionBase("Emission Base", 2D) = "black" {}
		_EmissionSkin("Emission Skin", 2D) = "black" {}
		_RainbowUV("Rainbow UV Scale", Vector) = (0, 0, 0, 0)
		_RainbowScale("Rainbow UV Speed", Vector) = (0, 0, 0, 0)
		_RainbowOffset("Rainbow Vertex Scale", Vector) = (0, 0, 0, 0)
	}

	SubShader 
	{
		Tags 
		{ 
			"RenderType"="Opaque"

			// Nelson 2026-04-09: noticed discontinuity with some effects, traced back to dynamic batching changing object-space coords. ;)
			"DisableBatching" = "True"
		}

		LOD 200
		
		CGPROGRAM

		#pragma surface surf Standard vertex:vert
		#pragma target 3.0

		fixed4 _Color;
		sampler2D _AlbedoBase;
		sampler2D _MetallicBase;
		sampler2D _EmissionBase;
		sampler2D _EmissionSkin;
		fixed2 _RainbowUV;
		fixed2 _RainbowScale;
		fixed3 _RainbowOffset;

		struct Input
		{
			float2 uv_AlbedoBase;
			float3 localPos;
		};

		void vert(inout appdata_full v, out Input OUT)
		{
			UNITY_INITIALIZE_OUTPUT(Input, OUT);
			OUT.localPos = v.vertex.xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard OUT)
		{
			fixed3 vertex = IN.localPos * _RainbowOffset; // local vertex with unused axis removed
			fixed rainbow = vertex.x + vertex.y + vertex.z; // get magnitude of vertex
			fixed2 uv = _RainbowUV * rainbow + _RainbowScale * _Time.y; // scale uv and offset by time

			fixed4 albedoBase = tex2D(_AlbedoBase, IN.uv_AlbedoBase);
			fixed4 metallicBase = tex2D(_MetallicBase, IN.uv_AlbedoBase);
			fixed4 emissionBase = tex2D(_EmissionBase, IN.uv_AlbedoBase);
			fixed4 emissionSkin = tex2D(_EmissionSkin, uv);

			fixed4 albedo = albedoBase;
			fixed4 metallic = metallicBase * albedoBase.a;
			fixed4 emission = emissionBase * albedoBase.a + emissionSkin * (1.0 - albedoBase.a);

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
