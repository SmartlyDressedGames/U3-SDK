////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemClothingAsset : ItemAsset
	{
		protected float _armor;
		/// <summary>
		/// Multiplier to incoming damage. Defaults to 1.0.
		/// </summary>
		public float armor => _armor;

		protected float _explosionArmor;
		/// <summary>
		/// Multiplier to explosive damage. Defaults to <see cref="armor"/> value if Armor_Explosion isn't specified.
		/// </summary>
		public float explosionArmor => _explosionArmor;

		/// <summary>
		/// Armor against falling damage. Defaults to 1.0, i.e., take the normal amount of damage.
		/// </summary>
		public float fallingDamageMultiplier
		{
			get;
			protected set;
		}

		public override bool showQuality => true;

		private bool _proofWater;
		public bool proofWater => _proofWater;

		private bool _proofFire;
		public bool proofFire => _proofFire;

		private bool _proofRadiation;
		public bool proofRadiation => _proofRadiation;

		/// <summary>
		/// If true on any worn clothing item, bones never break when falling.
		/// Defaults to false.
		/// </summary>
		public bool preventsFallingBrokenBones
		{
			get;
			protected set;
		}

		public bool visibleOnRagdoll
		{
			get;
			protected set;
		}

		public bool shouldBeVisible(bool isRagdoll)
		{
			return isRagdoll == false || visibleOnRagdoll;
		}

		public bool hairVisible
		{
			get;
			protected set;
		}

		public bool beardVisible
		{
			get;
			protected set;
		}

		/// <summary>
		/// Left-handed character skeleton is mirrored, so most item models are mirrored again to preserve appearance.
		/// Unfortunately this does not work well for some items e.g. the particle system on Elver/Dango glasses.
		/// </summary>
		internal bool shouldMirrorLeftHandedModel;

		public float movementSpeedMultiplier = 1.0f;

		/// <summary>
		/// Sound to play when equipped.
		/// </summary>
		public AudioReference wearAudio;

		public bool shouldDestroyClothingColliders
		{
			get;
			protected set;
		}

		internal GameObject cosmeticPreviewModelOverride;

		/// <summary>
		/// If set, find a child meshrenderer with this name and change its material to the character skin material.
		/// </summary>
		public string skinOverride
		{
			get;
			protected set;
		}

		/// <summary>
		/// The player can be wearing both a "real" in-game item and a cosmetic item in the same clothing slot.
		/// If true, the real item is shown rather than the cosmetic item. For example, night vision goggles
		/// are shown over any glasses cosmetic because of their gameplay-related green glow.
		/// </summary>
		public bool TakesPriorityOverCosmetic
		{
			get => hasPriorityOverCosmeticOverride ? priorityOverCosmeticOverride : GetDefaultTakesPriorityOverCosmetic();
		}

		/// <summary>
		/// Overrides value of TakesPriorityOverCosmetic if <see cref="hasPriorityOverCosmeticOverride"/> is true.
		/// </summary>
		protected bool priorityOverCosmeticOverride;
		/// <summary>
		/// If true, the value of <see cref="priorityOverCosmeticOverride"/> is used rather than <see cref="GetDefaultTakesPriorityOverCosmetic"/>.
		/// Defaults to false. True if <see cref="priorityOverCosmeticOverride"/> is set.
		/// </summary>
		protected bool hasPriorityOverCosmeticOverride;

		/// <summary>
		/// For 3D clothes. Ideally, this wouldn't be type specific, but we have a separate prefab property for each
		/// type of clothing at the moment.
		/// </summary>
		internal virtual UnityEngine.GameObject ClothingPrefab
		{
			get { return null; }
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			// As of 2023-10-09, only hats, shirts, pants, and vests are eligible for player armor,
			// but some items like masks have an armor value set.
			if (type == EItemType.HAT || type == EItemType.SHIRT || type == EItemType.PANTS || type == EItemType.VEST)
			{
				if (_armor != 1.0f)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Clothing_Armor", PlayerDashboardInventoryUI.FormatStatModifier(_armor, false, false)), DescSort_ClothingStat + DescSort_LowerIsBeneficial(_armor));
				}
				if (_explosionArmor != 1.0f)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Clothing_ExplosionArmor", PlayerDashboardInventoryUI.FormatStatModifier(_explosionArmor, false, false)), DescSort_ClothingStat + DescSort_LowerIsBeneficial(_explosionArmor));
				}
			}

			if (movementSpeedMultiplier != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ClothingMovementSpeedModifier", PlayerDashboardInventoryUI.FormatStatModifier(movementSpeedMultiplier, true, true)), DescSort_ClothingStat + DescSort_HigherIsBeneficial(movementSpeedMultiplier));
			}

			if (fallingDamageMultiplier != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_FallingDamageModifier", PlayerDashboardInventoryUI.FormatStatModifier(fallingDamageMultiplier, true, false)), DescSort_ClothingStat + DescSort_LowerIsBeneficial(fallingDamageMultiplier));
			}

			if (_proofFire)
			{
				builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_Clothing_FireProof"), true), DescSort_ClothingStat + DescSort_Beneficial);
			}
			if (_proofRadiation)
			{
				builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_Clothing_RadiationProof"), true), DescSort_ClothingStat + DescSort_Beneficial);
			}
			if (preventsFallingBrokenBones)
			{
				builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_Clothing_FallingBoneBreakingProof"), true), DescSort_ClothingStat + DescSort_Beneficial);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (isPro)
			{
				_armor = 1f;
				_explosionArmor = 1f;
				fallingDamageMultiplier = 1.0f;
			}
			else
			{
				_armor = p.data.ParseFloat("Armor");
				if (p.data.ContainsKey("Armor"))
				{
					_armor = p.data.ParseFloat("Armor");
				}
				else
				{
					_armor = 1.0f;
				}

				if (p.data.ContainsKey("Armor_Explosion"))
				{
					_explosionArmor = p.data.ParseFloat("Armor_Explosion");
				}
				else
				{
					_explosionArmor = armor;
				}

				fallingDamageMultiplier = p.data.ParseFloat("Falling_Damage_Multiplier", defaultValue: 1.0f);

				_proofWater = p.data.ContainsKey("Proof_Water");
				_proofFire = p.data.ContainsKey("Proof_Fire");
				_proofRadiation = p.data.ContainsKey("Proof_Radiation");
				preventsFallingBrokenBones = p.data.ParseBool("Prevents_Falling_Broken_Bones");

				movementSpeedMultiplier = p.data.ParseFloat("Movement_Speed_Multiplier", defaultValue: 1.0f);
			}

			visibleOnRagdoll = p.data.ParseBool("Visible_On_Ragdoll", defaultValue: true);

			// Added late in development for mesh override shirts, so most items do not have these set.
			hairVisible = p.data.ParseBool("Hair_Visible", defaultValue: true);
			beardVisible = p.data.ParseBool("Beard_Visible", defaultValue: true);

			shouldMirrorLeftHandedModel = p.data.ParseBool("Mirror_Left_Handed_Model", defaultValue: true);

			if (p.data.ContainsKey("WearAudio"))
			{
				wearAudio = p.data.ReadAudioReference("WearAudio", p.bundle);
			}
			else
			{
				if (type == EItemType.BACKPACK || type == EItemType.VEST)
				{
					wearAudio = new AudioReference("core.masterbundle", "Sounds/Zipper.mp3");
				}
				else
				{
					wearAudio = new AudioReference("core.masterbundle", "Sounds/Sleeve.mp3");
				}
			}

			shouldDestroyClothingColliders = p.data.ParseBool("Destroy_Clothing_Colliders", defaultValue: true);
			hasPriorityOverCosmeticOverride = p.data.TryParseBool("Priority_Over_Cosmetic", out priorityOverCosmeticOverride);

			skinOverride = p.data.GetString("Skin_Override");

			if (isPro && !Dedicator.IsDedicatedServer)
			{
				cosmeticPreviewModelOverride = p.bundle.load<GameObject>("CosmeticPreviewOverride");
			}
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Clothing
			// Game data for Clothing Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Clothing");
			data.Append("GUID", GUID); // Key

			data.Append("Armor", armor);
			data.Append("Armor_Explosion", explosionArmor);
			data.Append("Falling_Damage_Multiplier", fallingDamageMultiplier);
			data.Append("Proof_Water", proofWater);
			data.Append("Proof_Fire", proofFire);
			data.Append("Proof_Radiation", proofRadiation);
			data.Append("Prevents_Falling_Broken_Bones", preventsFallingBrokenBones);
			data.Append("Movement_Speed_Multiplier", movementSpeedMultiplier);
			data.Append("Mirror_Left_Handed_Model", shouldMirrorLeftHandedModel);
			data.Append("Priority_Over_Cosmetic", hasPriorityOverCosmeticOverride);
		}

		protected override AudioReference GetDefaultInventoryAudio()
		{
			if (size_x <= 1 || size_y <= 1)
			{
				return new AudioReference("core.masterbundle", "Sounds/Inventory/LightCloth.asset");
			}

			if (rarity == EItemRarity.COMMON || rarity == EItemRarity.UNCOMMON)
			{
				return new AudioReference("core.masterbundle", "Sounds/Inventory/LightClothEquipment.asset");
			}
			else
			{
				return new AudioReference("core.masterbundle", "Sounds/Inventory/MediumClothEquipment.asset");
			}
		}

		/// <summary>
		/// The player can be wearing both a "real" in-game item and a cosmetic item in the same clothing slot.
		/// This is called on the real item if <see cref="priorityOverCosmeticOverride"/> has not been set.
		/// If true, the real item is shown rather than the cosmetic item. If false, the cosmetic item can be seen.
		/// </summary>
		protected virtual bool GetDefaultTakesPriorityOverCosmetic()
		{
			return false;
		}
	}
}
