////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class Bumper : MonoBehaviour
	{
		public bool reverse;
		public bool instakill;

		private static readonly float DAMAGE_VEHICLE = 8;

		private InteractableVehicle vehicle;
		private float lastDamageImpact;

		public void init(InteractableVehicle newVehicle)
		{
			vehicle = newVehicle;
		}

		/// <summary>
		/// Get SteamID of vehicle's driver, or nil if not driven.
		/// </summary>
		protected CSteamID getInstigatorSteamID()
		{
			if (vehicle && vehicle.isDriven)
			{
				return vehicle.passengers[0].player.playerID.steamID;
			}
			else
			{
				return CSteamID.Nil;
			}
		}

		/// <summary>
		/// Crashed into something, if applicable take self damage from collision.
		/// </summary>
		protected void takeCrashDamage(float damage, bool canRepair = true)
		{
			if (vehicle == null || vehicle.asset == null || vehicle.asset.isVulnerableToBumper == false)
				return;

			float times = vehicle.asset.BumperSelfDamageMultiplier;
			EPlayerKill kill;
			DamageTool.damage(vehicle, false, transform.position, false, damage, times, canRepair, out kill, getInstigatorSteamID(), EDamageOrigin.Vehicle_Collision_Self_Damage);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other == null)
				return;

			if (!Provider.isServer)
				return;

			if (vehicle == null || vehicle.asset == null)
				return;

			if (other.isTrigger)
				return;

			if (other.transform.IsChildOf(vehicle.transform))
			{
				// Ignore anything attached to our vehicle.
				return;
			}

			InteractableVehicle otherVehicle = DamageTool.getVehicle(other.transform);
			if (otherVehicle != null)
			{
				// Ignore anything attached to a different vehicle. getVehicle handles the base
				// vehicle and related vehicles like train cars.
				return;
			}

			if (other.CompareTag("Debris"))
				return;

			float speed = Mathf.Clamp(vehicle.ReplicatedForwardVelocity * vehicle.asset.bumperMultiplier, -10, 10);

			if (reverse)
			{
				speed = -speed;
			}

			if (speed < vehicle.asset.BumperSpeedDamageThreshold)
				return;

			Player driver = vehicle.GetDriverPlayer();
			EPlayerKill kill = EPlayerKill.NONE;
			ERagdollEffect ragdollEffect = ERagdollEffect.None;
			if (vehicle.isSkinned)
			{
				ragdollEffect = driver.movement.GetVehicleRagdollEffect();
			}

			if (other.transform.CompareTag("Player"))
			{
				if (driver != null)
				{
					Player player = DamageTool.getPlayer(other.transform);

					if (driver != null && player != null && player.movement.getVehicle() == null && DamageTool.isPlayerAllowedToDamagePlayer(driver, player))
					{
						DamageTool.damage(player, EDeathCause.ROADKILL, ELimb.SPINE, vehicle.passengers[0].player.playerID.steamID, transform.forward, instakill ? 101 : vehicle.asset.BumperPlayerDamage, speed, out kill, trackKill: true, ragdollEffect: ragdollEffect);

						DamageTool.ServerSpawnLegacyImpact(other.transform.position + other.transform.up,
							-transform.forward,
							"Flesh",
							null,
							Provider.GatherClientConnectionsWithinSphere(other.transform.position, EffectManager.SMALL));

						takeCrashDamage(2);
					}
				}
			}
			else if (other.transform.CompareTag("Agent"))
			{
				Zombie zombie = DamageTool.getZombie(other.transform);

				if (zombie != null)
				{
					DamageZombieParameters parameters = new DamageZombieParameters(zombie, transform.forward, instakill ? 65000 : vehicle.asset.BumperZombieDamage);
					parameters.times = speed;
					parameters.instigator = this;
					parameters.ragdollEffect = ragdollEffect;

					uint xp;
					DamageTool.damageZombie(parameters, out kill, out xp);

					DamageTool.ServerSpawnLegacyImpact(other.transform.position + other.transform.up,
						-transform.forward,
						zombie.isRadioactive ? "Alien" : "Flesh",
						null,
						Provider.GatherClientConnectionsWithinSphere(other.transform.position, EffectManager.SMALL));

					takeCrashDamage(2);
				}
				else
				{
					Animal animal = DamageTool.getAnimal(other.transform);

					if (animal != null)
					{
						DamageAnimalParameters parameters = new DamageAnimalParameters(animal, transform.forward, instakill ? 65000 : vehicle.asset.BumperAnimalDamage);
						parameters.times = speed;
						parameters.instigator = this;
						parameters.ragdollEffect = ragdollEffect;

						uint xp;
						DamageTool.damageAnimal(parameters, out kill, out xp);

						DamageTool.ServerSpawnLegacyImpact(other.transform.position + other.transform.up,
							-transform.forward,
							"Flesh",
							null,
							Provider.GatherClientConnectionsWithinSphere(other.transform.position, EffectManager.SMALL));

						takeCrashDamage(2);
					}
				}
			}
			else
			{
				bool shouldSpawnImpact = false;

				if (other.transform.CompareTag("Barricade"))
				{
					if (instakill)
					{
						Transform root = DamageTool.getBarricadeRootTransform(other.transform);
						if (root.parent == null || !root.parent.CompareTag("Vehicle"))
						{
							shouldSpawnImpact = true;

							BarricadeManager.damage(root, 65000, speed, false, instigatorSteamID: getInstigatorSteamID(), damageOrigin: EDamageOrigin.Vehicle_Bumper);

							takeCrashDamage(DAMAGE_VEHICLE * speed);
						}
					}
				}
				else if (other.transform.CompareTag("Structure"))
				{
					if (instakill)
					{
						Transform root = DamageTool.getStructureRootTransform(other.transform);
						StructureManager.damage(root, transform.forward, 65000, speed, false, instigatorSteamID: getInstigatorSteamID(), damageOrigin: EDamageOrigin.Vehicle_Bumper);

						shouldSpawnImpact = true;

						takeCrashDamage(DAMAGE_VEHICLE * speed);
					}
				}
				else if (other.transform.CompareTag("Resource"))
				{
					Transform root = DamageTool.getResourceRootTransform(other.transform);
					shouldSpawnImpact = true;

					uint xp;
					ResourceManager.damage(root, transform.forward, instakill ? 65000 : vehicle.asset.BumperResourceDamage, speed, 1f, out kill, out xp, instigatorSteamID: getInstigatorSteamID(), damageOrigin: EDamageOrigin.Vehicle_Bumper);

					takeCrashDamage(DAMAGE_VEHICLE * speed);
				}
				else
				{
					InteractableObjectRubble rubble = other.transform.GetComponentInParent<InteractableObjectRubble>();
					if (rubble != null)
					{
						uint xp;
						DamageTool.damage(rubble.transform, transform.forward, rubble.getSection(other.transform), instakill ? 65000 : vehicle.asset.BumperObjectDamage, speed, out kill, out xp, instigatorSteamID: getInstigatorSteamID(), damageOrigin: EDamageOrigin.Vehicle_Bumper);

						if (Time.realtimeSinceStartup - lastDamageImpact > 0.2f)
						{
							lastDamageImpact = Time.realtimeSinceStartup;

							shouldSpawnImpact = true;

							takeCrashDamage(DAMAGE_VEHICLE * speed);
						}
					}
					else
					{

						if (Time.realtimeSinceStartup - lastDamageImpact > 0.2f)
						{
							ObjectAsset asset = LevelObjects.getAsset(other.transform);

							if (asset != null && !asset.isSoft)
							{
								lastDamageImpact = Time.realtimeSinceStartup;

								shouldSpawnImpact = true;

								takeCrashDamage(DAMAGE_VEHICLE * speed);
							}
						}
					}
				}

				if (shouldSpawnImpact)
				{
					Vector3 impactPosition = transform.position;
					BoxCollider box = transform.GetComponent<BoxCollider>();
					if (box != null)
					{
						impactPosition += transform.forward * box.size.z * 0.5f;
					}

					string materialName = PhysicsTool.GetMaterialName(impactPosition, other.transform, other);
					if (!string.IsNullOrEmpty(materialName))
					{
						// Melee impact is equivelant and placeholder for now.
						// Pass null for collider transform to disable bullet hole.
						DamageTool.ServerSpawnLegacyImpact(impactPosition,
							-transform.forward,
							materialName,
							null,
							Provider.GatherClientConnectionsWithinSphere(impactPosition, EffectManager.SMALL));
					}
				}

				if (!vehicle.isDead && vehicle.asset.isVulnerableToBumper)
				{
					if (!other.transform.CompareTag("Border"))
					{
						if ((vehicle.asset.engine == EEngine.PLANE && vehicle.ReplicatedSpeed > 20) || (vehicle.asset.engine == EEngine.HELICOPTER && vehicle.ReplicatedSpeed > 10))
						{
							takeCrashDamage(20000, canRepair: false);
						}
					}
				}
			}

			if (kill != EPlayerKill.NONE && driver != null)
			{
				driver.sendStat(kill);
			}
		}
	}
}
