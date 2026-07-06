////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class TemperatureBubble
	{
		public Transform origin;
		public float sqrRadius;
		public EPlayerTemperature temperature;

		public TemperatureBubble(Transform newOrigin, float newSqrRadius, EPlayerTemperature newTemperature)
		{
			origin = newOrigin;
			sqrRadius = newSqrRadius;
			temperature = newTemperature;
		}
	}
}
