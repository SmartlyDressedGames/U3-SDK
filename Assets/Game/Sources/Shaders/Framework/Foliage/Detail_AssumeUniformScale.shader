// Nelson 2025-09-10: this is nearly identical to Framework/Detail.
// Initially, assume uniform scale was an option using:
// [Toggle] ASSUME_UNIFORM_SCALE ("Uniform Scale", Float) = 0
// However, setting #pragma instancing_options inside #ifdef does not seem to be supported.
Shader "Framework/Detail (Uniform Scale)" 
{
	Properties 
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
	}

	SubShader 
	{
		Tags 
		{ 
			"RenderType" = "Opaque"
		}

		LOD 200
		
		CGPROGRAM
			 
		#pragma multi_compile_instancing
		#pragma instancing_options assumeuniformscaling

		#pragma surface surf StandardSpecular addshadow
		#pragma target 3.0
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input 
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandardSpecular OUT)
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			OUT.Albedo = c.rgb;
			OUT.Alpha = c.a;
			OUT.Specular = 0.0;
		}

		ENDCG
	}

	FallBack "Standard"
}
