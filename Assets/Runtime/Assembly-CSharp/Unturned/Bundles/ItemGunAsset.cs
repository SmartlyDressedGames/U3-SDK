////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public struct MagazineReplacement
	{
		public string map;
		public ushort legacyId;
		public System.Guid guid;
	}

	/// <summary>
	/// Controls how first-person arms are moved for turrets operated from the driver's seat.
	/// </summary>
	internal enum EDriverTurretViewmodelMode
	{
		/// <summary>
		/// Default. Pushes first-person arms off-screen while aiming. Originally implemented for the Fighter Jet where
		/// it looks weird if your arms are still visible when the camera zooms in while "aiming."
		/// </summary>
		OffscreenWhileAiming,

		/// <summary>
		/// Push first-person arms off-screen when equipped.
		/// </summary>
		AlwaysOffscreen,

		/// <summary>
		/// No particular use in mind, but included for completeness.
		/// </summary>
		AlwaysOnscreen,
	}

	public enum ERechamberGunAfterReloadMode
	{
		/// <summary>
		/// Default. Plays "Hammer" animation if ammo count was zero.
		/// </summary>
		IfAmmoWasEmpty,

		/// <summary>
		/// Regardless of ammo, does not play "Hammer" animation after reloading.
		/// </summary>
		Never,

		/// <summary>
		/// Regardless of ammo, will play "Hammer" animation after reloading.
		/// </summary>
		Always,
	}

	public class ItemGunAsset : ItemWeaponAsset
	{
		protected AudioClip _shoot;
		public AudioClip shoot => _shoot;

		protected AudioClip _reload;
		public AudioClip reload => _reload;

		protected AudioClip _hammer;
		public AudioClip hammer => _hammer;

		protected AudioClip _aim;
		public AudioClip aim => _aim;

		protected AudioClip _minigun;
		public AudioClip minigun => _minigun;

		protected AudioClip _chamberJammedSound;
		public AudioClip chamberJammedSound => _chamberJammedSound;

		/// <summary>
		/// Sound to play when input is pressed but weapon has a fire delay.
		/// </summary>
		public AudioClip fireDelaySound
		{
			get;
			protected set;
		}

		/// <summary>
		/// Maximum distance the gunshot can be heard.
		/// </summary>
		public float gunshotRolloffDistance
		{
			get;
			protected set;
		}

		protected GameObject _projectile;
		public GameObject projectile => _projectile;

		public override bool shouldFriendlySentryTargetUser => true;


		public float alertRadius;

		/// <summary>
		/// Override Rangefinder attachment's maximum range.
		/// Defaults to range value.
		/// </summary>
		public float rangeRangefinder
		{
			get;
			protected set;
		}

		/// <summary>
		/// Can this weapon instantly kill players by headshots?
		/// Only valid when game config also enables this.
		/// </summary>
		public bool instakillHeadshots
		{
			get;
			protected set;
		}

		/// <summary>
		/// Can this weapon be fired without consuming ammo?
		/// Some mods use this for turrets.
		/// </summary>
		public bool infiniteAmmo
		{
			get;
			protected set;
		}

		/// <summary>
		/// Ammo quantity to consume per shot fired.
		/// </summary>
		public byte ammoPerShot
		{
			get;
			protected set;
		}

		/// <summary>
		/// Simulation steps to wait after input before firing.
		/// </summary>
		public int fireDelay
		{
			get;
			protected set;
		}

		/// <summary>
		/// Can magazine be changed by player?
		/// </summary>
		public bool allowMagazineChange
		{
			get;
			protected set;
		}

		/// <summary>
		/// Can player ADS while sprinting and vice versa?
		/// </summary>
		public bool canAimDuringSprint
		{
			get;
			protected set;
		}

		/// <summary>
		/// If true, the gun cannot shoot unless the player is aiming.
		/// Note: String action overrides this.
		/// Defaults to true for miniguns.
		/// </summary>
		public bool MustAimToShoot
		{
			get;
			protected set;
		}

		/// <summary>
		/// If true, the gun will stop aiming regardless of player input.
		/// </summary>
		public bool ShouldForceStopAimingAfterShooting
		{
			get;
			set;
		}

		/// <summary>
		/// Seconds from pressing "aim" to fully aiming down sights.
		/// </summary>
		public float aimInDuration
		{
			get;
			protected set;
		}

		/// <summary>
		/// If true, Aim_Start and Aim_Stop animations are scaled according to actual aim duration.
		/// </summary>
		public bool shouldScaleAimAnimations
		{
			get;
			protected set;
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (itemInstance != null)
			{
				ushort instanceMagazineId = System.BitConverter.ToUInt16(itemInstance.state, 8);
				ItemMagazineAsset magazineAsset = Assets.find(EAssetType.ITEM, instanceMagazineId) as ItemMagazineAsset;

				if (magazineAsset != null)
				{
					// Nelson 2024-06-11: Please refer to public issue #4494.
					if (!string.IsNullOrEmpty(magazineAsset.itemName))
					{
						builder.Append(PlayerDashboardInventoryUI.localization.format("Ammo", "<color=" + Palette.hex(ItemTool.getRarityColorUI(magazineAsset.rarity)) + ">" + magazineAsset.itemName + "</color>", itemInstance.state[10], magazineAsset.MaxAmount), DescSort_Important);
					}
					else
					{
						builder.Append(PlayerDashboardInventoryUI.localization.format("Ammo", "<color=" + Palette.hex(ItemTool.getRarityColorUI(rarity)) + ">" + itemName + "</color>", itemInstance.state[10], magazineAsset.MaxAmount), DescSort_Important);
					}
				}
				else
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("Ammo", PlayerDashboardInventoryUI.localization.format("None"), 0, 0), DescSort_Important);
				}
			}

			if (itemInstance != null && builder.HasFlag(EItemDescriptionFlags.GunAttachments))
			{
				ushort instanceSightId = System.BitConverter.ToUInt16(itemInstance.state, 0);
				ushort instanceTacticalId = System.BitConverter.ToUInt16(itemInstance.state, 2);
				ushort instanceGripId = System.BitConverter.ToUInt16(itemInstance.state, 4);
				ushort instanceBarrelId = System.BitConverter.ToUInt16(itemInstance.state, 6);

				ItemSightAsset sightAsset = Assets.find(EAssetType.ITEM, instanceSightId) as ItemSightAsset;
				ItemTacticalAsset tacticalAsset = Assets.find(EAssetType.ITEM, instanceTacticalId) as ItemTacticalAsset;
				ItemGripAsset gripAsset = Assets.find(EAssetType.ITEM, instanceGripId) as ItemGripAsset;
				ItemBarrelAsset barrelAsset = Assets.find(EAssetType.ITEM, instanceBarrelId) as ItemBarrelAsset;

				if (sightAsset != null && (hasSight || instanceSightId != sightID))
				{
					// Nelson 2024-06-11: Please refer to public issue #4494.
					if (!string.IsNullOrEmpty(sightAsset.itemName))
					{
						builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_SightAttachment", "<color=" + Palette.hex(ItemTool.getRarityColorUI(sightAsset.rarity)) + ">" + sightAsset.itemName + "</color>"), DescSort_Important);
					}
				}
				else if (hasSight)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_SightAttachment", PlayerDashboardInventoryUI.localization.format("None")), DescSort_Important);
				}

				if (tacticalAsset != null && (hasTactical || instanceTacticalId != tacticalID))
				{
					// Nelson 2024-06-11: Please refer to public issue #4494.
					if (!string.IsNullOrEmpty(tacticalAsset.itemName))
					{
						builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_TacticalAttachment", "<color=" + Palette.hex(ItemTool.getRarityColorUI(tacticalAsset.rarity)) + ">" + tacticalAsset.itemName + "</color>"), DescSort_Important);
					}
				}
				else if (hasTactical)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_TacticalAttachment", PlayerDashboardInventoryUI.localization.format("None")), DescSort_Important);
				}

				if (gripAsset != null && (hasGrip || instanceGripId != gripID))
				{
					// Nelson 2024-06-11: Please refer to public issue #4494.
					if (!string.IsNullOrEmpty(gripAsset.itemName))
					{
						builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_GripAttachment", "<color=" + Palette.hex(ItemTool.getRarityColorUI(gripAsset.rarity)) + ">" + gripAsset.itemName + "</color>"), DescSort_Important);
					}
				}
				else if (hasGrip)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_GripAttachment", PlayerDashboardInventoryUI.localization.format("None")), DescSort_Important);
				}

				if (barrelAsset != null && (hasBarrel || instanceBarrelId != barrelID))
				{
					// Nelson 2024-06-11: Please refer to public issue #4494.
					if (!string.IsNullOrEmpty(barrelAsset.itemName))
					{
						builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_BarrelAttachment", "<color=" + Palette.hex(ItemTool.getRarityColorUI(barrelAsset.rarity)) + ">" + barrelAsset.itemName + "</color>"), DescSort_Important);
					}
				}
				else if (hasBarrel)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_BarrelAttachment", PlayerDashboardInventoryUI.localization.format("None")), DescSort_Important);
				}
			}

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			float roundsPerSecond = CalculateRoundsPerSecond();
			float roundsPerMinute = roundsPerSecond * 60.0f;
			builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Firerate", Mathf.RoundToInt(roundsPerMinute)), DescSort_GunStat);

			builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Spread", $"{Mathf.Rad2Deg * baseSpreadAngleRadians:N1}"), DescSort_GunStat);

			if (spreadAim != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Spread_Aim", $"{Mathf.Rad2Deg * baseSpreadAngleRadians * spreadAim:N1}"), DescSort_GunStat);
			}

			if (aimingRecoilMultiplier != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_RecoilModifier_Aiming", PlayerDashboardInventoryUI.FormatStatModifier(aimingRecoilMultiplier, false, false)), DescSort_GunStat + DescSort_LowerIsBeneficial(aimingRecoilMultiplier));
			}

			if (damageFalloffRange != 1.0f && damageFalloffMultiplier != 1.0f)
			{
				string start = MeasurementTool.FormatLengthString(range * damageFalloffRange);
				string end = MeasurementTool.FormatLengthString(range * damageFalloffMaxRange);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_DamageFalloff", start, end, $"{damageFalloffMultiplier:P}"), DescSort_GunStat);
			}

			if (_projectile != null)
			{
				BuildExplosiveDescription(builder, itemInstance);
			}
			else
			{
				BuildNonExplosiveDescription(builder, itemInstance);
			}
		}

		public override byte[] getState(EItemOrigin origin)
		{
			byte[] magazineState = getMagazineState(GetDefaultMagazineLegacyId());

			return new byte[18]
			{
				sightState[0],
				sightState[1],
				tacticalState[0],
				tacticalState[1],
				gripState[0],
				gripState[1],
				barrelState[0],
				barrelState[1],
				magazineState[0],
				magazineState[1],
				origin != EItemOrigin.WORLD || Random.value < (Provider.modeConfigData != null ? Provider.modeConfigData.Items.Gun_Bullets_Full_Chance : 0.9f) ? ammoMax : (byte) Mathf.CeilToInt(Random.Range(ammoMin, ammoMax + 1) * (Provider.modeConfigData != null ? Provider.modeConfigData.Items.Gun_Bullets_Multiplier : 1.0f)), // ammo
				(byte) firemode, // firemode
				1, // interact state
				100, // sight quality
				100, // tactical quality
				100, // grip quality
				100, // barrel quality
				100 // magazine quality
			};
		}

		public byte[] getState(ushort sight, ushort tactical, ushort grip, ushort barrel, ushort magazine, byte ammo)
		{
			byte[] sightBytes = System.BitConverter.GetBytes(sight);
			byte[] tacticalBytes = System.BitConverter.GetBytes(tactical);
			byte[] gripBytes = System.BitConverter.GetBytes(grip);
			byte[] barrelBytes = System.BitConverter.GetBytes(barrel);
			byte[] magazineBytes = System.BitConverter.GetBytes(magazine);

			return new byte[18]
			{
				sightBytes[0],
				sightBytes[1],
				tacticalBytes[0],
				tacticalBytes[1],
				gripBytes[0],
				gripBytes[1],
				barrelBytes[0],
				barrelBytes[1],
				magazineBytes[0],
				magazineBytes[1],
				ammo, // ammo
				(byte) firemode, // firemode
				1, // interact state
				100, // sight quality
				100, // tactical quality
				100, // grip quality
				100, // barrel quality
				100 // magazine quality
			};
		}


		public byte ammoMin;


		public byte ammoMax;

		private ushort _sightID;

		public ushort sightID
		{
			get => _sightID;
			set
			{
				_sightID = value;
				sightState = System.BitConverter.GetBytes(sightID);
			}
		}

		private byte[] sightState;

		private ushort _tacticalID;

		public ushort tacticalID
		{
			get => _tacticalID;
			set
			{
				_tacticalID = value;
				tacticalState = System.BitConverter.GetBytes(tacticalID);
			}
		}

		private byte[] tacticalState;

		private ushort _gripID;

		public ushort gripID
		{
			get => _gripID;
			set
			{
				_gripID = value;
				gripState = System.BitConverter.GetBytes(gripID);
			}
		}

		private byte[] gripState;

		private ushort _barrelID;

		public ushort barrelID
		{
			get => _barrelID;
			set
			{
				_barrelID = value;
				barrelState = System.BitConverter.GetBytes(barrelID);
			}
		}

		private byte[] barrelState;

		//private ushort _magazineID;
		//
		//public ushort magazineID
		//{
		//	get { return _magazineID; }
		//	set
		//	{
		//		_magazineID = value;
		//		magazineState = System.BitConverter.GetBytes(magazineID);
		//	}
		//}

		//private byte[] magazineState;

		private ushort defaultMagazineLegacyId;
		private System.Guid defaultMagazineGuid;
		private MagazineReplacement[] magazineReplacements;

		/// <summary>
		/// Selects a default magazine, following magazine replacements and spawn table resolution.
		/// </summary>
		public ushort GetDefaultMagazineLegacyId()
		{
			return SelectDefaultMagazine()?.id ?? 0;
		}

		/// <summary>
		/// Selects a default magazine, following magazine replacements and spawn table resolution.
		/// </summary>
		public ItemMagazineAsset SelectDefaultMagazine()
		{
			bool replaced = false;
			Asset asset = null;

			if (Level.info != null && magazineReplacements != null)
			{
				foreach (MagazineReplacement magazineReplacement in magazineReplacements)
				{
					if (magazineReplacement.map == Level.info.name)
					{
						asset = Assets.FindByGuidOrLegacyId(magazineReplacement.guid, EAssetType.ITEM, magazineReplacement.legacyId);
						replaced = true;
						break;
					}
				}
			}

			if (!replaced)
			{
				asset = Assets.FindByGuidOrLegacyId(defaultMagazineGuid, EAssetType.ITEM, defaultMagazineLegacyId);
			}

			if (asset is SpawnAsset spawnAsset)
			{
				asset = SpawnTableTool.Resolve(spawnAsset, EAssetType.ITEM, OnGetDefaultMagazineSpawnTableErrorContext);
			}

			return asset as ItemMagazineAsset;
		}

		private string OnGetDefaultMagazineSpawnTableErrorContext()
		{
			return $"{GUID:N} default magazine";
		}

		private byte[] getMagazineState(ushort id)
		{
			return System.BitConverter.GetBytes(id);
		}

		internal float CalculateRoundsPerSecond()
		{
			return 50.0f / Mathf.Max(1, firerate + 1);
		}

		public float unplace;
		public float replace;

		/// <summary>
		/// How long in seconds after firing to rechamber the gun by playing the Hammer animation.
		/// Only applicable if RechamberAfterShotCount is >0.
		/// Defaults to 0.25 seconds.
		/// </summary>
		public float RechamberAfterShotDelay
		{
			get;
			set;
		} = 0.25f;

		/// <summary>
		/// How long in seconds after hammering to eject a bullet casing.
		/// Defaults to 0.45 seconds.
		/// </summary>
		public float EjectAfterHammerDelay
		{
			get;
			set;
		} = 0.45f;

		/// <summary>
		/// How long in seconds after reloading to eject bullet casings.
		/// Only applicable if CasingEjectCountAfterReload is greater than zero.
		/// Defaults to 0.5 seconds.
		/// </summary>
		public float EjectAfterReloadDelay
		{
			get;
			set;
		} = 0.5f;

		public bool hasSight;


		public bool hasTactical;


		public bool hasGrip;


		public bool hasBarrel;


		public ushort[] attachmentCalibers
		{
			get;
			private set;
		}


		public ushort[] magazineCalibers
		{
			get;
			private set;
		}


		public byte firerate;


		public EAction action;


		public bool shouldDeleteEmptyMagazines;

		/// <summary>
		/// Defaults to false. If true, attachments must specify at least one non-zero caliber.
		/// Useful for blocking vanilla attachments.
		/// </summary>
		public bool requiresNonZeroAttachmentCaliber;


		public bool hasSafety;


		public bool hasSemi;


		public bool hasAuto;


		public bool hasBurst;


		public bool isTurret;

		internal EDriverTurretViewmodelMode driverTurretViewmodelMode;

		/// <summary>
		/// Determines whether "Hammer" animation plays after attaching a magazine.
		/// Note: this happens when a magazine replaces another OR fills previously empty slot.
		/// </summary>
		public ERechamberGunAfterReloadMode RechamberAfterMagazineAttached
		{
			get;
			set;
		}

		/// <summary>
		/// Determines whether "Hammer" animation plays after detached a magazine.
		/// Note: this happens when a magazine is removed from the gun without a replacement.
		/// </summary>
		public ERechamberGunAfterReloadMode RechamberAfterMagazineDetached
		{
			get;
			set;
		}

		public int bursts;

		internal EFiremode firemode;


		public float spreadAim;

		[System.Obsolete("Replaced by baseSpreadAngleRadians")]
		public float spreadHip;

		public float baseSpreadAngleRadians
		{
			get;
			private set;
		}

		/// <summary>
		/// Spread multiplier while sprinting.
		/// </summary>
		public float spreadSprint;

		/// <summary>
		/// Spread multiplier while crouched.
		/// </summary>
		public float spreadCrouch;

		/// <summary>
		/// Spread multiplier while prone.
		/// </summary>
		public float spreadProne;

		/// <summary>
		/// Spread multiplier while swimming.
		/// </summary>
		public float spreadSwimming;

		/// <summary>
		/// Spread multiplier while not grounded.
		/// </summary>
		public float spreadMidair;

		public float recoilMin_x;


		public float recoilMin_y;


		public float recoilMax_x;


		public float recoilMax_y;

		/// <summary>
		/// Recoil magnitude multiplier while the gun is aiming down sights.
		/// </summary>
		public float aimingRecoilMultiplier;

		/// <summary>
		/// Recoil magnitude while sprinting.
		/// </summary>
		public float recoilSprint;

		/// <summary>
		/// Recoil magnitude while crouched.
		/// </summary>
		public float recoilCrouch;

		/// <summary>
		/// Recoil magnitude while prone.
		/// </summary>
		public float recoilProne;

		/// <summary>
		/// Recoil magnitude while swimming.
		/// </summary>
		public float recoilSwimming;

		/// <summary>
		/// Recoil magnitude while not grounded.
		/// </summary>
		public float recoilMidair;

		public float recover_x;


		public float recover_y;


		public float shakeMin_x;


		public float shakeMin_y;


		public float shakeMin_z;


		public float shakeMax_x;


		public float shakeMax_y;


		public float shakeMax_z;


		public byte ballisticSteps;


		public float ballisticTravel;
		public float muzzleVelocity
		{
			get;
			protected set;
		}

		public float bulletGravityMultiplier
		{
			get;
			protected set;
		}

		public float ballisticForce;

		/// <summary>
		/// [0, 1] percentage of maximum range where damage begins decreasing toward falloff multiplier.
		/// </summary>
		public float damageFalloffRange;

		/// <summary>
		/// [0, 1] percentage of maximum range where damage finishes decreasing toward falloff multiplier.
		/// </summary>
		public float damageFalloffMaxRange;

		/// <summary>
		/// [0, 1] percentage of damage to apply at damageFalloffMaxRange.
		/// </summary>
		public float damageFalloffMultiplier;

		/// <summary>
		/// Seconds before physics projectile is destroyed.
		/// </summary>
		public float projectileLifespan;


		public bool projectilePenetrateBuildables;

		public float projectileExplosionLaunchSpeed;


		public float reloadTime;


		public float hammerTime;

		public System.Guid muzzleGuid;
		[System.Obsolete]
		public ushort muzzle;

		public EffectAsset FindMuzzleEffectAsset()
		{
#pragma warning disable
			return Assets.FindEffectAssetByGuidOrLegacyId(muzzleGuid, muzzle);
#pragma warning restore
		}

		public System.Guid shellGuid;
		[System.Obsolete]
		public ushort shell;

		public EffectAsset FindShellEffectAsset()
		{
#pragma warning disable
			return Assets.FindEffectAssetByGuidOrLegacyId(shellGuid, shell);
#pragma warning restore
		}

		public System.Guid projectileExplosionEffectGuid;
		public ushort explosion;

		public override bool showQuality => true;

		/// <summary>
		/// Is this gun setup to have a change of jamming?
		/// </summary>
		public bool canEverJam
		{
			get;
			protected set;
		}

		/// <summary>
		/// [0, 1] quality percentage that jamming will start happening.
		/// </summary>
		public float jamQualityThreshold
		{
			get;
			protected set;
		}

		/// <summary>
		/// [0, 1] percentage of the time that shots will jam the gun when at 0% quality.
		/// Chance of jamming is blended between 0% at jamQualityThreshold and jamMaxChance% at 0% quality.
		/// </summary>
		public float jamMaxChance
		{
			get;
			protected set;
		}

		/// <summary>
		/// Name of the animation to play when unjamming chamber.
		/// </summary>
		public string unjamChamberAnimName
		{
			get;
			protected set;
		}

		/// <summary>
		/// Movement speed multiplier while the gun is aiming down sights.
		/// </summary>
		public float aimingMovementSpeedMultiplier;

		/// <summary>
		/// If >0, hammer animation plays after shooting this many shots after RechamberAfterShotDelay seconds pass.
		/// Defaults to one for EAction.Pump and EAction.Bolt, zero otherwise.
		/// </summary>
		public int RechamberAfterShotCount
		{
			get;
			set;
		}

		/// <summary>
		/// If >0, emit particles after hammer after EjectAfterHammerDelay seconds pass.
		/// Only applicable if RechamberAfterShotCount is >0.
		/// Defaults to 1.
		/// </summary>
		public int CasingEjectCountAfterRechamberingAfterShooting
		{
			get;
			set;
		}

		/// <summary>
		/// If >0, emit particles after reloading after EjectAfterReloadDelay seconds pass.
		/// Defaults to ammoMax for EAction.Break.
		/// </summary>
		public int CasingEjectCountAfterReload
		{
			get;
			set;
		}

		/// <summary>
		/// If true, emit particles when a shot is fired.
		/// Defaults to true for EAction.Trigger and EAction.Minigun.
		/// </summary>
		public bool ShouldEjectCasingAfterShooting
		{
			get;
			set;
		}

		protected NPCRewardsList shootQuestRewards;

		public void GrantShootQuestRewards(Player player)
		{
			shootQuestRewards.Grant(player);
		}

		protected override bool doesItemTypeHaveSkins => true;

		public ItemGunAsset() : base()
		{

		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_shoot = LoadRedirectableAsset<AudioClip>(p.bundle, "Shoot", p.data, "ShootAudioClip");
			_reload = LoadRedirectableAsset<AudioClip>(p.bundle, "Reload", p.data, "ReloadAudioClip");
			_hammer = LoadRedirectableAsset<AudioClip>(p.bundle, "Hammer", p.data, "HammerAudioClip");
			_aim = LoadRedirectableAsset<AudioClip>(p.bundle, "Aim", p.data, "AimAudioClip");
			_minigun = LoadRedirectableAsset<AudioClip>(p.bundle, "Minigun", p.data, "MinigunAudioClip");
			_chamberJammedSound = LoadRedirectableAsset<AudioClip>(p.bundle, "ChamberJammed", p.data, "ChamberJammedAudioClip");
			fireDelaySound = LoadRedirectableAsset<AudioClip>(p.bundle, "FireDelay", p.data, "FireDelayAudioClip");

			_projectile = p.bundle.load<GameObject>("Projectile");

			ammoMin = p.data.ParseUInt8("Ammo_Min");
			ammoMax = p.data.ParseUInt8("Ammo_Max");

			sightID = p.data.ParseUInt16("Sight");
			tacticalID = p.data.ParseUInt16("Tactical");
			gripID = p.data.ParseUInt16("Grip");
			barrelID = p.data.ParseUInt16("Barrel");
			defaultMagazineLegacyId = p.data.ParseGuidOrLegacyId("Magazine", out defaultMagazineGuid);

			int magazineReplacementCount = p.data.ParseInt32("Magazine_Replacements");
			magazineReplacements = new MagazineReplacement[magazineReplacementCount];
			for (int magazineReplacementIndex = 0; magazineReplacementIndex < magazineReplacementCount; magazineReplacementIndex++)
			{
				System.Guid magazineReplacementGuid;
				ushort magazineReplacementID = p.data.ParseGuidOrLegacyId("Magazine_Replacement_" + magazineReplacementIndex + "_ID", out magazineReplacementGuid);
				string magazineReplacementMap = p.data.GetString("Magazine_Replacement_" + magazineReplacementIndex + "_Map");

				MagazineReplacement magazineReplacement = new MagazineReplacement();
				magazineReplacement.legacyId = magazineReplacementID;
				magazineReplacement.guid = magazineReplacementGuid;
				magazineReplacement.map = magazineReplacementMap;

				magazineReplacements[magazineReplacementIndex] = magazineReplacement;
			}

			unplace = p.data.ParseFloat("Unplace");
			replace = p.data.ParseFloat("Replace", defaultValue: 1.0f);
			RechamberAfterShotDelay = p.data.ParseFloat("RechamberAfterShotDelay", defaultValue: 0.25f);
			EjectAfterHammerDelay = p.data.ParseFloat("EjectAfterHammerDelay", defaultValue: 0.45f);
			EjectAfterReloadDelay = p.data.ParseFloat("EjectAfterReloadDelay", defaultValue: 0.5f);

			hasSight = p.data.ContainsKey("Hook_Sight");
			hasTactical = p.data.ContainsKey("Hook_Tactical");
			hasGrip = p.data.ContainsKey("Hook_Grip");
			hasBarrel = p.data.ContainsKey("Hook_Barrel");

			int magazineCaliberCount = p.data.ParseInt32("Magazine_Calibers");
			if (magazineCaliberCount > 0)
			{
				magazineCalibers = new ushort[magazineCaliberCount];
				for (int caliberIndex = 0; caliberIndex < magazineCaliberCount; caliberIndex++)
				{
					magazineCalibers[caliberIndex] = p.data.ParseUInt16("Magazine_Caliber_" + caliberIndex);
				}

				int attachmentCaliberCount = p.data.ParseInt32("Attachment_Calibers");
				if (attachmentCaliberCount > 0)
				{
					attachmentCalibers = new ushort[attachmentCaliberCount];
					for (int caliberIndex = 0; caliberIndex < attachmentCaliberCount; caliberIndex++)
					{
						attachmentCalibers[caliberIndex] = p.data.ParseUInt16("Attachment_Caliber_" + caliberIndex);
					}
				}
				else
				{
					attachmentCalibers = magazineCalibers;
				}
			}
			else
			{
				magazineCalibers = new ushort[1];
				magazineCalibers[0] = p.data.ParseUInt16("Caliber");

				attachmentCalibers = magazineCalibers;
			}

			firerate = p.data.ParseUInt8("Firerate");
			//_firerate = (byte) Mathf.Ceil(data.readByte("Firerate") / 2.0f);

			action = (EAction) System.Enum.Parse(typeof(EAction), p.data.GetString("Action"), true);

			if (p.data.ContainsKey("Delete_Empty_Magazines"))
			{
				shouldDeleteEmptyMagazines = true;
			}
			else
			{
				bool actionDeleteByDefault = action == EAction.Pump || action == EAction.Rail || action == EAction.String || action == EAction.Rocket || action == EAction.Break;
				shouldDeleteEmptyMagazines = p.data.ParseBool("Should_Delete_Empty_Magazines", defaultValue: actionDeleteByDefault);
			}
			requiresNonZeroAttachmentCaliber = p.data.ParseBool("Requires_NonZero_Attachment_Caliber");

			bursts = p.data.ParseInt32("Bursts");

			hasSafety = p.data.ContainsKey("Safety");
			hasSemi = p.data.ContainsKey("Semi");
			hasAuto = p.data.ContainsKey("Auto");
			hasBurst = bursts > 0;

			isTurret = p.data.ContainsKey("Turret");
			driverTurretViewmodelMode = p.data.ParseEnum("DriverTurretViewmodelMode", EDriverTurretViewmodelMode.OffscreenWhileAiming);

			if (hasAuto)
			{
				firemode = EFiremode.AUTO;
			}
			else if (hasSemi)
			{
				firemode = EFiremode.SEMI;
			}
			else if (hasBurst)
			{
				firemode = EFiremode.BURST;
			}
			else if (hasSafety)
			{
				firemode = EFiremode.SAFETY;
			}

			spreadAim = p.data.ParseFloat("Spread_Aim");

			if (p.data.ContainsKey("Spread_Angle_Degrees"))
			{
				baseSpreadAngleRadians = Mathf.Deg2Rad * p.data.ParseFloat("Spread_Angle_Degrees");
#pragma warning disable
				// Spread_Hip was added to a forward unit vector and then normalized,
				// so we can find the converted spreadHip with tan(angle) = opposite / adjacent where adjacent is 1
				spreadHip = Mathf.Tan(baseSpreadAngleRadians);
#pragma warning restore
			}
			else
			{
#pragma warning disable
				spreadHip = p.data.ParseFloat("Spread_Hip");
				// Spread_Hip was added to a forward unit vector and then normalized,
				// so we can find the converted angle with tan(angle) = opposite / adjacent where adjacent is 1
				baseSpreadAngleRadians = Mathf.Atan(spreadHip);
				if (shouldLogSpreadConversion)
				{
					UnturnedLog.info($"Converted \"{FriendlyName}\" Spread_Hip {spreadHip} to {baseSpreadAngleRadians * Mathf.Rad2Deg} degrees");
				}
#pragma warning restore
			}

			spreadSprint = p.data.ParseFloat("Spread_Sprint", defaultValue: 1.25f);
			spreadCrouch = p.data.ParseFloat("Spread_Crouch", defaultValue: 0.85f);
			spreadProne = p.data.ParseFloat("Spread_Prone", defaultValue: 0.7f);
			spreadSwimming = p.data.ParseFloat("Spread_Swimming", defaultValue: 1.1f);
			spreadMidair = p.data.ParseFloat("Spread_Midair", defaultValue: 1.5f);

			recoilMin_x = p.data.ParseFloat("Recoil_Min_X");
			recoilMin_y = p.data.ParseFloat("Recoil_Min_Y");

			recoilMax_x = p.data.ParseFloat("Recoil_Max_X");
			recoilMax_y = p.data.ParseFloat("Recoil_Max_Y");
			aimingRecoilMultiplier = p.data.ParseFloat("Aiming_Recoil_Multiplier", defaultValue: 1.0f);

			recover_x = p.data.ParseFloat("Recover_X");
			recover_y = p.data.ParseFloat("Recover_Y");

			recoilSprint = p.data.ParseFloat("Recoil_Sprint", defaultValue: 1.25f);
			recoilCrouch = p.data.ParseFloat("Recoil_Crouch", defaultValue: 0.85f);
			recoilProne = p.data.ParseFloat("Recoil_Prone", defaultValue: 0.7f);
			recoilSwimming = p.data.ParseFloat("Recoil_Swimming", defaultValue: 1.1f);
			recoilMidair = p.data.ParseFloat("Recoil_Midair", defaultValue: 1.0f);

			shakeMin_x = p.data.ParseFloat("Shake_Min_X");
			shakeMin_y = p.data.ParseFloat("Shake_Min_Y");
			shakeMin_z = p.data.ParseFloat("Shake_Min_Z");

			shakeMax_x = p.data.ParseFloat("Shake_Max_X");
			shakeMax_y = p.data.ParseFloat("Shake_Max_Y");
			shakeMax_z = p.data.ParseFloat("Shake_Max_Z");

			ballisticSteps = p.data.ParseUInt8("Ballistic_Steps");
			ballisticTravel = p.data.ParseFloat("Ballistic_Travel");

			bool hasBallisticSteps = p.data.ContainsKey("Ballistic_Steps") && ballisticSteps > 0;
			bool hasBallisticTravel = p.data.ContainsKey("Ballistic_Travel") && ballisticTravel > 0.1f;
			if (hasBallisticSteps && hasBallisticTravel)
			{
				// Having both of these options configurable was probably a bad idea...
				float testBallisticRange = ballisticSteps * ballisticTravel;
				float rangeError = Mathf.Abs(testBallisticRange - range);
				if (rangeError > 0.1f) // 0.1m tolerance for range mismatch
				{
					Assets.ReportError(this, "range and manual ballistic range are mismatched by " + rangeError + "m. Recommended to only have one or the other specified!");
				}
			}
			else if (hasBallisticSteps) // Doesn't have ballisticTravel
			{
				ballisticTravel = range / ballisticSteps;
			}
			else if (hasBallisticTravel) // Doesn't have ballisticSteps
			{
				ballisticSteps = (byte) Mathf.CeilToInt(range / ballisticTravel);
			}
			else // Doesn't have ballisticSteps or ballisticTravel
			{
				ballisticTravel = 10.0f;
				ballisticSteps = (byte) Mathf.CeilToInt(range / ballisticTravel);
			}

			muzzleVelocity = ballisticTravel * PlayerInput.TOCK_PER_SECOND;

			// Backwards compatibility for old Ballistic_Drop property disabling bullet drop.
			if (p.data.TryParseFloat("Ballistic_Drop", out float ballisticDrop))
			{
				if (ballisticDrop < 0.000001f)
				{
					bulletGravityMultiplier = 0.0f;
				}
				else
				{
					// The way Ballistic_Drop worked:
					// Each bullet has a normalized direction vector and moves with a constant speed. During physics update
					// the Ballistic_Drop value was subtracted from the Y component and direction was re-normalized.
					// We can slightly simplify performance with 2D rather than 3D as if bullet were moving only along Z axis.
					float heightDelta = 0.0f;
					Vector2 direction = Vector2.right;
					for (int step = 0; step < ballisticSteps; ++step)
					{
						heightDelta += direction.y * ballisticTravel;
						direction.y -= ballisticDrop;
						direction.Normalize();
					}

					// We can use this formula to calculate acceleration, initial vertical velocity being zero:
					// displacement = initial velocity * time + 0.5 * acceleration * time²
					// displacement = 0.5 * acceleration * time²
					// acceleration * time² = 2 * displacement
					// acceleration = 2 * displacement / time²
					float totalDeltaTime = ballisticSteps * UseableGun.BALLISTICS_DELTA_TIME;
					float gravity = (2.0f * heightDelta) / (totalDeltaTime * totalDeltaTime);
					bulletGravityMultiplier = gravity / -9.81f;

					if (shouldLogBallisticDropConversion)
					{
						UnturnedLog.info($"Converted \"{FriendlyName}\" Ballistic_Drop {ballisticDrop} to Bullet_Gravity_Multiplier {bulletGravityMultiplier}");
					}
				}
			}
			else
			{
				bulletGravityMultiplier = p.data.ParseFloat("Bullet_Gravity_Multiplier", defaultValue: 4.0f);
			}

			if (p.data.ContainsKey("Ballistic_Force"))
			{
				ballisticForce = p.data.ParseFloat("Ballistic_Force");
			}
			else
			{
				ballisticForce = 0.002f;
			}

			damageFalloffRange = p.data.ParseFloat("Damage_Falloff_Range", defaultValue: 1.0f);
			damageFalloffMaxRange = p.data.ParseFloat("Damage_Falloff_Max_Range", defaultValue: 1.0f);
			damageFalloffMultiplier = p.data.ParseFloat("Damage_Falloff_Multiplier", defaultValue: 1.0f);

			projectileLifespan = p.data.ParseFloat("Projectile_Lifespan", defaultValue: 30.0f);

			projectilePenetrateBuildables = p.data.ContainsKey("Projectile_Penetrate_Buildables");
			projectileExplosionLaunchSpeed = p.data.ParseFloat("Projectile_Explosion_Launch_Speed", defaultValue: playerDamageMultiplier.damage * 0.1f);

			reloadTime = p.data.ParseFloat("Reload_Time");
			hammerTime = p.data.ParseFloat("Hammer_Time");

#pragma warning disable
			muzzle = p.data.ParseGuidOrLegacyId("Muzzle", out muzzleGuid);
#pragma warning restore
			explosion = p.data.ParseGuidOrLegacyId("Explosion", out projectileExplosionEffectGuid);

			if (p.data.ContainsKey("Shell"))
			{
#pragma warning disable
				shell = p.data.ParseGuidOrLegacyId("Shell", out shellGuid);
#pragma warning restore
			}
			else
			{
				if (action == EAction.Pump || action == EAction.Break)
				{
					// Shell
					shellGuid = new System.Guid("0dc9bf936ce0409585fe9525287c7a7d");
				}
				else if (action != EAction.Rail)
				{
					// Casing
					shellGuid = new System.Guid("f380a6a6f41f422c9f5b9ac13e3b13e8");
				}
			}

			if (p.data.ContainsKey("Alert_Radius"))
			{
				alertRadius = p.data.ParseFloat("Alert_Radius");
			}
			else
			{
				alertRadius = 48;
			}

			if (p.data.ContainsKey("Range_Rangefinder"))
			{
				rangeRangefinder = p.data.ParseFloat("Range_Rangefinder");
			}
			else
			{
				rangeRangefinder = p.data.ParseFloat("Range");
			}

			instakillHeadshots = p.data.ParseBool("Instakill_Headshots");
			infiniteAmmo = p.data.ParseBool("Infinite_Ammo");
			ammoPerShot = p.data.ParseUInt8("Ammo_Per_Shot", defaultValue: 1);
			fireDelay = Mathf.RoundToInt(p.data.ParseFloat("Fire_Delay_Seconds") * PlayerInput.TOCK_PER_SECOND);
			allowMagazineChange = p.data.ParseBool("Allow_Magazine_Change", defaultValue: true);
			canAimDuringSprint = p.data.ParseBool("Can_Aim_During_Sprint", defaultValue: false);
			aimingMovementSpeedMultiplier = p.data.ParseFloat("Aiming_Movement_Speed_Multiplier", defaultValue: canAimDuringSprint ? 1.0f : 0.75f);
			MustAimToShoot = p.data.ParseBool("Must_Aim_To_Shoot", action == EAction.Minigun);
			ShouldForceStopAimingAfterShooting = p.data.ParseBool("Stop_Aiming_After_Shooting");

			canEverJam = p.data.ContainsKey("Can_Ever_Jam");
			if (canEverJam)
			{
				jamQualityThreshold = p.data.ParseFloat("Jam_Quality_Threshold", defaultValue: 0.4f);
				jamMaxChance = p.data.ParseFloat("Jam_Max_Chance", defaultValue: 0.1f);
				unjamChamberAnimName = p.data.GetString("Unjam_Chamber_Anim", defaultValue: "UnjamChamber");
			}

			float defaultGunshotRolloffDistance;
			if (action == EAction.String)
			{
				defaultGunshotRolloffDistance = 16.0f;
			}
			else if (action == EAction.Rocket)
			{
				defaultGunshotRolloffDistance = 64.0f;
			}
			else
			{
				defaultGunshotRolloffDistance = 512.0f;
			}
			gunshotRolloffDistance = p.data.ParseFloat("Gunshot_Rolloff_Distance", defaultValue: defaultGunshotRolloffDistance);

			shootQuestRewards.Parse(p.data, p.localization, this, "Shoot_Quest_Rewards", "Shoot_Quest_Reward_");

			// Experimented with calculating using existing Aim_Start animation length but that looked bad.
			aimInDuration = p.data.ParseFloat("Aim_In_Duration", defaultValue: 0.2f);
			shouldScaleAimAnimations = p.data.ParseBool("Scale_Aim_Animation_Speed", defaultValue: true);

			int defaultRechamberAfterShotCount = (action == EAction.Bolt || action == EAction.Pump) ? 1 : 0;
			RechamberAfterShotCount = p.data.ParseInt32("RechamberAfterShotCount", defaultRechamberAfterShotCount);

			CasingEjectCountAfterRechamberingAfterShooting = p.data.ParseInt32("CasingEjectCountAfterRechamberingAfterShooting", 1);

			int defaultCasingEjectCountAfterReload = (action == EAction.Break ? ammoMax : 0);
			CasingEjectCountAfterReload = p.data.ParseInt32("CasingEjectCountAfterReload", defaultCasingEjectCountAfterReload);

			bool defaultEjectCasingAfterShooting = (action == EAction.Trigger || action == EAction.Minigun);
			ShouldEjectCasingAfterShooting = p.data.ParseBool("EjectCasingAfterShooting", defaultValue: defaultEjectCasingAfterShooting);

			RechamberAfterMagazineAttached = p.data.ParseEnum("RechamberAfterMagazineAttached", ERechamberGunAfterReloadMode.IfAmmoWasEmpty);
			RechamberAfterMagazineDetached = p.data.ParseEnum("RechamberAfterMagazineDetached", ERechamberGunAfterReloadMode.Always);
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Gun
			// Game data for Gun Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Gun");
			data.Append("GUID", GUID); // PFK

			data.Append("Aim_In_Duration", aimInDuration);
			data.Append("Aiming_Movement_Speed_Multiplier", aimingMovementSpeedMultiplier);
			data.Append("Alert_Radius", alertRadius);
			data.Append("Can_Aim_During_Sprint", canAimDuringSprint);
			data.Append("Must_Aim_To_Shoot", MustAimToShoot);
			data.Append("Range_Rangefinder", rangeRangefinder);
			data.Append("Attachment_Calibers", attachmentCalibers.Length); // Get original value of "Attachment_Calibers".

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Gun_AttachmentCaliber
			// Keyless child table for Attachment_Caliber_# values.
			for (byte index = 0; index < attachmentCalibers.Length; index++)
			{
				CargoDeclaration aCal = builder.AddDeclaration("Gun_AttachmentCaliber");
				aCal.Append("GUID", GUID); // FK
				aCal.Append("Caliber", attachmentCalibers[index]);
			}

			data.Append("Magazine_Calibers", magazineCalibers.Length); // Get original value of "Magazine_Calibers".

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Gun_MagazineCaliber
			// Keyless child table for Magazine_Caliber_# values.
			for (byte index = 0; index < magazineCalibers.Length; index++)
			{
				CargoDeclaration mCal = builder.AddDeclaration("Gun_MagazineCaliber");
				mCal.Append("GUID", GUID); // FK
				mCal.Append("Caliber", magazineCalibers[index]);
			}

			data.Append("Requires_NonZero_Attachment_Caliber", requiresNonZeroAttachmentCaliber);
			data.Append("Damage_Falloff_Max_Range", damageFalloffMaxRange);
			data.Append("Damage_Falloff_Multiplier", damageFalloffMultiplier);
			data.Append("Damage_Falloff_Range", damageFalloffRange);
			data.Append("Instakill_Headshots", instakillHeadshots);
			data.Append("Action", action);
			data.Append("Auto", hasAuto);
			data.Append("hasBurst", hasBurst); // Useful to include here, rather than determining on-wiki from Bursts.
			data.Append("Bursts", bursts);
			data.Append("fireDelay", fireDelay); // Appends final value from "Fire_Delay_Seconds".
			data.Append("Firerate", firerate);
			data.Append("Safety", hasSafety);
			data.Append("Semi", hasSemi);
			data.Append("Barrel", barrelID);
			data.Append("Grip", gripID);
			data.Append("Sight", sightID);
			data.Append("Tactical", tacticalID);
			data.Append("Hook_Barrel", hasBarrel);
			data.Append("Hook_Grip", hasGrip);
			data.Append("Hook_Sight", hasSight);
			data.Append("Hook_Tactical", hasTactical);
			data.Append("Can_Ever_Jam", canEverJam);
			data.Append("Jam_Quality_Threshold", jamQualityThreshold);
			data.Append("Jam_Max_Chance", jamMaxChance);
			data.Append("Allow_Magazine_Change", allowMagazineChange);
			data.Append("Ammo_Max", ammoMax);
			data.Append("Ammo_Min", ammoMin);
			data.Append("Ammo_Per_Shot", ammoPerShot);
			data.Append("Hammer_Time", hammerTime);
			data.Append("Infinite_Ammo", infiniteAmmo);
			data.Append("Magazine", defaultMagazineLegacyId);
			data.Append("MagazineGUID", defaultMagazineGuid);
			data.Append("Magazine_Replacements", magazineReplacements.Length); // Get original value of "Magazine_Replacements".

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Gun_MagazineReplacement
			// Child table for Magazine_Replacements.
			// GUID and magazineReplacementIndex form a composite key.
			for (int index = 0; index < magazineReplacements.Length; index++)
			{
				CargoDeclaration mag = builder.AddDeclaration("Gun_MagazineReplacement");
				mag.Append("GUID", GUID); // FK
				mag.Append("magazineReplacementIndex", index);

				mag.Append("ID", magazineReplacements[index].legacyId);
				mag.Append("MagazineGUID", magazineReplacements[index].guid);
				mag.Append("Map", magazineReplacements[index].map);
			}

			data.Append("Reload_Time", reloadTime);
			data.Append("Replace", replace);
			data.Append("Should_Delete_Empty_Magazines", shouldDeleteEmptyMagazines);
			data.Append("Unplace", unplace);
			data.Append("Ballistic_Steps", ballisticSteps);
			data.Append("Ballistic_Travel", ballisticTravel);
			data.Append("Bullet_Gravity_Multiplier", bulletGravityMultiplier);
			data.Append("Ballistic_Force", ballisticForce);
			data.Append("Projectile_Explosion_Launch_Speed", projectileExplosionLaunchSpeed);
			data.Append("Projectile_Lifespan", projectileLifespan);
			data.Append("Projectile_Penetrate_Buildables", projectilePenetrateBuildables);
			data.Append("Aiming_Recoil_Multiplier", aimingRecoilMultiplier);
			data.Append("Recoil_Crouch", recoilCrouch);
			data.Append("Recoil_Max_X", recoilMax_x);
			data.Append("Recoil_Max_Y", recoilMax_y);
			data.Append("Recoil_Min_X", recoilMin_x);
			data.Append("Recoil_Min_Y", recoilMin_y);
			data.Append("Recoil_Midair", recoilMidair);
			data.Append("Recoil_Prone", recoilProne);
			data.Append("Recoil_Sprint", recoilSprint);
			data.Append("Recoil_Swimming", recoilSwimming);
			data.Append("Recover_X", recover_x);
			data.Append("Recover_Y", recover_y);
			data.Append("Shake_Max_X", shakeMax_x);
			data.Append("Shake_Min_X", shakeMin_x);
			data.Append("Shake_Max_Y", shakeMax_y);
			data.Append("Shake_Min_Y", shakeMin_y);
			data.Append("Shake_Max_Z", shakeMax_z);
			data.Append("Shake_Min_Z", shakeMin_z);
			data.Append("spreadAim", baseSpreadAngleRadians * spreadAim); // Derived from "Spread_Aim".
			data.Append("baseSpreadAngleRadians", baseSpreadAngleRadians); // Derived from "Spread_Angle_Degrees".
			data.Append("Spread_Crouch", spreadCrouch);
			data.Append("Spread_Midair", spreadMidair);
			data.Append("Spread_Prone", spreadProne);
			data.Append("Spread_Sprint", spreadSprint);
			data.Append("Spread_Swimming", spreadSwimming);
		}

		protected override AudioReference GetDefaultInventoryAudio()
		{
			if (name.Contains("Bow", System.StringComparison.InvariantCultureIgnoreCase))
			{
				// We do not currently have a good sound for wooden bows.
				return base.GetDefaultInventoryAudio();
			}

			if (size_x <= 2 && size_y <= 2)
			{
				// Small gun like a pistol.
				return new AudioReference("core.masterbundle", "Sounds/Inventory/SmallGunAttachment.asset");
			}
			else
			{
				return new AudioReference("core.masterbundle", "Sounds/Inventory/LargeGunAttachment.asset");
			}
		}

		private static CommandLineFlag shouldLogBallisticDropConversion = new CommandLineFlag(false, "-LogBallisticDropConversion");
		private static CommandLineFlag shouldLogSpreadConversion = new CommandLineFlag(false, "-LogGunSpreadConversion");

		[System.Obsolete("Replaced by GetDefaultMagazineLegacyId")]
		public ushort getMagazineID()
		{
			return GetDefaultMagazineLegacyId();
		}
	}
}
