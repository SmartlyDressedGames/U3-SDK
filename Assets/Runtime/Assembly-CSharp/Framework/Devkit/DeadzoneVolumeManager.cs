////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class DeadzoneVolumeManager : VolumeManager<DeadzoneVolume, DeadzoneVolumeManager>
	{
		public DeadzoneVolume GetMostDangerousOverlappingVolume(Vector3 position)
		{
			List<DeadzoneVolume> volumesToTest = GetOverlapTestVolumes(position);
			if (volumesToTest == null)
			{
				return null;
			}

			DeadzoneVolume volume = null;

			foreach (DeadzoneVolume testVolume in volumesToTest)
			{
				if (volume == null || testVolume.DeadzoneType > volume.DeadzoneType)
				{
					if (testVolume.IsPositionInsideVolume(position))
					{
						volume = testVolume;
						if (volume.DeadzoneType == EDeadzoneType.FullSuitRadiation)
						{
							// Max reached.
							break;
						}
					}
				}
			}

			return volume;
		}

		/// <summary>
		/// Hacked to check horizontal distance.
		/// </summary>
		public bool IsNavmeshCenterInsideAnyVolume(Vector3 position)
		{
			List<DeadzoneVolume> volumesToTest = GetOverlapTestVolumes(position);
			if (volumesToTest == null)
			{
				return false;
			}

			foreach (DeadzoneVolume volume in volumesToTest)
			{
				Vector3 newPosition = new Vector3(position.x, volume.transform.position.y, position.z);
				if (volume.IsPositionInsideVolume(newPosition))
				{
					return true;
				}
			}

			return false;
		}

		public DeadzoneVolumeManager()
		{
			FriendlyName = "Deadzone";
			SetDebugColor(new Color32(255, 0, 0, 255));
			benefitsFromStaticVolumes = true;
		}
	}
}
