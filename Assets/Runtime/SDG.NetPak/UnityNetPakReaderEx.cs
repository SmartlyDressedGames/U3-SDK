////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using SDG.Unturned; // QuaternionEx

namespace SDG.NetPak
{
	public static class UnityNetPakReaderEx
	{
		/// <summary>
		/// Uses "smallest three" optimization described by Glenn Fiedler: https://gafferongames.com/post/snapshot_compression/
		/// Quoting here in case the link moves: "Since we know the quaternion represents a rotation its length must
		/// be 1, so x^2+y^2+z^2+w^2 = 1. We can use this identity to drop one component and reconstruct it on the
		/// other side. For example, if you send x,y,z you can reconstruct w = sqrt(1 - x^2 - y^2 - z^2). You might
		/// think you need to send a sign bit for w in case it is negative, but you don’t, because you can make w always
		/// positive by negating the entire quaternion if w is negative (in quaternion space (x,y,z,w) and (-x,-y,-z,-w)
		/// represent the same rotation.) Don’t always drop the same component due to numerical precision issues.
		/// Instead, find the component with the largest absolute value and encode its index using two bits [0, 3]
		/// (0=x, 1=y, 2=z, 3=w), then send the index of the largest component and the smallest three components over
		/// the network (hence the name). On the other side use the index of the largest bit to know which component
		/// you have to reconstruct from the other three."
		/// </summary>
		public static bool ReadQuaternion(this NetPakReader reader, out Quaternion value, int bitsPerComponent = 9)
		{
			uint largestComponentIndex;
			if (!reader.ReadBits(2, out largestComponentIndex))
			{
				value = Quaternion.identity;
				return false;
			}

			float value0;
			float value1;
			float value2;
			if (!reader.ReadSignedNormalizedFloat(bitsPerComponent, out value0) ||
				!reader.ReadSignedNormalizedFloat(bitsPerComponent, out value1) ||
				!reader.ReadSignedNormalizedFloat(bitsPerComponent, out value2))
			{
				value = Quaternion.identity;
				return false;
			}

			value0 *= NetPakConst.INV_SQRT_OF_TWO;
			value1 *= NetPakConst.INV_SQRT_OF_TWO;
			value2 *= NetPakConst.INV_SQRT_OF_TWO;
			float largestComponentValue = Mathf.Sqrt(1.0f - ((value0 * value0) + (value1 * value1) + (value2 * value2)));

			bool result;
			switch (largestComponentIndex)
			{
				case 0: // X is the largest component.
					value = new Quaternion(largestComponentValue, value0, value1, value2);
					result = true;
					break;

				case 1: // Y is the largest component.
					value = new Quaternion(value0, largestComponentValue, value1, value2);
					result = true;
					break;

				case 2: // Z is the largest component.
					value = new Quaternion(value0, value1, largestComponentValue, value2);
					result = true;
					break;

				case 3: // W is the largest component.
					value = new Quaternion(value0, value1, value2, largestComponentValue);
					result = true;
					break;

				default:
#if WITH_NETPAK_EXCEPTIONS
					throw new System.Exception("invalid largest component index");
#else
					value = Quaternion.identity;
					result = false;
					break;
#endif // WITH_NETPAK_EXCEPTIONS
			}

#if WITH_NETPAK_EXCEPTIONS
			if (!value.IsNormalized()) // Sanity check...
			{
				throw new System.Exception("quaternion should be normalized");
			}
#endif // WITH_NETPAK_EXCEPTIONS

			return result;
		}

		/// <summary>
		/// Similar to the quaternion optimization, but needs a sign bit for the largest value.
		/// </summary>
		public static bool ReadNormalVector3(this NetPakReader reader, out Vector3 value, int bitsPerComponent = 9)
		{
			uint largestComponentIndex;
			bool largestComponentIsNegative;
			if (!reader.ReadBits(2, out largestComponentIndex) || !reader.ReadBit(out largestComponentIsNegative))
			{
				value = Vector3.forward;
				return false;
			}

			float value0;
			float value1;
			if (!reader.ReadSignedNormalizedFloat(bitsPerComponent, out value0) ||
				!reader.ReadSignedNormalizedFloat(bitsPerComponent, out value1))
			{
				value = Vector3.forward;
				return false;
			}

			value0 *= NetPakConst.INV_SQRT_OF_TWO;
			value1 *= NetPakConst.INV_SQRT_OF_TWO;
			float largestComponentValue = Mathf.Sqrt(1.0f - ((value0 * value0) + (value1 * value1)));
			if (largestComponentIsNegative)
			{
				largestComponentValue = -largestComponentValue;
			}

			bool result;
			switch (largestComponentIndex)
			{
				case 0: // X is the largest component.
					value = new Vector3(largestComponentValue, value0, value1);
					result = true;
					break;

				case 1: // Y is the largest component.
					value = new Vector3(value0, largestComponentValue, value1);
					result = true;
					break;

				case 2: // Z is the largest component.
					value = new Vector3(value0, value1, largestComponentValue);
					result = true;
					break;

				default:
#if WITH_NETPAK_EXCEPTIONS
					throw new System.Exception("invalid largest component index");
#else
					value = Vector3.forward;
					result = false;
					break;
#endif // WITH_NETPAK_EXCEPTIONS
			}

#if WITH_NETPAK_EXCEPTIONS
			if (!value.IsNormalized()) // Sanity check...
			{
				throw new System.Exception("vector should be normalized");
			}
#endif // WITH_NETPAK_EXCEPTIONS

			return result;
		}

