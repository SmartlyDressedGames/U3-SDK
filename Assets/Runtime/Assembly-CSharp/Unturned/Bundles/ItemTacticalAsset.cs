////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemTacticalAssetMeleeProperties
	{
		public float MeleeRange
		{
			get;
			set;
		}

		public PlayerDamageMultiplier MeleePlayerDamageMultiplier
		{
			get;
			set;
		}

		public DamagePlayerParameters.Bleeding MeleePlayerDamageBleeding
		{
			get;
			set;
		}

		public DamagePlayerParameters.Bones MeleePlayerDamageBones
		{
			get;
			set;
		}

		public ZombieDamageMultiplier MeleeZombieDamageMultiplier
		{
			get;
			set;
		}

		public EZombieStunOverride MeleeZombieStunOverride
		{
			get;
			set;
		}

		public AnimalDamageMultiplier MeleeAnimalDamageMultiplier
		{
			get;
			set;
		}

		/// <summary>
		/// Get animal or player damage based on game mode config.
		/// </summary>
		public IDamageMultiplier MeleeAnimalOrPlayerDamageMultiplier
		{
			get
			{
				bool usePlayerDmg = Provider.modeConfigData.Animals.Weapons_Use_Player_Damage;
				return usePlayerDmg ? MeleePlayerDamageMultiplier : MeleeAnimalDamageMultiplier;
			}
		}

		/// <summary>
		/// Get zombie or player damage based on game mode config.
		/// </summary>
		public IDamageMultiplier MeleeZombieOrPlayerDamageMultiplier
		{
			get
			{
				bool usePlayerDmg = Provider.modeConfigData.Zombies.Weapons_Use_Player_Damage;
				return usePlayerDmg ? MeleePlayerDamageMultiplier : MeleeZombieDamageMultiplier;
			}
		}

		public float MeleeZombieRagdollForceMultiplier
		{
			get;
			set;
		}

		public void InitPlayerDamageParameters(ref DamagePlayerParameters parameters)
		{
			parameters.bleedingModifier = MeleePlayerDamageBleeding;
			parameters.bonesModifier = MeleePlayerDamageBones;
		}

		internal void BuildDescription(ItemDescriptionBuilder builder)
		{
			if (MeleeRange > 0.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponRange", MeasurementTool.FormatLengthString(MeleeRange)), ItemAsset.DescSort_Weapon_NonExplosive_Common);
			}

			int playerDamageSortOrder = ItemAsset.DescSort_Weapon_NonExplosive_PlayerDamage;

			if (MeleePlayerDamageMultiplier.damage > 0.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Head", Mathf.FloorToInt(MeleePlayerDamageMultiplier.damage * MeleePlayerDamageMultiplier.skull)), playerDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Body", Mathf.FloorToInt(MeleePlayerDamageMultiplier.damage * MeleePlayerDamageMultiplier.spine)), playerDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Arm", Mathf.FloorToInt(MeleePlayerDamageMultiplier.damage * MeleePlayerDamageMultiplier.arm)), playerDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Leg", Mathf.FloorToInt(MeleePlayerDamageMultiplier.damage * MeleePlayerDamageMultiplier.leg)), playerDamageSortOrder++);
			}

			switch (MeleePlayerDamageBleeding)
			{
				case DamagePlayerParameters.Bleeding.Always:
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBleeding_Always"), playerDamageSortOrder);
					break;

				case DamagePlayerParameters.Bleeding.Heal:
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBleeding_Heal"), playerDamageSortOrder);
					break;
			}

			switch (MeleePlayerDamageBones)
			{
				case DamagePlayerParameters.Bones.Always:
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBones_Always"), playerDamageSortOrder);
					break;

				case DamagePlayerParameters.Bones.Heal:
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBones_Heal"), playerDamageSortOrder);
					break;
			}

			if (MeleeZombieDamageMultiplier.damage > 0.0f)
			{
				int zombieDamageSortOrder = ItemAsset.DescSort_Weapon_NonExplosive_ZombieDamage;
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Head", Mathf.FloorToInt(MeleeZombieDamageMultiplier.damage * MeleeZombieDamageMultiplier.skull)), zombieDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Body", Mathf.FloorToInt(MeleeZombieDamageMultiplier.damage * MeleeZombieDamageMultiplier.spine)), zombieDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Arm", Mathf.FloorToInt(MeleeZombieDamageMultiplier.damage * MeleeZombieDamageMultiplier.arm)), zombieDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Leg", Mathf.FloorToInt(MeleeZombieDamageMultiplier.damage * MeleeZombieDamageMultiplier.leg)), zombieDamageSortOrder++);
			}

			if (MeleeAnimalDamageMultiplier.damage > 0.0f)
			{
				int animalDamageSortOrder = ItemAsset.DescSort_Weapon_NonExplosive_AnimalDamage;
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Head", Mathf.FloorToInt(MeleeAnimalDamageMultiplier.damage * MeleeAnimalDamageMultiplier.skull)), animalDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Body", Mathf.FloorToInt(MeleeAnimalDamageMultiplier.damage * MeleeAnimalDamageMultiplier.spine)), animalDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Limb", Mathf.FloorToInt(MeleeAnimalDamageMultiplier.damage * MeleeAnimalDamageMultiplier.leg)), animalDamageSortOrder++);
			}
		}

		internal void PopulateAsset(in PopulateAssetParameters p)
		{
			MeleeRange = p.data.ParseFloat("Melee_Range", 2.0f);

			MeleePlayerDamageMultiplier = new PlayerDamageMultiplier(p.data.ParseFloat("Melee_Player_Damage", 40), p.data.ParseFloat("Melee_Player_Leg_Multiplier", 0.6f), p.data.ParseFloat("Melee_Player_Arm_Multiplier", 0.6f), p.data.ParseFloat("Melee_Player_Spine_Multiplier", 0.8f), p.data.ParseFloat("Melee_Player_Skull_Multiplier", 1.1f));
			MeleePlayerDamageBleeding = p.data.ParseEnum<DamagePlayerParameters.Bleeding>("Melee_Player_Damage_Bleeding");
			MeleePlayerDamageBones = p.data.ParseEnum<DamagePlayerParameters.Bones>("Melee_Player_Damage_Bones");
			MeleeZombieDamageMultiplier = new ZombieDamageMultiplier(p.data.ParseFloat("Melee_Zombie_Damage", 40), p.data.ParseFloat("Melee_Zombie_Leg_Multiplier", 0.3f), p.data.ParseFloat("Melee_Zombie_Arm_Multiplier", 0.3f), p.data.ParseFloat("Melee_Zombie_Spine_Multiplier", 0.6f), p.data.ParseFloat("Melee_Zombie_Skull_Multiplier", 1.1f));
			MeleeAnimalDamageMultiplier = new AnimalDamageMultiplier(p.data.ParseFloat("Melee_Animal_Damage", 40), p.data.ParseFloat("Melee_Animal_Leg_Multiplier", 0.3f), p.data.ParseFloat("Melee_Animal_Spine_Multiplier", 0.6f), p.data.ParseFloat("Melee_Animal_Skull_Multiplier", 1.1f));

			if (p.data.ContainsKey("Melee_Stun_Zombie_Always"))
			{
				MeleeZombieStunOverride = EZombieStunOverride.Always;
			}
			else if (p.data.ContainsKey("Melee_Stun_Zombie_Never"))
			{
				MeleeZombieStunOverride = EZombieStunOverride.Never;
			}
			else
			{
				MeleeZombieStunOverride = EZombieStunOverride.None;
			}

			MeleeZombieRagdollForceMultiplier = p.data.ParseFloat("Melee_Zombie_Ragdoll_Force_Multiplier", 1f);
		}
	}

	public class ItemTacticalAsset : ItemCaliberAsset
	{
		protected GameObject _tactical;
		public GameObject tactical => _tactical;

		private bool _isLaser;
		public bool isLaser => _isLaser;

		private bool _isLight;
		public bool isLight => _isLight;

		public PlayerSpotLightConfig lightConfig
		{
			get;
			protected set;
		}

		private bool _isRangefinder;
		public bool isRangefinder => _isRangefinder;

		private bool _isMelee;
		public bool isMelee => _isMelee;

		public ItemTacticalAssetMeleeProperties MeleeProperties
		{
			get;
			set;
		}

		public Color laserColor
		{
			get;
			protected set;
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (MeleeProperties != null)
			{
				MeleeProperties.BuildDescription(builder);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_tactical = loadRequiredAsset<GameObject>(p.bundle, "Tactical");

			_isLaser = p.data.ContainsKey("Laser");

			_isLight = p.data.ContainsKey("Light");
			if (isLight)
			{
				lightConfig = new PlayerSpotLightConfig(p.data);
			}

			_isRangefinder = p.data.ContainsKey("Rangefinder");
			_isMelee = p.data.ContainsKey("Melee");

			if (_isMelee)
			{
				// Ideally, weapon properties in common here and ItemWeaponAsset should be merged into a common class.
				MeleeProperties = new ItemTacticalAssetMeleeProperties();
				MeleeProperties.PopulateAsset(in p);
			}

			Color _laserColor = p.data.LegacyParseColor("Laser_Color", Color.red);
			_laserColor = MathfEx.Clamp01(_laserColor);
			_laserColor.a = 1.0f;
			laserColor = _laserColor;
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Tactical
			// Game data for Tactical Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Tactical");
			data.Append("GUID", GUID); // Key

			data.Append("Laser", isLaser);
			data.Append("Light", isLight);
			data.Append("Rangefinder", isRangefinder);
			data.Append("Melee", isMelee);
			data.Append("Laser_Color", laserColor);
		}
	}
}
