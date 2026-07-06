////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekWebImage : SleekWrapper
	{
		/// <summary>
		/// If true, SizeOffset_X and SizeOffset_Y are used when image is available.
		/// Defaults to false.
		/// </summary>
		public bool useImageDimensions;

		/// <summary>
		/// If useImageDimensions is on and image width exceeds this value, scale down
		/// respecting aspect ratio.
		/// </summary>
		public float maxImageDimensionsWidth = -1.0f;

		/// <summary>
		/// If useImageDimensions is on and image height exceeds this value, scale down
		/// respecting aspect ratio.
		/// </summary>
		public float maxImageDimensionsHeight = -1.0f;

		public void Refresh(string url, bool shouldCache = true)
		{
			ValidateNotDestroyed();
			Provider.IconQueryParams iconQueryParams = new Provider.IconQueryParams(url, OnImageReady, shouldCache);
			Provider.refreshIcon(iconQueryParams);
		}

		public SleekColor color
		{
			get
			{
				ValidateNotDestroyed();
				return internalImage.TintColor;
			}
			set
			{
				ValidateNotDestroyed();
				internalImage.TintColor = value;
			}
		}

		public override void OnDestroy()
		{
			// Clear reference so callback does not modify potentially released image.
			internalImage = null;
		}

		public void Clear()
		{
			ValidateNotDestroyed();
			internalImage.Texture = null;
		}

		public SleekWebImage()
		{
			internalImage = Glazier.Get().CreateImage();
			internalImage.SizeScale_X = 1.0f;
			internalImage.SizeScale_Y = 1.0f;
			AddChild(internalImage);
		}

		private void OnImageReady(Texture2D icon, bool responsibleForDestroy)
		{
			if (useImageDimensions && icon != null)
			{
				float newWidth = icon.width;
				float newHeight = icon.height;

				if (maxImageDimensionsHeight > 0.5f && newHeight > maxImageDimensionsHeight)
				{
					float aspectRatio = (float) icon.width / (float) icon.height;
					newWidth = maxImageDimensionsHeight * aspectRatio;
					newHeight = maxImageDimensionsHeight;
				}

				if (maxImageDimensionsWidth > 0.5f && newWidth > maxImageDimensionsWidth && icon.height > 0)
				{
					float aspectRatio = (float) icon.width / (float) icon.height;
					newWidth = maxImageDimensionsWidth;
					newHeight = maxImageDimensionsWidth / aspectRatio;
				}

				SizeOffset_X = newWidth;
				SizeOffset_Y = newHeight;
			}

			if (internalImage != null)
			{
				internalImage.SetTextureAndShouldDestroy(icon, responsibleForDestroy);
			}
			else if (responsibleForDestroy)
			{
				Object.Destroy(icon);
			}
		}

		internal ISleekImage internalImage;
	}
}
