////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class AmbianceVolumeManager : VolumeManager<AmbianceVolume, AmbianceVolumeManager>
	{
		public AmbianceVolumeManager()
		{
			FriendlyName = "Ambiance";
			SetDebugColor(new Color32(0, 127, 127, 255));
			supportsFalloff = true;
			benefitsFromStaticVolumes = true;
		}
	}
}
