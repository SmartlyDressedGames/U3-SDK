////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using SDG.Unturned; // QuaternionEx

namespace SDG.NetPak
{
	public static class UnityNetPakWriterEx
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
		public static bool WriteQuaternion(this NetPakWriter writer, Quaternion value, int bitsPerComponent = 9)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (!value.IsNormalized())
				throw new System.ArgumentException("quaternion should be normalized", "value");
#endif // WITH_NETPAK_EXCEPTIONS

			int largestComponentIndex = 0;
			float largestComponentValue;
			float largestComponentSign;

			if (value.x < 0.0f)
			{
				largestComponentValue = -value.x;
				largestComponentSign = -1.0f;
			}
			else
			{
				largestComponentValue = value.x;
				largestComponentSign = 1.0f;
			}

			for (int componentIndex = 1; componentIndex < 4; ++componentIndex)
			{
				float componentValue = value[componentIndex];
				if (componentValue < 0.0f)
				{
					componentValue = -componentValue;
					if (componentValue > largestComponentValue)
					{
						largestComponentIndex = componentIndex;
						largestComponentValue = componentValue;
						largestComponentSign = -1.0f;
					}
				}
				else
				{
					if (componentValue > largestComponentValue)
					{
						largestComponentIndex = componentIndex;
						largestComponentValue = componentValue;
						largestComponentSign = +1.0f;
					}
				}
			}

			float value0;
			float value1;
			float value2;
			switch (largestComponentIndex)
			{
				case 0:
					value0 = value.y;
					value1 = value.z;
					value2 = value.w;
					break;

				case 1:
					value0 = value.x;
					value1 = value.z;
					value2 = value.w;
					break;

				case 2:
					value0 = value.x;
					value1 = value.y;
					value2 = value.w;
					break;

				case 3:
					value0 = value.x;
					value1 = value.y;
					value2 = value.z;
					break;

				default:
#if WITH_NETPAK_EXCEPTIONS
					throw new System.Exception("invalid largest component index");
#else
					return false;
#endif // WITH_NETPAK_EXCEPTIONS
			}

			bool result = writer.WriteBits((uint) largestComponentIndex, 2);
			result &= writer.WriteSignedNormalizedFloat(value0 * largestComponentSign * NetPakConst.SQRT_OF_TWO, bitsPerComponent);
			result &= writer.WriteSignedNormalizedFloat(value1 * largestComponentSign * NetPakConst.SQRT_OF_TWO, bitsPerComponent);
			result &= writer.WriteSignedNormalizedFloat(value2 * largestComponentSign * NetPakConst.SQRT_OF_TWO, bitsPerComponent);
			return result;
		}

		/// <summary>
		/// Similar to the quaternion optimization, but needs a sign bit for the largest value.
		/// </summary>
		public static bool WriteNormalVector3(this NetPakWriter writer, Vector3 value, int bitsPerComponent = 9)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (!value.IsNormalized())
				throw new System.ArgumentException("vector should be normalized", "value");
