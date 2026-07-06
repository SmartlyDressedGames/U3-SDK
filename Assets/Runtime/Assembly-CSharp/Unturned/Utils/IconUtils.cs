////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using System.Collections.Generic;
using UnityEngine;
using Unturned.UnityEx;

namespace SDG.Unturned
{
	public class ItemDefIconInfo
	{
#if UNITY_EDITOR
		/// <summary>
		/// Icon uploaded to CDN.
		/// </summary>
		public string economyUploadPath;

		/// <summary>
		/// Icon used in game menus.
		/// </summary>
		public string gameResourcePath;

		/// <summary>
		/// Project-relative asset importer version of <see cref="gameResourcePath"/>
		/// </summary>
		public string assetImporterPath;
#endif // UNITY_EDITOR

		/// <summary>
		/// Icon saved for community members in Extras folder.
		/// </summary>
		public string extraPath;

		/// <summary>
		/// Has the small icon been captured yet?
		/// </summary>
		private bool hasSmall;

		/// <summary>
		/// Has the large icon been captured yet?
		/// </summary>
		private bool hasLarge;

		public void onSmallItemIconReady(int handle, Texture2D texture)
		{
#if UNITY_EDITOR
			byte[] bytes = texture.EncodeToPNG();
			ReadWrite.writeBytes(economyUploadPath + "/Icon_Small.png", false, false, bytes);
#endif // UNITY_EDITOR

			hasSmall = true;
			complete();
		}

		public void onLargeItemIconReady(int handle, Texture2D texture)
		{
			byte[] bytes = texture.EncodeToPNG();
#if UNITY_EDITOR
			ReadWrite.writeBytes(economyUploadPath + "/Icon_Large.png", false, false, bytes);
			ReadWrite.writeBytes(gameResourcePath + "/Icon_Large.png", false, false, bytes);
			importResourceIcon(assetImporterPath + "/Icon_Large.png");
#else
			UnturnedLog.info(extraPath);
#endif // UNITY_EDITOR
			ReadWrite.writeBytes(extraPath + ".png", false, false, bytes);

			hasLarge = true;
			complete();
		}

#if UNITY_EDITOR
		private void importResourceIcon(string path)
		{
			UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.ForceUpdate);
			UnityEditor.TextureImporter importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
			importer.textureType = UnityEditor.TextureImporterType.GUI;
			importer.alphaIsTransparency = true;
			importer.npotScale = UnityEditor.TextureImporterNPOTScale.None;
			importer.filterMode = FilterMode.Trilinear;
			importer.SaveAndReimport();
		}
#endif // UNITY_EDITOR

