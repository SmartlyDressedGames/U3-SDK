#ifndef RAIN_CGINC_INCLUDED
#define RAIN_CGINC_INCLUDED

#include "UnityCG.cginc"

static float rippleFrequency = 4; // Display on every 4th cycle

sampler2D _Rain_Puddle_Map;
sampler2D _Rain_Ripple_Map;
float _Rain_Water_Level;
float _Rain_Intensity;
float _Rain_Min_Height;

void rain(float3 worldPos, float3 worldNormal, float mask, inout half3 Albedo, inout half Metallic, inout half Smoothness)
{
	mask *= saturate(worldPos.y - _Rain_Min_Height); // Fade out puddles within 1 meter of ocean

	// Grab puddle sample
	// A channel is the height of the puddle
	float puddleSample = tex2D(_Rain_Puddle_Map, worldPos.xz / 64).a;
	puddleSample = lerp(1, puddleSample, mask); // Lower (or 0) water height in masked areas
	float puddle = saturate((_Rain_Water_Level - puddleSample) / 0.05) * saturate(worldNormal.y);

	Metallic = puddle * 0.4;
	Smoothness = saturate(puddle * 4);

	// Grab ripple texture
	float3 rippleSample_0 = tex2D(_Rain_Ripple_Map, worldPos.xz / 5);
	// R channel is 0 in the middle, 1 in the outer edge
	float rippleGradient_0 = rippleSample_0.r;
	// G channel prevents all raindrops from occuring at the same time
	float rippleTimeOffset_0 = _Time.y * 1.275 / rippleFrequency + rippleSample_0.g + worldPos.x / 21 + worldPos.z / 12;
	// B channel masks out the areas that have ripples
	float rippleMask_0 = rippleSample_0.b;

	float ripple_0 = (1.0 - saturate(abs(rippleGradient_0 - frac(rippleTimeOffset_0) * rippleFrequency) / 0.05)) * pow(1.0 - rippleGradient_0, 2) * rippleMask_0;
	ripple_0 *= saturate(_Rain_Intensity);

	float3 rippleSample_1 = tex2D(_Rain_Ripple_Map, worldPos.xz / 4 + 70);
	float rippleGradient_1 = rippleSample_1.r;
	float rippleTimeOffset_1 = _Time.y * 1.521 / rippleFrequency + rippleSample_1.g + 0.5 + worldPos.x / 9 - worldPos.z / 17;
	float rippleMask_1 = rippleSample_1.b;

	float ripple_1 = (1.0 - saturate(abs(rippleGradient_1 - frac(rippleTimeOffset_1) * rippleFrequency) / 0.05)) * pow(1.0 - rippleGradient_1, 2) * rippleMask_1;
	ripple_1 *= saturate(_Rain_Intensity - 1);

	float ripple = saturate(ripple_0 + ripple_1) * puddle;

	Albedo = saturate(Albedo + ripple * 0.7);
}

void rainSpecular(float3 worldPos, float3 worldNormal, float mask, inout half3 Albedo, inout half3 Specular, inout half Smoothness)
{
	half Metallic;
	rain(worldPos, worldNormal, mask, Albedo, Metallic, Smoothness);
	Specular = half3(Metallic, Metallic, Metallic);
}

#endif
