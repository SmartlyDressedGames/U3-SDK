////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.NetPak
{
	public static class SystemNetPakWriterEx
	{
		/// <summary>
		/// For example bitCount of 7 allows range [-64, +64).
		/// </summary>
		public static bool WriteSignedInt(this NetPakWriter writer, int value, int bitCount)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (bitCount < 1 || bitCount > 32) // 32 supported because we subtract one before left shift.
				throw new System.ArgumentOutOfRangeException("bitCount");
#endif // WITH_NETPAK_EXCEPTIONS

			int absMinValue = 1 << (bitCount - 1);

#if WITH_NETPAK_EXCEPTIONS
			if (value < -absMinValue || value >= absMinValue)
				throw new System.ArgumentOutOfRangeException("value");
#endif // WITH_NETPAK_EXCEPTIONS

			return writer.WriteBits((uint) (value + absMinValue), bitCount);
		}

		/// <summary>
		/// Values outside the range are clamped into range.
		/// For example intBitCount of 7 allows range [0, 128).
		/// </summary>
		public static bool WriteUnsignedClampedFloat(this NetPakWriter writer, float value, int intBitCount, int fracBitCount)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (intBitCount < 1 || intBitCount > 31) // 32 unsupported due to left shift overflow.
				throw new System.ArgumentOutOfRangeException("bitCount");
			if (fracBitCount < 1 || fracBitCount > 31) // 32 unsupported due to left shift overflow.
				throw new System.ArgumentOutOfRangeException("fracBitCount");
#endif // WITH_NETPAK_EXCEPTIONS

			int absMaxValue = 1 << intBitCount;
			if (value < 0.0f)
			{
				return writer.WriteBits(0, intBitCount) & writer.WriteBits(0, fracBitCount);
			}
			else if (value >= absMaxValue)
			{
				return writer.WriteBits(0xFFFFFFFFu, intBitCount) & writer.WriteBits(0xFFFFFFFFu, fracBitCount);
			}
			else
			{
				int intValue = Mathf.FloorToInt(value);
				bool result = writer.WriteBits((uint) intValue, intBitCount);

				// Specialized rather than using UNorm because we want [0,1) exclusive not [0,1] inclusive.
				// e.g. with fracBitCount of 2 we can represent 0.0, 0.25, 0.5, or 0.75
				float fracValue = value - intValue;
				uint maxFracValue = 1U << fracBitCount; // e.g. with bitCount of 2 maxValue is 4
				uint quantizedFracValue = (uint) (fracValue * maxFracValue);
				return result & writer.WriteBits(quantizedFracValue, fracBitCount);
			}
		}

		/// <summary>
		/// Values outside the range are clamped into range.
		/// For example intBitCount of 7 allows range [-64, +64).
		/// </summary>
		public static bool WriteClampedFloat(this NetPakWriter writer, float value, int intBitCount, int fracBitCount)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (intBitCount < 1 || intBitCount > 32) // 32 supported because we subtract one before left shift.
				throw new System.ArgumentOutOfRangeException("bitCount");
			if (fracBitCount < 1 || fracBitCount > 31) // 32 unsupported due to left shift overflow.
				throw new System.ArgumentOutOfRangeException("fracBitCount");
