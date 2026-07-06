////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class SafezoneManager : MonoBehaviour
	{
		private static List<SafezoneBubble> bubbles;

		public static bool checkPointValid(Vector3 point)
		{
			for (int bubbleIndex = 0; bubbleIndex < bubbles.Count; bubbleIndex++)
			{
				SafezoneBubble bubble = bubbles[bubbleIndex];

				if ((bubble.origin - point).sqrMagnitude < bubble.sqrRadius)
				{
					return false;
				}
			}

			return true;
		}

		public static SafezoneBubble registerBubble(Vector3 origin, float radius)
		{
			SafezoneBubble bubble = new SafezoneBubble(origin, radius * radius);
			bubbles.Add(bubble);
			return bubble;
		}

		public static void deregisterBubble(SafezoneBubble bubble)
		{
			bubbles.Remove(bubble);
		}

		private void onLevelLoaded(int level)
		{
			bubbles = new List<SafezoneBubble>();
		}

		private void Start()
		{
			Level.onPrePreLevelLoaded += onLevelLoaded;
		}
	}
}