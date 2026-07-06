// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Skybox/Sky"
{
	Properties
	{
		_SkyColor ("Sky Color", Color) = (.5, .5, .5, 1)
		_EquatorColor ("Equator Color", Color) = (.5, .5, .5, 1)
		_GroundColor ("Ground Color", Color) = (.5, .5, .5, 1)
		_SkyHackAmbientGround("Ambient Ground", Color) = (0.8, 0.8, 0.8, 1)
		_SkyHackAmbientEquator("Ambient Equator", Color) = (0.8, 0.8, 0.8, 1)
		_SunDirection ("Sun Direction", Vector) = (0.0, 0.0, 1.0)
		_SunColor("Sun Color", Color) = (1.0, 0.5, 0.0, 1.0)
		_SunInnerThreshold("Sun Inner Threshold", Float) = 0.9
		_SunOuterThreshold("Sun Outer Threshold", Float) = 0.8
		_StarsTexture("Stars", 2D) = "white" {}
		_StarsCutoff("Stars Cutoff", Float) = 1.0
		_MoonDirection("Moon Direction", Vector) = (0.0, 0.0, -1.0)
		_MoonLightDirection("Moon Light Direction", Vector) = (0.0, 1.0, 0.0)
		_MoonColor("Moon Color", Color) = (1.0, 0.0, 1.0, 1.0)
		_SqrMoonRadius("Sqr Moon Radius", Float) = 0.01
		_AuroraBorealisColorTexture("Aurora Borealis Color", 2D) = "white" {}
		_AuroraBorealisAlphaTexture("Aurora Borealis Alpha", 2D) = "white" {}
		_AuroraBorealisIntensity("Aurora Borealis Intensity", Float) = 1
		_CloudsTexture("Clouds", 2D) = "white" {}
		_CloudColor("Cloud Color", Color) = (.5, .5, .5, 1)
		_CloudRimColor("Cloud Rim Color", Color) = (.8, .6, 0.4, 1)
		_CloudIntensity("Cloud Intensity", Float) = 1
		_CloudParams("Cloud Parameters", Vector) = (0.5, 3.0, 0.0, 0.0)
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Background"
			"RenderType" = "Background"
			"PreviewType" = "Skybox"
		}

		Cull Off
		ZWrite Off

		Pass
		{
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			#pragma multi_compile __ UNITY_COLORSPACE_GAMMA
			#pragma multi_compile __ WITH_AURORA_BOREALIS
			#pragma multi_compile __ WITH_CLOUDS
			#pragma multi_compile __ WITH_STARS

			uniform half3 _SkyColor;
			uniform half3 _EquatorColor;
			uniform half3 _GroundColor;

			uniform float3 _SkyHackAmbientGround;
			uniform float3 _SkyHackAmbientEquator;

			uniform float3 _SunDirection; // Normal of the sun directional light source.
			uniform float3 _SunColor; // Tinted color controlled by level lighting.
			uniform float _SunInnerThreshold;
			uniform float _SunOuterThreshold;

			sampler2D _StarsTexture;
			uniform float _StarsCutoff;

			uniform float3 _MoonDirection;
			uniform float3 _MoonLightDirection;
			uniform float3 _MoonColor;
			uniform float _SqrMoonRadius;

			sampler2D _AuroraBorealisColorTexture;
			sampler2D _AuroraBorealisAlphaTexture;
			uniform float _AuroraBorealisIntensity;

			sampler2D _CloudsTexture;

			// Highlight colors change with time of day and are affected by weather, e.g. rain darkens and snow brightens.
			uniform float3 _CloudColor;
			uniform float3 _CloudRimColor;
			// [0, 1] Vanilla maps have relatively low (~0.1) intensity values, so treat as increasing the cloud coverage.
			uniform float _CloudIntensity;
			// R: macro alpha cutoff
			// G: macro alpha saturation
			uniform float4 _CloudParams;

			uniform fixed _AtmosphericFog;

			#if defined(UNITY_COLORSPACE_GAMMA)
			#define GAMMA 2
			#define COLOR_2_GAMMA(color) color
			#define COLOR_2_LINEAR(color) color*color
			#define LINEAR_2_OUTPUT(color) sqrt(color)
			#else
			#define GAMMA 2.2
			// HACK: to get gfx-tests in Gamma mode to agree until UNITY_ACTIVE_COLORSPACE_IS_GAMMA is working properly
			#define COLOR_2_GAMMA(color) ((unity_ColorSpaceDouble.r>2.0) ? pow(color,1.0/GAMMA) : color)
			#define COLOR_2_LINEAR(color) color
			#define LINEAR_2_LINEAR(color) color
			#endif

			struct appdata_t
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				half3 rayDir : TEXCOORD0;	// Vector for incoming ray, normalized ( == -eyeRay )
   			}; 

			v2f vert (appdata_t v)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(v.vertex);

				// Unity's built-in skybox shaders convert to world space, but it does not really matter because we
				// normalize in the fragment shader.
				OUT.rayDir = -v.vertex.xyz;
				return OUT;
			}

