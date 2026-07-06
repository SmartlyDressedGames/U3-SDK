// https://unity.com/blog/engine-platform/extending-unity-5-rendering-pipeline-command-buffers

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Decal/Blast"
{
	Properties
	{
		_MainTex ("Diffuse", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			Fog { Mode Off } // no fog in g-buffers pass
			Cull Front
			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha

			Stencil
			{
				Ref 1
				ReadMask 1
				Comp Equal
			}

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers nomrt
			
			#include "UnityCG.cginc"
			#include "UnityPBSLighting.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 screenUV : TEXCOORD1;
				float3 ray : TEXCOORD2;
				half3 orientation : TEXCOORD3;
			};

			v2f vert (float3 v : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (float4(v,1));
				o.screenUV = ComputeScreenPos (o.pos);
				o.ray = mul (UNITY_MATRIX_MV, float4(v,1)).xyz * float3(-1,-1,1);
				o.orientation = mul ((float3x3)unity_ObjectToWorld, float3(0,0,1));
				return o;
			}

			CBUFFER_START(UnityPerCamera2)
			// float4x4 _CameraToWorld;
			CBUFFER_END

			sampler2D _MainTex;
			sampler2D_float _CameraDepthTexture;
			sampler2D _NormalsCopy;

			// Refer to DecalRenderer for explanation.
			float3 _DecalHackAmbientEquator;
			float3 _DecalHackAmbientSky;
			float3 _DecalHackAmbientGround;

			void frag(v2f i, out half4 outDiffuse : COLOR0, out half4 outEmission : COLOR1)
			{
				i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
				float2 uv = i.screenUV.xy / i.screenUV.w;
				// read depth and reconstruct world position
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
				depth = Linear01Depth (depth);
				float4 vpos = float4(i.ray * depth,1);
				float3 wpos = mul (unity_CameraToWorld, vpos).xyz;
				float3 opos = mul (unity_WorldToObject, float4(wpos,1)).xyz;

				float mag = length(opos);
				clip (-mag + 0.5);

				half3 normal = tex2D(_NormalsCopy, uv).rgb;
				fixed3 wnormal = normal.rgb * 2.0 - 1.0;

				fixed4 col = tex2D(_MainTex, opos.xz + 0.5 + opos.y);
				col.a *= 1 - pow(mag * 2, 2);

				outDiffuse = col;

				// Without ambient emission the decal is black in shadow.
				half3 ambient = (1.0 - abs(wnormal.y)) * _DecalHackAmbientEquator;
				ambient += saturate(wnormal.y) * _DecalHackAmbientSky;
				ambient += saturate(-wnormal.y) * _DecalHackAmbientGround;

				// If adjusting emission alpha please refer to comment in Decal-Diffuse-Alpha.shader.
				outEmission = half4(col.rgb * ambient, col.a);
			}
			ENDCG
		}		

	}

	Fallback Off
}
