////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;

internal class DatWriterTests
{
	[Test]
	public void WriteKeyValuePairsInRoot()
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteKeyValue("a", "b");
		writer.WriteKeyValue("c", "d");

		string result = sw.ToString();
		Assert.AreEqual("a b\nc d\n", result);
	}

	[Test]
	public void WriteKeyWithoutValue()
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteKeyValue("a", null);
		writer.WriteKeyValue("b", null, "c");

		string result = sw.ToString();
		Assert.AreEqual("a\nb // c\n", result);
	}

	[TestCase("2023/04/19", "key 2023-04-19\n")]
	[TestCase("2023/04/19 3:14pm", "key 2023-04-19 15:14:00\n")]
	public void WriteDateTime(string input, string expectedOutput)
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteKeyValue("key", System.DateTime.Parse(input, System.Globalization.CultureInfo.InvariantCulture));

		string actualOutput = sw.ToString();
		Assert.AreEqual(expectedOutput, actualOutput);
	}

	/// <summary>
	/// If value starts with a quotation mark it will need to be quoted,
	/// otherwise parser will read it as a quoted string potentially without an end quote.
	/// </summary>
	[Test]
	public void WriteValueQuotationMark()
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteKeyValue("key", "\"");

		string result = sw.ToString();
		Assert.AreEqual("key \"\\\"\"\n", result);
	}

	/// <summary>
	/// If value contains quotation marks but doesn't start with one and doesn't end with a comment it doesn't need to be quoted.
	/// </summary>
	[Test]
	public void WriteValueContainingQuotationMarks()
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteKeyValue("key", "Hello, \"world\"!");

		string result = sw.ToString();
		Assert.AreEqual("key Hello, \"world\"!\n", result);
	}

	/// <summary>
	/// Backslash needs to be escaped within quoted value, otherwise it will be parsed as an escape.
	/// </summary>
	[Test]
	public void WriteQuotedValueContainingBackslash()
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteKeyValue("key", "\"\\"); // Value is: "\

		string result = sw.ToString();
		Assert.AreEqual("key \"\\\"\\\\\"\n", result);
	}

	/// <summary>
	/// '\n' needs to be escaped with backslash.
	/// '\r' should be skipped in output because we skip '\r' in input. (public issue #5226)
	/// </summary>
	[TestCase("\n", "key \\n\n")]
	[TestCase("\"\n", "key \"\\\"\\n\"\n")]
	[TestCase("a\rb", "key ab\n")]
	[TestCase("a\r\nb", "key a\\nb\n")]
	public void WriteQuotedValueContainingNewLine(string input, string expectedOutput)
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteKeyValue("key", input);

		string actualOutput = sw.ToString();
		Assert.AreEqual(expectedOutput, actualOutput);
	}

	[Test]
	public void WriteValuesInList()
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteKey("list");
		writer.WriteListStart();
		writer.WriteValue("a");
		writer.WriteValue("b");
		writer.WriteListEnd();

		string result = sw.ToString();
		Assert.AreEqual("list\n[\n\ta\n\tb\n]\n", result);
	}

	[Test]
	public void WriteCommentedValuesInList()
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteKey("list");
		writer.WriteListStart();
		writer.WriteValue("a", "1");
		writer.WriteValue("b", "2");
		writer.WriteListEnd();

		string result = sw.ToString();
		Assert.AreEqual("list\n[\n\t\"a\" // 1\n\t\"b\" // 2\n]\n", result);
	}

	[Test]
	public void WriteValuesInDictionary()
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteKey("dict");
		writer.WriteDictionaryStart();
		writer.WriteKeyValue("a", "b");
		writer.WriteDictionaryEnd();

		string result = sw.ToString();
		Assert.AreEqual("dict\n{\n\ta b\n}\n", result);
	}

	[Test]
	public void WriteCommentedValuesInDictionary()
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteKey("dict");
		writer.WriteDictionaryStart();
		writer.WriteKeyValue("a", "b", "c");
		writer.WriteDictionaryEnd();

		string result = sw.ToString();
		Assert.AreEqual("dict\n{\n\ta \"b\" // c\n}\n", result);
	}

	[Test]
	public void WriteExistingDictionary()
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		IDatDictionary dict = new DatDictionary()
		{
			{ "a", new DatValue("b") },
			{ "c", new DatValue("d") },
		};

		writer.WriteKey("dict");
		writer.WriteDictionary(dict);

		string result = sw.ToString();
		Assert.AreEqual("dict\n{\n\ta b\n\tc d\n}\n", result);
	}

	[Test]
	public void WriteExistingList()
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		DatList list = new DatList()
		{
			new DatValue("a"),
			new DatValue("b"),
		};

		writer.WriteKey("list");
		writer.WriteList(list);

		string result = sw.ToString();
		Assert.AreEqual("list\n[\n\ta\n\tb\n]\n", result);
	}

	[Test]
	public void CloseStack()
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteKey("key");
		writer.WriteListStart();
		writer.WriteListStart();
		writer.WriteDictionaryStart();
		writer.CloseStack();

		string result = sw.ToString();
		Assert.AreEqual("key\n[\n\t[\n\t\t{\n\t\t}\n\t]\n]\n", result);
	}

	[Test]
	public void Disposable()
	{
		string result;
		using (System.IO.StringWriter sw = new System.IO.StringWriter())
		{
			sw.NewLine = "\n";
			using (DatWriter writer = new DatWriter(sw))
			{
				writer.WriteKey("key");
				writer.WriteDictionaryStart();
				writer.WriteKeyValue("a", "b");
				// intentionally not closed, will be handled by Dispose
			}
			result = sw.ToString();
		}

		Assert.AreEqual("key\n{\n\ta b\n}\n", result);
	}

	[Test]
	public void WriteRegularComments()
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteComment("Hello,");
		writer.WriteComment("world!");

		string result = sw.ToString();
		Assert.AreEqual("// Hello,\n// world!\n", result);
	}

	[TestCase("")]
	[TestCase("key value\n")]
	[TestCase("dictionary\n{\n\tkey value\n}\n")]
	[TestCase("list\n[\n\tvalue1\n\tvalue2\n]\n")]
	[TestCase("list\n[\n\t{\n\t\tkey value\n\t}\n]\n")] // dictionary within list
	[TestCase("dictionary1\n{\n\tdictionary2\n\t{\n\t\tkey value\n\t}\n}\n")] // dictionary within dictionary
	public void MirrorParsedData(string input)
	{
		DatParser parser = new DatParser();
		IDatDictionary root = parser.Parse(input);
		Assert.IsFalse(parser.HasError);

		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteDictionary(root);
		string result = sw.ToString();
		Assert.AreEqual(input, result);
	}

	[System.Flags]
	public enum EDatTestEnumFlags
	{
		None = 0,
		Flag1 = 1 << 0,
		Flag2 = 2 << 1,
	}

	[Test]
	public void WriteEnumFlags()
	{
		DatWriter writer = new DatWriter();
		System.IO.StringWriter sw = new System.IO.StringWriter();
		sw.NewLine = "\n";
		writer.SetOutput(sw);

		writer.WriteKeyValueEnumString("Key", EDatTestEnumFlags.Flag1 | EDatTestEnumFlags.Flag2);

		string result = sw.ToString();
		Assert.AreEqual("Key Flag1, Flag2\n", result);
	}
}
