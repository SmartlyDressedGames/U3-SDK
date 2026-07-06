////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemTrapAsset : ItemBarricadeAsset
	{
		protected float _range2;
		public float range2 => _range2;

		public float playerDamage;
		public float zombieDamage;
		public float animalDamage;
		public float barricadeDamage;
		public float structureDamage;
		public float vehicleDamage;
		public float resourceDamage;
		public float objectDamage;

		/// <summary>
		/// Seconds after placement before damage can be dealt.
		/// </summary>
		public float trapSetupDelay;

		/// <summary>
		/// Seconds interval between damage dealt.
		/// i.e., will not cause damage if less than this amount of time passed since the last damage.
		/// </summary>
		public float trapCooldown;

		public float explosionLaunchSpeed;

		public System.Guid trapDetonationEffectGuid;
		private ushort _explosion2;
		public ushort explosion2 => _explosion2;

		protected bool _isBroken;
		public bool isBroken => _isBroken;

		protected bool _isExplosive;
		public bool isExplosive => _isExplosive;

		public bool damageTires;
		public bool requiresPower;

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (isExplosive)
			{
				int sortOrder = DescSort_ExplosiveTrapDamage;
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBlastRadius", MeasurementTool.FormatLengthString(range2)), sortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionPlayerDamage", Mathf.RoundToInt(playerDamage)), sortOrder);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionZombieDamage", Mathf.RoundToInt(zombieDamage)), sortOrder);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionAnimalDamage", Mathf.RoundToInt(animalDamage)), sortOrder);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBarricadeDamage", Mathf.RoundToInt(barricadeDamage)), sortOrder);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionStructureDamage", Mathf.RoundToInt(structureDamage)), sortOrder);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionVehicleDamage", Mathf.RoundToInt(vehicleDamage)), sortOrder);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionResourceDamage", Mathf.RoundToInt(resourceDamage)), sortOrder);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionObjectDamage", Mathf.RoundToInt(objectDamage)), sortOrder);
			}
			else
			{
				if (isBroken)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Trap_BreaksBones"), DescSort_TrapKeyword);
				}

				if (damageTires)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Trap_DamagesTires"), DescSort_TrapKeyword);
				}

				if (requiresPower)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Trap_RequiresPower"), DescSort_TrapKeyword);
				}

				if (playerDamage > 0.0f)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Trap_PlayerDamage", Mathf.RoundToInt(playerDamage)), DescSort_TrapStat);
				}

				if (zombieDamage > 0.0f)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Trap_ZombieDamage", Mathf.RoundToInt(zombieDamage)), DescSort_TrapStat);
				}

				if (animalDamage > 0.0f)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Trap_AnimalDamage", Mathf.RoundToInt(animalDamage)), DescSort_TrapStat);
				}
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_range2 = p.data.ParseFloat("Range2");
			playerDamage = p.data.ParseFloat("Player_Damage");
			zombieDamage = p.data.ParseFloat("Zombie_Damage");
			animalDamage = p.data.ParseFloat("Animal_Damage");
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

			trapSetupDelay = p.data.ParseFloat("Trap_Setup_Delay", defaultValue: 0.25f);
			trapCooldown = p.data.ParseFloat("Trap_Cooldown");
			_explosion2 = p.data.ParseGuidOrLegacyId("Explosion2", out trapDetonationEffectGuid);
			explosionLaunchSpeed = p.data.ParseFloat("Explosion_Launch_Speed", defaultValue: playerDamage * 0.1f);

			_isBroken = p.data.ContainsKey("Broken");
			_isExplosive = p.data.ContainsKey("Explosive");
			damageTires = p.data.ContainsKey("Damage_Tires");
			requiresPower = p.data.ParseBool("Requires_Power");
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Trap
			// Game data for Trap Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Trap");
			data.Append("GUID", GUID); // Key

			data.Append("Range2", range2);
			data.Append("Player_Damage", playerDamage);
			data.Append("Zombie_Damage", zombieDamage);
			data.Append("Animal_Damage", animalDamage);
			data.Append("Barricade_Damage", barricadeDamage);
			data.Append("Structure_Damage", structureDamage);
			data.Append("Vehicle_Damage", vehicleDamage);
			data.Append("Resource_Damage", resourceDamage);
			data.Append("Object_Damage", objectDamage);
			data.Append("Trap_Setup_Delay", trapSetupDelay);
			data.Append("Trap_Cooldown", trapCooldown);
			data.Append("Explosion2", explosion2);
			data.Append("Explosion_Launch_Speed", explosionLaunchSpeed);
			data.Append("Broken", isBroken);
			data.Append("Explosive", isExplosive);
			data.Append("Damage_Tires", damageTires);
			data.Append("Requires_Power", requiresPower);
		}
	}
}
