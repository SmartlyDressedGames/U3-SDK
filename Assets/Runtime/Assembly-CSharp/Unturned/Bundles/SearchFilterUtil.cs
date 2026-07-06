////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Unturned
{
	public static class SearchFilterUtil
	{
		public static bool parseKeyValue(string filter, string key, out string value)
		{
			value = null;

			if (string.IsNullOrEmpty(filter))
				return false;

			int indexOfKey = filter.IndexOf(key, StringComparison.InvariantCultureIgnoreCase);
			if (indexOfKey < 0)
				return false;

			int indexOfValue = indexOfKey + key.Length;
			int indexOfDelimiter = filter.IndexOf(' ', indexOfValue);
			if (indexOfDelimiter < 0)
			{
				value = filter.Substring(indexOfValue);
			}
			else
			{
				value = filter.Substring(indexOfValue, indexOfDelimiter - indexOfValue);
			}

			return string.IsNullOrEmpty(value) == false;
		}
	}
}
