#ifndef PROJECTION_MAPPING_CGINC_INCLUDED
#define PROJECTION_MAPPING_CGINC_INCLUDED

float3 triplanarBlend(float3 worldPos, float3 worldNormal)
{
	float3 blend = abs(worldNormal);
	float sum = blend.x + blend.y + blend.z;
	blend /= sum;

	return blend;
}

float3 triplanarBlend(float3 worldPos, float3 worldNormal, float sharpness)
{
	float3 blend = pow(abs(worldNormal), sharpness);
	float sum = blend.x + blend.y + blend.z;
	blend /= sum;

	return blend;
}

float3 planarSample3(sampler2D map, float3 worldPos, float scale)
{
	float3 sample_y = tex2D(map, worldPos.xz / scale);
	return sample_y;
}

float4 planarSample4(sampler2D map, float3 worldPos, float scale)
{
	float4 sample_y = tex2D(map, worldPos.xz / scale);
	return sample_y;
}

float3 triplanarSample3(sampler2D map, float3 worldPos, float3 blend, float scale)
{
	float3 sample_x = tex2D(map, worldPos.zy / scale);
	float3 sample_y = tex2D(map, worldPos.xz / scale);
	float3 sample_z = tex2D(map, worldPos.xy / scale);

	return sample_x * blend.x + sample_y * blend.y + sample_z * blend.z;
}

float4 triplanarSample4(sampler2D map, float3 worldPos, float3 blend, float scale)
{
	float4 sample_x = tex2D(map, worldPos.zy / scale);
	float4 sample_y = tex2D(map, worldPos.xz / scale);
	float4 sample_z = tex2D(map, worldPos.xy / scale);

	return sample_x * blend.x + sample_y * blend.y + sample_z * blend.z;
}

#endif