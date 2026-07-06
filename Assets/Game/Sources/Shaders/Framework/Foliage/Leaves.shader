Shader "Framework/Leaves" 
{
	Properties 
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutoff("Cutoff", float) = 0.5
	}

	SubShader 
	{
		Tags 
		{ 
			"Queue" = "AlphaTest" 
			"RenderType" = "TransparentCutout"
		}

		Cull Off
		LOD 200
		
		CGPROGRAM
			 
		#pragma multi_compile ___ GRASS_WIND_ON
		#pragma multi_compile_instancing
		#pragma surface surf StandardSpecular addshadow vertex:vert alphatest:_Cutoff
		#pragma target 3.0
		#include "Assets/Game/Sources/Shaders/CGIncludes/Landscapes/FoliageWind.cginc"
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		fixed4 _Color;

		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 color : COLOR;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Input 
		{
			float2 uv_MainTex;
		};

		void vert(inout appdata IN, out Input OUT)
		{
			UNITY_INITIALIZE_OUTPUT(Input, OUT);

#ifdef GRASS_WIND_ON
			float windInfluence = IN.color.r;
			float4 worldPos = mul(unity_ObjectToWorld, IN.vertex);
			windAnimation(worldPos, windInfluence);
			IN.vertex = mul(unity_WorldToObject, worldPos);
#endif
		}

		void surf(Input IN, inout SurfaceOutputStandardSpecular OUT)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			OUT.Albedo = c.rgb;
			OUT.Alpha = c.a;
			OUT.Specular = 0.0;
		}

		ENDCG
	} 

	Fallback "Standard"
}
