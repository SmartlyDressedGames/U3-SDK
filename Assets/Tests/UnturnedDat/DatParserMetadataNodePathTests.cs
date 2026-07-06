////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;

/// <summary>
/// Node paths aim to clarify error messages for nested lists and dictionaries.
/// For example, an item blueprint like:
/// Blueprints
/// [
///		{
///			InputItems
///			[
///				{
///					...
///				}
///				{
///					ErrorHere
///				}
///			]
///		}
/// ]
/// Could return a path like: /Blueprints/0/InputItems/1
/// </summary>
internal class DatParserMetadataNodePathTests
{
	[TestCase("Key1 Value1\nKey2 Value2\nKey3 Value3", "Key1")]
	[TestCase("Key1 Value1\nKey2 Value2\nKey3 Value3", "Key2")]
	[TestCase("Key1 Value1\nKey2 Value2\nKey3 Value3", "Key3")]
	public static void RootDictionaryPath(string input, string key)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError", $"parser.HasError, Error message: {parser.ErrorMessage}");
		dictionary.TryGetValue(key, out IDatValue node);
		string expectedPath = $"/{key}";
		Assert.IsTrue(node.TryGetNodePath(out string actualPath));
		Assert.AreEqual(expectedPath, actualPath);
	}

	[TestCase("List\n[\nValue1\nValue2\nValue3\n]", 0, "/List/0")]
	[TestCase("List\n[\nValue1\nValue2\nValue3\n]", 1, "/List/1")]
	[TestCase("List\n[\nValue1\nValue2\nValue3\n]", 2, "/List/2")]
	public static void ListPath(string input, int index, string expectedPath)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError", $"parser.HasError, Error message: {parser.ErrorMessage}");
		dictionary.TryGetList("List", out IDatList list);
		list.TryGetValue(index, out IDatValue node);
		Assert.IsTrue(node.TryGetNodePath(out string actualPath), "able to get node path");
		Assert.AreEqual(expectedPath, actualPath);
	}

	[TestCase("Dict\n{\nKey1 Value1\nKey2 Value2\nKey3 Value3\n}", "Key1", "/Dict/Key1")]
	[TestCase("Dict\n{\nKey1 Value1\nKey2 Value2\nKey3 Value3\n}", "Key2", "/Dict/Key2")]
	[TestCase("Dict\n{\nKey1 Value1\nKey2 Value2\nKey3 Value3\n}", "Key3", "/Dict/Key3")]
	public static void DictionaryPath(string input, string key, string expectedPath)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError", $"parser.HasError, Error message: {parser.ErrorMessage}");
		dictionary.TryGetDictionary("Dict", out IDatDictionary childDictionary);
		childDictionary.TryGetValue(key, out IDatValue node);
		Assert.IsTrue(node.TryGetNodePath(out string actualPath), "able to get node path");
		Assert.AreEqual(expectedPath, actualPath);
	}
}
