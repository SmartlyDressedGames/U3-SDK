Shader "Cosmetics/Shine" 
{
	Properties 
	{
		_Albedo("Albedo", 2D) = "black" {}
		_Metallic("Metallic", 2D) = "black" {}
		_Emission("Emission", 2D) = "black" {}
		_Shine("Shine", 2D) = "black" {}
		_ShineScale("Shine Wave Scale", Vector) = (0, 0, 0, 0)
		_ShineOffset("Shine Vertex Offset", Vector) = (0, 0, 0, 0)
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

		sampler2D _Albedo;
		sampler2D _Metallic;
		sampler2D _Emission;
		sampler2D _Shine;
		fixed3 _ShineScale;
		fixed3 _ShineOffset;

		struct Input
		{
			float2 uv_Albedo;
			fixed shine;
		};

		void vert(inout appdata_full v, out Input OUT)
		{
			UNITY_INITIALIZE_OUTPUT(Input, OUT);

			fixed3 vertex = v.vertex * _ShineOffset;
			fixed offset = vertex.x + vertex.y + vertex.z;
			OUT.shine = saturate(sin(offset + _Time.y * _ShineScale.x) * _ShineScale.y - _ShineScale.z);
		}

		void surf(Input IN, inout SurfaceOutputStandard OUT)
		{
			fixed4 albedo = tex2D(_Albedo, IN.uv_Albedo);
			fixed4 metallic = tex2D(_Metallic, IN.uv_Albedo);
			fixed4 emission = tex2D(_Emission, IN.uv_Albedo);
			fixed4 shine = tex2D(_Shine, IN.uv_Albedo);

			OUT.Albedo = lerp(albedo.rgb, shine.rgb, IN.shine * shine.a);

			OUT.Metallic = metallic.r;
			OUT.Smoothness = metallic.a;

			OUT.Emission = emission.rgb;
		}

		ENDCG
	} 

	FallBack "Diffuse"
}
