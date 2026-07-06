////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ArenaCompactorVolumeManager : VolumeManager<ArenaCompactorVolume, ArenaCompactorVolumeManager>
	{
		public ArenaCompactorVolumeManager()
		{
			FriendlyName = "Arena Mode Compactor";
			SetDebugColor(new Color32(20, 20, 20, 255));
		}
	}
}
