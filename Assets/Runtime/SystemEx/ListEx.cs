////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace Unturned.SystemEx
{
	public static class ListEx
	{
		/// <param name="predicate">returns true if the item should be removed.</param>
		public static void RemoveSwap<T>(this List<T> list, Predicate<T> predicate)
		{
			for (int index = list.Count - 1; index >= 0; --index)
			{
				if (predicate(list[index]))
				{
					list[index] = list[list.Count - 1];
					list.RemoveAt(list.Count - 1);
				}
			}
		}

		public static void AddIfNotContained<T>(this List<T> list, T item)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
	}
}
