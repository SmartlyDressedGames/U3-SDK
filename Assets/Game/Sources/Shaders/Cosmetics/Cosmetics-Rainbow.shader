Shader "Cosmetics/Rainbow" 
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

			// Nelson 2026-04-09: noticed discontinuity with some effects, traced back to dynamic batching changing object-space coords. ;)
			"DisableBatching" = "True"
		}

		LOD 200
		
		CGPROGRAM

		#pragma surface surf Standard vertex:vert 
		#pragma target 3.0

		#include "UnityCG.cginc"

		sampler2D _Albedo;
		fixed2 _RainbowUV;
		fixed2 _RainbowScale;
		fixed3 _RainbowOffset;

		struct Input
		{
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
