////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using Unturned.SystemEx;

internal class PortTests
{
	[Test]
	public void ParseSinglePortRange()
	{
		ushort actualMinPort;
		ushort actualMaxPort;
		Assert.IsTrue(IPv4Address.TryParsePortRange("27015", out actualMinPort, out actualMaxPort), "parsed range");
		Assert.AreEqual(27015, actualMinPort, "min port value");
		Assert.AreEqual(27015, actualMaxPort, "max port value");
	}

	[Test]
	public void ParsePortRange()
	{
		ushort actualMinPort;
		ushort actualMaxPort;
		Assert.IsTrue(IPv4Address.TryParsePortRange("27015-27016", out actualMinPort, out actualMaxPort), "parsed range");
		Assert.AreEqual(27015, actualMinPort, "min port value");
		Assert.AreEqual(27016, actualMaxPort, "max port value");
	}

	[Test]
	public void ParseSubstringWithSuffix()
	{
		ushort actualMinPort;
		ushort actualMaxPort;
		Assert.IsTrue(IPv4Address.TryParsePortRange("27015-27016suffix", 0, 11, out actualMinPort, out actualMaxPort), "parsed range");
		Assert.AreEqual(27015, actualMinPort, "min port value");
		Assert.AreEqual(27016, actualMaxPort, "max port value");
	}

	[Test]
	public void ParseSubstringWithPrefix()
	{
		ushort actualMinPort;
		ushort actualMaxPort;
		Assert.IsTrue(IPv4Address.TryParsePortRange("prefix27015-27016", 6, 11, out actualMinPort, out actualMaxPort), "parsed range");
		Assert.AreEqual(27015, actualMinPort, "min port value");
		Assert.AreEqual(27016, actualMaxPort, "max port value");
	}

	[Test]
	public void ParseSubstringWithPrefixAndSuffix()
	{
		ushort actualMinPort;
		ushort actualMaxPort;
		Assert.IsTrue(IPv4Address.TryParsePortRange("prefix27015-27016suffix", 6, 11, out actualMinPort, out actualMaxPort), "parsed range");
		Assert.AreEqual(27015, actualMinPort, "min port value");
		Assert.AreEqual(27016, actualMaxPort, "max port value");
	}

	[Test]
	public void ParseInvalidPortRange()
	{
		string[] testInputs = new string[]
		{
			null,
			string.Empty,
			"-",
			" -",
			"- ",
			"1-a",
			"a-1",
			"a1-2",
			"1-2a",
			"a1-2a",
		};

		foreach (string input in testInputs)
		{
			ushort minPort;
			ushort maxPort;
			Assert.IsFalse(IPv4Address.TryParsePortRange(input, out minPort, out maxPort), $"parse \"{input}\"");
		}
	}
}
