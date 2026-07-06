////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.NetPak
{
	public static class NetPakConst
	{
		/// <summary>
		/// Uses "smallest three" optimization described by Glenn Fiedler: https://gafferongames.com/post/snapshot_compression/
		/// Quoting here in case the link moves: "If v is the absolute value of the largest quaternion component,
		/// the next largest possible component value occurs when two components have the same absolute value and the
		/// other two components are zero. The length of that quaternion (v,v,0,0) is 1, therefore v^2 + v^2 = 1,
		/// 2v^2 = 1, v = 1/sqrt(2). This means you can encode the smallest three components in [-0.707107,+0.707107]
		/// instead of [-1,+1] giving you more precision with the same number of bits."
		/// </summary>
		public const float INV_SQRT_OF_TWO = 0.70710678118f; // 1/sqrt(2)
		public const float SQRT_OF_TWO = 1.41421356237f; // sqrt(2)

		/// <summary>
		/// Maximum number of bits to read/write for string byte count without overflowing the string buffer.
		/// </summary>
		public const int MAX_STRING_BYTE_COUNT_BITS = 11;
		/// <summary>
		/// Maximum number of UTF8 bytes for string.
		/// Before the "null or empty" flag was added the length had to be able to represent 0, but now the receiver
		/// can infer that the byte count is at least 1.
		/// </summary>
		public const int MAX_STRING_BYTE_COUNT = 1 << MAX_STRING_BYTE_COUNT_BITS; // 2048

		/// <summary>
		/// encoderShouldEmitUTF8Identifier enables byte order mark (BOM) which is unnecessary for UTF8.
		/// throwOnInvalidBytes allows reader to discard bad string packets.
		/// </summary>
		internal static System.Text.UTF8Encoding stringEncoding = new System.Text.UTF8Encoding(/*encoderShouldEmitUTF8Identifier*/false, /*throwOnInvalidBytes*/true);
		internal static byte[] STRING_BUFFER = new byte[MAX_STRING_BYTE_COUNT];

		public static int CountBits(uint value)
		{
			// There are faster ways to count bits, like special instructions on certain platforms, but this will be
			// fine for now because it is not used in the hot path.
			int bitCount = 0;
			while (value > 0)
			{
				++bitCount;
				value >>= 1;
			}
			return bitCount;
		}
	}
}
