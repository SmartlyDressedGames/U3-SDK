////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.NetPak;

/// <summary>
/// Credit to Markus Khun for the test file here under CC BY 4.0 license:
/// https://www.cl.cam.ac.uk/~mgk25/ucs/examples/UTF-8-test.txt
/// Several of these tests reference sections of the document.
/// </summary>
internal class StringNetPakTests
{
	[Test]
	public void ReadWriteNullString()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteString(null));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		string temp;
		Assert.IsTrue(reader.ReadString(out temp));
		Assert.IsTrue(string.IsNullOrEmpty(temp));
	}

	[Test]
	public void ReadWriteEmptyString()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteString(string.Empty));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		string temp;
		Assert.IsTrue(reader.ReadString(out temp));
		Assert.IsTrue(string.IsNullOrEmpty(temp));
	}

	[Test]
	public void ReadWriteNullPaddedString()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBits(0b0110, 4));
		Assert.IsTrue(writer.WriteString(null));
		Assert.IsTrue(writer.WriteBits(0b1001, 4));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		uint prefix;
		Assert.IsTrue(reader.ReadBits(4, out prefix));
		Assert.AreEqual(0b0110, prefix);
		string temp;
		Assert.IsTrue(reader.ReadString(out temp));
		Assert.IsTrue(string.IsNullOrEmpty(temp));
		uint suffix;
		Assert.IsTrue(reader.ReadBits(4, out suffix));
		Assert.AreEqual(0b1001, suffix);
	}

	[Test]
	public void ReadWriteEmptyPaddedString()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBits(0b0110, 4));
		Assert.IsTrue(writer.WriteString(string.Empty));
		Assert.IsTrue(writer.WriteBits(0b1001, 4));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		uint prefix;
		Assert.IsTrue(reader.ReadBits(4, out prefix));
		Assert.AreEqual(0b0110, prefix);
		string temp;
		Assert.IsTrue(reader.ReadString(out temp));
		Assert.IsTrue(string.IsNullOrEmpty(temp));
		uint suffix;
		Assert.IsTrue(reader.ReadBits(4, out suffix));
		Assert.AreEqual(0b1001, suffix);
	}

	[Test]
	public void ReadWriteAsciiString()
	{
		string expected = "Hello, world!";

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteString(expected));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		string actual;
		Assert.IsTrue(reader.ReadString(out actual));
		Assert.AreEqual(expected, actual);
	}

	[Test]
	public void ReadWritePaddedAsciiString()
	{
		string expected = "Hello, world!";

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBits(0b0110, 4));
		Assert.IsTrue(writer.WriteString(expected));
		Assert.IsTrue(writer.WriteBits(0b1001, 4));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		uint prefix;
		Assert.IsTrue(reader.ReadBits(4, out prefix));
		Assert.AreEqual(0b0110, prefix);
		string actual;
		Assert.IsTrue(reader.ReadString(out actual));
		Assert.AreEqual(expected, actual);
		uint suffix;
		Assert.IsTrue(reader.ReadBits(4, out suffix));
		Assert.AreEqual(0b1001, suffix);
	}

	[Test]
	public void ReadWriteUnicodeString()
	{
		string expected = "κόσμε";

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteString(expected));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		string actual;
		Assert.IsTrue(reader.ReadString(out actual));
		Assert.AreEqual(expected, actual);
	}

	[Test]
	public void ReadWritePaddedUnicodeString()
	{
		string expected = "κόσμε";

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBits(0b0110, 4));
		Assert.IsTrue(writer.WriteString(expected));
		Assert.IsTrue(writer.WriteBits(0b1001, 4));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		uint prefix;
		Assert.IsTrue(reader.ReadBits(4, out prefix));
		Assert.AreEqual(0b0110, prefix);
		string actual;
		Assert.IsTrue(reader.ReadString(out actual));
		Assert.AreEqual(expected, actual);
		uint suffix;
		Assert.IsTrue(reader.ReadBits(4, out suffix));
		Assert.AreEqual(0b1001, suffix);
	}

	[Test]
	public void ReadInvalidString()
	{
		byte[][] sequences = new byte[][]
		{
			new byte[] { 0xFE }, // 3.5.1
			new byte[] { 0xFF }, // 3.5.2
			new byte[] { 0xFE, 0xFE, 0xFF, 0xFF }, // 3.5.3

			// 4.1
			new byte[] { 0xC0, 0xAF }, // 4.1.1
			new byte[] { 0xE0, 0x80, 0xAF }, // 4.1.2
			new byte[] { 0xF0, 0x80, 0x80, 0xAF }, // 4.1.3
			new byte[] { 0xF8, 0x80, 0x80, 0x80, 0xAF }, // 4.1.4
			new byte[] { 0xFC, 0x80, 0x80, 0x80, 0x80, 0xAF }, // 4.1.5

			// 4.2
			new byte[] { 0xC1, 0xBF }, // 4.2.1
			new byte[] { 0xE0, 0x9F, 0xBF }, // 4.2.2
			new byte[] { 0xF0, 0x8f, 0xBF, 0xBF }, // 4.2.3
			new byte[] { 0xF8, 0x87, 0xBF, 0xBF, 0xBF }, // 4.2.4
			new byte[] { 0xFC, 0x83, 0xBF, 0xBF, 0xBF, 0xBF }, // 4.2.5

			// 4.3
			new byte[] { 0xC0, 0x80 }, // 4.3.1
			new byte[] { 0xE0, 0x80, 0x80 }, // 4.3.2
			new byte[] { 0xF0, 0x80, 0x80, 0x80 }, // 4.3.3
			new byte[] { 0xF8, 0x80, 0x80, 0x80, 0x80 }, // 4.3.4
			new byte[] { 0xFC, 0x80, 0x80, 0x80, 0x80, 0x80 }, // 4.3.5

			// 5.1
			new byte[] { 0xED, 0xA0, 0x80 }, // 5.1.1
			new byte[] { 0xED, 0xAD, 0xBF }, // 5.1.2
			new byte[] { 0xED, 0xAE, 0x80 }, // 5.1.3
			new byte[] { 0xED, 0xAF, 0xBF }, // 5.1.4
			new byte[] { 0xED, 0xB0, 0x80 }, // 5.1.5
			new byte[] { 0xED, 0xBE, 0x80 }, // 5.1.6
			new byte[] { 0xED, 0xBF, 0xBF }, // 5.1.7

			// 5.2
			new byte[] { 0xED, 0xA0, 0x80, 0xED, 0xB0, 0x80 }, // 5.2.1
			new byte[] { 0xED, 0xA0, 0x80, 0xED, 0xBF, 0xBF }, // 5.2.2
			new byte[] { 0xED, 0xAD, 0xBF, 0xED, 0xB0, 0x80 }, // 5.2.3
			new byte[] { 0xED, 0xAD, 0xBF, 0xED, 0xBF, 0xBF }, // 5.2.4
			new byte[] { 0xED, 0xAE, 0x80, 0xED, 0xB0, 0x80 }, // 5.2.5
			new byte[] { 0xED, 0xAE, 0x80, 0xED, 0xBF, 0xBF }, // 5.2.6
			new byte[] { 0xED, 0xAF, 0xBF, 0xED, 0xB0, 0x80 }, // 5.2.7
			new byte[] { 0xED, 0xAF, 0xBF, 0xED, 0xBF, 0xBF }, // 5.2.8
		};

		foreach (byte[] badSequence in sequences)
		{
			NetPakWriter writer = new NetPakWriter();
			writer.buffer = new byte[1024];
			Assert.IsTrue(writer.WriteBit(false)); // Not empty
			Assert.IsTrue(writer.WriteBits((uint) (badSequence.Length - 1), NetPakConst.MAX_STRING_BYTE_COUNT_BITS));
			Assert.IsTrue(writer.WriteBytes(badSequence));
			Assert.IsTrue(writer.Flush());

			NetPakReader reader = new NetPakReader();
			reader.SetBuffer(writer.buffer);
			string value;
			Assert.IsFalse(reader.ReadString(out value), "Value: {0}", value);
		}
	}

	[Test]
	public void WriteTruncatedAsciiString()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteString("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 3)); // 3 bits = max length of 8
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		string actual;
		Assert.IsTrue(reader.ReadString(out actual, 3));
		Assert.AreEqual("ABCDEFGH", actual);
	}

	/// <summary>
	/// String which cannot fit in buffer should be truncated.
	/// </summary>
	[Test]
	public void WriteStringLongerThanBuffer()
	{
		// Byte count is usually not the same as max length, but for this ascii test string it essentially is.
		int capacity = NetPakConst.MAX_STRING_BYTE_COUNT + 1;
		System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(capacity);
		for (int index = 0; index < capacity; ++index)
		{
			stringBuilder.Append('X');
		}
		string stringValue = stringBuilder.ToString();
		Assert.AreEqual(capacity, stringValue.Length);

		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteString(stringValue));
		Assert.IsTrue(writer.Flush());

		NetPakReader reader = new NetPakReader();
		reader.SetBuffer(writer.buffer);
		string actual;
		Assert.IsTrue(reader.ReadString(out actual));
		Assert.IsTrue(string.IsNullOrEmpty(actual));
	}
}
