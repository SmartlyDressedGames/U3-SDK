////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Framework.Water
{
	public class WaterUtility
	{
		[System.Obsolete]
		public static bool isPointInsideVolume(Vector3 point)
		{
			return WaterVolumeManager.Get().IsPositionInsideAnyVolume(point);
		}

		[System.Obsolete]
		public static bool isPointInsideVolume(Vector3 point, out WaterVolume volume)
		{
			volume = WaterVolumeManager.Get().GetFirstOverlappingVolume(point);
			return volume != null;
		}

		public static float getWaterSurfaceElevation(WaterVolume volume, Vector3 point)
		{
			point.y += 1024;
			Ray ray = new Ray(point, new Vector3(0, -1, 0));
			RaycastHit hit;
			if (volume.volumeCollider.Raycast(ray, out hit, 2048))
			{
				return hit.point.y;
			}
			else
			{
				return 0;
			}
		}

		[System.Obsolete]
		public static bool isPointInsideVolume(WaterVolume volume, Vector3 point)
		{
			return volume.IsPositionInsideVolume(point);
		}

		public static bool isPointUnderwater(Vector3 point)
		{
			return WaterVolumeManager.Get().IsPositionInsideAnyVolume(point);
		}

		/// <param name="volume">Null if under old water level, otherwise the volume.</param>
		public static bool isPointUnderwater(Vector3 point, out WaterVolume volume)
		{
			volume = WaterVolumeManager.Get().GetFirstOverlappingVolume(point);
			return volume != null;
		}

		/// <summary>
		/// Find the water elevation underneath point, or above point if underwater.
		/// </summary>
		public static float getWaterSurfaceElevation(Vector3 point)
		{
			bool foundSurface = false;
			float surfaceElevation = -1024;

			List<WaterVolume> volumesToTest = WaterVolumeManager.Get().GetOverlapTestVolumes(point);
			if (volumesToTest != null)
			{
				foreach (WaterVolume volume in volumesToTest)
				{
					if (volume.IsPositionInsideVolume(point))
					{
						return getWaterSurfaceElevation(volume, point);
					}
					else
					{
						Ray ray = new Ray(point, new Vector3(0, -1, 0));
						RaycastHit hit;
						if (volume.volumeCollider.Raycast(ray, out hit, 2048))
						{
							if (hit.point.y > surfaceElevation)
							{
								surfaceElevation = hit.point.y;
								foundSurface = true;
							}
						}
					}
				}
			}

			if (foundSurface)
			{
				return surfaceElevation;
			}

			return -1024;
		}

		public static void getUnderwaterInfo(Vector3 point, out bool isUnderwater, out float surfaceElevation)
		{
			WaterVolume volume = WaterVolumeManager.Get().GetFirstOverlappingVolume(point);
			if (volume != null)
			{
				isUnderwater = true;
				surfaceElevation = getWaterSurfaceElevation(volume, point);
			}
			else
			{
				isUnderwater = false;
				surfaceElevation = -1024;
			}
		}
	}
}
