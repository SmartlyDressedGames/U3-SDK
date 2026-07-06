////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class Vector3Ex
	{
		public static bool IsNormalized(this Vector3 vector, float threshold = 0.001f)
		{
			return (vector.x * vector.x)
				+ (vector.y * vector.y)
				+ (vector.z * vector.z)
				- 1.0f
				< threshold * threshold;
		}

		public static bool ContainsNaN(this Vector3 vector)
		{
			return float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);
		}

		public static bool ContainsInfinity(this Vector3 vector)
		{
			return float.IsInfinity(vector.x) || float.IsInfinity(vector.y) || float.IsInfinity(vector.z);
		}

		public static bool IsFinite(this Vector3 vector)
		{
			return !vector.ContainsNaN() && !vector.ContainsInfinity();
		}

		public static bool IsNearlyZero(this Vector3 vector, float tolerance = 0.001f)
		{
			return MathfEx.IsNearlyZero(vector.x, tolerance) & MathfEx.IsNearlyZero(vector.y, tolerance) & MathfEx.IsNearlyZero(vector.z, tolerance);
		}

		public static bool IsNearlyEqual(this Vector3 vector, Vector3 other, float tolerance = 0.001f)
		{
			return MathfEx.IsNearlyEqual(vector.x, other.x, tolerance) & MathfEx.IsNearlyEqual(vector.y, other.y, tolerance) & MathfEx.IsNearlyEqual(vector.z, other.z, tolerance);
		}

		public static bool AreComponentsNearlyEqual(this Vector3 vector, float tolerance = 0.001f)
		{
			return MathfEx.IsNearlyEqual(vector.x, vector.y, tolerance) & MathfEx.IsNearlyEqual(vector.y, vector.z, tolerance);
		}

		public static Vector3 GetRoundedIfNearlyEqualToOne(this Vector3 vector, float tolerance = 0.001f)
		{
			if (MathfEx.IsNearlyEqual(vector.x, 1.0f, tolerance))
			{
				vector.x = 1.0f;
			}
			else if (MathfEx.IsNearlyEqual(vector.x, -1.0f, tolerance))
			{
				vector.x = -1.0f;
			}
			if (MathfEx.IsNearlyEqual(vector.y, 1.0f, tolerance))
			{
				vector.y = 1.0f;
			}
			else if (MathfEx.IsNearlyEqual(vector.y, -1.0f, tolerance))
			{
				vector.y = -1.0f;
			}
			if (MathfEx.IsNearlyEqual(vector.z, 1.0f, tolerance))
			{
				vector.z = 1.0f;
			}
			else if (MathfEx.IsNearlyEqual(vector.z, -1.0f, tolerance))
			{
				vector.z = -1.0f;
			}
			return vector;
		}

		/// <summary>
		/// Get a copy with absolute value of each component. 
		/// </summary>
		public static Vector3 GetAbs(this Vector3 vector)
		{
			return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
		}

		/// <summary>
		/// Get component with lowest value.
		/// </summary>
		public static float GetMin(this Vector3 vector)
		{
			return MathfEx.Min(vector.x, vector.y, vector.z);
		}

		/// <summary>
		/// Get component with highest value.
		/// </summary>
		public static float GetMax(this Vector3 vector)
		{
			return MathfEx.Max(vector.x, vector.y, vector.z);
		}

		/// <summary>
		/// Get a copy with zero on the Y axis. 
		/// </summary>
		public static Vector3 GetHorizontal(this Vector3 vector)
		{
			return new Vector3(vector.x, 0.0f, vector.z);
		}

		/// <summary>
		/// Get length along the XZ plane.
		/// </summary>
		public static float GetHorizontalMagnitude(this Vector3 vector)
		{
			return Mathf.Sqrt((vector.x * vector.x) + (vector.z * vector.z));
		}

		/// <summary>
		/// Get squared length along the XZ plane.
		/// </summary>
		public static float GetHorizontalSqrMagnitude(this Vector3 vector)
		{
			return (vector.x * vector.x) + (vector.z * vector.z);
		}

		/// <summary>
		/// Get a copy of this vector with XZ plane magnitude clamped less than maximum.
		/// </summary>
		public static Vector3 ClampHorizontalMagnitude(this Vector3 vector, float maxMagnitude)
		{
			if (maxMagnitude <= 0.0f)
			{
				return Vector3.zero;
			}

			float sqrMagnitude = vector.GetHorizontalSqrMagnitude();
			float maxSqrMagnitude = maxMagnitude * maxMagnitude;
			if (sqrMagnitude > maxSqrMagnitude)
			{
				float scale = maxMagnitude / Mathf.Sqrt(sqrMagnitude);
				return new Vector3(vector.x * scale, vector.y, vector.z * scale);
			}
			else
			{
				return vector;
			}
		}

		/// <summary>
		/// Get a copy of this vector with magnitude clamped less than maximum.
		/// </summary>
		public static Vector3 ClampMagnitude(this Vector3 vector, float maxMagnitude)
		{
			if (maxMagnitude <= 0.0f)
			{
				return Vector3.zero;
			}

			float sqrMagnitude = vector.sqrMagnitude;
			float maxSqrMagnitude = MathfEx.Square(maxMagnitude);
			if (sqrMagnitude > maxSqrMagnitude)
			{
				float magnitude = Mathf.Sqrt(sqrMagnitude);
				return vector * (maxMagnitude / magnitude);
			}
			else
			{
				return vector;
			}
		}

		/// <summary>
		/// Parse from 3 comma-delimited floats optionally surrounded by parenthesis. (e.g. "1, 2, 3" OR "(1, 2, 3)")
		/// Note: Duplicated at UnityDatEx.TryParseVector3.
		/// </summary>
		/// <returns>True if all three floats were parsed successfully, false if empty or otherwise unable to parse.</returns>
		public static bool TryParseVector3(string s, out Vector3 result)
		{
			if (string.IsNullOrEmpty(s))
			{
				result = default;
				return false;
			}

			int startIndex;
			int endIndex;

			int openingParenthesisIndex = s.IndexOf('(');
			int closingParenthesisIndex;
			if (openingParenthesisIndex >= 0)
			{
				closingParenthesisIndex = s.IndexOf(')', openingParenthesisIndex + 2);
				if (closingParenthesisIndex < 0)
				{
					result = default;
					return false;
				}

				startIndex = openingParenthesisIndex + 1;
				endIndex = closingParenthesisIndex - 1;
			}
			else
			{
				startIndex = 0;
				endIndex = s.Length - 1;
			}

			int firstDelimiterIndex = s.IndexOf(',', startIndex);
			if (firstDelimiterIndex < 0 || firstDelimiterIndex + 2 > endIndex)
			{
				result = default;
				return false;
			}

			int secondDelimiterIndex = s.IndexOf(',', firstDelimiterIndex + 2);
			if (secondDelimiterIndex < 0 || secondDelimiterIndex + 1 > endIndex)
			{
				result = default;
				return false;
			}

			if (!float.TryParse(s.Substring(startIndex, firstDelimiterIndex - startIndex), out result.x))
			{
				result = default;
				return false;
			}

			if (!float.TryParse(s.Substring(firstDelimiterIndex + 1, secondDelimiterIndex - firstDelimiterIndex - 1), out result.y))
			{
				result = default;
				return false;
			}

			if (!float.TryParse(s.Substring(secondDelimiterIndex + 1, endIndex - secondDelimiterIndex), out result.z))
			{
				result = default;
				return false;
			}

			return true;
		}
	}
}
