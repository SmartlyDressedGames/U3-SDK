////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class OxygenBubble
	{
		public Transform origin;
		public float sqrRadius;

		public OxygenBubble(Transform newOrigin, float newSqrRadius)
		{
			origin = newOrigin;
			sqrRadius = newSqrRadius;
		}
	}
}
