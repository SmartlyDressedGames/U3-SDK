////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if GAME
using SDG.Framework.Devkit;
#endif
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows Unity events to call in airdrops.
	/// </summary>
	[AddComponentMenu("Unturned/Airdrop Spawner")]
	public class AirdropSpawner : MonoBehaviour
	{
		[Tooltip("Optional ID or GUID of spawn table asset to override cargo with when SpawnDefault is invoked.")]
		public string DefaultCargoSpawnTable;

		[Tooltip("If set, find spawnpoint node by name and call in airdrop there.")]
		public string SpawnpointName;

		[Tooltip("If true, select a random valid airdrop node and call in airdrop there.")]
		public bool UseRandomAirdropNode;

		public void SpawnDefault()
		{
#if GAME
			Spawn(DefaultCargoSpawnTable);
#endif
		}

		public void Spawn(string cargoSpawnTableId)
		{
#if GAME
			if (!Provider.isServer)
				return;

			if (EffectManager.isInstantiatingEffectForPreload)
			{
				// Prevent effect preload from unintentionally spawning stuff.
				return;
			}

			AirdropDevkitNode airdropNode = null;
			if (UseRandomAirdropNode)
			{
				airdropNode = LevelManager.GetRandomAirdropNode();
				if (airdropNode == null)
				{
					UnturnedLog.info("{0} unable to get a random airdrop node", transform.GetSceneHierarchyPath());
					return;
				}
			}

			SpawnAsset cargoSpawnTable;
			if (!string.IsNullOrEmpty(cargoSpawnTableId))
			{
				if (!CachingBcAssetRef.TryParse(cargoSpawnTableId, EAssetType.SPAWN, out CachingBcAssetRef cargoSpawnTableRef))
				{
					UnturnedLog.warn("{0} unable to parse cargo spawn table ID \"{1}\"", transform.GetSceneHierarchyPath(), cargoSpawnTableId);
					return;
				}

				cargoSpawnTable = cargoSpawnTableRef.Get<SpawnAsset>();
				if (cargoSpawnTable == null)
				{
					UnturnedLog.warn("{0} unable to find cargo spawn table \"{1}\"", transform.GetSceneHierarchyPath(), cargoSpawnTableId);
					return;
				}
			}
			else
			{
				if (airdropNode == null)
				{
					UnturnedLog.warn("{0} cargo spawn table required because UseAirdropNodes is false", transform.GetSceneHierarchyPath());
					return;
				}

				cargoSpawnTable = airdropNode.GetCargoSpawnTableOrLogWarning();
				if (cargoSpawnTable == null)
				{
					return;
				}
			}

			Vector3 dropPosition = transform.position;
			if (airdropNode != null)
			{
				dropPosition = airdropNode.transform.position;
			}
			else if (!string.IsNullOrEmpty(SpawnpointName))
			{
				Spawnpoint item = SpawnpointSystemV2.Get().FindFirstSpawnpoint(SpawnpointName);
				if (item != null)
				{
					dropPosition = item.transform.position;
				}
				else
				{
					UnturnedLog.warn("{0} unable to find spawnpoint \"{1}\"", transform.GetSceneHierarchyPath(), SpawnpointName);
				}
			}

			LevelManager.SpawnAirdrop(dropPosition, cargoSpawnTable);
#endif // GAME
		}
	}
}
