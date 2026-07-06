////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using Steamworks;
using System;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Exposes the same API as the older Block class used by existing netcode, but implemented using new bit reader/writer. 
	/// </summary>
	internal class NetPakBlockImplementation
	{
		[System.Obsolete]
		public bool longBinaryData;

		public object read(Type type)
		{
			if (type == Types.STRING_TYPE)
			{
				string value;
				reader.ReadString(out value);
				return value;
			}
			else if (type == Types.STRING_ARRAY_TYPE)
			{
				byte length;
				reader.ReadUInt8(out length);
				string[] values = new string[length];
				for (int index = 0; index < values.Length; ++index)
				{
					reader.ReadString(out values[index]);
				}
				return values;
			}
			else if (type == Types.BOOLEAN_TYPE)
			{
				bool value;
				reader.ReadBit(out value);
				return value;
			}
			else if (type == Types.BOOLEAN_ARRAY_TYPE)
			{
				// Note this was only used by ResourceManager.tellResources, and is no longer necessary.

				ushort length;
				reader.ReadUInt16(out length);
				bool[] values = new bool[length];
				for (int index = 0; index < values.Length; ++index)
				{
					reader.ReadBit(out values[index]);
				}
				return values;
			}
			else if (type == Types.BYTE_TYPE)
			{
				byte value;
				reader.ReadUInt8(out value);
				return value;
			}
			else if (type == Types.BYTE_ARRAY_TYPE)
			{
				byte length;
				reader.ReadUInt8(out length);
				byte[] values = new byte[length];
				reader.ReadBytes(values);
				return values;
			}
			else if (type == Types.INT16_TYPE)
			{
				short value;
				reader.ReadInt16(out value);
				return value;
			}
			else if (type == Types.UINT16_TYPE)
			{
				ushort value;
				reader.ReadUInt16(out value);
				return value;
			}
			else if (type == Types.INT32_TYPE)
			{
				int value;
				reader.ReadInt32(out value);
				return value;
			}
			else if (type == Types.INT32_ARRAY_TYPE)
			{
				ushort length;
				reader.ReadUInt16(out length);
				int[] values = new int[length];
				for (int index = 0; index < values.Length; ++index)
				{
					reader.ReadInt32(out values[index]);
				}
				return values;
			}
			else if (type == Types.UINT32_TYPE)
			{
				uint value;
				reader.ReadUInt32(out value);
				return value;
			}
			else if (type == Types.SINGLE_TYPE)
			{
				float value;
				reader.ReadFloat(out value);
				return value;
			}
			else if (type == Types.INT64_TYPE)
			{
				long value;
				reader.ReadInt64(out value);
				return value;
			}
			else if (type == Types.UINT64_TYPE)
			{
				ulong value;
				reader.ReadUInt64(out value);
				return value;
			}
			else if (type == Types.UINT64_ARRAY_TYPE)
			{
				ushort length;
				reader.ReadUInt16(out length);
				ulong[] values = new ulong[length];
				for (int index = 0; index < values.Length; ++index)
				{
					reader.ReadUInt64(out values[index]);
				}
				return values;
			}
			else if (type == Types.STEAM_ID_TYPE)
			{
				CSteamID value;
				reader.ReadSteamID(out value);
				return value;
			}
			else if (type == Types.GUID_TYPE)
			{
				System.Guid value;
				reader.ReadGuid(out value);
				return value;
			}
			else if (type == Types.VECTOR3_TYPE)
			{
				Vector3 value;
				reader.ReadClampedVector3(out value, fracBitCount: 9); // Higher fracBitCount for plugins modifying barricades and structures.
				return value;
			}
			else if (type == Types.QUATERNION_TYPE)
			{
				Quaternion value;
				reader.ReadQuaternion(out value);
				return value;
			}
			else if (type == Types.COLOR_TYPE)
			{
				Color32 value;
				reader.ReadColor32RGB(out value);
				return (Color) value;
			}
			else if (type == typeof(NetId))
			{
				NetId value;
				reader.ReadNetId(out value);
				return value;
			}
			else if (type.IsEnum)
			{
				// Backwards compatibility for methods whose parameter changed from a byte index to netpak enum.
				byte index;
				reader.ReadUInt8(out index);
				return Enum.ToObject(type, index);
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

		public object[] readForLegacyRPC(int offset, Type[] types)
		{
			object[] objects = new object[types.Length];

			for (int index = offset; index < types.Length; index++)
			{
				objects[index] = read(types[index]);
			}

			return objects;
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

		public void write(object objects)
		{
			Type type = objects.GetType();

			if (type == Types.STRING_TYPE)
			{
				writer.WriteString((string) objects);
			}
			else if (type == Types.STRING_ARRAY_TYPE)
			{
				string[] values = (string[]) objects;
				byte length = (byte) values.Length;
				writer.WriteUInt8(length);
				for (int index = 0; index < length; ++index)
				{
					writer.WriteString(values[index]);
				}
			}
			else if (type == Types.BOOLEAN_TYPE)
			{
				writer.WriteBit((bool) objects);
			}
			else if (type == Types.BOOLEAN_ARRAY_TYPE)
			{
				// Note this was only used by ResourceManager.tellResources, and is no longer necessary.

				bool[] values = (bool[]) objects;
				ushort length = (ushort) values.Length;
				writer.WriteUInt16(length);
				for (int index = 0; index < length; ++index)
				{
					writer.WriteBit(values[index]);
				}
			}
			else if (type == Types.BYTE_TYPE)
			{
				writer.WriteUInt8((byte) objects);
			}
			else if (type == Types.BYTE_ARRAY_TYPE)
			{
				byte[] values = (byte[]) objects;
				byte length = (byte) values.Length;
				writer.WriteUInt8(length);
				writer.WriteBytes(values, length);
			}
			else if (type == Types.INT16_TYPE)
			{
				writer.WriteInt16((short) objects);
			}
			else if (type == Types.UINT16_TYPE)
			{
				writer.WriteUInt16((ushort) objects);
			}
			else if (type == Types.INT32_TYPE)
			{
				writer.WriteInt32((int) objects);
			}
			else if (type == Types.INT32_ARRAY_TYPE)
			{
				int[] values = (int[]) objects;
				ushort length = (ushort) values.Length;
				writer.WriteUInt16(length);
				for (int index = 0; index < length; ++index)
				{
					writer.WriteInt32(values[index]);
				}
			}
			else if (type == Types.UINT32_TYPE)
			{
				writer.WriteUInt32((uint) objects);
			}
			else if (type == Types.SINGLE_TYPE)
			{
				writer.WriteFloat((float) objects);
			}
			else if (type == Types.INT64_TYPE)
			{
				writer.WriteInt64((long) objects);
			}
			else if (type == Types.UINT64_TYPE)
			{
				writer.WriteUInt64((ulong) objects);
			}
			else if (type == Types.UINT64_ARRAY_TYPE)
			{
				ulong[] values = (ulong[]) objects;
				ushort length = (ushort) values.Length;
				writer.WriteUInt16(length);
				for (int index = 0; index < length; ++index)
				{
					writer.WriteUInt64(values[index]);
				}
			}
			else if (type == Types.STEAM_ID_TYPE)
			{
				writer.WriteSteamID((CSteamID) objects);
			}
			else if (type == Types.GUID_TYPE)
			{
				writer.WriteGuid((System.Guid) objects);
			}
			else if (type == Types.VECTOR3_TYPE)
			{
				writer.WriteClampedVector3((Vector3) objects, fracBitCount: 9); // Higher fracBitCount for plugins modifying barricades and structures.
			}
			else if (type == Types.QUATERNION_TYPE)
			{
				Quaternion value = (Quaternion) objects;
				writer.WriteQuaternion(value);
			}
			else if (type == Types.COLOR_TYPE)
			{
				Color color = (Color) objects;
				writer.WriteColor32RGB((Color32) color);
			}
			else if (type == typeof(NetId))
			{
				NetId netId = (NetId) objects;
				writer.WriteNetId(netId);
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

		public void resetForRead(int prefix, byte[] buffer, int size)
		{
			// RPCs e.g. askEquip may have sent less data than the server expects, so we cannot restrict to send length.
			// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2424#issuecomment-784233599
#if WITH_NETPAK_EXCEPTIONS || UNITY_EDITOR || DEVELOPMENT_BUILD
			reader.SetBufferSegment(buffer, size);
#else
			reader.SetBuffer(buffer);
#endif

			reader.Reset();
			reader.readByteIndex = prefix;
		}

		public void resetForWrite(int prefix)
		{
			writer.Reset();
			writer.writeByteIndex = prefix;
		}

		public byte[] getBytes(out int size)
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif
			writer.Flush();
			size = writer.writeByteIndex;
			return writer.buffer;
		}

		public NetPakBlockImplementation()
		{
			reader = new NetPakReader();
			reader.SetBuffer(Provider.buffer);
			writer = new NetPakWriter();
			writer.buffer = Block.buffer;
		}

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

		private NetPakReader reader;
		private NetPakWriter writer;
	}
}
