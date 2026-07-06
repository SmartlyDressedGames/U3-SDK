////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableClock : InteractablePower
	{
		private Transform handHourTransform;
		private Transform handMinuteTransform;

		public override void updateState(Asset asset, byte[] state)
		{
			handHourTransform = transform.Find("Hand_Hour");
			handMinuteTransform = transform.Find("Hand_Minute");

			base.updateState(asset, state);

			RefreshIsConnectedToPowerWithoutNotify();
		}

		public override bool checkUseable()
		{
			return isWired;
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			text = "";
			color = Color.white;

			if (!isWired)
			{
				message = EPlayerMessage.POWER;
				return true;
			}
			else
			{
				message = EPlayerMessage.NONE;
				return false;
			}
		}

		private void Update()
		{
			if (!isWired)
			{
				return;
			}

			if (handHourTransform == null || handMinuteTransform == null)
			{
				return;
			}

			float time;
			if (LightingManager.day < LevelLighting.bias)
			{
				// 0 = dawn
				// 0.5 = noon
				// 1 = dusk
				time = LightingManager.day / LevelLighting.bias;
			}
			else
			{
				// 0 = dusk
				// 0.5 = midnight
				// 1 = dawn
				time = (LightingManager.day - LevelLighting.bias) / (1 - LevelLighting.bias);
			}

			float hour = time - 0.5f; // 1 full rotations per half day
			float minute = time * 12; // 12 full rotations per half day

			handHourTransform.localRotation = Quaternion.Euler(0, hour * -360, 0);
			handMinuteTransform.localRotation = Quaternion.Euler(0, minute * -360, 0);
		}
	}
}
