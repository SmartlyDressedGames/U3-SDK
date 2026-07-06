// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see THIRDPARTYNOTICES.txt)

#ifndef FOLIAGE_WIND_CGINC_INCLUDED
#define FOLIAGE_WIND_CGINC_INCLUDED

float4 _Grass_Displacement_Point;

// Taken from the unity terrain engine (TerrainEngine.cginc)
// Input has to be in the range 0 to 1
// Calculates the sin and cos of each input value xyzw
void windFastSinCos(float4 input, out float4 inputSin, out float4 inputCos) 
{
	input = input * 6.408849 - 3.1415927;

	// powers for taylor series
	float4 r5 = input * input;					// wavevec ^ 2
	float4 r6 = r5 * r5;						// wavevec ^ 4;
	float4 r7 = r6 * r5;						// wavevec ^ 6;
	float4 r8 = r6 * r5;						// wavevec ^ 8;

	float4 r1 = r5 * input;						// wavevec ^ 3
	float4 r2 = r1 * r5;						// wavevec ^ 5;
	float4 r3 = r2 * r5;						// wavevec ^ 7;

	// Vectors for taylor's series expansion of sin and cos
	float4 sin7 = {1, -0.16161616, 0.0083333, -0.00019841};
	float4 cos8 = {-0.5, 0.041666666, -0.0013888889, 0.000024801587};

	// sin
	inputSin = input + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;

	// cos
	inputCos = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
}

void windAnimation(inout float4 worldPos, float windInfluence)
{
#ifdef GRASS_DISPLACEMENT_ON
	float3 displacementOffset = worldPos.xyz - _Grass_Displacement_Point.xyz;
	float displacementPower = 1.0 - saturate((pow(displacementOffset.x, 2) + pow(displacementOffset.y, 2) + pow(displacementOffset.z, 2)) / 6.25);
	worldPos.y -= displacementPower;
	displacementOffset = normalize(displacementOffset) * displacementPower * windInfluence;
	worldPos.xz += displacementOffset.xz;
#endif

	float4 waveXSize = float4(0.012, 0.02, 0.06, 0.024) * 0.1;
	float4 waveZSize = float4 (0.006, .02, 0.02, 0.05) * 0.1;
	float4 waveSpeed = float4 (0.3, .5, .4, 1.2) * 0.15;

	float4 _waveXmove = float4(0.012, 0.02, -0.06, 0.048) * 2;
	float4 _waveZmove = float4 (0.006, .02, -0.02, 0.1);

	float4 waves;
	waves = worldPos.x * waveXSize;
	waves += worldPos.z * waveZSize;

	// Add in time to model them over time
	waves += _Time.y * waveSpeed;

	float4 sin;
	float4 cos;
	waves = frac(waves);
	windFastSinCos(waves, sin, cos);

	sin = sin * sin;
	sin = sin * sin;
	sin = sin * windInfluence;

	float3 waveMove = float3(0, 0, 0);
	waveMove.x = dot(sin, _waveXmove);
	waveMove.z = dot(sin, _waveZmove);

	worldPos.xz += waveMove.xz * 4;
}

#endif
