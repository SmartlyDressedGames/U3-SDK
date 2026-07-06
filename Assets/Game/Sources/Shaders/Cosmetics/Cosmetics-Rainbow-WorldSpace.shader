Shader "Cosmetics/Rainbow (World Space)" 
{
	Properties 
	{
		_Albedo("Albedo", 2D) = "white" {}
		_RainbowUV("Rainbow UV Scale", Vector) = (0, 0, 0, 0)
		_RainbowScale("Rainbow UV Speed", Vector) = (0, 0, 0, 0)
		_RainbowOffset("Rainbow Vertex Scale", Vector) = (0, 0, 0, 0)
	}

	SubShader 
	{
		Tags 
		{ 
			"RenderType"="Opaque"
		}

		LOD 200
		
		CGPROGRAM

		#pragma surface surf Standard
		#pragma target 3.0

		#include "UnityCG.cginc"

		sampler2D _Albedo;
		fixed2 _RainbowUV;
		fixed2 _RainbowScale;
		fixed3 _RainbowOffset;

		struct Input
		{
			float3 worldPos;
		};

		void surf(Input IN, inout SurfaceOutputStandard OUT)
		{
			fixed3 vertex = IN.worldPos * _RainbowOffset;
			fixed rainbow = vertex.x + vertex.y + vertex.z; // get magnitude of vertex
			fixed2 uv = _RainbowUV * rainbow + _RainbowScale * _Time.y; // scale uv and offset by time

			fixed4 albedo = tex2D(_Albedo, uv);

			OUT.Albedo = albedo.rgb;
			OUT.Alpha = 1;
			OUT.Metallic = 0;
			OUT.Smoothness = 0;
			OUT.Emission = albedo.rgb;
		}

		ENDCG
	} 

	FallBack "Diffuse"
}
