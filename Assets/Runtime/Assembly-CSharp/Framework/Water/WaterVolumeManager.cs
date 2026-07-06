////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Water
{
	public class WaterVolumeManager : VolumeManager<WaterVolume, WaterVolumeManager>
	{
		/// <summary>
		/// Water volume marked as being sea level.
		/// </summary>
		public static WaterVolume seaLevelVolume;

		public static float worldSeaLevel
		{
			get
			{
				if (seaLevelVolume != null)
					return seaLevelVolume.transform.TransformPoint(0, 0.5f, 0).y;
				else
					return -1024;
			}
		}

		private static List<WaterVolume> tempVolumes = new List<WaterVolume>();
		/// <summary>
		/// Prioritizes volumes with a fishing override.
		/// </summary>
		public WaterVolume GetFishingVolume(Vector3 position)
		{
			List<WaterVolume> volumesToTest = GetOverlapTestVolumes(position);
			if (volumesToTest == null)
			{
				return null;
			}

			tempVolumes.Clear();
			foreach (WaterVolume volume in volumesToTest)
			{
				if (volume.GetFishSpawnTable() != null)
				{
					if (volume.IsPositionInsideVolume(position))
					{
						return volume;
					}
				}
				else
				{
					tempVolumes.Add(volume);
				}
			}

			foreach (WaterVolume volume in tempVolumes)
			{
				if (volume.IsPositionInsideVolume(position))
				{
					return volume;
				}
			}

			return null;
		}

		public WaterVolumeManager()
		{
			FriendlyName = "Water";
			SetDebugColor(new Color32(50, 200, 200, 255));
			benefitsFromStaticVolumes = true;

			if (!Dedicator.IsDedicatedServer)
			{
				oldWaterQuality = GraphicsSettings.waterQuality;
				wasPlanarReflectionEnabled = oldWaterQuality == EGraphicQuality.ULTRA;
				GraphicsSettings.graphicsSettingsApplied += OnGraphicsSettingsApplied;
			}
		}

		private void OnGraphicsSettingsApplied()
		{
			EGraphicQuality newWaterQuality = GraphicsSettings.waterQuality;
			if (oldWaterQuality != newWaterQuality)
			{
				oldWaterQuality = newWaterQuality;

				foreach (WaterVolume volume in allVolumes)
				{
					volume.SyncWaterQuality();
				}

				bool newPlanarReflectionEnabled = newWaterQuality == EGraphicQuality.ULTRA;
				if (wasPlanarReflectionEnabled != newPlanarReflectionEnabled)
				{
					wasPlanarReflectionEnabled = newPlanarReflectionEnabled;
					foreach (WaterVolume volume in allVolumes)
					{
						volume.SyncPlanarReflectionEnabled();
					}
				}
			}
		}

		private EGraphicQuality oldWaterQuality;
		private bool wasPlanarReflectionEnabled;
	}
}
