////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class NoStructuresVolumeManager : VolumeManager<NoStructuresVolume, NoStructuresVolumeManager>
	{
		public NoStructuresVolumeManager()
		{
			FriendlyName = "No Structures";
			SetDebugColor(new Color32(150, 125, 175, 255));
			benefitsFromStaticVolumes = true;
		}
	}
}
