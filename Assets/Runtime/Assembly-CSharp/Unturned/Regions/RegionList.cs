////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal class RegionList<T>
	{
		public void Add(Vector3 position, T item)
		{
			int cell_x = GetCellIndex(position.x);
			int cell_z = GetCellIndex(position.z);
			List<T> list = GetOrAddList(cell_x, cell_z);
			list.Add(item);
		}

		/// <summary>
		/// Add item to every cell within bounds.
		/// </summary>
		public void Add(Bounds bounds, T item)
		{
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;
			int min_x = GetCellIndex(min.x);
			int min_z = GetCellIndex(min.z);
			int max_x = GetCellIndex(max.x);
			int max_z = GetCellIndex(max.z);
			for (int z = min_z; z <= max_z; ++z)
			{
				for (int x = min_x; x <= max_x; x++)
				{
					List<T> list = GetOrAddList(x, z);
					list.Add(item);
				}
			}
		}

		public bool RemoveFast(Vector3 position, T item, float tolerance)
		{
			foreach (List<T> list in EnumerateListsInSquare(position, tolerance))
			{
				if (list.RemoveFast(item))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Can be null if nothing has been added at position.
		/// </summary>
		public List<T> GetList(Vector3 position)
		{
			return grid[GetCellIndex(position.x), GetCellIndex(position.z)];
		}

		public IEnumerable<T> EnumerateAllItems()
		{
			foreach (List<T> list in grid)
			{
				if (list == null)
					continue;

				foreach (T item in list)
				{
					yield return item;
				}
			}
		}

		/// <summary>
		/// Does not add new lists to empty cells.
		/// </summary>
		public IEnumerable<List<T>> EnumerateListsInSquare(Vector3 position, float radius)
		{
			int min_x = GetCellIndex(position.x - radius);
			int max_x = GetCellIndex(position.x + radius);
			int min_z = GetCellIndex(position.z - radius);
			int max_z = GetCellIndex(position.z + radius);
			for (int x = min_x; x <= max_x; ++x)
			{
				for (int z = min_z; z <= max_z; ++z)
				{
					List<T> list = grid[x, z];
					if (list == null)
						continue;

					yield return grid[x, z];
				}
			}
		}

		public IEnumerable<T> EnumerateItemsInSquare(Vector3 position, float radius)
		{
			int min_x = GetCellIndex(position.x - radius);
			int max_x = GetCellIndex(position.x + radius);
			int min_z = GetCellIndex(position.z - radius);
			int max_z = GetCellIndex(position.z + radius);
			for (int x = min_x; x <= max_x; ++x)
			{
				for (int z = min_z; z <= max_z; ++z)
				{
					List<T> list = grid[x, z];
					if (list == null)
						continue;

					foreach (T item in list)
					{
						yield return item;
					}
				}
			}
		}

		public void DrawGrid(Vector3 cameraPosition, Color color)
		{
			cameraPosition.x = (GetCellIndex(cameraPosition.x) * CELL_SIZE) - 4096.0f + (CELL_SIZE * 0.5f);
			cameraPosition.y = (Mathf.FloorToInt(cameraPosition.y * 0.1f) * 10.0f) - 5.0f;
			cameraPosition.z = (GetCellIndex(cameraPosition.z) * CELL_SIZE) - 4096.0f + (CELL_SIZE * 0.5f);
			RuntimeGizmos.Get().GridXZ(cameraPosition, CELL_SIZE * 5.0f, 5, color);
		}

		public RegionList() : this(1024)
		{ }

		public RegionList(int listPoolSize)
		{
			this.listPoolSize = listPoolSize;
			Debug.Assert(((GRID_SIZE * GRID_SIZE) % listPoolSize) == 0);
			grid = new List<T>[GRID_SIZE, GRID_SIZE];
			listPool = new List<List<T>>(listPoolSize);
			for (int prewarmCounter = 0; prewarmCounter < listPoolSize; ++prewarmCounter)
			{
				listPool.Add(new List<T>());
			}
		}

		private List<T> GetOrAddList(int x, int z)
		{
			List<T> list = grid[x, z];
			if (list != null)
			{
				return list;
			}

			if (listPool.IsEmpty())
			{
				for (int index = 0; index < listPoolSize; ++index)
				{
					listPool.Add(new List<T>());
				}
			}

			list = listPool.GetAndRemoveTail();
			grid[x, z] = list;
			return list;
		}

		private int GetCellIndex(float position)
		{
			if (position <= -4096.0f)
			{
				return 0;
			}
			else if (position >= 4096.0f)
			{
				return GRID_SIZE - 1;
			}
			else
			{
				return Mathf.FloorToInt((position + 4096.0f) / CELL_SIZE);
			}
		}

		private List<T>[,] grid;
		private List<List<T>> listPool;
		private const int GRID_SIZE = 512;
		private const int CELL_SIZE = 8192 / GRID_SIZE;

		/// <summary>
		/// Number of Lists to preallocate in batches.
		/// (GRID_SIZE * GRID_SIZE) % LIST_POOL_SIZE should be zero leftover.
		/// Reduces constructor performance cost. (public issue #4209)
		/// </summary>
		private int listPoolSize = 1024;
	}
}
