////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;

namespace SDG.Framework.IO.Streams
{
	public class NetworkStream
	{
		private Stream stream
		{
			get;
			set;
		}

		public sbyte readSByte()
		{
			sbyte data = (sbyte) stream.ReadByte();
			return data;
		}

		public byte readByte()
		{
			byte data = (byte) stream.ReadByte();
			return data;
		}

		public short readInt16()
		{
			byte b = readByte();
			byte a = readByte();

			short data = (short) ((b << 8) | a);
			return data;
		}

		public ushort readUInt16()
		{
			byte b = readByte();
			byte a = readByte();

			ushort data = (ushort) ((b << 8) | a);
			return data;
		}

		public int readInt32()
		{
			byte d = readByte();
			byte c = readByte();
			byte b = readByte();
			byte a = readByte();

			int data = (d << 24) | (c << 16) | (b << 8) | a;
			return data;
		}

		public uint readUInt32()
		{
			byte d = readByte();
			byte c = readByte();
			byte b = readByte();
			byte a = readByte();

			uint data = (uint) ((d << 24) | (c << 16) | (b << 8) | a);
			return data;
		}

		public long readInt64()
		{
			byte h = readByte();
			byte g = readByte();
			byte f = readByte();
			byte e = readByte();
			byte d = readByte();
			byte c = readByte();
			byte b = readByte();
			byte a = readByte();

			long data = (h << 56) | (g << 48) | (f << 40) | (e << 32) | (d << 24) | (c << 16) | (b << 8) | a;
			return data;
		}

		public ulong readUInt64()
		{
			byte h = readByte();
			byte g = readByte();
			byte f = readByte();
			byte e = readByte();
			byte d = readByte();
			byte c = readByte();
			byte b = readByte();
			byte a = readByte();

			ulong data = (ulong) ((h << 56) | (g << 48) | (f << 40) | (e << 32) | (d << 24) | (c << 16) | (b << 8) | a);
			return data;
		}

		public char readChar()
		{
			return (char) readUInt16();
		}

		public string readString()
		{
			ushort length = readUInt16();
			char[] characterArray = new char[length];

			for (ushort characterIndex = 0; characterIndex < length; characterIndex++)
			{
				char character = readChar();
				characterArray[characterIndex] = character;
			}

			string data = new string(characterArray);
			return data;
		}

		public void readBytes(byte[] data, ulong offset, ulong length)
		{
			stream.Read(data, (int) offset, (int) length);
		}

		public void writeSByte(sbyte data)
		{
			stream.WriteByte((byte) data);
		}

		public void writeByte(byte data)
		{
			stream.WriteByte(data);
		}

		public void writeInt16(short data)
		{
			writeByte((byte) (data >> 8));
			writeByte((byte) data);
		}

		public void writeUInt16(ushort data)
		{
			writeByte((byte) (data >> 8));
			writeByte((byte) data);
		}

		public void writeInt32(int data)
		{
			writeByte((byte) (data >> 24));
			writeByte((byte) (data >> 16));
			writeByte((byte) (data >> 8));
			writeByte((byte) data);
		}

		public void writeUInt32(uint data)
		{
			writeByte((byte) (data >> 24));
			writeByte((byte) (data >> 16));
			writeByte((byte) (data >> 8));
			writeByte((byte) data);
		}

		public void writeInt64(long data)
		{
			writeByte((byte) (data >> 56));
			writeByte((byte) (data >> 48));
			writeByte((byte) (data >> 40));
			writeByte((byte) (data >> 32));
			writeByte((byte) (data >> 24));
			writeByte((byte) (data >> 16));
			writeByte((byte) (data >> 8));
			writeByte((byte) data);
		}

		public void writeUInt64(ulong data)
		{
			writeByte((byte) (data >> 56));
			writeByte((byte) (data >> 48));
			writeByte((byte) (data >> 40));
			writeByte((byte) (data >> 32));
			writeByte((byte) (data >> 24));
			writeByte((byte) (data >> 16));
			writeByte((byte) (data >> 8));
			writeByte((byte) data);
		}

		public void writeChar(char data)
		{
			writeUInt16(data);
		}

		public void writeString(string data)
		{
			ushort length = (ushort) data.Length;
			char[] characterArray = data.ToCharArray();

			writeUInt16(length);
			for (ushort characterIndex = 0; characterIndex < length; characterIndex++)
			{
				char character = characterArray[characterIndex];
				writeChar(character);
			}
		}

		public void writeBytes(byte[] data, ulong offset, ulong length)
		{
			stream.Write(data, (int) offset, (int) length);
		}

		public NetworkStream(Stream newStream)
		{
			this.stream = newStream;
		}
	}
}
