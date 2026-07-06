////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public static class DatValueEx
	{
		public static bool IsValueNullOrEmpty(this IDatValue valueNode)
		{
			return valueNode == null || string.IsNullOrEmpty(valueNode.Value);
		}

		public static bool TryParseInt8(this IDatValue valueNode, out sbyte value)
		{
			return sbyte.TryParse(valueNode.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
		}

		public static sbyte ParseInt8(this IDatValue valueNode, sbyte defaultValue = default)
		{
			return TryParseInt8(valueNode, out sbyte value) ? value : defaultValue;
		}

		public static bool TryParseUInt8(this IDatValue valueNode, out byte value)
		{
			return byte.TryParse(valueNode.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
		}

		public static byte ParseUInt8(this IDatValue valueNode, byte defaultValue = default)
		{
			return TryParseUInt8(valueNode, out byte value) ? value : defaultValue;
		}

		public static bool TryParseInt16(this IDatValue valueNode, out short value)
		{
			return short.TryParse(valueNode.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
		}

		public static short ParseInt16(this IDatValue valueNode, short defaultValue = default)
		{
			return TryParseInt16(valueNode, out short value) ? value : defaultValue;
		}

		public static bool TryParseUInt16(this IDatValue valueNode, out ushort value)
		{
			return ushort.TryParse(valueNode.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
		}

		public static ushort ParseUInt16(this IDatValue valueNode, ushort defaultValue = default)
		{
			return TryParseUInt16(valueNode, out ushort value) ? value : defaultValue;
		}

		public static bool TryParseInt32(this IDatValue valueNode, out int value)
		{
			return int.TryParse(valueNode.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
		}

		public static int ParseInt32(this IDatValue valueNode, int defaultValue = default)
		{
			return TryParseInt32(valueNode, out int value) ? value : defaultValue;
		}

		public static bool TryParseUInt32(this IDatValue valueNode, out uint value)
		{
			return uint.TryParse(valueNode.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
		}

		public static uint ParseUInt32(this IDatValue valueNode, uint defaultValue = default)
		{
			return TryParseUInt32(valueNode, out uint value) ? value : defaultValue;
		}

		public static bool TryParseInt64(this IDatValue valueNode, out long value)
		{
			return long.TryParse(valueNode.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
		}

		public static long ParseInt64(this IDatValue valueNode, long defaultValue = default)
		{
			return TryParseInt64(valueNode, out long value) ? value : defaultValue;
		}

		public static bool TryParseUInt64(this IDatValue valueNode, out ulong value)
		{
			return ulong.TryParse(valueNode.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
		}

		public static ulong ParseUInt64(this IDatValue valueNode, ulong defaultValue = default)
		{
			return TryParseUInt64(valueNode, out ulong value) ? value : defaultValue;
		}

		public static bool TryParseFloat(this IDatValue valueNode, out float value)
		{
			return float.TryParse(valueNode.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
		}

		public static float ParseFloat(this IDatValue valueNode, float defaultValue = default)
		{
			return TryParseFloat(valueNode, out float value) ? value : defaultValue;
		}

		public static bool TryParseDouble(this IDatValue valueNode, out double value)
		{
			return double.TryParse(valueNode.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
		}

		public static double ParseDouble(this IDatValue valueNode, double defaultValue = default)
		{
			return TryParseDouble(valueNode, out double value) ? value : defaultValue;
		}

		public static bool TryParseEnum<T>(this IDatValue valueNode, out T value) where T : struct
		{
			return System.Enum.TryParse(valueNode.Value, /*ignoreCase*/ true, out value);
		}

		public static T ParseEnum<T>(this IDatValue valueNode, T defaultValue) where T : struct
		{
			return TryParseEnum<T>(valueNode, out T value) ? value : defaultValue;
		}

		public static bool TryParseEnum(this IDatValue valueNode, System.Type enumType, out object value)
		{
			return System.Enum.TryParse(enumType, valueNode.Value, /*ignoreCase*/ true, out value);
		}

		public static object ParseEnum(this IDatValue valueNode, System.Type enumType, object defaultValue)
		{
			return TryParseEnum(valueNode, enumType, out object value) ? value : defaultValue;
		}

		public static bool TryParseBool(this IDatValue valueNode, out bool value)
		{
			if (!string.IsNullOrEmpty(valueNode.Value))
			{
				if (valueNode.Value.Length == 1)
				{
					char letter = valueNode.Value[0];
					if (letter == 'y' || letter == 't' || letter == '1')
					{
						value = true;
						return true;
					}
					else if (letter == 'n' || letter == 'f' || letter == '0')
					{
						value = false;
						return true;
					}
				}
				else
				{
					return bool.TryParse(valueNode.Value, out value);
				}
			}

			value = default;
			return false;
		}

		public static bool ParseBool(this IDatValue valueNode, bool defaultValue = default)
		{
			return TryParseBool(valueNode, out bool value) ? value : defaultValue;
		}

		public static bool TryParseGuid(this IDatValue valueNode, out System.Guid value)
		{
			return System.Guid.TryParse(valueNode.Value, out value);
		}

		public static System.Guid ParseGuid(this IDatValue valueNode, System.Guid defaultValue = default)
		{
			return TryParseGuid(valueNode, out System.Guid value) ? value : defaultValue;
		}

		public static bool TryParseDateTimeUtc(this IDatValue valueNode, out System.DateTime value)
		{
			// Nelson 2023-07-24: AssumeUniversal doesn't set Kind to UTC, rather it parses as UTC
			// and converts to local which is why we call ToUniversalTime afterwards.
			System.Globalization.DateTimeStyles styles = System.Globalization.DateTimeStyles.AssumeUniversal;
			bool success = System.DateTime.TryParse(valueNode.Value, System.Globalization.CultureInfo.InvariantCulture, styles, out value);
			value = value.ToUniversalTime();
			return success;
		}

		public static System.DateTime ParseDateTimeUtc(this IDatValue valueNode, System.DateTime defaultValue = default)
		{
			return TryParseDateTimeUtc(valueNode, out System.DateTime value) ? value : defaultValue;
		}

		public static System.Type ParseType(this IDatValue valueNode, System.Type defaultValue = default)
		{
			if (string.IsNullOrEmpty(valueNode.Value) || valueNode.Value.IndexOfAny(DatValue.INVALID_TYPE_CHARS) >= 0)
			{
				return defaultValue;
			}

			System.Type value = System.Type.GetType(valueNode.Value, /*throwOnError*/ false, /*ignoreCase*/ true);
			return value != null ? value : defaultValue;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// Note: this one in particular may seem a little silly, but it preserves the appropriate IDatValue type.
		/// </summary>
		public static TValueNode SetString<TValueNode>(this TValueNode valueNode, string value) where TValueNode : IDatValue
		{
			valueNode.Value = value;
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetInt8<TValueNode>(this TValueNode valueNode, sbyte value) where TValueNode : IDatValue
		{
			valueNode.Value = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetUInt8<TValueNode>(this TValueNode valueNode, byte value) where TValueNode : IDatValue
		{
			valueNode.Value = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetInt16<TValueNode>(this TValueNode valueNode, short value) where TValueNode : IDatValue
		{
			valueNode.Value = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetUInt16<TValueNode>(this TValueNode valueNode, ushort value) where TValueNode : IDatValue
		{
			valueNode.Value = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetInt32<TValueNode>(this TValueNode valueNode, int value) where TValueNode : IDatValue
		{
			valueNode.Value = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetUInt32<TValueNode>(this TValueNode valueNode, uint value) where TValueNode : IDatValue
		{
			valueNode.Value = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetInt64<TValueNode>(this TValueNode valueNode, long value) where TValueNode : IDatValue
		{
			valueNode.Value = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetUInt64<TValueNode>(this TValueNode valueNode, ulong value) where TValueNode : IDatValue
		{
			valueNode.Value = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetFloat<TValueNode>(this TValueNode valueNode, float value) where TValueNode : IDatValue
		{
			valueNode.Value = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetDouble<TValueNode>(this TValueNode valueNode, double value) where TValueNode : IDatValue
		{
			valueNode.Value = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetBool<TValueNode>(this TValueNode valueNode, bool value) where TValueNode : IDatValue
		{
			valueNode.Value = value ? "true" : "false";
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetGuid<TValueNode>(this TValueNode valueNode, System.Guid value) where TValueNode : IDatValue
		{
			valueNode.Value = value.ToString("N");
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetEnumString<TValueNode, TEnum>(this TValueNode valueNode, TEnum value) where TValueNode : IDatValue where TEnum : struct
		{
			valueNode.Value = value.ToString();
			return valueNode;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetDateTimeUtc<TValueNode>(this TValueNode valueNode, System.DateTime value) where TValueNode : IDatValue
		{
			if (value.Hour == 0 && value.Minute == 0 && value.Second == 0)
			{
				valueNode.Value = value.ToString("yyyy'-'MM'-'dd", System.Globalization.CultureInfo.InvariantCulture);
			}
			else
			{
				valueNode.Value = value.ToString("yyyy'-'MM'-'dd HH':'mm':'ss", System.Globalization.CultureInfo.InvariantCulture);
			}
			return valueNode;
		}
	}
}
