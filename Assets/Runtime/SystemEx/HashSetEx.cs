////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace Unturned.SystemEx
{
	public static class HashSetEx
	{
		/// <returns>true if any items were added.</returns>
		public static bool AddAny<T>(this HashSet<T> hashSet, IEnumerable<T> collection)
		{
			bool result = false;
			foreach (T item in collection)
			{
				result |= hashSet.Add(item);
			}
			return result;
		}
	}
}
