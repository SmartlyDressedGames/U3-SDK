Shader "Standard/Ice"
{
	Properties 
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Metallic ("Metallic", 2D) = "black" {}
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

		#pragma surface surf Standard
		#pragma target 3.0
		#include "Assets/Game/Sources/Shaders/CGIncludes/Snow.cginc"
		#include "Assets/Game/Sources/Shaders/CGIncludes/ProjectionMapping.cginc"
		#pragma multi_compile ___ IS_SNOWING

		sampler2D _MainTex;
		sampler2D _Metallic;

		struct Input 
		{
			float2 uv_MainTex;
			float3 worldPos;
			float3 worldNormal;
			float3 viewDir;
		};

		void surf (Input IN, inout SurfaceOutputStandard OUT) 
		{
			float3 albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
			OUT.Albedo = albedo;
			float4 metallic = tex2D(_Metallic, IN.uv_MainTex);
			OUT.Metallic = metallic.r;
			OUT.Smoothness = metallic.a;

#ifdef IS_SNOWING
			float3 blend = triplanarBlend(IN.worldPos, IN.worldNormal);
			snow(IN.worldPos, blend, IN.viewDir, 1, OUT.Albedo);
#endif
		}

		ENDCG
	}

	FallBack "Diffuse"
}
