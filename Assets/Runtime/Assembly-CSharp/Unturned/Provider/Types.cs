////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class Types
	{
		public static readonly System.Type STRING_TYPE = typeof(string);
		public static readonly System.Type STRING_ARRAY_TYPE = typeof(string[]);
		public static readonly System.Type BOOLEAN_TYPE = typeof(bool);
		public static readonly System.Type BOOLEAN_ARRAY_TYPE = typeof(bool[]);
		public static readonly System.Type BYTE_ARRAY_TYPE = typeof(byte[]);
		public static readonly System.Type BYTE_TYPE = typeof(byte);
		public static readonly System.Type INT16_TYPE = typeof(short);
		public static readonly System.Type UINT16_TYPE = typeof(ushort);
		public static readonly System.Type INT32_ARRAY_TYPE = typeof(int[]);
		public static readonly System.Type INT32_TYPE = typeof(int);
		public static readonly System.Type UINT32_TYPE = typeof(uint);
		public static readonly System.Type SINGLE_TYPE = typeof(float);
		public static readonly System.Type INT64_TYPE = typeof(long);
		public static readonly System.Type UINT64_ARRAY_TYPE = typeof(ulong[]);
		public static readonly System.Type UINT64_TYPE = typeof(ulong);
		public static readonly System.Type STEAM_ID_TYPE = typeof(Steamworks.CSteamID);
		public static readonly System.Type GUID_TYPE = typeof(System.Guid);
		public static readonly System.Type VECTOR3_TYPE = typeof(UnityEngine.Vector3);
		public static readonly System.Type COLOR_TYPE = typeof(UnityEngine.Color);

		/// <summary>
		/// Not originally supported by networking. Added temporarily during netpak rewrite because the quaternion
		/// compression is so much better for vehicles than three byte Euler rotation.
		/// </summary>
		public static readonly System.Type QUATERNION_TYPE = typeof(UnityEngine.Quaternion);

		public static readonly byte[] SHIFTS = new byte[8]
		{
			1,
			2,
			4,
			8,
			16,
			32,
			64,
			128
		};
	}
}
