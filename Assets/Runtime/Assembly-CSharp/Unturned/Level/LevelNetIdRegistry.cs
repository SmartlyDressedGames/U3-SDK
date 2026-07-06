////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// It is useful to be able to reference transforms generically over the network, for example to attach a bullet
	/// hole to a tree or vehicle without tagging it as a tree or vehicle, but most entities placed in the level do not
	/// have unique IDs. To work around this we count downward from uint.MaxValue for level objects to avoid conflicts
	/// with server-assigned net ids.
	/// </summary>
	internal static class LevelNetIdRegistry
	{
		/// <summary>
		/// Nelson 2025-06-10: this is used by older level file formats, but with placement of trees outside legacy
		/// bounds now supported we use only the index in that case with GetTreeNetIdV2.
		/// 
		/// Each region can have ushort.MaxValue trees, and we reserve that entire block so that a region can be slightly
		/// modified on the client or server without breaking all netids in the level.
		/// </summary>
		public static NetId GetTreeNetId(byte regionX, byte regionY, ushort index)
		{
			// Regions are [0, 63] so occupy 6 bits.
			return new NetId(TREE_FLAG | (uint) (regionX << 22) | (uint) (regionY << 16) | index);
		}

		public static NetId GetTreeNetIdV2(int index)
		{
			return new NetId(TREE_FLAG | ((uint) index));
		}

		/// <summary>
		/// Each region can have ushort.MaxValue objects, and we reserve that entire block so that a region can be slightly
		/// modified on the client or server without breaking all netids in the level.
		/// </summary>
		public static NetId GetRegularObjectNetId(byte regionX, byte regionY, ushort index)
		{
			// Regions are [0, 63] so occupy 6 bits.
			return new NetId(REGULAR_OBJECT_FLAG | (uint) (regionX << 22) | (uint) (regionY << 16) | index);
		}

		/// <summary>
		/// Devkit instance IDs should already be fairly stable. There is no way any level is using more than 30 bits
		/// for the instance ID, so it should be safe to set those bits to prevent collisions with server net IDs.
		/// </summary>
		public static NetId GetDevkitObjectNetId(uint instanceId)
		{
			return new NetId(DEVKIT_OBJECT_FLAG | instanceId);
		}

		private const uint TREE_FLAG = 0x80000000;
		private const uint REGULAR_OBJECT_FLAG = 0x40000000;
		private const uint DEVKIT_OBJECT_FLAG = 0xC0000000;
	}
}
