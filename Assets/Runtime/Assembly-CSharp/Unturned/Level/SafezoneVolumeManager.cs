////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SafezoneVolumeManager : VolumeManager<SafezoneVolume, SafezoneVolumeManager>
	{
		public SafezoneVolumeManager()
		{
			FriendlyName = "Safezone";
			SetDebugColor(new Color32(205, 145, 205, 255));
			benefitsFromStaticVolumes = true;
		}
	}
}
