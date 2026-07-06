////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.NetPak
{
	public static class SystemNetPakReaderEx
	{
		/// <summary>
		/// For example bitCount of 7 allows range [-64, +64).
		/// </summary>
		public static bool ReadSignedInt(this NetPakReader reader, int bitCount, out int value)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (bitCount < 1)
				throw new System.ArgumentOutOfRangeException("bitCount");
#endif // WITH_NETPAK_EXCEPTIONS

			uint bits;
			bool result = reader.ReadBits(bitCount, out bits);
			int absMinValue = 1 << (bitCount - 1);
			value = ((int) bits) - absMinValue;
			return result;
		}

		/// <summary>
		/// Values outside the range are clamped into range.
		/// For example intBitCount of 7 allows range [0, 128).
		/// </summary>
		public static bool ReadUnsignedClampedFloat(this NetPakReader reader, int intBitCount, int fracBitCount, out float value)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (fracBitCount < 1 || fracBitCount > 31) // 32 unsupported due to left shift overflow.
				throw new System.ArgumentOutOfRangeException("fracBitCount");
#endif // WITH_NETPAK_EXCEPTIONS

			uint intValue;
			uint quantizedFracValue;
			bool result = reader.ReadBits(intBitCount, out intValue) & reader.ReadBits(fracBitCount, out quantizedFracValue);

			// Specialized rather than using UNorm because we want [0,1) exclusive not [0,1] inclusive.
			// e.g. with fracBitCount of 2 we can represent 0.0, 0.25, 0.5, or 0.75
			uint maxFracValue = 1U << fracBitCount; // e.g. with bitCount of 2 maxValue is 4
			float fracValue = quantizedFracValue / (float) maxFracValue;

			value = intValue + fracValue;
			return result;
		}

		/// <summary>
		/// Values outside the range are clamped into range.
		/// For example intBitCount of 7 allows range [-64, +64).
		/// </summary>
		public static bool ReadClampedFloat(this NetPakReader reader, int intBitCount, int fracBitCount, out float value)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (fracBitCount < 1 || fracBitCount > 31) // 32 unsupported due to left shift overflow.
				throw new System.ArgumentOutOfRangeException("fracBitCount");
#endif // WITH_NETPAK_EXCEPTIONS

			int intValue;
			uint quantizedFracValue;
			bool result = ReadSignedInt(reader, intBitCount, out intValue) & reader.ReadBits(fracBitCount, out quantizedFracValue);

			// Specialized rather than using UNorm because we want [0,1) exclusive not [0,1] inclusive.
			// e.g. with fracBitCount of 2 we can represent 0.0, 0.25, 0.5, or 0.75
			uint maxFracValue = 1U << fracBitCount; // e.g. with bitCount of 2 maxValue is 4
			float fracValue = quantizedFracValue / (float) maxFracValue;

			value = intValue + fracValue;
			return result;
		}

		public static bool ReadInt8(this NetPakReader reader, out sbyte value)
		{
			uint bits;
			bool result = reader.ReadBits(8, out bits);
			value = (sbyte) bits;
			return result;
		}

		public static bool ReadInt16(this NetPakReader reader, out short value)
		{
			uint bits;
			bool result = reader.ReadBits(16, out bits);
			value = (short) bits;
			return result;
		}

		public static bool ReadInt32(this NetPakReader reader, out int value)
		{
			uint bits;
			bool result = reader.ReadBits(32, out bits);
			value = (int) bits;
			return result;
		}

		public static bool ReadInt64(this NetPakReader reader, out long value)
		{
			uint high;
			uint low;
			bool result = reader.ReadBits(32, out high) & reader.ReadBits(32, out low);
			value = (long) (((ulong) high << 32) | low);
			return result;
		}

		public static bool ReadUInt8(this NetPakReader reader, out byte value)
		{
			uint bits;
			bool result = reader.ReadBits(8, out bits);
			value = (byte) bits;
			return result;
		}

		public static bool ReadUInt16(this NetPakReader reader, out ushort value)
		{
			uint bits;
			bool result = reader.ReadBits(16, out bits);
			value = (ushort) bits;
			return result;
		}

		public static bool ReadUInt32(this NetPakReader reader, out uint value)
		{
			return reader.ReadBits(32, out value);
		}

		public static bool ReadUInt64(this NetPakReader reader, out ulong value)
		{
			uint high;
			uint low;
			bool result = reader.ReadBits(32, out high) & reader.ReadBits(32, out low);
			value = ((ulong) high << 32) | low;
			return result;
		}

		/// <summary>
		/// Decode a float in the range [0.0, 1.0]. Endpoints are encoded exactly, but not the midpoint (0.5).
		/// </summary>
		public static bool ReadUnsignedNormalizedFloat(this NetPakReader reader, int bitCount, out float value)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (bitCount < 1 || bitCount > 31) // 32 unsupported due to left shift overflow.
				throw new System.ArgumentOutOfRangeException("bitCount");
#endif // WITH_NETPAK_EXCEPTIONS

			uint quantizedValue;
			bool result = reader.ReadBits(bitCount, out quantizedValue);
			uint maxValue = (1U << bitCount) - 1; // e.g. with bitCount of 2 maxValue is 3
			value = quantizedValue / (float) maxValue;
			return result;
		}

		/// <summary>
		/// Decode a float in the range [-1.0, +1.0]. Endpoints and midpoint (0.0) are encoded exactly.
		/// </summary>
		public static bool ReadSignedNormalizedFloat(this NetPakReader reader, int bitCount, out float value)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (bitCount < 2 || bitCount > 32) // 32 supported because we subtract one before left shift.
				throw new System.ArgumentOutOfRangeException("bitCount");
