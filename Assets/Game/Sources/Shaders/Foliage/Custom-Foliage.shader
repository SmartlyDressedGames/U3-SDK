Shader "Custom/Foliage" 
{
	Properties 
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
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
		#pragma surface surf StandardSpecular addshadow vertex:vert
		#pragma target 3.0
		#include "TerrainEngine.cginc"
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		fixed4 _Color;
		float _Cutoff;

		struct Input 
		{
			float2 uv_MainTex;
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
			clip(c.a - _Cutoff);
			OUT.Albedo = c.rgb;
			OUT.Alpha = c.a;
			OUT.Specular = 0.0;
			OUT.Smoothness = 0.0;
			OUT.Emission = 0.0;
		}

		ENDCG
	} 

	Fallback "Standard"
}
