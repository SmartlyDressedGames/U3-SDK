////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;

internal class DatParserMetadataLineNumberTests
{
	[TestCase("key1 value1\nkey2 value2", "key1", 1)]
	[TestCase("key1 value1\nkey2 value2", "key2", 2)]
	[TestCase("key1 value1\nkey2\n[\n1\n]\nkey3 value3", "key3", 6)]
	[TestCase("flag", "flag", 1)]
	[TestCase("flag1\nflag2", "flag1", 1)]
	[TestCase("flag1\nflag2", "flag2", 2)]
	[TestCase("flag1\n\nflag2\n", "flag2", 3)]
	[TestCase("flag1\n\n\nflag2\n", "flag2", 4)]
	public void RootValueLineNumbers(string input, string key, int expectedLineNumber)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError");
		dictionary.TryGetValue(key, out IDatValue value);
		value.TryGetParsedLineNumber(out int actualLineNumber);
		Assert.AreEqual(expectedLineNumber, actualLineNumber);
	}

	[TestCase("list\n[\n]", "list", 2, 3)]
	[TestCase("list\n[\n1\n2\n]", "list", 2, 5)]
	[TestCase("key1 value1\n\nlist\n[\n1\n2\n]", "list", 4, 7)]
	public void RootListLineNumbers(string input, string key, int expectedStartingLineNumber, int expectedEndingLineNumber)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError");
		dictionary.TryGetList(key, out IDatList list);
		list.TryGetParsedLineNumberRange(out int actualStartingLineNumber, out int actualEndingLineNumber);
		Assert.AreEqual(expectedStartingLineNumber, actualStartingLineNumber);
		Assert.AreEqual(expectedEndingLineNumber, actualEndingLineNumber);
	}

	[TestCase("dict\n{\n}", "dict", 2, 3)]
	[TestCase("dict\n{\nkey1 value1\n}", "dict", 2, 4)]
	public void RootDictionaryLineNumbers(string input, string key, int expectedStartingLineNumber, int expectedEndingLineNumber)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError");
		dictionary.TryGetDictionary(key, out IDatDictionary dict);
		dict.TryGetParsedLineNumberRange(out int actualStartingLineNumber, out int actualEndingLineNumber);
		Assert.AreEqual(expectedStartingLineNumber, actualStartingLineNumber);
		Assert.AreEqual(expectedEndingLineNumber, actualEndingLineNumber);
	}

	[TestCase("dict\n{\nkey1 value1\nkey2 value2\n}", "key1", 3)]
	[TestCase("dict\n{\nkey1 value1\nkey2 value2\n}", "key2", 4)]
	public void DictionaryValueLineNumbers(string input, string key, int expectedLineNumber)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError");
		dictionary.GetDictionary("dict").TryGetValue(key, out IDatValue value);
		value.TryGetParsedLineNumber(out int actualLineNumber);
		Assert.AreEqual(expectedLineNumber, actualLineNumber);
	}

	[TestCase("list\n[\nvalue1\nvalue2\n]", 0, 3)]
	[TestCase("list\n[\nvalue1\nvalue2\n]", 1, 4)]
	public void ListValueLineNumbers(string input, int index, int expectedLineNumber)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError");
		dictionary.GetList("list").TryGetValue(index, out IDatValue value);
		value.TryGetParsedLineNumber(out int actualLineNumber);
		Assert.AreEqual(expectedLineNumber, actualLineNumber);
	}
}
