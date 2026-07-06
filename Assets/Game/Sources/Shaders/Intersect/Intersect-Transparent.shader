Shader "Custom/Intersect-Transparent"
{
	Properties
	{
		_IntersectColor("Intersect Color", Color) = (1, 1, 1, 1)

		// x = Size0
		// y = Alpha0
		// z = Size1
		// w = Alpha1
		_IntersectParams("Intersect Params", Vector) = (1, 1, 1, 1)
	}

	Subshader
	{
		Tags
		{
			"Queue" = "Transparent" 
			"IgnoreProjector" = "True" 
			"RenderType" = "Transparent"
		}

		LOD 200
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _IntersectColor;
			float4 _IntersectParams;
			sampler2D _CameraDepthTexture; //Depth Texture

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float4 texcoord : TEXCOORD0;
				float4 ref : TEXCOORD1;
			};

			v2f vert(appdata_base v)
			{
				v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.ref = ComputeScreenPos(o.pos);
				COMPUTE_EYEDEPTH(o.ref.z);

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float sceneZ = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
				float objectZ = i.ref.z;

				float sharpDepthAlpha = (1 - saturate((sceneZ - objectZ) / _IntersectParams.x)) * _IntersectParams.y;
				float fadeDepthAlpha = (1 - saturate((sceneZ - objectZ) / _IntersectParams.z)) * _IntersectParams.w;
				float depthAlpha = max(sharpDepthAlpha, fadeDepthAlpha);
				float wallAlpha = pow(i.texcoord.y, 2) / 10.0;
				float finalAlpha = max(max(sharpDepthAlpha, fadeDepthAlpha), wallAlpha);

				return float4(_IntersectColor.rgb, finalAlpha);//half4(_IntersectColor.xyz, finalAlpha);
			}

			ENDCG
		}
	}
}
