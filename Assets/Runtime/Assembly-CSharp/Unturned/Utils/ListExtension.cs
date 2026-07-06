////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public static class ListExtension
	{
		/// <summary>
		/// Get index within bounds assuming list is not empty.
		/// </summary>
		public static int GetRandomIndex<T>(this List<T> list)
		{
			return Random.Range(0, list.Count);
		}

		public static T RandomOrDefault<T>(this List<T> list)
		{
			if (list.Count > 0)
			{
				return list[Random.Range(0, list.Count)];
			}
			else
			{
				return default;
			}
		}

		/// <summary>
		/// Add a new item using its default constructor.
		/// </summary>
		public static T AddDefaulted<T>(this List<T> list) where T : class, new()
		{
			T item = new T();
			list.Add(item);
			return item;
		}

		public static bool IsEmpty<T>(this List<T> list)
		{
			return list.Count < 1;
		}

		public static T HeadOrDefault<T>(this List<T> list)
		{
			if (list.Count > 0)
			{
				return list[0];
			}
			else
			{
				return default;
			}
		}

		public static T TailOrDefault<T>(this List<T> list)
		{
			if (list.Count > 0)
			{
				return list[list.Count - 1];
			}
			else
			{
				return default;
			}
		}

		public static T GetTail<T>(this List<T> list)
		{
			return list[list.Count - 1];
		}

		public static T GetAndRemoveTail<T>(this List<T> list)
		{
			int index = list.Count - 1;
			T item = list[index];
			list.RemoveAt(index);
			return item;
		}

		public static void RemoveTail<T>(this List<T> list)
		{
			list.RemoveAt(list.Count - 1);
		}

		internal static int FindInsertionIndex<T>(this List<T> list, T item)
		{
			int index = list.BinarySearch(item);
			if (index < 0)
			{
				index = ~index;
			}
			return index;
		}

		internal static int FindInsertionIndex<T>(this List<T> list, T item, IComparer<T> comparer)
		{
			int index = list.BinarySearch(item, comparer);
			if (index < 0)
			{
				index = ~index;
			}
			return index;
		}
	}
}
