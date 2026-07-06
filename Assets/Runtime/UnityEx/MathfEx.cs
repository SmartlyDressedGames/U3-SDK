////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Extensions to Unity's built-in Mathf class.
	/// </summary>
	public static class MathfEx
	{
		public const float TAU = Mathf.PI * 2.0f;
		public const float HALF_PI = Mathf.PI * 0.5f;

		/// <summary>
		/// Clamps each component of vector into range.
		/// </summary>
		public static Vector3 Clamp(Vector3 value, float min, float max)
		{
			value.x = Mathf.Clamp(value.x, min, max);
			value.y = Mathf.Clamp(value.y, min, max);
			value.z = Mathf.Clamp(value.z, min, max);
			return value;
		}

		public static bool IsNearlyEqual(float a, float b, float tolerance = 0.01f)
		{
			return Mathf.Abs(b - a) < tolerance;
		}

		public static bool IsAngleDegreesNearlyEqual(float a, float b, float tolerance = 0.1f)
		{
			return Mathf.Abs(Mathf.DeltaAngle(a, b)) < tolerance;
		}

		public static bool IsNearlyZero(float x, float tolerance = 0.01f)
		{
			return Mathf.Abs(x) < tolerance;
		}

		public static bool IsNearlyEqual(Color a, Color b, float tolerance = 0.002f)
		{
			return IsNearlyEqual(a.r, b.r, tolerance: tolerance)
				&& IsNearlyEqual(a.g, b.g, tolerance: tolerance)
				&& IsNearlyEqual(a.b, b.b, tolerance: tolerance)
				&& IsNearlyEqual(a.a, b.a, tolerance: tolerance);
		}

		public static bool IsNearlyEqual(Vector3 a, Vector3 b, float tolerance = 0.001f)
		{
			return IsNearlyEqual(a.x, b.x, tolerance: tolerance)
				&& IsNearlyEqual(a.y, b.y, tolerance: tolerance)
				&& IsNearlyEqual(a.z, b.z, tolerance: tolerance);
		}

		public static bool IsNearlyEqual(Quaternion a, Quaternion b, float tolerance = 0.001f)
		{
			return IsNearlyEqual(a.x, b.x, tolerance: tolerance)
				&& IsNearlyEqual(a.y, b.y, tolerance: tolerance)
				&& IsNearlyEqual(a.z, b.z, tolerance: tolerance)
				&& IsNearlyEqual(a.w, b.w, tolerance: tolerance);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Square(float x)
		{
			return x * x;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Cube(float x)
		{
			return x * x * x;
		}

		/// <summary>
		/// Distance on the XZ plane between two points.
		/// </summary>
		public static float HorizontalDistanceSquared(Vector3 a, Vector3 b)
		{
			return Square(a.x - b.x) + Square(a.z - b.z);
		}

		public static byte RoundAndClampToByte(float value)
		{
			return (byte) Mathf.Min(Mathf.RoundToInt(value), byte.MaxValue);
		}

		public static sbyte RoundAndClampToSByte(float value)
		{
			return (sbyte) Mathf.Clamp(Mathf.RoundToInt(value), sbyte.MinValue, sbyte.MaxValue);
		}

		public static ushort RoundAndClampToUShort(float value)
		{
			return (ushort) Mathf.Clamp(Mathf.RoundToInt(value), ushort.MinValue, ushort.MaxValue);
		}

		public static short RoundAndClampToShort(float value)
		{
			return (short) Mathf.Clamp(Mathf.RoundToInt(value), short.MinValue, short.MaxValue);
		}

		public static uint RoundAndClampToUInt(float value)
		{
			int intValue = Mathf.RoundToInt(value);
			return intValue < 0 ? 0 : (uint) intValue;
		}

		public static Vector2 Clamp01(Vector2 value)
		{
			return new Vector2(Mathf.Clamp01(value.x), Mathf.Clamp01(value.y));
		}

		public static Color Clamp01(Color value)
		{
			return new Color(Mathf.Clamp01(value.r), Mathf.Clamp01(value.g), Mathf.Clamp01(value.b), Mathf.Clamp01(value.a));
		}

		public static ushort Min(ushort a, ushort b)
		{
			return (ushort) Mathf.Min(a, b);
		}

		public static byte Clamp(byte value, byte min, byte max)
		{
			return (byte) Mathf.Clamp(value, min, max);
		}

		public static ushort Clamp(ushort value, ushort min, ushort max)
		{
			return (ushort) Mathf.Clamp(value, min, max);
		}

		/// <summary>
		/// Minimum of three floats. Does not allocate an array unlike the Unity built-in.
		/// </summary>
		public static float Min(float a, float b, float c)
		{
			return Mathf.Min(Mathf.Min(a, b), c);
		}

		/// <summary>
		/// Maximum of three floats. Does not allocate an array unlike the Unity built-in.
		/// </summary>
		public static float Max(float a, float b, float c)
		{
			return Mathf.Max(Mathf.Max(a, b), c);
		}

		public static uint Min(uint a, uint b)
		{
			return a < b ? a : b;
		}

		public static byte Min(byte a, byte b)
		{
			return a < b ? a : b;
		}

		public static byte Max(byte a, byte b)
		{
			return a > b ? a : b;
		}

		public static uint Max(uint a, uint b)
		{
			return a > b ? a : b;
		}

		public static byte ClampToByte(int value)
		{
			const int min = byte.MinValue;
			const int max = byte.MaxValue;
			return (byte) Mathf.Clamp(value, min, max);
		}

		public static short ClampToShort(int value)
		{
			const int min = short.MinValue;
			const int max = short.MaxValue;
			return (short) Mathf.Clamp(value, min, max);
		}

		public static ushort ClampToUShort(int value)
		{
			const int min = ushort.MinValue;
			const int max = ushort.MaxValue;
			return (ushort) Mathf.Clamp(value, min, max);
		}

		public static uint ClampToUInt(int value)
		{
			const int min = 0;
			const int max = int.MaxValue;
			return (uint) Mathf.Clamp(value, min, max);
		}

		public static int ClampLongToInt(long value, int min, int max)
		{
			return value < min ? min : (value > max ? max : (int) value);
		}

		public static int ClampLongToInt(long value)
		{
			return ClampLongToInt(value, int.MinValue, int.MaxValue);
		}

		public static uint ClampLongToUInt(long value)
		{
			const long min = uint.MinValue;
			const long max = uint.MaxValue;
			return value < min ? uint.MinValue : (value > max ? uint.MaxValue : (uint) value);
		}

		public static int TruncateToInt(float value)
		{
			if (value >= 0.0f)
			{
				return Mathf.FloorToInt(value);
			}
			else
			{
				return Mathf.CeilToInt(value);
			}
		}

		public static ushort CeilToUShort(float value)
		{
			return ClampToUShort(Mathf.CeilToInt(value));
		}

		public static uint CeilToUInt(float value)
		{
			return ClampToUInt(Mathf.CeilToInt(value));
		}

		public static Vector2 RandomPositionInCircle(float radius)
		{
			return Random.insideUnitCircle * radius;
		}

		public static Vector3 RandomPositionInCircleY(Vector3 center, float radius)
		{
			Vector2 position = RandomPositionInCircle(radius);
			return new Vector3(center.x + position.x, center.y, center.z + position.y);
		}

		/// <summary>
		/// I often find myself looking back for this particular equation, so decided to stick it in here.
		/// Sort of a ceiled integer division.
		/// Credit to rjmunro under CC BY 4.0 license here: https://stackoverflow.com/a/503201/8491721
		/// (paraphrased)
		/// </summary>
		public static int GetPageCount(int itemCount, int itemsPerPage)
		{
			return ((itemCount - 1) / itemsPerPage) + 1;
		}

		/// <summary>
		/// "Skew Lines" nearest point.
		/// Calculate distance along ray1 of nearest point to ray0.
		/// For example ray0 is the mouse and ray1 is a gizmo handle and we want to know how far the mouse moved along the handle.
		/// </summary>
		public static float ProjectRayOntoRay(Vector3 origin0, Vector3 direction0, Vector3 origin1, Vector3 direction1)
		{
			Vector3 n = Vector3.Cross(direction1, direction0);
			Vector3 n2 = Vector3.Cross(direction0, n);
			return Vector3.Dot(origin0 - origin1, n2) / Vector3.Dot(direction1, n2);
		}

		/// <summary>
		/// Distance between nearest points of two rays.
		/// </summary>
		public static float DistanceBetweenRays(Vector3 origin0, Vector3 direction0, Vector3 origin1, Vector3 direction1)
		{
			Vector3 n = Vector3.Cross(direction0, direction1).normalized;
			return Mathf.Abs(Vector3.Dot(n, origin1 - origin0));
		}

		public static Vector3 NearestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
		{
			// https://en.wikipedia.org/wiki/Vector_projection
			Vector3 endRelativeToStart = lineEnd - lineStart;
			Vector3 pointRelativeToStart = point - lineStart;
			float t = Vector3.Dot(endRelativeToStart, pointRelativeToStart) / endRelativeToStart.sqrMagnitude;
			// By default Lerp is clamped.
			return Vector3.Lerp(lineStart, lineEnd, t);
		}

		public static Vector3 NearestPointOnCircle(Vector3 center, Vector3 normal, float radius, Vector3 point)
		{
			Vector3 pointRelativeToCenter = point - center;
			Vector3 localPointOnPlane = Vector3.ProjectOnPlane(pointRelativeToCenter, normal);
			Vector3 directionFromCenter = localPointOnPlane.normalized;
			return center + (directionFromCenter * radius);
		}

		/// <summary>
		/// Component-wise inverse lerp.
		/// </summary>
		public static Vector3 InverseLerp(Vector3 a, Vector3 b, Vector3 value)
		{
			return new Vector3(Mathf.InverseLerp(a.x, b.x, value.x),
				Mathf.InverseLerp(a.y, b.y, value.y),
				Mathf.InverseLerp(a.z, b.z, value.z));
		}

		/// <summary>
		/// Trust that 't' is already in the [0, 1] range.
		/// </summary>
		public static float SmoothStep01(float t)
		{
			return t * t * (3.0f - (2.0f * t));
		}

		/// <summary>
		/// Trust that 't' is already in the [0, 1] range.
		/// Quoting from the wikipedia page: https://en.wikipedia.org/wiki/Smoothstep
		/// "Ken Perlin suggested an improved version of the commonly used first-order smoothstep function,
		/// equivalent to the second order of its general form. It has zero 1st- and 2nd-order derivatives
		/// at x = 0 and x = 1"
		/// </summary>
		public static float SmootherStep01(float t)
		{
			return t * t * t * ((t * ((t * 6.0f) - 15.0f)) + 10.0f);
		}
	}
}