#ifdef WITH_AURORA_BOREALIS
			float3 sampleAuroraBorealis(float3 viewDir)
			{
				float3 resultColor = 0.0;
				const float minAuroraBorealisY = 0.1;
				const float maxAuroraBorealisY = 0.5;
				const float auroraBorealisHeight = maxAuroraBorealisY - minAuroraBorealisY;
				for (int index = 0; index < 10; ++index)
				{
					float offsetWave = cos(viewDir.z * 5 + index + _Time.y * 0.25);
					float3 planePositionRelativeToView = float3(0.0, 0.0, -0.45 + index * 0.1 + offsetWave * 0.02);
					float3 planeNormal = float3(0.0, 0.0, 1.0); // Matches older mesh approach.
					float distToPlane = dot(planePositionRelativeToView, planeNormal) / dot(viewDir, planeNormal);
					float3 hitPosition = viewDir * distToPlane;
					float minX = -0.9 + offsetWave * 0.2;
					float maxX = 0.9 + offsetWave * 0.2;
					if (distToPlane > 0.0 && hitPosition.y > minAuroraBorealisY && hitPosition.y < maxAuroraBorealisY && hitPosition.x > minX && hitPosition.x < maxX)
					{
						float normalizedU = (hitPosition.x - minX) / (maxX - minX);
						float normalizedV = (hitPosition.y - minAuroraBorealisY) / auroraBorealisHeight;
						float2 alphaTexcoord = float2(hitPosition.x * 0.02 + index * 0.1 + _Time.x * 0.1, normalizedV);
						float alpha = tex2D(_AuroraBorealisAlphaTexture, alphaTexcoord).a;

						// Older mesh used vertex colors to fade out the ends. Instead we fade out 25% at each end.
						float distFromCenterU = abs(0.5 - normalizedU) * 2.0; // [0, 1]
						alpha *= saturate(2.0 - distFromCenterU * 2.0);

						alpha *= abs(sin(normalizedU * 0.01 + normalizedV * 0.1 + index * 2.0 + _Time.x));

						float2 colorTexcoord = float2(index * 0.01 + _Time.x * 0.2, 0.5);
						float3 color = tex2D(_AuroraBorealisColorTexture, colorTexcoord).rgb;

						resultColor += color * alpha;
					}
				}

				return resultColor;
			}
#endif // WITH_AURORA_BOREALIS

#ifdef WITH_CLOUDS
			float3 blendClouds(float3 col, float3 viewDir)
			{
				// Note: particle clouds traveled along world +Z axis which is -Y UV axis.
				float2 texcoord = viewDir.xz / viewDir.y;

				// Higher-contrast large-scale cloud shapes.
				float macroAlpha = tex2D(_CloudsTexture, texcoord * 0.1 - float2(0.0, _Time.x * 0.01)).r;
				// Vanilla maps have relatively low (~0.1) intensity values, so treat it as increasing the cloud coverage.
				macroAlpha += _CloudIntensity * 0.25 * tex2D(_CloudsTexture, texcoord * 0.1 + 0.5 - float2(0.0, _Time.x * 0.01)).r;
				// Originally I experimented with making macro texture high contrast in the image editor, but that
				// makes the intensity transition more obvious. Instead we increase contrast procedurally here.
				macroAlpha = saturate((macroAlpha - _CloudParams.r) * _CloudParams.g);

				// 1 while sun is above, and still above zero while slight below horizon, but zero far below horizon.
				float sunAtmosphereFactor = saturate(_SunDirection.y * -2.0 + 1.0);
				// 1 while near view ray, and still above zero past 90 degrees, but zero while opposite.
				float sunViewFactor = saturate(0.5 - dot(viewDir, _SunDirection));
				float sunFactor = sunAtmosphereFactor * sunViewFactor;

				// 1 while moon is above, and still above zero while slight below horizon, but zero far below horizon.
				float moonAtmosphereFactor = saturate(_MoonDirection.y * -2.0 + 1.0);
				// 1 while near view ray, and zero near perpendicular.
				float moonViewFactor = saturate(-dot(viewDir, _MoonDirection));
				float moonFactor = moonAtmosphereFactor * moonViewFactor;

				float cloudsMedium = tex2D(_CloudsTexture, texcoord * 0.2 - float2(0.0, _Time.x * 0.04)).g;
				float cloudsSmall = tex2D(_CloudsTexture, texcoord - float2(0.0, _Time.x * 0.2)).b;
				float3 cloudBodyColor = _SkyHackAmbientGround.rgb + _CloudRimColor.rgb;
				cloudBodyColor = lerp(cloudBodyColor, _SunColor, sunFactor * cloudsMedium * 0.5);
				cloudBodyColor = lerp(cloudBodyColor, _MoonColor, moonFactor * cloudsMedium * 0.05);

				float3 cloudRimColor = _SkyHackAmbientEquator.rgb + _EquatorColor.rgb + _CloudRimColor.rgb;
				cloudRimColor = lerp(cloudRimColor, _SunColor, sunFactor);
				cloudRimColor = lerp(cloudRimColor, _MoonColor, moonFactor * 0.25);

				float cloudsBodyAlpha = saturate(macroAlpha + macroAlpha * cloudsMedium + macroAlpha * cloudsSmall);

				// Fade out clouds towards the horizon. (and below horizon)
				float cloudsAlpha = cloudsBodyAlpha * saturate(viewDir.y * 2.0);

				return lerp(col, lerp(cloudRimColor, cloudBodyColor, cloudsBodyAlpha), cloudsAlpha);
			}
