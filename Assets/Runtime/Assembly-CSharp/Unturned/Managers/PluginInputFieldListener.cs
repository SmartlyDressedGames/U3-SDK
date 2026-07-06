////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned
{
	public class PluginInputFieldListener : MonoBehaviour
	{
		public InputField targetInputField;

		protected void Start()
		{
			if (targetInputField != null)
			{
				targetInputField.onEndEdit.AddListener(onEndEdit);
			}
		}

		private void onEndEdit(string text)
		{
			if (targetInputField != null)
			{
				EffectManager.sendEffectTextCommitted(targetInputField.name, text);
			}
		}
	}

	public class TMP_PluginInputFieldListener : MonoBehaviour
	{
		public TMP_InputField targetInputField;

		protected void Start()
		{
			if (targetInputField != null)
			{
				targetInputField.onEndEdit.AddListener(onEndEdit);
			}
		}

		private void onEndEdit(string text)
		{
			if (targetInputField != null)
			{
				EffectManager.sendEffectTextCommitted(targetInputField.name, text);
			}
		}
	}
}
