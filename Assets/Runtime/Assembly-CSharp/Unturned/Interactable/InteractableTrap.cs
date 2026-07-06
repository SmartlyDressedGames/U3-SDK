////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableTrapDamageTires : MonoBehaviour
	{ }

	public class InteractableTrapTrigger : MonoBehaviour
	{
		public InteractableTrap parentTrap;

		private void OnTriggerEnter(Collider other)
		{
			if (parentTrap != null)
			{
				parentTrap.NotifyTrapEntered(other);
			}
		}
	}

	public class InteractableTrap : InteractablePower
	{
		private float range2;
		private float playerDamage;
		private float zombieDamage;
		private float animalDamage;
		private float barricadeDamage;
		private float structureDamage;
		private float vehicleDamage;
		private float resourceDamage;
		private float objectDamage;
		private float setupDelay = 0.25f;
		private float cooldown = 0.0f;
		private float explosionLaunchSpeed;
		public System.Guid trapDetonationEffectGuid;
		/// <summary>
		/// Kept because lots of modders have been using this script in Unity,
		/// so removing legacy effect id would break their content.
		/// </summary>
		private ushort explosion2;
		private bool isBroken;
		private bool isExplosive;
		private bool requiresPower;
		private float lastActive;
		private float lastTriggered;

		/// <summary>
		/// Active while powered.
		/// </summary>
		private GameObject poweredGameObject;

		public override void updateState(Asset asset, byte[] state)
		{
			ItemTrapAsset trapAsset = (ItemTrapAsset) asset;

			range2 = trapAsset.range2;
			playerDamage = trapAsset.playerDamage;
			zombieDamage = trapAsset.zombieDamage;
			animalDamage = trapAsset.animalDamage;
			barricadeDamage = trapAsset.barricadeDamage;
			structureDamage = trapAsset.structureDamage;
			vehicleDamage = trapAsset.vehicleDamage;
			resourceDamage = trapAsset.resourceDamage;
			objectDamage = trapAsset.objectDamage;
			setupDelay = trapAsset.trapSetupDelay;
			cooldown = trapAsset.trapCooldown;
			trapDetonationEffectGuid = trapAsset.trapDetonationEffectGuid;
			explosion2 = trapAsset.explosion2;
			explosionLaunchSpeed = trapAsset.explosionLaunchSpeed;
			isBroken = trapAsset.isBroken;
			isExplosive = trapAsset.isExplosive;
			requiresPower = trapAsset.requiresPower;

			if (trapAsset.damageTires)
			{
				transform.GetOrAddComponent<InteractableTrapDamageTires>();
			}

			if (requiresPower)
			{
				poweredGameObject = transform.Find("Powered")?.gameObject;
				RefreshIsConnectedToPowerWithoutNotify();
				UpdatePowerEffects();
			}
		}

		public override bool checkInteractable()
		{
			return false;
		}

		protected override void updateWired()
		{
			UpdatePowerEffects();
		}

		private void OnEnable()
		{
			lastActive = Time.realtimeSinceStartup;
		}

		private void UpdatePowerEffects()
		{
			if (poweredGameObject != null)
			{
				poweredGameObject.SetActive(isWired);
			}
		}

		internal void NotifyTrapEntered(Collider other)
		{
			if (other.isTrigger)
			{
				return;
			}

			if (Time.realtimeSinceStartup - lastActive < setupDelay)
			{
				return;
			}

			if (requiresPower && !isWired)
			{
				return;
			}

			if (other.transform.IsChildOf(transform))
			{
				return;
			}

			if (Time.realtimeSinceStartup - lastTriggered < cooldown)
			{
				return;
			}
			lastTriggered = Time.realtimeSinceStartup;

			if (Provider.isServer)
			{
				if (isExplosive)
				{
					bool shouldExplode;
					if (other.transform.CompareTag("Player"))
					{
						shouldExplode = (Provider.isPvP && (other.transform.parent == null || !other.transform.parent.CompareTag("Vehicle")))
							|| explosionLaunchSpeed > 0.01f;
					}
					else
					{
						shouldExplode = true;
					}

					if (shouldExplode)
					{
						// Save position because explosion destroys trap itself.
						Vector3 explosionPosition = transform.position;

						// Nelson 2025-08-25: fix explosion traps not self-damaging when server's
						// barricade armor multiplier is zero. (public issue #5188)
						// I.e., we can't always rely on explosion damage blowing the trap up.
						BarricadeManager.damage(transform, 5.0f, 1f, false, damageOrigin: EDamageOrigin.Trap_Wear_And_Tear);

						List<EPlayerKill> kills;
						ExplosionParameters explosionParameters = new ExplosionParameters(explosionPosition, range2, EDeathCause.LANDMINE, GetKillerId());
						explosionParameters.playerDamage = playerDamage;
						explosionParameters.zombieDamage = zombieDamage;
						explosionParameters.animalDamage = animalDamage;
						explosionParameters.barricadeDamage = barricadeDamage;
						explosionParameters.structureDamage = structureDamage;
						explosionParameters.vehicleDamage = vehicleDamage;
						explosionParameters.resourceDamage = resourceDamage;
						explosionParameters.objectDamage = objectDamage;
						explosionParameters.damageOrigin = EDamageOrigin.Trap_Explosion;
						explosionParameters.launchSpeed = explosionLaunchSpeed;
						DamageTool.explode(explosionParameters, out kills);

						EffectAsset detonationEffect = Assets.FindEffectAssetByGuidOrLegacyId(trapDetonationEffectGuid, explosion2);
						if (detonationEffect != null)
						{
							TriggerEffectParameters parameters = new TriggerEffectParameters(detonationEffect);
							parameters.position = explosionPosition;
							parameters.relevantDistance = EffectManager.LARGE;
							parameters.reliable = true;
							EffectManager.triggerEffect(parameters);
						}
					}
				}
				else
				{
					if (other.transform.CompareTag("Player"))
					{
						if (Provider.isPvP && (other.transform.parent == null || !other.transform.parent.CompareTag("Vehicle")))
						{
							Player player = DamageTool.getPlayer(other.transform);

							if (player != null)
							{
								EPlayerKill kill;
								DamageTool.damage(player, EDeathCause.SHRED, ELimb.SPINE, GetKillerId(), Vector3.up, playerDamage, 1f, out kill, trackKill: true);

								if (isBroken)
								{
									player.life.breakLegs();
								}

								DamageTool.ServerSpawnLegacyImpact(transform.position + Vector3.up,
									Vector3.down,
									"Flesh",
									null,
									Provider.GatherClientConnectionsWithinSphere(transform.position, EffectManager.SMALL));

								BarricadeManager.damage(transform, 5.0f, 1f, false, instigatorSteamID: player.channel?.owner?.playerID.steamID ?? default, damageOrigin: EDamageOrigin.Trap_Wear_And_Tear);
							}
						}
					}
					else if (other.transform.CompareTag("Agent"))
					{
						Zombie zombie = DamageTool.getZombie(other.transform);

						if (zombie != null)
						{
							DamageZombieParameters parameters = new DamageZombieParameters(zombie, transform.forward, zombieDamage);
							parameters.instigator = this;

							EPlayerKill kill;
							uint xp;
							DamageTool.damageZombie(parameters, out kill, out xp);

							DamageTool.ServerSpawnLegacyImpact(transform.position + Vector3.up,
								Vector3.down,
								zombie.isRadioactive ? "Alien" : "Flesh",
								null,
								Provider.GatherClientConnectionsWithinSphere(transform.position, EffectManager.SMALL));

							BarricadeManager.damage(transform, zombie.isHyper ? 10.0f : 5.0f, 1f, false, damageOrigin: EDamageOrigin.Trap_Wear_And_Tear);
						}
						else
						{
							Animal animal = DamageTool.getAnimal(other.transform);

							if (animal != null)
							{
								DamageAnimalParameters parameters = new DamageAnimalParameters(animal, transform.forward, animalDamage);
								parameters.instigator = this;

								EPlayerKill kill;
								uint xp;
								DamageTool.damageAnimal(parameters, out kill, out xp);

								DamageTool.ServerSpawnLegacyImpact(transform.position + Vector3.up,
									Vector3.down,
									"Flesh",
									null,
									Provider.GatherClientConnectionsWithinSphere(transform.position, EffectManager.SMALL));

								BarricadeManager.damage(transform, 5.0f, 1f, false, damageOrigin: EDamageOrigin.Trap_Wear_And_Tear);
							}
						}
					}
				}
			}
		}

		private CSteamID GetKillerId()
		{
			Transform barricadeRoot = DamageTool.getBarricadeRootTransform(transform);
			if (barricadeRoot != null)
			{
				BarricadeDrop barricade = BarricadeDrop.FindByRootFast(barricadeRoot);
				if (barricade != null)
				{
					BarricadeData serversideData = barricade.GetServersideData();
					if (serversideData != null)
					{
						return new CSteamID(serversideData.owner);
					}
				}
			}

			return CSteamID.Nil;			
		}
	}
}
