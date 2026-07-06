////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class EffectVolumeManager : VolumeManager<EffectVolume, EffectVolumeManager>
	{
		public EffectVolumeManager()
		{
			FriendlyName = "Effect";
			SetDebugColor(new Color32(255, 255, 255, 255));
		}
	}
}
