////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Framework.Foliage
{
	public static class FoliageVolumeSystem
	{
		[System.Obsolete]
		public static List<FoliageVolume> additiveVolumes => FoliageVolumeManager.Get().additiveVolumes;

		[System.Obsolete]
		public static List<FoliageVolume> subtractiveVolumes => FoliageVolumeManager.Get().subtractiveVolumes;
	}
}
