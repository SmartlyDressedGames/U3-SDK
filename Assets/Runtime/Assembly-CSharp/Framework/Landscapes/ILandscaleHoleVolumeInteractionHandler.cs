////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Framework.Landscapes
{
	[System.Obsolete]
	public interface ILandscaleHoleVolumeInteractionHandler
	{
		bool landscapeHoleAutoIgnoreTerrainCollision
		{
			get;
		}

		void landscapeHoleBeginCollision(LandscapeHoleVolume volume, List<TerrainCollider> terrainColliders);
		void landscapeHoleEndCollision(LandscapeHoleVolume volume, List<TerrainCollider> terrainColliders);
	}
}
