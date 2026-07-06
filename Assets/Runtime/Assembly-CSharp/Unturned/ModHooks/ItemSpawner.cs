////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows Unity events to spawn items.
	/// </summary>
	[AddComponentMenu("Unturned/Item Spawner")]
	public class ItemSpawner : MonoBehaviour
	{
		public enum ESpawnState
		{
			World,
			Admin,
		}

		[Tooltip("ID or GUID of item asset (or spawn table) to spawn when SpawnDefault is invoked.")]
		public string DefaultAsset;

		[Tooltip("Controls fullness and quality of spawned items.")]
		public ESpawnState ItemState;

		public void SpawnDefault()
		{
#if GAME
			Spawn(DefaultAsset);
#endif
		}

		public void Spawn(string assetId)
		{
#if GAME
			if (!Provider.isServer)
				return;

			if (EffectManager.isInstantiatingEffectForPreload)
			{
				// Prevent effect preload from unintentionally spawning stuff.
				return;
			}

			if (!CachingBcAssetRef.TryParse(assetId, EAssetType.ITEM, out CachingBcAssetRef assetRef))
			{
				UnturnedLog.warn("{0} unable to parse asset ID \"{1}\"", transform.GetSceneHierarchyPath(), assetId);
				return;
			}

			Asset asset = assetRef.Get();
			if (asset == null)
			{
				UnturnedLog.warn("{0} unable to find asset \"{1}\"", transform.GetSceneHierarchyPath(), assetId);
				return;
			}

			if (asset is SpawnAsset spawnAsset)
			{
				asset = SpawnTableTool.Resolve(spawnAsset, EAssetType.ITEM, OnGetSpawnErrorContext);
				if (asset == null)
				{
					// Allow spawn table to not spawn something.
					return;
				}
			}

			ItemAsset itemAsset = asset as ItemAsset;
			if (itemAsset == null)
			{
				UnturnedLog.warn($"{transform.GetSceneHierarchyPath()} tried to spawn item but asset ({asset.FriendlyName}) is {asset.GetTypeFriendlyName()}");
				return;
			}

			EItemOrigin origin = ItemState == ESpawnState.World ? EItemOrigin.WORLD : EItemOrigin.ADMIN;
			ItemManager.dropItem(new Item(itemAsset, origin), transform.position, false, true, false);
#endif // GAME
		}

#if GAME
		private string OnGetSpawnErrorContext()
		{
			return $"item spawner {transform.GetSceneHierarchyPath()}";
		}
#endif // GAME
	}
}
