////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class GermanyTeleporterA : MonoBehaviour
	{
		protected static List<Player> nearbyPlayers = new List<Player>();

		public Transform target;
		public float sqrRadius;
		public string eventID;

		public ushort teleportEffect;

		private float lastTeleport;
		private bool isListening;

		protected virtual IEnumerator teleport()
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
					if (player.quests.getQuestStatus(248) != ENPCQuestStatus.COMPLETED)
					{
						// Did not both converting these because the hardcoded per-level scripts need upgrading anyway.
						player.quests.sendAddQuest(248);
#pragma warning restore
					}

					player.teleportToLocationUnsafe(target.position, target.rotation.eulerAngles.y);
				}
			}
		}

		protected virtual void handleEventTriggered(Player player, string id)
		{
			if (id != eventID)
			{
				return;
			}

			if (Time.realtimeSinceStartup - lastTeleport < 5)
			{
				return;
			}
			lastTeleport = Time.realtimeSinceStartup;

#pragma warning disable
			// Did not both converting these because the hardcoded per-level scripts need upgrading anyway.
			EffectManager.sendEffect(teleportEffect, 16, transform.position);
#pragma warning restore
			StartCoroutine("teleport");
		}

		protected virtual void OnEnable()
		{
			if (!Provider.isServer)
			{
				return;
			}

			if (!isListening)
			{
				NPCEventManager.onEvent += handleEventTriggered;
				isListening = true;
			}
		}

		protected virtual void OnDisable()
		{
			if (isListening)
			{
				NPCEventManager.onEvent -= handleEventTriggered;
				isListening = false;
			}
		}
	}
}
