////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned
{
	public struct PhysicsMaterialNetId : System.IEquatable<PhysicsMaterialNetId>
	{
		public static readonly PhysicsMaterialNetId NULL = new PhysicsMaterialNetId(0);

		public PhysicsMaterialNetId(uint id)
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
			return obj is PhysicsMaterialNetId && id == ((PhysicsMaterialNetId) obj).id;
		}

		public bool Equals(PhysicsMaterialNetId other)
		{
			return id == other.id;
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}

		public override string ToString()
		{
			return id.ToString("X2"); // 2 digit hexadecimal uppercase
		}

		public static bool operator ==(PhysicsMaterialNetId lhs, PhysicsMaterialNetId rhs)
		{
			return lhs.id == rhs.id;
		}

		public static bool operator !=(PhysicsMaterialNetId lhs, PhysicsMaterialNetId rhs)
		{
			return lhs.id != rhs.id;
		}

		public uint id;
	}

	public static class PhysicsMaterialNetIdPakEx
	{
		public static bool ReadPhysicsMaterialNetId(this NetPakReader reader, out PhysicsMaterialNetId value)
		{
			return reader.ReadBits(PhysicsMaterialNetTable.idBitCount, out value.id);
		}

		public static bool WritePhysicsMaterialNetId(this NetPakWriter writer, PhysicsMaterialNetId value)
		{
			return writer.WriteBits(value.id, PhysicsMaterialNetTable.idBitCount);
		}

		public static bool ReadPhysicsMaterialName(this NetPakReader reader, out string materialName)
		{
			PhysicsMaterialNetId netId;
			bool result = ReadPhysicsMaterialNetId(reader, out netId);
			materialName = PhysicsMaterialNetTable.GetMaterialName(netId);
			return result;
		}

		public static bool WritePhysicsMaterialName(this NetPakWriter writer, string materialName)
		{
			PhysicsMaterialNetId netId = PhysicsMaterialNetTable.GetNetId(materialName);
			return WritePhysicsMaterialNetId(writer, netId);
		}
	}
}
