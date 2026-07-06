// Human and zombie layered clothing textures.
Shader "Standard/Clothes"
{
    Properties
    {
        _SkinColor ("Skin Color", Color) = (1, 1, 1)
		[MaterialToggle] _FlipShirt("Flip Shirt", Float) = 0 // Left handed model is flipped, so un-flip the texture to maintain design.
		_FaceAlbedoTexture("Face Albedo Texture", 2D) = "black" {}
		_FaceEmissionTexture("Face Emission Texture", 2D) = "black" {}
		_ShirtAlbedoTexture("Shirt Albedo Texture", 2D) = "black" {}
		_ShirtEmissionTexture("Shirt Emission Texture", 2D) = "black" {}
		_ShirtMetallicTexture("Shirt Metallic Texture", 2D) = "black" {}
		_PantsAlbedoTexture("Pants Albedo Texture", 2D) = "black" {}
		_PantsEmissionTexture("Pants Emission Texture", 2D) = "black" {}
		_PantsMetallicTexture("Pants Metallic Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags
		{
			"RenderType" = "Opaque"
		}

        LOD 200

        CGPROGRAM

        #pragma surface surf StandardSpecular
        #pragma target 3.0

		float3 _SkinColor;
		float _FlipShirt;
		sampler2D _FaceAlbedoTexture;
		sampler2D _FaceEmissionTexture;
        sampler2D _ShirtAlbedoTexture;
		sampler2D _ShirtEmissionTexture;
		sampler2D _ShirtMetallicTexture;
		sampler2D _PantsAlbedoTexture;
		sampler2D _PantsEmissionTexture;
		sampler2D _PantsMetallicTexture;

        struct Input
        {
            float2 uv_ShirtAlbedoTexture;
        };

        void surf(Input input, inout SurfaceOutputStandardSpecular output)
        {
			float2 faceUV = (input.uv_ShirtAlbedoTexture * 8.0) - float2(6.0, 7.0); // Offset face texture to upper-right.
			float4 faceAlbedo = tex2D(_FaceAlbedoTexture, faceUV);
			float4 faceEmission = tex2D(_FaceEmissionTexture, faceUV);
			float faceMask = step(0.0, faceUV.x) * step(faceUV.x, 1.0) * step(0.0, faceUV.y) * step(faceUV.y, 1.0);
			float faceAlpha = faceAlbedo.a * faceMask;

			// Front of shirt occupies the upper left 1/4, and back of shirt is the next 1/4 to the right.
			float2 shirtUV = input.uv_ShirtAlbedoTexture;
			float2 flipShirtU = ceil(shirtUV.x * 4.0) * 0.25 - frac(shirtUV.x * 4.0) * 0.25;
			float flipShirtAlpha = _FlipShirt * (shirtUV.x < 0.5) * (shirtUV.y > 0.75);
			shirtUV.x = lerp(shirtUV.x, flipShirtU, flipShirtAlpha);

			float4 shirtAlbedo = tex2D(_ShirtAlbedoTexture, shirtUV);
			float4 pantsAlbedo = tex2D(_PantsAlbedoTexture, input.uv_ShirtAlbedoTexture);
			output.Albedo = lerp(lerp(lerp(_SkinColor, faceAlbedo.rgb, faceAlpha), shirtAlbedo.rgb, shirtAlbedo.a), pantsAlbedo.rgb, pantsAlbedo.a);

			float4 shirtEmission = tex2D(_ShirtEmissionTexture, shirtUV);
			float4 pantsEmission = tex2D(_PantsEmissionTexture, input.uv_ShirtAlbedoTexture);
			output.Emission = lerp(lerp(faceEmission.rgb * faceAlpha, shirtEmission.rgb, shirtAlbedo.a), pantsEmission.rgb, pantsAlbedo.a) * 2.0;

			// Nelson 2025-09-10: previously, this actually was Metallic output, but the character
			// was noticeably shiny without ambient light. As I understand it, the "0" metallic
			// corresponds to ~0.04 gray specular, and 1 to the albedo, so we just use albedo as
			// the specular color.
			float4 shirtMetallic = tex2D(_ShirtMetallicTexture, shirtUV);
			float4 pantsMetallic = tex2D(_PantsMetallicTexture, input.uv_ShirtAlbedoTexture);
			output.Specular = output.Albedo.rgb * lerp(shirtMetallic.r * shirtAlbedo.a, pantsMetallic.r, pantsAlbedo.a);
			output.Smoothness = lerp(shirtMetallic.a * shirtAlbedo.a, pantsMetallic.a, pantsAlbedo.a);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
