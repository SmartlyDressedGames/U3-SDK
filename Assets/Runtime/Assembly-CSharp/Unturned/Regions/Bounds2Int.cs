////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public struct RegionBoundsInt : System.IEquatable<RegionBoundsInt>
	{
		public Vector2Int min;
		public Vector2Int max;

		public static bool operator ==(RegionBoundsInt lhs, RegionBoundsInt rhs)
		{
			return lhs.min == rhs.min && lhs.max == rhs.max;
		}

		public static bool operator !=(RegionBoundsInt lhs, RegionBoundsInt rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object obj)
		{
			return obj is RegionBoundsInt bounds && (this == bounds);
		}

		public override int GetHashCode()
		{
			return System.HashCode.Combine(min, max);
		}

		public override string ToString()
		{
			return $"({min}, {max})";
		}

		public bool Equals(RegionBoundsInt other)
		{
			return min == other.min && max == other.max;
		}

		public RegionBoundsIntEnumerator GetEnumerator()
		{
			return new RegionBoundsIntEnumerator(this);
		}

		public RegionBoundsInt(Vector2Int min, Vector2Int max)
		{
			this.min = min;
			this.max = max;
		}
	}

	/// <summary>
	/// Allows foreach loop to iterate Vector2Int within RegionBoundsInt.
	/// </summary>
	public struct RegionBoundsIntEnumerator : IEnumerator<Vector2Int>
	{
		public RegionBoundsIntEnumerator(RegionBoundsInt bounds)
		{
			min = bounds.min;
			max = bounds.max;
			coord = new Vector2Int(min.x - 1, min.y);
		}

		public Vector2Int Current => coord;

		object IEnumerator.Current => Current;

		public void Dispose()
		{

		}

		public bool MoveNext()
		{
			coord = new Vector2Int(coord.x + 1, coord.y);
			if (coord.x > max.x)
			{
				coord = new Vector2Int(min.x, coord.y + 1);
				if (coord.y > max.y)
				{
					return false;
				}
			}

			return true;
		}

		public void Reset()
		{
			coord = new Vector2Int(min.x - 1, min.y);
		}

		private Vector2Int min;
		private Vector2Int max;
		private Vector2Int coord;
	}
}
