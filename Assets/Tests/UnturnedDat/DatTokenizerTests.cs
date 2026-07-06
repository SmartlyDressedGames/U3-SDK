////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

internal static class DatTokenizerTests
{
	[Test]
	public static void TokenizeFlag()
	{
		DatToken[] expectedTokens = new DatToken[]
		{
			new DatToken(EDatTokenType.Key, "Flag"),
		};
		TestTokenizer("Flag", expectedTokens, true);
	}

	/// <summary>
	/// Nelson 2025-04-16: debugging an issue where an extra line is inserted between flags.
	/// </summary>
	[Test]
	public static void TokenizeFlagSpacing()
	{
		DatToken[] expectedTokens = new DatToken[]
		{
			new DatToken(EDatTokenType.Key, "Flag1"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Key, "Flag2"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Key, "Key"),
			new DatToken(EDatTokenType.Value, "Value"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Key, "Flag3"),
		};
		TestTokenizer("Flag1\n\nFlag2\nKey Value\n\nFlag3", expectedTokens, true);
	}

	[Test]
	public static void TokenizeKeyValue()
	{
		DatToken[] expectedTokens = new DatToken[]
		{
			new DatToken(EDatTokenType.Key, "Key"),
			new DatToken(EDatTokenType.Value, "Value"),
		};
		TestTokenizer("Key Value", expectedTokens, true);
	}

	[Test]
	public static void TokenizeTwoKeyValueLines()
	{
		DatToken[] expectedTokens = new DatToken[]
		{
			new DatToken(EDatTokenType.Key, "Key1"),
			new DatToken(EDatTokenType.Value, "Value1"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Key, "Key2"),
			new DatToken(EDatTokenType.Value, "Value2"),
		};
		TestTokenizer("Key1 Value1\nKey2 Value2", expectedTokens, true);
	}

	[Test]
	public static void TokenizeCommentedKeyValue()
	{
		DatToken[] expectedTokens = new DatToken[]
		{
			new DatToken(EDatTokenType.Comment, "Comment A"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Comment, "Comment B"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Key, "Key"),
			new DatToken(EDatTokenType.Value, "Value"),
			new DatToken(EDatTokenType.Comment, "Comment C"),
		};
		TestTokenizer("// Comment A\n//Comment B\nKey \"Value\" // Comment C", expectedTokens, true);
	}

	[Test]
	public static void TokenizeDictionary()
	{
		DatToken[] expectedTokens = new DatToken[]
		{
			new DatToken(EDatTokenType.Key, "Dictionary"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.OpenDictionary),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Key, "Key"),
			new DatToken(EDatTokenType.Value, "Value"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.CloseDictionary),
		};
		TestTokenizer("Dictionary\n{\nKey Value\n}", expectedTokens, true);
	}

	[Test]
	public static void TokenizeList()
	{
		DatToken[] expectedTokens = new DatToken[]
		{
			new DatToken(EDatTokenType.Key, "List"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.OpenList),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Comment, "Comment A"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Value, "Value A"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Value, "Value B"),
			new DatToken(EDatTokenType.Comment, "Comment B"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.CloseList),
		};
		TestTokenizer("List\n[\n// Comment A\nValue A\n\"Value B\" // Comment B\n]", expectedTokens, true);
	}

	/// <summary>
	/// Test that enabling/disabling comment tokenization doesn't affect non-comment tokens.
	/// </summary>
	[Test]
	public static void CompareWithAndWithoutComments()
	{
		string input = @"// A comment
// Another comment
Key1 ""Value1"" // Inline comment

// Comment 4
Key2 Value2
List
[
	// Comment 5
	List Item 1

	// Comment 6
	""List Item 2"" // Another inline comment
]

// Comment 7
// Comment 8
Dictionary
{
	// Comment 9
	Key3 Value3
	Key4 Value4
}

Key5 Mis-configured comment example // Not a comment
Flag // With mis-configured comment";

		DatToken[] expectedTokens = new DatToken[]
		{
			new DatToken(EDatTokenType.Comment, "A comment"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Comment, "Another comment"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Key, "Key1"),
			new DatToken(EDatTokenType.Value, "Value1"),
			new DatToken(EDatTokenType.Comment, "Inline comment"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Comment, "Comment 4"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Key, "Key2"),
			new DatToken(EDatTokenType.Value, "Value2"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Key, "List"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.OpenList),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Comment, "Comment 5"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Value, "List Item 1"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Comment, "Comment 6"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Value, "List Item 2"),
			new DatToken(EDatTokenType.Comment, "Another inline comment"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.CloseList),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Comment, "Comment 7"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Comment, "Comment 8"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Key, "Dictionary"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.OpenDictionary),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Comment, "Comment 9"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Key, "Key3"),
			new DatToken(EDatTokenType.Value, "Value3"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Key, "Key4"),
			new DatToken(EDatTokenType.Value, "Value4"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.CloseDictionary),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Key, "Key5"),
			new DatToken(EDatTokenType.Value, "Mis-configured comment example // Not a comment"),
			new DatToken(EDatTokenType.LineBreak),
			new DatToken(EDatTokenType.Key, "Flag"),
			new DatToken(EDatTokenType.Value, "// With mis-configured comment"),
		};

		TestTokenizer(input, expectedTokens, true);

		System.Func<DatToken, bool> isNotComment = (DatToken token) =>
		{
			return token.type != EDatTokenType.Comment;
		};
		TestTokenizer(input, expectedTokens.Where(isNotComment), false);
	}

	private static void TestTokenizer(string input, IEnumerable<DatToken> expectedTokens, bool enableComments)
	{
		DatTokenizer tokenizer = new DatTokenizer();
		tokenizer.EnableComments = enableComments;
		tokenizer.Tokenize(input);
		Assert.IsFalse(tokenizer.HasError);

		//UnityEngine.Debug.Log(string.Join(',', expectedTokens));

		int tokenIndex = 0;
		foreach (DatToken expectedToken in expectedTokens)
		{
			Assert.IsTrue(tokenizer.ReadToken(out DatToken actualToken));
			Assert.AreEqual(expectedToken.type, actualToken.type, $"tokens[{tokenIndex}] type");
			Assert.AreEqual(expectedToken.value, actualToken.value, $"tokens[{tokenIndex}] value");
			++tokenIndex;
		}

		Assert.IsFalse(tokenizer.ReadToken(out DatToken _), "no more tokens");
	}
}
