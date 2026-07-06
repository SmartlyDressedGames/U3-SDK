////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;
using UnityEngine;

internal class UnityDatExTests
{
	[TestCase("0,0", true, 0.0f, 0.0f)]
	[TestCase("1, 2", true, 1.0f, 2.0f)]
	[TestCase("(1, 2)", true, 1.0f, 2.0f)]
	[TestCase("0,a", false, 0.0f, 0.0f)]
	[TestCase("a,0", false, 0.0f, 0.0f)]
	[TestCase("0,", false, 0.0f, 0.0f)]
	[TestCase(",0", false, 0.0f, 0.0f)]
	[TestCase(",", false, 0.0f, 0.0f)]
	[TestCase("0,", false, 0.0f, 0.0f)]
	[TestCase("(0,0", false, 0.0f, 0.0f)]
	public void ValueTryParseVector2(string input, bool expectedSuccess, float expected_x, float expected_y)
	{
		Vector2 expectedValue = new Vector2(expected_x, expected_y);
		IDatValue value = new DatValue(input);
		Assert.AreEqual(expectedSuccess, value.TryParseVector2(out Vector2 actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("0,0", 0.0f, 0.0f, 1.0f, 2.0f)]
	[TestCase("1, 2", 1.0f, 2.0f, 0.0f, 0.0f)]
	[TestCase("(1, 2)", 1.0f, 2.0f, 0.0f, 0.0f)]
	[TestCase("a,", 1.0f, 2.0f, 1.0f, 2.0f)]
	[TestCase(",a", 1.0f, 2.0f, 1.0f, 2.0f)]
	[TestCase("0,", 1.0f, 2.0f, 1.0f, 2.0f)]
	[TestCase(",0", 1.0f, 2.0f, 1.0f, 2.0f)]
	[TestCase(",", 1.0f, 2.0f, 1.0f, 2.0f)]
	[TestCase("(0,0", 1.0f, 2.0f, 1.0f, 2.0f)]
	public void ValueParseVector2(string input, float expected_x, float expected_y, float default_x, float default_y)
	{
		Vector2 expectedValue = new Vector2(expected_x, expected_y);
		Vector2 defaultValue = new Vector2(default_x, default_y);
		IDatValue value = new DatValue(input);
		Assert.AreEqual(expectedValue, value.ParseVector2(defaultValue));
	}

	[TestCase("key 0,0", true, 0.0f, 0.0f)]
	[TestCase("key 1, 2", true, 1.0f, 2.0f)]
	[TestCase("key (1, 2)", true, 1.0f, 2.0f)]
	[TestCase("key 0,a", false, 0.0f, 0.0f)]
	[TestCase("key ,a", false, 0.0f, 0.0f)]
	[TestCase("key ,", false, 0.0f, 0.0f)]
	[TestCase("key (0,0", false, 0.0f, 0.0f)]
	[TestCase("key\n{\n}", true, 0.0f, 0.0f)]
	[TestCase("key\n{\nx 1\ny 2\n}", true, 1.0f, 2.0f)]
	public void DictionaryTryParseVector2(string input, bool expectedSuccess, float expected_x, float expected_y)
	{
		Vector2 expectedValue = new Vector2(expected_x, expected_y);
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseVector2("key", out Vector2 actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("key 0,0", 0.0f, 0.0f, 1.0f, 2.0f)]
	[TestCase("key 1, 2", 1.0f, 2.0f, 0.0f, 0.0f)]
	[TestCase("key (1, 2)", 1.0f, 2.0f, 0.0f, 0.0f)]
	[TestCase("key 0,", 1.0f, 2.0f, 1.0f, 2.0f)]
	[TestCase("key ,", 1.0f, 2.0f, 1.0f, 2.0f)]
	[TestCase("key (0,0", 1.0f, 2.0f, 1.0f, 2.0f)]
	[TestCase("key\n{\n}", 0.0f, 0.0f, 1.0f, 2.0f)]
	[TestCase("key\n{\nx 1\ny 2\n}", 1.0f, 2.0f, 0.0f, 0.0f)]
	public void DictionaryParseVector2(string input, float expected_x, float expected_y, float default_x, float default_y)
	{
		Vector2 expectedValue = new Vector2(expected_x, expected_y);
		Vector2 defaultValue = new Vector2(default_x, default_y);
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.AreEqual(expectedValue, dictionary.ParseVector2("key", defaultValue));
	}

	[TestCase("0,0,0", true, 0.0f, 0.0f, 0.0f)]
	[TestCase("1, 2, 3", true, 1.0f, 2.0f, 3.0f)]
	[TestCase("(1, 2, 3)", true, 1.0f, 2.0f, 3.0f)]
	[TestCase("0,0,a", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("0,a,0", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("a,0,0", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("0,0,", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("0,,0", false, 0.0f, 0.0f, 0.0f)]
	[TestCase(",0,0", false, 0.0f, 0.0f, 0.0f)]
	[TestCase(",,0", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("0,,", false, 0.0f, 0.0f, 0.0f)]
	[TestCase(",0,", false, 0.0f, 0.0f, 0.0f)]
	[TestCase(",,", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("0,", false, 0.0f, 0.0f, 0.0f)]
	[TestCase(",", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("(0,0,0", false, 0.0f, 0.0f, 0.0f)]
	public void ValueTryParseVector3(string input, bool expectedSuccess, float expected_x, float expected_y, float expected_z)
	{
		Vector3 expectedValue = new Vector3(expected_x, expected_y, expected_z);
		IDatValue value = new DatValue(input);
		Assert.AreEqual(expectedSuccess, value.TryParseVector3(out Vector3 actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("0,0,0", 0.0f, 0.0f, 0.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("1, 2, 3", 1.0f, 2.0f, 3.0f, 0.0f, 0.0f, 0.0f)]
	[TestCase("(1, 2, 3)", 1.0f, 2.0f, 3.0f, 0.0f, 0.0f, 0.0f)]
	[TestCase("0,0,", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("0,,0", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase(",0,0", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase(",,0", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("0,,", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase(",0,", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase(",,", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("0,", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase(",", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("(0,0,0", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	public void ValueParseVector3(string input, float expected_x, float expected_y, float expected_z, float default_x, float default_y, float default_z)
	{
		Vector3 expectedValue = new Vector3(expected_x, expected_y, expected_z);
		Vector3 defaultValue = new Vector3(default_x, default_y, default_z);
		IDatValue value = new DatValue(input);
		Assert.AreEqual(expectedValue, value.ParseVector3(defaultValue));
	}

	[TestCase("key 0,0,0", true, 0.0f, 0.0f, 0.0f)]
	[TestCase("key 1, 2, 3", true, 1.0f, 2.0f, 3.0f)]
	[TestCase("key (1, 2, 3)", true, 1.0f, 2.0f, 3.0f)]
	[TestCase("key 0,0,a", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("key 0,a,0", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("key a,0,0", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("key 0,0,", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("key 0,,0", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("key ,0,0", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("key ,,0", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("key 0,,", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("key ,0,", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("key ,,", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("key 0,", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("key ,", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("key (0,0,0,", false, 0.0f, 0.0f, 0.0f)]
	[TestCase("key\n{\n}", true, 0.0f, 0.0f, 0.0f)]
	[TestCase("key\n{\nx 1\ny 2\nz 3\n}", true, 1.0f, 2.0f, 3.0f)]
	public void DictionaryTryParseVector3(string input, bool expectedSuccess, float expected_x, float expected_y, float expected_z)
	{
		Vector3 expectedValue = new Vector3(expected_x, expected_y, expected_z);
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.AreEqual(expectedSuccess, dictionary.TryParseVector3("key", out Vector3 actualValue));
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestCase("key 0,0,0", 0.0f, 0.0f, 0.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("key 1, 2, 3", 1.0f, 2.0f, 3.0f, 0.0f, 0.0f, 0.0f)]
	[TestCase("key (1, 2, 3)", 1.0f, 2.0f, 3.0f, 0.0f, 0.0f, 0.0f)]
	[TestCase("key 0,0,", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("key 0,,0", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("key ,0,0", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("key ,,0", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("key 0,,", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("key ,0,", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("key ,,", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("key 0,", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("key ,", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("key (0,0,0", 1.0f, 2.0f, 3.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("key\n{\n}", 0.0f, 0.0f, 0.0f, 1.0f, 2.0f, 3.0f)]
	[TestCase("key\n{\nx 1\ny 2\nz 3\n}", 1.0f, 2.0f, 3.0f, 0.0f, 0.0f, 0.0f)]
	public void DictionaryParseVector3(string input, float expected_x, float expected_y, float expected_z, float default_x, float default_y, float default_z)
	{
		Vector3 expectedValue = new Vector3(expected_x, expected_y, expected_z);
		Vector3 defaultValue = new Vector3(default_x, default_y, default_z);
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.AreEqual(expectedValue, dictionary.ParseVector3("key", defaultValue));
	}

	[TestCase("key_x 1\nkey_y 2\nkey_z 3", 1.0f, 2.0f, 3.0f)]
	[TestCase("key_x a\nkey_y b\nkey_z c", 0.0f, 0.0f, 0.0f)]
	[TestCase("key (1,2,3)", 1.0f, 2.0f, 3.0f)]
	public void LegacyParseVector3(string input, float expected_x, float expected_y, float expected_z)
	{
		Vector3 expectedValue = new Vector3(expected_x, expected_y, expected_z);
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.AreEqual(expectedValue, dictionary.LegacyParseVector3("key"));
	}
}
