////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;
using System.Collections.Generic;

internal class DatStructTests
{
	struct TestStruct : IDatParseable
	{
		public int x;
		public int y;

		public bool TryParse(IDatNode node)
		{
			if (node is IDatValue value)
			{
				if (string.IsNullOrEmpty(value.Value))
				{
					return false;
				}

				string[] components = value.Value.Split(',');
				if (components.Length != 2)
				{
					return false;
				}

				return int.TryParse(components[0], out x) && int.TryParse(components[1], out y);
			}
			else if (node is IDatDictionary dict)
			{
				x = dict.ParseInt32("x");
				y = dict.ParseInt32("y");
				return true;
			}

			return false;
		}
	}

	[TestCase("key 1,2", true, 1, 2)]
	[TestCase("key 1", false, 0, 0)]
	[TestCase("key abc", false, 0, 0)]
	[TestCase("key\n{\nx 1\ny 2\n}", true, 1, 2)]
	[TestCase("key\n{\nx 1\n}", true, 1, 0)]
	[TestCase("key\n{\ny 2\n}", true, 0, 2)]
	[TestCase("key\n{\n}", true, 0, 0)]
	public void TryParseStruct(string input, bool expectedSuccess, int expected_x, int expected_y)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedSuccess, dictionary.TryParseStruct("key", out TestStruct actualValue), "TryParseStruct");
		Assert.AreEqual(expected_x, actualValue.x);
		Assert.AreEqual(expected_y, actualValue.y);
	}

	// Default value is only used if IDatParseable.TryParse returns false.
	[TestCase("key 1,2", 1, 2, 0, 0)]
	[TestCase("key abc", 3, 4, 3, 4)]
	[TestCase("key\n{\nx 1\ny 2\n}", 1, 2, 0, 0)]
	[TestCase("key\n{\nx 1\n}", 1, 0, 3, 4)]
	[TestCase("key\n{\ny 2\n}", 0, 2, 3, 4)]
	[TestCase("key\n{\n}", 0, 0, 3, 4)]
	[TestCase("key", 3, 4, 3, 4)]
	public void ParseStruct(string input, int expected_x, int expected_y, int default_x, int default_y)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		TestStruct actualValue = dictionary.ParseStruct("key", new TestStruct { x = default_x, y = default_y });
		Assert.AreEqual(expected_x, actualValue.x);
		Assert.AreEqual(expected_y, actualValue.y);
	}

	[TestCase("list\n[\n]", new int[] { }, new int[] { })]
	[TestCase("list\n[\n1,2\n]", new int[] { 1 }, new int[] { 2 })]
	[TestCase("list\n[\n1,2\n{\nx 3\ny 4\n}\n]", new int[] { 1, 3 }, new int[] { 2, 4 })]
	[TestCase("list\n[\n1,2\ninvalid value\n{\nx 3\ny 4\n}\n]", new int[] { 1, 3 }, new int[] { 2, 4 })]
	public void ParseListOfStructs(string input, int[] expected_x, int[] expected_y)
	{
		Assert.AreEqual(expected_x.Length, expected_y.Length);
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.IsTrue(rootDictionary.TryGetList("list", out IDatList list));
		List<TestStruct> structs = list.ParseListOfStructs<TestStruct>();
		Assert.AreEqual(expected_x.Length, structs.Count);
		for (int index = 0; index < structs.Count; ++index)
		{
			TestStruct actualValue = structs[index];
			Assert.AreEqual(expected_x[index], actualValue.x);
			Assert.AreEqual(expected_y[index], actualValue.y);
		}
	}

	[TestCase("array\n[\n]", new int[] { }, new int[] { })]
	[TestCase("array\n[\n1,2\n]", new int[] { 1 }, new int[] { 2 })]
	[TestCase("array\n[\n1,2\n{\nx 3\ny 4\n}\n]", new int[] { 1, 3 }, new int[] { 2, 4 })]
	[TestCase("array\n[\n1,2\ninvalid value\n{\nx 3\ny 4\n}\n]", new int[] { 1, 0, 3 }, new int[] { 2, 0, 4 })]
	public void ParseArrayOfStructs(string input, int[] expected_x, int[] expected_y)
	{
		Assert.AreEqual(expected_x.Length, expected_y.Length);
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.IsTrue(rootDictionary.TryGetList("array", out IDatList list), "TryGetList");
		TestStruct[] structs = list.ParseArrayOfStructs<TestStruct>();
		Assert.AreEqual(expected_x.Length, structs.Length, "structs.Length");
		for (int index = 0; index < structs.Length; ++index)
		{
			TestStruct actualValue = structs[index];
			Assert.AreEqual(expected_x[index], actualValue.x);
			Assert.AreEqual(expected_y[index], actualValue.y);
		}
	}

	[TestCase("list\n[\n]", new int[] { }, new int[] { })]
	[TestCase("list\n[\n1,2\n]", new int[] { 1 }, new int[] { 2 })]
	[TestCase("list\n[\n1,2\n{\nx 3\ny 4\n}\n]", new int[] { 1, 3 }, new int[] { 2, 4 })]
	[TestCase("list\n[\n1,2\ninvalid value\n{\nx 3\ny 4\n}\n]", new int[] { 1, 3 }, new int[] { 2, 4 })]
	public void DictionaryParseListOfStructs(string input, int[] expected_x, int[] expected_y)
	{
		Assert.AreEqual(expected_x.Length, expected_y.Length);
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		List<TestStruct> structs = rootDictionary.ParseListOfStructs<TestStruct>("list");
		Assert.AreEqual(expected_x.Length, structs.Count);
		for (int index = 0; index < structs.Count; ++index)
		{
			TestStruct actualValue = structs[index];
			Assert.AreEqual(expected_x[index], actualValue.x);
			Assert.AreEqual(expected_y[index], actualValue.y);
		}
	}

	[TestCase("array\n[\n]", new int[] { }, new int[] { })]
	[TestCase("array\n[\n1,2\n]", new int[] { 1 }, new int[] { 2 })]
	[TestCase("array\n[\n1,2\n{\nx 3\ny 4\n}\n]", new int[] { 1, 3 }, new int[] { 2, 4 })]
	[TestCase("array\n[\n1,2\ninvalid value\n{\nx 3\ny 4\n}\n]", new int[] { 1, 0, 3 }, new int[] { 2, 0, 4 })]
	public void DictionaryParseArrayOfStructs(string input, int[] expected_x, int[] expected_y)
	{
		Assert.AreEqual(expected_x.Length, expected_y.Length);
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		TestStruct[] structs = rootDictionary.ParseArrayOfStructs<TestStruct>("array");
		Assert.AreEqual(expected_x.Length, structs.Length, "structs.Length");
		for (int index = 0; index < structs.Length; ++index)
		{
			TestStruct actualValue = structs[index];
			Assert.AreEqual(expected_x[index], actualValue.x);
			Assert.AreEqual(expected_y[index], actualValue.y);
		}
	}
}
