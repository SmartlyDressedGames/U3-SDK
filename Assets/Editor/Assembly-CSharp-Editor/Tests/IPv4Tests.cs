////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;
using Unturned.SystemEx;

internal class IPv4Tests
{
	[Test]
	public void ParseLoopbackAddress()
	{
		uint actualAddress;
		Assert.IsTrue(IPv4Address.TryParse("127.0.0.1", out actualAddress), "parsed address");
		const uint expectedAddress = (127u << 24) | 1u;
		Assert.AreEqual(expectedAddress, actualAddress, "address value");
	}

	[Test]
	public void LoopbackAddressToString()
	{
		IPv4Address address;
		IPv4Address.TryParse("127.0.0.1", out address);
		Assert.AreEqual("127.0.0.1", address.ToString(), "address value");
	}

	[Test]
	public void ValidLocalPrivateAddresses()
	{
		IPv4Address[] testAddresses = new IPv4Address[]
		{
			new IPv4Address("10.0.0.0"),
			new IPv4Address("10.1.2.3"),
			new IPv4Address("10.255.255.255"),

			new IPv4Address("172.16.0.0"),
			new IPv4Address("172.16.5.6"),
			new IPv4Address("172.31.255.255"),

			new IPv4Address("192.168.0.0"),
			new IPv4Address("192.168.7.8"),
			new IPv4Address("192.168.255.255"),
		};

		foreach (IPv4Address address in testAddresses)
		{
			Assert.IsTrue(address.IsLocalPrivate, address.ToString());
		}
	}

	[Test]
	public void InvalidLocalPrivateAddresses()
	{
		IPv4Address[] testAddresses = new IPv4Address[]
		{
			new IPv4Address("1.2.3.4"),
			new IPv4Address("5.6.7.8"),
			new IPv4Address("9.10.11.12"),
		};

		foreach (IPv4Address address in testAddresses)
		{
			Assert.IsFalse(address.IsLocalPrivate, address.ToString());
		}
	}

	[Test]
	public void ValidWanAddresses()
	{
		IPv4Address[] testAddresses = new IPv4Address[]
		{
			// Some well known public DNS server IPs
			new IPv4Address("9.9.9.9"), // quad9 primary
			new IPv4Address("149.112.112.112"), // quad9 secondary
			new IPv4Address("8.8.8.8"), // Google primary
			new IPv4Address("8.8.4.4"), // Google secondary
		};

		foreach (IPv4Address address in testAddresses)
		{
			Assert.IsTrue(address.IsWideAreaNetwork, address.ToString());
		}
	}

	[Test]
	public void NonWanAddresses()
	{
		IPv4Address[] testAddresses = new IPv4Address[]
		{
			new IPv4Address("127.0.0.1"),
			new IPv4Address("127.0.0.255"),

			new IPv4Address("10.0.0.0"),
			new IPv4Address("10.1.2.3"),
			new IPv4Address("10.255.255.255"),

			new IPv4Address("172.16.0.0"),
			new IPv4Address("172.16.5.6"),
			new IPv4Address("172.31.255.255"),

			new IPv4Address("192.168.0.0"),
			new IPv4Address("192.168.7.8"),
			new IPv4Address("192.168.255.255"),

			// Link-local
			new IPv4Address("169.254.0.0"),
			new IPv4Address("169.254.127.127"),
			new IPv4Address("169.254.255.255"),
		};

		foreach (IPv4Address address in testAddresses)
		{
			Assert.IsFalse(address.IsWideAreaNetwork, address.ToString());
		}
	}

	[Test]
	public void ParseSubstringWithSuffix()
	{
		IPv4Address actualAddress;
		Assert.IsTrue(IPv4Address.TryParse("192.168.1.1suffix", 0, 11, out actualAddress), "parsed address");
		IPv4Address expectedAddress = new IPv4Address("192.168.1.1");
		Assert.AreEqual(expectedAddress, actualAddress, "address value");
	}

	[Test]
	public void ParseSubstringWithPrefix()
	{
		IPv4Address actualAddress;
		Assert.IsTrue(IPv4Address.TryParse("prefix192.168.1.1", 6, 11, out actualAddress), "parsed address");
		IPv4Address expectedAddress = new IPv4Address("192.168.1.1");
		Assert.AreEqual(expectedAddress, actualAddress, "address value");
	}

	[Test]
	public void ParseSubstringWithPrefixAndSuffix()
	{
		IPv4Address actualAddress;
		Assert.IsTrue(IPv4Address.TryParse("prefix192.168.1.1suffix", 6, 11, out actualAddress), "parsed address");
		IPv4Address expectedAddress = new IPv4Address("192.168.1.1");
		Assert.AreEqual(expectedAddress, actualAddress, "address value");
	}

