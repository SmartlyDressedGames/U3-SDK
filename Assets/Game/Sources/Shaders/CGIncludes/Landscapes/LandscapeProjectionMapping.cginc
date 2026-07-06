#ifndef LANDSCAPE_CGINC_INCLUDED
#define LANDSCAPE_CGINC_INCLUDED

#include "Assets/Game/Sources/Shaders/CGIncludes/ProjectionMapping.cginc"

float _Triplanar_Primary_Size;
float _Triplanar_Primary_Weight;

float _Triplanar_Secondary_Size;
float _Triplanar_Secondary_Weight;

float _Triplanar_Tertiary_Size;
float _Triplanar_Tertiary_Weight;

#define landscapeTriplanarBlend triplanarBlend
//float3 landscapeTriplanarBlend(float3 worldPos, float3 worldNormal)
//{
//	return triplanarBlend(worldPos, worldNormal);
//}

float3 landscapePlanarSample3(sampler2D map, float3 worldPos)
{
	return planarSample3(map, worldPos, _Triplanar_Primary_Size);
}

float4 landscapePlanarSample4(sampler2D map, float3 worldPos)
{
	return planarSample4(map, worldPos, _Triplanar_Primary_Size);
}

float3 landscapeTriplanarSample3(sampler2D map, float3 worldPos, float3 blend)
{
	return triplanarSample3(map, worldPos, blend, _Triplanar_Primary_Size);
	//float3 primary = triplanarSample3(map, worldPos, blend, _Triplanar_Primary_Size);
	//float3 secondary = triplanarSample3(map, worldPos, blend, _Triplanar_Secondary_Size);
	//float3 tertiary = triplanarSample3(map, worldPos, blend, _Triplanar_Tertiary_Size);

	//return primary * _Triplanar_Primary_Weight + secondary * _Triplanar_Secondary_Weight + tertiary * _Triplanar_Tertiary_Weight;
}

float4 landscapeTriplanarSample4(sampler2D map, float3 worldPos, float3 blend)
{
	return triplanarSample4(map, worldPos, blend, _Triplanar_Primary_Size);
	//float4 primary = triplanarSample4(map, worldPos, blend, _Triplanar_Primary_Size);
	//float4 secondary = triplanarSample4(map, worldPos, blend, _Triplanar_Secondary_Size);
	//float4 tertiary = triplanarSample4(map, worldPos, blend, _Triplanar_Tertiary_Size);

	//return primary * _Triplanar_Primary_Weight + secondary * _Triplanar_Secondary_Weight + tertiary * _Triplanar_Tertiary_Weight;
}

#endif