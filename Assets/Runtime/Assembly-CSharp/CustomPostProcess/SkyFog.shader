Shader "Hidden/Custom/SkyFog"
{
	HLSLINCLUDE

		// StdLib.hlsl holds pre-configured vertex shaders (VertDefault), varying structs (VaryingsDefault), and most of the data you need to write common effects.
		#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
		#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Builtins/Fog.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);

		uniform float4x4 _InverseProjectionMatrix;
		uniform float4x4 _CameraToWorld;

		uniform float3 _SkyColor;
		uniform float3 _EquatorColor;
		uniform float3 _GroundColor;

		// Global skybox amount.
		uniform float _AtmosphericFog;

		uniform float3 _WaterColor;
		uniform float _IsCameraUnderwater;
		uniform int _WaterCount;
		uniform float4x4 _WaterMatrices[3];

		float IsWithinWater(float3 originWS)
		{
			const float HALF_BOUND_PLUS_ONE = asfloat(0x3f000001); // 0.5f with LSB set (public issue #5022)
			for (int index = 0; index < _WaterCount; ++index)
			{
				float4x4 worldToLocal = _WaterMatrices[index];
				float3 originLocal = mul(worldToLocal, float4(originWS, 1.0)).xyz;
				originLocal = abs(originLocal);
				if (originLocal.x <= HALF_BOUND_PLUS_ONE && originLocal.y <= HALF_BOUND_PLUS_ONE && originLocal.z <= HALF_BOUND_PLUS_ONE)
				{
					return 1.0;
				}
			}

			return 0.0;
		}

		struct Varyings
		{
			float4 vertex : SV_POSITION;
			float2 texcoord : TEXCOORD0;
			float3 viewDir : TEXCOORD1;
		};
		
		Varyings Vert(AttributesDefault input)
		{
			Varyings output;
			output.vertex = float4(input.vertex.xy, 0.0, 1.0);
			output.texcoord = TransformTriangleVertexToUV(input.vertex.xy);
#if UNITY_UV_STARTS_AT_TOP
			output.texcoord = output.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif

			// There is probably a smarter way to combine these matrices?
			// Currently we also use _CameraToWorld in fragment shader to get forward axis.
			output.viewDir = mul(_InverseProjectionMatrix, float4(output.texcoord * 2.0 - 1.0, 1.0, 1.0)).xyz;
			output.viewDir = mul(_CameraToWorld, float4(output.viewDir, 0.0)).xyz;
			return output;
		}

		float4 Frag(Varyings input) : SV_Target
		{
			float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord);
			float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord);
			float depth = LinearEyeDepth(rawDepth);
			
			// 3rd column is the camera's -Z axis in world space.
			float3 forward = float3(-_CameraToWorld._m02, -_CameraToWorld._m12, -_CameraToWorld._m22);

			// Right angle triangle where adjacent is depth and we want to calculate hypotenuse (spherical depth).
			// cos = adj / hyp -> hyp = adj / cos
			float3 viewportDir = normalize(input.viewDir);
			float sphericalDepth = depth / dot(viewportDir, forward);

			// Treat below horizon as "not skybox" so that ocean along horizon gets fog.
			float notSkybox = rawDepth > 0 || viewportDir.y < 0.0f;

			float farClipDist = _ProjectionParams.z;
			float fogStartDist = farClipDist * 0.5;
			float fogTransitionDist = farClipDist - fogStartDist;
			float fogAlpha = saturate((sphericalDepth - fogStartDist) / fogTransitionDist);
			fogAlpha = pow(fogAlpha, 2.0);

			// Identical to sky gradient in skybox shader.
			float3 skyColor;
			float skyFactor = 1 - pow(1 - abs(viewportDir.y), 4);
			if (viewportDir.y > 0)
			{
				skyColor = lerp(_EquatorColor, _SkyColor, skyFactor);
			}
			else
			{
				skyColor = lerp(_EquatorColor, _GroundColor, skyFactor);
			}

			skyColor = lerp(skyColor, _FogColor.rgb, _AtmosphericFog);

			float3 outputColor = lerp(color.rgb, skyColor, fogAlpha * notSkybox);

			// _ProjectionParams.y is near clip plane distance. 
			float3 viewPos = _WorldSpaceCameraPos + input.viewDir * _ProjectionParams.y;
			if (abs(_IsCameraUnderwater - IsWithinWater(viewPos)) > 0.5)
			{
				// Water mask when fragment is underwater without global fog enabled, or if fragment is not underwater
				// and camera is underwater.
				outputColor = _WaterColor;
			}

			return float4(outputColor, color.a);
		}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag
			ENDHLSL
		}
	}
}
