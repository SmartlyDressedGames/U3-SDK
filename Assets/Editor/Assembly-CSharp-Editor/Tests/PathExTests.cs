////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using Unturned.SystemEx;

internal class PathExTests
{
	[TestCase("|charatstart.txt", "_charatstart.txt")]
	[TestCase("char|inmid.txt", "char_inmid.txt")]
	[TestCase("charatend.txt|", "charatend.txt_")]
	[TestCase("|charatstart|andmid.txt", "_charatstart_andmid.txt")]
	[TestCase("chars|in|mid.txt", "chars_in_mid.txt")]
	[TestCase("charatend|andmid.txt|", "charatend_andmid.txt_")]
	[TestCase("regular.txt", "regular.txt")]
	public void ReplaceInvalidFileNameChars(string input, string expectedOutput)
	{
		string actualOutput = PathEx.ReplaceInvalidFileNameChars(input, '_');
		Assert.AreEqual(expectedOutput, actualOutput);
	}
}
