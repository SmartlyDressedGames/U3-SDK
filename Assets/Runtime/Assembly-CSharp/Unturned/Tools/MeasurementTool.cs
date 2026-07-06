////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MeasurementTool
	{
		public static float speedToKPH(float speed)
		{
			return speed * 3.6f;
		}

		public static float KPHToMPH(float kph)
		{
			return kph / 1.609344f;
		}

		public static float KtoM(float k)
		{
			return k * 0.621371f;
		}

		public static float MtoYd(float m)
		{
			return m * 1.09361f;
		}

		public static int MtoYd(int m)
		{
			return (int) (m * 1.09361f);
		}

		public static long MtoYd(long m)
		{
			return (long) (m * 1.09361f);
		}

		public static string FormatLengthString(float length)
		{
			if (OptionsSettings.metric)
			{
				return $"{Mathf.RoundToInt(length)} m";
			}
			else
			{
				return $"{Mathf.RoundToInt(MtoYd(length))} yd";
			}
		}

		public static byte angleToByte(float angle)
		{
			if (angle < 0)
			{
				return (byte) ((360 + (angle % 360)) / 2.0f);
			}
			else
			{
				return (byte) (angle % 360 / 2);
			}
		}

		public static float byteToAngle(byte angle)
		{
			return angle * 2.0f;
		}

		[System.Obsolete("Newer code should not be using this, instead NetPak should handle it.")]
		public static byte angleToByte2(float angle)
		{
			if (angle < 0)
			{
				return (byte) ((360 + (angle % 360)) / 1.5f);
			}
			else
			{
				return (byte) (angle % 360 / 1.5f);
			}
		}

		[System.Obsolete("Newer code should not be using this, instead NetPak should handle it.")]
		public static float byteToAngle2(byte angle)
		{
			return angle * 1.5f;
		}
	}
}
