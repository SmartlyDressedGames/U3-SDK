////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SDG.Unturned
{
	public class BezierTestComponent : MonoBehaviour
	{
		public Vector3 startVertex;
		public Vector3 startTangent;
		public Vector3 endVertex;
		public Vector3 endTangent;

		public float constantInterval;
		public float uniformInterval;
		public float smartUniformInterval;
		public float smartUniformTolerance; // if distance delta is less than this we don't adjust the step size
		public int smartUniformAttempts; // number of times to adjust step size

		public float testDistance;

		public void OnDrawGizmos()
		{
			if (constantInterval > 0.001f)
			{
				Gizmos.color = Color.red;
				float constantIndex = 0;
				while (constantIndex < 1)
				{
					float t = constantIndex;
					Vector3 constantPosition = BezierTool.getPosition(startVertex, startVertex + startTangent, endVertex + endTangent, endVertex, t);
					Gizmos.DrawWireCube(constantPosition, new Vector3(0.1f, 0.1f, 0.1f));
					constantIndex += constantInterval;
				}
			}

			if (uniformInterval > 0.001f) // interestingly this generates the exact same result as constantInterval
			{
				Gizmos.color = Color.green;
				float length = BezierTool.getLengthEstimate(startVertex, startVertex + startTangent, endVertex + endTangent, endVertex);
				int numSteps = Mathf.CeilToInt(length / uniformInterval);
				float stepSize = 1.0f / numSteps;

				int uniformIndex = 0;
				while (uniformIndex <= numSteps)
				{
					float t = uniformIndex * stepSize;
					Vector3 uniformPosition = BezierTool.getPosition(startVertex, startVertex + startTangent, endVertex + endTangent, endVertex, t);
					Gizmos.DrawWireCube(uniformPosition, new Vector3(0.1f, 0.1f, 0.1f));
					uniformIndex++;
				}
			}

			/*
			 * Best implementation so far to uniformly space points along bezier curve.
			 */
			if (smartUniformInterval > 0.001f)
			{
				Gizmos.color = Color.blue;
				float length = BezierTool.getLengthEstimate(startVertex, startVertex + startTangent, endVertex + endTangent, endVertex);
				int numEstimatedSteps = Mathf.CeilToInt(length / smartUniformInterval);
				float initialStepSize = 1.0f / numEstimatedSteps;

				float stepSize = initialStepSize;
				Vector3 prevPosition = startVertex;

				double distanceTraveledT = 0;
				while (distanceTraveledT < 1)
				{
					float t = 0;
					Vector3 realPosition = prevPosition;

					for (int trialIndex = 0; trialIndex < Mathf.Max(2, smartUniformAttempts); trialIndex++)
					{
						t = (float) (distanceTraveledT + stepSize);
						realPosition = BezierTool.getPosition(startVertex, startVertex + startTangent, endVertex + endTangent, endVertex, t);

						float stepDistance = (realPosition - prevPosition).magnitude;
						float distanceDelta = Mathf.Abs(stepDistance - smartUniformInterval);

						if (distanceDelta < smartUniformTolerance)
						{
							break; // close enough, we can stop trying
						}
						else
						{
							stepSize *= smartUniformInterval / stepDistance; // for example interval = 2 but step distance was 1 we multiply step size by 2
						}
					}

					Gizmos.DrawWireCube(realPosition, new Vector3(0.1f, 0.1f, 0.1f));

					prevPosition = realPosition;
					distanceTraveledT += stepSize;

					stepSize = initialStepSize; // reset stepSize for next loop
				}
			}

			if (testDistance > 0.001f)
			{
				float bruteForceLength = BezierTool.getLengthBruteForce(startVertex, startVertex + startTangent, endVertex + endTangent, endVertex, 0.25f, 0.01f, 4);
				float t = BezierTool.getTFromDistance(startVertex, startVertex + startTangent, endVertex + endTangent, endVertex, testDistance, 0.25f, 0.01f, 4, bruteForceLength);
				Vector3 position = BezierTool.getPosition(startVertex, startVertex + startTangent, endVertex + endTangent, endVertex, t);

				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(position, position + Vector3.up);
			}

			Handles.DrawBezier(startVertex, endVertex, startVertex + startTangent, endVertex + endTangent, Color.white, null, 1.0f);
		}
	}
}
#endif // UNITY_EDITOR