////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemWeaponAsset : ItemAsset
	{
		public float range;

		public byte[] bladeIDs
		{
			get;
			protected set;
		}


		public PlayerDamageMultiplier playerDamageMultiplier;

		public DamagePlayerParameters.Bleeding playerDamageBleeding
		{
			get;
			protected set;
		}
		public DamagePlayerParameters.Bones playerDamageBones
		{
			get;
			protected set;
		}

		/// <summary>
		/// Added to player's food value.
		/// </summary>
		public float playerDamageFood
		{
			get;
			protected set;
		}

		/// <summary>
		/// Added to player's water value.
		/// </summary>
		public float playerDamageWater
		{
			get;
			protected set;
		}

		/// <summary>
		/// Added to player's virus value.
		/// </summary>
		public float playerDamageVirus
		{
			get;
			protected set;
		}

		/// <summary>
		/// Added to player's hallucination value.
		/// </summary>
		public float playerDamageHallucination
		{
			get;
			protected set;
		}


		public ZombieDamageMultiplier zombieDamageMultiplier;

		public EZombieStunOverride zombieStunOverride
		{
			get;
			protected set;
		}

		public float ZombieRagdollForceMultiplier
		{
			get;
			set;
		}

		public AnimalDamageMultiplier animalDamageMultiplier;

		/// <summary>
		/// Get animal or player damage based on game mode config.
		/// </summary>
		public IDamageMultiplier animalOrPlayerDamageMultiplier
		{
			get
			{
				bool usePlayerDmg = Provider.modeConfigData.Animals.Weapons_Use_Player_Damage;
				return usePlayerDmg ? playerDamageMultiplier : (IDamageMultiplier) animalDamageMultiplier;
			}
		}


		/// <summary>
		/// Get zombie or player damage based on game mode config.
		/// </summary>
		public IDamageMultiplier zombieOrPlayerDamageMultiplier
		{
			get
			{
				bool usePlayerDmg = Provider.modeConfigData.Zombies.Weapons_Use_Player_Damage;
				return usePlayerDmg ? playerDamageMultiplier : (IDamageMultiplier) zombieDamageMultiplier;
			}
		}


		public float barricadeDamage;


		public float structureDamage;


		public float vehicleDamage;


		public float resourceDamage;


		public float objectDamage;


		public float durability;


		public byte wear;


		public bool isInvulnerable;

		/// <summary>
		/// Should player/animal/zombie surface be nulled on hit?
		/// May be useful for weapons going overboard with the blood splatters.
		/// </summary>
		public bool allowFleshFx
		{
			get;
			protected set;
		}

		/// <summary>
		/// Should this weapon bypass the DamageTool.allowedToDamagePlayer test?
		/// Used by weapons that heal players in PvE.
		/// </summary>
		public bool bypassAllowedToDamagePlayer
		{
			get;
			protected set;
		}

		public bool hasBladeID(byte bladeID)
		{
			// Weapons should always load at least one bladeID which defaults to zero, so weapons
			// can damage trees and objects by default. (issue #3357)

			if (bladeIDs != null)
			{
				for (int index = 0; index < bladeIDs.Length; ++index)
				{
					if (bladeIDs[index] == bladeID)
					{
						return true;
					}
				}
			}

			return false;
		}

		public void initPlayerDamageParameters(ref DamagePlayerParameters parameters)
		{
			parameters.bleedingModifier = playerDamageBleeding;
			parameters.bonesModifier = playerDamageBones;
			parameters.foodModifier = playerDamageFood;
			parameters.waterModifier = playerDamageWater;
			parameters.virusModifier = playerDamageVirus;
			parameters.hallucinationModifier = playerDamageHallucination;
		}

		/// <summary>
		/// Please refer to ItemWeaponAsset.BuildDescription for an explanation of why this is necessary.
		/// </summary>
		protected void BuildExplosiveDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			int sortOrder = DescSort_Weapon_Explosive_RangeAndDamage;
			builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBlastRadius", MeasurementTool.FormatLengthString(range)), sortOrder++);
			builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionPlayerDamage", Mathf.RoundToInt(playerDamageMultiplier.damage)), sortOrder);
			builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionZombieDamage", Mathf.RoundToInt(zombieDamageMultiplier.damage)), sortOrder);
			builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionAnimalDamage", Mathf.RoundToInt(animalDamageMultiplier.damage)), sortOrder);
			builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBarricadeDamage", Mathf.RoundToInt(barricadeDamage)), sortOrder);
			builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionStructureDamage", Mathf.RoundToInt(structureDamage)), sortOrder);
			builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionVehicleDamage", Mathf.RoundToInt(vehicleDamage)), sortOrder);
			builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionResourceDamage", Mathf.RoundToInt(resourceDamage)), sortOrder);
			builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionObjectDamage", Mathf.RoundToInt(objectDamage)), sortOrder);
		}

		/// <summary>
		/// Please refer to ItemWeaponAsset.BuildDescription for an explanation of why this is necessary.
		/// </summary>
		protected void BuildNonExplosiveDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			if (range > 0.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponRange", MeasurementTool.FormatLengthString(range)), DescSort_Weapon_NonExplosive_Common);
			}

			int playerDamageSortOrder = DescSort_Weapon_NonExplosive_PlayerDamage;

			if (playerDamageMultiplier.damage > 0.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Head", Mathf.FloorToInt(playerDamageMultiplier.damage * playerDamageMultiplier.skull)), playerDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Body", Mathf.FloorToInt(playerDamageMultiplier.damage * playerDamageMultiplier.spine)), playerDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Arm", Mathf.FloorToInt(playerDamageMultiplier.damage * playerDamageMultiplier.arm)), playerDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Leg", Mathf.FloorToInt(playerDamageMultiplier.damage * playerDamageMultiplier.leg)), playerDamageSortOrder++);
			}

			int roundedPlayerDamageFood = Mathf.RoundToInt(playerDamageFood);
			if (roundedPlayerDamageFood > 0)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_FoodPositive", roundedPlayerDamageFood.ToString()), playerDamageSortOrder);
			}
			else if (roundedPlayerDamageFood < 0)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_FoodNegative", (-roundedPlayerDamageFood).ToString()), playerDamageSortOrder);
			}

			int roundedPlayerDamageWater = Mathf.RoundToInt(playerDamageWater);
			if (roundedPlayerDamageWater > 0)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_WaterPositive", roundedPlayerDamageWater.ToString()), playerDamageSortOrder);
			}
			else if (roundedPlayerDamageWater < 0)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_WaterNegative", (-roundedPlayerDamageWater).ToString()), playerDamageSortOrder);
			}

			int roundedPlayerDamageVirus = Mathf.RoundToInt(playerDamageVirus);
			if (roundedPlayerDamageVirus > 0)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_VirusPositive", roundedPlayerDamageVirus.ToString()), playerDamageSortOrder);
			}
			else if (roundedPlayerDamageVirus < 0)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_VirusNegative", (-roundedPlayerDamageVirus).ToString()), playerDamageSortOrder);
			}

			int roundedPlayerDamageHallucination = Mathf.RoundToInt(playerDamageHallucination);
			if (roundedPlayerDamageHallucination > 0)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_HallucinationPositive", $"{roundedPlayerDamageHallucination} s"), playerDamageSortOrder);
			}
			else if (roundedPlayerDamageHallucination < 0)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_HallucinationNegative", $"{-roundedPlayerDamageHallucination} s"), playerDamageSortOrder);
			}

			switch (playerDamageBleeding)
			{
				case DamagePlayerParameters.Bleeding.Always:
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBleeding_Always"), playerDamageSortOrder);
					break;

				case DamagePlayerParameters.Bleeding.Heal:
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBleeding_Heal"), playerDamageSortOrder);
					break;
			}

			switch (playerDamageBones)
			{
				case DamagePlayerParameters.Bones.Always:
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBones_Always"), playerDamageSortOrder);
					break;

				case DamagePlayerParameters.Bones.Heal:
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBones_Heal"), playerDamageSortOrder);
					break;
			}

			if (isInvulnerable)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Invulnerable"), DescSort_Weapon_NonExplosive_Common);
			}

			if (zombieDamageMultiplier.damage > 0.0f)
			{
				int zombieDamageSortOrder = DescSort_Weapon_NonExplosive_ZombieDamage;
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Head", Mathf.FloorToInt(zombieDamageMultiplier.damage * zombieDamageMultiplier.skull)), zombieDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Body", Mathf.FloorToInt(zombieDamageMultiplier.damage * zombieDamageMultiplier.spine)), zombieDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Arm", Mathf.FloorToInt(zombieDamageMultiplier.damage * zombieDamageMultiplier.arm)), zombieDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Leg", Mathf.FloorToInt(zombieDamageMultiplier.damage * zombieDamageMultiplier.leg)), zombieDamageSortOrder++);
			}

			if (animalDamageMultiplier.damage > 0.0f)
			{
				int animalDamageSortOrder = DescSort_Weapon_NonExplosive_AnimalDamage;
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Head", Mathf.FloorToInt(animalDamageMultiplier.damage * animalDamageMultiplier.skull)), animalDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Body", Mathf.FloorToInt(animalDamageMultiplier.damage * animalDamageMultiplier.spine)), animalDamageSortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Limb", Mathf.FloorToInt(animalDamageMultiplier.damage * animalDamageMultiplier.leg)), animalDamageSortOrder++);
			}

			if (barricadeDamage > 0.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Barricade", Mathf.FloorToInt(barricadeDamage)), DescSort_Weapon_NonExplosive_OtherDamage);
			}
			if (structureDamage > 0.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Structure", Mathf.FloorToInt(structureDamage)), DescSort_Weapon_NonExplosive_OtherDamage);
			}
			if (vehicleDamage > 0.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Vehicle", Mathf.FloorToInt(vehicleDamage)), DescSort_Weapon_NonExplosive_OtherDamage);
			}
			if (resourceDamage > 0.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Resource", Mathf.FloorToInt(resourceDamage)), DescSort_Weapon_NonExplosive_OtherDamage);
			}
			if (objectDamage > 0.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Object", Mathf.FloorToInt(objectDamage)), DescSort_Weapon_NonExplosive_OtherDamage);
			}
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			// Unfortunately, weapon damage properties are shared between bullet/melee/explosion/projectile/etc at the moment,
			// so subclass is responsible for calling the appropriate description building method.
		}

		public ItemWeaponAsset() : base()
		{
			playerDamageMultiplier = new PlayerDamageMultiplier(0, 0, 0, 0, 0);
			zombieDamageMultiplier = new ZombieDamageMultiplier(0, 0, 0, 0, 0);
			animalDamageMultiplier = new AnimalDamageMultiplier(0, 0, 0, 0);
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			int bladeIDCount = p.data.ParseInt32("BladeIDs");
			if (bladeIDCount > 0)
			{
				bladeIDs = new byte[bladeIDCount];
				for (int index = 0; index < bladeIDCount; ++index)
				{
					bladeIDs[index] = p.data.ParseUInt8("BladeID_" + index);
				}
			}
			else
			{
				bladeIDs = new byte[1];
				bladeIDs[0] = p.data.ParseUInt8("BladeID");
			}

			range = p.data.ParseFloat("Range");

			playerDamageMultiplier = new PlayerDamageMultiplier(p.data.ParseFloat("Player_Damage"), p.data.ParseFloat("Player_Leg_Multiplier"), p.data.ParseFloat("Player_Arm_Multiplier"), p.data.ParseFloat("Player_Spine_Multiplier"), p.data.ParseFloat("Player_Skull_Multiplier"));
			playerDamageBleeding = p.data.ParseEnum<DamagePlayerParameters.Bleeding>("Player_Damage_Bleeding");
			playerDamageBones = p.data.ParseEnum<DamagePlayerParameters.Bones>("Player_Damage_Bones");
			playerDamageFood = p.data.ParseFloat("Player_Damage_Food");
			playerDamageWater = p.data.ParseFloat("Player_Damage_Water");
			playerDamageVirus = p.data.ParseFloat("Player_Damage_Virus");
			playerDamageHallucination = p.data.ParseFloat("Player_Damage_Hallucination");
			zombieDamageMultiplier = new ZombieDamageMultiplier(p.data.ParseFloat("Zombie_Damage"), p.data.ParseFloat("Zombie_Leg_Multiplier"), p.data.ParseFloat("Zombie_Arm_Multiplier"), p.data.ParseFloat("Zombie_Spine_Multiplier"), p.data.ParseFloat("Zombie_Skull_Multiplier"));
			animalDamageMultiplier = new AnimalDamageMultiplier(p.data.ParseFloat("Animal_Damage"), p.data.ParseFloat("Animal_Leg_Multiplier"), p.data.ParseFloat("Animal_Spine_Multiplier"), p.data.ParseFloat("Animal_Skull_Multiplier"));
			barricadeDamage = p.data.ParseFloat("Barricade_Damage");
			structureDamage = p.data.ParseFloat("Structure_Damage");
			vehicleDamage = p.data.ParseFloat("Vehicle_Damage");
			resourceDamage = p.data.ParseFloat("Resource_Damage");

			if (p.data.ContainsKey("Object_Damage"))
			{
				objectDamage = p.data.ParseFloat("Object_Damage");
			}
			else
			{
				objectDamage = resourceDamage;
			}

			durability = p.data.ParseFloat("Durability");
			wear = p.data.ParseUInt8("Wear");
			if (wear < 1)
			{
				wear = 1;
			}

			isInvulnerable = p.data.ContainsKey("Invulnerable");

			if (p.data.ContainsKey("Allow_Flesh_Fx"))
			{
				allowFleshFx = p.data.ParseBool("Allow_Flesh_Fx");
			}
			else
			{
				allowFleshFx = true;
			}

			if (p.data.ContainsKey("Stun_Zombie_Always"))
			{
				zombieStunOverride = EZombieStunOverride.Always;
			}
			else if (p.data.ContainsKey("Stun_Zombie_Never"))
			{
				zombieStunOverride = EZombieStunOverride.Never;
			}
			else
			{
				zombieStunOverride = EZombieStunOverride.None;
			}

			ZombieRagdollForceMultiplier = p.data.ParseFloat("Zombie_Ragdoll_Force_Multiplier", 1f);

			bypassAllowedToDamagePlayer = p.data.ParseBool("Bypass_Allowed_To_Damage_Player", defaultValue: false);

			//if(playerDamageMultiplier.damage < 1)
			//{
			//	UnturnedLog.error(itemName + " is missing player damage.");
			//}

			//if(zombieDamageMultiplier.damage < 1)
			//{
			//	UnturnedLog.error(itemName + " is missing zombie damage.");
			//}

			//if(animalDamageMultiplier.damage < 1)
			//{
			//	UnturnedLog.error(itemName + " is missing animal damage.");
			//}
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Weapon
			// Game data for Weapon Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Weapon");
			data.Append("GUID", GUID); // PFK

			data.Append("Durability", durability);
			data.Append("Range", range);
			data.Append("Wear", wear);
			data.Append("Player_Damage", playerDamageMultiplier.damage);
			data.Append("Player_Leg_Multiplier", playerDamageMultiplier.leg);
			data.Append("Player_Arm_Multiplier", playerDamageMultiplier.arm);
			data.Append("Player_Spine_Multiplier", playerDamageMultiplier.spine);
			data.Append("Player_Skull_Multiplier", playerDamageMultiplier.skull);
			data.Append("Player_Damage_Bleeding", playerDamageBleeding);
			data.Append("Player_Damage_Bones", playerDamageBones);
			data.Append("Player_Damage_Food", playerDamageFood);
			data.Append("Player_Damage_Water", playerDamageWater);
			data.Append("Player_Damage_Virus", playerDamageVirus);
			data.Append("Player_Damage_Hallucination", playerDamageHallucination);
			data.Append("Zombie_Damage", zombieDamageMultiplier.damage);
			data.Append("Zombie_Leg_Multiplier", zombieDamageMultiplier.leg);
			data.Append("Zombie_Arm_Multiplier", zombieDamageMultiplier.arm);
			data.Append("Zombie_Spine_Multiplier", zombieDamageMultiplier.spine);
			data.Append("Zombie_Skull_Multiplier", zombieDamageMultiplier.skull);
			data.Append("zombieStunOverride", zombieStunOverride); // Appends final value from mutually-exclusive "Stun_Zombie_Always" and "Stun_Zombie_Never".
			data.Append("Animal_Damage", animalDamageMultiplier.damage);
			data.Append("Animal_Leg_Multiplier", animalDamageMultiplier.leg);
			data.Append("Animal_Spine_Multiplier", animalDamageMultiplier.spine);
			data.Append("Animal_Skull_Multiplier", animalDamageMultiplier.skull);
			data.Append("BladeIDs", bladeIDs.Length); // Get original value of "BladeIDs".
			data.Append("Barricade_Damage", barricadeDamage);
			data.Append("Structure_Damage", structureDamage);
			data.Append("Vehicle_Damage", vehicleDamage);
			data.Append("Resource_Damage", resourceDamage);
			data.Append("Object_Damage", objectDamage);
			data.Append("Invulnerable", isInvulnerable);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Weapon_BladeID
			// Child table for a weapon's BladeIDs.
			for (byte index = 0; index < bladeIDs.Length; index++)
			{
				CargoDeclaration blade = builder.AddDeclaration("Weapon_BladeID");
				blade.Append("GUID", GUID); // FK

				blade.Append("BladeID", (object) bladeIDs[index]); // Appends BladeID_# values.
			}
		}
	}
}
