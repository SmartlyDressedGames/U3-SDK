////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ItemCaliberAsset : ItemAsset
	{
		private ushort[] _calibers;
		public ushort[] calibers => _calibers;

		private float _recoil_x;
		public float recoil_x => _recoil_x;

		private float _recoil_y;
		public float recoil_y => _recoil_y;

		/// <summary>
		/// Recoil magnitude multiplier while the gun is aiming down sights.
		/// </summary>
		public float aimingRecoilMultiplier;

		/// <summary>
		/// Multiplier for gun's Aim_In_Duration.
		/// </summary>
		public float aimDurationMultiplier;

		private float _spread;
		public float spread => _spread;

		private float _sway;
		public float sway => _sway;

		private float _shake;
		public float shake => _shake;

		private int _firerateOffset;
		/// <summary>
		/// For backwards compatibility this is *subtracted* from the gun's firerate, so a positive number decreases
		/// the time between shots and a negative number increases the time between shots.
		/// </summary>
		public int FirerateOffset => _firerateOffset;

		protected bool _isPaintable;
		public bool isPaintable => _isPaintable;

		/// <summary>
		/// Multiplier for normal bullet damage.
		/// </summary>
		public float ballisticDamageMultiplier
		{
			get;
			protected set;
		}

		/// <summary>
		/// Multiplier for bullet acceleration due to gravity.
		/// </summary>
		public float BallisticGravityMultiplier
		{
			get;
			protected set;
		}

		/// <summary>
		/// Movement speed multiplier while the gun is aiming down sights.
		/// </summary>
		public float aimingMovementSpeedMultiplier;

		protected bool _isBipod;
		public bool ShouldOnlyAffectAimWhileProne => _isBipod;

		/// <summary>
		/// If true, gun can damage entities with Invulnerable tag. Defaults to false.
		/// </summary>
		public bool CanDamageInvulernableEntities
		{
			get;
			protected set;
		}

		public bool shouldDestroyAttachmentColliders
		{
			get;
			protected set;
		}

		/// <summary>
		/// Name to use when instantiating attachment prefab.
		/// By default the asset guid is used, but it can be overridden because some
		/// modders rely on the name for Unity's legacy animation component. Some maps
		/// had a lot of duplicate animations to work around the guid naming, in which
		/// case overriding the name simplified animation.
		/// </summary>
		public string instantiatedAttachmentName
		{
			get;
			protected set;
		}

		protected override bool doesItemTypeHaveSkins => true;

		/// <summary>
		/// Returns true if calibers list contains provided caliber ID.
		/// </summary>
		public bool CalibersContainId(ushort caliberId)
		{
			foreach (ushort testId in calibers)
			{
				if (testId == caliberId)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns true if calibers list contains any of the provided caliber IDs.
		/// </summary>
		public bool CalibersContainAnyOfIds(ushort[] caliberIds)
		{
			foreach (ushort caliberId in caliberIds)
			{
				if (CalibersContainId(caliberId))
				{
					return true;
				}
			}

			return false;
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (_recoil_x != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_RecoilModifier_X", PlayerDashboardInventoryUI.FormatStatModifier(_recoil_x, false, false)), DescSort_GunAttachmentStat + DescSort_LowerIsBeneficial(_recoil_x));
			}

			if (_recoil_y != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_RecoilModifier_Y", PlayerDashboardInventoryUI.FormatStatModifier(_recoil_y, false, false)), DescSort_GunAttachmentStat + DescSort_LowerIsBeneficial(_recoil_y));
			}

			if (_spread != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_SpreadModifier", PlayerDashboardInventoryUI.FormatStatModifier(_spread, false, false)), DescSort_GunAttachmentStat + DescSort_LowerIsBeneficial(_spread));
			}

			if (_sway != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_SwayModifier", PlayerDashboardInventoryUI.FormatStatModifier(_sway, true, false)), DescSort_GunAttachmentStat + DescSort_LowerIsBeneficial(_sway));
			}

			if (aimingRecoilMultiplier != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_RecoilModifier_Aiming", PlayerDashboardInventoryUI.FormatStatModifier(aimingRecoilMultiplier, false, false)), DescSort_GunAttachmentStat + DescSort_LowerIsBeneficial(aimingRecoilMultiplier));
			}

			if (aimDurationMultiplier != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_AimDurationModifier", PlayerDashboardInventoryUI.FormatStatModifier(aimDurationMultiplier, false, false)), DescSort_GunAttachmentStat + DescSort_LowerIsBeneficial(aimDurationMultiplier));
			}

			if (aimingMovementSpeedMultiplier != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_AimingMovementSpeedModifier", PlayerDashboardInventoryUI.FormatStatModifier(aimingMovementSpeedMultiplier, true, true)), DescSort_GunAttachmentStat + DescSort_HigherIsBeneficial(aimingMovementSpeedMultiplier));
			}

			if (ballisticDamageMultiplier != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_BulletDamageModifier", PlayerDashboardInventoryUI.FormatStatModifier(ballisticDamageMultiplier, true, true)), DescSort_GunAttachmentStat + DescSort_HigherIsBeneficial(ballisticDamageMultiplier));
			}

			if (BallisticGravityMultiplier != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_BulletGravityModifier", PlayerDashboardInventoryUI.FormatStatModifier(BallisticGravityMultiplier, true, false)), DescSort_GunAttachmentStat + DescSort_LowerIsBeneficial(BallisticGravityMultiplier));
			}

			if (CanDamageInvulernableEntities)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_InvulnerableModifier"), DescSort_GunAttachmentStat);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_calibers = new ushort[p.data.ParseUInt8("Calibers")];
			for (byte index = 0; index < calibers.Length; index++)
			{
				_calibers[index] = p.data.ParseUInt16("Caliber_" + index);
			}

			_recoil_x = p.data.ParseFloat("Recoil_X", defaultValue: 1.0f);
			_recoil_y = p.data.ParseFloat("Recoil_Y", defaultValue: 1.0f);
			aimingRecoilMultiplier = p.data.ParseFloat("Aiming_Recoil_Multiplier", defaultValue: 1.0f);
			aimDurationMultiplier = p.data.ParseFloat("Aim_Duration_Multiplier", defaultValue: 1.0f);
			_spread = p.data.ParseFloat("Spread", defaultValue: 1.0f);
			_sway = p.data.ParseFloat("Sway", defaultValue: 1.0f);
			_shake = p.data.ParseFloat("Shake", defaultValue: 1.0f);

			_firerateOffset = p.data.ParseInt32("Firerate");

			// When "Ballistic_Damage_Multiplier" was added to all attachment types the existing barrel-only
			// "Damage" property was accidentally forgotten about, so now it is used as a fallback default value.
			float damage = p.data.ParseFloat("Damage", defaultValue: 1.0f);
			ballisticDamageMultiplier = p.data.ParseFloat("Ballistic_Damage_Multiplier", defaultValue: damage);

			BallisticGravityMultiplier = p.data.ParseFloat("Ballistic_Drop", defaultValue: 1.0f);

			aimingMovementSpeedMultiplier = p.data.ParseFloat("Aiming_Movement_Speed_Multiplier", defaultValue: 1.0f);

			_isPaintable = p.data.ContainsKey("Paintable");
			_isBipod = p.data.ContainsKey("Bipod");
			CanDamageInvulernableEntities = p.data.ParseBool("Invulnerable", defaultValue: false);

			shouldDestroyAttachmentColliders = p.data.ParseBool("Destroy_Attachment_Colliders", defaultValue: true);
			instantiatedAttachmentName = p.data.GetString("Instantiated_Attachment_Name_Override", defaultValue: GUID.ToString("N"));
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Caliber
			// Game data for Caliber Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Caliber");
			data.Append("GUID", GUID); // Key

			data.Append("Recoil_X", recoil_x);
			data.Append("Recoil_Y", recoil_y);
			data.Append("Aiming_Recoil_Multiplier", aimingRecoilMultiplier);
			data.Append("Aim_Duration_Multiplier", aimDurationMultiplier);
			data.Append("Spread", spread);
			data.Append("Sway", sway);
			data.Append("Shake", shake);
			data.Append("Firerate", FirerateOffset);
			data.Append("Ballistic_Damage_Multiplier", ballisticDamageMultiplier);
			data.Append("Ballistic_Drop", BallisticGravityMultiplier);
			data.Append("Aiming_Movement_Speed_Multiplier", aimingMovementSpeedMultiplier);
			data.Append("Paintable", isPaintable);
			data.Append("Bipod", ShouldOnlyAffectAimWhileProne);
			data.Append("Invulnerable", CanDamageInvulernableEntities);

			data.Append("Calibers", calibers.Length); // Get original value of "Calibers".

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Caliber_Caliber
			// Child table for Caliber_# values.
			for (byte index = 0; index < calibers.Length; index++)
			{
				CargoDeclaration cal = builder.AddDeclaration("Caliber_Caliber");
				cal.Append("GUID", GUID); // FK
				cal.Append("Caliber", calibers[index]);
			}
		}

		protected override AudioReference GetDefaultInventoryAudio()
		{
			return new AudioReference("core.masterbundle", "Sounds/Inventory/SmallGunAttachment.asset");
		}

		[System.Obsolete("Changed type to int")]
		public byte firerate => MathfEx.ClampToByte(_firerateOffset);
	}
}
