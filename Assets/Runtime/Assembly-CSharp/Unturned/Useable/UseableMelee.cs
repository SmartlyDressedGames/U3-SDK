////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableMelee : Useable
	{
		private uint startedUse;
		private float startedSwing;
		private float weakAttackAnimLengthSeconds;
		private float strongAttackAnimLengthSeconds;
		private uint weakAttackAnimLengthFrames;
		private uint strongAttackAnimLengthFrames;

		/// <summary>
		/// For non-repeat weapons the "Use" audio clip is played once time reaches this point.
		/// </summary>
		private double playUseSoundTime;

		private bool isUsing;
		private bool isSwinging;
		private ESwingMode swingMode;

		private ParticleSystem firstEmitter;
		private ParticleSystem thirdEmitter;
		private Transform firstLightHook;
		private Transform thirdLightHook;
		private Transform firstFakeLight;

		private bool interact;

		public ItemMeleeAsset equippedMeleeAsset => player.equipment.asset as ItemMeleeAsset;

		private bool isUseable
		{
			get
			{
				if (swingMode == ESwingMode.WEAK)
				{
					return player.input.simulation - startedUse > weakAttackAnimLengthFrames;
				}
				else if (swingMode == ESwingMode.STRONG)
				{
					return player.input.simulation - startedUse > strongAttackAnimLengthFrames;
				}

				return false;
			}
		}

		private bool isDamageable
		{
			get
			{
				if (swingMode == ESwingMode.WEAK)
				{
					return player.input.simulation - startedUse > weakAttackAnimLengthFrames * equippedMeleeAsset.weak;
				}
				else if (swingMode == ESwingMode.STRONG)
				{
					return player.input.simulation - startedUse > strongAttackAnimLengthFrames * equippedMeleeAsset.strong;
				}

				return false;
			}
		}

		private void swing()
		{
			startedUse = player.input.simulation;
			startedSwing = Time.realtimeSinceStartup;
			isUsing = true;
			isSwinging = true;

			if (swingMode == ESwingMode.WEAK)
			{
				player.animator.play("Weak", false);
				playUseSoundTime = Time.timeAsDouble + weakAttackAnimLengthSeconds * equippedMeleeAsset.weak;
			}
			else if (swingMode == ESwingMode.STRONG)
			{
				player.animator.play("Strong", false);
				playUseSoundTime = Time.timeAsDouble + strongAttackAnimLengthSeconds * equippedMeleeAsset.strong;
			}
		}

		private void startSwing()
		{
			startedUse = player.input.simulation;
			startedSwing = Time.realtimeSinceStartup;
			isUsing = true;
			isSwinging = true;

			player.animator.play("Start_Swing", false);
		}

		private void stopSwing()
		{
			isUsing = false;
			isSwinging = false;

			player.animator.play("Stop_Swing", false);
		}


		private static ClientInstanceMethod<Vector3, Vector3, string, Transform> SendSpawnMeleeImpact = ClientInstanceMethod<Vector3, Vector3, string, Transform>.Get(typeof(UseableMelee), nameof(ReceiveSpawnMeleeImpact));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveSpawnMeleeImpact(Vector3 position, [NetPakNormal] Vector3 normal, string materialName, Transform colliderTransform)
		{
#if !DEDICATED_SERVER
			DamageTool.LocalSpawnBulletImpactEffect(position, normal, materialName, colliderTransform);
			DamageTool.PlayMeleeImpactAudio(position, materialName);

			if (equippedMeleeAsset == null)
			{
				return;
			}

			AudioReference impactAudio = player.equipment.GetUseableSpecialAudioOverride();
			if (impactAudio.IsNullOrEmpty)
			{
				impactAudio = equippedMeleeAsset.impactAudio;
			}

			float volumeMultiplier;
			float pitchMultiplier;
			AudioClip audioClip = impactAudio.LoadAudioClip(out volumeMultiplier, out pitchMultiplier);
			if (audioClip == null)
			{
				return;
			}

			OneShotAudioParameters parameters = new OneShotAudioParameters(position, audioClip);
			parameters.volume = volumeMultiplier;
			parameters.pitch = pitchMultiplier;
			parameters.SetLinearRolloff(1.0f, 16.0f);
			parameters.Play();
#endif // !DEDICATED_SERVER
		}

		internal void ServerSpawnMeleeImpact(Vector3 position, Vector3 normal, string materialName, Transform colliderTransform, List<ITransportConnection> transportConnections)
		{
			// Old code offsets position as well.
			position += normal * Random.Range(0.04f, 0.06f);

			SendSpawnMeleeImpact.Invoke(GetNetId(), ENetReliability.Unreliable, transportConnections, position, normal, materialName, colliderTransform);
		}

		[System.Obsolete]
		public void askInteractMelee(CSteamID steamID)
		{
			ReceiveInteractMelee();
		}

		private static readonly ServerInstanceMethod SendInteractMelee = ServerInstanceMethod.Get(typeof(UseableMelee), nameof(ReceiveInteractMelee));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = nameof(askInteractMelee))]
		public void ReceiveInteractMelee()
		{
			if (player.equipment.isBusy)
			{
				return;
			}

			if (player.equipment.asset == null)
			{
				return;
			}

			if (!equippedMeleeAsset.isLight)
			{
				return;
			}

			interact = !interact;

			player.equipment.state[0] = (byte) (interact ? 1 : 0);
			player.equipment.sendUpdateState();

			EffectManager.TriggerFiremodeEffect(transform.position);
		}

		[System.Obsolete]
		public void askSwingStart(CSteamID steamID)
		{
			ReceivePlaySwingStart();
		}

		private static readonly ClientInstanceMethod SendPlaySwingStart = ClientInstanceMethod.Get(typeof(UseableMelee), nameof(ReceivePlaySwingStart));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askSwingStart))]
		public void ReceivePlaySwingStart()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				startSwing();
			}
		}

		[System.Obsolete]
		public void askSwingStop(CSteamID steamID)
		{
			ReceivePlaySwingStop();
		}

		private static readonly ClientInstanceMethod SendPlaySwingStop = ClientInstanceMethod.Get(typeof(UseableMelee), nameof(ReceivePlaySwingStop));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askSwingStop))]
		public void ReceivePlaySwingStop()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				stopSwing();
			}
		}

		[System.Obsolete]
		public void askSwing(CSteamID steamID, byte mode)
		{
			ReceivePlaySwing((ESwingMode) mode);
		}

		private static readonly ClientInstanceMethod<ESwingMode> SendPlaySwing = ClientInstanceMethod<ESwingMode>.Get(typeof(UseableMelee), nameof(ReceivePlaySwing));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askSwing))]
		public void ReceivePlaySwing(ESwingMode mode)
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				swingMode = mode;

				swing();
			}
		}

		private void fire()
		{
			float quality = player.equipment.quality / 100f;

			if (Provider.isServer)
			{
				AlertTool.alert(transform.position, equippedMeleeAsset.alertRadius);
			}

			if (channel.IsLocalPlayer)
			{
				int data;
				if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Shot", out data))
				{
					Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Shot", data + 1);
				}

				Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
				RaycastInfo info = DamageTool.raycast(ray, ((ItemWeaponAsset) player.equipment.asset).range, RayMasks.DAMAGE_CLIENT, ignorePlayer: player);

				if (info.player != null && equippedMeleeAsset.playerDamageMultiplier.damage > 1 && (DamageTool.isPlayerAllowedToDamagePlayer(player, info.player) || equippedMeleeAsset.bypassAllowedToDamagePlayer))
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
				else if ((info.zombie != null && equippedMeleeAsset.zombieDamageMultiplier.damage > 1) || (info.animal != null && equippedMeleeAsset.animalDamageMultiplier.damage > 1))
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
				else if (info.vehicle != null && equippedMeleeAsset.vehicleDamage > 1)
				{
					if (equippedMeleeAsset.isRepair)
					{
						if (!info.vehicle.isExploded && !info.vehicle.isRepaired && info.vehicle.canPlayerRepair(player))
						{
							if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Hit", out data))
							{
								Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Hit", data + 1);
							}

							PlayerUI.hitmark(info.point, false, EPlayerHit.BUILD);
						}
					}
					else
					{
						if (!info.vehicle.isDead)
						{
							if (info.vehicle.asset != null && info.vehicle.canBeDamaged && (info.vehicle.asset.isVulnerable || ((ItemWeaponAsset) player.equipment.asset).isInvulnerable))
							{
								if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Hit", out data))
								{
									Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Hit", data + 1);
								}

								PlayerUI.hitmark(info.point, false, EPlayerHit.BUILD);
							}
						}
					}
				}
				else if (info.transform != null && info.transform.CompareTag("Barricade") && equippedMeleeAsset.barricadeDamage > 1)
				{
					BarricadeDrop barricade = BarricadeDrop.FindByRootFast(info.transform);
					if (barricade != null)
					{
						ItemBarricadeAsset asset = barricade.asset;
						if (asset != null)
						{
							if (equippedMeleeAsset.isRepair)
							{
								Interactable2HP hp = info.transform.GetComponent<Interactable2HP>();

								if (hp != null)
								{
									if (asset.isRepairable && hp.hp < 100)
									{
										if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Hit", out data))
										{
											Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Hit", data + 1);
										}

										PlayerUI.hitmark(info.point, false, EPlayerHit.BUILD);
									}
								}
							}
							else
							{
								if (asset.canBeDamaged && (asset.isVulnerable || ((ItemWeaponAsset) player.equipment.asset).isInvulnerable))
								{
									if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Hit", out data))
									{
										Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Hit", data + 1);
									}

									PlayerUI.hitmark(info.point, false, EPlayerHit.BUILD);
								}
							}
						}
					}
				}
				else if (info.transform != null && info.transform.CompareTag("Structure") && equippedMeleeAsset.structureDamage > 1)
				{
					StructureDrop structure = StructureDrop.FindByRootFast(info.transform);
					if (structure != null)
					{
						ItemStructureAsset asset = structure.asset;
						if (asset != null)
						{
							if (equippedMeleeAsset.isRepair)
							{
								Interactable2HP hp = info.transform.GetComponent<Interactable2HP>();

								if (hp != null)
								{
									if (asset.isRepairable && hp.hp < 100)
									{
										if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Hit", out data))
										{
											Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Hit", data + 1);
										}

										PlayerUI.hitmark(info.point, false, EPlayerHit.BUILD);
									}
								}
							}
							else
							{
								if (asset.canBeDamaged && (asset.isVulnerable || ((ItemWeaponAsset) player.equipment.asset).isInvulnerable))
								{
									if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Hit", out data))
									{
										Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Hit", data + 1);
									}

									PlayerUI.hitmark(info.point, false, EPlayerHit.BUILD);
								}
							}
						}
					}
				}
				else if (info.transform != null && info.transform.CompareTag("Resource") && equippedMeleeAsset.resourceDamage > 1)
				{
					byte x;
					byte y;
					ushort index;
					if (ResourceManager.tryGetRegion(info.transform, out x, out y, out index))
					{
						ResourceSpawnpoint spawnpoint = ResourceManager.getResourceSpawnpoint(x, y, index);
						bool vulnerable = spawnpoint.asset.vulnerableToAllMeleeWeapons || equippedMeleeAsset.hasBladeID(spawnpoint.asset.bladeID);

						if (spawnpoint != null && !spawnpoint.isDead && vulnerable)
						{
							if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Hit", out data))
							{
								Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Hit", data + 1);
							}

							PlayerUI.hitmark(info.point, false, EPlayerHit.BUILD);
						}
					}
				}
				else if (info.transform != null && equippedMeleeAsset.objectDamage > 1)
				{
					InteractableObjectRubble rubble = info.transform.GetComponentInParent<InteractableObjectRubble>();
					if (rubble != null)
					{
						info.transform = rubble.transform;
						info.section = rubble.getSection(info.collider.transform);
						if (rubble.IsSectionIndexValid(info.section) && !rubble.isSectionDead(info.section) && equippedMeleeAsset.hasBladeID(rubble.asset.rubbleBladeID))
						{
							if (rubble.asset.rubbleIsVulnerable || ((ItemWeaponAsset) player.equipment.asset).isInvulnerable)
							{
								if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Hit", out data))
								{
									Provider.provider.statisticsService.userStatisticsService.setStatistic("Accuracy_Hit", data + 1);
								}

								PlayerUI.hitmark(info.point, false, EPlayerHit.BUILD);
							}
						}
					}
				}

				if (!equippedMeleeAsset.allowFleshFx)
				{
					if (info.player != null || info.animal != null || info.zombie != null)
					{
#pragma warning disable
						info.material = EPhysicsMaterial.NONE;
#pragma warning restore
						info.materialName = string.Empty;
					}
				}

				player.input.sendRaycast(info, ERaycastInfoUsage.Melee);
			}

			if (Provider.isServer)
			{
				switch (swingMode)
				{
					case ESwingMode.WEAK:
						equippedMeleeAsset.weakAttackQuestRewards.Grant(player);
						break;

					case ESwingMode.STRONG:
						equippedMeleeAsset.strongAttackQuestRewards.Grant(player);
						break;
				}

				if (!player.input.hasInputs())
				{
					return;
				}

				InputInfo info = player.input.getInput(true, ERaycastInfoUsage.Melee);

				if (info == null)
				{
					return;
				}

				if ((info.point - player.look.aim.position).sqrMagnitude > MathfEx.Square(equippedMeleeAsset.range + 4))
				{
					return;
				}

				if ((!equippedMeleeAsset.isRepair || !equippedMeleeAsset.isRepeated) && !string.IsNullOrEmpty(info.materialName))
				{
					ServerSpawnMeleeImpact(info.point,
						info.normal,
						info.materialName,
						info.colliderTransform,
						channel.GatherOwnerAndClientConnectionsWithinSphere(info.point, EffectManager.SMALL));
				}

				EPlayerKill kill = EPlayerKill.NONE;
				uint xp = 0;

				float times = 1;
				times *= 1f + (channel.owner.player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.OVERKILL) * 0.5f);
				times *= swingMode == ESwingMode.STRONG ? equippedMeleeAsset.strength : 1f;
				times *= quality < 0.5f ? (0.5f + quality) : 1f;

				ERagdollEffect ragdollEffect = player.equipment.getUseableRagdollEffect();

				if (info.type != ERaycastInfoType.NONE && info.type != ERaycastInfoType.SKIP)
				{
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

				if (info.type == ERaycastInfoType.PLAYER)
				{
					if (info.player != null)
					{
						if (DamageTool.isPlayerAllowedToDamagePlayer(player, info.player) || equippedMeleeAsset.bypassAllowedToDamagePlayer)
						{
							IDamageMultiplier multiplier = equippedMeleeAsset.playerDamageMultiplier;
							DamagePlayerParameters parameters = DamagePlayerParameters.make(info.player, EDeathCause.MELEE, info.direction, multiplier, info.limb);
							parameters.killer = channel.owner.playerID.steamID;
							parameters.times = times;
							parameters.respectArmor = true;
							parameters.trackKill = true;
							parameters.ragdollEffect = ragdollEffect;
							equippedMeleeAsset.initPlayerDamageParameters(ref parameters);

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
						EZombieStunOverride stunOverride = equippedMeleeAsset.zombieStunOverride;
						if (Provider.modeConfigData.Zombies.Only_Critical_Stuns && stunOverride == EZombieStunOverride.None)
						{
							if (swingMode == ESwingMode.STRONG)
							{
								stunOverride = EZombieStunOverride.Always;
							}
						}

						IDamageMultiplier multiplier = equippedMeleeAsset.zombieOrPlayerDamageMultiplier;
						DamageZombieParameters parameters = DamageZombieParameters.make(info.zombie, info.direction, multiplier, info.limb);
						parameters.times = times;
						parameters.allowBackstab = true;
						parameters.respectArmor = true;
						parameters.instigator = player;
						parameters.zombieStunOverride = stunOverride;
						parameters.ragdollEffect = ragdollEffect;
						parameters.RagdollForceMultiplier = equippedMeleeAsset.ZombieRagdollForceMultiplier;

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
						IDamageMultiplier multiplier = equippedMeleeAsset.animalOrPlayerDamageMultiplier;
						DamageAnimalParameters parameters = DamageAnimalParameters.make(info.animal, info.direction, multiplier, info.limb);
						parameters.times = times;
						parameters.instigator = player;
						parameters.ragdollEffect = ragdollEffect;
						parameters.AlertPosition = transform.position;
						DamageTool.damageAnimal(parameters, out kill, out xp);
					}
				}
				else if (info.type == ERaycastInfoType.VEHICLE)
				{
					if (info.vehicle != null && info.vehicle.asset != null)
					{
						if (equippedMeleeAsset.isRepair)
						{
							if (!info.vehicle.isExploded && !info.vehicle.isRepaired && info.vehicle.canPlayerRepair(player))
							{
								times *= 1f + channel.owner.player.skills.mastery((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.MECHANIC);
								DamageTool.damage(info.vehicle, true, info.point, equippedMeleeAsset.isRepair, equippedMeleeAsset.vehicleDamage, times * Provider.modeConfigData.Vehicles.Melee_Repair_Multiplier, true, out kill, instigatorSteamID: channel.owner.playerID.steamID, damageOrigin: EDamageOrigin.Useable_Melee);
							}
						}
						else
						{
							if (info.vehicle.canBeDamaged && (info.vehicle.asset.isVulnerable || equippedMeleeAsset.isInvulnerable))
							{
								DamageTool.damage(info.vehicle, true, info.point, equippedMeleeAsset.isRepair, equippedMeleeAsset.vehicleDamage, times * Provider.modeConfigData.Vehicles.Melee_Damage_Multiplier, true, out kill, instigatorSteamID: channel.owner.playerID.steamID, damageOrigin: EDamageOrigin.Useable_Melee);
							}
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
							if (asset != null)
							{
								if (equippedMeleeAsset.isRepair)
								{
									if (asset.isRepairable)
									{
										times *= 1f + channel.owner.player.skills.mastery((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.MECHANIC);
										DamageTool.damage(info.transform, true, equippedMeleeAsset.barricadeDamage, times * Provider.modeConfigData.Barricades.Melee_Repair_Multiplier, out kill, instigatorSteamID: channel.owner.playerID.steamID);
									}
								}
								else
								{
									if (asset.canBeDamaged && (asset.isVulnerable || ((ItemWeaponAsset) player.equipment.asset).isInvulnerable))
									{
										if (barricade.interactable is InteractableSentry sentry)
										{
											sentry.AlertDamagedBy(player);
										}

										DamageTool.damage(info.transform, false, equippedMeleeAsset.barricadeDamage, times * Provider.modeConfigData.Barricades.Melee_Damage_Multiplier, out kill, instigatorSteamID: channel.owner.playerID.steamID, damageOrigin: EDamageOrigin.Useable_Melee);
									}
								}
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
							if (asset != null)
							{
								if (equippedMeleeAsset.isRepair)
								{
									if (asset.isRepairable)
									{
										times *= 1f + channel.owner.player.skills.mastery((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.MECHANIC);
										DamageTool.damage(info.transform, true, info.direction, equippedMeleeAsset.structureDamage, times * Provider.modeConfigData.Structures.Melee_Repair_Multiplier, out kill, instigatorSteamID: channel.owner.playerID.steamID, damageOrigin: EDamageOrigin.Useable_Melee);
									}
								}
								else
								{
									if (asset.canBeDamaged && (asset.isVulnerable || ((ItemWeaponAsset) player.equipment.asset).isInvulnerable))
									{
										DamageTool.damage(info.transform, false, info.direction, equippedMeleeAsset.structureDamage, times * Provider.modeConfigData.Structures.Melee_Damage_Multiplier, out kill, instigatorSteamID: channel.owner.playerID.steamID, damageOrigin: EDamageOrigin.Useable_Melee);
									}
								}
							}
						}
					}
				}
				else if (info.type == ERaycastInfoType.RESOURCE)
				{
					if (info.transform != null && info.transform.CompareTag("Resource"))
					{
						times *= 1f + (channel.owner.player.skills.mastery((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.OUTDOORS) * 0.5f);

						byte x;
						byte y;
						ushort index;
						if (ResourceManager.tryGetRegion(info.transform, out x, out y, out index))
						{
							ResourceSpawnpoint spawnpoint = ResourceManager.getResourceSpawnpoint(x, y, index);
							bool vulnerable = spawnpoint.asset.vulnerableToAllMeleeWeapons || equippedMeleeAsset.hasBladeID(spawnpoint.asset.bladeID);

							if (spawnpoint != null && !spawnpoint.isDead && vulnerable)
							{
								DamageTool.damage(info.transform, info.direction, equippedMeleeAsset.resourceDamage, times, 1f + (channel.owner.player.skills.mastery((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.OUTDOORS) * 0.5f), out kill, out xp, instigatorSteamID: channel.owner.playerID.steamID, damageOrigin: EDamageOrigin.Useable_Melee);
							}
						}
					}
				}
				else if (info.type == ERaycastInfoType.OBJECT)
				{
					if (info.transform != null && info.section < byte.MaxValue)
					{
						InteractableObjectRubble rubble = info.transform.GetComponentInParent<InteractableObjectRubble>();
						if (rubble != null && rubble.IsSectionIndexValid(info.section) && !rubble.isSectionDead(info.section) && equippedMeleeAsset.hasBladeID(rubble.asset.rubbleBladeID))
						{
							if (rubble.asset.rubbleIsVulnerable || ((ItemWeaponAsset) player.equipment.asset).isInvulnerable)
							{
								DamageTool.damage(rubble.transform, info.direction, info.section, equippedMeleeAsset.objectDamage, times, out kill, out xp, instigatorSteamID: channel.owner.playerID.steamID, damageOrigin: EDamageOrigin.Useable_Melee);
							}
						}
					}
				}

				// only do aggressor check if we didn't shoot a player (because we would already be marked aggressor) and if we weren't saving them from a zombie
				if (info.type != ERaycastInfoType.PLAYER && info.type != ERaycastInfoType.ZOMBIE && info.type != ERaycastInfoType.ANIMAL)
				{
					if (!player.life.isAggressor)
					{
						float bulletRange = equippedMeleeAsset.range + Provider.modeConfigData.Players.Ray_Aggressor_Distance;
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

		public override bool startPrimary()
		{
			if (player.equipment.isBusy || player.quests.IsCutsceneModeActive())
			{
				return false;
			}

			if (equippedMeleeAsset.isRepeated)
			{
				if (!isSwinging)
				{
					swingMode = ESwingMode.WEAK;
					startSwing();

					if (Provider.isServer)
					{
						SendPlaySwingStart.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
					}
					return true;
				}
			}
			else
			{
				if (isUseable)
				{
					player.equipment.isBusy = true;
					startedUse = player.input.simulation;
					startedSwing = Time.realtimeSinceStartup;
					isUsing = true;

					swingMode = ESwingMode.WEAK;
					swing();

					if (Provider.isServer)
					{
						SendPlaySwing.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner(), swingMode);
					}
					return true;
				}
			}

			return false;
		}

		public override void stopPrimary()
		{
			if (player.equipment.isBusy || player.quests.IsCutsceneModeActive())
			{
				return;
			}

			if (equippedMeleeAsset.isRepeated)
			{
				if (isSwinging)
				{
					stopSwing();

					if (Provider.isServer)
					{
						SendPlaySwingStop.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
					}
				}
			}
		}

		public override bool startSecondary()
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			if (!equippedMeleeAsset.isRepeated)
			{
				if (isUseable && player.life.stamina >= equippedMeleeAsset.stamina * (1f - (player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.EXERCISE) * 0.75f)))
				{
					player.life.askTire((byte) (equippedMeleeAsset.stamina * (1f - (player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.EXERCISE) * 0.5f))));

					player.equipment.isBusy = true;

					swingMode = ESwingMode.STRONG;
					swing();

					if (Provider.isServer)
					{
						SendPlaySwing.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner(), swingMode);
					}

					return true;
				}
			}

			return false;
		}

		public override bool canInspect => !isUsing && !isSwinging;

		public override void equip()
		{
			player.animator.play("Equip", true);

			if (equippedMeleeAsset.isLight)
			{
				interact = player.equipment.state[0] == 1;

				if (channel.IsLocalPlayer)
				{
					firstLightHook = player.equipment.firstModel.Find("Model_0").Find("Light");
					firstLightHook.tag = "Viewmodel";
					firstLightHook.gameObject.layer = LayerMasks.VIEWMODEL;

					Transform light = firstLightHook.Find("Light");
					if (light != null)
					{
						light.tag = "Viewmodel";
						light.gameObject.layer = LayerMasks.VIEWMODEL;
					}

					PlayerUI.message(EPlayerMessage.LIGHT, "");
				}

				thirdLightHook = player.equipment.thirdModel.Find("Model_0").Find("Light");
				LightLODTool.applyLightLOD(thirdLightHook);

				if (channel.IsLocalPlayer && thirdLightHook != null)
				{
					Transform light = thirdLightHook.Find("Light");
					if (light != null)
					{
						firstFakeLight = GameObject.Instantiate(light.gameObject).transform;
						firstFakeLight.name = "Emitter";
					}
				}
			}
			else
			{
				firstLightHook = null;
				thirdLightHook = null;
			}

			updateAttachments();

			if (equippedMeleeAsset.isRepeated)
			{
				if (channel.IsLocalPlayer)
				{
					if (player.equipment.firstModel.Find("Hit") != null)
					{
						firstEmitter = player.equipment.firstModel.Find("Hit").GetComponent<ParticleSystem>();
						firstEmitter.tag = "Viewmodel";
						firstEmitter.gameObject.layer = LayerMasks.VIEWMODEL;
					}
				}

				if (player.equipment.thirdModel.Find("Hit") != null)
				{
					thirdEmitter = player.equipment.thirdModel.Find("Hit").GetComponent<ParticleSystem>();
				}

				// Nelson 2024-02-06: Reviewing this code when fixing public issue #3871 I don't believe the length of
				// Stop_Swing is actually used, but keeping it just in case to preserve existing behavior.
				weakAttackAnimLengthSeconds = player.animator.GetAnimationLength("Start_Swing");
				strongAttackAnimLengthSeconds = player.animator.GetAnimationLength("Stop_Swing");
			}
			else
			{
				weakAttackAnimLengthSeconds = player.animator.GetAnimationLength("Weak");
				strongAttackAnimLengthSeconds = player.animator.GetAnimationLength("Strong");
			}

			// Nelson 2024-02-06: Preserving pre-existing cast to uint to avoid affecting gameplay timings.
			weakAttackAnimLengthFrames = (uint) (weakAttackAnimLengthSeconds / PlayerInput.RATE);
			strongAttackAnimLengthFrames = (uint) (strongAttackAnimLengthSeconds / PlayerInput.RATE);
		}

		public override void dequip()
		{
			player.disableItemSpotLight();

			if (channel.IsLocalPlayer)
			{
				player.animator.viewmodelCameraLocalPositionOffset = Vector3.zero;

				if (firstFakeLight != null)
				{
					Destroy(firstFakeLight.gameObject);
					firstFakeLight = null;
				}
			}
		}

		public override void updateState(byte[] newState)
		{
			if (equippedMeleeAsset.isLight)
			{
				interact = newState[0] == 1;
			}

			updateAttachments();
		}

		public override void tick()
		{
			if (!player.equipment.IsEquipAnimationFinished)
			{
				return;
			}

			if (isSwinging)
			{
				if (equippedMeleeAsset.isRepeated)
				{
					if (!Dedicator.IsDedicatedServer)
					{
						if (Time.realtimeSinceStartup - startedSwing > 0.1)
						{
							startedSwing = Time.realtimeSinceStartup;

							if (firstEmitter != null && player.look.perspective == EPlayerPerspective.FIRST)
							{
								firstEmitter.Emit(4);
							}

							if (thirdEmitter != null && (!channel.IsLocalPlayer || player.look.perspective == EPlayerPerspective.THIRD))
							{
								thirdEmitter.Emit(4);
							}

							if (equippedMeleeAsset.isRepair)
							{
								player.playSound(((ItemMeleeAsset) player.equipment.asset).use, 0.1f);
							}
							else
							{
								player.playSound(((ItemMeleeAsset) player.equipment.asset).use, 0.5f);
							}
						}
					}
				}
				else
				{
					if (Time.timeAsDouble >= playUseSoundTime)
					{
						if (!Dedicator.IsDedicatedServer)
						{
							if (swingMode == ESwingMode.WEAK)
							{
								player.playSound(((ItemMeleeAsset) player.equipment.asset).use, 0.5f);
							}
							else if (swingMode == ESwingMode.STRONG)
							{
								player.playSound(((ItemMeleeAsset) player.equipment.asset).use, 0.5f, 0.7f, 0.1f);
							}
						}

						isSwinging = false;
					}
				}
			}

			if (channel.IsLocalPlayer)
			{
				if (isSwinging)
				{
					if (equippedMeleeAsset.isRepeated && !equippedMeleeAsset.isRepair)
					{
						player.animator.viewmodelCameraLocalPositionOffset = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));
					}
					else
					{
						player.animator.viewmodelCameraLocalPositionOffset = Vector3.zero;
					}
				}

				if (InputEx.GetKeyDown(ControlsSettings.tactical))
				{
					if (equippedMeleeAsset.isLight)
					{
						SendInteractMelee.Invoke(GetNetId(), ENetReliability.Unreliable);
					}
				}
			}
		}

		public override void simulate(uint simulation, bool inputSteady)
		{
			if (isUsing && isDamageable)
			{
				if (equippedMeleeAsset.isRepeated)
				{
					startedUse = player.input.simulation;
				}
				else
				{
					player.equipment.isBusy = false;
					isUsing = false;
				}

				fire();
			}
		}

		private void updateAttachments()
		{
			if (equippedMeleeAsset.isLight)
			{
				if (!Dedicator.IsDedicatedServer)
				{
					if (channel.IsLocalPlayer && firstLightHook != null)
					{
						firstLightHook.gameObject.SetActive(interact);
					}

					if (thirdLightHook != null)
					{
						thirdLightHook.gameObject.SetActive(interact);
					}
				}

				if (interact && equippedMeleeAsset != null)
				{
					player.enableItemSpotLight(equippedMeleeAsset.lightConfig);
				}
				else
				{
					player.disableItemSpotLight();
				}
			}
		}

		private void Update()
		{
			if (channel.IsLocalPlayer)
			{
				if (firstFakeLight != null && thirdLightHook != null)
				{
					firstFakeLight.position = thirdLightHook.position;

					if (firstFakeLight.gameObject.activeSelf != (player.look.perspective == EPlayerPerspective.FIRST && thirdLightHook.gameObject.activeSelf))
					{
						firstFakeLight.gameObject.SetActive(player.look.perspective == EPlayerPerspective.FIRST && thirdLightHook.gameObject.activeSelf);
					}
				}
			}
		}
	}
}
