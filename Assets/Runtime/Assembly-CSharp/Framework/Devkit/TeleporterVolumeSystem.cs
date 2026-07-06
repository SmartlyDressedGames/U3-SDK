////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class TeleporterEntranceVolumeManager : VolumeManager<TeleporterEntranceVolume, TeleporterEntranceVolumeManager>
	{
#if !DEDICATED_SERVER
		protected override void OnUpdateGizmos(RuntimeGizmos runtimeGizmos)
		{
			base.OnUpdateGizmos(runtimeGizmos);

			foreach (TeleporterEntranceVolume volume in allVolumes)
			{
				Color volumeColor = volume.isSelected ? Color.yellow : debugColor;
				runtimeGizmos.Arrow(volume.transform.position, volume.transform.forward, 1.0f, volumeColor);

				if (!string.IsNullOrEmpty(volume.pairId))
				{
					List<TeleporterExitVolume> exitVolumes;
					if (TeleporterExitVolumeManager.Get().idToVolumes.TryGetValue(volume.pairId, out exitVolumes))
					{
						foreach (TeleporterExitVolume pairVolume in exitVolumes)
						{
							runtimeGizmos.Line(volume.transform.position, pairVolume.transform.position, volumeColor);
						}
					}
				}
			}
		}
#endif // !DEDICATED_SERVER

		public TeleporterEntranceVolumeManager()
		{
			FriendlyName = "Teleporter Entrance";
			SetDebugColor(new Color32(0, 0, 255, 255));
		}
	}

	public class TeleporterExitVolumeManager : VolumeManager<TeleporterExitVolume, TeleporterExitVolumeManager>
	{
		public TeleporterExitVolume FindExitVolume(string id)
		{
			if (string.IsNullOrEmpty(id))
				return null;

			List<TeleporterExitVolume> exitVolumes;
			if (!idToVolumes.TryGetValue(id, out exitVolumes))
				return null;

			return exitVolumes.RandomOrDefault();
		}

		public override void AddVolume(TeleporterExitVolume volume)
		{
			base.AddVolume(volume);
			AddVolumeToIdDictionary(volume);
		}

		public override void RemoveVolume(TeleporterExitVolume volume)
		{
			RemoveVolumeFromIdDictionary(volume);
			base.RemoveVolume(volume);
		}

		internal void AddVolumeToIdDictionary(TeleporterExitVolume volume)
		{
			if (string.IsNullOrEmpty(volume.id))
				return;

			List<TeleporterExitVolume> exitVolumes;
			if (!idToVolumes.TryGetValue(volume.id, out exitVolumes))
			{
				exitVolumes = new List<TeleporterExitVolume>();
				idToVolumes.Add(volume.id, exitVolumes);
			}
			exitVolumes.Add(volume);
		}

		internal void RemoveVolumeFromIdDictionary(TeleporterExitVolume volume)
		{
			if (string.IsNullOrEmpty(volume.id))
				return;

			List<TeleporterExitVolume> exitVolumes;
			if (idToVolumes.TryGetValue(volume.id, out exitVolumes))
			{
				exitVolumes.RemoveFast(volume);
				if (exitVolumes.Count < 1)
				{
					idToVolumes.Remove(volume.id);
				}
			}
		}

#if !DEDICATED_SERVER
		protected override void OnUpdateGizmos(RuntimeGizmos runtimeGizmos)
		{
			base.OnUpdateGizmos(runtimeGizmos);

			foreach (TeleporterExitVolume volume in allVolumes)
			{
				Color volumeColor = volume.isSelected ? Color.yellow : debugColor;
				runtimeGizmos.Arrow(volume.transform.position, volume.transform.forward, 1.0f, volumeColor);
			}
		}
#endif // !DEDICATED_SERVER

		public TeleporterExitVolumeManager()
		{
			FriendlyName = "Teleporter Exit";
			SetDebugColor(new Color32(0, 0, 255, 255));
		}

		internal Dictionary<string, List<TeleporterExitVolume>> idToVolumes = new Dictionary<string, List<TeleporterExitVolume>>();
	}
}
