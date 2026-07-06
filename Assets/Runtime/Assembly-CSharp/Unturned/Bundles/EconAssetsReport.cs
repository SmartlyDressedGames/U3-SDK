////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
using SDG.Framework.IO.Serialization;
using System.Collections.Generic;
using Unturned.UnityEx;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Export JSON report of Unturned's assets for economy usage.
	/// </summary>
	public class EconAssetsReport
	{
		public static void buildReport()
		{
			EconAssetsReport report = new EconAssetsReport();
			report.export();
		}

		protected void gatherAssets()
		{
			List<ItemAsset> itemAssets = new List<ItemAsset>();
			Assets.find(itemAssets);
			foreach (ItemAsset itemAsset in itemAssets)
			{
				Item exportItem = new Item();
				exportItem.ItemID = itemAsset.id;
				exportItem.ItemGuid = itemAsset.GUID;
				exportItem.DisplayName = itemAsset.itemName;
				exportItem.InternalName = itemAsset.name;

				if (itemAsset is ItemGearAsset gearAsset)
				{
					exportItem.UsesHairColor = !string.IsNullOrEmpty(gearAsset.hairOverride);
					exportItem.UsesBeardColor = !string.IsNullOrEmpty(gearAsset.BeardOverride);
					exportItem.UsesSkinColor = !string.IsNullOrEmpty(gearAsset.skinOverride);
				}

				UnityEngine.GameObject clothingPrefab = null;
				if (itemAsset is ItemShirtAsset shirtAsset)
				{
					exportItem.CosmeticSlot = ECosmeticSlot.Shirt;
				}
				else if (itemAsset is ItemPantsAsset pantsAsset)
				{
					exportItem.CosmeticSlot = ECosmeticSlot.Pants;
				}
				else if (itemAsset is ItemHatAsset hatAsset)
				{
					clothingPrefab = hatAsset.hat;
					exportItem.CosmeticSlot = ECosmeticSlot.Hat;
				}
				else if (itemAsset is ItemMaskAsset maskAsset)
				{
					clothingPrefab = maskAsset.mask;
					exportItem.CosmeticSlot = ECosmeticSlot.Mask;
				}
				else if (itemAsset is ItemGlassesAsset glassesAsset)
				{
					clothingPrefab = glassesAsset.glasses;
					exportItem.CosmeticSlot = ECosmeticSlot.Glasses;
				}
				else if (itemAsset is ItemBackpackAsset backpackAsset)
				{
					clothingPrefab = backpackAsset.backpack;
					exportItem.CosmeticSlot = ECosmeticSlot.Backpack;
				}
				else if (itemAsset is ItemVestAsset vestAsset)
				{
					clothingPrefab = vestAsset.vest;
					exportItem.CosmeticSlot = ECosmeticSlot.Vest;
				}

				if (clothingPrefab != null)
				{
					Transform effectTransform = clothingPrefab.transform.Find("Effect");
					exportItem.HasEffectTransform = effectTransform != null;
					if (effectTransform != null)
					{
						// Nelson 2024-09-18: At the time of writing, the only item with mythical effect scaling is
						// Molt's scarf. Should we support non-uniform scaling?
						Vector3 scale = effectTransform.localScale;
						if (!scale.AreComponentsNearlyEqual())
						{
							Assets.ReportError(itemAsset, "Effect transform has non-uniform scale");
						}
						else if(scale.x < 0.001)
						{
							Assets.ReportError(itemAsset, "Effect transform has negative scale");
						}

						// There are some special cases, but this check was helpful to catch some mistakes.
// 						if (itemAsset.type == EItemType.HAT || itemAsset.type == EItemType.MASK || itemAsset.type == EItemType.GLASSES)
// 						{
// 							Vector3 eulerRotation = effectTransform.localEulerAngles;
// 							if (!eulerRotation.IsNearlyEqual(new Vector3(0.0f, 270.0f, 270.0f), 5.0f))
// 							{
// 								Assets.ReportError(itemAsset, $"Effect transform rotation {eulerRotation} is unusual");
// 							}
// 						}
					}
				}

				Items.Add(exportItem);
			}

			List<SkinAsset> skinAssets = new List<SkinAsset>();
			Assets.find(skinAssets);
			foreach (SkinAsset skinAsset in skinAssets)
			{
				Skin exportSkin = new Skin();
				exportSkin.SkinID = skinAsset.id;
				exportSkin.SkinGuid = skinAsset.GUID;
				exportSkin.InternalName = skinAsset.name;
				exportSkin.SecondaryItemIDs = new int[skinAsset.secondarySkins.Keys.Count];
				int index = 0;
				foreach (ushort id in skinAsset.secondarySkins.Keys)
				{
					exportSkin.SecondaryItemIDs[index] = id;
					++index;
				}
				exportSkin.HasLayeredFallback = skinAsset.attachmentSkin != null;
				exportSkin.HasTertiaryFallback = skinAsset.tertiarySkin != null;
				exportSkin.HasMeshOverride = skinAsset.overrideMeshes.Count > 0;

				if (skinAsset.lightingTime.HasValue)
				{
					switch (skinAsset.lightingTime)
					{
						case ELightingTime.DAWN:
							exportSkin.LightingTime = Skin.ELightingTime.Dawn;
							break;
						case ELightingTime.DUSK:
							exportSkin.LightingTime = Skin.ELightingTime.Dusk;
							break;
					}
				}

				Skins.Add(exportSkin);
			}

			List<VehicleAsset> vehicleAssets = new List<VehicleAsset>();
			Assets.find(vehicleAssets);
			foreach (VehicleAsset vehicleAsset in vehicleAssets)
			{
				Vehicle exportVehicle = new Vehicle();
				exportVehicle.VehicleID = vehicleAsset.id;
				exportVehicle.VehicleGuid = vehicleAsset.GUID;
				exportVehicle.DisplayName = vehicleAsset.vehicleName;
				exportVehicle.InternalName = vehicleAsset.name;
				Vehicles.Add(exportVehicle);
			}

			List<OutfitAsset> outfitAssets = new List<OutfitAsset>();
			Assets.find(outfitAssets);
			foreach (OutfitAsset asset in outfitAssets)
			{
				Outfit outfit = new Outfit();
				outfit.OutfitGuid = asset.GUID;
				outfit.InternalName = asset.name;
				Outfits.Add(outfit);
			}
		}

		protected void export()
		{
			gatherAssets();

			string outputPath = UnityPaths.ProjectDirectory.FullName + @"\Economy\ItemSchema\Output\Release\AssetsReport.json";
			ISerializer serializer = new JSONSerializer();
			serializer.serialize(this, outputPath, true);
		}

		public enum ECosmeticSlot
		{
			None,
			Shirt,
			Pants,
			Hat,
			Backpack,
			Vest,
			Mask,
			Glasses,
		}

		public class Item
		{
			public int ItemID;
			public System.Guid ItemGuid;
			public string DisplayName;
			public string InternalName;
			public bool UsesHairColor;
			public bool UsesBeardColor;
			public bool UsesSkinColor;

			/// <summary>
			/// If true, contains child transform named Effect for mythical attachment. 
			/// </summary>
			public bool HasEffectTransform;

			public ECosmeticSlot CosmeticSlot;
		}

		public class Skin
		{
			public int SkinID;
			public System.Guid SkinGuid;
			public string InternalName;

			/// <summary>
			/// Attachment item IDs that get skinned specially.
			/// </summary>
			public int[] SecondaryItemIDs;

			/// <summary>
			/// Is there a fallback material for attachments that respects their main metallic areas?
			/// </summary>
			public bool HasLayeredFallback;

			/// <summary>
			/// Is there a fallback material without any special features?
			/// </summary>
			public bool HasTertiaryFallback;

			/// <summary>
			/// Is there a replacement mesh?
			/// </summary>
			public bool HasMeshOverride;

			public enum ELightingTime
			{
				None,
				Dawn,
				Dusk,
			}

			/// <summary>
			/// Dawn and dusk skins pull per-lighting colors.
			/// </summary>
			public ELightingTime LightingTime;
		}

		public class Vehicle
		{
			public int VehicleID;
			public System.Guid VehicleGuid;
			public string DisplayName;
			public string InternalName;
		}

		public class Outfit
		{
			public System.Guid OutfitGuid;
			public string InternalName;
		}

		public List<Item> Items;
		public List<Skin> Skins;
		public List<Vehicle> Vehicles;
		public List<Outfit> Outfits;

		protected EconAssetsReport()
		{
			Items = new List<Item>();
			Skins = new List<Skin>();
			Vehicles = new List<Vehicle>();
			Outfits = new List<Outfit>();
		}
	}
}
#endif // UNITY_EDTIOR
