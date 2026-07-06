////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void BlueprintItemIconsReady();

	public class BlueprintItemIconsInfo
	{
		public Texture2D[] textures;
		public BlueprintItemIconsReady callback;
		private int index;

		public void onItemIconReady(Texture2D texture)
		{
			if (index >= textures.Length)
			{
				return;
			}

			textures[index] = texture;

			index++;
			if (index == textures.Length)
			{
				callback?.Invoke();
			}
		}
	}
}