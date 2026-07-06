////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;

internal class CommandLineTests
{
	[Test]
	public void ParseEmptyAndNull()
	{
		string value;
		Assert.IsFalse(CommandLine.TryParseValue(null, null, out value));
		Assert.IsFalse(CommandLine.TryParseValue(string.Empty, null, out value));
		Assert.IsFalse(CommandLine.TryParseValue(null, string.Empty, out value));
		Assert.IsFalse(CommandLine.TryParseValue(string.Empty, string.Empty, out value));
		Assert.IsFalse(CommandLine.TryParseValue(" ", null, out value));
		Assert.IsFalse(CommandLine.TryParseValue(" ", string.Empty, out value));
		Assert.IsFalse(CommandLine.TryParseValue(" ", " ", out value));
		Assert.IsFalse(CommandLine.TryParseValue(null, " ", out value));
		Assert.IsFalse(CommandLine.TryParseValue(string.Empty, " ", out value));
	}

	[Test]
	public void ParseWithoutMatch()
	{
		string value;
		Assert.IsFalse(CommandLine.TryParseValue("-KeyButNotQuite=Value", "-Key", out value));
	}

	[Test]
	public void ParseWithoutValue()
	{
		string value;
		Assert.IsFalse(CommandLine.TryParseValue("-Key", "-Key", out value));
		Assert.IsFalse(CommandLine.TryParseValue("-Key ", "-Key", out value));
		Assert.IsFalse(CommandLine.TryParseValue("-Key  ", "-Key", out value));
	}

	[Test]
	public void ParseSimpleValue()
	{
		string value;
		Assert.IsTrue(CommandLine.TryParseValue("-Key=Value", "-Key", out value));
		Assert.AreEqual("Value", value);
	}

	[Test]
	public void ParseWithSingleSpaceBetweenKeyAndValue()
	{
		string value;
		Assert.IsTrue(CommandLine.TryParseValue("-Key Value -Key2 Value2", "-Key", out value));
		Assert.AreEqual("Value", value);
	}

	[Test]
	public void ParseWithEqualSignBetweenKeyAndValue()
	{
		string value;
		Assert.IsTrue(CommandLine.TryParseValue("-Key=Value -Key2 = Value2", "-Key", out value));
		Assert.AreEqual("Value", value);
	}

	[Test]
	public void ParseWithSpacesAndEqualSignBetweenKeyAndValue()
	{
		string value;
		Assert.IsTrue(CommandLine.TryParseValue("-Key = Value -Key2 = Value2", "-Key", out value));
		Assert.AreEqual("Value", value);
	}

	[Test]
	public void ParseSingleQuotedWord()
	{
		string value;
		Assert.IsTrue(CommandLine.TryParseValue("-Key = \"Word\" -Key2 = \"Word2\"", "-Key", out value));
		Assert.AreEqual("Word", value);
	}

	[Test]
	public void ParseMultipleQuotedWords()
	{
		string value;
		Assert.IsTrue(CommandLine.TryParseValue("-Key = \"Word1 Word2\" -Key2 = \"Word3 Word4\"", "-Key", out value));
		Assert.AreEqual("Word1 Word2", value);
	}

	[Test]
	public void ParseEmptyQuotedWord()
	{
		string value;
		Assert.IsTrue(CommandLine.TryParseValue("-Key = \"\" -Key2 = \"Word2\"", "-Key", out value));
		Assert.IsEmpty(value);
	}

	[Test]
	public void ParseSingleQuotationMark()
	{
		string value;
		Assert.IsTrue(CommandLine.TryParseValue("-Key = \"\\\"\"", "-Key", out value));
		Assert.AreEqual("\"", value);
	}

	[Test]
	public void ParseTwoQuotationMarks()
	{
		string value;
		Assert.IsTrue(CommandLine.TryParseValue("-Key = \"\\\"\\\"\"", "-Key", out value));
		Assert.AreEqual("\"\"", value);
	}

	[Test]
	public void ParseQuotedCharacter()
	{
		string value;
		Assert.IsTrue(CommandLine.TryParseValue("-Key = \"\\\"a\\\"\"", "-Key", out value));
		Assert.AreEqual("\"a\"", value);
	}

	[Test]
	public void ParseUnclosedQuotationMark()
	{
		string value;
		Assert.IsFalse(CommandLine.TryParseValue("-Key = \"", "-Key", out value));
		Assert.IsFalse(CommandLine.TryParseValue("-Key = \"   ", "-Key", out value));
		Assert.IsFalse(CommandLine.TryParseValue("-Key = \"string", "-Key", out value));
		Assert.IsFalse(CommandLine.TryParseValue("-Key = \"Word1 Word2", "-Key", out value));
		Assert.IsFalse(CommandLine.TryParseValue("-Key = \"Word1 \\\" Word2", "-Key", out value));
	}
}
