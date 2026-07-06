////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;

namespace SDG.Framework.IO.Streams.BitStreams
{
	// Made redundant by NetPakWriter.
	public class BitStreamWriter
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

		public void writeBit(byte data)
		{
			writeBits(data, 1);
		}

		public void writeBits(byte data, byte length)
		{
			if (length > bitsAvailable) // need to overflow into next data
			{
				byte cut = (byte) (length - bitsAvailable);
				writeBits((byte) (data >> cut), bitsAvailable); // write portion that fits into this part
				writeBits(data, cut); // write portion that fits into this part
			}
			else
			{
				byte offset = (byte) (8 - length - bitIndex);
				byte mask = (byte) (0xFF >> (8 - length)); // mask for the last length bits
				buffer |= (byte) ((data & mask) << offset); // append masked portion of data

				bitIndex += length;
				bitsAvailable -= length;

				if (bitIndex == 8 && bitsAvailable == 0)
				{
					emptyBuffer();
				}
			}
		}

		private void emptyBuffer()
		{
			stream.WriteByte(buffer);

			reset();
		}

		public void flush()
		{
			if (bitsAvailable == 8)
			{
				return;
			}

			emptyBuffer();
		}

		public void reset()
		{
			buffer = 0;
			bitIndex = 0;
			bitsAvailable = 8;
		}

		public BitStreamWriter(Stream newStream)
		{
			this.stream = newStream;

			reset();
		}
	}
}
