////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;

internal class DatDictionaryTests
{
	[TestCase("0", true, 0)]
	[TestCase("-129", false, 0)]
	[TestCase("-128", true, sbyte.MinValue)]
	[TestCase("1", true, 1)]
	[TestCase("127", true, sbyte.MaxValue)]
	[TestCase("x", false, 0)]
	public void TryParseInt8(string input, bool expectedSuccess, sbyte expectedValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseInt8("key", out sbyte actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("0", 0, 9)]
	[TestCase("-129", 3, 3)]
	[TestCase("-128", sbyte.MinValue, 0)]
	[TestCase("1", 1, 0)]
	[TestCase("127", sbyte.MaxValue, 0)]
	[TestCase("x", 2, 2)]
	public void ParseInt8(string input, sbyte expectedValue, sbyte defaultValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseInt8("key", defaultValue));
	}

	[TestCase("0", true, 0)]
	[TestCase("-1", false, 0)]
	[TestCase("1", true, 1)]
	[TestCase("255", true, byte.MaxValue)]
	[TestCase("x", false, 0)]
	public void TryParseUInt8(string input, bool expectedSuccess, byte expectedValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseUInt8("key", out byte actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("0", 0, 9)]
	[TestCase("-1", 3, 3)]
	[TestCase("1", 1, 0)]
	[TestCase("255", byte.MaxValue, 0)]
	[TestCase("x", 2, 2)]
	public void ParseUInt8(string input, byte expectedValue, byte defaultValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseUInt8("key", defaultValue));
	}

	[TestCase("0", true, 0)]
	[TestCase("-32768", true, short.MinValue)]
	[TestCase("-32769", false, 0)]
	[TestCase("1", true, 1)]
	[TestCase("32767", true, short.MaxValue)]
	[TestCase("32768", false, 0)]
	[TestCase("x", false, 0)]
	public void TryParseInt16(string input, bool expectedSuccess, short expectedValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseInt16("key", out short actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("0", 0, 9)]
	[TestCase("-32768", -32768, 3)]
	[TestCase("-32769", 3, 3)]
	[TestCase("1", 1, 0)]
	[TestCase("32767", 32767, 3)]
	[TestCase("32768", 3, 3)]
	[TestCase("x", 2, 2)]
	public void ParseInt16(string input, short expectedValue, short defaultValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseInt16("key", defaultValue));
	}

	[TestCase("0", true, (ushort) 0)]
	[TestCase("-1", false, (ushort) 0)]
	[TestCase("1", true, (ushort) 1)]
	[TestCase("65535", true, ushort.MaxValue)]
	[TestCase("65536", false, (ushort) 0)]
	[TestCase("x", false, (ushort) 0)]
	public void TryParseUInt16(string input, bool expectedSuccess, ushort expectedValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseUInt16("key", out ushort actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("0", (ushort) 0, (ushort) 9)]
	[TestCase("-1", (ushort) 3, (ushort) 3)]
	[TestCase("1", (ushort) 1, (ushort) 0)]
	[TestCase("65535", ushort.MaxValue, (ushort) 0)]
	[TestCase("65536", (ushort) 3, (ushort) 3)]
	[TestCase("x", (ushort) 2, (ushort) 2)]
	public void ParseUInt16(string input, ushort expectedValue, ushort defaultValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseUInt16("key", defaultValue));
	}

	[TestCase("0", true, 0)]
	[TestCase("-2,147,483,648", true, int.MinValue)]
	[TestCase("-2,147,483,649", false, 0)]
	[TestCase("1", true, 1)]
	[TestCase("2,147,483,647", true, int.MaxValue)]
	[TestCase("2,147,483,648", false, 0)]
	[TestCase("x", false, 0)]
	public void TryParseInt32(string input, bool expectedSuccess, int expectedValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseInt32("key", out int actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("0", 0, 9)]
	[TestCase("-2,147,483,648", int.MinValue, 3)]
	[TestCase("-2,147,483,649", 3, 3)]
	[TestCase("1", 1, 0)]
	[TestCase("2,147,483,647", int.MaxValue, 3)]
	[TestCase("2,147,483,648", 3, 3)]
	[TestCase("x", 2, 2)]
	public void ParseInt32(string input, int expectedValue, int defaultValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseInt32("key", defaultValue));
	}

