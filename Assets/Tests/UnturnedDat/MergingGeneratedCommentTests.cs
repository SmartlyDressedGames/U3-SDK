////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;

internal class MergingGeneratedCommentTests
{
	/*
	 * Adding generated comments with no prior comment
	 */
	[TestCase("Key Value\n", /*gen:*/ "Generated", /*output:*/ "// > Generated\nKey Value\n")]
	[TestCase("Key Value\n", /*gen:*/ "Generated Line 1\nGenerated Line 2", /*output:*/ "// > Generated Line 1\n// > Generated Line 2\nKey Value\n")]
	[TestCase("Key Value\n", /*gen:*/ "Generated Line 1\n\nGenerated Line 3", /*output:*/ "// > Generated Line 1\n// > \n// > Generated Line 3\nKey Value\n")]
	/*
	 * Adding generated comments with a prior single-line comment
	 */
	[TestCase("// A comment\nKey Value\n", /*gen:*/ "Generated", /*output:*/ "// > Generated\n// A comment\nKey Value\n")]
	[TestCase("// A comment\nKey Value\n", /*gen:*/ "Generated Line 1\nGenerated Line 2", /*output:*/ "// > Generated Line 1\n// > Generated Line 2\n// A comment\nKey Value\n")]
	[TestCase("// A comment\nKey Value\n", /*gen:*/ "Generated Line 1\n\nGenerated Line 3", /*output:*/ "// > Generated Line 1\n// > \n// > Generated Line 3\n// A comment\nKey Value\n")]
	/*
	 * Adding generated comments with a prior multi-line comment
	 */
	[TestCase("// A comment line 1\n// A comment line 2\nKey Value\n", /*gen:*/ "Generated", /*output:*/ "// > Generated\n// A comment line 1\n// A comment line 2\nKey Value\n")]
	[TestCase("// A comment line 1\n// A comment line 2\nKey Value\n", /*gen:*/ "Generated Line 1\nGenerated Line 2", /*output:*/ "// > Generated Line 1\n// > Generated Line 2\n// A comment line 1\n// A comment line 2\nKey Value\n")]
	[TestCase("// A comment line 1\n// A comment line 2\nKey Value\n", /*gen:*/ "Generated Line 1\n\nGenerated Line 3", /*output:*/ "// > Generated Line 1\n// > \n// > Generated Line 3\n// A comment line 1\n// A comment line 2\nKey Value\n")]
	/*
	 * Changing generated comments with prior single-line comments before, after, and mixed
	 */
	[TestCase("// Ahead\n// > Before\nKey Value\n", /*gen:*/ "After", /*output:*/ "// Ahead\n// > After\nKey Value\n")]
	[TestCase("// > Before\n// Below\nKey Value\n", /*gen:*/ "After", /*output:*/ "// > After\n// Below\nKey Value\n")]
	[TestCase("// Ahead\n// > Before\n// Below\nKey Value\n", /*gen:*/ "After", /*output:*/ "// Ahead\n// > After\n// Below\nKey Value\n")]
	/*
	 * Changing generated comments with prior empty single-line comments before, after, and mixed
	 */
	[TestCase("// \n// > Before\nKey Value\n", /*gen:*/ "After", /*output:*/ "// \n// > After\nKey Value\n")]
	[TestCase("// > Before\n// \nKey Value\n", /*gen:*/ "After", /*output:*/ "// > After\n// \nKey Value\n")]
	[TestCase("// \n// > Before\n//\nKey Value\n", /*gen:*/ "After", /*output:*/ "// \n// > After\n// \nKey Value\n")]
	/*
	 * Removing generated comments
	 */
	[TestCase("// Comment\nKey Value", /*gen:*/ "", /*output:*/ "// Comment\nKey Value\n")]
	[TestCase("// Comment\n// > Generated\nKey Value", /*gen:*/ "", /*output:*/ "// Comment\nKey Value\n")]
	[TestCase("// > Generated\n// Comment\nKey Value", /*gen:*/ "", /*output:*/ "// Comment\nKey Value\n")]
	[TestCase("// > Generated\n// Comment\n// > Generated\nKey Value", /*gen:*/ "", /*output:*/ "// Comment\nKey Value\n")]
	public void MergeGeneratedComment(string input, string generatedLines, string expectedOutput)
	{
		RunTest(input, generatedLines, expectedOutput, "first");

		// Applying generated lines to expected output should result in no change.
		RunTest(expectedOutput, generatedLines, expectedOutput, "re-running");
	}

	private static void RunTest(string input, string generatedLines, string expectedOutput, string pass)
	{
		DatParser parser = new DatParser();
		parser.EnableMetadata = true;
		IEditableDatDictionary dictionary = parser.Parse(input).Edit();
		IEditableDatValue key = dictionary.GetOrAddValue("Key");

		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		System.Collections.Generic.List<string> lines = new System.Collections.Generic.List<string>();
		string[] generatedLinesArray = string.IsNullOrEmpty(generatedLines) ? null : generatedLines.Split('\n');
		key.MergeGeneratedComment("> ", generatedLinesArray, sb, lines);

		System.IO.StringWriter stringWriter = new System.IO.StringWriter();
		stringWriter.NewLine = "\n";
		DatWriter datWriter = new DatWriter();
		datWriter.SetOutput(stringWriter);
		MetadataPreservingDatWriter mpWriter = new MetadataPreservingDatWriter();
		mpWriter.WriteRootDictionary(dictionary, datWriter);

		string actualOutput = stringWriter.ToString();
		Assert.AreEqual(expectedOutput, actualOutput, $"Pass: {pass}");
	}
}
