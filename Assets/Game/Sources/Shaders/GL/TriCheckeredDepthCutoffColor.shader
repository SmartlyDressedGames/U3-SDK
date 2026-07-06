Shader "GL/TriCheckeredDepthCutoffColor"
{
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
		}

		Pass
		{
			ZWrite Off
			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _CameraDepthTexture;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ref : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(v.vertex);
				OUT.color = v.color;
				OUT.ref = ComputeScreenPos(OUT.vertex);
				COMPUTE_EYEDEPTH(OUT.ref.z);

				return OUT;
			}

			float4 frag(v2f IN) : SV_Target
			{
				clip(frac((IN.ref.x + IN.ref.y) / IN.ref.w * 64) - 0.5);

				float sceneDepth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.ref)).r);
				float objectDepth = IN.ref.z;

				// 0 = close, 1 = far
				// if object > scene this will be clipped
				clip(sceneDepth - objectDepth);
				return IN.color;
			}

			ENDCG
		}
	}
}