////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class KillVolumeManager : VolumeManager<KillVolume, KillVolumeManager>
	{
		public KillVolumeManager()
		{
			FriendlyName = "Kill";
			SetDebugColor(new Color32(220, 100, 20, 255));
			benefitsFromStaticVolumes = true;
		}
	}
}
