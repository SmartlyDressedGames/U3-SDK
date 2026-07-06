////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class VehicleTool : MonoBehaviour
	{
		private static Queue<VehicleIconInfo> icons;

		/// <summary>
		/// Handles VehicleRedirectorAsset (if any) and returns actual vehicle asset (if any).
		/// </summary>
		public static VehicleAsset FindVehicleByLegacyIdAndHandleRedirects(ushort legacyId)
		{
			Asset asset = Assets.find(EAssetType.VEHICLE, legacyId);
			if (asset is VehicleRedirectorAsset redirectorAsset)
			{
				asset = redirectorAsset.TargetVehicle.Find();
			}
			return asset as VehicleAsset;
		}

		/// <summary>
		/// Handles VehicleRedirectorAsset returning load paint color override (if any) and returns actual vehicle asset (if any).
		/// </summary>
		public static VehicleAsset FindVehicleByLegacyIdAndHandleRedirectsWithLoadColor(ushort legacyId, out Color32 paintColor)
		{
			paintColor = new Color32(0, 0, 0, 0);
			Asset asset = Assets.find(EAssetType.VEHICLE, legacyId);
			if (asset is VehicleRedirectorAsset redirectorAsset)
			{
				if (redirectorAsset.LoadPaintColor.HasValue)
				{
					paintColor = redirectorAsset.LoadPaintColor.Value;
				}
				asset = redirectorAsset.TargetVehicle.Find();
			}
			return asset as VehicleAsset;
		}

		/// <summary>
		/// Handles VehicleRedirectorAsset returning spawn paint color override (if any) and returns actual vehicle asset (if any).
		/// </summary>
		public static VehicleAsset FindVehicleByLegacyIdAndHandleRedirectsWithSpawnColor(ushort legacyId, out Color32 paintColor)
		{
			paintColor = new Color32(0, 0, 0, 0);
			Asset asset = Assets.find(EAssetType.VEHICLE, legacyId);
			if (asset is VehicleRedirectorAsset redirectorAsset)
			{
				if (redirectorAsset.SpawnPaintColor.HasValue)
				{
					paintColor = redirectorAsset.SpawnPaintColor.Value;
				}
				asset = redirectorAsset.TargetVehicle.Find();
			}
			return asset as VehicleAsset;
		}

		/// <summary>
		/// Handles VehicleRedirectorAsset (if any) and returns actual vehicle asset (if any).
		/// </summary>
		public static VehicleAsset FindVehicleByGuidAndHandleRedirects(System.Guid guid)
		{
			Asset asset = Assets.find(guid);
			if (asset is VehicleRedirectorAsset redirectorAsset)
			{
				asset = redirectorAsset.TargetVehicle.Find();
			}
			return asset as VehicleAsset;
		}

		/// <summary>
		/// Handles VehicleRedirectorAsset returning load paint color override (if any) and returns actual vehicle asset (if any).
		/// </summary>
		public static VehicleAsset FindVehicleByGuidAndHandleRedirectsWithLoadColor(System.Guid guid, out Color32 paintColor)
		{
			paintColor = new Color32(0, 0, 0, 0);
			Asset asset = Assets.find(guid);
			if (asset is VehicleRedirectorAsset redirectorAsset)
			{
				if (redirectorAsset.LoadPaintColor.HasValue)
				{
					paintColor = redirectorAsset.LoadPaintColor.Value;
				}
				asset = redirectorAsset.TargetVehicle.Find();
			}
			return asset as VehicleAsset;
		}

		/// <summary>
		/// Handles VehicleRedirectorAsset returning spawn paint color override (if any) and returns actual vehicle asset (if any).
		/// </summary>
		public static VehicleAsset FindVehicleByGuidAndHandleRedirectsWithSpawnColor(System.Guid guid, out Color32 paintColor)
		{
			paintColor = new Color32(0, 0, 0, 0);
			Asset asset = Assets.find(guid);
			if (asset is VehicleRedirectorAsset redirectorAsset)
			{
				if (redirectorAsset.SpawnPaintColor.HasValue)
				{
					paintColor = redirectorAsset.SpawnPaintColor.Value;
				}
				asset = redirectorAsset.TargetVehicle.Find();
			}
			return asset as VehicleAsset;
		}

		/// <summary>
		/// Handles VehicleRedirectorAsset (if any) and returns actual vehicle asset (if any).
		/// </summary>
		public static VehicleAsset HandleRedirects(Asset asset)
		{
			if (asset is VehicleRedirectorAsset redirectorAsset)
			{
				asset = redirectorAsset.TargetVehicle.Find();
			}
			return asset as VehicleAsset;
		}

		/// <summary>
		/// Handles VehicleRedirectorAsset returning load paint color override (if any) and returns actual vehicle asset (if any).
		/// </summary>
		public static VehicleAsset HandleRedirectsWithLoadColor(Asset asset, out Color32 paintColor)
		{
			paintColor = new Color32(0, 0, 0, 0);
			if (asset is VehicleRedirectorAsset redirectorAsset)
			{
				if (redirectorAsset.LoadPaintColor.HasValue)
				{
					paintColor = redirectorAsset.LoadPaintColor.Value;
				}
				asset = redirectorAsset.TargetVehicle.Find();
			}
			return asset as VehicleAsset;
		}

		/// <summary>
		/// Handles VehicleRedirectorAsset returning spawn paint color override (if any) and returns actual vehicle asset (if any).
		/// </summary>
		public static VehicleAsset HandleRedirectsWithSpawnColor(Asset asset, out Color32 paintColor)
		{
			paintColor = new Color32(0, 0, 0, 0);
			if (asset is VehicleRedirectorAsset redirectorAsset)
			{
				if (redirectorAsset.SpawnPaintColor.HasValue)
				{
					paintColor = redirectorAsset.SpawnPaintColor.Value;
				}
				asset = redirectorAsset.TargetVehicle.Find();
			}
			return asset as VehicleAsset;
		}

		public static Transform getVehicle(ushort id, ushort skin, ushort mythic, VehicleAsset vehicleAsset, SkinAsset skinAsset)
		{
			GameObject modelPrefab = vehicleAsset?.GetOrLoadModel();
			if (modelPrefab != null)
			{
				if (id != vehicleAsset.id)
				{
					UnturnedLog.error("ID and asset ID are not in sync!");
				}

				Transform vehicle = Instantiate(modelPrefab).transform;
				vehicle.name = id.ToString();

				if (skinAsset != null)
				{
					InteractableVehicle character = vehicle.gameObject.AddComponent<InteractableVehicle>();
					character.id = id;
					character.skinID = skin;
					character.mythicID = mythic;
					character.fuel = 10000;
					character.isExploded = false;
					character.health = 10000;
					character.batteryCharge = 10000;
					character.safeInit(vehicleAsset);
					character.updateFires();
					character.updateSkin();
				}

				return vehicle;
			}
			else
			{
				Transform vehicle = new GameObject().transform;
				vehicle.name = id.ToString();
				vehicle.tag = "Vehicle";
				vehicle.gameObject.layer = LayerMasks.VEHICLE;

				return vehicle;
			}
		}

		public static int getIcon(ushort id, ushort skin, VehicleAsset vehicleAsset, SkinAsset skinAsset, int x, int y, bool readableOnCPU, VehicleIconReady callback)
		{
			if (vehicleAsset != null)
			{
				if (id != vehicleAsset.id)
				{
					UnturnedLog.error("ID and vehicle asset ID are not in sync!");
				}
			}

			if (skinAsset != null)
			{
				if (skin != skinAsset.id)
				{
					UnturnedLog.error("ID and skin asset ID are not in sync!");
				}
			}

			VehicleIconInfo info = new VehicleIconInfo();
			info.id = id;
			info.skin = skin;
			info.vehicleAsset = vehicleAsset;
			info.skinAsset = skinAsset;
			info.x = x;
			info.y = y;
			info.readableOnCPU = readableOnCPU;
			info.callback = callback;
			info.handle = handleCounter;
			icons.Enqueue(info);
			++handleCounter;
			return info.handle;
		}

		private static int handleCounter = 1;

		internal static Vector3 GetPositionForVehicle(Player player)
		{
			Vector3 point = player.transform.position + (player.transform.forward * 6);

			RaycastHit hit;
			Physics.Raycast(point + (Vector3.up * 16), Vector3.down, out hit, 32, RayMasks.BLOCK_VEHICLE);

			if (hit.collider != null)
			{
				point.y = hit.point.y + 16;
			}

			return point;
		}

		/// <summary>
		/// Supports redirects by VehicleRedirectorAsset. If redirector's SpawnPaintColor is set, that color is used.
		/// </summary>
		public static InteractableVehicle SpawnVehicleForPlayer(Player player, Asset asset)
		{
			if (player == null || asset == null)
				return null;

			Vector3 point = GetPositionForVehicle(player);
			return VehicleManager.spawnVehicleV2(asset, point, player.transform.rotation);
		}

		/// <summary>
		/// Supports redirects by VehicleRedirectorAsset. If redirector's SpawnPaintColor is set, that color is used.
		/// </summary>
		/// <returns>true if matching vehicle asset was found. (Not necessarily whether vehicle was spawned.)</returns>
		public static bool giveVehicle(Player player, ushort id)
		{
			// We only use this asset to check it exists. Actual redirect is handled by vehicle spawning.
			VehicleAsset finalAsset = FindVehicleByLegacyIdAndHandleRedirects(id);
			if (finalAsset != null)
			{
				Vector3 point = GetPositionForVehicle(player);
				VehicleManager.spawnVehicleV2(id, point, player.transform.rotation);

				return true;
			}

			return false;
		}

		private void Update()
		{
			if (icons == null || icons.Count == 0)
			{
				return;
			}

			VehicleIconInfo info = icons.Dequeue();

			if (info == null)
			{
				return;
			}

			if (info.vehicleAsset == null)
			{
				return;
			}

			Transform vehicle = getVehicle(info.id, info.skin, 0, info.vehicleAsset, info.skinAsset);
			vehicle.position = new Vector3(-256, -256, 0);

			Transform icon = vehicle.Find("Icon2");

			if (icon == null)
			{
				Destroy(vehicle.gameObject);

				Assets.ReportError(info.vehicleAsset, "missing 'Icon2' Transform");
				return;
			}

			float orthoSize = info.vehicleAsset.size2_z;
			Texture2D texture = ItemTool.captureIcon(info.id, info.skin, vehicle, icon, info.x, info.y, orthoSize, info.readableOnCPU);

			info.callback?.Invoke(info.handle, texture);
		}

		private void Start()
		{
			icons = new Queue<VehicleIconInfo>();
		}
	}
}
