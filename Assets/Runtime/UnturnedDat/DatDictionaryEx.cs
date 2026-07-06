////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public static class DatDictionaryEx
	{
		public static bool TryGetValue(this IDatDictionary dictionary, string key, out IDatValue node)
		{
			IDatNode _value;
			bool success = dictionary.TryGetNode(key, out _value);
			node = _value as IDatValue;
			return success && node != null;
		}

		public static bool TryGetDictionary(this IDatDictionary dictionary, string key, out IDatDictionary node)
		{
			IDatNode _value;
			bool success = dictionary.TryGetNode(key, out _value);
			node = _value as IDatDictionary;
			return success && node != null;
		}

		public static IDatDictionary GetDictionary(this IDatDictionary dictionary, string key)
		{
			return TryGetDictionary(dictionary, key, out IDatDictionary node) ? node : null;
		}

		public static bool TryGetList(this IDatDictionary dictionary, string key, out IDatList node)
		{
			IDatNode _value;
			bool success = dictionary.TryGetNode(key, out _value);
			node = _value as IDatList;
			return success && node != null;
		}

		public static IDatList GetList(this IDatDictionary dictionary, string key)
		{
			return TryGetList(dictionary, key, out IDatList node) ? node : null;
		}

		public static bool TryGetString(this IDatDictionary dictionary, string key, out string value)
		{
			if (TryGetValue(dictionary, key, out IDatValue node))
			{
				value = node.Value;
				return true;
			}
			else
			{
				value = null;
				return false;
			}
		}

		public static string GetString(this IDatDictionary dictionary, string key, string defaultValue = default)
		{
			bool result = TryGetString(dictionary, key, out string value);
			return result ? value : defaultValue;
		}

		public static bool TryParseInt8(this IDatDictionary dictionary, string key, out sbyte value)
		{
			value = default;
			return TryGetValue(dictionary, key, out IDatValue node) && node.TryParseInt8(out value);
		}

		public static sbyte ParseInt8(this IDatDictionary dictionary, string key, sbyte defaultValue = default)
		{
			return TryParseInt8(dictionary, key, out sbyte value) ? value : defaultValue;
		}

		public static bool TryParseUInt8(this IDatDictionary dictionary, string key, out byte value)
		{
			value = default;
			return TryGetValue(dictionary, key, out IDatValue node) && node.TryParseUInt8(out value);
		}

		public static byte ParseUInt8(this IDatDictionary dictionary, string key, byte defaultValue = default)
		{
			return TryParseUInt8(dictionary, key, out byte value) ? value : defaultValue;
		}

		public static bool TryParseInt16(this IDatDictionary dictionary, string key, out short value)
		{
			value = default;
			return TryGetValue(dictionary, key, out IDatValue node) && node.TryParseInt16(out value);
		}

		public static short ParseInt16(this IDatDictionary dictionary, string key, short defaultValue = default)
		{
			return TryParseInt16(dictionary, key, out short value) ? value : defaultValue;
		}

		public static bool TryParseUInt16(this IDatDictionary dictionary, string key, out ushort value)
		{
			value = default;
			return TryGetValue(dictionary, key, out IDatValue node) && node.TryParseUInt16(out value);
		}

		public static ushort ParseUInt16(this IDatDictionary dictionary, string key, ushort defaultValue = default)
		{
			return TryParseUInt16(dictionary, key, out ushort value) ? value : defaultValue;
		}

		public static bool TryParseInt32(this IDatDictionary dictionary, string key, out int value)
		{
			value = default;
			return TryGetValue(dictionary, key, out IDatValue node) && node.TryParseInt32(out value);
		}

		public static int ParseInt32(this IDatDictionary dictionary, string key, int defaultValue = default)
		{
			return TryParseInt32(dictionary, key, out int value) ? value : defaultValue;
		}

		public static bool TryParseUInt32(this IDatDictionary dictionary, string key, out uint value)
		{
			value = default;
			return TryGetValue(dictionary, key, out IDatValue node) && node.TryParseUInt32(out value);
		}

		public static uint ParseUInt32(this IDatDictionary dictionary, string key, uint defaultValue = default)
		{
			return TryParseUInt32(dictionary, key, out uint value) ? value : defaultValue;
		}

		public static bool TryParseInt64(this IDatDictionary dictionary, string key, out long value)
		{
			value = default;
			return TryGetValue(dictionary, key, out IDatValue node) && node.TryParseInt64(out value);
		}

		public static long ParseInt64(this IDatDictionary dictionary, string key, long defaultValue = default)
		{
			return TryParseInt64(dictionary, key, out long value) ? value : defaultValue;
		}

		public static bool TryParseUInt64(this IDatDictionary dictionary, string key, out ulong value)
		{
			value = default;
			return TryGetValue(dictionary, key, out IDatValue node) && node.TryParseUInt64(out value);
		}

		public static ulong ParseUInt64(this IDatDictionary dictionary, string key, ulong defaultValue = default)
		{
			return TryParseUInt64(dictionary, key, out ulong value) ? value : defaultValue;
		}

		public static bool TryParseFloat(this IDatDictionary dictionary, string key, out float value)
		{
			value = default;
			return TryGetValue(dictionary, key, out IDatValue node) && node.TryParseFloat(out value);
		}

		public static float ParseFloat(this IDatDictionary dictionary, string key, float defaultValue = default)
		{
			return TryParseFloat(dictionary, key, out float value) ? value : defaultValue;
		}

		public static bool TryParseDouble(this IDatDictionary dictionary, string key, out double value)
		{
			value = default;
			return TryGetValue(dictionary, key, out IDatValue node) && node.TryParseDouble(out value);
		}

		public static double ParseDouble(this IDatDictionary dictionary, string key, double defaultValue = default)
		{
			return TryParseDouble(dictionary, key, out double value) ? value : defaultValue;
		}

		public static bool TryParseEnum<T>(this IDatDictionary dictionary, string key, out T value) where T : struct
		{
			value = default;
			return TryGetValue(dictionary, key, out IDatValue node) && node.TryParseEnum<T>(out value);
		}

		public static T ParseEnum<T>(this IDatDictionary dictionary, string key, T defaultValue = default) where T : struct
		{
			return TryParseEnum<T>(dictionary, key, out T value) ? value : defaultValue;
		}

		public static bool TryParseBool(this IDatDictionary dictionary, string key, out bool value)
		{
			value = default;
			return TryGetValue(dictionary, key, out IDatValue node) && node.TryParseBool(out value);
		}

		public static bool ParseBool(this IDatDictionary dictionary, string key, bool defaultValue = default)
		{
			return TryParseBool(dictionary, key, out bool value) ? value : defaultValue;
		}

		public static bool TryParseGuid(this IDatDictionary dictionary, string key, out System.Guid value)
		{
			value = default;
			return TryGetValue(dictionary, key, out IDatValue node) && node.TryParseGuid(out value);
		}

		public static System.Guid ParseGuid(this IDatDictionary dictionary, string key, System.Guid defaultValue = default)
		{
			return TryParseGuid(dictionary, key, out System.Guid value) ? value : defaultValue;
		}

		public static bool TryParseDateTimeUtc(this IDatDictionary dictionary, string key, out System.DateTime value)
		{
			value = default;
			return TryGetValue(dictionary, key, out IDatValue node) && node.TryParseDateTimeUtc(out value);
		}

		public static System.DateTime ParseDateTimeUtc(this IDatDictionary dictionary, string key, System.DateTime defaultValue = default)
		{
			return TryParseDateTimeUtc(dictionary, key, out System.DateTime value) ? value : defaultValue;
		}

		public static System.Type ParseType(this IDatDictionary dictionary, string key, System.Type defaultValue = default)
		{
			System.Type value = defaultValue;
			if (TryGetValue(dictionary, key, out IDatValue node))
			{
				value = node.ParseType(defaultValue);
			}
			return value;
		}

		public static bool TryParseStruct<T>(this IDatDictionary dictionary, string key, out T value) where T : struct, IDatParseable
		{
			value = default;
			return dictionary.TryGetNode(key, out IDatNode node) && value.TryParse(node);
		}

		public static T ParseStruct<T>(this IDatDictionary dictionary, string key, T defaultValue = default) where T : struct, IDatParseable
		{
			return TryParseStruct<T>(dictionary, key, out T value) ? value : defaultValue;
		}

		/// <summary>
		/// TryParseStruct from each node in list and only add results which returned true.
		/// Returns null if list does not exist.
		/// </summary>
		public static List<T> ParseListOfStructs<T>(this IDatDictionary dictionary, string key) where T : struct, IDatParseable
		{
			return TryGetList(dictionary, key, out IDatList list) ? list.ParseListOfStructs<T>() : null;
		}

		/// <summary>
		/// Return array with the same length as list and call TryParseStruct on each node.
		/// Returns null if list does not exist.
		/// </summary>
		public static T[] ParseArrayOfStructs<T>(this IDatDictionary dictionary, string key, T defaultValue = default) where T : struct, IDatParseable
		{
			return TryGetList(dictionary, key, out IDatList list) ? list.ParseArrayOfStructs<T>(defaultValue) : null;
		}

		/// <summary>
		/// If key already exists with wrong type a dictionary is substituted, preserving line number.
		/// </summary>
		public static IEditableDatDictionary GetOrAddDictionary(this IEditableDatDictionary dictionary, string key, out bool isNew)
		{
			if (dictionary.TryGetNode(key, out IDatNode node))
			{
				isNew = false;
				if (node is IDatDictionary existingDictionary)
				{
					return existingDictionary.Edit();
				}
				else
				{
					return dictionary.ReplaceWithDictionary(key);
				}
			}
			else
			{
				isNew = true;
				return dictionary.AddDictionary(key);
			}
		}

		public static IEditableDatDictionary GetOrAddDictionary(this IEditableDatDictionary dictionary, string key)
		{
			return GetOrAddDictionary(dictionary, key, out bool isNew);
		}

		/// <summary>
		/// If key already exists with wrong type a value is substituted, preserving line number.
		/// </summary>
		public static IEditableDatList GetOrAddList(this IEditableDatDictionary dictionary, string key, out bool isNew)
		{
			if (dictionary.TryGetNode(key, out IDatNode node))
			{
				isNew = false;
				if (node is IDatList existingList)
				{
					return existingList.Edit();
				}
				else
				{
					return dictionary.ReplaceWithList(key);
				}
			}
			else
			{
				isNew = true;
				return dictionary.AddList(key);
			}
		}

		public static IEditableDatList GetOrAddList(this IEditableDatDictionary dictionary, string key)
		{
			return GetOrAddList(dictionary, key, out bool isNew);
		}

		/// <summary>
		/// If key already exists with wrong type a value is substituted, preserving line number.
		/// </summary>
		public static IEditableDatValue GetOrAddValue(this IEditableDatDictionary dictionary, string key, out bool isNew)
		{
			if (dictionary.TryGetNode(key, out IDatNode node))
			{
				isNew = false;
				if (node is IDatValue existingValue)
				{
					return existingValue.Edit();
				}
				else
				{
					return dictionary.ReplaceWithValue(key);
				}
			}
			else
			{
				isNew = true;
				return dictionary.AddValue(key);
			}
		}

		public static IEditableDatValue GetOrAddValue(this IEditableDatDictionary dictionary, string key)
		{
			return GetOrAddValue(dictionary, key, out bool isNew);
		}
	}
}