	[TestCase("0", true, (uint) 0)]
	[TestCase("-1", false, (uint) 0)]
	[TestCase("1", true, (uint) 1)]
	[TestCase("4,294,967,295", true, uint.MaxValue)]
	[TestCase("4,294,967,296", false, (uint) 0)]
	[TestCase("x", false, (uint) 0)]
	public void TryParseUInt32(string input, bool expectedSuccess, uint expectedValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseUInt32("key", out uint actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("0", (uint) 0, (uint) 9)]
	[TestCase("-1", (uint) 3, (uint) 3)]
	[TestCase("1", (uint) 1, (uint) 0)]
	[TestCase("4,294,967,295", uint.MaxValue, (uint) 0)]
	[TestCase("4,294,967,296", (uint) 3, (uint) 3)]
	[TestCase("x", (uint) 2, (uint) 2)]
	public void ParseUInt32(string input, uint expectedValue, uint defaultValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseUInt32("key", defaultValue));
	}

	[TestCase("0", true, 0)]
	[TestCase("-9,223,372,036,854,775,808", true, long.MinValue)]
	[TestCase("-9,223,372,036,854,775,809", false, 0)]
	[TestCase("1", true, 1)]
	[TestCase("9,223,372,036,854,775,807", true, long.MaxValue)]
	[TestCase("9,223,372,036,854,775,808", false, 0)]
	[TestCase("x", false, 0)]
	public void TryParseInt64(string input, bool expectedSuccess, long expectedValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseInt64("key", out long actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("0", 0, 9)]
	[TestCase("-9,223,372,036,854,775,808", long.MinValue, 3)]
	[TestCase("-9,223,372,036,854,775,809", 3, 3)]
	[TestCase("1", 1, 0)]
	[TestCase("9,223,372,036,854,775,807", long.MaxValue, 3)]
	[TestCase("9,223,372,036,854,775,808", 3, 3)]
	[TestCase("x", 2, 2)]
	public void ParseInt64(string input, long expectedValue, long defaultValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseInt64("key", defaultValue));
	}

	[TestCase("0", true, (ulong) 0)]
	[TestCase("-1", false, (ulong) 0)]
	[TestCase("1", true, (ulong) 1)]
	[TestCase("18,446,744,073,709,551,615", true, ulong.MaxValue)]
	[TestCase("18,446,744,073,709,551,616", false, (ulong) 0)]
	[TestCase("x", false, (ulong) 0)]
	public void TryParseUInt64(string input, bool expectedSuccess, ulong expectedValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseUInt64("key", out ulong actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("0", (ulong) 0, (ulong) 9)]
	[TestCase("-1", (ulong) 3, (ulong) 3)]
	[TestCase("1", (ulong) 1, (ulong) 0)]
	[TestCase("18,446,744,073,709,551,615", ulong.MaxValue, (ulong) 0)]
	[TestCase("18,446,744,073,709,551,616", (ulong) 3, (ulong) 3)]
	[TestCase("x", (ulong) 2, (ulong) 2)]
	public void ParseUInt64(string input, ulong expectedValue, ulong defaultValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseUInt64("key", defaultValue));
	}

	[TestCase("0", true, 0.0f)]
	[TestCase("0.0", true, 0.0f)]
	[TestCase("-1", true, -1.0f)]
	[TestCase("-1.0", true, -1.0f)]
	[TestCase("1", true, 1.0f)]
	[TestCase("1.0", true, 1.0f)]
	[TestCase("x", false, 0.0f)]
	public void TryParseFloat(string input, bool expectedSuccess, float expectedValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseFloat("key", out float actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("0", 0.0f, 9.0f)]
	[TestCase("0.0", 0.0f, 9.0f)]
	[TestCase("-1", -1.0f, 9.0f)]
	[TestCase("-1.0", -1.0f, 9.0f)]
	[TestCase("1", 1.0f, 9.0f)]
	[TestCase("1.0", 1.0f, 9.0f)]
	[TestCase("x", 2.0f, 2.0f)]
	public void ParseFloat(string input, float expectedValue, float defaultValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseFloat("key", defaultValue));
	}

