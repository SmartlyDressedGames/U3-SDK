////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.Unturned;
using System.Collections.Generic;

internal class RedactionTests
{
	[TestCase("Ends with IP: 192.168.1.1", "Ends with IP: [redacted]", true)]
	[TestCase("Ends with IP: 192.168.1.1.", "Ends with IP: [redacted].", true)]
	[TestCase("Ends with IP: 192.168.1.1:27015", "Ends with IP: [redacted]:27015", true)]
	[TestCase("192.168.1.1 starts with IP", "[redacted] starts with IP", true)]
	[TestCase("192.168.1.1. starts with IP", "[redacted]. starts with IP", true)]
	[TestCase("192.168.1.1:27015 starts with IP", "[redacted]:27015 starts with IP", true)]
	[TestCase("IP in middle 192.168.1.1 of string", "IP in middle [redacted] of string", true)]
	[TestCase("IP in middle 192.168.1.1. of string", "IP in middle [redacted]. of string", true)]
	[TestCase("IP in middle 192.168.1.1:27015 of string", "IP in middle [redacted]:27015 of string", true)]
	[TestCase("Two 192.168.1.1 IPs 192.168.1.2 in string", "Two [redacted] IPs [redacted] in string", true)]
	[TestCase("Two 192.168.1.1. IPs 192.168.1.2. in string", "Two [redacted]. IPs [redacted]. in string", true)]
	[TestCase("Two 192.168.1.1:27015 IPs 192.168.1.2:27015 in string", "Two [redacted]:27015 IPs [redacted]:27015 in string", true)]
	[TestCase("IP 192.168.1.1 in middle and end 192.168.1.2", "IP [redacted] in middle and end [redacted]", true)]
	[TestCase("IP 192.168.1.1. in middle and end 192.168.1.2.", "IP [redacted]. in middle and end [redacted].", true)]
	[TestCase("IP 192.168.1.1:27015 in middle and end 192.168.1.2:27015", "IP [redacted]:27015 in middle and end [redacted]:27015", true)]
	[TestCase("Hello, world!", "Hello, world!", false)]
	[TestCase("1.2.3 is not an IP", "1.2.3 is not an IP", false)]
	[TestCase("1.2.3.c is not an IP", "1.2.3.c is not an IP", false)]
	[TestCase("1.2.3.4.5 is not an IP", "1.2.3.4.5 is not an IP", false)]
	[TestCase("1.2.3.4.5. is not an IP", "1.2.3.4.5. is not an IP", false)]
	[TestCase("Not an IP 1.2.3 in the middle", "Not an IP 1.2.3 in the middle", false)]
	[TestCase("Not an IP 1.2.3.c in the middle", "Not an IP 1.2.3.c in the middle", false)]
	[TestCase("Not an IP 1.2.3.4.5 in the middle", "Not an IP 1.2.3.4.5 in the middle", false)]
	[TestCase("Not an IP 1.2.3.4.5. in the middle", "Not an IP 1.2.3.4.5. in the middle", false)]
	[TestCase("Not an IP at the end: 1.2.3", "Not an IP at the end: 1.2.3", false)]
	[TestCase("Not an IP at the end: 1.2.3.c", "Not an IP at the end: 1.2.3.c", false)]
	[TestCase("Not an IP at the end: 1.2.3.4.5", "Not an IP at the end: 1.2.3.4.5", false)]
	[TestCase("Not an IP at the end: 1.2.3.4.5.", "Not an IP at the end: 1.2.3.4.5.", false)]
	public void RedactIPv4Addresses(string input, string expectedOutput, bool expectedResult)
	{
		string actualOutput = input;
		bool actualResult = SDG.Unturned.Logs.RedactIPv4Addresses(ref actualOutput);
		Assert.AreEqual(expectedOutput, actualOutput);
		Assert.AreEqual(expectedResult, actualResult);
	}
}
