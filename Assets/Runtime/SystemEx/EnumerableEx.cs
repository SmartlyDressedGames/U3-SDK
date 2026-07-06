////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace Unturned.SystemEx
{
	public static class EnumerableEx
	{
		/// <summary>
		/// Kind of hacky? Useful if you have an unordered collection like HashSet<T>
		/// with one element and want to retrieve it.
		/// </summary>
		public static T EnumerateFirst<T>(this IEnumerable<T> collection)
		{
			IEnumerator<T> enumerator = collection.GetEnumerator();
			enumerator.MoveNext();
			return enumerator.Current;
		}
	}
}
