////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using Unturned.SystemEx;

/// <summary>
/// For APIs which seem to not fully handle non-ascii characters e.g. older Steam GameServer functions
/// we encode the strings as base64 utf8. This just uses the .NET utf8 and base64 functions, but good
/// to be safe.
/// </summary>
internal class Base64Tests
{
	[Test]
	public void Null()
	{
		string input = null;
		string base64;
		bool encodeResult = ConvertEx.TryEncodeUtf8StringAsBase64(input, out base64);
		Assert.IsTrue(encodeResult, "successfully encoded");
		string utf8;
		bool decodeResult = ConvertEx.TryDecodeBase64AsUtf8String(base64, out utf8);
		Assert.IsTrue(decodeResult, "successfully decoded");
		Assert.IsTrue(string.IsNullOrEmpty(utf8));
	}

	[Test]
	public void Empty()
	{
		string input = string.Empty;
		string base64;
		bool encodeResult = ConvertEx.TryEncodeUtf8StringAsBase64(input, out base64);
		Assert.IsTrue(encodeResult, "successfully encoded");
		string utf8;
		bool decodeResult = ConvertEx.TryDecodeBase64AsUtf8String(base64, out utf8);
		Assert.IsTrue(decodeResult, "successfully decoded");
		Assert.IsTrue(string.IsNullOrEmpty(utf8));
	}

	[Test]
	public void AsciiText()
	{
		string input = "Hello, world!";
		string base64;
		bool encodeResult = ConvertEx.TryEncodeUtf8StringAsBase64(input, out base64);
		Assert.IsTrue(encodeResult, "successfully encoded");
		string utf8;
		bool decodeResult = ConvertEx.TryDecodeBase64AsUtf8String(base64, out utf8);
		Assert.IsTrue(decodeResult, "successfully decoded");
		Assert.AreEqual(input, utf8);
	}

	[Test]
	public void RussianText()
	{
		string input = "Привет, мир!";
		string base64;
		bool encodeResult = ConvertEx.TryEncodeUtf8StringAsBase64(input, out base64);
		Assert.IsTrue(encodeResult, "successfully encoded");
		string utf8;
		bool decodeResult = ConvertEx.TryDecodeBase64AsUtf8String(base64, out utf8);
		Assert.IsTrue(decodeResult, "successfully decoded");
		Assert.AreEqual(input, utf8);
	}

	[Test]
	public void ChineseText()
	{
		string input = "你好世界！";
		string base64;
		bool encodeResult = ConvertEx.TryEncodeUtf8StringAsBase64(input, out base64);
		Assert.IsTrue(encodeResult, "successfully encoded");
		string utf8;
		bool decodeResult = ConvertEx.TryDecodeBase64AsUtf8String(base64, out utf8);
		Assert.IsTrue(decodeResult, "successfully decoded");
		Assert.AreEqual(input, utf8);
	}

	[Test]
	public void DecodeInvalidString()
	{
		Assert.DoesNotThrow(() =>
		{
			string invalidBase64 = "你好世界";
			string utf8;
			bool decodeResult = ConvertEx.TryDecodeBase64AsUtf8String(invalidBase64, out utf8);
			Assert.IsFalse(decodeResult, "should fail to decode");
			Assert.IsNull(utf8);
		});
	}
}
