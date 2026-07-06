// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Particles/Cutout" 
{
	Properties {
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Particle Texture", 2D) = "white" {}
	}

	Category 
	{
		Tags 
		{ 
			"Queue"="AlphaTest" 
			"IgnoreProjector"="True" 
			"RenderType"="TransparentCutout" 
		}
		
		SubShader 
		{
			Pass 
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles
				#pragma multi_compile_fog

				#include "UnityCG.cginc"

				fixed4 _TintColor;
				sampler2D _MainTex;
				float4 _MainTex_ST;
				
				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
				};

				v2f vert (appdata_t v)
				{
					v2f o;
					
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					UNITY_TRANSFER_FOG(o, o.vertex);
					
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
					UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0)); // fog towards black due to our blend mode
					
					if(col.a < 1.0) discard;
					
					return col;
				}
				
				ENDCG 
			}
		}	
	}
}
