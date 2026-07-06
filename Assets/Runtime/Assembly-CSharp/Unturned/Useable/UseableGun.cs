////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define DISABLE_SCOPE_SWAY
// #define DISABLE_SPREAD
// #define DISABLE_MAIN_CAMERA_RECOIL
// #define WITH_BALLISTIC_TRAJECTORY_GIZMOS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using SDG.Framework.Devkit;
using SDG.Framework.Water;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Unturned.UnityEx;

namespace SDG.Unturned
{
	/// <summary>
	/// Nelson 2025-10-06: better late than never. Current indices into the gun state array (and
	/// other item state arrays, for that matter) being unnamed makes it hard to read. Updates to
	/// gun-related code should prefer using these names.
	/// </summary>
	public static class GunStateIndices
	{
		public const int SIGHT_ID = 0;
		public const int TACTICAL_ID = 2;
		public const int GRIP_ID = 4;
		public const int BARREL_ID = 6;
		public const int MAGAZINE_ID = 8;
		public const int AMMO = 10;
		public const int FIREMODE = 11;
		public const int TACTICAL_ACTIVE = 12;
		public const int SIGHT_QUALITY = 13;
		public const int TACTICAL_QUALITY = 14;
		public const int GRIP_QUALITY = 15;
		public const int BARREL_QUALITY = 16;
		public const int MAGAZINE_QUALITY = 17;
	}

	public class BulletInfo
	{
		/// <summary>
		/// Starting position when the bullet was fired.
		/// </summary>
		public Vector3 origin;

		/// <summary>
		/// Only available on the server. For use by plugins developers who want to analyze deviation between approximate
		/// start direction and final hit position using <see cref="UseableGun.onBulletSpawned"/> and <see cref="UseableGun.onBulletHit"/>
		/// per public issue #4450. Note that origin and direction on server are not necessarily exactly the same as on
		/// the client for a variety of reasons, including that bullets on the client can be spawned between simulation
		/// frames when the aim direction was different. (Aim direction is updated every drawn frame on the client as
		/// opposed to every simulation frame on the server.) Rather than kicking for one particularly large deviation
		/// we would recommend tracking stats for each shot's actual deviation vs max theoretical deviation. Remember
		/// to account for bullet drop and that aim spread is relative to this direction. (For example, a shotgun may
		/// fire ~8 pellets in a cone around this direction.) Note also that in third-person the bullet can turn up to
		/// 90 degrees from the aim direction if the camera is up against a wall.
		/// </summary>
		public Vector3 ApproximatePlayerAimDirection
		{
			get;
			internal set;
		}

		public Vector3 position
		{
			get;
			internal set;
		}

		public Vector3 velocity
		{
			get;
			internal set;
		}

		public byte steps;
		public float quality;
		public byte pellet;
		public ushort dropID;
		public byte dropAmount;
		public byte dropQuality;
		public ItemBarrelAsset barrelAsset;
		public ItemMagazineAsset magazineAsset;

		/// <summary>
		/// Combination of gun and attachments' bullet gravity multipliers.
		/// </summary>
		internal float gravityMultiplier = 1.0f;

		public Vector3 GetDirection()
		{
			return velocity.normalized;
		}
	}

	public class UseableGun : Useable
	{
		public delegate void ChangeAttachmentRequestHandler(PlayerEquipment equipment, UseableGun gun, Item oldItem, ItemJar newItem, ref bool shouldAllow);

		/// <returns>Whether plugin allowed attachment.</returns>
		private bool changeAttachmentRequested(ChangeAttachmentRequestHandler handler, Item oldItem, ItemJar newItem)
		{
			if (handler != null)
			{
				bool shouldAllow = true;
				handler.Invoke(player.equipment, this, oldItem, newItem, ref shouldAllow);
				return shouldAllow;
			}
			else
			{
				return true;
			}
		}

		public static event ChangeAttachmentRequestHandler onChangeSightRequested;

		private bool changeSightRequested(Item oldItem, ItemJar newItem)
		{
			return changeAttachmentRequested(onChangeSightRequested, oldItem, newItem);
		}

		public static event ChangeAttachmentRequestHandler onChangeTacticalRequested;

		private bool changeTacticalRequested(Item oldItem, ItemJar newItem)
		{
			return changeAttachmentRequested(onChangeTacticalRequested, oldItem, newItem);
		}

		public static event ChangeAttachmentRequestHandler onChangeGripRequested;

		private bool changeGripRequested(Item oldItem, ItemJar newItem)
		{
			return changeAttachmentRequested(onChangeGripRequested, oldItem, newItem);
		}

		public static event ChangeAttachmentRequestHandler onChangeBarrelRequested;

		private bool changeBarrelRequested(Item oldItem, ItemJar newItem)
		{
			return changeAttachmentRequested(onChangeBarrelRequested, oldItem, newItem);
		}

		public static event ChangeAttachmentRequestHandler onChangeMagazineRequested;

		private bool changeMagazineRequested(Item oldItem, ItemJar newItem)
		{
			return changeAttachmentRequested(onChangeMagazineRequested, oldItem, newItem);
		}

		public delegate void BulletSpawnedHandler(UseableGun gun, BulletInfo bullet);
		/// <summary>
		/// Plugin-only event when bullet is fired on server.
		/// </summary>
		public static event BulletSpawnedHandler onBulletSpawned;

		public delegate void BulletHitHandler(UseableGun gun, BulletInfo bullet, InputInfo hit, ref bool shouldAllow);
		/// <summary>
		/// Plugin-only event when bullet hit is received from client.
		/// </summary>
		public static event BulletHitHandler onBulletHit;

		public delegate void ProjectileSpawnedHandler(UseableGun sender, GameObject projectile);
		/// <summary>
		/// Plugin-only event when projectile is spawned on server.
		/// </summary>
		public static event ProjectileSpawnedHandler onProjectileSpawned;

		public static event System.Action<UseableGun> OnReloading_Global;
		public static event System.Action<UseableGun> OnAimingChanged_Global;

		private static readonly float SHAKE_CROUCH = 0.85f;
		private static readonly float SHAKE_PRONE = 0.7f;

		private static readonly float SWAY_CROUCH = 0.85f;
		private static readonly float SWAY_PRONE = 0.7f;

		private Local localization;
		private IconsBundle icons;

		private SleekButtonIcon sightButton;
		private SleekJars sightJars;
		private SleekButtonIcon tacticalButton;
		private SleekJars tacticalJars;
		private SleekButtonIcon gripButton;
		private SleekJars gripJars;
		private SleekButtonIcon barrelButton;
		private ISleekLabel barrelQualityLabel;
		private ISleekImage barrelQualityImage;
		private SleekJars barrelJars;
		private SleekButtonIcon magazineButton;
		private ISleekLabel magazineQualityLabel;
		private ISleekImage magazineQualityImage;
		private SleekJars magazineJars;
		private ISleekLabel rangeLabel;
		private ISleekBox infoBox;
		private ISleekLabel ammoLabel;
		private ISleekLabel firemodeLabel;
		private ISleekLabel attachLabel;

		internal Attachments firstAttachments;
		private ParticleSystem firstShellEmitter;
		private ParticleSystem firstMuzzleEmitter;
		private Transform firstFakeLight;
		private Transform firstFakeLight_0;
		private Transform firstFakeLight_1;

		private Attachments thirdAttachments;
		private ParticleSystem thirdShellEmitter;
		private ParticleSystemRenderer thirdShellRenderer;
		private ParticleSystem thirdMuzzleEmitter;

		private float minigunSpeed;
		private float minigunDistance;
		private Transform firstMinigunBarrel;
		private Transform thirdMinigunBarrel;

		// Futuristic physical ammo labels on gun, requested by modders.
		private UnityEngine.UI.Text firstAmmoCounter;
		private UnityEngine.UI.Text thirdAmmoCounter;

		private EffectAsset currentTracerEffectAsset;
		private ParticleSystem tracerEmitter;

		private AudioSource gunshotAudioSource;
		private AudioSource whir;

		/// <summary>
		/// reticuleHook.localPosition after instantiation, or zero if null.
		/// </summary>
		private Vector3 originalReticuleHookLocalPosition;

		private bool isShooting;

		/// <summary>
		/// True if startPrimary was called this simulation frame.
		/// Allows gun to shoot even if stopPrimary is called immediately afterwards.
		/// </summary>
		private bool wasTriggerJustPulled;

		private bool isJabbing;
		public bool isAiming
		{
			get;
			protected set;
		}
		private bool isMinigunSpinning;
		private bool isSprinting;
		private bool isReloading;
		private bool isHammering;
		private bool isAttaching;
		private bool isUnjamming;

		private float lastShot;
		private float lastRechamber;
		private uint lastFire;
		private uint lastJab;
		private bool isFired;
		private int bursts;

		/// <summary>
		/// Remaining calls to tock before firing.
		/// </summary>
		private int fireDelayCounter;

		private int _aimAccuracy;
		private int AimAccuracy
		{
			set
			{
				if (_aimAccuracy != value)
				{
					_aimAccuracy = value;
					if (channel.IsLocalPlayer)
					{
						player.look.IsScopeHalfwayAimedIn = _aimAccuracy >= 5;
					}
				}
			}
		}
		
		private uint steadyAccuracy;
		private bool canSteady;
		private float swayTime;

		private List<BulletInfo> bullets; // bullets the owner is currently doing ballistics for or the server is waiting for

		private float startedReload;
		private float startedHammer;
		private float startedUnjammingChamber;
		private float reloadTime;
		private float hammerTime;
		private float unjamChamberDuration;
		private bool needsHammer;

		private bool needsRechamber;
		/// <summary>
		/// Shot counter used by needsRechamber and RechamberAfterShotCount.
		/// </summary>
		private int shotCountForRechamber;

		private bool needsEject;
		private bool needsUnload;
		private bool needsUnplace;
		private bool needsReplace;

		/// <summary>
		/// Is the tactical attachment toggle on?
		/// e.g. True when the laser is enabled.
		/// </summary>
		private bool interact;

		/// <summary>
		/// Should stat modifiers from the current tactical attachment be used?
		/// </summary>
		private bool shouldEnableTacticalStats
		{
			get
			{
				ItemTacticalAsset tac = thirdAttachments.tacticalAsset;
				if (tac != null)
				{
					if (tac.isLaser || tac.isLight || tac.isRangefinder)
					{
						// These types of tactical attachments only apply their benefit when turned on.
						return interact;
					}
					else
					{
						return true;
					}
				}
				else
				{
					return false;
				}
			}
		}

		private byte ammo;
		private EFiremode firemode;

		private static List<PlayerInventorySearchResultV2> sightSearchResults = new List<PlayerInventorySearchResultV2>();
		private static List<PlayerInventorySearchResultV2> tacticalSearchResults = new List<PlayerInventorySearchResultV2>();
		private static List<PlayerInventorySearchResultV2> gripSearchResults = new List<PlayerInventorySearchResultV2>();
		private static List<PlayerInventorySearchResultV2> barrelSearchResults = new List<PlayerInventorySearchResultV2>();
		private static List<PlayerInventorySearchResultV2> magazineSearchResults = new List<PlayerInventorySearchResultV2>();

		/// <summary>
		/// Factor e.g. 2 is a 2x multiplier.
		/// Prior to 2022-04-11 this was the target field of view. (90/fov)
		/// </summary>
		private float firstPersonZoomFactor;
		/// <summary>
		/// Zoom multiplier in third-person.
		/// </summary>
		private float thirdPersonZoomFactor = DEFAULT_THIRD_PERSON_ZOOM_FACTOR;
		/// <summary>
		/// Whether main camera field of view should zoom without scope camera / scope overlay.
		/// </summary>
		private bool shouldZoomUsingEyes;
		private float crosshair;

#if !DEDICATED_SERVER
		private GameObject laserGameObject;
		private Transform laserTransform;
		private Material laserMaterial;
#endif // !DEDICATED_SERVER

		private bool wasLaser;
		private bool wasLight;
		private bool wasRange;
		private bool wasBayonet;
		private bool inRange;
		private bool fireTacticalInput;
		private RaycastHit contact;

		public ItemGunAsset equippedGunAsset => player.equipment.asset as ItemGunAsset;

		protected VehicleTurretEventHook GetVehicleTurretEventHook()
		{
			if (player.equipment.isTurret)
			{
				Passenger seat = player.movement.getVehicleSeat();
				return seat?.turretEventHook;
			}
			else
			{
				return null;
			}
		}

		private UseableGunEventHook firstEventComponent;
		private UseableGunEventHook thirdEventComponent;
		private UseableGunEventHook characterEventComponent;

		[System.Obsolete]
		public void askFiremode(CSteamID steamID, byte id)
		{
			ReceiveChangeFiremode((EFiremode) id);
		}

		private static readonly ServerInstanceMethod<EFiremode> SendChangeFiremode = ServerInstanceMethod<EFiremode>.Get(typeof(UseableGun), nameof(ReceiveChangeFiremode));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askFiremode))]
		public void ReceiveChangeFiremode(EFiremode newFiremode)
		{
			if (player.equipment.isBusy)
			{
				return;
			}

			if (isFired)
			{
				return;
			}

			if (isReloading || isHammering || isUnjamming || needsRechamber)
			{
				return;
			}

			if (player.equipment.asset == null)
			{
				return;
			}

			if (newFiremode == EFiremode.SAFETY)
			{
				if (equippedGunAsset.hasSafety)
				{
					firemode = newFiremode;
				}
			}
			else if (newFiremode == EFiremode.SEMI)
			{
				if (equippedGunAsset.hasSemi)
				{
					firemode = newFiremode;
				}
			}
			else if (newFiremode == EFiremode.AUTO)
			{
				if (equippedGunAsset.hasAuto)
				{
					firemode = newFiremode;
				}
			}
			else if (newFiremode == EFiremode.BURST)
			{
				if (equippedGunAsset.hasBurst)
				{
					firemode = newFiremode;
				}
			}

			player.equipment.state[11] = (byte) firemode;
			player.equipment.sendUpdateState();

			EffectManager.TriggerFiremodeEffect(transform.position);
		}

		public void askInteractGun()
		{
			if (player.equipment.isBusy)
			{
				return;
			}

			if (isFired)
			{
				return;
			}

			if (isReloading || isHammering || isUnjamming || needsRechamber)
			{
				return;
			}

			if (thirdAttachments.tacticalAsset == null)
			{
				return;
			}

			if (thirdAttachments.tacticalAsset.isMelee)
			{
				if (!isSprinting && (!player.movement.isSafe || !player.movement.isSafeInfo.noWeapons) && firemode != EFiremode.SAFETY)
				{
					isJabbing = true;
				}
			}
			else
			{
				interact = !interact;

				player.equipment.state[12] = (byte) (interact ? 1 : 0);
				player.equipment.sendUpdateState();

				EffectManager.TriggerFiremodeEffect(transform.position);
			}
		}

		/// <summary>
		/// Original barrel and magazine assets are supplied because they may have already been deleted. Barrel is only
		/// valid if quality was greater than zero.
		/// </summary>
		private void project(Vector3 origin, Vector3 direction, ItemBarrelAsset barrelAsset, ItemMagazineAsset magazineAsset)
		{
			if (gunshotAudioSource != null)
			{
				playGunshot();
			}

			if (equippedGunAsset.ShouldEjectCasingAfterShooting)
			{
				EjectCasingAfterShooting();
			}

			if (barrelAsset == null || !barrelAsset.isBraked)
			{
				if (firstMuzzleEmitter != null && player.look.perspective == EPlayerPerspective.FIRST && !equippedGunAsset.isTurret)
				{
					firstMuzzleEmitter.Emit(1);

					Light light = firstMuzzleEmitter.GetComponent<Light>();
					if (light != null)
					{
						light.enabled = true;
					}

					if (firstFakeLight != null)
					{
						light = firstFakeLight.GetComponent<Light>();
						if (light != null)
						{
							light.enabled = true;
						}
					}
				}

				if (thirdMuzzleEmitter != null && (!channel.IsLocalPlayer || player.look.perspective == EPlayerPerspective.THIRD || equippedGunAsset.isTurret))
				{
					thirdMuzzleEmitter.Emit(1);

					Light light = thirdMuzzleEmitter.GetComponent<Light>();
					if (light != null)
					{
						light.enabled = true;
					}
				}
			}

			bool isMagazineProjectile = false;
			float damageMultiplier = 1.0f;
			float blastRadiusMultiplier = 1.0f;
			float forceMultiplier = 1.0f;

			if (magazineAsset != null)
			{
				isMagazineProjectile = magazineAsset.ProjectilePrefabOverride != null;
				damageMultiplier *= magazineAsset.projectileDamageMultiplier;
				blastRadiusMultiplier *= magazineAsset.projectileBlastRadiusMultiplier;
				forceMultiplier *= magazineAsset.projectileLaunchForceMultiplier;
			}

			Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);
			GameObject projectilePrefab = isMagazineProjectile ? magazineAsset.ProjectilePrefabOverride : equippedGunAsset.projectile;
			GameObject projectileGameObject = Instantiate(projectilePrefab, origin, rotation);
			projectileGameObject.name = "Projectile";
			EffectManager.RegisterDebris(projectileGameObject);

			Rigidbody rb = projectileGameObject.GetComponent<Rigidbody>();
			if (rb != null)
			{
				rb.AddForce(direction * equippedGunAsset.ballisticForce * forceMultiplier);
				rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
			}

			if (channel.IsLocalPlayer)
			{
				if (projectileGameObject.GetComponent<AudioSource>() != null)
				{
					projectileGameObject.GetComponent<AudioSource>().maxDistance = 512;
				}
			}

			// Note: be careful adjusting this because it sets `needsRechamber = true` and the shot count should only
			// be incremented by one for listen server / dedicated server.
			IncrementShotCountForRechamber();

			Rocket rocket = projectileGameObject.AddComponent<Rocket>();
			rocket.ignoreTransform = transform;

			if (Provider.isServer)
			{
				rocket.killer = channel.owner.playerID.steamID;

				if (isMagazineProjectile)
				{
					rocket.range = magazineAsset.range;
					rocket.playerDamage = magazineAsset.playerDamage;
					rocket.zombieDamage = magazineAsset.zombieDamage * damageMultiplier;
					rocket.animalDamage = magazineAsset.animalDamage * damageMultiplier;
					rocket.barricadeDamage = magazineAsset.barricadeDamage * damageMultiplier;
					rocket.structureDamage = magazineAsset.structureDamage * damageMultiplier;
					rocket.vehicleDamage = magazineAsset.vehicleDamage * damageMultiplier;
					rocket.resourceDamage = magazineAsset.resourceDamage * damageMultiplier;
					rocket.objectDamage = magazineAsset.objectDamage * damageMultiplier;
					rocket.penetrateBuildables = magazineAsset.ExplosionPenetratesBuildables;
					rocket.explosionLaunchSpeed = magazineAsset.explosionLaunchSpeed;
				}
				else
				{
					rocket.range = equippedGunAsset.range * blastRadiusMultiplier;
					rocket.playerDamage = equippedGunAsset.playerDamageMultiplier.damage * damageMultiplier;
					rocket.zombieDamage = equippedGunAsset.zombieDamageMultiplier.damage * damageMultiplier;
					rocket.animalDamage = equippedGunAsset.animalDamageMultiplier.damage * damageMultiplier;
					rocket.barricadeDamage = equippedGunAsset.barricadeDamage * damageMultiplier;
					rocket.structureDamage = equippedGunAsset.structureDamage * damageMultiplier;
					rocket.vehicleDamage = equippedGunAsset.vehicleDamage * damageMultiplier;
					rocket.resourceDamage = equippedGunAsset.resourceDamage * damageMultiplier;
					rocket.objectDamage = equippedGunAsset.objectDamage * damageMultiplier;
					rocket.penetrateBuildables = equippedGunAsset.projectilePenetrateBuildables;
					rocket.explosionLaunchSpeed = equippedGunAsset.projectileExplosionLaunchSpeed;
				}

				if (magazineAsset != null && !magazineAsset.IsExplosionEffectRefNull())
				{
					rocket.explosionEffectGuid = magazineAsset.explosionEffectGuid;
					rocket.explosion = magazineAsset.explosion;
				}
				else
				{
					rocket.explosionEffectGuid = equippedGunAsset.projectileExplosionEffectGuid;
					rocket.explosion = equippedGunAsset.explosion;
				}

				rocket.ragdollEffect = player.equipment.getUseableRagdollEffect();

				// Nelson 2025-04-29: some mods attach a Grenade component manually which then doesn't properly credit
				// the shooter. (public issue #5000).
				// Nelson 2025-10-07: this should be kept even though unowned Rocket/Grenade component checks for
				// ownership, as it only looks up the hierarchy and not for sibling components. (Grenade won't
				// find Rocket on the same game object.)
				Grenade grenade = projectileGameObject.GetComponent<Grenade>();
				if (grenade != null)
				{
					grenade.killer = rocket.killer;
				}
			}

			Destroy(projectileGameObject, equippedGunAsset.projectileLifespan);

			lastShot = Time.realtimeSinceStartup;

			onProjectileSpawned?.Invoke(this, projectileGameObject);
			InvokeModHookShotFiredEvents();
		}

		[System.Obsolete]
		public void askProject(CSteamID steamID, Vector3 origin, Vector3 direction, ushort barrelId, ushort magazineId)
		{
			ReceivePlayProject(origin, direction, barrelId, magazineId);
		}

		private static readonly ClientInstanceMethod<Vector3, Vector3, ushort, ushort> SendPlayProject = ClientInstanceMethod<Vector3, Vector3, ushort, ushort>.Get(typeof(UseableGun), nameof(ReceivePlayProject));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askProject))]
		public void ReceivePlayProject(Vector3 origin, [NetPakNormal] Vector3 direction, ushort barrelId, ushort magazineId)
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				ItemBarrelAsset barrelAsset = Assets.find(EAssetType.ITEM, barrelId) as ItemBarrelAsset;
				ItemMagazineAsset magazineAsset = Assets.find(EAssetType.ITEM, magazineId) as ItemMagazineAsset;
				project(origin, direction, barrelAsset, magazineAsset);
			}
		}

		private void trace(Vector3 pos, Vector3 dir)
		{
			if (tracerEmitter == null)
			{
				return;
			}

			if (thirdAttachments.barrelModel != null && thirdAttachments.barrelAsset.isBraked && player.equipment.state[16] > 0)
			{
				return;
			}

			tracerEmitter.transform.position = pos;
			tracerEmitter.transform.rotation = Quaternion.LookRotation(dir);
			tracerEmitter.Emit(1);
		}

#if !DEDICATED_SERVER
		private static MasterBundleReference<OneShotAudioDefinition> bulletFlybyAudioRef = new MasterBundleReference<OneShotAudioDefinition>("core.masterbundle", "Effects/Guns/BulletFlyby.asset");

		private void PlayFlybyAudio(Vector3 origin, Vector3 direction, float range)
		{
			if (MainCamera.instance == null)
				return;

			if (channel.IsLocalPlayer && !player.look.IsLocallyUsingFreecam)
			{
				// Only do flyby audio for local player if using freecam.
				return;
			}

			const float audioRadius = 5.0f;
			const float squaredAudioRadius = audioRadius * audioRadius;

			Vector3 listenerWorldPosition = MainCamera.instance.transform.position;
			float listenerDistanceAlongRay = Vector3.Dot(listenerWorldPosition - origin, direction);
			if (listenerDistanceAlongRay > 0.0f && listenerDistanceAlongRay < range)
			{
				Vector3 closestPointToListener = origin + (direction * listenerDistanceAlongRay);
				if ((closestPointToListener - listenerWorldPosition).sqrMagnitude < squaredAudioRadius)
				{
					OneShotAudioDefinition audioDef = bulletFlybyAudioRef.loadAsset();
					if (audioDef == null)
					{
						UnturnedLog.warn("Missing built-in bullet flyby audio");
						return;
					}

					AudioClip audioClip = audioDef.GetRandomClip();
					OneShotAudioParameters audioParams = new OneShotAudioParameters(closestPointToListener, audioClip);
					audioParams.minDistance = 0.0f;
					audioParams.maxDistance = audioRadius;
					audioParams.RandomizePitch(audioDef.minPitch, audioDef.maxPitch);
					audioParams.Play();
				}
			}
		}
