////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Determines how the "Explosion" effect is spawned when a barricade or structure is destroyed.
	/// 
	/// Nelson 2025-09-08: although explosion effect currently exists in Barricade and Structure
	/// sub-classes I think it makes sense to share this option (and ideally more in future).
	/// </summary>
	[System.Flags]
	public enum EPlaceableExplosionEffectFlags
	{
		/// <summary>
		/// Legacy behavior.
		/// </summary>
		None = 0,

		/// <summary>
		/// Effect spawns exactly at the model position without any offset.
		/// </summary>
		CopyModelPosition = 1 << 0,

		/// <summary>
		/// Effect spawns with same rotation as the model.
		/// </summary>
		CopyModelRotation = 1 << 1,
	}

	/// <summary>
	/// Common base for barricades and structures.
	/// 2023-01-16: not ideal to be adding this so late in development, but at least it is a step in the right direction.
	/// </summary>
	public class ItemPlaceableAsset : ItemAsset, IArmorFalloff
	{
		#region IArmorFalloff
		public float ArmorFalloffMaxRange { get; set; }
		public float ArmorFalloffRange { get; set; }
		public float ArmorFalloffMultiplier { get; set; }
		#endregion IArmorFalloff

		/// <summary>
		/// If true, this item is eligible for zombies to detect and attack when stuck.
		/// Defaults to true.
		/// </summary>
		public bool CanZombiesTarget
		{
			get;
			protected set;
		}

		/// <summary>
		/// Item or spawn table recovered when picked up below 100% health.
		/// </summary>
		public CachingAssetRef SalvageItemRef
		{
			get => _salvageItemRef;
			set => _salvageItemRef = value;
		}
		private CachingAssetRef _salvageItemRef;

		/// <summary>
		/// Minimum number of items to recover when salvaged.
		/// </summary>
		public int MinItemsRecoveredOnSalvage
		{
			get;
			set;
		}

		/// <summary>
		/// Maximum number of items to recover when salvaged.
		/// </summary>
		public int MaxItemsRecoveredOnSalvage
		{
			get;
			set;
		}

		/// <summary>
		/// Item or spawn table recovered when picked up at 100% health.
		/// Defaults to self.
		/// </summary>
		public CachingAssetRef SalvageItemFullHealthRef
		{
			get => _salvageItemFullHealthRef;
			set => _salvageItemFullHealthRef = value;
		}
		private CachingAssetRef _salvageItemFullHealthRef;

		/// <summary>
		/// Minimum number of items to recover when salvaged at full health.
		/// </summary>
		public int MinItemsRecoveredOnSalvageFullHealth
		{
			get;
			set;
		}

		/// <summary>
		/// Maximum number of items to recover when salvaged at full health.
		/// </summary>
		public int MaxItemsRecoveredOnSalvageFullHealth
		{
			get;
			set;
		}

		/// <summary>
		/// Minimum number of items to drop when destroyed.
		/// </summary>
		public int minItemsDroppedOnDestroy
		{
			get;
			protected set;
		}

		/// <summary>
		/// Maximum number of items to drop when destroyed.
		/// </summary>
		public int maxItemsDroppedOnDestroy
		{
			get;
			protected set;
		}

		/// <summary>
		/// Item or spawn table dropped when destroyed.
		/// </summary>
		public CachingAssetRef ItemDroppedOnDestroyRef
		{
			get => _itemDroppedOnDestroyRef;
			set => _itemDroppedOnDestroyRef = value;
		}
		private CachingAssetRef _itemDroppedOnDestroyRef;

		/// <summary>
		/// If non-null, this asset provides the listed crafting tags to nearby players.
		/// </summary>
		public CachingAssetRef[] PlaceableProvidedCraftingTags
		{
			get;
			protected set;
		}

		public EPlaceableExplosionEffectFlags ExplosionEffectFlags
		{
			get;
			set;
		}

		/// <summary>
		/// Note: this assumes SalvageItemRef points to an ItemAsset.
		/// </summary>
		public ItemAsset FindSalvageItemAsset()
		{
			if (SalvageItemRef.IsAssigned)
			{
				return SalvageItemRef.Get<ItemAsset>();
			}
			else
			{
				return FindDefaultSalvageItemAsset();
			}
		}

		private static List<BlueprintSupply> workingSalvageableItems = new List<BlueprintSupply>();
		/// <summary>
		/// By default a crafting ingredient is salvaged.
		/// </summary>
		public ItemAsset FindDefaultSalvageItemAsset()
		{
			foreach (Blueprint blueprint in blueprints)
			{
				if (blueprint.outputs.Length == 1 && blueprint.outputs[0].IsItem(this))
				{
					workingSalvageableItems.Clear();
					for (int index = 0; index < blueprint.supplies.Length; ++index)
					{
						BlueprintSupply supply = blueprint.supplies[index];
						if (supply.ShouldConsume) // Don't return a tool item.
						{
							workingSalvageableItems.Add(supply);
						}
					}

					if (workingSalvageableItems.IsEmpty())
					{
						continue;
					}

					return workingSalvageableItems.RandomOrDefault().FindItemAsset();
				}
			}

			return null;
		}

		public void GrantSalvageItems(Player player, bool fullHealth)
		{
			int itemCount = fullHealth
				? Random.Range(MinItemsRecoveredOnSalvageFullHealth, MaxItemsRecoveredOnSalvageFullHealth + 1)
				: Random.Range(MinItemsRecoveredOnSalvage, MaxItemsRecoveredOnSalvage + 1);
			// Prevent players from crashing themselves with huge numbers of items.
			itemCount = Mathf.Clamp(itemCount, 0, 100);

			if (itemCount < 1)
				return;

			Asset salvageAsset = fullHealth ? _salvageItemFullHealthRef.Get() : _salvageItemRef.Get();
			if (salvageAsset is SpawnAsset spawnAsset)
			{
				for (int index = 0; index < itemCount; ++index)
				{
					ItemAsset itemAsset = SpawnTableTool.Resolve<ItemAsset>(spawnAsset, EAssetType.ITEM,
						OnGetItemRecoveredOnSalvageSpawnTableErrorContext);
					if (itemAsset != null)
					{
						player.inventory.forceAddItem(new Item(itemAsset, EItemOrigin.NATURE), true);
					}
				}
				return;
			}

			ItemAsset salvageItemAsset = salvageAsset as ItemAsset;
			if (salvageItemAsset == null)
			{
				// Note: full health does not default to self here because it defaults to self in PopulateAsset.
				// I.e., it may have been set to null intentionally.
				if (!fullHealth)
				{
					salvageItemAsset = FindDefaultSalvageItemAsset();
				}
				if (salvageItemAsset == null)
				{
					return;
				}
			}

			for (int index = 0; index < itemCount; ++index)
			{
				player.inventory.forceAddItem(new Item(salvageItemAsset, EItemOrigin.NATURE), true);
			}
		}

		public bool DoesAnyPlaceableProvidedCraftingTagNameContainText(string text)
		{
			if (PlaceableProvidedCraftingTags != null && PlaceableProvidedCraftingTags.Length > 0)
			{
				for (int index = 0; index < PlaceableProvidedCraftingTags.Length; ++index)
				{
					ref CachingAssetRef tagRef = ref PlaceableProvidedCraftingTags[index];
					TagAsset tagAsset = tagRef.Get<TagAsset>();
					if (tagAsset == null || string.IsNullOrEmpty(tagAsset.PlainTextName))
						continue;

					if (tagAsset.PlainTextName.IndexOf(text, System.StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return true;
					}
				}
			}

			return false;
		}

		internal void SpawnItemDropsOnDestroy(Vector3 position)
		{
			int rewards = Random.Range(minItemsDroppedOnDestroy, maxItemsDroppedOnDestroy + 1);
			// Prevent players from crashing themselves with huge numbers of items.
			rewards = Mathf.Clamp(rewards, 0, 100);

			if (rewards < 1)
				return;

			Asset destroyAsset = _itemDroppedOnDestroyRef.Get();
			if (destroyAsset is SpawnAsset spawnAsset)
			{
				for (int index = 0; index < rewards; ++index)
				{
					ItemAsset itemAsset = SpawnTableTool.Resolve<ItemAsset>(spawnAsset, EAssetType.ITEM,
						OnGetItemDroppedOnDestroySpawnTableErrorContext);
					if (itemAsset != null)
					{
						ItemManager.dropItem(new Item(itemAsset, EItemOrigin.NATURE),
							position + new Vector3(Random.Range(-2.0f, 2.0f), 2.0f,
							Random.Range(-2.0f, 2.0f)), false, Dedicator.IsDedicatedServer, true);
					}
				}
				return;
			}

			ItemAsset destroyItemAsset = destroyAsset as ItemAsset;
			if (destroyItemAsset == null)
			{
				return;
			}

			for (int index = 0; index < rewards; ++index)
			{
				ItemManager.dropItem(new Item(destroyItemAsset, EItemOrigin.NATURE),
					position + new Vector3(Random.Range(-2.0f, 2.0f), 2.0f,
					Random.Range(-2.0f, 2.0f)), false, Dedicator.IsDedicatedServer, true);
			}
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (PlaceableProvidedCraftingTags != null && PlaceableProvidedCraftingTags.Length > 0)
			{
				Local localization = PlayerDashboardInventoryUI.localization;
				int sortOrder = DescSort_CraftingTags;
				builder.Append(localization.format("ItemDescription_ProvidesCraftingTags"), ++sortOrder);
				for (int index = 0; index < PlaceableProvidedCraftingTags.Length; ++index)
				{
					ref CachingAssetRef tagRef = ref PlaceableProvidedCraftingTags[index];
					TagAsset tagAsset = tagRef.Get<TagAsset>();
					if (tagAsset == null)
						continue;

					builder.Append(localization.format("ItemDescription_ListItem", tagAsset.RichTextName), ++sortOrder);
				}
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			CanZombiesTarget = p.data.ParseBool("Can_Zombies_Target", true);

			if (p.data.TryParseInt32("Items_Recovered_On_Salvage", out int itemsRecoveredOnSalvage))
			{
				MinItemsRecoveredOnSalvage = itemsRecoveredOnSalvage;
				MaxItemsRecoveredOnSalvage = itemsRecoveredOnSalvage;
			}
			else
			{
				MinItemsRecoveredOnSalvage = p.data.ParseInt32("Min_Items_Recovered_On_Salvage", 1);
				MaxItemsRecoveredOnSalvage = p.data.ParseInt32("Max_Items_Recovered_On_Salvage", 1);
			}
			if (!p.data.TryParseAssetRef("SalvageItem", out _salvageItemRef))
			{
				if (string.Equals(p.data.GetString("SalvageItem"), "this",
					System.StringComparison.InvariantCultureIgnoreCase))
				{
					_salvageItemRef = this;
				}
			}

			if (p.data.TryParseInt32("Items_Recovered_On_Salvage_Full_Health", out int itemsRecoveredOnSalvageFullHealth))
			{
				MinItemsRecoveredOnSalvageFullHealth = itemsRecoveredOnSalvageFullHealth;
				MaxItemsRecoveredOnSalvageFullHealth = itemsRecoveredOnSalvageFullHealth;
			}
			else
			{
				MinItemsRecoveredOnSalvageFullHealth = p.data.ParseInt32("Min_Items_Recovered_On_Salvage_Full_Health", 1);
				MaxItemsRecoveredOnSalvageFullHealth = p.data.ParseInt32("Max_Items_Recovered_On_Salvage_Full_Health", 1);
			}
			if (!p.data.TryParseAssetRef("SalvageItem_FullHealth", out _salvageItemFullHealthRef))
			{
				_salvageItemFullHealthRef = this;
			}

			if (p.data.TryParseInt32("Items_Dropped_On_Destroy", out int itemsDroppedOnDestroy))
			{
				minItemsDroppedOnDestroy = itemsDroppedOnDestroy;
				maxItemsDroppedOnDestroy = itemsDroppedOnDestroy;
			}
			else
			{
				minItemsDroppedOnDestroy = p.data.ParseInt32("Min_Items_Dropped_On_Destroy");
				maxItemsDroppedOnDestroy = p.data.ParseInt32("Max_Items_Dropped_On_Destroy");
			}
			if (!p.data.TryParseAssetRef("Item_Dropped_On_Destroy", out _itemDroppedOnDestroyRef))
			{
				if (string.Equals(p.data.GetString("Item_Dropped_On_Destroy"), "this",
					System.StringComparison.InvariantCultureIgnoreCase))
				{
					_itemDroppedOnDestroyRef = this;
				}
			}
			
			PlaceableProvidedCraftingTags = p.data.ParseArrayOfStructs<CachingAssetRef>("PlaceableProvidesCraftingTags");

			if (p.data.ParseBool("ExplosionEffect_CopyModelPosition"))
			{
				ExplosionEffectFlags |= EPlaceableExplosionEffectFlags.CopyModelPosition;
			}
			if (p.data.ParseBool("ExplosionEffect_CopyModelRotation"))
			{
				ExplosionEffectFlags |= EPlaceableExplosionEffectFlags.CopyModelRotation;
			}

			this.PopulateArmorFalloff(in p); // this. is necessary, at least in current C# version.
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Placeable
			// Game data for Placeable Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Placeable");
			data.Append("GUID", GUID); // Key

			data.Append("Can_Zombies_Target", CanZombiesTarget);
			data.Append("SalvageItem", SalvageItemRef);
			data.Append("Min_Items_Dropped_On_Destroy", minItemsDroppedOnDestroy);
			data.Append("Max_Items_Dropped_On_Destroy", maxItemsDroppedOnDestroy);
			data.Append("Item_Dropped_On_Destroy", ItemDroppedOnDestroyRef);
		}

		private string OnGetItemDroppedOnDestroySpawnTableErrorContext()
		{
			return $"{FriendlyName} items dropped on destroy";
		}

		private string OnGetItemRecoveredOnSalvageSpawnTableErrorContext()
		{
			return $"{FriendlyName} items recovered on salvage";
		}


		[System.Obsolete("Replaced by SalvageItemRef which supports spawn tables as well")]
		public AssetReference<ItemAsset> salvageItemRef
		{
			get => new AssetReference<ItemAsset>(SalvageItemRef.Guid);
		}

		[System.Obsolete("Replaced by ItemDroppedOnDestroyRef which supports items as well")]
		public AssetReference<SpawnAsset> ItemDroppedOnDestroy
		{
			get => new AssetReference<SpawnAsset>(_itemDroppedOnDestroyRef.Guid);
		}

		[System.Obsolete("Replaced by overload with fullHealth parameter (default false)")]
		public void GrantSalvageItems(Player player)
		{
			GrantSalvageItems(player, false);
		}
	}
}
