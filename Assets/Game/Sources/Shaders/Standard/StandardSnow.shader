Shader "Standard/Snow"
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
		#include "Assets/Game/Sources/Shaders/CGIncludes/ProjectionMapping.cginc"
		#include "Assets/Game/Sources/Shaders/CGIncludes/Landscapes/LandscapeProjectionMapping.cginc"
		#pragma multi_compile ___ TRIPLANAR_MAPPING_ON
		#include "Assets/Game/Sources/Shaders/CGIncludes/Snow.cginc"
		#pragma multi_compile ___ IS_SNOWING
		#pragma multi_compile_instancing

		sampler2D _MainTex;

		struct Input 
		{
#ifndef TRIPLANAR_MAPPING_ON
			float2 uv_MainTex;
#endif
			float3 worldPos;
			float3 worldNormal;
			float3 viewDir;
		};

		void surf (Input IN, inout SurfaceOutputStandardSpecular OUT) 
		{
#if defined(TRIPLANAR_MAPPING_ON) || defined(IS_SNOWING)
			float3 blend = triplanarBlend(IN.worldPos, IN.worldNormal);
#endif
#ifdef TRIPLANAR_MAPPING_ON
			float3 albedo = landscapeTriplanarSample3(_MainTex, IN.worldPos, blend);
#else
			float3 albedo = tex2D(_MainTex, IN.uv_MainTex);
#endif

			OUT.Albedo = albedo;
			OUT.Specular = 0;

#ifdef IS_SNOWING
			snow(IN.worldPos, blend, IN.viewDir, 1, OUT.Albedo);
#endif
		}

		ENDCG
	}

	FallBack "Diffuse"
}
