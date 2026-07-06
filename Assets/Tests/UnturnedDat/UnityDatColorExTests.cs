////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;
using UnityEngine;

internal class UnityDatColorExTests
{
	[TestCase("ff0000", true, 255, 0, 0)]
	[TestCase("00ff00", true, 0, 255, 0)]
	[TestCase("0000ff", true, 0, 0, 255)]
	[TestCase("#ff0000", true, 255, 0, 0)]
	[TestCase("#00ff00", true, 0, 255, 0)]
	[TestCase("#0000ff", true, 0, 0, 255)]
	[TestCase("", false, 0, 0, 0)]
	[TestCase("#", false, 0, 0, 0)]
	[TestCase("      ", false, 0, 0, 0)]
	[TestCase("#      ", false, 0, 0, 0)]
	[TestCase("zzzzzz", false, 0, 0, 0)]
	[TestCase("#zzzzzz", false, 0, 0, 0)]
	public void ValueTryParseColor32RGB(string input, bool expectedSuccess, byte expected_r, byte expected_g, byte expected_b)
	{
		Color32 expectedValue = new Color32(expected_r, expected_g, expected_b, byte.MaxValue);
		IDatValue value = new DatValue(input);
		Assert.AreEqual(expectedSuccess, value.TryParseColor32RGB(out Color32 actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("ff0000", 255, 0, 0, 127, 127, 127)]
	[TestCase("00ff00", 0, 255, 0, 127, 127, 127)]
	[TestCase("0000ff", 0, 0, 255, 127, 127, 127)]
	[TestCase("#ff0000", 255, 0, 0, 127, 127, 127)]
	[TestCase("#00ff00", 0, 255, 0, 127, 127, 127)]
	[TestCase("#0000ff", 0, 0, 255, 127, 127, 127)]
	[TestCase("", 127, 127, 127, 127, 127, 127)]
	[TestCase("#", 127, 127, 127, 127, 127, 127)]
	[TestCase("      ", 127, 127, 127, 127, 127, 127)]
	[TestCase("#      ", 127, 127, 127, 127, 127, 127)]
	[TestCase("zzzzzz", 127, 127, 127, 127, 127, 127)]
	[TestCase("#zzzzzz", 127, 127, 127, 127, 127, 127)]
	public void ValueParseColor32RGB(string input, byte expected_r, byte expected_g, byte expected_b, byte default_r, byte default_g, byte default_b)
	{
		Color32 expectedValue = new Color32(expected_r, expected_g, expected_b, byte.MaxValue);
		Color32 defaultValue = new Color32(default_r, default_g, default_b, byte.MaxValue);
		IDatValue value = new DatValue(input);
		Assert.AreEqual(expectedValue, value.ParseColor32RGB(defaultValue));
	}

	[TestCase("key ff0000", true, 255, 0, 0)]
	[TestCase("key 00ff00", true, 0, 255, 0)]
	[TestCase("key 0000ff", true, 0, 0, 255)]
	[TestCase("key #ff0000", true, 255, 0, 0)]
	[TestCase("key #00ff00", true, 0, 255, 0)]
	[TestCase("key #0000ff", true, 0, 0, 255)]
	[TestCase("key ", false, 0, 0, 0)]
	[TestCase("key #", false, 0, 0, 0)]
	[TestCase("key       ", false, 0, 0, 0)]
	[TestCase("key #      ", false, 0, 0, 0)]
	[TestCase("key zzzzzz", false, 0, 0, 0)]
	[TestCase("key #zzzzzz", false, 0, 0, 0)]
	[TestCase("key\n{\n}", true, 0, 0, 0)]
	[TestCase("key\n{\nr 1\ng 2\nb 3\n}", true, 1, 2, 3)]
	public void DictionaryTryParseColor32RGB(string input, bool expectedSuccess, byte expected_r, byte expected_g, byte expected_b)
	{
		Color32 expectedValue = new Color32(expected_r, expected_g, expected_b, byte.MaxValue);
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseColor32RGB("key", out Color32 actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("key ff0000", 255, 0, 0, 127, 127, 127)]
	[TestCase("key 00ff00", 0, 255, 0, 127, 127, 127)]
	[TestCase("key 0000ff", 0, 0, 255, 127, 127, 127)]
	[TestCase("key #ff0000", 255, 0, 0, 127, 127, 127)]
	[TestCase("key #00ff00", 0, 255, 0, 127, 127, 127)]
	[TestCase("key #0000ff", 0, 0, 255, 127, 127, 127)]
	[TestCase("key ", 127, 127, 127, 127, 127, 127)]
	[TestCase("key #", 127, 127, 127, 127, 127, 127)]
	[TestCase("key       ", 127, 127, 127, 127, 127, 127)]
	[TestCase("key #      ", 127, 127, 127, 127, 127, 127)]
	[TestCase("key zzzzzz", 127, 127, 127, 127, 127, 127)]
	[TestCase("key #zzzzzz", 127, 127, 127, 127, 127, 127)]
	[TestCase("key\n{\n}", 0, 0, 0, 127, 128, 129)]
	[TestCase("key\n{\nr 1\ng 2\nb 3\n}", 1, 2, 3, 127, 128, 129)]
	public void DictionaryParseColor32RGB(string input, byte expected_r, byte expected_g, byte expected_b, byte default_r, byte default_g, byte default_b)
	{
		Color32 expectedValue = new Color32(expected_r, expected_g, expected_b, byte.MaxValue);
		Color32 defaultValue = new Color32(default_r, default_g, default_b, byte.MaxValue);
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.AreEqual(expectedValue, dictionary.ParseColor32RGB("key", defaultValue));
	}

	[TestCase("ff0000", true, 255, 0, 0, 255)]
	[TestCase("00ff00", true, 0, 255, 0, 255)]
	[TestCase("0000ff", true, 0, 0, 255, 255)]
	[TestCase("#ff0000", true, 255, 0, 0, 255)]
	[TestCase("#00ff00", true, 0, 255, 0, 255)]
	[TestCase("#0000ff", true, 0, 0, 255, 255)]
	[TestCase("ff00007f", true, 255, 0, 0, 127)]
	[TestCase("00ff007f", true, 0, 255, 0, 127)]
	[TestCase("0000ff7f", true, 0, 0, 255, 127)]
	[TestCase("#ff00007f", true, 255, 0, 0, 127)]
	[TestCase("#00ff007f", true, 0, 255, 0, 127)]
	[TestCase("#0000ff7f", true, 0, 0, 255, 127)]
	[TestCase("", false, 0, 0, 0, 0)]
	[TestCase("#", false, 0, 0, 0, 0)]
	[TestCase("        ", false, 0, 0, 0, 0)]
	[TestCase("#        ", false, 0, 0, 0, 0)]
	[TestCase("zzzzzz", false, 0, 0, 0, 0)]
	[TestCase("#zzzzzz", false, 0, 0, 0, 0)]
	[TestCase("zzzzzzzz", false, 0, 0, 0, 0)]
	[TestCase("#zzzzzzzz", false, 0, 0, 0, 0)]
	public void ValueTryParseColor32RGBA(string input, bool expectedSuccess, byte expected_r, byte expected_g, byte expected_b, byte expected_a)
	{
		Color32 expectedValue = new Color32(expected_r, expected_g, expected_b, expected_a);
		IDatValue value = new DatValue(input);
		Assert.AreEqual(expectedSuccess, value.TryParseColor32RGBA(out Color32 actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("ff0000", 255, 0, 0, 255, 127, 127, 127, 0)]
	[TestCase("00ff00", 0, 255, 0, 255, 127, 127, 127, 0)]
	[TestCase("0000ff", 0, 0, 255, 255, 127, 127, 127, 0)]
	[TestCase("#ff0000", 255, 0, 0, 255, 127, 127, 127, 0)]
	[TestCase("#00ff00", 0, 255, 0, 255, 127, 127, 127, 0)]
	[TestCase("#0000ff", 0, 0, 255, 255, 127, 127, 127, 0)]
	[TestCase("ff00007f", 255, 0, 0, 127, 127, 127, 127, 0)]
	[TestCase("00ff007f", 0, 255, 0, 127, 127, 127, 127, 0)]
	[TestCase("0000ff7f", 0, 0, 255, 127, 127, 127, 127, 0)]
	[TestCase("#ff00007f", 255, 0, 0, 127, 127, 127, 127, 0)]
	[TestCase("#00ff007f", 0, 255, 0, 127, 127, 127, 127, 0)]
	[TestCase("#0000ff7f", 0, 0, 255, 127, 127, 127, 127, 0)]
	[TestCase("", 127, 127, 127, 127, 127, 127, 127, 127)]
	[TestCase("#", 127, 127, 127, 127, 127, 127, 127, 127)]
	[TestCase("      ", 127, 127, 127, 127, 127, 127, 127, 127)]
	[TestCase("#      ", 127, 127, 127, 127, 127, 127, 127, 127)]
	[TestCase("zzzzzz", 127, 127, 127, 127, 127, 127, 127, 127)]
	[TestCase("#zzzzzz", 127, 127, 127, 127, 127, 127, 127, 127)]
	public void ValueParseColor32RGBA(string input, byte expected_r, byte expected_g, byte expected_b, byte expected_a, byte default_r, byte default_g, byte default_b, byte default_a)
	{
		Color32 expectedValue = new Color32(expected_r, expected_g, expected_b, expected_a);
		Color32 defaultValue = new Color32(default_r, default_g, default_b, default_a);
		IDatValue value = new DatValue(input);
		Assert.AreEqual(expectedValue, value.ParseColor32RGBA(defaultValue));
	}

	[TestCase("key ff0000", true, 255, 0, 0, 255)]
	[TestCase("key 00ff00", true, 0, 255, 0, 255)]
	[TestCase("key 0000ff", true, 0, 0, 255, 255)]
	[TestCase("key #ff0000", true, 255, 0, 0, 255)]
	[TestCase("key #00ff00", true, 0, 255, 0, 255)]
	[TestCase("key #0000ff", true, 0, 0, 255, 255)]
	[TestCase("key ff00007f", true, 255, 0, 0, 127)]
	[TestCase("key 00ff007f", true, 0, 255, 0, 127)]
	[TestCase("key 0000ff7f", true, 0, 0, 255, 127)]
	[TestCase("key #ff00007f", true, 255, 0, 0, 127)]
	[TestCase("key #00ff007f", true, 0, 255, 0, 127)]
	[TestCase("key #0000ff7f", true, 0, 0, 255, 127)]
	[TestCase("key ", false, 0, 0, 0, 0)]
	[TestCase("key #", false, 0, 0, 0, 0)]
	[TestCase("key         ", false, 0, 0, 0, 0)]
	[TestCase("key #        ", false, 0, 0, 0, 0)]
	[TestCase("key zzzzzz", false, 0, 0, 0, 0)]
	[TestCase("key #zzzzzz", false, 0, 0, 0, 0)]
	[TestCase("key zzzzzzzz", false, 0, 0, 0, 0)]
	[TestCase("key #zzzzzzzz", false, 0, 0, 0, 0)]
	[TestCase("key\n{\n}", true, 0, 0, 0, 0)]
	[TestCase("key\n{\nr 1\ng 2\nb 3\na 4\n}", true, 1, 2, 3, 4)]
	public void DictionaryTryParseColor32RGBA(string input, bool expectedSuccess, byte expected_r, byte expected_g, byte expected_b, byte expected_a)
	{
		Color32 expectedValue = new Color32(expected_r, expected_g, expected_b, expected_a);
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseColor32RGBA("key", out Color32 actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("key ff0000", 255, 0, 0, 255, 127, 127, 127, 0)]
	[TestCase("key 00ff00", 0, 255, 0, 255, 127, 127, 127, 0)]
	[TestCase("key 0000ff", 0, 0, 255, 255, 127, 127, 127, 0)]
	[TestCase("key #ff0000", 255, 0, 0, 255, 127, 127, 127, 0)]
	[TestCase("key #00ff00", 0, 255, 0, 255, 127, 127, 127, 0)]
	[TestCase("key #0000ff", 0, 0, 255, 255, 127, 127, 127, 0)]
	[TestCase("key ff00007f", 255, 0, 0, 127, 127, 127, 127, 0)]
	[TestCase("key 00ff007f", 0, 255, 0, 127, 127, 127, 127, 0)]
	[TestCase("key 0000ff7f", 0, 0, 255, 127, 127, 127, 127, 0)]
	[TestCase("key #ff00007f", 255, 0, 0, 127, 127, 127, 127, 0)]
	[TestCase("key #00ff007f", 0, 255, 0, 127, 127, 127, 127, 0)]
	[TestCase("key #0000ff7f", 0, 0, 255, 127, 127, 127, 127, 0)]
	[TestCase("key ", 127, 127, 127, 127, 127, 127, 127, 127)]
	[TestCase("key #", 127, 127, 127, 127, 127, 127, 127, 127)]
	[TestCase("key       ", 127, 127, 127, 127, 127, 127, 127, 127)]
	[TestCase("key #      ", 127, 127, 127, 127, 127, 127, 127, 127)]
	[TestCase("key zzzzzz", 127, 127, 127, 127, 127, 127, 127, 127)]
	[TestCase("key #zzzzzz", 127, 127, 127, 127, 127, 127, 127, 127)]
	[TestCase("key\n{\n}", 0, 0, 0, 0, 127, 128, 129, 130)]
	[TestCase("key\n{\nr 1\ng 2\nb 3\na 4\n}", 1, 2, 3, 4, 127, 128, 129, 130)]
	public void DictionaryParseColor32RGBA(string input, byte expected_r, byte expected_g, byte expected_b, byte expected_a, byte default_r, byte default_g, byte default_b, byte default_a)
	{
		Color32 expectedValue = new Color32(expected_r, expected_g, expected_b, expected_a);
		Color32 defaultValue = new Color32(default_r, default_g, default_b, default_a);
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.AreEqual(expectedValue, dictionary.ParseColor32RGBA("key", defaultValue));
	}

	[TestCase("key_r 1\nkey_g 0.5\nkey_b 0.25", 1.0f, 0.5f, 0.25f, 0f, 0f, 0f)]
	[TestCase("key_r a\nkey_g b\nkey_b c", 1.0f, 0.5f, 0.25f, 1.0f, 0.5f, 0.25f)]
	[TestCase("key #ffffff", 1.0f, 1.0f, 1.0f, 0f, 0f, 0f)]
	public void LegacyParseColor(string input, float expected_r, float expected_g, float expected_b, float default_r, float default_g, float default_b)
	{
		Color expectedValue = new Color(expected_r, expected_g, expected_b);
		Color defaultValue = new Color(default_r, default_g, default_b);
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.AreEqual(expectedValue, dictionary.LegacyParseColor("key", defaultValue));
	}

	[TestCase("key_r 1\nkey_g 2\nkey_b 3", 1, 2, 3, 0, 0, 0)]
	[TestCase("key_r a\nkey_g b\nkey_b c", 1, 2, 3, 1, 2, 3)]
	[TestCase("key #ff7f3f", 255, 127, 63, 0, 0, 0)]
	public void LegacyParseColor32RGB(string input, byte expected_r, byte expected_g, byte expected_b, byte default_r, byte default_g, byte default_b)
	{
		Color32 expectedValue = new Color32(expected_r, expected_g, expected_b, byte.MaxValue);
		Color32 defaultValue = new Color32(default_r, default_g, default_b, byte.MaxValue);
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.AreEqual(expectedValue, dictionary.LegacyParseColor32RGB("key", defaultValue));
	}
}
