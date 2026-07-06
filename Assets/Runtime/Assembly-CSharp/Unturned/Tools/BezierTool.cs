////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class BezierTool
	{
		/// <param name="a">Start Vertex</param>
		/// <param name="b">Start Vertex + Start Tangent</param>
		/// <param name="c">End Vertex + End Tangent</param>
		/// <param name="d">End Vertex</param>
		public static Vector3 getPosition(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
		{
			t = Mathf.Clamp01(t);
			float t2 = 1.0f - t;

			return (t2 * t2 * t2 * a) + (3.0f * t2 * t2 * t * b) + (3.0f * t2 * t * t * c) + (t * t * t * d);
		}

		/// <param name="a">Start Vertex</param>
		/// <param name="b">Start Vertex + Start Tangent</param>
		/// <param name="c">End Vertex + End Tangent</param>
		/// <param name="d">End Vertex</param>
		public static Vector3 getVelocity(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
		{
			t = Mathf.Clamp01(t);
			float t2 = 1.0f - t;

			return (3.0f * t2 * t2 * (b - a)) + (6.0f * t2 * t * (c - b)) + (3.0f * t * t * (d - c));
		}

		/// <param name="a">Start Vertex</param>
		/// <param name="b">Start Vertex + Start Tangent</param>
		/// <param name="c">End Vertex + End Tangent</param>
		/// <param name="d">End Vertex</param>
		public static float getLengthEstimate(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
		{
			return ((d - a).magnitude + (d - c).magnitude + (b - c).magnitude + (b - a).magnitude) / 2.0f;
		}

		/// <param name="a">Start Vertex</param>
		/// <param name="b">Start Vertex + Start Tangent</param>
		/// <param name="c">End Vertex + End Tangent</param>
		/// <param name="d">End Vertex</param>
		/// <param name="distance">World units length along curve.</param>
		/// <param name="uniformInterval">Spacing between points.</param>
		/// <param name="intervalTolerance">Max estimate distance from uniform interval before we have to retry.</param>
		/// <param name="attempts">How many times to retry if the estimate is too far off.</param>
		/// <param name="cachedLength">If length is already known pass it in, otherwise it's recalculated.</param>
		/// <returns>Time along curve. [0-1]</returns>
		public static float getTFromDistance(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float distance, float uniformInterval, float intervalTolerance, int attempts, float cachedLength = -1)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Get T From Distance");

			if (distance < 0)
			{
				UnityEngine.Profiling.Profiler.EndSample();
				return 0; // From before this curve so sample 0
			}

			UnityEngine.Profiling.Profiler.BeginSample("Calculate Length");
			// Calculate length first so we can use it for early exit
			float length;
			if (cachedLength < 0)
			{
				length = getLengthEstimate(a, b, c, d);
			}
			else
			{
				length = cachedLength;
			}
			UnityEngine.Profiling.Profiler.EndSample();

			if (distance >= length)
			{
				UnityEngine.Profiling.Profiler.EndSample();
				return 1; // Early exit we passed the end of the curve
			}

			attempts = Mathf.Max(1, attempts); // At least 1 attempt needed

			int numEstimatedSteps = Mathf.CeilToInt(length / uniformInterval);
			float initialStepSize = 1.0f / numEstimatedSteps;

			float stepSize = initialStepSize;
			float stepDistance = uniformInterval;

			Vector3 prevPosition = a;
			double distanceTraveledWorld = 0;
			double distanceTraveledT = 0;

			while (distanceTraveledT < 1)
			{
				// These default values will never be used but intellisense isn't smart enough to know that
				float t = 0;
				Vector3 realPosition = prevPosition;

				for (int trialIndex = 0; trialIndex < attempts; trialIndex++)
				{
					t = (float) (distanceTraveledT + stepSize);
					UnityEngine.Profiling.Profiler.BeginSample("Sample Curve");
					realPosition = getPosition(a, a + b, d + c, d, t);
					UnityEngine.Profiling.Profiler.EndSample();
					UnityEngine.Profiling.Profiler.BeginSample("Measure Distance");
					stepDistance = (realPosition - prevPosition).magnitude;
					UnityEngine.Profiling.Profiler.EndSample();

					if (trialIndex < attempts - 1) // No point changing stepSize if this is the last hurrah
					{
						float distanceDelta = Mathf.Abs(stepDistance - uniformInterval);

						if (distanceDelta < intervalTolerance)
						{
							break; // Close enough, we can stop trying
						}
						else
						{
							stepSize *= intervalTolerance / stepDistance; // For example interval = 2 but step distance was 1 we multiply step size by 2
						}
					}
				}

				double world = distanceTraveledWorld + stepDistance;
				if (distance >= distanceTraveledWorld && distance <= world)
				{
					float windowScale = (float) ((distance - distanceTraveledWorld) / stepDistance); // [0-1] how far into this step the distance was
					windowScale *= stepSize;
					UnityEngine.Profiling.Profiler.EndSample();
					return (float) (distanceTraveledT + windowScale);
				}

				prevPosition = realPosition;
				distanceTraveledWorld += stepDistance;
				distanceTraveledT += stepSize;

				stepSize = initialStepSize; // reset stepSize for next loop
			}

			UnturnedLog.warn("Failed to find T along curve from distance!\nDistance: " + distance + " Length: " + length + "\nInterval: " + uniformInterval + " Tolerance: " + intervalTolerance + " Attempts: " + attempts);
			UnityEngine.Profiling.Profiler.EndSample();
			return 0.5f; // Obviously wrong value
		}

		/// <param name="a">Start Vertex</param>
		/// <param name="b">Start Vertex + Start Tangent</param>
		/// <param name="c">End Vertex + End Tangent</param>
		/// <param name="d">End Vertex</param>
		/// <param name="uniformInterval">Spacing between points.</param>
		/// <param name="intervalTolerance">Max estimate distance from uniform interval before we have to retry.</param>
		/// <param name="attempts">How many times to retry if the estimate is too far off.</param>
		public static float getLengthBruteForce(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float uniformInterval, float intervalTolerance, int attempts)
		{
			float lengthEstimate = getLengthEstimate(a, b, c, d);

			attempts = Mathf.Max(1, attempts); // At least 1 attempt needed

			int numEstimatedSteps = Mathf.CeilToInt(lengthEstimate / uniformInterval);
			float initialStepSize = 1.0f / numEstimatedSteps;

			float stepSize = initialStepSize;
			float stepDistance = uniformInterval;

			Vector3 prevPosition = a;
			double distanceTraveledWorld = 0;
			double distanceTraveledT = 0;

			while (distanceTraveledT < 1)
			{
				// These default values will never be used but intellisense isn't smart enough to know that
				float t = 0;
				Vector3 realPosition = prevPosition;

				for (int trialIndex = 0; trialIndex < attempts; trialIndex++)
				{
					t = (float) (distanceTraveledT + stepSize);
					realPosition = getPosition(a, a + b, d + c, d, t);
					stepDistance = (realPosition - prevPosition).magnitude;

					if (trialIndex < attempts - 1)
					{
						float distanceDelta = Mathf.Abs(stepDistance - uniformInterval);

						if (distanceDelta < intervalTolerance)
						{
							break; // Close enough, we can stop trying
						}
						else if (trialIndex < attempts - 1) // No point changing stepSize if this is the last hurrah
						{
							stepSize *= intervalTolerance / stepDistance; // For example interval = 2 but step distance was 1 we multiply step size by 2
						}
					}
				}

				UnturnedLog.info(stepDistance);
				prevPosition = realPosition;
				distanceTraveledWorld += stepDistance;
				distanceTraveledT += stepSize;

				stepSize = initialStepSize; // reset stepSize for next loop
			}

			return (float) distanceTraveledWorld;
		}
	}
}