		/// <summary>
		/// Default intBitCount of 13 allows a range of [-4096, +4096).
		/// </summary>
		public static bool ReadClampedVector3(this NetPakReader reader, out Vector3 value, int intBitCount = 13, int fracBitCount = 7)
		{
			bool result = reader.ReadClampedFloat(intBitCount, fracBitCount, out value.x);
			result &= reader.ReadClampedFloat(intBitCount, fracBitCount, out value.y);
			result &= reader.ReadClampedFloat(intBitCount, fracBitCount, out value.z);
			return result;
		}

		/// <summary>
		/// Read 8-bit per channel color excluding alpha.
		/// </summary>
		public static bool ReadColor32RGB(this NetPakReader reader, out Color32 value)
		{
			byte r;
			byte g;
			byte b;
			bool result = reader.ReadUInt8(out r) &
				reader.ReadUInt8(out g) &
				reader.ReadUInt8(out b);
			value = new Color32(r, g, b, byte.MaxValue);
			return result;
		}

		/// <summary>
		/// Read 8-bit per channel color excluding alpha.
		/// </summary>
		public static bool ReadColor32RGB(this NetPakReader reader, out Color value)
		{
			Color32 temp;
			bool result = ReadColor32RGB(reader, out temp);
			value = temp;
			return result;
		}

		/// <summary>
		/// Read 8-bit per channel color including alpha.
		/// </summary>
		public static bool ReadColor32RGBA(this NetPakReader reader, out Color32 value)
		{
			byte r;
			byte g;
			byte b;
			byte a;
			bool result = reader.ReadUInt8(out r) &
				reader.ReadUInt8(out g) &
				reader.ReadUInt8(out b) &
				reader.ReadUInt8(out a);
			value = new Color32(r, g, b, a);
			return result;
		}

		/// <summary>
		/// Read 8-bit per channel color including alpha.
		/// </summary>
		public static bool ReadColor32RGBA(this NetPakReader reader, out Color value)
		{
			Color32 temp;
			bool result = ReadColor32RGBA(reader, out temp);
			value = temp;
			return result;
		}

		/// <summary>
		/// Note: "Special" here refers to the -90 rotation on the X axis. :)
		/// Read only yaw if quaternion was flat, full quaternion otherwise.
		/// </summary>
		public static bool ReadSpecialYawOrQuaternion(this NetPakReader reader, out Quaternion value, int yawBitCount = 9, int quaternionBitsPerComponent = 9)
		{
			bool result = reader.ReadBit(out bool isOnlyRotatedAroundYAxis);
			if (isOnlyRotatedAroundYAxis)
			{
				result &= reader.ReadRadians(out float yawRadians, bitCount: yawBitCount);
				value = Quaternion.Euler(-90.0f, yawRadians * Mathf.Rad2Deg, 0.0f);
			}
			else
			{
				result &= reader.ReadQuaternion(out value, bitsPerComponent: quaternionBitsPerComponent);
			}
			return result;
		}

		public static bool ReadNormalVector3AsYaw(this NetPakReader reader, out Vector3 value, int bitCount = 16)
		{
			bool result = reader.ReadRadians(out float yawRadians, bitCount: bitCount);
			value = new Vector3(Mathf.Cos(yawRadians), 0.0f, Mathf.Sin(yawRadians));
			return result;
		}

		public static bool ReadVector3AsYawMagnitude(this NetPakReader reader, out Vector3 value, int yawBitCount = 16)
		{
			bool result = reader.ReadRadians(out float yawRadians, bitCount: yawBitCount);
			result &= reader.ReadFloat(out float magnitude);
			value = new Vector3(Mathf.Cos(yawRadians) * magnitude, 0.0f, Mathf.Sin(yawRadians) * magnitude);
			return result;
		}
	}
}
