////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Circular mask for 2D distances in meters on a 2D cell grid.
	/// Includes a cell if the meters distance between the center cell
	/// and the closest point on test cell is within radius.
	/// </summary>
	public class RegionRadiusMask
	{
		public List<Vector2Int> Offsets
		{
			get
			{
				if (isDirty)
				{
					isDirty = false;
					RebuildOffsets();
				}

				return offsets;
			}
		}

		/// <summary>
		/// World space distance in meters.
		/// </summary>
		public float Radius
		{
			get => _radius;
			set
			{
				if (_radius != value)
				{
					_radius = value;
					isDirty = true;
				}
			}
		}

		/// <summary>
		/// Region cell size in meters.
		/// </summary>
		public int CellSize
		{
			get => _cellSize;
			set
			{
				if (_cellSize != value)
				{
					_cellSize = value;
					isDirty = true;
				}
			}
		}

		public void DebugDumpToStringBuilder(System.Text.StringBuilder sb)
		{
			Vector2Int min = Vector2Int.zero;
			Vector2Int max = Vector2Int.zero;

			foreach (Vector2Int offset in Offsets)
			{
				min = Vector2Int.Min(min, offset);
				max = Vector2Int.Max(max, offset);
			}

			for (int y = min.y; y <= max.y; ++y)
			{
				for (int x = min.x; x <= max.x; ++x)
				{
					Vector2Int offset = new Vector2Int(x, y);
					if (Offsets.Contains(offset))
					{
						sb.Append('X');
					}
					else
					{
						sb.Append('O');
					}
				}
				sb.AppendLine();
			}
		}

		public string DebugDumpToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			DebugDumpToStringBuilder(sb);
			return sb.ToString();
		}

		private void RebuildOffsets()
		{
			Offsets.Clear();
			Offsets.Add(Vector2Int.zero);

			if (_radius < Mathf.Epsilon)
			{
				return;
			}

			// Even if radius is only 1m we need to round up to 1 cell so that the adjacent cell is included when at
			// the edge of the center cell.
			int gridRadius = Mathf.CeilToInt(_radius / _cellSize);

			for (int cardinalDistance = 1; cardinalDistance <= gridRadius; ++cardinalDistance)
			{
				Offsets.Add(new Vector2Int(-cardinalDistance, 0));
				Offsets.Add(new Vector2Int(cardinalDistance, 0));
				Offsets.Add(new Vector2Int(0, -cardinalDistance));
				Offsets.Add(new Vector2Int(0, cardinalDistance));
			}

			float sqrRadius = _radius * _radius;
			for (int x = 1; x <= gridRadius; ++x)
			{
				for (int y = 1; y <= gridRadius; ++y)
				{
					// Distance from closest point on this cell to the closest point on the center cell.
					int horizontalDistance = (x - 1) * _cellSize;
					int verticalDistance = (y - 1) * _cellSize;
					int sqrDistance = (horizontalDistance * horizontalDistance) + (verticalDistance * verticalDistance);

					//Debug.Log($"{x}, {y} sqrDistance: {sqrDistance} distance: {Mathf.Sqrt(sqrDistance)}");
					if (sqrDistance <= sqrRadius)
					{
						Offsets.Add(new Vector2Int(x, y));
						Offsets.Add(new Vector2Int(-x, y));
						Offsets.Add(new Vector2Int(x, -y));
						Offsets.Add(new Vector2Int(-x, -y));
					}
				}
			}
		}

		private List<Vector2Int> offsets = new List<Vector2Int>();
		private float _radius = -1.0f;
		private int _cellSize = 128;
		private bool isDirty = true;
	}
}
