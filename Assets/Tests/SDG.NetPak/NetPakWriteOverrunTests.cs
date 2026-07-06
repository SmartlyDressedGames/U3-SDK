////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.NetPak;

/// <summary>
/// Test that writing past end of buffer does not crash.
/// </summary>
internal class NetPakWriteOverrunTests
{
	[Test]
	public void WriteBitIntoEmptyBuffer()
	{
		TestDelegate code = () =>
		{
			NetPakWriter writer = new NetPakWriter();
			writer.buffer = new byte[0];
			bool result = writer.WriteBit(true);
			result &= writer.Flush();
			Assert.IsFalse(result);
		};

#if WITH_NETPAK_EXCEPTIONS
		Assert.Throws<System.Exception>(code);
#else
		code();
#endif // WITH_NETPAK_EXCEPTIONS
	}

	[Test]
	public void WriteUInt32IntoEmptyBuffer()
	{
		TestDelegate code = () =>
		{
			NetPakWriter writer = new NetPakWriter();
			writer.buffer = new byte[0];
			bool result = writer.WriteBits(0, 32);
			result &= writer.Flush();
			Assert.IsFalse(result);
		};

#if WITH_NETPAK_EXCEPTIONS
		Assert.Throws<System.Exception>(code);
#else
		code();
#endif // WITH_NETPAK_EXCEPTIONS
	}

	[Test]
	public void WriteByteArraysIntoEmptyBuffer()
	{
		for (int length = 1; length <= 5; ++length)
		{
			TestDelegate code = () =>
			{
				NetPakWriter writer = new NetPakWriter();
				writer.buffer = new byte[0];
				Assert.IsFalse(writer.WriteBytes(new byte[length]));
			};

#if WITH_NETPAK_EXCEPTIONS
			Assert.Throws<System.Exception>(code);
#else
			code();
#endif // WITH_NETPAK_EXCEPTIONS
		}
	}

	[Test]
	public void WriteBitIntoSingleByteBuffer()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1];
		bool result = writer.WriteBit(true);
		result &= writer.Flush();
		Assert.IsTrue(result);
	}

	[Test]
	public void WriteBitsWithinBuffer()
	{
		for (int bitCount = 1; bitCount <= 32; ++bitCount)
		{
			int bytesNeeded = ((bitCount - 1) / 8) + 1;
			NetPakWriter writer = new NetPakWriter();
			writer.buffer = new byte[bytesNeeded];
			bool result = writer.WriteBits(0, bitCount);
			result &= writer.Flush();
			Assert.IsTrue(result);
		}
	}

	[Test]
	public void WriteBitsPastEndOfBuffer()
	{
		for (int byteCount = 1; byteCount <= 5; ++byteCount)
		{
			TestDelegate code = () =>
			{
				NetPakWriter writer = new NetPakWriter();
				writer.buffer = new byte[byteCount];
				bool result = writer.WriteBit(true); // One more bit than available.
				for (int i = 0; i < byteCount; ++i)
					result &= writer.WriteBits(0, 8);
				result &= writer.Flush();
				Assert.IsFalse(result);
			};

#if WITH_NETPAK_EXCEPTIONS
			Assert.Throws<System.Exception>(code);
#else
			code();
#endif // WITH_NETPAK_EXCEPTIONS
		}
	}

	[Test]
	public void WriteBytesWithinBuffer()
	{
		for (int byteCount = 1; byteCount <= 5; ++byteCount)
		{
			NetPakWriter writer = new NetPakWriter();
			writer.buffer = new byte[byteCount];
			Assert.IsTrue(writer.WriteBytes(new byte[byteCount]));
		}
	}

	[Test]
	public void WriteBytesPastEndOfBuffer()
	{
		for (int byteCount = 1; byteCount <= 5; ++byteCount)
		{
			TestDelegate code = () =>
			{
				NetPakWriter writer = new NetPakWriter();
				writer.buffer = new byte[byteCount];
				Assert.IsFalse(writer.WriteBytes(new byte[byteCount + 1]));
			};

#if WITH_NETPAK_EXCEPTIONS
			Assert.Throws<System.Exception>(code);
#else
			code();
#endif // WITH_NETPAK_EXCEPTIONS
		}
	}
}
