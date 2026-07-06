////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SafezoneBubble
	{
		public Vector3 origin;
		public float sqrRadius;

		public SafezoneBubble(Vector3 newOrigin, float newSqrRadius)
		{
			origin = newOrigin;
			sqrRadius = newSqrRadius;
		}
	}
}
