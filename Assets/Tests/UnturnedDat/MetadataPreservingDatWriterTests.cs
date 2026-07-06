////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;

internal class MetadataPreservingDatWriterTests
{
	[TestCase("")]
	[TestCase("Key Value\n")]
	[TestCase("// A comment\nKey Value\n")]
	[TestCase("// A comment\nKey \"Value\" // Inline comment\n")]
	[TestCase("// Comment A\n// Comment B\nFlag\n")]
	[TestCase("// Comment A\nKey1 Value1\n\n// Comment B\nKey2 Value2\n")]
	[TestCase("List\n[\n\tItem 1\n\n\t// Comment\n\tItem 2\n]\n")]
	[TestCase("Dictionary\n{\n\tKey1 Value1\n\n\t// Comment\n\tKey2 Value2\n}\n")]
	[TestCase("Flag\n")]
	[TestCase(@"// First line comment
Key1 ""Value1"" // Inline comment


Key2 Value2

Flag1
Flag2

// Comment
Flag3


// Comment A
// Comment B
Flag4

// Comment 2 line 1 of 2
// Comment 2 line 2 of 2
Key3 Value3

ListWithoutPrefixComment
[
	List value 1
	List value 2

	[
		Sub-list value 1
		Sub-list value 2
	]

	{
		X 3
		Y 7
	}
]

DictionaryWithoutPrefixComment
{
	Key4 Value 4
	Key5 Value 5
}


// A comment
ListWithPrefixComment
[
	List value 3
	List value 4
]

// Dictionary comment
DictionaryWithPrefixComment
{
	Key6 Value 6
	Key7 Value 7
}
")]
	public void PreserveInputExactly(string input)
	{
		input = input.Replace("\r\n", "\n"); // This is for the '@' multi-line literal.

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);

		System.IO.StringWriter stringWriter = new System.IO.StringWriter();
		stringWriter.NewLine = "\n";
		DatWriter datWriter = new DatWriter();
		datWriter.SetOutput(stringWriter);
		MetadataPreservingDatWriter mpWriter = new MetadataPreservingDatWriter();
		mpWriter.WriteRootDictionary(dictionary, datWriter);

		string output = stringWriter.ToString();
		Assert.AreEqual(input, output);
	}
}
