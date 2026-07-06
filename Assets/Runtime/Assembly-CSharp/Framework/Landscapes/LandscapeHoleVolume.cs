////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;

namespace SDG.Framework.Landscapes
{
	public class LandscapeHoleVolume : LevelVolume<LandscapeHoleVolume, LandscapeHoleVolumeManager>
	{
		protected override void Awake()
		{
			supportsSphereShape = false;
			base.Awake();
		}
	}
}
