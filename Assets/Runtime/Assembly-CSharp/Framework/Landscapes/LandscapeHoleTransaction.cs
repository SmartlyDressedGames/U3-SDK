////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit.Transactions;

namespace SDG.Framework.Landscapes
{
	public class LandscapeHoleTransaction : IDevkitTransaction
	{
		protected LandscapeTile tile;
		protected bool[,] holesCopy;

		public bool delta => true;

		public void undo()
		{
			if (tile == null)
			{
				return;
			}

			bool[,] newHolesCopy = tile.holes;
			tile.holes = holesCopy;
			holesCopy = newHolesCopy;
			tile.SetHoles();
			tile.SyncDelayedLOD();
		}

		public void redo()
		{
			undo();
		}

		public void begin()
		{
			holesCopy = LandscapeHoleCopyPool.claim();
			for (int x = 0; x < Landscape.HOLES_RESOLUTION; x++)
			{
				for (int y = 0; y < Landscape.HOLES_RESOLUTION; y++)
				{
					holesCopy[x, y] = tile.holes[x, y];
				}
			}
		}

		public void end()
		{ }

		public void forget()
		{
			LandscapeHoleCopyPool.release(holesCopy);
		}

		public LandscapeHoleTransaction(LandscapeTile newTile)
		{
			tile = newTile;
		}
	}
}
