Shader "Landscapes/HeightTransition/Deferred/AddPass" 
{
	Properties 
	{
		_Fade("Fade", Range(0.001,0.5)) = 0.1

		[HideInInspector] _Control("Control (RGBA)", 2D) = "Black" {}
		[HideInInspector] _Splat3("Layer 3 (A)", 2D) = "Black" {}
		[HideInInspector] _Splat2("Layer 2 (B)", 2D) = "Black" {}
		[HideInInspector] _Splat1("Layer 1 (G)", 2D) = "Black" {}
		[HideInInspector] _Splat0("Layer 0 (R)", 2D) = "Black" {}
		[HideInInspector] _Normal3("Mask 3 (A)", 2D) = "Black" {} // Named normal so that the terrain engine will pass them in
		[HideInInspector] _Normal2("Mask 2 (B)", 2D) = "Black" {} // Named normal so that the terrain engine will pass them in
		[HideInInspector] _Normal1("Mask 1 (G)", 2D) = "Black" {} // Named normal so that the terrain engine will pass them in
		[HideInInspector] _Normal0("Mask 0 (R)", 2D) = "Black" {} // Named normal so that the terrain engine will pass them in

		[HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
	}
	
	SubShader 
	{
		Tags 
		{
			"Queue" = "Geometry-99"
			"RenderType" = "AlphaTest"
		}

		Stencil
		{
			Ref 1
			WriteMask 1
			Pass Replace
		}

		Blend One OneMinusSrcAlpha

		CGPROGRAM

		#pragma surface surf StandardSpecular vertex:SplatmapVert finalcolor:splatmapFinalColor finalprepass:splatmapFinalPrepass finalgbuffer:splatmapFinalGBuffer fullforwardshadows nometa
		#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
		#pragma multi_compile_fog
		#pragma target 3.0

		#pragma multi_compile_local __ _ALPHATEST_ON

		#define TERRAIN_SPLAT_ADDPASS
		#define TERRAIN_STANDARD_SHADER
		#include "UnityPBSLighting.cginc"
		#define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandardSpecular
		#include "Assets/Game/Sources/Shaders/CGIncludes/Landscapes/LandscapeProjectionMapping.cginc"
		#pragma multi_compile ___ TRIPLANAR_MAPPING_ON
		#pragma multi_compile ___ IS_RAINING IS_SNOWING
#ifdef IS_SNOWING
		#include "Assets/Game/Sources/Shaders/CGIncludes/Snow.cginc"
#endif
#ifdef IS_RAINING
		#include "Assets/Game/Sources/Shaders/CGIncludes/Rain.cginc"
#endif
		#include "Assets/Game/Sources/Shaders/CGIncludes/Landscapes/LandscapeCommon.cginc"
		#include "Assets/Game/Sources/Shaders/CGIncludes/Landscapes/LandscapeHeightTransition.cginc"

		void surf(Input IN, inout SurfaceOutputStandardSpecular OUT)
		{
			#ifdef _ALPHATEST_ON
				ClipHoles(IN.tc.xy);
			#endif

			float4 splat_control = tex2D(_Control, IN.tc);

#if defined(TRIPLANAR_MAPPING_ON) || defined(IS_SNOWING)
			float3 blend = landscapeTriplanarBlend(IN.worldPos, IN.worldNormal);
#endif

#ifdef TRIPLANAR_MAPPING_ON
			float4 tex0 = landscapeTriplanarSample4(_Splat0, IN.worldPos, blend);
			float4 tex1 = landscapeTriplanarSample4(_Splat1, IN.worldPos, blend);
			float4 tex2 = landscapeTriplanarSample4(_Splat2, IN.worldPos, blend);
			float4 tex3 = landscapeTriplanarSample4(_Splat3, IN.worldPos, blend);
			float4 mask0 = landscapeTriplanarSample4(_Normal0, IN.worldPos, blend);
			float4 mask1 = landscapeTriplanarSample4(_Normal1, IN.worldPos, blend);
			float4 mask2 = landscapeTriplanarSample4(_Normal2, IN.worldPos, blend);
			float4 mask3 = landscapeTriplanarSample4(_Normal3, IN.worldPos, blend);
#else
			float4 tex0 = landscapePlanarSample4(_Splat0, IN.worldPos);
			float4 tex1 = landscapePlanarSample4(_Splat1, IN.worldPos);
			float4 tex2 = landscapePlanarSample4(_Splat2, IN.worldPos);
			float4 tex3 = landscapePlanarSample4(_Splat3, IN.worldPos);
			float4 mask0 = landscapePlanarSample4(_Normal0, IN.worldPos);
			float4 mask1 = landscapePlanarSample4(_Normal1, IN.worldPos);
			float4 mask2 = landscapePlanarSample4(_Normal2, IN.worldPos);
			float4 mask3 = landscapePlanarSample4(_Normal3, IN.worldPos);
#endif

			fixed nothing = 1-splat_control.r - splat_control.g - splat_control.b - splat_control.a;
			splat_control += fixed4(tex0.a * splat_control.r, tex1.a * splat_control.g, tex2.a * splat_control.b, tex3.a * splat_control.a);
	
			fixed4 sc2;
			sc2.r =	clamp((splat_control.r - nothing)+_Fade,0.001,1) * clamp((splat_control.r - splat_control.g)+_Fade,0.001,1) * clamp((splat_control.r - splat_control.b)+_Fade,0.001,1) * clamp((splat_control.r - splat_control.a)+_Fade,0.001,1);
			sc2.g = clamp((splat_control.g - nothing)+_Fade,0.001,1) * clamp((splat_control.g - splat_control.r)+_Fade,0.001,1) * clamp((splat_control.g - splat_control.b)+_Fade,0.001,1) * clamp((splat_control.g - splat_control.a)+_Fade,0.001,1);
			sc2.b = clamp((splat_control.b - nothing)+_Fade,0.001,1) * clamp((splat_control.b - splat_control.r)+_Fade,0.001,1) * clamp((splat_control.b - splat_control.g)+_Fade,0.001,1) * clamp((splat_control.b - splat_control.a)+_Fade,0.001,1);
			sc2.a = clamp((splat_control.a - nothing)+_Fade,0.001,1) * clamp((splat_control.a - splat_control.r)+_Fade,0.001,1) * clamp((splat_control.a - splat_control.g)+_Fade,0.001,1) * clamp((splat_control.a - splat_control.b)+_Fade,0.001,1);
			fixed alpha = clamp((nothing - splat_control.r)+_Fade,0.001,1) * clamp((nothing - splat_control.g)+_Fade,0.001,1) * clamp((nothing - splat_control.b)+_Fade,0.001,1) * clamp((nothing - splat_control.a)+_Fade,0.001,1);
			float sum = sc2.r + sc2.g + sc2.b + sc2.a;
			sc2 = sc2 / sum;
			alpha = alpha / (sum+alpha);
			
			OUT.Albedo = tex0.rgb * sc2.r + tex1.rgb * sc2.g + tex2.rgb * sc2.b + tex3.rgb * sc2.a;
			OUT.Alpha = clamp(1-alpha-0.01,0.001,1);

			#ifdef IS_SNOWING
			float snowMask = mask0.r * sc2.r + mask1.r * sc2.g + mask2.r * sc2.b + mask3.r * sc2.a;
			snow(IN.worldPos, blend, IN.viewDir, snowMask, OUT.Albedo);
			#endif

			#ifdef IS_RAINING
			float puddle;
			rainSpecular(IN.worldPos, IN.worldNormal, 1, OUT.Albedo, OUT.Specular, OUT.Smoothness);
			#endif
		}

		ENDCG  
	}

	Fallback Off
}
