////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Substitute for StaticResourceRefs pointing to icons.
	/// </summary>
	public class StaticIconRef<T> where T : Object
	{
		public T GetOrLoad()
		{
			if (needsLoad)
			{
				needsLoad = false;
				IconsBundle bundle = Bundles.getIconsBundle(path);
				asset = bundle.load<T>(name);
				if (asset == null)
				{
					UnturnedLog.error("Missing icon {0} ({1})", path, typeof(T));
				}
			}

			return asset;
		}

		public StaticIconRef(string path, string name)
		{
			this.path = path;
			this.name = name;
			asset = null;
			needsLoad = true;
		}

		public static implicit operator T(StaticIconRef<T> resource)
		{
			return resource.GetOrLoad();
		}

		private string path;
		private string name;
		private T asset;
		private bool needsLoad;
	}
}