#endif // !DEDICATED_SERVER

		private void playGunshot()
		{
			AudioClip clip = equippedGunAsset.shoot;
			float volume = 1.0f;
			float maxDistance = equippedGunAsset.gunshotRolloffDistance;
			if (thirdAttachments.barrelAsset != null && player.equipment.state[16] > 0)
			{
				if (thirdAttachments.barrelAsset.shoot != null)
				{
					clip = thirdAttachments.barrelAsset.shoot;
				}
				volume *= thirdAttachments.barrelAsset.volume;
				maxDistance *= thirdAttachments.barrelAsset.gunshotRolloffDistanceMultiplier;
			}
			gunshotAudioSource.clip = clip;
			gunshotAudioSource.volume = volume;
			gunshotAudioSource.maxDistance = maxDistance;

			gunshotAudioSource.pitch = Random.Range(0.975f, 1.025f);

			// Clip can be null for some projectile guns like rocket launcher.
			if (gunshotAudioSource.clip != null)
			{
				// Does not use AudioSourcePool because gunshot has custom rolloff curve.
				gunshotAudioSource.PlayOneShot(gunshotAudioSource.clip);
			}
		}

		private void EjectCasingAfterShooting()
		{
			if (firstShellEmitter != null && player.look.perspective == EPlayerPerspective.FIRST && !equippedGunAsset.isTurret)
			{
				firstShellEmitter.Emit(1);
			}

			if (thirdShellEmitter != null)
			{
				thirdShellEmitter.Emit(1);
			}
		}

		private void shoot()
		{
			if (gunshotAudioSource != null)
			{
				playGunshot();
			}

			if (equippedGunAsset.ShouldEjectCasingAfterShooting)
			{
				EjectCasingAfterShooting();
			}

			if (thirdAttachments.barrelModel == null || !thirdAttachments.barrelAsset.isBraked || player.equipment.state[16] == 0)
			{
				if (firstMuzzleEmitter != null && player.look.perspective == EPlayerPerspective.FIRST && !equippedGunAsset.isTurret)
				{
					firstMuzzleEmitter.Emit(1);

					firstMuzzleEmitter.GetComponent<Light>().enabled = true;
					if (firstFakeLight != null)
					{
						firstFakeLight.GetComponent<Light>().enabled = true;
					}
				}

				if (thirdMuzzleEmitter != null && (!channel.IsLocalPlayer || player.look.perspective == EPlayerPerspective.THIRD || equippedGunAsset.isTurret))
				{
					thirdMuzzleEmitter.Emit(1);

					thirdMuzzleEmitter.GetComponent<Light>().enabled = true;
				}
			}

			if (!channel.IsLocalPlayer)
			{
				if (equippedGunAsset.range < 32)
				{
					trace(player.look.aim.position + (player.look.aim.forward * 32), player.look.aim.forward);
				}
				else
				{
					trace(player.look.aim.position + (player.look.aim.forward * Random.Range(32.0f, Mathf.Min(64.0f, equippedGunAsset.range))), player.look.aim.forward);
				}

#if !DEDICATED_SERVER
				PlayFlybyAudio(player.look.aim.position, player.look.aim.forward, equippedGunAsset.range);
#endif // !DEDICATED_SERVER
			}

			lastShot = Time.realtimeSinceStartup;

			// Note: be careful adjusting this because it sets `needsRechamber = true` and the shot count should only
			// be incremented by one for listen server / dedicated server.
			IncrementShotCountForRechamber();

			if (thirdAttachments.barrelAsset != null && thirdAttachments.barrelAsset.durability > 0)
			{
				if (thirdAttachments.barrelAsset.durability > player.equipment.state[16])
				{
					player.equipment.state[16] = 0;
				}
				else
				{
					player.equipment.state[16] -= thirdAttachments.barrelAsset.durability;
				}

				if (channel.IsLocalPlayer || Provider.isServer)
				{
					player.equipment.updateState();
				}
			}

			if (isAiming && equippedGunAsset.ShouldForceStopAimingAfterShooting)
			{
				isAiming = false;
				stopAim();
			}

			InvokeModHookShotFiredEvents();
		}

		[System.Obsolete]
		public void askShoot(CSteamID steamID)
		{
			ReceivePlayShoot();
		}

		private static readonly ClientInstanceMethod SendPlayShoot = ClientInstanceMethod.Get(typeof(UseableGun), nameof(ReceivePlayShoot));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askShoot))]
		public void ReceivePlayShoot()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				shoot();
			}
		}

		/// <summary>
		/// Called on server and owning client.
		/// </summary>
		private void fire()
		{
			float quality = player.equipment.quality / 100f;

			if (!equippedGunAsset.infiniteAmmo)
			{
				if (ammo >= equippedGunAsset.ammoPerShot)
				{
					ammo -= equippedGunAsset.ammoPerShot;

					if (equippedGunAsset.action != EAction.String)
					{
						player.equipment.state[10] = ammo;
						player.equipment.updateState();
					}
				}
				else
				{
					throw new System.Exception("Insufficient ammo");
				}
			}

			if (channel.IsLocalPlayer && ammo < equippedGunAsset.ammoPerShot)
			{
				PlayerUI.message(EPlayerMessage.RELOAD, "");
			}

			if (!isAiming)
			{
				player.equipment.uninspect();
			}

			if (Provider.isServer)
			{
				SendPlayShoot.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsWithinSphereExcludingOwner(transform.position, EffectManager.INSANE));

				lastShot = Time.realtimeSinceStartup;

				if (!channel.IsLocalPlayer)
				{
					// If locally controlled it will be invoked later as part of calling Shoot().
					// The server doesn't invoke Shoot locally, only on clients.
					InvokeModHookShotFiredEvents();
				}

				// Note: be VERY careful adjusting this because it sets `needsRechamber = true` and the shot count
				// should only be incremented by one for listen server / dedicated server. It's incremented by shoot()
				// and project(), so we only call it here if shoot() or project() WON'T be called (e.g., by
				// SendPlayShoot or SendPlayProject).
				bool willCallShootLocally = channel.IsLocalPlayer && equippedGunAsset.projectile == null;
				bool willCallProjectLocally = equippedGunAsset.projectile != null;
				if (!willCallShootLocally && !willCallProjectLocally)
				{
					IncrementShotCountForRechamber();
				}

				if (thirdAttachments.barrelAsset == null || !thirdAttachments.barrelAsset.isSilenced || player.equipment.state[16] == 0)
				{
					AlertTool.alert(transform.position, equippedGunAsset.alertRadius);
				}

				if (Provider.modeConfigData.Items.ShouldWeaponTakeDamage && player.equipment.quality > 0 && Random.value < ((ItemWeaponAsset) player.equipment.asset).durability)
				{
					if (player.equipment.quality > ((ItemWeaponAsset) player.equipment.asset).wear)
					{
						player.equipment.quality -= ((ItemWeaponAsset) player.equipment.asset).wear;
					}
					else
					{
						player.equipment.quality = 0;
					}

					player.equipment.sendUpdateQuality();
				}
			}

			//float quality = (player.equipment.quality < 50 ? 1f + (1f - player.equipment.quality / 50f) : 1f);

			if (channel.IsLocalPlayer)
			{
				if (!player.look.IsLocallyUsingFreecam)
				{
					if (player.look.perspective == EPlayerPerspective.THIRD)
					{
						RaycastHit target;
						Physics.Raycast(new Ray(MainCamera.instance.transform.position, MainCamera.instance.transform.forward), out target, 512, RayMasks.DAMAGE_CLIENT);

						if (target.transform != null)
						{
							if (Vector3.Dot(target.point - player.look.aim.position, MainCamera.instance.transform.forward) > 0.0f)
							{
								player.look.aim.rotation = Quaternion.LookRotation(target.point - player.look.aim.position);
							}
						}
						else
						{
							player.look.aim.rotation = Quaternion.LookRotation(MainCamera.instance.transform.position + (MainCamera.instance.transform.forward * 512.0f) - player.look.aim.position);
						}
					}
				}

				if (equippedGunAsset.projectile == null)
				{
					// This code is only invoked on client.
					Quaternion aimRotation = player.look.aim.rotation;
					if (player.look.perspective == EPlayerPerspective.FIRST)
					{
						Quaternion viewmodelOffset = Quaternion.Euler(player.animator.recoilViewmodelCameraRotation.currentPosition);
						aimRotation *= viewmodelOffset;
					}

					float spread = CalculateSpreadAngleRadians(quality, GetSimulationAimAlpha());
#if DISABLE_SPREAD
					spread = 0.0f;
#endif // DISABLE_SPREAD

					byte pellets = thirdAttachments.magazineAsset != null ? thirdAttachments.magazineAsset.pellets : (byte) 1;
					float bulletGravityMultiplier = CalculateBulletGravityMultiplier();
					for (byte pellet = 0; pellet < pellets; pellet++)
					{
						BulletInfo bullet = new BulletInfo();
						bullet.origin = player.look.aim.position;
						bullet.position = bullet.origin;

						Vector3 direction = aimRotation * RandomEx.GetRandomForwardVectorInCone(spread);
						bullet.velocity = direction * equippedGunAsset.muzzleVelocity;

						bullet.pellet = pellet;
						bullet.quality = quality;
						bullet.barrelAsset = thirdAttachments.barrelAsset;
						bullet.magazineAsset = thirdAttachments.magazineAsset;
						bullet.gravityMultiplier = bulletGravityMultiplier;
						bullets.Add(bullet);

						int data;
						if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Shot", out data))
						{
							Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Shot", data + 1);
						}
					}
				}
				else
				{
					Vector3 direction = player.look.aim.forward;

					Ray ray = new Ray(player.look.aim.position, direction);
					RaycastInfo info = DamageTool.raycast(ray, 512f, RayMasks.DAMAGE_CLIENT, ignorePlayer: player);

					if (info.transform != null)
					{
						player.input.sendRaycast(info, ERaycastInfoUsage.Gun);
					}

					Vector3 origin = player.look.aim.position;
					RaycastHit hit;
					if (!Physics.Raycast(new Ray(origin, direction), out hit, 1.0f, RayMasks.DAMAGE_SERVER))
					{
						origin += direction;
					}

					project(origin, direction, thirdAttachments.barrelAsset, thirdAttachments.magazineAsset);

					int data;
					if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Shot", out data))
					{
						Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Shot", data + 1);
					}
				}

				float recoil_x = Random.Range(equippedGunAsset.recoilMin_x, equippedGunAsset.recoilMax_x) * (quality < 0.5f ? 1f + (1f - (quality * 2f)) : 1f);
				float recoil_y = Random.Range(equippedGunAsset.recoilMin_y, equippedGunAsset.recoilMax_y) * (quality < 0.5f ? 1f + (1f - (quality * 2f)) : 1f);

				float shake_x = Random.Range(equippedGunAsset.shakeMin_x, equippedGunAsset.shakeMax_x);
				float shake_y = Random.Range(equippedGunAsset.shakeMin_y, equippedGunAsset.shakeMax_y);
				float shake_z = Random.Range(equippedGunAsset.shakeMin_z, equippedGunAsset.shakeMax_z);

				float skillMultiplier = player.skills.GetSharpshooterRecoilMultiplier();
				recoil_x *= skillMultiplier;
				recoil_y *= skillMultiplier;

				if (isAiming)
				{
					recoil_x *= equippedGunAsset.aimingRecoilMultiplier;
					recoil_y *= equippedGunAsset.aimingRecoilMultiplier;
				}

				if (thirdAttachments.sightAsset != null)
				{
					if (isAiming)
					{
						recoil_x *= thirdAttachments.sightAsset.aimingRecoilMultiplier;
						recoil_y *= thirdAttachments.sightAsset.aimingRecoilMultiplier;
					}

					recoil_x *= thirdAttachments.sightAsset.recoil_x;
					recoil_y *= thirdAttachments.sightAsset.recoil_y;

					shake_x *= thirdAttachments.sightAsset.shake;
					shake_y *= thirdAttachments.sightAsset.shake;
					shake_z *= thirdAttachments.sightAsset.shake;
				}

				if (thirdAttachments.tacticalAsset != null && shouldEnableTacticalStats)
				{
					if (isAiming)
					{
						recoil_x *= thirdAttachments.tacticalAsset.aimingRecoilMultiplier;
						recoil_y *= thirdAttachments.tacticalAsset.aimingRecoilMultiplier;
					}

					recoil_x *= thirdAttachments.tacticalAsset.recoil_x;
					recoil_y *= thirdAttachments.tacticalAsset.recoil_y;

					shake_x *= thirdAttachments.tacticalAsset.shake;
					shake_y *= thirdAttachments.tacticalAsset.shake;
					shake_z *= thirdAttachments.tacticalAsset.shake;
				}

				if (thirdAttachments.gripAsset != null && (!thirdAttachments.gripAsset.ShouldOnlyAffectAimWhileProne || player.stance.stance == EPlayerStance.PRONE))
				{
					if (isAiming)
					{
						recoil_x *= thirdAttachments.gripAsset.aimingRecoilMultiplier;
						recoil_y *= thirdAttachments.gripAsset.aimingRecoilMultiplier;
					}

					recoil_x *= thirdAttachments.gripAsset.recoil_x;
					recoil_y *= thirdAttachments.gripAsset.recoil_y;

					shake_x *= thirdAttachments.gripAsset.shake;
					shake_y *= thirdAttachments.gripAsset.shake;
					shake_z *= thirdAttachments.gripAsset.shake;
				}

				if (thirdAttachments.barrelAsset != null)
				{
					if (isAiming)
					{
						recoil_x *= thirdAttachments.barrelAsset.aimingRecoilMultiplier;
						recoil_y *= thirdAttachments.barrelAsset.aimingRecoilMultiplier;
					}

					recoil_x *= thirdAttachments.barrelAsset.recoil_x;
					recoil_y *= thirdAttachments.barrelAsset.recoil_y;

					shake_x *= thirdAttachments.barrelAsset.shake;
					shake_y *= thirdAttachments.barrelAsset.shake;
					shake_z *= thirdAttachments.barrelAsset.shake;
				}

				if (thirdAttachments.magazineAsset != null)
				{
					if (isAiming)
					{
						recoil_x *= thirdAttachments.magazineAsset.aimingRecoilMultiplier;
						recoil_y *= thirdAttachments.magazineAsset.aimingRecoilMultiplier;
					}

					recoil_x *= thirdAttachments.magazineAsset.recoil_x;
					recoil_y *= thirdAttachments.magazineAsset.recoil_y;

					shake_x *= thirdAttachments.magazineAsset.shake;
					shake_y *= thirdAttachments.magazineAsset.shake;
					shake_z *= thirdAttachments.magazineAsset.shake;
				}

				applyRecoilMagnitudeModifiers(ref recoil_x);
				applyRecoilMagnitudeModifiers(ref recoil_y);

				if (player.stance.stance == EPlayerStance.CROUCH)
				{
					shake_x *= SHAKE_CROUCH;
					shake_y *= SHAKE_CROUCH;
					shake_z *= SHAKE_CROUCH;
				}
				else if (player.stance.stance == EPlayerStance.PRONE)
				{
					shake_x *= SHAKE_PRONE;
					shake_y *= SHAKE_PRONE;
					shake_z *= SHAKE_PRONE;
				}

				if (player.look.perspective == EPlayerPerspective.FIRST)
				{
					float scale = Provider.modeConfigData.Gameplay.FirstPerson_RecoilMultiplier;
					if (isAiming)
					{
						scale *= Provider.modeConfigData.Gameplay.FirstPerson_AimingRecoilMultiplier;
						if (player.look.isScopeActive && player.look.scopeCameraZoomFactor > 1.0001f)
						{
							float weight = Provider.modeConfigData.Gameplay.FirstPerson_AimingZoomRecoilReduction;
							float maxZoomReduction = 1.0f / player.look.scopeCameraZoomFactor;
							scale *= Mathf.Lerp(1.0f, maxZoomReduction, weight);
						}
					}
					recoil_x *= scale;
					recoil_y *= scale;
					shake_x *= scale;
					shake_y *= scale;
					shake_z *= scale;
				}
				else if (player.look.perspective == EPlayerPerspective.THIRD)
				{
					recoil_x *= Provider.modeConfigData.Gameplay.ThirdPerson_RecoilMultiplier;
					recoil_y *= Provider.modeConfigData.Gameplay.ThirdPerson_RecoilMultiplier;
				}

#if !DISABLE_MAIN_CAMERA_RECOIL
				player.look.recoil(recoil_x, recoil_y, equippedGunAsset.recover_x, equippedGunAsset.recover_y);
#endif // !DISABLE_MAIN_CAMERA_RECOIL
				player.animator.AddRecoilViewmodelCameraOffset(shake_x, shake_y, shake_z);
				player.animator.AddRecoilViewmodelCameraRotation(recoil_x, recoil_y);

				updateInfo();

				if (equippedGunAsset.projectile == null)
				{
					shoot();
				}
			}

			if (Provider.isServer)
			{
				if (!channel.IsLocalPlayer)
				{
					if (thirdAttachments.barrelAsset != null && thirdAttachments.barrelAsset.durability > 0)
					{
						if (thirdAttachments.barrelAsset.durability > player.equipment.state[16])
						{
							player.equipment.state[16] = 0;
						}
						else
						{
							player.equipment.state[16] -= thirdAttachments.barrelAsset.durability;
						}

						player.equipment.updateState();
					}
				}

				equippedGunAsset.GrantShootQuestRewards(player);

				if (equippedGunAsset.projectile == null)
				{
					bool shouldDeleteMagazine = ammo == 0 && equippedGunAsset.shouldDeleteEmptyMagazines;

					byte pellets = thirdAttachments.magazineAsset != null ? thirdAttachments.magazineAsset.pellets : (byte) 1;
					float bulletGravityMultiplier = CalculateBulletGravityMultiplier();
					for (byte pellet = 0; pellet < pellets; pellet++)
					{
						BulletInfo bullet;

						if (channel.IsLocalPlayer)
						{
							bullet = bullets[bullets.Count - pellets + pellet];
						}
						else
						{
							bullet = new BulletInfo();
							bullet.origin = player.look.aim.position;
							bullet.ApproximatePlayerAimDirection = player.look.aim.forward;
							bullet.position = bullet.origin;
							bullet.pellet = pellet;
							bullet.quality = quality;
							bullet.barrelAsset = thirdAttachments.barrelAsset;
							bullet.magazineAsset = thirdAttachments.magazineAsset;
							bullet.gravityMultiplier = bulletGravityMultiplier;
							bullets.Add(bullet);

							onBulletSpawned?.Invoke(this, bullet);
						}

						if (thirdAttachments.magazineAsset != null && thirdAttachments.magazineAsset.isExplosive)
						{
							if (equippedGunAsset.action == EAction.String)
							{
								shouldDeleteMagazine = true;
							}
						}
						else if (equippedGunAsset.action == EAction.String)
						{
							if (player.equipment.state[GunStateIndices.MAGAZINE_QUALITY] > 0)
							{
								byte stuckCost = thirdAttachments.magazineAsset != null ? thirdAttachments.magazineAsset.stuck : (byte) 1;
								if (player.equipment.state[GunStateIndices.MAGAZINE_QUALITY] > stuckCost)
								{
									player.equipment.state[GunStateIndices.MAGAZINE_QUALITY] -= stuckCost;
								}
								else
								{
									player.equipment.state[GunStateIndices.MAGAZINE_QUALITY] = 0;
								}

								bullet.dropID = thirdAttachments.magazineID;
								bullet.dropAmount = player.equipment.state[GunStateIndices.AMMO];
								bullet.dropQuality = player.equipment.state[GunStateIndices.MAGAZINE_QUALITY];
							}

							shouldDeleteMagazine = true;
						}
					}

					if (shouldDeleteMagazine)
					{
						player.equipment.state[GunStateIndices.MAGAZINE_ID] = 0;
						player.equipment.state[GunStateIndices.MAGAZINE_ID + 1] = 0;
						player.equipment.state[GunStateIndices.AMMO] = 0;

						player.equipment.sendUpdateState();
					}
				}
				else
				{
					// Save attachments before potentially deleting.
					ItemBarrelAsset projectileBarrelAsset = player.equipment.state[GunStateIndices.BARREL_QUALITY] > 0 ? thirdAttachments.barrelAsset : null;
					ItemMagazineAsset projectileMagazineAsset = thirdAttachments.magazineAsset;

					if (player.input.hasInputs())
					{
						InputInfo info = player.input.getInput(false, ERaycastInfoUsage.Gun);

						if (info != null && info.transform != null)
						{
							player.look.aim.LookAt(info.point); // don't use pitch/yaw because third person redirections point
						}
					}

					if (ammo == 0 && equippedGunAsset.shouldDeleteEmptyMagazines)
					{
						player.equipment.state[GunStateIndices.MAGAZINE_ID] = 0;
						player.equipment.state[GunStateIndices.MAGAZINE_ID + 1] = 0;
						player.equipment.state[GunStateIndices.AMMO] = 0;

						player.equipment.sendUpdateState();
					}

					if (!channel.IsLocalPlayer)
					{
						Vector3 origin = player.look.aim.position;
						Vector3 direction = player.look.aim.forward;
						RaycastHit hit;
						if (!Physics.Raycast(new Ray(origin, direction), out hit, 1.0f, RayMasks.DAMAGE_SERVER))
						{
							origin += direction;
						}

						project(origin, direction, projectileBarrelAsset, projectileMagazineAsset);

						SendPlayProject.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner(),
							origin,
							direction,
							projectileBarrelAsset != null ? projectileBarrelAsset.id : (ushort) 0,
							projectileMagazineAsset != null ? projectileMagazineAsset.id : (ushort) 0);
					}

					player.life.markAggressive(false);
				}
			}

			if (isAiming && equippedGunAsset.ShouldForceStopAimingAfterShooting)
			{
				isAiming = false;
				stopAim();
			}

			if (equippedGunAsset.canEverJam)
			{
				// Only server can predict jams, but we reset ammo on the client to avoid desync.
				if (Provider.isServer)
				{
					if (quality < equippedGunAsset.jamQualityThreshold)
					{
						float jamAlpha = 1.0f - (quality / equippedGunAsset.jamQualityThreshold);
						float jamChance = Mathf.Lerp(0.0f, equippedGunAsset.jamMaxChance, jamAlpha);
						if (Random.value < jamChance)
						{
							// Reliable so that client doesn't keep firing by mistake.
							SendPlayChamberJammed.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), ammo);
						}
					}
				}
			}
		}

		private static MasterBundleReference<AudioClip> jabClipRef = new MasterBundleReference<AudioClip>("core.masterbundle", "Sounds/MeleeAttack_01.mp3");

		private void jab()
		{
			if (Provider.isServer)
			{
				AlertTool.alert(transform.position, 8);
			}

			ItemTacticalAssetMeleeProperties meleeProperties = thirdAttachments.tacticalAsset?.MeleeProperties;
			float meleeRange = meleeProperties?.MeleeRange ?? 2.0f;

			if (channel.IsLocalPlayer)
			{
				AudioClip attackClip = jabClipRef.loadAsset();
				if (attackClip == null)
				{
					UnturnedLog.warn("Missing built-in bayonet audio");
				}

				player.animator.AddBayonetViewmodelCameraOffset(0.0f, 0.0f, 0.8f);
				player.playSound(attackClip, 0.5f);

				int data;
				if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Shot", out data))
				{
					Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Shot", data + 1);
				}

				Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
				RaycastInfo info = DamageTool.raycast(ray, meleeRange, RayMasks.DAMAGE_CLIENT, ignorePlayer: player);

				if (info.player != null && DamageTool.isPlayerAllowedToDamagePlayer(player, info.player))
				{
					if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Hit", out data))
					{
						Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Hit", data + 1);
					}

					if (info.limb == ELimb.SKULL)
					{
						if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Headshots", out data))
						{
							Provider.provider.statisticsService.userStatisticsService.setStatistic("Headshots", data + 1);
						}
					}

					PlayerUI.hitmark(info.point, false, info.limb == ELimb.SKULL ? EPlayerHit.CRITICAL : EPlayerHit.ENTITIY);
				}
				else if (info.zombie != null || info.animal != null)
				{
					if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Hit", out data))
					{
						Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Hit", data + 1);
					}

					if (info.limb == ELimb.SKULL)
					{
						if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Headshots", out data))
						{
							Provider.provider.statisticsService.userStatisticsService.setStatistic("Headshots", data + 1);
						}
					}

					PlayerUI.hitmark(info.point, false, info.limb == ELimb.SKULL ? EPlayerHit.CRITICAL : EPlayerHit.ENTITIY);
				}

				player.input.sendRaycast(info, ERaycastInfoUsage.Bayonet);
			}

			if (Provider.isServer)
			{
				if (!player.input.hasInputs())
				{
					return;
				}

				InputInfo info = player.input.getInput(true, ERaycastInfoUsage.Bayonet);

				if (info == null)
				{
					return;
				}

				if ((info.point - player.look.aim.position).sqrMagnitude > MathfEx.Square(meleeRange + 4))
				{
					return;
				}

				if (!string.IsNullOrEmpty(info.materialName))
				{
					DamageTool.ServerSpawnLegacyImpact(info.point,
						info.normal,
						info.materialName,
						info.colliderTransform,
						channel.GatherOwnerAndClientConnectionsWithinSphere(info.point, EffectManager.SMALL));
				}

				EPlayerKill kill = EPlayerKill.NONE;
				uint xp = 0;

				float times = 1;
				times *= 1f + (channel.owner.player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.OVERKILL) * 0.5f);

				ERagdollEffect ragdollEffect = player.equipment.getUseableRagdollEffect();

				if (info.type == ERaycastInfoType.PLAYER)
				{
					if (info.player != null && meleeProperties?.MeleePlayerDamageMultiplier != null)
					{
						if (DamageTool.isPlayerAllowedToDamagePlayer(player, info.player))
						{
							IDamageMultiplier multiplier = meleeProperties.MeleePlayerDamageMultiplier;
							DamagePlayerParameters parameters = DamagePlayerParameters.make(info.player, EDeathCause.MELEE, info.direction, multiplier, info.limb);
							parameters.killer = channel.owner.playerID.steamID;
							parameters.times = times;
							parameters.respectArmor = true;
							parameters.trackKill = true;
							parameters.ragdollEffect = ragdollEffect;
							meleeProperties?.InitPlayerDamageParameters(ref parameters);

							if (player.input.IsUnderFakeLagPenalty)
							{
								parameters.times *= Provider.configData.Server.Fake_Lag_Damage_Penalty_Multiplier;
							}

							DamageTool.damagePlayer(parameters, out kill);
						}
					}
				}
				else if (info.type == ERaycastInfoType.ZOMBIE)
				{
					if (info.zombie != null && meleeProperties?.MeleeZombieOrPlayerDamageMultiplier != null)
					{
						IDamageMultiplier multiplier = meleeProperties.MeleeZombieOrPlayerDamageMultiplier;
						DamageZombieParameters parameters = DamageZombieParameters.make(info.zombie, info.direction, multiplier, info.limb);
						parameters.times = times;
						parameters.allowBackstab = true;
						parameters.respectArmor = true;
						parameters.instigator = player;
						parameters.zombieStunOverride = meleeProperties?.MeleeZombieStunOverride ?? EZombieStunOverride.None;
						parameters.ragdollEffect = ragdollEffect;
						parameters.RagdollForceMultiplier = meleeProperties?.MeleeZombieRagdollForceMultiplier ?? 1f;

						if (player.movement.nav != 255)
						{
							parameters.AlertPosition = transform.position;
						}

						DamageTool.damageZombie(parameters, out kill, out xp);
					}
				}
				else if (info.type == ERaycastInfoType.ANIMAL)
				{
					if (info.animal != null && meleeProperties?.MeleeAnimalOrPlayerDamageMultiplier != null)
					{
						IDamageMultiplier multiplier = meleeProperties.MeleeAnimalOrPlayerDamageMultiplier;
						DamageAnimalParameters parameters = DamageAnimalParameters.make(info.animal, info.direction, multiplier, info.limb);
						parameters.times = times;
						parameters.instigator = player;
						parameters.ragdollEffect = ragdollEffect;
						parameters.AlertPosition = transform.position;

						DamageTool.damageAnimal(parameters, out kill, out xp);
					}
				}

				// only do aggressor check if we didn't shoot a player (because we would already be marked aggressor) and if we weren't saving them from a zombie
				if (info.type != ERaycastInfoType.PLAYER && info.type != ERaycastInfoType.ZOMBIE && info.type != ERaycastInfoType.ANIMAL)
				{
					if (!player.life.isAggressor)
					{
						float bulletRange = meleeRange + Provider.modeConfigData.Players.Ray_Aggressor_Distance;
						bulletRange *= bulletRange;
						float rayAggressor = Provider.modeConfigData.Players.Ray_Aggressor_Distance;
						rayAggressor *= rayAggressor;

						Vector3 bulletNorm = player.look.aim.forward;

						for (int enemyIndex = 0; enemyIndex < Provider.clients.Count; enemyIndex++)
						{
							if (Provider.clients[enemyIndex] == channel.owner)
							{
								continue;
							}

							Player enemy = Provider.clients[enemyIndex].player;

							if (enemy == null)
							{
								continue;
							}

							Vector3 enemyOffset = enemy.look.aim.position - player.look.aim.position;
							Vector3 bulletProj = Vector3.Project(enemyOffset, bulletNorm);

							if (bulletProj.sqrMagnitude < bulletRange && (bulletProj - enemyOffset).sqrMagnitude < rayAggressor) // shot within 4 meters of enemy
							{
								player.life.markAggressive(false);
							}
						}
					}
				}

				if (Level.info.type == ELevelType.HORDE)
				{
					if (info.zombie != null)
					{
						if (info.limb == ELimb.SKULL)
						{
							player.skills.askPay(10);
						}
						else
						{
							player.skills.askPay(5);
						}
					}

					if (kill == EPlayerKill.ZOMBIE)
					{
						if (info.limb == ELimb.SKULL)
						{
							player.skills.askPay(50);
						}
						else
						{
							player.skills.askPay(25);
						}
					}
				}
				else
				{
					if (kill == EPlayerKill.PLAYER)
					{
						if (Level.info.type == ELevelType.ARENA)
						{
							player.skills.askPay(100);
						}
					}

					player.sendStat(kill);

					if (xp > 0)
					{
						player.skills.askPay(xp);
					}
				}
			}
		}

		/// <summary>
		/// Calculate damage multiplier for individual bullet.
		/// </summary>
		private float getBulletDamageMultiplier(ref BulletInfo bullet)
		{
			float damageMultiplier = bullet.quality < 0.5f ? (0.5f + bullet.quality) : 1f;

			// Magazine asset is stored per-bullet for cases like reloading explosive crossbow.
			if (bullet.magazineAsset != null)
			{
				damageMultiplier *= bullet.magazineAsset.ballisticDamageMultiplier;
			}

			// For non-magazine attachments chances are that they didn't change during shot.
			if (thirdAttachments.sightAsset != null)
			{
				damageMultiplier *= thirdAttachments.sightAsset.ballisticDamageMultiplier;
			}

			if (thirdAttachments.tacticalAsset != null && shouldEnableTacticalStats)
			{
				damageMultiplier *= thirdAttachments.tacticalAsset.ballisticDamageMultiplier;
			}

			if (thirdAttachments.barrelAsset != null)
			{
				damageMultiplier *= thirdAttachments.barrelAsset.ballisticDamageMultiplier;
			}

			if (thirdAttachments.gripAsset != null)
			{
				damageMultiplier *= thirdAttachments.gripAsset.ballisticDamageMultiplier;
			}

			return damageMultiplier;
		}

		internal const float BALLISTICS_DELTA_TIME = 0.02f; // 1.0f / PlayerInput.TOCK_PER_SECOND;

		private void ballistics()
		{
			if (equippedGunAsset.projectile != null || bullets == null)
			{
				return;
			}

			if (channel.IsLocalPlayer)
			{
				for (int bulletIndex = 0; bulletIndex < bullets.Count; bulletIndex++)
				{
					BulletInfo bullet = bullets[bulletIndex];
					byte pellets = bullet.magazineAsset != null ? bullet.magazineAsset.pellets : (byte) 1;

					if (channel.IsLocalPlayer)
					{
						EPlayerHit hit = EPlayerHit.NONE;

						// Pellet index is used to override the previous shotgun hitmarker
						// (if any) to reduce screen clutter with shotguns.
						int hitmarkerIndex = bullet.pellet;

						Ray ray = new Ray(bullet.position, bullet.velocity);
						float travelLength = Provider.modeConfigData.Gameplay.Ballistics ? bullet.velocity.magnitude * BALLISTICS_DELTA_TIME : equippedGunAsset.range;
						RaycastInfo info = DamageTool.raycast(ray, travelLength, RayMasks.DAMAGE_CLIENT, ignorePlayer: player);
						float totalDistanceTraveled = Vector3.Distance(bullet.origin, info.point);

#if WITH_BALLISTIC_TRAJECTORY_GIZMOS
						RuntimeGizmos.Get().Line(ray.origin, ray.origin + ray.direction * travelLength, Color.red, lifespan: 30.0f);
#endif // WITH_BALLISTIC_TRAJECTORY_GIZMOS

						if (info.player != null && equippedGunAsset.playerDamageMultiplier.damage > 1 && (DamageTool.isPlayerAllowedToDamagePlayer(player, info.player) || equippedGunAsset.bypassAllowedToDamagePlayer))
						{
							if (hit != EPlayerHit.CRITICAL)
							{
								hit = info.limb == ELimb.SKULL ? EPlayerHit.CRITICAL : EPlayerHit.ENTITIY;
							}

							PlayerUI.hitmark(info.point, pellets > 1, info.limb == ELimb.SKULL ? EPlayerHit.CRITICAL : EPlayerHit.ENTITIY);
						}
						else if (info.zombie != null && equippedGunAsset.zombieDamageMultiplier.damage > 1)
						{
							EPlayerHit shownHit = info.limb == ELimb.SKULL ? EPlayerHit.CRITICAL : EPlayerHit.ENTITIY;
							if (info.zombie.getBulletResistance() < 0.2f)
							{
								shownHit = EPlayerHit.GHOST;
							}

							if (hit != EPlayerHit.CRITICAL)
							{
								hit = shownHit;
							}

							PlayerUI.hitmark(info.point, pellets > 1, shownHit);
						}
						else if (info.animal != null && equippedGunAsset.animalDamageMultiplier.damage > 1)
						{
							if (info.animal.asset.DoesArmorFalloffShowHitmarker(totalDistanceTraveled))
							{
								if (hit != EPlayerHit.CRITICAL)
								{
									hit = info.limb == ELimb.SKULL ? EPlayerHit.CRITICAL : EPlayerHit.ENTITIY;
								}

								PlayerUI.hitmark(info.point, pellets > 1, info.limb == ELimb.SKULL ? EPlayerHit.CRITICAL : EPlayerHit.ENTITIY);
							}
						}
						else if (info.transform != null && info.transform.CompareTag("Barricade") && equippedGunAsset.barricadeDamage > 1)
						{
							BarricadeDrop barricade = BarricadeDrop.FindByRootFast(info.transform);
							if (barricade != null)
							{
								ItemBarricadeAsset asset = barricade.asset;
								if (asset != null && asset.canBeDamaged && (asset.isVulnerable || CanDamageInvulnerableEntities))
								{
									if (asset.DoesArmorFalloffShowHitmarker(totalDistanceTraveled))
									{
										if (hit == EPlayerHit.NONE)
										{
											hit = EPlayerHit.BUILD;
										}

										PlayerUI.hitmark(info.point, pellets > 1, EPlayerHit.BUILD);
									}
								}
							}
						}
						else if (info.transform != null && info.transform.CompareTag("Structure") && equippedGunAsset.structureDamage > 1)
						{
							StructureDrop structure = StructureDrop.FindByRootFast(info.transform);
							if (structure != null)
							{
								ItemStructureAsset asset = structure.asset;
								if (asset != null && asset.canBeDamaged && (asset.isVulnerable || CanDamageInvulnerableEntities))
								{
									if (asset.DoesArmorFalloffShowHitmarker(totalDistanceTraveled))
									{
										if (hit == EPlayerHit.NONE)
										{
											hit = EPlayerHit.BUILD;
										}

										PlayerUI.hitmark(info.point, pellets > 1, EPlayerHit.BUILD);
									}
								}
							}
						}
						else if (info.vehicle != null && !info.vehicle.isDead && equippedGunAsset.vehicleDamage > 1)
						{
							if (info.vehicle.asset != null && info.vehicle.canBeDamaged && (info.vehicle.asset.isVulnerable || CanDamageInvulnerableEntities))
							{
								if (info.vehicle.asset.DoesArmorFalloffShowHitmarker(totalDistanceTraveled))
								{
									if (hit == EPlayerHit.NONE)
									{
										hit = EPlayerHit.BUILD;
									}

									PlayerUI.hitmark(info.point, pellets > 1, EPlayerHit.BUILD);
								}
							}
						}
						else if (info.transform != null && info.transform.CompareTag("Resource") && equippedGunAsset.resourceDamage > 1)
						{
							byte x;
							byte y;
							ushort index;
							if (ResourceManager.tryGetRegion(info.transform, out x, out y, out index))
							{
								ResourceSpawnpoint spawnpoint = ResourceManager.getResourceSpawnpoint(x, y, index);

								if (spawnpoint != null && !spawnpoint.isDead && equippedGunAsset.hasBladeID(spawnpoint.asset.bladeID))
								{
									if (spawnpoint.asset.DoesArmorFalloffShowHitmarker(totalDistanceTraveled))
									{
										if (hit == EPlayerHit.NONE)
										{
											hit = EPlayerHit.BUILD;
										}

										PlayerUI.hitmark(info.point, pellets > 1, EPlayerHit.BUILD);
									}
								}
							}
						}
						else if (info.transform != null && equippedGunAsset.objectDamage > 1)
						{
							InteractableObjectRubble rubble = info.transform.GetComponentInParent<InteractableObjectRubble>();
							if (rubble != null)
							{
								info.transform = rubble.transform;
								info.section = rubble.getSection(info.collider.transform);
								if (rubble.IsSectionIndexValid(info.section) && !rubble.isSectionDead(info.section) && equippedGunAsset.hasBladeID(rubble.asset.rubbleBladeID))
								{
									if (rubble.asset.rubbleIsVulnerable || CanDamageInvulnerableEntities)
									{
										if (rubble.asset.DoesArmorFalloffShowHitmarker(totalDistanceTraveled))
										{
											if (hit == EPlayerHit.NONE)
											{
												hit = EPlayerHit.BUILD;
											}

											PlayerUI.hitmark(info.point, pellets > 1, EPlayerHit.BUILD);
										}
									}
								}
							}
						}

						//if(bullet.steps == 0)
						//{
						//	if(player.look.perspective == EPlayerPerspective.FIRST)
						//	{
						//		if(firstAttachments.barrelHook != null)
						//		{
						//			ray.origin = firstAttachments.barrelHook.position;
						//		}
						//	}
						//	else
						//	{
						//		if(thirdAttachments.barrelHook != null)
						//		{
						//			ray.origin = thirdAttachments.barrelHook.position;
						//		}
						//	}
						//}

						if (Provider.modeConfigData.Gameplay.Ballistics)
						{
							if (bullet.steps > 0 || equippedGunAsset.ballisticSteps <= 1)
							{
								Vector3 bulletDirection = bullet.GetDirection();
								if (equippedGunAsset.ballisticTravel < 32.0f)
								{
									trace(bullet.position + (bulletDirection * 32.0f), bulletDirection);
								}
								else
								{
									trace(bullet.position + (bulletDirection * Random.Range(32.0f, equippedGunAsset.ballisticTravel)), bulletDirection);
								}

#if !DEDICATED_SERVER
								if (pellets < 2)
								{
									PlayFlybyAudio(ray.origin, ray.direction, equippedGunAsset.ballisticTravel);
								}
#endif // !DEDICATED_SERVER
							}
						}
						else
						{
							if (equippedGunAsset.range < 32.0f)
							{
								trace(ray.origin + (ray.direction * 32.0f), ray.direction);
							}
							else
							{
								trace(ray.origin + (ray.direction * Random.Range(32.0f, Mathf.Min(64.0f, equippedGunAsset.range))), ray.direction);
							}

#if !DEDICATED_SERVER
							if (pellets < 2)
							{
								PlayFlybyAudio(ray.origin, ray.direction, equippedGunAsset.range);
							}
#endif // !DEDICATED_SERVER
						}

						if (player.input.isRaycastInvalid(info))
						{
							float acceleration = Physics.gravity.y;
							acceleration *= bullet.gravityMultiplier;

							bullet.position += bullet.velocity * BALLISTICS_DELTA_TIME;
							bullet.velocity = new Vector3(bullet.velocity.x, bullet.velocity.y + acceleration * BALLISTICS_DELTA_TIME, bullet.velocity.z);
						}
						else
						{
							if (hit != EPlayerHit.NONE)
							{
								int data;
								if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Hit", out data))
								{
									Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Hit", data + 1);
								}

								if (hit == EPlayerHit.CRITICAL)
								{
									if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Headshots", out data))
									{
										Provider.provider.statisticsService.userStatisticsService.setStatistic("Headshots", data + 1);
									}
								}
							}

							player.input.sendRaycast(info, ERaycastInfoUsage.Gun);
							bullet.steps = 254;
						}
					}
				}
			}

			if (Provider.isServer)
			{
				while (bullets.Count > 0)
				{
					BulletInfo bullet = bullets[0];
					byte pellets = bullet.magazineAsset != null ? bullet.magazineAsset.pellets : (byte) 1;

					if (!player.input.hasInputs())
					{
						break;
					}

					InputInfo info = player.input.getInput(true, ERaycastInfoUsage.Gun, bullet.origin);

					if (info == null)
					{
						break;
					}

					if (equippedGunAsset == null)
					{
						// Owner might have died as result of previous shot... messy.
						break;
					}

					if (!channel.IsLocalPlayer) // pos is NOT the origin aim point when hosting a listen server
					{
						if (Provider.modeConfigData.Gameplay.Ballistics)
						{
							if ((info.point - bullet.position).magnitude > (equippedGunAsset.ballisticTravel * (bullet.steps + 1 + PlayerInput.SAMPLES)) + 4.0f)
							{
								bullets.RemoveAt(0);
								continue;
							}
						}
						else
						{
							if ((info.point - player.look.aim.position).sqrMagnitude > MathfEx.Square(equippedGunAsset.range + 4))
							{
								break;
							}
						}
					}

					if (onBulletHit != null)
					{
						bool shouldAllow = true;
						onBulletHit(this, bullet, info, ref shouldAllow);
						if (shouldAllow == false)
						{
							bullets.RemoveAt(0);
							continue;
						}
					}

					if (!string.IsNullOrEmpty(info.materialName))
					{
						if (bullet.magazineAsset != null && !bullet.magazineAsset.IsImpactEffectRefNull())
						{
							DamageTool.ServerTriggerImpactEffectForMagazinesV2(bullet.magazineAsset.FindImpactEffectAsset(), info.point, info.normal, channel.owner);
						}
						else
						{
							DamageTool.ServerSpawnBulletImpact(info.point,
								info.normal,
								info.materialName,
								info.colliderTransform,
								channel.owner,
								channel.GatherOwnerAndClientConnectionsWithinSphere(info.point, EffectManager.SMALL));
						}
					}

					EPlayerKill kill = EPlayerKill.NONE;
					uint xp = 0;

					float times = getBulletDamageMultiplier(ref bullet);

					float distanceTraveled = Vector3.Distance(bullet.origin, info.point);
					// 0 within falloff range, 1 beyond falloff end
					float falloffStart = equippedGunAsset.range * equippedGunAsset.damageFalloffRange;
					float falloffEnd = equippedGunAsset.range * equippedGunAsset.damageFalloffMaxRange;
					float falloffAlpha = Mathf.InverseLerp(falloffStart, falloffEnd, distanceTraveled);
					times *= Mathf.Lerp(1.0f, equippedGunAsset.damageFalloffMultiplier, falloffAlpha);

					ERagdollEffect ragdollEffect = player.equipment.getUseableRagdollEffect();

					if (info.type == ERaycastInfoType.PLAYER)
					{
						if (info.player != null)
						{
							if (DamageTool.isPlayerAllowedToDamagePlayer(player, info.player) || equippedGunAsset.bypassAllowedToDamagePlayer)
							{
								bool instakillHeadshot = info.limb == ELimb.SKULL
									&& equippedGunAsset.instakillHeadshots
									&& Provider.modeConfigData.Players.Allow_Instakill_Headshots;

								IDamageMultiplier multiplier = equippedGunAsset.playerDamageMultiplier;
								DamagePlayerParameters parameters = DamagePlayerParameters.make(info.player, EDeathCause.GUN, info.direction * Mathf.Ceil(pellets / 2f), multiplier, info.limb);
								parameters.killer = channel.owner.playerID.steamID;
								parameters.times = times;
								parameters.respectArmor = !instakillHeadshot;
								parameters.trackKill = true;
								parameters.ragdollEffect = ragdollEffect;
								equippedGunAsset.initPlayerDamageParameters(ref parameters);

								if (player.input.IsUnderFakeLagPenalty)
								{
									parameters.times *= Provider.configData.Server.Fake_Lag_Damage_Penalty_Multiplier;
								}

								DamageTool.damagePlayer(parameters, out kill);
							}
						}
					}
					else if (info.type == ERaycastInfoType.ZOMBIE)
					{
						if (info.zombie != null)
						{
							bool instakillHeadshot = info.limb == ELimb.SKULL
								&& equippedGunAsset.instakillHeadshots
								&& Provider.modeConfigData.Zombies.Weapons_Use_Player_Damage
								&& Provider.modeConfigData.Players.Allow_Instakill_Headshots;

							Vector3 paramDir = info.direction * Mathf.Ceil(pellets / 2f);
							IDamageMultiplier multiplier = equippedGunAsset.zombieOrPlayerDamageMultiplier;
							DamageZombieParameters parameters = DamageZombieParameters.make(info.zombie, paramDir, multiplier, info.limb);
							parameters.times = times * info.zombie.getBulletResistance();
							parameters.allowBackstab = false;
							parameters.respectArmor = !instakillHeadshot;
							parameters.instigator = player;
							parameters.ragdollEffect = ragdollEffect;
							parameters.RagdollForceMultiplier = equippedGunAsset.ZombieRagdollForceMultiplier;

							if (player.movement.nav != 255)
							{
								parameters.AlertPosition = transform.position;
							}

							DamageTool.damageZombie(parameters, out kill, out xp);
						}
					}
					else if (info.type == ERaycastInfoType.ANIMAL)
					{
						if (info.animal != null)
						{
							Vector3 paramDir = info.direction * Mathf.Ceil(pellets / 2f);
							IDamageMultiplier multiplier = equippedGunAsset.animalOrPlayerDamageMultiplier;
							DamageAnimalParameters parameters = DamageAnimalParameters.make(info.animal, paramDir, multiplier, info.limb);
							parameters.times = times * info.animal.asset.GetArmorFalloffMultiplier(distanceTraveled);
							parameters.instigator = player;
							parameters.ragdollEffect = ragdollEffect;
							parameters.AlertPosition = transform.position;

							DamageTool.damageAnimal(parameters, out kill, out xp);
						}
					}
					else if (info.type == ERaycastInfoType.VEHICLE)
					{
						if (info.vehicle != null)
						{
							if (info.vehicle.asset != null && info.vehicle.canBeDamaged && (info.vehicle.asset.isVulnerable || CanDamageInvulnerableEntities))
							{
								bool isHighcal = CanDamageInvulnerableEntities;
								float vehicleDamageMp = isHighcal ? Provider.modeConfigData.Vehicles.Gun_Highcal_Damage_Multiplier : Provider.modeConfigData.Vehicles.Gun_Lowcal_Damage_Multiplier;
								vehicleDamageMp *= info.vehicle.asset.GetArmorFalloffMultiplier(distanceTraveled);

								DamageTool.damage(info.vehicle, true, info.point, false, equippedGunAsset.vehicleDamage, times * vehicleDamageMp, true, out kill, instigatorSteamID: channel.owner.playerID.steamID, damageOrigin: EDamageOrigin.Useable_Gun);
							}
						}
					}
					else if (info.type == ERaycastInfoType.BARRICADE)
					{
						if (info.transform != null && info.transform.CompareTag("Barricade"))
						{
							BarricadeDrop barricade = BarricadeDrop.FindByRootFast(info.transform);
							if (barricade != null)
							{
								ItemBarricadeAsset asset = barricade.asset;
								if (asset != null && asset.canBeDamaged && (asset.isVulnerable || CanDamageInvulnerableEntities))
								{
									bool isHighcal = CanDamageInvulnerableEntities;
									float barricadeDmgMp = isHighcal ? Provider.modeConfigData.Barricades.Gun_Highcal_Damage_Multiplier : Provider.modeConfigData.Barricades.Gun_Lowcal_Damage_Multiplier;
									barricadeDmgMp *= asset.GetArmorFalloffMultiplier(distanceTraveled);

									if (barricade.interactable is InteractableSentry sentry)
									{
										sentry.AlertDamagedBy(player);
									}

									DamageTool.damage(info.transform, false, equippedGunAsset.barricadeDamage, times * barricadeDmgMp, out kill, instigatorSteamID: channel.owner.playerID.steamID, damageOrigin: EDamageOrigin.Useable_Gun);
								}
							}
						}
					}
					else if (info.type == ERaycastInfoType.STRUCTURE)
					{
						if (info.transform != null && info.transform.CompareTag("Structure"))
						{
							StructureDrop structure = StructureDrop.FindByRootFast(info.transform);
							if (structure != null)
							{
								ItemStructureAsset asset = structure.asset;
								if (asset != null && asset.canBeDamaged && (asset.isVulnerable || CanDamageInvulnerableEntities))
								{
									bool isHighcal = CanDamageInvulnerableEntities;
									float structureDmgMp = isHighcal ? Provider.modeConfigData.Structures.Gun_Highcal_Damage_Multiplier : Provider.modeConfigData.Structures.Gun_Lowcal_Damage_Multiplier;
									structureDmgMp *= asset.GetArmorFalloffMultiplier(distanceTraveled);

									DamageTool.damage(info.transform, false, info.direction * Mathf.Ceil(pellets / 2f), equippedGunAsset.structureDamage, times * structureDmgMp, out kill, instigatorSteamID: channel.owner.playerID.steamID, damageOrigin: EDamageOrigin.Useable_Gun);
								}
							}
						}
					}
					else if (info.type == ERaycastInfoType.RESOURCE)
					{
						if (info.transform != null && info.transform.CompareTag("Resource"))
						{
							byte x;
							byte y;
							ushort index;
							if (ResourceManager.tryGetRegion(info.transform, out x, out y, out index))
							{
								ResourceSpawnpoint spawnpoint = ResourceManager.getResourceSpawnpoint(x, y, index);

								if (spawnpoint != null && !spawnpoint.isDead && equippedGunAsset.hasBladeID(spawnpoint.asset.bladeID))
								{
									float resourceDmgMp = spawnpoint.asset.GetArmorFalloffMultiplier(distanceTraveled);

									DamageTool.damage(info.transform, info.direction * Mathf.Ceil(pellets / 2f), equippedGunAsset.resourceDamage, times * resourceDmgMp, 1f, out kill, out xp, instigatorSteamID: channel.owner.playerID.steamID, damageOrigin: EDamageOrigin.Useable_Gun);
								}
							}
						}
					}
					else if (info.type == ERaycastInfoType.OBJECT)
					{
						if (info.transform != null && info.section < byte.MaxValue)
						{
							InteractableObjectRubble rubble = info.transform.GetComponentInParent<InteractableObjectRubble>();
							if (rubble != null && rubble.IsSectionIndexValid(info.section) && !rubble.isSectionDead(info.section) && equippedGunAsset.hasBladeID(rubble.asset.rubbleBladeID))
							{
								if (rubble.asset.rubbleIsVulnerable || CanDamageInvulnerableEntities)
								{
									float objectDmgMp = rubble.asset.GetArmorFalloffMultiplier(distanceTraveled);

									DamageTool.damage(rubble.transform, info.direction, info.section, equippedGunAsset.objectDamage, times * objectDmgMp, out kill, out xp, instigatorSteamID: channel.owner.playerID.steamID, damageOrigin: EDamageOrigin.Useable_Gun);
								}
							}
						}
					}

					// only do aggressor check if we didn't shoot a player (because we would already be marked aggressor) and if we weren't saving them from a zombie
					if (info.type != ERaycastInfoType.PLAYER && info.type != ERaycastInfoType.ZOMBIE && info.type != ERaycastInfoType.ANIMAL)
					{
						if (!player.life.isAggressor)
						{
							float bulletRange = equippedGunAsset.range + Provider.modeConfigData.Players.Ray_Aggressor_Distance;
							bulletRange *= bulletRange;
							float rayAggressor = Provider.modeConfigData.Players.Ray_Aggressor_Distance;
							rayAggressor *= rayAggressor;

							Vector3 bulletNorm = (bullet.position - player.look.aim.position).normalized;

							for (int enemyIndex = 0; enemyIndex < Provider.clients.Count; enemyIndex++)
							{
								if (Provider.clients[enemyIndex] == channel.owner)
								{
									continue;
								}

								Player enemy = Provider.clients[enemyIndex].player;

								if (enemy == null)
								{
									continue;
								}

								Vector3 enemyOffset = enemy.look.aim.position - player.look.aim.position;
								Vector3 bulletProj = Vector3.Project(enemyOffset, bulletNorm);

								if (bulletProj.sqrMagnitude < bulletRange && (bulletProj - enemyOffset).sqrMagnitude < rayAggressor) // shot within 4 meters of enemy
								{
									player.life.markAggressive(false);
								}
							}
						}
					}

					if (Level.info.type == ELevelType.HORDE)
					{
						if (info.zombie != null)
						{
							if (info.limb == ELimb.SKULL)
							{
								player.skills.askPay(10);
							}
							else
							{
								player.skills.askPay(5);
							}
						}

						if (kill == EPlayerKill.ZOMBIE)
						{
							if (info.limb == ELimb.SKULL)
							{
								player.skills.askPay(50);
							}
							else
							{
								player.skills.askPay(25);
							}
						}
					}
					else
					{
						if (kill == EPlayerKill.PLAYER)
						{
							if (Level.info.type == ELevelType.ARENA)
							{
								player.skills.askPay(100);
							}
						}

						player.sendStat(kill);

						if (xp > 0)
						{
							player.skills.askPay(xp);
						}
					}

					Vector3 hit = info.point + (info.normal * 0.25f);

					if (bullet.magazineAsset != null && bullet.magazineAsset.isExplosive)
					{
						DetonateExplosiveMagazine(bullet.magazineAsset, hit, player, ragdollEffect);
					}

					if (bullet.dropID != 0)
					{
						ItemManager.dropItem(new Item(bullet.dropID, bullet.dropAmount, bullet.dropQuality), hit, false, Dedicator.IsDedicatedServer, false);
					}

					bullets.RemoveAt(0);
				}
			}

			if (player.equipment.asset != null)
			{
				if (Provider.modeConfigData.Gameplay.Ballistics)
				{
					for (int bulletIndex = bullets.Count - 1; bulletIndex >= 0; bulletIndex--)
					{
						BulletInfo bullet = bullets[bulletIndex];

						bullet.steps++;
						if (bullet.steps >= equippedGunAsset.ballisticSteps)
						{
							bullets.RemoveAt(bulletIndex);
						}
					}
				}
				else
				{
					bullets.Clear();
				}
			}
		}

		[System.Obsolete]
		public void askAttachSight(CSteamID steamID, byte page, byte x, byte y, byte[] hash)
		{
			ReceiveAttachSight(page, x, y, hash);
		}

		private static readonly ServerInstanceMethod<byte, byte, byte, byte[]> SendAttachSight = ServerInstanceMethod<byte, byte, byte, byte[]>.Get(typeof(UseableGun), nameof(ReceiveAttachSight));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askAttachSight))]
		public void ReceiveAttachSight(byte page, byte x, byte y, byte[] hash)
		{
			if (player.equipment.isBusy)
			{
				return;
			}

			if (isFired)
			{
				return;
			}

			if (isReloading || isHammering || isUnjamming || needsRechamber)
			{
				return;
			}

			if (player.equipment.asset == null)
			{
				return;
			}

			if (equippedGunAsset.hasSight == false)
			{
				return;
			}

			Item item = null;

			if (thirdAttachments.sightAsset != null)
			{
				item = new Item(thirdAttachments.sightID, false, player.equipment.state[13]);
			}

			if (page != 255)
			{
				byte index = player.inventory.getIndex(page, x, y);

				if (index != 255)
				{
					ItemJar jar = player.inventory.getItem(page, index);

					ItemCaliberAsset asset = jar.GetAsset<ItemCaliberAsset>();
					if (asset == null)
					{
						return;
					}

					if (asset.shouldVerifyHash && !Hash.verifyHash(hash, asset.hash))
					{
						return;
					}

					if (asset.calibers.Length != 0)
					{
						bool compatible = false;

						for (byte assetCaliberStep = 0; assetCaliberStep < asset.calibers.Length; assetCaliberStep++)
						{
							for (byte gunCaliberStep = 0; gunCaliberStep < equippedGunAsset.attachmentCalibers.Length; gunCaliberStep++)
							{
								if (asset.calibers[assetCaliberStep] == equippedGunAsset.attachmentCalibers[gunCaliberStep])
								{
									compatible = true;
									break;
								}
							}
						}

						if (!compatible)
						{
							return;
						}
					}
					else if (equippedGunAsset.requiresNonZeroAttachmentCaliber)
					{
						// Attachment did not specify a caliber.
						return;
					}

					if (changeSightRequested(item, jar) == false)
						return;

					System.Buffer.BlockCopy(System.BitConverter.GetBytes(jar.item.id), 0, player.equipment.state, 0, 2);
					player.equipment.state[13] = jar.item.quality;

					player.inventory.removeItem(page, index);

					if (item != null)
					{
						player.inventory.forceAddItem(item, true);
					}

					player.equipment.sendUpdateState();

					EffectManager.TriggerFiremodeEffect(transform.position);

					return;
				}
			}

			if (changeSightRequested(item, null) == false)
				return;

			if (item != null)
			{
				player.inventory.forceAddItem(item, true);
			}

			player.equipment.state[0] = 0;
			player.equipment.state[1] = 0;

			player.equipment.sendUpdateState();

			EffectManager.TriggerFiremodeEffect(transform.position);
		}

		[System.Obsolete]
		public void askAttachTactical(CSteamID steamID, byte page, byte x, byte y, byte[] hash)
		{
			ReceiveAttachTactical(page, x, y, hash);
		}

		private static readonly ServerInstanceMethod<byte, byte, byte, byte[]> SendAttachTactical = ServerInstanceMethod<byte, byte, byte, byte[]>.Get(typeof(UseableGun), nameof(ReceiveAttachTactical));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askAttachTactical))]
		public void ReceiveAttachTactical(byte page, byte x, byte y, byte[] hash)
		{
			if (player.equipment.isBusy)
			{
				return;
			}

			if (isFired)
			{
				return;
			}

			if (isReloading || isHammering || isUnjamming || needsRechamber)
			{
				return;
			}

			if (player.equipment.asset == null)
			{
				return;
			}

			if (equippedGunAsset.hasTactical == false)
			{
				return;
			}

			Item item = null;

			if (thirdAttachments.tacticalAsset != null)
			{
				item = new Item(thirdAttachments.tacticalID, false, player.equipment.state[14]);
			}

			if (page != 255)
			{
				byte index = player.inventory.getIndex(page, x, y);

				if (index != 255)
				{
					ItemJar jar = player.inventory.getItem(page, index);

					ItemCaliberAsset asset = jar.GetAsset<ItemCaliberAsset>();
					if (asset == null)
					{
						return;
					}

					if (asset.shouldVerifyHash && !Hash.verifyHash(hash, asset.hash))
					{
						return;
					}

					if (asset.calibers.Length != 0)
					{
						bool compatible = false;

						for (byte assetCaliberStep = 0; assetCaliberStep < asset.calibers.Length; assetCaliberStep++)
						{
							for (byte gunCaliberStep = 0; gunCaliberStep < equippedGunAsset.attachmentCalibers.Length; gunCaliberStep++)
							{
								if (asset.calibers[assetCaliberStep] == equippedGunAsset.attachmentCalibers[gunCaliberStep])
								{
									compatible = true;
									break;
								}
							}
						}

						if (!compatible)
						{
							return;
						}
					}
					else if (equippedGunAsset.requiresNonZeroAttachmentCaliber)
					{
						// Attachment did not specify a caliber.
						return;
					}

					if (changeTacticalRequested(item, jar) == false)
						return;

					System.Buffer.BlockCopy(System.BitConverter.GetBytes(jar.item.id), 0, player.equipment.state, 2, 2);
					player.equipment.state[14] = jar.item.quality;

					player.inventory.removeItem(page, index);

					if (item != null)
					{
						player.inventory.forceAddItem(item, true);
					}

					player.equipment.sendUpdateState();

					EffectManager.TriggerFiremodeEffect(transform.position);

					return;
				}
			}

			if (changeTacticalRequested(item, null) == false)
				return;

			if (item != null)
			{
				player.inventory.forceAddItem(item, true);
			}

			player.equipment.state[2] = 0;
			player.equipment.state[3] = 0;

			player.equipment.sendUpdateState();

			EffectManager.TriggerFiremodeEffect(transform.position);
		}

		[System.Obsolete]
		public void askAttachGrip(CSteamID steamID, byte page, byte x, byte y, byte[] hash)
		{
			ReceiveAttachGrip(page, x, y, hash);
		}

		private static readonly ServerInstanceMethod<byte, byte, byte, byte[]> SendAttachGrip = ServerInstanceMethod<byte, byte, byte, byte[]>.Get(typeof(UseableGun), nameof(ReceiveAttachGrip));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askAttachGrip))]
		public void ReceiveAttachGrip(byte page, byte x, byte y, byte[] hash)
		{
			if (player.equipment.isBusy)
			{
				return;
			}

			if (isFired)
			{
				return;
			}

			if (isReloading || isHammering || isUnjamming || needsRechamber)
			{
				return;
			}

			if (player.equipment.asset == null)
			{
				return;
			}

			if (equippedGunAsset.hasGrip == false)
			{
				return;
			}

			Item item = null;

			if (thirdAttachments.gripAsset != null)
			{
				item = new Item(thirdAttachments.gripID, false, player.equipment.state[15]);
			}

			if (page != 255)
			{
				byte index = player.inventory.getIndex(page, x, y);

				if (index != 255)
				{
					ItemJar jar = player.inventory.getItem(page, index);

					ItemCaliberAsset asset = jar.GetAsset<ItemCaliberAsset>();
					if (asset == null)
					{
						return;
					}

					if (asset.shouldVerifyHash && !Hash.verifyHash(hash, asset.hash))
					{
						return;
					}

					if (asset.calibers.Length != 0)
					{
						bool compatible = false;

						for (byte assetCaliberStep = 0; assetCaliberStep < asset.calibers.Length; assetCaliberStep++)
						{
							for (byte gunCaliberStep = 0; gunCaliberStep < equippedGunAsset.attachmentCalibers.Length; gunCaliberStep++)
							{
								if (asset.calibers[assetCaliberStep] == equippedGunAsset.attachmentCalibers[gunCaliberStep])
								{
									compatible = true;
									break;
								}
							}
						}

						if (!compatible)
						{
							return;
						}
					}
					else if (equippedGunAsset.requiresNonZeroAttachmentCaliber)
					{
						// Attachment did not specify a caliber.
						return;
					}

					if (changeGripRequested(item, jar) == false)
						return;

					System.Buffer.BlockCopy(System.BitConverter.GetBytes(jar.item.id), 0, player.equipment.state, 4, 2);
					player.equipment.state[15] = jar.item.quality;

					player.inventory.removeItem(page, index);

					if (item != null)
					{
						player.inventory.forceAddItem(item, true);
					}

					player.equipment.sendUpdateState();

					EffectManager.TriggerFiremodeEffect(transform.position);

					return;
				}
			}

			if (changeGripRequested(item, null) == false)
				return;

			if (item != null)
			{
				player.inventory.forceAddItem(item, true);
			}

			player.equipment.state[4] = 0;
			player.equipment.state[5] = 0;

			player.equipment.sendUpdateState();

			EffectManager.TriggerFiremodeEffect(transform.position);
		}

		[System.Obsolete]
		public void askAttachBarrel(CSteamID steamID, byte page, byte x, byte y, byte[] hash)
		{
			ReceiveAttachBarrel(page, x, y, hash);
		}

		private static readonly ServerInstanceMethod<byte, byte, byte, byte[]> SendAttachBarrel = ServerInstanceMethod<byte, byte, byte, byte[]>.Get(typeof(UseableGun), nameof(ReceiveAttachBarrel));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askAttachBarrel))]
		public void ReceiveAttachBarrel(byte page, byte x, byte y, byte[] hash)
		{
			if (player.equipment.isBusy)
			{
				return;
			}

			if (isFired)
			{
				return;
			}

			if (isReloading || isHammering || isUnjamming || needsRechamber)
			{
				return;
			}

			if (player.equipment.asset == null)
			{
				return;
			}

			if (equippedGunAsset.hasBarrel == false)
			{
				return;
			}

			Item item = null;

			if (thirdAttachments.barrelAsset != null)
			{
				item = new Item(thirdAttachments.barrelID, false, player.equipment.state[16]);
			}

			if (page != 255)
			{
				byte index = player.inventory.getIndex(page, x, y);

				if (index != 255)
				{
					ItemJar jar = player.inventory.getItem(page, index);

					ItemCaliberAsset asset = jar.GetAsset<ItemCaliberAsset>();
					if (asset == null)
					{
						return;
					}

					if (asset.shouldVerifyHash && !Hash.verifyHash(hash, asset.hash))
					{
						return;
					}

					if (asset.calibers.Length != 0)
					{
						bool compatible = false;

						for (byte assetCaliberStep = 0; assetCaliberStep < asset.calibers.Length; assetCaliberStep++)
						{
							for (byte gunCaliberStep = 0; gunCaliberStep < equippedGunAsset.attachmentCalibers.Length; gunCaliberStep++)
							{
								if (asset.calibers[assetCaliberStep] == equippedGunAsset.attachmentCalibers[gunCaliberStep])
								{
									compatible = true;
									break;
								}
							}
						}

						if (!compatible)
						{
							return;
						}
					}
					else if (equippedGunAsset.requiresNonZeroAttachmentCaliber)
					{
						// Attachment did not specify a caliber.
						return;
					}

					if (changeBarrelRequested(item, jar) == false)
						return;

					System.Buffer.BlockCopy(System.BitConverter.GetBytes(jar.item.id), 0, player.equipment.state, 6, 2);
					player.equipment.state[16] = jar.item.quality;

					player.inventory.removeItem(page, index);

					if (item != null)
					{
						player.inventory.forceAddItem(item, true);
					}

					player.equipment.sendUpdateState();

					EffectManager.TriggerFiremodeEffect(transform.position);

					return;
				}
			}

			if (changeBarrelRequested(item, null) == false)
				return;

			if (item != null)
			{
				player.inventory.forceAddItem(item, true);
			}

			player.equipment.state[6] = 0;
			player.equipment.state[7] = 0;

			player.equipment.sendUpdateState();

			EffectManager.TriggerFiremodeEffect(transform.position);
		}

		[System.Obsolete]
		public void askAttachMagazine(CSteamID steamID, byte page, byte x, byte y, byte[] hash)
		{
		}

		private static readonly ServerInstanceMethod<byte, byte, byte, byte[]> SendAttachMagazine = ServerInstanceMethod<byte, byte, byte, byte[]>.Get(typeof(UseableGun), nameof(ReceiveAttachMagazine));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askAttachMagazine))]
		public void ReceiveAttachMagazine(in ServerInvocationContext context, byte page, byte x, byte y, byte[] hash)
		{
			if (player.equipment.isBusy)
			{
				context.LogWarning("busy");
				return;
			}

			if (isFired)
			{
				context.LogWarning("fired");
				return;
			}

			if (isReloading || isHammering || isUnjamming || needsRechamber)
			{
				context.LogWarning("reloading, hammering, unjamming, or needs rechamber");
				return;
			}

			if (player.equipment.asset == null)
			{
				context.LogWarning("no equipped asset");
				return;
			}

			if (equippedGunAsset.allowMagazineChange == false)
			{
				context.LogWarning("equipped asset doesn't permit magazine change");
				return;
			}

			bool shouldHammer;
			Item item = null;

			if (thirdAttachments.magazineAsset != null && (ammo > 0 || (!equippedGunAsset.shouldDeleteEmptyMagazines && !thirdAttachments.magazineAsset.ShouldDeleteAtZeroAmount)))
			{
				byte detachAmount = player.equipment.state[10];
				if (thirdAttachments.magazineAsset.shouldFillAfterDetach)
				{
					detachAmount = thirdAttachments.magazineAsset.MaxAmountAsByte;
				}

				item = new Item(thirdAttachments.magazineID, detachAmount, player.equipment.state[17]);
			}

			if (page != 255)
			{
				byte index = player.inventory.getIndex(page, x, y);

				if (index != 255)
				{
					ItemJar jar = player.inventory.getItem(page, index);

					ItemCaliberAsset asset = jar.GetAsset<ItemCaliberAsset>();
					if (asset == null)
					{
						context.LogWarning("Unable to find asset for requested item.");
						return;
					}

					if (asset.shouldVerifyHash && !Hash.verifyHash(hash, asset.hash))
					{
						context.LogWarning("Hash doesn't match.");
						return;
					}

					if (asset.calibers.Length != 0)
					{
						bool compatible = false;

						for (byte assetCaliberStep = 0; assetCaliberStep < asset.calibers.Length; assetCaliberStep++)
						{
							for (byte gunCaliberStep = 0; gunCaliberStep < equippedGunAsset.magazineCalibers.Length; gunCaliberStep++)
							{
								if (asset.calibers[assetCaliberStep] == equippedGunAsset.magazineCalibers[gunCaliberStep])
								{
									compatible = true;
									break;
								}
							}
						}

						if (!compatible)
						{
							context.LogWarning("Requested item is incompatible.");
							return;
						}
					}
					else if (equippedGunAsset.requiresNonZeroAttachmentCaliber)
					{
						context.LogWarning("Requested item doesn't specify a caliber and gun equires non-zero caliber.");
						return;
					}

					if (changeMagazineRequested(item, jar) == false)
					{
						context.LogWarning("Plugin prevented magazine change request.");
						return;
					}

					switch (equippedGunAsset.RechamberAfterMagazineAttached)
					{
						default:
						case ERechamberGunAfterReloadMode.IfAmmoWasEmpty:
							shouldHammer = (ammo == 0);
							break;

						case ERechamberGunAfterReloadMode.Never:
							shouldHammer = false;
							break;

						case ERechamberGunAfterReloadMode.Always:
							shouldHammer = true;
							break;
					}
					ammo = jar.item.amount;

					System.Buffer.BlockCopy(System.BitConverter.GetBytes(jar.item.id), 0, player.equipment.state, 8, 2);
					player.equipment.state[10] = jar.item.amount;
					player.equipment.state[17] = jar.item.quality;

					player.inventory.removeItem(page, index);

					if (item != null)
					{
						player.inventory.forceAddItem(item, true);
					}

					player.equipment.sendUpdateState();

					shouldHammer &= equippedGunAsset.hammer != null;
					SendPlayReload.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), shouldHammer);

					EffectManager.TriggerFiremodeEffect(transform.position);

					return;
				}
			}

			if (changeMagazineRequested(item, null) == false)
			{
				context.LogWarning("Plugin prevented magazine removal request.");
				return;
			}

			switch (equippedGunAsset.RechamberAfterMagazineDetached)
			{
				default:
				case ERechamberGunAfterReloadMode.IfAmmoWasEmpty:
					shouldHammer = (ammo == 0);
					break;

				case ERechamberGunAfterReloadMode.Never:
					shouldHammer = false;
					break;

				case ERechamberGunAfterReloadMode.Always:
					shouldHammer = true;
					break;
			}

			if (item != null)
			{
				player.inventory.forceAddItem(item, true);
			}

			player.equipment.state[8] = 0;
			player.equipment.state[9] = 0;
			player.equipment.state[10] = 0;

			player.equipment.sendUpdateState();

			shouldHammer &= equippedGunAsset.hammer != null;
			SendPlayReload.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), shouldHammer);

			EffectManager.TriggerFiremodeEffect(transform.position);
		}

		private void hammer()
		{
			player.equipment.isBusy = true;

			isHammering = true;
			startedHammer = Time.realtimeSinceStartup;

			float speed = 1.0f;
			speed += player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.DEXTERITY) * 0.5f;
			if (thirdAttachments.magazineAsset != null)
			{
				speed *= thirdAttachments.magazineAsset.speed;
			}

			// Nelson 2023-12-11: zero pitch randomization to prevent anim desync. (public issue #4249)
			player.playSound(equippedGunAsset.hammer, speed, 0.0f);

			updateAnimationSpeeds(speed);

			//if(isAiming)
			//{
			//	isAiming = false;
			//	stopAim();

			//	if(equippedGunAsset.action == EAction.Bolt || equippedGunAsset.action == EAction.Pump)
			//	{
			//				 player.animator.play("Scope", false);
			//		return;
			//	}
			//}

			shotCountForRechamber = 0;

			player.animator.play("Hammer", false);
			GetVehicleTurretEventHook()?.OnChamberingStarted?.TryInvoke(this);

			foreach (UseableGunEventHook eventComponent in EnumerateEventComponents())
				eventComponent.OnChamberingStarted?.TryInvoke(this);
		}

		[System.Obsolete]
		public void askReload(CSteamID steamID, bool newHammer)
		{
			ReceivePlayReload(newHammer);
		}

		private static readonly ClientInstanceMethod<bool> SendPlayReload = ClientInstanceMethod<bool>.Get(typeof(UseableGun), nameof(ReceivePlayReload));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askReload))]
		public void ReceivePlayReload(bool newHammer)
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				if (isAiming)
				{
					isAiming = false;
					stopAim();
				}

				if (isAttaching)
				{
					isAttaching = false;
					stopAttach();
				}

				isShooting = false;
				isSprinting = false;

				player.equipment.isBusy = true;

				needsHammer = newHammer;

				isReloading = true;
				startedReload = Time.realtimeSinceStartup;

				float speed = 1.0f;
				speed += player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.DEXTERITY) * 0.5f;
				if (thirdAttachments.magazineAsset != null)
				{
					speed *= thirdAttachments.magazineAsset.speed;
				}

				// Nelson 2023-12-11: zero pitch randomization to prevent anim desync. (public issue #4249)
				player.playSound(equippedGunAsset.reload, speed, 0.0f);

				updateAnimationSpeeds(speed);

				player.animator.play("Reload", false);

				needsUnplace = true;
				needsReplace = true;

				if (equippedGunAsset.CasingEjectCountAfterReload > 0)
				{
					needsUnload = true;
				}

				shotCountForRechamber = 0;

				OnReloading_Global.TryInvoke("OnReloading_Global", this);
				GetVehicleTurretEventHook()?.OnReloadingStarted?.TryInvoke(this);

				foreach (UseableGunEventHook eventComponent in EnumerateEventComponents())
					eventComponent.OnReloadingStarted?.TryInvoke(this);
			}
		}

		/// <summary>
		/// Requested for plugin use.
		/// </summary>
		public void ServerPlayReload(bool shouldHammer)
		{
			shouldHammer &= equippedGunAsset.hammer != null;
			SendPlayReload.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), shouldHammer);
		}

		[System.Obsolete]
		public void askPlayChamberJammed(CSteamID steamID, byte correctedAmmo)
		{
			ReceivePlayChamberJammed(correctedAmmo);
		}

		private static readonly ClientInstanceMethod<byte> SendPlayChamberJammed = ClientInstanceMethod<byte>.Get(typeof(UseableGun), nameof(ReceivePlayChamberJammed));
		/// <summary>
		/// Request from the server to play a gun jammed animation.
		/// Since client can't predict chamber jams we fixup the predicted ammo count.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askPlayChamberJammed))]
		public void ReceivePlayChamberJammed(byte correctedAmmo)
		{
			if (!player.equipment.IsEquipAnimationFinished)
				return;

			if (isAiming)
			{
				isAiming = false;
				stopAim();
			}

			if (isAttaching)
			{
				isAttaching = false;
				stopAttach();
			}

			isShooting = false;
			isSprinting = false;

			player.equipment.isBusy = true;

			isUnjamming = true;
			startedUnjammingChamber = Time.realtimeSinceStartup;

			float speed = 1.0f;
			// Nelson 2023-12-11: zero pitch randomization to prevent anim desync. (public issue #4249)
			player.playSound(equippedGunAsset.chamberJammedSound, speed, 0.0f);

			updateAnimationSpeeds(speed);

			player.animator.play(equippedGunAsset.unjamChamberAnimName, false);

			ammo = correctedAmmo;
		}

		[System.Obsolete]
		public void askAimStart(CSteamID steamID)
		{
			ReceivePlayAimStart();
		}

		private static readonly ClientInstanceMethod SendPlayAimStart = ClientInstanceMethod.Get(typeof(UseableGun), nameof(ReceivePlayAimStart));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askAimStart))]
		public void ReceivePlayAimStart()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				startAim();
			}
		}

		[System.Obsolete]
		public void askAimStop(CSteamID steamID)
		{
			ReceivePlayAimStop();
		}

		private static readonly ClientInstanceMethod SendPlayAimStop = ClientInstanceMethod.Get(typeof(UseableGun), nameof(ReceivePlayAimStop));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askAimStop))]
		public void ReceivePlayAimStop()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				stopAim();
			}
		}

		public override bool startPrimary()
		{
			bool canStartShooting = !isShooting && !isReloading && !isHammering && !isUnjamming && !isAttaching && !needsRechamber && firemode != EFiremode.SAFETY && !player.equipment.isBusy && !player.quests.IsCutsceneModeActive();
			canStartShooting &= isSprinting == false || equippedGunAsset.canAimDuringSprint;

			if (canStartShooting)
			{
				if (equippedGunAsset.action == EAction.String)
				{
					if (thirdAttachments.nockHook != null || isAiming)
					{
						isShooting = true;
					}
				}
				else if (equippedGunAsset.MustAimToShoot)
				{
					if (isAiming)
					{
						isShooting = true;
					}
				}
				else
				{
					isShooting = true;
				}
			}

			if (isShooting)
			{
				wasTriggerJustPulled = true;

				// Only set fireDelayCounter if not already queued up, otherwise sound can be spammed. (public issue 3820)
				if (fireDelayCounter < 1)
				{
					fireDelayCounter = equippedGunAsset.fireDelay;

					if (fireDelayCounter > 0 && channel.IsLocalPlayer && equippedGunAsset.fireDelaySound != null)
					{
						// Nelson 2023-12-11: zero pitch randomization to prevent anim desync. (public issue #4249)
						player.playSound(equippedGunAsset.fireDelaySound, 1.0f, 0.0f);
					}
				}
			}

			return isShooting;
		}

		public override void stopPrimary()
		{
			isShooting = false;
		}

		public override bool startSecondary()
		{
			bool canStartAim = !isAiming && !isReloading && !isHammering && !isUnjamming && !isAttaching && !needsRechamber && firemode != EFiremode.SAFETY;
			canStartAim &= isSprinting == false || equippedGunAsset.canAimDuringSprint;

			if (canStartAim)
			{
				isAiming = true;
				startAim();

				if (Provider.isServer)
				{
					SendPlayAimStart.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
				}
			}

			return isAiming;
		}

		public override void stopSecondary()
		{
			if (isAiming)
			{
				if (equippedGunAsset.MustAimToShoot)
				{
					if (isShooting)
					{
						isShooting = false;
					}
				}

				isAiming = false;
				stopAim();

				if (Provider.isServer)
				{
					SendPlayAimStop.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
				}
			}
		}

		public override bool canInspect => !isShooting && !isReloading && !isHammering && !isUnjamming && !isSprinting && !isAttaching && !isAiming && !needsRechamber;

		public override void equip()
		{
			lastShot = float.MaxValue;

			firstEventComponent = player.equipment.firstModel?.GetComponent<UseableGunEventHook>();
			thirdEventComponent = player.equipment.thirdModel?.GetComponent<UseableGunEventHook>();
			characterEventComponent = player.equipment.characterModel?.GetComponent<UseableGunEventHook>();

			if (!Dedicator.IsDedicatedServer)
			{
				if (channel.IsLocalPlayer)
				{
					gunshotAudioSource = player.gameObject.AddComponent<AudioSource>();
					gunshotAudioSource.priority = 63; // Prioritize our own gunshots. (lower values are prioritized)
				}
				else
				{
					gunshotAudioSource = player.equipment.thirdModel.gameObject.AddComponent<AudioSource>();
				}

				GameObject rolloffPrefab = Assets.coreMasterBundle.LoadAsset<GameObject>("Guns/Rolloff.prefab");
				gunshotAudioSource.clip = null;
				gunshotAudioSource.spatialBlend = 1f;
				gunshotAudioSource.rolloffMode = AudioRolloffMode.Custom;
				gunshotAudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, rolloffPrefab.GetComponent<AudioSource>().GetCustomCurve(AudioSourceCurveType.CustomRolloff));
				gunshotAudioSource.volume = 1f;
				gunshotAudioSource.playOnAwake = false;
				gunshotAudioSource.dopplerLevel = 0.0f;
#if !DEDICATED_SERVER
				gunshotAudioSource.outputAudioMixerGroup = UnturnedAudioMixer.GetDefaultGroup();
#endif
			}

			if (channel.IsLocalPlayer)
			{
				firstAttachments = player.equipment.firstModel.gameObject.GetComponent<Attachments>();

				firstMinigunBarrel = firstAttachments.transform.Find("Model_1");

				Transform firstAmmoCounterTransform = firstAttachments.transform.FindChildRecursive("Ammo_Counter");
				if (firstAmmoCounterTransform != null)
				{
					firstAmmoCounter = firstAmmoCounterTransform.GetComponent<UnityEngine.UI.Text>();
					firstAmmoCounterTransform.parent.gameObject.SetActive(true);

					firstAmmoCounterTransform.parent.gameObject.layer = LayerMasks.VIEWMODEL;
					firstAmmoCounterTransform.gameObject.layer = LayerMasks.VIEWMODEL;
				}

				if (firstAttachments.rope != null)
				{
					firstAttachments.rope.gameObject.SetActive(true);
				}

				if (firstAttachments.ejectHook != null)
				{
					EffectAsset shell = equippedGunAsset.FindShellEffectAsset();
					if (shell != null && shell.effect != null)
					{
						Transform emitter = EffectManager.InstantiateFromPool(shell).transform;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
						emitter.name = $"{shell.FriendlyName} (1p shell)";
#else // UNITY_EDITOR || DEVELOPMENT_BUILD
						emitter.name = "Emitter";
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
						emitter.parent = firstAttachments.ejectHook;
						emitter.localPosition = Vector3.zero;
						emitter.localRotation = Quaternion.identity;
						emitter.tag = "Viewmodel";
						emitter.gameObject.layer = LayerMasks.VIEWMODEL;

						firstShellEmitter = emitter.GetComponent<ParticleSystem>();
					}
				}

				if (firstAttachments.barrelHook != null)
				{
					EffectAsset muzzle = equippedGunAsset.FindMuzzleEffectAsset();
					if (muzzle != null && muzzle.effect != null)
					{
						Transform emitter = EffectManager.InstantiateFromPool(muzzle).transform;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
						emitter.name = $"{muzzle.FriendlyName} (1p muzzle)";
#else // UNITY_EDITOR || DEVELOPMENT_BUILD
						emitter.name = "Emitter";
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
						emitter.parent = firstAttachments.barrelHook;
						emitter.localPosition = Vector3.zero;
						emitter.localRotation = Quaternion.identity;
						emitter.tag = "Viewmodel";
						emitter.gameObject.layer = LayerMasks.VIEWMODEL;

						firstMuzzleEmitter = emitter.GetComponent<ParticleSystem>();
						ParticleSystem.MainModule main = firstMuzzleEmitter.main;
						main.simulationSpace = ParticleSystemSimulationSpace.Local;

						Light muzzleLight = emitter.GetComponent<Light>();
						if (muzzleLight != null)
						{
							muzzleLight.enabled = false;
							muzzleLight.cullingMask = RayMasks.VIEWMODEL;
						}
					}
				}

				if (equippedGunAsset.isTurret)
				{
					player.animator.viewmodelCameraLocalPositionOffset = Vector3.up;
				}

				switch (equippedGunAsset.driverTurretViewmodelMode)
				{
					case EDriverTurretViewmodelMode.AlwaysOffscreen:
						player.animator.drivingViewmodelCameraLocalPositionOffset = Vector3.up;
						break;

					default:
						player.animator.drivingViewmodelCameraLocalPositionOffset = Vector3.zero;
						break;
				}
			}

			thirdAttachments = player.equipment.thirdModel.gameObject.GetComponent<Attachments>();

			if (channel.IsLocalPlayer)
			{
				// We only show 3P ammo for local player.
				Transform thirdAmmoCounterTransform = thirdAttachments.transform.FindChildRecursive("Ammo_Counter");
				if (thirdAmmoCounterTransform != null)
				{
					thirdAmmoCounter = thirdAmmoCounterTransform.GetComponent<UnityEngine.UI.Text>();
					thirdAmmoCounterTransform.parent.gameObject.SetActive(true);

					thirdAmmoCounterTransform.parent.gameObject.layer = LayerMasks.ENEMY;
					thirdAmmoCounterTransform.gameObject.layer = LayerMasks.ENEMY;
				}
			}

			thirdMinigunBarrel = thirdAttachments.transform.Find("Model_1");

			if (!Dedicator.IsDedicatedServer && thirdMinigunBarrel != null && equippedGunAsset.action == EAction.Minigun)
			{
				if (channel.IsLocalPlayer)
				{
					whir = player.gameObject.AddComponent<AudioSource>();
				}
				else
				{
					whir = player.equipment.thirdModel.gameObject.AddComponent<AudioSource>();
				}

				whir.clip = equippedGunAsset.minigun;
				whir.spatialBlend = 1f;
				whir.rolloffMode = AudioRolloffMode.Linear;
				whir.minDistance = 1;
				whir.maxDistance = 16;
				whir.volume = 0.0f;
				whir.playOnAwake = false;
				whir.loop = true;
				whir.dopplerLevel = 0.0f;
#if !DEDICATED_SERVER
				whir.outputAudioMixerGroup = UnturnedAudioMixer.GetDefaultGroup();
#endif
				whir.Play();
			}

			if (thirdAttachments.ejectHook != null)
			{
				EffectAsset shell = equippedGunAsset.FindShellEffectAsset();
				if (shell != null && shell.effect != null)
				{
					Transform emitter = EffectManager.InstantiateFromPool(shell).transform;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					emitter.name = $"{shell.FriendlyName} (3p shell)";
#else // UNITY_EDITOR || DEVELOPMENT_BUILD
					emitter.name = "Emitter";
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
					emitter.localPosition = Vector3.zero;

					thirdShellEmitter = emitter.GetComponent<ParticleSystem>();
					thirdShellRenderer = emitter.GetComponent<ParticleSystemRenderer>();
					if (channel.IsLocalPlayer)
					{
						ParticleSystem.CollisionModule collision = thirdShellEmitter.collision;
						collision.enabled = true;

						ParticleSystem.TriggerModule trigger = thirdShellEmitter.trigger;
						trigger.enabled = true;
						trigger.inside = ParticleSystemOverlapAction.Ignore;
						trigger.outside = ParticleSystemOverlapAction.Ignore;
						trigger.enter = ParticleSystemOverlapAction.Callback;
						trigger.exit = ParticleSystemOverlapAction.Ignore;
						List<WaterVolume> waterVolumes = WaterVolumeManager.Get().InternalGetAllVolumes();
						for (int index = 0; index < waterVolumes.Count; ++index)
						{
							trigger.SetCollider(index, waterVolumes[index].volumeCollider);
						}

						if (player.look.perspective == EPlayerPerspective.FIRST && !equippedGunAsset.isTurret)
						{
							thirdShellRenderer.forceRenderingOff = true;
						}
					}
				}
			}

			if (thirdAttachments.barrelHook != null)
			{
				EffectAsset muzzle = equippedGunAsset.FindMuzzleEffectAsset();
				if (muzzle != null && muzzle.effect != null)
				{
					Transform emitter = EffectManager.InstantiateFromPool(muzzle).transform;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					emitter.name = $"{muzzle.FriendlyName} (3p muzzle)";
#else // UNITY_EDITOR || DEVELOPMENT_BUILD
					emitter.name = "Emitter";
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
					emitter.parent = equippedGunAsset.isTurret ? null : thirdAttachments.barrelHook;
					emitter.localPosition = Vector3.zero;
					emitter.localRotation = Quaternion.identity;

					thirdMuzzleEmitter = emitter.GetComponent<ParticleSystem>();

					Light muzzleLight = emitter.GetComponent<Light>();
					if (muzzleLight != null)
					{
						muzzleLight.enabled = false;
						muzzleLight.cullingMask = ~RayMasks.VIEWMODEL;
					}
				}

				if (channel.IsLocalPlayer)
				{
					if (muzzle != null && muzzle.effect != null)
					{
						firstFakeLight = GameObject.Instantiate(muzzle.effect).transform;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
						firstFakeLight.name = $"{muzzle.FriendlyName} (1p muzzle light)";
#else // UNITY_EDITOR || DEVELOPMENT_BUILD
						firstFakeLight.name = "Emitter";
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

						Light muzzleLight = firstFakeLight.GetComponent<Light>();
						if (muzzleLight != null)
						{
							muzzleLight.enabled = false;
							muzzleLight.cullingMask = ~RayMasks.VIEWMODEL;
						}
					}
				}
			}

			ammo = player.equipment.state[10];
			firemode = (EFiremode) player.equipment.state[11];
			interact = player.equipment.state[12] == 1;

			updateAttachments(true);

			startedReload = float.MaxValue;
			startedHammer = float.MaxValue;

			if (channel.IsLocalPlayer)
			{
				if (firemode == EFiremode.SAFETY)
				{
					PlayerUI.message(EPlayerMessage.SAFETY, "");
				}
				else if (ammo < equippedGunAsset.ammoPerShot)
				{
					PlayerUI.message(EPlayerMessage.RELOAD, "");
				}

				if (firstAttachments.reticuleHook != null)
				{
					originalReticuleHookLocalPosition = firstAttachments.reticuleHook.localPosition;
				}
				else
				{
					originalReticuleHookLocalPosition = Vector3.zero;
				}

				localization = Localization.read("/Player/Useable/PlayerUseableGun.dat");

				icons = Bundles.getIconsBundle("UI/Player/Icons/Useable/PlayerUseableGun");

				if (equippedGunAsset.hasSight)
				{
					sightButton = new SleekButtonIcon(icons.load<Texture2D>("Sight"));
					sightButton.PositionOffset_X = -25;
					sightButton.PositionOffset_Y = -25;
					sightButton.SizeOffset_X = 50;//200;
					sightButton.SizeOffset_Y = 50;
					sightButton.onClickedButton += onClickedSightHookButton;
					PlayerUI.container.AddChild(sightButton);
					sightButton.IsVisible = false;
				}

				if (equippedGunAsset.hasTactical)
				{
					tacticalButton = new SleekButtonIcon(icons.load<Texture2D>("Tactical"));
					tacticalButton.PositionOffset_X = -25;
					tacticalButton.PositionOffset_Y = -25;
					tacticalButton.SizeOffset_X = 50;
					tacticalButton.SizeOffset_Y = 50;
					tacticalButton.onClickedButton += onClickedTacticalHookButton;
					PlayerUI.container.AddChild(tacticalButton);
					tacticalButton.IsVisible = false;
				}

				if (equippedGunAsset.hasGrip)
				{
					gripButton = new SleekButtonIcon(icons.load<Texture2D>("Grip"));
					gripButton.PositionOffset_X = -25;
					gripButton.PositionOffset_Y = -25;
					gripButton.SizeOffset_X = 50;
					gripButton.SizeOffset_Y = 50;
					gripButton.onClickedButton += onClickedGripHookButton;
					PlayerUI.container.AddChild(gripButton);
					gripButton.IsVisible = false;
				}

				if (equippedGunAsset.hasBarrel)
				{
					barrelButton = new SleekButtonIcon(icons.load<Texture2D>("Barrel"));
					barrelButton.PositionOffset_X = -25;
					barrelButton.PositionOffset_Y = -25;
					barrelButton.SizeOffset_X = 50;
					barrelButton.SizeOffset_Y = 50;
					barrelButton.onClickedButton += onClickedBarrelHookButton;
					PlayerUI.container.AddChild(barrelButton);
					barrelButton.IsVisible = false;

					barrelQualityLabel = Glazier.Get().CreateLabel();
					barrelQualityLabel.PositionOffset_Y = -30;
					barrelQualityLabel.PositionScale_Y = 1;
					barrelQualityLabel.SizeOffset_Y = 30;
					barrelQualityLabel.SizeScale_X = 1;
					barrelQualityLabel.TextAlignment = TextAnchor.LowerLeft;
					barrelQualityLabel.FontSize = ESleekFontSize.Small;
					barrelButton.AddChild(barrelQualityLabel);
					barrelQualityLabel.IsVisible = false;

					barrelQualityImage = Glazier.Get().CreateImage();
					barrelQualityImage.PositionOffset_X = -15;
					barrelQualityImage.PositionOffset_Y = -15;
					barrelQualityImage.PositionScale_X = 1.0f;
					barrelQualityImage.PositionScale_Y = 1.0f;
					barrelQualityImage.SizeOffset_X = 10;
					barrelQualityImage.SizeOffset_Y = 10;
					barrelQualityImage.Texture = PlayerDashboardInventoryUI.icons.load<Texture2D>("Quality_1");
					barrelButton.AddChild(barrelQualityImage);
					barrelQualityImage.IsVisible = false;
				}

				if (equippedGunAsset.allowMagazineChange)
				{
					magazineButton = new SleekButtonIcon(icons.load<Texture2D>("Magazine"));
					magazineButton.PositionOffset_X = -25;
					magazineButton.PositionOffset_Y = -25;
					magazineButton.SizeOffset_X = 50;
					magazineButton.SizeOffset_Y = 50;
					magazineButton.onClickedButton += onClickedMagazineHookButton;
					PlayerUI.container.AddChild(magazineButton);
					magazineButton.IsVisible = false;

					magazineQualityLabel = Glazier.Get().CreateLabel();
					magazineQualityLabel.PositionOffset_Y = -30;
					magazineQualityLabel.PositionScale_Y = 1;
					magazineQualityLabel.SizeOffset_Y = 30;
					magazineQualityLabel.SizeScale_X = 1;
					magazineQualityLabel.TextAlignment = TextAnchor.LowerLeft;
					magazineQualityLabel.FontSize = ESleekFontSize.Small;
					magazineQualityLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
					magazineButton.AddChild(magazineQualityLabel);
					magazineQualityLabel.IsVisible = false;

					magazineQualityImage = Glazier.Get().CreateImage();
					magazineQualityImage.PositionOffset_X = -15;
					magazineQualityImage.PositionOffset_Y = -15;
					magazineQualityImage.PositionScale_X = 1.0f;
					magazineQualityImage.PositionScale_Y = 1.0f;
					magazineQualityImage.SizeOffset_X = 10;
					magazineQualityImage.SizeOffset_Y = 10;
					magazineQualityImage.Texture = PlayerDashboardInventoryUI.icons.load<Texture2D>("Quality_1");
					magazineButton.AddChild(magazineQualityImage);
					magazineQualityImage.IsVisible = false;
				}

				infoBox = Glazier.Get().CreateBox();
				infoBox.PositionOffset_Y = -70;
				infoBox.PositionScale_X = 0.7f;
				infoBox.PositionScale_Y = 1;
				infoBox.SizeOffset_Y = 70;
				infoBox.SizeScale_X = 0.3f;
				PlayerLifeUI.container.AddChild(infoBox);

				ammoLabel = Glazier.Get().CreateLabel();
				ammoLabel.SizeScale_X = 0.35f;
				ammoLabel.SizeScale_Y = 1f;
				ammoLabel.FontSize = ESleekFontSize.Large;
				infoBox.AddChild(ammoLabel);

				firemodeLabel = Glazier.Get().CreateLabel();
				firemodeLabel.PositionOffset_Y = 5;
				firemodeLabel.PositionScale_X = 0.35f;
				firemodeLabel.SizeScale_X = 0.65f;
				firemodeLabel.SizeScale_Y = 0.5f;
				infoBox.AddChild(firemodeLabel);

				attachLabel = Glazier.Get().CreateLabel();
				attachLabel.PositionOffset_Y = -5;
				attachLabel.PositionScale_X = 0.35f;
				attachLabel.PositionScale_Y = 0.5f;
				attachLabel.SizeScale_X = 0.65f;
				attachLabel.SizeScale_Y = 0.5f;
				attachLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				infoBox.AddChild(attachLabel);

				player.onLocalPluginWidgetFlagsChanged += OnLocalPluginWidgetFlagsChanged;
				UpdateInfoBoxVisibility();

				updateInfo();
			}

			// Nelson 2024-07-15: Experimenting with disabling the "blending" option for the equip animation. It was
			// problematic for new animations using the Spine_Hook bone because it isn't updated when an item isn't
			// held, so it would still be floating in front of the player when the next equip animation plays.
			// (public issue #4549) In my testing with blending disabled I don't notice any negative differences from
			// turning it off.
			player.animator.play("Equip", false);

			if (player.channel.IsLocalPlayer)
			{
				PlayerUI.disableDot();

				player.stance.onStanceUpdated += UpdateCrosshairEnabled;
				player.look.onPerspectiveUpdated += onPerspectiveUpdated;
				OptionsSettings.OnUnitSystemChanged += SyncScopeDistanceMarkerText;
			}

			if (channel.IsLocalPlayer || Provider.isServer)
			{
				if (equippedGunAsset.projectile == null)
				{
					bullets = new List<BulletInfo>();
				}
			}

			AimAccuracy = 0;
			steadyAccuracy = 0;
			canSteady = true;
			swayTime = Time.time;
		}

		public override void dequip()
		{
			if (infoBox != null)
			{
				if (sightButton != null)
				{
					PlayerUI.container.RemoveChild(sightButton);
				}

				if (tacticalButton != null)
				{
					PlayerUI.container.RemoveChild(tacticalButton);
				}

				if (gripButton != null)
				{
					PlayerUI.container.RemoveChild(gripButton);
				}

				if (barrelButton != null)
				{
					PlayerUI.container.RemoveChild(barrelButton);
				}

				if (magazineButton != null)
				{
					PlayerUI.container.RemoveChild(magazineButton);
				}

				if (rangeLabel != null)
				{
					rangeLabel.Parent.RemoveChild(rangeLabel);
				}

				PlayerLifeUI.container.RemoveChild(infoBox);

				player.onLocalPluginWidgetFlagsChanged -= OnLocalPluginWidgetFlagsChanged;
			}

			player.disableItemSpotLight();

			if (channel.IsLocalPlayer)
			{
				player.animator.viewmodelCameraLocalPositionOffset = Vector3.zero;
				player.animator.drivingViewmodelCameraLocalPositionOffset = Vector3.zero;

				if (gunshotAudioSource != null)
				{
					Destroy(gunshotAudioSource);
				}

				if (whir != null)
				{
					Destroy(whir);
				}

				DestroyLaser();

				if (isAiming)
				{
					isAiming = false;
					stopAim();
				}

				if (isAttaching)
				{
					stopAttach();
				}

				PlayerUI.isLocked = false;

				if (isAttaching)
				{
					PlayerLifeUI.open();
				}

				if (player.movement.getVehicle() == null)
				{
					PlayerUI.enableDot();
				}

				PlayerUI.disableCrosshair();

				player.look.disableScope();
				player.look.disableZoom();
				player.look.IsScopeHalfwayAimedIn = false;

				player.stance.onStanceUpdated -= UpdateCrosshairEnabled;
				player.look.onPerspectiveUpdated -= onPerspectiveUpdated;
				OptionsSettings.OnUnitSystemChanged -= SyncScopeDistanceMarkerText;

				if (firstFakeLight != null)
				{
					Destroy(firstFakeLight.gameObject);
				}

				if (firstFakeLight_0 != null)
				{
					Destroy(firstFakeLight_0.gameObject);
				}

				if (firstFakeLight_1 != null)
				{
					Destroy(firstFakeLight_1.gameObject);
				}
			}

			if (tracerEmitter != null)
			{
				EffectManager.DestroyIntoPool(tracerEmitter.gameObject);
				tracerEmitter = null;
			}

			if (firstMuzzleEmitter != null)
			{
				EffectManager.DestroyIntoPool(firstMuzzleEmitter.gameObject);
				firstMuzzleEmitter = null;
			}

			if (firstShellEmitter != null)
			{
				EffectManager.DestroyIntoPool(firstShellEmitter.gameObject);
				firstShellEmitter = null;
			}

			if (thirdMuzzleEmitter != null)
			{
				EffectManager.DestroyIntoPool(thirdMuzzleEmitter.gameObject);
				thirdMuzzleEmitter = null;
			}

			if (thirdShellEmitter != null)
			{
				if (channel.IsLocalPlayer)
				{
					ParticleSystem.CollisionModule collision = thirdShellEmitter.collision;
					collision.enabled = false;

					ParticleSystem.TriggerModule trigger = thirdShellEmitter.trigger;
					trigger.enabled = false;
				}

				if (thirdShellRenderer != null)
				{
					thirdShellRenderer.forceRenderingOff = false;
				}

				EffectManager.DestroyIntoPool(thirdShellEmitter.gameObject);
				thirdShellEmitter = null;
			}
		}

		public override void tick()
		{
			if (channel.IsLocalPlayer)
			{
				if (firstAttachments.rope != null)
				{
					if (firstAttachments.leftHook != null)
					{
						firstAttachments.rope.SetPosition(0, firstAttachments.leftHook.position);
					}

					if (firstAttachments.nockHook != null)
					{
						if (firstAttachments.magazineModel != null && firstAttachments.magazineModel.gameObject.activeSelf)
						{
							firstAttachments.rope.SetPosition(1, firstAttachments.nockHook.position);
						}
						else if (firstAttachments.restHook != null)
						{
							firstAttachments.rope.SetPosition(1, firstAttachments.restHook.position);
						}
					}
					else
					{
						if (isAiming)
						{
							firstAttachments.rope.SetPosition(1, player.equipment.firstRightHook.position);
						}
						else if ((isAttaching || isSprinting || player.equipment.isInspecting) && firstAttachments.magazineModel != null && firstAttachments.magazineModel.gameObject.activeSelf && firstAttachments.restHook != null)
						{
							firstAttachments.rope.SetPosition(1, firstAttachments.restHook.position);
						}
						else if (firstAttachments.leftHook != null)
						{
							firstAttachments.rope.SetPosition(1, firstAttachments.leftHook.position);
						}
					}

					if (firstAttachments.rightHook != null)
					{
						firstAttachments.rope.SetPosition(2, firstAttachments.rightHook.position);
					}
				}
			}

			if (!player.equipment.IsEquipAnimationFinished)
			{
				return;
			}

			if (Time.realtimeSinceStartup - lastShot > 0.05)
			{
				if (firstMuzzleEmitter != null)
				{
					Light light = firstMuzzleEmitter.GetComponent<Light>();
					if (light)
					{
						light.enabled = false;
					}
				}

				if (thirdMuzzleEmitter != null)
				{
					Light light = thirdMuzzleEmitter.GetComponent<Light>();
					if (light)
					{
						light.enabled = false;
					}
				}

				if (firstFakeLight != null)
				{
					Light light = firstFakeLight.GetComponent<Light>();
					if (light)
					{
						light.enabled = false;
					}
				}
			}

			if ((player.stance.stance == EPlayerStance.SPRINT && player.movement.isMoving) || firemode == EFiremode.SAFETY)
			{
				if (!isShooting && !isSprinting && !isReloading && !isHammering && !isUnjamming && !isAttaching && !isAiming && !needsRechamber)
				{
					isSprinting = true;
					player.animator.play("Sprint_Start", false);
				}
			}
			else
			{
				if (isSprinting)
				{
					isSprinting = false;

					if (!isAiming) // If we started sprinting, then aiming, then stopped while still aiming, don't play.
					{
						player.animator.play("Sprint_Stop", false);
					}
				}
			}

			if (channel.IsLocalPlayer)
			{
				if (InputEx.GetKeyUp(ControlsSettings.attach))
				{
					if (isAttaching)
					{
						isAttaching = false;
						player.animator.play("Attach_Stop", false);

						stopAttach();
					}
				}

				if (InputEx.GetKeyDown(ControlsSettings.tactical))
				{
					fireTacticalInput = true;
				}

				if (!PlayerUI.window.showCursor)
				{
					if (InputEx.ConsumeKeyDown(ControlsSettings.attach))
					{
						if (!isShooting && !isAttaching && !isSprinting && !isReloading && !isHammering && !isUnjamming && !isAiming && !needsRechamber)
						{
							isAttaching = true;
							player.animator.play("Attach_Start", false);

							updateAttach();

							startAttach();
						}
					}

					if (InputEx.GetKeyDown(ControlsSettings.reload))
					{
						if (!isShooting && !isReloading && !isHammering && !isUnjamming && !isSprinting && !isAttaching && !isAiming && !needsRechamber)
						{
							bool allowZeroCaliber = !equippedGunAsset.requiresNonZeroAttachmentCaliber;
							magazineSearchResults.Clear();
							player.inventory.FindAttachmentsByCaliber(magazineSearchResults, EItemType.MAGAZINE,
								equippedGunAsset.magazineCalibers, allowZeroCaliber);

							if (magazineSearchResults.Count > 0)
							{
								int highestAmount = 0;
								int indexOfHighestAmount = -1;

								for (int resultIndex = 0; resultIndex < magazineSearchResults.Count; resultIndex++)
								{
									if (magazineSearchResults[resultIndex].Jar.item.amount > highestAmount)
									{
										highestAmount = magazineSearchResults[resultIndex].Jar.item.amount;
										indexOfHighestAmount = resultIndex;
									}
								}

								if (indexOfHighestAmount >= 0)
								{
									ItemAsset pendingAsset = magazineSearchResults[indexOfHighestAmount].GetAsset();
									if (pendingAsset != null)
									{
										SendAttachMagazine.Invoke(GetNetId(), ENetReliability.Unreliable, magazineSearchResults[indexOfHighestAmount].Page, magazineSearchResults[indexOfHighestAmount].Jar.x, magazineSearchResults[indexOfHighestAmount].Jar.y, pendingAsset.hash);
									}
								}
							}
						}
					}

					if (InputEx.GetKeyDown(ControlsSettings.firemode))
					{
						if (!isAiming)
						{
							if (firemode == EFiremode.SAFETY)
							{
								if (equippedGunAsset.hasSemi)
								{
									SendChangeFiremode.Invoke(GetNetId(), ENetReliability.Reliable, EFiremode.SEMI);
								}
								else if (equippedGunAsset.hasAuto)
								{
									SendChangeFiremode.Invoke(GetNetId(), ENetReliability.Reliable, EFiremode.AUTO);
								}
								else if (equippedGunAsset.hasBurst)
								{
									SendChangeFiremode.Invoke(GetNetId(), ENetReliability.Reliable, EFiremode.BURST);
								}

								PlayerUI.message(EPlayerMessage.NONE, "");
							}
							else if (firemode == EFiremode.SEMI)
							{
								if (equippedGunAsset.hasAuto)
								{
									SendChangeFiremode.Invoke(GetNetId(), ENetReliability.Reliable, EFiremode.AUTO);
								}
								else if (equippedGunAsset.hasBurst)
								{
									SendChangeFiremode.Invoke(GetNetId(), ENetReliability.Reliable, EFiremode.BURST);
								}
								else if (equippedGunAsset.hasSafety)
								{
									SendChangeFiremode.Invoke(GetNetId(), ENetReliability.Reliable, EFiremode.SAFETY);

									PlayerUI.message(EPlayerMessage.SAFETY, "");
								}
							}
							else if (firemode == EFiremode.AUTO)
							{
								if (equippedGunAsset.hasBurst)
								{
									SendChangeFiremode.Invoke(GetNetId(), ENetReliability.Reliable, EFiremode.BURST);
								}
								else if (equippedGunAsset.hasSafety)
								{
									SendChangeFiremode.Invoke(GetNetId(), ENetReliability.Reliable, EFiremode.SAFETY);

									PlayerUI.message(EPlayerMessage.SAFETY, "");
								}
								else if (equippedGunAsset.hasSemi)
								{
									SendChangeFiremode.Invoke(GetNetId(), ENetReliability.Reliable, EFiremode.SEMI);
								}
							}
							else if (firemode == EFiremode.BURST)
							{
								if (equippedGunAsset.hasSafety)
								{
									SendChangeFiremode.Invoke(GetNetId(), ENetReliability.Reliable, EFiremode.SAFETY);

									PlayerUI.message(EPlayerMessage.SAFETY, "");
								}
								else if (equippedGunAsset.hasSemi)
								{
									SendChangeFiremode.Invoke(GetNetId(), ENetReliability.Reliable, EFiremode.SEMI);
								}
								else if (equippedGunAsset.hasAuto)
								{
									SendChangeFiremode.Invoke(GetNetId(), ENetReliability.Reliable, EFiremode.AUTO);
								}
							}
						}
					}
				}

				if (isAttaching)
				{
					if (sightButton != null)
					{
						if (player.look.perspective == EPlayerPerspective.FIRST && !equippedGunAsset.isTurret)
						{
							Vector3 sightHook = player.animator.viewmodelCamera.WorldToViewportPoint(firstAttachments.sightHook.position + (firstAttachments.sightHook.up * 0.05f) + (firstAttachments.sightHook.forward * 0.05f));
							Vector2 sightPosition = PlayerUI.container.ViewportToNormalizedPosition(sightHook);

							sightButton.PositionScale_X = sightPosition.x;
							sightButton.PositionScale_Y = sightPosition.y;
						}
						else
						{
							sightButton.PositionScale_X = 0.667f;//0.167f;
							sightButton.PositionScale_Y = 0.75f;
						}
					}

					if (tacticalButton != null)
					{
						if (player.look.perspective == EPlayerPerspective.FIRST && !equippedGunAsset.isTurret)
						{
							Vector3 tacticalHook = player.animator.viewmodelCamera.WorldToViewportPoint(firstAttachments.tacticalHook.position);
							Vector2 tacticalPosition = PlayerUI.container.ViewportToNormalizedPosition(tacticalHook);

							tacticalButton.PositionScale_X = tacticalPosition.x;
							tacticalButton.PositionScale_Y = tacticalPosition.y;
						}
						else
						{
							tacticalButton.PositionScale_X = 0.5f;//0.334f;
							tacticalButton.PositionScale_Y = 0.25f;
						}
					}

					if (gripButton != null)
					{
						if (player.look.perspective == EPlayerPerspective.FIRST && !equippedGunAsset.isTurret)
						{
							Vector3 gripHook = player.animator.viewmodelCamera.WorldToViewportPoint(firstAttachments.gripHook.position + (firstAttachments.gripHook.forward * -0.05f));
							Vector2 gripPosition = PlayerUI.container.ViewportToNormalizedPosition(gripHook);

							gripButton.PositionScale_X = gripPosition.x;
							gripButton.PositionScale_Y = gripPosition.y;
						}
						else
						{
							gripButton.PositionScale_X = 0.75f;//0.5f;
							gripButton.PositionScale_Y = 0.25f;
						}
					}

					if (barrelButton != null)
					{
						if (player.look.perspective == EPlayerPerspective.FIRST && !equippedGunAsset.isTurret)
						{
							Vector3 barrelHook = player.animator.viewmodelCamera.WorldToViewportPoint(firstAttachments.barrelHook.position + (firstAttachments.barrelHook.up * 0.05f));
							Vector2 barrelPosition = PlayerUI.container.ViewportToNormalizedPosition(barrelHook);

							barrelButton.PositionScale_X = barrelPosition.x;
							barrelButton.PositionScale_Y = barrelPosition.y;
						}
						else
						{
							barrelButton.PositionScale_X = 0.25f;//0.667f;
							barrelButton.PositionScale_Y = 0.25f;
						}
					}

					if (magazineButton != null)
					{
						if (player.look.perspective == EPlayerPerspective.FIRST && !equippedGunAsset.isTurret)
						{
							Vector2 magazineHook = player.animator.viewmodelCamera.WorldToViewportPoint(firstAttachments.magazineHook.position + (firstAttachments.magazineHook.forward * -0.1f));
							Vector2 magazinePosition = PlayerUI.container.ViewportToNormalizedPosition(magazineHook);

							magazineButton.PositionScale_X = magazinePosition.x;
							magazineButton.PositionScale_Y = magazinePosition.y;
						}
						else
						{
							magazineButton.PositionScale_X = 0.334f;//0.834f;
							magazineButton.PositionScale_Y = 0.75f;
						}
					}
				}

				if (rangeLabel != null)
				{
					if (PlayerLifeUI.scopeOverlay.IsVisible)
					{
						rangeLabel.PositionOffset_X = -300;
						rangeLabel.PositionOffset_Y = 100;
						rangeLabel.PositionScale_X = 0.5f;
						rangeLabel.PositionScale_Y = 0.5f;

						rangeLabel.TextAlignment = TextAnchor.UpperRight;
					}
					else
					{
						Vector3 lightHook;
						if (player.look.perspective == EPlayerPerspective.FIRST && firstAttachments.lightHook != null)
						{
							lightHook = player.animator.viewmodelCamera.WorldToViewportPoint(firstAttachments.lightHook.position);
						}
						else if (thirdAttachments.lightHook != null)
						{
							lightHook = MainCamera.instance.WorldToViewportPoint(thirdAttachments.lightHook.position);
						}
						else
						{
							lightHook = Vector3.zero;
						}

						Vector2 position = PlayerLifeUI.container.ViewportToNormalizedPosition(lightHook);

						rangeLabel.PositionOffset_X = -100;
						rangeLabel.PositionOffset_Y = -15;
						rangeLabel.PositionScale_X = position.x;
						rangeLabel.PositionScale_Y = position.y;

						rangeLabel.TextAlignment = TextAnchor.MiddleCenter;
					}
					rangeLabel.IsVisible = true;
				}
			}

			if (needsRechamber && Time.realtimeSinceStartup - lastShot > equippedGunAsset.RechamberAfterShotDelay && !isAiming)
			{
				needsRechamber = false;
				player.equipment.isBusy = false;
				lastRechamber = Time.realtimeSinceStartup;

				if (equippedGunAsset.CasingEjectCountAfterRechamberingAfterShooting > 0)
				{
					needsEject = true;
				}
				hammer();
			}

			if (needsEject && Time.realtimeSinceStartup - lastRechamber > equippedGunAsset.EjectAfterHammerDelay)
			{
				needsEject = false;

				if (firstShellEmitter != null && player.look.perspective == EPlayerPerspective.FIRST && !equippedGunAsset.isTurret)
				{
					firstShellEmitter.Emit(equippedGunAsset.CasingEjectCountAfterRechamberingAfterShooting);
				}

				if (thirdShellEmitter != null)
				{
					thirdShellEmitter.Emit(equippedGunAsset.CasingEjectCountAfterRechamberingAfterShooting);
				}
			}

			if (needsUnload && Time.realtimeSinceStartup - startedReload > equippedGunAsset.EjectAfterReloadDelay)
			{
				needsUnload = false;

				if (firstShellEmitter != null && player.look.perspective == EPlayerPerspective.FIRST && !equippedGunAsset.isTurret)
				{
					firstShellEmitter.Emit(equippedGunAsset.CasingEjectCountAfterReload);
				}

				if (thirdShellEmitter != null)
				{
					thirdShellEmitter.Emit(equippedGunAsset.CasingEjectCountAfterReload);
				}
			}

			if (needsUnplace && Time.realtimeSinceStartup - startedReload > reloadTime * equippedGunAsset.unplace)
			{
				needsUnplace = false;

				if (channel.IsLocalPlayer && firstAttachments.magazineModel != null)
				{
					firstAttachments.magazineModel.gameObject.SetActive(false);
				}

				if (thirdAttachments.magazineModel != null)
				{
					thirdAttachments.magazineModel.gameObject.SetActive(false);
				}

				foreach (UseableGunEventHook eventComponent in EnumerateEventComponents())
					eventComponent.OnMagazineHidden?.TryInvoke(this);
			}

			if (needsReplace && Time.realtimeSinceStartup - startedReload > reloadTime * equippedGunAsset.replace)
			{
				needsReplace = false;

				if (channel.IsLocalPlayer && firstAttachments.magazineModel != null)
				{
					firstAttachments.magazineModel.gameObject.SetActive(true);
				}

				if (thirdAttachments.magazineModel != null)
				{
					thirdAttachments.magazineModel.gameObject.SetActive(true);
				}

				foreach (UseableGunEventHook eventComponent in EnumerateEventComponents())
					eventComponent.OnMagazineVisible?.TryInvoke(this);
			}

			if (isReloading && Time.realtimeSinceStartup - startedReload > reloadTime)
			{
				isReloading = false;

				if (needsHammer)
				{
					hammer();
				}
				else
				{
					player.equipment.isBusy = false;
				}
			}

			if (isHammering && Time.realtimeSinceStartup - startedHammer > hammerTime)
			{
				isHammering = false;

				player.equipment.isBusy = false;
			}

			if (isUnjamming && Time.realtimeSinceStartup - startedUnjammingChamber > unjamChamberDuration)
			{
				isUnjamming = false;
				player.equipment.isBusy = false;
			}
		}

		public override void simulate(uint simulation, bool inputSteady)
		{
			// Timer prevents canceling gunshot audio and effects.
			// Previously used "tock" cycle, but that could effectively take zero seconds on server.
			if (isFired && Time.realtimeSinceStartup - lastShot > 0.15f)
			{
				isFired = false;

				// Nelson 2024-04-30: Mark not busy *after* rechamber. (public issue #4440)
				if (!needsRechamber)
				{
					player.equipment.isBusy = false;
				}
			}

			if (!canSteady)
			{
				if (!inputSteady && player.life.oxygen > 10)
				{
					canSteady = true;
				}
			}

			if (isAiming && thirdAttachments.sightAsset != null && thirdAttachments.sightAsset.zoom > 2.0f && player.life.oxygen > 0 && canSteady && inputSteady)
			{
				if (steadyAccuracy < 4)
				{
					steadyAccuracy++;
				}

				player.life.askSuffocate((byte) (5 - (player.skills.skills[(int) EPlayerSpeciality.OFFENSE][(int) EPlayerOffense.DIVING].level / 2)));
				if (player.life.oxygen == 0)
				{
					canSteady = false;
				}
			}
			else
			{
				if (steadyAccuracy > 0)
				{
					steadyAccuracy--;
				}
			}

			if (channel.IsLocalPlayer && player.equipment.IsEquipAnimationFinished)
			{
				if (fireTacticalInput)
				{
					if (!isReloading && !isHammering && !isUnjamming && !needsRechamber)
					{
						if (thirdAttachments.tacticalAsset != null)
						{
							if (thirdAttachments.tacticalAsset.isMelee)
							{
								if (!isSprinting && (!player.movement.isSafe || !player.movement.isSafeInfo.noWeapons) && firemode != EFiremode.SAFETY)
								{
									if (!Provider.isServer)
									{
										isJabbing = true;
									}

									player.input.keys[8] = true;
								}
							}
							else if (thirdAttachments.tacticalAsset.isLight || thirdAttachments.tacticalAsset.isLaser || thirdAttachments.tacticalAsset.isRangefinder)
							{
								player.input.keys[8] = true;
							}
						}
					}

					fireTacticalInput = false;
				}
			}

			if (Provider.isServer)
			{
				if (player.input.keys[8])
				{
					askInteractGun();
				}
			}
		}

		private void tockShoot(uint clock)
		{
			bool cancelShooting = firemode == EFiremode.SAFETY;
			cancelShooting |= isReloading;
			cancelShooting |= isHammering;
			cancelShooting |= isUnjamming;
			cancelShooting |= player.stance.stance == EPlayerStance.SPRINT && equippedGunAsset.canAimDuringSprint == false;
			cancelShooting |= isAttaching;

			// Cancel if we walked into water while shooting, otherwise isBusy lasts long enough to continue shooting underwater.
			cancelShooting |= (player.equipment.asset.canUseUnderwater == false) && (player.stance.isSubmerged || player.stance.stance == EPlayerStance.SWIM);

			if (cancelShooting)
			{
				bursts = 0;
				fireDelayCounter = 0;
				isShooting = false;
				wasTriggerJustPulled = false;

				return;
			}

			bool wantedToShoot = isShooting || wasTriggerJustPulled;
			wasTriggerJustPulled = false;

			// We do not return when we hit zero, so that if the player quickly tapped a delayed weapon it will still shoot.
			if (fireDelayCounter > 1)
			{
				--fireDelayCounter;
				return;
			}
			else if (fireDelayCounter > 0)
			{
				fireDelayCounter = 0;
				wantedToShoot = true;
			}

			if (firemode == EFiremode.SEMI)
			{
				isShooting = false;
			}

			if (firemode == EFiremode.BURST)
			{
				isShooting = false;

				if (wantedToShoot)
				{
					bursts += equippedGunAsset.bursts;
				}
			}

			int fireRateTicks = equippedGunAsset.firerate;
			if (thirdAttachments.sightAsset != null)
			{
				fireRateTicks -= thirdAttachments.sightAsset.FirerateOffset;
			}
			if (thirdAttachments.tacticalAsset != null && shouldEnableTacticalStats)
			{
				fireRateTicks -= thirdAttachments.tacticalAsset.FirerateOffset;
			}
			if (thirdAttachments.gripAsset != null)
			{
				fireRateTicks -= thirdAttachments.gripAsset.FirerateOffset;
			}
			if (thirdAttachments.barrelAsset != null)
			{
				fireRateTicks -= thirdAttachments.barrelAsset.FirerateOffset;
			}
			if (thirdAttachments.magazineAsset != null)
			{
				fireRateTicks -= thirdAttachments.magazineAsset.FirerateOffset;
			}
			fireRateTicks = Mathf.Max(fireRateTicks, 0);

			if (clock - lastFire > fireRateTicks)
			{
				if (bursts > 0)
				{
					--bursts;
				}

				if (ammo >= equippedGunAsset.ammoPerShot)
				{
					isFired = true;
					lastFire = clock;

					player.equipment.isBusy = true;

					fire();
				}
				else
				{
					if (Provider.isServer)
					{
						EffectManager.TriggerFiremodeEffect(transform.position);
					}

					bursts = 0;
					isShooting = false;
				}
			}
		}

		private void tockJab(uint clock)
		{
			isJabbing = false;

			if (clock - lastJab > 25)
			{
				lastJab = clock;

				jab();
			}
		}

		public override void tock(uint clock)
		{
			if (isShooting || wasTriggerJustPulled || bursts > 0 || fireDelayCounter > 0)
			{
				tockShoot(clock);
			}

			if (isJabbing)
			{
				tockJab(clock);
			}

			ballistics();

			if (isAiming)
			{
				if (_aimAccuracy < maxAimingAccuracy)
				{
					AimAccuracy = _aimAccuracy + 1;
				}
			}
			else
			{
				if (_aimAccuracy > 0)
				{
					AimAccuracy = _aimAccuracy - 1;
				}
			}
		}

		public override void updateState(byte[] newState)
		{
			ammo = newState[10];
			firemode = (EFiremode) newState[11];
			interact = newState[12] == 1;

			bool wasMagazineModelVisible = thirdAttachments.magazineModel != null && thirdAttachments.magazineModel.gameObject.activeSelf;

			if (channel.IsLocalPlayer)
			{
				firstAttachments.updateAttachments(newState, true);
			}

			thirdAttachments.updateAttachments(newState, false);
			updateAttachments(wasMagazineModelVisible);

			// Nelson 2025-02-07: When reloading from empty there *was* no magazine model (not visible) and a new one
			// is instantiated invisible until "replace" makes it visible again.
			if (!wasMagazineModelVisible)
			{
				foreach (UseableGunEventHook eventComponent in EnumerateEventComponents())
					eventComponent.OnMagazineHidden?.TryInvoke(this);
			}

			if (channel.IsLocalPlayer)
			{
				if (firstAttachments.reticuleHook != null)
				{
					originalReticuleHookLocalPosition = firstAttachments.reticuleHook.localPosition;
				}
				else
				{
					originalReticuleHookLocalPosition = Vector3.zero;
				}
			}

			if (infoBox != null)
			{
				if (isAttaching)
				{
					updateAttach();
				}

				updateInfo();
			}
		}

		private void updateAnimationSpeeds(float speed)
		{
			player.animator.setAnimationSpeed("Reload", speed);
			reloadTime = player.animator.GetAnimationLength("Reload");
			reloadTime = Mathf.Max(reloadTime, equippedGunAsset.reloadTime / speed);

			player.animator.setAnimationSpeed("Hammer", speed);
			hammerTime = player.animator.GetAnimationLength("Hammer");
			player.animator.setAnimationSpeed("Scope", speed);
			hammerTime = Mathf.Max(hammerTime, equippedGunAsset.hammerTime / speed);

			unjamChamberDuration = player.animator.GetAnimationLength(equippedGunAsset.unjamChamberAnimName);
		}

		private void updateAttachments(bool wasMagazineModelVisible)
		{
			if (channel.IsLocalPlayer)
			{
				ClientAssetIntegrity.QueueRequest(firstAttachments.sightAsset);
				ClientAssetIntegrity.QueueRequest(firstAttachments.tacticalAsset);
				ClientAssetIntegrity.QueueRequest(firstAttachments.gripAsset);
				ClientAssetIntegrity.QueueRequest(firstAttachments.barrelAsset);
				ClientAssetIntegrity.QueueRequest(firstAttachments.magazineAsset);

				if (firstAttachments.tacticalAsset != null)
				{
					if (firstAttachments.tacticalAsset.isLaser)
					{
						if (!wasLaser)
						{
							PlayerUI.message(EPlayerMessage.LASER, "");
						}

						wasLaser = true;
					}
					else
					{
						wasLaser = false;
					}

					if (firstAttachments.tacticalAsset.isLight)
					{
						if (!wasLight)
						{
							PlayerUI.message(EPlayerMessage.LIGHT, "");
						}

						wasLight = true;
					}
					else
					{
						wasLight = false;
					}

					if (firstAttachments.tacticalAsset.isRangefinder)
					{
						if (!wasRange)
						{
							PlayerUI.message(EPlayerMessage.RANGEFINDER, "");
						}

						wasRange = true;
					}
					else
					{
						wasRange = false;
					}

					if (firstAttachments.tacticalAsset.isMelee)
					{
						if (!wasBayonet)
						{
							PlayerUI.message(EPlayerMessage.BAYONET, "");
						}

						wasBayonet = true;
					}
					else
					{
						wasBayonet = false;
					}
				}
				else
				{
					wasLaser = false;
					wasLight = false;
					wasRange = false;
					wasBayonet = false;
				}

#if !DEDICATED_SERVER
				ClearScopeDistanceMarkers();
				if (equippedGunAsset.projectile == null // Not supported for rocket launchers.
					&& firstAttachments.sightAsset != null
					&& firstAttachments.sightAsset.distanceMarkers != null
					&& firstAttachments.sightAsset.distanceMarkers.Count > 0)
				{
					InstantiateScopeDistanceMarkers();
				}

				if (firstAttachments.tacticalAsset != null && firstAttachments.tacticalAsset.isLaser && interact)
				{
					if (laserGameObject == null)
					{
						GameObject laserPrefab = Assets.coreMasterBundle.LoadAsset<GameObject>("Guns/Laser.prefab");
						laserGameObject = Instantiate(laserPrefab);
						laserTransform = laserGameObject.transform;
						laserTransform.name = "Laser";
						laserTransform.position = Vector3.zero;
						laserTransform.rotation = Quaternion.identity;
						laserMaterial = laserGameObject.GetComponent<Renderer>().material;
					}

					laserMaterial.SetColor("_Color", firstAttachments.tacticalAsset.laserColor);
					laserMaterial.SetColor("_EmissionColor", firstAttachments.tacticalAsset.laserColor * 2.0f);
				}
				else if (laserGameObject != null)
				{
					DestroyLaser();
				}
#endif // !DEDICATED_SERVER

				if (firstAttachments.tacticalAsset != null && firstAttachments.tacticalAsset.isRangefinder && interact)
				{
					if (rangeLabel == null)
					{
						rangeLabel = Glazier.Get().CreateLabel();
						rangeLabel.SizeOffset_X = 200;
						rangeLabel.SizeOffset_Y = 30;
						rangeLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
						PlayerUI.window.AddChild(rangeLabel);
						rangeLabel.IsVisible = false;
					}
				}
				else if (rangeLabel != null)
				{
					rangeLabel.Parent.RemoveChild(rangeLabel);
					rangeLabel = null;
				}

				if (firstFakeLight_0 != null)
				{
					Destroy(firstFakeLight_0.gameObject);
					firstFakeLight_0 = null;
				}

				if (thirdAttachments.lightHook != null)
				{
					Transform light = thirdAttachments.lightHook.Find("Light");
					if (light != null)
					{
						firstFakeLight_0 = GameObject.Instantiate(light.gameObject).transform;
						firstFakeLight_0.name = "Emitter";
					}
				}

				if (firstFakeLight_1 != null)
				{
					Destroy(firstFakeLight_1.gameObject);
					firstFakeLight_1 = null;
				}

				if (thirdAttachments.light2Hook != null)
				{
					Transform light = thirdAttachments.light2Hook.Find("Light");
					if (light != null)
					{
						firstFakeLight_1 = GameObject.Instantiate(light.gameObject).transform;
						firstFakeLight_1.name = "Emitter";
					}
				}
			}

			if (firstMuzzleEmitter != null)
			{
				if (firstAttachments.barrelModel != null)
				{
					// Optional transform to override muzzle flash position.
					Transform muzzleTransform = firstAttachments.barrelModel.Find("Muzzle");
					if (muzzleTransform != null)
					{
						firstMuzzleEmitter.transform.position = muzzleTransform.position;
					}
					else
					{
						firstMuzzleEmitter.transform.localPosition = Vector3.up * 0.25f;
					}
				}
				else
				{
					firstMuzzleEmitter.transform.localPosition = Vector3.zero;
				}
			}

			if (thirdMuzzleEmitter != null)
			{
				if (thirdAttachments.barrelModel != null)
				{
					// Optional transform to override muzzle flash position.
					Transform muzzleTransform = thirdAttachments.barrelModel.Find("Muzzle");
					if (muzzleTransform != null)
					{
						thirdMuzzleEmitter.transform.position = muzzleTransform.position;
					}
					else
					{
						thirdMuzzleEmitter.transform.localPosition = Vector3.up * 0.25f;
					}
				}
				else
				{
					thirdMuzzleEmitter.transform.localPosition = Vector3.zero;
				}
			}

			// 2023-05-25: hack only update tracer effect when we have a magazine, otherwise arrow tracer is removed
			// immediately after shooting. In the future this should be updated to create the tracer per-bullet.
			if (thirdAttachments?.magazineAsset != null)
			{
				EffectAsset newTracerEffectAsset = thirdAttachments.magazineAsset.FindTracerEffectAsset();
				if (currentTracerEffectAsset != newTracerEffectAsset)
				{
					if (tracerEmitter != null)
					{
						EffectManager.DestroyIntoPool(tracerEmitter.gameObject);
						tracerEmitter = null;
					}

					currentTracerEffectAsset = newTracerEffectAsset;
					if (newTracerEffectAsset != null && newTracerEffectAsset.effect != null)
					{
						Transform emitter = EffectManager.InstantiateFromPool(newTracerEffectAsset).transform;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
						emitter.name = $"{newTracerEffectAsset.FriendlyName} (tracer)";
#else // UNITY_EDITOR || DEVELOPMENT_BUILD
						emitter.name = "Tracer";
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
						emitter.localPosition = Vector3.zero;
						emitter.localRotation = Quaternion.identity;

						tracerEmitter = emitter.GetComponent<ParticleSystem>();
					}
				}
			}

			// Match previous visibility for askReload.
			if (channel.IsLocalPlayer && firstAttachments.magazineModel != null)
			{
				firstAttachments.magazineModel.gameObject.SetActive(wasMagazineModelVisible);
			}
			if (thirdAttachments.magazineModel != null)
			{
				thirdAttachments.magazineModel.gameObject.SetActive(wasMagazineModelVisible);
			}

			if (!Dedicator.IsDedicatedServer)
			{
				if (thirdAttachments.tacticalAsset != null)
				{
					if (thirdAttachments.tacticalAsset.isLight || thirdAttachments.tacticalAsset.isLaser)
					{
						if (channel.IsLocalPlayer && firstAttachments.lightHook != null)
						{
							firstAttachments.lightHook.gameObject.SetActive(interact);
						}

						if (thirdAttachments.lightHook != null)
						{
							thirdAttachments.lightHook.gameObject.SetActive(interact);
						}

						if (firstFakeLight_0 != null)
						{
							firstFakeLight_0.gameObject.SetActive(interact);
						}
					}
					else if (thirdAttachments.tacticalAsset.isRangefinder)
					{
						if (channel.IsLocalPlayer)
						{
							if (firstAttachments.lightHook != null)
							{
								firstAttachments.lightHook.gameObject.SetActive(inRange && interact);
							}
							if (firstAttachments.light2Hook != null)
							{
								firstAttachments.light2Hook.gameObject.SetActive(!inRange && interact);
							}

							if (thirdAttachments.lightHook != null)
							{
								thirdAttachments.lightHook.gameObject.SetActive(inRange && interact);
							}
							if (thirdAttachments.light2Hook != null)
							{
								thirdAttachments.light2Hook.gameObject.SetActive(!inRange && interact);
							}
						}

						if (firstFakeLight_0 != null)
						{
							firstFakeLight_0.gameObject.SetActive(inRange && interact);
						}

						if (firstFakeLight_1 != null)
						{
							firstFakeLight_1.gameObject.SetActive(!inRange && interact);
						}
					}
				}
			}

			if (thirdAttachments.tacticalAsset != null && thirdAttachments.tacticalAsset.isLight && interact)
			{
				player.enableItemSpotLight(thirdAttachments.tacticalAsset.lightConfig);
			}
			else
			{
				player.disableItemSpotLight();
			}

			if (channel.IsLocalPlayer)
			{
				float restoreScopeAlpha = player.look.scopeAlpha;

				if (firstAttachments.sightAsset != null)
				{
					firstPersonZoomFactor = firstAttachments.sightAsset.zoom;
					thirdPersonZoomFactor = thirdAttachments.sightAsset.thirdPersonZoomFactor;
					shouldZoomUsingEyes = firstAttachments.sightAsset.shouldZoomUsingEyes;

					if (firstAttachments.scopeHook != null)
					{
						player.look.enableScope(firstPersonZoomFactor, firstAttachments.sightAsset);
						player.animator.viewmodelOffsetPreferenceUseScope = true;

						Renderer scopeRenderer = firstAttachments?.scopeHook?.GetComponent<Renderer>();
						if (scopeRenderer != null)
						{
							scopeRenderer.sharedMaterial = player.look.scopeMaterial;
							scopeRenderer.enabled = true;
							UpdateScopeAlpha();
						}

						firstAttachments.scopeHook.gameObject.SetActive(true);

						if (channel.owner.IsLeftHanded)
						{
							Vector3 scale = firstAttachments.scopeHook.localScale;
							scale.x *= -1;

							firstAttachments.scopeHook.localScale = scale;
						}
					}
					else
					{
						player.look.disableScope();
						player.animator.viewmodelOffsetPreferenceUseScope = false;
					}
				}
				else
				{
					firstPersonZoomFactor = 1.0f;
					thirdPersonZoomFactor = DEFAULT_THIRD_PERSON_ZOOM_FACTOR;
					shouldZoomUsingEyes = false;
					player.look.disableScope();
					player.animator.viewmodelOffsetPreferenceUseScope = false;
				}

				UpdateCrosshairEnabled();

				// Nelson 2025-08-13: this is pretty hacky, but if player was already aiming we want to restore scope
				// alpha (reset by disableScope) to prevent a one frame flicker. (public issue #5153)
				player.look.scopeAlpha = restoreScopeAlpha;
			}

			UpdateMovementSpeedMultiplier();
			UpdateAimInDuration();
		}

