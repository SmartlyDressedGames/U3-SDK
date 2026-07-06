////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class SphereColliderEx
	{
		public static float GetSphereVolume(this SphereCollider collider)
		{
			return 4f / 3f * Mathf.PI * MathfEx.Cube(Mathf.Abs(collider.radius));
		}

		/// <summary>
		/// Transforms sphere center position from local space to world space.
		/// </summary>
		public static Vector3 TransformSphereCenter(this SphereCollider collider)
		{
			return collider.transform.TransformPoint(collider.center);
		}

		public static int OverlapSphereNonAlloc(this SphereCollider collider, Collider[] results, int mask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			Transform transform = collider.transform;
			return Physics.OverlapSphereNonAlloc(transform.TransformPoint(collider.center),
				collider.radius,
				results,
				mask,
				queryTriggerInteraction);
		}

		/// <summary>
		/// Get first overlapping hit.
		/// </summary>
		public static Collider OverlapSphereSingle(this SphereCollider collider, int mask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			int numResults = OverlapSphereNonAlloc(collider, singleResult, mask, queryTriggerInteraction);
			return numResults > 0 ? singleResult[0] : null;
		}

		private static Collider[] singleResult = new Collider[1];
	}
}
