////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////

using SDG.Unturned;

namespace SDG.Framework.Devkit
{
	public class NavClipVolume : LevelVolume<NavClipVolume, NavClipVolumeManager>
	{
		protected override void Awake()
		{
			forceShouldAddCollider = true; // Needed in gameplay for AI physics.
			base.Awake();
			volumeCollider.isTrigger = false;
			gameObject.layer = LayerMasks.NAVMESH;
		}
	}
}
