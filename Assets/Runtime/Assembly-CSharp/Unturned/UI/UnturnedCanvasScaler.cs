////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned
{
	[RequireComponent(typeof(CanvasScaler))]
	public class UnturnedCanvasScaler : MonoBehaviour
	{
		public CanvasScaler scaler;

		private void Start()
		{
			if (scaler == null)
			{
				scaler = GetComponent<CanvasScaler>();
				Update();
			}
		}

		private void Update()
		{
			if (scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize)
			{
#if GAME
				scaler.scaleFactor = GraphicsSettings.userInterfaceScale;
#endif
			}
		}
	}
}
