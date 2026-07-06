////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class ZombieSoulFlag : MonoBehaviour
	{
		private static List<Player> nearbyPlayers = new List<Player>();

		// placed = 1, swap to 2 when charged
		public ushort flagPlaced;
		public ushort flagKills;

		public float sqrRadius;
		public byte soulsNeeded;

		public ushort collectEffect;
		public ushort teleportEffect;

		private ZombieRegion region;

		private bool isListening;

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

			nearbyPlayers.Clear();
			PlayerTool.getPlayersInRadius(transform.position, sqrRadius, nearbyPlayers);

			for (int index = 0; index < nearbyPlayers.Count; index++)
			{
				Player player = nearbyPlayers[index];

				if (player.life.isDead)
				{
					continue;
				}

				short placed_artifact;
				if (player.quests.getFlag(flagPlaced, out placed_artifact) && placed_artifact == 1)
				{
#pragma warning disable
					// Did not both converting these because the hardcoded per-level scripts need upgrading anyway.
					EffectManager.sendEffect(collectEffect, player.channel.GetOwnerTransportConnection(), zombie.transform.position + Vector3.up, (transform.position - zombie.transform.position + Vector3.up).normalized);
#pragma warning restore

					short artifact_kills; // souls_collected
					player.quests.getFlag(flagKills, out artifact_kills);
					artifact_kills++;
					player.quests.sendSetFlag(flagKills, artifact_kills);

					if (artifact_kills >= soulsNeeded)
					{
#pragma warning disable
						// Did not both converting these because the hardcoded per-level scripts need upgrading anyway.
						EffectManager.sendEffect(teleportEffect, player.channel.GetOwnerTransportConnection(), transform.position);
#pragma warning restore

						player.quests.sendSetFlag(flagPlaced, 2);
					}
				}
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
