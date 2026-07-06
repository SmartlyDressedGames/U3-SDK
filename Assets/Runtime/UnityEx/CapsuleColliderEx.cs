////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class CapsuleColliderEx
	{
		public static float GetCapsuleVolume(this CapsuleCollider collider)
		{
			float r = Mathf.Abs(collider.radius);
			float PI_r2 = Mathf.PI * r * r;
			float PI_r3 = PI_r2 * r;
			return (PI_r2 * collider.height) + (4f / 3f * PI_r3);
		}

		/// <summary>
		/// Transforms capsule center position from local space to world space.
		/// </summary>
		public static Vector3 TransformCapsuleCenter(this CapsuleCollider collider)
		{
			return collider.transform.TransformPoint(collider.center);
		}

		/// <summary>
		/// Convert direction enum into local-space unit vector.
		/// </summary>
		public static Vector3 GetCapsuleLocalDirection(this CapsuleCollider collider)
		{
			switch (collider.direction)
			{
				case 0: // X
					return new Vector3(1.0f, 0.0f, 0.0f);

				case 1: // Y
					return new Vector3(0.0f, 1.0f, 0.0f);

				case 2: // Z
					return new Vector3(0.0f, 0.0f, 1.0f);

				default:
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					throw new System.Exception($"Capsule collider {collider.GetSceneHierarchyPath()} has invalid direction {collider.direction}");
#else
					return Vector3.up; // Unit vector to avoid breaking things.
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			}
		}

		/// <summary>
		/// Get centers of the capsule hemispheres.
		/// </summary>
		public static void GetCapsulePoints(this CapsuleCollider collider, out Vector3 point0, out Vector3 point1)
		{
			Transform transform = collider.transform;
			Vector3 direction = collider.GetCapsuleLocalDirection();
			float halfHeightMinusRadius = (collider.height * 0.5f) - collider.radius;
			point0 = transform.TransformPoint(collider.center - (direction * halfHeightMinusRadius));
			point1 = transform.TransformPoint(collider.center + (direction * halfHeightMinusRadius));
		}

		public static int OverlapCapsuleNonAlloc(this CapsuleCollider collider, Collider[] results, int mask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			Vector3 point0;
			Vector3 point1;
			GetCapsulePoints(collider, out point0, out point1);
			return Physics.OverlapCapsuleNonAlloc(point0,
				point1,
				collider.radius,
				results,
				mask,
				queryTriggerInteraction);
		}

		/// <summary>
		/// Get first overlapping hit.
		/// </summary>
		public static Collider OverlapCapsuleSingle(this CapsuleCollider collider, int mask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			int numResults = OverlapCapsuleNonAlloc(collider, singleResult, mask, queryTriggerInteraction);
			return numResults > 0 ? singleResult[0] : null;
		}

		private static Collider[] singleResult = new Collider[1];
	}
}
