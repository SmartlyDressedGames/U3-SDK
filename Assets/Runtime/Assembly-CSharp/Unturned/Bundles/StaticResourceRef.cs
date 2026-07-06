////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Assets cannot be loaded from Resources during static initialization, so this reference defers the load until
	/// the first time user tries to use it.
	/// </summary>
	public class StaticResourceRef<T> where T : Object
	{
		public T GetOrLoad()
		{
			if (needsLoad)
			{
				needsLoad = false;
				asset = Resources.Load<T>(path);
				if (asset == null)
				{
					UnturnedLog.error("Missing resource {0} ({1})", path, typeof(T));
				}
			}

			return asset;
		}

		public StaticResourceRef(string path)
		{
			this.path = path;
			asset = null;
			needsLoad = true;
		}

		public static implicit operator T(StaticResourceRef<T> resource)
		{
			return resource.GetOrLoad();
		}

		private string path;
		private T asset;
		private bool needsLoad;
	}
}
