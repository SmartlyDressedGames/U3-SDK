////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;

internal static class DatValueEditTests
{
	[Test]
	public static void AddValueWithoutCommentAfterValueInDictionary()
	{
		string input = "Key1 Value1";
		string expectedOutput = "Key1 Value1\nKey2 Value2\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		dictionary.Edit().AddValue("Key2").Value = "Value2";

		TestOutput(dictionary, expectedOutput);
	}

	[Test]
	public static void AddValueWithCommentAfterValueInDictionary()
	{
		string input = "Key1 Value1";
		string expectedOutput = "Key1 Value1\n// A comment\nKey2 Value2\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		IEditableDatValue edit = dictionary.Edit().AddValue("Key2");
		edit.Comment = "A comment";
		edit.Value = "Value2";

		TestOutput(dictionary, expectedOutput);
	}

	/// <summary>
	/// When adding multiple values to a dictionary we want them sorted by the order they were added by default.
	/// </summary>
	[Test]
	public static void AddMultipleValuesToDictionary()
	{
		string input = "Key1 Value1";
		string expectedOutput = "Key1 Value1\nKey2 \"Value2\" // Comment 1\nKey3 Value3\n// Comment 2\nKey4 Value4\nKey5 Value5\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary dictionary = parser.Parse(input);
		dictionary.Edit().AddValue("Key2").SetString("Value2").SetInlineComment("Comment 1");
		dictionary.Edit().AddValue("Key3").SetString("Value3");
		dictionary.Edit().AddValue("Key4").SetString("Value4").SetComment("Comment 2");
		dictionary.Edit().AddValue("Key5").SetString("Value5");

		TestOutput(dictionary, expectedOutput);
	}

