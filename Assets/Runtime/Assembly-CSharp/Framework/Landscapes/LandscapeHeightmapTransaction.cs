////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit.Transactions;

namespace SDG.Framework.Landscapes
{
	public class LandscapeHeightmapTransaction : IDevkitTransaction
	{
		protected LandscapeTile tile;
		protected float[,] heightmapCopy;

		public bool delta => true;

		public void undo()
		{
			if (tile == null)
			{
				return;
			}

			float[,] newHeightmapCopy = tile.heightmap;
			tile.heightmap = heightmapCopy;
			heightmapCopy = newHeightmapCopy;
			tile.SetHeightsDelayLOD();
			tile.SyncDelayedLOD();
		}

		public void redo()
		{
			undo();
		}

		public void begin()
		{
			heightmapCopy = LandscapeHeightmapCopyPool.claim();
			for (int x = 0; x < Landscape.HEIGHTMAP_RESOLUTION; x++)
			{
				for (int y = 0; y < Landscape.HEIGHTMAP_RESOLUTION; y++)
				{
					heightmapCopy[x, y] = tile.heightmap[x, y];
				}
			}
		}

		public void end()
		{ }

		public void forget()
		{
			LandscapeHeightmapCopyPool.release(heightmapCopy);
		}

		public LandscapeHeightmapTransaction(LandscapeTile newTile)
		{
			tile = newTile;
		}
	}
}
