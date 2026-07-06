////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal class RegionDictionary<T>
	{
		public List<T> GetListOrNull(Vector2Int coord)
		{
			data.TryGetValue(coord, out List<T> region);
			return region;
		}

		public List<T> GetOrAddList(Vector2Int coord)
		{
			if (!data.TryGetValue(coord, out List<T> value))
			{
				value = new List<T>();
				data[coord] = value;
			}
			return value;
		}

		public List<T> GetOrAddList(byte x, byte y)
		{
			return GetOrAddList(new Vector2Int(x, y));
		}

		public void ReleaseListIfEmpty(Vector2Int coord)
		{
			if (data.TryGetValue(coord, out List<T> value))
			{
				if (value != null && value.IsEmpty())
				{
					data.Remove(coord);
				}
			}
		}

		public void GatherAllItems(List<T> results)
		{
			foreach (List<T> items in data.Values)
			{
				if (items != null && items.Count > 0)
				{
					results.AddRange(items);
				}
			}
		}

		public RegionDictionary()
		{
			data = new Dictionary<Vector2Int, List<T>>();
		}

		internal Dictionary<Vector2Int, List<T>> data;
	}
}
