////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class ItemMagazineAsset : ItemCaliberAsset
	{
		protected GameObject _magazine;
		public GameObject magazine => _magazine;

		/// <summary>
		/// If set and gun is a Projectile launcher, overrides Projectile prefab.
		/// </summary>
		public GameObject ProjectilePrefabOverride
		{
			get;
			set;
		}

		private byte _pellets;
		public byte pellets => _pellets;

		private byte _stuck;
		public byte stuck => _stuck;

		protected float _range;
		public float range => _range;

		/// <summary>
		/// Multiplier for explosive projectile damage.
		/// </summary>
		public float projectileDamageMultiplier
		{
			get;
			protected set;
		}

		/// <summary>
		/// Multiplier for explosive projectile's blast radius.
		/// </summary>
		public float projectileBlastRadiusMultiplier
		{
			get;
			protected set;
		}

		/// <summary>
		/// Multiplier for explosive projectile's initial force.
		/// </summary>
		public float projectileLaunchForceMultiplier
		{
			get;
			protected set;
		}

		/// <summary>
		/// Only applicable if Projectile prefab override is used.
		/// Seconds before physics projectile is destroyed.
		/// </summary>
		public float ProjectileLifespanOverride
		{
			get;
			set;
		}

		public float playerDamage;
		public float zombieDamage;
		public float animalDamage;
		public float barricadeDamage;
		public float structureDamage;
		public float vehicleDamage;
		public float resourceDamage;
		public float objectDamage;
		public float explosionLaunchSpeed;

		/// <summary>
		/// If true, per-surface effects like blood splatter are created.
		/// Defaults to true, but can be disabled particularly if a performance issue (e.g., an
		/// explosive many-pellet shotgun shell).
		/// </summary>
		public bool ExplosionPlaysImpactEffects
		{
			get;
			set;
		} = true;

		public bool ExplosionPenetratesBuildables
		{
			get;
			set;
		} = false;

		public System.Guid explosionEffectGuid;
		private ushort _explosion;
		public ushort explosion => _explosion;

		public bool IsExplosionEffectRefNull()
		{
			return explosion == 0 && explosionEffectGuid.IsEmpty();
		}

		public EffectAsset FindExplosionEffect()
		{
			return Assets.FindEffectAssetByGuidOrLegacyId(explosionEffectGuid, explosion);
		}

		public bool spawnExplosionOnDedicatedServer
		{
			get;
			protected set;
		}

		public System.Guid tracerEffectGuid;
		private ushort _tracer;
		public ushort tracer
		{
			[System.Obsolete]
			get => _tracer;
		}

		public EffectAsset FindTracerEffectAsset()
		{
#pragma warning disable
			return Assets.FindEffectAssetByGuidOrLegacyId(tracerEffectGuid, _tracer);
#pragma warning restore
		}

		private System.Guid _impactEffectGuid;
		public System.Guid ImpactEffectGuid => _impactEffectGuid;

		private ushort _impact;
		public ushort impact
		{
			[System.Obsolete]
			get => _impact;
		}

		public bool IsImpactEffectRefNull()
		{
			return _impact == 0 && _impactEffectGuid.IsEmpty();
		}

		public EffectAsset FindImpactEffectAsset()
		{
			return Assets.FindEffectAssetByGuidOrLegacyId(_impactEffectGuid, _impact);
		}

		public override bool showQuality => stuck > 0;

		private float _speed;
		public float speed => _speed;

		protected bool _isExplosive;
		public bool isExplosive => _isExplosive;

		[System.Obsolete("Moved into ItemAsset and renamed to ShouldDeleteAtZeroAmount.")]
		public bool deleteEmpty => ShouldDeleteAtZeroAmount;

		/// <summary>
		/// Should amount be filled to capacity when detached?
		/// </summary>
		public bool shouldFillAfterDetach
		{
			get;
			protected set;
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (_pellets > 1)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_PelletCount", _pellets), DescSort_GunAttachmentStat);
			}

			if (isExplosive)
			{
				int sortOrder = DescSort_ExplosiveBulletDamage;
				builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosiveBullet"), true), sortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBlastRadius", MeasurementTool.FormatLengthString(range)), sortOrder++);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionPlayerDamage", Mathf.RoundToInt(playerDamage)), sortOrder);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionZombieDamage", Mathf.RoundToInt(zombieDamage)), sortOrder);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionAnimalDamage", Mathf.RoundToInt(animalDamage)), sortOrder);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBarricadeDamage", Mathf.RoundToInt(barricadeDamage)), sortOrder);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionStructureDamage", Mathf.RoundToInt(structureDamage)), sortOrder);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionVehicleDamage", Mathf.RoundToInt(vehicleDamage)), sortOrder);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionResourceDamage", Mathf.RoundToInt(resourceDamage)), sortOrder);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionObjectDamage", Mathf.RoundToInt(objectDamage)), sortOrder);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_magazine = loadRequiredAsset<GameObject>(p.bundle, "Magazine");
			ProjectilePrefabOverride = p.bundle.load<GameObject>("Projectile");

			_pellets = p.data.ParseUInt8("Pellets");
			if (pellets < 1)
			{
				_pellets = 1;
			}

			_stuck = p.data.ParseUInt8("Stuck");

			projectileDamageMultiplier = p.data.ParseFloat("Projectile_Damage_Multiplier", defaultValue: 1.0f);
			projectileBlastRadiusMultiplier = p.data.ParseFloat("Projectile_Blast_Radius_Multiplier", defaultValue: 1.0f);
			projectileLaunchForceMultiplier = p.data.ParseFloat("Projectile_Launch_Force_Multiplier", defaultValue: 1.0f);

			_range = p.data.ParseFloat("Range");
			playerDamage = p.data.ParseFloat("Player_Damage");
			zombieDamage = p.data.ParseFloat("Zombie_Damage");
			animalDamage = p.data.ParseFloat("Animal_Damage");
			barricadeDamage = p.data.ParseFloat("Barricade_Damage");
			structureDamage = p.data.ParseFloat("Structure_Damage");
			vehicleDamage = p.data.ParseFloat("Vehicle_Damage");
			resourceDamage = p.data.ParseFloat("Resource_Damage");
			explosionLaunchSpeed = p.data.ParseFloat("Explosion_Launch_Speed", defaultValue: playerDamage * 0.1f);
			ExplosionPlaysImpactEffects = p.data.ParseBool("Explosion_Plays_Impact_Effects", true);
			ExplosionPenetratesBuildables = p.data.ParseBool("Explosion_Penetrate_Buildables");
			_explosion = p.data.ParseGuidOrLegacyId("Explosion", out explosionEffectGuid);

			if (p.data.ContainsKey("Object_Damage"))
			{
				objectDamage = p.data.ParseFloat("Object_Damage");
			}
			else
			{
				objectDamage = resourceDamage;
			}

			_tracer = p.data.ParseGuidOrLegacyId("Tracer", out tracerEffectGuid);
			_impact = p.data.ParseGuidOrLegacyId("Impact", out _impactEffectGuid);

			_speed = p.data.ParseFloat("Speed");
			if (speed < 0.01f)
			{
				_speed = 1.0f;
			}

			_isExplosive = p.data.ContainsKey("Explosive");
			spawnExplosionOnDedicatedServer = p.data.ContainsKey("Spawn_Explosion_On_Dedicated_Server");

			shouldFillAfterDetach = p.data.ParseBool("Should_Fill_After_Detach", defaultValue: false);
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Magazine
			// Game data for Magazine Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Magazine");
			data.Append("GUID", GUID); // Key

			data.Append("Pellets", pellets);
			data.Append("Stuck", stuck);
			data.Append("Projectile_Damage_Multiplier", projectileDamageMultiplier);
			data.Append("Projectile_Blast_Radius_Multiplier", projectileBlastRadiusMultiplier);
			data.Append("Projectile_Launch_Force_Multiplier", projectileLaunchForceMultiplier);
			data.Append("Range", range);
			data.Append("Player_Damage", playerDamage);
			data.Append("Zombie_Damage", zombieDamage);
			data.Append("Animal_Damage", animalDamage);
			data.Append("Barricade_Damage", barricadeDamage);
			data.Append("Structure_Damage", structureDamage);
			data.Append("Vehicle_Damage", vehicleDamage);
			data.Append("Resource_Damage", resourceDamage);
			data.Append("Explosion_Launch_Speed", explosionLaunchSpeed);
			data.Append("Explosion", explosion);
			data.Append("Object_Damage", objectDamage);
#pragma warning disable
			data.Append("Tracer", tracer);
			data.Append("Impact", impact);
#pragma warning restore
			data.Append("Speed", speed);
			data.Append("Explosive", isExplosive);
#pragma warning disable
			data.Append("Delete_Empty", deleteEmpty);
#pragma warning restore
			data.Append("Should_Fill_After_Detach", shouldFillAfterDetach);
		}
	}
}
