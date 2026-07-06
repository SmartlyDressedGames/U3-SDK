////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class HordePurchaseVolumeManager : VolumeManager<HordePurchaseVolume, HordePurchaseVolumeManager>
	{
		public HordePurchaseVolumeManager()
		{
			FriendlyName = "Horde Purchase";
			SetDebugColor(new Color32(20, 50, 20, 255));
			benefitsFromStaticVolumes = true;
		}
	}
}
