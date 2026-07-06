////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableBeacon : MonoBehaviour, IManualOnDestroy
	{
		private ItemBeaconAsset asset;

		public void updateState(ItemBarricadeAsset asset)
		{
			this.asset = (ItemBeaconAsset) asset;
		}

		public bool IsChildOfVehicle => transform.parent != null && transform.parent.CompareTag("Vehicle");

		/// <summary>
		/// Number of players inside the navmesh when the beacon was placed.
		/// Clamped to 1 if ShouldScaleWithNumberOfParticipants is false.
		/// </summary>
		public int initialParticipants
		{
			get;
			private set;
		}

		private byte nav;
		private bool wasInit;
		private float started;

		public void init(int amount)
		{
			if (wasInit)
			{
				return;
			}

			if (amount >= asset.wave)
			{
				remaining = 0;
				alive = asset.wave;
			}
			else
			{
				remaining = asset.wave - amount;
				alive = amount;
			}

			wasInit = true;
		}

		private int remaining;

		public int getRemaining()
		{
			return remaining;
		}

		public void spawnRemaining()
		{
			if (remaining <= 0)
			{
				return;
			}

			remaining--;
			alive++;
		}

		private int alive;

		public int getAlive()
		{
			return alive;
		}

		public void despawnAlive()
		{
			if (alive <= 0)
			{
				return;
			}

			alive--;

			if (remaining == 0 && alive == 0)
			{
				BarricadeManager.damage(transform, 10000.0f, 1.0f, false, damageOrigin: EDamageOrigin.Horde_Beacon_Self_Destruct);
			}
		}

		private bool isRegistered;

		private void Update()
		{
			if (!Provider.isServer)
			{
				return;
			}

			if (Time.realtimeSinceStartup - started < 3)
			{
				return;
			}

			if (isRegistered)
			{
				for (int index = 0; index < Provider.clients.Count; index++)
				{
					SteamPlayer player = Provider.clients[index];

					if (player.player == null || player.player.movement == null || player.player.life == null || player.player.life.isDead)
					{
						continue;
					}

					if (player.player.movement.nav == nav)
					{
						return;
					}
				}
			}

			BarricadeManager.damage(transform, 10000.0f, 1.0f, false, damageOrigin: EDamageOrigin.Horde_Beacon_Self_Destruct);
		}

		private void Start()
		{
			started = Time.realtimeSinceStartup;

			Transform engine = transform.Find("Engine");
			if (engine != null)
			{
				engine.gameObject.SetActive(true);

				//if(engine.gameObject.activeSelf)
				//{
				//	AudioSource audio = engine.GetComponent<AudioSource>();
				//	if(audio != null && audio.clip != null)
				//	{
				//		audio.time = Random.Range(0.0f, audio.clip.length);
				//	}
				//}
			}

			if (!Provider.isServer)
			{
				return;
			}

			if (isRegistered)
			{
				return;
			}

			if (IsChildOfVehicle) // no system for moving beacon (yet?) depends if that's balanced
			{
				return;
			}

			if (!LevelNavigation.checkNavigation(transform.position)) // not on the nav graph
			{
				return;
			}

			LevelNavigation.tryGetNavigation(transform.position, out nav);

			if (asset.ShouldScaleWithNumberOfParticipants)
			{
				initialParticipants = BeaconManager.getParticipants(nav);
			}
			else
			{
				initialParticipants = 1;
			}

			BeaconManager.registerBeacon(nav, this);
			isRegistered = true;
		}

		public void ManualOnDestroy()
		{
			if (!Provider.isServer)
			{
				return;
			}

			if (!isRegistered)
			{
				return;
			}

			BeaconManager.deregisterBeacon(nav, this);
			isRegistered = false;

			if (!wasInit)
			{
				return;
			}

			if (remaining > 0 || alive > 0)
			{
				return;
			}

			for (int playerIndex = 0; playerIndex < Provider.clients.Count; playerIndex++)
			{
				if (Provider.clients[playerIndex].player != null && Provider.clients[playerIndex].player.life.IsAlive && Provider.clients[playerIndex].player.movement.nav == nav)
				{
					Provider.clients[playerIndex].player.quests.trackHordeKill();
				}
			}

			int rewardItemDropsCount = asset.rewards;
			int rewardParticipants = Mathf.Max(1, initialParticipants); // At least one player has always participated.

			// Allow capping the max number of participants that count toward rewards
			// e.g. it's a lot of loot in vanilla if 10 people work together
			uint maxParticipants = Provider.modeConfigData.Zombies.Beacon_Max_Participants;
			if (maxParticipants > 0)
			{
				rewardParticipants = Mathf.Min(initialParticipants, (int) maxParticipants);
			}

			float participantsMultiplier = Mathf.Sqrt(rewardParticipants); // 1->1, 2->1.5, 3->1.75
			rewardItemDropsCount = Mathf.CeilToInt(rewardItemDropsCount * participantsMultiplier);

			// Allow tuning the magnitude of items dropped
			// e.g. some hosts want 50% less horde items to drop
			rewardItemDropsCount = Mathf.CeilToInt(rewardItemDropsCount * Provider.modeConfigData.Zombies.Beacon_Rewards_Multiplier);

			// Allow maxing out a specific number of items
			// e.g. even with 24 players only allowed 50 items to drop
			uint maxRewards = Provider.modeConfigData.Zombies.Beacon_Max_Rewards;
			if (maxRewards > 0)
			{
				rewardItemDropsCount = Mathf.Min(rewardItemDropsCount, (int) maxRewards);
			}

			// Along with host configurable limit we have a hard limit of 256 items,
			// otherwise players could accidentally crash their game from mis-configuring
			// the drops multiplier option.
			rewardItemDropsCount = Mathf.Min(rewardItemDropsCount, 256);

			for (int drop = 0; drop < rewardItemDropsCount; drop++)
			{
				ushort id = SpawnTableTool.ResolveLegacyId(asset.rewardID, EAssetType.ITEM, OnGetRewardErrorContext);

				if (id != 0)
				{
					ItemManager.dropItem(new Item(id, EItemOrigin.NATURE), transform.position, false, true, true);
				}
			}
		}

		private string OnGetRewardErrorContext()
		{
			return $"Horde beacon reward {asset?.FriendlyName}";
		}

		[System.Obsolete("Renamed to IsChildOfVehicle")]
		public bool isPlant => IsChildOfVehicle;
	}
}
