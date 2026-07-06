////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class AmbianceUtility
	{
		[System.Obsolete]
		public static bool isPointInsideVolume(Vector3 point, out AmbianceVolume volume)
		{
			volume = AmbianceVolumeManager.Get().GetFirstOverlappingVolume(point);
			return volume != null;
		}

		[System.Obsolete]
		public static bool isPointInsideVolume(AmbianceVolume volume, Vector3 point)
		{
			return volume.IsPositionInsideVolume(point);
		}
	}
}
