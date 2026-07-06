////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using SDG.NetPak;

internal class NetPakDeferReadTests
{
	/// <summary>
	/// Save state while already at the end of the buffer.
	/// </summary>
	[Test]
	public void ResumeEmpty()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteInt32(8310));
		writer.Flush();

		NetPakReader preDeferReader = new NetPakReader();
		preDeferReader.SetBufferSegment(writer.buffer, writer.writeByteIndex);
		int temp;
		Assert.IsTrue(preDeferReader.ReadInt32(out temp));
		Assert.AreEqual(8310, temp);
		uint savedScratch;
		int savedScratchBitCount;
		Assert.AreEqual(0, preDeferReader.RemainingSegmentLength);
		byte[] savedBuffer = new byte[0];
		Assert.IsTrue(preDeferReader.SaveState(out savedScratch, out savedScratchBitCount, savedBuffer));
		Assert.Less(savedBuffer.Length, writer.buffer.Length);
		Assert.AreNotSame(writer.buffer, savedBuffer);
		Assert.AreEqual(NetPakReader.EErrorFlags.None, preDeferReader.errors);
	}

	/// <summary>
	/// Save and then load state without any pre-defer reading.
	/// </summary>
	[Test]
	public void ResumeFromZero()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteInt32(8310));
		writer.Flush();

		NetPakReader preDeferReader = new NetPakReader();
		preDeferReader.SetBufferSegment(writer.buffer, writer.writeByteIndex);
		uint savedScratch;
		int savedScratchBitCount;
		byte[] savedBuffer = new byte[preDeferReader.RemainingSegmentLength];
		Assert.IsTrue(preDeferReader.SaveState(out savedScratch, out savedScratchBitCount, savedBuffer));
		Assert.Less(savedBuffer.Length, writer.buffer.Length);
		Assert.AreNotSame(writer.buffer, savedBuffer);
		Assert.AreEqual(NetPakReader.EErrorFlags.None, preDeferReader.errors);

		NetPakReader postDeferReader = new NetPakReader();
		postDeferReader.LoadState(savedScratch, savedScratchBitCount, savedBuffer, savedBuffer.Length);
		int temp;
		Assert.IsTrue(postDeferReader.ReadInt32(out temp));
		Assert.AreEqual(8310, temp);
		Assert.AreEqual(NetPakReader.EErrorFlags.None, postDeferReader.errors);
	}

	/// <summary>
	/// Pad written value with [1, 32] header bits before saving the state.
	/// </summary>
	[Test]
	public void ResumeWithPaddingBits()
	{
		for (int paddingBitCount = 1; paddingBitCount <= 32; ++paddingBitCount)
		{
			NetPakWriter writer = new NetPakWriter();
			writer.buffer = new byte[1024];
			Assert.IsTrue(writer.WriteBits(uint.MaxValue, paddingBitCount));
			Assert.IsTrue(writer.WriteInt32(8310));
			writer.Flush();

			NetPakReader preDeferReader = new NetPakReader();
			preDeferReader.SetBufferSegment(writer.buffer, writer.writeByteIndex);
			uint paddingBits;
			Assert.IsTrue(preDeferReader.ReadBits(paddingBitCount, out paddingBits));
			uint savedScratch;
			int savedScratchBitCount;
			byte[] savedBuffer = new byte[preDeferReader.RemainingSegmentLength];
			Assert.IsTrue(preDeferReader.SaveState(out savedScratch, out savedScratchBitCount, savedBuffer));
			Assert.Less(savedBuffer.Length, writer.buffer.Length);
			Assert.AreNotSame(writer.buffer, savedBuffer);
			Assert.AreEqual(NetPakReader.EErrorFlags.None, preDeferReader.errors);

			NetPakReader postDeferReader = new NetPakReader();
			postDeferReader.LoadState(savedScratch, savedScratchBitCount, savedBuffer, savedBuffer.Length);
			int temp;
			Assert.IsTrue(postDeferReader.ReadInt32(out temp));
			Assert.AreEqual(8310, temp);
			Assert.AreEqual(NetPakReader.EErrorFlags.None, postDeferReader.errors);
		}
	}

	/// <summary>
	/// Pad written value with [1, 16] header bytes before saving the state.
	/// </summary>
	[Test]
	public void ResumeWithPaddingBytes()
	{
		byte[] junk = new byte[16];
		for (int paddingByteCount = 1; paddingByteCount <= 16; ++paddingByteCount)
		{
			NetPakWriter writer = new NetPakWriter();
			writer.buffer = new byte[1024];
			Assert.IsTrue(writer.WriteBytes(junk, paddingByteCount));
			Assert.IsTrue(writer.WriteInt32(8310));
			writer.Flush();

			NetPakReader preDeferReader = new NetPakReader();
			preDeferReader.SetBufferSegment(writer.buffer, writer.writeByteIndex);
			Assert.IsTrue(preDeferReader.ReadBytes(junk, paddingByteCount));
			uint savedScratch;
			int savedScratchBitCount;
			byte[] savedBuffer = new byte[preDeferReader.RemainingSegmentLength];
			Assert.IsTrue(preDeferReader.SaveState(out savedScratch, out savedScratchBitCount, savedBuffer));
			Assert.Less(savedBuffer.Length, writer.buffer.Length);
			Assert.AreNotSame(writer.buffer, savedBuffer);
			Assert.AreEqual(NetPakReader.EErrorFlags.None, preDeferReader.errors);

			NetPakReader postDeferReader = new NetPakReader();
			postDeferReader.LoadState(savedScratch, savedScratchBitCount, savedBuffer, savedBuffer.Length);
			int temp;
			Assert.IsTrue(postDeferReader.ReadInt32(out temp));
			Assert.AreEqual(8310, temp);
			Assert.AreEqual(NetPakReader.EErrorFlags.None, postDeferReader.errors);
		}
	}

	[Test]
	public void ResumeString()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[1024];
		Assert.IsTrue(writer.WriteBits(uint.MaxValue, 23));
		Assert.IsTrue(writer.WriteString("Hello, world!"));
		writer.Flush();

		NetPakReader preDeferReader = new NetPakReader();
		preDeferReader.SetBufferSegment(writer.buffer, writer.writeByteIndex);
		uint headerBits;
		Assert.IsTrue(preDeferReader.ReadBits(23, out headerBits));
		uint savedScratch;
		int savedScratchBitCount;
		byte[] savedBuffer = new byte[preDeferReader.RemainingSegmentLength];
		Assert.IsTrue(preDeferReader.SaveState(out savedScratch, out savedScratchBitCount, savedBuffer));
		Assert.AreEqual(NetPakReader.EErrorFlags.None, preDeferReader.errors);

		Assert.AreNotSame(writer.buffer, savedBuffer);

		NetPakReader postDeferReader = new NetPakReader();
		postDeferReader.LoadState(savedScratch, savedScratchBitCount, savedBuffer, savedBuffer.Length);

		string temp;
		Assert.IsTrue(postDeferReader.ReadString(out temp));
		Assert.AreEqual("Hello, world!", temp);
	}

	[Test]
	public void ResumeWithOffset()
	{
		NetPakWriter writer = new NetPakWriter();
		writer.buffer = new byte[16];
		Assert.IsTrue(writer.WriteBits(0b1011, 4));
		Assert.IsTrue(writer.WriteInt16(short.MinValue));
		Assert.IsTrue(writer.WriteUInt64(0x1234567890ABCDEF));
		writer.Flush();
		Assert.AreEqual(writer.writeByteIndex, 11, "number of written bytes");

		NetPakReader preDeferReader = new NetPakReader();
		preDeferReader.SetBufferSegment(writer.buffer, writer.writeByteIndex);
		uint header;
		Assert.IsTrue(preDeferReader.ReadBits(4, out header));
		Assert.AreEqual(0b1011, header);
		uint savedScratch;
		int savedScratchBitCount;
		byte[] savedBuffer = new byte[preDeferReader.RemainingSegmentLength];
		Assert.IsTrue(preDeferReader.SaveState(out savedScratch, out savedScratchBitCount, savedBuffer));
		Assert.AreEqual(11 - 4, savedBuffer.Length); // Already read the first 4 bytes into scratch.
		Assert.AreEqual(32 - 4, savedScratchBitCount, "number of saved scratch bits");
		Assert.AreEqual(NetPakReader.EErrorFlags.None, preDeferReader.errors);

		Assert.AreNotSame(writer.buffer, savedBuffer);

		NetPakReader postDeferReader = new NetPakReader();
		postDeferReader.LoadState(savedScratch, savedScratchBitCount, savedBuffer, savedBuffer.Length);

		short x;
		Assert.IsTrue(postDeferReader.ReadInt16(out x));
		Assert.AreEqual(short.MinValue, x);

		ulong y;
		Assert.IsTrue(postDeferReader.ReadUInt64(out y));
		Assert.AreEqual(0x1234567890ABCDEF, y);
		Assert.AreEqual(NetPakReader.EErrorFlags.None, postDeferReader.errors);
	}
}
