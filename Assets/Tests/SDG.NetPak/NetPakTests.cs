////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.NetPak;
using System.Collections.Generic;

internal class NetPakTests
{
	[Test]
	public void BasicTest()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBits(0b10101000, 8));
		Assert.IsTrue(writer.Flush());
		Assert.AreEqual(0b10101000, writer.buffer[0]);
	}

	[Test]
	public void ReadWrite()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBits(0xFEDCB, 20));
		Assert.IsTrue(writer.WriteBits(0xA9876, 20));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		uint a;
		reader.ReadBits(20, out a);
		uint b;
		reader.ReadBits(20, out b);
		Assert.AreEqual(0xFEDCB, a);
		Assert.AreEqual(0xA9876, b);
	}

	[Test]
	public void WriteBools()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBit(true));
		Assert.IsTrue(writer.WriteBit(false));
		Assert.IsTrue(writer.WriteBit(true));
		Assert.IsTrue(writer.Flush());
		Assert.AreEqual(0b101, writer.buffer[0]);
	}

	[Test]
	public void ReadWriteBools()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBit(true));
		Assert.IsTrue(writer.WriteBit(false));
		Assert.IsTrue(writer.WriteBit(true));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		bool tempValue;
		Assert.IsTrue(reader.ReadBit(out tempValue) && tempValue);
		Assert.IsTrue(reader.ReadBit(out tempValue) && !tempValue);
		Assert.IsTrue(reader.ReadBit(out tempValue) && tempValue);
	}

	[Test]
	public void WriteAlignToByte()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBits(0b101, 3));
		Assert.IsTrue(writer.WriteBits(0b11, 2));
		Assert.IsTrue(writer.AlignToByte());
		Assert.IsTrue(writer.WriteBits(0xFF, 7));
		Assert.IsTrue(writer.AlignToByte());
		Assert.IsTrue(writer.WriteBits(0b1, 1));
		Assert.IsTrue(writer.AlignToByte());
		Assert.IsTrue(writer.Flush());
		Assert.AreEqual(0b00011101, writer.buffer[0]);
		Assert.AreEqual(0b01111111, writer.buffer[1]);
		Assert.AreEqual(0b00000001, writer.buffer[2]);
	}

	[Test]
	public void ReadWriteArray()
	{
		byte[] input = new byte[6] { 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54 };

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBytes(input));

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		byte[] output = new byte[6];
		Assert.IsTrue(reader.ReadBytes(output, 6));
		AssertBufsEqual(input, output);
	}

	[Test]
	public void ReadWriteOffsetArray()
	{
		byte[] input = new byte[6] { 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54 };

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBits(0b10, 2));
		Assert.IsTrue(writer.WriteBytes(input));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		uint offset;
		Assert.IsTrue(reader.ReadBits(2, out offset));
		Assert.AreEqual(0b10, offset);

		byte[] output = new byte[6];
		Assert.IsTrue(reader.ReadBytes(output, 6));
		AssertBufsEqual(input, output);
	}

	[Test]
	public void ReadWriteEmptyArray()
	{
		byte[] input = new byte[0];

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBits(0b10, 2));
		Assert.IsTrue(writer.WriteBytes(input));
		Assert.IsTrue(writer.WriteBits(0b101, 3));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		uint prefix;
		Assert.IsTrue(reader.ReadBits(2, out prefix));
		Assert.AreEqual(0b10, prefix);

		byte[] output = new byte[0];
		Assert.IsTrue(reader.ReadBytes(output, 0));
		AssertBufsEqual(input, output);

		uint suffix;
		Assert.IsTrue(reader.ReadBits(3, out suffix));
		Assert.AreEqual(0b101, suffix);
	}

	[Test]
	public void ReadWriteOneByteArray()
	{
		byte[] input = new byte[1] { 0xFE };

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBits(0b10, 2));
		Assert.IsTrue(writer.WriteBytes(input));
		Assert.IsTrue(writer.WriteBits(0b101, 3));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		uint prefix;
		Assert.IsTrue(reader.ReadBits(2, out prefix));
		Assert.AreEqual(0b10, prefix);

		byte[] output = new byte[1];
		Assert.IsTrue(reader.ReadBytes(output, 1));
		AssertBufsEqual(input, output);

		uint suffix;
		Assert.IsTrue(reader.ReadBits(3, out suffix));
		Assert.AreEqual(0b101, suffix);
	}

	[Test]
	public void ReadWriteTwoByteArray()
	{
		byte[] input = new byte[2] { 0xFE, 0xDC };

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBits(0b10, 2));
		Assert.IsTrue(writer.WriteBytes(input));
		Assert.IsTrue(writer.WriteBits(0b101, 3));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		uint prefix;
		Assert.IsTrue(reader.ReadBits(2, out prefix));
		Assert.AreEqual(0b10, prefix);

		byte[] output = new byte[2];
		Assert.IsTrue(reader.ReadBytes(output, 2));
		AssertBufsEqual(input, output);

		uint suffix;
		Assert.IsTrue(reader.ReadBits(3, out suffix));
		Assert.AreEqual(0b101, suffix);
	}

	[Test]
	public void ReadWriteThreeByteArray()
	{
		byte[] input = new byte[3] { 0xFE, 0xDC, 0xBA };

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBits(0b10, 2));
		Assert.IsTrue(writer.WriteBytes(input));
		Assert.IsTrue(writer.WriteBits(0b101, 3));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		uint prefix;
		Assert.IsTrue(reader.ReadBits(2, out prefix));
		Assert.AreEqual(0b10, prefix);

		byte[] output = new byte[3];
		Assert.IsTrue(reader.ReadBytes(output, 3));
		AssertBufsEqual(input, output);

		uint suffix;
		Assert.IsTrue(reader.ReadBits(3, out suffix));
		Assert.AreEqual(0b101, suffix);
	}

	[Test]
	public void ReadWriteFourByteArray()
	{
		byte[] input = new byte[4] { 0xFE, 0xDC, 0xBA, 0x98 };

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBits(0b10, 2));
		Assert.IsTrue(writer.WriteBytes(input));
		Assert.IsTrue(writer.WriteBits(0b101, 3));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);

		uint prefix;
		Assert.IsTrue(reader.ReadBits(2, out prefix));
		Assert.AreEqual(0b10, prefix);

		byte[] output = new byte[4];
		Assert.IsTrue(reader.ReadBytes(output, 4));
		AssertBufsEqual(input, output);

		uint suffix;
		Assert.IsTrue(reader.ReadBits(3, out suffix));
		Assert.AreEqual(0b101, suffix);
	}

	private void AssertBufsEqual(byte[] expected, List<byte> actual)
	{
		Assert.AreEqual(expected.Length, actual.Count);
		for (int index = 0; index < expected.Length; ++index)
		{
			Assert.AreEqual(expected[index], actual[index]);
		}
	}

	private void AssertBufsEqual(byte[] expected, byte[] actual)
	{
		Assert.AreEqual(expected.Length, actual.Length);
		for (int index = 0; index < expected.Length; ++index)
		{
			Assert.AreEqual(expected[index], actual[index]);
		}
	}
}
