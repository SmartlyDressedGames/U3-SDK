////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class ItemConsumeableAsset : ItemWeaponAsset
	{
		protected AudioClip _use;
		public AudioClip use => _use;

		public bool ShouldRandomizeUseAudioPitch
		{
			get;
			set;
		}

		private byte _health;
		public byte health => _health;

		private byte _food;
		public byte food => _food;

		private byte _water;
		public byte water => _water;

		private byte _virus;
		public byte virus => _virus;

		private byte _disinfectant;
		public byte disinfectant => _disinfectant;

		private byte _energy;
		public byte energy => _energy;

		private byte _vision;
		public byte vision => _vision;

		public sbyte oxygen
		{
			get;
			protected set;
		}

		private uint _warmth;
		public uint warmth => _warmth;

		/// <summary>
		/// Experience to add or subtract when used. Defaults to zero.
		/// </summary>
		public int experience;

		public enum Bleeding
		{
			None,
			Heal,
			Cut,
		}

		public Bleeding bleedingModifier
		{
			get;
			protected set;
		}

		public enum Bones
		{
			None,
			Heal,
			Break,
		}

		public Bones bonesModifier
		{
			get;
			protected set;
		}

		private bool _hasAid;
		public bool hasAid => _hasAid;

		public bool foodConstrainsWater
		{
			get;
			protected set;
		}

		public bool shouldDeleteAfterUse
		{
			get;
			protected set;
		}

		public override bool showQuality => type == EItemType.FOOD || type == EItemType.WATER;

		private System.Guid _explosionEffectGuid;
		public System.Guid ExplosionEffectGuid => _explosionEffectGuid;

		protected ushort _explosion;
		public ushort explosion
		{
			[System.Obsolete]
			get => _explosion;
		}

		public bool IsExplosionEffectRefNull()
		{
#pragma warning disable
			return _explosion == 0 && _explosionEffectGuid.IsEmpty();
#pragma warning restore
		}

		public EffectAsset FindExplosionEffectAsset()
		{
#pragma warning disable
			return Assets.FindEffectAssetByGuidOrLegacyId(_explosionEffectGuid, _explosion);
#pragma warning restore
		}

		public bool IsExplosive
		{
			get;
			private set;
		}

		public INPCReward[] questRewards
		{
			get => questRewardsList.rewards;
		}

		protected NPCRewardsList questRewardsList;

		public SpawnTableReward itemRewards
		{
			get;
			protected set;
		}

		/// <summary>
		/// Canned beans have skins from April Fools.
		/// </summary>
		protected override bool doesItemTypeHaveSkins => id == 13;

		public override bool shouldFriendlySentryTargetUser
		{
			get
			{
				if (IsExplosive)
				{
					return true;
				}
				else
				{
					return base.shouldFriendlySentryTargetUser;
				}
			}
		}

		public void GrantQuestRewards(Player player)
		{
			questRewardsList.Grant(player);
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (_health > 0)
			{
				string healthText = PlayerDashboardInventoryUI.FormatStatColor(_health.ToString(), true);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_HealthPositive", healthText), DescSort_ConsumeableStat + DescSort_Beneficial);
			}

			if (_food > 0)
			{
				string foodText = PlayerDashboardInventoryUI.FormatStatColor(_food.ToString(), true);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_FoodPositive", foodText), DescSort_ConsumeableStat + DescSort_Beneficial);
			}

			if (_water > 0)
			{
				string waterText = PlayerDashboardInventoryUI.FormatStatColor(water.ToString(), true);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_WaterPositive", waterText), DescSort_ConsumeableStat + DescSort_Beneficial);
			}

			if (_virus > 0)
			{
				string virusText = PlayerDashboardInventoryUI.FormatStatColor(virus.ToString(), false);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_VirusNegative", virusText), DescSort_ConsumeableStat + DescSort_Detrimental);
			}

			if (_disinfectant > 0)
			{
				string virusText = PlayerDashboardInventoryUI.FormatStatColor(_disinfectant.ToString(), true);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_VirusPositive", virusText), DescSort_ConsumeableStat + DescSort_Beneficial);
			}

			if (_energy > 0)
			{
				string energyText = PlayerDashboardInventoryUI.FormatStatColor(_energy.ToString(), true);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_StaminaPositive", energyText), DescSort_ConsumeableStat + DescSort_Beneficial);
			}

			if (oxygen > 0)
			{
				string oxygenText = PlayerDashboardInventoryUI.FormatStatColor(oxygen.ToString(), true);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_OxygenPositive", oxygenText), DescSort_ConsumeableStat + DescSort_Beneficial);
			}
			else if (oxygen < 0)
			{
				string oxygenText = PlayerDashboardInventoryUI.FormatStatColor((-oxygen).ToString(), false);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_OxygenNegative", oxygenText), DescSort_ConsumeableStat + DescSort_Detrimental);
			}

			int roundedWarmth = Mathf.RoundToInt(_warmth / 12.5f);
			if (roundedWarmth > 0)
			{
				string warmthText = PlayerDashboardInventoryUI.FormatStatColor($"{roundedWarmth} s", true);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_WarmthPositive", warmthText), DescSort_ConsumeableStat + DescSort_Beneficial);
			}

			if (itemInstance != null && itemInstance.quality < 50 && _food + _water > 0)
			{
				builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_ConsumeableMoldy"), false), DescSort_ConsumeableStat + DescSort_Detrimental);
			}

			switch (bleedingModifier)
			{
				case Bleeding.Heal:
					builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_ConsumeableBleeding_Heal"), true), DescSort_ConsumeableStat + DescSort_Beneficial);
					break;

				case Bleeding.Cut:
					builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_ConsumeableBleeding_Cut"), false), DescSort_ConsumeableStat + DescSort_Detrimental);
					break;
			}

			switch (bonesModifier)
			{
				case Bones.Heal:
					builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_ConsumeableBones_Heal"), true), DescSort_ConsumeableStat + DescSort_Beneficial);
					break;

				case Bones.Break:
					builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_ConsumeableBones_Break"), false), DescSort_ConsumeableStat + DescSort_Detrimental);
					break;
			}

			if (IsExplosive)
			{
				builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_Explosive"), false), DescSort_Important);

				BuildExplosiveDescription(builder, itemInstance);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_use = LoadRedirectableAsset<AudioClip>(p.bundle, "Use", p.data, "ConsumeAudioClip");
			ShouldRandomizeUseAudioPitch = p.data.ParseBool("Randomize_Consume_Audio_Pitch", true);

			_health = p.data.ParseUInt8("Health");
			_food = p.data.ParseUInt8("Food");
			_water = p.data.ParseUInt8("Water");
			_virus = p.data.ParseUInt8("Virus");
			_disinfectant = p.data.ParseUInt8("Disinfectant");
			_energy = p.data.ParseUInt8("Energy");
			_vision = p.data.ParseUInt8("Vision");
			oxygen = p.data.ParseInt8("Oxygen");
			_warmth = p.data.ParseUInt32("Warmth");
			experience = p.data.ParseInt32("Experience");

			if (p.data.ContainsKey("Bleeding"))
			{
				bleedingModifier = Bleeding.Heal;
			}
			else
			{
				bleedingModifier = p.data.ParseEnum<Bleeding>("Bleeding_Modifier");
			}

			if (p.data.ContainsKey("Broken"))
			{
				bonesModifier = Bones.Heal;
			}
			else
			{
				bonesModifier = p.data.ParseEnum<Bones>("Bones_Modifier");
			}

			_hasAid = p.data.ContainsKey("Aid");
			foodConstrainsWater = food >= water;//data.has("Food_Constrains_Water");
			shouldDeleteAfterUse = p.data.ParseBool("Should_Delete_After_Use", defaultValue: true);

			questRewardsList.Parse(p.data, p.localization, this, "Quest_Rewards", "Quest_Reward_");

			ushort itemRewardTableID = p.data.ParseUInt16("Item_Reward_Spawn_ID");
			int minItemRewards = p.data.ParseInt32("Min_Item_Rewards");
			int maxItemRewards = p.data.ParseInt32("Max_Item_Rewards");
			itemRewards = new SpawnTableReward(itemRewardTableID, minItemRewards, maxItemRewards);

			_explosion = p.data.ParseGuidOrLegacyId("Explosion", out _explosionEffectGuid);
			IsExplosive = !IsExplosionEffectRefNull();
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Consumeable
			// Game data for Consumeable Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Consumeable");
			data.Append("GUID", GUID); // Key

			data.Append("Health", health);
			data.Append("Food", food);
			data.Append("Water", water);
			data.Append("Virus", virus);
			data.Append("Disinfectant", disinfectant);
			data.Append("Energy", energy);
			data.Append("Vision", vision);
			data.Append("Oxygen", oxygen);
			data.Append("Warmth", warmth);
			data.Append("Experience", experience);
			data.Append("Bleeding_Modifier", bleedingModifier);
			data.Append("Bones_Modifier", bonesModifier);
			data.Append("Aid", hasAid);
			data.Append("Should_Delete_After_Use", shouldDeleteAfterUse);
			data.Append("Item_Reward_Spawn_ID", itemRewards.tableID);
			data.Append("Min_Item_Rewards", itemRewards.min);
			data.Append("Max_Item_Rewards", itemRewards.max);
#pragma warning disable
			data.Append("Explosion", explosion);
#pragma warning restore
		}

		[System.Obsolete("Removed shouldSend parameter")]
		public void grantQuestRewards(Player player, bool shouldSend)
		{
			GrantQuestRewards(player);
		}
	}
}
