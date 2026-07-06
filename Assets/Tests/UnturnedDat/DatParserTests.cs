////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;

internal class DatParserTests
{
	[TestCase("// comment")]
	[TestCase("//comment without space")]
	[TestCase("// multiple\n// comments")]
	[TestCase(" //")]
	[TestCase("//")]
	[TestCase("// ")]
	public void IgnoreRootComments(string input)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(0, dictionary.Count);
	}

	[TestCase("// comment\nkey value")]
	[TestCase("//comment without space\nkey value")]
	[TestCase("// multiple\n// comments\nkey value")]
	[TestCase("//\nkey value")] // empty comment
	public void IgnoreRootCommentsBeforeKey(string input)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(1, dictionary.Count);
		Assert.AreEqual("value", dictionary.GetString("key"));
	}

	[TestCase("key\n{\n// comment\n}")]
	[TestCase("key\n{\n//comment without space\n}")]
	[TestCase("key\n{\n// multiple\n// comments\n}")]
	[TestCase("key\n{\n//\n}")] // empty comment
	public void IgnoreDictionaryComments(string input)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(1, rootDictionary.Count);
		IDatDictionary childDictionary = rootDictionary.GetDictionary("key");
		Assert.AreEqual(0, childDictionary.Count);
	}

	[TestCase("key\n{\n// comment\nkey value\n}")]
	[TestCase("key\n{\n//comment without space\nkey value\n}")]
	[TestCase("key\n{\n// multiple\n// comments\nkey value\n}")]
	[TestCase("key\n{\n//\nkey value\n}")] // empty comment
	public void IgnoreDictionaryCommentsBeforeKey(string input)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(1, rootDictionary.Count);
		IDatDictionary childDictionary = rootDictionary.GetDictionary("key");
		Assert.AreEqual(1, childDictionary.Count);
		Assert.AreEqual("value", childDictionary.GetString("key"));
	}

	[TestCase("key\n{\nkey value\n// comment\n}")]
	[TestCase("key\n{\nkey value\n//comment without space\n}")]
	[TestCase("key\n{\nkey value\n// multiple\n// comments\n}")]
	[TestCase("key\n{\nkey value\n//\n}")] // empty comment
	public void IgnoreDictionaryCommentsAfterKey(string input)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(1, rootDictionary.Count);
		IDatDictionary childDictionary = rootDictionary.GetDictionary("key");
		Assert.AreEqual(1, childDictionary.Count);
		Assert.AreEqual("value", childDictionary.GetString("key"));
	}

	/// <summary>
	/// Same-line comments are only supported for quoted values. (required for .dat backwards compatibility)
	/// </summary>
	[TestCase("key value //comment", "key", "value //comment")]
	[TestCase("key value // comment", "key", "value // comment")]
	[TestCase("key \"value\"//comment", "key", "value")]
	[TestCase("key \"value\"// comment", "key", "value")]
	[TestCase("key \"value\" //comment", "key", "value")]
	[TestCase("key \"value\" // comment", "key", "value")]
	public void IgnoreCommentsOnSameLineAsDictionaryValue(string input, string expectedKey, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue, dictionary.GetString(expectedKey));
	}

	[TestCase("key\n[\n// comment\n]")]
	[TestCase("key\n[\n//comment without space\n]")]
	[TestCase("key\n[\n// multiple\n// comments\n]")]
	[TestCase("key\n[\n//\n]")] // empty comment
	public void IgnoreListComments(string input)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(1, rootDictionary.Count);
		IDatList list = rootDictionary.GetList("key");
		Assert.AreEqual(0, list.Count);
	}

	[TestCase("key\n[\n// comment\nvalue\n]")]
	[TestCase("key\n[\n//comment without space\nvalue\n]")]
	[TestCase("key\n[\n// multiple\n// comments\nvalue\n]")]
	[TestCase("key\n[\n//\nvalue\n]")] // empty comment
	public void IgnoreListCommentsBeforeValue(string input)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(1, rootDictionary.Count);
		IDatList list = rootDictionary.GetList("key");
		Assert.AreEqual(1, list.Count);
		Assert.AreEqual("value", list.GetString(0));
	}

	[TestCase("key\n[\nvalue\n// comment\n]")]
	[TestCase("key\n[\nvalue\n//comment without space\n]")]
	[TestCase("key\n[\nvalue\n// multiple\n// comments\n]")]
	[TestCase("key\n[\nvalue\n//\n]")] // empty comment
	public void IgnoreListCommentsAfterValue(string input)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(1, rootDictionary.Count);
		IDatList list = rootDictionary.GetList("key");
		Assert.AreEqual(1, list.Count);
		Assert.AreEqual("value", list.GetString(0));
	}

	/// <summary>
	/// Same-line comments are only supported for quoted values.
	/// </summary>
	[TestCase("key\n[\nvalue // comment\n]", "value // comment")]
	[TestCase("key\n[\nvalue //comment\n]", "value //comment")]
	[TestCase("key\n[\n\"value\" // comment\n]", "value")]
	[TestCase("key\n[\n\"value\" //comment\n]", "value")]
	[TestCase("key\n[\n\"value\"// comment\n]", "value")]
	[TestCase("key\n[\n\"value\"//comment\n]", "value")]
	public void IgnoreCommentsOnSameLineAsListValue(string input, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(1, rootDictionary.Count);
		IDatList list = rootDictionary.GetList("key");
		Assert.AreEqual(1, list.Count);
		Assert.AreEqual(expectedValue, list.GetString(0));
	}

	[TestCase("")]
	[TestCase(" ")]
	[TestCase("  ")]
	[TestCase("\n")]
	[TestCase("\n\n")]
	[TestCase(" \n ")]
	[TestCase(" \n \n")]
	[TestCase(" \n \n ")]
	[TestCase("  \n  \n")]
	[TestCase("  \n  \n ")]
	[TestCase("  \n  \n  ")]
	public void ParseEmptyRootDictionary(string input)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(0, dictionary.Count);
	}

	[TestCase("key backslash: \\\\", "key", @"backslash: \")]
	[TestCase(@"key ""backslash: \\""", "key", @"backslash: \")]
	public void ParseValueBackslash(string input, string expectedKey, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue, dictionary.GetString(expectedKey));
	}

	[TestCase("key \"\nvalue\"", "key", "\nvalue")]
	[TestCase("key \"value\n\"", "key", "value\n")]
	[TestCase("key \"\nvalue\n\"", "key", "\nvalue\n")]
	[TestCase("key \"line1\nline2\"", "key", "line1\nline2")]
	[TestCase("key \"line1\n\nline2\"", "key", "line1\n\nline2")]
	public void ParseMultiLineString(string input, string expectedKey, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue, dictionary.GetString(expectedKey));
	}

	/// <summary>
	/// Some old .dat options check ContainsKey rather than true/false.
	/// </summary>
	[TestCase("flag", "flag")]
	[TestCase(" flag", "flag")]
	[TestCase("flag ", "flag")]
	[TestCase(" flag ", "flag")]
	[TestCase("flag\n", "flag")]
	[TestCase(" flag\n", "flag")]
	[TestCase(" flag \n", "flag")]
	[TestCase(" flag \n ", "flag")]
	public void ParseFlag(string input, string expectedKey)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.IsTrue(dictionary.ContainsKey(expectedKey));
	}

	[TestCase("flag1\nflag2", "flag1", "flag2")]
	[TestCase("flag1 \nflag2", "flag1", "flag2")]
	[TestCase("flag1\n flag2", "flag1", "flag2")]
	[TestCase("flag1 \n flag2", "flag1", "flag2")]
	[TestCase(" flag1 \n flag2 ", "flag1", "flag2")]
	public void ParseMultipleFlags(string input, string expectedKey1, string expectedKey2)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.IsTrue(dictionary.ContainsKey(expectedKey1), $"{nameof(dictionary.ContainsKey)}({expectedKey1})");
		Assert.IsTrue(dictionary.ContainsKey(expectedKey2), $"{nameof(dictionary.ContainsKey)}({expectedKey2})");
	}

	[TestCase("key value ", "key", "value ")]
	[TestCase("key value \n", "key", "value ")]
	[TestCase("key value \n ", "key", "value ")]
	[TestCase("key value  ", "key", "value  ")]
	[TestCase("key value  \n", "key", "value  ")]
	[TestCase("key value  \n ", "key", "value  ")]
	[TestCase("key \"value\" ", "key", "value")]
	[TestCase("key \"value\" \n", "key", "value")]
	[TestCase("key \"value\" \n ", "key", "value")]
	[TestCase("key \"value\"  \n ", "key", "value")]
	public void ParseValueWithTrailingSpaces(string input, string expectedKey, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue, dictionary.GetString(expectedKey));
	}

	[TestCase("key value", "key", "value")]
	[TestCase(" key value", "key", "value")]
	[TestCase("key  value", "key", "value")]
	[TestCase(" key  value", "key", "value")]
	[TestCase("key value with space", "key", "value with space")]
	[TestCase(" key value with space", "key", "value with space")]
	[TestCase("key  value with space", "key", "value with space")]
	[TestCase(" key  value with space", "key", "value with space")]
	public void ParseSingleLineWithoutQuotes(string input, string expectedKey, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue, dictionary.GetString(expectedKey));
	}

	[TestCase("key1 value1\nkey2 value2", "key1", "value1", "key2", "value2")]
	[TestCase("key1  value1\nkey2  value2", "key1", "value1", "key2", "value2")]
	[TestCase(" key1 value1\n key2 value2", "key1", "value1", "key2", "value2")]
	[TestCase("key1 value with spaces 1\nkey2 value with spaces 2", "key1", "value with spaces 1", "key2", "value with spaces 2")]
	[TestCase(" key1 value with spaces 1\n key2 value with spaces 2", "key1", "value with spaces 1", "key2", "value with spaces 2")]
	[TestCase(" key1  value with spaces 1\n key2  value with spaces 2", "key1", "value with spaces 1", "key2", "value with spaces 2")]
	public void ParseMultiLineWithoutQuotes(string input, string expectedKey1, string expectedValue1, string expectedKey2, string expectedValue2)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue1, dictionary.GetString(expectedKey1));
		Assert.AreEqual(expectedValue2, dictionary.GetString(expectedKey2));
	}

	[TestCase("\"key\" value", "key", "value")]
	[TestCase(" \"key\" value", "key", "value")]
	[TestCase("\"key\"  value", "key", "value")]
	[TestCase(" \"key\"  value", "key", "value")]
	[TestCase("\"key with space\" value", "key with space", "value")]
	[TestCase(" \"key with space\" value", "key with space", "value")]
	[TestCase("\"key with space\"  value", "key with space", "value")]
	[TestCase(" \"key with space\"  value", "key with space", "value")]
	public void ParseQuotedKey(string input, string expectedKey, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue, dictionary.GetString(expectedKey));
	}

	[TestCase("key \"value\"", "key", "value")]
	[TestCase(" key \"value\"", "key", "value")]
	[TestCase("key  \"value\"", "key", "value")]
	[TestCase(" key  \"value\"", "key", "value")]
	[TestCase(" key  \"value\" ", "key", "value")]
	[TestCase("key \"value with space\"", "key", "value with space")]
	[TestCase(" key \"value with space\"", "key", "value with space")]
	[TestCase("key  \"value with space\"", "key", "value with space")]
	[TestCase(" key  \"value with space\"", "key", "value with space")]
	[TestCase(" key  \"value with space\" ", "key", "value with space")]
	public void ParseQuotedValue(string input, string expectedKey, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue, dictionary.GetString(expectedKey));
	}

	[TestCase("key \"quotation mark: \\\"\"", "key", "quotation mark: \"")]
	[TestCase("key \"quoted word: \\\"word\\\"\"", "key", "quoted word: \"word\"")]
	public void ParseQuotedValueWithEscapedQuotes(string input, string expectedKey, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue, dictionary.GetString(expectedKey));
	}

	/// <summary>
	/// Nelson 2025-04-14: this catches a quirk that even if a key is assigned a value, a list or dictionary on the
	/// next line takes precedence. Caught because you can't comment flags, for example this parses comment as flag value:
	/// Flag // comment
	/// </summary>
	[TestCase("Key Value\n[\n]", "Key")]
	[TestCase("Key Value\n{\n}", "Key")]
	[TestCase("Key // comment\n[\n]", "Key")]
	[TestCase("Key // comment\n{\n}", "Key")]
	[TestCase("\"Key\" Value\n[\n]", "Key")]
	[TestCase("\"Key\" Value\n{\n}", "Key")]
	[TestCase("\"Key\" // comment\n[\n]", "Key")]
	[TestCase("\"Key\" // comment\n{\n}", "Key")]
	public void ParseListOrDictionaryIgnoringValue(string input, string key)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		IDatNode node = rootDictionary[key];
		Assert.AreNotEqual(EDatNodeType.Value, node.NodeType);
	}

	[TestCase("key\n{\n}", "key")]
	[TestCase("key\n{\n\n}", "key")]
	[TestCase("key\n{\n\n\n}", "key")]
	public void ParseEmptyChildDictionary(string input, string expectedKey)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		IDatDictionary childDictionary;
		Assert.IsTrue(rootDictionary.TryGetDictionary(expectedKey, out childDictionary), "TryGetDictionary result");
		Assert.NotNull(childDictionary, nameof(childDictionary));
		Assert.AreEqual(0, childDictionary.Count);
	}

	[TestCase("key\n[\n]", "key")]
	[TestCase("key\n[\n\n]", "key")]
	[TestCase("key\n[\n\n\n]", "key")]
	public void ParseEmptyList(string input, string expectedKey)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		IDatList list;
		Assert.IsTrue(rootDictionary.TryGetList(expectedKey, out list), "TryGetList result");
		Assert.NotNull(list, nameof(list));
		Assert.AreEqual(0, list.Count);
	}

	[TestCase("key\n[\nvalue1\n]", "key", new string[] { "value1" })]
	[TestCase("key\n[\nvalue1\nvalue2\n]", "key", new string[] { "value1", "value2" })]
	[TestCase("key\n[\nvalue1\nvalue2\nvalue3\n]", "key", new string[] { "value1", "value2", "value3" })]
	[TestCase("key\n[\nvalue with spaces\n]", "key", new string[] { "value with spaces" })]
	[TestCase("key\n[\nvalue1\nvalue with spaces\nvalue3\n]", "key", new string[] { "value1", "value with spaces", "value3" })]
	[TestCase("key\n[\n\"value with quotes\"\n]", "key", new string[] { "value with quotes" })]
	[TestCase("key\n[\nvalue1\n\"value with quotes\"\nvalue3\n]", "key", new string[] { "value1", "value with quotes", "value3" })]
	public void ParseValuesInList(string input, string expectedKey, string[] expectedValues)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		IDatList list;
		Assert.IsTrue(rootDictionary.TryGetList(expectedKey, out list), "TryGetList result");
		Assert.NotNull(list, nameof(list));
		Assert.AreEqual(expectedValues.Length, list.Count);
		for (int index = 0; index < expectedValues.Length; ++index)
		{
			Assert.AreEqual(expectedValues[index], list.GetString(index));
		}
	}

	[TestCase("key\n[\n\nvalue\n]", "key", new string[] { "value" })]
	[TestCase("key\n[\nvalue\n\n]", "key", new string[] { "value" })]
	[TestCase("key\n[\n\nvalue\n\n]", "key", new string[] { "value" })]
	[TestCase("key\n[\n\nvalue1\nvalue2\n\n]", "key", new string[] { "value1", "value2" })]
	[TestCase("key\n[\n\nvalue1\n\nvalue2\n\n]", "key", new string[] { "value1", "value2" })]
	public void ParseListValuesWithEmptyLines(string input, string expectedKey, string[] expectedValues)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		IDatList list;
		Assert.IsTrue(rootDictionary.TryGetList(expectedKey, out list), "TryGetList result");
		Assert.NotNull(list, nameof(list));
		Assert.AreEqual(expectedValues.Length, list.Count);
		for (int index = 0; index < expectedValues.Length; ++index)
		{
			Assert.AreEqual(expectedValues[index], list.GetString(index));
		}
	}

	[TestCase("key\n[\n[\n]\n]", "key")]
	public void ParseEmptyChildList(string input, string expectedRootKey)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		IDatList list;
		Assert.IsTrue(rootDictionary.TryGetList(expectedRootKey, out list), "TryGetList result");
		Assert.NotNull(list, nameof(list));
		IDatList childList = list[0] as IDatList;
		Assert.NotNull(childList, nameof(childList));
	}

	[TestCase("key\n[\n[\n]\n[\n]\n]", "key")]
	public void ParseMultipleChildLists(string input, string expectedRootKey)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		IDatList list;
		Assert.IsTrue(rootDictionary.TryGetList(expectedRootKey, out list), "TryGetList result");
		Assert.NotNull(list, nameof(list));
		Assert.AreEqual(2, list.Count);
		Assert.NotNull(list[0] as IDatList);
		Assert.NotNull(list[1] as IDatList);
	}

	/// <summary>
	/// At the time of writing Unturned doesn't use lists with a mix of values, dictionaries, and sub-lists,
	/// but mis-configured files should be handled properly, and it could be used in a list of colors like:
	/// [
	///		#ff0000
	///		{
	///			r 0
	///			g 255
	///			b 0
	///		}
	///		#0000bb
	/// ]
	/// </summary>
	[TestCase("list\n[\nvalue\n{\n}\n]", new System.Type[] { typeof(IDatValue), typeof(IDatDictionary) })]
	[TestCase("list\n[\nvalue\n[\n]\n]", new System.Type[] { typeof(IDatValue), typeof(IDatList) })]
	[TestCase("list\n[\n{\n}\n[\n]\n]", new System.Type[] { typeof(IDatDictionary), typeof(IDatList) })]
	[TestCase("list\n[\n[\n]\n{\n}\n]", new System.Type[] { typeof(IDatList), typeof(IDatDictionary) })]
	[TestCase("list\n[\nvalue1\n{\n}\nvalue2\n]", new System.Type[] { typeof(IDatValue), typeof(IDatDictionary), typeof(IDatValue) })]
	[TestCase("list\n[\nvalue1\n[\n]\nvalue2\n]", new System.Type[] { typeof(IDatValue), typeof(IDatList), typeof(IDatValue) })]
	[TestCase("list\n[\nvalue\n{\n}\n[\n]\n]", new System.Type[] { typeof(IDatValue), typeof(IDatDictionary), typeof(IDatList) })]
	[TestCase("list\n[\nvalue1\n{\n}\n[\n]\nvalue2\n]", new System.Type[] { typeof(IDatValue), typeof(IDatDictionary), typeof(IDatList), typeof(IDatValue) })]
	[TestCase("list\n[\nvalue1\n[\n]\n{\n}\nvalue2\n]", new System.Type[] { typeof(IDatValue), typeof(IDatList), typeof(IDatDictionary), typeof(IDatValue) })]
	public void ParseListWithMixedTypes(string input, System.Type[] expectedTypes)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		IDatList list;
		Assert.IsTrue(rootDictionary.TryGetList("list", out list), "TryGetList result");
		Assert.NotNull(list, nameof(list));
		Assert.AreEqual(expectedTypes.Length, list.Count);
		for (int index = 0; index < expectedTypes.Length; ++index)
		{
			System.Type expectedType = expectedTypes[index];
			System.Type actualType = list[index].GetType();
			// IsAssignableFrom means: "Could an instance of actualType be assigned to a variable of type expectedType?"
			Assert.IsTrue(expectedType.IsAssignableFrom(actualType), $"Expected: {expectedType} Actual: {actualType}");
		}
	}

	[TestCase("key\n{\n\nkey1 value1\n}", "key", new string[] { "key1" }, new string[] { "value1" })]
	[TestCase("key\n{\nkey1 value1\n\n}", "key", new string[] { "key1" }, new string[] { "value1" })]
	[TestCase("key\n{\n\nkey1 value1\n\n}", "key", new string[] { "key1" }, new string[] { "value1" })]
	[TestCase("key\n{\n\nkey1 value1\nkey2 value2\n\n}", "key", new string[] { "key1", "key2" }, new string[] { "value1", "value2" })]
	[TestCase("key\n{\n\nkey1 value1\n\nkey2 value2\n\n}", "key", new string[] { "key1", "key2" }, new string[] { "value1", "value2" })]
	[TestCase("key\n{\n\nkey1 value1\n\n\nkey2 value2\n\n}", "key", new string[] { "key1", "key2" }, new string[] { "value1", "value2" })]
	public void ParseChildDictionaryValuesWithEmptyLines(string input, string expectedRootKey, string[] expectedChildKeys, string[] expectedChildValues)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		IDatDictionary childDictionary;
		Assert.IsTrue(rootDictionary.TryGetDictionary(expectedRootKey, out childDictionary), "TryGetDictionary result");
		Assert.NotNull(childDictionary, nameof(childDictionary));
		Assert.AreEqual(expectedChildKeys.Length, expectedChildValues.Length);
		Assert.AreEqual(expectedChildKeys.Length, childDictionary.Count);
		for (int index = 0; index < expectedChildKeys.Length; ++index)
		{
			Assert.AreEqual(expectedChildValues[index], childDictionary.GetString(expectedChildKeys[index]));
		}
	}

	[TestCase("key1\n{\nkey2 value\n}", "key1", "key2", "value")]
	[TestCase("key1\n{\nkey2 value with spaces\n}", "key1", "key2", "value with spaces")]
	[TestCase("key1\n{\nkey2 \"value with quotes\"\n}", "key1", "key2", "value with quotes")]
	[TestCase("key1\n{\nkey2 value2\nkey3 value3\nkey4 value4\n}", "key1", "key3", "value3")]
	public void ParseValueInChildDictionary(string input, string expectedRootKey, string expectedChildKey, string expectedChildValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		IDatDictionary childDictionary;
		Assert.IsTrue(rootDictionary.TryGetDictionary(expectedRootKey, out childDictionary), "TryGetDictionary result");
		Assert.NotNull(childDictionary, nameof(childDictionary));
		Assert.AreEqual(expectedChildValue, childDictionary.GetString(expectedChildKey));
	}

	[TestCase("key1\n{key2\n{\n}\n}", "key1", "key2")]
	public void ParseEmptyGrandchildDictionary(string input, string expectedRootKey, string expectedChildKey)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		IDatDictionary childDictionary;
		Assert.IsTrue(rootDictionary.TryGetDictionary(expectedRootKey, out childDictionary), "rootDictionary.TryGetDictionary result");
		Assert.NotNull(childDictionary, nameof(childDictionary));
		IDatDictionary grandchildDictionary;
		Assert.IsTrue(childDictionary.TryGetDictionary(expectedChildKey, out grandchildDictionary), "childDictionary.TryGetDictionary result");
		Assert.NotNull(grandchildDictionary, nameof(grandchildDictionary));
	}

	[TestCase("key1\n{key2\n{\nkey3 value\n}\n}", "key1", "key2", "key3", "value")]
	[TestCase("key1\n{key2\n{\nkey3 value with spaces\n}\n}", "key1", "key2", "key3", "value with spaces")]
	[TestCase("key1\n{key2\n{\nkey3 \"value with quotes\"\n}\n}", "key1", "key2", "key3", "value with quotes")]
	[TestCase("key1\n{key2\n{\nkey3 value3\nkey4 value4\nkey5 value5\n}\n}", "key1", "key2", "key4", "value4")]
	public void ParseValueInGrandchildDictionary(string input, string expectedRootKey, string expectedChildKey, string expectedGrandchildKey, string expectedGrandchildValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		IDatDictionary childDictionary;
		Assert.IsTrue(rootDictionary.TryGetDictionary(expectedRootKey, out childDictionary), "rootDictionary.TryGetDictionary result");
		Assert.NotNull(childDictionary, nameof(childDictionary));
		IDatDictionary grandchildDictionary;
		Assert.IsTrue(childDictionary.TryGetDictionary(expectedChildKey, out grandchildDictionary), "childDictionary.TryGetDictionary result");
		Assert.NotNull(grandchildDictionary, nameof(grandchildDictionary));
		Assert.AreEqual(expectedGrandchildValue, grandchildDictionary.GetString(expectedGrandchildKey));
	}

	[TestCase("key first\\nsecond", "first\nsecond")]
	[TestCase("key \"first\\nsecond\"", "first\nsecond")]
	public void ParseEscapedNewLine(string input, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue, dictionary.GetString("key"));
	}

	/// <summary>
	/// Parse keys/values from a typical old vanilla .dat file.
	/// </summary>
	// First case has no comments:
	[TestCase(@"
		Type Item
		ID 342
		GUID 4d362f61e4324509844feedd1a69ae19

		Flag1

		EnableSomething true
	")]
	// Second case has lots of comments:
	[TestCase(@"
		// comment
		Type Item
		ID 342
		// comment
		// comment
		GUID 4d362f61e4324509844feedd1a69ae19
		// comment

		// comment
		Flag1

		EnableSomething true
		// comment
	")]
	public void ParseSafeV1Asset(string contents)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(contents);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual("Item", dictionary.GetString("Type"));
		Assert.AreEqual(342, dictionary.ParseUInt16("ID"));
		Assert.AreEqual(new System.Guid("4d362f61e4324509844feedd1a69ae19"), dictionary.ParseGuid("GUID"));
		Assert.IsTrue(dictionary.ContainsKey("Flag1"));
		Assert.IsTrue(dictionary.ParseBool("EnableSomething"));
	}

	/// <summary>
	/// Parse contents of a typical vanilla .asset file. (half-baked format sigh)
	/// </summary>
	// First case has no comments:
	[TestCase(@"
		""Metadata""
		{
			""GUID"" ""4d362f61e4324509844feedd1a69ae19""
			""Type"" ""SDG.Unturned.ItemAsset""
		}
		""Asset""
		{
			""Name"" ""Bear+Dog""
			""Struct""
			{
				""X"" ""1""
				""Y"" ""2""
			}
			""Array""
			[
				{
					""A"" ""3""
					""B"" ""4""
				}
				{
					""A"" ""5""
					""B"" ""6""
				}
			]
		}
	")]
	// Second case has lots of comments:
	// Nelson 2025-04-15: Previously, this test case had comments on the lines between the key for a dictionary/list
	// and opening the dictionary/list. That was fine when comments were skipped, but when supporting metadata this
	// becomes messy differentiating between a "flag" and a mis-configured dat file. There's also the messiness of
	// comments on list/dict key being parsed as a value. :(
	[TestCase(@"
		// comment
		// comment
		""Metadata"" // comment
		{ // comment
			""GUID"" ""4d362f61e4324509844feedd1a69ae19"" // comment
			""Type"" ""SDG.Unturned.ItemAsset"" // comment
		} // comment
		// comment
		""Asset"" // comment
		{ // comment
			""Name"" ""Bear+Dog"" // comment
			""Struct"" // comment
			{ // comment
				""X"" ""1"" // comment
				""Y"" ""2"" // comment
			} // comment
			""Array"" // comment
			[ // comment
				// comment
				
				// comment
				{ // comment
					""A"" ""3"" // comment
					""B"" ""4"" // comment
				} // comment
				// comment
				{ // comment
					""A"" ""5"" // comment
					""B"" ""6"" // comment
				} // comment
			] // comment
		} // comment
		// comment
	")]
	// Third case has unnecessary commas because many modded "v2" assets have them.
	[TestCase(@"
		""Metadata"",
		{,
			""GUID"" ""4d362f61e4324509844feedd1a69ae19"",
			""Type"" ""SDG.Unturned.ItemAsset"",
		},
		""Asset"",
		{,
			""Name"" ""Bear+Dog"",
			""Struct"",
			{,
				""X"" ""1"",
				""Y"" ""2"",
			},
			""Array"",
			[,
				{,
					""A"" ""3"",
					""B"" ""4"",
				},
				{,
					""A"" ""5"",
					""B"" ""6"",
				},
			],
		},
	")]
	public void ParseSafeV2Asset(string contents)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(contents);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");

		IDatDictionary metadata = rootDictionary.GetDictionary("Metadata");
		Assert.NotNull(metadata, nameof(metadata));
		Assert.AreEqual(new System.Guid("4d362f61e4324509844feedd1a69ae19"), metadata.ParseGuid("GUID"));
		Assert.AreEqual("SDG.Unturned.ItemAsset", metadata.GetString("Type"));

		IDatDictionary asset = rootDictionary.GetDictionary("Asset");
		Assert.NotNull(asset, nameof(asset));
		Assert.AreEqual("Bear+Dog", asset.GetString("Name"));

		IDatDictionary substruct = asset.GetDictionary("Struct");
		Assert.NotNull(substruct, nameof(substruct));
		Assert.AreEqual(1, substruct.ParseInt32("X"));
		Assert.AreEqual(2, substruct.ParseInt32("Y"));

		IDatList list = asset.GetList("Array");
		Assert.NotNull(list, nameof(list));
		IDatDictionary listDict0 = list.GetDictionary(0);
		Assert.NotNull(listDict0, nameof(listDict0));
		IDatDictionary listDict1 = list.GetDictionary(1);
		Assert.NotNull(listDict1, nameof(listDict1));
		Assert.AreEqual(3, listDict0.ParseInt32("A"));
		Assert.AreEqual(4, listDict0.ParseInt32("B"));
		Assert.AreEqual(5, listDict1.ParseInt32("A"));
		Assert.AreEqual(6, listDict1.ParseInt32("B"));
	}

	/// <summary>
	/// For .dat file backwards compatibility '{' should be treated as a value. 
	/// </summary>
	[TestCase("key {", "key", "{")]
	[TestCase("key {\n", "key", "{")]
	[TestCase("key  {", "key", "{")]
	[TestCase("key  {\n", "key", "{")]
	[TestCase("key { \n", "key", "{ ")]
	[TestCase("key {}", "key", "{}")]
	[TestCase("key {}\n", "key", "{}")]
	[TestCase("key { }", "key", "{ }")]
	[TestCase("key { }\n", "key", "{ }")]
	public void ParseDictionaryOnSameLineAsValue(string input, string expectedKey, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue, dictionary.GetString(expectedKey));
	}

	/// <summary>
	/// For .dat file backwards compatibility '[' should be treated as a value. 
	/// </summary>
	[TestCase("key [", "key", "[")]
	[TestCase("key [\n", "key", "[")]
	[TestCase("key  [", "key", "[")]
	[TestCase("key  [\n", "key", "[")]
	[TestCase("key [ \n", "key", "[ ")]
	[TestCase("key []", "key", "[]")]
	[TestCase("key []\n", "key", "[]")]
	[TestCase("key [ ]", "key", "[ ]")]
	[TestCase("key [ ]\n", "key", "[ ]")]
	public void ParseListOnSameLineAsValue(string input, string expectedKey, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue, dictionary.GetString(expectedKey));
	}

	/// <summary>
	/// Many "v2" assets in curated maps seem to have unnecessary commas, so they need to be excluded from list entries.
	/// </summary>
	[TestCase("list\n[\n\"value\",\n]", new string[] { "value" })]
	[TestCase("list\n[\n\"value1\",\n\"value2\"\n]", new string[] { "value1", "value2" })]
	[TestCase("list\n[\n\"value1\",\n\"value2\",\n]", new string[] { "value1", "value2" })]
	public void IgnoreCommasAfterValuesInList(string input, string[] expectedValues)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		IDatList list;
		Assert.IsTrue(rootDictionary.TryGetList("list", out list), "TryGetList result");
		Assert.NotNull(list, nameof(list));
		Assert.AreEqual(expectedValues.Length, list.Count);
		for (int index = 0; index < expectedValues.Length; ++index)
		{
			Assert.AreEqual(expectedValues[index], list.GetString(index));
		}
	}

	/// <summary>
	/// Many "v2" assets in curated maps seem to have unnecessary commas, so they need to be excluded from list entries.
	/// </summary>
	[TestCase("list\n[\n{\nkey value\n},\n]", new string[] { "value" })]
	[TestCase("list\n[\n{\nkey value1\n},\n{\nkey value2\n}\n]", new string[] { "value1", "value2" })]
	[TestCase("list\n[\n{\nkey value1\n},\n{\nkey value2\n},\n]", new string[] { "value1", "value2" })]
	public void IgnoreCommasAfterDictionariesInList(string input, string[] expectedValues)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		IDatList list;
		Assert.IsTrue(rootDictionary.TryGetList("list", out list), "TryGetList result");
		Assert.NotNull(list, nameof(list));
		Assert.AreEqual(expectedValues.Length, list.Count);
		for (int index = 0; index < expectedValues.Length; ++index)
		{
			Assert.AreEqual(expectedValues[index], list.GetDictionary(index).GetString("key"));
		}
	}

	/// <summary>
	/// Many "v2" assets in curated maps seem to have unnecessary commas, so they need to be excluded dictionary keys.
	/// </summary>
	[TestCase("\"key1\" \"value1\",", new string[] { "key1" }, new string[] { "value1" })]
	[TestCase("\"key1\" \"value1\",\n", new string[] { "key1" }, new string[] { "value1" })]
	[TestCase("\"key1\" \"value1\",\n\"key2\" \"value2\"", new string[] { "key1", "key2" }, new string[] { "value1", "value2" })]
	[TestCase("\"key1\" \"value1\",\n\"key2\" \"value2\",", new string[] { "key1", "key2" }, new string[] { "value1", "value2" })]
	[TestCase("\"key1\" \"value1\",\n\"key2\" \"value2\",\n", new string[] { "key1", "key2" }, new string[] { "value1", "value2" })]
	public void IgnoreCommasAfterValuesInDictionary(string input, string[] expectedKeys, string[] expectedValues)
	{
		DatParser parser = new DatParser();
		IDatDictionary rootDictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedKeys.Length, expectedValues.Length);
		Assert.AreEqual(expectedKeys.Length, rootDictionary.Count);
		for (int index = 0; index < expectedKeys.Length; ++index)
		{
			Assert.AreEqual(expectedValues[index], rootDictionary.GetString(expectedKeys[index]));
		}
	}

	/// <summary>
	/// 2023-05-27: the 3.23.7.0 update added '\n' handling for unquoted strings which broke
	/// some mods that were using '\' in file paths. This test is for the supported escape
	/// sequences, and there is a separate error test for '\' in file paths. (public issue #3886)
	/// </summary>
	[TestCase("key \\n", "\n")] // \n
	[TestCase("key \\t", "\t")] // \t
	[TestCase("key \\\\", "\\")] // \
	[TestCase("key \"\\n\"", "\n")] // \n
	[TestCase("key \"\\t\"", "\t")] // \t
	[TestCase("key \"\\\\\"", "\\")] // \
	[TestCase("key \"\\\"\"", "\"")] // "
	public void ParseValidEscapeSequences(string input, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue, dictionary.GetString("key"));
	}

	/// <summary>
	/// 2023-07-12: main menu banner was broken because string included byte order mark.
	/// (Unity's DownloadHandler was included the BOM bytes in the string itself.)
	/// </summary>
	[Test]
	public void SkipBOM()
	{
		System.Text.UTF8Encoding encodingWithBom = new System.Text.UTF8Encoding(/*emit BOM*/ true);
		byte[] data;
		using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(new byte[32]))
		using (System.IO.StreamWriter writer = new System.IO.StreamWriter(memoryStream, encodingWithBom))
		{
			writer.WriteLine("Key Value");
			writer.Flush();
			data = memoryStream.ToArray();
		}
		Assert.AreEqual(0xEF, data[0], "first byte");
		Assert.AreEqual(0xBB, data[1], "second byte");
		Assert.AreEqual(0xBF, data[2], "third byte");

		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(data);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual("Value", dictionary.GetString("Key"));

		// Hack to easily put BOM characters in string.
		char[] charCopy = new char[data.Length];
		for (int index = 0; index < data.Length; ++index)
		{
			charCopy[index] = (char) data[index];
		}
		string stringWithBom = new string(charCopy);
		Assert.AreEqual(0xEF, stringWithBom[0], "first char");
		Assert.AreEqual(0xBB, stringWithBom[1], "second char");
		Assert.AreEqual(0xBF, stringWithBom[2], "third char");
		IDatDictionary stringDictionary = parser.Parse(stringWithBom);
		Assert.IsFalse(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual("Value", stringDictionary.GetString("Key"));
	}
}
