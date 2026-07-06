////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System;
using System.Text;
using UnityEngine;

namespace SDG.Unturned
{
	public class Block
	{
		public static readonly int BUFFER_SIZE = 65535;
		public static byte[] buffer = new byte[BUFFER_SIZE];

		private static object[][] objects = new object[][]
		{
			new object[1],
			new object[2],
			new object[3],
			new object[4],
			new object[5],
			new object[6],
			new object[7]
		};

		private static object[] getObjects(int index)
		{
			object[] list = objects[index];
			for (int i = 0; i < list.Length; i++)
			{
				list[i] = null;
			}

			return list;
		}

		public bool longBinaryData;

		public int step;
		public byte[] block;

		public string readString()
		{
			if (block != null && step < block.Length)
			{
				byte length = block[step];
				++step;

				string value;
				if (step + length <= block.Length)
				{
					value = Encoding.UTF8.GetString(block, step, length);
				}
				else
				{
					value = string.Empty;
				}
				step += length;

				return value;
			}
			else
			{
				return string.Empty;
			}
		}

		public string[] readStringArray()
		{
			if (block != null && step < block.Length)
			{
				string[] values = new string[readByte()];
				for (byte index = 0; index < values.Length; index++)
				{
					values[index] = readString();
				}

				return values;
			}
			else
			{
				return new string[0];
			}
		}

		public bool readBoolean()
		{
			if (block != null && step <= block.Length - 1)
			{
				bool value = BitConverter.ToBoolean(block, step);
				step++;

				return value;
			}
			else
			{
				return false;
			}
		}

		public bool[] readBooleanArray()
		{
			if (block != null && step < block.Length)
			{
				bool[] values = new bool[readUInt16()];
				ushort chunks = (ushort) Mathf.CeilToInt(values.Length / 8f);

				for (ushort index = 0; index < chunks; index++)
				{
					for (byte offset = 0; offset < 8; offset++)
					{
						if ((index * 8) + offset >= values.Length)
						{
							break;
						}

						values[(index * 8) + offset] = (block[step + index] & Types.SHIFTS[offset]) == Types.SHIFTS[offset];
					}
				}

				step += chunks;
				return values;
			}
			else
			{
				return new bool[0];
			}
		}

		public byte readByte()
		{
			if (block != null && step <= block.Length - 1)
			{
				byte value = block[step];
				step++;

				return value;
			}
			else
			{
				return 0;
			}
		}

		public byte[] readByteArray()
		{
			if (block != null && step < block.Length)
			{
				byte[] values;

				if (longBinaryData)
				{
					int count = readInt32();
					if (count < 30000)
					{
						values = new byte[count];
					}
					else
					{
						return new byte[0];
					}
				}
				else
				{
					byte count = block[step];
					values = new byte[count];
					step++;
				}

				if (step + values.Length <= block.Length)
				{
					try
					{
						Buffer.BlockCopy(block, step, values, 0, values.Length);
					}
					catch
					{
						// data is corrupted?
					}
				}

				step += values.Length;

				return values;
			}
			else
			{
				return new byte[0];
			}
		}

		public short readInt16()
		{
			if (block != null && step <= block.Length - 2)
			{
				readBitConverterBytes(2);
				short value = BitConverter.ToInt16(block, step);
				step += 2;

				return value;
			}
			else
			{
				return 0;
			}
		}

		public ushort readUInt16()
		{
			if (block != null && step <= block.Length - 2)
			{
				readBitConverterBytes(2);
				ushort value = BitConverter.ToUInt16(block, step);
				step += 2;

				return value;
			}
			else
			{
				return 0;
			}
		}

		public int readInt32()
		{
			if (block != null && step <= block.Length - 4)
			{
				readBitConverterBytes(4);
				int value = BitConverter.ToInt32(block, step);
				step += 4;

				return value;
			}
			else
			{
				return 0;
			}
		}

		public int[] readInt32Array()
		{
			ushort count = readUInt16();
			int[] values = new int[count];

			for (ushort index = 0; index < count; index++)
			{
				int value = readInt32();
				values[index] = value;
			}

			return values;
		}

		public uint readUInt32()
		{
			if (block != null && step <= block.Length - 4)
			{
				readBitConverterBytes(4);
				uint value = BitConverter.ToUInt32(block, step);
				step += 4;

				return value;
			}
			else
			{
				return 0;
			}
		}