#endif // WITH_NETPAK_EXCEPTIONS

			int largestComponentIndex = 0;
			float largestComponentAbsValue;
			bool largestComponentIsNegative;

			if (value.x < 0.0f)
			{
				largestComponentAbsValue = -value.x;
				largestComponentIsNegative = true;
			}
			else
			{
				largestComponentAbsValue = value.x;
				largestComponentIsNegative = false;
			}

			for (int componentIndex = 1; componentIndex < 3; ++componentIndex)
			{
				float componentValue = value[componentIndex];
				if (componentValue < 0.0f)
				{
					componentValue = -componentValue;
					if (componentValue > largestComponentAbsValue)
					{
						largestComponentIndex = componentIndex;
						largestComponentAbsValue = componentValue;
						largestComponentIsNegative = true;
					}
				}
				else
				{
					if (componentValue > largestComponentAbsValue)
					{
						largestComponentIndex = componentIndex;
						largestComponentAbsValue = componentValue;
						largestComponentIsNegative = false;
					}
				}
			}

			float value0;
			float value1;
			switch (largestComponentIndex)
			{
				case 0:
					value0 = value.y;
					value1 = value.z;
					break;

				case 1:
					value0 = value.x;
					value1 = value.z;
					break;

				case 2:
					value0 = value.x;
					value1 = value.y;
					break;

				default:
#if WITH_NETPAK_EXCEPTIONS
					throw new System.Exception("invalid largest component index");
#else
					return false;
#endif // WITH_NETPAK_EXCEPTIONS
			}

			bool result = writer.WriteBits((uint) largestComponentIndex, 2);
			result &= writer.WriteBit(largestComponentIsNegative);
			result &= writer.WriteSignedNormalizedFloat(value0 * NetPakConst.SQRT_OF_TWO, bitsPerComponent);
			result &= writer.WriteSignedNormalizedFloat(value1 * NetPakConst.SQRT_OF_TWO, bitsPerComponent);
			return result;
		}

		/// <summary>
		/// Default intBitCount of 13 allows a range of [-4096, +4096).
		/// </summary>
		public static bool WriteClampedVector3(this NetPakWriter writer, Vector3 value, int intBitCount = 13, int fracBitCount = 7)
		{
			bool result = writer.WriteClampedFloat(value.x, intBitCount, fracBitCount);
			result &= writer.WriteClampedFloat(value.y, intBitCount, fracBitCount);
			result &= writer.WriteClampedFloat(value.z, intBitCount, fracBitCount);
			return result;
		}

		/// <summary>
		/// Write 8-bit per channel color excluding alpha.
		/// </summary>
		public static bool WriteColor32RGB(this NetPakWriter writer, Color32 value)
		{
			bool result = writer.WriteUInt8(value.r);
			result &= writer.WriteUInt8(value.g);
			result &= writer.WriteUInt8(value.b);
			return result;
		}

		/// <summary>
		/// Write 8-bit per channel color including alpha.
		/// </summary>
		public static bool WriteColor32RGBA(this NetPakWriter writer, Color32 value)
		{
			bool result = writer.WriteUInt8(value.r);
			result &= writer.WriteUInt8(value.g);
			result &= writer.WriteUInt8(value.b);
			result &= writer.WriteUInt8(value.a);
			return result;
		}

		/// <summary>
		/// Note: "Special" here refers to the -90 rotation on the X axis. :)
		/// If quaternion is only a rotation around the Y axis (yaw) which is common for barricades and structures,
		/// write only yaw. Otherwise, write full quaternion.
		/// </summary>
		public static bool WriteSpecialYawOrQuaternion(this NetPakWriter writer, Quaternion value, int yawBitCount = 9, int quaternionBitsPerComponent = 9)
		{
			bool result;
			// Nelson 2024-06-26: Quaternion.eulerAngles.y isn't necessarily the yaw anymore. Please refer to
			// HousingConnections.GetModelYaw for more details.
			Vector3 z = value * Vector3.forward;
			bool isOnlyRotatedAroundYAxis = z.y > 0.9999f;
			if (isOnlyRotatedAroundYAxis)
			{
				result = writer.WriteBit(true);
				Vector3 y = value * Vector3.up;
				Vector2 direction = new Vector2(-y.z, -y.x).normalized;
				float yawRadians = Mathf.Atan2(direction.y, direction.x);
				result &= writer.WriteRadians(yawRadians, yawBitCount);
			}
			else
			{
				result = writer.WriteBit(false);
				result &= writer.WriteQuaternion(value, quaternionBitsPerComponent);
			}
			return result;
		}

		public static bool WriteNormalVector3AsYaw(this NetPakWriter writer, Vector3 value, int bitCount = 16)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (!value.IsNormalized())
				throw new System.ArgumentException("vector should be normalized", "value");
			if (!Mathf.Approximately(value.y, 0.0f))
				throw new System.ArgumentException("value.y should be zero", "value");
#endif // WITH_NETPAK_EXCEPTIONS

			float yawRadians = Mathf.Atan2(value.z, value.x);
			return writer.WriteRadians(yawRadians, bitCount);
		}

		public static bool WriteVector3AsYawMagnitude(this NetPakWriter writer, Vector3 value, int yawBitCount = 16)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (!Mathf.Approximately(value.y, 0.0f))
				throw new System.ArgumentException("value.y should be zero", "value");
#endif // WITH_NETPAK_EXCEPTIONS

			bool result;
			float yawRadians = Mathf.Atan2(value.z, value.x);
			float magnitude = value.magnitude;
			result = writer.WriteRadians(yawRadians, yawBitCount);
			result &= writer.WriteFloat(magnitude);
			return result;
		}
	}
}
