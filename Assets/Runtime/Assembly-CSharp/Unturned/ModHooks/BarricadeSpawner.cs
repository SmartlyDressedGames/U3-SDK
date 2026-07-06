////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows Unity events to spawn barricades.
	/// </summary>
	[AddComponentMenu("Unturned/Barricade Spawner")]
	public class BarricadeSpawner : MonoBehaviour
	{
		[Tooltip("ID or GUID of barricade asset (or spawn table) to spawn when SpawnDefault is invoked.")]
		public string DefaultAsset;

		[Tooltip("Finds ownership of BarricadeSpawner (e.g., parent barricade) and assigns to spawned barricade.")]
		public bool InheritOwnership;

		[Tooltip("If true and a vehicle exists in parent hierarchy, the spawned barricade will be attached to the vehicle.")]
		public bool ShouldAttachToVehicle;

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

			ItemBarricadeAsset barricadeAsset = asset as ItemBarricadeAsset;
			if (barricadeAsset == null)
			{
				UnturnedLog.warn($"{transform.GetSceneHierarchyPath()} tried to spawn barricade but asset ({asset.FriendlyName}) is {asset.GetTypeFriendlyName()}");
				return;
			}

			ulong ownerUser = 0;
			ulong ownerGroup = 0;
			if (InheritOwnership)
			{
				DamageTool.TryFindOwnership(transform, out ownerUser, out ownerGroup);
			}

			transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
			Barricade barricade = new Barricade(barricadeAsset);

			if (ShouldAttachToVehicle)
			{
				VehicleBarricadeRegion parentRegion = BarricadeManager.FindVehicleRegionByTransform(transform.root);
				if (parentRegion != null)
				{
					position = parentRegion.parent.InverseTransformPoint(position);
					rotation = parentRegion.parent.InverseTransformRotation(rotation);
					BarricadeManager.dropPlantedBarricade(parentRegion.parent, barricade, position, rotation, ownerUser, ownerGroup);
					return;
				}
			}

			BarricadeManager.dropNonPlantedBarricade(barricade, position, rotation, ownerUser, ownerGroup);
#endif // GAME
		}

#if GAME
		private string OnGetSpawnErrorContext()
		{
			return $"barricade spawner {transform.GetSceneHierarchyPath()}";
		}
#endif // GAME
	}
}