		public float readSingle()
		{
			if (block != null && step <= block.Length - 4)
			{
				readBitConverterBytes(4);
				float value = BitConverter.ToSingle(block, step);
				step += 4;

				return value;
			}
			else
			{
				return 0;
			}
		}

		public long readInt64()
		{
			if (block != null && step <= block.Length - 8)
			{
				readBitConverterBytes(8);
				long value = BitConverter.ToInt64(block, step);
				step += 8;

				return value;
			}
			else
			{
				return 0;
			}
		}

		public ulong readUInt64()
		{
			if (block != null && step <= block.Length - 8)
			{
				readBitConverterBytes(8);
				ulong value = BitConverter.ToUInt64(block, step);
				step += 8;

				return value;
			}
			else
			{
				return 0;
			}
		}

		public ulong[] readUInt64Array()
		{
			ushort count = readUInt16();
			ulong[] values = new ulong[count];

			for (ushort index = 0; index < count; index++)
			{
				ulong value = readUInt64();
				values[index] = value;
			}

			return values;
		}

		public CSteamID readSteamID()
		{
			return new CSteamID(readUInt64());
		}

		public Guid readGUID()
		{
			GuidBuffer buffer = new GuidBuffer();
			buffer.Read(readByteArray(), 0);
			return buffer.GUID;
		}

		public Vector3 readUInt16RVector3()
		{
			byte xR = readByte();
			double x = readUInt16() / (double) ushort.MaxValue;
			double y = readUInt16() / (double) ushort.MaxValue;
			byte zR = readByte();
			double z = readUInt16() / (double) ushort.MaxValue;

			x = (xR * Regions.REGION_SIZE) + (x * Regions.REGION_SIZE) - 4096.0;
			y = (y * 2048.0) - 1024.0; // = y * Level.HEIGHT;
			z = (zR * Regions.REGION_SIZE) + (z * Regions.REGION_SIZE) - 4096.0;

			return new Vector3((float) x, (float) y, (float) z);
		}

		public Vector3 readSingleVector3()
		{
			return new Vector3(readSingle(), readSingle(), readSingle());
		}

		public Quaternion readSingleQuaternion()
		{
			return Quaternion.Euler(readSingle(), readSingle(), readSingle());
		}

		public Color readColor()
		{
			return new Color(readByte() / 255f, readByte() / 255f, readByte() / 255f);
		}

		public object read(Type type)
		{
			if (type == Types.STRING_TYPE)
			{
				return readString();
			}
			else if (type == Types.STRING_ARRAY_TYPE)
			{
				return readStringArray();
			}
			else if (type == Types.BOOLEAN_TYPE)
			{
				return readBoolean();
			}
			else if (type == Types.BOOLEAN_ARRAY_TYPE)
			{
				return readBooleanArray();
			}
			else if (type == Types.BYTE_TYPE)
			{
				return readByte();
			}
			else if (type == Types.BYTE_ARRAY_TYPE)
			{
				return readByteArray();
			}
			else if (type == Types.INT16_TYPE)
			{
				return readInt16();
			}
			else if (type == Types.UINT16_TYPE)
			{
				return readUInt16();
			}
			else if (type == Types.INT32_TYPE)
			{
				return readInt32();
			}
			else if (type == Types.INT32_ARRAY_TYPE)
			{
				return readInt32Array();
			}
			else if (type == Types.UINT32_TYPE)
			{
				return readUInt32();
			}
			else if (type == Types.SINGLE_TYPE)
			{
				return readSingle();
			}
			else if (type == Types.INT64_TYPE)
			{
				return readInt64();
			}
			else if (type == Types.UINT64_TYPE)
			{
				return readUInt64();
			}
			else if (type == Types.UINT64_ARRAY_TYPE)
			{
				return readUInt64Array();
			}
			else if (type == Types.STEAM_ID_TYPE)
			{
				return readSteamID();
			}
			else if (type == Types.GUID_TYPE)
			{
				return readGUID();
			}
			else if (type == Types.VECTOR3_TYPE)
			{
				return readSingleVector3();
			}
			else if (type == Types.COLOR_TYPE)
			{
				return readColor();
			}
			else
			{
				throw new System.NotSupportedException(string.Format("Cannot read type {0}", type));
			}
		}

		public object[] read(int offset, Type type_0)
		{
			object[] list = getObjects(0);
			if (offset < 1)
			{
				list[0] = read(type_0);
			}

			return list;
		}

		public object[] read(int offset, Type type_0, Type type_1)
		{
			object[] list = getObjects(1);
			if (offset < 1)
			{
				list[0] = read(type_0);
			}
			if (offset < 2)
			{
				list[1] = read(type_1);
			}

