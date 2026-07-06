////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// See Bundles.getIconsBundle.
	/// </summary>
	public struct IconsBundle
	{
		/// <summary>
		/// In practice, T is a Texture2D or a Sprite.
		/// </summary>
		public T load<T>(string name) where T : Object
		{
			T result = null;
			if (Assets.coreMasterBundle != null)
			{
				result = Assets.coreMasterBundle.LoadAsset<T>($"{path}/{name}.png");
			}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			else
			{
				UnturnedLog.warn("Cannot load built-in icons because Assets.coreMasterBundle is null");
			}

			if (result == null)
			{
				UnturnedLog.warn($"Missing icon {path}/{name}.png");
			}
#endif //  UNITY_EDITOR || DEVELOPMENT_BUILD

			return result;
		}

		public IconsBundle(string path)
		{
			this.path = path;
		}

		private string path;
	}
}
