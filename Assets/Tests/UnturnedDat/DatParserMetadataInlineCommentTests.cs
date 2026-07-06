////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;

/// <summary>
/// Tests comments after a quoted value are parsed as-expected.
///
/// Examples of inline comments:
///
/// Key "Value" // A comment
/// Key1
/// {
///		Key2 "Value2" // A comment
/// }
/// Key3
/// [
///		"Value3" // A comment
/// ]
///
/// Examples of NOT inline comments:
///
/// The value here is "Value // Not a comment":
/// Key Value // Not a comment
///
/// Comments are not supported on dictionary/list starts/ends:
/// Key
/// { // Not a comment
/// </summary>
internal class DatParserMetadataInlineCommentTests
{
	[TestCase("key1 value1\nkey2 \"value2\"\nkey3 value3", "key1", null)]
	[TestCase("key1 value1\nkey2 \"value2\"\nkey3 value3", "key2", null)]
	[TestCase("key1 value1\nkey2 \"value2\"\nkey3 value3", "key3", null)]
	[TestCase("key1 value1\nkey2 \"value2\" // A comment\nkey3 value3", "key1", null)]
	[TestCase("key1 value1\nkey2 \"value2\" // A comment\nkey3 value3", "key2", "A comment")]
	[TestCase("key1 value1\nkey2 \"value2\" // A comment\nkey3 value3", "key3", null)]
	public void RootValueInlineComments(string input, string key, string expectedComment)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError");
		dictionary.TryGetValue(key, out IDatValue value);
		value.TryGetParsedInlineComment(out string actualComment);
		Assert.AreEqual(expectedComment, actualComment);
	}

	[TestCase("dict\n{\nkey1 value1\nkey2 \"value2\"\nkey3 value3\n}", "key1", null)]
	[TestCase("dict\n{\nkey1 value1\nkey2 \"value2\"\nkey3 value3\n}", "key2", null)]
	[TestCase("dict\n{\nkey1 value1\nkey2 \"value2\"\nkey3 value3\n}", "key3", null)]
	[TestCase("dict\n{\nkey1 value1\nkey2 \"value2\" // A comment\nkey3 value3\n}", "key1", null)]
	[TestCase("dict\n{\nkey1 value1\nkey2 \"value2\" // A comment\nkey3 value3\n}", "key2", "A comment")]
	[TestCase("dict\n{\nkey1 value1\nkey2 \"value2\" // A comment\nkey3 value3\n}", "key3", null)]
	public void DictionaryValueInlineComments(string input, string key, string expectedComment)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError");
		dictionary.GetDictionary("dict").TryGetValue(key, out IDatValue value);
		value.TryGetParsedInlineComment(out string actualComment);
		Assert.AreEqual(expectedComment, actualComment);
	}

	[TestCase("list\n[\nvalue1\n\"value2\"\nvalue3\n]", 0, null)]
	[TestCase("list\n[\nvalue1\n\"value2\"\nvalue3\n]", 1, null)]
	[TestCase("list\n[\nvalue1\n\"value2\"\nvalue3\n]", 2, null)]
	[TestCase("list\n[\nvalue1\n\"value2\" // A comment\nvalue3\n]", 0, null)]
	[TestCase("list\n[\nvalue1\n\"value2\" // A comment\nvalue3\n]", 1, "A comment")]
	[TestCase("list\n[\nvalue1\n\"value2\" // A comment\nvalue3\n]", 2, null)]
	public void ListValueInlineComments(string input, int index, string expectedComment)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError");
		dictionary.GetList("list").TryGetValue(index, out IDatValue value);
		value.TryGetParsedInlineComment(out string actualComment);
		Assert.AreEqual(expectedComment, actualComment);
	}
}
