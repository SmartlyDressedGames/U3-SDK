////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SDG.Unturned
{
	/// <summary>
	/// "Single-Render" scope as opposed to "Dual-Render" (rendering the scene a second time with a zoomed-in camera).
	/// Blits middle square of the player's view into the viewmodel scope material's render target.
	/// </summary>
	[System.Serializable]
	[PostProcess(typeof(SrScopeRenderer), PostProcessEvent.AfterStack, "Custom/Scope")]
	public sealed class SrScope : PostProcessEffectSettings
	{
		public FloatParameter standardDeviation = new FloatParameter();
		public FloatParameter scopeAlpha = new FloatParameter();

		public TextureParameter renderTarget = new TextureParameter() { };
	}

	public sealed class SrScopeRenderer : PostProcessEffectRenderer<SrScope>
	{
		public override void Init()
		{
			base.Init();

			gaussianBlurShader = Shader.Find("Hidden/Custom/GaussianBlur");
 			vignetteShader = Shader.Find("Hidden/Custom/ScopeVignette");

			scopeBlurTexId = Shader.PropertyToID("_ScopeBlurTex");
			scopeAlphaId = Shader.PropertyToID("_ScopeAlpha");
			standardDeviationId = Shader.PropertyToID("_StdDeviationSquared");
			halfKernelSizeId = Shader.PropertyToID("_HalfKernelSize");
		}

		public override void Render(PostProcessRenderContext context)
		{
			RenderTexture rt = (RenderTexture) settings.renderTarget.value;
			Vector2 screenSize = new Vector2(Screen.width, Screen.height);
			int smallAxis = screenSize.x < screenSize.y ? 0 : 1;
			int bigAxis = 1 - smallAxis;

			// For example: 1920 x 1080
			// smallAxis is 1 (Y) and bigAxis is 0 (X) We need to scale/offset along X axis to make proportions square
			// in the destination.
			Vector2 scale = new Vector2(1.0f, 1.0f);
			scale[bigAxis] = screenSize[smallAxis] / screenSize[bigAxis];

			Vector2 offset = new Vector2(0.0f, 0.0f);
			offset[bigAxis] = (scale[bigAxis] - 1.0f) * -0.5f;

			context.command.Blit(context.source, rt, scale, offset);

			bool blitSrcDest = false;

			if (GraphicsSettings.WantsDarkScopePeripheral)
			{
				if (settings.scopeAlpha > 0.001f)
				{
					PropertySheet vignetteSheet = context.propertySheets.Get(vignetteShader);
					vignetteSheet.properties.SetFloat(scopeAlphaId, settings.scopeAlpha);
					context.command.BlitFullscreenTriangle(context.source, context.destination, vignetteSheet, 0);
				}
				else
				{
					blitSrcDest = true;
				}
			}
			else
			{
				if (settings.standardDeviation > 0.001f)
				{
					// Nelson 2025-06-25: blur doesn't (currently) skip pixels, so higher-DPI screens would be effectively
					// less blurry than low-DPI. To prevent these, we treat 1920x1080 as the baseline resolution and scale
					// blur size. For example, a 4K screen will have 2x blur scale.
					float blurScale = screenSize[smallAxis] / 1080f;
					float stdDeviation = settings.standardDeviation * blurScale;
					float sqrStdDeviation = stdDeviation * stdDeviation;

					context.GetScreenSpaceTemporaryRT(context.command, scopeBlurTexId);

					PropertySheet blurSheet = context.propertySheets.Get(gaussianBlurShader);
					blurSheet.properties.SetFloat(standardDeviationId, sqrStdDeviation);
					blurSheet.properties.SetInt(halfKernelSizeId, Mathf.CeilToInt(stdDeviation * 3.0f));

					context.command.BlitFullscreenTriangle(context.source, scopeBlurTexId, blurSheet, 0);
					context.command.BlitFullscreenTriangle(scopeBlurTexId, context.destination, blurSheet, 1);

					context.command.ReleaseTemporaryRT(scopeBlurTexId);
				}
				else
				{
					blitSrcDest = true;
				}
			}

			if (blitSrcDest)
			{
				// Nelson 2025-06-27: if we don't blit here then there's a black frame flicker with TAA disabled.
				// Unsure why only when TAA disabled, and why it's not constantly black. Want to learn!
				context.command.Blit(context.source, context.destination);
			}
		}

		private Shader gaussianBlurShader;
		private Shader vignetteShader;

		private int scopeBlurTexId;
		private int scopeAlphaId;
		private int standardDeviationId;
		private int halfKernelSizeId;
	}
}