			return list;
		}

		public object[] read(Type type_0, Type type_1)
		{
			return read(0, type_0, type_1);
		}

		public object[] read(int offset, Type type_0, Type type_1, Type type_2)
		{
			object[] list = getObjects(2);
			if (offset < 1)
			{
				list[0] = read(type_0);
			}
			if (offset < 2)
			{
				list[1] = read(type_1);
			}
			if (offset < 3)
			{
				list[2] = read(type_2);
			}

			return list;
		}

		public object[] read(Type type_0, Type type_1, Type type_2)
		{
			return read(0, type_0, type_1, type_2);
		}

		public object[] read(int offset, Type type_0, Type type_1, Type type_2, Type type_3)
		{
			object[] list = getObjects(3);
			if (offset < 1)
			{
				list[0] = read(type_0);
			}
			if (offset < 2)
			{
				list[1] = read(type_1);
			}
			if (offset < 3)
			{
				list[2] = read(type_2);
			}
			if (offset < 4)
			{
				list[3] = read(type_3);
			}

			return list;
		}

		public object[] read(Type type_0, Type type_1, Type type_2, Type type_3)
		{
			return read(0, type_0, type_1, type_2, type_3);
		}

		public object[] read(int offset, Type type_0, Type type_1, Type type_2, Type type_3, Type type_4)
		{
			object[] list = getObjects(4);
			if (offset < 1)
			{
				list[0] = read(type_0);
			}
			if (offset < 2)
			{
				list[1] = read(type_1);
			}
			if (offset < 3)
			{
				list[2] = read(type_2);
			}
			if (offset < 4)
			{
				list[3] = read(type_3);
			}
			if (offset < 5)
			{
				list[4] = read(type_4);
			}

			return list;
		}

		public object[] read(Type type_0, Type type_1, Type type_2, Type type_3, Type type_4)
		{
			return read(0, type_0, type_1, type_2, type_3, type_4);
		}

		public object[] read(int offset, Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5)
		{
			object[] list = getObjects(5);
			if (offset < 1)
			{
				list[0] = read(type_0);
			}
			if (offset < 2)
			{
				list[1] = read(type_1);
			}
			if (offset < 3)
			{
				list[2] = read(type_2);
			}
			if (offset < 4)
			{
				list[3] = read(type_3);
			}
			if (offset < 5)
			{
				list[4] = read(type_4);
			}
			if (offset < 6)
			{
				list[5] = read(type_5);
			}

			return list;
		}

		public object[] read(Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5)
		{
			return read(0, type_0, type_1, type_2, type_3, type_4, type_5);
		}

		public object[] read(int offset, Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5, Type type_6)
		{
			object[] list = getObjects(6);
			if (offset < 1)
			{
				list[0] = read(type_0);
			}
			if (offset < 2)
			{
				list[1] = read(type_1);
			}
			if (offset < 3)
			{
				list[2] = read(type_2);
			}
			if (offset < 4)
			{
				list[3] = read(type_3);
			}
			if (offset < 5)
			{
				list[4] = read(type_4);
			}
			if (offset < 6)
			{
				list[5] = read(type_5);
			}
			if (offset < 7)
			{
				list[6] = read(type_6);
			}

			return list;
		}

		public object[] read(Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5, Type type_6)
		{
			return read(0, type_0, type_1, type_2, type_3, type_4, type_5, type_6);
		}

		public object[] read(int offset, params Type[] types)
		{
			object[] objects = new object[types.Length];

			for (int index = offset; index < types.Length; index++)
			{
				objects[index] = read(types[index]);
			}

			return objects;
		}

		public object[] read(params Type[] types)
		{
			return read(0, types);
		}

		protected void readBitConverterBytes(int length)
		{
			// Prior to Luggage the NetBlock implementation reversed byte order on BigEndian systems.
		}

		protected void writeBitConverterBytes(byte[] bytes)
		{
			// Prior to Luggage the NetBlock implementation reversed byte order on BigEndian systems.
			const int srcOffset = 0;
			int dstOffset = step;
			int count = bytes.Length;
			Buffer.BlockCopy(bytes, srcOffset, buffer, dstOffset, count);
		}

