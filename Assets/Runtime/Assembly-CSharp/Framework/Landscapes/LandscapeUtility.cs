////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Framework.Landscapes
{
	public class LandscapeUtility
	{
		/// <summary>
		/// If a heightmap coordinate is out of bounds the tile/heightamp coordinate will be adjusted so that it is in bounds again.
		/// </summary>
		public static void cleanHeightmapCoord(ref LandscapeCoord tileCoord, ref HeightmapCoord heightmapCoord)
		{
			if (heightmapCoord.x < 0)
			{
				tileCoord.y--;
				heightmapCoord.x += Landscape.HEIGHTMAP_RESOLUTION;
			}

			if (heightmapCoord.y < 0)
			{
				tileCoord.x--;
				heightmapCoord.y += Landscape.HEIGHTMAP_RESOLUTION;
			}

			if (heightmapCoord.x >= Landscape.HEIGHTMAP_RESOLUTION)
			{
				tileCoord.y++;
				heightmapCoord.x -= Landscape.HEIGHTMAP_RESOLUTION;
			}

			if (heightmapCoord.y >= Landscape.HEIGHTMAP_RESOLUTION)
			{
				tileCoord.x++;
				heightmapCoord.y -= Landscape.HEIGHTMAP_RESOLUTION;
			}
		}

		/// <summary>
		/// If a splatmap coordinate is out of bounds the tile/splatmap coordinate will be adjusted so that it is in bounds again.
		/// </summary>
		public static void cleanSplatmapCoord(ref LandscapeCoord tileCoord, ref SplatmapCoord splatmapCoord)
		{
			if (splatmapCoord.x < 0)
			{
				tileCoord.y--;
				splatmapCoord.x += Landscape.SPLATMAP_RESOLUTION;
			}

			if (splatmapCoord.y < 0)
			{
				tileCoord.x--;
				splatmapCoord.y += Landscape.SPLATMAP_RESOLUTION;
			}

			if (splatmapCoord.x >= Landscape.SPLATMAP_RESOLUTION)
			{
				tileCoord.y++;
				splatmapCoord.x -= Landscape.SPLATMAP_RESOLUTION;
			}

			if (splatmapCoord.y >= Landscape.SPLATMAP_RESOLUTION)
			{
				tileCoord.x++;
				splatmapCoord.y -= Landscape.SPLATMAP_RESOLUTION;
			}
		}
	}
}
