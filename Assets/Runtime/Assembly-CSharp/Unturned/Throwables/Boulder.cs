////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class Boulder : MonoBehaviour
	{
		private static readonly float DAMAGE_PLAYER = 3;
		private static readonly float DAMAGE_BARRICADE = 15;
		private static readonly float DAMAGE_STRUCTURE = 15;
		private static readonly float DAMAGE_OBJECT = 25;
		private static readonly float DAMAGE_VEHICLE = 10;
		private static readonly float DAMAGE_RESOURCE = 25;

		private bool isExploded;
		private Vector3 lastPos;

		private void OnTriggerEnter(Collider other)
		{
			if (isExploded)
			{
				return;
			}

			if (other.isTrigger)
			{
				return;
			}

			if (other.transform.CompareTag("Agent"))
			{
				return;
			}

			isExploded = true;
			Vector3 direction = (transform.position - lastPos).normalized;

			if (Provider.isServer)
			{
				float speed = Mathf.Clamp(transform.parent.GetComponent<Rigidbody>().velocity.magnitude, 0, 20);

				if (speed < 3)
				{
					return;
				}

				if (other.transform.CompareTag("Player"))
				{
					Player player = DamageTool.getPlayer(other.transform);

					if (player != null)
					{
						EPlayerKill kill;
						DamageTool.damage(player, EDeathCause.BOULDER, ELimb.SPINE, CSteamID.Nil, direction, DAMAGE_PLAYER, speed, out kill);
					}
				}
				else if (other.transform.CompareTag("Vehicle"))
				{
					// 2023-06-15: was pointed out that if targeting is disabled then damage should probably be disabled. (public issue #3952)
					if (Provider.modeConfigData.Zombies.Can_Target_Vehicles)
					{
						InteractableVehicle vehicle = other.transform.GetComponent<InteractableVehicle>();
						if (vehicle != null && vehicle.asset != null && vehicle.asset.isVulnerableToEnvironment)
						{
							VehicleManager.damage(vehicle, DAMAGE_VEHICLE, speed, true, damageOrigin: EDamageOrigin.Mega_Zombie_Boulder);
						}
					}
				}
				else if (other.transform.CompareTag("Barricade"))
				{
					// 2023-06-15: was pointed out that if targeting is disabled then damage should probably be disabled. (public issue #3952)
					if (Provider.modeConfigData.Zombies.Can_Target_Barricades)
					{
						Transform hit = DamageTool.getBarricadeRootTransform(other.transform);
						if (hit != null)
						{
							BarricadeManager.damage(hit, DAMAGE_BARRICADE, speed, true, damageOrigin: EDamageOrigin.Mega_Zombie_Boulder);
						}
					}
				}
				else if (other.transform.CompareTag("Structure"))
				{
					// 2023-06-15: was pointed out that if targeting is disabled then damage should probably be disabled. (public issue #3952)
					if (Provider.modeConfigData.Zombies.Can_Target_Structures)
					{
						Transform hit = DamageTool.getStructureRootTransform(other.transform);
						if (hit != null)
						{
							StructureManager.damage(hit, direction, DAMAGE_STRUCTURE, speed, true, damageOrigin: EDamageOrigin.Mega_Zombie_Boulder);
						}
					}
				}
				else if (other.transform.CompareTag("Resource"))
				{
					Transform hit = DamageTool.getResourceRootTransform(other.transform);
					if (hit != null)
					{
						EPlayerKill kill;
						uint xp;
						ResourceManager.damage(hit, direction, DAMAGE_RESOURCE, speed, 1f, out kill, out xp, damageOrigin: EDamageOrigin.Mega_Zombie_Boulder);
					}
				}
				else
				{
					InteractableObjectRubble rubble = other.transform.GetComponentInParent<InteractableObjectRubble>();
					if (rubble != null)
					{
						EPlayerKill kill;
						uint xp;
						DamageTool.damage(rubble.transform, direction, rubble.getSection(other.transform), DAMAGE_OBJECT, speed, out kill, out xp, damageOrigin: EDamageOrigin.Mega_Zombie_Boulder);
					}
				}
			}

			if (!Dedicator.IsDedicatedServer)
			{
				EffectAsset metal_2 = Assets.find(Metal_2_Ref);
				if (metal_2 != null)
				{
					EffectManager.effect(metal_2, transform.position, -direction);
				}
			}
		}

		private void FixedUpdate()
		{
			lastPos = transform.position;
		}

		private void Awake()
		{
			lastPos = transform.position;
		}

		internal static AssetReference<EffectAsset> Metal_2_Ref = new AssetReference<EffectAsset>("b7d53965bc6545c28e029175af35de30"); // (52)
	}
}
