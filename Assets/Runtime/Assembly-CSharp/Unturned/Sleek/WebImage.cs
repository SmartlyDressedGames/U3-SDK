////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned
{
	public class WebImage : MonoBehaviour
	{
		public Image targetImage;
		public string url;
		public bool shouldCache = true;

		/// <summary>
		/// If set, we are responsible for destroying texture.
		/// </summary>
		protected Texture2D texture;
		protected Sprite sprite;

		public void setAddressAndRefresh(string newURL, bool newShouldCache, bool forceRefresh)
		{
			if (forceRefresh)
			{
				Provider.destroyCachedIcon(newURL);
			}
			else
			{
				if (url != null && shouldCache && newShouldCache)
				{
					if (url.Equals(newURL, System.StringComparison.InvariantCultureIgnoreCase))
					{
						return;
					}
				}
			}

			url = newURL;
			shouldCache = newShouldCache;
			Refresh();
		}

		private void onImageReady(Texture2D texture, bool responsibleForDestroy)
		{
			cleanupResources();

			if (responsibleForDestroy)
			{
				this.texture = texture;
			}

			// Re-enable image with old sprite regardless of whether texture is valid.
			targetImage.enabled = true;

			if (texture != null)
			{
				sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
				sprite.name = texture.name + "Sprite";
				sprite.hideFlags = HideFlags.HideAndDontSave;
				targetImage.sprite = sprite;

				AspectRatioFitter fitter = GetComponent<AspectRatioFitter>();
				if (fitter != null)
				{
					fitter.aspectRatio = texture.width / (float) texture.height;
				}
			}
		}

		public void Refresh()
		{
			if (targetImage == null)
				return;

			targetImage.enabled = false; // Should not be visible until image is loaded.

			if (string.IsNullOrEmpty(url))
				return;

			Provider.IconQueryParams iconQueryParams = new Provider.IconQueryParams(url, onImageReady, shouldCache);
			Provider.refreshIcon(iconQueryParams);
		}

		protected void cleanupResources()
		{
			if (texture != null)
			{
				Destroy(texture);
				texture = null;
			}

			if (sprite != null)
			{
				Destroy(sprite);
				sprite = null;
			}
		}

		protected virtual void Start()
		{
			Refresh();
		}

		protected void OnDestroy()
		{
			cleanupResources();
		}
	}
}
