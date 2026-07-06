////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class NPCOverlapVolumeManager : VolumeManager<NPCOverlapVolume, NPCOverlapVolumeManager>
	{
		public int CountPlayersInVolume(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				//UnturnedLog.info("ID is empty");
				return 0;
			}

			List<NPCOverlapVolume> volumesById;
			if (!idToVolumes.TryGetValue(id, out volumesById))
			{
				//UnturnedLog.info("No volumes");
				return 0;
			}

			int overlapCount = 0;

			foreach (SteamPlayer client in Provider.clients)
			{
				if (client.player == null)
					continue;

				Vector3 playerPosition = client.player.transform.position;
				foreach (NPCOverlapVolume volume in volumesById)
				{
					if (volume != null && volume.IsPositionInsideVolume(playerPosition))
					{
						++overlapCount;
						break;
					}
				}
			}

			return overlapCount;
		}

		public override void AddVolume(NPCOverlapVolume volume)
		{
			base.AddVolume(volume);
			AddVolumeToIdDictionary(volume);
		}

		public override void RemoveVolume(NPCOverlapVolume volume)
		{
			RemoveVolumeFromIdDictionary(volume);
			base.RemoveVolume(volume);
		}

		internal void AddVolumeToIdDictionary(NPCOverlapVolume volume)
		{
			if (string.IsNullOrEmpty(volume.id))
				return;

			List<NPCOverlapVolume> volumesById;
			if (!idToVolumes.TryGetValue(volume.id, out volumesById))
			{
				volumesById = new List<NPCOverlapVolume>();
				idToVolumes.Add(volume.id, volumesById);
			}
			volumesById.Add(volume);
		}

		internal void RemoveVolumeFromIdDictionary(NPCOverlapVolume volume)
		{
			if (string.IsNullOrEmpty(volume.id))
				return;

			List<NPCOverlapVolume> volumesById;
			if (idToVolumes.TryGetValue(volume.id, out volumesById))
			{
				volumesById.RemoveFast(volume);
				if (volumesById.Count < 1)
				{
					idToVolumes.Remove(volume.id);
				}
			}
		}

#if !DEDICATED_SERVER
		protected override void OnUpdateGizmos(RuntimeGizmos runtimeGizmos)
		{
			base.OnUpdateGizmos(runtimeGizmos);

			foreach (NPCOverlapVolume volume in allVolumes)
			{
				if (!volume.isSelected)
					continue;
				runtimeGizmos.Label(volume.transform.position, volume.id);
			}
		}
#endif // !DEDICATED_SERVER

		public NPCOverlapVolumeManager()
		{
			FriendlyName = "NPC Overlap";
			SetDebugColor(new Color32(130, 20, 200, 255));
		}

		internal Dictionary<string, List<NPCOverlapVolume>> idToVolumes = new Dictionary<string, List<NPCOverlapVolume>>();
	}
}