	[TestCase("0", true, 0.0d)]
	[TestCase("0.0", true, 0.0d)]
	[TestCase("-1", true, -1.0d)]
	[TestCase("-1.0", true, -1.0d)]
	[TestCase("1", true, 1.0d)]
	[TestCase("1.0", true, 1.0d)]
	[TestCase("x", false, 0.0d)]
	public void TryParseDouble(string input, bool expectedSuccess, double expectedValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseDouble("key", out double actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("0", 0.0f, 9.0d)]
	[TestCase("0.0", 0.0f, 9.0d)]
	[TestCase("-1", -1.0f, 9.0d)]
	[TestCase("-1.0", -1.0f, 9.0d)]
	[TestCase("1", 1.0f, 9.0d)]
	[TestCase("1.0", 1.0f, 9.0d)]
	[TestCase("x", 2.0f, 2.0d)]
	public void ParseDouble(string input, double expectedValue, double defaultValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseDouble("key", defaultValue));
	}

	[TestCase("t", true, true)]
	[TestCase("y", true, true)]
	[TestCase("1", true, true)]
	[TestCase("f", true, false)]
	[TestCase("n", true, false)]
	[TestCase("0", true, false)]
	[TestCase("true", true, true)]
	[TestCase("false", true, false)]
	[TestCase("x", false, false)]
	[TestCase("xyz", false, false)]
	public void TryParseBool(string input, bool expectedSuccess, bool expectedValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseBool("key", out bool actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("t", true, false)]
	[TestCase("f", false, true)]
	[TestCase("x", false, false)]
	public void ParseBool(string input, bool expectedValue, bool defaultValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseBool("key", defaultValue));
	}

	public enum EDatTestEnum
	{
		None,
		Bear,
		Dog,
	}

	[TestCase("None", true, EDatTestEnum.None)]
	[TestCase("Bear", true, EDatTestEnum.Bear)]
	[TestCase("Dog", true, EDatTestEnum.Dog)]
	[TestCase("Other", false, EDatTestEnum.None)]
	public void TryParseEnum(string input, bool expectedSuccess, EDatTestEnum expectedValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseEnum("key", out EDatTestEnum actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("Bear", EDatTestEnum.Bear, EDatTestEnum.Dog)]
	[TestCase("Other", EDatTestEnum.Dog, EDatTestEnum.Dog)]
	public void ParseEnum(string input, EDatTestEnum expectedValue, EDatTestEnum defaultValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseEnum("key", defaultValue));
	}

	[System.Flags]
	public enum EDatTestEnumFlags
	{
		None = 0,
		Flag1 = 1 << 0,
		Flag2 = 2 << 1,
	}

	[TestCase("None", true, EDatTestEnumFlags.None)]
	[TestCase("Flag2", true, EDatTestEnumFlags.Flag2)]
	[TestCase("Flag1, Flag2", true, EDatTestEnumFlags.Flag1 | EDatTestEnumFlags.Flag2)]
	[TestCase("Other", false, EDatTestEnumFlags.None)]
	public void TryParseEnumFlags(string input, bool expectedSuccess, EDatTestEnumFlags expectedValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseEnum("key", out EDatTestEnumFlags actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("Flag2", EDatTestEnumFlags.Flag2, EDatTestEnumFlags.Flag1)]
	[TestCase("Other", EDatTestEnumFlags.Flag1, EDatTestEnumFlags.Flag1)]
	public void ParseEnumFlags(string input, EDatTestEnumFlags expectedValue, EDatTestEnumFlags defaultValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseEnum("key", defaultValue));
	}

