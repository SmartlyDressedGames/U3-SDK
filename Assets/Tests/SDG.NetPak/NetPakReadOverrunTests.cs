////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.NetPak;

internal class NetPakReadOverrunTests
{
	[Test]
	public void ReadBitFromEmptyBuffer()
	{
		TestDelegate code = () =>
		{
			NetPakReader reader = new NetPakReader();
			reader.SetBuffer(new byte[0]);
			bool value;
			Assert.IsFalse(reader.ReadBit(out value));
		};

#if WITH_NETPAK_EXCEPTIONS
		Assert.Throws<System.Exception>(code);
#else
		code();
#endif // WITH_NETPAK_EXCEPTIONS
	}

	[Test]
	public void ReadBitsWithinBuffer()
	{
		for (int bitCount = 1; bitCount <= 32; ++bitCount)
		{
			int bytesNeeded = ((bitCount - 1) / 8) + 1;
			NetPakReader reader = new NetPakReader();
			reader.SetBuffer(new byte[bytesNeeded]);
			uint value;
			Assert.IsTrue(reader.ReadBits(bitCount, out value));
		}
	}

	[Test]
	public void ReadBitsPastEndOfBuffer()
	{
		for (int byteCount = 1; byteCount <= 5; ++byteCount)
		{
			TestDelegate code = () =>
			{
				NetPakReader reader = new NetPakReader();
				reader.SetBuffer(new byte[byteCount]);
				for (int i = 0; i < byteCount; ++i)
				{
					uint value;
					Assert.IsTrue(reader.ReadBits(8, out value));
				}
				bool endBit;
				Assert.IsFalse(reader.ReadBit(out endBit));
			};

#if WITH_NETPAK_EXCEPTIONS
			Assert.Throws<System.Exception>(code);
#else
			code();
#endif // WITH_NETPAK_EXCEPTIONS
		}
	}

	[Test]
	public void ReadBytesWithinBuffer()
	{
		for (int byteCount = 1; byteCount <= 5; ++byteCount)
		{
			NetPakReader reader = new NetPakReader();
			reader.SetBuffer(new byte[byteCount]);
			Assert.IsTrue(reader.ReadBytes(new byte[byteCount], byteCount));
		}
	}

	[Test]
	public void ReadBytesPastEndOfBuffer()
	{
		for (int byteCount = 1; byteCount <= 5; ++byteCount)
		{
			TestDelegate code = () =>
			{
				NetPakReader reader = new NetPakReader();
				reader.SetBuffer(new byte[byteCount]);
				Assert.IsFalse(reader.ReadBytes(new byte[byteCount + 1], byteCount + 1));
			};

#if WITH_NETPAK_EXCEPTIONS
			Assert.Throws<System.Exception>(code);
#else
			code();
#endif // WITH_NETPAK_EXCEPTIONS
		}
	}

	[Test]
	public void ReadOffsetBytesWithinBuffer()
	{
		for (int byteCount = 1; byteCount <= 5; ++byteCount)
		{
			NetPakReader reader = new NetPakReader();
			reader.SetBuffer(new byte[byteCount + 1]);
			uint value;
			Assert.IsTrue(reader.ReadBits(7, out value));
			Assert.IsTrue(reader.ReadBytes(new byte[byteCount], byteCount));
		}
	}

	[Test]
	public void ReadOffsetBytesPastEndOfBuffer()
	{
		for (int byteCount = 1; byteCount <= 5; ++byteCount)
		{
			TestDelegate code = () =>
			{
				NetPakReader reader = new NetPakReader();
				reader.SetBuffer(new byte[byteCount + 1]);
				uint value;
				Assert.IsTrue(reader.ReadBits(9, out value));
				Assert.IsFalse(reader.ReadBytes(new byte[byteCount], byteCount));
			};

#if WITH_NETPAK_EXCEPTIONS
			Assert.Throws<System.Exception>(code);
#else
			code();
#endif // WITH_NETPAK_EXCEPTIONS
		}
	}
}
