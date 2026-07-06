////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;

namespace SDG.Framework.IO.Streams.BitStreams
{
	// Made redundant by NetPakReader.
	public class BitStreamReader
	{
		public Stream stream
		{
			get;
			protected set;
		}

		private byte buffer
		{
			get;
			set;
		}

		private byte bitIndex
		{
			get;
			set;
		}

		private byte bitsAvailable
		{
			get;
			set;
		}

		public void readBit(ref byte data)
		{
			readBits(ref data, 1);
		}

		public void readBits(ref byte data, byte length)
		{
			if (bitIndex == 8 && bitsAvailable == 0)
			{
				fillBuffer();
			}

			if (length > bitsAvailable) // need to overflow into next data
			{
				byte cut = (byte) (length - bitsAvailable); // amount to read in second portion
				readBits(ref data, bitsAvailable); // read available space
				data <<= cut; // shift left to make space for next data
				readBits(ref data, cut); // read second portion
			}
			else
			{
				byte offset = (byte) (8 - length - bitIndex);
				byte mask = (byte) (0xFF >> (8 - length)); // mask for the last length bits
				data |= (byte) ((buffer >> offset) & mask);

				bitIndex += length;
				bitsAvailable -= length;
			}
		}

		private void fillBuffer()
		{
			buffer = (byte) stream.ReadByte();

			bitIndex = 0;
			bitsAvailable = 8;
		}

		public void reset()
		{

			buffer = 0;
			bitIndex = 8;
			bitsAvailable = 0;
		}

		public BitStreamReader(Stream newStream)
		{
			this.stream = newStream;

			reset();
		}
	}
}
