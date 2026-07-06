Shader "Standard/Diffuse"
{
	Properties 
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}

	SubShader 
	{
		Tags 
		{ 
			"RenderType" = "Opaque" 
		}

		LOD 200

		Stencil
		{
			Ref 1
			WriteMask 1
			Pass Replace
		}
		
		CGPROGRAM

		#pragma surface surf StandardSpecular
		#pragma target 3.0
		#include "Assets/Game/Sources/Shaders/CGIncludes/Rain.cginc"
		#pragma multi_compile ___ IS_RAINING

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
			float3 worldPos;
			float3 worldNormal;
		};

		void surf (Input IN, inout SurfaceOutputStandardSpecular OUT) 
		{
			float3 albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
			OUT.Albedo = albedo;
			OUT.Specular = 0;

			#ifdef IS_RAINING
			float puddle;
			rainSpecular(IN.worldPos, IN.worldNormal, 1, OUT.Albedo, OUT.Specular, OUT.Smoothness);
			#endif
		}

		ENDCG
	}

	FallBack "Diffuse"
}
