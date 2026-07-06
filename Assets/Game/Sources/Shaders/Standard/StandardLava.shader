Shader "Standard/Lava"
{
	Properties 
	{
		_Albedo0("Albedo 0", 2D) = "black" {}
		_Albedo1("Albedo 1", 2D) = "black" {}
		_Emission0("Emission 0", 2D) = "black" {}
		_Emission1("Emission 1", 2D) = "black" {}
		_RainbowScale0("Rainbow UV Speed 0", Vector) = (0, 0, 0, 0)
		_RainbowScale1("Rainbow UV Speed 1", Vector) = (0, 0, 0, 0)
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
		#include "Assets/Game/Sources/Shaders/CGIncludes/ProjectionMapping.cginc"
		#pragma multi_compile_instancing

		fixed4 _Color;
		sampler2D _Albedo0;
		sampler2D _Albedo1;
		sampler2D _Emission0;
		sampler2D _Emission1;
		fixed4 _RainbowScale0;
		fixed4 _RainbowScale1;

		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float3 viewDir;
		};

		void surf(Input IN, inout SurfaceOutputStandard OUT)
		{
			float3 worldPos0 = IN.worldPos + _RainbowScale0 * _Time.y;
			float3 worldPos1 = IN.worldPos + _RainbowScale1 * _Time.y;

			fixed4 albedo0 = planarSample4(_Albedo0, worldPos0, _RainbowScale0.w);
			fixed4 albedo1 = planarSample4(_Albedo1, worldPos1, _RainbowScale1.w);

			fixed4 emission0 = planarSample4(_Emission0, worldPos0, _RainbowScale0.w);
			fixed4 emission1 = planarSample4(_Emission1, worldPos1, _RainbowScale1.w);

			//float3 blend0 = triplanarBlend(worldPos0, IN.worldNormal, 2);
			//float3 blend1 = triplanarBlend(worldPos1, IN.worldNormal, 2);

			//fixed4 albedo0 = triplanarSample4(_Albedo0, worldPos0, blend0, _RainbowScale0.w);
			//fixed4 albedo1 = triplanarSample4(_Albedo1, worldPos1, blend1, _RainbowScale1.w);

			//fixed4 emission0 = triplanarSample4(_Emission0, worldPos0, blend0, _RainbowScale0.w);
			//fixed4 emission1 = triplanarSample4(_Emission1, worldPos1, blend1, _RainbowScale1.w);

			fixed4 albedo = albedo0 * albedo0.a + albedo1 * (1.0 - albedo0.a);
			fixed4 emission = emission0 * (1.0 - albedo.a) * emission0.a + emission1 * (1.0 - albedo.a) * (1.0 - emission0.a);

			OUT.Albedo = albedo.rgb;
			OUT.Alpha = albedo.a;

			OUT.Emission = emission.rgb;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
