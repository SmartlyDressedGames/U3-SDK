Shader "Custom/Intersect"
{
	Properties
	{
		_IntersectColor("Intersect Color", Color) = (1, 1, 1, 1)
		_IntersectSize("Intersect Size", Float) = 2.5
	}

	Subshader
	{
		Tags
		{
			"Queue" = "AlphaTest" 
			"IgnoreProjector" = "True" 
			"RenderType" = "TransparentCutout"
		}

		Cull Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			half4 _IntersectColor;
			half _IntersectSize;
			sampler2D _CameraDepthTexture; //Depth Texture

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float4 ref : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);
				o.ref = ComputeScreenPos(o.pos);
				COMPUTE_EYEDEPTH(o.ref.z);

				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				float sceneZ = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
				float objectZ = i.ref.z;

				float diff = 1 - saturate((sceneZ - objectZ) / _IntersectSize);
		
				clip(diff - 0.5);
				return _IntersectColor;
			}

			ENDCG
		}
	}
}