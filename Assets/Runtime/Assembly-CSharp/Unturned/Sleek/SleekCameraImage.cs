////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekCameraImage : SleekWrapper
	{
		public void SetCamera(Camera camera)
		{
			if (targetCamera != null)
			{
				DestroyRenderTexture();
			}

			targetCamera = camera;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			if (targetCamera == null)
			{
				return;
			}

			Vector2 dimensions = GetAbsoluteSize();
			int width = Mathf.CeilToInt(dimensions.x);
			int height = Mathf.CeilToInt(dimensions.y);
			if (width < 1 || height < 1)
			{
				return;
			}

			if (renderTexture != null && (renderTexture.width != width || renderTexture.height != height))
			{
				// Nelson 2023-08-29: tested and the engine does not support changing these properties after creation.
				DestroyRenderTexture();
			}

			if (renderTexture == null)
			{
				var graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB;
				var depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D24_UNorm_S8_UInt;
				renderTexture = new RenderTexture(width, height, graphicsFormat, depthStencilFormat);
				renderTexture.hideFlags = HideFlags.HideAndDontSave;
				renderTexture.filterMode = FilterMode.Point;
				targetCamera.targetTexture = renderTexture;
				internalImage.Texture = renderTexture;
			}
		}

		public override void OnDestroy()
		{
			DestroyRenderTexture();
			base.OnDestroy();
		}

		public SleekCameraImage()
		{
			internalImage = Glazier.Get().CreateImage();
			internalImage.SizeScale_X = 1.0f;
			internalImage.SizeScale_Y = 1.0f;
			AddChild(internalImage);
		}

		private void DestroyRenderTexture()
		{
			if (targetCamera != null)
			{
				targetCamera.targetTexture = null;
			}

			if (internalImage != null)
			{
				internalImage.Texture = null;
			}

			if (renderTexture != null)
			{
				Object.Destroy(renderTexture);
				renderTexture = null;
			}
		}

		public ISleekImage internalImage;

		private RenderTexture renderTexture;
		private Camera targetCamera;
	}
}
