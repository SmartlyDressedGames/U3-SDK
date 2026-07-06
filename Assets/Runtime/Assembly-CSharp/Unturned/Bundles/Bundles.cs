////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Bundles : MonoBehaviour
	{
		private static bool _isInitialized;
		public static bool isInitialized => _isInitialized;

		/// <summary>
		/// 2026-05-11: previously, code using this called getBundle(path, true). If an asset bundle
		/// matching path existed on disk it allowed customizing UI icons, but in practice nobody
		/// used this (likely in part because it was undocumented). If an asset bundle *didn't*
		/// exist, icons were loaded from the Resources folder. Now this returns a wrapper object
		/// to load icons from the core asset bundle.
		/// </summary>
		public static IconsBundle getIconsBundle(string path)
		{
			return new IconsBundle(path);
		}

		public static Bundle getBundle(string path)
		{
			return getBundle(path, true);
		}

		public static Bundle getBundle(string path, bool prependRoot)
		{
			return new Bundle(path, prependRoot);
		}

		[System.Obsolete]
		public static Bundle getBundle(string path, bool prependRoot, bool loadFromResources)
		{
			return getBundle(path, prependRoot);
		}

		private void Awake()
		{
			if (isInitialized)
			{
				Destroy(gameObject);
				return;
			}

			_isInitialized = true;
			DontDestroyOnLoad(gameObject);
		}
	}
}