		public void writeString(string value)
		{
			// Nelson 2024-04-30: Looks like UTF8.GetBytes throws an exception if input string is null, so I'm going
			// through and ensuring we never pass null to it.
			if (value == null)
			{
				value = string.Empty;
			}

			byte[] bytes = Encoding.UTF8.GetBytes(value);
			byte bytesCount = (byte) bytes.Length;
			buffer[step] = bytesCount;
			step++;

			Buffer.BlockCopy(bytes, 0, buffer, step, bytesCount);
			step += bytesCount;
		}

		public void writeStringArray(string[] values)
		{
			byte valuesCount = (byte) values.Length;
			writeByte(valuesCount);
			for (byte index = 0; index < valuesCount; index++)
			{
				writeString(values[index]);
			}
		}

		public void writeBoolean(bool value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			buffer[step] = bytes[0];

			step++;
		}

		public void writeBooleanArray(bool[] values)
		{
			writeUInt16((ushort) values.Length);
			ushort chunks = (ushort) Mathf.CeilToInt(values.Length / 8f);

			for (ushort index = 0; index < chunks; index++)
			{
				buffer[step + index] = 0;
				for (byte offset = 0; offset < 8; offset++)
				{
					if ((index * 8) + offset >= values.Length)
					{
						break;
					}

					if (values[(index * 8) + offset])
					{
						buffer[step + index] |= Types.SHIFTS[offset];
					}
				}
			}

			step += chunks;
		}

		public void writeByte(byte value)
		{
			buffer[step] = value;

			step++;
		}

		public void writeByteArray(byte[] values)
		{
			if (values.Length >= 30000)
				return;

			if (longBinaryData)
			{
				writeInt32(values.Length);

				Buffer.BlockCopy(values, 0, buffer, step, values.Length);
				step += values.Length;
			}
			else
			{
				byte valuesCount = (byte) values.Length;
				buffer[step] = valuesCount;
				step++;

				Buffer.BlockCopy(values, 0, buffer, step, valuesCount);
				step += valuesCount;
			}
		}

		public void writeInt16(short value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			writeBitConverterBytes(bytes);

			step += 2;
		}

		public void writeUInt16(ushort value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			writeBitConverterBytes(bytes);

			step += 2;
		}

		public void writeInt32(int value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			writeBitConverterBytes(bytes);

			step += 4;
		}

		public void writeInt32Array(int[] values)
		{
			writeUInt16((ushort) values.Length);
			for (ushort index = 0; index < values.Length; index++)
			{
				writeInt32(values[index]);
			}
		}

		public void writeUInt32(uint value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			writeBitConverterBytes(bytes);

			step += 4;
		}

		public void writeSingle(float value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			writeBitConverterBytes(bytes);

			step += 4;
		}

		public void writeInt64(long value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			writeBitConverterBytes(bytes);

			step += 8;
		}

		public void writeUInt64(ulong value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			writeBitConverterBytes(bytes);

			step += 8;
		}

		public void writeUInt64Array(ulong[] values)
		{
			writeUInt16((ushort) values.Length);
			for (ushort index = 0; index < values.Length; index++)
			{
				writeUInt64(values[index]);
			}
		}

		public void writeSteamID(CSteamID steamID)
		{
			writeUInt64(steamID.m_SteamID);
		}

		public void writeGUID(Guid GUID)
		{
			GuidBuffer buffer = new GuidBuffer(GUID);
			buffer.Write(GuidBuffer.GUID_BUFFER, 0);
			writeByteArray(GuidBuffer.GUID_BUFFER);
		}

		public void writeUInt16RVector3(Vector3 value)
		{
			double x = value.x + 4096.0;
			double y = value.y + 1024.0;
			double z = value.z + 4096.0;

			byte xR = (byte) (x / Regions.REGION_SIZE);
			byte zR = (byte) (z / Regions.REGION_SIZE);

			x %= Regions.REGION_SIZE;
			y %= 2048;
			z %= Regions.REGION_SIZE;

			x /= Regions.REGION_SIZE;
			y /= 2048.0;
			z /= Regions.REGION_SIZE;

			writeByte(xR);
			writeUInt16((ushort) (x * ushort.MaxValue));
			writeUInt16((ushort) (y * ushort.MaxValue));
			writeByte(zR);
			writeUInt16((ushort) (z * ushort.MaxValue));
		}

		public void writeSingleVector3(Vector3 value)
		{
			writeSingle(value.x);
			writeSingle(value.y);
			writeSingle(value.z);
		}

		public void writeSingleQuaternion(Quaternion value)
		{
			Vector3 angles = value.eulerAngles;

			writeSingle(angles.x);
			writeSingle(angles.y);
			writeSingle(angles.z);
		}

