////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using Unturned.SystemEx;

internal class ByteDisplayTests
{
	[TestCase(0, "0 B")]
	[TestCase(1, "1 B")]
	[TestCase(-1, "-1 B")]
	[TestCase(512, "512 B")]
	[TestCase(-512, "-512 B")]
	[TestCase(1000, "1 kB")]
	[TestCase(-1000, "-1 kB")]
	[TestCase(1100, "1.1 kB")]
	[TestCase(-1100, "-1.1 kB")]
	[TestCase(1100000, "1.1 MB")]
	[TestCase(-1100000, "-1.1 MB")]
	[TestCase(1120000, "1.12 MB")]
	[TestCase(-1120000, "-1.12 MB")]
	[TestCase(1121034, "1.12 MB")]
	[TestCase(-1121034, "-1.12 MB")]
	public void Base10ToString(long input, string expectedString)
	{
		string actualString = ByteDisplay.Base10ToString(input);
		Assert.AreEqual(expectedString, actualString);
	}

	[TestCase(0, "0 B")]
	[TestCase(1, "1 B")]
	[TestCase(-1, "-1 B")]
	[TestCase(512, "512 B")]
	[TestCase(-512, "-512 B")]
	[TestCase(1024, "1 KiB")]
	[TestCase(-1024, "-1 KiB")]
	[TestCase(1536, "1.5 KiB")]
	[TestCase(-1536, "-1.5 KiB")]
	[TestCase(1048576, "1 MiB")]
	[TestCase(-1048576, "-1 MiB")]
	public void Base2ToString(long input, string expectedString)
	{
		string actualString = ByteDisplay.Base2ToString(input);
		Assert.AreEqual(expectedString, actualString);
	}
}
