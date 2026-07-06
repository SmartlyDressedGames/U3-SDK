#ifndef LANDSCAPE_LINEAR_TRANSITION_CGINC_INCLUDED
#define LANDSCAPE_LINEAR_TRANSITION_CGINC_INCLUDED

void splatmapFinalColor(Input IN, TERRAIN_SURFACE_OUTPUT OUT, inout fixed4 color)
{
	color *= OUT.Alpha;
#ifdef TERRAIN_SPLAT_ADDPASS
	UNITY_APPLY_FOG_COLOR(IN.fogCoord, color, fixed4(0,0,0,0));
#else
	UNITY_APPLY_FOG(IN.fogCoord, color);
#endif
}

void splatmapFinalPrepass(Input IN, TERRAIN_SURFACE_OUTPUT OUT, inout fixed4 normalSpec)
{
	normalSpec *= OUT.Alpha;
}

void splatmapFinalGBuffer(Input IN, TERRAIN_SURFACE_OUTPUT OUT, inout half4 diffuse, inout half4 specSmoothness, inout half4 normal, inout half4 emission)
{
	diffuse.rgb *= OUT.Alpha;
	specSmoothness *= OUT.Alpha;
	normal.rgb *= OUT.Alpha;
	emission *= OUT.Alpha;
}

#endif
