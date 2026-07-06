////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableSentry : InteractableStorage
	{
		private static List<Player> playersInRadius = new List<Player>();
		private static List<Zombie> zombiesInRadius = new List<Zombie>();
		private static List<Animal> animalsInRadius = new List<Animal>();
		private static List<InteractableVehicle> vehiclesInRadius = new List<InteractableVehicle>();

		public InteractablePower power;

		private bool hasWeapon;
		private bool interact;
		private Attachments attachments;
		private AudioSource gunshotAudioSource;
		private ParticleSystem shellEmitter;
		private ParticleSystem muzzleEmitter;
		private Light muzzleLight;
		private ParticleSystem tracerEmitter;

		private Transform yawTransform;
		private Transform pitchTransform;
		private Transform aimTransform;

		private GameObject onGameObject;
		private GameObject onModelGameObject;
		private Material onMaterial;
		private GameObject offGameObject;
		private GameObject offModelGameObject;
		private Material offMaterial;
		private GameObject spotGameObject;

		private Player targetPlayer;
		private Zombie targetZombie;
		private Animal targetAnimal;
		private InteractableVehicle targetVehicle;

		private float targetYaw;
		private float yaw;

		private float targetPitch;
		private float pitch;

		private bool isAlert;
		private double lastAlert;

		private bool isFiring;
		private double lastFire;
		private float fireTime;

		private bool isAiming;
		private double lastAim;

		private double lastScan;
		private double lastDrift;
		private double lastShot;

		public ItemSentryAsset sentryAsset
		{
			get;
			private set;
		}

		public ESentryMode sentryMode => sentryAsset.sentryMode;

		public bool isPowered
		{
			get
			{
				if (power == null)
					return false;

				if (sentryAsset.requiresPower)
				{
					return power.isWired;
				}
				else
				{
					return true;
				}
			}
		}

		private void trace(Vector3 pos, Vector3 dir)
		{
			if (tracerEmitter == null)
			{
				return;
			}

			if (attachments.barrelModel != null && attachments.barrelAsset.isBraked && displayItem.state[16] > 0)
			{
				return;
			}

			tracerEmitter.transform.position = pos;
			tracerEmitter.transform.rotation = Quaternion.LookRotation(dir);
			tracerEmitter.Emit(1);
		}

		public void shoot()
		{
			lastAlert = Time.timeAsDouble;

			if (!Dedicator.IsDedicatedServer)
			{
				if (gunshotAudioSource != null)
				{
					AudioClip clip = ((ItemGunAsset) displayAsset).shoot;
					float volume = 1.0f;
					float maxDistance = ((ItemGunAsset) displayAsset).gunshotRolloffDistance;
					if (attachments.barrelAsset != null && displayItem.state[16] > 0)
					{
						if (attachments.barrelAsset.shoot != null)
						{
							clip = attachments.barrelAsset.shoot;
						}
						volume *= attachments.barrelAsset.volume;
						maxDistance *= attachments.barrelAsset.gunshotRolloffDistanceMultiplier;
					}
					gunshotAudioSource.clip = clip;
					gunshotAudioSource.volume = volume;
					gunshotAudioSource.maxDistance = maxDistance;

					gunshotAudioSource.pitch = Random.Range(0.975f, 1.025f);
					gunshotAudioSource.PlayOneShot(gunshotAudioSource.clip);
				}

				if (((ItemGunAsset) displayAsset).action == EAction.Trigger)
				{
					if (shellEmitter != null)
					{
						shellEmitter.Emit(1);
					}
				}

				if (attachments.barrelModel == null || !attachments.barrelAsset.isBraked || displayItem.state[16] == 0)
				{
					if (muzzleEmitter != null)
					{
						muzzleEmitter.Emit(1);
					}

					if (muzzleLight != null)
					{
						muzzleLight.enabled = true;
					}
				}

				if (aimTransform != null)
				{
					if (((ItemGunAsset) displayAsset).range < 32)
					{
						trace(aimTransform.position + (aimTransform.forward * 32), aimTransform.forward);
					}
					else
					{
						trace(aimTransform.position + (aimTransform.forward * Random.Range(32.0f, Mathf.Min(64.0f, ((ItemGunAsset) displayAsset).range))), aimTransform.forward);
					}
				}
			}

			lastShot = Time.timeAsDouble;

			if (attachments.barrelAsset != null && attachments.barrelAsset.durability > 0)
			{
				if (attachments.barrelAsset.durability > displayItem.state[16])
				{
					displayItem.state[16] = 0;
				}
				else
				{
					displayItem.state[16] -= attachments.barrelAsset.durability;
				}
			}
		}

		public void alert(float newYaw, float newPitch)
		{
			targetYaw = newYaw;
			targetPitch = newPitch;

			lastAlert = Time.timeAsDouble;
		}

		public override void updateState(Asset asset, byte[] state)
		{
			sentryAsset = asset as ItemSentryAsset;

			if (!hasInitializedSentryComponents)
			{
				hasInitializedSentryComponents = true;

				yawTransform = transform.Find("Yaw");
				if (yawTransform != null)
				{
					pitchTransform = yawTransform.Find("Pitch");

					if (pitchTransform != null)
					{
						aimTransform = pitchTransform.Find("Aim");

						Transform spotTransform = aimTransform.Find("Spot");
						if (spotTransform != null)
						{
							spotGameObject = spotTransform.gameObject;
						}
					}
				}

				// Note that On, On_Model, Off, and Off_Model are usable on the server because modders wanted some sentry events.
				Transform onTransform = transform.FindChildRecursive("On");
				if (onTransform != null)
				{
					onGameObject = onTransform.gameObject;
				}

				Transform onModelTransform = transform.FindChildRecursive("On_Model");
				if (onModelTransform != null)
				{
					onModelGameObject = onModelTransform.gameObject;
					onMaterial = onModelGameObject.GetComponent<Renderer>()?.material;
				}

				Transform offTransform = transform.FindChildRecursive("Off");
				if (offTransform != null)
				{
					offGameObject = offTransform.gameObject;
				}

				Transform offModelTransform = transform.FindChildRecursive("Off_Model");
				if (offModelTransform != null)
				{
					offModelGameObject = offModelTransform.gameObject;
					offMaterial = offModelGameObject.GetComponent<Renderer>()?.material;
				}
			}

			isAlert = false;
			lastAlert = 0.0f;

			targetYaw = HousingConnections.GetModelYaw(transform);
			yaw = targetYaw;

			targetPitch = 0.0f;
			pitch = targetPitch;

			targetPlayer = null;
			targetAnimal = null;
			targetZombie = null;
			targetVehicle = null;

			base.updateState(asset, state);
		}

		public override void refreshDisplay()
		{
			base.refreshDisplay();

			hasWeapon = false;
			attachments = null;
			gunshotAudioSource = null;

			destroyEffects();

			if (spotGameObject != null)
			{
				spotGameObject.SetActive(false);
			}

			if (displayAsset == null || displayAsset.type != EItemType.GUN || ((ItemGunAsset) displayAsset).action == EAction.String || ((ItemGunAsset) displayAsset).action == EAction.Rocket)
			{
				return;
			}

			hasWeapon = true;
			attachments = displayModel.gameObject.GetComponent<Attachments>();

			interact = displayItem.state[12] == 1;

			if (!Dedicator.IsDedicatedServer)
			{
				gunshotAudioSource = displayModel.gameObject.AddComponent<AudioSource>();
				gunshotAudioSource.clip = null;
				gunshotAudioSource.spatialBlend = 1f;
				gunshotAudioSource.rolloffMode = AudioRolloffMode.Linear;
				gunshotAudioSource.volume = 1f;
				gunshotAudioSource.minDistance = 8;
				gunshotAudioSource.maxDistance = 256;
				gunshotAudioSource.playOnAwake = false;
				gunshotAudioSource.dopplerLevel = 0.0f;
#if !DEDICATED_SERVER
				gunshotAudioSource.outputAudioMixerGroup = UnturnedAudioMixer.GetDefaultGroup();
#endif
			}

			if (attachments.ejectHook != null && ((ItemGunAsset) displayAsset).action != EAction.String && ((ItemGunAsset) displayAsset).action != EAction.Rocket)
			{
				EffectAsset shell = ((ItemGunAsset) displayAsset).FindShellEffectAsset();
				if (shell != null && shell.effect != null)
				{
					Transform emitter = EffectManager.InstantiateFromPool(shell).transform;
					emitter.name = "Emitter";
					emitter.parent = attachments.ejectHook;
					emitter.localPosition = Vector3.zero;
					emitter.localRotation = Quaternion.identity;

					shellEmitter = emitter.GetComponent<ParticleSystem>();
				}
			}

			if (attachments.barrelHook != null)
			{
				EffectAsset muzzle = ((ItemGunAsset) displayAsset).FindMuzzleEffectAsset();
				if (muzzle != null && muzzle.effect != null)
				{
					Transform emitter = EffectManager.InstantiateFromPool(muzzle).transform;
					emitter.name = "Emitter";
					emitter.parent = attachments.barrelHook;
					emitter.localPosition = Vector3.zero;
					emitter.localRotation = Quaternion.identity;

					muzzleEmitter = emitter.GetComponent<ParticleSystem>();

					muzzleLight = emitter.GetComponent<Light>();
					if (muzzleLight != null)
					{
						muzzleLight.enabled = false;
						muzzleLight.cullingMask = ~RayMasks.VIEWMODEL;
					}
				}
			}

			if (muzzleEmitter != null)
			{
				if (attachments.barrelModel != null)
				{
					muzzleEmitter.transform.localPosition = Vector3.up * 0.25f;
				}
				else
				{
					muzzleEmitter.transform.localPosition = Vector3.zero;
				}
			}

			if (attachments.magazineAsset != null)
			{
				EffectAsset tracer = attachments.magazineAsset.FindTracerEffectAsset();
				if (tracer != null && tracer.effect != null)
				{
					Transform emitter = EffectManager.InstantiateFromPool(tracer).transform;
					emitter.name = "Tracer";
					emitter.localPosition = Vector3.zero;
					emitter.localRotation = Quaternion.identity;

					tracerEmitter = emitter.GetComponent<ParticleSystem>();
				}
			}

			if (!Dedicator.IsDedicatedServer)
			{
				if (attachments.tacticalAsset != null)
				{
					if (attachments.tacticalAsset.isLight || attachments.tacticalAsset.isLaser)
					{
						if (attachments.lightHook != null)
						{
							attachments.lightHook.gameObject.SetActive(interact);
						}
					}
				}

				if (spotGameObject != null)
				{
					spotGameObject.SetActive(attachments.tacticalAsset != null && attachments.tacticalAsset.isLight && interact);
				}
			}

			// Convert fire rate simulation ticks to seconds.
			int fireRateTicks = ((ItemGunAsset) displayAsset).firerate;
			if (attachments.sightAsset != null)
			{
				fireRateTicks -= attachments.sightAsset.FirerateOffset;
			}
			if (attachments.tacticalAsset != null)
			{
				fireRateTicks -= attachments.tacticalAsset.FirerateOffset;
			}
			if (attachments.gripAsset != null)
			{
				fireRateTicks -= attachments.gripAsset.FirerateOffset;
			}
			if (attachments.barrelAsset != null)
			{
				fireRateTicks -= attachments.barrelAsset.FirerateOffset;
			}
			if (attachments.magazineAsset != null)
			{
				fireRateTicks -= attachments.magazineAsset.FirerateOffset;
			}
			fireRateTicks = Mathf.Max(fireRateTicks, 1);

			fireTime = fireRateTicks;
			fireTime /= 50.0f;
			fireTime *= 3.33f; // lower than normal firerate
		}

		private void Update()
		{
			if (Provider.isServer && isPowered)
			{
				Vector3 fromPoint = transform.position + new Vector3(0.0f, 0.65f, 0.0f);

				if (Time.timeAsDouble - lastScan > 0.1f)
				{
					lastScan = Time.timeAsDouble;
					ScanForTargets(fromPoint);
				}

				if (targetPlayer != null)
				{
					switch (sentryMode)
					{
						case ESentryMode.FRIENDLY:
						case ESentryMode.NEUTRAL: // friendly/neutral sentry carefully watches surrendered enemies, but doesn't fire
							isFiring = targetPlayer.animator.gesture != EPlayerGesture.SURRENDER_START;
							break;
						case ESentryMode.HOSTILE: // hostile sentry fires at enemies regardless of whether they're surrendered
							isFiring = true;
							break;
					}

					isAiming = true;
				}
				else if (targetZombie != null)
				{
					isFiring = true;
					isAiming = true;
				}
				else if (targetAnimal != null)
				{
					switch (sentryMode)
					{
						case ESentryMode.FRIENDLY:
						case ESentryMode.NEUTRAL:
							isFiring = targetAnimal.isHunting;
							break;

						case ESentryMode.HOSTILE:
							isFiring = true;
							break;
					}

					isAiming = true;
				}
				else if (targetVehicle != null)
				{
					isFiring = true;
					isAiming = true;
				}
				else
				{
					isFiring = false;
					isAiming = false;
				}

				if (isAiming)
				{
					if (Time.timeAsDouble - lastAim > Provider.UPDATE_TIME)
					{
						lastAim = Time.timeAsDouble;

						Transform targetTransform = null;
						Vector3 toPoint = Vector3.zero;

						if (targetPlayer != null)
						{
							targetTransform = targetPlayer.transform;
							toPoint = targetPlayer.look.aim.position;
						}
						else if (targetZombie != null)
						{
							targetTransform = targetZombie.transform;

							toPoint = targetZombie.transform.position;
							switch (targetZombie.speciality)
							{
								case EZombieSpeciality.CRAWLER:
									toPoint += new Vector3(0.0f, 0.25f, 0.0f);
									break;
								case EZombieSpeciality.MEGA:
									toPoint += new Vector3(0.0f, 2.625f, 0.0f);
									break;
								case EZombieSpeciality.NORMAL:
									toPoint += new Vector3(0.0f, 1.75f, 0.0f);
									break;
								case EZombieSpeciality.SPRINTER:
									toPoint += new Vector3(0.0f, 1.0f, 0.0f);
									break;
							}
						}
						else if (targetAnimal != null)
						{
							targetTransform = targetAnimal.transform;
							toPoint = targetAnimal.transform.position + Vector3.up;
						}
						else if (targetVehicle != null)
						{
							targetTransform = targetVehicle.transform;
							toPoint = targetVehicle.GetSentryTargetingPoint();
						}

						if (targetTransform != null)
						{
							float aimYaw = Mathf.Atan2(toPoint.x - fromPoint.x, toPoint.z - fromPoint.z) * Mathf.Rad2Deg;
							float aimPitch = Mathf.Sin((toPoint.y - fromPoint.y) / (toPoint - fromPoint).magnitude) * Mathf.Rad2Deg;

							BarricadeManager.sendAlertSentry(transform, aimYaw, aimPitch);
						}
					}
				}

				if (isFiring && hasWeapon && !isOpen)
				{
					bool hasEnoughAmmoToFire = false;
					bool infiniteAmmo = sentryAsset.infiniteAmmo || ((ItemGunAsset) displayAsset).infiniteAmmo;
					if (infiniteAmmo)
					{
						hasEnoughAmmoToFire = true;
					}
					else
					{
						hasEnoughAmmoToFire = displayItem.state[10] >= ((ItemGunAsset) displayAsset).ammoPerShot;
					}

					if (hasEnoughAmmoToFire && Time.timeAsDouble - lastFire > fireTime)
					{
						lastFire += fireTime;
						if (Time.timeAsDouble - lastFire > fireTime)
						{
							lastFire = Time.timeAsDouble;
						}

						float quality = displayItem.quality / 100f;

						if (attachments.magazineAsset == null)
						{
							return;
						}

						if (!infiniteAmmo)
						{
							if (Random.value <= sentryAsset.AmmoConsumptionProbability)
							{
								displayItem.state[10] -= ((ItemGunAsset) displayAsset).ammoPerShot;
							}
						}

						if (attachments.barrelAsset == null || !attachments.barrelAsset.isSilenced || displayItem.state[16] == 0)
						{
							AlertTool.alert(transform.position, 48);
						}

						if (sentryAsset.infiniteQuality == false && Provider.modeConfigData.Items.ShouldWeaponTakeDamage && displayItem.quality > 0 && Random.value < ((ItemWeaponAsset) displayAsset).durability)
						{
							if (Random.value <= sentryAsset.QualityConsumptionProbability)
							{
								if (displayItem.quality > ((ItemWeaponAsset) displayAsset).wear)
								{
									displayItem.quality -= ((ItemWeaponAsset) displayAsset).wear;
								}
								else
								{
									displayItem.quality = 0;
								}
							}
						}

						if (((ItemGunAsset) displayAsset).projectile == null)
						{
							float spreadAngleRadians = CalculateSpreadAngleRadians(quality);

							BarricadeManager.sendShootSentry(transform);

							float bulletDamageMultiplier = GetBulletDamageMultiplier(quality);

							byte pellets = attachments.magazineAsset.pellets;
							for (byte pellet = 0; pellet < pellets; pellet++)
							{
								EPlayerKill kill = EPlayerKill.NONE;
								uint xp = 0;

								Transform targetTransform = null;
								float targetDistance = 0.0f;

								if (targetPlayer != null)
								{
									targetTransform = targetPlayer.transform;
								}
								else if (targetZombie != null)
								{
									targetTransform = targetZombie.transform;
								}
								else if (targetAnimal != null)
								{
									targetTransform = targetAnimal.transform;
								}

								if (targetTransform != null)
								{
									targetDistance = (targetTransform.position - transform.position).magnitude;
								}

								float normalizedTargetDistance = Mathf.Clamp01(targetDistance / ((ItemWeaponAsset) displayAsset).range);
								float chanceToHit = 1.0f - normalizedTargetDistance;
								chanceToHit *= CalculateChanceToHitSpreadMultiplier(spreadAngleRadians);
								chanceToHit *= 0.75f;

								if (targetTransform == null || Random.value > chanceToHit)
								{
									Vector3 localDirection = RandomEx.GetRandomForwardVectorInCone(spreadAngleRadians);
									Vector3 direction = aimTransform.TransformDirection(localDirection);

									Ray ray = new Ray(aimTransform.position, direction);
									// Nelson 2024-12-15: There was pre-existing code here to damage vehicles, but it
									// wasn't detecting them. I'm hesitant to mess with DAMAGE_SERVER mask because of
									// its other uses, so I'm slipping vehicle in here instead.
									int layerMask = RayMasks.DAMAGE_SERVER | RayMasks.VEHICLE;
									RaycastInfo info = DamageTool.raycast(ray, ((ItemWeaponAsset) displayAsset).range, layerMask);

									if (info.transform == null)
									{
										continue;
									}

									DamageTool.ServerSpawnBulletImpact(info.point, info.normal, info.materialName, info.collider?.transform, null, Provider.GatherClientConnectionsWithinSphere(info.point, EffectManager.SMALL));

									if (info.vehicle != null)
									{
										if (info.vehicle.asset != null && info.vehicle.canBeDamaged && (info.vehicle.asset.isVulnerable || ((ItemWeaponAsset) displayAsset).isInvulnerable))
										{
											DamageTool.damage(info.vehicle, false, Vector3.zero, false, ((ItemGunAsset) displayAsset).vehicleDamage, bulletDamageMultiplier, true, out kill, damageOrigin: EDamageOrigin.Sentry);
										}
									}
									else if (info.transform != null)
									{
										if (info.transform.CompareTag("Barricade"))
										{
											BarricadeDrop barricade = BarricadeDrop.FindByRootFast(info.transform);
											if (barricade != null)
											{
												ItemBarricadeAsset asset = barricade.asset;
												if (asset != null && asset.canBeDamaged && (asset.isVulnerable || ((ItemWeaponAsset) displayAsset).isInvulnerable))
												{
													DamageTool.damage(info.transform, false, ((ItemGunAsset) displayAsset).barricadeDamage, bulletDamageMultiplier, out kill, damageOrigin: EDamageOrigin.Sentry);
												}
											}
										}
										else if (info.transform.CompareTag("Structure"))
										{
											StructureDrop structure = StructureDrop.FindByRootFast(info.transform);
											if (structure != null)
											{
												ItemStructureAsset asset = structure.asset;
												if (asset != null && asset.canBeDamaged && (asset.isVulnerable || ((ItemWeaponAsset) displayAsset).isInvulnerable))
												{
													DamageTool.damage(info.transform, false, info.direction * Mathf.Ceil(attachments.magazineAsset.pellets / 2f), ((ItemGunAsset) displayAsset).structureDamage, bulletDamageMultiplier, out kill, damageOrigin: EDamageOrigin.Sentry);
												}
											}
										}
										else if (info.transform.CompareTag("Resource"))
										{
											byte x;
											byte y;
											ushort index;
											if (ResourceManager.tryGetRegion(info.transform, out x, out y, out index))
											{
												ResourceSpawnpoint spawnpoint = ResourceManager.getResourceSpawnpoint(x, y, index);

												if (spawnpoint != null && !spawnpoint.isDead && ((ItemWeaponAsset) displayAsset).hasBladeID(spawnpoint.asset.bladeID))
												{
													DamageTool.damage(info.transform, info.direction * Mathf.Ceil(attachments.magazineAsset.pellets / 2f), ((ItemGunAsset) displayAsset).resourceDamage, bulletDamageMultiplier, 1f, out kill, out xp, damageOrigin: EDamageOrigin.Sentry);
												}
											}
										}
										else if (info.section < byte.MaxValue)
										{
											InteractableObjectRubble rubble = info.transform.GetComponentInParent<InteractableObjectRubble>();
											if (rubble != null && rubble.IsSectionIndexValid(info.section) && !rubble.isSectionDead(info.section) && ((ItemWeaponAsset) displayAsset).hasBladeID(rubble.asset.rubbleBladeID))
											{
												if (rubble.asset.rubbleIsVulnerable || ((ItemWeaponAsset) displayAsset).isInvulnerable)
												{
													DamageTool.damage(rubble.transform, info.direction, info.section, ((ItemGunAsset) displayAsset).objectDamage, bulletDamageMultiplier, out kill, out xp, damageOrigin: EDamageOrigin.Sentry);
												}
											}
										}
									}

									if (attachments.magazineAsset != null && attachments.magazineAsset.isExplosive)
									{
										Vector3 explosionPoint = info.point + info.normal * 0.25f;
										UseableGun.DetonateExplosiveMagazine(attachments.magazineAsset, explosionPoint, null, ERagdollEffect.None);
									}
								}
								else
								{
									Vector3 hitPoint = Vector3.zero;

									if (targetPlayer != null)
									{
										hitPoint = targetPlayer.look.aim.position;
									}
									else if (targetZombie != null)
									{
										hitPoint = targetZombie.transform.position;
										switch (targetZombie.speciality)
										{
											case EZombieSpeciality.CRAWLER:
												hitPoint += new Vector3(0.0f, 0.25f, 0.0f);
												break;
											case EZombieSpeciality.MEGA:
												hitPoint += new Vector3(0.0f, 2.625f, 0.0f);
												break;
											case EZombieSpeciality.NORMAL:
												hitPoint += new Vector3(0.0f, 1.75f, 0.0f);
												break;
											case EZombieSpeciality.SPRINTER:
												hitPoint += new Vector3(0.0f, 1.0f, 0.0f);
												break;
										}
									}
									else if (targetAnimal != null)
									{
										hitPoint = targetAnimal.transform.position + Vector3.up;
									}

									DamageTool.ServerSpawnBulletImpact(hitPoint, -aimTransform.forward, "Flesh_Dynamic", null, null, Provider.GatherClientConnectionsWithinSphere(hitPoint, EffectManager.SMALL));

									Vector3 dir = aimTransform.forward * Mathf.Ceil(attachments.magazineAsset.pellets / 2f);
									if (targetPlayer != null)
									{
										DamageTool.damage(targetPlayer, EDeathCause.SENTRY, ELimb.SPINE, owner, dir, ((ItemGunAsset) displayAsset).playerDamageMultiplier, bulletDamageMultiplier, true, out kill, trackKill: true);
									}
									else if (targetZombie != null)
									{
										IDamageMultiplier multiplier = ((ItemGunAsset) displayAsset).zombieOrPlayerDamageMultiplier;
										DamageZombieParameters parameters = DamageZombieParameters.make(targetZombie, dir, multiplier, ELimb.SPINE);
										parameters.times = bulletDamageMultiplier;
										parameters.allowBackstab = false;
										parameters.respectArmor = true;
										parameters.instigator = this;
										parameters.RagdollForceMultiplier = ((ItemGunAsset) displayAsset).ZombieRagdollForceMultiplier;
										DamageTool.damageZombie(parameters, out kill, out xp);
									}
									else if (targetAnimal != null)
									{
										IDamageMultiplier multiplier = ((ItemGunAsset) displayAsset).animalOrPlayerDamageMultiplier;
										DamageAnimalParameters parameters = DamageAnimalParameters.make(targetAnimal, dir, multiplier, ELimb.SPINE);
										parameters.times = bulletDamageMultiplier;
										parameters.instigator = this;
										DamageTool.damageAnimal(parameters, out kill, out xp);
									}

									if (attachments.magazineAsset != null && attachments.magazineAsset.isExplosive)
									{
										Vector3 explosionPoint = hitPoint + aimTransform.forward * -0.25f;
										UseableGun.DetonateExplosiveMagazine(attachments.magazineAsset, explosionPoint, null, ERagdollEffect.None);
									}
								}
							}
						}

						rebuildState(); // save state[] changes
					}
				}
			}

			bool newAlert = Time.timeAsDouble - lastAlert < 1.0;
			if (newAlert != isAlert)
			{
				isAlert = newAlert;

				if (!Dedicator.IsDedicatedServer)
				{
					if (isAlert)
					{
						EffectManager.effect(sentryAsset.targetAcquiredEffect, transform.position);
					}
					else
					{
						EffectManager.effect(sentryAsset.targetLostEffect, transform.position);
					}
				}

				if (!isAlert)
				{
					targetYaw = HousingConnections.GetModelYaw(transform);
				}
			}

			if (power != null)
			{
				if (isPowered)
				{
					if (isAlert)
					{
						lastDrift = Time.timeAsDouble;
						yaw = Mathf.LerpAngle(yaw, targetYaw, 4.0f * Time.deltaTime);
					}
					else
					{
						float period = (float) (Time.timeAsDouble - lastDrift);
						period *= MathfEx.TAU;
						period /= sentryAsset.SweepPeriod;
						yaw = Mathf.LerpAngle(yaw, targetYaw + (Mathf.Sin(period) * sentryAsset.SweepHalfYaw), 4.0f * Time.deltaTime);
					}

					pitch = Mathf.LerpAngle(pitch, targetPitch, 4.0f * Time.deltaTime);

					if (yawTransform != null)
					{
						yawTransform.rotation = Quaternion.Euler(-90.0f, 0, yaw);
					}

					if (pitchTransform != null)
					{
						pitchTransform.localRotation = Quaternion.Euler(0, -90.0f, pitch);
					}
				}

				if (onGameObject != null)
				{
					onGameObject.SetActive(isAlert && isPowered);
				}

				if (onModelGameObject != null)
				{
					onModelGameObject.SetActive(isAlert);
				}

				if (offGameObject != null)
				{
					offGameObject.SetActive(!isAlert && isPowered);
				}

				if (offModelGameObject != null)
				{
					offModelGameObject.SetActive(!isAlert);
				}

				if (!Dedicator.IsDedicatedServer)
				{
					if (onMaterial != null)
					{
						onMaterial.SetColor("_EmissionColor", isAlert && isPowered ? onMaterial.color * 2f : Color.black);
					}

					if (offMaterial != null)
					{
						offMaterial.SetColor("_EmissionColor", !isAlert && isPowered ? offMaterial.color * 2f : Color.black);
					}

					if (Time.timeAsDouble - lastShot > 0.05)
					{
						if (muzzleLight != null)
						{
							muzzleLight.GetComponent<Light>().enabled = false;
						}
					}
				}
			}
		}

		private void destroyEffects()
		{
			if (tracerEmitter != null)
			{
				EffectManager.DestroyIntoPool(tracerEmitter.gameObject);
				tracerEmitter = null;
			}

			if (muzzleEmitter != null)
			{
				EffectManager.DestroyIntoPool(muzzleEmitter.gameObject);
				muzzleEmitter = null;
			}
			muzzleLight = null;

			if (shellEmitter != null)
			{
				EffectManager.DestroyIntoPool(shellEmitter.gameObject);
				shellEmitter = null;
			}
		}

		private void OnDestroy()
		{
			destroyEffects();

			if (onMaterial != null)
			{
				DestroyImmediate(onMaterial);
				onMaterial = null;
			}

			if (offMaterial != null)
			{
				DestroyImmediate(offMaterial);
				offMaterial = null;
			}
		}

		internal static readonly ClientInstanceMethod SendShoot = ClientInstanceMethod.Get(typeof(InteractableSentry), nameof(ReceiveShoot));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveShoot()
		{
			shoot();
		}

		internal static readonly ClientInstanceMethod<byte, byte> SendAlert = ClientInstanceMethod<byte, byte>.Get(typeof(InteractableSentry), nameof(ReceiveAlert));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveAlert(byte yaw, byte pitch)
		{
			alert(MeasurementTool.byteToAngle(yaw), MeasurementTool.byteToAngle(pitch));
		}

		/// <summary>
		/// Calculate damage multiplier for individual bullet.
		/// </summary>
		private float GetBulletDamageMultiplier(float quality)
		{
			float damageMultiplier = quality < 0.5f ? (0.5f + quality) : 1f;

			if (attachments.magazineAsset != null)
			{
				damageMultiplier *= attachments.magazineAsset.ballisticDamageMultiplier;
			}

			if (attachments.sightAsset != null)
			{
				damageMultiplier *= attachments.sightAsset.ballisticDamageMultiplier;
			}

			if (attachments.tacticalAsset != null)
			{
				damageMultiplier *= attachments.tacticalAsset.ballisticDamageMultiplier;
			}

			if (attachments.barrelAsset != null)
			{
				damageMultiplier *= attachments.barrelAsset.ballisticDamageMultiplier;
			}

			if (attachments.gripAsset != null)
			{
				damageMultiplier *= attachments.gripAsset.ballisticDamageMultiplier;
			}

			return damageMultiplier;
		}

		private float CalculateSpreadAngleRadians(float quality)
		{
			float spread = ((ItemGunAsset) displayAsset).baseSpreadAngleRadians;
			spread *= ((ItemGunAsset) displayAsset).spreadAim;
			spread *= quality < 0.5f ? 1f + (1f - (quality * 2f)) : 1f;

			if (attachments.tacticalAsset != null && interact)
			{
				spread *= attachments.tacticalAsset.spread;
			}

			if (attachments.gripAsset != null)
			{
				spread *= attachments.gripAsset.spread;
			}

			if (attachments.barrelAsset != null)
			{
				spread *= attachments.barrelAsset.spread;
			}

			if (attachments.magazineAsset != null)
			{
				spread *= attachments.magazineAsset.spread;
			}

			return spread;
		}

		/// <summary>
		/// Each shot has a percentage chance to hit the target. Higher values are more likely to hit. e.g., it
		/// decreases from 1.0 at point blank to 0.0 at the weapon's maximum range. This chance is affected by the
		/// gun's spread.
		/// </summary>
		private float CalculateChanceToHitSpreadMultiplier(float spreadAngleRadians)
		{
			// Prior to the gun spread rewrite, the chance multiplier was calculated simply as:
			//
			// (1.0 - spreadHip)
			//
			// After the rewrite, spreadHip is converted to radians with atan (refer to ItemGunAsset code for details):
			//
			// baseSpreadAngleRadians = Mathf.Atan(spreadHip)
			//
			// So we *could* get the same result as before by returning (1 - tan(spreadAngleRadians)). This has the
			// benefit of exponentially lower chance to hit all the way to 45 degrees at which point it will never hit.
			// For the meantime I'm going to change this to cosine so that it can still hit at 45 degrees.
			if (spreadAngleRadians > MathfEx.HALF_PI)
			{
				return 0.0f;
			}
			else
			{
				return Mathf.Cos(spreadAngleRadians);
			}
		}

		public void AlertDamagedBy(Player player)
		{
			if (targetPlayer != null || targetZombie != null || targetAnimal != null || targetVehicle != null)
				return;

			if (!sentryAsset.CanReactToAttacks)
				return;

			if (!MeetsPvPRequirement)
				return;

			InteractableVehicle playerVehicle = player.movement.getVehicle();
			if (playerVehicle != null)
			{
				if (!sentryAsset.CanTargetVehicles)
					return;

				targetVehicle = playerVehicle;
				lastFire = Time.timeAsDouble + 0.1;
			}
			else
			{
				if (!sentryAsset.CanTargetPlayers)
					return;

				targetPlayer = player;
				lastFire = Time.timeAsDouble + 0.1;
			}
		}

		private void ScanForTargets(Vector3 fromPoint)
		{
			float targetDistance = sentryAsset.detectionRadius;
			float targetLossDistance = sentryAsset.targetLossRadius;
			if (hasWeapon)
			{
				float maxWeaponDistance = ((ItemWeaponAsset) displayAsset).range;
				targetDistance = Mathf.Min(targetDistance, maxWeaponDistance);
				targetLossDistance = Mathf.Min(targetLossDistance, maxWeaponDistance);
			}

			float sqrTargetDistance = targetDistance * targetDistance;
			float sqrTargetLossDistance = targetLossDistance * targetLossDistance;

			// Maximum distance to detect anyone we are not already targeting.
			float sqrClosestDistance = sqrTargetDistance;

			bool hasAnyPendingTarget = false;
			Player pendingTargetPlayer = null;
			Zombie pendingTargetZombie = null;
			Animal pendingTargetAnimal = null;
			InteractableVehicle pendingTargetVehicle = null;

			if (MeetsPvPRequirement && sentryAsset.CanTargetPlayers)
			{
				// If we have an existing player target then expand the scan radius.
				float sqrPlayerScanDistance = targetPlayer != null ? sqrTargetLossDistance : sqrClosestDistance;

				playersInRadius.Clear();
				PlayerTool.getPlayersInRadius(fromPoint, sqrPlayerScanDistance, playersInRadius);

				for (int index = 0; index < playersInRadius.Count; index++)
				{
					Player player = playersInRadius[index];

					if (player.channel.owner.playerID.steamID == owner || player.quests.isMemberOfGroup(group))
					{
						continue;
					}

					if (player.life.isDead || player.animator.gesture == EPlayerGesture.ARREST_START) // ignore arrested players
					{
						continue;
					}

					if ((player.movement.isSafe && player.movement.isSafeInfo.noIncomingDamage) || !player.movement.canAddSimulationResultsToUpdates)
					{
						continue;
					}

					if (pendingTargetPlayer != null && player.animator.gesture == EPlayerGesture.SURRENDER_START) // if we already have a target and this guy is surrendering we may as well keep the active target 
					{
						continue;
					}

					if (sentryMode == ESentryMode.FRIENDLY)
					{
						bool punchedRecently = Time.realtimeSinceStartup - player.equipment.lastPunching < 2.0f;
						if (punchedRecently == false && (player.equipment.HasValidUseable == false || player.equipment.asset == null || player.equipment.asset.shouldFriendlySentryTargetUser == false)) // friendly sentry ignores players with non-dangerous items
						{
							continue;
						}
					}

					float sqrDistance = (player.look.aim.position - fromPoint).sqrMagnitude;

					if (player != targetPlayer && sqrDistance > sqrClosestDistance)
					{
						continue;
					}

					Vector3 diff = player.look.aim.position - fromPoint;
					float dist = diff.magnitude;
					Vector3 norm = diff / dist;

					if (player != targetPlayer)
					{
						if (Vector3.Dot(norm, aimTransform.forward) < 0.5f)
						{
							continue;
						}
					}

					if (dist > 0.025f)
					{
						RaycastHit obstruction;
						Physics.Raycast(new Ray(fromPoint, norm), out obstruction, dist - 0.025f, RayMasks.BLOCK_SENTRY);

						if (obstruction.transform != null && obstruction.transform != transform)
						{
							continue;
						}
						else
						{
							Physics.Raycast(new Ray(fromPoint + (norm * (dist - 0.025f)), -norm), out obstruction, dist - 0.025f, RayMasks.DAMAGE_SERVER);

							if (obstruction.transform != null && obstruction.transform != transform)
							{
								continue; ;
							}
						}
					}

					sqrClosestDistance = sqrDistance;
					pendingTargetPlayer = player;
					hasAnyPendingTarget = true;
				}
			}

			if (sentryAsset.CanTargetZombies)
			{
				// If we have an existing zombie target then expand the scan radius.
				float sqrZombieScanDistance = !hasAnyPendingTarget && targetZombie != null ? sqrTargetLossDistance : sqrClosestDistance;

				zombiesInRadius.Clear();
				ZombieManager.getZombiesInRadius(fromPoint, sqrZombieScanDistance, zombiesInRadius);

				for (int index = 0; index < zombiesInRadius.Count; index++)
				{
					Zombie zombie = zombiesInRadius[index];

					if (zombie.isDead || !zombie.isHunting)
					{
						continue;
					}

					Vector3 toPoint = zombie.transform.position;

					switch (zombie.speciality)
					{
						case EZombieSpeciality.CRAWLER:
							toPoint += new Vector3(0.0f, 0.25f, 0.0f);
							break;
						case EZombieSpeciality.MEGA:
							toPoint += new Vector3(0.0f, 2.625f, 0.0f);
							break;
						case EZombieSpeciality.NORMAL:
							toPoint += new Vector3(0.0f, 1.75f, 0.0f);
							break;
						case EZombieSpeciality.SPRINTER:
							toPoint += new Vector3(0.0f, 1.0f, 0.0f);
							break;
					}

					float sqrDistance = (toPoint - fromPoint).sqrMagnitude;

					if (zombie != targetZombie && sqrDistance > sqrClosestDistance)
					{
						continue;
					}

					Vector3 diff = toPoint - fromPoint;
					float dist = diff.magnitude;
					Vector3 norm = diff / dist;

					if (zombie != targetZombie)
					{
						if (Vector3.Dot(norm, aimTransform.forward) < 0.5f)
						{
							continue;
						}
					}

					if (dist > 0.025f)
					{
						RaycastHit obstruction;
						Physics.Raycast(new Ray(fromPoint, norm), out obstruction, dist - 0.025f, RayMasks.BLOCK_SENTRY);

						if (obstruction.transform != null && obstruction.transform != transform)
						{
							continue;
						}
						else
						{
							Physics.Raycast(new Ray(fromPoint + (norm * (dist - 0.025f)), -norm), out obstruction, dist - 0.025f, RayMasks.DAMAGE_SERVER);

							if (obstruction.transform != null && obstruction.transform != transform)
							{
								continue;
							}
						}
					}

					sqrClosestDistance = sqrDistance;
					pendingTargetPlayer = null;
					pendingTargetZombie = zombie;
					hasAnyPendingTarget = true;
				}
			}

			if (sentryAsset.CanTargetAnimals)
			{
				// If we have an existing animal target then expand the scan radius.
				float sqrAnimalScanDistance = !hasAnyPendingTarget && targetAnimal != null ? sqrTargetLossDistance : sqrClosestDistance;

				animalsInRadius.Clear();
				AnimalManager.getAnimalsInRadius(fromPoint, sqrAnimalScanDistance, animalsInRadius);

				for (int index = 0; index < animalsInRadius.Count; index++)
				{
					Animal animal = animalsInRadius[index];

					if (animal.isDead)
					{
						continue;
					}

					Vector3 toPoint = animal.transform.position;

					float sqrDistance = (toPoint - fromPoint).sqrMagnitude;

					if (animal != targetAnimal && sqrDistance > sqrClosestDistance)
					{
						continue;
					}

					Vector3 diff = toPoint - fromPoint;
					float dist = diff.magnitude;
					Vector3 norm = diff / dist;

					if (animal != targetAnimal)
					{
						if (Vector3.Dot(norm, aimTransform.forward) < 0.5f)
						{
							continue;
						}
					}

					if (dist > 0.025f)
					{
						RaycastHit obstruction;
						Physics.Raycast(new Ray(fromPoint, norm), out obstruction, dist - 0.025f, RayMasks.BLOCK_SENTRY);

						if (obstruction.transform != null && obstruction.transform != transform)
						{
							continue;
						}
						else
						{
							Physics.Raycast(new Ray(fromPoint + (norm * (dist - 0.025f)), -norm), out obstruction, dist - 0.025f, RayMasks.DAMAGE_SERVER);

							if (obstruction.transform != null && obstruction.transform != transform)
							{
								continue;
							}
						}
					}

					sqrClosestDistance = sqrDistance;
					pendingTargetPlayer = null;
					pendingTargetZombie = null;
					pendingTargetAnimal = animal;
					hasAnyPendingTarget = true;
				}
			}

			if (MeetsPvPRequirement && sentryMode == ESentryMode.HOSTILE && sentryAsset.CanTargetVehicles)
			{
				// If we have an existing vehicle target then expand the scan radius.
				float sqrVehicleScanDistance = !hasAnyPendingTarget && targetVehicle != null ? sqrTargetLossDistance : sqrClosestDistance;

				vehiclesInRadius.Clear();
				VehicleManager.getVehiclesInRadius(fromPoint, sqrVehicleScanDistance, vehiclesInRadius);

				for (int index = 0; index < vehiclesInRadius.Count; index++)
				{
					InteractableVehicle vehicle = vehiclesInRadius[index];

					if (vehicle.isDead)
						continue;

					if (vehicle.isInsideSafezone)
						continue;

					if (vehicle.IsFriendlyToSentry(this))
						continue;

					if (!vehicle.anySeatsOccupied)
						continue;

					Vector3 vehiclePoint = vehicle.GetSentryTargetingPoint();
					float sqrDistance = (vehiclePoint - fromPoint).sqrMagnitude;

					if (vehicle != targetVehicle && sqrDistance > sqrClosestDistance)
					{
						continue;
					}

					Vector3 diff = vehiclePoint - fromPoint;
					float dist = diff.magnitude;
					Vector3 norm = diff / dist;

					if (vehicle != targetVehicle)
					{
						if (Vector3.Dot(norm, aimTransform.forward) < 0.5f)
						{
							continue;
						}
					}

					if (dist > 0.025f)
					{
						RaycastHit obstruction;
						Physics.Raycast(new Ray(fromPoint, norm), out obstruction, dist - 0.025f, RayMasks.BLOCK_SENTRY);

						if (obstruction.transform != null && obstruction.transform != transform && !obstruction.transform.IsChildOf(vehicle.transform))
						{
							continue;
						}
						else
						{
							Physics.Raycast(new Ray(fromPoint + (norm * (dist - 0.025f)), -norm), out obstruction, dist - 0.025f, RayMasks.DAMAGE_SERVER);

							if (obstruction.transform != null && obstruction.transform != transform && !obstruction.transform.IsChildOf(vehicle.transform))
							{
								continue;
							}
						}
					}

					sqrClosestDistance = sqrDistance;
					pendingTargetVehicle = vehicle;
					hasAnyPendingTarget = true;
				}
			}

			if (pendingTargetPlayer != targetPlayer || pendingTargetZombie != targetZombie || pendingTargetAnimal != targetAnimal || pendingTargetVehicle != targetVehicle)
			{
				targetPlayer = pendingTargetPlayer;
				targetZombie = pendingTargetZombie;
				targetAnimal = pendingTargetAnimal;
				targetVehicle = pendingTargetVehicle;

				lastFire = Time.timeAsDouble + 0.1;
			}
		}

		private bool MeetsPvPRequirement => Provider.isPvP || sentryAsset.BypassesPvEMode;

		private bool hasInitializedSentryComponents;
	}
}
