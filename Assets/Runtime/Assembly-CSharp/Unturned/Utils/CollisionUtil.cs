////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public static class CollisionUtil
	{
		private static List<Collider> getBoundsWorkingList = new List<Collider>();

		/// <summary>
		/// Find colliders in gameObject and encapsulate their bounding boxes together.
		/// </summary>
		/// <returns>True if bounds were determined, false otherwise.</returns>
		public static bool EncapsulateColliderBounds(GameObject gameObject, bool includeInactive, out Bounds bounds)
		{
			getBoundsWorkingList.Clear();
			gameObject.GetComponentsInChildren(includeInactive, getBoundsWorkingList);
			if (getBoundsWorkingList.Count > 0)
			{
				bounds = getBoundsWorkingList[0].bounds;
				for (int index = 1; index < getBoundsWorkingList.Count; ++index)
				{
					bounds.Encapsulate(getBoundsWorkingList[index].bounds);
				}
				return true;
			}
			else
			{
				bounds = default;
				return false;
			}
		}

		public static Vector3 ClosestPoint(GameObject gameObject, Vector3 position, bool includeInactive)
		{
			return ClosestPoint(gameObject, position, includeInactive, RayMasks.ALL);
		}

		/// <summary>
		/// Find colliders in gameObject and the point closest to position, otherwise use gameObject position.
		/// </summary>
		/// <param name="layerMask">Collider is only included if its layer is enabled in layer mask.</param>
		public static Vector3 ClosestPoint(GameObject gameObject, Vector3 position, bool includeInactive, int layerMask)
		{
			getBoundsWorkingList.Clear();
			gameObject.GetComponentsInChildren(includeInactive, getBoundsWorkingList);
			if (getBoundsWorkingList.Count > 0)
			{
				Vector3 result;
				if (ClosestPoint(getBoundsWorkingList, position, layerMask, out result))
				{
					return result;
				}
			}

			return gameObject.transform.position;
		}

		public static bool ClosestPoint(List<Collider> colliders, Vector3 position, int layerMask, out Vector3 result)
		{
			bool hasResult = false;
			result = default;
			float sqrLowestDistance = -1;

			foreach (Collider collider in colliders)
			{
				if (collider == null || !collider.enabled || collider.isTrigger)
					continue; // Not ideal, but on client the vehicle colliders can be destroyed.

				if (collider is MeshCollider mc)
				{
					if (!mc.convex)
						continue;
				}
				else
				{
					bool isConvexPrimitive = collider is BoxCollider || collider is SphereCollider || collider is CapsuleCollider;
					if (!isConvexPrimitive)
						continue;
				}

				int colliderMask = 1 << collider.gameObject.layer;
				if ((layerMask & colliderMask) == 0)
				{
					continue;
				}

				Vector3 testPoint = collider.ClosestPoint(position);
				float sqrDistance = (testPoint - position).sqrMagnitude;

				if (hasResult)
				{
					if (sqrDistance < sqrLowestDistance)
					{
						result = testPoint;
						sqrLowestDistance = sqrDistance;
					}
				}
				else
				{
					hasResult = true;
					result = testPoint;
					sqrLowestDistance = sqrDistance;
				}
			}

			return hasResult;
		}

		public static bool ClosestPoint(List<Collider> colliders, Vector3 position, out Vector3 result)
		{
			return ClosestPoint(colliders, position, RayMasks.ALL, out result);
		}

		public static Vector3 ClosestPoint(List<Collider> colliders, Vector3 position, int layerMask)
		{
			Vector3 result;
			if (ClosestPoint(colliders, position, layerMask, out result))
			{
				return result;
			}
			else
			{
				return position;
			}
		}

		public static Vector3 ClosestPoint(List<Collider> colliders, Vector3 position)
		{
			return ClosestPoint(colliders, position, RayMasks.ALL);
		}

		public static int OverlapBoxColliderNonAlloc(BoxCollider collider, Collider[] results, int mask, QueryTriggerInteraction queryTriggerInteraction)
		{
			return collider.OverlapBoxNonAlloc(results, mask, queryTriggerInteraction);
		}

		/// <summary>
		/// Does sphere overlap anything?
		/// </summary>
		public static bool OverlapSphere(Vector3 position, float radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			int numOverlaps = Physics.OverlapSphereNonAlloc(position, radius, results, layerMask, queryTriggerInteraction);
			return numOverlaps > 0;
		}

		private static Collider[] results = new Collider[1];
	}
}
