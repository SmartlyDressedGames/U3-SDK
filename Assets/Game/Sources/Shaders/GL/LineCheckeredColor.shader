Shader "GL/LineCheckeredColor"
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
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

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

				return OUT;
			}

			float4 frag(v2f IN) : SV_Target
			{
				clip(frac((IN.ref.x + IN.ref.y) / IN.ref.w * 64) - 0.5);
				return IN.color;
			}

			ENDCG
		}
	}
}