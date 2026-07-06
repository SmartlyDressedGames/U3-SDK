////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Framework.Landscapes
{
	public class LandscapeHoleUtility
	{
		[System.Obsolete("New code should probably be using Landscape.IsPointInsideHole")]
		public static bool isPointInsideHoleVolume(Vector3 point)
		{
			LandscapeHoleVolume volume;
			return isPointInsideHoleVolume(point, out volume);
		}

		[System.Obsolete("New code should probably be using Landscape.IsPointInsideHole")]
		public static bool isPointInsideHoleVolume(Vector3 point, out LandscapeHoleVolume volume)
		{
			List<LandscapeHoleVolume> volumeList = LandscapeHoleVolumeManager.Get().InternalGetAllVolumes();
			for (int volumeIndex = 0; volumeIndex < volumeList.Count; volumeIndex++)
			{
				volume = volumeList[volumeIndex];
				if (isPointInsideHoleVolume(volume, point))
				{
					return true;
				}
			}

			volume = null;
			return false;
		}

		[System.Obsolete]
		public static bool isPointInsideHoleVolume(LandscapeHoleVolume volume, Vector3 point)
		{
			return volume.IsPositionInsideVolume(point);
		}

		[System.Obsolete]
		public static bool doesRayIntersectHoleVolume(Ray ray, out RaycastHit hit, out LandscapeHoleVolume volume, float maxDistance)
		{
			return LandscapeHoleVolumeManager.Get().Raycast(ray, out hit, out volume, maxDistance);
		}

		[System.Obsolete]
		public static bool doesRayIntersectHoleVolume(LandscapeHoleVolume volume, Ray ray, out RaycastHit hit, float maxDistance)
		{
			return volume.volumeCollider.Raycast(ray, out hit, maxDistance);
		}

		[System.Obsolete("Hole collision is handled by Unity now")]
		public static bool shouldRaycastIgnoreLandscape(Ray ray, float maxDistance)
		{
			LandscapeHoleVolume volume;
			RaycastHit volumeHit;
			if (doesRayIntersectHoleVolume(ray, out volumeHit, out volume, maxDistance))
			{
				RaycastHit landscapeHit;
				if (Physics.Raycast(ray, out landscapeHit, maxDistance, RayMasks.GROUND))
				{
					if (isPointInsideHoleVolume(volume, landscapeHit.point))
					{
						return true;
					}
				}
			}

			if (isPointInsideHoleVolume(ray.origin, out volume))
			{
				RaycastHit landscapeHit;
				if (Physics.Raycast(ray, out landscapeHit, maxDistance, RayMasks.GROUND))
				{
					if (isPointInsideHoleVolume(volume, landscapeHit.point))
					{
						return true;
					}
				}
			}

			return false;
		}

		[System.Obsolete("Hole collision is handled by Unity now")]
		public static bool shouldSpherecastIgnoreLandscape(Ray ray, float radius, float maxDistance)
		{
			// Account for the extended length of sphere, but not the width because we have no collider-sphere code.
			ray.origin -= ray.direction * radius;
			maxDistance += radius * 2.0f;
			return shouldRaycastIgnoreLandscape(ray, maxDistance);
		}

		[System.Obsolete("Hole collision is handled by Unity now")]
		public static void raycastIgnoreLandscapeIfNecessary(Ray ray, float maxDistance, ref int layerMask)
		{
			if (shouldRaycastIgnoreLandscape(ray, maxDistance))
			{
				layerMask &= ~RayMasks.GROUND; // Remove GROUND bit flag
			}
		}

		[System.Obsolete("Hole collision is handled by Unity now")]
		public static void spherecastIgnoreLandscapeIfNecessary(Ray ray, float radius, float maxDistance, ref int layerMask)
		{
			if (shouldSpherecastIgnoreLandscape(ray, radius, maxDistance))
			{
				layerMask &= ~RayMasks.GROUND; // Remove GROUND bit flag
			}
		}
	}
}
