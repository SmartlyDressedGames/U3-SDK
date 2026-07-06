////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class PlayerClipVolumeManager : VolumeManager<PlayerClipVolume, PlayerClipVolumeManager>
	{
		public PlayerClipVolumeManager()
		{
			FriendlyName = "Player Clip";
			SetDebugColor(new Color32(63, 0, 0, 255));
			benefitsFromStaticVolumes = true;
		}
	}
}