	/// <summary>
	/// This test aims to address public issue #4413 by ignoring whitespace in first and last digits.
	/// </summary>
	[Test]
	public void ParseWithUntrimmedWhitespace()
	{
		string[] testAddresses = new string[]
		{
			" 192.168.1.1",
			"192.168.1.1 ",
			" 192.168.1.1 ",
			"\u200b192.168.1.1",
			"192.168.1.1​​\u200b",
			"\u200b192.168.1.1​​\u200b",
		};

		IPv4Address expectedAddress = new IPv4Address("192.168.1.1");

		foreach (string address in testAddresses)
		{
			IPv4Address actualAddress;
			Assert.IsTrue(IPv4Address.TryParse(address, out actualAddress), $"parse \"{address}\"");
			Assert.AreEqual(expectedAddress, actualAddress, "address value");
		}
	}

	[Test]
	public void ParseInvalidAddress()
	{
		string[] testAddresses = new string[]
		{
			null,
			string.Empty,
			".",
			"..",
			"...",
			"....",
			"256.256.256.256",
			"1.2.3.4.5",
			" . ",
			" . . ",
			" . . . ",
			" . . . . ",
		};

		foreach (string address in testAddresses)
		{
			uint value;
			Assert.IsFalse(IPv4Address.TryParse(address, out value), $"parse \"{address}\"");
			Assert.AreEqual(0, value);
		}
	}

	[Test]
	public void ParseWithPort()
	{
		IPv4Address actualAddress;
		ushort? optionalPort;
		Assert.IsTrue(IPv4Address.TryParseWithOptionalPort("192.168.1.1:27015", out actualAddress, out optionalPort), "parsed address");
		IPv4Address expectedAddress = new IPv4Address("192.168.1.1");
		Assert.AreEqual(expectedAddress, actualAddress, "address value");
		Assert.AreEqual(27015, optionalPort, "port");
	}

	[TestCase("16", 0xFFFF0000u, true)]
	[TestCase("24", 0xFFFFFF00u, true)]
	[TestCase("", 0xFFFFFFFFu, false)]
	[TestCase("0", 0xFFFFFFFFu, false)]
	[TestCase("32", 0xFFFFFFFFu, false)]
	[TestCase("64", 0xFFFFFFFFu, false)]
	public void TryParseSubnetMask(string input, uint expectedSubnetMask, bool expectedSuccess)
	{
		bool actualSuccess = IPv4SubnetMask.TryParse(input, out IPv4SubnetMask actualMask);
		Assert.AreEqual(expectedSuccess, actualSuccess, "success");
		Assert.AreEqual(new IPv4SubnetMask(expectedSubnetMask), actualMask, "mask");
	}

	[TestCase("192.168.1.0", 24, "192.168.1.1", true)]
	[TestCase("192.168.1.0", 24, "192.168.1.254", true)]
	[TestCase("192.168.1.0", 24, "192.168.2.0", false)]
	[TestCase("192.168.0.0", 16, "192.168.3.63", true)]
	[TestCase("192.168.0.0", 16, "127.0.0.1", false)]
	public void SubnetMaskContainsHost(string routingPrefix, int subnetMaskBits, string hostAddress, bool expectedContains)
	{
		IPv4Address routingPrefixAddress = new IPv4Address(routingPrefix);
		IPv4SubnetMask mask = new IPv4SubnetMask(subnetMaskBits);
		IPv4Address address = new IPv4Address(hostAddress);
		Assert.AreEqual(expectedContains, mask.ContainsHost(routingPrefixAddress, address));
	}

	[TestCase(32)]
	[TestCase(31)]
	[TestCase(30)]
	[TestCase(24)]
	[TestCase(16)]
	[TestCase(8)]
	[TestCase(1)]
	public void CountSubnetMaskBits(int inputNetworkBits)
	{
		int expectedHostBits = 32 - inputNetworkBits;
		IPv4SubnetMask mask = new IPv4SubnetMask(inputNetworkBits);
		Assert.AreEqual(inputNetworkBits, mask.CountNetworkBits, $"network bits (mask: {mask})");
		Assert.AreEqual(expectedHostBits, mask.CountHostBits, $"host bits (mask: {mask})");
	}

