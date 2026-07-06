////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit.Transactions;

namespace SDG.Framework.Landscapes
{
	public class LandscapeSplatmapTransaction : IDevkitTransaction
	{
		protected LandscapeTile tile;
		protected float[,,] splatmapCopy;

		public bool delta => true;

		public void undo()
		{
			if (tile == null)
			{
				return;
			}

			float[,,] newSplatmapCopy = tile.splatmap;
			tile.splatmap = splatmapCopy;
			splatmapCopy = newSplatmapCopy;
			tile.data.SetAlphamaps(0, 0, tile.splatmap);
		}

		public void redo()
		{
			undo();
		}

		public void begin()
		{
			splatmapCopy = LandscapeSplatmapCopyPool.claim();
			for (int x = 0; x < Landscape.SPLATMAP_RESOLUTION; x++)
			{
				for (int y = 0; y < Landscape.SPLATMAP_RESOLUTION; y++)
				{
					for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
					{
						splatmapCopy[x, y, layer] = tile.splatmap[x, y, layer];
					}
				}
			}
		}

		public void end()
		{ }

		public void forget()
		{
			LandscapeSplatmapCopyPool.release(splatmapCopy);
		}

		public LandscapeSplatmapTransaction(LandscapeTile newTile)
		{
			tile = newTile;
		}
	}
}
