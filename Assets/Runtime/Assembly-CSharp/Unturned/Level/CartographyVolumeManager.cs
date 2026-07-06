////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class CartographyVolumeManager : VolumeManager<CartographyVolume, CartographyVolumeManager>
	{
		public CartographyVolume GetMainVolume()
		{
			return allVolumes.HeadOrDefault();
		}

#if !DEDICATED_SERVER
		protected override void OnUpdateGizmos(RuntimeGizmos runtimeGizmos)
		{
			base.OnUpdateGizmos(runtimeGizmos);

			foreach (CartographyVolume volume in allVolumes)
			{
				Color volumeColor = volume.isSelected ? Color.yellow : debugColor;
				runtimeGizmos.Arrow(volume.transform.position, volume.transform.forward, 1.0f, volumeColor);
			}
		}
#endif // !DEDICATED_SERVER

		public CartographyVolumeManager()
		{
			FriendlyName = "Cartography (GPS/Chart)";
			SetDebugColor(new Color32(150, 125, 100, 255));
		}
	}
}
