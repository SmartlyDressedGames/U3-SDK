////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows Unity events to spawn vehicles.
	/// </summary>
	[AddComponentMenu("Unturned/Vehicle Spawner")]
	public class VehicleSpawner : MonoBehaviour
	{
		[Tooltip("ID or GUID of vehicle asset (or spawn table) to spawn when SpawnDefault is invoked.")]
		public string DefaultAsset;

		[Tooltip("If true, apply PaintColorOverride.")]
		public bool UsePaintColorOverride;

		[Tooltip("If UsePaintColorOverride is true, this paint color is used instead of the vehicle's default.")]
		public Color32 PaintColorOverride;

		[Tooltip("Finds ownership of VehicleSpawner (e.g., parent barricade) and assigns to spawned vehicle.")]
		public bool InheritOwnership;

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

			if (!CachingBcAssetRef.TryParse(assetId, EAssetType.VEHICLE, out CachingBcAssetRef assetRef))
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
				asset = SpawnTableTool.Resolve(spawnAsset, EAssetType.VEHICLE, OnGetSpawnErrorContext);
				if (asset == null)
				{
					// Allow spawn table to not spawn something.
					return;
				}
			}

			if (!(asset is VehicleAsset || asset is VehicleRedirectorAsset))
			{
				UnturnedLog.warn($"{transform.GetSceneHierarchyPath()} tried to spawn vehicle but asset ({asset.FriendlyName}) is {asset.GetTypeFriendlyName()}");
				return;
			}

			ulong ownerUser = 0;
			ulong ownerGroup = 0;
			if (InheritOwnership)
			{
				DamageTool.TryFindOwnership(transform, out ownerUser, out ownerGroup);
			}

			Color32? paintColor = UsePaintColorOverride ? PaintColorOverride : null;
			transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
			VehicleManager.spawnVehicleInternal(asset, position, rotation, new Steamworks.CSteamID(ownerUser), new Steamworks.CSteamID(ownerGroup), paintColor);
#endif // GAME
		}

#if GAME
		private string OnGetSpawnErrorContext()
		{
			return $"vehicle spawner {transform.GetSceneHierarchyPath()}";
		}
#endif // GAME
	}
}
