////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class BoxColliderExtension
	{
		public static float GetBoxVolume(this BoxCollider collider)
		{
			Vector3 size = collider.size;
			return Mathf.Abs(size.x) * Mathf.Abs(size.y) * Mathf.Abs(size.z);
		}

		/// <summary>
		/// Transforms box center position from local space to world space.
		/// </summary>
		public static Vector3 TransformBoxCenter(this BoxCollider collider)
		{
			return collider.transform.TransformPoint(collider.center);
		}

		public static int OverlapBoxNonAlloc(this BoxCollider collider, Collider[] results, int mask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			Transform transform = collider.transform;
			return Physics.OverlapBoxNonAlloc(transform.TransformPoint(collider.center),
				collider.size * 0.5f,
				results,
				transform.rotation,
				mask,
				queryTriggerInteraction);
		}

		/// <summary>
		/// Get first overlapping hit.
		/// </summary>
		public static Collider OverlapBoxSingle(this BoxCollider collider, int mask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			int numResults = OverlapBoxNonAlloc(collider, singleResult, mask, queryTriggerInteraction);
			return numResults > 0 ? singleResult[0] : null;
		}

		private static Collider[] singleResult = new Collider[1];
	}
}
