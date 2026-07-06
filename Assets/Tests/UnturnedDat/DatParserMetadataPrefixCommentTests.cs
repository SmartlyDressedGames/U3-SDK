////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;

internal class DatParserMetadataPrefixCommentTests
{
	[TestCase("key value", "key", null, -1, -1)]
	[TestCase("// A comment\nkey value", "key", "A comment", 1, 1)]
	[TestCase("// part 1 of 2\n// part 2 of 2\nkey value", "key", "part 1 of 2\npart 2 of 2", 1, 2)]
	[TestCase("key1 value1\n\nkey2 value2", "key1", null, -1, -1)]
	[TestCase("key1 value1\n\nkey2 value2", "key2", null, -1, -1)]
	[TestCase("key1 value1\n\n// A comment\nkey2 value2", "key1", null, -1, -1)]
	[TestCase("key1 value1\n\n// A comment\nkey2 value2", "key2", "A comment", 3, 3)]
	public void RootValueComments(string input, string key, string expectedComment, int expectedStartingLineNumber,
		int expectedEndingLineNumber)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError", $"parser.HasError, Error message: {parser.ErrorMessage}");
		dictionary.TryGetValue(key, out IDatValue node);
		bool hasComment = node.TryGetParsedComment(out DatComment comment);
		Assert.AreEqual(!string.IsNullOrEmpty(expectedComment), hasComment, "hasComment");
		if (hasComment)
		{
			Assert.AreEqual(expectedComment, comment.MessageWithLineBreaks);
			Assert.AreEqual(expectedStartingLineNumber, comment.StartingLineNumber);
			Assert.AreEqual(expectedEndingLineNumber, comment.EndingLineNumber);
		}
	}

	/// <summary>
	/// Test that prefix comments are associated with root-level dictionaries properly.
	/// </summary>
	[TestCase("Dict1\n{\n}\nDict2\n{\n}", "Dict1", null, -1, -1)]
	[TestCase("Dict1\n{\n}\nDict2\n{\n}", "Dict2", null, -1, -1)]
	[TestCase("// Comment A\nDict1\n{\n}\n// Comment B\nDict2\n{\n}", "Dict1", "Comment A", 1, 1)]
	[TestCase("// Comment A\nDict1\n{\n}\n// Comment B\nDict2\n{\n}", "Dict2", "Comment B", 5, 5)]
	[TestCase("// Comment A1\n// Comment A2\nDict1\n{\n}\n// Comment B1\n//Comment B2\nDict2\n{\n}", "Dict1", "Comment A1\nComment A2", 1, 2)]
	[TestCase("// Comment A1\n// Comment A2\nDict1\n{\n}\n// Comment B1\n//Comment B2\nDict2\n{\n}", "Dict2", "Comment B1\nComment B2", 6, 7)]
	public void RootDictionaryComments(string input, string key, string expectedComment, int expectedStartingLineNumber,
		int expectedEndingLineNumber)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError", $"parser.HasError, Error message: {parser.ErrorMessage}");
		dictionary.TryGetDictionary(key, out IDatDictionary node);
		bool hasComment = node.TryGetParsedComment(out DatComment comment);
		Assert.AreEqual(!string.IsNullOrEmpty(expectedComment), hasComment, "hasComment");
		if (hasComment)
		{
			Assert.AreEqual(expectedComment, comment.MessageWithLineBreaks);
			Assert.AreEqual(expectedStartingLineNumber, comment.StartingLineNumber);
			Assert.AreEqual(expectedEndingLineNumber, comment.EndingLineNumber);
		}
	}

	/// <summary>
	/// Test that prefix comments are associated with root-level lists properly.
	/// </summary>
	[TestCase("List1\n[\n]\nList2\n[\n]", "List1", null, -1, -1)]
	[TestCase("List1\n[\n]\nList2\n[\n]", "List2", null, -1, -1)]
	[TestCase("// Comment A\nList1\n[\n]\n// Comment B\nList2\n[\n]", "List1", "Comment A", 1, 1)]
	[TestCase("// Comment A\nList1\n[\n]\n// Comment B\nList2\n[\n]", "List2", "Comment B", 5, 5)]
	[TestCase("// Comment A1\n// Comment A2\nList1\n[\n]\n// Comment B1\n//Comment B2\nList2\n[\n]", "List1", "Comment A1\nComment A2", 1, 2)]
	[TestCase("// Comment A1\n// Comment A2\nList1\n[\n]\n// Comment B1\n//Comment B2\nList2\n[\n]", "List2", "Comment B1\nComment B2", 6, 7)]
	public void RootListComments(string input, string key, string expectedComment, int expectedStartingLineNumber,
		int expectedEndingLineNumber)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError", $"parser.HasError, Error message: {parser.ErrorMessage}");
		dictionary.TryGetList(key, out IDatList node);
		bool hasComment = node.TryGetParsedComment(out DatComment comment);
		Assert.AreEqual(!string.IsNullOrEmpty(expectedComment), hasComment, "hasComment");
		if (hasComment)
		{
			Assert.AreEqual(expectedComment, comment.MessageWithLineBreaks);
			Assert.AreEqual(expectedStartingLineNumber, comment.StartingLineNumber);
			Assert.AreEqual(expectedEndingLineNumber, comment.EndingLineNumber);
		}
	}

	/// <summary>
	/// Test that prefix comments are associated with items in dictionaries properly.
	/// </summary>
	[TestCase("Dict\n{\nFlag\n// Comment A\nKey1 Value1\n}", "Key1", "Comment A", 4, 4)]
	[TestCase("Dict\n{\nFlag\n// Comment A\n// Comment B\nKey1 Value1\n}", "Key1", "Comment A\nComment B", 4, 5)]
	[TestCase("Dict\n{\n// Comment A\n//Comment B\nKey1 // Inline Comment\n{\n}\n\n}", "Key1", "Comment A\nComment B", 3, 4)] // Sub-dict
	[TestCase("Dict\n{\n// Comment A\n//Comment B\nKey1 // Inline Comment\n[\n]\n\n}", "Key1", "Comment A\nComment B", 3, 4)] // List
	[TestCase("Dict\n{\n// Comment A\n//Comment B\nKey1\n{\n}\n// Comment C\n//Comment D\nKey2\n{\n}\n}", "Key1", "Comment A\nComment B", 3, 4)] // Sub-dict
	[TestCase("Dict\n{\n// Comment A\n//Comment B\nKey1\n{\n}\n// Comment C\n//Comment D\nKey2\n{\n}\n}", "Key2", "Comment C\nComment D", 8, 9)] // Sub-dict
	[TestCase("Dict\n{\n// Comment A\n//Comment B\nKey1\n[\n]\n// Comment C\n//Comment D\nKey2\n[\n]\n}", "Key1", "Comment A\nComment B", 3, 4)] // List
	[TestCase("Dict\n{\n// Comment A\n//Comment B\nKey1\n[\n]\n// Comment C\n//Comment D\nKey2\n[\n]\n}", "Key2", "Comment C\nComment D", 8, 9)] // List
	public void DictionaryComments(string input, string key, string expectedComment, int expectedStartingLineNumber,
		int expectedEndingLineNumber)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError", $"parser.HasError, Error message: {parser.ErrorMessage}");
		dictionary.TryGetDictionary("Dict", out IDatDictionary childDictionary);
		childDictionary.TryGetNode(key, out IDatNode node);
		bool hasComment = node.TryGetParsedComment(out DatComment comment);
		Assert.AreEqual(!string.IsNullOrEmpty(expectedComment), hasComment, "hasComment");
		if (hasComment)
		{
			Assert.AreEqual(expectedComment, comment.MessageWithLineBreaks);
			Assert.AreEqual(expectedStartingLineNumber, comment.StartingLineNumber, $"starting line number {comment}");
			Assert.AreEqual(expectedEndingLineNumber, comment.EndingLineNumber, $"ending line number {comment}");
		}
	}

	/// <summary>
	/// Test that prefix comments are associated with items in lists properly.
	/// </summary>
	[TestCase("List\n[\nValue 1\n// Comment A\nValue2\n]", 0, null, -1, -1)]
	[TestCase("List\n[\nValue 1\n// Comment A\nValue2\n]", 1, "Comment A", 4, 4)]
	[TestCase("List\n[\n// Comment A\nValue1\n// Comment B\nValue2\n]", 0, "Comment A", 3, 3)]
	[TestCase("List\n[\n// Comment A\nValue1\n// Comment B\nValue2\n]", 1, "Comment B", 5, 5)]
	[TestCase("List\n[\n// Comment A\n// Comment B\nValue1\n// Comment C\n// Comment D\nValue2\n]", 0, "Comment A\nComment B", 3, 4)]
	[TestCase("List\n[\n// Comment A\n// Comment B\nValue1\n// Comment C\n// Comment D\nValue2\n]", 1, "Comment C\nComment D", 6, 7)]
	[TestCase("List\n[\n// Comment A\n// Comment B\n[\n]\n// Comment C\n// Comment D\n[\n]\n]", 0, "Comment A\nComment B", 3, 4)] // Sub-List
	[TestCase("List\n[\n// Comment A\n// Comment B\n[\n]\n// Comment C\n// Comment D\n[\n]\n]", 1, "Comment C\nComment D", 7, 8)] // Sub-List
	[TestCase("List\n[\n// Comment A\n// Comment B\n{\n}\n// Comment C\n// Comment D\n{\n}\n]", 0, "Comment A\nComment B", 3, 4)] // Dictionary
	[TestCase("List\n[\n// Comment A\n// Comment B\n{\n}\n// Comment C\n// Comment D\n{\n}\n]", 1, "Comment C\nComment D", 7, 8)] // Dictionary
	public void ListComments(string input, int index, string expectedComment, int expectedStartingLineNumber,
		int expectedEndingLineNumber)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsFalse(parser.HasError, "parser.HasError", $"parser.HasError, Error message: {parser.ErrorMessage}");
		dictionary.TryGetList("List", out IDatList childList);
		IDatNode node = childList[index];
		bool hasComment = node.TryGetParsedComment(out DatComment comment);
		Assert.AreEqual(!string.IsNullOrEmpty(expectedComment), hasComment, "hasComment");
		if (hasComment)
		{
			Assert.AreEqual(expectedComment, comment.MessageWithLineBreaks);
			Assert.AreEqual(expectedStartingLineNumber, comment.StartingLineNumber, $"starting line number {comment}");
			Assert.AreEqual(expectedEndingLineNumber, comment.EndingLineNumber, $"ending line number {comment}");
		}
	}
}