	[TestCase("6f65c8b5c7834d0c8898b881a667c527", true, "6f65c8b5c7834d0c8898b881a667c527")]
	[TestCase("00000000000000000000000000000000", true, "00000000000000000000000000000000")]
	[TestCase("xyz", false, "00000000000000000000000000000000")]
	public void TryParseGuid(string input, bool expectedSuccess, string expectedValueString)
	{
		System.Guid expectedValue = new System.Guid(expectedValueString);
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseGuid("key", out System.Guid actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("6f65c8b5c7834d0c8898b881a667c527", "6f65c8b5c7834d0c8898b881a667c527", "00000000000000000000000000000000")]
	[TestCase("00000000000000000000000000000000", "00000000000000000000000000000000", "6f65c8b5c7834d0c8898b881a667c527")]
	[TestCase("x", "6f65c8b5c7834d0c8898b881a667c527", "6f65c8b5c7834d0c8898b881a667c527")]
	public void ParseGuid(string input, string expectedValueString, string defaultValueString)
	{
		System.Guid expectedValue = new System.Guid(expectedValueString);
		System.Guid defaultValue = new System.Guid(defaultValueString);
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseGuid("key", defaultValue));
	}

	[TestCase("2023-04-19", true, /*year*/ 2023, /*month*/ 4, /*day*/ 19, /*hour*/ 0, /*minute*/ 0, /*second*/ 0)]
	[TestCase("2023-04-19 6:00", true, /*year*/ 2023, /*month*/ 4, /*day*/ 19, /*hour*/ 6, /*minute*/ 0, /*second*/ 0)]
	[TestCase("2023-04-19 13:21", true, /*year*/ 2023, /*month*/ 4, /*day*/ 19, /*hour*/ 13, /*minute*/ 21, /*second*/ 0)]
	[TestCase("2023-04-19 13:21:30", true, /*year*/ 2023, /*month*/ 4, /*day*/ 19, /*hour*/ 13, /*minute*/ 21, /*second*/ 30)]
	public void TryParseDateTimeUtc(string input, bool expectedSuccess, int expectedYear, int expectedMonth, int expectedDay, int expectedHour, int expectedMinute, int expectedSecond)
	{
		System.DateTime expectedValue = new System.DateTime(expectedYear, expectedMonth, expectedDay, expectedHour, expectedMinute, expectedSecond, System.DateTimeKind.Utc);
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseDateTimeUtc("key", out System.DateTime actualValue));
		Assert.AreEqual(expectedValue, actualValue);
		Assert.AreEqual(System.DateTimeKind.Utc, actualValue.Kind);
	}

	[TestCase("2023-04-19 10:48 AM", /*year*/ 2023, /*month*/ 4, /*day*/ 19, /*hour*/ 10, /*minute*/ 48, /*second*/ 0, /*year*/ 2020, /*month*/ 3, /*day*/ 16, /*hour*/ 0, /*minute*/ 0, /*second*/ 0)]
	[TestCase("invalid", /*year*/ 2020, /*month*/ 3, /*day*/ 16, /*hour*/ 0, /*minute*/ 0, /*second*/ 0, /*year*/ 2020, /*month*/ 3, /*day*/ 16, /*hour*/ 0, /*minute*/ 0, /*second*/ 0)]
	public void ParseDateTimeUtc(string input, int expectedYear, int expectedMonth, int expectedDay, int expectedHour, int expectedMinute, int expectedSecond, int defaultYear, int defaultMonth, int defaultDay, int defaultHour, int defaultMinute, int defaultSecond)
	{
		System.DateTime expectedValue = new System.DateTime(expectedYear, expectedMonth, expectedDay, expectedHour, expectedMinute, expectedSecond, System.DateTimeKind.Utc);
		System.DateTime defaultValue = new System.DateTime(defaultYear, defaultMonth, defaultDay, defaultHour, defaultMinute, defaultSecond, System.DateTimeKind.Utc);
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		System.DateTime actualValue = dictionary.ParseDateTimeUtc("key", defaultValue);
		Assert.AreEqual(expectedValue, actualValue);
		Assert.AreEqual(System.DateTimeKind.Utc, actualValue.Kind);
	}

	[TestCase("System.Int32", typeof(int), null)]
	[TestCase("abc", typeof(int), typeof(int))]
	[TestCase("SDG.Unturned.IDatValue", typeof(IDatValue), typeof(int))]
	public void ParseType(string input, System.Type expectedValue, System.Type defaultValue)
	{
		DatDictionary dictionary = new DatDictionary();
		dictionary["key"] = new DatValue(input);
		Assert.AreEqual(expectedValue, dictionary.ParseType("key", defaultValue));
	}
}
