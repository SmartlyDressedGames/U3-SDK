////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Landscapes;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Extensions to the built-in Physics class.
	/// 
	/// Shares similar functionality to the SDG.Framework.Utilities.PhysicsUtility class, but that should be moved here
	/// because the "framework" is unused and and the long name is annoying.
	/// </summary>
	public static class PhysicsEx
	{
		/// <summary>
		/// Wrapper that respects landscape hole volumes.
		/// </summary>
		[System.Obsolete("Hole collision is handled by Unity now")]
		public static bool Raycast(Ray ray, out RaycastHit hit, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			if ((layerMask & RayMasks.GROUND) == RayMasks.GROUND)
			{
				LandscapeHoleUtility.raycastIgnoreLandscapeIfNecessary(ray, maxDistance, ref layerMask);
			}
			return Physics.Raycast(ray, out hit, maxDistance, layerMask, queryTriggerInteraction);
		}

		/// <summary>
		/// Wrapper that respects landscape hole volumes.
		/// </summary>
		[System.Obsolete("Hole collision is handled by Unity now")]
		public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			if ((layerMask & RayMasks.GROUND) == RayMasks.GROUND)
			{
				LandscapeHoleUtility.raycastIgnoreLandscapeIfNecessary(new Ray(origin, direction), maxDistance, ref layerMask);
			}
			return Physics.Raycast(origin, direction, out hit, maxDistance, layerMask, queryTriggerInteraction);
		}

		/// <summary>
		/// Wrapper that respects landscape hole volumes.
		/// </summary>
		[System.Obsolete("Hole collision is handled by Unity now")]
		public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			if ((layerMask & RayMasks.GROUND) == RayMasks.GROUND)
			{
				LandscapeHoleUtility.spherecastIgnoreLandscapeIfNecessary(new Ray(origin, direction), radius, maxDistance, ref layerMask);
			}
			return Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
		}

		/// <summary>
		/// Wrapper that respects landscape hole volumes.
		/// </summary>
		[System.Obsolete("Hole collision is handled by Unity now")]
		public static bool SphereCast(Ray ray, float radius, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			if ((layerMask & RayMasks.GROUND) == RayMasks.GROUND)
			{
				LandscapeHoleUtility.spherecastIgnoreLandscapeIfNecessary(ray, radius, maxDistance, ref layerMask);
			}
			return Physics.SphereCast(ray, radius, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
		}
	}
}
