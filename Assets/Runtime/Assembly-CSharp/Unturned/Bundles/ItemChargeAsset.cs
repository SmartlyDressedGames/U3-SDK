////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemChargeAsset : ItemBarricadeAsset
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
		public float explosionLaunchSpeed;

		private System.Guid _detonationEffectGuid;
		public System.Guid DetonationEffectGuid => _detonationEffectGuid;

		private ushort _explosion2;
		public ushort explosion2
		{
			[System.Obsolete]
			get => _explosion2;
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			int sortOrder = DescSort_ExplosiveChargeDamage;
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
			explosionLaunchSpeed = p.data.ParseFloat("Explosion_Launch_Speed", defaultValue: playerDamage * 0.1f);

			if (p.data.ContainsKey("Object_Damage"))
			{
				objectDamage = p.data.ParseFloat("Object_Damage");
			}
			else
			{
				objectDamage = resourceDamage;
			}

			_explosion2 = p.data.ParseGuidOrLegacyId("Explosion2", out _detonationEffectGuid);
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Charge
			// Game data for Charge Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Charge");
			data.Append("GUID", GUID); // Key

			data.Append("Range2", range2);
			data.Append("Player_Damage", playerDamage);
			data.Append("Zombie_Damage", zombieDamage);
			data.Append("Animal_Damage", animalDamage);
			data.Append("Barricade_Damage", barricadeDamage);
			data.Append("Structure_Damage", structureDamage);
			data.Append("Vehicle_Damage", vehicleDamage);
			data.Append("Resource_Damage", resourceDamage);
			data.Append("Explosion_Launch_Speed", explosionLaunchSpeed);
			data.Append("Object_Damage", objectDamage);
#pragma warning disable
			data.Append("Explosion2", explosion2);
#pragma warning restore
		}
	}
}
