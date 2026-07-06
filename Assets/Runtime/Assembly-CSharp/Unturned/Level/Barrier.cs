////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class Barrier : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			if (Provider.isServer == false)
				return;

			if (other.transform.CompareTag("Player"))
			{
				Player player = DamageTool.getPlayer(other.transform);

				if (player != null)
				{
					EPlayerKill kill;
					player.life.askDamage(101, Vector3.up * 10, EDeathCause.SUICIDE, ELimb.SKULL, CSteamID.Nil, out kill);
				}
			}
			else if (other.CompareTag("Agent"))
			{
				Zombie zombie = DamageTool.getZombie(other.transform);
				if (zombie != null)
				{
					DamageZombieParameters parameters = DamageZombieParameters.makeInstakill(zombie);
					parameters.instigator = this;

					EPlayerKill kill;
					uint xp;
					DamageTool.damageZombie(parameters, out kill, out xp);
				}
				else
				{
					Animal animal = DamageTool.getAnimal(other.transform);
					if (animal != null)
					{
						DamageAnimalParameters parameters = DamageAnimalParameters.makeInstakill(animal);
						parameters.instigator = this;

						EPlayerKill kill;
						uint xp;
						DamageTool.damageAnimal(parameters, out kill, out xp);
					}
				}
			}
		}
	}
}
