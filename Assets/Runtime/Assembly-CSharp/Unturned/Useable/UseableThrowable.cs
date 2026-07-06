////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableThrowable : Useable
	{
		public delegate void ThrowableSpawnedHandler(UseableThrowable useable, GameObject throwable);
		/// <summary>
		/// Plugin-only event when throwable is spawned on server.
		/// </summary>
		public static event ThrowableSpawnedHandler onThrowableSpawned;

		private float startedUse;
		private float useTime;

		private bool isUsing;
		private bool isSwinging;
		private bool hasUsed;
		private ESwingMode swingMode;

		public ItemThrowableAsset equippedThrowableAsset => player.equipment.asset as ItemThrowableAsset;

		private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

		private bool isThrowable => Time.realtimeSinceStartup - startedUse > useTime * 0.6f;

		private void toss(Vector3 origin, Vector3 force)
		{
			Quaternion rotation = Quaternion.LookRotation(force);
			Transform throwable = Instantiate(equippedThrowableAsset.throwable, origin, rotation).transform;
			throwable.name = "Throwable";
			EffectManager.RegisterDebris(throwable.gameObject);

			Rigidbody rb = throwable.GetComponent<Rigidbody>();
			if (rb != null)
			{
				rb.AddForce(force);
				rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
			}

			if (equippedThrowableAsset.isExplosive)
			{
				if (Provider.isServer)
				{
					Grenade grenade = throwable.gameObject.AddComponent<Grenade>();
					grenade.killer = channel.owner.playerID.steamID;
					grenade.range = equippedThrowableAsset.range;
					grenade.playerDamage = equippedThrowableAsset.playerDamageMultiplier.damage;
					grenade.zombieDamage = equippedThrowableAsset.zombieDamageMultiplier.damage;
					grenade.animalDamage = equippedThrowableAsset.animalDamageMultiplier.damage;
					grenade.barricadeDamage = equippedThrowableAsset.barricadeDamage;
					grenade.structureDamage = equippedThrowableAsset.structureDamage;
					grenade.vehicleDamage = equippedThrowableAsset.vehicleDamage;
					grenade.resourceDamage = equippedThrowableAsset.resourceDamage;
					grenade.objectDamage = equippedThrowableAsset.objectDamage;
					grenade.explosionEffectGuid = equippedThrowableAsset.explosionEffectGuid;
					grenade.explosion = equippedThrowableAsset.explosion;
					grenade.fuseLength = equippedThrowableAsset.fuseLength;
					grenade.explosionLaunchSpeed = equippedThrowableAsset.explosionLaunchSpeed;
				}
				else
				{
					Destroy(throwable.gameObject, equippedThrowableAsset.fuseLength);
				}
			}
			else if (equippedThrowableAsset.isFlash)
			{
				if (!Dedicator.IsDedicatedServer)
				{
					Flashbang flash = throwable.gameObject.AddComponent<Flashbang>();
					flash.fuseLength = equippedThrowableAsset.fuseLength;
				}
				else
				{
					Destroy(throwable.gameObject, equippedThrowableAsset.fuseLength);
				}
			}
			else
			{
				throwable.gameObject.AddComponent<Distraction>();

				Destroy(throwable.gameObject, equippedThrowableAsset.fuseLength);
			}

			if (equippedThrowableAsset.isSticky)
			{
				StickyGrenade sticky = throwable.gameObject.AddComponent<StickyGrenade>();
				sticky.ignoreTransform = transform;
			}

			if (equippedThrowableAsset.explodeOnImpact && (Provider.isServer || equippedThrowableAsset.ExplodeOnImpactDestroyOnClient))
			{
				// By default throwable items are on the Debris layer which passes through vehicles,
				// but impact grenades should explode when they hit a vehicle to be consistent with RPGs.
				throwable.gameObject.SetLayerRecursively(LayerMasks.TRAP);

				ImpactGrenade impact = throwable.gameObject.AddComponent<ImpactGrenade>();
				impact.explodable = throwable.GetComponent<IExplodableThrowable>();
				impact.ignoreTransform = transform;

				if (impact.explodable == null && equippedThrowableAsset.ExplodeOnImpactDestroyOnClient)
				{
					impact.explodable = throwable.gameObject.AddComponent<LocallyPredictImpactDestroyThrowable>();
				}
			}

			if (Dedicator.IsDedicatedServer)
			{
				Transform smoke = throwable.Find("Smoke");
				if (smoke != null)
				{
					Destroy(smoke.gameObject);
				}
			}

			onThrowableSpawned?.Invoke(this, throwable.gameObject);
		}

		private void swing()
		{
			isSwinging = true;

			player.animator.play("Use", false);

			if (!Dedicator.IsDedicatedServer)
			{
				player.playSound(((ItemThrowableAsset) player.equipment.asset).use);
			}

			if (Provider.isServer)
			{
				AlertTool.alert(transform.position, 8);
			}
		}

		[System.Obsolete]
		public void askToss(CSteamID steamID, Vector3 origin, Vector3 force)
		{
			ReceiveToss(origin, force);
		}

		private static readonly ClientInstanceMethod<Vector3, Vector3> SendToss = ClientInstanceMethod<Vector3, Vector3>.Get(typeof(UseableThrowable), nameof(ReceiveToss));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askToss))]
		public void ReceiveToss(Vector3 origin, Vector3 force)
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				toss(origin, force);
			}
		}

		[System.Obsolete]
		public void askSwing(CSteamID steamID)
		{
			ReceivePlaySwing();
		}

		private static readonly ClientInstanceMethod SendPlaySwing = ClientInstanceMethod.Get(typeof(UseableThrowable), nameof(ReceivePlaySwing));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askSwing))]
		public void ReceivePlaySwing()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				swing();
			}
		}

		protected bool startAttack(ESwingMode newSwingMode)
		{
			if (player.equipment.isBusy || player.quests.IsCutsceneModeActive())
				return false;

			if (hasUsed)
			{
				// Nelson 2025-01-20: Fix starting a second use after first animation finishes on client.
				// (public issue #4849)
				return false;
			}

			player.equipment.isBusy = true;
			startedUse = Time.realtimeSinceStartup;
			isUsing = true;
			swingMode = newSwingMode;
			hasUsed = true;

			swing();

			if (Provider.isServer)
			{
				if (equippedThrowableAsset.isExplosive)
				{
					player.life.markAggressive(false);
				}

				SendPlaySwing.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
			}

			return true;
		}

		public override bool startPrimary()
		{
			return startAttack(ESwingMode.STRONG);
		}

		public override bool startSecondary()
		{
			return startAttack(ESwingMode.WEAK);
		}

		public override void equip()
		{
			player.animator.play("Equip", true);

			useTime = player.animator.GetAnimationLength("Use");
		}

		public override void tick()
		{
			if (!player.equipment.IsEquipAnimationFinished)
			{
				return;
			}

			if (channel.IsLocalPlayer || Provider.isServer)
			{
				if (isSwinging && isThrowable)
				{
					Vector3 origin = player.look.aim.position;
					Vector3 direction = player.look.aim.forward;
					RaycastHit hit;
					if (Physics.Raycast(new Ray(origin, direction), out hit, 1.5f, RayMasks.DAMAGE_SERVER))
					{
						// Wall is blocking aim. Ensure the grenade spawns at least 0.5m away from the wall.
						origin += direction * (hit.distance - 0.5f);
					}
					else
					{
						origin += direction;
					}

					float forceMagnitude;
					switch (swingMode)
					{
						case ESwingMode.STRONG:
							forceMagnitude = equippedThrowableAsset.strongThrowForce;
							break;

						default:
						case ESwingMode.WEAK:
							forceMagnitude = equippedThrowableAsset.weakThrowForce;
							break;
					}

					if (player.skills.boost == EPlayerBoost.OLYMPIC)
					{
						forceMagnitude *= equippedThrowableAsset.boostForceMultiplier;
					}

					Vector3 force = direction * forceMagnitude;
					toss(origin, force);

					if (channel.IsLocalPlayer)
					{
						int data;
						if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Throwables", out data))
						{
							Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Throwables", data + 1);
						}
					}
					else
					{
						SendToss.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner(), origin, force);
					}

					if (Provider.isServer)
					{
						player.equipment.useStepA();
					}

					isSwinging = false;
				}
			}
		}

		public override void simulate(uint simulation, bool inputSteady)
		{
			if (isUsing && isUseable)
			{
				player.equipment.isBusy = false;
				isUsing = false;

				if (Provider.isServer)
				{
					player.equipment.useStepB();
				}
			}
		}
	}
}
