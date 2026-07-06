Shader "Custom/Card" 
{
	Properties 
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
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
			 
		#pragma surface surf StandardSpecular addshadow
		#pragma target 3.0
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		float _Cutoff;

		struct Input 
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandardSpecular OUT)
		{
			fixed4 color = tex2D (_MainTex, IN.uv_MainTex);
			clip(color.a - _Cutoff);
			OUT.Albedo = color.rgb;
			OUT.Alpha = color.a;
			OUT.Specular = 0.0;
			OUT.Smoothness = 0.0;
			OUT.Emission = 0.0;
		}

		ENDCG
	} 

	Fallback "Standard"
}
