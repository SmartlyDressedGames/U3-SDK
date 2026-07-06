////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public static class DatListEx
	{
		public static bool TryGetValue(this IDatList list, int index, out IDatValue value)
		{
			list.TryGetNode(index, out IDatNode node);
			value = node as IDatValue;
			return value != null;
		}

		public static bool TryGetDictionary(this IDatList list, int index, out IDatDictionary dictionary)
		{
			list.TryGetNode(index, out IDatNode node);
			dictionary = node as IDatDictionary;
			return dictionary != null;
		}

		public static IDatDictionary GetDictionary(this IDatList list, int index)
		{
			return TryGetDictionary(list, index, out IDatDictionary dictionary) ? dictionary : null;
		}

		public static bool TryGetList(this IDatList thisList, int index, out IDatList list)
		{
			thisList.TryGetNode(index, out IDatNode node);
			list = node as IDatList;
			return list != null;
		}

		public static IDatList GetList(this IDatList thisList, int index)
		{
			return TryGetList(thisList, index, out IDatList list) ? list : null;
		}

		public static bool TryGetString(this IDatList list, int index, out string value)
		{
			if (TryGetValue(list, index, out IDatValue _value))
			{
				value = _value.Value;
				return true;
			}
			else
			{
				value = null;
				return false;
			}
		}

		public static string GetString(this IDatList list, int index, string defaultValue = null)
		{
			return TryGetString(list, index, out string value) ? value : defaultValue;
		}

		/// <summary>
		/// TryParseStruct from each node in this list and only add results which returned true.
		/// </summary>
		public static List<T> ParseListOfStructs<T>(this IDatList list) where T : struct, IDatParseable
		{
			List<T> results = new List<T>(list.Count);
			foreach (IDatNode node in list)
			{
				if (node != null && node.TryParseStruct(out T value))
				{
					results.Add(value);
				}
			}
			return results;
		}

		/// <summary>
		/// Return array with the same length as this list and call TryParseStruct on each node.
		/// </summary>
		public static T[] ParseArrayOfStructs<T>(this IDatList list, T defaultValue = default) where T : struct, IDatParseable
		{
			T[] results = new T[list.Count];
			for (int index = 0; index < results.Length; ++index)
			{
				if (list.TryGetNode(index, out IDatNode node) && node.TryParseStruct(out T value))
				{
					results[index] = value;
				}
				else
				{
					results[index] = defaultValue;
				}
			}
			return results;
		}
	}
}
