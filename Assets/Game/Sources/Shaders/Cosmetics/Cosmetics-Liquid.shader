Shader "Cosmetics/Liquid" 
{
	Properties 
	{
		_Albedo0("Albedo 0", 2D) = "black" {}
		_Albedo1("Albedo 1", 2D) = "black" {}
		_Metallic0("Metallic 0", 2D) = "black" {}
		_Metallic1("Metallic 1", 2D) = "black" {}
		_Emission0("Emission 0", 2D) = "black" {}
		_Emission1("Emission 1", 2D) = "black" {}
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
		sampler2D _Albedo0;
		sampler2D _Albedo1;
		sampler2D _Metallic0;
		sampler2D _Metallic1;
		sampler2D _Emission0;
		sampler2D _Emission1;
		fixed4 _RainbowScale;

		struct Input
		{
			float2 uv_Albedo0;
		};

		void vert(inout appdata_full v, out Input OUT)
		{
			UNITY_INITIALIZE_OUTPUT(Input, OUT);
		}

		void surf(Input IN, inout SurfaceOutputStandard OUT)
		{
			fixed2 uv0 = float2(_RainbowScale.x, _RainbowScale.y) * _Time.y + IN.uv_Albedo0; // scale uv and offset by time
			fixed2 uv1 = float2(_RainbowScale.z, _RainbowScale.w) * _Time.y + IN.uv_Albedo0; // scale uv and offset by time

			fixed4 albedo0 = tex2D(_Albedo0, uv0);
			fixed4 albedo1 = tex2D(_Albedo1, uv1);

			fixed4 metallic0 = tex2D(_Metallic0, uv0);
			fixed4 metallic1 = tex2D(_Metallic1, uv1);

			fixed4 emission0 = tex2D(_Emission0, uv0);
			fixed4 emission1 = tex2D(_Emission1, uv1);

			fixed4 albedo = albedo0 * albedo0.a + albedo1 * (1.0 - albedo0.a);
			fixed4 metallic = metallic0 * (1.0 - albedo.a) * metallic0.a + metallic1 * (1.0 - albedo.a) * (1.0 - metallic0.a);
			fixed4 emission = emission0 * (1.0 - albedo.a) * emission0.a + emission1 * (1.0 - albedo.a) * (1.0 - emission0.a);

			OUT.Albedo = albedo.rgb * albedo.a;

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
