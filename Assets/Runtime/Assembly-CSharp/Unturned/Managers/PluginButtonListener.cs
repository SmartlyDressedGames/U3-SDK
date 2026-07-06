////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned
{
	public class PluginButtonListener : MonoBehaviour
	{
		public Button targetButton;

		protected void Start()
		{
			if (targetButton != null)
			{
				targetButton.onClick.AddListener(onTargetButtonClicked);
			}
		}

		private void onTargetButtonClicked()
		{
			if (targetButton != null)
			{
				EffectManager.sendEffectClicked(targetButton.name);
			}
		}
	}
}