		public void writeColor(Color value)
		{
			writeByte((byte) (value.r * 255));
			writeByte((byte) (value.g * 255));
			writeByte((byte) (value.b * 255));
		}

		public void write(object objects)
		{
			Type type = objects.GetType();

			if (type == Types.STRING_TYPE)
			{
				writeString((string) objects);
			}
			else if (type == Types.STRING_ARRAY_TYPE)
			{
				writeStringArray((string[]) objects);
			}
			else if (type == Types.BOOLEAN_TYPE)
			{
				writeBoolean((bool) objects);
			}
			else if (type == Types.BOOLEAN_ARRAY_TYPE)
			{
				writeBooleanArray((bool[]) objects);
			}
			else if (type == Types.BYTE_TYPE)
			{
				writeByte((byte) objects);
			}
			else if (type == Types.BYTE_ARRAY_TYPE)
			{
				writeByteArray((byte[]) objects);
			}
			else if (type == Types.INT16_TYPE)
			{
				writeInt16((short) objects);
			}
			else if (type == Types.UINT16_TYPE)
			{
				writeUInt16((ushort) objects);
			}
			else if (type == Types.INT32_TYPE)
			{
				writeInt32((int) objects);
			}
			else if (type == Types.INT32_ARRAY_TYPE)
			{
				writeInt32Array((int[]) objects);
			}
			else if (type == Types.UINT32_TYPE)
			{
				writeUInt32((uint) objects);
			}
			else if (type == Types.SINGLE_TYPE)
			{
				writeSingle((float) objects);
			}
			else if (type == Types.INT64_TYPE)
			{
				writeInt64((long) objects);
			}
			else if (type == Types.UINT64_TYPE)
			{
				writeUInt64((ulong) objects);
			}
			else if (type == Types.UINT64_ARRAY_TYPE)
			{
				writeUInt64Array((ulong[]) objects);
			}
			else if (type == Types.STEAM_ID_TYPE)
			{
				writeSteamID((CSteamID) objects);
			}
			else if (type == Types.GUID_TYPE)
			{
				writeGUID((Guid) objects);
			}
			else if (type == Types.VECTOR3_TYPE)
			{
				writeSingleVector3((Vector3) objects);
			}
			else if (type == Types.COLOR_TYPE)
			{
				writeColor((Color) objects);
			}
			else
			{
				throw new System.NotSupportedException(string.Format("Cannot write {0} of type {1}", objects, type));
			}
		}

		public void write(object object_0, object object_1)
		{
			write(object_0);
			write(object_1);
		}

		public void write(object object_0, object object_1, object object_2)
		{
			write(object_0, object_1);
			write(object_2);
		}

		public void write(object object_0, object object_1, object object_2, object object_3)
		{
			write(object_0, object_1, object_2);
			write(object_3);
		}

		public void write(object object_0, object object_1, object object_2, object object_3, object object_4)
		{
			write(object_0, object_1, object_2, object_3);
			write(object_4);
		}

		public void write(object object_0, object object_1, object object_2, object object_3, object object_4, object object_5)
		{
			write(object_0, object_1, object_2, object_3, object_4);
			write(object_5);
		}

		public void write(object object_0, object object_1, object object_2, object object_3, object object_4, object object_5, object object_6)
		{
			write(object_0, object_1, object_2, object_3, object_4, object_5);
			write(object_6);
		}

		public void write(params object[] objects)
		{
			for (int index = 0; index < objects.Length; index++)
			{
				write(objects[index]);
			}
		}

		public byte[] getBytes(out int size)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif

			if (block == null)
			{
				size = step;
				return buffer;
			}
			else
			{
				size = block.Length;
				return block;
			}
		}

		public byte[] getHash()
		{
			if (block == null)
			{
				return Hash.SHA1(buffer);
			}
			else
			{
				return Hash.SHA1(block);
			}
		}

		public void reset(int prefix, byte[] contents)
		{
			step = prefix;
			block = contents;
		}

		public void reset(byte[] contents)
		{
			step = 0;
			block = contents;
		}

		public void reset(int prefix)
		{
			step = prefix;
			block = null;
		}

		public void reset()
		{
			step = 0;
			block = null;
		}

		public Block(int prefix, byte[] contents)
		{
			reset(prefix, contents);
		}

		public Block(byte[] contents)
		{
			reset(contents);
		}

		public Block(int prefix)
		{
			reset(prefix);
		}

		public Block()
		{
			reset();
		}
	}
}
