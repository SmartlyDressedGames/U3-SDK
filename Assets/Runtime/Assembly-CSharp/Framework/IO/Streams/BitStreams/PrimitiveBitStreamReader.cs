////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;

namespace SDG.Framework.IO.Streams.BitStreams
{
	public class PrimitiveBitStreamReader : BitStreamReader
	{
		public void readByte(ref byte data)
		{
			readBits(ref data, 8);
		}

		public void readInt16(ref short data)
		{
			byte a = 0;
			byte b = 0;
			readByte(ref a);
			readByte(ref b);

			data = (short) ((a << 8) | b);
		}

		public void readInt16(ref short data, byte length)
		{
			if (length == 16)
			{
				readInt16(ref data);
			}
			else if (length > 8)
			{
				byte a = 0;
				byte b = 0;

				readBits(ref a, (byte) (length - 8));
				readByte(ref b);

				data = (short) ((a << 8) | b);
			}
			else if (length == 8)
			{
				byte b = 0;
				readByte(ref b);

				data = b;
			}
			else
			{
				byte b = 0;
				readBits(ref b, length);

				data = b;
			}
		}

		public void readUInt16(ref ushort data)
		{
			byte a = 0;
			byte b = 0;
			readByte(ref a);
			readByte(ref b);

			data = (ushort) ((a << 8) | b);
		}

		public void readUInt16(ref ushort data, byte length)
		{
			if (length == 16)
			{
				readUInt16(ref data);
			}
			else if (length > 8)
			{
				byte a = 0;
				byte b = 0;

				readBits(ref a, (byte) (length - 8));
				readByte(ref b);

				data = (ushort) ((a << 8) | b);
			}
			else if (length == 8)
			{
				byte b = 0;
				readByte(ref b);

				data = b;
			}
			else
			{
				byte b = 0;
				readBits(ref b, length);

				data = b;
			}
		}

		public void readInt32(ref int data)
		{
			byte a = 0;
			byte b = 0;
			byte c = 0;
			byte d = 0;
			readByte(ref a);
			readByte(ref b);
			readByte(ref c);
			readByte(ref d);

			data = (a << 24) | (b << 16) | (c << 8) | d;
		}

		public void readInt32(ref int data, byte length)
		{
			if (length == 32)
			{
				readInt32(ref data);
			}
			else if (length > 24)
			{
				byte a = 0;
				byte b = 0;
				byte c = 0;
				byte d = 0;

				readBits(ref a, (byte) (length - 8));
				readByte(ref b);
				readByte(ref c);
				readByte(ref d);

				data = (a << 24) | (b << 16) | (c << 8) | d;
			}
			else if (length == 24)
			{
				byte b = 0;
				byte c = 0;
				byte d = 0;

				readByte(ref b);
				readByte(ref c);
				readByte(ref d);

				data = (b << 16) | (c << 8) | d;
			}
			else if (length > 16)
			{
				byte b = 0;
				byte c = 0;
				byte d = 0;

				readBits(ref b, (byte) (length - 8));
				readByte(ref c);
				readByte(ref d);

				data = (b << 16) | (c << 8) | d;
			}
			else if (length == 16)
			{
				byte c = 0;
				byte d = 0;

				readByte(ref c);
				readByte(ref d);

				data = (c << 8) | d;
			}
			else if (length > 8)
			{
				byte c = 0;
				byte d = 0;

				readBits(ref c, (byte) (length - 8));
				readByte(ref d);

				data = (c << 8) | d;
			}
			else if (length == 8)
			{
				byte d = 0;
				readByte(ref d);

				data = d;
			}
			else
			{
				byte d = 0;
				readBits(ref d, length);

				data = d;
			}
		}

		public PrimitiveBitStreamReader(Stream newStream) : base(newStream)
		{ }
	}
}