#endif // WITH_NETPAK_EXCEPTIONS

			int absMinValue = 1 << (intBitCount - 1);
			if (value < -absMinValue)
			{
				return writer.WriteBits(0, intBitCount) & writer.WriteBits(0, fracBitCount);
			}
			else if (value >= absMinValue)
			{
				return writer.WriteBits(0xFFFFFFFFu, intBitCount) & writer.WriteBits(0xFFFFFFFFu, fracBitCount);
			}
			else if (Mathf.Abs(value) < 0.0001f) // Centimeter = 0.01f, millimeter = 0.001f.
			{
				// 2023-02-06: adding near-zero case because precision loss of extremely small values
				// in FloorToInt or (fracValue = value - intValue) can result in the read value being off
				// by +/- one. (public issue #3686) I did experiment with different ways of separating
				// intValue and fracValue without luck, but this seems like a reasonable workaround
				// considering ClampedFloat is only used by position in the game. Barricades and structures
				// have some of the highest fracBitCount at 11: maxFracValue = 2048, inverse = 0.00048828125.
				return writer.WriteBits((uint) absMinValue, intBitCount) & writer.WriteBits(0, fracBitCount);
			}
			else
			{
				int intValue = Mathf.FloorToInt(value);
				// Identical to WriteSignedInt, inlined because we already calculated absMinValue.
				bool result = writer.WriteBits((uint) (value + absMinValue), intBitCount);

				// Specialized rather than using UNorm because we want [0,1) exclusive not [0,1] inclusive.
				// e.g. with fracBitCount of 2 we can represent 0.0, 0.25, 0.5, or 0.75
				float fracValue = value - intValue;
				uint maxFracValue = 1U << fracBitCount; // e.g. with bitCount of 2 maxValue is 4
				uint quantizedFracValue = (uint) (fracValue * maxFracValue);
				return result & writer.WriteBits(quantizedFracValue, fracBitCount);
			}
		}

		public static bool WriteInt8(this NetPakWriter writer, sbyte value)
		{
			return writer.WriteBits((uint) value, 8);
		}

		public static bool WriteInt16(this NetPakWriter writer, short value)
		{
			return writer.WriteBits((uint) value, 16);
		}

		public static bool WriteInt32(this NetPakWriter writer, int value)
		{
			return writer.WriteBits((uint) value, 32);
		}

		public static bool WriteInt64(this NetPakWriter writer, long value)
		{
			bool result = writer.WriteBits((uint) (value >> 32), 32);
			result &= writer.WriteBits((uint) value, 32);
			return result;
		}

		public static bool WriteUInt8(this NetPakWriter writer, byte value)
		{
			return writer.WriteBits(value, 8);
		}

		public static bool WriteUInt16(this NetPakWriter writer, ushort value)
		{
			return writer.WriteBits(value, 16);
		}

		public static bool WriteUInt32(this NetPakWriter writer, uint value)
		{
			return writer.WriteBits(value, 32);
		}

		public static bool WriteUInt64(this NetPakWriter writer, ulong value)
		{
			bool result = writer.WriteBits((uint) (value >> 32), 32);
			result &= writer.WriteBits((uint) value, 32);
			return result;
		}

		/// <summary>
		/// Encode a float in the range [0.0, 1.0]. Endpoints are encoded exactly, but not the midpoint (0.5).
		/// </summary>
		public static bool WriteUnsignedNormalizedFloat(this NetPakWriter writer, float value, int bitCount)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (value < 0.0f || value > 1.0f)
				throw new System.ArgumentOutOfRangeException("value");
			
			if (bitCount < 1 || bitCount > 31) // 32 unsupported due to left shift overflow.
				throw new System.ArgumentOutOfRangeException("bitCount");
#endif // WITH_NETPAK_EXCEPTIONS

			uint maxValue = (1U << bitCount) - 1; // e.g. with bitCount of 2 maxValue is 3
			uint quantizedValue = (uint) ((value * maxValue) + 0.5f);
			return writer.WriteBits(quantizedValue, bitCount);
		}

		/// <summary>
		/// Encode a float in the range [-1.0, +1.0]. Endpoints and midpoint (0.0) are encoded exactly.
		/// </summary>
		public static bool WriteSignedNormalizedFloat(this NetPakWriter writer, float value, int bitCount)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (value < -1.0f || value > 1.0f)
				throw new System.ArgumentOutOfRangeException("value");

			if (bitCount < 2 || bitCount > 32) // 32 supported because we subtract one before left shift.
				throw new System.ArgumentOutOfRangeException("bitCount");
#endif // WITH_NETPAK_EXCEPTIONS

			uint maxValuePlusOne = 1U << (bitCount - 1);
			uint maxValue = maxValuePlusOne - 1; // e.g. with bitCount of 2 maxValue is 1
			uint quantizedValue;
			if (value >= 0.0f)
			{
				quantizedValue = (uint) ((value * maxValue) + 0.5f);
			}
			else
			{
				quantizedValue = (uint) ((-value * maxValue) + 0.5f);
				quantizedValue |= maxValuePlusOne; // Negative flag.
			}
			return writer.WriteBits(quantizedValue, bitCount);
		}

		public static bool WriteFloat(this NetPakWriter writer, float value)
		{
			uint bits;
			unsafe
			{
				bits = *(uint*) &value;
			}
			return writer.WriteUInt32(bits);
		}

		/// <summary>
		/// Encode radians wrapped into the range [0, TWO_PI).
		/// </summary>
		public static bool WriteRadians(this NetPakWriter writer, float value, int bitCount = 8)
		{
			const float TAU = Mathf.PI * 2.0f;
			float remainder = ((value % TAU) + TAU) % TAU; // Todo there must be a smarter solution for this.
			float normalized = remainder / TAU;
			uint maxValue = 1U << bitCount; // e.g. with bitCount of 2 maxValue is 4
			uint quantizedValue = (uint) (normalized * maxValue);
			return writer.WriteBits(quantizedValue, bitCount);
		}

		/// <summary>
		/// Encode degrees wrapped into the range [0, 360).
		/// </summary>
		public static bool WriteDegrees(this NetPakWriter writer, float value, int bitCount = 8)
		{
			float remainder = ((value % 360.0f) + 360.0f) % 360.0f; // Todo there must be a smarter solution for this.
			float normalized = remainder / 360.0f;
			uint maxValue = 1U << bitCount; // e.g. with bitCount of 2 maxValue is 4
			uint quantizedValue = (uint) (normalized * maxValue);
			return writer.WriteBits(quantizedValue, bitCount);
		}

		public static bool WriteString(this NetPakWriter writer, string value, int lengthBitCount = NetPakConst.MAX_STRING_BYTE_COUNT_BITS)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (lengthBitCount < 0 || lengthBitCount > NetPakConst.MAX_STRING_BYTE_COUNT_BITS)
				throw new System.ArgumentOutOfRangeException("lengthBitCount");