#endif // WITH_CLOUDS

			half4 frag (v2f IN) : SV_Target
			{
				float3 rayDir = normalize(IN.rayDir);
				float3 viewDir = -rayDir;

				float3 col = float3(0.0, 0.0, 0.0);
				// < 0.0 means over horizon, add a bit because we need to lerp to hide the horizon line
				half scale = 1 - pow(1 - clamp(abs(rayDir.y), 0, 1), 4);

				// Nelson 2025-09-10: fixing sun in skybox reflection I thought this needed to be
				// smooth, but a harsh cutoff matches SkyFog.
				float overHorizonMask;

				// If adjusting skybox gradient please remember to update SkyFog post process effect!
				if(rayDir.y < 0)
				{
					col = COLOR_2_LINEAR(lerp(_EquatorColor, _SkyColor, scale));
					overHorizonMask = 1.0;
				}
				else
				{
					col = COLOR_2_LINEAR(lerp(_EquatorColor, _GroundColor, scale));
					overHorizonMask = 0.0;
				}

				#if defined(UNITY_COLORSPACE_GAMMA)
					col = LINEAR_2_OUTPUT(col);
				#endif

				float sunAlignment = dot(rayDir, _SunDirection);
				float sunAlpha = smoothstep(_SunOuterThreshold, _SunInnerThreshold, sunAlignment);
				sunAlpha *= overHorizonMask;
				// Reduce intensity as fog increases, otherwise the sun is unusually bright through the fog.
				float sunIntensity = lerp(4.0, 1.0, _AtmosphericFog);

#ifdef WITH_STARS
				// Not sure who to credit for this infinite plane projection trick.
				// (I saw it in a blog post referencing a dead link to a Shader Forge post)
				float2 starsCoord = rayDir.xz / rayDir.y;
				starsCoord.x += _Time.x * 0.01;
				starsCoord.y += _Time.y * 0.004;
				float4 starsColor = tex2D(_StarsTexture, starsCoord * 0.6);
				float starsMask = saturate(-rayDir.y);
#endif // WITH_STARS

				float3 moonCenter = -_MoonDirection; // Moon position relative to view ray origin (0, 0, 0).
				float moonCenterDistAlongView = dot(viewDir, moonCenter);
				float moonMask = moonCenterDistAlongView > 0.0;
				moonMask *= overHorizonMask;
				// We now have a right-angle triangle where the hypotenuse is the distance from the view ray origin
				// (0, 0, 0) to the moon center (1.0) and the adjacent is the distance along the ray to the projected
				// center. We can solve for the remaining side length which is the distance from the center to the
				// nearest point along the view ray.
				float sqrDistFromViewOriginToMoonCenter = 1.0;
				float sqrMoonCenterDistAlongView = moonCenterDistAlongView * moonCenterDistAlongView;
				float sqrDistFromMoonToNearestPointAlongViewRay = sqrDistFromViewOriginToMoonCenter - sqrMoonCenterDistAlongView;
				moonMask *= sqrDistFromMoonToNearestPointAlongViewRay < _SqrMoonRadius;
				// We now have another right angle triangle. The hypotenuse is the moon radius,
				// the known side length is the distance between the moon center and the nearest point on the
				// ray, so we can solve the distance between the nearest point on the ray and the hit.
				float distWithinMoon = sqrt(_SqrMoonRadius - sqrDistFromMoonToNearestPointAlongViewRay);
				float hitDistAlongViewRay = moonCenterDistAlongView - distWithinMoon;
				float3 moonHitPosition = viewDir * hitDistAlongViewRay;
				float3 moonHitNormal = normalize(moonHitPosition - moonCenter);
				float ndotl = saturate(dot(moonHitNormal, -_MoonLightDirection));

#ifdef WITH_STARS
				// Stars are obstructed by moon.
				starsMask *= (1.0 - moonMask);
				col = lerp(col, starsColor.rgb, max(0.0, starsColor.a - _StarsCutoff) * starsMask);
#endif // WITH_STARS

				col = lerp(col, _SunColor.rgb * sunIntensity, sunAlpha);
				col = lerp(col, _MoonColor.rgb, moonMask * ndotl);

#ifdef WITH_AURORA_BOREALIS
				col += sampleAuroraBorealis(viewDir) * _AuroraBorealisIntensity;
#endif // WITH_AURORA_BOREALIS

#ifdef WITH_CLOUDS
				col = blendClouds(col, viewDir);
#endif // WITH_CLOUDS

				col = lerp(col, unity_FogColor.rgb, _AtmosphericFog);
				return half4(col,1.0);

			}
			ENDCG 
		}
	} 	

	Fallback Off
}
