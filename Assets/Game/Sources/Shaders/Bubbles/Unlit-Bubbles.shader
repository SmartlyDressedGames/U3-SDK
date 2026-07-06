// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Unlit shader. Simplest possible colored shader.
// - no lighting
// - no lightmap support
// - no texture

Shader "Unlit/Bubbles" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
	Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
	LOD 200
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			fixed4 _Color;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{
				fixed4 c = tex2D(_MainTex, i.uv) * _Color;
				fixed4 col = UNITY_LIGHTMODEL_AMBIENT*c;//_Color;
				clip(-0.5 + col.a);
				return col;
			}
		ENDCG
	}
}

}
