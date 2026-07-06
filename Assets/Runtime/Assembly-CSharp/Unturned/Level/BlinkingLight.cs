////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class BlinkingLight : MonoBehaviour
	{
		public GameObject target;

		private float blinkTime;

		private void Update()
		{
			if (Time.time - blinkTime < 1.0f)
			{
				return;
			}
			blinkTime = Time.time;

			target.SetActive(!target.activeSelf);
		}
	}
}