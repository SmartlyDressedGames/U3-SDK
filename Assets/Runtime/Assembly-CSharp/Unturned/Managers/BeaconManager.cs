////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void BeaconUpdated(byte nav, bool hasBeacon);

	public class BeaconManager : MonoBehaviour
	{
		private static List<InteractableBeacon>[] beacons;

		public static BeaconUpdated onBeaconUpdated;

		public static int getParticipants(byte nav)
		{
			int sum = 0;

			for (int index = 0; index < Provider.clients.Count; index++)
			{
				SteamPlayer player = Provider.clients[index];

				if (player.player == null || player.player.movement == null || player.player.life == null || player.player.life.isDead)
				{
					continue;
				}

				if (player.player.movement.nav == nav)
				{
					sum++;
				}
			}

			return sum;
		}

		public static InteractableBeacon checkBeacon(byte nav)
		{
			if (beacons[nav].Count > 0)
			{
				return beacons[nav][0];
			}
			else
			{
				return null;
			}
		}

		public static void registerBeacon(byte nav, InteractableBeacon beacon)
		{
			if (!LevelNavigation.checkSafe(nav))
			{
				return;
			}

			beacons[nav].Add(beacon);

			onBeaconUpdated?.Invoke(nav, beacons[nav].Count > 0);
		}

		public static void deregisterBeacon(byte nav, InteractableBeacon beacon)
		{
			if (!LevelNavigation.checkSafe(nav))
			{
				return;
			}

			beacons[nav].Remove(beacon);

			onBeaconUpdated?.Invoke(nav, beacons[nav].Count > 0);
		}

		private void onLevelLoaded(int level)
		{
			if (LevelNavigation.bounds == null)
			{
				return;
			}

			beacons = new List<InteractableBeacon>[LevelNavigation.bounds.Count];
			for (int index = 0; index < beacons.Length; index++)
			{
				beacons[index] = new List<InteractableBeacon>();
			}
		}

		private void Start()
		{
			Level.onPrePreLevelLoaded += onLevelLoaded;
		}
	}
}