	[Test]
	public static void AddValuesToEmptyDictionary()
	{
		string input = "Key1 Value1\nTestDictionary\n{\n}\nKey2 Value2";
		string expectedOutput = "Key1 Value1\nTestDictionary\n{\n\tKey3 Value3\n\tKey4 Value4\n}\nKey2 Value2\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IDatDictionary dictionary = rootDictionary.GetDictionary("TestDictionary");
		dictionary.Edit().AddValue("Key3").SetString("Value3");
		dictionary.Edit().AddValue("Key4").SetString("Value4");

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void AddValuesToEmptyList()
	{
		string input = "Key1 Value1\nTestList\n[\n]\nKey2 Value2";
		string expectedOutput = "Key1 Value1\nTestList\n[\n\tList item 1\n\tList item 2\n]\nKey2 Value2\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IDatList list = rootDictionary.GetList("TestList");
		list.Edit().AddValue().SetString("List item 1");
		list.Edit().AddValue().SetString("List item 2");

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void AddListToRootDictionary()
	{
		string input = "Key1 Value1";
		string expectedOutput = "Key1 Value1\nList\n[\n\t64\n]\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IEditableDatList list = rootDictionary.Edit().AddList("List");
		list.AddValue().SetInt32(64);

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void AddDictionaryToRootDictionary()
	{
		string input = "Key1 Value1";
		string expectedOutput = "Key1 Value1\nDictionary\n{\n\tKey2 Value2\n}\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IEditableDatDictionary dictionary = rootDictionary.Edit().AddDictionary("Dictionary");
		dictionary.AddValue("Key2").SetString("Value2");

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void AddListToEmptyList()
	{
		string input = "Key1 Value1\nTestList\n[\n]\nKey2 Value2";
		string expectedOutput = "Key1 Value1\nTestList\n[\n\t[\n\t\t64\n\t]\n]\nKey2 Value2\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IDatList list = rootDictionary.GetList("TestList");
		list.Edit().AddList().AddValue().SetInt32(64);

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void AddListWithCommentToEmptyList()
	{
		string input = "Key1 Value1\nTestList\n[\n]\nKey2 Value2";
		string expectedOutput = "Key1 Value1\nTestList\n[\n\t// A comment\n\t[\n\t\t64\n\t]\n]\nKey2 Value2\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IDatList list = rootDictionary.GetList("TestList");
		list.Edit().AddList().SetComment("A comment").AddValue().SetInt32(64);

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void AddDictionaryToEmptyList()
	{
		string input = "Key1 Value1\nTestList\n[\n]\nKey2 Value2";
		string expectedOutput = "Key1 Value1\nTestList\n[\n\t{\n\t\tKey3 Value3\n\t}\n]\nKey2 Value2\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IDatList list = rootDictionary.GetList("TestList");
		list.Edit().AddDictionary().AddValue("Key3").SetString("Value3");

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void AddDictionaryWithCommentToEmptyList()
	{
		string input = "Key1 Value1\nTestList\n[\n]\nKey2 Value2";
		string expectedOutput = "Key1 Value1\nTestList\n[\n\t// A comment\n\t{\n\t\tKey3 Value3\n\t}\n]\nKey2 Value2\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IDatList list = rootDictionary.GetList("TestList");
		list.Edit().AddDictionary().SetComment("A comment").AddValue("Key3").SetString("Value3");

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void AddListToEmptyDictionary()
	{
		string input = "Key1 Value1\nTestDictionary\n{\n}\nKey2 Value2";
		string expectedOutput = "Key1 Value1\nTestDictionary\n{\n\tKey3\n\t[\n\t\t64\n\t]\n}\nKey2 Value2\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IDatDictionary dictionary = rootDictionary.GetDictionary("TestDictionary");
		dictionary.Edit().AddList("Key3").AddValue().SetInt32(64);

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void AddListWithCommentToEmptyDictionary()
	{
		string input = "Key1 Value1\nTestDictionary\n{\n}\nKey2 Value2";
		string expectedOutput = "Key1 Value1\nTestDictionary\n{\n\t// A comment\n\tKey3\n\t[\n\t\t64\n\t]\n}\nKey2 Value2\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IDatDictionary dictionary = rootDictionary.GetDictionary("TestDictionary");
		dictionary.Edit().AddList("Key3").SetComment("A comment").AddValue().SetInt32(64);

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void AddDictionaryToEmptyDictionary()
	{
		string input = "Key1 Value1\nTestDictionary\n{\n}\nKey2 Value2";
		string expectedOutput = "Key1 Value1\nTestDictionary\n{\n\tKey3\n\t{\n\t\tKey4 Value4\n\t}\n}\nKey2 Value2\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IDatDictionary dictionary = rootDictionary.GetDictionary("TestDictionary");
		dictionary.Edit().AddDictionary("Key3").AddValue("Key4").SetString("Value4");

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void AddDictionaryWithCommentToEmptyDictionary()
	{
		string input = "Key1 Value1\nTestDictionary\n{\n}\nKey2 Value2";
		string expectedOutput = "Key1 Value1\nTestDictionary\n{\n\t// A comment\n\tKey3\n\t{\n\t\tKey4 Value4\n\t}\n}\nKey2 Value2\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IDatDictionary dictionary = rootDictionary.GetDictionary("TestDictionary");
		dictionary.Edit().AddDictionary("Key3").SetComment("A comment").AddValue("Key4").SetString("Value4");

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void ReplaceValueWithValue()
	{
		string input = "Key1 Value1\nKey2 Value2\nKey3 Value3";
		string expectedOutput = "Key1 Value1\nKey2 Replacement Value\nKey3 Value3\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		rootDictionary.Edit().ReplaceWithValue("Key2").SetString("Replacement Value");
		
		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void ReplaceValueWithValueWithComment()
	{
		string input = "Key1 Value1\nKey2 Value2\nKey3 Value3";
		string expectedOutput = "Key1 Value1\n// A comment\nKey2 Replacement Value\nKey3 Value3\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		rootDictionary.Edit().ReplaceWithValue("Key2").SetString("Replacement Value").SetComment("A comment");

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void ReplaceValueWithDictionary()
	{
		string input = "Key1 Value1\nKey2 Value2\nKey3 Value3";
		string expectedOutput = "Key1 Value1\nKey2\n{\n\tTest 1\n}\nKey3 Value3\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IEditableDatDictionary dictionary = rootDictionary.Edit().ReplaceWithDictionary("Key2");
		dictionary.AddValue("Test").SetInt32(1);
		
		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void ReplaceValueWithDictionaryWithComment()
	{
		string input = "Key1 Value1\nKey2 Value2\nKey3 Value3";
		string expectedOutput = "Key1 Value1\n// A comment\nKey2\n{\n\tTest 1\n}\nKey3 Value3\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IEditableDatDictionary dictionary = rootDictionary.Edit().ReplaceWithDictionary("Key2").SetComment("A comment");
		dictionary.AddValue("Test").SetInt32(1);

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void ReplaceValueWithList()
	{
		string input = "Key1 Value1\nKey2 Value2\nKey3 Value3";
		string expectedOutput = "Key1 Value1\nKey2\n[\n\t64\n]\nKey3 Value3\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IEditableDatList list = rootDictionary.Edit().ReplaceWithList("Key2");
		list.AddValue().SetInt32(64);

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void ReplaceValueWithListWithComment()
	{
		string input = "Key1 Value1\nKey2 Value2\nKey3 Value3";
		string expectedOutput = "Key1 Value1\n// A comment\nKey2\n[\n\t64\n]\nKey3 Value3\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IEditableDatList list = rootDictionary.Edit().ReplaceWithList("Key2").SetComment("A comment");
		list.AddValue().SetInt32(64);

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void EditValueTopMargin()
	{
		string input = "Key1 Value1\nKey2 Value2\nKey3 Value3\n";
		string expectedOutput = "Key1 Value1\n\n\nKey2 Value2\nKey3 Value3\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		rootDictionary.Edit().TryGetValue("Key2", out IDatValue node);
		node.Edit().SetMargins(2, 0);

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void EditValueBottomMargin()
	{
		string input = "Key1 Value1\nKey2 Value2\nKey3 Value3\n";
		string expectedOutput = "Key1 Value1\nKey2 Value2\n\n\nKey3 Value3\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		rootDictionary.Edit().TryGetValue("Key2", out IDatValue node);
		node.Edit().SetMargins(0, 2);

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void CombineMargins()
	{
		string input = "Key1 Value1\nKey2 Value2\nKey3 Value3\n";
		string expectedOutput = "Key1 Value1\n\n\n\nKey2 Value2\n\n\nKey3 Value3\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		rootDictionary.Edit().TryGetValue("Key1", out IDatValue node1);
		node1.Edit().SetMargins(0, 3);
		rootDictionary.Edit().TryGetValue("Key2", out IDatValue node2);
		// Top margin should combine 3 > 2 = 3
		// Bottom margin should combine 2 > 1 = 2
		node2.Edit().SetMargins(2, 1);
		rootDictionary.Edit().TryGetValue("Key3", out IDatValue node3);
		node3.Edit().SetMargins(2, 0);

		TestOutput(rootDictionary, expectedOutput);
	}

	/// <summary>
	/// Margins at start and end of list should be ignored.
	/// </summary>
	[Test]
	public static void ZeroMarginsInList()
	{
		string input = "Key\n[\n\tValue1\n\tValue2\n]\n";
		string expectedOutput = "Key\n[\n\tValue1\n\n\tValue2\n]\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IEditableDatList list = rootDictionary.Edit().GetOrAddList("Key");
		list.TryGetValue(0, out IDatValue node1);
		node1.Edit().SetMargins(1, 1);
		list.TryGetValue(0, out IDatValue node2);
		node2.Edit().SetMargins(1, 1);

		TestOutput(rootDictionary, expectedOutput);
	}

	/// <summary>
	/// Margins at start and end of dictionary should be ignored.
	/// </summary>
	[Test]
	public static void ZeroMarginsInDictionary()
	{
		string input = "Dict\n{\n\tKey1 Value1\n\tKey2 Value2\n}\n";
		string expectedOutput = "Dict\n{\n\tKey1 Value1\n\n\tKey2 Value2\n}\n";

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IDatDictionary rootDictionary = parser.Parse(input);
		IEditableDatDictionary dict = rootDictionary.Edit().GetOrAddDictionary("Dict");
		dict.TryGetValue("Key1", out IDatValue node1);
		node1.Edit().SetMargins(1, 1);
		dict.TryGetValue("Key2", out IDatValue node2);
		node2.Edit().SetMargins(1, 1);

		TestOutput(rootDictionary, expectedOutput);
	}

	[Test]
	public static void EditBiggerFile()
	{
		string input = @"GUID f145006e7a704b77a4c54b2e121ee209
Type Fisher
Useable Fisher
ID 507

Size_X 3
Size_Y 1
Size_Z 0.25



Blueprints 1
Blueprint_0_Type Tool
Blueprint_0_Supplies 2
Blueprint_0_Supply_0_ID 40
Blueprint_0_Supply_0_Amount 5
Blueprint_0_Supply_1_ID 64
Blueprint_0_Supply_1_Amount 2
Blueprint_0_Level 1
Blueprint_0_Skill Craft
Blueprint_0_Build 27

EquipableModelParent LeftHook

Reward_ID 2";

		string expectedOutput = @"GUID f145006e7a704b77a4c54b2e121ee209
Type Fisher
Useable Fisher
ID 507

Size_X 3
Size_Y 1
Size_Z 0.25



Blueprints
[
	???
]
EquipableModelParent LeftHook

Reward_ID 2
";

		// Currently just looking to see whether replacing blueprints array would work as-expected.

		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IEditableDatDictionary rootDictionary = parser.Parse(input).Edit();
		rootDictionary.ReplaceWithList("Blueprints").AddValue().SetString("???");
		rootDictionary.Remove("Blueprint_0_Type");
		rootDictionary.Remove("Blueprint_0_Supplies");
		rootDictionary.Remove("Blueprint_0_Supply_0_ID");
		rootDictionary.Remove("Blueprint_0_Supply_0_Amount");
		rootDictionary.Remove("Blueprint_0_Supply_1_ID");
		rootDictionary.Remove("Blueprint_0_Supply_1_Amount");
		rootDictionary.Remove("Blueprint_0_Level");
		rootDictionary.Remove("Blueprint_0_Skill");
		rootDictionary.Remove("Blueprint_0_Build");

		expectedOutput = expectedOutput.Replace("\r\n", "\n"); // This is for the '@' multi-line literal.
		TestOutput(rootDictionary, expectedOutput);
	}

	private static void TestOutput(IDatDictionary dictionary, string expectedOutput)
	{
		System.IO.StringWriter stringWriter = new System.IO.StringWriter();
		stringWriter.NewLine = "\n";
		DatWriter datWriter = new DatWriter();
		datWriter.SetOutput(stringWriter);
		MetadataPreservingDatWriter mpWriter = new MetadataPreservingDatWriter();
		mpWriter.WriteRootDictionary(dictionary, datWriter);

		string actualOutput = stringWriter.ToString();
		Assert.AreEqual(expectedOutput, actualOutput);
	}
}
