////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class ClaimManager : MonoBehaviour
	{
		private static List<ClaimBubble> bubbles;
		private static List<ClaimPlant> plants;

		// isClaim is true if it's a new claim, so we shouldn't be able to overlap existing claims
		public static bool checkCanBuild(Vector3 point, CSteamID owner, CSteamID group, bool isClaim)
		{
			for (int bubbleIndex = 0; bubbleIndex < bubbles.Count; bubbleIndex++)
			{
				ClaimBubble bubble = bubbles[bubbleIndex];

				if (isClaim ? (bubble.origin - point).sqrMagnitude < 4.0f * bubble.sqrRadius : (bubble.origin - point).sqrMagnitude < bubble.sqrRadius)
				{
					if (Dedicator.IsDedicatedServer ? !OwnershipTool.checkToggle(owner, bubble.owner, group, bubble.group) : !bubble.hasOwnership)
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <param name="isClaim">True if it's a new claim flag.</param>
		public static bool canBuildOnVehicle(Transform vehicle, CSteamID owner, CSteamID group)
		{
			foreach (ClaimPlant plant in plants)
			{
				if (plant.parent != vehicle)
					continue;

				if (Dedicator.IsDedicatedServer)
				{
					if (!OwnershipTool.checkToggle(owner, plant.owner, group, plant.group))
					{
						return false;
					}
				}
				else
				{
					if (!plant.hasOwnership)
					{
						return false;
					}
				}
			}

			return true;
		}

		public static ClaimBubble registerBubble(Vector3 origin, float radius, ulong owner, ulong group)
		{
			ClaimBubble bubble = new ClaimBubble(origin, radius * radius, owner, group);
			bubbles.Add(bubble);
			return bubble;
		}

		public static void deregisterBubble(ClaimBubble bubble)
		{
			bubbles.Remove(bubble);
		}

		public static ClaimPlant registerPlant(Transform parent, ulong owner, ulong group)
		{
			ClaimPlant plant = new ClaimPlant(parent, owner, group);
			plants.Add(plant);
			return plant;
		}

		public static void deregisterPlant(ClaimPlant plant)
		{
			plants.Remove(plant);
		}

		private void onLevelLoaded(int level)
		{
			bubbles = new List<ClaimBubble>();
			plants = new List<ClaimPlant>();
		}

		private void Start()
		{
			Level.onPrePreLevelLoaded += onLevelLoaded;
		}
	}
}