#if !DEDICATED_SERVER
		private static StaticResourceRef<Material> unzoomedScopeMaterial = new StaticResourceRef<Material>("Materials/UnzoomedScope");
#endif

		private void applyRecoilMagnitudeModifiers(ref float value)
		{
			if (player.stance.stance == EPlayerStance.SPRINT)
			{
				value *= equippedGunAsset.recoilSprint;
			}
			else if (player.stance.stance == EPlayerStance.CROUCH)
			{
				value *= equippedGunAsset.recoilCrouch;
			}
			else if (player.stance.stance == EPlayerStance.PRONE)
			{
				value *= equippedGunAsset.recoilProne;
			}
			else if (player.stance.stance == EPlayerStance.SWIM)
			{
				value *= equippedGunAsset.recoilSwimming;
			}

			if (!player.movement.isGrounded)
			{
				value *= equippedGunAsset.recoilMidair;
			}
		}

		/// <summary>
		/// Note: This is the m/s² acceleration, not the multiplier.
		/// </summary>
		internal float CalculateBulletGravity()
		{
			return Physics.gravity.y * CalculateBulletGravityMultiplier();
		}

		internal float CalculateSpreadAngleRadians()
		{
			float quality = player.equipment.quality / 100f;
			float aimAlpha = GetInterpolatedAimAlpha();
			return CalculateSpreadAngleRadians(quality, aimAlpha);
		}

		internal float CalculateSpreadAngleRadians(float quality, float aimAlpha)
		{
			float spread = equippedGunAsset.baseSpreadAngleRadians;
			spread *= quality < 0.5f ? 1f + (1f - (quality * 2f)) : 1f;

			spread *= Mathf.Lerp(1.0f, equippedGunAsset.spreadAim, aimAlpha);

			spread *= player.skills.GetSharpshooterRecoilMultiplier();

			if (thirdAttachments.sightAsset != null && (!thirdAttachments.sightAsset.ShouldOnlyAffectAimWhileProne || player.stance.stance == EPlayerStance.PRONE))
			{
				spread *= Mathf.Lerp(1.0f, thirdAttachments.sightAsset.spread, aimAlpha);
			}

			if (thirdAttachments.tacticalAsset != null && shouldEnableTacticalStats && (!thirdAttachments.tacticalAsset.ShouldOnlyAffectAimWhileProne || player.stance.stance == EPlayerStance.PRONE))
			{
				spread *= thirdAttachments.tacticalAsset.spread;
			}

			if (thirdAttachments.gripAsset != null && (!thirdAttachments.gripAsset.ShouldOnlyAffectAimWhileProne || player.stance.stance == EPlayerStance.PRONE))
			{
				spread *= thirdAttachments.gripAsset.spread;
			}

			if (thirdAttachments.barrelAsset != null && (!thirdAttachments.barrelAsset.ShouldOnlyAffectAimWhileProne || player.stance.stance == EPlayerStance.PRONE))
			{
				spread *= thirdAttachments.barrelAsset.spread;
			}

			if (thirdAttachments.magazineAsset != null && (!thirdAttachments.magazineAsset.ShouldOnlyAffectAimWhileProne || player.stance.stance == EPlayerStance.PRONE))
			{
				spread *= thirdAttachments.magazineAsset.spread;
			}

			if (player.stance.stance == EPlayerStance.SPRINT)
			{
				spread *= equippedGunAsset.spreadSprint;
			}
			else if (player.stance.stance == EPlayerStance.CROUCH)
			{
				spread *= equippedGunAsset.spreadCrouch;
			}
			else if (player.stance.stance == EPlayerStance.PRONE)
			{
				spread *= equippedGunAsset.spreadProne;
			}
			else if (player.stance.stance == EPlayerStance.SWIM)
			{
				spread *= equippedGunAsset.spreadSwimming;
			}

			if (player.look.perspective == EPlayerPerspective.THIRD)
			{
				spread *= Provider.modeConfigData.Gameplay.ThirdPerson_SpreadMultiplier;
			}

			if (!player.movement.isGrounded)
			{
				spread *= equippedGunAsset.spreadMidair;
			}

			return spread;
		}

		internal void UpdateCrosshairEnabled()
		{
			if ((!equippedGunAsset.isTurret && equippedGunAsset.action != EAction.Minigun && ((isAiming && player.look.perspective == EPlayerPerspective.FIRST && (equippedGunAsset.action != EAction.String || thirdAttachments.sightHook != null)) || isAttaching)) || (player.movement.getVehicle() != null && player.look.perspective != EPlayerPerspective.FIRST))
			{
				PlayerUI.disableCrosshair();
			}
			else
			{
				PlayerUI.enableCrosshair();
			}
		}

		private void updateAttach()
		{
			if (sightButton != null)
			{
				bool allowZeroCaliber = !equippedGunAsset.requiresNonZeroAttachmentCaliber;
				sightSearchResults.Clear();
				player.inventory.FindAttachmentsByCaliber(sightSearchResults, EItemType.SIGHT,
					equippedGunAsset.attachmentCalibers, allowZeroCaliber);

				if (sightJars != null)
				{
					sightButton.RemoveChild(sightJars);
				}

				sightJars = new SleekJars(100f, sightSearchResults);
				sightJars.SizeScale_X = 1f;
				sightJars.SizeScale_Y = 1f;
				sightJars.onClickedJar = onClickedSightJar;
				sightButton.AddChild(sightJars);

				if (thirdAttachments.sightAsset != null)
				{
					Color rarityColor = ItemTool.getRarityColorUI(thirdAttachments.sightAsset.rarity);
					sightButton.backgroundColor = SleekColor.BackgroundIfLight(rarityColor);
					sightButton.textColor = rarityColor;
					sightButton.tooltip = thirdAttachments.sightAsset.itemName;

					sightButton.iconColor = rarityColor;
				}
				else
				{
					sightButton.backgroundColor = ESleekTint.BACKGROUND;
					sightButton.textColor = ESleekTint.FOREGROUND;
					sightButton.tooltip = localization.format("Sight_Hook_Tooltip");

					sightButton.iconColor = ESleekTint.FOREGROUND;
				}
			}

			if (tacticalButton != null)
			{
				bool allowZeroCaliber = !equippedGunAsset.requiresNonZeroAttachmentCaliber;
				tacticalSearchResults.Clear();
				player.inventory.FindAttachmentsByCaliber(tacticalSearchResults, EItemType.TACTICAL,
					equippedGunAsset.attachmentCalibers, allowZeroCaliber);

				if (tacticalJars != null)
				{
					tacticalButton.RemoveChild(tacticalJars);
				}

				tacticalJars = new SleekJars(100f, tacticalSearchResults);
				tacticalJars.SizeScale_X = 1f;
				tacticalJars.SizeScale_Y = 1f;
				tacticalJars.onClickedJar = onClickedTacticalJar;
				tacticalButton.AddChild(tacticalJars);

				if (thirdAttachments.tacticalAsset != null)
				{
					Color rarityColor = ItemTool.getRarityColorUI(thirdAttachments.tacticalAsset.rarity);
					tacticalButton.backgroundColor = SleekColor.BackgroundIfLight(rarityColor);
					tacticalButton.textColor = rarityColor;
					tacticalButton.tooltip = thirdAttachments.tacticalAsset.itemName;

					tacticalButton.iconColor = rarityColor;
				}
				else
				{
					tacticalButton.backgroundColor = ESleekTint.BACKGROUND;
					tacticalButton.textColor = ESleekTint.FOREGROUND;
					tacticalButton.tooltip = localization.format("Tactical_Hook_Tooltip");

					tacticalButton.iconColor = ESleekTint.FOREGROUND;
				}
			}

			if (gripButton != null)
			{
				bool allowZeroCaliber = !equippedGunAsset.requiresNonZeroAttachmentCaliber;
				gripSearchResults.Clear();
				player.inventory.FindAttachmentsByCaliber(gripSearchResults, EItemType.GRIP,
					equippedGunAsset.attachmentCalibers, allowZeroCaliber);

				if (gripJars != null)
				{
					gripButton.RemoveChild(gripJars);
				}

				gripJars = new SleekJars(100f, gripSearchResults);
				gripJars.SizeScale_X = 1f;
				gripJars.SizeScale_Y = 1f;
				gripJars.onClickedJar = onClickedGripJar;
				gripButton.AddChild(gripJars);

				if (thirdAttachments.gripAsset != null)
				{
					Color rarityColor = ItemTool.getRarityColorUI(thirdAttachments.gripAsset.rarity);
					gripButton.backgroundColor = SleekColor.BackgroundIfLight(rarityColor);
					gripButton.textColor = rarityColor;
					gripButton.tooltip = thirdAttachments.gripAsset.itemName;

					gripButton.iconColor = rarityColor;
				}
				else
				{
					gripButton.backgroundColor = ESleekTint.BACKGROUND;
					gripButton.textColor = ESleekTint.FOREGROUND;
					gripButton.tooltip = localization.format("Grip_Hook_Tooltip");

					gripButton.iconColor = ESleekTint.FOREGROUND;
				}
			}

			if (barrelButton != null)
			{
				bool allowZeroCaliber = !equippedGunAsset.requiresNonZeroAttachmentCaliber;
				barrelSearchResults.Clear();
				player.inventory.FindAttachmentsByCaliber(barrelSearchResults,
					EItemType.BARREL, equippedGunAsset.attachmentCalibers, allowZeroCaliber);

				if (barrelJars != null)
				{
					barrelButton.RemoveChild(barrelJars);
				}

				barrelJars = new SleekJars(100f, barrelSearchResults);
				barrelJars.SizeScale_X = 1f;
				barrelJars.SizeScale_Y = 1f;
				barrelJars.onClickedJar = onClickedBarrelJar;
				barrelButton.AddChild(barrelJars);

				if (thirdAttachments.barrelAsset != null)
				{
					Color rarityColor = ItemTool.getRarityColorUI(thirdAttachments.barrelAsset.rarity);
					barrelButton.backgroundColor = SleekColor.BackgroundIfLight(rarityColor);
					barrelButton.textColor = rarityColor;
					barrelButton.tooltip = thirdAttachments.barrelAsset.itemName;

					barrelButton.iconColor = rarityColor;
				}
				else
				{
					barrelButton.backgroundColor = ESleekTint.BACKGROUND;
					barrelButton.textColor = ESleekTint.FOREGROUND;
					barrelButton.tooltip = localization.format("Barrel_Hook_Tooltip");

					barrelButton.iconColor = ESleekTint.FOREGROUND;
				}

				if (thirdAttachments.barrelAsset != null && thirdAttachments.barrelAsset.showQuality)
				{
					barrelQualityImage.TintColor = ItemTool.getQualityColor(player.equipment.state[16] / 100.0f);

					barrelQualityLabel.Text = player.equipment.state[16] + "%";
					barrelQualityLabel.TextColor = barrelQualityImage.TintColor;

					barrelQualityLabel.IsVisible = true;
					barrelQualityImage.IsVisible = true;
				}
				else
				{
					barrelQualityLabel.IsVisible = false;
					barrelQualityImage.IsVisible = false;
				}
			}

			if (magazineButton != null)
			{
				bool allowZeroCaliber = !equippedGunAsset.requiresNonZeroAttachmentCaliber;
				magazineSearchResults.Clear();
				player.inventory.FindAttachmentsByCaliber(magazineSearchResults, EItemType.MAGAZINE,
					equippedGunAsset.magazineCalibers, allowZeroCaliber);

				if (magazineJars != null)
				{
					magazineButton.RemoveChild(magazineJars);
				}

				magazineJars = new SleekJars(100f, magazineSearchResults);
				magazineJars.SizeScale_X = 1f;
				magazineJars.SizeScale_Y = 1f;
				magazineJars.onClickedJar = onClickedMagazineJar;
				magazineButton.AddChild(magazineJars);

				if (thirdAttachments.magazineAsset != null)
				{
					Color rarityColor = ItemTool.getRarityColorUI(thirdAttachments.magazineAsset.rarity);
					magazineButton.backgroundColor = SleekColor.BackgroundIfLight(rarityColor);
					magazineButton.textColor = rarityColor;
					magazineButton.tooltip = thirdAttachments.magazineAsset.itemName;

					magazineButton.iconColor = rarityColor;
				}
				else
				{
					magazineButton.backgroundColor = ESleekTint.BACKGROUND;
					magazineButton.textColor = ESleekTint.FOREGROUND;
					magazineButton.tooltip = localization.format("Magazine_Hook_Tooltip");

					magazineButton.iconColor = ESleekTint.FOREGROUND;
				}

				if (thirdAttachments.magazineAsset != null && thirdAttachments.magazineAsset.showQuality)
				{
					magazineQualityImage.TintColor = ItemTool.getQualityColor(player.equipment.state[17] / 100.0f);

					magazineQualityLabel.Text = player.equipment.state[17] + "%";
					magazineQualityLabel.TextColor = magazineQualityImage.TintColor;

					magazineQualityLabel.IsVisible = true;
					magazineQualityImage.IsVisible = true;
				}
				else
				{
					magazineQualityLabel.IsVisible = false;
					magazineQualityImage.IsVisible = false;
				}
			}
		}

		private void updateInfo()
		{
			ammoLabel.TextColor = ammo < equippedGunAsset.ammoPerShot ? ESleekTint.BAD : ESleekTint.FONT;
			ammoLabel.Text = localization.format("Ammo", ammo, thirdAttachments.magazineAsset != null ? thirdAttachments.magazineAsset.MaxAmount : 0);

			if (firstAmmoCounter != null)
			{
				firstAmmoCounter.text = ammo.ToString();
			}
			if (thirdAmmoCounter != null)
			{
				thirdAmmoCounter.text = ammo.ToString();
			}

			if (firemode == EFiremode.SAFETY)
			{
				firemodeLabel.Text = localization.format("Firemode", localization.format("Safety"), ControlsSettings.firemode);
			}
			else if (firemode == EFiremode.SEMI)
			{
				firemodeLabel.Text = localization.format("Firemode", localization.format("Semi"), ControlsSettings.firemode);
			}
			else if (firemode == EFiremode.AUTO)
			{
				firemodeLabel.Text = localization.format("Firemode", localization.format("Auto"), ControlsSettings.firemode);
			}
			else if (firemode == EFiremode.BURST)
			{
				firemodeLabel.Text = localization.format("Firemode", localization.format("Burst"), ControlsSettings.firemode);
			}

			attachLabel.Text = localization.format("Attach", thirdAttachments.magazineAsset != null ? thirdAttachments.magazineAsset.itemName : localization.format("None"), ControlsSettings.attach);

			if (thirdAttachments.magazineAsset != null)
			{
				attachLabel.TextColor = ItemTool.getRarityColorUI(thirdAttachments.magazineAsset.rarity);
			}
			else
			{
				attachLabel.TextColor = ESleekTint.FONT;
			}
		}

		private void onPerspectiveUpdated(EPlayerPerspective newPerspective)
		{
			UpdateCrosshairEnabled();

			if (newPerspective == EPlayerPerspective.THIRD)
			{
				if (isAiming)
				{
					player.look.enableZoom(thirdPersonZoomFactor, true);
				}
			}
			else
			{
				if (isAiming)
				{
					if (equippedGunAsset.isTurret || equippedGunAsset.action == EAction.Minigun || shouldZoomUsingEyes)
					{
						player.look.enableZoom(firstPersonZoomFactor, true);
					}
					else
					{
						player.look.enableZoom(1.0f, true);
					}
				}
			}

			player.look.UpdateScopeOverlay();

			if (thirdShellRenderer != null)
			{
				thirdShellRenderer.forceRenderingOff = newPerspective == EPlayerPerspective.FIRST && !equippedGunAsset.isTurret;
			}
		}

		private void SyncScopeDistanceMarkerText()
		{
			foreach (DistanceMarker marker in scopeDistanceMarkers)
			{
				if (marker.textComponent == null)
				{
					// config.HasLabel is false
					continue;
				}

				if (OptionsSettings.metric)
				{
					marker.textComponent.text = $"{marker.distance} m";
				}
				else
				{
					marker.textComponent.text = $"{Mathf.RoundToInt(MeasurementTool.MtoYd(marker.distance))} yd";
				}
			}
		}

		private void onClickedSightHookButton(ISleekElement button)
		{
			SendAttachSight.Invoke(GetNetId(), ENetReliability.Unreliable, 255, 255, 255, new byte[0]);
		}

		private void onClickedTacticalHookButton(ISleekElement button)
		{
			SendAttachTactical.Invoke(GetNetId(), ENetReliability.Unreliable, 255, 255, 255, new byte[0]);
		}

		private void onClickedGripHookButton(ISleekElement button)
		{
			SendAttachGrip.Invoke(GetNetId(), ENetReliability.Unreliable, 255, 255, 255, new byte[0]);
		}

		private void onClickedBarrelHookButton(ISleekElement button)
		{
			SendAttachBarrel.Invoke(GetNetId(), ENetReliability.Unreliable, 255, 255, 255, new byte[0]);
		}

		private void onClickedMagazineHookButton(ISleekElement button)
		{
			SendAttachMagazine.Invoke(GetNetId(), ENetReliability.Unreliable, 255, 255, 255, new byte[0]);
		}

		private void onClickedSightJar(SleekJars jars, int index)
		{
			ItemAsset pendingAsset = sightSearchResults[index].GetAsset();
			if (pendingAsset == null)
				return;

			SendAttachSight.Invoke(GetNetId(), ENetReliability.Unreliable, sightSearchResults[index].Page, sightSearchResults[index].Jar.x, sightSearchResults[index].Jar.y, pendingAsset.hash);
		}

		private void onClickedTacticalJar(SleekJars jars, int index)
		{
			ItemAsset pendingAsset = tacticalSearchResults[index].GetAsset();
			if (pendingAsset == null)
				return;

			SendAttachTactical.Invoke(GetNetId(), ENetReliability.Unreliable, tacticalSearchResults[index].Page, tacticalSearchResults[index].Jar.x, tacticalSearchResults[index].Jar.y, pendingAsset.hash);
		}

		private void onClickedGripJar(SleekJars jars, int index)
		{
			ItemAsset pendingAsset = gripSearchResults[index].GetAsset();
			if (pendingAsset == null)
				return;

			SendAttachGrip.Invoke(GetNetId(), ENetReliability.Unreliable, gripSearchResults[index].Page, gripSearchResults[index].Jar.x, gripSearchResults[index].Jar.y, pendingAsset.hash);
		}

		private void onClickedBarrelJar(SleekJars jars, int index)
		{
			ItemAsset pendingAsset = barrelSearchResults[index].GetAsset();
			if (pendingAsset == null)
				return;

			SendAttachBarrel.Invoke(GetNetId(), ENetReliability.Unreliable, barrelSearchResults[index].Page, barrelSearchResults[index].Jar.x, barrelSearchResults[index].Jar.y, pendingAsset.hash);
		}

		private void onClickedMagazineJar(SleekJars jars, int index)
		{
			ItemAsset pendingAsset = magazineSearchResults[index].GetAsset();
			if (pendingAsset == null)
				return;

			SendAttachMagazine.Invoke(GetNetId(), ENetReliability.Unreliable, magazineSearchResults[index].Page, magazineSearchResults[index].Jar.x, magazineSearchResults[index].Jar.y, pendingAsset.hash);
		}

		private void startAim()
		{
			UpdateMovementSpeedMultiplier();

			if (channel.IsLocalPlayer)
			{
				if (!equippedGunAsset.isTurret && equippedGunAsset.action != EAction.Minigun)
				{
					if (player.look.IsUsing2DScope && firstAttachments.sightModel != null && firstAttachments.scopeHook != null && firstAttachments.scopeHook.Find("Reticule") != null)
					{
						// Special case for custom shaders "_ReticuleTexture" property without [MainTexture] attribute,
						// (public issue 5462) but there are still a lot of assumptions / possible null reference
						// exceptions here. :S
						Material reticuleMaterial = firstAttachments.scopeHook.Find("Reticule").GetComponent<Renderer>().sharedMaterial;
						Texture scope;
						if (reticuleMaterial.HasProperty(ReticuleTextureId))
						{
							scope = reticuleMaterial.GetTexture(ReticuleTextureId);
						}
						else
						{
							scope = reticuleMaterial.mainTexture;
						}

						if (scope.width <= 64)
						{
							PlayerLifeUI.scopeOverlay.scopeImage.PositionOffset_X = -scope.width / 2;
							PlayerLifeUI.scopeOverlay.scopeImage.PositionOffset_Y = -scope.height / 2;
							PlayerLifeUI.scopeOverlay.scopeImage.PositionScale_X = 0.5f;
							PlayerLifeUI.scopeOverlay.scopeImage.PositionScale_Y = 0.5f;
							PlayerLifeUI.scopeOverlay.scopeImage.SizeOffset_X = scope.width;
							PlayerLifeUI.scopeOverlay.scopeImage.SizeOffset_Y = scope.height;
							PlayerLifeUI.scopeOverlay.scopeImage.SizeScale_X = 0;
							PlayerLifeUI.scopeOverlay.scopeImage.SizeScale_Y = 0;
						}
						else
						{
							PlayerLifeUI.scopeOverlay.scopeImage.PositionOffset_X = 0;
							PlayerLifeUI.scopeOverlay.scopeImage.PositionOffset_Y = 0;
							PlayerLifeUI.scopeOverlay.scopeImage.PositionScale_X = 0;
							PlayerLifeUI.scopeOverlay.scopeImage.PositionScale_Y = 0;
							PlayerLifeUI.scopeOverlay.scopeImage.SizeOffset_X = 0;
							PlayerLifeUI.scopeOverlay.scopeImage.SizeOffset_Y = 0;

							if (firstAttachments.sightAsset.shouldOffsetScopeOverlayByOneTexel)
							{
								PlayerLifeUI.scopeOverlay.scopeImage.SizeScale_X = 1.0f + 1.0f / scope.width;
								PlayerLifeUI.scopeOverlay.scopeImage.SizeScale_Y = 1.0f + 1.0f / scope.height;
							}
							else
							{
								PlayerLifeUI.scopeOverlay.scopeImage.SizeScale_X = 1;
								PlayerLifeUI.scopeOverlay.scopeImage.SizeScale_Y = 1;
							}
						}

						PlayerLifeUI.scopeOverlay.scopeImage.Texture = scope;

						if (firstAttachments.aimHook.parent.Find("Reticule") != null)
						{
							Color reticuleColor = OptionsSettings.criticalHitmarkerColor;
							reticuleColor.a = 1.0f;
							PlayerLifeUI.scopeOverlay.scopeImage.TintColor = reticuleColor;
						}
						else
						{
							PlayerLifeUI.scopeOverlay.scopeImage.TintColor = ESleekTint.NONE;
						}

						player.animator.viewmodelCameraLocalPositionOffset = Vector3.up;
					}
					else
					{
						PlayerLifeUI.scopeOverlay.scopeImage.Texture = null;

						player.animator.viewmodelCameraLocalPositionOffset = Vector3.zero;
					}
				}
				else
				{
					PlayerLifeUI.scopeOverlay.scopeImage.Texture = null;
				}

				player.animator.viewmodelSwayMultiplier = 0.1f;
				player.animator.viewmodelOffsetPreferenceMultiplier = 0;

				if (equippedGunAsset.driverTurretViewmodelMode == EDriverTurretViewmodelMode.OffscreenWhileAiming)
				{
					player.animator.drivingViewmodelCameraLocalPositionOffset = Vector3.up;
				}

				player.look.shouldUseZoomFactorForSensitivity = true;

				if (equippedGunAsset.isTurret || equippedGunAsset.action == EAction.Minigun || shouldZoomUsingEyes)
				{
					if (player.look.perspective == EPlayerPerspective.FIRST)
					{
						player.look.enableZoom(firstPersonZoomFactor, true);
					}
					else if (player.look.perspective == EPlayerPerspective.THIRD)
					{
						player.look.enableZoom(thirdPersonZoomFactor, true);
					}
				}
				else
				{
					if (player.look.perspective == EPlayerPerspective.THIRD)
					{
						player.look.enableZoom(thirdPersonZoomFactor, true);
					}
					else
					{
						player.look.enableZoom(1.0f, true);
					}
				}

				player.look.UpdateScopeOverlay();

				UpdateCrosshairEnabled();
				PlayerUI.instance.groupUI.IsVisible = false;
			}

			// Nelson 2023-12-11: zero pitch randomization to prevent anim desync. (public issue #4249)
			player.playSound(equippedGunAsset.aim, 1.0f, 0.0f);

			isMinigunSpinning = true;
			player.animator.play("Aim_Start", false);

			OnAimingChanged_Global.TryInvoke("OnAimingChanged_Global", this);
			GetVehicleTurretEventHook()?.OnAimingStarted?.TryInvoke(this);

			if (channel.IsLocalPlayer)
			{
				GetVehicleTurretEventHook()?.OnAimingStarted_Local?.TryInvoke(this);
			}

			foreach (UseableGunEventHook eventComponent in EnumerateEventComponents())
				eventComponent.OnAimingStarted?.TryInvoke(this);
		}

		private void stopAim()
		{
			UpdateMovementSpeedMultiplier();

			if (channel.IsLocalPlayer)
			{
				// Non-turret gun may have pushed viewmodel off-screen for 2D scope. Turret should remain off-screen.
				if (!equippedGunAsset.isTurret)
				{
					player.animator.viewmodelCameraLocalPositionOffset = Vector3.zero;
				}

				if (equippedGunAsset.driverTurretViewmodelMode == EDriverTurretViewmodelMode.OffscreenWhileAiming)
				{
					player.animator.drivingViewmodelCameraLocalPositionOffset = Vector3.zero;
				}

				player.look.ConvertScopeSwayToInputRotation();
				player.animator.viewmodelSwayMultiplier = 1;
				player.animator.viewmodelOffsetPreferenceMultiplier = 1;

				player.look.UpdateScopeOverlay();

				player.look.shouldUseZoomFactorForSensitivity = false;

				UpdateCrosshairEnabled();
				PlayerUI.instance.groupUI.IsVisible = true;
			}

			isMinigunSpinning = false;
			player.animator.play("Aim_Stop", false);

			OnAimingChanged_Global.TryInvoke("OnAimingChanged_Global", this);
			GetVehicleTurretEventHook()?.OnAimingStopped?.TryInvoke(this);

			if (channel.IsLocalPlayer)
			{
				// This should be safe for UI cleanup because stopAim is called during dequip.
				GetVehicleTurretEventHook()?.OnAimingStopped_Local?.TryInvoke(this);
			}

			foreach (UseableGunEventHook eventComponent in EnumerateEventComponents())
				eventComponent.OnAimingStopped?.TryInvoke(this);
		}

		private void startAttach()
		{
			PlayerUI.isLocked = true;
			PlayerLifeUI.close();

			if (sightButton != null)
			{
				sightButton.IsVisible = true;
			}

			if (tacticalButton != null)
			{
				tacticalButton.IsVisible = true;
			}

			if (gripButton != null)
			{
				gripButton.IsVisible = true;
			}

			if (barrelButton != null)
			{
				barrelButton.IsVisible = true;
			}

			if (magazineButton != null)
			{
				magazineButton.IsVisible = true;
			}

			UpdateCrosshairEnabled();

			if (channel.IsLocalPlayer)
			{
				GetVehicleTurretEventHook()?.OnInspectingAttachmentsStarted_Local?.TryInvoke(this);
			}
		}

		private void stopAttach()
		{
			PlayerUI.isLocked = false;
			PlayerLifeUI.open();

			if (sightButton != null)
			{
				sightButton.IsVisible = false;
			}

			if (tacticalButton != null)
			{
				tacticalButton.IsVisible = false;
			}

			if (gripButton != null)
			{
				gripButton.IsVisible = false;
			}

			if (barrelButton != null)
			{
				barrelButton.IsVisible = false;
			}

			if (magazineButton != null)
			{
				magazineButton.IsVisible = false;
			}

			UpdateCrosshairEnabled();

			if (channel.IsLocalPlayer)
			{
				// This should be safe for UI cleanup because stopAttach is called during dequip.
				GetVehicleTurretEventHook()?.OnInspectingAttachmentsStopped_Local?.TryInvoke(this);
			}
		}

		protected void Update()
		{
			if (!Dedicator.IsDedicatedServer)
			{
				ItemGunAsset asset = player.equipment.asset as ItemGunAsset;

				if (asset != null && asset.action == EAction.Minigun)
				{
					if (isMinigunSpinning)
					{
						minigunSpeed = Mathf.Lerp(minigunSpeed, 1.0f, 8.0f * Time.deltaTime);
					}
					else
					{
						minigunSpeed = Mathf.Lerp(minigunSpeed, 0.0f, 2.0f * Time.deltaTime);
					}
					minigunDistance += minigunSpeed * 720.0f * Time.deltaTime;

					if (firstMinigunBarrel != null)
					{
						firstMinigunBarrel.localRotation = Quaternion.Euler(0.0f, minigunDistance, 0.0f);
					}

					if (thirdMinigunBarrel != null)
					{
						thirdMinigunBarrel.localRotation = Quaternion.Euler(0.0f, minigunDistance, 0.0f);
					}

					if (whir != null)
					{
						whir.volume = minigunSpeed;
						whir.pitch = Mathf.Lerp(0.75f, 1.0f, minigunSpeed);
					}
				}
			}

			if (player.movement.getVehicle() != null && player.movement.getVehicle().passengers[player.movement.getSeat()].turret != null)
			{
				Transform turretAim = player.movement.getVehicle().passengers[player.movement.getSeat()].turretAim;
				if (turretAim != null)
				{
					Transform muzzleHook = turretAim.Find("Barrel");
					Transform ejectHook = turretAim.Find("Eject");

					if (thirdMuzzleEmitter != null && muzzleHook != null)
					{
						thirdMuzzleEmitter.transform.position = muzzleHook.position;
						thirdMuzzleEmitter.transform.rotation = muzzleHook.rotation;
					}

					if (thirdShellEmitter != null && ejectHook != null)
					{
						thirdShellEmitter.transform.position = ejectHook.position;
						thirdShellEmitter.transform.rotation = ejectHook.rotation;
					}
				}
			}
			else
			{
				if (thirdShellEmitter != null)
				{
					thirdShellEmitter.transform.SetPositionAndRotation(thirdAttachments.ejectHook.position, thirdAttachments.ejectHook.rotation);
				}
			}

			if (channel.IsLocalPlayer)
			{
#if !DEDICATED_SERVER
				if (laserTransform != null)
				{
					if (player.look.perspective == EPlayerPerspective.FIRST)
					{
						Quaternion viewmodelOffset = Quaternion.Euler(player.animator.recoilViewmodelCameraRotation.currentPosition);
						Quaternion aimRotation = player.look.aim.rotation * viewmodelOffset;
						Vector3 laserDirection = aimRotation * Vector3.forward;
						if (!player.look.IsLocallyUsingFreecam && Physics.Raycast(new Ray(player.look.aim.position, laserDirection), out contact, 2048, RayMasks.BLOCK_LASER))
						{
							laserTransform.position = contact.point + (laserDirection * -0.05f);
							laserGameObject.SetActive(true);
						}
						else
						{
							laserGameObject.SetActive(false);
						}
					}
					else if (player.look.perspective == EPlayerPerspective.THIRD)
					{
						RaycastHit target;
						if (!player.look.IsLocallyUsingFreecam && Physics.Raycast(new Ray(MainCamera.instance.transform.position, MainCamera.instance.transform.forward), out target, 512, RayMasks.DAMAGE_CLIENT))
						{
							if (Physics.Raycast(new Ray(player.look.aim.position, (target.point - player.look.aim.position).normalized), out contact, 2048, RayMasks.BLOCK_LASER))
							{
								laserTransform.position = contact.point + (player.look.aim.forward * -0.05f);
								laserGameObject.SetActive(true);
							}
							else
							{
								laserGameObject.SetActive(false);
							}
						}
						else
						{
							laserGameObject.SetActive(false);
						}
					}
				}

				if (firstAttachments != null && firstAttachments.tacticalAsset != null && firstAttachments.tacticalAsset.isRangefinder)
				{
					bool newRange = false;

					if (player.look.perspective == EPlayerPerspective.FIRST)
					{
						newRange = Physics.Raycast(new Ray(player.look.aim.position, player.look.aim.forward), out contact, equippedGunAsset.rangeRangefinder, RayMasks.BLOCK_LASER);
					}
					else if (player.look.perspective == EPlayerPerspective.THIRD)
					{
						RaycastHit target;
						if (Physics.Raycast(new Ray(MainCamera.instance.transform.position, MainCamera.instance.transform.forward), out target, 512, RayMasks.DAMAGE_CLIENT))
						{
							newRange = Physics.Raycast(new Ray(player.look.aim.position, (target.point - player.look.aim.position).normalized), out contact, equippedGunAsset.rangeRangefinder, RayMasks.BLOCK_LASER);
						}
						else
						{
							newRange = false;
						}
					}

					if (rangeLabel != null)
					{
						if (inRange)
						{
							if (OptionsSettings.metric)
							{
								rangeLabel.Text = (int) contact.distance + " m";
							}
							else
							{
								rangeLabel.Text = (int) MeasurementTool.MtoYd(contact.distance) + " yd";
							}
						}
						else
						{
							if (OptionsSettings.metric)
							{
								rangeLabel.Text = "? m";
							}
							else
							{
								rangeLabel.Text = "? yd";
							}
						}

						rangeLabel.TextColor = inRange ? Palette.COLOR_G : Palette.COLOR_R;
					}

					if (newRange != inRange)
					{
						inRange = newRange;

						// Tactical laser takes priority.
						if (laserTransform == null)
						{
							if (firstAttachments.lightHook != null)
							{
								firstAttachments.lightHook.gameObject.SetActive(inRange && interact);
							}
							if (firstAttachments.light2Hook != null)
							{
								firstAttachments.light2Hook.gameObject.SetActive(!inRange && interact);
							}
							if (thirdAttachments.lightHook != null)
							{
								thirdAttachments.lightHook.gameObject.SetActive(inRange && interact);
							}
							if (thirdAttachments.light2Hook != null)
							{
								thirdAttachments.light2Hook.gameObject.SetActive(!inRange && interact);
							}
						}
					}
				}
#endif // !DEDICATED_SERVER

				if (firstFakeLight != null && thirdMuzzleEmitter != null)
				{
					firstFakeLight.position = thirdMuzzleEmitter.transform.position;
				}

				if (firstFakeLight_0 != null && thirdAttachments.lightHook != null)
				{
					firstFakeLight_0.position = thirdAttachments.lightHook.position;

					if (firstFakeLight_0.gameObject.activeSelf != (player.look.perspective == EPlayerPerspective.FIRST && thirdAttachments.lightHook.gameObject.activeSelf))
					{
						firstFakeLight_0.gameObject.SetActive(player.look.perspective == EPlayerPerspective.FIRST && thirdAttachments.lightHook.gameObject.activeSelf);
					}
				}

				if (firstFakeLight_1 != null && thirdAttachments.light2Hook != null)
				{
					firstFakeLight_1.position = thirdAttachments.light2Hook.position;

					if (firstFakeLight_1.gameObject.activeSelf != (player.look.perspective == EPlayerPerspective.FIRST && thirdAttachments.light2Hook.gameObject.activeSelf))
					{
						firstFakeLight_1.gameObject.SetActive(player.look.perspective == EPlayerPerspective.FIRST && thirdAttachments.light2Hook.gameObject.activeSelf);
					}
				}

				swayTime += Time.deltaTime * (1.0f - (steadyAccuracy / 4.0f));
				if (isAiming && firstAttachments.sightAsset != null)
				{
					float sway = (1.0f - (1.0f / firstAttachments.sightAsset.zoom)) * 1.25f;
					sway *= 1f - (player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.DIVING) * 0.5f);

					sway *= firstAttachments.sightAsset.sway;

					if (thirdAttachments != null && thirdAttachments.tacticalAsset != null && shouldEnableTacticalStats && (!thirdAttachments.tacticalAsset.ShouldOnlyAffectAimWhileProne || player.stance.stance == EPlayerStance.PRONE))
					{
						sway *= thirdAttachments.tacticalAsset.sway;
					}

					if (thirdAttachments != null && thirdAttachments.gripAsset != null && (!thirdAttachments.gripAsset.ShouldOnlyAffectAimWhileProne || player.stance.stance == EPlayerStance.PRONE))
					{
						sway *= thirdAttachments.gripAsset.sway;
					}

					if (thirdAttachments != null && thirdAttachments.barrelAsset != null && (!thirdAttachments.barrelAsset.ShouldOnlyAffectAimWhileProne || player.stance.stance == EPlayerStance.PRONE))
					{
						sway *= thirdAttachments.barrelAsset.sway;
					}

					if (thirdAttachments != null && thirdAttachments.magazineAsset != null && (!thirdAttachments.magazineAsset.ShouldOnlyAffectAimWhileProne || player.stance.stance == EPlayerStance.PRONE))
					{
						sway *= thirdAttachments.magazineAsset.sway;
					}

					if (player.stance.stance == EPlayerStance.CROUCH)
					{
						sway *= SWAY_CROUCH;
					}
					else if (player.stance.stance == EPlayerStance.PRONE)
					{
						sway *= SWAY_PRONE;
					}

#if !DISABLE_SCOPE_SWAY
					player.animator.scopeSway = Vector3.Lerp(player.animator.scopeSway, new Vector3(Mathf.Sin(0.75f * swayTime) * sway, Mathf.Sin(1.0f * swayTime) * sway, 0.0f), Time.deltaTime * 4.0f);
#endif // !DISABLE_SCOPE_SWAY
				}

				if (firstAttachments.reticuleHook != null && firstAttachments.sightAsset != null && firstAttachments.sightAsset.isHolographic)
				{
					UpdateHolographicReticulePosition();
				}

				if (scopeDistanceMarkers != null && scopeDistanceMarkers.Count > 0)
				{
					UpdateScopeDistanceMarkers();
				}

				UpdateScopeAlpha();
			}
		}

		internal void GetAimingViewmodelAlignment(out Transform alignmentTransform, out Vector3 alignmentOffset, out float alpha)
		{
			alignmentTransform = null;
			alignmentOffset = Vector3.zero;
			alpha = GetInterpolatedAimAlpha();

			if (equippedGunAsset.isTurret || equippedGunAsset.action == EAction.Minigun)
				return;

			if (firstAttachments != null)
			{
				if (firstAttachments.aimHook != null)
				{
					alignmentTransform = firstAttachments.aimHook;
					alignmentOffset = firstAttachments.sightAsset?.AimAlignmentLocalOffset ?? Vector3.zero;
				}
				else
				{
					// This was a long time ago, but as of 2022-11-30 I think the original rational was:
					// - "new" guns can specify a viewHook that is used while aiming without a sight attachment
					// - "old" guns like an Eaglefire without a sight attachment that *can* attach a sight
					//   should have a slight offset because the sightHook intersects the gun body. Certain other
					//   older guns like a Cobra that *cannot* attach sights used the sightHook as viewHook prior to its addition
					if (firstAttachments.viewHook != null)
					{
						alignmentTransform = firstAttachments.viewHook;
					}
					else
					{
						alignmentTransform = firstAttachments.sightHook;
						if (equippedGunAsset.hasSight)
						{
							alignmentOffset = new Vector3(0.0f, -0.04f, 0.01f);
						}
					}
				}
			}
		}

		/// <summary>
		/// This is a bit of a hack... aimAccuracy is [0, maxAimingAccuracy] and changed during each FixedUpdate call,
		/// but was used in some gameplay display features like holo sight, laser, ADS, etc. (yes, should
		/// be de-coupled from FixedUpdate but that is its own issue) To smooth this out we interpolate
		/// slightly behind the aimAccuracy value depending on the time since FixedUpdate.
		/// </summary>
		private float GetInterpolatedAimAlpha()
		{
			double deltaTime = Time.timeAsDouble - Time.fixedTimeAsDouble;
			float timeAlpha = (float) (deltaTime / Time.fixedDeltaTime);
			if (isAiming)
			{
				// aimAccuracy is increasing
				if (_aimAccuracy < maxAimingAccuracy)
				{
					return 1.0f - MathfEx.Square(1.0f - MathfEx.SmootherStep01((_aimAccuracy * maxAimingAccuracyReciprocal) + (timeAlpha * maxAimingAccuracyReciprocal)));
				}
				else
				{
					return 1.0f;
				}
			}
			else
			{
				// aimAccuracy is decreasing
				if (_aimAccuracy > 0)
				{
					return 1.0f - MathfEx.Square(1.0f - MathfEx.SmootherStep01((_aimAccuracy * maxAimingAccuracyReciprocal) - (timeAlpha * maxAimingAccuracyReciprocal)));
				}
				else
				{
					return 0.0f;
				}
			}
		}

		private float GetSimulationAimAlpha()
		{
			return _aimAccuracy * maxAimingAccuracyReciprocal;
		}

		private void UpdateInfoBoxVisibility()
		{
			bool visible = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowUseableGunStatus);
			if (Level.info != null && Level.info.configData != null)
			{
				visible &= Level.info.configData.PlayerUI_GunVisible;
			}
			infoBox.IsVisible = visible;
		}

		private void OnLocalPluginWidgetFlagsChanged(Player player, EPluginWidgetFlags oldFlags)
		{
			EPluginWidgetFlags newFlags = player.pluginWidgetFlags;
			if ((oldFlags & EPluginWidgetFlags.ShowUseableGunStatus) != (newFlags & EPluginWidgetFlags.ShowUseableGunStatus))
			{
				UpdateInfoBoxVisibility();
			}
		}

		private IEnumerable<UseableGunEventHook> EnumerateEventComponents()
		{
			if (firstEventComponent)
				yield return firstEventComponent;

			if (thirdEventComponent)
				yield return thirdEventComponent;

			if (characterEventComponent)
				yield return characterEventComponent;
		}

		private void InvokeModHookShotFiredEvents()
		{
			GetVehicleTurretEventHook()?.OnShotFired?.TryInvoke(this);

			foreach (UseableGunEventHook eventComponent in EnumerateEventComponents())
				eventComponent.OnShotFired?.TryInvoke(this);
		}

		private void ClearScopeDistanceMarkers()
		{
			if (scopeDistanceMarkers != null)
			{
				scopeDistanceMarkers.Clear();
			}
		}

		private void InstantiateScopeDistanceMarkers()
		{
			if (scopeDistanceMarkers == null)
			{
				scopeDistanceMarkers = new List<DistanceMarker>();
			}

			if (firstAttachments.scopeHook == null)
			{
				return;
			}

			Transform markerParent = firstAttachments.scopeHook.Find("Reticule");
			if (markerParent == null)
			{
				return;
			}

			if (scopeDistanceMarkerMaterial == null)
			{
				scopeDistanceMarkerMaterial = new Material(Shader.Find("Sprites/Default"));
				scopeDistanceMarkerMaterial.hideFlags = HideFlags.HideAndDontSave;
			}

			foreach (ItemSightAsset.DistanceMarker config in firstAttachments.sightAsset.distanceMarkers)
			{
				DistanceMarker marker = new DistanceMarker();
				marker.isActive = true;
				marker.distance = config.distance;

				GameObject markerGameObject = new GameObject($"DistanceMarker_{config.distance}m");
				markerGameObject.layer = LayerMasks.VIEWMODEL;
				marker.transform = markerGameObject.transform;
				marker.transform.SetParent(markerParent, /*worldPositionStays*/ false);
				marker.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);

				GameObject lineGameObject = new GameObject("Line");
				lineGameObject.layer = LayerMasks.VIEWMODEL;
				Transform lineTransform = lineGameObject.transform;
				lineTransform.SetParent(marker.transform, /*worldPositionStays*/ false);

				marker.lineComponent = lineGameObject.AddComponent<LineRenderer>();
				marker.lineComponent.alignment = LineAlignment.TransformZ;
				marker.lineComponent.endColor = config.color;
				marker.lineComponent.startColor = config.color;
				marker.lineComponent.useWorldSpace = false;
				marker.lineComponent.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				marker.lineComponent.widthMultiplier = 0.005f;
				marker.lineComponent.sharedMaterial = scopeDistanceMarkerMaterial;

				if (config.side == ItemSightAsset.DistanceMarker.ESide.Right)
				{
					marker.lineComponent.SetPositions(new Vector3[] { new Vector3(config.lineOffset * 2.0f, 0.0f, 0.0f), new Vector3((config.lineOffset + config.lineWidth) * 2.0f, 0.0f, 0.0f) });
				}
				else
				{
					marker.lineComponent.SetPositions(new Vector3[] { new Vector3(config.lineOffset * -2.0f, 0.0f, 0.0f), new Vector3((config.lineOffset + config.lineWidth) * -2.0f, 0.0f, 0.0f) });
				}

				if (config.hasLabel)
				{
					GameObject textGameObject = new GameObject("Text");
					textGameObject.layer = LayerMasks.VIEWMODEL;
					Transform textTransform = textGameObject.transform;
					textTransform.SetParent(marker.transform, /*worldPositionStays*/ false);

					marker.textComponent = textGameObject.AddComponent<TMPro.TextMeshPro>();
					marker.textComponent.color = config.color;
					marker.textComponent.fontSize = 0.35f;
					marker.textComponent.fontStyle = TMPro.FontStyles.Bold;
					// Text is updated during SyncScopeDistanceMarkerText.

					RectTransform textRectTransform = textGameObject.GetRectTransform();

					// Extra padding between horizontal line and label.
					const float textHorizontalOffset = 0.01f;

					if (config.side == ItemSightAsset.DistanceMarker.ESide.Right)
					{
						textRectTransform.localPosition = new Vector3((config.lineOffset + config.lineWidth) * 2.0f + textHorizontalOffset, 0.0f, 0.0f);
						marker.textComponent.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
						textRectTransform.pivot = new Vector3(0.0f, 0.5f); // middle left
					}
					else
					{
						textRectTransform.localPosition = new Vector3((config.lineOffset + config.lineWidth) * -2.0f - textHorizontalOffset, 0.0f, 0.0f);
						marker.textComponent.alignment = TMPro.TextAlignmentOptions.MidlineRight;
						textRectTransform.pivot = new Vector3(1.0f, 0.5f); // middle right
					}
				}

				scopeDistanceMarkers.Add(marker);
			}

			SyncScopeDistanceMarkerText();
		}

		private void UpdateScopeDistanceMarkers()
		{
			Camera scopeCamera = player.look.scopeCamera;
			float verticalFovDegrees = scopeCamera.fieldOfView;
			float verticalFovRadians = Mathf.Deg2Rad * verticalFovDegrees;

			float speed = equippedGunAsset.muzzleVelocity;
			float gravity = CalculateBulletGravity();

			foreach (DistanceMarker marker in scopeDistanceMarkers)
			{
				float angle = Mathf.Abs(SleekScopeOverlay.CalcAngle(speed, marker.distance, gravity));
				float percentage = angle / verticalFovRadians;
				// +1 is top, -1 is bottom
				float localPosition = percentage * -2.0f;
				marker.transform.localPosition = new Vector3(0.0f, localPosition, 0.0f);

				// Disable marker near the center or outside the bottom of the scope.
				bool shouldBeActive = localPosition < -0.01f && localPosition > -0.9f;
				if (marker.isActive != shouldBeActive)
				{
					marker.isActive = shouldBeActive;
					marker.transform.gameObject.SetActive(shouldBeActive);
				}
			}
		}

		/// <summary>
		/// Holographic sights follow the true aiming direction regardless of viewmodel animation.
		/// </summary>
		private void UpdateHolographicReticulePosition()
		{
			firstAttachments.reticuleHook.localPosition = originalReticuleHookLocalPosition;
			Plane reticulePlane = new Plane(firstAttachments.reticuleHook.forward, firstAttachments.reticuleHook.position);

			Quaternion viewmodelOffset = Quaternion.Euler(player.animator.recoilViewmodelCameraRotation.currentPosition);
			Quaternion aimRotation = player.animator.viewmodelCameraTransform.rotation * viewmodelOffset;
			Vector3 rayDirection = aimRotation * Vector3.forward;
			Vector3 rayOrigin = player.animator.viewmodelCameraTransform.position;
			float hitDistance;
			if (reticulePlane.Raycast(new Ray(rayOrigin, rayDirection), out hitDistance))
			{
				Vector3 hitPosition = rayOrigin + (rayDirection * hitDistance);
				Vector3 localHitPosition = firstAttachments.reticuleHook.parent.InverseTransformPoint(hitPosition);
				firstAttachments.reticuleHook.localPosition = Vector3.Lerp(originalReticuleHookLocalPosition, localHitPosition, GetInterpolatedAimAlpha());
			}
		}

		private void UpdateMovementSpeedMultiplier()
		{
			movementSpeedMultiplier = 1.0f;
			if (isAiming)
			{
				movementSpeedMultiplier *= equippedGunAsset.aimingMovementSpeedMultiplier;
			}

			if (thirdAttachments.barrelAsset != null)
			{
				movementSpeedMultiplier *= thirdAttachments.barrelAsset.equipableMovementSpeedMultiplier;
				if (isAiming)
				{
					movementSpeedMultiplier *= thirdAttachments.barrelAsset.aimingMovementSpeedMultiplier;
				}
			}

			if (thirdAttachments.tacticalAsset != null)
			{
				movementSpeedMultiplier *= thirdAttachments.tacticalAsset.equipableMovementSpeedMultiplier;
				if (isAiming)
				{
					movementSpeedMultiplier *= thirdAttachments.tacticalAsset.aimingMovementSpeedMultiplier;
				}
			}

			if (thirdAttachments.sightAsset != null)
			{
				movementSpeedMultiplier *= thirdAttachments.sightAsset.equipableMovementSpeedMultiplier;
				if (isAiming)
				{
					movementSpeedMultiplier *= thirdAttachments.sightAsset.aimingMovementSpeedMultiplier;
				}
			}

			if (thirdAttachments.magazineAsset != null)
			{
				movementSpeedMultiplier *= thirdAttachments.magazineAsset.equipableMovementSpeedMultiplier;
				if (isAiming)
				{
					movementSpeedMultiplier *= thirdAttachments.magazineAsset.aimingMovementSpeedMultiplier;
				}
			}

			if (thirdAttachments.gripAsset != null)
			{
				movementSpeedMultiplier *= thirdAttachments.gripAsset.equipableMovementSpeedMultiplier;
				if (isAiming)
				{
					movementSpeedMultiplier *= thirdAttachments.gripAsset.aimingMovementSpeedMultiplier;
				}
			}
		}

		private void UpdateAimInDuration()
		{
			float aimInDuration = equippedGunAsset.aimInDuration;
			if (thirdAttachments.barrelAsset != null)
			{
				aimInDuration *= thirdAttachments.barrelAsset.aimDurationMultiplier;
			}

			if (thirdAttachments.tacticalAsset != null)
			{
				aimInDuration *= thirdAttachments.tacticalAsset.aimDurationMultiplier;
			}

			if (thirdAttachments.sightAsset != null)
			{
				aimInDuration *= thirdAttachments.sightAsset.aimDurationMultiplier;
			}

			if (thirdAttachments.magazineAsset != null)
			{
				aimInDuration *= thirdAttachments.magazineAsset.aimDurationMultiplier;
			}

			if (thirdAttachments.gripAsset != null)
			{
				aimInDuration *= thirdAttachments.gripAsset.aimDurationMultiplier;
			}

			maxAimingAccuracy = Mathf.Clamp(Mathf.RoundToInt(aimInDuration * 50), 1, 200);
			maxAimingAccuracyReciprocal = 1.0f / maxAimingAccuracy;
			if (_aimAccuracy > maxAimingAccuracy)
			{
				AimAccuracy = maxAimingAccuracy;
			}

			if (equippedGunAsset.shouldScaleAimAnimations)
			{
				// Don't use aimInDuration directly because it gets rounded and clamped.
				float actualAimInDuration = maxAimingAccuracy / 50.0f;

				float aimStartLength = player.animator.GetAnimationLength("Aim_Start", scaled: false);
				player.animator.setAnimationSpeed("Aim_Start", aimStartLength / actualAimInDuration);
				float aimStopLength = player.animator.GetAnimationLength("Aim_Stop", scaled: false);
				player.animator.setAnimationSpeed("Aim_Stop", aimStopLength / actualAimInDuration);
			}
		}

		/// <summary>
		/// Note: This is the multiplier, not the m/s² acceleration.
		/// </summary>
		private float CalculateBulletGravityMultiplier()
		{
			float bulletGravityMultiplier = equippedGunAsset.bulletGravityMultiplier;

			if (thirdAttachments.barrelAsset != null)
			{
				bulletGravityMultiplier *= thirdAttachments.barrelAsset.BallisticGravityMultiplier;
			}

			if (thirdAttachments.tacticalAsset != null)
			{
				bulletGravityMultiplier *= thirdAttachments.tacticalAsset.BallisticGravityMultiplier;
			}

			if (thirdAttachments.sightAsset != null)
			{
				bulletGravityMultiplier *= thirdAttachments.sightAsset.BallisticGravityMultiplier;
			}

			if (thirdAttachments.magazineAsset != null)
			{
				bulletGravityMultiplier *= thirdAttachments.magazineAsset.BallisticGravityMultiplier;
			}

			if (thirdAttachments.gripAsset != null)
			{
				bulletGravityMultiplier *= thirdAttachments.gripAsset.BallisticGravityMultiplier;
			}

			return bulletGravityMultiplier;
		}

		private void DestroyLaser()
		{
#if !DEDICATED_SERVER
			if (laserGameObject != null)
			{
				Destroy(laserGameObject);
				laserGameObject = null;
			}

			laserTransform = null;

			if (laserMaterial != null)
			{
				Destroy(laserMaterial);
				laserMaterial = null;
			}
#endif // !DEDICATED_SERVER
		}

		private int maxAimingAccuracy;
		private float maxAimingAccuracyReciprocal;

		private class DistanceMarker
		{
			public bool isActive;
			public float distance;
			public Transform transform;
			public LineRenderer lineComponent;
			public TMPro.TextMeshPro textComponent;
		}
		private List<DistanceMarker> scopeDistanceMarkers;
		private static Material scopeDistanceMarkerMaterial;

		internal const float DEFAULT_THIRD_PERSON_ZOOM_FACTOR = 1.25f;

		/// <summary>
		/// Code common for regular gun and sentry gun.
		/// </summary>
		internal static void DetonateExplosiveMagazine(ItemMagazineAsset magazineAsset, Vector3 position, Player instigatingPlayer, ERagdollEffect ragdollEffect)
		{
			EffectAsset explosionEffect = magazineAsset.FindExplosionEffect();
			if (explosionEffect != null)
			{
				TriggerEffectParameters effectParams = new TriggerEffectParameters(explosionEffect);
				effectParams.position = position;
				effectParams.relevantDistance = EffectManager.MEDIUM;
				effectParams.wasInstigatedByPlayer = true;
				effectParams.reliable = true;
				EffectManager.triggerEffect(effectParams);
			}

			CSteamID killerSteamId = instigatingPlayer != null ? instigatingPlayer.channel.owner.playerID.steamID : CSteamID.Nil;

			List<EPlayerKill> kills;
			ExplosionParameters explosionParameters = new ExplosionParameters(position, magazineAsset.range, EDeathCause.SPLASH, killerSteamId);
			explosionParameters.playerDamage = magazineAsset.playerDamage;
			explosionParameters.zombieDamage = magazineAsset.zombieDamage;
			explosionParameters.animalDamage = magazineAsset.animalDamage;
			explosionParameters.barricadeDamage = magazineAsset.barricadeDamage;
			explosionParameters.structureDamage = magazineAsset.structureDamage;
			explosionParameters.vehicleDamage = magazineAsset.vehicleDamage;
			explosionParameters.resourceDamage = magazineAsset.resourceDamage;
			explosionParameters.objectDamage = magazineAsset.objectDamage;
			explosionParameters.damageOrigin = EDamageOrigin.Bullet_Explosion;
			explosionParameters.ragdollEffect = ragdollEffect;
			explosionParameters.launchSpeed = magazineAsset.explosionLaunchSpeed;
			explosionParameters.playImpactEffect = magazineAsset.ExplosionPlaysImpactEffects;
			explosionParameters.penetrateBuildables = magazineAsset.ExplosionPenetratesBuildables;
			DamageTool.explode(explosionParameters, out kills);

			if (instigatingPlayer != null)
			{
				foreach (EPlayerKill explosionKill in kills)
				{
					instigatingPlayer.sendStat(explosionKill);
				}
			}
		}

		private bool CanDamageInvulnerableEntities
		{
			get
			{
				if (((ItemWeaponAsset) player.equipment.asset).isInvulnerable)
				{
					return true;
				}

				if (thirdAttachments?.barrelAsset?.CanDamageInvulernableEntities ?? false)
				{
					return true;
				}

				if (thirdAttachments?.tacticalAsset?.CanDamageInvulernableEntities ?? false)
				{
					return true;
				}

				if (thirdAttachments?.gripAsset?.CanDamageInvulernableEntities ?? false)
				{
					return true;
				}

				if (thirdAttachments?.sightAsset?.CanDamageInvulernableEntities ?? false)
				{
					return true;
				}

				if (thirdAttachments?.magazineAsset?.CanDamageInvulernableEntities ?? false)
				{
					return true;
				}

				return false;
			}
		}

		private void IncrementShotCountForRechamber()
		{
			if (equippedGunAsset.RechamberAfterShotCount > 0)
			{
				++shotCountForRechamber;
				if (shotCountForRechamber >= equippedGunAsset.RechamberAfterShotCount)
				{
					shotCountForRechamber = 0;
					needsRechamber = true;
				}
			}
		}

		internal void UpdateScopeAlpha()
		{
			if (player.look.scopeMaterial != null)
			{
				float alpha = GetInterpolatedAimAlpha();
				player.look.scopeAlpha = alpha;
				if (player.look.IsUsing2DScope)
				{
					player.look.scopeMaterial.SetFloat(ScopeMaterialAlphaId, 0.0f);
				}
				else if (GraphicsSettings.scopeQuality == EGraphicQuality.OFF)
				{
					player.look.scopeMaterial.SetFloat(ScopeMaterialAlphaId, alpha);
					UnturnedPostProcess.instance.SetSingleRenderScopeZoomFactor(Mathf.Lerp(1.0f, player.look.scopeCameraZoomFactor, alpha), alpha);
				}
				else
				{
					player.look.scopeMaterial.SetFloat(ScopeMaterialAlphaId, 1.0f);
				}
			}
		}

		private static int ScopeMaterialAlphaId = Shader.PropertyToID("_Alpha");
		private static int ReticuleTextureId = Shader.PropertyToID("_ReticuleTexture");
	}
}
