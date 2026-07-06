////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void EditorRegionUpdated(byte old_x, byte old_y, byte new_x, byte new_y);
	public delegate void PlayerRegionUpdated(Player player, byte old_x, byte old_y, byte new_x, byte new_y, byte index, ref bool canIncrementIndex);

	public struct RegionCoordinate
	{
		public byte x;
		public byte y;

		public RegionCoordinate(byte x, byte y)
		{
			this.x = x;
			this.y = y;
		}

		public override string ToString()
		{
			return "(" + x + ", " + y + ")";
		}
	}

	public class Regions
	{
		internal const byte CONST_WORLD_SIZE = 64;
		public static readonly byte WORLD_SIZE = CONST_WORLD_SIZE;
		internal const byte CONST_REGION_SIZE = 8192 / CONST_WORLD_SIZE;
		public static readonly byte REGION_SIZE = CONST_REGION_SIZE;

		public static void getRegionsInRadius(Vector3 center, float radius, List<RegionCoordinate> result)
		{
			// World-space bounding square of the circle.
			Vector3 min = new Vector3(center.x - radius, center.y, center.z - radius);
			Vector3 max = new Vector3(center.x + radius, center.y, center.z + radius);

			// Convert world-space bounding square into region-space.
			int min_x;
			int min_y;
			getUnsafeCoordinates(min, out min_x, out min_y);
			int max_x;
			int max_y;
			getUnsafeCoordinates(max, out max_x, out max_y);

			if (min_x >= WORLD_SIZE || min_y >= WORLD_SIZE || max_x < 0 || max_y < 0)
			{
				// Bounding square is entirely outside the valid region grid.
				return;
			}

			// Clamp bounding square into valid coordinates so all "test" points will be valid.
			min_x = Mathf.Max(min_x, 0);
			max_x = Mathf.Min(max_x, WORLD_SIZE - 1);
			min_y = Mathf.Max(min_y, 0);
			max_y = Mathf.Min(max_y, WORLD_SIZE - 1);

			for (int test_x = min_x; test_x <= max_x; ++test_x)
			{
				for (int test_y = min_y; test_y <= max_y; ++test_y)
				{
					result.Add(new RegionCoordinate((byte) test_x, (byte) test_y));
				}
			}
		}

		/// <summary>
		/// Convert world-space point into region coordinates that may be out of bounds.
		/// </summary>
		private static void getUnsafeCoordinates(Vector3 point, out int x, out int y)
		{
			x = Mathf.FloorToInt((point.x + 4096.0f) / REGION_SIZE);
			y = Mathf.FloorToInt((point.z + 4096.0f) / REGION_SIZE);
		}

		/// <summary>
		/// Convert world-space position into a region coordinate that may be out-of-bounds.
		/// </summary>
		public static Vector2Int GetCoordinateVector2Int(Vector3 position)
		{
			getUnsafeCoordinates(position, out int x, out int y);
			return new Vector2Int(x, y);
		}

		public static void GetCoordinateBoundsVector2Int(Vector3 position, float radius, out Vector2Int min, out Vector2Int max)
		{
			Vector3 minPosition = new Vector3(position.x - radius, 0.0f, position.z - radius);
			Vector3 maxPosition = new Vector3(position.x + radius, 0.0f, position.z + radius);
			min = GetCoordinateVector2Int(minPosition);
			max = GetCoordinateVector2Int(maxPosition);
		}

		public static void GetCoordinateBoundsVector2Int(Bounds worldBounds, out Vector2Int min, out Vector2Int max)
		{
			Vector3 minPosition = worldBounds.min;
			Vector3 maxPosition = worldBounds.max;
			min = GetCoordinateVector2Int(minPosition);
			max = GetCoordinateVector2Int(maxPosition);
		}

		public static RegionBoundsInt GetCoordinateBoundsInt(Bounds worldBounds)
		{
			Vector3 minPosition = worldBounds.min;
			Vector3 maxPosition = worldBounds.max;
			Vector2Int min = GetCoordinateVector2Int(minPosition);
			Vector2Int max = GetCoordinateVector2Int(maxPosition);
			return new RegionBoundsInt(min, max);
		}

		public static bool IsVector2IntWithinLegacyRange(Vector2Int coord)
		{
			return coord.x >= 0 && coord.y >= 0 && coord.x < WORLD_SIZE && coord.y < WORLD_SIZE;
		}

		/// <summary>
		/// Returns true if coord is within legacy range.
		/// </summary>
		public static bool TryConvertVector2IntCoord(Vector2Int coord, out byte x, out byte y)
		{
			if (coord.x >= 0 && coord.y >= 0 && coord.x < WORLD_SIZE && coord.y < WORLD_SIZE)
			{
				x = (byte) coord.x;
				y = (byte) coord.y;
				return true;
			}
			else
			{
				x = byte.MaxValue;
				y = byte.MaxValue;
				return false;
			}
		}

		public static bool tryGetCoordinate(Vector3 point, out byte x, out byte y)
		{
			x = 255;
			y = 255;

			if (checkSafe(point))
			{
				x = (byte) ((point.x + 4096) / REGION_SIZE);
				y = (byte) ((point.z + 4096) / REGION_SIZE);

				return true;
			}

			return false;
		}

		public static bool tryGetPoint(int x, int y, out Vector3 point)
		{
			point = Vector3.zero;

			if (checkSafe(x, y))
			{
				point.x = (x * REGION_SIZE) - 4096;
				point.z = (y * REGION_SIZE) - 4096;

				return true;
			}

			return false;
		}

		internal static float HorizontalDistanceFromCenterSquared(int x, int y, Vector3 position)
		{
			const float offset = (CONST_REGION_SIZE * 0.5f) - 4096.0f;
			Vector3 regionCenter = new Vector3((x * REGION_SIZE) + offset, 0.0f, (y * REGION_SIZE) + offset);
			return MathfEx.HorizontalDistanceSquared(regionCenter, position);
		}

		public static bool checkSafe(Vector3 point)
		{
			if (point.x >= -4096 && point.z >= -4096 && point.x < 4096 && point.z < 4096)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Clamp position into the maximum bounds expected by the game, not necessarily the level bounds.
		/// </summary>
		/// <returns>True if position was modified.</returns>
		public static bool clampPositionIntoBounds(ref Vector3 position)
		{
			bool wasPositionModified = false;

			const float TOLERANCE = 0.1f; // Pull back into level slightly because some places test <MAX rather than <=MAX.
			const float MAX_XZ = 4096 - TOLERANCE; // half Level.INSANE_SIZE
			const float MIN_XZ = -MAX_XZ;
			const float MAX_Y = 1024 - TOLERANCE; // half Landscape.TILE_HEIGHT
			const float MIN_Y = -MAX_Y;

			if (position.x < MIN_XZ)
			{
				position.x = MIN_XZ;
				wasPositionModified = true;
			}
			else if (position.x > MAX_XZ)
			{
				position.x = MAX_XZ;
				wasPositionModified = true;
			}

			if (position.y < MIN_Y)
			{
				position.y = MIN_Y;
				wasPositionModified = true;
			}
			else if (position.y > MAX_Y)
			{
				position.y = MAX_Y;
				wasPositionModified = true;
			}

			if (position.z < MIN_XZ)
			{
				position.z = MIN_XZ;
				wasPositionModified = true;
			}
			else if (position.z > MAX_XZ)
			{
				position.z = MAX_XZ;
				wasPositionModified = true;
			}

			return wasPositionModified;
		}

		public static bool checkSafe(int x, int y)
		{
			if (x >= 0 && y >= 0 && x < WORLD_SIZE && y < WORLD_SIZE)
			{
				return true;
			}

			return false;
		}

		public static bool checkArea(byte x_0, byte y_0, byte x_1, byte y_1, byte area)
		{
			if (x_0 < x_1 - area || y_0 < y_1 - area)
			{
				return false;
			}

			if (x_0 > x_1 + area || y_0 > y_1 + area)
			{
				return false;
			}

			return true;
		}

		public static PooledTransportConnectionList GatherClientConnections(byte x, byte y, byte distance)
		{
			PooledTransportConnectionList list = TransportConnectionListPool.Get();
			foreach (SteamPlayer client in Provider.clients)
			{
				if (client.player == null)
					continue;

				if (checkArea(x, y, client.player.movement.region_x, client.player.movement.region_y, distance))
				{
					list.Add(client.transportConnection);
				}
			}
			return list;
		}

		[System.Obsolete("Replaced by GatherClientConnections")]
		public static IEnumerable<ITransportConnection> EnumerateClients(byte x, byte y, byte distance)
		{
			return GatherClientConnections(x, y, distance);
		}

		public static PooledTransportConnectionList GatherRemoteClientConnections(byte x, byte y, byte distance)
		{
			PooledTransportConnectionList list = TransportConnectionListPool.Get();
			foreach (SteamPlayer client in Provider.clients)
			{
				if (client.player == null)
					continue;

#if !DEDICATED_SERVER
				if (client.IsLocalServerHost)
					continue;
#endif // !DEDICATED_SERVER

				if (checkArea(x, y, client.player.movement.region_x, client.player.movement.region_y, distance))
				{
					list.Add(client.transportConnection);
				}
			}
			return list;
		}

		[System.Obsolete("Replaced by GatherRemoteClientConnections")]
		public static IEnumerable<ITransportConnection> EnumerateClients_Remote(byte x, byte y, byte distance)
		{
			return GatherRemoteClientConnections(x, y, distance);
		}
	}
}
