////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Landscapes;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Utilities
{
	public class PhysicsUtility
	{
		[System.Obsolete("Hole collision is handled by Unity now")]
		public static bool raycast(Ray ray, out RaycastHit hit, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			if ((layerMask & RayMasks.GROUND) == RayMasks.GROUND)
			{
				LandscapeHoleUtility.raycastIgnoreLandscapeIfNecessary(ray, maxDistance, ref layerMask);
			}
			return Physics.Raycast(ray, out hit, maxDistance, layerMask, queryTriggerInteraction);
		}

		[System.Obsolete("Hole collision is handled by Unity now")]
		public static RaycastHit[] raycastAll(Ray ray, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			if ((layerMask & RayMasks.GROUND) == RayMasks.GROUND)
			{
				LandscapeHoleUtility.raycastIgnoreLandscapeIfNecessary(ray, maxDistance, ref layerMask);
			}
			return Physics.RaycastAll(ray, maxDistance, layerMask, queryTriggerInteraction);
		}

		[System.Obsolete("Hole collision is handled by Unity now")]
		public static int sphereCastNonAlloc(Ray ray, float radius, RaycastHit[] results, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			if ((layerMask & RayMasks.GROUND) == RayMasks.GROUND)
			{
				LandscapeHoleUtility.spherecastIgnoreLandscapeIfNecessary(ray, radius, maxDistance, ref layerMask);
			}
			return Physics.SphereCastNonAlloc(ray, radius, results, maxDistance, layerMask, queryTriggerInteraction);
		}
	}
}
