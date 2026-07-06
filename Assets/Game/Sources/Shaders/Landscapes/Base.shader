// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see THIRDPARTYNOTICES.txt)

Shader "Landscapes/Base"
{
    Properties
	{
        _MainTex ("Base (RGB) Smoothness (A)", 2D) = "white" {}

        // used in fallback on old cards
        _Color ("Main Color", Color) = (1,1,1,1)

		[HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
    }

    SubShader
	{
        Tags
		{
            "RenderType" = "Opaque"
            "Queue" = "Geometry-100"
        }

		Stencil
		{
			Ref 1
			WriteMask 1
			Pass Replace
		}

        LOD 200

        CGPROGRAM
        #pragma surface surf StandardSpecular vertex:SplatmapVert addshadow fullforwardshadows
        #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
        #pragma target 3.0

		#pragma multi_compile_local __ _ALPHATEST_ON

        #define TERRAIN_BASE_PASS
		#define TERRAIN_INSTANCED_PERPIXEL_NORMAL
		#include "Assets/Game/Sources/Shaders/CGIncludes/Landscapes/LandscapeCommon.cginc"
        #include "UnityPBSLighting.cginc"

        sampler2D _MainTex;

        void surf (Input IN, inout SurfaceOutputStandardSpecular o)
		{
			#ifdef _ALPHATEST_ON
				ClipHoles(IN.tc.xy);
			#endif
            half4 c = tex2D (_MainTex, IN.tc.xy);
            o.Albedo = c.rgb;
            o.Alpha = 1;

            #if defined(INSTANCING_ON) && defined(SHADER_TARGET_SURFACE_ANALYSIS) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
                o.Normal = float3(0, 0, 1); // make sure that surface shader compiler realizes we write to normal, as UNITY_INSTANCING_ENABLED is not defined for SHADER_TARGET_SURFACE_ANALYSIS.
            #endif

            #if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
                o.Normal = normalize(tex2D(_TerrainNormalmapTexture, IN.tc.zw).xyz * 2 - 1).xzy;
            #endif
        }

        ENDCG

        UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
        UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
    }

    FallBack "Diffuse"
}