		private void complete()
		{
			if (!hasSmall || !hasLarge)
			{
				return;
			}

			IconUtils.icons.Remove(this);
		}
	}

	public enum ESkinIconSize
	{
		SMALL,
		LARGE
	}

	public struct SkinIconInfo
	{
		public ushort id;
		public ESkinIconSize size;

		public SkinIconInfo(ushort newID, ESkinIconSize newSize)
		{
			id = newID;
			size = newSize;
		}
	}

	public class ExtraItemIconInfo
	{
		public string extraPath;

		public void onItemIconReady(int handle, Texture2D texture)
		{
			byte[] bytes = texture.EncodeToPNG();
			ReadWrite.writeBytes(extraPath + ".png", false, false, bytes);
			Object.Destroy(texture);

			IconUtils.extraIcons.Remove(this);
		}
	}

	/// <summary>
	/// Moved icon code from MenuTitleUI to here.
	/// </summary>
	public class IconUtils
	{
		public static List<ItemDefIconInfo> icons = new List<ItemDefIconInfo>();
		public static List<ExtraItemIconInfo> extraIcons = new List<ExtraItemIconInfo>();

		/// <summary>
		/// These directories are excluded from source control and Steam depots so they might not exist yet.
		/// </summary>
		public static void CreateExtrasDirectory()
		{
			ReadWrite.createFolder("/Extras/Econ");
			ReadWrite.createFolder("/Extras/Icons");
			ReadWrite.createFolder("/Extras/CosmeticPreviews_2048x2048");
			ReadWrite.createFolder("/Extras/CosmeticPreviews_400x400");
			ReadWrite.createFolder("/Extras/CosmeticPreviews_200x200");
			ReadWrite.createFolder("/Extras/OutfitPreviews_2048x2048");
			ReadWrite.createFolder("/Extras/OutfitPreviews_400x400");
			ReadWrite.createFolder("/Extras/OutfitPreviews_200x200");
		}

		public static ItemDefIconInfo getItemDefIcon(ushort itemID, ushort vehicleID, ushort skinID)
		{
			ItemAsset itemAsset = Assets.find(EAssetType.ITEM, itemID) as ItemAsset;
			VehicleAsset vehicleAsset = VehicleTool.FindVehicleByLegacyIdAndHandleRedirects(vehicleID);

			if (itemAsset == null && vehicleAsset == null)
			{
				UnturnedLog.warn("Could not find a matching item ({0}) or vehicle ({1}) asset!", itemID, vehicleID);
				return null;
			}

			return getItemDefIcon(itemAsset, vehicleAsset, skinID);
		}

		public static ItemDefIconInfo getItemDefIcon(ItemAsset itemAsset, VehicleAsset vehicleAsset, ushort skinID)
		{
			ItemDefIconInfo info = new ItemDefIconInfo();

			if (skinID != 0)
			{
				SkinAsset skinAsset = Assets.find(EAssetType.SKIN, skinID) as SkinAsset;

				if (skinAsset == null)
				{
					UnturnedLog.warn("Couldn't find a skin asset for: " + skinID);
					return null;
				}

				ushort sharedSkinID;
				if (vehicleAsset != null)
				{
					sharedSkinID = vehicleAsset.id;
				}
				else
				{
					sharedSkinID = itemAsset.id;
				}

				string sharedSkinName;
				if (vehicleAsset != null)
				{
					sharedSkinName = vehicleAsset.sharedSkinName;
				}
				else
				{
					sharedSkinName = itemAsset.name;
				}

#if UNITY_EDITOR
				info.economyUploadPath = UnityPaths.ProjectDirectory.FullName + "/Economy/Icons/Skins/" + sharedSkinName + "/" + skinAsset.name;
				info.gameResourcePath = Application.dataPath + "/Resources/Economy/Skins/" + sharedSkinName + "/" + skinAsset.name;
				info.assetImporterPath = "Assets/Resources/Economy/Skins/" + sharedSkinName + "/" + skinAsset.name;
#endif // UNITY_EDITOR
				info.extraPath = ReadWrite.PATH + "/Extras/Econ/" + sharedSkinName + "_" + sharedSkinID + "_" + skinAsset.name + "_" + skinAsset.id;

				if (vehicleAsset != null)
				{
					const bool readableOnCPU = true;
					VehicleTool.getIcon(vehicleAsset.id, skinAsset.id, vehicleAsset, skinAsset, 200, 200, readableOnCPU, info.onSmallItemIconReady);
					VehicleTool.getIcon(vehicleAsset.id, skinAsset.id, vehicleAsset, skinAsset, 400, 400, readableOnCPU, info.onLargeItemIconReady);
				}
				else
				{
					const bool readableOnCPU = true;
					ItemTool.getIcon(itemAsset.id, skinAsset.id, 100, itemAsset.getState(), itemAsset, skinAsset, string.Empty, string.Empty, 200, 200, true, readableOnCPU, info.onSmallItemIconReady);
					ItemTool.getIcon(itemAsset.id, skinAsset.id, 100, itemAsset.getState(), itemAsset, skinAsset, string.Empty, string.Empty, 400, 400, true, readableOnCPU, info.onLargeItemIconReady);
				}
			}
			else
			{
				if (itemAsset != null && string.IsNullOrEmpty(itemAsset.proPath))
				{
					UnturnedLog.error("Failed to find pro path for: " + itemAsset.id + " " + vehicleAsset?.id + " " + skinID);
					return null;
				}

#if UNITY_EDITOR
				info.economyUploadPath = UnityPaths.ProjectDirectory.FullName + "/Economy/Icons" + itemAsset.proPath;
				info.gameResourcePath = Application.dataPath + "/Resources/Economy" + itemAsset.proPath;
				info.assetImporterPath = "Assets/Resources/Economy" + itemAsset.proPath;
#endif // UNITY_EDITOR
				info.extraPath = ReadWrite.PATH + "/Extras/Econ/" + itemAsset.name + "_" + itemAsset.id;

				//UnturnedLog.info(itemAsset.name);
				const bool readableOnCPU = true;
				ItemTool.getIcon(itemAsset.id, 0, 100, itemAsset.getState(), itemAsset, null, string.Empty, string.Empty, 200, 200, true, readableOnCPU, info.onSmallItemIconReady);
				ItemTool.getIcon(itemAsset.id, 0, 100, itemAsset.getState(), itemAsset, null, string.Empty, string.Empty, 400, 400, true, readableOnCPU, info.onLargeItemIconReady);
			}

			icons.Add(info);
			return info;
		}

		public static void captureItemIcon(ItemAsset itemAsset)
		{
			if (itemAsset == null)
			{
				return;
			}

			ExtraItemIconInfo info = new ExtraItemIconInfo();
			info.extraPath = ReadWrite.PATH + "/Extras/Icons/" + itemAsset.name + "_" + itemAsset.id;
			const bool readableOnCPU = true;
			ItemTool.getIcon(itemAsset.id, 0, 100, itemAsset.getState(), itemAsset, null, string.Empty, string.Empty, itemAsset.size_x * 512, itemAsset.size_y * 512, false, readableOnCPU, info.onItemIconReady);

			extraIcons.Add(info);
		}

		public static void captureAllItemIcons()
		{
			List<ItemAsset> items = new List<ItemAsset>();
			Assets.find(items);
			foreach (ItemAsset asset in items)
			{
				captureItemIcon(asset);
			}
		}

		public static void CaptureAllSkinIcons()
		{
			foreach (KeyValuePair<int, UnturnedEconInfo> kvp in TempSteamworksEconomy.econInfo)
			{
				UnturnedEconInfo item = kvp.Value;
				if (item.item_skin == 0)
					continue;

				ItemAsset itemAsset = Assets.find(item.target_game_asset_guid) as ItemAsset;
				VehicleAsset vehicleAsset = Assets.find(item.target_game_asset_guid) as VehicleAsset;
				getItemDefIcon(itemAsset, vehicleAsset, (ushort) item.item_skin);
			}
		}

		private static GameObject cosmeticPreviewGameObject;
		private static CosmeticPreviewCapture cosmeticPreviewCapture;

		public static void CaptureCosmeticPreviews()
		{
			InitCapturePreview();
			cosmeticPreviewCapture.CaptureCosmetics();
		}

		public static void CaptureAllOutfitPreviews()
		{
			InitCapturePreview();
			cosmeticPreviewCapture.CaptureAllOutfits();
		}

		public static void CaptureOutfitPreview(System.Guid guid)
		{
			InitCapturePreview();
			cosmeticPreviewCapture.CaptureOutfit(guid);
		}

		private static void InitCapturePreview()
		{
			if (cosmeticPreviewGameObject == null)
			{
				cosmeticPreviewGameObject = Object.Instantiate(Resources.Load<GameObject>("Characters/CosmeticPreviewCapture"), new Vector3(-1000.0f, 0.0f, 0.0f), Quaternion.Euler(90.0f, 0.0f, 0.0f));
				cosmeticPreviewCapture = cosmeticPreviewGameObject.GetComponent<CosmeticPreviewCapture>();
			}
		}
	}
}