#endif // WITH_NETPAK_EXCEPTIONS

			if (string.IsNullOrEmpty(value))
			{
				return writer.WriteBit(true);
			}

			try
			{
				int byteCount = NetPakConst.stringEncoding.GetBytes(value, 0, value.Length, NetPakConst.STRING_BUFFER, 0);
				// Before the "null or empty" bit flag was added this had to be able to represent zero, but now the
				// receiver can infer that byteCount is at least 1.
				int maxByteCount = 1 << lengthBitCount;
				if (byteCount > maxByteCount)
				{
					byteCount = maxByteCount;
				}
				bool result = writer.WriteBit(false);
				result &= writer.WriteBits((uint) (byteCount - 1), lengthBitCount);
				result &= writer.WriteBytes(NetPakConst.STRING_BUFFER, byteCount);
				return result;
			}
			catch
			{
				// Longer than buffer, in which case we write an empty string.
				return writer.WriteBit(true);
			}
		}

		public static bool WriteGuid(this NetPakWriter writer, System.Guid value)
		{
			ulong high;
			ulong low;
			unsafe
			{
				ulong* bitsPtr = (ulong*) &value;
				high = *bitsPtr;
				low = *(bitsPtr + 1);
			}
			bool result = writer.WriteUInt64(high);
			result &= writer.WriteUInt64(low);
			return result;
		}

		public static bool WriteDateTime(this NetPakWriter writer, System.DateTime value)
		{
			return writer.WriteInt64(value.ToBinary());
		}

#if UNITY_EDITOR
		/// <summary>
		/// Placeholder allowing user assembly to compile before specialized implementation is generated.
		/// </summary>
		public static bool WriteEnum<T>(this NetPakWriter writer, T value) where T : System.Enum
		{
#if WITH_NETPAK_EXCEPTIONS
			throw new System.NotImplementedException();
#else
			return false;
#endif // WITH_NETPAK_EXCEPTIONS
		}
#endif // UNITY_EDITOR

		public delegate bool WriteListItem<T>(T item);
		public static bool WriteList<T>(this NetPakWriter writer, System.Collections.Generic.List<T> list, WriteListItem<T> writeFunc, NetLength maxLength)
		{
			uint count = maxLength.Clamp(list.Count);
			bool result = writer.WriteBits(count, maxLength.bitCount);
			for (int index = 0; index < count; ++index)
			{
				result &= writeFunc(list[index]);
			}
			return result;
		}

		public delegate bool WriteListItemWithWriter<T>(NetPakWriter writer, T item);
		public static bool WriteList<T>(this NetPakWriter writer, System.Collections.Generic.List<T> list, WriteListItemWithWriter<T> writeFunc, NetLength maxLength)
		{
			uint count = maxLength.Clamp(list.Count);
			bool result = writer.WriteBits(count, maxLength.bitCount);
			for (int index = 0; index < count; ++index)
			{
				result &= writeFunc(writer, list[index]);
			}
			return result;
		}

		/// <summary>
		/// Ideally should not be used by new code.
		/// </summary>
		public static bool WriteStateArray(this NetPakWriter writer, byte[] value)
		{
			byte valueLength = (byte) value.Length;
			bool result = writer.WriteUInt8(valueLength);
			result &= writer.WriteBytes(value, valueLength);
			return result;
		}
	}
}
