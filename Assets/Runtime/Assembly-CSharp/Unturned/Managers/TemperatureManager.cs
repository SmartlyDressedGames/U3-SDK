////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class TemperatureManager : MonoBehaviour
	{
		private static List<TemperatureBubble> bubbles;

		public static EPlayerTemperature checkPointTemperature(Vector3 point, bool proofFire)
		{
			EPlayerTemperature temperature = EPlayerTemperature.NONE;

			for (int bubbleIndex = 0; bubbleIndex < bubbles.Count; bubbleIndex++)
			{
				TemperatureBubble bubble = bubbles[bubbleIndex];

				if (bubble.origin == null)
				{
					continue;
				}

				if (proofFire && bubble.temperature == EPlayerTemperature.BURNING)
				{
					continue;
				}

				if ((bubble.origin.position - point).sqrMagnitude < bubble.sqrRadius)
				{
					if (bubble.temperature == EPlayerTemperature.ACID)
					{
						return bubble.temperature;
					}
					else if (bubble.temperature == EPlayerTemperature.BURNING)
					{
						temperature = bubble.temperature;
					}
					else
					{
						if (temperature != EPlayerTemperature.BURNING)
						{
							temperature = bubble.temperature;
						}
					}
				}
			}

			return temperature;
		}

		public static TemperatureBubble registerBubble(Transform origin, float radius, EPlayerTemperature temperature)
		{
			TemperatureBubble bubble = new TemperatureBubble(origin, radius * radius, temperature);
			bubbles.Add(bubble);
			return bubble;
		}

		public static void deregisterBubble(TemperatureBubble bubble)
		{
			bubbles.Remove(bubble);
		}

		private void onLevelLoaded(int level)
		{
			bubbles = new List<TemperatureBubble>();
		}

		private void Start()
		{
			Level.onPrePreLevelLoaded += onLevelLoaded;
		}
	}
}