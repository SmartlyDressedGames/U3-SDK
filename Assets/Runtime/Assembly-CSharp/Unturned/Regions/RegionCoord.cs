////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using System;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Nelson 2025-06-10: new code should favor Vector2Int. We don't want to introduce further uint8 region usage.
	/// </summary>
	public struct RegionCoord : IFormattedFileReadable, IFormattedFileWritable, IEquatable<RegionCoord>
	{
		public static RegionCoord ZERO = new RegionCoord(0, 0);

		public byte x;
		public byte y;

		public void read(IFormattedFileReader reader)
		{
			reader = reader.readObject();

			x = reader.readValue<byte>("X");
			y = reader.readValue<byte>("Y");
		}

		public void write(IFormattedFileWriter writer)
		{
			writer.beginObject();

			writer.writeValue("X", x);
			writer.writeValue("Y", y);

			writer.endObject();
		}

		public static bool operator ==(RegionCoord a, RegionCoord b)
		{
			return a.x == b.x && a.y == b.y;
		}

		public static bool operator !=(RegionCoord a, RegionCoord b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			RegionCoord coord = (RegionCoord) obj;
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

		public bool Equals(RegionCoord other)
		{
			return x == other.x && y == other.y;
		}

		public void ClampIntoBounds()
		{
			x = (byte) Mathf.Max(x, 0);
			x = (byte) Mathf.Min(x, Regions.CONST_WORLD_SIZE - 1);
			y = (byte) Mathf.Max(y, 0);
			y = (byte) Mathf.Min(y, Regions.CONST_WORLD_SIZE - 1);
		}

		public RegionCoord(byte new_x, byte new_y)
		{
			x = new_x;
			y = new_y;
		}

		public RegionCoord(Vector3 position)
		{
			Regions.tryGetCoordinate(position, out x, out y);
		}
	}
}
