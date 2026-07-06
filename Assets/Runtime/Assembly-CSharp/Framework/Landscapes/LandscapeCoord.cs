////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using System;
using UnityEngine;

namespace SDG.Framework.Landscapes
{
	public struct LandscapeCoord : IFormattedFileReadable, IFormattedFileWritable, IEquatable<LandscapeCoord>
	{
		public static LandscapeCoord ZERO = new LandscapeCoord(0, 0);

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

		public static bool operator ==(LandscapeCoord a, LandscapeCoord b)
		{
			return a.x == b.x && a.y == b.y;
		}

		public static bool operator !=(LandscapeCoord a, LandscapeCoord b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			LandscapeCoord coord = (LandscapeCoord) obj;
			return x.Equals(coord.x) && y.Equals(coord.y);
		}

		public override int GetHashCode()
		{
			return x ^ y;
		}

		public override string ToString()
		{
			return '(' + x.ToString() + ", " + y.ToString() + ')';
		}

		public bool Equals(LandscapeCoord other)
		{
			return x == other.x && y == other.y;
		}

		public LandscapeCoord(int new_x, int new_y)
		{
			x = new_x;
			y = new_y;
		}

		public LandscapeCoord(Vector3 position)
		{
			x = Mathf.FloorToInt(position.x / Landscape.TILE_SIZE);
			y = Mathf.FloorToInt(position.z / Landscape.TILE_SIZE);
		}
	}
}
