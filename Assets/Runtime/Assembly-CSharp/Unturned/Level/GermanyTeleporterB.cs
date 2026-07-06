////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using UnityEngine;

namespace SDG.Unturned
{
	public class GermanyTeleporterB : GermanyTeleporterA
	{
		public float sqrBossRadius;

		public int navIndex;
		private ZombieRegion region;

		private bool isListeningPlayer;
		private bool isListeningZombie;

		protected override IEnumerator teleport()
		{
			yield return new WaitForSeconds(1);

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
					if (player.quests.getQuestStatus(248) == ENPCQuestStatus.COMPLETED)
#pragma warning restore
					{
						player.teleportToLocationUnsafe(target.position, target.rotation.eulerAngles.y);
					}
				}
			}
		}

		private void onPlayerLifeUpdated(Player player)
		{
			if (player == null || player.life.IsAlive)
			{
				return;
			}

			if ((player.transform.position - transform.position).sqrMagnitude > sqrBossRadius)
			{
				return;
			}

#pragma warning disable
			if (player.quests.getQuestStatus(248) == ENPCQuestStatus.COMPLETED)
			{
				return;
			}
			// Did not both converting these because the hardcoded per-level scripts need upgrading anyway.
			player.quests.sendRemoveQuest(248);
#pragma warning restore
		}

		private void onZombieLifeUpdated(Zombie zombie)
		{
			if (!zombie.isDead)
			{
				return;
			}

			if ((zombie.transform.position - transform.position).sqrMagnitude > sqrBossRadius)
			{
				return;
			}

			nearbyPlayers.Clear();
			PlayerTool.getPlayersInRadius(transform.position, sqrBossRadius, nearbyPlayers);

			for (int index = 0; index < nearbyPlayers.Count; index++)
			{
				Player player = nearbyPlayers[index];

				if (player.life.isDead)
				{
					continue;
				}
#pragma warning disable
				// Did not both converting these because the hardcoded per-level scripts need upgrading anyway.
				player.quests.sendRemoveQuest(248);
#pragma warning restore
				player.quests.sendSetFlag(248, 1);
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();

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

			region = ZombieManager.regions[navIndex];

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

		protected override void OnDisable()
		{
			base.OnDisable();

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
