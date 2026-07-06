// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Aurora Borealis" 
{
	Properties 
	{
		_MainTex ("Color Strip", 2D) = "white" {}
		_Pattern("Pattern", 2D) = "white" {}
		_Intensity ("Intensity", Float) = 1
	}

	SubShader 
	{
		Tags 
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		LOD 100
		Cull Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
	
		Pass 
		{  
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t 
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				half2 texcoord0 : TEXCOORD0; // color
				half2 texcoord1 : TEXCOORD1; // pattern
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _Pattern;
			float4 _Pattern_ST;
			float _Intensity;
			fixed _AtmosphericFog;
			
			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord1 = TRANSFORM_TEX(v.texcoord, _Pattern);
				o.color = v.color;

				float vertexTimeOffsetWave = v.vertex.x * 2 + v.vertex.y * 2 + sin(_Time.w / 64 + v.vertex.y * 4) / 16;
				float vertexTimeOffset = v.vertex.x * 3 + abs(sin(_Time.w / 64 + vertexTimeOffsetWave)) * 2;

				o.vertex.x += sin(_Time.w / 128 + vertexTimeOffset) * 32;

				float colorTimeOffset = v.vertex.y / 8;

				o.texcoord0.x = _Time.w / 256 + colorTimeOffset;
				
				float alphaTimeOffsetWave = v.vertex.x / 4 + v.vertex.y / 4 + sin(_Time.w / 32 + v.vertex.y / 4) / 16;
				float alphaTimeOffset = v.vertex.x / 4 + abs(sin(_Time.w / 64 + alphaTimeOffsetWave)) * 2;

				o.texcoord0.y = abs(sin(_Time.w / 64 + alphaTimeOffset));

				float patternTimeOffset = v.vertex.y * 8;

				o.texcoord1.x += _Time.w / 1024 + patternTimeOffset;

				return o;
			}
			
			fixed4 frag(v2f i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.texcoord0);
				fixed4 pat = tex2D(_Pattern, i.texcoord1);
				col.a *= pat.a;
				col.a *= i.texcoord0.y;
				col.a *= _Intensity;
				col.a *= i.color.r;
				col.a *= (1 - _AtmosphericFog); // Fade out during storm

				return col;
			}

			ENDCG
		}
	}
}
