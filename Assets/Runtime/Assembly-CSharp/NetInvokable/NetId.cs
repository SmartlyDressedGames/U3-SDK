////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned
{
	public struct NetId : System.IEquatable<NetId>
	{
		public static readonly NetId INVALID = new NetId(0);

		public NetId(uint id)
		{
			this.id = id;
		}

		/// <summary>
		/// Zero is treated as unset.
		/// </summary>
		public bool IsNull()
		{
			return id == 0U;
		}

		public void Clear()
		{
			id = 0;
		}

		public override bool Equals(object obj)
		{
			return obj is NetId && id == ((NetId) obj).id;
		}

		public bool Equals(NetId other)
		{
			return id == other.id;
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}

		public override string ToString()
		{
			return id.ToString("X8"); // 8 digit hexadecimal uppercase
		}

		public static bool operator ==(NetId lhs, NetId rhs)
		{
			return lhs.id == rhs.id;
		}

		public static bool operator !=(NetId lhs, NetId rhs)
		{
			return lhs.id != rhs.id;
		}

		public static NetId operator +(NetId lhs, uint rhs)
		{
			return new NetId(lhs.id + rhs);
		}

		public static NetId operator ++(NetId value)
		{
			return new NetId(value.id + 1U);
		}

		public static NetId operator -(NetId lhs, uint rhs)
		{
			return new NetId(lhs.id - rhs);
		}

		public static NetId operator --(NetId value)
		{
			return new NetId(value.id - 1U);
		}

		public uint id;
	}

	public static class NetIdPakEx
	{
		public static bool ReadNetId(this NetPakReader reader, out NetId value)
		{
			return reader.ReadBits(32, out value.id);
		}

		public static bool WriteNetId(this NetPakWriter writer, NetId value)
		{
			return writer.WriteBits(value.id, 32);
		}

		public static bool ReadTransform(this NetPakReader reader, out Transform value)
		{
			bool hasTransform;
			bool result = reader.ReadBit(out hasTransform);
			if (result & hasTransform)
			{
				NetId netId;
				string path;
				result &= reader.ReadNetId(out netId);
				result &= reader.ReadString(out path, lengthBitCount: TRANSFORM_PATH_BYTE_COUNT_BITS);
				value = NetIdRegistry.GetTransform(netId, path);
			}
			else
			{
				value = null;
			}

			return result;
		}

		public static bool WriteTransform(this NetPakWriter writer, Transform value)
		{
			NetId netId;
			string path;
			if (NetIdRegistry.GetTransformNetId(value, out netId, out path))
			{
				return writer.WriteBit(true) && writer.WriteNetId(netId) && writer.WriteString(path, lengthBitCount: TRANSFORM_PATH_BYTE_COUNT_BITS);
			}
			else
			{
				return writer.WriteBit(false);
			}
		}

		// 1 bit header for "has transform", then fill the rest of the byte. 
		internal const int TRANSFORM_PATH_BYTE_COUNT_BITS = 7;
	}
}
