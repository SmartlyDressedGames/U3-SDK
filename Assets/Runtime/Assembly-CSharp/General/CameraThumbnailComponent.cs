////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEngine;
using Unturned.UnityEx;

namespace SDG.Unturned
{
	public class CameraThumbnailComponent : MonoBehaviour
	{
		public Camera cameraComponent;
		public float delay = 3.0f;

		private IEnumerator Render(int width, int height, string exportFilePath)
		{
			yield return new WaitForEndOfFrame();

			RenderTexture blackTargetTexture = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			cameraComponent.targetTexture = blackTargetTexture;
			cameraComponent.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			cameraComponent.Render();

			RenderTexture.active = blackTargetTexture;
			// Copy rendered data from GPU to CPU texture.
			Texture2D blackTexture = new Texture2D(width, height, TextureFormat.ARGB32, /*mipChain*/ false, /*linear*/ false);
			blackTexture.filterMode = FilterMode.Point;
			blackTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			//blackTexture.Apply(/*updateMipmaps*/ false, /*makeNoLongerReadable*/ false);

			cameraComponent.targetTexture = null; // Must reset before releasing.
			RenderTexture.ReleaseTemporary(blackTargetTexture);

			byte[] exportData = blackTexture.EncodeToPNG();
			Destroy(blackTexture);
			File.WriteAllBytes(exportFilePath, exportData);
		}

		private IEnumerator RenderGreenScreen(int width, int height, string exportFilePath)
		{
			yield return new WaitForEndOfFrame();

			RenderTexture blackTargetTexture = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			cameraComponent.targetTexture = blackTargetTexture;
			cameraComponent.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
			cameraComponent.Render();

			RenderTexture greenTargetTexture = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			cameraComponent.targetTexture = greenTargetTexture;
			cameraComponent.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			cameraComponent.Render();

			RenderTexture.active = blackTargetTexture;
			// Copy rendered data from GPU to CPU texture.
			Texture2D blackTexture = new Texture2D(width, height, TextureFormat.ARGB32, /*mipChain*/ false, /*linear*/ false);
			blackTexture.filterMode = FilterMode.Point;
			blackTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			blackTexture.Apply(/*updateMipmaps*/ false, /*makeNoLongerReadable*/ false);

			RenderTexture.active = greenTargetTexture;
			Texture2D greenTexture = new Texture2D(width, height, TextureFormat.ARGB32, /*mipChain*/ false, /*linear*/ false);
			greenTexture.filterMode = FilterMode.Point;
			greenTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			greenTexture.Apply(/*updateMipmaps*/ false, /*makeNoLongerReadable*/ false);

			// This is an inefficient hack to capture transparent effects that do not work properly with alpha channel.
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					Color actualColor = blackTexture.GetPixel(x, y);
					Color greenColor = greenTexture.GetPixel(x, y);

					// For example if black rendered 0.25 and actual rendered 1.0
					// this suggests the black color was 25% transparent.
					float redTransparency = 1.0f - (greenColor.r - actualColor.r);
					float greenTransparency = 1.0f - (greenColor.g - actualColor.g);
					float blueTransparency = 1.0f - (greenColor.b - actualColor.b);
					if (redTransparency > 0.0f)
					{
						actualColor.r /= redTransparency;
					}
					if (greenTransparency > 0.0f)
					{
						actualColor.g /= greenTransparency;
					}
					if (blueTransparency > 0.0f)
					{
						actualColor.b /= blueTransparency;
					}
					actualColor.a = redTransparency + greenTransparency + blueTransparency;
					blackTexture.SetPixel(x, y, actualColor);
				}
			}

			cameraComponent.targetTexture = null; // Must reset before releasing.
			RenderTexture.ReleaseTemporary(blackTargetTexture);
			RenderTexture.ReleaseTemporary(greenTargetTexture);

			byte[] exportData = blackTexture.EncodeToPNG();
			Destroy(blackTexture);
			Destroy(greenTexture);
			File.WriteAllBytes(exportFilePath, exportData);
		}

		private IEnumerator Start()
		{
			yield return new WaitForSeconds(delay);
			//yield return Render(400, 400, UnityPaths.ProjectDirectory.FullName + "/Temp/Icon_Large_Game.png");
			yield return Render(4096, 4096, UnityPaths.ProjectDirectory.FullName + "/Temp/Icon_Large_Steam.png");
		}
	}
}
#endif // UNITY_EDITOR
