////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class OxygenManager : MonoBehaviour
	{
		private static List<OxygenBubble> bubbles;

		public static bool checkPointBreathable(Vector3 point)
		{
			for (int bubbleIndex = 0; bubbleIndex < bubbles.Count; bubbleIndex++)
			{
				OxygenBubble bubble = bubbles[bubbleIndex];

				if (bubble.origin == null)
				{
					continue;
				}

				if ((bubble.origin.position - point).sqrMagnitude < bubble.sqrRadius)
				{
					return true;
				}
			}

			return false;
		}

		public static OxygenBubble registerBubble(Transform origin, float radius)
		{
			OxygenBubble bubble = new OxygenBubble(origin, radius * radius);
			bubbles.Add(bubble);
			return bubble;
		}

		public static void deregisterBubble(OxygenBubble bubble)
		{
			bubbles.Remove(bubble);
		}

		private void onLevelLoaded(int level)
		{
			bubbles = new List<OxygenBubble>();
		}

		private void Start()
		{
			Level.onPrePreLevelLoaded += onLevelLoaded;
		}
	}
}