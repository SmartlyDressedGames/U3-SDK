////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class VendorSellingNameAscendingComparator : IComparer<VendorSellingBase>
	{
		public int Compare(VendorSellingBase a, VendorSellingBase b)
		{
			string aName = a.displayName;
			string bName = b.displayName;

			if (aName == null || bName == null)
			{
				return 0;
			}

			return aName.CompareTo(bName);
		}
	}

	public abstract class VendorSellingBase : VendorElement
	{
		public bool canBuy(Player player)
		{
			if (outerAsset.currency.isValid)
			{
				ItemCurrencyAsset currencyAsset = outerAsset.currency.Find();
				if (currencyAsset == null)
				{
					Assets.ReportError(outerAsset, "missing currency asset");
					return false;
				}
				else
				{
					return currencyAsset.canAfford(player, cost);
				}
			}
			else
			{
				return player.skills.experience >= cost;
			}
		}

		public virtual void buy(Player player)
		{
			if (outerAsset.currency.isValid)
			{
				ItemCurrencyAsset currencyAsset = outerAsset.currency.Find();
				if (currencyAsset == null)
				{
					Assets.ReportError(outerAsset, "missing currency asset");
				}
				else
				{
					bool spent = currencyAsset.spendValue(player, cost);
					if (spent == false)
					{
						UnturnedLog.error("Spending {0} currency at vendor went wrong (this should never happen)", cost);
					}
				}
			}
			else
			{
				player.skills.askSpend(cost);
			}
		}

		public virtual void format(Player player, out ushort total)
		{
			total = 0;
		}

		public VendorSellingBase(VendorAsset newOuterAsset, byte newIndex, System.Guid newTargetAssetGuid, ushort newLegacyAssetId, uint newCost, NPCConditionsList newConditionsList, NPCRewardsList newRewardsList, string newDescriptionOverride)
			: base(newOuterAsset, newIndex, newTargetAssetGuid, newLegacyAssetId, newCost, newConditionsList, newRewardsList, newDescriptionOverride)
		{ }
	}

	/// <summary>
	/// Represents an item the vendor is selling to players.
	/// </summary>
	public class VendorSellingItem : VendorSellingBase
	{
		public ItemAsset FindItemAsset()
		{
#pragma warning disable
			return Assets.FindItemByGuidOrLegacyId<ItemAsset>(TargetAssetGuid, id);
#pragma warning restore
		}

		public override string displayName
		{
			get
			{
				ItemAsset item = FindItemAsset();
				return item != null ? item.itemName : null;
			}
		}

		public override string displayDesc
		{
			get
			{
				if (descriptionOverride != null)
				{
					return descriptionOverride;
				}
				else
				{
					ItemAsset asset = FindItemAsset();
					return asset != null ? asset.itemDescription : null;
				}
			}
		}

		public override EItemRarity rarity
		{
			get
			{
				ItemAsset asset = FindItemAsset();
				return asset != null ? asset.rarity : EItemRarity.COMMON;
			}
		}

		public int sight
		{
			get;
			protected set;
		}

		public int tactical
		{
			get;
			protected set;
		}

		public int grip
		{
			get;
			protected set;
		}

		public int barrel
		{
			get;
			protected set;
		}

		public int magazine
		{
			get;
			protected set;
		}

		public int ammo
		{
			get;
			protected set;
		}

		public override void buy(Player player)
		{
			base.buy(player);

			ItemAsset asset = FindItemAsset();
			if (asset == null)
				return;

			byte[] stateOverride = null;
			if (asset is ItemGunAsset gunAsset)
			{
				stateOverride = GetGunStateOverride(gunAsset);
			}

			Item item;
			if (stateOverride != null)
			{
				item = new Item(asset.id, 1, 100, stateOverride);
			}
			else
			{
				item = new Item(asset.id, EItemOrigin.ADMIN);
			}

			player.inventory.forceAddItem(item, false, false);
		}

		public override void format(Player player, out ushort total)
		{
			total = 0;

			ItemAsset asset = FindItemAsset();
			if (asset == null)
				return;

			using (ScopedPlayerInventorySearchResultPool scope = new ScopedPlayerInventorySearchResultPool())
			{
				player.inventory.FindItemsByAsset(scope.PooledResults, asset, false, true);

				foreach (PlayerInventorySearchResultV2 searchResult in scope.PooledResults)
				{
					total += searchResult.Jar.item.amount;
				}
			}
		}

		/// <summary>
		/// Refer to NPCItemReward state.
		/// </summary>
		internal byte[] GetGunStateOverride(ItemGunAsset gunAsset)
		{
			if (sight > -1 || tactical > -1 || grip > -1 || barrel > -1 || magazine > -1 || ammo > -1)
			{
				ushort sightId = sight > -1 ? MathfEx.ClampToUShort(sight) : gunAsset.sightID;
				ushort tacticalId = tactical > -1 ? MathfEx.ClampToUShort(tactical) : gunAsset.tacticalID;
				ushort gripId = grip > -1 ? MathfEx.ClampToUShort(grip) : gunAsset.gripID;
				ushort barrelId = barrel > -1 ? MathfEx.ClampToUShort(barrel) : gunAsset.barrelID;
				ushort magazineId = magazine > -1 ? MathfEx.ClampToUShort(magazine) : gunAsset.GetDefaultMagazineLegacyId();
				byte spawnAmmo = ammo > -1 ? MathfEx.ClampToByte(ammo) : gunAsset.ammoMax;
				byte[] state = gunAsset.getState(sightId, tacticalId, gripId, barrelId, magazineId, spawnAmmo);
				return state;
			}

			return null;
		}

		public VendorSellingItem(VendorAsset newOuterAsset, byte newIndex, System.Guid newTargetAssetGuid, ushort newTargetAssetLegacyId, uint newCost, NPCConditionsList newConditionsList, NPCRewardsList newRewardsList, string newDescriptionOverride, int newSight, int newTactical, int newGrip, int newBarrel, int newMagazine, int newAmmo)
			: base(newOuterAsset, newIndex, newTargetAssetGuid, newTargetAssetLegacyId, newCost, newConditionsList, newRewardsList, newDescriptionOverride)
		{
			sight = newSight;
			tactical = newTactical;
			grip = newGrip;
			barrel = newBarrel;
			magazine = newMagazine;
			ammo = newAmmo;
		}
	}

	/// <summary>
	/// Represents a vehicle the vendor is selling to players.
	/// </summary>
	public class VendorSellingVehicle : VendorSellingBase
	{
		/// <summary>
		/// Returned asset is not necessarily a vehicle asset yet: It can also be a VehicleRedirectorAsset which the
		/// vehicle spawner requires to properly set paint color.
		/// </summary>
		public Asset FindAsset()
		{
#pragma warning disable
			return Assets.FindBaseVehicleAssetByGuidOrLegacyId(TargetAssetGuid, id);
#pragma warning restore
		}

		public VehicleAsset FindVehicleAssetAndHandleRedirects()
		{
			Asset asset = FindAsset();
			if (asset is VehicleRedirectorAsset redirectorAsset)
			{
				asset = redirectorAsset.TargetVehicle.Find();
			}
			return asset as VehicleAsset;
		}

		public override string displayName
		{
			get
			{
				VehicleAsset vehicle = FindVehicleAssetAndHandleRedirects();
				return vehicle != null ? vehicle.vehicleName : null;
			}
		}

		public override string displayDesc
		{
			get
			{
				return descriptionOverride;
			}
		}

		public override EItemRarity rarity
		{
			get
			{
				VehicleAsset vehicle = FindVehicleAssetAndHandleRedirects();
				return vehicle != null ? vehicle.rarity : EItemRarity.COMMON;
			}
		}

		public override bool hasIcon => false;

		public string spawnpoint
		{
			get;
			protected set;
		}

		/// <summary>
		/// If set, takes priority over VehicleRedirectorAsset's paint color and over VehicleAsset's default paint color.
		/// </summary>
		public Color32? paintColor
		{
			get;
			protected set;
		}

		public override void buy(Player player)
		{
			base.buy(player);

			// We don't use FindVehicleAssetAndHandleRedirects because vehicle spawner needs redirector for paint color.
			Asset vehicleAsset = FindAsset();
			if (vehicleAsset == null)
				return;

			Vector3 position;
			Quaternion rotation;

			Spawnpoint item = SpawnpointSystemV2.Get().FindFirstSpawnpoint(spawnpoint);
			if (item != null)
			{
				position = item.transform.position;
				rotation = item.transform.rotation;
			}
			else
			{
				UnturnedLog.error("Failed to find vendor selling spawnpoint: " + spawnpoint);

				// Fallback to player transform because it would suck to buy a vehicle and not receive it.
				position = VehicleTool.GetPositionForVehicle(player);
				rotation = player.transform.rotation;
			}

			VehicleManager.spawnLockedVehicleForPlayerV2(vehicleAsset, position, rotation, player, paintColor);
		}

		public VendorSellingVehicle(VendorAsset newOuterAsset, byte newIndex, System.Guid newTargetAssetGuid, ushort newTargetAssetLegacyId, uint newCost, string newSpawnpoint, Color32? newPaintColor, NPCConditionsList newConditionsList, NPCRewardsList newRewardsList, string newDescriptionOverride)
			: base(newOuterAsset, newIndex, newTargetAssetGuid, newTargetAssetLegacyId, newCost, newConditionsList, newRewardsList, newDescriptionOverride)
		{
			spawnpoint = newSpawnpoint;
			paintColor = newPaintColor;
		}
	}
}
