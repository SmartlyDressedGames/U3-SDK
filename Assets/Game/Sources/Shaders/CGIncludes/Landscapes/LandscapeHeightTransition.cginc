#ifndef LANDSCAPE_HEIGHT_TRANSITION_CGINC_INCLUDED
#define LANDSCAPE_HEIGHT_TRANSITION_CGINC_INCLUDED

float _Fade;

void splatmapFinalColor(Input IN, TERRAIN_SURFACE_OUTPUT OUT, inout fixed4 color)
{
	color *= OUT.Alpha;
	UNITY_APPLY_FOG(IN.fogCoord, color);
}

void splatmapFinalPrepass(Input IN, TERRAIN_SURFACE_OUTPUT OUT, inout fixed4 normalSpec)
{
#ifdef TERRAIN_SPLAT_ADDPASS
	normalSpec *= OUT.Alpha;
#endif
}

void splatmapFinalGBuffer(Input IN, TERRAIN_SURFACE_OUTPUT OUT, inout half4 diffuse, inout half4 specSmoothness, inout half4 normal, inout half4 emission)
{
#ifdef TERRAIN_SPLAT_ADDPASS
	diffuse *= OUT.Alpha;
	specSmoothness *= OUT.Alpha;
	emission *= OUT.Alpha;
#endif
}

#endif
