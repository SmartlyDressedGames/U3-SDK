////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class ZombieSoulTeleporter : MonoBehaviour
	{
		private static List<Player> nearbyPlayers = new List<Player>();

		public Transform target;
		public Transform targetBoss;
		public float sqrRadius;
		public byte soulsNeeded;

		public ushort collectEffect;
		public ushort teleportEffect;

		private ZombieRegion region;
		private byte soulsCollected;

		private bool isListening;

		private IEnumerator teleport()
		{
			yield return new WaitForSeconds(3.0f);

			if (target != null)
			{
				nearbyPlayers.Clear();
				PlayerTool.getPlayersInRadius(transform.position, sqrRadius, nearbyPlayers);

				for (int index = 0; index < nearbyPlayers.Count; index++)
				{
					Player player = nearbyPlayers[index];

					if (player.life.isDead)
					{
						continue;
					}

					short placed_treasure;
					short placed_skeleton;
#pragma warning disable
					// Did not both converting these because the hardcoded per-level scripts need upgrading anyway.
					if (player.quests.getFlag(211, out placed_treasure) && placed_treasure == 1 && player.quests.getFlag(212, out placed_skeleton) && placed_skeleton == 1 && player.quests.getQuestStatus(213) != ENPCQuestStatus.COMPLETED)
					{
						player.quests.sendSetFlag(214, 0);
						player.quests.sendAddQuest(213);
#pragma warning restore

						player.teleportToLocationUnsafe(targetBoss.position, targetBoss.rotation.eulerAngles.y);
					}
					else
					{
						player.teleportToLocationUnsafe(target.position, target.rotation.eulerAngles.y);

						if (player.equipment.HasValidUseable)
						{
							player.equipment.dequip();
						}
						player.equipment.canEquip = false;
					}
				}
			}
		}

		private void onZombieLifeUpdated(Zombie zombie)
		{
			if (!zombie.isDead)
			{
				return;
			}

			if ((zombie.transform.position - transform.position).sqrMagnitude > sqrRadius)
			{
				return;
			}

#pragma warning disable
			// Did not both converting these because the hardcoded per-level scripts need upgrading anyway.
			EffectManager.sendEffect(collectEffect, 16, zombie.transform.position + Vector3.up, (transform.position - zombie.transform.position + Vector3.up).normalized);
#pragma warning restore

			soulsCollected++;
			if (soulsCollected >= soulsNeeded)
			{
#pragma warning disable
				// Did not both converting these because the hardcoded per-level scripts need upgrading anyway.
				EffectManager.sendEffect(teleportEffect, 16, transform.position);
#pragma warning restore

				soulsCollected = 0;
				StartCoroutine("teleport");
			}
		}

		private void OnEnable()
		{
			if (!Provider.isServer)
			{
				return;
			}

			if (region != null)
			{
				return;
			}

			byte bound;
			if (LevelNavigation.tryGetBounds(transform.position, out bound))
			{
				region = ZombieManager.regions[bound];
			}

			if (region == null)
			{
				return;
			}

			if (!isListening)
			{
				region.onZombieLifeUpdated += onZombieLifeUpdated;
				isListening = true;
			}
		}

		private void OnDisable()
		{
			if (isListening && region != null)
			{
				region.onZombieLifeUpdated -= onZombieLifeUpdated;
				isListening = false;
			}

			region = null;
		}
	}
}
