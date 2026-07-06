Shader "Hidden/Custom/GaussianBlur"
{
	HLSLINCLUDE

		// StdLib.hlsl holds pre-configured vertex shaders (VertDefault), varying structs (VaryingsDefault), and most of the data you need to write common effects.
		#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		uniform float4 _MainTex_TexelSize;

		uniform float _StdDeviationSquared;
		uniform int _HalfKernelSize;

		// Formula described at: https://en.wikipedia.org/wiki/Gaussian_blur
		// G(x) = (1 / sqrt(2 * pi * stddev^2)) * e^-(x^2/2 * stddev^2)
		float GetGaussianWeight(int x)
		{
			return (1.0 / sqrt(TWO_PI * _StdDeviationSquared)) * pow(2.71828, -((x * x) / (2 * _StdDeviationSquared)));
		}

		float4 SampleBlur(float2 uv, float2 offsetDirection)
		{
			float3 sumColor = 0;
			float sumWeight = 0;

			for (int x = -_HalfKernelSize; x <= _HalfKernelSize; ++x)
			{
				float weight = GetGaussianWeight(x);
				// Nelson 2025-07-02: *2 here is to sample from a wider radius at a lower cost
				// (public issue #5086)
				float2 uvOffset = offsetDirection * x * 2;
				float3 sampledColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + uvOffset).rgb;
				sumColor += sampledColor * weight;
				sumWeight += weight;
			}

			return float4(sumColor / sumWeight, 1.0);
		}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass // 0, horizontal
		{
			HLSLPROGRAM
				#pragma vertex VertDefault
				#pragma fragment FragHorizontal

				float4 FragHorizontal(VaryingsDefault input) : SV_Target
				{
					return SampleBlur(input.texcoord, float2(_MainTex_TexelSize.x, 0));
				}
			ENDHLSL
		}

		Pass // 0, vertical
		{
			HLSLPROGRAM
				#pragma vertex VertDefault
				#pragma fragment FragVertical

				float4 FragVertical(VaryingsDefault input) : SV_Target
				{
					return SampleBlur(input.texcoord, float2(0, _MainTex_TexelSize.y));
				}
			ENDHLSL
		}
	}
}
