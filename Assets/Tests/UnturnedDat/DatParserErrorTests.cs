////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;

internal class DatParserErrorTests
{
	[TestCase("key1\n{")]
	[TestCase("key1\n{\n")]
	[TestCase("key1\n{\nkey2 value2")]
	[TestCase("key1\n{\nkey2 value2\n")]
	public void DictionaryWithoutClosingBrace(string input)
	{
		DatParser parser = new DatParser();
		parser.Parse(input);
		Assert.IsTrue(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
	}

	[TestCase("key\n[")]
	[TestCase("key\n[\n")]
	[TestCase("key\n[\nvalue")]
	[TestCase("key\n[\nvalue\nvalue\n")]
	public void ListWithoutClosingBracket(string input)
	{
		DatParser parser = new DatParser();
		parser.Parse(input);
		Assert.IsTrue(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
	}

	[TestCase("\"")]
	[TestCase(" \"")]
	[TestCase("\n\"")]
	[TestCase("\"\n")]
	[TestCase("\n\"\n")]
	[TestCase("key\n\"")]
	[TestCase("key\n\" ")]
	[TestCase("key\n\"\nvalue")]
	[TestCase("key\n\"\nvalue\nvalue\n")]
	public void StringWithoutClosingQuote(string input)
	{
		DatParser parser = new DatParser();
		parser.Parse(input);
		Assert.IsTrue(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
	}

	/// <summary>
	/// Error message should specify line number regardless of \n, \r, or \r\n.
	/// </summary>
	[TestCase("\"", 1)]
	[TestCase("\"\n", 1)] // 1 line after
	[TestCase("\"\r", 1)] // 1 line after
	[TestCase("\"\r\n", 1)] // 1 line after
	[TestCase("\n\"", 2)] // 1 line before
	[TestCase("\r\"", 2)] // 1 line before
	[TestCase("\r\n\"", 2)] // 1 line before
	[TestCase("\n\"\n", 2)] // 1 line before, 1 line after
	[TestCase("\r\"\r", 2)] // 1 line before, 1 line after
	[TestCase("\r\n\"\r\n", 2)] // 1 line before, 1 line after
	[TestCase("\n\"\n\n", 2)] // 1 line before, 2 lines after
	[TestCase("\r\"\r\r", 2)] // 1 line before, 2 lines after
	[TestCase("\r\n\"\r\n\r\n", 2)] // 1 line before, 2 lines after
	[TestCase("\n\n\"\n", 3)] // 2 lines before, 1 line after
	[TestCase("\r\r\"\r", 3)] // 2 lines before, 1 line after
	[TestCase("\r\n\r\n\"\r\n", 3)] // 2 lines before, 1 line after
	[TestCase("\n\n\"\n\n", 3)] // 2 lines before, 2 lines after
	[TestCase("\r\r\"\r\r", 3)] // 2 lines before, 2 lines after
	[TestCase("\r\n\r\n\"\r\n\r\n", 3)] // 2 lines before, 2 lines after
	public void LineNumberCounting(string input, int expectedErrorLineNumber)
	{
		DatParser parser = new DatParser();
		parser.Parse(input);
		Assert.IsTrue(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.IsTrue(parser.ErrorMessage.EndsWith($"string opened on line {expectedErrorLineNumber}"), $"EndsWith(\"string opened on line {expectedErrorLineNumber}\")");
	}

	/// <summary>
	/// Duplicate keys should log an error and use the latest value.
	/// </summary>
	[TestCase("key value1\nkey value2", "value2")]
	public void DuplicateKey(string input, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsTrue(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue, dictionary.GetString("key"));
	}

	/// <summary>
	/// 2023-05-27: the 3.23.7.0 update added '\n' handling for unquoted strings which broke
	/// some mods that were using '\' in file paths. This test ensures *most* of those work
	/// by treating unrecognized escape sequences as just '\', and reporting an error message.
	/// </summary>
	[TestCase("key path\\of\\asset", "path\\of\\asset")] // unquoted
	[TestCase("key \"path\\of\\asset\"", "path\\of\\asset")] // quoted
	public void InvalidEscapeSequence(string input, string expectedValue)
	{
		DatParser parser = new DatParser();
		IDatDictionary dictionary = parser.Parse(input);
		Assert.IsTrue(parser.HasError, $"parser.HasError, Error message: {parser.ErrorMessage}");
		Assert.AreEqual(expectedValue, dictionary.GetString("key"));
	}
}
