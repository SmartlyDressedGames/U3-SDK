////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class UndergroundWhitelistVolumeManager : VolumeManager<UndergroundWhitelistVolume, UndergroundWhitelistVolumeManager>
	{
		public UndergroundWhitelistVolumeManager()
		{
			FriendlyName = "Underground Whitelist";
			SetDebugColor(new Color32(63, 63, 63, 255));
			benefitsFromStaticVolumes = true;
		}
	}
}