	[TestCase("127.0.0.1", "127.0.0.1", 32, ushort.MinValue, ushort.MaxValue, true)]
	[TestCase("127.0.0.1:27015", "127.0.0.1", 32, (ushort) 27015, (ushort) 27015, true)]
	[TestCase("127.0.0.1:27015-27016", "127.0.0.1", 32, (ushort) 27015, (ushort) 27016, true)]
	[TestCase("192.168.1.0/24", "192.168.1.0", 24, ushort.MinValue, ushort.MaxValue, true)]
	[TestCase("127.0.0.1/24:27015", "127.0.0.0", 24, (ushort) 27015, (ushort) 27015, true)]
	[TestCase("127.0.0.1/24:27015-27016", "127.0.0.0", 24, (ushort) 27015, (ushort) 27016, true)]
	[TestCase("127.0.0.1/x:27015-27016", "127.0.0.1", 32, (ushort) 27015, (ushort) 27016, false)]
	[TestCase("127.0.0.1/24:y-27016", "127.0.0.0", 24, ushort.MinValue, ushort.MaxValue, false)]
	public void TryParseFilter(string input, string expectedAddress, int expectedSubnetMaskBits, ushort expectedMinPort, ushort expectedMaxPort, bool expectedSuccess)
	{
		bool actualSuccess = IPv4Filter.TryParse(input, out IPv4Filter actualFilter);
		Assert.AreEqual(expectedSuccess, actualSuccess, "success");
		IPv4Filter expectedFilter = new IPv4Filter(new IPv4Address(expectedAddress), new IPv4SubnetMask(expectedSubnetMaskBits), expectedMinPort, expectedMaxPort);
		Assert.AreEqual(expectedFilter, actualFilter, "mask");
	}

	[TestCase("127.0.0.1", "127.0.0.1:27015", true)]
	[TestCase("127.0.0.1:27015", "127.0.0.1:27015", true)]
	[TestCase("127.0.0.1:27021", "127.0.0.1:27015", false)]
	[TestCase("127.0.0.1:27000-27030", "127.0.0.1:27015", true)]
	[TestCase("127.0.0.1:27000-27030", "127.0.0.1:26999", false)]
	[TestCase("127.0.0.1:27000-27030", "127.0.0.1:27031", false)]
	[TestCase("192.168.45.0/24", "192.168.45.1:27015", true)]
	[TestCase("192.168.45.0/24", "192.168.45.254:27015", true)]
	[TestCase("192.168.45.0/24", "192.168.44.254:27015", false)]
	[TestCase("192.168.45.0/24", "192.168.46.1:27015", false)]
	[TestCase("192.168.45.0/24:27000-27030", "192.168.45.1:27015", true)]
	[TestCase("192.168.45.0/24:27000-27030", "192.168.45.1:26999", false)]
	[TestCase("192.168.45.0/24:27000-27030", "192.168.45.1:27031", false)]
	public void MatchesFilter(string inputFilter, string inputAddress, bool expectedMatches)
	{
		Assert.IsTrue(IPv4Filter.TryParse(inputFilter, out IPv4Filter filter), "parsed filter");
		Assert.IsTrue(IPv4Address.TryParseWithOptionalPort(inputAddress, out IPv4Address hostAddress, out ushort? hostPort), "parsed address");
		Assert.IsTrue(hostPort.HasValue, "parsed port");
		Assert.AreEqual(expectedMatches, filter.Matches(hostAddress, hostPort.Value));
	}

	[TestCase("192.168.1.1", "192.168.1.1", "192.168.1.1")]
	[TestCase("192.168.1.0/24", "192.168.1.0", "192.168.1.255")]
	[TestCase("192.168.1.16/28", "192.168.1.16", "192.168.1.31")]
	public void GetFilterAddressRange(string inputFilter, string inputExpectedMinAddress, string inputExpectedMaxAddress)
	{
		Assert.IsTrue(IPv4Filter.TryParse(inputFilter, out IPv4Filter filter), "parsed filter");
		Assert.IsTrue(IPv4Address.TryParse(inputExpectedMinAddress, out IPv4Address expectedMinAddress), "parsed min address");
		Assert.IsTrue(IPv4Address.TryParse(inputExpectedMaxAddress, out IPv4Address expectedMaxAddress), "parsed max address");
		filter.GetAddressRange(out IPv4Address actualMinAddress, out IPv4Address actualMaxAddress);
		Assert.AreEqual(expectedMinAddress, actualMinAddress);
		Assert.AreEqual(expectedMaxAddress, actualMaxAddress);
		Assert.IsTrue(filter.Matches(actualMinAddress, 0));
		if (actualMinAddress.value > 0)
		{
			Assert.IsFalse(filter.Matches(new IPv4Address(actualMinAddress.value - 1), 0));
		}
		Assert.IsTrue(filter.Matches(actualMaxAddress, 0));
		if (actualMaxAddress.value < uint.MaxValue)
		{
			Assert.IsFalse(filter.Matches(new IPv4Address(actualMaxAddress.value + 1), 0));
		}
	}
}
