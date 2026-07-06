////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class ZombieBossQuest : MonoBehaviour
	{
		private static List<Player> nearbyPlayers = new List<Player>();

		public Transform target;
		public float sqrRadius;

		public ushort teleportEffect;

		private ZombieRegion region;

		private bool isListeningPlayer;
		private bool isListeningZombie;

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

#pragma warning disable
					// Did not both converting these because the hardcoded per-level scripts need upgrading anyway.
					player.quests.sendRemoveQuest(213);
#pragma warning restore
					player.quests.setFlag(213, 1);

					player.teleportToLocationUnsafe(target.position, target.rotation.eulerAngles.y);
				}
			}
		}

		private void onPlayerLifeUpdated(Player player)
		{
			if (player == null || player.life.IsAlive)
			{
				return;
			}

			if ((player.transform.position - transform.position).sqrMagnitude > sqrRadius)
			{
				return;
			}

#pragma warning disable
			// Did not both converting these because the hardcoded per-level scripts need upgrading anyway.
			player.quests.sendRemoveQuest(213);
#pragma warning restore
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
			EffectManager.sendEffect(teleportEffect, 16, zombie.transform.position + Vector3.up);
#pragma warning restore
			StartCoroutine("teleport");
		}

		private void OnEnable()
		{
			if (!Provider.isServer)
			{
				return;
			}

			if (!isListeningPlayer)
			{
				PlayerLife.onPlayerLifeUpdated += onPlayerLifeUpdated;
				isListeningPlayer = true;
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

			if (!isListeningZombie)
			{
				region.onZombieLifeUpdated += onZombieLifeUpdated;
				isListeningZombie = true;
			}
		}

		private void OnDisable()
		{
			if (isListeningPlayer)
			{
				PlayerLife.onPlayerLifeUpdated -= onPlayerLifeUpdated;
				isListeningPlayer = false;
			}

			if (isListeningZombie && region != null)
			{
				region.onZombieLifeUpdated -= onZombieLifeUpdated;
				isListeningZombie = false;
			}

			region = null;
		}
	}
}
