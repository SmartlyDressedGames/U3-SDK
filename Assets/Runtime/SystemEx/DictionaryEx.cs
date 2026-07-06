////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace Unturned.SystemEx
{
	public static class DictionaryEx
	{
		public static TValue GetOrAddDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
		{
			TValue value;
			if (!dictionary.TryGetValue(key, out value))
			{
				value = default;
				dictionary.Add(key, value);
			}
			return value;
		}

		public static TValue GetOrAddNew<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
		{
			TValue value;
			if (!dictionary.TryGetValue(key, out value))
			{
				value = new TValue();
				dictionary.Add(key, value);
			}
			return value;
		}
	}
}
