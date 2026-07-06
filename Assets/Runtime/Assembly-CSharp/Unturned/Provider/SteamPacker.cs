////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System;

namespace SDG.Unturned
{
	public class SteamPacker
	{
		[System.Obsolete]
		public static Block block = new Block();

		[System.Obsolete]
		public static bool longBinaryData
		{
			get => luggageBlock.longBinaryData;

			set => luggageBlock.longBinaryData = value;
		}

		[System.Obsolete]
		public static object read(Type type)
		{
			return luggageBlock.read(type);
		}

		[System.Obsolete]
		public static object[] read(Type type_0, Type type_1, Type type_2)
		{
			return luggageBlock.read(type_0, type_1, type_2);
		}

		[System.Obsolete]
		public static object[] read(Type type_0, Type type_1, Type type_2, Type type_3)
		{
			return luggageBlock.read(type_0, type_1, type_2, type_3);
		}

		[System.Obsolete]
		public static object[] read(Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5)
		{
			return luggageBlock.read(type_0, type_1, type_2, type_3, type_4, type_5);
		}

		[System.Obsolete]
		public static object[] read(Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5, Type type_6)
		{
			return luggageBlock.read(type_0, type_1, type_2, type_3, type_4, type_5, type_6);
		}

		[System.Obsolete]
		public static object[] read(params Type[] types)
		{
			return luggageBlock.read(types);
		}

		[System.Obsolete]
		public static void openRead(int prefix, byte[] bytes)
		{
			openRead(prefix, bytes.Length, bytes);
		}

		[System.Obsolete]
		public static void openRead(int prefix, int size, byte[] bytes)
		{
			luggageBlock.resetForRead(prefix, bytes, size);
		}

		[System.Obsolete]
		public static void closeRead()
		{
			// does nothing 
		}

		[System.Obsolete]
		public static void write(object objects)
		{
			luggageBlock.write(objects);
		}

		[System.Obsolete]
		public static void write(object object_0, object object_1)
		{
			luggageBlock.write(object_0, object_1);
		}

		[System.Obsolete]
		public static void write(object object_0, object object_1, object object_2)
		{
			luggageBlock.write(object_0, object_1, object_2);
		}

		[System.Obsolete]
		public static void write(object object_0, object object_1, object object_2, object object_3)
		{
			luggageBlock.write(object_0, object_1, object_2, object_3);
		}

		[System.Obsolete]
		public static void write(object object_0, object object_1, object object_2, object object_3, object object_4, object object_5)
		{
			luggageBlock.write(object_0, object_1, object_2, object_3, object_4, object_5);
		}

		[System.Obsolete]
		public static void write(object object_0, object object_1, object object_2, object object_3, object object_4, object object_5, object object_6)
		{
			luggageBlock.write(object_0, object_1, object_2, object_3, object_4, object_5, object_6);
		}

		[System.Obsolete]
		public static void write(params object[] objects)
		{
			luggageBlock.write(objects);
		}

		[System.Obsolete]
		public static void openWrite(int prefix)
		{
			luggageBlock.resetForWrite(prefix);
		}

		[System.Obsolete]
		public static byte[] closeWrite(out int size)
		{
			return luggageBlock.getBytes(out size);
		}

		public static byte[] getBytes(int prefix, out int size, params object[] objects)
		{
			luggageBlock.resetForWrite(prefix);
			luggageBlock.write(objects);

			return luggageBlock.getBytes(out size);
		}

		[System.Obsolete]
		public static object[] getObjects(CSteamID steamID, int offset, int prefix, byte[] bytes, params Type[] types)
		{
			return getObjects(steamID, offset, prefix, bytes.Length, bytes, types);
		}

		[System.Obsolete]
		public static object[] getObjects(CSteamID steamID, int offset, int prefix, int size, byte[] bytes, params Type[] types)
		{
			luggageBlock.resetForRead(offset + prefix, bytes, size);

			if (types[0].GetElementType() == typeof(ClientInvocationContext))
			{
				object[] objects = luggageBlock.read(1, types);

#pragma warning disable
				ClientInvocationContext context = new ClientInvocationContext();
#pragma warning restore
				objects[0] = context;

				return objects;
			}
			else if (types[0].GetElementType() == typeof(ServerInvocationContext))
			{
				object[] objects = luggageBlock.read(1, types);

#pragma warning disable
				ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
#pragma warning restore
				objects[0] = context;

				return objects;
			}
			else
			{
				return luggageBlock.read(types);
			}
		}

		internal static object[] getObjectsForLegacyRPC(int offset, int prefix, int size, byte[] bytes, Type[] types, int typesOffset)
		{
			luggageBlock.resetForRead(offset + prefix, bytes, size);
			return luggageBlock.readForLegacyRPC(typesOffset, types);
		}

		/// <summary>
		/// Temporary replacement for static block member because plugins might depend on it.
		/// </summary>
		private static NetPakBlockImplementation luggageBlock = new NetPakBlockImplementation();
	}
}
