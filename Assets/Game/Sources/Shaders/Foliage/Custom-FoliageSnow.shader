Shader "Custom/FoliageSnow" 
{
	Properties 
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Mask("Mask", 2D) = "black" {}
		_WaveAndDistance("Wave and distance", Vector) = (12, 3.6, 1, 1)
		_Cutoff("Cutoff", float) = 0.5
	}

	SubShader 
	{
		Tags 
		{ 
			"Queue" = "AlphaTest" 
			"IgnoreProjector" = "True" 
			"RenderType" = "TransparentCutout"
		}

		Cull Off
		LOD 200
		
		CGPROGRAM
			 
		#pragma multi_compile ___ NICE_FOLIAGE_ON
		#pragma surface surf StandardSpecular addshadow vertex:vert alphatest:_Cutoff
		#pragma target 3.0
		#include "TerrainEngine.cginc"
		#include "UnityCG.cginc"
		#include "Assets/Game/Sources/Shaders/CGIncludes/ProjectionMapping.cginc"
		#include "Assets/Game/Sources/Shaders/CGIncludes/Snow.cginc"
		#pragma multi_compile ___ IS_SNOWING

		sampler2D _MainTex;
		sampler2D _Mask;
		fixed4 _Color;

		struct Input 
		{
			float2 uv_MainTex;
			float3 worldPos;
			float3 worldNormal;
			float3 viewDir;
		};

		void vert(inout appdata_full v, out Input OUT) 
		{
			UNITY_INITIALIZE_OUTPUT(Input, OUT);

#ifdef NICE_FOLIAGE_ON
			float waveAmount = v.color.r * _WaveAndDistance.z;
			_WaveAndDistance.x += _Time.x;

			TerrainWaveGrass(v.vertex, waveAmount, v.color);
#endif
		}

		void surf(Input IN, inout SurfaceOutputStandardSpecular OUT)
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			OUT.Albedo = c.rgb;
			OUT.Alpha = c.a;
			OUT.Specular = 0.0;
			OUT.Smoothness = 0.0;
			OUT.Emission = 0.0;

#ifdef IS_SNOWING
			float3 blend = triplanarBlend(IN.worldPos, IN.worldNormal);
			snow(IN.worldPos, blend, IN.viewDir, tex2D(_Mask, IN.uv_MainTex).r, OUT.Albedo);
#endif
		}

		ENDCG
	} 

	Fallback "Standard"
}
