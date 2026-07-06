////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;

namespace SDG.Framework.IO.Streams.BitStreams
{
	public class PrimitiveBitStreamWriter : BitStreamWriter
	{
		public void writeByte(byte data)
		{
			writeBits(data, 8);
		}

		public void writeInt16(short data)
		{
			writeByte((byte) (data >> 8));
			writeByte((byte) data);
		}

		public void writeInt16(short data, byte length)
		{
			if (length == 16)
			{
				writeInt16(data);
			}
			else if (length > 8)
			{
				writeBits((byte) (data >> 8), (byte) (length - 8));
				writeByte((byte) data);
			}
			else if (length == 8)
			{
				writeByte((byte) data);
			}
			else
			{
				writeBits((byte) data, length);
			}
		}

		public void writeUInt16(ushort data)
		{
			writeByte((byte) (data >> 8));
			writeByte((byte) data);
		}

		public void writeUInt16(ushort data, byte length)
		{
			if (length == 16)
			{
				writeUInt16(data);
			}
			else if (length > 8)
			{
				writeBits((byte) (data >> 8), (byte) (length - 8));
				writeByte((byte) data);
			}
			else if (length == 8)
			{
				writeByte((byte) data);
			}
			else
			{
				writeBits((byte) data, length);
			}
		}

		public void writeInt32(int data)
		{
			writeByte((byte) (data >> 24));
			writeByte((byte) (data >> 16));
			writeByte((byte) (data >> 8));
			writeByte((byte) data);
		}

		public void writeInt32(int data, byte length)
		{
			if (length == 32)
			{
				writeInt32(data);
			}
			else if (length > 24)
			{
				writeBits((byte) (data >> 24), (byte) (length - 8));
				writeByte((byte) (data >> 16));
				writeByte((byte) (data >> 8));
				writeByte((byte) data);
			}
			else if (length == 24)
			{
				writeByte((byte) (data >> 16));
				writeByte((byte) (data >> 8));
				writeByte((byte) data);
			}
			else if (length > 16)
			{
				writeBits((byte) (data >> 16), (byte) (length - 8));
				writeByte((byte) (data >> 8));
				writeByte((byte) data);
			}
			else if (length == 16)
			{
				writeByte((byte) (data >> 8));
				writeByte((byte) data);
			}
			else if (length > 8)
			{
				writeBits((byte) (data >> 8), (byte) (length - 8));
				writeByte((byte) data);
			}
			else if (length == 8)
			{
				writeByte((byte) data);
			}
			else
			{
				writeBits((byte) data, length);
			}
		}

		public PrimitiveBitStreamWriter(Stream newStream) : base(newStream)
		{ }
	}
}
