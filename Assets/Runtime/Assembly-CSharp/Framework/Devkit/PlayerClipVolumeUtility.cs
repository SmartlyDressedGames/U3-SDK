////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class PlayerClipVolumeUtility
	{
		[System.Obsolete]
		public static bool isPointInsideVolume(Vector3 point)
		{
			return PlayerClipVolumeManager.Get().IsPositionInsideAnyVolume(point);
		}
	}
}
