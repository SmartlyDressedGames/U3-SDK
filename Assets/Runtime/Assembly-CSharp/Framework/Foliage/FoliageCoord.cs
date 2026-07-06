////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using System;
using UnityEngine;

namespace SDG.Framework.Foliage
{
	public struct FoliageCoord : IFormattedFileReadable, IFormattedFileWritable, IEquatable<FoliageCoord>
	{
		public static FoliageCoord ZERO = new FoliageCoord(0, 0);

		public int x;
		public int y;

		public void read(IFormattedFileReader reader)
		{
			reader = reader.readObject();

			x = reader.readValue<int>("X");
			y = reader.readValue<int>("Y");
		}

		public void write(IFormattedFileWriter writer)
		{
			writer.beginObject();

			writer.writeValue("X", x);
			writer.writeValue("Y", y);

			writer.endObject();
		}

		public static bool operator ==(FoliageCoord a, FoliageCoord b)
		{
			return a.x == b.x && a.y == b.y;
		}

		public static bool operator !=(FoliageCoord a, FoliageCoord b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			FoliageCoord coord = (FoliageCoord) obj;
			return x == coord.x && y == coord.y;
		}

		public override int GetHashCode()
		{
			return x ^ y;
		}

		public override string ToString()
		{
			return '(' + x.ToString() + ", " + y.ToString() + ')';
		}

		public bool Equals(FoliageCoord other)
		{
			return x == other.x && y == other.y;
		}

		public FoliageCoord(int new_x, int new_y)
		{
			x = new_x;
			y = new_y;
		}

		public FoliageCoord(Vector3 position)
		{
			x = Mathf.FloorToInt(position.x / FoliageSystem.TILE_SIZE);
			y = Mathf.FloorToInt(position.z / FoliageSystem.TILE_SIZE);
		}
	}
}
