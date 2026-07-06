////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class TemperatureTrigger : MonoBehaviour
	{
		public EPlayerTemperature temperature;
		private TemperatureBubble bubble;

		private void OnEnable()
		{
			if (bubble != null)
			{
				return;
			}

			bubble = TemperatureManager.registerBubble(transform, transform.localScale.x, temperature);
		}

		private void OnDisable()
		{
			if (bubble == null)
			{
				return;
			}

			TemperatureManager.deregisterBubble(bubble);
			bubble = null;
		}
	}
}