#endif // WITH_NETPAK_EXCEPTIONS

			uint quantizedValue;
			bool result = reader.ReadBits(bitCount, out quantizedValue);
			uint maxValuePlusOne = 1U << (bitCount - 1);
			uint maxValue = maxValuePlusOne - 1; // e.g. with bitCount of 2 maxValue is 1
			if ((quantizedValue & maxValuePlusOne) == maxValuePlusOne) // Is negative flag set?
			{
				// Mask out the sign bit.
				value = -((quantizedValue & maxValue) / (float) maxValue);
			}
			else
			{
				value = quantizedValue / (float) maxValue;
			}
			return result;
		}

		public static bool ReadFloat(this NetPakReader reader, out float value)
		{
			uint bits;
			bool result = reader.ReadUInt32(out bits);
			unsafe
			{
				value = *(float*) &bits;
			}
			return result;
		}

		public static bool ReadRadians(this NetPakReader reader, out float value, int bitCount = 8)
		{
			uint quantizedValue;
			bool result = reader.ReadBits(bitCount, out quantizedValue);

			const float TAU = Mathf.PI * 2.0f;
			uint maxValue = 1U << bitCount; // e.g. with bitCount of 2 maxValue is 4
			value = quantizedValue / (float) maxValue * TAU;
			return result;
		}

		public static bool ReadDegrees(this NetPakReader reader, out float value, int bitCount = 8)
		{
			uint quantizedValue;
			bool result = reader.ReadBits(bitCount, out quantizedValue);
			uint maxValue = 1U << bitCount; // e.g. with bitCount of 2 maxValue is 4
			value = quantizedValue / (float) maxValue * 360.0f;
			return result;
		}

		public static bool ReadString(this NetPakReader reader, out string value, int lengthBitCount = NetPakConst.MAX_STRING_BYTE_COUNT_BITS)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (lengthBitCount < 0 || lengthBitCount > NetPakConst.MAX_STRING_BYTE_COUNT_BITS)
				throw new System.ArgumentOutOfRangeException(nameof(lengthBitCount), $"max lengthBitCount is {NetPakConst.MAX_STRING_BYTE_COUNT_BITS}");
#endif // WITH_NETPAK_EXCEPTIONS

			bool isNullOrEmpty;
			if (!reader.ReadBit(out isNullOrEmpty))
			{
				value = string.Empty;
				return false;
			}

			if (isNullOrEmpty)
			{
				value = string.Empty;
				return true;
			}

			uint lengthBits;
			if (!reader.ReadBits(lengthBitCount, out lengthBits))
			{
				value = string.Empty;
				return false;
			}

			// We know length is at least 1 because it was not null or empty.
			int byteCount = (int) (lengthBits + 1);
			if (!reader.ReadBytes(NetPakConst.STRING_BUFFER, byteCount))
			{
				value = string.Empty;
				return false;
			}

			// throwOnInvalidBytes is enabled, so if we catch an exception the packet should be discarded.
			try
			{
				value = NetPakConst.stringEncoding.GetString(NetPakConst.STRING_BUFFER, 0, byteCount);
				return true;
			}
			catch
			{
				value = string.Empty;
				return false;
			}
		}

		public static bool ReadGuid(this NetPakReader reader, out System.Guid value)
		{
			ulong high;
			ulong low;
			bool result = reader.ReadUInt64(out high) & reader.ReadUInt64(out low);
			unsafe
			{
				fixed (System.Guid* valuePtr = &value)
				{
					ulong* bitsPtr = (ulong*) valuePtr;
					*bitsPtr = high;
					*(bitsPtr + 1) = low;
				}
			}
			return result;
		}

		public static bool ReadDateTime(this NetPakReader reader, out System.DateTime value)
		{
			long dateData;
			bool result = reader.ReadInt64(out dateData);
			value = System.DateTime.FromBinary(dateData);
			return result;
		}

#if UNITY_EDITOR
		/// <summary>
		/// Placeholder allowing user assembly to compile before specialized implementation is generated.
		/// </summary>
		public static bool ReadEnum<T>(this NetPakReader reader, out T value) where T : System.Enum
		{
#if WITH_NETPAK_EXCEPTIONS
			throw new System.NotImplementedException();
#else
			value = default;
			return false;
#endif // WITH_NETPAK_EXCEPTIONS
		}
#endif // UNITY_EDITOR

		public delegate bool ReadListItem<T>(out T item);
		public static bool ReadList<T>(this NetPakReader reader, System.Collections.Generic.List<T> list, ReadListItem<T> readFunc, NetLength maxLength)
		{
			uint count;
			bool result = reader.ReadBits(maxLength.bitCount, out count);
			count = maxLength.Clamp(count);
			for (int index = 0; index < count; ++index)
			{
				T item;
				result &= readFunc(out item);
				list.Add(item);
			}
			return result;
		}

		public delegate bool ReadListItemWithReader<T>(NetPakReader reader, out T item);
		public static bool ReadList<T>(this NetPakReader reader, System.Collections.Generic.List<T> list, ReadListItemWithReader<T> readFunc, NetLength maxLength)
		{
			uint count;
			bool result = reader.ReadBits(maxLength.bitCount, out count);
			count = maxLength.Clamp(count);
			for (int index = 0; index < count; ++index)
			{
				T item;
				result &= readFunc(reader, out item);
				list.Add(item);
			}
			return result;
		}

		/// <summary>
		/// Ideally should not be used by new code.
		/// </summary>
		public static bool ReadStateArray(this NetPakReader reader, out byte[] value)
		{
			byte valueLength;
			bool result = reader.ReadUInt8(out valueLength);
			value = new byte[valueLength];
			result &= reader.ReadBytes(value, valueLength);
			return result;
		}
	}
}
