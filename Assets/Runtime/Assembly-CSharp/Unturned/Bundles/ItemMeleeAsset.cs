////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemMeleeAsset : ItemWeaponAsset
	{
		protected AudioClip _use;
		public AudioClip use => _use;

		public override byte[] getState(EItemOrigin origin)
		{
			if (isLight)
			{
				return new byte[1]
				{
					1 // interact state 
				};
			}
			else
			{
				return new byte[0];
			}
		}

		private float _strength;
		public float strength => _strength;

		private float _weak;
		public float weak => _weak;

		private float _strong;
		public float strong => _strong;

		private byte _stamina;
		public byte stamina => _stamina;

		private bool _isRepair;
		public bool isRepair => _isRepair;

		private bool _isRepeated;
		public bool isRepeated => _isRepeated;

		private bool _isLight;
		public bool isLight => _isLight;

		public PlayerSpotLightConfig lightConfig
		{
			get;
			protected set;
		}

		public override bool showQuality => true;

		public override bool shouldFriendlySentryTargetUser => true;

		public float alertRadius
		{
			get;
			protected set;
		}

		protected override bool doesItemTypeHaveSkins => true;

		public AudioReference impactAudio;

		internal NPCRewardsList weakAttackQuestRewards;
		internal NPCRewardsList strongAttackQuestRewards;

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			// "Repeated" melee weapons (e.g., blowtorch, chainsaw) don't have strong attacks.
			if (!_isRepeated)
			{
				if (strength != 1.0f)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Melee_StrongAttackModifier", PlayerDashboardInventoryUI.FormatStatModifier(strength, true, true)), DescSort_MeleeStat + DescSort_HigherIsBeneficial(strength));
				}

				if (stamina > 0)
				{
					string staminaText = PlayerDashboardInventoryUI.FormatStatColor(stamina.ToString(), false);
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Melee_StrongAttackStamina", staminaText), DescSort_MeleeStat);
				}
			}

			BuildNonExplosiveDescription(builder, itemInstance);
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_use = LoadRedirectableAsset<AudioClip>(p.bundle, "Use", p.data, "AttackAudioClip");

			_strength = p.data.ParseFloat("Strength");

			_weak = p.data.ParseFloat("Weak", 0.5f);
			_strong = p.data.ParseFloat("Strong", 0.33f);

			_stamina = p.data.ParseUInt8("Stamina");

			_isRepair = p.data.ContainsKey("Repair");
			_isRepeated = p.data.ContainsKey("Repeated");
			_isLight = p.data.ContainsKey("Light");
			if (isLight)
			{
				lightConfig = new PlayerSpotLightConfig(p.data);
			}

			if (p.data.ContainsKey("Alert_Radius"))
			{
				alertRadius = p.data.ParseFloat("Alert_Radius");
			}
			else
			{
				alertRadius = 8;
			}

			impactAudio = p.data.ReadAudioReference("ImpactAudioDef", p.bundle);

			weakAttackQuestRewards.Parse(p.data, p.localization, this, "Weak_Attack_Quest_Rewards", "Weak_Attack_Quest_Reward_");
			strongAttackQuestRewards.Parse(p.data, p.localization, this, "Strong_Attack_Quest_Rewards", "Strong_Attack_Quest_Reward_");
		}
	}
}
