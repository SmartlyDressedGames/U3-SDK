////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class DeadzoneUtility
	{
		[System.Obsolete]
		public static bool isPointInsideVolume(Vector3 point, out DeadzoneVolume volume)
		{
			volume = DeadzoneVolumeManager.Get().GetMostDangerousOverlappingVolume(point);
			return volume != null;
		}

		[System.Obsolete]
		public static bool isPointInsideVolume(DeadzoneVolume volume, Vector3 point)
		{
			return volume.IsPositionInsideVolume(point);
		}
	}
}
